# Run from a separate PowerShell window after exiting Codex. Never dot-source to migrate.
[CmdletBinding()]
param([switch]$CheckOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-CodexStopped {
    $busy = @(Get-CimInstance Win32_Process | Where-Object {
        $_.Name -match '^(ChatGPT|Codex|codex)(\.exe)?$' -or
        ($_.ExecutablePath -and $_.ExecutablePath -match '\\OpenAI\.Codex_[^\\]*\\|\\OpenAI\\Codex\\|\\\.codex\\') -or
        ($_.ProcessId -ne $PID -and $_.CommandLine -and $_.CommandLine -match '\\\.codex\\')
    })
    if ($busy.Count) { throw "Codex-related processes are running ($($busy.Count)). Exit Codex and its helpers, then retry. No process will be killed." }
}

function Get-TreeInventory([string]$Root) {
    $rootItem = Get-Item -LiteralPath $Root -Force
    if (!$rootItem.PSIsContainer -or ($rootItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw 'Source/destination root must be an ordinary directory.'
    }
    $prefix = $rootItem.FullName.TrimEnd('\') + '\'
    $pending = [Collections.Generic.Stack[string]]::new()
    $pending.Push($rootItem.FullName)
    while ($pending.Count) {
        foreach ($item in Get-ChildItem -LiteralPath $pending.Pop() -Force -ErrorAction Stop) {
            if ($item.Attributes -band [IO.FileAttributes]::Encrypted) { throw 'EFS-encrypted items require a separate migration review.' }
            $relative = $item.FullName.Substring($prefix.Length)
            $kind = if ($item.PSIsContainer) { 'Directory' } else { 'File' }
            $linkRelative = $null
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                if ($item.LinkType -ne 'Junction') { throw "Unsupported reparse point: $relative" }
                $target = [IO.Path]::GetFullPath([string]$item.Target)
                if (!$target.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
                    throw "External junction requires manual review: $relative"
                }
                $kind = 'Junction'
                $linkRelative = $target.Substring($prefix.Length)
            } elseif ($item.PSIsContainer) { $pending.Push($item.FullName) }
            [pscustomobject]@{ Relative = $relative; Kind = $kind; LinkRelative = $linkRelative }
        }
    }
}

function Set-PreservedAcl([string]$From, [string]$To) {
    $acl = Get-Acl -LiteralPath $From
    $accessOnly = if ((Get-Item -LiteralPath $From -Force).PSIsContainer) {
        [Security.AccessControl.DirectorySecurity]::new()
    } else { [Security.AccessControl.FileSecurity]::new() }
    $accessSection = [Security.AccessControl.AccessControlSections]::Access
    $accessOnly.SetSecurityDescriptorSddlForm($acl.GetSecurityDescriptorSddlForm($accessSection), $accessSection)
    $acl = $accessOnly
    # Materialize inherited source rules; do not inherit broader permissions from D:.
    $acl.SetAccessRuleProtection($true, $true)
    Set-Acl -LiteralPath $To -AclObject $acl
    $actual = Get-Acl -LiteralPath $To
    $expectedRules = @($acl.Access | ForEach-Object { "$($_.IdentityReference)|$($_.AccessControlType)|$($_.FileSystemRights)|$($_.InheritanceFlags)|$($_.PropagationFlags)" } | Sort-Object)
    $actualRules = @($actual.Access | ForEach-Object { "$($_.IdentityReference)|$($_.AccessControlType)|$($_.FileSystemRights)|$($_.InheritanceFlags)|$($_.PropagationFlags)" } | Sort-Object)
    if (!$actual.AreAccessRulesProtected -or ($expectedRules -join "`n") -cne ($actualRules -join "`n")) {
        throw 'Destination ACL verification failed.'
    }
}

function Copy-VerifiedTree([string]$From, [string]$To) {
    if (Test-Path -LiteralPath $To) { throw 'Destination already exists; manual review is required. Nothing will be overwritten.' }
    $items = @(Get-TreeInventory $From)
    New-Item -ItemType Directory -Path $To | Out-Null
    Set-PreservedAcl $From $To
    foreach ($item in @($items | Where-Object Kind -eq 'Directory' | Sort-Object { $_.Relative.Length })) {
        $dest = Join-Path $To $item.Relative
        New-Item -ItemType Directory -Path $dest | Out-Null
        Set-PreservedAcl (Join-Path $From $item.Relative) $dest
    }
    foreach ($item in @($items | Where-Object Kind -eq 'File')) {
        $src = Join-Path $From $item.Relative
        $dest = Join-Path $To $item.Relative
        # Create an empty protected file before copying any content, including alternate streams.
        [IO.File]::Create($dest).Dispose()
        Set-PreservedAcl $src $dest
        foreach ($stream in @(Get-Item -LiteralPath $src -Stream *)) {
            $suffix = if ($stream.Stream -eq ':$DATA') { '' } else { ':' + $stream.Stream }
            $inputStream = [IO.File]::OpenRead($src + $suffix)
            try {
                $outputStream = [IO.File]::Create($dest + $suffix)
                try { $inputStream.CopyTo($outputStream) } finally { $outputStream.Dispose() }
            } finally { $inputStream.Dispose() }
        }
        [IO.File]::SetLastWriteTimeUtc($dest, [IO.File]::GetLastWriteTimeUtc($src))
        [IO.File]::SetCreationTimeUtc($dest, [IO.File]::GetCreationTimeUtc($src))
        $basicAttributes = [IO.File]::GetAttributes($src) -band ([IO.FileAttributes]::ReadOnly -bor [IO.FileAttributes]::Hidden -bor [IO.FileAttributes]::System -bor [IO.FileAttributes]::Archive)
        [IO.File]::SetAttributes($dest, $basicAttributes)
    }
    foreach ($item in @($items | Where-Object Kind -eq 'Junction')) {
        $targetItem = @($items | Where-Object Relative -eq $item.LinkRelative)
        if ($targetItem.Count -ne 1 -or $targetItem[0].Kind -ne 'Directory') {
            throw 'Junction target must be a real internal directory; link chains/cycles are unsupported.'
        }
        New-Item -ItemType Junction -Path (Join-Path $To $item.Relative) -Target (Join-Path $To $item.LinkRelative) | Out-Null
    }
    Assert-IdenticalTree $From $To
}

function Assert-IdenticalTree([string]$From, [string]$To) {
    $left = @(Get-TreeInventory $From | Sort-Object Relative)
    $right = @(Get-TreeInventory $To | Sort-Object Relative)
    if ($left.Count -ne $right.Count) { throw 'Inventory count mismatch.' }
    for ($i = 0; $i -lt $left.Count; $i++) {
        $a = $left[$i]; $b = $right[$i]
        if ($a.Relative -cne $b.Relative -or $a.Kind -ne $b.Kind -or $a.LinkRelative -cne $b.LinkRelative) {
            throw 'Inventory/link mismatch.'
        }
        if ($a.Kind -eq 'File') {
            $src = Join-Path $From $a.Relative; $dst = Join-Path $To $a.Relative
            $streams = @(Get-Item -LiteralPath $src -Stream * | Sort-Object Stream)
            $destStreams = @(Get-Item -LiteralPath $dst -Stream * | Sort-Object Stream)
            if ($streams.Count -ne $destStreams.Count) { throw 'Stream count mismatch.' }
            for ($j = 0; $j -lt $streams.Count; $j++) {
                if ($streams[$j].Stream -cne $destStreams[$j].Stream) { throw 'Stream name mismatch.' }
                $suffix = if ($streams[$j].Stream -eq ':$DATA') { '' } else { ':' + $streams[$j].Stream }
                if ((Get-FileHash -LiteralPath ($src + $suffix) -Algorithm SHA256).Hash -ne
                    (Get-FileHash -LiteralPath ($dst + $suffix) -Algorithm SHA256).Hash) {
                    throw 'SHA256 mismatch. Source is untouched.'
                }
            }
        }
    }
    Write-Host "Verified $($left.Count) entries, all file streams, and internal junction targets."
}

# Dot-sourcing exposes helpers exclusively for the disposable-fixture test.
if ($MyInvocation.InvocationName -eq '.') { return }
$source = Join-Path $env:USERPROFILE '.codex'
$destination = 'D:\CodexData'
Assert-CodexStopped
if (Test-Path -LiteralPath $destination) { throw 'D:\CodexData already exists; review it manually before proceeding.' }
if ((Get-Volume -DriveLetter D).FileSystem -ne 'NTFS') { throw 'Destination D: must use NTFS for ACLs and junctions.' }
$inventory = @(Get-TreeInventory $source)
Write-Host "Preflight inventory: $($inventory.Count) entries. Source remains unchanged."
if ($CheckOnly) { Write-Host 'CheckOnly: no copy, configuration change, or directory switch performed.'; return }
Copy-VerifiedTree $source $destination
Assert-CodexStopped
# Recheck immediately before the rename. Do not launch Codex during migration.
Assert-IdenticalTree $source $destination
$backup = $source + '.backup-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8)
Rename-Item -LiteralPath $source -NewName ([IO.Path]::GetFileName($backup))
try {
    New-Item -ItemType Junction -Path $source -Target $destination | Out-Null
    $link = Get-Item -LiteralPath $source -Force
    if ($link.LinkType -ne 'Junction' -or [string]$link.Target -ne $destination) { throw 'Final junction validation failed.' }
} catch {
    # Never delete automatically. If junction creation failed before creating an entry, restore the original name.
    if (!(Test-Path -LiteralPath $source)) { Rename-Item -LiteralPath $backup -NewName '.codex' }
    throw
}
Write-Host "Migration complete. Data: $destination. Original backup: $backup"
Write-Host 'Keep the backup until restart validation passes. No C: space is reclaimed yet.'
Write-Host 'Reopen D:\projects\神秘复苏 in Codex UI. Existing project records and CODEX_HOME/config were not modified.'

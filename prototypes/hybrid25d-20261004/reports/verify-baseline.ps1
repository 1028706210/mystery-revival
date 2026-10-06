$ErrorActionPreference = 'Stop'
$root = 'C:\Users\李帅朋\Documents\ChatGPT\神秘复苏\prototypes\hybrid25d-20261004'
$source = 'D:\projects\神秘复苏'
$validation = Join-Path $root 'Validation'
$reports = Join-Path $root 'reports'
$baselinePath = Join-Path $reports 'baseline.json'
$evidencePath = Join-Path $reports 'baseline-verification-final.json'
if (Test-Path -LiteralPath $evidencePath) { throw 'Verification already exists; preserve it.' }
$baselineRaw = Get-Content -LiteralPath $baselinePath -Raw
$baseline = $baselineRaw | ConvertFrom-Json
$before = @($baseline.sourceInventory)
$checks = [ordered]@{}
$actualSourceFiles = @()
foreach ($relative in $baseline.sourceRoots) {
    $items = @(Get-ChildItem -LiteralPath (Join-Path $source $relative) -Recurse -Force)
    if (@($items | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }).Count -gt 0) { throw 'Unexpected source reparse point.' }
    $actualSourceFiles += @($items | Where-Object { -not $_.PSIsContainer } | ForEach-Object { $_.FullName.Substring($source.Length + 1).Replace('\','/') })
}
$checks.sourceFileSetUnchanged = (@(Compare-Object -ReferenceObject @($before.path) -DifferenceObject $actualSourceFiles).Count -eq 0)
$sourceMismatches = @()
$copyDifferences = @()
foreach ($entry in $before) {
    $sourcePath = Join-Path $source $entry.path
    $copyPath = Join-Path $validation $entry.path
    $sourceSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $sourcePath).Hash
    $copySha = (Get-FileHash -Algorithm SHA256 -LiteralPath $copyPath).Hash
    if ($sourceSha -ne $entry.sha256) { $sourceMismatches += $entry.path }
    if ($copySha -ne $entry.sha256) { $copyDifferences += [pscustomobject]@{ path=$entry.path; originalSha256=$entry.sha256; copySha256=$copySha } }
}
$checks.sourceHashesUnchanged = ($sourceMismatches.Count -eq 0)
$actualCopyPaths = @(Get-ChildItem -LiteralPath $validation -File -Recurse -Force | ForEach-Object { $_.FullName.Substring($validation.Length + 1).Replace('\','/') })
$checks.copyFileSetMatches = (@(Compare-Object -ReferenceObject @($before.path) -DifferenceObject $actualCopyPaths).Count -eq 0)
$expectedDelta = @('Game/Packages/manifest.json','Game/Packages/packages-lock.json')
$checks.onlyTwoApprovedConfigFilesDiffer = ($copyDifferences.Count -eq 2 -and @(Compare-Object -ReferenceObject $expectedDelta -DifferenceObject @($copyDifferences.path)).Count -eq 0)

foreach ($name in @('manifest.json','packages-lock.json')) {
    $original = Get-Content -LiteralPath (Join-Path $source "Game\Packages\$name") -Raw | ConvertFrom-Json
    $copy = Get-Content -LiteralPath (Join-Path $validation "Game\Packages\$name") -Raw | ConvertFrom-Json
    $originalNames = @($original.dependencies.PSObject.Properties.Name)
    $copyNames = @($copy.dependencies.PSObject.Properties.Name)
    $checks["${name}_onlyPhysicsAdded"] = (@(Compare-Object -ReferenceObject $originalNames -DifferenceObject @($copyNames | Where-Object { $_ -ne 'com.unity.modules.physics' })).Count -eq 0 -and $copyNames -contains 'com.unity.modules.physics')
    foreach ($dependency in $originalNames) {
        $oldValue = $original.dependencies.$dependency | ConvertTo-Json -Compress -Depth 20
        $newValue = $copy.dependencies.$dependency | ConvertTo-Json -Compress -Depth 20
        $checks["${name}_preserves_$dependency"] = ($oldValue -eq $newValue)
    }
    if ($name -eq 'manifest.json') {
        $checks.physicsManifestVersion = ($copy.dependencies.'com.unity.modules.physics' -eq '1.0.0')
    } else {
        $physics = $copy.dependencies.'com.unity.modules.physics'
        $checks.physicsLockEntry = ($physics.version -eq '1.0.0' -and $physics.depth -eq 0 -and $physics.source -eq 'builtin' -and @($physics.dependencies.PSObject.Properties).Count -eq 0)
    }
}
$checks.versionPreserved = ((Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $source 'Game\ProjectSettings\ProjectVersion.txt')).Hash -eq (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $validation 'Game\ProjectSettings\ProjectVersion.txt')).Hash)
$checks.graphicsPreserved = ((Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $source 'Game\ProjectSettings\GraphicsSettings.asset')).Hash -eq (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $validation 'Game\ProjectSettings\GraphicsSettings.asset')).Hash)
$forbidden = @(Get-ChildItem -LiteralPath (Join-Path $validation 'Game') -Force | Where-Object { $_.Name -match '^(Library(?:\..*)?|Temp|Logs|UserSettings)$' })
$checks.noRuntimeCacheCopied = ($forbidden.Count -eq 0)
$checks.noInternalAuditCopied = (@(Get-ChildItem -LiteralPath $validation -Directory -Recurse -Force | Where-Object { $_.Name -eq 'internal' -or $_.Name -eq 'audit-evidence' }).Count -eq 0)
$localPhysicsPath = 'D:\Unity\Editors\6000.5.10f1\Editor\Data\Resources\PackageManager\BuiltInPackages\com.unity.modules.physics\package.json'
$localPhysics = Get-Content -LiteralPath $localPhysicsPath -Raw | ConvertFrom-Json
$checks.lockedEditorContainsBuiltinPhysics100 = ($localPhysics.name -eq 'com.unity.modules.physics' -and $localPhysics.version -eq '1.0.0' -and $localPhysics.type -eq 'module')
$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
$ended = [DateTimeOffset]::Now
$startIso = [regex]::Match($baselineRaw, '"startedUtc":\s*"([^"]+)"').Groups[1].Value
$snapshotStart = [DateTimeOffset]::Parse($startIso)
$effectiveMinutes = [Math]::Ceiling(($ended - $snapshotStart).TotalMinutes) + 5
$evidence = [ordered]@{
    task='H25D-00'
    verifiedUtc=$ended.UtcDateTime.ToString('o')
    verifiedClientLocal=[TimeZoneInfo]::ConvertTime($ended, [TimeZoneInfo]::FindSystemTimeZoneById('Pacific Standard Time')).ToString('o')
    result= if ($failed.Count -eq 0) { 'PASS' } else { 'FAIL' }
    checks=$checks
    checkCount=$checks.Count
    failedChecks=@($failed | ForEach-Object { $_.Key })
    sourceFileCount=$before.Count
    unchangedCopyFileCount=$before.Count - $copyDifferences.Count
    copyDifferences=$copyDifferences
    sourceMismatches=$sourceMismatches
    localPhysicsPackage=@{path=$localPhysicsPath;sha256=(Get-FileHash -Algorithm SHA256 -LiteralPath $localPhysicsPath).Hash}
    unityStarted=$false
    unityRelatedProcesses=@(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^Unity|^bee|AssetImportWorker' } | Select-Object Id,ProcessName,Path)
}
$evidence | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $evidencePath -Encoding utf8
$baseline.configDeltaVerified = ($failed.Count -eq 0)
$baseline.effectiveMinutes = $effectiveMinutes
$baseline.remainingBatchEffectiveMinutes = [Math]::Max(0,30-$effectiveMinutes)
$baseline | Add-Member -NotePropertyName finalVerification -NotePropertyValue $evidence -Force
$baseline | Add-Member -NotePropertyName timingMethod -NotePropertyValue 'ISO offset parsed with DateTimeOffset; snapshot and verification timestamps measured; 5 minutes preflight/report allowance included, effective time is an estimate rounded up. Waiting=0. Initial DateTime coercion accounting was invalid and corrected before delivery; initial verification checks retained.' -Force
$baseline | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $baselinePath -Encoding utf8
"BASELINE_VERIFY=$($evidence.result) checks=$($checks.Count) failed=$($failed.Count) source=$($before.Count) unchangedCopies=$($evidence.unchangedCopyFileCount) allowedConfigDeltas=$($copyDifferences.Count)"
"EFFECTIVE_MINUTES_ESTIMATE=$effectiveMinutes REMAINING_MINUTES=$($baseline.remainingBatchEffectiveMinutes) CLIENT_END=$($evidence.verifiedClientLocal)"
if ($failed.Count -gt 0) { $failed | ForEach-Object { "FAILED=$($_.Key)" }; exit 1 }

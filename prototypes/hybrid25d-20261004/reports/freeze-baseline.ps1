$ErrorActionPreference = 'Stop'
$source = 'D:\projects\神秘复苏'
$root = 'C:\Users\李帅朋\Documents\ChatGPT\神秘复苏\prototypes\hybrid25d-20261004'
$reports = Join-Path $root 'reports'
$validation = Join-Path $root 'Validation'
$sourceRoots = @('Game\Assets', 'Game\Packages', 'Game\ProjectSettings', 'tests', 'tools')
$started = [DateTimeOffset]::Now

foreach ($path in @((Join-Path $validation 'Game'), (Join-Path $validation 'tests'), (Join-Path $validation 'tools'), (Join-Path $reports 'baseline.json'), (Join-Path $reports 'source-inventory-before.json'))) {
    if (Test-Path -LiteralPath $path) { throw "Existing target; no overwrite: $path" }
}

function Get-SourceInventory {
    $items = [Collections.Generic.List[object]]::new()
    foreach ($relativeRoot in $sourceRoots) {
        $queue = [Collections.Generic.Queue[string]]::new()
        $queue.Enqueue((Join-Path $source $relativeRoot))
        while ($queue.Count -gt 0) {
            $directory = $queue.Dequeue()
            if ((Get-Item -LiteralPath $directory -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Source reparse point; stop: $directory" }
            foreach ($item in Get-ChildItem -LiteralPath $directory -Force) {
                if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Source reparse point; stop: $($item.FullName)" }
                if ($item.PSIsContainer) { $queue.Enqueue($item.FullName); continue }
                $items.Add([pscustomobject]@{
                    path = $item.FullName.Substring($source.Length + 1).Replace('\', '/')
                    bytes = $item.Length
                    sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $item.FullName).Hash
                    lastWriteUtc = $item.LastWriteTimeUtc.ToString('o')
                })
            }
        }
    }
    @($items | Sort-Object path)
}

function Get-InventoryDigest($items) {
    $text = (($items | ForEach-Object { "$($_.sha256)|$($_.bytes)|$($_.path)" }) -join "`n") + "`n"
    $sha = [Security.Cryptography.SHA256]::Create()
    try { [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($text))).Replace('-', '') } finally { $sha.Dispose() }
}

$before = @(Get-SourceInventory)
$processes = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^Unity|^bee|AssetImportWorker' } | Select-Object Id, ProcessName, Path)
$space = @(Get-PSDrive -Name C,D | Select-Object Name, Free)
New-Item -ItemType Directory -Path $reports -Force | Out-Null
New-Item -ItemType Directory -Path $validation -Force | Out-Null
foreach ($entry in $before) {
    $sourcePath = Join-Path $source $entry.path
    $destination = Join-Path $validation $entry.path
    $parent = Split-Path -Parent $destination
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    if (Test-Path -LiteralPath $destination) { throw "Destination collision: $destination" }
    Copy-Item -LiteralPath $sourcePath -Destination $destination
    if ((Get-FileHash -Algorithm SHA256 -LiteralPath $destination).Hash -ne $entry.sha256) { throw "Copy mismatch: $destination" }
}
$after = @(Get-SourceInventory)
if ((Get-InventoryDigest $before) -ne (Get-InventoryDigest $after)) { throw 'Source changed during copy; preserve partial snapshot and stop.' }
$before | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $reports 'source-inventory-before.json') -Encoding utf8
$after | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $reports 'source-inventory-after-copy.json') -Encoding utf8

$oldRoot = 'C:\Users\李帅朋\Documents\ChatGPT\神秘复苏\building_samples\corridor-wall-integration-20261003'
$oldRelativePaths = @('editor-smoke-green/run.json','editor-smoke-green/smoke-output/result.txt','editor-smoke-green/unity.log','player-smoke-final/run.json','player-smoke-final/smoke-output/result.txt','player-smoke-final/player.log','review-final.md','review-result.json')
$oldEvidence = foreach ($relative in $oldRelativePaths) {
    $path = Join-Path $oldRoot $relative
    [pscustomobject]@{ path = $path; sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash }
}
$oldEditorLines = Get-Content -LiteralPath (Join-Path $oldRoot 'editor-smoke-green/unity.log')
$oldPlayerLines = Get-Content -LiteralPath (Join-Path $oldRoot 'player-smoke-final/player.log')
$baseline = [ordered]@{
    schemaVersion = 1
    task = 'H25D-00'
    role = '06 testing and integration'
    startedUtc = $started.UtcDateTime.ToString('o')
    snapshotCompletedUtc = [DateTimeOffset]::UtcNow.ToString('o')
    clientTimezone = 'America/Los_Angeles'
    sourceRoot = $source
    validationRoot = $validation
    sourceRoots = $sourceRoots
    excludedGameRoots = @('Library','Library.*','Temp','Logs','UserSettings')
    sourceInventoryCount = $before.Count
    sourceInventoryBytes = ($before | Measure-Object bytes -Sum).Sum
    sourceInventorySha256 = Get-InventoryDigest $before
    sourceInventoryDigestFormat = 'SHA256 of UTF8(no BOM): sorted SHA256|bytes|relative/path lines, LF, trailing LF'
    sourceUnchangedDuringCopy = $true
    copyByteIdentityBeforeConfigChange = $true
    gitHeadAvailable = $false
    gitNote = 'C and D repositories have no valid HEAD; files are untracked. Source SHA inventory is the baseline.'
    editorVersion = '6000.5.10f1'
    editorRevision = '3bd4f66ad299'
    preflightUnityProcesses = $processes
    preflightSpace = $space
    capabilities = @{ hashing = 'PowerShell SHA256'; copy = 'native PowerShell LiteralPath'; unityRuntime = 'not invoked in H25D-00'; desktop = 'not used' }
    oldBLD = @{
        status = 'historical candidate retained; not synchronized or packaged'
        editorExitCode = 0
        editorSmokePassCount = @($oldEditorLines | Select-String -Pattern 'SMOKE_PASS').Count
        editorResult = (Get-Content -LiteralPath (Join-Path $oldRoot 'editor-smoke-green/smoke-output/result.txt') -Raw).Trim()
        editorSearchExceptionCount = @($oldEditorLines | Select-String -Pattern 'ArgumentOutOfRangeException').Count
        newExeExitCode = 0
        newExeSmokePassCount = @($oldPlayerLines | Select-String -Pattern 'SMOKE_PASS').Count
        newExeResult = (Get-Content -LiteralPath (Join-Path $oldRoot 'player-smoke-final/smoke-output/result.txt') -Raw).Trim()
        staticReview = 'limited static review passed; Search exception disclosed as nonblocking for historical smoke'
        evidence = @($oldEvidence)
        rerunInThisTask = $false
    }
    allowedConfigDelta = @('Validation/Game/Packages/manifest.json: add com.unity.modules.physics 1.0.0','Validation/Game/Packages/packages-lock.json: add builtin physics 1.0.0 depth 0, empty dependencies')
    configDeltaVerified = $false
    productSyncAuthorized = $false
    H25D03Released = $false
    runtimeChecks = 'uncovered; baseline isolation only'
    historicalEffectiveHours = $null
    historicalBudgetNote = 'historical total is unknown, not reset'
    batchEffectiveCapMinutes = 30
    effectiveMinutes = $null
    remainingBatchEffectiveMinutes = $null
    sourceInventory = $before
}
$baseline | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $reports 'baseline.json') -Encoding utf8
"SNAPSHOT_PASS files=$($before.Count) bytes=$($baseline.sourceInventoryBytes) sha=$($baseline.sourceInventorySha256)"
"UNITY_PROCESS_COUNT=$($processes.Count)"
"HISTORICAL_EDITOR=$($baseline.oldBLD.editorSmokePassCount) EXE=$($baseline.oldBLD.newExeSmokePassCount) SEARCH_EXCEPTIONS=$($baseline.oldBLD.editorSearchExceptionCount)"

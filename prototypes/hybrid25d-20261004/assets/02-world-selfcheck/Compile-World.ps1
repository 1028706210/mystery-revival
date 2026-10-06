param(
    [string]$UnityEditor = 'D:\Unity\Editors\6000.5.10f1\Editor'
)
$ErrorActionPreference = 'Stop'
$prototypeRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$worldPath = Join-Path $prototypeRoot 'modules\02\Assets\Hybrid25D\World'
$mono = Join-Path $UnityEditor 'Data\MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $UnityEditor 'Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe'
$managed = Join-Path $UnityEditor 'Data\Managed\UnityEngine'
$netstandard = Join-Path $UnityEditor 'Data\NetStandard\ref\2.1.0\netstandard.dll'
$references = @(
    (Join-Path $managed 'UnityEngine.CoreModule.dll'),
    (Join-Path $managed 'UnityEngine.PhysicsModule.dll'),
    (Join-Path $managed 'UnityEngine.AudioModule.dll'),
    (Join-Path $managed 'UnityEngine.InputLegacyModule.dll'),
    $netstandard
)
foreach ($path in @($mono, $compiler) + $references) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing local compile tool/reference: $path" }
}
$sources = @(Get-ChildItem -LiteralPath $worldPath -Filter '*.cs' -File | Sort-Object Name)
if ($sources.Count -ne 3) { throw 'Expected exactly the three H25D-02 source files.' }
$buildPath = Join-Path $PSScriptRoot 'build'
New-Item -ItemType Directory -Path $buildPath -Force | Out-Null
$output = Join-Path $buildPath 'MysteryRevival.Hybrid25D.World.dll'
$logPath = Join-Path $PSScriptRoot 'compile.log'
$arguments = @('-target:library', '-langversion:7.2', '-nostdlib+', '-warnaserror+', "-out:$output")
$arguments += $references | ForEach-Object { "-r:$_" }
$arguments += $sources.FullName
& $mono $compiler @arguments 2>&1 | Tee-Object -FilePath $logPath
$compileExit = $LASTEXITCODE
"COMPILE_EXIT=$compileExit" | Tee-Object -FilePath $logPath -Append
if ($compileExit -ne 0) { exit $compileExit }
"COMPILED_AGAINST=Unity6000.5.10f1 CoreModule/PhysicsModule/AudioModule/InputLegacyModule (no Editor launch)" | Tee-Object -FilePath $logPath -Append
$contractExe = Join-Path $buildPath 'Check-Contract.exe'
& $mono $compiler '-langversion:7.2' '-warnaserror+' "-out:$contractExe" (Join-Path $PSScriptRoot 'Check-Contract.cs') 2>&1 | Tee-Object -FilePath $logPath -Append
$contractCompileExit = $LASTEXITCODE
"CONTRACT_COMPILE_EXIT=$contractCompileExit" | Tee-Object -FilePath $logPath -Append
if ($contractCompileExit -ne 0) { exit $contractCompileExit }
& $mono $contractExe $output $managed 2>&1 | Tee-Object -FilePath $logPath -Append
$contractExit = $LASTEXITCODE
"CONTRACT_EXIT=$contractExit" | Tee-Object -FilePath $logPath -Append
if ($contractExit -ne 0) { exit $contractExit }
exit 0

param(
    [string]$UnityEditor = 'D:\Unity\Editors\6000.5.10f1\Editor',
    [string]$LogName = 'core-latest.log'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$mono = Join-Path $UnityEditor 'Data\MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $UnityEditor 'Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe'
foreach ($path in @($mono, $compiler)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required local Unity tool missing: $path" }
}
if ([IO.Path]::GetFileName($LogName) -ne $LogName) { throw 'LogName must be a file name.' }
$artifacts = Join-Path $root 'artifacts\tests'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null
$log = Join-Path $artifacts $LogName
$exe = Join-Path $artifacts 'IncidentStateTests.exe'
$source = Join-Path $root 'Game\Assets\Scripts\Domain\IncidentState.cs'
$tests = Join-Path $root 'tests\IncidentStateTests.cs'
"UTC=$([DateTime]::UtcNow.ToString('o'))`nCompiler=$compiler`nRuntime=$mono" | Set-Content -LiteralPath $log -Encoding UTF8
& $mono $compiler '-langversion:7.2' '-warnaserror+' "-out:$exe" $source $tests 2>&1 | Tee-Object -FilePath $log -Append
$compileExit = $LASTEXITCODE
"COMPILE_EXIT=$compileExit" | Tee-Object -FilePath $log -Append
if ($compileExit -ne 0) { exit $compileExit }
& $mono $exe 2>&1 | Tee-Object -FilePath $log -Append
$testExit = $LASTEXITCODE
"TEST_EXIT=$testExit" | Tee-Object -FilePath $log -Append
exit $testExit

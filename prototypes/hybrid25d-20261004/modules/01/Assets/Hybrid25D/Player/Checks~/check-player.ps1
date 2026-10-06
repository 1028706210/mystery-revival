param([string]$UnityData = 'D:\Unity\Editors\6000.5.10f1\Editor\Data')
$ErrorActionPreference = 'Stop'
$playerRoot = Split-Path $PSScriptRoot -Parent
$binRoot = Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Path $binRoot -Force | Out-Null
$compiler = Join-Path $UnityData 'DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll'
$runtime = Join-Path $UnityData 'DotNetSdk\dotnet.exe'
$core = Join-Path $UnityData 'Managed\UnityEngine\UnityEngine.CoreModule.dll'
$logicArgs = @('-nologo', '-noconfig', '-nostdlib+', '-target:exe', ('-out:' + (Join-Path $binRoot 'PlayerLogicChecks.dll')))
$logicArgs += Get-ChildItem -LiteralPath (Join-Path $UnityData 'DotNetSdk\packs\Microsoft.NETCore.App.Ref\8.0.21\ref\net8.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName }
$logicArgs += @(('-r:' + $core), (Join-Path $playerRoot 'HybridPlayerMovement.cs'), (Join-Path $PSScriptRoot 'PlayerLogicChecks.cs'))
& $runtime $compiler @logicArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item -LiteralPath $core -Destination (Join-Path $binRoot 'UnityEngine.CoreModule.dll')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PlayerLogicChecks.runtimeconfig.json') -Destination (Join-Path $binRoot 'PlayerLogicChecks.runtimeconfig.json')
& $runtime (Join-Path $binRoot 'PlayerLogicChecks.dll')
$logicResult = $LASTEXITCODE
if ($logicResult -ne 0) { exit $logicResult }

# Compile the complete module against the installed Unity managed API without launching the Editor.
$moduleArgs = @('-nologo', '-noconfig', '-nostdlib+', '-target:library', ('-out:' + (Join-Path $binRoot 'Hybrid25D.Player.dll')))
$moduleArgs += Get-ChildItem -LiteralPath (Join-Path $UnityData 'NetStandard\ref\2.1.0') -Filter '*.dll' | ForEach-Object { '-r:' + $_.FullName }
$moduleArgs += @(('-r:' + $core),
    ('-r:' + (Join-Path $UnityData 'Managed\UnityEngine\UnityEngine.PhysicsModule.dll')),
    ('-r:' + (Join-Path $UnityData 'Managed\UnityEngine\UnityEngine.InputLegacyModule.dll')))
$moduleArgs += Get-ChildItem -LiteralPath $playerRoot -Filter '*.cs' | ForEach-Object { $_.FullName }
& $runtime $compiler @moduleArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Output 'MODULE_COMPILE passed (managed API only; not Unity import/Play Mode)'

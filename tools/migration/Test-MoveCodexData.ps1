[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Move-CodexData.ps1')
$fixture = Join-Path $PSScriptRoot ('test-fixture-' + [guid]::NewGuid().ToString('N'))
$src = Join-Path $fixture 'source'
$dst = Join-Path $fixture 'copied'
New-Item -ItemType Directory -Path (Join-Path $src 'chrome\version') -Force | Out-Null
[IO.File]::WriteAllText((Join-Path $src 'example.txt'), 'nonsecret fixture')
Set-Content -LiteralPath (Join-Path $src 'example.txt') -Stream 'fixture' -Value 'alternate stream'
New-Item -ItemType Junction -Path (Join-Path $src 'chrome\latest') -Target (Join-Path $src 'chrome\version') | Out-Null
Copy-VerifiedTree $src $dst
$overwriteBlocked = $false
try { Copy-VerifiedTree $src $dst } catch { $overwriteBlocked = $_.Exception.Message -like 'Destination already exists*' }
if (!$overwriteBlocked) { throw 'Expected refusal of an existing destination.' }
$tamper = [IO.File]::OpenWrite((Join-Path $dst 'example.txt'))
try { $tamper.WriteByte(88) } finally { $tamper.Dispose() }
$mismatchBlocked = $false
try { Assert-IdenticalTree $src $dst } catch { $mismatchBlocked = $_.Exception.Message -like 'SHA256 mismatch*' }
if (!$mismatchBlocked) { throw 'Expected rejection of a modified file.' }
Write-Host "PASS: copied file/ADS/junction, preserved ACL, refused overwrite, rejected hash mismatch. Fixture retained: $fixture"

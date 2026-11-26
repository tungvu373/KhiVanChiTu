$ErrorActionPreference = 'Stop'
$localServerData = Join-Path $env:LOCALAPPDATA 'KhiVanChiTu/LocalServer'
New-Item -ItemType Directory -Force -Path $localServerData | Out-Null
$localServerArchive = Join-Path $localServerData 'nakama-3.42.0-windows-amd64.tar.gz'
$localServerUri = 'https://github.com/heroiclabs/nakama/releases/download/v3.42.0/nakama-3.42.0-windows-amd64.tar.gz'
if (Test-Path -LiteralPath (Join-Path $localServerData 'nakama.exe')) { return }
curl.exe --fail --location --silent --show-error --output $localServerArchive $localServerUri
if ($LASTEXITCODE -ne 0) { throw 'Could not download Nakama.' }
$localServerHash = '79384b422fd1991d3d83985684579ad046d904c143ebda784f35dfe3e14068ae'
if ((Get-FileHash -LiteralPath $localServerArchive -Algorithm SHA256).Hash -ne $localServerHash) {
    throw 'The Nakama download hash does not match the GitHub release.'
}
tar -xf $localServerArchive -C $localServerData nakama.exe
if ($LASTEXITCODE -ne 0) { throw 'Could not extract Nakama.' }

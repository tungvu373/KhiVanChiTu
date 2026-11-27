param(
    [switch]$InstallOnly
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$localData = Join-Path $env:LOCALAPPDATA 'KhiVanChiTu\LocalServer'
New-Item -ItemType Directory -Force -Path $localData | Out-Null

$cfExe = Join-Path $localData 'cloudflared.exe'
if (-not (Test-Path $cfExe)) {
    Write-Host '[1/3] Dang tai cloudflared.exe chinh chu tu Cloudflare (GitHub Releases)...' -ForegroundColor Cyan
    $dlUrl = 'https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe'
    Invoke-WebRequest -Uri $dlUrl -OutFile $cfExe -UseBasicParsing
    Write-Host "Da tai xong: $cfExe" -ForegroundColor Green
}

if ($InstallOnly) {
    Write-Host 'cloudflared ready.'
    exit 0
}

$nakamaExe = Join-Path $localData 'nakama.exe'
if (-not (Test-Path $nakamaExe)) {
    Write-Host '[1.5/3] Chua co Nakama, dang chay Install-Nakama.ps1...' -ForegroundColor Yellow
    & (Join-Path $scriptDir 'Install-Nakama.ps1')
}

Write-Host '[2/3] Khoi dong Local Server (Node.js + Nakama + PostgreSQL, che do Public Tunnel)...' -ForegroundColor Cyan
$env:KHIVANTU_PUBLIC_TUNNEL = '1'
$serverLogOut = Join-Path $localData 'public_server.out.log'
$serverLogErr = Join-Path $localData 'public_server.err.log'
$serverProc = Start-Process -FilePath 'node' -ArgumentList 'server.mjs' -WorkingDirectory $scriptDir -RedirectStandardOutput $serverLogOut -RedirectStandardError $serverLogErr -PassThru -NoNewWindow

$ready = $false
for ($i = 0; $i -lt 80; $i++) {
    if ($serverProc.HasExited) {
        Get-Content $serverLogErr -ErrorAction SilentlyContinue
        throw "LocalServer exited early with code $($serverProc.ExitCode)"
    }
    try {
        $resp = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/healthcheck' -UseBasicParsing -TimeoutSec 2
        if ($resp.StatusCode -eq 200) { $ready = $true; break }
    } catch {}
    Start-Sleep -Milliseconds 350
}
if (-not $ready) {
    Stop-Process -Id $serverProc.Id -Force -ErrorAction SilentlyContinue
    throw 'LocalServer healthcheck timeout.'
}

Write-Host '[3/3] Dang mo Cloudflare Public HTTPS Tunnel -> http://127.0.0.1:8080 ...' -ForegroundColor Cyan
Write-Host 'Nhan Ctrl+C trong cua so nay de tat ca may chu va duong ham.' -ForegroundColor Yellow

try {
    & $cfExe tunnel --url http://127.0.0.1:8080 --no-autoupdate
}
finally {
    if ($serverProc -and -not $serverProc.HasExited) {
        Write-Host 'Dang tat Local Server...' -ForegroundColor Yellow
        Stop-Process -Id $serverProc.Id -Force -ErrorAction SilentlyContinue
    }
}

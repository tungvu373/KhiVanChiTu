@echo off
chcp 65001 >nul
cd /d "%~dp0"
where node >nul 2>&1
if errorlevel 1 (
  echo [LOI] May chua cai Node.js ^(yeu cau Node.js 22 tro len^).
  echo Vui long tai va cai Node.js tai https://nodejs.org/ roi mo lai file nay.
  pause
  exit /b 1
)
echo Dang kiem tra Console Dieu Khien Khi Van Chi Tu...
powershell -NoProfile -ExecutionPolicy Bypass -Command "try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:9090/api/status' -UseBasicParsing -TimeoutSec 1; exit 0 } catch { exit 1 }"
if errorlevel 1 (
  echo Dang khoi dong Console nen tai http://khivan.localhost:9090/ ...
  start "KhiVan Server Console" /min node control-console.mjs
  powershell -NoProfile -ExecutionPolicy Bypass -Command "for($i=0;$i -lt 20;$i++){try{$r=Invoke-WebRequest -Uri 'http://127.0.0.1:9090/api/status' -UseBasicParsing -TimeoutSec 1; if($r.StatusCode -eq 200){exit 0}}catch{}; Start-Sleep -Milliseconds 250}"
)
start "" "http://khivan.localhost:9090/"

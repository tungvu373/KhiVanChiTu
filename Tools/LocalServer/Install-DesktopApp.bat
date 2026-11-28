@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install-DesktopApp.ps1"
echo.
echo Ban da co the mo App "Khi Van Server Console" ngay tren man hinh Desktop!
pause

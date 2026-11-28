@echo off
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Process-GoogleFlowAudio.ps1" -AlsoScanDownloads %*
pause

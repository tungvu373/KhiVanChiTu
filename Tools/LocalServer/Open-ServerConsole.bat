@echo off
chcp 65001 >nul
cd /d "%~dp0"
wscript.exe "%~dp0Launch-KhivanConsole.vbs"

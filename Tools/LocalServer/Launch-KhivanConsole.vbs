Option Explicit
Dim fso, scriptDir, psPath, shell, cmd
Set fso = CreateObject("Scripting.FileSystemObject")
scriptDir = fso.GetParentFolderName(WScript.ScriptFullName)
psPath = fso.BuildPath(scriptDir, "Launch-KhivanConsole.ps1")
Set shell = CreateObject("WScript.Shell")
cmd = "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File """ & psPath & """"
shell.Run cmd, 0, False

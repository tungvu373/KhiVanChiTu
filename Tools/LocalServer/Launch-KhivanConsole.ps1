$ErrorActionPreference = 'Stop'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ConsolePort = 9090
$StatusUrl = "http://127.0.0.1:$ConsolePort/api/status"
$AppUrl = "http://khivan.localhost:$ConsolePort/"
$ProfileDir = Join-Path $env:LOCALAPPDATA "KhiVanChiTu\ConsoleAppProfile"

function Show-ErrorBox([string]$Message) {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
    [System.Windows.Forms.MessageBox]::Show(
        $Message,
        "KhiVan Server Console",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error
    ) | Out-Null
}

function Test-ConsoleReady {
    try {
        $r = Invoke-WebRequest -Uri $StatusUrl -UseBasicParsing -TimeoutSec 1
        return ($r.StatusCode -eq 200)
    } catch {
        return $false
    }
}

$nodeCmd = Get-Command node.exe -ErrorAction SilentlyContinue
if (-not $nodeCmd) {
    Show-ErrorBox "May tinh chua cai Node.js (yeu cau Node.js 22 tro len).`nVui long tai Node.js tai https://nodejs.org/ roi mo lai ung dung."
    exit 1
}

if (-not (Test-ConsoleReady)) {
    Start-Process -FilePath $nodeCmd.Source `
        -ArgumentList "control-console.mjs" `
        -WorkingDirectory $ScriptDir `
        -WindowStyle Hidden

    $ready = $false
    for ($i = 0; $i -lt 24; $i++) {
        Start-Sleep -Milliseconds 250
        if (Test-ConsoleReady) {
            $ready = $true
            break
        }
    }

    if (-not $ready) {
        Show-ErrorBox "Khong khoi dong duoc KhiVan Server Console tai cong $ConsolePort.`nHay kiem tra xem co tien trinh nao dang chiem cong $ConsolePort khong."
        exit 1
    }
}

New-Item -ItemType Directory -Force -Path $ProfileDir | Out-Null

$browserCandidates = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
    "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe",
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe"
)

$appBrowser = $browserCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($appBrowser) {
    $args = @(
        "--app=$AppUrl",
        "--window-size=1140,860",
        "--user-data-dir=`"$ProfileDir`"",
        "--no-first-run",
        "--no-default-browser-check"
    )
    Start-Process -FilePath $appBrowser -ArgumentList $args
} else {
    Start-Process $AppUrl
}

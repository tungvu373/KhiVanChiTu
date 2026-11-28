param(
    [switch]$Silent
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..")
$SourcePng = Join-Path $RepoRoot "Assets\_Game\Resources\CodexDrop\HUD\hud_toggle_bagua.png"
$OutPng = Join-Path $ScriptDir "khivan-console.png"
$OutIco = Join-Path $ScriptDir "khivan-console.ico"
$VbsPath = Join-Path $ScriptDir "Launch-KhivanConsole.vbs"

Add-Type -AssemblyName System.Drawing

function New-IconFromPng([string]$SrcPng, [string]$DstPng, [string]$DstIco) {
    if (-not (Test-Path $SrcPng)) { return }

    $srcImg = [System.Drawing.Image]::FromFile($SrcPng)
    try {
        # Save crisp 128x128 PNG for web app favicon
        $pngBmp = New-Object System.Drawing.Bitmap(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g1 = [System.Drawing.Graphics]::FromImage($pngBmp)
        $g1.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g1.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g1.DrawImage($srcImg, 0, 0, 128, 128)
        $g1.Dispose()
        $pngBmp.Save($DstPng, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngBmp.Dispose()

        # Build multi-size PNG-in-ICO (256, 64, 32) for sharp Windows Desktop & Taskbar icon
        $sizes = @(256, 64, 32)
        $frames = @()
        foreach ($sz in $sizes) {
            $bmp = New-Object System.Drawing.Bitmap($sz, $sz, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $g.DrawImage($srcImg, 0, 0, $sz, $sz)
            $g.Dispose()
            $ms = New-Object System.IO.MemoryStream
            $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
            $bmp.Dispose()
            $frames += [PSCustomObject]@{ Size = $sz; Bytes = $ms.ToArray() }
            $ms.Dispose()
        }

        $fs = [System.IO.File]::Open($DstIco, [System.IO.FileMode]::Create)
        $bw = New-Object System.IO.BinaryWriter($fs)
        try {
            $bw.Write([UInt16]0)
            $bw.Write([UInt16]1)
            $bw.Write([UInt16]$frames.Count)

            $offset = 6 + (16 * $frames.Count)
            foreach ($f in $frames) {
                $dimByte = if ($f.Size -ge 256) { [byte]0 } else { [byte]$f.Size }
                $bw.Write($dimByte)
                $bw.Write($dimByte)
                $bw.Write([byte]0)
                $bw.Write([byte]0)
                $bw.Write([UInt16]1)
                $bw.Write([UInt16]32)
                $bw.Write([UInt32]$f.Bytes.Length)
                $bw.Write([UInt32]$offset)
                $offset += $f.Bytes.Length
            }
            foreach ($f in $frames) {
                $bw.Write($f.Bytes)
            }
        } finally {
            $bw.Dispose()
            $fs.Dispose()
        }
    } finally {
        $srcImg.Dispose()
    }
}

New-IconFromPng -SrcPng $SourcePng -DstPng $OutPng -DstIco $OutIco

$DesktopDir = [Environment]::GetFolderPath('Desktop')
$ShortcutPath = Join-Path $DesktopDir "KhiVan Server Console.lnk"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = "$env:WINDIR\System32\wscript.exe"
$Shortcut.Arguments = "`"$VbsPath`""
$Shortcut.WorkingDirectory = $ScriptDir
if (Test-Path $OutIco) {
    $Shortcut.IconLocation = "$OutIco,0"
}
$Shortcut.Description = "Khi Van Chi Tu - App Dieu Khien Local Server & Public Tunnel"
$Shortcut.Save()

Write-Output "CREATED_SHORTCUT=$ShortcutPath"
Write-Output "CREATED_ICON=$OutIco"

if (-not $Silent) {
    Write-Host "Da tao App Desktop thanh cong: $ShortcutPath" -ForegroundColor Green
}

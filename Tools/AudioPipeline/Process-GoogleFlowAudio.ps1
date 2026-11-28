param(
    [string]$InputDir = "",
    [switch]$AlsoScanDownloads,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
if ([string]::IsNullOrWhiteSpace($InputDir)) {
    $InputDir = Join-Path $scriptRoot "RawGoogleFlowDrop"
}
$outputRoot = Join-Path $projectRoot "Assets\_Game\Resources\AudioDrop"
$toolsDir = Join-Path $env:LOCALAPPDATA "KhiVanChiTu\AudioTools"

New-Item -ItemType Directory -Force -Path $InputDir | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot "Ambience") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot "Voice\Male") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot "Voice\Female") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot "Monsters") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $outputRoot "SFX") | Out-Null

function Resolve-Ffmpeg {
    $cmd = Get-Command ffmpeg -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $localExe = Get-ChildItem -Path $toolsDir -Filter "ffmpeg.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($localExe) { return $localExe.FullName }

    $knownRoots = @(
        (Join-Path $env:APPDATA "SnapAny\tools\ffmpeg"),
        (Join-Path $env:APPDATA "Subtitle Edit\ffmpeg"),
        (Join-Path $env:LOCALAPPDATA "CapCut\Apps")
    )
    foreach ($kr in $knownRoots) {
        if (Test-Path $kr) {
            $hit = Get-ChildItem -Path $kr -Filter "ffmpeg.exe" -Recurse -Depth 3 -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($hit) { return $hit.FullName }
        }
    }

    Write-Host "[AudioPipeline] Downloading portable FFmpeg into $toolsDir ..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null
    $zipPath = Join-Path $toolsDir "ffmpeg-essentials.zip"
    $url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
    $curlExe = Get-Command curl.exe -ErrorAction SilentlyContinue
    if ($curlExe) {
        & $curlExe.Source -fSL --retry 2 -o $zipPath $url
        if ($LASTEXITCODE -ne 0) { throw "curl.exe failed to download FFmpeg (exit=$LASTEXITCODE)" }
    } else {
        $ProgressPreference = 'SilentlyContinue'
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing
    }
    Write-Host "[AudioPipeline] Extracting FFmpeg..." -ForegroundColor Cyan
    $ProgressPreference = 'SilentlyContinue'
    Expand-Archive -Path $zipPath -DestinationPath $toolsDir -Force
    Remove-Item -Path $zipPath -Force -ErrorAction SilentlyContinue

    $localExe = Get-ChildItem -Path $toolsDir -Filter "ffmpeg.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $localExe) {
        throw "Failed to locate ffmpeg.exe after extracting archive."
    }
    return $localExe.FullName
}

function Get-RouteSpec([string]$baseName) {
    $lo = $baseName.ToLowerInvariant()
    if ($lo.StartsWith("khivan_")) { $lo = $lo.Substring(7) }

    if ($lo.StartsWith("amb_")) {
        return @{ SubDir = "Ambience"; Name = $lo; Lufs = -24; Channels = 2; Rate = 44100; SeamlessLoop = $true; TrimSilence = $false; MaxDuration = 0 }
    }
    if ($lo.StartsWith("vo_male_")) {
        $isRealm = $lo.Contains("_realm_")
        return @{ SubDir = "Voice\Male"; Name = $lo; Lufs = -13; Channels = $(if ($isRealm) { 2 } else { 1 }); Rate = $(if ($isRealm) { 44100 } else { 24000 }); SeamlessLoop = $false; TrimSilence = $true; MaxDuration = 0 }
    }
    if ($lo.StartsWith("vo_female_")) {
        $isRealm = $lo.Contains("_realm_")
        return @{ SubDir = "Voice\Female"; Name = $lo; Lufs = -13; Channels = $(if ($isRealm) { 2 } else { 1 }); Rate = $(if ($isRealm) { 44100 } else { 24000 }); SeamlessLoop = $false; TrimSilence = $true; MaxDuration = 0 }
    }
    if ($lo.StartsWith("mob_")) {
        $isBoss = $lo.Contains("boss")
        return @{ SubDir = "Monsters"; Name = $lo; Lufs = -16; Channels = 1; Rate = 24000; SeamlessLoop = $false; TrimSilence = $true; MaxDuration = $(if ($isBoss) { 4.2 } else { 2.2 }) }
    }
    if ($lo.StartsWith("sfx_")) {
        return @{ SubDir = "SFX"; Name = $lo; Lufs = -15; Channels = 1; Rate = 24000; SeamlessLoop = $false; TrimSilence = $true; MaxDuration = 3.0 }
    }
    if ($lo.StartsWith("bgm_")) {
        return @{ SubDir = "BGM"; Name = $lo; Lufs = -18; Channels = 2; Rate = 44100; SeamlessLoop = $true; TrimSilence = $false; MaxDuration = 0 }
    }
    return $null
}

$ffmpeg = Resolve-Ffmpeg
Write-Host "[AudioPipeline] Using FFmpeg: $ffmpeg" -ForegroundColor Green

$sources = @()
if (Test-Path $InputDir) {
    $sources += Get-ChildItem -Path $InputDir -File | Where-Object { $_.Extension -match '^\.(mp4|mov|webm|wav|mp3|ogg|m4a)$' }
}
if ($AlsoScanDownloads) {
    $dlDir = Join-Path $env:USERPROFILE "Downloads"
    if (Test-Path $dlDir) {
        $sources += Get-ChildItem -Path $dlDir -File | Where-Object {
            $_.Extension -match '^\.(mp4|mov|webm|wav|mp3|ogg|m4a)$' -and
            $_.BaseName.ToLowerInvariant() -match '^(khivan_)?(amb_|vo_male_|vo_female_|mob_|sfx_|bgm_)'
        }
    }
}

if ($sources.Count -eq 0) {
    Write-Host "[AudioPipeline] No input clips found in: $InputDir" -ForegroundColor Yellow
    Write-Host "Drop Google Flow .mp4/.wav/.mp3 exports named like vo_male_realm_nguyenanh.mp4, amb_act1_meadow.mp4, mob_boss_roar.mp4 into RawGoogleFlowDrop and re-run."
    exit 0
}

$processed = 0
$skipped = 0

foreach ($file in $sources) {
    $spec = Get-RouteSpec $file.BaseName
    if ($null -eq $spec) {
        Write-Host "[SKIP] Unknown naming prefix: $($file.Name)" -ForegroundColor DarkYellow
        $skipped++
        continue
    }

    $destFolder = Join-Path $outputRoot $spec.SubDir
    New-Item -ItemType Directory -Force -Path $destFolder | Out-Null
    $destFile = Join-Path $destFolder ($spec.Name + ".ogg")

    if ((Test-Path $destFile) -and (-not $Force)) {
        if ((Get-Item $destFile).LastWriteTimeUtc -ge $file.LastWriteTimeUtc) {
            Write-Host "[UP-TO-DATE] $($spec.SubDir)\$($spec.Name).ogg" -ForegroundColor DarkGray
            $skipped++
            continue
        }
    }

    Write-Host "[PROCESS] $($file.Name) -> $($spec.SubDir)\$($spec.Name).ogg (I=$($spec.Lufs) LUFS, $($spec.Channels)ch, $($spec.Rate)Hz)" -ForegroundColor Cyan

    if ($spec.SeamlessLoop) {
        # True seamless loop: crossfade last 1.2s onto first 1.2s so Unity loop wrap has zero seam or click
        $complex = "[0:a]loudnorm=I=$($spec.Lufs):TP=-1.5:LRA=11,asplit=2[a][b];[a]atrim=start=1.2,asetpts=PTS-STARTPTS[main];[b]atrim=start=0:end=1.2,asetpts=PTS-STARTPTS[head];[main][head]acrossfade=d=1.2:c1=tri:c2=tri[out]"
        $args = @(
            "-hide_banner",
            "-loglevel", "error",
            "-y",
            "-i", $file.FullName,
            "-vn",
            "-filter_complex", $complex,
            "-map", "[out]",
            "-ac", $spec.Channels.ToString(),
            "-ar", $spec.Rate.ToString(),
            "-c:a", "libvorbis",
            "-q:a", "4",
            $destFile
        )
    } else {
        $filters = @()
        if ($spec.TrimSilence) {
            # Trim leading AND trailing silence so voice auto-ducking releases right after speech finishes
            $filters += "silenceremove=start_periods=1:start_duration=0.04:start_threshold=-42dB"
            $filters += "areverse,silenceremove=start_periods=1:start_duration=0.08:start_threshold=-42dB,areverse"
        }
        if ($spec.MaxDuration -gt 0) {
            $maxStr = $spec.MaxDuration.ToString([System.Globalization.CultureInfo]::InvariantCulture)
            $fadeStart = ([Math]::Max(0.2, $spec.MaxDuration - 0.28)).ToString("0.00", [System.Globalization.CultureInfo]::InvariantCulture)
            $filters += "atrim=0:$maxStr,asetpts=PTS-STARTPTS"
            $filters += "afade=t=out:st=${fadeStart}:d=0.28"
        }
        $filters += "loudnorm=I=$($spec.Lufs):TP=-1.5:LRA=11"
        $filterChain = ($filters -join ",")
        $args = @(
            "-hide_banner",
            "-loglevel", "error",
            "-y",
            "-i", $file.FullName,
            "-vn",
            "-ac", $spec.Channels.ToString(),
            "-ar", $spec.Rate.ToString(),
            "-af", $filterChain,
            "-c:a", "libvorbis",
            "-q:a", "4",
            $destFile
        )
    }
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    & $ffmpeg @args
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $prevEap
    if ($exitCode -ne 0) {
        Write-Host "[ERROR] FFmpeg failed on $($file.Name) (exit=$exitCode)" -ForegroundColor Red
        continue
    }
    $processed++
}

Write-Host "[AudioPipeline] Done. Processed=$processed, Skipped=$skipped -> $outputRoot" -ForegroundColor Green

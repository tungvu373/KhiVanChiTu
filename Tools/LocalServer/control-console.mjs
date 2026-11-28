import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { spawn, execFile } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const CONSOLE_HOST = '127.0.0.1';
const CONSOLE_PORT = Number(process.env.KHIVANTU_CONSOLE_PORT || 9090);
const LOCAL_DATA = path.join(process.env.LOCALAPPDATA || path.join(os.homedir(), 'AppData', 'Local'), 'KhiVanChiTu', 'LocalServer');
const PUBLIC_URL_FILE = path.join(LOCAL_DATA, 'public_url.txt');
const KEYS_FILE = path.join(LOCAL_DATA, 'keys.json');
const LOG_SERVER_OUT = path.join(LOCAL_DATA, 'public_server.out.log');
const LOG_SERVER_ERR = path.join(LOCAL_DATA, 'public_server.err.log');
const LOG_TUNNEL = path.join(LOCAL_DATA, 'tunnel.log');
const CF_EXE = path.join(LOCAL_DATA, 'cloudflared.exe');
const NAKAMA_EXE = path.join(LOCAL_DATA, 'nakama.exe');
const WEBGL_INDEX = path.resolve(__dirname, '..', '..', 'Builds', 'LocalWeb', 'index.html');
const NODE_MODULES_CHECK = path.join(__dirname, 'node_modules', '@heroiclabs', 'nakama-js');
const ICON_PNG = path.join(__dirname, 'khivan-console.png');

fs.mkdirSync(LOCAL_DATA, { recursive: true });

const ALLOWED_HOSTS = new Set([
  `127.0.0.1:${CONSOLE_PORT}`,
  `localhost:${CONSOLE_PORT}`,
  `khivan.localhost:${CONSOLE_PORT}`,
  `server.khivan.localhost:${CONSOLE_PORT}`
]);

let managedServer = null;
let managedTunnel = null;
let currentMode = 'unknown'; // 'public' | 'lan' | 'stopped' | 'unknown'
let actionBusy = false;
let actionMessage = '';
let lastStatsCache = { ts: 0, data: null };

function getLanIp() {
  const nets = os.networkInterfaces();
  for (const name of Object.keys(nets)) {
    for (const net of nets[name] || []) {
      if (net.family === 'IPv4' && !net.internal) {
        if (net.address.startsWith('192.168.') || net.address.startsWith('10.') || net.address.startsWith('172.')) {
          return net.address;
        }
      }
    }
  }
  return '127.0.0.1';
}

function readJsonSafe(filePath) {
  try {
    return JSON.parse(fs.readFileSync(filePath, 'utf8'));
  } catch {
    return null;
  }
}

function readPublicUrlFile() {
  try {
    const txt = fs.readFileSync(PUBLIC_URL_FILE, 'utf8').trim();
    if (/^https:\/\/[a-z0-9-]+\.trycloudflare\.com$/i.test(txt)) return txt;
  } catch {}
  return '';
}

function writePublicUrlFile(url) {
  try {
    if (url) fs.writeFileSync(PUBLIC_URL_FILE, url.trim() + '\n', 'utf8');
    else if (fs.existsSync(PUBLIC_URL_FILE)) fs.unlinkSync(PUBLIC_URL_FILE);
  } catch {}
}

function tailFile(filePath, maxLines = 60) {
  try {
    if (!fs.existsSync(filePath)) return '(Chưa có log)';
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split(/\r?\n/);
    return lines.slice(-maxLines).join('\n').trim() || '(Trống)';
  } catch (e) {
    return `(Không đọc được log: ${e.message})`;
  }
}

function probeHttp(url, timeoutMs = 1200) {
  return new Promise((resolve) => {
    const req = http.get(url, { timeout: timeoutMs }, (res) => {
      let body = '';
      res.on('data', (c) => { if (body.length < 4096) body += c; });
      res.on('end', () => resolve({ ok: res.statusCode >= 200 && res.statusCode < 400, status: res.statusCode, body }));
    });
    req.on('timeout', () => { req.destroy(); resolve({ ok: false, status: 0, body: '' }); });
    req.on('error', () => resolve({ ok: false, status: 0, body: '' }));
  });
}

function runPowerShell(script) {
  return new Promise((resolve, reject) => {
    execFile(
      'powershell.exe',
      ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', script],
      { windowsHide: true, maxBuffer: 1024 * 1024 },
      (err, stdout, stderr) => {
        if (err) return reject(new Error(stderr || err.message));
        resolve(stdout.trim());
      }
    );
  });
}

async function queryProcessMetrics() {
  const now = Date.now();
  if (lastStatsCache.data && now - lastStatsCache.ts < 2500) {
    return lastStatsCache.data;
  }

  const ps = `
$ErrorActionPreference = 'SilentlyContinue'
$procs = Get-CimInstance Win32_Process | Where-Object {
  ($_.Name -match '^(node|nakama|postgres|cloudflared)\\.exe$') -and
  ($_.ProcessId -ne ${process.pid}) -and
  (
    ($_.Name -eq 'node.exe' -and $_.CommandLine -match 'server\\.mjs') -or
    ($_.Name -eq 'nakama.exe' -and $_.ExecutablePath -match 'KhiVanChiTu\\\\LocalServer') -or
    ($_.Name -eq 'postgres.exe' -and $_.CommandLine -match 'KhiVanChiTu') -or
    ($_.Name -eq 'cloudflared.exe' -and $_.CommandLine -match '127\\.0\\.0\\.1:8080')
  )
}
$items = @()
foreach ($p in $procs) {
  $gp = Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue
  if ($gp) {
    $items += [PSCustomObject]@{
      pid = $p.ProcessId
      name = $p.Name.ToLower()
      ramMB = [math]::Round($gp.WorkingSet64 / 1MB, 2)
      cpuSec = [math]::Round($gp.CPU, 2)
    }
  }
}
$tcp8080 = @(Get-NetTCPConnection -LocalPort 8080 -State Established -ErrorAction SilentlyContinue).Count
$os = Get-CimInstance Win32_OperatingSystem
$result = [PSCustomObject]@{
  hostRamTotalGB = [math]::Round($os.TotalVisibleMemorySize / 1MB, 2)
  hostRamFreeGB = [math]::Round($os.FreePhysicalMemory / 1MB, 2)
  tcp8080 = $tcp8080
  processes = $items
}
$result | ConvertTo-Json -Depth 4 -Compress
`;

  try {
    const raw = await runPowerShell(ps);
    const parsed = JSON.parse(raw || '{}');
    const list = Array.isArray(parsed.processes)
      ? parsed.processes
      : parsed.processes
        ? [parsed.processes]
        : [];
    const grouped = {
      node: { count: 0, ramMB: 0, cpuSec: 0 },
      nakama: { count: 0, ramMB: 0, cpuSec: 0 },
      postgres: { count: 0, ramMB: 0, cpuSec: 0 },
      cloudflared: { count: 0, ramMB: 0, cpuSec: 0 }
    };
    let totalRamMB = 0;
    for (const p of list) {
      const key = p.name.replace(/\.exe$/i, '');
      if (grouped[key]) {
        grouped[key].count += 1;
        grouped[key].ramMB = Number((grouped[key].ramMB + (p.ramMB || 0)).toFixed(2));
        grouped[key].cpuSec = Number((grouped[key].cpuSec + (p.cpuSec || 0)).toFixed(2));
      }
      totalRamMB += p.ramMB || 0;
    }
    const data = {
      hostRamTotalGB: parsed.hostRamTotalGB || Number((os.totalmem() / 1073741824).toFixed(2)),
      hostRamFreeGB: parsed.hostRamFreeGB || Number((os.freemem() / 1073741824).toFixed(2)),
      tcp8080: parsed.tcp8080 || 0,
      totalServerRamMB: Number(totalRamMB.toFixed(2)),
      components: grouped
    };
    lastStatsCache = { ts: now, data };
    return data;
  } catch {
    return {
      hostRamTotalGB: Number((os.totalmem() / 1073741824).toFixed(2)),
      hostRamFreeGB: Number((os.freemem() / 1073741824).toFixed(2)),
      tcp8080: 0,
      totalServerRamMB: 0,
      components: {
        node: { count: 0, ramMB: 0, cpuSec: 0 },
        nakama: { count: 0, ramMB: 0, cpuSec: 0 },
        postgres: { count: 0, ramMB: 0, cpuSec: 0 },
        cloudflared: { count: 0, ramMB: 0, cpuSec: 0 }
      }
    };
  }
}

async function stopAllGameServerProcesses() {
  if (managedTunnel && !managedTunnel.killed) {
    try { managedTunnel.kill(); } catch {}
  }
  managedTunnel = null;
  if (managedServer && !managedServer.killed) {
    try { managedServer.kill(); } catch {}
  }
  managedServer = null;

  const ps = `
$ErrorActionPreference = 'SilentlyContinue'
$targets = Get-CimInstance Win32_Process | Where-Object {
  ($_.ProcessId -ne ${process.pid}) -and (
    ($_.Name -eq 'cloudflared.exe' -and $_.CommandLine -match '127\\.0\\.0\\.1:8080') -or
    ($_.Name -eq 'node.exe' -and $_.CommandLine -match 'server\\.mjs') -or
    ($_.Name -eq 'nakama.exe' -and $_.ExecutablePath -match 'KhiVanChiTu\\\\LocalServer') -or
    ($_.Name -eq 'postgres.exe' -and $_.CommandLine -match 'KhiVanChiTu') -or
    ($_.Name -eq 'powershell.exe' -and $_.CommandLine -match 'Start-PublicHost\\.ps1')
  )
}
foreach ($t in $targets) {
  Stop-Process -Id $t.ProcessId -Force -ErrorAction SilentlyContinue
}
`;
  await runPowerShell(ps);
  writePublicUrlFile('');
  currentMode = 'stopped';
  lastStatsCache = { ts: 0, data: null };
  await new Promise((r) => setTimeout(r, 700));
}

function runCommand(cmd, args, cwd) {
  return new Promise((resolve, reject) => {
    execFile(
      cmd,
      args,
      { cwd, windowsHide: true, maxBuffer: 4 * 1024 * 1024 },
      (err, stdout, stderr) => {
        if (err) return reject(new Error((stderr || stdout || err.message).trim()));
        resolve((stdout || '').trim());
      }
    );
  });
}

async function ensureServerDependencies({ needTunnel }) {
  if (!fs.existsSync(NODE_MODULES_CHECK)) {
    actionMessage = 'Lần đầu chạy: đang cài thư viện Node (npm install)...';
    await runCommand('cmd.exe', ['/c', 'npm', 'install', '--no-audit', '--no-fund'], __dirname);
  }
  if (!fs.existsSync(NAKAMA_EXE)) {
    actionMessage = 'Lần đầu chạy: đang tải Nakama Server...';
    await runCommand('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', '.\\Install-Nakama.ps1'], __dirname);
  }
  if (needTunnel && !fs.existsSync(CF_EXE)) {
    actionMessage = 'Lần đầu chạy: đang tải Cloudflare Tunnel...';
    await runCommand('powershell.exe', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', '.\\Start-PublicHost.ps1', '-InstallOnly'], __dirname);
  }
}

async function startNodeServer(publicTunnel) {
  if (!fs.existsSync(NAKAMA_EXE)) {
    throw new Error('Chưa cài Nakama. Hãy chạy Install-Nakama.ps1 trước.');
  }
  const outFd = fs.openSync(LOG_SERVER_OUT, 'w');
  const errFd = fs.openSync(LOG_SERVER_ERR, 'w');
  const env = { ...process.env, KHIVANTU_PUBLIC_TUNNEL: publicTunnel ? '1' : '0' };
  const child = spawn(process.execPath, ['server.mjs'], {
    cwd: __dirname,
    env,
    stdio: ['ignore', outFd, errFd],
    detached: true,
    windowsHide: true
  });
  child.unref();
  managedServer = child;
  child.on('exit', () => {
    if (managedServer === child) managedServer = null;
  });

  // Wait up to 30s for healthcheck on 127.0.0.1:8080
  for (let i = 0; i < 75; i++) {
    if (child.exitCode !== null) {
      throw new Error(`server.mjs thoát sớm (mã ${child.exitCode}). Xem tab Server Error để biết chi tiết.`);
    }
    const res = await probeHttp('http://127.0.0.1:8080/healthcheck', 1000);
    if (res.ok) return true;
    await new Promise((r) => setTimeout(r, 400));
  }
  throw new Error('Quá thời gian chờ máy chủ (8080) khởi động.');
}

async function startCloudflareTunnel() {
  if (!fs.existsSync(CF_EXE)) {
    throw new Error('Chưa có cloudflared.exe. Hãy chạy Start-PublicHost.ps1 -InstallOnly.');
  }
  fs.writeFileSync(LOG_TUNNEL, '', 'utf8');
  const tunnelLogStream = fs.createWriteStream(LOG_TUNNEL, { flags: 'a' });
  const child = spawn(CF_EXE, ['tunnel', '--url', 'http://127.0.0.1:8080', '--no-autoupdate'], {
    cwd: LOCAL_DATA,
    stdio: ['ignore', 'pipe', 'pipe'],
    detached: true,
    windowsHide: true
  });
  child.unref();
  managedTunnel = child;

  return new Promise((resolve, reject) => {
    let foundUrl = '';
    let settled = false;
    const timeout = setTimeout(() => {
      if (!settled) {
        settled = true;
        resolve(foundUrl);
      }
    }, 20000);

    const onChunk = (buf) => {
      const text = buf.toString('utf8');
      tunnelLogStream.write(text);
      const match = text.match(/https:\/\/[a-z0-9-]+\.trycloudflare\.com/i);
      if (match && !foundUrl) {
        foundUrl = match[0];
        writePublicUrlFile(foundUrl);
        if (!settled) {
          settled = true;
          clearTimeout(timeout);
          resolve(foundUrl);
        }
      }
    };

    child.stdout.on('data', onChunk);
    child.stderr.on('data', onChunk);
    child.on('exit', (code) => {
      tunnelLogStream.end();
      if (managedTunnel === child) managedTunnel = null;
      if (!settled) {
        settled = true;
        clearTimeout(timeout);
        reject(new Error(`cloudflared thoát sớm (mã ${code}).`));
      }
    });
  });
}

async function getFullStatus() {
  const [gw, nakama, cf, metrics] = await Promise.all([
    probeHttp('http://127.0.0.1:8080/healthcheck', 1000),
    probeHttp('http://127.0.0.1:7351/', 1000),
    probeHttp('http://127.0.0.1:20241/ready', 1000),
    queryProcessMetrics()
  ]);

  const lanIp = getLanIp();
  const keys = readJsonSafe(KEYS_FILE) || {};
  const tunnelRunning = cf.ok || metrics.components.cloudflared.count > 0;
  const serverRunning = gw.ok || metrics.components.node.count > 0;
  const publicBaseUrl = tunnelRunning ? readPublicUrlFile() : '';

  let mode = 'stopped';
  if (serverRunning && tunnelRunning) mode = 'public';
  else if (serverRunning) mode = 'lan';
  currentMode = mode;

  return {
    mode,
    busy: actionBusy,
    actionMessage,
    webglBuildReady: fs.existsSync(WEBGL_INDEX),
    services: {
      gateway: gw.ok,
      nakama: nakama.ok,
      postgres: metrics.components.postgres.count > 0,
      tunnel: tunnelRunning
    },
    urls: {
      consoleDomain: `http://khivan.localhost:${CONSOLE_PORT}/`,
      publicGame: publicBaseUrl ? `${publicBaseUrl}/game/` : '',
      publicBase: publicBaseUrl,
      lanGame: `http://${lanIp}:8080/game/`,
      localGame: 'http://127.0.0.1:8080/game/',
      nakamaConsole: 'http://127.0.0.1:7351/',
      nakamaUser: 'admin',
      nakamaPassword: keys.console || keys.consolePassword || ''
    },
    metrics,
    timestamp: new Date().toISOString()
  };
}

function sendJson(res, statusCode, obj) {
  const body = JSON.stringify(obj);
  res.writeHead(statusCode, {
    'Content-Type': 'application/json; charset=utf-8',
    'Cache-Control': 'no-store'
  });
  res.end(body);
}

function renderHtml() {
  return `<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<link rel="icon" type="image/png" href="/icon.png" />
<title>Khí Vận Chi Tử — Bảng Điều Khiển Máy Chủ</title>
<style>
  :root {
    --bg: #f6f3e8;
    --card: #fffdf7;
    --border: #dfd6be;
    --ink: #22302a;
    --muted: #607067;
    --jade: #1e7f5c;
    --jade-soft: #e6f4ee;
    --gold: #b8841b;
    --gold-soft: #fdf4dc;
    --danger: #b83a2d;
    --danger-soft: #fdebe9;
    --blue: #2364aa;
    --blue-soft: #e8f1fb;
  }
  * { box-sizing: border-box; }
  body {
    margin: 0;
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
    background: radial-gradient(circle at top right, #edf7f1 0%, var(--bg) 55%, #f2ecdc 100%);
    color: var(--ink);
    min-height: 100vh;
    padding: 20px;
  }
  .wrap { max-width: 1060px; margin: 0 auto; }
  header {
    display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between;
    gap: 12px; margin-bottom: 18px;
    background: var(--card); border: 1px solid var(--border); border-radius: 16px;
    padding: 16px 20px; box-shadow: 0 6px 20px rgba(34,48,42,0.06);
  }
  .brand h1 { margin: 0; font-size: 20px; color: var(--jade); letter-spacing: 0.2px; }
  .brand p { margin: 4px 0 0; font-size: 13px; color: var(--muted); }
  .badge {
    display: inline-flex; align-items: center; gap: 8px;
    padding: 8px 14px; border-radius: 999px; font-weight: 700; font-size: 13px;
    border: 1px solid transparent;
  }
  .badge.public { background: var(--jade-soft); color: var(--jade); border-color: #b7e2cf; }
  .badge.lan { background: var(--blue-soft); color: var(--blue); border-color: #bfd8f5; }
  .badge.stopped { background: var(--danger-soft); color: var(--danger); border-color: #f4c4be; }
  .dot { width: 10px; height: 10px; border-radius: 50%; background: currentColor; }

  .controls {
    display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 12px; margin-bottom: 18px;
  }
  button.act {
    border: none; border-radius: 14px; padding: 14px 16px;
    font-size: 15px; font-weight: 700; cursor: pointer;
    display: flex; flex-direction: column; align-items: flex-start; gap: 4px;
    transition: transform .12s ease, opacity .15s ease, box-shadow .15s ease;
    box-shadow: 0 4px 14px rgba(0,0,0,0.07);
  }
  button.act:hover:not(:disabled) { transform: translateY(-1px); }
  button.act:disabled { opacity: 0.55; cursor: wait; }
  button.act small { font-weight: 400; font-size: 12px; opacity: 0.9; }
  .btn-public { background: linear-gradient(135deg, #1e7f5c, #289b72); color: #fff; }
  .btn-lan { background: linear-gradient(135deg, #2364aa, #347ec9); color: #fff; }
  .btn-stop { background: linear-gradient(135deg, #b83a2d, #d14e40); color: #fff; }

  .grid { display: grid; grid-template-columns: 1.15fr 0.85fr; gap: 16px; margin-bottom: 16px; }
  @media (max-width: 820px) { .grid { grid-template-columns: 1fr; } }
  .card {
    background: var(--card); border: 1px solid var(--border); border-radius: 16px;
    padding: 16px 18px; box-shadow: 0 4px 16px rgba(34,48,42,0.05);
  }
  .card h2 {
    margin: 0 0 12px; font-size: 15px; color: var(--ink);
    display: flex; align-items: center; justify-content: space-between;
  }
  .link-row {
    display: flex; align-items: center; justify-content: space-between; gap: 10px;
    padding: 10px 12px; border-radius: 11px; border: 1px solid var(--border);
    background: #faf7ee; margin-bottom: 9px;
  }
  .link-row.featured { background: var(--gold-soft); border-color: #e6c675; }
  .link-meta { min-width: 0; flex: 1; }
  .link-meta .lbl { font-size: 12px; color: var(--muted); font-weight: 600; }
  .link-meta .val {
    font-family: Consolas, monospace; font-size: 13px; color: var(--ink);
    white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-top: 2px;
  }
  .mini-btns { display: flex; gap: 6px; flex-shrink: 0; }
  .mini-btn {
    border: 1px solid var(--border); background: #fff; color: var(--ink);
    border-radius: 8px; padding: 6px 10px; font-size: 12px; font-weight: 600;
    cursor: pointer; text-decoration: none;
  }
  .mini-btn:hover { background: var(--jade-soft); border-color: #9dd4be; color: var(--jade); }

  .pills { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 12px; }
  .pill {
    font-size: 12px; font-weight: 600; padding: 5px 10px; border-radius: 999px;
    border: 1px solid var(--border); background: #f4f1e6; color: var(--muted);
  }
  .pill.on { background: var(--jade-soft); color: var(--jade); border-color: #b7e2cf; }
  .pill.off { background: var(--danger-soft); color: var(--danger); border-color: #f4c4be; }

  table.metrics { width: 100%; border-collapse: collapse; font-size: 13px; }
  table.metrics th, table.metrics td {
    text-align: left; padding: 7px 6px; border-bottom: 1px solid #ede7d6;
  }
  table.metrics th { color: var(--muted); font-weight: 600; font-size: 12px; }
  .num { text-align: right !important; font-family: Consolas, monospace; }
  .ram-bar {
    height: 8px; border-radius: 99px; background: #e8e2d0; overflow: hidden; margin-top: 10px;
  }
  .ram-fill { height: 100%; background: linear-gradient(90deg, #1e7f5c, #d4a12a); width: 2%; transition: width .3s; }

  .tabs { display: flex; gap: 8px; margin-bottom: 10px; }
  .tab {
    border: 1px solid var(--border); background: #f6f2e4; color: var(--ink);
    padding: 6px 12px; border-radius: 8px; font-size: 12px; font-weight: 600; cursor: pointer;
  }
  .tab.active { background: var(--jade); color: #fff; border-color: var(--jade); }
  pre.logbox {
    margin: 0; background: #1d2623; color: #d8eee4; padding: 12px;
    border-radius: 10px; font-family: Consolas, monospace; font-size: 12px;
    height: 220px; overflow: auto; white-space: pre-wrap; word-break: break-word;
  }
  .toast {
    position: fixed; bottom: 18px; right: 18px; background: #1d2623; color: #fff;
    padding: 10px 16px; border-radius: 10px; font-size: 13px; opacity: 0;
    pointer-events: none; transition: opacity .2s; box-shadow: 0 6px 20px rgba(0,0,0,0.2);
  }
  .toast.show { opacity: 1; }
  .header-right { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
  .btn-power {
    border: 1px solid #e6b4ae; background: #fff5f4; color: var(--danger);
    border-radius: 999px; padding: 8px 14px; font-weight: 700; font-size: 12.5px;
    cursor: pointer; transition: background .15s, color .15s, transform .1s;
  }
  .btn-power:hover { background: var(--danger); color: #fff; border-color: var(--danger); }
  .btn-power:active { transform: scale(0.97); }
  .modal-bg {
    position: fixed; inset: 0; background: rgba(20, 32, 28, 0.45);
    display: none; align-items: center; justify-content: center; padding: 16px; z-index: 99;
  }
  .modal-bg.open { display: flex; }
  .modal-box {
    background: var(--card); border: 1px solid var(--border); border-radius: 16px;
    max-width: 460px; width: 100%; padding: 20px; box-shadow: 0 16px 40px rgba(0,0,0,0.22);
  }
  .modal-box h3 { margin: 0 0 8px; font-size: 17px; color: var(--ink); }
  .modal-box p { margin: 0 0 16px; font-size: 13.5px; color: var(--muted); line-height: 1.5; }
  .modal-actions { display: flex; flex-direction: column; gap: 8px; }
  .modal-btn {
    border: 1px solid var(--border); border-radius: 10px; padding: 10px 14px;
    font-size: 13.5px; font-weight: 700; cursor: pointer; text-align: center; background: #faf7ee; color: var(--ink);
  }
  .modal-btn.danger { background: var(--danger); color: #fff; border-color: var(--danger); }
  .modal-btn.jade { background: var(--jade-soft); color: var(--jade); border-color: #b7e2cf; }
  .modal-btn:hover { opacity: 0.92; }
  .shutdown-screen {
    position: fixed; inset: 0; background: var(--bg); z-index: 120;
    display: none; flex-direction: column; align-items: center; justify-content: center;
    text-align: center; padding: 24px;
  }
  .shutdown-screen.open { display: flex; }
  .notice { font-size: 12px; color: var(--muted); margin-top: 8px; line-height: 1.5; }
</style>
</head>
<body>
<div class="wrap">
  <header>
    <div class="brand">
      <h1>Khí Vận Chi Tử — Console Điều Khiển Máy Chủ</h1>
      <p>App Desktop độc lập · <strong>http://khivan.localhost:${CONSOLE_PORT}/</strong> (Tự bật tắt, không chạy qua Antigravity)</p>
    </div>
    <div class="header-right">
      <div id="statusBadge" class="badge stopped"><span class="dot"></span><span>Đang kiểm tra...</span></div>
      <button id="btnShutdownApp" class="btn-power" onclick="openShutdownDialog()">⏻ Tắt App Console</button>
    </div>
  </header>

  <div class="controls">
    <button id="btnPublic" class="act btn-public" onclick="doAction('start-public')">
      <span>▶ Bật Máy Chủ Public (Internet)</span>
      <small>Mở Local Server + Cloudflare HTTPS cho người mạng ngoài</small>
    </button>
    <button id="btnLan" class="act btn-lan" onclick="doAction('start-lan')">
      <span>📶 Bật Chỉ Mạng Wi-Fi (LAN)</span>
      <small>Chạy Node + Nakama + DB nội bộ, không mở đường hầm Internet</small>
    </button>
    <button id="btnStop" class="act btn-stop" onclick="doAction('stop')">
      <span>■ Tắt Toàn Bộ Máy Chủ</span>
      <small>Giải phóng toàn bộ RAM của Node, Nakama, Postgres & Tunnel</small>
    </button>
  </div>

  <div class="grid">
    <div class="card">
      <h2>Đường Dẫn Truy Cập & Quản Trị</h2>

      <div class="link-row featured">
        <div class="link-meta">
          <div class="lbl">Link Public Internet (Gửi bạn bè mạng khác / 4G)</div>
          <div class="val" id="urlPublic">Chưa bật Public Tunnel</div>
        </div>
        <div class="mini-btns">
          <button class="mini-btn" onclick="copyText(state.urls.publicGame)">Copy</button>
          <a class="mini-btn" id="openPublic" target="_blank" rel="noopener">Mở</a>
        </div>
      </div>

      <div class="link-row">
        <div class="link-meta">
          <div class="lbl">Link Wi-Fi LAN (Cùng mạng nhà)</div>
          <div class="val" id="urlLan">...</div>
        </div>
        <div class="mini-btns">
          <button class="mini-btn" onclick="copyText(state.urls.lanGame)">Copy</button>
          <a class="mini-btn" id="openLan" target="_blank" rel="noopener">Mở</a>
        </div>
      </div>

      <div class="link-row">
        <div class="link-meta">
          <div class="lbl">Link Máy Này (Localhost)</div>
          <div class="val" id="urlLocal">http://127.0.0.1:8080/game/</div>
        </div>
        <div class="mini-btns">
          <button class="mini-btn" onclick="copyText(state.urls.localGame)">Copy</button>
          <a class="mini-btn" id="openLocal" target="_blank" rel="noopener">Mở</a>
        </div>
      </div>

      <div class="link-row">
        <div class="link-meta">
          <div class="lbl">Nakama Admin Console (Tài khoản: admin · Xem User, BXH, Chat)</div>
          <div class="val" id="urlNakama">http://127.0.0.1:7351/</div>
        </div>
        <div class="mini-btns">
          <button class="mini-btn" onclick="copyText(state.urls.nakamaPassword, 'Đã copy mật khẩu admin Nakama!')">Copy Pass</button>
          <a class="mini-btn" href="http://127.0.0.1:7351/" target="_blank" rel="noopener">Mở</a>
        </div>
      </div>

      <div id="webglWarning" class="notice" style="display:none;background:#fdebe9;color:#b83a2d;padding:10px 12px;border-radius:10px;border:1px solid #f4c4be;margin-top:10px;">
        <strong>Chưa có bản build WebGL tại <code>Builds/LocalWeb/</code>:</strong> Máy chủ &amp; Console vẫn chạy bình thường, nhưng để chơi game ở đường dẫn <code>/game/</code>, hãy mở Unity và Build WebGL vào <code>Builds/LocalWeb/</code> (hoặc giải nén bản build vào đó).
      </div>
      <div class="notice">
        Mẹo: Khi bấm <strong>Bật Public</strong> lại từ đầu, Cloudflare Quick Tunnel miễn phí sẽ cấp một link <code>*.trycloudflare.com</code> mới. Giữ máy tính không Sleep để mọi người không bị ngắt kết nối.
      </div>
    </div>

    <div class="card">
      <h2>Tài Nguyên Máy Chủ Thực Tế <span id="tcpCount" style="font-size:12px;color:var(--jade)">0 kết nối</span></h2>
      <div class="pills">
        <span id="pGateway" class="pill off">Node Gateway :8080</span>
        <span id="pNakama" class="pill off">Nakama :7350</span>
        <span id="pPostgres" class="pill off">PostgreSQL :55432</span>
        <span id="pTunnel" class="pill off">Cloudflare Tunnel</span>
      </div>
      <table class="metrics">
        <thead>
          <tr><th>Thành phần</th><th class="num">Tiến trình</th><th class="num">RAM (MB)</th><th class="num">CPU tích lũy</th></tr>
        </thead>
        <tbody>
          <tr><td>Node.js (WebGL/Proxy)</td><td class="num" id="mNodeCnt">0</td><td class="num" id="mNodeRam">0</td><td class="num" id="mNodeCpu">0s</td></tr>
          <tr><td>Nakama (Online/BXH)</td><td class="num" id="mNakCnt">0</td><td class="num" id="mNakRam">0</td><td class="num" id="mNakCpu">0s</td></tr>
          <tr><td>PostgreSQL (Database)</td><td class="num" id="mPgCnt">0</td><td class="num" id="mPgRam">0</td><td class="num" id="mPgCpu">0s</td></tr>
          <tr><td>Cloudflare Tunnel</td><td class="num" id="mCfCnt">0</td><td class="num" id="mCfRam">0</td><td class="num" id="mCfCpu">0s</td></tr>
          <tr style="font-weight:700"><td>Tổng máy chủ</td><td class="num">—</td><td class="num" id="mTotRam">0 MB</td><td class="num" id="mHostRam">—</td></tr>
        </tbody>
      </table>
      <div class="ram-bar"><div id="ramFill" class="ram-fill"></div></div>
      <div class="notice" id="ramSummary">Đang đo tài nguyên...</div>
    </div>
  </div>

  <div class="card">
    <h2>Nhật Ký Hoạt Động Trực Tiếp (Live Logs)</h2>
    <div class="tabs">
      <button class="tab active" onclick="selectTab('serverOut', this)">Server Out</button>
      <button class="tab" onclick="selectTab('serverErr', this)">Server Error</button>
      <button class="tab" onclick="selectTab('tunnel', this)">Cloudflare Tunnel</button>
    </div>
    <pre id="logBox" class="logbox">Đang tải log...</pre>
  </div>
</div>

<div id="toast" class="toast"></div>

<div id="shutdownModal" class="modal-bg">
  <div class="modal-box">
    <h3>Tắt App Console Khí Vận?</h3>
    <p id="shutdownDesc">Bạn muốn tắt hẳn ứng dụng Console (giải phóng cổng 9090 và 100% RAM)?</p>
    <div class="modal-actions">
      <button id="btnShutdownAll" class="modal-btn danger" onclick="confirmShutdown(true)">⏻ Tắt Cả Máy Chủ Game &amp; Đóng App Console</button>
      <button id="btnShutdownOnly" class="modal-btn jade" onclick="confirmShutdown(false)">Chỉ Đóng App Console (Giữ Máy Chủ Chạy Nền)</button>
      <button class="modal-btn" onclick="closeShutdownDialog()">Hủy</button>
    </div>
  </div>
</div>

<div id="shutdownDone" class="shutdown-screen">
  <h2 style="color:var(--jade);margin:0 0 8px;">App Console Đã Tắt Hoàn Toàn</h2>
  <p id="shutdownDoneText" style="color:var(--muted);max-width:440px;line-height:1.5;">Tiến trình Console đã dừng sạch sẽ (0 MB RAM). Bạn có thể đóng cửa sổ này và mở lại bất cứ lúc nào từ icon trên Desktop.</p>
</div>

<script>
let state = { urls: {} };
let activeTab = 'serverOut';
let lastLogs = { serverOut: '', serverErr: '', tunnel: '' };

function showToast(msg) {
  const el = document.getElementById('toast');
  el.textContent = msg;
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 2400);
}

function copyText(text, customMsg) {
  if (!text) { showToast('Chưa có đường dẫn để copy!'); return; }
  navigator.clipboard.writeText(text).then(
    () => showToast(customMsg || 'Đã copy: ' + text),
    () => showToast('Không thể copy tự động')
  );
}

function selectTab(name, btn) {
  activeTab = name;
  document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
  btn.classList.add('active');
  document.getElementById('logBox').textContent = lastLogs[activeTab] || '(Trống)';
}

function setPill(id, on) {
  const el = document.getElementById(id);
  el.className = 'pill ' + (on ? 'on' : 'off');
}

async function refreshStatus() {
  try {
    const res = await fetch('/api/status', { cache: 'no-store' });
    const data = await res.json();
    state = data;

    const badge = document.getElementById('statusBadge');
    if (data.busy) {
      badge.className = 'badge lan';
      badge.innerHTML = '<span class="dot"></span><span>' + (data.actionMessage || 'Đang xử lý...') + '</span>';
    } else if (data.mode === 'public') {
      badge.className = 'badge public';
      badge.innerHTML = '<span class="dot"></span><span>ĐANG CHẠY PUBLIC (INTERNET)</span>';
    } else if (data.mode === 'lan') {
      badge.className = 'badge lan';
      badge.innerHTML = '<span class="dot"></span><span>ĐANG CHẠY CHỈ LAN</span>';
    } else {
      badge.className = 'badge stopped';
      badge.innerHTML = '<span class="dot"></span><span>MÁY CHỦ ĐANG TẮT</span>';
    }

    ['btnPublic', 'btnLan', 'btnStop'].forEach(id => {
      document.getElementById(id).disabled = !!data.busy;
    });

    document.getElementById('urlPublic').textContent = data.urls.publicGame || 'Chưa bật Public Tunnel';
    document.getElementById('openPublic').href = data.urls.publicGame || '#';
    document.getElementById('urlLan').textContent = data.urls.lanGame;
    document.getElementById('openLan').href = data.urls.lanGame;
    document.getElementById('urlLocal').textContent = data.urls.localGame;
    document.getElementById('openLocal').href = data.urls.localGame;
    document.getElementById('webglWarning').style.display = data.webglBuildReady ? 'none' : 'block';

    setPill('pGateway', data.services.gateway);
    setPill('pNakama', data.services.nakama);
    setPill('pPostgres', data.services.postgres);
    setPill('pTunnel', data.services.tunnel);

    const c = data.metrics.components;
    document.getElementById('mNodeCnt').textContent = c.node.count;
    document.getElementById('mNodeRam').textContent = c.node.ramMB.toFixed(1);
    document.getElementById('mNodeCpu').textContent = c.node.cpuSec.toFixed(1) + 's';

    document.getElementById('mNakCnt').textContent = c.nakama.count;
    document.getElementById('mNakRam').textContent = c.nakama.ramMB.toFixed(1);
    document.getElementById('mNakCpu').textContent = c.nakama.cpuSec.toFixed(1) + 's';

    document.getElementById('mPgCnt').textContent = c.postgres.count;
    document.getElementById('mPgRam').textContent = c.postgres.ramMB.toFixed(1);
    document.getElementById('mPgCpu').textContent = c.postgres.cpuSec.toFixed(1) + 's';

    document.getElementById('mCfCnt').textContent = c.cloudflared.count;
    document.getElementById('mCfRam').textContent = c.cloudflared.ramMB.toFixed(1);
    document.getElementById('mCfCpu').textContent = c.cloudflared.cpuSec.toFixed(1) + 's';

    const totMB = data.metrics.totalServerRamMB;
    const hostGB = data.metrics.hostRamTotalGB || 16;
    const pct = Math.min(100, (totMB / (hostGB * 1024)) * 100);
    document.getElementById('mTotRam').textContent = totMB.toFixed(1) + ' MB';
    document.getElementById('mHostRam').textContent = pct.toFixed(2) + '% RAM máy';
    document.getElementById('ramFill').style.width = Math.max(1, pct * 4) + '%';
    document.getElementById('tcpCount').textContent = data.metrics.tcp8080 + ' kết nối TCP :8080';
    document.getElementById('ramSummary').textContent =
      'Tổng RAM máy: ' + hostGB + ' GB (Đang trống ' + data.metrics.hostRamFreeGB + ' GB) · Máy chủ chiếm ' + totMB.toFixed(1) + ' MB (' + pct.toFixed(2) + '%).';
  } catch (e) {
    console.error(e);
  }
}

async function refreshLogs() {
  try {
    const res = await fetch('/api/logs', { cache: 'no-store' });
    lastLogs = await res.json();
    const box = document.getElementById('logBox');
    const atBottom = box.scrollHeight - box.scrollTop - box.clientHeight < 40;
    box.textContent = lastLogs[activeTab] || '(Trống)';
    if (atBottom) box.scrollTop = box.scrollHeight;
  } catch {}
}

async function doAction(action) {
  showToast('Đang thực thi lệnh: ' + action + '...');
  try {
    const res = await fetch('/api/' + action, { method: 'POST' });
    const data = await res.json();
    if (!res.ok) {
      showToast('Lỗi: ' + (data.error || res.status));
    } else {
      showToast(data.message || 'Hoàn tất!');
    }
  } catch (e) {
    showToast('Lỗi kết nối console: ' + e.message);
  }
  await refreshStatus();
  await refreshLogs();
}

let statusTimer = null;
let logsTimer = null;

function openShutdownDialog() {
  const running = state.mode === 'public' || state.mode === 'lan';
  document.getElementById('shutdownDesc').textContent = running
    ? 'Máy chủ game hiện đang chạy (' + (state.mode === 'public' ? 'Public Internet' : 'Wi-Fi LAN') + '). Bạn muốn dừng luôn máy chủ hay chỉ đóng App Console?'
    : 'Máy chủ game đang tắt. Xác nhận tắt hẳn tiến trình App Console để giải phóng cổng 9090 và toàn bộ RAM?';
  document.getElementById('btnShutdownOnly').style.display = running ? 'block' : 'none';
  document.getElementById('btnShutdownAll').textContent = running
    ? '⏻ Tắt Cả Máy Chủ Game & Đóng App Console'
    : '⏻ Tắt Hẳn App Console';
  document.getElementById('shutdownModal').classList.add('open');
}

function closeShutdownDialog() {
  document.getElementById('shutdownModal').classList.remove('open');
}

async function confirmShutdown(stopServer) {
  closeShutdownDialog();
  if (statusTimer) clearInterval(statusTimer);
  if (logsTimer) clearInterval(logsTimer);
  showToast(stopServer ? 'Đang tắt máy chủ & đóng App Console...' : 'Đang đóng App Console...');
  try {
    await fetch('/api/console/shutdown?stopServer=' + (stopServer ? '1' : '0'), { method: 'POST' });
  } catch {}
  document.getElementById('shutdownDoneText').textContent = stopServer
    ? 'Đã tắt toàn bộ Máy Chủ Game, Cloudflare Tunnel và App Console (0 MB RAM). Nhấn icon "Khí Vận Server Console" trên Desktop để mở lại khi cần.'
    : 'Đã tắt tiến trình App Console (máy chủ game vẫn đang chạy nền). Nhấn icon "Khí Vận Server Console" trên Desktop bất cứ lúc nào để quản lý lại.';
  document.getElementById('shutdownDone').classList.add('open');
  setTimeout(() => { try { window.close(); } catch {} }, 450);
}

refreshStatus();
refreshLogs();
statusTimer = setInterval(refreshStatus, 3000);
logsTimer = setInterval(refreshLogs, 4000);
</script>
</body>
</html>`;
}

function isLoopbackRequest(req) {
  const addr = req.socket.remoteAddress || '';
  const isLocalAddr = addr === '127.0.0.1' || addr === '::1' || addr === '::ffff:127.0.0.1';
  const host = (req.headers.host || '').toLowerCase();
  if (!isLocalAddr || !ALLOWED_HOSTS.has(host)) return false;

  if (req.method === 'POST') {
    const origin = (req.headers.origin || '').toLowerCase();
    if (origin) {
      try {
        const u = new URL(origin);
        if (!ALLOWED_HOSTS.has(u.host.toLowerCase())) return false;
      } catch {
        return false;
      }
    }
  }
  return true;
}

const server = http.createServer(async (req, res) => {
  if (!isLoopbackRequest(req)) {
    res.writeHead(403, { 'Content-Type': 'text/plain; charset=utf-8' });
    res.end('Forbidden: Local Control Console only accepts loopback requests.');
    return;
  }

  const url = new URL(req.url || '/', `http://${req.headers.host}`);

  if (req.method === 'GET' && url.pathname === '/') {
    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8', 'Cache-Control': 'no-store' });
    res.end(renderHtml());
    return;
  }

  if (req.method === 'GET' && (url.pathname === '/icon.png' || url.pathname === '/favicon.ico')) {
    if (fs.existsSync(ICON_PNG)) {
      res.writeHead(200, { 'Content-Type': 'image/png', 'Cache-Control': 'public, max-age=3600' });
      res.end(fs.readFileSync(ICON_PNG));
    } else {
      res.writeHead(204);
      res.end();
    }
    return;
  }

  if (req.method === 'POST' && url.pathname === '/api/console/shutdown') {
    const stopServer = url.searchParams.get('stopServer') !== '0';
    try {
      if (stopServer) {
        actionBusy = true;
        actionMessage = 'Đang tắt máy chủ & đóng App Console...';
        await stopAllGameServerProcesses();
      }
      sendJson(res, 200, { ok: true, stopServer, message: 'App Console đang tắt...' });
    } catch (e) {
      sendJson(res, 500, { error: e.message || String(e) });
    } finally {
      setTimeout(() => {
        try { server.close(); } catch {}
        process.exit(0);
      }, 250);
    }
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/status') {
    const status = await getFullStatus();
    sendJson(res, 200, status);
    return;
  }

  if (req.method === 'GET' && url.pathname === '/api/logs') {
    sendJson(res, 200, {
      serverOut: tailFile(LOG_SERVER_OUT, 60),
      serverErr: tailFile(LOG_SERVER_ERR, 60),
      tunnel: tailFile(LOG_TUNNEL, 60)
    });
    return;
  }

  if (req.method === 'POST' && (url.pathname === '/api/start-public' || url.pathname === '/api/start-lan' || url.pathname === '/api/stop')) {
    if (actionBusy) {
      sendJson(res, 409, { error: 'Đang có thao tác khác chạy, vui lòng chờ vài giây.' });
      return;
    }
    actionBusy = true;
    try {
      if (url.pathname === '/api/stop') {
        actionMessage = 'Đang tắt toàn bộ máy chủ...';
        await stopAllGameServerProcesses();
        sendJson(res, 200, { ok: true, message: 'Đã tắt toàn bộ máy chủ và đường hầm.' });
      } else if (url.pathname === '/api/start-lan') {
        actionMessage = 'Đang kiểm tra thư viện & khởi động chế độ chỉ LAN...';
        await stopAllGameServerProcesses();
        await ensureServerDependencies({ needTunnel: false });
        actionMessage = 'Đang khởi động Local Server (LAN)...';
        await startNodeServer(false);
        currentMode = 'lan';
        sendJson(res, 200, { ok: true, message: 'Đã bật máy chủ chế độ Wi-Fi LAN!' });
      } else if (url.pathname === '/api/start-public') {
        actionMessage = 'Đang kiểm tra thư viện & khởi động Local Server + Cloudflare...';
        await stopAllGameServerProcesses();
        await ensureServerDependencies({ needTunnel: true });
        actionMessage = 'Đang khởi động Local Server & Cloudflare Tunnel...';
        await startNodeServer(true);
        const pubUrl = await startCloudflareTunnel();
        currentMode = 'public';
        sendJson(res, 200, {
          ok: true,
          publicUrl: pubUrl ? `${pubUrl}/game/` : '',
          message: pubUrl ? `Đã bật Public! Link mới: ${pubUrl}/game/` : 'Đã bật Public, đang chờ đường hầm cấp link...'
        });
      }
    } catch (e) {
      sendJson(res, 500, { error: e.message || String(e) });
    } finally {
      actionBusy = false;
      actionMessage = '';
      lastStatsCache = { ts: 0, data: null };
    }
    return;
  }

  res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
  res.end('Not found');
});

server.listen(CONSOLE_PORT, CONSOLE_HOST, () => {
  console.log(`[Khivan Console] Ready at http://khivan.localhost:${CONSOLE_PORT}/ (and http://127.0.0.1:${CONSOLE_PORT}/)`);
});

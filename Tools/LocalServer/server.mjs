import { spawn } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { existsSync, createReadStream } from 'node:fs';
import { mkdir, readFile, stat, writeFile } from 'node:fs/promises';
import http from 'node:http';
import { networkInterfaces } from 'node:os';
import { extname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { setTimeout as delay } from 'node:timers/promises';
import EmbeddedPostgres from 'embedded-postgres';
import { build } from 'esbuild';
import proxyFactory from 'http-proxy-3';

const dir = fileURLToPath(new URL('.', import.meta.url));
const localData = join(process.env.LOCALAPPDATA, 'KhiVanChiTu', 'LocalServer');
const ports = { web: 8080, postgres: 55432, nakama: 7350 };
const backend = 'http://127.0.0.1:' + ports.nakama;
const ipv4 = Object.values(networkInterfaces()).flat()
  .filter(address => address.family === 'IPv4' && !address.internal);
const privateIp = /^(192\.168\.|10\.|172\.(1[6-9]|2[0-9]|3[01])\.)/;
const chosen = process.env.KHIVANTU_LAN_IP;
const lan = ipv4.find(address => address.address === chosen ||
  (!chosen && privateIp.test(address.address)));
if (chosen && !privateIp.test(lan?.address ?? '')) {
  throw new Error('KHIVANTU_LAN_IP must match a private IPv4 address on this PC.');
}
const interfaces = ['127.0.0.1', ...(lan ? [lan.address] : [])];
const allowedHosts = new Set([...interfaces, 'localhost'].map(ip => ip + ':' + ports.web));
const ipv4Number = address => address.split('.').reduce((value, octet) => value * 256 + Number(octet), 0);
const insideLan = ip => ip === '127.0.0.1' || (lan && privateIp.test(ip ?? '') &&
  (ipv4Number(ip) & ipv4Number(lan.netmask)) ===
  (ipv4Number(lan.address) & ipv4Number(lan.netmask)));

const pgDir = join(localData, 'postgres');
const nakamaDir = join(localData, 'nakama-data');
await mkdir(nakamaDir, { recursive: true });
const configPath = join(localData, 'keys.json');
const makeKey = () => randomBytes(32).toString('hex');
if (!existsSync(configPath)) {
  await writeFile(configPath, JSON.stringify({
    database: makeKey(), api: makeKey(), session: makeKey(),
    refresh: makeKey(), console: makeKey(), consoleSigning: makeKey(), runtime: makeKey()
  }), { flag: 'wx' });
}
const keys = JSON.parse(await readFile(configPath, 'utf8'));
const exe = join(localData, 'nakama.exe');
if (!existsSync(exe)) throw new Error('Run .\\Install-Nakama.ps1 before starting.');

const pg = new EmbeddedPostgres({
  databaseDir: pgDir, user: 'postgres', password: keys.database, port: ports.postgres,
  onLog: () => {}, onError: console.error
});
const databaseArgs = ['--database.address',
  'postgres:' + keys.database + '@127.0.0.1:' + ports.postgres + '/postgres'];
const servers = [];
const liveSockets = new Set();
let processNakama;
let pgStarted = false;
let stopping = false;

function done(child) {
  return new Promise((accept, reject) => {
    child.once('error', reject);
    child.once('close', code => code === 0 ? accept() : reject(new Error('Nakama exited: ' + code)));
  });
}

async function stop(code) {
  if (stopping) return;
  stopping = true;
  for (const socket of liveSockets) socket.destroy();
  for (const server of servers) server.close();
  if (processNakama && processNakama.exitCode === null) {
    const exited = new Promise(accept => processNakama.once('exit', accept));
    processNakama.kill();
    await Promise.race([exited, delay(3000)]);
  }
  if (pgStarted) await pg.stop();
  process.exit(code);
}

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => stop(0).catch(error => { console.error(error); process.exit(1); }));
}

function send(res, code, type, body) {
  res.writeHead(code, { 'Content-Type': type, 'Cache-Control': 'no-store' });
  res.end(body);
}

const contentTypes = {
  '.html': 'text/html; charset=utf-8', '.css': 'text/css; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8', '.json': 'application/json; charset=utf-8',
  '.wasm': 'application/wasm', '.data': 'application/octet-stream',
  '.unityweb': 'application/octet-stream',
  '.symbols': 'application/octet-stream', '.svg': 'image/svg+xml',
  '.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg',
  '.webp': 'image/webp', '.ico': 'image/x-icon', '.webmanifest': 'application/manifest+json; charset=utf-8',
  '.ttf': 'font/ttf', '.otf': 'font/otf'
};
const gameDir = resolve(dir, '../../Builds/LocalWeb');
const isLocal = req => insideLan(req.socket.remoteAddress) && allowedHosts.has(req.headers.host);
const proxy = proxyFactory.createProxyServer({ target: backend, ws: true });
proxy.on('error', (_error, _req, endpoint) => {
  if (endpoint instanceof http.ServerResponse) {
    send(endpoint, 502, 'text/plain; charset=utf-8', 'Nakama chưa sẵn sàng.');
  } else endpoint.destroy();
});

async function serveGame(path, res) {
  const file = resolve(gameDir, path.slice('/game/'.length) || 'index.html');
  const scoped = relative(gameDir, file);
  if (!scoped || scoped.startsWith('..') || resolve(gameDir, scoped) !== file) {
    return send(res, 404, 'text/plain', 'Not found');
  }
  const info = await stat(file).catch(() => null);
  if (!info?.isFile()) return send(res, 404, 'text/plain; charset=utf-8', 'Chưa có bản Unity Web tại Builds/LocalWeb.');
  const compression = extname(file);
  const encoding = { '.br': 'br', '.gz': 'gzip' }[compression];
  const type = contentTypes[encoding ? extname(file.slice(0, -compression.length)) : compression];
  res.writeHead(200, {
    'Content-Type': type ?? 'application/octet-stream',
    'Content-Length': info.size,
    'Cache-Control': 'no-cache',
    ...(encoding ? { 'Content-Encoding': encoding } : {})
  });
  createReadStream(file).on('error', () => res.destroy()).pipe(res);
}

try {
  const result = await build({ entryPoints: [join(dir, 'client.js')], bundle: true,
    write: false, minify: true, target: 'safari15', platform: 'browser' });
  const browserBundle = result.outputFiles[0].contents;
  const page = await readFile(join(dir, 'index.html'));
  if (!existsSync(join(pgDir, 'PG_VERSION'))) await pg.initialise();
  await pg.start();
  pgStarted = true;
  await done(spawn(exe, ['migrate', 'up', ...databaseArgs], { cwd: localData, stdio: 'inherit' }));
  processNakama = spawn(exe, [...databaseArgs, '--data_dir', nakamaDir,
    '--socket.address', '127.0.0.1', '--socket.port', String(ports.nakama), '--socket.server_key', keys.api,
    '--session.encryption_key', keys.session, '--session.refresh_encryption_key', keys.refresh,
    '--session.token_expiry_sec', '7200', '--console.address', '127.0.0.1',
    '--console.password', keys.console, '--console.signing_key', keys.consoleSigning,
    '--runtime.http_key', keys.runtime
  ], { cwd: localData, stdio: 'inherit' });
  processNakama.on('error', error => { console.error(error); void stop(1); });
  processNakama.on('exit', () => { if (!stopping) void stop(1); });

  let ready = false;
  for (let attempt = 0; attempt < 60 && !ready; attempt++) {
    ready = await fetch(backend + '/healthcheck').then(res => res.ok).catch(() => false);
    if (!ready) await delay(250);
  }
  if (!ready) throw new Error('Nakama health check failed.');

  for (const host of interfaces) {
    const server = http.createServer(async (req, res) => {
      if (!isLocal(req)) return send(res, 403, 'text/plain', 'LAN only');
      try {
        const path = decodeURIComponent(new URL(req.url, 'http://localhost').pathname);
        if (path === '/game' && req.method === 'GET') {
          res.writeHead(302, { Location: '/game/' });
          return res.end();
        }
        if (path === '/v2/account/authenticate/device' && req.method === 'POST') return proxy.web(req, res);
        if (path === '/v2/account' && (req.method === 'GET' || req.method === 'PUT')) return proxy.web(req, res);
        if (path.startsWith('/v2/channel/') && (req.method === 'GET' || req.method === 'POST')) return proxy.web(req, res);
        if (path.startsWith('/v2/leaderboard/') && (req.method === 'GET' || req.method === 'POST' || req.method === 'PUT')) return proxy.web(req, res);
        if (path.startsWith('/v2/friend') && (req.method === 'GET' || req.method === 'POST' || req.method === 'DELETE')) return proxy.web(req, res);
        if (path.startsWith('/v2/rpc/') && (req.method === 'GET' || req.method === 'POST')) return proxy.web(req, res);
        if (req.method !== 'GET') return send(res, 405, 'text/plain', 'Method not allowed');
        if (path === '/') return send(res, 200, contentTypes['.html'], page);
        if (path === '/client.js') return send(res, 200, contentTypes['.js'], browserBundle);
        if (path === '/api/config') return send(res, 200, contentTypes['.json'], JSON.stringify({ key: keys.api }));
        if (path === '/healthcheck') return proxy.web(req, res);
        if (path.startsWith('/game/')) return await serveGame(path, res);
        return send(res, 404, 'text/plain', 'Not found');
      } catch {
        return send(res, 400, 'text/plain', 'Invalid request');
      }
    });
    server.on('connection', socket => {
      liveSockets.add(socket);
      socket.on('close', () => liveSockets.delete(socket));
    });
    server.on('upgrade', (req, socket, head) => {
      if (isLocal(req) && req.url.startsWith('/ws?') && req.headers.origin === 'http://' + req.headers.host) {
        proxy.ws(req, socket, head);
      } else socket.destroy();
    });
    servers.push(server);
    await new Promise((accept, reject) => {
      server.once('error', reject);
      server.listen(ports.web, host, accept);
    });
    console.log('Local test: http://' + host + ':' + ports.web + '/');
  }
} catch (error) {
  console.error(error);
  await stop(1);
}

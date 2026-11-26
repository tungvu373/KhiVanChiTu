import { Client } from '@heroiclabs/nakama-js';

const connection = document.querySelector('#connection');
const join = document.querySelector('#join');
const form = document.querySelector('#chat');
const input = document.querySelector('#message');
const send = document.querySelector('#send');
const history = document.querySelector('#history');
const status = document.querySelector('#status');
const rendered = new Set();
let socket;
let channel;

function showMessage(message) {
  if (rendered.has(message.message_id)) return;
  let text;
  try {
    const content = typeof message.content === 'string'
      ? JSON.parse(message.content) : message.content;
    text = content?.text;
  } catch {
    return;
  }
  if (typeof text !== 'string') return;
  rendered.add(message.message_id);
  const row = document.createElement('li');
  const name = document.createElement('strong');
  name.textContent = message.username || 'Khách';
  row.append(name, document.createTextNode(text));
  history.append(row);
  if (history.children.length > 100) history.firstElementChild.remove();
  history.scrollTop = history.scrollHeight;
}

join.addEventListener('click', async () => {
  join.disabled = true;
  status.textContent = 'Đang kết nối…';
  try {
    const config = await fetch('/api/config');
    if (!config.ok) throw new Error('Không đọc được cấu hình.');
    const { key } = await config.json();
    let deviceId = localStorage.getItem('kvct-local-test-id');
    if (!deviceId) {
      deviceId = 'test-' + Array.from(crypto.getRandomValues(new Uint8Array(20)),
        byte => byte.toString(16).padStart(2, '0')).join('');
      localStorage.setItem('kvct-local-test-id', deviceId);
    }
    const client = new Client(key, location.hostname, location.port || '80', false);
    const session = await client.authenticateDevice(deviceId, true);
    socket = client.createSocket(false, false);
    socket.onchannelmessage = showMessage;
    socket.ondisconnect = () => {
      channel = undefined;
      connection.textContent = 'Mất kết nối';
      status.textContent = 'Bấm kết nối để vào lại phòng.';
      form.hidden = true;
      join.hidden = false;
      join.disabled = false;
    };
    await socket.connect(session, true);
    channel = await socket.joinChat('phong-thu-lan', 1, true, false);
    const recent = await client.listChannelMessages(session, channel.id, 25, false);
    for (const message of (recent.messages ?? []).reverse()) showMessage(message);
    if (!channel) throw new Error('Mất kết nối.');
    connection.textContent = 'Đã kết nối';
    status.textContent = 'Bạn đang dùng hồ sơ khách ' + session.username + '.';
    join.hidden = true;
    form.hidden = false;
  } catch {
    socket?.disconnect(false);
    join.disabled = false;
    status.textContent = 'Chưa vào được phòng. Hãy thử kết nối lại.';
  }
});

form.addEventListener('submit', async event => {
  event.preventDefault();
  const text = input.value.trim();
  if (!channel || !text || send.disabled) return;
  send.disabled = true;
  try {
    await socket.writeChatMessage(channel.id, { text });
    if (input.value.trim() === text) input.value = '';
  } catch {
    status.textContent = 'Gửi tin thất bại. Hãy thử lại.';
  } finally {
    send.disabled = false;
  }
});

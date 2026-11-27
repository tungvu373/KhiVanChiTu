# Máy chủ Local & Bảng Điều Khiển (`http://khivan.localhost:9090/`)

## Cách chạy nhanh sau khi `git pull` (Windows x64)

1. Cài **Node.js 22 trở lên** (nếu máy chưa có).
2. Mở file **`Open-ServerConsole.bat`** (double-click).
   - Trình duyệt sẽ tự mở trang **`http://khivan.localhost:9090/`**.
   - Lần đầu bấm **Bật Máy Chủ Public (Internet)** hoặc **Bật Chỉ Mạng Wi-Fi (LAN)**, Console sẽ tự động chạy `npm install`, tải `Nakama` và `cloudflared` về `%LOCALAPPDATA%\KhiVanChiTu\LocalServer\`.
3. Để chơi game tại `/game/`, hãy build bản WebGL từ Unity vào thư mục **`Builds/LocalWeb/`** ở gốc project (hoặc giải nén bản build vào đó).

---

## Chạy bằng dòng lệnh (tùy chọn)

```powershell
# Mở Console điều khiển (http://khivan.localhost:9090/)
npm run console

# Hoặc chạy trực tiếp chế độ chỉ LAN (cần npm ci && .\Install-Nakama.ps1 lần đầu)
npm start

# Hoặc chạy trực tiếp chế độ Public HTTPS Tunnel
npm run start:public
```

- Máy chủ web & WebSocket lắng nghe cổng `8080`. Nakama (`7350`/`7351`), PostgreSQL (`55432`) và Console (`9090`) chỉ lắng nghe trên loopback (`127.0.0.1`).
- Dữ liệu máy chủ (database, keys, logs) nằm trong `%LOCALAPPDATA%\KhiVanChiTu\LocalServer`.
- Nếu PC có nhiều card mạng, đặt `$env:KHIVANTU_LAN_IP` thành IPv4 riêng của card muốn dùng rồi khởi động lại.

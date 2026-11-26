# Máy chủ thử nghiệm LAN

Chạy các lệnh sau trong PowerShell tại thư mục này trên Windows x64 có Node.js 22 trở lên.

```powershell
npm ci
.\Install-Nakama.ps1
npm start
```

Mở `http://127.0.0.1:8080/` trên PC. Mở địa chỉ LAN mà lệnh in ra trên điện thoại cùng mạng. Bấm **Kết nối** trên cả hai thiết bị và gửi tin ở mỗi đầu. Mỗi trình duyệt dùng một tài khoản khách riêng. Nhấn Ctrl+C trong terminal máy chủ để dừng.

Máy chủ giới hạn cổng web và WebSocket trong mạng LAN của máy. Nakama, giao diện quản trị và PostgreSQL chỉ lắng nghe trên loopback. Dữ liệu thử nằm trong `%LOCALAPPDATA%\KhiVanChiTu\LocalServer`. Nếu PC có nhiều card mạng, đặt `$env:KHIVANTU_LAN_IP` thành IPv4 riêng của card muốn dùng rồi khởi động lại. Khi điện thoại không mở được, kiểm tra hai máy ở cùng mạng và Windows Firewall có cho phép Node.js nhận kết nối.

Bản Unity Web sau này đặt trực tiếp trong `Builds/LocalWeb` ở gốc project và mở tại `/game/`. Bản build cần Unity Web Build Support và tích hợp client Nakama riêng. Trang gốc chỉ dùng để kiểm tra kết nối LAN, tài khoản thử và chat.

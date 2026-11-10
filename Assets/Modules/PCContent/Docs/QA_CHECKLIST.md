**Bộ kiểm thử bàn giao PC và kiểm tra chung Android**

Mỗi báo cáo ghi commit, build ID, Unity, Windows/Android version, CPU/GPU/RAM hoặc model điện thoại và độ phân giải. Chạy bằng tài khoản/hồ sơ test; không dùng save production.

| Ca | Thao tác | Kết quả cần quan sát | Người xác nhận |
| --- | --- | --- | --- |
| Q01 | Clone sạch, LFS pull, import | Không missing asset/script, compile thành công | PC |
| Q02 | Mở PCGameplayWork | Có marker pc-workspace; không tự mở thêm scene gameplay | PC |
| Q03 | Tạo nhân vật, chơi, lưu, thoát và mở lại | Tiến trình PC được giữ; save gốc không đổi | PC + chủ dự án kiểm scope |
| Q04 | Ghép kiếm đủ/không đủ điều kiện, túi đầy | Không mất/nhân kiếm; phản hồi rõ | PC và Android |
| Q05 | Nâng cấp đủ/thiếu tiền | Không trừ sai, không kẹt UI | PC và Android |
| Q06 | Boss thắng, thua, hết giờ | Thưởng/tiến trình đúng, có đường chơi tiếp | PC và Android |
| Q07 | Mở/đóng drawer, modal, pause liên tiếp | Không click xuyên, không chồng lớp gây kẹt | PC và Android |
| Q08 | Chuột, kéo ghép, Escape, mất focus | Ghi hành vi hiện có và lỗi; không tự coi Escape đã được hỗ trợ | PC |
| Q09 | 1280×720, 1920×1080, resize | HUD đọc được, không cắt nút/text | PC |
| Q10 | ContentSandbox chạy nhiều vòng | Không hở chunk, không tăng object vô hạn, không có save gameplay | PC |
| Q11 | Phiên dài có boss/VFX | Ghi FPS/frame time/memory theo thời điểm, không chỉ ảnh tĩnh | PC và Android |
| Q12 | Background/resume, kill app, Back, tai thỏ | Tiến trình an toàn, lớp UI đúng | Chủ dự án Android |
| Q13 | Update từ bản test trước | Save còn đọc được; có cách phục hồi khi lỗi | PC và Android |
| Q14 | Mất mạng/retry/hai thiết bị nếu online đã tích hợp | Không nhận/tiêu trùng; tài khoản thật có state nhất quán | Chủ dự án + PC hỗ trợ |

Các dòng trên là ca phải thực hiện, không phải kết quả đã đạt. Xem HANDOFF_REPORT để biết những kiểm tra đã chạy trong lần tạo module.

```text
ID / mức độ P0-P1-P2:
Build / commit:
Thiết bị / OS / độ phân giải:
Profile test:
Bước tái hiện:
Expected:
Actual:
Tần suất:
Log / ảnh / video:
Đã thử lại ở build:
```

P0: crash/không khởi động/mất hoặc nhân dữ liệu. P1: chặn tiến trình/input chính/sai quy tắc nghiêm trọng. P2: hiển thị/âm thanh không chặn chơi. Không nhập dữ liệu người chơi thật vào log hay tài liệu Git.

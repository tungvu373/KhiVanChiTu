**Prompt bắt đầu cho Antigravity trên máy người cộng tác**

Sao chép đoạn dưới vào chat của Antigravity sau khi mở thư mục project gốc và kết nối Unity CLI/Pipeline:

```text
Tôi phụ trách PC/Windows, nội dung chặng đầu và QA của Khí Vận Chi Tử.
Chủ dự án phụ trách Android. Nhánh làm việc hiện tại: codex/pc-content-handoff.

Đọc AGENTS.md ở root, Assets/Modules/PCContent/AGENTS.md, README.md,
Docs/TASKS.md, Docs/CONTENT_CONTRACT.md và Docs/QA_CHECKLIST.md trong module.

Mọi lệnh unity chạy tại project root. Dùng unity status --format json trước;
chỉ điều khiển Editor đúng checkout qua Pipeline. Khám phá lệnh hiện có bằng
unity command, không đoán tên lệnh từ phiên bản cũ. Không mở Editor thứ hai trên
cùng checkout, không sửa Scene/Prefab YAML bằng text.

Scene của tôi:
Assets/Modules/PCContent/Scenes/PCGameplayWork.unity
Assets/Modules/PCContent/Scenes/ContentSandbox.unity

Giữ marker __PCWorkspace_SaveIsolation / pc-workspace; không dùng save Android.
Giữ nguyên Android scene/build list/PlayerSettings, Packages, core gameplay,
economy và backend. Phạm vi sửa chính là Assets/Modules/PCContent/**.

Bắt đầu PC-01: kiểm tra clone/LFS/compile, mở hai scene, chạy workspace validator,
Play và xác minh frame tiến triển, đọc console; báo cáo những gì thực sự đã kiểm.
Tiếp theo CONTENT-01: đề xuất một chủ đề và dựng hai chunk theo prefab mẫu;
commit nhỏ có .meta, ảnh/video và tiêu chí nghiệm thu. Không làm lại toàn bộ HUD
hoặc chạy builder ghi đè toàn scene. Dừng để phối hợp nếu phải sửa asset chung.
```

Khi giao task tiếp theo, thêm ID, đầu ra, giới hạn file, tiêu chí nghiệm thu và thời gian dự kiến. Chỉ yêu cầu AI sửa phạm vi đủ rõ; review diff trước khi gửi PR. Nếu terminal bị sandbox chặn localhost, cho phép riêng lệnh Unity/IPC cần thiết theo giao diện quyền của Antigravity.

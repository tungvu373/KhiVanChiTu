# Bàn giao ba bổ sung gameplay cho Antigravity IDE

Nhánh làm việc hiện tại là codex/android-release. Mục tiêu gồm Kiếm Xuyên/Phá Giáp, hộ thể boss tầng 10 và kỳ ngộ nhận ngay hoặc diệt 8 quái lấy đan. Người dùng đã cho phép triển khai. Phiên Codex không có API đọc phần trăm quota tài khoản. File này cập nhật theo từng mốc để có thể tiếp tục bất kỳ lúc nào.

## Mốc hiện tại

Đã triển khai GameplayChoicesService, OpeningBossGuard và hook vào DataManager, FlyingSword, BossEnemy, Enemy, EncounterManager, AlchemyService. Đã compile và chơi thử qua Editor trên bản sao scene có save profile gameplay-choices-qa. Kiếm pháp mở khi có kiếm cấp 2, đổi miễn phí. Thử thách lưu số quái còn lại. Đã bật menu chọn nhân vật có sẵn trong scene Android qua Editor để kích hoạt phiên save trước khi chơi.

Kết quả đã xác minh bằng runtime API/callback và ảnh chụp Unity:

- UI chọn hai kiếm pháp hiển thị đúng portrait; đổi style và load save giữ lựa chọn.
- Kiếm Xuyên gây khoảng 40.3466 / 20.1733 / 20.1733 damage lên ba mục tiêu bố trí phía sau nhau.
- Hộ thể boss tầng 10 từ 3564 xuống 1782 sau đòn 1782, HP vẫn 11880; combat có 6 đối tượng (boss và hộ tống sẵn có).
- Thử thách 8 kill tăng đan từ 1 lên 2; kill tiếp theo vẫn 2. Nạp save sau 7 kill giữ remaining=1.
- Nhận phôi ngay tăng tổng mảnh từ 12 lên 13, click resolve lần nữa vẫn 13.
- Khi đang chọn, CanSpawnEncounter=false; sau tử trận challenge remaining=0.
- Đã thêm SpawnVersion để reset chuỗi Phá Giáp khi Enemy được tái sử dụng từ pool. Cần kiểm lại build sau thay đổi này.
- Scene thử tạm được xóa qua AssetDatabase, Editor trở về SampleScene_MobileWork. Ảnh local ở Assets/Temp/gameplay-choices-style.png.

## Việc tiếp theo cho Antigravity IDE

Kiểm tra trạng thái compile cuối và git log trước khi tiếp tục. Ưu tiên đo cân bằng hai kiếm pháp trên thiết bị Android thật, kiểm nghiệm cả thắng/thua boss tầng 10, thao tác chạm/modal/Back và resume. Kiểm save cũ không có ba trường mới. Bộ kiểm tra hiện tại dùng Editor và callback có kiểm soát, chưa phải regression Android/Windows build đầy đủ. Chưa push gameplay lên GitHub tại thời điểm ghi file.

Prompt chuyển tiếp khi ứng dụng báo còn khoảng 10% hạn mức:

```text
Đọc Assets/.docs/ANTIGRAVITY_GAMEPLAY_HANDOFF.md và git diff/git log trên
nhánh codex/android-release. Tiếp tục kiểm thử và hoàn thiện ba bổ sung
gameplay đã triển khai theo các kết quả/mục còn lại trong file.
Dùng Unity Editor đang mở qua Pipeline, chạy CLI tại project root.
Tạo hồ sơ QA riêng khi thử save; chỉ commit các file thuộc gameplay này.
```

## Bắt đầu tiếp tục

Đọc AGENTS.md ở root và Assets/.agents/rules/rules.md. Chạy Unity CLI tại root KhiVanChiTu, kết nối Editor đang mở bằng Pipeline. Kiểm git status và diff trước khi sửa. Có thay đổi tài liệu của người dùng từ trước, chỉ stage file thuộc gameplay này.

1. Compile bằng unity recompile, kiểm console.
2. Chơi bằng bản sao scene và WorkspaceSaveProfile riêng để bảo vệ save gốc. Tạo/nạp nhân vật thật để isSessionActive=true.
3. Kiểm UI mở khóa kiếm pháp sau merge, đổi hai kiểu, save/reload.
4. Kiểm xuyên tối đa hai quái phía sau, streak Phá Giáp và shield boss tầng 10. Boss hiện đã có minion từ SpawnMinionsAheadOfBoss, tận dụng phần này.
5. Kiểm kỳ ngộ hai lựa chọn, click lặp không nhận hai lần, 8 kill nhận đan một lần, reload giữ tiến độ, tử trận hủy.
6. Kiểm UI safe area, modal/drawer không chồng, hiệu năng và pooling boss. Tối ưu FindObjectsByType trong mỗi hit nếu cần.
7. Lưu scene, đọc console cuối và cập nhật file này với kết quả thật. Chưa push/commit gameplay ở mốc này.

# Sửa vị trí chữ và nút đóng mobile — 14/11/2026

Đã áp dụng và lưu `Assets/Scenes/SampleScene_MobileWork.unity` bằng Unity Pipeline trong Editor 6000.6.4f1 đang mở.

## Thay đổi

- Căn lại hai dòng cảnh giới/tu vi và thanh phân kỳ trong khung HUD. Chiều cao dòng đủ để Unity không ẩn glyph khi dùng VerticalOverflow.Truncate.
- Căn giữa thông tin tầng trong khung; tách dòng khiêu chiến và đồng hồ boss để không đè nhau.
- Avatar lấy đúng CharacterName, kể cả tên hợp lệ `1`. Cập nhật khi tên thay đổi; tên dài tiếng Việt được xuống dòng trong vùng dành riêng.
- Xóa BtnDaoAn trong HeaderBar của CharacterDetailModal và bỏ mã tạo lại nút này trong DaoAnModalUI.
- Đồng bộ 15 nút đóng góc phải trên thành X đỏ (#FF5252), giữ callback và vùng bấm. Các nút tác vụ đóng ở chân panel giữ chức năng/nhãn hiện có.
- Thêm khoảng đệm chữ theo khung sliced; chừa vùng riêng cho tiêu đề và nút X. Sửa tiêu đề/phụ đề Đạo Vận và Bí Cảnh, thẻ nội dung, Thành Tựu, thông tin nhân vật và phần âm thanh trong bảng tạm dừng.
- Thêm khoảng đệm hai bên thanh MP, thanh tài nguyên và các bộ lọc kho đồ.
- Các sửa đổi runtime nằm trong luồng skin/repair hiện có để việc mở lại panel và đổi kích thước không ghi đè bố cục đã sửa.

## Kiểm tra

- Biên dịch C#: 0 lỗi, 0 cảnh báo biên dịch.
- Play Mode: **95/95 mục đạt, 687 assertion**. Báo cáo: `QA/runtime-checks.json`, tổng hợp `QA/final-summary.json`.
- Ảnh và geometry thực tế ở 720×1280 và 1080×2340: HUD thường/boss, avatar, Đạo Vận, Bí Cảnh, 5 drawer, kho đồ, Thành Tựu, Mốc, tạm dừng.
- Kiểm tra tên `1`, thay tên trực tiếp, tên dài tiếng Việt, nút avatar không tái tạo Đạo Ấn, bấm X đóng từng panel, EventSystem raycast và kích thước vùng bấm.
- Device Simulator Pixel 5: viewport 1080×2340, safe area 1080×2204; kiểm tra HUD và Đạo Vận. X nằm trong vùng an toàn.
- Cuối kiểm tra: Edit Mode, scene đã lưu, không còn marker QA; production PlayerPrefs không đổi. SHA-256 file save chính và .bak giữ nguyên (`QA/save-integrity.json`). Dữ liệu giả nằm trong profile `ui-content-repair-20261114-qa` riêng.
- Console cuối: 0 lỗi, còn 2 warning (`QA/final-console.json`). Đây là kiểm tra Editor/Simulator; chưa xác nhận bản APK trên Galaxy A12 cho lượt sửa UI này.

## Mã nguồn và khôi phục

Các phần chính: PortraitHudController, CultivationManager, DaoAnModalUI, JadeUIAdaptiveSkin, MobileUIResponsiveRepairs; bổ sung MobileUIContentSafety và MobileUIContentRepairTool. Bộ QA dùng lại hạ tầng Phase09 với mode content-repair và chọn đúng nhóm kích thước Game View khi Android đang là target.

`Before/` giữ bản trước khi sửa của scene và mã nguồn liên quan, bao gồm .meta. Không dùng Git restore rộng vì workspace đã có thay đổi của người dùng trước lượt này. Đối chiếu bản Before với file hiện tại trước khi khôi phục chọn lọc; không ghi đè các chỉnh sửa mới hơn.

Đối chiếu ID scene: không thêm ID; 12 ID bị xóa đều thuộc BtnDaoAn và Text con. Scene còn lại giữ ID (`QA/scene-identifiers.json`).

Để chạy lại: mở mobile scene đã lưu trong Edit Mode → `TuTien/UI/QA/Check framed text and close buttons`. Evidence và checkpoint phải có tại thư mục QA của báo cáo này. Menu `TuTien/UI/Repair framed text and close buttons` áp dụng lại phần sửa scene và hỗ trợ Undo.

# Phase 4 — Audit và bản sửa thực tế

Ngày 18/11/2026. Scene: Assets/Scenes/SampleScene_MobileWork.unity. Unity 6000.6.4f1, Pipeline 0.9.1-exp.1.

## Kết luận audit Antigravity

Script chưa commit ApplyUIFixesPhase4 sửa HUD, tên nhân vật, Đạo Ấn và nút đóng; không phải triển khai Equipment & Inventory theo prompt. Kho cũ chạm để phân giải, không có detail bottom sheet; bố cục nhỏ và nhiều lớp skin cũ vẫn chi phối. Khi mở drawer, Canvas có thể mất overrideSorting khiến navigation hub che mất giao diện. GridLayout có component bị disable, làm kiếm giãn hết khung.

## Đã thay đổi

- Kho bốn cột, ItemSlot/ItemCard prefab thật, ảnh kiếm hiện có và biểu tượng vector đan/dược liệu, tên dài ellipsis và tên đầy đủ trong chi tiết, số lượng phân cách hàng nghìn. Rarity giữ nguyên các ngưỡng cấp hiện có và PillQuality.
- Chi tiết bottom sheet: tên, hình, rarity/cấp/trạng thái, damage/speed/phôi kiếm hoặc mô tả tài nguyên/đan, phân giải qua xác nhận cũ. Trang bị/tháo kiếm gọi đường lệnh chuột phải hiện có. Không thêm trang bị Pháp Khí vì core không hỗ trợ tháo.
- Giữ năm danh mục, ba filter thu thập cũ, chuyển dạng ô/danh sách, sắp cấp kiếm tăng/giảm, phân trang 48 vật phẩm, nhận ưu tiên và tách cấp thấp. Ô trống có thông báo.
- Kiếm Lộ: chọn hai nguyên liệu từ detail, preview deterministic theo công thức hiện có, nút ghép chỉ bật khi hợp lệ, feedback thành công; vẫn giữ kéo thả, ghép nhanh, sắp kho, tạo kiếm, phân giải và pagination. Hàng thao tác ghép cố định dưới cùng.
- Ownership layout rõ ràng để skin cũ không sửa lại cây mới khi chưa active. Navigation tái xác nhận overrideSorting khi trình bày từng lớp.

## Kiểm thử

Evidence/checks.json: 42 kiểm tra trên profile riêng equipment-cultivation-redesign-qa; 1080×1920, 1080×2340, 720×1280. Chọn item, Back chi tiết trước kho, category filter, list/grid, scroll, mở/đóng, equip/unequip cảm ứng, merge thật, thiếu nguyên liệu, kho hòm tạm trống, nâng cấp cũ thiếu tài nguyên. Kiểm tra drawer thực sự render trên hub và primary merge có hit area tối thiểu 120 reference units. SaveToSlot/LoadFromSlot round trip và DataManager ghi/đọc lại inventory profile QA khớp.

Evidence/console-errors.json rỗng trong lần chạy cuối. Evidence/save-preservation.json xác nhận PlayerPrefs, JSON và backup production không đổi. Compilation cuối không có lỗi mới; project còn warning API obsolete có sẵn. Các file previous-failure là lịch sử lần QA phát hiện lỗi, không phải kết quả cuối.

Chưa kiểm trên điện thoại Android thật/Device Simulator notch; không khẳng định đã kiểm các thiết bị này. SafeAreaFitter tiếp tục xử lý safe area ở screen và bottom sheet.

Không sửa Item Database, service formula, drop rate, upgrade cost, persistence schema hoặc equipment stats. Các file gameplay và UI khác dirty từ trước vẫn giữ nguyên ngoài commit. Scene được stage bằng Git three-way từ snapshot trước thao tác để tách thay đổi người dùng có sẵn; scene trên đĩa luôn do Editor lưu.

Commit dependency riêng đưa Design System/HUD/TMP đã hoàn thành nhưng chưa được theo dõi vào Git; không đổi code của các phase đó.

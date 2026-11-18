# Sword level cap correction — 18/11/2026

Chủ dự án xác nhận cấp kiếm tối đa hiện tại là **10**. Thay đổi này được thực hiện riêng sau Phase 6; không bắt đầu phase tiếp theo.

## Quy tắc và sửa đổi

- `SwordRules.MaxLevel = 10` là nguồn giới hạn chung cho ghép tay, ghép nhanh, preview và thông tin vật phẩm. Phôi cần: cấp 1–2 dùng 2; cấp 3–9 dùng số phôi bằng cấp hiện tại. Cấp 10 dừng ghép. Quy luật ghép cũ ở cấp 1–7 không đổi; mở tiếp 8 → 9 và 9 → 10.
- Ghép nhanh mở rộng mảng đếm và vòng lặp tới cấp 10, tránh kẹp kiếm cấp 9–10 xuống cấp 8. Intake kho/hòm tạm cũng giữ đúng cấp 9–10.
- Chi tiết hiển thị cấp tối đa 10 và vô hiệu hóa chọn ghép kiếm cấp 10. Mốc `sword_lvl_10` giữ nguyên điều kiện/thưởng và nay có thể đạt.
- Công thức damage/speed, rarity, nguồn rơi, số ô xuất chiến, save schema, hành động Tế Kiếm cấp 8 và bảng phân giải hiện tại không đổi. Bảng giá phân giải vẫn chặn ở hàng giá cấp 8; cấp 9–10 nhận cùng hàng giá đó như hành vi trước sửa, chưa có giá mới được tự đặt.
- GDD, hướng dẫn AGENTS và công cụ audit endgame đã cập nhật để không tiếp tục gọi cấp 8 là giới hạn kiếm.

## Hình ảnh thực tế

Giao diện đã có hình ảnh, nhưng chưa có một bộ tranh minh họa đầy đủ theo tiêu chuẩn mỹ thuật RPG cao cấp:

| Khu vực | Hiện đang dùng |
| --- | --- |
| Kiếm, chi tiết kiếm, ghép | Sprite kiếm có sẵn từ atlas `Assets/Import/Asset/kiem.png`. Prefab đang gán 8 sprite; cấp 9–10 dùng lại sprite cuối qua cơ chế fallback hiện có. Chưa có ảnh riêng cấp 9–10. |
| Luyện Đan | Ảnh lò `Assets/Art/UI/Generated/JadeV5_AlchemyDecoration.png`. |
| Luyện Thể | Icon vector hình cơ thể, không phải tranh nhân vật minh họa. |
| Pháp Khí | Icon vector áo, mũ, giày, ngọc bội; chưa có tranh từng pháp khí. |
| Trận Pháp | Icon vector trận; chưa có tranh riêng cho từng trận trong view mới. |
| Kho: đan và dược liệu | Icon vector viên đan, thảo dược cùng các sprite đang có; không phải tranh vật phẩm chi tiết đầy đủ. |
| Mốc Tu Tiên / Thiên Phú | `NavigationIcon` vẽ vector theo nhóm/nhánh trên các thẻ; chưa có illustration riêng cho mỗi thiên phú. |

Vì vậy không nên coi các phase UI đã hoàn thành là đã hoàn thành toàn bộ phần mỹ thuật minh họa. Lần sửa giới hạn này không tạo hay tuyên bố có thêm tranh mới.

## Kiểm thử

`SwordCapQA` chạy Play Mode qua Pipeline, profile riêng `sword-cap10-qa`. Lần cuối có **225 checks**, không có lỗi QA; bao gồm thao tác thêm kiếm khi lấp đầy kho để kiểm tra hòm tạm.

- Game loop thực sự tiến frame; kiểm quy luật ở cả 10 cấp.
- Ghép tay 8 → 9, 9 → 10; chặn 10 → 11.
- Ghép nhanh chuỗi 8 → 10 và nâng kiếm xuất chiến; giữ nguyên stack kiếm cấp 10 đã có.
- Nút workbench thực hiện ghép cấp 9, preview cấp 10; nút ghép khóa ở cấp tối đa. Detail cấp 8 vẫn có thể nâng; cấp 10 hiển thị tối đa.
- Điều kiện mốc kiếm cấp 10 đạt và thẻ Phase 6 chuyển sang Claimable.
- Kho nhận đúng cấp 9–10; hòm tạm giữ cấp 10 khi kho đầy.
- Save xuống đĩa qua DataManager và Load lại giữ cấp/stack/trang bị; không đổi schema. Pause ownership giữ nguyên.
- Hash production PlayerPrefs, JSON và backup không đổi; scene sau cleanup giống byte-for-byte bản trước QA. Không có sửa layout scene/prefab.

Evidence: [checks](checks.json), [bảo toàn save](save-preservation.json). Biên dịch thành công. Đây là kiểm thử Editor; không phải kiểm Android vật lý.

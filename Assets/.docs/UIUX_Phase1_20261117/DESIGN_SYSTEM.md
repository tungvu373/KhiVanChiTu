# Phase 1 — Ngọc Trầm / Vàng Cổ

Unity 6000.6.4f1 · Android portrait · Reference canvas 1080×1920.

Foundation nằm riêng tại Assets/UI/Foundation. Theme hiện có của gameplay chưa được thay thế. Bản này hiện thực design system dành cho các phase tiếp theo, không phải redesign HUD/gameplay.

## Màu sắc và vai trò

| Token | Hex | Sử dụng |
|---|---|---|
| Ink | #10282A | Nền sâu, chữ trên CTA vàng |
| Surface | #163C3C | Panel chính, resource/stat rows |
| Raised | #1E4C48 | Button phụ, toast/tooltip |
| Gold | #C9A76A | CTA chính, lựa chọn, giá trị quan trọng |
| Ivory | #F3EBD8 | Text chính |
| Muted | #AFBDB3 | Mô tả, thông tin phụ |
| Jade | #69CAA2 | Điểm nhấn, trạng thái tích cực |
| Progress fill | #28634F | Nền progress đậm để giữ nhãn ivory dễ đọc |
| Danger | #D85F67 | Nguy hiểm/lỗi; không dùng thay cho nhãn xác nhận |

Chỉ một CTA vàng trong một cụm hành động. Card thường có nền ngọc, khoảng trống và điểm nhấn mảnh; không dùng khung hoa văn nặng cho từng dòng. Backdrop tối là lớp chặn input, không phải chỉ một Image trang trí.

Tỷ lệ tương phản tính từ token sRGB: ivory/surface 10,13:1; ivory/raised 8,10:1; ink/gold 6,78:1; ivory/progressFill 5,92:1; muted/ink 7,91:1. Đây là kiểm tra màu chuẩn, không thay thế đánh giá độ sáng/màu trên màn điện thoại.

## Typography và tiếng Việt

| Vai trò | Reference px | Font |
|---|---:|---|
| Screen title | 52 | Source Sans Pro Semibold SDF |
| Heading/item title | 38 | Source Sans Pro Regular SDF; có thể dùng Semibold từ theme |
| Body/action/value | 32 | Source Sans Pro Regular SDF |
| Caption/meta | 28 | Source Sans Pro Regular SDF |

Hai font SDF được tạo từ font đã có trong project, atlas 2048², SDFAA, padding 8, sampling 72, dynamic/multi-atlas. Builder prewarm ASCII, các chữ ă/â/đ/ê/ô/ơ/ư hoa-thường, Latin có dấu và toàn bộ U+1EA0–U+1EF9; TryAddCharacters phải không thiếu glyph. Không đổi serialized UnityEngine.UI.Text cũ thành TMP.

Không bật auto-size để nhồi nội dung. Text mô tả dài ở dialog nằm trong ScrollRect có content theo preferred height; footer nằm ngoài scroll. Rich text mặc định tắt để tên nhân vật không bị hiểu thành markup. Tên item ngắn dùng truncate có chủ đích; chi tiết đầy đủ có thể được adapter đưa vào sheet/tooltip.

## Spacing, shape và icon

| Token | Reference units |
|---|---:|
| Screen margin | 36 |
| Panel/content padding | 32 |
| Component gap | 24 |
| Button height | 132 |
| Compact caption inset | 16–24 |
| Surface sprite | 64×64; border slice 14 mỗi cạnh |

Surface9Slice.png là sprite hình học nhỏ được tạo bằng Texture2D trong Editor, không chứa chữ hoặc art gameplay. Các góc tròn được giữ nguyên bằng Image.Type.Sliced; vùng giữa co giãn. Độ dày nội dung không phụ thuộc viền trang trí. Icon dùng art hiện có, preserveAspect, không kéo dài; chọn một màu vật liệu và một trọng tâm trên mỗi control. Sword sample dùng sprite kiem_2, không dùng icon linh thạch làm kiếm.

## Canvas và Safe Area

- ScaleWithScreenSize; reference 1080×1920; matchWidthOrHeight=0 để giữ chiều rộng logical ổn định trên portrait dài. 720×1280 có scale 2/3; 1080×2340 tăng chiều cao nội dung.
- Canvas background full viewport; SafeArea root dùng normalized Screen.safeArea; layers và nội dung nằm bên trong. Root cập nhật khi resolution/insets thay đổi.
- Controls dùng anchors/layout group. Gallery là ScrollRect + RectMask2D + content VLG/ContentSizeFitter; không đặt fitter trên các child do parent kiểm soát height.
- 132 reference units không có nghĩa 132dp. Physical touch target/notch/Android Back thực tế vẫn cần QA trên thiết bị.
- Không đổi PlayerSettings/orientation/build profile toàn project trong phase này: các màn hiện tại chưa được migrate.

## Layers và input

| Layer | Sorting order | Ownership |
|---|---:|---|
| GameplayHUD | 100 | View HUD trong foundation |
| Navigation | 110 | Header/footer hoặc nav sau này |
| ScreenOverlay | 200 | FullScreenPanel |
| Modal | 300 | BottomSheet, RewardModal, ConfirmDialog |
| Notification | 400 | Tooltip, Toast, không nhận raycast |

FoundationRoot theo dõi panel đang mở, chọn panel trên cùng theo sorting/sibling order, chặn HUD/navigation khi có panel. Back đóng panel trên cùng, không gọi confirm. ConfirmDialog không dismiss bằng backdrop; Close/Back có thể hủy. Không đặt timeScale hoặc gọi service gameplay. FoundationRoot chỉ được thêm vào root mới, chưa đặt cạnh MobileUINavigationManager của gameplay để tránh hai owner xử lý Escape.

## Trạng thái và feedback

Primary/secondary có pressed/highlight/disabled transition 120ms. Tab chọn dùng nền vàng/chữ ink, tab khác dùng raised/ivory. Item chọn có vạch vàng. Progress clamp 0–1 và xử lý denominator 0/NaN/Infinity. Toast dùng WaitForSecondsRealtime; tooltip hiện khi hover/chạm giữ và ẩn khi rời/thả. Callback nguy hiểm chỉ chạy sau confirm; một lần xác nhận tương ứng một callback.

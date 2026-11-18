# KHÍ VẬN CHI TỬ — UI/UX REDESIGN PLAN

**Cơ sở đánh giá:** video Unity Editor dài 167,26 giây, Game View 1080×1920 ở Scale 0.5, quay ngày 17/11/2026; kiểm tra timeline từ 00:00 tới 02:47, trích frame mỗi 2 giây và xem chi tiết vùng màn hình game.

**Phạm vi:** thiết kế lại UI/UX/HUD cho Android mobile 9:16 theo phong cách tu tiên xanh ngọc–vàng cổ; **bảo toàn gameplay, dữ liệu, tiến trình, công thức và các nút chức năng hiện có**. Video cho phép đánh giá hình ảnh/luồng thao tác đã hiển thị, chưa thể xác thực code nội bộ, tương tác cảm ứng trên máy thật hay mọi màn hình chưa quay.

## A. Evidence map — màn hình quan sát được

| Mốc | Giao diện | Nhận xét có cơ sở từ video |
|---|---|---|
| 00:00–00:03 | Menu chính | Tranh nền chủ đề tu tiên đẹp; 3 CTA rõ; nhưng phần giữa-dưới nhiều vùng trống |
| 00:04–00:07 | Chọn nhân vật / xác nhận | 3 hàng dùng avatar rất giống nhau; nút X dễ bị hiểu nhầm và chạm nhầm; nhiều không gian chưa sử dụng |
| 00:08–00:14 | Báo cáo offline + gameplay HUD | Modal báo cáo, 3–4 tầng trạng thái trên, kỹ năng + hai hàng navigation dưới; HP/MP nhỏ so với các dải thông tin khác |
| 00:14–00:35 | Kiếm Lộ | Danh sách ô 5 cột nhỏ, phân trang; trạng thái Khóa; cụm lọc/sắp xếp; nút nghiệp vụ không có vị trí cố định rõ |
| 00:36–00:44 | Luyện Đan | Khung tiêu đề/trạng thái/lò trống, hai công thức song song; nhiều đoạn văn nhỏ và nút cùng mức nhấn |
| 00:46–00:54 | Pháp Khí, Trận Pháp | Các ô thông số và thẻ hiệu ứng dày đặc; tốn diện tích cho viền/tên/thông số lặp |
| 00:56–01:13 | Mốc Tu Tiên, Thiên Phú Đạo Vận | Nhiều thẻ nhiệm vụ/thiên phú xếp dọc; thiếu chỉ báo mục khuyến nghị hoặc ưu tiên có thể thực hiện |
| 01:14–02:00 | Bí Cảnh, minigames và thưởng | Hình ảnh sự kiện khá mạnh nhưng style HUD lệch: panel xanh-xám phẳng, thanh tiến độ xanh neon, CTA phía dưới đôi lúc không có nhãn rõ; nhiều chữ hướng dẫn cùng lúc |
| 02:02–02:10 | Kho Đồ | Bộ lọc, nguyên liệu, chỉ số, 4 cột ô item, hành động hàng loạt và dock nền chiếm gần toàn trang |
| 02:12–02:18 | Pause / Settings | Theme chung ổn nhưng slider nền/vạch màu vàng chưa cùng phong cách; menu có 4 nút dọc và nhiều khoảng cách không cân đối |
| 02:20–02:28 | Toggle HUD | Có chế độ giảm HUD, chứng tỏ nhu cầu không gian trận đấu; trạng thái thu gọn thiếu cấu trúc hiển thị ưu tiên rõ |
| 02:30–02:47 | Đổi nhân vật, luyện thể, trở về menu | Chuyển chế độ/nhân vật hoạt động; panel Luyện Thể có bảng nhiều chỉ số; cần chuẩn hóa |

## B. Chẩn đoán theo mức độ ưu tiên

### P0 — cản trở việc đọc và chơi
1. **Vùng chiến đấu không đủ ưu tiên thị giác.** Trên gameplay HUD, thanh tiến trình/quest/phụ trạng thái chiếm một phần lớn ở trên; nút kỹ năng + 2 dải chức năng chiếm dưới. Quái, nhân vật, cơ chế đang bị UI cạnh tranh không gian.
2. **Mật độ thông tin quá cao.** Có nhiều tên dài, hệ số, timer, progress, giá tài nguyên cùng cỡ và màu. Người dùng không biết nên nhìn vào đâu đầu tiên.
3. **Bí Cảnh thiếu tính thống nhất và rõ chức năng.** Nút có viền nhưng không thấy chữ ở đáy các minigame, progress bar màu xanh neon, panel mô tả dày đặc khác hẳn game chính.
4. **Vùng chạm có nguy cơ quá nhỏ.** Ô vật phẩm, tab và nút đóng rất nhỏ trên bản ghi; cần xác nhận bằng kiểm thử trên máy thật.

### P1 — tăng thời gian thao tác và tải nhận thức
5. **Điều hướng hai tầng không rõ hệ phân cấp.** Hàng Kho/Bí Cảnh/Vận Khí/Mốc nằm trên 5 tab Kiếm Lộ/Luyện Thể/Luyện Đan/Pháp Khí/Trận Pháp; quá nhiều đích điều hướng cạnh tranh nhau.
6. **Panel dạng cửa sổ nổi không thích hợp mobile.** Modal nội dung trong nửa dưới bị nén, dữ liệu cần cuộn, trong khi HUD chính vẫn lộ phía sau.
7. **Thiếu một CTA chính duy nhất ở nhiều màn hình.** Ví dụ upgrade/tu luyện/kích hoạt/nhận/sắp xếp nằm ngang hàng về trọng lượng thị giác.
8. **Trạng thái UI chưa mã hóa nhất quán.** Đã nhận / chưa đạt / có thể mua / đang dùng / khóa đều nằm trong card trang trí giống nhau.

### P2 — làm giảm cảm nhận chất lượng
9. Chưa thống nhất typography, spacing, icon, bảng màu; phần game world và UI panel có nhiều phong cách khác nhau.
10. Damage text quá to và chồng nhau trong nhiều pha chiến đấu, làm che chủ thể.
11. Các hàng thông tin hình như có thể thay đổi chiều cao/nội dung tùy trạng thái; cần đảm bảo không nhảy layout khi tiến trình thay đổi.

## C. Thiết kế đích

### C1. Information architecture đề xuất
- **Bottom navigation (5 điểm):** `Chiến đấu` · `Trang bị` · `Tu luyện` · `Bí cảnh` · `Khác`.
- `Trang bị`: Kiếm Lộ, Kho Đồ, phân giải, lọc/sắp xếp, thông tin vật phẩm.
- `Tu luyện`: Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp; mỗi hệ thống là một tab cấp hai có ngữ cảnh.
- `Bí cảnh`: Danh sách thử thách, phần thưởng, độ khó, vé/phí, lịch sử hoặc trạng thái.
- `Khác`: Đạo Vận, Mốc Tu Tiên, các chức năng phụ, Cài đặt.
- Mọi entry point và nghiệp vụ cũ phải **vẫn truy cập được trong tối đa hai lần chạm từ màn hình chính** (trừ luồng xác nhận có chủ đích). Không tự xóa chức năng chỉ vì không còn chỗ trong thanh nav.

### C2. Gameplay HUD — cấu trúc 3 vùng
- **Top (khoảng 12–17% chiều cao hữu dụng):** Avatar + cảnh giới/tầng + 2 loại tiền, dấu + tùy chọn. Một mục tiêu nhiệm vụ có thể thu gọn (chỉ 1–2 dòng). Boss/timer dạng banner trạng thái chỉ xuất hiện khi cần.
- **Center (ưu tiên 58–65%):** Nhân vật/quái/đường đi/kết quả hit. Damage popups gộp, phân tầng, bớt che mô hình. Nút sự kiện tạm thời né vị trí quan trọng.
- **Bottom (khoảng 18–24%):** 2 thanh HP/MP rõ ràng + 4 skill buttons biểu tượng, cooldown radial, chi phí/tín hiệu hết MP + dock 5 đích khi người dùng mở UI. Trong trạng thái compact, ưu tiên HP/MP và 1–2 quick action nếu vẫn cần chơi.
- Trạng thái `Full HUD` và `Compact HUD` phải có quy tắc nhất quán, tránh nhảy panel và không giấu tài nguyên sống còn một cách bất ngờ.

### C3. Visual design system (bản đề xuất)
- Nền sâu: `#10282A`; bề mặt: `#163C3C`; vàng viền: `#C9A76A`; chữ chính: `#F3EBD8`; chữ phụ: `#AFBDB3`; trạng thái hoàn thành: `#69CAA2`; cảnh báo: `#E7AF5F`; lỗi: `#D85F67`.
- Chỉ giữ viền hoa văn nặng ở title, hero item, confirmation; card thường dùng viền nhẹ + 9-slice. Tránh đóng khung vàng cho mọi item nhỏ.
- **Typography 3–4 cấp rõ:** title, heading, body, caption; font TMP có đủ dấu tiếng Việt, không dùng bé quá mức để nhồi nội dung.
- Component: `ResourcePill`, `PrimaryButton`, `SecondaryButton`, `IconButton`, `TabBar`, `StatRow`, `ProgressBar`, `ItemSlot`, `ItemCard`, `ScreenHeader`, `BottomSheet`, `FullScreenPanel`, `RewardModal`, `Toast`.
- Nút màu vàng/teal là hành động chính; secondary tối; không dùng toàn bộ nút như CTA chính.
- Touch target mục tiêu **>= 44–48dp vật lý tương đương** khi xuất hiện trên thiết bị; kiểm bằng máy thật, không đánh giá từ ảnh Game View Scale 0.5.

### C4. Cấu trúc panel
- `FullScreenPanel`: kho, kiếm lộ, thiên phú, hệ thống progression phức tạp, danh sách Bí Cảnh.
- `BottomSheet` chiếm khoảng 50–75%: chi tiết item, thao tác nâng cấp nhanh, thông báo kết quả ngắn; có overlay tối phía sau, điểm đóng rõ.
- `Modal` xác nhận/nguy hiểm: xóa nhân vật, rời trận, sử dụng tài nguyên hiếm, quay về menu.
- Không để hai tầng bottom nav hiển thị cùng lúc với full panel. Không để UI từ nền nhận click khi modal đang hoạt động.

## D. Kế hoạch triển khai theo phase

### Phase 0 — Inventory chức năng và snapshot (CHƯA sửa UI)
**Mục tiêu:** lập danh sách đầy đủ prefab, Canvas, scene, button/OnClick, TMP, trạng thái mở/tắt, input, dữ liệu binding, script liên quan. Chụp baseline bằng Unity tại 1080×1920, 1080×2340 và 720×1280; ghi lại FPS/GC baseline. Viết `UI_FEATURE_MAP.md`, `UI_SCREEN_MAP.md`, `UI_BASELINE.md`. Ghi commit Git.
**Gate:** có danh sách chức năng kèm đường đến UI, đầy đủ theo màn hình đã quay; chưa thay đổi logic gameplay.

### Phase 1 — Foundation & design system
**Mục tiêu:** một `UIRoot`, layer `GameplayHUD`, `PersistentNav`, `ScreenOverlay`, `Modal`, `Toast`; CanvasScaler `Scale With Screen Size`, ref 1080×1920, anchors/safe area theo thiết bị; sprite 9-slice, typography TMP, theme token, components; một test screen.
**Gate:** 3 tỷ lệ màn hình không tràn cắt; tiêu chuẩn font và nút hợp lý; không phát sinh click xuyên modal; build Unity không lỗi.

### Phase 2 — Gameplay HUD (ƯU TIÊN CAO NHẤT)
**Mục tiêu:** thay HUD 3 vùng, thu quest & tài nguyên, chuyển nhiệm vụ phụ vào drawer; rõ HP/MP, 4 skill cooldown/chi phí; toggle compact/full; damage popup tối ưu; banner boss/wave đúng thời điểm. Giữ nguyên tất cả sự kiện cũ, HP/MP logic, timer, skill triggers, trạng thái battle.
**Gate:** chiến đấu quan sát rõ; thông tin HP/MP luôn chính xác; idle/đánh boss vẫn hoạt động; không trùng listener hay cản raycast gameplay.

### Phase 3 — Global navigation + overlay behavior
**Mục tiêu:** dock 5 mục, secondary tabs; screen header/close/back; vòng đời modal/scroll; transition state. Quy định hiển thị/bật tắt HUD khi mở full screen hoặc bottom sheet.
**Gate:** có đường truy cập tới tất cả 9+ module đã tồn tại; từ mỗi module có thể về gameplay; Android Back/ESC xử lý đúng; không rơi vào screen rỗng.

### Phase 4 — Kiếm Lộ & Kho Đồ
**Mục tiêu:** lưới 4 cột tối ưu chiều rộng, rarity rõ, stack level/count, selection state, bộ lọc thu gọn, sort, pagination hoặc scroll phù hợp; actions batch ở footer sticky; item detail bottom sheet; kho hồn/nguyên liệu theo tab. Không thay đổi cách tính phân giải, spawn, merge hay tài nguyên.
**Gate:** thử thao tác chọn/sắp xếp/đổi trang/nâng cấp/phân giải/đóng; item count và tài nguyên trước/sau khớp dữ liệu.

### Phase 5 — Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp
**Mục tiêu:** dùng chung `CultivationScreen` và các pattern `CurrentStat / NextStat / Cost / CTA / State`; mỗi thẻ hệ thống nêu 1 thông điệp chính; trạng thái locked/affordable/maxed; progress và thời gian đặt cố định. Pháp Khí/Trận Pháp vẫn giữ cơ chế và dữ liệu hiện tại.
**Gate:** đối chiếu từng công thức/nút trước sau redesign; hiển thị đúng nguyên liệu, thanh tiến độ, giá, thời gian, số cấp.

### Phase 6 — Mốc Tu Tiên & Thiên Phú Đạo Vận
**Mục tiêu:** tabs `Có thể nhận`, `Đang làm`, `Hoàn thành`; danh sách mốc gọn, progress lớn; card thiên phú có icon rarity, hiệu ứng chính, phí và trạng thái. Có CTA “Nhận tất cả” chỉ nếu game hiện hỗ trợ hoặc khi được phê duyệt thêm chức năng mới.
**Gate:** logic claim, unlock, tầng/cấp, số lượng và tiết kiệm tài nguyên không đổi.

### Phase 7 — Bí Cảnh, minigames, thưởng (ƯU TIÊN THẨM MỸ CAO)
**Mục tiêu:** thống nhất bố cục Header (tên+timer) / Objective (1–2 dòng; xem thêm) / Play area / Progress / CTA Exit; giữ art làm chủ thể; thay thanh xanh neon bằng UI progress phù hợp theme; đảm bảo mọi nút thoát/tiếp tục có nhãn/icon rõ; reward modal chung. Áp dụng cho: Vạn Ma Triều Dâng, Trảm Phá Cấm Chế, Đan Thần Kỳ Duyên, Vượt Lôi Trì, Kiếm Chứng Cổ Thần.
**Gate:** timer, điều kiện thắng/thua, thao tác chạm, độ khó, RNG, kết quả/thưởng hoạt động tương đương trước khi sửa. Không đưa VFX UI lên che mục tiêu chính.

### Phase 8 — Menu, nhân vật, Settings, Pause
**Mục tiêu:** bảo tồn tranh menu, cân lại các nút; màn chọn nhân vật hiển thị chân dung/cảnh giới/tầng/thời gian như một thẻ dễ bấm; nút xóa đưa vào menu phụ nguy hiểm và xác nhận rõ; pause dùng slider đúng theme + chỉ báo %, trạng thái save/quit rõ.
**Gate:** New Game/Continue/Load/Delete/Save/Pause/Exit không tạo lỗi mất dữ liệu; xác nhận 2 bước cho hành động nguy hiểm.

### Phase 9 — Feedback, micro-animation & polish
**Mục tiêu:** toast tài nguyên +/-, trạng thái không đủ tài nguyên, chuyển tab, item rarity, reward, trạng thái có thể claim; easing 120–250ms có thể bỏ nếu FPS yếu; giảm particle UI vô ích; audio/hover/click feedback; UI sprite atlas/overdraw.
**Gate:** không lag khi nhiều VFX hay khi mở inventory; không dính input hoặc tooltip; font tiếng Việt hiển thị đầy đủ.

### Phase 10 — QA, visual regression & release gate
**Mục tiêu:** screenshot từng màn tại 3+ tỉ lệ, test chu trình từ mở game → chọn nhân vật → idle → trang bị → luyện → bí cảnh → nhận thưởng → pause/save → load; chạy 15–30 phút trên máy Android thật; đối chiếu giá trị tài nguyên và damage/farming trước-sau; build không Console error.
**Gate:** không overlap/cut/clipped text, không nút mất chức năng, không UI bị kẹt, không chạm xuyên, không mất save; hiệu năng UI không suy giảm rõ so với baseline.

## E. Quy tắc bắt buộc khi giao nhiệm vụ cho Antigravity / Codex

1. **Không làm tất cả phase cùng một lượt:** mỗi phase là commit riêng; chạy project, chụp ảnh và báo cáo rõ trước-sau; chỉ chuyển phase sau khi đạt gate.
2. **Không sửa gameplay logic khi đang redesign:** giới hạn thay đổi vào prefab, Canvas, RectTransform, TMP, Image/Sprite, UI adapter/controller, style assets, nav/view state. Nếu buộc phải đổi public API, phải giải thích và giữ compatibility.
3. **Không phá tính năng:** liệt kê nút/trạng thái cũ và mapping mới; giữ persistence, IAP/ad hooks nếu đang có, save schema, unlock state và dữ liệu.
4. **Không hard-code cho một điện thoại:** Unity RectTransform anchors, CanvasScaler, safe area, ContentSizeFitter/LayoutGroup có kiểm soát; vùng cuộn; TMP wrapping/ellipsis có chủ đích.
5. **Không tạo ảnh trước khi thống nhất layout:** mockup đơn sắc/greyscale trước, áp theme sau. Không lấy panel khung có nhiều chi tiết làm lý do để đặt chữ sát mép.
6. **Mỗi phase báo cáo:** `Files changed`, `Functionality mapping`, `Screenshots`, `Build result`, `Known issues`, `Acceptance checklist`.
7. **Screenshot của AI phải là ảnh Game View thật**, không phải hình minh họa do AI dựng; nên có crop 1080×1920 để đối chiếu với video gốc.
8. **Dữ liệu động bắt buộc được test** với long strings, số tài nguyên lớn, progress 0%/100%, full inventory, low HP/MP, boss timer về 0, màn hình có safe area.

## F. Master prompt có thể gửi cho Antigravity / Codex

> Bạn là Lead Unity Mobile UI/UX Engineer + UI Designer của dự án Khí Vận Chi Tử, Unity 6.6 Android. Nhiệm vụ là redesign hoàn toàn UI/UX/HUD theo tài liệu `KhiVanChiTu_UIUX_Redesign_Plan.md` và video review đính kèm, bảo toàn 100% gameplay, dữ liệu, cơ chế, tính năng, nút cũ. **Trước hết CHỈ THỰC HIỆN PHASE 0**, audit Canvas, UI prefabs, button OnClick, panel flow, save/loading, screenshots và lập feature map. Không làm UI mới ở Phase 0. Sau khi xuất các tài liệu và report, chờ phase kế tiếp. Mỗi phase phải có commit, screenshot Unity thực tế ở 1080×1920 và 1080×2340, test chức năng và gate. Ưu tiên rõ trận đấu, dễ thao tác ngón tay, không text overflow, không nút lỗi, ngôn ngữ hình ảnh tu tiên xanh ngọc – vàng cổ. Tuyệt đối không thay đổi gameplay logic để tiện làm UI.

## G. Definition of Done (kết thúc toàn bộ)

- Màn chính nhìn thấy nhân vật/quái/hiệu ứng rõ, thông tin ưu tiên trong 1–2 giây.
- Tất cả module cũ còn truy cập được; 4 kỹ năng, kho đồ, Kiếm Lộ, Luyện Thể/Đan, Pháp Khí/Trận Pháp, Bí Cảnh, Mốc, Thiên Phú, pause/save/load/đổi nhân vật đều chạy.
- Tiêu đề, nội dung, nút, thanh HP/MP, slider, trạng thái disable, reward và error cùng design system.
- Không còn nút cuối màn hình Bí Cảnh thiếu nhãn/chức năng khó hiểu.
- Không còn các danh sách chữ dày đặc mà không có phân cấp; card chi tiết có thể mở rộng khi cần.
- Không có text sát viền, cắt chữ hoặc scroll nằm dưới nav, bao gồm màn 9:16, 19.5:9 và 20:9.
- Test trên Android thật, nhận chạm đúng, không mất tiến trình, không thay đổi cơ chế drop/upgrade/boss/RNG.

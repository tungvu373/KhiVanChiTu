# Phase 9 — Global polish & micro interactions

Unity 6000.6.4f1, mobile scene, 18/11/2026. Áp dụng bằng Editor Scripting/Pipeline, không sửa scene YAML thủ công. Các phase trước vẫn giữ controller, event, công thức, giá trị và save.

## Thay đổi

- Bổ sung press/release cho nhãn TMP bằng motion director hiện có: 70/100 ms unscaled, chỉ scale hình/nhãn, vùng chạm đứng yên. Nút native trong RpgViewScope và FoundationAction sinh động đều được hỗ trợ. Không thêm listener thực thi command.
- Nhấn chọn item có accent ngắn; header/chủ thể khi mở view có reveal 180 ms. Animation đóng/mở của drawer/modal và reward giữ controller hiện có, không thêm trì hoãn navigation. Kênh accent tự tắt khi hoàn thành, giới hạn tối đa sáu trong một đợt và khoảng cách giữa các đợt như motion director gốc.
- Một toast chung, không raycast, thời gian unscaled 2,6 giây, fade 120 ms. Thông báo mới thay thông báo trước; chiều cao tự tăng theo nội dung dài. Theo dõi chính feedback text của merge, trang bị, nâng cấp, luyện đan, thiên phú và mốc thưởng để bổ sung accent/toast, không suy diễn kết quả transaction.
- Nút không khả dụng có thể giải thích bằng `condition`/requirements/preview đang hiển thị của presenter. Không gọi lại giao dịch khi bấm nút disabled. Feature unlock dựa vào sự kiện và trạng thái FeatureUnlockService, không mở khóa bằng UI.
- Các nhãn TMP trong view mới không nhận raycast. Typography, padding, màu, khung, control height tiếp tục lấy từ Foundation của Phase 1. Giữ rarity và icon/sprite hiện có.
- Sửa cách hiển thị giá trị cực lớn trong Kho, Tu luyện và Progression: từ 1E+15 dùng `0.##E+0`, thay cho chuỗi B dài. Chỉ sửa định dạng UI; không thay EconomyManager hoặc giá trị lưu.

## Kiểm chứng thực thi

[83 case có expected/actual/status](Evidence/polish/cases.json) PASS, không FAIL. Quét các route Merge, Storage, bốn tu luyện, Talents, Milestones, RealmList, Character, Achievements, Resources, Buffs, Log, Supplies, SwordStyle, Settings tại 1080×1920, 1080×2340, 720×1280 và 1440×3200. Có tên nhân vật dài, tài nguyên 1E+30, 64 kiếm tới cấp 10, kiểm overflow TMP (Ellipsis có chủ đích được miễn), HP/MP, skill mana/cooldown/locked, missing scripts.

Đã thực thi pointer down/up để xác nhận TMP co nhẹ trong khi hit rect giữ nguyên, trả về rest ở timeScale 0; bấm unavailable hiển thị đúng điều kiện; toast dài wrap, không chặn input, tự hết hạn ở timeScale 0; motion channels ngủ ở trạng thái nghỉ. [Console errors](Evidence/polish/console-errors.json) rỗng. [Save preservation](Evidence/polish/preservation.json) cả ba hash dữ liệu thật không đổi.

Ảnh Game View thật trong `Evidence/polish/`: HUD, mọi route ở bốn kích thước, button-pressed, insufficient-toast, long-toast. Đã nhìn trực tiếp ảnh HUD, Luyện Thể và toast để phát hiện phần kiểm code không thấy. Full chức năng minigame, merge/upgrade, các trạng thái progression và save restart được chạy lại trong Phase 10 tiếp theo; không coi quét layout này là thay thế toàn bộ functional QA.

## Đo hiệu năng và giới hạn

120 frame Editor thực sự tiến, profile `global-ui-20261118-055037`, 1080×1920, inventory đang mở. Median Main Thread 9,38 ms, Canvas.BuildBatch 0,0126 ms, GPU Frame Time 1,48 ms, GC Allocated In Frame 19.187 byte; tất cả là số của **toàn Editor frame**, không phải UI độc lập hay FPS Android. Texture Memory khoảng 406 MiB gồm asset/editor/font/game, không chỉ UI. 379 Graphic active trong hierarchy inventory, 70 raycast targets; 0 callback dirty layout/vertex trong cửa sổ idle đã đo.

Draw Calls Count trả 0 nên **không dùng số đó làm draw-call budget**; SetPass median 61 là toàn frame. Hai recorder Canvas.SendWillRenderCanvases/LayoutRebuilder.Rebuild không có trong phiên này; ghi trong raw profile. Reflection microprobe báo 0 byte, không đủ để khẳng định zero-allocation của toàn UI. [Raw profiler](Evidence/polish/inventory-profile.json).

Không thấy bằng chứng Canvas rebuild idle là bottleneck; không viết lại layout hoặc thêm cache theo phỏng đoán. Kiểm repository không có SpriteAtlas asset; chưa thêm atlas cho các tranh nền/portrait lớn khác nhau vì chưa có Frame Debugger/device evidence chứng minh lợi ích. Atlas, exact overdraw và texture budget Android tiếp tục là điểm cần xác minh thiết bị, không ghi PASS giả. Không tuyên bố hiệu năng tăng hay production-ready Android từ phép đo Editor.

Lượt đo/quét đầu được loại khỏi so sánh vì test đã pause trước `Invoke(LoadCurrentGame, 0.05)` chạy. Runner đã sửa để đợi `isSessionActive`, thêm thời gian settle modal trước chụp và kiểm inherited alpha HUD. `Evidence/baseline` giữ dấu vết **INVALID FIXTURE**, không dùng làm baseline hiệu năng trước/sau. Không có thay đổi gameplay từ sửa test harness.

Phase 9 hoàn thành phần polish và kiểm tra trên Editor; chuyển Phase 10 theo yêu cầu chủ dự án. Các giới hạn Android thực, cutout, build và thao tác xóa dữ liệu được ghi nhận riêng trong nghiệm thu cuối.

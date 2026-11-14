# Phase 04 — HUD visual & functional report

**Kết quả:** prototype HUD portrait đã lưu trong `Assets/Scenes/SampleScene_MobileWork.unity`, kèm bốn prefab tái dùng. [Layout](new_hud_layout.md) và [mapping](hud_feature_mapping.json) mô tả implementation thực tế.

## Hình ảnh bản chạy

Các ảnh là frame composited từ Unity Play Mode, gồm Screen Space Overlay. Dữ liệu được tạo trong profile QA riêng `portrait-hud-p04-qa`, không lấy tiến trình người chơi làm fixture. Art/nhân vật/VFX world giữ nguyên.

### Màn hẹp360×640

![HUD mobile360x640](<C:/Lap trinh game/Save file/KhiVanChiTu/Docs/UI_Redesign/Images/Phase04/hud-360-final.png>)

Bốn kỹ năng ngang, MP số tách trạng thái cast, bốn quick route và năm dock tab. Tên Việt dài hiển thị hai dòng; ví có compact units và lối đọc giá trị đầy đủ. Hit rect nhỏ nhất của control HUD là132reference≈44logical tại width360.

### iPhone12 —390×844logical, render1170×2532

![HUD iPhone12 safe area](<C:/Lap trinh game/Save file/KhiVanChiTu/Docs/UI_Redesign/Images/Phase04/hud-iphone12-safearea.png>)

Device Simulator trả safe area(x0,y102,w1170,h2289): trên141px, dưới102px. HUD nằm trong vùng này; world tiếp tục full bleed. Các raycast mục tiêu chính đã được kiểm trên Simulator. Đây không phải ảnh chụp thiết bị iPhone thật.

### Focus mode

![HUD focus mode](<C:/Lap trinh game/Save file/KhiVanChiTu/Docs/UI_Redesign/Images/Phase04/hud-focus-final.png>)

Ẩn skill/quick/dock, giữ thông tin, HP/MP và đường “Hiện HUD”. Rect clear bảo thủ của center tại1080×1920 tăng từ1008×608 lên1008×1122reference. Không thay camera/frustum, không tắt auto combat hoặc đặt timeScale từ controller HUD. Không quy đổi những rect này thành tuyên bố80% opaque coverage.

### Menu phụ

![HUD secondary menu](<C:/Lap trinh game/Save file/KhiVanChiTu/Docs/UI_Redesign/Images/Phase04/hud-menu-final.png>)

Menu có nền đặc để đọc, khung ngọc–vàng, backdrop và close. Nhân vật/Thành tựu mở view nguồn; Hiệu ứng và Nhật ký giữ dữ liệu/factory cũ. CanvasGroup khóa dock dưới menu, world polling nhận blocking state. Back đóng menu trước khi xử lý drawer/root.

## Phạm vi thay đổi

- Dựng hierarchy shell mới; dời các object control/data có sẵn, giữ object identity và serialized references.
- Profile/name/tu vi và resource presentation mới. Hai currency Text gốc dùng một projection event-driven; owner cũ tắt riêng ở scene prototype.
- Four-skill layout ngang giữ nguyên listener,cost,unlock,cooldown/ready data. Thêm numeric MP đọc từ SkillManager, kể cả khi ManaText cũ đang hiển thị cooldown/tuyệt kỹ.
- Dock giữ thứ tự/index0–4; direct quick access Kho/Bí cảnh/Vận khí/Mốc. Năm workbench giữ nội dung/commands; shortcut phân giải được tăng vùng chạm.
- Menu phụ, focus mode, safe area, glyph refresh khi đổi render scale và feedback line currency/progression.
- Bốn bridge trong source cũ: gameplay-choice presentation/input gate, Back route, legacy menu handoff và unscaled Character modal khi prototype hiện diện. Không sửa combat/economy formulas hoặc save schema.
- Không sinh ảnh mới hoặc sửa import art. Source Sans Pro/atlas Jade dùng lại. Landscape/PC assets giữ nguyên.

## Kiểm chứng

| Kiểm tra | Kết quả / phạm vi |
|---|---|
| Compile | Các lượt compile hoàn tất0error; kiểm cuối up-to-date,0error |
| Scene identity |1.327/1.327 object ban đầu còn đủ; scene cuối1.358 object;0 missing script |
| Persistent events | Cả5 target/method gốc giữ nguyên |
| Mapping |955 control/data records duy nhất;110 action contracts nguồn được liên kết |
| Prefabs |4 asset có GUID/dependency thật; không phải tên placeholder |
| Main entry | Nút confirm gốc nhận raycast sau activation tick và vào đúng slot QA |
| Closed HUD | Raycast top-hit của5dock,4quick,4skill,menu,wallet,focus; center không bị HUD nhận hit |
| Drawers | Mở/đóng cả5tab; close và dock nhận hit trên backdrop; thao tác ở timeScale0 được kiểm |
| Quick routes | Kho,Bí cảnh,Vận khí,Mốc mở view nguồn đúng |
| Secondary | Nhân vật,Thành tựu,Log,Buffs mở đúng; log giữ ScrollRect; empty buff state có nội dung |
| Pause | Nhân vật mở/đóng hoàn tất ở timeScale0; focus cũng hoạt động; có lượt kiểm ở.4 và1 |
| Data | Currency event cập nhật ngay; MP67 hiển thị đúng fixture; một tap normal skill tiêu40MP một lần và mở cooldown nguồn |
| State | Số dư0,số lớn,tên Việt dài,normal-skill lock,ultimate disabled/active/cooldown và focus được quan sát |
| Responsive | Native1080×1920,360×640 và iPhone12 Simulator1170×2532 được kiểm; đổi size khi focus không làm dock hiện lại |
| Modal safety | Menu phụ khóa dock; popup offline thật chặn các target HUD và được xử lý qua public close handler của source trong profile QA |
| Persistence |18 file save/data/backup đã có giữ nguyên SHA256; production PlayerPrefs hash giữ nguyên; chỉ profile QA phát sinh dữ liệu riêng |
| Isolation | PC module,Packages,ProjectSettings và imported art khớp baseline |
| Git | HEAD/branch/index giữ nguyên; không stage/commit |

Bằng chứng runtime ghi **43 pass assertions**, có các lần lặp ở nhiều resolution; không có record marked failure trong bộ kết quả đã lưu. Không diễn giải43 thành43 test case độc lập hoặc coi955 static mappings là955 thao tác gameplay đã chạy.

Các lượt thử ban đầu có guard dừng khi gọi quá sớm trước graphic-registration tick, chờ animation close chưa đủ và khi popup offline đang giữ focus. Đã điều chỉnh thứ tự/chờ/dismiss và chạy lại các nhóm liên quan. Toàn bộ console session được archive trước khi dọn hiển thị console; không che các lượt thử này khỏi bằng chứng.

EventSystem checks dùng GraphicRaycaster rồi gọi Button.onClick trong Editor; chưa thay thế kiểm native touch/drag trên máy thật. Mỗi record có frame và resolution, xác nhận game/player loop đã tiến thay vì chỉ chụp một frame đứng.

## Tệp bàn giao

- `Docs/UI_Redesign/new_hud_layout.md`
- `Docs/UI_Redesign/hud_feature_mapping.json`
- `Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab`
- `Assets/UI/MobileHUD/hud_prefabs/HudCoreDock.prefab`
- `Assets/UI/MobileHUD/hud_prefabs/HudActionButton.prefab`
- `Assets/UI/MobileHUD/hud_prefabs/HudResourceChip.prefab`
- `Docs/UI_Redesign/hud_visual_report.md`
- Bốn ảnh review trong `Docs/UI_Redesign/Images/Phase04/`.

Shell và dock cần scene/controller/services của dự án; không chứa game managers nhân đôi. Scene mobile đã lưu là prototype tích hợp hoàn chỉnh. Năm workbench/các modal đang hoạt động được giữ làm backend view; chưa migrate toàn bộ57route Phase02 thành full-screen pages/router mới.

Bằng chứng local bị Git-ignore nằm trong `Docs/UI_Redesign/Backups/Phase04-20261112-173028/`: snapshot/hashes ban đầu,ZIP rollback,live hierarchy trước/sau,checks JSON,geometry/resolution snapshots,console archive và bản gốc các capture QA. Capture tạm đã được đưa ra khỏi Assets để không thành texture của game.

## Giới hạn còn lại

- Chưa build Android/iOS hoặc thử thiết bị vật lý; chưa kiểm exhaustive multi-touch,drag/swipe,keyboard/accessibility hoặc mọi unlock combination.
- Chưa profiling overdraw,Canvas rebuild,texture memory hoặc FPS. Không có tuyên bố tăng hiệu năng.
- Nội dung workbench/modal vẫn dùng implementation cũ; phase này không thay các hệ thống economy/save hoặc giải quyết toàn bộ các finding Phase01.
- Chưa triển khai toàn bộ screen architecture Phase02,asset generation plan Phase03 hay thay toàn bộ category icon bằng icon skill độc lập.
- Portrait là layout đích. Landscape game layout riêng và các scene PC giữ nguyên.
- Các dữ liệu/cảnh giới trong ảnh là fixture. Không dùng số liệu ảnh để chứng minh balance hoặc tiến trình người chơi.

Editor bàn giao ở Edit Mode, scene đã lưu clean, marker QA được gỡ. Override chạy nền phục vụ QA đã hoàn tác; hash PlayerSettings/Pipelineconfig khớp trước phase. Rollback chọn lọc và checksum checkpoint được ghi trong layout document.

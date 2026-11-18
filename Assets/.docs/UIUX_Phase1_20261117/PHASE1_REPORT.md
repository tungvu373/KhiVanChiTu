# PHASE 1 — UI Design System & Foundation

Ngày: 17/11/2026. Unity 6000.6.4f1, Android target đang chọn trong Editor.

**Kết quả:** đã tạo thư viện foundation, scene showcase riêng và kiểm chứng runtime trong Unity Editor ở đủ ba kích thước. Dừng sau Phase 1. Chưa redesign/migrate màn gameplay cụ thể.

## 1. Công việc đã thực hiện

- Theme Ngọc Trầm / Vàng Cổ: palette, typography, spacing, control states, icon aspect, 9-slice và quy tắc padding. Giá trị cụ thể trong DESIGN_SYSTEM.md và design_tokens.json.
- Hai TMP SDF font từ Source Sans Pro Regular/Semibold đã có trong project. Builder kiểm tra bộ glyph tiếng Việt trước khi tạo asset; không chuyển các Text legacy của game.
- CanvasScaler ScaleWithScreenSize, reference 1080×1920, width match; Safe Area normalized theo viewport/insets.
- Các lớp GameplayHUD, Navigation, ScreenOverlay, Modal và Notification với sorting riêng; modal stack, Back, backdrop, focus và chặn click vào HUD phía sau.
- 15 prefab có API/binding và hành vi thật. Showcase dùng dữ liệu cục bộ để thử tăng/reset progress, chọn tab/item, mở/đóng panel, confirm/reward callback, toast và tooltip.
- Surface 64×64, border slice 14, padding nội dung 32; không dùng khung dày làm giảm vùng đọc.
- Tạo prefab và scene bằng Unity Editor API, không sửa YAML thủ công. Màn thử nghiệm có camera riêng, EventSystem Input System và không có manager gameplay/save.

## 2. Files thêm/sửa

Toàn bộ implementation mới nằm trong Assets/UI/Foundation cùng meta do Unity tạo. Danh sách và SHA-256 chi tiết trong phase1_manifest.json.

| Nhóm | Files |
|---|---|
| Runtime theme/root | Runtime/FoundationTheme.cs, FoundationRoot.cs |
| Runtime controls | FoundationAction.cs, FoundationTabs.cs, FoundationValue.cs, FoundationProgress.cs, FoundationItem.cs |
| Runtime overlays | FoundationPanel.cs, FoundationNotice.cs, FoundationTooltipTrigger.cs |
| Factory/demo | FoundationFactory.cs, FoundationShowcase.cs |
| Editor | Editor/FoundationBuilder.cs, FoundationValidation.cs |
| Theme | Theme/JadeFoundation.asset, VietnameseBodySDF.asset, VietnameseHeadingSDF.asset, Surface9Slice.png |
| Prefabs | PrimaryButton, SecondaryButton, ScreenHeader, TabBar, ResourceDisplay, ItemSlot, ItemCard, ProgressBar, StatRow, FullScreenPanel, BottomSheet, RewardModal, ConfirmDialog, Tooltip, Toast |
| Scene | Showcase/UIFoundationShowcase.unity |
| Tài liệu | Assets/.docs/UIUX_Phase1_20261117/DESIGN_SYSTEM.md, COMPONENT_GUIDE.md, design_tokens.json, PHASE1_REPORT.md, phase1_manifest.json |
| Bằng chứng | Evidence/validation-results.json và 9 PNG Game View; file lỗi kiểm thử cũ giữ ở validation-history-first-failure.txt |

Trong lúc thiết lập TMP, các tài nguyên TMP Essentials/Examples & Extras đã được import trong Editor. Foundation chỉ cần Essentials. Các tài nguyên import này được giữ nguyên và cần review riêng khi stage Git; builder không tự import Examples hoặc đổi package manifest.

## 3. Chức năng và dữ liệu được giữ nguyên

- So sánh lại **1.398 file** trong manifest Phase 0: **0 thay đổi nội dung, 0 file thiếu**. Phạm vi bao gồm source C#, scene/prefab/meta gốc và Packages/ProjectSettings.
- Không chỉnh formula, tỷ lệ drop, RNG, skill, tài nguyên, merge, progression hoặc save schema/key/profile.
- Không thêm FoundationRoot vào scene gameplay/PC. Không thay navigation owner hoặc các event cũ. Prefab foundation chưa được dùng để thay control production.
- Showcase không có DataManager, không gọi PlayerPrefs, không cấp reward/tài nguyên vào nhân vật. “Reward” trong gallery chỉ đổi DemoValue.
- Các màn cũ chưa được chạy regression đầy đủ trong phase này; checksum bảo toàn source không phải bằng chứng gameplay/save-load runtime parity.

## 4. Kết quả kiểm tra và ảnh Game View

Bộ kiểm thử thực tế chạy khi Play trong duy nhất UIFoundationShowcase. Có kiểm tra Time.frameCount tăng; không coi một khung hình tĩnh là game đang chạy.

| Kích thước thực | Kết quả runtime | Gallery | Controls cuối danh sách | Dialog |
|---|---|---|---|---|
| 1080×1920 | Pass | [Ảnh](Evidence/gallery-1080x1920.png) | [Ảnh](Evidence/controls-1080x1920.png) | [Ảnh](Evidence/dialog-1080x1920.png) |
| 1080×2340 | Pass | [Ảnh](Evidence/gallery-1080x2340.png) | [Ảnh](Evidence/controls-1080x2340.png) | [Ảnh](Evidence/dialog-1080x2340.png) |
| 720×1280 | Pass | [Ảnh](Evidence/gallery-720x1280.png) | [Ảnh](Evidence/controls-720x1280.png) | [Ảnh](Evidence/dialog-720x1280.png) |

Đã đọc PNG header của cả 9 ảnh: kích thước khớp tên file. Đã xem trực tiếp ảnh gallery ở cả ba kích thước, dialog 720×1280/1080×2340 và controls 720×1280. Body dài nằm trong vùng cuộn; header/footer được giữ riêng. Việc một phần control ở mép viewport bị mask khi cuộn là hành vi scroll có chủ đích.

Các kiểm tra đã chạy trong Unity:

- Primary command đúng một lần; disabled không thực thi; reset mẫu hoạt động.
- Progress 0%, 100%, denominator 0; tab selection; item selection.
- Stack hai lớp: Back hủy confirm, trở lại panel dưới và không gọi accept; confirm/reward gọi callback đúng một lần; BottomSheet đóng bằng Back.
- Toast/Tooltip Show/Hide bằng API. Touch-hold/pointer trigger có implementation; thao tác cảm ứng vật lý và thời gian tự ẩn trên Android chưa được test.
- TMP font được gán, text không báo overflow nội bộ tại thời điểm kiểm tra; ScrollRect có content/viewport dương, button không có kích thước 0.
- Simulated safe inset trên/dưới được áp dụng; không thay thế thử notch thật.
- EventSystem.RaycastAll kiểm tra vị trí giữa modal: hit thuộc ConfirmDialog, HUD CanvasGroup không nhận raycast phía sau.
- Màu progress được chỉnh sau visual review để nhãn dễ đọc; icon chuyển Simple/preserveAspect để không bóp méo kiếm.

Kết quả cuối nằm trong Evidence/validation-results.json, mới hơn file lỗi lịch sử. Bộ test không gọi click cảm ứng Android thật; callback kiểm tra bằng API và raycast kiểm tra qua EventSystem của Editor.

## 5. Lỗi còn tồn tại / giới hạn

- Console vẫn chứa lỗi/cảnh báo lịch sử; không xóa Console để tạo cảm giác sạch. Trong quá trình làm có lỗi khởi tạo font khi TMP Essentials chưa sẵn sàng và lỗi ở bộ kiểm thử Game View; đã xử lý trước lần pass cuối. File lỗi kiểm thử cũ được giữ làm lịch sử.
- CLI Pipeline vẫn không truy cập được port 7800 từ terminal, nên thao tác Editor thực hiện qua cửa sổ Unity và menu công cụ của phase.
- Chưa test Android thật, physical dp, notch/gesture thật, Android Back hệ điều hành, FPS/GC/overdraw hoặc APK/AAB.
- Orientation/build settings global vẫn giữ baseline. Foundation hỗ trợ portrait; việc khóa orientation/review active Android profile cho game hiện tại còn thuộc tích hợp sau.
- Migration legacy Text, path aliases, layout writers Jade và navigation/gameplay coupling từ Phase 0 chưa được sửa trong Phase 1.
- Git worktree/index không truy cập được trong phiên này; không stage/commit để tránh lẫn hoặc ghi đè user work. **Chưa có commit Phase 1.** Không reset/clean/stash/push.

## 6. Compile / build và bàn giao

Mã runtime và Editor đã biên dịch thành công với compiler/reference assemblies của Unity 6000.6.4f1. Editor đã compile/import và thực thi builder/validator thực tế; 15 prefab, font/theme và scene đã được lưu qua Editor. Không tuyên bố Console toàn project sạch lỗi chỉ vì compile thành công.

**Android build:** chưa chạy; không có kết quả APK/AAB/device.

Editor đã thoát Play Mode sau kiểm tra; scene showcase được để mở để review. Menu và cách dùng nằm trong COMPONENT_GUIDE.md. Không tự chuyển sang Phase 2.

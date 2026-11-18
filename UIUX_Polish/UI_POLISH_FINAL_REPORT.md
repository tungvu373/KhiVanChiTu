# Kết quả UI/UX polish

Đã triển khai đủ 8 stage của master prompt trong chính cuộc trò chuyện được hẹn giờ. Giữ bố cục hiện có; chỉnh artwork HUD, điều hướng, thông báo và lỗi trình bày. Không triển khai thêm phase khác.

## Kết quả kiểm chứng

- 7020 assertion/test đã thực thi PASS, 0 FAIL; 4 BLOCKED và 2 NOT RUN được ghi rõ trong [báo cáo QA](UI_POLISH_QA_REPORT.md). Số đếm gồm từng assertion tự động, không phải hàng nghìn kịch bản kiểm thử thủ công.
- 317 ca HUD/navigation/notification/presentation, gồm **80 lần bấm Gọn/Đủ**: 20 lần liên tiếp ở mỗi raster 1080×1920, 1080×2340, 720×1280; thêm 20 lần khi combat chạy cùng fixture banner boss. Tọa độ, khả năng raycast và trạng thái phục hồi đều ổn định.
- Kiểm thử native: Inventory 48 assertion; tu luyện 739; progression 4072; Bí Cảnh 1251; menu/settings tạo nhân vật 440 và load lại 134. Không có runtime error trong các suite.
- 244 screenshot thật: 74 ảnh 1080×1920, 64 ảnh 1080×2340, 84 ảnh 720×1280, 22 ảnh 1440×3200. [Danh mục ảnh và SHA-256](Evidence/screenshots.json).
- Compile C# không có error. Build Android **Succeeded, 0 error, 2 warning**, 54.1 giây. [Build report](Evidence/android-build.json), [APK integrity](Evidence/apk-integrity.json). APK thực tế 116,612,737 byte; totalSize của BuildReport là số liệu khác với dung lượng APK nén.
- Scene không có missing script hoặc reference lỗi. 315 binding OnClick gốc giữ nguyên. 121 file gameplay/settings được đối chiếu hash; tất cả giữ nguyên. Các file dirty có sẵn ngoài scene không bị sửa; scene được stage riêng phần delta do Editor tạo.
- PlayerPrefs, save và backup nhân vật thật giữ nguyên trong mọi suite. Chỉ dùng profile QA riêng. Không xóa/reset nhân vật thật.

## Artwork đã tích hợp

Đã dùng image_gen thật; giữ nguyên file RGBA sinh ra, có alpha và ghi hash. Asset chính: `Assets/UI/Refinement/Art/JadeSurface.png`, 1254×1254. Sprite chung dùng cho thông tin nhân vật/tài nguyên, mục tiêu, controls, skill slot và progress track. Import Sprite/Single, alpha, 9-slice 180px, mipmap tắt. Tỷ lệ 9-slice được sửa sau khi đối chiếu ảnh thật để tránh bó nền thành viên thuốc. HP/MP fill, cooldown và icon gameplay giữ riêng.

Một biến thể khung skill được tạo nhưng loại vì viền vàng dày. Không dùng nó, không trình bày placeholder thành asset hoàn chỉnh. Không còn asset bắt buộc chờ AI. [Prompt và nguồn gốc](HUD_IMAGE_GENERATION_PROMPTS.md), [manifest 194 Image dependency](HUD_ASSET_MANIFEST.md).

## Đạo Vận và badge

Audit implementation hiện tại tìm thấy Đạo Vận ở route `Talents`, nhóm **Khác**; mô tả trong prompt là vị trí Tu Luyện của bản trước. Đã chuyển đúng route có thật vào **Trang bị → Đạo Vận (đầu tiên) → Kiếm Lộ → Kho Đồ**. Không còn primary entry trong Tu Luyện hoặc Khác. Back trả về Trang bị. Giữ `DaoAnModalUI`, `DaoAnProgressionService`, catalog, level và save hiện có; không tạo thêm hệ thống Đạo Vận.

`NotificationBadge` cung cấp SetVisible/SetCount, hỗ trợ TMP count tùy chọn; indicator tròn nhỏ trên góc phải, ignoreLayout và không nhận raycast. `NavigationNotifications` gom event economy/service, đánh dấu dirty rồi tính lại một lần; không quét catalog/inventory mỗi frame. Có 29 badge trên 5 group và 24 route.

Điều kiện thật đã nối: Đạo Ấn có thể mua, nhánh Luyện Thể có thể nâng, Pháp Khí đủ điều kiện/nguyên liệu, đồ đang chờ xử lý trong Storage, mốc đã đủ nhận thưởng. Group tổng hợp child theo mapping route mới; Đạo Vận tính vào Trang bị. Mở màn hình không xóa available-action badge. Badge được tính lại từ dữ liệu lưu hiện có; không thêm cờ unseen hoặc schema save giả. Những route chưa có điều kiện được nối không tự hiện chấm đỏ.

## Nguyên nhân lỗi Gọn/Đủ

`MobileDrawerUIController.AnimateDockCollapse` của layout cũ và `CombatHudPresentation.ApplyLayout` cùng ghi RectTransform `PortraitHudShell/BottomDockToggle`. Animation cũ kéo x về 0 và y về vị trí đáy; navigation mới ngừng nhánh layout dock cũ nên nút bị trôi và có thể mất vùng chạm.

Thêm quyền sở hữu layout rõ ràng `externalDockLayout` cho scene mobile: drawer giữ state/callback nhưng không chạy animation ghi vị trí HUD do presentation mới quản lý. Nút phục hồi là sibling ngoài nội dung bị ẩn và ignore layout. Không tạo lại nút, không nhân listener. Scene PC mặc định vẫn dùng hành vi cũ. [Ca toggle có tọa độ/raster/frame](Evidence/refinement/cases.json).

## Bí Cảnh

Đối chiếu tên thật: Vạn Ma Triều Dâng, Trảm Phá Cấm Chế, Đan Thần Kỳ Duyên, Vượt Lôi Trì, **Kiếm Chủng Cổ Thần**. Title căn giữa vùng nội dung bên trái Exit; objective/instruction/result rõ ràng, timer/score có gutter đối xứng, action label chừa cùng khoảng hai bên dù icon nằm trái. Card description wrap; prefab RealmCard và các instance scene được sửa bằng Unity API.

Arena 384×440, spawn/input, timer, RNG, win/lose và reward giữ nguyên. Giới hạn kiếm vẫn **10**; phần thưởng vùng vàng Kiếm Chủng cấp 8 là quy tắc reward hiện có, được giữ nguyên. Đã kiểm tra mở/chạy/thắng/thua/thoát/nhận thưởng và save/load theo khả năng suite native; ca timeout checkpoint và dữ liệu reward cực lớn được ghi rõ là fixture trong raw evidence.

## File thay đổi

- Scene: `Assets/Scenes/SampleScene_MobileWork.unity` (chỉ delta của công việc này trong commit).
- Prefab: `Assets/UI/SecretRealm/Prefabs/RealmCard.prefab`.
- Runtime cũ: `Assets/Scripts/UI/MobileDrawerUIController.cs`, `Assets/UI/CombatHUD/Runtime/CombatHudPresentation.cs`, `Assets/UI/Navigation/Runtime/GlobalNavigationController.cs`, `Assets/UI/SecretRealm/Runtime/RealmPlayView.cs`.
- Runtime mới: `Assets/UI/Refinement/Runtime/NotificationBadge.cs`, `NavigationNotifications.cs`.
- Editor: `Assets/UI/Refinement/Editor/HudRefinementBuilder.cs`, `HudPolishChecks.cs`; thêm mode refinement vào `Assets/UI/Polish/Editor/GlobalUIQA.cs` để không ghi đè evidence Phase 9/10.
- Artwork mới và .meta: `Assets/UI/Refinement/Art/JadeSurface.png`.
- Báo cáo, manifest, raw assertions, screenshot và bằng chứng nằm trong `UIUX_Polish/`.

Menu Editor có thể chạy lại: **TuTien/UI Refinement/Apply audited HUD polish**. Lần apply kiểm chứng giữ nguyên 2637 object identity, 29 badge, không còn marker save QA. [Idempotency](Evidence/editor-idempotency.json).

## Giới hạn và kết quả từng phần

Không có điện thoại Android kết nối: chưa xác minh touch/cutout/FPS/GPU/overdraw trên thiết bị thật. Không coi build APK là test trên điện thoại. Exact UI draw-call counter trả 0 trong Editor nên không dùng làm bằng chứng; chưa có baseline Phase 0 runtime đáng tin cậy cho pixel diff. Rotation và persisted unseen-content tracking không được thực thi trong scope portrait/available-action này.

Inventory được đo 120 frame Editor: 0 dirty layout/vertex callback trong cửa sổ đo; số GC, texture và CPU/GPU là toàn frame/Editor. Không tuyên bố tối ưu FPS hoặc zero allocation trên Android. HP/MP boundary fixture tạm dừng hai writer native để giữ đúng 0/100% khi chụp, sau đó khôi phục trước test combat. Banner boss được bind bằng trạng thái StageManager thử nghiệm, không phải chứng minh thắng boss tự nhiên.

Các attempt lỗi đã lưu: thiếu CanvasRenderer của badge; tỷ lệ 9-slice không phù hợp; MP đầy bị native auto-cast tiêu hao ngay. Đã sửa và chạy lại suite 317 ca không lỗi. Build đầu tạo APK nhưng một lệnh dò hộp thoại không được Pipeline hỗ trợ bị tính là 1 error; đã lưu báo cáo và **build lại sạch 0 error**. Không gửi lệnh vào Editor trong lần build lại.

Computer Use đã dùng để xem trạng thái cửa sổ build theo cách chỉ đọc; không thao tác Inspector/chuột. Theo yêu cầu mới của chủ dự án, đã đóng phiên capture/reset runtime và ngừng dùng Computer Use. Các screenshot nghiệm thu đến từ Unity ScreenCapture.

Cache URP/TMP phát sinh khi build đã sao lưu ở `Temp/UIUXPolishBuildGenerated` và phục hồi các file vốn sạch về nội dung ban đầu, không đưa hàng loạt cache vào commit. Các thay đổi người dùng có trước được giữ nguyên. Dừng sau lần polish này.

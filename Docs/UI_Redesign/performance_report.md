# Phase 10 — Performance report

Ngày: 2026-11-13T17:46:13+07:00. Bằng chứng: [Phase10 checkpoint](Backups/Phase10-20261113-101509Z/checkpoint.json).

Đã giảm chi phí cập nhật luyện thể và số Graphic nhận raycast trong túi đồ. Đã cấu hình nén texture riêng cho Android/iOS. **Chưa xác minh FPS, nhiệt, pin, bộ nhớ player hoặc chất lượng nén trên thiết bị thật.**

## Môi trường và cách đo

Unity 6000.6.4f1, URP 17.6.0, uGUI 2.6.0, Pipeline 0.9.1-exp.1. Target đang mở StandaloneWindows64, D3D11; RTX 4060 Laptop, i5-13500HX, RAM 16.1 GB. Android và iOS BuildPipeline support đều false; không chuyển target hoặc đổi package/PlayerSettings/quality.

`MobileUIOptimizationChecks` chạy trong Editor Play Mode với profile `ui-optimization-p10-qa`, raster 1080×1920, cùng fixture 64 kiếm (10 trang bị), sáu nhánh luyện thể cấp 20, tiền 1e9, cùng stage. Time scale 0 để đóng băng thế giới, UI dùng thời gian unscaled. Kết quả chỉ đại diện fixture UI này; không đại diện combat đang chạy. Mỗi cửa sổ đo 180 frame thực sự tiến triển, bảy tình huống, **1.260 quan sát cho mỗi lượt**. `before`, `after` (cache layout), `final` (thêm raycast) đều được giữ; khoảng frame quan sát tối đa là 1 trong các lượt này.

Stopwatch đo đồng bộ lời gọi `Economy.AddLinhThach(128)` cùng mọi subscriber, hoặc `BodyTemperingPanelUI.Refresh()` trực tiếp. Các callback dirty được gắn trước cửa sổ đo và tháo sau đo. ProfilerRecorder dung lượng 1.024; sample đầu/đuôi có thể bằng 0, số sample thực trong JSON. Counter và đơn vị lấy từ Editor đang chạy. Các mảng thô, p95, max và frame gap nằm trong [before-profile.json](Backups/Phase10-20261113-101509Z/before-profile.json), [after-profile.json](Backups/Phase10-20261113-101509Z/after-profile.json), [final-profile.json](Backups/Phase10-20261113-101509Z/final-profile.json).

`GC.GetAllocatedBytesForCurrentThread()` trả toàn 0 trong môi trường này, nên không dùng làm bằng chứng không cấp phát. Dùng counter `GC Allocated In Frame` cho số liệu GC toàn Editor; có cả chi phí harness/Editor và không tách riêng feature. Không chuyển Main Thread time thành FPS hay quy mức giảm callback thành mức tăng FPS.

## Tối ưu đã đo

| Tình huống | Callback median trước (ms) | Sau (ms) | Giảm thời gian callback | Layout dirty trước → sau | Vertex dirty trước → sau |
|---|---:|---:|---:|---:|---:|
| body-currency-10frames | 0.6156 | 0.4669 | 24.2% | 72 → 36 | 288 → 217 |
| body-currency-every-frame | 0.6223 | 0.4798 | 22.9% | 720 → 360 | 1260 → 540 |
| body-refresh-direct | 0.2805 | 0.1086 | 61.3% | 360 → 0 | 747 → 25 |

`BodyTemperingPanelUI` dựng geometry một lần cho mỗi `centerModalBox`; vẫn cập nhật đủ title, tier, lore, trạng thái khóa, số liệu hiện tại/kế tiếp và x1/x5/max mỗi lần có event. Chiều cao mechanics được tính lại khi text, chiều rộng hoặc font size đổi. Title mobile được tạo trực tiếp ở cỡ chữ responsive, tránh viết thẻ size 20 rồi bỏ bằng regex. View cũ trong scene không có reconstruction giữ cách định dạng cũ. Không cache cost, unlock hoặc dữ liệu gameplay.

Túi đồ 64 kiếm: active Graphic giữ **551**, active raycast target **187 → 112** (giảm 75, 40,1%). Chỉ `Ribbon`, `CountBadge`, `IconBadge` mất raycast; Image nền/target của item, Button, ScrollRect và backdrop giữ nguyên. Đây là giảm số target đã đo, không phải bằng chứng input latency giảm 40%.

## Counter toàn Editor

Các giá trị dưới đây là median, đơn vị ms trừ GC bytes/frame. Scene view, Editor, renderer và nhịp chạy nền ảnh hưởng counter; các cửa sổ không phải benchmark player. Phép đo đồng bộ callback ở trên là bằng chứng chính cho thay đổi CPU.

| Tình huống | Main Thread trước → sau | GPU Frame Time trước → sau | Canvas.BuildBatch trước → sau | UpdateBatches trước → sau | GC bytes/frame trước → sau |
|---|---:|---:|---:|---:|---:|
| hud-idle | 42.153 → 11.541 | 2.470 → 1.997 | 0.009 → 0.010 | 0.050 → 0.052 | 15915 → 15859 |
| body-idle | 11.039 → 11.201 | 1.978 → 2.034 | 0.009 → 0.010 | 0.108 → 0.114 | 15859 → 15859 |
| body-currency-10frames | 10.999 → 11.319 | 1.952 → 2.051 | 0.010 → 0.011 | 0.144 → 0.152 | 15859 → 15859 |
| body-currency-every-frame | 12.213 → 12.212 | 2.050 → 2.117 | 0.010 → 0.011 | 0.219 → 0.169 | 28397 → 22303 |
| body-refresh-direct | 12.233 → 11.566 | 2.011 → 2.104 | 0.010 → 0.011 | 0.199 → 0.112 | 26529 → 20435 |
| storage-64-idle | 42.162 → 12.030 | 3.034 → 2.152 | 0.010 → 0.011 | 0.377 → 0.413 | 15953 → 15859 |
| hud-motion-stress | 42.002 → 11.384 | 2.902 → 1.985 | 0.009 → 0.010 | 0.050 → 0.056 | 15957 → 15859 |

GPU counter có sample khác 0 trên D3D11. Không có phép đo GPU native Android/iOS. `SetPass Calls Count` Render là counter toàn Editor (median 143 ở fixture), không phải draw call UI riêng. Các counter `Batches Count`/`Draw Calls Count` được liệt kê dưới category UI Toolkit nên bị loại khỏi kết luận uGUI.

## Animation

Trong stress nhấn nút/accent, marker `KV.UI.Motion`: 147 sample có hoạt động, p95 toàn cửa sổ 0.0066 ms, max 0.0288 ms. Marker chỉ đo bước motion, không bao gồm factory, layout hoặc GPU. Kiểm tra sau 0,8 giây xác nhận press và accent tắt component lúc nghỉ. Giữ giới hạn sáu accent/khoảng 0,6 giây và cơ chế hủy khi đóng/mở nhanh; không thêm vòng Animator hoặc hiệu ứng mới.

## Texture và bộ nhớ

Audit 36 texture UI/SecretRealm, 27 PNG generated, trong đó 25 V5. Generated không bật Read/Write hay mipmaps. V5 đã giới hạn 256–512; giữ kích thước, PPU, alpha, sRGB, filtering, GUID, sprite rect và border. Không giảm tiếp độ phân giải. So sánh audit trước/sau xác nhận các trường này không đổi. PNG không đổi hash. Giữ portrait RGB vốn đã Compressed và chín texture SecretRealm ngoài phạm vi generated.

Qua `TextureImporter.SetPlatformTextureSettings`, 26 texture RGBA có override Android `ETC2_RGBA8`, iOS `ASTC_4x4`, quality 100, Crunch false, max size bằng thiết lập hiện có. API xác nhận format hợp lệ cho Sprite và đọc lại cả 52 override thành công. Android hiện cấu hình Vulkan/OpenGLES3; iOS minimum 15.0. Chọn block 4×4 cho hoa văn/alpha sắc hơn các block ASTC lớn hơn, nhưng chất lượng phải được kiểm trên build thật. Tham chiếu [Unity GPU texture formats](https://docs.unity3d.com/6000.0/Documentation/Manual/texture-formats-reference.html) và [MobileTextureSubtarget](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MobileTextureSubtarget.html).

Ước tính payload chỉ của 26 texture, dùng kích thước import hiện tại: RGBA32 **22,807,184 byte (21.75 MiB)**; block 4×4×16 byte **5,712,448 byte (5.45 MiB)**, giảm lý thuyết 74.95%. Đây là **ước tính**, chưa có native import, dimension padding, fallback, driver allocation hoặc residency player. Không cộng thẳng mức giảm này vào RAM của ứng dụng. Native alpha/gradient/9-slice và texture không có kích thước chia hết 4 cần kiểm lại khi cài module build.

| PNG | Nguồn px | Import desktop px | Max | Runtime memory Editor MiB | Override Android / iOS |
|---|---:|---:|---:|---:|---|
| ImmortalRealmPortrait.png | 941×1672 | 941×1672 | 2048 | 10.504 | Giữ Automatic, Compressed |
| JadeGoldAtlas.png | 1254×1254 | 1254×1254 | 2048 | 11.998 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_AlchemyDecoration.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_CultivationHalo.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_EquipmentSlot.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_FormationDecoration.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_GoldJadeBorder.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_HudContainer.png | 2172×724 | 512×171 | 512 | 0.669 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_NavigationButton.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_PanelBackground.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_PopupFrame.png | 1024×1536 | 341×512 | 512 | 1.333 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityBlue.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityGold.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityGreen.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityNeutral.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityOrange.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RarityViolet.png | 1254×1254 | 256×256 | 256 | 0.501 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_ResourceCoDuyen.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_ResourceMedicineDust.png | 1254×1254 | 512×512 | 512 | 1.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_ResourceOre.png | 1254×1254 | 512×512 | 512 | 1.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_ResourceSpiritStone.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_ResourceTrack.png | 2172×724 | 512×171 | 512 | 0.669 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_RewardFrame.png | 1024×1536 | 341×512 | 512 | 0.667 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_SectionDivider.png | 2172×724 | 512×171 | 512 | 0.669 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_SelectionFrame.png | 1254×1254 | 512×512 | 512 | 2.001 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_TabBackground.png | 2172×724 | 512×171 | 512 | 0.335 | ETC2 RGBA8 / ASTC 4×4 |
| JadeV5_UpgradeButton.png | 2172×724 | 512×171 | 512 | 0.669 | ETC2 RGBA8 / ASTC 4×4 |

`Profiler.GetRuntimeMemorySizeLong` có thể tính thêm bản sao/object của Editor; không dùng cột trên làm VRAM chính xác. Audit trước toàn Editor allocated 1593.6 MiB, reserved 3292.4 MiB, managed used 101.8 MiB. Audit đã chủ động load texture; các sample là snapshot tổng Editor, không phải player peak hoặc kiểm tra leak. Native Memory Profiler snapshot, cold start, peak khi đổi screen và device low-memory: **NOT TESTED**.

## Sprite Atlas, trùng lặp, overdraw và layout

- Không có `.spriteatlas`/`.spriteatlasv2`; Sprite Packer Disabled. `JadeGoldAtlas.png` là texture chia sprite cũ, không phải SpriteAtlas asset. Không có script gắn tag UNITY-SKILL:SPRITEATLAS.
- Đã đánh giá nhóm icon/rarity/slot V5 dùng chung làm ứng viên atlas Built-in, rotation/tight packing phải tắt và padding phải bảo vệ 9-slice. Chưa có counter draw call uGUI riêng/native hoặc pack/build A/B để chứng minh lợi ích. Không thêm atlas, bật packing toàn project hoặc đổi delivery/package trong Phase10. Giữ atlas là hạng mục benchmark sau khi có native build; không đánh dấu tối ưu atlas PASS.
- 27 PNG không có nhóm SHA-256 trùng nhau. Giữ cả phiên bản cũ vì scene/prefab lịch sử và PC còn dependency; không xóa asset.
- Có 28 LayoutGroup trong scene kể cả inactive, 13 cùng object có ContentSizeFitter; tại màn edit ban đầu chỉ 1 group active. Không có dirty callback trong cửa sổ HUD/body/storage idle. Không tháo LayoutGroup cần cho text/scroll responsive chỉ vì tồn tại component.
- Geometry rectangle coverage là proxy trên màn hình, không đo fragment overdraw: HUD khoảng 1.15×, body modal 7.84×, storage 5.80×. Proxy có thể tính cả phần bị mask và không xét lỗ alpha/occlusion. Modal nhiều lớp là điểm cần đo trên GPU mobile; không tắt lớp/camera khi chưa có bằng chứng và kiểm tương đương.
- Không chia Canvas mới trong Phase10: chi phí BuildBatch hiện nhỏ trong fixture, không có bằng chứng cần đánh đổi thêm batch/mask để tách Canvas. Cấu trúc, scene và prefab giữ nguyên byte so với checkpoint Phase10.

## Giới hạn giao hàng

Build Android/iOS, FPS thiết bị, thermal soak, pin, overdraw fragment, UI draw calls riêng, memory leak/peak player, screenshot native nén ETC2/ASTC và device input latency: **NOT TESTED**. Safe Area Simulator và kiểm tra bốn raster được báo trong final delivery; chúng không thay thế benchmark native. Ngân sách đề xuất 16,67 ms ở 60 FPS hoặc 33,33 ms ở 30 FPS chưa phải SLO được chứng nhận trên thiết bị.

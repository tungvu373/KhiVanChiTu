# Jade & Gold — Mobile UI Redesign V2

Ngày triển khai: 12/11/2026. Scene: `Assets/Scenes/SampleScene_MobileWork.unity`.

Đã triển khai giao diện uGUI trong scene đang mở: HUD, dock 5 tab, drawer, menu chính/chọn/tạo nhân vật/cài đặt, danh sách luyện thể/pháp khí/trận pháp, vùng cuộn luyện đan, kho đồ, vận khí, thành tựu, bí cảnh, các khung modal và HUD kỹ năng. Dữ liệu và hành động vẫn do controller/service hiện có xử lý.

**Trạng thái nghiệm thu:** bản triển khai đã compile và kiểm thử các luồng chính trong Unity Editor. Kiểm tra reference và listener lưu trong scene đạt. Chưa tuyên bố nghiệm thu 100% mọi trạng thái trên thiết bị Android/iOS thật; các trường hợp chưa kiểm tra được ghi ở cuối tài liệu.

## Môi trường và phạm vi

- Unity **6000.6.4f1**, URP **17.6.0**, uGUI **2.6.0**, Input System **1.20.0**, Pipeline **0.9.1-exp.1**.
- CLI kết nối Editor đang mở tại port 7800, chạy từ project root. Không mở Editor thứ hai.
- Canvas reference **1080 × 1920**, scale theo chiều rộng. `SafeAreaFitter` hiện có tiếp tục sở hữu safe area.
- Không sửa scene/prefab YAML bằng text. Scene được thay đổi/lưu qua Editor APIs; prefab được tạo bằng `PrefabUtility`.
- Không sửa module PC, công thức gameplay, giá, drop rate, schema save hoặc danh sách scene build.
- Git branch: `codex/mobile-ui-jade-v2`. Chỉ checkpoint file thuộc redesign; các thay đổi có sẵn của người dùng được giữ riêng.

## Các giai đoạn 00–10

Prompt không định nghĩa tên từng phase, nên các số được ánh xạ thành các gói bàn giao dưới đây. Mỗi dòng ghi triển khai, file, xác minh và phần còn lại.

| Phase | Triển khai / tài liệu | File thay đổi chính | Xác minh | Còn lại |
|---|---|---|---|---|
| 00 | Kiểm kê hierarchy, component, reference, UnityEvent; sao lưu scene | `baseline.json`, `hierarchy-before.tsv`, `source-bindings.txt`, `JadeUIRedesignAudit.cs`, scene backup | 2.187 component gốc, 3.524 object reference, 5 persistent listener được ghi trước khi sửa | Listener runtime đối chiếu thêm bằng source và thao tác thực tế |
| 01 | Design tokens jade/gold/ivory, font tiếng Việt, khung và trạng thái nút dùng chung | `JadeUITheme.cs`, `JadeUIAdaptiveSkin.cs` | Màu/trạng thái chi phí/khóa vẫn do controller cũ cập nhật | Đánh giá tương phản ngoài trời trên máy thật |
| 02 | Tạo ảnh gốc, import sprite alpha/9-slice, prefab bề mặt tái sử dụng | `Art/UI/Generated/JadeGoldAtlas.png`, `ImmortalRealmPortrait.png`, 6 prefab | 16 sprite con trong atlas + ảnh nền portrait; ảnh không chứa số liệu/text động | Kiểm tra texture memory/overdraw khi profiling thiết bị |
| 03 | Sắp lại profile, tiền tệ, nhiệm vụ, tầng, HP/MP và thông tin tu vi | `JadeUIRedesignTool.cs`, `JadeUIRuntimeLayout.cs`, `CultivationManager.cs`, `CultivationPhaseBarUI.cs`, mobile scene | Tên Việt dài, số tiền lớn, 6 viewport portrait, inset iPhone được kiểm tra | Font/accessibility ở mức phóng lớn hệ điều hành |
| 04 | Dock 5 tab, selected frame, drawer 55% safe-height, thứ tự lớp, menu phụ | mobile scene, `MobileDrawerUIController.cs`, `MobileUINavigationManager.cs`, `JadeSecondaryMenu.cs` | Open/switch/close, backdrop, Back, swipe; raycast dock và nút đóng; animation khi timeScale=0 | Touch thật nhiều ngón/gesture hệ thống |
| 05 | Kiếm lô 5 cột, hai hàng xuất chiến, kho cuộn và toolbar | mobile scene, `JadeUIRedesignTool.cs`, `JadeUIRuntimeLayout.cs` | Drag merge hợp lệ, từ chối cấp khác, phân trang trước/sau, sắp xếp và route ghép nhanh | Kiểm thử kéo nhiều ngón và đầy toàn bộ kho trên thiết bị |
| 06 | Card có nút nâng lớn, nội dung luyện đan vào ScrollRect, cây luyện thể giữ gate | mobile scene, `BodyTemperingPanelUI.cs`, `JadeUIRedesignTool.cs` | Nâng luyện thể/pháp khí, học/chọn trận, nấu đan/timer/tốc hành, cập nhật kho/chi phí | Ma trận mọi cấp max, mọi phẩm đan và mọi gate |
| 07 | Menu chính và 3 save slot dọc; dialog tự fit; kho, vận khí, thành tựu và bí cảnh | mobile scene, `JadeUIAdaptiveSkin.cs`, `JadeModalFit.cs`, `JadeModalPresentation.cs` | Tạo tên rỗng, tạo tên dài, resume save QA, route modal, đổi kho ô/danh sách/filter/scroll | Toàn bộ xác nhận xóa, phân giải quý, mọi kết quả độ kiếp/bí cảnh |
| 08 | Press feedback unscaled, theme nội dung sinh động, icon kỹ năng và popup kiếm pháp | `JadeButtonFeedback.cs`, `JadeUIStyleScope.cs`, `JadeUITheme.cs`, `GameplayChoicesService.cs` | Giữ Image fill/cooldown; kiếm pháp nhận raycast, mở/chọn và ẩn dưới bí cảnh; UI lúc pause | Cast từng kỹ năng ở mọi trạng thái cooldown/lock trên máy thật |
| 09 | QA profile riêng, kiểm tra raycast, nhiều viewport, Device Simulator, đối chiếu contract | `JadeUIRuntimeChecks.cs`, `runtime-checks.tsv`, `notch-simulator.json`, `binding-verification.txt` | Kết quả chi tiết bên dưới; hash save người chơi được đối chiếu | APK/iOS build và test thật; không có kết quả physical-device |
| 10 | Lưu scene, prefab/meta, ảnh, checklist, hướng dẫn chạy lại và Git checkpoint | Tài liệu này, `FEATURE_PARITY.md`, `changed-files.txt`, ảnh và checkpoint Git | Compile/console cuối; scene không chứa marker QA sau khi dừng test | Các hạng mục chưa xác minh không được coi là đã pass |

## Hướng hình ảnh và tài nguyên

Palette: midnight teal `#081C22`, jade surface `#0A1F22`, ivory `#F1E8CF`, gold `#D5B574`, jade accent `#6EDDB6`. Font dùng `UiTextStyle.BodyFont` / Source Sans Pro hiện có để giữ dấu tiếng Việt và reference `Text` cũ.

Atlas gốc được imagegen tạo cho dự án; Sprite Editor Data Provider import 16 sprite: Panel, Window, Slot, SelectedSlot, PrimaryButton, SecondaryButton, ResourceBar, Divider, Sword, Body, Alchemy, Artifact, Formation, SpiritStone, SpiritFlame, CultivationHalo. Border của panel/window/slot/button/bar là 9-slice. PNG nguồn được giữ nguyên; không vẽ số dư/giá/timer vào ảnh.

Sáu prefab trong `Assets/Art/UI/Generated/Prefabs/` là **bề mặt trang trí tái sử dụng**, không phải các controller gameplay thay thế. Scene/controller giữ nguyên các nút và event cũ.

## Điều hướng và dữ liệu

- Kiếm Lô, Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp giữ đúng thứ tự controller index 0–4.
- `Tiện ích` mở các handler Kho đồ, Vận khí, Bí cảnh, Thành tựu hiện có.
- Hai shortcut kho/thành tựu nhỏ trùng trong quest bar được gom vào menu phụ; component và listener gốc vẫn giữ, không xóa.
- Quest giữ nút Mốc/Nhận và Gọn/Mở. Toggle phụ trùng lặp được ẩn.
- Back xử lý modal trên cùng trước; bí cảnh hub được bổ sung vào bộ điều hướng hiện có. Drawer vẫn dùng toggle tab, nút Đóng, backdrop và swipe handle.
- Text/Image/controller gốc giữ nguyên type và reference. `CultivationManager` chỉ đổi format trình bày khi có `JadeUITheme`; tên, realm và tu vi vẫn lấy từ dữ liệu đang chạy.
- `JadeUIStyleScope` chỉ lên lịch skin khi hierarchy thay đổi. Không polling wallet và không rebuild toàn danh sách mỗi frame từ theme.
- `JadeModalFit` giữ dialog trong vùng khả dụng; modal builder cũ được thích nghi sau OnEnable. Modal surface nhận raycast để click nền trong hộp không xuyên xuống trận.

## Kết quả đã kiểm tra

`runtime-checks.tsv` ghi các case đã thực thi. Những test button gọi handler UnityEvent thật; raycast được kiểm tra riêng bằng GraphicRaycaster. Drag/drop và swipe được phát qua PointerEventData trong Editor, không phải thao tác tay trên điện thoại.

- Game loop được xác nhận tăng `Time.frameCount`, Pipeline báo `playerLoopTicking: true`.
- Main menu → settings → select → create; tên rỗng bị chặn; tên `Nguyễn Thiên Vân Trường Phong` được tạo và nạp lại từ save QA.
- 5 tab; đóng bằng nút, backdrop, Back và swipe. Mở và vuốt đóng drawer chạy được khi `timeScale = 0`.
- Giữ time scale `.4` khi mở/đóng menu cài đặt; UI không chiếm quyền thay đổi slow motion.
- Kiếm: merge 2 kiếm Lv1 qua drag/drop thành Lv2; thả Lv1 lên Lv2 bị trả về đúng ô; trước/sau trang, sort, ghép nhanh.
- Luyện thể: mở chi tiết, nút nâng nhận raycast và tăng đúng một level, đóng modal.
- Pháp khí: nút nâng nhận raycast và tăng đúng một level. Trận pháp: học/nâng và chọn trận cập nhật service.
- Luyện đan: bắt đầu nấu, timer còn lại được đọc thực tế; dùng bụi hoàn tất, trạng thái brewing kết thúc và inventory binding cập nhật.
- Kho đồ: tab kiếm, grid/list, cuộn và Back. Vận khí, thành tựu, bí cảnh: mở từ menu và đóng bằng Back.
- Kiếm pháp: nút 132 px nhận raycast, mở popup, chọn Phá Giáp cập nhật service; kiểm tra nút này ẩn khi mở bí cảnh và nút Vào Ải nhận raycast. Chỉ thay presentation, không đổi công thức hai kiếm pháp.
- Dock/nút đóng kiểm tra GraphicRaycaster tại **1080×1920, 720×1280, 1080×2340, 1080×2400, 390×844, 360×640**.
- Device Simulator **Apple iPhone 13 Pro**, render **1170×2532**, safe area **(0,102,1170,2289)**; anchor y kỳ vọng **0.04028436 / 0.9443128**, khớp `SafeAreaFitter`; dock/nút đóng nhận raycast.
- Save validation chỉ dùng `jade-ui-qa`. Marker chỉ tồn tại khi preview; được xóa khi trở lại Edit Mode. Hash key `KhiVanChiTu_SaveData` gốc được đối chiếu, không đổi.
- `binding-verification.txt`: mọi component gốc và các reference/controller contract/persistent listener được đối chiếu sau khi lưu. Đây là kiểm tra cấu trúc, không thay cho toàn bộ kiểm thử feature.
- `check_scene_scope.py` chỉ đọc YAML, không ghi scene: đối chiếu **290 serialized object ngoài Canvas**, không có object bị đổi hoặc mất. Kết quả tại `scene-scope-verification.json`.

## Chạy lại và phục hồi

1. Mở mobile work scene. Dùng menu **TuTien / UI / Jade V2**.
2. `09 Verify bindings` đối chiếu với baseline. Không ghi đè `baseline.json` bằng bản đã redesign.
3. `07 Start isolated QA` lưu scene, thêm marker tạm và vào Play Mode bằng profile riêng. Đi qua menu chọn slot QA hiện có.
4. `QA Check dock and drawer hits` kiểm tra các vùng nhận chạm hiện tại. `JadeUIRedesignAudit.SetResolution(width,height)` chọn Fixed Resolution để tránh ảnh aspect bị upscale.
5. Dừng Play Mode. Cleanup tự xóa marker; menu `08 Cleanup QA profile marker` cũng cho phép dọn lại an toàn. Không lưu marker QA vào bản phát hành.
6. Scene gốc giữ tại `Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity`; bản gốc cũng còn trong lịch sử Git. Không mở đè/merge backup qua scene Android khác.

## Các trường hợp chưa xác minh / giới hạn

- Chưa build/chạy trên điện thoại Android hay iPhone thật; chưa xác minh bàn phím ảo, gesture hệ thống và hành vi pause/focus của OS.
- Chưa chạy trọn ma trận mọi trạng thái khóa/max/thiếu tài nguyên, mọi phẩm đan, mọi nhánh talent, mọi skill cast/cooldown, bảo vệ vật phẩm quý, batch dismantle và mọi kết quả độ kiếp/bí cảnh.
- Provider quảng cáo ngoài chưa được kết nối trong lần kiểm tra; nút nhân đôi thưởng phản ánh trạng thái chưa sẵn sàng. Không tuyên bố test quảng cáo thật, IAP, network hay backend mới.
- Ảnh QA dùng dữ liệu profile riêng; tài nguyên rất lớn là fixture, không phải số dư của người chơi và không phải giá trị bake vào ảnh.
- Chưa đo CPU/GPU/memory trên thiết bị; không tuyên bố tăng FPS. Cảnh chiến đấu và VFX gameplay gốc nằm ngoài phạm vi chỉnh mỹ thuật này.
- Các warning có sẵn của project cần được phân biệt với compile/exception mới; xem console cuối và log kiểm tra.
- Khi chuyển từ Device Simulator về Game View, đã vào lại Play Mode để làm mới render surface; ảnh cuối 1080 được chụp ở phiên có Canvas 1080×1920, scale factor 1. Đây không thay cho thử đổi viewport/OS orientation trên thiết bị thật.

## Ảnh đối chiếu

- [Menu chính](menu-runtime-1080.png)
- [Kiếm lô](sword-final-1080.png)
- [Luyện thể](body-final-1080.png)
- [Luyện đan](alchemy-final-1080.png)
- [Kho đồ](storage-final-1080.png)
- [Vận khí](fate-final-1080.png)
- [iPhone 13 Pro, notch và drawer](iphone13pro-drawer.png)

`WorkInProgress/` lưu ảnh trong quá trình sửa, không mặc định là trạng thái cuối và được loại khỏi checkpoint. `before.png` là ảnh trước khi sửa. Kết quả code/contract và phạm vi chưa kiểm tra ở trên có giá trị nghiệm thu cao hơn ảnh đơn lẻ.

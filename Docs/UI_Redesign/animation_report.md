# Phase 08 — Premium UI animation

Đã tích hợp motion vào **Assets/Scenes/SampleScene_MobileWork.unity**, dùng coroutine uGUI hiện có và các component nhẹ. **9 preset runtime `.asset`** nằm trong [Profiles](../../Assets/UI/Animation/Profiles), kèm **9 AnimationClip `.anim`** trong [Clips](../../Assets/UI/Animation/Clips). Canvas tham chiếu các preset qua MobileUIAnimationDirector. [Metadata asset đầy đủ](animation_assets.json).

Runtime đọc duration/easing của preset. Clip `Preview_*` là bản preview riêng cho Animation Window/visual mẫu, không phải Animator gắn vào nút sống. Clip không loop, không có AnimationEvent. Không đưa callback gameplay hoặc cấp thưởng vào lúc animation kết thúc.

## Ngôn ngữ chuyển động

Theo design tokens Phase 03: ease-out không overshoot; modal 140/120 ms, drawer 200/180 ms, press 70/100 ms, reveal 320 ms, progression accent 180 ms. Thời gian dùng `Time.unscaledDeltaTime`; UI vẫn hoạt động khi timeScale=0. Không thêm thư viện tween hoặc package.

| Preset | Thời lượng | Áp dụng |
|---|---:|---|
| `ButtonDown` | 70 ms | Co phần chữ của nút tới .97; RectTransform vùng bấm giữ nguyên. |
| `ButtonUp` | 100 ms | Trả phần chữ về scale nghỉ từ pose hiện tại. |
| `ModalClose` | 120 ms | Fade ra + scale khung trang trí 1→.98; nhả raycast/input ngay khi đóng. |
| `ModalOpen` | 140 ms | Fade nội dung + scale khung trang trí .98→1; input sẵn sàng ngay. |
| `ProgressAccent` | 180 ms | Thông số 180 ms cho accent cảnh giới; không trì hoãn text/fill nguồn. |
| `RarityAccent` | 320 ms | Một lượt opacity .55→1 trên frame rarity có sẵn, tối đa 6/lượt. |
| `RewardReveal` | 320 ms | Reveal 320 ms trên dữ liệu reward đã có; không cấp thưởng từ animation. |
| `SheetClose` | 180 ms | Đóng drawer; đổi tab dùng nửa thời lượng close rồi mở tab mới. |
| `SheetOpen` | 200 ms | Drawer đi từ ClosedY theo chiều cao thật đến 0; nhận ý định tab mới nhất. |

Popup native (Kho, Nhân vật, Thành tựu, Phân giải, Offline reward, Độ kiếp) dùng enter/exit chung, hủy thế hệ cũ khi bị ngắt. Các panel đơn giản có JadeModalPresentation nhận enter fade; controller vốn đóng ngay bằng SetActive vẫn giữ hành vi đóng đó. Chiến báo có hook reveal riêng vì hierarchy dùng `Card`. Thông báo đột phá/tử trận dùng fade yên tĩnh, thay đoạn scale bật mạnh; banner đặt blocksRaycasts/interactable=false.

**Layout và animation có kênh riêng.** Kiểm tra phát hiện adapter responsive ghi scale của popup trong lúc tween. Bản cuối giữ layout sở hữu popup RectTransform; chỉ `P6Frame` không chứa Selectable được scale .98→1. Với visual không có khung phù hợp, motion chỉ fade. Vùng bấm và data binding không bị co theo hiệu ứng press. Native CanvasGroup được lấy/tạo bằng kiểm null của Unity.

Wallet và progression có timer riêng: chỉ tăng currency mới sáng line ví, tiêu tiền không phát hiệu ứng gain và không làm line cảnh giới nháy. Text đổi ngay từ event economy. Accent cảnh giới phát từ OnStageChanged; countdown/fill vẫn theo service/UTC hiện có. Frame rarity Lv8 thật được kiểm với sprite tương ứng và raycastTarget=false. Rarity reveal chỉ một lượt, tối đa 6 frame cùng batch và cooldown batch 0,6 giây; khi nghỉ component ngừng Update.

## Input, pause và thao tác nhanh

- Nút chỉ nhận press khi Selectable đang interactable; pointer-up/exit và OnDisable trả visual về nghỉ. Không thêm listener giao dịch hoặc sound click thứ hai.
- Native popup cập nhật cờ input/raycast ngay lúc nhận yêu cầu. Phiên tween cũ không được ẩn popup vừa mở lại. Disable giữa chừng khôi phục pose, lần mở tiếp theo vẫn hiển thị/nhận input.
- Drawer giữ một owner chuyển động và hàng đợi ý định cuối. Tap tab trong lúc slide không bị bỏ qua; toggle cùng tab vẫn giữ đúng parity. Back/close trong lúc mở được xử lý sau transition hiện tại. Disable hủy transition thuộc drawer và dọn backdrop.
- Reward/confirmation vẫn do controller/service hiện có xử lý. Kiểm bấm xác nhận trong lúc đang animate cho callback đúng một lần; reveal reward không tự cấp thêm tiền.
- `MobileUIAnimationDirector.reducedMotion` là tùy chọn presentation trong Inspector, chưa thêm setting/save UI mới. Chế độ này dùng fade 80 ms, bỏ press/frame scale và rarity glow; drawer snap tới vị trí đích. Gameplay/world VFX không thuộc tùy chọn này.

## Kiểm chứng

**46/46 ca cuối đạt, 287 assertions.** Các ca đo pose giữa animation và trạng thái sau settle, thay vì chỉ chụp một frame cuối. Full suite chạy 360×640 và 390×844; DataManager dùng profile riêng `ui-motion-p08-qa`, frame counter tiến thật. Simulator chạy profile 1170×2532, safeArea=(0,102,1170,2289): close control nằm trong safe area trong khi fade, tab mới nhất đúng và gameplay clock không đổi. Module Simulator cung cấp inset thật, không gán safeArea giả.

Các ca gồm press nhanh/disabled/disable-restore; 8 lượt popup ngắt qua lại; reopen sau close và disable; 6 loại popup native; result reveal/cancel; notification interruption/exit khi pause; tab queue/toggle/close/disable; center raycast; low-motion popup/drawer; currency/progression độc lập; rarity từ 20 item Lv8, cap 6, sleep-at-rest và low-motion static. Khối assertion asset kiểm duration, endpoint và easing monotonic của 9 preset.

Ảnh Play đã xem: [Kho 360×640](Backups/Phase08-20261113-045800Z/storage-360.png), [Kho 390×844](Backups/Phase08-20261113-045800Z/storage-390.png), [Simulator HUD/drawer](Backups/Phase08-20261113-045800Z/simulator-hud.png). Ảnh dùng fixture QA, không phải progression production. Các retry/failure giữ trong evidence và không cộng vào số PASS cuối.

## Đo hiệu năng

Đã đo baseline trước sửa presentation, rồi đo lại trên cùng Game View **306×543**, timeScale=0, khoảng **120 frame thật**. Stress làm 12 opacity channel và một press channel, sau warm-up. Đây là mức opacity nhiều hơn cap rarity production.

| Counter | Baseline median | Stress median | Stress p95 | Stress max |
|---|---:|---:|---:|---:|
| Main Thread (Editor, ms) | 9.6441 | 8.9036 | 9.7341 | 9.9761 |
| GC Allocated In Frame (Editor, byte) | 15907 | 15907 | 16099 | 16237 |
| KV.UI.Motion (ms) | — | 0.0076 | 0.0093 | 0.0167 |

`KV.UI.Motion` chỉ bao các bước tween/ghi visual được đánh dấu; không bao trọn factory/list rebuild, Canvas layout/render hay Editor overhead. Main Thread/GC gồm toàn Editor và gameplay scene. Chênh lệch median giữa hai lượt không được coi là cải thiện FPS. Không tuyên bố toàn UI zero-GC hoặc bảo đảm 60 FPS trên điện thoại. Component opacity/scale ngủ sau khi hoàn tất; vẫn có allocation lúc yêu cầu coroutine hoặc tạo/cache component lần đầu.

Chưa build APK/iOS, chưa đo GPU/overdraw, cold-open mọi màn, pin/nhiệt hoặc thiết bị vật lý. Các số đo raw và percentile nằm trong `Backups/Phase08-20261113-045800Z/baseline-profiler.json`, `motion-profiler.json`, `performance-summary.json`.

## Assets và phạm vi mã

Nguồn runtime: `Assets/UI/Animation/Runtime/` gồm preset, scene director, modal pose, visual scale và finite accent. Nguồn tạo asset/QA: `Assets/UI/Animation/Editor/`. Điều chỉnh preset để tune đồng bộ duration/easing. Clip preview là snapshot tác giả của Phase 08; nếu tune preset thì preview không tự viết lại asset người dùng.

Scene thêm một MobileUIAnimationDirector, giữ toàn bộ hierarchy/object ID và UnityEvent cũ. Các adapter source được sửa trong phạm vi UI: JadeButtonFeedback, JadeModalPresentation, MobileDrawerUIController, PortraitHudController, StorageModalUI, DismantleConfirmModalUI, CharacterDetailModalUI, AchievementModalUI, OfflineRewardPopupUI, TribulationModalUI, CombatNotificationUI, SecretRealmPlayController. Scene chưa cài director tiếp tục nhánh legacy; PC module không được sửa/chạy lại trong phase này.

Các source thuộc gameplay, economy, save schema, package, build settings và art PNG/meta được đối chiếu bằng hash. Không đổi camera, hit-stop hoặc timeScale của gameplay. Kết quả bảo toàn cuối nằm trong `Backups/Phase08-20261113-045800Z/final-validation.json`.

## Chạy lại và rollback

Trên scene mobile đã lưu ở Edit Mode, qua Pipeline gọi `MobileUIAnimationChecks.Start("motion")`; suite reset đúng profile QA của nó. `Start("simulator")` kiểm inset và trả cửa sổ/profile Simulator về trạng thái trước. Harness khôi phục runInBackground, lựa chọn Game View, gỡ marker riêng và lưu scene. `Cleanup()` dùng khi một lượt bị gián đoạn và Editor đã về Edit Mode. Không dùng profile QA này làm save cá nhân.

Checkpoint trước phase: `Backups/Phase08-20261113-045800Z/baseline.json` và `ui-source-before.zip`. Rollback chọn lọc sau khi giữ lại mọi sửa đổi mới hơn: kiểm hash ZIP; phục hồi 12 script UI và scene từ checkpoint; compile xong rồi mới gỡ các asset/script Animation thuộc phase này nếu muốn. Không restore toàn repo hoặc ghi đè player saves. GUID scene và .meta cũ được giữ. Không stage/commit/reset/stash/clean.

## Từng ca cuối

| Case | Assertions | Kết quả |
|---|---:|---|
| `scene-assets-and-unscaled-clock` | 218 | PASS |
| `finite-channels-sleep-at-rest` | 1 | PASS |
| `press-visual-only` | 2 | PASS |
| `press-rapid-release` | 1 | PASS |
| `press-disabled` | 1 | PASS |
| `press-disable-restores` | 1 | PASS |
| `popup-intermediate-and-input` | 3 | PASS |
| `popup-close-reopen-latest-wins` | 3 | PASS |
| `popup-eight-interruptions` | 1 | PASS |
| `popup-external-disable-recovery` | 1 | PASS |
| `closing-releases-input-immediately` | 1 | PASS |
| `character-rapid-open-close` | 1 | PASS |
| `achievement-rapid-open-close` | 1 | PASS |
| `confirm-during-animation-exactly-once` | 1 | PASS |
| `reward-reveal-never-grants` | 3 | PASS |
| `result-reveal-intermediate` | 3 | PASS |
| `result-close-cancels-reveal` | 1 | PASS |
| `tribulation-rapid-open-close` | 1 | PASS |
| `tribulation-animation-does-not-consume-pill` | 1 | PASS |
| `breakthrough-banner-passive` | 2 | PASS |
| `notification-interruption` | 1 | PASS |
| `notification-unscaled-exit` | 1 | PASS |
| `tabs-latest-request` | 2 | PASS |
| `tabs-close-during-open` | 1 | PASS |
| `tabs-toggle-parity` | 1 | PASS |
| `tabs-disable-mid-transition` | 1 | PASS |
| `arena-raycast-after-transitions` | 1 | PASS |
| `low-motion-no-scale` | 1 | PASS |
| `low-motion-fade-settles` | 1 | PASS |
| `low-motion-tabs-snap` | 1 | PASS |
| `currency-immediate-data-and-gain-only-accent` | 3 | PASS |
| `currency-accent-rests` | 1 | PASS |
| `spend-is-not-gain` | 1 | PASS |
| `progression-accent-independent` | 2 | PASS |
| `progression-accent-finite` | 1 | PASS |
| `rarity-real-items-and-six-channel-cap` | 3 | PASS |
| `rarity-sleeps-after-reveal` | 1 | PASS |
| `low-motion-rarity-static` | 1 | PASS |
| `portrait-popup-360` | 2 | PASS |
| `portrait-popup-390` | 2 | PASS |
| `motion-never-owns-time-scale` | 1 | PASS |
| `simulator-real-safe-area` | 2 | PASS |
| `simulator-popup-close-safe-and-live` | 6 | PASS |
| `simulator-close-settled` | 1 | PASS |
| `simulator-rapid-tabs` | 1 | PASS |
| `simulator-no-gameplay-clock-change` | 1 | PASS |

Kiểm bảo toàn cuối: **27/27 file dữ liệu có sẵn giữ nguyên SHA-256**; production PlayerPrefs hash khớp baseline. Giữ đủ **1.691 object**, **5 persistent callbacks** và toàn bộ reference cũ. Diff scene chỉ thêm component MobileUIAnimationDirector cùng 9 reference preset trên Canvas; không thay serialized layout cũ. Editor dừng Play, scene sạch, không marker QA hay cửa sổ Simulator do suite tạo còn mở. Console hiện tại không có error; 2 cảnh báo TerrainCollider/MeshCollider được lưu trong log, không sửa physics ở phase UI.

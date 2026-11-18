# Foundation component guide

Namespace: KhiVanChiTu.UI.Foundation. All assets: Assets/UI/Foundation.

## Library

| Prefab | Component/API | Hành vi |
|---|---|---|
| PrimaryButton / SecondaryButton | FoundationAction.Bind(caption, Action, available) | Gọi command và UnityEvent clicked; disabled không chạy |
| ScreenHeader | FoundationAction | Back callback do screen adapter sở hữu |
| TabBar | FoundationTabs.Select(index), selected event, pages[] | Chọn tab, style, bật page tương ứng nếu được gán |
| ResourceDisplay / StatRow | FoundationValue.Set(label, value, optionalIcon) | Hiển thị dữ liệu snapshot, không tính economy/stat |
| ItemSlot / ItemCard | FoundationItem.Bind(...), SetSelected | Icon/level/count/description, click chọn và callback |
| ProgressBar | FoundationProgress.Set(current, maximum, caption) | Clamp fill anchor + nhãn; không sở hữu HP/MP |
| FullScreenPanel | FoundationPanel.Show(title, body, callback) | Scroll body, footer cố định, close/confirm/Back |
| BottomSheet | FoundationPanel | Surface từ đáy đến 72% safe height, backdrop chặn input |
| RewardModal | FoundationPanel | Chỉ emit accept callback; reward transaction do service adapter |
| ConfirmDialog | FoundationPanel | Accept/cancel riêng, backdrop không tự xác nhận |
| Tooltip / Toast | FoundationNotice.Show(message, seconds), Hide | Nonblocking raycast, timeout unscaled; seconds=0 hiện tới khi ẩn |
| Tooltip target | FoundationTooltipTrigger | Hover và touch hold; tự ẩn khi target disable |

Prefab button tự wire internal Execute trong OnEnable, tự detach trong OnDisable. Bind chỉ thay command do component sở hữu, không RemoveAllListeners trên button. Consumer không được vừa bind transaction vừa thêm cùng transaction vào clicked. Trong scene gameplay cũ, dùng adapter giữ nguồn dữ liệu/service cũ và một nguồn gửi command.

## Showcase và công cụ Editor

Menu TuTien > UI Foundation:

1. Create Library and Showcase: tạo theme/font/surface, 15 prefab và UIFoundationShowcase.unity. Không ghi đè prefab/scene đã tồn tại; scene được tạo additive rồi đóng, giữ scene đang làm việc.
2. Open Isolated Showcase: chỉ mở khi scene hiện tại không dirty và Editor không Play. Không tự save/discard user work.
3. Validate and Capture Three Sizes: chỉ chạy khi đang Play với duy nhất showcase scene. Từ chối MonoBehaviour ngoài foundation/Unity/TMP; kiểm callback/state/modal raycast/safe area, đổi ba Game View sizes rồi phục hồi preset cũ.
4. Ensure Showcase Camera: sửa riêng preview camera/sample icon trong showcase đang mở, lưu qua Editor.

Showcase chỉ thay DemoValue. Tăng progress, reset mẫu, đổi tab/chọn item, mở panel/sheet/reward/confirm và toast đều có phản hồi thật. Đóng modal không ghi data nhân vật; không có DataManager hoặc PlayerPrefs trong scene.

## Integration contract cho phase sau

Không kéo toàn bộ FoundationRoot vào Canvas cũ mà chưa xử lý navigation ownership. Giữ GUID/meta, callback arguments và state machine hiện tại. Có thể instantiate prefab widget dưới parent được chỉ định rồi bind adapter; FoundationFactory không tìm Canvas toàn project. Cần kiểm coroutine/service lifecycle trước khi di chuyển một screen. Phase 1 không migrate các màn gameplay hay đổi formula/drop/save schema.

TMP Essentials cần tồn tại trước khi builder tạo font. Không cần TMP Examples & Extras. Không thay UPM packages hoặc tự mở importer/installer tương tác từ builder. Các asset TMP được người dùng import trong phiên làm việc phải được giữ riêng khi review Git.

# 35 ảnh AI đã tích hợp vào game

Đã tạo 35 PNG nền trong suốt bằng `image_gen` và gắn vào scene mobile đang dùng, prefab kiếm, kho/chi tiết vật phẩm, HUD kỹ năng, tu luyện, điều hướng và phần thưởng. Đây là artwork thực trong game; không phải panel màu hoặc asset chờ thay sau.

- 10 hình kiếm cho cấp 1–10, giữ đúng các nhóm rarity hiện tại.
- 4 kỹ năng, 2 loại đan, 3 thảo dược, quặng, dược trần và hòm.
- 4 pháp khí, hình Luyện Thể, lò luyện đan, 4 hình trận pháp bổ sung, Linh Thạch, Cơ Duyên và cổng Bí Cảnh.

Bảng [art direction và ánh xạ](ART_DIRECTION.md), [nguồn/SHA-256/alpha của 35 ảnh](manifest.json). Ảnh gốc giữ nguyên byte sau khi tạo. Unity import Sprite/Single, giữ alpha/tỷ lệ, tối đa 512, không mipmap hoặc bản CPU-readable. Icon kỹ năng/nav lớn hơn trong vùng nút hiện có.

Kiểm thử thực tế qua Unity Editor đang mở, Pipeline và Play Mode với profile dùng một lần:

| Nhóm | Assertion đã chạy thành công |
|---|---:|
| UI + ảnh mới + hồi quy chung | 162 |
| Kho, chọn, ghép, nâng cấp, trang bị và lưu/tải | 48 |
| Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp | 739 |
| 5 Bí Cảnh, input gốc, kết quả và thưởng | 1257 |
| Ảnh thưởng theo cấp kiếm thực tế | 12 |
| Tổng | 2218 |

Hồi quy chung ở 1080×1920, 1080×2340, 720×1280, 1440×3200. Kiểm tra ảnh/subject chuyên biệt ở ba độ phân giải đầu. Sau hồi quy 5 Bí Cảnh, đối chiếu screenshot đã phát hiện icon kiếm thưởng chung chưa khớp cấp; đã bind lại theo swordLevel hiện có và chạy thêm 12 assertion trên tám hàng thưởng. Không sửa reward level hoặc cách tính thưởng. Bộ kết quả cuối không có exception runtime. 213 screenshot từ Unity ScreenCapture được lưu kèm evidence. Không dùng Computer Use hoặc desktop capture.

- [Test case có kết quả thực thi](QA_CASES.json), [tóm tắt máy đọc được](Evidence/summary.json).
- [Kiểm tra tham chiếu](Evidence/static-audit.json): không missing script/reference; đúng 35 sprite; prefab Variant có 10 ảnh; cap vẫn 10; không còn profile marker.
- [Bảo toàn mã/dữ liệu](Evidence/source-preservation.json): 218 file được kiểm tra hash; 315 document OnClick nguyên vẹn; chỉnh sửa có sẵn của người dùng được giữ lại. Save chính, backup và PlayerPrefs đều giữ nguyên sau từng bộ test.
- Các lần thử lỗi được giữ ở `Evidence/Attempts`: hai lần fixture giả định All không phân trang; một lần harness chạm trước khi animation mở Bí Cảnh hoàn tất. Đã kiểm tra đúng tab/phân trang gốc và chờ raycast target thực; không sửa gameplay để vượt test. PNG của lần thử lỗi được giữ local, không đưa vào Git.
- [Kết quả Android build](Evidence/android-build.json). APK kiểm thử được đóng gói từ workspace hiện tại, gồm cả các chỉnh sửa có sẵn; commit bộ ảnh chỉ chứa thay đổi của tác vụ này. Bản cuối build Succeeded, 0 lỗi, 37 cảnh báo; APK thực 125,615,014 byte (phân biệt với tổng build output gồm symbol). Chưa kiểm tra trên điện thoại Android thật; không suy diễn FPS/texture memory thiết bị từ Editor.

Ảnh giao diện thực tế:

- [Gameplay HUD](Evidence/global/icons-hud-1080x1920.png).
- [Kiếm Lộ](Evidence/global/icons-swords-720x1280.png).
- [Dược liệu và nguyên liệu](Evidence/global/icons-materials-720x1280.png).
- [Đan dược](Evidence/global/icons-pills-720x1280.png).
- [Luyện Đan](Evidence/global/icons-Alchemy-0-720x1280.png).
- [Pháp khí](Evidence/global/icons-Artifact-0-720x1280.png).
- [Trận pháp](Evidence/global/icons-Formation-0-720x1280.png).
- [Phần thưởng Bí Cảnh](Evidence/reward-icons/reward-level8.png).

Không sửa công thức, rarity, drop, chi phí, thời gian, chỉ số hay dữ liệu người chơi. Original MergeItem prefab vẫn giữ nguyên; mobile dùng native variant chứa bộ ảnh mới. World FlyingSword renderer/VFX vẫn thuộc hệ hiệu ứng hiện có, không nằm trong thay artwork UI này.

Áp dụng lại bằng menu **TuTien / AI Icons / Import and apply illustrated icon set**. Tool kiểm tra đủ PNG thật rồi cập nhật qua Unity Editor API. Thay đổi scene/prefab không được thực hiện bằng sửa YAML thủ công.

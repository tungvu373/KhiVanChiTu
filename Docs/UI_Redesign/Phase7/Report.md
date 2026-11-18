# Phase 7 — Complete Secret Realm UI Redesign

18/11/2026 · Unity 6000.6.4f1 · Pipeline 7800 · Scene `Assets/Scenes/SampleScene_MobileWork.unity`.

## Audit và phạm vi

Tên thực tế: **Vạn Ma Triều Dâng**, **Trảm Phá Cấm Chế**, **Đan Thần Kỳ Duyên**, **Vượt Lôi Trì**, **Kiếm Chủng Cổ Thần**. “Kiếm Chứng” trong đầu bài được đối chiếu và dùng lại tên hiện có của game.

Các controller: `SecretRealmModalUI`, `SecretRealmPlayController`; giao dịch/vào cửa/thưởng: `SecretRealmService`; save: `SecretRealmSaveState`. Cả năm game được sinh động trong cùng scene, không phải năm scene/prefab minigame độc lập. Đã audit dependency scene và prefab qua AssetDatabase, ghi tại [scene-audit.json](scene-audit.json); không tìm thấy prefab riêng đang phụ thuộc controller Bí Cảnh. Các scene backup, recovery, PC và showcase không được sửa.

| Game | Cơ chế thực tế được giữ nguyên |
| --- | --- |
| Vạn Ma Triều Dâng | 30 giây, mục tiêu 50; chạm yêu ma hoặc tuyệt kỹ quét tối đa 5; auto damage và spawn gốc. |
| Trảm Phá Cấm Chế | 20 giây, bia 1.000.000 HP; chạm gây 50.000 sát thương, có sát thương tự động. |
| Đan Thần Kỳ Duyên | Chọn một trong ba đỉnh theo sấm ngôn; index đúng do RNG gốc; không có đồng hồ đếm ngược thực sự. |
| Vượt Lôi Trì | 25 giây, mục tiêu 20 lôi tinh; chạm thu thập; vòng sấm hiện chỉ là telegraph/hiệu ứng, không có phép trừ điểm/HP được bổ sung. |
| Kiếm Chủng Cổ Thần | Ba lượt; kim dao động gốc, ngưỡng 24/82, thưởng kiếm 8/6/4; có thể kết thúc sớm khi đạt 8. Không có trạng thái thua riêng. |

Vào cửa dùng một lượt miễn phí mỗi ngày, sau đó 5 Cơ Duyên. Service không có khóa cảnh giới riêng từng game; UI không tự đặt điều kiện khóa mới. Trạng thái thiếu lượt/tiền được thể hiện rõ và khóa nút tham gia. Giới hạn ghép kiếm 10 không làm thay đổi bảng thưởng kiếm 4/6/8 của Bí Cảnh.

## Giao diện đã thực hiện

- Danh sách năm Realm Card có tranh thực, tên, mô tả, chế độ/thời lượng, thưởng đúng với service, kỷ lục, điều kiện và primary action. Thẻ tăng chiều cao theo nội dung; scroll portrait, điều hướng trực tiếp tới đúng thẻ, mở lại không nhân đôi.
- Màn chơi thống nhất header/exit, mục tiêu, hướng dẫn, timer và điểm nổi bật, progress 64 reference units, vùng tương tác chiếm hơn nửa chiều cao, primary action 132 reference units hoặc chỉ dẫn chạm trực tiếp. Áp dụng safe area và Design System Phase 1.
- Giữ nguyên các Button, callback, tọa độ spawn, kích thước target, ngưỡng kim và input handler gốc. Viewport logic có gutter hai bên để chứa mục tiêu vốn sinh tới ±155 cộng nửa rộng 32; vùng chơi co giãn đồng đều. Layout đặt ở parent riêng để không tranh quyền sở hữu offset với shake của controller.
- Các đỉnh có ảnh chi tiết lấy trực tiếp từ tranh ba lò hiện có; yêu ma có mặt nạ vector dễ nhận diện, không lệ thuộc emoji thiếu glyph. Nhãn động mirror Text gốc sang TMP, không viết vào state gameplay.
- Chiến báo có tranh theo game, thành tích, trạng thái đạt/chưa đạt/hết giờ hoặc kết quả thường/cực phẩm; các Reward Card lấy đúng `RewardResult`, scroll được. Thưởng vốn đã cấp trước khi hiện modal, nên nút ghi **Hoàn tất**, không phát thưởng lần hai.
- Feedback mở màn bằng nhãn ngắn, kết quả fade 0,18 giây theo unscaled time; chọn đỉnh báo ngay trong lúc chờ chiến báo; timer dưới 5 giây đổi màu cảnh báo và xử lý cả dấu phẩy/dấu chấm thập phân. Giữ các hiệu ứng gameplay gốc, không thêm RNG hoặc can thiệp time scale.

## Mỹ thuật

Tái sử dụng ba tranh gốc trong `Assets/Resources/SecretRealm`. Bổ sung hai tranh hoàn chỉnh bằng built-in imagegen, đã đưa vào project và gắn reference thật trong scene:

- `Assets/UI/SecretRealm/Art/LightningPool.png`
- `Assets/UI/SecretRealm/Art/AncientSwordTomb.png`

Ba ảnh alpha riêng `SwordGreenCutout.png`, `SwordPurpleCutout.png`, `SwordGoldCutout.png` trong cùng thư mục loại bỏ ô nền của sprite kiếm. View chỉ đặt `Image.overrideSprite` theo đúng sprite mà core đang chọn; không sửa atlas gốc, rarity, reward level hoặc lựa chọn sprite của controller. Gauge được giảm độ bão hòa; toàn bộ vị trí/kích thước zone và kim giữ nguyên.

[Prompt và nguồn ảnh](ArtPrompts.md). Không dùng panel màu hay asset rỗng thay cho ảnh đại diện. Các yếu tố nền/viền/nút dùng jade đậm, vàng cổ, ivory và typography/spacing từ FoundationTheme; màu báo nguy và phân vùng phẩm chất vẫn mang ý nghĩa tương tác của game.

Ảnh render thực tế:

- [Danh sách Bí Cảnh](Evidence/realm-list-top-1080x1920.png) · [Hai tranh mới trong danh sách](Evidence/realm-list-bottom-1080x1920.png).
- [Vạn Ma Triều Dâng](Evidence/play-HordeSurvival-1080x1920.png) · [Trảm Phá Cấm Chế](Evidence/play-BarrierBreak-1080x1920.png).
- [Đan Thần Kỳ Duyên](Evidence/play-DanCauldron-1080x1920.png) · [Vượt Lôi Trì](Evidence/play-LightningDodge-1080x1920.png) · [Kiếm Chủng Cổ Thần](Evidence/play-SwordTomb-1080x1920.png).
- [Chiến báo thắng](Evidence/victory-horde-1080x1920.png) · [Hết giờ](Evidence/timeout-BarrierBreak-1080x1920.png) · [Nội dung dài](Evidence/realm-long-text-1080x1920.png) · [Thưởng với giá trị lớn](Evidence/reward-large-values-1080x1920.png).

## Kiểm thử và giới hạn

Evidence cuối: [checks.json](Evidence/checks.json), [console-errors.json](Evidence/console-errors.json), [save-preservation.json](Evidence/save-preservation.json). Bộ `RealmQA` dùng profile `realm-phase7-qa`, xác nhận game loop thực sự tiến frame. Kiểm trên Game View **1080×1920, 1080×2340, 720×1280**.

Các file `previous-failure-*.txt` là lịch sử trước khi sửa lỗi render/QA fixture; không phải kết quả bàn giao. Lượt cuối bắt buộc kiểm tra không có lỗi runtime, mask yêu ma có CanvasRenderer và callback chạm trực tiếp, cùng mapping ảnh alpha kiếm không đổi sprite lựa chọn của core. Khi thoát giữa hiệu ứng rung, vị trí hiển thị được đặt lại ở đầu lượt mới sau khi controller đã dừng coroutine cũ.

Đã kiểm toàn bộ game: mở danh sách, vào cửa, bắt đầu, vùng target, raycast chạm thực, exit không nhận thưởng; scroll, font/overflow, không che nút, Back và điều hướng trực tiếp. Chạy thắng bằng action/target Button gốc: trảm 50 yêu ma với spawn/clock gốc; 20 nhát phá bia; chọn đúng/sai đỉnh từ sấm ngôn thật; thu 20 lôi tinh; ba lượt kiếm ngoài rìa và canh đúng vùng vàng với kim đang chuyển động (không inject vị trí kim hay điểm thắng).

Chạy **đủ đồng hồ gốc** 20 giây của cấm chế và 25 giây của lôi trì để kiểm hết giờ, cảnh báo timer và kết quả. Cấm chế có thưởng tiến độ; lôi trì 0 tinh cầu không có thưởng, được giữ đúng. Đan và kiếm không có timer thua riêng, nên không tạo trạng thái thua/hết giờ giả. Horde có luồng hoàn thành tự nhiên và exit; nhánh hết giờ được kiểm từ checkpoint QA gần hết hạn trong profile riêng, không coi đó là đo đủ 30 giây. Auto damage của Horde vẫn có thể diệt mục tiêu khi timeScale=0 theo code gốc; QA dùng thêm một lần spawn gốc riêng cho kiểm tra mask/native tap, và không thay cơ chế này.

Kiểm miễn phí, trừ đúng 5 CD, thiếu tiền bị chặn, reset lượt ngày bằng service gốc; đóng chiến báo không cấp thưởng trùng; Save/Load Bí Cảnh qua DataManager. Kiểm tên/mô tả dài và tám loại thưởng với số lớn bằng dữ liệu hiển thị tạm trong bộ nhớ, không sửa catalog/công thức hoặc cấp các thưởng stress đó. Hash production PlayerPrefs, JSON và backup trước/sau phải không đổi.

Không chạy Android build/thiết bị thật, Device Simulator có notch, hay thử chạm trên màn hình cảm ứng vật lý trong Phase này. Đây là kiểm thử Editor và native uGUI PointerEventData qua Pipeline, không được coi là chứng nhận thiết bị.

## Bảo toàn logic và commit

[logic-preservation.json](logic-preservation.json) so sánh các phương thức controller với snapshot trước sửa: chỉ các hook trình bày/đọc reference được thay; toàn bộ phương thức setup, update, spawn, input, RNG, kết thúc và tính thưởng giữ nguyên. Các file manager/data/tuning/ProjectSettings được so SHA-256 với baseline đầu Phase. Các sửa dirty có trước và save người chơi được giữ ngoài commit.

Scene/prefab được tạo, sửa và lưu bởi Editor Scripting qua Pipeline. Không sửa YAML scene/prefab bằng text. Delta scene được stage riêng qua Git three-way từ snapshot đầu Phase để không nhận nhầm thay đổi cũ của người dùng. Dừng tại Phase 7.

## Kết quả bàn giao cuối

- 1268 checks qua; 0 lỗi runtime/compile ở lượt cuối. Console hiện tại có 0 error và 2 warning đã có.
- 47/51 phương thức controller nguyên vẹn; bốn phương thức chỉ thêm hook hiển thị/reference. 119 file bảo vệ giữ SHA-256.
- Scene có đúng ba view mới, 0 missing script, 0 marker QA; 224 object Editor mới còn đủ trong scene staged, không có reference local mới bị mất.
- Save production giữ nguyên cả PlayerPrefs, JSON và backup; các sửa dirty có trước được giữ ngoài commit.
- Chi tiết: [FinalVerification.json](FinalVerification.json).

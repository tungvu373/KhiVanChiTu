# Bộ icon AI — Khí Vận Chi Tử

35 PNG thật do built-in `image_gen` tạo, nền alpha; ảnh gốc được sao chép nguyên byte vào `Assets/UI/GameIcons/Art/`. Không dùng ảnh placeholder. SHA-256, kích thước gốc, đường dẫn và kiểm chứng byte nằm trong [manifest.json](manifest.json). Bốn prompt kỹ năng nằm trong [generation.json](generation.json), các prompt còn lại trong `Generation/<ID>.json`.

Phong cách: vật phẩm vẽ 2.5D, khối rõ ở kích thước nhỏ, ngọc bích/xanh ngọc đậm, kim loại vàng cổ và màu nguyên tố. Không có chữ, tên, số lượng hoặc khung rarity nướng vào bitmap. Những dữ liệu này vẫn do UI/game hiện có hiển thị. UI giữ bố cục và vùng chạm; chỉ tăng hình kỹ năng lên 96px (72px compact) và hình navigation lên 80px trong nút cũ.

| Nhóm | Asset ID | Sử dụng |
|---|---|---|
| Kiếm | Sword01–Sword10 | Native MergeItem prefab variant của mobile; kho, chi tiết, trang bị, nguyên liệu và kết quả ghép |
| Kỹ năng | SkillSword, SkillPalm, SkillThunder, SkillYinYang | Bốn slot HUD; Lôi/Vạn Kiếm dùng lại cho trận pháp tương ứng |
| Đan | PillCultivation, PillBreakthrough | Hai công thức; quality vẫn ở khung/chữ, không đổi rules |
| Dược liệu | HerbLingzhi, HerbCondensing, HerbSnowLotus | Ba dược liệu có thật trong kho; chi tiết dùng cùng hình |
| Nguyên liệu | Ore, Dust | Quặng Phôi và Dược Trần/Bụi Dược |
| Hòm | Chest | Hòm tạm và biểu tượng nhóm tiện ích |
| Pháp khí | Robe, Crown, Boots, Amulet | Bốn món trang bị thụ động có thật |
| Tu luyện | Body, Cauldron | Luyện Thể và lò Luyện Đan |
| Trận pháp | SpiritLotus, FrostFormation, BloodFormation, GoldenBell | Tụ Linh, Băng, Huyết Ma và Kim Cang; hai trận còn lại dùng SkillThunder/SkillSword |
| Tài nguyên | SpiritStone, FortuneCoin | Linh Thạch, Cơ Duyên; thưởng/thiên phú tương ứng |
| Bí Cảnh | RealmPortal | Icon điều hướng Bí Cảnh |

Rarity kiếm giữ nguyên: cấp 1 Trắng, 2 Lục, 3–4 Lam, 5–7 Tím, 8–10 Cam. Ảnh cấp 9–10 mới phân biệt hình dáng; không tạo rarity mới hoặc sửa giới hạn/công thức. Công thức, Item Database, SkillData, drop/reward và save giữ nguyên. Hình bay/VFX trong chiến đấu vẫn thuộc hệ render hiệu ứng đang có; bộ này thay artwork giao diện.

Import Sprite/Single, alpha từ PNG, FullRect, không 9-slice, không mipmap, không giữ CPU-readable copy, kích thước import tối đa 512. Asset nguồn vẫn giữ đầy đủ độ phân giải gốc. Không tuyên bố hiệu năng Android từ số đo Editor.

`GameIconCatalogue` là bảng tham chiếu ảnh UI, không chứa ID vật phẩm, chỉ số, chi phí hoặc dữ liệu người chơi. `Assets/Resources/UI/GameIconCatalogue.asset` cho các view tải cùng một bộ hình. Original `Assets/Prefabs/MergeItem.prefab` giữ nguyên; scene mobile dùng variant `Assets/UI/GameIcons/Prefabs/MergeItemIllustrated.prefab` với 10 sprite.

Menu áp dụng lại: **TuTien / AI Icons / Import and apply illustrated icon set**. Tool yêu cầu đủ 35 PNG thật trước khi thay refs, cập nhật qua Unity Editor API, không sửa scene/prefab YAML bằng text. Native button/callback/cooldown/input và cơ chế phân trang inventory được giữ nguyên.

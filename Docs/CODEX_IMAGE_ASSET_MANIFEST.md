# BÁO CÁO DANH MỤC HÌNH ẢNH CHO CODEX RENDER (V2 — CHUẨN WEBGL/MOBILE)
> **Nguyên tắc bắt buộc:** Mọi vị trí cần hình minh họa trong UI Toolkit & Map **KHÔNG thay thế bằng vector/emoji/icon chữ**.  
> **Thư mục thả file chuẩn (đã cập nhật an toàn cho Build WebGL/APK):** **`Assets/_Game/Resources/CodexDrop/`**  
> Ngay khi Codex render xong và thả ảnh đúng tên file dưới đây, `CodexArtImportPostprocessor` sẽ tự động nén chuẩn WebGL/iPhone (`MipMaps = Off` cho UI) và `CodexArtLoader` tự động hiển thị lên giao diện mà không cần sửa code hay kéo thả thủ công.

---

## 0. PHONG CÁCH MỸ THUẬT CHUNG (ART BIBLE CHO CODEX)

* **Tông màu chủ đạo (Light Sunlit Xianxia):** Sáng sủa, trong trẻo, tươi mát ban ngày — đồng bộ hoàn toàn với ảnh mẫu đồng cỏ xanh trời nắng.
  * **Nền UI sáng:** Bạch ngọc (`#F6F3EA`), giấy tuyên ấm (`#EDE6D6`), xanh ngọc nhạt (`#DDF2EA`), viền vàng ánh kim dịu (`#C89B3C`) và xanh ngọc bích (`#2D8B74`). **Tuyệt đối không dùng nền đen/xanh đen tối như bản cũ.**
  * **Phong cách minh họa vật phẩm/kỹ năng:** Tranh vẽ tay kỹ thuật số (painted Xianxia RPG illustration), nét gọn sắc, ánh sáng ban ngày ấm, độ tương phản tốt khi đặt trên nền bạch ngọc/giấy tuyên sáng.
  * **Định dạng xuất chuẩn:**
    * Vật phẩm / huy hiệu / khung ô tròn / chân dung / ngọn lửa: **PNG 32-bit nền trong suốt (RGBA, Alpha sạch, không viền trắng/đen thừa)**.
    * Khung bảng 9-slice UI: **PNG 32-bit**, chừa viền bo góc đều nhau (padding 32–48px) để cắt 9-slice.
    * Ảnh nền / Texture Map: **PNG hoặc JPG chất lượng cao**, texture đất/cỏ bắt buộc **seamless tileable** (nối khít 4 cạnh không thấy vết ghép).

---

## 1. HÌNH ẢNH ĐÃ CÓ SẴN — TÁI SỬ DỤNG NGAY (47 ẢNH, KHÔNG CẦN RENDER LẠI)

Các ảnh dưới đây đã kiểm tra trực tiếp: **nền trong suốt sắc nét, hợp với HUD tông sáng**, giữ nguyên để tiết kiệm thời gian render:

| Nhóm | Số lượng | Đường dẫn hiện có trong Project | Ghi chú sử dụng |
| :--- | :---: | :--- | :--- |
| **10 Cấp Phi Kiếm (Lv.1–Lv.10)** | 10 | [Assets/UI/GameIcons/Art/Sword01.png](file:///c:/Lap%20trinh%20game/Save%20file/KhiVanChiTu/Assets/UI/GameIcons/Art/Sword01.png) … `Sword10.png` | Dùng cho UI Kiếm Lộ, Ghép Kiếm, Kho |
| **Sprite Sheet Phi Kiếm bay 3D/2D** | 1 | [Assets/Import/Asset/kiem.png](file:///c:/Lap%20trinh%20game/Save%20file/KhiVanChiTu/Assets/Import/Asset/kiem.png) | Dùng cho `FlyingSword.prefab` bay đánh quái |
| **Lò Luyện Đan & Thảo Dược sơ cấp** | 6 | `Cauldron.png`, `RouteAlchemy.png`, `HerbLingzhi.png`, `HerbCondensing.png`, `HerbSnowLotus.png`, `Dust.png` | Tái sử dụng làm Lò Đan + 3 thảo dược Bậc 1–3 + Dược Trần |
| **Tiền tệ & Nguyên liệu cơ bản** | 3 | `SpiritStone.png` (Linh Thạch), `FortuneCoin.png` (Cơ Duyên), `Ore.png` (Quặng Tinh Thiết) | Dùng trực tiếp trên HUD sáng |
| **4 Pháp Khí nền tảng** | 4 | `Robe.png` (Đạo Bào), `Crown.png` (Đạo Quán), `Boots.png` (Vân Phong Hài), `Amulet.png` (Linh Ngọc Bội) | Làm ảnh cho 4 ô Trang Bị đầu |
| **4 Kỹ Năng Chiến Đấu** | 4 | `SkillThunder.png` (Lôi Phạt), `SkillYinYang.png` (Âm Dương), `SkillSword.png` (Vạn Kiếm), `SkillPalm.png` (Ấn Chưởng) | Dùng trực tiếp cho 4 ô tròn Kỹ Năng |
| **4/6 Đại Trận Pháp** | 4 | `FrostFormation.png`, `SpiritLotus.png`, `BloodFormation.png`, `GoldenBell.png` | Còn thiếu 2 trận → nằm trong Nhóm C bên dưới |
| **6 Node Nhánh Thể Phách** | 6 | `RouteArt/BodyVitality.png`, `BodyGuard.png`, `BodyRecovery.png`, `BodyForce.png`, `BodyDrain.png`, `BodyFocus.png` | Tái sử dụng trong Nhánh Thể Phách của Cây Đạo Vận |
| **Ảnh Đại Diện & Nút Điều Hướng** | 9 | `Menu/Art/CharacterPortrait.png`, `RouteMerge.png`, `RouteTalents.png`, `RouteFormation.png`, `RouteArtifact.png`, `RouteMilestones.png`, `RouteAchievements.png`, `RouteSettings.png`, `RouteStorage.png` | Dùng trong các ô tròn HUD đóng/mở |

---

## 2. DANH MỤC 79 HÌNH ẢNH MỚI CẦN CODEX RENDER

> **Thư mục gốc nhận ảnh:** `Assets/_Game/Resources/CodexDrop/` (đã tạo sẵn đầy đủ các thư mục con).

### Nhóm A — Khung HUD Sáng, 2 Cụm Ô Tròn Đóng/Mở & Nền Giao Diện (12 ảnh — *Ưu tiên Mốc 1*)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết & Gợi ý prompt cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| A1 | `HUD/hud_circle_btn_idle.png` *(✅ Đã render)* | 256×256 | Trong suốt | Khung ô tròn HUD sáng (`56px` hiển thị): nền bạch ngọc mờ nhẹ, viền vàng ánh kim mảnh chạm vân mây, sáng dịu ban ngày. |
| A2 | `HUD/hud_circle_btn_active.png` *(✅ Đã render)* | 256×256 | Trong suốt | Trạng thái đang chọn/sáng của ô tròn HUD: viền vàng rực pha ánh ngọc bích phát sáng ấm. |
| A3 | `HUD/hud_circle_btn_locked.png` *(✅ Đã render)* | 256×256 | Trong suốt | Khung ô tròn khóa (tông sáng xám đá ngọc mờ + họa tiết ổ khóa cổ phong tinh tế ở góc dưới, không đen tối). |
| A4 | `HUD/hud_toggle_bagua.png` *(✅ Đã render)* | 256×256 | Trong suốt | Nút tròn trung tâm đóng/mở Cụm Tu Luyện góc dưới-phải: biểu tượng Bát Quái bạch ngọc – thanh ngọc – vàng kim tinh xảo. |
| A5 | `HUD/hud_avatar_frame.png` *(✅ Đã render)* | 256×256 | Trong suốt | Khung viền tròn ôm Avatar góc trên-trái: bạch ngọc + dây sen vàng thanh thoát (chừa rãnh tròn ngoài cho vòng % Tu Vi). |
| A6 | `HUD/hud_panel_light_9slice.png` *(✅ Đã render)* | 512×512 | Trong suốt | Khung bảng chính 9-slice: nền giấy tuyên / bạch ngọc sáng (`#F7F3E8`), 4 góc trang trí hoa văn mây tiên vàng đồng mảnh. |
| A7 | `HUD/hud_card_light_9slice.png` *(✅ Đã render)* | 512×512 | Trong suốt | Khung thẻ con 9-slice bên trong bảng: trắng kem dịu (`#FFFDF7`), viền ngọc nhạt mảnh, bóng đổ rất nhẹ. |
| A8 | `HUD/hud_btn_primary_9slice.png` *(✅ Đã render)* | 512×192 | Trong suốt | Nút hành động chính 9-slice: chuyển sắc vàng mật ong – hổ phách sáng, viền sáng lấp lánh, nổi chữ đậm. |
| A9 | `HUD/hud_btn_secondary_9slice.png` *(✅ Đã render)* | 512×192 | Trong suốt | Nút phụ 9-slice: xanh ngọc bích sáng dịu (`#DCEFE6`), viền ngọc đậm, tinh gọn. |
| A10 | `HUD/rarity_1_pham.png` … `rarity_5_thien.png` *(5 file)* | 256×256 ×5 | Trong suốt | 5 khung ô vuông bo góc hiển thị phẩm chất Trang Bị/Đan: `rarity_1_pham` (Trắng ngọc), `rarity_2_linh` (Xanh lá ngọc), `rarity_3_huyen` (Lam thiên), `rarity_4_dia` (Tím linh), `rarity_5_thien` (Vàng cam tiên quang). *(Tính gọn chung bộ khung A10 — nếu tách 5 file riêng tổng file vật lý là 82 file).* |
| A11 | `Backgrounds/bg_main_menu_daylight.jpg` *(✅ Đã render)* | 1920×1080 | Kín nền | Nền Main Menu ban ngày sáng tươi: tiên sơn trôi giữa trời xanh mây trắng, thảm cỏ xanh rực nắng, kiếm tiên bay xa xa (hợp cả PC 16:9 & cắt giữa Mobile 9:16). |
| A12 | `Backgrounds/bg_loading_screen.jpg` *(✅ Đã render)* | 1920×1080 | Kín nền | Nền màn hình Loading chuyển Scene: trời xanh mây trắng, đài sen bạch ngọc trên biển mây nắng sớm, chừa vùng thoáng dưới đáy cho thanh tiến độ. |

---

### Nhóm B — Luyện Đan Remake: 8 Đan Đột Phá, 8 Phẩm Đan Sư, 4 Linh Hỏa & Thảo Dược Cao Cấp (24 ảnh)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| B1 | `Pills/pill_00_waste.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Phế Đan**: viên đan xám nứt vỡ bốc khói tro nhẹ (dùng Tế Đan Nuôi Hỏa). *(Ưu tiên Mốc 1)* |
| B2 | `Pills/pill_01_truc_co.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Trúc Cơ Đan (Bậc 1)**: viên đan tròn màu ngọc bích thanh mát, ánh sáng dịu. *(Ưu tiên Mốc 1)* |
| B3 | `Pills/pill_02_ket_kim.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Kết Kim Đan (Bậc 2)**: viên đan ánh kim vàng óng, vòng khí nhỏ quanh thân. *(Ưu tiên Mốc 1)* |
| B4 | `Pills/pill_03_nguyen_anh.png` | 256×256 | Trong suốt | **Ngưng Anh Đan (Bậc 3)**: viên đan xanh lam pha tím ánh sao, có vệt sáng linh hồn. |
| B5 | `Pills/pill_04_hoa_than.png` | 256×256 | Trong suốt | **Hóa Thần Đan (Bậc 4)**: viên đan đỏ hổ phách – vàng rực, tia hào quang thần thức. |
| B6 | `Pills/pill_05_luyen_hu.png` | 256×256 | Trong suốt | **Luyện Hư Đan (Bậc 5)**: viên đan xanh bạch kim bán trong suốt, vệt khí hư không. |
| B7 | `Pills/pill_06_hop_the.png` | 256×256 | Trong suốt | **Hợp Thể Đan (Bậc 6)**: viên đan song sắc thanh – kim hòa quyện kiểu thái cực sáng. |
| B8 | `Pills/pill_07_dai_thua.png` | 256×256 | Trong suốt | **Đại Thừa Đan (Bậc 7)**: viên tiên đan vàng kim khắc **3 vòng Đan Văn** tỏa sáng lộng lẫy. |
| B9 | `Pills/pill_08_do_kiep.png` | 256×256 | Trong suốt | **Độ Kiếp Tiên Đan (Bậc 8)**: viên đan cửu sắc ngọc quang kèm tia lôi quang vàng lam. |
| B10–B12 | `Pills/herb_04_long_huyet.png`, `herb_05_phuong_hoang_chi.png`, `herb_06_tien_lo_lien.png` | 256×256 ×3 | Trong suốt | **3 Thảo dược cao cấp** (Yêu Vực → Tiên Kiếp): *Long Huyết Thảo* (đỏ ngọc), *Phượng Hoàng Chi* (vàng cam), *Tiên Lộ Thanh Liên* (sen xanh trắng phát sáng). |
| B13 | `Pills/alchemist_rank_1.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Huy hiệu Đan Sư Nhất Phẩm**: ấn ngọc/lò đan sơ cấp tông sáng. *(Ưu tiên Mốc 1)*. |
| B14–B20 | `Pills/alchemist_rank_2.png` … `alchemist_rank_8.png` *(7 file)* | 256×256 ×7 | Trong suốt | **7 Huy hiệu Cảnh Giới Đan Sư (Nhị Phẩm → Bát Phẩm)**: ấn ngọc/lò đan tăng dần độ tinh xảo & hào quang. |
| B21–B24 | `Pills/flame_1_mortal.png`, `flame_2_azure_lotus.png`, `flame_3_violet_core.png`, `flame_4_immortal_gold.png` | 256×256 ×4 | Trong suốt | **4 Bậc Linh Hỏa Lò Đan** (nâng bằng Tế Đan Nuôi Hỏa): *Phàm Hỏa* (lửa cam ấm — *ưu tiên Mốc 1*), *Thanh Liên Hỏa* (lửa sen xanh ngọc), *Tử Cực Hỏa* (lửa tím huyền), *Tiên Thiên Hỏa* (lửa vàng trắng rực rỡ). |

---

### Nhóm C — Trận Pháp (2 Trận còn thiếu hình riêng) & Vòng Trận Sáng (3 ảnh — *Ưu tiên Mốc 1*)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| C1 | `Formations/formation_thunder.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Cửu Thiên Lôi Trận**: trận đồ bát quái xanh lam – vàng tia sét sắc nét, đồng bộ phong cách 4 ảnh trận có sẵn. |
| C2 | `Formations/formation_sword_rain.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Vạn Kiếm Quy Tông Trận**: trận đồ vòng tròn vàng ngọc có nhiều lưỡi phi kiếm thanh mảnh tỏa sáng. |
| C3 | `Formations/formation_ground_ring_light.png` *(✅ Đã render)* | 512×512 | Trong suốt | **Hoa văn trận đồ đặt dưới chân nhân vật trên nền cỏ xanh sáng** (nét vàng trắng rực rỡ nổi bật rõ trên cỏ ban ngày). |

---

### Nhóm D — Trang Bị Loot RPG & Nguyên Liệu Nâng Cấp (10 ảnh)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| D1 | `Equipment/equip_bracer.png` | 256×256 | Trong suốt | **Hộ Uyển (Ô Trang Bị #5)**: vòng hộ cổ tay bạch ngọc khảm vàng cổ phong. |
| D2 | `Equipment/equip_ring.png` | 256×256 | Trong suốt | **Trữ Vật Giới (Ô Trang Bị #6)**: nhẫn ngọc bích chạm vân mây phát ánh sáng dịu. |
| D3–D5 | `Equipment/set_thanh_van.png`, `set_huyen_vu.png`, `set_dan_tong.png` | 256×256 ×3 | Trong suốt | **3 Huy hiệu Bộ Trang Bị (Set Bonus)**: *Thanh Vân* (mây xanh kiếm ý), *Huyền Vũ* (mai rùa vàng hộ thuẫn), *Đan Tông* (lò ngọc lửa tiên). |
| D6 | `Equipment/mat_reforge_stone.png` | 256×256 | Trong suốt | **Đá Tẩy Luyện / Linh Văn Thạch**: viên tinh thạch đa giác xanh ngọc – vàng sáng (reroll dòng phụ trang bị). |
| D7 | `Equipment/mat_forge_talisman.png` | 256×256 | Trong suốt | **Phù Rèn Kiếm (+10% ghép kiếm)**: lá bùa giấy vàng sáng vẽ ấn kiếm đỏ son phát sáng. |
| D8 | `Equipment/mat_skill_scroll.png` | 256×256 | Trong suốt | **Tàn Quyển Công Pháp (nâng cấp Kỹ Năng)**: cuộn trúc/giấy cổ ánh vàng buộc dây ngọc. |
| D9 | `Equipment/mat_dao_point.png` | 256×256 | Trong suốt | **Điểm Đạo Vận (Đạo Tinh)**: hạt châu thái cực bạch ngọc – kim quang (cộng node lớn Cây Kỹ Năng). |
| D10 | `Equipment/mat_calm_incense.png` | 256×256 | Trong suốt | **Tĩnh Tâm Hương (Trấn Tâm)**: nén trầm hương xanh ngọc tỏa khói trắng thanh tịnh (reset chuỗi phạt đột phá). |

---

### Nhóm E — Cây Kỹ Năng Đạo Vận (Node Graph UI Sáng) (9 ảnh)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| E1 | `DaoVan/daovan_bg_canvas.jpg` *(✅ Đã render)* | 1024×1024 | Seamless / Kín nền | Nền bản đồ Cây Đạo Vận **tông sáng**: giấy tuyên trắng ngà pha mây tiên xanh ngọc cực nhạt & đường kinh mạch mờ tinh tế. *(Ưu tiên Mốc 1)* |
| E2 | `DaoVan/daovan_core_node.png` *(✅ Đã render)* | 256×256 | Trong suốt | **Node Trung Tâm "Đạo Tâm"**: đài sen bạch ngọc phát hào quang vàng rực rỡ. *(Ưu tiên Mốc 1)* |
| E3–E7 | `DaoVan/branch_sword.png`, `branch_body.png`, `branch_cultivation.png`, `branch_wealth.png`, `branch_arts.png` | 256×256 ×5 | Trong suốt | **5 Huy hiệu nhánh lớn của Cây Đạo Vận**: *Kiếm Đạo*, *Thể Phách*, *Tu Luyện*, *Tài Lộc*, *Bách Nghệ*. |
| E8–E9 | `DaoVan/node_frame_unlocked.png`, `node_frame_maxed.png` *(✅ Đã render)* | 256×256 ×2 | Trong suốt | Khung tròn viền node Cây Kỹ Năng: trạng thái đã mở (viền thanh ngọc) & trạng thái tối đa (viền hoàng kim cánh sen). *(Ưu tiên Mốc 1)* |

---

### Nhóm F — Bản Đồ Xanh Sáng (Theo Ảnh Mẫu) & Hệ Thống Quái Mới (15 ảnh)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex (khớp ảnh mẫu đồng cỏ xanh sáng) |
| :-: | :--- | :---: | :---: | :--- |
| F1 | `Map/tex_grass_meadow_albedo.png` *(✅ Đã render)* | 1024×1024 | Seamless | **Texture nền cỏ đồng bằng xanh tươi ban ngày** (cel-shaded/stylized tươi sáng như ảnh mẫu, nối khít 4 cạnh). *(Ưu tiên Mốc 1)* |
| F2 | `Map/tex_dirt_patch_albedo.png` *(✅ Đã render)* | 512×512 | Trong suốt (viền mềm) | **Mảng đường đất vàng cát nhạt** rải tự nhiên trên nền cỏ như ảnh mẫu. *(Ưu tiên Mốc 1)* |
| F3 | `Map/tex_sky_daylight_panorama.jpg` *(✅ Đã render)* | 2048×1024 | Panorama 360° | **Vòm trời xanh trong ban ngày + mây trắng bồng bềnh + dãy núi xanh lam xa chân trời** (đúng tông ảnh mẫu, siêu nhẹ cho WebGL). *(Ưu tiên Mốc 1)* |
| F4–F5 | `Map/tex_act2_sand_bright.png`, `Map/tex_act3_jade_cliff.png` | 1024×1024 ×2 | Seamless | Texture nền sáng cho **Vực 2 (Hoang Mạc Nắng Vàng)** và **Vực 3 (Tiên Sơn Bạch Ngọc)**. |
| F6–F7 | `Map/prop_grass_tuft.png`, `Map/prop_bush_tree_stylized.png` | 512×512 ×2 | Trong suốt | Bụi cỏ xanh nhọn + bụi cây/cây stylized ban ngày. |
| F8–F11 | `Monsters/act1_wolf.png`, `act1_bird.png`, `act1_turtle.png`, `act1_boss_guardian.png` *(✅ Đã render)* | 256×256 ×4 | Trong suốt | **4 Chân dung Quái & Boss Thủ Lĩnh Vực 1** cho thanh máu Boss & bảng Sổ Tay Rơi Đồ Theo Ải. *(Ưu tiên Mốc 1)*. |
| F12–F15 | `Monsters/act2_scorpion.png`, `act2_boss_overlord.png`, `act3_thunder_spirit.png`, `act3_boss_archon.png` | 256×256 ×4 | Trong suốt | **4 Chân dung Quái & Boss Vực 2–3** cho thanh máu Boss & Sổ Tay Rơi Đồ. |

---

### Nhóm G — Hình Ảnh Nút HUD Tiện Ích, Đột Phá & Online (6 ảnh)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex |
| :-: | :--- | :---: | :---: | :--- |
| G1 | `HUD/nav_leaderboard.png` | 256×256 | Trong suốt | Hình **Bảng Xếp Hạng (Thiên Kiêu Bảng)**: bia đá bạch ngọc khắc vương miện vàng. |
| G2 | `HUD/nav_world_chat.png` | 256×256 | Trong suốt | Hình **Chat Thế Giới / Hộ Pháp**: cuộn truyền âm phù xanh ngọc – vàng sáng. |
| G3 | `HUD/nav_friends.png` | 256×256 | Trong suốt | Hình **Đạo Hữu / Kết Bạn & Nhắn Riêng**: song ngọc bội kết duyên vàng – xanh ngọc. |
| G4 | `HUD/nav_skills.png` *(✅ Đã render)* | 256×256 | Trong suốt | Hình **Kỹ Năng Chiến Đấu**: quyển bí kíp phát ánh sáng kiếm khí lam vàng. *(Ưu tiên Mốc 1)* |
| G5 | `HUD/nav_map_stage.png` *(✅ Đã render)* | 256×256 | Trong suốt | Hình **Bản Đồ Ải & Sổ Tay Rơi Đồ**: la bàn tiên giới đồng vàng – ngọc xanh. *(Ưu tiên Mốc 1)* |
| G6 | `HUD/nav_breakthrough.png` *(✅ Đã render)* | 256×256 | Trong suốt | Hình **Đột Phá Cảnh Giới**: tia lôi quang vàng lam đánh xuống đài sen sáng rực. *(Ưu tiên Mốc 1)* |

---

### Nhóm H — Sân Khấu Trực Quan V2, Tiên Môn (Tháp / Bí Cảnh / Luận Kiếm) & Kỳ Ngộ (8 ảnh bổ sung Đợt V2)

| # | Đường dẫn con trong `Resources/CodexDrop/` | Kích thước | Nền | Mô tả chi tiết cho Codex (Light Sunlit Xianxia) |
| :-: | :--- | :---: | :---: | :--- |
| H1 | `HUD/nav_immortal_gate.png` | 256×256 | Trong suốt | Biểu tượng nút HUD **Tiên Môn (Lịch Luyện)**: cổng đá bạch ngọc chạm rồng mây vàng kim mở ra ánh sáng xanh ngọc ban ngày, nét sắc gọn cho ô tròn HUD. |
| H2 | `Stages/stage_sword_forge.png` | 512×512 | Trong suốt | Sân khấu trung tâm **Đài Rèn Kiếm Lộ V2**: bệ đá bạch ngọc cổ khắc trận văn vàng kim và khe tụ linh hỏa xanh ngọc ở giữa (chừa vùng thoáng phía trên tâm bệ để UI đặt 2 thanh phi kiếm đối xứng hợp nhất). |
| H3 | `Stages/stage_alchemy_cauldron.png` | 512×512 | Trong suốt | Sân khấu trung tâm **Đan Lô Cửu Long V2**: đỉnh luyện đan thanh đồng – bạch ngọc khảm rồng vàng tinh xảo ánh nắng ấm, ba chân vững chãi chừa khoảng thoáng dưới đáy lò để chồng hiệu ứng ngọn Linh Hỏa cháy động. |
| H4 | `Stages/stage_tribulation_altar.png` | 512×512 | Trong suốt | Sân khấu trung tâm **Độ Kiếp Đài V2**: tu sĩ áo trắng ngồi xếp bằng tĩnh tọa trên đài sen bạch ngọc phát vầng kim quang, phía trên đỉnh đầu có mây kiếp xanh lam – vàng kim tụ lôi sáng rõ trên nền giao diện giấy tuyên. |
| H5 | `Stages/mode_demon_tower.png` | 512×256 | Trong suốt | Minh họa thẻ ngang **Vạn Yêu Tháp (100 Tầng)**: ngọn bảo tháp cổ mái ngọc bích vươn cao giữa mây trắng nắng vàng, quanh thân tháp lượn vòng kiếm khí vàng kim và yêu khí tím nhạt. |
| H6 | `Stages/mode_secret_realm.png` | 512×256 | Trong suốt | Minh họa thẻ ngang **Thái Hư Bí Cảnh (Kiếm Mộ · Dược Viên · Khoáng Động)**: cổng vòm không gian bạch ngọc mở ra thung lũng tiên thảo phát sáng, thanh kiếm cổ cắm trên đá và tinh thể quặng ngọc bích. |
| H7 | `Stages/mode_pvp_arena.png` | 512×256 | Trong suốt | Minh họa thẻ ngang `Coming Soon` **Luận Kiếm Đài (Đấu Pháp Thiên Kiêu)**: lôi đài bạch ngọc lơ lửng giữa biển mây ban ngày rực nắng, hai luồng kiếm quang thanh ngọc và hoàng kim giao phong trên đài. |
| H8 | `Stages/event_fortuitous_orb.png` | 256×256 | Trong suốt | Biểu tượng nổi **Kỳ Ngộ Linh Cơ**: viên linh châu ngọc bích – vàng kim tỏa mây lành kèm hồ lô tiên nhỏ lấp lánh, nổi bật để làm nút sự kiện trôi góc màn hình. |

---

### Tổng kết tiến độ hình ảnh hiện tại
* **Đợt 1 (Nhóm A–G):** **83/83 ảnh vật lý** đã hoàn thành trong `Assets/_Game/Resources/CodexDrop/` kèm bản bake seamless & 9-slice trong `Assets/_Game/Resources/BakedMaps/`.
* **Đợt 2 bổ sung nâng cấp V2 (Nhóm H):** **8 ảnh mới (`H1`–`H8`)** cho Sân khấu UI V2 & Sảnh Tiên Môn (nâng tổng kho CodexDrop lên **91 ảnh**).
* **Tài sản có sẵn tái sử dụng ngay:** **47 ảnh** nền trong suốt sắc nét trong `Assets/UI/GameIcons/` & `Assets/UI/Menu/Art/`.
* Mọi vị trí chưa có file PNG từ Codex sẽ dùng **Khung Bạch Ngọc Sáng có nhãn mã file** qua `CodexArtLoader` — tuyệt đối **không** thay thế bằng icon chữ/emoji.

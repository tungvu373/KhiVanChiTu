# Phase 05 — AI UI Asset Generation

Đã tạo và nhập **25 PNG nguyên bản** cho bộ UI ngọc tối–vàng cổ. Đủ 15 nhóm yêu cầu: panel, viền, popup, HUD, navigation/upgrade button, 4 resource icon, equipment slot, 6 rarity frame, cultivation halo, alchemy/formation decoration, reward frame, tab và các lớp divider/track/selection.

- PNG + `.meta`: `Assets/Art/UI/Generated/JadeV5_*.png`.
- Manifest chính: `Docs/UI_Redesign/asset_manifest.json`; bản giống byte trong thư mục Generated giúp bàn giao pack. Manifest chứa SHA-256, GUID, kích thước thật, alpha, sprite rect/border, cách dùng và **toàn bộ 25 prompt đã chạy**.
- [Xem toàn bộ asset](Images/Phase05/asset_overview.png).

## Sinh ảnh và định hướng

Dùng skill `imagegen`, chế độ **built-in image_gen**: 20 lần sinh mới + 5 lần edit biến thể màu từ rarity neutral vừa sinh. Không dùng API-key/CLI fallback. Giữ bản gốc trong thư mục generated_images của Codex và copy vào dự án; hash xác nhận 25 bản copy giống byte đầu ra generator. Model ID không được built-in tool cung cấp.

Art theo `design_system.md` và `design_tokens.json`: nephrite tối, vàng cổ, ngọc xanh, mây cuộn ở góc, ánh sáng ấm phía trên trái, center yên. Màu trong manifest là mục tiêu art direction, không phải cam kết mỗi pixel khớp hex. Không bake chữ, số, giá hoặc countdown. Không ghi đè `JadeGoldAtlas.png` hay `ImmortalRealmPortrait.png` và không tính hai ảnh cũ là ảnh sinh mới.

Manifest Phase03 có 84 entry gồm reuse reference, optional art, icon riêng từng feature và vector utility. Báo cáo này xác nhận **pack 25 component** theo các nhóm Phase05; không đánh dấu toàn bộ 84 entry đã được sinh.

## Alpha và chất lượng

Cả 25 ảnh đều là PNG **RGBA 8-bit**, có pixel alpha=0, alpha bốn góc ≤8/255, không có pixel alpha>8 ở mép canvas. Các mặt nền có center alpha 253/254, gần đục nhưng không phải 255. Một số pixel viền ngoài có alpha thấp (tối đa8/255), không phải tất cả đều bằng0. Không cần tách nền; không thêm alpha giả. PNG nguồn không bị crop/resample/repaint. Các sheet QA được composite riêng bằng System.Drawing; không dùng để thay thế asset.

Đã xem [nền tối/sáng/magenta — phần 1](Images/Phase05/alpha_proof_1.png), [phần 2](Images/Phase05/alpha_proof_2.png), [24/40/64/88 logical px](Images/Phase05/small_size_review.png). Không thấy nền checker bake, matte hoặc ornament méo trong phạm vi dùng đã chốt. Resource giữ silhouette khác nhau; chi tiết mảnh và khác biệt màu rarity suy giảm ở 24px, vì vậy luôn giữ nhãn cấp/phẩm chất và badge trạng thái riêng. Cultivation/Alchemy/Formation nên hiển thị lớn hơn icon tài nguyên; halo là lớp tĩnh, không phải animation/VFX prefab.

25 ảnh đầu ra được chọn sau QA; không cần sinh lại vì lỗi ảnh/alpha. Giới hạn co giãn dọc của bốn thanh ngang đã được xử lý bằng quy tắc sử dụng cố định chiều cao, không che giấu dưới nhãn “9-slice mọi kích thước”.

## Import và tái sử dụng

Editor đang mở: Unity **6000.6.4f1**, Pipeline **0.9.1-exp.1**. Nhập qua API Editor/Pipeline, không sửa YAML. Mỗi PNG chứa một sub-sprite có tên rõ ràng trong mode Multiple. Frame rect bỏ đệm trong suốt theo bounds alpha>8 +16px source padding; PNG gốc vẫn nguyên byte. Rect/border đã scale thực tế được đọc lại từ `Sprite` sau import và ghi trong manifest. Đây là full-rect mesh, không tight mesh.

Thiết lập chung: PPU100, Canvas reference PPU100, multiplier1, sRGB, Bilinear, Clamp, alphaIsTransparency=true, mipmap off, Read/Write off, Uncompressed. Max texture256 cho 6 rarity + navigation;512 cho phần còn lại. Không thêm atlas hoặc platform override. Kích thước nguồn generator khác target prompt được ghi trung thực trong bảng dưới.

- **13 frame co giãn hai chiều**: panel/border/popup/navigation/equipment/6rarity/reward/selection. Không dùng rect nhỏ hơn tổng border đã scale; xem minimumRectSize trong manifest.
- **4 thanh chỉ co ngang**: HUD height118.330ref; Upgrade97.309ref; Tab95.420ref; ResourceTrack63.298ref ở cấu hình import hiện tại. Giữ chiều cao Image gốc; đặt nó trong parent tương tác tối thiểu144ref. Nếu cần đổi chiều cao đáng kể, dùng component nhiều lớp hoặc tạo biến thể art phù hợp, không kéo dài ornament giữa cạnh.
- **8 Simple**: 4 resource icon + cultivation/alchemy/formation + divider. Giữ aspect. Divider có ngọc giữa nên không nine-slice. Decorative Image dùng raycastTarget=false; hit target, binding và text là lớp UI riêng.
- Popup/reward/rarity/border/selection có center mở. Ghép một nền tối riêng khi nội dung cần che gameplay. ResourceTrack không chứa fill tiến độ.
- Hiệu ứng pressed/disabled/focus/locked/cooldown dùng lớp state tái sử dụng theo design tokens; không bake số liệu hoặc trạng thái gameplay vào ảnh.

Đã review **51 mẫu 9-slice** qua [sheet 1](Images/Phase05/nine_slice_1.png), [2](Images/Phase05/nine_slice_2.png), [3](Images/Phase05/nine_slice_3.png), [4](Images/Phase05/nine_slice_4.png), [5](Images/Phase05/nine_slice_5.png). Đây là composite QA nguồn tại 1/3 reference scale, dùng rect/border đã xác minh từ Unity; không phải ảnh chụp runtime.

Trong Unity, **59 trường hợp mesh uGUI** đạt: 17 Sliced ×3 kích thước +8 Simple. Kiểm importer, border minimum, mesh không rỗng, vị trí finite/trong rect, UV trong texture. Dựng `Image.OnPopulateMesh(VertexHelper)` trong preview scene tạm rồi đóng; scene đang mở vẫn clean,12root, Edit Mode. Không cần lưu scene vì không thay scene. Hai lần thử cú pháp import và một lần chọn overload QA đã được sửa trước khi chạy thành công; không thêm script QA vào Assets/runtime.

## Danh mục PNG

| File | Native PNG | Imported texture | Cách dùng |
|---|---:|---:|---|
| `JadeV5_PanelBackground.png` | 1254×1254 | 512×512 | Sliced · 2 chiều |
| `JadeV5_GoldJadeBorder.png` | 1254×1254 | 512×512 | Sliced · 2 chiều |
| `JadeV5_PopupFrame.png` | 1024×1536 | 341×512 | Sliced · 2 chiều |
| `JadeV5_HudContainer.png` | 2172×724 | 512×171 | Sliced · chỉ ngang |
| `JadeV5_NavigationButton.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_UpgradeButton.png` | 2172×724 | 512×171 | Sliced · chỉ ngang |
| `JadeV5_EquipmentSlot.png` | 1254×1254 | 512×512 | Sliced · 2 chiều |
| `JadeV5_ResourceSpiritStone.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_ResourceCoDuyen.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_ResourceOre.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_ResourceMedicineDust.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_RarityNeutral.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_CultivationHalo.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_AlchemyDecoration.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_FormationDecoration.png` | 1254×1254 | 512×512 | Simple · giữ aspect |
| `JadeV5_RarityGreen.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_RarityBlue.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_RarityViolet.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_RarityOrange.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_RarityGold.png` | 1254×1254 | 256×256 | Sliced · 2 chiều |
| `JadeV5_RewardFrame.png` | 1024×1536 | 341×512 | Sliced · 2 chiều |
| `JadeV5_TabBackground.png` | 2172×724 | 512×171 | Sliced · chỉ ngang |
| `JadeV5_SectionDivider.png` | 2172×724 | 512×171 | Simple · giữ aspect |
| `JadeV5_ResourceTrack.png` | 2172×724 | 512×171 | Sliced · chỉ ngang |
| `JadeV5_SelectionFrame.png` | 1254×1254 | 512×512 | Sliced · 2 chiều |

## Ý nghĩa dữ liệu và giới hạn tích hợp

Rarity: neutral cho kiếmLv1 / Waste; green cho Lv2 / Basic; blue cho Lv3–4 / Refined; violet cho Lv5–7; orange cho Lv8; gold cho pill Supreme. Không thêm tier kiếm vàng. `SelectionFrame` độc lập với phẩm chất. Bốn resource icon ứng với Linh Thạch, Cơ Duyên, quặng Pháp Khí và Bụi Dược hiện có. Decoration không thêm công thức, trận pháp hoặc realm mới.

PNG chưa được gán vào scene/prefab/HUD Phase04. Gameplay, event, data binding, save và layout hiện tại được giữ nguyên. Đây là kiểm asset/import/mesh trong Editor; chưa xác nhận tích hợp HUD, text contrast cuối, animation, thiết bị vật lý, ASTC hoặc hiệu năng. Dung lượng texture RGBA32 không mipmap là ước tính theo kích thước import, không phải số đo residency thiết bị.

## Bảo toàn và rollback

Đối chiếu SHA-256: **2.108/2.108 file baseline không đổi**, không file mất. `design_system.md` và `design_tokens.json` không đổi. Branch `feature/mobile-ui-redesign`, HEAD và Git index giữ nguyên. Không stage/commit, không vào Play Mode hoặc ghi player save. Console cuối:0error,2warning đã có từ preflight; không compile failure.

Bằng chứng cục bộ trong `Docs/UI_Redesign/Backups/Phase05-20261112-185025/` gồm baseline, prompt provenance, alpha stats, import specs/results, mesh results và preservation check. Thư mục Backups bị Git ignore theo quy tắc dự án; dữ liệu bàn giao cần thiết đã nằm trong manifest và các sheet Images.

Rollback chọn lọc: khi asset chưa được tham chiếu, dùng AssetDatabase xóa đúng 25 `JadeV5_*.png` của manifest cùng meta, pack manifest và các output Docs/Images Phase05 mới. Không xóa cả thư mục Generated vì chứa art cũ. Nếu đã tích hợp sau này, kiểm reference/later edits trước khi xóa. Không reset/clean/stash hoặc restore scene/gameplay để rollback pack này.

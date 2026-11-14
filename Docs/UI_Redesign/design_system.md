# Phase 03 — Premium Xianxia Visual Design System

**Khí Vận Chi Tử · Ngọc tối, vàng cổ, ánh trăng · 1080×1920 portrait**

Trạng thái: **DESIGN ONLY — chưa áp dụng vào scene, prefab, code, import hoặc save**. Đây là chuẩn hình ảnh và bàn giao sản xuất art cho kiến trúc Phase 02. Không đánh dấu feature nào đã được triển khai chỉ vì đã có style hoặc prompt.

Ba đầu ra: [design_system.md](design_system.md), [design_tokens.json](design_tokens.json), [image_generation_manifest.json](image_generation_manifest.json). Token JSON chứa giá trị chuẩn; tài liệu này giải thích cách dùng. Manifest có nguồn, trạng thái, kích thước, prompt và cổng QA của từng asset. **Không sinh ảnh mới trong phase này.**

## 1. Nguồn chuẩn và giới hạn thực tế

- [Inventory Phase 01](feature_inventory.json): 48 nhóm màn/luồng, 7.712 FeatureID/control/data/callback/contract. [Map Phase 02](screen_redesign_map.json): 57 route, 195 cạnh; các route là thiết kế chưa dựng.
- [Kiến trúc](new_ui_architecture.md), [navigation](navigation_flow.md), [component architecture](ui_component_architecture.md), GDD mục 3 và brief mobile cung cấp cấu trúc và định hướng. Implementation và inventory thắng các hệ thống chỉ xuất hiện trong đề xuất.
- Unity 6000.6.4f1, uGUI 2.6.0, URP 17.6.0, Input System 1.20.0. Legacy `UnityEngine.UI.Text` / `UiTextStyle` được giữ. Không cần chuyển TMP hoặc thêm package để dùng thiết kế.
- Art đã có: `Assets/Art/UI/Generated/JadeGoldAtlas.png` và `ImmortalRealmPortrait.png`, cùng sáu prefab trang trí Jade. Đã xem cả atlas và nền; khung/ngọc–vàng nhất quán nhưng chi tiết mảnh và halo phải kiểm tra khi thu nhỏ.
- Năm dock giữ tên/thứ tự: **Kiếm Lô · Luyện Thể · Luyện Đan · Pháp Khí · Trận Pháp**. Kho, Bí cảnh, Vận khí, Mốc và hub Đạo Lộ tuân Phase 02.
- “Artifact crafting” áp dụng lên **bốn slot pháp khí được nâng cấp hiện có**. Không tạo thêm lệnh rèn ngẫu nhiên, công thức hay loot slot.
- Account/leaderboard/season trong brief chưa có implementation production tương ứng: không làm visual production mới cho chúng. Không đưa nút test sinh kiếm hoặc cheat recovery vào giao diện người chơi.
- Phạm vi PC giữ nguyên. Màu/shape chỉ mô tả dữ liệu; không thêm rarity enum, currency, gate, xác suất, công thức hay trường save.

## 2. Bản sắc nghệ thuật

**Cảm giác:** một pháp khí truyền thừa được chạm khắc tinh tế, đặt trước tiên cảnh đêm. Ngọc là vật liệu nền, vàng cổ là cấu trúc, ánh sáng là phản hồi hiếm và có mục đích.

| Thành phần | Quy tắc |
|---|---|
| Nền | Lam ngọc gần đen, chiều sâu bằng thay đổi surface và khe lõm; nền nội dung đọc gần như đặc |
| Ngọc | Nephrite tối, vân rất nhẹ, không dùng mây sáng chạy sau mọi dòng chữ |
| Kim loại | Vàng cổ xước mờ, cạnh trên sáng hơn cạnh dưới; tránh vàng neon và phản xạ nhựa |
| Ánh sáng | Key góc trên trái khoảng45°, rim lạnh nhẹ; highlight cùng hướng ở tất cả icon/khung |
| Hoa văn | Vân mây, móc hồi văn góc, vòng trận hình học; không ký tự Hán giả hoặc đoạn chữ bake trong ảnh |
| Điểm nhấn | Một tâm điểm cho mỗi bảng: tiêu đề hệ thống, hành động chính hoặc kết quả; không cùng lúc cả ba phát sáng |
| Độ trang trí | Phần lớn diện tích yên; trang trí tập trung ở header/góc. Mục tiêu khoảng80% yên/20% nhấn là chỉ dẫn thị giác, chưa đo pixel |
| Combat | Nhân vật, đàn kiếm, boss và telegraph là tâm điểm. Không biến toàn bộ HUD thành các bảng vàng dày |

Ba lớp vật liệu dùng lại:

1. **Nền mực:** ink/surface, phẳng và rõ chữ; page, nội dung dài, settings.
2. **Ngọc khắc:** surfaceRaised + mép vàng mảnh; workbench, item, card, action.
3. **Pháp ấn:** gold/jade halo nhỏ; selected, breakpoint, thành công sau command. Không để glow thường trực trên mọi ô.

Không dùng texture parchment sáng làm nền chính: nó cạnh tranh với combat và phá tương phản chung. Thông tin lore vẫn đặt trên nền ngọc tối.

## 3. Bảng màu và vai trò

Tất cả màu là sRGB. Opacity là token riêng, không giảm alpha cả cây chữ để thể hiện disabled.

| Token palette | Hex | Vai trò |
|---|---|---|
| ink | #081C22 | Full background, chữ trên CTA vàng sáng |
| surface | #0A1F22 | Panel chính, gần/hoàn toàn đặc sau chữ |
| raised | #123338 | Card và hàng tương tác |
| inset | #06181C | Rãnh, slot trống, track |
| jadeDeep | #164E43 | Mặt CTA ngọc |
| ivory | #F1E8CF | Nội dung và nhãn chính |
| secondary | #B5C8C3 | Mô tả, đơn vị |
| muted / disabled | #91AAA5 | Thông tin phụ/nhãn vô hiệu vẫn đọc được |
| gold | #D5B574 | Viền chủ đạo, tiêu đề nhấn |
| goldBright | #F3D995 | Focus, phần thưởng, selection |
| jade | #6EDDB6 | Tăng trưởng, đủ điều kiện, action tích cực |
| line | #42635F | Divider trang trí; không dùng làm đường viền control duy nhất |
| danger | #FF9793 | Lỗi, thiếu tài nguyên, cảnh báo thao tác mất đồ |
| dangerSurface | #4C252B | Nền nút phá hủy/xóa khi caller cho phép |
| warning | #F4C46F | Cảnh báo có thể tiếp tục |
| info | #91C8FF | Thông tin, nhấn lam |
| health / mana | #F28A90 / #83BFFF | HP / MP |
| cultivation | #BAA5F5 | Thanh tu vi, không đồng nghĩa rarity |

**Tương phản tĩnh trên nền đặc:** ivory/surface≈13,95:1; secondary/raised≈7,72:1; muted/raised≈5,46:1; gold/raised≈6,87:1; jade/raised≈8,15:1. Bộ kiểm tra tính lại đầy đủ trong JSON; tiêu chí chữ ≥4,5:1, đường bao điều khiển quan trọng ≥3:1.

Các tỷ lệ trên chỉ dành cho cặp token phẳng. Atlas có sắc độ/vân riêng, opacity và ảnh nền làm thay đổi kết quả. Không công bố “đạt WCAG toàn UI” từ phép tính này. Chữ trên art phải có panel đặc hoặc scrim đã đo trên vùng sáng nhất. Nút vàng dùng chữ ink; không dùng ivory trên nền vàng sáng.

Màu dành riêng cho ý nghĩa:

- Đỏ/hồng: HP, lỗi/thiếu; luôn có nhãn/ngữ cảnh để phân biệt.
- Lam: MP hoặc thông tin; label HP/MP và hình dạng thanh vẫn rõ.
- Vàng: nhấn/selected/claim; không đổi mọi item thành “hiếm”.
- Ngọc: action tích cực; điều kiện mở được lấy từ service, không suy đoán bằng màu ví.

## 4. Typography tiếng Việt

Font đã có, không tải thêm font:

| Role | Font | Cỡ / line spacing | Dùng cho |
|---|---|---|---|
| hero | Cormorant Bold |72 /1,20 | Một tiêu đề lớn ở title, chữ ngắn |
| title | Source Sans Pro Semibold |60 /1,25 | Tên page/dialog lớn |
| heading | Source Sans Pro Semibold |48 /1,30 | Tên section, title workbench |
| body | Source Sans Pro Regular |42 /1,35 | Nội dung và dòng quan trọng |
| action | Source Sans Pro Semibold |42 /1,20 | CTA và thao tác |
| stat | Source Sans Pro Semibold |48 /1,20 | Số chính |
| support | Source Sans Pro Regular |36 /1,35 | Mô tả ngắn, đơn vị |
| dock / badge | Source Sans Pro Semibold |36 /1,20 | Điều hướng và trạng thái ngắn |
| microDecorative | Source Sans Pro Regular |32 /1,30 | Chỉ annotation không thiết yếu |
| damage | Source Sans Pro Semibold |48 /1,20 | Chữ damage, tương thích renderer hiện có phải test sau |

Các số là **Canvas reference unit**. Ở width360 logical,42ref≈14logical,36ref≈12logical,48ref≈16logical. Cỡ32 dự kiến ở Phase 02 được nâng lên42 cho body, không áp lại cỡ32 lên chi phí, timer, tên hay mô tả bắt buộc. Đây là điều chỉnh typography của phase hình ảnh; geometry tối thiểu và flow giữ nguyên. Nếu chữ lớn hơn làm hết chỗ thì tăng chiều cao/wrap/scroll hoặc expanded mode, không thu nhỏ hit target.

Paths:

- Regular: `Assets/Resources/SourceSansPro-Regular.ttf`; `UiTextStyle.BodyFont` hiện dùng Resources key `SourceSansPro-Regular`.
- Semibold: `Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf`.
- Display: `Assets/Import/Asset/Font/Cormorant/Cormorant-Bold.otf`.
- Semibold/Display cần serialized Font reference ở view tương lai; không giả định đã có Resources key. Tránh scanner `JadeUITheme.StyleTree` ghi đè font display của view mới.
- OFL của Source Sans Pro/Cormorant nằm cạnh nguồn import; giữ notice. Cmap của font chỉ chứng minh có glyph, chưa chứng minh cách render dấu.
- Không chọn Babylonica/KD làm body, button hoặc số. Không dùng emoji làm icon; `UiTextStyle.RemoveIcons` có thể xóa chúng.
- Không bật Best Fit thu chữ dưới role tối thiểu. Line spacing trên là tỉ lệ hướng dẫn cho `Text.lineSpacing`; phải kiểm tra line box/đỉnh dấu, không coi nó là chiều cao pixel tuyệt đối.

Quy tắc đọc:

- Viết hoa đầu câu, tránh toàn bộ paragraph IN HOA. Không tăng tracking cưỡng bức cho tiếng Việt.
- Tên dài tối đa hai dòng trong card; chạm info đọc đầy đủ. Chỉ ellipsis nội dung phụ; chi phí, số lượng, đuôi đơn vị, timer và cảnh báo mất đồ phải đủ.
- Wallet có số rút gọn và cách đọc số đầy đủ, cùng nguồn dữ liệu. Không đổi formatter làm sai precision của command.
- Fixture tối thiểu: “Cửu Thiên Tiên Miện”, “Vạn Kiếm Quy Tông Trận”, “Huyết Ma Hóa Sinh Trận”, “Đột Phá Đan · Cực phẩm”, tên người chơi dài có dấu; chuỗi số lớn và timer dài.
- Không bake bất kỳ tên, số, giá, phần trăm, countdown hoặc thông báo nào vào sprite.

## 5. Spacing, safe area và nhịp bố cục

Grid cơ sở6; spacing12/24/36/48/72/96. Lề ngang safe content36. Inset safe area áp **một lần** qua `SafeAreaFitter`; background được tràn viền. Đơn vị ảnh, ref pixel và dp thiết bị không đồng nhất.

| Thành phần | Kích thước reference |
|---|---:|
| Hit nhỏ nhất |132×132 |
| Action thường xuyên |144×144 hoặc cao144, rộng theo nội dung |
| Dock | cao176;5 cell×192 +4 gap×12 +2 margin×36 =1080 |
| Dock icon / utility icon |72 /48; hit region không theo kích thước icon |
| Quick access / Skill row / Vital envelope |144 /176 /96 |
| Profile-wallet envelope / Quest gọn | khoảng232 /tối đa108 |
| Workbench header/footer | mỗi phần tối thiểu144 |
| Page header/footer | tối thiểu168; dài chữ tăng chiều cao |
| Padding card / panel |36 /48 |
| Modal | margin48; rộng tối đa960; body cuộn |
| List row | tối thiểu168; wrap cần cao hơn |
| Slider | track12, thumb60, hit cao144 |
| Toggle | hình72, hit144; nhãn body42 |

Giữ công thức sheet Phase 02:

`sheetH = min(0.55 × safeH, safeH -232 -108 -176 -144 -320 -48)`.

Với safeH1920: tối đa892, trừ header/footer144 còn604 cho body cuộn. Màn thấp cần compact/expanded page mode của **cùng route và state**, không ép fit chữ vào một card.

Canh trái cho nội dung, canh phải cho giá trị/cost có cột ổn định; label và số baseline cùng hàng. Icon-text gap12/24; tách section36/48. Row hành động không nằm sát gesture inset. Hit region không đè nhau dù vùng nhìn nhỏ.

Không đặt panel nền đặc tràn toàn bề ngang trên HUD farm chỉ để “đẹp”. Mục tiêu arena nhìn rõ của Phase 02 là mục tiêu thiết kế, chưa đo hiệu quả trên thiết bị.

## 6. Panel, border và composition

| Recipe | Vật liệu / viền | Nội dung |
|---|---|---|
| PageFrame | ink/surface; header có divider vàng mảnh | PageHeader → tabs tùy chọn → viewport → footer |
| WorkbenchSheet | Window/Panel, hai góc trên có ornament | Header/handle → tài nguyên/gate → thao tác → footer cố định |
| RaisedCard | raised; border3 hoặc divider | Một nhóm dữ liệu, không thêm khung vàng quanh từng dòng |
| InsetWell | inset, viền tối | Slot trống, track, phần so sánh |
| DecisionDialog | Window, max4 góc; dimmer.72 | Title → hậu quả rõ → context item/slot → cancel/confirm |
| RewardDialog | Window, ornament đầu bảng | Dữ liệu thưởng thật → nút claim/close đúng policy |
| ItemSlot | Slot, strip rarity6 ở một cạnh | Icon, level, quantity; badge equipped và selection là lớp riêng |
| Tooltip/Popover | surfaceRaised, padding36 | Nội dung vừa viewport; không che nút thao tác bắt buộc |

Radius mục tiêu12 control nhỏ,24 panel,36 modal; dùng artwork/corner đúng cấu trúc, không chồng thêm rounded mask lên khung đã khắc góc. Viền thường3ref; focus/selection6ref. Line divider2ref chỉ trang trí. Những đường có vai trò control dùng focus/gold đủ tương phản.

Hoa văn đầu bảng: tối đa opacity.08; vân nền.035. Vùng chữ có luminance thay đổi mục tiêu≤4%; không đặt vân sáng ngang baseline. Mỗi sheet chỉ một header ornament hoặc medallion. Nút thường không mang cả mây, vòng trận, đá quý và tia sáng cùng lúc.

Sáu prefab Jade hiện có là art building blocks; không coi chúng đã có data binding/navigation. Không gắn command mới vào một prefab trang trí.

## 7. Buttons và trạng thái tương tác

Các recipe primary/secondary/destructive/reward dùng chung cấu trúc: face/frame → icon tùy chọn → label → indicator tùy chọn. Frame và mặt nhìn có thể co. Hit RectTransform cố định, không scale theo press.

| State | Hình thức | Nội dung/hành vi |
|---|---|---|
| Normal | Mặt jadeDeep + nhãn ivory; border gold | Một động từ rõ, dữ liệu giá từ nguồn thật |
| Pressed | Overlay mực.14; scale visual.98 trong70ms | Không dịch layout hoặc kích hoạt command sớm |
| Focused | Vòng goldBright6ref, inset6 | Hiện cho keyboard/gamepad; không cần hover để đọc |
| Selected | Viền inset + dấu/nhãn “Đã chọn” | Không ghi đè strip rarity của item |
| Disabled | Mặt inset, chữ muted vẫn đặc; icon alpha.55 | Có lý do, không giảm cả CanvasGroup làm mờ chữ |
| Locked | Lock + “Chưa mở” + điều kiện | Điều kiện cảnh giới/parent/service đang dùng |
| Insufficient | Chi phí đỏ + “Thiếu tài nguyên”, có sở hữu/cần | Không giả vờ đủ tiền từ visual animation |
| Pending | Label “Đang xử lý…” + indicator; giữ rộng nút | Ngăn tap lặp, chỉ dùng cho thao tác thực có chờ; không bịa xác nhận server |
| Cooldown | Dark fill.78 + thời gian rõ | Mana/lock vẫn đọc; fill chỉ projection của service |
| Max | “Tối đa”, tắt CTA | Không in Infinity/int.MaxValue thành giá |
| Claimed | Check + “Đã nhận” | Không animate như chưa nhận để kích click |
| Error | Nhãn lỗi đọc được, retry view khi phù hợp | Không retry transaction/grant tự động |
| Success | Check + accent ngắn sau kết quả thật | Không cộng thưởng trong animation callback |

Primary: ngọc tối, chữ ivory. Secondary: raised, viền vàng tiết chế. Destructive: dangerSurface, chữ ivory và warning icon, đồng thời giữ confirm của caller. Reward: mặt vàng sáng trung tính, chữ ink; **atlas hiện có đã có màu**, không tint toàn bộ bằng màu vàng rồi mặc định đạt contrast. Nếu cần recipe mới, dùng neutral face/overlay hoặc asset có kiểm tra.

Thứ tự giải quyết: binding error → pending → locked → cooldown → insufficient → max/claimed → pressed/selected → normal. Max ẩn cost không hợp lệ. Selected/equipped/focus/quality là trục độc lập, có thể đồng thời xuất hiện. Tooltip/nhãn vẫn giải thích nguyên nhân thực khi một action bị chặn.

## 8. Hệ icon thống nhất

Hai họ, cùng optical grid:

- **Illustrative feature icons:** jade/gold, một silhouette chính + tối đa một accent; ánh sáng trên trái, tránh nền phong cảnh. Main dock dùng Sword/Body/Alchemy/Artifact/Formation hiện có. HP/MP, quality và currency không dùng chung một biểu tượng mơ hồ.
- **Utility glyphs:** nét đơn, round cap/join, hình học sạch; không texture, không vàng nổi3D. Close/back/lock/check/info/filter/sort/grid/list/save/volume dùng cùng stroke family. Neutral mask được tint bằng semantic token.

Illustrative authoring512², safe inset64, core384²; utility256², inset32, stroke16. Hiển thị thường72/120/192ref, utility48/72. Giữ aspect; không kéo méo và không nine-slice icon. Artwork nhỏ phải nhận ra khi bỏ màu; không dùng chi tiết mảnh dưới24px trên source512 nếu hiển thị72ref.

Mỗi control có text hoặc tên truy cập tương đương; icon không thay thế hoàn toàn điều kiện/cost. Màu sắc có shape/mark bổ sung. Chuỗi Unicode “X”, “✓”, emoji không thay bộ icon; font renderer có thể thiếu glyph hoặc bị RemoveIcons.

Manifest định nghĩa4 skill,6 body node,4 artifact,6 formation,3 herb,2 recipe-pill,2 material,5 trial icons. Một icon recipe được tái dùng cho9realm×quality với label/badge riêng; không cần144 ảnh đan. Đạo Ấn38 node giữ ID/text và icon-category dùng lại; không bịa thêm cây vận mệnh từ hình vẽ.

## 9. Rarity, tiến trình và loadout

### Kiếm: cấp hiện có, không thêm phẩm chất loot

| Level | Accent đề xuất | Mark bổ sung |
|---|---|---|
|1 | #BBC6CA | một notch + Lv.1 |
|2 | #99D9A6 | hai notch + Lv.2 |
|3–4 | #91C8FF | ba notch + level chính xác |
|5–7 | #D1ABF3 | bốn notch + level chính xác |
|8 | #FFB17D | năm notch + Lv.8 |

Các band theo `StorageModalUI.GetRarityColorForLevel`, chỉ làm dịu/tăng đọc màu. **Trang bị** dùng badge/viền vàng bên trong, không thay màu cấp kiếm. Selected là inset frame; high-value warning là icon riêng. Chỉ tăng ornament trong cùng token rules, không tạo “Legendary/Mythic” mới.

Capacity lấy thật từ service/array, có thể10, không dùng /4 cố định. Slot logic trống không có child trang trí: `MergeManager` kiểm `childCount`. Frame là root Image; lock/label/selected overlay ở **DecorationLayer sibling**. Không thay MergeItem identity hoặc names/index Equip.

### Đan: đúng PillQuality và hai recipe

| Enum | Tên | Accent / cue |
|---|---|---|
| Waste | Phế đan | #ADB7BA, nét đứt/broken outline |
| Basic | Sơ phẩm | #A0DF8A, một diamond nhỏ |
| Refined | Trung phẩm | #91C8FF, hai diamond |
| Supreme | Cực phẩm | #F3D995, ba diamond |

Tu Vi Đan có spiral đi lên; Đột Phá Đan có cloud-split/lightning mark. Quality là badge/border, recipe là icon silhouette, cảnh giới là nhãn. Không đổi xác suất hoặc bonus từ art. Label tiếng Việt chuẩn hóa là presentation dự kiến, chưa sửa enum/binding.

### Cảnh giới và các hệ tiến trình

- Chín cảnh giới: Luyện Khí, Trúc Cơ, Kim Đan, Nguyên Anh, Hóa Thần, Luyện Hư, Hợp Thể, Đại Thừa, Độ Kiếp. Seal1–9 cung nhỏ đi cùng tên rõ; màu accent được liệt kê trong token JSON.
- Pha: Sơ Kỳ / Trung Kỳ / Hậu Kỳ / Đỉnh Phong qua `CultivationDisplay`. Giữ mapping save legacy15stage Luyện Khí và projection thật; không ép9×10 vào dữ liệu chỉ vì visual có9seal.
- Body: hai nhánh **Cương Thân** (jade + shield mark) và **Thần Lực** (warm red + force mark). Cạnh prerequisite rõ, không dùng glow làm gate. Index `0→2→1` và `3→4→5` đúng source; giữ parent level/realm và grandfathered existing levels.
- Artifact: Robe/Crown/Boots/Amulet, nhãn current UI: Phi Thiên Áo / Cửu Thiên Tiên Miện / Vân Phong Hài / Linh Ngọc Bội. `ArtifactService.AuraTier` là tổng hợp0..9, không vẽ aura rarity riêng từng item nếu service không có.
- Formation: Thunder/Spirit/Frost/SwordRain/BloodDrain/GoldenBell; icon và tên riêng. “Đang dùng”, “Đã mở” và Spirit active/cooldown có badge khác nhau, không gom thành một viền sáng.
- Đạo Ấn:38 ID catalog,6tier,8filter theo inventory. Tier seal đánh số bằng runtime Text; giữ node-specific cost, prerequisite và bonus. Không gọi tier là một loại rarity mới.

## 10. Các biến thể theo hệ thống thật

Tất cả dùng cùng palette/type/spacing/frame, chỉ thay một motif và accent. Không làm13 bộ skin rời.

| Nhóm / tên visual | Route hoặc nguồn hiện có | Composition và trạng thái cần giữ |
|---|---|---|
| Main menu · Vân Sơn Khởi Hành | title.home/characters/create, enter/delete/quit confirm | Portrait tiên cảnh tràn viền; moon/cultivator không bị kéo méo. Title lớn, CTA Continue rõ,3slot và tạo tên trên nền đặc. Empty/occupied/name-empty/delete confirmation có cùng visual grammar |
| Gameplay HUD · Ngự Kiếm Minh Nguyệt | battle.home/shortcuts, quest/guide/tooltip/log | Các island nhỏ cho profile/wallet/realm; HP/MP/tu vi phân biệt;4skill có tên/cost/cooldown/lock.5dock và4quickaction; không lấy background title che world |
| Upgrade · Đạo Lộ Ngọc Giản | Công pháp, Thần thông, Đạo Ấn, Mốc, Thành tựu, Đan hỗ trợ, Kiếm Lô | Card level→next,owned/required,cost,CTA.4legacy upgrades và4study/CD vẫn riêng; claim một/tất cả đúng dataset;5buff local không giả IAP |
| Cultivation · Cửu Cảnh Liên Đài | progression.character, modal.tribulation | Một halo ngọc nhỏ, realm/phase/stat hierarchy,capacity đúng. Chuẩn bị độ kiếp có đan đang dùng,tỷ lệ/điều kiện thật,cảnh báo và cancel; chỉ sau kết quả mới success/fail accent |
| Alchemy · Đan Hỏa Thanh Ngọc | workbench.alchemy / AlchemyService | Cauldron/vapor ở header; purple spirit flame là profession accent.3herb,2recipe,realm,dust,inventory quality. Idle/brewing/ready/no-ingredient/cooldown/max/claim thể hiện bằng nội dung rõ |
| Artifact crafting · Luyện Khí Kim Văn | workbench.artifact / ArtifactService | Bốn card slot cố định,quặng/Linh Thạch,level và bonus; icon đúng SlotNames hiện có. Unlocked/insufficient/max và thành công nâng cấp; không thêm “rèn mới” |
| Body refinement · Song Mạch Luyện Thể | workbench.body, body.node-detail | Hai branch,6node đúng index,link prerequisite,stat/lore ở dialog. x1/x5/max và khóa bởi realm/parent không gộp thành nút chung |
| Formations · Lục Trận Huyền Văn | workbench.formation / FormationService | Sáu formation cards: Cửu Thiên Lôi, Bát Quái Tụ Linh, Cực Hàn Băng, Vạn Kiếm Quy Tông, Huyết Ma Hóa Sinh, Huyền Vũ Kim Cang. Select/upgrade và Spirit activate/time/cooldown giữ độc lập |
| Inventory · Tàng Các | inventory.items/info/sacrifice-lv8, confirm.dismantle | Full page,5group/filter,pending stash,3preference toggles,grid/list. Quantity dễ đọc,selected/equipped/high-value phân biệt. Dismantle cảnh báo rõ số lượng/giá trị và đúng caller; không auto-confirm khi host thiếu |
| Equipment · Ngự Kiếm Pháp Y | workbench.sword, progression.character, workbench.artifact | Kiếm có equipment capacity tới10 + item level;4artifact ở đúng nơi hiện có. Touch pick→target/drag/info cùng commands; không tạo slot giáp/vũ khí mới hoặc paper-doll inventory mới |
| Combat · Bí Cảnh Kiếm Ảnh | battle.style/encounter, challenge.hub/play và5trial | Vạn Ma/Cấm Chế/Tam Đỉnh/Lôi Trì/Kiếm Chủng có icon riêng và cùng timer/score grammar. Telegraph rõ, boss HP ưu tiên; rời ải có confirm. Nhãn Kiếm Chủng phản ánh4/6/8 theo source, không đổi reward |
| Rewards · Kim Quang Hồi Báo | challenge.result, modal.offline | Vàng tập trung header, actual rewards/quantity làm nội dung. Empty/zero không dùng sample jackpot. Offline x1/x2 và SDK pending/unavailable/cancel/error/duplicate guard giữ đúng policy |
| Settings · Tĩnh Tâm Các | settings.audio/session và return/quit confirms | Flat rows,không texture dưới slider. Giá trị âm thanh/toggle/save đúng implementation; tách quit/return và confirm. Save API void chỉ nói “đã gửi yêu cầu” khi chưa có xác nhận ghi |

Ghi chú Inventory: “phân biệt” ở đây là badge/cue độc lập cho từng trục trạng thái; không được dùng một màu thay cả selected/equipped/high-value.

Các confirm còn lại dùng DecisionDialog chung. Route lỗi dùng error message rõ + quay về caller/retry view. Hai route lifecycle save/input không có art. Diagnostics dùng style trung tính và nhãn minh họa; không xuất hiện như feature production. Manifest liên kết **mọi57route** với variant, decision/error/nonvisual/development policy.

## 11. Shadows và chiều sâu

| Elevation | Offset / softness ref | Alpha | Dùng |
|---|---|---:|---|
| flat |0,6 /12 |.24 | Control/card nổi nhẹ |
| panel |0,12 /24 |.32 | Sheet/page card |
| modal |0,18 /36 |.40 | Dialog trên dimmer |

Màu shadow #020B10. Softness là chỉ dẫn authoring alpha sprite, không yêu cầu blur shader realtime. Dùng một shadow sprite tái sử dụng hoặc artwork đã có; không chồng `Outline` + `Shadow` + bloom để tạo viền. Khung có shadow bake thì không cộng lại shadow recipe cùng cường độ.

Shadow/glow được tách khỏi content/hitbox, raycastTarget=false. RectMask không cắt nội dung bắt buộc hoặc dấu chữ. Glow cần vượt clip chỉ đặt ở host thích hợp, không thêm Canvas riêng cho từng card.

## 12. Visual effects và lighting

UI có ba cấp:

1. **Idle:** không loop trên card thường; halo optional ở một điểm focus, alpha≤.16. Không chiếm spotlight của sword/boss.
2. **Action:** press tint, selection ring, progress tween ngắn. Chưa nhận command success thì không hiển thị thành công.
3. **Milestone/reward:** accent320ms trong header, tối đa8 sprite nhỏ theo budget thiết kế; Low VFX dùng static check/không particle.

Mọi giới hạn trên là **budget đề xuất, chưa profiling**. Không tuyên bố giảm draw call hoặc tăng FPS. Đo overdraw,Canvas rebuild,texture residency và thiết bị thấp trước khi chốt budget runtime.

Giữ VFX world/sword/telegraph đang hoạt động, không đổi tốc độ/timeScale/timing combat. Không recolor danger telegraph thành gold chỉ để khớp skin. Không full-screen blur, flash trắng lặp hoặc brightness pulse ở mọi nút. Low-HP edge nếu có vẫn do combat state hiện có; UI không tự phát trạng thái nguy hiểm giả.

Các style có Low VFX/reduced-motion rendering variant. **Không thêm nút Settings chưa tồn tại trong phase này**; dùng setting hiện có nếu phù hợp khi tích hợp, hoặc để static fallback.

## 13. Animation language

Thời gian dùng **unscaled UI clock**, không sở hữu `Time.timeScale`. Hành vi pause/slow motion của gameplay/menu hiện có được giữ; nếu owner đổi timeScale thì phải restore giá trị trước đó.

| Chuyển động | Duration | Easing / cách dùng |
|---|---:|---|
| Press down / release |70 /100ms | Visual child scale1→.98→1, hit rect cố định |
| State/focus selection |100–120ms | Fade/border, không bounce |
| Sheet open / close |200 /180ms | Y translate, easeOut [0.22,1,0.36,1] |
| Page open / close |160 /140ms | Fade + travel tối đa24ref |
| Modal open / close |140 /120ms | Fade + scale.98→1; dimmer độc lập |
| Progress gain |180ms | Visual value tiến tới snapshot, không trì hoãn ready |
| Claim accent |320ms | Sau success thật; không gắn grant ở OnComplete |
| Toast |140 in /2400 hold /140 out | Không che action/mana/quest cần thiết |
| Loading |1000ms loop | Một indicator khi thật có chờ; low variant static hourglass |
| Optional tutorial focus |1600ms | Chỉ một target, amplitude nhẹ; low variant tĩnh |
| Damage text |650ms dự kiến | Giữ backend/world renderer; timing thực tế cần test, không thay combat clock |

Không spring overshoot, camera shake hoặc haptic mới do skin. Các timed trial dùng giờ/timer service; animation không tự pause trận. Chuyển compact↔expanded giữ một route instance và view state.

Interrupt/back/pause phải hủy animation cũ, bind/focus đúng generation, không để invisible raycast blocker. Dimmer chặn GraphicRaycaster **và bridge input polling/Physics** của Phase 02; không dùng dimmer để tự dừng autosimulation. Back/close/offline claim/trial leave vẫn theo navigation policy, không suy ra hành vi từ opacity.

Low-motion: fade80ms, không drift/rotate/scale punch/ambient glint; giữ label,focus ring,progress,timer và thông tin thành công dạng tĩnh. Đây là renderer variant dự kiến, chưa có control mới được triển khai.

## 14. Sprite sizing và nine-slice

`JadeGoldAtlas.png` hiện có **1254×1254 RGBA**, không phải2048. `ImmortalRealmPortrait.png` là **941×1672 RGB**, không alpha. Manifest ghi SHA-256,GUID,source dimensions và subSprite rect thực; không dựa trên kích thước đã yêu cầu image generator ở công việc trước.

| Sprite atlas hiện có | Border L/B/R/T | Dùng |
|---|---|---|
| Panel / Window / Slot / SelectedSlot |30/30/30/30 | `Image.Type.Sliced` |
| PrimaryButton / SecondaryButton / ResourceBar |52/16/52/16 | `Image.Type.Sliced` |
| Divider |0/0/0/0 | Simple, giữ aspect; không có nine-slice |
|8 icon còn lại |0/0/0/0 | Simple, preserveAspect |

Có **7 sprite border khác0**, không phải8. Không chia lại atlas bằng16ô vuông đều: rect button/bar/icon được crop khác nhau và có tọa độ fractional. Không thay GUID của asset đang được tham chiếu.

Quy tắc source mới trong manifest:

- Icon512²/utility256²; source alpha thật, margin trong suốt; pivot trung tâm. Frames dự kiến512² hoặc button768×192; border48 hoặc72/36 theo từng entry, cần review thực tế.
- Frames: corner chứa toàn ornament, mép thẳng, center stretchable yên. `L+R < width`, `B+T < height`, và đích không nhỏ hơn tổng border đã scale. Padding content lớn hơn ornament band.
- PPU100; Canvas reference PPU100; `pixelsPerUnitMultiplier=1` như theme hiện tại. Hãy xác minh thickness cuối cùng trên Canvas; không coi PPU là dp.
- UI: sRGB,Bilinear,Clamp,mipmap off,Read/Write off theo profile dự kiến; full rect cho frame sliced, không mesh tight xoay packed frame. Pack padding8px/extrude4px là mục tiêu future import.
- Transparent PNG giữ straight alpha, không matte trắng/đen/checkerboard. Kiểm halo trên nền tối/sáng/magenta. RGB background không ép thêm alpha chỉ để thống nhất file.
- Compression mobile là lựa chọn phải QA: thử ASTC6×6 cho art mềm;4×4 hoặc RGBA32 cho line mảnh nếu cần. Không tự đổi import/platform override ở phase này; ghi lại dung lượng/residency thực trước release.
- Không bake mọi state thành ảnh: label/lock/cost/focus/cooldown là lớp UI tái dùng. Chỉ tạo artwork mới khi silhouette/material thật sự khác.
- Generator có thể trả kích thước khác target. Ghi actual dimensions trước, kiểm alpha/content rồi chuẩn bị export chính xác không ghi đè nguồn; cập nhật border theo export. Không import file chỉ vì prompt đã ghi kích thước.

## 15. Image manifest và quy trình sản xuất

Manifest chứa **84 entry**:

- 18 nguồn/subSprite reuse: nền + atlas +16subSprite.
- 32 icon minh họa dự kiến cho feature thực tế.
- 6 ornament optional,3 neutral-frame conditional khi atlas không đạt QA.
- 25 utility glyph cần author deterministic; không dùng image generation cho nét đơn.

Có **41 job imagegen được mô tả**, gồm32core icon +6optional ornament +3conditional frame. Không đồng nghĩa41ảnh đã sinh hoặc đều bắt buộc. Priority P1:4skill còn dùng icon category;P2: body/artifact/formation/material/trial nếu thiếu nguồn phù hợp;P3 chỉ khi có ích sau QA. Reuse art hiện có là mặc định.

Mỗi job có ID,sourceFeature/route,style-reference ID,target path,kích thước,alpha,prompt,negative constraints,safe inset,sprite border và fallback phù hợp. Các path `Assets/Art/UI/RedesignV3/...` là **đích dự kiến, chưa có file hoặc import**.

Future production workflow:

1. Kiểm lại asset hiện có và chọn reuse trước; không overwrite atlas/meta.
2. Với bitmap mới dùng built-in `image_gen`,mỗi asset một job. View source local trước khi edit; ở manifest hiện tại nguồn là style reference, không phải edit target.
3. Giữ prompt/provenance,actual dimensions,hash,tool/model info nếu có và QA disposition; chép output dùng trong project vào Assets trước khi tạo reference.
4. Chuẩn bị source alpha/border/size riêng,không nhờ generator đóng gói atlas. Không sinh chữ/số/timer/cost.
5. Import/9-slice/prefab qua Editor/Pipeline đúng phase implementation; tạo reference chính xác và giữ ID/gate/binding.
6. Chụp từng state và điều kiện đọc nhỏ; kiểm compression và profile trên thiết bị. Chỉ lúc đó mới đánh dấu asset ready/imported.

## 16. Integration không làm mất tính năng

- Token file hiện là dữ liệu thiết kế có `$type/$value`, chưa có importer runtime. Semantic reference kiểu `{color.semantic.text}` phải được resolve khi tích hợp; không gán chuỗi vào field Color của Unity.
- Skin không tự discover command bằng tên nút. Một action một owner, không persistent UnityEvent cộng thêm runtime listener trùng.
- UIRegistry/AppRouter/OverlayHost và các adapter mới vẫn là đề xuất Phase 02. Không coi skin hiện có đã giải quyết hardcoded Find/path/Canvas builders.
- Mọi field displayed,animation,state,callback và gate của inventory giữ parity. UI mới được phép có typography/icon/border khác, không được đổi service command hoặc save schema.
- Chú ý case đã ghi nhận: capacity/4,Kiếm Chủng copy4/6/8,result sample jackpot,dismantle missing-host fallback,trial Back,body gate,legacy skill CD. Visual phải cho phép biểu diễn đúng,không lấp lỗi bằng icon/animation.
- Settings chỉ có setting thật; “Low VFX/reduced motion” là rendering specification, không phải bằng chứng nút mới đã hoạt động.
- Error panel/empty state không chứa phần thưởng mẫu hoặc nút giả. Thông báo save phân biệt request với outcome thật; không dùng logo cloud/check để tuyên bố đã sync backend.

## 17. Cổng nghiệm thu

**Tĩnh trong Phase 03:** JSON parse,alias/type/unit,color format,contrast calculations,sprite border arithmetic,asset path/hash,route coverage57/57 và asset-reference integrity. So sánh hash source/settings/packages/AGENTS,docs Phase00–02,Git HEAD/branch/index và18 save data files trước/sau. Kết quả chi tiết ghi cuối tài liệu và JSON.

**Sau implementation, chưa chạy trong Phase 03:**

- Preview mọi variant ở1080×1920,720×1280,1080×2340/2400; logical360×640 và390×844; notch/home inset,keyboard,short viewport.
- Tiếng Việt đủ dấu: chữ có dấu kép ở đầu/cuối dòng,hai dòng và tên dài; số lớn/giá/timer không bị cắt; display font chỉ ở role lớn.
- Empty/ready/locked/insufficient/pending/error/success/max/selected/equipped/claimed/cooldown,grid/list,body dependency,state restoration.
- 1 tap = 1 command;drag/scroll/touch-info;Back đúng lớp;modal block pointer và polling input;offline/trial callback không grant trùng.
- Test timeScale0/.4/1,animation interruption,pause/keyboard;Low VFX không mất thông tin và không strobe.
- Kiểm ảnh thực ở scale nhỏ,9-slice đa kích thước,alpha fringing,contrast trên texture,compression;profile overdraw/CPU/texture memory.
- Thiết bị mobile thật và save/profile isolation. Screenshot hoặc token checks không thay thế thử touch và physical-device QA.

Rollback Phase 03 chỉ cần bỏ/khôi phục ba tài liệu theo [rollback plan](rollback_plan.md); không có scene/prefab/code/art mới phải revert. Giữ nguyên mọi thay đổi người dùng đã có.


## 18. Kết quả xác minh Phase 03

- 177 token;148 tham chiếu token đã resolve,không thiếu/vòng lặp.10/10 cặp màu đạt ngưỡng riêng;contrast nhỏ nhất trong nhóm kiểm là5,46:1. Đây là kiểm tra màu phẳng,chưa kiểm ảnh render.
- 13 visual variants;57/57 route có style policy;84 asset entry có ID duy nhất và tham chiếu hợp lệ.41 job imagegen ở trạng thái kế hoạch;25 utility glyph chưa author.66 target path tương lai được xác nhận chưa tồn tại.
- Atlas16subSprite được đối chiếu meta thực;7 border khác0 và mọi tổng border hợp lệ. Nguồn art/font/đầu vào Phase01–02 có SHA-256 trong JSON.
- Cmap của Regular,Semibold,Display có đủ104 chữ tiếng Việt precomposed được kiểm. Combining marks và giới hạn render ghi trong token JSON;chưa test line box/shaping ở Unity. Nếu input dạng decomposed cần chuẩn hóa NFC cho hiển thị,không sửa tên lưu.
- 2.090 file Assets/Packages/ProjectSettings/root AGENTS đã kiểm hash:0 thay đổi,0 file nguồn mới. Docs Phase00–02 giữ nguyên.18 file save data/backup giữ nguyên hash.
- Git vẫn feature/mobile-ui-redesign tại cedbda443b351aea875ea4b96685b1dc1acbc41a;index giữ nguyên,chỉ thêm3 tài liệu yêu cầu vào trạng thái untracked. Không stage/commit.
- Editor cuối kiểm:SampleScene_MobileWork clean,12root,stopped,không compile,0console error;2warning đã có từ trước. Không vào Play Mode,không save scene và không import ảnh mới.
- Bằng chứng local:[Backups/Phase03-20261112-170523](Backups/Phase03-20261112-170523/). Runtime/device QA và texture contrast thực tế vẫn chờ phase implementation.

# Phase 04 — Portrait mobile HUD prototype

Trạng thái: **đã triển khai một prototype HUD hoạt động** trong `Assets/Scenes/SampleScene_MobileWork.unity`. Reference1080×1920, portrait9:16, uGUI legacy Text, SafeAreaFitter hiện có. Không thay gameplay formulas, save schema, build settings, PC scene hoặc art gốc.

## Kết quả

HUD có upper information mới, profile hai dòng tên, ví tài nguyên rút gọn có trang đọc số đầy đủ, bốn pha tu luyện, quest gọn/mở, floor/kill/boss state, HP và MP, bốn action kỹ năng ngang, bốn quick route trực tiếp, dock năm hệ thống và chế độ focus. Menu Đạo lộ cung cấp Nhân vật, Thành tựu, Hiệu ứng và Nhật ký.

Đây là prototype HUD của Phase04. Inventory/Đạo Lộ/workbench mở các view đang hoạt động hiện có; chưa dựng toàn bộ57route/page router của Phase02. Nội dung năm workbench giữ nguyên controls/data/binding; phase này điều chỉnh shell, layer, entry và shortcut phân giải.

[Mapping máy đọc](hud_feature_mapping.json) chứa955 control/data records thuộc HUD mobile,110 action contract nguồn và identity map toàn scene.955 records không tương đương955 tính năng gameplay độc lập.

## Hierarchy đã lưu

```text
Canvas                                      # CanvasScaler1080x1920, match width0
  MobileUIRoot [SafeAreaFitter, DrawerController, NavigationManager]
    PortraitHudShell [PortraitHudController]  # sibling đầu, không thêm full-screen raycast graphic
      UpperInformation
        CharacterFrame                       # object gốc, click handler gốc
          InfoText                           # source cũ giữ reference, renderer tắt
          PlayerName                         # hai dòng42ref
          RealmName                          # projection phụ ẩn; realm hiển thị trong StagePanel
        TopBar                               # ví; hai Text object gốc
        PauseButton                          # handler gốc
        StagePanel                           # realm + tu vi +4phase + tribulation gate
        TutorialTargetQuestBar
        StageUI                              # floor, kill progress, boss timer
        JadeMenuButton                       # entry Đạo lộ
      PrimaryActions
        VitalBars
          PlayerHPBar                        # nguồn CombatManager giữ nguyên
          PlayerManaBar
            ManaText                         # trạng thái skill gốc
            ManaValue                        # số MP đọc từ SkillManager
        MobileSkillHUD                       # bốn Button gốc, layout ngang
        QuickAccessBar                       # Kho / Bí cảnh / Vận khí / Mốc
      SwordStyleActionHost                   # nhận button runtime gốc, giữ listener/gate
      CombatNotificationBanner
      BottomDockToggle                       # luôn có đường hiện lại HUD
      SecondaryOverlay
        MenuDimmer / MenuCard / DetailsPage
    ActiveBuffPanel                          # alias trực tiếp cho ShopManager factory
    CombatLogPanel                           # alias trực tiếp cho GameLogger factory
    MobileBackdrop
    Drawer_0 ... Drawer_4                     # nội dung và MergeSlot topology giữ nguyên
    MobileDock                               # sibling sau drawer/backdrop, dưới các modal
    BodyTalentDetailModal
  Các modal/gameplay choice/menu hiện có      # giữ host, source và lifecycle
```

Không tạo Canvas/EventSystem cho từng card. Không thêm EventSystem thứ hai. Dock giữ một layer sibling có chủ đích để backdrop không che tab; menu phụ khóa CanvasGroup của dock khi mở. Các modal bên ngoài MobileUIRoot vẫn nằm trên HUD.

## Bố cục reference thực tế

Tất cả số dưới là Canvas reference unit, không phải dp thiết bị. Safe-area inset áp một lần tại MobileUIRoot.

| Vùng | Hình học / quy tắc |
|---|---|
| Profile | x36, top24,534×144; avatar120; tên42ref trong112ref cao, đủ hai dòng fixture |
| Wallet | right180, top24,318×144; hai dòng resource42ref; nhấn để đọc số đầy đủ và quặng |
| Pause | right36, top24,132×144 |
| Tu luyện | top180, rộng852, cao156;48ref label +48ref value +48ref phase |
| Đạo lộ | right36, top180,132×144 |
| Quest | left36, top348,852×144 khi mở;144×144 khi gọn |
| Focus toggle | right36, top348,132×144 |
| Floor / boss | left36, top504,636×94; boss timer có line box48 |
| Kiếm pháp runtime | right36, top504,342×132 |
| Vital bars | bottom532, rộng1008, cao120; hai row60, chữ36 trên nền tối; track6 |
| Skill row | bottom344, rộng1008, cao176; bốn hit rect240×176 |
| Quick access | bottom200, rộng1008, cao132; bốn hit rect240×132 |
| Dock | bottom12, rộng1008, cao176; năm cell xấp xỉ189,6×176, khoảng12 |
| Secondary card | max1008×1100, margin safe36, body theo viewport |
| Phân giải trong Kiếm Lô | shortcut430×132, chữ36; giữ handler mở Kho/phân giải |

Hit nhỏ nhất132ref tương đương44logical tại width360. Nút thường xuyên144ref tương đương48logical. Icon nhỏ hơn vùng chạm; chữ/ảnh trang trí không nhận raycast. Các rect có critical label36ref cần tối thiểu48ref cao vì Source Sans Pro thực tế cần45ref cho một dòng; không dùng hộp44ref.

Tên Việt dài đã kiểm: “Thiên Vân Đạo Hữu Trường Sinh”. Giá trị resource đến hàng nghìn tỷ được rút gọn K/M/B/T, cực lớn dùng scientific notation; detail view đọc giá trị đầy đủ theo độ chính xác dữ liệu nguồn. Không nội suy wallet hoặc tự cộng tài nguyên ở view.

## Visibility và chế độ focus

- Closed HUD giữ world camera/frustum tràn màn hình. Không dùng ảnh nền menu che combat hoặc đổi camera để tạo khoảng trống giả.
- Với reference1080×1920 và không có thông báo tạm, rect trung tâm được báo cáo là1008×608ref giữa các vùng HUD. Đây là vùng clear hình chữ nhật bảo thủ; những khoảng trống quanh các island vẫn nhìn thấy world.
- Focus ẩn quick actions, skill row và dock; giữ HP/MP, thông tin trên cùng, kiếm pháp và nút “Hiện HUD”. Rect clear trung tâm tăng lên1008×1122ref.
- Màn cao hơn tăng phần arena ở giữa, không kéo giãn button theo chiều dọc.
- Không công bố “80% màn hình trống” hoặc lợi ích FPS: chưa đo opaque pixel coverage/overdraw. Các rect trên là hình học, không phải benchmark.
- Quest có collapse riêng. Nhiệm vụ, claim, progress, hướng dẫn và đường vào Mốc vẫn dùng controller nguồn.
- Workbench có thể che phần dưới khi mở; dock vẫn chuyển tab/đóng tab và nút close/swipe/backdrop gốc vẫn còn.

## Dữ liệu và hành vi

| Feature | Nguồn/owner giữ nguyên | Adapter mới |
|---|---|---|
| Nhân vật/realm/tu vi | CultivationManager + CharacterFrameButton | Name/tu vi projection, OnTuViChanged/OnStageChanged; InfoText cũ vẫn là reference nguồn |
| LT/CD | EconomyManager | Một projection event-driven duy nhất trên hai Text gốc; MobileCurrencyText cũ tắt ở scene này để tránh hai owner |
| HP | CombatManager | Chỉ thay vị trí, frame và chữ; reference/fill giữ nguyên |
| MP/cast status | SkillManager | Giữ ManaText cũ, thêm số MP hiện tại/max đọc trực tiếp; so sánh hai giá trị khi HUD hiển thị |
| Bốn skill | MobileSkillHUD | Cùng Button, listener, cost, cooldown, lock và ready data; không có thuật toán cast thứ hai |
| Dock/workbench | MobileDrawerUIController | Cùng index0..4 và unlock service; focus thay presentation của lower HUD |
| Kho/Bí cảnh/Vận khí | Storage/BiCanh/DaoAnButtonHandler | Dời Button gốc thành quick route, không reimplement giao dịch |
| Mốc | TutorialManager.OpenMilestoneModal | Entry trực tiếp mới gọi đúng view đang có |
| Menu phụ | Existing character/achievement/shop/log services | Một menu backdrop/card; Back đóng menu trước drawer/root |
| Kiếm pháp/kỳ ngộ | GameplayChoicesService | Optional BindPortraitHudStyle chỉ chuyển parent/style của action runtime gốc |
| Buff | ShopManager | Giữ năm row, stack/countdown/fill và path lookup gốc; menu có empty state |
| Log | GameLogger | Giữ content/ScrollRect; style row một lần sau khi producer hoàn tất tạo row |
| Pause/save/quit | PauseButtonHandler/PauseMenuManager/DataManager | Không thay save command hoặc time-scale ownership |

World polling trong GameplayChoicesService nhận thêm gate menu phụ; auto combat không bị menu HUD tự dừng. Data subscription được tháo khi disable/destroy và nối lại khi enable/service instance đổi. View chỉ cập nhật text khi dữ liệu đổi; không tạo lại danh sách mỗi frame.

UI transitions dùng unscaled time. CharacterDetailModalUI chỉ dùng unscaled clock khi prototype hiện diện; scene không có prototype giữ nhánh clock cũ. Không đặt timeScale về1 trong controller HUD.

## Art, responsive và state

Tái dùng JadeGoldAtlas/Source Sans Pro đã chốt Phase03. Không sinh ảnh mới, không đổi texture import hoặc ép chuyển TMP. Frame, label, icon và cooldown là các lớp riêng; text quantity/cost/timer không bake vào ảnh.

Normal/locked/insufficient/cooldown/active/ready/selected/claimed vẫn theo service. Ultimate glow được giảm thành accent tĩnh alpha.16, không pulse sáng mạnh. Currency/progression dùng feedback line ngắn. Disabled label vẫn đọc; lựa chọn/khóa có chữ và cue.

Khi đổi kích thước render nhưng reference rect giữ cùng aspect, adapter invalidate mesh chữ để không giữ glyph raster ở scale cũ. SafeAreaFitter tiếp tục cập nhật theo Screen.safeArea. Device Simulator iPhone12 trả1170×2532 với safe area(x0,y102,w1170,h2289), tương đương390×844logical.

Landscape-oriented art/material/scene dùng nơi khác giữ nguyên. Prototype đích portrait; không đổi orientation flags/build profile hoặc tự migrate PC sang bố cục mới.

## Prefab bàn giao

Trong `Assets/UI/MobileHUD/hud_prefabs/`:

1. `PortraitHudShell.prefab`: hierarchy HUD, presenter, skill controls, quick actions và menu.
2. `HudCoreDock.prefab`: năm tab, icon, selected feedback và CanvasGroup.
3. `HudActionButton.prefab`: control144ref, Text/button feedback.
4. `HudResourceChip.prefab`: resource skin component dùng lại.

Shell và dock là hai phần của scene prototype; prefab không chứa bản sao game managers. Khi tái dùng phải bind vào MobileUIRoot/DrawerController, service scene và các factory alias đã nêu. Scene mobile đã lưu là mẫu tích hợp đầy đủ; không instantiate thêm một HUD cạnh instance hiện có.

Tool Editor:

- `TuTien/UI/Portrait HUD/01 Build functional prototype`: chỉ dựng lần đầu trên mobile scene đã checkpoint; từ chối rebuild nếu đã có.
- `02 Reflow existing prototype`: lưu lại bố cục/prefabs đã có, không xóa controls.
- `03 Start isolated validation`: chỉ dùng profile `portrait-hud-p04-qa`.
- `04 Cleanup validation marker`: gỡ marker QA, kiểm production PlayerPrefs.

Các thao tác scene/prefab được thực hiện bằng Editor API qua Pipeline, không sửa YAML thủ công.

## Xác minh và rollback

Đã giữ1.327/1.327 object scene ban đầu, không mất object; cả5 persistent Button events giữ nguyên target/method. Scene cuối1.358 object,0 missing script.955 record HUD có mapping duy nhất;110 contract nguồn được liên kết. Chi tiết runtime và ảnh trong [hud_visual_report.md](hud_visual_report.md).

Chỉ5 file có sẵn thay đổi: mobile scene và bốn presentation bridges/controllers. Packages, ProjectSettings, PC module, imported art, các tài liệu Phase00–03,18 file save cũ, Git HEAD/branch/index giữ nguyên. Override chạy nền của QA đã hoàn tác và hash settings khớp baseline.

Rollback checkpoint: `Docs/UI_Redesign/Backups/Phase04-20261112-173028/hud-source-before.zip`, SHA256 `386c802fbe246af94b9ef3e25573f81be69c0753bfd3f9bb57bae3674005f712`. Manager bridge gốc còn trong `GameplayChoicesService.cs.before`, được lấy từ backup có hash khớp chính xác Phase04 start.

Khi rollback: dừng Play, sao lưu công việc mới của người dùng, giải nén vào vùng recovery riêng, review rồi phục hồi chọn lọc scene/scripts bằng Editor/asset nguyên vẹn. Chỉ bỏ bốn script mới và folder prefab của phase này sau khi gỡ references. Không reset/clean/stash, không ghi đè save hoặc các file không thuộc phase. Nhánh Git vẫn `feature/mobile-ui-redesign`; chưa stage/commit.

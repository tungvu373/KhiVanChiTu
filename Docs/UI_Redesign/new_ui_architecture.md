# Phase 02 — New mobile UI architecture

Trạng thái: **thiết kế, chưa triển khai UI**. Reference 1080×1920 portrait; Unity 6000.6.4f1, uGUI 2.6.0, URP 17.6.0, Input System 1.20.0. Không đổi scene/prefab, service gameplay, dữ liệu cân bằng, save hoặc PlayerSettings trong phase này.

Nguồn chuẩn: [feature_inventory.json](feature_inventory.json), [feature_parity_matrix.md](feature_parity_matrix.md), GDD mục 3 và brief mobile. Inventory hiện bao phủ 48 nhóm màn/luồng và 7.712 dòng control/data/callback/contract, không phải 7.712 tính năng gameplay độc lập. Source/scene đối chiếu trước Phase 02 không thay đổi so với inventory. Các đề xuất trong brief chưa có implementation/backend không được coi là feature đang hoạt động.

Bàn giao kèm [navigation_flow.md](navigation_flow.md), [screen_redesign_map.json](screen_redesign_map.json) và [ui_component_architecture.md](ui_component_architecture.md).

## 1. Quyết định kiến trúc

- **Năm bàn thao tác lõi** vẫn là Kiếm Lô, Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp. Giữ tên/thứ tự giúp nhận diện và tương thích chỉ số controller; không giữ cây Canvas/MobileUIRoot cũ.
- **Quick access** đưa Kho, Bí cảnh, Vận khí và Mốc lên một hàng nút trực tiếp. Không bắt mở Tiện ích trước. Chế độ gọn vẫn có disclosure/popover và dismiss để giữ tương đương toggle/đóng menu cũ.
- **Đạo Lộ** là hub full-screen dùng chung page shell, mở từ avatar. Các tab riêng: Nhân vật, Công pháp, Thần thông, Đạo Ấn, Mốc, Thành tựu, Đan hỗ trợ. Không hợp nhất command, currency hay gate giữa các tab.
- **Kho đồ** và **Bí cảnh** là full-screen: dành không gian cho danh sách/filter và mini-game. Workbench dùng bottom sheet để còn nhìn thấy chiến đấu.
- **Overlay host duy nhất** sở hữu modal, popover, focus và Back. Không để một service tự tạo Canvas sorting riêng bên ngoài app shell.
- **Game services và save boundary giữ nguyên nguồn dữ liệu chuẩn.** Router chỉ quản lý view/navigation. UI không tự cộng/trừ ví, grant reward, reset slot, đổi công thức hoặc tạo backend giả.
- Hierarchy ngoài UI được giữ. Cấu trúc slot mà MergeManager/MergeItem phụ thuộc được giữ trong một subtree nhỏ và có lý do cụ thể; toàn bộ outer layout có thể thay.

## 2. Danh mục và mức ưu tiên

| Khu vực | Mục đích | Entry chính | Bề mặt |
|---|---|---|---|
| Khởi hành | Chọn/tạo/tiếp tục đúng nhân vật, âm thanh, thoát | App start | Title pages + decision dialog |
| Chiến đấu | Cảnh giới/quest/HP/MP, bốn kỹ năng, kiếm pháp/kỳ ngộ | Sau xác nhận nhân vật | HUD nổi trên arena |
| Năm workbench | Ghép/trang bị/nâng cấp/luyện đan/chọn trận | Dock 5 tab | Bottom sheet |
| Kho | Xem mọi nhóm, hòm tạm, ưu tiên nhận, grid/list, phân giải | Quick access; shortcut Kiếm Lô | Full-screen page |
| Đạo Lộ | Thông tin và phát triển dài hạn, claim mốc/thành tựu | Avatar; shortcut Vận khí/Mốc | Full-screen tab pages |
| Bí cảnh | Chọn một trong năm ải, chơi, kết quả | Quick access | Hub + trial page + result |
| Quyết định | Xác nhận enter/delete/quit, độ kiếp, offline, phân giải | Caller có ngữ cảnh | Typed modal |
| Diagnostics | Source/UI lịch sử, dữ liệu online mock, control test không hoạt động | Development-only diagnostics | Tách khỏi luồng người chơi |
| Lifecycle | Save, InputSystem, world damage/HP | Event tự động hiện có | Không tạo nút giả |

## 3. Hierarchy portrait mới

```text
Canvas                                  # giữ root alias khi bridge còn cần GameObject.Find("Canvas")
  AppRoot                               # CanvasScaler 1080x1920, match width=0
    FullBleed                           # background decorative, không nhận input
    SafeAreaRoot                        # SafeAreaFitter hiện có
      ShellCanvas                       # sorting 0
        TitleHost                       # home / roster / creation
        BattleShell
          FloatingProfileWallet
          RealmPhaseAnchor
          QuestTracker
          CombatNoticesAndBuffs
          SkillActionRow
          VitalBars
          QuickAccessBar                # Kho / Bí cảnh / Vận khí / Mốc
          CoreDock                      # 5 tab + collapse control
      WorkspaceCanvas                   # sorting 10
        WorkbenchHost
          SheetHeader
          ActiveWorkbench               # đúng một view active
          SheetActionFooter
      PageCanvas                        # sorting 20
        PageHost                        # Inventory / Progression / Challenge
          PageHeader
          OptionalSectionTabs
          Viewport/Content
          PageActionFooter
      OverlayCanvas                     # sorting 30
        DragHost                        # dưới dimmer/modal khi có interruption
        Dimmer
        ModalStack
        InfoPopoverHost
        ToastAndStatusHost
    UIRegistry                          # view refs/semantic anchors; không là save manager
    AppRouter                           # chủ sở hữu navigation state
    CommandAndProjectionAdapters        # nối controller/service hiện có
```

Các canvas con là boundary theo lifetime/update ownership, không phải một canvas cho mỗi card. Chưa có kết quả profiling; số canvas có thể giảm khi đo cho thấy không cần tách. CanvasScaler chỉ ở root; các nested canvas và safe-area không được scale/inset lần hai.

Battle world/camera nằm ngoài subtree này, tiếp tục render phía sau. Không thu nhỏ frustum hay đổi gameplay camera để giả tạo arena còn nhìn thấy. Page full-screen che presentation nhưng không tự dừng simulation.

**Phần giữ có ích:** alias root Canvas trong giai đoạn bridge; đối tượng/service runtime; topology logic slot và array/index ổn định. Không giữ RightPanel, MainMenuManager child scanning hoặc các path Find cũ làm kiến trúc mới.

## 4. Bố cục 1080×1920

Các số sau là design unit, không phải cam kết dp vật lý.

| Token | Giá trị dự kiến | Quy tắc |
|---|---:|---|
| Lề ngang safe content | 36 | Ngoài safe-area chỉ có background |
| Khoảng cách | 12 / 24 / 36 | Một owner layout cho mỗi trục |
| Hit target tối thiểu | 132×132 | ~44 logical px khi logical width=360 |
| Tác vụ thường xuyên | 144×144 hoặc lớn hơn | ~48 logical px tại width=360 |
| CoreDock | cao176 | 5 cell rộng192, gap12, lề36 |
| Quick access | cao144 | 4 hit region; mặt nền nhỏ hơn hit region |
| Skill row | cao176 | 4 action; giữ mana, fill, lock và cooldown |
| Vital bars | cao96 | Tránh chữ nằm dưới hit region skill |
| Profile/wallet envelope | khoảng232 | Card nổi; không phủ nền đặc toàn chiều ngang |
| Quest compact | tối đa108 | Một mục tiêu; reward/progress; mở rộng khi cần |
| Body/card action footer | tối thiểu144 | Không thu hit target theo label |
| Type | 48 title;36 heading;32 body;28 supporting | Không dùng font nhỏ để nhét số dài |

HUD mặc định dùng widget nổi và khoảng trống nhìn xuyên, không phải các dải nền đặc chiếm hết envelope. Arena render toàn safe view. Mục tiêu hình ảnh là giữ khoảng80% cảnh chiến đấu không bị mặt panel đặc phủ khi farm; đây là tiêu chí chụp/test sau, chưa được đo. Tên dài, quest mở rộng và chế độ combat khẩn cấp có thể tăng diện tích thông tin.

Workbench được tính bằng:
```text
sheetHeight = min(0.55 × safeHeight,
                  safeHeight - topStatus - quest - dock
                             - quickAccess - arenaClearMin - gaps)
arenaClearMin = 320 reference units nếu khả dụng
```

Tại safeHeight1920, top232 + quest108 + dock176 + quick144 + gaps48:
sheetHeight tối đa892, còn clear arena320; header144 + footer144 để body604 cuộn.
Không ép mọi slot/card phải hiện cùng lúc. Khi safe-height nhỏ, quest tự dùng compact, body cuộn và có nút Mở rộng full-page cho cùng workbench/state; không giảm hit target dưới mức tối thiểu. Back từ expanded view quay về compact sheet đúng tab.

Trên720×1280,1080×2340,1080×2400 và logical360×640/390×844: dùng cùng width reference, reflow theo safe rect; không đặt notch inset theo tên điện thoại. Landscape tạm dùng scrollable centered portrait presentation và giữ các nút quay lại/thoát; phase này không đổi orientation setting.

## 5. Main menu

TitleHome có brand/ảnh portrait, summary nhân vật chơi gần nhất và CTA rõ tên/slot. Nếu lastPlayedSlotIndex hợp lệ và slot có dữ liệu, Tiếp Tục mở **xác nhận vào đúng nhân vật**; không bỏ confirmation. Chọn nhân vật luôn mở roster3 slot để chọn bất kỳ nhân vật nào. Khi không có save, Chơi Mới đưa đến roster để người chơi chọn ô trước khi tạo.

Roster giữ cả empty/data panel, đạo hiệu, cảnh giới/tầng, playtime/ngày và Xóa trên từng ô có dữ liệu. Nút Xóa có vùng chạm riêng, không dùng cùng pointer handler với Chọn. Create chỉ trim và chặn tên rỗng như source; không tự thêm giới hạn tên hoặc reset save. Keyboard Back bỏ focus trước, hủy quay lại roster và bỏ draft theo hành vi cũ.

Settings từ title dùng AudioSettingsPanel; session settings dùng cùng panel đó nhưng có thêm lưu/về menu/thoát. Context khác nhau, không dùng chung quit delegate gây lưu nhầm slot. Quyền sở hữu timeScale của MainMenuManager và DataManager phải giữ qua bridge.

## 6. Workbench và thao tác thường xuyên

### Kiếm Lô
- Hai nhóm xuất chiến/kho,10 equip slot theo capacity thật;5 cột, page và inventory scroll giữ riêng.
- Footer: Ghép Nhanh, Sắp Xếp, Trang Trước/Sau và chỉ số trang; Kho là quick access, Phân giải mở Inventory với context phù hợp.
- Giữ drag/drop, merge cùng cấp/pieces, không merge khi thả lên equip, đổi chỗ equip, từ chối khóa/max, trả về chỗ cũ.
- Bổ sung touch affordance chọn → ô đích và long-press info/actions, cùng PC right-click hiện có. Chúng gọi cùng command/handler, không viết thuật toán merge thứ hai.
- Tế Kiếm Lv8 là action riêng trong nhóm thao tác phụ, giữ số lượng/tiêu hủy/thu Kiếm Ý theo MergeManager. Nút sinh kiếm test đang inactive không trở thành nút đúc miễn phí cho người chơi.

### Luyện Thể
Hai nhánh và sáu node có thứ tự/ID cố định. Card hiện level/effect/next/cost và cả realm gate lẫn parent-level requirement. Chọn card mở NodeDetail với lore/mechanics/current/next và x1/x5/max riêng. Grandfathering cấp cũ>0 vẫn do PlayerStatsService quyết định.

### Luyện Đan
Kho3 dược liệu + rank/dust là resource header; hai recipe card riêng; lò là status/timer rõ ràng; Tốc Hành và Uống Tu Vi Đan là action riêng. Giữ thứ tự chất lượng được service chọn khi uống. Đột Phá Đan không bị biến thành nút uống tự do; dùng trong quy trình độ kiếp như cũ.

### Pháp Khí và Trận Pháp
Bốn artifact slot và sáu formation ID không đổi. Artifact có stat/cost/aura/max/gate. Formation giữ Học/Nâng, Chọn và kích hoạt Tụ Linh có timer/cooldown riêng; không gộp chọn trận với nâng cấp. Giá/timer/mốc lấy từ service, không đưa vào skin.

## 7. Inventory và Đạo Lộ

Inventory giữ5 category All/Swords/Pills/Materials/Pending, grid/list và3 preference toggle của UI hiện có. Toggle preference ở band trong page, không bắt người chơi vào settings khác. ClaimPriority, BatchDismantle, action từng kiếm/đan/dược/quặng/hòm tạm đều có binding riêng. Dust không được biến thành vật phẩm có action phá hủy nếu source không có.

Grid action và vật phẩm high-value vẫn đi qua xác nhận; low-value compact-list action giữ semantics hiện hành. Item information/long-press bổ sung cách đọc, không bắt mọi tác vụ đi qua một popup mới. Capacity, equipped/high-value, số phôi, count và reward breakdown vẫn hiện. Gate phân giải500 kiếm từ quái vẫn áp dụng ở UI lẫn lệnh service.

Đạo Lộ dùng page shell và section tabs; khi có7 nhãn, rail ngang cuộn với peek và auto-scroll tab được chọn, không thu nhỏ chữ/chạm.
- Nhân vật: giữ mọi chỉ số/tuvi/wallet/loadout/công pháp. Counter loadout dự kiến lấy capacity thực, khắc phục hiển thị /4; đánh dấu thay đổi presentation.
- Công pháp: bốn upgrade cũ vẫn mua bằng giá và service cũ.
- Thần thông: auto-unlock và route học/nâng bằng CD được trình bày rõ riêng; không tự xóa route legacy vì đã có auto-unlock.
- Đạo Ấn: giữ8 filter,6 tier, từng node ID/cost/max/realm/prerequisite/effect, giữ scroll khi refresh.
- Mốc và Thành tựu: vẫn hai dataset, mỗi loại có claim one/all, category/progress/claimed state riêng.
- Đan hỗ trợ: shop buff local5 item/index, stack/timer/cost theo ShopManager. Không gọi đây là shop IAP hoặc bán bằng tiền thật.

## 8. Bí cảnh, popup và error states

Hub có5 ải và free1/day hoặc5CD. Entry chỉ tiêu đúng một lần bởi TryConsumeEntry trước LaunchMiniGame. TrialArenaPage dùng mode khác nhau, không hợp nhất các luật/target/cooldown của năm mini-game. Result dùng RewardResult đã grant, chỉ đóng/return hub; không grant lại khi mở/đóng view.

Back trong trial đang chạy mở LeaveTrialDecision. Hủy giữ run, timer vẫn theo luật cũ; đồng ý gọi CloseAll, không hoàn lượt. Nếu run kết thúc trong lúc dialog mở, bỏ request rời cũ và hiện result đã cấp. Back tại result tương đương nút xác nhận về hub. Đây là luồng điều hướng bổ sung cho khoảng trống trong inventory.

Độ kiếp giữ gate ready/alive/not-boss, đan phù hợp, tiêu hao trước trận, defer và route thiếu đan về Luyện Đan. Offline giữ x1/x2, availability provider, lỗi/cancel/duplicate callback; Close/Back là nhận1x khi không có ad pending. UI không giả sẵn sàng quảng cáo.

Missing view/binding là RouteError có Thử lại/Quay lại; không gọi command có side effect trước khi có host bắt buộc. Ví dụ phân giải không gọi failsafe auto-confirm khi dialog chưa dựng được. Service/legacy fallback hiện hành không sửa trong phase này; adapter phải đảm bảo host và các thay đổi edge-case được ghi thành acceptance test riêng.

## 9. Tiết kiệm bước, không rút gọn hành vi

Số tap tính từ trạng thái gameplay gốc, bỏ animation, không tính mở khóa hoặc xác nhận giao dịch.

| Mục tiêu | Cũ | Mới dự kiến | Phần phải giữ |
|---|---:|---:|---|
| Kho | Tiện ích → Kho =2 | Kho =1 | 5 nhóm, preference, claim/phân giải |
| Hub bí cảnh | Tiện ích → Bí cảnh =2 | Bí cảnh =1 | free/paid gate và5 loại |
| Vận khí | Tiện ích → Vận khí =2 | Vận khí =1 | 8 filter + mọi node |
| Mốc | Quest/Mốc =1 | Quick Mốc hoặc Quest =1 | claim từng/tất cả |
| Chi tiết nhân vật | Avatar =1 | Avatar =1 | loadout/chỉ số đầy đủ |
| Một workbench | Dock =1 | Dock =1 | switch/toggle/close/swipe |
| Node luyện thể | Dock → node =2 | Dock → node =2 | x1/x5/max riêng |
| Tiếp tục last-slot từ title | Chọn → slot → confirm =3 | Tiếp tục named-slot → confirm =2 | xác nhận enter và chọn slot khác |
| Công pháp/shop/thẻ skill cũ | source có, mobile hidden/unreachable | Avatar → tab =2 | mọi command hiện có |
| Chi tiết vật phẩm | hover/đường phụ | long-press hoặc info | hover/right-click vẫn dùng được |

Đây là path-length theo flow nguồn, không phải kết quả usability test. Filter/section/scroll vị trí trả về được giữ để tránh buộc tìm lại item sau mỗi action.

## 10. Bảo toàn và đưa vào implementation

Mỗi FeatureID có target route, component/role, context selector, command/data gốc, gate/error contract và trạng thái chưa triển khai trong JSON map. Nhiều instance lịch sử/PC có thể dùng cùng view template nhưng vẫn giữ nguồn/ID và selector riêng; đó không phải gộp command hoặc ghi đè scene PC.

Runtime factories phải chuyển sang host được inject trước khi bật shell mới. Old path lookup, auto-builder, menu scan/hide và external Canvas không được chạy song song với owner mới. Chỉ giữ một binding owner cho mỗi action; năm persistent UnityEvent hiện có cần ledger chuyển target và xác nhận một tap gọi đúng một lệnh.

Tích hợp theo lát dọc: shell/read-only projection → menu/save routing → một workbench hoàn chỉnh →4 workbench còn lại → inventory/progression → trial/modal/lifecycle. Giữ legacy fallback cho scene PC và scene chưa migrate. Chỉ gỡ hierarchy cũ sau khi binding ledger và kiểm thử tương đương đạt; lưu scene/prefab qua Editor, không text-replace YAML.

Phase 02 không tạo Scene/Prefab/C# hoặc đánh dấu chức năng mới đã hoạt động. Tiêu chí thiết kế và coverage được xác minh trong screen_redesign_map.json; kiểm thử runtime/thiết bị là cổng bàn giao implementation sau.


## 11. Ánh xạ đầy đủ theo nhóm nguồn

Mỗi hàng dưới là nhóm nguồn, không thay cho ánh xạ từng FeatureID. JSON kèm theo có đủ7.712 record độc lập, giữ callback/data/selector gốc và target component/route; GUID/fileID đích còn null vì chưa triển khai.

| Nhóm cũ | Vị trí mới | Bề mặt | Số record |
|---|---|---|---:|
| MENU — Menu chính | title.home — Màn chính | root-page | 81 |
| SELECT — Chọn nhân vật (3 slot) | title.characters — Chọn nhân vật | page | 190 |
| CREATE — Tạo nhân vật | title.create — Tạo nhân vật | page | 71 |
| MAIN_SETTINGS — Cài đặt từ menu | settings.audio — Âm thanh từ menu | page | 83 |
| CONFIRM_MENU — Xác nhận ở menu | confirm.context — Hộp xác nhận theo ngữ cảnh | modal | 61 |
| HUD — HUD nhân vật và chiến đấu | battle.home — Đấu trường và HUD | root-page | 342 |
| QUEST — Nhiệm vụ hướng dẫn và thanh quest | hud.quest — Quest gọn / nhận mốc | inline | 113 |
| DOCK — Dock 5 tab và drawer shell | battle.home — Đấu trường và HUD | root-page | 97 |
| UTILITY — Menu tiện ích | battle.shortcuts — Lối tắt trực tiếp / chế độ gọn | inline-or-popover | 21 |
| SWORD — Kiếm Lô, trang bị, phân trang | workbench.sword — Kiếm Lô | workbench-sheet | 1252 |
| BODY — Cây luyện thể (6 node) | workbench.body — Luyện Thể | workbench-sheet | 305 |
| BODY_DETAIL — Chi tiết luyện thể | body.node-detail — Chi tiết thiên phú | modal-large | 82 |
| ALCHEMY — Luyện đan, lò và kho đan | workbench.alchemy — Luyện Đan | workbench-sheet | 209 |
| ARTIFACT — Pháp khí (4 slot) | workbench.artifact — Pháp Khí | workbench-sheet | 158 |
| FORMATION — Trận pháp (6 trận) | workbench.formation — Trận Pháp | workbench-sheet | 309 |
| SKILLHUD — HUD 4 kỹ năng | battle.home — Đấu trường và HUD | root-page | 98 |
| STYLE — Chọn/đổi kiếm pháp | battle.style — Kiếm pháp | popover | 14 |
| ENCOUNTER — Kỳ ngộ và thử thách 8 quái | battle.encounter — Lựa chọn kỳ ngộ | modal-choice | 4 |
| CHARACTER — Chi tiết nhân vật | progression.character — Nhân vật | full-page-tab | 218 |
| STORAGE — Kho đồ và hòm tạm | inventory.items — Kho đồ / hòm tạm | full-page | 173 |
| DISMANTLE_CONFIRM — Xác nhận phân giải | confirm.dismantle — Xác nhận phân giải | modal | 46 |
| DAOAN — Đạo Ấn / Vận khí | progression.daoan — Vận khí / Đạo Ấn | full-page-tab | 513 |
| ACHIEVEMENTS — Thành tựu | progression.achievements — Thành tựu | full-page-tab | 139 |
| MILESTONES — Mốc tu luyện | progression.milestones — Mốc tu luyện | full-page-tab | 82 |
| SECRET_HUB — Hub bí cảnh (5 ải) | challenge.hub — Hub bí cảnh | full-page | 135 |
| TRIAL_ROOT — Khung chơi bí cảnh | challenge.play — Khung thí luyện theo activeType | full-page-trial-host | 48 |
| TRIAL_HORDE — Bí cảnh Vạn Ma | challenge.horde — Vạn Ma | full-page-trial | 8 |
| TRIAL_BARRIER — Bí cảnh Cấm Chế | challenge.barrier — Cấm Chế | full-page-trial | 11 |
| TRIAL_CAULDRON — Bí cảnh Tam Đỉnh | challenge.cauldron — Tam Đỉnh | full-page-trial | 11 |
| TRIAL_LIGHTNING — Bí cảnh Lôi Trì | challenge.lightning — Lôi Trì | full-page-trial | 6 |
| TRIAL_SWORD — Bí cảnh Kiếm Chủng | challenge.sword-tomb — Kiếm Chủng | full-page-trial | 6 |
| SECRET_RESULT — Chiến báo bí cảnh | challenge.result — Chiến báo bí cảnh | modal-result | 26 |
| TRIBULATION — Xác nhận độ kiếp | modal.tribulation — Chuẩn bị độ kiếp | modal-large | 81 |
| OFFLINE — Thưởng bế quan | modal.offline — Thưởng bế quan | modal-reward | 23 |
| PAUSE — Cài đặt trong game | settings.session — Cài đặt và lưu game | modal-large | 312 |
| CONFIRM_PAUSE — Xác nhận về menu / thoát | confirm.context — Hộp xác nhận theo ngữ cảnh | modal | 73 |
| LEGACY_UPGRADES — Công pháp nâng cấp cũ | progression.disciplines — Công pháp | full-page-tab | 1772 |
| LEGACY_SKILL — Thẻ học/nâng kỹ năng cũ | progression.skills — Thần thông | full-page-tab | 206 |
| LEGACY_SHOP — Cửa hàng cũ | progression.buff-shop — Đan hỗ trợ | full-page-tab | 223 |
| LEGACY_ONLINE — ViewModel online / auth minh họa | diagnostics.online-preview — Dữ liệu online minh họa | development-reference | 1 |
| TOOLTIP — Tooltip kiếm/kỹ năng | hud.tooltip — Thông tin chạm/hover | popover | 2 |
| COMBAT_NOTIFICATION — Thông báo chiến đấu / buff / log | hud.combat-log — Thông báo / nhật ký | inline-expandable | 65 |
| WORLD_DAMAGE — Chữ sát thương trong thế giới | battle.world-damage — Chữ sát thương | world-presentation | 3 |
| LEGACY_TUTORIAL — Tutorial overlay cũ | hud.quest-guide — Hướng dẫn và chỉ mục tiêu | overlay-passive | 11 |
| LEGACY_SPLASH — Splash/loading trong recovery | diagnostics.recovery-splash — Splash recovery | development-reference | 9 |
| LEGACY_CHEAT — Cheat panel trong recovery | diagnostics.recovery-cheats — Cheat recovery | development-reference | 7 |
| SAVE_LIFECYCLE — Luồng save/load và lifecycle | system.save — Save/lifecycle | nonvisual | 8 |
| WORLD_COMBAT_UI — HP địch/Boss và overlay chiến đấu | battle.world-overlays — HP địch/boss và vignette | world-presentation | 4 |

### Danh mục route đề xuất

| Route ID | View/component | Surface | Return/Back |
|---|---|---|---|
| title.home | TitleHomePage | root-page | confirm.quit-title |
| title.characters | CharacterRosterPage | page | title.home |
| title.create | CharacterCreatePage | page | keyboard-first-then:title.characters |
| settings.audio | AudioSettingsPage | page | caller |
| confirm.enter-character | DecisionDialog | modal | cancel-to-caller |
| confirm.delete-character | DecisionDialog | modal | cancel-to-caller |
| confirm.quit-title | DecisionDialog | modal | cancel-to-caller |
| battle.home | BattleShell | root-page | settings.session |
| battle.shortcuts | QuickAccessBar | inline-or-popover | collapse-if-popover |
| workbench.sword | SwordWorkbench | workbench-sheet | close-sheet-to-battle |
| workbench.body | BodyWorkbench | workbench-sheet | close-sheet-to-battle |
| workbench.alchemy | AlchemyWorkbench | workbench-sheet | close-sheet-to-battle |
| workbench.artifact | ArtifactWorkbench | workbench-sheet | close-sheet-to-battle |
| workbench.formation | FormationWorkbench | workbench-sheet | close-sheet-to-battle |
| body.node-detail | BodyNodeDialog | modal-large | restore-workbench.body |
| inventory.items | InventoryPage | full-page | restore-caller-state |
| inventory.item-info | ItemActionPopover | popover | dismiss-to-item |
| inventory.sacrifice-lv8 | SacrificeSwordPanel | action-sheet | restore-workbench.sword |
| confirm.dismantle | DismantleDecisionDialog | modal | cancel-without-command |
| progression.character | CharacterProfilePage | full-page-tab | restore-caller-state |
| progression.disciplines | DisciplinePage | full-page-tab | restore-caller-state |
| progression.skills | SkillStudyPage | full-page-tab | restore-caller-state |
| progression.daoan | DaoAnPage | full-page-tab | restore-caller-state |
| progression.milestones | MilestonePage | full-page-tab | restore-caller-state |
| progression.achievements | AchievementPage | full-page-tab | restore-caller-state |
| progression.buff-shop | LocalBuffShopPage | full-page-tab | restore-caller-state |
| challenge.hub | SecretRealmHubPage | full-page | restore-caller-state |
| challenge.horde | TrialArenaPage | full-page-trial | confirm.leave-trial |
| challenge.barrier | TrialArenaPage | full-page-trial | confirm.leave-trial |
| challenge.cauldron | TrialArenaPage | full-page-trial | confirm.leave-trial |
| challenge.lightning | TrialArenaPage | full-page-trial | confirm.leave-trial |
| challenge.sword-tomb | TrialArenaPage | full-page-trial | confirm.leave-trial |
| challenge.result | TrialResultDialog | modal-result | close-all-to:challenge.hub |
| confirm.leave-trial | DecisionDialog | modal | cancel-to-live-trial |
| battle.style | SwordStylePopover | popover | close-without-style-change |
| battle.encounter | EncounterChoiceDialog | modal-choice | existing-skip-command |
| modal.tribulation | TribulationDialog | modal-large | defer-via-existing-command |
| modal.offline | OfflineRewardDialog | modal-reward | claim-one-unless-ad-pending |
| settings.session | SessionSettingsDialog | modal-large | resume-with-previous-time-policy |
| confirm.return-title | DecisionDialog | modal | cancel-to-settings |
| confirm.quit-session | DecisionDialog | modal | cancel-to-settings |
| hud.quest | QuestTracker | inline | collapse-quest |
| hud.quest-guide | TutorialGuideOverlay | overlay-passive | existing-tutorial-policy |
| hud.tooltip | InfoPopover | popover | dismiss |
| hud.combat-log | CombatNoticeFeed | inline-expandable | collapse-feed |
| battle.world-damage | ExistingDamagePopup | world-presentation | automatic-existing-lifetime |
| battle.world-overlays | ExistingCombatOverlays | world-presentation | automatic-existing-lifetime |
| system.save | ExistingSaveBoundary | nonvisual | not-navigable |
| system.input | ExistingInputBoundary | nonvisual | not-navigable |
| overlay.route-error | RouteErrorPanel | modal-error | return-to-known-safe-caller |
| diagnostics.hub | DiagnosticsIndex | development-page | caller |
| diagnostics.online-preview | OnlineFixturePreview | development-reference | diagnostics.hub |
| diagnostics.recovery-splash | RecoverySplashReference | development-reference | diagnostics.hub |
| diagnostics.recovery-cheats | RecoveryCheatReference | development-reference | diagnostics.hub |
| diagnostics.sword-spawn | SwordSpawnReference | development-reference | diagnostics.hub |
| challenge.play | TrialArenaPage | full-page-trial-host | confirm.leave-trial |
| confirm.context | DecisionDialog | modal | cancel-to-caller |

## Xác minh bàn giao Phase 02

- Đủ7.712/7.712 FeatureID,0 trùng/thiếu; callback/data/row hash gốc khớp;48 nhóm nguồn có mapping.
- 57 route và component đích hợp lệ;195 cạnh được kiểm tra, mapped route reachable theo production/development/lifecycle root phù hợp.
- Bốn tài liệu liên kết đúng, Markdown fence cân bằng; GUID/fileID đích còn null có chủ đích vì chưa tạo view.
- Mọi file có trước Phase02 giữ nguyên SHA-256; Assets/Packages/ProjectSettings, AGENTS, tài liệu Phase00/01, HEAD/branch/index không đổi.
- Scene mobile vẫn sạch,12 root; hash save hiện tại giữ nguyên so với đầu Phase02. Không so với save cũ của Phase01 vì người dùng có thể đã chơi giữa hai phase.
- Đây là xác minh coverage/thiết kế và bảo toàn nội dung; chưa triển khai hoặc chứng nhận runtime/thiết bị.

# BÀN GIAO ANTIGRAVITY — TIẾP TỤC PHASE 3

Ngày: 17/11/2026. Project: `C:\Lap trinh game\Save file\KhiVanChiTu`. Unity 6000.6.4f1. Android portrait. Người dùng yêu cầu dừng Codex để bàn giao vì sắp hết hạn mức.

**Trạng thái: Phase 3 đang làm dở. Implementation đã được áp dụng vào scene mobile, nhưng CHƯA chạy Play Mode QA navigation, CHƯA có screenshot Phase 3, CHƯA build Android và CHƯA commit. Không báo hoàn thành Phase 3 trước khi kiểm tra.**

## 1. Công việc được giao

Chỉ hoàn thiện Phase 3 — Global Navigation Redesign. Không làm lại Phase 0–2, không tự sang Phase 4.

5 nhóm chính:

| Nhóm | Chức năng |
|---|---|
| Chiến Đấu | Gameplay, HUD Full/Compact của Phase 2 |
| Trang Bị | Kiếm Lộ, Kho Đồ; giữ merge, equip, unlock ô, lọc, phân trang, phân giải và item action bên trong hệ thống cũ |
| Tu Luyện | Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp; thêm lối truy cập Công Pháp & Kỹ Năng đã tồn tại và Độ Kiếp theo gate cũ |
| Bí Cảnh | Danh sách và 5 minigame; giữ phí, lượt miễn phí, gameplay, timer và thưởng |
| Khác | Thiên Phú/Vận Khí, Mốc, Thành Tựu, Nhân Vật, Tài Nguyên, Hiệu Ứng, Nhật Ký, Settings/Pause, hỗ trợ tu luyện, Kiếm Pháp |

Yêu cầu còn phải kiểm chứng: Screen Open/Close, Back/ESC/Android Back, modal stack, FullScreenPanel, BottomSheet; không click xuyên, không hai fullscreen cùng hiển thị/nhận input sai, không mất đường về gameplay, Safe Area đúng. Bảo toàn gameplay, save/load, công thức, drop, tài nguyên và các nút gốc.

## 2. Đọc các tài liệu này trước

- `AGENTS.md` tại project root.
- `Docs/UI_Redesign/project_audit.md`, `project_baseline.json`, `rollback_plan.md`.
- `Assets/.docs/KhiVanChiTu_UIUX_Redesign_Plan.md` và storyboard đã có trong project/cuộc hội thoại.
- `Assets/.docs/UIUX_Phase0_20261117/` — audit Phase 0, không chạy lại.
- `Assets/.docs/UIUX_Phase1_20261117/DESIGN_SYSTEM.md`, `COMPONENT_GUIDE.md`.
- `Assets/.docs/UIUX_Phase2_20261117/PHASE2_REPORT.md`, `HUD_BINDINGS.md`.
- `Assets/.docs/GDD.md`, `TDD.md` để hiểu project; không retune gameplay.

Không thao tác scene PC. Nếu cần đọc/sửa PC-module, đọc `Assets/Modules/PCContent/AGENTS.md` trước.

## 3. Pipeline và môi trường

Người dùng đã xác minh trong Windows PowerShell ngoài Codex sandbox:

- TCP `127.0.0.1:7800` kết nối được.
- `unity command editor status` SUCCESS, đúng project, ready, compiling=false.

Trong Codex, lệnh đó vẫn trả `os error 10013`, socket forbidden. Chính sách phiên Codex: workspace-write, network restricted, sandbox_approval=false. Đây là giới hạn sandbox, **không phải Pipeline hoặc Unity bị hỏng**. Không tắt sandbox/Firewall, không yêu cầu đóng Unity. Antigravity nên dùng môi trường terminal của mình để kết nối Pipeline trực tiếp.

Chạy mọi Unity CLI từ project root:

```powershell
Set-Location 'C:\Lap trinh game\Save file\KhiVanChiTu'
unity status --format json
unity command editor status
unity command scene list_open
unity command console status
unity command
```

Sau đó dùng command surface thực tế để đọc hierarchy, console và sửa qua Editor API/eval/menu. CLI hiện có là 1.0.0; `unity pipeline list` không tồn tại ở bản này (gợi ý là `unity pipeline editors`), đừng lặp lệnh từ tài liệu phiên bản cũ.

Unity đang mở scene `Assets/Scenes/SampleScene_MobileWork.unity`, không mở Editor thứ hai. Codex chỉ dùng Computer Use để gọi menu Editor tự động hóa và xem trạng thái; không lặp Inspector. Không sửa YAML `.unity`/`.prefab` thủ công.

## 4. Những gì đã triển khai

### Files mới

| File | Vai trò |
|---|---|
| `Assets/UI/Navigation/Runtime/GlobalNavigationController.cs` | Route catalog 24 đích, 5 group, điều phối mở/đóng, Back, registry screen/sheet/modal, khóa input và ẩn HUD |
| `Assets/UI/Navigation/Runtime/NavigationIcon.cs` | 5 icon vector cùng stroke, không phụ thuộc glyph font |
| `Assets/UI/Navigation/Editor/GlobalNavigationBuilder.cs` | Kiểm kê scene, tạo nav mới, di chuyển card chức năng legacy bằng Editor API |
| `Assets/UI/Navigation/**/*.meta` | GUID do Unity tạo |
| `Assets/.docs/UIUX_Phase3_20261117/` | Checkpoint, inventory và bàn giao |

### Files có sẵn đã sửa

| File | Thay đổi |
|---|---|
| `Assets/Scripts/UI/MobileUINavigationManager.cs` | Đầu HandleBackNavigation chuyển cho GlobalNavigationController nếu cùng scene và đang trong session; luồng Main Menu cũ vẫn dùng khi không trong game |
| `Assets/UI/CombatHUD/Runtime/CombatHudPresentation.cs` | Thêm externalNavigation; nhường vị trí dock cho nav mới; tăng khoảng đáy 32 units khi Full để có footer 144 units; không thay binding/tính toán HUD |
| `Assets/Scripts/UI/JadeUITheme.cs` | Skip subtree của GlobalNavigationController để không ghi đè style mới |
| `Assets/UI/Reconstruction/Runtime/MobileUIReconstruction.cs` | Guard tương tự cho style mới |
| `Assets/Scenes/SampleScene_MobileWork.unity` | Đã chạy Build qua Editor: thêm navigation, giữ Button gốc, chuyển các card legacy vào mobile sheets |

`ProjectSettings/TimeManager.asset` không thay đổi thêm trong Phase 3 so với checkpoint. Ngoại lệ serialization của Phase 2 vẫn là trạng thái kế thừa, không reset nó về HEAD.

### Scene và kiến trúc hiện tại

- Root scene Canvas ScreenSpaceOverlay, sorting=0.
- `Canvas/MobileUIRoot`: SafeAreaFitter, MobileDrawerUIController, MobileUINavigationManager và các component UI cũ.
- HUD Phase 2 ở `Canvas/MobileUIRoot/PortraitHudShell`.
- Component GlobalNavigationController mới nằm trên `Canvas/MobileUIRoot/GlobalNavigationShell`.
- FullScreenPanel mới có header/back/breadcrumb, ScrollRect secondary cards; footer 5 nhóm có icon và nhãn.
- Footer sorting 120, hub sorting 100. Blocker đầy màn hình là `Canvas/GlobalNavigationBlockerModal`, chỉ hiện khi có layer, được đặt ngay dưới layer trên cùng.
- Overlay cũ giữ nguyên parent/path để các factory Canvas.Find không hỏng. Registry gắn nested Canvas/GraphicRaycaster/CanvasGroup trong runtime để quản lý order/input; không sửa manager gameplay.
- Hub tắt khi legacy fullscreen đang mở; drawer/sheet/modal khóa các lớp phía sau. Stack dùng thứ tự activation và close callback của controller gốc.
- Nút Menu trên HUD được thêm listener mở nhóm Khác; listener gốc không xóa. Nút Gọn/Đủ của Phase 2 vẫn giữ hành vi compact, footer mới ẩn khi compact và không ở hub.

### Các chức năng legacy đã đưa ra mobile

Trong scene có 4 upgrade card và 4 SkillCardUI ở `Canvas/RightPanel/UpgradeScrollView/UpgradePanel` nhưng nhánh RightPanel bị ẩn trên mobile. Đã **di chuyển chính các card này** vào ProgressionBottomSheet, không tạo transaction upgrade mới. UpgradeUIPlaceholder trên UIManager vẫn giữ serialized references; SkillCardUI giữ callback/data gốc.

5 ShopItemUI cũ đã di chuyển từ `Canvas/ShopPanel/ScrollView/Content` vào SuppliesBottomSheet. ShopToggleUI.shopPanel được redirect tới sheet mới để nút shop cũ còn dùng được. BuyButton, itemIndex và ShopManager không sửa.

Các card được bố trí lại cho chiều dọc, chữ/nút không scale desktop xuống. **Chưa xem Game View các sheet nên phải kiểm tra overflow, chiều cao và listener thực sự.**

Bí Cảnh: card group chỉ mở danh sách cũ và cuộn đến loại đã chọn. **Không gọi LaunchMiniGame để vượt TryConsumeEntry**. Nút BtnEnter cũ vẫn kiểm tra/tiêu lượt và phí trước khi launch.

Độ Kiếp gọi TribulationModalUI.Open, gate gốc vẫn quyết định. Kiếm Pháp gọi Button runtime cũ của GameplayChoicesService khi interactable.

## 5. Những việc đã kiểm tra và chưa kiểm tra

Đã kiểm tra:

- Read-only inventory từ Editor: `Evidence/editor-before.json`, scene clean trước Build, đúng scene, một root Canvas.
- Runtime và Editor C# đã compile bằng Roslyn đi kèm Unity với exit 0 **trước bước Build và các chỉnh nhỏ cuối**; không coi đây là compile final.
- Unity đã import script, xuất hiện menu và thực thi Build; file scene đã được lưu (khoảng 7,16 MB). Build kiểm tra giữ toàn bộ button IDs có trước khi thêm nav.
- Hash 349 file bảo vệ: chỉ 4 source UI ở trên và scene mobile thay đổi tính tới bàn giao. `Evidence/handoff-source-status.json` ghi danh sách.

CHƯA kiểm tra:

- Chưa vào Play Mode cho navigation Phase 3.
- Chưa có runtime-checks, test runner hoặc screenshot Phase 3.
- Chưa xác minh UI hierarchy runtime sau factory/Start/OnEnable.
- Chưa kiểm tra ESC/Android Back, pointer raycast, modal nesting, rapid tap, source buttons đã di chuyển.
- Chưa build/cài Android, chưa profiling.
- Chưa commit. Git trong Codex trả lỗi không truy cập project root; Antigravity cần đọc git status/index bằng môi trường của mình.

### Chỉnh nhỏ cuối chưa áp lại scene

Sau Build, source `UtilitySheet()` đã đổi `SheetTitle` font 42 → 40 để tránh dòng chữ bị sát chiều cao. Scene được build trước thay đổi này nên có thể vẫn serialize font 42. Sửa riêng SheetTitle của hai sheet bằng Editor API rồi lưu; **không chạy lại Build để tạo trùng hierarchy**.

## 6. Ưu tiên kiểm tra/sửa tiếp

1. Kết nối Pipeline, đọc console và scene dirty state. Nếu có thay đổi người dùng sau checkpoint, giữ nguyên. Lưu scene bằng Editor sau khi xác minh đúng scene và biết nguồn dirty.
2. Đọc implementation mới và compile final bằng `unity recompile` khi Pipeline ready. Các menu nằm ở `TuTien/UI Global Navigation`.
3. **Không chạy lại `1. Build Mobile Navigation`**: nó chủ động từ chối nếu nav đã tồn tại. Không chạy lại `0. Inspect Current Scene` để ghi đè `editor-before.json`; xuất inventory sau sửa sang `editor-after.json` bằng eval riêng.
4. Tạo Editor validation runner riêng dưới `Assets/UI/Navigation/Editor/`. Có thể tái dùng cách cô lập save/chụp Game View trong `UI/CombatHUD/Editor/CombatHudValidation.cs` làm mẫu; **không chạy lại Phase 2 hoặc sửa kết quả Phase 2**.
5. Dùng WorkspaceSaveProfile mới, ví dụ `navigation-phase3-qa`, marker chỉ tồn tại tạm, trước khi DataManager load. Runner phải từ chối profile không khớp. Không lấy save production làm fixture.
6. Kiểm tra tất cả 24 route: group → route → close/Back → group → Chiến Đấu. Các route đang khóa phải hiện đúng gate, không ép unlock để sửa navigation.
7. Lưu ý gate Độ Kiếp hiện đọc Button trên `hud.legacy.realm`; kiểm tra source CultivationPhaseBarUI tạo Button đúng root đó và cập nhật readiness. Nếu không đúng, đọc gate hiện có thay vì tự tạo điều kiện gameplay.
8. Kiểm tra pause confirmation, storage → dismantle confirm, body → node detail, Bí Cảnh list → từng minigame → result/exit → list. Back phải chỉ đóng lớp trên cùng, không đồng thời đóng cha hoặc rơi về menu.
9. Test ESC thật qua Input System, một lần press chỉ một Back; Main Menu/character selection/confirm vẫn đi theo router cũ. Android Back phải được xác minh thiết bị nếu có, nếu chỉ Editor thì ghi giới hạn.
10. Test raycast: dưới modal không nhận click, footer/HUD bị khóa; source input của minigame vẫn nhận ở lớp trên cùng. Giữ pause/timeScale semantics cũ, navigation không đặt Time.timeScale.
11. Xác minh chỉ một fullscreen hiển thị/nhận input. Registry có thể giữ màn cha active nhưng alpha=0 khi lồng màn con để bảo toàn state; kiểm tra đây là stack có chủ đích, không phải hai màn hiển thị ngoài ý muốn. Kiểm tra modal khi đang animate đóng/mở và khi nhấn nhanh.
12. Kiểm tra card Công Pháp & Kỹ Năng và 5 ShopItem: source Start/OnEnable có wire listener; text/nút nằm trong bounds; transaction chỉ một lần. Có thể mua/nâng một lần **trong QA profile** để so before/after theo service cũ.
13. Chụp và xem Game View tại 1080×1920, 1080×2340, 720×1280: battle/full/compact, 4 group hubs, legacy routes, utility sheets, modal stack. Test Safe Area/insets; không chỉ dựa trên code.
14. Trước/sau QA so hash production PlayerPrefs `KhiVanChiTu_SaveData`, persistent JSON và `.bak` như Phase 2; cleanup marker, restore preset Game View/timeScale trước test, không xóa save người chơi.
15. So source/hash/GUID/Button events với checkpoint Phase 3, không phải HEAD hoặc snapshot Phase 0. Ghi kết quả trung thực, sửa lỗi xong compile/test lại phần liên quan.
16. Báo cáo file, chức năng, test/screenshot, lỗi còn lại, compile/build. Tạo commit riêng **chỉ Phase 3** sau kiểm tra; không gom thay đổi Phase 0–2 hoặc dirty không liên quan. Dừng trước Phase 4.

## 7. Checkpoint và bảo toàn Git

- `Backups/before-phase3.zip`: scene mobile + 4 source UI liên quan + meta, trước sửa. Đã mở rộng archive trước khi sửa hai style guards.
- SHA-256 archive hiện tại: **5052d48583e5405ccf8b00b9efa6fe09561c87ce2b485aedcd22a2edf273be84**.
- `before_manifest.json`: hash từng file trong archive.
- `protected_before.json`: hash 349 file (scripts/scenes/prefabs/config/data), baseline sau Phase 2.
- `Evidence/editor-before.json`: inventory trước Build.
- `Evidence/handoff-source-status.json`: các file cũ thay đổi và cờ chưa QA/build/commit.

Không reset, clean, stash, broad restore, đổi GUID hoặc ghi đè thay đổi người dùng. ZIP là checkpoint chọn lọc, không tự giải nén đè vào project. Nếu cần rollback, kiểm SHA-256 rồi dùng Editor/AssetDatabase và đối chiếu thay đổi mới trước khi khôi phục. Không upload save hoặc backup cá nhân.

## 8. Quy tắc kết thúc

Chỉ tuyên bố các kiểm tra đã thực sự chạy. Không viết “navigation đã pass” dựa vào compile hoặc cấu trúc code. Nếu bị chặn Pipeline/Editor/device/build/Git, ghi đúng phần chưa xác minh và bằng chứng lỗi. Chỉ tiếp tục và kết thúc Phase 3; không đổi gameplay hoặc tự chuyển phase.

## 9. Trạng thái tại lúc dừng

Đã lưu scene qua Editor sau khi nhận yêu cầu bàn giao. Đã đọc lại title Unity: `SampleScene_MobileWork`, không còn dấu `*`, đang Edit Mode. Unity vẫn mở; không có QA Play Mode Phase 3 được chạy trước khi dừng. Chỉ chỉnh font SheetTitle trong source builder chưa áp lại hierarchy như lưu ý ở trên.

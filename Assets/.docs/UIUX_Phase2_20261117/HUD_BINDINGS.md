# HUD Phase 2 — binding và navigation

Presentation được gắn trên PortraitHudController trong `SampleScene_MobileWork`. Các manager, controller, serialized reference và Button gốc vẫn là nguồn quyền quyết định. `externalLayout` chỉ nhường quyền layout/skin.

## Dữ liệu chỉ đọc

| View | Nguồn | Quy tắc trình bày |
|---|---|---|
| P2Character | CultivationManager.CharacterName | TMP heading có tiếng Việt; một dòng, ellipsis |
| P2Realm | CultivationManager.GetDisplayRealmName() | Caption một dòng |
| Cultivation progress | currentTuVi / GetCurrentStageData().requiredTuVi | FoundationProgress, không ghi tu vi |
| P2Wallet | EconomyManager.currentLinhThach / currentCoDuyen | PortraitHudController.Compact, LT/CD |
| P2Objective | Quest gốc / TxtQuestTitle.text | Bỏ markup hiển thị, giữ nội dung động |
| Mốc/Nhận | BtnClaimQuick caption và interactable | Nhãn mới phản ánh hành động nguồn |
| HP | CombatManager.playerCurrentHp / playerMaxHp | Fill normalized; dưới 25% đổi màu |
| MP | SkillManager.currentLinhLuc / maxLinhLuc | Fill normalized; số nguyên để dễ đọc |
| Wave | StageManager.currentStage, enemiesKilled, enemiesToBoss | Tầng + số lượng + progress |
| Boss visibility | StageManager.isBossStage hoặc CombatManager.isTribulationBossActive | Chỉ hiện theo cờ nguồn |
| Boss HP | CombatManager.CurrentBoss.hp / maxHp | Fill và phần trăm trên tiêu đề |
| Battle timer | StageManager.bossTimer khi isBossStage | Ceil giây; không tự chạy countdown ở UI |
| Skill state | SkillData.isUnlocked, GetActive/NormalSkillCooldown, GetSkillManaCost, isSkillActive, Button.interactable | Khóa / cooldown / thiếu MP / xuất trận / chi phí / chờ |
| Full/Compact | MobileDrawerUIController.IsDockCollapsed | Compact ẩn quest/dock, giữ combat controls |
| Kiếm pháp | Text status của Button kiếm pháp runtime gốc | Thay font/skin, không đổi listener/gate/choice |

## Sự kiện và đường đi

| Nút/vùng | Đường xử lý được giữ |
|---|---|
| Dừng | PauseButton gốc → PauseMenuManager |
| Menu | JadeMenuButton gốc → PortraitHudController.ToggleSecondary |
| Đóng/backdrop menu | CloseMenu/MenuDimmer gốc → CloseSecondary |
| Gọn/Đủ | BottomDockToggle gốc → MobileDrawerUIController.ToggleDockCollapse |
| Nội dung nhiệm vụ | P2MainObjective persistent callback → CombatHudPresentation.OpenObjectives → milestoneButton.onClick.Invoke → TutorialManager.OpenMilestoneModal |
| Mốc/Nhận | P2ClaimObjective FoundationAction.clicked → ClaimObjective → BtnClaimQuick.onClick.Invoke → TutorialManager.OnQuestBarClicked |
| Nhân vật | ProfileButton gốc → PortraitHudController.OpenProfile → CharacterDetailModalUI |
| Tài nguyên | Wallet button gốc → PortraitHudController.ShowWallet / trang chi tiết cũ |
| Thành tựu | BtnHUDAchievement gốc + handler/subscription cũ |
| Hiệu ứng | BuffsButton gốc → ShowBuffs |
| Nhật ký | LogButton gốc → ShowLog |
| Kho | BtnHUDStorage gốc → StorageButtonHandler → StorageModalUI |
| Bí Cảnh | BtnHUDBiCanh gốc → BiCanhButtonHandler → SecretRealmModalUI |
| Vận Khí | BtnHUDDaoAn gốc → DaoAnButtonHandler → DaoAnModalUI |
| Mốc | MilestoneButton gốc → TutorialManager.OpenMilestoneModal |
| Kiếm Lộ / Luyện Thể / Luyện Đan / Pháp Khí / Trận Pháp | 5 dock Buttons gốc → MobileDrawerUIController.ToggleDrawer(index 0–4) |
| Vạn kiếm | Btn_Ultimate gốc → MobileSkillHUD listener → SkillManager.CastActiveSkill |
| Ấn chưởng / Lôi phạt / Âm dương | Btn_Skill_0/1/2 gốc → MobileSkillHUD listener → CastNormalSkillManual(index 0–2) |
| Kiếm pháp | Button runtime của GameplayChoicesService, giữ command OpenStyles và trạng thái source |

Hai callback mới chỉ chuyển tiếp nút đã có. Không thêm listener cast kỹ năng thứ hai, không gọi transaction economy/save mới từ presentation.

## Biên giới bảo toàn

- `CombatHudPresentation.Refresh` đọc dữ liệu và cập nhật view; không gán HP/MP, cooldown, tài nguyên, nhiệm vụ hoặc timer gameplay.
- Fixture gán dữ liệu nằm trong thư mục Editor và yêu cầu profile `hud-phase2-qa`. Các gán reflection cooldown không có trong player runtime assembly.
- `DamagePopup` chỉ có hai hook Begin/Style. AddDamage, tổng sát thương, cửa sổ accumulate 0,42s, lifetime 0,95s, movement và pooling cũ không đổi. UnityEngine.Random không được gọi trong helper mới.
- `SetupCustom` status không bị đổi nội dung: helper nhận biết callout không phải damage và trả renderer cũ về hiển thị.
- Old CanvasGroup ẩn source graphics của realm/quest/stage/vitals; không disable các service để ẩn UI. Điều này giữ binding nhưng vẫn cần profiling chi phí UI legacy.
- Ngăn skin writer cũ bằng guard theo CombatHudPresentation; scene khác không bật externalLayout tiếp tục luồng cũ. Đây là cô lập source, chưa phải kiểm thử runtime các scene PC.

## Kiểm tra có thể lặp lại

`TuTien/UI Combat HUD/2. Validate with Isolated Profile`: 6 trạng thái chụp × 3 kích thước, live timer × 3, callback skill, navigation, 4 shortcut, 5 drawer, Pause/Resume. Kết quả cuối: `Evidence/runtime-checks.json`, `qa-console.json`, `save-preservation.json`.

`python Assets/.docs/UIUX_Phase2_20261117/verify_phase2.py`: kiểm tra hash source, GUID, serialized IDs và persistent Button events so với backup. Script chỉ ghi báo cáo, không sửa Unity assets/save. Hiện exit 1 vì TimeManager tự nâng định dạng; không phải mất Button hoặc thay công thức. Không bỏ qua kết quả này khi review commit.

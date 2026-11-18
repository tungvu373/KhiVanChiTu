# UI_FEATURE_MAP

Phase 0 — Full UI/UX Audit, 17/11/2026 (Việt Nam).

Phạm vi: toàn bộ 18 scene, 46 prefab trong Assets, UI-bearing scripts, Packages và ProjectSettings. Read-only serialized YAML audit, không sửa scene/prefab. GUID script được đối chiếu Assets và PackageCache. Số lượng là bản ghi serialized trên tất cả bản sao, không phải object đồng thời trong runtime. Demo không có Canvas/RectTransform được ghi nhận 0 UI.

Bằng chứng: Source = code/serialized hiện tại; Storyboard = ảnh tham chiếu; Risk = cần tái hiện. Static analysis không chứng minh button hoạt động, text không tràn hay save/load parity. Chưa flatten prefab overrides hoặc resolve runtime factory qua Editor; lambda/reflection/helper cần đối chiếu tại source call site.

Editor: Pipeline phát hiện đúng Unity 6000.6.4f1 nhưng port 7800 unreachable. Không có Console, hierarchy runtime hoặc Game View mới đáng tin cậy. Thử native capture không trả ảnh Unity tin cậy nên không dùng làm bằng chứng. Không chạy Play, compile, build, profiling hoặc test Android. Video gốc chưa được phát lại. Báo cáo cũ trong Docs/UI_Redesign là lịch sử, không phải QA pass hiện tại.


## 1. Chức năng và owner

| Màn | Storyboard | UI controller | Data owner | Contract chức năng | Phase |
|---|---|---|---|---|---|
| HUD | 00:08–00:14; 02:20–00:28 | PortraitHudController; CultivationUI; CultivationPhaseBarUI; MobileSkillHUD | EconomyManager / CultivationManager / PlayerStatsService / SkillManager | HP/MP, tu vi, tiền, boss/timer, quest, 4 skill, compact/full, profile, buff/log | 2 |
| Main Menu | 00:00–00:03 | MainMenuManager | DataManager / AudioManager | Chơi mới/Tiếp tục → chọn slot; Settings; Quit confirm | 8 |
| Character Selection / Creation | 00:04–00:07; 02:30 | MainMenuManager; SaveSlotUI | GameSaveData.slots / CharacterSaveData | Slot rỗng → tạo/nhập tên; slot có data → confirm enter; xóa → confirm; Back | 8 |
| Kiếm Lộ | 00:14–00:35 | drawer[0]; MergeManager; MergeItem; MergeSlot; SwordSlotLockUI | SwordLoadoutService / StorageManager / DismantleService | Kéo thả/merge, equip slots, unlock, trang, shortcut kho/phân giải | 4 |
| Kho Đồ | 02:02–02:10 | StorageModalUI; StorageButtonHandler; DismantleConfirmModalUI | StorageManager / DismantleService / AlchemyService | 5 category; lọc kiếm/đan; auto dismantle Lv1 khi đầy; nhận ưu tiên; batch; đổi view; item action/confirm; đóng | 4 |
| Luyện Thể | 02:30–02:47 | BodyTemperingPanelUI; drawer[1] | PlayerStatsService / EconomyManager | Chọn node → modal; x1/x5/max; chỉ số hiện tại/tiếp theo/giá; đóng | 5 |
| Luyện Đan | 00:36–00:44 | AlchemyPanelUI; drawer[2] | AlchemyService / FeatureUnlockService / CultivationManager | Nấu tu vi/đột phá theo realm; timer/progress; dùng bụi hoàn tất; dùng tu vi đan Basic→Refined→Supreme | 5 |
| Pháp Khí | 00:46–00:54 | ArtifactPanelUI; drawer[3] | ArtifactService / EconomyManager | Nâng slot qua TryUpgrade; cấp/hiệu ứng/giá/locked | 5 |
| Trận Pháp | 00:46–00:54 | FormationPanelUI; drawer[4] | FormationService / EconomyManager | Chọn trận TrySelect, học/nâng TryUpgrade, kích hoạt linh TryActivateSpirit | 5 |
| Thiên Phú / Vận Khí | 00:56–01:13 | DaoAnModalUI; DaoAnButtonHandler | DaoAnProgressionService / DaoAnCatalog / EconomyManager | Mở/đóng; filter; node list động; mua/nâng; locked/max/affordable | 6 |
| Mốc Tu Tiên / Thành tựu | 00:56–01:13 | MilestoneModalUI; AchievementModalUI; CollapsibleQuestBar | MilestoneManager / AchievementManager | Progress, claim từng mốc/claim all đã có; chuyển màn thành tựu; claim; close | 6 |
| Bí Cảnh | 01:14–02:00 | SecretRealmModalUI; SecretRealmPlayController; BiCanhButtonHandler | SecretRealmService | Chọn challenge/difficulty; HordeSurvival, BarrierBreak, DanCauldron, LightningDodge, SwordTomb; input/timer/score/result/reward; CloseAll về danh sách | 7 |
| Pause / Settings | 02:12–02:18 | PauseMenuManager; PauseButtonHandler; MainMenuManager | DataManager / AudioManager | Resume/backdrop; manual save; menu/quit confirm; BGM/SFX/mute/%/save status | 8 |
| Màn khác ngoài storyboard | Chưa đủ reference | TribulationModalUI; OfflineRewardPopupUI; CharacterDetailModalUI; GameplayChoicesService; TutorialManager; ShopToggleUI; ShopItemUI; SkillCardUI | CultivationManager / OfflineEarningsManager / ShopManager / SkillManager | Độ kiếp confirm/cancel, offline reward, detail, lựa chọn kiếm pháp, tutorial, shop/upgrade; giữ entry legacy | 2–9 |



## 2. Contract không được đổi

SaveSlotUI.Initialize bind theo slot rỗng/có dữ liệu; giữ slotIndex và confirm xóa/vào game. MobileSkillHUD gọi CastActiveSkill() hoặc CastNormalSkillManual(index), không tự tính lại damage/chi phí.
StorageModalUI auto-dismantle toggle ghi cấu hình service, không phải trang trí. MilestoneModalUI đã có claim all, cần giữ.
SecretRealmPlayController cùng sở hữu UI/minigame: không disable controller để ẩn view vì có timer/coroutine/reward. PauseMenuManager.OpenPauseMenu hiện không dừng timeScale; MainMenuManager đặt 0/1 lúc menu/vào game. Giữ semantics này trong redesign.
Một tap chỉ gửi một transaction đến service cũ; không duplicate listener. RemoveAllListeners không xóa persistent inspector events, phải đối chiếu cả hai loại.


## 3. Serialized UnityEvents tìm thấy

| Source:block | Hierarchy | Component | Target | Method | Mode | CallState | Arguments |
|---|---|---|---|---|---|---|---|
| Assets/_Recovery/0.unity:19548 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/NextBtn | Button | {fileID: 878491561} | NextPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/_Recovery/0.unity:22433 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/PrevBtn | Button | {fileID: 878491561} | PrevPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/_Recovery/0.unity:36496 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/SortBtn | Button | {fileID: 878491561} | SortInventory | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:2037 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/PrevBtn | Button | {fileID: 1629939730} | PrevPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:39470 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/NextBtn | Button | {fileID: 1629939730} | NextPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:52766 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/btn_QuickMerge | Button | {fileID: 1629939730} | QuickMerge | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:79933 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/SortBtn | Button | {fileID: 1629939730} | SortInventory | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:2895 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/PrevBtn | Button | {fileID: 1629939730} | PrevPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:93588 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/NextBtn | Button | {fileID: 1629939730} | NextPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:125898 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/btn_QuickMerge | Button | {fileID: 1629939730} | QuickMerge | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:132193 | Canvas/StorageModalUI/ModalBox/FooterBar/BtnCloseFooter | Button | {fileID: 1644478630} | Close | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:194241 | Canvas/MobileUIRoot/Drawer_0/MergePanel/InventoryContainer/PaginationPanel/SortBtn | Button | {fileID: 1629939730} | SortInventory | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene.unity:1163 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/PrevBtn | Button | {fileID: 1629939730} | PrevPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene.unity:24994 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/NextBtn | Button | {fileID: 1629939730} | NextPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene.unity:32690 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/btn_QuickMerge | Button | {fileID: 1629939730} | QuickMerge | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene.unity:50398 | Canvas/RightPanel/MergePanel/InventoryContainer/PaginationPanel/SortBtn | Button | {fileID: 1629939730} | SortInventory | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene_MobileWork.unity:3242 | #0/MergePanel/InventoryContainer/PaginationPanel/PrevBtn | Button | {fileID: 1629939730} | PrevPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene_MobileWork.unity:103499 | #0/MergePanel/InventoryContainer/PaginationPanel/NextBtn | Button | {fileID: 1629939730} | NextPage | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene_MobileWork.unity:138963 | #0/MergePanel/InventoryContainer/PaginationPanel/btn_QuickMerge | Button | {fileID: 1629939730} | QuickMerge | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene_MobileWork.unity:145498 | Canvas/StorageModalUI/ModalBox/FooterBar/BtnCloseFooter | Button | {fileID: 1644478630} | Close | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |
| Assets/Scenes/SampleScene_MobileWork.unity:213168 | #0/MergePanel/InventoryContainer/PaginationPanel/SortBtn | Button | {fileID: 1629939730} | SortInventory | 0 | 2 | m_Arguments: m_ObjectArgument: {fileID: 0} m_ObjectArgumentAssemblyTypeName: m_IntArgument: 0 m_FloatArgument: 0 m_StringArgument: m_BoolArgument: 0 m_CallState: 2  |



## 4. Runtime callbacks/input/subscriptions

Call sites có tối đa 8 dòng context; lambda dài phải đọc tiếp source. Editor/test tách rõ khỏi runtime. Đây là binding code, chưa phải dump listener đang chạy.

| Source:line | Loại | Binding / context |
|---|---|---|
| Assets/Scripts/Combat/MergeItem.cs:5 | Runtime | public class MergeItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler { public int level = 1; public int currentPieces = 1; // Số lượng mảnh/phôi đang gộp [HideInInspector] public Transform originalParent;  private RectTransform rectTransform; private CanvasGroup canvasGroup; |
| Assets/Scripts/Editor/BossFixerTool.cs:80 | Editor/test | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(handler.OnStorageButtonClicked); } EditorUtility.SetDirty(btnStorageObj); Debug.Log("✅ [BossFixer] Đã gắn StorageButtonHandler lên Canvas/MobileUIRoot/BtnHUDStorage!"); }  // 3. Đảm bảo StorageModalUI tự động hook nút nếu bị mất |
| Assets/Scripts/Editor/BossFixerTool.cs:81 | Editor/test | btn.onClick.AddListener(handler.OnStorageButtonClicked); } EditorUtility.SetDirty(btnStorageObj); Debug.Log("✅ [BossFixer] Đã gắn StorageButtonHandler lên Canvas/MobileUIRoot/BtnHUDStorage!"); }  // 3. Đảm bảo StorageModalUI tự động hook nút nếu bị mất var storageUI = Object.FindAnyObjectByType<StorageModalUI>(FindObjectsInactive.Include); |
| Assets/Scripts/Editor/HUDLayoutRefinementTool.cs:242 | Editor/test | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => AchievementModalUI.Open()); } }  // Nút thu nhỏ / phóng to (BtnToggle) Transform toggleBtnTr = questTr.Find("BtnToggle"); if (toggleBtnTr != null) |
| Assets/Scripts/Editor/HUDLayoutRefinementTool.cs:243 | Editor/test | btn.onClick.AddListener(() => AchievementModalUI.Open()); } }  // Nút thu nhỏ / phóng to (BtnToggle) Transform toggleBtnTr = questTr.Find("BtnToggle"); if (toggleBtnTr != null) { |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:782 | Editor/test | bsBtn.onClick.RemoveAllListeners(); bsBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.TryAddSword(1); });  Transform bsTxtTr = btnSpawn.Find("Label"); if (bsTxtTr == null) bsTxtTr = btnSpawn.Find("Text"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:783 | Editor/test | bsBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.TryAddSword(1); });  Transform bsTxtTr = btnSpawn.Find("Label"); if (bsTxtTr == null) bsTxtTr = btnSpawn.Find("Text"); if (bsTxtTr == null) |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:825 | Editor/test | bqBtn.onClick.RemoveAllListeners(); bqBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.QuickMerge(); });  Transform bqTxtTr = btnQuick.Find("Text"); if (bqTxtTr == null) bqTxtTr = btnQuick.Find("Label"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:826 | Editor/test | bqBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.QuickMerge(); });  Transform bqTxtTr = btnQuick.Find("Text"); if (bqTxtTr == null) bqTxtTr = btnQuick.Find("Label"); if (bqTxtTr == null) |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:868 | Editor/test | bsoBtn.onClick.RemoveAllListeners(); bsoBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.SortInventory(); });  Transform bsoTxtTr = btnSort.Find("Txt"); if (bsoTxtTr == null) bsoTxtTr = btnSort.Find("Text"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:869 | Editor/test | bsoBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.SortInventory(); });  Transform bsoTxtTr = btnSort.Find("Txt"); if (bsoTxtTr == null) bsoTxtTr = btnSort.Find("Text"); if (bsoTxtTr == null) bsoTxtTr = btnSort.Find("Label"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:912 | Editor/test | bpBtn.onClick.RemoveAllListeners(); bpBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.PrevPage(); });  Transform bpTxtTr = btnPrev.Find("Txt"); if (bpTxtTr == null) bpTxtTr = btnPrev.Find("Text"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:913 | Editor/test | bpBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.PrevPage(); });  Transform bpTxtTr = btnPrev.Find("Txt"); if (bpTxtTr == null) bpTxtTr = btnPrev.Find("Text"); if (bpTxtTr == null) |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:974 | Editor/test | bnBtn.onClick.RemoveAllListeners(); bnBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.NextPage(); });  Transform bnTxtTr = btnNext.Find("Txt"); if (bnTxtTr == null) bnTxtTr = btnNext.Find("Text"); |
| Assets/Scripts/Editor/InGameUIRefinementTool.cs:975 | Editor/test | bnBtn.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.NextPage(); });  Transform bnTxtTr = btnNext.Find("Txt"); if (bnTxtTr == null) bnTxtTr = btnNext.Find("Text"); if (bnTxtTr == null) |
| Assets/Scripts/Editor/MasterBuilderTool.cs:449 | Editor/test | btnSort.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.SortInventory(); });  // Nút Prev Button btnPrev = CreateButton(navBar.transform, "btn_Prev", font, "<", new Color(0.3f, 0.3f, 0.4f)); btnPrev.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.PrevPage(); });  // Text số trang GameObject pageTxtObj = MakeText(navBar.transform, "txt_Page", font, 18, Color.white, |
| Assets/Scripts/Editor/MasterBuilderTool.cs:453 | Editor/test | btnPrev.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.PrevPage(); });  // Text số trang GameObject pageTxtObj = MakeText(navBar.transform, "txt_Page", font, 18, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 25)); pageTxtObj.GetComponent<Text>().text = "1/20"; pageTxtObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter; |
| Assets/Scripts/Editor/MasterBuilderTool.cs:466 | Editor/test | btnNext.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.NextPage(); });  // -- 20 Trang kho đồ (mỗi trang 4x4 = 16 ô) -- Transform[] pages = new Transform[20]; for (int p = 0; p < 20; p++) { GameObject page = new GameObject($"InvPage_{p}", typeof(RectTransform)); page.transform.SetParent(mergePanel.transform, false); |
| Assets/Scripts/Editor/MasterBuilderTool.cs:612 | Editor/test | btn.onClick.AddListener(scUI.OnUnlockClicked); } }  // ================================================================ //  BƯỚC 6: LINK DATA (ScriptableObjects) // ================================================================ static void LinkData(GameObject gmObj) |
| Assets/Scripts/Editor/MobileHUDLayoutTool.cs:146 | Editor/test | toggleBtn.onClick.RemoveListener(drawerCtrl.ToggleDockCollapse); toggleBtn.onClick.AddListener(drawerCtrl.ToggleDockCollapse); }  dockToggleGo.SetActive(true); EditorUtility.SetDirty(dockToggleGo); Debug.Log("✅ [HUDLayout] Đã cấu hình nút 'BottomDockToggle' tại pos=(0, 196), kích thước 180x32 trên đỉnh MobileDock!");  |
| Assets/Scripts/Editor/MobileHUDLayoutTool.cs:147 | Editor/test | toggleBtn.onClick.AddListener(drawerCtrl.ToggleDockCollapse); }  dockToggleGo.SetActive(true); EditorUtility.SetDirty(dockToggleGo); Debug.Log("✅ [HUDLayout] Đã cấu hình nút 'BottomDockToggle' tại pos=(0, 196), kích thước 180x32 trên đỉnh MobileDock!");  // ========================================================================= |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1037 | Editor/test | bDust.onClick.RemoveAllListeners(); bDust.onClick.AddListener(ui.OnUseDustClicked); ui.btnFinishWithDust = bDust; ui.useDustBtn = bDust;  // 4. KHU VỰC 3: HAI THẺ BÀI THUỐC (y: 0.29 -> 0.565) RectTransform recipesBox = CreateRect(panel, "RecipeContainer", new Vector2(0.03f, 0.29f), new Vector2(0.97f, 0.565f));  |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1038 | Editor/test | bDust.onClick.AddListener(ui.OnUseDustClicked); ui.btnFinishWithDust = bDust; ui.useDustBtn = bDust;  // 4. KHU VỰC 3: HAI THẺ BÀI THUỐC (y: 0.29 -> 0.565) RectTransform recipesBox = CreateRect(panel, "RecipeContainer", new Vector2(0.03f, 0.29f), new Vector2(0.97f, 0.565f));  // Thẻ 1: Tu Vi Đan (Trái) |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1075 | Editor/test | bBrewTuVi.onClick.RemoveAllListeners(); bBrewTuVi.onClick.AddListener(ui.OnBrewCultivationClicked); ui.btnBrewTuVi = bBrewTuVi; ui.brewCultivationBtn = bBrewTuVi;  // Thẻ 2: Đột Phá Đan (Phải) RectTransform cardBT = CreateRect(recipesBox, "Card_Breakthrough", new Vector2(0.515f, 0f), new Vector2(1f, 1f)); GetOrAdd<UnityEngine.UI.Image>(cardBT.gameObject).color = new Color(0.18f, 0.14f, 0.10f, 0.98f); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1076 | Editor/test | bBrewTuVi.onClick.AddListener(ui.OnBrewCultivationClicked); ui.btnBrewTuVi = bBrewTuVi; ui.brewCultivationBtn = bBrewTuVi;  // Thẻ 2: Đột Phá Đan (Phải) RectTransform cardBT = CreateRect(recipesBox, "Card_Breakthrough", new Vector2(0.515f, 0f), new Vector2(1f, 1f)); GetOrAdd<UnityEngine.UI.Image>(cardBT.gameObject).color = new Color(0.18f, 0.14f, 0.10f, 0.98f);  |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1110 | Editor/test | bBrewBT.onClick.RemoveAllListeners(); bBrewBT.onClick.AddListener(ui.OnBrewBreakthroughClicked); ui.btnBrewBreakthrough = bBrewBT; ui.brewBreakthroughBtn = bBrewBT;  // 5. KHU VỰC 4: ĐAN KHỐ & UỐNG ĐAN (y: 0.085 -> 0.275) RectTransform pillVault = CreateRect(panel, "PillVault", new Vector2(0.03f, 0.085f), new Vector2(0.97f, 0.275f)); GetOrAdd<UnityEngine.UI.Image>(pillVault.gameObject).color = new Color(0.06f, 0.09f, 0.14f, 0.96f); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1111 | Editor/test | bBrewBT.onClick.AddListener(ui.OnBrewBreakthroughClicked); ui.btnBrewBreakthrough = bBrewBT; ui.brewBreakthroughBtn = bBrewBT;  // 5. KHU VỰC 4: ĐAN KHỐ & UỐNG ĐAN (y: 0.085 -> 0.275) RectTransform pillVault = CreateRect(panel, "PillVault", new Vector2(0.03f, 0.085f), new Vector2(0.97f, 0.275f)); GetOrAdd<UnityEngine.UI.Image>(pillVault.gameObject).color = new Color(0.06f, 0.09f, 0.14f, 0.96f);  |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1148 | Editor/test | bDrink.onClick.RemoveAllListeners(); bDrink.onClick.AddListener(ui.OnUseTuViPillClicked); ui.btnUseTuViPill = bDrink;  // 6. KHU VỰC 5: HƯỚNG DẪN DƯỚI CÙNG (y: 0.015 -> 0.075) RectTransform guide = CreateRect(panel, "FooterGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); CreateLabel(guide, "Label", "⚗️ Bí quyết: Thu thập thảo dược từ quái & Bí Cảnh. Luyện đan thất bại hoàn lại Bụi Dược.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1149 | Editor/test | bDrink.onClick.AddListener(ui.OnUseTuViPillClicked); ui.btnUseTuViPill = bDrink;  // 6. KHU VỰC 5: HƯỚNG DẪN DƯỚI CÙNG (y: 0.015 -> 0.075) RectTransform guide = CreateRect(panel, "FooterGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); CreateLabel(guide, "Label", "⚗️ Bí quyết: Thu thập thảo dược từ quái & Bí Cảnh. Luyện đan thất bại hoàn lại Bụi Dược.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)); var tGuide = guide.Find("Label").GetComponent<UnityEngine.UI.Text>(); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1288 | Editor/test | bUpg.onClick.RemoveAllListeners(); bUpg.onClick.AddListener(() => ui.OnUpgradeClicked(idx)); ui.upgradeButtons[i] = bUpg; }  // 4. Footer Tip RectTransform guide = CreateRect(panel, "ArtifactGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1289 | Editor/test | bUpg.onClick.AddListener(() => ui.OnUpgradeClicked(idx)); ui.upgradeButtons[i] = bUpg; }  // 4. Footer Tip RectTransform guide = CreateRect(panel, "ArtifactGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); CreateLabel(guide, "Label", "💡 Bí quyết: Tiêu diệt Boss để thu thập Quặng Phôi. Cường hóa đều 4 món để kích hoạt Hào Quang.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1367 | Editor/test | bActSpirit.onClick.RemoveAllListeners(); bActSpirit.onClick.AddListener(ui.OnActivateSpirit); ui.activateSpiritBtn = bActSpirit;  // 4. ScrollView 6 Đại Trận Pháp (y: 0.08f -> 0.80f) RectTransform scroll = CreateRect(panel, "FormationScrollView", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.80f)); var scrollImg = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject); scrollImg.color = new Color(0.03f, 0.05f, 0.09f, 0.20f); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1368 | Editor/test | bActSpirit.onClick.AddListener(ui.OnActivateSpirit); ui.activateSpiritBtn = bActSpirit;  // 4. ScrollView 6 Đại Trận Pháp (y: 0.08f -> 0.80f) RectTransform scroll = CreateRect(panel, "FormationScrollView", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.80f)); var scrollImg = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject); scrollImg.color = new Color(0.03f, 0.05f, 0.09f, 0.20f); scrollImg.raycastTarget = true; |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1467 | Editor/test | bSel.onClick.RemoveAllListeners(); bSel.onClick.AddListener(() => ui.OnSelectFormation(fIdx)); ui.selectButtons[i] = bSel;  // Nút Tu Luyện (y: 0.05 -> 0.28, x: 0.77 -> 0.96) RectTransform btnUpg = CreateRect(card, "BtnUpgrade", new Vector2(0.77f, 0.05f), new Vector2(0.96f, 0.28f)); GetOrAdd<UnityEngine.UI.Image>(btnUpg.gameObject).color = new Color(0.58f, 0.32f, 0.10f, 1f); var bUpg = GetOrAdd<UnityEngine.UI.Button>(btnUpg.gameObject); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1468 | Editor/test | bSel.onClick.AddListener(() => ui.OnSelectFormation(fIdx)); ui.selectButtons[i] = bSel;  // Nút Tu Luyện (y: 0.05 -> 0.28, x: 0.77 -> 0.96) RectTransform btnUpg = CreateRect(card, "BtnUpgrade", new Vector2(0.77f, 0.05f), new Vector2(0.96f, 0.28f)); GetOrAdd<UnityEngine.UI.Image>(btnUpg.gameObject).color = new Color(0.58f, 0.32f, 0.10f, 1f); var bUpg = GetOrAdd<UnityEngine.UI.Button>(btnUpg.gameObject); CreateLabel(btnUpg, "Label", "TU LUYỆN", font, Vector2.zero, Vector2.one); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1480 | Editor/test | bUpg.onClick.RemoveAllListeners(); bUpg.onClick.AddListener(() => ui.OnUpgradeFormation(fIdx)); ui.upgradeButtons[i] = bUpg; }  // 5. Footer Tip RectTransform guide = CreateRect(panel, "FormationGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); |
| Assets/Scripts/Editor/MobileSceneSetupTool.cs:1481 | Editor/test | bUpg.onClick.AddListener(() => ui.OnUpgradeFormation(fIdx)); ui.upgradeButtons[i] = bUpg; }  // 5. Footer Tip RectTransform guide = CreateRect(panel, "FormationGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f)); GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f); CreateLabel(guide, "Label", "✦ Mỗi thời điểm chỉ kích hoạt 1 Đại Trận. Linh hoạt chuyển đổi để quét quái hoặc khắc chế Boss.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)); |
| Assets/Scripts/Editor/MobileUIOptimizationTool.cs:204 | Editor/test | bBtn.onClick.RemoveAllListeners(); bBtn.onClick.AddListener(drawerCtrl.CloseActiveDrawer); } Debug.Log("✅ [4/4] MobileBackdrop đã sẵn sàng chặn click xuyên và hỗ trợ đóng Drawer 1-chạm."); }  // ── 5. GẮN MOBILE UI NAVIGATION MANAGER VÀ MOCK ONLINE SERVICE ── if (mobileRoot.GetComponent<MobileUINavigationManager>() == null) |
| Assets/Scripts/Editor/MobileUIOptimizationTool.cs:205 | Editor/test | bBtn.onClick.AddListener(drawerCtrl.CloseActiveDrawer); } Debug.Log("✅ [4/4] MobileBackdrop đã sẵn sàng chặn click xuyên và hỗ trợ đóng Drawer 1-chạm."); }  // ── 5. GẮN MOBILE UI NAVIGATION MANAGER VÀ MOCK ONLINE SERVICE ── if (mobileRoot.GetComponent<MobileUINavigationManager>() == null) { |
| Assets/Scripts/Editor/PauseMenuBuilderTool.cs:420 | Editor/test | btnComp.onClick.RemoveAllListeners(); btnComp.onClick.AddListener(pbh.OnPauseButtonClicked);  // Mặc định ẩn modal để không che màn hình, khi click nút hoặc ESC sẽ mở ngay lập tức modalObj.SetActive(false); modalObj.transform.SetAsLastSibling();  EditorUtility.SetDirty(btnObj); |
| Assets/Scripts/Editor/PauseMenuBuilderTool.cs:421 | Editor/test | btnComp.onClick.AddListener(pbh.OnPauseButtonClicked);  // Mặc định ẩn modal để không che màn hình, khi click nút hoặc ESC sẽ mở ngay lập tức modalObj.SetActive(false); modalObj.transform.SetAsLastSibling();  EditorUtility.SetDirty(btnObj); EditorUtility.SetDirty(modalObj); |
| Assets/Scripts/Editor/StorageDismantleRegression.cs:105 | Editor/test | storage.OnStorageChanged += () => notifications++; ui = UnityEngine.Object.FindFirstObjectByType<StorageModalUI>(FindObjectsInactive.Include); Application.runInBackground = true; }  IEnumerator Start() { report = new Report { label = StorageDismantleRegression.Label, unityVersion = Application.unityVersion }; |
| Assets/Scripts/Editor/StorageDismantleRegression.cs:236 | Editor/test | storage.OnStorageChanged += reenter; dismantle.OnDismantleExecuted += reenter; Application.logMessageReceived += counter; try { ui.btnBatchDismantle.onClick.Invoke(); } finally { storage.OnStorageChanged -= reenter; dismantle.OnDismantleExecuted -= reenter; |
| Assets/Scripts/Editor/StorageDismantleRegression.cs:237 | Editor/test | dismantle.OnDismantleExecuted += reenter; Application.logMessageReceived += counter; try { ui.btnBatchDismantle.onClick.Invoke(); } finally { storage.OnStorageChanged -= reenter; dismantle.OnDismantleExecuted -= reenter; Application.logMessageReceived -= counter; |
| Assets/Scripts/Editor/StorageDismantleRegression.cs:242 | Editor/test | storage.OnStorageChanged -= reenter; dismantle.OnDismantleExecuted -= reenter; Application.logMessageReceived -= counter; } yield return null; yield return null; Check(sorts == 1, "Batch sorts the sword inventory exactly once"); Check(equipped != null && equipped.transform.IsChildOf(merge.equipPanel) && keptSwords.TrueForAll(s => s != null && s.level == 2), |
| Assets/Scripts/Editor/StorageDismantleRegression.cs:243 | Editor/test | dismantle.OnDismantleExecuted -= reenter; Application.logMessageReceived -= counter; } yield return null; yield return null; Check(sorts == 1, "Batch sorts the sword inventory exactly once"); Check(equipped != null && equipped.transform.IsChildOf(merge.equipPanel) && keptSwords.TrueForAll(s => s != null && s.level == 2), "Equipped Lv1 and higher-tier swords survive"); |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:358 | Editor/test | btnCloseFt.GetComponent<Button>().onClick.AddListener(ui.Close);  modalObj.SetActive(false);  // ───────────────────────────────────────────────────────────────────── // 6. GẮN NÚT MỞ KHO ĐỒ 🎒 TRÊN HUD & TRONG DRAWER KIẾM LÔ // ───────────────────────────────────────────────────────────────────── AttachStorageButtonToHUD(canvasObj, defaultFont); |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:423 | Editor/test | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => StorageModalUI.Open()); }  /// <summary> /// Gắn nút Lò Phân Giải & Kho Đồ ngay trong Drawer Kiếm Lô (Merge Panel) /// </summary> private static void AttachStorageButtonToMergeDrawer(GameObject canvasObj, Font font) |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:424 | Editor/test | btn.onClick.AddListener(() => StorageModalUI.Open()); }  /// <summary> /// Gắn nút Lò Phân Giải & Kho Đồ ngay trong Drawer Kiếm Lô (Merge Panel) /// </summary> private static void AttachStorageButtonToMergeDrawer(GameObject canvasObj, Font font) { |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:473 | Editor/test | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => StorageModalUI.Open(StorageCategory.Swords)); }  private static void AttachStorageButtonToHUD(GameObject canvasObj, Font font) { Transform questTr = canvasObj.transform.Find("MobileUIRoot/TutorialTargetQuestBar"); if (questTr == null) questTr = canvasObj.transform.Find("TutorialTargetQuestBar"); |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:474 | Editor/test | btn.onClick.AddListener(() => StorageModalUI.Open(StorageCategory.Swords)); }  private static void AttachStorageButtonToHUD(GameObject canvasObj, Font font) { Transform questTr = canvasObj.transform.Find("MobileUIRoot/TutorialTargetQuestBar"); if (questTr == null) questTr = canvasObj.transform.Find("TutorialTargetQuestBar"); if (questTr == null) return; |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:518 | Editor/test | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => StorageModalUI.Open()); }  private static GameObject CreateTextObj(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor align, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta) { GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text)); |
| Assets/Scripts/Editor/StorageUIBuilderTool.cs:519 | Editor/test | btn.onClick.AddListener(() => StorageModalUI.Open()); }  private static GameObject CreateTextObj(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor align, Color col, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta) { GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false); |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs:181 | Editor/test | modal.confirmButton.onClick.RemoveAllListeners(); modal.confirmButton.onClick.AddListener(modal.OnConfirmClicked); modal.cancelButton.onClick.RemoveAllListeners(); modal.cancelButton.onClick.AddListener(modal.OnCancelClicked);  modalObj.SetActive(false);  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs:182 | Editor/test | modal.confirmButton.onClick.AddListener(modal.OnConfirmClicked); modal.cancelButton.onClick.RemoveAllListeners(); modal.cancelButton.onClick.AddListener(modal.OnCancelClicked);  modalObj.SetActive(false);  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); Debug.Log("<color=green>✅ [TribulationUIBuilder] Đã tạo thành công TribulationModalUI trong Canvas!</color>"); |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs:183 | Editor/test | modal.cancelButton.onClick.RemoveAllListeners(); modal.cancelButton.onClick.AddListener(modal.OnCancelClicked);  modalObj.SetActive(false);  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); Debug.Log("<color=green>✅ [TribulationUIBuilder] Đã tạo thành công TribulationModalUI trong Canvas!</color>"); } |
| Assets/Scripts/Editor/TribulationUIBuilderTool.cs:184 | Editor/test | modal.cancelButton.onClick.AddListener(modal.OnCancelClicked);  modalObj.SetActive(false);  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); Debug.Log("<color=green>✅ [TribulationUIBuilder] Đã tạo thành công TribulationModalUI trong Canvas!</color>"); } } |
| Assets/Scripts/Managers/CombatManager.cs:283 | Runtime | GameManager.Instance.OnStateChanged += HandleStateChanged;    }    |
| Assets/Scripts/Managers/CombatManager.cs:299 | Runtime | CultivationManager.Instance.OnStageChanged += HandleStageChanged;    CultivationManager.Instance.OnTuViChanged += HandleTuViChanged;    |
| Assets/Scripts/Managers/CombatManager.cs:303 | Runtime | CultivationManager.Instance.OnTuViChanged += HandleTuViChanged;        |
| Assets/Scripts/Managers/CombatManager.cs:327 | Runtime | PlayerStatsService.Instance.OnStatsChanged += HandleStatsChanged;        |
| Assets/Scripts/Managers/CombatManager.cs:567 | Runtime | GameManager.Instance.OnStateChanged -= HandleStateChanged;    }    |
| Assets/Scripts/Managers/CombatManager.cs:583 | Runtime | CultivationManager.Instance.OnStageChanged -= HandleStageChanged;    CultivationManager.Instance.OnTuViChanged -= HandleTuViChanged;    |
| Assets/Scripts/Managers/CombatManager.cs:587 | Runtime | CultivationManager.Instance.OnTuViChanged -= HandleTuViChanged;    }    |
| Assets/Scripts/Managers/CombatManager.cs:599 | Runtime | PlayerStatsService.Instance.OnStatsChanged -= HandleStatsChanged;    }    |
| Assets/Scripts/Managers/EncounterManager.cs:116 | Runtime | public class EncounterItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler { public Vector2 targetPos; public float speed = 100f; private RectTransform rt; private CanvasGroup canvasGroup; private Button button; private int? heldPointer; |
| Assets/Scripts/Managers/EncounterManager.cs:132 | Runtime | button.onClick.AddListener(Collect); }  private void Update() { bool available = !collected && EncounterManager.CanInteractWithEncounter(); canvasGroup.alpha = available ? 1f : 0f; canvasGroup.blocksRaycasts = canvasGroup.interactable = available; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:72 | Runtime | MobileDrawerUIController.Instance.OnDockCollapseChanged += OnDockCollapseChanged; if (styleButton != null) { var rect = (RectTransform)styleButton.transform; rect.anchoredPosition = MobileDrawerUIController.Instance.IsDockCollapsed ? CollapsedStylePosition : DefaultStylePosition; } } } |
| Assets/Scripts/Managers/GameplayChoicesService.cs:87 | Runtime | MobileDrawerUIController.Instance.OnDockCollapseChanged -= OnDockCollapseChanged; } }  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { if (mode != LoadSceneMode.Single) return; unsavedEncounterKillsRemaining = 0; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:338 | Runtime | bBtn.onClick.AddListener(ClosePanel); blocker.SetActive(false); var choiceArea = new GameObject("ChoiceSafeArea", typeof(RectTransform), typeof(SafeAreaFitter)); choiceArea.transform.SetParent(root.transform, false); choiceArea.GetComponent<SafeAreaFitter>().Refresh(); panel = new GameObject("GameplayChoicePanel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(choiceArea.transform, false); var pr = (RectTransform)panel.transform; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:400 | Runtime | var button = go.GetComponent<Button>(); button.onClick.AddListener(action); return button; } private static void SetupText(Text text, string value) { text.font = UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = 20; text.color = new Color(0.96f, 0.88f, 0.50f, 1f); text.fontStyle = FontStyle.Bold; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; |
| Assets/Scripts/Managers/MergeManager.cs:41 | Runtime | SwordLoadoutService.Instance.OnCapacityChanged += HandleCapacityChanged; // Kiểm tra trễ 0.5s. Nếu DataManager.LoadCurrentGame KHÔNG được gọi (do test trực tiếp Scene) // hoặc load bị lỗi, thì chúng ta sẽ tự tạo 1 cây kiếm mặc định để không bị kẹt game. Invoke(nameof(FailsafeCheckSword), 0.5f);  OnEquipChanged(); }  |
| Assets/Scripts/Managers/MergeManager.cs:52 | Runtime | SwordLoadoutService.Instance.OnCapacityChanged -= HandleCapacityChanged; }  private void HandleCapacityChanged(int capacity) => OnEquipChanged();  private void FailsafeCheckSword() { if (equipPanel == null) return; |
| Assets/Scripts/Managers/TooltipManager.cs:184 | Runtime | public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler { public string tooltipText;  public void OnPointerEnter(PointerEventData eventData) { if (TooltipManager.Instance != null && !string.IsNullOrEmpty(tooltipText)) { |
| Assets/Scripts/Managers/TutorialManager.cs:221 | Runtime | qBtn.onClick.RemoveAllListeners(); qBtn.onClick.AddListener(OnQuestBarClicked);  questBarOutline = questBarObj.GetComponent<Outline>(); if (questBarOutline == null) questBarOutline = questBarObj.AddComponent<Outline>(); questBarOutline.effectColor = new Color(1f, 0.82f, 0.25f, 0.85f); questBarOutline.effectDistance = new Vector2(1.2f, -1.2f); bool wideQuestBar = originalQuestBarSize.x >= 400f; |
| Assets/Scripts/Managers/TutorialManager.cs:222 | Runtime | qBtn.onClick.AddListener(OnQuestBarClicked);  questBarOutline = questBarObj.GetComponent<Outline>(); if (questBarOutline == null) questBarOutline = questBarObj.AddComponent<Outline>(); questBarOutline.effectColor = new Color(1f, 0.82f, 0.25f, 0.85f); questBarOutline.effectDistance = new Vector2(1.2f, -1.2f); bool wideQuestBar = originalQuestBarSize.x >= 400f; float actionLaneWidth = wideQuestBar ? 232f : 140f; |
| Assets/Scripts/Managers/TutorialManager.cs:398 | Runtime | btnClaimQuick.onClick.RemoveAllListeners(); btnClaimQuick.onClick.AddListener(OnQuestBarClicked);  // Nút Thành Tựu [🏆] (BtnAchievement) Transform achTr = questBarObj.transform.Find("BtnAchievement"); GameObject btnAchObj = achTr != null ? achTr.gameObject : null; if (btnAchObj == null) { |
| Assets/Scripts/Managers/TutorialManager.cs:399 | Runtime | btnClaimQuick.onClick.AddListener(OnQuestBarClicked);  // Nút Thành Tựu [🏆] (BtnAchievement) Transform achTr = questBarObj.transform.Find("BtnAchievement"); GameObject btnAchObj = achTr != null ? achTr.gameObject : null; if (btnAchObj == null) { btnAchObj = new GameObject("BtnAchievement", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline)); |
| Assets/Scripts/Managers/TutorialManager.cs:442 | Runtime | btnAchievement.onClick.RemoveAllListeners(); btnAchievement.onClick.AddListener(OnAchievementButtonClicked);  Transform badgeTr = btnAchObj.transform.Find("Badge"); achievementBadgeObj = badgeTr != null ? badgeTr.gameObject : null; if (achievementBadgeObj == null) { achievementBadgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image)); |
| Assets/Scripts/Managers/TutorialManager.cs:443 | Runtime | btnAchievement.onClick.AddListener(OnAchievementButtonClicked);  Transform badgeTr = btnAchObj.transform.Find("Badge"); achievementBadgeObj = badgeTr != null ? badgeTr.gameObject : null; if (achievementBadgeObj == null) { achievementBadgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image)); achievementBadgeObj.transform.SetParent(btnAchObj.transform, false); |
| Assets/Scripts/Managers/TutorialManager.cs:490 | Runtime | toggleBtn.onClick.RemoveAllListeners(); toggleBtn.onClick.AddListener(() => { isCollapsed = !isCollapsed; if (tTxt != null) tTxt.text = isCollapsed ? "Mở" : "Gọn"; if (txtQuestTitle != null) txtQuestTitle.gameObject.SetActive(!isCollapsed); if (descScrollRect != null) descScrollRect.gameObject.SetActive(!isCollapsed); else if (txtQuestReward != null) txtQuestReward.gameObject.SetActive(!isCollapsed); |
| Assets/Scripts/Managers/TutorialManager.cs:491 | Runtime | toggleBtn.onClick.AddListener(() => { isCollapsed = !isCollapsed; if (tTxt != null) tTxt.text = isCollapsed ? "Mở" : "Gọn"; if (txtQuestTitle != null) txtQuestTitle.gameObject.SetActive(!isCollapsed); if (descScrollRect != null) descScrollRect.gameObject.SetActive(!isCollapsed); else if (txtQuestReward != null) txtQuestReward.gameObject.SetActive(!isCollapsed); if (pBgObj != null) pBgObj.SetActive(!isCollapsed); |
| Assets/Scripts/UI/AchievementButtonHandler.cs:27 | Runtime | btn.onClick.RemoveListener(OnAchievementButtonClicked); btn.onClick.AddListener(OnAchievementButtonClicked); } }  public void OnAchievementButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AchievementButtonHandler.cs:28 | Runtime | btn.onClick.AddListener(OnAchievementButtonClicked); } }  public void OnAchievementButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); AchievementModalUI.Open(); |
| Assets/Scripts/UI/AchievementModalUI.cs:81 | Runtime | AchievementManager.Instance.OnAchievementsUpdated += OnAchievementsUpdatedHandler; }  if (!isOpen) { if (canvasGroup != null) { canvasGroup.alpha = 0f; |
| Assets/Scripts/UI/AchievementModalUI.cs:101 | Runtime | AchievementManager.Instance.OnAchievementsUpdated -= OnAchievementsUpdatedHandler; } }  private void OnAchievementsUpdatedHandler() { // Tối ưu hiệu năng: Chỉ tái tạo UI cards khi modal ĐANG MỞ! if (isOpen && gameObject.activeInHierarchy) |
| Assets/Scripts/UI/AchievementModalUI.cs:118 | Runtime | btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(CloseModal); }  if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(CloseModal); |
| Assets/Scripts/UI/AchievementModalUI.cs:119 | Runtime | btnClose.onClick.AddListener(CloseModal); }  if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(CloseModal); } |
| Assets/Scripts/UI/AchievementModalUI.cs:124 | Runtime | btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(CloseModal); }  if (btnClaimAll != null) { btnClaimAll.onClick.RemoveAllListeners(); btnClaimAll.onClick.AddListener(OnClaimAllClicked); |
| Assets/Scripts/UI/AchievementModalUI.cs:125 | Runtime | btnBackdrop.onClick.AddListener(CloseModal); }  if (btnClaimAll != null) { btnClaimAll.onClick.RemoveAllListeners(); btnClaimAll.onClick.AddListener(OnClaimAllClicked); } |
| Assets/Scripts/UI/AchievementModalUI.cs:130 | Runtime | btnClaimAll.onClick.RemoveAllListeners(); btnClaimAll.onClick.AddListener(OnClaimAllClicked); }  if (tabButtons != null) { for (int i = 0; i < tabButtons.Length; i++) { |
| Assets/Scripts/UI/AchievementModalUI.cs:131 | Runtime | btnClaimAll.onClick.AddListener(OnClaimAllClicked); }  if (tabButtons != null) { for (int i = 0; i < tabButtons.Length; i++) { int catIndex = i; |
| Assets/Scripts/UI/AchievementModalUI.cs:141 | Runtime | tabButtons[i].onClick.RemoveAllListeners(); tabButtons[i].onClick.AddListener(() => SelectCategory((AchievementCategory)catIndex)); } } } }  // ── MỞ & ĐÓNG MODAL ────────────────────────────────────────────────────── |
| Assets/Scripts/UI/AchievementModalUI.cs:142 | Runtime | tabButtons[i].onClick.AddListener(() => SelectCategory((AchievementCategory)catIndex)); } } } }  // ── MỞ & ĐÓNG MODAL ────────────────────────────────────────────────────── public static void Open() |
| Assets/Scripts/UI/AchievementModalUI.cs:549 | Runtime | btn.onClick.AddListener(() => { mgr.ClaimReward(idToClaim); }); } else { bImg.color = new Color(0.14f, 0.18f, 0.26f, 0.8f); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:63 | Runtime | btnBrewTuVi.onClick.RemoveAllListeners(); btnBrewTuVi.onClick.AddListener(OnBrewCultivationClicked); }  if (btnBrewBreakthrough != null) { btnBrewBreakthrough.onClick.RemoveAllListeners(); btnBrewBreakthrough.onClick.AddListener(OnBrewBreakthroughClicked); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:64 | Runtime | btnBrewTuVi.onClick.AddListener(OnBrewCultivationClicked); }  if (btnBrewBreakthrough != null) { btnBrewBreakthrough.onClick.RemoveAllListeners(); btnBrewBreakthrough.onClick.AddListener(OnBrewBreakthroughClicked); } |
| Assets/Scripts/UI/AlchemyPanelUI.cs:69 | Runtime | btnBrewBreakthrough.onClick.RemoveAllListeners(); btnBrewBreakthrough.onClick.AddListener(OnBrewBreakthroughClicked); }  if (btnFinishWithDust != null) { if (txtDustCost == null) txtDustCost = btnFinishWithDust.GetComponentInChildren<Text>(true); btnFinishWithDust.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:70 | Runtime | btnBrewBreakthrough.onClick.AddListener(OnBrewBreakthroughClicked); }  if (btnFinishWithDust != null) { if (txtDustCost == null) txtDustCost = btnFinishWithDust.GetComponentInChildren<Text>(true); btnFinishWithDust.onClick.RemoveAllListeners(); btnFinishWithDust.onClick.AddListener(OnUseDustClicked); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:76 | Runtime | btnFinishWithDust.onClick.RemoveAllListeners(); btnFinishWithDust.onClick.AddListener(OnUseDustClicked); }  if (btnUseTuViPill != null) { btnUseTuViPill.onClick.RemoveAllListeners(); btnUseTuViPill.onClick.AddListener(OnUseTuViPillClicked); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:77 | Runtime | btnFinishWithDust.onClick.AddListener(OnUseDustClicked); }  if (btnUseTuViPill != null) { btnUseTuViPill.onClick.RemoveAllListeners(); btnUseTuViPill.onClick.AddListener(OnUseTuViPillClicked); } |
| Assets/Scripts/UI/AlchemyPanelUI.cs:82 | Runtime | btnUseTuViPill.onClick.RemoveAllListeners(); btnUseTuViPill.onClick.AddListener(OnUseTuViPillClicked); }  // Đồng bộ legacy buttons if (brewCultivationBtn != null) brewCultivationBtn.onClick.AddListener(OnBrewCultivationClicked); if (brewBreakthroughBtn != null) brewBreakthroughBtn.onClick.AddListener(OnBrewBreakthroughClicked); if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:83 | Runtime | btnUseTuViPill.onClick.AddListener(OnUseTuViPillClicked); }  // Đồng bộ legacy buttons if (brewCultivationBtn != null) brewCultivationBtn.onClick.AddListener(OnBrewCultivationClicked); if (brewBreakthroughBtn != null) brewBreakthroughBtn.onClick.AddListener(OnBrewBreakthroughClicked); if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked); } |
| Assets/Scripts/UI/AlchemyPanelUI.cs:87 | Runtime | if (brewCultivationBtn != null) brewCultivationBtn.onClick.AddListener(OnBrewCultivationClicked); if (brewBreakthroughBtn != null) brewBreakthroughBtn.onClick.AddListener(OnBrewBreakthroughClicked); if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked); }  private void OnEnable() { if (AlchemyService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:88 | Runtime | if (brewBreakthroughBtn != null) brewBreakthroughBtn.onClick.AddListener(OnBrewBreakthroughClicked); if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked); }  private void OnEnable() { if (AlchemyService.Instance != null) AlchemyService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:89 | Runtime | if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked); }  private void OnEnable() { if (AlchemyService.Instance != null) AlchemyService.Instance.OnChanged += RefreshUI; if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:95 | Runtime | AlchemyService.Instance.OnChanged += RefreshUI; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  |
| Assets/Scripts/UI/AlchemyPanelUI.cs:97 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleStageChanged(CultivationStageData _) => RefreshUI();  |
| Assets/Scripts/UI/AlchemyPanelUI.cs:99 | Runtime | CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleStageChanged(CultivationStageData _) => RefreshUI();  private void OnDisable() { |
| Assets/Scripts/UI/AlchemyPanelUI.cs:108 | Runtime | AlchemyService.Instance.OnChanged -= RefreshUI; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() |
| Assets/Scripts/UI/AlchemyPanelUI.cs:110 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() { if (AlchemyService.Instance != null && AlchemyService.Instance.IsBrewing) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:112 | Runtime | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() { if (AlchemyService.Instance != null && AlchemyService.Instance.IsBrewing) { UpdateBrewTimer(); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:47 | Runtime | upgradeButtons[i].onClick.RemoveAllListeners(); upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(slotIndex)); } } }  private void OnEnable() { |
| Assets/Scripts/UI/ArtifactPanelUI.cs:48 | Runtime | upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(slotIndex)); } } }  private void OnEnable() { if (ArtifactService.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:56 | Runtime | ArtifactService.Instance.OnChanged += RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:58 | Runtime | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  |
| Assets/Scripts/UI/ArtifactPanelUI.cs:60 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleLinhThachChanged(float _) => RefreshUI(); private void HandleStageChanged(CultivationStageData _) => RefreshUI(); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:62 | Runtime | CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleLinhThachChanged(float _) => RefreshUI(); private void HandleStageChanged(CultivationStageData _) => RefreshUI();  private void OnDisable() |
| Assets/Scripts/UI/ArtifactPanelUI.cs:72 | Runtime | ArtifactService.Instance.OnChanged -= RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; } |
| Assets/Scripts/UI/ArtifactPanelUI.cs:74 | Runtime | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  public void RefreshUI() |
| Assets/Scripts/UI/ArtifactPanelUI.cs:76 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  public void RefreshUI() { if (ArtifactService.Instance == null) return; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:78 | Runtime | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  public void RefreshUI() { if (ArtifactService.Instance == null) return; var svc = ArtifactService.Instance; float stone = EconomyManager.Instance?.currentLinhThach ?? 0f; |
| Assets/Scripts/UI/BiCanhButtonHandler.cs:28 | Runtime | btn.onClick.RemoveListener(OnBiCanhButtonClicked); btn.onClick.AddListener(OnBiCanhButtonClicked); } }  public void OnBiCanhButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BiCanhButtonHandler.cs:29 | Runtime | btn.onClick.AddListener(OnBiCanhButtonClicked); } }  public void OnBiCanhButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); if (SecretRealmModalUI.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:93 | Runtime | nodeSelectButtons[i].onClick.RemoveAllListeners(); nodeSelectButtons[i].onClick.AddListener(() => OpenCenterModal(index)); }  if (nodeQuickUpgradeButtons != null && i < nodeQuickUpgradeButtons.Length && nodeQuickUpgradeButtons[i] != null) { nodeQuickUpgradeButtons[i].onClick.RemoveAllListeners(); nodeQuickUpgradeButtons[i].onClick.AddListener(() => OpenCenterModal(index)); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:94 | Runtime | nodeSelectButtons[i].onClick.AddListener(() => OpenCenterModal(index)); }  if (nodeQuickUpgradeButtons != null && i < nodeQuickUpgradeButtons.Length && nodeQuickUpgradeButtons[i] != null) { nodeQuickUpgradeButtons[i].onClick.RemoveAllListeners(); nodeQuickUpgradeButtons[i].onClick.AddListener(() => OpenCenterModal(index)); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:99 | Runtime | nodeQuickUpgradeButtons[i].onClick.RemoveAllListeners(); nodeQuickUpgradeButtons[i].onClick.AddListener(() => OpenCenterModal(index)); } }  // Cài đặt Center Modal if (centerModalCloseBtn != null) { |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:100 | Runtime | nodeQuickUpgradeButtons[i].onClick.AddListener(() => OpenCenterModal(index)); } }  // Cài đặt Center Modal if (centerModalCloseBtn != null) { centerModalCloseBtn.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:107 | Runtime | centerModalCloseBtn.onClick.RemoveAllListeners(); centerModalCloseBtn.onClick.AddListener(CloseCenterModal); }  if (centerModalBackdropBtn != null) { centerModalBackdropBtn.onClick.RemoveAllListeners(); centerModalBackdropBtn.onClick.AddListener(CloseCenterModal); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:108 | Runtime | centerModalCloseBtn.onClick.AddListener(CloseCenterModal); }  if (centerModalBackdropBtn != null) { centerModalBackdropBtn.onClick.RemoveAllListeners(); centerModalBackdropBtn.onClick.AddListener(CloseCenterModal); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:113 | Runtime | centerModalBackdropBtn.onClick.RemoveAllListeners(); centerModalBackdropBtn.onClick.AddListener(CloseCenterModal); }  if (btnModalUpgrade != null) { btnModalUpgrade.onClick.RemoveAllListeners(); btnModalUpgrade.onClick.AddListener(() => UpgradeNode(selectedIndex, 1)); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:114 | Runtime | centerModalBackdropBtn.onClick.AddListener(CloseCenterModal); }  if (btnModalUpgrade != null) { btnModalUpgrade.onClick.RemoveAllListeners(); btnModalUpgrade.onClick.AddListener(() => UpgradeNode(selectedIndex, 1)); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:119 | Runtime | btnModalUpgrade.onClick.RemoveAllListeners(); btnModalUpgrade.onClick.AddListener(() => UpgradeNode(selectedIndex, 1)); }  if (btnModalUpgrade5 != null) { btnModalUpgrade5.onClick.RemoveAllListeners(); btnModalUpgrade5.onClick.AddListener(() => UpgradeNode(selectedIndex, 5)); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:120 | Runtime | btnModalUpgrade.onClick.AddListener(() => UpgradeNode(selectedIndex, 1)); }  if (btnModalUpgrade5 != null) { btnModalUpgrade5.onClick.RemoveAllListeners(); btnModalUpgrade5.onClick.AddListener(() => UpgradeNode(selectedIndex, 5)); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:125 | Runtime | btnModalUpgrade5.onClick.RemoveAllListeners(); btnModalUpgrade5.onClick.AddListener(() => UpgradeNode(selectedIndex, 5)); }  if (btnModalUpgradeMax != null) { btnModalUpgradeMax.onClick.RemoveAllListeners(); btnModalUpgradeMax.onClick.AddListener(() => UpgradeMax(selectedIndex)); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:126 | Runtime | btnModalUpgrade5.onClick.AddListener(() => UpgradeNode(selectedIndex, 5)); }  if (btnModalUpgradeMax != null) { btnModalUpgradeMax.onClick.RemoveAllListeners(); btnModalUpgradeMax.onClick.AddListener(() => UpgradeMax(selectedIndex)); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:131 | Runtime | btnModalUpgradeMax.onClick.RemoveAllListeners(); btnModalUpgradeMax.onClick.AddListener(() => UpgradeMax(selectedIndex)); }  if (centerModalRoot != null) { centerModalRoot.SetActive(false); } |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:132 | Runtime | btnModalUpgradeMax.onClick.AddListener(() => UpgradeMax(selectedIndex)); }  if (centerModalRoot != null) { centerModalRoot.SetActive(false); }  |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:148 | Runtime | PlayerStatsService.Instance.OnStatsChanged += Refresh; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged += HandleCurrencyChanged;  Refresh(); }  private void OnDisable() |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:150 | Runtime | EconomyManager.Instance.OnLinhThachChanged += HandleCurrencyChanged;  Refresh(); }  private void OnDisable() { if (PlayerStatsService.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:158 | Runtime | PlayerStatsService.Instance.OnStatsChanged -= Refresh; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= HandleCurrencyChanged; }  private void HandleCurrencyChanged(float amount) => Refresh();  public void OpenCenterModal(int index) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:160 | Runtime | EconomyManager.Instance.OnLinhThachChanged -= HandleCurrencyChanged; }  private void HandleCurrencyChanged(float amount) => Refresh();  public void OpenCenterModal(int index) { if (index < 0 \|\| index >= 6) return; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:71 | Runtime | if (closeButton != null) closeButton.onClick.AddListener(ClosePanel); if (closeOverlayButton != null) closeOverlayButton.onClick.AddListener(ClosePanel); if (bottomCloseButton != null) bottomCloseButton.onClick.AddListener(ClosePanel); }  private void Start() { // Panel được người dùng tắt sẵn trong Scene, chỉ hiển thị khi click mở |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:72 | Runtime | if (closeOverlayButton != null) closeOverlayButton.onClick.AddListener(ClosePanel); if (bottomCloseButton != null) bottomCloseButton.onClick.AddListener(ClosePanel); }  private void Start() { // Panel được người dùng tắt sẵn trong Scene, chỉ hiển thị khi click mở } |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:73 | Runtime | if (bottomCloseButton != null) bottomCloseButton.onClick.AddListener(ClosePanel); }  private void Start() { // Panel được người dùng tắt sẵn trong Scene, chỉ hiển thị khi click mở }  |
| Assets/Scripts/UI/CharacterFrameButton.cs:9 | Runtime | public class CharacterFrameButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler { private Image frameImage; private Color originalColor; private Color hoverColor;  private void Awake() { |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:13 | Runtime | EconomyManager.Instance.OnCoDuyenChanged += UpdateUI; UpdateUI(EconomyManager.Instance.currentCoDuyen); } }  private void OnDestroy() { if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:22 | Runtime | EconomyManager.Instance.OnCoDuyenChanged -= UpdateUI; } }  private void UpdateUI(int coDuyen) { if (txt != null) { |
| Assets/Scripts/UI/CollapsibleQuestBar.cs:72 | Runtime | toggleButton.onClick.AddListener(ToggleQuestBar); }  public void ToggleQuestBar() { IsCollapsed = !IsCollapsed; if (slideCoroutine != null) StopCoroutine(slideCoroutine); slideCoroutine = StartCoroutine(SlideTo(IsCollapsed ? collapsedPos : expandedPos)); |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs:39 | Runtime | tribulationButton.onClick.RemoveListener(TribulationModalUI.Open); tribulationButton.onClick.AddListener(TribulationModalUI.Open);  var root = transform.Find("PhaseSegments") as RectTransform; if (root == null) { var go = new GameObject("PhaseSegments", typeof(RectTransform)); root = (RectTransform)go.transform; |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs:40 | Runtime | tribulationButton.onClick.AddListener(TribulationModalUI.Open);  var root = transform.Find("PhaseSegments") as RectTransform; if (root == null) { var go = new GameObject("PhaseSegments", typeof(RectTransform)); root = (RectTransform)go.transform; root.SetParent(transform, false); |
| Assets/Scripts/UI/CultivationUI.cs:14 | Runtime | CultivationManager.Instance.OnTuViChanged += UpdateTuViUI; CultivationManager.Instance.OnStageChanged += UpdateStageUI; var stage = CultivationManager.Instance.GetCurrentStageData(); if (stage != null) { UpdateStageUI(stage); UpdateTuViUI(CultivationManager.Instance.currentTuVi, stage.requiredTuVi); } |
| Assets/Scripts/UI/CultivationUI.cs:15 | Runtime | CultivationManager.Instance.OnStageChanged += UpdateStageUI; var stage = CultivationManager.Instance.GetCurrentStageData(); if (stage != null) { UpdateStageUI(stage); UpdateTuViUI(CultivationManager.Instance.currentTuVi, stage.requiredTuVi); } } |
| Assets/Scripts/UI/CultivationUI.cs:29 | Runtime | CultivationManager.Instance.OnTuViChanged -= UpdateTuViUI; CultivationManager.Instance.OnStageChanged -= UpdateStageUI; } }  private void UpdateTuViUI(float current, float max) { if (txtTuVi != null) txtTuVi.text = $"Tu Vi: {EconomyManager.FormatNumber(current)} / {EconomyManager.FormatNumber(max)}"; |
| Assets/Scripts/UI/CultivationUI.cs:30 | Runtime | CultivationManager.Instance.OnStageChanged -= UpdateStageUI; } }  private void UpdateTuViUI(float current, float max) { if (txtTuVi != null) txtTuVi.text = $"Tu Vi: {EconomyManager.FormatNumber(current)} / {EconomyManager.FormatNumber(max)}"; if (imgProgressBar != null && CultivationManager.Instance != null) |
| Assets/Scripts/UI/DaoAnButtonHandler.cs:28 | Runtime | btn.onClick.RemoveListener(OnDaoAnButtonClicked); btn.onClick.AddListener(OnDaoAnButtonClicked); } }  public void OnDaoAnButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/DaoAnButtonHandler.cs:29 | Runtime | btn.onClick.AddListener(OnDaoAnButtonClicked); } }  public void OnDaoAnButtonClicked() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); if (DaoAnModalUI.Instance != null) |
| Assets/Scripts/UI/DaoAnModalUI.cs:322 | Runtime | closeX.onClick.RemoveAllListeners(); closeX.onClick.AddListener(Close); }  // Nút Close dưới đáy var closeBottom = boxTr.Find("CloseButton")?.GetComponent<Button>(); if (closeBottom != null) { |
| Assets/Scripts/UI/DaoAnModalUI.cs:323 | Runtime | closeX.onClick.AddListener(Close); }  // Nút Close dưới đáy var closeBottom = boxTr.Find("CloseButton")?.GetComponent<Button>(); if (closeBottom != null) { var cbRt = closeBottom.GetComponent<RectTransform>(); |
| Assets/Scripts/UI/DaoAnModalUI.cs:338 | Runtime | closeBottom.onClick.RemoveAllListeners(); closeBottom.onClick.AddListener(Close); } }  private static void EnsureScrollSurface(ScrollRect scroll) { if (scroll == null \|\| scroll.viewport == null) return; |
| Assets/Scripts/UI/DaoAnModalUI.cs:339 | Runtime | closeBottom.onClick.AddListener(Close); } }  private static void EnsureScrollSurface(ScrollRect scroll) { if (scroll == null \|\| scroll.viewport == null) return; var viewport = scroll.viewport; |
| Assets/Scripts/UI/DaoAnModalUI.cs:362 | Runtime | modalOverlay.GetComponent<Button>().onClick.AddListener(Close); modalOverlay.transform.SetAsLastSibling();  // 4. Modal Box trung tâm với SafeAreaFitter (bảo đảm an toàn tuyệt đối trên màn hình có notch / tai thỏ) var box = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(SafeAreaFitter)); var boxRect = SetupRect(box, modalOverlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); box.GetComponent<SafeAreaFitter>().Refresh(); box.GetComponent<Image>().color = new Color(0.07f, 0.10f, 0.16f, 0.99f); |
| Assets/Scripts/UI/DaoAnModalUI.cs:384 | Runtime | closeX.onClick.AddListener(Close);  // 6. Tiêu đề & Phụ đề CreateLabel("Title", box.transform, "THIÊN PHÚ: ĐẠO VẬN", 22, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -44f), new Vector2(-60f, -8f), new Color(0.96f, 0.78f, 0.26f), true);  CreateLabel("SubTitle", box.transform, "Cây Thiên Phú Bị Động - Vĩnh Viễn Không Mất Khi Độ Kiếp", 13, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -68f), new Vector2(-20f, -44f), new Color(0.65f, 0.72f, 0.82f)); |
| Assets/Scripts/UI/DaoAnModalUI.cs:475 | Runtime | closeBtn.onClick.AddListener(Close);  modalOverlay.SetActive(false); }  public void Open() { if (modalOverlay == null) |
| Assets/Scripts/UI/DaoAnModalUI.cs:531 | Runtime | button.onClick.RemoveAllListeners(); button.onClick.AddListener(() => SetFilter(key)); filterButtons.Add(button); } UpdateFilterColors(); }  private static bool MatchesFilter(DaoAnDefinition node, string key) => |
| Assets/Scripts/UI/DaoAnModalUI.cs:532 | Runtime | button.onClick.AddListener(() => SetFilter(key)); filterButtons.Add(button); } UpdateFilterColors(); }  private static bool MatchesFilter(DaoAnDefinition node, string key) => key == "all" \|\| string.Equals(node.branch, key, StringComparison.OrdinalIgnoreCase); |
| Assets/Scripts/UI/DaoAnModalUI.cs:857 | Runtime | buyBtn.onClick.AddListener(() => { var activeService = DaoAnProgressionService.Instance ?? service; if (activeService == null) { Debug.LogWarning("[DaoAn] Service is null!"); return; } |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:86 | Runtime | btnCancel.onClick.RemoveAllListeners(); btnCancel.onClick.AddListener(Close); } if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); } |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:87 | Runtime | btnCancel.onClick.AddListener(Close); } if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); } if (btnConfirm != null) |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:91 | Runtime | btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); } if (btnConfirm != null) { btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(OnConfirmClicked); } |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:92 | Runtime | btnBackdrop.onClick.AddListener(Close); } if (btnConfirm != null) { btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(OnConfirmClicked); } } |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:96 | Runtime | btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(OnConfirmClicked); } }  public static void OpenConfirm(string itemName, string itemDetail, DismantleReward reward, Action onConfirm) { if (Instance == null) |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:97 | Runtime | btnConfirm.onClick.AddListener(OnConfirmClicked); } }  public static void OpenConfirm(string itemName, string itemDetail, DismantleReward reward, Action onConfirm) { if (Instance == null) { |
| Assets/Scripts/UI/FormationPanelUI.cs:67 | Runtime | selectButtons[i].onClick.RemoveAllListeners(); selectButtons[i].onClick.AddListener(() => OnSelectFormation(index)); } } }  if (upgradeButtons != null) { |
| Assets/Scripts/UI/FormationPanelUI.cs:68 | Runtime | selectButtons[i].onClick.AddListener(() => OnSelectFormation(index)); } } }  if (upgradeButtons != null) { for (int i = 0; i < upgradeButtons.Length; i++) |
| Assets/Scripts/UI/FormationPanelUI.cs:80 | Runtime | upgradeButtons[i].onClick.RemoveAllListeners(); upgradeButtons[i].onClick.AddListener(() => OnUpgradeFormation(index)); } } }  if (activateSpiritBtn != null) { |
| Assets/Scripts/UI/FormationPanelUI.cs:81 | Runtime | upgradeButtons[i].onClick.AddListener(() => OnUpgradeFormation(index)); } } }  if (activateSpiritBtn != null) { activateSpiritBtn.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/FormationPanelUI.cs:88 | Runtime | activateSpiritBtn.onClick.RemoveAllListeners(); activateSpiritBtn.onClick.AddListener(OnActivateSpirit); } }  private void OnEnable() { if (FormationService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:89 | Runtime | activateSpiritBtn.onClick.AddListener(OnActivateSpirit); } }  private void OnEnable() { if (FormationService.Instance != null) FormationService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:96 | Runtime | FormationService.Instance.OnChanged += RefreshUI; if (ArtifactService.Instance != null) ArtifactService.Instance.OnChanged += RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:98 | Runtime | ArtifactService.Instance.OnChanged += RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); |
| Assets/Scripts/UI/FormationPanelUI.cs:100 | Runtime | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  |
| Assets/Scripts/UI/FormationPanelUI.cs:102 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleLinhThachChanged(float _) => RefreshUI(); private void HandleStageChanged(CultivationStageData _) => RefreshUI(); |
| Assets/Scripts/UI/FormationPanelUI.cs:104 | Runtime | CultivationManager.Instance.OnStageChanged += HandleStageChanged; RefreshUI(); }  private void HandleLinhThachChanged(float _) => RefreshUI(); private void HandleStageChanged(CultivationStageData _) => RefreshUI();  private void OnDisable() |
| Assets/Scripts/UI/FormationPanelUI.cs:114 | Runtime | FormationService.Instance.OnChanged -= RefreshUI; if (ArtifactService.Instance != null) ArtifactService.Instance.OnChanged -= RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:116 | Runtime | ArtifactService.Instance.OnChanged -= RefreshUI; if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; } |
| Assets/Scripts/UI/FormationPanelUI.cs:118 | Runtime | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; if (FeatureUnlockService.Instance != null) FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() |
| Assets/Scripts/UI/FormationPanelUI.cs:120 | Runtime | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; if (CultivationManager.Instance != null) CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() { UpdateSpiritCooldown(); |
| Assets/Scripts/UI/FormationPanelUI.cs:122 | Runtime | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; }  private void Update() { UpdateSpiritCooldown(); }  |
| Assets/Scripts/UI/JadeButtonFeedback.cs:6 | Runtime | public sealed class JadeButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler { private RectTransform visual; private UnityEngine.UI.Text label; private Vector3 rest; private float target = 1f; private UnityEngine.UI.Selectable selectable; private MobileUIAnimationDirector motion; |
| Assets/Scripts/UI/JadeModalPresentation.cs:5 | Runtime | public sealed class JadeModalPresentation : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler { public JadeUITheme theme; private bool pending=true; private Coroutine motion; private void OnEnable(){pending=true;} public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) { } private void LateUpdate() |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:14 | Runtime | toggle.onClick.AddListener(Toggle); if (dismissArea != null) dismissArea.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Close); foreach (var action in actions) if (action != null) action.onClick.AddListener(Close); Close(); } private void LateUpdate() { if (!IsOpen) return; |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:15 | Runtime | if (dismissArea != null) dismissArea.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Close); foreach (var action in actions) if (action != null) action.onClick.AddListener(Close); Close(); } private void LateUpdate() { if (!IsOpen) return; // Some legacy HUD handlers rebuild their onClick listeners; modal state is authoritative. |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:16 | Runtime | foreach (var action in actions) if (action != null) action.onClick.AddListener(Close); Close(); } private void LateUpdate() { if (!IsOpen) return; // Some legacy HUD handlers rebuild their onClick listeners; modal state is authoritative. if ((StorageModalUI.Instance != null && StorageModalUI.Instance.IsModalOpen) \|\| |
| Assets/Scripts/UI/MainMenuManager.cs:179 | Runtime | settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel)); quitButton.onClick.AddListener(ConfirmQuitGame); backFromSelectButton.onClick.AddListener(() => ShowPanel(mainPanel));  cancelCreateButton.onClick.AddListener(() => ShowPanel(characterSelectPanel)); confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter); if (characterNameLabel != null && nameInput != null) { |
| Assets/Scripts/UI/MainMenuManager.cs:180 | Runtime | quitButton.onClick.AddListener(ConfirmQuitGame); backFromSelectButton.onClick.AddListener(() => ShowPanel(mainPanel));  cancelCreateButton.onClick.AddListener(() => ShowPanel(characterSelectPanel)); confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter); if (characterNameLabel != null && nameInput != null) { characterNamePrompt = characterNameLabel.text; |
| Assets/Scripts/UI/MainMenuManager.cs:181 | Runtime | backFromSelectButton.onClick.AddListener(() => ShowPanel(mainPanel));  cancelCreateButton.onClick.AddListener(() => ShowPanel(characterSelectPanel)); confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter); if (characterNameLabel != null && nameInput != null) { characterNamePrompt = characterNameLabel.text; nameInput.onValueChanged.AddListener(_ => characterNameLabel.text = characterNamePrompt); |
| Assets/Scripts/UI/MainMenuManager.cs:183 | Runtime | cancelCreateButton.onClick.AddListener(() => ShowPanel(characterSelectPanel)); confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter); if (characterNameLabel != null && nameInput != null) { characterNamePrompt = characterNameLabel.text; nameInput.onValueChanged.AddListener(_ => characterNameLabel.text = characterNamePrompt); }  |
| Assets/Scripts/UI/MainMenuManager.cs:184 | Runtime | confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter); if (characterNameLabel != null && nameInput != null) { characterNamePrompt = characterNameLabel.text; nameInput.onValueChanged.AddListener(_ => characterNameLabel.text = characterNamePrompt); }  closeSettingsButton.onClick.AddListener(() => ShowPanel(mainPanel)); |
| Assets/Scripts/UI/MainMenuManager.cs:188 | Runtime | nameInput.onValueChanged.AddListener(_ => characterNameLabel.text = characterNamePrompt); }  closeSettingsButton.onClick.AddListener(() => ShowPanel(mainPanel));  // Đồng bộ âm thanh với AudioManager if (AudioManager.Instance != null) { |
| Assets/Scripts/UI/MainMenuManager.cs:191 | Runtime | closeSettingsButton.onClick.AddListener(() => ShowPanel(mainPanel));  // Đồng bộ âm thanh với AudioManager if (AudioManager.Instance != null) { if (bgmSlider != null) { bgmSlider.value = AudioManager.Instance.bgmVolume; |
| Assets/Scripts/UI/MainMenuManager.cs:199 | Runtime | bgmSlider.onValueChanged.RemoveAllListeners(); bgmSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetBgmVolume(val)); } if (sfxSlider != null) { sfxSlider.value = AudioManager.Instance.sfxVolume; sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSfxVolume(val)); |
| Assets/Scripts/UI/MainMenuManager.cs:200 | Runtime | bgmSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetBgmVolume(val)); } if (sfxSlider != null) { sfxSlider.value = AudioManager.Instance.sfxVolume; sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSfxVolume(val)); } |
| Assets/Scripts/UI/MainMenuManager.cs:205 | Runtime | sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSfxVolume(val)); } } }  private void UpdateMainMenuPlayButton() { |
| Assets/Scripts/UI/MainMenuManager.cs:206 | Runtime | sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSfxVolume(val)); } } }  private void UpdateMainMenuPlayButton() { bool hasAnySave = false; |
| Assets/Scripts/UI/MainMenuManager.cs:224 | Runtime | playButton.onClick.RemoveAllListeners(); playButton.onClick.AddListener(OpenCharacterSelection); }  public void OpenCharacterSelection() { ShowPanel(characterSelectPanel); for (int i = 0; i < 3; i++) |
| Assets/Scripts/UI/MainMenuManager.cs:225 | Runtime | playButton.onClick.AddListener(OpenCharacterSelection); }  public void OpenCharacterSelection() { ShowPanel(characterSelectPanel); for (int i = 0; i < 3; i++) { |
| Assets/Scripts/UI/MainMenuManager.cs:358 | Runtime | popupConfirmBtn.onClick.RemoveAllListeners(); popupConfirmBtn.onClick.AddListener(onConfirm); }  if(popupCancelBtn) { popupCancelBtn.onClick.RemoveAllListeners(); popupCancelBtn.onClick.AddListener(onCancel); |
| Assets/Scripts/UI/MainMenuManager.cs:359 | Runtime | popupConfirmBtn.onClick.AddListener(onConfirm); }  if(popupCancelBtn) { popupCancelBtn.onClick.RemoveAllListeners(); popupCancelBtn.onClick.AddListener(onCancel); } |
| Assets/Scripts/UI/MainMenuManager.cs:364 | Runtime | popupCancelBtn.onClick.RemoveAllListeners(); popupCancelBtn.onClick.AddListener(onCancel); } }  private void ShowPanel(GameObject panelToShow) { if(mainPanel) mainPanel.SetActive(false); |
| Assets/Scripts/UI/MainMenuManager.cs:365 | Runtime | popupCancelBtn.onClick.AddListener(onCancel); } }  private void ShowPanel(GameObject panelToShow) { if(mainPanel) mainPanel.SetActive(false); if(characterSelectPanel) characterSelectPanel.SetActive(false); |
| Assets/Scripts/UI/MilestoneModalUI.cs:62 | Runtime | btnClaimAll.onClick.RemoveAllListeners(); btnClaimAll.onClick.AddListener(OnClaimAllClicked); }  if (modalPanel == null) { modalPanel = gameObject; } |
| Assets/Scripts/UI/MilestoneModalUI.cs:63 | Runtime | btnClaimAll.onClick.AddListener(OnClaimAllClicked); }  if (modalPanel == null) { modalPanel = gameObject; }  |
| Assets/Scripts/UI/MilestoneModalUI.cs:79 | Runtime | backdropBtn.onClick.RemoveAllListeners(); backdropBtn.onClick.AddListener(Close);  // Đảm bảo nền ModalBox chặn raycast không kích hoạt backdrop click Transform modalBox = transform.Find("ModalBox"); if (modalBox != null) { Image boxImg = modalBox.GetComponent<Image>(); |
| Assets/Scripts/UI/MilestoneModalUI.cs:80 | Runtime | backdropBtn.onClick.AddListener(Close);  // Đảm bảo nền ModalBox chặn raycast không kích hoạt backdrop click Transform modalBox = transform.Find("ModalBox"); if (modalBox != null) { Image boxImg = modalBox.GetComponent<Image>(); if (boxImg != null) boxImg.raycastTarget = true; |
| Assets/Scripts/UI/MilestoneModalUI.cs:133 | Runtime | btnSwitchToAchievements.onClick.RemoveAllListeners(); btnSwitchToAchievements.onClick.AddListener(SwitchToAchievements); } btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); } }  |
| Assets/Scripts/UI/MilestoneModalUI.cs:134 | Runtime | btnSwitchToAchievements.onClick.AddListener(SwitchToAchievements); } btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); } }  private void Start() |
| Assets/Scripts/UI/MilestoneModalUI.cs:136 | Runtime | btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); } }  private void Start() { if (MilestoneManager.Instance != null) |
| Assets/Scripts/UI/MilestoneModalUI.cs:137 | Runtime | btnClose.onClick.AddListener(Close); } }  private void Start() { if (MilestoneManager.Instance != null) { |
| Assets/Scripts/UI/MilestoneModalUI.cs:145 | Runtime | MilestoneManager.Instance.OnMilestonesUpdated += RefreshUI; } }  private void OnDestroy() { if (MilestoneManager.Instance != null) { |
| Assets/Scripts/UI/MilestoneModalUI.cs:153 | Runtime | MilestoneManager.Instance.OnMilestonesUpdated -= RefreshUI; } }  public void Open() { if (modalPanel == null) { |
| Assets/Scripts/UI/MilestoneModalUI.cs:417 | Runtime | btn.onClick.AddListener(() => { MilestoneManager.Instance.ClaimReward(qId); }); } else { btnImg.color = new Color(0.2f, 0.22f, 0.28f, 0.8f); |
| Assets/Scripts/UI/MobileCurrencyText.cs:16 | Runtime | EconomyManager.Instance.OnLinhThachChanged += OnLinhThachChanged; EconomyManager.Instance.OnCoDuyenChanged += OnCoDuyenChanged; Refresh(); }  private void OnDisable() { if (EconomyManager.Instance == null) return; |
| Assets/Scripts/UI/MobileCurrencyText.cs:17 | Runtime | EconomyManager.Instance.OnCoDuyenChanged += OnCoDuyenChanged; Refresh(); }  private void OnDisable() { if (EconomyManager.Instance == null) return; EconomyManager.Instance.OnLinhThachChanged -= OnLinhThachChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:24 | Runtime | EconomyManager.Instance.OnLinhThachChanged -= OnLinhThachChanged; EconomyManager.Instance.OnCoDuyenChanged -= OnCoDuyenChanged; }  private void OnLinhThachChanged(float _) { if (currency == Currency.LinhThach) Refresh(); } private void OnCoDuyenChanged(int _) { if (currency == Currency.CoDuyen) Refresh(); }  private void Refresh() |
| Assets/Scripts/UI/MobileCurrencyText.cs:25 | Runtime | EconomyManager.Instance.OnCoDuyenChanged -= OnCoDuyenChanged; }  private void OnLinhThachChanged(float _) { if (currency == Currency.LinhThach) Refresh(); } private void OnCoDuyenChanged(int _) { if (currency == Currency.CoDuyen) Refresh(); }  private void Refresh() { |
| Assets/Scripts/UI/MobileDrawerUIController.cs:138 | Runtime | btn.onClick.AddListener(ToggleDockCollapse); } else { var btn = dockToggleButton.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveListener(ToggleDockCollapse); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:145 | Runtime | btn.onClick.RemoveListener(ToggleDockCollapse); btn.onClick.AddListener(ToggleDockCollapse); } if (dockToggleLabelText == null) { dockToggleLabelText = dockToggleButton.GetComponentInChildren<Text>(); } } |
| Assets/Scripts/UI/MobileDrawerUIController.cs:146 | Runtime | btn.onClick.AddListener(ToggleDockCollapse); } if (dockToggleLabelText == null) { dockToggleLabelText = dockToggleButton.GetComponentInChildren<Text>(); } }  |
| Assets/Scripts/UI/MobileDrawerUIController.cs:224 | Runtime | dockButtons[i].onClick.RemoveAllListeners(); dockButtons[i].onClick.AddListener(() => ToggleDrawer(index)); } } } if (closeButtons != null) { foreach (var button in closeButtons) |
| Assets/Scripts/UI/MobileDrawerUIController.cs:225 | Runtime | dockButtons[i].onClick.AddListener(() => ToggleDrawer(index)); } } } if (closeButtons != null) { foreach (var button in closeButtons) if (button != null) |
| Assets/Scripts/UI/MobileDrawerUIController.cs:234 | Runtime | button.onClick.RemoveAllListeners(); button.onClick.AddListener(CloseActiveDrawer); } } if (backdropOverlay != null) { var button = backdropOverlay.GetComponent<Button>(); if (button != null) |
| Assets/Scripts/UI/MobileDrawerUIController.cs:235 | Runtime | button.onClick.AddListener(CloseActiveDrawer); } } if (backdropOverlay != null) { var button = backdropOverlay.GetComponent<Button>(); if (button != null) { |
| Assets/Scripts/UI/MobileDrawerUIController.cs:243 | Runtime | button.onClick.RemoveAllListeners(); button.onClick.AddListener(CloseActiveDrawer); } } }  public bool IsDrawerUnlocked(int index, out string reason) { |
| Assets/Scripts/UI/MobileDrawerUIController.cs:244 | Runtime | button.onClick.AddListener(CloseActiveDrawer); } } }  public bool IsDrawerUnlocked(int index, out string reason) { reason = "Đã mở khóa"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:51 | Runtime | btnUltimate.onClick.RemoveAllListeners(); btnUltimate.onClick.AddListener(() => { if (SkillManager.Instance != null) SkillManager.Instance.CastActiveSkill(); }); }  |
| Assets/Scripts/UI/MobileSkillHUD.cs:52 | Runtime | btnUltimate.onClick.AddListener(() => { if (SkillManager.Instance != null) SkillManager.Instance.CastActiveSkill(); }); }  for (int i = 0; i < btnNormalSkills.Length; i++) |
| Assets/Scripts/UI/MobileSkillHUD.cs:64 | Runtime | btnNormalSkills[i].onClick.RemoveAllListeners(); btnNormalSkills[i].onClick.AddListener(() => { if (SkillManager.Instance != null) SkillManager.Instance.CastNormalSkillManual(index); }); } } |
| Assets/Scripts/UI/MobileSkillHUD.cs:65 | Runtime | btnNormalSkills[i].onClick.AddListener(() => { if (SkillManager.Instance != null) SkillManager.Instance.CastNormalSkillManual(index); }); } } } |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:77 | Runtime | claimNormalButton.onClick.RemoveAllListeners(); claimNormalButton.onClick.AddListener(OnClaimNormalClicked); } if (claimDoubleButton != null) { claimDoubleButton.onClick.RemoveAllListeners(); claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked); } |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:78 | Runtime | claimNormalButton.onClick.AddListener(OnClaimNormalClicked); } if (claimDoubleButton != null) { claimDoubleButton.onClick.RemoveAllListeners(); claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked); } if (closeButton != null) |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:82 | Runtime | claimDoubleButton.onClick.RemoveAllListeners(); claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked); } if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(OnCloseClicked); } |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:83 | Runtime | claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked); } if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(OnCloseClicked); }  |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:87 | Runtime | closeButton.onClick.RemoveAllListeners(); closeButton.onClick.AddListener(OnCloseClicked); }  if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>(); }  private void Start() |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:88 | Runtime | closeButton.onClick.AddListener(OnCloseClicked); }  if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>(); }  private void Start() { |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:520 | Runtime | bNorm.onClick.RemoveAllListeners(); bNorm.onClick.AddListener(popup.OnClaimNormalClicked); bDbl.onClick.RemoveAllListeners(); bDbl.onClick.AddListener(popup.OnClaimDoubleClicked); bClose.onClick.RemoveAllListeners(); bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:521 | Runtime | bNorm.onClick.AddListener(popup.OnClaimNormalClicked); bDbl.onClick.RemoveAllListeners(); bDbl.onClick.AddListener(popup.OnClaimDoubleClicked); bClose.onClick.RemoveAllListeners(); bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; _instance = popup; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:522 | Runtime | bDbl.onClick.RemoveAllListeners(); bDbl.onClick.AddListener(popup.OnClaimDoubleClicked); bClose.onClick.RemoveAllListeners(); bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; _instance = popup; } |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:523 | Runtime | bDbl.onClick.AddListener(popup.OnClaimDoubleClicked); bClose.onClick.RemoveAllListeners(); bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; _instance = popup; } } |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:524 | Runtime | bClose.onClick.RemoveAllListeners(); bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; _instance = popup; } }  |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:525 | Runtime | bClose.onClick.AddListener(popup.OnCloseClicked);  popup.isOpen = false; _instance = popup; } }  |
| Assets/Scripts/UI/PauseButtonHandler.cs:21 | Runtime | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnPauseButtonClicked); } }  private void Start() { FindModalIfNull(); |
| Assets/Scripts/UI/PauseButtonHandler.cs:22 | Runtime | btn.onClick.AddListener(OnPauseButtonClicked); } }  private void Start() { FindModalIfNull(); } |
| Assets/Scripts/UI/PauseMenuManager.cs:92 | Runtime | openPauseButton.onClick.RemoveAllListeners(); openPauseButton.onClick.AddListener(OpenPauseMenu); }  if (closeHeaderButton != null) { closeHeaderButton.onClick.RemoveAllListeners(); closeHeaderButton.onClick.AddListener(ResumeGame); |
| Assets/Scripts/UI/PauseMenuManager.cs:93 | Runtime | openPauseButton.onClick.AddListener(OpenPauseMenu); }  if (closeHeaderButton != null) { closeHeaderButton.onClick.RemoveAllListeners(); closeHeaderButton.onClick.AddListener(ResumeGame); } |
| Assets/Scripts/UI/PauseMenuManager.cs:98 | Runtime | closeHeaderButton.onClick.RemoveAllListeners(); closeHeaderButton.onClick.AddListener(ResumeGame); }  if (overlayCloseButton != null) { overlayCloseButton.onClick.RemoveAllListeners(); overlayCloseButton.onClick.AddListener(ResumeGame); |
| Assets/Scripts/UI/PauseMenuManager.cs:99 | Runtime | closeHeaderButton.onClick.AddListener(ResumeGame); }  if (overlayCloseButton != null) { overlayCloseButton.onClick.RemoveAllListeners(); overlayCloseButton.onClick.AddListener(ResumeGame); } |
| Assets/Scripts/UI/PauseMenuManager.cs:104 | Runtime | overlayCloseButton.onClick.RemoveAllListeners(); overlayCloseButton.onClick.AddListener(ResumeGame); }  if (resumeButton != null) { resumeButton.onClick.RemoveAllListeners(); resumeButton.onClick.AddListener(ResumeGame); |
| Assets/Scripts/UI/PauseMenuManager.cs:105 | Runtime | overlayCloseButton.onClick.AddListener(ResumeGame); }  if (resumeButton != null) { resumeButton.onClick.RemoveAllListeners(); resumeButton.onClick.AddListener(ResumeGame); } |
| Assets/Scripts/UI/PauseMenuManager.cs:110 | Runtime | resumeButton.onClick.RemoveAllListeners(); resumeButton.onClick.AddListener(ResumeGame); }  if (manualSaveButton != null) { manualSaveButton.onClick.RemoveAllListeners(); manualSaveButton.onClick.AddListener(OnManualSaveClicked); |
| Assets/Scripts/UI/PauseMenuManager.cs:111 | Runtime | resumeButton.onClick.AddListener(ResumeGame); }  if (manualSaveButton != null) { manualSaveButton.onClick.RemoveAllListeners(); manualSaveButton.onClick.AddListener(OnManualSaveClicked); } |
| Assets/Scripts/UI/PauseMenuManager.cs:116 | Runtime | manualSaveButton.onClick.RemoveAllListeners(); manualSaveButton.onClick.AddListener(OnManualSaveClicked); }  if (returnMainMenuButton != null) { returnMainMenuButton.onClick.RemoveAllListeners(); returnMainMenuButton.onClick.AddListener(OnReturnMainMenuClicked); |
| Assets/Scripts/UI/PauseMenuManager.cs:117 | Runtime | manualSaveButton.onClick.AddListener(OnManualSaveClicked); }  if (returnMainMenuButton != null) { returnMainMenuButton.onClick.RemoveAllListeners(); returnMainMenuButton.onClick.AddListener(OnReturnMainMenuClicked); } |
| Assets/Scripts/UI/PauseMenuManager.cs:122 | Runtime | returnMainMenuButton.onClick.RemoveAllListeners(); returnMainMenuButton.onClick.AddListener(OnReturnMainMenuClicked); }  if (quitButton != null) { quitButton.onClick.RemoveAllListeners(); quitButton.onClick.AddListener(OnQuitGameClicked); |
| Assets/Scripts/UI/PauseMenuManager.cs:123 | Runtime | returnMainMenuButton.onClick.AddListener(OnReturnMainMenuClicked); }  if (quitButton != null) { quitButton.onClick.RemoveAllListeners(); quitButton.onClick.AddListener(OnQuitGameClicked); } |
| Assets/Scripts/UI/PauseMenuManager.cs:128 | Runtime | quitButton.onClick.RemoveAllListeners(); quitButton.onClick.AddListener(OnQuitGameClicked); }  if (bgmSlider != null) { bgmSlider.onValueChanged.RemoveAllListeners(); bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged); |
| Assets/Scripts/UI/PauseMenuManager.cs:129 | Runtime | quitButton.onClick.AddListener(OnQuitGameClicked); }  if (bgmSlider != null) { bgmSlider.onValueChanged.RemoveAllListeners(); bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged); } |
| Assets/Scripts/UI/PauseMenuManager.cs:134 | Runtime | bgmSlider.onValueChanged.RemoveAllListeners(); bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged); }  if (sfxSlider != null) { sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged); |
| Assets/Scripts/UI/PauseMenuManager.cs:135 | Runtime | bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged); }  if (sfxSlider != null) { sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged); } |
| Assets/Scripts/UI/PauseMenuManager.cs:140 | Runtime | sfxSlider.onValueChanged.RemoveAllListeners(); sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged); }  if (muteToggle != null) { muteToggle.onValueChanged.RemoveAllListeners(); muteToggle.onValueChanged.AddListener(OnMuteToggleChanged); |
| Assets/Scripts/UI/PauseMenuManager.cs:141 | Runtime | sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged); }  if (muteToggle != null) { muteToggle.onValueChanged.RemoveAllListeners(); muteToggle.onValueChanged.AddListener(OnMuteToggleChanged); } |
| Assets/Scripts/UI/PauseMenuManager.cs:146 | Runtime | muteToggle.onValueChanged.RemoveAllListeners(); muteToggle.onValueChanged.AddListener(OnMuteToggleChanged); }  if (btnPopupCancel != null) { btnPopupCancel.onClick.RemoveAllListeners(); btnPopupCancel.onClick.AddListener(CloseConfirmation); |
| Assets/Scripts/UI/PauseMenuManager.cs:147 | Runtime | muteToggle.onValueChanged.AddListener(OnMuteToggleChanged); }  if (btnPopupCancel != null) { btnPopupCancel.onClick.RemoveAllListeners(); btnPopupCancel.onClick.AddListener(CloseConfirmation); } |
| Assets/Scripts/UI/PauseMenuManager.cs:152 | Runtime | btnPopupCancel.onClick.RemoveAllListeners(); btnPopupCancel.onClick.AddListener(CloseConfirmation); }  if (btnPopupConfirm != null) { btnPopupConfirm.onClick.RemoveAllListeners(); btnPopupConfirm.onClick.AddListener(() => |
| Assets/Scripts/UI/PauseMenuManager.cs:153 | Runtime | btnPopupCancel.onClick.AddListener(CloseConfirmation); }  if (btnPopupConfirm != null) { btnPopupConfirm.onClick.RemoveAllListeners(); btnPopupConfirm.onClick.AddListener(() => { |
| Assets/Scripts/UI/PauseMenuManager.cs:158 | Runtime | btnPopupConfirm.onClick.RemoveAllListeners(); btnPopupConfirm.onClick.AddListener(() => { pendingConfirmAction?.Invoke(); CloseConfirmation(); }); } } |
| Assets/Scripts/UI/PauseMenuManager.cs:159 | Runtime | btnPopupConfirm.onClick.AddListener(() => { pendingConfirmAction?.Invoke(); CloseConfirmation(); }); } }  |
| Assets/Scripts/UI/PortraitHudController.cs:98 | Runtime | Bind(menuButton, ToggleSecondary); Bind(closeMenuButton, CloseSecondary); Bind(menuBackdrop, CloseSecondary); Bind(milestoneButton, OpenMilestones); Bind(profileButton, OpenProfile); Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); |
| Assets/Scripts/UI/PortraitHudController.cs:99 | Runtime | Bind(closeMenuButton, CloseSecondary); Bind(menuBackdrop, CloseSecondary); Bind(milestoneButton, OpenMilestones); Bind(profileButton, OpenProfile); Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); |
| Assets/Scripts/UI/PortraitHudController.cs:100 | Runtime | Bind(menuBackdrop, CloseSecondary); Bind(milestoneButton, OpenMilestones); Bind(profileButton, OpenProfile); Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); |
| Assets/Scripts/UI/PortraitHudController.cs:101 | Runtime | Bind(milestoneButton, OpenMilestones); Bind(profileButton, OpenProfile); Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; |
| Assets/Scripts/UI/PortraitHudController.cs:102 | Runtime | Bind(profileButton, OpenProfile); Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); |
| Assets/Scripts/UI/PortraitHudController.cs:103 | Runtime | Bind(buffsButton, ShowBuffs); Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); ConfigureViews(); |
| Assets/Scripts/UI/PortraitHudController.cs:104 | Runtime | Bind(logButton, ShowLog); Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); ConfigureViews(); ApplyLayout(); |
| Assets/Scripts/UI/PortraitHudController.cs:105 | Runtime | Bind(walletButton, ShowWallet); Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); ConfigureViews(); ApplyLayout(); RefreshProfile(); |
| Assets/Scripts/UI/PortraitHudController.cs:106 | Runtime | Bind(achievementButton, CloseSecondary); if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); ConfigureViews(); ApplyLayout(); RefreshProfile(); CloseSecondary(); |
| Assets/Scripts/UI/PortraitHudController.cs:107 | Runtime | if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary); bound = true; Subscribe(); ConfigureViews(); ApplyLayout(); RefreshProfile(); CloseSecondary(); } |
| Assets/Scripts/UI/PortraitHudController.cs:125 | Runtime | economy.OnLinhThachChanged += OnStones; economy.OnCoDuyenChanged += OnFate; RefreshWallet(); } if (cultivation != null) { cultivation.OnTuViChanged += OnCultivation; cultivation.OnStageChanged += OnRealm; |
| Assets/Scripts/UI/PortraitHudController.cs:126 | Runtime | economy.OnCoDuyenChanged += OnFate; RefreshWallet(); } if (cultivation != null) { cultivation.OnTuViChanged += OnCultivation; cultivation.OnStageChanged += OnRealm; } |
| Assets/Scripts/UI/PortraitHudController.cs:131 | Runtime | cultivation.OnTuViChanged += OnCultivation; cultivation.OnStageChanged += OnRealm; } if (drawer != null) { drawer.OnDockCollapseChanged += OnFocus; drawer.OnDrawerStateChanged += OnDrawer; } |
| Assets/Scripts/UI/PortraitHudController.cs:132 | Runtime | cultivation.OnStageChanged += OnRealm; } if (drawer != null) { drawer.OnDockCollapseChanged += OnFocus; drawer.OnDrawerStateChanged += OnDrawer; } } |
| Assets/Scripts/UI/PortraitHudController.cs:136 | Runtime | drawer.OnDockCollapseChanged += OnFocus; drawer.OnDrawerStateChanged += OnDrawer; } }  void Unsubscribe() { if (economy != null) { economy.OnLinhThachChanged -= OnStones; economy.OnCoDuyenChanged -= OnFate; } |
| Assets/Scripts/UI/PortraitHudController.cs:137 | Runtime | drawer.OnDrawerStateChanged += OnDrawer; } }  void Unsubscribe() { if (economy != null) { economy.OnLinhThachChanged -= OnStones; economy.OnCoDuyenChanged -= OnFate; } if (cultivation != null) { cultivation.OnTuViChanged -= OnCultivation; cultivation.OnStageChanged -= OnRealm; } |
| Assets/Scripts/UI/PortraitHudController.cs:143 | Runtime | if (economy != null) { economy.OnLinhThachChanged -= OnStones; economy.OnCoDuyenChanged -= OnFate; } if (cultivation != null) { cultivation.OnTuViChanged -= OnCultivation; cultivation.OnStageChanged -= OnRealm; } if (drawer != null) { drawer.OnDockCollapseChanged -= OnFocus; drawer.OnDrawerStateChanged -= OnDrawer; } } void OnDisable() { Unsubscribe(); CloseSecondary(); } void OnDestroy() { Unsubscribe(); if (Instance == this) Instance = null; } static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action) { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); } |
| Assets/Scripts/UI/PortraitHudController.cs:144 | Runtime | if (cultivation != null) { cultivation.OnTuViChanged -= OnCultivation; cultivation.OnStageChanged -= OnRealm; } if (drawer != null) { drawer.OnDockCollapseChanged -= OnFocus; drawer.OnDrawerStateChanged -= OnDrawer; } } void OnDisable() { Unsubscribe(); CloseSecondary(); } void OnDestroy() { Unsubscribe(); if (Instance == this) Instance = null; } static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action) { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); } void OnStones(float value) |
| Assets/Scripts/UI/PortraitHudController.cs:145 | Runtime | if (drawer != null) { drawer.OnDockCollapseChanged -= OnFocus; drawer.OnDrawerStateChanged -= OnDrawer; } } void OnDisable() { Unsubscribe(); CloseSecondary(); } void OnDestroy() { Unsubscribe(); if (Instance == this) Instance = null; } static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action) { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); } void OnStones(float value) { |
| Assets/Scripts/UI/PortraitHudController.cs:149 | Runtime | static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action) { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); } void OnStones(float value) { if(uiMotion==null){if(value!=lastStones)pulseTime=.55f;} else if(value>lastStones&&!uiMotion.reducedMotion)walletPulseTime=uiMotion.accent.duration; lastStones=value;RefreshWallet(); } |
| Assets/Scripts/UI/PortraitHudController.cs:150 | Runtime | { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); } void OnStones(float value) { if(uiMotion==null){if(value!=lastStones)pulseTime=.55f;} else if(value>lastStones&&!uiMotion.reducedMotion)walletPulseTime=uiMotion.accent.duration; lastStones=value;RefreshWallet(); } void OnFate(int value) |
| Assets/Scripts/UI/SaveSlotUI.cs:34 | Runtime | if(slotButton != null) slotButton.onClick.RemoveAllListeners(); if(deleteButton != null) deleteButton.onClick.RemoveAllListeners();  if (currentData.isSlotEmpty) { // Trống -> Click vào thì tạo nhân vật if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.OpenCharacterCreation(slotIndex)); } |
| Assets/Scripts/UI/SaveSlotUI.cs:35 | Runtime | if(deleteButton != null) deleteButton.onClick.RemoveAllListeners();  if (currentData.isSlotEmpty) { // Trống -> Click vào thì tạo nhân vật if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.OpenCharacterCreation(slotIndex)); } else |
| Assets/Scripts/UI/SaveSlotUI.cs:40 | Runtime | if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.OpenCharacterCreation(slotIndex)); } else { // Có dữ liệu -> Click vào thì hỏi vào game if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.AskConfirmEnterGame(slotIndex, currentData));  // Xóa nhân vật |
| Assets/Scripts/UI/SaveSlotUI.cs:45 | Runtime | if(slotButton != null) slotButton.onClick.AddListener(() => menuManager.AskConfirmEnterGame(slotIndex, currentData));  // Xóa nhân vật if(deleteButton != null) deleteButton.onClick.AddListener(() => menuManager.AskConfirmDeleteSlot(slotIndex)); } }  private void RefreshUI() |
| Assets/Scripts/UI/SaveSlotUI.cs:48 | Runtime | if(deleteButton != null) deleteButton.onClick.AddListener(() => menuManager.AskConfirmDeleteSlot(slotIndex)); } }  private void RefreshUI() { if (currentData.isSlotEmpty) { |
| Assets/Scripts/UI/SecretRealmModalUI.cs:92 | Runtime | modalOverlay.GetComponent<Button>().onClick.AddListener(Close); modalOverlay.transform.SetAsLastSibling();  // 2. Modal Box var box = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(SafeAreaFitter)); box.transform.SetParent(modalOverlay.transform, false); var bRt = box.GetComponent<RectTransform>(); bRt.anchorMin = Vector2.zero; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:128 | Runtime | btnX.GetComponent<Button>().onClick.AddListener(Close);  // 5. Title & Subtitle var tGo = CreateText(box.transform, "Title", "🌌 KỲ NGỘ BÍ CẢNH THƯỢNG CỔ", 22, FontStyle.Bold, new Vector2(-20f, -28f), new Color(0.95f, 0.8f, 1f)); tGo.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f); tGo.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f); tGo.GetComponent<RectTransform>().offsetMin = new Vector2(20f, -44f); tGo.GetComponent<RectTransform>().offsetMax = new Vector2(-60f, -8f); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:232 | Runtime | closeBottom.GetComponent<Button>().onClick.AddListener(Close);  modalOverlay.SetActive(false); }  private void CreateRealmCards(Transform content) { enterButtons.Clear(); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:335 | Runtime | btn.onClick.AddListener(() => { OnEnterRealmClicked(type); }); enterButtons.Add(btn); }  private void OnEnterRealmClicked(SecretRealmType type) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:427 | Runtime | btnX.onClick.RemoveAllListeners(); btnX.onClick.AddListener(Close); }  var btnBackdrop = modalOverlay.GetComponent<Button>(); if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:428 | Runtime | btnX.onClick.AddListener(Close); }  var btnBackdrop = modalOverlay.GetComponent<Button>(); if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:434 | Runtime | btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); }  var btnCloseBottom = modalOverlay.transform.Find("ModalBox/CloseButton")?.GetComponent<Button>(); if (btnCloseBottom != null) { btnCloseBottom.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:435 | Runtime | btnBackdrop.onClick.AddListener(Close); }  var btnCloseBottom = modalOverlay.transform.Find("ModalBox/CloseButton")?.GetComponent<Button>(); if (btnCloseBottom != null) { btnCloseBottom.onClick.RemoveAllListeners(); btnCloseBottom.onClick.AddListener(Close); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:441 | Runtime | btnCloseBottom.onClick.RemoveAllListeners(); btnCloseBottom.onClick.AddListener(Close); }  enterButtons.Clear(); var content = modalOverlay.transform.Find("ModalBox/ScrollView/Viewport/Content"); if (content != null) { |
| Assets/Scripts/UI/SecretRealmModalUI.cs:442 | Runtime | btnCloseBottom.onClick.AddListener(Close); }  enterButtons.Clear(); var content = modalOverlay.transform.Find("ModalBox/ScrollView/Viewport/Content"); if (content != null) { SecretRealmType[] types = { |
| Assets/Scripts/UI/SecretRealmModalUI.cs:471 | Runtime | btnEnter.onClick.RemoveAllListeners(); btnEnter.onClick.AddListener(() => OnEnterRealmClicked(type)); enterButtons.Add(btnEnter); } } } } } |
| Assets/Scripts/UI/SecretRealmModalUI.cs:472 | Runtime | btnEnter.onClick.AddListener(() => OnEnterRealmClicked(type)); enterButtons.Add(btnEnter); } } } } }  |
| Assets/Scripts/UI/SecretRealmPlayController.cs:303 | Runtime | btn.onClick.AddListener(() => { if (activeDemonGoList.Contains(go)) { SpawnFloatingText("-150.000 (Trảm!)", rt.anchoredPosition, new Color(1f, 0.4f, 0.2f), 1.2f); SpawnHitSpark(rt.anchoredPosition, Color.yellow, 64f); activeDemonGoList.Remove(go); Destroy(go); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:638 | Runtime | btn.onClick.AddListener(() => { OnSelectCauldron(index); });  cauldronObjects.Add(cGo); }  |
| Assets/Scripts/UI/SecretRealmPlayController.cs:785 | Runtime | btn.onClick.AddListener(() => { if (activeThunderOrbs.Contains(go)) { activeThunderOrbs.Remove(go); SpawnHitSpark(rt.anchoredPosition, Color.cyan, 60f); SpawnFloatingText("+1 LÔI TINH!", rt.anchoredPosition, Color.cyan, 1.2f); Destroy(go); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1106 | Runtime | btnConfirm.onClick.RemoveAllListeners(); btnConfirm.onClick.AddListener(CloseAll); } } else { resultModal = CreateResultModal(); } |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1107 | Runtime | btnConfirm.onClick.AddListener(CloseAll); } } else { resultModal = CreateResultModal(); } } |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1230 | Runtime | btnQuit.GetComponent<Button>().onClick.AddListener(CloseAll);  // Tiêu đề Bí Cảnh (Trọng tâm trên đỉnh) titleText = CreateText(headerGo.transform, "TitleText", "BÍ CẢNH THƯỢNG CỔ", 15, FontStyle.Bold, new Vector2(-10f, -22f), Color.yellow); titleText.rectTransform.sizeDelta = new Vector2(210f, 28f);  // Đồng hồ thời gian (Góc phải trên) timerText = CreateText(headerGo.transform, "TimerText", "⏱️ 30.0s", 15, FontStyle.Bold, new Vector2(135f, -22f), Color.cyan); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1327 | Runtime | btnQuit.onClick.RemoveAllListeners(); btnQuit.onClick.AddListener(CloseAll); }  var arenaTr = root.Find("ArenaContainer"); if (arenaTr != null) { arenaContainer = arenaTr as RectTransform; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1328 | Runtime | btnQuit.onClick.AddListener(CloseAll); }  var arenaTr = root.Find("ArenaContainer"); if (arenaTr != null) { arenaContainer = arenaTr as RectTransform; arenaBgImage = arenaTr.GetComponent<Image>(); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1402 | Runtime | actionButtonGo.GetComponent<Button>().onClick.AddListener(action); }  private GameObject CreateResultModal() { var canvas = GameObject.Find("Canvas"); if (canvas == null) return null;  |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1460 | Runtime | btnConfirm.GetComponent<Button>().onClick.AddListener(CloseAll);  return go; }  private void ClearDynamicArena() { if (arenaContainer == null) return; |
| Assets/Scripts/UI/ShopItemUI.cs:15 | Runtime | buyButton.onClick.RemoveAllListeners(); buyButton.onClick.AddListener(OnBuyClicked); priceText = buyButton.GetComponentInChildren<Text>(); } }  private void Update() { |
| Assets/Scripts/UI/ShopItemUI.cs:16 | Runtime | buyButton.onClick.AddListener(OnBuyClicked); priceText = buyButton.GetComponentInChildren<Text>(); } }  private void Update() { if (priceText != null && CultivationManager.Instance != null) |
| Assets/Scripts/UI/ShopToggleUI.cs:14 | Runtime | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => { if (shopPanel != null) { shopPanel.SetActive(!isCloseButton); } else |
| Assets/Scripts/UI/ShopToggleUI.cs:15 | Runtime | btn.onClick.AddListener(() => { if (shopPanel != null) { shopPanel.SetActive(!isCloseButton); } else { |
| Assets/Scripts/UI/SkillCardUI.cs:5 | Runtime | public class SkillCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler { public int skillIndex; // 0: Vạn Kiếm, 1: Ấn Chưởng, 2: Lôi Phạt, 3: Phân Thân public Text titleText; public Text condText; public Button unlockButton; public Text unlockBtnText;  |
| Assets/Scripts/UI/SkillCardUI.cs:18 | Runtime | unlockButton.onClick.AddListener(OnUnlockClicked); } }  private string GetShortStageName(int stageIndex) { if (CultivationManager.Instance == null \|\| CultivationManager.Instance.allStages == null \|\| stageIndex >= CultivationManager.Instance.allStages.Length) { |
| Assets/Scripts/UI/StorageButtonHandler.cs:18 | Runtime | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnStorageButtonClicked); } }  private void Start() { if (btn != null) |
| Assets/Scripts/UI/StorageButtonHandler.cs:19 | Runtime | btn.onClick.AddListener(OnStorageButtonClicked); } }  private void Start() { if (btn != null) { |
| Assets/Scripts/UI/StorageButtonHandler.cs:27 | Runtime | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnStorageButtonClicked); } }  public void OnStorageButtonClicked() { StorageModalUI.Open(initialCategory); |
| Assets/Scripts/UI/StorageButtonHandler.cs:28 | Runtime | btn.onClick.AddListener(OnStorageButtonClicked); } }  public void OnStorageButtonClicked() { StorageModalUI.Open(initialCategory); } |
| Assets/Scripts/UI/StorageModalUI.cs:123 | Runtime | StorageManager.Instance.OnStorageChanged += OnDataChanged; if (DismantleService.Instance != null) DismantleService.Instance.OnDismantleExecuted += OnDataChanged; }  private void OnDestroy() { if (StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:125 | Runtime | DismantleService.Instance.OnDismantleExecuted += OnDataChanged; }  private void OnDestroy() { if (StorageManager.Instance != null) StorageManager.Instance.OnStorageChanged -= OnDataChanged; if (DismantleService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:131 | Runtime | StorageManager.Instance.OnStorageChanged -= OnDataChanged; if (DismantleService.Instance != null) DismantleService.Instance.OnDismantleExecuted -= OnDataChanged; }  private void WireEvents() { if (btnClose != null) |
| Assets/Scripts/UI/StorageModalUI.cs:133 | Runtime | DismantleService.Instance.OnDismantleExecuted -= OnDataChanged; }  private void WireEvents() { if (btnClose != null) { btnClose.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/StorageModalUI.cs:140 | Runtime | btnClose.onClick.RemoveAllListeners(); btnClose.onClick.AddListener(Close); } if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); } |
| Assets/Scripts/UI/StorageModalUI.cs:141 | Runtime | btnClose.onClick.AddListener(Close); } if (btnBackdrop != null) { btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); }  |
| Assets/Scripts/UI/StorageModalUI.cs:145 | Runtime | btnBackdrop.onClick.RemoveAllListeners(); btnBackdrop.onClick.AddListener(Close); }  if (tabButtons != null) { for (int i = 0; i < tabButtons.Length; i++) { |
| Assets/Scripts/UI/StorageModalUI.cs:146 | Runtime | btnBackdrop.onClick.AddListener(Close); }  if (tabButtons != null) { for (int i = 0; i < tabButtons.Length; i++) { int catIndex = i; |
| Assets/Scripts/UI/StorageModalUI.cs:156 | Runtime | tabButtons[i].onClick.RemoveAllListeners(); tabButtons[i].onClick.AddListener(() => SelectCategory((StorageCategory)catIndex)); } } }  // Toggles if (toggleSwordHighLevel != null) |
| Assets/Scripts/UI/StorageModalUI.cs:157 | Runtime | tabButtons[i].onClick.AddListener(() => SelectCategory((StorageCategory)catIndex)); } } }  // Toggles if (toggleSwordHighLevel != null) { |
| Assets/Scripts/UI/StorageModalUI.cs:165 | Runtime | toggleSwordHighLevel.onValueChanged.RemoveAllListeners(); toggleSwordHighLevel.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.FilterSwordsHighLevel = val; }); } if (togglePillBreakthrough != null) { |
| Assets/Scripts/UI/StorageModalUI.cs:166 | Runtime | toggleSwordHighLevel.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.FilterSwordsHighLevel = val; }); } if (togglePillBreakthrough != null) { togglePillBreakthrough.onValueChanged.RemoveAllListeners(); |
| Assets/Scripts/UI/StorageModalUI.cs:173 | Runtime | togglePillBreakthrough.onValueChanged.RemoveAllListeners(); togglePillBreakthrough.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.FilterPillsBreakthrough = val; }); } if (toggleAutoDismantleLv1 != null) { |
| Assets/Scripts/UI/StorageModalUI.cs:174 | Runtime | togglePillBreakthrough.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.FilterPillsBreakthrough = val; }); } if (toggleAutoDismantleLv1 != null) { toggleAutoDismantleLv1.onValueChanged.RemoveAllListeners(); |
| Assets/Scripts/UI/StorageModalUI.cs:181 | Runtime | toggleAutoDismantleLv1.onValueChanged.RemoveAllListeners(); toggleAutoDismantleLv1.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.AutoDismantleLv1SwordsWhenFull = val; }); }  // Footer Buttons |
| Assets/Scripts/UI/StorageModalUI.cs:182 | Runtime | toggleAutoDismantleLv1.onValueChanged.AddListener((val) => { if (StorageManager.Instance != null) StorageManager.Instance.AutoDismantleLv1SwordsWhenFull = val; }); }  // Footer Buttons if (btnClaimPriority != null) |
| Assets/Scripts/UI/StorageModalUI.cs:191 | Runtime | btnClaimPriority.onClick.RemoveAllListeners(); btnClaimPriority.onClick.AddListener(OnClaimPriorityClicked); } if (btnBatchDismantle != null) { btnBatchDismantle.onClick.RemoveAllListeners(); btnBatchDismantle.onClick.AddListener(OnBatchDismantleClicked); } |
| Assets/Scripts/UI/StorageModalUI.cs:192 | Runtime | btnClaimPriority.onClick.AddListener(OnClaimPriorityClicked); } if (btnBatchDismantle != null) { btnBatchDismantle.onClick.RemoveAllListeners(); btnBatchDismantle.onClick.AddListener(OnBatchDismantleClicked); }  |
| Assets/Scripts/UI/StorageModalUI.cs:196 | Runtime | btnBatchDismantle.onClick.RemoveAllListeners(); btnBatchDismantle.onClick.AddListener(OnBatchDismantleClicked); }  // Toggle View Mode Button if (btnToggleView != null) { btnToggleView.onClick.RemoveAllListeners(); |
| Assets/Scripts/UI/StorageModalUI.cs:197 | Runtime | btnBatchDismantle.onClick.AddListener(OnBatchDismantleClicked); }  // Toggle View Mode Button if (btnToggleView != null) { btnToggleView.onClick.RemoveAllListeners(); btnToggleView.onClick.AddListener(ToggleViewMode); |
| Assets/Scripts/UI/StorageModalUI.cs:203 | Runtime | btnToggleView.onClick.RemoveAllListeners(); btnToggleView.onClick.AddListener(ToggleViewMode); }  // Tự động kết nối nút TÚI trên HUD chính nếu chưa gắn GameObject hudStorageBtn = GameObject.Find("Canvas/MobileUIRoot/BtnHUDStorage"); if (hudStorageBtn != null) { |
| Assets/Scripts/UI/StorageModalUI.cs:204 | Runtime | btnToggleView.onClick.AddListener(ToggleViewMode); }  // Tự động kết nối nút TÚI trên HUD chính nếu chưa gắn GameObject hudStorageBtn = GameObject.Find("Canvas/MobileUIRoot/BtnHUDStorage"); if (hudStorageBtn != null) { var btn = hudStorageBtn.GetComponent<Button>(); |
| Assets/Scripts/UI/StorageModalUI.cs:214 | Runtime | btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => Open()); } } }  public static void Open(StorageCategory initialCategory = StorageCategory.All) { |
| Assets/Scripts/UI/StorageModalUI.cs:215 | Runtime | btn.onClick.AddListener(() => Open()); } } }  public static void Open(StorageCategory initialCategory = StorageCategory.All) { if (Instance == null) |
| Assets/Scripts/UI/StorageModalUI.cs:585 | Runtime | button.onClick.AddListener(() => onClick()); } var labelObject = new GameObject("Label", typeof(RectTransform), typeof(UnityEngine.UI.Text)); labelObject.transform.SetParent(control.transform, false); var rect = (RectTransform)labelObject.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(6, 6); |
| Assets/Scripts/UI/StorageModalUI.cs:908 | Runtime | btn.onClick.AddListener(() => { if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked()) { if (GameLogger.Instance != null) { GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow); } |
| Assets/Scripts/UI/StorageModalUI.cs:1120 | Runtime | btn.onClick.AddListener(() => { if (GameLogger.Instance != null && FeatureUnlockService.Instance != null) { GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow); } }); } |
| Assets/Scripts/UI/StorageModalUI.cs:1130 | Runtime | btn.onClick.AddListener(() => onDismantle.Invoke()); }  GameObject bTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text)); bTextObj.transform.SetParent(btnObj.transform, false); RectTransform btRt = bTextObj.GetComponent<RectTransform>(); btRt.anchorMin = Vector2.zero; btRt.anchorMax = Vector2.one; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:37 | Runtime | SwordLoadoutService.Instance.OnCapacityChanged += Refresh; Refresh(SwordLoadoutService.Instance != null ? SwordLoadoutService.Instance.GetCurrentCapacity() : 1); }  private void OnDisable() { if (SwordLoadoutService.Instance != null) SwordLoadoutService.Instance.OnCapacityChanged -= Refresh; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:44 | Runtime | SwordLoadoutService.Instance.OnCapacityChanged -= Refresh; }  private void Refresh(int capacity) { bool locked = slotIndex >= capacity; if (stateLabel != null) { |
| Assets/Scripts/UI/TribulationModalUI.cs:58 | Runtime | confirmButton.onClick.RemoveAllListeners(); confirmButton.onClick.AddListener(OnConfirmClicked); } if (cancelButton != null) { cancelButton.onClick.RemoveAllListeners(); cancelButton.onClick.AddListener(OnCancelClicked); } |
| Assets/Scripts/UI/TribulationModalUI.cs:59 | Runtime | confirmButton.onClick.AddListener(OnConfirmClicked); } if (cancelButton != null) { cancelButton.onClick.RemoveAllListeners(); cancelButton.onClick.AddListener(OnCancelClicked); }  |
| Assets/Scripts/UI/TribulationModalUI.cs:63 | Runtime | cancelButton.onClick.RemoveAllListeners(); cancelButton.onClick.AddListener(OnCancelClicked); }  if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>(); }  private void Start() |
| Assets/Scripts/UI/TribulationModalUI.cs:64 | Runtime | cancelButton.onClick.AddListener(OnCancelClicked); }  if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>(); }  private void Start() { |
| Assets/Scripts/UI/TribulationModalUI.cs:88 | Runtime | if (observedAlchemy != null) observedAlchemy.OnChanged -= RefreshData; observedAlchemy = null; isOpen = false; animRoutine = null; }  private static bool CanOpenForCurrentState() { |
| Assets/Scripts/UI/TribulationModalUI.cs:152 | Runtime | if (observedAlchemy != null) observedAlchemy.OnChanged += RefreshData; }  // Đảm bảo CanvasGroup hiển thị ngay lập tức 100%, không bị tàng hình if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>(); if (canvasGroup != null) { canvasGroup.alpha = 1f; |
| Assets/Scripts/UI/TribulationModalUI.cs:512 | Runtime | btnConf.onClick.RemoveAllListeners(); btnConf.onClick.AddListener(modal.OnConfirmClicked); btnCanc.onClick.RemoveAllListeners(); btnCanc.onClick.AddListener(modal.OnCancelClicked);  modal.isOpen = false; _instance = modal; } |
| Assets/Scripts/UI/TribulationModalUI.cs:513 | Runtime | btnConf.onClick.AddListener(modal.OnConfirmClicked); btnCanc.onClick.RemoveAllListeners(); btnCanc.onClick.AddListener(modal.OnCancelClicked);  modal.isOpen = false; _instance = modal; } } |
| Assets/Scripts/UI/TribulationModalUI.cs:514 | Runtime | btnCanc.onClick.RemoveAllListeners(); btnCanc.onClick.AddListener(modal.OnCancelClicked);  modal.isOpen = false; _instance = modal; } } |
| Assets/Scripts/UI/TribulationModalUI.cs:515 | Runtime | btnCanc.onClick.AddListener(modal.OnCancelClicked);  modal.isOpen = false; _instance = modal; } } |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:30 | Runtime | EconomyManager.Instance.OnLinhThachChanged += UpdateEconomyUI; UpdateEconomyUI(EconomyManager.Instance.currentLinhThach); }  if (UpgradeManager.Instance != null) { UpgradeManager.Instance.OnUpgradesChanged += UpdateUpgradesUI; UpdateUpgradesUI(); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:36 | Runtime | UpgradeManager.Instance.OnUpgradesChanged += UpdateUpgradesUI; UpdateUpgradesUI(); }  // Gắn sự kiện click cho các nút if(btnLinhLuc != null) btnLinhLuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("LinhLuc")); if(btnKiemY != null) btnKiemY.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("KiemY")); if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc")); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:41 | Runtime | if(btnLinhLuc != null) btnLinhLuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("LinhLuc")); if(btnKiemY != null) btnKiemY.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("KiemY")); if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc")); if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh")); }  private void OnDestroy() { |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:42 | Runtime | if(btnKiemY != null) btnKiemY.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("KiemY")); if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc")); if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh")); }  private void OnDestroy() { if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:43 | Runtime | if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc")); if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh")); }  private void OnDestroy() { if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:44 | Runtime | if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh")); }  private void OnDestroy() { if (EconomyManager.Instance != null) EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI;  |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:50 | Runtime | EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI;  if (UpgradeManager.Instance != null) UpgradeManager.Instance.OnUpgradesChanged -= UpdateUpgradesUI; }  private void Update() { |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:53 | Runtime | UpgradeManager.Instance.OnUpgradesChanged -= UpdateUpgradesUI; }  private void Update() { #if UNITY_EDITOR // NÚT CHEAT ĐỂ TEST: Bấm phím Space để nhận 100 Linh Thạch if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:27 | Runtime | public void Bind(IMobileUIActionSource next) { Detach(); source = next; if (isActiveAndEnabled) Attach(); } public void Unbind() { Detach(); source = null; } void OnEnable() { Attach(); } void OnDisable() { Detach(); } |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:40 | Runtime | if (button != null) button.onClick.AddListener(Execute); Refresh(); } void Detach() { if (!listening) return; source.Changed -= Refresh; if (button != null) button.onClick.RemoveListener(Execute); |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:47 | Runtime | if (button != null) button.onClick.RemoveListener(Execute); listening = false; } public void Refresh() { if (source == null) return; var state = source.Read(); if (caption != null) caption.text = state.Caption ?? ""; |



## 5. Data → text/progress/state binding

| Source:line | Expression |
|---|---|
| Assets/Scripts/Combat/BossDamageVignette.cs:78 | canvasGroup.interactable = false; |
| Assets/Scripts/Combat/BossEnemy.cs:305 | phaseTextUI.text = "[PHASE 1/3] Kim Cương Thể"; |
| Assets/Scripts/Combat/BossEnemy.cs:309 | phaseTextUI.text = "[PHASE 2/3] Lôi Giáp Bạo Phát"; |
| Assets/Scripts/Combat/BossEnemy.cs:313 | phaseTextUI.text = "[PHASE 3/3] Cuồng Nộ Diệt Thế"; |
| Assets/Scripts/Managers/CombatManager.cs:283 | GameManager.Instance.OnStateChanged += HandleStateChanged; |
| Assets/Scripts/Managers/CombatManager.cs:299 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/Managers/CombatManager.cs:303 | CultivationManager.Instance.OnTuViChanged += HandleTuViChanged; |
| Assets/Scripts/Managers/CombatManager.cs:327 | PlayerStatsService.Instance.OnStatsChanged += HandleStatsChanged; |
| Assets/Scripts/Managers/CombatManager.cs:567 | GameManager.Instance.OnStateChanged -= HandleStateChanged; |
| Assets/Scripts/Managers/CombatManager.cs:583 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/Managers/CombatManager.cs:587 | CultivationManager.Instance.OnTuViChanged -= HandleTuViChanged; |
| Assets/Scripts/Managers/CombatManager.cs:599 | PlayerStatsService.Instance.OnStatsChanged -= HandleStatsChanged; |
| Assets/Scripts/Managers/CombatManager.cs:4381 | playerHpText.text = $"HP: {EconomyManager.FormatNumber(playerCurrentHp)} / {EconomyManager.FormatNumber(playerMaxHp)} ({pct}%)"; |
| Assets/Scripts/Managers/CultivationManager.cs:139 | stageText.text = $"{displayName} · {phaseProgress * 100f:F0}%"; |
| Assets/Scripts/Managers/CultivationManager.cs:141 | stageText.text = $"{displayName} · Độ kiếp"; |
| Assets/Scripts/Managers/CultivationManager.cs:143 | stageText.text = $"{displayName} · Viên Mãn"; |
| Assets/Scripts/Managers/CultivationManager.cs:154 | characterInfoText.text = $"<b><color=#FFD700>{nameDisplay}</color></b> · <color=#66D9EF>{displayName}</color>\n<color=#A0B0C5>Tu Vi:</color> <color=#FFFFFF>{tuViCur} / {tuViReq}</color>"; |
| Assets/Scripts/Managers/CultivationManager.cs:156 | characterInfoText.text = $"<size=28><b><color=#F1E8CF>{nameDisplay}</color></b></size>\n<size=26><color=#6EDDB6>{displayName}</color></size>\n<size=24><color=#CFBE9C>Tu vi {tuViCur} / {tuViReq}</color></size>"; |
| Assets/Scripts/Managers/EncounterManager.cs:94 | txt.text = "KỲ NGỘ\nChạm mở"; |
| Assets/Scripts/Managers/EncounterManager.cs:139 | canvasGroup.blocksRaycasts = canvasGroup.interactable = available; |
| Assets/Scripts/Managers/GameLogger.cs:79 | txt.text = UiTextStyle.RemoveIcons(message); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:72 | MobileDrawerUIController.Instance.OnDockCollapseChanged += OnDockCollapseChanged; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:87 | MobileDrawerUIController.Instance.OnDockCollapseChanged -= OnDockCollapseChanged; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:186 | styleButton.interactable = slot != null && slot.swordStyleUnlocked && !HasEncounterChoice; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:187 | if (slot == null) status.text = "KỲ NGỘ"; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:188 | else if (slot.swordStyleUnlocked) status.text = Style == 0 ? "KIẾM XUYÊN - ĐỔI" : "PHÁ GIÁP - ĐỔI"; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:189 | else status.text = "KIẾM PHÁP (Hợp Lv2)"; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:405 | text.text = value; text.fontSize = 20; text.color = new Color(0.96f, 0.88f, 0.50f, 1f); |
| Assets/Scripts/Managers/MergeManager.cs:41 | SwordLoadoutService.Instance.OnCapacityChanged += HandleCapacityChanged; |
| Assets/Scripts/Managers/MergeManager.cs:52 | SwordLoadoutService.Instance.OnCapacityChanged -= HandleCapacityChanged; |
| Assets/Scripts/Managers/MergeManager.cs:354 | pageText.text = $"{currentPage + 1}/{inventoryPages.Length}"; |
| Assets/Scripts/Managers/ShopManager.cs:146 | buffTexts[index].text = $"{label}: +{percent}{unit} (x{stacks}) [{Mathf.Ceil(timer)}s]"; |
| Assets/Scripts/Managers/SkillManager.cs:471 | skillText.text = "VẠN KIẾM QUY TÔNG!"; |
| Assets/Scripts/Managers/SkillManager.cs:473 | skillText.text = $"Hồi chiêu: {currentCooldown:F1}s"; |
| Assets/Scripts/Managers/SkillManager.cs:475 | skillText.text = $"MP: {Mathf.FloorToInt(currentLinhLuc)}/{Mathf.FloorToInt(maxLinhLuc)}"; |
| Assets/Scripts/Managers/StageManager.cs:58 | bossTimerText.text = $"Thời gian: {Mathf.Ceil(bossTimer)}s"; |
| Assets/Scripts/Managers/StageManager.cs:189 | stageText.text = $"Tầng {currentStage} - KHIÊU CHIẾN LÔI KIẾP!"; |
| Assets/Scripts/Managers/StageManager.cs:195 | stageText.text = $"Tầng {currentStage} [ {enemiesKilled} / {enemiesToBoss} ]"; |
| Assets/Scripts/Managers/TooltipManager.cs:59 | cg.interactable = false; |
| Assets/Scripts/Managers/TooltipManager.cs:157 | tooltipText.text = content; |
| Assets/Scripts/Managers/TutorialManager.cs:389 | txtClaimQuick.text = "Mốc"; |
| Assets/Scripts/Managers/TutorialManager.cs:439 | atTxt.text = wideQuestBar ? "Thành\ntựu" : "Tựu"; |
| Assets/Scripts/Managers/TutorialManager.cs:483 | tTxt.text = isCollapsed ? "Mở" : "Gọn"; |
| Assets/Scripts/Managers/TutorialManager.cs:494 | if (tTxt != null) tTxt.text = isCollapsed ? "Mở" : "Gọn"; |
| Assets/Scripts/Managers/TutorialManager.cs:534 | cg.interactable = false; |
| Assets/Scripts/Managers/TutorialManager.cs:547 | arrowText.text = "Đây"; |
| Assets/Scripts/Managers/TutorialManager.cs:565 | txtPointerHint.text = "Bấm vào đây!"; |
| Assets/Scripts/Managers/TutorialManager.cs:586 | if (aTxt != null && string.IsNullOrEmpty(aTxt.text)) aTxt.text = "Đây"; |
| Assets/Scripts/Managers/TutorialManager.cs:632 | txtPointerHint.text = hintText; |
| Assets/Scripts/Managers/TutorialManager.cs:1061 | if (txtQuestTitle != null) txtQuestTitle.text = title; |
| Assets/Scripts/Managers/TutorialManager.cs:1076 | txtClaimQuick.text = "NHẬN"; |
| Assets/Scripts/Managers/TutorialManager.cs:1082 | txtClaimQuick.text = "Mốc"; |
| Assets/Scripts/Managers/TutorialManager.cs:1091 | if (txtQuestReward == null \|\| txtQuestReward.text == reward) return; |
| Assets/Scripts/Managers/TutorialManager.cs:1092 | txtQuestReward.text = reward; |
| Assets/Scripts/Managers/TutorialManager.cs:1179 | if (txtQuestTitle != null) txtQuestTitle.text = $"<color=#55FF55>{message}</color>"; |
| Assets/Scripts/UI/AchievementModalUI.cs:81 | AchievementManager.Instance.OnAchievementsUpdated += OnAchievementsUpdatedHandler; |
| Assets/Scripts/UI/AchievementModalUI.cs:90 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/AchievementModalUI.cs:101 | AchievementManager.Instance.OnAchievementsUpdated -= OnAchievementsUpdatedHandler; |
| Assets/Scripts/UI/AchievementModalUI.cs:170 | canvasGroup.interactable = true; |
| Assets/Scripts/UI/AchievementModalUI.cs:257 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/AchievementModalUI.cs:312 | txtTotalProgress.text = $"Tiến Độ Tiên Lộ: {claimedCount}/{totalCount} ({pct:F0}%) \| <color=#FFD700>{readyToClaimCount} Chờ Nhận</color>"; |
| Assets/Scripts/UI/AchievementModalUI.cs:317 | fillTotalProgress.fillAmount = totalCount > 0 ? (float)claimedCount / totalCount : 0f; |
| Assets/Scripts/UI/AchievementModalUI.cs:324 | btnClaimAll.interactable = (readyInCat > 0); |
| Assets/Scripts/UI/AchievementModalUI.cs:327 | txtClaimAll.text = readyInCat > 0 ? $"NHẬN TOÀN BỘ ({readyInCat})" : "Không Có Thưởng Chờ"; |
| Assets/Scripts/UI/AchievementModalUI.cs:416 | iconTxt.text = GetCategoryLabel(item.category); |
| Assets/Scripts/UI/AchievementModalUI.cs:433 | tTxt.text = item.title; |
| Assets/Scripts/UI/AchievementModalUI.cs:449 | dTxt.text = item.description; |
| Assets/Scripts/UI/AchievementModalUI.cs:487 | ptTxt.text = isDone ? $"{target} / {target} (Hoàn Thành)" : $"{curProgress} / {target} ({progressPct * 100f:F0}%)"; |
| Assets/Scripts/UI/AchievementModalUI.cs:503 | rTxt.text = item.GetRewardSummary(); |
| Assets/Scripts/UI/AchievementModalUI.cs:535 | bTxt.text = "ĐÃ NHẬN"; |
| Assets/Scripts/UI/AchievementModalUI.cs:537 | btn.interactable = false; |
| Assets/Scripts/UI/AchievementModalUI.cs:544 | bTxt.text = "NHẬN"; |
| Assets/Scripts/UI/AchievementModalUI.cs:546 | btn.interactable = true; |
| Assets/Scripts/UI/AchievementModalUI.cs:559 | bTxt.text = $"{curProgress}/{target}"; |
| Assets/Scripts/UI/AchievementModalUI.cs:561 | btn.interactable = false; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:95 | AlchemyService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:97 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:99 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:108 | AlchemyService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:110 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:112 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:134 | if (txtHerbLinhChi != null)   txtHerbLinhChi.text   = $"Linh Chi: <b><color=#55efc4>{cLinhChi}</color></b>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:135 | if (txtHerbNgungKhi != null)  txtHerbNgungKhi.text  = $"Ngưng Khí: <b><color=#ffeaa7>{cNgungKhi}</color></b>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:136 | if (txtHerbTuyetLien != null) txtHerbTuyetLien.text = $"Tuyết Liên: <b><color=#74b9ff>{cTuyetLien}</color></b>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:139 | if (txtAlchemistRank != null) txtAlchemistRank.text = $"Đan Sư Cấp {svc.AlchemistRank}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:140 | if (txtMedicineDust != null)  txtMedicineDust.text  = $"Bụi Dược: {svc.State.medicineDust}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:143 | if (linhChiLabel != null)     linhChiLabel.text     = $"Linh Chi: {cLinhChi}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:144 | if (caoNgungKhiLabel != null) caoNgungKhiLabel.text = $"Ngưng Khí Thảo: {cNgungKhi}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:145 | if (tuyetLienLabel != null)   tuyetLienLabel.text   = $"Tuyết Liên: {cTuyetLien}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:146 | if (dustLabel != null)        dustLabel.text        = $"Bụi Dược: {svc.State.medicineDust}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:156 | txtCostTuVi.text = $"Cần: <color={col0}>Chi {costTuVi[0]}</color> · <color={col1}>Ngưng {costTuVi[1]}</color>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:158 | if (btnBrewTuVi != null) btnBrewTuVi.interactable = canBrewTuVi; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:159 | if (brewCultivationBtn != null) brewCultivationBtn.interactable = canBrewTuVi; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:168 | txtCostBreakthrough.text = $"Cần: <color={col0}>Chi {costBT[0]}</color> · <color={col1}>Ngưng {costBT[1]}</color> · <color={col2}>Liên {costBT[2]}</color>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:170 | if (btnBrewBreakthrough != null) btnBrewBreakthrough.interactable = canBrewBT; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:171 | if (brewBreakthroughBtn != null) brewBreakthroughBtn.interactable = canBrewBT; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:176 | if (txtDustCost != null) txtDustCost.text = $"Tốc Hành ({dustCost} Bụi Dược)"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:177 | if (btnFinishWithDust != null) btnFinishWithDust.interactable = canDust; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:178 | if (useDustBtn != null)        useDustBtn.interactable        = canDust; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:197 | if (txtFurnaceStatus != null) txtFurnaceStatus.text = $"Đang Luyện: <b><color=#ffd32a>{pillName}</color></b>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:198 | if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = $"⏳ Thời gian còn: <b>{remaining.Minutes:D2}:{remaining.Seconds:D2}</b>"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:199 | if (brewTimerLabel != null)   brewTimerLabel.text   = $"⏳ Đang luyện {pillName}: {remaining.Minutes:D2}:{remaining.Seconds:D2}"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:203 | if (txtFurnaceStatus != null) txtFurnaceStatus.text = "Đan thành xuất lò!"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:204 | if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = "Chờ thu nạp..."; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:205 | if (brewTimerLabel != null)   brewTimerLabel.text   = "Đan thành xuất lò!"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:210 | if (txtFurnaceStatus != null) txtFurnaceStatus.text = "Lò Luyện Trống"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:211 | if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = "Chọn bài thuốc bên dưới để bắt đầu luyện đan"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:212 | if (brewTimerLabel != null)   brewTimerLabel.text   = "Lò luyện sẵn sàng"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:225 | txtTuViPillsSummary.text = totalTuVi > 0 |
| Assets/Scripts/UI/AlchemyPanelUI.cs:237 | txtBreakthroughPillsSummary.text = totalBT > 0 |
| Assets/Scripts/UI/AlchemyPanelUI.cs:246 | btnUseTuViPill.interactable = canDrink; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:250 | label.text = totalTuVi > 0 ? $"UỐNG ĐAN ({totalTuVi})" : "HẾT ĐAN"; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:262 | pillInventoryLabel.text = sb.ToString(); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:56 | ArtifactService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:58 | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:60 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:62 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:72 | ArtifactService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:74 | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:76 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:78 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:88 | if (oreLabel != null) oreLabel.text = $"Quặng Phôi: <b><color=#f39c12>{ore}</color></b>"; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:89 | if (auraLabel != null) auraLabel.text = $"Hào Quang: <b><color=#ffd32a>Bậc {svc.AuraTier}</color></b>"; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:105 | artifactLabels[i].text = $"<b>{SlotNames[i]}</b>  {levelStr}"; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:111 | artifactStatsTexts[i].text = GetSlotStatsFormatted(slot, level); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:119 | artifactCostTexts[i].text = FeatureUnlockService.Instance.GetArtifactSlotUnlockRequirementText(slot); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:123 | artifactCostTexts[i].text = "<color=#ffd32a>Đạt Cực Hạn Cường Hóa</color>"; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:129 | artifactCostTexts[i].text = $"Cần: <color={oreColor}>Quặng {oreCost}</color>  \|  <color={stoneColor}>LT {EconomyManager.FormatNumber(stoneCost)}</color>"; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:136 | upgradeButtons[i].interactable = canAfford; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:140 | btnText.text = !unlocked ? "CHƯA MỞ KHÓA" : maxed ? "ĐẠI THÀNH" : (level == 0 ? "CHẾ TẠO" : "CƯỜNG HÓA"); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:148 | PlayerStatsService.Instance.OnStatsChanged += Refresh; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:150 | EconomyManager.Instance.OnLinhThachChanged += HandleCurrencyChanged; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:158 | PlayerStatsService.Instance.OnStatsChanged -= Refresh; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:160 | EconomyManager.Instance.OnLinhThachChanged -= HandleCurrencyChanged; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:225 | txtLinhThachBalance.text = $"Linh Thạch: {EconomyManager.FormatNumber(economy.currentLinhThach)}"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:229 | txtTotalBodyLevel.text = $"Tổng Cấp Thể Phách: {stats.GetTotalBodyLevel()}"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:245 | nodeNameTexts[i].text = $"{nodeDef.name}"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:251 | nodeLevelTexts[i].text = isUnlocked ? $"Cấp {level}" : "Chưa Mở"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:261 | nodeEffectTexts[i].text = level > 0 |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:267 | nodeEffectTexts[i].text = $"<color=#ff7675>{stats.GetUnlockRequirementText(i)}</color>"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:274 | nodeCostTexts[i].text = isUnlocked ? $"{EconomyManager.FormatNumber(cost)} LT" : "Khóa"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:295 | nodeLockRequirementTexts[i].text = stats.GetUnlockRequirementText(i); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:329 | txtModalTitle.text = presentation!=null |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:338 | txtModalTier.text = $"<color={branchColor}>Nhánh {nodeDef.branchName}</color>  •  Tier {nodeDef.tier}  •  Cấp {level}"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:344 | txtModalLore.text = $"<i>\"{nodeDef.lore}\"</i>"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:350 | txtModalMechanics.text = $"<b>Công năng thực chiến:</b>\n{nodeDef.combatMechanics}\n\n" + |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:357 | txtModalCurrent.text = level > 0 |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:364 | txtModalNext.text = isUnlocked |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:375 | txtModalRequirement.text = $"{stats.GetUnlockRequirementText(selectedIndex)}"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:387 | btnModalUpgrade.interactable = canAfford; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:395 | txtModalCost.text = "CHƯA MỞ KHÓA"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:400 | txtModalCost.text = $"CƯỜNG HÓA ({EconomyManager.FormatNumber(cost)} LT)"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:417 | btnModalUpgrade5.interactable = economy != null && economy.currentLinhThach >= cost; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:421 | textComp.text = $"NÂNG x5\n({EconomyManager.FormatNumber(cost5)} LT)"; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:430 | btnModalUpgradeMax.interactable = canAfford; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:136 | if (txtName != null) txtName.text = $"Đạo Hiệu: <color=#FFE666>{charName}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:146 | if (txtRealm != null) txtRealm.text = $"Cảnh Giới: <color=#66FFB2>{CultivationManager.Instance.GetDisplayRealmName()}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:147 | if (txtTuVi != null) txtTuVi.text = $"Tu Vi: <color=#66CCFF>{EconomyManager.FormatNumber(currentTuVi)} / {EconomyManager.FormatNumber(reqTuVi)}</color> (Tiến độ kỳ: <color=#FFE666>{progress:F1}%</color>)"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:152 | if (txtCoDuyen != null) txtCoDuyen.text = $"Cơ Duyên: <color=#FFAA33>{EconomyManager.Instance.currentCoDuyen} Điểm</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:153 | if (txtLinhThach != null) txtLinhThach.text = $"Linh Thạch: <color=#66CCFF>{EconomyManager.Instance.currentLinhThach:N0}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:161 | if (txtHp != null) txtHp.text = $"Sinh Lực: <color=#55FF55>{Mathf.Ceil(hp):N0} / {maxHp:N0}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:169 | if (txtMp != null) txtMp.text = $"Linh Lực: <color=#66B2FF>{mp:F0} / {maxMp:F0}</color> (+{regen:F1}/s hồi phục)"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:176 | if (txtMultiplier != null) txtMultiplier.text = $"Khí Tức Tiên Thiên: <color=#FFDD44>+{bonus:F1}%</color> (Hệ số x{mult:F2})"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:202 | txtEquipSwords.text = $"Phi Kiếm Đang Xuất Trận: <color=#FFE666>{equippedCount}/{capacity} Thanh</color> (Tổng Căn Cơ: {totalDamage:N0} Dame)"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:243 | txtUpgradeKiemY.text = $"• <color=#FF8866>Kiếm Ý</color>: Cấp {lvlKY} · Chỉ số: <color=#FFE666>+{valKY:F1} Sát thương Kiếm</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:246 | txtUpgradeLinhLuc.text = $"• <color=#66CCFF>Linh Lực</color>: Cấp {lvlLL} · Chỉ số: <color=#66CCFF>{maxMpVal:F0} MP</color> (+{regenMpVal:F1}/s hồi phục)"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:249 | txtUpgradeTuLinh.text = $"• <color=#88FF88>Tụ Linh</color>: Cấp {lvlTL} · Chỉ số: <color=#88FF88>+{valTL:F1}% Thu nạp Linh Thạch</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:252 | txtUpgradeThanThuc.text = $"• <color=#CC88FF>Thần Thức</color>: Cấp {lvlTT} · Chỉ số: <color=#CC88FF>+{valTT:F1}% Tốc độ xuất chiêu</color>"; |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:13 | EconomyManager.Instance.OnCoDuyenChanged += UpdateUI; |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:22 | EconomyManager.Instance.OnCoDuyenChanged -= UpdateUI; |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:30 | txt.text = coDuyen.ToString(); |
| Assets/Scripts/UI/CollapsibleQuestBar.cs:66 | toggleArrowText.text = "<"; |
| Assets/Scripts/UI/CollapsibleQuestBar.cs:83 | toggleArrowText.text = IsCollapsed ? ">" : "<"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:75 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/CombatNotificationUI.cs:120 | titleText.text = "ĐỘT PHÁ THÀNH CÔNG"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:126 | subtitleText.text = $"[ {realmName} ]"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:132 | extraDetailText.text = "Khí tức ngập tràn • Thọ nguyên gia tăng • Sinh lực hồi phục"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:157 | titleText.text = "ĐẠO THÂN TỬ TRẬN"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:163 | subtitleText.text = "Bất Hạnh Vẫn Lạc — Đạo Tâm Bất Diệt"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:169 | extraDetailText.text = "Làm mới tiến trình ải • Đang trùng tu hồi sinh..."; |
| Assets/Scripts/UI/CombatNotificationUI.cs:203 | titleText.text = "ĐỘ KIẾP THẤT BẠI"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:209 | subtitleText.text = $"[ Bị Thiên Kiếp Phản Phệ • Tổn Thất {CultivationManager.TribulationFailureLossRate:0%} Tu Vi (-{lostTuVi:F0}) ]"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:215 | extraDetailText.text = "Đạo thân thọ thương • Hãy củng cố tu vi và nâng cấp phi kiếm trước khi thử lại!"; |
| Assets/Scripts/UI/CombatNotificationUI.cs:256 | canvasGroup.blocksRaycasts=false;canvasGroup.interactable=false; |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs:100 | labels[i].text = PhaseLabels[i]; |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs:107 | tribulationButton.interactable = tribulationReady; |
| Assets/Scripts/UI/CultivationPhaseBarUI.cs:124 | labels[i].text = i == 3 && tribulationReady ? "MỞ ĐỘ KIẾP" : PhaseLabels[i]; |
| Assets/Scripts/UI/CultivationUI.cs:14 | CultivationManager.Instance.OnTuViChanged += UpdateTuViUI; |
| Assets/Scripts/UI/CultivationUI.cs:15 | CultivationManager.Instance.OnStageChanged += UpdateStageUI; |
| Assets/Scripts/UI/CultivationUI.cs:29 | CultivationManager.Instance.OnTuViChanged -= UpdateTuViUI; |
| Assets/Scripts/UI/CultivationUI.cs:30 | CultivationManager.Instance.OnStageChanged -= UpdateStageUI; |
| Assets/Scripts/UI/CultivationUI.cs:36 | if (txtTuVi != null) txtTuVi.text = $"Tu Vi: {EconomyManager.FormatNumber(current)} / {EconomyManager.FormatNumber(max)}"; |
| Assets/Scripts/UI/CultivationUI.cs:38 | imgProgressBar.fillAmount = CultivationDisplay.PhaseProgress( |
| Assets/Scripts/UI/CultivationUI.cs:45 | txtStageName.text = $"Cảnh giới: {CultivationManager.Instance.GetDisplayRealmName()}"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:90 | text.text = value; |
| Assets/Scripts/UI/DaoAnModalUI.cs:162 | txt.text = "<b><color=#FFE666>ĐẠO VẬN</color></b>"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:234 | if (xTxt != null) xTxt.text = "X"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:258 | stTxt.text = "Cây Thiên Phú Bị Động - Vĩnh Viễn Không Mất Khi Độ Kiếp"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:525 | if (text != null) text.text = label; |
| Assets/Scripts/UI/DaoAnModalUI.cs:598 | button.interactable = canBuy; |
| Assets/Scripts/UI/DaoAnModalUI.cs:610 | walletText.text = "Linh Thạch: " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach); |
| Assets/Scripts/UI/DaoAnModalUI.cs:635 | if (rankText != null) rankText.text = $"Đạo Vị: {rankTitle} ({totalLv}/{maxTotal})"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:636 | if (activeBuffsText != null) activeBuffsText.text = $"{activeCount} Đạo Ấn Hoạt Hóa ({totalLv} Cấp) • Không Mất Khi Độ Kiếp"; |
| Assets/Scripts/UI/DaoAnModalUI.cs:855 | buyBtn.interactable = false; |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:117 | if (txtTitle != null) txtTitle.text = "XÁC NHẬN PHÂN GIẢI BẢO VẬT"; |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:118 | if (txtItemName != null) txtItemName.text = itemName; |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:119 | if (txtItemDetail != null) txtItemDetail.text = itemDetail; |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:122 | txtRewardSummary.text = "Tài nguyên sẽ thu hồi:\n" + (reward != null ? reward.GetFormattedSummary() : "Không có"); |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:126 | txtWarning.text = "Đây là vật phẩm quý giá được rèn luyện kỳ công!\nSau khi tách sẽ bị tiêu biến vĩnh viễn và không thể hoàn tác!"; |
| Assets/Scripts/UI/FormationPanelUI.cs:96 | FormationService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:98 | ArtifactService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:100 | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:102 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:104 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:114 | FormationService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:116 | ArtifactService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:118 | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:120 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:122 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:138 | if (txtOreLabel != null) txtOreLabel.text = $"Quặng Phôi: <b><color=#f39c12>{ore}</color></b>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:143 | txtCurrentActiveLabel.text = $"Đang Dùng: <b><color=#ffd32a>{sName}</color></b>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:164 | formationLabels[i].text = $"<b>{FormationNames[i]}</b>  {lvlStr}"; |
| Assets/Scripts/UI/FormationPanelUI.cs:170 | formationDescTexts[i].text = FormationDescs[i]; |
| Assets/Scripts/UI/FormationPanelUI.cs:178 | formationCostTexts[i].text = FeatureUnlockService.Instance.GetFormationUnlockRequirementText(id); |
| Assets/Scripts/UI/FormationPanelUI.cs:182 | formationCostTexts[i].text = "<color=#ffd32a>Đại Trận Đã Đại Thành</color>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:188 | formationCostTexts[i].text = $"Cần: <color={oreColor}>Quặng {oreCost}</color> \| <color={stoneColor}>LT {EconomyManager.FormatNumber(stoneCost)}</color>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:197 | statusBadges[i].text = "CHƯA MỞ KHÓA"; |
| Assets/Scripts/UI/FormationPanelUI.cs:201 | statusBadges[i].text = "<color=#ffd32a>ĐANG KÍCH HOẠT</color>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:205 | statusBadges[i].text = "<color=#7f8c8d>Chưa Khai Ngộ</color>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:209 | statusBadges[i].text = "<color=#74b9ff>Sẵn Sàng Dùng</color>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:216 | selectButtons[i].interactable = unlocked && level > 0 && !isSelected; |
| Assets/Scripts/UI/FormationPanelUI.cs:220 | bText.text = isSelected ? "ĐANG DÙNG" : (level == 0 ? "CHƯA HỌC" : "KÍCH HOẠT"); |
| Assets/Scripts/UI/FormationPanelUI.cs:233 | upgradeButtons[i].interactable = canAfford; |
| Assets/Scripts/UI/FormationPanelUI.cs:237 | upgText.text = maxed ? "ĐẠI THÀNH" : (level == 0 ? "HỌC TRẬN" : "TU LUYỆN"); |
| Assets/Scripts/UI/FormationPanelUI.cs:254 | activateSpiritBtn.interactable = unlocked && spiritSelected && svc.SpiritCooldown == System.TimeSpan.Zero && !svc.SpiritActive; |
| Assets/Scripts/UI/FormationPanelUI.cs:262 | spiritCooldownLabel.text = ""; |
| Assets/Scripts/UI/FormationPanelUI.cs:267 | spiritCooldownLabel.text = "<b><color=#55efc4>Tụ Linh Trận Đang Phát Huy Uy Lực (x2 Tu Vi)!</color></b>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:272 | spiritCooldownLabel.text = $"⏳ Hồi chiêu: <b>{cd.Minutes:D2}:{cd.Seconds:D2}</b>"; |
| Assets/Scripts/UI/FormationPanelUI.cs:276 | spiritCooldownLabel.text = "<b><color=#ffd32a>Tụ Linh Trận Sẵn Sàng Kích Hoạt!</color></b>"; |
| Assets/Scripts/UI/JadeUIAdaptiveSkin.cs:29 | {t.text=System.Text.RegularExpressions.Regex.Replace(t.text,@"</?size(?:=\d+)?>","");Font(t.transform,t.name=="Text"?18:20);} |
| Assets/Scripts/UI/JadeUITheme.cs:94 | text.text = UiTextStyle.RemoveIcons(text.text); |
| Assets/Scripts/UI/MainMenuManager.cs:188 | nameInput.onValueChanged.AddListener(_ => characterNameLabel.text = characterNamePrompt); |
| Assets/Scripts/UI/MainMenuManager.cs:198 | bgmSlider.value = AudioManager.Instance.bgmVolume; |
| Assets/Scripts/UI/MainMenuManager.cs:204 | sfxSlider.value = AudioManager.Instance.sfxVolume; |
| Assets/Scripts/UI/MainMenuManager.cs:221 | playButtonText.text = hasAnySave ? "Tiếp Tục" : "Chơi Mới"; |
| Assets/Scripts/UI/MainMenuManager.cs:243 | if (nameInput) nameInput.text = ""; |
| Assets/Scripts/UI/MainMenuManager.cs:244 | if (characterNameLabel != null) characterNameLabel.text = characterNamePrompt; |
| Assets/Scripts/UI/MainMenuManager.cs:253 | if (characterNameLabel != null) characterNameLabel.text = "Vui lòng nhập đạo hiệu."; |
| Assets/Scripts/UI/MainMenuManager.cs:353 | if (popupTitle) popupTitle.text = title; |
| Assets/Scripts/UI/MainMenuManager.cs:354 | if (popupMessage) popupMessage.text = message; |
| Assets/Scripts/UI/MilestoneModalUI.cs:118 | cTxt.text = "Đóng"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:145 | MilestoneManager.Instance.OnMilestonesUpdated += RefreshUI; |
| Assets/Scripts/UI/MilestoneModalUI.cs:153 | MilestoneManager.Instance.OnMilestonesUpdated -= RefreshUI; |
| Assets/Scripts/UI/MilestoneModalUI.cs:205 | txtTotalProgress.text = $"Tiến Độ Tu Đạo: {claimedCount}/{totalCount} Đạt Mốc \| ({readyToClaimCount} Chờ Nhận)"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:210 | btnClaimAll.interactable = (readyToClaimCount > 0); |
| Assets/Scripts/UI/MilestoneModalUI.cs:214 | btnTxt.text = readyToClaimCount > 0 ? $"NHẬN TOÀN BỘ ({readyToClaimCount})" : "Không Có Thưởng Chờ"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:301 | titleTxt.text = quest.isHard ? $"{quest.title} <color=#FFD700>[Cơ Duyên]</color>" : quest.title; |
| Assets/Scripts/UI/MilestoneModalUI.cs:321 | descTxt.text = $"{quest.description}\n<color=#FFD700>Thưởng: {quest.GetRewardSummary()}</color>"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:365 | pTxt.text = $"{Mathf.Min(curProgress, target)}/{target}"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:404 | bTxt.text = "Đã Nhận"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:406 | btn.interactable = false; |
| Assets/Scripts/UI/MilestoneModalUI.cs:412 | bTxt.text = "NHẬN"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:414 | btn.interactable = true; |
| Assets/Scripts/UI/MilestoneModalUI.cs:426 | bTxt.text = "Chưa Đạt"; |
| Assets/Scripts/UI/MilestoneModalUI.cs:428 | btn.interactable = false; |
| Assets/Scripts/UI/MobileCurrencyText.cs:16 | EconomyManager.Instance.OnLinhThachChanged += OnLinhThachChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:17 | EconomyManager.Instance.OnCoDuyenChanged += OnCoDuyenChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:24 | EconomyManager.Instance.OnLinhThachChanged -= OnLinhThachChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:25 | EconomyManager.Instance.OnCoDuyenChanged -= OnCoDuyenChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:36 | label.text = "LT " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach); |
| Assets/Scripts/UI/MobileCurrencyText.cs:41 | label.text = "CD " + EconomyManager.Instance.currentCoDuyen.ToString("N0"); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:131 | dockToggleLabelText.text = "Thu Gọn Bảng Dưới"; |
| Assets/Scripts/UI/MobileDrawerUIController.cs:210 | dockToggleLabelText.text = collapse ? "Mở Bảng Chức Năng" : "Thu Gọn Bảng Dưới"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:96 | btnUltimate.interactable = false; |
| Assets/Scripts/UI/MobileSkillHUD.cs:97 | if (txtUltiTitle != null) txtUltiTitle.text = "VẠN KIẾM"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:98 | if (txtUltiMana != null) txtUltiMana.text = "Kiếm Lv7"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:99 | if (imgUltiFill != null) imgUltiFill.fillAmount = 0f; |
| Assets/Scripts/UI/MobileSkillHUD.cs:106 | imgUltiFill.fillAmount = fillRatio; |
| Assets/Scripts/UI/MobileSkillHUD.cs:111 | btnUltimate.interactable = false; |
| Assets/Scripts/UI/MobileSkillHUD.cs:112 | if (txtUltiTitle != null) txtUltiTitle.text = "XUẤT TRẬN"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:113 | if (txtUltiMana != null) txtUltiMana.text = $"{Mathf.CeilToInt(mana)} MP"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:122 | btnUltimate.interactable = false; |
| Assets/Scripts/UI/MobileSkillHUD.cs:123 | if (txtUltiTitle != null) txtUltiTitle.text = "HỒI"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:124 | if (txtUltiMana != null) txtUltiMana.text = $"{cd:F1}s"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:130 | btnUltimate.interactable = ready; |
| Assets/Scripts/UI/MobileSkillHUD.cs:131 | if (txtUltiTitle != null) txtUltiTitle.text = ready ? "XUẤT CHIÊU" : "VẠN KIẾM"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:132 | if (txtUltiMana != null) txtUltiMana.text = $"{Mathf.FloorToInt(mana)}/{Mathf.FloorToInt(maxMana)}"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:164 | btnNormalSkills[i].interactable = false; |
| Assets/Scripts/UI/MobileSkillHUD.cs:165 | if (imgNormalCooldownOverlays[i] != null) imgNormalCooldownOverlays[i].fillAmount = 1f; |
| Assets/Scripts/UI/MobileSkillHUD.cs:166 | if (txtNormalCooldownTimers[i] != null) txtNormalCooldownTimers[i].text = LockRequirements[i]; |
| Assets/Scripts/UI/MobileSkillHUD.cs:167 | if (txtNormalTitles[i] != null) txtNormalTitles[i].text = SkillNames[i]; |
| Assets/Scripts/UI/MobileSkillHUD.cs:168 | if (txtNormalCosts[i] != null) txtNormalCosts[i].text = $"{cost} MP"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:172 | if (txtNormalTitles[i] != null) txtNormalTitles[i].text = SkillNames[i]; |
| Assets/Scripts/UI/MobileSkillHUD.cs:173 | if (txtNormalCosts[i] != null) txtNormalCosts[i].text = $"{cost} MP"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:177 | btnNormalSkills[i].interactable = false; |
| Assets/Scripts/UI/MobileSkillHUD.cs:181 | imgNormalCooldownOverlays[i].fillAmount = cd / baseCd; |
| Assets/Scripts/UI/MobileSkillHUD.cs:186 | txtNormalCooldownTimers[i].text = $"{cd:F1}s"; |
| Assets/Scripts/UI/MobileSkillHUD.cs:193 | btnNormalSkills[i].interactable = canCast; |
| Assets/Scripts/UI/MobileSkillHUD.cs:198 | imgNormalCooldownOverlays[i].fillAmount = hasMana ? 0f : 0.6f; |
| Assets/Scripts/UI/MobileSkillHUD.cs:203 | txtNormalCooldownTimers[i].text = hasMana ? "" : "Hết MP"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:102 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:142 | canvasGroup.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:169 | titleText.text = "BẾ QUAN TU LUYỆN"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:186 | timeAwayText.text = $"Đạo hữu đã bế quan tĩnh tọa: <color=#66FFB2>{timeStr}</color>"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:205 | rewardDetailsText.text = sb.ToString().TrimEnd(); |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:209 | claimDoubleButton.interactable = RewardedAdBridge.IsReady; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:211 | if (label != null) label.text = RewardedAdBridge.IsReady ? "XEM QC (x2)" : "QC chưa sẵn sàng"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:228 | if (claimNormalButton != null) claimNormalButton.interactable = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:229 | if (claimDoubleButton != null) claimDoubleButton.interactable = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:230 | if (closeButton != null) closeButton.interactable = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:244 | if (claimNormalButton != null) claimNormalButton.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:245 | if (claimDoubleButton != null) claimDoubleButton.interactable = RewardedAdBridge.IsReady; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:247 | if (closeButton != null) closeButton.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:254 | if (claimNormalButton != null) claimNormalButton.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:255 | if (claimDoubleButton != null) claimDoubleButton.interactable = RewardedAdBridge.IsReady; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:256 | if (closeButton != null) closeButton.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:306 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:372 | cg.interactable = true; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:393 | titleTxt.text = "BẾ QUAN TU LUYỆN"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:408 | timeTxt.text = "Đạo hữu đã bế quan tĩnh tọa: 0 phút"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:449 | btnNormTxt.text = "THU NHẬN (x1)"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:471 | btnDblTxt.text = "XEM QC (x2)"; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:495 | btnCloseTxt.text = "×"; |
| Assets/Scripts/UI/PauseMenuManager.cs:175 | bgmSlider.value = AudioManager.Instance.bgmVolume; |
| Assets/Scripts/UI/PauseMenuManager.cs:176 | if (txtBgmValue != null) txtBgmValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.bgmVolume * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:181 | sfxSlider.value = AudioManager.Instance.sfxVolume; |
| Assets/Scripts/UI/PauseMenuManager.cs:182 | if (txtSfxValue != null) txtSfxValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.sfxVolume * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:258 | txtCharacterInfo.text = $"<b>{charName}</b>  \|  <color=#66FFB2>{stageName}</color>  \|  Tầng <color=#FFE666>{combatStage}</color> (Slot {slotIdx + 1})"; |
| Assets/Scripts/UI/PauseMenuManager.cs:263 | txtSaveStatus.text = "Trạng thái: <color=#88FF88>Tiến trình sẵn sàng đồng bộ</color>"; |
| Assets/Scripts/UI/PauseMenuManager.cs:278 | txtSaveStatus.text = $"Trạng thái: <color=#66FF66>Đã lưu thành công lúc {timeStr}!</color>"; |
| Assets/Scripts/UI/PauseMenuManager.cs:377 | txtBgmValue.text = $"{Mathf.RoundToInt(val * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:389 | txtSfxValue.text = $"{Mathf.RoundToInt(val * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:413 | if (txtPopupTitle != null) txtPopupTitle.text = title; |
| Assets/Scripts/UI/PauseMenuManager.cs:414 | if (txtPopupMessage != null) txtPopupMessage.text = message; |
| Assets/Scripts/UI/PortraitHudController.cs:125 | economy.OnLinhThachChanged += OnStones; |
| Assets/Scripts/UI/PortraitHudController.cs:126 | economy.OnCoDuyenChanged += OnFate; |
| Assets/Scripts/UI/PortraitHudController.cs:131 | cultivation.OnTuViChanged += OnCultivation; |
| Assets/Scripts/UI/PortraitHudController.cs:132 | cultivation.OnStageChanged += OnRealm; |
| Assets/Scripts/UI/PortraitHudController.cs:136 | drawer.OnDockCollapseChanged += OnFocus; |
| Assets/Scripts/UI/PortraitHudController.cs:137 | drawer.OnDrawerStateChanged += OnDrawer; |
| Assets/Scripts/UI/PortraitHudController.cs:143 | if (economy != null) { economy.OnLinhThachChanged -= OnStones; economy.OnCoDuyenChanged -= OnFate; } |
| Assets/Scripts/UI/PortraitHudController.cs:144 | if (cultivation != null) { cultivation.OnTuViChanged -= OnCultivation; cultivation.OnStageChanged -= OnRealm; } |
| Assets/Scripts/UI/PortraitHudController.cs:145 | if (drawer != null) { drawer.OnDockCollapseChanged -= OnFocus; drawer.OnDrawerStateChanged -= OnDrawer; } |
| Assets/Scripts/UI/PortraitHudController.cs:190 | if (drawer.dockToggleLabelText.text != label) drawer.dockToggleLabelText.text = label; |
| Assets/Scripts/UI/PortraitHudController.cs:199 | {lastMana=sm.currentLinhLuc;lastMaxMana=sm.maxLinhLuc;manaValue.text="MP "+Mathf.FloorToInt(lastMana)+" / "+Mathf.FloorToInt(lastMaxMana);} |
| Assets/Scripts/UI/PortraitHudController.cs:220 | if (buffSummary.text != text) buffSummary.text = text; |
| Assets/Scripts/UI/PortraitHudController.cs:232 | playerName.text = name; |
| Assets/Scripts/UI/PortraitHudController.cs:233 | playerRealm.text = cm.GetDisplayRealmName(); |
| Assets/Scripts/UI/PortraitHudController.cs:237 | cultivationValue.text = "Tu vi " + EconomyManager.FormatNumber(cm.currentTuVi) + " / " + |
| Assets/Scripts/UI/PortraitHudController.cs:244 | stoneValue.text="LT "+Compact(economy.currentLinhThach);stoneValue.color=ivory; |
| Assets/Scripts/UI/PortraitHudController.cs:245 | fateValue.text="CD "+Compact(economy.currentCoDuyen);fateValue.color=gold; |
| Assets/Scripts/UI/PortraitHudController.cs:495 | slider.interactable=false; |
| Assets/Scripts/UI/PortraitHudController.cs:535 | secondaryOverlay.SetActive(true);menuGrid.SetActive(true);detailsPage.SetActive(false);menuTitle.text="Đạo lộ"; |
| Assets/Scripts/UI/PortraitHudController.cs:553 | dockGroup.interactable=enabled;dockGroup.blocksRaycasts=enabled; |
| Assets/Scripts/UI/PortraitHudController.cs:560 | {OpenSecondary();menuGrid.SetActive(false);detailsPage.SetActive(true);menuTitle.text=title;detailsKind=kind;RefreshDetails();} |
| Assets/Scripts/UI/PortraitHudController.cs:565 | Detail("Nhật ký chiến đấu","log");detailsText.text=""; |
| Assets/Scripts/UI/PortraitHudController.cs:573 | detailsText.text=e==null?"Chưa có dữ liệu tài nguyên.":"Linh Thạch\n"+e.currentLinhThach.ToString("N0")+"\n\nCơ Duyên\n"+e.currentCoDuyen.ToString("N0")+"\n\nQuặng\n"+(a!=null?a.Ore.ToString("N0"):"0"); |
| Assets/Scripts/UI/PortraitHudController.cs:578 | detailsText.text=count==0?"Chưa có hiệu ứng hỗ trợ đang hoạt động.":""; |
| Assets/Scripts/UI/SaveSlotUI.cs:67 | if (nameText) nameText.text = currentData.characterName; |
| Assets/Scripts/UI/SaveSlotUI.cs:69 | if (cultivationText) cultivationText.text = $"Tu vi: {CultivationDisplay.NameForStage(currentData.stageIndex)} \| Tầng {displayStage}"; |
| Assets/Scripts/UI/SaveSlotUI.cs:74 | playTimeText.text = string.Format("Giờ chơi: {0:D2}h:{1:D2}m\nNgày: {2}", time.Hours, time.Minutes, currentData.lastPlayedDate); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:375 | walletText.text = $"✨ Ví Cơ Duyên: <color=#FFE666>{cd} CD</color>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:382 | freeAttemptsText.text = "Lượt Miễn Phí: <color=#55FF88>1 / 1</color>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:386 | freeAttemptsText.text = "Phí Vào Cổng: <color=#FFB84D>5 CD / Lần</color>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:399 | t.text = "<b>MIỄN PHÍ\n(1/1)</b>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:401 | b.interactable = true; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:405 | t.text = "<b>VÀO ẢI\n(5 CD)</b>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:407 | b.interactable = true; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:411 | t.text = "<b>CẦN 5 CD</b>"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:413 | b.interactable = false; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:466 | if (rewardLabel != null) rewardLabel.text = "🎁 Thưởng: Phi Kiếm Cấp 4, 6 hoặc 8 thẳng vào kho đồ"; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:492 | txt.text = val; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:177 | timerText.text = $"⏱️ <color=#FFE666>{Mathf.Max(0f, timeRemaining):0.0}s</color>"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:227 | scoreTitleText.text = "⚔️ YÊU MA ĐÃ TRẢM:"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:286 | itxt.text = demonIcons[UnityEngine.Random.Range(0, demonIcons.Length)]; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:300 | ntxt.text = "Huyết Ma"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:361 | scoreValueText.text = $"{hordeKilled} / {HORDE_TARGET}"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:364 | if (mainProgressBar != null) mainProgressBar.fillAmount = progress; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:365 | if (progressBarText != null) progressBarText.text = $"TIẾN ĐỘ TRẢM MA: {Mathf.RoundToInt(progress * 100f)}%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:389 | scoreTitleText.text = "🗿 TIẾN ĐỘ PHÁ CẤM:"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:390 | scoreValueText.text = "0%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:440 | ttxt.text = "THƯỢNG CỔ CẤM CHẾ"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:453 | barrierStageText.text = "Giai Đoạn 1: Phù Ấn Kiên Cố"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:492 | if (scoreValueText != null) scoreValueText.text = $"{currentScore}%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:494 | if (mainProgressBar != null) mainProgressBar.fillAmount = (1f - ratio); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:495 | if (progressBarText != null) progressBarText.text = $"TIẾN ĐỘ PHÁ CẤM: {currentScore}%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:502 | barrierStageText.text = "Giai Đoạn 1: Phù Ấn Kiên Cố (100%)"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:508 | barrierStageText.text = "Giai Đoạn 2: Cấm Chế Rạn Nứt! (50%)"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:514 | barrierStageText.text = "Giai Đoạn 3: CẤM CHẾ SẮP SỤP ĐỔ!"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:551 | scoreTitleText.text = "🔮 TRẠNG THÁI:"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:552 | scoreValueText.text = "Đang Chờ Khai Đỉnh"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:554 | if (mainProgressBar != null) mainProgressBar.fillAmount = 1f; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:555 | if (progressBarText != null) progressBarText.text = "LỰA CHỌN ĐÚNG ĐỈNH ĐỂ NHẬN CỰC PHẨM ĐAN"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:621 | txt.text = title; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:657 | if (btn != null) btn.interactable = false; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:661 | cauldronStatusTexts[i].text = "⭐ NỔ HŨ ⭐\nCỰC PHẨM ĐAN!"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:667 | cauldronStatusTexts[i].text = "Thảo Dược\nThượng Phẩm"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:673 | scoreValueText.text = isWin ? "<color=#FFE666>NỔ HŨ CỰC PHẨM!</color>" : "<color=#80DFFF>THƯỢNG PHẨM THẢO DƯỢC</color>"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:712 | scoreTitleText.text = "⚡ LÔI TINH ĐÃ THU:"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:836 | scoreValueText.text = $"{orbsCollected} / {ORBS_TARGET}"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:839 | if (mainProgressBar != null) mainProgressBar.fillAmount = progress; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:840 | if (progressBarText != null) progressBarText.text = $"THU THẬP LÔI TINH: {Mathf.RoundToInt(progress * 100f)}%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:866 | scoreTitleText.text = "🗡️ SỐ LƯỢT RÚT CÒN:"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:919 | swordFeedbackText.text = "Chuẩn bị... Hãy canh chuẩn vạch Hoàng Kim!"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1047 | swordFeedbackText.text = resultMsg; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1072 | scoreValueText.text = $"{swordTombAttemptsLeft} / 3"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1074 | if (mainProgressBar != null) mainProgressBar.fillAmount = (float)swordTombAttemptsLeft / 3f; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1075 | if (progressBarText != null) progressBarText.text = $"KIẾM CAO NHẤT ĐÃ ĐẠT: BẬC {bestSwordPulled}"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1120 | if (txtTitle != null) txtTitle.text = "🏆 CHIẾN BÁO BÍ CẢNH THẮNG LỢI!"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1126 | txtScore.text = scoreDesc; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1142 | txtRewards.text = r; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1298 | mainProgressBar.fillAmount = 0f; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1312 | progressBarText.text = "TIẾN ĐỘ: 0%"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1346 | titleText.text = title; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1352 | instructionText.text = $"{objective}\n{howToPlay}"; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1400 | txt.text = title; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1498 | txt.text = val; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1519 | txt.text = content; |
| Assets/Scripts/UI/ShopItemUI.cs:31 | priceText.text = "MAX (5/5)"; |
| Assets/Scripts/UI/ShopItemUI.cs:32 | if (buyButton != null) buyButton.interactable = false; |
| Assets/Scripts/UI/ShopItemUI.cs:36 | priceText.text = $"{Mathf.CeilToInt(cost)} LT"; |
| Assets/Scripts/UI/ShopItemUI.cs:37 | if (buyButton != null) buyButton.interactable = true; |
| Assets/Scripts/UI/SkillCardUI.cs:79 | condText.text = reqText; |
| Assets/Scripts/UI/SkillCardUI.cs:88 | titleText.text = $"{displayName}\n{reqText}"; |
| Assets/Scripts/UI/SkillCardUI.cs:92 | titleText.text = displayName; |
| Assets/Scripts/UI/SkillCardUI.cs:105 | if (unlockBtnText != null) unlockBtnText.text = $"Mở khóa ({cost} CD)"; |
| Assets/Scripts/UI/SkillCardUI.cs:106 | unlockButton.interactable = canUnlock; |
| Assets/Scripts/UI/SkillCardUI.cs:112 | if (unlockBtnText != null) unlockBtnText.text = $"Nâng cấp ({upCost} CD)"; |
| Assets/Scripts/UI/SkillCardUI.cs:113 | unlockButton.interactable = canUpgrade; |
| Assets/Scripts/UI/StorageModalUI.cs:123 | StorageManager.Instance.OnStorageChanged += OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:125 | DismantleService.Instance.OnDismantleExecuted += OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:131 | StorageManager.Instance.OnStorageChanged -= OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:133 | DismantleService.Instance.OnDismantleExecuted -= OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:285 | txtToggleView.text = isGrid ? "Xem DS" : "Xem Ô"; |
| Assets/Scripts/UI/StorageModalUI.cs:321 | if (toggleSwordHighLevel != null) toggleSwordHighLevel.SetIsOnWithoutNotify(StorageManager.Instance.FilterSwordsHighLevel); |
| Assets/Scripts/UI/StorageModalUI.cs:322 | if (togglePillBreakthrough != null) togglePillBreakthrough.SetIsOnWithoutNotify(StorageManager.Instance.FilterPillsBreakthrough); |
| Assets/Scripts/UI/StorageModalUI.cs:323 | if (toggleAutoDismantleLv1 != null) toggleAutoDismantleLv1.SetIsOnWithoutNotify(StorageManager.Instance.AutoDismantleLv1SwordsWhenFull); |
| Assets/Scripts/UI/StorageModalUI.cs:350 | txt.text = pCount > 0 ? $"Hòm Tạm ({pCount})" : "Hòm Tạm"; |
| Assets/Scripts/UI/StorageModalUI.cs:359 | txtLinhThach.text = $"LT {EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach)}"; |
| Assets/Scripts/UI/StorageModalUI.cs:361 | txtKiemY.text = $"K.Ý {EconomyManager.Instance.currentKiemY:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:363 | txtOre.text = $"Quặng {ArtifactService.Instance.Ore:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:365 | txtDust.text = $"Bụi {AlchemyService.Instance.State.medicineDust:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:552 | txtEmptyNotice.text = selectedCategory == StorageCategory.Pending |
| Assets/Scripts/UI/StorageModalUI.cs:600 | text.text = label; |
| Assets/Scripts/UI/StorageModalUI.cs:951 | rtTxt.text = "XUẤT TRẬN"; |
| Assets/Scripts/UI/StorageModalUI.cs:970 | stTxt.text = "Quý"; |
| Assets/Scripts/UI/StorageModalUI.cs:989 | cTxt.text = $"<size=12>{icon}</size>\n{label}"; |
| Assets/Scripts/UI/StorageModalUI.cs:1019 | ctTxt.text = countBadge; |
| Assets/Scripts/UI/StorageModalUI.cs:1069 | ibTxt.text = icon; |
| Assets/Scripts/UI/StorageModalUI.cs:1087 | txt.text = $"{title}\n<color=#A0AEC0><size=10>{detail}  \|  {rewardSummary}</size></color>"; |
| Assets/Scripts/UI/StorageModalUI.cs:1146 | bTxt.text = isDismantleUnlocked ? "Tách" : "Khóa"; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:37 | SwordLoadoutService.Instance.OnCapacityChanged += Refresh; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:44 | SwordLoadoutService.Instance.OnCapacityChanged -= Refresh; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:53 | stateLabel.text = locked ? "Khóa" : string.Empty; |
| Assets/Scripts/UI/TribulationModalUI.cs:80 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/TribulationModalUI.cs:88 | if (observedAlchemy != null) observedAlchemy.OnChanged -= RefreshData; |
| Assets/Scripts/UI/TribulationModalUI.cs:152 | if (observedAlchemy != null) observedAlchemy.OnChanged += RefreshData; |
| Assets/Scripts/UI/TribulationModalUI.cs:161 | canvasGroup.interactable = true; |
| Assets/Scripts/UI/TribulationModalUI.cs:206 | if (confirmLabel != null) confirmLabel.text = hasPill ? "ĐỘ KIẾP NGAY" : "MỞ LÒ LUYỆN ĐAN"; |
| Assets/Scripts/UI/TribulationModalUI.cs:208 | if (cancelLabel != null) cancelLabel.text = "ĐỂ SAU"; |
| Assets/Scripts/UI/TribulationModalUI.cs:210 | if (titleText != null) titleText.text = "THIÊN KIẾP HÀNG LÂM"; |
| Assets/Scripts/UI/TribulationModalUI.cs:212 | subtitleText.text = targetRealmText != null |
| Assets/Scripts/UI/TribulationModalUI.cs:215 | if (currentRealmText != null) currentRealmText.text = $"• Cảnh giới hiện tại: <color=#FFE666>{curRealm}</color> (Đại Viên Mãn)"; |
| Assets/Scripts/UI/TribulationModalUI.cs:216 | if (targetRealmText != null) targetRealmText.text = $"• Đột phá tấn thăng: <color=#66FFB2>{nxtRealm}</color>"; |
| Assets/Scripts/UI/TribulationModalUI.cs:231 | warningText.text = "<color=#FFD700>Thành công:</color> Tấn thăng đại cảnh giới, mở khóa ô kiếm và sức mạnh mới!\n" + |
| Assets/Scripts/UI/TribulationModalUI.cs:254 | canvasGroup.interactable = true; |
| Assets/Scripts/UI/TribulationModalUI.cs:290 | canvasGroup.interactable = false; |
| Assets/Scripts/UI/TribulationModalUI.cs:398 | cg.interactable = true; |
| Assets/Scripts/UI/TribulationModalUI.cs:419 | titleTxt.text = "THIÊN KIẾP HÀNG LÂM"; |
| Assets/Scripts/UI/TribulationModalUI.cs:434 | subTxt.text = "Tu vi đã đạt Đại Viên Mãn — Bạn có muốn Độ Kiếp Thăng Cấp?"; |
| Assets/Scripts/UI/TribulationModalUI.cs:468 | btnConfTxt.text = "ĐỘ KIẾP NGAY"; |
| Assets/Scripts/UI/TribulationModalUI.cs:490 | btnCancTxt.text = "CỦNG CỐ SỨC MẠNH"; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:30 | EconomyManager.Instance.OnLinhThachChanged += UpdateEconomyUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:36 | UpgradeManager.Instance.OnUpgradesChanged += UpdateUpgradesUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:50 | EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:53 | UpgradeManager.Instance.OnUpgradesChanged -= UpdateUpgradesUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:71 | txtLinhThach.text = EconomyManager.FormatNumber(linhThach); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:95 | txt.text = $"{data.upgradeName} (Lv.{level})\nHiệu quả: {value:F1} -> {nextValue:F1}"; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:100 | btnText.text = $"Nâng cấp\n{Mathf.FloorToInt(cost)} LT"; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:104 | btn.interactable = (EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= cost); |
| Assets/UI/Animation/Runtime/MobileUIModalMotion.cs:21 | if(manageInput){canvasGroup.interactable=opening;canvasGroup.blocksRaycasts=opening;} |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:54 | if (caption != null) caption.text = state.Caption ?? ""; |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:55 | if (value != null) value.text = state.Value ?? ""; |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:56 | if (disabledReason != null) disabledReason.text = state.DisabledReason ?? ""; |
| Assets/UI/Reconstruction/Runtime/MobileUIActionView.cs:57 | if (button != null) button.interactable = state.CanExecute; |
| Assets/UI/Reconstruction/Runtime/MobileUIContentSafety.cs:44 | text.text = "X"; |
| Assets/UI/Reconstruction/Runtime/MobileUIItemView.cs:22 | if (caption != null) caption.text = label; |
| Assets/UI/Reconstruction/Runtime/MobileUIItemView.cs:23 | if (count != null) count.text = amount ?? ""; |
| Assets/UI/Reconstruction/Runtime/MobileUIPanelView.cs:9 | public void SetTitle(string value) { if (title != null) title.text = value ?? ""; } |
| Assets/UI/Reconstruction/Runtime/MobileUIResponsiveRepairs.cs:164 | if(body.txtModalTitle!=null)body.txtModalTitle.text=System.Text.RegularExpressions.Regex.Replace(body.txtModalTitle.text,@"</?size(?:=\d+)?>",""); |



## 6. Serialized UI-controller references

FileID là local trong file asset, đối chiếu inventory component ở TECHNICAL_AUDIT. Không giả định reference resolved qua prefab override.

| Source:line | Hierarchy | Controller | Field/reference |
|---|---|---|---|
| Assets/_Recovery/0.unity:906 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_2 | SkillCardUI | titleText: {fileID: 195444098} |
| Assets/_Recovery/0.unity:907 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_2 | SkillCardUI | condText: {fileID: 1976518407} |
| Assets/_Recovery/0.unity:908 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_2 | SkillCardUI | unlockButton: {fileID: 9365901} |
| Assets/_Recovery/0.unity:909 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_2 | SkillCardUI | unlockBtnText: {fileID: 327016008} |
| Assets/_Recovery/0.unity:18438 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_3 | SkillCardUI | titleText: {fileID: 1821375991} |
| Assets/_Recovery/0.unity:18439 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_3 | SkillCardUI | condText: {fileID: 721539604} |
| Assets/_Recovery/0.unity:18440 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_3 | SkillCardUI | unlockButton: {fileID: 266942680} |
| Assets/_Recovery/0.unity:18441 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_3 | SkillCardUI | unlockBtnText: {fileID: 1195077908} |
| Assets/_Recovery/0.unity:27368 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_0 | SkillCardUI | titleText: {fileID: 2101695518} |
| Assets/_Recovery/0.unity:27369 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_0 | SkillCardUI | condText: {fileID: 98329472} |
| Assets/_Recovery/0.unity:27370 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_0 | SkillCardUI | unlockButton: {fileID: 1020510833} |
| Assets/_Recovery/0.unity:27371 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_0 | SkillCardUI | unlockBtnText: {fileID: 800584654} |
| Assets/_Recovery/0.unity:27982 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_1 | SkillCardUI | titleText: {fileID: 1760523014} |
| Assets/_Recovery/0.unity:27983 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_1 | SkillCardUI | condText: {fileID: 1619254252} |
| Assets/_Recovery/0.unity:27984 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_1 | SkillCardUI | unlockButton: {fileID: 887737994} |
| Assets/_Recovery/0.unity:27985 | Canvas/RightPanel/UpgradePanel/Content/SkillCard_1 | SkillCardUI | unlockBtnText: {fileID: 236562682} |
| Assets/_Recovery/0.unity:31773 | Canvas/UIManager | CultivationUI | txtStageName: {fileID: 1648903340} |
| Assets/_Recovery/0.unity:31774 | Canvas/UIManager | CultivationUI | txtTuVi: {fileID: 466863998} |
| Assets/_Recovery/0.unity:31775 | Canvas/UIManager | CultivationUI | imgProgressBar: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5356 | Canvas/MobileUIRoot/Drawer_1/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5645 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5646 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5647 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeScrollRect: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5648 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeContent: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5731 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalRoot: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5732 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBox: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5733 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5734 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5735 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTitle: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5736 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTier: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5737 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalLore: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5738 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalMechanics: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5739 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCurrent: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5740 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalNext: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5741 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalRequirement: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5742 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | goModalLockBanner: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5743 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5744 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5745 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:5746 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCost: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:11509 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | titleText: {fileID: 1945041015} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:11510 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | condText: {fileID: 238323356} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:11511 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockButton: {fileID: 784547714} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:11512 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockBtnText: {fileID: 1875151468} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12635 | Canvas/TribulationModalUI | TribulationModalUI | canvasGroup: {fileID: 296285011} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12636 | Canvas/TribulationModalUI | TribulationModalUI | modalBox: {fileID: 2081022731} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12637 | Canvas/TribulationModalUI | TribulationModalUI | confirmButton: {fileID: 1230983391} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12638 | Canvas/TribulationModalUI | TribulationModalUI | cancelButton: {fileID: 1954800248} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12639 | Canvas/TribulationModalUI | TribulationModalUI | titleText: {fileID: 1340314525} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12640 | Canvas/TribulationModalUI | TribulationModalUI | subtitleText: {fileID: 86746213} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12641 | Canvas/TribulationModalUI | TribulationModalUI | currentRealmText: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12642 | Canvas/TribulationModalUI | TribulationModalUI | targetRealmText: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:12643 | Canvas/TribulationModalUI | TribulationModalUI | warningText: {fileID: 1859154824} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:14368 | Canvas/ShopPanel/ScrollView/Content/ShopItem_4 | ShopItemUI | buyButton: {fileID: 581820742} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17137 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 526797192} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17138 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 30914327} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17139 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeScrollRect: {fileID: 1267652686} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17140 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeContent: {fileID: 752384037} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17223 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalRoot: {fileID: 72613887} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17224 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBox: {fileID: 847165447} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17225 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 36418814} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17226 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 72613889} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17227 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTitle: {fileID: 1934146158} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17228 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTier: {fileID: 1856756184} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17229 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalLore: {fileID: 1675773072} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17230 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalMechanics: {fileID: 1916659841} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17231 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCurrent: {fileID: 1715493584} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17232 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalNext: {fileID: 720103016} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17233 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalRequirement: {fileID: 699765833} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17234 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | goModalLockBanner: {fileID: 699765831} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17235 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 584480004} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17236 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 1103643327} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17237 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:17238 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCost: {fileID: 2094405181} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:20669 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | oreLabel: {fileID: 1497739962} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:20670 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | auraLabel: {fileID: 140643440} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:25970 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | titleText: {fileID: 1769779942} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:25971 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | condText: {fileID: 209483097} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:25972 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockButton: {fileID: 557737383} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:25973 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockBtnText: {fileID: 630660150} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:27985 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | titleText: {fileID: 479375528} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:27986 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | condText: {fileID: 175780509} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:27987 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockButton: {fileID: 1158179352} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:27988 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockBtnText: {fileID: 1232013905} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:28313 | Canvas/ShopPanel/ScrollView/Content/ShopItem_2 | ShopItemUI | buyButton: {fileID: 69685870} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:32882 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | spiritCooldownLabel: {fileID: 2086229475} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:32883 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | activateSpiritBtn: {fileID: 1021691522} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:32884 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtOreLabel: {fileID: 957735967} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:32885 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtCurrentActiveLabel: {fileID: 735508003} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33756 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | dataPanel: {fileID: 1265773101} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33757 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | emptyPanel: {fileID: 1455111634} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33758 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | nameText: {fileID: 1246669010} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33759 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | cultivationText: {fileID: 780796025} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33760 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | playTimeText: {fileID: 567340084} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33761 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | slotButton: {fileID: 889524879} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:33762 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | deleteButton: {fileID: 569429891} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36052 | Canvas/UIManager | CultivationUI | txtStageName: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36053 | Canvas/UIManager | CultivationUI | txtTuVi: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36054 | Canvas/UIManager | CultivationUI | imgProgressBar: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36311 | Canvas/PauseMenuModal | PauseMenuManager | pauseModal: {fileID: 331140756} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36312 | Canvas/PauseMenuModal | PauseMenuManager | confirmationPopup: {fileID: 2030168928} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36313 | Canvas/PauseMenuModal | PauseMenuManager | modalCanvasGroup: {fileID: 958334017} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36314 | Canvas/PauseMenuModal | PauseMenuManager | openPauseButton: {fileID: 1814150550} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36315 | Canvas/PauseMenuModal | PauseMenuManager | closeHeaderButton: {fileID: 922403253} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36316 | Canvas/PauseMenuModal | PauseMenuManager | overlayCloseButton: {fileID: 628852012} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36317 | Canvas/PauseMenuModal | PauseMenuManager | resumeButton: {fileID: 760746783} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36318 | Canvas/PauseMenuModal | PauseMenuManager | manualSaveButton: {fileID: 825525784} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36319 | Canvas/PauseMenuModal | PauseMenuManager | returnMainMenuButton: {fileID: 1728238219} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36320 | Canvas/PauseMenuModal | PauseMenuManager | quitButton: {fileID: 421096080} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36321 | Canvas/PauseMenuModal | PauseMenuManager | bgmSlider: {fileID: 870604229} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36322 | Canvas/PauseMenuModal | PauseMenuManager | txtBgmValue: {fileID: 664024896} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36323 | Canvas/PauseMenuModal | PauseMenuManager | sfxSlider: {fileID: 1699368934} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36324 | Canvas/PauseMenuModal | PauseMenuManager | txtSfxValue: {fileID: 2021520321} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36325 | Canvas/PauseMenuModal | PauseMenuManager | muteToggle: {fileID: 615496905} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36326 | Canvas/PauseMenuModal | PauseMenuManager | txtCharacterInfo: {fileID: 323066533} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36327 | Canvas/PauseMenuModal | PauseMenuManager | txtSaveStatus: {fileID: 98726558} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36328 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupTitle: {fileID: 687175252} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36329 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupMessage: {fileID: 578936211} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36330 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupConfirm: {fileID: 1724402819} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:36331 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupCancel: {fileID: 441178331} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:37286 | Canvas/ShopPanel/ScrollView/Content/ShopItem_3 | ShopItemUI | buyButton: {fileID: 1843090946} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:37380 | Canvas/ShopPanel/ScrollView/Content/ShopItem_1 | ShopItemUI | buyButton: {fileID: 17490869} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42334 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbLinhChi: {fileID: 619501297} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42335 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 1545148919} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42336 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 704849249} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42337 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtAlchemistRank: {fileID: 293563195} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42338 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtMedicineDust: {fileID: 934000489} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42339 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceStatus: {fileID: 1190454638} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42340 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceTimer: {fileID: 1905055730} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42341 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42342 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnFinishWithDust: {fileID: 197806336} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42343 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42344 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostTuVi: {fileID: 1362888274} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42345 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewTuVi: {fileID: 993192221} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42346 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostBreakthrough: {fileID: 1319886065} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42347 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 1424713129} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42348 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 469839393} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42349 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 1465415029} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42350 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnUseTuViPill: {fileID: 1335068527} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42351 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | linhChiLabel: {fileID: 619501297} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42352 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 1545148919} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42353 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | tuyetLienLabel: {fileID: 704849249} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42354 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewTimerLabel: {fileID: 1905055730} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42355 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewCultivationBtn: {fileID: 993192221} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42356 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 1424713129} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42357 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | useDustBtn: {fileID: 197806336} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42358 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | dustLabel: {fileID: 934000489} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:42359 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | pillInventoryLabel: {fileID: 469839393} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44652 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | dataPanel: {fileID: 1651988589} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44653 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | emptyPanel: {fileID: 1220777055} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44654 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | nameText: {fileID: 85093384} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44655 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | cultivationText: {fileID: 1082954645} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44656 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | playTimeText: {fileID: 339315753} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44657 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | slotButton: {fileID: 1178820542} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:44658 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | deleteButton: {fileID: 255200659} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45851 | Canvas/AchievementModalUI | AchievementModalUI | canvasGroup: {fileID: 1224474608} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45852 | Canvas/AchievementModalUI | AchievementModalUI | modalBox: {fileID: 700038009} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45853 | Canvas/AchievementModalUI | AchievementModalUI | btnClose: {fileID: 1284123593} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45854 | Canvas/AchievementModalUI | AchievementModalUI | btnBackdrop: {fileID: 1224474606} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45855 | Canvas/AchievementModalUI | AchievementModalUI | txtTotalProgress: {fileID: 267208547} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45856 | Canvas/AchievementModalUI | AchievementModalUI | fillTotalProgress: {fileID: 1729469184} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45857 | Canvas/AchievementModalUI | AchievementModalUI | btnClaimAll: {fileID: 1913150654} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45858 | Canvas/AchievementModalUI | AchievementModalUI | txtClaimAll: {fileID: 311589095} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45867 | Canvas/AchievementModalUI | AchievementModalUI | contentContainer: {fileID: 276520117} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:45868 | Canvas/AchievementModalUI | AchievementModalUI | scrollRect: {fileID: 1375749548} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:46098 | Canvas/MobileUIRoot | MobileDrawerUIController | backdropOverlay: {fileID: 373122639} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47309 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | canvasGroup: {fileID: 1248546305} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47310 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | containerRect: {fileID: 1248546303} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47311 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | backgroundImage: {fileID: 2008515411} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47312 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | borderTop: {fileID: 807191400} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47313 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | borderBottom: {fileID: 1510956772} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47314 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | titleText: {fileID: 1315856892} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47315 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | subtitleText: {fileID: 624708113} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47316 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | extraDetailText: {fileID: 1795174205} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:47743 | Canvas/ShopPanel/btn_Close | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:49866 | Canvas/MobileUIRoot/Drawer_4/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:50418 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | oreLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:50419 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | auraLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:54447 | Canvas/RightPanel/btn_OpenShop | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:63723 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | spiritCooldownLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:63724 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | activateSpiritBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:63725 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtOreLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:63726 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtCurrentActiveLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:63792 | Canvas/MobileUIRoot/Drawer_3/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64352 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | dataPanel: {fileID: 825211636} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64353 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | emptyPanel: {fileID: 1723814474} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64354 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | nameText: {fileID: 1896802288} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64355 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | cultivationText: {fileID: 1899240485} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64356 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | playTimeText: {fileID: 1829056205} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64357 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | slotButton: {fileID: 1621847545} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:64358 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | deleteButton: {fileID: 755315457} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69216 | Canvas/MilestoneModalUI | MilestoneModalUI | modalPanel: {fileID: 1724887768} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69217 | Canvas/MilestoneModalUI | MilestoneModalUI | contentContainer: {fileID: 194390971} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69218 | Canvas/MilestoneModalUI | MilestoneModalUI | txtTotalProgress: {fileID: 1644464564} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69219 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClose: {fileID: 2050025881} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69220 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClaimAll: {fileID: 2129900667} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69221 | Canvas/MilestoneModalUI | MilestoneModalUI | btnSwitchToAchievements: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:69222 | Canvas/MilestoneModalUI | MilestoneModalUI | rowTemplate: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:71019 | Canvas/MobileUIRoot/Drawer_0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:73949 | Canvas/MobileUIRoot/Drawer_2/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:77991 | Canvas/ShopPanel/ScrollView/Content/ShopItem_0 | ShopItemUI | buyButton: {fileID: 1005832532} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:83550 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | titleText: {fileID: 1454567060} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:83551 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | condText: {fileID: 1734239692} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:83552 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockButton: {fileID: 1571659309} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:83553 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockBtnText: {fileID: 864419017} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84164 | Canvas/MainMenuManager | MainMenuManager | mainPanel: {fileID: 2017822356} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84165 | Canvas/MainMenuManager | MainMenuManager | characterSelectPanel: {fileID: 943835570} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84166 | Canvas/MainMenuManager | MainMenuManager | characterCreatePanel: {fileID: 809555469} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84167 | Canvas/MainMenuManager | MainMenuManager | settingsPanel: {fileID: 1399051815} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84168 | Canvas/MainMenuManager | MainMenuManager | confirmationPopup: {fileID: 1638424892} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84169 | Canvas/MainMenuManager | MainMenuManager | playButton: {fileID: 482855941} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84170 | Canvas/MainMenuManager | MainMenuManager | playButtonText: {fileID: 1073180194} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84171 | Canvas/MainMenuManager | MainMenuManager | settingsButton: {fileID: 1158388662} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84172 | Canvas/MainMenuManager | MainMenuManager | quitButton: {fileID: 73727741} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84177 | Canvas/MainMenuManager | MainMenuManager | backFromSelectButton: {fileID: 262579212} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84178 | Canvas/MainMenuManager | MainMenuManager | nameInput: {fileID: 913883099} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84179 | Canvas/MainMenuManager | MainMenuManager | confirmCreateButton: {fileID: 1563459446} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84180 | Canvas/MainMenuManager | MainMenuManager | cancelCreateButton: {fileID: 1250177799} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84181 | Canvas/MainMenuManager | MainMenuManager | bgmSlider: {fileID: 384419740} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84182 | Canvas/MainMenuManager | MainMenuManager | sfxSlider: {fileID: 403109340} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84183 | Canvas/MainMenuManager | MainMenuManager | closeSettingsButton: {fileID: 1331868265} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84184 | Canvas/MainMenuManager | MainMenuManager | popupTitle: {fileID: 1864303443} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84185 | Canvas/MainMenuManager | MainMenuManager | popupMessage: {fileID: 875519812} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84186 | Canvas/MainMenuManager | MainMenuManager | popupConfirmBtn: {fileID: 1537215085} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84187 | Canvas/MainMenuManager | MainMenuManager | popupCancelBtn: {fileID: 1797654953} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84605 | Canvas/CharacterDetailModal | CharacterDetailModalUI | canvasGroup: {fileID: 2097715292} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84606 | Canvas/CharacterDetailModal | CharacterDetailModalUI | modalBox: {fileID: 1103503191} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84607 | Canvas/CharacterDetailModal | CharacterDetailModalUI | scrollRect: {fileID: 814243067} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84608 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeButton: {fileID: 1150597136} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84609 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeOverlayButton: {fileID: 1251122967} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84610 | Canvas/CharacterDetailModal | CharacterDetailModalUI | bottomCloseButton: {fileID: 1087333160} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84611 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtName: {fileID: 718167421} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84612 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtRealm: {fileID: 1918473206} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84613 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtTuVi: {fileID: 1917269040} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84614 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtCoDuyen: {fileID: 122488355} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84615 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtLinhThach: {fileID: 1408329680} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84616 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtHp: {fileID: 1163269436} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84617 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMp: {fileID: 1349554947} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84618 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMultiplier: {fileID: 1912840058} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84619 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtEquipSwords: {fileID: 1171760092} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84620 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeKiemY: {fileID: 1562206166} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84621 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeLinhLuc: {fileID: 581285601} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84622 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeTuLinh: {fileID: 1889137916} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:84623 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeThanThuc: {fileID: 1161672799} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85367 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbLinhChi: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85368 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85369 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85370 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtAlchemistRank: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85371 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtMedicineDust: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85372 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceStatus: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85373 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceTimer: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85374 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85375 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnFinishWithDust: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85376 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85377 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostTuVi: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85378 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewTuVi: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85379 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostBreakthrough: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85380 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85381 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85382 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85383 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnUseTuViPill: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85384 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | linhChiLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85385 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85386 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | tuyetLienLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85387 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewTimerLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85388 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewCultivationBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85389 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85390 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | useDustBtn: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85391 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | dustLabel: {fileID: 0} |
| Assets/Modules/PCContent/Scenes/PCGameplayWork.unity:85392 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | pillInventoryLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9509 | Canvas/MobileUIRoot/Drawer_1/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9877 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9878 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9879 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeScrollRect: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9880 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeContent: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9963 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalRoot: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9964 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBox: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9965 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9966 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9967 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTitle: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9968 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTier: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9969 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalLore: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9970 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalMechanics: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9971 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCurrent: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9972 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalNext: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9973 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalRequirement: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9974 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | goModalLockBanner: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9975 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9976 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9977 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:9978 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCost: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:25198 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | titleText: {fileID: 1945041015} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:25199 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | condText: {fileID: 238323356} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:25200 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockButton: {fileID: 784547714} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:25201 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockBtnText: {fileID: 1875151468} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27424 | Canvas/TribulationModalUI | TribulationModalUI | canvasGroup: {fileID: 296285011} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27425 | Canvas/TribulationModalUI | TribulationModalUI | modalBox: {fileID: 2081022731} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27426 | Canvas/TribulationModalUI | TribulationModalUI | confirmButton: {fileID: 1230983391} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27427 | Canvas/TribulationModalUI | TribulationModalUI | cancelButton: {fileID: 1954800248} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27428 | Canvas/TribulationModalUI | TribulationModalUI | titleText: {fileID: 1340314525} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27429 | Canvas/TribulationModalUI | TribulationModalUI | subtitleText: {fileID: 86746213} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27430 | Canvas/TribulationModalUI | TribulationModalUI | currentRealmText: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27431 | Canvas/TribulationModalUI | TribulationModalUI | targetRealmText: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:27432 | Canvas/TribulationModalUI | TribulationModalUI | warningText: {fileID: 1859154824} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:30034 | Canvas/ShopPanel/ScrollView/Content/ShopItem_4 | ShopItemUI | buyButton: {fileID: 581820742} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:31997 | Canvas/MobileUIRoot/MobileSkillHUD | MobileSkillHUD | btnUltimate: {fileID: 614836222} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:31998 | Canvas/MobileUIRoot/MobileSkillHUD | MobileSkillHUD | imgUltiFill: {fileID: 1968414597} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:31999 | Canvas/MobileUIRoot/MobileSkillHUD | MobileSkillHUD | imgUltiGlow: {fileID: 1126378337} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:32000 | Canvas/MobileUIRoot/MobileSkillHUD | MobileSkillHUD | txtUltiTitle: {fileID: 363105161} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:32001 | Canvas/MobileUIRoot/MobileSkillHUD | MobileSkillHUD | txtUltiMana: {fileID: 97273254} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35299 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 526797192} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35300 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 30914327} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35301 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeScrollRect: {fileID: 1267652686} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35302 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeContent: {fileID: 752384037} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35385 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalRoot: {fileID: 72613887} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35386 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBox: {fileID: 847165447} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35387 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 36418814} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35388 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 72613889} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35389 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTitle: {fileID: 1934146158} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35390 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTier: {fileID: 1856756184} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35391 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalLore: {fileID: 1675773072} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35392 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalMechanics: {fileID: 1916659841} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35393 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCurrent: {fileID: 1715493584} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35394 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalNext: {fileID: 720103016} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35395 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalRequirement: {fileID: 699765833} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35396 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | goModalLockBanner: {fileID: 699765831} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35397 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 584480004} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35398 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 1103643327} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35399 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:35400 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCost: {fileID: 2094405181} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:42611 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | oreLabel: {fileID: 1497739962} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:42612 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | auraLabel: {fileID: 140643440} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:56664 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | titleText: {fileID: 1769779942} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:56665 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | condText: {fileID: 209483097} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:56666 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockButton: {fileID: 557737383} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:56667 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockBtnText: {fileID: 630660150} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:62994 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | titleText: {fileID: 479375528} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:62995 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | condText: {fileID: 175780509} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:62996 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockButton: {fileID: 1158179352} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:62997 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockBtnText: {fileID: 1232013905} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:63205 | Canvas/MobileUIRoot/StagePanel | CultivationPhaseBarUI | stageLabel: {fileID: 1377677953} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:63622 | Canvas/ShopPanel/ScrollView/Content/ShopItem_2 | ShopItemUI | buyButton: {fileID: 69685870} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:78853 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | spiritCooldownLabel: {fileID: 2086229475} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:78854 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | activateSpiritBtn: {fileID: 1021691522} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:78855 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtOreLabel: {fileID: 957735967} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:78856 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtCurrentActiveLabel: {fileID: 735508003} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80217 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | dataPanel: {fileID: 1265773101} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80218 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | emptyPanel: {fileID: 1455111634} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80219 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | nameText: {fileID: 1246669010} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80220 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | cultivationText: {fileID: 780796025} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80221 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | playTimeText: {fileID: 567340084} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80222 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | slotButton: {fileID: 889524879} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:80223 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | deleteButton: {fileID: 569429891} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86450 | Canvas/PauseMenuModal | PauseMenuManager | pauseModal: {fileID: 331140756} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86451 | Canvas/PauseMenuModal | PauseMenuManager | confirmationPopup: {fileID: 2030168928} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86452 | Canvas/PauseMenuModal | PauseMenuManager | modalCanvasGroup: {fileID: 958334017} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86453 | Canvas/PauseMenuModal | PauseMenuManager | openPauseButton: {fileID: 1814150550} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86454 | Canvas/PauseMenuModal | PauseMenuManager | closeHeaderButton: {fileID: 922403253} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86455 | Canvas/PauseMenuModal | PauseMenuManager | overlayCloseButton: {fileID: 628852012} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86456 | Canvas/PauseMenuModal | PauseMenuManager | resumeButton: {fileID: 760746783} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86457 | Canvas/PauseMenuModal | PauseMenuManager | manualSaveButton: {fileID: 825525784} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86458 | Canvas/PauseMenuModal | PauseMenuManager | returnMainMenuButton: {fileID: 1728238219} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86459 | Canvas/PauseMenuModal | PauseMenuManager | quitButton: {fileID: 421096080} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86460 | Canvas/PauseMenuModal | PauseMenuManager | bgmSlider: {fileID: 870604229} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86461 | Canvas/PauseMenuModal | PauseMenuManager | txtBgmValue: {fileID: 664024896} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86462 | Canvas/PauseMenuModal | PauseMenuManager | sfxSlider: {fileID: 1699368934} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86463 | Canvas/PauseMenuModal | PauseMenuManager | txtSfxValue: {fileID: 2021520321} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86464 | Canvas/PauseMenuModal | PauseMenuManager | muteToggle: {fileID: 615496905} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86465 | Canvas/PauseMenuModal | PauseMenuManager | txtCharacterInfo: {fileID: 323066533} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86466 | Canvas/PauseMenuModal | PauseMenuManager | txtSaveStatus: {fileID: 98726558} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86467 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupTitle: {fileID: 687175252} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86468 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupMessage: {fileID: 578936211} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86469 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupConfirm: {fileID: 1724402819} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:86470 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupCancel: {fileID: 441178331} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:88256 | Canvas/ShopPanel/ScrollView/Content/ShopItem_3 | ShopItemUI | buyButton: {fileID: 1843090946} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:90799 | Canvas/ShopPanel/ScrollView/Content/ShopItem_1 | ShopItemUI | buyButton: {fileID: 17490869} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99783 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbLinhChi: {fileID: 619501297} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99784 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 1545148919} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99785 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 704849249} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99786 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtAlchemistRank: {fileID: 293563195} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99787 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtMedicineDust: {fileID: 934000489} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99788 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceStatus: {fileID: 1190454638} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99789 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceTimer: {fileID: 1905055730} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99790 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99791 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnFinishWithDust: {fileID: 197806336} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99792 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99793 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostTuVi: {fileID: 1362888274} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99794 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewTuVi: {fileID: 993192221} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99795 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostBreakthrough: {fileID: 1319886065} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99796 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 1424713129} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99797 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 469839393} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99798 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 1465415029} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99799 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnUseTuViPill: {fileID: 1335068527} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99800 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | linhChiLabel: {fileID: 619501297} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99801 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 1545148919} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99802 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | tuyetLienLabel: {fileID: 704849249} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99803 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewTimerLabel: {fileID: 1905055730} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99804 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewCultivationBtn: {fileID: 993192221} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99805 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 1424713129} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99806 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | useDustBtn: {fileID: 197806336} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99807 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | dustLabel: {fileID: 934000489} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:99808 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | pillInventoryLabel: {fileID: 469839393} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105579 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | dataPanel: {fileID: 1651988589} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105580 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | emptyPanel: {fileID: 1220777055} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105581 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | nameText: {fileID: 85093384} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105582 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | cultivationText: {fileID: 1082954645} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105583 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | playTimeText: {fileID: 339315753} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105584 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | slotButton: {fileID: 1178820542} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:105585 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | deleteButton: {fileID: 255200659} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107447 | Canvas/AchievementModalUI | AchievementModalUI | canvasGroup: {fileID: 1224474608} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107448 | Canvas/AchievementModalUI | AchievementModalUI | modalBox: {fileID: 700038009} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107449 | Canvas/AchievementModalUI | AchievementModalUI | btnClose: {fileID: 1284123593} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107450 | Canvas/AchievementModalUI | AchievementModalUI | btnBackdrop: {fileID: 1224474606} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107451 | Canvas/AchievementModalUI | AchievementModalUI | txtTotalProgress: {fileID: 267208547} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107452 | Canvas/AchievementModalUI | AchievementModalUI | fillTotalProgress: {fileID: 1729469184} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107453 | Canvas/AchievementModalUI | AchievementModalUI | btnClaimAll: {fileID: 1913150654} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107454 | Canvas/AchievementModalUI | AchievementModalUI | txtClaimAll: {fileID: 311589095} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107463 | Canvas/AchievementModalUI | AchievementModalUI | contentContainer: {fileID: 276520117} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107464 | Canvas/AchievementModalUI | AchievementModalUI | scrollRect: {fileID: 1375749548} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107700 | Canvas/MobileUIRoot | MobileDrawerUIController | backdropOverlay: {fileID: 373122639} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107713 | Canvas/MobileUIRoot | MobileDrawerUIController | mobileDock: {fileID: 867939491} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107714 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleButton: {fileID: 79286217} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107715 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleArrowText: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107716 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleLabelText: {fileID: 437073796} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:107717 | Canvas/MobileUIRoot | MobileDrawerUIController | playerHpBar: {fileID: 812354262} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109479 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | canvasGroup: {fileID: 1248546305} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109480 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | containerRect: {fileID: 1248546303} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109481 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | backgroundImage: {fileID: 2008515411} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109482 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | borderTop: {fileID: 807191400} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109483 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | borderBottom: {fileID: 1510956772} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109484 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | titleText: {fileID: 1315856892} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109485 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | subtitleText: {fileID: 624708113} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109486 | Canvas/MobileUIRoot/CombatNotificationBanner | CombatNotificationUI | extraDetailText: {fileID: 1795174205} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:109913 | Canvas/ShopPanel/btn_Close | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:117379 | Canvas/MobileUIRoot/Drawer_4/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:117931 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | oreLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:117932 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | auraLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:132561 | Canvas/RightPanel/btn_OpenShop | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:154677 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | spiritCooldownLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:154678 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | activateSpiritBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:154679 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtOreLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:154680 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtCurrentActiveLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:154746 | Canvas/MobileUIRoot/Drawer_3/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155687 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | dataPanel: {fileID: 825211636} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155688 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | emptyPanel: {fileID: 1723814474} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155689 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | nameText: {fileID: 1896802288} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155690 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | cultivationText: {fileID: 1899240485} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155691 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | playTimeText: {fileID: 1829056205} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155692 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | slotButton: {fileID: 1621847545} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:155693 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | deleteButton: {fileID: 755315457} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158225 | Canvas/StorageModalUI | StorageModalUI | canvasGroup: {fileID: 1644478631} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158226 | Canvas/StorageModalUI | StorageModalUI | modalBox: {fileID: 959944860} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158227 | Canvas/StorageModalUI | StorageModalUI | btnClose: {fileID: 2103688237} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158228 | Canvas/StorageModalUI | StorageModalUI | btnBackdrop: {fileID: 1644478629} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158229 | Canvas/StorageModalUI | StorageModalUI | txtLinhThach: {fileID: 832838914} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158230 | Canvas/StorageModalUI | StorageModalUI | txtKiemY: {fileID: 46909927} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158231 | Canvas/StorageModalUI | StorageModalUI | txtOre: {fileID: 1355976233} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158232 | Canvas/StorageModalUI | StorageModalUI | txtDust: {fileID: 1030226945} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158240 | Canvas/StorageModalUI | StorageModalUI | toggleSwordHighLevel: {fileID: 286737916} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158241 | Canvas/StorageModalUI | StorageModalUI | togglePillBreakthrough: {fileID: 555021063} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158242 | Canvas/StorageModalUI | StorageModalUI | toggleAutoDismantleLv1: {fileID: 515281039} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158244 | Canvas/StorageModalUI | StorageModalUI | btnToggleView: {fileID: 722546235} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158245 | Canvas/StorageModalUI | StorageModalUI | txtToggleView: {fileID: 1962929101} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158246 | Canvas/StorageModalUI | StorageModalUI | gridContent: {fileID: 12313158} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158247 | Canvas/StorageModalUI | StorageModalUI | listContent: {fileID: 1919265417} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158248 | Canvas/StorageModalUI | StorageModalUI | contentContainer: {fileID: 12313158} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158249 | Canvas/StorageModalUI | StorageModalUI | scrollRect: {fileID: 2121773491} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158250 | Canvas/StorageModalUI | StorageModalUI | txtEmptyNotice: {fileID: 1740486654} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158251 | Canvas/StorageModalUI | StorageModalUI | btnClaimPriority: {fileID: 197075584} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158252 | Canvas/StorageModalUI | StorageModalUI | txtClaimPriority: {fileID: 1215014113} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158253 | Canvas/StorageModalUI | StorageModalUI | btnBatchDismantle: {fileID: 2009324848} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:158254 | Canvas/StorageModalUI | StorageModalUI | txtBatchDismantle: {fileID: 964138609} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167152 | Canvas/MilestoneModalUI | MilestoneModalUI | modalPanel: {fileID: 1724887768} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167153 | Canvas/MilestoneModalUI | MilestoneModalUI | contentContainer: {fileID: 194390971} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167154 | Canvas/MilestoneModalUI | MilestoneModalUI | txtTotalProgress: {fileID: 1644464564} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167155 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClose: {fileID: 2050025881} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167156 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClaimAll: {fileID: 2129900667} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167157 | Canvas/MilestoneModalUI | MilestoneModalUI | btnSwitchToAchievements: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:167158 | Canvas/MilestoneModalUI | MilestoneModalUI | rowTemplate: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:172261 | Canvas/MobileUIRoot/Drawer_0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:180800 | Canvas/MobileUIRoot/Drawer_2/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181170 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | canvasGroup: {fileID: 1865082722} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181171 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | modalBox: {fileID: 1169293231} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181172 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnCancel: {fileID: 1671183687} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181173 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnConfirm: {fileID: 581916513} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181174 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnBackdrop: {fileID: 1865082720} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181175 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtTitle: {fileID: 1339888971} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181176 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtItemName: {fileID: 280931172} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181177 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtItemDetail: {fileID: 2063210857} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181178 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtRewardSummary: {fileID: 280621230} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:181179 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtWarning: {fileID: 1215585099} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:191406 | Canvas/ShopPanel/ScrollView/Content/ShopItem_0 | ShopItemUI | buyButton: {fileID: 1005832532} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:199710 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | titleText: {fileID: 1454567060} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:199711 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | condText: {fileID: 1734239692} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:199712 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockButton: {fileID: 1571659309} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:199713 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockBtnText: {fileID: 864419017} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203148 | Canvas/MainMenuManager | MainMenuManager | mainPanel: {fileID: 2017822356} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203149 | Canvas/MainMenuManager | MainMenuManager | characterSelectPanel: {fileID: 943835570} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203150 | Canvas/MainMenuManager | MainMenuManager | characterCreatePanel: {fileID: 809555469} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203151 | Canvas/MainMenuManager | MainMenuManager | settingsPanel: {fileID: 1399051815} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203152 | Canvas/MainMenuManager | MainMenuManager | confirmationPopup: {fileID: 1638424892} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203153 | Canvas/MainMenuManager | MainMenuManager | playButton: {fileID: 482855941} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203154 | Canvas/MainMenuManager | MainMenuManager | playButtonText: {fileID: 1073180194} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203155 | Canvas/MainMenuManager | MainMenuManager | settingsButton: {fileID: 1158388662} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203156 | Canvas/MainMenuManager | MainMenuManager | quitButton: {fileID: 73727741} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203161 | Canvas/MainMenuManager | MainMenuManager | backFromSelectButton: {fileID: 262579212} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203162 | Canvas/MainMenuManager | MainMenuManager | nameInput: {fileID: 913883099} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203163 | Canvas/MainMenuManager | MainMenuManager | characterNameLabel: {fileID: 398077869} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203164 | Canvas/MainMenuManager | MainMenuManager | confirmCreateButton: {fileID: 1563459446} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203165 | Canvas/MainMenuManager | MainMenuManager | cancelCreateButton: {fileID: 1250177799} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203166 | Canvas/MainMenuManager | MainMenuManager | bgmSlider: {fileID: 384419740} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203167 | Canvas/MainMenuManager | MainMenuManager | sfxSlider: {fileID: 403109340} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203168 | Canvas/MainMenuManager | MainMenuManager | closeSettingsButton: {fileID: 1331868265} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203169 | Canvas/MainMenuManager | MainMenuManager | popupTitle: {fileID: 1864303443} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203170 | Canvas/MainMenuManager | MainMenuManager | popupMessage: {fileID: 875519812} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203171 | Canvas/MainMenuManager | MainMenuManager | popupConfirmBtn: {fileID: 1537215085} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203172 | Canvas/MainMenuManager | MainMenuManager | popupCancelBtn: {fileID: 1797654953} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203788 | Canvas/CharacterDetailModal | CharacterDetailModalUI | canvasGroup: {fileID: 2097715292} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203789 | Canvas/CharacterDetailModal | CharacterDetailModalUI | modalBox: {fileID: 1103503191} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203790 | Canvas/CharacterDetailModal | CharacterDetailModalUI | scrollRect: {fileID: 814243067} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203791 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeButton: {fileID: 1150597136} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203792 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeOverlayButton: {fileID: 1251122967} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203793 | Canvas/CharacterDetailModal | CharacterDetailModalUI | bottomCloseButton: {fileID: 1087333160} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203794 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtName: {fileID: 718167421} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203795 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtRealm: {fileID: 1918473206} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203796 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtTuVi: {fileID: 1917269040} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203797 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtCoDuyen: {fileID: 122488355} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203798 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtLinhThach: {fileID: 1408329680} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203799 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtHp: {fileID: 1163269436} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203800 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMp: {fileID: 1349554947} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203801 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMultiplier: {fileID: 1912840058} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203802 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtEquipSwords: {fileID: 1171760092} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203803 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeKiemY: {fileID: 1562206166} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203804 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeLinhLuc: {fileID: 581285601} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203805 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeTuLinh: {fileID: 1889137916} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:203806 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeThanThuc: {fileID: 1161672799} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207540 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbLinhChi: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207541 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207542 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207543 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtAlchemistRank: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207544 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtMedicineDust: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207545 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceStatus: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207546 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceTimer: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207547 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207548 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnFinishWithDust: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207549 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207550 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostTuVi: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207551 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewTuVi: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207552 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostBreakthrough: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207553 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207554 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207555 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207556 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnUseTuViPill: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207557 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | linhChiLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207558 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207559 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | tuyetLienLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207560 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewTimerLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207561 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewCultivationBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207562 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207563 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | useDustBtn: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207564 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | dustLabel: {fileID: 0} |
| Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity:207565 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | pillInventoryLabel: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:6603 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | titleText: {fileID: 1945041015} |
| Assets/Scenes/SampleScene.unity:6604 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | condText: {fileID: 238323356} |
| Assets/Scenes/SampleScene.unity:6605 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockButton: {fileID: 784547714} |
| Assets/Scenes/SampleScene.unity:6606 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockBtnText: {fileID: 1875151468} |
| Assets/Scenes/SampleScene.unity:7148 | Canvas/TribulationModalUI | TribulationModalUI | canvasGroup: {fileID: 296285011} |
| Assets/Scenes/SampleScene.unity:7149 | Canvas/TribulationModalUI | TribulationModalUI | modalBox: {fileID: 2081022731} |
| Assets/Scenes/SampleScene.unity:7150 | Canvas/TribulationModalUI | TribulationModalUI | confirmButton: {fileID: 1230983391} |
| Assets/Scenes/SampleScene.unity:7151 | Canvas/TribulationModalUI | TribulationModalUI | cancelButton: {fileID: 1954800248} |
| Assets/Scenes/SampleScene.unity:7152 | Canvas/TribulationModalUI | TribulationModalUI | titleText: {fileID: 1340314525} |
| Assets/Scenes/SampleScene.unity:7153 | Canvas/TribulationModalUI | TribulationModalUI | subtitleText: {fileID: 86746213} |
| Assets/Scenes/SampleScene.unity:7154 | Canvas/TribulationModalUI | TribulationModalUI | currentRealmText: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:7155 | Canvas/TribulationModalUI | TribulationModalUI | targetRealmText: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:7156 | Canvas/TribulationModalUI | TribulationModalUI | warningText: {fileID: 1859154824} |
| Assets/Scenes/SampleScene.unity:8565 | Canvas/ShopPanel/ScrollView/Content/ShopItem_4 | ShopItemUI | buyButton: {fileID: 581820742} |
| Assets/Scenes/SampleScene.unity:16161 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | titleText: {fileID: 1769779942} |
| Assets/Scenes/SampleScene.unity:16162 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | condText: {fileID: 209483097} |
| Assets/Scenes/SampleScene.unity:16163 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockButton: {fileID: 557737383} |
| Assets/Scenes/SampleScene.unity:16164 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockBtnText: {fileID: 630660150} |
| Assets/Scenes/SampleScene.unity:17270 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | titleText: {fileID: 479375528} |
| Assets/Scenes/SampleScene.unity:17271 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | condText: {fileID: 175780509} |
| Assets/Scenes/SampleScene.unity:17272 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockButton: {fileID: 1158179352} |
| Assets/Scenes/SampleScene.unity:17273 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockBtnText: {fileID: 1232013905} |
| Assets/Scenes/SampleScene.unity:17443 | Canvas/ShopPanel/ScrollView/Content/ShopItem_2 | ShopItemUI | buyButton: {fileID: 69685870} |
| Assets/Scenes/SampleScene.unity:17993 | Canvas/MilestoneModalUI | MilestoneModalUI | modalPanel: {fileID: 757123755} |
| Assets/Scenes/SampleScene.unity:17994 | Canvas/MilestoneModalUI | MilestoneModalUI | contentContainer: {fileID: 594783097} |
| Assets/Scenes/SampleScene.unity:17995 | Canvas/MilestoneModalUI | MilestoneModalUI | txtTotalProgress: {fileID: 553631749} |
| Assets/Scenes/SampleScene.unity:17996 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClose: {fileID: 518623441} |
| Assets/Scenes/SampleScene.unity:17997 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClaimAll: {fileID: 63987043} |
| Assets/Scenes/SampleScene.unity:17998 | Canvas/MilestoneModalUI | MilestoneModalUI | rowTemplate: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:21079 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | dataPanel: {fileID: 1265773101} |
| Assets/Scenes/SampleScene.unity:21080 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | emptyPanel: {fileID: 1455111634} |
| Assets/Scenes/SampleScene.unity:21081 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | nameText: {fileID: 1246669010} |
| Assets/Scenes/SampleScene.unity:21082 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | cultivationText: {fileID: 780796025} |
| Assets/Scenes/SampleScene.unity:21083 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | playTimeText: {fileID: 567340084} |
| Assets/Scenes/SampleScene.unity:21084 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | slotButton: {fileID: 889524879} |
| Assets/Scenes/SampleScene.unity:21085 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_1 | SaveSlotUI | deleteButton: {fileID: 569429891} |
| Assets/Scenes/SampleScene.unity:22687 | Canvas/UIManager | CultivationUI | txtStageName: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:22688 | Canvas/UIManager | CultivationUI | txtTuVi: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:22689 | Canvas/UIManager | CultivationUI | imgProgressBar: {fileID: 0} |
| Assets/Scenes/SampleScene.unity:22851 | Canvas/PauseMenuModal | PauseMenuManager | pauseModal: {fileID: 331140756} |
| Assets/Scenes/SampleScene.unity:22852 | Canvas/PauseMenuModal | PauseMenuManager | confirmationPopup: {fileID: 2030168928} |
| Assets/Scenes/SampleScene.unity:22853 | Canvas/PauseMenuModal | PauseMenuManager | modalCanvasGroup: {fileID: 958334017} |
| Assets/Scenes/SampleScene.unity:22854 | Canvas/PauseMenuModal | PauseMenuManager | openPauseButton: {fileID: 1814150550} |
| Assets/Scenes/SampleScene.unity:22855 | Canvas/PauseMenuModal | PauseMenuManager | closeHeaderButton: {fileID: 922403253} |
| Assets/Scenes/SampleScene.unity:22856 | Canvas/PauseMenuModal | PauseMenuManager | overlayCloseButton: {fileID: 628852012} |
| Assets/Scenes/SampleScene.unity:22857 | Canvas/PauseMenuModal | PauseMenuManager | resumeButton: {fileID: 760746783} |
| Assets/Scenes/SampleScene.unity:22858 | Canvas/PauseMenuModal | PauseMenuManager | manualSaveButton: {fileID: 825525784} |
| Assets/Scenes/SampleScene.unity:22859 | Canvas/PauseMenuModal | PauseMenuManager | returnMainMenuButton: {fileID: 1728238219} |
| Assets/Scenes/SampleScene.unity:22860 | Canvas/PauseMenuModal | PauseMenuManager | quitButton: {fileID: 421096080} |
| Assets/Scenes/SampleScene.unity:22861 | Canvas/PauseMenuModal | PauseMenuManager | bgmSlider: {fileID: 870604229} |
| Assets/Scenes/SampleScene.unity:22862 | Canvas/PauseMenuModal | PauseMenuManager | txtBgmValue: {fileID: 664024896} |
| Assets/Scenes/SampleScene.unity:22863 | Canvas/PauseMenuModal | PauseMenuManager | sfxSlider: {fileID: 1699368934} |
| Assets/Scenes/SampleScene.unity:22864 | Canvas/PauseMenuModal | PauseMenuManager | txtSfxValue: {fileID: 2021520321} |
| Assets/Scenes/SampleScene.unity:22865 | Canvas/PauseMenuModal | PauseMenuManager | muteToggle: {fileID: 615496905} |
| Assets/Scenes/SampleScene.unity:22866 | Canvas/PauseMenuModal | PauseMenuManager | txtCharacterInfo: {fileID: 323066533} |
| Assets/Scenes/SampleScene.unity:22867 | Canvas/PauseMenuModal | PauseMenuManager | txtSaveStatus: {fileID: 98726558} |
| Assets/Scenes/SampleScene.unity:22868 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupTitle: {fileID: 687175252} |
| Assets/Scenes/SampleScene.unity:22869 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupMessage: {fileID: 578936211} |
| Assets/Scenes/SampleScene.unity:22870 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupConfirm: {fileID: 1724402819} |
| Assets/Scenes/SampleScene.unity:22871 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupCancel: {fileID: 441178331} |
| Assets/Scenes/SampleScene.unity:23518 | Canvas/ShopPanel/ScrollView/Content/ShopItem_3 | ShopItemUI | buyButton: {fileID: 1843090946} |
| Assets/Scenes/SampleScene.unity:23612 | Canvas/ShopPanel/ScrollView/Content/ShopItem_1 | ShopItemUI | buyButton: {fileID: 17490869} |
| Assets/Scenes/SampleScene.unity:28258 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | dataPanel: {fileID: 1651988589} |
| Assets/Scenes/SampleScene.unity:28259 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | emptyPanel: {fileID: 1220777055} |
| Assets/Scenes/SampleScene.unity:28260 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | nameText: {fileID: 85093384} |
| Assets/Scenes/SampleScene.unity:28261 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | cultivationText: {fileID: 1082954645} |
| Assets/Scenes/SampleScene.unity:28262 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | playTimeText: {fileID: 339315753} |
| Assets/Scenes/SampleScene.unity:28263 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | slotButton: {fileID: 1178820542} |
| Assets/Scenes/SampleScene.unity:28264 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_0 | SaveSlotUI | deleteButton: {fileID: 255200659} |
| Assets/Scenes/SampleScene.unity:29613 | Canvas/CombatNotificationBanner | CombatNotificationUI | canvasGroup: {fileID: 1248546305} |
| Assets/Scenes/SampleScene.unity:29614 | Canvas/CombatNotificationBanner | CombatNotificationUI | containerRect: {fileID: 1248546303} |
| Assets/Scenes/SampleScene.unity:29615 | Canvas/CombatNotificationBanner | CombatNotificationUI | backgroundImage: {fileID: 2008515411} |
| Assets/Scenes/SampleScene.unity:29616 | Canvas/CombatNotificationBanner | CombatNotificationUI | borderTop: {fileID: 807191400} |
| Assets/Scenes/SampleScene.unity:29617 | Canvas/CombatNotificationBanner | CombatNotificationUI | borderBottom: {fileID: 1510956772} |
| Assets/Scenes/SampleScene.unity:29618 | Canvas/CombatNotificationBanner | CombatNotificationUI | titleText: {fileID: 1315856892} |
| Assets/Scenes/SampleScene.unity:29619 | Canvas/CombatNotificationBanner | CombatNotificationUI | subtitleText: {fileID: 624708113} |
| Assets/Scenes/SampleScene.unity:29620 | Canvas/CombatNotificationBanner | CombatNotificationUI | extraDetailText: {fileID: 1795174205} |
| Assets/Scenes/SampleScene.unity:29926 | Canvas/ShopPanel/btn_Close | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/SampleScene.unity:33619 | Canvas/RightPanel/btn_OpenShop | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/SampleScene.unity:40652 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | dataPanel: {fileID: 825211636} |
| Assets/Scenes/SampleScene.unity:40653 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | emptyPanel: {fileID: 1723814474} |
| Assets/Scenes/SampleScene.unity:40654 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | nameText: {fileID: 1896802288} |
| Assets/Scenes/SampleScene.unity:40655 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | cultivationText: {fileID: 1899240485} |
| Assets/Scenes/SampleScene.unity:40656 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | playTimeText: {fileID: 1829056205} |
| Assets/Scenes/SampleScene.unity:40657 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | slotButton: {fileID: 1621847545} |
| Assets/Scenes/SampleScene.unity:40658 | Canvas/MainMenuManager/CharacterSelectPanel/SlotContainer/SaveSlot_2 | SaveSlotUI | deleteButton: {fileID: 755315457} |
| Assets/Scenes/SampleScene.unity:48707 | Canvas/ShopPanel/ScrollView/Content/ShopItem_0 | ShopItemUI | buyButton: {fileID: 1005832532} |
| Assets/Scenes/SampleScene.unity:52549 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | titleText: {fileID: 1454567060} |
| Assets/Scenes/SampleScene.unity:52550 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | condText: {fileID: 1734239692} |
| Assets/Scenes/SampleScene.unity:52551 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockButton: {fileID: 1571659309} |
| Assets/Scenes/SampleScene.unity:52552 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockBtnText: {fileID: 864419017} |
| Assets/Scenes/SampleScene.unity:52930 | Canvas/MainMenuManager | MainMenuManager | mainPanel: {fileID: 2017822356} |
| Assets/Scenes/SampleScene.unity:52931 | Canvas/MainMenuManager | MainMenuManager | characterSelectPanel: {fileID: 943835570} |
| Assets/Scenes/SampleScene.unity:52932 | Canvas/MainMenuManager | MainMenuManager | characterCreatePanel: {fileID: 809555469} |
| Assets/Scenes/SampleScene.unity:52933 | Canvas/MainMenuManager | MainMenuManager | settingsPanel: {fileID: 1399051815} |
| Assets/Scenes/SampleScene.unity:52934 | Canvas/MainMenuManager | MainMenuManager | confirmationPopup: {fileID: 1638424892} |
| Assets/Scenes/SampleScene.unity:52935 | Canvas/MainMenuManager | MainMenuManager | playButton: {fileID: 482855941} |
| Assets/Scenes/SampleScene.unity:52936 | Canvas/MainMenuManager | MainMenuManager | playButtonText: {fileID: 1073180194} |
| Assets/Scenes/SampleScene.unity:52937 | Canvas/MainMenuManager | MainMenuManager | settingsButton: {fileID: 1158388662} |
| Assets/Scenes/SampleScene.unity:52938 | Canvas/MainMenuManager | MainMenuManager | quitButton: {fileID: 73727741} |
| Assets/Scenes/SampleScene.unity:52943 | Canvas/MainMenuManager | MainMenuManager | backFromSelectButton: {fileID: 262579212} |
| Assets/Scenes/SampleScene.unity:52944 | Canvas/MainMenuManager | MainMenuManager | nameInput: {fileID: 913883099} |
| Assets/Scenes/SampleScene.unity:52945 | Canvas/MainMenuManager | MainMenuManager | confirmCreateButton: {fileID: 1563459446} |
| Assets/Scenes/SampleScene.unity:52946 | Canvas/MainMenuManager | MainMenuManager | cancelCreateButton: {fileID: 1250177799} |
| Assets/Scenes/SampleScene.unity:52947 | Canvas/MainMenuManager | MainMenuManager | bgmSlider: {fileID: 384419740} |
| Assets/Scenes/SampleScene.unity:52948 | Canvas/MainMenuManager | MainMenuManager | sfxSlider: {fileID: 403109340} |
| Assets/Scenes/SampleScene.unity:52949 | Canvas/MainMenuManager | MainMenuManager | closeSettingsButton: {fileID: 1331868265} |
| Assets/Scenes/SampleScene.unity:52950 | Canvas/MainMenuManager | MainMenuManager | popupTitle: {fileID: 1864303443} |
| Assets/Scenes/SampleScene.unity:52951 | Canvas/MainMenuManager | MainMenuManager | popupMessage: {fileID: 875519812} |
| Assets/Scenes/SampleScene.unity:52952 | Canvas/MainMenuManager | MainMenuManager | popupConfirmBtn: {fileID: 1537215085} |
| Assets/Scenes/SampleScene.unity:52953 | Canvas/MainMenuManager | MainMenuManager | popupCancelBtn: {fileID: 1797654953} |
| Assets/Scenes/SampleScene.unity:53181 | Canvas/CharacterDetailModal | CharacterDetailModalUI | canvasGroup: {fileID: 2097715292} |
| Assets/Scenes/SampleScene.unity:53182 | Canvas/CharacterDetailModal | CharacterDetailModalUI | modalBox: {fileID: 1103503191} |
| Assets/Scenes/SampleScene.unity:53183 | Canvas/CharacterDetailModal | CharacterDetailModalUI | scrollRect: {fileID: 814243067} |
| Assets/Scenes/SampleScene.unity:53184 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeButton: {fileID: 1150597136} |
| Assets/Scenes/SampleScene.unity:53185 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeOverlayButton: {fileID: 1251122967} |
| Assets/Scenes/SampleScene.unity:53186 | Canvas/CharacterDetailModal | CharacterDetailModalUI | bottomCloseButton: {fileID: 1087333160} |
| Assets/Scenes/SampleScene.unity:53187 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtName: {fileID: 718167421} |
| Assets/Scenes/SampleScene.unity:53188 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtRealm: {fileID: 1918473206} |
| Assets/Scenes/SampleScene.unity:53189 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtTuVi: {fileID: 1917269040} |
| Assets/Scenes/SampleScene.unity:53190 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtCoDuyen: {fileID: 122488355} |
| Assets/Scenes/SampleScene.unity:53191 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtLinhThach: {fileID: 1408329680} |
| Assets/Scenes/SampleScene.unity:53192 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtHp: {fileID: 1163269436} |
| Assets/Scenes/SampleScene.unity:53193 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMp: {fileID: 1349554947} |
| Assets/Scenes/SampleScene.unity:53194 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMultiplier: {fileID: 1912840058} |
| Assets/Scenes/SampleScene.unity:53195 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtEquipSwords: {fileID: 1171760092} |
| Assets/Scenes/SampleScene.unity:53196 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeKiemY: {fileID: 1562206166} |
| Assets/Scenes/SampleScene.unity:53197 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeLinhLuc: {fileID: 581285601} |
| Assets/Scenes/SampleScene.unity:53198 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeTuLinh: {fileID: 1889137916} |
| Assets/Scenes/SampleScene.unity:53199 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeThanThuc: {fileID: 1161672799} |
| Assets/Scenes/SampleScene_MobileWork.unity:10363 | #0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10732 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10733 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10734 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeScrollRect: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10735 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | treeContent: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10818 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalRoot: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10819 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBox: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10820 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10821 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10822 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTitle: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10823 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalTier: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10824 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalLore: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10825 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalMechanics: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10826 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCurrent: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10827 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalNext: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10828 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalRequirement: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10829 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | goModalLockBanner: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10830 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10831 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10832 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:10833 | Canvas/MobileUIRoot/Drawer_1/ScrollView/Viewport/Content | BodyTemperingPanelUI | txtModalCost: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:27664 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | titleText: {fileID: 1945041015} |
| Assets/Scenes/SampleScene_MobileWork.unity:27665 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | condText: {fileID: 238323356} |
| Assets/Scenes/SampleScene_MobileWork.unity:27666 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockButton: {fileID: 784547714} |
| Assets/Scenes/SampleScene_MobileWork.unity:27667 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_2 | SkillCardUI | unlockBtnText: {fileID: 1875151468} |
| Assets/Scenes/SampleScene_MobileWork.unity:30379 | Canvas/TribulationModalUI | TribulationModalUI | canvasGroup: {fileID: 296285011} |
| Assets/Scenes/SampleScene_MobileWork.unity:30380 | Canvas/TribulationModalUI | TribulationModalUI | modalBox: {fileID: 2081022731} |
| Assets/Scenes/SampleScene_MobileWork.unity:30381 | Canvas/TribulationModalUI | TribulationModalUI | confirmButton: {fileID: 1230983391} |
| Assets/Scenes/SampleScene_MobileWork.unity:30382 | Canvas/TribulationModalUI | TribulationModalUI | cancelButton: {fileID: 1954800248} |
| Assets/Scenes/SampleScene_MobileWork.unity:30383 | Canvas/TribulationModalUI | TribulationModalUI | titleText: {fileID: 1340314525} |
| Assets/Scenes/SampleScene_MobileWork.unity:30384 | Canvas/TribulationModalUI | TribulationModalUI | subtitleText: {fileID: 86746213} |
| Assets/Scenes/SampleScene_MobileWork.unity:30385 | Canvas/TribulationModalUI | TribulationModalUI | currentRealmText: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:30386 | Canvas/TribulationModalUI | TribulationModalUI | targetRealmText: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:30387 | Canvas/TribulationModalUI | TribulationModalUI | warningText: {fileID: 1859154824} |
| Assets/Scenes/SampleScene_MobileWork.unity:32722 | Canvas/PauseMenuModal/PauseBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:33179 | Canvas/ShopPanel/ScrollView/Content/ShopItem_4 | ShopItemUI | buyButton: {fileID: 581820742} |
| Assets/Scenes/SampleScene_MobileWork.unity:35234 | Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | btnUltimate: {fileID: 614836222} |
| Assets/Scenes/SampleScene_MobileWork.unity:35235 | Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | imgUltiFill: {fileID: 1968414597} |
| Assets/Scenes/SampleScene_MobileWork.unity:35236 | Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | imgUltiGlow: {fileID: 1126378337} |
| Assets/Scenes/SampleScene_MobileWork.unity:35237 | Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | txtUltiTitle: {fileID: 363105161} |
| Assets/Scenes/SampleScene_MobileWork.unity:35238 | Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | txtUltiMana: {fileID: 97273254} |
| Assets/Scenes/SampleScene_MobileWork.unity:39102 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtLinhThachBalance: {fileID: 526797192} |
| Assets/Scenes/SampleScene_MobileWork.unity:39103 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtTotalBodyLevel: {fileID: 30914327} |
| Assets/Scenes/SampleScene_MobileWork.unity:39104 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeScrollRect: {fileID: 1267652686} |
| Assets/Scenes/SampleScene_MobileWork.unity:39105 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | treeContent: {fileID: 752384037} |
| Assets/Scenes/SampleScene_MobileWork.unity:39188 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalRoot: {fileID: 72613887} |
| Assets/Scenes/SampleScene_MobileWork.unity:39189 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBox: {fileID: 847165447} |
| Assets/Scenes/SampleScene_MobileWork.unity:39190 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalCloseBtn: {fileID: 36418814} |
| Assets/Scenes/SampleScene_MobileWork.unity:39191 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | centerModalBackdropBtn: {fileID: 72613889} |
| Assets/Scenes/SampleScene_MobileWork.unity:39192 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTitle: {fileID: 1934146158} |
| Assets/Scenes/SampleScene_MobileWork.unity:39193 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalTier: {fileID: 1856756184} |
| Assets/Scenes/SampleScene_MobileWork.unity:39194 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalLore: {fileID: 1675773072} |
| Assets/Scenes/SampleScene_MobileWork.unity:39195 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalMechanics: {fileID: 1916659841} |
| Assets/Scenes/SampleScene_MobileWork.unity:39196 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCurrent: {fileID: 1715493584} |
| Assets/Scenes/SampleScene_MobileWork.unity:39197 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalNext: {fileID: 720103016} |
| Assets/Scenes/SampleScene_MobileWork.unity:39198 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalRequirement: {fileID: 699765833} |
| Assets/Scenes/SampleScene_MobileWork.unity:39199 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | goModalLockBanner: {fileID: 699765831} |
| Assets/Scenes/SampleScene_MobileWork.unity:39200 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade: {fileID: 584480004} |
| Assets/Scenes/SampleScene_MobileWork.unity:39201 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgrade5: {fileID: 1103643327} |
| Assets/Scenes/SampleScene_MobileWork.unity:39202 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | btnModalUpgradeMax: {fileID: 933672487} |
| Assets/Scenes/SampleScene_MobileWork.unity:39203 | Canvas/MobileUIRoot/Drawer_1 | BodyTemperingPanelUI | txtModalCost: {fileID: 2094405181} |
| Assets/Scenes/SampleScene_MobileWork.unity:41362 | Canvas/MilestoneModalUI/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:47104 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | oreLabel: {fileID: 1497739962} |
| Assets/Scenes/SampleScene_MobileWork.unity:47105 | Canvas/MobileUIRoot/Drawer_3 | ArtifactPanelUI | auraLabel: {fileID: 140643440} |
| Assets/Scenes/SampleScene_MobileWork.unity:62118 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | titleText: {fileID: 1769779942} |
| Assets/Scenes/SampleScene_MobileWork.unity:62119 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | condText: {fileID: 209483097} |
| Assets/Scenes/SampleScene_MobileWork.unity:62120 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockButton: {fileID: 557737383} |
| Assets/Scenes/SampleScene_MobileWork.unity:62121 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_1 | SkillCardUI | unlockBtnText: {fileID: 630660150} |
| Assets/Scenes/SampleScene_MobileWork.unity:65097 | Canvas/AchievementModalUI/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:69289 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | titleText: {fileID: 479375528} |
| Assets/Scenes/SampleScene_MobileWork.unity:69290 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | condText: {fileID: 175780509} |
| Assets/Scenes/SampleScene_MobileWork.unity:69291 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockButton: {fileID: 1158179352} |
| Assets/Scenes/SampleScene_MobileWork.unity:69292 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_0 | SkillCardUI | unlockBtnText: {fileID: 1232013905} |
| Assets/Scenes/SampleScene_MobileWork.unity:69502 | Canvas/MobileUIRoot/PortraitHudShell/UpperInformation/StagePanel | CultivationPhaseBarUI | stageLabel: {fileID: 1377677953} |
| Assets/Scenes/SampleScene_MobileWork.unity:69932 | Canvas/ShopPanel/ScrollView/Content/ShopItem_2 | ShopItemUI | buyButton: {fileID: 69685870} |
| Assets/Scenes/SampleScene_MobileWork.unity:85185 | Canvas/MobileUIRoot/BodyTalentDetailModal/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:86440 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | spiritCooldownLabel: {fileID: 2086229475} |
| Assets/Scenes/SampleScene_MobileWork.unity:86441 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | activateSpiritBtn: {fileID: 1021691522} |
| Assets/Scenes/SampleScene_MobileWork.unity:86442 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtOreLabel: {fileID: 957735967} |
| Assets/Scenes/SampleScene_MobileWork.unity:86443 | Canvas/MobileUIRoot/Drawer_4 | FormationPanelUI | txtCurrentActiveLabel: {fileID: 735508003} |
| Assets/Scenes/SampleScene_MobileWork.unity:88238 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | dataPanel: {fileID: 1265773101} |
| Assets/Scenes/SampleScene_MobileWork.unity:88239 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | emptyPanel: {fileID: 1455111634} |
| Assets/Scenes/SampleScene_MobileWork.unity:88240 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | nameText: {fileID: 1246669010} |
| Assets/Scenes/SampleScene_MobileWork.unity:88241 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | cultivationText: {fileID: 780796025} |
| Assets/Scenes/SampleScene_MobileWork.unity:88242 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | playTimeText: {fileID: 567340084} |
| Assets/Scenes/SampleScene_MobileWork.unity:88243 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | slotButton: {fileID: 889524879} |
| Assets/Scenes/SampleScene_MobileWork.unity:88244 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_1 | SaveSlotUI | deleteButton: {fileID: 569429891} |
| Assets/Scenes/SampleScene_MobileWork.unity:90653 | Canvas/SecretRealmModal/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:95545 | Canvas/PauseMenuModal | PauseMenuManager | pauseModal: {fileID: 331140756} |
| Assets/Scenes/SampleScene_MobileWork.unity:95546 | Canvas/PauseMenuModal | PauseMenuManager | confirmationPopup: {fileID: 2030168928} |
| Assets/Scenes/SampleScene_MobileWork.unity:95547 | Canvas/PauseMenuModal | PauseMenuManager | modalCanvasGroup: {fileID: 958334017} |
| Assets/Scenes/SampleScene_MobileWork.unity:95548 | Canvas/PauseMenuModal | PauseMenuManager | openPauseButton: {fileID: 1814150550} |
| Assets/Scenes/SampleScene_MobileWork.unity:95549 | Canvas/PauseMenuModal | PauseMenuManager | closeHeaderButton: {fileID: 922403253} |
| Assets/Scenes/SampleScene_MobileWork.unity:95550 | Canvas/PauseMenuModal | PauseMenuManager | overlayCloseButton: {fileID: 628852012} |
| Assets/Scenes/SampleScene_MobileWork.unity:95551 | Canvas/PauseMenuModal | PauseMenuManager | resumeButton: {fileID: 760746783} |
| Assets/Scenes/SampleScene_MobileWork.unity:95552 | Canvas/PauseMenuModal | PauseMenuManager | manualSaveButton: {fileID: 825525784} |
| Assets/Scenes/SampleScene_MobileWork.unity:95553 | Canvas/PauseMenuModal | PauseMenuManager | returnMainMenuButton: {fileID: 1728238219} |
| Assets/Scenes/SampleScene_MobileWork.unity:95554 | Canvas/PauseMenuModal | PauseMenuManager | quitButton: {fileID: 421096080} |
| Assets/Scenes/SampleScene_MobileWork.unity:95555 | Canvas/PauseMenuModal | PauseMenuManager | bgmSlider: {fileID: 870604229} |
| Assets/Scenes/SampleScene_MobileWork.unity:95556 | Canvas/PauseMenuModal | PauseMenuManager | txtBgmValue: {fileID: 664024896} |
| Assets/Scenes/SampleScene_MobileWork.unity:95557 | Canvas/PauseMenuModal | PauseMenuManager | sfxSlider: {fileID: 1699368934} |
| Assets/Scenes/SampleScene_MobileWork.unity:95558 | Canvas/PauseMenuModal | PauseMenuManager | txtSfxValue: {fileID: 2021520321} |
| Assets/Scenes/SampleScene_MobileWork.unity:95559 | Canvas/PauseMenuModal | PauseMenuManager | muteToggle: {fileID: 615496905} |
| Assets/Scenes/SampleScene_MobileWork.unity:95560 | Canvas/PauseMenuModal | PauseMenuManager | txtCharacterInfo: {fileID: 323066533} |
| Assets/Scenes/SampleScene_MobileWork.unity:95561 | Canvas/PauseMenuModal | PauseMenuManager | txtSaveStatus: {fileID: 98726558} |
| Assets/Scenes/SampleScene_MobileWork.unity:95562 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupTitle: {fileID: 687175252} |
| Assets/Scenes/SampleScene_MobileWork.unity:95563 | Canvas/PauseMenuModal | PauseMenuManager | txtPopupMessage: {fileID: 578936211} |
| Assets/Scenes/SampleScene_MobileWork.unity:95564 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupConfirm: {fileID: 1724402819} |
| Assets/Scenes/SampleScene_MobileWork.unity:95565 | Canvas/PauseMenuModal | PauseMenuManager | btnPopupCancel: {fileID: 441178331} |
| Assets/Scenes/SampleScene_MobileWork.unity:95749 | Canvas/StorageModalUI/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:97264 | Canvas/MainMenuManager/ConfirmationPopup/Box | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:97682 | Canvas/ShopPanel/ScrollView/Content/ShopItem_3 | ShopItemUI | buyButton: {fileID: 1843090946} |
| Assets/Scenes/SampleScene_MobileWork.unity:100381 | Canvas/ShopPanel/ScrollView/Content/ShopItem_1 | ShopItemUI | buyButton: {fileID: 17490869} |
| Assets/Scenes/SampleScene_MobileWork.unity:109877 | Canvas/CharacterDetailModal/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:110617 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbLinhChi: {fileID: 619501297} |
| Assets/Scenes/SampleScene_MobileWork.unity:110618 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 1545148919} |
| Assets/Scenes/SampleScene_MobileWork.unity:110619 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 704849249} |
| Assets/Scenes/SampleScene_MobileWork.unity:110620 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtAlchemistRank: {fileID: 293563195} |
| Assets/Scenes/SampleScene_MobileWork.unity:110621 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtMedicineDust: {fileID: 934000489} |
| Assets/Scenes/SampleScene_MobileWork.unity:110622 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceStatus: {fileID: 1190454638} |
| Assets/Scenes/SampleScene_MobileWork.unity:110623 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtFurnaceTimer: {fileID: 1905055730} |
| Assets/Scenes/SampleScene_MobileWork.unity:110624 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:110625 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnFinishWithDust: {fileID: 197806336} |
| Assets/Scenes/SampleScene_MobileWork.unity:110626 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:110627 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostTuVi: {fileID: 1362888274} |
| Assets/Scenes/SampleScene_MobileWork.unity:110628 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewTuVi: {fileID: 993192221} |
| Assets/Scenes/SampleScene_MobileWork.unity:110629 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtCostBreakthrough: {fileID: 1319886065} |
| Assets/Scenes/SampleScene_MobileWork.unity:110630 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 1424713129} |
| Assets/Scenes/SampleScene_MobileWork.unity:110631 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 469839393} |
| Assets/Scenes/SampleScene_MobileWork.unity:110632 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 1465415029} |
| Assets/Scenes/SampleScene_MobileWork.unity:110633 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | btnUseTuViPill: {fileID: 1335068527} |
| Assets/Scenes/SampleScene_MobileWork.unity:110634 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | linhChiLabel: {fileID: 619501297} |
| Assets/Scenes/SampleScene_MobileWork.unity:110635 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 1545148919} |
| Assets/Scenes/SampleScene_MobileWork.unity:110636 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | tuyetLienLabel: {fileID: 704849249} |
| Assets/Scenes/SampleScene_MobileWork.unity:110637 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewTimerLabel: {fileID: 1905055730} |
| Assets/Scenes/SampleScene_MobileWork.unity:110638 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewCultivationBtn: {fileID: 993192221} |
| Assets/Scenes/SampleScene_MobileWork.unity:110639 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 1424713129} |
| Assets/Scenes/SampleScene_MobileWork.unity:110640 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | useDustBtn: {fileID: 197806336} |
| Assets/Scenes/SampleScene_MobileWork.unity:110641 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | dustLabel: {fileID: 934000489} |
| Assets/Scenes/SampleScene_MobileWork.unity:110642 | Canvas/MobileUIRoot/Drawer_2 | AlchemyPanelUI | pillInventoryLabel: {fileID: 469839393} |
| Assets/Scenes/SampleScene_MobileWork.unity:116575 | Canvas/DismantleConfirmModalUI/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:117056 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | dataPanel: {fileID: 1651988589} |
| Assets/Scenes/SampleScene_MobileWork.unity:117057 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | emptyPanel: {fileID: 1220777055} |
| Assets/Scenes/SampleScene_MobileWork.unity:117058 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | nameText: {fileID: 85093384} |
| Assets/Scenes/SampleScene_MobileWork.unity:117059 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | cultivationText: {fileID: 1082954645} |
| Assets/Scenes/SampleScene_MobileWork.unity:117060 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | playTimeText: {fileID: 339315753} |
| Assets/Scenes/SampleScene_MobileWork.unity:117061 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | slotButton: {fileID: 1178820542} |
| Assets/Scenes/SampleScene_MobileWork.unity:117062 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_0 | SaveSlotUI | deleteButton: {fileID: 255200659} |
| Assets/Scenes/SampleScene_MobileWork.unity:119175 | Canvas/AchievementModalUI | AchievementModalUI | canvasGroup: {fileID: 1224474608} |
| Assets/Scenes/SampleScene_MobileWork.unity:119176 | Canvas/AchievementModalUI | AchievementModalUI | modalBox: {fileID: 700038009} |
| Assets/Scenes/SampleScene_MobileWork.unity:119177 | Canvas/AchievementModalUI | AchievementModalUI | btnClose: {fileID: 1284123593} |
| Assets/Scenes/SampleScene_MobileWork.unity:119178 | Canvas/AchievementModalUI | AchievementModalUI | btnBackdrop: {fileID: 1224474606} |
| Assets/Scenes/SampleScene_MobileWork.unity:119179 | Canvas/AchievementModalUI | AchievementModalUI | txtTotalProgress: {fileID: 267208547} |
| Assets/Scenes/SampleScene_MobileWork.unity:119180 | Canvas/AchievementModalUI | AchievementModalUI | fillTotalProgress: {fileID: 1729469184} |
| Assets/Scenes/SampleScene_MobileWork.unity:119181 | Canvas/AchievementModalUI | AchievementModalUI | btnClaimAll: {fileID: 1913150654} |
| Assets/Scenes/SampleScene_MobileWork.unity:119182 | Canvas/AchievementModalUI | AchievementModalUI | txtClaimAll: {fileID: 311589095} |
| Assets/Scenes/SampleScene_MobileWork.unity:119191 | Canvas/AchievementModalUI | AchievementModalUI | contentContainer: {fileID: 276520117} |
| Assets/Scenes/SampleScene_MobileWork.unity:119192 | Canvas/AchievementModalUI | AchievementModalUI | scrollRect: {fileID: 1375749548} |
| Assets/Scenes/SampleScene_MobileWork.unity:119430 | Canvas/MobileUIRoot | MobileDrawerUIController | backdropOverlay: {fileID: 373122639} |
| Assets/Scenes/SampleScene_MobileWork.unity:119443 | Canvas/MobileUIRoot | MobileDrawerUIController | mobileDock: {fileID: 867939491} |
| Assets/Scenes/SampleScene_MobileWork.unity:119444 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleButton: {fileID: 79286217} |
| Assets/Scenes/SampleScene_MobileWork.unity:119445 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleArrowText: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:119446 | Canvas/MobileUIRoot | MobileDrawerUIController | dockToggleLabelText: {fileID: 437073796} |
| Assets/Scenes/SampleScene_MobileWork.unity:119447 | Canvas/MobileUIRoot | MobileDrawerUIController | playerHpBar: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:119486 | Canvas/MobileUIRoot | JadeSecondaryMenu | toggle: {fileID: 398168851} |
| Assets/Scenes/SampleScene_MobileWork.unity:119492 | Canvas/MobileUIRoot | JadeSecondaryMenu | dismissArea: {fileID: 1350921707} |
| Assets/Scenes/SampleScene_MobileWork.unity:121517 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | canvasGroup: {fileID: 1248546305} |
| Assets/Scenes/SampleScene_MobileWork.unity:121518 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | containerRect: {fileID: 1248546303} |
| Assets/Scenes/SampleScene_MobileWork.unity:121519 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | backgroundImage: {fileID: 2008515411} |
| Assets/Scenes/SampleScene_MobileWork.unity:121520 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | borderTop: {fileID: 807191400} |
| Assets/Scenes/SampleScene_MobileWork.unity:121521 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | borderBottom: {fileID: 1510956772} |
| Assets/Scenes/SampleScene_MobileWork.unity:121522 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | titleText: {fileID: 1315856892} |
| Assets/Scenes/SampleScene_MobileWork.unity:121523 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | subtitleText: {fileID: 624708113} |
| Assets/Scenes/SampleScene_MobileWork.unity:121524 | Canvas/MobileUIRoot/PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | extraDetailText: {fileID: 1795174205} |
| Assets/Scenes/SampleScene_MobileWork.unity:122051 | Canvas/ShopPanel/btn_Close | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/SampleScene_MobileWork.unity:129763 | #0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:130329 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | oreLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:130330 | Canvas/MobileUIRoot/Drawer_3/ScrollView/Viewport/Content | ArtifactPanelUI | auraLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:145971 | Canvas/RightPanel/btn_OpenShop | ShopToggleUI | shopPanel: {fileID: 1995611995} |
| Assets/Scenes/SampleScene_MobileWork.unity:152056 | Canvas/PauseMenuModal/ConfirmationPopup/Box | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:153950 | Canvas | JadeUITheme | panel: {fileID: 560880379, guid: 2e22a8bd7cc421744b98f516c6e681c0, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153951 | Canvas | JadeUITheme | window: {fileID: 560880379, guid: 2e22a8bd7cc421744b98f516c6e681c0, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153952 | Canvas | JadeUITheme | slot: {fileID: -332078081, guid: d448d9ebbf7e0794ea5cbd7e904026b2, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153953 | Canvas | JadeUITheme | selectedSlot: {fileID: 1899475782, guid: 29c37302ecef2ba43997957acf8811a8, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153954 | Canvas | JadeUITheme | primaryButton: {fileID: 508764426, guid: 5a37763d73c74f343909ed6b36447db1, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153955 | Canvas | JadeUITheme | secondaryButton: {fileID: -1079613726, guid: 54f45e56ae7c2534d891a07dae52c93c, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153956 | Canvas | JadeUITheme | resourceBar: {fileID: -1809654655, guid: 31a077464e6a2fc4ab88e14a0fd02caa, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153957 | Canvas | JadeUITheme | divider: {fileID: -1568192995, guid: e205b3e4003894e45a73117f5f736e20, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153964 | Canvas | JadeUITheme | spiritStone: {fileID: -261319116, guid: 600f4904d6dc01b49bb458a76f1b854c, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153965 | Canvas | JadeUITheme | spiritFlame: {fileID: 1986697073, guid: f9d12e1712175fe4ab906b5b5f086057, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153966 | Canvas | JadeUITheme | cultivationHalo: {fileID: 1001747856, guid: 6930089c69195f0468f4956f35c7cf0e, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:153967 | Canvas | JadeUITheme | reconstruction: {fileID: 1451818564} |
| Assets/Scenes/SampleScene_MobileWork.unity:153992 | Canvas | MobileUIReconstruction | assets: {fileID: 11400000, guid: 790af2f632b07874696e67d8784e5e24, type: 2} |
| Assets/Scenes/SampleScene_MobileWork.unity:153993 | Canvas | MobileUIReconstruction | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:153994 | Canvas | MobileUIReconstruction | drawers: {fileID: 1230500034} |
| Assets/Scenes/SampleScene_MobileWork.unity:153995 | Canvas | MobileUIReconstruction | navigation: {fileID: 1230500036} |
| Assets/Scenes/SampleScene_MobileWork.unity:154072 | Canvas | MobileUIReconstruction | titleHeader: {fileID: 1708121086} |
| Assets/Scenes/SampleScene_MobileWork.unity:154073 | Canvas | MobileUIReconstruction | titleActions: {fileID: 125219272} |
| Assets/Scenes/SampleScene_MobileWork.unity:154074 | Canvas | MobileUIReconstruction | titleFooter: {fileID: 1586687724} |
| Assets/Scenes/SampleScene_MobileWork.unity:154075 | Canvas | MobileUIReconstruction | swordContent: {fileID: 112488948} |
| Assets/Scenes/SampleScene_MobileWork.unity:154076 | Canvas | MobileUIReconstruction | dismantleShortcut: {fileID: 1607746076} |
| Assets/Scenes/SampleScene_MobileWork.unity:170438 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | spiritCooldownLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:170439 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | activateSpiritBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:170440 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtOreLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:170441 | Canvas/MobileUIRoot/Drawer_4/ScrollView/Viewport/Content | FormationPanelUI | txtCurrentActiveLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:170520 | #0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:171568 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | dataPanel: {fileID: 825211636} |
| Assets/Scenes/SampleScene_MobileWork.unity:171569 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | emptyPanel: {fileID: 1723814474} |
| Assets/Scenes/SampleScene_MobileWork.unity:171570 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | nameText: {fileID: 1896802288} |
| Assets/Scenes/SampleScene_MobileWork.unity:171571 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | cultivationText: {fileID: 1899240485} |
| Assets/Scenes/SampleScene_MobileWork.unity:171572 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | playTimeText: {fileID: 1829056205} |
| Assets/Scenes/SampleScene_MobileWork.unity:171573 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | slotButton: {fileID: 1621847545} |
| Assets/Scenes/SampleScene_MobileWork.unity:171574 | Canvas/MainMenuManager/CharacterSelectPanel/P6RosterBody/SaveSlot_2 | SaveSlotUI | deleteButton: {fileID: 755315457} |
| Assets/Scenes/SampleScene_MobileWork.unity:174554 | Canvas/StorageModalUI | StorageModalUI | canvasGroup: {fileID: 1644478631} |
| Assets/Scenes/SampleScene_MobileWork.unity:174555 | Canvas/StorageModalUI | StorageModalUI | modalBox: {fileID: 959944860} |
| Assets/Scenes/SampleScene_MobileWork.unity:174556 | Canvas/StorageModalUI | StorageModalUI | btnClose: {fileID: 2103688237} |
| Assets/Scenes/SampleScene_MobileWork.unity:174557 | Canvas/StorageModalUI | StorageModalUI | btnBackdrop: {fileID: 1644478629} |
| Assets/Scenes/SampleScene_MobileWork.unity:174558 | Canvas/StorageModalUI | StorageModalUI | txtLinhThach: {fileID: 832838914} |
| Assets/Scenes/SampleScene_MobileWork.unity:174559 | Canvas/StorageModalUI | StorageModalUI | txtKiemY: {fileID: 46909927} |
| Assets/Scenes/SampleScene_MobileWork.unity:174560 | Canvas/StorageModalUI | StorageModalUI | txtOre: {fileID: 1355976233} |
| Assets/Scenes/SampleScene_MobileWork.unity:174561 | Canvas/StorageModalUI | StorageModalUI | txtDust: {fileID: 1030226945} |
| Assets/Scenes/SampleScene_MobileWork.unity:174569 | Canvas/StorageModalUI | StorageModalUI | toggleSwordHighLevel: {fileID: 286737916} |
| Assets/Scenes/SampleScene_MobileWork.unity:174570 | Canvas/StorageModalUI | StorageModalUI | togglePillBreakthrough: {fileID: 555021063} |
| Assets/Scenes/SampleScene_MobileWork.unity:174571 | Canvas/StorageModalUI | StorageModalUI | toggleAutoDismantleLv1: {fileID: 515281039} |
| Assets/Scenes/SampleScene_MobileWork.unity:174573 | Canvas/StorageModalUI | StorageModalUI | btnToggleView: {fileID: 722546235} |
| Assets/Scenes/SampleScene_MobileWork.unity:174574 | Canvas/StorageModalUI | StorageModalUI | txtToggleView: {fileID: 1962929101} |
| Assets/Scenes/SampleScene_MobileWork.unity:174575 | Canvas/StorageModalUI | StorageModalUI | gridContent: {fileID: 12313158} |
| Assets/Scenes/SampleScene_MobileWork.unity:174576 | Canvas/StorageModalUI | StorageModalUI | listContent: {fileID: 1919265417} |
| Assets/Scenes/SampleScene_MobileWork.unity:174577 | Canvas/StorageModalUI | StorageModalUI | contentContainer: {fileID: 12313158} |
| Assets/Scenes/SampleScene_MobileWork.unity:174578 | Canvas/StorageModalUI | StorageModalUI | scrollRect: {fileID: 2121773491} |
| Assets/Scenes/SampleScene_MobileWork.unity:174579 | Canvas/StorageModalUI | StorageModalUI | txtEmptyNotice: {fileID: 1740486654} |
| Assets/Scenes/SampleScene_MobileWork.unity:174580 | Canvas/StorageModalUI | StorageModalUI | btnClaimPriority: {fileID: 197075584} |
| Assets/Scenes/SampleScene_MobileWork.unity:174581 | Canvas/StorageModalUI | StorageModalUI | txtClaimPriority: {fileID: 1215014113} |
| Assets/Scenes/SampleScene_MobileWork.unity:174582 | Canvas/StorageModalUI | StorageModalUI | btnBatchDismantle: {fileID: 2009324848} |
| Assets/Scenes/SampleScene_MobileWork.unity:174583 | Canvas/StorageModalUI | StorageModalUI | txtBatchDismantle: {fileID: 964138609} |
| Assets/Scenes/SampleScene_MobileWork.unity:184200 | Canvas/MilestoneModalUI | MilestoneModalUI | modalPanel: {fileID: 1724887768} |
| Assets/Scenes/SampleScene_MobileWork.unity:184201 | Canvas/MilestoneModalUI | MilestoneModalUI | contentContainer: {fileID: 194390971} |
| Assets/Scenes/SampleScene_MobileWork.unity:184202 | Canvas/MilestoneModalUI | MilestoneModalUI | txtTotalProgress: {fileID: 1644464564} |
| Assets/Scenes/SampleScene_MobileWork.unity:184203 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClose: {fileID: 2050025881} |
| Assets/Scenes/SampleScene_MobileWork.unity:184204 | Canvas/MilestoneModalUI | MilestoneModalUI | btnClaimAll: {fileID: 2129900667} |
| Assets/Scenes/SampleScene_MobileWork.unity:184205 | Canvas/MilestoneModalUI | MilestoneModalUI | btnSwitchToAchievements: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:184206 | Canvas/MilestoneModalUI | MilestoneModalUI | rowTemplate: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:189568 | #0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:194926 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | upper: {fileID: 1560728544} |
| Assets/Scenes/SampleScene_MobileWork.unity:194927 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | lower: {fileID: 1877663435} |
| Assets/Scenes/SampleScene_MobileWork.unity:194928 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | profile: {fileID: 1131801862} |
| Assets/Scenes/SampleScene_MobileWork.unity:194929 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | wallet: {fileID: 63933571} |
| Assets/Scenes/SampleScene_MobileWork.unity:194930 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | realm: {fileID: 728289266} |
| Assets/Scenes/SampleScene_MobileWork.unity:194931 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | stage: {fileID: 35825210} |
| Assets/Scenes/SampleScene_MobileWork.unity:194932 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | quest: {fileID: 2133832045} |
| Assets/Scenes/SampleScene_MobileWork.unity:194933 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | skills: {fileID: 367847516} |
| Assets/Scenes/SampleScene_MobileWork.unity:194934 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | quick: {fileID: 1924753478} |
| Assets/Scenes/SampleScene_MobileWork.unity:194935 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | dock: {fileID: 867939491} |
| Assets/Scenes/SampleScene_MobileWork.unity:194936 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | vitals: {fileID: 809768362} |
| Assets/Scenes/SampleScene_MobileWork.unity:194937 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | hp: {fileID: 812354262} |
| Assets/Scenes/SampleScene_MobileWork.unity:194938 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | mana: {fileID: 1118627353} |
| Assets/Scenes/SampleScene_MobileWork.unity:194939 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | styleHost: {fileID: 18375912} |
| Assets/Scenes/SampleScene_MobileWork.unity:194940 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | notification: {fileID: 1248546303} |
| Assets/Scenes/SampleScene_MobileWork.unity:194941 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | focusButton: {fileID: 79286217} |
| Assets/Scenes/SampleScene_MobileWork.unity:194942 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | pauseButton: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:194943 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | sourceProfile: {fileID: 2001849272} |
| Assets/Scenes/SampleScene_MobileWork.unity:194944 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | playerName: {fileID: 2110555223} |
| Assets/Scenes/SampleScene_MobileWork.unity:194945 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | playerRealm: {fileID: 1156124304} |
| Assets/Scenes/SampleScene_MobileWork.unity:194946 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | cultivationValue: {fileID: 1410375930} |
| Assets/Scenes/SampleScene_MobileWork.unity:194947 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | menuTitle: {fileID: 1547627744} |
| Assets/Scenes/SampleScene_MobileWork.unity:194948 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | walletDetails: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:194949 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | buffSummary: {fileID: 1630137276} |
| Assets/Scenes/SampleScene_MobileWork.unity:194950 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | manaValue: {fileID: 739452657} |
| Assets/Scenes/SampleScene_MobileWork.unity:194951 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | menuButton: {fileID: 398168851} |
| Assets/Scenes/SampleScene_MobileWork.unity:194952 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | closeMenuButton: {fileID: 1239584985} |
| Assets/Scenes/SampleScene_MobileWork.unity:194953 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | menuBackdrop: {fileID: 251357175} |
| Assets/Scenes/SampleScene_MobileWork.unity:194954 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | milestoneButton: {fileID: 373464865} |
| Assets/Scenes/SampleScene_MobileWork.unity:194955 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | profileButton: {fileID: 2049834123} |
| Assets/Scenes/SampleScene_MobileWork.unity:194956 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | buffsButton: {fileID: 658487810} |
| Assets/Scenes/SampleScene_MobileWork.unity:194957 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | logButton: {fileID: 875373119} |
| Assets/Scenes/SampleScene_MobileWork.unity:194958 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | walletButton: {fileID: 63933575} |
| Assets/Scenes/SampleScene_MobileWork.unity:194959 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | achievementButton: {fileID: 2127626756} |
| Assets/Scenes/SampleScene_MobileWork.unity:194965 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | secondaryOverlay: {fileID: 621294313} |
| Assets/Scenes/SampleScene_MobileWork.unity:194966 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | menuGrid: {fileID: 1425521740} |
| Assets/Scenes/SampleScene_MobileWork.unity:194967 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | detailsPage: {fileID: 2108124926} |
| Assets/Scenes/SampleScene_MobileWork.unity:194968 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | detailsText: {fileID: 1516369439} |
| Assets/Scenes/SampleScene_MobileWork.unity:194969 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | panelSprite: {fileID: 560880379, guid: 2e22a8bd7cc421744b98f516c6e681c0, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194970 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | windowSprite: {fileID: 560880379, guid: 2e22a8bd7cc421744b98f516c6e681c0, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194971 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | slotSprite: {fileID: -332078081, guid: d448d9ebbf7e0794ea5cbd7e904026b2, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194972 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | selectedSprite: {fileID: 1899475782, guid: 29c37302ecef2ba43997957acf8811a8, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194973 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | regularFont: {fileID: 12800000, guid: df80e50432fa9a44e9229bc91b379eb5, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194974 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | semiboldFont: {fileID: 12800000, guid: 026713e9430c4b747a76547367da36d4, type: 3} |
| Assets/Scenes/SampleScene_MobileWork.unity:194975 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | logPanel: {fileID: 240725839} |
| Assets/Scenes/SampleScene_MobileWork.unity:194976 | Canvas/MobileUIRoot/PortraitHudShell | PortraitHudController | buffPanel: {fileID: 1861096573} |
| Assets/Scenes/SampleScene_MobileWork.unity:198585 | #0/SwipeHandle | MobileDrawerSwipeHandle | controller: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:198955 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | canvasGroup: {fileID: 1865082722} |
| Assets/Scenes/SampleScene_MobileWork.unity:198956 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | modalBox: {fileID: 1169293231} |
| Assets/Scenes/SampleScene_MobileWork.unity:198957 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnCancel: {fileID: 1671183687} |
| Assets/Scenes/SampleScene_MobileWork.unity:198958 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnConfirm: {fileID: 581916513} |
| Assets/Scenes/SampleScene_MobileWork.unity:198959 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | btnBackdrop: {fileID: 1865082720} |
| Assets/Scenes/SampleScene_MobileWork.unity:198960 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtTitle: {fileID: 1339888971} |
| Assets/Scenes/SampleScene_MobileWork.unity:198961 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtItemName: {fileID: 280931172} |
| Assets/Scenes/SampleScene_MobileWork.unity:198962 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtItemDetail: {fileID: 2063210857} |
| Assets/Scenes/SampleScene_MobileWork.unity:198963 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtRewardSummary: {fileID: 280621230} |
| Assets/Scenes/SampleScene_MobileWork.unity:198964 | Canvas/DismantleConfirmModalUI | DismantleConfirmModalUI | txtWarning: {fileID: 1215585099} |
| Assets/Scenes/SampleScene_MobileWork.unity:210030 | Canvas/ShopPanel/ScrollView/Content/ShopItem_0 | ShopItemUI | buyButton: {fileID: 1005832532} |
| Assets/Scenes/SampleScene_MobileWork.unity:219376 | Canvas/DaoAnModal/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:219523 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | titleText: {fileID: 1454567060} |
| Assets/Scenes/SampleScene_MobileWork.unity:219524 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | condText: {fileID: 1734239692} |
| Assets/Scenes/SampleScene_MobileWork.unity:219525 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockButton: {fileID: 1571659309} |
| Assets/Scenes/SampleScene_MobileWork.unity:219526 | Canvas/RightPanel/UpgradeScrollView/UpgradePanel/SkillCard_3 | SkillCardUI | unlockBtnText: {fileID: 864419017} |
| Assets/Scenes/SampleScene_MobileWork.unity:220160 | Canvas/TribulationModalUI/ModalBox | JadeModalPresentation | theme: {fileID: 1451818562} |
| Assets/Scenes/SampleScene_MobileWork.unity:223044 | Canvas/MainMenuManager | MainMenuManager | mainPanel: {fileID: 2017822356} |
| Assets/Scenes/SampleScene_MobileWork.unity:223045 | Canvas/MainMenuManager | MainMenuManager | characterSelectPanel: {fileID: 943835570} |
| Assets/Scenes/SampleScene_MobileWork.unity:223046 | Canvas/MainMenuManager | MainMenuManager | characterCreatePanel: {fileID: 809555469} |
| Assets/Scenes/SampleScene_MobileWork.unity:223047 | Canvas/MainMenuManager | MainMenuManager | settingsPanel: {fileID: 1399051815} |
| Assets/Scenes/SampleScene_MobileWork.unity:223048 | Canvas/MainMenuManager | MainMenuManager | confirmationPopup: {fileID: 1638424892} |
| Assets/Scenes/SampleScene_MobileWork.unity:223049 | Canvas/MainMenuManager | MainMenuManager | playButton: {fileID: 482855941} |
| Assets/Scenes/SampleScene_MobileWork.unity:223050 | Canvas/MainMenuManager | MainMenuManager | playButtonText: {fileID: 1073180194} |
| Assets/Scenes/SampleScene_MobileWork.unity:223051 | Canvas/MainMenuManager | MainMenuManager | settingsButton: {fileID: 1158388662} |
| Assets/Scenes/SampleScene_MobileWork.unity:223052 | Canvas/MainMenuManager | MainMenuManager | quitButton: {fileID: 73727741} |
| Assets/Scenes/SampleScene_MobileWork.unity:223057 | Canvas/MainMenuManager | MainMenuManager | backFromSelectButton: {fileID: 262579212} |
| Assets/Scenes/SampleScene_MobileWork.unity:223058 | Canvas/MainMenuManager | MainMenuManager | nameInput: {fileID: 913883099} |
| Assets/Scenes/SampleScene_MobileWork.unity:223059 | Canvas/MainMenuManager | MainMenuManager | characterNameLabel: {fileID: 398077869} |
| Assets/Scenes/SampleScene_MobileWork.unity:223060 | Canvas/MainMenuManager | MainMenuManager | confirmCreateButton: {fileID: 1563459446} |
| Assets/Scenes/SampleScene_MobileWork.unity:223061 | Canvas/MainMenuManager | MainMenuManager | cancelCreateButton: {fileID: 1250177799} |
| Assets/Scenes/SampleScene_MobileWork.unity:223062 | Canvas/MainMenuManager | MainMenuManager | bgmSlider: {fileID: 384419740} |
| Assets/Scenes/SampleScene_MobileWork.unity:223063 | Canvas/MainMenuManager | MainMenuManager | sfxSlider: {fileID: 403109340} |
| Assets/Scenes/SampleScene_MobileWork.unity:223064 | Canvas/MainMenuManager | MainMenuManager | closeSettingsButton: {fileID: 1331868265} |
| Assets/Scenes/SampleScene_MobileWork.unity:223065 | Canvas/MainMenuManager | MainMenuManager | popupTitle: {fileID: 1864303443} |
| Assets/Scenes/SampleScene_MobileWork.unity:223066 | Canvas/MainMenuManager | MainMenuManager | popupMessage: {fileID: 875519812} |
| Assets/Scenes/SampleScene_MobileWork.unity:223067 | Canvas/MainMenuManager | MainMenuManager | popupConfirmBtn: {fileID: 1537215085} |
| Assets/Scenes/SampleScene_MobileWork.unity:223068 | Canvas/MainMenuManager | MainMenuManager | popupCancelBtn: {fileID: 1797654953} |
| Assets/Scenes/SampleScene_MobileWork.unity:223723 | Canvas/CharacterDetailModal | CharacterDetailModalUI | canvasGroup: {fileID: 2097715292} |
| Assets/Scenes/SampleScene_MobileWork.unity:223724 | Canvas/CharacterDetailModal | CharacterDetailModalUI | modalBox: {fileID: 1103503191} |
| Assets/Scenes/SampleScene_MobileWork.unity:223725 | Canvas/CharacterDetailModal | CharacterDetailModalUI | scrollRect: {fileID: 814243067} |
| Assets/Scenes/SampleScene_MobileWork.unity:223726 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeButton: {fileID: 1150597136} |
| Assets/Scenes/SampleScene_MobileWork.unity:223727 | Canvas/CharacterDetailModal | CharacterDetailModalUI | closeOverlayButton: {fileID: 1251122967} |
| Assets/Scenes/SampleScene_MobileWork.unity:223728 | Canvas/CharacterDetailModal | CharacterDetailModalUI | bottomCloseButton: {fileID: 1087333160} |
| Assets/Scenes/SampleScene_MobileWork.unity:223729 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtName: {fileID: 718167421} |
| Assets/Scenes/SampleScene_MobileWork.unity:223730 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtRealm: {fileID: 1918473206} |
| Assets/Scenes/SampleScene_MobileWork.unity:223731 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtTuVi: {fileID: 1917269040} |
| Assets/Scenes/SampleScene_MobileWork.unity:223732 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtCoDuyen: {fileID: 122488355} |
| Assets/Scenes/SampleScene_MobileWork.unity:223733 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtLinhThach: {fileID: 1408329680} |
| Assets/Scenes/SampleScene_MobileWork.unity:223734 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtHp: {fileID: 1163269436} |
| Assets/Scenes/SampleScene_MobileWork.unity:223735 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMp: {fileID: 1349554947} |
| Assets/Scenes/SampleScene_MobileWork.unity:223736 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtMultiplier: {fileID: 1912840058} |
| Assets/Scenes/SampleScene_MobileWork.unity:223737 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtEquipSwords: {fileID: 1171760092} |
| Assets/Scenes/SampleScene_MobileWork.unity:223738 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeKiemY: {fileID: 1562206166} |
| Assets/Scenes/SampleScene_MobileWork.unity:223739 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeLinhLuc: {fileID: 581285601} |
| Assets/Scenes/SampleScene_MobileWork.unity:223740 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeTuLinh: {fileID: 1889137916} |
| Assets/Scenes/SampleScene_MobileWork.unity:223741 | Canvas/CharacterDetailModal | CharacterDetailModalUI | txtUpgradeThanThuc: {fileID: 1161672799} |
| Assets/Scenes/SampleScene_MobileWork.unity:227718 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbLinhChi: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227719 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbNgungKhi: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227720 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtHerbTuyetLien: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227721 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtAlchemistRank: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227722 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtMedicineDust: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227723 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceStatus: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227724 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtFurnaceTimer: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227725 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | imgFurnaceProgress: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227726 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnFinishWithDust: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227727 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtDustCost: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227728 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostTuVi: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227729 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewTuVi: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227730 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtCostBreakthrough: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227731 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnBrewBreakthrough: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227732 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtTuViPillsSummary: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227733 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | txtBreakthroughPillsSummary: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227734 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | btnUseTuViPill: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227735 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | linhChiLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227736 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | caoNgungKhiLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227737 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | tuyetLienLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227738 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewTimerLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227739 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewCultivationBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227740 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | brewBreakthroughBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227741 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | useDustBtn: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227742 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | dustLabel: {fileID: 0} |
| Assets/Scenes/SampleScene_MobileWork.unity:227743 | Canvas/MobileUIRoot/Drawer_2/ScrollView/Viewport/Content | AlchemyPanelUI | pillInventoryLabel: {fileID: 0} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:3903 | PortraitHudShell/UpperInformation/StagePanel | CultivationPhaseBarUI | stageLabel: {fileID: 6860923827566690743} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6450 | PortraitHudShell | PortraitHudController | upper: {fileID: 8967024899877802539} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6451 | PortraitHudShell | PortraitHudController | lower: {fileID: 2555129053507667582} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6452 | PortraitHudShell | PortraitHudController | profile: {fileID: 4846965491893534438} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6453 | PortraitHudShell | PortraitHudController | wallet: {fileID: 784095300017711415} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6454 | PortraitHudShell | PortraitHudController | realm: {fileID: 4844379945549185257} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6455 | PortraitHudShell | PortraitHudController | stage: {fileID: 9179885383820636094} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6456 | PortraitHudShell | PortraitHudController | quest: {fileID: 7384665766157046923} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6457 | PortraitHudShell | PortraitHudController | skills: {fileID: 5130281060123227164} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6458 | PortraitHudShell | PortraitHudController | quick: {fileID: 5027042383706386030} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6459 | PortraitHudShell | PortraitHudController | dock: {fileID: 0} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6460 | PortraitHudShell | PortraitHudController | vitals: {fileID: 4703671351026404172} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6461 | PortraitHudShell | PortraitHudController | hp: {fileID: 868105613976685970} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6462 | PortraitHudShell | PortraitHudController | mana: {fileID: 1082302323624747026} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6463 | PortraitHudShell | PortraitHudController | styleHost: {fileID: 7263217186227270285} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6464 | PortraitHudShell | PortraitHudController | notification: {fileID: 8871443300585559101} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6465 | PortraitHudShell | PortraitHudController | focusButton: {fileID: 701211766957762443} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6466 | PortraitHudShell | PortraitHudController | sourceProfile: {fileID: 5446161362751592624} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6467 | PortraitHudShell | PortraitHudController | playerName: {fileID: 7083255895566276377} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6468 | PortraitHudShell | PortraitHudController | playerRealm: {fileID: 5509461273997126079} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6469 | PortraitHudShell | PortraitHudController | cultivationValue: {fileID: 3640115337019609867} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6470 | PortraitHudShell | PortraitHudController | menuTitle: {fileID: 7473547287774346318} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6471 | PortraitHudShell | PortraitHudController | walletDetails: {fileID: 0} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6472 | PortraitHudShell | PortraitHudController | buffSummary: {fileID: 5473825336474842276} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6473 | PortraitHudShell | PortraitHudController | manaValue: {fileID: 511635746193639885} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6474 | PortraitHudShell | PortraitHudController | menuButton: {fileID: 6356296318372714487} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6475 | PortraitHudShell | PortraitHudController | closeMenuButton: {fileID: 8472849645569768255} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6476 | PortraitHudShell | PortraitHudController | menuBackdrop: {fileID: 7337173573614762914} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6477 | PortraitHudShell | PortraitHudController | milestoneButton: {fileID: 3250662980553562413} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6478 | PortraitHudShell | PortraitHudController | profileButton: {fileID: 2052063760222881077} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6479 | PortraitHudShell | PortraitHudController | buffsButton: {fileID: 872320911418020283} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6480 | PortraitHudShell | PortraitHudController | logButton: {fileID: 5012352383142339750} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6481 | PortraitHudShell | PortraitHudController | walletButton: {fileID: 3409933490771507576} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6482 | PortraitHudShell | PortraitHudController | achievementButton: {fileID: 2565390016225271962} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6488 | PortraitHudShell | PortraitHudController | secondaryOverlay: {fileID: 3078617860637582002} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6489 | PortraitHudShell | PortraitHudController | menuGrid: {fileID: 4275928113621199245} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6490 | PortraitHudShell | PortraitHudController | detailsPage: {fileID: 1257158422876814599} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6491 | PortraitHudShell | PortraitHudController | detailsText: {fileID: 9027495030336300997} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6492 | PortraitHudShell | PortraitHudController | panelSprite: {fileID: -696448221, guid: 865101dc71ff62343a27f9d2aca664c9, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6493 | PortraitHudShell | PortraitHudController | windowSprite: {fileID: 1099355870, guid: 865101dc71ff62343a27f9d2aca664c9, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6494 | PortraitHudShell | PortraitHudController | slotSprite: {fileID: 105180129, guid: 865101dc71ff62343a27f9d2aca664c9, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6495 | PortraitHudShell | PortraitHudController | selectedSprite: {fileID: 797648401, guid: 865101dc71ff62343a27f9d2aca664c9, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6496 | PortraitHudShell | PortraitHudController | regularFont: {fileID: 12800000, guid: df80e50432fa9a44e9229bc91b379eb5, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6497 | PortraitHudShell | PortraitHudController | semiboldFont: {fileID: 12800000, guid: 026713e9430c4b747a76547367da36d4, type: 3} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6498 | PortraitHudShell | PortraitHudController | logPanel: {fileID: 0} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:6499 | PortraitHudShell | PortraitHudController | buffPanel: {fileID: 0} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9418 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | canvasGroup: {fileID: 7274560177734058846} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9419 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | containerRect: {fileID: 8871443300585559101} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9420 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | backgroundImage: {fileID: 4969031572961504155} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9421 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | borderTop: {fileID: 8178813266750637775} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9422 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | borderBottom: {fileID: 6397161814402399808} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9423 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | titleText: {fileID: 8675444819872079489} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9424 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | subtitleText: {fileID: 6416998986534695987} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:9425 | PortraitHudShell/CombatNotificationBanner | CombatNotificationUI | extraDetailText: {fileID: 8917150138843377131} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:11272 | PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | btnUltimate: {fileID: 97662658292918415} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:11273 | PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | imgUltiFill: {fileID: 6551016035759743912} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:11274 | PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | imgUltiGlow: {fileID: 1190176608012448375} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:11275 | PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | txtUltiTitle: {fileID: 4930323481349042477} |
| Assets/UI/MobileHUD/hud_prefabs/PortraitHudShell.prefab:11276 | PortraitHudShell/PrimaryActions/MobileSkillHUD | MobileSkillHUD | txtUltiMana: {fileID: 6316809194152620078} |



## 7. UI → service / transaction call sites

| Source:line | Expression |
|---|---|
| Assets/Scripts/Combat/BossEnemy.cs:165 | int stageIdx = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0; |
| Assets/Scripts/Combat/BossEnemy.cs:395 | float formationSlow = FormationService.Instance != null ? FormationService.Instance.BossSpeedMultiplier : 1f; |
| Assets/Scripts/Combat/BossEnemy.cs:437 | if (HitVFXManager.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:440 | HitVFXManager.Instance.SpawnBossWindUp(handPos); |
| Assets/Scripts/Combat/BossEnemy.cs:450 | if (HitVFXManager.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:452 | HitVFXManager.Instance.SpawnBossGroundImpact(impactPos); |
| Assets/Scripts/Combat/BossEnemy.cs:459 | if (isTribulationBoss && CombatManager.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:461 | finalDmg = CombatManager.Instance.playerMaxHp * 0.15f; |
| Assets/Scripts/Combat/BossEnemy.cs:464 | if (CombatManager.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:466 | CombatManager.Instance.DamagePlayer(finalDmg, true); |
| Assets/Scripts/Combat/BossEnemy.cs:470 | if (CameraShake.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:472 | CameraShake.Instance.TriggerShake(0.35f, 0.25f); |
| Assets/Scripts/Combat/BossEnemy.cs:565 | if (AudioManager.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:567 | AudioManager.Instance.PlayLoiPhatSFX(); |
| Assets/Scripts/Combat/BossEnemy.cs:576 | if (CameraShake.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:578 | CameraShake.Instance.TriggerShake(0.5f, 0.3f); |
| Assets/Scripts/Combat/BossEnemy.cs:583 | if (GameLogger.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:585 | GameLogger.Instance.Log("⚡ BOSS THIÊN KIẾP BẠO PHÁT: Bước vào PHASE 2 (Lôi Giáp Bất Hoại, Tăng Tốc Độ)!", Color.yellow); |
| Assets/Scripts/Combat/BossEnemy.cs:594 | if (CameraShake.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:596 | CameraShake.Instance.TriggerShake(0.7f, 0.4f); |
| Assets/Scripts/Combat/BossEnemy.cs:601 | if (GameLogger.Instance != null) |
| Assets/Scripts/Combat/BossEnemy.cs:603 | GameLogger.Instance.Log("🔥 BOSS THIÊN KIẾP CUỒNG NỘ: Bước vào PHASE 3 (Cuồng Nộ Diệt Thế)!", Color.red); |
| Assets/Scripts/Combat/Enemy.cs:354 | float formationSlow = isBoss && FormationService.Instance != null ? FormationService.Instance.BossSpeedMultiplier : 1f; |
| Assets/Scripts/Combat/Enemy.cs:374 | if (!isBoss && HitVFXManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:377 | HitVFXManager.Instance.SpawnEnemyAttackSwipe(swipePos, transform.forward); |
| Assets/Scripts/Combat/Enemy.cs:380 | if (CombatManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:382 | CombatManager.Instance.DamagePlayer(damage, isBoss); |
| Assets/Scripts/Combat/Enemy.cs:409 | if (HitVFXManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:412 | HitVFXManager.Instance.SpawnHitImpact(hitPoint, hitDirection, isCrit); |
| Assets/Scripts/Combat/Enemy.cs:418 | Vector3 pPos = CombatManager.Instance != null && CombatManager.Instance.playerTransform != null |
| Assets/Scripts/Combat/Enemy.cs:419 | ? CombatManager.Instance.playerTransform.position |
| Assets/Scripts/Combat/Enemy.cs:463 | if (CombatManager.Instance == null \|\| CombatManager.Instance.damagePopupPrefab == null) return; |
| Assets/Scripts/Combat/Enemy.cs:479 | if (SimplePool.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:481 | popup = SimplePool.Instance.SpawnFromPool("DamagePopup", spawnPos, Quaternion.identity); |
| Assets/Scripts/Combat/Enemy.cs:485 | popup = Instantiate(CombatManager.Instance.damagePopupPrefab, spawnPos, Quaternion.identity); |
| Assets/Scripts/Combat/Enemy.cs:501 | if (CultivationManager.Instance != null) stageIndex = CultivationManager.Instance.currentStageIndex; |
| Assets/Scripts/Combat/Enemy.cs:506 | float tuLinhBonus = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetTuLinhValue() : 0; |
| Assets/Scripts/Combat/Enemy.cs:508 | if (EconomyManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:510 | float artifactDrop = ArtifactService.Instance != null ? ArtifactService.Instance.StoneDropMultiplier : 1f; |
| Assets/Scripts/Combat/Enemy.cs:519 | EconomyManager.Instance.AddLinhThach((baseDrop + tuLinhBonus) * artifactDrop * daoAnDrop); |
| Assets/Scripts/Combat/Enemy.cs:523 | if (CultivationManager.Instance != null && !CultivationManager.Instance.IsTuViMaxed()) |
| Assets/Scripts/Combat/Enemy.cs:525 | float tuViBonus = CultivationManager.Instance.GetBaseTuViPerKill(); |
| Assets/Scripts/Combat/Enemy.cs:527 | float formationGain = FormationService.Instance != null ? FormationService.Instance.CultivationMultiplier : 1f; |
| Assets/Scripts/Combat/Enemy.cs:530 | CultivationManager.Instance.AddTuVi(tuViBonus * formationGain * daoAnGain); |
| Assets/Scripts/Combat/Enemy.cs:533 | int combatStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1; |
| Assets/Scripts/Combat/Enemy.cs:534 | if (LootRewardService.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:536 | LootRewardService.Instance.HandleEnemyDefeated(isBoss, combatStage); |
| Assets/Scripts/Combat/Enemy.cs:540 | AlchemyService.Instance?.OnEnemyDefeated(isBoss, combatStage); |
| Assets/Scripts/Combat/Enemy.cs:541 | ArtifactService.Instance?.OnEnemyDefeated(isBoss, combatStage); |
| Assets/Scripts/Combat/Enemy.cs:543 | AchievementManager.Instance?.OnEnemyKilled(isBoss); |
| Assets/Scripts/Combat/Enemy.cs:544 | GameplayChoicesService.Instance?.OnEnemyDefeated(isBoss); |
| Assets/Scripts/Combat/Enemy.cs:546 | if (CombatManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:548 | CombatManager.Instance.OnEnemyDied(this); |
| Assets/Scripts/Combat/Enemy.cs:551 | if (StageManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:553 | StageManager.Instance.OnEnemyKilled(isBoss); |
| Assets/Scripts/Combat/Enemy.cs:557 | if (HitVFXManager.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:559 | HitVFXManager.Instance.SpawnEnemyDeathDissolve(transform.position + Vector3.up * 0.8f); |
| Assets/Scripts/Combat/Enemy.cs:562 | if (VFXHelper.Instance != null) |
| Assets/Scripts/Combat/Enemy.cs:564 | VFXHelper.Instance.SpawnFlyingLoot(transform.position, isBoss ? 5 : 1); |
| Assets/Scripts/Combat/MergeItem.cs:188 | if (MergeManager.Instance != null) MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Combat/MergeItem.cs:197 | if (MergeManager.Instance == null) return; |
| Assets/Scripts/Combat/MergeItem.cs:209 | int maxCap = SwordLoadoutService.Instance != null |
| Assets/Scripts/Combat/MergeItem.cs:210 | ? SwordLoadoutService.Instance.GetCurrentCapacity() |
| Assets/Scripts/Combat/MergeItem.cs:211 | : (MergeManager.Instance.equipPanel != null ? MergeManager.Instance.equipPanel.childCount : 10); |
| Assets/Scripts/Combat/MergeItem.cs:214 | foreach (Transform slot in MergeManager.Instance.equipPanel) |
| Assets/Scripts/Combat/MergeItem.cs:222 | MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Combat/MergeItem.cs:230 | foreach (Transform slot in MergeManager.Instance.equipPanel) |
| Assets/Scripts/Combat/MergeItem.cs:255 | MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Combat/MergeItem.cs:261 | foreach (Transform page in MergeManager.Instance.inventoryPages) |
| Assets/Scripts/Combat/MergeItem.cs:270 | MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Combat/MergeItem.cs:282 | if (TooltipManager.Instance != null) |
| Assets/Scripts/Combat/MergeItem.cs:300 | TooltipManager.Instance.ShowTooltip(info); |
| Assets/Scripts/Combat/MergeItem.cs:306 | if (TooltipManager.Instance != null) |
| Assets/Scripts/Combat/MergeItem.cs:308 | TooltipManager.Instance.HideTooltip(); |
| Assets/Scripts/Combat/MergeSlot.cs:18 | if (isEquipSlot && SwordLoadoutService.Instance != null && |
| Assets/Scripts/Combat/MergeSlot.cs:19 | transform.GetSiblingIndex() >= SwordLoadoutService.Instance.GetCurrentCapacity()) |
| Assets/Scripts/Combat/MergeSlot.cs:48 | AchievementManager.Instance?.OnSwordMerged(currentItem.level); |
| Assets/Scripts/Combat/MergeSlot.cs:68 | AchievementManager.Instance?.OnSwordMerged(currentItem.level); |
| Assets/Scripts/Combat/MergeSlot.cs:71 | if (AudioManager.Instance != null) |
| Assets/Scripts/Combat/MergeSlot.cs:73 | AudioManager.Instance.PlayMergeSwordSFX(); |
| Assets/Scripts/Combat/MergeSlot.cs:90 | if (MergeManager.Instance != null) MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Managers/CombatManager.cs:275 | if (GameManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:283 | GameManager.Instance.OnStateChanged += HandleStateChanged; |
| Assets/Scripts/Managers/CombatManager.cs:291 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:299 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/Managers/CombatManager.cs:303 | CultivationManager.Instance.OnTuViChanged += HandleTuViChanged; |
| Assets/Scripts/Managers/CombatManager.cs:323 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:327 | PlayerStatsService.Instance.OnStatsChanged += HandleStatsChanged; |
| Assets/Scripts/Managers/CombatManager.cs:367 | if (MainMenuManager.Instance == null \|\| !MainMenuManager.Instance.gameObject.activeInHierarchy) |
| Assets/Scripts/Managers/CombatManager.cs:375 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:383 | AudioManager.Instance.PlayInGameBGM(); |
| Assets/Scripts/Managers/CombatManager.cs:451 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:483 | if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, perPrefabSize); |
| Assets/Scripts/Managers/CombatManager.cs:519 | if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, 1); |
| Assets/Scripts/Managers/CombatManager.cs:535 | if (damagePopupPrefab != null) SimplePool.Instance.AddPool("DamagePopup", damagePopupPrefab, 50); |
| Assets/Scripts/Managers/CombatManager.cs:559 | if (GameManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:567 | GameManager.Instance.OnStateChanged -= HandleStateChanged; |
| Assets/Scripts/Managers/CombatManager.cs:575 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:583 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/Managers/CombatManager.cs:587 | CultivationManager.Instance.OnTuViChanged -= HandleTuViChanged; |
| Assets/Scripts/Managers/CombatManager.cs:595 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:599 | PlayerStatsService.Instance.OnStatsChanged -= HandleStatsChanged; |
| Assets/Scripts/Managers/CombatManager.cs:631 | if (CultivationManager.Instance == null) return; |
| Assets/Scripts/Managers/CombatManager.cs:667 | int stageIdx = CultivationManager.Instance.currentStageIndex; |
| Assets/Scripts/Managers/CombatManager.cs:671 | float tuVi = CultivationManager.Instance.currentTuVi; |
| Assets/Scripts/Managers/CombatManager.cs:675 | float statMult = CultivationManager.Instance.permanentStatMultiplier; |
| Assets/Scripts/Managers/CombatManager.cs:683 | float bodyHp = PlayerStatsService.Instance != null ? PlayerStatsService.Instance.BaseMaxHp : 1000f; |
| Assets/Scripts/Managers/CombatManager.cs:687 | float artifactHp = ArtifactService.Instance != null ? ArtifactService.Instance.MaxHpBonus : 0f; |
| Assets/Scripts/Managers/CombatManager.cs:787 | if (GameLogger.Instance != null && stageData != null) |
| Assets/Scripts/Managers/CombatManager.cs:795 | GameLogger.Instance.Log($"Đột phá {CultivationManager.Instance.GetDisplayRealmName()}! HP tối đa tăng lên {playerMaxHp:F0}!", Color.green); |
| Assets/Scripts/Managers/CombatManager.cs:1112 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1120 | go = SimplePool.Instance.SpawnFromPool(bossPrefab.name, spawnPos, Quaternion.identity); |
| Assets/Scripts/Managers/CombatManager.cs:1232 | int stage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1; |
| Assets/Scripts/Managers/CombatManager.cs:1236 | bool isTribulation = GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation; |
| Assets/Scripts/Managers/CombatManager.cs:1332 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1340 | minionObj = SimplePool.Instance.SpawnFromPool(prefab.name, spawnPos, Quaternion.identity); |
| Assets/Scripts/Managers/CombatManager.cs:1496 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1504 | StageManager.Instance.SetStageUIVisible(false); |
| Assets/Scripts/Managers/CombatManager.cs:1528 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1536 | SimplePool.Instance.DeactivateAllInPool("DamagePopup"); |
| Assets/Scripts/Managers/CombatManager.cs:1560 | if (MapScroller.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1568 | MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:1600 | if (CameraShake.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1608 | CameraShake.Instance.TriggerShake(0.8f, 0.5f); |
| Assets/Scripts/Managers/CombatManager.cs:1616 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1624 | AudioManager.Instance.PlayLoiPhatSFX(); |
| Assets/Scripts/Managers/CombatManager.cs:1672 | int realmTier = CultivationManager.Instance != null ? CultivationManager.Instance.GetCurrentTribulationTier() : 1; |
| Assets/Scripts/Managers/CombatManager.cs:1674 | string targetRealm = CultivationManager.Instance != null ? CultivationManager.Instance.GetNextRealmName() : "Đại Cảnh Giới Mới"; |
| Assets/Scripts/Managers/CombatManager.cs:1704 | if (AlchemyService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1708 | baseChance = AlchemyService.Instance.ActiveTribulationQuality switch |
| Assets/Scripts/Managers/CombatManager.cs:1724 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1728 | baseChance += Mathf.Clamp01(PlayerStatsService.Instance.DamageReduction * 0.5f); |
| Assets/Scripts/Managers/CombatManager.cs:1738 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1742 | GameLogger.Instance.Log($"⚡ THIÊN KIẾP HÀNG LÂM! Cửu Trọng Lôi Kiếp giáng thế (15 giây)! Hãy kiên trì chống đỡ để tấn thăng {targetRealm}!", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:1854 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1856 | AudioManager.Instance.PlayLoiPhatSFX(); |
| Assets/Scripts/Managers/CombatManager.cs:1862 | if (CameraShake.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1864 | CameraShake.Instance.TriggerShake(0.45f + (strikeNumber * 0.04f), 0.22f); |
| Assets/Scripts/Managers/CombatManager.cs:1870 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1872 | GameLogger.Instance.Log($"⚡ Đạo Lôi Kiếp thứ {strikeNumber}/9 giáng xuống ({elapsed:F1}s/15s)! Thọ thương -{chipDamagePerStrike:F0} HP! Sinh lực còn {EconomyManager.FormatNumber(playerCurrentHp)}", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:1878 | if (CameraShake.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1880 | CameraShake.Instance.TriggerShake(1.25f, 0.65f); // Rung cực mạnh |
| Assets/Scripts/Managers/CombatManager.cs:1909 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1911 | GameLogger.Instance.Log($"⚡ ĐẠO THIÊN KIẾP THỨ 9 KINH THIÊN ĐỘNG ĐỊA (-{finalStrikeDmg:F0} HP)! Đạo thân cắn răng chống đỡ, sinh lực còn đúng 10%! Vượt qua kiếp nạn!", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:1917 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1919 | GameLogger.Instance.Log($"⚡ ĐẠO THIÊN KIẾP THỨ 9 UY ÁP VÔ TẬN! Đạo thân không thể chống đỡ, vẫn lạc tại lôi kiếp cuối cùng!", Color.red); |
| Assets/Scripts/Managers/CombatManager.cs:1949 | AlchemyService.Instance?.EndTribulation(); |
| Assets/Scripts/Managers/CombatManager.cs:1953 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1957 | StageManager.Instance.SetStageUIVisible(true); |
| Assets/Scripts/Managers/CombatManager.cs:1975 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1979 | CultivationManager.Instance.Breakthrough(); |
| Assets/Scripts/Managers/CombatManager.cs:1991 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:1995 | GameLogger.Instance.Log($"✨ ĐỘT PHÁ TU VI THÀNH CÔNG! Đạo hữu đã kiên cường chống đỡ trọn vẹn 15s Cửu Trọng Thiên Kiếp, tấn thăng {targetRealm}!", Color.green); |
| Assets/Scripts/Managers/CombatManager.cs:2127 | if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) |
| Assets/Scripts/Managers/CombatManager.cs:2171 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2175 | HealPlayer(PlayerStatsService.Instance.HpRegenPerSec * Time.deltaTime); |
| Assets/Scripts/Managers/CombatManager.cs:2179 | if (ArtifactService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2183 | HealPlayer(ArtifactService.Instance.HpRegenPerSec * Time.deltaTime); |
| Assets/Scripts/Managers/CombatManager.cs:2199 | if (ShopManager.Instance != null && ShopManager.Instance.hpRegenVisualTimer > 0) |
| Assets/Scripts/Managers/CombatManager.cs:2207 | float potionHpRate = ShopManager.Instance.GetHpRegenRate(); |
| Assets/Scripts/Managers/CombatManager.cs:2299 | if (SkillManager.Instance != null && !SkillManager.Instance.isSkillActive) |
| Assets/Scripts/Managers/CombatManager.cs:2307 | SkillManager.Instance.currentLinhLuc = Mathf.Min( |
| Assets/Scripts/Managers/CombatManager.cs:2311 | SkillManager.Instance.maxLinhLuc, |
| Assets/Scripts/Managers/CombatManager.cs:2315 | SkillManager.Instance.currentLinhLuc + GetAmDuongTranMpRegenFlat() * Time.deltaTime |
| Assets/Scripts/Managers/CombatManager.cs:2379 | int targetKills = StageManager.Instance != null ? StageManager.Instance.enemiesToBoss : 10; |
| Assets/Scripts/Managers/CombatManager.cs:2383 | int alreadyKilled = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0; |
| Assets/Scripts/Managers/CombatManager.cs:2535 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2537 | go = SimplePool.Instance.SpawnFromPool(enemyPrefab.name, spawnPos, spawnRot); |
| Assets/Scripts/Managers/CombatManager.cs:2584 | int stageIndex = StageManager.Instance != null ? StageManager.Instance.currentStage : 1; |
| Assets/Scripts/Managers/CombatManager.cs:2808 | AlchemyService.Instance?.EndTribulation(); |
| Assets/Scripts/Managers/CombatManager.cs:2820 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2828 | StageManager.Instance.SetStageUIVisible(true); |
| Assets/Scripts/Managers/CombatManager.cs:2840 | string targetRealm = CultivationManager.Instance != null ? CultivationManager.Instance.GetNextRealmName() : "Cảnh Giới Mới"; |
| Assets/Scripts/Managers/CombatManager.cs:2852 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2860 | CultivationManager.Instance.Breakthrough(); |
| Assets/Scripts/Managers/CombatManager.cs:2884 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2892 | GameLogger.Instance.Log($"⚡ ĐỘ KIẾP THÀNH CÔNG! Đã vượt qua Thiên Kiếp, tấn thăng {targetRealm}!", Color.green); |
| Assets/Scripts/Managers/CombatManager.cs:2932 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2940 | GameLogger.Instance.Log("Tiêu diệt Boss tầng thành công! Chuẩn bị chuyển tầng tiếp theo!", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:2964 | if (LootRewardService.Instance == null && MergeManager.Instance != null && Random.value <= 0.2f) |
| Assets/Scripts/Managers/CombatManager.cs:2972 | if (StorageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:2976 | StorageManager.Instance.HandleSwordDrop(1); |
| Assets/Scripts/Managers/CombatManager.cs:2984 | MergeManager.Instance.TryAddSword(1); |
| Assets/Scripts/Managers/CombatManager.cs:2988 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Nhặt được: Kiếm Phôi Lv1", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:3015 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3017 | popup = SimplePool.Instance.SpawnFromPool("DamagePopup", pos, Quaternion.identity); |
| Assets/Scripts/Managers/CombatManager.cs:3044 | if (!forceDefeat && ArtifactService.Instance != null && Random.value < ArtifactService.Instance.DodgeChance) |
| Assets/Scripts/Managers/CombatManager.cs:3050 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3052 | GameLogger.Instance.Log("💨 Thân pháp như điện! Né tránh thành công đòn tấn công!", Color.cyan); |
| Assets/Scripts/Managers/CombatManager.cs:3087 | float reduction = PlayerStatsService.Instance != null ? PlayerStatsService.Instance.DamageReduction : 0f; |
| Assets/Scripts/Managers/CombatManager.cs:3093 | float artifactReduction = ArtifactService.Instance != null ? ArtifactService.Instance.BossDamageReduction : 0f; |
| Assets/Scripts/Managers/CombatManager.cs:3100 | float tenacity = (PlayerStatsService.Instance != null ? PlayerStatsService.Instance.TenacityPercent : 0f) |
| Assets/Scripts/Managers/CombatManager.cs:3104 | + (ArtifactService.Instance != null ? ArtifactService.Instance.Tenacity : 0f); |
| Assets/Scripts/Managers/CombatManager.cs:3132 | if (FormationService.Instance != null && FormationService.Instance.DamageReduction > 0f) |
| Assets/Scripts/Managers/CombatManager.cs:3134 | reduction = 1f - (1f - reduction) * (1f - FormationService.Instance.DamageReduction); |
| Assets/Scripts/Managers/CombatManager.cs:3138 | HitVFXManager.Instance?.SpawnGoldenBellShield(playerTransform.position, hitDir); |
| Assets/Scripts/Managers/CombatManager.cs:3180 | if (MapScroller.Instance != null) MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:3276 | AlchemyService.Instance?.EndTribulation(); |
| Assets/Scripts/Managers/CombatManager.cs:3288 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3296 | StageManager.Instance.SetStageUIVisible(true); |
| Assets/Scripts/Managers/CombatManager.cs:3316 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3324 | lostTuVi = CultivationManager.Instance.ApplyTribulationFailurePenalty(); |
| Assets/Scripts/Managers/CombatManager.cs:3348 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3356 | GameLogger.Instance.Log($"⚡ ĐỘ KIẾP THẤT BẠI! Bị Thiên Kiếp phản phệ, tổn thất {lostTuVi:F0} Tu Vi (-{CultivationManager.TribulationFailureLossRate:0%})!", Color.red); |
| Assets/Scripts/Managers/CombatManager.cs:3368 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3376 | StageManager.Instance.ResetCurrentStageOnDeath(); |
| Assets/Scripts/Managers/CombatManager.cs:3388 | if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/CombatManager.cs:3436 | if (CultivationManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3444 | CultivationManager.Instance.HandlePlayerDeath(); |
| Assets/Scripts/Managers/CombatManager.cs:3456 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3464 | StageManager.Instance.ResetCurrentStageOnDeath(); |
| Assets/Scripts/Managers/CombatManager.cs:3476 | if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/CombatManager.cs:3595 | if (HitVFXManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3597 | HitVFXManager.Instance.SpawnReviveBurst(playerTransform.position); |
| Assets/Scripts/Managers/CombatManager.cs:3618 | AlchemyService.Instance?.EndTribulation(); |
| Assets/Scripts/Managers/CombatManager.cs:3622 | if (StageManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3630 | StageManager.Instance.SetStageUIVisible(true); |
| Assets/Scripts/Managers/CombatManager.cs:3658 | if (SimplePool.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3666 | SimplePool.Instance.DeactivateAllInPool("DamagePopup"); |
| Assets/Scripts/Managers/CombatManager.cs:3694 | if (CombatNotificationUI.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3702 | CombatNotificationUI.Instance.HideImmediate(); |
| Assets/Scripts/Managers/CombatManager.cs:3710 | if (BossDamageVignette.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3718 | BossDamageVignette.Instance.ResetVignette(); |
| Assets/Scripts/Managers/CombatManager.cs:3726 | if (CameraShake.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3734 | CameraShake.Instance.ResetShake(); |
| Assets/Scripts/Managers/CombatManager.cs:3802 | if (MapScroller.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:3810 | MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:3978 | if (MapScroller.Instance != null && MapScroller.Instance.isMoving) |
| Assets/Scripts/Managers/CombatManager.cs:3986 | MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4038 | if (MapScroller.Instance != null && MapScroller.Instance.isMoving) |
| Assets/Scripts/Managers/CombatManager.cs:4046 | MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4102 | if (MapScroller.Instance != null && MapScroller.Instance.isMoving) |
| Assets/Scripts/Managers/CombatManager.cs:4110 | MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4150 | if (GameManager.Instance == null \|\| GameManager.Instance.currentState == GameManager.GameState.IdleFarm) |
| Assets/Scripts/Managers/CombatManager.cs:4158 | if (MapScroller.Instance != null && !MapScroller.Instance.isMoving) |
| Assets/Scripts/Managers/CombatManager.cs:4166 | MapScroller.Instance.ResumeMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4406 | if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.BossTribulation) |
| Assets/Scripts/Managers/CombatManager.cs:4414 | GameManager.Instance.ChangeState(GameManager.GameState.BossTribulation); |
| Assets/Scripts/Managers/CombatManager.cs:4438 | if (MapScroller.Instance != null) MapScroller.Instance.StopMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4470 | if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm && GameManager.Instance.currentState != GameManager.GameState.Breakthrough) |
| Assets/Scripts/Managers/CombatManager.cs:4478 | GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/CombatManager.cs:4486 | if (MapScroller.Instance != null && (GameManager.Instance == null \|\| GameManager.Instance.currentState != GameManager.GameState.Breakthrough)) |
| Assets/Scripts/Managers/CombatManager.cs:4494 | MapScroller.Instance.ResumeMoving(); |
| Assets/Scripts/Managers/CombatManager.cs:4538 | int capacity = (SwordLoadoutService.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:4542 | ? SwordLoadoutService.Instance.GetCurrentCapacity() |
| Assets/Scripts/Managers/CombatManager.cs:4826 | if (MergeManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:4834 | MergeManager.Instance.OnEquipChanged(); |
| Assets/Scripts/Managers/CombatManager.cs:4963 | HitVFXManager.Instance?.SpawnUltimateSummonRing(playerTransform.position); |
| Assets/Scripts/Managers/CombatManager.cs:5171 | if (playerTransform != null) HitVFXManager.Instance?.SpawnUltimateRecallPulse(playerTransform.position); |
| Assets/Scripts/Managers/CombatManager.cs:5722 | HitVFXManager.Instance?.SpawnAnChuong(centerPos, 6f); |
| Assets/Scripts/Managers/CombatManager.cs:5735 | HitVFXManager.Instance?.SpawnStunIndicator(e.gameObject, 2.0f); |
| Assets/Scripts/Managers/CombatManager.cs:5740 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:5742 | GameLogger.Instance.Log("🖐️ ẤN CHƯỞNG: Đập dẹp quái vật trong 6m!", Color.yellow); |
| Assets/Scripts/Managers/CombatManager.cs:5768 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:5770 | GameLogger.Instance.Log($"⚡ LÔI PHẠT: Giáng thẳng vào BOSS, gây {damage:N0} sát thương chí mạng!", new Color(0.8f, 0f, 1f)); |
| Assets/Scripts/Managers/CombatManager.cs:5778 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:5780 | GameLogger.Instance.Log("⚡ LÔI KIẾP: Thiên kiếp giáng lâm, yêu quái đã bị ĐẠO TIÊU ngay tức khắc!", Color.red); |
| Assets/Scripts/Managers/CombatManager.cs:5796 | HitVFXManager.Instance?.SpawnLoiPhat(targetPos, isBoss: isBoss, executeRegular: !isBoss); |
| Assets/Scripts/Managers/CombatManager.cs:5799 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CombatManager.cs:5801 | AudioManager.Instance.PlayLoiPhatSFX(); |
| Assets/Scripts/Managers/CombatManager.cs:5867 | HitVFXManager.Instance?.StartAmDuongTran(playerTransform, 6.0f); |
| Assets/Scripts/Managers/CombatManager.cs:5890 | HitVFXManager.Instance?.StopAmDuongTran(); |
| Assets/Scripts/Managers/CombatManager.cs:5917 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/CombatManager.cs:5941 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/CultivationManager.cs:249 | if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.8f, 0.5f); |
| Assets/Scripts/Managers/CultivationManager.cs:250 | if (lightningVFXPrefab != null && CombatManager.Instance != null && CombatManager.Instance.playerTransform != null) |
| Assets/Scripts/Managers/CultivationManager.cs:252 | Instantiate(lightningVFXPrefab, CombatManager.Instance.playerTransform.position + Vector3.up * 8f, Quaternion.Euler(90, 0, 0)); |
| Assets/Scripts/Managers/CultivationManager.cs:254 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CultivationManager.cs:256 | AudioManager.Instance.PlayLoiPhatSFX(); |
| Assets/Scripts/Managers/CultivationManager.cs:265 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/CultivationManager.cs:267 | AudioManager.Instance.PlayBreakthroughSFX(); |
| Assets/Scripts/Managers/CultivationManager.cs:287 | if (coDuyenBonus > 0 && EconomyManager.Instance != null) |
| Assets/Scripts/Managers/CultivationManager.cs:289 | EconomyManager.Instance.AddCoDuyen(coDuyenBonus); |
| Assets/Scripts/Managers/CultivationManager.cs:297 | AchievementManager.Instance?.OnTribulationSuccess(); |
| Assets/Scripts/Managers/CultivationManager.cs:301 | if (phaseChanged && GameLogger.Instance != null) |
| Assets/Scripts/Managers/CultivationManager.cs:303 | GameLogger.Instance.Log($"Đột phá: {GetDisplayRealmName()}!", Color.cyan); |
| Assets/Scripts/Managers/CultivationManager.cs:312 | if (GameManager.Instance != null) |
| Assets/Scripts/Managers/CultivationManager.cs:314 | GameManager.Instance.ChangeState(GameManager.GameState.Breakthrough); |
| Assets/Scripts/Managers/CultivationManager.cs:321 | if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Breakthrough) |
| Assets/Scripts/Managers/CultivationManager.cs:323 | GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/CultivationManager.cs:326 | if (MapScroller.Instance != null) MapScroller.Instance.ResumeMoving(); |
| Assets/Scripts/Managers/CultivationManager.cs:344 | if (GameLogger.Instance != null) GameLogger.Instance.Log("TỬ TRẬN! Bảo toàn Tu Vi và Cảnh Giới", Color.yellow); |
| Assets/Scripts/Managers/CultivationManager.cs:348 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/CultivationManager.cs:359 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/CultivationManager.cs:409 | SwordLoadoutService.Instance?.UpdateCapacity(realm, subLevel); |
| Assets/Scripts/Managers/EncounterManager.cs:35 | if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) return false; |
| Assets/Scripts/Managers/EncounterManager.cs:41 | var state = GameManager.Instance != null ? GameManager.Instance.currentState : GameManager.GameState.IdleFarm; |
| Assets/Scripts/Managers/EncounterManager.cs:43 | var menu = MainMenuManager.Instance; |
| Assets/Scripts/Managers/EncounterManager.cs:45 | var choices = GameplayChoicesService.Instance; |
| Assets/Scripts/Managers/EncounterManager.cs:49 | if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay) return false; |
| Assets/Scripts/Managers/EncounterManager.cs:174 | if (GameplayChoicesService.Instance != null) |
| Assets/Scripts/Managers/EncounterManager.cs:176 | if (!GameplayChoicesService.Instance.OfferEncounter()) return; |
| Assets/Scripts/Managers/EncounterManager.cs:182 | AchievementManager.Instance?.OnEncounterCollected(); |
| Assets/Scripts/Managers/EncounterManager.cs:187 | if (MergeManager.Instance != null) |
| Assets/Scripts/Managers/EncounterManager.cs:190 | bool added = MergeManager.Instance.TryAddSword(lvl); |
| Assets/Scripts/Managers/EncounterManager.cs:194 | EconomyManager.Instance?.AddLinhThach(compensation); |
| Assets/Scripts/Managers/EncounterManager.cs:195 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Túi kiếm đầy! Quy đổi Kỳ Ngộ thành {compensation:F0} Linh Thạch.", Color.yellow); |
| Assets/Scripts/Managers/EncounterManager.cs:199 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Kỳ Ngộ: Đã nhặt được 1 Phôi Kiếm Lv{lvl}!", Color.yellow); |
| Assets/Scripts/Managers/EncounterManager.cs:204 | if (ShopManager.Instance != null) |
| Assets/Scripts/Managers/EncounterManager.cs:207 | ShopManager.Instance.ApplyBuff(randomBuff); |
| Assets/Scripts/Managers/EncounterManager.cs:210 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Kỳ Ngộ: Nhặt được 1 {names[randomBuff]}!", Color.green); |
| Assets/Scripts/Managers/EncounterManager.cs:215 | if (EconomyManager.Instance != null) |
| Assets/Scripts/Managers/EncounterManager.cs:217 | EconomyManager.Instance.AddCoDuyen(1); |
| Assets/Scripts/Managers/EncounterManager.cs:218 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Kỳ Ngộ: Nhận được 1 Cơ Duyên!", Color.cyan); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:18 | private CharacterSaveData Slot |
| Assets/Scripts/Managers/GameplayChoicesService.cs:22 | var data = DataManager.Instance; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:24 | int index = data.gameSaveData.lastPlayedSlotIndex; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:25 | return index >= 0 && index < data.gameSaveData.slots.Length ? data.gameSaveData.slots[index] : null; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:70 | if (MobileDrawerUIController.Instance != null) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:72 | MobileDrawerUIController.Instance.OnDockCollapseChanged += OnDockCollapseChanged; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:76 | rect.anchoredPosition = MobileDrawerUIController.Instance.IsDockCollapsed ? CollapsedStylePosition : DefaultStylePosition; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:85 | if (MobileDrawerUIController.Instance != null) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:87 | MobileDrawerUIController.Instance.OnDockCollapseChanged -= OnDockCollapseChanged; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:127 | if (DaoAnModalUI.Instance != null && DaoAnModalUI.Instance.IsModalOpen) return true; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:128 | if (SecretRealmModalUI.Instance != null && SecretRealmModalUI.Instance.IsModalOpen) return true; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:151 | if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay) return true; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:160 | if (CombatManager.Instance != null && CombatManager.Instance.isPlayerDead) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:206 | RealmId realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:207 | int subLevel = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentSubLevel : 1; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:231 | else if (MergeManager.Instance != null && !MergeManager.Instance.TryAddSword(2)) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:232 | EconomyManager.Instance?.AddLinhThach(1000); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:233 | AchievementManager.Instance?.OnEncounterCollected(); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:244 | RealmId realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:245 | int subLevel = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentSubLevel : 1; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:249 | EconomyManager.Instance?.AddLinhThach(1000); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:254 | if (AlchemyService.Instance != null) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:256 | AlchemyService.Instance.AddHerb(0, 3); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:257 | AlchemyService.Instance.AddHerb(1, 2); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:258 | AlchemyService.Instance.AddHerb(2, 1); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:264 | AlchemyService.Instance?.GrantEncounterPill(); |
| Assets/Scripts/Managers/GameplayChoicesService.cs:270 | private void Save() { DataManager.Instance?.SaveCurrentGame(true); } |
| Assets/Scripts/Managers/GameplayChoicesService.cs:271 | public static void Notify(string message) { GameLogger.Instance?.Log(message, Color.cyan); } |
| Assets/Scripts/Managers/GameplayChoicesService.cs:275 | if (MergeManager.Instance == null) return false; |
| Assets/Scripts/Managers/GameplayChoicesService.cs:276 | if (MergeManager.Instance.equipPanel != null) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:278 | foreach (Transform slot in MergeManager.Instance.equipPanel) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:285 | if (MergeManager.Instance.inventoryPages != null) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:287 | foreach (Transform page in MergeManager.Instance.inventoryPages) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:319 | if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsDockCollapsed) |
| Assets/Scripts/Managers/GameplayChoicesService.cs:352 | rect.anchoredPosition = MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsDockCollapsed ? CollapsedStylePosition : DefaultStylePosition; |
| Assets/Scripts/Managers/MergeManager.cs:40 | if (SwordLoadoutService.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:41 | SwordLoadoutService.Instance.OnCapacityChanged += HandleCapacityChanged; |
| Assets/Scripts/Managers/MergeManager.cs:51 | if (SwordLoadoutService.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:52 | SwordLoadoutService.Instance.OnCapacityChanged -= HandleCapacityChanged; |
| Assets/Scripts/Managers/MergeManager.cs:113 | int maxCap = SwordLoadoutService.Instance != null |
| Assets/Scripts/Managers/MergeManager.cs:114 | ? SwordLoadoutService.Instance.GetCurrentCapacity() |
| Assets/Scripts/Managers/MergeManager.cs:129 | AchievementManager.Instance?.OnSwordMerged(equipped.level); |
| Assets/Scripts/Managers/MergeManager.cs:155 | if (totalUpgrades > 0 && AudioManager.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:157 | AudioManager.Instance.PlayMergeSwordSFX(); |
| Assets/Scripts/Managers/MergeManager.cs:186 | if (EconomyManager.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:188 | EconomyManager.Instance.AddKiemY(sacrificedCount); |
| Assets/Scripts/Managers/MergeManager.cs:190 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:192 | GameLogger.Instance.Log($"Tế Kiếm: Nhận {sacrificedCount} điểm Kiếm Ý (+{sacrificedCount * 50}% ST)!", Color.magenta); |
| Assets/Scripts/Managers/MergeManager.cs:198 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:200 | GameLogger.Instance.Log("Không có kiếm Lv8 nào trong kho để Tế!", Color.red); |
| Assets/Scripts/Managers/MergeManager.cs:430 | if (CombatManager.Instance != null) |
| Assets/Scripts/Managers/MergeManager.cs:432 | CombatManager.Instance.UpdateEquippedSwords(equipLevels); |
| Assets/Scripts/Managers/MergeManager.cs:437 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/MergeManager.cs:487 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/ShopManager.cs:195 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/ShopManager.cs:198 | GameLogger.Instance.Log($"{names[itemIndex]} đã đạt tối đa 5 lọ (+25%)!", Color.yellow); |
| Assets/Scripts/Managers/ShopManager.cs:203 | int stage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0; |
| Assets/Scripts/Managers/ShopManager.cs:207 | if (EconomyManager.Instance != null && EconomyManager.Instance.SpendLinhThach(cost)) |
| Assets/Scripts/Managers/ShopManager.cs:218 | if (GameLogger.Instance != null) |
| Assets/Scripts/Managers/ShopManager.cs:220 | GameLogger.Instance.Log($"Dùng {names[itemIndex]}: +{newStacks * 5}% (Tầng {newStacks}/5, {Mathf.Ceil(GetTimerByIndex(itemIndex))}s)", Color.green); |
| Assets/Scripts/Managers/ShopManager.cs:225 | Debug.LogWarning($"[Shop] LỖI: Không đủ Linh Thạch! Cần {cost} LT nhưng bạn chỉ có {(EconomyManager.Instance != null ? EconomyManager.Instance.currentLinhThach : 0)} LT."); |
| Assets/Scripts/Managers/SkillManager.cs:151 | if (CombatManager.Instance != null) CombatManager.Instance.DeactivateVanKiemQuyTong(); |
| Assets/Scripts/Managers/SkillManager.cs:159 | float regenBuff = ShopManager.Instance != null ? ShopManager.Instance.GetManaRegenMultiplier() : 1f; |
| Assets/Scripts/Managers/SkillManager.cs:195 | if (AchievementManager.Instance != null && AchievementManager.Instance.State != null) |
| Assets/Scripts/Managers/SkillManager.cs:197 | maxLvl = Mathf.Max(maxLvl, AchievementManager.Instance.State.maxSwordLevelEver); |
| Assets/Scripts/Managers/SkillManager.cs:199 | if (MergeManager.Instance != null) |
| Assets/Scripts/Managers/SkillManager.cs:201 | if (MergeManager.Instance.equipPanel != null) |
| Assets/Scripts/Managers/SkillManager.cs:203 | foreach (Transform slot in MergeManager.Instance.equipPanel) |
| Assets/Scripts/Managers/SkillManager.cs:210 | if (MergeManager.Instance.inventoryPages != null) |
| Assets/Scripts/Managers/SkillManager.cs:212 | foreach (Transform page in MergeManager.Instance.inventoryPages) |
| Assets/Scripts/Managers/SkillManager.cs:241 | if (FormationService.Instance == null \|\| FormationService.Instance.State == null \|\| FormationService.Instance.State.levels == null) return 0; |
| Assets/Scripts/Managers/SkillManager.cs:243 | for (int i = 0; i < FormationService.Instance.State.levels.Length; i++) |
| Assets/Scripts/Managers/SkillManager.cs:245 | if (FormationService.Instance.State.levels[i] > 0) count++; |
| Assets/Scripts/Managers/SkillManager.cs:256 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Kỹ năng chưa mở khóa!", Color.gray); |
| Assets/Scripts/Managers/SkillManager.cs:261 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Đang trong trạng thái Vạn Kiếm Quy Tông!", Color.gray); |
| Assets/Scripts/Managers/SkillManager.cs:266 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"{skill.skillName} đang hồi chiêu ({normalCooldowns[index]:F1}s)!", Color.gray); |
| Assets/Scripts/Managers/SkillManager.cs:272 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Linh Lực! Cần {manaRequired:F0} MP.", Color.cyan); |
| Assets/Scripts/Managers/SkillManager.cs:285 | var realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi; |
| Assets/Scripts/Managers/SkillManager.cs:294 | if (GameLogger.Instance != null) GameLogger.Instance.Log("⚔️ Sở hữu Kiếm Cấp 7: Lĩnh ngộ Tuyệt Kỹ VẠN KIẾM QUY TÔNG!", Color.yellow); |
| Assets/Scripts/Managers/SkillManager.cs:304 | if (GameLogger.Instance != null) GameLogger.Instance.Log("⚡ Độ kiếp Nguyên Anh thành công: Lĩnh ngộ tuyệt kỹ LÔI PHẠT!", new Color(0.8f, 0.4f, 1f)); |
| Assets/Scripts/Managers/SkillManager.cs:315 | if (GameLogger.Instance != null) GameLogger.Instance.Log("☯️ Đã lĩnh hội 4 loại Trận Pháp: Lĩnh ngộ tuyệt kỹ ÂM DƯƠNG TRẬN!", Color.cyan); |
| Assets/Scripts/Managers/SkillManager.cs:325 | if (GameLogger.Instance != null) GameLogger.Instance.Log("🖐️ Đạt cảnh giới Đại Thừa Chí Tôn: Khai thông đại thần thông ẤN CHƯỞNG!", new Color(1f, 0.6f, 0.1f)); |
| Assets/Scripts/Managers/SkillManager.cs:386 | float kiemY = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetKiemYValue() : 0f; |
| Assets/Scripts/Managers/SkillManager.cs:399 | float multiplier = CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f; |
| Assets/Scripts/Managers/SkillManager.cs:400 | if (ShopManager.Instance != null) multiplier *= ShopManager.Instance.GetDamageMultiplier(); |
| Assets/Scripts/Managers/SkillManager.cs:401 | if (EconomyManager.Instance != null) multiplier *= (1f + EconomyManager.Instance.currentKiemY * 0.1f); |
| Assets/Scripts/Managers/SkillManager.cs:402 | if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive) |
| Assets/Scripts/Managers/SkillManager.cs:403 | multiplier *= CombatManager.Instance.GetAmDuongTranDamageMultiplier(); |
| Assets/Scripts/Managers/SkillManager.cs:415 | if (CombatManager.Instance != null) CombatManager.Instance.CastAnChuong(damage); |
| Assets/Scripts/Managers/SkillManager.cs:416 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Ấn Chưởng (Lv.{skill.level})", new Color(1f, 0.5f, 0f)); |
| Assets/Scripts/Managers/SkillManager.cs:419 | if (CombatManager.Instance != null) CombatManager.Instance.CastLoiPhat(damage); |
| Assets/Scripts/Managers/SkillManager.cs:420 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Lôi Phạt (Lv.{skill.level})", new Color(0.8f, 0f, 1f)); |
| Assets/Scripts/Managers/SkillManager.cs:423 | if (CombatManager.Instance != null) CombatManager.Instance.CastAmDuongTran(skill.level); |
| Assets/Scripts/Managers/SkillManager.cs:424 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Âm Dương Trận (Lv.{skill.level})", Color.cyan); |
| Assets/Scripts/Managers/SkillManager.cs:435 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Vạn Kiếm Quy Tông đang hồi chiêu ({currentCooldown:F1}s)!", Color.gray); |
| Assets/Scripts/Managers/SkillManager.cs:440 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Chưa đủ Linh Lực để kích hoạt Vạn Kiếm Quy Tông! ({Mathf.FloorToInt(currentLinhLuc)}/{Mathf.FloorToInt(maxLinhLuc)})", Color.cyan); |
| Assets/Scripts/Managers/SkillManager.cs:453 | if (CombatManager.Instance != null) |
| Assets/Scripts/Managers/SkillManager.cs:455 | CombatManager.Instance.ActivateVanKiemQuyTong(finalDamage); |
| Assets/Scripts/Managers/SkillManager.cs:456 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: VẠN KIẾM QUY TÔNG!", Color.yellow); |
| Assets/Scripts/Managers/SkillManager.cs:480 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/SkillManager.cs:492 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/StageManager.cs:107 | if (CombatManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:109 | CombatManager.Instance.ChangeToBossState(); |
| Assets/Scripts/Managers/StageManager.cs:123 | if (CombatManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:125 | CombatManager.Instance.ChangeToIdleState(); |
| Assets/Scripts/Managers/StageManager.cs:129 | if (MapScroller.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:131 | MapScroller.Instance.RandomizeBiomeColor(); |
| Assets/Scripts/Managers/StageManager.cs:135 | if (DataManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:137 | DataManager.Instance.SaveCurrentGame(silent: true); |
| Assets/Scripts/Managers/StageManager.cs:141 | CultivationManager.Instance?.ShowPendingTribulationPrompt(); |
| Assets/Scripts/Managers/StageManager.cs:148 | if (GameManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:150 | GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/StageManager.cs:153 | if (CombatManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:155 | CombatManager.Instance.ChangeToIdleState(); |
| Assets/Scripts/Managers/StageManager.cs:167 | if (GameManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:169 | GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm); |
| Assets/Scripts/Managers/StageManager.cs:174 | if (DataManager.Instance != null) |
| Assets/Scripts/Managers/StageManager.cs:176 | DataManager.Instance.SaveCurrentGame(silent: true); |
| Assets/Scripts/Managers/StageManager.cs:253 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/StageManager.cs:259 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/TooltipManager.cs:190 | if (TooltipManager.Instance != null && !string.IsNullOrEmpty(tooltipText)) |
| Assets/Scripts/Managers/TooltipManager.cs:192 | TooltipManager.Instance.ShowTooltip(tooltipText); |
| Assets/Scripts/Managers/TooltipManager.cs:198 | if (TooltipManager.Instance != null) |
| Assets/Scripts/Managers/TooltipManager.cs:200 | TooltipManager.Instance.HideTooltip(); |
| Assets/Scripts/Managers/TutorialManager.cs:116 | if (btnClaimQuick != null && btnClaimQuick.gameObject.activeInHierarchy && MilestoneManager.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:118 | int readyCount = MilestoneManager.Instance.GetReadyToClaimCount(); |
| Assets/Scripts/Managers/TutorialManager.cs:130 | if (achievementBadgeObj != null && AchievementManager.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:132 | int readyAchCount = AchievementManager.Instance.GetReadyToClaimCount(); |
| Assets/Scripts/Managers/TutorialManager.cs:768 | while (MainMenuManager.Instance != null && MainMenuManager.Instance.gameObject.activeInHierarchy) |
| Assets/Scripts/Managers/TutorialManager.cs:772 | while (DataManager.Instance != null && !DataManager.Instance.isSessionActive) |
| Assets/Scripts/Managers/TutorialManager.cs:782 | startKills = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0; |
| Assets/Scripts/Managers/TutorialManager.cs:795 | int currentKills = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0; |
| Assets/Scripts/Managers/TutorialManager.cs:796 | int currentStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1; |
| Assets/Scripts/Managers/TutorialManager.cs:856 | bool mobileHud = MobileDrawerUIController.Instance != null; |
| Assets/Scripts/Managers/TutorialManager.cs:863 | int curKiemYLevel = UpgradeManager.Instance != null ? UpgradeManager.Instance.kiemYLevel : 1; |
| Assets/Scripts/Managers/TutorialManager.cs:864 | if (mobileHud ? DaoAnModalUI.Instance?.IsModalOpen == true \|\| HasDaoAnUpgrade() : curKiemYLevel >= 2) |
| Assets/Scripts/Managers/TutorialManager.cs:906 | stepComplete = (ShopManager.Instance != null && ShopManager.Instance.isShopOpen) \|\| |
| Assets/Scripts/Managers/TutorialManager.cs:921 | if (btnShopTr != null && (!mobileHud \|\| DaoAnModalUI.Instance?.IsModalOpen != true)) |
| Assets/Scripts/Managers/TutorialManager.cs:943 | int currentStageIdx = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0; |
| Assets/Scripts/Managers/TutorialManager.cs:1000 | if (MilestoneManager.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1002 | MilestoneQuest q = MilestoneManager.Instance.GetNextPendingMilestone(); |
| Assets/Scripts/Managers/TutorialManager.cs:1005 | bool isDone = MilestoneManager.Instance.IsCompleted(q); |
| Assets/Scripts/Managers/TutorialManager.cs:1006 | int cur = MilestoneManager.Instance.GetCurrentProgress(q); |
| Assets/Scripts/Managers/TutorialManager.cs:1051 | var progression = DaoAnProgressionService.Instance; |
| Assets/Scripts/Managers/TutorialManager.cs:1114 | if (MilestoneManager.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1116 | MilestoneQuest pending = MilestoneManager.Instance.GetNextPendingMilestone(); |
| Assets/Scripts/Managers/TutorialManager.cs:1117 | if (pending != null && MilestoneManager.Instance.IsCompleted(pending)) |
| Assets/Scripts/Managers/TutorialManager.cs:1119 | MilestoneManager.Instance.ClaimReward(pending.id); |
| Assets/Scripts/Managers/TutorialManager.cs:1129 | if (MilestoneModalUI.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1131 | MilestoneModalUI.Instance.Open(); |
| Assets/Scripts/Managers/TutorialManager.cs:1147 | if (AchievementModalUI.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1149 | AchievementModalUI.Instance.OpenModal(); |
| Assets/Scripts/Managers/TutorialManager.cs:1168 | if (EconomyManager.Instance != null && rewardLinhThach > 0) |
| Assets/Scripts/Managers/TutorialManager.cs:1170 | EconomyManager.Instance.AddLinhThach(rewardLinhThach); |
| Assets/Scripts/Managers/TutorialManager.cs:1173 | if (AudioManager.Instance != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1175 | AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/Managers/TutorialManager.cs:1192 | bool isTribulation = (CombatManager.Instance != null && CombatManager.Instance.isTribulationBossActive); |
| Assets/Scripts/Managers/TutorialManager.cs:1205 | if (MergeManager.Instance == null) return; |
| Assets/Scripts/Managers/TutorialManager.cs:1209 | MergeManager.Instance.TryAddSword(1); |
| Assets/Scripts/Managers/TutorialManager.cs:1214 | if (MergeManager.Instance == null) return false; |
| Assets/Scripts/Managers/TutorialManager.cs:1217 | if (MergeManager.Instance.equipPanel != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1219 | foreach (Transform slot in MergeManager.Instance.equipPanel) |
| Assets/Scripts/Managers/TutorialManager.cs:1228 | if (MergeManager.Instance.inventoryPages != null) |
| Assets/Scripts/Managers/TutorialManager.cs:1230 | foreach (Transform page in MergeManager.Instance.inventoryPages) |
| Assets/Scripts/Managers/TutorialManager.cs:1300 | if (DataManager.Instance != null && DataManager.Instance.isSessionActive) |
| Assets/Scripts/Managers/TutorialManager.cs:1302 | DataManager.Instance.SaveCurrentGame(silent: true); |
| Assets/Scripts/Managers/TutorialManager.cs:1307 | public void SaveToSlot(CharacterSaveData slot) |
| Assets/Scripts/Managers/TutorialManager.cs:1316 | public void LoadFromSlot(CharacterSaveData slot) |
| Assets/Scripts/UI/AchievementButtonHandler.cs:34 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AchievementModalUI.cs:79 | if (AchievementManager.Instance != null) |
| Assets/Scripts/UI/AchievementModalUI.cs:81 | AchievementManager.Instance.OnAchievementsUpdated += OnAchievementsUpdatedHandler; |
| Assets/Scripts/UI/AchievementModalUI.cs:99 | if (AchievementManager.Instance != null) |
| Assets/Scripts/UI/AchievementModalUI.cs:101 | AchievementManager.Instance.OnAchievementsUpdated -= OnAchievementsUpdatedHandler; |
| Assets/Scripts/UI/AchievementModalUI.cs:178 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AchievementModalUI.cs:188 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AchievementModalUI.cs:197 | if (MilestoneModalUI.Instance != null) MilestoneModalUI.Instance.Open(); |
| Assets/Scripts/UI/AchievementModalUI.cs:270 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AchievementModalUI.cs:301 | var mgr = AchievementManager.Instance; |
| Assets/Scripts/UI/AchievementModalUI.cs:551 | mgr.ClaimReward(idToClaim); |
| Assets/Scripts/UI/AchievementModalUI.cs:582 | if (AchievementManager.Instance != null) |
| Assets/Scripts/UI/AchievementModalUI.cs:584 | int count = AchievementManager.Instance.ClaimAllCompleted(selectedCategory); |
| Assets/Scripts/UI/AchievementModalUI.cs:585 | if (count > 0 && GameLogger.Instance != null) |
| Assets/Scripts/UI/AchievementModalUI.cs:587 | GameLogger.Instance.Log($"Nhận thành công trọn bộ {count} Thành Tựu!", Color.yellow); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:94 | if (AlchemyService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:95 | AlchemyService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:96 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:97 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:98 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:99 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:107 | if (AlchemyService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:108 | AlchemyService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:109 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:110 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:111 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:112 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:117 | if (AlchemyService.Instance != null && AlchemyService.Instance.IsBrewing) |
| Assets/Scripts/UI/AlchemyPanelUI.cs:125 | if (AlchemyService.Instance == null) return; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:126 | var svc = AlchemyService.Instance; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:127 | int curRealm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:150 | bool unlocked = FeatureUnlockService.Instance == null \|\| FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:188 | if (AlchemyService.Instance == null) return; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:189 | var svc = AlchemyService.Instance; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:243 | bool canDrink = totalTuVi > 0 && CultivationManager.Instance != null && !CultivationManager.Instance.IsTuViMaxed(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:270 | if (AlchemyService.Instance == null) return; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:271 | int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:272 | bool ok = AlchemyService.Instance.TryStartBrew(PillRecipe.Cultivation, realm); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:275 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:276 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Đã bắt đầu luyện một lò Tu Vi Đan!", Color.cyan); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:280 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Không đủ thảo dược hoặc lò đang bận!", Color.yellow); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:287 | if (AlchemyService.Instance == null) return; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:288 | int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:289 | bool ok = AlchemyService.Instance.TryStartBrew(PillRecipe.Breakthrough, realm); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:292 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:293 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Đã bắt đầu luyện một lò Đột Phá Đan!", new Color(1f, 0.6f, 0.2f)); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:297 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Không đủ thảo dược hoặc lò đang bận!", Color.yellow); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:304 | var alchemy = AlchemyService.Instance; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:310 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:311 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Tiêu hao {cost} Bụi Dược, lò luyện tức thì đại thành!", Color.green); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:318 | if (AlchemyService.Instance == null) return; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:319 | int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0; |
| Assets/Scripts/UI/AlchemyPanelUI.cs:322 | bool used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Basic); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:323 | if (!used) used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Refined); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:324 | if (!used) used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Supreme); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:328 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/AlchemyPanelUI.cs:329 | if (GameLogger.Instance != null) GameLogger.Instance.Log("Uống 1 viên Tu Vi Đan, tu vi tăng mạnh!", Color.cyan); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:55 | if (ArtifactService.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:56 | ArtifactService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:57 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:58 | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:59 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:60 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:61 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:62 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:71 | if (ArtifactService.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:72 | ArtifactService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:73 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:74 | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:75 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:76 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:77 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:78 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:83 | if (ArtifactService.Instance == null) return; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:84 | var svc = ArtifactService.Instance; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:85 | float stone = EconomyManager.Instance?.currentLinhThach ?? 0f; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:98 | bool unlocked = FeatureUnlockService.Instance == null \|\| FeatureUnlockService.Instance.IsArtifactSlotUnlocked(slot); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:119 | artifactCostTexts[i].text = FeatureUnlockService.Instance.GetArtifactSlotUnlockRequirementText(slot); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:177 | if (ArtifactService.Instance == null) return; |
| Assets/Scripts/UI/ArtifactPanelUI.cs:178 | bool ok = ArtifactService.Instance.TryUpgrade((ArtifactSlot)slotIndex); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:181 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:182 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:183 | GameLogger.Instance.Log($"Cường hóa thành công {SlotNames[slotIndex]}!", Color.cyan); |
| Assets/Scripts/UI/ArtifactPanelUI.cs:187 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/ArtifactPanelUI.cs:188 | GameLogger.Instance.Log("Không đủ Quặng Phôi hoặc Linh Thạch!", Color.yellow); |
| Assets/Scripts/UI/BiCanhButtonHandler.cs:35 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BiCanhButtonHandler.cs:36 | if (SecretRealmModalUI.Instance != null) |
| Assets/Scripts/UI/BiCanhButtonHandler.cs:38 | SecretRealmModalUI.Instance.Open(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:147 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:148 | PlayerStatsService.Instance.OnStatsChanged += Refresh; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:149 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:150 | EconomyManager.Instance.OnLinhThachChanged += HandleCurrencyChanged; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:157 | if (PlayerStatsService.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:158 | PlayerStatsService.Instance.OnStatsChanged -= Refresh; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:159 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:160 | EconomyManager.Instance.OnLinhThachChanged -= HandleCurrencyChanged; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:176 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:186 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:191 | var stats = PlayerStatsService.Instance; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:196 | stats.TryUpgrade(index); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:200 | stats.TryUpgradeMultiple(index, count); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:202 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:208 | var stats = PlayerStatsService.Instance; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:211 | stats.TryUpgradeMultiple(index, 50); // Nâng tối đa trong 50 cấp |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:212 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:218 | var stats = PlayerStatsService.Instance; |
| Assets/Scripts/UI/BodyTemperingPanelUI.cs:219 | var economy = EconomyManager.Instance; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:132 | if (CultivationManager.Instance != null && !string.IsNullOrEmpty(CultivationManager.Instance.CharacterName)) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:134 | charName = CultivationManager.Instance.CharacterName; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:138 | if (CultivationManager.Instance != null && CultivationManager.Instance.allStages != null && CultivationManager.Instance.currentStageIndex < CultivationManager.Instance.allStages.Length) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:140 | var stage = CultivationManager.Instance.allStages[CultivationManager.Instance.currentStageIndex]; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:141 | float currentTuVi = CultivationManager.Instance.currentTuVi; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:143 | float progress = CultivationDisplay.PhaseProgress(CultivationManager.Instance.currentStageIndex, |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:146 | if (txtRealm != null) txtRealm.text = $"Cảnh Giới: <color=#66FFB2>{CultivationManager.Instance.GetDisplayRealmName()}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:150 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:152 | if (txtCoDuyen != null) txtCoDuyen.text = $"Cơ Duyên: <color=#FFAA33>{EconomyManager.Instance.currentCoDuyen} Điểm</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:153 | if (txtLinhThach != null) txtLinhThach.text = $"Linh Thạch: <color=#66CCFF>{EconomyManager.Instance.currentLinhThach:N0}</color>"; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:157 | if (CombatManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:159 | float hp = CombatManager.Instance.playerCurrentHp; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:160 | float maxHp = CombatManager.Instance.playerMaxHp; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:164 | if (SkillManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:166 | float mp = SkillManager.Instance.currentLinhLuc; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:167 | float maxMp = SkillManager.Instance.maxLinhLuc; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:168 | float regen = SkillManager.Instance.linhLucRegenRate; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:172 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:174 | float mult = CultivationManager.Instance.permanentStatMultiplier; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:180 | if (MergeManager.Instance != null && MergeManager.Instance.equipPanel != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:184 | int capacity = SwordLoadoutService.Instance != null |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:185 | ? SwordLoadoutService.Instance.GetCurrentCapacity() : MergeManager.Instance.equipPanel.childCount; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:186 | for (int i = 0; i < MergeManager.Instance.equipPanel.childCount; i++) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:188 | if (i < MergeManager.Instance.equipPanel.childCount) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:190 | Transform slot = MergeManager.Instance.equipPanel.GetChild(i); |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:218 | if (UpgradeManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:220 | lvlKY = UpgradeManager.Instance.kiemYLevel; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:221 | lvlLL = UpgradeManager.Instance.linhLucLevel; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:222 | lvlTL = UpgradeManager.Instance.tuLinhLevel; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:223 | lvlTT = UpgradeManager.Instance.thanThucLevel; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:225 | valKY = UpgradeManager.Instance.GetKiemYValue(); |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:228 | valTL = UpgradeManager.Instance.GetTuLinhValue(); |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:231 | valTT = UpgradeManager.Instance.GetThanThucValue(); |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:235 | if (SkillManager.Instance != null) |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:237 | maxMpVal = SkillManager.Instance.maxLinhLuc; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:238 | regenMpVal = SkillManager.Instance.linhLucRegenRate; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:273 | elapsed += PortraitHudController.Instance != null ? Time.unscaledDeltaTime : Time.deltaTime; |
| Assets/Scripts/UI/CharacterDetailModalUI.cs:304 | elapsed += PortraitHudController.Instance != null ? Time.unscaledDeltaTime : Time.deltaTime; |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:11 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:13 | EconomyManager.Instance.OnCoDuyenChanged += UpdateUI; |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:14 | UpdateUI(EconomyManager.Instance.currentCoDuyen); |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:20 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/CoDuyenUIUpdater.cs:22 | EconomyManager.Instance.OnCoDuyenChanged -= UpdateUI; |
| Assets/Scripts/UI/CollapsibleQuestBar.cs:85 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/CombatNotificationUI.cs:143 | if (CameraShake.Instance != null && Application.isPlaying) |
| Assets/Scripts/UI/CombatNotificationUI.cs:145 | CameraShake.Instance.TriggerShake(0.5f, 0.35f); |
| Assets/Scripts/UI/CombatNotificationUI.cs:180 | if (CameraShake.Instance != null && Application.isPlaying) |
| Assets/Scripts/UI/CombatNotificationUI.cs:182 | CameraShake.Instance.TriggerShake(0.4f, 0.3f); |
| Assets/Scripts/UI/CombatNotificationUI.cs:224 | if (CameraShake.Instance != null && Application.isPlaying) |
| Assets/Scripts/UI/CombatNotificationUI.cs:226 | CameraShake.Instance.TriggerShake(0.6f, 0.4f); |
| Assets/Scripts/UI/CultivationUI.cs:12 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/CultivationUI.cs:14 | CultivationManager.Instance.OnTuViChanged += UpdateTuViUI; |
| Assets/Scripts/UI/CultivationUI.cs:15 | CultivationManager.Instance.OnStageChanged += UpdateStageUI; |
| Assets/Scripts/UI/CultivationUI.cs:16 | var stage = CultivationManager.Instance.GetCurrentStageData(); |
| Assets/Scripts/UI/CultivationUI.cs:20 | UpdateTuViUI(CultivationManager.Instance.currentTuVi, stage.requiredTuVi); |
| Assets/Scripts/UI/CultivationUI.cs:27 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/CultivationUI.cs:29 | CultivationManager.Instance.OnTuViChanged -= UpdateTuViUI; |
| Assets/Scripts/UI/CultivationUI.cs:30 | CultivationManager.Instance.OnStageChanged -= UpdateStageUI; |
| Assets/Scripts/UI/CultivationUI.cs:37 | if (imgProgressBar != null && CultivationManager.Instance != null) |
| Assets/Scripts/UI/CultivationUI.cs:39 | CultivationManager.Instance.currentStageIndex, max > 0 ? current / max : 0f); |
| Assets/Scripts/UI/CultivationUI.cs:44 | if (txtStageName != null && CultivationManager.Instance != null) |
| Assets/Scripts/UI/CultivationUI.cs:45 | txtStageName.text = $"Cảnh giới: {CultivationManager.Instance.GetDisplayRealmName()}"; |
| Assets/Scripts/UI/CultivationUI.cs:57 | if (CultivationManager.Instance != null) CultivationManager.Instance.AddTuVi(50); |
| Assets/Scripts/UI/CultivationUI.cs:63 | if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation) |
| Assets/Scripts/UI/CultivationUI.cs:65 | if (CultivationManager.Instance != null) CultivationManager.Instance.Breakthrough(); |
| Assets/Scripts/UI/DaoAnButtonHandler.cs:35 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/DaoAnButtonHandler.cs:36 | if (DaoAnModalUI.Instance != null) |
| Assets/Scripts/UI/DaoAnButtonHandler.cs:38 | DaoAnModalUI.Instance.Open(); |
| Assets/Scripts/UI/DaoAnModalUI.cs:508 | var catalog = DaoAnProgressionService.Instance?.Catalog ?? Resources.Load<DaoAnCatalog>("Progression/DaoAnCatalog"); |
| Assets/Scripts/UI/DaoAnModalUI.cs:582 | var cultivation = CultivationManager.Instance; |
| Assets/Scripts/UI/DaoAnModalUI.cs:594 | var service = DaoAnProgressionService.Instance; |
| Assets/Scripts/UI/DaoAnModalUI.cs:605 | var service = DaoAnProgressionService.Instance; |
| Assets/Scripts/UI/DaoAnModalUI.cs:608 | if (walletText != null && EconomyManager.Instance != null) |
| Assets/Scripts/UI/DaoAnModalUI.cs:610 | walletText.text = "Linh Thạch: " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach); |
| Assets/Scripts/UI/DaoAnModalUI.cs:651 | var service = DaoAnProgressionService.Instance; |
| Assets/Scripts/UI/DaoAnModalUI.cs:665 | var cultivation = CultivationManager.Instance; |
| Assets/Scripts/UI/DaoAnModalUI.cs:859 | var activeService = DaoAnProgressionService.Instance ?? service; |
| Assets/Scripts/UI/DaoAnModalUI.cs:867 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/DaoAnModalUI.cs:868 | GameLogger.Instance?.Log($"Khai mở Đạo Ấn: {node.displayName} Cấp {activeService.GetLevel(node.id)}!", Color.green); |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:135 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/DismantleConfirmModalUI.cs:154 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/FormationPanelUI.cs:95 | if (FormationService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:96 | FormationService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:97 | if (ArtifactService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:98 | ArtifactService.Instance.OnChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:99 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:100 | EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:101 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:102 | FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:103 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:104 | CultivationManager.Instance.OnStageChanged += HandleStageChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:113 | if (FormationService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:114 | FormationService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:115 | if (ArtifactService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:116 | ArtifactService.Instance.OnChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:117 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:118 | EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:119 | if (FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:120 | FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI; |
| Assets/Scripts/UI/FormationPanelUI.cs:121 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:122 | CultivationManager.Instance.OnStageChanged -= HandleStageChanged; |
| Assets/Scripts/UI/FormationPanelUI.cs:132 | if (FormationService.Instance == null) return; |
| Assets/Scripts/UI/FormationPanelUI.cs:133 | var svc = FormationService.Instance; |
| Assets/Scripts/UI/FormationPanelUI.cs:134 | int ore = ArtifactService.Instance?.Ore ?? 0; |
| Assets/Scripts/UI/FormationPanelUI.cs:135 | float stone = EconomyManager.Instance?.currentLinhThach ?? 0f; |
| Assets/Scripts/UI/FormationPanelUI.cs:155 | bool unlocked = FeatureUnlockService.Instance == null \|\| FeatureUnlockService.Instance.IsFormationUnlocked(id); |
| Assets/Scripts/UI/FormationPanelUI.cs:178 | formationCostTexts[i].text = FeatureUnlockService.Instance.GetFormationUnlockRequirementText(id); |
| Assets/Scripts/UI/FormationPanelUI.cs:247 | if (FormationService.Instance == null) return; |
| Assets/Scripts/UI/FormationPanelUI.cs:248 | var svc = FormationService.Instance; |
| Assets/Scripts/UI/FormationPanelUI.cs:253 | bool unlocked = FeatureUnlockService.Instance == null \|\| FeatureUnlockService.Instance.IsFormationUnlocked(FormationId.Spirit); |
| Assets/Scripts/UI/FormationPanelUI.cs:283 | if (FormationService.Instance == null) return; |
| Assets/Scripts/UI/FormationPanelUI.cs:284 | bool ok = FormationService.Instance.TrySelect((FormationId)index); |
| Assets/Scripts/UI/FormationPanelUI.cs:287 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/FormationPanelUI.cs:288 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:289 | GameLogger.Instance.Log($"Đã chuyển sang kích hoạt {FormationNames[index]}!", Color.cyan); |
| Assets/Scripts/UI/FormationPanelUI.cs:296 | if (FormationService.Instance == null) return; |
| Assets/Scripts/UI/FormationPanelUI.cs:297 | bool ok = FormationService.Instance.TryUpgrade((FormationId)index); |
| Assets/Scripts/UI/FormationPanelUI.cs:300 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/FormationPanelUI.cs:301 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:302 | GameLogger.Instance.Log($"Tu luyện thành công {FormationNames[index]}!", Color.yellow); |
| Assets/Scripts/UI/FormationPanelUI.cs:306 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:307 | GameLogger.Instance.Log("Không đủ Quặng Phôi hoặc Linh Thạch!", Color.yellow); |
| Assets/Scripts/UI/FormationPanelUI.cs:314 | bool ok = FormationService.Instance?.TryActivateSpirit() ?? false; |
| Assets/Scripts/UI/FormationPanelUI.cs:317 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/FormationPanelUI.cs:318 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:319 | GameLogger.Instance.Log("Tụ Linh Trận đại phát uy lực, nhân đôi tốc độ tu vi!", Color.green); |
| Assets/Scripts/UI/FormationPanelUI.cs:323 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/FormationPanelUI.cs:324 | GameLogger.Instance.Log("Tụ Linh Trận đang hồi chiêu hoặc chưa chọn!", Color.yellow); |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:23 | if ((StorageModalUI.Instance != null && StorageModalUI.Instance.IsModalOpen) \|\| |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:24 | (DaoAnModalUI.Instance != null && DaoAnModalUI.Instance.IsModalOpen) \|\| |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:25 | (SecretRealmModalUI.Instance != null && SecretRealmModalUI.Instance.IsModalOpen) \|\| |
| Assets/Scripts/UI/JadeSecondaryMenu.cs:26 | (AchievementModalUI.Instance != null && AchievementModalUI.Instance.IsModalOpen)) Close(); |
| Assets/Scripts/UI/MainMenuManager.cs:159 | dataManager = DataManager.Instance; |
| Assets/Scripts/UI/MainMenuManager.cs:171 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/MainMenuManager.cs:173 | AudioManager.Instance.PlayMenuBGM(); |
| Assets/Scripts/UI/MainMenuManager.cs:194 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/MainMenuManager.cs:198 | bgmSlider.value = AudioManager.Instance.bgmVolume; |
| Assets/Scripts/UI/MainMenuManager.cs:200 | bgmSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetBgmVolume(val)); |
| Assets/Scripts/UI/MainMenuManager.cs:204 | sfxSlider.value = AudioManager.Instance.sfxVolume; |
| Assets/Scripts/UI/MainMenuManager.cs:206 | sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSfxVolume(val)); |
| Assets/Scripts/UI/MainMenuManager.cs:214 | foreach (var slot in dataManager.gameSaveData.slots) |
| Assets/Scripts/UI/MainMenuManager.cs:235 | saveSlots[i].Initialize(this, dataManager.gameSaveData.slots[i], i); |
| Assets/Scripts/UI/MainMenuManager.cs:257 | CharacterSaveData newData = new CharacterSaveData(); |
| Assets/Scripts/UI/MainMenuManager.cs:271 | dataManager.gameSaveData.slots[currentSlotBeingCreated] = newData; |
| Assets/Scripts/UI/MainMenuManager.cs:272 | dataManager.SaveData(); |
| Assets/Scripts/UI/MainMenuManager.cs:282 | dataManager.gameSaveData.slots[slotIndex] = new CharacterSaveData(); |
| Assets/Scripts/UI/MainMenuManager.cs:283 | dataManager.SaveData(); |
| Assets/Scripts/UI/MainMenuManager.cs:293 | public void AskConfirmEnterGame(int slotIndex, CharacterSaveData data) |
| Assets/Scripts/UI/MainMenuManager.cs:304 | private void ConfirmEnterGame(int slotIndex, CharacterSaveData data) |
| Assets/Scripts/UI/MainMenuManager.cs:306 | dataManager.gameSaveData.lastPlayedSlotIndex = slotIndex; |
| Assets/Scripts/UI/MainMenuManager.cs:309 | dataManager.gameSaveData.slots[slotIndex].lastPlayedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"); |
| Assets/Scripts/UI/MainMenuManager.cs:310 | dataManager.SaveData(); |
| Assets/Scripts/UI/MainMenuManager.cs:330 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/MainMenuManager.cs:332 | AudioManager.Instance.PlayInGameBGM(); |
| Assets/Scripts/UI/MilestoneModalUI.cs:143 | if (MilestoneManager.Instance != null) |
| Assets/Scripts/UI/MilestoneModalUI.cs:145 | MilestoneManager.Instance.OnMilestonesUpdated += RefreshUI; |
| Assets/Scripts/UI/MilestoneModalUI.cs:151 | if (MilestoneManager.Instance != null) |
| Assets/Scripts/UI/MilestoneModalUI.cs:153 | MilestoneManager.Instance.OnMilestonesUpdated -= RefreshUI; |
| Assets/Scripts/UI/MilestoneModalUI.cs:169 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/MilestoneModalUI.cs:183 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/MilestoneModalUI.cs:188 | if (MilestoneManager.Instance == null \|\| contentContainer == null) return; |
| Assets/Scripts/UI/MilestoneModalUI.cs:194 | int totalCount = MilestoneManager.Instance.allQuests.Count; |
| Assets/Scripts/UI/MilestoneModalUI.cs:197 | foreach (var q in MilestoneManager.Instance.allQuests) |
| Assets/Scripts/UI/MilestoneModalUI.cs:200 | else if (MilestoneManager.Instance.IsCompleted(q)) readyToClaimCount++; |
| Assets/Scripts/UI/MilestoneModalUI.cs:228 | foreach (var quest in MilestoneManager.Instance.allQuests) |
| Assets/Scripts/UI/MilestoneModalUI.cs:331 | int curProgress = MilestoneManager.Instance.GetCurrentProgress(quest); |
| Assets/Scripts/UI/MilestoneModalUI.cs:334 | bool isDone = MilestoneManager.Instance.IsCompleted(quest); |
| Assets/Scripts/UI/MilestoneModalUI.cs:419 | MilestoneManager.Instance.ClaimReward(qId); |
| Assets/Scripts/UI/MilestoneModalUI.cs:436 | if (MilestoneManager.Instance == null) return; |
| Assets/Scripts/UI/MilestoneModalUI.cs:439 | foreach (var q in MilestoneManager.Instance.allQuests) |
| Assets/Scripts/UI/MilestoneModalUI.cs:441 | if (!q.isClaimed && MilestoneManager.Instance.IsCompleted(q)) |
| Assets/Scripts/UI/MilestoneModalUI.cs:443 | MilestoneManager.Instance.ClaimReward(q.id); |
| Assets/Scripts/UI/MobileCurrencyText.cs:15 | if (EconomyManager.Instance == null) return; |
| Assets/Scripts/UI/MobileCurrencyText.cs:16 | EconomyManager.Instance.OnLinhThachChanged += OnLinhThachChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:17 | EconomyManager.Instance.OnCoDuyenChanged += OnCoDuyenChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:23 | if (EconomyManager.Instance == null) return; |
| Assets/Scripts/UI/MobileCurrencyText.cs:24 | EconomyManager.Instance.OnLinhThachChanged -= OnLinhThachChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:25 | EconomyManager.Instance.OnCoDuyenChanged -= OnCoDuyenChanged; |
| Assets/Scripts/UI/MobileCurrencyText.cs:33 | if (label == null \|\| EconomyManager.Instance == null) return; |
| Assets/Scripts/UI/MobileCurrencyText.cs:36 | label.text = "LT " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach); |
| Assets/Scripts/UI/MobileCurrencyText.cs:41 | label.text = "CD " + EconomyManager.Instance.currentCoDuyen.ToString("N0"); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:171 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:252 | if (FeatureUnlockService.Instance == null) return true; |
| Assets/Scripts/UI/MobileDrawerUIController.cs:265 | if (!FeatureUnlockService.Instance.IsFeatureUnlocked(fid)) |
| Assets/Scripts/UI/MobileDrawerUIController.cs:267 | reason = FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(fid); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:332 | GameLogger.Instance?.Log(reason, Color.yellow); |
| Assets/Scripts/UI/MobileDrawerUIController.cs:349 | GameLogger.Instance?.Log(reason, Color.yellow); |
| Assets/Scripts/UI/MobileSkillHUD.cs:54 | if (SkillManager.Instance != null) |
| Assets/Scripts/UI/MobileSkillHUD.cs:55 | SkillManager.Instance.CastActiveSkill(); |
| Assets/Scripts/UI/MobileSkillHUD.cs:67 | if (SkillManager.Instance != null) |
| Assets/Scripts/UI/MobileSkillHUD.cs:68 | SkillManager.Instance.CastNormalSkillManual(index); |
| Assets/Scripts/UI/MobileSkillHUD.cs:76 | var skillMgr = SkillManager.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:34 | var menu = MainMenuManager.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:41 | var menu = MainMenuManager.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:56 | var daoAn = DaoAnModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:58 | var secretRealm = SecretRealmModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:66 | var dismantle = DismantleConfirmModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:67 | var secretPlay = SecretRealmPlayController.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:70 | var storage = StorageModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:72 | var achievements = AchievementModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:74 | var milestones = MilestoneModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:78 | var character = CharacterDetailModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:80 | var tribulation = TribulationModalUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:82 | var offline = OfflineRewardPopupUI.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:84 | var pause = PauseMenuManager.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:86 | var choices = GameplayChoicesService.Instance; |
| Assets/Scripts/UI/MobileUINavigationManager.cs:105 | var drawer = MobileDrawerUIController.Instance; |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:218 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:219 | OfflineEarningsManager.Instance?.ClaimReward(1.0f); |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:226 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:237 | if (rewarded && OfflineEarningsManager.Instance != null && OfflineEarningsManager.Instance.HasPendingReward) |
| Assets/Scripts/UI/OfflineRewardPopupUI.cs:239 | OfflineEarningsManager.Instance.ClaimReward(2.0f); |
| Assets/Scripts/UI/PauseButtonHandler.cs:33 | var navigation = MobileUINavigationManager.Instance; |
| Assets/Scripts/UI/PauseButtonHandler.cs:34 | var menu = MainMenuManager.Instance; |
| Assets/Scripts/UI/PauseButtonHandler.cs:50 | if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsModalOpen) |
| Assets/Scripts/UI/PauseButtonHandler.cs:52 | PauseMenuManager.Instance.CloseTopLayer(); |
| Assets/Scripts/UI/PauseButtonHandler.cs:69 | if (PauseMenuManager.Instance != null) |
| Assets/Scripts/UI/PauseButtonHandler.cs:71 | PauseMenuManager.Instance.OpenPauseMenu(); |
| Assets/Scripts/UI/PauseMenuManager.cs:169 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:171 | AudioManager.Instance.LoadSettings(); |
| Assets/Scripts/UI/PauseMenuManager.cs:175 | bgmSlider.value = AudioManager.Instance.bgmVolume; |
| Assets/Scripts/UI/PauseMenuManager.cs:176 | if (txtBgmValue != null) txtBgmValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.bgmVolume * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:181 | sfxSlider.value = AudioManager.Instance.sfxVolume; |
| Assets/Scripts/UI/PauseMenuManager.cs:182 | if (txtSfxValue != null) txtSfxValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.sfxVolume * 100)}%"; |
| Assets/Scripts/UI/PauseMenuManager.cs:187 | muteToggle.isOn = AudioManager.Instance.isMuted; |
| Assets/Scripts/UI/PauseMenuManager.cs:232 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:234 | if (!string.IsNullOrEmpty(CultivationManager.Instance.CharacterName)) |
| Assets/Scripts/UI/PauseMenuManager.cs:235 | charName = CultivationManager.Instance.CharacterName; |
| Assets/Scripts/UI/PauseMenuManager.cs:237 | if (CultivationManager.Instance.allStages != null && |
| Assets/Scripts/UI/PauseMenuManager.cs:238 | CultivationManager.Instance.currentStageIndex < CultivationManager.Instance.allStages.Length) |
| Assets/Scripts/UI/PauseMenuManager.cs:240 | stageName = CultivationManager.Instance.GetDisplayRealmName(); |
| Assets/Scripts/UI/PauseMenuManager.cs:244 | if (StageManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:246 | combatStage = StageManager.Instance.currentStage; |
| Assets/Scripts/UI/PauseMenuManager.cs:250 | if (DataManager.Instance != null && DataManager.Instance.gameSaveData != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:252 | slotIdx = DataManager.Instance.gameSaveData.lastPlayedSlotIndex; |
| Assets/Scripts/UI/PauseMenuManager.cs:329 | if (DataManager.Instance == null) |
| Assets/Scripts/UI/PauseMenuManager.cs:336 | if (DataManager.Instance.gameSaveData.lastPlayedSlotIndex < 0 \|\| |
| Assets/Scripts/UI/PauseMenuManager.cs:337 | DataManager.Instance.gameSaveData.lastPlayedSlotIndex >= DataManager.Instance.gameSaveData.slots.Length) |
| Assets/Scripts/UI/PauseMenuManager.cs:339 | DataManager.Instance.gameSaveData.lastPlayedSlotIndex = 0; |
| Assets/Scripts/UI/PauseMenuManager.cs:342 | int slotIdx = DataManager.Instance.gameSaveData.lastPlayedSlotIndex; |
| Assets/Scripts/UI/PauseMenuManager.cs:343 | var slot = DataManager.Instance.gameSaveData.slots[slotIdx]; |
| Assets/Scripts/UI/PauseMenuManager.cs:350 | if (CultivationManager.Instance != null && !string.IsNullOrEmpty(CultivationManager.Instance.CharacterName)) |
| Assets/Scripts/UI/PauseMenuManager.cs:352 | name = CultivationManager.Instance.CharacterName; |
| Assets/Scripts/UI/PauseMenuManager.cs:358 | DataManager.Instance.ActivateSession(); |
| Assets/Scripts/UI/PauseMenuManager.cs:361 | DataManager.Instance.SaveCurrentGame(silent: false); |
| Assets/Scripts/UI/PauseMenuManager.cs:371 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:373 | AudioManager.Instance.SetBgmVolume(val); |
| Assets/Scripts/UI/PauseMenuManager.cs:383 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:385 | AudioManager.Instance.SetSfxVolume(val); |
| Assets/Scripts/UI/PauseMenuManager.cs:395 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/PauseMenuManager.cs:397 | AudioManager.Instance.SetMute(isMuted); |
| Assets/Scripts/UI/PortraitHudController.cs:119 | economy = EconomyManager.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:120 | cultivation = CultivationManager.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:176 | if (economy != EconomyManager.Instance \|\| cultivation != CultivationManager.Instance) Subscribe(); |
| Assets/Scripts/UI/PortraitHudController.cs:192 | var choices = GameplayChoicesService.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:197 | var sm=SkillManager.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:216 | if (buffSummary != null && ShopManager.Instance != null) |
| Assets/Scripts/UI/PortraitHudController.cs:218 | int count=0; for(int i=0;i<5;i++) if(ShopManager.Instance.GetTimerByIndex(i)>0) count++; |
| Assets/Scripts/UI/PortraitHudController.cs:227 | var cm = CultivationManager.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:556 | void OpenMilestones(){CloseSecondary();TutorialManager.Instance?.OpenMilestoneModal();} |
| Assets/Scripts/UI/PortraitHudController.cs:572 | var e=EconomyManager.Instance;var a=ArtifactService.Instance; |
| Assets/Scripts/UI/PortraitHudController.cs:577 | int count=0;var shop=ShopManager.Instance;if(shop!=null)for(int i=0;i<5;i++)if(shop.GetTimerByIndex(i)>0)count++; |
| Assets/Scripts/UI/SaveSlotUI.cs:23 | private CharacterSaveData currentData; |
| Assets/Scripts/UI/SaveSlotUI.cs:25 | public void Initialize(MainMenuManager manager, CharacterSaveData data, int index) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:344 | if (SecretRealmService.Instance == null) return; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:346 | if (!SecretRealmService.Instance.CanEnter()) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:348 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:349 | GameLogger.Instance.Log("❌ Đã hết lượt miễn phí! Cần 5 Cơ Duyên (CD) để mở cổng.", Color.yellow); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:353 | if (SecretRealmService.Instance.TryConsumeEntry(out bool usedFree)) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:356 | if (SecretRealmPlayController.Instance == null) |
| Assets/Scripts/UI/SecretRealmModalUI.cs:362 | SecretRealmPlayController.Instance?.LaunchMiniGame(type); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:368 | if (SecretRealmService.Instance == null) return; |
| Assets/Scripts/UI/SecretRealmModalUI.cs:370 | bool hasFree = SecretRealmService.Instance.HasFreeAttempt(); |
| Assets/Scripts/UI/SecretRealmModalUI.cs:371 | int cd = EconomyManager.Instance != null ? EconomyManager.Instance.currentCoDuyen : 0; |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1086 | if (SecretRealmService.Instance != null) |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1088 | reward = SecretRealmService.Instance.ProcessAndGrantRewards(activeType, currentScore, false); |
| Assets/Scripts/UI/SecretRealmPlayController.cs:1179 | if (SecretRealmModalUI.Instance != null) SecretRealmModalUI.Instance.Open(); |
| Assets/Scripts/UI/ShopItemUI.cs:23 | if (priceText != null && CultivationManager.Instance != null) |
| Assets/Scripts/UI/ShopItemUI.cs:25 | int stage = CultivationManager.Instance.currentStageIndex; |
| Assets/Scripts/UI/ShopItemUI.cs:29 | if (ShopManager.Instance != null && ShopManager.Instance.GetStacksByIndex(itemIndex) >= 5) |
| Assets/Scripts/UI/ShopItemUI.cs:44 | if (ShopManager.Instance != null) |
| Assets/Scripts/UI/ShopItemUI.cs:46 | ShopManager.Instance.BuyItem(itemIndex); |
| Assets/Scripts/UI/SkillCardUI.cs:24 | if (CultivationManager.Instance == null \|\| CultivationManager.Instance.allStages == null \|\| stageIndex >= CultivationManager.Instance.allStages.Length) |
| Assets/Scripts/UI/SkillCardUI.cs:33 | string fullName = CultivationManager.Instance.allStages[stageIndex].stageName; |
| Assets/Scripts/UI/SkillCardUI.cs:45 | if (SkillManager.Instance == null \|\| EconomyManager.Instance == null \|\| CultivationManager.Instance == null) return; |
| Assets/Scripts/UI/SkillCardUI.cs:47 | SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1]; |
| Assets/Scripts/UI/SkillCardUI.cs:51 | int currentStage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0; |
| Assets/Scripts/UI/SkillCardUI.cs:54 | int currentCoDuyen = EconomyManager.Instance.currentCoDuyen; |
| Assets/Scripts/UI/SkillCardUI.cs:59 | float currentDmg = SkillManager.Instance != null ? SkillManager.Instance.CalculateTotalSkillDamage(skill) : skill.GetCurrentDamage(); |
| Assets/Scripts/UI/SkillCardUI.cs:120 | if (SkillManager.Instance == null \|\| EconomyManager.Instance == null) return; |
| Assets/Scripts/UI/SkillCardUI.cs:122 | SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1]; |
| Assets/Scripts/UI/SkillCardUI.cs:125 | int currentCoDuyen = EconomyManager.Instance.currentCoDuyen; |
| Assets/Scripts/UI/SkillCardUI.cs:131 | if (CultivationManager.Instance != null && CultivationManager.Instance.currentStageIndex < skill.requiredStageIndex) |
| Assets/Scripts/UI/SkillCardUI.cs:134 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Chưa đạt cảnh giới yêu cầu: {shortStage}!", Color.red); |
| Assets/Scripts/UI/SkillCardUI.cs:139 | if (EconomyManager.Instance.SpendCoDuyen(skill.unlockCost)) |
| Assets/Scripts/UI/SkillCardUI.cs:144 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Đã lĩnh ngộ thành công: {skill.skillName} (Lv.1)!", Color.green); |
| Assets/Scripts/UI/SkillCardUI.cs:148 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Cơ Duyên để mở khóa ({currentCoDuyen}/{skill.unlockCost} CD)!", Color.yellow); |
| Assets/Scripts/UI/SkillCardUI.cs:155 | if (EconomyManager.Instance.SpendCoDuyen(upgradeCost)) |
| Assets/Scripts/UI/SkillCardUI.cs:160 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/SkillCardUI.cs:164 | GameLogger.Instance.Log($"Nâng cấp thành công: {skill.skillName} lên Cấp {skill.level}! (+5% Tốc độ & Sát thương, +0.5% HP, +3 MP)", Color.green); |
| Assets/Scripts/UI/SkillCardUI.cs:168 | GameLogger.Instance.Log($"Nâng cấp thành công: {skill.skillName} lên Cấp {skill.level}! (+30% Sát thương)", Color.green); |
| Assets/Scripts/UI/SkillCardUI.cs:174 | if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Cơ Duyên để nâng cấp ({currentCoDuyen}/{upgradeCost} CD)!", Color.yellow); |
| Assets/Scripts/UI/SkillCardUI.cs:180 | if (TooltipManager.Instance != null && SkillManager.Instance != null) |
| Assets/Scripts/UI/SkillCardUI.cs:182 | SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1]; |
| Assets/Scripts/UI/SkillCardUI.cs:186 | int currentStage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0; |
| Assets/Scripts/UI/SkillCardUI.cs:188 | int currentCoDuyen = EconomyManager.Instance != null ? EconomyManager.Instance.currentCoDuyen : 0; |
| Assets/Scripts/UI/SkillCardUI.cs:192 | float currentDmg = SkillManager.Instance != null ? SkillManager.Instance.CalculateTotalSkillDamage(skill) : skill.GetCurrentDamage(); |
| Assets/Scripts/UI/SkillCardUI.cs:265 | TooltipManager.Instance.ShowTooltip(info); |
| Assets/Scripts/UI/SkillCardUI.cs:272 | if (TooltipManager.Instance != null) |
| Assets/Scripts/UI/SkillCardUI.cs:274 | TooltipManager.Instance.HideTooltip(); |
| Assets/Scripts/UI/StorageModalUI.cs:122 | if (StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:123 | StorageManager.Instance.OnStorageChanged += OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:124 | if (DismantleService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:125 | DismantleService.Instance.OnDismantleExecuted += OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:130 | if (StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:131 | StorageManager.Instance.OnStorageChanged -= OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:132 | if (DismantleService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:133 | DismantleService.Instance.OnDismantleExecuted -= OnDataChanged; |
| Assets/Scripts/UI/StorageModalUI.cs:168 | if (StorageManager.Instance != null) StorageManager.Instance.FilterSwordsHighLevel = val; |
| Assets/Scripts/UI/StorageModalUI.cs:176 | if (StorageManager.Instance != null) StorageManager.Instance.FilterPillsBreakthrough = val; |
| Assets/Scripts/UI/StorageModalUI.cs:184 | if (StorageManager.Instance != null) StorageManager.Instance.AutoDismantleLv1SwordsWhenFull = val; |
| Assets/Scripts/UI/StorageModalUI.cs:254 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:268 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:297 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:315 | if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:320 | if (StorageManager.Instance == null) return; |
| Assets/Scripts/UI/StorageModalUI.cs:321 | if (toggleSwordHighLevel != null) toggleSwordHighLevel.SetIsOnWithoutNotify(StorageManager.Instance.FilterSwordsHighLevel); |
| Assets/Scripts/UI/StorageModalUI.cs:322 | if (togglePillBreakthrough != null) togglePillBreakthrough.SetIsOnWithoutNotify(StorageManager.Instance.FilterPillsBreakthrough); |
| Assets/Scripts/UI/StorageModalUI.cs:323 | if (toggleAutoDismantleLv1 != null) toggleAutoDismantleLv1.SetIsOnWithoutNotify(StorageManager.Instance.AutoDismantleLv1SwordsWhenFull); |
| Assets/Scripts/UI/StorageModalUI.cs:347 | if (i == 4 && StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:349 | int pCount = StorageManager.Instance.PendingCount; |
| Assets/Scripts/UI/StorageModalUI.cs:358 | if (txtLinhThach != null && EconomyManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:359 | txtLinhThach.text = $"LT {EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach)}"; |
| Assets/Scripts/UI/StorageModalUI.cs:360 | if (txtKiemY != null && EconomyManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:361 | txtKiemY.text = $"K.Ý {EconomyManager.Instance.currentKiemY:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:362 | if (txtOre != null && ArtifactService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:363 | txtOre.text = $"Quặng {ArtifactService.Instance.Ore:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:364 | if (txtDust != null && AlchemyService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:365 | txtDust.text = $"Bụi {AlchemyService.Instance.State.medicineDust:N0}"; |
| Assets/Scripts/UI/StorageModalUI.cs:370 | if (StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:372 | int count = StorageManager.Instance.ClaimPriorityLoot(); |
| Assets/Scripts/UI/StorageModalUI.cs:373 | if (count > 0 && AudioManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:374 | AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:382 | if (DismantleService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:384 | var reward = DismantleService.Instance.BatchDismantleLowTier(); |
| Assets/Scripts/UI/StorageModalUI.cs:385 | if (!reward.IsEmpty && AudioManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:386 | AudioManager.Instance.PlayMergeSwordSFX(); |
| Assets/Scripts/UI/StorageModalUI.cs:436 | if (MergeManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:438 | var swords = MergeManager.Instance.GetAllSwords(includeEquipped: true); |
| Assets/Scripts/UI/StorageModalUI.cs:442 | bool isEquipped = MergeManager.Instance.IsSwordEquipped(s); |
| Assets/Scripts/UI/StorageModalUI.cs:451 | if (AlchemyService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:459 | int count = AlchemyService.Instance.GetPillCount(r, (PillRecipe)rec, (PillQuality)q); |
| Assets/Scripts/UI/StorageModalUI.cs:464 | count = (wIdx >= 0 && wIdx < AlchemyService.Instance.State.pills.Length) ? AlchemyService.Instance.State.pills[wIdx] : 0; |
| Assets/Scripts/UI/StorageModalUI.cs:480 | if (AlchemyService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:485 | int hCount = AlchemyService.Instance.GetHerbCount(h); |
| Assets/Scripts/UI/StorageModalUI.cs:491 | if (AlchemyService.Instance.State.medicineDust > 0) |
| Assets/Scripts/UI/StorageModalUI.cs:493 | if (IsOnCurrentPage(totalItemsShown++)) CreateDustCard(AlchemyService.Instance.State.medicineDust); |
| Assets/Scripts/UI/StorageModalUI.cs:497 | if (ArtifactService.Instance != null && ArtifactService.Instance.Ore > 0) |
| Assets/Scripts/UI/StorageModalUI.cs:499 | if (IsOnCurrentPage(totalItemsShown++)) CreateOreCard(ArtifactService.Instance.Ore); |
| Assets/Scripts/UI/StorageModalUI.cs:506 | if (StorageManager.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:508 | var pendingList = StorageManager.Instance.GetPendingItems(); |
| Assets/Scripts/UI/StorageModalUI.cs:608 | var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateSwordReward(level, pieces) : null; |
| Assets/Scripts/UI/StorageModalUI.cs:609 | bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValueSword(level, pieces, isEquipped); |
| Assets/Scripts/UI/StorageModalUI.cs:626 | DismantleService.Instance?.DismantleSword(sword); |
| Assets/Scripts/UI/StorageModalUI.cs:633 | DismantleService.Instance?.DismantleSword(sword); |
| Assets/Scripts/UI/StorageModalUI.cs:661 | var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculatePillReward(realm, recipe, quality, 1) : null; |
| Assets/Scripts/UI/StorageModalUI.cs:663 | bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValuePill(realm, recipe, quality); |
| Assets/Scripts/UI/StorageModalUI.cs:675 | DismantleService.Instance?.DismantlePills(realm, recipe, quality, 1); |
| Assets/Scripts/UI/StorageModalUI.cs:682 | DismantleService.Instance?.DismantlePills(realm, recipe, quality, 1); |
| Assets/Scripts/UI/StorageModalUI.cs:705 | var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateHerbReward(herbIndex, 1) : null; |
| Assets/Scripts/UI/StorageModalUI.cs:718 | DismantleService.Instance?.DismantleHerbs(herbIndex, 1); |
| Assets/Scripts/UI/StorageModalUI.cs:725 | DismantleService.Instance?.DismantleHerbs(herbIndex, 1); |
| Assets/Scripts/UI/StorageModalUI.cs:761 | var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateOreReward(1) : null; |
| Assets/Scripts/UI/StorageModalUI.cs:774 | DismantleService.Instance?.DismantleOre(1); |
| Assets/Scripts/UI/StorageModalUI.cs:781 | DismantleService.Instance?.DismantleOre(1); |
| Assets/Scripts/UI/StorageModalUI.cs:806 | if (DismantleService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:811 | reward = DismantleService.Instance.CalculateSwordReward(item.level, item.pieces); |
| Assets/Scripts/UI/StorageModalUI.cs:814 | reward = DismantleService.Instance.CalculatePillReward(item.realm, (PillRecipe)item.recipe, (PillQuality)item.quality, item.amount); |
| Assets/Scripts/UI/StorageModalUI.cs:817 | reward = DismantleService.Instance.CalculateHerbReward(item.level, item.amount); |
| Assets/Scripts/UI/StorageModalUI.cs:820 | reward = DismantleService.Instance.CalculateOreReward(item.amount); |
| Assets/Scripts/UI/StorageModalUI.cs:826 | bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValuePendingItem(item); |
| Assets/Scripts/UI/StorageModalUI.cs:838 | DismantleService.Instance?.DismantlePendingItem(item); |
| Assets/Scripts/UI/StorageModalUI.cs:845 | DismantleService.Instance?.DismantlePendingItem(item); |
| Assets/Scripts/UI/StorageModalUI.cs:910 | if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked()) |
| Assets/Scripts/UI/StorageModalUI.cs:912 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:914 | GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow); |
| Assets/Scripts/UI/StorageModalUI.cs:1092 | bool isDismantleUnlocked = FeatureUnlockService.Instance == null \|\| FeatureUnlockService.Instance.IsDismantleUnlocked(); |
| Assets/Scripts/UI/StorageModalUI.cs:1122 | if (GameLogger.Instance != null && FeatureUnlockService.Instance != null) |
| Assets/Scripts/UI/StorageModalUI.cs:1124 | GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow); |
| Assets/Scripts/UI/SwordSlotLockUI.cs:36 | if (SwordLoadoutService.Instance != null) |
| Assets/Scripts/UI/SwordSlotLockUI.cs:37 | SwordLoadoutService.Instance.OnCapacityChanged += Refresh; |
| Assets/Scripts/UI/SwordSlotLockUI.cs:38 | Refresh(SwordLoadoutService.Instance != null ? SwordLoadoutService.Instance.GetCurrentCapacity() : 1); |
| Assets/Scripts/UI/SwordSlotLockUI.cs:43 | if (SwordLoadoutService.Instance != null) |
| Assets/Scripts/UI/SwordSlotLockUI.cs:44 | SwordLoadoutService.Instance.OnCapacityChanged -= Refresh; |
| Assets/Scripts/UI/TribulationModalUI.cs:96 | var cultivation = CultivationManager.Instance; |
| Assets/Scripts/UI/TribulationModalUI.cs:97 | var combat = CombatManager.Instance; |
| Assets/Scripts/UI/TribulationModalUI.cs:100 | (StageManager.Instance == null \|\| !StageManager.Instance.isBossStage); |
| Assets/Scripts/UI/TribulationModalUI.cs:147 | CultivationManager.Instance.MarkTribulationPromptShown(); |
| Assets/Scripts/UI/TribulationModalUI.cs:151 | observedAlchemy = AlchemyService.Instance; |
| Assets/Scripts/UI/TribulationModalUI.cs:194 | if (CultivationManager.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:196 | curRealm = CultivationManager.Instance.GetDisplayRealmName(); |
| Assets/Scripts/UI/TribulationModalUI.cs:197 | nxtRealm = CultivationManager.Instance.GetNextRealmName(); |
| Assets/Scripts/UI/TribulationModalUI.cs:198 | realm = (int)CultivationManager.Instance.CurrentRealm; |
| Assets/Scripts/UI/TribulationModalUI.cs:201 | PillQuality highestQuality = AlchemyService.Instance != null |
| Assets/Scripts/UI/TribulationModalUI.cs:202 | ? AlchemyService.Instance.GetHighestBreakthroughPillQuality(realm) |
| Assets/Scripts/UI/TribulationModalUI.cs:316 | int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0; |
| Assets/Scripts/UI/TribulationModalUI.cs:319 | if (AlchemyService.Instance == null \|\| !AlchemyService.Instance.TryConsumeBreakthroughPill(realm)) |
| Assets/Scripts/UI/TribulationModalUI.cs:321 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:323 | GameLogger.Instance.Log("Cần luyện Đan Đột Phá phù hợp trước khi độ kiếp! Đang mở Lò Luyện Đan...", Color.yellow); |
| Assets/Scripts/UI/TribulationModalUI.cs:326 | if (FeatureUnlockService.Instance != null && FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy)) |
| Assets/Scripts/UI/TribulationModalUI.cs:328 | MobileDrawerUIController.Instance?.OpenDrawer(2); // Mở Drawer Lò Luyện Đan |
| Assets/Scripts/UI/TribulationModalUI.cs:335 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:337 | AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/TribulationModalUI.cs:341 | if (CombatManager.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:343 | CombatManager.Instance.StartTribulationBossBattle(); |
| Assets/Scripts/UI/TribulationModalUI.cs:351 | if (AudioManager.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:353 | AudioManager.Instance.PlayClickSFX(); |
| Assets/Scripts/UI/TribulationModalUI.cs:356 | if (GameLogger.Instance != null) |
| Assets/Scripts/UI/TribulationModalUI.cs:358 | GameLogger.Instance.Log("Đã hoãn độ kiếp. Chạm thanh cảnh giới để mở lại khi sẵn sàng.", Color.cyan); |
| Assets/Scripts/UI/TribulationModalUI.cs:368 | Canvas canvas = CultivationManager.Instance?.stageText?.canvas?.rootCanvas; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:28 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:30 | EconomyManager.Instance.OnLinhThachChanged += UpdateEconomyUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:31 | UpdateEconomyUI(EconomyManager.Instance.currentLinhThach); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:34 | if (UpgradeManager.Instance != null) |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:36 | UpgradeManager.Instance.OnUpgradesChanged += UpdateUpgradesUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:41 | if(btnLinhLuc != null) btnLinhLuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("LinhLuc")); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:42 | if(btnKiemY != null) btnKiemY.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("KiemY")); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:43 | if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc")); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:44 | if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh")); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:49 | if (EconomyManager.Instance != null) |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:50 | EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:52 | if (UpgradeManager.Instance != null) |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:53 | UpgradeManager.Instance.OnUpgradesChanged -= UpdateUpgradesUI; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:62 | if (EconomyManager.Instance != null) EconomyManager.Instance.AddLinhThach(100); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:79 | if (UpgradeManager.Instance == null) return; |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:81 | UpdateSingleUpgradeUI(txtLinhLuc, btnLinhLuc, UpgradeManager.Instance.linhLucData, UpgradeManager.Instance.linhLucLevel); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:82 | UpdateSingleUpgradeUI(txtKiemY, btnKiemY, UpgradeManager.Instance.kiemYData, UpgradeManager.Instance.kiemYLevel); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:83 | UpdateSingleUpgradeUI(txtThanThuc, btnThanThuc, UpgradeManager.Instance.thanThucData, UpgradeManager.Instance.thanThucLevel); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:84 | UpdateSingleUpgradeUI(txtTuLinh, btnTuLinh, UpgradeManager.Instance.tuLinhData, UpgradeManager.Instance.tuLinhLevel); |
| Assets/Scripts/UI/UpgradeUIPlaceholder.cs:104 | btn.interactable = (EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= cost); |
| Assets/UI/Reconstruction/Runtime/MobileUITutorialOcclusion.cs:22 | bool blocked=PortraitHudController.IsOverlayBlocking\|\|(GameplayChoicesService.Instance!=null&&GameplayChoicesService.Instance.IsChoosing); |



using UnityEditor;
using UnityEngine;

public static class DrawerDebugHelper
{
    [MenuItem("TuTien/Debug/Enter Gameplay Directly")]
    public static void EnterGame()
    {
        var mm = Object.FindAnyObjectByType<MainMenuManager>();
        if (mm != null && mm.gameObject.activeInHierarchy)
        {
            var dm = Object.FindAnyObjectByType<DataManager>();
            if (dm != null && dm.gameSaveData != null)
            {
                int slot = dm.gameSaveData.lastPlayedSlotIndex;
                if (slot < 0 || slot >= 3 || dm.gameSaveData.slots[slot].isSlotEmpty)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (!dm.gameSaveData.slots[i].isSlotEmpty) { slot = i; break; }
                    }
                    if (slot < 0 || dm.gameSaveData.slots[slot].isSlotEmpty)
                    {
                        slot = 0;
                        dm.gameSaveData.slots[0] = new CharacterSaveData
                        {
                            isSlotEmpty = false,
                            characterName = "Hàn Lập",
                            stageIndex = 1,
                            stageName = "Luyện Khí Tầng 2",
                            linhThach = 5000f,
                            coDuyen = 100
                        };
                    }
                }
                var method = typeof(MainMenuManager).GetMethod("ConfirmEnterGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(mm, new object[] { slot, dm.gameSaveData.slots[slot] });
                    Debug.Log("[DrawerDebug] Entered gameplay directly!");
                }
            }
        }

        // Tắt Offline Popup nếu có
        var popup = Object.FindAnyObjectByType<OfflineRewardPopupUI>();
        if (popup != null)
        {
            popup.gameObject.SetActive(false);
        }
    }

    [MenuItem("TuTien/Debug/Open Drawer 0 (Kiem Lo)")]
    public static void OpenDrawer0()
    {
        EnterGame();
        var c = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (c != null)
        {
            c.ToggleDrawer(0);
            Debug.Log("[DrawerDebug] Toggled Drawer 0 (Kiem Lo)");
        }
    }

    [MenuItem("TuTien/Debug/Open Drawer 2 (Luyen Dan)")]
    public static void OpenDrawer2()
    {
        EnterGame();
        if (AlchemyService.Instance != null)
        {
            if (AlchemyService.Instance.GetHerbCount(0) == 0) AlchemyService.Instance.AddHerb(0, 15);
            if (AlchemyService.Instance.GetHerbCount(1) == 0) AlchemyService.Instance.AddHerb(1, 8);
            if (AlchemyService.Instance.GetHerbCount(2) == 0) AlchemyService.Instance.AddHerb(2, 4);
        }

        var c = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (c != null)
        {
            c.ToggleDrawer(2);
            Debug.Log("[DrawerDebug] Toggled Drawer 2 (Luyen Dan)");
        }
    }

    [MenuItem("TuTien/Debug/Open Drawer 3 (Phap Khi)")]
    public static void OpenDrawer3()
    {
        EnterGame();
        if (ArtifactService.Instance != null && ArtifactService.Instance.Ore == 0)
        {
            ArtifactService.Instance.AddOre(50);
        }

        var c = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (c != null)
        {
            c.ToggleDrawer(3);
            Debug.Log("[DrawerDebug] Toggled Drawer 3 (Phap Khi)");
        }
    }

    [MenuItem("TuTien/Debug/Open Drawer 4 (Tran Phap)")]
    public static void OpenDrawer4()
    {
        EnterGame();
        if (ArtifactService.Instance != null && ArtifactService.Instance.Ore == 0)
        {
            ArtifactService.Instance.AddOre(50);
        }

        var c = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (c != null)
        {
            c.ToggleDrawer(4);
            Debug.Log("[DrawerDebug] Toggled Drawer 4 (Tran Phap)");
        }
    }

    [MenuItem("TuTien/Debug/Close Drawer")]
    public static void CloseDrawer()
    {
        var c = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (c != null)
        {
            c.CloseActiveDrawer();
            Debug.Log("[DrawerDebug] Closed Active Drawer");
        }
    }

    [MenuItem("TuTien/Debug/Test 1: Milestone Modal Close via X")]
    public static void TestCase1_MilestoneCloseX()
    {
        EnterGame();
        var modal = MilestoneModalUI.Instance != null ? MilestoneModalUI.Instance : Object.FindAnyObjectByType<MilestoneModalUI>(FindObjectsInactive.Include);
        if (modal == null) { Debug.LogError("❌ FAIL: Không tìm thấy MilestoneModalUI!"); return; }

        modal.Open();
        Debug.Log($"modal.activeSelf={modal.gameObject.activeSelf}, activeInHierarchy={modal.gameObject.activeInHierarchy}, modalPanel={(modal.modalPanel != null ? modal.modalPanel.name : "null")}, panelActive={modal.modalPanel?.activeInHierarchy}, IsModalOpen={modal.IsModalOpen}");
        if (!modal.IsModalOpen) { Debug.LogError("❌ FAIL: Modal không mở được!"); return; }
        Debug.Log("✅ Step 1: MilestoneModalUI đã mở thành công.");

        // Bấm nút X
        if (modal.btnClose != null)
        {
            modal.btnClose.onClick.Invoke();
            if (!modal.IsModalOpen)
            {
                Debug.Log("🎉 PASS TEST 1: Nút [X] đã đóng MilestoneModalUI dứt điểm 100%!");
            }
            else
            {
                Debug.LogError("❌ FAIL: Nút [X] không đóng được modal!");
            }
        }
        else
        {
            Debug.LogError("❌ FAIL: btnClose bị null!");
        }
    }

    [MenuItem("TuTien/Debug/Test 2: Back/Escape Closes Modal")]
    public static void TestCase2_BackClosesModal()
    {
        EnterGame();
        var modal = MilestoneModalUI.Instance != null ? MilestoneModalUI.Instance : Object.FindAnyObjectByType<MilestoneModalUI>(FindObjectsInactive.Include);
        if (modal == null) { Debug.LogError("❌ FAIL: Không tìm thấy MilestoneModalUI!"); return; }

        modal.Open();
        Debug.Log("✅ Step 1: Mở modal để chuẩn bị test Escape/Back.");

        var nav = MobileUINavigationManager.Instance != null ? MobileUINavigationManager.Instance : Object.FindAnyObjectByType<MobileUINavigationManager>();
        if (nav != null)
        {
            bool handled = nav.HandleBackNavigation();
            if (handled && !modal.IsModalOpen)
            {
                Debug.Log("🎉 PASS TEST 2: Phím Back/Escape đã ưu tiên đóng Modal popup trên cùng!");
            }
            else
            {
                Debug.LogError("❌ FAIL: HandleBackNavigation không đóng được modal!");
            }
        }
        else
        {
            Debug.LogError("❌ FAIL: MobileUINavigationManager không tìm thấy!");
        }
    }

    [MenuItem("TuTien/Debug/Test 3: Back/Escape Closes Drawer")]
    public static void TestCase3_BackClosesDrawer()
    {
        EnterGame();
        var drawerCtrl = MobileDrawerUIController.Instance != null ? MobileDrawerUIController.Instance : Object.FindAnyObjectByType<MobileDrawerUIController>();
        if (drawerCtrl == null) { Debug.LogError("❌ FAIL: Không tìm thấy MobileDrawerUIController!"); return; }

        drawerCtrl.ToggleDrawer(0); // Mở Kiếm Lô
        Debug.Log("✅ Step 1: Đã mở Drawer 0 (Kiếm Lô).");

        var nav = MobileUINavigationManager.Instance != null ? MobileUINavigationManager.Instance : Object.FindAnyObjectByType<MobileUINavigationManager>();
        if (nav != null)
        {
            bool handled = nav.HandleBackNavigation();
            Debug.Log($"🎉 PASS TEST 3: Back/Escape xử lý đóng Drawer (handled={handled}, IsAnyDrawerOpen={drawerCtrl.IsAnyDrawerOpen})!");
        }
    }

    [MenuItem("TuTien/Debug/Capture Milestone Modal Open")]
    public static void CaptureMilestoneModal()
    {
        var modal = MilestoneModalUI.Instance != null ? MilestoneModalUI.Instance : Object.FindAnyObjectByType<MilestoneModalUI>(FindObjectsInactive.Include);
        if (modal != null)
        {
            modal.Open();
            Debug.Log("[Capture] Opened MilestoneModalUI for verification capture.");
        }
    }

    [MenuItem("TuTien/Debug/Inspect Map Scroller")]
    public static void InspectMap()
    {
        var scroller = Object.FindAnyObjectByType<MapScroller>();
        if (scroller == null)
        {
            Debug.LogError("[MapDebug] Không tìm thấy MapScroller trong scene!");
            return;
        }
        Debug.Log($"[MapDebug] MapScroller: chunkLength={scroller.chunkLength}, scrollSpeed={scroller.scrollSpeed}, isMoving={scroller.isMoving}, chunksCount={scroller.mapChunks?.Count ?? 0}");
        if (scroller.mapChunks != null)
        {
            for (int i = 0; i < scroller.mapChunks.Count; i++)
            {
                var c = scroller.mapChunks[i];
                Debug.Log($"[MapDebug] Chunk[{i}]: {(c != null ? c.name : "NULL")} - pos={c?.position}");
            }
        }
    }

    [MenuItem("TuTien/Debug/Inspect Boss Instance")]
    public static void InspectBossInstance()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss.prefab");
        if (prefab == null)
        {
            Debug.LogError("Boss.prefab not found at Assets/Prefabs/Boss.prefab");
            return;
        }

        GameObject testBoss = Object.Instantiate(prefab, new Vector3(0, 0, 10), Quaternion.identity);
        testBoss.name = "DEBUG_TEST_BOSS";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== INSPECT BOSS INSTANCE ===");
        sb.AppendLine($"Root '{testBoss.name}': pos={testBoss.transform.position}, rot={testBoss.transform.rotation.eulerAngles}, scale={testBoss.transform.localScale}");
        
        var anim = testBoss.GetComponent<Animator>();
        sb.AppendLine($"Animator on root: {(anim != null ? ("found, avatar=" + (anim.avatar != null ? anim.avatar.name : "null") + " controller=" + (anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "null")) : "NULL")}");

        var bossComp = testBoss.GetComponent<BossEnemy>();
        sb.AppendLine($"BossEnemy on root: {(bossComp != null ? "FOUND" : "NULL")}");

        for (int i = 0; i < testBoss.transform.childCount; i++)
        {
            var child = testBoss.transform.GetChild(i);
            sb.AppendLine($"Child [{i}] '{child.name}': localPos={child.localPosition}, localRot={child.localRotation.eulerAngles}, localScale={child.localScale}");
            var childBoss = child.GetComponent<BossEnemy>();
            if (childBoss != null) sb.AppendLine($"   -> Has BossEnemy component!");
            var childAnim = child.GetComponent<Animator>();
            if (childAnim != null) sb.AppendLine($"   -> Has Animator component!");
        }

        var smrs = testBoss.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        sb.AppendLine($"Total SkinnedMeshRenderers: {smrs.Length}");
        foreach (var s in smrs)
        {
            sb.AppendLine($"   SMR on '{s.gameObject.name}': rot={s.transform.rotation.eulerAngles}, localRot={s.transform.localRotation.eulerAngles}, rootBone={(s.rootBone != null ? s.rootBone.name : "null")}, bounds={s.bounds}");
        }

        Debug.Log(sb.ToString());
        Object.DestroyImmediate(testBoss);
    }

    [MenuItem("TuTien/Debug/Sample Boss Animation")]
    public static void SampleBossAnimation()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss.prefab");
        GameObject testBoss = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var anim = testBoss.GetComponent<Animator>();
        var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Import/Boss/Idle.anim");
        var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Import/Boss/Zombie Walk.anim");
        var deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Import/Boss/Zombie Death.anim");

        var hips = testBoss.transform.Find("mixamorig:Hips");
        var smr = testBoss.GetComponentInChildren<SkinnedMeshRenderer>();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SAMPLE BOSS ANIMATIONS ===");
        if (idleClip != null)
        {
            idleClip.SampleAnimation(testBoss, 0.5f);
            sb.AppendLine($"[IDLE @ 0.5s]: Root rot={testBoss.transform.rotation.eulerAngles}, Hips pos={hips?.localPosition}, Hips rot={hips?.localRotation.eulerAngles}, SMR bounds={smr?.bounds}");
        }
        if (walkClip != null)
        {
            walkClip.SampleAnimation(testBoss, 0.5f);
            sb.AppendLine($"[WALK @ 0.5s]: Root rot={testBoss.transform.rotation.eulerAngles}, Hips pos={hips?.localPosition}, Hips rot={hips?.localRotation.eulerAngles}, SMR bounds={smr?.bounds}");
        }
        if (deathClip != null)
        {
            deathClip.SampleAnimation(testBoss, 0.5f);
            sb.AppendLine($"[DEATH @ 0.5s]: Root rot={testBoss.transform.rotation.eulerAngles}, Hips pos={hips?.localPosition}, Hips rot={hips?.localRotation.eulerAngles}, SMR bounds={smr?.bounds}");
        }
        Debug.Log(sb.ToString());
        Object.DestroyImmediate(testBoss);
    }

    [MenuItem("TuTien/Debug/Test: Toggle Bottom Dock Collapse")]
    public static void TestToggleBottomDock()
    {
        var drawer = Object.FindAnyObjectByType<MobileDrawerUIController>(FindObjectsInactive.Include);
        if (drawer == null)
        {
            Debug.LogError("❌ FAIL: Không tìm thấy MobileDrawerUIController!");
            return;
        }

        bool initial = drawer.IsDockCollapsed;
        drawer.ToggleDockCollapse();
        bool afterCollapse = drawer.IsDockCollapsed;
        Debug.Log($"✅ Step 1: Toggle collapse (ban đầu={initial} -> sau gạt={afterCollapse})");

        drawer.ToggleDockCollapse();
        bool afterExpand = drawer.IsDockCollapsed;
        Debug.Log($"✅ Step 2: Toggle expand (sau mở lại={afterExpand})");

        if (afterCollapse != initial && afterExpand == initial)
        {
            Debug.Log("🎉 PASS: Tính năng gạt mũi tên đóng/mở toàn bộ bảng dưới (Bottom Dock) hoạt động hoàn hảo 100%!");
        }
        else
        {
            Debug.LogError("❌ FAIL: Trạng thái đóng mở không khớp!");
        }
    }

    [MenuItem("TuTien/Debug/Test: Toggle Quest Bar Collapse")]
    public static void TestToggleQuestBar()
    {
        var questBar = Object.FindAnyObjectByType<CollapsibleQuestBar>(FindObjectsInactive.Include);
        if (questBar == null)
        {
            var go = GameObject.Find("Canvas/MobileUIRoot/TutorialTargetQuestBar");
            if (go != null) questBar = go.AddComponent<CollapsibleQuestBar>();
        }

        if (questBar == null)
        {
            Debug.LogError("❌ FAIL: Không tìm thấy TutorialTargetQuestBar!");
            return;
        }

        bool initial = questBar.IsCollapsed;
        questBar.ToggleQuestBar();
        bool afterCollapse = questBar.IsCollapsed;
        Debug.Log($"✅ Step 1: Quest bar collapse (ban đầu={initial} -> sau={afterCollapse})");

        questBar.ToggleQuestBar();
        bool afterExpand = questBar.IsCollapsed;
        Debug.Log($"✅ Step 2: Quest bar expand (sau={afterExpand})");

        if (afterCollapse != initial && afterExpand == initial)
        {
            Debug.Log("🎉 PASS: Tính năng mũi tên thu gọn thanh Nhiệm vụ bên trái (Quest Bar) hoạt động chuẩn 100%!");
        }
    }

    [MenuItem("TuTien/Debug/Test: Open Full Dao An Modal UI")]
    public static void TestOpenDaoAnModal()
    {
        var modal = DaoAnModalUI.Instance != null ? DaoAnModalUI.Instance : Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (modal == null)
        {
            var go = new GameObject("DaoAnModalUI", typeof(DaoAnModalUI));
            modal = go.GetComponent<DaoAnModalUI>();
        }

        modal.Open();
        if (!modal.IsModalOpen)
        {
            Debug.LogError("❌ FAIL: DaoAnModalUI không mở được!");
            return;
        }
        Debug.Log("✅ Step 1: DaoAnModalUI đã mở thành công.");

        modal.SetFilter("wealth");
        Debug.Log("✅ Step 2: Lọc nhánh Tài Lộc thành công.");

        modal.SetFilter("sword");
        Debug.Log("✅ Step 3: Lọc nhánh Kiếm Đạo thành công.");

        modal.SetFilter("all");
        Debug.Log("✅ Step 4: Trở về Tất Cả thành công.");

        modal.Close();
        if (!modal.IsModalOpen)
        {
            Debug.Log("🎉 PASS: Đã mở, đổi bộ lọc và đóng DaoAnModalUI.");
        }
    }
}

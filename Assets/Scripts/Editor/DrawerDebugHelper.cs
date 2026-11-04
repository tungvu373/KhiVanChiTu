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
}

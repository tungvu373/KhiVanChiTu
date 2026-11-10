#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MobileHUDLayoutTool
{
    [MenuItem("TuTien/UI/Apply Mobile HUD Refinements")]
    public static void ApplyMobileHUDRefinements()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in active scene!");
            return;
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("Canvas/MobileUIRoot not found in active scene!");
            return;
        }

        var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // =========================================================================
        // 1. CẤU HÌNH NÚT ĐẠO VẬN (BtnHUDDaoAn) Ở GÓC TRÊN BÊN TRÁI CẠNH CHARACTERFRAME
        // =========================================================================
        Transform daoAnTr = mobileRoot.Find("BtnHUDDaoAn");
        GameObject daoAnGo = daoAnTr != null ? daoAnTr.gameObject : null;
        if (daoAnGo == null)
        {
            daoAnGo = new GameObject("BtnHUDDaoAn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(DaoAnButtonHandler));
            daoAnGo.transform.SetParent(mobileRoot, false);
        }

        var daoAnRt = daoAnGo.GetComponent<RectTransform>();
        daoAnRt.anchorMin = new Vector2(0f, 1f);
        daoAnRt.anchorMax = new Vector2(0f, 1f);
        daoAnRt.pivot = new Vector2(0f, 1f);
        daoAnRt.anchoredPosition = new Vector2(226f, -10f); // Ngay sát cạnh phải của CharacterFrame (rộng 210)
        daoAnRt.sizeDelta = new Vector2(108f, 42f);

        var daoAnImg = daoAnGo.GetComponent<Image>();
        if (daoAnImg == null) daoAnImg = daoAnGo.AddComponent<Image>();
        daoAnImg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

        var daoAnOutline = daoAnGo.GetComponent<Outline>();
        if (daoAnOutline == null) daoAnOutline = daoAnGo.AddComponent<Outline>();
        daoAnOutline.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.85f);
        daoAnOutline.effectDistance = new Vector2(1f, -1f);

        var daoAnTxt = daoAnGo.GetComponentInChildren<Text>();
        if (daoAnTxt == null)
        {
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(daoAnGo.transform, false);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            daoAnTxt = txtGo.GetComponent<Text>();
        }
        daoAnTxt.font = defaultFont;
        daoAnTxt.text = "<b><color=#FFE666>☯️ ĐẠO VẬN</color></b>";
        daoAnTxt.fontSize = 15;
        daoAnTxt.alignment = TextAnchor.MiddleCenter;

        if (daoAnGo.GetComponent<DaoAnButtonHandler>() == null)
        {
            daoAnGo.AddComponent<DaoAnButtonHandler>();
        }

        daoAnGo.SetActive(true);
        EditorUtility.SetDirty(daoAnGo);
        Debug.Log("✅ [HUDLayout] Đã cấu hình nút '☯️ ĐẠO VẬN' tại pos=(226, -10), kích thước 108x42 cạnh CharacterFrame!");

        // =========================================================================
        // 2. CẤU HÌNH NÚT THU GỌN / MỞ RỘNG BẢNG DƯỚI (BottomDockToggle) TRÊN MOBILEDOCK
        // =========================================================================
        Transform mobileDockTr = mobileRoot.Find("MobileDock");
        Transform dockToggleTr = mobileRoot.Find("BottomDockToggle");
        GameObject dockToggleGo = dockToggleTr != null ? dockToggleTr.gameObject : null;
        if (dockToggleGo == null)
        {
            dockToggleGo = new GameObject("BottomDockToggle", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            dockToggleGo.transform.SetParent(mobileRoot, false);
        }

        // Đảm bảo SiblingIndex nằm ngay sau MobileDock để vẽ đè lên viền dock
        if (mobileDockTr != null)
        {
            dockToggleGo.transform.SetSiblingIndex(mobileDockTr.GetSiblingIndex() + 1);
        }

        var dockToggleRt = dockToggleGo.GetComponent<RectTransform>();
        dockToggleRt.anchorMin = new Vector2(0.5f, 0f);
        dockToggleRt.anchorMax = new Vector2(0.5f, 0f);
        dockToggleRt.pivot = new Vector2(0.5f, 0f);
        dockToggleRt.anchoredPosition = new Vector2(0f, 196f); // 2px trên viền trên của MobileDock
        dockToggleRt.sizeDelta = new Vector2(180f, 44f);

        var dockToggleImg = dockToggleGo.GetComponent<Image>();
        if (dockToggleImg == null) dockToggleImg = dockToggleGo.AddComponent<Image>();
        dockToggleImg.color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

        var dockToggleOutline = dockToggleGo.GetComponent<Outline>();
        if (dockToggleOutline == null) dockToggleOutline = dockToggleGo.AddComponent<Outline>();
        dockToggleOutline.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.85f);
        dockToggleOutline.effectDistance = new Vector2(1f, -1f);

        var dockToggleTxt = dockToggleGo.GetComponentInChildren<Text>();
        if (dockToggleTxt == null)
        {
            var txtGo = new GameObject("LabelText", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(dockToggleGo.transform, false);
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            dockToggleTxt = txtGo.GetComponent<Text>();
        }
        dockToggleTxt.font = defaultFont;
        dockToggleTxt.text = "Thu Gọn Bảng Dưới";
        dockToggleTxt.fontSize = 14;
        dockToggleTxt.fontStyle = FontStyle.Bold;
        dockToggleTxt.alignment = TextAnchor.MiddleCenter;
        dockToggleTxt.color = new Color(0.96f, 0.78f, 0.26f, 1f);

        var drawerCtrl = mobileRoot.GetComponent<MobileDrawerUIController>();
        if (drawerCtrl != null)
        {
            drawerCtrl.dockToggleButton = dockToggleRt;
            drawerCtrl.dockToggleLabelText = dockToggleTxt;
            if (mobileDockTr != null) drawerCtrl.mobileDock = mobileDockTr.GetComponent<RectTransform>();
            Transform hpTr = mobileRoot.Find("PlayerHPBar");
            if (hpTr != null) drawerCtrl.playerHpBar = hpTr.GetComponent<RectTransform>();
            EditorUtility.SetDirty(drawerCtrl);
        }

        var toggleBtn = dockToggleGo.GetComponent<Button>();
        if (toggleBtn != null && drawerCtrl != null)
        {
            toggleBtn.onClick.RemoveListener(drawerCtrl.ToggleDockCollapse);
            toggleBtn.onClick.AddListener(drawerCtrl.ToggleDockCollapse);
        }

        dockToggleGo.SetActive(true);
        EditorUtility.SetDirty(dockToggleGo);
        Debug.Log("✅ [HUDLayout] Đã cấu hình nút 'BottomDockToggle' tại pos=(0, 196), kích thước 180x32 trên đỉnh MobileDock!");

        // =========================================================================
        // 2b. CẤU HÌNH MOBILEDOCK VÀ 5 TAB TRẢI ĐỀU 100% CHIỀU RỘNG MÀN HÌNH (20% MỖI TAB)
        // =========================================================================
        if (mobileDockTr != null)
        {
            var dockRt = mobileDockTr.GetComponent<RectTransform>();
            dockRt.anchorMin = new Vector2(0f, 0f);
            dockRt.anchorMax = new Vector2(1f, 0f);
            dockRt.pivot = new Vector2(0.5f, 0f);
            dockRt.anchoredPosition = Vector2.zero;
            dockRt.sizeDelta = new Vector2(0f, 194f);

            // Xóa LayoutGroup nếu có để tự quản lý tỷ lệ anchor chính xác không bị co giãn méo
            var hlg = mobileDockTr.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) Object.DestroyImmediate(hlg);

            int tabCount = mobileDockTr.childCount;
            for (int i = 0; i < tabCount; i++)
            {
                var tabRt = mobileDockTr.GetChild(i) as RectTransform;
                if (tabRt == null) continue;
                float startX = (float)i / tabCount;
                float endX = (float)(i + 1) / tabCount;
                tabRt.anchorMin = new Vector2(startX, 0f);
                tabRt.anchorMax = new Vector2(endX, 1f);
                tabRt.offsetMin = new Vector2(2f, 4f);
                tabRt.offsetMax = new Vector2(-2f, -4f);
                EditorUtility.SetDirty(tabRt);
            }
            EditorUtility.SetDirty(dockRt);
            Debug.Log($"✅ [HUDLayout] Đã cấu hình MobileDock trải dài 100% màn hình với {tabCount} tab (mỗi tab chiếm {(100f / tabCount):F1}%)!");
        }

        // =========================================================================
        // 2c. TỐI ƯU NÚT PAUSE (Touch Target >= 44x44)
        // =========================================================================
        Transform pauseTr = mobileRoot.Find("PauseButton") ?? mobileRoot.Find("TopBar/PauseButton");
        if (pauseTr != null)
        {
            var pauseRt = pauseTr.GetComponent<RectTransform>();
            if (pauseRt.sizeDelta.x < 44f || pauseRt.sizeDelta.y < 44f)
            {
                pauseRt.sizeDelta = new Vector2(46f, 46f);
                EditorUtility.SetDirty(pauseRt);
                Debug.Log("✅ [HUDLayout] Đã mở rộng nút Pause lên 46x46px chuẩn touch target mobile!");
            }
        }

        // =========================================================================
        // 3. TÁI TẠO TOÀN DIỆN MODAL ĐẠO VẬN (DaoAnModal) VỚI GIAO DIỆN CHUẨN THƯƠNG MẠI
        // =========================================================================
        Transform oldModalTr = canvas.transform.Find("DaoAnModal");
        if (oldModalTr != null)
        {
            Object.DestroyImmediate(oldModalTr.gameObject);
        }

        var daoAnModalUI = Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (daoAnModalUI == null)
        {
            var uiMgr = GameObject.Find("Canvas/UIManager") ?? canvas;
            daoAnModalUI = uiMgr.AddComponent<DaoAnModalUI>();
        }
        daoAnModalUI.BuildFreshModal(canvas.transform);
        Debug.Log("✅ [HUDLayout] Đã tái tạo DaoAnModal với Canvas SortingOrder=90, Nút Close [X], Tab cuộn ngang và Font hợp lệ 100%!");

        // =========================================================================
        // 4. ĐẢM BẢO CÁC MODAL OVERLAY Ở TRẠNG THÁI ẨN (activeSelf = false) KHI MỞ GAME
        // =========================================================================
        string[] modalNames = {
            "StorageModalUI", "DaoAnModal", "PauseMenuModal", "CharacterDetailModal",
            "TribulationModalUI", "MilestoneModalUI", "AchievementModalUI",
            "DismantleConfirmModalUI", "BodyTalentDetailModal"
        };
        foreach (var mName in modalNames)
        {
            var tr = canvas.transform.Find(mName);
            if (tr != null && tr.gameObject.activeSelf)
            {
                tr.gameObject.SetActive(false);
                EditorUtility.SetDirty(tr.gameObject);
                Debug.Log($"✅ [HUDLayout] Đã ẩn overlay '{mName}' để không che phủ màn hình chính.");
            }
        }

        // =========================================================================
        // 5. LƯU SCENE HIỆN TẠI
        // =========================================================================
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        Debug.Log($"🎉 [HUDLayout] Đã áp dụng toàn bộ tinh chỉnh HUD và lưu Scene thành công: {activeScene.path}!");
    }
}
#endif

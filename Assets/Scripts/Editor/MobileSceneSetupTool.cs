#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MobileSceneSetupTool
{
    // Thang chữ duy nhất cho UI mobile. Chỉ dùng các nấc này để UI vẫn đồng đều khi thêm màn hình mới.
    private const int FontHud = 24;
    private const int FontBody = 26;
    private const int FontButton = 26;
    private const int FontTitle = 32;

    [MenuItem("TuTien/Mobile Refactor/1. Setup Mobile Canvas & Work Scene")]
    public static void SetupMobileWorkScene()
    {
        string originalPath = "Assets/Scenes/SampleScene.unity";
        string workPath = "Assets/Scenes/SampleScene_MobileWork.unity";

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(workPath) != null)
        {
            Debug.Log("[MobileSetup] File SampleScene_MobileWork.unity da ton tai hoac duoc tao truoc do.");
        }
        else
        {
            if (!AssetDatabase.CopyAsset(originalPath, workPath))
            {
                Debug.LogError("[MobileSetup] Không thể tạo bản sao scene.");
                return;
            }
            AssetDatabase.Refresh();
            Debug.Log("[MobileSetup] Da tao ban sao an toan: SampleScene_MobileWork.unity");
        }

        if (!System.IO.File.Exists(workPath))
        {
            Debug.LogError("[MobileSetup] Không tìm thấy scene nguồn hoặc scene làm việc.");
            return;
        }
        if (EditorSceneManager.GetActiveScene().path != workPath)
            EditorSceneManager.OpenScene(workPath);

        RemoveSplashFromOpenScene();

        Canvas canvas = null;
        foreach (var candidate in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (candidate.name == "Canvas" && candidate.transform.Find("RightPanel") != null)
            {
                canvas = candidate;
                break;
            }
        }
        if (canvas != null)
        {
            foreach (var candidate in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (candidate == canvas) continue;
                Transform misplacedRoot = candidate.transform.Find("MobileUIRoot");
                if (misplacedRoot != null) Object.DestroyImmediate(misplacedRoot.gameObject);
            }
            Undo.RecordObject(canvas.gameObject, "Setup Mobile Canvas");
            UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f;
            }

            RectTransform mobileRoot = canvas.transform.Find("MobileUIRoot") as RectTransform;
            if (mobileRoot == null)
            {
                GameObject rootGO = new GameObject("MobileUIRoot", typeof(RectTransform));
                rootGO.transform.SetParent(canvas.transform, false);
                RectTransform rt = rootGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                mobileRoot = rt;
            }

            if (mobileRoot.GetComponent<SafeAreaFitter>() == null)
                mobileRoot.gameObject.AddComponent<SafeAreaFitter>();
            BuildMobileShell(mobileRoot);
            Transform rightPanel = canvas.transform.Find("RightPanel");
            Transform mergePanel = rightPanel != null ? rightPanel.Find("MergePanel") : null;
            Transform swordDrawer = mobileRoot.Find("Drawer_0");
            if (mergePanel != null && swordDrawer != null)
            {
                mergePanel.SetParent(swordDrawer, false);
            }
            if (swordDrawer != null) ConfigureSwordDrawer(swordDrawer as RectTransform);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
                if (mainCamera.GetComponent<DynamicCameraFrustum>() == null)
                    mainCamera.gameObject.AddComponent<DynamicCameraFrustum>();
            }

            // Tái cấu trúc toàn bộ HUD cũ, chống đè chéo trên Mobile Portrait 9:16
            OrganizeMobileHUD(canvas, mobileRoot);
            OrganizeCombatFeedback(canvas, mobileRoot);
            ApplyDefaultTypography(canvas);

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[MobileSetup] Canvas 1080x1920 va MobileUIRoot da san sang! UI HUD da duoc to chuc gon gang.");
        }
        else Debug.LogError("[MobileSetup] Không tìm thấy Canvas gameplay chứa RightPanel.");
    }

    [MenuItem("TuTien/Mobile Refactor/2. Remove Splash From Gameplay Scenes")]
    public static void RemoveSplashFromGameplayScenes()
    {
        EditorSceneManager.SaveOpenScenes();
        string currentPath = EditorSceneManager.GetActiveScene().path;
        string[] paths = { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/SampleScene_MobileWork.unity" };
        foreach (string path in paths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            if (EditorSceneManager.GetActiveScene().path != path)
                EditorSceneManager.OpenScene(path);
            RemoveSplashFromOpenScene();
            Camera mainCamera = Camera.main;
            if (mainCamera != null) mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
            GameObject canvas = GameObject.Find("Canvas");
            Transform legacyBackground = canvas != null ? canvas.transform.Find("FullBackground") : null;
            if (legacyBackground != null) legacyBackground.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }
        if (!string.IsNullOrEmpty(currentPath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(currentPath) != null)
            EditorSceneManager.OpenScene(currentPath);
    }

    [MenuItem("TuTien/Mobile Refactor/3. Re-organize HUD in Open Scene")]
    public static void ReorganizeHUDInOpenScene()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[MobileSetup] Không tìm thấy Canvas trong Scene hiện tại!");
            return;
        }

        RectTransform mobileRoot = canvas.transform.Find("MobileUIRoot") as RectTransform;
        if (mobileRoot == null)
        {
            Debug.LogError("[MobileSetup] Không tìm thấy MobileUIRoot! Hãy chạy Setup Mobile Canvas (Menu 1) trước.");
            return;
        }

        Undo.RecordObject(canvas.gameObject, "Reorganize Mobile HUD");
        OrganizeMobileHUD(canvas, mobileRoot);
        OrganizeCombatFeedback(canvas, mobileRoot);
        ApplyDefaultTypography(canvas);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[MobileSetup] Đã sắp xếp lại toàn bộ HUD gọn gàng trong scene: {EditorSceneManager.GetActiveScene().name}!");
    }

    private static void RemoveSplashFromOpenScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "SplashScreenCanvas")
            {
                Undo.DestroyObjectImmediate(root);
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log($"[MobileSetup] Removed SplashScreenCanvas from {scene.path}");
                break;
            }
        }
    }

    private static void BuildSwordSlots(Transform mergePanel)
    {
        Transform equipPanel = mergePanel.Find("EquipPanel");
        if (equipPanel == null || equipPanel.childCount == 0) return;
        GameObject template = equipPanel.GetChild(0).gameObject;
        for (int i = equipPanel.childCount; i < SwordLoadoutService.MAX_SWORD_CAPACITY; i++)
        {
            GameObject slot = Object.Instantiate(template, equipPanel);
            slot.name = "EquipSlot_" + (i + 1);
            foreach (var item in slot.GetComponentsInChildren<MergeItem>())
                Object.DestroyImmediate(item.gameObject);
        }
        RectTransform equipRect = equipPanel as RectTransform;
        equipRect.anchorMin = new Vector2(0f, 0.5f);
        equipRect.anchorMax = Vector2.one;
        equipRect.offsetMin = Vector2.zero;
        equipRect.offsetMax = Vector2.zero;
        var grid = equipPanel.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(135f, 135f);
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(12, 12, 12, 12);
        }
        foreach (Transform slot in equipPanel)
        {
            if (slot.GetComponent<SwordSlotLockUI>() == null)
                slot.gameObject.AddComponent<SwordSlotLockUI>();
        }
    }

    private static void ConfigureSwordDrawer(RectTransform drawer)
    {
        RectTransform outerScroll = drawer.Find("ScrollView") as RectTransform;
        if (outerScroll == null) outerScroll = drawer.Find("MergePanel/InventoryContainer/ScrollView") as RectTransform;
        if (outerScroll == null) return;
        RectTransform outerContent = outerScroll.Find("Viewport/Content") as RectTransform;
        Transform merge = drawer.Find("MergePanel");
        if (merge == null && outerContent != null) merge = outerContent.Find("MergePanel");
        if (merge == null) return;

        // Chỉ di chuyển object hiện hữu: MergeManager vẫn giữ đúng tham chiếu trang bị/kho.
        merge.SetParent(drawer, false);
        RectTransform mergeRect = merge as RectTransform;
        mergeRect.anchorMin = new Vector2(0.03f, 0.03f);
        mergeRect.anchorMax = new Vector2(0.97f, 0.90f);
        mergeRect.offsetMin = Vector2.zero;
        mergeRect.offsetMax = Vector2.zero;
        BuildSwordSlots(merge);

        RectTransform equip = merge.Find("EquipPanel") as RectTransform;
        RectTransform inventory = merge.Find("InventoryContainer") as RectTransform;
        if (equip == null || inventory == null) return;
        equip.anchorMin = new Vector2(0f, 0.58f);
        equip.anchorMax = Vector2.one;
        equip.offsetMin = Vector2.zero;
        equip.offsetMax = Vector2.zero;
        inventory.anchorMin = Vector2.zero;
        inventory.anchorMax = new Vector2(1f, 0.57f);
        inventory.offsetMin = Vector2.zero;
        inventory.offsetMax = Vector2.zero;

        RectTransform pagesContainer = inventory.Find("PagesContainer") as RectTransform;
        if (pagesContainer == null) pagesContainer = outerContent.Find("PagesContainer") as RectTransform;
        RectTransform pagination = inventory.Find("PaginationPanel") as RectTransform;
        if (pagesContainer == null || pagination == null) return;
        pagination.anchorMin = Vector2.zero;
        pagination.anchorMax = new Vector2(1f, 0.14f);
        pagination.offsetMin = Vector2.zero;
        pagination.offsetMax = Vector2.zero;

        // Tái sử dụng ScrollView cũ, đặt nó đúng quanh lưới kho. Trang bị và nút phân trang
        // nằm ngoài ScrollRect nên không di chuyển khi kéo.
        if (outerScroll.parent != inventory) outerScroll.SetParent(inventory, false);
        outerScroll.anchorMin = new Vector2(0f, 0.15f);
        outerScroll.anchorMax = Vector2.one;
        outerScroll.offsetMin = Vector2.zero;
        outerScroll.offsetMax = Vector2.zero;
        if (pagesContainer.parent != outerContent) pagesContainer.SetParent(outerContent, false);
        pagesContainer.anchorMin = Vector2.zero;
        pagesContainer.anchorMax = Vector2.one;
        pagesContainer.offsetMin = Vector2.zero;
        pagesContainer.offsetMax = Vector2.zero;

        var pages = new System.Collections.Generic.List<RectTransform>();
        foreach (Transform child in pagesContainer)
            if (child.name.StartsWith("Page_")) pages.Add(child as RectTransform);
        pages.Sort((a, b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));
        if (pages.Count < 5) return;

        // Dồn dữ liệu trang 6..20 vào 5 trang đầu, giữ nguyên toàn bộ ô và kiếm trong ô.
        if (pages.Count > 5)
        {
            for (int p = 5; p < pages.Count; p++)
            {
                RectTransform target = pages[p % 5];
                while (pages[p].childCount > 0)
                    pages[p].GetChild(0).SetParent(target, false);
                Object.DestroyImmediate(pages[p].gameObject);
            }
            pages.RemoveRange(5, pages.Count - 5);
        }

        for (int i = 0; i < 5; i++)
        {
            RectTransform page = pages[i];
            page.anchorMin = Vector2.zero;
            page.anchorMax = Vector2.one;
            page.offsetMin = Vector2.zero;
            page.offsetMax = Vector2.zero;
            page.gameObject.SetActive(i == 0);
            var grid = page.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (grid == null) continue;
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(135f, 135f);
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(12, 12, 12, 12);
        }
        int maxSlots = 0;
        foreach (RectTransform page in pages) maxSlots = Mathf.Max(maxSlots, page.childCount);
        outerContent.sizeDelta = new Vector2(0f, Mathf.CeilToInt(maxSlots / 5f) * 147f + 30f);

        MergeManager manager = Object.FindAnyObjectByType<MergeManager>();
        if (manager != null)
        {
            manager.inventoryPages = pages.ConvertAll(p => (Transform)p).ToArray();
            if (manager.pageText != null) manager.pageText.text = "1/5";
            EditorUtility.SetDirty(manager);
        }
    }

    private static void BuildMobileShell(RectTransform root)
    {
        string[] names = { "Kiếm Lô", "Luyện Thể", "Luyện Đan", "Pháp Khí", "Trận Pháp" };
        var controller = root.GetComponent<MobileDrawerUIController>();
        if (controller == null) controller = root.gameObject.AddComponent<MobileDrawerUIController>();

        RectTransform backdrop = CreateRect(root, "MobileBackdrop", Vector2.zero, Vector2.one);
        var backdropImage = GetOrAdd<UnityEngine.UI.Image>(backdrop.gameObject);
        backdropImage.color = new Color(0f, 0f, 0f, 0.35f);
        GetOrAdd<UnityEngine.UI.Button>(backdrop.gameObject);
        controller.backdropOverlay = backdrop.gameObject;

        var panels = new RectTransform[names.Length];
        var dockButtons = new UnityEngine.UI.Button[names.Length];
        var closeButtons = new UnityEngine.UI.Button[names.Length];
        Font font = GetDefaultFont();

        for (int i = 0; i < names.Length; i++)
        {
            float topAnchor = 0.58f;
            if (i == 0) topAnchor = 0.66f;
            else if (i == 1) topAnchor = 0.74f;
            else if (i == 2) topAnchor = 0.76f;
            else if (i == 3) topAnchor = 0.76f;
            else if (i == 4) topAnchor = 0.76f;
            panels[i] = CreateRect(root, "Drawer_" + i, new Vector2(0f, 0.10f), new Vector2(1f, topAnchor));
            GetOrAdd<UnityEngine.UI.Image>(panels[i].gameObject).color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            string titleText = names[i];
            if (i == 1) titleText = "THIÊN PHÚ THỂ TU";
            else if (i == 2) titleText = "LUYỆN ĐAN CÁC";
            else if (i == 3) titleText = "HỘ THÂN PHÁP KHÍ";
            else if (i == 4) titleText = "TRẬN PHÁP CÁC";
            CreateLabel(panels[i], "Title", titleText, font, new Vector2(0.04f, 0.93f), new Vector2(0.75f, 0.99f));
            SetLabelStyle(panels[i].Find("Title"), FontTitle, TextAnchor.MiddleLeft);
            RectTransform close = CreateRect(panels[i], "CloseButton", new Vector2(0.85f, 0.93f), new Vector2(0.97f, 0.99f));
            GetOrAdd<UnityEngine.UI.Image>(close.gameObject).color = new Color(0.23f, 0.29f, 0.37f, 1f);
            closeButtons[i] = GetOrAdd<UnityEngine.UI.Button>(close.gameObject);
            CreateLabel(close, "Label", "✕", font, Vector2.zero, Vector2.one);
            SetLabelStyle(close.Find("Label"), FontHud, TextAnchor.MiddleCenter);
            RectTransform handle = CreateRect(panels[i], "SwipeHandle", new Vector2(0.42f, 0.975f), new Vector2(0.58f, 0.992f));
            GetOrAdd<UnityEngine.UI.Image>(handle.gameObject).color = new Color(0.65f, 0.7f, 0.78f, 0.55f);
            GetOrAdd<MobileDrawerSwipeHandle>(handle.gameObject);
            // Chỉ Kiếm Lô có lưới đủ dài để cuộn. Các thao tác ngắn luôn nằm cố định.
            RectTransform content = i == 0 && panels[i].Find("MergePanel/InventoryContainer/ScrollView") == null
                ? EnsureDrawerScroll(panels[i]) : RestoreStaticDrawer(panels[i]);
            if (i == 1) BuildBodyTemperingPanel(content, font);
            else if (i == 2) BuildAlchemyPanel(content, font);
            else if (i == 3) BuildArtifactPanel(content, font);
            else if (i == 4) BuildFormationPanel(content, font);
        }

        // MergePanel đã được chuyển vào ScrollView sau lượt dựng đầu tiên; vẫn phải cập nhật ô khóa
        // khi chạy lại tool, không chỉ khi nó còn ở RightPanel cũ.
        // Kiếm Lô được cấu hình sau BuildMobileShell để giữ trang bị và phân trang cố định.

        RectTransform dock = CreateRect(root, "MobileDock", Vector2.zero, new Vector2(1f, 0.10f));
        GetOrAdd<UnityEngine.UI.Image>(dock.gameObject).color = new Color(0.04f, 0.07f, 0.12f, 0.98f);
        for (int i = 0; i < names.Length; i++)
        {
            RectTransform cell = CreateRect(dock, "Dock_" + i, new Vector2(i / 5f, 0f), new Vector2((i + 1) / 5f, 1f));
            GetOrAdd<UnityEngine.UI.Image>(cell.gameObject).color = new Color(0.12f, 0.19f, 0.27f, 1f);
            dockButtons[i] = GetOrAdd<UnityEngine.UI.Button>(cell.gameObject);
            CreateLabel(cell, "Label", names[i], font, Vector2.zero, Vector2.one);
            SetLabelStyle(cell.Find("Label"), FontButton, TextAnchor.MiddleCenter);
        }

        controller.drawerPanels = panels;
        controller.dockButtons = dockButtons;
        controller.closeButtons = closeButtons;
        backdrop.gameObject.SetActive(false);
        foreach (var panel in panels)
        {
            panel.anchoredPosition = new Vector2(0f, -900f);
            panel.gameObject.SetActive(false);
        }
        EditorUtility.SetDirty(controller);
    }

    private static void BuildBodyTemperingPanel(RectTransform panel, Font font)
    {
        // 1. Dọn sạch cấu trúc cũ nếu có
        for (int r = 0; r < 6; r++)
        {
            Transform oldRow = panel.Find("StatRow_" + r);
            if (oldRow != null) Object.DestroyImmediate(oldRow.gameObject);
        }
        Transform oldInsp = panel.Find("InspectorBox");
        if (oldInsp != null) Object.DestroyImmediate(oldInsp.gameObject);
        Transform oldTree = panel.Find("TreeArea");
        if (oldTree != null) Object.DestroyImmediate(oldTree.gameObject);

        var ui = GetOrAdd<BodyTemperingPanelUI>(panel.gameObject);

        // Subtitle
        RectTransform subTitle = CreateRect(panel, "SubTitle", new Vector2(0.04f, 0.88f), new Vector2(0.70f, 0.925f));
        var subLabel = GetOrAdd<UnityEngine.UI.Text>(subTitle.gameObject);
        subLabel.text = "Khí Thể Song Tu • Đúc Vạn Kiếp Bất Diệt Thể";
        subLabel.font = font;
        subLabel.fontSize = 14;
        subLabel.color = new Color(0.68f, 0.75f, 0.85f, 0.85f);
        subLabel.alignment = TextAnchor.MiddleLeft;

        // Resource Bar
        RectTransform resBar = CreateRect(panel, "ResourceBar", new Vector2(0.04f, 0.825f), new Vector2(0.96f, 0.875f));
        GetOrAdd<UnityEngine.UI.Image>(resBar.gameObject).color = new Color(0.05f, 0.08f, 0.13f, 0.92f);
        
        RectTransform lbBalance = CreateRect(resBar, "TxtLinhThach", new Vector2(0.03f, 0f), new Vector2(0.50f, 1f));
        var txtBalance = GetOrAdd<UnityEngine.UI.Text>(lbBalance.gameObject);
        txtBalance.text = "Linh Thạch: 0 💎";
        txtBalance.font = font;
        txtBalance.fontSize = 15;
        txtBalance.fontStyle = FontStyle.Bold;
        txtBalance.color = new Color(1f, 0.88f, 0.35f, 1f);
        txtBalance.alignment = TextAnchor.MiddleLeft;
        ui.txtLinhThachBalance = txtBalance;

        RectTransform lbTotal = CreateRect(resBar, "TxtTotalLevel", new Vector2(0.50f, 0f), new Vector2(0.97f, 1f));
        var txtTotal = GetOrAdd<UnityEngine.UI.Text>(lbTotal.gameObject);
        txtTotal.text = "Tổng Thể Cấp: 0";
        txtTotal.font = font;
        txtTotal.fontSize = 15;
        txtTotal.color = new Color(0.2f, 0.85f, 0.95f, 1f);
        txtTotal.alignment = TextAnchor.MiddleRight;
        ui.txtTotalBodyLevel = txtTotal;

        // 2. SCROLLABLE TREE VIEW (Cuộn dọc thoáng đãng, không bị chật)
        RectTransform scroll = CreateRect(panel, "TreeScrollView", new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.81f));
        var scrollImg = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject);
        scrollImg.color = new Color(0.03f, 0.05f, 0.09f, 0.20f);
        scrollImg.raycastTarget = true;

        var scrollRect = GetOrAdd<UnityEngine.UI.ScrollRect>(scroll.gameObject);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 40f;
        ui.treeScrollRect = scrollRect;

        RectTransform viewport = CreateRect(scroll, "Viewport", Vector2.zero, Vector2.one);
        GetOrAdd<UnityEngine.UI.RectMask2D>(viewport.gameObject);
        scrollRect.viewport = viewport;

        RectTransform content = CreateRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 1020f);
        scrollRect.content = content;
        ui.treeContent = content;

        // Branch Headers Trong Content
        RectTransform hLeft = CreateRect(content, "Header_Left", new Vector2(0.01f, 1f), new Vector2(0.49f, 1f));
        hLeft.pivot = new Vector2(0.5f, 1f);
        hLeft.anchoredPosition = new Vector2(0f, -8f);
        hLeft.sizeDelta = new Vector2(0f, 36f);
        GetOrAdd<UnityEngine.UI.Image>(hLeft.gameObject).color = new Color(0.07f, 0.22f, 0.28f, 0.95f);
        CreateLabel(hLeft, "Txt", "🛡️ CƯƠNG THÂN (PHÒNG NGỰ)", font, Vector2.zero, Vector2.one);
        SetLabelStyle(hLeft.Find("Txt"), 14, TextAnchor.MiddleCenter);
        var tLeft = hLeft.Find("Txt").GetComponent<UnityEngine.UI.Text>();
        if (tLeft != null) { tLeft.color = new Color(0.3f, 0.9f, 1f, 1f); tLeft.fontStyle = FontStyle.Bold; }

        RectTransform hRight = CreateRect(content, "Header_Right", new Vector2(0.51f, 1f), new Vector2(0.99f, 1f));
        hRight.pivot = new Vector2(0.5f, 1f);
        hRight.anchoredPosition = new Vector2(0f, -8f);
        hRight.sizeDelta = new Vector2(0f, 36f);
        GetOrAdd<UnityEngine.UI.Image>(hRight.gameObject).color = new Color(0.28f, 0.12f, 0.10f, 0.95f);
        CreateLabel(hRight, "Txt", "⚔️ THẦN LỰC (TẤN CÔNG)", font, Vector2.zero, Vector2.one);
        SetLabelStyle(hRight.Find("Txt"), 14, TextAnchor.MiddleCenter);
        var tRight = hRight.Find("Txt").GetComponent<UnityEngine.UI.Text>();
        if (tRight != null) { tRight.color = new Color(1f, 0.65f, 0.5f, 1f); tRight.fontStyle = FontStyle.Bold; }

        // Node Cards configuration:
        // Index 0: Khí Huyết (T1 Left)
        // Index 1: Cương Khí (T3 Left)
        // Index 2: Dũ Hợp (T2 Left)
        // Index 3: Kình Lực (T1 Right)
        // Index 4: Phệ Huyết (T2 Right)
        // Index 5: Định Tâm (T3 Right)
        float cardHeight = 240f;
        float gapY = 65f;
        float startY = -55f;

        // Y offsets for Tier 1, Tier 2, Tier 3
        float yTier1 = startY;
        float yTier2 = yTier1 - cardHeight - gapY;
        float yTier3 = yTier2 - cardHeight - gapY;

        float[] nodeYPositions = new float[6]
        {
            yTier1, // 0: Khí Huyết (T1 Left)
            yTier3, // 1: Cương Khí (T3 Left)
            yTier2, // 2: Dũ Hợp (T2 Left)
            yTier1, // 3: Kình Lực (T1 Right)
            yTier2, // 4: Phệ Huyết (T2 Right)
            yTier3  // 5: Định Tâm (T3 Right)
        };
        bool[] isRightCol = new bool[6] { false, false, false, true, true, true };

        ui.nodeCardTransforms = new RectTransform[6];
        ui.nodeBackgrounds = new UnityEngine.UI.Image[6];
        ui.nodeOutlines = new UnityEngine.UI.Image[6];
        ui.nodeNameTexts = new UnityEngine.UI.Text[6];
        ui.nodeLevelTexts = new UnityEngine.UI.Text[6];
        ui.nodeEffectTexts = new UnityEngine.UI.Text[6];
        ui.nodeCostTexts = new UnityEngine.UI.Text[6];
        ui.nodeSelectButtons = new UnityEngine.UI.Button[6];
        ui.nodeQuickUpgradeButtons = new UnityEngine.UI.Button[6];
        ui.nodeLockOverlays = new GameObject[6];
        ui.nodeLockRequirementTexts = new UnityEngine.UI.Text[6];

        for (int i = 0; i < 6; i++)
        {
            float minX = isRightCol[i] ? 0.51f : 0.01f;
            float maxX = isRightCol[i] ? 0.99f : 0.49f;

            RectTransform card = CreateRect(content, "NodeCard_" + i, new Vector2(minX, 1f), new Vector2(maxX, 1f));
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0f, nodeYPositions[i]);
            card.sizeDelta = new Vector2(0f, cardHeight);
            ui.nodeCardTransforms[i] = card;

            var cardBg = GetOrAdd<UnityEngine.UI.Image>(card.gameObject);
            cardBg.color = new Color(0.09f, 0.14f, 0.22f, 0.95f);
            ui.nodeBackgrounds[i] = cardBg;

            var cardBtn = GetOrAdd<UnityEngine.UI.Button>(card.gameObject);
            ui.nodeSelectButtons[i] = cardBtn;

            var outlineComp = GetOrAdd<UnityEngine.UI.Outline>(card.gameObject);
            outlineComp.effectColor = new Color(0.18f, 0.65f, 0.85f, 0.70f);
            outlineComp.effectDistance = new Vector2(2f, -2f);

            RectTransform outlineRect = CreateRect(card, "OutlineBorder", Vector2.zero, Vector2.one);
            var outlineImg = GetOrAdd<UnityEngine.UI.Image>(outlineRect.gameObject);
            outlineImg.color = new Color(0.18f, 0.65f, 0.85f, 0.65f);
            outlineImg.raycastTarget = false;
            ui.nodeOutlines[i] = outlineImg;

            // Node Name & Icon
            RectTransform rName = CreateRect(card, "TxtName", new Vector2(0.05f, 0.76f), new Vector2(0.68f, 0.96f));
            var tName = GetOrAdd<UnityEngine.UI.Text>(rName.gameObject);
            tName.font = font;
            tName.fontSize = 17;
            tName.fontStyle = FontStyle.Bold;
            tName.color = Color.white;
            tName.alignment = TextAnchor.MiddleLeft;
            tName.raycastTarget = false;
            ui.nodeNameTexts[i] = tName;

            // Node Level
            RectTransform rLv = CreateRect(card, "TxtLevel", new Vector2(0.68f, 0.76f), new Vector2(0.95f, 0.96f));
            var tLv = GetOrAdd<UnityEngine.UI.Text>(rLv.gameObject);
            tLv.font = font;
            tLv.fontSize = 15;
            tLv.fontStyle = FontStyle.Bold;
            tLv.color = new Color(1f, 0.88f, 0.35f, 1f);
            tLv.alignment = TextAnchor.MiddleRight;
            tLv.raycastTarget = false;
            ui.nodeLevelTexts[i] = tLv;

            // Node Effect
            RectTransform rEff = CreateRect(card, "TxtEffect", new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.74f));
            var tEff = GetOrAdd<UnityEngine.UI.Text>(rEff.gameObject);
            tEff.font = font;
            tEff.fontSize = 13;
            tEff.color = new Color(0.85f, 0.90f, 0.95f, 0.95f);
            tEff.alignment = TextAnchor.UpperLeft;
            tEff.supportRichText = true;
            tEff.raycastTarget = false;
            ui.nodeEffectTexts[i] = tEff;

            // Nút Chi Tiết / Cường Hóa (To, dễ bấm)
            RectTransform btnAction = CreateRect(card, "BtnAction", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.28f));
            var imgBtn = GetOrAdd<UnityEngine.UI.Image>(btnAction.gameObject);
            imgBtn.color = new Color(0.14f, 0.50f, 0.35f, 1f);
            var bAction = GetOrAdd<UnityEngine.UI.Button>(btnAction.gameObject);
            ui.nodeQuickUpgradeButtons[i] = bAction;

            CreateLabel(btnAction, "TxtCost", "CHI TIẾT & CƯỜNG HÓA", font, Vector2.zero, Vector2.one);
            var tCost = btnAction.Find("TxtCost").GetComponent<UnityEngine.UI.Text>();
            tCost.fontSize = 13;
            tCost.fontStyle = FontStyle.Bold;
            tCost.color = Color.white;
            tCost.alignment = TextAnchor.MiddleCenter;
            ui.nodeCostTexts[i] = tCost;

            // Lock Overlay
            RectTransform lockOver = CreateRect(card, "LockOverlay", Vector2.zero, Vector2.one);
            GetOrAdd<UnityEngine.UI.Image>(lockOver.gameObject).color = new Color(0.04f, 0.06f, 0.10f, 0.90f);
            CreateLabel(lockOver, "TxtReq", "🔒 Chưa Mở", font, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
            var tReq = lockOver.Find("TxtReq").GetComponent<UnityEngine.UI.Text>();
            tReq.fontSize = 13;
            tReq.color = new Color(1f, 0.55f, 0.55f, 1f);
            tReq.alignment = TextAnchor.MiddleCenter;
            ui.nodeLockOverlays[i] = lockOver.gameObject;
            ui.nodeLockRequirementTexts[i] = tReq;
            lockOver.gameObject.SetActive(false);
        }

        // Connector Lines Giữa Các Tier
        ui.connectorLines = new UnityEngine.UI.Image[4];
        float lineH = gapY;

        // Line 0: 0->2 (Khí Huyết -> Dũ Hợp)
        RectTransform line0 = CreateRect(content, "Line_0_2", new Vector2(0.245f, 1f), new Vector2(0.255f, 1f));
        line0.pivot = new Vector2(0.5f, 1f);
        line0.anchoredPosition = new Vector2(0f, yTier1 - cardHeight);
        line0.sizeDelta = new Vector2(0f, lineH);
        ui.connectorLines[0] = GetOrAdd<UnityEngine.UI.Image>(line0.gameObject);

        // Line 1: 2->1 (Dũ Hợp -> Cương Khí)
        RectTransform line1 = CreateRect(content, "Line_2_1", new Vector2(0.245f, 1f), new Vector2(0.255f, 1f));
        line1.pivot = new Vector2(0.5f, 1f);
        line1.anchoredPosition = new Vector2(0f, yTier2 - cardHeight);
        line1.sizeDelta = new Vector2(0f, lineH);
        ui.connectorLines[1] = GetOrAdd<UnityEngine.UI.Image>(line1.gameObject);

        // Line 2: 3->4 (Kình Lực -> Phệ Huyết)
        RectTransform line2 = CreateRect(content, "Line_3_4", new Vector2(0.745f, 1f), new Vector2(0.755f, 1f));
        line2.pivot = new Vector2(0.5f, 1f);
        line2.anchoredPosition = new Vector2(0f, yTier1 - cardHeight);
        line2.sizeDelta = new Vector2(0f, lineH);
        ui.connectorLines[2] = GetOrAdd<UnityEngine.UI.Image>(line2.gameObject);

        // Line 3: 4->5 (Phệ Huyết -> Định Tâm)
        RectTransform line3 = CreateRect(content, "Line_4_5", new Vector2(0.745f, 1f), new Vector2(0.755f, 1f));
        line3.pivot = new Vector2(0.5f, 1f);
        line3.anchoredPosition = new Vector2(0f, yTier2 - cardHeight);
        line3.sizeDelta = new Vector2(0f, lineH);
        ui.connectorLines[3] = GetOrAdd<UnityEngine.UI.Image>(line3.gameObject);

        // 3. CENTER SCREEN DETAIL MODAL (Nằm trực tiếp trên MobileUIRoot, hiển thị ở giữa màn hình)
        RectTransform mobileRoot = panel.parent as RectTransform;
        if (mobileRoot != null)
        {
            RectTransform modalRoot = CreateRect(mobileRoot, "BodyTalentDetailModal", Vector2.zero, Vector2.one);
            var rootImg = GetOrAdd<UnityEngine.UI.Image>(modalRoot.gameObject);
            rootImg.color = new Color(0f, 0f, 0f, 0.80f);
            rootImg.raycastTarget = true;
            ui.centerModalBackdropBtn = GetOrAdd<UnityEngine.UI.Button>(modalRoot.gameObject);
            ui.centerModalRoot = modalRoot.gameObject;

            // Modal Box ở GIỮA MÀN HÌNH (860 x 950)
            RectTransform boxRt = CreateRect(modalRoot, "ModalBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            boxRt.pivot = new Vector2(0.5f, 0.5f);
            boxRt.anchoredPosition = Vector2.zero;
            boxRt.sizeDelta = new Vector2(860f, 950f);
            ui.centerModalBox = boxRt;

            var boxBg = GetOrAdd<UnityEngine.UI.Image>(boxRt.gameObject);
            boxBg.color = new Color(0.07f, 0.10f, 0.16f, 0.98f);
            boxBg.raycastTarget = true;

            var boxOutline = GetOrAdd<UnityEngine.UI.Outline>(boxRt.gameObject);
            boxOutline.effectColor = new Color(0.98f, 0.82f, 0.28f, 0.95f);
            boxOutline.effectDistance = new Vector2(3f, -3f);

            // Nút Đóng [X] góc phải Modal Box (56x56)
            RectTransform closeRt = CreateRect(boxRt, "BtnClose", new Vector2(1f, 1f), new Vector2(1f, 1f));
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-15f, -15f);
            closeRt.sizeDelta = new Vector2(56f, 56f);
            var closeImg = GetOrAdd<UnityEngine.UI.Image>(closeRt.gameObject);
            closeImg.color = new Color(0.85f, 0.22f, 0.22f, 0.95f);
            closeImg.raycastTarget = true;
            ui.centerModalCloseBtn = GetOrAdd<UnityEngine.UI.Button>(closeRt.gameObject);

            CreateLabel(closeRt, "Txt", "✕", font, Vector2.zero, Vector2.one);
            var tClose = closeRt.Find("Txt").GetComponent<UnityEngine.UI.Text>();
            tClose.fontSize = 26;
            tClose.fontStyle = FontStyle.Bold;
            tClose.color = Color.white;
            tClose.alignment = TextAnchor.MiddleCenter;

            // Modal Title
            RectTransform mTitleRt = CreateRect(boxRt, "TxtModalTitle", new Vector2(0.06f, 0.86f), new Vector2(0.82f, 0.96f));
            var tMTitle = GetOrAdd<UnityEngine.UI.Text>(mTitleRt.gameObject);
            tMTitle.font = font;
            tMTitle.fontSize = 22;
            tMTitle.fontStyle = FontStyle.Bold;
            tMTitle.color = new Color(1f, 0.90f, 0.35f, 1f);
            tMTitle.alignment = TextAnchor.MiddleLeft;
            tMTitle.supportRichText = true;
            ui.txtModalTitle = tMTitle;

            // Modal Tier Subtitle
            RectTransform mTierRt = CreateRect(boxRt, "TxtModalTier", new Vector2(0.06f, 0.81f), new Vector2(0.82f, 0.86f));
            var tMTier = GetOrAdd<UnityEngine.UI.Text>(mTierRt.gameObject);
            tMTier.font = font;
            tMTier.fontSize = 15;
            tMTier.color = new Color(0.3f, 0.85f, 1f, 1f);
            tMTier.alignment = TextAnchor.MiddleLeft;
            tMTier.supportRichText = true;
            ui.txtModalTier = tMTier;

            // Modal Lore Box
            RectTransform mLoreRt = CreateRect(boxRt, "TxtModalLore", new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.80f));
            var tMLore = GetOrAdd<UnityEngine.UI.Text>(mLoreRt.gameObject);
            tMLore.font = font;
            tMLore.fontSize = 14;
            tMLore.fontStyle = FontStyle.Italic;
            tMLore.color = new Color(0.90f, 0.88f, 0.72f, 0.90f);
            tMLore.alignment = TextAnchor.MiddleLeft;
            tMLore.supportRichText = true;
            ui.txtModalLore = tMLore;

            // Card Cơ Chế Thực Chiến
            RectTransform mMechCard = CreateRect(boxRt, "MechCard", new Vector2(0.06f, 0.44f), new Vector2(0.94f, 0.71f));
            GetOrAdd<UnityEngine.UI.Image>(mMechCard.gameObject).color = new Color(0.05f, 0.08f, 0.13f, 0.90f);

            RectTransform mMechRt = CreateRect(mMechCard, "TxtModalMechanics", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
            var tMMech = GetOrAdd<UnityEngine.UI.Text>(mMechRt.gameObject);
            tMMech.font = font;
            tMMech.fontSize = 15;
            tMMech.color = new Color(0.90f, 0.94f, 0.98f, 1f);
            tMMech.alignment = TextAnchor.UpperLeft;
            tMMech.supportRichText = true;
            ui.txtModalMechanics = tMMech;

            // Card So Sánh Chỉ Số (Current vs Next)
            RectTransform mProgCard = CreateRect(boxRt, "ProgCard", new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.42f));
            GetOrAdd<UnityEngine.UI.Image>(mProgCard.gameObject).color = new Color(0.05f, 0.08f, 0.13f, 0.90f);

            RectTransform mCurRt = CreateRect(mProgCard, "TxtModalCurrent", new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.94f));
            var tMCur = GetOrAdd<UnityEngine.UI.Text>(mCurRt.gameObject);
            tMCur.font = font;
            tMCur.fontSize = 15;
            tMCur.color = Color.white;
            tMCur.alignment = TextAnchor.MiddleLeft;
            tMCur.supportRichText = true;
            ui.txtModalCurrent = tMCur;

            RectTransform mNextRt = CreateRect(mProgCard, "TxtModalNext", new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.48f));
            var tMNext = GetOrAdd<UnityEngine.UI.Text>(mNextRt.gameObject);
            tMNext.font = font;
            tMNext.fontSize = 15;
            tMNext.color = Color.white;
            tMNext.alignment = TextAnchor.MiddleLeft;
            tMNext.supportRichText = true;
            ui.txtModalNext = tMNext;

            // Warning Banner
            RectTransform mReqRt = CreateRect(boxRt, "TxtModalReq", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.21f));
            var tMReq = GetOrAdd<UnityEngine.UI.Text>(mReqRt.gameObject);
            tMReq.font = font;
            tMReq.fontSize = 14;
            tMReq.color = new Color(1f, 0.45f, 0.45f, 1f);
            tMReq.alignment = TextAnchor.MiddleCenter;
            ui.txtModalRequirement = tMReq;
            ui.goModalLockBanner = mReqRt.gameObject;
            mReqRt.gameObject.SetActive(false);

            // Action Buttons
            // Nút Cường Hóa (To, chiếm 60% ngang)
            RectTransform btnUpRt = CreateRect(boxRt, "BtnUpgrade", new Vector2(0.06f, 0.04f), new Vector2(0.64f, 0.14f));
            var imgBtnUp = GetOrAdd<UnityEngine.UI.Image>(btnUpRt.gameObject);
            imgBtnUp.color = new Color(0.12f, 0.52f, 0.36f, 1f);
            ui.btnModalUpgrade = GetOrAdd<UnityEngine.UI.Button>(btnUpRt.gameObject);

            CreateLabel(btnUpRt, "TxtCost", "CƯỜNG HÓA THỂ PHÁCH", font, Vector2.zero, Vector2.one);
            var tCostMain = btnUpRt.Find("TxtCost").GetComponent<UnityEngine.UI.Text>();
            tCostMain.fontSize = 16;
            tCostMain.fontStyle = FontStyle.Bold;
            tCostMain.color = Color.white;
            tCostMain.alignment = TextAnchor.MiddleCenter;
            ui.txtModalCost = tCostMain;

            // Nút Nâng x5
            RectTransform btn5Rt = CreateRect(boxRt, "BtnUpgrade5", new Vector2(0.66f, 0.04f), new Vector2(0.94f, 0.14f));
            var imgBtn5 = GetOrAdd<UnityEngine.UI.Image>(btn5Rt.gameObject);
            imgBtn5.color = new Color(0.18f, 0.35f, 0.52f, 1f);
            ui.btnModalUpgrade5 = GetOrAdd<UnityEngine.UI.Button>(btn5Rt.gameObject);

            CreateLabel(btn5Rt, "Label", "NÂNG x5", font, Vector2.zero, Vector2.one);
            var t5 = btn5Rt.Find("Label").GetComponent<UnityEngine.UI.Text>();
            t5.fontSize = 14;
            t5.fontStyle = FontStyle.Bold;
            t5.color = Color.white;
            t5.alignment = TextAnchor.MiddleCenter;

            modalRoot.gameObject.SetActive(false);
        }

        // Legacy arrays
        ui.statLabels = ui.nodeEffectTexts;
        ui.upgradeButtons = ui.nodeQuickUpgradeButtons;

        EditorUtility.SetDirty(ui);
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 min, Vector2 max)
    {
        Transform existing = parent.Find(name);
        RectTransform rect = existing as RectTransform;
        if (rect == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            rect = go.GetComponent<RectTransform>();
        }
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    // Mọi drawer có phần nội dung dài đều dùng chung cấu trúc ScrollRect/Viewport/Content.
    // Header và nút đóng cố định, chỉ phần Content nhận thao tác kéo dọc.
    private static RectTransform EnsureDrawerScroll(RectTransform panel)
    {
        RectTransform scroll = CreateRect(panel, "ScrollView", new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.80f));
        var scrollImage = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject);
        scrollImage.color = new Color(0.03f, 0.05f, 0.09f, 0.18f);
        scrollImage.raycastTarget = true;
        GetOrAdd<UnityEngine.UI.RectMask2D>(scroll.gameObject);
        var scrollRect = GetOrAdd<UnityEngine.UI.ScrollRect>(scroll.gameObject);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 36f;

        RectTransform viewport = CreateRect(scroll, "Viewport", Vector2.zero, Vector2.one);
        GetOrAdd<UnityEngine.UI.RectMask2D>(viewport.gameObject);
        RectTransform content = CreateRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 1320f);
        scrollRect.viewport = viewport;
        scrollRect.content = content;

        var toMove = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in panel)
        {
            if (child != scroll && child.name != "Title" && child.name != "CloseButton" && child.name != "SwipeHandle")
                toMove.Add(child);
        }
        foreach (Transform child in toMove)
            child.SetParent(content, false);

        return content;
    }

    private static RectTransform RestoreStaticDrawer(RectTransform panel)
    {
        Transform scroll = panel.Find("ScrollView");
        Transform content = scroll != null ? scroll.Find("Viewport/Content") : null;
        if (content != null)
        {
            // Giữ nguyên các UI component và callback đã có khi sửa scene từ bản cuộn cũ.
            while (content.childCount > 0)
                content.GetChild(0).SetParent(panel, false);
        }
        if (scroll != null) scroll.gameObject.SetActive(false);
        return panel;
    }

    private static void CreateLabel(Transform parent, string name, string value, Font font, Vector2 min, Vector2 max)
    {
        RectTransform rect = CreateRect(parent, name, min, max);
        var label = GetOrAdd<UnityEngine.UI.Text>(rect.gameObject);
        label.text = value;
        label.font = font;
        label.fontSize = FontBody;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private static void SetLabelStyle(Transform labelTransform, int fontSize, TextAnchor alignment)
    {
        if (labelTransform == null) return;
        var label = labelTransform.GetComponent<UnityEngine.UI.Text>();
        if (label == null) return;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.raycastTarget = false;
    }

    private static Font GetDefaultFont()
    {
        // LegacyRuntime.ttf là font mặc định hợp lệ của Unity 6 (Arial.ttf đã bị loại bỏ).
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    // Các UI cũ dùng nhiều font khác nhau. Chuẩn hóa ở một điểm giúp scene cũ và UI sinh sau này
    // luôn quay về font mặc định dễ đọc, không cần sửa từng prefab thủ công.
    private static void ApplyDefaultTypography(Canvas canvas)
    {
        Font font = GetDefaultFont();
        if (font == null) return;

        foreach (var text in canvas.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            text.font = font;
            text.resizeTextForBestFit = false;
            text.raycastTarget = false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DRAWER 2 — LUYỆN ĐAN
    // ─────────────────────────────────────────────────────────────────────────
    private static void BuildAlchemyPanel(RectTransform panel, Font font)
    {
        // 1. Dọn dẹp các đối tượng cũ
        string[] oldNames = {
            "HerbRow_0", "HerbRow_1", "HerbRow_2", "BrewTimerRow", "BtnBrewCultivation",
            "BtnBrewBreakthrough", "BtnUseDust", "DustRow", "PillInventory", "RecipeGuide",
            "HerbVault", "FurnaceCard", "RecipeContainer", "PillVault", "FooterGuide"
        };
        foreach (var old in oldNames)
        {
            Transform t = panel.Find(old);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        var ui = GetOrAdd<AlchemyPanelUI>(panel.gameObject);

        // 2. KHU VỰC 1: DƯỢC CÁC & CẤP ĐAN SƯ (y: 0.77 -> 0.91)
        RectTransform herbVault = CreateRect(panel, "HerbVault", new Vector2(0.03f, 0.77f), new Vector2(0.97f, 0.91f));
        GetOrAdd<UnityEngine.UI.Image>(herbVault.gameObject).color = new Color(0.07f, 0.11f, 0.17f, 0.95f);

        // Hàng trên: Cấp Đan Sư & Bụi Dược
        RectTransform rankRow = CreateRect(herbVault, "RankRow", new Vector2(0.04f, 0.54f), new Vector2(0.48f, 0.94f));
        var txtRank = GetOrAdd<UnityEngine.UI.Text>(rankRow.gameObject);
        txtRank.text = "🎖️ Đan Sư Cấp 1";
        txtRank.font = font;
        txtRank.fontSize = 15;
        txtRank.fontStyle = FontStyle.Bold;
        txtRank.color = new Color(1f, 0.85f, 0.30f, 1f);
        txtRank.alignment = TextAnchor.MiddleLeft;
        txtRank.raycastTarget = false;
        ui.txtAlchemistRank = txtRank;

        RectTransform dustRow = CreateRect(herbVault, "DustRow", new Vector2(0.52f, 0.54f), new Vector2(0.96f, 0.94f));
        var txtDust = GetOrAdd<UnityEngine.UI.Text>(dustRow.gameObject);
        txtDust.text = "💨 Bụi Dược: 0";
        txtDust.font = font;
        txtDust.fontSize = 15;
        txtDust.fontStyle = FontStyle.Bold;
        txtDust.color = new Color(0.85f, 0.70f, 1f, 1f);
        txtDust.alignment = TextAnchor.MiddleRight;
        txtDust.raycastTarget = false;
        ui.txtMedicineDust = txtDust;
        ui.dustLabel = txtDust;

        // Hàng dưới: 3 Thảo dược chính
        RectTransform badge0 = CreateRect(herbVault, "Badge_LinhChi", new Vector2(0.03f, 0.08f), new Vector2(0.33f, 0.48f));
        GetOrAdd<UnityEngine.UI.Image>(badge0.gameObject).color = new Color(0.12f, 0.18f, 0.25f, 0.95f);
        CreateLabel(badge0, "Label", "🌿 Linh Chi: 0", font, Vector2.zero, Vector2.one);
        var txtLinhChi = badge0.Find("Label").GetComponent<UnityEngine.UI.Text>();
        txtLinhChi.fontSize = 12;
        txtLinhChi.fontStyle = FontStyle.Bold;
        txtLinhChi.color = new Color(0.33f, 0.94f, 0.77f, 1f);
        ui.txtHerbLinhChi = txtLinhChi;
        ui.linhChiLabel = txtLinhChi;

        RectTransform badge1 = CreateRect(herbVault, "Badge_NgungKhi", new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.48f));
        GetOrAdd<UnityEngine.UI.Image>(badge1.gameObject).color = new Color(0.12f, 0.18f, 0.25f, 0.95f);
        CreateLabel(badge1, "Label", "🌾 Ngưng Khí: 0", font, Vector2.zero, Vector2.one);
        var txtNgungKhi = badge1.Find("Label").GetComponent<UnityEngine.UI.Text>();
        txtNgungKhi.fontSize = 12;
        txtNgungKhi.fontStyle = FontStyle.Bold;
        txtNgungKhi.color = new Color(1f, 0.92f, 0.65f, 1f);
        ui.txtHerbNgungKhi = txtNgungKhi;
        ui.caoNgungKhiLabel = txtNgungKhi;

        RectTransform badge2 = CreateRect(herbVault, "Badge_TuyetLien", new Vector2(0.67f, 0.08f), new Vector2(0.97f, 0.48f));
        GetOrAdd<UnityEngine.UI.Image>(badge2.gameObject).color = new Color(0.12f, 0.18f, 0.25f, 0.95f);
        CreateLabel(badge2, "Label", "❄️ Tuyết Liên: 0", font, Vector2.zero, Vector2.one);
        var txtTuyetLien = badge2.Find("Label").GetComponent<UnityEngine.UI.Text>();
        txtTuyetLien.fontSize = 12;
        txtTuyetLien.fontStyle = FontStyle.Bold;
        txtTuyetLien.color = new Color(0.45f, 0.73f, 1f, 1f);
        ui.txtHerbTuyetLien = txtTuyetLien;
        ui.tuyetLienLabel = txtTuyetLien;

        // 3. KHU VỰC 2: LÒ LUYỆN TRUNG TÂM (y: 0.58 -> 0.755)
        RectTransform furnaceCard = CreateRect(panel, "FurnaceCard", new Vector2(0.03f, 0.58f), new Vector2(0.97f, 0.755f));
        GetOrAdd<UnityEngine.UI.Image>(furnaceCard.gameObject).color = new Color(0.10f, 0.14f, 0.21f, 0.98f);

        RectTransform rFurnaceTitle = CreateRect(furnaceCard, "TxtFurnaceStatus", new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.94f));
        var txtFurnaceStatus = GetOrAdd<UnityEngine.UI.Text>(rFurnaceTitle.gameObject);
        txtFurnaceStatus.text = "🔥 Lò Luyện Trống";
        txtFurnaceStatus.font = font;
        txtFurnaceStatus.fontSize = 17;
        txtFurnaceStatus.fontStyle = FontStyle.Bold;
        txtFurnaceStatus.color = new Color(1f, 0.83f, 0.35f, 1f);
        txtFurnaceStatus.alignment = TextAnchor.MiddleCenter;
        txtFurnaceStatus.raycastTarget = false;
        ui.txtFurnaceStatus = txtFurnaceStatus;

        RectTransform rFurnaceTimer = CreateRect(furnaceCard, "TxtFurnaceTimer", new Vector2(0.04f, 0.32f), new Vector2(0.96f, 0.60f));
        var txtFurnaceTimer = GetOrAdd<UnityEngine.UI.Text>(rFurnaceTimer.gameObject);
        txtFurnaceTimer.text = "Chọn bài thuốc bên dưới để bắt đầu luyện";
        txtFurnaceTimer.font = font;
        txtFurnaceTimer.fontSize = 13;
        txtFurnaceTimer.color = new Color(0.72f, 0.78f, 0.86f, 1f);
        txtFurnaceTimer.alignment = TextAnchor.MiddleCenter;
        txtFurnaceTimer.raycastTarget = false;
        ui.txtFurnaceTimer = txtFurnaceTimer;
        ui.brewTimerLabel = txtFurnaceTimer;

        // Nút hoàn tất nhanh bằng Bụi Dược
        RectTransform btnDustBox = CreateRect(furnaceCard, "BtnFinishWithDust", new Vector2(0.24f, 0.05f), new Vector2(0.76f, 0.28f));
        var imgBtnDust = GetOrAdd<UnityEngine.UI.Image>(btnDustBox.gameObject);
        imgBtnDust.color = new Color(0.38f, 0.22f, 0.52f, 1f);
        var bDust = GetOrAdd<UnityEngine.UI.Button>(btnDustBox.gameObject);
        CreateLabel(btnDustBox, "Label", "💨 Tốc Hành (10 Bụi Dược)", font, Vector2.zero, Vector2.one);
        var txtDustBtn = btnDustBox.Find("Label").GetComponent<UnityEngine.UI.Text>();
        txtDustBtn.fontSize = 12;
        txtDustBtn.fontStyle = FontStyle.Bold;
        txtDustBtn.color = Color.white;
        bDust.onClick.RemoveAllListeners();
        bDust.onClick.AddListener(ui.OnUseDustClicked);
        ui.btnFinishWithDust = bDust;
        ui.useDustBtn = bDust;

        // 4. KHU VỰC 3: HAI THẺ BÀI THUỐC (y: 0.29 -> 0.565)
        RectTransform recipesBox = CreateRect(panel, "RecipeContainer", new Vector2(0.03f, 0.29f), new Vector2(0.97f, 0.565f));

        // Thẻ 1: Tu Vi Đan (Trái)
        RectTransform cardTuVi = CreateRect(recipesBox, "Card_TuVi", new Vector2(0f, 0f), new Vector2(0.485f, 1f));
        GetOrAdd<UnityEngine.UI.Image>(cardTuVi.gameObject).color = new Color(0.08f, 0.15f, 0.20f, 0.98f);
        
        CreateLabel(cardTuVi, "Title", "💊 TU VI ĐAN", font, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));
        var tTuViTitle = cardTuVi.Find("Title").GetComponent<UnityEngine.UI.Text>();
        tTuViTitle.fontSize = 15;
        tTuViTitle.fontStyle = FontStyle.Bold;
        tTuViTitle.color = new Color(0.18f, 0.80f, 0.44f, 1f);

        CreateLabel(cardTuVi, "Desc", "Tăng mạnh tu vi tức thì", font, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.76f));
        var tTuViDesc = cardTuVi.Find("Desc").GetComponent<UnityEngine.UI.Text>();
        tTuViDesc.fontSize = 12;
        tTuViDesc.color = new Color(0.70f, 0.75f, 0.80f, 1f);

        CreateLabel(cardTuVi, "Cost", "Cần: 🌿x3  🌾x2", font, new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.56f));
        var tTuViCost = cardTuVi.Find("Cost").GetComponent<UnityEngine.UI.Text>();
        tTuViCost.fontSize = 12;
        tTuViCost.fontStyle = FontStyle.Bold;
        tTuViCost.color = new Color(0.33f, 0.94f, 0.77f, 1f);
        ui.txtCostTuVi = tTuViCost;

        RectTransform btnBrewTuViRect = CreateRect(cardTuVi, "BtnBrewTuVi", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.30f));
        GetOrAdd<UnityEngine.UI.Image>(btnBrewTuViRect.gameObject).color = new Color(0.12f, 0.48f, 0.32f, 1f);
        var bBrewTuVi = GetOrAdd<UnityEngine.UI.Button>(btnBrewTuViRect.gameObject);
        CreateLabel(btnBrewTuViRect, "Label", "LUYỆN ĐAN", font, Vector2.zero, Vector2.one);
        var tBrewTuViBtn = btnBrewTuViRect.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tBrewTuViBtn.fontSize = 13;
        tBrewTuViBtn.fontStyle = FontStyle.Bold;
        tBrewTuViBtn.color = Color.white;
        bBrewTuVi.onClick.RemoveAllListeners();
        bBrewTuVi.onClick.AddListener(ui.OnBrewCultivationClicked);
        ui.btnBrewTuVi = bBrewTuVi;
        ui.brewCultivationBtn = bBrewTuVi;

        // Thẻ 2: Đột Phá Đan (Phải)
        RectTransform cardBT = CreateRect(recipesBox, "Card_Breakthrough", new Vector2(0.515f, 0f), new Vector2(1f, 1f));
        GetOrAdd<UnityEngine.UI.Image>(cardBT.gameObject).color = new Color(0.18f, 0.14f, 0.10f, 0.98f);

        CreateLabel(cardBT, "Title", "⚡ ĐỘT PHÁ ĐAN", font, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));
        var tBTTitle = cardBT.Find("Title").GetComponent<UnityEngine.UI.Text>();
        tBTTitle.fontSize = 15;
        tBTTitle.fontStyle = FontStyle.Bold;
        tBTTitle.color = new Color(0.95f, 0.55f, 0.15f, 1f);

        CreateLabel(cardBT, "Desc", "Tăng xác suất độ kiếp", font, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.76f));
        var tBTDesc = cardBT.Find("Desc").GetComponent<UnityEngine.UI.Text>();
        tBTDesc.fontSize = 12;
        tBTDesc.color = new Color(0.70f, 0.75f, 0.80f, 1f);

        CreateLabel(cardBT, "Cost", "Cần: 🌿x5  🌾x3  ❄️x1", font, new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.56f));
        var tBTCost = cardBT.Find("Cost").GetComponent<UnityEngine.UI.Text>();
        tBTCost.fontSize = 12;
        tBTCost.fontStyle = FontStyle.Bold;
        tBTCost.color = new Color(1f, 0.92f, 0.65f, 1f);
        ui.txtCostBreakthrough = tBTCost;

        RectTransform btnBrewBTRect = CreateRect(cardBT, "BtnBrewBreakthrough", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.30f));
        GetOrAdd<UnityEngine.UI.Image>(btnBrewBTRect.gameObject).color = new Color(0.58f, 0.32f, 0.10f, 1f);
        var bBrewBT = GetOrAdd<UnityEngine.UI.Button>(btnBrewBTRect.gameObject);
        CreateLabel(btnBrewBTRect, "Label", "LUYỆN ĐAN", font, Vector2.zero, Vector2.one);
        var tBrewBTBtn = btnBrewBTRect.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tBrewBTBtn.fontSize = 13;
        tBrewBTBtn.fontStyle = FontStyle.Bold;
        tBrewBTBtn.color = Color.white;
        bBrewBT.onClick.RemoveAllListeners();
        bBrewBT.onClick.AddListener(ui.OnBrewBreakthroughClicked);
        ui.btnBrewBreakthrough = bBrewBT;
        ui.brewBreakthroughBtn = bBrewBT;

        // 5. KHU VỰC 4: ĐAN KHỐ & UỐNG ĐAN (y: 0.085 -> 0.275)
        RectTransform pillVault = CreateRect(panel, "PillVault", new Vector2(0.03f, 0.085f), new Vector2(0.97f, 0.275f));
        GetOrAdd<UnityEngine.UI.Image>(pillVault.gameObject).color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

        CreateLabel(pillVault, "VaultTitle", "📦 ĐAN KHỐ CẢNH GIỚI HIỆN TẠI", font, new Vector2(0.04f, 0.74f), new Vector2(0.96f, 0.96f));
        var tVaultTitle = pillVault.Find("VaultTitle").GetComponent<UnityEngine.UI.Text>();
        tVaultTitle.fontSize = 13;
        tVaultTitle.fontStyle = FontStyle.Bold;
        tVaultTitle.color = new Color(0.85f, 0.90f, 0.95f, 1f);
        tVaultTitle.alignment = TextAnchor.MiddleLeft;

        CreateLabel(pillVault, "TxtTuViSummary", "Tu Vi Đan: (Chưa có)", font, new Vector2(0.04f, 0.40f), new Vector2(0.66f, 0.72f));
        var tTuViSummary = pillVault.Find("TxtTuViSummary").GetComponent<UnityEngine.UI.Text>();
        tTuViSummary.fontSize = 12;
        tTuViSummary.alignment = TextAnchor.MiddleLeft;
        ui.txtTuViPillsSummary = tTuViSummary;

        CreateLabel(pillVault, "TxtBTSummary", "Đột Phá Đan: (Chưa có)", font, new Vector2(0.04f, 0.08f), new Vector2(0.66f, 0.40f));
        var tBTSummary = pillVault.Find("TxtBTSummary").GetComponent<UnityEngine.UI.Text>();
        tBTSummary.fontSize = 12;
        tBTSummary.alignment = TextAnchor.MiddleLeft;
        ui.txtBreakthroughPillsSummary = tBTSummary;
        ui.pillInventoryLabel = tTuViSummary;

        // Nút Uống Đan Tu Vi tức thì
        RectTransform btnDrink = CreateRect(pillVault, "BtnDrinkTuVi", new Vector2(0.68f, 0.16f), new Vector2(0.96f, 0.72f));
        GetOrAdd<UnityEngine.UI.Image>(btnDrink.gameObject).color = new Color(0.18f, 0.46f, 0.71f, 1f);
        var bDrink = GetOrAdd<UnityEngine.UI.Button>(btnDrink.gameObject);
        CreateLabel(btnDrink, "Label", "UỐNG ĐAN", font, Vector2.zero, Vector2.one);
        var tDrink = btnDrink.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tDrink.fontSize = 13;
        tDrink.fontStyle = FontStyle.Bold;
        tDrink.color = Color.white;
        bDrink.onClick.RemoveAllListeners();
        bDrink.onClick.AddListener(ui.OnUseTuViPillClicked);
        ui.btnUseTuViPill = bDrink;

        // 6. KHU VỰC 5: HƯỚNG DẪN DƯỚI CÙNG (y: 0.015 -> 0.075)
        RectTransform guide = CreateRect(panel, "FooterGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f));
        GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
        CreateLabel(guide, "Label", "⚗️ Bí quyết: Thu thập thảo dược từ quái & Bí Cảnh. Luyện đan thất bại hoàn lại Bụi Dược.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f));
        var tGuide = guide.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tGuide.fontSize = 11;
        tGuide.color = new Color(0.55f, 0.62f, 0.72f, 0.9f);

        EditorUtility.SetDirty(ui);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DRAWER 3 — PHÁP KHÍ (LUYỆN KHÍ ĐIỆN)
    // ─────────────────────────────────────────────────────────────────────────
    private static void BuildArtifactPanel(RectTransform panel, Font font)
    {
        // 1. Dọn dẹp cấu trúc cũ
        string[] oldNames = {
            "OreRow", "ArtRow_Robe", "ArtRow_Crown", "ArtRow_Boots", "ArtRow_Amulet",
            "ResourceBar", "ArtifactScrollView", "ArtifactGuide"
        };
        foreach (var old in oldNames)
        {
            Transform t = panel.Find(old);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        var ui = GetOrAdd<ArtifactPanelUI>(panel.gameObject);

        // 2. Resource Bar ở trên (y: 0.875 -> 0.925)
        RectTransform resBar = CreateRect(panel, "ResourceBar", new Vector2(0.03f, 0.875f), new Vector2(0.97f, 0.925f));
        GetOrAdd<UnityEngine.UI.Image>(resBar.gameObject).color = new Color(0.06f, 0.09f, 0.14f, 0.95f);

        RectTransform rOre = CreateRect(resBar, "TxtOre", new Vector2(0.04f, 0f), new Vector2(0.50f, 1f));
        var txtOre = GetOrAdd<UnityEngine.UI.Text>(rOre.gameObject);
        txtOre.font = font;
        txtOre.fontSize = 15;
        txtOre.fontStyle = FontStyle.Bold;
        txtOre.color = new Color(1f, 0.85f, 0.30f, 1f);
        txtOre.alignment = TextAnchor.MiddleLeft;
        txtOre.text = "⚙️ Quặng Phôi: 0";
        ui.oreLabel = txtOre;

        RectTransform rAura = CreateRect(resBar, "TxtAura", new Vector2(0.50f, 0f), new Vector2(0.96f, 1f));
        var txtAura = GetOrAdd<UnityEngine.UI.Text>(rAura.gameObject);
        txtAura.font = font;
        txtAura.fontSize = 15;
        txtAura.fontStyle = FontStyle.Bold;
        txtAura.color = new Color(0.45f, 0.85f, 1f, 1f);
        txtAura.alignment = TextAnchor.MiddleRight;
        txtAura.text = "✨ Hào Quang: Bậc 0";
        ui.auraLabel = txtAura;

        // 3. ScrollView danh sách Pháp Khí (y: 0.08f -> 0.865f)
        RectTransform scroll = CreateRect(panel, "ArtifactScrollView", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.865f));
        var scrollImg = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject);
        scrollImg.color = new Color(0.03f, 0.05f, 0.09f, 0.20f);
        scrollImg.raycastTarget = true;

        var scrollRect = GetOrAdd<UnityEngine.UI.ScrollRect>(scroll.gameObject);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 40f;

        RectTransform viewport = CreateRect(scroll, "Viewport", Vector2.zero, Vector2.one);
        GetOrAdd<UnityEngine.UI.RectMask2D>(viewport.gameObject);
        scrollRect.viewport = viewport;

        RectTransform content = CreateRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 800f);
        scrollRect.content = content;

        // 4 Cards
        ui.artifactLabels = new UnityEngine.UI.Text[4];
        ui.artifactStatsTexts = new UnityEngine.UI.Text[4];
        ui.artifactCostTexts = new UnityEngine.UI.Text[4];
        ui.upgradeButtons = new UnityEngine.UI.Button[4];

        float cardH = 175f;
        float spacing = 14f;
        for (int i = 0; i < 4; i++)
        {
            float yPos = -10f - i * (cardH + spacing);
            RectTransform card = CreateRect(content, "ArtCard_" + i, new Vector2(0.02f, 1f), new Vector2(0.98f, 1f));
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0f, yPos);
            card.sizeDelta = new Vector2(0f, cardH);
            GetOrAdd<UnityEngine.UI.Image>(card.gameObject).color = new Color(0.10f, 0.15f, 0.22f, 0.98f);

            // Row 1: Tên & Cấp (y: 0.68 -> 0.96)
            RectTransform rName = CreateRect(card, "TxtName", new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.96f));
            var tName = GetOrAdd<UnityEngine.UI.Text>(rName.gameObject);
            tName.font = font;
            tName.fontSize = 15;
            tName.fontStyle = FontStyle.Bold;
            tName.color = new Color(0.95f, 0.85f, 0.40f, 1f);
            tName.alignment = TextAnchor.MiddleLeft;
            tName.text = "Pháp Khí " + i;
            ui.artifactLabels[i] = tName;

            // Row 2: Stats (y: 0.30 -> 0.68)
            RectTransform rStats = CreateRect(card, "TxtStats", new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.68f));
            var tStats = GetOrAdd<UnityEngine.UI.Text>(rStats.gameObject);
            tStats.font = font;
            tStats.fontSize = 12;
            tStats.color = new Color(0.85f, 0.90f, 0.95f, 1f);
            tStats.alignment = TextAnchor.UpperLeft;
            tStats.supportRichText = true;
            ui.artifactStatsTexts[i] = tStats;

            // Row 3: Chi phí (y: 0.05 -> 0.28)
            RectTransform rCost = CreateRect(card, "TxtCost", new Vector2(0.04f, 0.05f), new Vector2(0.68f, 0.28f));
            var tCost = GetOrAdd<UnityEngine.UI.Text>(rCost.gameObject);
            tCost.font = font;
            tCost.fontSize = 12;
            tCost.fontStyle = FontStyle.Bold;
            tCost.color = new Color(0.33f, 0.94f, 0.77f, 1f);
            tCost.alignment = TextAnchor.MiddleLeft;
            tCost.supportRichText = true;
            ui.artifactCostTexts[i] = tCost;

            // Nút Cường Hóa (y: 0.05 -> 0.28, x: 0.70 -> 0.96)
            RectTransform btnUpg = CreateRect(card, "BtnUpgrade", new Vector2(0.70f, 0.05f), new Vector2(0.96f, 0.28f));
            GetOrAdd<UnityEngine.UI.Image>(btnUpg.gameObject).color = new Color(0.12f, 0.48f, 0.32f, 1f);
            var bUpg = GetOrAdd<UnityEngine.UI.Button>(btnUpg.gameObject);
            CreateLabel(btnUpg, "Label", "CƯỜNG HÓA", font, Vector2.zero, Vector2.one);
            var tBtn = btnUpg.Find("Label").GetComponent<UnityEngine.UI.Text>();
            tBtn.fontSize = 13;
            tBtn.fontStyle = FontStyle.Bold;
            tBtn.color = Color.white;
            int idx = i;
            bUpg.onClick.RemoveAllListeners();
            bUpg.onClick.AddListener(() => ui.OnUpgradeClicked(idx));
            ui.upgradeButtons[i] = bUpg;
        }

        // 4. Footer Tip
        RectTransform guide = CreateRect(panel, "ArtifactGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f));
        GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
        CreateLabel(guide, "Label", "💡 Bí quyết: Tiêu diệt Boss để thu thập Quặng Phôi. Cường hóa đều 4 món để kích hoạt Hào Quang.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f));
        var tGuide = guide.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tGuide.fontSize = 11;
        tGuide.color = new Color(0.55f, 0.62f, 0.72f, 0.9f);

        EditorUtility.SetDirty(ui);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DRAWER 4 — TRẬN PHÁP (TRẬN PHÁP CÁC)
    // ─────────────────────────────────────────────────────────────────────────
    private static void BuildFormationPanel(RectTransform panel, Font font)
    {
        // 1. Dọn dẹp cấu trúc cũ
        string[] oldNames = {
            "SpiritCooldownRow", "FormRow_Thunder", "FormRow_Spirit", "FormRow_Frost",
            "ResourceBar", "ActiveSpiritBanner", "FormationScrollView", "FormationGuide"
        };
        foreach (var old in oldNames)
        {
            Transform t = panel.Find(old);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        var ui = GetOrAdd<FormationPanelUI>(panel.gameObject);

        // 2. Resource & State Bar ở trên (y: 0.88f -> 0.93f)
        RectTransform resBar = CreateRect(panel, "ResourceBar", new Vector2(0.03f, 0.88f), new Vector2(0.97f, 0.93f));
        GetOrAdd<UnityEngine.UI.Image>(resBar.gameObject).color = new Color(0.06f, 0.09f, 0.14f, 0.95f);

        RectTransform rOre = CreateRect(resBar, "TxtOre", new Vector2(0.04f, 0f), new Vector2(0.48f, 1f));
        var txtOre = GetOrAdd<UnityEngine.UI.Text>(rOre.gameObject);
        txtOre.font = font;
        txtOre.fontSize = 15;
        txtOre.fontStyle = FontStyle.Bold;
        txtOre.color = new Color(1f, 0.85f, 0.30f, 1f);
        txtOre.alignment = TextAnchor.MiddleLeft;
        txtOre.text = "⚙️ Quặng Phôi: 0";
        ui.txtOreLabel = txtOre;

        RectTransform rActive = CreateRect(resBar, "TxtActive", new Vector2(0.50f, 0f), new Vector2(0.96f, 1f));
        var txtActive = GetOrAdd<UnityEngine.UI.Text>(rActive.gameObject);
        txtActive.font = font;
        txtActive.fontSize = 15;
        txtActive.fontStyle = FontStyle.Bold;
        txtActive.color = new Color(0.45f, 0.85f, 1f, 1f);
        txtActive.alignment = TextAnchor.MiddleRight;
        txtActive.text = "Đang Dùng: ⚡ Lôi Trận";
        ui.txtCurrentActiveLabel = txtActive;

        // 3. Active Spirit Banner (Tụ Linh Trận) (y: 0.81f -> 0.87f)
        RectTransform spiritBanner = CreateRect(panel, "ActiveSpiritBanner", new Vector2(0.03f, 0.81f), new Vector2(0.97f, 0.87f));
        GetOrAdd<UnityEngine.UI.Image>(spiritBanner.gameObject).color = new Color(0.12f, 0.18f, 0.28f, 0.98f);

        RectTransform rSpLabel = CreateRect(spiritBanner, "TxtSpiritStatus", new Vector2(0.04f, 0f), new Vector2(0.66f, 1f));
        var tSpLabel = GetOrAdd<UnityEngine.UI.Text>(rSpLabel.gameObject);
        tSpLabel.font = font;
        tSpLabel.fontSize = 12;
        tSpLabel.alignment = TextAnchor.MiddleLeft;
        tSpLabel.supportRichText = true;
        tSpLabel.text = "⚡ Tụ Linh Trận Sẵn Sàng Kích Hoạt!";
        ui.spiritCooldownLabel = tSpLabel;

        RectTransform btnActSpirit = CreateRect(spiritBanner, "BtnActivateSpirit", new Vector2(0.68f, 0.12f), new Vector2(0.96f, 0.88f));
        GetOrAdd<UnityEngine.UI.Image>(btnActSpirit.gameObject).color = new Color(0.45f, 0.22f, 0.65f, 1f);
        var bActSpirit = GetOrAdd<UnityEngine.UI.Button>(btnActSpirit.gameObject);
        CreateLabel(btnActSpirit, "Label", "🌀 KÍCH HOẠT", font, Vector2.zero, Vector2.one);
        var tActBtn = btnActSpirit.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tActBtn.fontSize = 12;
        tActBtn.fontStyle = FontStyle.Bold;
        tActBtn.color = Color.white;
        bActSpirit.onClick.RemoveAllListeners();
        bActSpirit.onClick.AddListener(ui.OnActivateSpirit);
        ui.activateSpiritBtn = bActSpirit;

        // 4. ScrollView 6 Đại Trận Pháp (y: 0.08f -> 0.80f)
        RectTransform scroll = CreateRect(panel, "FormationScrollView", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.80f));
        var scrollImg = GetOrAdd<UnityEngine.UI.Image>(scroll.gameObject);
        scrollImg.color = new Color(0.03f, 0.05f, 0.09f, 0.20f);
        scrollImg.raycastTarget = true;

        var scrollRect = GetOrAdd<UnityEngine.UI.ScrollRect>(scroll.gameObject);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.12f;
        scrollRect.scrollSensitivity = 40f;

        RectTransform viewport = CreateRect(scroll, "Viewport", Vector2.zero, Vector2.one);
        GetOrAdd<UnityEngine.UI.RectMask2D>(viewport.gameObject);
        scrollRect.viewport = viewport;

        RectTransform content = CreateRect(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 1380f);
        scrollRect.content = content;

        // 6 Cards
        ui.formationLabels = new UnityEngine.UI.Text[6];
        ui.formationDescTexts = new UnityEngine.UI.Text[6];
        ui.formationCostTexts = new UnityEngine.UI.Text[6];
        ui.selectButtons = new UnityEngine.UI.Button[6];
        ui.upgradeButtons = new UnityEngine.UI.Button[6];
        ui.statusBadges = new UnityEngine.UI.Text[6];

        float cardH = 210f;
        float spacing = 14f;
        for (int i = 0; i < 6; i++)
        {
            float yPos = -10f - i * (cardH + spacing);
            RectTransform card = CreateRect(content, "FormCard_" + i, new Vector2(0.02f, 1f), new Vector2(0.98f, 1f));
            card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0f, yPos);
            card.sizeDelta = new Vector2(0f, cardH);
            GetOrAdd<UnityEngine.UI.Image>(card.gameObject).color = new Color(0.10f, 0.15f, 0.22f, 0.98f);

            // Row 1: Tên & Badge (y: 0.75 -> 0.96)
            RectTransform rName = CreateRect(card, "TxtName", new Vector2(0.04f, 0.75f), new Vector2(0.62f, 0.96f));
            var tName = GetOrAdd<UnityEngine.UI.Text>(rName.gameObject);
            tName.font = font;
            tName.fontSize = 15;
            tName.fontStyle = FontStyle.Bold;
            tName.color = new Color(0.95f, 0.85f, 0.40f, 1f);
            tName.alignment = TextAnchor.MiddleLeft;
            tName.text = FormationPanelUI.FormationNames[i];
            ui.formationLabels[i] = tName;

            RectTransform rBadge = CreateRect(card, "TxtBadge", new Vector2(0.64f, 0.75f), new Vector2(0.96f, 0.96f));
            var tBadge = GetOrAdd<UnityEngine.UI.Text>(rBadge.gameObject);
            tBadge.font = font;
            tBadge.fontSize = 12;
            tBadge.fontStyle = FontStyle.Bold;
            tBadge.alignment = TextAnchor.MiddleRight;
            tBadge.supportRichText = true;
            tBadge.text = "[SẴN SÀNG]";
            ui.statusBadges[i] = tBadge;

            // Row 2: Desc (y: 0.30 -> 0.74)
            RectTransform rDesc = CreateRect(card, "TxtDesc", new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.74f));
            var tDesc = GetOrAdd<UnityEngine.UI.Text>(rDesc.gameObject);
            tDesc.font = font;
            tDesc.fontSize = 12;
            tDesc.color = new Color(0.85f, 0.90f, 0.95f, 1f);
            tDesc.alignment = TextAnchor.UpperLeft;
            tDesc.supportRichText = true;
            tDesc.text = FormationPanelUI.FormationDescs[i];
            ui.formationDescTexts[i] = tDesc;

            // Row 3: Chi phí (y: 0.05 -> 0.28, x: 0.04 -> 0.54)
            RectTransform rCost = CreateRect(card, "TxtCost", new Vector2(0.04f, 0.05f), new Vector2(0.54f, 0.28f));
            var tCost = GetOrAdd<UnityEngine.UI.Text>(rCost.gameObject);
            tCost.font = font;
            tCost.fontSize = 11;
            tCost.fontStyle = FontStyle.Bold;
            tCost.color = new Color(0.33f, 0.94f, 0.77f, 1f);
            tCost.alignment = TextAnchor.MiddleLeft;
            tCost.supportRichText = true;
            ui.formationCostTexts[i] = tCost;

            // Nút Kích Hoạt (y: 0.05 -> 0.28, x: 0.55 -> 0.75)
            RectTransform btnSel = CreateRect(card, "BtnSelect", new Vector2(0.55f, 0.05f), new Vector2(0.75f, 0.28f));
            GetOrAdd<UnityEngine.UI.Image>(btnSel.gameObject).color = new Color(0.15f, 0.45f, 0.35f, 1f);
            var bSel = GetOrAdd<UnityEngine.UI.Button>(btnSel.gameObject);
            CreateLabel(btnSel, "Label", "KÍCH HOẠT", font, Vector2.zero, Vector2.one);
            var tSel = btnSel.Find("Label").GetComponent<UnityEngine.UI.Text>();
            tSel.fontSize = 12;
            tSel.fontStyle = FontStyle.Bold;
            tSel.color = Color.white;
            int fIdx = i;
            bSel.onClick.RemoveAllListeners();
            bSel.onClick.AddListener(() => ui.OnSelectFormation(fIdx));
            ui.selectButtons[i] = bSel;

            // Nút Tu Luyện (y: 0.05 -> 0.28, x: 0.77 -> 0.96)
            RectTransform btnUpg = CreateRect(card, "BtnUpgrade", new Vector2(0.77f, 0.05f), new Vector2(0.96f, 0.28f));
            GetOrAdd<UnityEngine.UI.Image>(btnUpg.gameObject).color = new Color(0.58f, 0.32f, 0.10f, 1f);
            var bUpg = GetOrAdd<UnityEngine.UI.Button>(btnUpg.gameObject);
            CreateLabel(btnUpg, "Label", "TU LUYỆN", font, Vector2.zero, Vector2.one);
            var tUpg = btnUpg.Find("Label").GetComponent<UnityEngine.UI.Text>();
            tUpg.fontSize = 12;
            tUpg.fontStyle = FontStyle.Bold;
            tUpg.color = Color.white;
            bUpg.onClick.RemoveAllListeners();
            bUpg.onClick.AddListener(() => ui.OnUpgradeFormation(fIdx));
            ui.upgradeButtons[i] = bUpg;
        }

        // 5. Footer Tip
        RectTransform guide = CreateRect(panel, "FormationGuide", new Vector2(0.03f, 0.015f), new Vector2(0.97f, 0.075f));
        GetOrAdd<UnityEngine.UI.Image>(guide.gameObject).color = new Color(0.08f, 0.11f, 0.16f, 0.85f);
        CreateLabel(guide, "Label", "✦ Mỗi thời điểm chỉ kích hoạt 1 Đại Trận. Linh hoạt chuyển đổi để quét quái hoặc khắc chế Boss.", font, new Vector2(0.02f, 0f), new Vector2(0.98f, 1f));
        var tGuide = guide.Find("Label").GetComponent<UnityEngine.UI.Text>();
        tGuide.fontSize = 11;
        tGuide.color = new Color(0.55f, 0.62f, 0.72f, 0.9f);

        EditorUtility.SetDirty(ui);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TÁI CẤU TRÚC VÀ ĐỊNH VỊ TOÀN BỘ HUD GAMEPLAY (CHỐNG ĐÈ CHÉO TRÊN MOBILE)
    // ─────────────────────────────────────────────────────────────────────────
    private static void OrganizeMobileHUD(Canvas canvas, RectTransform mobileRoot)
    {
        // 1. TẮT CÁC PANEL CŨ KHÔNG DÙNG TRÊN MOBILE
        string[] legacyPanels = { "FullBackground", "RightPanel", "ShopPanel", "TutorialOverlay", "MilestoneModalUI" };
        foreach (var pName in legacyPanels)
        {
            Transform t = canvas.transform.Find(pName);
            if (t != null) t.gameObject.SetActive(false);
        }

        // HUD gameplay phải dùng cùng safe area với dock/drawer để không bị che bởi notch.
        // Đưa nó xuống dưới lớp drawer để panel đang mở luôn chặn input và hiển thị phía trên HUD.
        string[] mobileHud =
        {
            "TopBar", "CharacterFrame", "PauseButton", "StagePanel", "StageUI",
            "PlayerHPBar", "PlayerManaBar", "CombatLogPanel", "TutorialTargetQuestBar", "ActiveBuffPanel"
        };
        foreach (string hudName in mobileHud)
        {
            Transform hud = canvas.transform.Find(hudName);
            if (hud == null || hud.parent == mobileRoot) continue;
            hud.SetParent(mobileRoot, false);
            hud.SetAsFirstSibling();
        }

        // 2. HEADER 3 TẦNG: tài nguyên -> cảnh giới/tiến độ -> HP/MP.
        // Các mốc cách nhau rõ ràng để không còn đè nhau ở tỷ lệ 9:16.
        Transform topBar = mobileRoot.Find("TopBar");
        if (topBar != null)
        {
            RectTransform rt = topBar as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(0f, 84f);

            Transform lt = topBar.Find("txt_LinhThach");
            if (lt != null)
            {
                RectTransform ltRt = lt as RectTransform;
                ltRt.anchorMin = new Vector2(0.11f, 0.12f);
                ltRt.anchorMax = new Vector2(0.49f, 0.88f);
                ltRt.offsetMin = Vector2.zero; ltRt.offsetMax = Vector2.zero;
                var txt = lt.GetComponent<UnityEngine.UI.Text>();
                if (txt != null) { txt.alignment = TextAnchor.MiddleLeft; txt.fontSize = FontHud; }
                var display = GetOrAdd<MobileCurrencyText>(lt.gameObject);
                display.currency = MobileCurrencyText.Currency.LinhThach;
            }
            Transform cd = topBar.Find("txt_CoDuyen");
            if (cd != null)
            {
                RectTransform cdRt = cd as RectTransform;
                cdRt.anchorMin = new Vector2(0.51f, 0.12f);
                cdRt.anchorMax = new Vector2(0.89f, 0.88f);
                cdRt.offsetMin = Vector2.zero; cdRt.offsetMax = Vector2.zero;
                var txt = cd.GetComponent<UnityEngine.UI.Text>();
                if (txt != null) { txt.alignment = TextAnchor.MiddleLeft; txt.fontSize = FontHud; }
                Object.DestroyImmediate(cd.GetComponent<CoDuyenUIUpdater>());
                var display = GetOrAdd<MobileCurrencyText>(cd.gameObject);
                display.currency = MobileCurrencyText.Currency.CoDuyen;
            }
        }

        // 3. REPOSITION CHARACTER FRAME (Avatar mở Modal Chi Tiết) -> Góc Trên Bên Trái
        Transform charFrame = mobileRoot.Find("CharacterFrame");
        if (charFrame != null)
        {
            RectTransform rt = charFrame as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -5f);
            rt.sizeDelta = new Vector2(74f, 74f);
        }

        // 4. REPOSITION PAUSE BUTTON -> Góc Trên Bên Phải
        Transform pauseBtn = mobileRoot.Find("PauseButton");
        if (pauseBtn != null)
        {
            RectTransform rt = pauseBtn as RectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-12f, -5f);
            rt.sizeDelta = new Vector2(66f, 66f);
        }

        // 5. REPOSITION STAGE PANEL (Cảnh Giới Tu Vi) -> Dưới TopBar
        Transform stagePanel = mobileRoot.Find("StagePanel");
        if (stagePanel != null)
        {
            RectTransform rt = stagePanel as RectTransform;
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -90f);
            rt.sizeDelta = new Vector2(0f, 40f);

            Transform stText = stagePanel.Find("StageText");
            if (stText != null)
            {
                RectTransform stRt = stText as RectTransform;
                stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
                stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;
                var txt = stText.GetComponent<UnityEngine.UI.Text>();
                if (txt != null) { txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = FontBody; }
            }
        }

        // 6. REPOSITION STAGE UI (Tầng Đánh Quái & Boss Timer) -> Căn Giữa Dưới Cảnh Giới
        Transform stageUI = mobileRoot.Find("StageUI");
        if (stageUI != null)
        {
            RectTransform rt = stageUI as RectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -136f);
            rt.sizeDelta = new Vector2(760f, 42f);

            Transform stText = stageUI.Find("StageText");
            if (stText != null)
            {
                RectTransform stRt = stText as RectTransform;
                stRt.anchorMin = new Vector2(0f, 0.48f);
                stRt.anchorMax = Vector2.one;
                stRt.anchoredPosition = Vector2.zero;
                var txt = stText.GetComponent<UnityEngine.UI.Text>();
                if (txt != null) { txt.alignment = TextAnchor.MiddleCenter; txt.fontSize = FontHud; }
            }
            Transform pBg = stageUI.Find("ProgressBG");
            if (pBg != null)
            {
                RectTransform pRt = pBg as RectTransform;
                pRt.anchorMin = new Vector2(0.12f, 0.06f);
                pRt.anchorMax = new Vector2(0.88f, 0.40f);
                pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;
            }
        }

        // 7. HP/MP nằm ngay dưới tiến độ, mỗi thanh có khoảng cách 8 px cố định.
        Transform hpBar = mobileRoot.Find("PlayerHPBar");
        if (hpBar != null)
        {
            RectTransform rt = hpBar as RectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -186f);
            rt.sizeDelta = new Vector2(560f, 20f);
        }

        Transform mpBar = mobileRoot.Find("PlayerManaBar");
        if (mpBar != null)
        {
            RectTransform rt = mpBar as RectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -214f);
            rt.sizeDelta = new Vector2(560f, 16f);
        }

        // 8. REPOSITION COMBAT LOG PANEL -> Nằm trên Dock Bar (y: 160)
        Transform logPanel = mobileRoot.Find("CombatLogPanel");
        if (logPanel != null)
        {
            RectTransform rt = logPanel as RectTransform;
            rt.anchorMin = new Vector2(0.05f, 0f);
            rt.anchorMax = new Vector2(0.95f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 205f);
            rt.sizeDelta = new Vector2(0f, 88f);

            var img = logPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0.04f, 0.06f, 0.1f, 0.45f);
        }

        // 9. REPOSITION TUTORIAL QUEST BAR (Nếu có thanh nhiệm vụ tân thủ)
        Transform questBar = mobileRoot.Find("TutorialTargetQuestBar");
        if (questBar != null)
        {
            RectTransform qRt = questBar as RectTransform;
            qRt.anchorMin = new Vector2(0.05f, 1f);
            qRt.anchorMax = new Vector2(0.95f, 1f);
            qRt.pivot = new Vector2(0.5f, 1f);
            qRt.anchoredPosition = new Vector2(0f, -244f);
            qRt.sizeDelta = new Vector2(0f, 38f);
        }

        // 10. REPOSITION ACTIVE BUFF PANEL -> Dưới thanh Quest/Mana
        Transform buffPanel = mobileRoot.Find("ActiveBuffPanel");
        if (buffPanel != null)
        {
            RectTransform rt = buffPanel as RectTransform;
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -290f);
            rt.sizeDelta = new Vector2(0f, 30f);
        }

        // 11. ĐẢM BẢO THỨ TỰ SIBLING (Z-ORDER):
        mobileRoot.SetAsLastSibling();

        string[] modalNames = { "PauseMenuModal", "CharacterDetailModal", "TribulationModalUI", "OfflineRewardPopupUI" };
        foreach (var mName in modalNames)
        {
            Transform m = canvas.transform.Find(mName);
            if (m != null) m.SetAsLastSibling();
        }

        Transform mainMenu = canvas.transform.Find("MainMenuManager");
        if (mainMenu != null)
        {
            mainMenu.SetAsLastSibling();
            mainMenu.gameObject.SetActive(true);
        }
    }

    private static void OrganizeCombatFeedback(Canvas canvas, RectTransform mobileRoot)
    {
        Transform banner = canvas.transform.Find("CombatNotificationBanner");
        if (banner == null) banner = mobileRoot.Find("CombatNotificationBanner");
        if (banner == null) return;
        if (banner.parent != mobileRoot) banner.SetParent(mobileRoot, false);

        RectTransform rect = banner as RectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.66f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(760f, 150f);
        rect.SetAsLastSibling();
    }
}
#endif

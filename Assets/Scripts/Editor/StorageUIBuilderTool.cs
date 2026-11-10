#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool kiến tạo Giao diện Túi Trữ Vật, Bộ Lọc Ưu Tiên & Lò Phân Giải (StorageModalUI & DismantleConfirmModalUI).
/// Tự động sinh Hierarchy hoàn chỉnh chuẩn Mobile Portrait 9:16 và gắn nút mở trên HUD.
/// </summary>
public static class StorageUIBuilderTool
{
    [MenuItem("TuTien/UI/Tạo Sẵn Kho Đồ & Lò Phân Giải (StorageModalUI)")]
    public static void CreateOrSelectStorageUI()
    {
        EnsureStorageUIInScene(forceRecreate: true);
    }

    public static void EnsureStorageUIInScene(bool forceRecreate)
    {
        if (Application.isPlaying) return;

        // 1. Tìm Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Canvas c = Object.FindAnyObjectByType<Canvas>();
            if (c != null) canvasObj = c.gameObject;
        }

        if (canvasObj == null)
        {
            Debug.LogError("❌ [StorageUIBuilder] Không tìm thấy Canvas trong Scene!");
            return;
        }

        // 2. Đảm bảo Managers có StorageManager và DismantleService
        GameObject managers = GameObject.Find("Managers");
        if (managers == null) managers = GameObject.Find("DataManager");
        if (managers == null) managers = new GameObject("Managers");

        if (managers.GetComponent<StorageManager>() == null) managers.AddComponent<StorageManager>();
        if (managers.GetComponent<DismantleService>() == null) managers.AddComponent<DismantleService>();
        EditorUtility.SetDirty(managers);

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 3. Xoá phiên bản cũ nếu forceRecreate
        Transform oldStorage = canvasObj.transform.Find("StorageModalUI");
        if (oldStorage != null && forceRecreate)
        {
            Undo.DestroyObjectImmediate(oldStorage.gameObject);
        }

        Transform oldConfirm = canvasObj.transform.Find("DismantleConfirmModalUI");
        if (oldConfirm != null && forceRecreate)
        {
            Undo.DestroyObjectImmediate(oldConfirm.gameObject);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. TẠO CONFIRMATION MODAL (DismantleConfirmModalUI)
        // ─────────────────────────────────────────────────────────────────────
        GameObject confirmObj = new GameObject("DismantleConfirmModalUI", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(DismantleConfirmModalUI));
        confirmObj.transform.SetParent(canvasObj.transform, false);

        RectTransform confRt = confirmObj.GetComponent<RectTransform>();
        confRt.anchorMin = Vector2.zero;
        confRt.anchorMax = Vector2.one;
        confRt.offsetMin = Vector2.zero;
        confRt.offsetMax = Vector2.zero;

        Image confBg = confirmObj.GetComponent<Image>();
        confBg.color = new Color(0f, 0f, 0f, 0.82f);
        confBg.raycastTarget = true;

        Button confBackdrop = confirmObj.AddComponent<Button>();
        confBackdrop.transition = Selectable.Transition.None;

        CanvasGroup confCg = confirmObj.GetComponent<CanvasGroup>();
        confCg.alpha = 1f;

        DismantleConfirmModalUI confUI = confirmObj.GetComponent<DismantleConfirmModalUI>();
        confUI.canvasGroup = confCg;
        confUI.btnBackdrop = confBackdrop;

        // Box
        GameObject confBox = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        confBox.transform.SetParent(confirmObj.transform, false);
        RectTransform cbRt = confBox.GetComponent<RectTransform>();
        cbRt.anchorMin = cbRt.anchorMax = new Vector2(0.5f, 0.5f);
        cbRt.sizeDelta = new Vector2(560f, 380f);
        cbRt.anchoredPosition = Vector2.zero;

        Image cbImg = confBox.GetComponent<Image>();
        cbImg.color = new Color(0.08f, 0.10f, 0.15f, 0.98f);

        Outline cbOl = confBox.GetComponent<Outline>();
        cbOl.effectColor = new Color(0.96f, 0.55f, 0.20f, 0.95f);
        cbOl.effectDistance = new Vector2(2f, -2f);

        confUI.modalBox = cbRt;

        // Title
        GameObject confTitle = CreateTextObj("TxtTitle", confBox.transform, defaultFont, 18, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.96f, 0.65f, 0.20f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(520f, 32f));
        confUI.txtTitle = confTitle.GetComponent<Text>();

        // Item Name
        GameObject confItemName = CreateTextObj("TxtItemName", confBox.transform, defaultFont, 16, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.95f, 0.92f, 0.85f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(520f, 28f));
        confUI.txtItemName = confItemName.GetComponent<Text>();

        // Item Detail
        GameObject confItemDetail = CreateTextObj("TxtItemDetail", confBox.transform, defaultFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.68f, 0.78f, 0.90f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(520f, 24f));
        confUI.txtItemDetail = confItemDetail.GetComponent<Text>();

        // Reward Box
        GameObject confRewBox = new GameObject("RewardBox", typeof(RectTransform), typeof(Image));
        confRewBox.transform.SetParent(confBox.transform, false);
        RectTransform crbRt = confRewBox.GetComponent<RectTransform>();
        crbRt.anchorMin = crbRt.anchorMax = new Vector2(0.5f, 0.5f);
        crbRt.anchoredPosition = new Vector2(0f, -10f);
        crbRt.sizeDelta = new Vector2(500f, 90f);
        confRewBox.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.90f);

        GameObject confRewText = CreateTextObj("TxtReward", confRewBox.transform, defaultFont, 12, FontStyle.Normal, TextAnchor.MiddleCenter,
            Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        confUI.txtRewardSummary = confRewText.GetComponent<Text>();

        // Warning
        GameObject confWarn = CreateTextObj("TxtWarning", confBox.transform, defaultFont, 11, FontStyle.Italic, TextAnchor.MiddleCenter,
            new Color(1f, 0.40f, 0.38f, 1f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(520f, 40f));
        confUI.txtWarning = confWarn.GetComponent<Text>();

        // Cancel Button
        GameObject btnCancelObj = CreateButtonObj("BtnCancel", confBox.transform, defaultFont, "Giữ Lại", 13,
            new Color(0.20f, 0.28f, 0.38f, 0.95f), new Vector2(0.5f, 0f), new Vector2(-120f, 32f), new Vector2(180f, 44f));
        confUI.btnCancel = btnCancelObj.GetComponent<Button>();

        // Confirm Button
        GameObject btnConfObj = CreateButtonObj("BtnConfirm", confBox.transform, defaultFont, "⚠️ Chắc Chắn Tách", 13,
            new Color(0.80f, 0.22f, 0.20f, 0.95f), new Vector2(0.5f, 0f), new Vector2(120f, 32f), new Vector2(180f, 44f));
        confUI.btnConfirm = btnConfObj.GetComponent<Button>();

        confirmObj.SetActive(false);

        // ─────────────────────────────────────────────────────────────────────
        // 5. TẠO GIAO DIỆN CHÍNH (StorageModalUI)
        // ─────────────────────────────────────────────────────────────────────
        GameObject modalObj = new GameObject("StorageModalUI", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(StorageModalUI));
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform mRt = modalObj.GetComponent<RectTransform>();
        mRt.anchorMin = Vector2.zero;
        mRt.anchorMax = Vector2.one;
        mRt.offsetMin = Vector2.zero;
        mRt.offsetMax = Vector2.zero;

        Image mBg = modalObj.GetComponent<Image>();
        mBg.color = new Color(0f, 0f, 0f, 0.82f);
        mBg.raycastTarget = true;

        Button backdropBtn = modalObj.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        StorageModalUI ui = modalObj.GetComponent<StorageModalUI>();
        ui.canvasGroup = cg;
        ui.btnBackdrop = backdropBtn;

        // Modal Box chính (670 x 880px)
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(modalObj.transform, false);

        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.sizeDelta = new Vector2(670f, 880f);
        boxRt.anchoredPosition = Vector2.zero;

        Image boxBg = boxObj.GetComponent<Image>();
        boxBg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

        Outline boxOl = boxObj.GetComponent<Outline>();
        boxOl.effectColor = new Color(0.96f, 0.77f, 0.26f, 0.95f);
        boxOl.effectDistance = new Vector2(2f, -2f);

        ui.modalBox = boxRt;

        // Close Button
        GameObject closeObj = CreateButtonObj("BtnClose", boxObj.transform, defaultFont, "✕", 22,
            new Color(0.85f, 0.25f, 0.25f, 0.95f), new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(44f, 44f));
        ui.btnClose = closeObj.GetComponent<Button>();

        // Header Title
        GameObject titleObj = CreateTextObj("TxtTitle", boxObj.transform, defaultFont, 21, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.96f, 0.82f, 0.32f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(480f, 28f));

        // Subtitle
        CreateTextObj("TxtSubtitle", boxObj.transform, defaultFont, 11, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.68f, 0.78f, 0.90f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(500f, 18f))
            .GetComponent<Text>().text = "Quản lý vật phẩm, thu thập ưu tiên & lò phân giải chống lạm phát";

        // Wallet Bar (Hàng tài nguyên hiển thị)
        GameObject walletBar = new GameObject("WalletBar", typeof(RectTransform), typeof(Image));
        walletBar.transform.SetParent(boxObj.transform, false);
        RectTransform wbRt = walletBar.GetComponent<RectTransform>();
        wbRt.anchorMin = wbRt.anchorMax = new Vector2(0.5f, 1f);
        wbRt.anchoredPosition = new Vector2(0f, -72f);
        wbRt.sizeDelta = new Vector2(630f, 28f);
        walletBar.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.95f);

        ui.txtLinhThach = CreateTextObj("TxtLinhThach", walletBar.transform, defaultFont, 11, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.45f, 0.82f, 1f), new Vector2(0f, 0f), new Vector2(0.25f, 1f), Vector2.zero, Vector2.zero).GetComponent<Text>();
        ui.txtKiemY = CreateTextObj("TxtKiemY", walletBar.transform, defaultFont, 11, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.88f, 0.40f, 1f), new Vector2(0.25f, 0f), new Vector2(0.50f, 1f), Vector2.zero, Vector2.zero).GetComponent<Text>();
        ui.txtOre = CreateTextObj("TxtOre", walletBar.transform, defaultFont, 11, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(1f, 0.73f, 0.15f), new Vector2(0.50f, 0f), new Vector2(0.75f, 1f), Vector2.zero, Vector2.zero).GetComponent<Text>();
        ui.txtDust = CreateTextObj("TxtDust", walletBar.transform, defaultFont, 11, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.60f, 0.98f, 0.60f), new Vector2(0.75f, 0f), new Vector2(1.0f, 1f), Vector2.zero, Vector2.zero).GetComponent<Text>();

        // Tabs Row (5 Tabs: Tất Cả, Kiếm Khí, Đan Dược, Nguyên Liệu, Hòm Tạm)
        string[] tabNames = { "Tất Cả", "Kiếm Khí", "Đan Dược", "Nguyên Liệu", "Hòm Tạm" };
        float tabW = 122f;
        float startX = -244f;
        for (int i = 0; i < 5; i++)
        {
            float posX = startX + i * tabW;
            GameObject tBtnObj = CreateButtonObj($"Tab_{i}", boxObj.transform, defaultFont, tabNames[i], 12,
                new Color(0.12f, 0.16f, 0.22f, 0.95f), new Vector2(0.5f, 1f), new Vector2(posX, -110f), new Vector2(118f, 32f));
            ui.tabButtons[i] = tBtnObj.GetComponent<Button>();
        }

        // Khung Cài Đặt Bộ Lọc Ưu Tiên & Chế Độ Xem (Priority Filter Bar)
        GameObject filterBox = new GameObject("PriorityFilterBar", typeof(RectTransform), typeof(Image));
        filterBox.transform.SetParent(boxObj.transform, false);
        RectTransform fbRt = filterBox.GetComponent<RectTransform>();
        fbRt.anchorMin = fbRt.anchorMax = new Vector2(0.5f, 1f);
        fbRt.anchoredPosition = new Vector2(0f, -148f);
        fbRt.sizeDelta = new Vector2(630f, 32f);
        filterBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        ui.toggleSwordHighLevel = CreateToggleObj("TogSword", filterBox.transform, defaultFont, "Kiếm ≥ Lv2", new Vector2(-235f, 0f), 125f);
        ui.togglePillBreakthrough = CreateToggleObj("TogPill", filterBox.transform, defaultFont, "Đan Đột Phá", new Vector2(-105f, 0f), 125f);
        ui.toggleAutoDismantleLv1 = CreateToggleObj("TogAutoDismantle", filterBox.transform, defaultFont, "Tự Tách Lv1", new Vector2(25f, 0f), 125f);

        GameObject btnToggleView = CreateButtonObj("BtnToggleView", filterBox.transform, defaultFont, "📋 Xem DS", 11,
            new Color(0.16f, 0.24f, 0.35f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(245f, 0f), new Vector2(115f, 26f));
        ui.btnToggleView = btnToggleView.GetComponent<Button>();
        ui.txtToggleView = btnToggleView.GetComponentInChildren<Text>();

        // Scroll View Container (Hoàn toàn nằm gọn trong ModalBox giữa FilterBar và FooterBar, dùng RectMask2D chống tràn)
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(RectMask2D));
        scrollObj.transform.SetParent(boxObj.transform, false);
        RectTransform sRt = scrollObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f);
        sRt.anchorMax = new Vector2(1f, 1f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.offsetMin = new Vector2(16f, 66f);   // Đỉnh của FooterBar là 56px, chừa lề 10px an toàn
        sRt.offsetMax = new Vector2(-16f, -170f); // Đáy của FilterBar là -164px, chừa lề 6px an toàn

        Image scrollImg = scrollObj.GetComponent<Image>();
        scrollImg.color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
        scrollImg.raycastTarget = true;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.elasticity = 0.1f;
        sr.inertia = true;
        sr.decelerationRate = 0.135f;
        sr.scrollSensitivity = 28f;
        sr.viewport = sRt;
        ui.scrollRect = sr;

        // 1. Grid Content Container (Dạng Ô 6 Cột chuẩn túi RPG)
        GameObject gridObj = new GameObject("ContentGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        gridObj.transform.SetParent(scrollObj.transform, false);
        RectTransform cntGridRt = gridObj.GetComponent<RectTransform>();
        cntGridRt.anchorMin = new Vector2(0f, 1f);
        cntGridRt.anchorMax = new Vector2(1f, 1f);
        cntGridRt.pivot = new Vector2(0.5f, 1f);
        cntGridRt.anchoredPosition = Vector2.zero;
        cntGridRt.sizeDelta = new Vector2(0f, 0f);

        GridLayoutGroup glg = gridObj.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(92f, 92f);
        glg.spacing = new Vector2(8f, 8f);
        glg.padding = new RectOffset(10, 10, 8, 8);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 6;

        ContentSizeFitter csfGrid = gridObj.GetComponent<ContentSizeFitter>();
        csfGrid.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfGrid.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // 2. List Content Container (Dạng Danh Sách thẻ gọn gàng 48px)
        GameObject listObj = new GameObject("ContentList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        listObj.transform.SetParent(scrollObj.transform, false);
        RectTransform cntListRt = listObj.GetComponent<RectTransform>();
        cntListRt.anchorMin = new Vector2(0f, 1f);
        cntListRt.anchorMax = new Vector2(1f, 1f);
        cntListRt.pivot = new Vector2(0.5f, 1f);
        cntListRt.anchoredPosition = Vector2.zero;
        cntListRt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vlg = listObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csfList = listObj.GetComponent<ContentSizeFitter>();
        csfList.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csfList.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        listObj.SetActive(false); // Mặc định hiển thị Dạng Ô (Grid Slots)
        gridObj.SetActive(true);

        sr.content = cntGridRt;
        ui.gridContent = cntGridRt;
        ui.listContent = cntListRt;
        ui.contentContainer = cntGridRt;

        // Empty Notice
        GameObject emptyObj = CreateTextObj("TxtEmptyNotice", scrollObj.transform, defaultFont, 14, FontStyle.Italic, TextAnchor.MiddleCenter,
            new Color(0.55f, 0.65f, 0.75f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        ui.txtEmptyNotice = emptyObj.GetComponent<Text>();
        ui.txtEmptyNotice.gameObject.SetActive(false);

        // Footer Actions Bar (Được neo cố định ở đáy ModalBox)
        GameObject footerObj = new GameObject("FooterBar", typeof(RectTransform));
        footerObj.transform.SetParent(boxObj.transform, false);
        RectTransform fRt = footerObj.GetComponent<RectTransform>();
        fRt.anchorMin = fRt.anchorMax = new Vector2(0.5f, 0f);
        fRt.anchoredPosition = new Vector2(0f, 32f);
        fRt.sizeDelta = new Vector2(630f, 48f);

        GameObject btnClaim = CreateButtonObj("BtnClaimPriority", footerObj.transform, defaultFont, "⚡ Nhận Ưu Tiên", 13,
            new Color(0.15f, 0.55f, 0.40f, 0.98f), new Vector2(0.5f, 0.5f), new Vector2(-155f, 0f), new Vector2(180f, 42f));
        ui.btnClaimPriority = btnClaim.GetComponent<Button>();
        ui.txtClaimPriority = btnClaim.GetComponentInChildren<Text>();

        GameObject btnBatch = CreateButtonObj("BtnBatchDismantle", footerObj.transform, defaultFont, "♻️ Tách Phế Phẩm", 13,
            new Color(0.70f, 0.38f, 0.12f, 0.98f), new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(180f, 42f));
        ui.btnBatchDismantle = btnBatch.GetComponent<Button>();
        ui.txtBatchDismantle = btnBatch.GetComponentInChildren<Text>();

        GameObject btnCloseFt = CreateButtonObj("BtnCloseFooter", footerObj.transform, defaultFont, "✕ Đóng", 13,
            new Color(0.25f, 0.28f, 0.35f, 0.98f), new Vector2(0.5f, 0.5f), new Vector2(240f, 0f), new Vector2(110f, 42f));
        btnCloseFt.GetComponent<Button>().onClick.AddListener(ui.Close);

        modalObj.SetActive(false);

        // ─────────────────────────────────────────────────────────────────────
        // 6. GẮN NÚT MỞ KHO ĐỒ 🎒 TRÊN HUD & TRONG DRAWER KIẾM LÔ
        // ─────────────────────────────────────────────────────────────────────
        AttachStorageButtonToHUD(canvasObj, defaultFont);
        AttachStorageButtonToMainHUD(canvasObj, defaultFont);
        AttachStorageButtonToMergeDrawer(canvasObj, defaultFont);

        Canvas.ForceUpdateCanvases();
        EditorUtility.SetDirty(modalObj);
        EditorUtility.SetDirty(confirmObj);
        EditorSceneManager.MarkSceneDirty(canvasObj.scene);

        Debug.Log("🎉 [StorageUIBuilder] HOÀN TẤT: Đã tạo StorageModalUI & DismantleConfirmModalUI và gắn 3 nút mở (HUD Top, Drawer Kiếm Lô, QuestBar) thành công!");
    }

    /// <summary>
    /// Gắn nút Kho Đồ NỔI BẬT ngay trên Top-Right của màn hình chính (cạnh nút Cài đặt ⚙️)
    /// </summary>
    private static void AttachStorageButtonToMainHUD(GameObject canvasObj, Font font)
    {
        Transform mobileRoot = canvasObj.transform.Find("MobileUIRoot");
        if (mobileRoot == null) return;

        Transform existing = mobileRoot.Find("BtnHUDStorage");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject bObj = new GameObject("BtnHUDStorage", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline), typeof(StorageButtonHandler));
        bObj.transform.SetParent(mobileRoot, false);

        RectTransform rt = bObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-88f, -14f); // Ngay bên trái nút Pause (-12f, -14f)
        rt.sizeDelta = new Vector2(120f, 48f);

        Image img = bObj.GetComponent<Image>();
        img.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

        Outline ol = bObj.GetComponent<Outline>();
        ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.95f);
        ol.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        tObj.transform.SetParent(bObj.transform, false);
        RectTransform tt = tObj.GetComponent<RectTransform>();
        tt.anchorMin = Vector2.zero;
        tt.anchorMax = Vector2.one;
        tt.sizeDelta = Vector2.zero;

        Text t = tObj.GetComponent<Text>();
        if (font != null) t.font = font;
        t.fontSize = 13;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.96f, 0.85f, 0.40f, 1f);
        t.text = "KHO ĐỒ";

        Button btn = bObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => StorageModalUI.Open());
    }

    /// <summary>
    /// Gắn nút Lò Phân Giải & Kho Đồ ngay trong Drawer Kiếm Lô (Merge Panel)
    /// </summary>
    private static void AttachStorageButtonToMergeDrawer(GameObject canvasObj, Font font)
    {
        Transform mergePanel = canvasObj.transform.Find("MobileUIRoot/Drawer_0/MergePanel");
        if (mergePanel == null) return;

        Transform existing = mergePanel.Find("BtnDrawerDismantle");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject bObj = new GameObject("BtnDrawerDismantle", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
        bObj.transform.SetParent(mergePanel, false);

        RectTransform rt = bObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -4f);
        rt.sizeDelta = new Vector2(240f, 44f);

        Image img = bObj.GetComponent<Image>();
        img.color = new Color(0.70f, 0.22f, 0.15f, 0.96f); // Đỏ cam nổi bật Lò phân giải

        Outline ol = bObj.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.82f, 0.35f, 0.95f);
        ol.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        tObj.transform.SetParent(bObj.transform, false);
        RectTransform tt = tObj.GetComponent<RectTransform>();
        tt.anchorMin = Vector2.zero;
        tt.anchorMax = Vector2.one;
        tt.sizeDelta = Vector2.zero;

        Text t = tObj.GetComponent<Text>();
        if (font != null) t.font = font;
        t.fontSize = 13;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = "♻️ LÒ PHÂN GIẢI & KHO ĐỒ";

        Button btn = bObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => StorageModalUI.Open(StorageCategory.Swords));
    }

    private static void AttachStorageButtonToHUD(GameObject canvasObj, Font font)
    {
        Transform questTr = canvasObj.transform.Find("MobileUIRoot/TutorialTargetQuestBar");
        if (questTr == null) questTr = canvasObj.transform.Find("TutorialTargetQuestBar");
        if (questTr == null) return;

        Transform existingBtn = questTr.Find("BtnStorage");
        if (existingBtn == null)
        {
            GameObject bObj = new GameObject("BtnStorage", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
            bObj.transform.SetParent(questTr, false);
            existingBtn = bObj.transform;

            GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tObj.transform.SetParent(bObj.transform, false);
            RectTransform tt = tObj.GetComponent<RectTransform>();
            tt.anchorMin = Vector2.zero;
            tt.anchorMax = Vector2.one;
            tt.sizeDelta = Vector2.zero;

            Text t = tObj.GetComponent<Text>();
            if (font != null) t.font = font;
            t.fontSize = 16;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = "🎒";
        }

        RectTransform rt = existingBtn.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-126f, 0f); // Nằm ngay bên trái nút Thành Tựu (-88f)
        rt.sizeDelta = new Vector2(34f, 32f);

        Image img = existingBtn.GetComponent<Image>();
        img.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

        Outline ol = existingBtn.GetComponent<Outline>();
        ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.75f);
        ol.effectDistance = new Vector2(1f, -1f);

        Button btn = existingBtn.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => StorageModalUI.Open());
    }

    private static GameObject CreateTextObj(string name, Transform parent, Font font, int size, FontStyle style, TextAnchor align, Color col,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;

        Text t = go.GetComponent<Text>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = col;
        t.supportRichText = true;
        return go;
    }

    private static GameObject CreateButtonObj(string name, Transform parent, Font font, string text, int size, Color bgCol,
        Vector2 pivot, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = pivot;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;

        Image img = go.GetComponent<Image>();
        img.color = bgCol;

        Outline ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.50f);
        ol.effectDistance = new Vector2(1f, -1f);

        GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        tObj.transform.SetParent(go.transform, false);
        RectTransform trt = tObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        Text t = tObj.GetComponent<Text>();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = text;

        return go;
    }

    private static Toggle CreateToggleObj(string name, Transform parent, Font font, string label, Vector2 pos, float width)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 24f);

        Toggle tog = go.GetComponent<Toggle>();

        // Checkbox Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(go.transform, false);
        RectTransform bgrt = bgObj.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0f, 0.5f);
        bgrt.anchorMax = new Vector2(0f, 0.5f);
        bgrt.pivot = new Vector2(0f, 0.5f);
        bgrt.anchoredPosition = Vector2.zero;
        bgrt.sizeDelta = new Vector2(18f, 18f);

        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);
        tog.targetGraphic = bgImg;

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkObj.transform.SetParent(bgObj.transform, false);
        RectTransform chkRt = checkObj.GetComponent<RectTransform>();
        chkRt.anchorMin = Vector2.zero;
        chkRt.anchorMax = Vector2.one;
        chkRt.sizeDelta = new Vector2(-4f, -4f);

        Image chkImg = checkObj.GetComponent<Image>();
        chkImg.color = new Color(0.96f, 0.78f, 0.26f, 1f);
        tog.graphic = chkImg;

        // Label
        GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        lblObj.transform.SetParent(go.transform, false);
        RectTransform lRt = lblObj.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0f);
        lRt.anchorMax = new Vector2(1f, 1f);
        lRt.offsetMin = new Vector2(24f, 0f);
        lRt.offsetMax = Vector2.zero;

        Text txt = lblObj.GetComponent<Text>();
        if (font != null) txt.font = font;
        txt.fontSize = 11;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.color = new Color(0.85f, 0.88f, 0.95f, 1f);
        txt.text = label;

        return tog;
    }
}
#endif


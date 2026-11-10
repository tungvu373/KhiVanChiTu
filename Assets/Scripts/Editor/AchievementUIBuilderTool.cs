#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool kiến tạo Giao diện Thành Tựu Tiên Lộ (AchievementModalUI) trực tiếp lên Canvas.
/// Tự động xây dựng Hierarchy chuẩn Mobile 9:16 và liên kết vào AchievementManager & QuestBar.
/// </summary>
public class AchievementUIBuilderTool
{
    [MenuItem("TuTien/UI/Tạo Sẵn Bảng Thành Tựu (AchievementModalUI)")]
    public static void CreateOrSelectMenu()
    {
        EnsureAchievementUIInScene(forceSelect: true);
    }

    public static void EnsureAchievementUIInScene(bool forceSelect)
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
            if (forceSelect) Debug.LogError("❌ Không tìm thấy Canvas trong Scene!");
            return;
        }

        // 2. Đảm bảo Managers có AchievementManager
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
        {
            managers = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers Object");
        }
        AchievementManager am = managers.GetComponent<AchievementManager>();
        if (am == null) am = managers.AddComponent<AchievementManager>();
        EditorUtility.SetDirty(am);

        // 3. Tìm hoặc tạo AchievementModalUI
        Transform existingModal = canvasObj.transform.Find("AchievementModalUI");
        if (existingModal != null)
        {
            if (forceSelect)
            {
                Selection.activeGameObject = null;
                Undo.DestroyObjectImmediate(existingModal.gameObject);
                existingModal = null;
            }
            else
            {
                return;
            }
        }

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 4. Tạo Overlay toàn màn hình (Modal Backdrop)
        GameObject modalObj = new GameObject("AchievementModalUI", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(AchievementModalUI));
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform mRt = modalObj.GetComponent<RectTransform>();
        mRt.anchorMin = Vector2.zero;
        mRt.anchorMax = Vector2.one;
        mRt.offsetMin = Vector2.zero;
        mRt.offsetMax = Vector2.zero;

        Image mBg = modalObj.GetComponent<Image>();
        mBg.color = new Color(0f, 0f, 0f, 0.78f);
        mBg.raycastTarget = true;

        Button backdropBtn = modalObj.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        AchievementModalUI ui = modalObj.GetComponent<AchievementModalUI>();
        ui.canvasGroup = cg;
        ui.btnBackdrop = backdropBtn;

        // 5. Tạo Modal Box chính (Khung giao diện 680 x 860px)
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(modalObj.transform, false);

        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.sizeDelta = new Vector2(680f, 860f);
        boxRt.anchoredPosition = Vector2.zero;

        Image boxBg = boxObj.GetComponent<Image>();
        boxBg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f); // Midnight Dark Fantasy
        boxBg.raycastTarget = true;

        Outline boxOl = boxObj.GetComponent<Outline>();
        boxOl.effectColor = new Color(0.96f, 0.77f, 0.26f, 0.95f); // Viền vàng kim tiên gia
        boxOl.effectDistance = new Vector2(2f, -2f);

        ui.modalBox = boxRt;

        // ── NÚT ĐÓNG [✕] ──
        GameObject closeObj = new GameObject("BtnClose", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(boxObj.transform, false);
        RectTransform cRt = closeObj.GetComponent<RectTransform>();
        cRt.anchorMin = cRt.anchorMax = new Vector2(1f, 1f);
        cRt.pivot = new Vector2(1f, 1f);
        cRt.anchoredPosition = new Vector2(-12f, -12f);
        cRt.sizeDelta = new Vector2(56f, 56f);

        Image cImg = closeObj.GetComponent<Image>();
        cImg.color = new Color(0.85f, 0.25f, 0.25f, 0.95f);

        GameObject cTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        cTxtObj.transform.SetParent(closeObj.transform, false);
        RectTransform ctRt = cTxtObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;

        Text cTxt = cTxtObj.GetComponent<Text>();
        cTxt.font = defaultFont;
        cTxt.fontSize = 26;
        cTxt.fontStyle = FontStyle.Bold;
        cTxt.alignment = TextAnchor.MiddleCenter;
        cTxt.color = Color.white;
        cTxt.text = "✕";

        ui.btnClose = closeObj.GetComponent<Button>();

        // ── TIÊU ĐỀ HEADER ──
        GameObject titleObj = new GameObject("TxtHeaderTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(boxObj.transform, false);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.anchoredPosition = new Vector2(0f, -14f);
        tRt.sizeDelta = new Vector2(440f, 32f);

        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = defaultFont;
        titleTxt.fontSize = 22;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.text = "🏆 THÀNH TỰU TIÊN LỘ";
        titleTxt.color = new Color(0.96f, 0.82f, 0.32f, 1f);

        Outline tOl = titleObj.GetComponent<Outline>();
        tOl.effectColor = Color.black;
        tOl.effectDistance = new Vector2(1.5f, -1.5f);

        // ── DÒNG TIẾN ĐỘ TỔNG ──
        GameObject progObj = new GameObject("TxtTotalProgress", typeof(RectTransform), typeof(Text));
        progObj.transform.SetParent(boxObj.transform, false);
        RectTransform pRt = progObj.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 1f);
        pRt.pivot = new Vector2(0.5f, 1f);
        pRt.anchoredPosition = new Vector2(0f, -48f);
        pRt.sizeDelta = new Vector2(480f, 22f);

        Text progTxt = progObj.GetComponent<Text>();
        progTxt.font = defaultFont;
        progTxt.fontSize = 12;
        progTxt.alignment = TextAnchor.MiddleCenter;
        progTxt.text = "Tiến Độ Tiên Lộ: 0/42 (0%) | 0 Chờ Nhận";
        progTxt.color = new Color(0.75f, 0.85f, 0.98f, 1f);

        ui.txtTotalProgress = progTxt;

        // ── THANH TIẾN ĐỘ TỔNG (Bar) ──
        GameObject barBgObj = new GameObject("TotalProgBG", typeof(RectTransform), typeof(Image));
        barBgObj.transform.SetParent(boxObj.transform, false);
        RectTransform bbRt = barBgObj.GetComponent<RectTransform>();
        bbRt.anchorMin = bbRt.anchorMax = new Vector2(0.5f, 1f);
        bbRt.pivot = new Vector2(0.5f, 1f);
        bbRt.anchoredPosition = new Vector2(0f, -74f);
        bbRt.sizeDelta = new Vector2(480f, 8f);

        Image bbImg = barBgObj.GetComponent<Image>();
        bbImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        GameObject barFillObj = new GameObject("TotalProgFill", typeof(RectTransform), typeof(Image));
        barFillObj.transform.SetParent(barBgObj.transform, false);
        RectTransform bfRt = barFillObj.GetComponent<RectTransform>();
        bfRt.anchorMin = Vector2.zero;
        bfRt.anchorMax = Vector2.one;
        bfRt.sizeDelta = Vector2.zero;

        Image bfImg = barFillObj.GetComponent<Image>();
        bfImg.type = Image.Type.Filled;
        bfImg.fillMethod = Image.FillMethod.Horizontal;
        bfImg.fillOrigin = 0;
        bfImg.fillAmount = 0.35f;
        bfImg.color = new Color(0.2f, 0.85f, 0.35f, 1f);

        ui.fillTotalProgress = bfImg;

        // ── HÀNG TAB DANH MỤC (6 TABS) ──
        GameObject tabRowObj = new GameObject("TabRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabRowObj.transform.SetParent(boxObj.transform, false);
        RectTransform trRt = tabRowObj.GetComponent<RectTransform>();
        trRt.anchorMin = trRt.anchorMax = new Vector2(0.5f, 1f);
        trRt.pivot = new Vector2(0.5f, 1f);
        trRt.anchoredPosition = new Vector2(0f, -92f);
        trRt.sizeDelta = new Vector2(640f, 38f);

        HorizontalLayoutGroup hlg = tabRowObj.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        string[] tabNames = { "Tất Cả", "Tu Luyện", "Chiến Đấu", "Kiếm Đạo", "Bách Nghệ", "Kỳ Duyên" };
        ui.tabButtons = new Button[6];

        for (int i = 0; i < 6; i++)
        {
            GameObject tabBtnObj = new GameObject($"Tab_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            tabBtnObj.transform.SetParent(tabRowObj.transform, false);

            Image tabImg = tabBtnObj.GetComponent<Image>();
            tabImg.color = i == 0 ? new Color(0.96f, 0.77f, 0.26f, 0.95f) : new Color(0.12f, 0.16f, 0.24f, 0.90f);

            Outline tabOl = tabBtnObj.GetComponent<Outline>();
            tabOl.effectColor = new Color(0.25f, 0.35f, 0.5f, 0.5f);
            tabOl.effectDistance = new Vector2(1f, -1f);

            GameObject tabTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            tabTxtObj.transform.SetParent(tabBtnObj.transform, false);
            RectTransform ttRt = tabTxtObj.GetComponent<RectTransform>();
            ttRt.anchorMin = Vector2.zero;
            ttRt.anchorMax = Vector2.one;
            ttRt.sizeDelta = Vector2.zero;

            Text tabTxt = tabTxtObj.GetComponent<Text>();
            tabTxt.font = defaultFont;
            tabTxt.fontSize = 11;
            tabTxt.fontStyle = i == 0 ? FontStyle.Bold : FontStyle.Normal;
            tabTxt.alignment = TextAnchor.MiddleCenter;
            tabTxt.color = i == 0 ? new Color(0.08f, 0.08f, 0.12f, 1f) : new Color(0.75f, 0.82f, 0.92f, 1f);
            tabTxt.text = tabNames[i];

            ui.tabButtons[i] = tabBtnObj.GetComponent<Button>();
        }

        // ── SCROLL VIEW & VIEWPORT ──
        GameObject svObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        svObj.transform.SetParent(boxObj.transform, false);
        RectTransform svRt = svObj.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0f, 0f);
        svRt.anchorMax = new Vector2(1f, 1f);
        svRt.pivot = new Vector2(0.5f, 0.5f);
        svRt.offsetMin = new Vector2(20f, 68f);  // Cách đáy 68px (chừa chỗ cho nút Nhận Toàn Bộ)
        svRt.offsetMax = new Vector2(-20f, -140f); // Cách đỉnh 140px (chừa chỗ cho header + tabs)

        ScrollRect sr = svObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 25f;
        sr.movementType = ScrollRect.MovementType.Clamped;
        ui.scrollRect = sr;

        // Viewport dùng RectMask2D chuẩn mực (không lỗi render)
        GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        vpObj.transform.SetParent(svObj.transform, false);
        RectTransform vpRt = vpObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        sr.viewport = vpRt;

        // Content
        GameObject contObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contObj.transform.SetParent(vpObj.transform, false);
        RectTransform contRt = contObj.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0f, 1f);
        contRt.anchorMax = new Vector2(1f, 1f);
        contRt.pivot = new Vector2(0.5f, 1f);
        contRt.sizeDelta = new Vector2(0f, 100f);
        sr.content = contRt;
        ui.contentContainer = contRt;

        VerticalLayoutGroup vlg = contObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(6, 6, 8, 8);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = contObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── NÚT NHẬN TOÀN BỘ (DƯỚI ĐÁY) ──
        GameObject claimAllObj = new GameObject("BtnClaimAll", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        claimAllObj.transform.SetParent(boxObj.transform, false);
        RectTransform caRt = claimAllObj.GetComponent<RectTransform>();
        caRt.anchorMin = caRt.anchorMax = new Vector2(0.5f, 0f);
        caRt.pivot = new Vector2(0.5f, 0f);
        caRt.anchoredPosition = new Vector2(0f, 14f);
        caRt.sizeDelta = new Vector2(460f, 44f);

        Image caImg = claimAllObj.GetComponent<Image>();
        caImg.color = new Color(0.96f, 0.77f, 0.26f, 1f); // Gold button

        Outline caOl = claimAllObj.GetComponent<Outline>();
        caOl.effectColor = new Color(0.6f, 0.45f, 0.1f, 0.8f);
        caOl.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject caTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        caTxtObj.transform.SetParent(claimAllObj.transform, false);
        RectTransform catRt = caTxtObj.GetComponent<RectTransform>();
        catRt.anchorMin = Vector2.zero;
        catRt.anchorMax = Vector2.one;
        catRt.sizeDelta = Vector2.zero;

        Text caTxt = caTxtObj.GetComponent<Text>();
        caTxt.font = defaultFont;
        caTxt.fontSize = 14;
        caTxt.fontStyle = FontStyle.Bold;
        caTxt.alignment = TextAnchor.MiddleCenter;
        caTxt.color = new Color(0.10f, 0.10f, 0.14f, 1f);
        caTxt.text = "✨ NHẬN TOÀN BỘ";

        ui.btnClaimAll = claimAllObj.GetComponent<Button>();
        ui.txtClaimAll = caTxt;

        // Lưu và hoàn thiện
        modalObj.SetActive(false);
        EditorUtility.SetDirty(modalObj);
        EditorSceneManager.MarkSceneDirty(canvasObj.scene);

        Debug.Log("✅ [AchievementUIBuilderTool] Đã khởi tạo hoàn tất 'AchievementModalUI' trên Canvas!");

        if (forceSelect)
        {
            modalObj.SetActive(true);
            Selection.activeGameObject = modalObj;
            EditorGUIUtility.PingObject(modalObj);
        }
    }
}
#endif


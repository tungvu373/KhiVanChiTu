#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tạo sẵn Panel Chi Tiết Nhân Vật (CharacterDetailModal) trên Canvas trong Scene,
/// tích hợp ScrollView hoàn chỉnh (ScrollRect + RectMask2D + VerticalLayoutGroup + Scrollbar)
/// để người chơi thoải mái cuộn chuột / kéo xem toàn bộ thông tin mà không bao giờ bị tràn khỏi khung.
/// </summary>
[InitializeOnLoad]
public class CharacterDetailBuilderTool
{
    static CharacterDetailBuilderTool()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureCharacterDetailInScene(forceRebuild: false);
        };
    }

    [MenuItem("TuTien/UI/Tạo Panel Chi Tiết Nhân Vật (Character Detail)")]
    public static void CreateOrSelectMenu()
    {
        EnsureCharacterDetailInScene(forceRebuild: true);
    }

    [MenuItem("TuTien/UI/Làm Mới Scroll View Bảng Nhân Vật (Fix Tràn Viền)")]
    public static void RefreshScrollViewMenu()
    {
        EnsureCharacterDetailInScene(forceRebuild: true);
    }

    public static void EnsureCharacterDetailInScene(bool forceRebuild)
    {
        if (Application.isPlaying) return;

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Canvas c = Object.FindObjectOfType<Canvas>();
            if (c != null) canvasObj = c.gameObject;
        }

        if (canvasObj == null)
        {
            Debug.LogWarning("[CharacterDetailBuilder] Không tìm thấy Canvas trong Scene!");
            return;
        }

        Font fontToUse = FindBestFont(canvasObj);

        // 1. Cập nhật CharacterFrame ở góc trái dưới: Thêm click event & bỏ dòng "Loại: Nhân Tộc"
        SetupCharacterFrame(canvasObj, fontToUse);

        // 2. Kiểm tra Panel cũ: Nếu chưa có ScrollView chuẩn (hoặc phiên bản cũ thiếu Image bắt raycast / chưa bật childControlHeight) thì tự động rebuild
        Transform existing = canvasObj.transform.Find("CharacterDetailModal");
        if (existing != null)
        {
            if (!forceRebuild)
            {
                Transform sv = existing.Find("ModalBox/DetailScrollView");
                Transform vp = sv != null ? sv.Find("Viewport") : null;
                Image existingVpImg = vp != null ? vp.GetComponent<Image>() : null;
                VerticalLayoutGroup existingVlg = vp != null ? vp.Find("Content")?.GetComponent<VerticalLayoutGroup>() : null;

                if (sv == null || existingVpImg == null || existingVlg == null || !existingVlg.childControlHeight)
                {
                    forceRebuild = true;
                }
                else
                {
                    return;
                }
            }

            if (forceRebuild)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }
        }

        // 3. Tạo GameObject CharacterDetailModal chính (Full screen overlay)
        GameObject modalObj = new GameObject("CharacterDetailModal", typeof(RectTransform), typeof(CanvasGroup), typeof(CharacterDetailModalUI));
        Undo.RegisterCreatedObjectUndo(modalObj, "Tạo CharacterDetailModal");
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform modalRt = modalObj.GetComponent<RectTransform>();
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.offsetMin = Vector2.zero;
        modalRt.offsetMax = Vector2.zero;

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 4. Dark Overlay (Bấm ra ngoài thì đóng)
        GameObject overlayObj = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(modalObj.transform, false);
        RectTransform overRt = overlayObj.GetComponent<RectTransform>();
        overRt.anchorMin = Vector2.zero;
        overRt.anchorMax = Vector2.one;
        overRt.offsetMin = Vector2.zero;
        overRt.offsetMax = Vector2.zero;

        Image overImg = overlayObj.GetComponent<Image>();
        overImg.color = new Color(0f, 0f, 0f, 0.7f); // Nền tối làm nổi bật panel
        Button overBtn = overlayObj.GetComponent<Button>();

        // 5. ModalBox (Hộp nội dung chính ở giữa màn hình)
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image));
        boxObj.transform.SetParent(modalObj.transform, false);
        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(530f, 580f);

        Image boxBg = boxObj.GetComponent<Image>();
        boxBg.color = new Color(0.08f, 0.10f, 0.15f, 0.98f); // Nền mực lam tiên hiệp

        // Viền vàng kim quanh ModalBox
        CreateBorder(boxObj.transform, "BorderTop", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 3), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderBottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 3), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderLeft", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(3, 0), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderRight", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(3, 0), new Color(1f, 0.82f, 0.2f, 0.95f));

        // 6. Header Bar (Tiêu đề + Nút X)
        GameObject headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
        headerObj.transform.SetParent(boxObj.transform, false);
        RectTransform hRt = headerObj.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(0, 50f);

        Image hImg = headerObj.GetComponent<Image>();
        hImg.color = new Color(0.12f, 0.16f, 0.24f, 1f);

        // Header Title
        GameObject hTitleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Outline));
        hTitleObj.transform.SetParent(headerObj.transform, false);
        Text hTitle = hTitleObj.GetComponent<Text>();
        hTitle.font = fontToUse;
        hTitle.fontSize = 22;
        hTitle.fontStyle = FontStyle.Bold;
        hTitle.alignment = TextAnchor.MiddleCenter;
        hTitle.text = "📜 HỒ SƠ ĐẠO HỮU 📜";
        hTitle.color = new Color(1f, 0.88f, 0.3f);
        hTitleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform htRt = hTitleObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.sizeDelta = Vector2.zero;

        // Close Button [✕]
        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(headerObj.transform, false);
        RectTransform cbRt = closeBtnObj.GetComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(1, 0.5f);
        cbRt.anchorMax = new Vector2(1, 0.5f);
        cbRt.pivot = new Vector2(1, 0.5f);
        cbRt.anchoredPosition = new Vector2(-10, 0);
        cbRt.sizeDelta = new Vector2(34, 34);

        Image cbImg = closeBtnObj.GetComponent<Image>();
        cbImg.color = new Color(0.8f, 0.2f, 0.2f, 0.85f);
        Button closeBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeTxt = closeTxtObj.GetComponent<Text>();
        closeTxt.font = fontToUse;
        closeTxt.fontSize = 18;
        closeTxt.fontStyle = FontStyle.Bold;
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeTxt.text = "✕";
        closeTxt.color = Color.white;
        RectTransform ctxtRt = closeTxtObj.GetComponent<RectTransform>();
        ctxtRt.anchorMin = Vector2.zero;
        ctxtRt.anchorMax = Vector2.one;
        ctxtRt.sizeDelta = Vector2.zero;

        // 7. SCROLL VIEW (VÙNG CUỘN ĐẢM BẢO KHÔNG BAO GIỜ BỊ TRÔI NGOÀI PANEL)
        GameObject svObj = new GameObject("DetailScrollView", typeof(RectTransform), typeof(ScrollRect));
        svObj.transform.SetParent(boxObj.transform, false);
        RectTransform svRt = svObj.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0);
        svRt.anchorMax = new Vector2(1, 1);
        svRt.offsetMin = new Vector2(16, 52); // Cách đáy 52px (chỗ nút Đóng Lại)
        svRt.offsetMax = new Vector2(-16, -54); // Cách đỉnh 54px (chỗ Header bar)

        ScrollRect sr = svObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.elasticity = 0.1f;
        sr.scrollSensitivity = 28f;

        // Viewport (Mặt nạ cắt xén RectMask2D kèm Image bắt Raycast để nhận diện lăn chuột / kéo)
        GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        vpObj.transform.SetParent(svObj.transform, false);
        RectTransform vpRt = vpObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = new Vector2(-14, 0); // Chừa 14px cho thanh Scrollbar
        
        Image vpImg = vpObj.GetComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0); // Trong suốt nhưng bắt Raycast cho ScrollRect
        vpImg.raycastTarget = true;

        // Content Area (Chứa các dòng chữ, bung chiều cao theo nội dung)
        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(vpObj.transform, false);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 16);
        vlg.spacing = 5f; // Khoảng cách đều giữa các dòng
        vlg.childControlWidth = true;
        vlg.childControlHeight = true; // QUAN TRỌNG: Bắt buộc true để áp dụng preferredHeight của từng text element
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        sr.viewport = vpRt;
        sr.content = contentRt;

        // Scrollbar (Thanh cuộn mỏng dọc bên phải)
        GameObject sbObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        sbObj.transform.SetParent(svObj.transform, false);
        RectTransform sbRt = sbObj.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1, 0);
        sbRt.anchorMax = new Vector2(1, 1);
        sbRt.pivot = new Vector2(1, 0.5f);
        sbRt.anchoredPosition = Vector2.zero;
        sbRt.sizeDelta = new Vector2(8, 0);

        Image sbImg = sbObj.GetComponent<Image>();
        sbImg.color = new Color(0f, 0f, 0f, 0.45f);

        // Sliding Area
        GameObject slideObj = new GameObject("SlidingArea", typeof(RectTransform));
        slideObj.transform.SetParent(sbObj.transform, false);
        RectTransform slideRt = slideObj.GetComponent<RectTransform>();
        slideRt.anchorMin = Vector2.zero;
        slideRt.anchorMax = Vector2.one;
        slideRt.sizeDelta = Vector2.zero;

        // Handle
        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(slideObj.transform, false);
        RectTransform hdlRt = handleObj.GetComponent<RectTransform>();
        hdlRt.anchorMin = Vector2.zero;
        hdlRt.anchorMax = Vector2.one;
        hdlRt.sizeDelta = Vector2.zero;

        Image hdlImg = handleObj.GetComponent<Image>();
        hdlImg.color = new Color(1f, 0.82f, 0.2f, 0.85f); // Tay cầm trượt vàng kim sắc nét

        Scrollbar sb = sbObj.GetComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        sb.handleRect = hdlRt;
        sb.targetGraphic = hdlImg;

        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        // --- Section 1: Thông Tin Bản Thân ---
        Text txtName = CreateLayoutText(contentObj.transform, "TxtName", 16, "🏷️ Đạo Hiệu: <color=#FFE666>Đạo Hữu</color>", fontToUse, 24f);
        Text txtRealm = CreateLayoutText(contentObj.transform, "TxtRealm", 16, "🌟 Cảnh Giới: <color=#66FFB2>Luyện Khí Tầng 1</color>", fontToUse, 24f);
        Text txtTuVi = CreateLayoutText(contentObj.transform, "TxtTuVi", 16, "🔮 Tu Vi: <color=#66CCFF>0 / 100</color> (Tiến độ: <color=#FFE666>0.0%</color>)", fontToUse, 24f);
        Text txtCoDuyen = CreateLayoutText(contentObj.transform, "TxtCoDuyen", 15, "✨ Cơ Duyên: <color=#FFAA33>0 Điểm</color>", fontToUse, 22f);
        Text txtLinhThach = CreateLayoutText(contentObj.transform, "TxtLinhThach", 15, "💎 Linh Thạch: <color=#66CCFF>0</color>", fontToUse, 22f);

        // Đường phân cách 1
        CreateLayoutDivider(contentObj.transform, "Divider1");

        // --- Section 2: Chỉ Số Sinh Tồn & Chiến Đấu ---
        Text txtHp = CreateLayoutText(contentObj.transform, "TxtHp", 15, "❤️ Sinh Lực: <color=#55FF55>1,000 / 1,000</color>", fontToUse, 22f);
        Text txtMp = CreateLayoutText(contentObj.transform, "TxtMp", 15, "🔷 Linh Lực: <color=#66B2FF>100 / 100</color> (+10.0/s hồi phục)", fontToUse, 22f);
        Text txtMultiplier = CreateLayoutText(contentObj.transform, "TxtMultiplier", 15, "⚡ Khí Tức Tiên Thiên: <color=#FFDD44>+0.0%</color> (Hệ số x1.00)", fontToUse, 22f);
        Text txtEquipSwords = CreateLayoutText(contentObj.transform, "TxtEquipSwords", 15, "🗡️ Phi Kiếm Đang Xuất Trận: <color=#FFE666>1/4 Thanh</color>", fontToUse, 22f);

        // Đường phân cách 2
        CreateLayoutDivider(contentObj.transform, "Divider2");

        // --- Section 3: Chỉ Số Công Pháp Nâng Cấp (Kiếm Ý, Linh Lực, Tụ Linh, Thần Thức) ---
        Text txtTitleUpgrades = CreateLayoutText(contentObj.transform, "TxtTitleUpgrades", 15, "⚡ <b>CHỈ SỐ CÔNG PHÁP NÂNG CẤP:</b>", fontToUse, 22f);
        txtTitleUpgrades.color = new Color(1f, 0.85f, 0.35f);

        // 1. Kiếm Ý (Thứ tự đúng như yêu cầu: Kiếm Ý, Linh Lực, Tụ Linh, Thần Thức)
        Text txtUpgradeKY = CreateLayoutText(contentObj.transform, "TxtUpgradeKY", 14, "• 🗡️ <color=#FF8866>Kiếm Ý</color>: Cấp 1  ➔  Chỉ số: <color=#FFE666>+15.0 Sát thương Kiếm</color>", fontToUse, 22f);
        // 2. Linh Lực
        Text txtUpgradeLL = CreateLayoutText(contentObj.transform, "TxtUpgradeLL", 14, "• 🔷 <color=#66CCFF>Linh Lực</color>: Cấp 1  ➔  Chỉ số: <color=#66CCFF>100 MP</color> (+10.0/s hồi phục)", fontToUse, 22f);
        // 3. Tụ Linh
        Text txtUpgradeTL = CreateLayoutText(contentObj.transform, "TxtUpgradeTL", 14, "• ✨ <color=#88FF88>Tụ Linh</color>: Cấp 1  ➔  Chỉ số: <color=#88FF88>+15.0% Thu nạp Linh Thạch</color>", fontToUse, 22f);
        // 4. Thần Thức
        Text txtUpgradeTT = CreateLayoutText(contentObj.transform, "TxtUpgradeTT", 14, "• 👁️ <color=#CC88FF>Thần Thức</color>: Cấp 1  ➔  Chỉ số: <color=#CC88FF>+12.0% Tốc độ xuất chiêu</color>", fontToUse, 22f);

        // 8. Bottom Close Button
        GameObject bCloseObj = new GameObject("BottomCloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        bCloseObj.transform.SetParent(boxObj.transform, false);
        RectTransform bcbRt = bCloseObj.GetComponent<RectTransform>();
        bcbRt.anchorMin = new Vector2(0.5f, 0);
        bcbRt.anchorMax = new Vector2(0.5f, 0);
        bcbRt.pivot = new Vector2(0.5f, 0);
        bcbRt.anchoredPosition = new Vector2(0, 10);
        bcbRt.sizeDelta = new Vector2(150, 34);

        Image bcbImg = bCloseObj.GetComponent<Image>();
        bcbImg.color = new Color(0.18f, 0.24f, 0.35f, 0.95f);
        Button bcbBtn = bCloseObj.GetComponent<Button>();

        GameObject bcbTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        bcbTxtObj.transform.SetParent(bCloseObj.transform, false);
        Text bcbTxt = bcbTxtObj.GetComponent<Text>();
        bcbTxt.font = fontToUse;
        bcbTxt.fontSize = 15;
        bcbTxt.fontStyle = FontStyle.Bold;
        bcbTxt.alignment = TextAnchor.MiddleCenter;
        bcbTxt.text = "ĐÓNG LẠI";
        bcbTxt.color = Color.white;
        RectTransform bcTxtRt = bcbTxtObj.GetComponent<RectTransform>();
        bcTxtRt.anchorMin = Vector2.zero;
        bcTxtRt.anchorMax = Vector2.one;
        bcTxtRt.sizeDelta = Vector2.zero;

        // 9. Gán tất cả References vào CharacterDetailModalUI
        CharacterDetailModalUI ui = modalObj.GetComponent<CharacterDetailModalUI>();
        ui.canvasGroup = cg;
        ui.modalBox = boxRt;
        ui.scrollRect = sr;
        ui.closeButton = closeBtn;
        ui.closeOverlayButton = overBtn;
        ui.bottomCloseButton = bcbBtn;

        ui.txtName = txtName;
        ui.txtRealm = txtRealm;
        ui.txtTuVi = txtTuVi;
        ui.txtCoDuyen = txtCoDuyen;
        ui.txtLinhThach = txtLinhThach;

        ui.txtHp = txtHp;
        ui.txtMp = txtMp;
        ui.txtMultiplier = txtMultiplier;
        ui.txtEquipSwords = txtEquipSwords;

        ui.txtUpgradeKiemY = txtUpgradeKY;
        ui.txtUpgradeLinhLuc = txtUpgradeLL;
        ui.txtUpgradeTuLinh = txtUpgradeTL;
        ui.txtUpgradeThanThuc = txtUpgradeTT;

        modalObj.SetActive(false);
        modalObj.transform.SetAsLastSibling();

        EditorUtility.SetDirty(modalObj);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = modalObj;
        EditorGUIUtility.PingObject(modalObj);
        Debug.Log("<color=green>✅ [CharacterDetail] ĐÃ NÂNG CẤP SCROLL VIEW THÀNH CÔNG! Nội dung được bao bọc hoàn toàn bên trong Viewport có mặt nạ RectMask2D, hỗ trợ lăn chuột / kéo mượt mà.</color>");
    }

    private static void SetupCharacterFrame(GameObject canvasObj, Font fontToUse)
    {
        Transform charFrame = canvasObj.transform.Find("CharacterFrame");
        if (charFrame == null) return;

        // Gắn component CharacterFrameButton
        CharacterFrameButton btn = charFrame.GetComponent<CharacterFrameButton>();
        if (btn == null)
        {
            btn = charFrame.gameObject.AddComponent<CharacterFrameButton>();
            EditorUtility.SetDirty(charFrame.gameObject);
        }

        // Đảm bảo Image trên CharacterFrame bật raycastTarget
        Image img = charFrame.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        // Bỏ "Loại: Nhân Tộc" khỏi InfoText nếu còn tồn tại
        Transform infoObj = charFrame.Find("InfoText");
        if (infoObj != null)
        {
            Text infoTxt = infoObj.GetComponent<Text>();
            if (infoTxt != null && infoTxt.text.Contains("Loại:"))
            {
                infoTxt.text = infoTxt.text.Replace("\nLoại: Nhân Tộc", "").Replace("Loại: Nhân Tộc", "").TrimEnd();
                EditorUtility.SetDirty(infoTxt);
            }
        }

        // Thêm dòng hint nhỏ "[ Nhấn xem chi tiết ]"
        Transform hintObj = charFrame.Find("DetailHintText");
        if (hintObj == null)
        {
            GameObject hint = new GameObject("DetailHintText", typeof(RectTransform), typeof(Text));
            hint.transform.SetParent(charFrame, false);
            RectTransform hRt = hint.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0, 0);
            hRt.anchorMax = new Vector2(1, 0);
            hRt.pivot = new Vector2(0.5f, 0);
            hRt.anchoredPosition = new Vector2(45, 6);
            hRt.sizeDelta = new Vector2(180, 18);

            Text hTxt = hint.GetComponent<Text>();
            hTxt.font = fontToUse;
            hTxt.fontSize = 11;
            hTxt.fontStyle = FontStyle.Italic;
            hTxt.alignment = TextAnchor.MiddleLeft;
            hTxt.text = "🔍 [Nhấn xem chi tiết]";
            hTxt.color = new Color(0.6f, 0.9f, 1f, 0.85f);
            hTxt.raycastTarget = false;

            EditorUtility.SetDirty(charFrame.gameObject);
        }
    }

    private static Text CreateLayoutText(Transform parent, string name, int fontSize, string defaultText, Font font, float preferredHeight)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        txtObj.transform.SetParent(parent, false);

        LayoutElement le = txtObj.GetComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.minHeight = preferredHeight;
        le.flexibleWidth = 1f;

        Text txt = txtObj.GetComponent<Text>();
        txt.font = font;
        txt.fontSize = fontSize;
        txt.text = defaultText;
        txt.color = Color.white;
        txt.raycastTarget = false;
        txt.alignment = TextAnchor.MiddleLeft;
        return txt;
    }

    private static void CreateLayoutDivider(Transform parent, string name)
    {
        GameObject div = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        div.transform.SetParent(parent, false);

        LayoutElement le = div.GetComponent<LayoutElement>();
        le.preferredHeight = 2f;
        le.minHeight = 2f;
        le.flexibleWidth = 1f;

        Image img = div.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.18f);
        img.raycastTarget = false;
    }

    private static Image CreateBorder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Color color)
    {
        GameObject line = new GameObject(name, typeof(RectTransform), typeof(Image));
        line.transform.SetParent(parent, false);
        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;

        Image img = line.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static Font FindBestFont(GameObject canvasObj)
    {
        Text existingText = canvasObj.GetComponentInChildren<Text>(true);
        if (existingText != null && existingText.font != null)
        {
            return existingText.font;
        }

        try
        {
            Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (runtimeFont != null) return runtimeFont;
        }
        catch { }

        return Font.CreateDynamicFontFromOSFont("Arial", 16);
    }
}
#endif

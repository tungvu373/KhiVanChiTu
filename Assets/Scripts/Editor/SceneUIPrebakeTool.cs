#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MASTER TOOL SẮP XẾP LẠI TOÀN BỘ HUD & PREBAKE 100% GIAO DIỆN VÀO SCENE.
/// Đáp ứng yêu cầu: "sắp xếp lại HUD, UI của tôi hãy tạo sẵn tất cả ở scene chứ không chờ khởi chạy".
/// - Sắp xếp Top HUD (CharacterFrame, 4 nút chức năng Đạo Vận, Bí Cảnh, Túi Đồ, Nhiệm Vụ, 2 Ví tiền, Cài Đặt) không bao giờ bị đè.
/// - Tạo sẵn SecretRealmModal, SecretRealmPlayRoot, SecretRealmResultModal trực tiếp trong Hierarchy của Scene.
/// - Đặt tất cả các Modal ở trạng thái inactive (activeSelf = false) để màn hình Scene view và mở game luôn thoáng đãng.
/// - Lưu Scene trực tiếp và kiểm định.
/// </summary>
public static class SceneUIPrebakeTool
{
    private static Font DefaultFont
    {
        get
        {
            var f = UiTextStyle.BodyFont;
            if (f != null) return f;
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) return f;
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    [MenuItem("TuTien/UI/🚀 Sắp Xếp HUD & Tạo Sẵn Tất Cả UI Vào Scene")]
    public static void PrebakeAllUIToScene()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas not found in active scene!");
            return;
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("❌ Canvas/MobileUIRoot not found in active scene!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas, "Prebake All UI To Scene");

        Debug.Log("🛠️ [PrebakeUI] 1. Bắt đầu sắp xếp lại Top HUD...");
        ArrangeTopHUD(mobileRoot);

        Debug.Log("🛠️ [PrebakeUI] 2. Tạo sẵn SecretRealmModal (Bí Cảnh Hub) trong Scene...");
        PrebakeSecretRealmModal(canvas.transform);

        Debug.Log("🛠️ [PrebakeUI] 3. Tạo sẵn SecretRealmPlayRoot (Đấu Trường 5 Mini-Game) trong Scene...");
        PrebakeSecretRealmPlayRoot(canvas.transform);

        Debug.Log("🛠️ [PrebakeUI] 4. Tạo sẵn SecretRealmResultModal (Chiến Báo Phát Thưởng) trong Scene...");
        PrebakeSecretRealmResultModal(canvas.transform);

        Debug.Log("🛠️ [PrebakeUI] 5. Đóng tất cả các Modal trong Scene về trạng thái ẩn (activeSelf = false)...");
        HideAllModalsInScene(canvas.transform);

        // Đảm bảo các component cần thiết gắn vào Canvas
        if (canvas.GetComponent<SecretRealmService>() == null) canvas.AddComponent<SecretRealmService>();
        if (canvas.GetComponent<SecretRealmModalUI>() == null) canvas.AddComponent<SecretRealmModalUI>();
        if (canvas.GetComponent<SecretRealmPlayController>() == null) canvas.AddComponent<SecretRealmPlayController>();

        // Lưu scene
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log($"🎉 [PrebakeUI] HOÀN TẤT 100%! Đã sắp xếp HUD và tạo sẵn toàn bộ UI trong Scene: {activeScene.path}");
    }

    private static void ArrangeTopHUD(Transform mobileRoot)
    {
        // 1. CharacterFrame (Top Left)
        var charFrame = mobileRoot.Find("CharacterFrame");
        if (charFrame != null)
        {
            var rt = charFrame.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -6f);
            rt.sizeDelta = new Vector2(215f, 50f);
        }

        // 2. Nút ĐẠO VẬN (BtnHUDDaoAn)
        var btnDaoAn = GetOrCreateChild(mobileRoot, "BtnHUDDaoAn");
        ConfigureTopButton(btnDaoAn, new Vector2(235f, -9f), new Vector2(104f, 44f),
            "<b><color=#FFE666>☯ ĐẠO VẬN</color></b>",
            new Color(0.12f, 0.15f, 0.22f, 0.95f),
            new Color(1f, 0.85f, 0.3f, 0.9f));
        if (btnDaoAn.GetComponent<DaoAnButtonHandler>() == null) btnDaoAn.AddComponent<DaoAnButtonHandler>();

        // 3. Nút BÍ CẢNH (BtnHUDBiCanh)
        var btnBiCanh = GetOrCreateChild(mobileRoot, "BtnHUDBiCanh");
        ConfigureTopButton(btnBiCanh, new Vector2(348f, -9f), new Vector2(104f, 44f),
            "<b><color=#E699FF>🌌 BÍ CẢNH</color></b>",
            new Color(0.15f, 0.10f, 0.25f, 0.95f),
            new Color(0.85f, 0.45f, 1f, 0.9f));
        if (btnBiCanh.GetComponent<BiCanhButtonHandler>() == null) btnBiCanh.AddComponent<BiCanhButtonHandler>();

        // 4. Nút TÚI ĐỒ (BtnHUDStorage)
        var btnStorage = GetOrCreateChild(mobileRoot, "BtnHUDStorage");
        ConfigureTopButton(btnStorage, new Vector2(461f, -9f), new Vector2(100f, 44f),
            "<b><color=#80DFFF>🎒 TÚI ĐỒ</color></b>",
            new Color(0.10f, 0.15f, 0.24f, 0.95f),
            new Color(0.3f, 0.8f, 1f, 0.9f));
        if (btnStorage.GetComponent<StorageButtonHandler>() == null) btnStorage.AddComponent<StorageButtonHandler>();

        // 5. Nút NHIỆM VỤ (BtnHUDAchievement)
        var btnAch = GetOrCreateChild(mobileRoot, "BtnHUDAchievement");
        ConfigureTopButton(btnAch, new Vector2(570f, -9f), new Vector2(108f, 44f),
            "<b><color=#FFB84D>🏆 NHIỆM VỤ</color></b>",
            new Color(0.18f, 0.14f, 0.10f, 0.95f),
            new Color(1f, 0.7f, 0.2f, 0.9f));
        if (btnAch.GetComponent<AchievementButtonHandler>() == null) btnAch.AddComponent<AchievementButtonHandler>();

        // 6. Căn chỉnh tiền tệ & Pause bên phải
        var topBar = mobileRoot.Find("TopBar");
        if (topBar != null)
        {
            var bgLt = topBar.Find("BG_LinhThach");
            var txtLt = topBar.Find("txt_LinhThach");
            if (bgLt != null)
            {
                var rt = bgLt.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-235f, 0f);
                rt.sizeDelta = new Vector2(110f, 34f);
            }
            if (txtLt != null)
            {
                var rt = txtLt.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-235f, 0f);
                rt.sizeDelta = new Vector2(110f, 34f);
            }

            var bgCd = topBar.Find("BG_CoDuyen");
            var txtCd = topBar.Find("txt_CoDuyen");
            if (bgCd != null)
            {
                var rt = bgCd.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-120f, 0f);
                rt.sizeDelta = new Vector2(105f, 34f);
            }
            if (txtCd != null)
            {
                var rt = txtCd.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-120f, 0f);
                rt.sizeDelta = new Vector2(105f, 34f);
            }
        }

        // 7. PauseButton (Far Right)
        var pauseBtn = mobileRoot.Find("PauseButton");
        if (pauseBtn != null)
        {
            var rt = pauseBtn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-10f, -9f);
            rt.sizeDelta = new Vector2(46f, 44f);
        }
    }

    private static void ConfigureTopButton(GameObject go, Vector2 pos, Vector2 size, string text, Color bgColor, Color outlineColor)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.GetComponent<Button>();
        if (btn == null) btn = go.AddComponent<Button>();

        var ol = go.GetComponent<Outline>();
        if (ol == null) ol = go.AddComponent<Outline>();
        ol.effectColor = outlineColor;
        ol.effectDistance = new Vector2(1.2f, -1.2f);

        var txtGo = go.transform.Find("Text")?.gameObject;
        if (txtGo == null)
        {
            txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
        }
        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        var txt = txtGo.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = 12;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.supportRichText = true;
        txt.text = text;

        go.SetActive(true);
        EditorUtility.SetDirty(go);
    }

    private static void PrebakeSecretRealmModal(Transform canvas)
    {
        var existing = canvas.Find("SecretRealmModal");
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
            return;
        }

        // 1. Overlay
        var modalGo = new GameObject("SecretRealmModal", typeof(RectTransform), typeof(Image), typeof(Button));
        modalGo.transform.SetParent(canvas, false);
        var mrt = modalGo.GetComponent<RectTransform>();
        mrt.anchorMin = Vector2.zero;
        mrt.anchorMax = Vector2.one;
        mrt.offsetMin = mrt.offsetMax = Vector2.zero;
        modalGo.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.92f);

        // 2. ModalBox
        var box = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(SafeAreaFitter));
        box.transform.SetParent(modalGo.transform, false);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        box.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.15f, 0.99f);
        var bol = box.GetComponent<Outline>();
        bol.effectColor = new Color(0.85f, 0.45f, 1f, 0.5f);
        bol.effectDistance = new Vector2(1f, -1f);

        // 3. Top Border
        var border = new GameObject("BorderTop", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(box.transform, false);
        var brdRt = border.GetComponent<RectTransform>();
        brdRt.anchorMin = new Vector2(0f, 1f);
        brdRt.anchorMax = new Vector2(1f, 1f);
        brdRt.offsetMin = new Vector2(0f, -4f);
        brdRt.offsetMax = Vector2.zero;
        border.GetComponent<Image>().color = new Color(0.85f, 0.45f, 1f, 1f);

        // 4. Close X
        var btnX = new GameObject("BtnCloseX", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnX.transform.SetParent(box.transform, false);
        var xrt = btnX.GetComponent<RectTransform>();
        xrt.anchorMin = xrt.anchorMax = Vector2.one;
        xrt.offsetMin = new Vector2(-52f, -52f);
        xrt.offsetMax = new Vector2(-12f, -12f);
        btnX.GetComponent<Image>().color = new Color(0.2f, 0.15f, 0.25f, 0.95f);
        var xtxt = CreateText(btnX.transform, "X", "X", 20, FontStyle.Bold, Vector2.zero, Color.white);
        xtxt.alignment = TextAnchor.MiddleCenter;

        // 5. Title & Subtitle
        var tGo = CreateText(box.transform, "Title", "🌌 KỲ NGỘ BÍ CẢNH THƯỢNG CỔ", 22, FontStyle.Bold, new Vector2(-20f, -28f), new Color(0.95f, 0.8f, 1f));
        var trt = tGo.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(20f, -44f);
        trt.offsetMax = new Vector2(-60f, -8f);
        tGo.alignment = TextAnchor.MiddleLeft;

        var stGo = CreateText(box.transform, "SubTitle", "Thí Luyện Dị Giới - Thu Thập Trân Bảo & Đốt Cháy Tiến Trình", 13, FontStyle.Normal, new Vector2(0f, -56f), new Color(0.7f, 0.75f, 0.85f));
        var strt = stGo.GetComponent<RectTransform>();
        strt.anchorMin = new Vector2(0f, 1f);
        strt.anchorMax = new Vector2(1f, 1f);
        strt.offsetMin = new Vector2(20f, -68f);
        strt.offsetMax = new Vector2(-20f, -44f);
        stGo.alignment = TextAnchor.MiddleLeft;

        // 6. StatsBar
        var statsBar = new GameObject("StatsBar", typeof(RectTransform), typeof(Image));
        statsBar.transform.SetParent(box.transform, false);
        var srt = statsBar.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.offsetMin = new Vector2(16f, -132f);
        srt.offsetMax = new Vector2(-16f, -74f);
        statsBar.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.96f);

        var walletTxt = CreateText(statsBar.transform, "Wallet", "✨ Ví Cơ Duyên: 0 CD", 15, FontStyle.Bold, Vector2.zero, new Color(1f, 0.85f, 0.3f));
        var wrt = walletTxt.GetComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0f, 0f);
        wrt.anchorMax = new Vector2(0.5f, 1f);
        wrt.offsetMin = new Vector2(14f, 0f);
        wrt.offsetMax = new Vector2(-10f, 0f);
        walletTxt.alignment = TextAnchor.MiddleLeft;

        var freeTxt = CreateText(statsBar.transform, "FreeAttempts", "Lượt Miễn Phí: 1/1", 15, FontStyle.Bold, Vector2.zero, new Color(0.3f, 1f, 0.5f));
        var frt = freeTxt.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0.5f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = new Vector2(10f, 0f);
        frt.offsetMax = new Vector2(-14f, 0f);
        freeTxt.alignment = TextAnchor.MiddleRight;

        // 7. ScrollView & 5 Cards
        var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(box.transform, false);
        var scrt = scrollGo.GetComponent<RectTransform>();
        scrt.anchorMin = Vector2.zero;
        scrt.anchorMax = Vector2.one;
        scrt.offsetMin = new Vector2(16f, 76f);
        scrt.offsetMax = new Vector2(-16f, -140f);
        scrollGo.GetComponent<Image>().color = Color.clear;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollGo.transform, false);
        var vprt = viewport.GetComponent<RectTransform>();
        vprt.anchorMin = Vector2.zero;
        vprt.anchorMax = Vector2.one;
        vprt.offsetMin = vprt.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(4, 4, 6, 6);

        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sRect = scrollGo.GetComponent<ScrollRect>();
        sRect.content = crt;
        sRect.viewport = vprt;
        sRect.horizontal = false;
        sRect.vertical = true;
        sRect.movementType = ScrollRect.MovementType.Clamped;

        // Sinh 5 cards
        CreateRealmCard(content.transform, "Card_HordeSurvival", "⚔️ VẠN MA TRIỀU DÂNG (Sinh Tồn 30s)", "Yêu ma tràn ngập 4 phương! Dồn toàn lực tuyệt kỹ quét sạch bầy quái trước khi hết giờ.", "🎁 Thưởng: Rương Thảo Dược Bội Thu", new Color(0.9f, 0.3f, 0.3f));
        CreateRealmCard(content.transform, "Card_BarrierBreak", "🗿 TRẢM PHÁ CẤM CHẾ (Dồn Sát Thương 20s)", "Thạch bi phù văn viễn cổ chứa bảo vật. Đập vỡ cấm chế nhận quặng quý và phôi pháp khí.", "🎁 Thưởng: Quặng Tinh Thiết & Phôi Pháp Khí", new Color(0.2f, 0.65f, 1f));
        CreateRealmCard(content.transform, "Card_DanCauldron", "🔮 ĐAN THẦN KỲ DUYÊN (Tam Đỉnh Tầm Bảo)", "Lạc vào Đan Các cổ tích! Giải mã sấm ngôn và chọn 1 trong 3 đỉnh đan để nổ hũ thần đan.", "🎁 Thưởng: Thượng Cổ Tuyệt Phẩm Đan, Thảo Dược & CD", new Color(0.85f, 0.5f, 1f));
        CreateRealmCard(content.transform, "Card_LightningDodge", "⚡ VƯỢT LÔI TRÌ (Né Sét Nhặt Tinh Cầu 25s)", "Sấm sét thiên kiếp giáng liên tục! Nhanh tay nhặt các hạt Lôi Tinh Cầu phát sáng trên sàn.", "🎁 Thưởng: Tu Vi Thuần Khiết & Tàn Quyển Trận", new Color(0.3f, 0.9f, 0.9f));
        CreateRealmCard(content.transform, "Card_SwordTomb", "🗡️ KIẾM CHỦNG CỔ THẦN (Rút Kiếm Cực Phẩm)", "3 lượt rút kiếm cổ! Canh đúng vạch Hoàng Kim ở giữa để rút ngay Phi Kiếm Bậc Cao.", "🎁 Thưởng: Phi Kiếm Cấp 5 -> Cấp 8 thẳng vào kho", new Color(1f, 0.8f, 0.2f));

        // 8. Close Button
        var closeBottom = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        closeBottom.transform.SetParent(box.transform, false);
        var cbrt = closeBottom.GetComponent<RectTransform>();
        cbrt.anchorMin = new Vector2(0.2f, 0f);
        cbrt.anchorMax = new Vector2(0.8f, 0f);
        cbrt.offsetMin = new Vector2(0f, 14f);
        cbrt.offsetMax = new Vector2(0f, 66f);
        closeBottom.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.32f, 0.95f);
        var cbol = closeBottom.GetComponent<Outline>();
        cbol.effectColor = new Color(0.85f, 0.45f, 1f, 0.7f);
        cbol.effectDistance = new Vector2(1f, -1f);
        var cbtxt = CreateText(closeBottom.transform, "Text", "ĐÓNG GIAO DIỆN", 15, FontStyle.Bold, Vector2.zero, Color.white);
        cbtxt.alignment = TextAnchor.MiddleCenter;

        modalGo.SetActive(false);
        EditorUtility.SetDirty(modalGo);
    }

    private static void CreateRealmCard(Transform parent, string cardName, string title, string desc, string reward, Color accentColor)
    {
        var card = new GameObject(cardName, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        var crt = card.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(0f, 130f);
        var le = card.GetComponent<LayoutElement>();
        le.minHeight = 130f;
        le.preferredHeight = 130f;
        card.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
        var ol = card.GetComponent<Outline>();
        ol.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
        ol.effectDistance = new Vector2(1f, -1f);

        var tTxt = CreateText(card.transform, "Title", title, 15, FontStyle.Bold, Vector2.zero, accentColor);
        var trt = tTxt.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(14f, -32f);
        trt.offsetMax = new Vector2(-130f, -4f);
        tTxt.alignment = TextAnchor.MiddleLeft;

        var dTxt = CreateText(card.transform, "Desc", desc, 12, FontStyle.Normal, Vector2.zero, new Color(0.85f, 0.85f, 0.9f));
        var drt = dTxt.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0f, 1f);
        drt.anchorMax = new Vector2(1f, 1f);
        drt.offsetMin = new Vector2(14f, -76f);
        drt.offsetMax = new Vector2(-130f, -34f);
        dTxt.alignment = TextAnchor.UpperLeft;

        var rTxt = CreateText(card.transform, "Reward", reward, 11, FontStyle.Italic, Vector2.zero, new Color(1f, 0.85f, 0.35f));
        var rrt = rTxt.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0f, 0f);
        rrt.anchorMax = new Vector2(1f, 0f);
        rrt.offsetMin = new Vector2(14f, 6f);
        rrt.offsetMax = new Vector2(-130f, 44f);
        rTxt.alignment = TextAnchor.MiddleLeft;

        var btnGo = new GameObject("BtnEnter", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(card.transform, false);
        var brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1f, 0.5f);
        brt.anchorMax = new Vector2(1f, 0.5f);
        brt.anchoredPosition = new Vector2(-68f, 0f);
        brt.sizeDelta = new Vector2(116f, 50f);
        btnGo.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.25f, 0.95f);
        var bol = btnGo.GetComponent<Outline>();
        bol.effectColor = new Color(0.4f, 1f, 0.6f, 0.8f);
        bol.effectDistance = new Vector2(1f, -1f);

        var btxt = CreateText(btnGo.transform, "Text", "VÀO ẢI", 13, FontStyle.Bold, Vector2.zero, Color.white);
        btxt.alignment = TextAnchor.MiddleCenter;
    }

    private static void PrebakeSecretRealmPlayRoot(Transform canvas)
    {
        var existing = canvas.Find("SecretRealmPlayRoot");
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
            return;
        }

        var root = new GameObject("SecretRealmPlayRoot", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.98f);

        // Header Panel
        var headerGo = new GameObject("HeaderPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
        headerGo.transform.SetParent(root.transform, false);
        var hrt = headerGo.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.83f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        headerGo.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f, 0.98f);
        var hol = headerGo.GetComponent<Outline>();
        hol.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.8f);
        hol.effectDistance = new Vector2(0f, -2f);

        var btnQuit = new GameObject("BtnQuit", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnQuit.transform.SetParent(headerGo.transform, false);
        var qrt = btnQuit.GetComponent<RectTransform>();
        qrt.anchorMin = qrt.anchorMax = new Vector2(0f, 1f);
        qrt.anchoredPosition = new Vector2(50f, -22f);
        qrt.sizeDelta = new Vector2(76f, 32f);
        btnQuit.GetComponent<Image>().color = new Color(0.6f, 0.15f, 0.2f, 0.95f);
        var qtxt = CreateText(btnQuit.transform, "Txt", "❌ RỜI", 12, FontStyle.Bold, Vector2.zero, Color.white);
        qtxt.rectTransform.sizeDelta = qrt.sizeDelta;

        var titleTxt = CreateText(headerGo.transform, "TitleText", "BÍ CẢNH THƯỢNG CỔ", 15, FontStyle.Bold, new Vector2(-10f, -22f), Color.yellow);
        titleTxt.rectTransform.sizeDelta = new Vector2(210f, 28f);

        var timerTxt = CreateText(headerGo.transform, "TimerText", "⏱️ 30.0s", 15, FontStyle.Bold, new Vector2(135f, -22f), Color.cyan);
        timerTxt.rectTransform.sizeDelta = new Vector2(90f, 28f);

        var ruleBox = new GameObject("RuleCard", typeof(RectTransform), typeof(Image), typeof(Outline));
        ruleBox.transform.SetParent(headerGo.transform, false);
        var rrt = ruleBox.GetComponent<RectTransform>();
        rrt.anchoredPosition = new Vector2(0f, -62f);
        rrt.sizeDelta = new Vector2(350f, 44f);
        ruleBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.9f);
        var rol = ruleBox.GetComponent<Outline>();
        rol.effectColor = new Color(0.4f, 0.6f, 0.85f, 0.4f);
        rol.effectDistance = new Vector2(1f, -1f);

        var ruleTxt = CreateText(ruleBox.transform, "TxtRule", "Hướng dẫn...", 11, FontStyle.Normal, Vector2.zero, Color.white);
        ruleTxt.rectTransform.sizeDelta = new Vector2(340f, 40f);
        ruleTxt.alignment = TextAnchor.MiddleLeft;

        var scTitle = CreateText(headerGo.transform, "ScoreTitle", "TIẾN ĐỘ:", 12, FontStyle.Bold, new Vector2(-60f, -95f), Color.white);
        scTitle.rectTransform.sizeDelta = new Vector2(140f, 20f);
        var scVal = CreateText(headerGo.transform, "ScoreValue", "0 / 50", 14, FontStyle.Bold, new Vector2(80f, -95f), Color.yellow);
        scVal.rectTransform.sizeDelta = new Vector2(140f, 20f);

        // Arena Container
        var areaGo = new GameObject("ArenaContainer", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        areaGo.transform.SetParent(root.transform, false);
        var art = areaGo.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 0.14f);
        art.anchorMax = new Vector2(1f, 0.83f);
        art.offsetMin = art.offsetMax = Vector2.zero;
        areaGo.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 1f);

        // Footer Panel
        var footerGo = new GameObject("FooterPanel", typeof(RectTransform), typeof(Image));
        footerGo.transform.SetParent(root.transform, false);
        var frt = footerGo.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 0.14f);
        frt.offsetMin = frt.offsetMax = Vector2.zero;
        footerGo.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.15f, 0.98f);

        var pBarBg = new GameObject("MainProgressBarBg", typeof(RectTransform), typeof(Image));
        pBarBg.transform.SetParent(footerGo.transform, false);
        var prt = pBarBg.GetComponent<RectTransform>();
        prt.anchoredPosition = new Vector2(0f, 32f);
        prt.sizeDelta = new Vector2(340f, 18f);
        pBarBg.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 1f);

        var pFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        pFill.transform.SetParent(pBarBg.transform, false);
        var firt = pFill.GetComponent<RectTransform>();
        firt.anchorMin = Vector2.zero;
        firt.anchorMax = Vector2.one;
        firt.offsetMin = firt.offsetMax = Vector2.zero;
        var fImg = pFill.GetComponent<Image>();
        fImg.color = new Color(0.2f, 0.85f, 0.45f, 1f);
        fImg.type = Image.Type.Filled;
        fImg.fillMethod = Image.FillMethod.Horizontal;
        fImg.fillAmount = 0f;

        var pTxt = CreateText(pBarBg.transform, "TxtProgress", "TIẾN ĐỘ: 0%", 11, FontStyle.Bold, Vector2.zero, Color.white);
        pTxt.rectTransform.anchorMin = Vector2.zero;
        pTxt.rectTransform.anchorMax = Vector2.one;
        pTxt.rectTransform.offsetMin = pTxt.rectTransform.offsetMax = Vector2.zero;

        root.SetActive(false);
        EditorUtility.SetDirty(root);
    }

    private static void PrebakeSecretRealmResultModal(Transform canvas)
    {
        var existing = canvas.Find("SecretRealmResultModal");
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
            return;
        }

        var go = new GameObject("SecretRealmResultModal", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(go.transform, false);
        var crt = card.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(340f, 440f);
        crt.anchoredPosition = Vector2.zero;
        card.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.98f);
        var ol = card.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.82f, 0.2f, 0.95f);
        ol.effectDistance = new Vector2(2f, -2f);

        CreateText(card.transform, "TitleText", "🏆 CHIẾN BÁO BÍ CẢNH THẮNG LỢI!", 16, FontStyle.Bold, new Vector2(0f, 185f), Color.yellow);
        CreateText(card.transform, "ScoreText", "Điểm số hoàn thành: 100", 13, FontStyle.Bold, new Vector2(0f, 145f), Color.cyan);

        var rBox = new GameObject("RewardBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        rBox.transform.SetParent(card.transform, false);
        var rrt = rBox.GetComponent<RectTransform>();
        rrt.anchoredPosition = new Vector2(0f, 15f);
        rrt.sizeDelta = new Vector2(310f, 210f);
        rBox.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        var rol = rBox.GetComponent<Outline>();
        rol.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.5f);
        rol.effectDistance = new Vector2(1f, -1f);

        var rtxt = CreateText(rBox.transform, "RewardDetailsText", "Phần thưởng...", 12, FontStyle.Normal, Vector2.zero, Color.white);
        rtxt.rectTransform.sizeDelta = new Vector2(290f, 195f);
        rtxt.alignment = TextAnchor.UpperLeft;

        var btnConfirm = new GameObject("BtnConfirm", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnConfirm.transform.SetParent(card.transform, false);
        var brt = btnConfirm.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0f, -170f);
        brt.sizeDelta = new Vector2(270f, 50f);
        btnConfirm.GetComponent<Image>().color = new Color(0.18f, 0.65f, 0.35f, 0.98f);
        var bol = btnConfirm.GetComponent<Outline>();
        bol.effectColor = new Color(1f, 0.9f, 0.4f, 0.9f);
        bol.effectDistance = new Vector2(1.5f, -1.5f);

        var btxt = CreateText(btnConfirm.transform, "Txt", "🏆 XÁC NHẬN THU NHẬN", 14, FontStyle.Bold, Vector2.zero, Color.white);
        btxt.rectTransform.sizeDelta = brt.sizeDelta;

        go.SetActive(false);
        EditorUtility.SetDirty(go);
    }

    private static void HideAllModalsInScene(Transform canvas)
    {
        string[] modalNames = {
            "SecretRealmModal", "SecretRealmPlayRoot", "SecretRealmResultModal",
            "DaoAnModal", "StorageModalUI", "AchievementModalUI", "MilestoneModalUI",
            "TribulationModalUI", "CharacterDetailModal", "PauseMenuModal",
            "DismantleConfirmModalUI", "BodyTalentDetailModal", "RightPanel", "ShopPanel"
        };

        foreach (var mName in modalNames)
        {
            var tr = canvas.Find(mName);
            if (tr != null)
            {
                tr.gameObject.SetActive(false);
                EditorUtility.SetDirty(tr.gameObject);
            }
        }
    }

    private static GameObject GetOrCreateChild(Transform parent, string childName)
    {
        var tr = parent.Find(childName);
        if (tr != null) return tr.gameObject;
        var go = new GameObject(childName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text CreateText(Transform parent, string name, string val, int size, FontStyle style, Vector2 pos, Color col)
    {
        var tr = parent.Find(name);
        GameObject go = tr != null ? tr.gameObject : new GameObject(name, typeof(RectTransform), typeof(Text));
        if (tr == null) go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 30f);

        var txt = go.GetComponent<Text>();
        if (txt == null) txt = go.AddComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.color = col;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = val;
        return txt;
    }
}
#endif


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tự động tạo Nút Cài Đặt/Pause ở góc trên cùng bên phải và Panel Pause Game hoàn chỉnh trên Canvas.
/// Tích hợp đầy đủ: Tạm Dừng, Cài Đặt Âm Thanh, Trở Về Main Menu, Thoát Game, và Đồng Bộ Lưu Tiến Trình DataManager.
/// </summary>
[InitializeOnLoad]
public class PauseMenuBuilderTool
{
    static PauseMenuBuilderTool()
    {
        EditorApplication.delayCall += () =>
        {
            EnsurePauseMenuInScene(forceRebuild: false);
        };
    }

    [MenuItem("TuTien/UI/Tạo Nút & Panel Pause (Cài Đặt Game)")]
    public static void CreateOrSelectMenu()
    {
        EnsurePauseMenuInScene(forceRebuild: true);
    }

    public static void EnsurePauseMenuInScene(bool forceRebuild)
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
            Debug.LogWarning("[PauseMenuBuilder] Không tìm thấy Canvas trong Scene!");
            return;
        }

        Font fontToUse = FindBestFont(canvasObj);

        // 1. Kiểm tra PauseButton & PauseMenuModal cũ
        Transform existingBtn = canvasObj.transform.Find("PauseButton");
        Transform existingModal = canvasObj.transform.Find("PauseMenuModal");

        if (!forceRebuild && existingBtn != null && existingModal != null)
        {
            if (existingBtn.GetComponent<PauseButtonHandler>() == null)
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
            if (existingBtn != null) Undo.DestroyObjectImmediate(existingBtn.gameObject);
            if (existingModal != null) Undo.DestroyObjectImmediate(existingModal.gameObject);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. TẠO PAUSE BUTTON (BÊN PHẢI NGOÀI CÙNG PHÍA TRÊN)
        // ─────────────────────────────────────────────────────────────────────
        GameObject btnObj = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(PauseButtonHandler));
        Undo.RegisterCreatedObjectUndo(btnObj, "Tạo PauseButton");
        btnObj.transform.SetParent(canvasObj.transform, false);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1, 1);
        btnRt.anchorMax = new Vector2(1, 1);
        btnRt.pivot = new Vector2(1, 1);
        btnRt.anchoredPosition = new Vector2(-15, -10); // Góc trên cùng bên phải
        btnRt.sizeDelta = new Vector2(110, 36);

        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f); // Nền mực lam tiên hiệp
        btnImg.raycastTarget = true;

        Button btnComp = btnObj.GetComponent<Button>();
        ColorBlock cb = btnComp.colors;
        cb.normalColor = new Color(0.12f, 0.16f, 0.24f, 0.95f);
        cb.highlightedColor = new Color(0.22f, 0.30f, 0.42f, 1f);
        cb.pressedColor = new Color(0.08f, 0.11f, 0.16f, 1f);
        btnComp.colors = cb;

        // Viền vàng kim quanh PauseButton
        CreateBorder(btnObj.transform, "BorderTop", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 2), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(btnObj.transform, "BorderBottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 2), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(btnObj.transform, "BorderLeft", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(2, 0), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(btnObj.transform, "BorderRight", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(2, 0), new Color(1f, 0.82f, 0.2f, 0.95f));

        // Text trên PauseButton
        GameObject bTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        bTxtObj.transform.SetParent(btnObj.transform, false);
        Text bTxt = bTxtObj.GetComponent<Text>();
        bTxt.font = fontToUse;
        bTxt.fontSize = 14;
        bTxt.fontStyle = FontStyle.Bold;
        bTxt.alignment = TextAnchor.MiddleCenter;
        bTxt.text = "⚙️ Cài Đặt";
        bTxt.color = new Color(1f, 0.88f, 0.35f);
        bTxt.raycastTarget = false;
        bTxtObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btxtRt = bTxtObj.GetComponent<RectTransform>();
        btxtRt.anchorMin = Vector2.zero;
        btxtRt.anchorMax = Vector2.one;
        btxtRt.sizeDelta = Vector2.zero;

        // ─────────────────────────────────────────────────────────────────────
        // 3. TẠO PAUSE MENU MODAL (TOÀN MÀN HÌNH)
        // ─────────────────────────────────────────────────────────────────────
        GameObject modalObj = new GameObject("PauseMenuModal", typeof(RectTransform), typeof(CanvasGroup), typeof(PauseMenuManager));
        Undo.RegisterCreatedObjectUndo(modalObj, "Tạo PauseMenuModal");
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform modalRt = modalObj.GetComponent<RectTransform>();
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.offsetMin = Vector2.zero;
        modalRt.offsetMax = Vector2.zero;

        CanvasGroup modalCg = modalObj.GetComponent<CanvasGroup>();
        modalCg.alpha = 1f;

        // 4. Dark Overlay (Bấm ra ngoài để tiếp tục game)
        GameObject overlayObj = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayObj.transform.SetParent(modalObj.transform, false);
        RectTransform overRt = overlayObj.GetComponent<RectTransform>();
        overRt.anchorMin = Vector2.zero;
        overRt.anchorMax = Vector2.one;
        overRt.offsetMin = Vector2.zero;
        overRt.offsetMax = Vector2.zero;

        Image overImg = overlayObj.GetComponent<Image>();
        overImg.color = new Color(0f, 0f, 0f, 0.8f);
        Button overBtn = overlayObj.GetComponent<Button>();

        // 5. PauseBox (Hộp trung tâm)
        GameObject boxObj = new GameObject("PauseBox", typeof(RectTransform), typeof(Image));
        boxObj.transform.SetParent(modalObj.transform, false);
        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(490f, 530f);

        Image boxBg = boxObj.GetComponent<Image>();
        boxBg.color = new Color(0.09f, 0.12f, 0.18f, 0.98f); // Nền mực lam tiên hiệp

        // Viền vàng kim 3px
        CreateBorder(boxObj.transform, "BorderTop", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 3), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderBottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 3), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderLeft", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(3, 0), new Color(1f, 0.82f, 0.2f, 0.95f));
        CreateBorder(boxObj.transform, "BorderRight", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(3, 0), new Color(1f, 0.82f, 0.2f, 0.95f));

        // 6. Header Bar
        GameObject headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
        headerObj.transform.SetParent(boxObj.transform, false);
        RectTransform hRt = headerObj.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(0, 48f);

        Image hImg = headerObj.GetComponent<Image>();
        hImg.color = new Color(0.14f, 0.18f, 0.26f, 1f);

        GameObject hTitleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Outline));
        hTitleObj.transform.SetParent(headerObj.transform, false);
        Text hTitle = hTitleObj.GetComponent<Text>();
        hTitle.font = fontToUse;
        hTitle.fontSize = 20;
        hTitle.fontStyle = FontStyle.Bold;
        hTitle.alignment = TextAnchor.MiddleCenter;
        hTitle.text = "⏸️ TẠM DỪNG & CÀI ĐẶT ⚙️";
        hTitle.color = new Color(1f, 0.88f, 0.35f);
        hTitleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform htRt = hTitleObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.sizeDelta = Vector2.zero;

        // Close Button [✕]
        GameObject closeBtnObj = new GameObject("CloseHeaderBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(headerObj.transform, false);
        RectTransform cbRt = closeBtnObj.GetComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(1, 0.5f);
        cbRt.anchorMax = new Vector2(1, 0.5f);
        cbRt.pivot = new Vector2(1, 0.5f);
        cbRt.anchoredPosition = new Vector2(-8, 0);
        cbRt.sizeDelta = new Vector2(32, 32);

        Image cbImg = closeBtnObj.GetComponent<Image>();
        cbImg.color = new Color(0.8f, 0.22f, 0.22f, 0.9f);
        Button closeHeaderBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeTxt = closeTxtObj.GetComponent<Text>();
        closeTxt.font = fontToUse;
        closeTxt.fontSize = 17;
        closeTxt.fontStyle = FontStyle.Bold;
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeTxt.text = "✕";
        closeTxt.color = Color.white;
        RectTransform ctxtRt = closeTxtObj.GetComponent<RectTransform>();
        ctxtRt.anchorMin = Vector2.zero;
        ctxtRt.anchorMax = Vector2.one;
        ctxtRt.sizeDelta = Vector2.zero;

        // 7. Save Info Card (Hiển thị Đạo hiệu, Cảnh giới, Slot lưu và trạng thái đồng bộ)
        GameObject infoCardObj = new GameObject("SaveInfoCard", typeof(RectTransform), typeof(Image));
        infoCardObj.transform.SetParent(boxObj.transform, false);
        RectTransform icRt = infoCardObj.GetComponent<RectTransform>();
        icRt.anchorMin = new Vector2(0.5f, 1);
        icRt.anchorMax = new Vector2(0.5f, 1);
        icRt.pivot = new Vector2(0.5f, 1);
        icRt.anchoredPosition = new Vector2(0, -56);
        icRt.sizeDelta = new Vector2(450, 68);

        Image icBg = infoCardObj.GetComponent<Image>();
        icBg.color = new Color(0.13f, 0.17f, 0.25f, 0.95f);
        CreateBorder(infoCardObj.transform, "CardBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.35f, 0.48f, 0.65f, 0.4f));

        // TxtCharInfo
        GameObject tciObj = new GameObject("TxtCharacterInfo", typeof(RectTransform), typeof(Text));
        tciObj.transform.SetParent(infoCardObj.transform, false);
        Text txtCharInfo = tciObj.GetComponent<Text>();
        txtCharInfo.font = fontToUse;
        txtCharInfo.fontSize = 14;
        txtCharInfo.alignment = TextAnchor.MiddleLeft;
        txtCharInfo.text = "🏷️ <b>Đạo Hữu</b> | 🌟 <color=#66FFB2>Luyện Khí</color> | ⚔️ Tầng <color=#FFE666>1</color> (Slot 1)";
        txtCharInfo.color = Color.white;
        RectTransform tciRt = tciObj.GetComponent<RectTransform>();
        tciRt.anchorMin = new Vector2(0, 0.5f);
        tciRt.anchorMax = new Vector2(1, 1);
        tciRt.offsetMin = new Vector2(12, 0);
        tciRt.offsetMax = new Vector2(-12, -4);

        // TxtSaveStatus
        GameObject tssObj = new GameObject("TxtSaveStatus", typeof(RectTransform), typeof(Text));
        tssObj.transform.SetParent(infoCardObj.transform, false);
        Text txtSaveStatus = tssObj.GetComponent<Text>();
        txtSaveStatus.font = fontToUse;
        txtSaveStatus.fontSize = 13;
        txtSaveStatus.alignment = TextAnchor.MiddleLeft;
        txtSaveStatus.text = "💾 Trạng thái: <color=#88FF88>Tiến trình sẵn sàng đồng bộ</color>";
        txtSaveStatus.color = new Color(0.8f, 0.9f, 1f);
        RectTransform tssRt = tssObj.GetComponent<RectTransform>();
        tssRt.anchorMin = new Vector2(0, 0);
        tssRt.anchorMax = new Vector2(1, 0.5f);
        tssRt.offsetMin = new Vector2(12, 4);
        tssRt.offsetMax = new Vector2(-12, 0);

        // 8. Audio Settings Panel
        GameObject audioSection = new GameObject("AudioSection", typeof(RectTransform), typeof(Image));
        audioSection.transform.SetParent(boxObj.transform, false);
        RectTransform asRt = audioSection.GetComponent<RectTransform>();
        asRt.anchorMin = new Vector2(0.5f, 1);
        asRt.anchorMax = new Vector2(0.5f, 1);
        asRt.pivot = new Vector2(0.5f, 1);
        asRt.anchoredPosition = new Vector2(0, -132);
        asRt.sizeDelta = new Vector2(450, 142);

        Image asBg = audioSection.GetComponent<Image>();
        asBg.color = new Color(0.11f, 0.14f, 0.21f, 0.9f);
        CreateBorder(audioSection.transform, "AudioBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Color(0.35f, 0.48f, 0.65f, 0.4f));

        // Title Audio
        GameObject atObj = new GameObject("TitleAudio", typeof(RectTransform), typeof(Text));
        atObj.transform.SetParent(audioSection.transform, false);
        Text atTxt = atObj.GetComponent<Text>();
        atTxt.font = fontToUse;
        atTxt.fontSize = 14;
        atTxt.fontStyle = FontStyle.Bold;
        atTxt.text = "🎵 CÀI ĐẶT ÂM THANH:";
        atTxt.color = new Color(1f, 0.85f, 0.35f);
        RectTransform atRt = atObj.GetComponent<RectTransform>();
        atRt.anchorMin = new Vector2(0, 1);
        atRt.anchorMax = new Vector2(1, 1);
        atRt.pivot = new Vector2(0, 1);
        atRt.anchoredPosition = new Vector2(12, -8);
        atRt.sizeDelta = new Vector2(-24, 20);

        // BGM Slider Row
        Slider bgmSlider = CreateAudioSliderRow(audioSection.transform, "BgmRow", "Âm Nhạc (BGM):", -34f, fontToUse, out Text txtBgmVal);
        // SFX Slider Row
        Slider sfxSlider = CreateAudioSliderRow(audioSection.transform, "SfxRow", "Hiệu Ứng (SFX):", -70f, fontToUse, out Text txtSfxVal);
        // Mute Toggle Row
        Toggle muteToggle = CreateMuteToggleRow(audioSection.transform, "MuteRow", -106f, fontToUse);

        // 9. Action Buttons Area (Resume, Save, Main Menu, Quit)
        GameObject actionsArea = new GameObject("ActionsArea", typeof(RectTransform));
        actionsArea.transform.SetParent(boxObj.transform, false);
        RectTransform aaRt = actionsArea.GetComponent<RectTransform>();
        aaRt.anchorMin = new Vector2(0.5f, 0);
        aaRt.anchorMax = new Vector2(0.5f, 0);
        aaRt.pivot = new Vector2(0.5f, 0);
        aaRt.anchoredPosition = new Vector2(0, 16);
        aaRt.sizeDelta = new Vector2(450, 225);

        // 1. Nút Tiếp Tục [▶ TIẾP TỤC CHƠI]
        Button btnResume = CreateActionButton(actionsArea.transform, "BtnResume", "▶ TIẾP TỤC CHƠI", new Vector2(0, 175), 44f, new Color(0.12f, 0.42f, 0.26f), fontToUse, 16);
        // 2. Nút Lưu Tiến Trình [💾 LƯU TIẾN TRÌNH]
        Button btnSave = CreateActionButton(actionsArea.transform, "BtnSave", "💾 LƯU TIẾN TRÌNH", new Vector2(0, 122), 40f, new Color(0.14f, 0.32f, 0.52f), fontToUse, 15);
        // 3. Nút Về Main Menu [🏠 VỀ MENU CHÍNH]
        Button btnMainMenu = CreateActionButton(actionsArea.transform, "BtnMainMenu", "🏠 VỀ MENU CHÍNH", new Vector2(0, 72), 40f, new Color(0.50f, 0.36f, 0.14f), fontToUse, 15);
        // 4. Nút Thoát Game [🚪 THOÁT GAME]
        Button btnQuit = CreateActionButton(actionsArea.transform, "BtnQuit", "🚪 THOÁT TRÒ CHƠI", new Vector2(0, 22), 40f, new Color(0.52f, 0.16f, 0.16f), fontToUse, 15);

        // ─────────────────────────────────────────────────────────────────────
        // 10. CONFIRMATION POPUP (HỘP XÁC NHẬN NỘI BỘ TRÁNH LỖI BẤM NHẦM)
        // ─────────────────────────────────────────────────────────────────────
        GameObject confObj = new GameObject("ConfirmationPopup", typeof(RectTransform), typeof(Image));
        confObj.transform.SetParent(modalObj.transform, false);
        RectTransform confRt = confObj.GetComponent<RectTransform>();
        confRt.anchorMin = Vector2.zero;
        confRt.anchorMax = Vector2.one;
        confRt.sizeDelta = Vector2.zero;

        Image confBg = confObj.GetComponent<Image>();
        confBg.color = new Color(0f, 0f, 0f, 0.88f); // Nền tối làm nổi bật hộp hỏi

        GameObject confBox = new GameObject("Box", typeof(RectTransform), typeof(Image));
        confBox.transform.SetParent(confObj.transform, false);
        RectTransform cfBoxRt = confBox.GetComponent<RectTransform>();
        cfBoxRt.anchorMin = new Vector2(0.5f, 0.5f);
        cfBoxRt.anchorMax = new Vector2(0.5f, 0.5f);
        cfBoxRt.pivot = new Vector2(0.5f, 0.5f);
        cfBoxRt.anchoredPosition = Vector2.zero;
        cfBoxRt.sizeDelta = new Vector2(430, 210);

        Image cfBoxBg = confBox.GetComponent<Image>();
        cfBoxBg.color = new Color(0.11f, 0.14f, 0.22f, 1f);
        CreateBorder(confBox.transform, "ConfBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Color(1f, 0.82f, 0.2f, 0.95f));

        // Title
        GameObject cfTitleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
        cfTitleObj.transform.SetParent(confBox.transform, false);
        Text cfTitle = cfTitleObj.GetComponent<Text>();
        cfTitle.font = fontToUse;
        cfTitle.fontSize = 18;
        cfTitle.fontStyle = FontStyle.Bold;
        cfTitle.alignment = TextAnchor.MiddleCenter;
        cfTitle.text = "XÁC NHẬN";
        cfTitle.color = new Color(1f, 0.88f, 0.35f);
        RectTransform cfTitleRt = cfTitleObj.GetComponent<RectTransform>();
        cfTitleRt.anchorMin = new Vector2(0, 1);
        cfTitleRt.anchorMax = new Vector2(1, 1);
        cfTitleRt.pivot = new Vector2(0.5f, 1);
        cfTitleRt.anchoredPosition = new Vector2(0, -12);
        cfTitleRt.sizeDelta = new Vector2(-20, 32);

        // Message
        GameObject cfMsgObj = new GameObject("Message", typeof(RectTransform), typeof(Text));
        cfMsgObj.transform.SetParent(confBox.transform, false);
        Text cfMsg = cfMsgObj.GetComponent<Text>();
        cfMsg.font = fontToUse;
        cfMsg.fontSize = 14;
        cfMsg.alignment = TextAnchor.MiddleCenter;
        cfMsg.text = "Đạo hữu có muốn lưu tiến trình hiện tại không?";
        cfMsg.color = Color.white;
        RectTransform cfMsgRt = cfMsgObj.GetComponent<RectTransform>();
        cfMsgRt.anchorMin = new Vector2(0, 0.35f);
        cfMsgRt.anchorMax = new Vector2(1, 0.85f);
        cfMsgRt.offsetMin = new Vector2(16, 0);
        cfMsgRt.offsetMax = new Vector2(-16, 0);

        // Confirm Button
        Button cfBtnConfirm = CreateActionButton(confBox.transform, "BtnConfirm", "ĐỒNG Ý (LƯU)", new Vector2(105, 14), 38f, new Color(0.12f, 0.42f, 0.26f), fontToUse, 14);
        RectTransform cfBcRt = cfBtnConfirm.GetComponent<RectTransform>();
        cfBcRt.sizeDelta = new Vector2(150, 38);

        // Cancel Button
        Button cfBtnCancel = CreateActionButton(confBox.transform, "BtnCancel", "HỦY BỎ", new Vector2(-105, 14), 38f, new Color(0.35f, 0.38f, 0.45f), fontToUse, 14);
        RectTransform cfBcnRt = cfBtnCancel.GetComponent<RectTransform>();
        cfBcnRt.sizeDelta = new Vector2(130, 38);

        confObj.SetActive(false); // Mặc định ẩn popup

        // ─────────────────────────────────────────────────────────────────────
        // 11. GÁN TẤT CẢ REFERENCES VÀO PAUSEMENUMANAGER
        // ─────────────────────────────────────────────────────────────────────
        PauseMenuManager pmm = modalObj.GetComponent<PauseMenuManager>();
        pmm.pauseModal = boxObj;
        pmm.confirmationPopup = confObj;
        pmm.modalCanvasGroup = modalCg;

        pmm.openPauseButton = btnComp;
        pmm.closeHeaderButton = closeHeaderBtn;
        pmm.overlayCloseButton = overBtn;

        pmm.resumeButton = btnResume;
        pmm.manualSaveButton = btnSave;
        pmm.returnMainMenuButton = btnMainMenu;
        pmm.quitButton = btnQuit;

        pmm.bgmSlider = bgmSlider;
        pmm.txtBgmValue = txtBgmVal;
        pmm.sfxSlider = sfxSlider;
        pmm.txtSfxValue = txtSfxVal;
        pmm.muteToggle = muteToggle;

        pmm.txtCharacterInfo = txtCharInfo;
        pmm.txtSaveStatus = txtSaveStatus;

        pmm.txtPopupTitle = cfTitle;
        pmm.txtPopupMessage = cfMsg;
        pmm.btnPopupConfirm = cfBtnConfirm;
        pmm.btnPopupCancel = cfBtnCancel;

        // Gán tham chiếu và sự kiện cho PauseButtonHandler
        PauseButtonHandler pbh = btnObj.GetComponent<PauseButtonHandler>();
        if (pbh != null)
        {
            pbh.pauseMenuModal = modalObj;
        }

        btnComp.onClick.RemoveAllListeners();
        btnComp.onClick.AddListener(pbh.OnPauseButtonClicked);

        // Mặc định ẩn modal để không che màn hình, khi click nút hoặc ESC sẽ mở ngay lập tức
        modalObj.SetActive(false);
        modalObj.transform.SetAsLastSibling();

        EditorUtility.SetDirty(btnObj);
        EditorUtility.SetDirty(modalObj);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = modalObj;
        EditorGUIUtility.PingObject(modalObj);
        Debug.Log("<color=green>✅ [PauseMenuBuilder] ĐÃ TẠO THÀNH CÔNG NÚT PAUSE & PANEL CÀI ĐẶT! Có đầy đủ Pause, Audio Settings, Về Main Menu, Thoát, và Đồng bộ Save Game.</color>");
    }

    private static Slider CreateAudioSliderRow(Transform parent, string name, string labelText, float anchoredY, Font font, out Text txtValue)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rRt = row.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0, 1);
        rRt.anchorMax = new Vector2(1, 1);
        rRt.pivot = new Vector2(0.5f, 1);
        rRt.anchoredPosition = new Vector2(0, anchoredY);
        rRt.sizeDelta = new Vector2(-24, 26);

        // Label
        GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        lblObj.transform.SetParent(row.transform, false);
        Text lbl = lblObj.GetComponent<Text>();
        lbl.font = font;
        lbl.fontSize = 13;
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.text = labelText;
        lbl.color = Color.white;
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0, 0);
        lblRt.anchorMax = new Vector2(0.38f, 1);
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;

        // Slider
        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(row.transform, false);
        RectTransform sRt = sliderObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0.40f, 0.5f);
        sRt.anchorMax = new Vector2(0.85f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = Vector2.zero;
        sRt.sizeDelta = new Vector2(0, 16);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        // Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);

        // Fill Area
        GameObject faObj = new GameObject("Fill Area", typeof(RectTransform));
        faObj.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = faObj.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.25f);
        faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = new Vector2(4, 0);
        faRt.offsetMax = new Vector2(-4, 0);

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(faObj.transform, false);
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.color = new Color(1f, 0.82f, 0.2f, 1f); // Màu vàng kim

        // Handle Slide Area
        GameObject haObj = new GameObject("Handle Slide Area", typeof(RectTransform));
        haObj.transform.SetParent(sliderObj.transform, false);
        RectTransform haRt = haObj.GetComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(8, 0);
        haRt.offsetMax = new Vector2(-8, 0);

        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(haObj.transform, false);
        RectTransform hRt = handleObj.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(16, 16);
        Image hImg = handleObj.GetComponent<Image>();
        hImg.color = new Color(1f, 0.95f, 0.65f, 1f);

        slider.fillRect = fillRt;
        slider.handleRect = hRt;
        slider.targetGraphic = hImg;
        slider.direction = Slider.Direction.LeftToRight;

        // Value text
        GameObject valObj = new GameObject("TxtValue", typeof(RectTransform), typeof(Text));
        valObj.transform.SetParent(row.transform, false);
        txtValue = valObj.GetComponent<Text>();
        txtValue.font = font;
        txtValue.fontSize = 12;
        txtValue.alignment = TextAnchor.MiddleRight;
        txtValue.text = "80%";
        txtValue.color = new Color(1f, 0.85f, 0.35f);
        RectTransform valRt = valObj.GetComponent<RectTransform>();
        valRt.anchorMin = new Vector2(0.86f, 0);
        valRt.anchorMax = new Vector2(1, 1);
        valRt.offsetMin = Vector2.zero;
        valRt.offsetMax = Vector2.zero;

        return slider;
    }

    private static Toggle CreateMuteToggleRow(Transform parent, string name, float anchoredY, Font font)
    {
        GameObject toggleObj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        toggleObj.transform.SetParent(parent, false);
        RectTransform tRt = toggleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 1);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.pivot = new Vector2(0.5f, 1);
        tRt.anchoredPosition = new Vector2(0, anchoredY);
        tRt.sizeDelta = new Vector2(-24, 24);

        Toggle toggle = toggleObj.GetComponent<Toggle>();

        // Checkbox Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.5f);
        bgRt.anchorMax = new Vector2(0, 0.5f);
        bgRt.pivot = new Vector2(0, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        bgRt.sizeDelta = new Vector2(18, 18);

        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.2f, 0.25f, 0.35f, 1f);

        // Checkmark
        GameObject cmObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        cmObj.transform.SetParent(bgObj.transform, false);
        RectTransform cmRt = cmObj.GetComponent<RectTransform>();
        cmRt.anchorMin = new Vector2(0.15f, 0.15f);
        cmRt.anchorMax = new Vector2(0.85f, 0.85f);
        cmRt.sizeDelta = Vector2.zero;

        Image cmImg = cmObj.GetComponent<Image>();
        cmImg.color = new Color(1f, 0.82f, 0.2f, 1f);

        // Label
        GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        lblObj.transform.SetParent(toggleObj.transform, false);
        Text lbl = lblObj.GetComponent<Text>();
        lbl.font = font;
        lbl.fontSize = 13;
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.text = "🔇 Tắt toàn bộ âm thanh (Mute)";
        lbl.color = new Color(0.9f, 0.9f, 0.95f);
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0, 0);
        lblRt.anchorMax = new Vector2(1, 1);
        lblRt.offsetMin = new Vector2(26, 0);
        lblRt.offsetMax = Vector2.zero;

        toggle.graphic = cmImg;
        toggle.targetGraphic = bgImg;
        toggle.isOn = false;
        return toggle;
    }

    private static Button CreateActionButton(Transform parent, string name, string text, Vector2 anchoredPos, float height, Color bgColor, Font font, int fontSize)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(430, height);

        Image img = btnObj.GetComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = new Color(Mathf.Min(bgColor.r * 1.3f, 1f), Mathf.Min(bgColor.g * 1.3f, 1f), Mathf.Min(bgColor.b * 1.3f, 1f), 1f);
        cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        btn.colors = cb;

        // Border
        CreateBorder(btnObj.transform, "BtnBorder", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Color(1f, 1f, 1f, 0.2f));

        GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        tObj.transform.SetParent(btnObj.transform, false);
        Text txt = tObj.GetComponent<Text>();
        txt.font = font;
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = text;
        txt.color = Color.white;
        tObj.GetComponent<Outline>().effectColor = Color.black;

        RectTransform trt = tObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        return btn;
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

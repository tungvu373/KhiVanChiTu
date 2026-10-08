using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIBuilder : EditorWindow
{
    [MenuItem("Tools/Game Sprint 2026/Tạo UI Main Menu")]
    public static void BuildMainMenuUI()
    {
        // 1. DataManager
        if (FindObjectOfType<DataManager>() == null)
        {
            GameObject dataMgrObj = new GameObject("DataManager");
            dataMgrObj.AddComponent<DataManager>();
        }

        // 2. Canvas & EventSystem
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        // 3. MainMenuManager Object
        GameObject menuManagerObj = CreateUIElement("MainMenuManager", canvas.transform);
        MainMenuManager menuManager = menuManagerObj.AddComponent<MainMenuManager>();
        SetRectStretch(menuManagerObj.GetComponent<RectTransform>());

        // --- Tạo UI Elements ---
        Font arial = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // A. Main Panel
        GameObject mainPanel = CreateUIElement("MainPanel", menuManagerObj.transform);
        Image mainBg = mainPanel.AddComponent<Image>();
        mainBg.color = new Color(0.12f, 0.12f, 0.15f, 1f);
        SetRectStretch(mainPanel.GetComponent<RectTransform>());

        Text titleText = CreateText("TitleText", mainPanel.transform, "KHÍ VẬN CHI TỬ", arial, 80);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.9f, 0.7f, 0.2f);
        SetRect(titleText.GetComponent<RectTransform>(), 0, 250, 800, 150);

        Button playBtn = CreateButton("PlayButton", mainPanel.transform, "Chơi Mới", arial, out Text playBtnText);
        SetRect(playBtn.GetComponent<RectTransform>(), 0, 0, 300, 80);
        playBtnText.fontSize = 28;
        
        Button settingsBtn = CreateButton("SettingsButton", mainPanel.transform, "Cài Đặt", arial, out _);
        SetRect(settingsBtn.GetComponent<RectTransform>(), 0, -100, 300, 80);

        Button quitBtn = CreateButton("QuitButton", mainPanel.transform, "Thoát", arial, out _);
        SetRect(quitBtn.GetComponent<RectTransform>(), 0, -200, 300, 80);

        // B. Character Select Panel
        GameObject selectPanel = CreateUIElement("CharacterSelectPanel", menuManagerObj.transform);
        Image selectBg = selectPanel.AddComponent<Image>();
        selectBg.color = new Color(0.1f, 0.15f, 0.18f, 1f);
        SetRectStretch(selectPanel.GetComponent<RectTransform>());

        Text selectTitle = CreateText("SelectTitle", selectPanel.transform, "CHỌN TIÊN TÔN", arial, 50);
        selectTitle.alignment = TextAnchor.MiddleCenter;
        SetRect(selectTitle.GetComponent<RectTransform>(), 0, 350, 600, 80);

        Button backFromSelectBtn = CreateButton("BackButton", selectPanel.transform, "Quay Lại", arial, out _);
        SetRect(backFromSelectBtn.GetComponent<RectTransform>(), -800, 450, 150, 60);

        GameObject slotContainer = CreateUIElement("SlotContainer", selectPanel.transform);
        SetRect(slotContainer.GetComponent<RectTransform>(), 0, -50, 1200, 500);
        HorizontalLayoutGroup hLayout = slotContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.spacing = 80;
        hLayout.childControlHeight = false;
        hLayout.childControlWidth = false;

        SaveSlotUI[] slots = new SaveSlotUI[3];
        for (int i = 0; i < 3; i++)
        {
            slots[i] = CreateSaveSlot($"SaveSlot_{i}", slotContainer.transform, arial, i);
        }

        // C. Character Create Panel
        GameObject createPanel = CreateUIElement("CharacterCreatePanel", menuManagerObj.transform);
        Image createBg = createPanel.AddComponent<Image>();
        createBg.color = new Color(0.18f, 0.15f, 0.1f, 1f);
        SetRectStretch(createPanel.GetComponent<RectTransform>());

        Text createTitle = CreateText("CreateTitle", createPanel.transform, "KHAI SINH NHÂN VẬT", arial, 50);
        createTitle.alignment = TextAnchor.MiddleCenter;
        SetRect(createTitle.GetComponent<RectTransform>(), 0, 300, 600, 80);

        // Input Field cho Tên
        GameObject inputObj = CreateUIElement("NameInput", createPanel.transform);
        SetRect(inputObj.GetComponent<RectTransform>(), 0, 100, 400, 60);
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = Color.white;
        InputField nameInput = inputObj.AddComponent<InputField>();
        nameInput.targetGraphic = inputBg; // Lỗi nghiêm trọng: Thiếu TargetGraphic thì không click vào nhập text được
        Text inputText = CreateText("Text", inputObj.transform, "", arial, 28);
        inputText.color = Color.black;
        SetRectStretch(inputText.GetComponent<RectTransform>());
        nameInput.textComponent = inputText;

        Text inputLabel = CreateText("Label", inputObj.transform, "Đạo Hữu Tôn Tính Đại Danh:", arial, 24);
        SetRect(inputLabel.GetComponent<RectTransform>(), 0, 60, 400, 40);

        // Giới tính (Toggles)
        GameObject toggleGroupObj = CreateUIElement("GenderToggles", createPanel.transform);
        SetRect(toggleGroupObj.GetComponent<RectTransform>(), 0, -50, 400, 60);
        ToggleGroup tGroup = toggleGroupObj.AddComponent<ToggleGroup>();

        Toggle maleToggle = CreateToggle("MaleToggle", toggleGroupObj.transform, "Nam Tử", arial);
        SetRect(maleToggle.GetComponent<RectTransform>(), -100, 0, 120, 40);
        maleToggle.group = tGroup;
        maleToggle.isOn = true;

        Toggle femaleToggle = CreateToggle("FemaleToggle", toggleGroupObj.transform, "Nữ Tử", arial);
        SetRect(femaleToggle.GetComponent<RectTransform>(), 100, 0, 120, 40);
        femaleToggle.group = tGroup;

        Button confirmCreateBtn = CreateButton("ConfirmButton", createPanel.transform, "Bắt Đầu Tu Tiên", arial, out _);
        SetRect(confirmCreateBtn.GetComponent<RectTransform>(), 150, -200, 250, 70);

        Button cancelCreateBtn = CreateButton("CancelButton", createPanel.transform, "Hủy Bỏ", arial, out _);
        SetRect(cancelCreateBtn.GetComponent<RectTransform>(), -150, -200, 250, 70);

        // D. Settings Panel
        GameObject settingsUI = CreateUIElement("SettingsPanel", menuManagerObj.transform);
        Image settingsBg = settingsUI.AddComponent<Image>();
        settingsBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        SetRectStretch(settingsUI.GetComponent<RectTransform>());
        
        Button closeSettingsBtn = CreateButton("CloseButton", settingsUI.transform, "Xong", arial, out _);
        SetRect(closeSettingsBtn.GetComponent<RectTransform>(), 0, -300, 200, 60);
        
        Slider bgmSlider = CreateSlider("BGMSlider", settingsUI.transform);
        SetRect(bgmSlider.GetComponent<RectTransform>(), 0, 100, 400, 20);
        CreateText("Label", bgmSlider.transform, "Âm Nhạc (BGM)", arial, 24).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
        
        Slider sfxSlider = CreateSlider("SFXSlider", settingsUI.transform);
        SetRect(sfxSlider.GetComponent<RectTransform>(), 0, -50, 400, 20);
        CreateText("Label", sfxSlider.transform, "Âm Thanh (SFX)", arial, 24).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

        // E. Confirmation Popup
        GameObject popup = CreateUIElement("ConfirmationPopup", menuManagerObj.transform);
        Image popupBg = popup.AddComponent<Image>();
        popupBg.color = new Color(0, 0, 0, 0.8f);
        SetRectStretch(popup.GetComponent<RectTransform>());

        GameObject popupBox = CreateUIElement("Box", popup.transform);
        Image boxBg = popupBox.AddComponent<Image>();
        boxBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        SetRect(popupBox.GetComponent<RectTransform>(), 0, 0, 500, 300);

        Text popupTitleText = CreateText("Title", popupBox.transform, "Xác Nhận", arial, 36);
        popupTitleText.alignment = TextAnchor.MiddleCenter;
        popupTitleText.color = new Color(0.9f, 0.3f, 0.3f);
        SetRect(popupTitleText.GetComponent<RectTransform>(), 0, 100, 400, 50);

        Text popupMsgText = CreateText("Message", popupBox.transform, "Bạn có chắc chắn muốn thực hiện hành động này?", arial, 24);
        popupMsgText.alignment = TextAnchor.MiddleCenter;
        SetRect(popupMsgText.GetComponent<RectTransform>(), 0, 10, 460, 100);

        Button confirmBtn = CreateButton("ConfirmBtn", popupBox.transform, "Đồng Ý", arial, out _);
        SetRect(confirmBtn.GetComponent<RectTransform>(), 120, -100, 160, 50);
        
        Button cancelBtn = CreateButton("CancelBtn", popupBox.transform, "Hủy", arial, out _);
        SetRect(cancelBtn.GetComponent<RectTransform>(), -120, -100, 160, 50);

        // --- GÁN REFERENCE CHO MAIN MENU MANAGER ---
        menuManager.mainPanel = mainPanel;
        menuManager.characterSelectPanel = selectPanel;
        menuManager.characterCreatePanel = createPanel;
        menuManager.settingsPanel = settingsUI;
        menuManager.confirmationPopup = popup;

        menuManager.playButton = playBtn;
        menuManager.playButtonText = playBtnText;
        menuManager.settingsButton = settingsBtn;
        menuManager.quitButton = quitBtn;

        menuManager.saveSlots = slots;
        menuManager.backFromSelectButton = backFromSelectBtn;

        menuManager.nameInput = nameInput;
        menuManager.maleToggle = maleToggle;
        menuManager.femaleToggle = femaleToggle;
        menuManager.confirmCreateButton = confirmCreateBtn;
        menuManager.cancelCreateButton = cancelCreateBtn;

        menuManager.bgmSlider = bgmSlider;
        menuManager.sfxSlider = sfxSlider;
        menuManager.closeSettingsButton = closeSettingsBtn;

        menuManager.popupTitle = popupTitleText;
        menuManager.popupMessage = popupMsgText;
        menuManager.popupConfirmBtn = confirmBtn;
        menuManager.popupCancelBtn = cancelBtn;

        // Ẩn tất cả ngoại trừ main menu
        selectPanel.SetActive(false);
        createPanel.SetActive(false);
        settingsUI.SetActive(false);
        popup.SetActive(false);

        Debug.Log("🎉 Auto-generate UI Main Menu thành công!");
    }

    private static SaveSlotUI CreateSaveSlot(string name, Transform parent, Font font, int index)
    {
        GameObject slotObj = CreateUIElement(name, parent);
        SetRect(slotObj.GetComponent<RectTransform>(), 0, 0, 300, 450);
        
        Image slotBg = slotObj.AddComponent<Image>();
        slotBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        
        Button slotBtn = slotObj.AddComponent<Button>();
        slotBtn.targetGraphic = slotBg;

        SaveSlotUI slotUI = slotObj.AddComponent<SaveSlotUI>();
        slotUI.slotIndex = index;
        slotUI.slotButton = slotBtn;

        // 1. Data Panel
        GameObject dataPanel = CreateUIElement("DataPanel", slotObj.transform);
        SetRectStretch(dataPanel.GetComponent<RectTransform>());
        slotUI.dataPanel = dataPanel;

        GameObject modelObj = CreateUIElement("ModelImage", dataPanel.transform);
        SetRect(modelObj.GetComponent<RectTransform>(), 0, 80, 180, 180);
        Image modelImg = modelObj.AddComponent<Image>();
        slotUI.modelImage = modelImg;

        Text nameText = CreateText("NameText", dataPanel.transform, "Tên Nhân Vật", font, 28);
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.cyan;
        SetRect(nameText.GetComponent<RectTransform>(), 0, -50, 280, 40);
        slotUI.nameText = nameText;

        Text culText = CreateText("CultivationText", dataPanel.transform, "Luyện Khí Kỳ", font, 22);
        culText.alignment = TextAnchor.MiddleCenter;
        SetRect(culText.GetComponent<RectTransform>(), 0, -100, 280, 30);
        slotUI.cultivationText = culText;

        Text timeText = CreateText("TimeText", dataPanel.transform, "Thời gian...", font, 16);
        timeText.alignment = TextAnchor.MiddleCenter;
        SetRect(timeText.GetComponent<RectTransform>(), 0, -150, 280, 50);
        slotUI.playTimeText = timeText;

        Button deleteBtn = CreateButton("DeleteBtn", dataPanel.transform, "X", font, out _);
        SetRect(deleteBtn.GetComponent<RectTransform>(), 120, 190, 40, 40);
        deleteBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
        slotUI.deleteButton = deleteBtn;

        // 2. Empty Panel
        GameObject emptyPanel = CreateUIElement("EmptyPanel", slotObj.transform);
        SetRectStretch(emptyPanel.GetComponent<RectTransform>());
        slotUI.emptyPanel = emptyPanel;

        Text emptyText = CreateText("EmptyText", emptyPanel.transform, "Trống\n{ Tạo mới }", font, 30);
        emptyText.alignment = TextAnchor.MiddleCenter;
        emptyText.color = Color.gray;
        SetRectStretch(emptyText.GetComponent<RectTransform>());

        return slotUI;
    }

    // --- CÁC HÀM TIỆN ÍCH ---
    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static Text CreateText(string name, Transform parent, string text, Font font, int fontSize)
    {
        GameObject go = CreateUIElement(name, parent);
        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.color = Color.white;
        return t;
    }

    private static Button CreateButton(string name, Transform parent, string label, Font font, out Text textComponent)
    {
        GameObject go = CreateUIElement(name, parent);
        Image img = go.AddComponent<Image>();
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        textComponent = CreateText("Text", go.transform, label, font, 24);
        textComponent.color = Color.black;
        textComponent.alignment = TextAnchor.MiddleCenter;
        SetRectStretch(textComponent.GetComponent<RectTransform>());

        return btn;
    }

    private static Toggle CreateToggle(string name, Transform parent, string label, Font font)
    {
        GameObject go = CreateUIElement(name, parent);
        Toggle toggle = go.AddComponent<Toggle>();

        GameObject bg = CreateUIElement("Background", go.transform);
        Image bgImg = bg.AddComponent<Image>();
        SetRect(bg.GetComponent<RectTransform>(), -40, 0, 40, 40);
        toggle.targetGraphic = bgImg;

        GameObject checkmark = CreateUIElement("Checkmark", bg.transform);
        Image chkImg = checkmark.AddComponent<Image>();
        chkImg.color = Color.black;
        SetRect(checkmark.GetComponent<RectTransform>(), 0, 0, 25, 25);
        toggle.graphic = chkImg;

        Text t = CreateText("Label", go.transform, label, font, 24);
        t.alignment = TextAnchor.MiddleLeft;
        SetRect(t.GetComponent<RectTransform>(), 20, 0, 100, 40);

        return toggle;
    }

    private static Slider CreateSlider(string name, Transform parent)
    {
        GameObject go = CreateUIElement(name, parent);
        Slider slider = go.AddComponent<Slider>();

        GameObject bg = CreateUIElement("Background", go.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.gray;
        SetRectStretch(bg.GetComponent<RectTransform>());

        GameObject fillArea = CreateUIElement("Fill Area", go.transform);
        SetRectStretch(fillArea.GetComponent<RectTransform>());
        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.white;
        SetRectStretch(fill.GetComponent<RectTransform>());

        GameObject handleArea = CreateUIElement("Handle Slide Area", go.transform);
        SetRectStretch(handleArea.GetComponent<RectTransform>());
        GameObject handle = CreateUIElement("Handle", handleArea.transform);
        Image handleImg = handle.AddComponent<Image>();
        SetRect(handle.GetComponent<RectTransform>(), 0, 0, 30, 0);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImg;

        return slider;
    }

    private static void SetRect(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetRectStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
}

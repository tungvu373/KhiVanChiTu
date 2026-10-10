using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject settingsPanel;
    public GameObject confirmationPopup;

    [Header("Main Menu Buttons")]
    public Button playButton; 
    public Text playButtonText;
    public Button settingsButton;
    public Button quitButton;

    [Header("Character Selection")]
    public SaveSlotUI[] saveSlots; 
    public Button backFromSelectButton;

    [Header("Character Creation")]
    public InputField nameInput;
    public Button confirmCreateButton;
    public Button cancelCreateButton;
    private int currentSlotBeingCreated = -1;

    [Header("Settings")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Button closeSettingsButton;

    [Header("Confirmation Popup")]
    public Text popupTitle;
    public Text popupMessage;
    public Button popupConfirmBtn;
    public Button popupCancelBtn;

    private DataManager dataManager;
    private System.Collections.Generic.List<GameObject> hiddenUIElements = new System.Collections.Generic.List<GameObject>();

    private void Start()
    {
        // Đóng băng mọi hoạt động của game (HP, MP, Quái vật) khi đang ở Menu
        Time.timeScale = 0f; 
        
        // Khởi tạo Menu ngay lập tức
        InitializeMenu();

        // Đợi 1 frame để tất cả các Manager khác (như SkillManager) kịp tạo UI trong hàm Start của chúng
        StartCoroutine(HideUIRoutine());
    }

    private System.Collections.IEnumerator HideUIRoutine()
    {
        yield return new WaitForEndOfFrame();
        // Tự động tìm và ẨN tất cả UI của Game (Thanh HP, MP, Log...)
        HideGameplayUI();
    }

    private void HideGameplayUI()
    {
        hiddenUIElements.Clear();
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            foreach (Transform child in c.transform)
            {
                // Không ẩn chính MainMenu
                if (child == this.transform || child.IsChildOf(this.transform)) continue;
                // Không ẩn hệ thống Click Event
                if (child.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) continue;

                if (child.gameObject.activeSelf)
                {
                    hiddenUIElements.Add(child.gameObject);
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void RestoreGameplayUI()
    {
        foreach(var obj in hiddenUIElements)
        {
            if (obj != null) obj.SetActive(true);
        }
        hiddenUIElements.Clear();
    }

    private void InitializeMenu()
    {
        dataManager = DataManager.Instance;
        if(dataManager == null) 
        {
            Debug.LogError("Không tìm thấy DataManager trong Scene! Hãy chắc chắn có object gắn DataManager.");
            return;
        }

        SetupButtons();
        UpdateMainMenuPlayButton();
        ShowPanel(mainPanel);
    }

    private void SetupButtons()
    {
        settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        quitButton.onClick.AddListener(ConfirmQuitGame);
        backFromSelectButton.onClick.AddListener(() => ShowPanel(mainPanel));
        
        cancelCreateButton.onClick.AddListener(() => ShowPanel(characterSelectPanel));
        confirmCreateButton.onClick.AddListener(OnConfirmCreateCharacter);

        closeSettingsButton.onClick.AddListener(() => ShowPanel(mainPanel));
    }

    private void UpdateMainMenuPlayButton()
    {
        bool hasAnySave = false;
        foreach (var slot in dataManager.gameSaveData.slots)
        {
            if (!slot.isSlotEmpty) hasAnySave = true;
        }
        
        if (playButtonText != null)
        {
            playButtonText.text = hasAnySave ? "Tiếp Tục" : "Chơi Mới";
        }
        
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OpenCharacterSelection);
    }

    public void OpenCharacterSelection()
    {
        ShowPanel(characterSelectPanel);
        for (int i = 0; i < 3; i++)
        {
            if (i < saveSlots.Length && saveSlots[i] != null)
            {
                saveSlots[i].Initialize(this, dataManager.gameSaveData.slots[i], i);
            }
        }
    }

    public void OpenCharacterCreation(int slotIndex)
    {
        currentSlotBeingCreated = slotIndex;
        if (nameInput) nameInput.text = "";
        ShowPanel(characterCreatePanel);
    }

    private void OnConfirmCreateCharacter()
    {
        string charName = nameInput != null ? nameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(charName))
        {
            Debug.LogWarning("Tên không được để trống!");
            return;
        }

        CharacterSaveData newData = new CharacterSaveData();
        newData.isSlotEmpty = false;
        newData.characterName = charName;
        newData.stageName = "Luyện Khí Kỳ Tầng 1";
        newData.stageIndex = 0;
        newData.currentTuVi = 0f;
        newData.linhThach = 0f;
        newData.coDuyen = 0;
        newData.kiemY = 0;
        newData.playTime = 0f;
        newData.lastPlayedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        dataManager.gameSaveData.slots[currentSlotBeingCreated] = newData;
        dataManager.SaveData();

        // Sau khi tạo thành công thì vào thẳng game luôn
        ConfirmEnterGame(currentSlotBeingCreated, newData);
    }

    public void AskConfirmDeleteSlot(int slotIndex)
    {
        ShowConfirmation("Xác nhận xóa", "Bạn có chắc muốn xóa nhân vật này? Dữ liệu không thể khôi phục.", 
        () => {
            dataManager.gameSaveData.slots[slotIndex] = new CharacterSaveData();
            dataManager.SaveData();
            UpdateMainMenuPlayButton();
            OpenCharacterSelection(); // Refresh UI
            confirmationPopup.SetActive(false);
        }, 
        () => {
            confirmationPopup.SetActive(false);
        });
    }

    public void AskConfirmEnterGame(int slotIndex, CharacterSaveData data)
    {
        ShowConfirmation("Vào game", $"Bắt đầu tu tiên với nhân vật {data.characterName}?", 
        () => {
            ConfirmEnterGame(slotIndex, data);
        }, 
        () => {
            confirmationPopup.SetActive(false);
        });
    }

    private void ConfirmEnterGame(int slotIndex, CharacterSaveData data)
    {
        dataManager.gameSaveData.lastPlayedSlotIndex = slotIndex;
        
        // Cập nhật ngày giờ lần chơi cuối
        dataManager.gameSaveData.slots[slotIndex].lastPlayedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        dataManager.SaveData();
        
        Debug.Log("Bắt đầu vào Game! (Đã ẩn Main Menu)");
        
        // 1. Khôi phục lại thời gian để Game bắt đầu chạy
        Time.timeScale = 1f;
        
        // 2. Hiện lại toàn bộ thanh HP, MP, Game UI
        RestoreGameplayUI();
        
        // 3. Reset UI về Panel chính để lần sau bật lại không bị kẹt
        ShowPanel(mainPanel);
        
        // 4. Ẩn toàn bộ hệ thống MainMenu để người chơi bắt đầu Idle Farm phía sau
        gameObject.SetActive(false);

        // 5. Load dữ liệu nhân vật từ slot vào từng Manager (dùng Invoke để đợi 1 frame sau khi UI đã khởi tạo)
        dataManager.Invoke(nameof(DataManager.LoadCurrentGame), 0.05f);
    }

    private void ConfirmQuitGame()
    {
        ShowConfirmation("Thoát game", "Bạn có muốn thoát game?", 
        () => {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }, 
        () => {
            confirmationPopup.SetActive(false);
        });
    }

    private void ShowConfirmation(string title, string message, UnityEngine.Events.UnityAction onConfirm, UnityEngine.Events.UnityAction onCancel)
    {
        if(confirmationPopup != null) confirmationPopup.SetActive(true);
        if (popupTitle) popupTitle.text = title;
        if (popupMessage) popupMessage.text = message;

        if(popupConfirmBtn)
        {
            popupConfirmBtn.onClick.RemoveAllListeners();
            popupConfirmBtn.onClick.AddListener(onConfirm);
        }

        if(popupCancelBtn)
        {
            popupCancelBtn.onClick.RemoveAllListeners();
            popupCancelBtn.onClick.AddListener(onCancel);
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if(mainPanel) mainPanel.SetActive(false);
        if(characterSelectPanel) characterSelectPanel.SetActive(false);
        if(characterCreatePanel) characterCreatePanel.SetActive(false);
        if(settingsPanel) settingsPanel.SetActive(false);
        if(confirmationPopup) confirmationPopup.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}

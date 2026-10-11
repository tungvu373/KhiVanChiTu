using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý Panel Pause Game (Tạm Dừng, Cài Đặt Âm Thanh, Trở Về Main Menu, Thoát Game)
/// Đồng bộ tuyệt đối với hệ thống lưu trữ dữ liệu DataManager.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    private static PauseMenuManager _instance;
    public static PauseMenuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PauseMenuManager>(true);
            }
            return _instance;
        }
    }

    [Header("UI Panels")]
    public GameObject pauseModal;
    public GameObject confirmationPopup;
    public CanvasGroup modalCanvasGroup;

    [Header("Top-Right Open Button")]
    public Button openPauseButton;

    [Header("Modal Header & Close")]
    public Button closeHeaderButton;
    public Button overlayCloseButton;

    [Header("Action Buttons")]
    public Button resumeButton;
    public Button manualSaveButton;
    public Button returnMainMenuButton;
    public Button quitButton;

    [Header("Audio Controls")]
    public Slider bgmSlider;
    public Text txtBgmValue;
    public Slider sfxSlider;
    public Text txtSfxValue;
    public Toggle muteToggle;

    [Header("Status & Info Display")]
    public Text txtCharacterInfo;
    public Text txtSaveStatus;

    [Header("Confirmation Popup Elements")]
    public Text txtPopupTitle;
    public Text txtPopupMessage;
    public Button btnPopupConfirm;
    public Button btnPopupCancel;

    public bool IsModalOpen => isPaused;

    private Action pendingConfirmAction;
    private float previousTimeScale = 1f;
    private bool isPaused = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }
        _instance = this;
        isPaused = false;

        SetupEventListeners();
    }

    private void Start()
    {
        // Panel đã được tắt sẵn trong Scene, chỉ tải cấu hình âm thanh
        InitAudioSliders();
    }

    private void Update()
    {
        // Khi modal đang mở, nếu có popup xác nhận thì ESC chỉ đóng popup xác nhận
        if (confirmationPopup != null && confirmationPopup.activeSelf)
        {
            bool isEscapePressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                isEscapePressed = UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
            }

            if (isEscapePressed)
            {
                CloseConfirmation();
            }
        }
    }

    private void SetupEventListeners()
    {
        if (openPauseButton != null)
        {
            openPauseButton.onClick.RemoveAllListeners();
            openPauseButton.onClick.AddListener(OpenPauseMenu);
        }

        if (closeHeaderButton != null)
        {
            closeHeaderButton.onClick.RemoveAllListeners();
            closeHeaderButton.onClick.AddListener(ResumeGame);
        }

        if (overlayCloseButton != null)
        {
            overlayCloseButton.onClick.RemoveAllListeners();
            overlayCloseButton.onClick.AddListener(ResumeGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (manualSaveButton != null)
        {
            manualSaveButton.onClick.RemoveAllListeners();
            manualSaveButton.onClick.AddListener(OnManualSaveClicked);
        }

        if (returnMainMenuButton != null)
        {
            returnMainMenuButton.onClick.RemoveAllListeners();
            returnMainMenuButton.onClick.AddListener(OnReturnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitGameClicked);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.RemoveAllListeners();
            muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        }

        if (btnPopupCancel != null)
        {
            btnPopupCancel.onClick.RemoveAllListeners();
            btnPopupCancel.onClick.AddListener(CloseConfirmation);
        }

        if (btnPopupConfirm != null)
        {
            btnPopupConfirm.onClick.RemoveAllListeners();
            btnPopupConfirm.onClick.AddListener(() =>
            {
                pendingConfirmAction?.Invoke();
                CloseConfirmation();
            });
        }
    }

    private void InitAudioSliders()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.LoadSettings();

            if (bgmSlider != null)
            {
                bgmSlider.value = AudioManager.Instance.bgmVolume;
                if (txtBgmValue != null) txtBgmValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.bgmVolume * 100)}%";
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxVolume;
                if (txtSfxValue != null) txtSfxValue.text = $"{Mathf.RoundToInt(AudioManager.Instance.sfxVolume * 100)}%";
            }

            if (muteToggle != null)
            {
                muteToggle.isOn = AudioManager.Instance.isMuted;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MỞ / ĐÓNG PANEL PAUSE
    // ─────────────────────────────────────────────────────────────────────────
    public void OpenPauseMenu()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Đóng băng mọi tiến trình game khi tạm dừng

        gameObject.SetActive(true);
        if (pauseModal != null) pauseModal.SetActive(true);
        transform.SetAsLastSibling();

        RefreshInfoDisplay();
        InitAudioSliders();

        if (confirmationPopup != null) confirmationPopup.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale; // Khôi phục thời gian chạy game

        if (confirmationPopup != null) confirmationPopup.SetActive(false);
        if (pauseModal != null) pauseModal.SetActive(false);
        gameObject.SetActive(false); // Ẩn toàn bộ modal để DarkOverlay không chặn click màn hình
    }

    private void RefreshInfoDisplay()
    {
        // 1. Cập nhật thông tin nhân vật
        string charName = "Đạo Hữu";
        string stageName = "Luyện Khí";
        int combatStage = 1;

        if (CultivationManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(CultivationManager.Instance.CharacterName))
                charName = CultivationManager.Instance.CharacterName;

            if (CultivationManager.Instance.allStages != null && 
                CultivationManager.Instance.currentStageIndex < CultivationManager.Instance.allStages.Length)
            {
                stageName = CultivationManager.Instance.allStages[CultivationManager.Instance.currentStageIndex].stageName;
            }
        }

        if (StageManager.Instance != null)
        {
            combatStage = StageManager.Instance.currentStage;
        }

        int slotIdx = 0;
        if (DataManager.Instance != null && DataManager.Instance.gameSaveData != null)
        {
            slotIdx = DataManager.Instance.gameSaveData.lastPlayedSlotIndex;
            if (slotIdx < 0) slotIdx = 0;
        }

        if (txtCharacterInfo != null)
        {
            txtCharacterInfo.text = $"🏷️ <b>{charName}</b>  |  🌟 <color=#66FFB2>{stageName}</color>  |  ⚔️ Tầng <color=#FFE666>{combatStage}</color> (Slot {slotIdx + 1})";
        }

        if (txtSaveStatus != null)
        {
            txtSaveStatus.text = "💾 Trạng thái: <color=#88FF88>Tiến trình sẵn sàng đồng bộ</color>";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CÁC HÀNH ĐỘNG CHÍNH (SAVE, MAIN MENU, QUIT)
    // ─────────────────────────────────────────────────────────────────────────
    private void OnManualSaveClicked()
    {
        // Đồng bộ lưu game ngay lập tức
        SyncAndSaveGame();

        if (txtSaveStatus != null)
        {
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            txtSaveStatus.text = $"💾 Trạng thái: <color=#66FF66>✅ Đã lưu thành công lúc {timeStr}!</color>";
        }
    }

    private void OnReturnMainMenuClicked()
    {
        ShowConfirmation(
            "TRỞ VỀ MAIN MENU",
            "Đạo hữu có muốn <color=#FFE666>lưu lại toàn bộ tiến trình</color> và trở về Màn Hình Chính không?",
            () =>
            {
                // 1. Đồng bộ lưu dữ liệu triệt để
                SyncAndSaveGame();

                // 2. Mở khóa TimeScale trước khi chuyển Scene
                Time.timeScale = 1f;

                // 3. Nạp lại Scene để MainMenuManager tái khởi tạo giao diện chuẩn
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        );
    }

    private void OnQuitGameClicked()
    {
        ShowConfirmation(
            "THOÁT TRÒ CHƠI",
            "Đạo hữu có muốn <color=#FFE666>lưu lại tiến trình hiện tại</color> và thoát game không?",
            () =>
            {
                // 1. Đồng bộ lưu dữ liệu triệt để
                SyncAndSaveGame();

                // 2. Khôi phục thời gian
                Time.timeScale = 1f;

                // 3. Thoát Game
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ĐỒNG BỘ SAVE GAME CHẶT CHẼ VỚI DATAMANAGER
    // ─────────────────────────────────────────────────────────────────────────
    public static void SyncAndSaveGame()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogWarning("[PauseMenu] Không tìm thấy DataManager trong Scene!");
            return;
        }

        // 1. Đảm bảo lastPlayedSlotIndex hợp lệ (phòng trường hợp người chơi test thẳng Scene)
        if (DataManager.Instance.gameSaveData.lastPlayedSlotIndex < 0 ||
            DataManager.Instance.gameSaveData.lastPlayedSlotIndex >= DataManager.Instance.gameSaveData.slots.Length)
        {
            DataManager.Instance.gameSaveData.lastPlayedSlotIndex = 0;
        }

        int slotIdx = DataManager.Instance.gameSaveData.lastPlayedSlotIndex;
        var slot = DataManager.Instance.gameSaveData.slots[slotIdx];

        // 2. Nếu slot đang rỗng (do chưa qua màn hình tạo nhân vật của Menu), điền tên mặc định
        if (slot.isSlotEmpty)
        {
            slot.isSlotEmpty = false;
            string name = "Đạo Hữu";
            if (CultivationManager.Instance != null && !string.IsNullOrEmpty(CultivationManager.Instance.CharacterName))
            {
                name = CultivationManager.Instance.CharacterName;
            }
            slot.characterName = name;
        }

        // 3. Kích hoạt cờ session để DataManager chấp thuận lệnh Save
        DataManager.Instance.ActivateSession();

        // 4. Kích hoạt Save toàn diện từ tất cả các Manager vào Slot
        DataManager.Instance.SaveCurrentGame(silent: false);

        Debug.Log($"<color=cyan>[PauseMenu] 💾 ĐÃ ĐỒNG BỘ DỮ LIỆU THÀNH CÔNG VÀO SLOT {slotIdx + 1}!</color>");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CÀI ĐẶT ÂM THANH
    // ─────────────────────────────────────────────────────────────────────────
    private void OnBgmSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBgmVolume(val);
        }
        if (txtBgmValue != null)
        {
            txtBgmValue.text = $"{Mathf.RoundToInt(val * 100)}%";
        }
    }

    private void OnSfxSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(val);
        }
        if (txtSfxValue != null)
        {
            txtSfxValue.text = $"{Mathf.RoundToInt(val * 100)}%";
        }
    }

    private void OnMuteToggleChanged(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMute(isMuted);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CONFIRMATION POPUP
    // ─────────────────────────────────────────────────────────────────────────
    private void ShowConfirmation(string title, string message, Action onConfirm)
    {
        if (confirmationPopup == null)
        {
            onConfirm?.Invoke();
            return;
        }

        pendingConfirmAction = onConfirm;
        if (txtPopupTitle != null) txtPopupTitle.text = title;
        if (txtPopupMessage != null) txtPopupMessage.text = message;

        confirmationPopup.SetActive(true);
        confirmationPopup.transform.SetAsLastSibling();
    }

    public void CloseConfirmation()
    {
        pendingConfirmAction = null;
        if (confirmationPopup != null) confirmationPopup.SetActive(false);
    }
}

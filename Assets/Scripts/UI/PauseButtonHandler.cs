using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào PauseButton (Góc trên cùng bên phải).
/// Luôn hoạt động trên Canvas gameplay để:
/// 1. Nhận click mở PauseMenuModal ngay cả khi Modal đang bị ẩn/tắt hoàn toàn.
/// 2. Bắt phím tắt ESC trong Update() để Tạm dừng / Tiếp tục chơi mượt mà.
/// </summary>
public class PauseButtonHandler : MonoBehaviour
{
    public GameObject pauseMenuModal;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnPauseButtonClicked);
        }
    }

    private void Start()
    {
        FindModalIfNull();
    }

    private void Update()
    {
        // Bắt phím ESC qua Input System mới
        bool isEscapePressed = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            isEscapePressed = UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
        }

        if (isEscapePressed)
        {
            TogglePauseMenu();
        }
    }

    public void OnPauseButtonClicked()
    {
        TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {
        FindModalIfNull();

        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsModalOpen)
        {
            if (PauseMenuManager.Instance.confirmationPopup != null && 
                PauseMenuManager.Instance.confirmationPopup.activeSelf)
            {
                PauseMenuManager.Instance.CloseConfirmation();
                return;
            }

            PauseMenuManager.Instance.ResumeGame();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        FindModalIfNull();

        if (pauseMenuModal != null)
        {
            pauseMenuModal.SetActive(true);
        }

        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.OpenPauseMenu();
        }
    }

    private void FindModalIfNull()
    {
        if (pauseMenuModal == null)
        {
            Transform modal = transform.parent != null ? transform.parent.Find("PauseMenuModal") : null;
            if (modal == null)
            {
                var pmm = FindObjectOfType<PauseMenuManager>(true);
                if (pmm != null) modal = pmm.transform;
            }
            if (modal != null)
            {
                pauseMenuModal = modal.gameObject;
            }
        }
    }
}

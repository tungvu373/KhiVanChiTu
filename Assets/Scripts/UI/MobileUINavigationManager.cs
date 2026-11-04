using UnityEngine;

/// <summary>
/// Quản lý điều hướng tập trung chuẩn Mobile và PC (Android Back / Escape Navigation).
/// Tuân thủ nghiêm ngặt BRIEF_ANTIGRAVITY_UI_DA_NEN_TANG_2026-11-03:
/// - Phím Back/Escape luôn đóng lớp UI trên cùng trước.
/// - Ngăn ngừa click xuyên màn hình (click-through) khi Modal/Drawer đang mở.
/// - Hoạt động độc lập bằng Unscaled Time để đảm bảo luôn đóng được dù game đang Pause.
/// </summary>
public class MobileUINavigationManager : MonoBehaviour
{
    public static MobileUINavigationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Kiểm tra phím Escape (PC) và nút Back hệ thống (Android)
        bool backTriggered = false;

        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            backTriggered = true;
        }
        #endif

        if (!backTriggered)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    backTriggered = true;
                }
            }
            catch (System.InvalidOperationException)
            {
                // New Input System is active in Player Settings; ignore safely
            }
        }

        if (backTriggered)
        {
            HandleBackNavigation();
        }
    }

    /// <summary>
    /// Xử lý đóng theo ngăn xếp ưu tiên (Layered Stack Navigation)
    /// </summary>
    public bool HandleBackNavigation()
    {
        // ── ƯU TIÊN 1: ĐÓNG CÁC MODAL POPUP TRUNG TÂM ────────────────────────
        // 1a. Bảng Nhiệm Vụ Mốc (MilestoneModalUI)
        if (MilestoneModalUI.Instance != null && MilestoneModalUI.Instance.IsModalOpen)
        {
            MilestoneModalUI.Instance.Close();
            return true;
        }

        // 1b. Bảng Luyện Thể Chi Tiết (BodyTalentDetailModal)
        var bodyTalentModal = GameObject.Find("Canvas/MobileUIRoot/BodyTalentDetailModal");
        if (bodyTalentModal != null && bodyTalentModal.activeInHierarchy)
        {
            bodyTalentModal.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            Debug.Log("[UINav] Đã đóng BodyTalentDetailModal qua Back/Escape.");
            return true;
        }

        // 1c. Bảng Thông Tin Nhân Vật (CharacterDetailModal)
        var charDetailModal = GameObject.Find("Canvas/CharacterDetailModal");
        if (charDetailModal != null && charDetailModal.activeInHierarchy)
        {
            charDetailModal.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            Debug.Log("[UINav] Đã đóng CharacterDetailModal qua Back/Escape.");
            return true;
        }

        // 1d. Bảng Độ Kiếp (TribulationModalUI)
        if (TribulationModalUI.Instance != null && TribulationModalUI.Instance.gameObject.activeInHierarchy)
        {
            TribulationModalUI.Instance.gameObject.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            Debug.Log("[UINav] Đã đóng TribulationModalUI qua Back/Escape.");
            return true;
        }

        // 1e. Bảng Pause Menu nếu đang mở
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsModalOpen)
        {
            PauseMenuManager.Instance.ResumeGame();
            return true;
        }

        // ── ƯU TIÊN 2: ĐÓNG DRAWER ĐANG MỞ (TAB 0 ĐẾN 4) ──────────────────────
        if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsAnyDrawerOpen)
        {
            MobileDrawerUIController.Instance.CloseActiveDrawer();
            Debug.Log("[UINav] Đã trượt đóng Drawer qua Back/Escape.");
            return true;
        }

        // ── ƯU TIÊN 3: NẾU Ở NGOÀI MÀN HÌNH CHƠI TỰ DO -> MỞ PAUSE MENU ───────
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.OpenPauseMenu();
            Debug.Log("[UINav] Mở Pause Menu qua Escape.");
            return true;
        }

        return false;
    }
}

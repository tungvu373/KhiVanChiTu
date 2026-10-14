using UnityEngine;
using System;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// DATA STRUCTURES
// ─────────────────────────────────────────────────────────────────────────────
[Serializable]
public class SwordSaveEntry
{
    public int level = 1;
    public int pieces = 1;
    public bool isEquipped = false;
    public int equipSlotIndex = -1; // 0-3
}

[Serializable]
public class GameSaveData
{
    public CharacterSaveData[] slots = new CharacterSaveData[3]
    {
        new CharacterSaveData(), new CharacterSaveData(), new CharacterSaveData()
    };
    public int lastPlayedSlotIndex = -1;
}

[Serializable]
public class CharacterSaveData
{
    public bool isSlotEmpty = true;
    public string characterName = "";

    // ── CultivationManager ──────────────────────────────────────────────────
    public int stageIndex = 0;
    public string stageName = "Luyện Khí Kỳ";
    public float currentTuVi = 0f;
    public float permanentStatMultiplier = 1f;

    // ── EconomyManager ──────────────────────────────────────────────────────
    public float linhThach = 0f;
    public int coDuyen = 0;
    public int kiemY = 0;

    // ── UpgradeManager ──────────────────────────────────────────────────────
    public int linhLucLevel = 1;
    public int kiemYLevel = 1;
    public int thanThucLevel = 1;
    public int tuLinhLevel = 1;

    // ── SkillManager ────────────────────────────────────────────────────────
    public bool skillActiveUnlocked = false;
    public int skillActiveLevel = 1;
    public bool skillNormal0Unlocked = false;
    public int skillNormal0Level = 1;
    public bool skillNormal1Unlocked = false;
    public int skillNormal1Level = 1;
    public bool skillNormal2Unlocked = false;
    public int skillNormal2Level = 1;

    // ── CombatManager ───────────────────────────────────────────────────────
    public float playerMaxHp = 1000f;
    public float playerCurrentHp = 1000f;

    // ── MergeManager ────────────────────────────────────────────────────────
    public SwordSaveEntry[] swords = new SwordSaveEntry[0];

    // ── StageManager (Tầng Đánh Quái) ───────────────────────────────────
    public int combatStage = 1;
    public int enemiesKilled = 0;

    // ── Hướng Dẫn Tân Thủ (Tutorial) ──────────────────────────────────────────
    public bool tutorialCompleted = false;
    public bool tutorialSwordDone = false;
    public bool tutorialUpgradeDone = false;
    public bool tutorialCoDuyenDone = false;
    public bool tutorialShopDone = false;

    // ── Nhiệm Vụ Mốc Tu Luyện (Milestone Quests) ─────────────────────────────
    public string[] claimedMilestoneIds = new string[0];

    // ── Tiến trình ──────────────────────────────────────────────────────────
    public float playTime = 0f;
    public string lastPlayedDate = "";
}

// ─────────────────────────────────────────────────────────────────────────────
// DATA MANAGER
// ─────────────────────────────────────────────────────────────────────────────
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public GameSaveData gameSaveData = new GameSaveData();
    private const string SAVE_KEY = "KhiVanChiTu_SaveData";

    // Thời điểm bắt đầu session hiện tại (để tính thời gian chơi cộng dồn)
    private float sessionStartTime;
    // Cờ ngăn save trùng khi OnApplicationQuit và OnApplicationFocus(false) cùng kích hoạt
    private bool _isSaving = false;

    // Cờ an toàn tuyệt đối: Chỉ cho phép lưu đè khi người chơi ĐÃ THỰC SỰ VÀO GAME (Session Active)
    public bool isSessionActive { get; private set; } = false;

    public void ActivateSession()
    {
        isSessionActive = true;
    }

    private string BackupFilePath => System.IO.Path.Combine(Application.persistentDataPath, "KhiVanChiTu_SaveData.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void Start()
    {
        sessionStartTime = Time.realtimeSinceStartup;
        // Auto-Save định kỳ mỗi 30 giây (chỉ lưu khi đang thực sự chơi game)
        InvokeRepeating(nameof(AutoSave), 30f, 30f);
    }

    private void AutoSave()
    {
        // Tuyệt đối không auto-save khi đang ở Main Menu
        if (!isSessionActive) return;
        if (gameSaveData.lastPlayedSlotIndex < 0) return;
        
        SaveCurrentGame(silent: true);
    }

    // ── Bắt sự kiện tắt game đột ngột (Alt+F4, tắt cửa sổ, Task Manager) ──
    private void OnApplicationQuit()
    {
        // Chỉ lưu khi đã vào game (tránh trường hợp mở lên rồi tắt ngay ở Menu làm đè trắng save)
        if (isSessionActive)
        {
            SaveCurrentGame(silent: true);
        }
    }

    // ── Bắt sự kiện mất focus (Alt+Tab, minimize) trên Windows Build ───────
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isSessionActive)
        {
            SaveCurrentGame(silent: true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SAVE CURRENT GAME (TẬP TRUNG)
    // ─────────────────────────────────────────────────────────────────────────
    public void SaveCurrentGame(bool silent = false)
    {
        if (_isSaving) return;

        // CHỐT CHẶN AN TOÀN: Nếu chưa load nhân vật (đang ở Menu), TUYỆT ĐỐI KHÔNG GHI ĐÈ SAVE!
        if (!isSessionActive)
        {
            return;
        }

        _isSaving = true;

        int slotIdx = gameSaveData.lastPlayedSlotIndex;
        if (slotIdx < 0 || slotIdx >= gameSaveData.slots.Length)
        {
            _isSaving = false;
            return;
        }

        CharacterSaveData slot = gameSaveData.slots[slotIdx];
        if (slot.isSlotEmpty)
        {
            _isSaving = false;
            return;
        }

        // Cộng dồn thời gian chơi session hiện tại
        slot.playTime += Time.realtimeSinceStartup - sessionStartTime;
        sessionStartTime = Time.realtimeSinceStartup; // Reset để lần sau không cộng lại
        slot.lastPlayedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Gọi Save trên từng Manager (theo thứ tự)
        CultivationManager.Instance?.SaveToSlot(slot);
        EconomyManager.Instance?.SaveToSlot(slot);
        UpgradeManager.Instance?.SaveToSlot(slot);
        SkillManager.Instance?.SaveToSlot(slot);
        CombatManager.Instance?.SaveToSlot(slot);
        StageManager.Instance?.SaveToSlot(slot);
        MergeManager.Instance?.SaveToSlot(slot);
        TutorialManager.Instance?.SaveToSlot(slot);
        MilestoneManager.Instance?.SaveToSlot(slot);

        SaveData(); // Ghi xuống PlayerPrefs & File backup

        if (!silent)
            Debug.Log($"✅ Đã lưu nhân vật [{slot.characterName}] - {slot.stageName} (Tầng {slot.combatStage})");

        _isSaving = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOAD CURRENT GAME (TẬP TRUNG)
    // ─────────────────────────────────────────────────────────────────────────
    public void LoadCurrentGame()
    {
        int slotIdx = gameSaveData.lastPlayedSlotIndex;
        if (slotIdx < 0 || slotIdx >= gameSaveData.slots.Length) return;

        CharacterSaveData slot = gameSaveData.slots[slotIdx];
        if (slot.isSlotEmpty) return;

        sessionStartTime = Time.realtimeSinceStartup; // Reset bộ đếm session

        // Load theo thứ tự phụ thuộc: Cultivation → Economy → Upgrade → Skill → Combat → Stage → Merge → Tutorial → Milestone
        CultivationManager.Instance?.LoadFromSlot(slot);
        CultivationManager.Instance?.SetCharacterName(slot.characterName); // Truyền tên nhân vật
        EconomyManager.Instance?.LoadFromSlot(slot);
        UpgradeManager.Instance?.LoadFromSlot(slot);
        SkillManager.Instance?.LoadFromSlot(slot);
        CombatManager.Instance?.LoadFromSlot(slot);
        StageManager.Instance?.LoadFromSlot(slot);
        MergeManager.Instance?.LoadFromSlot(slot); // Cuối cùng vì gọi CombatManager.UpdateEquippedSwords
        TutorialManager.Instance?.LoadFromSlot(slot);
        MilestoneManager.Instance?.LoadFromSlot(slot);

        // KÍCH HOẠT CỜ SESSION: Bây giờ game đã nạp dữ liệu xong, cho phép hệ thống auto-save hoạt động
        isSessionActive = true;

        Debug.Log($"✅ Đã load thành công [{slot.characterName}] - {slot.stageName} | Tầng {slot.combatStage} | PlayTime: {slot.playTime:F0}s");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOW-LEVEL (PlayerPrefs + Sao lưu kép ra File JSON trên ổ cứng)
    // ─────────────────────────────────────────────────────────────────────────
    public void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");

        // Nếu PlayerPrefs rỗng (hoặc bị reset), thử tìm trong file JSON sao lưu dự phòng
        if (string.IsNullOrEmpty(json) && System.IO.File.Exists(BackupFilePath))
        {
            try
            {
                json = System.IO.File.ReadAllText(BackupFilePath);
                Debug.Log($"[DataManager] Đã phục hồi dữ liệu từ file sao lưu: {BackupFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataManager] Lỗi đọc file sao lưu: {ex.Message}");
            }
        }

        if (string.IsNullOrEmpty(json))
        {
            gameSaveData = new GameSaveData();
        }
        else
        {
            try
            {
                gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
                if (gameSaveData == null || gameSaveData.slots == null || gameSaveData.slots.Length != 3)
                {
                    gameSaveData = new GameSaveData();
                }
            }
            catch
            {
                gameSaveData = new GameSaveData();
            }
        }
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameSaveData, true);
            // 1. Lưu vào Windows Registry (PlayerPrefs)
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            // 2. Lưu kép vào file JSON ổ cứng (AppData) để đảm bảo không bao giờ mất
            System.IO.File.WriteAllText(BackupFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataManager] Lỗi ghi dữ liệu lưu: {ex.Message}");
        }
    }
}

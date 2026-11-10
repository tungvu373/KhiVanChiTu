using UnityEngine;
using System;
using System.Collections.Generic;
using KhiVanChiTu.Collaboration;

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
    public int swordStyle;
    public bool swordStyleUnlocked;
    public int encounterKillsRemaining;
    public int saveVersion = SaveMigration.CURRENT_SAVE_VERSION;
    public int realmId = 0;
    public int subLevel = 1;
    public int monsterSwordsAcquired = 0;
    public long lastActiveUtcTicks = 0;
    public OfflineEarningsManager.OfflineRewardData pendingOfflineReward;
    public int[] bodyStatLevels = new int[6];
    public bool[] bodyMilestonePassives = new bool[6];
    public AlchemySaveState alchemy = new AlchemySaveState();
    public ArtifactSaveState artifacts = new ArtifactSaveState();
    public FormationSaveState formations = new FormationSaveState();
    public SecretRealmSaveState secretRealm = new SecretRealmSaveState();

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

    // ── Hệ Thống Thành Tựu Tiên Lộ (Achievements) ───────────────────────────
    public AchievementSaveState achievements = new AchievementSaveState();

    // Vận Khí: permanent upgrades belong to this character slot. Keep daoAn for save compatibility.
    public DaoAnSaveState daoAn = new DaoAnSaveState();

    // ── StorageManager (Kho Đồ, Bộ Lọc & Lò Phân Giải) ──────────────────────
    public StorageSaveState storage = new StorageSaveState();

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
    private string saveProfileId = string.Empty;
    public string SaveStorageKey => WorkspaceSaveProfile.Key(SAVE_KEY, saveProfileId);
    public string SaveProfileId => saveProfileId;

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

    private string BackupFilePath => System.IO.Path.Combine(Application.persistentDataPath,
        WorkspaceSaveProfile.FileName("KhiVanChiTu_SaveData.json", saveProfileId));

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        try
        {
            string detectedProfile = WorkspaceSaveProfile.Resolve(scene);
            if (!string.IsNullOrEmpty(detectedProfile) && detectedProfile != saveProfileId)
            {
                Debug.Log($"[DataManager] Chuyển đổi saveProfileId từ '{saveProfileId}' sang '{detectedProfile}' theo scene '{scene.name}'");
                saveProfileId = detectedProfile;
                LoadData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DataManager] Lỗi phân giải saveProfile trên scene {scene.name}: {ex.Message}");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Capture before DontDestroyOnLoad moves this object out of its authoring scene.
        // Keeping this immutable also isolates autosave/quit after the workspace scene unloads.
        saveProfileId = WorkspaceSaveProfile.Resolve(gameObject.scene);
        Instance = this;
        if (GetComponent<GameplayChoicesService>() == null) gameObject.AddComponent<GameplayChoicesService>();
        DontDestroyOnLoad(gameObject);
        if (SwordLoadoutService.Instance == null && GetComponent<SwordLoadoutService>() == null)
            gameObject.AddComponent<SwordLoadoutService>();
        if (PlayerStatsService.Instance == null && GetComponent<PlayerStatsService>() == null)
            gameObject.AddComponent<PlayerStatsService>();
        if (DaoAnProgressionService.Instance == null && GetComponent<DaoAnProgressionService>() == null)
            gameObject.AddComponent<DaoAnProgressionService>();
        if (DaoAnModalUI.Instance == null && GetComponent<DaoAnModalUI>() == null)
            gameObject.AddComponent<DaoAnModalUI>();
        if (AlchemyService.Instance == null && GetComponent<AlchemyService>() == null)
            gameObject.AddComponent<AlchemyService>();
        if (ArtifactService.Instance == null && GetComponent<ArtifactService>() == null)
            gameObject.AddComponent<ArtifactService>();
        if (FormationService.Instance == null && GetComponent<FormationService>() == null)
            gameObject.AddComponent<FormationService>();
        if (SecretRealmService.Instance == null && GetComponent<SecretRealmService>() == null)
            gameObject.AddComponent<SecretRealmService>();
        if (OfflineEarningsManager.Instance == null && GetComponent<OfflineEarningsManager>() == null)
            gameObject.AddComponent<OfflineEarningsManager>();
        if (StorageManager.Instance == null && GetComponent<StorageManager>() == null)
            gameObject.AddComponent<StorageManager>();
        if (DismantleService.Instance == null && GetComponent<DismantleService>() == null)
            gameObject.AddComponent<DismantleService>();
        if (FeatureUnlockService.Instance == null && GetComponent<FeatureUnlockService>() == null)
            gameObject.AddComponent<FeatureUnlockService>();
        if (LootRewardService.Instance == null && GetComponent<LootRewardService>() == null)
            gameObject.AddComponent<LootRewardService>();
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
        else if (hasFocus && isSessionActive)
        {
            RefreshOfflineReward();
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (!isSessionActive) return;
        if (isPaused) SaveCurrentGame(silent: true);
        else RefreshOfflineReward();
    }

    private void RefreshOfflineReward()
    {
        int index = gameSaveData.lastPlayedSlotIndex;
        if (index < 0 || index >= gameSaveData.slots.Length) return;
        OfflineEarningsManager.Instance?.ProcessOfflineEarnings(gameSaveData.slots[index]);
        SaveCurrentGame(silent: true);
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
        PlayerStatsService.Instance?.SaveToSlot(slot);
        AlchemyService.Instance?.SaveToSlot(slot);
        ArtifactService.Instance?.SaveToSlot(slot);
        FormationService.Instance?.SaveToSlot(slot);
        SecretRealmService.Instance?.SaveToSlot(slot);
        OfflineEarningsManager.Instance?.SaveToSlot(slot);
        UpgradeManager.Instance?.SaveToSlot(slot);
        SkillManager.Instance?.SaveToSlot(slot);
        CombatManager.Instance?.SaveToSlot(slot);
        StageManager.Instance?.SaveToSlot(slot);
        MergeManager.Instance?.SaveToSlot(slot);
        TutorialManager.Instance?.SaveToSlot(slot);
        MilestoneManager.Instance?.SaveToSlot(slot);
        AchievementManager.Instance?.SaveToSlot(slot);
        DaoAnProgressionService.Instance?.SaveToSlot(slot);
        StorageManager.Instance?.SaveToSlot(slot);

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
        PlayerStatsService.Instance?.LoadFromSlot(slot);
        AlchemyService.Instance?.LoadFromSlot(slot);
        ArtifactService.Instance?.LoadFromSlot(slot);
        FormationService.Instance?.LoadFromSlot(slot);
        SecretRealmService.Instance?.LoadFromSlot(slot);
        UpgradeManager.Instance?.LoadFromSlot(slot);
        SkillManager.Instance?.LoadFromSlot(slot);
        CombatManager.Instance?.LoadFromSlot(slot);
        StageManager.Instance?.LoadFromSlot(slot);
        MergeManager.Instance?.LoadFromSlot(slot); // Cuối cùng vì gọi CombatManager.UpdateEquippedSwords
        TutorialManager.Instance?.LoadFromSlot(slot);
        MilestoneManager.Instance?.LoadFromSlot(slot);
        AchievementManager.Instance?.LoadFromSlot(slot);
        DaoAnProgressionService.Instance?.LoadFromSlot(slot);
        StorageManager.Instance?.LoadFromSlot(slot);
        OfflineEarningsManager.Instance?.ProcessOfflineEarnings(slot);

        // KÍCH HOẠT CỜ SESSION: Bây giờ game đã nạp dữ liệu xong, cho phép hệ thống auto-save hoạt động
        isSessionActive = true;
        // Ghi ngay mốc UTC và phần thưởng chờ nhận; thoát game trước khi claim vẫn giữ được thưởng.
        SaveCurrentGame(silent: true);

        Debug.Log($"✅ Đã load thành công [{slot.characterName}] - {slot.stageName} | Tầng {slot.combatStage} | PlayTime: {slot.playTime:F0}s");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOW-LEVEL (Crash-safe Atomic Write + Multi-tier Fallback Recovery)
    // ─────────────────────────────────────────────────────────────────────────
    public void LoadData()
    {
        gameSaveData = null;

        // Tier 1: Ưu tiên đọc từ file JSON chính trên ổ cứng
        if (System.IO.File.Exists(BackupFilePath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(BackupFilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    var parsed = JsonUtility.FromJson<GameSaveData>(json);
                    if (ValidateGameSaveData(parsed))
                    {
                        gameSaveData = parsed;
                        Debug.Log($"[DataManager] ✅ Đã nạp dữ liệu an toàn từ file chính: {BackupFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManager] File chính bị lỗi/hỏng ({ex.Message}), đang chuyển sang file .bak dự phòng...");
            }
        }

        // Tier 2: Nếu file chính hỏng hoặc không có, đọc từ file .bak (Last Known Good)
        string bakPath = BackupFilePath + ".bak";
        if (gameSaveData == null && System.IO.File.Exists(bakPath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(bakPath);
                if (!string.IsNullOrEmpty(json))
                {
                    var parsed = JsonUtility.FromJson<GameSaveData>(json);
                    if (ValidateGameSaveData(parsed))
                    {
                        gameSaveData = parsed;
                        Debug.Log($"[DataManager] ♻️ Đã phục hồi thành công dữ liệu từ file sao lưu .bak: {bakPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManager] File .bak cũng không thể đọc được: {ex.Message}");
            }
        }

        // Tier 3: Nếu cả 2 file trên đĩa không được, thử phục hồi từ PlayerPrefs
        if (gameSaveData == null)
        {
            string prefJson = PlayerPrefs.GetString(SaveStorageKey, "");
            if (!string.IsNullOrEmpty(prefJson))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<GameSaveData>(prefJson);
                    if (ValidateGameSaveData(parsed))
                    {
                        gameSaveData = parsed;
                        Debug.Log("[DataManager] ♻️ Đã phục hồi dữ liệu từ PlayerPrefs.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DataManager] PlayerPrefs cũng bị lỗi cú pháp: {ex.Message}");
                }
            }
        }

        // Tier 4: Nếu không còn nguồn nào hợp lệ (cài mới hoàn toàn), khởi tạo dữ liệu mặc định
        if (gameSaveData == null)
        {
            Debug.Log("[DataManager] Khởi tạo dữ liệu save trắng (3 slot mới hoàn toàn).");
            gameSaveData = new GameSaveData();
        }

        // V3: Chạy migration cho cả 3 slot một lần khi load
        bool migrationOccurred = false;
        if (gameSaveData != null && gameSaveData.slots != null)
        {
            foreach (var slot in gameSaveData.slots)
            {
                if (SaveMigration.Run(slot)) migrationOccurred = true;
            }
            if (migrationOccurred)
            {
                Debug.Log("[DataManager] Migration hoàn tất. Đang lưu lại toàn bộ 3 slot...");
                SaveData();
            }
        }
    }

    private bool ValidateGameSaveData(GameSaveData data)
    {
        return data != null && data.slots != null && data.slots.Length == 3;
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameSaveData, true);
            if (string.IsNullOrEmpty(json)) return;

            string targetPath = BackupFilePath;
            string tmpPath = targetPath + ".tmp";
            string bakPath = targetPath + ".bak";

            // 1. Ghi ra file tạm (.tmp) trước
            System.IO.File.WriteAllText(tmpPath, json);

            // 2. Nếu file chính đã tồn tại, copy sang .bak làm lớp đệm an toàn
            if (System.IO.File.Exists(targetPath))
            {
                try
                {
                    System.IO.File.Copy(targetPath, bakPath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DataManager] Không thể cập nhật file .bak: {ex.Message}");
                }
            }

            // 3. Hoán đổi nguyên tử: Xóa file chính cũ (nếu có) và Move .tmp sang file chính
            if (System.IO.File.Exists(targetPath))
            {
                System.IO.File.Delete(targetPath);
            }
            System.IO.File.Move(tmpPath, targetPath);

            // 4. Lưu vào PlayerPrefs làm kênh phụ trợ
            PlayerPrefs.SetString(SaveStorageKey, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DataManager] Lỗi ghi dữ liệu lưu: {ex.Message}");
        }
    }
}

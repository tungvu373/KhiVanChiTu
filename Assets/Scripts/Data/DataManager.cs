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
    public bool skillNormal0Unlocked = false;
    public bool skillNormal1Unlocked = false;
    public bool skillNormal2Unlocked = false;

    // ── CombatManager ───────────────────────────────────────────────────────
    public float playerMaxHp = 1000f;
    public float playerCurrentHp = 1000f;

    // ── MergeManager ────────────────────────────────────────────────────────
    public SwordSaveEntry[] swords = new SwordSaveEntry[0];

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
        // Auto-Save định kỳ mỗi 30 giây (chạy cả trong Editor lẫn Build)
        InvokeRepeating(nameof(AutoSave), 30f, 30f);
    }

    private void AutoSave()
    {
        // Chỉ auto-save khi đang thực sự chơi (không phải đang ở Menu)
        if (gameSaveData.lastPlayedSlotIndex < 0) return;
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.IdleFarm
            || GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm)
        {
            SaveCurrentGame(silent: true);
        }
    }

    // ── Bắt sự kiện tắt game đột ngột (Alt+F4, tắt cửa sổ, Task Manager) ──
    private void OnApplicationQuit()
    {
        SaveCurrentGame(silent: true);
    }

    // ── Bắt sự kiện mất focus (Alt+Tab, minimize) trên Windows Build ───────
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
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
        MergeManager.Instance?.SaveToSlot(slot);

        SaveData(); // Ghi xuống PlayerPrefs

        if (!silent)
            Debug.Log($"✅ Đã lưu nhân vật [{slot.characterName}] - {slot.stageName}");

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

        // Load theo thứ tự phụ thuộc: Cultivation → Economy → Upgrade → Skill → Combat → Merge
        CultivationManager.Instance?.LoadFromSlot(slot);
        CultivationManager.Instance?.SetCharacterName(slot.characterName); // Truyền tên nhân vật
        EconomyManager.Instance?.LoadFromSlot(slot);
        UpgradeManager.Instance?.LoadFromSlot(slot);
        SkillManager.Instance?.LoadFromSlot(slot);
        CombatManager.Instance?.LoadFromSlot(slot);
        MergeManager.Instance?.LoadFromSlot(slot); // Cuối cùng vì gọi CombatManager.UpdateEquippedSwords

        Debug.Log($"✅ Đã load nhân vật [{slot.characterName}] - {slot.stageName} | PlayTime: {slot.playTime:F0}s");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LOW-LEVEL (đọc/ghi PlayerPrefs)
    // ─────────────────────────────────────────────────────────────────────────
    public void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json))
            gameSaveData = new GameSaveData();
        else
            gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(gameSaveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }
}

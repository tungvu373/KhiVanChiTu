using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QUẢN LÝ KHO TRỮ VẬT, BỘ LỌC THU THẬP ƯU TIÊN & HÒM TẠM CHỜ LỌC (STORAGE MANAGER).
/// Giải quyết triệt để bài toán vật phẩm phình to:
/// - Lọc và ưu tiên nhận vật phẩm giá trị cao trước.
/// - Hòm tạm trữ an toàn khi kho Merge hoặc túi đồ bị đầy.
/// - Tự động tái chế / phân giải tài nguyên rác theo cấu hình người chơi.
/// </summary>
public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance { get; private set; }

    public StorageSaveState State { get; private set; } = new StorageSaveState();

    public event Action OnStorageChanged;

    public bool FilterSwordsHighLevel
    {
        get => State.filterSwordsHighLevel;
        set { State.filterSwordsHighLevel = value; OnStorageChanged?.Invoke(); }
    }

    public bool FilterPillsBreakthrough
    {
        get => State.filterPillsBreakthrough;
        set { State.filterPillsBreakthrough = value; OnStorageChanged?.Invoke(); }
    }

    public bool FilterRareHerbs
    {
        get => State.filterRareHerbs;
        set { State.filterRareHerbs = value; OnStorageChanged?.Invoke(); }
    }

    public bool AutoDismantleLv1SwordsWhenFull
    {
        get => State.autoDismantleLv1SwordsWhenFull;
        set { State.autoDismantleLv1SwordsWhenFull = value; OnStorageChanged?.Invoke(); }
    }

    public bool AutoDismantleWastePills
    {
        get => State.autoDismantleWastePills;
        set { State.autoDismantleWastePills = value; OnStorageChanged?.Invoke(); }
    }

    public int PendingCount => State.pendingLoot != null ? State.pendingLoot.Count : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. XỬ LÝ RƠI ĐỒ THÔNG MINH (SMART LOOT HANDLING)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Xử lý khi quái hoặc rương rơi Kiếm Phôi.
    /// Tự động kiểm tra sức chứa, bộ lọc ưu tiên và tự động phân giải nếu kho đầy.
    /// </summary>
    public bool HandleSwordDrop(int level, bool isMonsterDrop = false)
    {
        level = Mathf.Clamp(level, 1, 8);

        // 1. Kiểm tra kho merge có ô trống không
        if (MergeManager.Instance != null && MergeManager.Instance.HasEmptyInventorySlot())
        {
            bool added = MergeManager.Instance.TryAddSword(level);
            if (added && isMonsterDrop)
            {
                FeatureUnlockService.Instance?.OnMonsterSwordAcquired(level);
            }
            return added;
        }

        // 2. KHO MERGE ĐÃ ĐẦY:
        // CỔNG: Chỉ cho phép tự động phân giải nếu tính năng Tách Trang Bị ĐÃ MỞ (sau 500 kiếm quái rơi)
        bool canAutoDismantle = AutoDismantleLv1SwordsWhenFull && (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsDismantleUnlocked());

        if (level == 1 && canAutoDismantle)
        {
            // Tự động phân giải ngay kiếm Lv1 để lấy Linh Thạch
            if (DismantleService.Instance != null)
            {
                var reward = DismantleService.Instance.CalculateSwordReward(1, 1);
                DismantleService.Instance.ApplyReward(reward);
                if (GameLogger.Instance != null)
                {
                    GameLogger.Instance.Log($"Kho kiếm đầy: Tự động phân giải Kiếm Lv1 -> {reward.GetFormattedSummary()}", Color.yellow);
                }
            }
            if (isMonsterDrop)
            {
                FeatureUnlockService.Instance?.OnMonsterSwordAcquired(level);
            }
            return true;
        }

        // 3. Nếu chưa mở tách hoặc tắt auto dismantle hoặc kiếm cấp cao: Chuyển vào Hòm Tạm Chờ Lọc
        EnqueuePendingSword(level, 1);
        if (isMonsterDrop)
        {
            FeatureUnlockService.Instance?.OnMonsterSwordAcquired(level);
        }
        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Kho kiếm đầy! {DismantleService.GetSwordDisplayName(level)} đã chuyển vào Hòm Tạm Chờ Lọc.", Color.cyan);
        }
        return true;
    }

    /// <summary>
    /// Tự động phân giải phế đan ngay khi luyện ra nếu bật chế độ tự động
    /// </summary>
    public void HandleWastePillCrafted()
    {
        if (AutoDismantleWastePills && DismantleService.Instance != null)
        {
            // Nhận 2 Dược Trần + 10 Linh Thạch
            var reward = DismantleService.Instance.CalculatePillReward(0, PillRecipe.Cultivation, PillQuality.Waste, 1);
            DismantleService.Instance.ApplyReward(reward);
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"Lò Luyện: Phế Đan tự phân giải -> {reward.GetFormattedSummary()}", Color.gray);
            }
        }
        else if (AlchemyService.Instance != null)
        {
            AlchemyService.Instance.State.medicineDust += 2;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. HÒM TẠM CHỜ LỌC (PENDING LOOT QUEUE)
    // ─────────────────────────────────────────────────────────────────────────

    public List<PendingLootItem> GetPendingItems()
    {
        if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
        return State.pendingLoot;
    }

    public void EnqueuePendingSword(int level, int pieces = 1)
    {
        if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
        string name = DismantleService.GetSwordDisplayName(level);
        State.pendingLoot.Add(new PendingLootItem(PendingLootType.Sword, name, level, pieces, 1));
        OnStorageChanged?.Invoke();
    }

    public void EnqueuePendingPill(int realm, int recipe, int quality, int amount = 1)
    {
        if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
        State.pendingLoot.Add(new PendingLootItem(realm, recipe, quality, amount));
        OnStorageChanged?.Invoke();
    }

    public void EnqueuePendingHerb(int herbIndex, int amount)
    {
        if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
        string[] names = { "Linh Chi", "Cỏ Ngưng Khí", "Tuyết Liên" };
        string hName = (herbIndex >= 0 && herbIndex < 3) ? names[herbIndex] : "Thảo Dược";
        State.pendingLoot.Add(new PendingLootItem(PendingLootType.Herb, hName, herbIndex, 1, amount));
        OnStorageChanged?.Invoke();
    }

    public void EnqueuePendingOre(int amount)
    {
        if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
        State.pendingLoot.Add(new PendingLootItem(PendingLootType.Ore, "Quặng Phôi Pháp Khí", 1, 1, amount));
        OnStorageChanged?.Invoke();
    }

    public void RemovePendingItem(PendingLootItem item)
    {
        TryRemovePendingItem(item);
    }

    public bool TryRemovePendingItem(PendingLootItem item)
    {
        if (item == null || State.pendingLoot == null || !State.pendingLoot.Remove(item)) return false;
        OnStorageChanged?.Invoke();
        return true;
    }

    /// <summary>Compact the queue once and notify only after the whole batch has been removed.</summary>
    public int RemovePendingItems(Predicate<PendingLootItem> shouldRemove)
    {
        if (shouldRemove == null || State.pendingLoot == null) return 0;
        int removed = State.pendingLoot.RemoveAll(shouldRemove);
        if (removed > 0) OnStorageChanged?.Invoke();
        return removed;
    }

    public void ClearPendingLoot()
    {
        if (State.pendingLoot != null)
        {
            State.pendingLoot.Clear();
            OnStorageChanged?.Invoke();
        }
    }

    /// <summary>
    /// Lọc những thứ ưu tiên nhận trước từ Hòm Tạm vào Túi/Kho:
    /// - Ưu tiên 1: Đan Dược Đột Phá & Cực Phẩm.
    /// - Ưu tiên 2: Kiếm cấp cao nhất có thể nhét vào ô trống kho merge.
    /// - Ưu tiên 3: Thảo dược quý (Tuyết Liên) và Quặng.
    /// </summary>
    public int ClaimPriorityLoot()
    {
        if (State.pendingLoot == null || State.pendingLoot.Count == 0) return 0;

        int claimedCount = 0;

        // 1. Sắp xếp danh sách theo độ ưu tiên giảm dần
        State.pendingLoot.Sort((a, b) => GetLootPriorityScore(b).CompareTo(GetLootPriorityScore(a)));

        for (int i = State.pendingLoot.Count - 1; i >= 0; i--)
        {
            var item = State.pendingLoot[i];
            bool claimed = false;

            switch (item.itemType)
            {
                case PendingLootType.Sword:
                    if (MergeManager.Instance != null && MergeManager.Instance.HasEmptyInventorySlot())
                    {
                        if (MergeManager.Instance.TryAddSword(item.level))
                        {
                            claimed = true;
                        }
                    }
                    break;

                case PendingLootType.Pill:
                    if (AlchemyService.Instance != null)
                    {
                        int pIdx = item.realm * 8 + item.recipe * 4 + item.quality;
                        if (pIdx >= 0 && pIdx < AlchemyService.Instance.State.pills.Length)
                        {
                            AlchemyService.Instance.State.pills[pIdx] += item.amount;
                            claimed = true;
                        }
                    }
                    break;

                case PendingLootType.Herb:
                    if (AlchemyService.Instance != null && item.level >= 0 && item.level < 3)
                    {
                        AlchemyService.Instance.AddHerb(item.level, item.amount);
                        claimed = true;
                    }
                    break;

                case PendingLootType.Ore:
                    if (ArtifactService.Instance != null)
                    {
                        ArtifactService.Instance.AddOre(item.amount);
                        claimed = true;
                    }
                    break;
            }

            if (claimed)
            {
                State.pendingLoot.RemoveAt(i);
                claimedCount++;
            }
        }

        if (claimedCount > 0)
        {
            OnStorageChanged?.Invoke();
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"Đã lọc & nhận ưu tiên {claimedCount} vật phẩm từ Hòm Tạm!", Color.green);
            }
        }
        else
        {
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("Kho kiếm đã đầy! Hãy phân giải bớt kiếm phôi để nhận thêm.", Color.yellow);
            }
        }

        return claimedCount;
    }

    /// <summary>
    /// Tính điểm ưu tiên của vật phẩm trong Hòm Tạm (điểm càng cao càng được nhận trước)
    /// </summary>
    private int GetLootPriorityScore(PendingLootItem item)
    {
        if (item == null) return 0;
        int score = 0;
        switch (item.itemType)
        {
            case PendingLootType.Sword:
                score = 1000 + item.level * 100 + item.pieces * 10;
                break;
            case PendingLootType.Pill:
                score = 500 + item.quality * 100 + (item.recipe == (int)PillRecipe.Breakthrough ? 300 : 0) + item.realm * 20;
                break;
            case PendingLootType.Herb:
                score = 300 + item.level * 100; // Tuyết Liên level 2 = 500 điểm
                break;
            case PendingLootType.Ore:
                score = 400;
                break;
        }
        return score;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. LƯU & TẢI DỮ LIỆU (SAVE / LOAD)
    // ─────────────────────────────────────────────────────────────────────────

    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot != null)
        {
            slot.storage = State;
        }
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (slot != null && slot.storage != null)
        {
            State = slot.storage;
            if (State.pendingLoot == null) State.pendingLoot = new List<PendingLootItem>();
            OnStorageChanged?.Invoke();
        }
    }
}


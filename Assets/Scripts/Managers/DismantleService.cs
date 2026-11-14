using System;
using System.Text;
using UnityEngine;

[Serializable]
public class DismantleReward
{
    public float linhThach;
    public int kiemY;
    public int tinhThietOre;
    public int medicineDust;
    public int[] herbs = new int[3]; // 0: Linh Chi, 1: Cỏ Ngưng Khí, 2: Tuyết Liên

    public bool IsEmpty => linhThach <= 0 && kiemY <= 0 && tinhThietOre <= 0 && medicineDust <= 0 
        && (herbs == null || (herbs[0] <= 0 && herbs[1] <= 0 && herbs[2] <= 0));

    public void Add(DismantleReward other)
    {
        if (other == null) return;
        linhThach += other.linhThach;
        kiemY += other.kiemY;
        tinhThietOre += other.tinhThietOre;
        medicineDust += other.medicineDust;
        if (other.herbs != null)
        {
            if (herbs == null) herbs = new int[3];
            for (int i = 0; i < 3 && i < other.herbs.Length; i++)
            {
                herbs[i] += other.herbs[i];
            }
        }
    }

    public string GetFormattedSummary()
    {
        StringBuilder sb = new StringBuilder();
        if (linhThach > 0) sb.Append($"<color=#72D2FF>+{linhThach:N0} Linh Thạch</color> ");
        if (kiemY > 0) sb.Append($"<color=#E066FF>+{kiemY} Kiếm Ý</color> ");
        if (tinhThietOre > 0) sb.Append($"<color=#FFB90F>+{tinhThietOre} Quặng</color> ");
        if (medicineDust > 0) sb.Append($"<color=#98FB98>+{medicineDust} Dược Trần</color> ");
        if (herbs != null)
        {
            if (herbs[0] > 0) sb.Append($"<color=#7FFF00>+{herbs[0]} Linh Chi</color> ");
            if (herbs[1] > 0) sb.Append($"<color=#00FA9A>+{herbs[1]} Cỏ NK</color> ");
            if (herbs[2] > 0) sb.Append($"<color=#00FFFF>+{herbs[2]} Tuyết Liên</color> ");
        }
        return sb.Length > 0 ? sb.ToString().Trim() : "Không có tài nguyên";
    }
}

/// <summary>
/// HỆ THỐNG TÁCH VẬT PHẨM & TÁI CHẾ TÀI NGUYÊN (DISMANTLE SYSTEM).
/// Tuân thủ nguyên tắc kinh tế chống lạm phát (Deflationary Valuation Economy):
/// - Thu hồi chuẩn 60% giá trị quy đổi cơ bản, bảo toàn công sức cày cuốc.
/// - Nhận diện vật phẩm quý giá / cày cuốc cao để bảo vệ người chơi bằng Modal xác nhận.
/// </summary>
public class DismantleService : MonoBehaviour
{
    public static DismantleService Instance { get; private set; }

    public event Action OnDismantleExecuted;
    private bool batchInProgress;

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
    // 1. TÍNH TOÁN GIÁ TRỊ PHÂN GIẢI (ANTI-INFLATION REWARD FORMULAS)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tính giá trị hoàn trả khi phân giải Kiếm.
    /// Giá phôi cơ bản Lv1 = 100 Linh Thạch. Hệ số thu hồi: 60%.
    /// Có thưởng thêm Quặng Tinh Thiết và Kiếm Ý cho kiếm cấp cao.
    /// </summary>
    public DismantleReward CalculateSwordReward(int level, int pieces)
    {
        DismantleReward reward = new DismantleReward();
        level = Mathf.Clamp(level, 1, 8);
        pieces = Mathf.Max(1, pieces);

        // Bảng giá cơ sở cho 1 phôi ở từng cấp (đã tính theo 60% tổng phôi cấu thành)
        float baseStonePerPiece = 60f;
        int orePerPiece = 0;
        int kiemYPerPiece = 0;

        switch (level)
        {
            case 1:
                baseStonePerPiece = 60f;
                break;
            case 2:
                baseStonePerPiece = 120f;
                break;
            case 3:
                baseStonePerPiece = 360f;
                orePerPiece = 1;
                break;
            case 4:
                baseStonePerPiece = 1440f;
                orePerPiece = 2;
                break;
            case 5:
                baseStonePerPiece = 7200f;
                orePerPiece = 5;
                kiemYPerPiece = 1;
                break;
            case 6:
                baseStonePerPiece = 43200f;
                orePerPiece = 10;
                kiemYPerPiece = 3;
                break;
            case 7:
                baseStonePerPiece = 302400f;
                orePerPiece = 25;
                kiemYPerPiece = 10;
                break;
            case 8:
                baseStonePerPiece = 2419200f;
                orePerPiece = 50;
                kiemYPerPiece = 25;
                break;
        }

        // Tách kiếm không phát Quặng trước Trúc Cơ (KE_HOACH_TAI_CAU_TRUC_MO_KHOA_THEO_TU_VI.md)
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.CanDropOre())
        {
            orePerPiece = 0;
        }

        reward.linhThach = baseStonePerPiece * pieces;
        reward.tinhThietOre = orePerPiece * pieces;
        reward.kiemY = kiemYPerPiece * pieces;
        return reward;
    }

    /// <summary>
    /// Tính giá trị hoàn trả khi phân giải Đan Dược.
    /// </summary>
    public DismantleReward CalculatePillReward(int realm, PillRecipe recipe, PillQuality quality, int count)
    {
        DismantleReward reward = new DismantleReward();
        if (count <= 0) return reward;

        realm = Mathf.Clamp(realm, 0, 8);
        float realmMult = 1f + realm * 0.5f;
        float recipeMult = (recipe == PillRecipe.Breakthrough) ? 1.5f : 1.0f;

        int dustPerPill = 2;
        float stonePerPill = 10f;

        switch (quality)
        {
            case PillQuality.Waste:
                dustPerPill = 2;
                stonePerPill = 10f;
                break;
            case PillQuality.Basic:
                dustPerPill = 4;
                stonePerPill = 50f * realmMult * recipeMult;
                reward.herbs[0] += 1 * count; // Hoàn lại 1 Linh Chi
                break;
            case PillQuality.Refined:
                dustPerPill = 8;
                stonePerPill = 150f * realmMult * recipeMult;
                reward.herbs[0] += 1 * count; // 1 Linh Chi
                reward.herbs[1] += 1 * count; // 1 Cỏ Ngưng Khí
                break;
            case PillQuality.Supreme:
                dustPerPill = 15;
                stonePerPill = 500f * realmMult * recipeMult;
                reward.herbs[1] += 1 * count; // 1 Cỏ Ngưng Khí
                reward.herbs[2] += 1 * count; // 1 Tuyết Liên
                break;
        }

        reward.medicineDust = dustPerPill * count;
        reward.linhThach = stonePerPill * count;
        return reward;
    }

    public DismantleReward CalculateOreReward(int count)
    {
        DismantleReward reward = new DismantleReward();
        if (count <= 0) return reward;
        reward.linhThach = count * 50f;
        return reward;
    }

    public DismantleReward CalculateHerbReward(int herbIndex, int count)
    {
        DismantleReward reward = new DismantleReward();
        if (count <= 0) return reward;
        float basePrice = 30f;
        if (herbIndex == 1) basePrice = 60f;
        if (herbIndex == 2) basePrice = 150f;
        reward.linhThach = count * basePrice;
        return reward;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. NHẬN DIỆN VẬT PHẨM GIÁ TRỊ CAO (HIGH VALUE / EFFORT CHECK)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsHighValueSword(int level, int pieces, bool isEquipped = false)
    {
        // Kiếm đang xuất trận hoặc Kiếm cấp >= 3 hoặc tích luỹ >= 3 phôi
        if (isEquipped) return true;
        return level >= 3 || pieces >= 3;
    }

    public bool IsHighValuePill(int realm, PillRecipe recipe, PillQuality quality)
    {
        // Đan Đột Phá hoặc Đan Cực Phẩm hoặc Đan Trung Phẩm cảnh giới cao (>= Nguyên Anh)
        if (recipe == PillRecipe.Breakthrough) return true;
        if (quality == PillQuality.Supreme) return true;
        if (realm >= 3 && quality >= PillQuality.Refined) return true;
        return false;
    }

    public bool IsHighValuePendingItem(PendingLootItem item)
    {
        if (item == null) return false;
        switch (item.itemType)
        {
            case PendingLootType.Sword:
                return IsHighValueSword(item.level, item.pieces);
            case PendingLootType.Pill:
                return IsHighValuePill(item.realm, (PillRecipe)item.recipe, (PillQuality)item.quality);
            case PendingLootType.Ore:
                return item.amount >= 50;
            case PendingLootType.Herb:
                return item.level == 2 || item.amount >= 50; // Tuyết Liên hoặc số lượng lớn
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. THỰC THI PHÂN GIẢI & CỘNG TÀI NGUYÊN (EXECUTION)
    // ─────────────────────────────────────────────────────────────────────────

    public void ApplyReward(DismantleReward reward)
    {
        if (reward == null || reward.IsEmpty) return;

        if (reward.linhThach > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddLinhThach(reward.linhThach);
        }
        if (reward.kiemY > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddKiemY(reward.kiemY);
        }
        if (reward.tinhThietOre > 0 && ArtifactService.Instance != null)
        {
            ArtifactService.Instance.AddOre(reward.tinhThietOre);
        }
        if (AlchemyService.Instance != null)
        {
            if (reward.medicineDust > 0)
            {
                AlchemyService.Instance.State.medicineDust += reward.medicineDust;
            }
            if (reward.herbs != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (reward.herbs[i] > 0) AlchemyService.Instance.AddHerb(i, reward.herbs[i]);
                }
            }
        }
    }

    /// <summary>
    /// Phân giải 1 thanh kiếm cụ thể
    /// </summary>
    public bool DismantleSword(MergeItem item)
    {
        if (item == null || MergeManager.Instance == null) return false;
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked()) return false;

        int level = item.level;
        int pieces = item.currentPieces;
        bool isEquipped = MergeManager.Instance.IsSwordEquipped(item);

        DismantleReward reward = CalculateSwordReward(level, pieces);
        if (!MergeManager.Instance.RemoveSword(item)) return false;

        ApplyReward(reward);
        OnDismantleExecuted?.Invoke();

        string swordName = GetSwordDisplayName(level);
        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Phân Giải [{swordName}]: Nhận {reward.GetFormattedSummary()}", Color.green);
        }
        Debug.Log($"[Dismantle] Đã phân giải {swordName} (Lv.{level}, {pieces} phôi) -> {reward.GetFormattedSummary()}");
        return true;
    }

    /// <summary>
    /// Phân giải Đan Dược
    /// </summary>
    public bool DismantlePills(int realm, PillRecipe recipe, PillQuality quality, int count)
    {
        if (AlchemyService.Instance == null || count <= 0) return false;
        if (FeatureUnlockService.Instance != null && (!FeatureUnlockService.Instance.IsDismantleUnlocked() || !FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy))) return false;

        int available = AlchemyService.Instance.GetPillCount(realm, recipe, quality);
        if (available < count) return false;

        // Trừ đan
        int index = realm * 8 + (int)recipe * 4 + (int)quality;
        if (index >= 0 && index < AlchemyService.Instance.State.pills.Length)
        {
            AlchemyService.Instance.State.pills[index] -= count;
        }

        DismantleReward reward = CalculatePillReward(realm, recipe, quality, count);
        ApplyReward(reward);
        OnDismantleExecuted?.Invoke();

        string pillName = $"{quality} {(recipe == PillRecipe.Breakthrough ? "Đột Phá Đan" : "Tu Vi Đan")}";
        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Phân Giải x{count} [{pillName}]: Nhận {reward.GetFormattedSummary()}", Color.green);
        }
        Debug.Log($"[Dismantle] Đã phân giải x{count} {pillName} -> {reward.GetFormattedSummary()}");
        return true;
    }

    /// <summary>
    /// Phân giải Quặng Boss
    /// </summary>
    public bool DismantleOre(int count)
    {
        if (ArtifactService.Instance == null || count <= 0) return false;
        if (FeatureUnlockService.Instance != null && (!FeatureUnlockService.Instance.IsDismantleUnlocked() || !FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Formation))) return false;
        if (!ArtifactService.Instance.SpendOre(count)) return false;

        DismantleReward reward = CalculateOreReward(count);
        ApplyReward(reward);
        OnDismantleExecuted?.Invoke();

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Phân Giải x{count} Quặng: Nhận {reward.GetFormattedSummary()}", Color.green);
        }
        return true;
    }

    /// <summary>
    /// Phân giải Thảo Dược
    /// </summary>
    public bool DismantleHerbs(int herbIndex, int count)
    {
        if (AlchemyService.Instance == null || count <= 0 || herbIndex < 0 || herbIndex >= 3) return false;
        if (FeatureUnlockService.Instance != null && (!FeatureUnlockService.Instance.IsDismantleUnlocked() || !FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy))) return false;
        if (AlchemyService.Instance.GetHerbCount(herbIndex) < count) return false;

        AlchemyService.Instance.State.herbs[herbIndex] -= count;
        DismantleReward reward = CalculateHerbReward(herbIndex, count);
        ApplyReward(reward);
        OnDismantleExecuted?.Invoke();

        string[] names = { "Linh Chi", "Cỏ Ngưng Khí", "Tuyết Liên" };
        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Phân Giải x{count} {names[herbIndex]}: Nhận {reward.GetFormattedSummary()}", Color.green);
        }
        return true;
    }

    /// <summary>
    /// Phân giải 1 vật phẩm đang nằm trong Hòm Tạm Chờ Lọc
    /// </summary>
    public bool DismantlePendingItem(PendingLootItem item)
    {
        if (item == null || StorageManager.Instance == null) return false;
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked()) return false;

        DismantleReward reward = new DismantleReward();
        switch (item.itemType)
        {
            case PendingLootType.Sword:
                reward = CalculateSwordReward(item.level, item.pieces);
                break;
            case PendingLootType.Pill:
                reward = CalculatePillReward(item.realm, (PillRecipe)item.recipe, (PillQuality)item.quality, item.amount);
                break;
            case PendingLootType.Ore:
                reward = CalculateOreReward(item.amount);
                break;
            case PendingLootType.Herb:
                reward = CalculateHerbReward(item.level, item.amount);
                break;
        }

        if (!StorageManager.Instance.TryRemovePendingItem(item)) return false;
        ApplyReward(reward);
        OnDismantleExecuted?.Invoke();

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Phân Giải Hòm Tạm [{item.displayName}]: Nhận {reward.GetFormattedSummary()}", Color.green);
        }
        return true;
    }

    /// <summary>
    /// Phân giải nhanh hàng loạt toàn bộ phế phẩm / vật phẩm cấp thấp (Kiếm Lv1 chưa trang bị & Phế đan)
    /// </summary>
    public DismantleReward BatchDismantleLowTier()
    {
        DismantleReward totalReward = new DismantleReward();
        if (batchInProgress || (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked()))
            return totalReward;

        batchInProgress = true;
        try
        {
            return ExecuteBatchDismantleLowTier(totalReward);
        }
        finally
        {
            batchInProgress = false;
        }
    }

    private DismantleReward ExecuteBatchDismantleLowTier(DismantleReward totalReward)
    {
        int swordCount = 0;
        int pillCount = 0;

        // 1. Quét kho kiếm: Tìm Kiếm Lv1 không trang bị
        if (MergeManager.Instance != null)
        {
            var swords = MergeManager.Instance.GetAllSwords(includeEquipped: false);
            bool inventoryChanged = false;
            foreach (var s in swords)
            {
                if (s != null && s.level == 1)
                {
                    int pieces = s.currentPieces;
                    if (!MergeManager.Instance.RemoveSword(s, false)) continue;
                    swordCount += Mathf.Max(1, pieces);
                    inventoryChanged = true;
                }
            }
            if (inventoryChanged) MergeManager.Instance.SortInventory();
        }

        // 2. Quét kho đan: Tìm Phế đan của mọi cảnh giới
        if (AlchemyService.Instance != null)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int rec = 0; rec < 2; rec++)
                {
                    int wasteIdx = r * 8 + rec * 4 + (int)PillQuality.Waste;
                    if (wasteIdx >= 0 && wasteIdx < AlchemyService.Instance.State.pills.Length)
                    {
                        int count = AlchemyService.Instance.State.pills[wasteIdx];
                        if (count > 0)
                        {
                            AlchemyService.Instance.State.pills[wasteIdx] = 0;
                            pillCount += count;
                        }
                    }
                }
            }
        }

        // 3. Quét Hòm Tạm: Phân giải các món Lv1 hoặc Phế đan trong hòm tạm
        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.RemovePendingItems(item =>
            {
                if (item == null) return false;
                if (item.itemType == PendingLootType.Sword && item.level == 1)
                {
                    swordCount += Mathf.Max(1, item.pieces);
                    return true;
                }
                else if (item.itemType == PendingLootType.Pill && item.quality == (int)PillQuality.Waste && item.amount > 0)
                {
                    pillCount += item.amount;
                    return true;
                }
                return false;
            });
        }

        // Every Lv1 sword and Waste pill has the same unit value. Calculate
        // once per type rather than allocating a reward + herb array per row.
        if (swordCount > 0) totalReward.Add(CalculateSwordReward(1, swordCount));
        if (pillCount > 0) totalReward.Add(CalculatePillReward(0, PillRecipe.Cultivation, PillQuality.Waste, pillCount));

        if (!totalReward.IsEmpty)
        {
            ApplyReward(totalReward);
            OnDismantleExecuted?.Invoke();

            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"Tách Phế Phẩm: Đã phân giải {swordCount} Kiếm Lv1 & {pillCount} Phế Đan -> {totalReward.GetFormattedSummary()}", Color.green);
            }
            Debug.Log($"[Dismantle] Đã tách nhanh hàng loạt -> {totalReward.GetFormattedSummary()}");
        }

        return totalReward;
    }

    public static string GetSwordDisplayName(int level)
    {
        switch (level)
        {
            case 1: return "Thiết Kiếm (Lv1)";
            case 2: return "Thanh Phong Kiếm (Lv2)";
            case 3: return "Huyền Thiết Kiếm (Lv3)";
            case 4: return "Xích Viêm Kiếm (Lv4)";
            case 5: return "Tử Điện Kiếm (Lv5)";
            case 6: return "Băng Phách Kiếm (Lv6)";
            case 7: return "Hư Vô Kiếm (Lv7)";
            case 8: return "Thiên Tinh Kiếm (Lv8)";
            default: return $"Phi Kiếm (Lv{level})";
        }
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý Hệ Thống Thành Tựu Tiên Đạo (Immortal Path Achievement System).
/// Độc lập, lưu trữ an toàn, theo dõi tiến trình toàn diện trên mọi nhánh gameplay.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public AchievementSaveState State { get; private set; } = new AchievementSaveState();
    public List<AchievementItem> allAchievements = new List<AchievementItem>();

    public event Action<AchievementItem> OnAchievementCompleted;
    public event Action<AchievementItem> OnAchievementClaimed;
    public event Action OnAchievementsUpdated;

    private HashSet<string> completedCache = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying) { Destroy(gameObject); return; }
        }
        Instance = this;

        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (allAchievements == null)
        {
            allAchievements = new List<AchievementItem>();
        }
        if (allAchievements.Count == 0)
        {
            InitializeAchievements();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void InitializeAchievements()
    {
        allAchievements.Clear();

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 1: TU LUYỆN & ĐỘ KIẾP (CULTIVATION)
        // ═══════════════════════════════════════════════════════════════════
        AddAchievement("cult_lk5", "🌱 Sơ Nhập Đạo Đồ", "Đạt Luyện Khí Kỳ Tầng 5", 
            AchievementCategory.Cultivation, AchievementRequirementType.Realm, 1, 500, 0, 0);

        AddAchievement("cult_tc", "⚡ Trúc Cơ Thành Tựu", "Đột phá thành công Trúc Cơ Kỳ", 
            AchievementCategory.Cultivation, AchievementRequirementType.Realm, 2, 2000, 2, 0);

        AddAchievement("cult_kd", "🔥 Kết Đan Đại Đạo", "Đột phá thành công Kim Đan Kỳ", 
            AchievementCategory.Cultivation, AchievementRequirementType.Realm, 3, 5000, 3, 0);

        AddAchievement("cult_na", "🌌 Nguyên Anh Xuất Khiếu", "Đột phá thành công Nguyên Anh Kỳ", 
            AchievementCategory.Cultivation, AchievementRequirementType.Realm, 4, 10000, 5, 0);

        AddAchievement("cult_ht", "👑 Hóa Thần Đỉnh Phong", "Đột phá thành công Hóa Thần Kỳ", 
            AchievementCategory.Cultivation, AchievementRequirementType.Realm, 5, 25000, 10, 0);

        AddAchievement("trib_1", "⚡ Nghịch Thiên Cải Mệnh", "Vượt qua 1 lần Lôi Kiếp Thiên Phạt", 
            AchievementCategory.Cultivation, AchievementRequirementType.TribulationSuccess, 1, 1500, 1, 0);

        AddAchievement("trib_3", "🛡️ Kim Thân Bất Diệt", "Vượt qua 3 lần Lôi Kiếp Thiên Phạt", 
            AchievementCategory.Cultivation, AchievementRequirementType.TribulationSuccess, 3, 4000, 3, 0);

        AddAchievement("trib_5", "🌪️ Vạn Kiếp Bất Hủ", "Vượt qua 5 lần Lôi Kiếp Thiên Phạt", 
            AchievementCategory.Cultivation, AchievementRequirementType.TribulationSuccess, 5, 10000, 5, 0);

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 2: CHIẾN ĐẤU & VƯỢT ẢI (COMBAT)
        // ═══════════════════════════════════════════════════════════════════
        AddAchievement("kill_50", "⚔️ Trảm Thảo Trừ Căn", "Tiêu diệt 50 quái vật tại bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.KillMonster, 50, 500, 0, 0);

        AddAchievement("kill_200", "⚔️ Bách Chiến Bất Bại", "Tiêu diệt 200 quái vật tại bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.KillMonster, 200, 1500, 0, 0);

        AddAchievement("kill_1000", "⚔️ Thiên Nhân Trảm", "Tiêu diệt 1,000 quái vật tại bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.KillMonster, 1000, 5000, 2, 0);

        AddAchievement("kill_5000", "⚔️ Vạn Ma Tận Diệt", "Tiêu diệt 5,000 quái vật tại bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.KillMonster, 5000, 15000, 5, 0);

        AddAchievement("boss_5", "💀 Trảm Yêu Vương I", "Đánh bại 5 Boss thủ ải bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.DefeatBoss, 5, 1000, 1, 0);

        AddAchievement("boss_20", "💀 Trảm Yêu Vương II", "Đánh bại 20 Boss thủ ải bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.DefeatBoss, 20, 3500, 2, 0);

        AddAchievement("boss_50", "💀 Tuyệt Thế Diệt Yêu", "Đánh bại 50 Boss thủ ải bí cảnh", 
            AchievementCategory.Combat, AchievementRequirementType.DefeatBoss, 50, 9000, 4, 0);

        AddAchievement("stage_10", "🚩 Vượt Ải Thập Trọng", "Vượt qua tầng chiến đấu 10", 
            AchievementCategory.Combat, AchievementRequirementType.Stage, 10, 1000, 0, 0);

        AddAchievement("stage_30", "🚩 Vượt Ải Tam Thập", "Vượt qua tầng chiến đấu 30", 
            AchievementCategory.Combat, AchievementRequirementType.Stage, 30, 3000, 2, 0);

        AddAchievement("stage_50", "🚩 Vượt Ải Ngũ Thập", "Vượt qua tầng chiến đấu 50", 
            AchievementCategory.Combat, AchievementRequirementType.Stage, 50, 7000, 3, 0);

        AddAchievement("stage_100", "🚩 Thông Thiên Triệt Địa", "Vượt qua tầng chiến đấu 100", 
            AchievementCategory.Combat, AchievementRequirementType.Stage, 100, 15000, 6, 0);

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 3: KIẾM ĐẠO & NGỰ KIẾM (SWORD)
        // ═══════════════════════════════════════════════════════════════════
        AddAchievement("sword_lv5", "🗡️ Thanh Phong Kiếm", "Ghép thành công phi kiếm Cấp 5", 
            AchievementCategory.Sword, AchievementRequirementType.MergeSwordLevel, 5, 800, 0, 0);

        AddAchievement("sword_lv10", "🗡️ Xích Diễm Thần Kiếm", "Ghép thành công phi kiếm Cấp 10", 
            AchievementCategory.Sword, AchievementRequirementType.MergeSwordLevel, 10, 2500, 1, 0);

        AddAchievement("sword_lv15", "🗡️ Huyền Băng Trảm Long", "Ghép thành công phi kiếm Cấp 15", 
            AchievementCategory.Sword, AchievementRequirementType.MergeSwordLevel, 15, 6000, 2, 0);

        AddAchievement("sword_lv20", "🗡️ Tru Tiên Cổ Kiếm", "Ghép thành công phi kiếm Cấp 20", 
            AchievementCategory.Sword, AchievementRequirementType.MergeSwordLevel, 20, 15000, 5, 0);

        AddAchievement("merge_50", "🔨 Lô Hỏa Thuần Thục", "Thực hiện ghép phi kiếm 50 lần", 
            AchievementCategory.Sword, AchievementRequirementType.TotalMerges, 50, 1000, 0, 0);

        AddAchievement("merge_200", "🔨 Bách Luyện Thành Cương", "Thực hiện ghép phi kiếm 200 lần", 
            AchievementCategory.Sword, AchievementRequirementType.TotalMerges, 200, 3500, 2, 0);

        AddAchievement("merge_500", "🔨 Thiên Chùy Vạn Đúc", "Thực hiện ghép phi kiếm 500 lần", 
            AchievementCategory.Sword, AchievementRequirementType.TotalMerges, 500, 10000, 4, 0);

        AddAchievement("equip_3", "✨ Tam Kiếm Tề Phi", "Trang bị đồng thời 3 phi kiếm trên trận", 
            AchievementCategory.Sword, AchievementRequirementType.EquippedSwords, 3, 1500, 0, 0);

        AddAchievement("equip_5", "✨ Ngũ Kiếm Quy Tâm", "Trang bị đồng thời 5 phi kiếm trên trận", 
            AchievementCategory.Sword, AchievementRequirementType.EquippedSwords, 5, 4000, 2, 0);

        AddAchievement("equip_8", "✨ Bát Quái Kiếm Trận", "Trang bị đồng thời 8 phi kiếm trên trận", 
            AchievementCategory.Sword, AchievementRequirementType.EquippedSwords, 8, 8000, 3, 0);

        AddAchievement("equip_10", "✨ Vạn Kiếm Quy Tông", "Mở khóa trọn vẹn 10 ô trang bị phi kiếm", 
            AchievementCategory.Sword, AchievementRequirementType.EquippedSwords, 10, 15000, 6, 0);

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 4: BÁCH NGHỆ (ARTS - THỂ TU, LUYỆN ĐAN, PHÁP KHÍ, TRẬN PHÁP)
        // ═══════════════════════════════════════════════════════════════════
        AddAchievement("body_10", "💪 Cương Cốt Đồng Bì", "Tổng cấp 6 chỉ số Luyện Thể đạt 10", 
            AchievementCategory.Arts, AchievementRequirementType.BodyStatsTotal, 10, 1200, 0, 0);

        AddAchievement("body_30", "💪 Kim Cương Bất Hoại", "Tổng cấp 6 chỉ số Luyện Thể đạt 30", 
            AchievementCategory.Arts, AchievementRequirementType.BodyStatsTotal, 30, 4000, 2, 0);

        AddAchievement("body_60", "💪 Thần Ma Cửu Biến", "Tổng cấp 6 chỉ số Luyện Thể đạt 60", 
            AchievementCategory.Arts, AchievementRequirementType.BodyStatsTotal, 60, 10000, 5, 0);

        AddAchievement("alch_10", "⚗️ Sơ Nhập Đan Đạo", "Luyện chế thành công 10 mẻ đan dược", 
            AchievementCategory.Arts, AchievementRequirementType.CraftPillsTotal, 10, 1000, 0, 0);

        AddAchievement("alch_50", "⚗️ Đan Đạo Tông Sư", "Luyện chế thành công 50 mẻ đan dược", 
            AchievementCategory.Arts, AchievementRequirementType.CraftPillsTotal, 50, 4500, 2, 0);

        AddAchievement("alch_supreme", "🌟 Đan Hỏa Nổ Hũ", "Nấu thành công 1 viên Đan Cực Phẩm", 
            AchievementCategory.Arts, AchievementRequirementType.CraftSupremePill, 1, 3000, 2, 0);

        AddAchievement("art_up5", "🛡️ Linh Khí Hộ Thân", "Cường hóa Pháp Khí tổng cộng 5 lần", 
            AchievementCategory.Arts, AchievementRequirementType.ArtifactUpgrades, 5, 1500, 0, 20);

        AddAchievement("art_up20", "🛡️ Tuyệt Thế Pháp Bảo", "Cường hóa Pháp Khí tổng cộng 20 lần", 
            AchievementCategory.Arts, AchievementRequirementType.ArtifactUpgrades, 20, 5000, 2, 50);

        AddAchievement("form_3", "🔮 Tam Giới Trận Đồ", "Học và mở khóa 3 Đại Trận Pháp", 
            AchievementCategory.Arts, AchievementRequirementType.FormationsUnlocked, 3, 2500, 0, 30);

        AddAchievement("form_6", "🔮 Lục Đại Trận Pháp", "Mở khóa trọn bộ 6 Đại Trận Pháp hộ thân", 
            AchievementCategory.Arts, AchievementRequirementType.FormationsUnlocked, 6, 8000, 3, 60);

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 5: KỲ DUYÊN & KINH TẾ (DESTINY)
        // ═══════════════════════════════════════════════════════════════════
        AddAchievement("stone_10k", "💰 Phú Giáp Nhất Phương", "Tích lũy đạt 10,000 Linh Thạch", 
            AchievementCategory.Destiny, AchievementRequirementType.LifetimeLinhThach, 10000, 0, 1, 0);

        AddAchievement("stone_100k", "💰 Phú Khả Địch Quốc", "Tích lũy đạt 100,000 Linh Thạch", 
            AchievementCategory.Destiny, AchievementRequirementType.LifetimeLinhThach, 100000, 0, 3, 0);

        AddAchievement("stone_500k", "💰 Tiên Gia Cự Phách", "Tích lũy đạt 500,000 Linh Thạch", 
            AchievementCategory.Destiny, AchievementRequirementType.LifetimeLinhThach, 500000, 0, 6, 0);

        AddAchievement("enc_10", "🎁 Tiên Nhân Chỉ Lộ", "Thu thập 10 Kỳ Ngộ ngẫu nhiên trên màn hình", 
            AchievementCategory.Destiny, AchievementRequirementType.EncountersCollected, 10, 1500, 1, 0);

        AddAchievement("enc_30", "🎁 Phúc Trạch Vô Biên", "Thu thập 30 Kỳ Ngộ ngẫu nhiên trên màn hình", 
            AchievementCategory.Destiny, AchievementRequirementType.EncountersCollected, 30, 4500, 3, 0);

        AddAchievement("off_3", "🧘 Tĩnh Tâm Bế Quan", "Thu hoạch phần thưởng Bế Quan 3 lần", 
            AchievementCategory.Destiny, AchievementRequirementType.OfflineClaims, 3, 1000, 1, 0);

        AddAchievement("off_10", "🧘 Khổ Tu Đắc Đạo", "Thu hoạch phần thưởng Bế Quan 10 lần", 
            AchievementCategory.Destiny, AchievementRequirementType.OfflineClaims, 10, 3500, 2, 0);
    }

    private void AddAchievement(string id, string title, string desc, AchievementCategory cat,
        AchievementRequirementType reqType, int target, float linhThach, int coDuyen, int ore)
    {
        allAchievements.Add(new AchievementItem
        {
            id = id,
            title = title,
            description = desc,
            category = cat,
            requirementType = reqType,
            targetAmount = target,
            rewardLinhThach = linhThach,
            rewardCoDuyen = coDuyen,
            rewardOre = ore
        });
    }

    // ── TIẾN TRÌNH & ĐIỀU KIỆN ───────────────────────────────────────────────
    public int GetCurrentProgress(AchievementItem item)
    {
        if (item == null) return 0;

        switch (item.requirementType)
        {
            case AchievementRequirementType.Realm:
                if (item.id == "cult_lk5")
                {
                    if (CultivationManager.Instance == null) return 0;
                    return (int)CultivationManager.Instance.CurrentRealm > 0 || CultivationManager.Instance.CurrentSubLevel >= 5 ? 1 : 0;
                }
                int curRealm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0;
                return curRealm >= (item.targetAmount - 1) ? item.targetAmount : curRealm;

            case AchievementRequirementType.TribulationSuccess:
                return State.tribulationSuccessCount;

            case AchievementRequirementType.KillMonster:
                int currentStageKills = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0;
                return Mathf.Max(State.totalEnemiesKilled, currentStageKills);

            case AchievementRequirementType.DefeatBoss:
                int stageBosses = StageManager.Instance != null ? Mathf.Max(0, StageManager.Instance.currentStage - 1) : 0;
                return Mathf.Max(State.totalBossesKilled, stageBosses);

            case AchievementRequirementType.Stage:
                return StageManager.Instance != null ? StageManager.Instance.currentStage : 1;

            case AchievementRequirementType.MergeSwordLevel:
                int highestSword = MergeManager.Instance != null ? GetHighestSwordLevelInInventory() : 1;
                return Mathf.Max(State.maxSwordLevelEver, highestSword);

            case AchievementRequirementType.TotalMerges:
                return State.totalSwordMerges;

            case AchievementRequirementType.EquippedSwords:
                return MergeManager.Instance != null ? MergeManager.Instance.EquippedCount : 1;

            case AchievementRequirementType.BodyStatsTotal:
                return PlayerStatsService.Instance != null ? PlayerStatsService.Instance.TotalBodyLevel : 0;

            case AchievementRequirementType.CraftPillsTotal:
                return State.totalPillsBrewed;

            case AchievementRequirementType.CraftSupremePill:
                return State.supremePillsBrewed;

            case AchievementRequirementType.ArtifactUpgrades:
                if (ArtifactService.Instance == null || ArtifactService.Instance.State == null || ArtifactService.Instance.State.levels == null) return 0;
                int totalArtLv = 0;
                foreach (int lv in ArtifactService.Instance.State.levels) totalArtLv += lv;
                return totalArtLv;

            case AchievementRequirementType.FormationsUnlocked:
                if (FormationService.Instance == null || FormationService.Instance.State == null || FormationService.Instance.State.levels == null) return 0;
                int unlocked = 0;
                foreach (int lv in FormationService.Instance.State.levels) if (lv > 0) unlocked++;
                return unlocked;

            case AchievementRequirementType.LifetimeLinhThach:
                float curStones = EconomyManager.Instance != null ? EconomyManager.Instance.currentLinhThach : 0f;
                return Mathf.FloorToInt(Mathf.Max(State.lifetimeLinhThachEarned, curStones));

            case AchievementRequirementType.EncountersCollected:
                return State.encountersCollected;

            case AchievementRequirementType.OfflineClaims:
                return State.offlineClaimsCount;

            default:
                return 0;
        }
    }

    private int GetHighestSwordLevelInInventory()
    {
        if (MergeManager.Instance == null || MergeManager.Instance.inventoryPages == null) return 1;
        int maxLvl = 1;
        foreach (Transform page in MergeManager.Instance.inventoryPages)
        {
            if (page == null) continue;
            foreach (Transform slot in page)
            {
                if (slot == null) continue;
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null && item.level > maxLvl) maxLvl = item.level;
            }
        }
        return maxLvl;
    }

    public bool IsCompleted(AchievementItem item)
    {
        if (item == null) return false;
        return GetCurrentProgress(item) >= item.targetAmount;
    }

    public List<AchievementItem> GetAchievementsByCategory(AchievementCategory category)
    {
        EnsureInitialized();
        if (category == AchievementCategory.All) return allAchievements;
        List<AchievementItem> list = new List<AchievementItem>();
        foreach (var a in allAchievements)
        {
            if (a.category == category) list.Add(a);
        }
        return list;
    }

    // ── NHẬN THƯỞNG ──────────────────────────────────────────────────────────
    public bool ClaimReward(string achievementId)
    {
        EnsureInitialized();
        AchievementItem item = allAchievements.Find(a => a.id == achievementId);
        if (item == null || item.isClaimed || !IsCompleted(item)) return false;

        item.isClaimed = true;

        // 1. Thưởng Linh Thạch
        if (item.rewardLinhThach > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddLinhThach(item.rewardLinhThach);
        }

        // 2. Thưởng Cơ Duyên
        if (item.rewardCoDuyen > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddCoDuyen(item.rewardCoDuyen);
        }

        // 3. Thưởng Quặng Phôi
        if (item.rewardOre > 0 && ArtifactService.Instance != null)
        {
            ArtifactService.Instance.AddOre(item.rewardOre);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"🏆 Hoàn Thành Thành Tựu [{item.title}]: {item.GetRewardSummary()}!", Color.green);
        }

        // Tự động lưu ngầm
        if (DataManager.Instance != null && DataManager.Instance.isSessionActive)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }

        OnAchievementClaimed?.Invoke(item);
        OnAchievementsUpdated?.Invoke();
        return true;
    }

    public int ClaimAllCompleted(AchievementCategory category = AchievementCategory.All)
    {
        EnsureInitialized();
        int claimedCount = 0;
        List<AchievementItem> list = GetAchievementsByCategory(category);
        foreach (var item in list)
        {
            if (!item.isClaimed && IsCompleted(item))
            {
                if (ClaimReward(item.id)) claimedCount++;
            }
        }
        return claimedCount;
    }

    public int GetReadyToClaimCount(AchievementCategory category = AchievementCategory.All)
    {
        EnsureInitialized();
        int count = 0;
        List<AchievementItem> list = GetAchievementsByCategory(category);
        foreach (var item in list)
        {
            if (!item.isClaimed && IsCompleted(item)) count++;
        }
        return count;
    }

    public int GetTotalClaimedCount()
    {
        EnsureInitialized();
        int count = 0;
        foreach (var item in allAchievements) if (item.isClaimed) count++;
        return count;
    }

    // ── STAT TRACKING HOOKS ──────────────────────────────────────────────────
    public void OnEnemyKilled(bool isBoss)
    {
        State.totalEnemiesKilled++;
        if (isBoss) State.totalBossesKilled++;
        CheckNewCompletions();
    }

    public void OnSwordMerged(int newLevel)
    {
        State.totalSwordMerges++;
        if (newLevel > State.maxSwordLevelEver) State.maxSwordLevelEver = newLevel;
        CheckNewCompletions();
    }

    public void OnPillBrewed(PillQuality quality)
    {
        State.totalPillsBrewed++;
        if (quality == PillQuality.Supreme) State.supremePillsBrewed++;
        CheckNewCompletions();
    }

    public void OnEncounterCollected()
    {
        State.encountersCollected++;
        CheckNewCompletions();
    }

    public void OnOfflineClaimed()
    {
        State.offlineClaimsCount++;
        CheckNewCompletions();
    }

    public void OnTribulationSuccess()
    {
        State.tribulationSuccessCount++;
        CheckNewCompletions();
    }

    public void OnLinhThachEarned(float amount)
    {
        if (amount > 0)
        {
            State.lifetimeLinhThachEarned += amount;
            CheckNewCompletions();
        }
    }

    private void CheckNewCompletions()
    {
        EnsureInitialized();
        foreach (var item in allAchievements)
        {
            if (!item.isClaimed && !completedCache.Contains(item.id) && IsCompleted(item))
            {
                completedCache.Add(item.id);
                OnAchievementCompleted?.Invoke(item);
                OnAchievementsUpdated?.Invoke();
            }
        }
    }

    // ── SAVE & LOAD SLOTS ────────────────────────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot == null) return;
        EnsureInitialized();
        List<string> claimed = new List<string>();
        foreach (var a in allAchievements)
        {
            if (a.isClaimed) claimed.Add(a.id);
        }
        State.claimedIds = claimed.ToArray();
        slot.achievements = State;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        EnsureInitialized();
        State = slot?.achievements ?? new AchievementSaveState();
        if (State.claimedIds == null) State.claimedIds = new string[0];

        HashSet<string> set = new HashSet<string>(State.claimedIds);
        completedCache.Clear();

        foreach (var a in allAchievements)
        {
            a.isClaimed = set.Contains(a.id);
            if (a.isClaimed) completedCache.Add(a.id);
        }

        OnAchievementsUpdated?.Invoke();
    }
}


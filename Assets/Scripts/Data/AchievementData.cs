using System;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementCategory
{
    All = 0,         // Tất Cả
    Cultivation = 1, // Tu Luyện & Độ Kiếp
    Combat = 2,      // Chiến Đấu & Vượt Ải
    Sword = 3,       // Kiếm Đạo & Ngự Kiếm
    Arts = 4,        // Bách Nghệ (Luyện Đan, Thể Tu, Pháp Bảo, Trận Pháp)
    Destiny = 5      // Kỳ Duyên & Kinh Tế
}

public enum AchievementRequirementType
{
    Realm,                // Đạt đại cảnh giới
    TribulationSuccess,   // Số lần độ kiếp thành công
    KillMonster,          // Tiêu diệt quái thường
    DefeatBoss,           // Tiêu diệt Boss vượt ải
    Stage,                // Đạt mốc tầng chiến đấu
    MergeSwordLevel,      // Ghép được kiếm đạt cấp chỉ định
    TotalMerges,          // Tổng số lần ghép kiếm
    MonsterSwordDrops,    // Số thanh kiếm nhặt được từ quái vật rơi (mở khóa Phân Giải)
    EquippedSwords,       // Số phi kiếm trang bị cùng lúc
    BodyStatsTotal,       // Tổng cấp 6 chỉ số Luyện Thể
    CraftPillsTotal,      // Tổng số lần luyện đan thành công
    CraftSupremePill,     // Số lần luyện ra Đan Cực Phẩm (Supreme)
    ArtifactUpgrades,     // Tổng cấp nâng cấp 4 Pháp Khí
    FormationsUnlocked,   // Số Đại Trận Pháp mở khóa
    LifetimeLinhThach,    // Linh Thạch tích lũy/kiếm được
    EncountersCollected,  // Số lần nhặt Kỳ Ngộ ngẫu nhiên
    OfflineClaims         // Số lần nhận thưởng Bế Quan Tu Luyện
}

[Serializable]
public class AchievementItem
{
    public string id;
    public string title;
    public string description;
    public AchievementCategory category;
    public AchievementRequirementType requirementType;
    public int targetAmount;

    [Header("Phần Thưởng")]
    public float rewardLinhThach;
    public int rewardCoDuyen;
    public int rewardOre;

    [NonSerialized]
    public bool isClaimed = false;

    public string GetRewardSummary()
    {
        List<string> parts = new List<string>();
        if (rewardLinhThach > 0) parts.Add($"+{rewardLinhThach:F0} LT");
        if (rewardCoDuyen > 0) parts.Add($"+{rewardCoDuyen} Cơ Duyên");
        if (rewardOre > 0) parts.Add($"+{rewardOre} Quặng");
        return string.Join(" | ", parts);
    }
}

[Serializable]
public class AchievementSaveState
{
    public string[] claimedIds = new string[0];
    public int totalEnemiesKilled = 0;
    public int totalBossesKilled = 0;
    public int totalSwordMerges = 0;
    public int maxSwordLevelEver = 1;
    public int totalPillsBrewed = 0;
    public int supremePillsBrewed = 0;
    public int encountersCollected = 0;
    public int offlineClaimsCount = 0;
    public int tribulationSuccessCount = 0;
    public float lifetimeLinhThachEarned = 0f;
}


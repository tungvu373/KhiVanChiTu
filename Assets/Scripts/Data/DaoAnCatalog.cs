using System;
using UnityEngine;

public enum DaoAnEffect
{
    MonsterLinhThach,       // 0 - Tụ Linh
    OfflineLinhThach,       // 1 - Bế Quan
    SwordDamage,            // 2 - Kiếm Tâm
    SwordDropRate,          // 3 - Kiếm Duyên
    MonsterTuVi,            // 4 - Hội Linh
    OfflineTuVi,            // 5 - Tĩnh Tu
    HerbDropRate,           // 6 - Dược Duyên
    OreDropRate,            // 7 - Khoáng Duyên
    TribulationProtection,  // 8 - Nghịch Mệnh
    FormationCooldown,      // 9 - Lôi Uy
    FormationCostReduction, // 10 - Trận Tài
    AlchemyCostReduction,   // 11 - Tiết Dược
    BossLinhThach,          // 12 - Chiến Lộc
    ArtifactOreReduction,   // 13 - Tinh Quặng
    ArtifactCostReduction,  // 14 - Bảo Tài
    BodyCostReduction,      // 15 - Thể Tài
    BossOreDrop,            // 16 - Báu Khoáng
    FormationDuration,      // 17 - Dư Trận
    SwordCritChance,        // 18 - Vạn Kiếm
    BossStoneJackpot,
    SwordTierWeight,
    BossHerbBonus,
    AlchemyBrewSpeed,
    BossDamageReduction,
    RegularMonsterStone,
    SwordFlightSpeed,
    SwordBossDamage,
    BossTuVi,
    PillTuVi,
    AlchemyDustCostReduction,
    AlchemyWasteDust,
    SpiritCooldownReduction,
    SwordRainCooldownReduction,
    ArtifactArmorStoneReduction,
    ArtifactAccessoryOreReduction,
    BodyGuardCostReduction,
    BodyForceCostReduction
}

[Serializable]
public class DaoAnDefinition
{
    public string id;
    public string displayName;
    public string description;
    public string prerequisiteId;
    public int prerequisiteLevel = 1;
    public DaoAnEffect effect;
    public float[] costs = Array.Empty<float>();
    // Each entry is the bonus purchased at that level. 0.05 means +5%.
    public float[] bonuses = Array.Empty<float>();
    public RealmId[] minRealms = Array.Empty<RealmId>();
    public int[] minSubLevels = Array.Empty<int>();

    // Phân loại danh mục & tầng hiển thị UI chuẩn Figma
    public string branch = "wealth";
    public int tier = 1; // 1 to 6
    public string tierName = "Tầng 1: Luyện Khí Sơ Kỳ";
    public string icon = "☯️";
    public string unit = "%";

    public int MaxLevel => costs == null || bonuses == null ? 0 : Math.Min(costs.Length, bonuses.Length);

    public float GetTotalBonus(int level)
    {
        float total = 0f;
        for (int i = 0; i < Math.Min(level, MaxLevel); i++)
        {
            float bonus = bonuses[i];
            if (!float.IsNaN(bonus) && !float.IsInfinity(bonus)) total += Mathf.Max(0f, bonus);
        }
        return total;
    }
}

[CreateAssetMenu(fileName = "DaoAnCatalog", menuName = "Khi Van Chi Tu/Vận Khí Catalog")]
public class DaoAnCatalog : ScriptableObject
{
    public DaoAnDefinition[] nodes = Array.Empty<DaoAnDefinition>();

    public DaoAnDefinition Find(string id)
    {
        if (string.IsNullOrEmpty(id) || nodes == null) return null;
        foreach (var node in nodes)
            if (node != null && node.id == id) return node;
        return null;
    }
}

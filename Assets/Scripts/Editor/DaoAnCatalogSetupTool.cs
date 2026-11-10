#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DaoAnCatalogSetupTool
{
    private const string AssetPath = "Assets/Resources/Progression/DaoAnCatalog.asset";

    [MenuItem("TuTien/Progression/Create or Update Full Vận Khí Catalog")]
    public static void CreateOrUpdateCatalog()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Progression"))
            AssetDatabase.CreateFolder("Assets/Resources", "Progression");

        var catalog = AssetDatabase.LoadAssetAtPath<DaoAnCatalog>(AssetPath);
        bool isNew = (catalog == null);
        if (isNew)
        {
            catalog = ScriptableObject.CreateInstance<DaoAnCatalog>();
        }

        var existing = catalog.nodes != null && catalog.nodes.Length > 0 ? catalog.nodes : CreateDefaults();
        var ids = new System.Collections.Generic.HashSet<string>(existing.Where(node => node != null).Select(node => node.id));
        if (!isNew) Undo.RecordObject(catalog, "Extend Đạo Vận catalog");
        catalog.nodes = existing.Concat(CreateExpansion().Where(node => ids.Add(node.id))).ToArray();
        if (isNew) AssetDatabase.CreateAsset(catalog, AssetPath);
        else EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssetIfDirty(catalog);
        Debug.Log($"[DaoAn] Updated {catalog.nodes.Length} nodes at {AssetPath}");
    }

    private static DaoAnDefinition[] CreateExpansion()
    {
        const RealmId lk = RealmId.LuyenKhi;
        const RealmId tc = RealmId.TrucCo;
        const RealmId kd = RealmId.KimDan;
        const RealmId na = RealmId.NguyenAnh;
        const RealmId ht = RealmId.HoaThan;

        return new[]
        {
            Branch("field_stone", "Tán Lộc Tụ Kim", "Tăng Linh Thạch từ quái thường.",
                "wealth", 2, "wealth", 2, DaoAnEffect.RegularMonsterStone, "% Linh Thạch", .03f,
                (lk, 4, 5_000f), (tc, 1, 20_000_000f), (kd, 1, 170_000_000f)),
            Branch("retreat_stone", "Kim Khố Bế Quan", "Tích thêm Linh Thạch trong thời gian bế quan.",
                "wealth", 3, "repose", 2, DaoAnEffect.OfflineLinhThach, "% Linh Thạch offline", .03f,
                (tc, 4, 45_000_000f), (kd, 4, 360_000_000f), (na, 4, 2_900_000_000f)),

            Branch("sword_flight", "Ngự Phong Kiếm Bộ", "Phi kiếm lao tới mục tiêu nhanh hơn.",
                "sword", 3, "sword_luck", 2, DaoAnEffect.SwordFlightSpeed, "% Tốc độ lao kiếm", .02f,
                (tc, 1, 22_000_000f), (kd, 1, 180_000_000f), (na, 1, 1_450_000_000f)),
            Branch("sword_boss_hunt", "Trảm Yêu Kiếm Quyết", "Tăng sát thương phi kiếm khi đánh Boss.",
                "sword", 4, "sword", 2, DaoAnEffect.SwordBossDamage, "% Sát thương kiếm lên Boss", .03f,
                (kd, 4, 420_000_000f), (na, 4, 3_350_000_000f), (ht, 4, 26_800_000_000f)),

            Branch("boss_tuvi", "Hóa Yêu Quy Nguyên", "Tăng Tu Vi nhận khi đánh bại Boss.",
                "cultivation", 3, "monster_cultivation", 1, DaoAnEffect.BossTuVi, "% Tu Vi từ Boss", .03f,
                (tc, 1, 24_000_000f), (kd, 1, 190_000_000f), (na, 1, 1_500_000_000f)),
            Branch("pill_tuvi", "Đan Tức Nhập Mạch", "Tăng Tu Vi nhận khi dùng Tu Vi Đan.",
                "cultivation", 2, "monster_cultivation", 2, DaoAnEffect.PillTuVi, "% Tu Vi từ đan", .03f,
                (lk, 8, 700_000f), (tc, 4, 48_000_000f), (kd, 4, 380_000_000f)),

            Branch("alchemy_dust_cost", "Tụ Hỏa Tiết Trần", "Giảm Bụi Dược dùng để hoàn tất mẻ đan tức thì.",
                "alchemy", 3, "herb_luck", 1, DaoAnEffect.AlchemyDustCostReduction, "Bụi Dược tiết kiệm", 1f,
                (tc, 1, 26_000_000f), (kd, 1, 210_000_000f), (na, 1, 1_700_000_000f)),
            Branch("alchemy_waste_dust", "Hồi Trần Bí Pháp", "Thu thêm Bụi Dược khi mẻ luyện ra Phế Đan.",
                "alchemy", 4, "alchemy_dust_cost", 1, DaoAnEffect.AlchemyWasteDust, "Bụi Dược thêm", 1f,
                (kd, 4, 450_000_000f), (na, 4, 3_600_000_000f)),

            Branch("spirit_recovery", "Bát Quái Hồi Linh", "Rút ngắn hồi chiêu kích hoạt Tụ Linh Trận.",
                "formation", 3, "formation_cd", 1, DaoAnEffect.SpiritCooldownReduction, "% Giảm hồi chiêu", .03f,
                (tc, 4, 52_000_000f), (kd, 4, 420_000_000f), (na, 4, 3_400_000_000f)),
            Branch("sword_rain_recovery", "Kiếm Vũ Liên Hoàn", "Rút ngắn chu kỳ mưa kiếm, tối thiểu 4 giây.",
                "formation", 4, "formation_cd", 1, DaoAnEffect.SwordRainCooldownReduction, "% Giảm chu kỳ", .03f,
                (kd, 1, 220_000_000f), (na, 1, 1_750_000_000f), (ht, 1, 14_000_000_000f)),

            Branch("artifact_armor_stone", "Bảo Y Tiết Tài", "Giảm Linh Thạch cường hóa Đạo Bào và Mũ.",
                "body_artifact", 5, "artifact_stone_efficiency", 1, DaoAnEffect.ArtifactArmorStoneReduction, "% Giảm Linh Thạch", .025f,
                (na, 4, 3_300_000_000f), (na, 7, 6_700_000_000f), (ht, 4, 27_000_000_000f)),
            Branch("artifact_accessory_ore", "Linh Phôi Tinh Luyện", "Giảm Quặng cường hóa Giày và Hộ Phù.",
                "body_artifact", 5, "artifact_ore_efficiency", 1, DaoAnEffect.ArtifactAccessoryOreReduction, "% Giảm Quặng", .03f,
                (na, 7, 6_800_000_000f), (ht, 1, 13_600_000_000f), (ht, 4, 27_200_000_000f)),

            Branch("body_guard_cost", "Cương Thân Điều Tức", "Giảm Linh Thạch cho các thiên phú nhánh Cương Thân.",
                "body", 6, "body_stone_efficiency", 1, DaoAnEffect.BodyGuardCostReduction, "% Giảm Linh Thạch", .03f,
                (ht, 1, 13_000_000_000f), (ht, 4, 26_000_000_000f), (ht, 7, 52_000_000_000f)),
            Branch("body_force_cost", "Thần Lực Luyện Cốt", "Giảm Linh Thạch cho các thiên phú nhánh Thần Lực.",
                "body", 6, "body_stone_efficiency", 1, DaoAnEffect.BodyForceCostReduction, "% Giảm Linh Thạch", .03f,
                (ht, 1, 14_000_000_000f), (ht, 4, 28_000_000_000f), (ht, 7, 56_000_000_000f))
        };
    }

    private static DaoAnDefinition Branch(string id, string name, string description, string branch,
        int tier, string prerequisite, int prerequisiteLevel, DaoAnEffect effect, string unit, float bonus,
        params (RealmId realm, int subLevel, float cost)[] ranks)
    {
        return new DaoAnDefinition
        {
            id = id, displayName = name, description = description, branch = branch,
            tier = tier, tierName = $"Tầng {tier}: {CultivationDisplay.RealmName(ranks[0].realm)}",
            prerequisiteId = prerequisite, prerequisiteLevel = prerequisiteLevel, effect = effect, unit = unit,
            minRealms = ranks.Select(rank => rank.realm).ToArray(),
            minSubLevels = ranks.Select(rank => rank.subLevel).ToArray(),
            costs = ranks.Select(rank => rank.cost).ToArray(),
            bonuses = ranks.Select(_ => bonus).ToArray()
        };
    }

    private static DaoAnDefinition[] CreateDefaults()
    {
        return new[]
        {
            // TẦNG 1: LUYỆN KHÍ SƠ KỲ (LK 1 - 3)
            new DaoAnDefinition
            {
                id = "wealth", displayName = "Tụ Linh Đạo Ấn", description = "Tăng Linh Thạch rơi từ quái vật và Boss",
                branch = "wealth", tier = 1, tierName = "Tầng 1: Luyện Khí Sơ Kỳ (Khí Hải Sơ Định)",
                icon = "🌀", unit = "% Linh Thạch",
                effect = DaoAnEffect.MonsterLinhThach,
                costs = new[] { 500f, 1500f, 4500f, 12000f, 30000f, 80000f },
                bonuses = new[] { 0.05f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.LuyenKhi, RealmId.TrucCo, RealmId.HoaThan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "sword_luck", displayName = "Kiếm Duyên Đạo Ấn", description = "Tăng xác suất quái rơi Phi Kiếm (Cần 500 kiếm mở Phân Giải)",
                branch = "sword", tier = 1, tierName = "Tầng 1: Luyện Khí Sơ Kỳ (Khí Hải Sơ Định)",
                icon = "🗡️", unit = "% Rơi Kiếm",
                prerequisiteId = "wealth", prerequisiteLevel = 1,
                effect = DaoAnEffect.SwordDropRate,
                costs = new[] { 800f, 2400f, 7000f, 20000f, 55000f, 150000f },
                bonuses = new[] { 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 2, 7, 1, 1, 1, 1 }
            },

            // TẦNG 2: LUYỆN KHÍ HẬU KỲ (LK 4 - 7)
            new DaoAnDefinition
            {
                id = "kim_thiem", displayName = "Kim Thiềm Đạo Ấn", description = "Có cơ hội nhân đôi Linh Thạch rơi từ Boss",
                branch = "wealth", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "🐸", unit = "% Nổ Hũ LT",
                prerequisiteId = "wealth", prerequisiteLevel = 2,
                effect = DaoAnEffect.BossStoneJackpot,
                costs = new[] { 1500f, 4500f, 12000f, 35000f, 90000f, 250000f },
                bonuses = new[] { 0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.15f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan },
                minSubLevels = new[] { 4, 7, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "monster_cultivation", displayName = "Hội Linh Đạo Ấn", description = "Gia tăng Tu Vi nhận được khi tiêu diệt yêu ma",
                branch = "cultivation", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "💫", unit = "% Tu Vi Quái",
                prerequisiteId = "wealth", prerequisiteLevel = 1,
                effect = DaoAnEffect.MonsterTuVi,
                costs = new[] { 1200f, 3600f, 10000f, 28000f, 75000f, 200000f },
                bonuses = new[] { 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.HoaThan, RealmId.HoaThan },
                minSubLevels = new[] { 4, 7, 4, 1, 1, 7 }
            },
            new DaoAnDefinition
            {
                id = "sword", displayName = "Vô Song Kiếm Tâm", description = "Cường hóa toàn bộ Phi Kiếm xuất trận, tăng sát thương cơ bản",
                branch = "sword", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "⚡", unit = "% Sát Thương",
                prerequisiteId = "wealth", prerequisiteLevel = 2,
                effect = DaoAnEffect.SwordDamage,
                costs = new[] { 2000f, 6000f, 18000f, 50000f, 130000f, 350000f },
                bonuses = new[] { 0.03f, 0.03f, 0.03f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 7, 4, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "repose", displayName = "Bế Quan Ngộ Đạo", description = "Gia tăng hiệu suất nhận Linh Thạch khi treo máy offline",
                branch = "wealth", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "🧘", unit = "% LT Treo Máy",
                prerequisiteId = "wealth", prerequisiteLevel = 1,
                effect = DaoAnEffect.OfflineLinhThach,
                costs = new[] { 1000f, 3000f, 9000f, 25000f, 70000f, 200000f },
                bonuses = new[] { 0.05f, 0.05f, 0.05f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 7, 1, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "herb_luck", displayName = "Dược Linh Dẫn Dụ", description = "Dẫn dụ thảo mộc, tăng xác suất rơi Dược Liệu Quý khi đánh quái",
                branch = "alchemy", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "🌿", unit = "% Rơi Thảo Dược",
                effect = DaoAnEffect.HerbDropRate,
                costs = new[] { 1600f, 4800f, 14000f, 40000f, 110000f, 300000f },
                bonuses = new[] { 0.04f, 0.04f, 0.04f, 0.04f, 0.04f, 0.04f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.HopThe },
                minSubLevels = new[] { 8, 1, 1, 1, 1, 1 }
            },

            // TẦNG 3: TRÚC CƠ KỲ (TC 1 - 3)
            new DaoAnDefinition
            {
                id = "tribulation_guard", displayName = "Nghịch Mệnh Hộ Thân", description = "Khí vận nghịch thiên, giảm tổn hao Tu Vi khi lôi kiếp thất bại",
                branch = "cultivation", tier = 3, tierName = "Tầng 3: Trúc Cơ Kỳ (Đạo Cơ Vững Chắc)",
                icon = "🛡️", unit = "% Giảm Mất Tu Vi",
                prerequisiteId = "monster_cultivation", prerequisiteLevel = 1,
                effect = DaoAnEffect.TribulationProtection,
                costs = new[] { 2500f, 7500f, 22000f, 60000f, 160000f, 450000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 9, 1, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "formation_cd", displayName = "Cửu Thiên Lôi Uy", description = "Rút ngắn khoảng cách giữa các lần Lôi Trận giáng sét",
                branch = "formation", tier = 3, tierName = "Tầng 3: Trúc Cơ Kỳ (Đạo Cơ Vững Chắc)",
                icon = "🌩️", unit = "% Giảm Hồi Chiêu",
                prerequisiteId = "sword", prerequisiteLevel = 1,
                effect = DaoAnEffect.FormationCooldown,
                costs = new[] { 3000f, 9000f, 26000f, 75000f, 200000f, 550000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.TrucCo, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "ore_luck", displayName = "Linh Khoáng Duyên", description = "Khai mở linh nhãn địa mạch, tăng xác suất quái rơi Quặng Rèn",
                branch = "body_artifact", tier = 3, tierName = "Tầng 3: Trúc Cơ Kỳ (Đạo Cơ Vững Chắc)",
                icon = "💎", unit = "% Rơi Quặng",
                effect = DaoAnEffect.OreDropRate,
                costs = new[] { 2800f, 8400f, 24000f, 70000f, 190000f, 500000f },
                bonuses = new[] { 0.04f, 0.04f, 0.04f, 0.04f, 0.04f, 0.04f },
                minRealms = new[] { RealmId.TrucCo, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.HopThe },
                minSubLevels = new[] { 1, 7, 1, 1, 1, 1 }
            },

            // TẦNG 4: KIM ĐAN KỲ (KD 1 - 4)
            new DaoAnDefinition
            {
                id = "formation_stone_efficiency", displayName = "Trận Tài Tinh Thông", description = "Lĩnh ngộ trận văn huyền ảo, giảm tiêu hao Linh Thạch nâng cấp Pháp Trận",
                branch = "formation", tier = 4, tierName = "Tầng 4: Kim Đan Kỳ (Ngưng Kết Kim Đan)",
                icon = "📜", unit = "% Giảm Giá Trận",
                prerequisiteId = "formation_cd", prerequisiteLevel = 1,
                effect = DaoAnEffect.FormationCostReduction,
                costs = new[] { 4500f, 13500f, 38000f, 100000f, 270000f, 750000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.TrucCo, RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu },
                minSubLevels = new[] { 4, 7, 4, 4, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "offline_cultivation", displayName = "Tĩnh Tu Minh Ngộ", description = "Thiền định nhập định tầng sâu, gia tăng Tu Vi nhận được khi offline",
                branch = "cultivation", tier = 4, tierName = "Tầng 4: Kim Đan Kỳ (Ngưng Kết Kim Đan)",
                icon = "☯️", unit = "% Tu Vi Offline",
                prerequisiteId = "repose", prerequisiteLevel = 1,
                effect = DaoAnEffect.OfflineTuVi,
                costs = new[] { 4200f, 12600f, 36000f, 95000f, 260000f, 700000f },
                bonuses = new[] { 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 7, 1, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "alchemy_material_efficiency", displayName = "Tiết Dược Bí Thuật", description = "Giảm Linh Chi và Cỏ Ngưng Khí, giữ ít nhất một mỗi loại",
                branch = "alchemy", tier = 4, tierName = "Tầng 4: Kim Đan Kỳ (Ngưng Kết Kim Đan)",
                icon = "🧪", unit = "Thảo Dược Tiết Kiệm",
                prerequisiteId = "herb_luck", prerequisiteLevel = 1,
                effect = DaoAnEffect.AlchemyCostReduction,
                costs = new[] { 5000f, 15000f, 42000f, 110000f, 300000f, 850000f },
                bonuses = new[] { 1f, 1f, 1f, 1f, 1f, 1f },
                minRealms = new[] { RealmId.KimDan, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "boss_stone", displayName = "Chiến Lộc Đạo Ấn", description = "Sát phạt yêu ma đầu lĩnh, tăng mạnh Linh Thạch rơi độc quyền từ Boss",
                branch = "wealth", tier = 4, tierName = "Tầng 4: Kim Đan Kỳ (Ngưng Kết Kim Đan)",
                icon = "👑", unit = "% LT Boss",
                prerequisiteId = "kim_thiem", prerequisiteLevel = 1,
                effect = DaoAnEffect.BossLinhThach,
                costs = new[] { 5500f, 16500f, 46000f, 120000f, 330000f, 900000f },
                bonuses = new[] { 0.03f, 0.03f, 0.03f, 0.03f, 0.03f, 0.03f },
                minRealms = new[] { RealmId.KimDan, RealmId.KimDan, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 4, 7, 4, 4, 4, 4 }
            },

            // TẦNG 5: NGUYÊN ANH KỲ (NY 1 - 4)
            new DaoAnDefinition
            {
                id = "artifact_ore_efficiency", displayName = "Tinh Quặng Chi Đạo", description = "Rèn đúc hóa cảnh, giảm tiêu hao Quặng Rèn khi nâng cấp Pháp Khí",
                branch = "body_artifact", tier = 5, tierName = "Tầng 5: Nguyên Anh Kỳ (Ngưng Anh Hóa Thần)",
                icon = "⚒️", unit = "% Giảm Quặng",
                prerequisiteId = "ore_luck", prerequisiteLevel = 1,
                effect = DaoAnEffect.ArtifactOreReduction,
                costs = new[] { 7000f, 21000f, 60000f, 160000f, 420000f, 1200000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.NguyenAnh, RealmId.NguyenAnh, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 7, 4, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "sword_crit", displayName = "Vạn Kiếm Quy Tông", description = "Kiếm khí ngập trời, tăng tỷ lệ kích hoạt Bạo Kích x2 sát thương kiếm",
                branch = "sword", tier = 5, tierName = "Tầng 5: Nguyên Anh Kỳ (Ngưng Anh Hóa Thần)",
                icon = "🌟", unit = "% Bạo Kích",
                prerequisiteId = "sword", prerequisiteLevel = 2,
                effect = DaoAnEffect.SwordCritChance,
                costs = new[] { 8500f, 25000f, 72000f, 190000f, 500000f, 1400000f },
                bonuses = new[] { 0.04f, 0.04f, 0.04f, 0.04f, 0.04f, 0.04f },
                minRealms = new[] { RealmId.NguyenAnh, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.HopThe, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 1, 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "artifact_stone_efficiency", displayName = "Bảo Tài Hộ Mệnh", description = "Luyện khí tâm đắc, giảm tiêu hao Linh Thạch khi nâng cấp Pháp Khí",
                branch = "body_artifact", tier = 5, tierName = "Tầng 5: Nguyên Anh Kỳ (Ngưng Anh Hóa Thần)",
                icon = "📿", unit = "% Giảm LT Pháp Khí",
                prerequisiteId = "artifact_ore_efficiency", prerequisiteLevel = 1,
                effect = DaoAnEffect.ArtifactCostReduction,
                costs = new[] { 7500f, 22500f, 65000f, 175000f, 460000f, 1300000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.NguyenAnh, RealmId.NguyenAnh, RealmId.HoaThan, RealmId.LuyenHu, RealmId.HopThe, RealmId.DaiThua },
                minSubLevels = new[] { 4, 7, 1, 1, 1, 1 }
            },

            // TẦNG 6: HÓA THẦN KỲ (HT 1 - 4)
            new DaoAnDefinition
            {
                id = "body_stone_efficiency", displayName = "Thể Tài Cường Hóa", description = "Thân thể kim cang, giảm tiêu hao Linh Thạch khi nâng cấp Luyện Thể",
                branch = "body", tier = 6, tierName = "Tầng 6: Hóa Thần Kỳ (Phá Toái Hư Không)",
                icon = "🥋", unit = "% Giảm LT Thể",
                effect = DaoAnEffect.BodyCostReduction,
                costs = new[] { 10000f, 30000f, 85000f, 230000f, 600000f, 1700000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f, 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.HoaThan, RealmId.HoaThan, RealmId.HoaThan, RealmId.LuyenHu, RealmId.HopThe, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 7, 4, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "boss_ore", displayName = "Báu Khoáng Thần Tích", description = "Khai sơn phá thạch, tăng bảo đảm +1 Quặng Rèn rơi thêm từ Boss",
                branch = "body_artifact", tier = 6, tierName = "Tầng 6: Hóa Thần Kỳ (Phá Toái Hư Không)",
                icon = "⛏️", unit = "Quặng Boss Thêm",
                prerequisiteId = "artifact_ore_efficiency", prerequisiteLevel = 1,
                effect = DaoAnEffect.BossOreDrop,
                costs = new[] { 12000f, 36000f, 100000f, 270000f, 720000f, 2000000f },
                bonuses = new[] { 1f, 1f, 1f, 1f, 1f, 1f },
                minRealms = new[] { RealmId.HoaThan, RealmId.HoaThan, RealmId.LuyenHu, RealmId.LuyenHu, RealmId.HopThe, RealmId.DaiThua },
                minSubLevels = new[] { 4, 7, 1, 7, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "spirit_duration", displayName = "Dư Trận Trường Cửu", description = "Kéo dài thời gian hiệu lực của Tụ Linh Trận chủ động",
                branch = "formation", tier = 6, tierName = "Tầng 6: Hóa Thần Kỳ (Phá Toái Hư Không)",
                icon = "🌀", unit = "s Duy Trì Trận",
                prerequisiteId = "formation_stone_efficiency", prerequisiteLevel = 1,
                effect = DaoAnEffect.FormationDuration,
                costs = new[] { 11000f, 33000f, 95000f, 250000f, 660000f, 1850000f },
                bonuses = new[] { 2f, 2f, 2f, 2f, 2f, 2f },
                minRealms = new[] { RealmId.HoaThan, RealmId.HoaThan, RealmId.HoaThan, RealmId.HopThe, RealmId.HopThe, RealmId.DaiThua },
                minSubLevels = new[] { 1, 4, 7, 1, 7, 4 }
            },
            new DaoAnDefinition
            {
                id = "sword_tier_weight", displayName = "Trạch Kiếm Đạo Ấn", description = "Kiếm từ quái thường dễ có cấp 2 trở lên trong các cấp đã mở",
                branch = "sword", tier = 3, tierName = "Tầng 3: Trúc Cơ Kỳ (Đạo Cơ Vững Chắc)",
                icon = "🗡️", unit = "% Trọng Số Kiếm Cấp Cao",
                prerequisiteId = "sword_luck", prerequisiteLevel = 2,
                effect = DaoAnEffect.SwordTierWeight,
                costs = new[] { 32_000_000f, 260_000_000f, 2_100_000_000f },
                bonuses = new[] { 0.04f, 0.04f, 0.04f },
                minRealms = new[] { RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh },
                minSubLevels = new[] { 4, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "boss_herb", displayName = "Dược Lộc Hộ Sơn", description = "Nhận thêm Linh Chi khi đánh Boss sau khi mở Luyện Đan",
                branch = "alchemy", tier = 2, tierName = "Tầng 2: Luyện Khí Trung & Hậu Kỳ",
                icon = "🌿", unit = "Linh Chi Thêm",
                prerequisiteId = "herb_luck", prerequisiteLevel = 1,
                effect = DaoAnEffect.BossHerbBonus,
                costs = new[] { 2_000_000f, 32_000_000f, 260_000_000f },
                bonuses = new[] { 1f, 1f, 1f },
                minRealms = new[] { RealmId.LuyenKhi, RealmId.TrucCo, RealmId.KimDan },
                minSubLevels = new[] { 9, 4, 4 }
            },
            new DaoAnDefinition
            {
                id = "alchemy_brew_speed", displayName = "Đan Hỏa Tốc Luyện", description = "Rút ngắn thời gian luyện đan, tối thiểu 10 giây mỗi mẻ",
                branch = "alchemy", tier = 3, tierName = "Tầng 3: Trúc Cơ Kỳ (Đạo Cơ Vững Chắc)",
                icon = "🔥", unit = "% Giảm Thời Gian Luyện",
                prerequisiteId = "herb_luck", prerequisiteLevel = 1,
                effect = DaoAnEffect.AlchemyBrewSpeed,
                costs = new[] { 16_000_000f, 130_000_000f, 1_050_000_000f },
                bonuses = new[] { 0.04f, 0.04f, 0.04f },
                minRealms = new[] { RealmId.TrucCo, RealmId.KimDan, RealmId.NguyenAnh },
                minSubLevels = new[] { 1, 1, 1 }
            },
            new DaoAnDefinition
            {
                id = "boss_damage_reduction", displayName = "Kim Thân Hộ Mệnh", description = "Giảm sát thương nhận vào từ Boss, kể cả Boss Độ Kiếp",
                branch = "body", tier = 6, tierName = "Tầng 6: Hóa Thần Kỳ (Phá Toái Hư Không)",
                icon = "🛡️", unit = "% Giảm Sát Thương Boss",
                prerequisiteId = "body_stone_efficiency", prerequisiteLevel = 1,
                effect = DaoAnEffect.BossDamageReduction,
                costs = new[] { 8_000_000_000f, 16_000_000_000f, 33_000_000_000f },
                bonuses = new[] { 0.02f, 0.02f, 0.02f },
                minRealms = new[] { RealmId.HoaThan, RealmId.HoaThan, RealmId.HoaThan },
                minSubLevels = new[] { 1, 4, 7 }
            }
        };
    }
}
#endif

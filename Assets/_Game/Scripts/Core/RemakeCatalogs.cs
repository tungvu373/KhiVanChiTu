using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class DaoVanNodeSpec
    {
        public string id;
        public string parentId;
        public string branchName;
        public string title;
        public string effectDesc;
        public string codexIconKey;
        public string reusedIconKey;
        public int maxLevel;
        public bool costsDaoPoints; // True = Diem Dao Van, False = Linh Thach
        public double baseCost;
        public float costGrowth;
        public Vector2 normalizedPos; // Offset from core (in pixels at zoom 1.0)
    }

    public sealed class MainQuestSpec
    {
        public string id;
        public string title;
        public string targetRoute; // "arena", "swordpath", "daovan", "alchemy", "equipment", "formation", "stage", "breakthrough"
        public int targetCount;
        public double rewardLinhThach;
        public int rewardDaoPoints;
        public int rewardSwordLevel;
    }

    public sealed class FormationSpec
    {
        public int index;
        public string name;
        public int minRealmIndex;
        public string realmReqText;
        public string codexIconKey;
        public string reusedIconKey;
        public string effectDesc;
        public float swordDamagePct;
        public float critChancePct;
        public float tuViPct;
        public float maxHpPct;
        public float swordSpeedPct;
        public float damageReductionPct;
    }

    public sealed class PioneerMilestoneSpec
    {
        public int index;
        public string title;
        public string targetHint;
        public int reqRealmIndex;
        public int reqLinearStage;
        public double rewardLinhThach;
        public int rewardDaoPoints;
        public int rewardForgeTalismans;
        public int rewardSkillScrolls;
        public int rewardCalmIncense;
    }

    public static class RemakeCatalogs
    {
        public static readonly IReadOnlyList<DaoVanNodeSpec> StarterDaoVanNodes = new[]
        {
            new DaoVanNodeSpec
            {
                id = "core_daotam", parentId = null, branchName = "Đạo Tâm",
                title = "Đạo Tâm Sơ Khai",
                effectDesc = "+12% Tu Vi & +10% Công Kiếm / cấp",
                codexIconKey = "DaoVan/daovan_core_node", reusedIconKey = "RouteTalents",
                maxLevel = 5, costsDaoPoints = true, baseCost = 1, costGrowth = 1f,
                normalizedPos = new Vector2(0f, 0f)
            },
            // === NHANH KIEM DAO (TOP) ===
            new DaoVanNodeSpec
            {
                id = "branch_sword", parentId = "core_daotam", branchName = "Kiếm Đạo",
                title = "Kiếm Tâm Thông Minh",
                effectDesc = "+15% Công Kiếm & +2% Bạo Kích / cấp",
                codexIconKey = "DaoVan/branch_sword", reusedIconKey = "RouteMerge",
                maxLevel = 10, costsDaoPoints = false, baseCost = 60, costGrowth = 1.50f,
                normalizedPos = new Vector2(0f, -108f)
            },
            new DaoVanNodeSpec
            {
                id = "sword_crit_dmg", parentId = "branch_sword", branchName = "Trọng Kích",
                title = "Kiếm Mang Sát Phạt",
                effectDesc = "+18% Sát Thương Bạo Kích / cấp",
                codexIconKey = "DaoVan/branch_sword", reusedIconKey = "Sword05",
                maxLevel = 10, costsDaoPoints = false, baseCost = 220, costGrowth = 1.58f,
                normalizedPos = new Vector2(-82f, -206f)
            },
            new DaoVanNodeSpec
            {
                id = "sword_armor_pierce", parentId = "branch_sword", branchName = "Xuyên Phá",
                title = "Phá Cương Kiếm Đạo",
                effectDesc = "+6% Xuyên Giáp Quái & +8% Công Boss / cấp",
                codexIconKey = "DaoVan/branch_sword", reusedIconKey = "Sword08",
                maxLevel = 10, costsDaoPoints = false, baseCost = 340, costGrowth = 1.62f,
                normalizedPos = new Vector2(82f, -206f)
            },
            new DaoVanNodeSpec
            {
                id = "sword_mastery", parentId = "sword_armor_pierce", branchName = "Kiếm Tôn",
                title = "Đại Đạo Vạn Kiếm (Node Lớn)",
                effectDesc = "+28% Tổng Sát Thương Phi Kiếm / cấp",
                codexIconKey = "DaoVan/daovan_core_node", reusedIconKey = "Sword10",
                maxLevel = 5, costsDaoPoints = true, baseCost = 2, costGrowth = 1.3f,
                normalizedPos = new Vector2(0f, -294f)
            },
            // === NHANH THE PHACH (TOP RIGHT — GOP 6 CHI SO LUYEN THE) ===
            new DaoVanNodeSpec
            {
                id = "branch_body", parentId = "core_daotam", branchName = "Thể Phách",
                title = "Bá Thể Tổng Hợp (6 Chỉ Số Luyện Thể)",
                effectDesc = "Gộp Khí Huyết · Cương Khí · Dũ Hợp · Kình Lực · Phệ Huyết · Định Tâm (+140 HP & +1.5% Hồi máu/cấp)",
                codexIconKey = "DaoVan/branch_body", reusedIconKey = "BodyVitality",
                maxLevel = 10, costsDaoPoints = false, baseCost = 55, costGrowth = 1.48f,
                normalizedPos = new Vector2(118f, -36f)
            },
            new DaoVanNodeSpec
            {
                id = "body_armor", parentId = "branch_body", branchName = "Cương Khí",
                title = "Bất Hoại Kim Thân (Cương Khí · Định Tâm)",
                effectDesc = "+3% Giảm Sát Thương & +2% Chống Thất Thủ / cấp",
                codexIconKey = "DaoVan/branch_body", reusedIconKey = "BodyGuard",
                maxLevel = 10, costsDaoPoints = false, baseCost = 200, costGrowth = 1.55f,
                normalizedPos = new Vector2(226f, -92f)
            },
            new DaoVanNodeSpec
            {
                id = "body_leech", parentId = "branch_body", branchName = "Phệ Huyết",
                title = "Thôn Linh Huyết Mạch (Dũ Hợp · Kình Lực · Phệ Huyết)",
                effectDesc = "+0.8% Hút Máu & +10% Kình Lực Công Kiếm / cấp",
                codexIconKey = "DaoVan/branch_body", reusedIconKey = "BodyDrain",
                maxLevel = 10, costsDaoPoints = false, baseCost = 260, costGrowth = 1.58f,
                normalizedPos = new Vector2(236f, 22f)
            },
            // === NHANH TU LUYEN (BOTTOM RIGHT) ===
            new DaoVanNodeSpec
            {
                id = "branch_cultivation", parentId = "core_daotam", branchName = "Tu Luyện",
                title = "Tụ Linh Quy Nguyên",
                effectDesc = "+15% Tu Vi từ quái & +5% Bế Quan / cấp",
                codexIconKey = "DaoVan/branch_cultivation", reusedIconKey = "RouteProgression",
                maxLevel = 10, costsDaoPoints = false, baseCost = 65, costGrowth = 1.54f,
                normalizedPos = new Vector2(78f, 96f)
            },
            new DaoVanNodeSpec
            {
                id = "cult_offline", parentId = "branch_cultivation", branchName = "Bế Quan",
                title = "Tĩnh Tọa Quy Nhất (24/7 AFK)",
                effectDesc = "+14% Hiệu Suất Bế Quan Ẩn Tab / cấp",
                codexIconKey = "DaoVan/branch_cultivation", reusedIconKey = "RouteMilestones",
                maxLevel = 10, costsDaoPoints = false, baseCost = 240, costGrowth = 1.58f,
                normalizedPos = new Vector2(182f, 166f)
            },
            new DaoVanNodeSpec
            {
                id = "cult_enlightenment", parentId = "cult_offline", branchName = "Ngộ Đạo",
                title = "Đại Thừa Ngộ Đạo (Node Lớn)",
                effectDesc = "+25% Tu Vi Toàn Cục & -4% Hồi Chiêu / cấp",
                codexIconKey = "DaoVan/daovan_core_node", reusedIconKey = "RouteAchievements",
                maxLevel = 5, costsDaoPoints = true, baseCost = 2, costGrowth = 1.3f,
                normalizedPos = new Vector2(106f, 246f)
            },
            // === NHANH TAI LOC (BOTTOM LEFT) ===
            new DaoVanNodeSpec
            {
                id = "branch_wealth", parentId = "core_daotam", branchName = "Tài Lộc",
                title = "Tụ Bảo Thiên Vận",
                effectDesc = "+15% Linh Thạch & +8% Tỉ lệ Rơi Phôi Kiếm / cấp",
                codexIconKey = "DaoVan/branch_wealth", reusedIconKey = "SpiritStone",
                maxLevel = 10, costsDaoPoints = false, baseCost = 50, costGrowth = 1.48f,
                normalizedPos = new Vector2(-78f, 96f)
            },
            new DaoVanNodeSpec
            {
                id = "wealth_ore", parentId = "branch_wealth", branchName = "Khai Khoáng",
                title = "Tinh Thiết Cơ Duyên",
                effectDesc = "+20% Quặng Cường Hóa & +12% Rơi Trang Bị / cấp",
                codexIconKey = "DaoVan/branch_wealth", reusedIconKey = "Ore",
                maxLevel = 10, costsDaoPoints = false, baseCost = 220, costGrowth = 1.55f,
                normalizedPos = new Vector2(-180f, 166f)
            },
            new DaoVanNodeSpec
            {
                id = "wealth_fortune", parentId = "wealth_ore", branchName = "Thiên Vận",
                title = "Khí Vận Bảo Hộ (Node Lớn)",
                effectDesc = "+30% Linh Thạch & +10% Xác Suất Ghép Kiếm / cấp",
                codexIconKey = "DaoVan/branch_wealth", reusedIconKey = "FortuneCoin",
                maxLevel = 5, costsDaoPoints = true, baseCost = 2, costGrowth = 1.3f,
                normalizedPos = new Vector2(-106f, 246f)
            },
            // === NHANH BACH NGHE (TOP LEFT) ===
            new DaoVanNodeSpec
            {
                id = "branch_arts", parentId = "core_daotam", branchName = "Bách Nghệ",
                title = "Đan Trận Song Tu",
                effectDesc = "+15% Thảo Dược rơi & +1.5% Đột Phá / cấp",
                codexIconKey = "DaoVan/branch_arts", reusedIconKey = "RouteAlchemy",
                maxLevel = 10, costsDaoPoints = false, baseCost = 60, costGrowth = 1.52f,
                normalizedPos = new Vector2(-118f, -36f)
            },
            new DaoVanNodeSpec
            {
                id = "arts_alchemy", parentId = "branch_arts", branchName = "Đan Đạo",
                title = "Thánh Thủ Đan Tông",
                effectDesc = "+3% Thành Công Luyện Đan & +20% XP Đan Sư / cấp",
                codexIconKey = "DaoVan/branch_arts", reusedIconKey = "Cauldron",
                maxLevel = 10, costsDaoPoints = false, baseCost = 250, costGrowth = 1.56f,
                normalizedPos = new Vector2(-226f, -92f)
            },
            new DaoVanNodeSpec
            {
                id = "arts_formation", parentId = "branch_arts", branchName = "Trận Đạo",
                title = "Thiên Địa Đại Trận",
                effectDesc = "+12% Uy Lực Trận Pháp & +2% Tỉ Lệ Đột Phá / cấp",
                codexIconKey = "DaoVan/branch_arts", reusedIconKey = "RouteFormation",
                maxLevel = 10, costsDaoPoints = false, baseCost = 290, costGrowth = 1.60f,
                normalizedPos = new Vector2(-236f, 22f)
            }
        };

        public static readonly IReadOnlyList<FormationSpec> Formations = new[]
        {
            new FormationSpec
            {
                index = 0, name = "1. Cửu Thiên Lôi Trận", minRealmIndex = 1, realmReqText = "Trúc Cơ Sơ Kỳ",
                codexIconKey = "Formations/formation_thunder", reusedIconKey = "RouteFormation",
                effectDesc = "+25% Công Kiếm & Tăng Sát Thương Lôi Phạt",
                swordDamagePct = 25f, critChancePct = 4f
            },
            new FormationSpec
            {
                index = 1, name = "2. Cực Hàn Băng Trận", minRealmIndex = 2, realmReqText = "Kim Đan Sơ Kỳ",
                codexIconKey = "", reusedIconKey = "FrostFormation",
                effectDesc = "+15% Tỉ Lệ Bạo Kích & +18% Công Kiếm",
                swordDamagePct = 18f, critChancePct = 15f
            },
            new FormationSpec
            {
                index = 2, name = "3. Bát Quái Tụ Linh Trận", minRealmIndex = 3, realmReqText = "Nguyên Anh Sơ Kỳ",
                codexIconKey = "", reusedIconKey = "SpiritLotus",
                effectDesc = "+40% Tốc Độ Tu Vi & +20% Linh Thạch",
                tuViPct = 40f
            },
            new FormationSpec
            {
                index = 3, name = "4. Huyết Ma Hóa Sinh Trận", minRealmIndex = 4, realmReqText = "Hóa Thần Sơ Kỳ",
                codexIconKey = "", reusedIconKey = "BloodFormation",
                effectDesc = "+35% Khí Huyết Tối Đa & +22% Công Kiếm",
                swordDamagePct = 22f, maxHpPct = 35f
            },
            new FormationSpec
            {
                index = 4, name = "5. Vạn Kiếm Quy Tông Trận", minRealmIndex = 5, realmReqText = "Luyện Hư Sơ Kỳ",
                codexIconKey = "Formations/formation_sword_rain", reusedIconKey = "RouteFormation",
                effectDesc = "+55% Công Kiếm & +30% Tốc Độ Phi Kiếm",
                swordDamagePct = 55f, swordSpeedPct = 30f
            },
            new FormationSpec
            {
                index = 5, name = "6. Huyền Vũ Kim Cang Trận", minRealmIndex = 6, realmReqText = "Hợp Thể Sơ Kỳ",
                codexIconKey = "", reusedIconKey = "GoldenBell",
                effectDesc = "+45% Khí Huyết, +35% Giảm Thương & +35% Tu Vi",
                tuViPct = 35f, maxHpPct = 45f, damageReductionPct = 35f
            }
        };

        public static readonly IReadOnlyList<MainQuestSpec> MainQuests = new[]
        {
            new MainQuestSpec { id = "q1_kill5", title = "Tiêu diệt 5 Yêu Thú tại Đồng Cỏ 1-1", targetRoute = "arena", targetCount = 5, rewardLinhThach = 120, rewardDaoPoints = 0, rewardSwordLevel = 1 },
            new MainQuestSpec { id = "q2_merge_lv2", title = "Mở Kiếm Lộ và ghép thành Kiếm Cấp 2", targetRoute = "swordpath", targetCount = 1, rewardLinhThach = 200, rewardDaoPoints = 1, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q3_reach_t4", title = "Tu luyện đạt Luyện Khí Tầng 4 (Mở Đạo Vận & Lôi Phạt)", targetRoute = "breakthrough", targetCount = 4, rewardLinhThach = 300, rewardDaoPoints = 1, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q4_daovan_skill", title = "Nâng Đạo Tâm trên Cây Đạo Vận", targetRoute = "daovan", targetCount = 1, rewardLinhThach = 450, rewardDaoPoints = 1, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q5_alchemy_t7", title = "Đạt Tầng 7 & Luyện 1 viên Trúc Cơ Đan", targetRoute = "alchemy", targetCount = 1, rewardLinhThach = 800, rewardDaoPoints = 1, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q6_boss_1_10", title = "Đánh bại Cổ Mộc Yêu Vương ở Ải 1-10", targetRoute = "stage", targetCount = 1, rewardLinhThach = 1500, rewardDaoPoints = 2, rewardSwordLevel = 2 },
            new MainQuestSpec { id = "q7_truc_co", title = "Dùng Trúc Cơ Đan Đột Phá Trúc Cơ Sơ Kỳ", targetRoute = "breakthrough", targetCount = 1, rewardLinhThach = 3000, rewardDaoPoints = 3, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q8_act2_enter", title = "Tiến vào Vực 2: Hoang Mạc Nắng Vàng (Ải 2-1)", targetRoute = "stage", targetCount = 1, rewardLinhThach = 4500, rewardDaoPoints = 1, rewardSwordLevel = 2 },
            new MainQuestSpec { id = "q9_formation_1", title = "Kích hoạt Cửu Thiên Lôi Trận & mặc Trang Bị", targetRoute = "formation", targetCount = 1, rewardLinhThach = 6500, rewardDaoPoints = 2, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q10_act2_boss", title = "Chinh phục Sa Hoàng Yêu Đế tại Ải 2-10", targetRoute = "stage", targetCount = 1, rewardLinhThach = 14000, rewardDaoPoints = 3, rewardSwordLevel = 3 },
            new MainQuestSpec { id = "q11_kim_dan", title = "Đột phá đạt Kim Đan Sơ Kỳ & mở Ải 3-1", targetRoute = "breakthrough", targetCount = 1, rewardLinhThach = 28000, rewardDaoPoints = 3, rewardSwordLevel = 0 },
            new MainQuestSpec { id = "q12_act3_boss", title = "Đánh bại Thái Hư Kiếm Tôn tại Vực 3 (Ải 3-10)", targetRoute = "stage", targetCount = 1, rewardLinhThach = 60000, rewardDaoPoints = 5, rewardSwordLevel = 4 }
        };

        public static readonly IReadOnlyList<PioneerMilestoneSpec> PioneerMilestones = new[]
        {
            new PioneerMilestoneSpec { index = 0, title = "Mốc 1 (Ngày 1 · ~4h): Nhập Đạo Đồng Cỏ", targetHint = "Đạt Trúc Cơ Sơ Kỳ", reqRealmIndex = 1, reqLinearStage = 9, rewardLinhThach = 2000, rewardDaoPoints = 2, rewardForgeTalismans = 1, rewardSkillScrolls = 1, rewardCalmIncense = 0 },
            new PioneerMilestoneSpec { index = 1, title = "Mốc 2 (Ngày 1 · ~24h): Hoành Tảo Hoang Mạc", targetHint = "Đạt Kim Đan Sơ Kỳ · Vượt 2-10", reqRealmIndex = 2, reqLinearStage = 19, rewardLinhThach = 12000, rewardDaoPoints = 3, rewardForgeTalismans = 2, rewardSkillScrolls = 2, rewardCalmIncense = 1 },
            new PioneerMilestoneSpec { index = 2, title = "Mốc 3 (Ngày 3 · ~72h): Đăng Đỉnh Bạch Ngọc", targetHint = "Đạt Nguyên Anh Sơ Kỳ · Vượt 3-10", reqRealmIndex = 3, reqLinearStage = 29, rewardLinhThach = 45000, rewardDaoPoints = 4, rewardForgeTalismans = 3, rewardSkillScrolls = 3, rewardCalmIncense = 1 },
            new PioneerMilestoneSpec { index = 3, title = "Mốc 4 (Ngày 7 · ~168h): Khai Phá Yêu Vực", targetHint = "Đạt Hóa Thần Sơ Kỳ (Mở 2 Trận & Ấn Chưởng)", reqRealmIndex = 4, reqLinearStage = 39, rewardLinhThach = 150000, rewardDaoPoints = 5, rewardForgeTalismans = 4, rewardSkillScrolls = 4, rewardCalmIncense = 2 },
            new PioneerMilestoneSpec { index = 4, title = "Mốc 5 (Ngày 13 · ~312h): Hư Không Kiếm Đạo", targetHint = "Đạt Luyện Hư Sơ Kỳ", reqRealmIndex = 5, reqLinearStage = 59, rewardLinhThach = 500000, rewardDaoPoints = 6, rewardForgeTalismans = 5, rewardSkillScrolls = 5, rewardCalmIncense = 2 },
            new PioneerMilestoneSpec { index = 5, title = "Mốc 6 (Ngày 20 · ~492h): Hợp Nhất Thiên Địa", targetHint = "Đạt Hợp Thể Sơ Kỳ", reqRealmIndex = 6, reqLinearStage = 79, rewardLinhThach = 1800000, rewardDaoPoints = 8, rewardForgeTalismans = 6, rewardSkillScrolls = 6, rewardCalmIncense = 3 },
            new PioneerMilestoneSpec { index = 6, title = "Mốc 7 (Ngày 30 · 720h): Đại Thừa Đắc Đạo", targetHint = "Đạt Đại Thừa Sơ Kỳ (Mục tiêu 30 ngày)", reqRealmIndex = 7, reqLinearStage = 99, rewardLinhThach = 6000000, rewardDaoPoints = 12, rewardForgeTalismans = 10, rewardSkillScrolls = 10, rewardCalmIncense = 5 },
            new PioneerMilestoneSpec { index = 7, title = "Mốc 8 (Endgame): Tiên Kiếp Chí Tôn", targetHint = "Đạt Độ Kiếp & Chinh Phục Ải 120", reqRealmIndex = 8, reqLinearStage = 119, rewardLinhThach = 20000000, rewardDaoPoints = 20, rewardForgeTalismans = 15, rewardSkillScrolls = 15, rewardCalmIncense = 8 }
        };
    }
}

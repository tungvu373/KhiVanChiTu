using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Bang can bang chinh thuc ban Remake V2 (720 gio 24/7 len Dai Thua So Ky).
    /// Tang 10 chiem 19% tong Tu Vi moi Dai Canh Gioi; chong nan ngay dau Luyen Khi -> Truc Co = 45% goc.
    /// </summary>
    public static class CultivationTable
    {
        public static readonly string[] RealmNames =
        {
            "Luyện Khí", "Trúc Cơ", "Kim Đan", "Nguyên Anh",
            "Hóa Thần", "Luyện Hư", "Hợp Thể", "Đại Thừa", "Độ Kiếp"
        };

        // Gio treo may 24/7 ky vong cua moi Dai Canh Gioi (cong don 0..7 = 720h)
        public static readonly float[] ExpectedRealmHours24x7 =
        {
            4f,     // 0: Luyen Khi (4h)
            20f,    // 1: Truc Co (tong 24h = 1 ngay)
            48f,    // 2: Kim Dan (tong 72h = 3 ngay)
            96f,    // 3: Nguyen Anh (tong 168h = 7 ngay)
            144f,   // 4: Hoa Than (tong 312h = 13 ngay)
            180f,   // 5: Luyen Hu (tong 492h = 20.5 ngay)
            228f,   // 6: Hop The -> len Dai Thua (tong 720h = 30 ngay)
            480f,   // 7: Dai Thua (thang 2+)
            720f    // 8: Do Kiep (Endgame)
        };

        // Tu Vi mục tiêu quy chuẩn của cả Đại Cảnh Giới (khớp nhịp tăng theo Stage 1..120)
        private static readonly double[] BaseRealmTotalTuVi =
        {
            6800,           // Luyen Khi (~4h o ai 1-1 -> 1-10)
            62000,          // Truc Co (~20h o ai 2-1 -> 2-10)
            420000,         // Kim Dan (~48h)
            3200000,        // Nguyen Anh (~96h)
            24000000,       // Hoa Than (~144h)
            165000000,      // Luyen Hu (~180h)
            1150000000,     // Hop The -> Dai Thua (~228h, tich luy 720h)
            8500000000,     // Dai Thua
            60000000000     // Do Kiep
        };

        // Ty le phan bo 10 tang trong 1 Dai Canh Gioi (Tang 10 = 19% -> tong = 100%)
        private static readonly double[] SubStageWeights =
        {
            0.045, 0.055, 0.065, // Tang 1-3: So Ky (16.5%)
            0.075, 0.085, 0.095, // Tang 4-6: Trung Ky (25.5%)
            0.115, 0.130, 0.145, // Tang 7-9: Hau Ky (39.0%)
            0.190                // Tang 10: Dinh Phong Binh Canh (19.0%)
        };

        private static readonly float[] RealmBreakthroughBaseChance =
        {
            0.45f, // Luyen Khi -> Truc Co (Bao hiem tan thu ngay 1)
            0.32f, // Truc Co -> Kim Dan
            0.25f, // Kim Dan -> Nguyen Anh
            0.20f, // Nguyen Anh -> Hoa Than
            0.15f, // Hoa Than -> Luyen Hu
            0.12f, // Luyen Hu -> Hop The
            0.08f, // Hop The -> Dai Thua
            0.05f, // Dai Thua -> Do Kiep
            0.03f  // Do Kiep Vien Man
        };

        // Chuan hoa ghep 2 thanh cung cap (Lv1->Lv2..Lv9->Lv10)
        public static readonly float[] SwordMergeBaseRate =
        {
            0.95f, 0.85f, 0.75f, 0.65f, 0.55f, 0.45f, 0.35f, 0.25f, 0.15f
        };

        public static readonly float[] SwordMergePityStep =
        {
            0.15f, 0.12f, 0.10f, 0.08f, 0.07f, 0.06f, 0.05f, 0.05f, 0.04f
        };

        public static readonly double[] SwordMergeLinhThachCost =
        {
            40, 120, 320, 850, 2200, 5600, 14000, 36000, 95000
        };

        public static string GetRealmDisplayName(int realmIndex, int subStage)
        {
            string rName = RealmNames[Mathf.Clamp(realmIndex, 0, RealmNames.Length - 1)];
            int s = Mathf.Clamp(subStage, 1, 10);
            string phase = s <= 3 ? "Sơ Kỳ" : s <= 6 ? "Trung Kỳ" : s <= 9 ? "Hậu Kỳ" : "Đỉnh Phong";
            return $"{rName} Tầng {s} ({phase})";
        }

        public static string GetRealmShortName(int realmIndex, int subStage)
        {
            string rName = RealmNames[Mathf.Clamp(realmIndex, 0, RealmNames.Length - 1)];
            return $"{rName} · T{Mathf.Clamp(subStage, 1, 10)}";
        }

        public static double GetRequiredTuVi(int realmIndex, int subStage)
        {
            int r = Mathf.Clamp(realmIndex, 0, BaseRealmTotalTuVi.Length - 1);
            int s = Mathf.Clamp(subStage, 1, 10) - 1;
            return Math.Round(BaseRealmTotalTuVi[r] * SubStageWeights[s]);
        }

        public static int GetMaxSwordSlotsForRealm(int realmIndex)
        {
            int r = Mathf.Clamp(realmIndex, 0, RealmNames.Length - 1);
            return Mathf.Clamp(1 + r, 1, 10);
        }

        public static float GetBaseBreakthroughChance(int realmIndex)
        {
            return RealmBreakthroughBaseChance[Mathf.Clamp(realmIndex, 0, RealmBreakthroughBaseChance.Length - 1)];
        }

        public static float GetPillBreakthroughBonus(RemakePillQuality quality) => quality switch
        {
            RemakePillQuality.HaPham => 0.15f,
            RemakePillQuality.TrungPham => 0.28f,
            RemakePillQuality.ThuongPham => 0.42f,
            RemakePillQuality.CucPham => 0.60f,
            _ => 0f
        };

        public static float GetFailurePenaltyPercent(int consecutiveFailures, RemakePillQuality pillUsed)
        {
            float basePenalty = consecutiveFailures switch
            {
                0 => 0.50f,
                1 => 0.75f,
                _ => 1.00f
            };
            float mitigation = pillUsed switch
            {
                RemakePillQuality.ThuongPham => 0.05f,
                RemakePillQuality.CucPham => 0.10f,
                _ => 0f
            };
            return Mathf.Clamp(basePenalty - mitigation, 0.40f, 1.00f);
        }

        public static double CalculateSwordDamage(int level)
        {
            int lv = Mathf.Clamp(level, 1, 10);
            return Math.Round(12.0 * Math.Pow(2.05, lv - 1));
        }

        public static float CalculateSwordSpeed(int level)
        {
            return 7.5f + Mathf.Clamp(level, 1, 10) * 1.15f;
        }

        public static float GetSwordMergeEffectiveChance(int fromLevel, float pityBonus)
        {
            int idx = Mathf.Clamp(fromLevel - 1, 0, SwordMergeBaseRate.Length - 1);
            return Mathf.Clamp01(SwordMergeBaseRate[idx] + pityBonus);
        }

        public static double GetSwordMergeCost(int fromLevel)
        {
            int idx = Mathf.Clamp(fromLevel - 1, 0, SwordMergeLinhThachCost.Length - 1);
            return SwordMergeLinhThachCost[idx];
        }

        public static string GetPillNameForRealmBreakthrough(int fromRealmIndex) => Mathf.Clamp(fromRealmIndex, 0, 7) switch
        {
            0 => "Trúc Cơ Đan",
            1 => "Kết Kim Đan",
            2 => "Ngưng Anh Đan",
            3 => "Hóa Thần Đan",
            4 => "Luyện Hư Đan",
            5 => "Hợp Thể Đan",
            6 => "Đại Thừa Đan",
            _ => "Độ Kiếp Tiên Đan"
        };

        public static string GetPillQualityName(RemakePillQuality q) => q switch
        {
            RemakePillQuality.PheDan => "Phế Đan",
            RemakePillQuality.HaPham => "Hạ Phẩm",
            RemakePillQuality.TrungPham => "Trung Phẩm",
            RemakePillQuality.ThuongPham => "Thượng Phẩm",
            RemakePillQuality.CucPham => "Cực Phẩm (Đan Văn)",
            _ => "Hạ Phẩm"
        };

        public static bool IsFeatureUnlocked(RemakeFeatureId feature, int realmIndex, int subStage)
        {
            int linearRealmStage = realmIndex * 10 + subStage; // Luyen Khi 1 = 1, Truc Co 1 = 11...
            return feature switch
            {
                RemakeFeatureId.CombatAndSwordPath => true,
                RemakeFeatureId.DaoVanTree => linearRealmStage >= 4,          // Luyen Khi Tang 4
                RemakeFeatureId.SkillLoiPhat => linearRealmStage >= 4,        // Luyen Khi Tang 4
                RemakeFeatureId.Alchemy => linearRealmStage >= 7,             // Luyen Khi Tang 7
                RemakeFeatureId.BreakthroughTrucCo => linearRealmStage >= 10, // Luyen Khi Tang 10
                RemakeFeatureId.EquipmentLoot => linearRealmStage >= 11,      // Truc Co So Ky
                RemakeFeatureId.Formation1 => linearRealmStage >= 11,         // Truc Co So Ky
                RemakeFeatureId.SkillYinYang => linearRealmStage >= 11,       // Truc Co So Ky
                RemakeFeatureId.SkillSwordRain => linearRealmStage >= 21,     // Kim Dan So Ky
                RemakeFeatureId.SkillPalm => linearRealmStage >= 41,          // Hoa Than So Ky
                RemakeFeatureId.FormationSubSlot => linearRealmStage >= 41,   // Hoa Than So Ky
                RemakeFeatureId.ImmortalTower => linearRealmStage >= 15,      // Truc Co Tang 5
                RemakeFeatureId.SecretRealm => linearRealmStage >= 21,        // Kim Dan So Ky
                RemakeFeatureId.PvpComingSoon => false,                       // Coming Soon (Du kien Nguyen Anh T5)
                RemakeFeatureId.OnlineFeatures => true,
                _ => false
            };
        }

        public static string GetFeatureUnlockRequirementText(RemakeFeatureId feature) => feature switch
        {
            RemakeFeatureId.DaoVanTree => "Mở ở Luyện Khí Tầng 4",
            RemakeFeatureId.SkillLoiPhat => "Mở ở Luyện Khí Tầng 4",
            RemakeFeatureId.Alchemy => "Mở ở Luyện Khí Tầng 7",
            RemakeFeatureId.BreakthroughTrucCo => "Mở ở Luyện Khí Tầng 10",
            RemakeFeatureId.EquipmentLoot => "Mở ở Trúc Cơ Sơ Kỳ",
            RemakeFeatureId.Formation1 => "Mở ở Trúc Cơ Sơ Kỳ",
            RemakeFeatureId.SkillYinYang => "Mở ở Trúc Cơ Sơ Kỳ",
            RemakeFeatureId.SkillSwordRain => "Mở ở Kim Đan Sơ Kỳ",
            RemakeFeatureId.SkillPalm => "Mở ở Hóa Thần Sơ Kỳ",
            RemakeFeatureId.FormationSubSlot => "Mở ở Hóa Thần Sơ Kỳ",
            RemakeFeatureId.ImmortalTower => "Mở ở Trúc Cơ Tầng 5",
            RemakeFeatureId.SecretRealm => "Mở ở Kim Đan Sơ Kỳ",
            RemakeFeatureId.PvpComingSoon => "Sắp Ra Mắt (Dự kiến Nguyên Anh T5)",
            RemakeFeatureId.OnlineFeatures => "Mở ở Mốc 3 (Online LAN)",
            _ => "Đã mở"
        };

        // =========================================================
        // MOC 2: 8 DAN DUOC, 8 PHAM DAN SU, 4 LINH HOA, 6 TRAN PHAP, TRANG BI
        // =========================================================
        public static readonly string[] AlchemistRankNames =
        {
            "Nhất Phẩm", "Nhị Phẩm", "Tam Phẩm", "Tứ Phẩm",
            "Ngũ Phẩm", "Lục Phẩm", "Thất Phẩm", "Bát Phẩm"
        };

        public static readonly int[] AlchemistRankXpReq = { 0, 60, 180, 420, 900, 1800, 3400, 6200 };

        public static string GetAlchemistRankName(int rank) =>
            AlchemistRankNames[Mathf.Clamp(rank - 1, 0, AlchemistRankNames.Length - 1)];

        public static float GetAlchemistQualityBonus(int rank) => Mathf.Clamp(rank - 1, 0, 7) * 0.03f;

        public static string GetSpiritFlameName(int tier) => Mathf.Clamp(tier, 1, 4) switch
        {
            1 => "Phàm Hỏa",
            2 => "Thanh Liên Hỏa",
            3 => "Tử Cực Hỏa",
            _ => "Tiên Thiên Hỏa"
        };

        public static string GetSpiritFlameCodexKey(int tier) => Mathf.Clamp(tier, 1, 4) switch
        {
            1 => "Pills/flame_1_mortal",
            2 => "Pills/flame_2_azure_lotus",
            3 => "Pills/flame_3_violet_core",
            _ => "Pills/flame_4_immortal_gold"
        };

        public static int GetSpiritFlameNextTierXp(int currentTier) => currentTier switch
        {
            1 => 80,
            2 => 220,
            3 => 550,
            _ => 999999
        };

        public static string GetPillCodexKey(int realmTarget) => Mathf.Clamp(realmTarget, 0, 7) switch
        {
            0 => "Pills/pill_01_truc_co",
            1 => "Pills/pill_02_ket_kim",
            2 => "Pills/pill_03_nguyen_anh",
            3 => "Pills/pill_04_hoa_than",
            4 => "Pills/pill_05_luyen_hu",
            5 => "Pills/pill_06_hop_the",
            6 => "Pills/pill_07_dai_thua",
            _ => "Pills/pill_08_do_kiep"
        };

        public static int GetPillRequiredHerbTier(int targetRealmIndex) => Mathf.Clamp(1 + (targetRealmIndex >= 2 ? (targetRealmIndex + 1) / 2 : 0), 1, 6);
        public static int GetPillRequiredHerbCount(int targetRealmIndex) => 2 + Mathf.Clamp(targetRealmIndex, 0, 7);
        public static double GetPillBrewLinhThachCost(int targetRealmIndex) => targetRealmIndex == 0 ? 0 : Math.Round(120 * Math.Pow(2.4, targetRealmIndex));

        public static string GetEquipSlotName(RemakeEquipSlot slot) => slot switch
        {
            RemakeEquipSlot.Robe => "Đạo Bào",
            RemakeEquipSlot.Crown => "Đạo Quán",
            RemakeEquipSlot.Boots => "Vân Phong Hài",
            RemakeEquipSlot.Amulet => "Linh Ngọc Bội",
            RemakeEquipSlot.Bracer => "Hộ Uyển",
            _ => "Trữ Vật Giới"
        };

        public static (string codexKey, string reusedKey) GetEquipSlotIcons(RemakeEquipSlot slot) => slot switch
        {
            RemakeEquipSlot.Robe => ("", "Robe"),
            RemakeEquipSlot.Crown => ("", "Crown"),
            RemakeEquipSlot.Boots => ("", "Boots"),
            RemakeEquipSlot.Amulet => ("", "Amulet"),
            RemakeEquipSlot.Bracer => ("Equipment/equip_bracer", ""),
            _ => ("Equipment/equip_ring", "")
        };

        public static string GetRarityName(RemakeRarity r) => r switch
        {
            RemakeRarity.Pham => "Phàm",
            RemakeRarity.Linh => "Linh",
            RemakeRarity.Huyen => "Huyền",
            RemakeRarity.Dia => "Địa",
            RemakeRarity.Thien => "Thiên",
            _ => "Phàm"
        };

        public static string GetRarityFrameCodexKey(RemakeRarity r) => r switch
        {
            RemakeRarity.Pham => "HUD/rarity_1_pham",
            RemakeRarity.Linh => "HUD/rarity_2_linh",
            RemakeRarity.Huyen => "HUD/rarity_3_huyen",
            RemakeRarity.Dia => "HUD/rarity_4_dia",
            RemakeRarity.Thien => "HUD/rarity_5_thien",
            _ => "HUD/rarity_1_pham"
        };

        public static string GetEquipSetName(RemakeEquipSet setId) => setId switch
        {
            RemakeEquipSet.ThanhVan => "Bộ Thanh Vân (Kiếm Ý)",
            RemakeEquipSet.HuyenVu => "Bộ Huyền Vũ (Hộ Thuẫn)",
            RemakeEquipSet.DanTong => "Bộ Đan Tông (Tu Đan)",
            _ => "Đơn Lẻ"
        };

        public static string GetEquipSetCodexKey(RemakeEquipSet setId) => setId switch
        {
            RemakeEquipSet.ThanhVan => "Equipment/set_thanh_van",
            RemakeEquipSet.HuyenVu => "Equipment/set_huyen_vu",
            RemakeEquipSet.DanTong => "Equipment/set_dan_tong",
            _ => ""
        };

        public static string GetEquipStatLabel(RemakeEquipStatType t, float v) => t switch
        {
            RemakeEquipStatType.FlatSwordAtk => $"+{Mathf.RoundToInt(v)} Công Kiếm",
            RemakeEquipStatType.SwordDamagePct => $"+{v:0.#}% Sát Thương Kiếm",
            RemakeEquipStatType.CritChancePct => $"+{v:0.#}% Tỉ Lệ Bạo Kích",
            RemakeEquipStatType.MaxHpPct => $"+{v:0.#}% Khí Huyết Tối Đa",
            RemakeEquipStatType.TuViGainPct => $"+{v:0.#}% Tốc Độ Tu Vi",
            _ => $"+{v:0.#}% Linh Thạch Rơi"
        };

        public static double GetEquipSlotEnhanceCostLt(int currentLv) => Math.Round(80 * Math.Pow(1.42, currentLv));
        public static int GetEquipSlotEnhanceCostOre(int currentLv) => 2 + currentLv * 2;

        public static float CalculateEquipItemScore(RemakeEquipmentItem item, int realmIndex = 2)
        {
            if (item == null) return 0f;
            float score = (1f + (int)item.rarity * 1.4f) * (1f + item.tier * 0.45f) * 20f;
            if (item.setId != RemakeEquipSet.None) score += 18f;
            if (item.substats != null)
            {
                for (int i = 0; i < item.substats.Count; i++)
                {
                    var st = item.substats[i];
                    score += st.statType switch
                    {
                        RemakeEquipStatType.FlatSwordAtk => st.value * 0.85f,
                        RemakeEquipStatType.SwordDamagePct => st.value * 3.8f,
                        RemakeEquipStatType.CritChancePct => st.value * 7.5f,
                        RemakeEquipStatType.MaxHpPct => st.value * 2.2f,
                        RemakeEquipStatType.TuViGainPct => st.value * 2.6f,
                        _ => st.value * 1.5f
                    };
                }
            }
            return Mathf.Round(score);
        }

        public static (double hp, double dmg, double rewardLt, int enemyCount) GetDemonTowerFloorSpec(int floor)
        {
            int f = Mathf.Clamp(floor, 1, 100);
            double hp = Math.Round(1800.0 * Math.Pow(1.19, f - 1));
            double dmg = Math.Round(38.0 * Math.Pow(1.14, f - 1));
            double rewardLt = Math.Round(350.0 * Math.Pow(1.15, f - 1));
            int count = (f % 5 == 0) ? 1 : 4; // Floor 5,10,15... is Elite Guardian Boss
            return (hp, dmg, rewardLt, count);
        }

        public static (string title, double hp, double dmg, int enemyCount) GetSecretRealmTierSpec(RemakeSecretRealmKind kind, int tier)
        {
            int t = Mathf.Clamp(tier, 1, 5);
            string baseName = kind switch
            {
                RemakeSecretRealmKind.SwordTomb => "Kiếm Mộ Cổ",
                RemakeSecretRealmKind.HerbGarden => "Bách Thảo Viên",
                _ => "Tinh Thiết Động"
            };
            double hp = Math.Round(4500.0 * Math.Pow(4.2, t - 1));
            double dmg = Math.Round(72.0 * Math.Pow(2.9, t - 1));
            return ($"{baseName} · Tầng {t}", hp, dmg, 5);
        }

        public static string FormatNumber(double value)
        {
            if (value < 0) return "-" + FormatNumber(-value);
            if (value < 1000) return Math.Floor(value).ToString("0");
            if (value < 1_000_000) return (value / 1_000.0).ToString("0.##") + "K";
            if (value < 1_000_000_000) return (value / 1_000_000.0).ToString("0.##") + "M";
            return (value / 1_000_000_000.0).ToString("0.##") + "B";
        }
    }
}

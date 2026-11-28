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

        // Tu Vi mục tiêu quy chuẩn của cả Đại Cảnh Giới (khớp nhịp 720h / 30 ngày ở nhịp chiến đấu chậm rãi mới)
        private static readonly double[] BaseRealmTotalTuVi =
        {
            36000,          // 0: Luyen Khi (~4h o ai 1-1 -> 1-10, ~650 kills/h)
            420000,         // 1: Truc Co (~20h o ai 2-1 -> 2-10)
            3200000,        // 2: Kim Dan (~48h)
            22000000,       // 3: Nguyen Anh (~96h)
            135000000,      // 4: Hoa Than (~144h)
            720000000,      // 5: Luyen Hu (~180h)
            3600000000,     // 6: Hop The -> len Dai Thua (~228h, tich luy 720h)
            22000000000,    // 7: Dai Thua
            120000000000    // 8: Do Kiep
        };

        public static double GetRealmTotalTuVi(int realmIndex)
        {
            return BaseRealmTotalTuVi[Mathf.Clamp(realmIndex, 0, BaseRealmTotalTuVi.Length - 1)];
        }

        // Ty le phan bo 10 tang trong 1 Dai Canh Gioi (Tang 10 = 19% -> tong = 100%)
        // Luyen Khi (R0) nhe nhang hon o T1-T3 (~5 phut len T4 mo Dao Van & Loi Phat), bu sang T7-T9 va giu nguyen T10 = 19%!
        private static readonly double[] LuyenKhiSubStageWeights =
        {
            0.018, 0.026, 0.036, // Tang 1-3: So Ky (~5-6 phut dau game)
            0.065, 0.080, 0.100, // Tang 4-6: Trung Ky
            0.135, 0.165, 0.185, // Tang 7-9: Hau Ky
            0.190                // Tang 10: Dinh Phong Binh Canh (19.0%)
        };

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
            string phase = s <= 3 ? "Sơ Kỳ" : s <= 6 ? "Trung Kỳ" : s < 10 ? "Đỉnh Phong" : "Đỉnh Phong Viên Mãn";
            return $"{rName} Tầng {s} ({phase})";
        }

        public static string GetRealmShortName(int realmIndex, int subStage)
        {
            string rName = RealmNames[Mathf.Clamp(realmIndex, 0, RealmNames.Length - 1)];
            return $"{rName} · T{Mathf.Clamp(subStage, 1, 10)}";
        }

        /// <summary>
        /// Chi yeu cau Dot Pha tai cac moc binh canh: So Ky (T3->T4), Trung Ky (T6->T7), Dinh Phong (T10->Dai Canh Gioi).
        /// Cac tang thuong (1->2, 2->3, 4->5, 5->6, 7->8, 8->9, 9->10) tu dong tang cap khi du 100% Tu Vi.
        /// </summary>
        public static bool RequiresBreakthroughAt(int subStage)
        {
            return subStage == 3 || subStage == 6 || subStage >= 10;
        }

        public static string GetBreakthroughGateTitle(int realmIndex, int subStage)
        {
            string rName = RealmNames[Mathf.Clamp(realmIndex, 0, RealmNames.Length - 1)];
            if (subStage == 3) return $"{rName}: Sơ Kỳ → Trung Kỳ";
            if (subStage == 6) return $"{rName}: Trung Kỳ → Đỉnh Phong";
            if (subStage >= 10)
            {
                string nextR = RealmNames[Mathf.Clamp(realmIndex + 1, 0, RealmNames.Length - 1)];
                return $"Độ Kiếp {rName} → {nextR}";
            }
            return $"{rName} Tầng {subStage}";
        }

        public static double GetRequiredTuVi(int realmIndex, int subStage)
        {
            int r = Mathf.Clamp(realmIndex, 0, BaseRealmTotalTuVi.Length - 1);
            int s = Mathf.Clamp(subStage, 1, 10) - 1;
            double[] weights = r == 0 ? LuyenKhiSubStageWeights : SubStageWeights;
            return Math.Round(BaseRealmTotalTuVi[r] * weights[s]);
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
            return Math.Round(16.0 * Math.Pow(2.25, lv - 1));
        }

        /// <summary>Toc do bay Phi Kiem (u/s) — nhip cham rai ro rang, tang dan theo cap kiem.</summary>
        public static float CalculateSwordSpeed(int level)
        {
            return 3.8f + Mathf.Clamp(level, 1, 10) * 0.42f;
        }

        /// <summary>Thoi gian nghi giua 2 nhat lao kiem (giay) — giam dan khi kiem cap cao hon.</summary>
        public static float CalculateSwordAttackCooldown(int level)
        {
            int lv = Mathf.Clamp(level, 1, 10);
            return Mathf.Clamp(1.18f - (lv - 1) * 0.045f, 0.72f, 1.18f);
        }

        /// <summary>Toc do xoay quy dao ho than (do/giay).</summary>
        public static float CalculateSwordOrbitSpeed(int level)
        {
            int lv = Mathf.Clamp(level, 1, 10);
            return 56f + lv * 3.5f;
        }

        // =========================================================
        // THUAT TOAN CAN BANG QUAI - BOSS - PLAYER & MO KHOA THEO TU VI
        // =========================================================
        private static readonly int[] Act1SubGates = { 1, 1, 1, 2, 3, 4, 5, 6, 7, 7 };
        private static readonly int[] Act23SubGates = { 1, 1, 2, 3, 4, 5, 6, 7, 8, 8 };

        public static void GetStageRequiredRealm(int linearIndex, out int reqRealm, out int reqSubStage)
        {
            int lin = Mathf.Clamp(linearIndex, 0, 119);
            if (lin < 10)
            {
                reqRealm = 0;
                reqSubStage = Act1SubGates[lin];
                return;
            }
            if (lin < 20)
            {
                reqRealm = 1;
                reqSubStage = Act23SubGates[lin - 10];
                return;
            }
            if (lin < 30)
            {
                reqRealm = 2;
                reqSubStage = Act23SubGates[lin - 20];
                return;
            }
            int linearCult = 30 + Mathf.FloorToInt((lin - 30) * (57f / 89f));
            reqRealm = Mathf.Clamp(linearCult / 10, 3, 8);
            reqSubStage = Mathf.Clamp((linearCult % 10) + 1, 1, 10);
        }

        public static bool IsStageRealmMet(int reqRealm, int reqSubStage, int playerRealm, int playerSubStage)
        {
            int reqLin = reqRealm * 10 + Mathf.Clamp(reqSubStage, 1, 10);
            int curLin = playerRealm * 10 + Mathf.Clamp(playerSubStage, 1, 10);
            return curLin >= reqLin;
        }

        public static double EstimateExpectedPlayerDps(int realmIndex, int subStage)
        {
            double prog = Mathf.Clamp(realmIndex, 0, 8) + (Mathf.Clamp(subStage, 1, 10) - 1) / 10.0;
            int slots = GetMaxSwordSlotsForRealm(realmIndex);
            double expectedLv = Math.Min(10.0, 1.0 + prog * 1.02);
            double swordDmg = 16.0 * Math.Pow(2.25, expectedLv - 1.0);
            double swordSpeed = 3.8 + expectedLv * 0.42;
            double swordCd = Math.Max(0.72, 1.18 - (expectedLv - 1.0) * 0.045);
            double cycleSec = (3.6 / swordSpeed) + 0.38 + swordCd;

            double critChance = Math.Min(0.48, 0.08 + prog * 0.045);
            double critDmg = 2.0 + prog * 0.14;
            double critMult = 1.0 + critChance * (critDmg - 1.0);

            double metaDmgMult = 1.0 + prog * 0.38
                + (realmIndex >= 1 ? 0.25 : 0.0)
                + (realmIndex >= 2 ? 0.35 : 0.0)
                + (Math.Pow(1.16, prog) - 1.0);

            double skillMult = (realmIndex == 0 && subStage < 4) ? 1.00
                : realmIndex == 0 ? 1.22
                : realmIndex == 1 ? 1.38
                : realmIndex <= 3 ? 1.55
                : 1.72;

            return (slots * swordDmg / cycleSec) * critMult * metaDmgMult * skillMult;
        }

        public static double EstimateExpectedPlayerHp(int realmIndex, int subStage)
        {
            int r = Mathf.Clamp(realmIndex, 0, 8);
            int s = Mathf.Clamp(subStage, 1, 10);
            double prog = r + (s - 1) / 10.0;
            double baseHp = 220.0 * Math.Pow(2.3, r) + s * 28.0 * Math.Pow(1.8, r);
            double bodyDao = Math.Min(10.0, prog * 1.15) * 140.0 * Math.Pow(1.6, r);
            double pctMult = 1.0 + prog * 0.18 + (r >= 1 ? 0.15 : 0.0) + (r >= 4 ? 0.25 : 0.0);
            double dr = Math.Min(0.48, prog * 0.055);
            return ((baseHp + bodyDao) * pctMult) / Math.Max(0.45, 1.0 - dr);
        }

        public static void ComputeBalancedStageStats(
            int linearIndex,
            bool isBoss,
            out double enemyHp,
            out double enemyDamage,
            out double tuViPerKill,
            out double linhThachPerKill,
            out double recommendedPower)
        {
            int lin = Mathf.Clamp(linearIndex, 0, 119);
            int diffIdx = lin / 30;
            int stageNum = (lin % 10) + 1;
            GetStageRequiredRealm(lin, out int reqR, out int reqS);

            // Target effective stage progress smoothly rises within an act even across stages sharing a gate
            int evalRealm = reqR;
            int evalSub = Mathf.Clamp(reqS + (isBoss ? 1 : 0), 1, 10);
            double expectedDps = EstimateExpectedPlayerDps(evalRealm, evalSub);
            double expectedEhp = EstimateExpectedPlayerHp(evalRealm, evalSub);

            // Within-act intra-stage pacing multiplier (1-1 = 0.92x -> 1-9 = 1.24x -> 1-10 = 1.28x)
            double stageFactor = 0.92 + (stageNum - 1) * 0.040;
            // Target TTK for normal monster: ~4.0s early -> ~6.2s late (deliberate, weighty pace)
            double targetNormalTtk = 4.3 + diffIdx * 0.30 + (stageNum - 1) * 0.05;
            double normalBaseHp = Math.Max(28.0, Math.Round(expectedDps * targetNormalTtk * stageFactor));

            // Boss tankiness multiplier: 7.0x -> 8.2x normal 10th-stage base (~8.2x-10x X-9, ~33x-45x X-1)
            // Delivers ~38-52s dramatic tanky Boss duel at recommended Realm (beatable inside 60s limit, punishing if under-geared!)
            double bossTankMult = 7.0 + diffIdx * 0.40;
            enemyHp = isBoss ? Math.Round(normalBaseHp * bossTankMult) : normalBaseHp;

            // Monster hit damage: ~3.4% on-curve EHP per slow hit (~2.1s), Boss hits 2.15x harder (~9.2% EHP per 2.4s slam)
            double normalHit = Math.Max(7.0, Math.Round(expectedEhp * 0.034 * stageFactor));
            enemyDamage = isBoss ? Math.Round(normalHit * 2.15) : normalHit;

            // Continuous strictly monotonic Tu Vi & Linh Thach scaling across ALL 120 stages (R0..R8, zero drops across Act boundaries!)
            double cultProg = lin < 30
                ? (lin / 10.0)
                : (3.0 + (lin - 30) * (5.7 / 89.0));
            int rLow = Mathf.Clamp((int)Math.Floor(cultProg), 0, BaseRealmTotalTuVi.Length - 2);
            int rHigh = rLow + 1;
            double frac = Math.Max(0.0, Math.Min(1.0, cultProg - rLow));
            double anchorLow = GetRealmTotalTuVi(rLow) / (ExpectedRealmHours24x7[rLow] * 650.0 * (1.0 + rLow * 0.22));
            double anchorHigh = GetRealmTotalTuVi(rHigh) / (ExpectedRealmHours24x7[rHigh] * 650.0 * (1.0 + rHigh * 0.22));
            double interpTuVi = anchorLow * Math.Pow(anchorHigh / Math.Max(1.0, anchorLow), frac);
            double normalTuVi = Math.Max(11.0, Math.Round(interpTuVi * 0.80));
            tuViPerKill = isBoss ? Math.Round(normalTuVi * 11.0) : normalTuVi;

            // Linh Thach per kill strictly climbs with stage & difficulty
            double normalLt = Math.Max(9.0, Math.Round(normalTuVi * (0.85 + cultProg * 0.095)));
            linhThachPerKill = isBoss ? Math.Round(normalLt * 12.0) : normalLt;

            recommendedPower = Math.Round(expectedDps * (isBoss ? 1.15 : 0.90) * 10.0);
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
                RemakeFeatureId.PvpComingSoon => false,                       // Legacy flag kept for tests
                RemakeFeatureId.PvpArena => linearRealmStage >= 35,           // Nguyen Anh Tang 5 (Luan Kiem Dai)
                RemakeFeatureId.Companions => linearRealmStage >= 17,         // Truc Co Tang 7 (Linh Thu Ho Dao)
                RemakeFeatureId.CodexCollection => true,                      // Do Giam Van Gioi
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
            RemakeFeatureId.PvpArena => "Mở ở Nguyên Anh Tầng 5",
            RemakeFeatureId.Companions => "Mở ở Trúc Cơ Tầng 7",
            RemakeFeatureId.CodexCollection => "Đã mở",
            RemakeFeatureId.OnlineFeatures => "Mở ở Mốc 3 (Online LAN)",
            _ => "Đã mở"
        };

        public static string GetGenderLabel(RemakeCharacterGender g) => g == RemakeCharacterGender.Male ? "Nam Tu" : "Nữ Tu";

        public static string GetPortraitCodexKey(RemakeCharacterGender g) =>
            g == RemakeCharacterGender.Male ? "HUD/portrait_male_cultivator" : "HUD/portrait_female_cultivator";

        public static string GetGenderIconCodexKey(RemakeCharacterGender g) =>
            g == RemakeCharacterGender.Male ? "HUD/icon_gender_male" : "HUD/icon_gender_female";

        public static string GetAvatarFrameCodexKey(int frameId) => frameId switch
        {
            1 => "HUD/frame_sword_saint",
            2 => "HUD/frame_heavenly_dao",
            _ => "HUD/hud_avatar_frame"
        };

        public static (string name, string codexKey, string desc, int reqRealmIndex) GetCompanionSpec(RemakeCompanionId id) => id switch
        {
            RemakeCompanionId.AzureCrane => ("Thanh Loan Tiên Hạc", "Companions/pet_azure_crane", "+12% Tốc Độ Phi Kiếm & +10% Tốc Độ Tu Vi", 1),
            RemakeCompanionId.WhiteFox => ("Bạch Hồ Linh Thú", "Companions/pet_white_fox", "Tự thu thập Kỳ Ngộ Linh Cơ & +15% Linh Thạch rơi", 2),
            RemakeCompanionId.JadeTurtle => ("Huyền Quy Hộ Đạo", "Companions/pet_jade_turtle", "+15% Khí Huyết, +8% Giảm Thương & -10% Phạt Độ Kiếp", 3),
            RemakeCompanionId.ThunderSpirit => ("Cửu Tiêu Lôi Linh", "Companions/pet_thunder_spirit", "+18% Sát Thương Kiếm & +20% Phá Khiên Thủ Lĩnh", 4),
            _ => ("Chưa chọn Linh Thú", "", "Mở khóa Linh Thú Hộ Đạo từ Trúc Cơ Tầng 7", 1)
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

        public static bool IsOffensiveEquipSlot(RemakeEquipSlot slot) =>
            slot == RemakeEquipSlot.Crown || slot == RemakeEquipSlot.Bracer || slot == RemakeEquipSlot.Ring;

        /// <summary>
        /// Guaranteed slot enhancement stats applied whenever a slot is enhanced (+1..+N),
        /// ensuring enhancing ANY slot ALWAYS boosts real combat stats & Combat Power (even before rolling ATK/HP substats).
        /// Offensive slots (Crown/Bracer/Ring): Flat Sword ATK + Sword Damage%.
        /// Defensive slots (Robe/Boots/Amulet): Flat Max HP + Max HP% + Damage Reduction%.
        /// </summary>
        public static void GetEquipSlotEnhanceBonus(
            RemakeEquipSlot slot,
            int enhanceLevel,
            out float flatSwordAtk,
            out float swordDamagePct,
            out double flatMaxHp,
            out float maxHpPct,
            out float damageReductionPct)
        {
            flatSwordAtk = 0f;
            swordDamagePct = 0f;
            flatMaxHp = 0.0;
            maxHpPct = 0f;
            damageReductionPct = 0f;

            int lv = Mathf.Max(0, enhanceLevel);
            if (lv <= 0) return;

            if (IsOffensiveEquipSlot(slot))
            {
                flatSwordAtk = lv * (24f + lv * 4f);
                swordDamagePct = lv * 1.5f;
            }
            else
            {
                flatMaxHp = Math.Round(lv * (180.0 + lv * 35.0));
                maxHpPct = lv * 2.0f;
                damageReductionPct = Mathf.Min(12f, lv * 0.6f);
            }
        }

        public static float CalculateEquipItemScore(RemakeEquipmentItem item, int realmIndex = 2, int slotEnhanceLevel = 0)
        {
            int enh = Mathf.Max(0, slotEnhanceLevel);
            float enhSubMult = 1f + enh * 0.08f;
            float enhBaseScore = enh * (28f + enh * 3.5f);

            if (item == null) return Mathf.Round(enhBaseScore);

            float score = (1f + (int)item.rarity * 1.4f) * (1f + item.tier * 0.45f) * 20f;
            if (item.setId != RemakeEquipSet.None) score += 18f;
            if (item.substats != null)
            {
                for (int i = 0; i < item.substats.Count; i++)
                {
                    var st = item.substats[i];
                    float val = st.value * enhSubMult;
                    score += st.statType switch
                    {
                        RemakeEquipStatType.FlatSwordAtk => val * 0.85f,
                        RemakeEquipStatType.SwordDamagePct => val * 3.8f,
                        RemakeEquipStatType.CritChancePct => val * 7.5f,
                        RemakeEquipStatType.MaxHpPct => val * 2.2f,
                        RemakeEquipStatType.TuViGainPct => val * 2.6f,
                        _ => val * 1.5f
                    };
                }
            }
            return Mathf.Round(score + enhBaseScore);
        }

        public static (double hp, double dmg, double rewardLt, int enemyCount) GetDemonTowerFloorSpec(int floor)
        {
            int f = Mathf.Clamp(floor, 1, 100);
            double prog = 1.4 + (f - 1) * (7.4 / 99.0);
            int r = Mathf.Clamp((int)Math.Floor(prog), 1, 8);
            int s = Mathf.Clamp(1 + (int)Math.Floor((prog - r) * 10.0), 1, 10);
            double dps = EstimateExpectedPlayerDps(r, s);
            double ehp = EstimateExpectedPlayerHp(r, s);
            bool isBossFloor = (f % 5 == 0);
            double hp = Math.Round(dps * (isBossFloor ? 24.0 : 5.2));
            double dmg = Math.Round(ehp * (isBossFloor ? 0.085 : 0.036));
            double rewardLt = Math.Round(350.0 * Math.Pow(1.13, f - 1));
            int count = isBossFloor ? 1 : 4; // Floor 5,10,15... is Elite Guardian Boss
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
            int r = Mathf.Clamp(1 + t, 2, 6); // T1=Kim Dan .. T5=Hop The
            double dps = EstimateExpectedPlayerDps(r, 4);
            double ehp = EstimateExpectedPlayerHp(r, 4);
            double hp = Math.Round(dps * 4.8);
            double dmg = Math.Round(ehp * 0.032);
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

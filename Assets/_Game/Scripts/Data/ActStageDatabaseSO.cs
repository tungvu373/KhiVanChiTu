using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    [CreateAssetMenu(fileName = "ActStageDatabase", menuName = "KhiVan/Remake/Act Stage Database")]
    public sealed class ActStageDatabaseSO : ScriptableObject
    {
        [Serializable]
        public sealed class StageSpec
        {
            public RemakeDifficulty difficulty;
            [Range(0, 2)] public int actIndex;
            [Range(1, 10)] public int stageNumber;
            public string stageCode;
            public string stageName;
            public int killsToClearWave = 10;
            public bool isBossStage;
            public int requiredRealmIndex;
            public int requiredSubStage = 1;
            public double recommendedPower;
            public double enemyHp;
            public double enemyDamage;
            public double tuViPerKill;
            public double linhThachPerKill;
            [Range(0f, 0.2f)] public float swordDropRate = 0.02f;
            public int swordDropMinLevel = 1;
            public int swordDropMaxLevel = 1;
            [Range(0f, 1f)] public float herbDropRate = 0.22f;
            public int herbTier = 1;
            public RemakeRarity maxEquipmentRarity = RemakeRarity.Pham;
            public MonsterDefinitionSO[] enemyPool;
            public MonsterDefinitionSO bossMonster;

            public int LinearIndex => (int)difficulty * 30 + actIndex * 10 + (stageNumber - 1);
            public int RequiredLinearCultivation => requiredRealmIndex * 10 + Mathf.Clamp(requiredSubStage, 1, 10);
            public bool IsActImplementedInM1 => difficulty == RemakeDifficulty.PhamGioi && actIndex == 0;
        }

        [SerializeField] private List<StageSpec> stages = new List<StageSpec>();
        public IReadOnlyList<StageSpec> Stages => stages;

        public void SetStages(List<StageSpec> generated)
        {
            stages = generated ?? new List<StageSpec>();
        }

        public StageSpec GetStage(RemakeDifficulty difficulty, int actIndex, int stageNumber)
        {
            int linear = Mathf.Clamp((int)difficulty * 30 + Mathf.Clamp(actIndex, 0, 2) * 10 + (Mathf.Clamp(stageNumber, 1, 10) - 1), 0, 119);
            if (stages != null && linear < stages.Count && stages[linear] != null && stages[linear].LinearIndex == linear)
                return stages[linear];

            if (stages != null)
            {
                for (int i = 0; i < stages.Count; i++)
                {
                    if (stages[i] != null && stages[i].LinearIndex == linear)
                        return stages[i];
                }
            }

            return BuildFallbackSpec(difficulty, actIndex, stageNumber);
        }

        public StageSpec GetStageByLinear(int linearIndex)
        {
            int clamped = Mathf.Clamp(linearIndex, 0, 119);
            int diff = clamped / 30;
            int rem = clamped % 30;
            int act = rem / 10;
            int stg = rem % 10 + 1;
            return GetStage((RemakeDifficulty)diff, act, stg);
        }

        public static string GetDifficultyLabel(RemakeDifficulty diff) => diff switch
        {
            RemakeDifficulty.PhamGioi => "Phàm Giới",
            RemakeDifficulty.YeuVuc => "Yêu Vực",
            RemakeDifficulty.MaUyen => "Ma Uyên",
            RemakeDifficulty.TienKiep => "Tiên Kiếp",
            _ => "Phàm Giới"
        };

        public static string GetActLabel(int actIndex) => actIndex switch
        {
            0 => "Vực 1: Đồng Cỏ Thanh Vân",
            1 => "Vực 2: Hoang Mạc Nắng Vàng",
            2 => "Vực 3: Tiên Sơn Bạch Ngọc",
            _ => "Vực 1: Đồng Cỏ Thanh Vân"
        };

        public static string GetHerbName(int tier) => tier switch
        {
            1 => "Linh Chi Thảo",
            2 => "Ngưng Khí Thảo",
            3 => "Băng Tuyết Liên",
            4 => "Long Huyết Thảo",
            5 => "Phượng Hoàng Chi",
            _ => "Tiên Lộ Thanh Liên"
        };

        public static StageSpec BuildFallbackSpec(RemakeDifficulty difficulty, int actIndex, int stageNumber)
        {
            int d = Mathf.Clamp((int)difficulty, 0, 3);
            int a = Mathf.Clamp(actIndex, 0, 2);
            int s = Mathf.Clamp(stageNumber, 1, 10);
            int linear = d * 30 + a * 10 + (s - 1);

            bool isBoss = s == 10;
            CultivationTable.GetStageRequiredRealm(linear, out int reqRealm, out int reqSub);
            CultivationTable.ComputeBalancedStageStats(
                linear, isBoss,
                out double hp, out double dmg, out double tuVi, out double linhThach, out double recPower);

            return new StageSpec
            {
                difficulty = (RemakeDifficulty)d,
                actIndex = a,
                stageNumber = s,
                stageCode = $"{a + 1}-{s}",
                stageName = isBoss ? $"Thủ Lĩnh {GetActLabel(a)} ({a + 1}-10)" : $"{GetActLabel(a)} · Ải {a + 1}-{s}",
                killsToClearWave = isBoss ? 1 : 10,
                isBossStage = isBoss,
                requiredRealmIndex = reqRealm,
                requiredSubStage = reqSub,
                recommendedPower = recPower,
                enemyHp = hp,
                enemyDamage = dmg,
                tuViPerKill = tuVi,
                linhThachPerKill = linhThach,
                swordDropRate = Mathf.Clamp(0.015f + (s - 1) * 0.0015f + d * 0.001f, 0.015f, 0.03f),
                swordDropMinLevel = Mathf.Clamp(1 + d, 1, 4),
                swordDropMaxLevel = Mathf.Clamp(1 + d + (a >= 1 ? 1 : 0), 1, 5),
                herbDropRate = isBoss ? 1.0f : Mathf.Clamp(0.20f + s * 0.012f, 0.20f, 0.35f),
                herbTier = Mathf.Clamp(1 + d + (a >= 2 ? 1 : 0), 1, 6),
                maxEquipmentRarity = (RemakeRarity)Mathf.Clamp(d + (s >= 7 ? 1 : 0), 0, 4)
            };
        }
    }
}

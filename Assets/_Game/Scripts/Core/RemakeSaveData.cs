using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    [Serializable]
    public sealed class RemakeGraphicsSettingsData
    {
        public RemakeGraphicsPreset preset = RemakeGraphicsPreset.Medium;
        [Range(0, 2)] public int renderScaleIndex = 1; // 0=70%, 1=85%, 2=100%
        public RemakeShadowMode shadowMode = RemakeShadowMode.Low;
        public RemakeVfxDensity vfxDensity = RemakeVfxDensity.Full;
        public int targetFps = 60; // 30 or 60
        public bool showFpsCounter = true;
        public bool afkDimMode = false;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        [Range(0f, 1f)] public float masterVolume = 1.0f;
        [Range(0f, 1f)] public float musicVolume = 0.70f;
        [Range(0f, 1f)] public float ambienceVolume = 0.65f;
        [Range(0f, 1f)] public float voiceVolume = 0.95f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        public bool audioVolumeInitialized = true;
        public bool extendedAudioInitialized = true;
    }

    [Serializable]
    public sealed class RemakeDaoVanSaveEntry
    {
        public string nodeId;
        public int level;
    }

    [Serializable]
    public sealed class RemakePillStackEntry
    {
        public int realmTarget; // 0 = Truc Co Dan, 1 = Ket Kim Dan...
        public RemakePillQuality quality;
        public int count;
    }

    [Serializable]
    public sealed class RemakeEquipSubstatEntry
    {
        public RemakeEquipStatType statType;
        public float value;
    }

    [Serializable]
    public sealed class RemakeEquipmentItem
    {
        public string uid;
        public RemakeEquipSlot slot;
        public RemakeRarity rarity;
        public RemakeEquipSet setId;
        public int tier = 1;
        public bool isLocked = false;
        public List<RemakeEquipSubstatEntry> substats = new List<RemakeEquipSubstatEntry>();
    }

    [Serializable]
    public sealed class RemakeSaveData
    {
        public int schemaVersion = 1;
        public string daoName = "";
        public RemakeCharacterGender gender = RemakeCharacterGender.Female;
        public bool hasCreatedCharacter = false;
        public long createdUtcTicks = 0;
        public long lastTickUtcTicks = 0;
        public long lastSavedUtcTicks = 0;

        // Cultivation & Breakthrough
        public int realmIndex = 0;
        public int subStage = 1;
        public double tuVi = 0;
        public int consecutiveBreakthroughFails = 0;
        public long mindCalmResetUtcTicks = 0;
        public float pendingTribulationSeconds;
        public float pendingTribulationChance;
        public bool pendingTribulationSuccess;
        public int pendingTribulationPillQuality = -1;

        // Currencies & Materials
        public double linhThach = 100;
        public int coDuyen = 0;
        public int ore = 0;
        public int herbDust = 0;
        public int daoPoints = 1;
        public int alchemistRank = 1;
        public int alchemistXp = 0;
        public int[] herbsByTier = new int[6]; // Tier 1..6

        // Campaign
        public int activeDifficulty = 0;
        public int activeAct = 0;
        public int activeStage = 1;
        public int highestUnlockedLinearStage = 0; // 0..119
        public int stageWaveKills = 0;
        public bool autoChallengeBoss = true;
        public double measuredKillsPerSec = 0.45;
        public double playerHpRatio = 1.0;

        // Swords (Kiem Lo)
        public List<int> swordInventoryLevels = new List<int>();
        public List<int> equippedSwordLevels = new List<int>();
        public float[] swordPityByLevel = new float[10];
        public int swordStyle = 0;

        // Dao Van Starter (Moc 1: Dao Tam + 5 Nhanh goc)
        public List<RemakeDaoVanSaveEntry> daoVanNodes = new List<RemakeDaoVanSaveEntry>();

        // Combat Skills (1..4)
        public int loiPhatLevel = 1;
        public bool loiPhatAuto = true;
        public int yinYangLevel = 1;
        public bool yinYangAuto = true;
        public int swordRainLevel = 1;
        public bool swordRainAuto = true;
        public int palmLevel = 1;
        public bool palmAuto = true;

        // Alchemy Pills, Flame & Support Items
        public List<RemakePillStackEntry> pills = new List<RemakePillStackEntry>();
        public float pendingBrewSeconds;
        public int pendingBrewTargetRealm;
        public int spiritFlameTier = 1; // 1..4 (Pham -> Tien Thien Hoa)
        public int spiritFlameXp = 0;
        public int forgeTalismanCount = 1; // +10% ghep kiem
        public int reforgeStoneCount = 1;  // Tay luyen trang bi
        public int skillScrollCount = 1;   // Tan quyen cong phap
        public int calmIncenseCount = 0;   // Tinh tam huong

        // Equipment & Sets (6 slots)
        public int[] equipSlotEnhanceLevels = new int[6];
        public List<RemakeEquipmentItem> equippedGear = new List<RemakeEquipmentItem>();
        public List<RemakeEquipmentItem> equipmentBag = new List<RemakeEquipmentItem>();
        public int autoDismantleMaxRarity = -1; // -1=Off, 0=Pham, 1=<=Linh, 2=<=Huyen

        // Formations (6 Dai Tran Phap + Sub Slot)
        public int activeMainFormation = -1; // -1=None, 0..5
        public int activeSubFormation = -1;  // -1=None, 0..5 (50% effect at Hoa Than)

        // Quests, 2-Beat Tutorial, Progressive Feature Unlock Tutorials, 30-Day Pioneer Diary & Online
        public int tutorialStep = 0; // 0: Kill 5, 1: OpenBagua/KiemLo, 2: MergeLv2, 3: ClaimQuest/Done
        public bool tutorialCompleted = false;
        public bool tutorialGuaranteedSwordDropped = false;
        public int completedFeatureTutorialsMask = 0; // Bitmask per RemakeFeatureId for guided unlock pointers
        public int mainQuestIndex = 0;
        public int mainQuestProgress = 0;
        public int totalKills = 0;
        public int totalMerges = 0;
        public bool bossAct1Cleared = false;
        public int claimedPioneerMilestonesMask = 0;
        public string nakamaUserId = "";
        public int hoPhapBonusSteps = 0; // +1..3 (+1% each)
        public long lastTruyenCongDayUtc = 0;

        // V2 Expansion: Demon Tower (100 floors), Secret Realm (3 gates/day) & Ky Ngo
        public int demonTowerHighestFloor = 0;
        public bool autoClimbDemonTower = true;
        public long lastTowerSalaryDayUtc = 0;
        public int secretRealmDailyRemaining = 3;
        public long lastSecretRealmResetDayUtc = 0;
        public int[] secretRealmClearedTier = new int[3];
        public long tuViDoubleBuffEndUtcTicks = 0;
        public int kyNgoClaimedCount = 0;

        // V3 Expansion: Companions, Codex Collection, Async PvP & Avatar Frames
        public int activeCompanionId = 0; // RemakeCompanionId
        public int unlockedCompanionsMask = 0;
        public int codexSwordsMask = 1;   // Bit (lv-1) unlocked
        public int codexBossesMask = 0;   // Bit actIndex unlocked
        public int codexPillsMask = 0;    // Bit realmTarget unlocked
        public int codexClaimedMask = 0;  // Claimed milestone tiers (bits 0..7)
        public int pvpWins = 0;
        public int pvpDailyRemaining = 5;
        public long lastPvpResetDayUtc = 0;
        public int swordPrestigePoints = 0;
        public int equippedAvatarFrame = 0; // 0=Default, 1=SwordSaint, 2=HeavenlyDao
        public int unlockedFramesMask = 1;

        // Graphics & Audio Settings
        public RemakeGraphicsSettingsData graphics = new RemakeGraphicsSettingsData();

        public static RemakeSaveData CreateDefault(bool isTouchPlatform)
        {
            long now = DateTime.UtcNow.Ticks;
            var save = new RemakeSaveData
            {
                schemaVersion = SaveService.CurrentSchemaVersion,
                daoName = "",
                hasCreatedCharacter = false,
                createdUtcTicks = now,
                lastTickUtcTicks = now,
                lastSavedUtcTicks = now,
                realmIndex = 0,
                subStage = 1,
                tuVi = 0,
                linhThach = 120,
                daoPoints = 1,
                activeDifficulty = 0,
                activeAct = 0,
                activeStage = 1,
                highestUnlockedLinearStage = 0,
                measuredKillsPerSec = 0.45
            };

            save.equippedSwordLevels.Add(1); // Tang 1 Kiem Lv.1 khoi dau
            save.graphics = new RemakeGraphicsSettingsData
            {
                preset = isTouchPlatform ? RemakeGraphicsPreset.Low : RemakeGraphicsPreset.Medium,
                renderScaleIndex = isTouchPlatform ? 0 : 1,
                shadowMode = isTouchPlatform ? RemakeShadowMode.Off : RemakeShadowMode.Low,
                vfxDensity = isTouchPlatform ? RemakeVfxDensity.Minimal : RemakeVfxDensity.Full,
                targetFps = isTouchPlatform ? 30 : 60,
                showFpsCounter = true,
                afkDimMode = false,
                soundEnabled = true,
                musicEnabled = true
            };

            return save;
        }

        public static RemakeSaveData CreateFullTestSave(bool isTouchPlatform, RemakeGraphicsSettingsData preserveGraphics = null)
        {
            var save = CreateDefault(isTouchPlatform);
            if (preserveGraphics != null) save.graphics = preserveGraphics;

            save.schemaVersion = SaveService.CurrentSchemaVersion;
            save.daoName = "Thiên Đạo Test";
            save.hasCreatedCharacter = true;
            save.tutorialCompleted = true;
            save.tutorialStep = 3;
            save.tutorialGuaranteedSwordDropped = true;
            save.completedFeatureTutorialsMask = 0x7FFFFFFF;

            // Dai Thua T3 (So Ky Binh Canh) unlocks 100% ALL features, 6 formations + sub-slot, all 4 skills, Tower & Secret Realm
            save.realmIndex = 7;
            save.subStage = 3;
            save.tuVi = CultivationTable.GetRequiredTuVi(7, 3);
            save.consecutiveBreakthroughFails = 1;

            save.linhThach = 9_999_999;
            save.coDuyen = 5_000;
            save.ore = 5_000;
            save.herbDust = 2_500;
            save.daoPoints = 60;
            save.alchemistRank = 5;
            save.alchemistXp = 950;
            save.spiritFlameTier = 3;
            save.spiritFlameXp = 120;
            save.herbsByTier = new[] { 80, 80, 80, 80, 80, 80 };

            save.forgeTalismanCount = 30;
            save.reforgeStoneCount = 30;
            save.skillScrollCount = 30;
            save.calmIncenseCount = 20;

            save.equippedSwordLevels.Clear();
            save.equippedSwordLevels.AddRange(new[] { 8, 7, 7, 6, 6, 5, 5, 4 });
            save.swordInventoryLevels.Clear();
            save.swordInventoryLevels.AddRange(new[] { 1, 1, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 });
            save.swordPityByLevel[5] = 0.25f;

            save.daoVanNodes.Clear();
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "core_daotam", level = 3 });
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "branch_sword", level = 4 });
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "branch_body", level = 5 });
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "branch_cultivation", level = 4 });
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "branch_wealth", level = 3 });
            save.daoVanNodes.Add(new RemakeDaoVanSaveEntry { nodeId = "branch_arts", level = 3 });

            save.pills.Clear();
            foreach (int r in new[] { 0, 1, 2, 7 })
            {
                save.pills.Add(new RemakePillStackEntry { realmTarget = r, quality = RemakePillQuality.PheDan, count = 6 });
                save.pills.Add(new RemakePillStackEntry { realmTarget = r, quality = RemakePillQuality.HaPham, count = 6 });
                save.pills.Add(new RemakePillStackEntry { realmTarget = r, quality = RemakePillQuality.TrungPham, count = 4 });
                save.pills.Add(new RemakePillStackEntry { realmTarget = r, quality = RemakePillQuality.ThuongPham, count = 3 });
                save.pills.Add(new RemakePillStackEntry { realmTarget = r, quality = RemakePillQuality.CucPham, count = 2 });
            }

            save.equipSlotEnhanceLevels = new[] { 5, 5, 4, 4, 3, 3 };
            save.equippedGear.Clear();
            RemakeEquipSlot[] allSlots =
            {
                RemakeEquipSlot.Robe, RemakeEquipSlot.Crown, RemakeEquipSlot.Boots,
                RemakeEquipSlot.Amulet, RemakeEquipSlot.Bracer, RemakeEquipSlot.Ring
            };
            for (int i = 0; i < allSlots.Length; i++)
            {
                save.equippedGear.Add(new RemakeEquipmentItem
                {
                    uid = $"test-eq-{i}",
                    slot = allSlots[i],
                    rarity = i < 4 ? RemakeRarity.Dia : RemakeRarity.Huyen,
                    setId = i < 4 ? RemakeEquipSet.ThanhVan : RemakeEquipSet.DanTong,
                    tier = 5,
                    substats = new List<RemakeEquipSubstatEntry>
                    {
                        new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.FlatSwordAtk, value = 240f + i * 25f },
                        new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.CritChancePct, value = 4.0f + i * 0.5f }
                    }
                });
            }

            save.equipmentBag.Clear();
            save.equipmentBag.Add(new RemakeEquipmentItem
            {
                uid = "test-bag-thien",
                slot = RemakeEquipSlot.Robe,
                rarity = RemakeRarity.Thien,
                setId = RemakeEquipSet.ThanhVan,
                tier = 6,
                substats = new List<RemakeEquipSubstatEntry>
                {
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.FlatSwordAtk, value = 520f },
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.SwordDamagePct, value = 24f },
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.CritChancePct, value = 7.5f }
                }
            });
            save.equipmentBag.Add(new RemakeEquipmentItem
            {
                uid = "test-bag-locked",
                slot = RemakeEquipSlot.Ring,
                rarity = RemakeRarity.Dia,
                setId = RemakeEquipSet.HuyenVu,
                tier = 5,
                isLocked = true,
                substats = new List<RemakeEquipSubstatEntry>
                {
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.MaxHpPct, value = 18f },
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.TuViGainPct, value = 16f }
                }
            });
            save.equipmentBag.Add(new RemakeEquipmentItem
            {
                uid = "test-bag-low",
                slot = RemakeEquipSlot.Boots,
                rarity = RemakeRarity.Linh,
                setId = RemakeEquipSet.None,
                tier = 2,
                substats = new List<RemakeEquipSubstatEntry>
                {
                    new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.LinhThachPct, value = 12f }
                }
            });

            save.activeMainFormation = 4; // Van Kiem Quy Tong Tran
            save.activeSubFormation = 0;  // Cuu Thien Loi Tran
            save.loiPhatLevel = 10;
            save.yinYangLevel = 8;
            save.swordRainLevel = 6;
            save.palmLevel = 4;

            save.activeDifficulty = 0;
            save.activeAct = 0;
            save.activeStage = 9;
            save.highestUnlockedLinearStage = 119; // All 120 stages unlocked across 4 difficulties x 3 acts
            save.autoChallengeBoss = false;

            save.demonTowerHighestFloor = 10;
            save.autoClimbDemonTower = true;
            save.secretRealmDailyRemaining = 3;
            save.secretRealmClearedTier = new[] { 2, 2, 1 };
            save.activeCompanionId = (int)RemakeCompanionId.AzureCrane;
            save.unlockedCompanionsMask = (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4);
            save.codexSwordsMask = 0b11111111; // Lv.1..8
            save.codexBossesMask = 0b111;
            save.codexPillsMask = 0b10000111;
            save.pvpDailyRemaining = 5;
            save.swordPrestigePoints = 450;
            save.unlockedFramesMask = 0b111;
            save.totalKills = 50;
            save.mainQuestIndex = 6;
            save.claimedPioneerMilestonesMask = 0;

            return save;
        }
    }
}

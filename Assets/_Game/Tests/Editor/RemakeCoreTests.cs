using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KhiVanChiTu.Remake.Tests
{
    public class RemakeCoreTests
    {
        private string _testProfile = "qa-remake-unit";

        [SetUp]
        public void SetUp()
        {
            SaveService.SetProfileOverride(_testProfile);
            SaveService.DeleteSaveFilesForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            SaveService.DeleteSaveFilesForTesting();
            SaveService.SetProfileOverride(null);
        }

        [Test]
        public void ExpectedHoursSumTo720HoursAtDaiThua()
        {
            float sum = 0f;
            for (int i = 0; i <= 6; i++)
                sum += CultivationTable.ExpectedRealmHours24x7[i];
            Assert.That(sum, Is.EqualTo(720f).Within(0.01f));
        }

        [Test]
        public void SubStage10IsRoughly19PercentOfRealmTuVi()
        {
            for (int r = 0; r < 8; r++)
            {
                double total = 0;
                for (int s = 1; s <= 10; s++) total += CultivationTable.GetRequiredTuVi(r, s);
                double t10 = CultivationTable.GetRequiredTuVi(r, 10);
                Assert.That(t10 / total, Is.InRange(0.185, 0.195), $"Realm {r}");
            }
        }

        [Test]
        public void BreakthroughFailurePenaltyEscalates50To75To100()
        {
            Assert.That(CultivationTable.GetFailurePenaltyPercent(0, RemakePillQuality.PheDan), Is.EqualTo(0.50f).Within(0.001f));
            Assert.That(CultivationTable.GetFailurePenaltyPercent(1, RemakePillQuality.PheDan), Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(CultivationTable.GetFailurePenaltyPercent(2, RemakePillQuality.PheDan), Is.EqualTo(1.00f).Within(0.001f));
            Assert.That(CultivationTable.GetFailurePenaltyPercent(0, RemakePillQuality.CucPham), Is.EqualTo(0.40f).Within(0.001f));
        }

        [Test]
        public void ProgressiveFeatureUnlocksMatchV2Spec()
        {
            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, 0, 3));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, 0, 4));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillLoiPhat, 0, 4));
            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, 0, 6));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, 0, 7));
            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, 0, 10));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, 1, 1));
        }

        [Test]
        public void StageDatabaseCovers120StagesWithUpwardScaling()
        {
            var prev = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.PhamGioi, 0, 1);
            var boss = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.PhamGioi, 0, 10);
            Assert.IsTrue(boss.isBossStage);
            Assert.Greater(boss.enemyHp, prev.enemyHp * 15);
            Assert.Greater(boss.tuViPerKill, prev.tuViPerKill * 10);
            Assert.That(ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.TienKiep, 2, 10).LinearIndex, Is.EqualTo(119));
        }

        [Test]
        public void SaveServiceNeverTouchesLegacySaveFile()
        {
            var save = SaveService.Current;
            save.daoName = "UnitTestCultivator";
            save.linhThach = 777;
            SaveService.Save(save);

            string path = SaveService.GetSaveFilePath();
            Assert.IsTrue(File.Exists(path));
            Assert.IsFalse(Path.GetFileName(path).Contains("KhiVanChiTu_SaveData.json"));
            Assert.IsTrue(Path.GetFileName(path).StartsWith(_testProfile));

            SaveService.SetProfileOverride(_testProfile);
            var reloaded = SaveService.Load();
            Assert.That(reloaded.daoName, Is.EqualTo("UnitTestCultivator"));
            Assert.That(reloaded.linhThach, Is.EqualTo(777));
        }

        [Test]
        public void CodexArtRegistryContains103CatalogItemsAndReusedSprites()
        {
            var reg = CodexArtLoader.Registry;
            Assert.IsNotNull(reg, "RemakeArtRegistry should exist in Resources");
            Assert.That(reg.CodexCatalog.Count, Is.EqualTo(103));
            Assert.GreaterOrEqual(reg.ReusedSprites.Count, 40);
            Assert.IsNotNull(CodexArtLoader.LoadCodexTexture("HUD/hud_toggle_bagua"));
            Assert.IsNotNull(CodexArtLoader.LoadCodexTexture("HUD/nav_immortal_gate"));
            Assert.IsNotNull(CodexArtLoader.LoadCodexTexture("Stages/stage_sword_forge"));
        }

        [Test]
        public void Milestone2Systems_AlchemyFormationsEquipmentAndSkillsWork()
        {
            var go = new GameObject("TestEngineM2");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 4; // Hoa Than -> Unlocks Equipment, Formation SubSlot, all 4 Skills!
                save.subStage = 1;
                save.linhThach = 500000;
                save.ore = 100;
                save.reforgeStoneCount = 5;
                save.skillScrollCount = 3;
                save.calmIncenseCount = 2;
                save.herbsByTier[0] = 20;
                save.herbsByTier[2] = 20;

                // Test Alchemy: 3 Ha -> 1 Trung -> Flame Sacrifice
                save.pills.Add(new RemakePillStackEntry { realmTarget = 0, quality = RemakePillQuality.HaPham, count = 3 });
                save.pills.Add(new RemakePillStackEntry { realmTarget = 0, quality = RemakePillQuality.PheDan, count = 6 });
                Assert.IsTrue(engine.TryMergePills(0, RemakePillQuality.HaPham));
                Assert.That(engine.GetPillCount(0, RemakePillQuality.TrungPham), Is.EqualTo(1));
                Assert.IsTrue(engine.TrySacrificePillToFlame(0, RemakePillQuality.PheDan));
                Assert.Greater(save.spiritFlameXp, 0);

                // Test Formations: Main + 50% SubSlot
                Assert.IsTrue(engine.TryActivateFormation(0, false));
                Assert.IsTrue(engine.TryActivateFormation(1, true));
                Assert.IsFalse(engine.TryActivateFormation(0, true)); // Same slot blocked
                Assert.That(save.activeMainFormation, Is.EqualTo(0));
                Assert.That(save.activeSubFormation, Is.EqualTo(1));

                // Test Equipment Equip + Enhance + Reforge
                var item = new RemakeEquipmentItem
                {
                    uid = "eq_test_1",
                    slot = RemakeEquipSlot.Robe,
                    rarity = RemakeRarity.Huyen,
                    setId = RemakeEquipSet.ThanhVan,
                    tier = 2
                };
                item.substats.Add(new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.SwordDamagePct, value = 20f });
                save.equipmentBag.Add(item);
                Assert.IsTrue(engine.TryEquipGear("eq_test_1"));
                Assert.IsNotNull(engine.GetEquippedItem(RemakeEquipSlot.Robe));
                Assert.IsTrue(engine.TryEnhanceEquipSlot((int)RemakeEquipSlot.Robe));
                Assert.That(save.equipSlotEnhanceLevels[0], Is.EqualTo(1));
                Assert.IsTrue(engine.TryReforgeGear("eq_test_1"));

                // Test 4 Skills Unlock & Scroll Upgrade + Calm Incense
                for (int s = 0; s < 4; s++) Assert.IsTrue(engine.IsSkillUnlocked(s), $"Skill {s}");
                Assert.IsTrue(engine.TryUpgradeSkill(3, true));
                Assert.That(save.palmLevel, Is.EqualTo(2));
                save.consecutiveBreakthroughFails = 2;
                Assert.IsTrue(engine.TryUseCalmIncense());
                Assert.That(save.consecutiveBreakthroughFails, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

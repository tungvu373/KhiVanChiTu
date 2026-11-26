using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace KhiVanChiTu.Remake.Tests
{
    public sealed class RemakeV2OverhaulTests
    {
        private const string TestProfile = "nunit-v2-isolated";

        [SetUp]
        public void SetUp()
        {
            SaveService.SetProfileOverride(TestProfile);
            SaveService.DeleteSaveFilesForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            SaveService.DeleteSaveFilesForTesting();
            SaveService.SetProfileOverride(null);
        }

        [Test]
        public void V2FeatureUnlocks_TowerSecretRealmAndComingSoonPvp()
        {
            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, 1, 4));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, 1, 5));

            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, 1, 9));
            Assert.IsTrue(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, 2, 1));

            Assert.IsFalse(CultivationTable.IsFeatureUnlocked(RemakeFeatureId.PvpComingSoon, 8, 10));
        }

        [Test]
        public void SaveService_MigratesV1SaveToV3WithV2Defaults()
        {
            var legacy = new RemakeSaveData
            {
                schemaVersion = 1,
                daoName = "CoTuSi",
                hasCreatedCharacter = true,
                linhThach = 7777,
                secretRealmDailyRemaining = 0,
                secretRealmClearedTier = null
            };
            File.WriteAllText(SaveService.GetSaveFilePath(), JsonUtility.ToJson(legacy));

            var loaded = SaveService.Load();
            Assert.That(loaded.schemaVersion, Is.EqualTo(3));
            Assert.That(loaded.daoName, Is.EqualTo("CoTuSi"));
            Assert.That(loaded.linhThach, Is.EqualTo(7777));
            Assert.IsTrue(loaded.autoChallengeBoss);
            Assert.NotNull(loaded.secretRealmClearedTier);
            Assert.That(loaded.secretRealmClearedTier.Length, Is.EqualTo(3));
            Assert.That(loaded.secretRealmDailyRemaining, Is.EqualTo(3));
        }

        [Test]
        public void QuickMergeSwords_AutoEquipLock_And_BossHoldWorkCleanly()
        {
            var go = new GameObject("TestEngineV2_QoL");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 2; // Kim Dan -> allows 3 sword slots & Equipment
                save.subStage = 1;
                save.linhThach = 200000;
                save.tutorialCompleted = true;
                save.equippedSwordLevels.Clear();
                save.equippedSwordLevels.Add(1);
                save.swordInventoryLevels.Clear();
                save.swordInventoryLevels.Add(1);
                save.swordInventoryLevels.Add(1);
                save.swordInventoryLevels.Add(1);

                int merged = engine.TryQuickMergeSwords(3);
                Assert.GreaterOrEqual(merged, 2, "Quick merge should fuse available Lv.1-2 pairs cleanly");

                // Gear Lock & Auto-Equip Best
                var weak = new RemakeEquipmentItem { uid = "weak-robe", slot = RemakeEquipSlot.Robe, rarity = RemakeRarity.Pham, tier = 1 };
                var strong = new RemakeEquipmentItem { uid = "strong-robe", slot = RemakeEquipSlot.Robe, rarity = RemakeRarity.Dia, tier = 3 };
                save.equippedGear.Clear();
                save.equippedGear.Add(weak);
                save.equipmentBag.Clear();
                save.equipmentBag.Add(strong);

                int eqCount = engine.TryAutoEquipBestGear();
                Assert.That(eqCount, Is.EqualTo(1));
                Assert.That(engine.GetEquippedItem(RemakeEquipSlot.Robe).uid, Is.EqualTo("strong-robe"));

                // Locked bag item cannot be dismantled
                Assert.IsTrue(engine.TryToggleGearLock("weak-robe"));
                Assert.IsTrue(weak.isLocked);
                Assert.IsFalse(engine.TryDismantleGear("weak-robe"));
                Assert.IsTrue(engine.TryToggleGearLock("weak-robe"));
                Assert.IsTrue(engine.TryDismantleGear("weak-robe"));

                // Manual Boss challenge from safe X-9 hold
                save.activeDifficulty = 0;
                save.activeAct = 0;
                save.activeStage = 9;
                save.highestUnlockedLinearStage = 9;
                engine.SetAutoChallengeBoss(false);
                Assert.IsFalse(save.autoChallengeBoss);
                Assert.IsTrue(engine.CanChallengeActBossNow());
                Assert.IsTrue(engine.TryChallengeCurrentActBoss());
                Assert.That(save.activeStage, Is.EqualTo(10));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DemonTower_And_SecretRealm_ModeSwitchAndSweepWork()
        {
            var go = new GameObject("TestEngineV2_ImmortalGate");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 2; // Kim Dan -> unlocks Tower + Secret Realm
                save.subStage = 1;
                save.secretRealmDailyRemaining = 3;

                // Demon Tower start & exit
                Assert.IsTrue(engine.TryStartDemonTowerFloor(1));
                Assert.That(engine.ActiveArenaMode, Is.EqualTo(RemakeArenaMode.DemonTower));
                Assert.That(engine.ActiveChallengeFloor, Is.EqualTo(1));
                engine.ExitChallengeToCampaign();
                Assert.That(engine.ActiveArenaMode, Is.EqualTo(RemakeArenaMode.Campaign));

                // Tower salary claim requires >= floor 1 cleared
                Assert.IsFalse(engine.TryClaimTowerDailySalary());
                save.demonTowerHighestFloor = 5;
                double beforeLt = save.linhThach;
                Assert.IsTrue(engine.TryClaimTowerDailySalary());
                Assert.Greater(save.linhThach, beforeLt);

                // Secret Realm enter & sweep gate check
                Assert.IsFalse(engine.TrySweepSecretRealm(RemakeSecretRealmKind.SwordTomb, 1));
                Assert.IsTrue(engine.TryEnterSecretRealm(RemakeSecretRealmKind.SwordTomb, 1));
                Assert.That(engine.ActiveArenaMode, Is.EqualTo(RemakeArenaMode.SecretRealm));
                Assert.That(save.secretRealmDailyRemaining, Is.EqualTo(2));
                engine.ExitChallengeToCampaign();

                save.secretRealmClearedTier[(int)RemakeSecretRealmKind.SwordTomb] = 2;
                int prevBag = save.swordInventoryLevels.Count;
                Assert.IsTrue(engine.TrySweepSecretRealm(RemakeSecretRealmKind.SwordTomb, 2));
                Assert.Greater(save.swordInventoryLevels.Count, prevBag);
                Assert.That(save.secretRealmDailyRemaining, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TestAllAccount_UnlocksAllFeaturesAndIsolatesFilePath()
        {
            var full = RemakeSaveData.CreateFullTestSave(false);
            Assert.IsTrue(full.hasCreatedCharacter);
            Assert.That(full.daoName, Is.EqualTo("Thiên Đạo Test"));
            Assert.That(full.realmIndex, Is.EqualTo(7));
            Assert.That(full.highestUnlockedLinearStage, Is.EqualTo(119));
            Assert.That(full.equippedSwordLevels.Count, Is.EqualTo(8));
            Assert.That(full.equippedGear.Count, Is.EqualTo(6));
            Assert.GreaterOrEqual(full.equipmentBag.Count, 3);
            Assert.GreaterOrEqual(full.demonTowerHighestFloor, 10);
            Assert.That(full.secretRealmDailyRemaining, Is.EqualTo(3));

            foreach (RemakeFeatureId fid in Enum.GetValues(typeof(RemakeFeatureId)))
            {
                if (fid == RemakeFeatureId.PvpComingSoon) continue;
                Assert.IsTrue(CultivationTable.IsFeatureUnlocked(fid, full.realmIndex, full.subStage), $"Expected unlocked: {fid}");
            }

            SaveService.SetProfileOverride(null);
            string mainPath = SaveService.GetSaveFilePath();
            SaveService.SetProfileOverride(SaveService.TestAllProfileId);
            string testPath = SaveService.GetSaveFilePath();
            Assert.AreNotEqual(mainPath, testPath);
            Assert.IsTrue(SaveService.IsTestAllProfileActive);
        }
    }
}

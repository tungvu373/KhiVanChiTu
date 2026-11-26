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

        [Test]
        public void FifaSwordMerge_MultiFodderProbabilityAndExecutionWork()
        {
            var go = new GameObject("TestEngineV3_FifaSword");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.tutorialCompleted = true;
                save.realmIndex = 4;
                save.subStage = 1;
                save.linhThach = 500000;
                save.equippedSwordLevels.Clear();
                save.equippedSwordLevels.Add(4);
                save.swordInventoryLevels.Clear();
                save.swordInventoryLevels.Add(4);
                save.swordInventoryLevels.Add(4);
                save.swordInventoryLevels.Add(4);
                save.swordInventoryLevels.Add(3);
                save.swordInventoryLevels.Add(2);

                float c1 = engine.ComputeFifaSwordMergeChance(4, new[] { 4 }, false, out _, out _);
                float c3 = engine.ComputeFifaSwordMergeChance(4, new[] { 4, 4, 4 }, false, out _, out _);
                Assert.Greater(c3, c1, "Adding more fodder swords must increase FIFA merge probability");
                Assert.GreaterOrEqual(c3, 0.99f, "3 same-level Lv.4 fodders should reach ~100%");

                bool ok = engine.TryMergeSwordFifa(true, 0, new[] { 0, 1, 2 }, false);
                Assert.IsTrue(ok);
                Assert.That(save.equippedSwordLevels[0], Is.EqualTo(5));
                Assert.That(save.swordInventoryLevels.Count, Is.EqualTo(2), "3 fodders consumed cleanly");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AlchemyBrewTimer_And_ExcessPillSinksWork()
        {
            var go = new GameObject("TestEngineV3_Alchemy");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 2;
                save.subStage = 1;
                save.linhThach = 100000;
                save.tuVi = 0;
                save.herbsByTier[0] = 20;
                save.pills.Clear();

                Assert.Greater(engine.GetAlchemyBrewSuccessChance(0), 0.34f);
                Assert.IsTrue(engine.TryStartBrewPill(0));
                Assert.IsTrue(engine.IsBrewingPill);
                Assert.IsFalse(engine.TryStartBrewPill(0), "Should not allow concurrent brew");

                engine.AdvanceBrewTimer(GameplayEngine.BrewDuration + 0.1f);
                Assert.IsFalse(engine.IsBrewingPill);
                int totalOwned = 0;
                for (int q = 0; q <= 4; q++) totalOwned += engine.GetPillCount(0, (RemakePillQuality)q);
                Assert.GreaterOrEqual(totalOwned, 1, "Brew countdown finishing must grant a pill (or PheDan)");

                // Excess pill sinks: Absorb TuVi & Refine LinhThach
                save.pills.Add(new RemakePillStackEntry { realmTarget = 1, quality = RemakePillQuality.HaPham, count = 10 });
                double ltBefore = save.linhThach;
                double tuViBefore = save.tuVi;
                Assert.IsTrue(engine.TryConsumePillForTuVi(1, RemakePillQuality.HaPham, 2));
                Assert.Greater(save.tuVi, tuViBefore);
                Assert.IsTrue(engine.TryRefinePillToLinhThach(1, RemakePillQuality.HaPham, 3));
                Assert.Greater(save.linhThach, ltBefore);
                Assert.That(engine.GetPillCount(1, RemakePillQuality.HaPham), Is.EqualTo(5));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BreakthroughSeclusion_15sTribulationFires9StrikesAndResolves()
        {
            var go = new GameObject("TestEngineV3_Tribulation");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 0;
                save.subStage = 1;
                save.tuVi = engine.GetRequiredTuViForCurrent();

                int strikeEvents = 0;
                bool resolvedCalled = false;
                engine.OnTribulationStrike += (_, __) => strikeEvents++;
                engine.OnBreakthroughResolved += (_, __) => resolvedCalled = true;

                Assert.IsTrue(engine.TryStartTribulation(null));
                Assert.IsTrue(engine.IsInTribulation);
                Assert.That(engine.TribulationTimeRemaining, Is.EqualTo(GameplayEngine.TribulationDuration).Within(0.01f));

                // Invulnerable while in Tribulation seclusion
                double hpBefore = engine.PlayerCurrentHp;
                engine.OnPlayerDamaged(999999);
                Assert.That(engine.PlayerCurrentHp, Is.EqualTo(hpBefore));

                for (int step = 0; step < 30; step++)
                    engine.AdvanceTribulation(0.55f);

                Assert.IsFalse(engine.IsInTribulation);
                Assert.That(strikeEvents, Is.EqualTo(GameplayEngine.TribulationStrikesTotal));
                Assert.IsTrue(resolvedCalled);
                Assert.That(save.subStage, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DaoVanFogOfWar_HidesDeepLockedNodesUntilParentUnlocked()
        {
            var go = new GameObject("TestEngineV3_DaoVanFog");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 1;
                save.subStage = 1;
                save.daoVanNodes.Clear();

                DaoVanNodeSpec core = null, branchSword = null, deepMastery = null;
                foreach (var n in RemakeCatalogs.StarterDaoVanNodes)
                {
                    if (n.id == "core_daotam") core = n;
                    else if (n.id == "branch_sword") branchSword = n;
                    else if (n.id == "sword_mastery") deepMastery = n;
                }

                Assert.IsTrue(RemakeDrawerViews.IsDaoVanNodeVisible(engine, core));
                Assert.IsFalse(RemakeDrawerViews.IsDaoVanNodeVisible(engine, branchSword));
                Assert.IsFalse(RemakeDrawerViews.IsDaoVanNodeVisible(engine, deepMastery));

                save.daoPoints = 2;
                Assert.IsTrue(engine.TryUpgradeDaoVanNode("core_daotam"));
                Assert.IsTrue(RemakeDrawerViews.IsDaoVanNodeVisible(engine, branchSword), "Direct child reveals once parent unlocked");
                Assert.IsFalse(RemakeDrawerViews.IsDaoVanNodeVisible(engine, deepMastery), "Deep child stays hidden until its own parent unlocks");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HeavenlyTribulationVfxPrefab_LoadsWithZeroShadowsAndPoolsCleanly()
        {
            var prefab = Resources.Load<RemakeTribulationVfx>("VFX/VFX_HeavenlyTribulation");
            Assert.NotNull(prefab, "VFX_HeavenlyTribulation.prefab must exist under Assets/_Game/Resources/VFX/");

            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                Assert.GreaterOrEqual(renderers.Length, 16, "Expected multi-pillar bolts, forks, ground arcs, and Shuriken layers");
                foreach (var r in renderers)
                {
                    Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
                    Assert.IsFalse(r.receiveShadows);
                }
                Assert.That(instance.GetComponentsInChildren<Light>(true).Length, Is.EqualTo(0), "No dynamic PointLights allowed for WebGL/mobile FPS");

                instance.SetSeclusionActive(true, Vector3.zero);
                Assert.IsTrue(instance.IsSeclusionActive);
                instance.PlayStrike(Vector3.zero, 9, 9);
                Assert.IsTrue(instance.IsStrikeVisualActive);
                instance.PlayOutcomeBurst(Vector3.zero, true);
                instance.ClearImmediate();
                Assert.IsFalse(instance.IsSeclusionActive);
                Assert.IsFalse(instance.IsStrikeVisualActive);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance.gameObject);
            }
        }
    }
}

using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
            Assert.That(loaded.schemaVersion, Is.EqualTo(SaveService.CurrentSchemaVersion));
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
                save.swordPityByLevel[0] = 0.25f; // Deterministic 100% Lv.1 merge in CI
                save.swordPityByLevel[1] = 0.25f;

                int merged = engine.TryQuickMergeSwords(3);
                Assert.GreaterOrEqual(merged, 1, "The first Lv.1 pair has a guaranteed success.");
                Assert.That(save.equippedSwordLevels[0], Is.GreaterThanOrEqualTo(2));

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
                save.subStage = 3; // So Ky -> Trung Ky gate!
                save.tuVi = engine.GetRequiredTuViForCurrent();

                int strikeEvents = 0;
                bool resolvedCalled = false;
                engine.OnTribulationStrike += (_, __) => strikeEvents++;
                engine.OnBreakthroughResolved += (_, __) => resolvedCalled = true;

                Assert.IsTrue(engine.IsBreakthroughReady);
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
                Assert.That(save.subStage, Is.EqualTo(4));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void PhaseGateBreakthrough_AutoLevelsNonGateFloorsAndBlocksOnlyAt3_6_10()
        {
            var go = new GameObject("TestEngineV4_PhaseGates");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 0;
                save.subStage = 1;
                save.tuVi = 0;

                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(1));
                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(2));
                Assert.IsTrue(CultivationTable.RequiresBreakthroughAt(3));  // So Ky -> Trung Ky
                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(4));
                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(5));
                Assert.IsTrue(CultivationTable.RequiresBreakthroughAt(6));  // Trung Ky -> Dinh Phong
                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(7));
                Assert.IsFalse(CultivationTable.RequiresBreakthroughAt(9));
                Assert.IsTrue(CultivationTable.RequiresBreakthroughAt(10)); // Dinh Phong -> Dai Canh Gioi

                // Feed enough TuVi via monster kills to auto-advance T1 -> T2 -> T3 and cap at T3 gate
                save.tuVi = CultivationTable.GetRequiredTuVi(0, 1) + CultivationTable.GetRequiredTuVi(0, 2) + CultivationTable.GetRequiredTuVi(0, 3) + 500;
                engine.OnMonsterKilled(null, false, null);

                Assert.That(save.subStage, Is.EqualTo(3), "Should auto-advance T1->T2->T3 and stop at So Ky gate (T3)");
                Assert.IsTrue(engine.IsBreakthroughReady, "Breakthrough should be ready at T3 100%");
                Assert.IsTrue(engine.TryAttemptBreakthrough(null), "Gate T3->T4 should resolve cleanly");
                Assert.That(save.subStage, Is.EqualTo(4));

                // Non-gate floor T4 cannot invoke breakthrough manually
                save.tuVi = CultivationTable.GetRequiredTuVi(0, 4);
                Assert.IsFalse(engine.IsBreakthroughReady, "T4 is not a breakthrough gate");
                Assert.IsFalse(engine.TryStartTribulation(null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void StageAutoAdvance_DeathRespawnCountdown_BossRepeat_And_RealmLocksWork()
        {
            var go = new GameObject("TestEngineV4_StageLoop");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 0;
                save.subStage = 1; // Luyen Khi T1: unlocks 1-1, 1-2, 1-3 (1-4 requires T2!)
                save.activeDifficulty = 0;
                save.activeAct = 0;
                save.activeStage = 1;
                save.highestUnlockedLinearStage = 0;
                save.stageWaveKills = 0;

                // 1. Clearing 10 kills on 1-1 without dying auto-advances to 1-2!
                for (int i = 0; i < 10; i++)
                    engine.OnMonsterKilled(null, false, null);
                Assert.That(save.activeStage, Is.EqualTo(2), "Clearing 1-1 without death must auto-advance to 1-2");
                Assert.GreaterOrEqual(save.highestUnlockedLinearStage, 1);

                // 2. Realm Lock blocks Act 2 (requires Truc Co T1) even if linear stage is unlocked
                save.highestUnlockedLinearStage = 25;
                Assert.IsFalse(engine.TrySelectStage(RemakeDifficulty.PhamGioi, 1, 1), "Act 2 must require Truc Co Sơ Kỳ");
                save.realmIndex = 1;
                save.subStage = 1;
                Assert.IsTrue(engine.TrySelectStage(RemakeDifficulty.PhamGioi, 1, 1), "Truc Co T1 unlocks Act 2-1");

                // 3. Death triggers 5s Respawn Countdown and stays on exact same stage (NEVER drops back!)
                save.activeDifficulty = 0;
                save.activeAct = 0;
                save.activeStage = 5;
                bool enteredRespawn = false;
                bool exitedRespawn = false;
                engine.OnRespawnStateChanged += r => { if (r) enteredRespawn = true; else exitedRespawn = true; };

                engine.OnPlayerDamaged(9999999);
                Assert.IsTrue(engine.IsRespawning);
                Assert.IsTrue(enteredRespawn);
                Assert.That(engine.RespawnTimeRemaining, Is.EqualTo(GameplayEngine.RespawnDuration).Within(0.01f));
                Assert.That(save.activeStage, Is.EqualTo(5), "Death must stay on stage 5, never retreat");

                // Player takes no further damage while respawning
                engine.OnPlayerDamaged(9999999);
                Assert.IsTrue(engine.IsRespawning);

                engine.AdvanceRespawnTimer(GameplayEngine.RespawnDuration + 0.1f);
                Assert.IsFalse(engine.IsRespawning);
                Assert.IsTrue(exitedRespawn);
                Assert.That(engine.PlayerCurrentHp, Is.EqualTo(engine.PlayerMaxHp).Within(1.0));
                Assert.That(save.activeStage, Is.EqualTo(5));

                // 4. Boss X-10 repeats continuously whether WIN or LOSE!
                save.activeAct = 0;
                save.activeStage = 10;
                engine.SetActiveBoss(true, "Cổ Mộc Yêu Vương", 2000, 2000);
                engine.OnMonsterKilled(null, true, null); // WIN Boss 1-10
                Assert.That(save.activeStage, Is.EqualTo(10), "Winning Boss 1-10 must keep activeStage = 10 for continuous repeat");
                Assert.IsTrue(save.bossAct1Cleared);

                engine.SetActiveBoss(true, "Cổ Mộc Yêu Vương", 2000, 2000);
                engine.OnPlayerDamaged(9999999); // LOSE Boss 1-10
                Assert.IsTrue(engine.IsRespawning);
                Assert.That(save.activeStage, Is.EqualTo(10), "Losing Boss 1-10 must stay on Boss 1-10 after respawn");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BalanceAlgorithm_BossTankinessAndMonotonicPlayerCurveHoldAcross120Stages()
        {
            double prevDps = 0;
            for (int r = 0; r <= 8; r++)
            {
                double dps = CultivationTable.EstimateExpectedPlayerDps(r, 1);
                Assert.Greater(dps, prevDps, $"Expected player DPS must scale up at realm {r}");
                prevDps = dps;
            }

            // Boss tankiness >= 7.5x normal X-9 (30x+ X-1), and synchronized to 60s Boss timer across all 120 stages!
            var s1_1 = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.PhamGioi, 0, 1);
            var s1_9 = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.PhamGioi, 0, 9);
            var b1_10 = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.PhamGioi, 0, 10);
            Assert.Greater(b1_10.enemyHp, s1_9.enemyHp * 7.5, "Boss 1-10 must be dramatically tankier than normal 1-9");
            Assert.Greater(b1_10.enemyHp, s1_1.enemyHp * 30.0, "Boss 1-10 must be 30x+ stronger than stage 1-1");
            double act1BossTtk = b1_10.enemyHp / CultivationTable.EstimateExpectedPlayerDps(b1_10.requiredRealmIndex, b1_10.requiredSubStage + 1);
            Assert.That(act1BossTtk, Is.InRange(28.0, 56.0), "Boss 1-10 TTK at recommended power must be tanky yet beatable within 60s");

            var b120 = ActStageDatabaseSO.BuildFallbackSpec(RemakeDifficulty.TienKiep, 2, 10);
            double endgameDps = CultivationTable.EstimateExpectedPlayerDps(b120.requiredRealmIndex, b120.requiredSubStage + 1);
            double boss120Ttk = b120.enemyHp / endgameDps;
            Assert.That(boss120Ttk, Is.InRange(28.0, 62.0), "Boss 120 HP must stay mathematically synchronized with endgame player DPS");
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

        [Test]
        public void AoEDamageBatch_CoalescesSixKillsIntoSingleEventSetAndInvalidatesStatCache()
        {
            var go = new GameObject("TestEngineV3_AoEBatch");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 1;
                save.subStage = 1;
                save.activeAct = 0;
                save.activeStage = 2;
                save.equippedSwordLevels.Clear();
                save.equippedSwordLevels.Add(1);
                save.swordInventoryLevels.Clear();
                save.swordInventoryLevels.Add(4);
                engine.InvalidateStatCache();

                double atkBefore = engine.GetTotalSwordAttackPower();
                Assert.IsTrue(engine.TryEquipFromBag(0));
                double atkAfterEquip = engine.GetTotalSwordAttackPower();
                Assert.Greater(atkAfterEquip, atkBefore, "Stat cache must invalidate on TryEquipFromBag");

                engine.SetSwordStyle(RemakeSwordStyle.PhaGiap);
                float spdBefore = engine.GetSwordSpeedMultiplier();
                engine.SetSwordStyle(RemakeSwordStyle.KiemXuyen);
                Assert.Greater(engine.GetSwordSpeedMultiplier(), spdBefore, "Stat cache must invalidate on SetSwordStyle");

                int statsEvents = 0, stageEvents = 0, questEvents = 0;
                engine.OnStatsChanged += () => statsEvents++;
                engine.OnStageChanged += () => stageEvents++;
                engine.OnQuestOrTutorialChanged += () => questEvents++;

                engine.BeginAoEDamageBatch();
                try
                {
                    for (int i = 0; i < 6; i++)
                        engine.OnMonsterKilled(null, false, null);
                }
                finally
                {
                    engine.EndAoEDamageBatch();
                }

                Assert.That(statsEvents, Is.EqualTo(1), "6 simultaneous AoE kills must coalesce into 1 OnStatsChanged");
                Assert.That(stageEvents, Is.EqualTo(1), "6 simultaneous AoE kills must coalesce into 1 OnStageChanged");
                Assert.That(questEvents, Is.EqualTo(1), "6 simultaneous AoE kills must coalesce into 1 OnQuestOrTutorialChanged");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void WavePlans_And_ChallengeSceneRoutingMatchDesign()
        {
            // Campaign normal stage: 3 waves (3+3+4 = 10 kills matching 30-day economy)
            GameplayEngine.GetWavePlan(RemakeArenaMode.Campaign, false, 1, out int wCamp, out int[] cCamp, out bool bCamp);
            Assert.That(wCamp, Is.EqualTo(3));
            Assert.That(cCamp, Is.EqualTo(new[] { 3, 3, 4 }));
            Assert.IsFalse(bCamp);

            // Campaign Boss stage X-10: 2 waves (3 guards -> 1 Boss)
            GameplayEngine.GetWavePlan(RemakeArenaMode.Campaign, true, 1, out int wBoss, out int[] cBoss, out bool bBoss);
            Assert.That(wBoss, Is.EqualTo(2));
            Assert.That(cBoss, Is.EqualTo(new[] { 3, 1 }));
            Assert.IsTrue(bBoss);

            // Demon Tower normal vs floor-5 Boss
            GameplayEngine.GetWavePlan(RemakeArenaMode.DemonTower, false, 3, out int wTNorm, out int[] cTNorm, out bool bTNorm);
            Assert.That(wTNorm, Is.EqualTo(2));
            Assert.That(cTNorm, Is.EqualTo(new[] { 4, 5 }));
            Assert.IsFalse(bTNorm);

            GameplayEngine.GetWavePlan(RemakeArenaMode.DemonTower, false, 5, out int wTBoss, out int[] cTBoss, out bool bTBoss);
            Assert.That(wTBoss, Is.EqualTo(2));
            Assert.That(cTBoss, Is.EqualTo(new[] { 3, 1 }));
            Assert.IsTrue(bTBoss);

            // Secret Realm: 3 waves (4+5+4 = 13 enemies)
            GameplayEngine.GetWavePlan(RemakeArenaMode.SecretRealm, false, 1, out int wSr, out int[] cSr, out bool bSr);
            Assert.That(wSr, Is.EqualTo(3));
            Assert.That(cSr, Is.EqualTo(new[] { 4, 5, 4 }));
            Assert.IsFalse(bSr);

            // Dedicated scene mapping for 3 Secret Realms + Demon Tower
            Assert.That(SceneFlowController.GetWorldSceneForSecretRealm(RemakeSecretRealmKind.SwordTomb), Is.EqualTo(SceneFlowController.WorldSecretSwordScene));
            Assert.That(SceneFlowController.GetWorldSceneForSecretRealm(RemakeSecretRealmKind.HerbGarden), Is.EqualTo(SceneFlowController.WorldSecretHerbScene));
            Assert.That(SceneFlowController.GetWorldSceneForSecretRealm(RemakeSecretRealmKind.OreCavern), Is.EqualTo(SceneFlowController.WorldSecretOreScene));
        }

        [Test]
        public void StarterSave_HasZeroEquipment_And_UnequipSwordAndGearWork()
        {
            var fresh = RemakeSaveData.CreateDefault(false);
            Assert.That(fresh.equippedGear.Count, Is.EqualTo(0), "New game starter save must have ZERO equipped gear");
            Assert.That(fresh.equipmentBag.Count, Is.EqualTo(0), "New game starter save must have ZERO equipment in bag");
            Assert.That(fresh.equippedSwordLevels.Count, Is.EqualTo(1), "Starter save begins with 1 Lv.1 sword");

            var go = new GameObject("TestEngineV7_Unequip");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 1;
                save.subStage = 1;
                save.equippedSwordLevels.Clear();
                save.swordInventoryLevels.Clear();
                save.equippedGear.Clear();
                save.equipmentBag.Clear();

                save.equippedSwordLevels.Add(4);
                Assert.Greater(engine.GetTotalSwordAttackPower(), 0);
                Assert.IsTrue(engine.TryUnequipSword(0));
                Assert.That(save.equippedSwordLevels.Count, Is.EqualTo(0));
                Assert.That(save.swordInventoryLevels.Count, Is.EqualTo(1));
                Assert.That(save.swordInventoryLevels[0], Is.EqualTo(4));
                Assert.That(engine.GetTotalSwordAttackPower(), Is.EqualTo(0), "Zero equipped swords should deal 0 sword attack power");

                var crown = new RemakeEquipmentItem
                {
                    uid = "test_crown_thien_01",
                    slot = RemakeEquipSlot.Crown,
                    rarity = RemakeRarity.Thien,
                    tier = 3
                };
                save.equipmentBag.Add(crown);

                Assert.IsTrue(engine.TryEquipGear(crown.uid));
                Assert.IsNotNull(engine.GetEquippedItem(RemakeEquipSlot.Crown));
                Assert.That(save.equipmentBag.Count, Is.EqualTo(0));

                Assert.IsTrue(engine.TryUnequipGear(RemakeEquipSlot.Crown));
                Assert.IsNull(engine.GetEquippedItem(RemakeEquipSlot.Crown));
                Assert.That(save.equipmentBag.Count, Is.EqualTo(1));
                Assert.That(save.equipmentBag[0].uid, Is.EqualTo(crown.uid));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Tribulation_DrainsHpProgressivelyPerStrike_BeforeAnnouncingOutcome()
        {
            var go = new GameObject("TestEngineV7_TribHpDrain");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 0;
                save.subStage = 3; // Guaranteed So Ky -> Trung Ky gate
                save.tuVi = engine.GetRequiredTuViForCurrent();

                bool outcomeAnnounced = false;
                double lastStrikeHp = double.MaxValue;
                int strikesObserved = 0;

                engine.OnBreakthroughResolved += (_, __) => outcomeAnnounced = true;
                engine.OnTribulationStrike += (idx, total) =>
                {
                    Assert.IsFalse(outcomeAnnounced, "Outcome must NOT be announced before all lightning strikes complete");
                    Assert.Less(engine.PlayerCurrentHp, lastStrikeHp, $"HP must decrease on lightning strike {idx}/{total}");
                    lastStrikeHp = engine.PlayerCurrentHp;
                    strikesObserved = idx;
                };

                Assert.IsTrue(engine.TryStartTribulation(null));
                double fullHp = engine.PlayerMaxHp;
                Assert.That(engine.PlayerCurrentHp, Is.EqualTo(fullHp).Within(1.0));

                // Advance to 13.8s (after all 9 strikes have landed, BEFORE 15.0s completion!)
                for (int s = 0; s < 28; s++)
                    engine.AdvanceTribulation(0.49f);

                Assert.IsTrue(engine.IsInTribulation, "Still in seclusion right after 9th strike before 15s ends");
                Assert.That(strikesObserved, Is.EqualTo(GameplayEngine.TribulationStrikesTotal));
                Assert.IsFalse(outcomeAnnounced, "Outcome still pending right after 9th strike");
                Assert.LessOrEqual(engine.PlayerCurrentHp, fullHp * 0.10, "HP must be drained nearly empty (<10%) after 9th strike");
                Assert.GreaterOrEqual(engine.PlayerCurrentHp, 1.0);

                // Finish remaining 1.5s -> announces success & refills HP!
                engine.AdvanceTribulation(2.0f);
                Assert.IsFalse(engine.IsInTribulation);
                Assert.IsTrue(outcomeAnnounced);
                Assert.That(save.subStage, Is.EqualTo(4));
                Assert.That(engine.PlayerCurrentHp, Is.EqualTo(engine.PlayerMaxHp).Within(1.0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RearCombatCamera_FacesBehindBack_AndClampsYawAndPitchAgainstFrontOrUnderView()
        {
            Vector3 player = Vector3.zero;
            Vector3 facing = Vector3.forward; // RemakePlayer_Xianxia faces +Z

            // 1. Default rear view sits directly behind character's back (-Z), looking forward (+Z)
            MeadowEnvironmentController.ComputeRearCameraPose(
                player, facing,
                MeadowEnvironmentController.DefaultRearYawOffsetDeg,
                MeadowEnvironmentController.DefaultRearPitchDeg,
                MeadowEnvironmentController.DefaultRearDistance,
                out Vector3 defaultPos, out Quaternion defaultRot);

            Assert.Less(defaultPos.z, -4.0f, "Default rear camera must be behind the character (-Z)");
            Assert.Greater(defaultPos.y, 2.0f, "Default rear camera must stay well above ground");
            Assert.Greater(Vector3.Dot(defaultRot * Vector3.forward, facing), 0.85f, "Rear camera must look forward past character's back");

            // 2. Extreme horizontal drag (+/- 500 deg) must stay strictly behind shoulder plane (never look at front face!)
            foreach (float extremeYaw in new[] { -500f, -180f, 180f, 500f })
            {
                MeadowEnvironmentController.ComputeRearCameraPose(
                    player, facing, extremeYaw, MeadowEnvironmentController.DefaultRearPitchDeg,
                    MeadowEnvironmentController.DefaultRearDistance,
                    out Vector3 sidePos, out _);

                Vector3 camToPlayerFlat = Vector3.ProjectOnPlane(player - sidePos, Vector3.up).normalized;
                Assert.Greater(Vector3.Dot(camToPlayerFlat, facing), 0.45f,
                    $"Extreme yaw {extremeYaw} must never orbit in front of the character's face");
            }

            // 3. Extreme downward / upward pitch drag must NEVER dip below ground or look up from underneath!
            foreach (float extremePitch in new[] { -90f, -15f, 0f, 5f, 120f })
            {
                MeadowEnvironmentController.ComputeRearCameraPose(
                    player, facing, 0f, extremePitch, MeadowEnvironmentController.MinRearDistance,
                    out Vector3 vPos, out Quaternion vRot);

                Assert.GreaterOrEqual(vPos.y, player.y + MeadowEnvironmentController.MinRearCameraHeightAbovePlayer - 0.001f,
                    $"Camera at pitch {extremePitch} must never dip below minimum safe elevation");
                Assert.Less((vRot * Vector3.forward).y, -0.05f,
                    $"Camera at pitch {extremePitch} must never look upward from underneath the character");
            }
        }

        [Test]
        public void V3Economy_MonotonicTuViAndLinhThachAcross120Stages_AndFastOnboarding()
        {
            // Fast Luyen Khi T1-T3 onboarding while keeping total 36k & T10 = 19%
            double t1 = CultivationTable.GetRequiredTuVi(0, 1);
            double t10 = CultivationTable.GetRequiredTuVi(0, 10);
            Assert.LessOrEqual(t1, 700.0, "Luyen Khi T1 should be fast (~5 min to T4)");
            Assert.That(t10, Is.EqualTo(Math.Round(36000.0 * 0.19)).Within(1.0));

            double prevNormalTuVi = 0;
            double prevNormalLt = 0;
            double prevBossTuVi = 0;
            double prevBossLt = 0;
            for (int lin = 0; lin < 120; lin++)
            {
                var diff = (RemakeDifficulty)(lin / 30);
                int act = (lin % 30) / 10;
                int stage = (lin % 10) + 1;
                var spec = ActStageDatabaseSO.BuildFallbackSpec(diff, act, stage);
                if (spec.isBossStage)
                {
                    Assert.GreaterOrEqual(spec.tuViPerKill, prevBossTuVi, $"Boss TuVi dropped at linear stage {lin}");
                    Assert.GreaterOrEqual(spec.linhThachPerKill, prevBossLt, $"Boss LT dropped at linear stage {lin}");
                    prevBossTuVi = spec.tuViPerKill;
                    prevBossLt = spec.linhThachPerKill;
                }
                else
                {
                    Assert.GreaterOrEqual(spec.tuViPerKill, prevNormalTuVi, $"Normal TuVi dropped at linear stage {lin}");
                    Assert.GreaterOrEqual(spec.linhThachPerKill, prevNormalLt, $"Normal LT dropped at linear stage {lin}");
                    prevNormalTuVi = spec.tuViPerKill;
                    prevNormalLt = spec.linhThachPerKill;
                }
            }
        }

        [Test]
        public void V3ThreeSlotSave_CreateGenderPeekSelectAndDeleteWorkCleanly()
        {
            SaveService.SelectSlot(0);
            Assert.That(SaveService.GetSaveFilePathForSlot(0), Is.EqualTo(SaveService.GetSaveFilePath()), "Slot 0 must preserve legacy filename");

            // Create Female in Slot 0
            var s0 = SaveService.Load();
            s0.hasCreatedCharacter = true;
            s0.daoName = "ThanhVanNu";
            s0.gender = RemakeCharacterGender.Female;
            s0.realmIndex = 2;
            SaveService.Save();

            // Create Male in Slot 1
            SaveService.SelectSlot(1);
            var s1 = SaveService.Load();
            s1.hasCreatedCharacter = true;
            s1.daoName = "KiemTuNam";
            s1.gender = RemakeCharacterGender.Male;
            s1.realmIndex = 4;
            SaveService.Save();

            // Slot 2 empty
            var p0 = SaveService.PeekSlot(0);
            var p1 = SaveService.PeekSlot(1);
            var p2 = SaveService.PeekSlot(2);
            Assert.IsNotNull(p0);
            Assert.That(p0.gender, Is.EqualTo(RemakeCharacterGender.Female));
            Assert.That(p0.daoName, Is.EqualTo("ThanhVanNu"));
            Assert.IsNotNull(p1);
            Assert.That(p1.gender, Is.EqualTo(RemakeCharacterGender.Male));
            Assert.That(p1.daoName, Is.EqualTo("KiemTuNam"));
            Assert.IsNull(p2);

            // Delete Slot 1 -> Slot 0 stays untouched
            SaveService.DeleteSlot(1);
            Assert.IsNull(SaveService.PeekSlot(1));
            Assert.IsNotNull(SaveService.PeekSlot(0));
            Assert.That(SaveService.FindFirstOccupiedSlotOrDefault(), Is.EqualTo(0));
            SaveService.SelectSlot(0);
        }

        [Test]
        public void V3Features_BossShieldBreak_Codex_Companions_AutoTower_OfflineAndPvpWork()
        {
            var go = new GameObject("TestEngineV3_Features");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 4; // Hoa Than (unlocks Codex, Companions, PvP, Tower)
                save.subStage = 5;
                save.linhThach = 500000;
                save.coDuyen = 50;
                save.tutorialCompleted = true;

                // 1. Boss Shield Break triggers stun + bonus damage window
                engine.SetActiveBoss(true, "Thái Hư Ma Tôn", 10000, 10000);
                engine.AdvanceBossShieldCycle(12f);
                Assert.IsTrue(engine.BossShieldActive);
                Assert.IsFalse(engine.IsBossStunned);
                engine.ApplyBossShieldDamage(6000, true);
                Assert.IsFalse(engine.BossShieldActive);
                Assert.IsTrue(engine.IsBossStunned);
                Assert.Greater(engine.BossBrokenRemaining, 4.0f);

                // 2. Codex Collection unlocks & milestone claim
                save.codexSwordsMask = 0b11111; // 5 swords -> milestone 0 (3 entries) ready!
                int total = engine.GetCodexUnlockedCount(out int sw, out _, out _);
                Assert.That(sw, Is.EqualTo(5));
                Assert.GreaterOrEqual(total, 5);
                Assert.IsTrue(engine.IsCodexMilestoneReady(0));
                int cdBefore = save.coDuyen;
                Assert.IsTrue(engine.TryClaimCodexMilestone(0));
                Assert.Greater(save.coDuyen, cdBefore);

                // 3. Companion unlock & switch
                Assert.IsTrue(engine.TryUnlockOrSelectCompanion(RemakeCompanionId.AzureCrane));
                Assert.That(save.activeCompanionId, Is.EqualTo((int)RemakeCompanionId.AzureCrane));

                // 4. Auto-Climb Demon Tower toggle
                engine.SetAutoClimbDemonTower(true);
                Assert.IsTrue(save.autoClimbDemonTower);

                // 5. Live Luan Kiem Dai PvP + Avatar Frame purchase
                engine.EnsureDailyPvpReset();
                Assert.That(save.pvpDailyRemaining, Is.EqualTo(5));
                double myPower = engine.GetPlayerCombatPowerEstimate();
                Assert.IsTrue(engine.TryChallengePvpOpponent("Bạch Ngọc Kiếm Khách", myPower * 0.5));
                Assert.That(save.pvpDailyRemaining, Is.EqualTo(4));
                Assert.Greater(save.swordPrestigePoints, 0);
                save.swordPrestigePoints = 1000;
                Assert.IsTrue(engine.TryBuyPvpReward(3)); // Frame Sword Saint
                Assert.That(save.equippedAvatarFrame, Is.EqualTo(1));

                // 6. Progressive Feature Unlock Tutorials bitmask tracking
                save.completedFeatureTutorialsMask = 0;
                Assert.IsFalse(engine.IsFeatureTutorialCompleted(RemakeFeatureId.DaoVanTree));
                engine.MarkFeatureTutorialCompleted(RemakeFeatureId.DaoVanTree);
                Assert.IsTrue(engine.IsFeatureTutorialCompleted(RemakeFeatureId.DaoVanTree));
                Assert.IsFalse(engine.IsFeatureTutorialCompleted(RemakeFeatureId.Alchemy));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CloudCushionVfx_PrefabLoadsZeroShadowsAndEvolvesAcrossAll9Realms()
        {
            var prefab = Resources.Load<RemakeCloudCushionVfx>("VFX/VFX_PlayerCloudCushion");
            Assert.NotNull(prefab, "VFX_PlayerCloudCushion.prefab must exist under Assets/_Game/Resources/VFX/");

            var inst = UnityEngine.Object.Instantiate(prefab);
            try
            {
                foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
                    Assert.IsFalse(r.receiveShadows);
                }
                Assert.That(inst.GetComponentsInChildren<Light>(true).Length, Is.EqualTo(0));

                // Realm colors evolve distinctly from Luyen Khi -> Kim Dan -> Nguyen Anh -> Hoa Than
                RemakeCloudCushionVfx.GetRealmCloudColors(0, 0f, out Color cLk, out Color rLk);
                RemakeCloudCushionVfx.GetRealmCloudColors(2, 0f, out Color cKd, out Color rKd);
                RemakeCloudCushionVfx.GetRealmCloudColors(3, 0f, out Color cNa, out Color rNa);
                RemakeCloudCushionVfx.GetRealmCloudColors(4, 0f, out Color cHt, out Color rHt);
                Assert.AreNotEqual(rLk, rKd);
                Assert.AreNotEqual(rKd, rNa);
                Assert.AreNotEqual(rNa, rHt);

                // Tribulation lifts cloud higher than normal seat height
                float normalY = inst.TickCloud(0.05f);
                inst.SetTribulationActive(true);
                float tribY = normalY;
                for (int i = 0; i < 30; i++) tribY = inst.TickCloud(0.05f);
                Assert.Greater(tribY, normalY + 0.08f, "Tribulation must visibly lift the cloud cushion higher");

                // Death dissolve hides cloud cleanly
                inst.SetDissolveProgress(1f);
                inst.TickCloud(0.05f);
                var mr = inst.GetComponentInChildren<MeshRenderer>(true);
                Assert.NotNull(mr);
                Assert.IsFalse(mr.enabled, "Fully dissolved cloud mesh should disable renderer");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst.gameObject);
            }
        }

        [Test]
        public void VietnameseTypography_AllSixSdfFontsLoadWithFullGlyphsAndFallback()
        {
            string[] resPaths =
            {
                "RemakeFontSDF",
                "Fonts/RemakeFont_UiSemiBoldSDF",
                "Fonts/RemakeFont_CormorantBoldSDF",
                "Fonts/RemakeFont_CharmBoldSDF",
                "Fonts/RemakeFont_BabylonicaSDF",
                "Fonts/RemakeFont_SymbolFallbackSDF"
            };

            const string viSample =
                "Khí Vận Chi Tử · Luyện Khí · Trúc Cơ · Kim Đan · Nguyên Anh · Hóa Thần · Luyện Hư · Hợp Thể · Đại Thừa · Độ Kiếp " +
                "ĂÂĐÊÔƠƯăâđêôơưÁÀẢÃẠẮẰẲẴẶẤẦẨẪẬÉÈẺẼẸẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌỐỒỔỖỘỚỜỞỠỢÚÙỦŨỤỨỪỬỮỰÝỲỶỸỴ" +
                "áàảãạắằẳẵặấầẩẫậéèẻẽẹếềểễệíìỉĩịóòỏõọốồổỗộớờởỡợúùủũụứừửữựýỳỷỹỵ ←→◀▶✓▲▼";

            foreach (string rp in resPaths)
            {
                var fa = Resources.Load<FontAsset>(rp);
                Assert.NotNull(fa, $"Missing FontAsset in Resources: {rp}");
                Assert.NotNull(fa.sourceFontFile, $"FontAsset {rp} must retain sourceFontFile for dynamic runtime SDF");
                try
                {
                    Assert.IsTrue(fa.HasCharacters(viSample, out var missing, searchFallbacks: true, tryAddCharacter: true),
                        $"FontAsset {rp} missing Vietnamese/UI glyphs even with fallback: {(missing != null ? string.Join(",", missing) : "")}");
                }
                finally
                {
                    fa.ClearFontAssetData(true);
                }
            }

            var baseFont = Resources.Load<FontAsset>("RemakeFontSDF");
            Assert.NotNull(baseFont.fontWeightTable);
            Assert.NotNull(baseFont.fontWeightTable[7].regularTypeface, "Base Be Vietnam Pro Regular must map Bold weight to SemiBold FontAsset");
        }

        [Test]
        public void AudioVolumeSettings_MigrateLegacyZeroesAndPersistSliderChanges()
        {
            // Legacy JSON missing audioVolumeInitialized / volume fields must migrate cleanly to audible defaults
            var legacy = new RemakeSaveData
            {
                schemaVersion = SaveService.CurrentSchemaVersion,
                hasCreatedCharacter = true,
                daoName = "AmLuongTest"
            };
            legacy.graphics.audioVolumeInitialized = false;
            legacy.graphics.masterVolume = 0f;
            legacy.graphics.musicVolume = 0f;
            legacy.graphics.sfxVolume = 0f;
            File.WriteAllText(SaveService.GetSaveFilePath(), JsonUtility.ToJson(legacy));

            var loaded = SaveService.Load();
            Assert.IsTrue(loaded.graphics.audioVolumeInitialized);
            Assert.That(loaded.graphics.masterVolume, Is.EqualTo(1.0f).Within(0.001f));
            Assert.That(loaded.graphics.musicVolume, Is.EqualTo(0.70f).Within(0.001f));
            Assert.That(loaded.graphics.sfxVolume, Is.EqualTo(0.85f).Within(0.001f));

            // GraphicsSettingsService setters clamp [0..1] and persist through SaveService
            var go = new GameObject("TestGraphicsAudioVol");
            try
            {
                var svc = go.AddComponent<GraphicsSettingsService>();
                svc.SetMasterVolume(1.5f, true);
                svc.SetMusicVolume(0.35f, true);
                svc.SetSfxVolume(-0.2f, true);

                var reloaded = SaveService.Load();
                Assert.That(reloaded.graphics.masterVolume, Is.EqualTo(1.0f).Within(0.001f));
                Assert.That(reloaded.graphics.musicVolume, Is.EqualTo(0.35f).Within(0.001f));
                Assert.That(reloaded.graphics.sfxVolume, Is.EqualTo(0.0f).Within(0.001f));
                Assert.IsTrue(reloaded.graphics.audioVolumeInitialized);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void NotificationSystem_Categorizes3ZonesCoalescesLootAndFixesFalseRed()
        {
            // 1. Explicit 5-palette categories & zero false-red on stage-clear realm hint
            var waveMeta = BootOverlayController.ResolveMeta(RemakeNotifyCategory.CombatWave, false);
            Assert.That(waveMeta.CssClass, Is.EqualTo("toast-azure"));
            var breakMeta = BootOverlayController.ResolveMeta(RemakeNotifyCategory.Breakthrough, false);
            Assert.That(breakMeta.CssClass, Is.EqualTo("toast-gold"));
            Assert.IsTrue(breakMeta.UseCalligraphy);
            var alchMeta = BootOverlayController.ResolveMeta(RemakeNotifyCategory.Alchemy, false);
            Assert.That(alchMeta.CssClass, Is.EqualTo("toast-violet"));
            var lootMeta = BootOverlayController.ResolveMeta(RemakeNotifyCategory.LootDrop, false);
            Assert.That(lootMeta.CssClass, Is.EqualTo("toast-jade"));
            var errMeta = BootOverlayController.ResolveMeta(RemakeNotifyCategory.Alchemy, true);
            Assert.That(errMeta.CssClass, Is.EqualTo("toast-danger"));
            Assert.That(errMeta.BadgeLabel, Is.EqualTo("NỔ LÒ"));

            // Legacy string classifier also must NOT turn "Vượt Ải ... Cần đạt ..." red!
            var legacyCat = BootOverlayController.InferLegacyCategory("Vượt Ải 1-9! Cần đạt Trúc Cơ để mở Ải 1-10.", out bool legacyErr);
            Assert.That(legacyCat, Is.EqualTo(RemakeNotifyCategory.CombatWave));
            Assert.IsFalse(legacyErr);

            // 2. 3-Zone spatial routing
            Assert.That(BootOverlayController.ResolveEffectiveZone(RemakeNotifyCategory.Breakthrough, RemakeNotifyZone.Auto, false, false),
                Is.EqualTo(RemakeNotifyZone.GrandTop));
            Assert.That(BootOverlayController.ResolveEffectiveZone(RemakeNotifyCategory.LootDrop, RemakeNotifyZone.Auto, false, false),
                Is.EqualTo(RemakeNotifyZone.CombatTicker));
            Assert.That(BootOverlayController.ResolveEffectiveZone(RemakeNotifyCategory.SwordPath, RemakeNotifyZone.Auto, true, false),
                Is.EqualTo(RemakeNotifyZone.DrawerInline));
            Assert.That(BootOverlayController.ResolveEffectiveZone(RemakeNotifyCategory.Alchemy, RemakeNotifyZone.Auto, true, true),
                Is.EqualTo(RemakeNotifyZone.DrawerInline));
            Assert.That(BootOverlayController.ResolveEffectiveZone(RemakeNotifyCategory.SwordPath, RemakeNotifyZone.DrawerInline, false, false),
                Is.EqualTo(RemakeNotifyZone.GrandTop));

            // 3. Suppress routine waves 1/3, 2/3 while keeping Boss/Elite final waves
            Assert.IsTrue(BootOverlayController.ShouldSuppressWaveToast(1, 3, "Ải 1-1 · Đợt 1/3"));
            Assert.IsTrue(BootOverlayController.ShouldSuppressWaveToast(2, 3, "Ải 1-1 · Đợt 2/3"));
            Assert.IsFalse(BootOverlayController.ShouldSuppressWaveToast(2, 2, "Thủ Lĩnh 1-10 xuất hiện!"));
            Assert.IsFalse(BootOverlayController.ShouldSuppressWaveToast(3, 3, "Đợt Tinh Anh cuối cùng!"));

            // 4. 3s duplicate loot coalescing
            Assert.IsTrue(BootOverlayController.TryCoalesceMessage("Nhặt Phôi Kiếm Cấp 1!", 1.2f, 1, "Nhặt Phôi Kiếm Cấp 1!", out int c2, out string fmt2));
            Assert.That(c2, Is.EqualTo(2));
            Assert.That(fmt2, Is.EqualTo("Nhặt Phôi Kiếm Cấp 1! ×2"));
            Assert.IsTrue(BootOverlayController.TryCoalesceMessage("Nhặt Phôi Kiếm Cấp 1!", 2.5f, 2, "Nhặt Phôi Kiếm Cấp 1!", out int c3, out string fmt3));
            Assert.That(c3, Is.EqualTo(3));
            Assert.That(fmt3, Is.EqualTo("Nhặt Phôi Kiếm Cấp 1! ×3"));
            Assert.IsFalse(BootOverlayController.TryCoalesceMessage("Nhặt Phôi Kiếm Cấp 1!", 3.5f, 3, "Nhặt Phôi Kiếm Cấp 1!", out int cReset, out _));
            Assert.That(cReset, Is.EqualTo(1));
        }

        [Test]
        public void SmartBadges_EliminateFakeSwordDotAndCountAllTenFeatures()
        {
            var go = new GameObject("TestEngineSmartBadges");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.tutorialCompleted = true;
                save.totalMerges = 5;
                save.realmIndex = 0; // Luyen Khi -> max 1 sword slot
                save.subStage = 1;
                save.linhThach = 0; // Cannot afford merge
                save.skillScrollCount = 0;
                save.equippedSwordLevels.Clear();
                save.equippedSwordLevels.Add(4);
                save.swordInventoryLevels.Clear();
                save.swordInventoryLevels.Add(1); // Single weak sword in bag -> MUST NOT trigger fake red dot!

                var cleanSnap = InGameHudController.ComputeSmartBadges(save, engine, null);
                Assert.IsFalse(cleanSnap.SwordPath, "Single weaker bag sword with full slots and no pair must NOT show a fake red dot");
                Assert.That(cleanSnap.BaguaCount, Is.EqualTo(0));
                Assert.IsFalse(cleanSnap.ImmortalGate);

                // Now add a second Lv.1 sword + enough Linh Thach -> SwordPath lights up!
                save.swordInventoryLevels.Add(1);
                save.linhThach = 50000;
                var pairSnap = InGameHudController.ComputeSmartBadges(save, engine, null);
                Assert.IsTrue(pairSnap.SwordPath);
                Assert.That(pairSnap.BaguaCount, Is.EqualTo(1));

                // Unlock all features & populate actionable states across multiple modules
                save.realmIndex = 4; // Hoa Than -> unlocks Alchemy, Gear, Formations + SubSlot, Skills, Companions, Tower, SecretRealm
                save.subStage = 1;
                save.herbsByTier = new[] { 50, 50, 50, 50, 50, 50 };
                save.skillScrollCount = 2;
                save.activeMainFormation = -1;
                save.activeSubFormation = -1;
                save.equipmentBag.Add(new RemakeEquipmentItem { uid = "eq-up", slot = RemakeEquipSlot.Robe, rarity = RemakeRarity.Dia, tier = 3 });
                save.demonTowerHighestFloor = 3;
                save.lastTowerSalaryDayUtc = 0;
                save.secretRealmDailyRemaining = 3;

                var richSnap = InGameHudController.ComputeSmartBadges(save, engine, null);
                Assert.IsTrue(richSnap.Alchemy);
                Assert.IsTrue(richSnap.Equipment);
                Assert.IsTrue(richSnap.Formation);
                Assert.IsTrue(richSnap.Skills);
                Assert.IsTrue(richSnap.Companion);
                Assert.IsTrue(richSnap.ImmortalGate);
                Assert.GreaterOrEqual(richSnap.BaguaCount, 6);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void FiveBusAudio_MigratesAmbienceVoiceAutoDuckingAndGenderRoutes()
        {
            // 1. Save migration for new ambienceVolume & voiceVolume sliders
            var legacy = new RemakeSaveData
            {
                schemaVersion = SaveService.CurrentSchemaVersion,
                hasCreatedCharacter = true,
                daoName = "FiveBusTest"
            };
            legacy.graphics.extendedAudioInitialized = false;
            legacy.graphics.ambienceVolume = 0f;
            legacy.graphics.voiceVolume = 0f;
            File.WriteAllText(SaveService.GetSaveFilePath(), JsonUtility.ToJson(legacy));

            var loaded = SaveService.Load();
            Assert.IsTrue(loaded.graphics.extendedAudioInitialized);
            Assert.That(loaded.graphics.ambienceVolume, Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(loaded.graphics.voiceVolume, Is.EqualTo(0.95f).Within(0.001f));

            // 2. Auto-ducking targets: FullCinematic (Nguyen Anh quote with baked music) drops BGM to 8%
            Assert.That(RemakeAudioService.ResolveDuckMode(RemakeVoiceLineId.RealmNguyenAnh), Is.EqualTo(RemakeVoiceDuckMode.FullCinematic));
            Assert.That(RemakeAudioService.ResolveDuckMode(RemakeVoiceLineId.SkillVanKiem), Is.EqualTo(RemakeVoiceDuckMode.Standard));
            RemakeAudioService.ComputeVoiceDuckTargets(true, RemakeVoiceDuckMode.FullCinematic, out float cinBgm, out float cinAmb);
            Assert.That(cinBgm, Is.EqualTo(0.08f).Within(0.001f));
            Assert.That(cinAmb, Is.EqualTo(0.25f).Within(0.001f));
            RemakeAudioService.ComputeVoiceDuckTargets(true, RemakeVoiceDuckMode.Standard, out float stdBgm, out float stdAmb);
            Assert.That(stdBgm, Is.EqualTo(0.40f).Within(0.001f));
            Assert.That(stdAmb, Is.EqualTo(0.55f).Within(0.001f));
            RemakeAudioService.ComputeVoiceDuckTargets(false, RemakeVoiceDuckMode.FullCinematic, out float idleBgm, out float idleAmb);
            Assert.That(idleBgm, Is.EqualTo(1.0f).Within(0.001f));
            Assert.That(idleAmb, Is.EqualTo(1.0f).Within(0.001f));

            // 3. Priority hierarchy: Realm Breakthrough (P1) > Boss (P2) > Combat Skill (P3)
            Assert.Less(RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.RealmNguyenAnh),
                RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.BossEncounter));
            Assert.Less(RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.BossEncounter),
                RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.SkillVanKiem));

            // 4. Gendered voice routing (Male vs Female)
            Assert.That(AudioDropLoader.GetVoiceKey(RemakeVoiceLineId.RealmNguyenAnh, RemakeCharacterGender.Male),
                Is.EqualTo("Voice/Male/vo_male_realm_nguyenanh"));
            Assert.That(AudioDropLoader.GetVoiceKey(RemakeVoiceLineId.RealmNguyenAnh, RemakeCharacterGender.Female),
                Is.EqualTo("Voice/Female/vo_female_realm_nguyenanh"));
            Assert.That(AudioDropLoader.GetAmbienceKey(RemakeAmbienceZone.Act1Meadow),
                Is.EqualTo("Ambience/amb_act1_meadow"));

            // 5. All 5 monster audio families resolve cleanly from metadata
            Assert.That(MonsterDefinitionSO.ResolveAudioFamilyFromMetadata("act1_wolf", false, RemakeMonsterArchetype.FastMelee),
                Is.EqualTo(RemakeMonsterAudioFamily.Beast));
            Assert.That(MonsterDefinitionSO.ResolveAudioFamilyFromMetadata("act3_jade_crane", false, RemakeMonsterArchetype.FlyingAgile),
                Is.EqualTo(RemakeMonsterAudioFamily.Avian));
            Assert.That(MonsterDefinitionSO.ResolveAudioFamilyFromMetadata("act2_sand_turtle", false, RemakeMonsterArchetype.ArmoredTank),
                Is.EqualTo(RemakeMonsterAudioFamily.Armored));
            Assert.That(MonsterDefinitionSO.ResolveAudioFamilyFromMetadata("act3_thunder_spirit", false, RemakeMonsterArchetype.FastMelee),
                Is.EqualTo(RemakeMonsterAudioFamily.SpiritInsect));
            Assert.That(MonsterDefinitionSO.ResolveAudioFamilyFromMetadata("act1_boss_guardian", true, RemakeMonsterArchetype.BossGuardian),
                Is.EqualTo(RemakeMonsterAudioFamily.Boss));

            // 6. GraphicsSettingsService setters persist ambience & voice volumes
            var go = new GameObject("TestGraphicsFiveBus");
            try
            {
                var svc = go.AddComponent<GraphicsSettingsService>();
                svc.SetAmbienceVolume(0.45f, true);
                svc.SetVoiceVolume(0.88f, true);
                var reloaded = SaveService.Load();
                Assert.That(reloaded.graphics.ambienceVolume, Is.EqualTo(0.45f).Within(0.001f));
                Assert.That(reloaded.graphics.voiceVolume, Is.EqualTo(0.88f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AudioDropBatch1_AllTwelveGoogleFlowClipsImportAndLoadCleanly()
        {
            AudioDropLoader.InvalidateCache();

            // 1. Signature Nguyen Anh FullCinematic Male + Female voices (stereo 44.1kHz with music bed)
            var vMaleNa = AudioDropLoader.LoadVoice(RemakeVoiceLineId.RealmNguyenAnh, RemakeCharacterGender.Male);
            var vFemaleNa = AudioDropLoader.LoadVoice(RemakeVoiceLineId.RealmNguyenAnh, RemakeCharacterGender.Female);
            Assert.NotNull(vMaleNa, "Missing vo_male_realm_nguyenanh in Resources/AudioDrop/Voice/Male/");
            Assert.NotNull(vFemaleNa, "Missing vo_female_realm_nguyenanh in Resources/AudioDrop/Voice/Female/");
            Assert.Greater(vMaleNa.length, 1.5f);
            Assert.Greater(vFemaleNa.length, 1.5f);

            // 2. Character select & Van Kiem voices (Male + Female)
            Assert.NotNull(AudioDropLoader.LoadVoice(RemakeVoiceLineId.SelectPreview, RemakeCharacterGender.Male));
            Assert.NotNull(AudioDropLoader.LoadVoice(RemakeVoiceLineId.SelectPreview, RemakeCharacterGender.Female));
            Assert.NotNull(AudioDropLoader.LoadVoice(RemakeVoiceLineId.SkillVanKiem, RemakeCharacterGender.Male));
            Assert.NotNull(AudioDropLoader.LoadVoice(RemakeVoiceLineId.SkillVanKiem, RemakeCharacterGender.Female));

            // 3. 3 Seamless Ambience zones
            var ambMeadow = AudioDropLoader.LoadAmbience(RemakeAmbienceZone.Act1Meadow);
            var ambWaste = AudioDropLoader.LoadAmbience(RemakeAmbienceZone.Act2Wasteland);
            var ambStorm = AudioDropLoader.LoadAmbience(RemakeAmbienceZone.TribulationStorm);
            Assert.NotNull(ambMeadow);
            Assert.NotNull(ambWaste);
            Assert.NotNull(ambStorm);
            Assert.Greater(ambMeadow.length, 4.0f);

            // 4. 3 Monster & Boss SFX packs
            var mobBeast = AudioDropLoader.LoadMonsterCue(RemakeMonsterAudioFamily.Beast, "attack");
            var mobArmored = AudioDropLoader.LoadMonsterCue(RemakeMonsterAudioFamily.Armored, "hurt");
            var mobBoss = AudioDropLoader.LoadMonsterCue(RemakeMonsterAudioFamily.Boss, "roar");
            Assert.NotNull(mobBeast);
            Assert.NotNull(mobArmored);
            Assert.NotNull(mobBoss);
            Assert.LessOrEqual(mobBeast.length, 3.0f, "Normal monster SFX should be trimmed snappy (< 3s)");
            Assert.LessOrEqual(mobArmored.length, 3.0f, "Armored monster SFX should be trimmed snappy (< 3s)");
        }

        [Test]
        public void EquipmentEnhance_And_Skills_AlwaysIncreaseCombatPowerAndEquipScore()
        {
            var go = new GameObject("TestEquipEnhancePower");
            try
            {
                var engine = go.AddComponent<GameplayEngine>();
                var save = SaveService.Current;
                save.hasCreatedCharacter = true;
                save.realmIndex = 2; // Kim Dan -> unlocks Equipment & Skills
                save.subStage = 1;
                save.linhThach = 1_000_000;
                save.ore = 500;
                save.skillScrollCount = 5;
                save.equippedSwordLevels.Clear();
                save.equippedSwordLevels.Add(3);
                save.equippedGear.Clear();
                save.equipSlotEnhanceLevels = new int[6];

                // Even with an item that ONLY has non-combat substats (TuVi / LinhThach), enhancing MUST increase Combat Power & Lực TB!
                var utilityRobe = new RemakeEquipmentItem
                {
                    uid = "util-robe",
                    slot = RemakeEquipSlot.Robe,
                    rarity = RemakeRarity.Huyen,
                    tier = 2,
                    substats = new System.Collections.Generic.List<RemakeEquipSubstatEntry>
                    {
                        new RemakeEquipSubstatEntry { statType = RemakeEquipStatType.TuViGainPct, value = 12f }
                    }
                };
                save.equippedGear.Add(utilityRobe);
                engine.InvalidateStatCache();

                double powerBefore = engine.GetPlayerCombatPowerEstimate();
                float scoreBefore = CultivationTable.CalculateEquipItemScore(utilityRobe, save.realmIndex, save.equipSlotEnhanceLevels[(int)RemakeEquipSlot.Robe]);
                double hpBefore = engine.PlayerMaxHp;

                // 1. Enhance Defensive slot (Robe) -> increases HP, DR, Equip Score AND Combat Power!
                Assert.IsTrue(engine.TryEnhanceEquipSlot((int)RemakeEquipSlot.Robe));
                double powerAfterRobe = engine.GetPlayerCombatPowerEstimate();
                float scoreAfterRobe = CultivationTable.CalculateEquipItemScore(utilityRobe, save.realmIndex, save.equipSlotEnhanceLevels[(int)RemakeEquipSlot.Robe]);
                Assert.Greater(engine.PlayerMaxHp, hpBefore, "Enhancing defensive slot must boost Max HP");
                Assert.Greater(scoreAfterRobe, scoreBefore, "Enhancing slot must visibly boost Lực TB score");
                Assert.Greater(powerAfterRobe, powerBefore, "Enhancing defensive slot must visibly increase Lực Chiến");

                // 2. Enhance Offensive slot (Crown, even when empty) -> increases Sword ATK AND Combat Power!
                double atkBefore = engine.GetTotalSwordAttackPower();
                Assert.IsTrue(engine.TryEnhanceEquipSlot((int)RemakeEquipSlot.Crown));
                Assert.Greater(engine.GetTotalSwordAttackPower(), atkBefore, "Enhancing offensive slot must boost Sword ATK");
                double powerAfterCrown = engine.GetPlayerCombatPowerEstimate();
                Assert.Greater(powerAfterCrown, powerAfterRobe, "Enhancing offensive slot must visibly increase Lực Chiến");

                // 3. Upgrading Combat Skill also increases Combat Power!
                Assert.IsTrue(engine.TryUpgradeSkill(0, true));
                Assert.Greater(engine.GetPlayerCombatPowerEstimate(), powerAfterCrown, "Upgrading unlocked skill must boost Lực Chiến");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IdleVoiceScheduler_P4PriorityAndNonRepeatingShuffleWork()
        {
            Assert.Greater(RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.IdleLine1),
                RemakeAudioService.ResolveVoicePriority(RemakeVoiceLineId.SkillVanKiem),
                "Ambient idle voices must have lower priority (P4) than skills (P3) and breakthroughs (P1)");
            Assert.That(RemakeAudioService.ResolveDuckMode(RemakeVoiceLineId.IdleLine2), Is.EqualTo(RemakeVoiceDuckMode.Standard));
            Assert.That(AudioDropLoader.GetVoiceKey(RemakeVoiceLineId.IdleLine1, RemakeCharacterGender.Male), Is.EqualTo("Voice/Male/vo_male_idle_01"));
            Assert.That(AudioDropLoader.GetVoiceKey(RemakeVoiceLineId.IdleLine4, RemakeCharacterGender.Female), Is.EqualTo("Voice/Female/vo_female_idle_04"));

            // Non-repeating shuffle check
            int last = 2;
            var picked = RemakeAudioService.PickNextIdleVoiceLine(last, 2, out int nextIdx);
            Assert.AreNotEqual(last, nextIdx, "Idle voice picker must never repeat the same clip back-to-back");
            Assert.That(picked, Is.EqualTo(RemakeVoiceLineId.IdleLine4));
        }
    }
}

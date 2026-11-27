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
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KhiVanChiTu.Remake.Tests
{
    public sealed class RemakeRecoveryRegressionTests
    {
        private string _profile;
        private GameObject _engineObject;

        [SetUp]
        public void SetUp()
        {
            _profile = "qa-recovery-" + Guid.NewGuid().ToString("N").Substring(0, 24);
            SaveService.SetProfileOverride(_profile);
        }

        [TearDown]
        public void TearDown()
        {
            if (_engineObject != null) UnityEngine.Object.DestroyImmediate(_engineObject);
            SaveService.DeleteSaveFilesForTesting();
            SaveService.SetProfileOverride(null);
        }

        [Test]
        public void InvalidPrimaryUsesBackupAndKeepsItOnNextWrite()
        {
            var save = RemakeSaveData.CreateFullTestSave(false);
            save.daoName = "Backup canary";
            SaveService.Save(save);
            save.daoName = "Primary canary";
            SaveService.Save(save);

            string path = SaveService.GetSaveFilePath();
            string backup = File.ReadAllText(path + ".bak");
            File.WriteAllText(path, "{}");
            var recovered = SaveService.Load();
            Assert.That(recovered.daoName, Is.EqualTo("Backup canary"));

            recovered.daoName = "Recovered canary";
            SaveService.Save(recovered);
            Assert.That(SaveService.Load().daoName, Is.EqualTo("Recovered canary"));
            Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo(backup));
        }

        [Test]
        public void FutureSaveIsPreservedOnSaveAndReset()
        {
            SaveService.Save(RemakeSaveData.CreateFullTestSave(false));
            var newer = RemakeSaveData.CreateFullTestSave(false);
            newer.schemaVersion = SaveService.CurrentSchemaVersion + 1;
            string path = SaveService.GetSaveFilePath();
            string original = JsonUtility.ToJson(newer);
            File.WriteAllText(path, original);

            Assert.That(SaveService.Load().hasCreatedCharacter, Is.False);
            Assert.That(SaveService.IsActiveSaveFromNewerVersion, Is.True);
            SaveService.Save(RemakeSaveData.CreateDefault(false));
            SaveService.ResetCurrentSave();
            Assert.That(File.ReadAllText(path), Is.EqualTo(original));
        }

        [Test]
        public void FailedWriteLeavesPrimaryAndBackupUntouched()
        {
            SaveService.Save(RemakeSaveData.CreateFullTestSave(false));
            SaveService.Save();
            string path = SaveService.GetSaveFilePath();
            string primary = File.ReadAllText(path);
            string backup = File.ReadAllText(path + ".bak");
            var save = SaveService.Current;
            long successfulSaveAt = save.lastSavedUtcTicks;
            using (File.Open(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                save.linhThach++;
                SaveService.Save();
            }
            Assert.That(File.ReadAllText(path), Is.EqualTo(primary));
            Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo(backup));
            Assert.That(save.lastSavedUtcTicks, Is.EqualTo(successfulSaveAt));
        }

        [Test]
        public void BossKillCommitsStatsAndLootAtEndOfAoEBatch()
        {
            var save = RemakeSaveData.CreateFullTestSave(false);
            save.activeStage = 10;
            save.activeAct = 0;
            save.activeDifficulty = 0;
            save.highestUnlockedLinearStage = 9;
            SaveService.Save(save);

            var engine = ReopenEngine();
            int kills = save.totalKills;
            int swords = save.swordInventoryLevels.Count;
            int gear = save.equipmentBag.Count;
            engine.BeginAoEDamageBatch();
            engine.OnMonsterKilled(null, true);
            engine.EndAoEDamageBatch();

            var after = SaveService.Load();
            Assert.That(after.totalKills, Is.EqualTo(kills + 1));
            Assert.That(after.swordInventoryLevels.Count, Is.EqualTo(swords + 1));
            Assert.That(after.equipmentBag.Count, Is.EqualTo(gear + 1));
            Assert.That(after.bossAct1Cleared, Is.True);
            Assert.That(after.highestUnlockedLinearStage, Is.EqualTo(10));
        }

        [Test]
        public void PaidBrewResumesAndGrantsExactlyOneResult()
        {
            SaveService.Save(RemakeSaveData.CreateFullTestSave(false));
            var engine = ReopenEngine();
            var save = SaveService.Current;
            int herbs = save.herbsByTier[0];
            int pills = save.pills.Sum(stack => stack.count);
            Assert.That(engine.TryStartBrewPill(0), Is.True);
            engine.AdvanceBrewTimer(1f);
            SaveService.Save();

            engine = ReopenEngine();
            Assert.That(engine.IsBrewingPill, Is.True);
            Assert.That(engine.BrewTimeRemaining, Is.EqualTo(2.5f).Within(0.001f));
            engine.AdvanceBrewTimer(4f);

            engine = ReopenEngine();
            engine.AdvanceBrewTimer(4f);
            Assert.That(engine.IsBrewingPill, Is.False);
            Assert.That(SaveService.Current.herbsByTier[0], Is.EqualTo(herbs - CultivationTable.GetPillRequiredHerbCount(0)));
            Assert.That(SaveService.Current.pills.Sum(stack => stack.count), Is.EqualTo(pills + 1));
        }

        [Test]
        public void MajorTribulationResumesWithoutChargingOrRerolling()
        {
            var initial = RemakeSaveData.CreateFullTestSave(false);
            initial.subStage = 10;
            initial.tuVi = CultivationTable.GetRequiredTuVi(initial.realmIndex, initial.subStage);
            SaveService.Save(initial);
            var engine = ReopenEngine();
            int pills = engine.GetPillCount(initial.realmIndex, RemakePillQuality.CucPham);
            Assert.That(engine.TryStartTribulation(RemakePillQuality.CucPham), Is.True);
            bool plannedSuccess = SaveService.Current.pendingTribulationSuccess;
            engine.AdvanceTribulation(4f);
            int strikesBeforeClosing = engine.TribulationStrikesDone;
            SaveService.Save();

            engine = ReopenEngine();
            Assert.That(engine.IsInTribulation, Is.True);
            Assert.That(engine.TribulationStrikesDone, Is.EqualTo(strikesBeforeClosing));
            bool? result = null;
            engine.OnBreakthroughResolved += (success, _) => result = success;
            engine.AdvanceTribulation(15f);
            Assert.That(result, Is.EqualTo(plannedSuccess));

            engine = ReopenEngine();
            Assert.That(engine.IsInTribulation, Is.False);
            Assert.That(engine.GetPillCount(initial.realmIndex, RemakePillQuality.CucPham), Is.EqualTo(pills - 1));
        }

        [Test]
        public void EveryFlyingSwordTierPlacesTheBladeTipAtTheMonster()
        {
            const string prefabPath = "Assets/_Game/Prefabs/Player/RemakeFlyingSword.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<RemakeFlyingSwordActor>(prefabPath);
            Assert.That(prefab, Is.Not.Null);
            var sword = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var mesh = sword.GetComponentInChildren<MeshFilter>();
                var target = new Vector3(2f, 3f, -6f);
                foreach (var incoming in new[] { Vector3.right, Vector3.back, Vector3.up, new Vector3(-3f, 2f, 1f).normalized })
                {
                    for (int tier = 1; tier <= 10; tier++)
                    {
                        sword.Configure(0, 1, tier, null);
                        Assert.That(mesh.sharedMesh, Is.Not.Null, $"Tier {tier}");
                        CallSwordMethod(sword, "FaceFlightDirection", incoming);
                        CallSwordMethod(sword, "ApplyVisualSpinRoll", 0.037f, 980f, 0f);
                        CallSwordMethod(sword, "PositionBladeTipAt", target);

                        float tipZ = mesh.sharedMesh.bounds.min.z;
                        float handleZ = mesh.sharedMesh.bounds.max.z;
                        Vector3 tip = mesh.transform.TransformPoint(new Vector3(0f, 0f, tipZ));
                        Vector3 handle = mesh.transform.TransformPoint(new Vector3(0f, 0f, handleZ));
                        Assert.That(Vector3.Distance(tip, target), Is.LessThan(0.001f), $"Tier {tier}");
                        Assert.That(Vector3.Dot(tip - handle, incoming), Is.GreaterThan(0.5f), $"Tier {tier}");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sword.gameObject);
            }
        }

        [Test]
        public void StandardVisualMeshesAreAvailableToPlayer()
        {
            Assert.That(Resources.GetBuiltinResource<Mesh>("Quad.fbx"), Is.Not.Null);
            Assert.That(Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"), Is.Not.Null);
        }
        private static void CallSwordMethod(RemakeFlyingSwordActor sword, string method, params object[] args)
        {
            typeof(RemakeFlyingSwordActor).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sword, args);
        }

        private GameplayEngine ReopenEngine()
        {
            if (_engineObject != null) UnityEngine.Object.DestroyImmediate(_engineObject);
            SaveService.Load();
            _engineObject = new GameObject("QA engine");
            var engine = _engineObject.AddComponent<GameplayEngine>();
            engine.ReloadFromActiveProfile();
            return engine;
        }
    }
}

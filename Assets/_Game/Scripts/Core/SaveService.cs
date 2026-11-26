using System;
using System.IO;
using System.Runtime.InteropServices;
using KhiVanChiTu.Collaboration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.Remake
{
    public static class SaveService
    {
        public const string BaseSaveFileName = "KhiVanChiTu_RemakeSave.json";
        public const string TestAllProfileId = "test_all";
        private static string _profileOverride;
        private static RemakeSaveData _cached;

        public static bool IsTestAllProfileActive => string.Equals(_profileOverride, TestAllProfileId, StringComparison.Ordinal);

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLBridge_SyncIndexedDb();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _cached = null;
            _profileOverride = null;
        }

        public static void SetProfileOverride(string profileId)
        {
            _profileOverride = string.IsNullOrEmpty(profileId) ? null : profileId;
            _cached = null;
        }

        public static string ResolveProfileId()
        {
            if (!string.IsNullOrEmpty(_profileOverride))
                return _profileOverride;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                string profile = WorkspaceSaveProfile.Resolve(scene);
                if (!string.IsNullOrEmpty(profile))
                    return profile;
            }
            return string.Empty;
        }

        public static string GetSaveFilePath()
        {
            string profile = ResolveProfileId();
            string fileName = WorkspaceSaveProfile.FileName(BaseSaveFileName, profile);
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static RemakeSaveData Current
        {
            get
            {
                if (_cached == null) _cached = Load();
                return _cached;
            }
        }

        public static RemakeSaveData Load()
        {
            string path = GetSaveFilePath();
            RemakeSaveData loaded = TryReadJson(path) ?? TryReadJson(path + ".bak");
            bool isTouch = PlatformWebBridge.IsTouchPlatform();

            if (loaded == null)
            {
                loaded = RemakeSaveData.CreateDefault(isTouch);
            }
            else
            {
                Migrate(loaded, isTouch);
            }

            _cached = loaded;
            return loaded;
        }

        public static void Save(RemakeSaveData data = null)
        {
            RemakeSaveData target = data ?? Current;
            target.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            if (target.lastTickUtcTicks <= 0)
                target.lastTickUtcTicks = target.lastSavedUtcTicks;

            string path = GetSaveFilePath();
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonUtility.ToJson(target, true);
            string tmpPath = path + ".tmp";
            string bakPath = path + ".bak";

            try
            {
                File.WriteAllText(tmpPath, json);
                if (File.Exists(path))
                {
                    if (File.Exists(bakPath)) File.Delete(bakPath);
                    File.Move(path, bakPath);
                }
                File.Move(tmpPath, path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Atomic save fallback used ({ex.Message})");
                File.WriteAllText(path, json);
            }

            _cached = target;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLBridge_SyncIndexedDb();
#endif
        }

        public static void ResetCurrentSave(bool keepGraphics = true)
        {
            RemakeGraphicsSettingsData oldGfx = keepGraphics && _cached != null ? _cached.graphics : null;
            RemakeSaveData fresh = RemakeSaveData.CreateDefault(PlatformWebBridge.IsTouchPlatform());
            if (oldGfx != null) fresh.graphics = oldGfx;
            _cached = fresh;
            Save(fresh);
        }

        public static RemakeSaveData SeedOrSelectTestAllAccount(bool forceReset = false)
        {
            RemakeGraphicsSettingsData oldGfx = _cached != null ? _cached.graphics : null;
            _profileOverride = TestAllProfileId;
            _cached = null;
            RemakeSaveData existing = Load();
            if (forceReset || !existing.hasCreatedCharacter)
            {
                RemakeSaveData full = RemakeSaveData.CreateFullTestSave(PlatformWebBridge.IsTouchPlatform(), oldGfx ?? existing.graphics);
                _cached = full;
                Save(full);
            }
            return Current;
        }

        public static bool DeleteSaveFilesForTesting()
        {
            string path = GetSaveFilePath();
            bool deleted = false;
            foreach (string candidate in new[] { path, path + ".bak", path + ".tmp" })
            {
                if (File.Exists(candidate))
                {
                    File.Delete(candidate);
                    deleted = true;
                }
            }
            _cached = null;
            return deleted;
        }

        private static RemakeSaveData TryReadJson(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                string raw = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var parsed = JsonUtility.FromJson<RemakeSaveData>(raw);
                if (parsed == null || parsed.schemaVersion <= 0) return null;
                return parsed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Could not parse save at {filePath}: {ex.Message}");
                return null;
            }
        }

        private static void Migrate(RemakeSaveData data, bool isTouch)
        {
            if (data.herbsByTier == null || data.herbsByTier.Length != 6)
                data.herbsByTier = new int[6];
            if (data.swordPityByLevel == null || data.swordPityByLevel.Length != 10)
                data.swordPityByLevel = new float[10];
            if (data.swordInventoryLevels == null)
                data.swordInventoryLevels = new System.Collections.Generic.List<int>();
            if (data.equippedSwordLevels == null)
                data.equippedSwordLevels = new System.Collections.Generic.List<int>();
            if (data.equippedSwordLevels.Count == 0)
                data.equippedSwordLevels.Add(1);
            if (data.daoVanNodes == null)
                data.daoVanNodes = new System.Collections.Generic.List<RemakeDaoVanSaveEntry>();
            if (data.pills == null)
                data.pills = new System.Collections.Generic.List<RemakePillStackEntry>();
            if (data.graphics == null)
                data.graphics = RemakeSaveData.CreateDefault(isTouch).graphics;
            if (data.measuredKillsPerSec <= 0.01)
                data.measuredKillsPerSec = 0.45;
            if (data.alchemistRank < 1)
                data.alchemistRank = 1;
            if (data.spiritFlameTier < 1)
                data.spiritFlameTier = 1;
            if (data.equipSlotEnhanceLevels == null || data.equipSlotEnhanceLevels.Length != 6)
                data.equipSlotEnhanceLevels = new int[6];
            if (data.equippedGear == null)
                data.equippedGear = new System.Collections.Generic.List<RemakeEquipmentItem>();
            if (data.equipmentBag == null)
                data.equipmentBag = new System.Collections.Generic.List<RemakeEquipmentItem>();
            if (data.yinYangLevel < 1) data.yinYangLevel = 1;
            if (data.swordRainLevel < 1) data.swordRainLevel = 1;
            if (data.palmLevel < 1) data.palmLevel = 1;
            if (data.schemaVersion < 2)
            {
                data.forgeTalismanCount = Math.Max(data.forgeTalismanCount, 1);
                data.reforgeStoneCount = Math.Max(data.reforgeStoneCount, 1);
                data.skillScrollCount = Math.Max(data.skillScrollCount, 1);
                if (data.activeMainFormation == 0 && !CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, data.realmIndex, data.subStage))
                    data.activeMainFormation = -1;
                if (data.activeSubFormation == 0 && !CultivationTable.IsFeatureUnlocked(RemakeFeatureId.FormationSubSlot, data.realmIndex, data.subStage))
                    data.activeSubFormation = -1;
            }
            if (data.secretRealmClearedTier == null || data.secretRealmClearedTier.Length != 3)
                data.secretRealmClearedTier = new int[3];
            if (data.schemaVersion < 3)
            {
                data.autoChallengeBoss = true;
                data.secretRealmDailyRemaining = 3;
            }
            data.schemaVersion = 3;
        }
    }
}

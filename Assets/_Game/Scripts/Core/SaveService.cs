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
        public const int CurrentSchemaVersion = 4;
        private static string _profileOverride;
        private static RemakeSaveData _cached;
        private static string _loadedSavePath;
        private static bool _primaryIsKnownGood;

        public static bool IsActiveSaveFromNewerVersion { get; private set; }

        public static bool IsTestAllProfileActive => string.Equals(_profileOverride, TestAllProfileId, StringComparison.Ordinal);

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLBridge_SyncIndexedDb();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _profileOverride = null;
            ClearLoadedSave();
        }

        public static void SetProfileOverride(string profileId)
        {
            _profileOverride = string.IsNullOrEmpty(profileId) ? null : profileId;
            ClearLoadedSave();
        }

        private static void ClearLoadedSave()
        {
            _cached = null;
            _loadedSavePath = null;
            _primaryIsKnownGood = false;
            IsActiveSaveFromNewerVersion = false;
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
            RemakeSaveData primary = TryReadJson(path);
            RemakeSaveData loaded = primary ?? TryReadJson(path + ".bak");
            bool isTouch = PlatformWebBridge.IsTouchPlatform();

            _loadedSavePath = path;
            _primaryIsKnownGood = primary != null;
            IsActiveSaveFromNewerVersion = loaded != null && loaded.schemaVersion > CurrentSchemaVersion;
            if (IsActiveSaveFromNewerVersion)
                Debug.LogWarning("[SaveService] Save data requires a newer game version.");

            if (loaded == null || IsActiveSaveFromNewerVersion)
                loaded = RemakeSaveData.CreateDefault(isTouch);
            else
                Migrate(loaded, isTouch);

            _cached = loaded;
            return loaded;
        }

        public static void Save(RemakeSaveData data = null)
        {
            string path = GetSaveFilePath();
            if (_loadedSavePath != path)
                Load();
            if (IsActiveSaveFromNewerVersion) return;

            RemakeSaveData target = data ?? Current;
            long savedAt = DateTime.UtcNow.Ticks;
            long previousSavedAt = target.lastSavedUtcTicks;
            long previousTick = target.lastTickUtcTicks;
            target.lastSavedUtcTicks = savedAt;
            if (target.lastTickUtcTicks <= 0)
                target.lastTickUtcTicks = savedAt;

            string tmpPath = path + ".tmp";
            string bakPath = path + ".bak";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false), 4096, true))
                        writer.Write(JsonUtility.ToJson(target));
#if UNITY_WEBGL && !UNITY_EDITOR
                    stream.Flush();
#else
                    stream.Flush(true);
#endif
                }

                if (File.Exists(path))
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    // MEMFS changes are flushed to IndexedDB as one transaction after the rename.
                    if (_primaryIsKnownGood)
                    {
                        File.Delete(bakPath);
                        File.Move(path, bakPath);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                    File.Move(tmpPath, path);
#else
                    File.Replace(tmpPath, path, _primaryIsKnownGood ? bakPath : null);
#endif
                }
                else
                {
                    File.Move(tmpPath, path);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                target.lastSavedUtcTicks = previousSavedAt;
                target.lastTickUtcTicks = previousTick;
                Debug.LogWarning($"[SaveService] Could not write save: {ex.Message}");
                return;
            }

            _cached = target;
            _primaryIsKnownGood = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLBridge_SyncIndexedDb();
#endif
        }

        public static void ResetCurrentSave(bool keepGraphics = true)
        {
            _ = Current;
            if (IsActiveSaveFromNewerVersion) return;
            RemakeGraphicsSettingsData oldGfx = keepGraphics && _cached != null ? _cached.graphics : null;
            RemakeSaveData fresh = RemakeSaveData.CreateDefault(PlatformWebBridge.IsTouchPlatform());
            if (oldGfx != null) fresh.graphics = oldGfx;
            _cached = fresh;
            Save(fresh);
        }

        public static RemakeSaveData SeedOrSelectTestAllAccount(bool forceReset = false)
        {
            RemakeGraphicsSettingsData oldGfx = _cached != null ? _cached.graphics : null;
            SetProfileOverride(TestAllProfileId);
            RemakeSaveData existing = Load();
            if (!IsActiveSaveFromNewerVersion && (forceReset || !existing.hasCreatedCharacter))
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
            ClearLoadedSave();
            return deleted;
        }

        private static RemakeSaveData TryReadJson(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                string raw = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(raw)) return null;
                // Use a sentinel so a JSON object without a schema cannot appear to be a valid v1 save.
                var parsed = new RemakeSaveData { schemaVersion = 0 };
                JsonUtility.FromJsonOverwrite(raw, parsed);
                if (parsed.schemaVersion <= 0) return null;
                if (parsed.schemaVersion <= CurrentSchemaVersion &&
                    (parsed.hasCreatedCharacter ? string.IsNullOrWhiteSpace(parsed.daoName) : parsed.createdUtcTicks <= 0))
                    return null;
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
            data.schemaVersion = CurrentSchemaVersion;
        }
    }
}

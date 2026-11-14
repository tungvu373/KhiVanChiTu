using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// Runs the real storage UI/services in an unsaved, empty scene. No DataManager,
// save profile, PlayerPrefs or player inventory is used by this regression.
[InitializeOnLoad]
public static class StorageDismantleRegression
{
    const string ReturnSceneKey = "StorageRegression.ReturnScene";
    const string LabelKey = "StorageRegression.Label";
    public const string OutputDirectory = "Assets/.docs/StorageCrashRegression";

    static StorageDismantleRegression()
    {
        EditorApplication.playModeStateChanged += RestoreScene;
    }

    [MenuItem("TuTien/QA/Storage Crash/Run Isolated Regression")]
    public static void Run() => Prepare("fixed");

    public static void Prepare(string label)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before starting the isolated regression.");
        if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("The regression requires one saved, clean source scene.");
        var source = SceneManager.GetActiveScene();
        var ui = UnityEngine.Object.FindFirstObjectByType<StorageModalUI>(FindObjectsInactive.Include);
        if (ui == null) throw new InvalidOperationException("No storage UI in the source scene.");
        SessionState.SetString(ReturnSceneKey, source.path);
        SessionState.SetString(LabelKey, label);
        var fixture = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(fixture);
        var canvas = new GameObject("StorageRegressionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        var clone = UnityEngine.Object.Instantiate(ui.gameObject, canvas.transform, false);
        clone.SetActive(false);
        new GameObject("StorageRegressionProbe").AddComponent<StorageDismantleRegressionProbe>();
        EditorSceneManager.CloseScene(source, true);
        EditorApplication.isPlaying = true;
    }

    public static string Label => SessionState.GetString(LabelKey, "fixed");

    static void RestoreScene(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        string path = SessionState.GetString(ReturnSceneKey, "");
        if (string.IsNullOrEmpty(path)) return;
        SessionState.EraseString(ReturnSceneKey);
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }
}

[DefaultExecutionOrder(-10000)]
public sealed class StorageDismantleRegressionProbe : MonoBehaviour
{
    [Serializable] public class Sample
    {
        public string mode;
        public int pendingBefore, pendingAfter, notifications, rowsImmediatelyAfter, rowsAfterFrame;
        public double openMs, batchMs;
        public long batchAllocatedBytes;
        public float stoneReward;
        public bool passed;
    }
    [Serializable] public class Report
    {
        public string label, unityVersion;
        public List<Sample> samples = new List<Sample>();
        public List<string> checks = new List<string>();
        public string failure;
        public bool passed;
    }

    StorageManager storage;
    DismantleService dismantle;
    AlchemyService alchemy;
    EconomyManager economy;
    StorageModalUI ui;
    Report report;
    int notifications;
    Exception failure;

    void Awake()
    {
        var services = new GameObject("IsolatedStorageServices");
        storage = services.AddComponent<StorageManager>();
        dismantle = services.AddComponent<DismantleService>();
        alchemy = services.AddComponent<AlchemyService>();
        economy = services.AddComponent<EconomyManager>();
        storage.OnStorageChanged += () => notifications++;
        ui = UnityEngine.Object.FindFirstObjectByType<StorageModalUI>(FindObjectsInactive.Include);
        Application.runInBackground = true;
    }

    IEnumerator Start()
    {
        report = new Report { label = StorageDismantleRegression.Label, unityVersion = Application.unityVersion };
        var sequence = RunSamples();
        while (true)
        {
            bool more;
            try { more = sequence.MoveNext(); }
            catch (Exception ex) { failure = ex; break; }
            if (!more) break;
            yield return sequence.Current;
        }
        report.failure = failure?.ToString();
        report.passed = failure == null && report.samples.TrueForAll(s => s.passed);
        Directory.CreateDirectory(StorageDismantleRegression.OutputDirectory);
        File.WriteAllText(Path.Combine(StorageDismantleRegression.OutputDirectory, report.label + ".json"), JsonUtility.ToJson(report, true));
        Debug.Log("[StorageRegression] " + JsonUtility.ToJson(report));
        EditorApplication.isPlaying = false;
    }

    IEnumerator RunSamples()
    {
        bool baseline = report.label == "baseline";
        int[] sizes = baseline ? new[] { 8, 16, 32 } : new[] { 8, 16, 32, 775, 10000 };
        foreach (StorageViewMode mode in Enum.GetValues(typeof(StorageViewMode)))
        foreach (int size in sizes)
        {
            storage.State.pendingLoot.Clear();
            // Four valuable survivors must remain, in their original order.
            for (int i = 0; i < 4; i++)
                storage.State.pendingLoot.Add(new PendingLootItem(PendingLootType.Sword, "Keep " + i, 3, 1, 1));
            for (int i = 0; i < size; i++)
                storage.State.pendingLoot.Add(new PendingLootItem(PendingLootType.Sword, "Thiết Kiếm", 1, 1, 1));
            alchemy.State.pills[0] = 1000000;
            alchemy.State.pills[1] = 7;
            alchemy.State.medicineDust = 0;
            economy.currentLinhThach = 0;
            ui.currentViewMode = mode;
            var watch = Stopwatch.StartNew();
            ui.Show(StorageCategory.All);
            watch.Stop();
            var sample = new Sample { mode = mode.ToString(), pendingBefore = size + 4, openMs = watch.Elapsed.TotalMilliseconds };
            yield return null; // Let UI.Start subscribe, and old rows be destroyed.
            if (!baseline && size == 775)
            {
                ScreenCapture.CaptureScreenshot(Path.Combine(StorageDismantleRegression.OutputDirectory, mode + ".png"));
                yield return null;
                yield return null;
            }
            notifications = 0;
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            UnityEngine.Profiling.Profiler.BeginSample("StorageRegression.BatchDismantle");
            watch.Restart();
            DismantleReward reward;
            try { reward = dismantle.BatchDismantleLowTier(); }
            finally { watch.Stop(); UnityEngine.Profiling.Profiler.EndSample(); }
            sample.batchMs = watch.Elapsed.TotalMilliseconds;
            sample.batchAllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
            sample.notifications = notifications;
            sample.pendingAfter = storage.PendingCount;
            sample.rowsImmediatelyAfter = ui.contentContainer.childCount;
            sample.stoneReward = reward.linhThach;
            yield return null;
            yield return null;
            sample.rowsAfterFrame = ui.contentContainer.childCount;
            sample.passed = storage.PendingCount == 4 && alchemy.State.pills[0] == 0 && alchemy.State.pills[1] == 7
                && alchemy.State.medicineDust == 2000000 && Mathf.Approximately(reward.linhThach, 10000000f + size * 60f)
                && (baseline || (sample.notifications == 1 && sample.rowsImmediatelyAfter <= 51 && sample.rowsAfterFrame <= 51));
            for (int i = 0; i < 4; i++) sample.passed &= storage.State.pendingLoot[i].displayName == "Keep " + i;
            report.samples.Add(sample);
            if (!sample.passed) throw new InvalidOperationException("Sample failed: " + JsonUtility.ToJson(sample));
            // A second tap must never grant any of the previous rewards again.
            var duplicate = dismantle.BatchDismantleLowTier();
            if (!duplicate.IsEmpty) throw new InvalidOperationException("Repeated batch granted a duplicate reward.");
            yield return null;
        }
        if (!baseline)
        {
            var checks = RunFunctionalChecks();
            while (checks.MoveNext()) yield return checks.Current;
        }
    }

    void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        report.checks.Add(description);
    }

    IEnumerator RunFunctionalChecks()
    {
        storage.State.pendingLoot.Clear();
        Array.Clear(alchemy.State.pills, 0, alchemy.State.pills.Length);
        alchemy.State.medicineDust = 0;
        economy.currentLinhThach = 0;
        var merge = new GameObject("RegressionMerge").AddComponent<MergeManager>();
        merge.equipPanel = new GameObject("Equipped", typeof(RectTransform)).transform;
        var equipped = MakeSword(merge.equipPanel, 1, 2);
        var page = new GameObject("InventoryPage", typeof(RectTransform)).transform;
        merge.inventoryPages = new[] { page };
        var keptSwords = new List<MergeItem>();
        for (int i = 0; i < 8; i++)
        {
            var slot = new GameObject("Slot" + i, typeof(RectTransform), typeof(UnityEngine.UI.Image)).transform;
            slot.SetParent(page);
            var sword = MakeSword(slot, i < 6 ? 1 : 2, 1);
            if (i >= 6) keptSwords.Add(sword);
        }
        for (int r = 0; r < 9; r++)
        for (int rec = 0; rec < 2; rec++)
        {
            alchemy.State.pills[r * 8 + rec * 4] = 2;
            alchemy.State.pills[r * 8 + rec * 4 + 1] = 3;
        }
        var low = new PendingLootItem(PendingLootType.Sword, "Low", 1, 3, 1);
        var high = new PendingLootItem(PendingLootType.Sword, "High", 2, 2, 1);
        storage.State.pendingLoot.AddRange(new[] { low, null, high,
            new PendingLootItem(0, 0, 0, 5), new PendingLootItem(1, 1, 2, 6),
            new PendingLootItem(PendingLootType.Herb, "Herb", 2, 1, 100),
            new PendingLootItem(PendingLootType.Ore, "Ore", 1, 1, 4) });
        yield return null;
        ui.Show(StorageCategory.All);
        yield return null;
        int sorts = 0;
        Application.LogCallback counter = (message, stack, type) => { if (message.StartsWith("[Merge] Đã sắp xếp")) sorts++; };
        Action reenter = () => Check(dismantle.BatchDismantleLowTier().IsEmpty, "Reentrant batch grants no extra reward");
        storage.OnStorageChanged += reenter;
        dismantle.OnDismantleExecuted += reenter;
        Application.logMessageReceived += counter;
        try { ui.btnBatchDismantle.onClick.Invoke(); }
        finally
        {
            storage.OnStorageChanged -= reenter;
            dismantle.OnDismantleExecuted -= reenter;
            Application.logMessageReceived -= counter;
        }
        yield return null;
        yield return null;
        Check(sorts == 1, "Batch sorts the sword inventory exactly once");
        Check(equipped != null && equipped.transform.IsChildOf(merge.equipPanel) && keptSwords.TrueForAll(s => s != null && s.level == 2),
            "Equipped Lv1 and higher-tier swords survive");
        Check(storage.PendingCount == 5 && storage.State.pendingLoot.Contains(high) && !storage.State.pendingLoot.Contains(low),
            "Mixed pending queue preserves null, valuable swords, pills, herbs and ore");
        Check(Mathf.Approximately(economy.currentLinhThach, 950) && alchemy.State.medicineDust == 82,
            "Actual footer button grants exactly 9 sword pieces and 41 Waste pills worth of rewards");
        for (int r = 0; r < 9; r++)
        for (int rec = 0; rec < 2; rec++)
            Check(alchemy.State.pills[r * 8 + rec * 4] == 0 && alchemy.State.pills[r * 8 + rec * 4 + 1] == 3,
                "Waste cleared and Basic preserved in realm/recipe " + r + "/" + rec);
        Check(dismantle.DismantlePendingItem(high), "Single pending dismantle succeeds for an owned item");
        float balance = economy.currentLinhThach;
        Check(!dismantle.DismantlePendingItem(high) && economy.currentLinhThach == balance,
            "Stale pending item cannot be dismantled twice");

        var locked = new GameObject("LockedFeatureGate").AddComponent<FeatureUnlockService>();
        storage.State.pendingLoot.Add(low);
        alchemy.State.pills[0] = 7;
        Check(dismantle.BatchDismantleLowTier().IsEmpty && storage.State.pendingLoot.Contains(low) && alchemy.State.pills[0] == 7,
            "Locked batch preserves items and resources");
        Destroy(locked.gameObject);
        yield return null;

        // Visit every page through its real buttons, including a short last page.
        foreach (StorageViewMode mode in Enum.GetValues(typeof(StorageViewMode)))
        {
            storage.State.pendingLoot.Clear();
            for (int i = 0; i < 107; i++) storage.State.pendingLoot.Add(new PendingLootItem(PendingLootType.Sword, "P" + i, 3, 1, 1));
            ui.currentViewMode = mode;
            ui.Show(StorageCategory.Pending);
            yield return null;
            var names = new HashSet<string>();
            for (int p = 0; p < 3; p++)
            {
                foreach (var text in ui.contentContainer.GetComponentsInChildren<UnityEngine.UI.Text>())
                {
                    string value = System.Text.RegularExpressions.Regex.Replace(text.text, "<[^>]+>", "");
                    foreach (var item in storage.State.pendingLoot)
                        if (value == "Hòm\n" + item.displayName || value.StartsWith("[Hòm Tạm] " + item.displayName + "\n")) names.Add(item.displayName);
                }
                Check(ui.contentContainer.childCount <= 51, mode + " page " + p + " stays bounded");
                if (p < 2)
                {
                    FindPagerPart(mode, "StorageNextPage").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    yield return null;
                    yield return null;
                }
            }
            Check(names.Count == 107, mode + " pagination exposes all 107 items");
            FindPagerPart(mode, "StoragePreviousPage").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            yield return null;
            Check(FindPagerPart(mode, "StoragePageNumber").GetComponentInChildren<UnityEngine.UI.Text>().text == "Trang 2/3",
                mode + " previous-page button navigates correctly");
            Canvas.ForceUpdateCanvases();
            ui.scrollRect.verticalNormalizedPosition = 0;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(StorageDismantleRegression.OutputDirectory, mode + "-pager.png"));
            yield return null;
            yield return null;
            storage.RemovePendingItems(item => true);
            yield return null;
            yield return null;
            Check(ui.contentContainer.childCount == 0 && ui.txtEmptyNotice.gameObject.activeSelf,
                mode + " shrinking the last page clamps to an empty first page");
        }
        // Replay only a deserialized COPY of the real pending queue. Nothing is
        // loaded through DataManager, and no player file is written or migrated.
        string playerFile = Path.Combine(Application.persistentDataPath, "KhiVanChiTu_SaveData.json");
        if (File.Exists(playerFile))
        {
            var snapshot = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(playerFile));
            for (int slotIndex = 0; slotIndex < snapshot.slots.Length; slotIndex++)
            {
                var pending = snapshot.slots[slotIndex].storage.pendingLoot;
                var survivors = new List<string>();
                int pieces = 0, pills = 0;
                foreach (var item in pending)
                {
                    if (item != null && item.itemType == PendingLootType.Sword && item.level == 1) pieces += Mathf.Max(1, item.pieces);
                    else if (item != null && item.itemType == PendingLootType.Pill && item.quality == 0 && item.amount > 0) pills += item.amount;
                    else survivors.Add(item?.id);
                }
                storage.State.pendingLoot.Clear();
                storage.State.pendingLoot.AddRange(pending);
                Array.Clear(alchemy.State.pills, 0, alchemy.State.pills.Length);
                var savedPills = snapshot.slots[slotIndex].alchemy?.pills;
                if (savedPills != null) Array.Copy(savedPills, alchemy.State.pills, Math.Min(savedPills.Length, alchemy.State.pills.Length));
                for (int r = 0; r < 9; r++)
                for (int rec = 0; rec < 2; rec++) pills += Mathf.Max(0, alchemy.State.pills[r * 8 + rec * 4]);
                var timer = Stopwatch.StartNew();
                ui.Show(StorageCategory.All);
                timer.Stop();
                var replay = new Sample { mode = "PlayerSnapshot" + slotIndex, pendingBefore = pending.Count, openMs = timer.Elapsed.TotalMilliseconds };
                yield return null;
                notifications = 0;
                timer.Restart();
                var reward = dismantle.BatchDismantleLowTier();
                timer.Stop();
                replay.batchMs = timer.Elapsed.TotalMilliseconds;
                replay.pendingAfter = storage.PendingCount;
                replay.notifications = notifications;
                replay.stoneReward = reward.linhThach;
                yield return null;
                yield return null;
                replay.rowsAfterFrame = ui.contentContainer.childCount;
                Check(storage.PendingCount == survivors.Count && Mathf.Approximately(reward.linhThach, pieces * 60f + pills * 10f),
                    "Read-only player snapshot replay removed the expected low-tier items from " + pending.Count + " rows");
                for (int i = 0; i < survivors.Count; i++)
                    if (storage.State.pendingLoot[i]?.id != survivors[i]) throw new InvalidOperationException("Snapshot survivor order changed.");
                Check(true, "Player snapshot preserves every surviving item ID and queue order");
                replay.passed = true;
                report.samples.Add(replay);
            }
        }
        Check(DataManager.Instance == null, "Regression never instantiated DataManager or a player save session");
    }

    Transform FindPagerPart(StorageViewMode mode, string part)
    {
        return ui.contentContainer.Find(mode == StorageViewMode.GridSlots ? part : "StoragePageNavigation/" + part);
    }

    static MergeItem MakeSword(Transform parent, int level, int pieces)
    {
        var go = new GameObject("Sword", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var sword = go.AddComponent<MergeItem>();
        sword.SetLevel(level, pieces);
        return sword;
    }
}

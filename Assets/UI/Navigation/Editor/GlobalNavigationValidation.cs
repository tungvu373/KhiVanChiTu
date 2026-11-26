using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KhiVanChiTu.Collaboration;
using KhiVanChiTu.UI.CombatHUD;
using KhiVanChiTu.UI.Foundation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.UI.Navigation.Editor
{
    [InitializeOnLoad]
    public static class GlobalNavigationValidation
    {
        const string Profile = "navigation-phase3-qa";
        const string Key = "GlobalNavP3QA.";
        public const string Evidence = "Assets/.docs/UIUX_Phase3_20261117/Evidence";
        public const string ScenePath = "Assets/Scenes/SampleScene_MobileWork.unity";

        static readonly Vector2Int[] Sizes = { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(720, 1280) };
        static readonly List<object> checks = new List<object>();
        static readonly List<object> errors = new List<object>();
        static readonly List<int> addedSizes = new List<int>();

        static EditorWindow gameView;
        static PropertyInfo selectedSizeProp;
        static object sizeGroup;
        static int previousSizeIndex;
        static float previousScale;

        static GlobalNavigationController nav;
        static int phase, sizeIndex, frame;
        static double readyAt, timeout;
        static bool running;

        static GlobalNavigationValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key + "Active", false))
            {
                EditorApplication.delayCall += Cleanup;
            }
        }

        static string Hash(string s) => Hash128.Compute(s).ToString();
        static string FileHash(string path) => File.Exists(path) ? Hash(Convert.ToBase64String(File.ReadAllBytes(path))) : "absent";
        static string ProductionFile => Path.Combine(Application.persistentDataPath, "KhiVanChiTu_SaveData.json");
        static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;

        [MenuItem("TuTien/UI Global Navigation/2. Apply Scene Fixes and Export After Inventory")]
        public static void ApplyFixesAndExportInventory()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open mobile scene in Edit Mode.");

            var navInstance = UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include);
            if (navInstance == null) throw new InvalidOperationException("GlobalNavigationController not found in active scene.");

            bool modified = false;
            if (navInstance.progressionSheet != null)
            {
                var pTitle = navInstance.progressionSheet.Find("BottomSheet/SheetTitle")?.GetComponent<TMP_Text>();
                if (pTitle != null && Mathf.Abs(pTitle.fontSize - 40f) > 0.01f)
                {
                    Undo.RecordObject(pTitle, "Set SheetTitle font 40");
                    pTitle.fontSize = 40f;
                    EditorUtility.SetDirty(pTitle);
                    modified = true;
                }
            }

            if (navInstance.suppliesSheet != null)
            {
                var sTitle = navInstance.suppliesSheet.Find("BottomSheet/SheetTitle")?.GetComponent<TMP_Text>();
                if (sTitle != null && Mathf.Abs(sTitle.fontSize - 40f) > 0.01f)
                {
                    Undo.RecordObject(sTitle, "Set SheetTitle font 40");
                    sTitle.fontSize = 40f;
                    EditorUtility.SetDirty(sTitle);
                    modified = true;
                }
            }

            // Ensure all NavigationIcon objects have CanvasRenderer (required for uGUI MaskableGraphic / RectMask2D)
            foreach (var icon in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<NavigationIcon>(true)))
            {
                if (icon.GetComponent<CanvasRenderer>() == null)
                {
                    Undo.AddComponent<CanvasRenderer>(icon.gameObject);
                    EditorUtility.SetDirty(icon.gameObject);
                    modified = true;
                }
            }

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Global Navigation] ProgressionSheet & SuppliesSheet SheetTitle font sizes synchronized to 40.");
            }

            // Export editor-after.json
            Directory.CreateDirectory(Evidence);
            var roots = scene.GetRootGameObjects();
            var all = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var data = new
            {
                scene = scene.path,
                dirty = scene.isDirty,
                compiling = EditorApplication.isCompiling,
                canvases = all.Select(t => t.GetComponent<Canvas>()).Where(c => c != null).Select(c => new { path = PathOf(c.transform), c.sortingOrder, c.overrideSorting, mode = c.renderMode.ToString() }),
                hud = all.Where(t => t.GetComponent<PortraitHudController>() != null || t.GetComponent<GlobalNavigationController>() != null || t.name.Contains("Drawer_") || t.name == "MobileUIRoot" || t.name.Contains("Modal") || t.name == "ShopPanel").Select(t => new { path = PathOf(t), active = t.gameObject.activeSelf, components = t.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name) }),
                buttons = all.Select(t => t.GetComponent<UnityEngine.UI.Button>()).Where(b => b != null).Select(b => new { path = PathOf(b.transform), active = b.gameObject.activeInHierarchy, id = GlobalObjectId.GetGlobalObjectIdSlow(b).ToString(), events = Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Select(i => new { method = b.onClick.GetPersistentMethodName(i), target = b.onClick.GetPersistentTarget(i)?.name }) })
            };
            File.WriteAllText(Evidence + "/editor-after.json", Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
            Debug.Log("[Global Navigation] Exported editor-after.json successfully.");
        }

        [MenuItem("TuTien/UI Global Navigation/3. Validate Navigation with Isolated Profile")]
        public static void StartValidation()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath || scene.isDirty)
                throw new InvalidOperationException("Open saved mobile scene in Edit Mode first.");
            if (UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include).Length != 0)
                throw new InvalidOperationException("Existing save-profile owner found. Keep it intact.");

            Directory.CreateDirectory(Evidence);
            checks.Clear();
            addedSizes.Clear();
            errors.Clear();
            Application.logMessageReceived += CollectLog;

            SessionState.SetString(Key + "Production", Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData", "")));
            SessionState.SetString(Key + "ProductionFile", FileHash(ProductionFile));
            SessionState.SetString(Key + "ProductionBak", FileHash(ProductionFile + ".bak"));
            SessionState.SetFloat(Key + "Scale", Time.timeScale);
            previousScale = Time.timeScale;
            SessionState.SetBool(Key + "Active", true);

            var marker = new GameObject("__Navigation_Phase3_QA_Temporary");
            marker.AddComponent<WorkspaceSaveProfile>().profileId = Profile;

            phase = 0;
            sizeIndex = 0;
            running = true;
            readyAt = EditorApplication.timeSinceStartup + 1;
            timeout = readyAt + 60;

            EditorApplication.update += Tick;
            EditorApplication.isPlaying = true;
        }

        static void RequireIsolation()
        {
            Expect(DataManager.Instance != null && DataManager.Instance.SaveProfileId == Profile, "QA must use the disposable navigation profile.");
        }

        static void Expect(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }

        static void Wait(int next, double seconds = 0.25)
        {
            phase = next;
            frame = Time.frameCount;
            readyAt = EditorApplication.timeSinceStartup + seconds;
            timeout = readyAt + 30;
        }

        static void Tick()
        {
            if (!running) return;
            try
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup > timeout)
                    throw new TimeoutException("Navigation validation timed out at phase " + phase);
                if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < readyAt) return;

                if (phase == 0)
                {
                    if (DataManager.Instance == null || MainMenuManager.Instance == null) return;
                    RequireIsolation();
                    nav = UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include);
                    Expect(nav != null, "GlobalNavigationController missing in scene");

                    var slot = new CharacterSaveData
                    {
                        isSlotEmpty = false,
                        characterName = "Khí Vận Đạo Hữu - QA Phase 3",
                        linhThach = 999999999,
                        coDuyen = 888888,
                        stageIndex = 15,
                        realmId = 4,
                        subLevel = 2,
                        combatStage = 5,
                        playerMaxHp = 50000,
                        playerCurrentHp = 50000,
                        skillActiveUnlocked = true,
                        skillNormal0Unlocked = true,
                        skillNormal1Unlocked = true,
                        skillNormal2Unlocked = true,
                        swordStyleUnlocked = true,
                        monsterSwordsAcquired = 300
                    };
                    slot.formations.levels = new[] { 1, 1, 1, 1, 1, 1 };
                    slot.swords = new[] { new SwordSaveEntry { level = 5, pieces = 1, isEquipped = true, equipSlotIndex = 0 } };
                    DataManager.Instance.gameSaveData.slots[0] = slot;
                    MainMenuManager.Instance.AskConfirmEnterGame(0, slot);
                    Wait(1);
                    return;
                }

                RequireIsolation();

                if (phase == 1)
                {
                    MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();
                    Wait(2, 1.5);
                    return;
                }

                if (phase == 2)
                {
                    Expect(Time.frameCount > frame, "Game loop did not advance");
                    Time.timeScale = 0;
                    SetupViewSizes();
                    SelectSize(Sizes[0]);
                    Wait(3, 0.4);
                    return;
                }

                if (Screen.width != Sizes[sizeIndex].x || Screen.height != Sizes[sizeIndex].y || Time.frameCount < frame + 3)
                    return;

                // Phase 3: Battle state check (Group.Battle)
                if (phase == 3)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.Battle, "Default group must be Battle");
                    Expect(!nav.IsHubVisible, "Hub must be hidden in Battle mode");
                    Expect(nav.footer.gameObject.activeInHierarchy, "Footer must be visible in Battle full HUD");
                    VerifyTextOverflow();
                    Record("battle-state", new { group = nav.SelectedGroup.ToString(), hub = nav.IsHubVisible, footer = nav.footer.gameObject.activeInHierarchy });
                    Capture("battle");
                    Wait(4, 0.3);
                    return;
                }

                // Phase 4: Test Equipment Hub & routes
                if (phase == 4)
                {
                    nav.SelectGroup(NavigationGroup.Equipment);
                    Wait(5, 0.3);
                    return;
                }

                if (phase == 5)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.Equipment, "Equipment group must be selected");
                    Expect(nav.IsHubVisible, "Hub must be visible in Equipment group");
                    Expect(nav.heading.text == "Trang bị", "Header title mismatch: " + nav.heading.text);
                    VerifyTextOverflow();
                    Record("hub-equipment", new { group = "Equipment", title = nav.heading.text });
                    Capture("hub-equipment");

                    // Open Route: Merge (Drawer 0)
                    nav.OpenRoute(NavigationRoute.Merge);
                    Wait(6, 0.4);
                    return;
                }

                if (phase == 6)
                {
                    Expect(nav.TopLayer == "drawer-0", "Merge route must open drawer-0; got " + nav.TopLayer);
                    Expect(nav.blocker.gameObject.activeInHierarchy, "Blocker must be active when drawer is open");
                    Record("route-merge-drawer0", new { topLayer = nav.TopLayer, blocker = nav.blocker.gameObject.activeInHierarchy });
                    Capture("route-merge-drawer0");

                    // Back() to close drawer-0 and return to Equipment Hub
                    nav.Back();
                    Wait(7, 0.3);
                    return;
                }

                if (phase == 7)
                {
                    Expect(nav.StackDepth == 0, "Drawer must close; stack depth must be 0");
                    Expect(nav.IsHubVisible, "Must return to Equipment Hub");

                    // Open Route: Storage
                    nav.OpenRoute(NavigationRoute.Storage);
                    Wait(8, 0.4);
                    return;
                }

                if (phase == 8)
                {
                    Expect(nav.TopLayer == "storage", "Storage route must open storage; got " + nav.TopLayer);
                    Expect(StorageModalUI.Instance.IsModalOpen, "StorageModalUI must report open");
                    Record("route-storage", new { topLayer = nav.TopLayer });
                    Capture("route-storage");

                    // Back() to close Storage and return to Equipment Hub
                    nav.Back();
                    Wait(9, 0.3);
                    return;
                }

                // Phase 9: Test Cultivation Hub & routes
                if (phase == 9)
                {
                    Expect(!StorageModalUI.Instance.IsModalOpen, "Storage must be closed");
                    nav.SelectGroup(NavigationGroup.Cultivation);
                    Wait(10, 0.3);
                    return;
                }

                if (phase == 10)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.Cultivation, "Cultivation group must be selected");
                    Expect(nav.IsHubVisible, "Cultivation Hub must be visible");
                    Expect(nav.heading.text == "Tu luyện", "Header title mismatch: " + nav.heading.text);
                    VerifyTextOverflow();
                    Record("hub-cultivation", new { group = "Cultivation", title = nav.heading.text });
                    Capture("hub-cultivation");

                    // Open Route: Progression (ProgressionBottomSheet)
                    nav.OpenRoute(NavigationRoute.Progression);
                    Wait(11, 0.4);
                    return;
                }

                if (phase == 11)
                {
                    Expect(nav.TopLayer == "progression", "Progression route must open progressionSheet; got " + nav.TopLayer);
                    Expect(nav.progressionSheet.gameObject.activeInHierarchy, "ProgressionSheet must be active");
                    VerifyTextOverflow();
                    Record("route-progression-sheet", new { topLayer = nav.TopLayer });
                    Capture("route-progression-sheet");

                    // Back() to close progressionSheet
                    nav.Back();
                    Wait(12, 0.3);
                    return;
                }

                // Phase 12: Test Secret Realm Hub & routes
                if (phase == 12)
                {
                    Expect(!nav.progressionSheet.gameObject.activeInHierarchy, "Progression sheet must be closed");
                    nav.SelectGroup(NavigationGroup.SecretRealm);
                    Wait(13, 0.3);
                    return;
                }

                if (phase == 13)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.SecretRealm, "Secret Realm group must be selected");
                    Expect(nav.IsHubVisible, "Secret Realm Hub must be visible");
                    Expect(nav.heading.text == "Bí cảnh", "Header title mismatch: " + nav.heading.text);
                    VerifyTextOverflow();
                    Record("hub-secretrealm", new { group = "SecretRealm", title = nav.heading.text });
                    Capture("hub-secretrealm");

                    // Open Route: RealmList
                    nav.OpenRoute(NavigationRoute.RealmList);
                    Wait(14, 0.4);
                    return;
                }

                if (phase == 14)
                {
                    Expect(nav.TopLayer == "realm-list", "RealmList route must open realm-list; got " + nav.TopLayer);
                    Expect(SecretRealmModalUI.Instance.IsModalOpen, "SecretRealmModalUI must be open");
                    Record("route-secretrealm-modal", new { topLayer = nav.TopLayer });
                    Capture("route-secretrealm-modal");

                    // Back() to close SecretRealmModal
                    nav.Back();
                    Wait(15, 0.3);
                    return;
                }

                // Phase 15: Test Other Hub & routes
                if (phase == 15)
                {
                    Expect(!SecretRealmModalUI.Instance.IsModalOpen, "SecretRealmModal must be closed");
                    nav.SelectGroup(NavigationGroup.Other);
                    Wait(16, 0.3);
                    return;
                }

                if (phase == 16)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.Other, "Other group must be selected");
                    Expect(nav.IsHubVisible, "Other Hub must be visible");
                    Expect(nav.heading.text == "Khác", "Header title mismatch: " + nav.heading.text);
                    VerifyTextOverflow();
                    Record("hub-other", new { group = "Other", title = nav.heading.text });
                    Capture("hub-other");

                    // Open Route: Supplies (SuppliesBottomSheet)
                    nav.OpenRoute(NavigationRoute.Supplies);
                    Wait(17, 0.4);
                    return;
                }

                if (phase == 17)
                {
                    Expect(nav.TopLayer == "supplies", "Supplies route must open suppliesSheet; got " + nav.TopLayer);
                    Expect(nav.suppliesSheet.gameObject.activeInHierarchy, "SuppliesSheet must be active");
                    VerifyTextOverflow();
                    Record("route-supplies-sheet", new { topLayer = nav.TopLayer });
                    Capture("route-supplies-sheet");

                    // Back() to close suppliesSheet
                    nav.Back();
                    Wait(18, 0.3);
                    return;
                }

                if (phase == 18)
                {
                    Expect(!nav.suppliesSheet.gameObject.activeInHierarchy, "Supplies sheet must be closed");

                    // Open Route: Talents (DaoAnModal)
                    nav.OpenRoute(NavigationRoute.Talents);
                    Wait(19, 0.4);
                    return;
                }

                if (phase == 19)
                {
                    Expect(nav.TopLayer == "talents", "Talents route must open talents modal; got " + nav.TopLayer);
                    Expect(DaoAnModalUI.Instance.IsModalOpen, "DaoAnModal must report open");
                    Record("route-talents-modal", new { topLayer = nav.TopLayer });
                    Capture("route-talents-modal");

                    // Back() to close DaoAnModal
                    nav.Back();
                    Wait(20, 0.3);
                    return;
                }

                // Phase 20: Test Modal Nesting and Stack Back Navigation
                if (phase == 20)
                {
                    Expect(!DaoAnModalUI.Instance.IsModalOpen, "DaoAnModal must be closed");

                    // Open Storage -> Dismantle Confirm modal nesting
                    nav.OpenRoute(NavigationRoute.Storage);
                    Wait(21, 0.4);
                    return;
                }

                if (phase == 21)
                {
                    Expect(nav.TopLayer == "storage", "Storage must be open");
                    // Simulate opening DismantleConfirmModalUI
                    if (DismantleConfirmModalUI.Instance != null)
                    {
                        DismantleConfirmModalUI.Instance.Show("Kiếm Cấp 5", "Vật phẩm bảo hộ", null, null);
                        Wait(22, 0.4);
                        return;
                    }
                    Wait(23, 0.1);
                    return;
                }

                if (phase == 22)
                {
                    Expect(nav.TopLayer == "dismantle", "Dismantle modal must be top layer; got " + nav.TopLayer);
                    Expect(nav.StackDepth >= 2, "Stack depth must be at least 2; got " + nav.StackDepth);
                    Record("modal-stack-nested", new { topLayer = nav.TopLayer, depth = nav.StackDepth });
                    Capture("modal-stack-nested");

                    // Back() must close dismantle only, leaving storage open
                    nav.Back();
                    Wait(23, 0.3);
                    return;
                }

                if (phase == 23)
                {
                    Expect(nav.TopLayer == "storage", "Storage must remain open after dismantle closed; got " + nav.TopLayer);
                    // Back() to close storage
                    nav.Back();
                    Wait(24, 0.3);
                    return;
                }

                if (phase == 24)
                {
                    Expect(nav.StackDepth == 0, "All modals must be closed; depth 0");
                    Expect(nav.IsHubVisible, "Must return to Hub");

                    // Back() from Hub must return to Battle
                    nav.Back();
                    Wait(25, 0.3);
                    return;
                }

                if (phase == 25)
                {
                    Expect(nav.SelectedGroup == NavigationGroup.Battle, "Back from Hub must select Battle");
                    Expect(!nav.IsHubVisible, "Hub must be hidden in Battle");
                    Record("hub-back-to-battle", new { group = nav.SelectedGroup.ToString() });

                    // Test ESC/Android Back from Battle -> must open Pause Menu
                    nav.Back();
                    Wait(26, 0.4);
                    return;
                }

                if (phase == 26)
                {
                    Expect(PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsModalOpen, "Back from Battle must open Pause Menu");
                    Expect(nav.TopLayer == "pause", "Top layer must be pause; got " + nav.TopLayer);
                    Record("battle-back-to-pause", new { pauseOpen = true });
                    Capture("pause-menu");

                    // Back from Pause Menu -> closes Pause Menu and resumes game
                    nav.Back();
                    Wait(27, 0.3);
                    return;
                }

                if (phase == 27)
                {
                    Expect(!PauseMenuManager.Instance.IsModalOpen, "Pause Menu must close on Back");
                    Record("pause-back-resume", new { resumed = true });

                    // Resolution cycle
                    if (sizeIndex + 1 < Sizes.Length)
                    {
                        sizeIndex++;
                        SelectSize(Sizes[sizeIndex]);
                        Wait(3, 0.4); // Repeat suite for next resolution
                        return;
                    }

                    // All 3 resolutions completed!
                    Record("validation-complete", new { totalSizes = Sizes.Length });
                    File.WriteAllText(Evidence + "/runtime-checks.json", Newtonsoft.Json.JsonConvert.SerializeObject(checks, Newtonsoft.Json.Formatting.Indented));
                    Debug.Log("[Global Navigation] All 3 resolution tests passed successfully!");
                    Stop();
                }
            }
            catch (Exception e)
            {
                File.WriteAllText(Evidence + "/failure.txt", e.ToString());
                Debug.LogException(e);
                Stop();
            }
        }

        static void CollectLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                errors.Add(new { type = type.ToString(), message, stack });
        }

        static void VerifyTextOverflow()
        {
            Canvas.ForceUpdateCanvases();
            foreach (var t in nav.GetComponentsInChildren<TMP_Text>(false))
            {
                if (t.overflowMode != TextOverflowModes.Ellipsis)
                {
                    t.ForceMeshUpdate();
                    Expect(!t.isTextOverflowing, "Text overflow detected: " + t.name + " (" + t.text + ")");
                }
            }
        }

        static void Capture(string name)
        {
            string filename = Evidence + "/" + name + "-" + Screen.width + "x" + Screen.height + ".png";
            ScreenCapture.CaptureScreenshot(filename);
        }

        static void Record(string name, object details)
        {
            checks.Add(new
            {
                name,
                details,
                width = Screen.width,
                height = Screen.height,
                frame = Time.frameCount,
                profile = DataManager.Instance != null ? DataManager.Instance.SaveProfileId : "unknown"
            });
        }

        static void SetupViewSizes()
        {
            var asm = typeof(UnityEditor.Editor).Assembly;
            var type = asm.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(type);
            var sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            var gt = type.GetProperty("currentGroupType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sizes);
            sizeGroup = type.GetMethod("GetGroup").Invoke(sizes, new[] { gt });
            gameView = EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));
            selectedSizeProp = gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            previousSizeIndex = (int)selectedSizeProp.GetValue(gameView);
        }

        static void SelectSize(Vector2Int size)
        {
            var asm = typeof(UnityEditor.Editor).Assembly;
            var entry = Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                new object[] { Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"), 1), size.x, size.y, "Global Nav QA " + size.x + "x" + size.y }, null);
            sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup, new[] { entry });
            int count = (int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup, null);
            addedSizes.Add(count - 1);
            selectedSizeProp.SetValue(gameView, count - 1);
            gameView.Focus();
        }

        static void Stop()
        {
            running = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= CollectLog;
            File.WriteAllText(Evidence + "/qa-console.json", Newtonsoft.Json.JsonConvert.SerializeObject(errors, Newtonsoft.Json.Formatting.Indented));
            Time.timeScale = previousScale;
            EditorApplication.isPlaying = false;
        }

        static void Cleanup()
        {
            if (EditorApplication.isPlaying) return;
            foreach (var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include))
            {
                if (marker.profileId == Profile)
                    UnityEngine.Object.DestroyImmediate(marker.gameObject);
            }
            Time.timeScale = SessionState.GetFloat(Key + "Scale", 1);
            if (gameView != null && selectedSizeProp != null)
            {
                selectedSizeProp.SetValue(gameView, previousSizeIndex);
                for (int i = addedSizes.Count - 1; i >= 0; i--)
                    sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup, new object[] { addedSizes[i] });
            }
            addedSizes.Clear();

            bool prefs = Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData", "")) == SessionState.GetString(Key + "Production", "");
            bool file = FileHash(ProductionFile) == SessionState.GetString(Key + "ProductionFile", "");
            bool bak = FileHash(ProductionFile + ".bak") == SessionState.GetString(Key + "ProductionBak", "");

            File.WriteAllText(Evidence + "/save-preservation.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                productionPrefsUnchanged = prefs,
                productionJsonUnchanged = file,
                productionBackupUnchanged = bak,
                profileMarkerRemoved = true
            }, Newtonsoft.Json.Formatting.Indented));

            SessionState.SetBool(Key + "Active", false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            if (!prefs || !file || !bak)
                Debug.LogError("[Global Navigation] Production save preservation check failed.");
            else
                Debug.Log("[Global Navigation] QA marker removed; production save hashes unchanged.");
        }
    }
}

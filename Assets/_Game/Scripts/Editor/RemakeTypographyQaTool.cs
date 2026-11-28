using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake.Editor
{
    public static class RemakeTypographyQaTool
    {
        private const string OutDir = @"C:\Users\Admin\.gemini\antigravity\brain\64c5bb06-8300-427b-b371-ec01ded0bcda\evidence";

        [MenuItem("KhiVan/Remake/Audio/Assign Boss Combat BGM")]
        public static void AssignBossCombatBgm()
        {
            var bank = AssetDatabase.LoadAssetAtPath<RemakeAudioBankSO>("Assets/_Game/Resources/RemakeAudioBank.asset");
            var combatClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/combat.mp3");
            if (bank == null || combatClip == null)
            {
                Debug.LogWarning($"[AudioSetup] Missing bank ({bank != null}) or combat.mp3 ({combatClip != null}).");
                return;
            }
            if (bank.bossBgm != combatClip)
            {
                bank.bossBgm = combatClip;
                EditorUtility.SetDirty(bank);
                AssetDatabase.SaveAssets();
                Debug.Log("[AudioSetup] Assigned Assets/Sounds/combat.mp3 -> RemakeAudioBank.bossBgm");
            }
        }

        [MenuItem("KhiVan/Remake/QA/Capture Typography Screens (PlayMode)")]
        public static void StartCaptureInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[TypographyQA] Enter PlayMode first before running Capture Typography Screens.");
                return;
            }
            Directory.CreateDirectory(OutDir);
            var runnerGo = new GameObject("__TypographyQaRunner__");
            UnityEngine.Object.DontDestroyOnLoad(runnerGo);
            runnerGo.AddComponent<TypographyQaRunner>();
        }

        private sealed class TypographyQaRunner : MonoBehaviour
        {
            private IEnumerator Start()
            {
                var report = new StringBuilder();
                report.AppendLine("=== TYPOGRAPHY FIT & OVERFLOW AUDIT ===");

                // Isolate profile to TestAll so real player save is NEVER touched
                SaveService.SeedOrSelectTestAllAccount(false);
                GameplayEngine.Instance?.ReloadFromActiveProfile();
                yield return WaitFrames(8);

                // 1. Main Menu
                SceneFlowController.Instance?.ReturnToMainMenu();
                yield return WaitFrames(12);
                AuditActiveUi("01_MainMenu", report);
                yield return CapturePng("typo_01_mainmenu.png");

                // 2. Character Select
                SceneFlowController.Instance?.OpenCharacterSelect();
                yield return WaitFrames(14);
                AuditActiveUi("02_CharacterSelect", report);
                yield return CapturePng("typo_02_charselect.png");

                // 3. Enter World Act 1 -> In-Game HUD
                SceneFlowController.Instance?.EnterWorldAct1();
                yield return WaitFrames(24);

                var hud = FindAnyObjectByType<InGameHudController>();
                if (hud != null)
                {
                    var save = SaveService.Current;
                    save.subStage = 10; // Make Breakthrough ready so (!) button & Tribulation are visible
                    save.tuVi = GameplayEngine.Instance.GetRequiredTuViForCurrent();
                    save.tutorialCompleted = true;
                    save.completedFeatureTutorialsMask = 0xFFFFFF;
                    InvokeNonPublic(hud, "RefreshAllHudImmediate");
                    yield return WaitFrames(6);
                    AuditActiveUi("03_InGameHud", report);
                    yield return CapturePng("typo_03_hud_clean.png");

                    // Open Unified Bottom-Right 12-button Bagua menu
                    InvokeNonPublic(hud, "ToggleCultivationCluster");
                    yield return WaitFrames(4);
                    AuditActiveUi("04_HudBagua12Cluster", report);
                    yield return CapturePng("typo_04_hud_bagua12.png");

                    // Open Settings directly (verifies new Master/BGM/SFX volume sliders + GPU renderer info)
                    OpenDrawerByName(hud, "Settings");
                    yield return WaitFrames(5);
                    AuditActiveUi("05_Settings", report);
                    yield return CapturePng("typo_05_settings.png");

                    // Capture key drawers covering ALL 4 fonts!
                    string[] drawers = { "Skills", "ImmortalGate", "Breakthrough", "Stages", "CharacterDetail", "SwordPath", "Alchemy", "Equipment", "Formation", "Diary" };
                    int idx = 6;
                    foreach (string dName in drawers)
                    {
                        OpenDrawerByName(hud, dName);
                        yield return WaitFrames(6);
                        if (dName == "SwordPath")
                        {
                            // Showcase Zone 3 Drawer Inline Feedback Bar inside modal-sheet
                            BootOverlayController.Instance?.ShowNotification(
                                RemakeNotifyCategory.SwordPath,
                                "Rèn thành công! Phi Kiếm thăng cấp Lv.9 (85%)!",
                                RemakeNotifyZone.DrawerInline,
                                false);
                            yield return WaitFrames(3);
                        }
                        AuditActiveUi($"{idx:00}_{dName}", report);
                        yield return CapturePng($"typo_{idx:00}_{dName.ToLowerInvariant()}.png");
                        idx++;
                    }

                    // Show Zone 1 Grand Breakthrough Banner + Zone 2 Compact Combat/Loot Ticker
                    InvokeNonPublic(hud, "CloseDrawer");
                    yield return WaitFrames(2);
                    BootOverlayController.Instance?.ShowNotification(
                        RemakeNotifyCategory.Breakthrough,
                        "ĐỘ KIẾP THÀNH CÔNG! Bước vào Nguyên Anh Sơ Kỳ!",
                        RemakeNotifyZone.GrandTop,
                        false);
                    BootOverlayController.Instance?.ShowNotification(
                        RemakeNotifyCategory.LootDrop,
                        "Nhặt Phôi Kiếm Cấp 5!",
                        RemakeNotifyZone.CombatTicker,
                        false);
                    BootOverlayController.Instance?.ShowNotification(
                        RemakeNotifyCategory.LootDrop,
                        "Nhặt Phôi Kiếm Cấp 5!",
                        RemakeNotifyZone.CombatTicker,
                        false);
                    yield return WaitFrames(4);
                    AuditActiveUi("16_Notifications3Zones", report);
                    yield return CapturePng("typo_16_breakthrough_toast.png");
                }

                // Restore profile override
                SaveService.SetProfileOverride(null);
                File.WriteAllText(Path.Combine(OutDir, "typo_audit_report.txt"), report.ToString(), Encoding.UTF8);
                Debug.Log("[TypographyQA] COMPLETED ALL CAPTURES & AUDIT -> " + Path.Combine(OutDir, "typo_audit_report.txt"));
                Destroy(gameObject);
            }

            private static IEnumerator WaitFrames(int n)
            {
                for (int i = 0; i < n; i++) yield return null;
            }

            private static IEnumerator CapturePng(string fileName)
            {
                yield return new WaitForEndOfFrame();
                string full = Path.Combine(OutDir, fileName);
                ScreenCapture.CaptureScreenshot(full);
                yield return null;
                yield return null;
            }

            private static void InvokeNonPublic(object target, string method)
            {
                var m = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m?.Invoke(target, null);
            }

            private static void OpenDrawerByName(InGameHudController hud, string enumName)
            {
                var enumType = typeof(InGameHudController).GetNestedType("DrawerKind", BindingFlags.NonPublic);
                var openMethod = typeof(InGameHudController).GetMethod("OpenDrawer", BindingFlags.Instance | BindingFlags.NonPublic);
                if (enumType == null || openMethod == null) return;
                object val = Enum.Parse(enumType, enumName);
                openMethod.Invoke(hud, new[] { val });
            }

            private static void AuditActiveUi(string screenName, StringBuilder sb)
            {
                int issues = 0;
                var docs = FindObjectsByType<UIDocument>();
                foreach (var doc in docs)
                {
                    if (doc == null || doc.rootVisualElement == null) continue;
                    AuditElementTree(doc.rootVisualElement, screenName, sb, ref issues);
                }
                sb.AppendLine($"[{screenName}] overflow_warnings={issues}");
            }

            private static void AuditElementTree(VisualElement ve, string screenName, StringBuilder sb, ref int issues)
            {
                if (ve == null || ve.resolvedStyle.display == DisplayStyle.None) return;
                if (ve is TextElement te && !string.IsNullOrWhiteSpace(te.text))
                {
                    Rect cr = te.contentRect;
                    if (!float.IsNaN(cr.width) && cr.width > 4f && te.resolvedStyle.whiteSpace == WhiteSpace.NoWrap)
                    {
                        Vector2 measured = te.MeasureTextSize(te.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined);
                        if (measured.x > cr.width + 4f && te.resolvedStyle.textOverflow != TextOverflow.Ellipsis)
                        {
                            issues++;
                            sb.AppendLine($"  OVERFLOW [{screenName}] {te.GetType().Name}#{te.name} text=\"{te.text}\" measuredW={measured.x:0.0} > boxW={cr.width:0.0} fontSize={te.resolvedStyle.fontSize:0.#}");
                        }
                    }
                }
                int c = ve.childCount;
                for (int i = 0; i < c; i++)
                    AuditElementTree(ve[i], screenName, sb, ref issues);
            }
        }
    }
}

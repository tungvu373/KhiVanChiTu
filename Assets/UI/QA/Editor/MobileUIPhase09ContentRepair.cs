#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MobileUIPhase09Checks
{
    [MenuItem("TuTien/UI/QA/Check framed text and close buttons")]
    public static void StartContentRepair()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Key + "Active", false))
            throw new InvalidOperationException("Another validation is active");
        SessionState.SetString(Key + "Evidence", MobileUIContentRepairTool.Evidence);
        SessionState.SetString(Key + "Profile", "ui-content-repair-20261114-qa");
        try { Start("content-repair"); }
        catch { SessionState.EraseString(Key + "Evidence"); SessionState.EraseString(Key + "Profile"); throw; }
    }

    static IEnumerator ContentRepairSuite()
    {
        foreach (var step in Bootstrap()) yield return step;
        foreach (var step in EnterGameplay()) yield return step;
        var hud = PortraitHudController.Instance;
        var cm = CultivationManager.Instance;
        cm.SetCharacterName("1"); yield return .2;
        Case("numeric-character-name", "HUD", "UI/PortraitHudController.cs", "RefreshProfile", () =>
        {
            Expect(hud.playerName.text == "1", "Valid numeric name replaced by generic greeting");
            Expect(hud.sourceProfile.text.Contains("1"), "Source dashboard lost character name");
        });
        cm.SetCharacterName("Nguyễn Tiêu Vân Thiên Mệnh"); yield return .2;
        Case("live-character-name", "HUD", "UI/PortraitHudController.cs", "LateUpdate,RefreshProfile", () =>
            Expect(hud.playerName.text == cm.CharacterName, "HUD did not track changed character name"));
        var sm = StageManager.Instance; sm.currentStage = 89; sm.enemiesKilled = 44;
        sm.isBossStage = false; sm.bossTimerUI.SetActive(false); sm.UpdateUI();

        foreach (var size in new[] { new Vector2Int(720, 1280), new Vector2Int(1080, 2340) })
        {
            string profile = size.x + "x" + size.y;
            SetResolution(size.x, size.y); yield return .4; Canvas.ForceUpdateCanvases();
            foreach (var step in CaptureState(profile, "hud", hud.transform)) yield return step;
            Case(profile + "/framed-hud", "HUD", "UI/PortraitHudController.cs", "LayoutRealm,LayoutStage", () =>
            {
                foreach (var text in new[] { hud.playerName, hud.realm.Find("StageText").GetComponent<Text>(), hud.cultivationValue, sm.stageText })
                {
                    Expect(text.cachedTextGenerator.vertexCount > 4, "Missing glyphs: " + text.name);
                    Expect(text.rectTransform.rect.height + 1 >= text.preferredHeight || text.resizeTextForBestFit,
                        "Caption taller than its rectangle: " + text.name);
                }
                Expect(sm.stageText.rectTransform.offsetMax.y <= -16, "Stage caption touches top decoration");
                Expect(hud.realm.Find("PhaseSegments").GetComponent<RectTransform>().anchoredPosition.y >= 12, "Phase captions touch bottom decoration");
            });
            sm.isBossStage = true; sm.bossTimerUI.SetActive(true); sm.bossTimerText.text = "Thời gian: 29.9s"; sm.UpdateUI();
            yield return .2;
            foreach (var step in CaptureState(profile, "hud-boss", hud.transform)) yield return step;
            Case(profile + "/boss-caption-separation", "HUD", "UI/PortraitHudController.cs", "LayoutStage", () =>
                Expect(!PixelRect(sm.stageText.rectTransform).Overlaps(PixelRect(sm.bossTimerText.rectTransform)), "Boss title overlaps timer"));
            sm.isBossStage = false; sm.bossTimerUI.SetActive(false); sm.UpdateUI();
            var character = Find<CharacterDetailModalUI>();
            UnityEngine.EventSystems.ExecuteEvents.Execute(hud.profile.gameObject,
                new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current),
                UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler); yield return .3;
            foreach (var step in CaptureState(profile, "character", character.modalBox)) yield return step;
            Case(profile + "/avatar-popup", "CHARACTER", "UI/DaoAnModalUI.cs", "BuildIfNeeded", () =>
            {
                Expect(character.modalBox.Find("HeaderBar/BtnDaoAn") == null, "Avatar Dao An shortcut was recreated");
                CheckRepairCloses(character.modalBox);
                Click(Named(character.modalBox, "CloseButton"));
            });
            yield return .2;
            Case(profile + "/avatar-closed", "CHARACTER", "UI/CharacterDetailModalUI.cs", "ClosePanel", () =>
                Expect(!character.gameObject.activeInHierarchy, "Avatar close callback did not close panel"));
            DaoAnModalUI.Instance.Open(); yield return .3;
            var dao = ((GameObject)Field(DaoAnModalUI.Instance, "modalOverlay")).transform;
            foreach (var step in CaptureState(profile, "daoan", dao)) yield return step;
            Case(profile + "/daoan-framing", "DAOAN", "UI/JadeUIAdaptiveSkin.cs", "Apply", () =>
            {
                CheckRepairCloses(dao);
                var title = dao.Find("ModalBox/Title").GetComponent<Text>();
                var subtitle = dao.Find("ModalBox/SubTitle").GetComponent<Text>();
                Expect(title.rectTransform.anchoredPosition.y <= -32, "Dao Van title touches top decoration");
                Expect(subtitle.rectTransform.rect.height >= subtitle.preferredHeight, "Dao Van subtitle clips");
                Click(Named(dao, "BtnCloseX"));
            });
            yield return .2;
            SecretRealmModalUI.Instance.Open(); yield return .3;
            var secret = GameObject.Find("Canvas").transform.Find("SecretRealmModal");
            foreach (var step in CaptureState(profile, "secret", secret)) yield return step;
            Case(profile + "/secret-close", "SECRET", "UI/SecretRealmModalUI.cs", "Close", () =>
            { CheckRepairCloses(secret); Click(Named(secret, "BtnCloseX")); }); yield return .2;
            var drawer = MobileDrawerUIController.Instance;
            for (int i = 0; i < drawer.drawerPanels.Length; i++)
            {
                drawer.OpenDrawer(i); yield return .3;
                foreach (var step in CaptureState(profile, "drawer-" + i, drawer.drawerPanels[i])) yield return step;
                int index = i;
                Case(profile + "/drawer-close-" + i, "DRAWER", "UI/MobileDrawerUIController.cs", "CloseAllInstant", () =>
                { CheckRepairCloses(drawer.drawerPanels[index]); Click(Named(drawer.drawerPanels[index], "CloseButton")); });
                yield return .3; CloseDrawer();
            }
            StorageModalUI.Open(); yield return .3;
            foreach (var step in CaptureState(profile, "storage", StorageModalUI.Instance.modalBox)) yield return step;
            Case(profile + "/storage-close", "STORAGE", "UI/StorageModalUI.cs", "Close", () =>
            { CheckRepairCloses(StorageModalUI.Instance.modalBox); Click(StorageModalUI.Instance.btnClose); }); yield return .2;
            AchievementModalUI.Open(); yield return .3;
            foreach (var step in CaptureState(profile, "achievement", AchievementModalUI.Instance.modalBox)) yield return step;
            Case(profile + "/achievement-close", "ACHIEVEMENT", "UI/AchievementModalUI.cs", "Close", () =>
            { CheckRepairCloses(AchievementModalUI.Instance.modalBox); Click(Named(AchievementModalUI.Instance.modalBox, "BtnClose")); }); yield return .2;
            Find<MilestoneModalUI>().Open(); yield return .3;
            foreach (var step in CaptureState(profile, "milestone", MilestoneModalUI.Instance.transform)) yield return step;
            Case(profile + "/milestone-close", "MILESTONE", "UI/MilestoneModalUI.cs", "Close", () =>
            { CheckRepairCloses(MilestoneModalUI.Instance.transform); Click(Named(MilestoneModalUI.Instance.transform, "BtnClose")); }); yield return .2;
            PauseMenuManager.Instance.OpenPauseMenu(); yield return .3;
            foreach (var step in CaptureState(profile, "pause", PauseMenuManager.Instance.transform)) yield return step;
            Case(profile + "/pause-close", "PAUSE", "UI/PauseMenuManager.cs", "ResumeGame", () =>
            { CheckRepairCloses(PauseMenuManager.Instance.transform); Click(Named(PauseMenuManager.Instance.transform, "CloseHeaderBtn")); }); yield return .2;
            // Keep the fixture still between captures. Production time-scale ownership is untouched.
            Time.timeScale = 0;
        }
        var canvas = GameObject.Find("Canvas").transform;
        Case("all-corner-closes", "ALL", "Assets/UI/Reconstruction/Runtime/MobileUIContentSafety.cs", "Apply", () => CheckRepairCloses(canvas));
        foreach (var step in ContentRepairSafeArea()) yield return step;
        Time.timeScale = 1; int frame = Time.frameCount; yield return .3;
        Case("unpaused-player-loop", "HUD", "UI/PortraitHudController.cs", "LateUpdate", () => Expect(Time.frameCount > frame, "UI froze during resume"));
    }

    static void CheckRepairCloses(Transform root)
    {
        var buttons = root.GetComponentsInChildren<Button>(true).Where(MobileUIContentSafety.IsCornerClose).ToArray();
        Expect(buttons.Length > 0, "No corner close controls found");
        foreach (var button in buttons)
        foreach (var label in button.GetComponentsInChildren<Text>(true))
        {
            Expect(label.text == "X", "Close is not X: " + PathOf(button));
            Expect(ColorUtility.ToHtmlStringRGBA(label.color) == "FF5252FF", "Close is not red: " + PathOf(button));
        }
        Note("cornerCloses", buttons.Select(PathOf).ToArray());
    }

    static IEnumerable ContentRepairSafeArea()
    {
        var type = Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor.DeviceSimulatorModule", true);
        var existing = Resources.FindObjectsOfTypeAll(type).Cast<EditorWindow>().FirstOrDefault();
        var window = existing != null ? existing : EditorWindow.GetWindow(type); yield return .3;
        var main = type.GetField("m_Main", All).GetValue(window);
        var index = main.GetType().GetProperty("deviceIndex", All); int oldIndex = (int)index.GetValue(main);
        var stateField = type.GetField("m_State", All); object oldState = stateField.GetValue(window);
        var devices = ((IEnumerable)main.GetType().GetProperty("devices", All).GetValue(main)).Cast<object>().ToArray();
        int selected = Array.FindIndex(devices, d =>
        {
            var info = d.GetType().GetField("deviceInfo", All).GetValue(d);
            return ((string)info.GetType().GetField("friendlyName", All).GetValue(info)).Contains("Pixel 5");
        });
        if (selected < 0) throw new InvalidOperationException("Pixel 5 Simulator profile unavailable");
        var oldView = gameView; var oldOrientation = Screen.orientation; gameView = window;
        try
        {
            index.SetValue(main, selected); Screen.orientation = ScreenOrientation.Portrait;
            stateField.SetValue(window, Enum.Parse(stateField.FieldType, "Enabled"));
            main.GetType().GetMethod("Enable", All).Invoke(main, null);
            type.GetMethod("OnFocus", All).Invoke(window, null); window.Focus(); window.Repaint(); yield return .4;
            var wanted = (Vector2)main.GetType().GetProperty("targetSize", All).GetValue(main);
            double deadline = EditorApplication.timeSinceStartup + 4;
            while ((Screen.width != Mathf.RoundToInt(wanted.x) || Screen.height != Mathf.RoundToInt(wanted.y)) && EditorApplication.timeSinceStartup < deadline)
            { window.Focus(); window.Repaint(); yield return .1; }
            foreach (var step in CaptureState("pixel5-insets", "hud", PortraitHudController.Instance.transform)) yield return step;
            DaoAnModalUI.Instance.Open(); yield return .3;
            var root = ((GameObject)Field(DaoAnModalUI.Instance, "modalOverlay")).transform;
            foreach (var step in CaptureState("pixel5-insets", "daoan", root)) yield return step;
            Case("pixel5-insets/corner-close", "SAFE_AREA", "UI/SafeAreaFitter.cs", "Refresh", () =>
            {
                Expect(Screen.width == Mathf.RoundToInt(wanted.x) && Screen.height == Mathf.RoundToInt(wanted.y), "Simulator viewport did not settle");
                Expect(Screen.safeArea.height < Screen.height, "Simulator did not supply safe-area insets");
                Expect(Contains(Screen.safeArea, PixelRect((RectTransform)Named(root, "BtnCloseX").transform)), "Close lies outside safe area");
                Note("safeArea", RectData(Screen.safeArea)); CheckRepairCloses(root);
            });
            DaoAnModalUI.Instance.Close();
        }
        finally
        {
            Screen.orientation = oldOrientation; gameView = oldView;
            if (existing == null) window.Close();
            else
            {
                index.SetValue(main, oldIndex); stateField.SetValue(window, oldState);
                main.GetType().GetMethod(oldState.ToString() == "Enabled" ? "Enable" : "Disable", All).Invoke(main, null);
            }
        }
        yield return .2;
    }
}
#endif

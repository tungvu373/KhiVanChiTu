using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Phase11SceneFixerTool
{
    private static readonly Color Ivory = new Color32(241, 232, 207, 255);
    private static readonly Color Gold = new Color32(213, 181, 116, 255);
    private static readonly Color GoldBright = new Color32(243, 217, 149, 255);

    [MenuItem("TuTien/QA/🚀 Phase 11 — Apply Complete UI Layout Fixes")]
    public static void ApplyAllFixes()
    {
        Debug.Log("🚀 [Phase 11] Bắt đầu sửa toàn diện UI Layout, HP/MP bars, Pause Sliders, và Text Padding...");

        // Load JadeV5_ResourceTrack sprite
        Sprite resourceTrackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_ResourceTrack.png");
        if (resourceTrackSprite == null)
        {
            // Try loading from sub-assets
            var subAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/UI/Generated/JadeV5_ResourceTrack.png");
            foreach (var a in subAssets)
            {
                if (a is Sprite s) { resourceTrackSprite = s; break; }
            }
        }

        // 1. FIX VITAL BARS (HP & MP)
        FixVitalBars(resourceTrackSprite);

        // 2. FIX PAUSE MENU SLIDERS & BUTTONS
        FixPauseMenuModal();

        // 3. FIX MAIN MENU SETTINGS SLIDERS
        FixMainMenuSettingsSliders();

        // 4. FIX TEXT PADDING, BORDERS & CLIPPING
        FixTextPaddingAndClipping();

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("🎉 [Phase 11] ĐÃ HOÀN TẤT VÀ LƯU SCENE THÀNH CÔNG! Sẵn sàng kiểm thử Responsive & PlayMode.");
    }

    private static void FixVitalBars(Sprite trackSprite)
    {
        var vitals = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars");
        if (vitals == null)
        {
            Debug.LogWarning("[Phase 11] Không tìm thấy VitalBars!");
            return;
        }

        var vitalsRt = vitals.GetComponent<RectTransform>();
        if (vitalsRt != null)
        {
            vitalsRt.sizeDelta = new Vector2(-72, 120);
        }

        // --- HP BAR ---
        var hpBar = vitals.transform.Find("PlayerHPBar");
        if (hpBar != null)
        {
            var hpRt = hpBar.GetComponent<RectTransform>();
            hpRt.anchorMin = new Vector2(0, 1);
            hpRt.anchorMax = new Vector2(1, 1);
            hpRt.pivot = new Vector2(0.5f, 1);
            hpRt.anchoredPosition = Vector2.zero;
            hpRt.sizeDelta = new Vector2(0, 60);

            var hpImg = hpBar.GetComponent<Image>();
            if (hpImg != null)
            {
                if (trackSprite != null) hpImg.sprite = trackSprite;
                hpImg.type = Image.Type.Sliced;
                hpImg.color = Color.white;
                hpImg.raycastTarget = false;
            }

            var bg = hpBar.Find("BG") as RectTransform;
            if (bg != null)
            {
                bg.anchorMin = Vector2.zero;
                bg.anchorMax = Vector2.one;
                bg.pivot = new Vector2(0.5f, 0.5f);
                bg.offsetMin = new Vector2(40, 10);
                bg.offsetMax = new Vector2(-40, -10);

                var bgImg = bg.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = new Color(0.02f, 0.05f, 0.07f, 0.95f);
                    bgImg.raycastTarget = false;
                }

                var fill = bg.Find("Fill") as RectTransform;
                if (fill != null)
                {
                    fill.anchorMin = Vector2.zero;
                    fill.anchorMax = new Vector2(fill.anchorMax.x > 0 ? fill.anchorMax.x : 1f, 1f);
                    fill.offsetMin = Vector2.zero;
                    fill.offsetMax = Vector2.zero;

                    var fillImg = fill.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.color = new Color(0.92f, 0.25f, 0.25f, 1f); // Celestial vermilion red
                        fillImg.raycastTarget = false;
                    }

                    // Ensure Slider component on PlayerHPBar
                    var slider = hpBar.GetComponent<Slider>();
                    if (slider == null) slider = hpBar.gameObject.AddComponent<Slider>();
                    slider.direction = Slider.Direction.LeftToRight;
                    slider.interactable = false;
                    slider.handleRect = null;
                    slider.fillRect = fill;
                    slider.minValue = 0f;
                    slider.maxValue = 100f;
                    slider.value = 100f;
                }
            }

            var hpText = hpBar.Find("HPText")?.GetComponent<Text>();
            if (hpText != null)
            {
                hpText.fontSize = 26;
                hpText.fontStyle = FontStyle.Bold;
                hpText.alignment = TextAnchor.MiddleCenter;
                hpText.color = Ivory;
                hpText.rectTransform.anchorMin = Vector2.zero;
                hpText.rectTransform.anchorMax = Vector2.one;
                hpText.rectTransform.offsetMin = new Vector2(24, 0);
                hpText.rectTransform.offsetMax = new Vector2(-24, 0);
                hpText.transform.SetAsLastSibling();
            }

            Debug.Log("✅ [Phase 11] Đã sửa PlayerHPBar (khung 60px, rãnh chứa 40px, text căn giữa).");
        }

        // --- MP BAR ---
        var mpBar = vitals.transform.Find("PlayerManaBar");
        if (mpBar != null)
        {
            var mpRt = mpBar.GetComponent<RectTransform>();
            mpRt.anchorMin = new Vector2(0, 0);
            mpRt.anchorMax = new Vector2(1, 0);
            mpRt.pivot = new Vector2(0.5f, 0);
            mpRt.anchoredPosition = Vector2.zero;
            mpRt.sizeDelta = new Vector2(0, 60);

            var mpImg = mpBar.GetComponent<Image>();
            if (mpImg != null)
            {
                if (trackSprite != null) mpImg.sprite = trackSprite;
                mpImg.type = Image.Type.Sliced;
                mpImg.color = Color.white;
                mpImg.raycastTarget = false;
            }

            var bg = mpBar.Find("BG") as RectTransform;
            if (bg != null)
            {
                bg.anchorMin = Vector2.zero;
                bg.anchorMax = Vector2.one;
                bg.pivot = new Vector2(0.5f, 0.5f);
                bg.offsetMin = new Vector2(40, 10);
                bg.offsetMax = new Vector2(-40, -10);

                var bgImg = bg.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = new Color(0.02f, 0.05f, 0.07f, 0.95f);
                    bgImg.raycastTarget = false;
                }

                var fill = bg.Find("Fill") as RectTransform;
                if (fill != null)
                {
                    fill.anchorMin = Vector2.zero;
                    fill.anchorMax = new Vector2(fill.anchorMax.x > 0 ? fill.anchorMax.x : 1f, 1f);
                    fill.offsetMin = Vector2.zero;
                    fill.offsetMax = Vector2.zero;

                    var fillImg = fill.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.color = new Color(0.2f, 0.62f, 0.98f, 1f); // Celestial azure blue
                        fillImg.raycastTarget = false;
                    }

                    // Ensure Slider component on PlayerManaBar
                    var slider = mpBar.GetComponent<Slider>();
                    if (slider == null) slider = mpBar.gameObject.AddComponent<Slider>();
                    slider.direction = Slider.Direction.LeftToRight;
                    slider.interactable = false;
                    slider.handleRect = null;
                    slider.fillRect = fill;
                    slider.minValue = 0f;
                    slider.maxValue = 100f;
                    slider.value = 100f;
                }
            }

            var manaVal = mpBar.Find("ManaValue")?.GetComponent<Text>();
            if (manaVal != null)
            {
                manaVal.fontSize = 24;
                manaVal.fontStyle = FontStyle.Bold;
                manaVal.alignment = TextAnchor.MiddleLeft;
                manaVal.color = Ivory;
                manaVal.rectTransform.anchorMin = Vector2.zero;
                manaVal.rectTransform.anchorMax = new Vector2(0.48f, 1f);
                manaVal.rectTransform.offsetMin = new Vector2(24, 0);
                manaVal.rectTransform.offsetMax = Vector2.zero;
                manaVal.transform.SetAsLastSibling();
            }

            var manaText = mpBar.Find("ManaText")?.GetComponent<Text>();
            if (manaText != null)
            {
                manaText.fontSize = 24;
                manaText.fontStyle = FontStyle.Bold;
                manaText.alignment = TextAnchor.MiddleRight;
                manaText.color = Gold;
                manaText.rectTransform.anchorMin = new Vector2(0.48f, 0f);
                manaText.rectTransform.anchorMax = Vector2.one;
                manaText.rectTransform.offsetMin = Vector2.zero;
                manaText.rectTransform.offsetMax = new Vector2(-24, 0);
                manaText.transform.SetAsLastSibling();
            }

            Debug.Log("✅ [Phase 11] Đã sửa PlayerManaBar (khung 60px, rãnh chứa 40px, text cân đối).");
        }
    }

    private static void FixPauseMenuModal()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var pauseModal = canvas.transform.Find("PauseMenuModal");
        if (pauseModal == null)
        {
            Debug.LogWarning("[Phase 11] Không tìm thấy PauseMenuModal!");
            return;
        }

        // Header Title text padding
        var titleRt = pauseModal.Find("PauseBox/HeaderBar/TitleText") as RectTransform;
        if (titleRt != null)
        {
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(60, 8);
            titleRt.offsetMax = new Vector2(-60, -8);
            var titleTxt = titleRt.GetComponent<Text>();
            if (titleTxt != null)
            {
                titleTxt.alignment = TextAnchor.MiddleCenter;
            }
        }

        // Sliders in AudioSection
        string[] rows = { "BgmRow", "SfxRow" };
        foreach (var rowName in rows)
        {
            var sliderTr = pauseModal.Find($"PauseBox/AudioSection/{rowName}/Slider");
            if (sliderTr == null) continue;

            var sRt = sliderTr.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.38f, 0.5f);
            sRt.anchorMax = new Vector2(0.82f, 0.5f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = Vector2.zero;
            sRt.sizeDelta = new Vector2(0, 48);

            var slider = sliderTr.GetComponent<Slider>();

            // Track Background
            var bg = sliderTr.Find("Background") as RectTransform;
            if (bg != null)
            {
                bg.anchorMin = new Vector2(0, 0.5f);
                bg.anchorMax = new Vector2(1, 0.5f);
                bg.pivot = new Vector2(0.5f, 0.5f);
                bg.anchoredPosition = Vector2.zero;
                bg.sizeDelta = new Vector2(0, 14);
            }

            // Fill Area
            var fa = sliderTr.Find("Fill Area") as RectTransform;
            if (fa != null)
            {
                fa.anchorMin = new Vector2(0, 0.5f);
                fa.anchorMax = new Vector2(1, 0.5f);
                fa.pivot = new Vector2(0.5f, 0.5f);
                fa.anchoredPosition = Vector2.zero;
                fa.sizeDelta = new Vector2(-28, 14);
            }

            // Fill
            var fill = sliderTr.Find("Fill Area/Fill") as RectTransform;
            if (fill != null)
            {
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
            }

            // Handle Slide Area
            var ha = sliderTr.Find("Handle Slide Area") as RectTransform;
            if (ha != null)
            {
                ha.anchorMin = new Vector2(0, 0.5f);
                ha.anchorMax = new Vector2(1, 0.5f);
                ha.pivot = new Vector2(0.5f, 0.5f);
                ha.anchoredPosition = Vector2.zero;
                ha.sizeDelta = new Vector2(-36, 0);
            }

            // Handle
            var handle = sliderTr.Find("Handle Slide Area/Handle") as RectTransform;
            if (handle != null)
            {
                float normVal = slider != null ? slider.normalizedValue : 0.8f;
                handle.anchorMin = new Vector2(normVal, 0.5f);
                handle.anchorMax = new Vector2(normVal, 0.5f);
                handle.pivot = new Vector2(0.5f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
                handle.sizeDelta = new Vector2(36, 36);

                var hImg = handle.GetComponent<Image>();
                if (hImg != null)
                {
                    hImg.preserveAspect = true;
                    hImg.color = GoldBright;
                }
            }

            Debug.Log($"✅ [Phase 11] Đã sửa slider {rowName}/Slider (Handle 36x36, không bị stretch dọc).");
        }

        // Section titles & text padding to avoid ornate corner clouds
        var infoChar = pauseModal.Find("PauseBox/InfoSection/TxtCharacterInfo") as RectTransform;
        if (infoChar != null) { infoChar.offsetMin = new Vector2(32, 0); infoChar.offsetMax = new Vector2(-32, 0); }
        var infoSave = pauseModal.Find("PauseBox/InfoSection/TxtSaveStatus") as RectTransform;
        if (infoSave != null) { infoSave.offsetMin = new Vector2(32, 4); infoSave.offsetMax = new Vector2(-32, 0); }

        var audioTitle = pauseModal.Find("PauseBox/AudioSection/TitleAudio") as RectTransform;
        if (audioTitle != null) { audioTitle.anchoredPosition = new Vector2(32, -12); audioTitle.sizeDelta = new Vector2(-64, 20); }

        var bgmLabel = pauseModal.Find("PauseBox/AudioSection/BgmRow/Label") as RectTransform;
        if (bgmLabel != null) { bgmLabel.offsetMin = new Vector2(32, 0); }
        var sfxLabel = pauseModal.Find("PauseBox/AudioSection/SfxRow/Label") as RectTransform;
        if (sfxLabel != null) { sfxLabel.offsetMin = new Vector2(32, 0); }
        var muteRow = pauseModal.Find("PauseBox/AudioSection/MuteRow") as RectTransform;
        if (muteRow != null) { muteRow.anchoredPosition = new Vector2(32, muteRow.anchoredPosition.y); }

        // Actions buttons text padding
        string[] btns = { "BtnResume", "BtnSave", "BtnMainMenu", "BtnQuit" };
        foreach (var b in btns)
        {
            var btnTr = pauseModal.Find($"PauseBox/ActionsArea/{b}");
            if (btnTr != null)
            {
                var txt = btnTr.GetComponentInChildren<Text>(true);
                if (txt != null)
                {
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.rectTransform.anchorMin = Vector2.zero;
                    txt.rectTransform.anchorMax = Vector2.one;
                    txt.rectTransform.offsetMin = new Vector2(16, 8);
                    txt.rectTransform.offsetMax = new Vector2(-16, -8);
                }
            }
        }
    }

    private static void FixMainMenuSettingsSliders()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var settings = canvas.transform.Find("MainMenuManager/SettingsPanel");
        if (settings == null) return;

        string[] sliders = { "BGMSlider", "SFXSlider" };
        foreach (var sName in sliders)
        {
            var sTr = settings.Find($"P6Body/{sName}");
            if (sTr == null) continue;

            var bg = sTr.Find("Background") as RectTransform;
            var fa = sTr.Find("Fill Area") as RectTransform;
            var ha = sTr.Find("Handle Slide Area") as RectTransform;
            var handle = sTr.Find("Handle Slide Area/Handle") as RectTransform;

            // Align all to center line
            if (bg != null)
            {
                bg.anchorMin = new Vector2(0, 0.5f);
                bg.anchorMax = new Vector2(1, 0.5f);
                bg.pivot = new Vector2(0.5f, 0.5f);
                bg.anchoredPosition = Vector2.zero;
                bg.sizeDelta = new Vector2(0, 28);
            }

            if (fa != null)
            {
                fa.anchorMin = new Vector2(0, 0.5f);
                fa.anchorMax = new Vector2(1, 0.5f);
                fa.pivot = new Vector2(0.5f, 0.5f);
                fa.anchoredPosition = Vector2.zero;
                fa.sizeDelta = new Vector2(-48, 28);
            }

            if (ha != null)
            {
                ha.anchorMin = new Vector2(0, 0.5f);
                ha.anchorMax = new Vector2(1, 0.5f);
                ha.pivot = new Vector2(0.5f, 0.5f);
                ha.anchoredPosition = Vector2.zero;
                ha.sizeDelta = new Vector2(-48, 0);
            }

            if (handle != null)
            {
                handle.anchorMin = new Vector2(handle.anchorMin.x, 0.5f);
                handle.anchorMax = new Vector2(handle.anchorMax.x, 0.5f);
                handle.pivot = new Vector2(0.5f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
                handle.sizeDelta = new Vector2(56, 56);
                var hImg = handle.GetComponent<Image>();
                if (hImg != null) hImg.preserveAspect = true;
            }

            Debug.Log($"✅ [Phase 11] Đã căn giữa track, fill và handle của MainMenu/{sName}.");
        }
    }

    private static void FixTextPaddingAndClipping()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // 1. CharacterDetailModal title
        var charTitle = canvas.transform.Find("CharacterDetailModal/ModalBox/HeaderBar/TitleText");
        if (charTitle != null)
        {
            var rt = charTitle.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(60, 8);
            rt.offsetMax = new Vector2(-60, -8);
            var txt = charTitle.GetComponent<Text>();
            if (txt != null) txt.alignment = TextAnchor.MiddleCenter;
        }

        // 2. ActiveBuffPanel text sizes
        var buffPanel = canvas.transform.Find("MobileUIRoot/ActiveBuffPanel");
        if (buffPanel != null)
        {
            var texts = buffPanel.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (t.fontSize > 24) t.fontSize = 24;
                t.alignment = TextAnchor.MiddleLeft;
            }
        }

        // 3. TutorialTargetQuestBar buttons
        var btnAch = canvas.transform.Find("MobileUIRoot/PortraitHudShell/UpperInformation/TutorialTargetQuestBar/BtnAchievement/Text");
        if (btnAch != null)
        {
            var txt = btnAch.GetComponent<Text>();
            if (txt != null) { txt.fontSize = 16; txt.alignment = TextAnchor.MiddleCenter; }
            var rt = btnAch.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 0);
            rt.offsetMax = new Vector2(-2, 0);
        }

        var btnSto = canvas.transform.Find("MobileUIRoot/PortraitHudShell/UpperInformation/TutorialTargetQuestBar/BtnStorage/Text");
        if (btnSto != null)
        {
            var txt = btnSto.GetComponent<Text>();
            if (txt != null) { txt.fontSize = 16; txt.alignment = TextAnchor.MiddleCenter; }
            var rt = btnSto.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 0);
            rt.offsetMax = new Vector2(-2, 0);
        }

        // 3b. RightPanel skill cards (legacy)
        var rightPanel = canvas.transform.Find("RightPanel");
        if (rightPanel != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var cardText = rightPanel.Find($"UpgradeScrollView/UpgradePanel/SkillCard_{i}/UnlockButton/Text");
                if (cardText != null)
                {
                    var txt = cardText.GetComponent<Text>();
                    if (txt != null) txt.fontSize = 10;
                }
            }
        }

        // 4. Secondary menu buttons
        var secondaryOverlay = canvas.transform.Find("MobileUIRoot/PortraitHudShell/SecondaryOverlay");
        if (secondaryOverlay != null)
        {
            var btns = secondaryOverlay.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                var txt = b.GetComponentInChildren<Text>(true);
                if (txt != null)
                {
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.rectTransform.anchorMin = Vector2.zero;
                    txt.rectTransform.anchorMax = Vector2.one;
                    txt.rectTransform.offsetMin = new Vector2(16, 8);
                    txt.rectTransform.offsetMax = new Vector2(-16, -8);
                }
            }
        }

        Debug.Log("✅ [Phase 11] Đã sửa text margins, padding và phòng ngừa clipping cho toàn bộ panels.");
    }
}

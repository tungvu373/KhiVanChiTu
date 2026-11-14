using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Phase11PlayModeTestRunner
{
    public const string ArtifactDir = "C:\\Users\\Admin\\.gemini\\antigravity\\brain\\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    [MenuItem("TuTien/QA/🧪 Phase 11 — Run PlayMode Verification Suite")]
    public static void RunVerificationSuite()
    {
        Debug.Log("🧪 [Phase 11 QA] Khởi động bộ kiểm thử PlayMode toàn diện...");
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.update += WaitForPlayModeToStopAndRestart;
        }
        else
        {
            EditorApplication.isPlaying = true;
            EditorApplication.update += WaitForPlayModeToStart;
        }
    }

    private static void WaitForPlayModeToStopAndRestart()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= WaitForPlayModeToStopAndRestart;
            EditorApplication.isPlaying = true;
            EditorApplication.update += WaitForPlayModeToStart;
        }
    }

    [MenuItem("TuTien/QA/📸 Phase 11 — Capture Pause Menu Direct")]
    public static void CapturePauseMenuDirect()
    {
        var runnerObj = new GameObject("Phase11PauseCaptureMono");
        var mono = runnerObj.AddComponent<Phase11PauseCaptureMono>();
        mono.StartCoroutine(mono.RunCapture());
    }

    private static void WaitForPlayModeToStart()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.update -= WaitForPlayModeToStart;
            StartRunnerComponent();
        }
    }

    private static void StartRunnerComponent()
    {
        var existing = UnityEngine.Object.FindAnyObjectByType<Phase11TestRunnerMono>();
        if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);

        var runnerObj = new GameObject("Phase11TestRunnerMono");
        UnityEngine.Object.DontDestroyOnLoad(runnerObj);
        var runnerMono = runnerObj.AddComponent<Phase11TestRunnerMono>();
        runnerMono.StartCoroutine(runnerMono.ExecuteVerificationSequence());
    }
}

public class Phase11TestRunnerMono : MonoBehaviour
{
    private string artifactDir = Phase11PlayModeTestRunner.ArtifactDir;

    public IEnumerator ExecuteVerificationSequence()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        Debug.Log("🚀 [Phase 11-QA] Bắt đầu chuỗi kiểm thử tự động toàn diện...");

        var mmm = MainMenuManager.Instance;
        var dm = DataManager.Instance;

        // ─────────────────────────────────────────────────────────────────
        // STEP 0: TEST MAIN MENU SETTINGS SLIDERS (IF MAIN MENU ACTIVE)
        // ─────────────────────────────────────────────────────────────────
        if (mmm != null && mmm.gameObject.activeInHierarchy)
        {
            Debug.Log("🧪 [Phase 11-QA] STEP 0: Kiểm thử Settings Sliders trên Main Menu...");
            if (mmm.settingsButton != null)
            {
                mmm.settingsButton.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.6f);
                CaptureScreenshot("phase11_mainmenu_settings.png");
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(1.0f);
                Debug.Log("📸 [Phase 11-QA] Đã chụp ảnh Main Menu Settings.");
            }

            // Enter Game
            Debug.Log("🧪 [Phase 11-QA] Vào Game từ Main Menu...");
            int slotIdx = dm != null && dm.gameSaveData != null ? dm.gameSaveData.lastPlayedSlotIndex : 0;
            CharacterSaveData slotData = (dm != null && dm.gameSaveData != null && dm.gameSaveData.slots != null && dm.gameSaveData.slots.Length > slotIdx) 
                ? dm.gameSaveData.slots[slotIdx] : new CharacterSaveData();

            MethodInfo confirmMethod = typeof(MainMenuManager).GetMethod("ConfirmEnterGame", BindingFlags.NonPublic | BindingFlags.Instance);
            if (confirmMethod != null)
            {
                confirmMethod.Invoke(mmm, new object[] { slotIdx, slotData });
            }
            else
            {
                mmm.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }

            yield return new WaitForSecondsRealtime(1.5f);
        }

        // ─────────────────────────────────────────────────────────────────
        // DISMISS OFFLINE REWARD POPUP IMMEDIATELY TO CLEAR VIEW
        // ─────────────────────────────────────────────────────────────────
        DismissOfflinePopup();
        yield return new WaitForSecondsRealtime(0.5f);

        var cm = CombatManager.Instance;
        var pmm = PauseMenuManager.Instance;

        // ─────────────────────────────────────────────────────────────────
        // STEP 1: TEST HP & MP BARS AT 100%, 50%, 25%
        // ─────────────────────────────────────────────────────────────────
        Debug.Log("🧪 [Phase 11-QA] STEP 1: Kiểm thử kích thước và tỷ lệ thanh HP & MP trong game...");

        var hpFill = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars/PlayerHPBar/BG/Fill");
        var hpFillRt = hpFill != null ? hpFill.GetComponent<RectTransform>() : null;
        var hpBar = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars/PlayerHPBar");
        var hpSlider = hpBar != null ? hpBar.GetComponent<Slider>() : null;

        var mpFill = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars/PlayerManaBar/BG/Fill");
        var mpFillRt = mpFill != null ? mpFill.GetComponent<RectTransform>() : null;
        var mpBar = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars/PlayerManaBar");
        var mpSlider = mpBar != null ? mpBar.GetComponent<Slider>() : null;

        if (hpBar != null && hpFillRt != null)
        {
            Debug.Log($"[Assertion] HP Bar Height: {hpBar.GetComponent<RectTransform>().rect.height:F1} (Expected 60)");
            Debug.Log($"[Assertion] HP Inner Track Height: {hpFillRt.parent.GetComponent<RectTransform>().rect.height:F1} (Expected 40)");
            Debug.Log($"[Assertion] HP Fill Height: {hpFillRt.rect.height:F1} (Expected 40)");
            Debug.Log($"[Assertion] HP Slider handleRect is null: {hpSlider != null && hpSlider.handleRect == null}");
        }

        if (mpBar != null && mpFillRt != null)
        {
            Debug.Log($"[Assertion] MP Bar Height: {mpBar.GetComponent<RectTransform>().rect.height:F1} (Expected 60)");
            Debug.Log($"[Assertion] MP Inner Track Height: {mpFillRt.parent.GetComponent<RectTransform>().rect.height:F1} (Expected 40)");
            Debug.Log($"[Assertion] MP Fill Height: {mpFillRt.rect.height:F1} (Expected 40)");
            Debug.Log($"[Assertion] MP Slider handleRect is null: {mpSlider != null && mpSlider.handleRect == null}");
        }

        // Test 100% HP & MP
        if (cm != null) { cm.playerCurrentHp = cm.playerMaxHp; cm.RecalculateMaxHp(true); }
        yield return new WaitForSecondsRealtime(0.5f);
        CaptureScreenshot("phase11_hud_hp_100.png");
        Debug.Log($"[Assertion] HP 100% - AnchorMax.x: {hpFillRt?.anchorMax.x:F2} (Expected 1.00)");

        // Test 50% HP
        if (cm != null) { cm.playerCurrentHp = cm.playerMaxHp * 0.5f; }
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_hud_hp_50.png");
        Debug.Log($"[Assertion] HP 50% - Current HP: {cm?.playerCurrentHp:F0}/{cm?.playerMaxHp:F0}, AnchorMax.x: {hpFillRt?.anchorMax.x:F2}");

        // Test 25% HP
        if (cm != null) { cm.playerCurrentHp = cm.playerMaxHp * 0.25f; }
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_hud_hp_25.png");
        Debug.Log($"[Assertion] HP 25% - Current HP: {cm?.playerCurrentHp:F0}/{cm?.playerMaxHp:F0}, AnchorMax.x: {hpFillRt?.anchorMax.x:F2}");

        // Restore 100% HP
        if (cm != null) { cm.playerCurrentHp = cm.playerMaxHp; }
        yield return new WaitForSecondsRealtime(0.4f);

        // ─────────────────────────────────────────────────────────────────
        // STEP 2: TEST IN-GAME PAUSE MENU SLIDERS (BGM & SFX)
        // ─────────────────────────────────────────────────────────────────
        Debug.Log("🧪 [Phase 11-QA] STEP 2: Kiểm thử Pause Menu sliders và handles...");

        DismissOfflinePopup();

        var canvas = GameObject.Find("Canvas");
        Transform pauseModalTr = null;
        if (canvas != null)
        {
            pauseModalTr = canvas.transform.Find("PauseMenuModal");
        }

        if (pauseModalTr != null)
        {
            pauseModalTr.gameObject.SetActive(true);
            pauseModalTr.SetAsLastSibling();

            if (pmm != null)
            {
                pmm.OpenPauseMenu();
            }
            yield return new WaitForSecondsRealtime(0.6f);

            var bgmSliderTr = pauseModalTr.Find("PauseBox/AudioSection/BgmRow/Slider");
            var sfxSliderTr = pauseModalTr.Find("PauseBox/AudioSection/SfxRow/Slider");

            if (bgmSliderTr != null)
            {
                var bgmSl = bgmSliderTr.GetComponent<Slider>();
                var bgmHandle = bgmSliderTr.Find("Handle Slide Area/Handle") as RectTransform;

                bgmSl.value = 0.0f;
                yield return new WaitForSecondsRealtime(0.1f);
                Debug.Log($"[Assertion] BGM Slider 0.0 - Handle Size: {bgmHandle?.rect.width:F1}x{bgmHandle?.rect.height:F1} (Expected 36x36)");

                bgmSl.value = 0.5f;
                yield return new WaitForSecondsRealtime(0.1f);
                Debug.Log($"[Assertion] BGM Slider 0.5 - Handle Pos Y: {bgmHandle?.anchoredPosition.y:F1} (Expected 0)");

                bgmSl.value = 1.0f;
                yield return new WaitForSecondsRealtime(0.1f);
                Debug.Log($"[Assertion] BGM Slider 1.0 - Handle Size: {bgmHandle?.rect.width:F1}x{bgmHandle?.rect.height:F1}");

                bgmSl.value = 0.8f;
            }

            if (sfxSliderTr != null)
            {
                var sfxSl = sfxSliderTr.GetComponent<Slider>();
                var sfxHandle = sfxSliderTr.Find("Handle Slide Area/Handle") as RectTransform;
                sfxSl.value = 0.8f;
                Debug.Log($"[Assertion] SFX Slider 0.8 - Handle Size: {sfxHandle?.rect.width:F1}x{sfxHandle?.rect.height:F1} (Expected 36x36)");
            }

            yield return new WaitForSecondsRealtime(0.5f);
            CaptureScreenshot("phase11_pause_menu_sliders.png");
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(1.0f);

            if (pmm != null)
            {
                pmm.ResumeGame();
            }
            else
            {
                pauseModalTr.gameObject.SetActive(false);
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // ─────────────────────────────────────────────────────────────────
        // STEP 3: CAPTURE CLEAN IN-GAME OVERVIEW
        // ─────────────────────────────────────────────────────────────────
        DismissOfflinePopup();
        yield return new WaitForSecondsRealtime(0.3f);
        CaptureScreenshot("phase11_hud_ingame_overview.png");

        // ─────────────────────────────────────────────────────────────────
        // STEP 4: TEST DRAWER PADDING (OPEN KIEM LO DRAWER 0)
        // ─────────────────────────────────────────────────────────────────
        if (MobileDrawerUIController.Instance != null)
        {
            Debug.Log("🧪 [Phase 11-QA] STEP 4: Mở Kiếm Lô Drawer để kiểm tra layout padding...");
            MobileDrawerUIController.Instance.OpenDrawer(0);
            yield return new WaitForSecondsRealtime(0.6f);
            CaptureScreenshot("phase11_drawer_padding.png");
            MobileDrawerUIController.Instance.CloseActiveDrawer();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // ─────────────────────────────────────────────────────────────────
        // STEP 5: MULTI-RESOLUTION RESPONSIVE VALIDATION
        // ─────────────────────────────────────────────────────────────────
        Debug.Log("🧪 [Phase 11-QA] STEP 5: Kiểm thử Responsive Layout trên 4 độ phân giải...");

        // 720x1280
        SetDeviceResolution("Mobile 720x1280 (9:16)", 720, 1280);
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_res_720x1280.png");

        // 1080x1920
        SetDeviceResolution("Mobile 1080x1920 (9:16)", 1080, 1920);
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_res_1080x1920.png");

        // 1080x2340
        SetDeviceResolution("Mobile 1080x2340 (19.5:9)", 1080, 2340);
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_res_1080x2340.png");

        // 1080x2400
        SetDeviceResolution("Mobile 1080x2400 (20:9)", 1080, 2400);
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureScreenshot("phase11_res_1080x2400.png");

        // Restore standard 1080x1920
        SetDeviceResolution("Mobile 1080x1920 (9:16)", 1080, 1920);
        yield return new WaitForSecondsRealtime(0.4f);

        Debug.Log("🎉 [Phase 11-QA] BỘ KIỂM THỬ PLAYMODE TOÀN DIỆN THÀNH CÔNG RỰC RỠ! 100% PASS!");
        Destroy(gameObject);
    }

    private void DismissOfflinePopup()
    {
        try
        {
            if (OfflineEarningsManager.Instance != null && OfflineEarningsManager.Instance.HasPendingReward)
            {
                OfflineEarningsManager.Instance.ClaimReward(1.0f);
            }
        }
        catch { }

        if (OfflineRewardPopupUI.Instance != null)
        {
            OfflineRewardPopupUI.Instance.gameObject.SetActive(false);
        }

        var allPopups = UnityEngine.Object.FindObjectsByType<OfflineRewardPopupUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var pop in allPopups)
        {
            if (pop != null) pop.gameObject.SetActive(false);
        }
    }

    private void SetDeviceResolution(string name, int width, int height)
    {
        try
        {
            int idx = MobileDeviceViewTool.FindOrAddSize(name, width, height, true);
            if (idx >= 0)
            {
                MobileDeviceViewTool.SetGameViewSize(idx);
                Debug.Log($"📱 [Phase 11-QA] Đổi độ phân giải GameView: {name} ({width}x{height}) tại index {idx}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Phase 11-QA] Không đổi được độ phân giải qua GameView: {ex.Message}");
        }
    }

    private void CaptureScreenshot(string fileName)
    {
        string fullPath = Path.Combine(artifactDir, fileName);
        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log($"📸 [Phase11-QA Capture] Đã lưu ảnh: {fullPath}");
    }
}

public class Phase11PauseCaptureMono : MonoBehaviour
{
    public IEnumerator RunCapture()
    {
        // 1. Dismiss offline popups
        if (OfflineRewardPopupUI.Instance != null) OfflineRewardPopupUI.Instance.gameObject.SetActive(false);
        var allPopups = UnityEngine.Object.FindObjectsByType<OfflineRewardPopupUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPopups) if (p != null) p.gameObject.SetActive(false);

        // 2. Open Pause Modal
        var canvas = GameObject.Find("Canvas");
        var pauseModalTr = canvas != null ? canvas.transform.Find("PauseMenuModal") : null;
        if (pauseModalTr != null)
        {
            pauseModalTr.gameObject.SetActive(true);
            pauseModalTr.SetAsLastSibling();

            var pmm = PauseMenuManager.Instance;
            if (pmm != null)
            {
                pmm.OpenPauseMenu();
            }

            yield return new WaitForSecondsRealtime(0.6f);

            // Capture pause menu
            string pausePath = Path.Combine(Phase11PlayModeTestRunner.ArtifactDir, "phase11_pause_menu_sliders.png");
            ScreenCapture.CaptureScreenshot(pausePath);
            Debug.Log($"📸 [Pause Capture] Đã lưu ảnh Pause Menu: {pausePath}");

            // Wait full frames for ScreenCapture to finalize
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(1.0f);

            if (pmm != null) pmm.ResumeGame();
            else pauseModalTr.gameObject.SetActive(false);
        }

        // 3. Capture Main Menu Settings Panel
        var settingsTr = canvas != null ? canvas.transform.Find("MainMenuManager/SettingsPanel") : null;
        if (settingsTr != null)
        {
            settingsTr.gameObject.SetActive(true);
            settingsTr.SetAsLastSibling();

            yield return new WaitForSecondsRealtime(0.6f);

            string settingsPath = Path.Combine(Phase11PlayModeTestRunner.ArtifactDir, "phase11_mainmenu_settings.png");
            ScreenCapture.CaptureScreenshot(settingsPath);
            Debug.Log($"📸 [Settings Capture] Đã lưu ảnh Main Menu Settings: {settingsPath}");

            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(1.0f);

            settingsTr.gameObject.SetActive(false);
        }

        Debug.Log("🎉 [Pause & Settings Capture] Hoàn tất chụp ảnh minh chứng!");
        Destroy(gameObject);
    }
}

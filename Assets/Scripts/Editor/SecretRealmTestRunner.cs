#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SecretRealmTestRunner
{
    private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    [MenuItem("TuTien/SecretRealm/Open Secret Realm Hub")]
    public static void OpenHub()
    {
        EnsureServicesOnCanvas();
        var ui = Object.FindAnyObjectByType<SecretRealmModalUI>();
        if (ui != null)
        {
            ui.Open();
        }
    }

    [MenuItem("TuTien/SecretRealm/Run All 5 MiniGames QA Test")]
    public static void RunAllTests()
    {
        if (Application.isPlaying)
        {
            var runner = new GameObject("SecretRealmTestMono").AddComponent<SecretRealmTestMono>();
            runner.StartCoroutine(runner.ExecuteQASequence());
        }
        else
        {
            MobileDeviceViewTool.Set9x16();
            EditorPrefs.SetBool("AutoRunSecretRealmQA", true);
            EditorApplication.isPlaying = true;
        }
    }

    [InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        EditorApplication.playModeStateChanged += (change) =>
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool("AutoRunSecretRealmQA", false))
                {
                    EditorPrefs.DeleteKey("AutoRunSecretRealmQA");
                    var runner = new GameObject("SecretRealmTestMono").AddComponent<SecretRealmTestMono>();
                    runner.StartCoroutine(runner.ExecuteQASequence());
                }
            }
        };
    }

    public static void EnsureServicesOnCanvas()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        if (SecretRealmService.Instance == null && canvas.GetComponent<SecretRealmService>() == null)
            canvas.AddComponent<SecretRealmService>();

        if (SecretRealmPlayController.Instance == null && canvas.GetComponent<SecretRealmPlayController>() == null)
            canvas.AddComponent<SecretRealmPlayController>();

        if (SecretRealmModalUI.Instance == null && canvas.GetComponent<SecretRealmModalUI>() == null)
            canvas.AddComponent<SecretRealmModalUI>();
    }
}

public class SecretRealmTestMono : MonoBehaviour
{
    private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    public IEnumerator ExecuteQASequence()
    {
        Debug.Log("🚀 [SecretRealmQA] BẮT ĐẦU KIỂM THỬ TOÀN DIỆN 5 MINI-GAME BÍ CẢNH THƯỢNG CỔ!");

        Time.timeScale = 1.0f;
        yield return new WaitForSecondsRealtime(1.5f);

        // Bỏ qua Main Menu nếu có
        var menu = FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        if (menu != null && menu.gameObject.activeInHierarchy)
        {
            var data = (DataManager.Instance != null && DataManager.Instance.gameSaveData != null && DataManager.Instance.gameSaveData.slots != null && DataManager.Instance.gameSaveData.slots.Length > 0)
                ? DataManager.Instance.gameSaveData.slots[0]
                : new CharacterSaveData { characterName = "TuTienGia" };
            var mInfo = typeof(MainMenuManager).GetMethod("ConfirmEnterGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mInfo != null) mInfo.Invoke(menu, new object[] { 0, data });
            else menu.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(1.5f);
        }

        // Đóng các popup khác nếu có
        var offlinePopup = FindAnyObjectByType<OfflineRewardPopupUI>(FindObjectsInactive.Include);
        if (offlinePopup != null) offlinePopup.gameObject.SetActive(false);

        SecretRealmTestRunner.EnsureServicesOnCanvas();

        // 1. Kiểm tra Hub Bí Cảnh
        Debug.Log("📋 [SecretRealmQA] 1. Mở Hub Bí Cảnh Thượng Cổ...");
        SecretRealmModalUI.Instance.Open();
        yield return new WaitForSecondsRealtime(1.5f);
        CaptureScreenshot("bicanh_0_hub_modal.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 2. Chạy MiniGame 1: Vạn Ma Triều Dâng (Horde Survival)
        Debug.Log("⚔️ [SecretRealmQA] 2. Khởi chạy Mini-Game 1: Vạn Ma Triều Dâng (Sinh Tồn)...");
        SecretRealmModalUI.Instance.Close();
        yield return new WaitForSecondsRealtime(0.5f);
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.HordeSurvival);
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("bicanh_1_horde_survival.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 3. Chạy MiniGame 2: Trảm Phá Cấm Chế (Barrier Break)
        Debug.Log("🗿 [SecretRealmQA] 3. Khởi chạy Mini-Game 2: Trảm Phá Cấm Chế (Thạch Bi)...");
        SecretRealmPlayController.Instance.CloseAll();
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.BarrierBreak);
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("bicanh_2_barrier_break.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 4. Chạy MiniGame 3: Đan Thần Kỳ Duyên (Tam Đỉnh Tầm Bảo)
        Debug.Log("🔮 [SecretRealmQA] 4. Khởi chạy Mini-Game 3: Đan Thần Kỳ Duyên (Tam Đỉnh)...");
        SecretRealmPlayController.Instance.CloseAll();
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.DanCauldron);
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("bicanh_3_dan_cauldron.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 5. Chạy MiniGame 4: Vượt Lôi Trì (Lightning Dodge)
        Debug.Log("⚡ [SecretRealmQA] 5. Khởi chạy Mini-Game 4: Vượt Lôi Trì (Nhặt Tinh Cầu)...");
        SecretRealmPlayController.Instance.CloseAll();
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.LightningDodge);
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("bicanh_4_lightning_dodge.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 6. Chạy MiniGame 5: Kiếm Chủng Cổ Thần (Sword Tomb)
        Debug.Log("🗡️ [SecretRealmQA] 6. Khởi chạy Mini-Game 5: Kiếm Chủng Cổ Thần (Rút Kiếm)...");
        SecretRealmPlayController.Instance.CloseAll();
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.SwordTomb);
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("bicanh_5_sword_tomb.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 7. Test Màn hình Kết Quả & Phát Thưởng
        Debug.Log("🎁 [SecretRealmQA] 7. Kiểm tra Bảng Kết Quả & Phát Thưởng...");
        SecretRealmPlayController.Instance.ShowResultModal();
        yield return new WaitForSecondsRealtime(1.5f);
        CaptureScreenshot("bicanh_6_result_modal.png");
        yield return new WaitForSecondsRealtime(1.5f);

        Debug.Log("🎉 [SecretRealmQA] HOÀN TẤT KIỂM THỬ THÀNH CÔNG TOÀN BỘ 5 MINI-GAME BÍ CẢNH TRÊN PLAY MODE!");
        yield return new WaitForSecondsRealtime(1.0f);
        EditorApplication.isPlaying = false;
    }

    private void CaptureScreenshot(string filename)
    {
        string localPath = Path.Combine("Temp", filename);
        if (!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
        ScreenCapture.CaptureScreenshot(localPath);

        string brainPath = Path.Combine(ArtifactDir, filename);
        try
        {
            ScreenCapture.CaptureScreenshot(brainPath);
            Debug.Log($"📸 [Capture Artifact] Đã lưu: {brainPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Could not save to artifact dir: {ex.Message}");
        }
    }
}
#endif

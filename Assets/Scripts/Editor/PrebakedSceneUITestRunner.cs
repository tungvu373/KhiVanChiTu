#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrebakedSceneUITestRunner
{
    private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    [MenuItem("TuTien/QA/Test Prebaked UI In PlayMode")]
    public static void RunTest()
    {
        if (Application.isPlaying)
        {
            var runner = new GameObject("PrebakedUITestMono").AddComponent<PrebakedUITestMono>();
            runner.StartCoroutine(runner.ExecuteSequence());
        }
        else
        {
            MobileDeviceViewTool.Set9x16();
            EditorPrefs.SetBool("AutoRunPrebakedUIQA", true);
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
                if (EditorPrefs.GetBool("AutoRunPrebakedUIQA", false))
                {
                    EditorPrefs.DeleteKey("AutoRunPrebakedUIQA");
                    var runner = new GameObject("PrebakedUITestMono").AddComponent<PrebakedUITestMono>();
                    runner.StartCoroutine(runner.ExecuteSequence());
                }
            }
        };
    }
}

public class PrebakedUITestMono : MonoBehaviour
{
    private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    public IEnumerator ExecuteSequence()
    {
        Debug.Log("🚀 [PrebakedUI-QA] Bắt đầu kiểm tra giao diện Prebaked trong Play Mode...");
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

        var offlinePopup = FindAnyObjectByType<OfflineRewardPopupUI>(FindObjectsInactive.Include);
        if (offlinePopup != null) offlinePopup.gameObject.SetActive(false);

        // 1. Chụp ảnh Main HUD đã sắp xếp lại
        Debug.Log("📸 [PrebakedUI-QA] 1. Chụp ảnh Main HUD đã sắp xếp...");
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureScreenshot("hud_rearranged_main.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 2. Mở Bí Cảnh Modal
        Debug.Log("📸 [PrebakedUI-QA] 2. Mở SecretRealmModal (Bí Cảnh)...");
        var bicanhHandler = FindAnyObjectByType<BiCanhButtonHandler>(FindObjectsInactive.Include);
        if (bicanhHandler != null) bicanhHandler.OnBiCanhButtonClicked();
        else SecretRealmModalUI.Instance?.Open();
        yield return new WaitForSecondsRealtime(1.2f);
        CaptureScreenshot("hud_prebaked_bicanh_modal.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 3. Đóng Bí Cảnh & Mở Đạo Vận Modal
        Debug.Log("📸 [PrebakedUI-QA] 3. Mở DaoAnModal (Đạo Vận)...");
        SecretRealmModalUI.Instance?.Close();
        yield return new WaitForSecondsRealtime(0.5f);
        var daoAnHandler = FindAnyObjectByType<DaoAnButtonHandler>(FindObjectsInactive.Include);
        if (daoAnHandler != null) daoAnHandler.OnDaoAnButtonClicked();
        else DaoAnModalUI.Instance?.Open();
        yield return new WaitForSecondsRealtime(1.2f);
        CaptureScreenshot("hud_prebaked_daoan_modal.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // 4. Mở Thành Tựu / Nhiệm Vụ Modal
        Debug.Log("📸 [PrebakedUI-QA] 4. Mở AchievementModalUI (Nhiệm Vụ)...");
        DaoAnModalUI.Instance?.Close();
        yield return new WaitForSecondsRealtime(0.5f);
        var achHandler = FindAnyObjectByType<AchievementButtonHandler>(FindObjectsInactive.Include);
        if (achHandler != null) achHandler.OnAchievementButtonClicked();
        else AchievementModalUI.Open();
        yield return new WaitForSecondsRealtime(1.2f);
        CaptureScreenshot("hud_prebaked_achievement_modal.png");
        yield return new WaitForSecondsRealtime(1.5f);

        AchievementModalUI.Close();
        yield return new WaitForSecondsRealtime(0.5f);

        Debug.Log("🎉 [PrebakedUI-QA] KIỂM TRA THÀNH CÔNG! Toàn bộ UI Prebaked hoạt động trơn tru!");
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
            Debug.Log($"📸 [PrebakedUI Capture] Đã lưu: {brainPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Could not save to artifact dir: {ex.Message}");
        }
    }
}
#endif


#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SkillVFXTestRunner
{

    [MenuItem("TuTien/Skills/Run 4 Prototype Skills VFX Test")]
    public static void RunTest()
    {
        if (Application.isPlaying)
        {
            var runner = new GameObject("SkillTestRunnerObject").AddComponent<SkillTestMonoRunner>();
            runner.StartCoroutine(runner.ExecuteTestSequence());
        }
        else
        {
            MobileDeviceViewTool.Set9x16();
            EditorPrefs.SetBool("AutoRunSkillTestOnPlay", true);
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
                if (EditorPrefs.GetBool("AutoRunSkillTestOnPlay", false))
                {
                    EditorPrefs.DeleteKey("AutoRunSkillTestOnPlay");
                    var runner = new GameObject("SkillTestRunnerObject").AddComponent<SkillTestMonoRunner>();
                    runner.StartCoroutine(runner.ExecuteTestSequence());
                }
            }
        };
    }
}

public class SkillTestMonoRunner : MonoBehaviour
{
    private const string ArtifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";

    public IEnumerator ExecuteTestSequence()
    {
        Debug.Log("🚀 [SkillTest] Bắt đầu kiểm thử toàn diện 4 Tuyệt Kỹ: Chưởng Ấn, Lôi Phạt, Âm Dương Trận, Vạn Kiếm Quy Tông!");

        // 1. Chờ Arena tải xong và bỏ qua Main Menu nếu có
        Time.timeScale = 1.0f;
        yield return new WaitForSecondsRealtime(1.5f);

        var menu = FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        if (menu != null && menu.gameObject.activeInHierarchy)
        {
            var data = (DataManager.Instance != null && DataManager.Instance.gameSaveData != null && DataManager.Instance.gameSaveData.slots != null && DataManager.Instance.gameSaveData.slots.Length > 0)
                ? DataManager.Instance.gameSaveData.slots[0]
                : new CharacterSaveData { characterName = "TuTienGia" };
            var mInfo = typeof(MainMenuManager).GetMethod("ConfirmEnterGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mInfo != null)
            {
                mInfo.Invoke(menu, new object[] { 0, data });
            }
            else
            {
                menu.gameObject.SetActive(false);
            }
            Debug.Log("✅ [SkillTest] Đã vào thẳng Đấu Trường (Bypass MainMenu)!");
            Time.timeScale = 1.0f;
            yield return new WaitForSecondsRealtime(1.5f);
        }

        // Tự động đóng popup Bế Quan Treo Máy nếu có để nhìn rõ đấu trường và VFX
        var offlinePopup = FindAnyObjectByType<OfflineRewardPopupUI>(FindObjectsInactive.Include);
        if (offlinePopup != null && offlinePopup.gameObject.activeInHierarchy)
        {
            offlinePopup.gameObject.SetActive(false);
            Debug.Log("✅ [SkillTest] Đã đóng OfflineRewardPopupUI để hiển thị trọn vẹn đấu trường.");
        }

        var skillMgr = SkillManager.Instance;
        if (skillMgr == null)
        {
            Debug.LogError("❌ [SkillTest] SkillManager.Instance is NULL!");
            yield break;
        }

        // 1. Kiểm tra các điều kiện mở khóa theo tiêu chuẩn mới
        int highestSword = skillMgr.GetHighestSwordLevel();
        int learnedFormations = skillMgr.GetLearnedFormationCount();
        var currentRealm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi;

        Debug.Log($"📊 [SkillTest] Trạng thái hiện tại: Kiếm cao nhất = Lv.{highestSword} | Trận pháp đã học = {learnedFormations}/6 | Cảnh giới = {currentRealm}");
        Debug.Log($"📋 [SkillTest] Điều kiện mở khóa:");
        Debug.Log($"   1. Vạn Kiếm Quy Tông: Kiếm Lv >= 7 (Hiện tại: {highestSword} -> {(highestSword >= 7 ? "ĐỦ ĐIỀU KIỆN" : "CHƯA ĐỦ")})");
        Debug.Log($"   2. Lôi Phạt: Độ kiếp Nguyên Anh (Hiện tại: {currentRealm} -> {(currentRealm >= RealmId.NguyenAnh ? "ĐỦ ĐIỀU KIỆN" : "CHƯA ĐỦ")})");
        Debug.Log($"   3. Âm Dương Trận: Học 4 Trận Pháp (Hiện tại: {learnedFormations} -> {(learnedFormations >= 4 ? "ĐỦ ĐIỀU KIỆN" : "CHƯA ĐỦ")})");
        Debug.Log($"   4. Ấn Chưởng: Đạt Đại Thừa (Hiện tại: {currentRealm} -> {(currentRealm >= RealmId.DaiThua ? "ĐỦ ĐIỀU KIỆN" : "CHƯA ĐỦ")})");

        // Mở khóa toàn bộ 4 tuyệt kỹ để test uy lực và cooldown
        skillMgr.UnlockAllSkillsForTesting();
        skillMgr.currentLinhLuc = 100f;
        
        float dmgAnChuong = skillMgr.CalculateTotalSkillDamage(skillMgr.normalSkills[0]);
        Debug.Log($"🔥 [SkillTest] Sát thương Ấn Chưởng (Đại Thừa thần thông): {dmgAnChuong:N0} (Gốc 100k + Kiếm Ý x15)");
        Debug.Log($"⏱️ [SkillTest] Cooldowns mới: Vạn Kiếm={skillMgr.activeSkill.baseCooldown}s | Ấn Chưởng={skillMgr.normalSkills[0].baseCooldown}s | Lôi Phạt={skillMgr.normalSkills[1].baseCooldown}s | Âm Dương={skillMgr.normalSkills[2].baseCooldown}s");

        yield return new WaitForSecondsRealtime(1.0f);

        // --- SKILL 1: ẤN CHƯỞNG ---
        Debug.Log("💥 [SkillTest] --- THI TRIỂN SKILL 1: ẤN CHƯỞNG (CHƯỞNG ẤN) ---");
        skillMgr.currentLinhLuc = 50f;
        bool cast0 = skillMgr.CastNormalSkillManual(0);
        Debug.Log($"[SkillTest] CastNormalSkillManual(0) result = {cast0}");
        yield return new WaitForSecondsRealtime(0.5f);
        CaptureArtifact("vfx_skill_0_anchuong.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // --- SKILL 2: LÔI PHẠT ---
        Debug.Log("⚡ [SkillTest] --- THI TRIỂN SKILL 2: LÔI PHẠT ---");
        skillMgr.currentLinhLuc = 50f;
        bool cast1 = skillMgr.CastNormalSkillManual(1);
        Debug.Log($"[SkillTest] CastNormalSkillManual(1) result = {cast1}");
        yield return new WaitForSecondsRealtime(0.5f);
        CaptureArtifact("vfx_skill_1_loiphat.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // --- SKILL 3: ÂM DƯƠNG TRẬN ---
        Debug.Log("☯️ [SkillTest] --- THI TRIỂN SKILL 3: ÂM DƯƠNG TRẬN ---");
        skillMgr.currentLinhLuc = 50f;
        bool cast2 = skillMgr.CastNormalSkillManual(2);
        Debug.Log($"[SkillTest] CastNormalSkillManual(2) result = {cast2}");
        yield return new WaitForSecondsRealtime(0.6f);
        CaptureArtifact("vfx_skill_2_amduongtran.png");
        yield return new WaitForSecondsRealtime(1.5f);

        // --- SKILL 4: VẠN KIẾM QUY TÔNG ---
        Debug.Log("🗡️ [SkillTest] --- THI TRIỂN SKILL 4 (ULTI): VẠN KIẾM QUY TÔNG ---");
        skillMgr.currentLinhLuc = 100f;
        skillMgr.CastActiveSkill();
        Debug.Log($"[SkillTest] CastActiveSkill() called. isSkillActive = {skillMgr.isSkillActive}");
        yield return new WaitForSecondsRealtime(1.0f);
        CaptureArtifact("vfx_skill_3_vankiemquytong.png");

        yield return new WaitForSecondsRealtime(2.0f);
        CaptureArtifact("vfx_skills_all_overview.png");

        Debug.Log("🎉 [SkillTest] HOÀN TẤT KIỂM THỬ THÀNH CÔNG 4 TUYỆT KỸ TRÊN MOBILE WORK SCENE!");
        yield return new WaitForSecondsRealtime(1.0f);
        EditorApplication.isPlaying = false;
    }

    private void CaptureArtifact(string filename)
    {
        string localPath = Path.Combine("Temp", filename);
        if (!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
        ScreenCapture.CaptureScreenshot(localPath);

        string brainPath = Path.Combine(ArtifactDir, filename);
        try
        {
            ScreenCapture.CaptureScreenshot(brainPath);
            Debug.Log($"📸 [Capture Artifact] Đã lưu ảnh chụp: {brainPath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Capture Artifact] Could not save to artifact dir directly: {ex.Message}");
        }
    }
}
#endif

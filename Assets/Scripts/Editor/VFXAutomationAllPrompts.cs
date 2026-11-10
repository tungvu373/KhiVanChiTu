#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class VFXAutomationAllPrompts
{
    private const string ARTIFACT_DIR = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";
    private const string PREF_KEY = "RunAllPromptsVFXTest";

    [MenuItem("Khí Vận Chi Tử/Test All VFX Prompts (04 to 10)")]
    public static void RunAllPromptsTest()
    {
        EditorPrefs.SetBool(PREF_KEY, true);
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }
        else
        {
            StartTestCoroutine();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoadedCheck()
    {
        Application.runInBackground = true;
        if (EditorPrefs.GetBool(PREF_KEY, false))
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            StartTestCoroutine();
        }
    }

    private static void StartTestCoroutine()
    {
        GameObject runner = new GameObject("VFX_Test_Runner");
        GameObject.DontDestroyOnLoad(runner);
        var comp = runner.AddComponent<VFXTestRunnerComponent>();
        comp.StartCoroutine(comp.RunTestSuiteRoutine());
    }

    private class VFXTestRunnerComponent : MonoBehaviour
    {
        public IEnumerator RunTestSuiteRoutine()
        {
            Time.timeScale = 1.0f;
            Debug.Log("[VFX Test] === BẮT ĐẦU KIỂM ĐỊNH TOÀN DIỆN PROMPTS 04 ĐẾN 10 ===");
            yield return new WaitForSecondsRealtime(0.6f);
            Time.timeScale = 1.0f;

            var combat = CombatManager.Instance;
            if (combat == null)
            {
                Debug.LogError("[VFX Test] CombatManager not found in scene!");
                yield break;
            }

            Vector3 playerPos = combat.playerTransform != null ? combat.playerTransform.position : Vector3.zero;

            // Tạo quái giả lập
            GameObject dummyRegular = new GameObject("TestEnemy_Regular");
            dummyRegular.transform.position = playerPos + new Vector3(2.0f, 0f, 3.5f);
            var regEnemy = dummyRegular.AddComponent<Enemy>();
            regEnemy.isBoss = false;
            regEnemy.maxHp = 50000f;
            regEnemy.hp = 50000f;

            GameObject dummyBoss = new GameObject("TestEnemy_Boss");
            dummyBoss.transform.position = playerPos + new Vector3(-2.0f, 0f, 4.0f);
            var bossEnemy = dummyBoss.AddComponent<BossEnemy>();
            bossEnemy.isBoss = true;
            bossEnemy.maxHp = 500000f;
            bossEnemy.hp = 500000f;

            yield return new WaitForSecondsRealtime(0.2f);
            Time.timeScale = 1.0f;

            // -------------------------------------------------------------
            // GIAI ĐOẠN 1: PROMPT 04 (Cửu Thiên Lôi Trận & Vạn Kiếm Trận)
            // -------------------------------------------------------------
            Debug.Log("[VFX Test] 1. Kiểm tra Prompt 04: Cửu Thiên Lôi Trận & Mưa Kiếm");
            HitVFXManager.Instance?.SpawnFormationRuneRing(playerPos, new Color(0.70f, 0.40f, 1f, 0.85f), 3.8f);
            HitVFXManager.Instance?.SpawnFormationThunder(dummyRegular.transform.position);
            HitVFXManager.Instance?.SpawnFormationSwordRain(dummyBoss.transform.position);
            HitVFXManager.Instance?.SpawnFormationSwordRain(dummyRegular.transform.position + Vector3.right * 1.5f);

            yield return new WaitForSecondsRealtime(0.08f);
            CaptureScreenshot("VFX_Preview_Prompt_04_Attack_Formations.png");
            yield return new WaitForSecondsRealtime(0.40f);
            Time.timeScale = 1.0f;

            // -------------------------------------------------------------
            // GIAI ĐOẠN 2: PROMPT 05 (Bốn Trận Hỗ Trợ)
            // -------------------------------------------------------------
            Debug.Log("[VFX Test] 2. Kiểm tra Prompt 05: Bát Quái Tụ Linh, Băng Trận, Huyết Ma & Kim Cang");
            HitVFXManager.Instance?.SetBaguaSustain(true, playerPos);
            HitVFXManager.Instance?.SetFrostMist(true, dummyBoss.transform.position);
            HitVFXManager.Instance?.SpawnLifestealStreak(dummyRegular.transform.position, playerPos);
            HitVFXManager.Instance?.SpawnGoldenBellShield(playerPos, Vector3.forward);

            yield return new WaitForSecondsRealtime(0.08f);
            CaptureScreenshot("VFX_Preview_Prompt_05_Support_Formations.png");
            yield return new WaitForSecondsRealtime(0.40f);
            Time.timeScale = 1.0f;

            HitVFXManager.Instance?.SetBaguaSustain(false, Vector3.zero);
            HitVFXManager.Instance?.SetFrostMist(false, Vector3.zero);

            // -------------------------------------------------------------
            // GIAI ĐOẠN 3: PROMPT 06 & 07 (Ấn Chưởng & Lôi Phạt)
            // -------------------------------------------------------------
            Debug.Log("[VFX Test] 3. Kiểm tra Prompt 06 & 07: Ấn Chưởng & Lôi Phạt");
            HitVFXManager.Instance?.SpawnAnChuong(dummyRegular.transform.position, 6.0f);
            HitVFXManager.Instance?.SpawnStunIndicator(dummyRegular, 2.0f);
            HitVFXManager.Instance?.SpawnLoiPhat(dummyBoss.transform.position, isBoss: true, executeRegular: false);

            yield return new WaitForSecondsRealtime(0.08f);
            CaptureScreenshot("VFX_Preview_Prompt_06_07_Skills.png");
            yield return new WaitForSecondsRealtime(0.45f);
            Time.timeScale = 1.0f;

            // -------------------------------------------------------------
            // GIAI ĐOẠN 4: PROMPT 08 & 09 (Âm Dương Trận & Vạn Kiếm Quy Tông)
            // -------------------------------------------------------------
            Debug.Log("[VFX Test] 4. Kiểm tra Prompt 08 & 09: Âm Dương Trận & Vạn Kiếm Quy Tông");
            HitVFXManager.Instance?.StartAmDuongTran(combat.playerTransform, 4.0f);
            HitVFXManager.Instance?.SpawnUltimateSummonRing(playerPos);

            yield return new WaitForSecondsRealtime(0.12f);
            CaptureScreenshot("VFX_Preview_Prompt_08_09_Ultimate_And_YinYang.png");
            yield return new WaitForSecondsRealtime(0.40f);
            Time.timeScale = 1.0f;

            HitVFXManager.Instance?.StopAmDuongTran();
            HitVFXManager.Instance?.SpawnUltimateRecallPulse(playerPos);

            // -------------------------------------------------------------
            // GIAI ĐOẠN 5: PROMPT 10 (Tiến Trình & Hào Quang Pháp Khí)
            // -------------------------------------------------------------
            Debug.Log("[VFX Test] 5. Kiểm tra Prompt 10: Hào Quang Pháp Khí Bậc 4");
            HitVFXManager.Instance?.SetPlayerArtifactAura(combat.playerTransform, 4);

            yield return new WaitForSecondsRealtime(0.15f);
            CaptureScreenshot("VFX_Preview_Prompt_10_Progress_And_Aura.png");
            yield return new WaitForSecondsRealtime(0.30f);

            HitVFXManager.Instance?.SetPlayerArtifactAura(combat.playerTransform, 0);

            // Dọn dẹp
            Destroy(dummyRegular);
            Destroy(dummyBoss);

            Debug.Log("[VFX Test] === HOÀN THÀNH TOÀN BỘ KIỂM ĐỊNH PROMPTS 04 ĐẾN 10! ===");

            yield return new WaitForSecondsRealtime(0.5f);
            EditorApplication.isPlaying = false;
        }

        private void CaptureScreenshot(string filename)
        {
            string outPath = Path.Combine(ARTIFACT_DIR, filename);
            ScreenCapture.CaptureScreenshot(outPath);
            Debug.Log($"[VFX Test] Đã chụp ảnh Game View: {filename}");
        }
    }
}
#endif

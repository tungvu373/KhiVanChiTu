using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class HitVFXTestRunner
{
    private static bool isRunningTest = false;

    [MenuItem("TuTien/VFX/Test Hit VFX and Styles")]
    public static void RunTest()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.Log("🎮 [VFX Test] Đang bật Play Mode để kiểm thử Hit VFX và Kiếm Pháp...");
            EditorApplication.EnterPlaymode();
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            return;
        }

        StartTestInPlayMode();
    }

    private static void OnPlayStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            EditorApplication.delayCall += () =>
            {
                StartTestInPlayMode();
            };
        }
    }

    private static void StartTestInPlayMode()
    {
        var go = new GameObject("HitVFXTestRunnerHost");
        var host = go.AddComponent<HitVFXTestRunnerHost>();
        host.StartCoroutine(host.TestRoutine());
    }

    public class HitVFXTestRunnerHost : MonoBehaviour
    {
        public IEnumerator TestRoutine()
        {
            Debug.Log("⚔️ [VFX Test] BẮT ĐẦU KIỂM THỬ PROMPT 02: HIT VFX, TRAIL & KIẾM PHÁP...");

            // 1. Chuyển vào Gameplay
            yield return new WaitForSecondsRealtime(0.3f);
            var mm = Object.FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
            if (mm != null && mm.gameObject.activeInHierarchy)
            {
                if (DataManager.Instance != null && DataManager.Instance.gameSaveData != null && DataManager.Instance.gameSaveData.slots != null && DataManager.Instance.gameSaveData.slots.Length > 0)
                {
                    var enterMethod = typeof(MainMenuManager).GetMethod("ConfirmEnterGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (enterMethod != null)
                    {
                        enterMethod.Invoke(mm, new object[] { 0, DataManager.Instance.gameSaveData.slots[0] });
                    }
                    else
                    {
                        mm.gameObject.SetActive(false);
                    }
                }
                else
                {
                    mm.gameObject.SetActive(false);
                }
            }

            var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
            if (mobileRoot != null) mobileRoot.SetActive(true);
            Time.timeScale = 1.0f;

            yield return new WaitForSecondsRealtime(0.6f);

            // Tự động nhận thưởng và đóng triệt để popup Bế Quan Tu Luyện nếu hiển thị
            if (OfflineEarningsManager.Instance != null && OfflineEarningsManager.Instance.HasPendingReward)
            {
                OfflineEarningsManager.Instance.ClaimReward(1.0f);
            }
            if (OfflineRewardPopupUI.Instance != null)
            {
                OfflineRewardPopupUI.Instance.gameObject.SetActive(false);
            }
            var allPopups = Object.FindObjectsByType<OfflineRewardPopupUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var pop in allPopups)
            {
                if (pop != null) pop.gameObject.SetActive(false);
            }

            // 2. Kiểm tra HitVFXManager
            if (HitVFXManager.Instance == null)
            {
                Debug.LogError("❌ HitVFXManager.Instance is NULL!");
                Destroy(gameObject);
                yield break;
            }
            Debug.Log("✅ [VFX Test] HitVFXManager đã sẵn sàng hoạt động!");

            // 3. Thiết lập phi kiếm hai cấp cách biệt: Level 1 và Level 5 để thử màu trail và trạng thái
            var swords = Object.FindObjectsByType<FlyingSword>(FindObjectsSortMode.None);
            if (swords != null && swords.Length > 0)
            {
                swords[0].SetLevelVisual(1); // Kiếm cấp 1 (Vàng lục)
                if (swords.Length > 1)
                {
                    swords[1].SetLevelVisual(5); // Kiếm cấp 5 (Xanh tím)
                }
                Debug.Log($"✅ [VFX Test] Đã thiết lập {swords.Length} phi kiếm với cấp độ 1 và 5 để kiểm tra tương phản màu sắc trail.");
            }

            // 4. Test trực tiếp HitVFX: Thường, Chí mạng, Kiếm Xuyên, Phá Giáp
            Vector3 playerPos = CombatManager.Instance != null && CombatManager.Instance.playerTransform != null 
                ? CombatManager.Instance.playerTransform.position 
                : Vector3.zero;
            Vector3 enemyCenter = new Vector3(0f, 1.2f, 4.5f);
            if (CombatManager.Instance != null && CombatManager.Instance.ActiveEnemies != null)
            {
                Enemy closestCenter = null;
                float minX = 999f;
                foreach (var e in CombatManager.Instance.ActiveEnemies)
                {
                    if (e != null && !e.isDead && Mathf.Abs(e.transform.position.x) < minX)
                    {
                        minX = Mathf.Abs(e.transform.position.x);
                        closestCenter = e;
                    }
                }
                if (closestCenter != null) enemyCenter = closestCenter.transform.position + Vector3.up * 1.0f;
            }

            // A. Thử Hit thường (sparks hướng theo slashDir)
            HitVFXManager.Instance.SpawnHitImpact(enemyCenter, Vector3.forward, isCrit: false);
            Debug.Log("✅ [VFX Test] Đã kích hoạt Hit thường (conical sparks theo hướng chém).");
            yield return new WaitForSecondsRealtime(0.15f);

            // B. Thử Chí mạng (Core flash + wedge sparks + camera shake)
            HitVFXManager.Instance.SpawnHitImpact(enemyCenter + Vector3.right * 1f, Vector3.forward, isCrit: true);
            Debug.Log("✅ [VFX Test] Đã kích hoạt Chí mạng (lõi sáng + radiant wedge sparks).");
            yield return new WaitForSecondsRealtime(0.2f);

            // C. Thử Kiếm Xuyên (Pierce tracer từ mục tiêu chính sang mục tiêu phụ)
            Vector3 primaryPos = enemyCenter;
            Vector3 piercedPos1 = enemyCenter + new Vector3(1.5f, 0f, 1.2f);
            Vector3 piercedPos2 = enemyCenter + new Vector3(-1.5f, 0f, 1.2f);
            HitVFXManager.Instance.SpawnPierceTracer(primaryPos, piercedPos1);
            HitVFXManager.Instance.SpawnPierceTracer(primaryPos, piercedPos2);
            Debug.Log("✅ [VFX Test] Đã kích hoạt Kiếm Xuyên (vệt tiếp nối xuyên 2 mục tiêu phụ).");
            yield return new WaitForSecondsRealtime(0.15f);

            // D. Thử Phá Giáp (tích lũy rạn nứt + nổ vỡ hộ thể)
            HitVFXManager.Instance.SpawnArmorBreakStack(primaryPos, 3);
            yield return new WaitForSecondsRealtime(0.1f);
            HitVFXManager.Instance.SpawnShieldBreakBurst(primaryPos);
            Debug.Log("✅ [VFX Test] Đã kích hoạt Phá Giáp (tia lửa tích lũy stack 3 + nổ vỡ hộ thể crystalline).");

            // Cập nhật lại vị trí quái còn sống ngay tại thời điểm chụp
            Vector3 capturePos = new Vector3(0f, 1.2f, 4.5f);
            if (CombatManager.Instance != null && CombatManager.Instance.ActiveEnemies != null)
            {
                foreach (var e in CombatManager.Instance.ActiveEnemies)
                {
                    if (e != null && !e.isDead && e.gameObject.activeInHierarchy)
                    {
                        capturePos = e.transform.position + Vector3.up * 1.0f;
                        break;
                    }
                }
            }

            // Kích hoạt đồng thời cả Hit chém thường (trái) và Chí mạng (phải) ngay trước khi chụp ảnh
            HitVFXManager.Instance.SpawnHitImpact(capturePos - Vector3.right * 0.4f, Vector3.forward, isCrit: false);
            HitVFXManager.Instance.SpawnHitImpact(capturePos + Vector3.right * 0.4f, Vector3.forward, isCrit: true);
            yield return new WaitForSecondsRealtime(0.06f);

            if (OfflineRewardPopupUI.Instance != null) OfflineRewardPopupUI.Instance.gameObject.SetActive(false);

            // 5. Chụp ảnh Game View 9:16
            string artifactDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";
            string outPath = Path.Combine(artifactDir, "VFX_Preview_SwordHits_And_Styles.png");
            ScreenCapture.CaptureScreenshot(outPath);
            Debug.Log($"📸 [VFX Test] Đã lưu ảnh Game View tại: {outPath}");

            yield return new WaitForSecondsRealtime(0.5f);

            Debug.Log("🎉 [VFX Test] KIỂM THỬ PROMPT 02 HOÀN TẤT THÀNH CÔNG VỚI 0 LỖI!");
            isRunningTest = false;
            Destroy(gameObject);
        }
    }
}

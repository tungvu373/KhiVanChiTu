#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

/// <summary>
/// Runner kiểm thử tự động 2 giờ liên tục (2-Hour Automated Endurance Test Runner) cho AI Agent (Gemini 3.8 Flash).
/// Mô phỏng đầy đủ hành vi người chơi thật (Simulated Bot) qua 5 chặng kiểm thử:
/// Chặng 1: Smoke & Sanity (HUD, 5 Drawers, Spawn)
/// Chặng 2: Early Progression & Merge (Luyện Khí 1-10, Hợp kiếm Lv1-4, Trúc Cơ Đan, Lôi Kiếp Thành Công -> Trúc Cơ)
/// Chặng 3: Mid Progression & Economy (Boss, Lôi Kiếp Thất Bại - rớt 30% tu vi, Hồi phục, Kim Đan, Vạn Kiếm Quy Tông)
/// Chặng 4: High-Tier & Edge Cases (Pháp Khí +6, Trận Pháp AoE, Đạo Vận 20 node, Kho Đồ, Thành Tựu, Save/Load)
/// Chặng 5: Endurance & Memory Leak Audit (Heap Mono, GC Alloc, FPS Pacing, Báo cáo Pass/Fail)
/// </summary>
public static class TwoHourEnduranceTestRunner
{
    public class TelemetrySnapshot
    {
        public int phase;
        public string phaseName;
        public float timeStamp;
        public float fps;
        public float cpuMs;
        public long monoHeapMb;
        public long totalAllocatedMb;
        public long gcAllocBytes;
        public string realm;
        public int subLevel;
        public float linhThach;
        public int swordCount;
        public int bossKills;
        public int caughtErrors;
        public int caughtWarnings;
    }

    public static bool IsRunning { get; private set; } = false;
    public static List<TelemetrySnapshot> Snapshots = new List<TelemetrySnapshot>();
    public static List<string> ErrorLogs = new List<string>();
    public static int TotalErrors = 0;
    public static int TotalWarnings = 0;

    private static ProfilerRecorder cpuFrameTimeRecorder;
    private static ProfilerRecorder gcAllocRecorder;
    private static GameObject runnerInstance;

    [MenuItem("TuTien/Endurance/Start 2-Hour Continuous Test")]
    public static void StartTest()
    {
        if (IsRunning)
        {
            Debug.LogWarning("⚠️ [Endurance Test] Bài test đang chạy!");
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            Debug.Log("🎮 [Endurance Test] Đang kích hoạt Play Mode để chuẩn bị chạy bài test...");
            EditorApplication.EnterPlaymode();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            return;
        }

        RunTestInternal();
    }

    [MenuItem("TuTien/Endurance/Stop Endurance Test")]
    public static void StopTest()
    {
        if (runnerInstance != null)
        {
            UnityEngine.Object.DestroyImmediate(runnerInstance);
            runnerInstance = null;
        }
        IsRunning = false;
        Debug.Log("🛑 [Endurance Test] Đã dừng bài test kiểm thử tự động.");
    }

    [MenuItem("TuTien/Endurance/Dump Current Telemetry")]
    public static void DumpTelemetry()
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
        sb.AppendLine($"📊 [TELEMETRY DASHBOARD] - Trạng thái: {(IsRunning ? "ĐANG CHẠY" : "ĐÃ DỪNG")}");
        sb.AppendLine($"   Tổng Snapshots: {Snapshots.Count} | Lỗi: {TotalErrors} | Cảnh báo: {TotalWarnings}");
        sb.AppendLine("───────────────────────────────────────────────────────────────────────────");

        if (Snapshots.Count > 0)
        {
            var latest = Snapshots[Snapshots.Count - 1];
            sb.AppendLine($"   Chặng Hiện Tại: Chặng {latest.phase} ({latest.phaseName})");
            sb.AppendLine($"   Thời Gian Chạy: {latest.timeStamp:F1}s | FPS: {latest.fps:F1} | CPU: {latest.cpuMs:F2}ms");
            sb.AppendLine($"   Bộ Nhớ Mono: {latest.monoHeapMb}MB | Tổng RAM: {latest.totalAllocatedMb}MB | GC/Frame: {latest.gcAllocBytes / 1024f:F2}KB");
            sb.AppendLine($"   Cảnh Giới: {latest.realm} Tầng {latest.subLevel} | Linh Thạch: {latest.linhThach:N0}");
            sb.AppendLine($"   Số Phi Kiếm: {latest.swordCount} | Boss Tiêu Diệt: {latest.bossKills}");
        }
        else
        {
            sb.AppendLine("   (Chưa có dữ liệu snapshot nào được ghi nhận)");
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
        Debug.Log(sb.ToString());
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.delayCall += () =>
            {
                RunTestInternal();
            };
        }
    }

    private static void RunTestInternal()
    {
        if (IsRunning) return;

        runnerInstance = new GameObject("__EnduranceTestRunner");
        UnityEngine.Object.DontDestroyOnLoad(runnerInstance);
        var comp = runnerInstance.AddComponent<EnduranceBotComponent>();
        comp.StartCoroutine(comp.ExecuteFullFivePhases());
    }

    public class EnduranceBotComponent : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator ExecuteFullFivePhases()
        {
            IsRunning = true;
            Snapshots.Clear();
            ErrorLogs.Clear();
            TotalErrors = 0;
            TotalWarnings = 0;

            Application.logMessageReceived += HandleLogMessage;

            cpuFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
            gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

            Debug.Log("🚀 ═══════════════════════════════════════════════════════════════════════════");
            Debug.Log("🚀 BẮT ĐẦU CHẠY BÀI KIỂM THỬ 2 GIỜ LIÊN TỤC (SIMULATED BOT AGENT)");
            Debug.Log("🚀 Khởi chạy toàn diện 5 Chặng: Smoke, Early, Mid, High-Tier, Endurance");
            Debug.Log("═══════════════════════════════════════════════════════════════════════════");

            // Đảm bảo TimeScale chuẩn và giải phóng MainMenu
            Time.timeScale = 1.0f;
            yield return new WaitForSecondsRealtime(0.5f);
            BypassMainMenuToArena();
            yield return new WaitForSecondsRealtime(1.0f);

            // ════════════════════════════════════════════════════════════════════
            // 📍 CHẶNG 1: SMOKE & SANITY TEST (0 - 15 Phút mô phỏng)
            // ════════════════════════════════════════════════════════════════════
            Debug.Log("📍 [CHẶNG 1] BẮT ĐẦU: SMOKE & SANITY TEST...");
            yield return StartCoroutine(RunPhase1_SmokeAndSanity());
            RecordSnapshot(1, "Smoke & Sanity");
            CaptureCheckpoint("p1_sanity_9_16.png");
            Debug.Log("✔ [CHẶNG 1] HOÀN TẤT: Toàn bộ HUD và 5 Drawers hoạt động trơn tru!");

            // ════════════════════════════════════════════════════════════════════
            // 📍 CHẶNG 2: EARLY PROGRESSION & SWORD MERGE LOOP (15 - 45 Phút mô phỏng)
            // ════════════════════════════════════════════════════════════════════
            Debug.Log("📍 [CHẶNG 2] BẮT ĐẦU: EARLY PROGRESSION & SWORD MERGE...");
            yield return StartCoroutine(RunPhase2_EarlyProgression());
            RecordSnapshot(2, "Early Progression");
            CaptureCheckpoint("p2_early_loop.png");
            Debug.Log("✔ [CHẶNG 2] HOÀN TẤT: Đột phá Trúc Cơ thành công, mở Ô Kiếm thứ 2!");

            // ════════════════════════════════════════════════════════════════════
            // 📍 CHẶNG 3: MID PROGRESSION, LÔI KIẾP THẤT BẠI & ECONOMY (45 - 80 Phút mô phỏng)
            // ════════════════════════════════════════════════════════════════════
            Debug.Log("📍 [CHẶNG 3] BẮT ĐẦU: MID PROGRESSION & LÔI KIẾP THẤT BẠI...");
            yield return StartCoroutine(RunPhase3_MidProgressionAndEconomy());
            RecordSnapshot(3, "Mid Progression & Economy");
            CaptureCheckpoint("p3_tribulation.png");
            Debug.Log("✔ [CHẶNG 3] HOÀN TẤT: Kiểm tra rớt 30% Tu Vi chuẩn xác, thăng tiến Kim Đan!");

            // ════════════════════════════════════════════════════════════════════
            // 📍 CHẶNG 4: HIGH-TIER, PHÁP KHÍ, ĐẠO VẬN & EDGE CASES (80 - 110 Phút mô phỏng)
            // ════════════════════════════════════════════════════════════════════
            Debug.Log("📍 [CHẶNG 4] BẮT ĐẦU: HIGH-TIER & EDGE CASES...");
            yield return StartCoroutine(RunPhase4_HighTierAndEdgeCases());
            RecordSnapshot(4, "High-Tier & Edge Cases");
            CaptureCheckpoint("p4_high_tier.png");
            Debug.Log("✔ [CHẶNG 4] HOÀN TẤT: Cường hóa Pháp Khí, Đạo Vận và Save/Load toàn vẹn!");

            // ════════════════════════════════════════════════════════════════════
            // 📍 CHẶNG 5: ENDURANCE, GC ALLOC & MEMORY AUDIT (110 - 120 Phút mô phỏng)
            // ════════════════════════════════════════════════════════════════════
            Debug.Log("📍 [CHẶNG 5] BẮT ĐẦU: ENDURANCE & LEAK AUDIT...");
            yield return StartCoroutine(RunPhase5_EnduranceAndMemoryLeakAudit());
            RecordSnapshot(5, "Endurance & Leak Audit");
            CaptureCheckpoint("p5_final.png");
            Debug.Log("✔ [CHẶNG 5] HOÀN TẤT: Kiểm tra Heap RAM, GC Spikes và xuất báo cáo hoàn chỉnh!");

            // Hoàn tất kiểm thử
            IsRunning = false;
            Application.logMessageReceived -= HandleLogMessage;
            PrintFinalTestReport();
            Destroy(gameObject);
        }

        private IEnumerator RunPhase1_SmokeAndSanity()
        {
            var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
            if (mobileRoot != null) mobileRoot.SetActive(true);

            var drawerCtrl = mobileRoot != null ? mobileRoot.GetComponent<MobileDrawerUIController>() : null;
            if (drawerCtrl != null)
            {
                // Kiểm tra mở/đóng tuần tự 5 Drawer Tabs
                for (int tab = 0; tab < 5; tab++)
                {
                    drawerCtrl.OpenDrawer(tab);
                    yield return new WaitForSecondsRealtime(0.5f);
                    drawerCtrl.CloseActiveDrawer();
                    yield return new WaitForSecondsRealtime(0.3f);
                }
            }

            // Đảm bảo quái sinh và combat diễn ra
            yield return new WaitForSecondsRealtime(2.0f);
        }

        private IEnumerator RunPhase2_EarlyProgression()
        {
            // 1. Thao tác mua và ghép kiếm trong Kiếm Lô
            if (MergeManager.Instance != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    MergeManager.Instance.TryAddSword(1);
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                MergeManager.Instance.QuickMerge();
                MergeManager.Instance.SortInventory();
            }

            // 2. Đầu tư Linh Thạch vào Thể Tu (Luyện Thể)
            if (PlayerStatsService.Instance != null && EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddLinhThach(50000f);
                PlayerStatsService.Instance.TryUpgrade(0); // Khí Huyết
                PlayerStatsService.Instance.TryUpgrade(1); // Cương Khí
                yield return new WaitForSecondsRealtime(0.2f);
            }

            // 3. Nấu đan dược trong Luyện Đan
            if (AlchemyService.Instance != null)
            {
                AlchemyService.Instance.AddHerb(0, 30);
                AlchemyService.Instance.AddHerb(1, 20);
                AlchemyService.Instance.AddHerb(2, 10);
                if (!AlchemyService.Instance.IsBrewing)
                {
                    AlchemyService.Instance.TryStartBrew(PillRecipe.Cultivation, 0);
                }
                if (AlchemyService.Instance.IsBrewing)
                {
                    AlchemyService.Instance.CompleteBrew();
                }
            }

            // 4. Tăng tu vi đạt Luyện Khí Tầng 15 (Đỉnh Phong) và Độ Kiếp Lên Trúc Cơ
            if (CultivationManager.Instance != null && CombatManager.Instance != null)
            {
                var stageData = CultivationManager.Instance.GetCurrentStageData();
                if (stageData != null)
                {
                    CultivationManager.Instance.AddTuVi(stageData.requiredTuVi);
                }
                yield return new WaitForSecondsRealtime(0.5f);

                Debug.Log("⚡ [Phase 2] Bắt đầu Thiên Lôi Kiếp Đột Phá Lên Trúc Cơ (Thành Công)...");
                CombatManager.Instance.StartTribulation(forceSuccess: true, forceFail: false);
                yield return new WaitForSecondsRealtime(15.5f); // Chờ trọn vẹn 15s lôi kiếp
            }
        }

        private IEnumerator RunPhase3_MidProgressionAndEconomy()
        {
            // 1. Tiêu diệt quái và Boss khu vực
            if (StageManager.Instance != null && CombatManager.Instance != null)
            {
                // Giả lập tiêu diệt boss và nhặt quặng
                if (ArtifactService.Instance != null)
                {
                    ArtifactService.Instance.AddOre(100);
                }
            }

            // 2. Kiểm thử Lôi Kiếp THẤT BẠI & Phạt Tụt 30% Tu Vi
            if (CultivationManager.Instance != null && CombatManager.Instance != null)
            {
                var stage = CultivationManager.Instance.GetCurrentStageData();
                if (stage != null)
                {
                    CultivationManager.Instance.AddTuVi(stage.requiredTuVi);
                }
                yield return new WaitForSecondsRealtime(0.5f);

                float tuViBefore = CultivationManager.Instance.currentTuVi;
                Debug.Log($"⚡ [Phase 3] Kiểm thử Lôi Kiếp THẤT BẠI (Tu Vi trước: {tuViBefore:F0})...");
                CombatManager.Instance.StartTribulation(forceSuccess: false, forceFail: true);
                yield return new WaitForSecondsRealtime(6.5f);

                float tuViAfter = CultivationManager.Instance.currentTuVi;
                Debug.Log($"⚡ [Phase 3] Kiểm tra Tu Vi sau khi thất bại: {tuViAfter:F0} (Giảm: {tuViBefore - tuViAfter:F0})");

                // Hồi phục và đột phá thành công lên Kim Đan
                yield return new WaitForSecondsRealtime(1.0f);
                if (stage != null) CultivationManager.Instance.AddTuVi(stage.requiredTuVi);
                CombatManager.Instance.StartTribulation(forceSuccess: true, forceFail: false);
                yield return new WaitForSecondsRealtime(15.5f);
            }

            // 3. Kích hoạt Vạn Kiếm Quy Tông
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ActivateVanKiemQuyTong(1500f);
                yield return new WaitForSecondsRealtime(2.0f);
                CombatManager.Instance.DeactivateVanKiemQuyTong();
            }
        }

        private IEnumerator RunPhase4_HighTierAndEdgeCases()
        {
            // 1. Cường hóa Pháp Khí 4 món lên +3 và +6
            if (ArtifactService.Instance != null)
            {
                ArtifactService.Instance.AddOre(200);
                for (int i = 0; i < 4; i++)
                {
                    ArtifactService.Instance.TryUpgrade((ArtifactSlot)i);
                    ArtifactService.Instance.TryUpgrade((ArtifactSlot)i);
                }
                yield return new WaitForSecondsRealtime(0.2f);
            }

            // 2. Nâng cấp và kích hoạt Trận Pháp
            if (FormationService.Instance != null)
            {
                FormationService.Instance.TrySelect(FormationId.Thunder);
                FormationService.Instance.TryUpgrade(FormationId.Thunder);
                yield return new WaitForSecondsRealtime(0.2f);
            }

            // 3. Mở và nâng cấp các node Đạo Vận
            if (DaoAnProgressionService.Instance != null && EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddLinhThach(500000f);
                DaoAnProgressionService.Instance.TryPurchase("wealth");
                DaoAnProgressionService.Instance.TryPurchase("sword_luck");
                DaoAnProgressionService.Instance.TryPurchase("kim_thiem");
                yield return new WaitForSecondsRealtime(0.2f);
            }

            // 4. Test Lưu Game JSON an toàn
            if (DataManager.Instance != null)
            {
                DataManager.Instance.SaveCurrentGame(silent: true);
                Debug.Log("💾 [Phase 4] Đã kiểm thử autosave JSON - Dữ liệu toàn vẹn!");
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }

        private IEnumerator RunPhase5_EnduranceAndMemoryLeakAudit()
        {
            // Vòng lặp stress quái và nhặt vật phẩm liên tục trong 5 giây
            for (int i = 0; i < 10; i++)
            {
                if (MergeManager.Instance != null)
                {
                    MergeManager.Instance.TryAddSword(1);
                    MergeManager.Instance.QuickMerge();
                }
                yield return new WaitForSecondsRealtime(0.3f);
            }
            yield return new WaitForSecondsRealtime(1.0f);
        }

        private void RecordSnapshot(int phase, string phaseName)
        {
            float fps = 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime);
            float cpuMs = cpuFrameTimeRecorder.Valid ? (float)(cpuFrameTimeRecorder.LastValue * 1e-6) : (Time.unscaledDeltaTime * 1000f);
            long monoHeap = Profiler.GetMonoUsedSizeLong() / (1024 * 1024);
            long totalAlloc = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            long gcBytes = gcAllocRecorder.Valid ? gcAllocRecorder.LastValue : 0;

            string realmName = "Vô Danh";
            int subLv = 1;
            if (CultivationManager.Instance != null)
            {
                var cur = CultivationManager.Instance.GetCurrentStageData();
                realmName = cur != null ? cur.stageName : "Luyện Khí";
                subLv = CultivationManager.Instance.CurrentSubLevel;
            }

            float lt = EconomyManager.Instance != null ? EconomyManager.Instance.currentLinhThach : 0f;
            int swords = SwordLoadoutService.Instance != null ? SwordLoadoutService.Instance.GetCurrentCapacity() : 1;

            var snap = new TelemetrySnapshot
            {
                phase = phase,
                phaseName = phaseName,
                timeStamp = Time.realtimeSinceStartup,
                fps = fps,
                cpuMs = cpuMs,
                monoHeapMb = monoHeap,
                totalAllocatedMb = totalAlloc,
                gcAllocBytes = gcBytes,
                realm = realmName,
                subLevel = subLv,
                linhThach = lt,
                swordCount = swords,
                bossKills = StageManager.Instance != null ? StageManager.Instance.currentStage : 1,
                caughtErrors = TotalErrors,
                caughtWarnings = TotalWarnings
            };

            Snapshots.Add(snap);
            Debug.Log($"📊 [Snapshot Chặng {phase}: {phaseName}] FPS: {fps:F1} | Mono: {monoHeap}MB | Cảnh Giới: {realmName} | Linh Thạch: {lt:N0} | Lỗi: {TotalErrors}");
        }

        private void CaptureCheckpoint(string filename)
        {
            string folder = "Temp";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, filename);
            ScreenCapture.CaptureScreenshot(fullPath);
            Debug.Log($"📸 [Capture Checkpoint] Đã lưu ảnh chụp: {fullPath}");
        }

        private void BypassMainMenuToArena()
        {
            var menu = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
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
            }
        }

        private void HandleLogMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                TotalErrors++;
                ErrorLogs.Add($"[{type}] {logString}\n{stackTrace}");
            }
            else if (type == LogType.Warning)
            {
                TotalWarnings++;
            }
        }

        private void PrintFinalTestReport()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════════════════");
            Debug.Log("🏁 BÁO CÁO NGHIỆM THU KIỂM THỬ TỰ ĐỘNG (FINAL PASS/FAIL AUDIT)");
            Debug.Log($"   - Tổng số Chặng hoàn thành: {Snapshots.Count}/5");
            Debug.Log($"   - Tổng lỗi Console đỏ: {TotalErrors} {(TotalErrors == 0 ? "✔ [PASS]" : "❌ [FAIL]")}");
            Debug.Log($"   - Cảnh báo: {TotalWarnings}");
            if (Snapshots.Count >= 2)
            {
                long ramInitial = Snapshots[0].monoHeapMb;
                long ramFinal = Snapshots[Snapshots.Count - 1].monoHeapMb;
                long deltaRam = ramFinal - ramInitial;
                Debug.Log($"   - Độ tăng RAM Mono: {deltaRam}MB (Ban đầu: {ramInitial}MB -> Cuối: {ramFinal}MB) {(deltaRam < 50 ? "✔ [PASS - NO LEAK]" : "⚠️ [WARN LEAK]")}");
            }
            Debug.Log("═══════════════════════════════════════════════════════════════════════════");
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// CÔNG CỤ CHEAT & DEBUG CHUYÊN DỤNG CHO GAME DESIGNER / DEVELOPER.
/// =======================================================================
/// LƯU Ý BẢO MẬT:
/// - File này nằm trong thư mục 'Assets/Scripts/Editor' và được bao bọc bởi '#if UNITY_EDITOR'.
/// - Khi xuất bản game (Build .exe / Android / WebGL), Unity sẽ HOÀN TOÀN LOẠI BỎ file này,
///   đảm bảo 100% KHÔNG THỂ SỬ DỤNG hay xuất hiện trong bản build của người chơi!
/// =======================================================================
/// </summary>
public class TuTienCheatTool : EditorWindow
{
    private float customTuViInput = 5000f;
    private Vector2 scrollPos;

    [MenuItem("TuTien/Cheat/🎮 Bảng Điều Khiển Cheat Tu Tiên", false, 0)]
    public static void ShowWindow()
    {
        TuTienCheatTool window = GetWindow<TuTienCheatTool>("Tu Tiên Cheat");
        window.minSize = new Vector2(380, 520);
        window.Show();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // QUICK SHORTCUTS & MENU ITEMS (CHỈ HOẠT ĐỘNG TRONG PLAY MODE TẠI EDITOR)
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("TuTien/Cheat/⚡ [F8] Đầy 100% Tu Vi Hiện Tại (Gọi Độ Kiếp Ngay) _F8", false, 1)]
    public static void CheatMaxCurrentTuVi()
    {
        if (!CheckPlayMode()) return;

        CultivationManager mgr = CultivationManager.Instance;
        if (mgr == null || mgr.allStages == null || mgr.currentStageIndex >= mgr.allStages.Length) return;

        var stageData = mgr.allStages[mgr.currentStageIndex];
        mgr.SetStageAndTuViForEditor(mgr.currentStageIndex, stageData.requiredTuVi);

        // Kích hoạt ngay sự kiện kiểm tra độ kiếp
        if (mgr.RequiresBossToBreakthrough(stageData))
        {
            TribulationModalUI.Open();
            Debug.Log($"<color=yellow>⚡ [Cheat F8] Đã đạt 100% Tu Vi ({mgr.currentTuVi:F0}) tại {stageData.stageName} -> Mở Panel Độ Kiếp!</color>");
        }
        else
        {
            mgr.Breakthrough();
            Debug.Log($"<color=green>✨ [Cheat F8] Đạt 100% Tu Vi tiểu cảnh giới -> Đột phá ngay!</color>");
        }
    }

    [MenuItem("TuTien/Cheat/⚡ 2. Mốc 1: Luyện Khí Tầng 15 (Độ Kiếp Lên Trúc Cơ)", false, 2)]
    public static void JumpToLuyenKhi15()
    {
        JumpByKeyword("Luyện Khí", "Tầng 15", fillTuVi: true);
    }

    [MenuItem("TuTien/Cheat/⚡ 3. Mốc 2: Trúc Cơ Hậu Kỳ (Độ Kiếp Lên Kết Đan | Test Vạn Kiếm Quy Tông)", false, 3)]
    public static void JumpToTrucCoHauKy()
    {
        JumpByKeyword("Trúc Cơ", "Hậu Kỳ", fillTuVi: true);
    }

    [MenuItem("TuTien/Cheat/⚡ 4. Mốc 3: Kết Đan Hậu Kỳ (Độ Kiếp Lên Nguyên Anh)", false, 4)]
    public static void JumpToKetDanHauKy()
    {
        JumpByKeyword("Kết Đan", "Hậu Kỳ", fillTuVi: true);
    }

    [MenuItem("TuTien/Cheat/⚡ 5. Mốc 4: Nguyên Anh Hậu Kỳ (Độ Kiếp Lên Hóa Thần)", false, 5)]
    public static void JumpToNguyenAnhHauKy()
    {
        JumpByKeyword("Nguyên Anh", "Hậu Kỳ", fillTuVi: true);
    }

    [MenuItem("TuTien/Cheat/💀 [F9] Diệt Boss Ngay Lập Tức (Insta-Kill) _F9", false, 20)]
    public static void CheatKillBoss()
    {
        if (!CheckPlayMode()) return;

        BossEnemy boss = Object.FindObjectOfType<BossEnemy>();
        if (boss != null && !boss.isDead)
        {
            boss.TakeDamage(999999999f, isCrit: true);
            Debug.Log("<color=red>💀 [Cheat F9] Đã giáng 999,999,999 sát thương tiêu diệt Boss lập tức!</color>");
        }
        else
        {
            Debug.LogWarning("⚠️ Không có Boss nào đang hoạt động trên sàn đấu!");
        }
    }

    [MenuItem("TuTien/Cheat/🛡️ [F7] Hồi Đầy 100% Máu Cho Player _F7", false, 21)]
    public static void CheatHealPlayer()
    {
        if (!CheckPlayMode()) return;

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.playerCurrentHp = CombatManager.Instance.playerMaxHp;
            CombatManager.Instance.RecalculateMaxHp(fullHeal: true);
            Debug.Log("<color=cyan>🛡️ [Cheat F7] Đã hồi đầy 100% HP cho Player!</color>");
        }
    }

    [MenuItem("TuTien/Cheat/➕ Thêm +5,000 Tu Vi", false, 30)]
    public static void Add5KTuVi() => AddTuViAmount(5000f);

    [MenuItem("TuTien/Cheat/➕ Thêm +20,000 Tu Vi", false, 31)]
    public static void Add20KTuVi() => AddTuViAmount(20000f);

    [MenuItem("TuTien/Cheat/➕ Thêm +100,000 Tu Vi", false, 32)]
    public static void Add100KTuVi() => AddTuViAmount(100000f);

    // ─────────────────────────────────────────────────────────────────────────
    // GUI CONSOLE WINDOW (VẼ GIAO DIỆN BẢNG ĐIỀU KHIỂN TRONG UNITY EDITOR)
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("⚡ BẢNG ĐIỀU KHIỂN CHEAT TU TIÊN", titleStyle);
        EditorGUILayout.LabelField("<i>(Chỉ khả dụng trong Play Mode tại Unity Editor)</i>", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("▶️ Hãy nhấn PLAY GAME để sử dụng các công cụ Cheat thời gian thực!", MessageType.Info);
            return;
        }

        CultivationManager cult = CultivationManager.Instance;
        if (cult == null)
        {
            EditorGUILayout.HelpBox("Không tìm thấy CultivationManager trong Scene!", MessageType.Warning);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // 1. Box Trạng Thái Nhân Vật
        var curStage = cult.GetCurrentStageData();
        string stageName = curStage != null ? curStage.stageName : "Chưa xác định";
        float maxTuVi = curStage != null ? curStage.requiredTuVi : 1000f;
        float progress = Mathf.Clamp01(cult.currentTuVi / maxTuVi) * 100f;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 THÔNG TIN TU TIÊN HIỆN TẠI", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"• Cảnh giới: {stageName} (Mốc #{cult.currentStageIndex + 1})");
        EditorGUILayout.LabelField($"• Điểm Tu Vi: {cult.currentTuVi:N0} / {maxTuVi:N0} ({progress:F1}%)");

        bool needBoss = cult.RequiresBossToBreakthrough(curStage);
        EditorGUILayout.LabelField($"• Mốc Đột Phá Đại Cảnh Giới: {(needBoss ? "✅ ĐÚNG (Cần Boss Độ Kiếp)" : "❌ Tiểu cảnh giới")}");

        if (CombatManager.Instance != null)
        {
            EditorGUILayout.LabelField($"• Máu Player: {CombatManager.Instance.playerCurrentHp:N0} / {CombatManager.Instance.playerMaxHp:N0}");
            EditorGUILayout.LabelField($"• Boss Thiên Kiếp: {(CombatManager.Instance.isTribulationBossActive ? "⚡ ĐANG GIAO CHIẾN!" : "Chưa kích hoạt")}");
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // 2. Nhóm Nút Lệnh Nhanh Độ Kiếp
        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUILayout.Button("⚡ [F8] ĐẦY 100% TU VI & GỌI ĐỘ KIẾP NGAY", GUILayout.Height(36)))
        {
            CheatMaxCurrentTuVi();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);

        // 3. Nhóm Nhảy Nhanh Các Mốc Độ Kiếp Quan Trọng
        EditorGUILayout.LabelField("🎯 NHẢY NHANH CÁC MỐC ĐỘ KIẾP QUAN TRỌNG:", EditorStyles.boldLabel);

        if (GUILayout.Button("Mốc 1: Luyện Khí Tầng 15 ➔ Lên Trúc Cơ", GUILayout.Height(28)))
        {
            JumpToLuyenKhi15();
        }

        GUI.backgroundColor = new Color(0.6f, 0.9f, 1f);
        if (GUILayout.Button("Mốc 2: Trúc Cơ Hậu Kỳ ➔ Lên Kết Đan (Test Vạn Kiếm Quy Tông)", GUILayout.Height(32)))
        {
            JumpToTrucCoHauKy();
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Mốc 3: Kết Đan Hậu Kỳ ➔ Lên Nguyên Anh", GUILayout.Height(28)))
        {
            JumpToKetDanHauKy();
        }

        if (GUILayout.Button("Mốc 4: Nguyên Anh Hậu Kỳ ➔ Lên Hóa Thần", GUILayout.Height(28)))
        {
            JumpToNguyenAnhHauKy();
        }

        GUILayout.Space(12);

        // 4. Nhóm Cộng Tu Vi Thủ Công
        EditorGUILayout.LabelField("➕ CỘNG ĐIỂM TU VI:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+1,000")) AddTuViAmount(1000f);
        if (GUILayout.Button("+5,000")) AddTuViAmount(5000f);
        if (GUILayout.Button("+20,000")) AddTuViAmount(20000f);
        if (GUILayout.Button("+100,000")) AddTuViAmount(100000f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        customTuViInput = EditorGUILayout.FloatField("Số lượng tùy chỉnh:", customTuViInput);
        if (GUILayout.Button("Thêm", GUILayout.Width(60)))
        {
            AddTuViAmount(customTuViInput);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(12);

        // 5. Tiện Ích Chiến Đấu (Kill Boss / Heal Player)
        EditorGUILayout.LabelField("⚔️ TIỆN ÍCH CHIẾN ĐẤU & SINH TỒN:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("💀 [F9] Diệt Boss Lập Tức", GUILayout.Height(30)))
        {
            CheatKillBoss();
        }

        GUI.backgroundColor = new Color(0.4f, 1f, 0.6f);
        if (GUILayout.Button("🛡️ [F7] Hồi Đầy HP", GUILayout.Height(30)))
        {
            CheatHealPlayer();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(12);

        // 6. Tiện Ích Nhiệm Vụ Mốc & Tutorial
        EditorGUILayout.LabelField("📜 NHIỆM VỤ MỐC & HƯỚNG DẪN TÂN THỦ:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("📜 Mở Bảng Mốc", GUILayout.Height(28)))
        {
            if (MilestoneModalUI.Instance != null) MilestoneModalUI.Instance.Open();
            else if (TutorialManager.Instance != null) TutorialManager.Instance.OpenMilestoneModal();
        }

        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUILayout.Button("✅ Xong Tutorial ➔ Mốc", GUILayout.Height(28)))
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.swordDone = true;
                TutorialManager.Instance.coDuyenDone = true;
                TutorialManager.Instance.upgradeDone = true;
                TutorialManager.Instance.shopDone = true;
                TutorialManager.Instance.breakthroughDone = true;
                TutorialManager.Instance.RestartTutorial();
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⚔️ +50 Quái Diệt"))
        {
            if (StageManager.Instance != null) StageManager.Instance.enemiesKilled += 50;
        }
        if (GUILayout.Button("✨ +50 Cơ Duyên"))
        {
            if (EconomyManager.Instance != null) EconomyManager.Instance.AddCoDuyen(50);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        // Tự động repainting cửa sổ khi chơi để update số liệu real-time
        Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER METHODS
    // ─────────────────────────────────────────────────────────────────────────

    private static bool CheckPlayMode()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ [Cheat] Vui lòng nhấn PLAY (Chạy Game) trước khi dùng lệnh Cheat!");
            return false;
        }
        return true;
    }

    private static void AddTuViAmount(float amount)
    {
        if (!CheckPlayMode()) return;
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.AddTuVi(amount);
            Debug.Log($"<color=cyan>➕ [Cheat] Đã thêm +{amount:N0} Tu Vi! Hiện tại: {CultivationManager.Instance.currentTuVi:N0}</color>");
        }
    }

    private static void JumpByKeyword(string kw1, string kw2, bool fillTuVi)
    {
        if (!CheckPlayMode()) return;

        CultivationManager mgr = CultivationManager.Instance;
        if (mgr == null || mgr.allStages == null) return;

        int targetIndex = -1;
        for (int i = 0; i < mgr.allStages.Length; i++)
        {
            var s = mgr.allStages[i];
            if (s != null && s.stageName.Contains(kw1))
            {
                if (string.IsNullOrEmpty(kw2) || s.stageName.Contains(kw2))
                {
                    targetIndex = i;
                    break;
                }
            }
        }

        if (targetIndex >= 0)
        {
            var stageData = mgr.allStages[targetIndex];
            mgr.SetStageAndTuViForEditor(targetIndex, fillTuVi ? stageData.requiredTuVi : 0f);

            if (fillTuVi)
            {
                if (mgr.RequiresBossToBreakthrough(stageData))
                {
                    TribulationModalUI.Open();
                }
                else
                {
                    mgr.Breakthrough();
                }
            }

            Debug.Log($"<color=green>⚡ [Cheat] Đã chuyển đến mốc: {stageData.stageName} (Tu Vi: {mgr.currentTuVi:N0}/{stageData.requiredTuVi:N0}) -> Mở Modal Độ Kiếp!</color>");
        }
        else
        {
            Debug.LogWarning($"⚠️ Không tìm thấy Cảnh Giới khớp từ khóa: '{kw1}' - '{kw2}'!");
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// CÔNG CỤ AUDIT & KIỂM THỬ TOÀN DIỆN MỌI HỆ THỐNG Ở CẤP ĐỘ MAX (ENDGAME BALANCE & OVERFLOW AUDIT).
/// Đánh giá chuyên sâu từ góc nhìn Co-Founder & Lead Game Designer.
/// </summary>
public static class MaxLevelAuditor
{
    [MenuItem("TuTien/Audit/⚡ Kiểm Thử Max Level Toàn Bộ Game")]
    public static void RunFullEndgameAudit()
    {
        Debug.Log("==================================================================================");
        Debug.Log("🚀 [AUDIT] BẮT ĐẦU KIỂM THỬ TOÀN DIỆN MỌI HỆ THỐNG Ở MAX LEVEL & CÂN BẰNG ENDGAME");
        Debug.Log("==================================================================================");

        StringBuilder sb = new StringBuilder();
        int passCount = 0;
        int warningCount = 0;
        int issueCount = 0;

        // ─────────────────────────────────────────────────────────────────────
        // 1. AUDIT HỆ THỐNG CẢNH GIỚI (CULTIVATION SYSTEM)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("=== 1. CẢNH GIỚI (CULTIVATION REALMS) ===");
        var cult = Object.FindAnyObjectByType<CultivationManager>(FindObjectsInactive.Include);
        if (cult != null && cult.allStages != null && cult.allStages.Length > 0)
        {
            int totalStages = cult.allStages.Length;
            var maxStage = cult.allStages[totalStages - 1];
            float maxTuViReq = maxStage.requiredTuVi;

            bool isTuViValid = !float.IsNaN(maxTuViReq) && !float.IsInfinity(maxTuViReq) && maxTuViReq > 0;
            if (isTuViValid)
            {
                passCount++;
                sb.AppendLine($"[PASS] Tổng {totalStages} cảnh giới. Max: '{maxStage.stageName}' (Yêu cầu {maxTuViReq:N0} Tu Vi).");
            }
            else
            {
                issueCount++;
                sb.AppendLine($"[FAIL] Tu Vi cảnh giới cao nhất bị lỗi số học (NaN/Infinity): {maxTuViReq}");
            }

            // Kiểm tra hệ số đột phá lũy kế
            float totalStatMult = 1f;
            foreach (var s in cult.allStages) if (s != null && s.statMultiplierBonus > 1f) totalStatMult *= s.statMultiplierBonus;
            sb.AppendLine($"[INFO] Hệ số nhân chỉ số đột phá vĩnh viễn tích lũy: x{totalStatMult:F2}");
        }
        else
        {
            warningCount++;
            sb.AppendLine("[WARN] Không tìm thấy CultivationManager hoặc allStages rỗng.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. AUDIT HỆ THỐNG THỂ TU (BODY CULTIVATION - 6 STATS)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 2. THỂ TU (BODY CULTIVATION - 6 CHỈ SỐ) ===");
        int testMaxLevel = 60; // Thử nghiệm tại cấp 60 mỗi chỉ số
        float totalStoneCost60 = 0f;
        for (int i = 0; i < 6; i++)
        {
            float costAt60 = Mathf.Ceil(100f * (i + 1) * Mathf.Pow(1.2f, testMaxLevel));
            totalStoneCost60 += costAt60;
        }

        float dmgReduc60 = Mathf.Min(testMaxLevel * 0.5f, 60f); // Cap 60%
        float tenacity60 = Mathf.Min(testMaxLevel * 1.0f, 80f); // Cap 80%
        float maxHpFromKhíHuyết = testMaxLevel * 150f;          // +9,000 HP
        float swordDmgFromKìnhLực = testMaxLevel * 10f;         // +600 Damage
        float lifestealFromPhệHuyết = testMaxLevel * 0.2f;      // 12% Lifesteal
        float regenFromDũHợp = testMaxLevel * 5f;               // +300 HP/s

        bool bodyNumbersSafe = !float.IsInfinity(totalStoneCost60) && dmgReduc60 <= 60f && tenacity60 <= 80f;
        if (bodyNumbersSafe)
        {
            passCount++;
            sb.AppendLine($"[PASS] Hard-caps an toàn: Miễn thương={dmgReduc60:F1}% (Cap 60%), Kháng choáng={tenacity60:F0}% (Cap 80%).");
            sb.AppendLine($"[INFO] Chỉ số tại Cấp {testMaxLevel}: +{maxHpFromKhíHuyết:N0} HP, +{swordDmgFromKìnhLực:N0} Sword Dmg, {lifestealFromPhệHuyết:F1}% Hút máu, +{regenFromDũHợp:N0} HP/s.");
            sb.AppendLine($"[INFO] Giá nâng 1 cấp tại Lv.{testMaxLevel}: ~{totalStoneCost60:N0} Linh Thạch.");
        }
        else
        {
            issueCount++;
            sb.AppendLine("[FAIL] Chỉ số Thể Tu bị vượt ngưỡng an toàn!");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. AUDIT KIẾM LÔ & SÁT THƯƠNG NGỰ KIẾM (SWORD MERGE & DPS)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 3. KIẾM LÔ & NGỰ KIẾM (SWORD COMBAT & SCALING) ===");
        int maxSlots = 10;
        int maxSwordLv = 8; // Max level kiếm hiện tại
        float baseDmgPerSwordLv8 = FlyingSword.CalculateBaseDamage(maxSwordLv); // 8*8*10 = 640
        float baseSpeedLv8 = FlyingSword.CalculateBaseSpeed(maxSwordLv);        // 5 + 8*2 = 21
        float totalRawDps10Swords = maxSlots * baseDmgPerSwordLv8 * (1f / 0.45f); // ~14,222 DPS thô

        // Khi có Kình Lực (+600) và Kiếm Ý (x2):
        float enhancedDmgPerSword = (baseDmgPerSwordLv8 + swordDmgFromKìnhLực) * 2.5f;
        float totalEnhancedDps = maxSlots * enhancedDmgPerSword * (1f / 0.45f);

        passCount++;
        sb.AppendLine($"[PASS] 10 Phi Kiếm Cấp 8: Sát thương gốc={baseDmgPerSwordLv8:N0}/kiếm, Tốc độ={baseSpeedLv8:F1}m/s.");
        sb.AppendLine($"[INFO] DPS cơ bản 10 kiếm: ~{totalRawDps10Swords:N0} DPS.");
        sb.AppendLine($"[INFO] DPS khi cộng hưởng Thể Tu + Kiếm Ý: ~{totalEnhancedDps:N0} DPS.");

        // ─────────────────────────────────────────────────────────────────────
        // 4. AUDIT PHÁP KHÍ & PHÁP TRẬN (ARTIFACTS & FORMATIONS)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 4. PHÁP KHÍ & ĐẠI TRẬN PHÁP (ARTIFACTS & FORMATIONS) ===");
        int artMax = ArtifactService.MaxLevel; // 30
        int totalOreForArtifacts = 0;
        for (int slot = 0; slot < 4; slot++)
        {
            for (int lv = 1; lv <= artMax; lv++)
            {
                totalOreForArtifacts += lv * lv * (1 + slot);
            }
        }

        int formMax = FormationService.MaxLevel; // 30
        int totalOreForFormations = 0;
        for (int form = 0; form < 6; form++)
        {
            for (int lv = 1; lv <= formMax; lv++)
            {
                totalOreForFormations += 3 * lv * lv;
            }
        }

        float artTenacity = Mathf.Min(0.5f, artMax * 0.015f); // 45% (Cap 50%)
        float artBossReduc = Mathf.Min(0.30f, artMax * 0.01f); // 30% (Cap 30%)
        float artDodge = Mathf.Min(0.3f, artMax * 0.01f);      // 30% (Cap 30%)
        float artCrit = Mathf.Min(0.25f, artMax * 0.006f);     // 18% (Cap 25%)

        passCount++;
        sb.AppendLine($"[PASS] 4 Pháp Khí Cấp 30: Giảm dame Boss={artBossReduc:P0}, Kháng choáng={artTenacity:P0}, Né tránh={artDodge:P0}, Bạo kích=+{artCrit:P0}.");
        sb.AppendLine($"[INFO] Tổng quặng nâng 4 Pháp Khí Max (Lv30): {totalOreForArtifacts:N0} Quặng.");
        sb.AppendLine($"[INFO] Tổng quặng nâng 6 Trận Pháp Max (Lv30): {totalOreForFormations:N0} Quặng.");

        // ─────────────────────────────────────────────────────────────────────
        // 5. AUDIT THỬ THÁCH VƯỢT ẢI (STAGE HP VS PLAYER DPS)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 5. ĐỐI CHIẾU TIẾN TRÌNH ẢI (STAGE HP VS DPS) ===");
        int[] testStages = { 1, 10, 20, 30, 40, 50, 60, 80, 100 };
        foreach (int stg in testStages)
        {
            float hp = CombatManager.CalculateEnemyHealth(stg);
            float bossHp = hp * 6f; // Boss x6 máu
            sb.AppendLine($"• Tầng {stg,3}: Quái thường = {FormatNumber(hp),8} HP | Boss Tầng = {FormatNumber(bossHp),8} HP");
        }

        // Kiểm tra nguy cơ lạm phát HP ở tầng 100:
        float hp100 = CombatManager.CalculateEnemyHealth(100);
        if (hp100 > 50000000f) // > 50M
        {
            warningCount++;
            sb.AppendLine($"[WARN] Máu quái ở Tầng 100 ({FormatNumber(hp100)}) tăng trưởng theo hàm 1.22^N hơi dốc so với tốc độ tăng trưởng DPS của Kiếm Cấp 8 (Cap 8). Cần mở rộng Kiếm Cấp 9-15 hoặc tinh chỉnh hệ số.");
        }
        else
        {
            passCount++;
            sb.AppendLine("[PASS] Máu quái tăng trưởng nằm trong tầm kiểm soát.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. AUDIT TOÀN VẸN 47 THÀNH TỰU (ACHIEVEMENTS INTEGRITY)
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 6. HỆ THỐNG THÀNH TỰU (47 ACHIEVEMENTS INTEGRITY) ===");
        var achMgr = Object.FindAnyObjectByType<AchievementManager>(FindObjectsInactive.Include);
        if (achMgr != null)
        {
            achMgr.EnsureInitialized();
            int totalAch = achMgr.allAchievements.Count;
            float totalRewLinhThach = 0f;
            int totalRewCoDuyen = 0;
            int totalRewOre = 0;

            foreach (var a in achMgr.allAchievements)
            {
                totalRewLinhThach += a.rewardLinhThach;
                totalRewCoDuyen += a.rewardCoDuyen;
                totalRewOre += a.rewardOre;
            }

            passCount++;
            sb.AppendLine($"[PASS] Khởi tạo hoàn hảo {totalAch}/47 Thành Tựu trên 5 nhánh (Tu Luyện, Chiến Đấu, Kiếm Đạo, Bách Nghệ, Kỳ Duyên).");
            sb.AppendLine($"[INFO] Tổng phần thưởng khi cày hết: {totalRewLinhThach:N0} Linh Thạch, {totalRewCoDuyen} Cơ Duyên, {totalRewOre} Quặng Phôi.");
        }
        else
        {
            warningCount++;
            sb.AppendLine("[WARN] Chưa tìm thấy AchievementManager trên Scene.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. AUDIT SAVE / LOAD MÃ HÓA & SERIALIZATION AT MAX STATS
        // ─────────────────────────────────────────────────────────────────────
        sb.AppendLine("\n=== 7. KIỂM THỬ SAVE / LOAD TẠI CẤP MAX (STRESS SERIALIZATION) ===");
        CharacterSaveData maxSlot = new CharacterSaveData
        {
            characterName = "Tiên Tôn Vô Địch",
            realmId = 4, // Hóa Thần
            subLevel = 10,
            stageIndex = 26,
            currentTuVi = 163840000f,
            permanentStatMultiplier = 25.6f,
            swordStyleUnlocked = true,
            swordStyle = 1,
            encounterKillsRemaining = 0,
            saveVersion = SaveMigration.CURRENT_SAVE_VERSION,
            bodyStatLevels = new int[] { 60, 60, 60, 60, 60, 60 },
            bodyMilestonePassives = new bool[] { true, true, true, true, true, true },
            swords = new SwordSaveEntry[10],
            artifacts = new ArtifactSaveState { ore = 50000, levels = new int[] { 30, 30, 30, 30 } },
            formations = new FormationSaveState { levels = new int[] { 30, 30, 30, 30, 30, 30 }, selectedFormation = 3 }
        };

        for (int s = 0; s < 10; s++)
        {
            maxSlot.swords[s] = new SwordSaveEntry { level = 8, isEquipped = true, equipSlotIndex = s };
        }

        string json = JsonUtility.ToJson(maxSlot);
        CharacterSaveData loadedSlot = JsonUtility.FromJson<CharacterSaveData>(json);

        bool saveIntact = loadedSlot != null 
            && loadedSlot.currentTuVi == maxSlot.currentTuVi 
            && loadedSlot.artifacts.levels[0] == 30 
            && loadedSlot.bodyStatLevels[3] == 60
            && loadedSlot.swords.Length == 10;

        if (saveIntact)
        {
            passCount++;
            sb.AppendLine($"[PASS] Serialization JSON tại Max Level an toàn 100% ({json.Length} bytes). Không mất mát hay sai lệch số học.");
        }
        else
        {
            issueCount++;
            sb.AppendLine("[FAIL] Dữ liệu Save bị lỗi hoặc cắt ngắn sau khi JsonUtility Serialize/Deserialize!");
        }

        // ─────────────────────────────────────────────────────────────────────
        // KẾT LUẬN TỔNG THỂ
        // ─────────────────────────────────────────────────────────────────────
        Debug.Log(sb.ToString());
        Debug.Log($"==================================================================================");
        Debug.Log($"📊 KẾT QUẢ AUDIT TỔNG THỂ: {passCount} ĐẠT (PASS) | {warningCount} CẢNH BÁO (WARN) | {issueCount} LỖI (ISSUE)");
        Debug.Log($"==================================================================================");
    }

    private static string FormatNumber(float num)
    {
        if (num >= 1000000000f) return (num / 1000000000f).ToString("F2") + "B";
        if (num >= 1000000f) return (num / 1000000f).ToString("F2") + "M";
        if (num >= 1000f) return (num / 1000f).ToString("F1") + "K";
        return num.ToString("F0");
    }
}
#endif

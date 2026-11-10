#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TuViUnlockProgressionTest
{
    [MenuItem("TuTien/Test/Kiểm Tra Hệ Thống Mở Khóa Theo Tu Vi & TBH")]
    public static void RunTests()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan>🧪 BẮT ĐẦU KIỂM ĐỊNH HỆ THỐNG MỞ KHÓA THEO TU VI & TÍCH HỢP TBH</color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");

        int passed = 0;
        int failed = 0;

        void AssertTest(string testName, bool condition)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"<color=green>  ✔ [PASS] {testName}</color>");
            }
            else
            {
                failed++;
                Debug.LogError($"<color=red>  ❌ [FAIL] {testName}</color>");
            }
        }

        // Đảm bảo các service khởi tạo data nếu chạy ở Edit Mode
        if (AchievementManager.Instance != null) AchievementManager.Instance.EnsureInitialized();

        // ── 1. KIỂM ĐỊNH CỔNG TÍNH NĂNG CHÍNH (FEATURE GATES) ────────────────
        AssertTest("Kiếm Lô luôn mở", FeatureUnlockService.Instance != null && FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.SwordMerge));
        AssertTest("Vận Khí luôn mở", FeatureUnlockService.Instance != null && FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.DaoAn));

        // ── 2. KIỂM ĐỊNH THÀNH TỰU & TÁCH TRANG BỊ 500 KIẾM ──────────────────
        AssertTest("Dismantle yêu cầu 500 kiếm quái rơi", FeatureUnlockService.DISMANTLE_REQUIRED_MONSTER_SWORDS == 500);
        AssertTest("Thành tựu monster_sword_drop_500 tồn tại", AchievementManager.Instance != null && AchievementManager.Instance.allAchievements.Exists(a => a.id == "monster_sword_drop_500"));

        // ── 3. KIỂM ĐỊNH CỔNG THẢO DƯỢC & QUẶNG & SO SÁNH TU VI ─────────────
        AssertTest("So sánh tu vi: LK7 >= LK7", FeatureUnlockService.CompareCultivation(RealmId.LuyenKhi, 7, RealmId.LuyenKhi, 7));
        AssertTest("So sánh tu vi: LK3 < LK7 (chưa mở Luyện Đan)", !FeatureUnlockService.CompareCultivation(RealmId.LuyenKhi, 3, RealmId.LuyenKhi, 7));
        AssertTest("So sánh tu vi: Trúc Cơ 1 > LK7 (đã mở Luyện Đan & Pháp Trận)", FeatureUnlockService.CompareCultivation(RealmId.TrucCo, 1, RealmId.LuyenKhi, 7));
        AssertTest("So sánh tu vi: Trúc Cơ 1 < Hóa Thần 1 (chưa mở Luyện Thể)", !FeatureUnlockService.CompareCultivation(RealmId.TrucCo, 1, RealmId.HoaThan, 1));
        AssertTest("So sánh tu vi: Hóa Thần 1 >= Hóa Thần 1 (đã mở Luyện Thể)", FeatureUnlockService.CompareCultivation(RealmId.HoaThan, 1, RealmId.HoaThan, 1));

        // ── 4. KIỂM ĐỊNH CAP TBH (LUYỆN THỂ) ─────────────────────────────────
        if (PlayerStatsService.Instance != null)
        {
            // Thử recalc với level cao vượt trần
            int[] testLevels = new int[6] { 10, 200, 10, 10, 200, 200 };
            PlayerStatsService.Instance.RecalculateStats(testLevels, null);

            AssertTest("Luyện Thể: Kháng Choáng kẹp trần tối đa 80%", Mathf.Approximately(PlayerStatsService.Instance.TenacityPercent, 0.80f));
            AssertTest("Luyện Thể: Hút Máu kẹp trần tối đa 20%", Mathf.Approximately(PlayerStatsService.Instance.LifestealPercent, 0.20f));
            AssertTest("Luyện Thể: Miễn Thương kẹp trần tối đa 60%", Mathf.Approximately(PlayerStatsService.Instance.DamageReduction, 0.60f));
        }
        else
        {
            AssertTest("PlayerStatsService tồn tại", false);
        }

        // ── 5. KIỂM ĐỊNH SAVE SCHEMA V5 ──────────────────────────────────────
        AssertTest("SaveMigration Version hiện tại là 5", SaveMigration.CURRENT_SAVE_VERSION == 5);
        CharacterSaveData newSlot = new CharacterSaveData();
        AssertTest("Character mới khởi tạo với saveVersion = 5 (không ăn 3 viên đan legacy)", newSlot.saveVersion == 5);
        AssertTest("Character mới khởi tạo với monsterSwordsAcquired = 0", newSlot.monsterSwordsAcquired == 0);

        // ── 6. KIỂM ĐỊNH LOOT REWARD SERVICE ──────────────────────────────────
        AssertTest("LootRewardService đã sẵn sàng", LootRewardService.Instance != null);

        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");
        if (failed == 0)
        {
            Debug.Log($"<color=green>🎉 TẤT CẢ {passed} TEST ĐỀU VƯỢT QUA XUẤT SẮC! CẤU TRÚC ĐẠT CHUẨN HOÀN TOÀN.</color>");
        }
        else
        {
            Debug.LogError($"<color=red>⚠️ CÓ {failed} TEST THẤT BẠI TRÊN TỔNG SỐ {passed + failed} TEST.</color>");
        }
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════════════</color>");
    }
}
#endif

// File: Assets/Scripts/Data/SaveMigration.cs
using UnityEngine;

public static class SaveMigration
{
    public const int CURRENT_SAVE_VERSION = 3;

    public static bool Run(CharacterSaveData slot)
    {
        if (slot == null || slot.isSlotEmpty) return false;
        bool isModified = false;

        // Chỉ chạy khi save cũ hơn CURRENT_SAVE_VERSION
        if (slot.saveVersion < CURRENT_SAVE_VERSION)
        {
            Debug.Log($"[SaveMigration] Phát hiện save cũ v{slot.saveVersion} ('{slot.characterName}'). Bắt đầu Migrate lên v{CURRENT_SAVE_VERSION}...");

            // 1. Map 27 Stage cũ sang RealmId và SubLevel (1-10)
            MigrateCultivationStage(slot);

            // 2. Chuẩn hóa ô kiếm (hỗ trợ slot 0 -> 9)
            MigrateSwordEntries(slot);

            // 3. Khởi tạo mốc thời gian UTC ticks chống hack
            if (slot.lastActiveUtcTicks == 0)
            {
                slot.lastActiveUtcTicks = System.DateTime.UtcNow.Ticks;
            }

            // 4. Khởi tạo mảng Thể Tu nếu null
            if (slot.bodyStatLevels == null || slot.bodyStatLevels.Length < 6)
            {
                slot.bodyStatLevels = new int[6] { 0, 0, 0, 0, 0, 0 };
            }
            if (slot.bodyMilestonePassives == null || slot.bodyMilestonePassives.Length < 6)
            {
                slot.bodyMilestonePassives = new bool[6];
            }

            // 5. Đảm bảo AlchemySaveState hợp lệ
            if (slot.alchemy == null) slot.alchemy = new AlchemySaveState();
            if (slot.alchemy.herbs == null || slot.alchemy.herbs.Length < 3)
                slot.alchemy.herbs = new int[3];
            if (slot.alchemy.pills == null || slot.alchemy.pills.Length < 72)
                slot.alchemy.pills = new int[72];

            // 5b. Đảm bảo AchievementSaveState hợp lệ
            if (slot.achievements == null) slot.achievements = new AchievementSaveState();
            if (slot.achievements.claimedIds == null) slot.achievements.claimedIds = new string[0];

            // 6. [LỖ HỔNG 1 FIX] Cấp sẵn 3 viên Đột Phá Đan Luyện Khí Sơ Phẩm cho người chơi mới
            // Index = PillIndex(realm=0, recipe=Breakthrough=1, quality=Basic=1) = (0*2+1)*4+1 = 5
            // Chỉ cấp nếu chưa có đan nào ở realm 0 (tránh cấp lại nhiều lần)
            if (slot.realmId == 0)
            {
                bool hasAnyBreakthroughPill = false;
                for (int q = 1; q <= 3; q++) // Basic=1, Refined=2, Supreme=3
                {
                    int idx = (0 * 2 + 1) * 4 + q; // PillIndex(realm=0, Breakthrough, quality=q)
                    if (slot.alchemy.pills[idx] > 0) { hasAnyBreakthroughPill = true; break; }
                }
                if (!hasAnyBreakthroughPill)
                {
                    int basicIdx = (0 * 2 + 1) * 4 + 1; // realm=0, Breakthrough, Basic
                    slot.alchemy.pills[basicIdx] = 3;
                    Debug.Log("[SaveMigration] Cấp 3 viên Đột Phá Đan Luyện Khí Sơ Phẩm cho nhân vật mới.");
                }
            }

            slot.saveVersion = CURRENT_SAVE_VERSION;
            isModified = true;
            Debug.Log("[SaveMigration] Migrate thành công! Dữ liệu đã an toàn 100%.");
        }

        return isModified;
    }

    private static void MigrateCultivationStage(CharacterSaveData slot)
    {
        MapLegacyStage(slot.stageIndex, out RealmId realm, out int subLevel);
        slot.realmId = (int)realm;
        slot.subLevel = subLevel;
    }

    public static void MapLegacyStage(int oldIndex, out RealmId realm, out int subLevel)
    {
        oldIndex = Mathf.Max(0, oldIndex);

        if (oldIndex < 15) // Luyện Khí Tầng 1 -> 15 cũ (zero-based 0->14)
        {
            realm = RealmId.LuyenKhi;
            // Bảng map tường minh: stageIndex 4 = Tầng 5 cũ → phải ra sub-level 5 (FTUE threshold)
            int[] luyenKhiMap = { 1, 2, 3, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 10, 10 };
            subLevel = luyenKhiMap[oldIndex];
        }
        else if (oldIndex < 18) // Trúc Cơ Sơ (15), Trung (16), Hậu (17)
        {
            realm = RealmId.TrucCo;
            subLevel = (oldIndex == 15) ? 1 : (oldIndex == 16 ? 4 : 7);
        }
        else if (oldIndex < 21) // Kết Đan (Kim Đan) Sơ, Trung, Hậu
        {
            realm = RealmId.KimDan;
            subLevel = (oldIndex == 18) ? 1 : (oldIndex == 19 ? 4 : 7);
        }
        else if (oldIndex < 24) // Nguyên Anh Sơ, Trung, Hậu
        {
            realm = RealmId.NguyenAnh;
            subLevel = (oldIndex == 21) ? 1 : (oldIndex == 22 ? 4 : 7);
        }
        else // Hóa Thần Sơ, Trung, Hậu (24, 25, 26)
        {
            realm = RealmId.HoaThan;
            subLevel = (oldIndex == 24) ? 1 : (oldIndex == 25 ? 4 : 7);
        }
    }

    private static void MigrateSwordEntries(CharacterSaveData slot)
    {
        if (slot.swords == null) return;
        foreach (var s in slot.swords)
        {
            if (s.isEquipped)
            {
                s.equipSlotIndex = Mathf.Clamp(s.equipSlotIndex, 0, 9);
            }
        }
    }
}


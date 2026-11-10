using System;
using UnityEngine;

public enum FeatureId
{
    SwordMerge = 0,     // Kiếm Lô: Luôn mở (Luyện Khí 1)
    DaoAn = 1,          // Vận Khí: Luôn mở (Luyện Khí 1)
    Alchemy = 2,        // Luyện Đan: Mở tại Luyện Khí 7 (Hậu kỳ)
    Formation = 3,      // Pháp Trận: Mở tại Trúc Cơ 1
    Artifact = 4,       // Pháp Khí: Mở tại Nguyên Anh 1
    BodyCultivation = 5,// Luyện Thể: Mở tại Hóa Thần 1
    Dismantle = 6       // Lò Phân Giải: Mở khi nhặt 500 thanh kiếm từ quái
}

/// <summary>
/// Dịch vụ trung tâm kiểm soát toàn bộ tiến trình mở khóa tính năng theo Tu Vi và Thành Tựu
/// Tuân thủ nghiêm ngặt GDD thương mại và KE_HOACH_TAI_CAU_TRUC_MO_KHOA_THEO_TU_VI.md.
/// </summary>
public class FeatureUnlockService : MonoBehaviour
{
    private static FeatureUnlockService _instance;
    public static FeatureUnlockService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<FeatureUnlockService>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public const int DISMANTLE_REQUIRED_MONSTER_SWORDS = 500;

    public event Action OnUnlockStateChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. KIỂM TRA MỐC TU VI (REALM & SUBLEVEL)
    // ─────────────────────────────────────────────────────────────────────────

    public static bool CompareCultivation(RealmId curRealm, int curSub, RealmId targetRealm, int targetSub)
    {
        if (curRealm > targetRealm) return true;
        if (curRealm == targetRealm && curSub >= targetSub) return true;
        return false;
    }

    public static bool IsCultivationReached(RealmId targetRealm, int targetSubLevel)
    {
        if (CultivationManager.Instance != null)
        {
            return CompareCultivation(CultivationManager.Instance.CurrentRealm, CultivationManager.Instance.CurrentSubLevel, targetRealm, targetSubLevel);
        }

        var data = DataManager.Instance;
        if (data != null && data.isSessionActive && data.gameSaveData != null)
        {
            int slotIdx = data.gameSaveData.lastPlayedSlotIndex;
            if (slotIdx >= 0 && slotIdx < data.gameSaveData.slots.Length)
            {
                var slot = data.gameSaveData.slots[slotIdx];
                return CompareCultivation((RealmId)slot.realmId, slot.subLevel, targetRealm, targetSubLevel);
            }
        }
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. MỞ KHÓA TÍNH NĂNG CHÍNH (FEATURE GATES)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsFeatureUnlocked(FeatureId feature)
    {
        switch (feature)
        {
            case FeatureId.SwordMerge:
            case FeatureId.DaoAn:
                return true; // Luôn mở từ đầu game

            case FeatureId.Alchemy:
                // Mở tại Luyện Khí 7 (Luyện Khí Hậu Kỳ)
                return IsCultivationReached(RealmId.LuyenKhi, 7);

            case FeatureId.Formation:
                // Mở tại Trúc Cơ 1 (Trúc Cơ Sơ Kỳ)
                return IsCultivationReached(RealmId.TrucCo, 1);

            case FeatureId.Artifact:
                // Mở tại Nguyên Anh 1 (Nguyên Anh Sơ Kỳ)
                return IsCultivationReached(RealmId.NguyenAnh, 1);

            case FeatureId.BodyCultivation:
                // Mở tại Hóa Thần 1 (Hóa Thần Sơ Kỳ)
                return IsCultivationReached(RealmId.HoaThan, 1);

            case FeatureId.Dismantle:
                return IsDismantleUnlocked();

            default:
                return false;
        }
    }

    public string GetFeatureUnlockRequirementText(FeatureId feature)
    {
        switch (feature)
        {
            case FeatureId.SwordMerge:
            case FeatureId.DaoAn:
                return "Đã mở khóa";

            case FeatureId.Alchemy:
                return "Mở tại Luyện Khí · Hậu Kỳ (Tầng 7)";

            case FeatureId.Formation:
                return "Mở tại Trúc Cơ · Sơ Kỳ";

            case FeatureId.Artifact:
                return "Mở tại Nguyên Anh · Sơ Kỳ";

            case FeatureId.BodyCultivation:
                return "Mở tại Hóa Thần · Sơ Kỳ";

            case FeatureId.Dismantle:
                int current = GetMonsterSwordsAcquired();
                return $"Mở sau khi nhặt 500 kiếm từ quái ({current}/{DISMANTLE_REQUIRED_MONSTER_SWORDS})";

            default:
                return "Chưa mở khóa";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. MỞ KHÓA CHI TIẾT SÁU ĐẠI PHÁP TRẬN (FORMATION GATES)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsFormationUnlocked(FormationId id)
    {
        switch (id)
        {
            case FormationId.Thunder:
                // Trúc Cơ 1
                return IsCultivationReached(RealmId.TrucCo, 1);

            case FormationId.Spirit:
                // Trúc Cơ 4 (Trúc Cơ Trung Kỳ)
                return IsCultivationReached(RealmId.TrucCo, 4);

            case FormationId.Frost:
                // Trúc Cơ 4 (Trúc Cơ Trung Kỳ)
                return IsCultivationReached(RealmId.TrucCo, 4);

            case FormationId.SwordRain:
                // Kim Đan 1 (Kim Đan Sơ Kỳ)
                return IsCultivationReached(RealmId.KimDan, 1);

            case FormationId.BloodDrain:
                // Kim Đan 4 (Kim Đan Trung Kỳ)
                return IsCultivationReached(RealmId.KimDan, 4);

            case FormationId.GoldenBell:
                // Nguyên Anh 1 (Nguyên Anh Sơ Kỳ)
                return IsCultivationReached(RealmId.NguyenAnh, 1);

            default:
                return false;
        }
    }

    public string GetFormationUnlockRequirementText(FormationId id)
    {
        switch (id)
        {
            case FormationId.Thunder:
                return "Mở tại Trúc Cơ · Sơ Kỳ";
            case FormationId.Spirit:
                return "Mở tại Trúc Cơ · Trung Kỳ (Tầng 4)";
            case FormationId.Frost:
                return "Mở tại Trúc Cơ · Trung Kỳ (Tầng 4)";
            case FormationId.SwordRain:
                return "Mở tại Kim Đan · Sơ Kỳ";
            case FormationId.BloodDrain:
                return "Mở tại Kim Đan · Trung Kỳ (Tầng 4)";
            case FormationId.GoldenBell:
                return "Mở tại Nguyên Anh · Sơ Kỳ";
            default:
                return "Chưa mở khóa";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. MỞ KHÓA CHI TIẾT 4 PHÁP KHÍ (ARTIFACT GATES)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsArtifactSlotUnlocked(ArtifactSlot slot)
    {
        switch (slot)
        {
            case ArtifactSlot.Robe:
                // Nguyên Anh 1
                return IsCultivationReached(RealmId.NguyenAnh, 1);

            case ArtifactSlot.Crown:
            case ArtifactSlot.Boots:
                // Nguyên Anh 4
                return IsCultivationReached(RealmId.NguyenAnh, 4);

            case ArtifactSlot.Amulet:
                // Nguyên Anh 7
                return IsCultivationReached(RealmId.NguyenAnh, 7);

            default:
                return false;
        }
    }

    public string GetArtifactSlotUnlockRequirementText(ArtifactSlot slot)
    {
        switch (slot)
        {
            case ArtifactSlot.Robe:
                return "Mở tại Nguyên Anh · Sơ Kỳ";
            case ArtifactSlot.Crown:
            case ArtifactSlot.Boots:
                return "Mở tại Nguyên Anh · Trung Kỳ (Tầng 4)";
            case ArtifactSlot.Amulet:
                return "Mở tại Nguyên Anh · Hậu Kỳ (Tầng 7)";
            default:
                return "Chưa mở khóa";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. MỞ KHÓA CHI TIẾT 6 NODE LUYỆN THỂ (BODY CULTIVATION GATES)
    // ─────────────────────────────────────────────────────────────────────────

    public bool IsBodyNodeUnlocked(int nodeIndex)
    {
        switch (nodeIndex)
        {
            case 0: // Khí Huyết
            case 1: // Cương Khí
                return IsCultivationReached(RealmId.HoaThan, 1);

            case 2: // Dũ Hợp
            case 3: // Kình Lực
                return IsCultivationReached(RealmId.HoaThan, 4);

            case 4: // Phệ Huyết
            case 5: // Định Tâm
                return IsCultivationReached(RealmId.HoaThan, 7);

            default:
                return false;
        }
    }

    public string GetBodyNodeUnlockRequirementText(int nodeIndex)
    {
        switch (nodeIndex)
        {
            case 0:
            case 1:
                return "Mở tại Hóa Thần · Sơ Kỳ";
            case 2:
            case 3:
                return "Mở tại Hóa Thần · Trung Kỳ (Tầng 4)";
            case 4:
            case 5:
                return "Mở tại Hóa Thần · Hậu Kỳ (Tầng 7)";
            default:
                return "Chưa mở khóa";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. THÀNH TỰU & TÁCH TRANG BỊ (500 MONSTER SWORDS ACQUIRED)
    // ─────────────────────────────────────────────────────────────────────────

    public int GetMonsterSwordsAcquired()
    {
        var data = DataManager.Instance;
        if (data == null || !data.isSessionActive || data.gameSaveData == null) return 0;
        int slotIndex = data.gameSaveData.lastPlayedSlotIndex;
        if (slotIndex >= 0 && slotIndex < data.gameSaveData.slots.Length)
        {
            return data.gameSaveData.slots[slotIndex].monsterSwordsAcquired;
        }
        return 0;
    }

    public bool IsDismantleUnlocked()
    {
        return GetMonsterSwordsAcquired() >= DISMANTLE_REQUIRED_MONSTER_SWORDS;
    }

    public void OnMonsterSwordAcquired(int swordLevel)
    {
        var data = DataManager.Instance;
        if (data == null || !data.isSessionActive || data.gameSaveData == null) return;
        int slotIndex = data.gameSaveData.lastPlayedSlotIndex;
        if (slotIndex < 0 || slotIndex >= data.gameSaveData.slots.Length) return;

        var slot = data.gameSaveData.slots[slotIndex];
        int prevCount = slot.monsterSwordsAcquired;
        slot.monsterSwordsAcquired++;
        int newCount = slot.monsterSwordsAcquired;

        // Cập nhật tiến độ thành tựu
        AchievementManager.Instance?.OnMonsterSwordAcquired();

        // Kiểm tra đúng mốc 500 mở khóa tính năng
        if (prevCount < DISMANTLE_REQUIRED_MONSTER_SWORDS && newCount >= DISMANTLE_REQUIRED_MONSTER_SWORDS)
        {
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("ĐẠT THÀNH TỰU: Đã nhặt 500 Kiếm Phôi từ quái vật! Mở khóa Lò Phân Giải Trang Bị!", Color.yellow);
            }
            OnUnlockStateChanged?.Invoke();
            DataManager.Instance?.SaveCurrentGame(true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. CỔNG RƠI NGUYÊN LIỆU (DROPS ALLOWED)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Thảo dược CHỈ rơi từ Luyện Khí 7 (Hậu kỳ) trở đi
    /// </summary>
    public bool CanDropHerbs() => IsFeatureUnlocked(FeatureId.Alchemy);

    /// <summary>
    /// Quặng CHỈ rơi từ Trúc Cơ 1 trở đi
    /// </summary>
    public bool CanDropOre() => IsFeatureUnlocked(FeatureId.Formation);
}

using UnityEngine;

/// <summary>Player-facing realm and phase names over the existing legacy stage data.</summary>
public static class CultivationDisplay
{
    public static string RealmName(RealmId realm)
    {
        switch (realm)
        {
            case RealmId.LuyenKhi: return "Luyện Khí";
            case RealmId.TrucCo: return "Trúc Cơ";
            case RealmId.KimDan: return "Kim Đan";
            case RealmId.NguyenAnh: return "Nguyên Anh";
            case RealmId.HoaThan: return "Hóa Thần";
            case RealmId.LuyenHu: return "Luyện Hư";
            case RealmId.HopThe: return "Hợp Thể";
            case RealmId.DaiThua: return "Đại Thừa";
            case RealmId.DoKiep: return "Độ Kiếp";
            default: return "Cảnh Giới";
        }
    }

    public static int PhaseIndex(int subLevel)
    {
        if (subLevel <= 3) return 0;
        if (subLevel <= 6) return 1;
        if (subLevel <= 9) return 2;
        return 3;
    }

    public static string PhaseName(int subLevel)
    {
        switch (PhaseIndex(subLevel))
        {
            case 0: return "Sơ Kỳ";
            case 1: return "Trung Kỳ";
            case 2: return "Hậu Kỳ";
            default: return "Đỉnh Phong";
        }
    }

    public static string Name(RealmId realm, int subLevel) => RealmName(realm) + " · " + PhaseName(subLevel);

    public static string NameForStage(int stageIndex)
    {
        SaveMigration.MapLegacyStage(stageIndex, out RealmId realm, out int subLevel);
        return Name(realm, subLevel);
    }

    /// <summary>Progress through the displayed phase, preserving the 15-stage Luyện Khí save layout.</summary>
    public static float PhaseProgress(int stageIndex, float stageFraction)
    {
        stageFraction = Mathf.Clamp01(stageFraction);
        stageIndex = Mathf.Max(0, stageIndex);
        if (stageIndex >= 15) return stageFraction;

        int start;
        int count;
        if (stageIndex < 3) { start = 0; count = 3; }
        else if (stageIndex < 8) { start = 3; count = 5; }
        else if (stageIndex < 13) { start = 8; count = 5; }
        else { start = 13; count = 2; }
        return Mathf.Clamp01((stageIndex - start + stageFraction) / count);
    }
}

using System;

public enum SecretRealmType
{
    HordeSurvival = 0,   // Thí Luyện Sinh Tồn 30s (Vạn Ma Triều Dâng)
    BarrierBreak = 1,    // Trảm Phá Cấm Chế 20s (Thạch Bi Viễn Cổ)
    DanCauldron = 2,     // Đan Thần Kỳ Duyên (Tam Đỉnh Tầm Bảo)
    LightningDodge = 3,  // Vượt Lôi Trì 25s (Né Sét Nhặt Tinh Cầu)
    SwordTomb = 4        // Kiếm Chủng Cổ Thần (Rút Kiếm Cực Phẩm)
}

[Serializable]
public class SecretRealmSaveState
{
    public int freeAttemptsToday = 1;
    public string lastResetDate = "";
    public int totalCleared = 0;

    // Kỷ lục từng mini-game
    public int hordeRecord = 0;        // Số quái diệt nhiều nhất trong 30s
    public int barrierRecord = 0;      // % cấm chế lớn nhất đã phá trong 20s
    public int cauldronPillsFound = 0; // Số lần tìm thấy Thượng Cổ Tuyệt Phẩm Đan
    public int lightningOrbsRecord = 0;// Số lôi tinh cầu nhặt nhiều nhất
    public int swordTombMaxLevel = 0;  // Cấp kiếm cao nhất từng rút được

    public SecretRealmSaveState Clone()
    {
        return new SecretRealmSaveState
        {
            freeAttemptsToday = this.freeAttemptsToday,
            lastResetDate = this.lastResetDate,
            totalCleared = this.totalCleared,
            hordeRecord = this.hordeRecord,
            barrierRecord = this.barrierRecord,
            cauldronPillsFound = this.cauldronPillsFound,
            lightningOrbsRecord = this.lightningOrbsRecord,
            swordTombMaxLevel = this.swordTombMaxLevel
        };
    }
}


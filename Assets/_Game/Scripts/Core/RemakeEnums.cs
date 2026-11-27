namespace KhiVanChiTu.Remake
{
    public enum RemakeRealmId
    {
        LuyenKhi = 0,
        TrucCo = 1,
        KimDan = 2,
        NguyenAnh = 3,
        HoaThan = 4,
        LuyenHu = 5,
        HopThe = 6,
        DaiThua = 7,
        DoKiep = 8
    }

    public enum RemakeDifficulty
    {
        PhamGioi = 0,
        YeuVuc = 1,
        MaUyen = 2,
        TienKiep = 3
    }

    public enum RemakeMonsterArchetype
    {
        FastMelee = 0,
        FlyingAgile = 1,
        ArmoredTank = 2,
        BossGuardian = 3
    }

    public enum RemakeSwordStyle
    {
        KiemXuyen = 0,
        PhaGiap = 1
    }

    public enum RemakePillQuality
    {
        PheDan = 0,
        HaPham = 1,
        TrungPham = 2,
        ThuongPham = 3,
        CucPham = 4
    }

    public enum RemakeRarity
    {
        Pham = 0,
        Linh = 1,
        Huyen = 2,
        Dia = 3,
        Thien = 4
    }

    public enum RemakeEquipSlot
    {
        Robe = 0,
        Crown = 1,
        Boots = 2,
        Amulet = 3,
        Bracer = 4,
        Ring = 5
    }

    public enum RemakeEquipSet
    {
        None = 0,
        ThanhVan = 1,
        HuyenVu = 2,
        DanTong = 3
    }

    public enum RemakeEquipStatType
    {
        FlatSwordAtk = 0,
        SwordDamagePct = 1,
        CritChancePct = 2,
        MaxHpPct = 3,
        TuViGainPct = 4,
        LinhThachPct = 5
    }

    public enum RemakeGraphicsPreset
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum RemakeShadowMode
    {
        Off = 0,
        Low = 1,
        High = 2
    }

    public enum RemakeVfxDensity
    {
        Minimal = 0,
        Full = 1
    }

    public enum RemakeFeatureId
    {
        CombatAndSwordPath = 0,
        DaoVanTree = 1,
        SkillLoiPhat = 2,
        Alchemy = 3,
        EquipmentLoot = 4,
        Formation1 = 5,
        BreakthroughTrucCo = 6,
        OnlineFeatures = 7,
        FormationSubSlot = 8,
        SkillYinYang = 9,
        SkillSwordRain = 10,
        SkillPalm = 11,
        ImmortalTower = 12,
        SecretRealm = 13,
        PvpComingSoon = 14,
        PvpArena = 15,
        Companions = 16,
        CodexCollection = 17
    }

    public enum RemakeArenaMode
    {
        Campaign = 0,
        DemonTower = 1,
        SecretRealm = 2
    }

    public enum RemakeSecretRealmKind
    {
        SwordTomb = 0,
        HerbGarden = 1,
        OreCavern = 2
    }

    public enum RemakeCharacterGender
    {
        Female = 0,
        Male = 1
    }

    public enum RemakeCompanionId
    {
        None = 0,
        AzureCrane = 1,
        WhiteFox = 2,
        JadeTurtle = 3,
        ThunderSpirit = 4
    }
}

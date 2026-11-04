using System;

public enum PillRecipe { Cultivation, Breakthrough }
public enum PillQuality { Waste, Basic, Refined, Supreme }
public enum ArtifactSlot { Robe, Crown, Boots, Amulet }
public enum FormationId { Thunder, Spirit, Frost, SwordRain, BloodDrain, GoldenBell }

[Serializable]
public class AlchemySaveState
{
    public int[] herbs = new int[3]; // Linh Chi, Cỏ Ngưng Khí, Tuyết Liên
    public int[] pills = new int[9 * 2 * 4]; // realm, recipe, quality
    public int medicineDust;
    public int masteryXp;
    public int craftsWithoutSupreme;
    public int brewingRecipe = -1;
    public int brewingRealm;
    public long brewEndUtcTicks;
}

[Serializable]
public class ArtifactSaveState
{
    public int ore;
    public int[] levels = new int[4];
}

[Serializable]
public class FormationSaveState
{
    public int[] levels = { 1, 0, 0, 0, 0, 0 };
    public int selectedFormation;
    public long spiritEndUtcTicks;
    public long spiritReadyUtcTicks;
}

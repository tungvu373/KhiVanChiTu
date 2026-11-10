using System;
using System.Collections.Generic;

public enum PendingLootType
{
    Sword = 0,
    Pill = 1,
    Herb = 2,
    Ore = 3
}

[Serializable]
public class PendingLootItem
{
    public string id = Guid.NewGuid().ToString("N");
    public PendingLootType itemType;
    public string displayName = "";
    public int level = 1;
    public int pieces = 1;
    public int realm = 0;
    public int recipe = 0;
    public int quality = 0;
    public int amount = 1;

    public PendingLootItem()
    {
        id = Guid.NewGuid().ToString("N");
    }

    public PendingLootItem(PendingLootType type, string name, int lvl, int pcs, int amt)
    {
        id = Guid.NewGuid().ToString("N");
        itemType = type;
        displayName = name;
        level = lvl;
        pieces = pcs;
        amount = amt;
    }

    public PendingLootItem(int pillRealm, int pillRecipe, int pillQuality, int amt)
    {
        id = Guid.NewGuid().ToString("N");
        itemType = PendingLootType.Pill;
        realm = pillRealm;
        recipe = pillRecipe;
        quality = pillQuality;
        amount = amt;
        displayName = $"{(PillQuality)pillQuality} {((PillRecipe)pillRecipe == PillRecipe.Breakthrough ? "Đột Phá Đan" : "Tu Vi Đan")}";
    }
}

[Serializable]
public class StorageSaveState
{
    public bool filterSwordsHighLevel = false;         // Ưu tiên nhận kiếm cấp cao (>= Lv2)
    public bool filterPillsBreakthrough = true;        // Ưu tiên đan đột phá & cực phẩm
    public bool filterRareHerbs = true;                // Ưu tiên thảo dược quý (Tuyết Liên)
    public bool autoDismantleLv1SwordsWhenFull = true; // Khi kho kiếm đầy, tự động đổi kiếm Lv1 ra Linh Thạch
    public bool autoDismantleWastePills = true;        // Tự động phân giải phế đan ra Dược Trần & Linh Thạch
    public List<PendingLootItem> pendingLoot = new List<PendingLootItem>();
}


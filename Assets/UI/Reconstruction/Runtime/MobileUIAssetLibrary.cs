using UnityEngine;

/// <summary>Scene-selected art. Contains no currency, progression, or save state.</summary>
public sealed class MobileUIAssetLibrary : ScriptableObject
{
    public Sprite panel, border, popup, hud, navigation, upgrade, slot, selection;
    public Sprite reward, tab, track, divider, stone, fate, ore, dust;
    public Sprite cultivation, alchemy, formation;
    // Neutral, green, blue, violet, orange, gold. Gold is pill Supreme only.
    public Sprite[] rarity = new Sprite[6];
    public MobileUIPanelView workbenchPrefab, modalPrefab, pagePrefab;
    public UnityEngine.UI.ScrollRect scrollPrefab;
    public Font regular, semibold;

    public Sprite SwordRarity(int level)
    {
        int index = level <= 1 ? 0 : level == 2 ? 1 : level <= 4 ? 2 : level <= 7 ? 3 : 4;
        return rarity != null && index < rarity.Length ? rarity[index] : null;
    }
    public Sprite PillRarity(PillQuality quality)
    {
        int index = quality == PillQuality.Supreme ? 5 : quality == PillQuality.Refined ? 2 : quality == PillQuality.Basic ? 1 : 0;
        return rarity != null && index < rarity.Length ? rarity[index] : null;
    }
}

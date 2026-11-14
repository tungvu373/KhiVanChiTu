using UnityEngine;

/// <summary>Visual item adapter. Never add this hierarchy inside a MergeSlot's logical child list.</summary>
public sealed class MobileUIItemView : MonoBehaviour
{
    public MobileUIAssetLibrary assets;
    public UnityEngine.UI.Image icon, rarity, selection;
    public UnityEngine.UI.Text caption, count;
    public void ShowSword(Sprite itemIcon, int level, string amount, bool selected)
    {
        Show(itemIcon, assets != null ? assets.SwordRarity(level) : null, "Lv." + level, amount, selected);
    }
    public void ShowPill(Sprite itemIcon, PillQuality quality, string amount, bool selected)
    {
        Show(itemIcon, assets != null ? assets.PillRarity(quality) : null, quality.ToString(), amount, selected);
    }
    void Show(Sprite item, Sprite frame, string label, string amount, bool selected)
    {
        if (icon != null) { icon.sprite = item; icon.enabled = item != null; icon.preserveAspect = true; }
        if (rarity != null) { rarity.sprite = frame; rarity.enabled = frame != null; }
        if (selection != null) selection.enabled = selected;
        if (caption != null) caption.text = label;
        if (count != null) count.text = amount ?? "";
    }
}

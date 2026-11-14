using UnityEngine;

/// <summary>Normalizes Unity's downscaled Sprite PPU, preserving border dimensions in reference units.</summary>
[ExecuteAlways, DisallowMultipleComponent]
public sealed class MobileUISlicedSurface : MonoBehaviour
{
    UnityEngine.UI.Image image;
    public static bool IsHorizontal(Sprite sprite)
    {
        if (sprite == null) return false;
        string n = sprite.name;
        return n == "JadeV5_HudContainer" || n == "JadeV5_UpgradeButton" ||
               n == "JadeV5_TabBackground" || n == "JadeV5_ResourceTrack";
    }
    public static void Configure(UnityEngine.UI.Image target)
    {
        if (target == null || target.sprite == null || !target.sprite.name.StartsWith("JadeV5_") || target.sprite.border.sqrMagnitude == 0) return;
        var component = target.GetComponent<MobileUISlicedSurface>();
        if (component == null) component = target.gameObject.AddComponent<MobileUISlicedSurface>();
        component.Refresh();
    }
    void OnEnable() { Refresh(); }
    void OnRectTransformDimensionsChange() { Refresh(); }
    public void Refresh()
    {
        if (image == null) image = GetComponent<UnityEngine.UI.Image>();
        if (image == null || image.sprite == null || !image.sprite.name.StartsWith("JadeV5_") || image.sprite.border.sqrMagnitude == 0) return;
        float height = image.rectTransform.rect.height;
        if (height <= 0) return;
        var canvas = image.canvas;
        float canvasPPU = canvas != null ? canvas.referencePixelsPerUnit : 100f;
        float multiplier = canvasPPU / image.sprite.pixelsPerUnit;
        if (IsHorizontal(image.sprite)) multiplier *= image.sprite.rect.height / height;
        else if(image.sprite.name=="JadeV5_EquipmentSlot"||image.sprite.name=="JadeV5_SelectionFrame") multiplier/=.55f;
        image.type = UnityEngine.UI.Image.Type.Sliced;
        if (Mathf.Abs(image.pixelsPerUnitMultiplier - multiplier) > .0001f)
            image.pixelsPerUnitMultiplier = Mathf.Max(.01f, multiplier);
    }
}

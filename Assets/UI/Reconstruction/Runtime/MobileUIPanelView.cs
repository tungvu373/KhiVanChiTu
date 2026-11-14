using UnityEngine;

/// <summary>Reusable presentation slots. Existing controllers can keep their original content objects.</summary>
public sealed class MobileUIPanelView : MonoBehaviour
{
    public RectTransform header, body, footer;
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Image surface, ornament, emblem;
    public void SetTitle(string value) { if (title != null) title.text = value ?? ""; }
    public void SetEmblem(Sprite value)
    {
        if (emblem == null) return;
        emblem.sprite = value; emblem.enabled = value != null;
        emblem.preserveAspect = true; emblem.raycastTarget = false;
    }
}

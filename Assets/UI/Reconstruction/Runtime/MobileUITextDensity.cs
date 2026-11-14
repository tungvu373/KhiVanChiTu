using UnityEngine;

/// <summary>Rasterizes legacy Text at the parent viewport's density, preserving its visible rectangle.</summary>
[DisallowMultipleComponent]
public sealed class MobileUITextDensity : MonoBehaviour
{
    UnityEngine.UI.Text label;
    Vector2 size;
    Vector3 restScale;
    int fontSize;
    bool applied;
    public void Restore()
    {
        if(!applied||label==null)return;
        label.fontSize=fontSize;label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x);label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y);label.transform.localScale=restScale;applied=false;
    }
    public void Apply()
    {
        if(applied)return;label=GetComponent<UnityEngine.UI.Text>();if(label==null)return;
        size=label.rectTransform.rect.size;restScale=label.transform.localScale;fontSize=label.fontSize;
        label.fontSize=fontSize*3;label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x*3);label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y*3);label.transform.localScale=restScale/3;
        label.cachedTextGenerator.Invalidate();label.SetAllDirty();applied=true;
    }
}

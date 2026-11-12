using UnityEngine;

/// <summary>Reapply layout once after a legacy modal builder binds its existing hierarchy.</summary>
[DefaultExecutionOrder(1400)]
public sealed class JadeModalPresentation : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    public JadeUITheme theme;
    private bool pending=true;
    private void OnEnable(){pending=true;}
    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) { }
    private void LateUpdate()
    {
        if(!pending||theme==null)return;
        pending=false;
        JadeUIAdaptiveSkin.Apply((RectTransform)transform,theme);
    }
}

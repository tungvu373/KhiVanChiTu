using UnityEngine;

/// <summary>Fits the original 360-unit trial coordinates into this scene's 1080-unit safe viewport.</summary>
[DefaultExecutionOrder(1500), DisallowMultipleComponent]
public sealed class MobileUILogicalViewport : MonoBehaviour
{
    RectTransform view, parent;
    Vector2 previousSize;
    Rect previousSafe;
    void OnEnable() { Refresh(); }
    void LateUpdate()
    {
        if(parent==null||previousSize!=parent.rect.size||previousSafe!=Screen.safeArea)Refresh();
    }
    public void Refresh()
    {
        view=(RectTransform)transform;parent=view.parent as RectTransform;
        if(parent==null||Screen.width<=0||Screen.height<=0)return;
        previousSize=parent.rect.size;previousSafe=Screen.safeArea;
        var safe=Screen.safeArea;
        if(safe.width<=0||safe.height<=0||safe.xMax>Screen.width||safe.yMax>Screen.height)safe=new Rect(0,0,Screen.width,Screen.height);
        Vector2 available=new Vector2(parent.rect.width*safe.width/Screen.width,parent.rect.height*safe.height/Screen.height);
        float scale=available.x/360f;
        if(scale<=0)return;
        view.anchorMin=view.anchorMax=view.pivot=new Vector2(.5f,.5f);
        view.sizeDelta=new Vector2(360,available.y/scale);view.localScale=Vector3.one*scale;
        view.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*parent.rect.width,(safe.center.y/Screen.height-.5f)*parent.rect.height);
    }
}

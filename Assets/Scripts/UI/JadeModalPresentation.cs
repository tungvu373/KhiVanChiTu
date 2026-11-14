using UnityEngine;

/// <summary>Reapply layout once after a legacy modal builder binds its existing hierarchy.</summary>
[DefaultExecutionOrder(1400)]
public sealed class JadeModalPresentation : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    public JadeUITheme theme;
    private bool pending=true;
    private Coroutine motion;
    private void OnEnable(){pending=true;}
    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) { }
    private void LateUpdate()
    {
        if(!pending||theme==null)return;
        pending=false;
        JadeUIAdaptiveSkin.Apply((RectTransform)transform,theme);
        if (theme.reconstruction != null) theme.reconstruction.StyleElement((RectTransform)transform);
        if (theme.reconstruction != null) MobileUIResponsiveRepairs.ApplyTree(theme.reconstruction,transform);
        var director=MobileUIAnimationDirector.For(this);
        bool native=GetComponentInParent<StorageModalUI>()!=null||GetComponentInParent<DismantleConfirmModalUI>()!=null||
            GetComponentInParent<CharacterDetailModalUI>()!=null||GetComponentInParent<TribulationModalUI>()!=null||
            GetComponentInParent<AchievementModalUI>()!=null||GetComponentInParent<OfflineRewardPopupUI>()!=null||
            (transform.parent!=null&&transform.parent.name=="SecretRealmResultModal");
        if(director!=null&&!native)
        {
            var group=GetComponent<CanvasGroup>();if(group==null)group=gameObject.AddComponent<CanvasGroup>();
            bool reward=transform.parent!=null&&transform.parent.name=="SecretRealmResultModal";
            motion=StartCoroutine(MobileUIModalMotion.Animate(this,group,(RectTransform)transform,true,manageInput:false,hideOnClose:false,reward:reward));
        }
    }
    private void OnDisable(){if(motion!=null)StopCoroutine(motion);motion=null;}
}

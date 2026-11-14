using System.Collections;
using UnityEngine;

/// <summary>One interruptible pose channel per modal visual. Old generations never hide a newer opening.</summary>
[DisallowMultipleComponent]
public sealed class MobileUIModalMotion : MonoBehaviour
{
    RectTransform visual;CanvasGroup group;Vector3 rest;bool initialized,running;
    Vector3 currentScale;float currentAlpha;int generation;
    public bool IsAnimating=>running;
    public int Generation=>generation;
    public Vector3 RestScale=>rest;
    public Vector3 VisualScale=>visual!=null?visual.localScale:rest;
    public bool HasScaleChannel=>visual!=null;
    public float NormalizedProgress{get;private set;}

    public static IEnumerator Animate(MonoBehaviour owner,CanvasGroup canvasGroup,RectTransform target,bool opening,bool manageInput=true,bool hideOnClose=true,bool reward=false)
    {
        if(canvasGroup==null){canvasGroup=owner.GetComponent<CanvasGroup>();if(canvasGroup==null)canvasGroup=owner.gameObject.AddComponent<CanvasGroup>();}
        // This runs when the iterator is requested, before Unity advances its first animation frame.
        if(manageInput){canvasGroup.interactable=opening;canvasGroup.blocksRaycasts=opening;}
        return Run(owner,canvasGroup,target,opening,hideOnClose,reward,manageInput);
    }
    static IEnumerator Run(MonoBehaviour owner,CanvasGroup canvasGroup,RectTransform target,bool opening,bool hideOnClose,bool reward,bool nativeOwner)
    {
        var director=MobileUIAnimationDirector.For(owner);
        if(director==null||target==null)yield break;
        if(canvasGroup==null){canvasGroup=owner.GetComponent<CanvasGroup>();if(canvasGroup==null)canvasGroup=owner.gameObject.AddComponent<CanvasGroup>();}
        var state=target.GetComponent<MobileUIModalMotion>();if(state==null)state=target.gameObject.AddComponent<MobileUIModalMotion>();
        if(!state.initialized)
        {
            // Layout adapters own the popup RectTransform. Only its non-interactive frame may scale.
            var frame=target.Find("P6Frame") as RectTransform;
            if(frame!=null&&frame.GetComponentsInChildren<UnityEngine.UI.Selectable>(true).Length==0)state.visual=frame;
            state.group=canvasGroup;state.rest=state.visual!=null?state.visual.localScale:Vector3.one;state.initialized=true;
        }
        int version=++state.generation;bool interrupted=state.running;
        var preset=opening?(reward?director.rewardReveal:director.modalOpen):director.modalClose;
        float duration=director.Duration(preset);
        Vector3 from=interrupted?state.currentScale:state.rest*(director.reducedMotion?1:preset.scaleFrom);
        float alpha=interrupted?state.currentAlpha:preset.alphaFrom;
        Vector3 to=state.rest*(director.reducedMotion?1:preset.scaleTo);
        state.running=true;float elapsed=0;
        while(elapsed<duration)
        {
            if(state==null||state.generation!=version)yield break;
            using(MobileUIAnimationDirector.MotionMarker.Auto())
            {
                elapsed+=Time.unscaledDeltaTime;state.NormalizedProgress=Mathf.Clamp01(elapsed/duration);
                float t=preset.Evaluate(state.NormalizedProgress);state.currentScale=Vector3.LerpUnclamped(from,to,t);state.currentAlpha=Mathf.Lerp(alpha,preset.alphaTo,t);
                if(state.visual!=null)state.visual.localScale=state.currentScale;canvasGroup.alpha=state.currentAlpha;
            }
            yield return null;
        }
        if(state==null||state.generation!=version)yield break;
        if(state.visual!=null)state.visual.localScale=to;canvasGroup.alpha=preset.alphaTo;state.running=false;state.NormalizedProgress=1;
        if(!opening&&hideOnClose)owner.gameObject.SetActive(false);
    }
    void OnDisable()
    {
        generation++;running=false;
        if(!initialized)return;
        if(visual!=null)visual.localScale=rest;
        if(group!=null)group.alpha=1;
    }
}

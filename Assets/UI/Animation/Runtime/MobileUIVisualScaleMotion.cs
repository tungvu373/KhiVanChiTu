using UnityEngine;

/// <summary>Visual-only scale tween; sleeps between pointer transitions and never changes a touch RectTransform.</summary>
[DisallowMultipleComponent,DefaultExecutionOrder(1700)]
public sealed class MobileUIVisualScaleMotion : MonoBehaviour
{
    RectTransform visual;Vector3 from,to,rest;MobileUIMotionPreset preset;float elapsed,duration;
    public bool IsAnimating=>enabled;
    public void Play(Vector3 restScale,MobileUIMotionPreset timing,bool reduced)
    {
        visual=(RectTransform)transform;rest=restScale;preset=timing;from=visual.localScale;
        to=rest*(reduced?1:timing.scaleTo);duration=reduced?0.08f:Mathf.Max(.01f,timing.duration);elapsed=0;enabled=true;
    }
    void LateUpdate()
    {
        if(preset==null||visual==null){enabled=false;return;}
        using(MobileUIAnimationDirector.MotionMarker.Auto())
        {
            elapsed+=Time.unscaledDeltaTime;visual.localScale=Vector3.LerpUnclamped(from,to,preset.Evaluate(elapsed/duration));
            if(elapsed>=duration){visual.localScale=to;enabled=false;}
        }
    }
    public void Cancel(){if(visual!=null)visual.localScale=rest;enabled=false;}
}

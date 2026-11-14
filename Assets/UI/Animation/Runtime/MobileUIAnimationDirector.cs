using UnityEngine;
using Unity.Profiling;

/// <summary>Scene-scoped motion library. Controllers retain navigation, input and transaction ownership.</summary>
[DefaultExecutionOrder(-1500),DisallowMultipleComponent]
public sealed class MobileUIAnimationDirector : MonoBehaviour
{
    public MobileUIMotionPreset modalOpen,modalClose,sheetOpen,sheetClose,buttonDown,buttonUp,rewardReveal,accent,progress;
    [Tooltip("Presentation-only option; no new persistent setting or gameplay state.")]
    public bool reducedMotion;
    static MobileUIAnimationDirector instance;
    int accentFrame=-1,accentCount;float nextAccentBatch;
    public static readonly ProfilerMarker MotionMarker=new ProfilerMarker("KV.UI.Motion");
    void Awake(){instance=this;}
    void OnEnable(){instance=this;}
    void OnDisable(){if(instance==this)instance=null;}
    public static MobileUIAnimationDirector For(Component owner)
    {
        return instance!=null&&instance.isActiveAndEnabled&&owner!=null&&owner.gameObject.scene==instance.gameObject.scene?instance:null;
    }
    public float Duration(MobileUIMotionPreset preset)=>reducedMotion?0.08f:Mathf.Max(0.01f,preset.duration);
    public void Press(RectTransform visual,Vector3 rest,bool down)
    {
        if(visual==null)return;
        var tween=visual.GetComponent<MobileUIVisualScaleMotion>();
        if(tween==null)tween=visual.gameObject.AddComponent<MobileUIVisualScaleMotion>();
        tween.Play(rest,down?buttonDown:buttonUp,reducedMotion);
    }
    public void Accent(UnityEngine.UI.Graphic visual,MobileUIMotionPreset preset=null)
    {
        if(visual==null||!visual.gameObject.activeInHierarchy||reducedMotion)return;
        if(accentFrame!=Time.frameCount)
        {
            if(Time.unscaledTime<nextAccentBatch)return;
            accentFrame=Time.frameCount;accentCount=0;nextAccentBatch=Time.unscaledTime+.6f;
        }
        if(accentCount++>=6)return;
        var pulse=visual.GetComponent<MobileUIAccentPulse>();
        if(pulse==null)pulse=visual.gameObject.AddComponent<MobileUIAccentPulse>();
        pulse.Play(visual,preset!=null?preset:accent,reducedMotion);
    }
}

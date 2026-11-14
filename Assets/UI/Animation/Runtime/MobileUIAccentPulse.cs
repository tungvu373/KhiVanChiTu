using UnityEngine;

/// <summary>Finite opacity reveal on an existing decorative Graphic; no particle, material or raycast changes.</summary>
[DisallowMultipleComponent,DefaultExecutionOrder(1700)]
public sealed class MobileUIAccentPulse : MonoBehaviour
{
    UnityEngine.UI.Graphic visual;MobileUIMotionPreset preset;float elapsed,duration;
    public void Play(UnityEngine.UI.Graphic target,MobileUIMotionPreset timing,bool lowMotion)
    {
        visual=target;preset=timing;duration=lowMotion ? .08f : Mathf.Max(.01f,timing.duration);elapsed=0;
        if(lowMotion){target.canvasRenderer.SetAlpha(1);enabled=false;return;}
        target.canvasRenderer.SetAlpha(timing.alphaFrom);enabled=true;
    }
    void LateUpdate()
    {
        if(visual==null||preset==null){enabled=false;return;}
        using(MobileUIAnimationDirector.MotionMarker.Auto())
        {
            elapsed+=Time.unscaledDeltaTime;visual.canvasRenderer.SetAlpha(Mathf.Lerp(preset.alphaFrom,preset.alphaTo,preset.Evaluate(elapsed/duration)));
            if(elapsed>=duration){visual.canvasRenderer.SetAlpha(1);enabled=false;}
        }
    }
    void OnDisable(){if(visual!=null)visual.canvasRenderer.SetAlpha(1);}
}

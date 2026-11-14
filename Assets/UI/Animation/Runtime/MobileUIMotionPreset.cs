using UnityEngine;

/// <summary>Reusable, unscaled presentation timing. No gameplay callbacks or data are stored here.</summary>
[CreateAssetMenu(menuName="Khí Vận/UI/Motion preset")]
public sealed class MobileUIMotionPreset : ScriptableObject
{
    [Min(0.01f)] public float duration=0.14f;
    [Range(0.9f,1.05f)] public float scaleFrom=0.98f,scaleTo=1f;
    [Range(0,1)] public float alphaFrom,alphaTo=1f;
    public AnimationCurve easing=AnimationCurve.EaseInOut(0,0,1,1);
    public AnimationClip previewClip;
    public float Evaluate(float normalized)=>Mathf.Clamp01(easing.Evaluate(Mathf.Clamp01(normalized)));
}

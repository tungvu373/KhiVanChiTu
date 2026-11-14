using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Unscaled press feedback on the visual only; the original touch target stays fixed.</summary>
[DisallowMultipleComponent]
public sealed class JadeButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private RectTransform visual;
    private UnityEngine.UI.Text label;
    private Vector3 rest;
    private float target = 1f;
    private UnityEngine.UI.Selectable selectable;
    private MobileUIAnimationDirector motion;
    private void Awake()
    {
        selectable=GetComponent<UnityEngine.UI.Selectable>();motion=MobileUIAnimationDirector.For(this);
        label = GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (label != null && label.transform != transform) { visual = label.rectTransform; rest = visual.localScale; }
    }
    public void OnPointerDown(PointerEventData e)
    {
        if(e.button!=PointerEventData.InputButton.Left||selectable==null||!selectable.IsInteractable())return;
        target=.96f;if(motion!=null)motion.Press(visual,rest,true);
    }
    public void OnPointerUp(PointerEventData e) { target = 1f;if(motion!=null)motion.Press(visual,rest,false); }
    public void OnPointerExit(PointerEventData e) { target = 1f;if(motion!=null)motion.Press(visual,rest,false); }
    private void Update()
    {
        if(motion!=null)return;
        if(label!=null&&label.color.r<.25f&&label.color.g<.25f&&label.color.b<.25f)label.color=JadeUITheme.Ivory;
        if (visual != null && visual.localScale != rest * target)
            visual.localScale = Vector3.MoveTowards(visual.localScale, rest * target, Time.unscaledDeltaTime * 2f);
    }
    private void OnDisable()
    {
        target=1f;
        if(visual!=null){var tween=visual.GetComponent<MobileUIVisualScaleMotion>();if(tween!=null)tween.Cancel();visual.localScale=rest;}
    }
}

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
    private void Awake()
    {
        label = GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (label != null && label.transform != transform) { visual = label.rectTransform; rest = visual.localScale; }
    }
    public void OnPointerDown(PointerEventData e) { if (GetComponent<UnityEngine.UI.Selectable>().IsInteractable()) target = .96f; }
    public void OnPointerUp(PointerEventData e) { target = 1f; }
    public void OnPointerExit(PointerEventData e) { target = 1f; }
    private void Update()
    {
        if(label!=null&&label.color.r<.25f&&label.color.g<.25f&&label.color.b<.25f)label.color=JadeUITheme.Ivory;
        if (visual != null && visual.localScale != rest * target)
            visual.localScale = Vector3.MoveTowards(visual.localScale, rest * target, Time.unscaledDeltaTime * 2f);
    }
    private void OnDisable() { target = 1f; if (visual != null) visual.localScale = rest; }
}

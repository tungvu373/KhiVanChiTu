using UnityEngine;

/// <summary>Fits an authored dialog to its safe-area parent while preserving internal geometry.</summary>
[DisallowMultipleComponent]
public sealed class JadeModalFit : MonoBehaviour
{
    public float preferredScale = 1f;
    public Vector2 margin = new Vector2(48, 64);
    private RectTransform box, parent;
    private Vector2 lastAvailable, lastSize;
    private float targetScale = 1f;
    private void Awake() { box = (RectTransform)transform; parent = transform.parent as RectTransform; }
    private void OnEnable() { Fit(); }
    private void LateUpdate() { if (parent != null && (parent.rect.size != lastAvailable || box.rect.size != lastSize || Mathf.Abs(box.localScale.x-targetScale)>.001f)) Fit(); }
    public void Fit()
    {
        if (box == null) box = (RectTransform)transform;
        if (parent == null) parent = transform.parent as RectTransform;
        if (parent == null || box.rect.width <= 0 || box.rect.height <= 0) return;
        lastAvailable = parent.rect.size; lastSize = box.rect.size;
        float scale = Mathf.Min(preferredScale, (lastAvailable.x - margin.x) / lastSize.x, (lastAvailable.y - margin.y) / lastSize.y);
        targetScale=Mathf.Max(.1f,scale);
        box.localScale = Vector3.one * targetScale;
    }
}

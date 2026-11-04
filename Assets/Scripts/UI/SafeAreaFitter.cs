using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    [SerializeField] private bool applyTop = true;
    [SerializeField] private bool applyBottom = true;
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Refresh();
    }

    private void Update()
    {
        if (lastSafeArea != Screen.safeArea || lastWidth != Screen.width || lastHeight != Screen.height) Refresh();
    }

    public void Refresh()
    {
        if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y = applyBottom ? (anchorMin.y / Screen.height) : 0f;
        anchorMax.x /= Screen.width;
        anchorMax.y = applyTop ? (anchorMax.y / Screen.height) : 1f;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}

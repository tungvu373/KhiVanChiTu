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
        // Simulator and Game View can briefly report different screen sizes when switching views.
        if (safeArea.width <= 0f || safeArea.height <= 0f ||
            safeArea.xMin < 0f || safeArea.yMin < 0f ||
            safeArea.xMax > lastWidth || safeArea.yMax > lastHeight)
            safeArea = new Rect(0f, 0f, lastWidth, lastHeight);
        Vector2 anchorMin;
        Vector2 anchorMax;
        anchorMin.x = Mathf.Clamp01(safeArea.xMin / Screen.width);
        anchorMax.x = Mathf.Clamp01(safeArea.xMax / Screen.width);
        anchorMin.y = applyBottom ? Mathf.Clamp01(safeArea.yMin / Screen.height) : 0f;
        anchorMax.y = applyTop ? Mathf.Clamp01(safeArea.yMax / Screen.height) : 1f;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}

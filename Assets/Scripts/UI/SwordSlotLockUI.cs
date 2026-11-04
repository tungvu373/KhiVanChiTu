using UnityEngine;
using UnityEngine.UI;

/// <summary>Phủ trạng thái khóa/trống lên ô trang bị kiếm chưa mở.</summary>
public class SwordSlotLockUI : MonoBehaviour
{
    private Text stateLabel;
    private Image background;
    private int slotIndex;

    private void Awake()
    {
        slotIndex = transform.GetSiblingIndex();
        background = GetComponent<Image>();
        Transform labelTransform = transform.Find("LockState");
        if (labelTransform == null)
        {
            GameObject labelObject = new GameObject("LockState", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            stateLabel = labelObject.GetComponent<Text>();
            stateLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stateLabel.fontSize = 24;
            stateLabel.alignment = TextAnchor.MiddleCenter;
            stateLabel.raycastTarget = false;
        }
        else stateLabel = labelTransform.GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (SwordLoadoutService.Instance != null)
            SwordLoadoutService.Instance.OnCapacityChanged += Refresh;
        Refresh(SwordLoadoutService.Instance != null ? SwordLoadoutService.Instance.GetCurrentCapacity() : 1);
    }

    private void OnDisable()
    {
        if (SwordLoadoutService.Instance != null)
            SwordLoadoutService.Instance.OnCapacityChanged -= Refresh;
    }

    private void Refresh(int capacity)
    {
        bool locked = slotIndex >= capacity;
        if (stateLabel != null)
        {
            stateLabel.gameObject.SetActive(locked);
            stateLabel.text = locked ? "🔒\nKhóa" : string.Empty;
            stateLabel.color = locked ? new Color(0.75f, 0.78f, 0.84f, 0.95f) : Color.clear;
        }
        if (background != null)
            background.color = locked ? new Color(0.08f, 0.1f, 0.14f, 0.92f) : new Color(0.16f, 0.2f, 0.28f, 1f);
    }
}

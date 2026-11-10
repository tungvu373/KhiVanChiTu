using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh Nhiệm Vụ Tu Chân thu gọn linh hoạt bên giữa-trái màn hình chuẩn Mobile Portrait.
/// Người chơi có thể bấm nút mũi tên ◀ / ▶ để thu gọn hoặc mở rộng bất cứ lúc nào,
/// tối ưu tầm nhìn chiến đấu ngự kiếm khi nằm nghiêng chơi game 1 tay.
/// </summary>
public class CollapsibleQuestBar : MonoBehaviour
{
    public static CollapsibleQuestBar Instance { get; private set; }

    public bool IsCollapsed { get; private set; } = false;
    private RectTransform rectTransform;
    private Vector2 expandedPos;
    private Vector2 collapsedPos;
    private Button toggleButton;
    private Text toggleArrowText;
    private Coroutine slideCoroutine;
    private const float DURATION = 0.22f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (rectTransform != null)
        {
            expandedPos = rectTransform.anchoredPosition;
            float width = rectTransform.sizeDelta.x > 0 ? rectTransform.sizeDelta.x : 260f;
            collapsedPos = new Vector2(-width + 32f, expandedPos.y);
        }
        BuildToggleButtonIfNeeded();
    }

    private void BuildToggleButtonIfNeeded()
    {
        if (transform.Find("BtnQuestToggle") != null) return;

        var btnObj = new GameObject("BtnQuestToggle", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(transform, false);

        var rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(26f, 0f);

        btnObj.GetComponent<Image>().color = new Color(0.11f, 0.15f, 0.22f, 0.95f);

        var txtObj = new GameObject("ArrowText", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(btnObj.transform, false);
        var txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        toggleArrowText = txtObj.GetComponent<Text>();
        toggleArrowText.font = UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        toggleArrowText.text = "<";
        toggleArrowText.fontSize = 18;
        toggleArrowText.alignment = TextAnchor.MiddleCenter;
        toggleArrowText.color = new Color(0.96f, 0.78f, 0.26f, 1f);

        toggleButton = btnObj.GetComponent<Button>();
        toggleButton.onClick.AddListener(ToggleQuestBar);
    }

    public void ToggleQuestBar()
    {
        IsCollapsed = !IsCollapsed;
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(IsCollapsed ? collapsedPos : expandedPos));

        if (toggleArrowText != null)
        {
            toggleArrowText.text = IsCollapsed ? ">" : "<";
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private IEnumerator SlideTo(Vector2 targetPos)
    {
        if (rectTransform == null) yield break;
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / DURATION);
            rectTransform.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos;
        slideCoroutine = null;
    }
}


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    private GameObject tooltipObj;
    private Text tooltipText;
    private RectTransform tooltipRt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        CreateTooltipUI();
    }

    private void Start()
    {
        // Tự động gắn Tooltip cho các nút bấm có sẵn trên màn hình
        AttachTooltip("btn_QuickMerge", "Gộp tất cả phôi kiếm cùng cấp trong kho.\nTiết kiệm thời gian kéo thả!");
        AttachTooltip("btn_Sort", "Sắp xếp lại kho đồ từ cấp cao xuống thấp.");
        AttachTooltip("btn_TeKiem", "Xóa toàn bộ kiếm Lv8 trong kho.\nMỗi thanh +1 Kiếm Ý.\n(1 Kiếm Ý = +50% Sát thương vĩnh viễn)");
        AttachTooltip("btn_OpenShop", "Mở Tiệm Đan Dược\n(Mua các loại đan dược tăng sức mạnh tạm thời)");
    }

    private void CreateTooltipUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        tooltipObj = new GameObject("TooltipPanel", typeof(RectTransform), typeof(Image));
        tooltipObj.transform.SetParent(canvas.transform, false);
        tooltipObj.transform.SetAsLastSibling(); // Luôn nổi lên trên cùng

        tooltipRt = tooltipObj.GetComponent<RectTransform>();
        tooltipRt.pivot = new Vector2(0, 1); // Neo góc trên trái để chuột không che mất

        Image bg = tooltipObj.GetComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        
        Outline outline = tooltipObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.8f, 0f); // Viền vàng kim
        outline.effectDistance = new Vector2(1, -1);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(tooltipObj.transform, false);
        
        tooltipText = txtObj.GetComponent<Text>();
        tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipText.color = Color.white;
        tooltipText.fontSize = 14;
        tooltipText.alignment = TextAnchor.UpperLeft;

        // Tự động co giãn theo nội dung Text
        ContentSizeFitter csf = tooltipObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup vlg = tooltipObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredWidth = 250; // Giới hạn chiều rộng tối đa (nếu text quá dài sẽ tự xuống dòng)

        tooltipObj.SetActive(false); // Ẩn mặc định
    }

    private void Update()
    {
        if (tooltipObj != null && tooltipObj.activeSelf)
        {
            // Tương thích với New Input System
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipObj.transform.parent.GetComponent<RectTransform>(), 
                mousePos, 
                null, 
                out localPoint);
            
            // Đẩy tooltip lệch khỏi con trỏ một chút
            tooltipRt.anchoredPosition = localPoint + new Vector2(15, -15);
        }
    }

    public void ShowTooltip(string content)
    {
        if (tooltipObj == null) return;
        
        tooltipObj.transform.SetAsLastSibling(); // Đảm bảo không bị UI khác đè lên
        tooltipText.text = content;
        tooltipObj.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipObj != null)
        {
            tooltipObj.SetActive(false);
        }
    }

    private void AttachTooltip(string objName, string content)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            UITooltipTrigger trigger = obj.GetComponent<UITooltipTrigger>();
            if (trigger == null) trigger = obj.AddComponent<UITooltipTrigger>();
            trigger.tooltipText = content;
        }
    }
}

public class UITooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && !string.IsNullOrEmpty(tooltipText))
        {
            TooltipManager.Instance.ShowTooltip(tooltipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}

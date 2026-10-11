using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [Header("Font tùy chọn (kéo Font tiếng Việt vào đây nếu muốn tùy biến)")]
    public Font customTooltipFont;

    private GameObject tooltipObj;
    private Text tooltipText;
    private RectTransform tooltipRt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
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
        
        // Tooltip cho Đơn vị tiền tệ
        AttachTooltip("txt_LinhThach", "Linh Thạch\nTiền tệ chính rớt ra từ quái vật.\nDùng để Nâng cấp thuộc tính và Đột Phá cảnh giới.");
        AttachTooltip("txt_CoDuyen", "Cơ Duyên\nĐiểm may mắn quý hiếm rớt ra khi đánh Boss.\nDùng để quay Gacha hoặc mua đồ hiếm.");
        AttachTooltip("Icon_LinhThach", "Linh Thạch\nTiền tệ chính rớt ra từ quái vật.\nDùng để Nâng cấp thuộc tính và Đột Phá cảnh giới.");
        AttachTooltip("Icon_CoDuyen", "Cơ Duyên\nĐiểm may mắn quý hiếm rớt ra khi đánh Boss.\nDùng để quay Gacha hoặc mua đồ hiếm.");
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
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        bg.raycastTarget = false; // QUAN TRỌNG: Không chặn tia chuột để tránh chớp giật Tooltip!
        
        // Đảm bảo không bao giờ chặn chuột
        CanvasGroup cg = tooltipObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        Outline outline = tooltipObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.9f); // Viền vàng kim tu tiên
        outline.effectDistance = new Vector2(1, -1);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(tooltipObj.transform, false);
        
        tooltipText = txtObj.GetComponent<Text>();
        Font fontToUse = customTooltipFont;
        if (fontToUse == null)
        {
            Text existingText = canvas.GetComponentInChildren<Text>(true);
            if (existingText != null && existingText.font != null)
            {
                fontToUse = existingText.font;
            }
        }
        if (fontToUse == null)
        {
            try
            {
                fontToUse = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch { }
        }
        tooltipText.font = fontToUse;
        tooltipText.color = Color.white;
        tooltipText.fontSize = 13;
        tooltipText.lineSpacing = 1.15f;
        tooltipText.alignment = TextAnchor.UpperLeft;
        tooltipText.raycastTarget = false;
        tooltipText.supportRichText = true;
        tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
        tooltipText.verticalOverflow = VerticalWrapMode.Overflow;

        // Tự động co giãn theo nội dung Text
        ContentSizeFitter csf = tooltipObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup vlg = tooltipObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredWidth = 240; // Chiều rộng bảng thông tin gọn gàng
        le.flexibleWidth = 0;

        tooltipObj.SetActive(false); // Ẩn mặc định
    }

    private void Update()
    {
        if (tooltipObj != null && tooltipObj.activeSelf)
        {
            // Tương thích với New Input System
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            
            RectTransform canvasRt = tooltipObj.transform.parent.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRt, 
                mousePos, 
                null, 
                out localPoint);
            
            // Xử lý thích ứng biên màn hình:
            // Nếu chuột ở nửa dưới màn hình (vùng Thẻ Kỹ Năng / Nâng Cấp), bật Pivot Y = 0 để Tooltip nổi lên TRÊN chuột
            float pivotY = (mousePos.y < Screen.height * 0.45f) ? 0f : 1f;
            float pivotX = (mousePos.x > Screen.width * 0.75f) ? 1f : 0f;
            tooltipRt.pivot = new Vector2(pivotX, pivotY);

            float offsetX = (pivotX == 0f) ? 15f : -15f;
            float offsetY = (pivotY == 0f) ? 20f : -15f;
            tooltipRt.anchoredPosition = localPoint + new Vector2(offsetX, offsetY);
        }
    }

    public void ShowTooltip(string content)
    {
        if (tooltipObj == null) return;
        
        tooltipObj.transform.SetAsLastSibling(); // Luôn nổi lên trên cùng
        tooltipText.text = content;
        
        // Ép tính toán lại kích thước lập tức để tránh lỗi giật layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRt);
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

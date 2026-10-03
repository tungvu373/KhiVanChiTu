using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MergeItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int level = 1;
    public int currentPieces = 1; // Số lượng mảnh/phôi đang gộp
    [HideInInspector] public Transform originalParent;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Text levelText;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        levelText = GetComponentInChildren<Text>();
        if (levelText != null)
        {
            levelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            levelText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    public int GetRequiredPieces()
    {
        if (level == 1) return 2;
        if (level == 2) return 2;
        if (level >= 8) return 9999; // Giới hạn cấp 8
        return level; // Cấp 3 cần 3, Cấp 4 cần 4...
    }

    public void SetLevel(int newLevel, int newPieces = 1)
    {
        level = newLevel;
        currentPieces = newPieces;
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (levelText != null) 
        {
            if (level >= 8)
                levelText.text = "Lv.MAX";
            else
                levelText.text = $"Lv.{level}\n({currentPieces}/{GetRequiredPieces()})";
        }
        
        // Màu sắc thay đổi theo cấp độ (Hue dịch chuyển)
        Image img = GetComponent<Image>();
        if (img != null)
        {
            float hue = (level * 0.15f) % 1f;
            img.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        
        // Nhấc lên lớp ngoài cùng Canvas để không bị đè
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) transform.SetParent(canvas.transform, true); // Phải dùng true ở đây để giữ vị trí dưới chuột
        
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false; // Xuyên thấu để bắt được sự kiện Drop của ô bên dưới
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        
        // Nếu không thả trúng ô nào (Transform vẫn ở root Canvas)
        if (transform.parent == transform.root || transform.parent.GetComponent<Canvas>() != null)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
        
        if (MergeManager.Instance != null) MergeManager.Instance.OnEquipChanged();
    }
}

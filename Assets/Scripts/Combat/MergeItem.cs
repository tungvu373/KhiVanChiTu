using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MergeItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    private void Start()
    {
        // Khi object được kích hoạt (hoặc chuyển trang), đảm bảo UI cập nhật lại 
        // để phòng trường hợp nó được Instantiate ở trang (page) đang ẩn (inactive)
        UpdateUI();
    }

    public int GetRequiredPieces()
    {
        if (level == 1) return 2;
        if (level == 2) return 2;
        if (level >= 8) return 9999; // Giới hạn cấp 8
        return level; // Cấp 3 cần 3, Cấp 4 cần 4...
    }

    [Header("Sprites cho từng Level (1-8)")]
    public Sprite[] levelSprites;
    
    public void SetLevel(int newLevel, int newPieces = 1)
    {
        level = newLevel;
        currentPieces = newPieces;
        UpdateUI();
    }
    
    private Image auraImage;

    public void UpdateUI()
    {
        // 1. Tắt hiển thị chữ Level đè lên ảnh
        if (levelText != null) 
        {
            levelText.gameObject.SetActive(false); 
        }
        
        // Đổi hình ảnh kiếm UI khớp với Level
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.preserveAspect = true; // Tự động căn chỉnh ảnh vừa khít
            
            if (levelSprites != null && levelSprites.Length > 0)
            {
                int index = Mathf.Clamp(level - 1, 0, levelSprites.Length - 1);
                img.sprite = levelSprites[index];
                img.color = Color.white;
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, -45f); // Xoay chéo góc 45 độ
            }
            else
            {
                float hue = (level * 0.15f) % 1f;
                img.color = Color.HSVToRGB(hue, 0.7f, 0.9f);
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, -45f);
            }
            
            // Tạo hào quang (Aura) từ Level 4 trở lên
            if (level >= 4)
            {
                if (auraImage == null)
                {
                    GameObject auraObj = new GameObject("SwordAura", typeof(RectTransform), typeof(Image));
                    auraObj.transform.SetParent(this.transform, false);
                    auraObj.transform.SetAsFirstSibling(); // Nằm lót đằng sau thanh kiếm
                    
                    RectTransform rt = auraObj.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    
                    auraImage = auraObj.GetComponent<Image>();
                }
                
                auraImage.gameObject.SetActive(true);
                auraImage.sprite = img.sprite; // Lấy đúng hình dáng kiếm
                auraImage.preserveAspect = true;
                
                // Vì aura là con của kiếm, nó tự động kế thừa góc xoay chéo của kiếm cha.
                // Đặt localRotation = identity (0 độ) để không bị xoay đúp thành nằm ngang!
                auraImage.rectTransform.localRotation = Quaternion.identity;
            }
            else
            {
                if (auraImage != null) auraImage.gameObject.SetActive(false);
            }
        }
        
        // Đổi màu nền của Ô chứa (Slot) theo cấp độ kiếm
        if (transform.parent != null && transform.parent.GetComponent<Canvas>() == null)
        {
            Image slotImg = transform.parent.GetComponent<Image>();
            if (slotImg != null)
            {
                float hue = (level * 0.15f) % 1f;
                // Màu tối sẫm ngả màu theo level để làm nổi kiếm
                slotImg.color = Color.HSVToRGB(hue, 0.6f, 0.2f); 
            }
        }
    }

    private void Update()
    {
        // Hiệu ứng lấp lánh (Aura Pulse)
        if (auraImage != null && auraImage.gameObject.activeInHierarchy)
        {
            float hue = (level * 0.15f) % 1f;
            Color auraColor = Color.HSVToRGB(hue, 1f, 1f); // Màu chói nhất
            
            // Nhấp nháy Alpha (Độ mờ) từ 0.2 đến 0.8
            float alpha = 0.2f + Mathf.PingPong(Time.time * 3f, 0.6f);
            
            // Phình to thu nhỏ nhẹ từ 1.1 đến 1.3
            float scale = 1.1f + Mathf.PingPong(Time.time * 2f, 0.2f);
            
            auraColor.a = alpha;
            auraImage.color = auraColor;
            auraImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Trả lại màu đen/xám mặc định cho ô (slot) cũ
        if (transform.parent != null)
        {
            Image slotImg = transform.parent.GetComponent<Image>();
            if (slotImg != null) slotImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        }

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
        UpdateUI(); // Cập nhật lại màu cho ô Slot mới
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Nhấn chuột phải để trang bị/tháo gỡ nhanh
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (MergeManager.Instance == null) return;
            
            // Xóa màu ô cũ trước khi bay đi
            if (transform.parent != null)
            {
                Image oldSlotImg = transform.parent.GetComponent<Image>();
                if (oldSlotImg != null) oldSlotImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            }
            
            bool isEquipped = transform.parent.name.StartsWith("Equip");
            if (!isEquipped)
            {
                // TÌM Ô EQUIP TRỐNG
                foreach (Transform slot in MergeManager.Instance.equipPanel)
                {
                    if (slot.childCount == 0)
                    {
                        transform.SetParent(slot, false);
                        rectTransform.anchoredPosition = Vector2.zero;
                        UpdateUI();
                        MergeManager.Instance.OnEquipChanged();
                        return;
                    }
                }
                
                // NẾU KHÔNG CÓ Ô TRỐNG, TÌM KIẾM CẤP THẤP NHẤT ĐỂ THAY THẾ (Nếu kiếm này cao cấp hơn)
                Transform lowestSlot = null;
                int lowestLevel = int.MaxValue;
                foreach (Transform slot in MergeManager.Instance.equipPanel)
                {
                    MergeItem equipItem = slot.GetComponentInChildren<MergeItem>();
                    if (equipItem != null && equipItem.level < lowestLevel)
                    {
                        lowestLevel = equipItem.level;
                        lowestSlot = slot;
                    }
                }
                
                if (lowestSlot != null && lowestLevel < this.level)
                {
                    MergeItem oldItem = lowestSlot.GetComponentInChildren<MergeItem>();
                    Transform myOldSlot = transform.parent; // Ô ở kho đồ
                    
                    // Đổi chỗ
                    transform.SetParent(lowestSlot, false);
                    rectTransform.anchoredPosition = Vector2.zero;
                    
                    oldItem.transform.SetParent(myOldSlot, false);
                    oldItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    
                    UpdateUI();
                    oldItem.UpdateUI();
                    MergeManager.Instance.OnEquipChanged();
                }
            }
            else
            {
                // NẾU ĐANG TRANG BỊ -> THÁO GỠ XUỐNG KHO ĐỒ
                foreach (Transform page in MergeManager.Instance.inventoryPages)
                {
                    foreach (Transform slot in page)
                    {
                        if (slot.childCount == 0)
                        {
                            transform.SetParent(slot, false);
                            rectTransform.anchoredPosition = Vector2.zero;
                            UpdateUI();
                            MergeManager.Instance.OnEquipChanged();
                            return;
                        }
                    }
                }
                Debug.LogWarning("Kho đồ đã đầy, không thể tháo kiếm xuống!");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            float dmg = FlyingSword.CalculateBaseDamage(level);
            float spd = FlyingSword.CalculateBaseSpeed(level);
            
            string info = $"<color=#00FF00>Kiếm Phôi Cấp {level}</color>\nSát thương: {dmg:F0}\nTốc độ: {spd:F0}\n\n";
            
            if (level >= 8)
            {
                info += "<color=orange>Đã đạt Cấp Tối Đa</color>\n";
            }
            else
            {
                info += $"<color=yellow>Tiến độ đột phá: {currentPieces} / {GetRequiredPieces()}</color>\n";
            }
            
            info += "\n<i>[Trái] Kéo thả để ghép\n[Phải] Trang bị nhanh</i>";
            
            TooltipManager.Instance.ShowTooltip(info);
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

using UnityEngine;
using UnityEngine.UI;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }
    
    public Transform equipPanel;
    public Transform[] inventoryPages;
    public Text pageText;
    private int currentPage = 0;
    public GameObject mergeItemPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Khởi tạo 1 cây kiếm cấp 1 mặc định vào ô Equip đầu tiên (Tránh việc kẹt game không có damage)
        if (equipPanel != null && equipPanel.childCount > 0)
        {
            Transform firstEquipSlot = equipPanel.GetChild(0);
            if (firstEquipSlot.childCount == 0 && mergeItemPrefab != null) // Nếu ô trống
            {
                GameObject newItem = Instantiate(mergeItemPrefab, firstEquipSlot);
                MergeItem item = newItem.GetComponent<MergeItem>();
                item.SetLevel(1);
            }
            
            // Cập nhật lên môi trường 3D
            OnEquipChanged();
        }
    }

    public bool TryAddSword(int level = 1)
    {
        if (inventoryPages == null || inventoryPages.Length == 0 || mergeItemPrefab == null) return false;
        
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                if (slot.childCount == 0) // Tìm thấy ô trống trong kho
                {
                    GameObject newItem = Instantiate(mergeItemPrefab, slot);
                    MergeItem item = newItem.GetComponent<MergeItem>();
                    item.SetLevel(level);
                    
                    Debug.Log($"[Merge] Rớt 1 Kiếm Phôi Lv.{level} vào kho đồ!");
                    return true;
                }
            }
        }
        return false; // Kho đồ đã đầy
    }
    
    public void NextPage()
    {
        if (inventoryPages != null && currentPage < inventoryPages.Length - 1)
        {
            inventoryPages[currentPage].gameObject.SetActive(false);
            currentPage++;
            inventoryPages[currentPage].gameObject.SetActive(true);
            UpdatePageText();
        }
    }

    public void PrevPage()
    {
        if (inventoryPages != null && currentPage > 0)
        {
            inventoryPages[currentPage].gameObject.SetActive(false);
            currentPage--;
            inventoryPages[currentPage].gameObject.SetActive(true);
            UpdatePageText();
        }
    }

    private void UpdatePageText()
    {
        if (pageText != null && inventoryPages != null)
        {
            pageText.text = $"{currentPage + 1}/{inventoryPages.Length}";
        }
    }
    
    public void SortInventory()
    {
        if (inventoryPages == null) return;

        // 1. Thu thập tất cả các phôi kiếm trong toàn bộ kho (trừ Equip)
        System.Collections.Generic.List<MergeItem> allItems = new System.Collections.Generic.List<MergeItem>();
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null)
                {
                    allItems.Add(item);
                }
            }
        }

        // 2. Sắp xếp giảm dần (Cấp độ cao nhất lên trước, Phôi nhiều lên trước)
        allItems.Sort((a, b) => {
            int levelCmp = b.level.CompareTo(a.level);
            if (levelCmp != 0) return levelCmp;
            return b.currentPieces.CompareTo(a.currentPieces);
        });

        // 3. Gắn lại vào các ô lưới theo thứ tự (dồn hết khoảng trống)
        int itemIndex = 0;
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                if (itemIndex < allItems.Count)
                {
                    allItems[itemIndex].transform.SetParent(slot);
                    allItems[itemIndex].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    itemIndex++;
                }
            }
        }
        
        Debug.Log("[Merge] Đã sắp xếp và dồn kho đồ thành công!");
    }
    
    public void OnEquipChanged()
    {
        if (equipPanel == null) return;
        
        int[] equipLevels = new int[4];
        int index = 0;
        foreach (Transform slot in equipPanel)
        {
            MergeItem item = slot.GetComponentInChildren<MergeItem>();
            equipLevels[index] = item != null ? item.level : 0;
            index++;
            if (index >= 4) break;
        }
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.UpdateEquippedSwords(equipLevels);
        }
    }
}

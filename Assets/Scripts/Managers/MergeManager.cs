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
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Kiểm tra trễ 0.5s. Nếu DataManager.LoadCurrentGame KHÔNG được gọi (do test trực tiếp Scene)
        // hoặc load bị lỗi, thì chúng ta sẽ tự tạo 1 cây kiếm mặc định để không bị kẹt game.
        Invoke(nameof(FailsafeCheckSword), 0.5f);
        
        OnEquipChanged();
    }

    private void FailsafeCheckSword()
    {
        if (equipPanel == null) return;
        
        bool hasSword = false;
        foreach (Transform equipSlot in equipPanel)
        {
            if (equipSlot.childCount > 0)
            {
                hasSword = true;
                break;
            }
        }

        if (!hasSword && mergeItemPrefab != null && equipPanel.childCount > 0)
        {
            Debug.Log("[MergeManager] Failsafe: Không tìm thấy kiếm nào sau khi load. Tự động tạo 1 phôi kiếm cấp 1.");
            Transform firstEquipSlot = equipPanel.GetChild(0);
            GameObject newItem = Instantiate(mergeItemPrefab, firstEquipSlot);
            newItem.GetComponent<MergeItem>().SetLevel(1);
            OnEquipChanged();
        }
    }


    public void QuickMerge()
    {
        if (inventoryPages == null) return;
        
        // 1. Thu thập tất cả item trong kho (không tính đang trang bị)
        var allItems = new System.Collections.Generic.List<MergeItem>();
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null) allItems.Add(item);
            }
        }

        // 2. Thống kê số lượng phôi theo từng cấp
        int[] counts = new int[9]; // Mảng đếm phôi từ level 1 -> 8
        foreach (var item in allItems)
        {
            counts[item.level] += item.currentPieces;
            item.transform.SetParent(null); // Rời khỏi slot ngay lập tức để tạo chỗ trống
            Destroy(item.gameObject); // Xóa hết item cũ (sẽ thực thi cuối frame)
        }

        // 3. Xử lý gộp từ cấp thấp lên cấp cao
        for (int L = 1; L < 8; L++)
        {
            int required = GetRequiredPiecesForLevel(L);
            int upgrades = counts[L] / required;
            counts[L] = counts[L] % required; // Phôi lẻ còn dư
            counts[L + 1] += upgrades;
        }

        // 4. Sinh lại các thanh kiếm theo số lượng đã tính toán
        for (int L = 1; L <= 8; L++)
        {
            if (counts[L] > 0)
            {
                // Vì mỗi item có thể chứa nhiều phôi (chưa đủ để lên cấp)
                // Ta chỉ cần sinh 1 item cho mỗi cấp, và gán currentPieces = counts[L]
                TryAddSwordWithPieces(L, counts[L]);
            }
        }
        
        // 5. Sắp xếp lại kho sau khi ghép
        SortInventory();
        Debug.Log("[Merge] Đã ghép nhanh tuân thủ tỷ lệ thành công!");
    }

    public void SacrificeLevel8Swords()
    {
        if (inventoryPages == null) return;
        
        int sacrificedCount = 0;
        
        // Chỉ quét kho đồ, KHÔNG quét ô trang bị
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null && item.level == 8) // Giới hạn là kiếm cấp 8
                {
                    // Mỗi 1 phôi kiếm cấp 8 = 1 điểm Kiếm Ý
                    sacrificedCount += item.currentPieces;
                    Destroy(item.gameObject);
                }
            }
        }
        
        if (sacrificedCount > 0)
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddKiemY(sacrificedCount);
            }
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"Tế Kiếm: Nhận {sacrificedCount} điểm Kiếm Ý (+{sacrificedCount * 50}% ST)!", Color.magenta);
            }
            Debug.Log($"[Sacrifice] Đã Tế {sacrificedCount} kiếm Lv8 thành công!");
        }
        else
        {
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("Không có kiếm Lv8 nào trong kho để Tế!", Color.red);
            }
        }
        
        SortInventory();
    }

    private int GetRequiredPiecesForLevel(int level)
    {
        if (level == 1) return 2;
        if (level == 2) return 2;
        if (level >= 8) return 9999;
        return level;
    }

    private void TryAddSwordWithPieces(int level, int pieces)
    {
        if (inventoryPages == null || inventoryPages.Length == 0 || mergeItemPrefab == null) return;
        
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                if (slot.childCount == 0) // Tìm thấy ô trống trong kho
                {
                    GameObject newItem = Instantiate(mergeItemPrefab, slot);
                    MergeItem item = newItem.GetComponent<MergeItem>();
                    item.SetLevel(level);
                    item.currentPieces = pieces;
                    item.UpdateUI();
                    return;
                }
            }
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

        // 1. Thu thập tất cả items
        var allItems = new System.Collections.Generic.List<MergeItem>();
        foreach (Transform page in inventoryPages)
            foreach (Transform slot in page)
            {
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null) allItems.Add(item);
            }

        // 2. Sắp xếp
        allItems.Sort((a, b) => {
            int cmp = b.level.CompareTo(a.level);
            return cmp != 0 ? cmp : b.currentPieces.CompareTo(a.currentPieces);
        });

        // 3. Tách tất cả item ra khỏi slot trước (dùng transform root tạm)
        foreach (var item in allItems)
            item.transform.SetParent(transform); // Detach tạm về MergeManager

        // 4. Dọn sạch slot (đảm bảo không còn orphan) và TẨY MÀU ô cũ
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                // Tẩy màu ô về mặc định
                Image slotImg = slot.GetComponent<Image>();
                if (slotImg != null) slotImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

                foreach (Transform child in slot)
                    Destroy(child.gameObject); // Xóa bất kỳ thứ gì còn sót
            }
        }

        // 5. Gán lại theo thứ tự
        int idx = 0;
        foreach (Transform page in inventoryPages)
        {
            foreach (Transform slot in page)
            {
                if (idx < allItems.Count)
                {
                    allItems[idx].transform.SetParent(slot);
                    allItems[idx].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    allItems[idx].UpdateUI(); // Tô lại màu cho ô mới
                    idx++;
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

    // ── SAVE / LOAD ─────────────────────────────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        var swordList = new System.Collections.Generic.List<SwordSaveEntry>();

        // 1. Quét ô trang bị (equip panel)
        if (equipPanel != null)
        {
            int eIdx = 0;
            foreach (Transform equipSlot in equipPanel)
            {
                MergeItem item = equipSlot.GetComponentInChildren<MergeItem>();
                if (item != null)
                {
                    swordList.Add(new SwordSaveEntry
                    {
                        level = item.level,
                        pieces = item.currentPieces,
                        isEquipped = true,
                        equipSlotIndex = eIdx
                    });
                }
                eIdx++;
            }
        }

        // 2. Quét kho đồ (inventory pages)
        if (inventoryPages != null)
        {
            foreach (Transform page in inventoryPages)
            {
                foreach (Transform invSlot in page)
                {
                    MergeItem item = invSlot.GetComponentInChildren<MergeItem>();
                    if (item != null)
                    {
                        swordList.Add(new SwordSaveEntry
                        {
                            level = item.level,
                            pieces = item.currentPieces,
                            isEquipped = false,
                            equipSlotIndex = -1
                        });
                    }
                }
            }
        }

        slot.swords = swordList.ToArray();
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (mergeItemPrefab == null) return;

        // 1. Xóa sạch ô trang bị
        if (equipPanel != null)
            foreach (Transform equipSlot in equipPanel)
                foreach (Transform child in equipSlot)
                    Destroy(child.gameObject);

        // 2. Xóa sạch kho đồ
        if (inventoryPages != null)
            foreach (Transform page in inventoryPages)
                foreach (Transform invSlot in page)
                    foreach (Transform child in invSlot)
                        Destroy(child.gameObject);

        if (slot.swords == null || slot.swords.Length == 0)
        {
            // Không có save kiếm → tạo kiếm mặc định cấp 1 vào ô đầu tiên
            if (equipPanel != null && equipPanel.childCount > 0)
            {
                GameObject newItem = Instantiate(mergeItemPrefab, equipPanel.GetChild(0));
                newItem.GetComponent<MergeItem>().SetLevel(1);
            }
            OnEquipChanged();
            return;
        }

        // 3. Tái tạo kiếm trang bị
        foreach (var entry in slot.swords)
        {
            if (entry.isEquipped && equipPanel != null && entry.equipSlotIndex < equipPanel.childCount)
            {
                Transform target = equipPanel.GetChild(entry.equipSlotIndex);
                GameObject obj = Instantiate(mergeItemPrefab, target);
                obj.GetComponent<MergeItem>().SetLevel(entry.level, entry.pieces);
            }
        }

        // 4. Tái tạo kiếm kho đồ
        foreach (var entry in slot.swords)
        {
            if (!entry.isEquipped)
                TryAddSwordWithPieces(entry.level, entry.pieces);
        }

        // 5. Cập nhật phi kiếm 3D
        OnEquipChanged();
    }
}


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class MergeBuilderTool
{
    [MenuItem("TuTien/Tạo hệ thống Lò Luyện (Giai đoạn 5)")]
    public static void GenerateMergeSystem()
    {
        // 1. Tìm MergePanel đã được tạo ở Giai đoạn 3 (UIBuilderTool)
        GameObject mergePanel = GameObject.Find("MergePanel");
        if (mergePanel == null)
        {
            Debug.LogError("Không tìm thấy MergePanel! Hãy chạy tạo UI trước.");
            return;
        }

        // Xóa sạch children cũ của MergePanel
        for (int i = mergePanel.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(mergePanel.transform.GetChild(i).gameObject);
        }

        // Xóa GridLayoutGroup cũ nếu có
        GridLayoutGroup oldGrid = mergePanel.GetComponent<GridLayoutGroup>();
        if (oldGrid != null) Object.DestroyImmediate(oldGrid);

        // 2. Tạo Equip Panel (Phía trên, 4 ô)
        GameObject equipPanel = new GameObject("EquipPanel", typeof(RectTransform));
        equipPanel.transform.SetParent(mergePanel.transform, false);
        SetRect(equipPanel, new Vector2(0, 0.75f), new Vector2(1, 1f), new Vector2(0.5f, 0.5f));
        
        GridLayoutGroup equipGrid = equipPanel.AddComponent<GridLayoutGroup>();
        equipGrid.cellSize = new Vector2(80, 80);
        equipGrid.spacing = new Vector2(20, 0);
        equipGrid.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < 4; i++)
        {
            GameObject slot = new GameObject($"EquipSlot_{i}", typeof(RectTransform));
            slot.transform.SetParent(equipPanel.transform, false);
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.2f, 0.4f, 0.2f, 1f); // Nền xanh nhạt phân biệt
            slot.AddComponent<MergeSlot>();
        }

        // 3. Tạo Inventory Container (Có phân trang)
        GameObject invContainer = new GameObject("InventoryContainer", typeof(RectTransform));
        invContainer.transform.SetParent(mergePanel.transform, false);
        SetRect(invContainer, new Vector2(0, 0), new Vector2(1, 0.7f), new Vector2(0.5f, 0.5f));

        GameObject pagesContainer = new GameObject("PagesContainer", typeof(RectTransform));
        pagesContainer.transform.SetParent(invContainer.transform, false);
        SetRect(pagesContainer, new Vector2(0, 0.15f), new Vector2(1, 1), new Vector2(0.5f, 0.5f));

        Transform[] pages = new Transform[20];
        for (int p = 0; p < 20; p++)
        {
            GameObject page = new GameObject($"Page_{p}", typeof(RectTransform));
            page.transform.SetParent(pagesContainer.transform, false);
            SetRect(page, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            
            GridLayoutGroup invGrid = page.AddComponent<GridLayoutGroup>();
            invGrid.cellSize = new Vector2(75, 75); // Thu nhỏ lại chút để chứa vừa 18 ô (6 cột x 3 hàng)
            invGrid.spacing = new Vector2(10, 10);
            invGrid.padding = new RectOffset(10, 10, 10, 10);
            invGrid.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < 20; i++) // Thay đổi từ 18 thành 20 ô để vuông vức màn hình (10x2)
            {
                GameObject slot = new GameObject($"InventorySlot_{p}_{i}", typeof(RectTransform));
                slot.transform.SetParent(page.transform, false);
                Image slotImg = slot.AddComponent<Image>();
                slotImg.color = new Color(0.1f, 0.1f, 0.15f, 1f); 
                slot.AddComponent<MergeSlot>();
            }
            pages[p] = page.transform;
            page.SetActive(p == 0); // Chỉ bật trang 1
        }

        // 4. Tạo Pagination Panel (Góc phải dưới)
        GameObject pagePanel = new GameObject("PaginationPanel", typeof(RectTransform));
        pagePanel.transform.SetParent(invContainer.transform, false);
        SetRect(pagePanel, new Vector2(0.6f, 0), new Vector2(1, 0.15f), new Vector2(0.5f, 0.5f)); 
        
        HorizontalLayoutGroup hg = pagePanel.AddComponent<HorizontalLayoutGroup>();
        hg.childAlignment = TextAnchor.MiddleRight;
        hg.spacing = 10;
        hg.padding = new RectOffset(0, 20, 0, 0);
        
        // Nút Sắp Xếp (Sort)
        GameObject sortBtnObj = new GameObject("SortBtn", typeof(RectTransform));
        sortBtnObj.transform.SetParent(pagePanel.transform, false);
        sortBtnObj.AddComponent<Image>().color = new Color(0.7f, 0.4f, 0.1f); // Màu cam đậm nổi bật
        Button sortBtn = sortBtnObj.AddComponent<Button>();
        CreateText(sortBtnObj.transform, "Txt", "Sắp xếp", 20);
        sortBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 40);
        
        GameObject prevBtn = new GameObject("PrevBtn", typeof(RectTransform));
        prevBtn.transform.SetParent(pagePanel.transform, false);
        prevBtn.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        Button prev = prevBtn.AddComponent<Button>();
        CreateText(prevBtn.transform, "Txt", "<-", 24);
        prevBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 40);

        GameObject pageTxtObj = new GameObject("PageText", typeof(RectTransform));
        pageTxtObj.transform.SetParent(pagePanel.transform, false);
        Text pageTxt = pageTxtObj.AddComponent<Text>();
        pageTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pageTxt.fontSize = 24;
        pageTxt.color = Color.white;
        pageTxt.alignment = TextAnchor.MiddleCenter;
        pageTxt.text = "1/20";
        pageTxtObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 40);

        GameObject nextBtn = new GameObject("NextBtn", typeof(RectTransform));
        nextBtn.transform.SetParent(pagePanel.transform, false);
        nextBtn.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        Button next = nextBtn.AddComponent<Button>();
        CreateText(nextBtn.transform, "Txt", "->", 24);
        nextBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 40);

        // 5. Tạo Prefab MergeItem (Kiếm Phôi)
        GameObject itemObj = new GameObject("MergeItemPrefab", typeof(RectTransform));
        itemObj.AddComponent<CanvasGroup>();
        Image img = itemObj.AddComponent<Image>();
        img.color = Color.white;
        
        GameObject txtObj = new GameObject("txt_Level", typeof(RectTransform));
        txtObj.transform.SetParent(itemObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = "Lv.1";
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        itemObj.AddComponent<MergeItem>();

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string mergePrefabPath = "Assets/Prefabs/MergeItem.prefab";
        GameObject mergePrefab = PrefabUtility.SaveAsPrefabAsset(itemObj, mergePrefabPath);
        Object.DestroyImmediate(itemObj);

        // 5. Biến FlyingSword trên Scene thành Prefab để CombatManager tự sinh ra
        GameObject swordInScene = GameObject.Find("FlyingSword");
        GameObject swordPrefab = null;
        if (swordInScene != null)
        {
            string swordPrefabPath = "Assets/Prefabs/FlyingSword.prefab";
            swordPrefab = PrefabUtility.SaveAsPrefabAsset(swordInScene, swordPrefabPath);
            Object.DestroyImmediate(swordInScene); // Xóa khỏi cảnh để CombatManager lo
        }
        else
        {
            swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FlyingSword.prefab");
        }

        // 6. Liên kết vào Managers
        MergeManager mergeManager = Object.FindObjectOfType<MergeManager>();
        if (mergeManager == null)
        {
            GameObject managers = GameObject.Find("Managers");
            if (managers != null) mergeManager = managers.AddComponent<MergeManager>();
        }

        if (mergeManager != null)
        {
            mergeManager.equipPanel = equipPanel.transform;
            mergeManager.inventoryPages = pages;
            mergeManager.pageText = pageTxt;
            mergeManager.mergeItemPrefab = mergePrefab;
            
            // Gán sự kiện Click cho nút sang trang và sắp xếp
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(prev.onClick, mergeManager.PrevPage);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(prev.onClick, mergeManager.PrevPage);
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(next.onClick, mergeManager.NextPage);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(next.onClick, mergeManager.NextPage);
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(sortBtn.onClick, mergeManager.SortInventory);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sortBtn.onClick, mergeManager.SortInventory);

            EditorUtility.SetDirty(mergeManager);
        }
        
        CombatManager combatManager = Object.FindObjectOfType<CombatManager>();
        if (combatManager != null && swordPrefab != null)
        {
            combatManager.swordPrefab = swordPrefab;
            EditorUtility.SetDirty(combatManager);
        }

        Debug.Log("✅ Đã tạo xong Hệ thống Kho Đồ & Ô Trang bị (Drag & Drop) và tối ưu Phi Kiếm 3D!");
    }

    private static Text CreateText(Transform parent, string name, string content, int size)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform));
        txtObj.transform.SetParent(parent, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = size;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = content;
        
        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        return txt;
    }

    private static void SetRect(GameObject obj, Vector2 min, Vector2 max, Vector2 pivot)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif

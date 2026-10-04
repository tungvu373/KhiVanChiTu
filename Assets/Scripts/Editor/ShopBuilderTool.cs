#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ShopBuilderTool
{
    [MenuItem("TuTien/Giai đoạn cuối/Tạo Shop Đan Dược")]
    public static void GenerateShopUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Tạo ShopManager nếu chưa có
        GameObject managers = GameObject.Find("Managers");
        if (managers != null && managers.GetComponent<ShopManager>() == null)
        {
            managers.AddComponent<ShopManager>();
        }

        // 1. TẠO NÚT MỞ SHOP Ở RIGHT PANEL
        GameObject rightPanel = GameObject.Find("RightPanel");
        if (rightPanel == null) return;

        GameObject btnShopObj = GameObject.Find("btn_OpenShop");
        if (btnShopObj != null) Object.DestroyImmediate(btnShopObj);

        btnShopObj = new GameObject("btn_OpenShop", typeof(RectTransform));
        btnShopObj.transform.SetParent(rightPanel.transform, false);
        RectTransform btnRt = btnShopObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1, 1);
        btnRt.anchorMax = new Vector2(1, 1);
        btnRt.pivot = new Vector2(1, 1);
        btnRt.anchoredPosition = new Vector2(-10, -50); // Góc trên cùng bên phải, dưới thanh Nav một chút
        btnRt.sizeDelta = new Vector2(120, 40);

        Button btnOpen = btnShopObj.AddComponent<Button>();
        Image btnImg = btnShopObj.AddComponent<Image>();
        btnImg.color = new Color(0.8f, 0.4f, 0.1f, 1f); // Màu cam nổi bật

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnShopObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "Tiệm Đan Dược";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 14;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        // 2. TẠO PANEL SHOP Ở GIỮA MÀN HÌNH
        GameObject shopPanel = GameObject.Find("ShopPanel");
        if (shopPanel != null) Object.DestroyImmediate(shopPanel);

        shopPanel = new GameObject("ShopPanel", typeof(RectTransform));
        shopPanel.transform.SetParent(canvas.transform, false);
        RectTransform spRt = shopPanel.GetComponent<RectTransform>();
        spRt.anchorMin = new Vector2(0.5f, 0.5f);
        spRt.anchorMax = new Vector2(0.5f, 0.5f);
        spRt.pivot = new Vector2(0.5f, 0.5f);
        spRt.anchoredPosition = Vector2.zero;
        spRt.sizeDelta = new Vector2(400, 500); // Khoảng nhỏ giữa màn hình
        
        Image spBg = shopPanel.AddComponent<Image>();
        spBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        
        // Mặc định ẩn Shop
        shopPanel.SetActive(false);

        // Gắn script mở Shop
        ShopToggleUI openToggle = btnShopObj.AddComponent<ShopToggleUI>();
        openToggle.shopPanel = shopPanel;
        openToggle.isCloseButton = false;

        // 3. TẠO SCROLL VIEW CHO SHOP
        GameObject svObj = new GameObject("ScrollView", typeof(RectTransform));
        svObj.transform.SetParent(shopPanel.transform, false);
        RectTransform svRt = svObj.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0);
        svRt.anchorMax = new Vector2(1, 1);
        svRt.offsetMin = new Vector2(10, 10);
        svRt.offsetMax = new Vector2(-10, -70);
        
        Image svBg = svObj.AddComponent<Image>();
        svBg.color = new Color(0, 0, 0, 0.2f);
        svObj.AddComponent<Mask>().showMaskGraphic = true;
        ScrollRect sr = svObj.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 20f;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(svObj.transform, false);
        RectTransform cRt = contentObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1);
        cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(0.5f, 1);
        cRt.sizeDelta = new Vector2(0, 500);

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = cRt;

        // 4. THÊM CÁC MẶT HÀNG (ĐAN DƯỢC)
        string[] itemNames = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Nộ Đan", "Tĩnh Tâm Đan" };
        string[] itemDescs = { "+5% tốc độ hồi Linh lực (15s)", "+5% tốc độ Phi kiếm (15s)", "Hồi 5% HP mỗi giây (15s)", "+5% Sát thương (15s)", "+5% Thần Thức (15s)" };
        int[] itemCosts = { 500, 500, 500, 500, 1000 };
        Color[] itemColors = { Color.blue, Color.green, Color.red, new Color(1f, 0.5f, 0f), Color.magenta };

        for (int i = 0; i < 5; i++)
        {
            CreateShopItem(contentObj.transform, i, itemNames[i], itemDescs[i], itemCosts[i], itemColors[i]);
        }

        // Tiêu đề Shop
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(shopPanel.transform, false);
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.text = "TIỆM ĐAN DƯỢC";
        titleTxt.alignment = TextAnchor.UpperCenter;
        titleTxt.color = new Color(1f, 0.8f, 0f); // Vàng gold
        titleTxt.fontSize = 24;
        titleTxt.raycastTarget = false; // QUAN TRỌNG: KHÔNG CHẶN CLICK
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 1);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.pivot = new Vector2(0.5f, 1);
        tRt.anchoredPosition = new Vector2(0, -20);
        tRt.sizeDelta = new Vector2(0, 40);

        // --- ĐỂ NÚT CLOSE Ở CUỐI CÙNG ĐỂ KHÔNG BỊ ĐÈ ---
        GameObject btnCloseObj = new GameObject("btn_Close", typeof(RectTransform));
        btnCloseObj.transform.SetParent(shopPanel.transform, false);
        RectTransform bcRt = btnCloseObj.GetComponent<RectTransform>();
        bcRt.anchorMin = new Vector2(1, 1);
        bcRt.anchorMax = new Vector2(1, 1);
        bcRt.pivot = new Vector2(1, 1);
        bcRt.anchoredPosition = new Vector2(-10, -10);
        bcRt.sizeDelta = new Vector2(40, 40);
        
        Button btnClose = btnCloseObj.AddComponent<Button>();
        Image bcImg = btnCloseObj.AddComponent<Image>();
        bcImg.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Đỏ
        
        GameObject bcTxtObj = new GameObject("Text", typeof(RectTransform));
        bcTxtObj.transform.SetParent(btnCloseObj.transform, false);
        Text bcTxt = bcTxtObj.AddComponent<Text>();
        bcTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bcTxt.text = "X";
        bcTxt.alignment = TextAnchor.MiddleCenter;
        bcTxt.color = Color.white;
        bcTxt.fontSize = 20;
        bcTxt.raycastTarget = false;
        bcTxtObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bcTxtObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bcTxtObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        
        // Gắn script đóng Shop
        ShopToggleUI closeToggle = btnCloseObj.AddComponent<ShopToggleUI>();
        closeToggle.shopPanel = shopPanel;
        closeToggle.isCloseButton = true;

        // 5. TẠO ACTIVE BUFFS PANEL Ở GÓC TRÁI (MÀN HÌNH COMBAT)
        GameObject mainCanvas = GameObject.Find("Canvas");
        if (mainCanvas != null)
        {
            GameObject abPanel = GameObject.Find("ActiveBuffPanel");
            if (abPanel != null) Object.DestroyImmediate(abPanel);

            abPanel = new GameObject("ActiveBuffPanel", typeof(RectTransform));
            abPanel.transform.SetParent(mainCanvas.transform, false);
            RectTransform abRt = abPanel.GetComponent<RectTransform>();
            abRt.anchorMin = new Vector2(0, 1);
            abRt.anchorMax = new Vector2(0, 1);
            abRt.pivot = new Vector2(0, 1);
            abRt.anchoredPosition = new Vector2(10, -50);
            abRt.sizeDelta = new Vector2(250, 300); // Tăng kích thước để chứa Progress Bar

            VerticalLayoutGroup abVlg = abPanel.AddComponent<VerticalLayoutGroup>();
            abVlg.childAlignment = TextAnchor.UpperLeft;
            abVlg.childControlHeight = false;
            abVlg.childControlWidth = true;
            abVlg.spacing = 15; // Tăng khoảng cách

            // Header
            GameObject abHeaderObj = new GameObject("Header", typeof(RectTransform));
            abHeaderObj.transform.SetParent(abPanel.transform, false);
            Text abHeader = abHeaderObj.AddComponent<Text>();
            abHeader.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            abHeader.text = "Tác Dụng Đan Dược:";
            abHeader.color = new Color(0.8f, 1f, 0.2f);
            abHeader.fontSize = 16;
            abHeader.fontStyle = FontStyle.Bold;
            abHeaderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 25);

            // Tạo sẵn 5 dòng UI Đếm ngược (Background + Fill Image + Text)
            for (int i = 0; i < 5; i++)
            {
                GameObject rowObj = new GameObject($"BuffRow_{i}", typeof(RectTransform));
                rowObj.transform.SetParent(abPanel.transform, false);
                RectTransform rRt = rowObj.GetComponent<RectTransform>();
                rRt.sizeDelta = new Vector2(0, 30); // Chiều cao mỗi thanh đếm ngược

                // Nền đen của thanh đếm ngược
                GameObject bgBarObj = new GameObject("BgBar", typeof(RectTransform));
                bgBarObj.transform.SetParent(rowObj.transform, false);
                Image bgBar = bgBarObj.AddComponent<Image>();
                bgBar.color = new Color(0, 0, 0, 0.5f);
                RectTransform bbgRt = bgBarObj.GetComponent<RectTransform>();
                bbgRt.anchorMin = new Vector2(0, 0);
                bbgRt.anchorMax = new Vector2(1, 1);
                bbgRt.offsetMin = Vector2.zero;
                bbgRt.offsetMax = Vector2.zero;

                // Thanh Image đếm ngược (Màu của đan dược)
                GameObject fillBarObj = new GameObject("FillBar", typeof(RectTransform));
                fillBarObj.transform.SetParent(bgBarObj.transform, false);
                Image fillBar = fillBarObj.AddComponent<Image>();
                fillBar.color = itemColors[i];
                RectTransform fbRt = fillBarObj.GetComponent<RectTransform>();
                fbRt.anchorMin = new Vector2(0, 0);
                fbRt.anchorMax = new Vector2(1, 1); // Thay đổi anchorMax.x trong logic để đếm ngược
                fbRt.offsetMin = Vector2.zero;
                fbRt.offsetMax = Vector2.zero;

                // Text hiển thị
                GameObject buffTxtObj = new GameObject("Text", typeof(RectTransform));
                buffTxtObj.transform.SetParent(rowObj.transform, false);
                Text bTxt = buffTxtObj.AddComponent<Text>();
                bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                bTxt.text = "";
                bTxt.color = Color.white;
                bTxt.fontSize = 14;
                bTxt.fontStyle = FontStyle.Bold;
                bTxt.alignment = TextAnchor.MiddleCenter;
                
                // Viền chữ để dễ đọc
                Outline outline = buffTxtObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
                
                RectTransform buffTxtRt = buffTxtObj.GetComponent<RectTransform>();
                buffTxtRt.anchorMin = new Vector2(0, 0);
                buffTxtRt.anchorMax = new Vector2(1, 1);
                buffTxtRt.offsetMin = Vector2.zero;
                buffTxtRt.offsetMax = Vector2.zero;
                
                rowObj.SetActive(false); // Ẩn mặc định
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Đã tạo UI Shop Đan Dược thành công!");
    }

    private static void CreateShopItem(Transform parent, int index, string name, string desc, int cost, Color iconColor)
    {
        GameObject item = new GameObject($"ShopItem_{index}", typeof(RectTransform));
        item.transform.SetParent(parent, false);
        RectTransform iRt = item.GetComponent<RectTransform>();
        iRt.sizeDelta = new Vector2(0, 80);
        
        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Icon
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
        iconObj.transform.SetParent(item.transform, false);
        Image icon = iconObj.AddComponent<Image>();
        icon.color = iconColor;
        RectTransform icRt = iconObj.GetComponent<RectTransform>();
        icRt.anchorMin = new Vector2(0, 0.5f);
        icRt.anchorMax = new Vector2(0, 0.5f);
        icRt.pivot = new Vector2(0, 0.5f);
        icRt.anchoredPosition = new Vector2(40, 0);
        icRt.sizeDelta = new Vector2(50, 50);

        // Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(item.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.text = name;
        title.color = Color.white;
        title.fontSize = 16;
        title.fontStyle = FontStyle.Bold;
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 1);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.pivot = new Vector2(0, 1);
        tRt.anchoredPosition = new Vector2(80, -10);
        tRt.sizeDelta = new Vector2(-180, 25);

        // Desc
        GameObject descObj = new GameObject("Desc", typeof(RectTransform));
        descObj.transform.SetParent(item.transform, false);
        Text descTxt = descObj.AddComponent<Text>();
        descTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descTxt.text = desc;
        descTxt.color = Color.gray;
        descTxt.fontSize = 12;
        RectTransform dRt = descObj.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0, 0);
        dRt.anchorMax = new Vector2(1, 0);
        dRt.pivot = new Vector2(0, 0);
        dRt.anchoredPosition = new Vector2(80, 10);
        dRt.sizeDelta = new Vector2(-180, 35);

        // Buy Button
        GameObject btnObj = new GameObject("BuyButton", typeof(RectTransform));
        btnObj.transform.SetParent(item.transform, false);
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f); // Xanh lục
        RectTransform bRt = btnObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(1, 0.5f);
        bRt.anchorMax = new Vector2(1, 0.5f);
        bRt.pivot = new Vector2(1, 0.5f);
        bRt.anchoredPosition = new Vector2(-10, 0);
        bRt.sizeDelta = new Vector2(80, 40);

        GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        Text btnTxt = btnTxtObj.AddComponent<Text>();
        btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnTxt.text = $"{cost} LT";
        btnTxt.alignment = TextAnchor.MiddleCenter;
        btnTxt.color = Color.white;
        btnTxt.fontSize = 14;
        btnTxtObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        btnTxtObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        btnTxtObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Logic
        ShopItemUI uiScript = item.AddComponent<ShopItemUI>();
        uiScript.itemIndex = index;
        uiScript.buyButton = btn;
    }
}
#endif

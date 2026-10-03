#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class DashboardBuilderTool
{
    [MenuItem("TuTien/Giai đoạn 7/2. Tái tạo Dashboard (UI)")]
    public static void GenerateNewDashboard()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        
        // 1. THANH STAGE PROGRESS (TOP CENTER)
        GameObject stagePanel = GameObject.Find("StagePanel");
        if (stagePanel != null) Object.DestroyImmediate(stagePanel);
        
        stagePanel = new GameObject("StagePanel", typeof(RectTransform));
        stagePanel.transform.SetParent(canvas.transform, false);
        RectTransform spRt = stagePanel.GetComponent<RectTransform>();
        spRt.anchorMin = new Vector2(0.5f, 1);
        spRt.anchorMax = new Vector2(0.5f, 1);
        spRt.pivot = new Vector2(0.5f, 1);
        spRt.anchoredPosition = new Vector2(0, -30);
        spRt.sizeDelta = new Vector2(500, 40);
        
        Image bg = stagePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        GameObject stageTxtObj = new GameObject("StageText", typeof(RectTransform));
        stageTxtObj.transform.SetParent(stagePanel.transform, false);
        Text txt = stageTxtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "Cảnh Giới: Luyện Khí - Tiến độ: 0%";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.yellow;
        txt.fontSize = 22;
        RectTransform txtRt = stageTxtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        
        // 2. KHUNG NHÂN VẬT (BOTTOM LEFT)
        GameObject charPanel = GameObject.Find("CharacterFrame");
        if (charPanel != null) Object.DestroyImmediate(charPanel);
        
        charPanel = new GameObject("CharacterFrame", typeof(RectTransform));
        charPanel.transform.SetParent(canvas.transform, false);
        RectTransform cpRt = charPanel.GetComponent<RectTransform>();
        cpRt.anchorMin = new Vector2(0, 0);
        cpRt.anchorMax = new Vector2(0, 0);
        cpRt.pivot = new Vector2(0, 0);
        cpRt.anchoredPosition = new Vector2(10, 10);
        cpRt.sizeDelta = new Vector2(300, 120);
        
        Image cbg = charPanel.AddComponent<Image>();
        cbg.color = new Color(0.1f, 0.2f, 0.3f, 0.9f);
        
        GameObject avaObj = new GameObject("Avatar", typeof(RectTransform));
        avaObj.transform.SetParent(charPanel.transform, false);
        Image ava = avaObj.AddComponent<Image>();
        ava.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        RectTransform avaRt = avaObj.GetComponent<RectTransform>();
        avaRt.anchorMin = new Vector2(0, 0.5f);
        avaRt.anchorMax = new Vector2(0, 0.5f);
        avaRt.pivot = new Vector2(0, 0.5f);
        avaRt.anchoredPosition = new Vector2(10, 0);
        avaRt.sizeDelta = new Vector2(80, 100);
        
        GameObject infoObj = new GameObject("InfoText", typeof(RectTransform));
        infoObj.transform.SetParent(charPanel.transform, false);
        Text infoTxt = infoObj.AddComponent<Text>();
        infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        infoTxt.text = "Tên: Đạo Hữu\nCảnh giới: Luyện Khí\nTu Vi: 0/100\nLoại: Nhân Tộc";
        infoTxt.color = Color.white;
        infoTxt.fontSize = 18;
        RectTransform infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 0);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.offsetMin = new Vector2(100, 10);
        infoRt.offsetMax = new Vector2(-10, -10);

        // 3. TẠO SCROLL VIEW CHUẨN CHO UPGRADE PANEL
        GameObject rightPanel = GameObject.Find("RightPanel");
        if (rightPanel != null)
        {
            // Dọn dẹp lỗi cũ nếu lỡ gắn ScrollRect vào RightPanel
            ScrollRect oldSr = rightPanel.GetComponent<ScrollRect>();
            if (oldSr != null) Object.DestroyImmediate(oldSr);
            Mask oldMask = rightPanel.GetComponent<Mask>();
            if (oldMask != null) Object.DestroyImmediate(oldMask);

            GameObject svObj = GameObject.Find("UpgradeScrollView");
            if (svObj != null) Object.DestroyImmediate(svObj);
            
            // Tạo Vùng cuộn chiếm đúng nửa dưới bên phải
            svObj = new GameObject("UpgradeScrollView", typeof(RectTransform));
            svObj.transform.SetParent(rightPanel.transform, false);
            RectTransform svRt = svObj.GetComponent<RectTransform>();
            svRt.anchorMin = new Vector2(0, 0);
            svRt.anchorMax = new Vector2(1, 0.5f);
            svRt.offsetMin = Vector2.zero;
            svRt.offsetMax = Vector2.zero;
            
            svObj.AddComponent<Image>().color = new Color(0,0,0,0.1f);
            svObj.AddComponent<Mask>();
            ScrollRect sr = svObj.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 15f; // Chậm lại theo yêu cầu

            // Tạo thanh Scrollbar hiển thị bên phải
            GameObject scrollbarObj = GameObject.Find("UpgradeScrollbar");
            if (scrollbarObj != null) Object.DestroyImmediate(scrollbarObj);
            
            scrollbarObj = new GameObject("UpgradeScrollbar", typeof(RectTransform));
            scrollbarObj.transform.SetParent(rightPanel.transform, false);
            RectTransform sbRt = scrollbarObj.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1, 0);
            sbRt.anchorMax = new Vector2(1, 0.5f); // Bằng chiều cao của ScrollView
            sbRt.pivot = new Vector2(1, 0);
            sbRt.sizeDelta = new Vector2(15, 0);
            sbRt.anchoredPosition = Vector2.zero;

            Image sbBg = scrollbarObj.AddComponent<Image>();
            sbBg.color = new Color(0, 0, 0, 0.3f);

            GameObject handleObj = new GameObject("Handle", typeof(RectTransform));
            handleObj.transform.SetParent(scrollbarObj.transform, false);
            RectTransform hRt = handleObj.GetComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.sizeDelta = new Vector2(-4, -4);

            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.color = new Color(1, 1, 1, 0.5f);

            Scrollbar sb = scrollbarObj.AddComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.handleRect = hRt;
            sb.targetGraphic = handleImg;

            // Gắn thanh cuộn vào logic
            sr.verticalScrollbar = sb;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            // Lôi UpgradePanel nhét vào trong ScrollView
            GameObject upgradePanel = GameObject.Find("UpgradePanel");
            if (upgradePanel != null)
            {
                upgradePanel.transform.SetParent(svObj.transform, false);
                RectTransform upRt = upgradePanel.GetComponent<RectTransform>();
                
                // QUAN TRỌNG: Phải set Anchor là TOP-Stretch thì ContentSizeFitter mới hoạt động để bung chiều cao
                upRt.anchorMin = new Vector2(0, 1);
                upRt.anchorMax = new Vector2(1, 1);
                upRt.pivot = new Vector2(0.5f, 1);
                upRt.offsetMin = Vector2.zero;
                upRt.offsetMax = Vector2.zero;
                
                ContentSizeFitter csf = upgradePanel.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = upgradePanel.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                
                sr.content = upRt;
            }
        }
        
        // 4. KẾT NỐI VÀO CULTIVATION MANAGER ĐỂ HIỂN THỊ DATA
        CultivationManager cult = Object.FindObjectOfType<CultivationManager>();
        if (cult != null)
        {
            cult.stageText = txt;
            cult.characterInfoText = infoTxt;
            EditorUtility.SetDirty(cult);
        }
        
        Debug.Log("✅ Đã tạo xong Khung Nhân Vật & Thanh Stage! Bọc Scroll View thành công.");
    }
}
#endif

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SkillUIBuilderTool
{
    [MenuItem("TuTien/Giai đoạn 7/3. Thêm Thẻ Kỹ Năng vào UI")]
    public static void GenerateSkillUI()
    {
        // TÌM UPGRADE PANEL
        GameObject upgradePanel = GameObject.Find("UpgradePanel");
        if (upgradePanel == null) 
        {
            Debug.LogError("Không tìm thấy UpgradePanel! Bạn đã chạy Tool tạo giao diện Giai đoạn 3 chưa?");
            return;
        }

        UpgradeUIPlaceholder uiPlaceholder = Object.FindObjectOfType<UpgradeUIPlaceholder>();
        if (uiPlaceholder == null) 
        {
            Debug.LogError("LỖI: Không tìm thấy Component UpgradeUIPlaceholder trong Scene!");
            return;
        }
        
        // DỌN SẠCH TÀN DƯ CŨ TRONG BẢNG (để tránh tạo trùng lặp khi ấn nhiều lần)
        for (int i = upgradePanel.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(upgradePanel.transform.GetChild(i).gameObject);
        }

        // ĐẢM BẢO GRID LAYOUT VÀ CONTENT SIZE FITTER CÓ SẴN TRÊN UPGRADE PANEL
        GridLayoutGroup grid = upgradePanel.GetComponent<GridLayoutGroup>();
        if (grid == null) 
        {
            grid = upgradePanel.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(380, 150); // Kích thước thẻ ngang to
            grid.spacing = new Vector2(20, 20);
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
        }

        ContentSizeFitter csf = upgradePanel.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = upgradePanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Lưu ý: KHÔNG TẠO THÊM SCROLLRECT MỚI TRÊN ĐÂY. 
        // Vì Giai đoạn 7 - Mục 2 (DashboardBuilderTool) đã bọc UpgradePanel này vào trong UpgradeScrollView rồi!
        
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- BƯỚC 1: KHÔI PHỤC LẠI 4 THẺ CHỈ SỐ CƠ BẢN ---
        CreateUpgradeCard(upgradePanel.transform, "LinhLuc", defaultFont, ref uiPlaceholder.txtLinhLuc, ref uiPlaceholder.btnLinhLuc);
        CreateUpgradeCard(upgradePanel.transform, "KiemY", defaultFont, ref uiPlaceholder.txtKiemY, ref uiPlaceholder.btnKiemY);
        CreateUpgradeCard(upgradePanel.transform, "ThanThuc", defaultFont, ref uiPlaceholder.txtThanThuc, ref uiPlaceholder.btnThanThuc);
        CreateUpgradeCard(upgradePanel.transform, "TuLinh", defaultFont, ref uiPlaceholder.txtTuLinh, ref uiPlaceholder.btnTuLinh);
        
        // --- BƯỚC 2: TẠO 4 THẺ KỸ NĂNG ---
        string[] skillNames = { "Vạn Kiếm Quy Tông", "Ấn Chưởng", "Lôi Phạt", "Phân Thân" };
        Color[] colors = { Color.yellow, Color.cyan, Color.magenta, Color.green };

        for (int i = 0; i < 4; i++)
        {
            GameObject card = new GameObject("SkillCard_" + i, typeof(RectTransform));
            card.transform.SetParent(upgradePanel.transform, false);
            
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(card.transform, false);
            Image icon = iconObj.AddComponent<Image>();
            icon.color = colors[i];
            RectTransform iRt = iconObj.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 1);
            iRt.anchorMax = new Vector2(0.5f, 1);
            iRt.pivot = new Vector2(0.5f, 1);
            iRt.anchoredPosition = new Vector2(0, -10);
            iRt.sizeDelta = new Vector2(40, 40); 
            
            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(card.transform, false);
            Text title = titleObj.AddComponent<Text>();
            title.font = defaultFont;
            title.text = skillNames[i];
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.fontSize = 14;
            RectTransform tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, 0);
            tRt.anchorMax = new Vector2(1, 0);
            tRt.offsetMin = new Vector2(0, 50); 
            tRt.offsetMax = new Vector2(0, 80);

            GameObject condObj = new GameObject("Condition", typeof(RectTransform));
            condObj.transform.SetParent(card.transform, false);
            Text cond = condObj.AddComponent<Text>();
            cond.font = defaultFont;
            cond.text = "Đang tải..."; 
            cond.alignment = TextAnchor.MiddleCenter;
            cond.color = Color.red;
            cond.fontSize = 12;
            RectTransform cRt = condObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 0);
            cRt.anchorMax = new Vector2(1, 0);
            cRt.offsetMin = new Vector2(0, 30);
            cRt.offsetMax = new Vector2(0, 50);

            GameObject btnObj = new GameObject("UnlockButton", typeof(RectTransform));
            btnObj.transform.SetParent(card.transform, false);
            Button btn = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.8f, 0.6f, 0.1f);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.1f, 0);
            btnRt.anchorMax = new Vector2(0.9f, 0);
            btnRt.offsetMin = new Vector2(0, 5);
            btnRt.offsetMax = new Vector2(0, 25); 

            GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform));
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            Text btnTxt = btnTxtObj.AddComponent<Text>();
            btnTxt.font = defaultFont;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            btnTxt.fontSize = 12;
            RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
            btnTxtRt.anchorMin = Vector2.zero;
            btnTxtRt.anchorMax = Vector2.one;
            btnTxtRt.sizeDelta = Vector2.zero;

            SkillCardUI scUI = card.AddComponent<SkillCardUI>();
            scUI.skillIndex = i;
            scUI.titleText = title;
            scUI.condText = cond;
            scUI.unlockButton = btn;
            scUI.unlockBtnText = btnTxt;
            
            btn.onClick.AddListener(scUI.OnUnlockClicked);
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Đã THÊM THÀNH CÔNG 8 Thẻ Nâng Cấp (4 Chỉ số + 4 Kỹ năng) vào đúng Grid cũ! Scroll sẽ hoạt động hoàn hảo.");
    }

    private static void CreateUpgradeCard(Transform parent, string name, Font font, ref Text outText, ref Button outBtn)
    {
        GameObject card = new GameObject($"Card_{name}");
        card.transform.SetParent(parent, false);
        card.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 1f); 

        GameObject txtObj = new GameObject("Info", typeof(RectTransform));
        txtObj.transform.SetParent(card.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = 14; 
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperCenter;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0, 0.45f);
        txtRt.anchorMax = new Vector2(1, 1);
        txtRt.pivot = new Vector2(0.5f, 1);
        txtRt.anchoredPosition = new Vector2(0, -5);
        txtRt.sizeDelta = new Vector2(0, -10);
        outText = txt;

        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = "Button";
        btnObj.transform.SetParent(card.transform, false);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.1f, 0.1f);
        btnRt.anchorMax = new Vector2(0.9f, 0.45f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = Vector2.zero;
        btnRt.sizeDelta = Vector2.zero;
        outBtn = btnObj.GetComponent<Button>();
        outBtn.GetComponentInChildren<Text>().font = font;
        outBtn.GetComponentInChildren<Text>().fontSize = 14;
    }
}
#endif

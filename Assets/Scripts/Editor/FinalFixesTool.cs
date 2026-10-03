#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FinalFixesTool
{
    [MenuItem("TuTien/Giai đoạn 9 (Hoàn thiện)/3. Vá lỗi vệt kéo thả và Giao diện")]
    public static void ApplyFixes()
    {
        // 1. Vá lỗi lưu vệt khi kéo item (Thêm nền đen đặc phía sau Canvas)
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform bgCheck = canvas.transform.Find("FullBackground");
            if (bgCheck == null)
            {
                GameObject bgObj = new GameObject("FullBackground", typeof(RectTransform));
                bgObj.transform.SetParent(canvas.transform, false);
                bgObj.transform.SetAsFirstSibling(); // Cho xuống dưới cùng
                
                Image bgImg = bgObj.AddComponent<Image>();
                bgImg.color = new Color(0.02f, 0.02f, 0.03f, 1f); // Đen tuyền
                bgImg.raycastTarget = false; // Không cản trở click
                
                RectTransform bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.anchorMin = new Vector2(0.5f, 0); // Chỉ che nửa bên phải màn hình UI
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
            }
            else
            {
                // Nếu đã tạo rồi thì sửa lại
                RectTransform bgRt = bgCheck.GetComponent<RectTransform>();
                bgRt.anchorMin = new Vector2(0.5f, 0); 
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
            }
        }
        
        // 2. Thêm Text hiển thị Cơ Duyên vào góc trên bên trái cùng với Linh Thạch
        GameObject ltObj = GameObject.Find("txt_LinhThach");
        if (ltObj != null)
        {
            Transform cdCheck = ltObj.transform.parent.Find("txt_CoDuyen");
            if (cdCheck == null)
            {
                GameObject cdObj = GameObject.Instantiate(ltObj, ltObj.transform.parent);
                cdObj.name = "txt_CoDuyen";
                
                RectTransform cdRt = cdObj.GetComponent<RectTransform>();
                cdRt.anchoredPosition = new Vector2(cdRt.anchoredPosition.x + 250, cdRt.anchoredPosition.y); // Dịch sang phải
                
                Text cdTxt = cdObj.GetComponent<Text>();
                cdTxt.text = "Cơ Duyên: 0";
                cdTxt.color = new Color(1f, 0.8f, 0f); // Màu vàng
                
                // Viết 1 script nhỏ để tự update Cơ Duyên
                CoDuyenUIUpdater cdUpdater = cdObj.AddComponent<CoDuyenUIUpdater>();
                cdUpdater.txt = cdTxt;
            }
        }

        Debug.Log("✅ Đã vá lỗi lưu vệt thành công và thêm hiển thị Cơ Duyên!");
    }

    [MenuItem("TuTien/Giai đoạn 9 (Hoàn thiện)/4. Tái tạo Toàn bộ Bảng Nâng Cấp (Mới 100%)")]
    public static void RebuildUpgradePanel_BrandNew()
    {
        // 1. Tìm RightPanel
        GameObject rightPanel = GameObject.Find("RightPanel");
        if (rightPanel == null)
        {
            Debug.LogError("Không tìm thấy RightPanel!");
            return;
        }

        // 2. Xóa sạch UpgradePanel cũ đi
        GameObject oldPanel = GameObject.Find("UpgradePanel");
        if (oldPanel != null)
        {
            Object.DestroyImmediate(oldPanel);
        }

        GameObject gm = GameObject.Find("GameManager");
        if (gm == null) return;
        UpgradeUIPlaceholder uiPlaceholder = gm.GetComponent<UpgradeUIPlaceholder>();
        if (uiPlaceholder == null) return;

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 3. Tạo mới UpgradePanel (dạng Scroll View hoàn chỉnh)
        GameObject scrollObj = new GameObject("UpgradePanel", typeof(RectTransform));
        scrollObj.transform.SetParent(rightPanel.transform, false);
        
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = Vector2.zero;
        scrollRt.sizeDelta = Vector2.zero;

        Image bgMask = scrollObj.AddComponent<Image>();
        bgMask.color = new Color(0, 0, 0, 0.2f);
        scrollObj.AddComponent<Mask>().showMaskGraphic = true;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        // 4. Tạo Viewport và Content
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(1,1,1,0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 450); // Chiều cao tạm

        scroll.content = contentRt;
        scroll.viewport = vpRt;

        // 5. Cài đặt Grid Layout
        GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(380, 150);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        // 6. Tạo 4 Thẻ Cơ Bản
        CreateUpgradeCard(content.transform, "LinhLuc", defaultFont, ref uiPlaceholder.txtLinhLuc, ref uiPlaceholder.btnLinhLuc);
        CreateUpgradeCard(content.transform, "KiemY", defaultFont, ref uiPlaceholder.txtKiemY, ref uiPlaceholder.btnKiemY);
        CreateUpgradeCard(content.transform, "ThanThuc", defaultFont, ref uiPlaceholder.txtThanThuc, ref uiPlaceholder.btnThanThuc);
        CreateUpgradeCard(content.transform, "TuLinh", defaultFont, ref uiPlaceholder.txtTuLinh, ref uiPlaceholder.btnTuLinh);

        // 7. Tạo 4 Thẻ Skill
        string[] skillNames = { "Vạn Kiếm Quy Tông", "Ấn Chưởng", "Lôi Phạt", "Phân Thân" };
        Color[] colors = { Color.yellow, Color.cyan, Color.magenta, Color.green };

        for (int i = 0; i < 4; i++)
        {
            GameObject card = new GameObject("SkillCard_" + i, typeof(RectTransform));
            card.transform.SetParent(content.transform, false);
            
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
            tRt.offsetMin = new Vector2(0, 40);
            tRt.offsetMax = new Vector2(0, 70);

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
        Debug.Log("✅ Đã đập đi xây lại mới tinh toàn bộ khu vực Bảng Nâng Cấp (8 Thẻ + Kéo cuộn mượt mà)!");
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

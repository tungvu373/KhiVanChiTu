#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIBuilderTool
{
    [MenuItem("TuTien/Tự động tạo UI theo GDD (Hoàn chỉnh)")]
    public static void GenerateUI()
    {
        // 1. Tạo Managers
        GameObject managers = GameObject.Find("Managers");
        if (managers == null) managers = new GameObject("Managers");
        
        if (!managers.GetComponent<GameManager>()) managers.AddComponent<GameManager>();
        if (!managers.GetComponent<EconomyManager>()) managers.AddComponent<EconomyManager>();
        
        CultivationManager cultivationManager = managers.GetComponent<CultivationManager>();
        if (cultivationManager == null) cultivationManager = managers.AddComponent<CultivationManager>();
        
        if (!managers.GetComponent<GameLogger>()) managers.AddComponent<GameLogger>();
        
        // Load Stages
        string[] stageGuids = AssetDatabase.FindAssets("t:CultivationStageData", new[] { "Assets/ScriptableObjects/Stages" });
        System.Collections.Generic.List<CultivationStageData> stages = new System.Collections.Generic.List<CultivationStageData>();
        foreach (string guid in stageGuids)
        {
            stages.Add(AssetDatabase.LoadAssetAtPath<CultivationStageData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        stages.Sort((a, b) => a.name.CompareTo(b.name));
        cultivationManager.allStages = stages.ToArray();

        if (!managers.GetComponent<SkillManager>()) managers.AddComponent<SkillManager>();
        
        UpgradeManager upgradeManager = managers.GetComponent<UpgradeManager>();
        if (upgradeManager == null) upgradeManager = managers.AddComponent<UpgradeManager>();
        
        upgradeManager.linhLucData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/LinhLuc.asset");
        upgradeManager.kiemYData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/KiemY.asset");
        upgradeManager.thanThucData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/ThanThuc.asset");
        upgradeManager.tuLinhData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/TuLinh.asset");

        // 2. Tạo Canvas
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) Object.DestroyImmediate(oldEventSystem);
        
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // 3. UIManager & Placeholder
        GameObject uiManagerObj = new GameObject("UIManager");
        uiManagerObj.transform.SetParent(canvasObj.transform, false);
        UpgradeUIPlaceholder uiPlaceholder = uiManagerObj.AddComponent<UpgradeUIPlaceholder>();
        CultivationUI cultUI = uiManagerObj.AddComponent<CultivationUI>();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- TOP BAR (Thanh Tài Nguyên) ---
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvasObj.transform, false);
        topBar.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        RectTransform rtTop = topBar.GetComponent<RectTransform>();
        rtTop.anchorMin = new Vector2(0, 1);
        rtTop.anchorMax = new Vector2(1, 1);
        rtTop.pivot = new Vector2(0.5f, 1);
        rtTop.anchoredPosition = Vector2.zero;
        rtTop.sizeDelta = new Vector2(0, 100);

        GameObject txtLTObj = CreateText(topBar.transform, "txt_LinhThach", defaultFont, 36, Color.yellow, TextAnchor.MiddleLeft);
        SetRect(txtLTObj, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(150, 0), new Vector2(400, 80));
        uiPlaceholder.txtLinhThach = txtLTObj.GetComponent<Text>();

        GameObject txtStageObj = CreateText(topBar.transform, "txt_StageName", defaultFont, 36, Color.cyan, TextAnchor.MiddleCenter);
        SetRect(txtStageObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 50));
        cultUI.txtStageName = txtStageObj.GetComponent<Text>();

        GameObject txtTuViObj = CreateText(topBar.transform, "txt_TuVi", defaultFont, 24, Color.white, TextAnchor.MiddleCenter);
        SetRect(txtTuViObj, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 25), new Vector2(600, 40));
        cultUI.txtTuVi = txtTuViObj.GetComponent<Text>();

        // --- RIGHT PANEL (Trạm Nâng Cấp & Lò Luyện) ---
        GameObject rightPanel = new GameObject("RightPanel");
        rightPanel.transform.SetParent(canvasObj.transform, false);
        rightPanel.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        RectTransform rtRight = rightPanel.GetComponent<RectTransform>();
        rtRight.anchorMin = new Vector2(0.55f, 0); // 45% màn hình như GDD
        rtRight.anchorMax = new Vector2(1, 1);
        rtRight.pivot = new Vector2(1, 0.5f);
        rtRight.anchoredPosition = new Vector2(0, -50); 
        rtRight.sizeDelta = new Vector2(0, -100);

        // Nửa trên: Lò Luyện Placeholder
        GameObject mergePanel = new GameObject("MergePanel");
        mergePanel.transform.SetParent(rightPanel.transform, false);
        mergePanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);
        SetRect(mergePanel, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        CreateText(mergePanel.transform, "MergeTitle", defaultFont, 40, Color.gray, TextAnchor.MiddleCenter).GetComponent<Text>().text = "Lò Luyện (Grid 4x4)\n[Đang xây dựng]";

        // Nửa dưới: Thẻ Nâng cấp
        GameObject upgradePanel = new GameObject("UpgradePanel", typeof(RectTransform));
        upgradePanel.transform.SetParent(rightPanel.transform, false);
        SetRect(upgradePanel, new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        GridLayoutGroup grid = upgradePanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(380, 180); // Thu nhỏ lại một chút để hiển thị đủ 4 nút
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.childAlignment = TextAnchor.MiddleCenter;

        CreateUpgradeCard(upgradePanel.transform, "LinhLuc", defaultFont, ref uiPlaceholder.txtLinhLuc, ref uiPlaceholder.btnLinhLuc);
        CreateUpgradeCard(upgradePanel.transform, "KiemY", defaultFont, ref uiPlaceholder.txtKiemY, ref uiPlaceholder.btnKiemY);
        CreateUpgradeCard(upgradePanel.transform, "ThanThuc", defaultFont, ref uiPlaceholder.txtThanThuc, ref uiPlaceholder.btnThanThuc);
        CreateUpgradeCard(upgradePanel.transform, "TuLinh", defaultFont, ref uiPlaceholder.txtTuLinh, ref uiPlaceholder.btnTuLinh);

        // --- CHEAT BUTTONS ---
        GameObject cheatPanel = new GameObject("CheatPanel", typeof(RectTransform));
        cheatPanel.transform.SetParent(canvasObj.transform, false);
        VerticalLayoutGroup cheatLayout = cheatPanel.AddComponent<VerticalLayoutGroup>();
        cheatLayout.spacing = 10;
        SetRect(cheatPanel, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(120, 150), new Vector2(200, 100));

        Button btnAddTuVi = CreateSimpleButton(cheatPanel.transform, "btn_CheatTuVi", defaultFont, "Cheat: +50 Tu Vi");
        btnAddTuVi.onClick.AddListener(() => { if (CultivationManager.Instance != null) CultivationManager.Instance.AddTuVi(50); });

        Button btnBoss = CreateSimpleButton(cheatPanel.transform, "btn_CheatBoss", defaultFont, "Cheat: Đánh Boss");
        btnBoss.onClick.AddListener(() => 
        { 
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation)
                if (CultivationManager.Instance != null) CultivationManager.Instance.Breakthrough();
        });
        
        // 4. Dịch chuyển Camera sang PHẢI để phần 3D (X=0) nằm gọn bên TRÁI
        if (Camera.main != null) Camera.main.transform.position = new Vector3(4.5f, 12f, -10f);

        Debug.Log("✅ Đã thiết lập xong Bố cục chuẩn GDD: Single-screen Layout!");
    }

    private static GameObject CreateText(Transform parent, string name, Font font, int size, Color color, TextAnchor align)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = align;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        return obj;
    }

    private static void SetRect(GameObject obj, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void CreateUpgradeCard(Transform parent, string name, Font font, ref Text outText, ref Button outBtn)
    {
        GameObject card = new GameObject($"Card_{name}");
        card.transform.SetParent(parent, false);
        card.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 1f); 

        GameObject txtObj = CreateText(card.transform, "Info", font, 24, Color.white, TextAnchor.UpperCenter);
        SetRect(txtObj, new Vector2(0, 0.45f), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -15), new Vector2(0, -30));
        outText = txtObj.GetComponent<Text>();

        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = "Button";
        btnObj.transform.SetParent(card.transform, false);
        SetRect(btnObj, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        outBtn = btnObj.GetComponent<Button>();
        outBtn.GetComponentInChildren<Text>().font = font;
        outBtn.GetComponentInChildren<Text>().fontSize = 28;
    }

    private static Button CreateSimpleButton(Transform parent, string name, Font font, string text)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = name;
        btnObj.transform.SetParent(parent, false);
        Button btn = btnObj.GetComponent<Button>();
        Text txt = btn.GetComponentInChildren<Text>();
        txt.font = font;
        txt.text = text;
        txt.fontSize = 20;
        return btn;
    }
}
#endif

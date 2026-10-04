using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    private GameObject logPanel;
    private List<GameObject> activeLogs = new List<GameObject>();
    private const int MAX_LOGS = 6;
    private Font defaultFont;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void Start()
    {
        CreateLogUI();
    }

    private void CreateLogUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // Xóa log cũ nếu có
        Transform oldLog = canvas.transform.Find("CombatLogPanel");
        if (oldLog != null) Destroy(oldLog.gameObject);

        // Tạo Panel (góc dưới bên phải của màn hình Combat - tức là X=0.55)
        logPanel = new GameObject("CombatLogPanel", typeof(RectTransform));
        logPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = logPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.54f, 0); // Sát viền RightPanel
        rt.anchorMax = new Vector2(0.54f, 0);
        rt.pivot = new Vector2(1, 0); // Góc dưới phải
        rt.anchoredPosition = new Vector2(-10, 10);
        rt.sizeDelta = new Vector2(400, 200);

        // Layout để các text xếp chồng lên nhau từ dưới lên
        VerticalLayoutGroup vlg = logPanel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerRight;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.spacing = 5;

        // Cố định size bằng ContentSizeFitter
        ContentSizeFitter csf = logPanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    public void Log(string message, Color color)
    {
        if (logPanel == null) return;

        // Tạo Text
        GameObject txtObj = new GameObject("LogText", typeof(RectTransform), typeof(Text), typeof(Outline));
        txtObj.transform.SetParent(logPanel.transform, false);
        txtObj.transform.SetAsLastSibling();

        Text txt = txtObj.GetComponent<Text>();
        txt.font = defaultFont;
        txt.text = message;
        txt.fontSize = 16;
        txt.alignment = TextAnchor.MiddleRight;
        txt.color = color;

        Outline outline = txtObj.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        activeLogs.Add(txtObj);

        // Nếu vượt quá MAX_LOGS thì xóa cái cũ nhất
        if (activeLogs.Count > MAX_LOGS)
        {
            GameObject oldest = activeLogs[0];
            activeLogs.RemoveAt(0);
            Destroy(oldest);
        }

        // Tự động xóa sau 5 giây
        Destroy(txtObj, 5f);
    }

    private void Update()
    {
        // Dọn dẹp các log đã bị destroy
        activeLogs.RemoveAll(item => item == null);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    private GameObject logPanel;
    private Transform logContent;
    private ScrollRect scrollRect;
    private List<GameObject> activeLogs = new List<GameObject>();
    private const int MAX_LOGS = 50; // Cho phép giữ tới 50 dòng log để cuộn
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
        Log("Hệ thống Nhật ký (Combat Log) đã khởi động!", Color.green);
    }

    private void CreateLogUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // Tìm Panel đã được tạo từ Editor Tool
        Transform logPanelTransform = canvas.transform.Find("CombatLogPanel");
        if (logPanelTransform != null)
        {
            logPanel = logPanelTransform.gameObject;
            scrollRect = logPanel.GetComponent<ScrollRect>();
            Transform viewport = logPanelTransform.Find("Viewport");
            if (viewport != null)
            {
                logContent = viewport.Find("Content");
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy CombatLogPanel! Vui lòng chạy tool TuTien -> Fix -> Create Combat Log UI");
        }
    }

    public void Log(string message, Color color)
    {
        // Vẫn in ra Console để đảm bảo log có chạy
        Debug.Log($"[GameLogger] {message}");

        if (logPanel == null || logContent == null) 
        {
            Debug.LogWarning("[GameLogger] UI chưa được gán! Hãy chắc chắn GameLogger có trên Managers và CombatLogPanel tồn tại.");
            return;
        }

        // Tạo Text
        GameObject txtObj = new GameObject("LogText", typeof(RectTransform), typeof(Text), typeof(Outline));
        txtObj.transform.SetParent(logContent, false); // Gắn vào Content của ScrollView
        txtObj.transform.SetAsLastSibling();

        Text txt = txtObj.GetComponent<Text>();
        txt.font = defaultFont;
        txt.text = message;
        txt.fontSize = 14; // Chữ nhỏ lại một chút để hiển thị được nhiều hơn
        txt.alignment = TextAnchor.MiddleLeft;
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

        // Tự động cuộn xuống dưới cùng sau khi thêm log (chờ hết frame)
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void Update()
    {
        // Dọn dẹp các log đã bị destroy
        activeLogs.RemoveAll(item => item == null);
    }
}

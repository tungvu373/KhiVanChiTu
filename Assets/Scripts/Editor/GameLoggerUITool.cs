using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class GameLoggerUITool : EditorWindow
{
    [MenuItem("TuTien/Fix/Create Combat Log UI")]
    public static void CreateUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        Transform oldLog = canvas.transform.Find("CombatLogPanel");
        if (oldLog != null) DestroyImmediate(oldLog.gameObject);

        // Tạo Panel
        GameObject logPanel = new GameObject("CombatLogPanel", typeof(RectTransform), typeof(Image));
        logPanel.transform.SetParent(canvas.transform, false);
        logPanel.transform.SetAsLastSibling(); 
        
        RectTransform rt = logPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0); 
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0); 
        rt.anchoredPosition = new Vector2(-150, 5); // Dưới thanh MP (MP ở Y=115, cao 25 -> mép dưới là 115)
        rt.sizeDelta = new Vector2(300, 105); 

        Image bg = logPanel.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        ScrollRect scrollRect = logPanel.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 15f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObj.transform.SetParent(logPanel.transform, false);
        RectTransform viewportRt = viewportObj.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero; viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero; viewportRt.offsetMax = Vector2.zero;
        
        Image vpImg = viewportObj.GetComponent<Image>();
        vpImg.color = Color.white; 
        Mask mask = viewportObj.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0, 1);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.spacing = 3;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;
        
        // Thêm Text mẫu để test
        GameObject txtObj = new GameObject("SampleText", typeof(RectTransform), typeof(Text), typeof(Outline));
        txtObj.transform.SetParent(contentObj.transform, false);
        Text txt = txtObj.GetComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "Khung trạng thái đã sẵn sàng!";
        txt.fontSize = 14;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.color = Color.green;
        txtObj.GetComponent<Outline>().effectColor = Color.black;
        txtObj.GetComponent<Outline>().effectDistance = new Vector2(1, -1);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Tạo Khung trạng thái thành công! Hãy xem thử trong Scene.");
    }
}

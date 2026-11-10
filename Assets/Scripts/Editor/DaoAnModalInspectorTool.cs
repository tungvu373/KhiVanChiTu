using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DaoAnModalInspectorTool
{
    [MenuItem("Tools/Inspect DaoAnModal")]
    public static void Inspect()
    {
        var canvas = GameObject.Find("Canvas");
        var modal = canvas != null ? canvas.transform.Find("DaoAnModal") : null;
        if (modal == null)
        {
            Debug.LogError("[InspectDaoAnModal] Canvas/DaoAnModal not found!");
            return;
        }

        Debug.Log($"=== Inspecting {modal.name} (active: {modal.gameObject.activeSelf}) ===");
        PrintRecursive(modal, 0);
    }

    private static void PrintRecursive(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        var rt = t.GetComponent<RectTransform>();
        string rectInfo = rt != null ? $"[anchors:({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}), size:({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}), pos:({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0})]" : "";
        
        string compInfo = "";
        var img = t.GetComponent<Image>();
        if (img != null) compInfo += $" Img({img.color})";
        var txt = t.GetComponent<Text>();
        if (txt != null) compInfo += $" Txt(\"{txt.text}\")";
        var btn = t.GetComponent<Button>();
        if (btn != null) compInfo += " Btn";
        var sf = t.GetComponent<SafeAreaFitter>();
        if (sf != null) compInfo += " SafeAreaFitter";

        Debug.Log($"{indent}- {t.name} {rectInfo}{compInfo}");

        for (int i = 0; i < t.childCount; i++)
        {
            PrintRecursive(t.GetChild(i), depth + 1);
        }
    }
}


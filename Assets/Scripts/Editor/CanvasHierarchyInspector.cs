using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CanvasHierarchyInspector
{
    [MenuItem("Tools/Inspect Canvas Hierarchy")]
    public static void Inspect()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        string report = "=== Top-level children in Canvas ===\n";
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            report += $"[{i}] {child.name} (active: {child.gameObject.activeSelf})\n";
        }
        Debug.Log(report);

        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            string dReport = "=== DaoAnModal Children ===\n";
            foreach (Transform c in daoAn)
            {
                var rt = c.GetComponent<RectTransform>();
                dReport += $"  - {c.name} active:{c.gameObject.activeSelf} [anchors:({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}), pos:({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}), size:({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0})]\n";
                foreach (Transform sub in c)
                {
                    var srt = sub.GetComponent<RectTransform>();
                    dReport += $"    * {sub.name} active:{sub.gameObject.activeSelf} [anchors:({srt.anchorMin.x:F2},{srt.anchorMin.y:F2})-({srt.anchorMax.x:F2},{srt.anchorMax.y:F2}), pos:({srt.anchoredPosition.x:F0},{srt.anchoredPosition.y:F0}), size:({srt.sizeDelta.x:F0},{srt.sizeDelta.y:F0})]\n";
                }
            }
            Debug.Log(dReport);
        }
    }
}

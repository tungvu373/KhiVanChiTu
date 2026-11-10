#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CanvasFullTreeInspector
{
    [MenuItem("Tools/Dump Full Canvas Tree")]
    public static void Dump()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        string report = $"=== CANVAS ROOT: {canvas.name} (Child Count: {canvas.transform.childCount}) ===\n";
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            var rt = child.GetComponent<RectTransform>();
            string pos = rt != null ? $"pos:({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0}) size:({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0})" : "";
            report += $"[{i}] {child.name} (active: {child.gameObject.activeSelf}) {pos}\n";
            
            if (child.name == "MobileUIRoot")
            {
                for (int j = 0; j < child.childCount; j++)
                {
                    var c2 = child.GetChild(j);
                    var rt2 = c2.GetComponent<RectTransform>();
                    string pos2 = rt2 != null ? $"pos:({rt2.anchoredPosition.x:F0},{rt2.anchoredPosition.y:F0}) size:({rt2.sizeDelta.x:F0},{rt2.sizeDelta.y:F0})" : "";
                    report += $"     -> [{j}] {c2.name} (active: {c2.gameObject.activeSelf}) {pos2}\n";
                    if (c2.name == "TopBar" || c2.name == "MobileDock" || c2.name == "SkillPanel" || c2.name == "MobileSkillHUD")
                    {
                        for (int k = 0; k < c2.childCount; k++)
                        {
                            var c3 = c2.GetChild(k);
                            report += $"          * [{k}] {c3.name} (active: {c3.gameObject.activeSelf})\n";
                        }
                    }
                }
            }
        }
        Debug.Log(report);
    }
}
#endif


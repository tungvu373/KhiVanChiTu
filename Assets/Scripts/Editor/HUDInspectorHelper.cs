#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public static class HUDInspectorHelper
{
    [MenuItem("TuTien/Debug/Inspect MergePanel")]
    public static void InspectMergePanel()
    {
        var drawer0 = GameObject.Find("Canvas/MobileUIRoot/Drawer_0");
        if (drawer0 == null)
        {
            Debug.LogError("Drawer_0 not found!");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== MERGE PANEL HIERARCHY ===");
        Transform merge = drawer0.transform.Find("MergePanel");
        if (merge != null)
        {
            DumpHierarchy(merge, sb, 0);
        }

        var mgr = Object.FindAnyObjectByType<MergeManager>(FindObjectsInactive.Include);
        if (mgr != null)
        {
            sb.AppendLine("\n=== MERGE MANAGER REFS ===");
            sb.AppendLine($"pageText: {(mgr.pageText != null ? mgr.pageText.name : "null")}");
            sb.AppendLine($"inventoryPages count: {(mgr.inventoryPages != null ? mgr.inventoryPages.Length : 0)}");
            if (mgr.inventoryPages != null)
            {
                for (int i = 0; i < mgr.inventoryPages.Length; i++)
                {
                    var p = mgr.inventoryPages[i];
                    sb.AppendLine($"  Page [{i}]: {(p != null ? (p.name + " active=" + p.gameObject.activeSelf + " slots=" + p.childCount) : "null")}");
                }
            }
        }

        File.WriteAllText("Assets/Temp/merge_panel_dump.txt", sb.ToString());
        Debug.Log("[HUDInspector] Wrote merge_panel_dump.txt");
    }

    private static void DumpHierarchy(Transform t, StringBuilder sb, int indent)
    {
        string ind = new string(' ', indent * 2);
        var rt = t as RectTransform;
        sb.AppendLine($"{ind}- '{t.name}': active={t.gameObject.activeSelf}, pos={rt.anchoredPosition}, size={rt.sizeDelta}, aMin={rt.anchorMin}, aMax={rt.anchorMax}, pivot={rt.pivot}");
        for (int i = 0; i < t.childCount; i++)
        {
            DumpHierarchy(t.GetChild(i), sb, indent + 1);
        }
    }
}
#endif

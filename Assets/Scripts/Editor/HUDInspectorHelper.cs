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

    [MenuItem("TuTien/Debug/Inspect Canvas Root")]
    public static void InspectCanvasRoot()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("=== CANVAS DIRECT CHILDREN ===");
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            var rt = child.GetComponent<RectTransform>();
            string pos = rt != null ? $"pos={rt.anchoredPosition} size={rt.sizeDelta} aMin={rt.anchorMin} aMax={rt.anchorMax} piv={rt.pivot}" : "NO_RECT";
            sb.AppendLine($"[{i}] '{child.name}' activeSelf={child.gameObject.activeSelf} activeHierarchy={child.gameObject.activeInHierarchy} {pos}");
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot != null)
        {
            sb.AppendLine("\n=== MOBILE UI ROOT DIRECT CHILDREN ===");
            for (int i = 0; i < mobileRoot.childCount; i++)
            {
                var child = mobileRoot.GetChild(i);
                var rt = child.GetComponent<RectTransform>();
                string pos = rt != null ? $"pos={rt.anchoredPosition} size={rt.sizeDelta} aMin={rt.anchorMin} aMax={rt.anchorMax} piv={rt.pivot}" : "NO_RECT";
                sb.AppendLine($"[{i}] '{child.name}' activeSelf={child.gameObject.activeSelf} activeHierarchy={child.gameObject.activeInHierarchy} {pos}");
                
                // Dump sub-children for TopBar, CharacterFrame, TutorialTargetQuestBar, StageUI, PlayerHPBar
                if (child.name == "TopBar" || child.name == "CharacterFrame" || child.name == "TutorialTargetQuestBar" || child.name == "StageUI" || child.name == "PlayerHPBar")
                {
                    for (int j = 0; j < child.childCount; j++)
                    {
                        var grandChild = child.GetChild(j);
                        var grt = grandChild.GetComponent<RectTransform>();
                        string gpos = grt != null ? $"pos={grt.anchoredPosition} size={grt.sizeDelta} aMin={grt.anchorMin} aMax={grt.anchorMax} piv={grt.pivot}" : "NO_RECT";
                        var txt = grandChild.GetComponent<Text>();
                        string tVal = txt != null ? $" text='{txt.text}'" : "";
                        sb.AppendLine($"     -> [{j}] '{grandChild.name}' active={grandChild.gameObject.activeSelf} {gpos}{tVal}");
                    }
                }
            }
        }

        Directory.CreateDirectory("Temp");
        File.WriteAllText("Temp/canvas_root_dump.txt", sb.ToString());
        Debug.Log("[HUDInspector] Wrote Temp/canvas_root_dump.txt\n" + sb.ToString());
    }

    [MenuItem("TuTien/Debug/Inspect All HUD Buttons")]
    public static void InspectAllHUDButtons()
    {
        var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("Canvas/MobileUIRoot not found!");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== ALL HUD BUTTONS IN Canvas/MobileUIRoot ===");
        var buttons = mobileRoot.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            var rt = b.GetComponent<RectTransform>();
            var txt = b.GetComponentInChildren<Text>();
            sb.AppendLine($"Button '{b.gameObject.name}' (parent: '{b.transform.parent.name}') | ActiveSelf: {b.gameObject.activeSelf} | ActiveInHierarchy: {b.gameObject.activeInHierarchy} | Pos: {rt.anchoredPosition} | Size: {rt.sizeDelta} | Text: '{(txt != null ? txt.text : "NO_TEXT")}'");
        }

        var questBar = mobileRoot.transform.Find("TutorialTargetQuestBar");
        sb.AppendLine($"\nTutorialTargetQuestBar activeSelf: {(questBar != null ? questBar.gameObject.activeSelf.ToString() : "NULL")}, activeInHierarchy: {(questBar != null ? questBar.gameObject.activeInHierarchy.ToString() : "NULL")}");

        var btnStorage = GameObject.Find("BtnStorage");
        if (btnStorage == null)
        {
            var allStorage = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allStorage)
            {
                if (go.name == "BtnStorage")
                {
                    btnStorage = go;
                    break;
                }
            }
        }
        sb.AppendLine($"BtnStorage found: {(btnStorage != null ? (btnStorage.name + " active=" + btnStorage.activeInHierarchy + " parent=" + btnStorage.transform.parent.name) : "NOT_FOUND")}");

        Debug.Log(sb.ToString());
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

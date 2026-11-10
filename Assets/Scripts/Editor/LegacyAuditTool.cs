using UnityEditor;
using UnityEngine;

public static class LegacyAuditTool
{
    [MenuItem("Tools/Audit Legacy Canvas Objects")]
    public static void Audit()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        string[] targets = { "UIManager", "RightPanel", "ShopPanel", "FullBackground" };
        foreach (var tName in targets)
        {
            var tr = canvas.transform.Find(tName);
            if (tr == null)
            {
                Debug.Log($"Target '{tName}': NOT FOUND");
                continue;
            }

            var comps = tr.GetComponents<Component>();
            string compList = "";
            foreach (var c in comps) if (c != null) compList += c.GetType().Name + ", ";

            Debug.Log($"=== [{tName}] (active: {tr.gameObject.activeSelf}, childCount: {tr.childCount}) Comps: {compList} ===");
            for (int i = 0; i < tr.childCount; i++)
            {
                var ch = tr.GetChild(i);
                Debug.Log($"   child [{i}] {ch.name} (active: {ch.gameObject.activeSelf})");
            }
        }
    }
}


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AuditAllCloseButtons
{
    [MenuItem("TuTien/QA/Audit All Close Buttons")]
    public static void Run()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Debug.Log("=== AUDITING ALL CLOSE BUTTONS ===");
        var allButtons = canvas.GetComponentsInChildren<Button>(true);
        int closeCount = 0;
        foreach (var btn in allButtons)
        {
            string name = btn.name.ToLower();
            string pName = btn.transform.parent != null ? btn.transform.parent.name.ToLower() : "";
            if (name.Contains("close") || name == "btnclose" || pName.Contains("close") || MobileUIContentSafety.IsCornerClose(btn))
            {
                closeCount++;
                string path = GetHierarchyPath(btn.transform);
                var txt = btn.GetComponentInChildren<Text>(true);
                var tmp = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
                string label = txt != null ? $"Text='{txt.text}', color={txt.color}" :
                               tmp != null ? $"TMP='{tmp.text}', color={tmp.color}" : "NO_TEXT";
                var img = btn.GetComponent<Image>();
                string sprite = img != null && img.sprite != null ? img.sprite.name : "no_sprite";
                Debug.Log($"[CloseBtn #{closeCount}] {path} -> {label} (Sprite: {sprite})");
            }
        }
        Debug.Log($"Audit complete. Found {closeCount} close buttons.");
    }

    private static string GetHierarchyPath(Transform t)
    {
        string p = t.name;
        while (t.parent != null && t.parent.GetComponent<Canvas>() == null)
        {
            t = t.parent;
            p = t.name + "/" + p;
        }
        return p;
    }
}
#endif

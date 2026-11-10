using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class DaoAnUIDebugCapture
{
    [MenuItem("TuTien/Debug/Capture and Inspect DaoAn UI")]
    public static void CaptureAndInspect()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[DaoAnDebug] Canvas not found!");
            return;
        }

        var daoAnModal = canvas.transform.Find("DaoAnModal");
        if (daoAnModal == null)
        {
            Debug.LogError("[DaoAnDebug] Canvas/DaoAnModal not found!");
            return;
        }

        // Make sure it is active
        daoAnModal.gameObject.SetActive(true);

        Debug.Log("=== HIERARCHY OF DAO AN MODAL ===");
        PrintHierarchy(daoAnModal, "");

        // Capture Game View
        string brainDir = @"C:\Users\Admin\.gemini\antigravity\brain\2f68ccec-4dfd-4ad5-80f1-30734aeab11c";
        string filePath = Path.Combine(brainDir, "daoan_current_debug.png");
        ScreenCapture.CaptureScreenshot(filePath);
        Debug.Log($"[DaoAnDebug] Saved screenshot to: {filePath}");
    }

    private static void PrintHierarchy(Transform t, string indent)
    {
        var rt = t.GetComponent<RectTransform>();
        var img = t.GetComponent<Image>();
        var txt = t.GetComponent<Text>();
        var btn = t.GetComponent<Button>();

        string info = $"{indent}- [{t.name}] active={t.gameObject.activeSelf}";
        if (rt != null)
        {
            info += $" anchors=({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}) offMin=({rt.offsetMin.x:F0},{rt.offsetMin.y:F0}) offMax=({rt.offsetMax.x:F0},{rt.offsetMax.y:F0}) size=({rt.sizeDelta.x:F0},{rt.sizeDelta.y:F0}) pos=({rt.anchoredPosition.x:F0},{rt.anchoredPosition.y:F0})";
        }
        if (img != null)
        {
            info += $" ImgColor={img.color} sprite={(img.sprite != null ? img.sprite.name : "null")}";
        }
        if (txt != null)
        {
            info += $" Txt=\"{txt.text}\" font={txt.font?.name} size={txt.fontSize}";
        }
        if (btn != null)
        {
            info += " [HasButton]";
        }

        Debug.Log(info);

        // Don't recurse into Content if it has too many items, just report count
        if (t.name == "Content")
        {
            Debug.Log($"{indent}  (Content has {t.childCount} children)");
            return;
        }

        for (int i = 0; i < t.childCount; i++)
        {
            PrintHierarchy(t.GetChild(i), indent + "  ");
        }
    }
}


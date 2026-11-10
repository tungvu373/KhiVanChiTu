#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public static class TopIconsFinder
{
    [MenuItem("TuTien/Debug/Find Top Icons")]
    public static void FindIcons()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var sb = new StringBuilder();
        foreach (var img in canvas.GetComponentsInChildren<Image>(true))
        {
            var rt = img.rectTransform;
            // Check if near top: anchorMin.y >= 0.8f or anchoredPosition.y > 800f
            if (rt.position.y > Screen.height * 0.8f || rt.anchorMin.y >= 0.8f)
            {
                sb.AppendLine($"Top Image: {GetPath(img.transform)} | activeSelf={img.gameObject.activeSelf}, sprite={(img.sprite != null ? img.sprite.name : "none")}, pos={rt.anchoredPosition}, size={rt.sizeDelta}, aMin={rt.anchorMin}, aMax={rt.anchorMax}, color={img.color}");
            }
        }
        File.WriteAllText("Assets/Temp/top_icons.txt", sb.ToString());
        Debug.Log("[TopIconsFinder] Wrote top_icons.txt");
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif


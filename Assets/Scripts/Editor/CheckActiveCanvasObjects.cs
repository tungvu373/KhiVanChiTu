using UnityEditor;
using UnityEngine;

public static class CheckActiveCanvasObjects
{
    [MenuItem("Tools/Check Active Canvas Objects")]
    public static void Check()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        Debug.Log("=== Active GameObjects under Canvas ===");
        CheckActiveRecursive(canvas.transform, "");
    }

    private static void CheckActiveRecursive(Transform t, string path)
    {
        string curPath = string.IsNullOrEmpty(path) ? t.name : $"{path}/{t.name}";
        if (t.gameObject.activeSelf)
        {
            Debug.Log($"ACTIVE: {curPath}");
            for (int i = 0; i < t.childCount; i++)
            {
                CheckActiveRecursive(t.GetChild(i), curPath);
            }
        }
    }
}


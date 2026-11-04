using UnityEditor;
using UnityEngine;

public static class CanvasInspector
{
    [MenuItem("TuTien/Debug/Inspect Canvas Hierarchy")]
    public static void Inspect()
    {
        var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
        if (mobileRoot == null) { Debug.LogError("No MobileUIRoot found!"); return; }
        Debug.Log($"=== TOTAL CHILDREN OF MobileUIRoot: {mobileRoot.transform.childCount} ===");
        for (int i = 0; i < mobileRoot.transform.childCount; i++)
        {
            var child = mobileRoot.transform.GetChild(i);
            var rt = child.GetComponent<RectTransform>();
            Debug.Log($"Child [{i}]: {child.name} (active: {child.gameObject.activeSelf}, subchildren: {child.childCount})");
        }

        var drawerObj = GameObject.Find("Canvas/MobileUIRoot/MobileDrawer") ?? GameObject.Find("MobileDrawer");
        if (drawerObj != null)
        {
            Debug.Log($"=== MOBILE DRAWER ({drawerObj.transform.childCount} children) ===");
            for (int i = 0; i < drawerObj.transform.childCount; i++)
            {
                var c = drawerObj.transform.GetChild(i);
                Debug.Log($"Drawer Child [{i}]: {c.name} (active: {c.gameObject.activeSelf})");
                for (int j = 0; j < c.childCount; j++)
                {
                    var sub = c.GetChild(j);
                    Debug.Log($"   -> [{j}] {sub.name} (active: {sub.gameObject.activeSelf})");
                }
            }
        }
    }
}

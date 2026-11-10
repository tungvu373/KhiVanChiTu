using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class TopBarInspector
{
    [MenuItem("TuTien/Debug/Inspect TopBar")]
    public static void Inspect()
    {
        var topBar = GameObject.Find("Canvas/MobileUIRoot/TopBar");
        if (topBar == null) return;
        Debug.Log($"TopBar children ({topBar.transform.childCount}):");
        for (int i = 0; i < topBar.transform.childCount; i++)
        {
            var child = topBar.transform.GetChild(i);
            var txt = child.GetComponent<Text>() ?? child.GetComponentInChildren<Text>();
            Debug.Log($"  TopBar[{i}]: {child.name} | active={child.gameObject.activeSelf} | txt='{txt?.text}'");
        }
    }
}


using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class StageInspector
{
    [MenuItem("TuTien/Debug/Inspect Stage Elements")]
    public static void Inspect()
    {
        var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
        if (mobileRoot == null) return;
        var p1 = mobileRoot.transform.Find("StagePanel");
        var p2 = mobileRoot.transform.Find("StageUI");
        if (p1 != null)
        {
            var comps = string.Join(", ", p1.GetComponents<Component>().Select(c => c?.GetType().Name));
            Debug.Log($"StagePanel: active={p1.gameObject.activeSelf}, components={comps}");
            for (int i = 0; i < p1.childCount; i++) Debug.Log($"  StagePanel[{i}]: {p1.GetChild(i).name} - txt='{p1.GetChild(i).GetComponent<Text>()?.text}'");
        }
        if (p2 != null)
        {
            var comps = string.Join(", ", p2.GetComponents<Component>().Select(c => c?.GetType().Name));
            Debug.Log($"StageUI: active={p2.gameObject.activeSelf}, components={comps}");
            for (int i = 0; i < p2.childCount; i++) Debug.Log($"  StageUI[{i}]: {p2.GetChild(i).name} - txt='{p2.GetChild(i).GetComponent<Text>()?.text}'");
        }
    }
}

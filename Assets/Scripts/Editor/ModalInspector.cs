using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ModalInspector
{
    [MenuItem("TuTien/Debug/Inspect MilestoneModalUI")]
    public static void Inspect()
    {
        var modal = GameObject.Find("Canvas/MilestoneModalUI");
        if (modal == null)
        {
            var all = Resources.FindObjectsOfTypeAll<MilestoneModalUI>();
            if (all.Length > 0) modal = all[0].gameObject;
        }
        if (modal == null) return;

        var modalBox = modal.transform.Find("ModalBox");
        for (int i = 0; i < modalBox.childCount; i++)
        {
            var child = modalBox.GetChild(i);
            var rt = child.GetComponent<RectTransform>();
            var img = child.GetComponent<Image>();
            Debug.Log($"ModalBox Child[{i}]: {child.name} | anchor=({rt.anchorMin}, {rt.anchorMax}) | pos={rt.anchoredPosition} | size={rt.sizeDelta} | raycast={img?.raycastTarget}");
        }
    }
}

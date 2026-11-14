using UnityEngine;

/// <summary>Style new legacy log rows once after their producer finishes assigning text and size.</summary>
[DefaultExecutionOrder(1700)]
public sealed class PortraitHudLogStyleScope : MonoBehaviour
{
    bool pending = true;
    void OnTransformChildrenChanged() { pending = true; }
    void OnEnable() { pending = true; }
    void LateUpdate()
    {
        if (!pending) return;
        pending = false;
        foreach (var text in GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            text.font = UiTextStyle.BodyFont;
            text.fontSize = 36;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var row = text.GetComponent<UnityEngine.UI.LayoutElement>() ?? text.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            row.minHeight = 54;
            row.preferredHeight = Mathf.Max(54, text.preferredHeight + 12);
        }
    }
}

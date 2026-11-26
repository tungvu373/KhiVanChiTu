using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationValue : MonoBehaviour
    {
        public TMP_Text label, value;
        public UnityEngine.UI.Image icon;
        public void Set(string caption, string display, Sprite sprite = null)
        {
            if (label != null) label.text = caption ?? "";
            if (value != null) value.text = display ?? "";
            if (icon != null) { icon.sprite = sprite; icon.enabled = sprite != null; }
        }
    }
}

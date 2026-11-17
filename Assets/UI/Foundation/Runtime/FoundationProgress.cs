using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationProgress : MonoBehaviour
    {
        public RectTransform fill;
        public TMP_Text label;
        public float Normalized { get; private set; }
        public void Set(float current, float maximum, string caption = null)
        {
            if (float.IsNaN(current) || float.IsInfinity(current)) current = 0;
            if (float.IsNaN(maximum) || float.IsInfinity(maximum)) maximum = 0;
            Normalized = maximum > 0 ? Mathf.Clamp01(current / maximum) : 0;
            if (fill != null) { fill.anchorMax = new Vector2(Normalized, 1); fill.offsetMin = fill.offsetMax = Vector2.zero; }
            if (label != null) label.text = caption ?? Mathf.RoundToInt(Normalized * 100) + "%";
        }
    }
}

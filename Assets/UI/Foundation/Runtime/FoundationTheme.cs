using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    [CreateAssetMenu(menuName = "KhiVanChiTu/UI/Foundation Theme")]
    public sealed class FoundationTheme : ScriptableObject
    {
        public Color ink = new Color32(16, 40, 42, 255);
        public Color surface = new Color32(22, 60, 60, 255);
        public Color raised = new Color32(30, 76, 72, 255);
        public Color gold = new Color32(201, 167, 106, 255);
        public Color ivory = new Color32(243, 235, 216, 255);
        public Color muted = new Color32(175, 189, 179, 255);
        public Color jade = new Color32(105, 202, 162, 255);
        public Color danger = new Color32(216, 95, 103, 255);
        public TMP_FontAsset bodyFont, headingFont;
        public Sprite surfaceSprite;
        public float margin = 36, padding = 32, gap = 24;
        public float titleSize = 52, headingSize = 38, bodySize = 32, captionSize = 28;
        public float controlHeight = 132;
        // Reference units, not physical dp. Device touch checks remain a release gate.
        public Vector2 referenceResolution = new Vector2(1080, 1920);
        public string VietnameseGlyphs => "\u0103\u0102\u00e2\u00c2\u0111\u0110\u00ea\u00ca\u00f4\u00d4\u01a1\u01a0\u01b0\u01af";
    }
}

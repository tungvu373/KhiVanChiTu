using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KhiVanChiTu.UI.Refinement
{
    /// <summary>Layout-independent, non-interactive available-action indicator.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NotificationBadge : MaskableGraphic
    {
        public TMP_Text countLabel;
        public int Count { get; private set; }
        public void SetVisible(bool visible) => SetCount(visible ? 1 : 0);
        public void SetCount(int count)
        {
            Count = Mathf.Max(0, count);
            if (gameObject.activeSelf != (Count > 0)) gameObject.SetActive(Count > 0);
            if (countLabel != null)
            {
                countLabel.raycastTarget = false;
                countLabel.text = Count > 99 ? "99+" : Count.ToString();
            }
        }
        protected override void OnEnable() { base.OnEnable(); raycastTarget = false; }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect; var center = r.center;
            mesh.AddVert(center, color, Vector2.zero);
            const int segments = 24;
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                mesh.AddVert(center + new Vector2(Mathf.Cos(a) * r.width / 2, Mathf.Sin(a) * r.height / 2), color, Vector2.zero);
                if (i > 0) mesh.AddTriangle(0, i, i + 1);
            }
        }
    }
}

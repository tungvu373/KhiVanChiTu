using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Ve vong tron % Tu Vi hoac quat dem nguoc hoi chieu ky nang bang Painter2D trong 1 Draw Call.
    /// </summary>
    public sealed class RadialRingElement : VisualElement
    {
        private float _progress = 0f;
        private Color _trackColor = new Color(0.75f, 0.85f, 0.79f, 0.55f);
        private Color _fillColor = new Color(0.87f, 0.62f, 0.14f, 0.95f);
        private float _lineWidth = 4.5f;

        public RadialRingElement()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
        }

        public void SetProgress(float progress01, Color fillColor, Color trackColor, float width = 4.5f)
        {
            float clamped = Mathf.Clamp01(progress01);
            if (Mathf.Abs(_progress - clamped) < 0.005f &&
                _fillColor == fillColor &&
                _trackColor == trackColor &&
                Mathf.Approximately(_lineWidth, width))
                return;

            _progress = clamped;
            _fillColor = fillColor;
            _trackColor = trackColor;
            _lineWidth = width;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            float w = contentRect.width;
            float h = contentRect.height;
            if (w <= 4f || h <= 4f) return;

            var painter = ctx.painter2D;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float radius = Mathf.Min(w, h) * 0.5f - _lineWidth;
            if (radius <= 2f) return;

            painter.lineCap = LineCap.Round;
            painter.lineWidth = _lineWidth;

            painter.strokeColor = _trackColor;
            painter.BeginPath();
            painter.Arc(center, radius, 0f, 360f);
            painter.Stroke();

            if (_progress > 0.005f)
            {
                painter.strokeColor = _fillColor;
                painter.BeginPath();
                painter.Arc(center, radius, -90f, -90f + 360f * _progress);
                painter.Stroke();
            }
        }
    }

    /// <summary>
    /// Lop ve duong noi linh mach giua cac node Cay Ky Nang Dao Van bang Painter2D (co Fog of War & Zoom).
    /// </summary>
    public sealed class DaoVanGraphLayer : VisualElement
    {
        private IReadOnlyList<DaoVanNodeSpec> _nodes;
        private Func<string, int> _levelLookup;
        private Func<DaoVanNodeSpec, bool> _visibilityLookup;
        private Vector2 _panOffset;
        private float _zoom = 1f;

        public DaoVanGraphLayer()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Bind(IReadOnlyList<DaoVanNodeSpec> nodes, Func<string, int> levelLookup, Vector2 pan, float zoom, Func<DaoVanNodeSpec, bool> visibilityLookup = null)
        {
            _nodes = nodes;
            _levelLookup = levelLookup;
            _visibilityLookup = visibilityLookup;
            _panOffset = pan;
            _zoom = Mathf.Clamp(zoom, 0.45f, 1.85f);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (_nodes == null || _levelLookup == null) return;
            float cx = contentRect.width * 0.5f + _panOffset.x;
            float cy = contentRect.height * 0.5f + _panOffset.y;

            var painter = ctx.painter2D;
            painter.lineCap = LineCap.Round;

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (string.IsNullOrEmpty(node.parentId)) continue;
                if (_visibilityLookup != null && !_visibilityLookup(node)) continue;

                DaoVanNodeSpec parent = FindParent(node.parentId);
                if (parent == null) continue;
                if (_visibilityLookup != null && !_visibilityLookup(parent)) continue;

                Vector2 a = new Vector2(cx + parent.normalizedPos.x * _zoom, cy + parent.normalizedPos.y * _zoom);
                Vector2 b = new Vector2(cx + node.normalizedPos.x * _zoom, cy + node.normalizedPos.y * _zoom);

                bool unlocked = _levelLookup(node.id) > 0;
                bool parentActive = _levelLookup(parent.id) > 0;

                painter.lineWidth = Mathf.Clamp((unlocked ? 4.4f : 2.6f) * _zoom, 1.6f, 7.2f);
                painter.strokeColor = unlocked
                    ? new Color(0.84f, 0.60f, 0.16f, 0.94f)
                    : parentActive
                        ? new Color(0.16f, 0.58f, 0.46f, 0.78f)
                        : new Color(0.62f, 0.70f, 0.67f, 0.45f);

                painter.BeginPath();
                painter.MoveTo(a);
                Vector2 mid = (a + b) * 0.5f + new Vector2((b.y - a.y) * 0.08f, (a.x - b.x) * 0.08f);
                painter.QuadraticCurveTo(mid, b);
                painter.Stroke();
            }
        }

        private DaoVanNodeSpec FindParent(string id)
        {
            for (int i = 0; i < _nodes.Count; i++)
                if (_nodes[i].id == id) return _nodes[i];
            return null;
        }
    }
}

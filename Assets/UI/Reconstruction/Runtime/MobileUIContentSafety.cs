using UnityEngine;

/// <summary>Final presentation pass for framed captions and existing corner-close controls.</summary>
public static class MobileUIContentSafety
{
    public static readonly Color CloseRed = new Color32(255, 82, 82, 255);

    public static bool IsCornerClose(UnityEngine.UI.Button button)
    {
        if (button == null) return false;
        string name = button.name.ToLowerInvariant().Replace("_", "");
        if (!name.Contains("close") || name.Contains("footer") || name.Contains("bottom")) return false;
        var rect = button.transform as RectTransform;
        if (rect == null) return false;
        for (var parent = rect.parent; parent != null; parent = parent.parent)
        {
            if (parent.name.Contains("Footer")) return false;
            if (parent.name.Contains("Header")) return true;
            if (parent.GetComponent<Canvas>() != null) break;
        }
        return rect.anchorMax.y > .75f && rect.anchorMax.x > .75f;
    }

    public static void Apply(Transform scope)
    {
        if (scope == null) return;
        foreach (var text in scope.GetComponentsInChildren<UnityEngine.UI.Text>(true)) PadFramedCaption(text);
        foreach (var button in scope.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            if (IsCornerClose(button)) StyleClose(button);
        foreach (var rect in scope.GetComponentsInChildren<RectTransform>(true))
        {
            string name = rect.name;
            if (name == "HeaderBar" || name == "P6Header") ProtectHeader(rect, "TitleText", "P6Title", "Title");
            if (name == "ModalBox" || name == "PauseBox" || name == "Box" || name == "DialogBox" || name == "MenuCard")
                ProtectHeader(rect, "Title", "TxtTitle", "TxtHeaderTitle", "TxtModalTitle");
        }
    }

    static void StyleClose(UnityEngine.UI.Button button)
    {
        var rect = (RectTransform)button.transform;
        foreach (var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            text.text = "X";
            text.supportRichText = false;
            text.color = CloseRed;
            text.fontStyle = FontStyle.Bold;
            text.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.rect.height * .30f), 20, 44);
            text.resizeTextForBestFit = false;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            var label = text.rectTransform;
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = new Vector2(rect.rect.width * .20f, rect.rect.height * .18f);
            label.offsetMax = -label.offsetMin;
        }
    }

    static void PadFramedCaption(UnityEngine.UI.Text text)
    {
        var rect = text.rectTransform;
        if (rect.parent == null || text.GetComponentInParent<UnityEngine.UI.InputField>() != null ||
            rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one) return;
        var image = rect.parent.GetComponent<UnityEngine.UI.Image>();
        if (image == null || image.sprite == null || image.type != UnityEngine.UI.Image.Type.Sliced ||
            image.sprite.border.sqrMagnitude == 0 || image.GetComponent<UnityEngine.UI.LayoutGroup>() != null) return;
        // Sprite borders include the whole corner tile, including its empty area.
        // Keep a conservative caption inset without treating that entire tile as opaque.
        float canvasPPU = image.canvas != null ? image.canvas.referencePixelsPerUnit : 100f;
        float ppu = Mathf.Max(.01f, image.sprite.pixelsPerUnit / canvasPPU * image.pixelsPerUnitMultiplier);
        var border = image.sprite.border / ppu;
        var size = image.rectTransform.rect.size;
        float left = Mathf.Clamp(border.x * .45f, 8, Mathf.Max(8, size.x * .16f));
        float right = Mathf.Clamp(border.z * .45f, 8, Mathf.Max(8, size.x * .16f));
        float bottom = Mathf.Clamp(border.y * .20f, 6, Mathf.Max(6, size.y * .18f));
        float top = Mathf.Clamp(border.w * .20f, 6, Mathf.Max(6, size.y * .18f));
        rect.offsetMin = new Vector2(Mathf.Max(rect.offsetMin.x, left), Mathf.Max(rect.offsetMin.y, bottom));
        rect.offsetMax = new Vector2(Mathf.Min(rect.offsetMax.x, -right), Mathf.Min(rect.offsetMax.y, -top));
    }

    static Rect InLocal(RectTransform parent, RectTransform child)
    {
        var corners = new Vector3[4];
        child.GetWorldCorners(corners);
        Vector2 min = parent.InverseTransformPoint(corners[0]), max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            Vector2 point = parent.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, point); max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    static void ProtectHeader(RectTransform header, params string[] titleNames)
    {
        float padding = Mathf.Min(60, header.rect.width * .06f);
        float gap = Mathf.Min(20, header.rect.width * .025f);
        foreach (string name in titleNames)
        {
            var title = header.Find(name) as RectTransform;
            if (title == null || title.GetComponent<UnityEngine.UI.Text>() == null) continue;
            var bounds = InLocal(header, title);
            float left = Mathf.Max(padding, bounds.xMin - header.rect.xMin);
            float right = Mathf.Max(padding, header.rect.xMax - bounds.xMax);
            float top = Mathf.Max(Mathf.Min(32, header.rect.width * .035f), header.rect.yMax - bounds.yMax);
            foreach (var button in header.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                if (!IsCornerClose(button)) continue;
                var close = InLocal(header, (RectTransform)button.transform);
                if (close.yMax < bounds.yMin) continue;
                right = Mathf.Max(right, header.rect.xMax - close.xMin + gap);
            }
            if (header.rect.width - left - right < 80) continue;
            title.anchorMin = new Vector2(0, 1); title.anchorMax = Vector2.one;
            title.pivot = new Vector2(.5f, 1);
            title.sizeDelta = new Vector2(-left - right, bounds.height);
            title.anchoredPosition = new Vector2((left - right) * .5f, -top);
            var text = title.GetComponent<UnityEngine.UI.Text>();
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
        }
    }
}

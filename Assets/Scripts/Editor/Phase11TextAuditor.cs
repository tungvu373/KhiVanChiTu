using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class Phase11TextAuditor
{
    [MenuItem("TuTien/QA/Phase 11 — Audit Text Alignment and Borders")]
    public static void AuditTextAndBorders()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== PHASE 11 AUDIT: TEXT BOUNDS & BORDER PADDING ===");

        var allTexts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var allTMPs = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        sb.AppendLine($"Total Text components: {allTexts.Length}");
        sb.AppendLine($"Total TMP components: {allTMPs.Length}");

        int zeroMarginCount = 0;
        int potentialClipCount = 0;

        foreach (var t in allTexts)
        {
            var rt = t.rectTransform;
            var parent = rt.parent as RectTransform;
            if (parent == null) continue;

            string path = Phase11UIInspector.GetHierarchyPath(rt);

            // Check if parent has an ornate image (Border / Frame / Panel)
            var parentImg = parent.GetComponent<Image>();
            bool hasFrame = parentImg != null && parentImg.sprite != null && 
                           (parentImg.sprite.name.Contains("Border") || parentImg.sprite.name.Contains("Frame") || 
                            parentImg.sprite.name.Contains("Panel") || parentImg.sprite.name.Contains("Track") ||
                            parentImg.sprite.name.Contains("Button") || parentImg.sprite.name.Contains("Box"));

            // Check if text fills 100% of parent with 0 padding
            bool fullStretch = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
            bool zeroOffset = rt.offsetMin == Vector2.zero && rt.offsetMax == Vector2.zero;

            if (hasFrame && fullStretch && zeroOffset)
            {
                zeroMarginCount++;
                sb.AppendLine($"[ZeroMargin On Frame] {path} | Parent Sprite: {parentImg.sprite.name} | Text: '{t.text.Trim()}' (size: {t.fontSize})");
            }

            // Check if text rect is smaller than font line height (causing vertical clipping)
            if (rt.rect.height > 0 && rt.rect.height < t.fontSize * 1.1f && !t.resizeTextForBestFit)
            {
                potentialClipCount++;
                sb.AppendLine($"[Height Clip Risk] {path} | Rect H: {rt.rect.height:F1} < Font {t.fontSize} | Text: '{t.text.Trim()}'");
            }
        }

        foreach (var tmp in allTMPs)
        {
            var rt = tmp.rectTransform;
            var parent = rt.parent as RectTransform;
            if (parent == null) continue;

            string path = Phase11UIInspector.GetHierarchyPath(rt);
            var parentImg = parent.GetComponent<Image>();
            bool hasFrame = parentImg != null && parentImg.sprite != null;

            bool fullStretch = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
            bool zeroOffset = rt.offsetMin == Vector2.zero && rt.offsetMax == Vector2.zero;

            if (hasFrame && fullStretch && zeroOffset && tmp.margin == Vector4.zero)
            {
                zeroMarginCount++;
                sb.AppendLine($"[TMP ZeroMargin On Frame] {path} | Parent Sprite: {parentImg.sprite.name} | Text: '{tmp.text.Trim()}'");
            }
        }

        sb.AppendLine($"\nSummary: Found {zeroMarginCount} texts with zero margin on decorative frames, {potentialClipCount} texts with vertical clip risk.");
        Debug.Log(sb.ToString());
    }
}


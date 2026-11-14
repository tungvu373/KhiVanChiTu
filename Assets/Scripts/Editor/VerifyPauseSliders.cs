using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class VerifyPauseSliders
{
    [MenuItem("TuTien/QA/Verify Pause Sliders")]
    public static void Verify()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found!"); return; }

        var pauseModal = canvas.transform.Find("PauseMenuModal");
        if (pauseModal == null) { Debug.LogError("PauseMenuModal not found under Canvas!"); return; }

        var sb = new StringBuilder();
        sb.AppendLine("=== VERIFY PAUSE MENU MODAL SLIDERS ===");

        string[] rows = { "BgmRow", "SfxRow" };
        foreach (var r in rows)
        {
            var sliderTr = pauseModal.Find($"PauseBox/AudioSection/{r}/Slider");
            if (sliderTr == null) { sb.AppendLine($"{r}/Slider: NOT FOUND"); continue; }

            var sl = sliderTr.GetComponent<Slider>();
            var sRt = sliderTr.GetComponent<RectTransform>();
            var bg = sliderTr.Find("Background") as RectTransform;
            var fa = sliderTr.Find("Fill Area") as RectTransform;
            var fill = sliderTr.Find("Fill Area/Fill") as RectTransform;
            var ha = sliderTr.Find("Handle Slide Area") as RectTransform;
            var handle = sliderTr.Find("Handle Slide Area/Handle") as RectTransform;

            sb.AppendLine($"\n--- {r}/Slider ---");
            sb.AppendLine($"  Slider Rect: {sRt.rect.width:F1} x {sRt.rect.height:F1}");
            if (bg != null) sb.AppendLine($"  Background Rect: {bg.rect.width:F1} x {bg.rect.height:F1}, pos={bg.anchoredPosition}");
            if (fa != null) sb.AppendLine($"  Fill Area Rect: {fa.rect.width:F1} x {fa.rect.height:F1}, pos={fa.anchoredPosition}");
            if (fill != null) sb.AppendLine($"  Fill Rect: {fill.rect.width:F1} x {fill.rect.height:F1}, pos={fill.anchoredPosition}");
            if (ha != null) sb.AppendLine($"  Handle Area Rect: {ha.rect.width:F1} x {ha.rect.height:F1}, pos={ha.anchoredPosition}");
            if (handle != null)
            {
                var hImg = handle.GetComponent<Image>();
                sb.AppendLine($"  Handle Rect: {handle.rect.width:F1} x {handle.rect.height:F1}, pos={handle.anchoredPosition}, anchors=({handle.anchorMin.x:F2},{handle.anchorMin.y:F2})-({handle.anchorMax.x:F2},{handle.anchorMax.y:F2}), preserveAspect={hImg?.preserveAspect}");
            }
        }

        Debug.Log(sb.ToString());
    }
}


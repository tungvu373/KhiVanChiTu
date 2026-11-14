using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class Phase11UIInspector
{
    [MenuItem("TuTien/QA/Phase 11 — Deep Inspect VitalBars and Pause")]
    public static void DeepInspect()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== PHASE 11 DEEP INSPECT: VITAL BARS ===");

        var vitalBars = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars");
        if (vitalBars != null)
        {
            DumpHierarchy(sb, vitalBars.transform, 0);
        }
        else
        {
            sb.AppendLine("VitalBars NOT FOUND at 'Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars'!");
            // Search anywhere
            var found = GameObject.Find("VitalBars");
            if (found != null) DumpHierarchy(sb, found.transform, 0);
        }

        sb.AppendLine("\n=== PHASE 11 DEEP INSPECT: PAUSE MENU MODAL ===");
        var pauseModal = GameObject.Find("Canvas/PauseMenuModal");
        if (pauseModal != null)
        {
            DumpHierarchy(sb, pauseModal.transform, 0);
        }
        else
        {
            sb.AppendLine("PauseMenuModal NOT FOUND at 'Canvas/PauseMenuModal'!");
        }

        sb.AppendLine("\n=== ALL OBJECTS WITH 'HP' OR 'MANA' IN SCENE ===");
        var allGOs = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allGOs)
        {
            string n = go.name.ToLower();
            if ((n.Contains("hp") || n.Contains("mana")) && (go.GetComponent<Slider>() != null || go.GetComponent<Image>() != null || go.GetComponent<Text>() != null || go.GetComponent<TextMeshProUGUI>() != null))
            {
                sb.AppendLine($"[Match] {GetHierarchyPath(go.transform)} (Active: {go.activeInHierarchy})");
                var slider = go.GetComponent<Slider>();
                if (slider != null) sb.AppendLine($"   -> HAS SLIDER: min={slider.minValue}, max={slider.maxValue}, val={slider.value}, dir={slider.direction}");
                var img = go.GetComponent<Image>();
                if (img != null) sb.AppendLine($"   -> HAS IMAGE: type={img.type}, fillMethod={img.fillMethod}, fillAmount={img.fillAmount}, sprite={(img.sprite != null ? img.sprite.name : "null")}");
                var txt = go.GetComponent<Text>();
                if (txt != null) sb.AppendLine($"   -> HAS TEXT: '{txt.text}', font={txt.font?.name}, size={txt.fontSize}, align={txt.alignment}");
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp != null) sb.AppendLine($"   -> HAS TMP: '{tmp.text}', size={tmp.fontSize}, align={tmp.alignment}");
            }
        }

        Debug.Log(sb.ToString());
    }

    private static void DumpHierarchy(StringBuilder sb, Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        var rt = t.GetComponent<RectTransform>();
        sb.AppendLine($"{indent}* [{t.name}] (ActiveSelf: {t.gameObject.activeSelf}, ActiveInHierarchy: {t.gameObject.activeInHierarchy})");
        if (rt != null)
        {
            sb.AppendLine($"{indent}  RT: anchMin=({rt.anchorMin.x:F3},{rt.anchorMin.y:F3}) anchMax=({rt.anchorMax.x:F3},{rt.anchorMax.y:F3}) piv=({rt.pivot.x:F3},{rt.pivot.y:F3}) sizeDelta=({rt.sizeDelta.x:F1},{rt.sizeDelta.y:F1}) pos=({rt.anchoredPosition.x:F1},{rt.anchoredPosition.y:F1}) rect=({rt.rect.width:F1}x{rt.rect.height:F1})");
        }

        var comps = t.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null || c is Transform) continue;
            if (c is Image img)
            {
                sb.AppendLine($"{indent}  [Image] type={img.type}, sprite={(img.sprite != null ? img.sprite.name : "null")}, color={ColorUtility.ToHtmlStringRGBA(img.color)}, fillAmount={img.fillAmount}");
            }
            else if (c is Slider sl)
            {
                sb.AppendLine($"{indent}  [Slider] val={sl.value}, min={sl.minValue}, max={sl.maxValue}, fillRect={(sl.fillRect != null ? sl.fillRect.name : "null")}, handleRect={(sl.handleRect != null ? sl.handleRect.name : "null")}");
            }
            else if (c is Text tx)
            {
                sb.AppendLine($"{indent}  [Text] text='{tx.text}', size={tx.fontSize}, align={tx.alignment}");
            }
            else if (c is TextMeshProUGUI tmp)
            {
                sb.AppendLine($"{indent}  [TMP] text='{tmp.text}', size={tmp.fontSize}, align={tmp.alignment}");
            }
            else if (c is LayoutGroup lg)
            {
                sb.AppendLine($"{indent}  [{c.GetType().Name}] pad=({lg.padding.left},{lg.padding.right},{lg.padding.top},{lg.padding.bottom})");
            }
            else
            {
                sb.AppendLine($"{indent}  [{c.GetType().Name}]");
            }
        }

        for (int i = 0; i < t.childCount; i++)
        {
            DumpHierarchy(sb, t.GetChild(i), depth + 1);
        }
    }

    public static string GetHierarchyPath(Transform t)
    {
        if (t == null) return "";
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}

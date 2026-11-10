using UnityEngine;
using UnityEngine.UI;

/// <summary>Four compact realm phases in the existing mobile StagePanel.</summary>
public class CultivationPhaseBarUI : MonoBehaviour
{
    private static readonly string[] PhaseLabels = { "SƠ KỲ", "TRUNG KỲ", "HẬU KỲ", "ĐỈNH PHONG" };
    [SerializeField] private UnityEngine.UI.Text stageLabel;
    [SerializeField] private UnityEngine.UI.Image[] fills = new UnityEngine.UI.Image[4];
    [SerializeField] private UnityEngine.UI.Text[] labels = new UnityEngine.UI.Text[4];
    private Button tribulationButton;

    public static CultivationPhaseBarUI Ensure(UnityEngine.UI.Text title)
    {
        if (title == null || title.transform.parent == null) return null;
        var parent = title.transform.parent as RectTransform;
        if (parent == null) return null;
        var bar = parent.GetComponent<CultivationPhaseBarUI>();
        if (bar == null) bar = parent.gameObject.AddComponent<CultivationPhaseBarUI>();
        bar.Build(title);
        return bar;
    }

    private void Build(UnityEngine.UI.Text title)
    {
        stageLabel = title;
        var titleRect = (RectTransform)title.transform;
        titleRect.anchorMin = new Vector2(0.012f, 0f);
        titleRect.anchorMax = new Vector2(0.38f, 1f);
        titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;
        title.alignment = TextAnchor.MiddleLeft;
        title.fontSize = 21;
        title.raycastTarget = false;

        tribulationButton = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        tribulationButton.targetGraphic = GetComponent<Graphic>() ?? title;
        tribulationButton.transition = Selectable.Transition.None;
        tribulationButton.navigation = new Navigation { mode = Navigation.Mode.None };
        tribulationButton.onClick.RemoveListener(TribulationModalUI.Open);
        tribulationButton.onClick.AddListener(TribulationModalUI.Open);

        var root = transform.Find("PhaseSegments") as RectTransform;
        if (root == null)
        {
            var go = new GameObject("PhaseSegments", typeof(RectTransform));
            root = (RectTransform)go.transform;
            root.SetParent(transform, false);
        }
        root.anchorMin = new Vector2(0.39f, 0.15f);
        root.anchorMax = new Vector2(0.99f, 0.85f);
        root.offsetMin = root.offsetMax = Vector2.zero;

        if (fills == null || fills.Length != 4) fills = new UnityEngine.UI.Image[4];
        if (labels == null || labels.Length != 4) labels = new UnityEngine.UI.Text[4];
        for (int i = 0; i < PhaseLabels.Length; i++)
        {
            var segment = root.Find("Phase_" + i) as RectTransform;
            if (segment == null)
            {
                var go = new GameObject("Phase_" + i, typeof(RectTransform), typeof(UnityEngine.UI.Image));
                segment = (RectTransform)go.transform;
                segment.SetParent(root, false);
            }
            segment.anchorMin = new Vector2(i * 0.25f, 0f);
            segment.anchorMax = new Vector2((i + 1) * 0.25f, 1f);
            segment.offsetMin = new Vector2(2f, 0f);
            segment.offsetMax = new Vector2(-2f, 0f);
            var background = segment.GetComponent<UnityEngine.UI.Image>();
            background.color = new Color(0.08f, 0.14f, 0.19f, 0.96f);
            background.raycastTarget = false;

            var fill = segment.Find("Fill") as RectTransform;
            if (fill == null)
            {
                var go = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                fill = (RectTransform)go.transform;
                fill.SetParent(segment, false);
            }
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            fills[i] = fill.GetComponent<UnityEngine.UI.Image>();
            fills[i].type = UnityEngine.UI.Image.Type.Simple;
            fills[i].raycastTarget = false;

            var label = segment.Find("Label") as RectTransform;
            if (label == null)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(UnityEngine.UI.Text));
                label = (RectTransform)go.transform;
                label.SetParent(segment, false);
            }
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = label.offsetMax = Vector2.zero;
            labels[i] = label.GetComponent<UnityEngine.UI.Text>();
            labels[i].font = title.font;
            labels[i].fontSize = 19;
            labels[i].alignment = TextAnchor.MiddleCenter;
            labels[i].text = PhaseLabels[i];
            labels[i].raycastTarget = false;
        }
    }

    public void Refresh(int stageIndex, float stageFraction, bool tribulationReady)
    {
        tribulationButton.interactable = tribulationReady;
        tribulationButton.targetGraphic.raycastTarget = tribulationReady;
        if (fills == null || labels == null || fills.Length != 4 || labels.Length != 4) return;
        SaveMigration.MapLegacyStage(stageIndex, out _, out int subLevel);
        int currentPhase = CultivationDisplay.PhaseIndex(subLevel);
        float phaseProgress = CultivationDisplay.PhaseProgress(stageIndex, stageFraction);
        for (int i = 0; i < 4; i++)
        {
            if (fills[i] == null || labels[i] == null) continue;
            float progress = i < currentPhase ? 1f : i == currentPhase ? phaseProgress : 0f;
            var fillRect = (RectTransform)fills[i].transform;
            fillRect.anchorMax = new Vector2(progress, 1f);
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            fills[i].color = i == 3 ? new Color(0.98f, 0.76f, 0.22f, 0.95f) :
                new Color(0.20f, 0.72f, 0.66f, 0.95f);
            labels[i].color = i == currentPhase ? Color.white :
                i < currentPhase ? new Color(0.82f, 0.96f, 0.88f) : new Color(0.52f, 0.60f, 0.65f);
            labels[i].text = i == 3 && tribulationReady ? "MỞ ĐỘ KIẾP" : PhaseLabels[i];
        }
    }
}

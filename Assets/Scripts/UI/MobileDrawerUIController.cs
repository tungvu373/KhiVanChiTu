// File: Assets/Scripts/UI/MobileDrawerUIController.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Quản lý Bảng Trượt Drawer (5 Tab) và Thanh Bottom Dock chuẩn Mobile Portrait.
/// Tích hợp tính năng đóng/mở toàn bộ phần dưới bằng mũi tên (▲/▼),
/// giúp đấu trường mở rộng 95% diện tích màn hình để tối ưu trải nghiệm ngự kiếm 1 tay.
/// </summary>
public class MobileDrawerUIController : MonoBehaviour
{
    public static MobileDrawerUIController Instance { get; private set; }

    [Header("Panels Drawer")]
    public RectTransform[] drawerPanels; // 0: KiemLo, 1: LuyenThe, 2: LuyenDan, 3: PhapKhi, 4: TranPhap
    public GameObject backdropOverlay;
    public Button[] dockButtons;
    public Button[] closeButtons;

    [Header("Collapsible Bottom Section")]
    public RectTransform mobileDock;
    public RectTransform dockToggleButton;
    public Text dockToggleArrowText;
    public Text dockToggleLabelText;
    public RectTransform playerHpBar;
    public bool IsDockCollapsed { get; private set; } = false;

    private int activeIndex = -1;
    private bool isTransitioning = false;
    private bool closeAfterTransition;
    private Coroutine dockSlideCoroutine;
    private Vector2 defaultDockPos = Vector2.zero;
    private Vector2 defaultHpPos = Vector2.zero;
    private bool hpPosCaptured = false;

    public bool IsBlockingGameplay => activeIndex >= 0 || isTransitioning;
    public bool IsAnyDrawerOpen => activeIndex >= 0;
    public int ActiveDrawerIndex => activeIndex;
    public event System.Action<int> OnDrawerStateChanged;
    public event System.Action<bool> OnDockCollapseChanged;
    private const float DURATION = 0.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Canvas.ForceUpdateCanvases();
        ResolveReferences();
        WireButtons();
        CloseAllInstant();
    }

    private void Start()
    {
        BuildDockToggleIfNeeded();
        AttachQuestBarCollapseIfNeeded();
    }

    private void ResolveReferences()
    {
        if (mobileDock == null)
        {
            var t = transform.Find("MobileDock");
            if (t != null) mobileDock = t.GetComponent<RectTransform>();
        }
        if (mobileDock != null)
        {
            defaultDockPos = mobileDock.anchoredPosition;
        }

        if (playerHpBar == null)
        {
            var t = transform.Find("PlayerHPBar");
            if (t != null) playerHpBar = t.GetComponent<RectTransform>();
        }
        if (playerHpBar != null && !hpPosCaptured)
        {
            defaultHpPos = playerHpBar.anchoredPosition;
            hpPosCaptured = true;
        }
    }

    private void BuildDockToggleIfNeeded()
    {
        if (mobileDock == null) ResolveReferences();
        if (mobileDock == null) return;

        if (dockToggleButton == null)
        {
            var existing = transform.Find("BottomDockToggle");
            if (existing != null) dockToggleButton = existing.GetComponent<RectTransform>();
        }

        if (dockToggleButton == null)
        {
            // Tạo nút mũi tên nổi đóng/mở thanh Bottom Dock
            var go = new GameObject("BottomDockToggle", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(transform, false);

            dockToggleButton = go.GetComponent<RectTransform>();
            dockToggleButton.anchorMin = new Vector2(0.5f, 0f);
            dockToggleButton.anchorMax = new Vector2(0.5f, 0f);
            dockToggleButton.pivot = new Vector2(0.5f, 0f);

            float dockHeight = mobileDock.rect.height > 0 ? mobileDock.rect.height : 192f;
            dockToggleButton.anchoredPosition = new Vector2(0f, dockHeight + 2f);
            dockToggleButton.sizeDelta = new Vector2(180f, 32f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

            var ol = go.GetComponent<Outline>();
            ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            // Text mũi tên và nhãn
            var txtObj = new GameObject("LabelText", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(go.transform, false);
            var txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            dockToggleLabelText = txtObj.GetComponent<Text>();
            dockToggleLabelText.font = UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dockToggleLabelText.text = "Thu Gọn Bảng Dưới";
            dockToggleLabelText.fontSize = 14;
            dockToggleLabelText.fontStyle = FontStyle.Bold;
            dockToggleLabelText.alignment = TextAnchor.MiddleCenter;
            dockToggleLabelText.color = new Color(0.96f, 0.78f, 0.26f, 1f);

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(ToggleDockCollapse);
        }
        else
        {
            var btn = dockToggleButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(ToggleDockCollapse);
                btn.onClick.AddListener(ToggleDockCollapse);
            }
            if (dockToggleLabelText == null)
            {
                dockToggleLabelText = dockToggleButton.GetComponentInChildren<Text>();
            }
        }

    }

    private void AttachQuestBarCollapseIfNeeded()
    {
        var questBar = transform.Find("TutorialTargetQuestBar");
        if (questBar != null && questBar.GetComponent<CollapsibleQuestBar>() == null)
        {
            questBar.gameObject.AddComponent<CollapsibleQuestBar>();
        }
    }

    public void ToggleDockCollapse()
    {
        IsDockCollapsed = !IsDockCollapsed;
        if (dockSlideCoroutine != null) StopCoroutine(dockSlideCoroutine);
        dockSlideCoroutine = StartCoroutine(AnimateDockCollapse(IsDockCollapsed));

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        OnDockCollapseChanged?.Invoke(IsDockCollapsed);
    }

    private IEnumerator AnimateDockCollapse(bool collapse)
    {
        float dockHeight = mobileDock != null && mobileDock.rect.height > 0 ? mobileDock.rect.height : 192f;
        Vector2 targetDockPos = collapse ? new Vector2(defaultDockPos.x, -dockHeight - 10f) : defaultDockPos;
        Vector2 targetBtnPos = collapse ? new Vector2(0f, 12f) : new Vector2(0f, dockHeight + 2f);
        Vector2 targetHpPos = collapse ? new Vector2(defaultHpPos.x, 24f) : defaultHpPos;

        if (collapse && activeIndex != -1)
        {
            CloseActiveDrawer();
        }

        Vector2 startDockPos = mobileDock != null ? mobileDock.anchoredPosition : Vector2.zero;
        Vector2 startBtnPos = dockToggleButton != null ? dockToggleButton.anchoredPosition : Vector2.zero;
        Vector2 startHpPos = playerHpBar != null ? playerHpBar.anchoredPosition : Vector2.zero;

        float elapsed = 0f;
        while (elapsed < DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / DURATION);

            if (mobileDock != null) mobileDock.anchoredPosition = Vector2.Lerp(startDockPos, targetDockPos, t);
            if (dockToggleButton != null) dockToggleButton.anchoredPosition = Vector2.Lerp(startBtnPos, targetBtnPos, t);
            if (playerHpBar != null && hpPosCaptured) playerHpBar.anchoredPosition = Vector2.Lerp(startHpPos, targetHpPos, t);

            yield return null;
        }

        if (mobileDock != null) mobileDock.anchoredPosition = targetDockPos;
        if (dockToggleButton != null) dockToggleButton.anchoredPosition = targetBtnPos;
        if (playerHpBar != null && hpPosCaptured) playerHpBar.anchoredPosition = targetHpPos;

        if (dockToggleLabelText != null)
        {
            dockToggleLabelText.text = collapse ? "Mở Bảng Chức Năng" : "Thu Gọn Bảng Dưới";
        }
        dockSlideCoroutine = null;
    }

    private void WireButtons()
    {
        if (dockButtons != null)
        {
            for (int i = 0; i < dockButtons.Length; i++)
            {
                int index = i;
                if (dockButtons[i] != null)
                {
                    dockButtons[i].onClick.RemoveAllListeners();
                    dockButtons[i].onClick.AddListener(() => ToggleDrawer(index));
                }
            }
        }
        if (closeButtons != null)
        {
            foreach (var button in closeButtons)
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(CloseActiveDrawer);
                }
        }
        if (backdropOverlay != null)
        {
            var button = backdropOverlay.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(CloseActiveDrawer);
            }
        }
    }

    public bool IsDrawerUnlocked(int index, out string reason)
    {
        reason = "Đã mở khóa";
        if (FeatureUnlockService.Instance == null) return true;

        FeatureId fid;
        switch (index)
        {
            case 0: fid = FeatureId.SwordMerge; break;
            case 1: fid = FeatureId.BodyCultivation; break;
            case 2: fid = FeatureId.Alchemy; break;
            case 3: fid = FeatureId.Artifact; break;
            case 4: fid = FeatureId.Formation; break;
            default: return true;
        }

        if (!FeatureUnlockService.Instance.IsFeatureUnlocked(fid))
        {
            reason = FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(fid);
            return false;
        }
        return true;
    }

    public void UpdateDockVisuals()
    {
        if (dockButtons == null) return;
        for (int i = 0; i < dockButtons.Length; i++)
        {
            if (dockButtons[i] == null) continue;
            var img = dockButtons[i].GetComponent<Image>();
            var txt = dockButtons[i].GetComponentInChildren<Text>();
            bool isActive = (i == activeIndex);
            bool isUnlocked = IsDrawerUnlocked(i, out _);

            var jadeTheme = GetComponentInParent<JadeUITheme>();
            if (jadeTheme != null)
            {
                if (img != null) img.color = isUnlocked ? Color.white : new Color(.48f, .54f, .51f, .9f);
                if (txt != null) txt.color = !isUnlocked ? new Color(.59f, .65f, .62f) : (isActive ? JadeUITheme.Gold : JadeUITheme.Ivory);
                var selected = dockButtons[i].transform.Find("JadeSelected");
                if (selected != null) selected.gameObject.SetActive(isActive);
                var icon = dockButtons[i].transform.Find("JadeNavIcon");
                if (icon != null) icon.GetComponent<Image>().color = isUnlocked ? Color.white : new Color(.55f, .6f, .56f);
                continue;
            }

            if (img != null)
            {
                if (!isUnlocked)
                {
                    img.color = new Color(0.04f, 0.06f, 0.09f, 0.75f);
                }
                else
                {
                    img.color = isActive 
                        ? new Color(0.96f, 0.78f, 0.26f, 0.18f) // Gold glowing tint
                        : new Color(0.08f, 0.12f, 0.18f, 0.95f); // Dark midnight
                }
            }
            if (txt != null)
            {
                if (!isUnlocked)
                {
                    txt.color = new Color(0.35f, 0.40f, 0.48f, 0.70f); // Dimmed locked gray
                }
                else
                {
                    txt.color = isActive 
                        ? new Color(0.96f, 0.78f, 0.26f, 1f) // Gold text
                        : new Color(0.75f, 0.82f, 0.90f, 1f); // Crisp slate white
                }
            }
        }
    }

    private static float ClosedY(RectTransform panel) => -Mathf.Max(1f, panel.rect.height);

    public void OpenDrawer(int index)
    {
        if (isTransitioning || drawerPanels == null || index < 0 || index >= drawerPanels.Length || drawerPanels[index] == null) return;
        if (!IsDrawerUnlocked(index, out string reason))
        {
            GameLogger.Instance?.Log(reason, Color.yellow);
            return;
        }
        if (IsDockCollapsed)
        {
            ToggleDockCollapse();
        }
        if (activeIndex == index) return;
        StartCoroutine(TransitionTo(index));
    }

    public void ToggleDrawer(int index)
    {
        if (isTransitioning || drawerPanels == null || index < 0 || index >= drawerPanels.Length || drawerPanels[index] == null) return;
        if (!IsDrawerUnlocked(index, out string reason))
        {
            GameLogger.Instance?.Log(reason, Color.yellow);
            return;
        }

        if (IsDockCollapsed)
        {
            ToggleDockCollapse();
        }

        if (activeIndex == index)
        {
            CloseActiveDrawer();
        }
        else
        {
            StartCoroutine(TransitionTo(index));
        }
    }

    public void CloseActiveDrawer()
    {
        if (isTransitioning) closeAfterTransition = true;
        else if (activeIndex != -1) StartCoroutine(SlideOut(activeIndex));
    }

    private IEnumerator TransitionTo(int targetIndex)
    {
        closeAfterTransition = false;
        isTransitioning = true;
        if (backdropOverlay != null) backdropOverlay.SetActive(true);
        if (dockToggleButton != null) dockToggleButton.gameObject.SetActive(false);

        if (activeIndex != -1)
        {
            yield return StartCoroutine(SlidePanel(drawerPanels[activeIndex], 0f, ClosedY(drawerPanels[activeIndex]), DURATION * 0.5f));
            drawerPanels[activeIndex].gameObject.SetActive(false);
        }

        drawerPanels[targetIndex].gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        yield return StartCoroutine(SlidePanel(drawerPanels[targetIndex], ClosedY(drawerPanels[targetIndex]), 0f, DURATION));

        activeIndex = targetIndex;
        UpdateDockVisuals();
        OnDrawerStateChanged?.Invoke(activeIndex);
        isTransitioning = false;
        if (closeAfterTransition) CloseActiveDrawer();
    }

    private IEnumerator SlideOut(int targetIndex)
    {
        isTransitioning = true;
        yield return StartCoroutine(SlidePanel(drawerPanels[targetIndex], 0f, ClosedY(drawerPanels[targetIndex]), DURATION));
        drawerPanels[targetIndex].gameObject.SetActive(false);

        if (backdropOverlay != null) backdropOverlay.SetActive(false);
        if (dockToggleButton != null) dockToggleButton.gameObject.SetActive(true);
        activeIndex = -1;
        UpdateDockVisuals();
        OnDrawerStateChanged?.Invoke(activeIndex);
        isTransitioning = false;
        closeAfterTransition = false;
    }

    private IEnumerator SlidePanel(RectTransform panel, float startY, float endY, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / time);
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, Mathf.Lerp(startY, endY, t));
            yield return null;
        }
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, endY);
    }

    private void CloseAllInstant()
    {
        if (backdropOverlay != null) backdropOverlay.SetActive(false);
        if (drawerPanels == null) return;
        for (int i = 0; i < drawerPanels.Length; i++)
        {
            if (drawerPanels[i] != null)
            {
                drawerPanels[i].anchoredPosition = new Vector2(drawerPanels[i].anchoredPosition.x, ClosedY(drawerPanels[i]));
                drawerPanels[i].gameObject.SetActive(false);
            }
        }
        activeIndex = -1;
        UpdateDockVisuals();
        OnDrawerStateChanged?.Invoke(activeIndex);
    }
}

// File: Assets/Scripts/UI/MobileDrawerUIController.cs
using UnityEngine;
using System.Collections;

public class MobileDrawerUIController : MonoBehaviour
{
    public static MobileDrawerUIController Instance { get; private set; }

    [Header("Panels Drawer")]
    public RectTransform[] drawerPanels; // 0: KiemLo, 1: LuyenThe, 2: LuyenDan, 3: PhapKhi, 4: TranPhap
    public GameObject backdropOverlay;
    public UnityEngine.UI.Button[] dockButtons;
    public UnityEngine.UI.Button[] closeButtons;

    private int activeIndex = -1;
    private bool isTransitioning = false;
    public bool IsBlockingGameplay => activeIndex >= 0 || isTransitioning;
    public bool IsAnyDrawerOpen => activeIndex >= 0;
    public int ActiveDrawerIndex => activeIndex;
    public event System.Action<int> OnDrawerStateChanged;
    private const float DURATION = 0.25f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Canvas.ForceUpdateCanvases();
        WireButtons();
        CloseAllInstant();
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
            var button = backdropOverlay.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(CloseActiveDrawer);
            }
        }
    }

    public void UpdateDockVisuals()
    {
        if (dockButtons == null) return;
        for (int i = 0; i < dockButtons.Length; i++)
        {
            if (dockButtons[i] == null) continue;
            var img = dockButtons[i].GetComponent<UnityEngine.UI.Image>();
            var txt = dockButtons[i].GetComponentInChildren<UnityEngine.UI.Text>();
            bool isActive = (i == activeIndex);

            if (img != null)
            {
                img.color = isActive 
                    ? new Color(0.96f, 0.78f, 0.26f, 0.18f) // Gold glowing tint
                    : new Color(0.08f, 0.12f, 0.18f, 0.95f); // Dark midnight
            }
            if (txt != null)
            {
                txt.color = isActive 
                    ? new Color(0.96f, 0.78f, 0.26f, 1f) // Gold text
                    : new Color(0.55f, 0.62f, 0.72f, 1f); // Dimmed slate
            }
        }
    }

    private static float ClosedY(RectTransform panel) => -Mathf.Max(1f, panel.rect.height);

    public void OpenDrawer(int index)
    {
        if (isTransitioning || drawerPanels == null || index < 0 || index >= drawerPanels.Length || drawerPanels[index] == null) return;
        if (activeIndex == index) return;
        StartCoroutine(TransitionTo(index));
    }

    public void ToggleDrawer(int index)
    {
        if (isTransitioning || drawerPanels == null || index < 0 || index >= drawerPanels.Length || drawerPanels[index] == null) return;

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
        if (activeIndex == -1 || isTransitioning) return;
        StartCoroutine(SlideOut(activeIndex));
    }

    private IEnumerator TransitionTo(int targetIndex)
    {
        isTransitioning = true;
        if (backdropOverlay != null) backdropOverlay.SetActive(true);

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
    }

    private IEnumerator SlideOut(int targetIndex)
    {
        isTransitioning = true;
        yield return StartCoroutine(SlidePanel(drawerPanels[targetIndex], 0f, ClosedY(drawerPanels[targetIndex]), DURATION));
        drawerPanels[targetIndex].gameObject.SetActive(false);

        if (backdropOverlay != null) backdropOverlay.SetActive(false);
        activeIndex = -1;
        UpdateDockVisuals();
        OnDrawerStateChanged?.Invoke(activeIndex);
        isTransitioning = false;
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

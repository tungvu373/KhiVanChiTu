using UnityEngine;

/// <summary>A thumb-sized expandable route to the existing HUD actions.</summary>
public sealed class JadeSecondaryMenu : MonoBehaviour
{
    public UnityEngine.UI.Button toggle;
    public UnityEngine.UI.Button[] actions;
    public GameObject dismissArea;
    public bool IsOpen { get; private set; }
    private void Awake()
    {
        // A portrait prototype owns the replacement menu, including these existing shortcut objects.
        if (GetComponentInChildren<PortraitHudController>(true) != null) return;
        toggle.onClick.AddListener(Toggle);
        if (dismissArea != null) dismissArea.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Close);
        foreach (var action in actions) if (action != null) action.onClick.AddListener(Close);
        Close();
    }
    private void LateUpdate()
    {
        if (!IsOpen) return;
        // Some legacy HUD handlers rebuild their onClick listeners; modal state is authoritative.
        if ((StorageModalUI.Instance != null && StorageModalUI.Instance.IsModalOpen) ||
            (DaoAnModalUI.Instance != null && DaoAnModalUI.Instance.IsModalOpen) ||
            (SecretRealmModalUI.Instance != null && SecretRealmModalUI.Instance.IsModalOpen) ||
            (AchievementModalUI.Instance != null && AchievementModalUI.Instance.IsModalOpen)) Close();
    }
    public void Toggle() => SetOpen(!IsOpen);
    public void Close() => SetOpen(false);
    private void SetOpen(bool open)
    {
        IsOpen = open;
        if (dismissArea != null) dismissArea.SetActive(open);
        foreach (var action in actions) if (action != null) action.gameObject.SetActive(open);
    }
}

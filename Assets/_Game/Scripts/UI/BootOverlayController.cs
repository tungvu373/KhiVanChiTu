using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public sealed class BootOverlayController : MonoBehaviour
    {
        public static BootOverlayController Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private VisualElement _loadingOverlay;
        private VisualElement _loadingProgressFill;
        private Label _loadingStatusLabel;
        private VisualElement _afkDimVeil;
        private VisualElement _toastPill;
        private Label _toastText;

        private VisualElement _desktopInputModal;
        private Label _desktopInputTitle;
        private TextField _desktopInputField;
        private Action<string> _desktopInputCallback;

        private float _toastTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            BindVisuals();
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.OnLoadingOverlayChanged += HandleLoadingChanged;
            if (GameplayEngine.Instance != null)
                GameplayEngine.Instance.OnToastMessage += ShowToast;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += RefreshAfkVeil;
            RefreshAfkVeil();
        }

        private void OnDisable()
        {
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.OnLoadingOverlayChanged -= HandleLoadingChanged;
            if (GameplayEngine.Instance != null)
                GameplayEngine.Instance.OnToastMessage -= ShowToast;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= RefreshAfkVeil;
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f && _toastPill != null)
                    _toastPill.style.display = DisplayStyle.None;
            }
        }

        public void ShowToast(string message)
        {
            if (_toastPill == null || _toastText == null || string.IsNullOrEmpty(message)) return;
            _toastText.text = message;
            _toastPill.style.display = DisplayStyle.Flex;
            _toastTimer = 2.6f;
        }

        public void PromptTextInput(string title, string initialText, int maxLength, Action<string> onConfirmed)
        {
            if (PlatformWebBridge.Instance != null &&
                PlatformWebBridge.Instance.TryOpenTouchNativeInput(title, initialText, maxLength, onConfirmed))
            {
                return;
            }

            if (_desktopInputModal == null || _desktopInputField == null)
            {
                onConfirmed?.Invoke(initialText);
                return;
            }

            _desktopInputTitle.text = title;
            _desktopInputField.maxLength = maxLength;
            _desktopInputField.SetValueWithoutNotify(initialText ?? "");
            _desktopInputCallback = onConfirmed;
            _desktopInputModal.style.display = DisplayStyle.Flex;
            _desktopInputField.Focus();
        }

        private void BindVisuals()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null && uiDocument.panelSettings == null)
                uiDocument.panelSettings = Resources.Load<PanelSettings>("RemakePanelSettings");
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            if (uiDocument.rootVisualElement.childCount == 0 && uiDocument.visualTreeAsset != null)
                uiDocument.visualTreeAsset.CloneTree(uiDocument.rootVisualElement);
            var root = uiDocument.rootVisualElement;

            _loadingOverlay = root.Q("loading-overlay");
            _loadingProgressFill = root.Q("loading-progress-fill");
            _loadingStatusLabel = root.Q<Label>("loading-status-label");
            _afkDimVeil = root.Q("afk-dim-veil");
            _toastPill = root.Q("toast-pill");
            _toastText = root.Q<Label>("toast-text");

            var bgSlot = root.Q("loading-bg-art");
            Texture2D loadingTex = CodexArtLoader.LoadCodexTexture("Backgrounds/bg_loading_screen");
            if (loadingTex != null && bgSlot != null)
                bgSlot.style.backgroundImage = new StyleBackground(loadingTex);

            _desktopInputModal = root.Q("desktop-input-modal");
            _desktopInputTitle = root.Q<Label>("desktop-input-title");
            _desktopInputField = root.Q<TextField>("desktop-input-field");
            root.Q<Button>("btn-desktop-input-cancel")?.RegisterCallback<ClickEvent>(_ =>
            {
                _desktopInputModal.style.display = DisplayStyle.None;
                _desktopInputCallback = null;
            });
            root.Q<Button>("btn-desktop-input-ok")?.RegisterCallback<ClickEvent>(_ =>
            {
                _desktopInputModal.style.display = DisplayStyle.None;
                var cb = _desktopInputCallback;
                _desktopInputCallback = null;
                cb?.Invoke(_desktopInputField != null ? _desktopInputField.value : "");
            });
        }

        private void HandleLoadingChanged(bool visible, float progress, string status)
        {
            if (_loadingOverlay == null) return;
            _loadingOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_loadingProgressFill != null)
                _loadingProgressFill.style.width = Length.Percent(Mathf.Clamp01(progress) * 100f);
            if (_loadingStatusLabel != null && !string.IsNullOrEmpty(status))
                _loadingStatusLabel.text = status;
        }

        private void RefreshAfkVeil()
        {
            if (_afkDimVeil == null || GraphicsSettingsService.Instance == null) return;
            bool show = GraphicsSettingsService.Instance.Settings.afkDimMode &&
                        SceneFlowController.Instance != null &&
                        SceneFlowController.Instance.IsInGame;
            _afkDimVeil.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public enum RemakeFeedbackKind
    {
        GoldSuccess,
        JadeReward,
        CrimsonAlert
    }

    public sealed class BootOverlayController : MonoBehaviour
    {
        public static BootOverlayController Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private sealed class BannerSlot
        {
            public VisualElement root;
            public Label tag;
            public Label msg;
            public float remaining;
            public float duration;
        }

        private VisualElement _loadingOverlay;
        private VisualElement _loadingProgressFill;
        private Label _loadingStatusLabel;
        private VisualElement _afkDimVeil;
        private VisualElement _toastStack;
        private readonly List<BannerSlot> _bannerSlots = new List<BannerSlot>();

        private VisualElement _desktopInputModal;
        private Label _desktopInputTitle;
        private TextField _desktopInputField;
        private Action<string> _desktopInputCallback;

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
            {
                GameplayEngine.Instance.OnToastMessage += ShowToast;
                GameplayEngine.Instance.OnWaveStarted += HandleWaveStarted;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += RefreshAfkVeil;
            RefreshAfkVeil();
        }

        private void OnDisable()
        {
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.OnLoadingOverlayChanged -= HandleLoadingChanged;
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnToastMessage -= ShowToast;
                GameplayEngine.Instance.OnWaveStarted -= HandleWaveStarted;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= RefreshAfkVeil;
        }

        public bool SuppressAmbientToasts { get; set; }
        public bool IsMiniModeActive { get; set; }

        private void HandleWaveStarted(int waveIdx, int totalWaves, string bannerText)
        {
            if (string.IsNullOrEmpty(bannerText)) return;
            if (SuppressAmbientToasts || IsMiniModeActive) return;
            bool isBossOrElite = waveIdx >= totalWaves && (bannerText.Contains("Thủ Lĩnh") || bannerText.Contains("Tinh Anh"));
            ShowFeedback(bannerText, isBossOrElite ? RemakeFeedbackKind.GoldSuccess : RemakeFeedbackKind.JadeReward, $"ĐỢT {waveIdx}/{totalWaves}");
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _bannerSlots.Count; i++)
            {
                var slot = _bannerSlots[i];
                if (slot.remaining <= 0f) continue;
                slot.remaining -= dt;
                if (slot.remaining <= 0f)
                {
                    slot.remaining = 0f;
                    slot.root.style.display = DisplayStyle.None;
                    continue;
                }
                float elapsed = slot.duration - slot.remaining;
                float scaleVal = elapsed < 0.14f ? Mathf.Lerp(1.07f, 1.0f, elapsed / 0.14f) : 1.0f;
                float alphaVal = slot.remaining < 0.45f ? Mathf.Clamp01(slot.remaining / 0.45f) : 1.0f;
                slot.root.style.scale = new Scale(new Vector3(scaleVal, scaleVal, 1f));
                slot.root.style.opacity = alphaVal;
            }
        }

        public void ShowToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string lo = message.ToLowerInvariant();
            bool isAmbientCombatLog = lo.StartsWith("nhặt phôi") || lo.StartsWith("hạ gục thủ lĩnh") ||
                lo.StartsWith("vượt ải") || lo.StartsWith("đợt ");
            if ((SuppressAmbientToasts || IsMiniModeActive) && isAmbientCombatLog) return;
            if (IsMiniModeActive && !lo.Contains("đột phá") && !lo.Contains("độ kiếp") && !lo.Contains("trọng thương") && !lo.Contains("hết "))
                return;
            RemakeFeedbackKind kind = ClassifyMessage(message, out string badge);
            ShowFeedback(message, kind, badge);
        }

        public void ShowFeedback(string message, RemakeFeedbackKind kind, string badgeLabel = null)
        {
            if (_toastStack == null || _bannerSlots.Count == 0 || string.IsNullOrEmpty(message)) return;

            // Reuse an inactive slot, or recycle the oldest active slot
            BannerSlot target = null;
            float lowestRem = float.MaxValue;
            for (int i = 0; i < _bannerSlots.Count; i++)
            {
                if (_bannerSlots[i].remaining <= 0f) { target = _bannerSlots[i]; break; }
                if (_bannerSlots[i].remaining < lowestRem) { lowestRem = _bannerSlots[i].remaining; target = _bannerSlots[i]; }
            }
            if (target == null) return;

            target.root.RemoveFromClassList("toast-gold");
            target.root.RemoveFromClassList("toast-jade");
            target.root.RemoveFromClassList("toast-danger");

            string cssClass = kind == RemakeFeedbackKind.GoldSuccess ? "toast-gold"
                : kind == RemakeFeedbackKind.CrimsonAlert ? "toast-danger" : "toast-jade";
            target.root.AddToClassList(cssClass);
            target.tag.text = !string.IsNullOrEmpty(badgeLabel) ? badgeLabel
                : kind == RemakeFeedbackKind.GoldSuccess ? "THÀNH CÔNG"
                : kind == RemakeFeedbackKind.CrimsonAlert ? "CHÚ Ý" : "NHẬN";
            target.msg.text = message;
            target.duration = kind == RemakeFeedbackKind.GoldSuccess ? 3.4f : 2.8f;
            target.remaining = target.duration;
            target.root.style.display = DisplayStyle.Flex;
            target.root.style.opacity = 1f;
            target.root.style.scale = new Scale(new Vector3(1.07f, 1.07f, 1f));
            target.root.BringToFront();
        }

        private static RemakeFeedbackKind ClassifyMessage(string message, out string badge)
        {
            string lo = message.ToLowerInvariant();
            if (lo.Contains("thất bại") || lo.Contains("nổ lò") || lo.Contains("thiếu") ||
                lo.Contains("không đủ") || lo.Contains("cần ") || lo.Contains("khóa") ||
                lo.Contains("hết ") || lo.Contains("thất thủ") || lo.Contains("không thể") ||
                lo.Contains("chưa đủ") || lo.Contains("chưa đạt") || lo.Contains("trống"))
            {
                badge = lo.Contains("thất bại") || lo.Contains("nổ lò") ? "THẤT BẠI" : "CHÚ Ý";
                return RemakeFeedbackKind.CrimsonAlert;
            }
            if (lo.Contains("thành công") || lo.Contains("độ kiếp") || lo.Contains("thăng cấp") ||
                lo.Contains("nâng ") || lo.Contains("cường hóa") || lo.Contains("hoàn thành") ||
                lo.Contains("mở khóa") || lo.Contains("đột phá") || lo.Contains("thức tỉnh") ||
                lo.Contains("vượt tháp") || lo.Contains("kỳ ngộ") || lo.Contains("cực phẩm") ||
                lo.Contains("khai trận") || lo.Contains("đặt trận") || lo.Contains("tẩy luyện"))
            {
                badge = lo.Contains("độ kiếp") || lo.Contains("đột phá") ? "ĐỘT PHÁ"
                    : lo.Contains("thăng cấp") || lo.Contains("nâng ") || lo.Contains("cường hóa") ? "THĂNG CẤP"
                    : "THÀNH CÔNG";
                return RemakeFeedbackKind.GoldSuccess;
            }
            badge = lo.Contains("nhặt") || lo.Contains("nhận") || lo.Contains("bổng lộc") ? "NHẬN"
                : lo.Contains("thiên kiếp") ? "THIÊN KIẾP"
                : lo.Contains("bế quan") ? "BẾ QUAN"
                : lo.Contains("khai lò") ? "LUYỆN ĐAN"
                : "THÔNG BÁO";
            return RemakeFeedbackKind.JadeReward;
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
            _toastStack = root.Q("toast-stack");

            if (_toastStack != null && _bannerSlots.Count == 0)
            {
                _toastStack.Clear();
                for (int i = 0; i < 3; i++)
                {
                    var bRoot = new VisualElement { pickingMode = PickingMode.Ignore };
                    bRoot.AddToClassList("toast-banner");
                    bRoot.AddToClassList("toast-jade");
                    bRoot.style.display = DisplayStyle.None;

                    var tag = new Label("NHẬN") { pickingMode = PickingMode.Ignore };
                    tag.AddToClassList("toast-tag");
                    var msg = new Label("") { pickingMode = PickingMode.Ignore };
                    msg.AddToClassList("toast-msg");

                    bRoot.Add(tag);
                    bRoot.Add(msg);
                    _toastStack.Add(bRoot);
                    _bannerSlots.Add(new BannerSlot { root = bRoot, tag = tag, msg = msg });
                }
            }

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

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

    public readonly struct RemakeNotifyMeta
    {
        public readonly string CssClass;
        public readonly string BadgeLabel;
        public readonly string CodexIconKey;
        public readonly string ReusedIconKey;
        public readonly bool UseCalligraphy;
        public readonly RemakeNotifyZone DefaultZone;

        public RemakeNotifyMeta(
            string cssClass,
            string badgeLabel,
            string codexIconKey,
            string reusedIconKey,
            bool useCalligraphy,
            RemakeNotifyZone defaultZone)
        {
            CssClass = cssClass;
            BadgeLabel = badgeLabel;
            CodexIconKey = codexIconKey;
            ReusedIconKey = reusedIconKey;
            UseCalligraphy = useCalligraphy;
            DefaultZone = defaultZone;
        }
    }

    public sealed class BootOverlayController : MonoBehaviour
    {
        public static BootOverlayController Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private sealed class BannerSlot
        {
            public VisualElement root;
            public VisualElement icon;
            public Label tag;
            public Label msg;
            public float remaining;
            public float duration;
            public string baseMessage;
            public int coalesceCount;
            public float lastTouchTime;
        }

        private VisualElement _loadingOverlay;
        private VisualElement _loadingProgressFill;
        private Label _loadingStatusLabel;
        private VisualElement _afkDimVeil;
        private VisualElement _toastStack;
        private VisualElement _combatTickerStack;
        private readonly List<BannerSlot> _bannerSlots = new List<BannerSlot>();
        private readonly List<BannerSlot> _tickerSlots = new List<BannerSlot>();

        private VisualElement _desktopInputModal;
        private Label _desktopInputTitle;
        private TextField _desktopInputField;
        private Action<string> _desktopInputCallback;

        private bool _suppressAmbientToasts;
        private bool _isMiniModeActive;

        /// <summary>
        /// Zone 3 callback invoked by InGameHudController to render drawer-scoped feedback inside modal-sheet.
        /// Args: (category, cssClass, badgeLabel, message, codexIconKey, reusedIconKey).
        /// </summary>
        public event Action<RemakeNotifyCategory, string, string, string, string, string> OnDrawerInlineRequested;

        public bool SuppressAmbientToasts
        {
            get => _suppressAmbientToasts;
            set
            {
                if (_suppressAmbientToasts == value) return;
                _suppressAmbientToasts = value;
                SyncTickerVisibility();
            }
        }

        public bool IsMiniModeActive
        {
            get => _isMiniModeActive;
            set
            {
                if (_isMiniModeActive == value) return;
                _isMiniModeActive = value;
                SyncTickerVisibility();
            }
        }

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
                GameplayEngine.Instance.OnCategoryNotification += HandleCategoryNotification;
                GameplayEngine.Instance.OnWaveStarted += HandleWaveStarted;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += RefreshAfkVeil;
            RefreshAfkVeil();
            SyncTickerVisibility();
        }

        private void OnDisable()
        {
            if (SceneFlowController.Instance != null)
                SceneFlowController.Instance.OnLoadingOverlayChanged -= HandleLoadingChanged;
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnCategoryNotification -= HandleCategoryNotification;
                GameplayEngine.Instance.OnWaveStarted -= HandleWaveStarted;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= RefreshAfkVeil;
        }

        public static bool ShouldSuppressWaveToast(int waveIdx, int totalWaves, string bannerText)
        {
            if (string.IsNullOrEmpty(bannerText)) return true;
            // Suppress routine waves (Đợt 1/3, Đợt 2/3) — already visible on the Stage Pill!
            bool isBossOrElite = waveIdx >= totalWaves &&
                (bannerText.IndexOf("Thủ Lĩnh", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 bannerText.IndexOf("Tinh Anh", StringComparison.OrdinalIgnoreCase) >= 0);
            return !isBossOrElite;
        }

        private void HandleWaveStarted(int waveIdx, int totalWaves, string bannerText)
        {
            if (ShouldSuppressWaveToast(waveIdx, totalWaves, bannerText)) return;
            if (_suppressAmbientToasts || _isMiniModeActive) return;
            ShowNotification(RemakeNotifyCategory.BossCombat, bannerText, RemakeNotifyZone.CombatTicker, false);
        }

        private void HandleCategoryNotification(RemakeNotifyCategory category, string message, RemakeNotifyZone zone, bool isError)
        {
            ShowNotification(category, message, zone, isError);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            TickSlots(_bannerSlots, dt, 1.06f);
            TickSlots(_tickerSlots, dt, 1.04f);
        }

        private static void TickSlots(List<BannerSlot> slots, float dt, float entryScale)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.remaining <= 0f) continue;
                slot.remaining -= dt;
                if (slot.remaining <= 0f)
                {
                    slot.remaining = 0f;
                    slot.coalesceCount = 0;
                    slot.baseMessage = null;
                    slot.root.style.display = DisplayStyle.None;
                    continue;
                }
                float elapsed = slot.duration - slot.remaining;
                float scaleVal = elapsed < 0.14f ? Mathf.Lerp(entryScale, 1.0f, elapsed / 0.14f) : 1.0f;
                float alphaVal = slot.remaining < 0.40f ? Mathf.Clamp01(slot.remaining / 0.40f) : 1.0f;
                slot.root.style.scale = new Scale(new Vector3(scaleVal, scaleVal, 1f));
                slot.root.style.opacity = alphaVal;
            }
        }

        private void SyncTickerVisibility()
        {
            if (_toastStack != null)
            {
                // Keep modal headers 100% unobstructed when a Drawer is open (drawer actions render in Zone 3)
                _toastStack.style.display = _suppressAmbientToasts ? DisplayStyle.None : DisplayStyle.Flex;
            }
            if (_combatTickerStack == null) return;
            bool hide = _suppressAmbientToasts || _isMiniModeActive;
            _combatTickerStack.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            if (hide)
            {
                for (int i = 0; i < _tickerSlots.Count; i++)
                {
                    _tickerSlots[i].remaining = 0f;
                    _tickerSlots[i].coalesceCount = 0;
                    _tickerSlots[i].baseMessage = null;
                    _tickerSlots[i].root.style.display = DisplayStyle.None;
                }
            }
        }

        public static RemakeNotifyMeta ResolveMeta(RemakeNotifyCategory category, bool isError = false)
        {
            if (isError || category == RemakeNotifyCategory.Error)
            {
                string label = category == RemakeNotifyCategory.Breakthrough || category == RemakeNotifyCategory.Tribulation
                    ? "THẤT BẠI"
                    : category == RemakeNotifyCategory.Alchemy ? "NỔ LÒ"
                    : category == RemakeNotifyCategory.SwordPath ? "RÈN THẤT BẠI"
                    : category == RemakeNotifyCategory.BossCombat || category == RemakeNotifyCategory.CombatWave ? "TRỌNG THƯƠNG"
                    : "CHÚ Ý";
                return new RemakeNotifyMeta("toast-danger", label, "", "", false, RemakeNotifyZone.GrandTop);
            }

            switch (category)
            {
                case RemakeNotifyCategory.Breakthrough:
                    return new RemakeNotifyMeta("toast-gold", "ĐỘT PHÁ", "HUD/nav_breakthrough", "", true, RemakeNotifyZone.GrandTop);
                case RemakeNotifyCategory.Tribulation:
                    return new RemakeNotifyMeta("toast-gold", "THIÊN KIẾP", "HUD/nav_breakthrough", "", true, RemakeNotifyZone.GrandTop);
                case RemakeNotifyCategory.KyNgo:
                    return new RemakeNotifyMeta("toast-gold", "KỲ NGỘ", "Stages/event_fortuitous_orb", "", true, RemakeNotifyZone.GrandTop);
                case RemakeNotifyCategory.BossCombat:
                    return new RemakeNotifyMeta("toast-gold", "THỦ LĨNH", "HUD/icon_boss_shield", "GoldenBell", false, RemakeNotifyZone.CombatTicker);
                case RemakeNotifyCategory.CombatWave:
                    return new RemakeNotifyMeta("toast-azure", "CHIẾN ĐẤU", "HUD/nav_map_stage", "", false, RemakeNotifyZone.CombatTicker);
                case RemakeNotifyCategory.LootDrop:
                    return new RemakeNotifyMeta("toast-jade", "NHẶT ĐỒ", "", "RouteMerge", false, RemakeNotifyZone.CombatTicker);
                case RemakeNotifyCategory.SwordPath:
                    return new RemakeNotifyMeta("toast-jade", "KIẾM LỘ", "", "RouteMerge", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Equipment:
                    return new RemakeNotifyMeta("toast-jade", "TRANG BỊ", "", "RouteArtifact", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.QuestDiary:
                    return new RemakeNotifyMeta("toast-jade", "TIÊN LỘ", "", "RouteMilestones", false, RemakeNotifyZone.GrandTop);
                case RemakeNotifyCategory.Codex:
                    return new RemakeNotifyMeta("toast-jade", "ĐỒ GIÁM", "HUD/nav_codex", "RouteAchievements", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Companion:
                    return new RemakeNotifyMeta("toast-jade", "LINH THÚ", "Companions/pet_azure_crane", "SpiritLotus", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Skill:
                    return new RemakeNotifyMeta("toast-azure", "KỸ NĂNG", "HUD/nav_skills", "SkillThunder", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.ImmortalGate:
                    return new RemakeNotifyMeta("toast-azure", "TIÊN MÔN", "HUD/nav_immortal_gate", "", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Social:
                    return new RemakeNotifyMeta("toast-azure", "ĐẠO HỮU", "HUD/nav_friends", "", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Alchemy:
                    return new RemakeNotifyMeta("toast-violet", "LUYỆN ĐAN", "", "RouteAlchemy", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.DaoVan:
                    return new RemakeNotifyMeta("toast-violet", "ĐẠO VẬN", "DaoVan/daovan_core_node", "RouteTalents", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Formation:
                    return new RemakeNotifyMeta("toast-violet", "TRẬN PHÁP", "Formations/formation_thunder", "RouteFormation", false, RemakeNotifyZone.DrawerInline);
                case RemakeNotifyCategory.Warning:
                    return new RemakeNotifyMeta("toast-danger", "CẢNH BÁO", "", "", false, RemakeNotifyZone.GrandTop);
                default:
                    return new RemakeNotifyMeta("toast-jade", "THÔNG BÁO", "", "", false, RemakeNotifyZone.GrandTop);
            }
        }

        public static RemakeNotifyZone ResolveEffectiveZone(
            RemakeNotifyCategory category,
            RemakeNotifyZone requested,
            bool drawerOpen,
            bool isError)
        {
            if (requested != RemakeNotifyZone.Auto)
            {
                if (requested == RemakeNotifyZone.DrawerInline && !drawerOpen)
                    return RemakeNotifyZone.GrandTop;
                return requested;
            }

            if (drawerOpen)
            {
                if (!isError && (category == RemakeNotifyCategory.Breakthrough ||
                                 category == RemakeNotifyCategory.Tribulation ||
                                 category == RemakeNotifyCategory.KyNgo))
                {
                    return RemakeNotifyZone.GrandTop;
                }
                if (category == RemakeNotifyCategory.LootDrop ||
                    category == RemakeNotifyCategory.CombatWave ||
                    category == RemakeNotifyCategory.BossCombat)
                {
                    return RemakeNotifyZone.CombatTicker;
                }
                return RemakeNotifyZone.DrawerInline;
            }

            RemakeNotifyMeta meta = ResolveMeta(category, isError);
            if (meta.DefaultZone == RemakeNotifyZone.DrawerInline)
                return RemakeNotifyZone.GrandTop;
            return meta.DefaultZone;
        }

        public static bool TryCoalesceMessage(
            string prevBase,
            float prevAgeSec,
            int prevCount,
            string incoming,
            out int newCount,
            out string formatted)
        {
            if (!string.IsNullOrEmpty(prevBase) &&
                string.Equals(prevBase, incoming, StringComparison.Ordinal) &&
                prevAgeSec >= 0f && prevAgeSec <= 3.0f)
            {
                newCount = Mathf.Max(2, prevCount + 1);
                formatted = $"{incoming} ×{newCount}";
                return true;
            }
            newCount = 1;
            formatted = incoming;
            return false;
        }

        public void ShowNotification(
            RemakeNotifyCategory category,
            string message,
            RemakeNotifyZone zone = RemakeNotifyZone.Auto,
            bool isError = false)
        {
            if (string.IsNullOrEmpty(message)) return;

            RemakeNotifyZone effectiveZone = ResolveEffectiveZone(category, zone, _suppressAmbientToasts, isError);

            // Mini-Mode: only major breakthrough/tribulation/respawn alerts show
            if (_isMiniModeActive)
            {
                bool allowInMini = category == RemakeNotifyCategory.Breakthrough ||
                                   category == RemakeNotifyCategory.Tribulation ||
                                   category == RemakeNotifyCategory.Error ||
                                   category == RemakeNotifyCategory.Warning;
                if (!allowInMini) return;
                effectiveZone = RemakeNotifyZone.GrandTop;
            }

            // Suppress Zone 2 ambient combat/loot ticker whenever any Drawer is open
            if (effectiveZone == RemakeNotifyZone.CombatTicker && _suppressAmbientToasts)
                return;

            RemakeNotifyMeta meta = ResolveMeta(category, isError);

            if (effectiveZone == RemakeNotifyZone.DrawerInline)
            {
                if (OnDrawerInlineRequested != null)
                {
                    OnDrawerInlineRequested.Invoke(category, meta.CssClass, meta.BadgeLabel, message, meta.CodexIconKey, meta.ReusedIconKey);
                    return;
                }
                effectiveZone = RemakeNotifyZone.GrandTop;
            }

            if (effectiveZone == RemakeNotifyZone.CombatTicker)
            {
                ShowInCombatTicker(meta, message);
                return;
            }

            ShowInGrandTop(meta, message, category, isError);
        }

        public void ShowToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            RemakeNotifyCategory cat = InferLegacyCategory(message, out bool isError);
            ShowNotification(cat, message, RemakeNotifyZone.Auto, isError);
        }

        public void ShowFeedback(string message, RemakeFeedbackKind kind, string badgeLabel = null)
        {
            if (string.IsNullOrEmpty(message)) return;
            string css = kind == RemakeFeedbackKind.GoldSuccess ? "toast-gold"
                : kind == RemakeFeedbackKind.CrimsonAlert ? "toast-danger" : "toast-jade";
            string badge = !string.IsNullOrEmpty(badgeLabel) ? badgeLabel
                : kind == RemakeFeedbackKind.GoldSuccess ? "THÀNH CÔNG"
                : kind == RemakeFeedbackKind.CrimsonAlert ? "CHÚ Ý" : "NHẬN";
            bool useCalli = badge == "ĐỘT PHÁ" || badge == "THIÊN KIẾP" || badge == "BẾ QUAN" || badge == "KỲ NGỘ";
            var meta = new RemakeNotifyMeta(css, badge, "", "", useCalli, RemakeNotifyZone.GrandTop);
            ShowInGrandTop(meta, message, RemakeNotifyCategory.QuestDiary, kind == RemakeFeedbackKind.CrimsonAlert);
        }

        private void ShowInGrandTop(RemakeNotifyMeta meta, string message, RemakeNotifyCategory category, bool isError)
        {
            if (_toastStack == null || _bannerSlots.Count == 0) return;

            BannerSlot target = null;
            float lowestRem = float.MaxValue;
            for (int i = 0; i < _bannerSlots.Count; i++)
            {
                if (_bannerSlots[i].remaining <= 0f) { target = _bannerSlots[i]; break; }
                if (_bannerSlots[i].remaining < lowestRem) { lowestRem = _bannerSlots[i].remaining; target = _bannerSlots[i]; }
            }
            if (target == null) return;

            ApplySlotStyle(target, meta, message, meta.UseCalligraphy);
            bool isGrandMajor = !isError && (category == RemakeNotifyCategory.Breakthrough ||
                                             category == RemakeNotifyCategory.Tribulation ||
                                             category == RemakeNotifyCategory.KyNgo ||
                                             category == RemakeNotifyCategory.BossCombat);
            target.duration = isGrandMajor ? 3.4f : 2.7f;
            target.remaining = target.duration;
            target.baseMessage = message;
            target.coalesceCount = 1;
            target.lastTouchTime = Time.unscaledTime;
            target.root.style.display = DisplayStyle.Flex;
            target.root.style.opacity = 1f;
            target.root.style.scale = new Scale(new Vector3(1.06f, 1.06f, 1f));
            target.root.BringToFront();
        }

        private void ShowInCombatTicker(RemakeNotifyMeta meta, string message)
        {
            if (_combatTickerStack == null || _tickerSlots.Count == 0) return;
            float now = Time.unscaledTime;

            // Coalesce identical combat/loot lines within 3.0 seconds (e.g., "Nhặt Phôi Kiếm Cấp 1! ×3")
            for (int i = 0; i < _tickerSlots.Count; i++)
            {
                var existing = _tickerSlots[i];
                if (existing.remaining <= 0f) continue;
                if (TryCoalesceMessage(existing.baseMessage, now - existing.lastTouchTime, existing.coalesceCount, message, out int nextCount, out string mergedText))
                {
                    existing.coalesceCount = nextCount;
                    existing.lastTouchTime = now;
                    existing.duration = 2.2f;
                    existing.remaining = 2.2f;
                    existing.msg.text = mergedText;
                    existing.root.style.opacity = 1f;
                    existing.root.style.scale = new Scale(new Vector3(1.05f, 1.05f, 1f));
                    existing.root.BringToFront();
                    return;
                }
            }

            BannerSlot target = null;
            float lowestRem = float.MaxValue;
            for (int i = 0; i < _tickerSlots.Count; i++)
            {
                if (_tickerSlots[i].remaining <= 0f) { target = _tickerSlots[i]; break; }
                if (_tickerSlots[i].remaining < lowestRem) { lowestRem = _tickerSlots[i].remaining; target = _tickerSlots[i]; }
            }
            if (target == null) return;

            ApplySlotStyle(target, meta, message, false);
            target.duration = 2.2f;
            target.remaining = 2.2f;
            target.baseMessage = message;
            target.coalesceCount = 1;
            target.lastTouchTime = now;
            target.root.style.display = DisplayStyle.Flex;
            target.root.style.opacity = 1f;
            target.root.style.scale = new Scale(new Vector3(1.04f, 1.04f, 1f));
            target.root.BringToFront();
        }

        private static void ApplySlotStyle(BannerSlot slot, RemakeNotifyMeta meta, string message, bool useCalligraphy)
        {
            slot.root.RemoveFromClassList("toast-gold");
            slot.root.RemoveFromClassList("toast-jade");
            slot.root.RemoveFromClassList("toast-azure");
            slot.root.RemoveFromClassList("toast-violet");
            slot.root.RemoveFromClassList("toast-danger");
            slot.root.RemoveFromClassList("toast-calligraphy");

            slot.root.AddToClassList(meta.CssClass);
            if (useCalligraphy)
                slot.root.AddToClassList("toast-calligraphy");

            ApplyToastIcon(slot.icon, meta.CodexIconKey, meta.ReusedIconKey);
            slot.tag.text = meta.BadgeLabel;
            slot.msg.text = message;
        }

        public static void ApplyToastIcon(VisualElement iconEl, string codexKey, string reusedKey)
        {
            if (iconEl == null) return;
            Texture2D tex = !string.IsNullOrEmpty(codexKey) ? CodexArtLoader.LoadCodexTexture(codexKey) : null;
            if (tex != null)
            {
                iconEl.style.backgroundImage = new StyleBackground(tex);
                iconEl.style.display = DisplayStyle.Flex;
                return;
            }
            Sprite sp = !string.IsNullOrEmpty(reusedKey) ? CodexArtLoader.LoadReusedSprite(reusedKey) : null;
            if (sp != null)
            {
                iconEl.style.backgroundImage = new StyleBackground(sp);
                iconEl.style.display = DisplayStyle.Flex;
                return;
            }
            iconEl.style.backgroundImage = StyleKeyword.None;
            iconEl.style.display = DisplayStyle.None;
        }

        public static RemakeNotifyCategory InferLegacyCategory(string message, out bool isError)
        {
            string lo = (message ?? "").ToLowerInvariant();
            isError = lo.Contains("thất bại") || lo.Contains("nổ lò") || lo.Contains("thiếu ") ||
                      lo.Contains("không đủ") || lo.Contains("không thể") || lo.Contains("chưa kết nối") ||
                      lo.Contains("không tìm thấy") || lo.Contains("đã đầy");

            if (lo.Contains("độ kiếp") || lo.Contains("đột phá") || lo.Contains("trấn tâm") || lo.Contains("tĩnh tâm"))
                return RemakeNotifyCategory.Breakthrough;
            if (lo.Contains("thiên kiếp") || lo.Contains("bế quan"))
                return RemakeNotifyCategory.Tribulation;
            if (lo.Contains("kỳ ngộ"))
                return RemakeNotifyCategory.KyNgo;
            if (lo.Contains("thủ lĩnh") || lo.Contains("hộ thể") || lo.Contains("khiên"))
                return RemakeNotifyCategory.BossCombat;
            if (lo.Contains("nhặt "))
                return RemakeNotifyCategory.LootDrop;
            if (lo.Contains("vượt ải") || lo.Contains("hồi sinh") || lo.Contains("trọng thương"))
            {
                isError = lo.Contains("trọng thương") || lo.Contains("thất thủ");
                return RemakeNotifyCategory.CombatWave;
            }
            if (lo.Contains("kiếm") || lo.Contains("phôi") || lo.Contains("rèn"))
                return RemakeNotifyCategory.SwordPath;
            if (lo.Contains("đan") || lo.Contains("lò") || lo.Contains("linh hỏa") || lo.Contains("tế "))
                return RemakeNotifyCategory.Alchemy;
            if (lo.Contains("đạo vận"))
                return RemakeNotifyCategory.DaoVan;
            if (lo.Contains("trận"))
                return RemakeNotifyCategory.Formation;
            if (lo.Contains("trang bị") || lo.Contains("cường hóa") || lo.Contains("tẩy luyện") || lo.Contains("phân rã"))
                return RemakeNotifyCategory.Equipment;
            if (lo.Contains("kỹ năng"))
                return RemakeNotifyCategory.Skill;
            if (lo.Contains("đồ giám"))
                return RemakeNotifyCategory.Codex;
            if (lo.Contains("linh thú"))
                return RemakeNotifyCategory.Companion;
            if (lo.Contains("tháp") || lo.Contains("bí cảnh") || lo.Contains("luận kiếm"))
                return RemakeNotifyCategory.ImmortalGate;
            if (lo.Contains("đạo hữu") || lo.Contains("kết bạn") || lo.Contains("truyền công") || lo.Contains("hộ pháp"))
                return RemakeNotifyCategory.Social;
            if (isError)
                return RemakeNotifyCategory.Warning;
            return RemakeNotifyCategory.QuestDiary;
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
            _combatTickerStack = root.Q("combat-ticker-stack");

            root.RegisterCallback<GeometryChangedEvent>(_ => UpdateOverlayBreakpoint());
            UpdateOverlayBreakpoint();

            if (_toastStack != null && _bannerSlots.Count == 0)
            {
                _toastStack.Clear();
                for (int i = 0; i < 2; i++)
                    _bannerSlots.Add(CreateSlotElement(_toastStack, "toast-banner"));
            }

            if (_combatTickerStack != null && _tickerSlots.Count == 0)
            {
                _combatTickerStack.Clear();
                for (int i = 0; i < 2; i++)
                    _tickerSlots.Add(CreateSlotElement(_combatTickerStack, "combat-ticker-pill"));
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

        private static BannerSlot CreateSlotElement(VisualElement host, string rootClass)
        {
            var bRoot = new VisualElement { pickingMode = PickingMode.Ignore };
            bRoot.AddToClassList(rootClass);
            bRoot.AddToClassList("toast-jade");
            bRoot.style.display = DisplayStyle.None;

            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("toast-icon");
            icon.AddToClassList("art-slot");
            icon.style.display = DisplayStyle.None;

            var tag = new Label("NHẬN") { pickingMode = PickingMode.Ignore };
            tag.AddToClassList("toast-tag");

            var msg = new Label("") { pickingMode = PickingMode.Ignore };
            msg.AddToClassList("toast-msg");

            bRoot.Add(icon);
            bRoot.Add(tag);
            bRoot.Add(msg);
            host.Add(bRoot);

            return new BannerSlot { root = bRoot, icon = icon, tag = tag, msg = msg };
        }

        private void UpdateOverlayBreakpoint()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            int sw = Mathf.Max(1, Screen.width);
            int sh = Mathf.Max(1, Screen.height);
            bool isDesktop = (sw > sh * 1.15f) && sw >= 700 && sh >= 420;
            uiDocument.rootVisualElement.EnableInClassList("desktop-wide", isDesktop);
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

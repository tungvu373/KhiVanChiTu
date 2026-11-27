using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public sealed class InGameHudController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private enum DrawerKind
        {
            None, SwordPath, DaoVan, Alchemy, Equipment, Formation, Skills, Stages, Breakthrough, Settings, Diary, Leaderboard, Friends, WorldChat, ImmortalGate, KyNgo, PvpPreview, Codex, Companions, OfflineSummary, CharacterDetail
        }

        private VisualElement _safeRoot;
        private VisualElement _clusterCultivationDrawer;
        private VisualElement _clusterUtilityDrawer;
        private bool _cultivationExpanded;
        private bool _utilityExpanded;
        private DrawerKind _activeDrawer = DrawerKind.None;

        // Top-Left & Radial TuVi
        private RadialRingElement _tuViRing;
        private VisualElement _avatarPortrait;
        private VisualElement _avatarFrameOverlay;
        private int _lastDrawnAvatarKey = -1;
        private Label _hudDaoName;
        private Label _hudRealmLabel;
        private VisualElement _hpFill;
        private VisualElement _mpFill;
        private VisualElement _wrapBreakthrough;
        private Label _badgeBreakthroughBang;

        // Left Collapsible Utility Stack & 15s Tribulation Banner
        private Button _btnToggleLeftHud;
        private VisualElement _leftHudCollapsible;
        private VisualElement _badgeLeftHudCollapsed;
        private bool _leftHudExpanded = true;
        private VisualElement _tribulationPanel;
        private Label _tribulationTimerLabel;
        private Label _tribulationTargetLabel;
        private VisualElement _tribulationProgressFill;
        private Label _tribulationStrikeLabel;
        private float _tribulationPulseTimer;

        // Player Respawn Countdown Banner
        private VisualElement _respawnPanel;
        private Label _respawnTimerLabel;
        private Label _respawnReasonLabel;
        private VisualElement _respawnProgressFill;

        // Mini-Mode
        private Label _miniRealmStage;
        private VisualElement _miniTuViFill;
        private Label _miniFpsLabel;
        private Button _btnMiniBreakthrough;
        private Button _btnMiniCameraView;

        // Stage & Boss
        private Label _hudStageLabel;
        private Button _btnChallengeBoss;
        private VisualElement _challengeBanner;
        private Label _challengeBannerLabel;
        private VisualElement _bossPanel;
        private Label _bossNameLabel;
        private Label _bossTimerLabel;
        private VisualElement _bossHpFill;
        private Label _bossHpText;
        private VisualElement _bossShieldRow;
        private VisualElement _bossShieldIcon;
        private Label _bossShieldLabel;
        private VisualElement _bossShieldFill;
        private bool _isMiniMode;
        private VisualElement _wrapKyNgo;
        private Label _kyNgoTimerLabel;
        private VisualElement _buffDoubleTuViPill;
        private Label _buffDoubleTuViLabel;

        // Top-Right
        private Label _hudLinhThachLabel;
        private VisualElement _fpsPill;
        private Label _hudFpsLabel;
        private Button _btnAfkDim;
        private Button _btnCameraView;
        private VisualElement _cameraDragSurface;
        private bool _isDraggingCamera;
        private int _cameraDragPointerId = -1;
        private Vector2 _lastCameraDragPos;

        // Quest & 4 Combat Skills
        private Label _questTitleLabel;
        private Button _btnQuestTracker;
        private readonly VisualElement[] _skillWraps = new VisualElement[4];
        private readonly RadialRingElement[] _skillCdRings = new RadialRingElement[4];
        private readonly Button[] _skillAutoBtns = new Button[4];
        private VisualElement _bossPortraitSlot;
        private int _lastBossActDrawn = -1;
        private Label _chatTickerLabel;

        // Cluster Lock / Badges
        private Button _btnNavDaoVan;
        private Button _btnNavAlchemy;
        private Button _btnNavEquipment;
        private Button _btnNavFormation;
        private Button _btnNavSkills;
        private VisualElement _badgeClusterBagua;
        private VisualElement _badgeSwordPath;
        private VisualElement _badgeDaoVan;
        private VisualElement _badgeMainQuest;
        private VisualElement _badgePioneerDiary;
        private VisualElement _badgeUtilityCluster;
        private bool _mainQuestRewardReady;
        private bool _pioneerRewardReady;

        // Lazy Modal
        private VisualElement _modalBackdrop;
        private VisualElement _modalSheet;
        private Label _modalTitle;
        private VisualElement _modalBody;

        // Tutorial Pointer Overlay
        private VisualElement _tutorialOverlay;
        private VisualElement _tutorialRing;
        private Label _tutorialTip;

        // Pooled Damage Popups (UI Toolkit zero runtime alloc after startup)
        private sealed class PopupSlot
        {
            public Label label;
            public Vector3 worldPos;
            public float timer;
        }
        private readonly List<PopupSlot> _popupPool = new List<PopupSlot>();
        private VisualElement _popupsLayer;

        // Dao Van Interactive State
        private Vector2 _daoVanPan = Vector2.zero;
        private float _daoVanZoom = 0.72f;

        // Coalesced HUD dirty flags (max 1 HUD update per frame -> zero AoE event storms!)
        private bool _hudDirty = true;
        private bool _drawerDirty;
        private float _badgeCheckCooldown;
        private bool _cachedHasDaoReady;
        private bool _desktopDockInitialized;

        private void OnEnable()
        {
            BindHudVisuals();
            SubscribeEvents();
            RefreshAllHudImmediate();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Update()
        {
            HandlePcKeyboardHotkeys();
            UpdateRealtimeBars();
            UpdateDamagePopups();
            UpdateTutorialPointerPosition();
        }

        private void HandlePcKeyboardHotkeys()
        {
            var kb = Keyboard.current;
            if (kb == null || PlatformWebBridge.IsTouchPlatform()) return;
            var engine = GameplayEngine.Instance;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (_activeDrawer != DrawerKind.None) CloseDrawer();
                else OpenDrawer(DrawerKind.Settings);
                return;
            }
            if (_activeDrawer != DrawerKind.None) return;

            if (kb.tabKey.wasPressedThisFrame) ToggleLeftHud();
            if (engine == null) return;
            if (kb.digit1Key.wasPressedThisFrame) engine.TryCastSkill(0);
            else if (kb.digit2Key.wasPressedThisFrame) engine.TryCastSkill(1);
            else if (kb.digit3Key.wasPressedThisFrame) engine.TryCastSkill(2);
            else if (kb.digit4Key.wasPressedThisFrame) engine.TryCastSkill(3);
        }

        private void LateUpdate()
        {
            if (_hudDirty)
            {
                _hudDirty = false;
                RefreshAllHudImmediate();
            }
            if (_drawerDirty)
            {
                _drawerDirty = false;
                RefreshActiveDrawerIfOpen();
            }
        }

        private void BindHudVisuals()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null && uiDocument.panelSettings == null)
                uiDocument.panelSettings = Resources.Load<PanelSettings>("RemakePanelSettings");
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            if (uiDocument.rootVisualElement.childCount == 0 && uiDocument.visualTreeAsset != null)
                uiDocument.visualTreeAsset.CloneTree(uiDocument.rootVisualElement);
            var root = uiDocument.rootVisualElement;

            _safeRoot = root.Q("hud-safe-root");
            _safeRoot?.RegisterCallback<GeometryChangedEvent>(_ => ApplySafeAreaAndBreakpoints());

            // Inject Painter2D Radial Rings
            var avatarHost = root.Q("avatar-ring-host");
            if (avatarHost != null && _tuViRing == null)
            {
                _tuViRing = new RadialRingElement();
                _tuViRing.AddToClassList("full-screen");
                avatarHost.Add(_tuViRing);
            }
            for (int s = 0; s < 4; s++)
            {
                var skillHost = root.Q($"cd-host-{s}");
                if (skillHost != null && _skillCdRings[s] == null)
                {
                    _skillCdRings[s] = new RadialRingElement();
                    _skillCdRings[s].AddToClassList("full-screen");
                    skillHost.Add(_skillCdRings[s]);
                }
            }

            // Cache Labels/Bars
            _hudDaoName = root.Q<Label>("hud-dao-name");
            _hudRealmLabel = root.Q<Label>("hud-realm-label");
            _hpFill = root.Q("hud-hp-fill");
            _mpFill = root.Q("hud-mp-fill");
            _wrapBreakthrough = root.Q("wrap-btn-breakthrough");
            _badgeBreakthroughBang = root.Q<Label>("badge-breakthrough-bang");

            _btnToggleLeftHud = root.Q<Button>("btn-toggle-left-hud");
            _leftHudCollapsible = root.Q("left-hud-collapsible");
            _badgeLeftHudCollapsed = root.Q("badge-left-hud-collapsed");
            _tribulationPanel = root.Q("tribulation-hud-panel");
            _tribulationTimerLabel = root.Q<Label>("tribulation-timer-label");
            _tribulationTargetLabel = root.Q<Label>("tribulation-target-label");
            _tribulationProgressFill = root.Q("tribulation-progress-fill");
            _tribulationStrikeLabel = root.Q<Label>("tribulation-strike-label");

            _respawnPanel = root.Q("respawn-hud-panel");
            _respawnTimerLabel = root.Q<Label>("respawn-timer-label");
            _respawnReasonLabel = root.Q<Label>("respawn-reason-label");
            _respawnProgressFill = root.Q("respawn-progress-fill");

            _miniRealmStage = root.Q<Label>("mini-realm-stage");
            _miniTuViFill = root.Q("mini-tuvi-fill");
            _miniFpsLabel = root.Q<Label>("mini-fps-label");
            _btnMiniBreakthrough = root.Q<Button>("btn-mini-breakthrough");
            _btnMiniCameraView = root.Q<Button>("btn-mini-camera-view");

            _hudStageLabel = root.Q<Label>("hud-stage-label");
            _btnChallengeBoss = root.Q<Button>("btn-challenge-boss");
            _challengeBanner = root.Q("challenge-banner");
            _challengeBannerLabel = root.Q<Label>("challenge-banner-label");
            _bossPanel = root.Q("boss-hp-panel");
            _bossNameLabel = root.Q<Label>("boss-name-label");
            _bossTimerLabel = root.Q<Label>("boss-timer-label");
            _bossHpFill = root.Q("boss-hp-fill");
            _bossHpText = root.Q<Label>("boss-hp-text");
            _bossShieldRow = root.Q("boss-shield-row");
            _bossShieldIcon = root.Q("icon-boss-shield");
            _bossShieldLabel = root.Q<Label>("boss-shield-label");
            _bossShieldFill = root.Q("boss-shield-fill");
            _wrapKyNgo = root.Q("wrap-ky-ngo");
            _kyNgoTimerLabel = root.Q<Label>("ky-ngo-timer-label");
            _buffDoubleTuViPill = root.Q("buff-double-tuvi-pill");
            _buffDoubleTuViLabel = root.Q<Label>("buff-double-tuvi-label");

            _hudLinhThachLabel = root.Q<Label>("hud-linhthach-label");
            _fpsPill = root.Q("pill-fps");
            _hudFpsLabel = root.Q<Label>("hud-fps-label");
            _btnAfkDim = root.Q<Button>("btn-afk-dim");
            _btnCameraView = root.Q<Button>("btn-camera-view");

            _questTitleLabel = root.Q<Label>("quest-title-label");
            _btnQuestTracker = root.Q<Button>("btn-quest-tracker");
            _bossPortraitSlot = root.Q("boss-portrait-slot");
            _chatTickerLabel = root.Q<Label>("chat-ticker-label");
            for (int s = 0; s < 4; s++)
            {
                _skillWraps[s] = root.Q($"wrap-skill-{s}");
                _skillAutoBtns[s] = root.Q<Button>($"btn-auto-skill-{s}");
            }

            _clusterCultivationDrawer = root.Q("cluster-cultivation-drawer");
            _clusterUtilityDrawer = root.Q("cluster-utility-drawer");
            _btnNavDaoVan = root.Q<Button>("btn-nav-daovan");
            _btnNavAlchemy = root.Q<Button>("btn-nav-alchemy");
            _btnNavEquipment = root.Q<Button>("btn-nav-equipment");
            _btnNavFormation = root.Q<Button>("btn-nav-formation");
            _btnNavSkills = root.Q<Button>("btn-nav-skills");
            _badgeClusterBagua = root.Q("badge-cluster-bagua");
            _badgeSwordPath = root.Q("badge-swordpath");
            _badgeDaoVan = root.Q("badge-daovan");
            _badgeMainQuest = root.Q("badge-main-quest");
            _badgePioneerDiary = root.Q("badge-pioneer-diary");
            _badgeUtilityCluster = root.Q("badge-cluster-utility");

            _modalBackdrop = root.Q("modal-backdrop");
            _modalSheet = root.Q("modal-sheet");
            _modalTitle = root.Q<Label>("modal-title");
            _modalBody = root.Q("modal-body");

            _tutorialOverlay = root.Q("tutorial-overlay");
            _tutorialRing = root.Q("tutorial-highlight-ring");
            _tutorialTip = root.Q<Label>("tutorial-tip-label");

            _popupsLayer = root.Q("damage-popups-layer");
            PrewarmPopupPool();

            // Bind REAL Images / Codex Art Slots (zero emoji!)
            _avatarPortrait = root.Q("avatar-portrait");
            _avatarFrameOverlay = root.Q("avatar-frame-overlay");
            CodexArtLoader.ApplyIconOrPlaceholder(_avatarPortrait, CultivationTable.GetPortraitCodexKey(SaveService.Current.gender), "[Avatar]", "CharacterPortrait");
            CodexArtLoader.ApplyIconOrPlaceholder(_avatarFrameOverlay, CultivationTable.GetAvatarFrameCodexKey(SaveService.Current.equippedAvatarFrame), "");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-breakthrough"), "HUD/nav_breakthrough", "[G6: Đột Phá]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-stage-pill"), "HUD/nav_map_stage", "[G5: Ải]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("boss-portrait-slot"), "Monsters/act1_boss_guardian", "[F11: Boss]");
            CodexArtLoader.ApplyIconOrPlaceholder(_bossShieldIcon, "HUD/icon_boss_shield", "", "GoldenBell");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-linhthach"), "", "[LT]", "SpiritStone");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-cluster-utility"), "", "[Tiện Ích]", "RouteSettings");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-codex"), "HUD/nav_codex", "", "RouteAchievements");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-companion"), "Companions/pet_azure_crane", "", "SpiritLotus");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-settings"), "", "[Cài Đặt]", "RouteSettings");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-leaderboard"), "HUD/nav_leaderboard", "[G1: BXH]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-friends"), "HUD/nav_friends", "[G3: Đạo Hữu]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-mainmenu"), "", "[Menu]", "CharacterPortrait");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-chat-bubble"), "HUD/nav_world_chat", "G2");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-skill-0"), "", "[Lôi Phạt]", "SkillThunder");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-skill-1"), "", "[Âm Dương]", "SkillYinYang");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-skill-2"), "", "[Vạn Kiếm]", "SkillSword");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-skill-3"), "", "[Ấn Chưởng]", "SkillPalm");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-diary"), "", "[Tiên Lộ]", "RouteMilestones");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-camera-view"), "HUD/nav_combat_camera", "");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-mini-camera-view"), "HUD/nav_combat_camera", "");

            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-cluster-bagua"), "HUD/hud_toggle_bagua", "[A4: Bát Quái]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-swordpath"), "", "[Kiếm Lộ]", "RouteMerge");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-daovan"), "DaoVan/daovan_core_node", "[Đạo Vận]", "RouteTalents");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-alchemy"), "", "[Luyện Đan]", "RouteAlchemy");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-equipment"), "", "[Trang Bị]", "RouteArtifact");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-formation"), "Formations/formation_thunder", "[Trận Pháp]", "RouteFormation");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-nav-skills"), "HUD/nav_skills", "[G4: Kỹ Năng]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-immortal-gate-quick"), "HUD/nav_immortal_gate", "[Tiên Môn]");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("icon-ky-ngo"), "Stages/event_fortuitous_orb", "[Kỳ Ngộ]");

            // Wire Buttons
            root.Q("btn-hud-character-profile")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.CharacterDetail));
            _btnChallengeBoss?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                GameplayEngine.Instance?.TryChallengeCurrentActBoss();
            });
            root.Q<Button>("btn-exit-challenge")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                GameplayEngine.Instance?.ExitChallengeToCampaign();
            });
            root.Q<Button>("btn-immortal-gate-quick")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.ImmortalGate));
            root.Q<Button>("btn-ky-ngo")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.KyNgo));
            _btnToggleLeftHud?.RegisterCallback<ClickEvent>(_ => ToggleLeftHud());
            root.Q<Button>("btn-cluster-cultivation")?.RegisterCallback<ClickEvent>(_ => ToggleCultivationCluster());
            root.Q<Button>("btn-cluster-utility")?.RegisterCallback<ClickEvent>(_ => ToggleUtilityCluster());
            root.Q<Button>("btn-afk-dim")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                GraphicsSettingsService.Instance?.ToggleAfkDimMode();
            });
            root.Q<Button>("btn-stage-pill")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Stages));
            root.Q<Button>("btn-breakthrough")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Breakthrough));
            _btnMiniBreakthrough?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Breakthrough));
            _btnCameraView?.RegisterCallback<ClickEvent>(_ => ToggleCombatCamera());
            _btnMiniCameraView?.RegisterCallback<ClickEvent>(_ => ToggleCombatCamera());
            _cameraDragSurface = root.Q("camera-drag-surface");
            BindCameraDragSurface();
            RefreshCameraButtons();
            root.Q<Button>("btn-chat-bubble")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeOnlineService.Instance?.SyncAllOnlineData();
                OpenDrawer(DrawerKind.WorldChat);
            });

            _btnQuestTracker?.RegisterCallback<ClickEvent>(_ => HandleQuestTrackerClicked());

            for (int s = 0; s < 4; s++)
            {
                int slotIdx = s;
                root.Q<Button>($"btn-skill-{s}")?.RegisterCallback<ClickEvent>(_ => GameplayEngine.Instance?.TryCastSkill(slotIdx));
                _skillAutoBtns[s]?.RegisterCallback<ClickEvent>(_ =>
                {
                    if (GameplayEngine.Instance == null) return;
                    GameplayEngine.Instance.SetSkillAuto(slotIdx, !GameplayEngine.Instance.GetSkillAuto(slotIdx));
                });
            }

            root.Q<Button>("btn-nav-swordpath")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.SwordPath));
            _btnNavDaoVan?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.DaoVan));
            _btnNavAlchemy?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Alchemy));
            _btnNavEquipment?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Equipment));
            _btnNavFormation?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Formation));
            _btnNavSkills?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Skills));

            root.Q<Button>("btn-nav-diary")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Diary));
            root.Q<Button>("btn-nav-codex")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Codex));
            root.Q<Button>("btn-nav-companion")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Companions));
            root.Q<Button>("btn-nav-settings")?.RegisterCallback<ClickEvent>(_ => OpenDrawer(DrawerKind.Settings));
            root.Q<Button>("btn-nav-leaderboard")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeOnlineService.Instance?.SyncAllOnlineData();
                OpenDrawer(DrawerKind.Leaderboard);
            });
            root.Q<Button>("btn-nav-friends")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeOnlineService.Instance?.SyncAllOnlineData();
                OpenDrawer(DrawerKind.Friends);
            });
            root.Q<Button>("btn-nav-mainmenu")?.RegisterCallback<ClickEvent>(_ => SceneFlowController.Instance?.OpenCharacterSelect());

            root.Q<Button>("btn-close-modal")?.RegisterCallback<ClickEvent>(_ => CloseDrawer());
            ApplySafeAreaAndBreakpoints();
        }

        private void SubscribeEvents()
        {
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnStatsChanged += MarkHudDirty;
                GameplayEngine.Instance.OnStageChanged += HandleStageChanged;
                GameplayEngine.Instance.OnQuestOrTutorialChanged += MarkHudDirty;
                GameplayEngine.Instance.OnBossStatusUpdated += HandleBossStatus;
                GameplayEngine.Instance.OnBreakthroughResolved += HandleBreakthroughOrMergeRefresh;
                GameplayEngine.Instance.OnSwordMergeResolved += HandleSwordMergeRefresh;
                GameplayEngine.Instance.OnAlchemyBrewResolved += HandleAlchemyBrewResolved;
                GameplayEngine.Instance.OnTribulationStateChanged += HandleTribulationStateChanged;
                GameplayEngine.Instance.OnTribulationStrike += HandleTribulationStrike;
                GameplayEngine.Instance.OnRespawnStateChanged += HandleRespawnStateChanged;
                GameplayEngine.Instance.OnOfflineCatchupSummary += HandleOfflineCatchupSummary;
                GameplayEngine.Instance.OnBossShieldStateChanged += MarkHudDirty;
            }
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated += RefreshFpsHud;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += HandleGraphicsChanged;
            if (RemakeVfxPool.Instance != null)
                RemakeVfxPool.Instance.OnDamagePopupRequested += SpawnDamagePopup;
            if (RemakeOnlineService.Instance != null)
                RemakeOnlineService.Instance.OnOnlineStateUpdated += HandleOnlineUpdated;
        }

        private void UnsubscribeEvents()
        {
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnStatsChanged -= MarkHudDirty;
                GameplayEngine.Instance.OnStageChanged -= HandleStageChanged;
                GameplayEngine.Instance.OnQuestOrTutorialChanged -= MarkHudDirty;
                GameplayEngine.Instance.OnBossStatusUpdated -= HandleBossStatus;
                GameplayEngine.Instance.OnBreakthroughResolved -= HandleBreakthroughOrMergeRefresh;
                GameplayEngine.Instance.OnSwordMergeResolved -= HandleSwordMergeRefresh;
                GameplayEngine.Instance.OnAlchemyBrewResolved -= HandleAlchemyBrewResolved;
                GameplayEngine.Instance.OnTribulationStateChanged -= HandleTribulationStateChanged;
                GameplayEngine.Instance.OnTribulationStrike -= HandleTribulationStrike;
                GameplayEngine.Instance.OnRespawnStateChanged -= HandleRespawnStateChanged;
                GameplayEngine.Instance.OnOfflineCatchupSummary -= HandleOfflineCatchupSummary;
                GameplayEngine.Instance.OnBossShieldStateChanged -= MarkHudDirty;
            }
            if (BootOverlayController.Instance != null)
            {
                BootOverlayController.Instance.SuppressAmbientToasts = false;
                BootOverlayController.Instance.IsMiniModeActive = false;
            }
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated -= RefreshFpsHud;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= HandleGraphicsChanged;
            if (RemakeVfxPool.Instance != null)
                RemakeVfxPool.Instance.OnDamagePopupRequested -= SpawnDamagePopup;
            if (RemakeOnlineService.Instance != null)
                RemakeOnlineService.Instance.OnOnlineStateUpdated -= HandleOnlineUpdated;
        }

        private void MarkHudDirty() => _hudDirty = true;

        private void HandleStageChanged()
        {
            _hudDirty = true;
            if (_activeDrawer == DrawerKind.Stages || _activeDrawer == DrawerKind.ImmortalGate)
                _drawerDirty = true;
        }

        private void HandleGraphicsChanged()
        {
            _hudDirty = true;
            if (_activeDrawer == DrawerKind.Settings)
                _drawerDirty = true;
        }

        private void HandleAlchemyBrewResolved()
        {
            _hudDirty = true;
            if (_activeDrawer == DrawerKind.Alchemy)
                _drawerDirty = true;
        }

        private void HandleOnlineUpdated()
        {
            if (_chatTickerLabel != null && RemakeOnlineService.Instance != null)
                _chatTickerLabel.text = RemakeOnlineService.Instance.LatestChatTicker;
            if (_activeDrawer == DrawerKind.Leaderboard || _activeDrawer == DrawerKind.Friends || _activeDrawer == DrawerKind.WorldChat)
                _drawerDirty = true;
        }

        private void HandleBreakthroughOrMergeRefresh(bool _, string __)
        {
            _hudDirty = true;
            if (_activeDrawer == DrawerKind.Breakthrough) _drawerDirty = true;
        }

        private void HandleSwordMergeRefresh(bool _, int __, float ___)
        {
            _hudDirty = true;
            if (_activeDrawer == DrawerKind.SwordPath) _drawerDirty = true;
        }

        private void HandleTribulationStateChanged(bool entering)
        {
            if (entering && _activeDrawer == DrawerKind.Breakthrough)
                CloseDrawer();
            _hudDirty = true;
        }

        private void HandleTribulationStrike(int strikeIndex, int totalStrikes)
        {
            _tribulationPulseTimer = 0.25f;
        }

        private void HandleRespawnStateChanged(bool respawning)
        {
            _hudDirty = true;
        }

        private void HandleOfflineCatchupSummary(GameplayEngine.OfflineCatchupSummary summary)
        {
            if (summary != null && !_isMiniMode)
                OpenDrawer(DrawerKind.OfflineSummary);
        }

        private void ToggleLeftHud()
        {
            RemakeAudioService.Instance?.PlayClick();
            _leftHudExpanded = !_leftHudExpanded;
            ApplyLeftHudVisibility();
        }

        private void ApplyLeftHudVisibility()
        {
            if (_leftHudCollapsible != null)
                _leftHudCollapsible.style.display = _leftHudExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            if (_btnToggleLeftHud != null)
                _btnToggleLeftHud.text = _leftHudExpanded ? "◀ Ẩn Tiện Ích" : "▶ Tiện Ích";
            RefreshLeftHudCollapsedBadge();
        }

        private void RefreshLeftHudCollapsedBadge()
        {
            SetBadgeVisible(_badgeLeftHudCollapsed, !_leftHudExpanded && _mainQuestRewardReady);
        }

        private static void SetBadgeVisible(VisualElement badge, bool visible)
        {
            if (badge != null)
                badge.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RefreshPioneerRewardBadges(GameplayEngine engine)
        {
            _pioneerRewardReady = false;
            foreach (var milestone in RemakeCatalogs.PioneerMilestones)
            {
                if (!engine.IsPioneerMilestoneReady(milestone)) continue;
                _pioneerRewardReady = true;
                break;
            }

            SetBadgeVisible(_badgePioneerDiary, _pioneerRewardReady);
            SetBadgeVisible(_badgeUtilityCluster, _pioneerRewardReady && !_utilityExpanded);
        }

        private void ToggleCombatCamera()
        {
            var environment = MeadowEnvironmentController.Instance;
            if (environment == null || !environment.TryToggleCombatCamera()) return;
            RemakeAudioService.Instance?.PlayClick();
            RefreshCameraButtons();
            if (MeadowEnvironmentController.IsRearCombatView)
                BootOverlayController.Instance?.ShowToast("Góc nhìn sau lưng — Kéo màn hình để xoay nhẹ góc quan sát");
        }

        private void BindCameraDragSurface()
        {
            if (_cameraDragSurface == null) return;

            _cameraDragSurface.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (!MeadowEnvironmentController.IsRearCombatView || _activeDrawer != DrawerKind.None) return;
                if (evt.button != 0 && evt.button != 1 && evt.pointerType != UnityEngine.UIElements.PointerType.touch) return;
                _isDraggingCamera = true;
                _cameraDragPointerId = evt.pointerId;
                _lastCameraDragPos = evt.position;
                _cameraDragSurface.CapturePointer(evt.pointerId);
            });

            _cameraDragSurface.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!_isDraggingCamera || evt.pointerId != _cameraDragPointerId) return;
                if (!MeadowEnvironmentController.IsRearCombatView || _activeDrawer != DrawerKind.None)
                {
                    EndCameraDrag();
                    return;
                }
                Vector2 cur = evt.position;
                Vector2 delta = cur - _lastCameraDragPos;
                _lastCameraDragPos = cur;
                if (delta.sqrMagnitude > 0.001f)
                    MeadowEnvironmentController.Instance?.ApplyRearCameraDrag(delta.x, delta.y);
            });

            _cameraDragSurface.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.pointerId == _cameraDragPointerId)
                    EndCameraDrag();
            });

            _cameraDragSurface.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                _isDraggingCamera = false;
                _cameraDragPointerId = -1;
            });

            _cameraDragSurface.RegisterCallback<WheelEvent>(evt =>
            {
                if (!MeadowEnvironmentController.IsRearCombatView || _activeDrawer != DrawerKind.None) return;
                MeadowEnvironmentController.Instance?.ApplyRearCameraZoom(evt.delta.y);
                evt.StopPropagation();
            });
        }

        private void EndCameraDrag()
        {
            if (_cameraDragSurface != null && _cameraDragPointerId >= 0 && _cameraDragSurface.HasPointerCapture(_cameraDragPointerId))
                _cameraDragSurface.ReleasePointer(_cameraDragPointerId);
            _isDraggingCamera = false;
            _cameraDragPointerId = -1;
        }

        private void RefreshCameraButtons()
        {
            bool rear = MeadowEnvironmentController.IsRearCombatView;
            string tooltip = rear ? "Góc nhìn từ trên cao" : "Góc nhìn sau lưng";
            ApplyCameraButtonState(_btnCameraView, rear, tooltip);
            ApplyCameraButtonState(_btnMiniCameraView, rear, tooltip);

            bool allowDrag = rear && _activeDrawer == DrawerKind.None;
            if (!allowDrag && _isDraggingCamera)
                EndCameraDrag();
            if (_cameraDragSurface != null)
                _cameraDragSurface.pickingMode = allowDrag ? PickingMode.Position : PickingMode.Ignore;
        }

        private static void ApplyCameraButtonState(Button button, bool rear, string tooltip)
        {
            if (button == null) return;
            button.EnableInClassList("active", rear);
            button.tooltip = tooltip;
        }

        private void ToggleCultivationCluster()
        {
            RemakeAudioService.Instance?.PlayClick();
            _cultivationExpanded = !_cultivationExpanded;
            if (_cultivationExpanded) _utilityExpanded = false;
            ApplyClusterVisibility();
        }

        private void ToggleUtilityCluster()
        {
            RemakeAudioService.Instance?.PlayClick();
            _utilityExpanded = !_utilityExpanded;
            if (_utilityExpanded) _cultivationExpanded = false;
            ApplyClusterVisibility();
        }

        private void ApplyClusterVisibility()
        {
            if (_clusterCultivationDrawer != null)
                _clusterCultivationDrawer.style.display = _cultivationExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            if (_clusterUtilityDrawer != null)
                _clusterUtilityDrawer.style.display = _utilityExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            SetBadgeVisible(_badgeUtilityCluster, _pioneerRewardReady && !_utilityExpanded);
        }

        private void OpenDrawer(DrawerKind kind)
        {
            RemakeAudioService.Instance?.PlayClick();
            _activeDrawer = kind;
            // Auto-complete progressive feature unlock tutorial when player visits the newly unlocked feature!
            CompleteFeatureTutorialForDrawer(kind);
            if (_utilityExpanded)
            {
                _utilityExpanded = false;
                ApplyClusterVisibility();
            }
            bool useWide = kind == DrawerKind.Diary || kind == DrawerKind.Stages || kind == DrawerKind.Codex || kind == DrawerKind.CharacterDetail;
            _modalSheet?.EnableInClassList("drawer-sheet-wide", useWide);
            if (_modalBackdrop != null) _modalBackdrop.style.display = DisplayStyle.Flex;
            if (BootOverlayController.Instance != null) BootOverlayController.Instance.SuppressAmbientToasts = true;
            RefreshCameraButtons();
            RefreshActiveDrawerIfOpen();
        }

        private static void CompleteFeatureTutorialForDrawer(DrawerKind kind)
        {
            var eng = GameplayEngine.Instance;
            if (eng == null) return;
            var save = SaveService.Current;
            if (!save.tutorialCompleted) return;

            switch (kind)
            {
                case DrawerKind.DaoVan:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.DaoVanTree);
                    break;
                case DrawerKind.Skills:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillLoiPhat, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.SkillLoiPhat);
                    break;
                case DrawerKind.Alchemy:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.Alchemy);
                    break;
                case DrawerKind.Equipment:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.EquipmentLoot);
                    break;
                case DrawerKind.Formation:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.Formation1);
                    break;
                case DrawerKind.Companions:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Companions, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.Companions);
                    break;
                case DrawerKind.ImmortalGate:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.ImmortalTower);
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.SecretRealm);
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.PvpArena, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.PvpArena);
                    break;
                case DrawerKind.PvpPreview:
                    if (CultivationTable.IsFeatureUnlocked(RemakeFeatureId.PvpArena, save.realmIndex, save.subStage))
                        eng.MarkFeatureTutorialCompleted(RemakeFeatureId.PvpArena);
                    break;
            }
        }

        private void CloseDrawer()
        {
            RemakeAudioService.Instance?.PlayClick();
            _activeDrawer = DrawerKind.None;
            _modalSheet?.EnableInClassList("drawer-sheet-wide", false);
            if (_modalBackdrop != null) _modalBackdrop.style.display = DisplayStyle.None;
            if (BootOverlayController.Instance != null) BootOverlayController.Instance.SuppressAmbientToasts = false;
            RefreshCameraButtons();
        }

        private void HandleQuestTrackerClicked()
        {
            var engine = GameplayEngine.Instance;
            if (engine == null) return;
            var save = SaveService.Current;
            if (save.mainQuestIndex >= RemakeCatalogs.MainQuests.Count) return;
            var q = RemakeCatalogs.MainQuests[save.mainQuestIndex];

            if (save.mainQuestProgress >= q.targetCount)
            {
                engine.TryClaimMainQuest();
                return;
            }

            switch (q.targetRoute)
            {
                case "swordpath": OpenDrawer(DrawerKind.SwordPath); break;
                case "daovan": OpenDrawer(DrawerKind.DaoVan); break;
                case "alchemy": OpenDrawer(DrawerKind.Alchemy); break;
                case "equipment": OpenDrawer(DrawerKind.Equipment); break;
                case "formation": OpenDrawer(DrawerKind.Formation); break;
                case "stage": OpenDrawer(DrawerKind.Stages); break;
                case "breakthrough": OpenDrawer(DrawerKind.Breakthrough); break;
                default:
                    BootOverlayController.Instance?.ShowToast("Phi kiếm đang tự động chiến đấu trên đồng cỏ!");
                    break;
            }
        }

        private void RefreshAllHudImmediate()
        {
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            if (engine == null) return;

            double reqTuVi = engine.GetRequiredTuViForCurrent();
            float tuViRatio = Mathf.Clamp01((float)(save.tuVi / System.Math.Max(1, reqTuVi)));
            int pct = Mathf.RoundToInt(tuViRatio * 100f);

            if (_hudDaoName != null) _hudDaoName.text = string.IsNullOrEmpty(save.daoName) ? "Thanh Vân Tử" : save.daoName;
            if (_hudRealmLabel != null)
                _hudRealmLabel.text = $"{CultivationTable.GetRealmShortName(save.realmIndex, save.subStage)} ({pct}%)";
            _tuViRing?.SetProgress(tuViRatio, new Color(0.86f, 0.62f, 0.14f, 0.96f), new Color(0.75f, 0.85f, 0.80f, 0.55f), 4.5f);

            int avatarKey = (int)save.gender * 10 + save.equippedAvatarFrame;
            if (_lastDrawnAvatarKey != avatarKey)
            {
                _lastDrawnAvatarKey = avatarKey;
                if (_avatarPortrait != null)
                {
                    string pKey = CultivationTable.GetPortraitCodexKey(save.gender);
                    CodexArtLoader.ApplyIconOrPlaceholder(_avatarPortrait, pKey, save.gender == RemakeCharacterGender.Male ? "Nam" : "Nữ");
                }
                if (_avatarFrameOverlay != null)
                {
                    string fKey = CultivationTable.GetAvatarFrameCodexKey(save.equippedAvatarFrame);
                    if (!string.IsNullOrEmpty(fKey))
                    {
                        var fTex = CodexArtLoader.LoadCodexTexture(fKey);
                        if (fTex != null)
                        {
                            _avatarFrameOverlay.style.backgroundImage = new StyleBackground(fTex);
                            _avatarFrameOverlay.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            _avatarFrameOverlay.style.display = DisplayStyle.None;
                        }
                    }
                    else
                    {
                        _avatarFrameOverlay.style.display = DisplayStyle.None;
                    }
                }
            }

            if (_wrapBreakthrough != null)
                _wrapBreakthrough.style.display = engine.IsBreakthroughReady ? DisplayStyle.Flex : DisplayStyle.None;

            if (_miniRealmStage != null)
                _miniRealmStage.text = $"{CultivationTable.GetRealmShortName(save.realmIndex, save.subStage)} · {save.activeAct + 1}-{save.activeStage}";
            if (_miniTuViFill != null) _miniTuViFill.style.width = Length.Percent(pct);
            if (_btnMiniBreakthrough != null)
                _btnMiniBreakthrough.style.display = engine.IsBreakthroughReady ? DisplayStyle.Flex : DisplayStyle.None;

            var stage = engine.GetCurrentStageSpec();
            if (_hudStageLabel != null)
            {
                string waveTxt = $"Đợt {engine.CurrentWaveIndex}/{engine.TotalWavesInStage}";
                if (engine.ActiveArenaMode == RemakeArenaMode.DemonTower)
                    _hudStageLabel.text = $"Vạn Yêu Tháp T{engine.ActiveChallengeFloor} · {waveTxt}";
                else if (engine.ActiveArenaMode == RemakeArenaMode.SecretRealm)
                    _hudStageLabel.text = $"{CultivationTable.GetSecretRealmTierSpec(engine.ActiveSecretRealmKind, engine.ActiveSecretRealmTier).title} · {waveTxt}";
                else
                {
                    string diffName = ActStageDatabaseSO.GetDifficultyLabel(stage.difficulty);
                    _hudStageLabel.text = stage.isBossStage
                        ? $"{diffName} · {stage.stageCode} · {waveTxt} (Boss)"
                        : $"{diffName} · {stage.stageCode} · {waveTxt} ({engine.WaveEnemiesRemaining})";
                }
            }
            if (_btnChallengeBoss != null)
                _btnChallengeBoss.style.display = engine.CanChallengeActBossNow() ? DisplayStyle.Flex : DisplayStyle.None;
            if (_challengeBanner != null)
                _challengeBanner.style.display = engine.ActiveArenaMode != RemakeArenaMode.Campaign ? DisplayStyle.Flex : DisplayStyle.None;
            if (_wrapKyNgo != null)
                _wrapKyNgo.style.display = engine.HasKyNgoAvailable ? DisplayStyle.Flex : DisplayStyle.None;
            if (_buffDoubleTuViPill != null)
                _buffDoubleTuViPill.style.display = engine.IsTuViDoubleBuffActive ? DisplayStyle.Flex : DisplayStyle.None;

            if (_hudLinhThachLabel != null) _hudLinhThachLabel.text = CultivationTable.FormatNumber(save.linhThach);

            if (_btnAfkDim != null)
            {
                bool afk = save.graphics.afkDimMode;
                _btnAfkDim.EnableInClassList("selected", afk);
                _btnAfkDim.text = afk ? "15 FPS" : "Tĩnh";
            }

            // Lock states on Cluster 1 circular buttons
            bool daoVanOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage);
            bool alchemyOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, save.realmIndex, save.subStage);
            bool equipOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, save.realmIndex, save.subStage);
            bool formOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, save.realmIndex, save.subStage);
            bool skillOpen = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillLoiPhat, save.realmIndex, save.subStage);

            _btnNavDaoVan?.EnableInClassList("locked", !daoVanOpen);
            _btnNavAlchemy?.EnableInClassList("locked", !alchemyOpen);
            _btnNavEquipment?.EnableInClassList("locked", !equipOpen);
            _btnNavFormation?.EnableInClassList("locked", !formOpen);
            _btnNavSkills?.EnableInClassList("locked", !skillOpen);

            for (int s = 0; s < 4; s++)
            {
                if (_skillWraps[s] != null)
                    _skillWraps[s].style.display = engine.IsSkillUnlocked(s) ? DisplayStyle.Flex : DisplayStyle.None;
                if (_skillAutoBtns[s] != null)
                    _skillAutoBtns[s].EnableInClassList("selected", engine.GetSkillAuto(s));
            }

            // Notification badges (throttled DaoVan scan so combat kills stay ultra-cheap!)
            bool hasMergeReady = HasMergeableSwordPair();
            if (Time.unscaledTime >= _badgeCheckCooldown)
            {
                _badgeCheckCooldown = Time.unscaledTime + 0.35f;
                _cachedHasDaoReady = daoVanOpen && HasAnyUpgradeableDaoVanNode(engine);
            }
            bool hasDaoReady = daoVanOpen && _cachedHasDaoReady;
            if (_badgeSwordPath != null) _badgeSwordPath.style.display = hasMergeReady ? DisplayStyle.Flex : DisplayStyle.None;
            if (_badgeDaoVan != null) _badgeDaoVan.style.display = hasDaoReady ? DisplayStyle.Flex : DisplayStyle.None;
            if (_badgeClusterBagua != null) _badgeClusterBagua.style.display = (hasMergeReady || hasDaoReady) ? DisplayStyle.Flex : DisplayStyle.None;

            // Main Quest 1-Line Tracker & Collapsible Left HUD (NO modal rebuild on combat kills!)
            RefreshQuestTracker();
            RefreshPioneerRewardBadges(engine);
            ApplyLeftHudVisibility();
            RefreshFpsHud();
            HandleBossStatus(engine.HasActiveBoss ? engine.ActiveBossName : null, engine.ActiveBossHp, engine.ActiveBossMaxHp);
        }

        private void RefreshQuestTracker()
        {
            if (_questTitleLabel == null || _btnQuestTracker == null) return;
            var save = SaveService.Current;
            if (save.mainQuestIndex < 0 || save.mainQuestIndex >= RemakeCatalogs.MainQuests.Count)
            {
                _mainQuestRewardReady = false;
                SetBadgeVisible(_badgeMainQuest, false);
                _questTitleLabel.text = "Hoàn thành Chính Tuyến Mốc 1! Tiếp tục treo máy Vực 1";
                SetTrackerNeutralBorder();
                return;
            }
            var q = RemakeCatalogs.MainQuests[save.mainQuestIndex];
            bool done = save.mainQuestProgress >= q.targetCount;
            _mainQuestRewardReady = done;
            SetBadgeVisible(_badgeMainQuest, done);
            _questTitleLabel.text = done
                ? $"[Nhận thưởng] {q.title}"
                : $"{q.title} ({save.mainQuestProgress}/{q.targetCount})";
            Color borderC = done
                ? new Color(0.90f, 0.65f, 0.12f)
                : new Color(0.78f, 0.61f, 0.24f);
            _btnQuestTracker.style.borderTopColor = borderC;
            _btnQuestTracker.style.borderRightColor = borderC;
            _btnQuestTracker.style.borderBottomColor = borderC;
            _btnQuestTracker.style.borderLeftColor = borderC;
        }

        private void SetTrackerNeutralBorder()
        {
            if (_btnQuestTracker == null) return;
            Color c = new Color(0.16f, 0.56f, 0.44f);
            _btnQuestTracker.style.borderTopColor = c;
            _btnQuestTracker.style.borderRightColor = c;
            _btnQuestTracker.style.borderBottomColor = c;
            _btnQuestTracker.style.borderLeftColor = c;
        }

        private void RefreshFpsHud()
        {
            var fpsSvc = FpsMonitorService.Instance;
            if (fpsSvc == null) return;
            bool visible = SaveService.Current.graphics.showFpsCounter;
            if (_fpsPill != null) _fpsPill.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_hudFpsLabel != null)
            {
                bool isDesktop = uiDocument != null && uiDocument.rootVisualElement != null && uiDocument.rootVisualElement.ClassListContains("desktop-wide");
                _hudFpsLabel.text = isDesktop ? fpsSvc.DisplayText : $"{Mathf.RoundToInt(fpsSvc.CurrentFps)} FPS";
                _hudFpsLabel.EnableInClassList("fps-good", fpsSvc.StatusTier == 0);
                _hudFpsLabel.EnableInClassList("fps-warn", fpsSvc.StatusTier == 1);
                _hudFpsLabel.EnableInClassList("fps-low", fpsSvc.StatusTier == 2);
            }
            if (_miniFpsLabel != null)
                _miniFpsLabel.text = $"{Mathf.RoundToInt(fpsSvc.CurrentFps)} FPS";
        }

        private void HandleBossStatus(string bossName, double hp, double maxHp)
        {
            if (_bossPanel == null) return;
            if (_isMiniMode || string.IsNullOrEmpty(bossName))
            {
                _bossPanel.style.display = DisplayStyle.None;
                return;
            }
            _bossPanel.style.display = DisplayStyle.Flex;
            int actIdx = Mathf.Clamp(SaveService.Current.activeAct, 0, 2);
            if (_bossPortraitSlot != null && _lastBossActDrawn != actIdx)
            {
                _lastBossActDrawn = actIdx;
                string pKey = actIdx == 1 ? "Monsters/act2_boss_overlord" : actIdx == 2 ? "Monsters/act3_boss_archon" : "Monsters/act1_boss_guardian";
                CodexArtLoader.ApplyIconOrPlaceholder(_bossPortraitSlot, pKey, "[Boss]");
            }
            if (_bossNameLabel != null) _bossNameLabel.text = bossName;
            float pct = Mathf.Clamp01((float)(hp / System.Math.Max(1, maxHp))) * 100f;
            if (_bossHpFill != null) _bossHpFill.style.width = Length.Percent(pct);
            if (_bossHpText != null) _bossHpText.text = $"{CultivationTable.FormatNumber(hp)} / {CultivationTable.FormatNumber(maxHp)}";
        }

        private void UpdateRealtimeBars()
        {
            var engine = GameplayEngine.Instance;
            if (engine == null) return;
            if (_hpFill != null)
                _hpFill.style.width = Length.Percent(Mathf.Clamp01((float)(engine.PlayerCurrentHp / System.Math.Max(1, engine.PlayerMaxHp))) * 100f);
            if (_mpFill != null)
                _mpFill.style.width = Length.Percent(Mathf.Clamp01(engine.PlayerCurrentMp / Mathf.Max(1f, engine.PlayerMaxMp)) * 100f);
            Color[] cdColors =
            {
                new Color(0.18f, 0.56f, 0.85f, 0.92f),
                new Color(0.16f, 0.68f, 0.54f, 0.92f),
                new Color(0.86f, 0.62f, 0.14f, 0.92f),
                new Color(0.84f, 0.34f, 0.22f, 0.92f)
            };
            for (int s = 0; s < 4; s++)
            {
                if (_skillCdRings[s] == null || !engine.IsSkillUnlocked(s)) continue;
                float rem = engine.GetSkillCooldownRemaining(s);
                float tot = engine.GetSkillCooldownTotal(s);
                float readyFrac = 1f - Mathf.Clamp01(rem / Mathf.Max(0.1f, tot));
                _skillCdRings[s].SetProgress(readyFrac, cdColors[s], new Color(0.80f, 0.85f, 0.82f, 0.5f), 3.0f);
            }

            if (_bossTimerLabel != null && engine.HasActiveBoss)
                _bossTimerLabel.text = $"{Mathf.Max(0f, engine.ActiveBossTimeRemaining):0.0}s";

            if (_bossShieldRow != null)
            {
                bool showShield = engine.HasActiveBoss && !_isMiniMode && (engine.BossShieldActive || engine.BossBrokenRemaining > 0f);
                _bossShieldRow.style.display = showShield ? DisplayStyle.Flex : DisplayStyle.None;
                if (showShield)
                {
                    if (engine.BossShieldActive)
                    {
                        float shieldPct = Mathf.Clamp01((float)(engine.BossShieldHp / System.Math.Max(1.0, engine.BossShieldMaxHp))) * 100f;
                        if (_bossShieldIcon != null)
                            CodexArtLoader.ApplyIconOrPlaceholder(_bossShieldIcon, "HUD/icon_boss_shield", "Khiên");
                        if (_bossShieldLabel != null)
                            _bossShieldLabel.text = $"Hộ Thể {Mathf.RoundToInt(shieldPct)}% · {engine.BossShieldTimeRemaining:0.0}s (Dùng Chiêu Phá!)";
                        if (_bossShieldFill != null)
                        {
                            _bossShieldFill.style.width = Length.Percent(shieldPct);
                            _bossShieldFill.style.backgroundColor = new Color(0.92f, 0.72f, 0.16f, 0.96f);
                        }
                    }
                    else
                    {
                        float breakPct = Mathf.Clamp01(engine.BossBrokenRemaining / 5f) * 100f;
                        if (_bossShieldIcon != null)
                            CodexArtLoader.ApplyIconOrPlaceholder(_bossShieldIcon, "HUD/icon_boss_break", "Phá");
                        if (_bossShieldLabel != null)
                            _bossShieldLabel.text = $"PHÁ HỘ THỂ! Choáng {engine.BossBrokenRemaining:0.0}s (+35% ST)";
                        if (_bossShieldFill != null)
                        {
                            _bossShieldFill.style.width = Length.Percent(breakPct);
                            _bossShieldFill.style.backgroundColor = new Color(0.20f, 0.78f, 0.56f, 0.96f);
                        }
                    }
                }
            }

            if (_wrapKyNgo != null)
            {
                _wrapKyNgo.style.display = engine.HasKyNgoAvailable ? DisplayStyle.Flex : DisplayStyle.None;
                if (_kyNgoTimerLabel != null && engine.HasKyNgoAvailable)
                    _kyNgoTimerLabel.text = $"Kỳ Ngộ · {Mathf.CeilToInt(engine.KyNgoTimeRemaining)}s";
            }

            if (_buffDoubleTuViPill != null)
            {
                _buffDoubleTuViPill.style.display = engine.IsTuViDoubleBuffActive ? DisplayStyle.Flex : DisplayStyle.None;
                if (_buffDoubleTuViLabel != null && engine.IsTuViDoubleBuffActive)
                    _buffDoubleTuViLabel.text = $"x2 Tu Vi · {Mathf.CeilToInt(engine.TuViDoubleBuffRemainingSec)}s";
            }

            if (_challengeBanner != null && _challengeBannerLabel != null && engine.ActiveArenaMode != RemakeArenaMode.Campaign)
            {
                _challengeBanner.style.display = DisplayStyle.Flex;
                string rem = $"{Mathf.Max(0f, engine.ChallengeModeTimeRemaining):0}s";
                _challengeBannerLabel.text = engine.ActiveArenaMode == RemakeArenaMode.DemonTower
                    ? $"Tháp T{engine.ActiveChallengeFloor} · Đợt {engine.CurrentWaveIndex}/{engine.TotalWavesInStage} · {rem}"
                    : $"Bí Cảnh T{engine.ActiveSecretRealmTier} · Đợt {engine.CurrentWaveIndex}/{engine.TotalWavesInStage} · {rem}";
            }

            // Pulse Breakthrough Red '!' Badge when ready
            if (_badgeBreakthroughBang != null && engine.IsBreakthroughReady)
            {
                float s = 1.0f + 0.14f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 5.2f));
                _badgeBreakthroughBang.style.scale = new Scale(new Vector3(s, s, 1f));
            }

            // 15-Second Breakthrough Seclusion & Tribulation Banner
            if (_tribulationPanel != null)
            {
                bool inTrib = engine.IsInTribulation;
                _tribulationPanel.style.display = inTrib ? DisplayStyle.Flex : DisplayStyle.None;
                if (inTrib)
                {
                    float frac = 1f - Mathf.Clamp01(engine.TribulationTimeRemaining / GameplayEngine.TribulationDuration);
                    if (_tribulationTimerLabel != null)
                        _tribulationTimerLabel.text = $"{engine.TribulationTimeRemaining:0.0}s";
                    if (_tribulationTargetLabel != null)
                        _tribulationTargetLabel.text = $"Hướng tới {engine.TribulationTargetLabel} · Tỉ lệ {Mathf.RoundToInt(engine.TribulationTargetChance * 100)}%";
                    if (_tribulationProgressFill != null)
                        _tribulationProgressFill.style.width = Length.Percent(frac * 100f);
                    if (_tribulationStrikeLabel != null)
                        _tribulationStrikeLabel.text = $"Thiên Lôi giáng xuống: Đạo {engine.TribulationStrikesDone}/{GameplayEngine.TribulationStrikesTotal}";
                    if (_tribulationPulseTimer > 0f)
                    {
                        _tribulationPulseTimer = Mathf.Max(0f, _tribulationPulseTimer - Time.unscaledDeltaTime);
                        float ps = 1f + 0.045f * (_tribulationPulseTimer / 0.25f);
                        _tribulationPanel.style.scale = new Scale(new Vector3(ps, ps, 1f));
                    }
                    else
                    {
                        _tribulationPanel.style.scale = new Scale(Vector3.one);
                    }
                }
            }

            // 5-Second Player Death Respawn Countdown Banner (halts monster spawns & attacks)
            if (_respawnPanel != null)
            {
                bool respawning = engine.IsRespawning;
                _respawnPanel.style.display = respawning ? DisplayStyle.Flex : DisplayStyle.None;
                if (respawning)
                {
                    if (_respawnTimerLabel != null)
                        _respawnTimerLabel.text = $"{engine.RespawnTimeRemaining:0.0}s";
                    if (_respawnReasonLabel != null)
                        _respawnReasonLabel.text = string.IsNullOrEmpty(engine.RespawnReasonText)
                            ? "Yêu thú đã tạm rút · Tái chiến sau khi hồi sinh"
                            : engine.RespawnReasonText;
                    if (_respawnProgressFill != null)
                        _respawnProgressFill.style.width = Length.Percent(engine.RespawnProgress01 * 100f);
                }
            }

            // Live Alchemy 3.5s Countdown Slide Bar (zero rebuild per frame)
            if (_activeDrawer == DrawerKind.Alchemy && _modalBody != null)
            {
                var brewFill = _modalBody.Q("alchemy-brew-fill");
                var brewLbl = _modalBody.Q<Label>("alchemy-brew-label");
                if (engine.IsBrewingPill)
                {
                    float p = engine.BrewProgress01;
                    if (brewFill != null) brewFill.style.width = Length.Percent(p * 100f);
                    if (brewLbl != null)
                    {
                        string pillName = CultivationTable.GetPillNameForRealmBreakthrough(engine.BrewingTargetRealm);
                        brewLbl.text = $"Đang luyện {pillName}... còn {engine.BrewTimeRemaining:0.0}s ({Mathf.RoundToInt(p * 100)}%)";
                    }
                }
            }
        }

        private void RefreshActiveDrawerIfOpen()
        {
            if (_activeDrawer == DrawerKind.None || _modalBody == null || _modalTitle == null) return;
            _modalBody.Clear();

            switch (_activeDrawer)
            {
                case DrawerKind.SwordPath:
                    _modalTitle.text = "Kiếm Lộ — Phi Kiếm & Ghép Có Xác Suất";
                    RemakeDrawerViews.BuildSwordPath(_modalBody);
                    break;
                case DrawerKind.DaoVan:
                    _modalTitle.text = "Cây Kỹ Năng Đạo Vận (Kèm Thể Phách)";
                    RemakeDrawerViews.BuildDaoVanTree(_modalBody, _daoVanPan, _daoVanZoom, (p, z) =>
                    {
                        _daoVanPan = p;
                        _daoVanZoom = z;
                        RefreshActiveDrawerIfOpen();
                    });
                    break;
                case DrawerKind.Alchemy:
                    _modalTitle.text = "Luyện Đan — Duy Nhất Đan Đột Phá";
                    RemakeDrawerViews.BuildAlchemy(_modalBody);
                    break;
                case DrawerKind.Stages:
                    _modalTitle.text = "Bản Đồ Vực–Ải & Sổ Tay Rơi Đồ";
                    RemakeDrawerViews.BuildStages(_modalBody);
                    break;
                case DrawerKind.Breakthrough:
                    _modalTitle.text = "Đột Phá Cảnh Giới (Bế Quan Thiên Kiếp 15s)";
                    RemakeDrawerViews.BuildBreakthrough(_modalBody, CloseDrawer);
                    break;
                case DrawerKind.Settings:
                    _modalTitle.text = "Cài Đặt Đồ Họa 3D, FPS & Tiến Độ Codex";
                    RemakeDrawerViews.BuildSettings(_modalBody);
                    break;
                case DrawerKind.Equipment:
                    _modalTitle.text = "Trang Bị Loot RPG (6 Ô & Bộ 3 Set)";
                    RemakeDrawerViews.BuildEquipment(_modalBody);
                    break;
                case DrawerKind.Formation:
                    _modalTitle.text = "Sáu Đại Trận Pháp & Ô Trận Phụ";
                    RemakeDrawerViews.BuildFormation(_modalBody);
                    break;
                case DrawerKind.Skills:
                    _modalTitle.text = "Bốn Đại Kỹ Năng Chiến Đấu";
                    RemakeDrawerViews.BuildSkills(_modalBody);
                    break;
                case DrawerKind.Diary:
                    _modalTitle.text = "Nhật Ký Tiên Lộ (Mục Tiêu 30 Ngày)";
                    RemakeDrawerViews.BuildPioneerDiary(_modalBody);
                    break;
                case DrawerKind.Leaderboard:
                    _modalTitle.text = "Bảng Xếp Hạng Tiên Lộ (LAN Nakama)";
                    RemakeDrawerViews.BuildLeaderboard(_modalBody);
                    break;
                case DrawerKind.Friends:
                    _modalTitle.text = "Đạo Hữu · Truyền Công & Hộ Pháp";
                    RemakeDrawerViews.BuildFriends(_modalBody);
                    break;
                case DrawerKind.WorldChat:
                    _modalTitle.text = "Chat Thế Giới & Thiên Đạo";
                    RemakeDrawerViews.BuildWorldChat(_modalBody);
                    break;
                case DrawerKind.ImmortalGate:
                    _modalTitle.text = "Tiên Môn — Vạn Yêu Tháp & Bí Cảnh";
                    RemakeDrawerViews.BuildImmortalGate(_modalBody, () => OpenDrawer(DrawerKind.PvpPreview));
                    break;
                case DrawerKind.KyNgo:
                    _modalTitle.text = "Kỳ Ngộ Linh Cơ — Cơ Duyên Thoáng Qua";
                    RemakeDrawerViews.BuildKyNgo(_modalBody, CloseDrawer);
                    break;
                case DrawerKind.PvpPreview:
                    _modalTitle.text = "Luận Kiếm Đài — Liên Đấu Phi Kiếm";
                    RemakeDrawerViews.BuildPvpArena(_modalBody);
                    break;
                case DrawerKind.Codex:
                    _modalTitle.text = "Đồ Giám Vạn Giới — Sưu Tầm & Buff Vĩnh Viễn";
                    RemakeDrawerViews.BuildCodexCollection(_modalBody);
                    break;
                case DrawerKind.Companions:
                    _modalTitle.text = "Linh Thú Hộ Đạo — Đồng Hành Tu Luyện";
                    RemakeDrawerViews.BuildCompanions(_modalBody);
                    break;
                case DrawerKind.OfflineSummary:
                    _modalTitle.text = "Tổng Kết Bế Quan — Thu Hoạch Khi Vắng Mặt";
                    RemakeDrawerViews.BuildOfflineSummary(_modalBody, CloseDrawer);
                    break;
                case DrawerKind.CharacterDetail:
                    _modalTitle.text = "Thông Tin Chi Tiết Nhân Vật";
                    RemakeDrawerViews.BuildCharacterDetail(_modalBody, route =>
                    {
                        switch (route)
                        {
                            case "swordpath": OpenDrawer(DrawerKind.SwordPath); break;
                            case "equipment": OpenDrawer(DrawerKind.Equipment); break;
                            case "daovan": OpenDrawer(DrawerKind.DaoVan); break;
                            case "charselect":
                                CloseDrawer();
                                SceneFlowController.Instance?.OpenCharacterSelect();
                                break;
                        }
                    });
                    break;
            }
        }

        private void ApplySafeAreaAndBreakpoints()
        {
            if (_safeRoot == null || uiDocument == null || uiDocument.rootVisualElement == null) return;
            int swRaw = Mathf.Max(1, Screen.width);
            int shRaw = Mathf.Max(1, Screen.height);
            bool isMiniScreen = (!PlatformWebBridge.IsTouchPlatform() && ((swRaw < 480 && shRaw < 540) || shRaw < 400));
            bool isLandscapeDesktop = (swRaw > shRaw * 1.15f) && swRaw >= 700 && shRaw >= 420 && !isMiniScreen;

            if (uiDocument.panelSettings != null)
            {
                Vector2Int targetRes = isLandscapeDesktop ? new Vector2Int(1280, 720) : new Vector2Int(540, 960);
                float targetMatch = isLandscapeDesktop ? 1f : 0f;
                if (uiDocument.panelSettings.referenceResolution != targetRes || !Mathf.Approximately(uiDocument.panelSettings.match, targetMatch))
                {
                    uiDocument.panelSettings.referenceResolution = targetRes;
                    uiDocument.panelSettings.match = targetMatch;
                }
            }

            float w = uiDocument.rootVisualElement.resolvedStyle.width;
            float h = uiDocument.rootVisualElement.resolvedStyle.height;
            if (float.IsNaN(w) || float.IsNaN(h) || w <= 0 || h <= 0) return;

            bool isMini = isMiniScreen || w < 450f || h < 420f;
            bool isDesktop = (w > h * 1.15f || isLandscapeDesktop) && !isMini;
            bool miniChanged = _isMiniMode != isMini;
            _isMiniMode = isMini;
            if (BootOverlayController.Instance != null)
                BootOverlayController.Instance.IsMiniModeActive = isMini;
            if (miniChanged)
            {
                var eng = GameplayEngine.Instance;
                if (eng != null)
                    HandleBossStatus(eng.HasActiveBoss ? eng.ActiveBossName : null, eng.ActiveBossHp, eng.ActiveBossMaxHp);
            }

            // Apply on rootVisualElement so BOTH hud-safe-root AND modal-backdrop/drawer-sheet receive desktop-wide / mini-mode!
            uiDocument.rootVisualElement.EnableInClassList("mini-mode", isMini);
            uiDocument.rootVisualElement.EnableInClassList("desktop-wide", isDesktop);
            _safeRoot.EnableInClassList("mini-mode", isMini);
            _safeRoot.EnableInClassList("desktop-wide", isDesktop);

            // On PC/Laptop landscape, open the horizontal 6-button Cultivation Dock by default once (user can still collapse via Bagua!)
            if (isDesktop && !_desktopDockInitialized)
            {
                _desktopDockInitialized = true;
                _cultivationExpanded = true;
                ApplyClusterVisibility();
            }

            Rect safe = Screen.safeArea;
            float sw = Mathf.Max(1f, Screen.width);
            float sh = Mathf.Max(1f, Screen.height);
            float leftPad = Mathf.Max(10f, (safe.xMin / sw) * w + 8f);
            float rightPad = Mathf.Max(10f, ((sw - safe.xMax) / sw) * w + 8f);
            float topPad = Mathf.Max(10f, ((sh - safe.yMax) / sh) * h + 8f);
            float botPad = Mathf.Max(10f, (safe.yMin / sh) * h + 8f);

            _safeRoot.style.paddingLeft = leftPad;
            _safeRoot.style.paddingRight = rightPad;
            _safeRoot.style.paddingTop = topPad;
            _safeRoot.style.paddingBottom = botPad;
        }

        private void UpdateTutorialPointerPosition()
        {
            if (_tutorialOverlay == null || uiDocument == null) return;
            var save = SaveService.Current;
            var engine = GameplayEngine.Instance;
            if (_isMiniMode || (engine != null && (engine.IsInTribulation || engine.IsRespawning)))
            {
                _tutorialOverlay.style.display = DisplayStyle.None;
                return;
            }

            VisualElement target = null;
            string tipText = null;
            var root = uiDocument.rootVisualElement;

            if (!save.tutorialCompleted)
            {
                if (save.tutorialStep == 1 || save.tutorialStep == 2)
                {
                    if (_activeDrawer == DrawerKind.SwordPath)
                    {
                        target = root.Q("btn-sword-merge-1");
                        tipText = "Bấm ghép 2 Kiếm Cấp 1 → Cấp 2!";
                    }
                    else if (_activeDrawer == DrawerKind.None && _cultivationExpanded)
                    {
                        target = root.Q("btn-nav-swordpath");
                        tipText = "Mở Kiếm Lộ để ghép kiếm!";
                    }
                    else if (_activeDrawer == DrawerKind.None)
                    {
                        target = root.Q("btn-cluster-cultivation");
                        tipText = "Mở Cụm Tu Luyện";
                    }
                }
            }
            else if (engine != null && _activeDrawer == DrawerKind.None)
            {
                // Progressive Feature Unlock Guided Tutorials (same pulsing ring + tooltip style as Sword Path!)
                ResolveNextFeatureUnlockTutorial(root, save, engine, out target, out tipText);
            }

            if (target == null || target.resolvedStyle.display == DisplayStyle.None || target.worldBound.width <= 1f)
            {
                _tutorialOverlay.style.display = DisplayStyle.None;
                return;
            }

            _tutorialOverlay.style.display = DisplayStyle.Flex;
            Rect b = target.worldBound;
            float pulse = Mathf.Sin(Time.unscaledTime * 6f) * 3f;
            _tutorialRing.style.left = b.xMin - 4f - pulse;
            _tutorialRing.style.top = b.yMin - 4f - pulse;
            _tutorialRing.style.width = b.width + 8f + pulse * 2f;
            _tutorialRing.style.height = b.height + 8f + pulse * 2f;

            _tutorialTip.text = tipText;
            _tutorialTip.style.maxWidth = Mathf.Min(250f, root.resolvedStyle.width - 24f);
            _tutorialTip.style.whiteSpace = WhiteSpace.Normal;
            float tipW = float.IsNaN(_tutorialTip.resolvedStyle.width) || _tutorialTip.resolvedStyle.width < 10f
                ? 250f : _tutorialTip.resolvedStyle.width;
            float tipX = Mathf.Clamp(b.center.x - tipW * 0.5f, 12f, Mathf.Max(12f, root.resolvedStyle.width - tipW - 12f));
            float tipY = b.yMin > 90f ? b.yMin - 42f : b.yMax + 8f;
            _tutorialTip.style.left = tipX;
            _tutorialTip.style.top = tipY;
        }

        private void ResolveNextFeatureUnlockTutorial(
            VisualElement root,
            RemakeSaveData save,
            GameplayEngine engine,
            out VisualElement target,
            out string tipText)
        {
            target = null;
            tipText = null;

            (RemakeFeatureId fid, int hostGroup, string btnName, string clusterTip, string navTip)[] steps =
            {
                (RemakeFeatureId.DaoVanTree,    1, "btn-nav-daovan",          "Đạo Vận đã mở! Mở Cụm Tu Luyện",      "Mở Cây Đạo Vận để tăng chỉ số vĩnh viễn!"),
                (RemakeFeatureId.SkillLoiPhat,  1, "btn-nav-skills",          "Kỹ Năng đã mở! Mở Cụm Tu Luyện",      "Mở Kỹ Năng để nâng cấp & bật Tự Động!"),
                (RemakeFeatureId.Alchemy,       1, "btn-nav-alchemy",         "Luyện Đan đã mở! Mở Cụm Tu Luyện",    "Mở Luyện Đan để luyện Đan Đột Phá!"),
                (RemakeFeatureId.EquipmentLoot, 1, "btn-nav-equipment",       "Trang Bị đã mở! Mở Cụm Tu Luyện",     "Mở Trang Bị để mặc & cường hóa bảo giáp!"),
                (RemakeFeatureId.Formation1,    1, "btn-nav-formation",       "Trận Pháp đã mở! Mở Cụm Tu Luyện",    "Mở Trận Pháp để khai mở Lục Đại Trận!"),
                (RemakeFeatureId.ImmortalTower, 3, "btn-immortal-gate-quick", "Mở Tiện Ích để vào Vạn Yêu Tháp!",    "Vạn Yêu Tháp đã mở! Chạm Tiên Môn để leo tháp!"),
                (RemakeFeatureId.Companions,    2, "btn-nav-companion",       "Linh Thú đã mở! Mở Cụm Tiện Ích",     "Mở Linh Thú để thu phục đồng hành hộ đạo!"),
                (RemakeFeatureId.SecretRealm,   3, "btn-immortal-gate-quick", "Mở Tiện Ích để vào Bí Cảnh!",         "Bí Cảnh đã mở! Vào Tiên Môn càn quét nguyên liệu!"),
                (RemakeFeatureId.PvpArena,      3, "btn-immortal-gate-quick", "Mở Tiện Ích để vào Luận Kiếm Đài!",   "Luận Kiếm Đài đã mở! Vào Tiên Môn tỷ thí!")
            };

            for (int i = 0; i < steps.Length; i++)
            {
                var st = steps[i];
                if (!CultivationTable.IsFeatureUnlocked(st.fid, save.realmIndex, save.subStage)) continue;
                if (engine.IsFeatureTutorialCompleted(st.fid)) continue;

                if (st.hostGroup == 1)
                {
                    if (_cultivationExpanded) { target = root.Q(st.btnName); tipText = st.navTip; }
                    else { target = root.Q("btn-cluster-cultivation"); tipText = st.clusterTip; }
                }
                else if (st.hostGroup == 2)
                {
                    if (_utilityExpanded) { target = root.Q(st.btnName); tipText = st.navTip; }
                    else { target = root.Q("btn-cluster-utility"); tipText = st.clusterTip; }
                }
                else
                {
                    if (_leftHudExpanded) { target = root.Q(st.btnName); tipText = st.navTip; }
                    else { target = root.Q("btn-toggle-left-hud"); tipText = st.clusterTip; }
                }
                return;
            }
        }

        private void PrewarmPopupPool()
        {
            if (_popupsLayer == null || _popupPool.Count > 0) return;
            for (int i = 0; i < 16; i++)
            {
                var lbl = new Label();
                lbl.AddToClassList("damage-popup-label");
                lbl.pickingMode = PickingMode.Ignore;
                lbl.style.display = DisplayStyle.None;
                _popupsLayer.Add(lbl);
                _popupPool.Add(new PopupSlot { label = lbl, timer = 0f });
            }
        }

        private void SpawnDamagePopup(Vector3 worldPos, double amount, bool isCrit)
        {
            if (_popupsLayer == null || _popupPool.Count == 0) return;
            if (SaveService.Current.graphics.vfxDensity == RemakeVfxDensity.Minimal && !isCrit && Random.value < 0.45f)
                return;

            PopupSlot chosen = _popupPool[0];
            for (int i = 0; i < _popupPool.Count; i++)
            {
                if (_popupPool[i].timer <= 0f) { chosen = _popupPool[i]; break; }
            }

            chosen.worldPos = worldPos + new Vector3(Random.Range(-0.35f, 0.35f), 0f, 0f);
            chosen.timer = 0.68f;
            chosen.label.text = isCrit ? $"{CultivationTable.FormatNumber(amount)}!" : CultivationTable.FormatNumber(amount);
            chosen.label.EnableInClassList("damage-popup-crit", isCrit);
            chosen.label.style.display = DisplayStyle.Flex;
        }

        private void UpdateDamagePopups()
        {
            Camera cam = Camera.main;
            if (cam == null || uiDocument == null) return;
            float pw = uiDocument.rootVisualElement.resolvedStyle.width;
            float ph = uiDocument.rootVisualElement.resolvedStyle.height;
            if (pw <= 0 || ph <= 0) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _popupPool.Count; i++)
            {
                var slot = _popupPool[i];
                if (slot.timer <= 0f) continue;
                slot.timer -= dt;
                if (slot.timer <= 0f)
                {
                    slot.label.style.display = DisplayStyle.None;
                    continue;
                }

                float elapsed = 0.68f - slot.timer;
                Vector3 wp = slot.worldPos + Vector3.up * (elapsed * 1.25f);
                Vector3 sp = cam.WorldToScreenPoint(wp);
                if (sp.z <= 0f)
                {
                    slot.label.style.display = DisplayStyle.None;
                    continue;
                }

                float ux = (sp.x / Mathf.Max(1f, Screen.width)) * pw - 16f;
                float uy = ph - (sp.y / Mathf.Max(1f, Screen.height)) * ph - 12f;
                slot.label.style.left = ux;
                slot.label.style.top = uy;
                slot.label.style.opacity = Mathf.Clamp01(slot.timer / 0.25f);
            }
        }

        private static bool HasMergeableSwordPair()
        {
            var save = SaveService.Current;
            return save.swordInventoryLevels.Count > 0 && (save.equippedSwordLevels.Count > 0 || save.swordInventoryLevels.Count >= 2);
        }

        private static bool HasAnyUpgradeableDaoVanNode(GameplayEngine engine)
        {
            if (engine == null) return false;
            var all = RemakeCatalogs.StarterDaoVanNodes;
            for (int i = 0; i < all.Count; i++)
                if (engine.CanUnlockOrUpgradeNode(all[i])) return true;
            return false;
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private Label _fpsLabel;
        private Button _btnPreset;
        private Button _btnGfxLow;
        private Button _btnGfxMed;
        private Button _btnGfxHigh;

        private VisualElement _existingSaveBox;
        private VisualElement _portraitSlot;
        private VisualElement _avatarFrameSlot;
        private VisualElement _devAccountSwitcher;
        private Button _btnAccountMain;
        private Button _btnAccountTest;
        private Label _saveDaoNameLabel;
        private Label _saveRealmLabel;
        private Button _btnOpenCharSelect;
        private Button _btnPlayContinue;
        private Button _btnResetSave;

        private void OnEnable()
        {
            BindUi();
            RefreshUi();
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated += UpdateFpsLabel;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += RefreshGraphicsButtons;
        }

        private void OnDisable()
        {
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated -= UpdateFpsLabel;
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= RefreshGraphicsButtons;
        }

        private void BindUi()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null && uiDocument.panelSettings == null)
                uiDocument.panelSettings = Resources.Load<PanelSettings>("RemakePanelSettings");
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            if (uiDocument.rootVisualElement.childCount == 0 && uiDocument.visualTreeAsset != null)
                uiDocument.visualTreeAsset.CloneTree(uiDocument.rootVisualElement);
            var root = uiDocument.rootVisualElement;
            root.RegisterCallback<GeometryChangedEvent>(_ => ApplyMenuResponsiveLayout());
            ApplyMenuResponsiveLayout();

            var bgSlot = root.Q("menu-bg-slot");
            CodexArtLoader.ApplyIconOrPlaceholder(bgSlot, "Backgrounds/bg_main_menu_daylight", "[A11: Nền Sơn Môn Ban Ngày]");

            _portraitSlot = root.Q("menu-portrait-slot");
            _avatarFrameSlot = root.Q("menu-avatar-frame");
            CodexArtLoader.ApplyIconOrPlaceholder(_portraitSlot, CultivationTable.GetPortraitCodexKey(SaveService.Current.gender), "[Chân Dung]", "CharacterPortrait");
            CodexArtLoader.ApplyIconOrPlaceholder(_avatarFrameSlot, CultivationTable.GetAvatarFrameCodexKey(SaveService.Current.equippedAvatarFrame), "");

            _fpsLabel = root.Q<Label>("menu-fps-label");
            _btnPreset = root.Q<Button>("btn-menu-preset");
            _btnGfxLow = root.Q<Button>("btn-gfx-low");
            _btnGfxMed = root.Q<Button>("btn-gfx-med");
            _btnGfxHigh = root.Q<Button>("btn-gfx-high");

            _devAccountSwitcher = root.Q("dev-account-switcher");
            _existingSaveBox = root.Q("existing-save-box");
            _btnAccountMain = root.Q<Button>("btn-account-main");
            _btnAccountTest = root.Q<Button>("btn-account-test");
            _saveDaoNameLabel = root.Q<Label>("save-dao-name");
            _saveRealmLabel = root.Q<Label>("save-realm-info");
            _btnOpenCharSelect = root.Q<Button>("btn-open-charselect");
            _btnPlayContinue = root.Q<Button>("btn-play-continue");
            _btnResetSave = root.Q<Button>("btn-reset-remake-save");

            if (!Application.isEditor && !Debug.isDebugBuild && _devAccountSwitcher != null)
                _devAccountSwitcher.style.display = DisplayStyle.None;

            _btnGfxLow?.RegisterCallback<ClickEvent>(_ => SelectPreset(RemakeGraphicsPreset.Low));
            _btnGfxMed?.RegisterCallback<ClickEvent>(_ => SelectPreset(RemakeGraphicsPreset.Medium));
            _btnGfxHigh?.RegisterCallback<ClickEvent>(_ => SelectPreset(RemakeGraphicsPreset.High));
            _btnPreset?.RegisterCallback<ClickEvent>(_ =>
            {
                var cur = SaveService.Current.graphics.preset;
                SelectPreset((RemakeGraphicsPreset)(((int)cur + 1) % 3));
            });

            _btnAccountMain?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SaveService.SetProfileOverride(null);
                SaveService.SelectSlot(SaveService.FindFirstOccupiedSlotOrDefault());
                GameplayEngine.Instance?.ReloadFromActiveProfile();
                RemakeOnlineService.Instance?.SwitchAccountProfile();
                RefreshUi();
            });

            _btnAccountTest?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SaveService.SeedOrSelectTestAllAccount(false);
                GameplayEngine.Instance?.ReloadFromActiveProfile();
                RemakeOnlineService.Instance?.SwitchAccountProfile();
                RefreshUi();
            });

            _btnOpenCharSelect?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SceneFlowController.Instance?.OpenCharacterSelect();
            });

            _btnPlayContinue?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SceneFlowController.Instance?.EnterWorldAct1();
            });

            _btnResetSave?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                if (SaveService.IsActiveSaveFromNewerVersion) return;
                if (SaveService.IsTestAllProfileActive)
                {
                    SaveService.SeedOrSelectTestAllAccount(true);
                    BootOverlayController.Instance?.ShowNotification(RemakeNotifyCategory.QuestDiary, "Đã đặt lại Tài Khoản Test Full!", RemakeNotifyZone.GrandTop);
                    GameplayEngine.Instance?.ReloadFromActiveProfile();
                    RemakeOnlineService.Instance?.SwitchAccountProfile();
                    RefreshUi();
                }
            });
        }

        private void SelectPreset(RemakeGraphicsPreset preset)
        {
            RemakeAudioService.Instance?.PlayClick();
            GraphicsSettingsService.Instance?.ApplyPreset(preset);
            RefreshGraphicsButtons();
        }

        private void RefreshUi()
        {
            if (!SaveService.IsTestAllProfileActive)
                SaveService.SelectSlot(SaveService.FindFirstOccupiedSlotOrDefault());

            var save = SaveService.Current;
            bool readOnly = SaveService.IsActiveSaveFromNewerVersion;
            bool hasChar = save.hasCreatedCharacter;
            bool showSaveBox = hasChar || readOnly;
            bool isTest = SaveService.IsTestAllProfileActive;

            int occupiedCount = 0;
            for (int i = 0; i < SaveService.MaxCharacterSlots; i++)
            {
                var p = SaveService.PeekSlot(i);
                if (p != null && p.hasCreatedCharacter) occupiedCount++;
            }

            _btnAccountMain?.EnableInClassList("selected", !isTest);
            _btnAccountTest?.EnableInClassList("selected", isTest);
            if (_existingSaveBox != null) _existingSaveBox.style.display = showSaveBox ? DisplayStyle.Flex : DisplayStyle.None;
            if (_btnPlayContinue != null)
            {
                _btnPlayContinue.style.display = hasChar ? DisplayStyle.Flex : DisplayStyle.None;
                _btnPlayContinue.text = $"VÀO NHANH · {save.daoName} (Ô #{SaveService.ActiveSlotIndex + 1})";
            }
            if (_btnOpenCharSelect != null)
                _btnOpenCharSelect.text = occupiedCount > 0 ? $"CHỌN / TẠO NHÂN VẬT ({occupiedCount}/3 Ô)" : "TẠO NHÂN VẬT & KHAI ĐẠO (3 Ô)";
            if (_btnResetSave != null) _btnResetSave.style.display = isTest ? DisplayStyle.Flex : DisplayStyle.None;

            CodexArtLoader.ApplyIconOrPlaceholder(_portraitSlot, CultivationTable.GetPortraitCodexKey(save.gender), "[Chân Dung]", "CharacterPortrait");
            CodexArtLoader.ApplyIconOrPlaceholder(_avatarFrameSlot, CultivationTable.GetAvatarFrameCodexKey(save.equippedAvatarFrame), "");

            if (readOnly)
            {
                if (_saveDaoNameLabel != null) _saveDaoNameLabel.text = "Bản lưu thuộc phiên bản mới hơn";
                if (_saveRealmLabel != null) _saveRealmLabel.text = "Hãy cập nhật trò chơi để tiếp tục tu hành.";
            }
            else if (hasChar)
            {
                string gLbl = CultivationTable.GetGenderLabel(save.gender);
                if (_saveDaoNameLabel != null) _saveDaoNameLabel.text = $"Ô #{SaveService.ActiveSlotIndex + 1} · {save.daoName} ({gLbl})";
                if (_saveRealmLabel != null)
                    _saveRealmLabel.text = $"{CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)} · Ải {save.activeAct + 1}-{save.activeStage}";
            }

            RefreshGraphicsButtons();
            UpdateFpsLabel();
        }

        private void RefreshGraphicsButtons()
        {
            var preset = SaveService.Current.graphics.preset;
            _btnGfxLow?.EnableInClassList("selected", preset == RemakeGraphicsPreset.Low);
            _btnGfxMed?.EnableInClassList("selected", preset == RemakeGraphicsPreset.Medium);
            _btnGfxHigh?.EnableInClassList("selected", preset == RemakeGraphicsPreset.High);
            if (_btnPreset != null)
            {
                string label = preset switch
                {
                    RemakeGraphicsPreset.Low => "Đồ Họa: Thấp",
                    RemakeGraphicsPreset.Medium => "Đồ Họa: Trung",
                    _ => "Đồ Họa: Cao"
                };
                _btnPreset.text = label;
            }
        }

        private void UpdateFpsLabel()
        {
            if (_fpsLabel == null || FpsMonitorService.Instance == null) return;
            _fpsLabel.text = FpsMonitorService.Instance.DisplayText;
            _fpsLabel.style.display = SaveService.Current.graphics.showFpsCounter ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplyMenuResponsiveLayout()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;
            int sw = Mathf.Max(1, Screen.width);
            int sh = Mathf.Max(1, Screen.height);
            bool isLandscapeDesktop = (sw > sh * 1.15f) && sw >= 700 && sh >= 420;

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

            uiDocument.rootVisualElement.EnableInClassList("desktop-wide", isLandscapeDesktop);
        }
    }
}

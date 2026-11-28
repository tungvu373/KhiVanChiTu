using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    public sealed class CharacterSelectController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameObject femalePreviewModel;
        [SerializeField] private GameObject malePreviewModel;
        [SerializeField] private Transform previewTurntable;
        [SerializeField] private Transform pedestalRing;

        private Label _fpsLabel;
        private VisualElement _slotsColumn;
        private VisualElement _createBox;
        private Label _createTitle;
        private Button _btnGenderFemale;
        private Button _btnGenderMale;
        private TextField _inputDaoName;
        private Button _btnEnter;
        private VisualElement _previewGenderIcon;
        private Label _previewStageLabel;
        private VisualElement _showcaseRing;
        private VisualElement _showcaseArt;
        private VisualElement _showcaseFrame;

        private VisualElement _deleteModal;
        private Label _deleteMessage;
        private int _pendingDeleteSlot = -1;

        private int _selectedSlot;
        private RemakeCharacterGender _draftGender = RemakeCharacterGender.Female;
        private bool _userEditedName;

        private void OnEnable()
        {
            BindUi();
            _selectedSlot = SaveService.FindFirstOccupiedSlotOrDefault();
            RefreshAll();
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated += UpdateFpsLabel;
        }

        private void OnDisable()
        {
            if (FpsMonitorService.Instance != null)
                FpsMonitorService.Instance.OnFpsUpdated -= UpdateFpsLabel;
        }

        private void Update()
        {
            if (_showcaseRing != null)
            {
                float bobPx = Mathf.Sin(Time.unscaledTime * 1.85f) * 4.5f;
                _showcaseRing.style.translate = new Translate(0f, bobPx, 0f);
            }
            if (pedestalRing != null)
                pedestalRing.Rotate(0f, 0f, 14f * Time.unscaledDeltaTime, Space.Self);
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
            root.RegisterCallback<GeometryChangedEvent>(_ => ApplyResponsiveLayout());
            ApplyResponsiveLayout();

            var bg = root.Q("charselect-bg-slot");
            if (bg != null)
            {
                Texture2D bgTex = CodexArtLoader.LoadCodexTexture("Backgrounds/bg_character_select_daylight")
                    ?? CodexArtLoader.LoadCodexTexture("Backgrounds/bg_main_menu_daylight");
                if (bgTex != null) bg.style.backgroundImage = new StyleBackground(bgTex);
            }

            _fpsLabel = root.Q<Label>("cs-fps-label");
            _slotsColumn = root.Q("cs-slots-vertical");
            _createBox = root.Q("cs-create-box");
            _createTitle = root.Q<Label>("cs-create-title");
            _btnGenderFemale = root.Q<Button>("btn-gender-female");
            _btnGenderMale = root.Q<Button>("btn-gender-male");
            _inputDaoName = root.Q<TextField>("cs-input-dao-name");
            _btnEnter = root.Q<Button>("btn-cs-enter");
            _previewGenderIcon = root.Q("cs-preview-gender-icon");
            _previewStageLabel = root.Q<Label>("cs-preview-stage-label");
            _showcaseRing = root.Q("cs-2d-showcase-ring");
            _showcaseArt = root.Q("cs-2d-showcase-art");
            _showcaseFrame = root.Q("cs-2d-showcase-frame");

            _deleteModal = root.Q("cs-delete-modal");
            _deleteMessage = root.Q<Label>("cs-delete-message");

            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("ico-gender-female"), "HUD/icon_gender_female", "", "CharacterPortrait");
            CodexArtLoader.ApplyIconOrPlaceholder(root.Q("ico-gender-male"), "HUD/icon_gender_male", "", "RouteMerge");

            root.Q<Button>("btn-cs-back")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SceneFlowController.Instance?.ReturnToMainMenu();
            });

            _btnGenderFemale?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SelectDraftGender(RemakeCharacterGender.Female);
                RemakeAudioService.Instance?.TryPlayVoice(RemakeVoiceLineId.SelectPreview, RemakeCharacterGender.Female, true);
            });
            _btnGenderMale?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                SelectDraftGender(RemakeCharacterGender.Male);
                RemakeAudioService.Instance?.TryPlayVoice(RemakeVoiceLineId.SelectPreview, RemakeCharacterGender.Male, true);
            });

            _inputDaoName?.RegisterValueChangedCallback(_ => _userEditedName = true);

            root.Q<Button>("btn-cs-touch-keyboard")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                BootOverlayController.Instance?.PromptTextInput(
                    "Nhập Đạo Hiệu",
                    _inputDaoName != null ? _inputDaoName.value : "Thanh Vân Tử",
                    20,
                    val =>
                    {
                        if (_inputDaoName != null && !string.IsNullOrWhiteSpace(val))
                        {
                            _userEditedName = true;
                            _inputDaoName.SetValueWithoutNotify(val);
                        }
                    });
            });

            _btnEnter?.RegisterCallback<ClickEvent>(_ => OnEnterOrCreateClicked());

            root.Q<Button>("btn-cs-delete-cancel")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                _pendingDeleteSlot = -1;
                if (_deleteModal != null) _deleteModal.style.display = DisplayStyle.None;
            });

            root.Q<Button>("btn-cs-delete-confirm")?.RegisterCallback<ClickEvent>(_ =>
            {
                RemakeAudioService.Instance?.PlayClick();
                if (_pendingDeleteSlot >= 0 && _pendingDeleteSlot < SaveService.MaxCharacterSlots)
                {
                    SaveService.DeleteSlot(_pendingDeleteSlot);
                    BootOverlayController.Instance?.ShowNotification(RemakeNotifyCategory.Warning, $"Đã xóa nhân vật tại Ô #{_pendingDeleteSlot + 1}!", RemakeNotifyZone.GrandTop);
                    _selectedSlot = _pendingDeleteSlot;
                }
                _pendingDeleteSlot = -1;
                if (_deleteModal != null) _deleteModal.style.display = DisplayStyle.None;
                RefreshAll();
            });
        }

        public void SelectDraftGender(RemakeCharacterGender gender)
        {
            _draftGender = gender;
            if (!_userEditedName && _inputDaoName != null)
                _inputDaoName.SetValueWithoutNotify(gender == RemakeCharacterGender.Male ? "Thanh Vân Kiếm Tử" : "Thanh Vân Tiên Tử");
            RefreshGenderButtonsAndPreview();
        }

        private void OnEnterOrCreateClicked()
        {
            RemakeAudioService.Instance?.PlayClick();
            var peek = SaveService.PeekSlot(_selectedSlot);
            SaveService.SelectSlot(_selectedSlot);

            if (peek != null && peek.schemaVersion > SaveService.CurrentSchemaVersion)
            {
                BootOverlayController.Instance?.ShowNotification(RemakeNotifyCategory.Error, "Bản lưu ô này thuộc phiên bản mới hơn, không thể mở!", RemakeNotifyZone.GrandTop, true);
                return;
            }

            if (peek != null && peek.hasCreatedCharacter)
            {
                GameplayEngine.Instance?.ReloadFromActiveProfile();
                RemakeOnlineService.Instance?.SwitchAccountProfile();
                SceneFlowController.Instance?.EnterWorldAct1();
            }
            else
            {
                string chosen = _inputDaoName != null && !string.IsNullOrWhiteSpace(_inputDaoName.value)
                    ? _inputDaoName.value.Trim()
                    : (_draftGender == RemakeCharacterGender.Male ? "Thanh Vân Kiếm Tử" : "Thanh Vân Tiên Tử");
                GameplayEngine.Instance?.ReloadFromActiveProfile();
                GameplayEngine.Instance?.CreateOrSelectCharacter(chosen, _draftGender);
                RemakeOnlineService.Instance?.SwitchAccountProfile();
                SceneFlowController.Instance?.EnterWorldAct1();
            }
        }

        public void RefreshAll()
        {
            if (_slotsColumn == null) return;
            _slotsColumn.Clear();

            RemakeSaveData selectedPeek = null;
            for (int i = 0; i < SaveService.MaxCharacterSlots; i++)
            {
                int slotIdx = i;
                var data = SaveService.PeekSlot(slotIdx);
                bool occupied = data != null && data.hasCreatedCharacter;
                bool isSel = slotIdx == _selectedSlot;
                if (isSel) selectedPeek = data;

                var card = new VisualElement
                {
                    name = $"char-slot-card-{slotIdx}",
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        justifyContent = Justify.SpaceBetween,
                        marginBottom = i < SaveService.MaxCharacterSlots - 1 ? 7 : 0,
                        paddingTop = 8, paddingBottom = 8, paddingLeft = 10, paddingRight = 10,
                        borderTopWidth = isSel ? 2.5f : 1.5f,
                        borderBottomWidth = isSel ? 2.5f : 1.5f,
                        borderLeftWidth = isSel ? 2.5f : 1.5f,
                        borderRightWidth = isSel ? 2.5f : 1.5f,
                        borderTopLeftRadius = 12, borderTopRightRadius = 12,
                        borderBottomLeftRadius = 12, borderBottomRightRadius = 12,
                        borderTopColor = isSel ? new Color(0.84f, 0.58f, 0.12f) : new Color(0.74f, 0.78f, 0.74f),
                        borderBottomColor = isSel ? new Color(0.84f, 0.58f, 0.12f) : new Color(0.74f, 0.78f, 0.74f),
                        borderLeftColor = isSel ? new Color(0.84f, 0.58f, 0.12f) : new Color(0.74f, 0.78f, 0.74f),
                        borderRightColor = isSel ? new Color(0.84f, 0.58f, 0.12f) : new Color(0.74f, 0.78f, 0.74f),
                        backgroundColor = isSel ? new Color(1.00f, 0.98f, 0.90f, 0.98f) : new Color(0.98f, 0.97f, 0.93f, 0.94f)
                    }
                };

                var selectArea = new Button(() =>
                {
                    RemakeAudioService.Instance?.PlayClick();
                    _selectedSlot = slotIdx;
                    _userEditedName = false;
                    var slotPeek = SaveService.PeekSlot(slotIdx);
                    if (slotPeek != null && slotPeek.hasCreatedCharacter)
                        RemakeAudioService.Instance?.TryPlayVoice(RemakeVoiceLineId.SelectPreview, slotPeek.gender, false);
                    RefreshAll();
                })
                {
                    name = $"btn-select-slot-{slotIdx}",
                    style =
                    {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        backgroundColor = Color.clear,
                        borderTopWidth = 0, borderBottomWidth = 0, borderLeftWidth = 0, borderRightWidth = 0,
                        marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 6,
                        paddingTop = 0, paddingBottom = 0, paddingLeft = 0, paddingRight = 0
                    }
                };

                if (occupied)
                {
                    var avatarWrap = new VisualElement
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { width = 50, height = 50, minWidth = 50, minHeight = 50, marginRight = 10, alignItems = Align.Center, justifyContent = Justify.Center, flexShrink = 0 }
                    };
                    var port = new VisualElement
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { width = 42, height = 42, borderTopLeftRadius = 21, borderTopRightRadius = 21, borderBottomLeftRadius = 21, borderBottomRightRadius = 21, backgroundColor = new Color(0.99f, 0.98f, 0.95f) }
                    };
                    port.AddToClassList("art-slot");
                    CodexArtLoader.ApplyIconOrPlaceholder(port, CultivationTable.GetPortraitCodexKey(data.gender), "", "CharacterPortrait");
                    var frame = new VisualElement { pickingMode = PickingMode.Ignore };
                    frame.AddToClassList("full-screen");
                    frame.AddToClassList("art-slot");
                    CodexArtLoader.ApplyIconOrPlaceholder(frame, CultivationTable.GetAvatarFrameCodexKey(data.equippedAvatarFrame), "");
                    avatarWrap.Add(port);
                    avatarWrap.Add(frame);
                    selectArea.Add(avatarWrap);

                    double reqTv = CultivationTable.GetRequiredTuVi(data.realmIndex, data.subStage);
                    float tvPct = reqTv > 0 ? Mathf.Clamp01((float)(data.tuVi / reqTv)) : 0f;
                    string gLbl = CultivationTable.GetGenderLabel(data.gender);
                    string realmLbl = CultivationTable.GetRealmDisplayName(data.realmIndex, data.subStage);

                    var info = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexGrow = 1, flexShrink = 1, alignItems = Align.FlexStart } };
                    var topLine = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, flexWrap = Wrap.NoWrap } };
                    var slotNameLbl = new Label($"Ô {slotIdx + 1} · {data.daoName}")
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { fontSize = 16.5f, color = new Color(0.11f, 0.22f, 0.19f), marginRight = 6, overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis, whiteSpace = WhiteSpace.NoWrap, flexShrink = 1 }
                    };
                    slotNameLbl.AddToClassList("font-heading");
                    topLine.Add(slotNameLbl);
                    var gTag = new Label(gLbl)
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            fontSize = 10f, flexShrink = 0,
                            color = data.gender == RemakeCharacterGender.Male ? new Color(0.12f, 0.36f, 0.68f) : new Color(0.68f, 0.24f, 0.42f),
                            backgroundColor = data.gender == RemakeCharacterGender.Male ? new Color(0.86f, 0.93f, 1.00f) : new Color(1.00f, 0.90f, 0.94f),
                            paddingLeft = 5, paddingRight = 5, paddingTop = 1, paddingBottom = 1,
                            borderTopLeftRadius = 5, borderTopRightRadius = 5, borderBottomLeftRadius = 5, borderBottomRightRadius = 5
                        }
                    };
                    gTag.AddToClassList("font-ui-semibold");
                    topLine.Add(gTag);
                    info.Add(topLine);

                    var realmStageLbl = new Label($"{realmLbl} · Ải {data.activeAct + 1}-{data.activeStage}")
                    {
                        pickingMode = PickingMode.Ignore,
                        style = { fontSize = 14f, color = new Color(0.15f, 0.48f, 0.36f), marginTop = 2 }
                    };
                    realmStageLbl.AddToClassList("font-heading");
                    info.Add(realmStageLbl);

                    var tvRow = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, width = Length.Percent(100), marginTop = 3 } };
                    var track = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexGrow = 1, height = 6, marginRight = 6 } };
                    track.AddToClassList("bar-track");
                    track.Add(new VisualElement { pickingMode = PickingMode.Ignore, style = { width = Length.Percent(tvPct * 100f), height = Length.Percent(100), backgroundColor = new Color(0.86f, 0.60f, 0.12f) } });
                    tvRow.Add(track);
                    var tvPctLbl = new Label($"Tu Vi {Mathf.RoundToInt(tvPct * 100)}%") { pickingMode = PickingMode.Ignore, style = { fontSize = 10f, color = new Color(0.42f, 0.36f, 0.18f), flexShrink = 0 } };
                    tvPctLbl.AddToClassList("font-ui-semibold");
                    tvRow.Add(tvPctLbl);
                    info.Add(tvRow);

                    selectArea.Add(info);
                    card.Add(selectArea);

                    var delBtn = new Button(() =>
                    {
                        RemakeAudioService.Instance?.PlayClick();
                        OpenDeleteConfirm(slotIdx, data);
                    })
                    {
                        name = $"btn-delete-slot-{slotIdx}",
                        text = "Xóa",
                        style = { paddingLeft = 10, paddingRight = 10, paddingTop = 5, paddingBottom = 5, fontSize = 11f, flexShrink = 0 }
                    };
                    delBtn.AddToClassList("btn-danger");
                    card.Add(delBtn);
                }
                else
                {
                    var plus = new Label("+")
                    {
                        pickingMode = PickingMode.Ignore,
                        style =
                        {
                            width = 42, height = 42, marginRight = 10, fontSize = 22,
                            unityTextAlign = TextAnchor.MiddleCenter,
                            color = new Color(0.16f, 0.52f, 0.40f),
                            backgroundColor = new Color(0.88f, 0.95f, 0.92f),
                            borderTopLeftRadius = 21, borderTopRightRadius = 21, borderBottomLeftRadius = 21, borderBottomRightRadius = 21
                        }
                    };
                    plus.AddToClassList("font-ui-semibold");
                    selectArea.Add(plus);

                    var emptyInfo = new VisualElement { pickingMode = PickingMode.Ignore, style = { flexGrow = 1, alignItems = Align.FlexStart } };
                    var emptyTitle = new Label($"Ô Lưu #{slotIdx + 1} — Trống") { pickingMode = PickingMode.Ignore, style = { fontSize = 15.5f, color = new Color(0.24f, 0.34f, 0.30f) } };
                    emptyTitle.AddToClassList("font-heading");
                    emptyInfo.Add(emptyTitle);
                    emptyInfo.Add(new Label("Chạm để chọn giới tính Nam / Nữ và tạo nhân vật mới") { pickingMode = PickingMode.Ignore, style = { fontSize = 11.5f, color = new Color(0.45f, 0.52f, 0.49f), marginTop = 2, whiteSpace = WhiteSpace.Normal } });
                    selectArea.Add(emptyInfo);
                    card.Add(selectArea);
                }

                _slotsColumn.Add(card);
            }

            bool selOccupied = selectedPeek != null && selectedPeek.hasCreatedCharacter;
            if (_createBox != null) _createBox.style.display = selOccupied ? DisplayStyle.None : DisplayStyle.Flex;
            _showcaseRing?.EnableInClassList("compact", !selOccupied);
            if (_createTitle != null) _createTitle.text = $"Khai Mở Đạo Lộ Mới (Ô #{_selectedSlot + 1})";
            if (_btnEnter != null)
            {
                _btnEnter.text = selOccupied
                    ? $"TIẾP TỤC TU HÀNH · {selectedPeek.daoName} (Ô #{_selectedSlot + 1})"
                    : $"KHỞI TẠO & NHẬP ĐẠO (Ô #{_selectedSlot + 1})";
            }

            if (selOccupied)
            {
                Set2DPreview(
                    selectedPeek.gender,
                    selectedPeek.equippedAvatarFrame,
                    $"{CultivationTable.GetGenderLabel(selectedPeek.gender)} · {selectedPeek.daoName} ({CultivationTable.GetRealmShortName(selectedPeek.realmIndex, selectedPeek.subStage)})");
            }
            else
            {
                if (!_userEditedName && _inputDaoName != null)
                    _inputDaoName.SetValueWithoutNotify(_draftGender == RemakeCharacterGender.Male ? "Thanh Vân Kiếm Tử" : "Thanh Vân Tiên Tử");
                RefreshGenderButtonsAndPreview();
            }
        }

        private void RefreshGenderButtonsAndPreview()
        {
            _btnGenderFemale?.EnableInClassList("selected", _draftGender == RemakeCharacterGender.Female);
            _btnGenderMale?.EnableInClassList("selected", _draftGender == RemakeCharacterGender.Male);
            string label = _draftGender == RemakeCharacterGender.Male
                ? "Nam Tu · Kiếm Tu Bạch Lam (Khởi Tạo Mới)"
                : "Nữ Tu · Tiên Tử Bạch Ngọc (Khởi Tạo Mới)";
            Set2DPreview(_draftGender, 0, label);
        }

        private void Set2DPreview(RemakeCharacterGender gender, int equippedFrameId, string caption)
        {
            // Use crisp 2D showcase illustration on Character Select instead of close-up low-poly 3D models
            if (femalePreviewModel != null) femalePreviewModel.SetActive(false);
            if (malePreviewModel != null) malePreviewModel.SetActive(false);

            if (_showcaseArt != null)
            {
                string showcaseKey = gender == RemakeCharacterGender.Male ? "HUD/showcase_male_cultivator" : "HUD/showcase_female_cultivator";
                Texture2D tex = CodexArtLoader.LoadCodexTexture(showcaseKey)
                    ?? CodexArtLoader.LoadCodexTexture(CultivationTable.GetPortraitCodexKey(gender));
                if (tex != null)
                    _showcaseArt.style.backgroundImage = new StyleBackground(tex);
                else
                    CodexArtLoader.ApplyIconOrPlaceholder(_showcaseArt, CultivationTable.GetPortraitCodexKey(gender), "", "CharacterPortrait");
            }

            if (_showcaseFrame != null)
            {
                string fKey = CultivationTable.GetAvatarFrameCodexKey(equippedFrameId);
                Texture2D fTex = !string.IsNullOrEmpty(fKey) ? CodexArtLoader.LoadCodexTexture(fKey) : null;
                if (fTex != null)
                {
                    _showcaseFrame.style.backgroundImage = new StyleBackground(fTex);
                    _showcaseFrame.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _showcaseFrame.style.display = DisplayStyle.None;
                }
            }

            if (_previewStageLabel != null) _previewStageLabel.text = caption;
            if (_previewGenderIcon != null)
                CodexArtLoader.ApplyIconOrPlaceholder(_previewGenderIcon, CultivationTable.GetGenderIconCodexKey(gender), "", gender == RemakeCharacterGender.Male ? "RouteMerge" : "CharacterPortrait");
        }

        private void OpenDeleteConfirm(int slotIdx, RemakeSaveData data)
        {
            _pendingDeleteSlot = slotIdx;
            if (_deleteMessage != null)
            {
                string realm = CultivationTable.GetRealmDisplayName(data.realmIndex, data.subStage);
                string gLbl = CultivationTable.GetGenderLabel(data.gender);
                _deleteMessage.text = $"Bạn có chắc chắn muốn xóa nhân vật tại Ô #{slotIdx + 1}?\n\n" +
                    $"• Đạo Hiệu: {data.daoName} ({gLbl})\n" +
                    $"• Cảnh Giới: {realm} · Ải {data.activeAct + 1}-{data.activeStage}\n\n" +
                    "Toàn bộ Tu Vi, Phi Kiếm & Trang Bị ở ô này sẽ bị xóa vĩnh viễn!";
            }
            if (_deleteModal != null) _deleteModal.style.display = DisplayStyle.Flex;
        }

        private void UpdateFpsLabel()
        {
            if (_fpsLabel == null || FpsMonitorService.Instance == null) return;
            _fpsLabel.text = FpsMonitorService.Instance.DisplayText;
        }

        private void ApplyResponsiveLayout()
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
            var mainPanel = uiDocument.rootVisualElement.Q("cs-main-panel");
            if (mainPanel != null)
            {
                mainPanel.style.alignSelf = isLandscapeDesktop ? Align.FlexStart : Align.Center;
                mainPanel.style.maxWidth = isLandscapeDesktop ? 440 : 520;
            }
        }
    }
}

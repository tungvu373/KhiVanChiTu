using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Local gameplay choices for direct play and selected character slots.</summary>
public class GameplayChoicesService : MonoBehaviour
{
    public static GameplayChoicesService Instance { get; private set; }
    public bool HasEncounterChoice { get; private set; }
    private GameObject panel;
    private GameObject blocker;
    public bool IsChoosing => panel != null && panel.activeSelf;
    internal Transform ChoiceTransform => panel != null ? panel.transform : null;
    private Text status;
    private Button styleButton;
    private float refreshTimer;
    private int unsavedEncounterKillsRemaining;
    private CharacterSaveData Slot
    {
        get
        {
            var data = DataManager.Instance;
            if (data == null || !data.isSessionActive) return null;
            int index = data.gameSaveData.lastPlayedSlotIndex;
            return index >= 0 && index < data.gameSaveData.slots.Length ? data.gameSaveData.slots[index] : null;
        }
    }
    public int Style => Slot != null && Slot.swordStyleUnlocked ? Mathf.Clamp(Slot.swordStyle, 0, 1) : -1;
    public int ChallengeRemaining
    {
        get => Slot?.encounterKillsRemaining ?? unsavedEncounterKillsRemaining;
        private set
        {
            var slot = Slot;
            if (slot != null) slot.encounterKillsRemaining = value;
            else unsavedEncounterKillsRemaining = value;
        }
    }

    private readonly Vector2 defaultStylePos = new Vector2(-20f, 260f);
    private readonly Vector2 collapsedStylePos = new Vector2(-20f, 85f);
    private Coroutine styleSlideCoroutine;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (MobileDrawerUIController.Instance != null)
        {
            MobileDrawerUIController.Instance.OnDockCollapseChanged += OnDockCollapseChanged;
            if (styleButton != null)
            {
                var rect = (RectTransform)styleButton.transform;
                rect.anchoredPosition = MobileDrawerUIController.Instance.IsDockCollapsed ? collapsedStylePos : defaultStylePos;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
        if (MobileDrawerUIController.Instance != null)
        {
            MobileDrawerUIController.Instance.OnDockCollapseChanged -= OnDockCollapseChanged;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        unsavedEncounterKillsRemaining = 0;
        if (HasEncounterChoice) ClosePanel();
    }

    private void OnDockCollapseChanged(bool isCollapsed)
    {
        if (styleButton == null) return;
        if (styleSlideCoroutine != null) StopCoroutine(styleSlideCoroutine);
        styleSlideCoroutine = StartCoroutine(AnimateStyleButton(isCollapsed));
    }

    private System.Collections.IEnumerator AnimateStyleButton(bool collapse)
    {
        if (styleButton == null) yield break;
        var rect = (RectTransform)styleButton.transform;
        Vector2 start = rect.anchoredPosition;
        Vector2 target = collapse ? collapsedStylePos : defaultStylePos;
        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        rect.anchoredPosition = target;
        styleSlideCoroutine = null;
    }

    internal bool IsAnyModalBlocking()
    {
        if (DaoAnModalUI.Instance != null && DaoAnModalUI.Instance.IsModalOpen) return true;
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var daoAn = canvas.transform.Find("DaoAnModal");
            if (daoAn != null && daoAn.gameObject.activeInHierarchy) return true;
            var pause = canvas.transform.Find("PauseMenuModal");
            if (pause != null && pause.gameObject.activeInHierarchy) return true;
            var charDetail = canvas.transform.Find("CharacterDetailModal");
            if (charDetail != null && charDetail.gameObject.activeInHierarchy) return true;
            var trib = canvas.transform.Find("TribulationModalUI");
            if (trib != null && trib.gameObject.activeInHierarchy) return true;
            var storage = canvas.transform.Find("StorageModalUI");
            if (storage != null && storage.gameObject.activeInHierarchy) return true;
            var mile = canvas.transform.Find("MilestoneModalUI");
            if (mile != null && mile.gameObject.activeInHierarchy) return true;
            var ach = canvas.transform.Find("AchievementModalUI");
            if (ach != null && ach.gameObject.activeInHierarchy) return true;
            var dis = canvas.transform.Find("DismantleConfirmModalUI");
            if (dis != null && dis.gameObject.activeInHierarchy) return true;
            var offline = canvas.transform.Find("OfflineRewardPopupUI");
            if (offline != null && offline.gameObject.activeInHierarchy) return true;
        }
        if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay) return true;
        return false;
    }

    private void Update()
    {
        var slot = Slot;
        if (status == null && slot != null) BuildUI();

        if (CombatManager.Instance != null && CombatManager.Instance.isPlayerDead)
        {
            if (HasEncounterChoice) ClosePanel();
            if (ChallengeRemaining > 0)
            {
                ChallengeRemaining = 0;
                Notify("Thử thách thất bại. Chọn kỳ ngộ mới để thử lại.");
                Save();
            }
        }

        if (status == null) return;
        int remaining = ChallengeRemaining;
        bool showStatus = (slot != null || remaining > 0) && !IsAnyModalBlocking();
        if (styleButton.gameObject.activeSelf != showStatus) styleButton.gameObject.SetActive(showStatus);

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0) return;
        refreshTimer = 0.25f;
        // Also detects drag-merges and older saves already containing upgraded swords.
        if (slot != null && !slot.swordStyleUnlocked && HasLevel2OrHigherSword())
        {
            slot.swordStyleUnlocked = true;
            Save();
            Notify("Đã mở Kiếm Pháp. Chạm để chọn và đổi miễn phí.");
        }
        styleButton.interactable = slot != null && slot.swordStyleUnlocked && !HasEncounterChoice;
        if (slot == null) status.text = "KỲ NGỘ";
        else if (slot.swordStyleUnlocked) status.text = Style == 0 ? "KIẾM XUYÊN - ĐỔI" : "PHÁ GIÁP - ĐỔI";
        else status.text = "KIẾM PHÁP (Hợp Lv2)";
        if (remaining > 0) status.text += $" ({remaining})";
    }
    public void SelectStyle(int style)
    {
        if (Slot == null || !Slot.swordStyleUnlocked || style < 0 || style > 1) return;
        Slot.swordStyle = style;
        Save();
        ClosePanel();
    }
    public bool OfferEncounter()
    {
        if (IsChoosing || ChallengeRemaining > 0) return false;
        HasEncounterChoice = true;
        ShowPanel("KỲ NGỘ\nChọn phần thưởng hoặc nhận thử thách.");
        AddChoice("Nhận ngay 1 phôi kiếm cấp 2", 0, () => ResolveEncounter(false));

        RealmId realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi;
        int subLevel = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentSubLevel : 1;

        if (realm == RealmId.LuyenKhi && subLevel < 7)
        {
            AddChoice("Diệt 8 quái · nhận 1,000 Linh Thạch\nTử trận sẽ thất bại", 1, () => ResolveEncounter(true));
        }
        else if (realm == RealmId.LuyenKhi && subLevel >= 7)
        {
            AddChoice("Diệt 8 quái · nhận Thảo Dược Quý (nấu Đan)\nTử trận sẽ thất bại", 1, () => ResolveEncounter(true));
        }
        else
        {
            AddChoice("Diệt 8 quái · nhận 1 Đột Phá Đan\nTử trận sẽ thất bại", 1, () => ResolveEncounter(true));
        }

        AddChoice("Bỏ qua", 2, ClosePanel);
        return true;
    }
    public void ResolveEncounter(bool challenge)
    {
        if (!HasEncounterChoice) return;
        // Consume the choice before granting rewards so repeated clicks cannot grant twice.
        HasEncounterChoice = false;
        if (challenge) ChallengeRemaining = 8;
        else if (MergeManager.Instance != null && !MergeManager.Instance.TryAddSword(2))
            EconomyManager.Instance?.AddLinhThach(1000);
        AchievementManager.Instance?.OnEncounterCollected();
        ClosePanel();
        Save();
    }
    public void OnEnemyDefeated(bool boss)
    {
        int remaining = ChallengeRemaining;
        if (boss || remaining <= 0) return;
        ChallengeRemaining = remaining - 1;
        if (remaining == 1)
        {
            RealmId realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi;
            int subLevel = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentSubLevel : 1;

            if (realm == RealmId.LuyenKhi && subLevel < 7)
            {
                EconomyManager.Instance?.AddLinhThach(1000);
                Notify("Hoàn thành kỳ ngộ! Nhận 1,000 Linh Thạch.");
            }
            else if (realm == RealmId.LuyenKhi && subLevel >= 7)
            {
                if (AlchemyService.Instance != null)
                {
                    AlchemyService.Instance.AddHerb(0, 3);
                    AlchemyService.Instance.AddHerb(1, 2);
                    AlchemyService.Instance.AddHerb(2, 1);
                }
                Notify("Hoàn thành kỳ ngộ! Nhận Thảo Dược Quý (3 Linh Chi, 2 Cỏ NK, 1 Tuyết Liên) để luyện đan.");
            }
            else
            {
                AlchemyService.Instance?.GrantEncounterPill();
                Notify("Hoàn thành kỳ ngộ! Nhận 1 Đột Phá Đan.");
            }
        }
        Save();
    }
    private void Save() { DataManager.Instance?.SaveCurrentGame(true); }
    public static void Notify(string message) { GameLogger.Instance?.Log(message, Color.cyan); }

    private bool HasLevel2OrHigherSword()
    {
        if (MergeManager.Instance == null) return false;
        if (MergeManager.Instance.equipPanel != null)
        {
            foreach (Transform slot in MergeManager.Instance.equipPanel)
            {
                if (slot == null) continue;
                var item = slot.GetComponentInChildren<MergeItem>();
                if (item != null && item.level >= 2) return true;
            }
        }
        if (MergeManager.Instance.inventoryPages != null)
        {
            foreach (Transform page in MergeManager.Instance.inventoryPages)
            {
                if (page == null) continue;
                foreach (Transform slot in page)
                {
                    if (slot == null) continue;
                    var item = slot.GetComponentInChildren<MergeItem>();
                    if (item != null && item.level >= 2) return true;
                }
            }
        }
        return false;
    }

    private void BuildUI()
    {
        var root = new GameObject("GameplayChoicesCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 35;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        var safe = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        safe.transform.SetParent(root.transform, false);
        safe.GetComponent<SafeAreaFitter>().Refresh();
        styleButton = MakeButton(safe.transform, "SwordStyleButton", new Vector2(-16f, 240f), new Vector2(240f, 34f), "", OpenStyles);
        var rect = (RectTransform)styleButton.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsDockCollapsed)
        {
            rect.anchoredPosition = collapsedStylePos;
        }
        status = styleButton.GetComponentInChildren<Text>();
        if (status != null)
        {
            status.fontSize = 14;
            status.fontStyle = FontStyle.Bold;
        }
        styleButton.gameObject.SetActive(Slot != null);
        blocker = new GameObject("ChoiceBackdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        blocker.transform.SetParent(root.transform, false);
        var br = (RectTransform)blocker.transform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
        br.offsetMin = br.offsetMax = Vector2.zero;
        blocker.GetComponent<Image>().color = new Color(0, 0, 0, 0.4f);
        var bBtn = blocker.GetComponent<Button>();
        bBtn.transition = Selectable.Transition.None;
        bBtn.onClick.AddListener(ClosePanel);
        blocker.SetActive(false);
        var choiceArea = new GameObject("ChoiceSafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        choiceArea.transform.SetParent(root.transform, false);
        choiceArea.GetComponent<SafeAreaFitter>().Refresh();
        panel = new GameObject("GameplayChoicePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(choiceArea.transform, false);
        var pr = (RectTransform)panel.transform;
        pr.sizeDelta = new Vector2(960, 580);
        panel.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.98f);
        panel.SetActive(false);
    }
    public void OpenStyles()
    {
        if (Slot == null || !Slot.swordStyleUnlocked || HasEncounterChoice) return;
        ShowPanel("CHỌN KIẾM PHÁP - ĐỔI MIỄN PHÍ");
        AddChoice("Kiếm Xuyên - đánh thêm tối đa 2 quái phía sau\nMỗi mục tiêu phụ nhận 50% sát thương", 0, () => SelectStyle(0));
        AddChoice("Phá Giáp - bám cùng mục tiêu tăng sát thương\nTối đa +80%; phá hộ thể nhanh gấp đôi", 1, () => SelectStyle(1));
        AddChoice("Đóng", 2, ClosePanel);
    }
    private void ShowPanel(string title)
    {
        if (status == null) BuildUI();
        foreach (Transform child in panel.transform) Destroy(child.gameObject);
        panel.SetActive(true);
        blocker.SetActive(true);
        var heading = new GameObject("Title", typeof(RectTransform), typeof(Text));
        heading.transform.SetParent(panel.transform, false);
        ((RectTransform)heading.transform).sizeDelta = new Vector2(900, 90);
        ((RectTransform)heading.transform).anchoredPosition = new Vector2(0, 225);
        SetupText(heading.GetComponent<Text>(), title);
    }
    private void AddChoice(string text, int index, UnityEngine.Events.UnityAction action)
    { MakeButton(panel.transform, "Choice" + index, new Vector2(0, 100 - index * 140), new Vector2(880, 120), text, action); }
    internal void ClosePanel()
    {
        HasEncounterChoice = false;
        if (panel != null) panel.SetActive(false);
        if (blocker != null) blocker.SetActive(false);
    }
    private static Button MakeButton(Transform parent, string name, Vector2 position, Vector2 size, string label, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = size; rect.anchoredPosition = position;
        go.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
        var ol = go.AddComponent<Outline>();
        ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.75f);
        ol.effectDistance = new Vector2(1, -1);
        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(go.transform, false);
        var tr = (RectTransform)textObject.transform;
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(12, 4); tr.offsetMax = new Vector2(-12, -4);
        SetupText(textObject.GetComponent<Text>(), label);
        var button = go.GetComponent<Button>(); button.onClick.AddListener(action); return button;
    }
    private static void SetupText(Text text, string value)
    {
        text.font = UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value; text.fontSize = 20; text.color = new Color(0.96f, 0.88f, 0.50f, 1f);
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>Local gameplay choices, persisted inside the selected character slot.</summary>
public class GameplayChoicesService : MonoBehaviour
{
    public static GameplayChoicesService Instance { get; private set; }
    public bool HasEncounterChoice { get; private set; }
    private GameObject panel;
    private GameObject blocker;
    public bool IsChoosing => panel != null && panel.activeSelf;
    private Text status;
    private Button styleButton;
    private float refreshTimer;
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
    public int ChallengeRemaining => Slot != null ? Slot.encounterKillsRemaining : 0;

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void Update()
    {
        if (Slot == null) return;
        if (status == null) BuildUI();
        if (CombatManager.Instance != null && CombatManager.Instance.isPlayerDead)
        {
            if (HasEncounterChoice) ClosePanel();
            if (Slot.encounterKillsRemaining > 0)
            {
                Slot.encounterKillsRemaining = 0;
                Notify("Thử thách thất bại. Chọn kỳ ngộ mới để thử lại.");
                Save();
            }
        }
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0) return;
        refreshTimer = 0.25f;
        // Also detects drag-merges and older saves already containing upgraded swords.
        if (!Slot.swordStyleUnlocked && MergeManager.Instance != null)
            foreach (var item in Object.FindObjectsByType<MergeItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (item.level >= 2) { Slot.swordStyleUnlocked = true; Save(); Notify("Đã mở Kiếm Pháp. Chạm để chọn và đổi miễn phí."); break; }
        styleButton.interactable = Slot.swordStyleUnlocked && !HasEncounterChoice;
        styleButton.gameObject.SetActive(MobileDrawerUIController.Instance == null || !MobileDrawerUIController.Instance.IsBlockingGameplay);
        status.text = Slot.swordStyleUnlocked
            ? (Style == 0 ? "KIẾM XUYÊN · ĐỔI" : "PHÁ GIÁP · ĐỔI")
            : "KIẾM PHÁP · HỢP KIẾM CẤP 2 ĐỂ MỞ";
        if (ChallengeRemaining > 0) status.text += "\nKỳ ngộ: còn " + ChallengeRemaining + " quái · thưởng 1 Đột Phá Đan";
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
        if (Slot == null || HasEncounterChoice || ChallengeRemaining > 0) return false;
        HasEncounterChoice = true;
        ShowPanel("KỲ NGỘ\nChọn phần thưởng hoặc nhận thử thách.");
        AddChoice("Nhận ngay 1 phôi kiếm cấp 2", 0, () => ResolveEncounter(false));
        AddChoice("Diệt 8 quái · nhận 1 Đột Phá Đan\nTử trận sẽ thất bại", 1, () => ResolveEncounter(true));
        AddChoice("Bỏ qua", 2, ClosePanel);
        return true;
    }
    public void ResolveEncounter(bool challenge)
    {
        if (!HasEncounterChoice || Slot == null) return;
        // Consume the choice before granting rewards so repeated clicks cannot grant twice.
        HasEncounterChoice = false;
        if (challenge) Slot.encounterKillsRemaining = 8;
        else if (MergeManager.Instance != null && !MergeManager.Instance.TryAddSword(2))
            EconomyManager.Instance?.AddLinhThach(1000);
        AchievementManager.Instance?.OnEncounterCollected();
        ClosePanel();
        Save();
    }
    public void OnEnemyDefeated(bool boss)
    {
        if (boss || Slot == null || Slot.encounterKillsRemaining <= 0) return;
        Slot.encounterKillsRemaining--;
        if (Slot.encounterKillsRemaining == 0)
        {
            AlchemyService.Instance?.GrantEncounterPill();
            Notify("Hoàn thành kỳ ngộ! Nhận 1 Đột Phá Đan.");
        }
        Save();
    }
    private void Save() { DataManager.Instance?.SaveCurrentGame(true); }
    public static void Notify(string message) { GameLogger.Instance?.Log(message, Color.cyan); }

    private void BuildUI()
    {
        var root = new GameObject("GameplayChoicesCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 65;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        var safe = new GameObject("SafeArea", typeof(RectTransform));
        safe.transform.SetParent(root.transform, false);
        var safeRect = (RectTransform)safe.transform;
        safeRect.anchorMin = new Vector2(Screen.safeArea.xMin / Screen.width, Screen.safeArea.yMin / Screen.height);
        safeRect.anchorMax = new Vector2(Screen.safeArea.xMax / Screen.width, Screen.safeArea.yMax / Screen.height);
        safeRect.offsetMin = safeRect.offsetMax = Vector2.zero;
        styleButton = MakeButton(safe.transform, "SwordStyleButton", new Vector2(0, 205), new Vector2(840, 90), "", OpenStyles);
        var rect = (RectTransform)styleButton.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
        status = styleButton.GetComponentInChildren<Text>();
        blocker = new GameObject("ChoiceBackdrop", typeof(RectTransform), typeof(Image));
        blocker.transform.SetParent(safe.transform, false);
        var br = (RectTransform)blocker.transform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
        br.offsetMin = br.offsetMax = Vector2.zero;
        blocker.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
        blocker.SetActive(false);
        panel = new GameObject("GameplayChoicePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(safe.transform, false);
        var pr = (RectTransform)panel.transform;
        pr.sizeDelta = new Vector2(960, 580);
        panel.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.98f);
        panel.SetActive(false);
    }
    public void OpenStyles()
    {
        if (Slot == null || !Slot.swordStyleUnlocked || HasEncounterChoice) return;
        ShowPanel("CHỌN KIẾM PHÁP · ĐỔI MIỄN PHÍ");
        AddChoice("Kiếm Xuyên · đánh thêm tối đa 2 quái phía sau\nMỗi mục tiêu phụ nhận 50% sát thương", 0, () => SelectStyle(0));
        AddChoice("Phá Giáp · bám cùng mục tiêu tăng sát thương\nTối đa +80%; phá hộ thể nhanh gấp đôi", 1, () => SelectStyle(1));
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
    private void ClosePanel()
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
        go.GetComponent<Image>().color = new Color(0.13f, 0.22f, 0.29f);
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
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value; text.fontSize = 28; text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
    }
}

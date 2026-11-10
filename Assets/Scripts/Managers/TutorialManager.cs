using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Quản lý Hệ Thống Hướng Dẫn Tân Thủ theo phong cách Mục Tiêu Trực Quan (Target-Driven Guided Quest).
/// 1. Thanh Nhiệm Vụ Mục Tiêu (Target Quest Bar):
///    - Thiết kế nhỏ gọn, tinh tế neo ở góc trên bên trái, tuyệt đối không che khuất hay ảnh hưởng bất kỳ UI nào.
///    - Luôn hiển thị duy nhất 1 mục tiêu rõ ràng kèm tiến độ và phần thưởng nóng.
/// 2. Con Trỏ Chỉ Dẫn Động (Animated Pointer Arrow):
///    - Mũi tên bay lượn nhấp nháy, tự động trỏ thẳng vào đúng Button/Ô cần thao tác.
///    - Toàn bộ con trỏ và hào quang có blocksRaycasts = false để người chơi click xuyên qua 100% vào nút bấm.
/// 3. Hành trình 5 Bước Khai Tâm chuẩn mực:
///    - Bước 1: Để Phi Kiếm tiêu diệt 3 Yêu Thú.
///    - Bước 2: Nhấn "Ghép Nhanh" (hoặc kéo kiếm) để hợp nhất Kiếm Cấp 2.
///    - Bước 3–4: Mở và nâng một cấp Vận Khí trên HUD mobile.
///    - Bước 5: Đột Phá cảnh giới đầu tiên (thăng lên Luyện Khí Tầng 2).
/// 4. Tự động ẩn khi vào trận chiến Độ Kiếp (Thiên Kiếp), không làm ảnh hưởng giao diện combat.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Trạng thái 5 Bước Mục Tiêu")]
    public bool swordDone = false;       // Bước 1: Diệt 3 quái thường
    public bool coDuyenDone = false;     // Bước 2: Ghép kiếm lên cấp 2
    public bool upgradeDone = false;     // Bước 3: Nâng cấp Kiếm Ý
    public bool shopDone = false;        // Bước 4: Khám phá Tiệm Đan Dược
    public bool breakthroughDone = false;// Bước 5: Đột phá cảnh giới

    [Header("Legacy UI References (Tương thích ngược)")]
    [HideInInspector] public GameObject tutorialOverlay;
    [HideInInspector] public Text tutorialText;

    [Header("UI References (Có thể tự do căn chỉnh trên Canvas)")]
    public GameObject questBarObj;
    public Text txtQuestTitle;
    public Text txtQuestReward;
    public Image questBarBg;
    public Image questProgressFill;
    public Outline questBarOutline;

    [Header("Quick Milestone Action")]
    public Button btnClaimQuick;
    public Text txtClaimQuick;
    public ScrollRect descScrollRect;
    public Button btnAchievement;
    public GameObject achievementBadgeObj;

    [Header("Pointer Arrow References")]
    public GameObject pointerObj;
    public RectTransform pointerRt;
    public Text txtPointerHint;
    private RectTransform currentTargetRect;
    private int currentStepForOffset = 0;

    [Header("Căn Chỉnh Vị Trí Mũi Tên Chỉ Dẫn (Pointer Positioning)")]
    [Tooltip("Biên độ dập dềnh nảy (Pixel)")]
    public float bounceAmplitude = 8f;

    [Tooltip("Tốc độ dập dềnh nảy")]
    public float bounceSpeed = 6f;

    [Header("Offset Tinh Chỉnh Riêng Cho Từng Bước (X, Y)")]
    [Tooltip("Bước 1: Quan sát trận chiến (StageUI)")]
    public Vector2 step1Offset = new Vector2(0f, 35f);

    [Tooltip("Bước 2: Nút Ghép Nhanh (btn_QuickMerge)")]
    public Vector2 step2Offset = new Vector2(0f, 35f);

    [Tooltip("Bước 3: Nút Kiếm Ý (Card_KiemY / btnKiemY)")]
    public Vector2 step3Offset = new Vector2(0f, 35f);

    [Tooltip("Bước 4: Nút Mở Tiệm Đan Dược (btn_OpenShop)")]
    public Vector2 step4Offset = new Vector2(0f, 35f);

    [Tooltip("Bước 5: Nút Cảnh Giới TopBar (txt_Stage - Giá trị âm để trỏ từ dưới lên)")]
    public Vector2 step5Offset = new Vector2(0f, -55f);

    private Coroutine questRoutine;
    private bool isCollapsed = false;
    private Vector2 originalQuestBarSize = new Vector2(430f, 56f);
    private int startKills = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Vô hiệu hóa Overlay đen cũ nếu còn tồn tại trong Scene để không bao giờ che màn hình
        DisableLegacyOverlay();
    }

    private void Start()
    {
        EnsureUI();
        RestartTutorial();
    }

    private void Update()
    {
        // Hiệu ứng dập dềnh lượn sóng cho Mũi Tên Chỉ Dẫn
        UpdatePointerPosition();

        // Hiệu ứng nhịp đập pulse cho nút [NHẬN] khi có mốc sẵn sàng nhận thưởng
        UpdateClaimButtonPulse();
    }

    private void UpdateClaimButtonPulse()
    {
        if (btnClaimQuick != null && btnClaimQuick.gameObject.activeInHierarchy && MilestoneManager.Instance != null)
        {
            int readyCount = MilestoneManager.Instance.GetReadyToClaimCount();
            if (readyCount > 0)
            {
                float s = 1.0f + Mathf.PingPong(Time.unscaledTime * 2.5f, 0.08f);
                btnClaimQuick.transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                btnClaimQuick.transform.localScale = Vector3.one;
            }
        }

        if (achievementBadgeObj != null && AchievementManager.Instance != null)
        {
            int readyAchCount = AchievementManager.Instance.GetReadyToClaimCount();
            achievementBadgeObj.SetActive(readyAchCount > 0);
            if (readyAchCount > 0 && btnAchievement != null)
            {
                float s = 1.0f + Mathf.PingPong(Time.unscaledTime * 2.0f, 0.08f);
                btnAchievement.transform.localScale = new Vector3(s, s, 1f);
            }
            else if (btnAchievement != null)
            {
                btnAchievement.transform.localScale = Vector3.one;
            }
        }
    }

    /// <summary>
    /// Vô hiệu hóa triệt để bảng Overlay đen cũ để không bao giờ làm tối màn hình hay chặn tương tác.
    /// </summary>
    private void DisableLegacyOverlay()
    {
        GameObject legacyOverlay = GameObject.Find("TutorialOverlay");
        if (legacyOverlay != null)
        {
            legacyOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// Khởi tạo UI Target Quest Bar và Pointer Arrow nếu chưa có trong Canvas, hoặc bind component nếu đã tạo sẵn.
    /// Đảm bảo tuyệt đối không ảnh hưởng đến bất kỳ UI hiện có nào.
    /// </summary>
    public void EnsureUI()
    {
        DisableLegacyOverlay();

        Canvas canvas = FindMainCanvas();
        if (canvas == null) return;

        Font defaultFont = UiTextStyle.BodyFont;

        // ── 1. TÌM HOẶC TẠO THANH NHIỆM VỤ MỤC TIÊU (TARGET QUEST BAR) ──────────
        bool isNewQuestBar = false;
        if (questBarObj == null)
        {
            questBarObj = GameObject.Find("TutorialTargetQuestBar");
            if (questBarObj == null && canvas != null)
            {
                Transform foundTr = canvas.transform.Find("TutorialTargetQuestBar");
                if (foundTr != null) questBarObj = foundTr.gameObject;
            }
        }

        if (questBarObj == null)
        {
            questBarObj = new GameObject("TutorialTargetQuestBar", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            questBarObj.transform.SetParent(canvas.transform, false);
            isNewQuestBar = true;
        }

        RectTransform qRt = questBarObj.GetComponent<RectTransform>();

        // TUYỆT ĐỐI GIỮ NGUYÊN 100% ANCHOR, PIVOT, VỊ TRÍ NẾU OBJECT ĐÃ CÓ TRÊN SCENE DO USER CĂN CHỈNH!
        // CHỈ GÁN MẶC ĐỊNH NẾU ĐÂY LÀ OBJECT MỚI ĐƯỢC TẠO LẦN ĐẦU
        if (isNewQuestBar)
        {
            qRt.anchorMin = new Vector2(0f, 0.52f);
            qRt.anchorMax = new Vector2(0f, 0.52f);
            qRt.pivot = new Vector2(0f, 0.5f);
            qRt.anchoredPosition = new Vector2(10f, 0f);
            qRt.sizeDelta = new Vector2(260f, 44f);
            originalQuestBarSize = qRt.sizeDelta;
        }
        else
        {
            // Lưu lại kích thước do user tự căn chỉnh (nếu thanh không bị thu nhỏ)
            if (!isCollapsed && qRt.sizeDelta.x > 60f)
            {
                originalQuestBarSize = qRt.sizeDelta;
            }
        }

        questBarBg = questBarObj.GetComponent<Image>();
        if (questBarBg == null) questBarBg = questBarObj.AddComponent<Image>();
        questBarBg.color = new Color(0.06f, 0.06f, 0.12f, 0.94f);
        questBarBg.raycastTarget = true; // Cho phép bấm trực tiếp vào thanh để nhận thưởng hoặc mở bảng mốc

        // Click vào thân thanh Quest Bar để nhận thưởng nhanh hoặc mở bảng mốc
        Button qBtn = questBarObj.GetComponent<Button>();
        if (qBtn == null) qBtn = questBarObj.AddComponent<Button>();
        qBtn.transition = Selectable.Transition.None;
        qBtn.onClick.RemoveAllListeners();
        qBtn.onClick.AddListener(OnQuestBarClicked);

        questBarOutline = questBarObj.GetComponent<Outline>();
        if (questBarOutline == null) questBarOutline = questBarObj.AddComponent<Outline>();
        questBarOutline.effectColor = new Color(1f, 0.82f, 0.25f, 0.85f);
        questBarOutline.effectDistance = new Vector2(1.2f, -1.2f);
        bool wideQuestBar = originalQuestBarSize.x >= 400f;
        float actionLaneWidth = wideQuestBar ? 232f : 140f;

        // Thanh tiến độ mini ở mép dưới thanh Quest
        Transform pBgTr = questBarObj.transform.Find("MiniProgressBG");
        GameObject pBgObj = pBgTr != null ? pBgTr.gameObject : null;
        if (pBgObj == null)
        {
            pBgObj = new GameObject("MiniProgressBG", typeof(RectTransform), typeof(Image));
            pBgObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform pBgRt = pBgObj.GetComponent<RectTransform>();
        pBgRt.anchorMin = new Vector2(0, 0);
        pBgRt.anchorMax = new Vector2(1, 0);
        pBgRt.pivot = new Vector2(0, 0);
        pBgRt.anchoredPosition = Vector2.zero;
        pBgRt.sizeDelta = new Vector2(0, 4f);
        Image pBgImg = pBgObj.GetComponent<Image>();
        pBgImg.color = new Color(0, 0, 0, 0.6f);
        pBgImg.raycastTarget = false;

        Transform pFillTr = pBgObj.transform.Find("MiniProgressFill");
        GameObject pFillObj = pFillTr != null ? pFillTr.gameObject : null;
        if (pFillObj == null)
        {
            pFillObj = new GameObject("MiniProgressFill", typeof(RectTransform), typeof(Image));
            pFillObj.transform.SetParent(pBgObj.transform, false);
        }
        questProgressFill = pFillObj.GetComponent<Image>();
        questProgressFill.color = new Color(0.2f, 0.85f, 0.35f, 1f);
        questProgressFill.raycastTarget = false;
        RectTransform pFillRt = pFillObj.GetComponent<RectTransform>();
        pFillRt.anchorMin = Vector2.zero;
        pFillRt.anchorMax = new Vector2(0.2f, 1f);
        pFillRt.offsetMin = Vector2.zero;
        pFillRt.offsetMax = Vector2.zero;

        // DÒNG 1: Tên Nhiệm Vụ / Mục Tiêu Chính (TxtQuestTitle)
        Transform txtTr = questBarObj.transform.Find("TxtQuestTitle");
        GameObject txtObj = txtTr != null ? txtTr.gameObject : null;
        if (txtObj == null)
        {
            txtObj = new GameObject("TxtQuestTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
            txtObj.transform.SetParent(questBarObj.transform, false);
        }
        txtQuestTitle = txtObj.GetComponent<Text>();
        txtQuestTitle.font = defaultFont;
        txtQuestTitle.fontSize = wideQuestBar ? 18 : 13;
        txtQuestTitle.resizeTextForBestFit = true;
        txtQuestTitle.resizeTextMinSize = wideQuestBar ? 15 : 11;
        txtQuestTitle.resizeTextMaxSize = txtQuestTitle.fontSize;
        txtQuestTitle.fontStyle = FontStyle.Bold;
        txtQuestTitle.alignment = TextAnchor.MiddleLeft;
        txtQuestTitle.color = Color.white;
        txtQuestTitle.raycastTarget = false;
        Outline tOl = txtObj.GetComponent<Outline>();
        if (tOl == null) tOl = txtObj.AddComponent<Outline>();
        tOl.effectColor = Color.black;
        tOl.effectDistance = new Vector2(1f, -1f);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0f, 0.4f);
        txtRt.anchorMax = new Vector2(1f, 1f);
        txtRt.pivot = new Vector2(0f, 0.5f);
        txtRt.offsetMin = new Vector2(12f, 0f);
        txtRt.offsetMax = new Vector2(-actionLaneWidth, -2f);

        // DÒNG 2: Scroll View / Scroll Area Mô Tả & Phần Thưởng (QuestDescScrollView)
        Transform sTr = questBarObj.transform.Find("QuestDescScrollView");
        GameObject scrollObj = sTr != null ? sTr.gameObject : null;
        if (scrollObj == null)
        {
            scrollObj = new GameObject("QuestDescScrollView", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
            scrollObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform sRt = scrollObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f);
        sRt.anchorMax = new Vector2(1f, 0.4f);
        sRt.pivot = new Vector2(0f, 0.5f);
        sRt.offsetMin = new Vector2(12f, 4f);
        sRt.offsetMax = new Vector2(-actionLaneWidth, 0f);
        var scrollSurface = scrollObj.GetComponent<Image>();
        if (scrollSurface == null) scrollSurface = scrollObj.AddComponent<Image>();
        scrollSurface.color = Color.clear;
        scrollSurface.raycastTarget = true;

        descScrollRect = scrollObj.GetComponent<ScrollRect>();
        descScrollRect.horizontal = true;
        descScrollRect.vertical = false;
        descScrollRect.movementType = ScrollRect.MovementType.Clamped;
        descScrollRect.inertia = true;
        descScrollRect.scrollSensitivity = 15f;

        // Text Mô tả / Phần thưởng (TxtQuestReward) đặt bên trong Scroll View
        Transform rTr = scrollObj.transform.Find("TxtQuestReward");
        if (rTr == null) rTr = questBarObj.transform.Find("TxtQuestReward");
        GameObject rObj = rTr != null ? rTr.gameObject : null;
        if (rObj == null)
        {
            rObj = new GameObject("TxtQuestReward", typeof(RectTransform), typeof(Text), typeof(Outline));
        }
        rObj.transform.SetParent(scrollObj.transform, false);

        txtQuestReward = rObj.GetComponent<Text>();
        txtQuestReward.font = defaultFont;
        txtQuestReward.fontSize = wideQuestBar ? 15 : 11;
        txtQuestReward.alignment = TextAnchor.MiddleLeft;
        txtQuestReward.color = new Color(1f, 0.88f, 0.35f, 1f);
        txtQuestReward.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtQuestReward.verticalOverflow = VerticalWrapMode.Truncate;
        txtQuestReward.raycastTarget = false;

        Outline rOl = rObj.GetComponent<Outline>();
        if (rOl == null) rOl = rObj.AddComponent<Outline>();
        rOl.effectColor = Color.black;
        rOl.effectDistance = new Vector2(1f, -1f);

        RectTransform rRt = rObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(0f, 1f);
        rRt.pivot = new Vector2(0f, 0.5f);
        rRt.anchoredPosition = Vector2.zero;
        rRt.sizeDelta = Vector2.zero;
        var rewardSize = rObj.GetComponent<ContentSizeFitter>();
        if (rewardSize == null) rewardSize = rObj.AddComponent<ContentSizeFitter>();
        rewardSize.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        descScrollRect.content = rRt;

        // NÚT HÀNH ĐỘNG NHANH [NHẬN] / [📜 MỐC] (BtnClaimQuick)
        Transform baTr = questBarObj.transform.Find("BtnClaimQuick");
        GameObject btnActionObj = baTr != null ? baTr.gameObject : null;
        if (btnActionObj == null)
        {
            btnActionObj = new GameObject("BtnClaimQuick", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
            btnActionObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform baRt = btnActionObj.GetComponent<RectTransform>();
        baRt.anchorMin = new Vector2(1f, 0.5f);
        baRt.anchorMax = new Vector2(1f, 0.5f);
        baRt.pivot = new Vector2(1f, 0.5f);
        baRt.anchoredPosition = new Vector2(wideQuestBar ? -64f : -28f, 0f);
        baRt.sizeDelta = wideQuestBar ? new Vector2(88f, 60f) : new Vector2(74f, 34f);

        Image baImg = btnActionObj.GetComponent<Image>();
        baImg.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
        Outline baOutline = btnActionObj.GetComponent<Outline>();
        baOutline.effectColor = Color.black;
        baOutline.effectDistance = new Vector2(1f, -1f);

        Transform batTr = btnActionObj.transform.Find("Text");
        GameObject baTxtObj = batTr != null ? batTr.gameObject : null;
        if (baTxtObj == null)
        {
            baTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            baTxtObj.transform.SetParent(btnActionObj.transform, false);
        }
        txtClaimQuick = baTxtObj.GetComponent<Text>();
        txtClaimQuick.font = defaultFont;
        txtClaimQuick.fontSize = wideQuestBar ? 17 : 12;
        txtClaimQuick.fontStyle = FontStyle.Bold;
        txtClaimQuick.alignment = TextAnchor.MiddleCenter;
        txtClaimQuick.text = "Mốc";
        txtClaimQuick.color = Color.white;
        RectTransform batRt = baTxtObj.GetComponent<RectTransform>();
        batRt.anchorMin = Vector2.zero;
        batRt.anchorMax = Vector2.one;
        batRt.offsetMin = Vector2.zero;
        batRt.offsetMax = Vector2.zero;

        btnClaimQuick = btnActionObj.GetComponent<Button>();
        btnClaimQuick.onClick.RemoveAllListeners();
        btnClaimQuick.onClick.AddListener(OnQuestBarClicked);

        // Nút Thành Tựu [🏆] (BtnAchievement)
        Transform achTr = questBarObj.transform.Find("BtnAchievement");
        GameObject btnAchObj = achTr != null ? achTr.gameObject : null;
        if (btnAchObj == null)
        {
            btnAchObj = new GameObject("BtnAchievement", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
            btnAchObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform achRt = btnAchObj.GetComponent<RectTransform>();
        achRt.anchorMin = new Vector2(1f, 0.5f);
        achRt.anchorMax = new Vector2(1f, 0.5f);
        achRt.pivot = new Vector2(1f, 0.5f);
        achRt.anchoredPosition = new Vector2(wideQuestBar ? -156f : -104f, 0f);
        achRt.sizeDelta = wideQuestBar ? new Vector2(72f, 60f) : new Vector2(32f, 32f);

        Image achImg = btnAchObj.GetComponent<Image>();
        achImg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

        Outline achOl = btnAchObj.GetComponent<Outline>();
        achOl.effectColor = new Color(0.96f, 0.77f, 0.26f, 0.85f);
        achOl.effectDistance = new Vector2(1f, -1f);

        Transform achTxtTr = btnAchObj.transform.Find("Text");
        GameObject achTxtObj = achTxtTr != null ? achTxtTr.gameObject : null;
        if (achTxtObj == null)
        {
            achTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            achTxtObj.transform.SetParent(btnAchObj.transform, false);
        }
        RectTransform atRt = achTxtObj.GetComponent<RectTransform>();
        atRt.anchorMin = Vector2.zero;
        atRt.anchorMax = Vector2.one;
        atRt.sizeDelta = Vector2.zero;

        Text atTxt = achTxtObj.GetComponent<Text>();
        atTxt.font = defaultFont;
        atTxt.fontSize = wideQuestBar ? 15 : 10;
        atTxt.alignment = TextAnchor.MiddleCenter;
        atTxt.text = wideQuestBar ? "Thành\ntựu" : "Tựu";

        btnAchievement = btnAchObj.GetComponent<Button>();
        btnAchievement.onClick.RemoveAllListeners();
        btnAchievement.onClick.AddListener(OnAchievementButtonClicked);

        Transform badgeTr = btnAchObj.transform.Find("Badge");
        achievementBadgeObj = badgeTr != null ? badgeTr.gameObject : null;
        if (achievementBadgeObj == null)
        {
            achievementBadgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            achievementBadgeObj.transform.SetParent(btnAchObj.transform, false);
        }
        RectTransform badgeRt = achievementBadgeObj.GetComponent<RectTransform>();
        badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(1f, 1f);
        badgeRt.pivot = new Vector2(1f, 1f);
        badgeRt.anchoredPosition = new Vector2(4f, 4f);
        badgeRt.sizeDelta = new Vector2(12f, 12f);
        achievementBadgeObj.GetComponent<Image>().color = new Color(0.95f, 0.25f, 0.25f, 1f);
        achievementBadgeObj.SetActive(false);

        // Vô hiệu hóa nút BtnMilestone cũ nếu còn tồn tại để tránh trùng lặp
        Transform oldBtnM = questBarObj.transform.Find("BtnMilestone");
        if (oldBtnM != null) oldBtnM.gameObject.SetActive(false);

        // Nút Thu Nhỏ/Mở Rộng [-] (BtnToggle)
        Transform btTr = questBarObj.transform.Find("BtnToggle");
        GameObject btnToggleObj = btTr != null ? btTr.gameObject : null;
        if (btnToggleObj == null)
        {
            btnToggleObj = new GameObject("BtnToggle", typeof(RectTransform), typeof(Button), typeof(Text));
            btnToggleObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform btRt = btnToggleObj.GetComponent<RectTransform>();
        btRt.anchorMin = new Vector2(1, 0.5f);
        btRt.anchorMax = new Vector2(1, 0.5f);
        btRt.pivot = new Vector2(1, 0.5f);
        btRt.anchoredPosition = new Vector2(-4f, 0f);
        btRt.sizeDelta = wideQuestBar ? new Vector2(54f, 60f) : new Vector2(22f, 24f);

        Text tTxt = btnToggleObj.GetComponent<Text>();
        tTxt.font = defaultFont;
        tTxt.fontSize = wideQuestBar ? 16 : 10;
        tTxt.fontStyle = FontStyle.Bold;
        tTxt.text = isCollapsed ? "Mở" : "Gọn";
        tTxt.color = new Color(1f, 0.85f, 0.35f, 1f);
        tTxt.alignment = TextAnchor.MiddleCenter;
        tTxt.raycastTarget = true;
        btRt.SetAsLastSibling();

        Button toggleBtn = btnToggleObj.GetComponent<Button>();
        toggleBtn.onClick.RemoveAllListeners();
        toggleBtn.onClick.AddListener(() =>
        {
            isCollapsed = !isCollapsed;
            if (tTxt != null) tTxt.text = isCollapsed ? "Mở" : "Gọn";
            if (txtQuestTitle != null) txtQuestTitle.gameObject.SetActive(!isCollapsed);
            if (descScrollRect != null) descScrollRect.gameObject.SetActive(!isCollapsed);
            else if (txtQuestReward != null) txtQuestReward.gameObject.SetActive(!isCollapsed);
            if (pBgObj != null) pBgObj.SetActive(!isCollapsed);
            if (btnClaimQuick != null) btnClaimQuick.gameObject.SetActive(!isCollapsed);
            if (btnAchievement != null) btnAchievement.gameObject.SetActive(!isCollapsed);
            if (qRt != null)
            {
                qRt.sizeDelta = isCollapsed ? (wideQuestBar ? new Vector2(64f, 68f) : new Vector2(38f, 38f)) :
                    (originalQuestBarSize.x > 80f ? originalQuestBarSize : new Vector2(260f, 44f));
                if (btRt != null)
                {
                    btRt.anchorMin = isCollapsed ? new Vector2(0.5f, 0.5f) : new Vector2(1f, 0.5f);
                    btRt.anchorMax = isCollapsed ? new Vector2(0.5f, 0.5f) : new Vector2(1f, 0.5f);
                    btRt.pivot = isCollapsed ? new Vector2(0.5f, 0.5f) : new Vector2(1f, 0.5f);
                    btRt.anchoredPosition = isCollapsed ? Vector2.zero : new Vector2(-4f, 0f);
                }
            }
        });

        // ── 2. TÌM HOẶC TẠO CON TRỎ MŨI TÊN CHỈ DẪN (POINTER ARROW) ─────────────
        if (pointerObj == null)
        {
            pointerObj = GameObject.Find("TutorialPointerArrow");
            if (pointerObj == null && canvas != null)
            {
                Transform pFound = canvas.transform.Find("TutorialPointerArrow");
                if (pFound != null) pointerObj = pFound.gameObject;
            }
        }

        if (pointerObj == null)
        {
            pointerObj = new GameObject("TutorialPointerArrow", typeof(RectTransform), typeof(CanvasGroup));
            pointerObj.transform.SetParent(canvas.transform, false);

            // QUAN TRỌNG: blocksRaycasts = false để người chơi click xuyên qua vào nút bên dưới!
            CanvasGroup cg = pointerObj.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            pointerRt = pointerObj.GetComponent<RectTransform>();
            pointerRt.sizeDelta = new Vector2(80f, 80f);

            // Nhãn chỉ vị trí cần nhấn.
            GameObject arrowIconObj = new GameObject("ArrowIcon", typeof(RectTransform), typeof(Text), typeof(Outline));
            arrowIconObj.transform.SetParent(pointerObj.transform, false);
            Text arrowText = arrowIconObj.GetComponent<Text>();
            arrowText.font = defaultFont;
            arrowText.fontSize = 14;
            arrowText.fontStyle = FontStyle.Bold;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.text = "Đây";
            arrowText.color = new Color(1f, 0.85f, 0.2f);
            arrowText.raycastTarget = false;
            Outline arrowOutline = arrowIconObj.GetComponent<Outline>();
            arrowOutline.effectColor = Color.black;
            arrowOutline.effectDistance = new Vector2(2f, -2f);
            RectTransform aRt = arrowIconObj.GetComponent<RectTransform>();
            aRt.anchoredPosition = new Vector2(0, -10f);
            aRt.sizeDelta = new Vector2(50f, 50f);

            // Bong bóng gợi ý chữ nổi phía trên mũi tên
            GameObject hintObj = new GameObject("TxtHint", typeof(RectTransform), typeof(Text), typeof(Outline));
            hintObj.transform.SetParent(pointerObj.transform, false);
            txtPointerHint = hintObj.GetComponent<Text>();
            txtPointerHint.font = defaultFont;
            txtPointerHint.fontSize = 14;
            txtPointerHint.fontStyle = FontStyle.Bold;
            txtPointerHint.alignment = TextAnchor.MiddleCenter;
            txtPointerHint.text = "Bấm vào đây!";
            txtPointerHint.color = new Color(1f, 0.95f, 0.4f);
            txtPointerHint.raycastTarget = false;
            Outline hintOutline = hintObj.GetComponent<Outline>();
            hintOutline.effectColor = Color.black;
            hintOutline.effectDistance = new Vector2(1.5f, -1.5f);
            RectTransform hRt = hintObj.GetComponent<RectTransform>();
            hRt.anchoredPosition = new Vector2(0, 26f);
            hRt.sizeDelta = new Vector2(160f, 26f);

            // Đảm bảo con trỏ hiển thị sẵn sàng
            pointerObj.SetActive(true);
        }

        if (pointerObj != null)
        {
            if (pointerRt == null) pointerRt = pointerObj.GetComponent<RectTransform>();
            if (txtPointerHint == null) txtPointerHint = pointerObj.transform.Find("TxtHint")?.GetComponent<Text>();
            
            // Đảm bảo icon có ký tự chuẩn
            Text aTxt = pointerObj.transform.Find("ArrowIcon")?.GetComponent<Text>();
            if (aTxt != null && string.IsNullOrEmpty(aTxt.text)) aTxt.text = "Đây";
        }
    }

    private Canvas FindMainCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.isRootCanvas && c.gameObject.name.Contains("Canvas"))
            {
                return c;
            }
        }
        return canvases.Length > 0 ? canvases[0] : null;
    }

    /// <summary>
    /// Gắn mũi tên chỉ dẫn vào đúng tâm/mép của nút bấm mục tiêu (hỗ trợ stepIndex để nhận offset riêng).
    /// </summary>
    public void PointToTarget(RectTransform target, string hintText = "Bấm vào đây!", int stepIndex = 0)
    {
        if (target == null)
        {
            Debug.LogWarning("[TutorialManager] PointToTarget: Target RectTransform bị null!");
            return;
        }

        currentTargetRect = target;
        currentStepForOffset = stepIndex;

        if (pointerObj == null) EnsureUI();
        if (pointerObj == null) return;

        // Chỉ ẩn nếu đang runtime và target bị tắt hoàn toàn
        if (Application.isPlaying && !target.gameObject.activeInHierarchy)
        {
            pointerObj.SetActive(false);
            return;
        }

        pointerObj.transform.SetAsLastSibling(); // Luôn nổi lên trên cùng
        pointerObj.SetActive(true);

        if (txtPointerHint != null)
        {
            txtPointerHint.text = hintText;
        }

        UpdatePointerPosition();
    }

    public void HidePointer()
    {
        currentTargetRect = null;
        currentStepForOffset = 0;
        if (pointerObj != null)
        {
            pointerObj.SetActive(false);
        }
    }

    public Vector2 GetOffsetForStep(int step)
    {
        switch (step)
        {
            case 1: return step1Offset;
            case 2: return step2Offset;
            case 3: return step3Offset;
            case 4: return step4Offset;
            case 5: return step5Offset;
            default: return new Vector2(0f, 35f);
        }
    }

    /// <summary>
    /// Tính toán vị trí mũi tên chính xác theo World Corners của Target RectTransform.
    /// Hoàn toàn độc lập với Pivot hay kích thước nút.
    /// </summary>
    public void UpdatePointerPosition()
    {
        if (pointerObj == null || !pointerObj.activeSelf || currentTargetRect == null) return;

        // Nếu Target bị ẩn khi đang chơi, tạm ẩn mũi tên
        if (Application.isPlaying && !currentTargetRect.gameObject.activeInHierarchy)
        {
            pointerObj.SetActive(false);
            return;
        }

        // Lấy 4 góc World Corners chuẩn xác của RectTransform mục tiêu
        Vector3[] corners = new Vector3[4];
        currentTargetRect.GetWorldCorners(corners);
        Vector3 targetTopCenter = (corners[1] + corners[2]) * 0.5f;
        Vector3 targetBottomCenter = (corners[0] + corners[3]) * 0.5f;

        Vector2 stepOffset = GetOffsetForStep(currentStepForOffset);
        float bounce = Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceAmplitude;

        Transform hintTr = pointerObj.transform.Find("TxtHint");

        // Nếu offset.y >= 0: Nhãn nằm phía trên nút
        if (stepOffset.y >= 0)
        {
            pointerObj.transform.position = targetTopCenter + new Vector3(stepOffset.x, stepOffset.y + bounce, 0f);
            if (hintTr != null) ((RectTransform)hintTr).anchoredPosition = new Vector2(0f, 26f);
        }
        // Nếu offset.y < 0: Nhãn nằm phía dưới nút (dành cho TopBar)
        else
        {
            pointerObj.transform.position = targetBottomCenter + new Vector3(stepOffset.x, stepOffset.y - bounce, 0f);
            if (hintTr != null) ((RectTransform)hintTr).anchoredPosition = new Vector2(0f, -26f);
        }
    }

    /// <summary>
    /// Hàm hỗ trợ test / preview con trỏ trực tiếp trong Editor hoặc Inspector.
    /// </summary>
    public void TestPointAtStep(int step)
    {
        EnsureUI();
        switch (step)
        {
            case 1:
                RectTransform s1 = FindButtonRect("StageUI");
                if (s1 != null) PointToTarget(s1, "Quan sát trận chiến!", 1);
                else Debug.LogWarning("Không tìm thấy StageUI để test!");
                break;
            case 2:
                RectTransform s2 = FindButtonRect("btn_QuickMerge");
                if (s2 != null) PointToTarget(s2, "Nhấn Ghép Nhanh!", 2);
                else Debug.LogWarning("Không tìm thấy btn_QuickMerge để test!");
                break;
            case 3:
                RectTransform s3 = FindKiemYButtonRect();
                if (s3 != null) PointToTarget(s3, "Nâng Cấp Kiếm Ý!", 3);
                else Debug.LogWarning("Không tìm thấy nút Kiếm Ý để test!");
                break;
            case 4:
                RectTransform s4 = FindButtonRect("btn_OpenShop");
                if (s4 != null) PointToTarget(s4, "Mở Tiệm Đan Dược!", 4);
                else Debug.LogWarning("Không tìm thấy btn_OpenShop để test!");
                break;
            case 5:
                RectTransform s5 = FindButtonRect("txt_Stage");
                if (s5 != null) PointToTarget(s5, "Đột Phá Cảnh Giới!", 5);
                else Debug.LogWarning("Không tìm thấy txt_Stage để test!");
                break;
        }
    }

    /// <summary>
    /// Bắt đầu hoặc làm mới luồng hướng dẫn tân thủ.
    /// </summary>
    public void RestartTutorial()
    {
        if (questRoutine != null)
        {
            StopCoroutine(questRoutine);
            questRoutine = null;
        }

        // Nếu đã hoàn tất cả 5 bước tân thủ: Tiếp tục chạy hiển thị Nhiệm Vụ Mốc Tu Luyện
        if (swordDone && coDuyenDone && upgradeDone && shopDone && breakthroughDone)
        {
            HidePointer();
            questRoutine = StartCoroutine(MilestoneLoopRoutine());
            return;
        }

        questRoutine = StartCoroutine(TargetQuestLoopRoutine());
    }

    /// <summary>
    /// Vòng lặp điều phối 5 Bước Mục Tiêu (The 5-Step Golden Path):
    /// Bắt chính xác thời điểm hoàn thành và tự động điều chuyển mũi tên chỉ dẫn.
    /// </summary>
    private IEnumerator TargetQuestLoopRoutine()
    {
        EnsureUI();

        // 1. Chờ người chơi vào game thực sự (qua MainMenu)
        while (MainMenuManager.Instance != null && MainMenuManager.Instance.gameObject.activeInHierarchy)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        while (DataManager.Instance != null && !DataManager.Instance.isSessionActive)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // ═══════════════════════════════════════════════════════════════════
        // BƯỚC 1: XUẤT TRẬN THỬ LỬA (DIỆT 3 YÊU THÚ ĐẦU TIÊN)
        // ═══════════════════════════════════════════════════════════════════
        if (!swordDone)
        {
            startKills = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0;
            int targetKills = 3;

            // Mũi tên gợi ý quan sát trận chiến Combat
            RectTransform stageUiTr = null;
            GameObject sObj = GameObject.Find("StageUI");
            if (sObj != null) stageUiTr = sObj.GetComponent<RectTransform>();

            while (!swordDone)
            {
                // Tạm ẩn khi đang đối đầu Boss Thiên Kiếp
                CheckTribulationVisibility();

                int currentKills = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0;
                int currentStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
                int killedInStep = Mathf.Max(0, currentKills - startKills);

                // Nếu người chơi đã qua tầng hoặc diệt đủ 3 quái
                if (killedInStep >= targetKills || currentStage > 1)
                {
                    swordDone = true;
                    yield return CompleteStepFeedback("HOÀN THÀNH BƯỚC 1", 100);
                    break;
                }

                UpdateQuestUI($"[1/5] Để Phi Kiếm diệt 3 Yêu Thú ({killedInStep}/{targetKills})", "+100 Linh Thạch", (float)killedInStep / targetKills);
                
                if (stageUiTr != null) PointToTarget(stageUiTr, "Quan sát trận chiến!", 1);
                else HidePointer();

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BƯỚC 2: ĐÚC KIẾM HỢP NHẤT (GHÉP RA PHI KIẾM CẤP 2)
        // ═══════════════════════════════════════════════════════════════════
        if (!coDuyenDone)
        {
            // Hỗ trợ tân thủ: Đảm bảo có ít nhất 2 phôi kiếm cấp 1 để người chơi bấm nút là ghép được ngay
            EnsureSwordsForMerge();

            while (!coDuyenDone)
            {
                CheckTribulationVisibility();

                // Kiểm tra xem đã có kiếm Level >= 2 trong kho hoặc trang bị chưa
                if (HasSwordLevel(2))
                {
                    coDuyenDone = true;
                    yield return CompleteStepFeedback("GHÉP KIẾM THÀNH CÔNG!", 100);
                    break;
                }

                UpdateQuestUI("[2/5] Nhấn 'Ghép Nhanh' để tạo Kiếm Cấp 2", "+100 Linh Thạch", 0.5f);

                // Mũi tên chỉ thẳng vào nút Ghép Nhanh (btn_QuickMerge)
                RectTransform btnMergeTr = FindButtonRect("btn_QuickMerge");
                if (btnMergeTr != null)
                {
                    PointToTarget(btnMergeTr, "Nhấn Ghép Nhanh!", 2);
                }
                else
                {
                    HidePointer();
                }

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BƯỚC 3: KHÁM PHÁ VẬN KHÍ (CÔNG PHÁP KIẾM Ý TRÊN HUD CŨ)
        // ═══════════════════════════════════════════════════════════════════
        bool mobileHud = MobileDrawerUIController.Instance != null;
        if (!upgradeDone)
        {
            while (!upgradeDone)
            {
                CheckTribulationVisibility();

                int curKiemYLevel = UpgradeManager.Instance != null ? UpgradeManager.Instance.kiemYLevel : 1;
                if (mobileHud ? DaoAnModalUI.Instance?.IsModalOpen == true || HasDaoAnUpgrade() : curKiemYLevel >= 2)
                {
                    upgradeDone = true;
                    yield return CompleteStepFeedback(mobileHud ? "ĐÃ MỞ BẢNG VẬN KHÍ!" : "TĂNG CẤP KIẾM Ý THÀNH CÔNG!", 150);
                    break;
                }

                UpdateQuestUI(mobileHud ? "[3/5] Mở bảng Vận Khí" : "[3/5] Nâng cấp 1 bậc Kiếm Ý", "+150 Linh Thạch", 0.5f);

                // Mũi tên chỉ thẳng vào nút Kiếm Ý trong Upgrade Panel
                RectTransform btnKiemYTr = mobileHud ? FindButtonRect("BtnHUDDaoAn") : FindKiemYButtonRect();
                if (btnKiemYTr != null)
                {
                    PointToTarget(btnKiemYTr, mobileHud ? "Mở Vận Khí!" : "Nâng Cấp Kiếm Ý!", 3);
                }
                else
                {
                    HidePointer();
                }

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BƯỚC 4: NÂNG VẬN KHÍ (TIỆM ĐAN DƯỢC TRÊN HUD CŨ)
        // ═══════════════════════════════════════════════════════════════════
        if (!shopDone)
        {
            while (!shopDone)
            {
                CheckTribulationVisibility();

                bool stepComplete;
                if (mobileHud)
                {
                    stepComplete = HasDaoAnUpgrade();
                }
                else
                {
                    GameObject p = GameObject.Find("Canvas")?.transform.Find("ShopPanel")?.gameObject;
                    if (p == null) p = GameObject.Find("ShopPanel");
                    stepComplete = (ShopManager.Instance != null && ShopManager.Instance.isShopOpen) ||
                                   (p != null && p.activeInHierarchy);
                }

                if (stepComplete)
                {
                    shopDone = true;
                    yield return CompleteStepFeedback(mobileHud ? "ĐÃ NÂNG CẤP VẬN KHÍ!" : "ĐÃ MỞ TIỆM ĐAN DƯỢC!", 150);
                    break;
                }

                UpdateQuestUI(mobileHud ? "[4/5] Nâng 1 cấp Vận Khí" : "[4/5] Mở Tiệm Đan Dược", "+150 Linh Thạch", 0.5f);

                // Mũi tên chỉ thẳng vào nút btn_OpenShop
                RectTransform btnShopTr = mobileHud ? FindButtonRect("BtnHUDDaoAn") : FindButtonRect("btn_OpenShop");
                if (btnShopTr != null && (!mobileHud || DaoAnModalUI.Instance?.IsModalOpen != true))
                {
                    PointToTarget(btnShopTr, mobileHud ? "Mở Vận Khí!" : "Mở Tiệm Đan Dược!", 4);
                }
                else
                {
                    HidePointer();
                }

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // BƯỚC 5: TU TIÊN NHẬP ĐẠO (ĐỘT PHÁ CẢNH GIỚI ĐẦU TIÊN)
        // ═══════════════════════════════════════════════════════════════════
        if (!breakthroughDone)
        {
            while (!breakthroughDone)
            {
                CheckTribulationVisibility();

                int currentStageIdx = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
                if (currentStageIdx >= 1)
                {
                    breakthroughDone = true;
                    yield return CompleteStepFeedback("ĐỘT PHÁ THÀNH CÔNG! HOÀN TẤT TÂN THỦ!", 300);
                    break;
                }

                UpdateQuestUI("[5/5] Thu thập Tu Vi để Đột Phá Luyện Khí Tầng 2", "+300 Linh Thạch", 0.8f);

                // Mũi tên chỉ vào thanh Cảnh Giới trên TopBar
                RectTransform topBarStageTr = FindButtonRect(mobileHud ? "StagePanel" : "txt_Stage");
                if (topBarStageTr != null)
                {
                    PointToTarget(topBarStageTr, "Đột Phá Cảnh Giới!", 5);
                }
                else
                {
                    HidePointer();
                }

                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // KẾT THÚC TOÀN BỘ HƯỚNG DẪN TÂN THỦ ➔ CHUYỂN TIẾP SANG NHIỆM VỤ MỐC
        // ═══════════════════════════════════════════════════════════════════
        HidePointer();
        if (questBarObj != null)
        {
            UpdateQuestUI("Hoàn tất Khai Tâm! Mở khóa Mốc Tu Tiên", "+Quà Đan Dược & Cơ Duyên", 1f);
            if (questBarOutline != null) questBarOutline.effectColor = Color.green;
            yield return new WaitForSecondsRealtime(2.5f);
        }

        SaveProgress();
        Debug.Log("✅ [TutorialManager] Đã hoàn tất 100% Target Tutorial 5 Bước Khai Tâm! Khởi động Luồng Nhiệm Vụ Mốc.");

        // Bắt đầu chuỗi Nhiệm Vụ Mốc dài hạn
        questRoutine = StartCoroutine(MilestoneLoopRoutine());
    }

    /// <summary>
    /// Vòng lặp Nhiệm Vụ Mốc (Milestone Quest Loop):
    /// Tiếp tục ghim các mục tiêu diệt quái, đánh boss, ghép kiếm, nâng kiếm ý lên thanh Quest Bar.
    /// </summary>
    private IEnumerator MilestoneLoopRoutine()
    {
        EnsureUI();
        if (questBarObj != null) questBarObj.SetActive(true);
        HidePointer();

        while (true)
        {
            CheckTribulationVisibility();

            if (MilestoneManager.Instance != null)
            {
                MilestoneQuest q = MilestoneManager.Instance.GetNextPendingMilestone();
                if (q != null)
                {
                    bool isDone = MilestoneManager.Instance.IsCompleted(q);
                    int cur = MilestoneManager.Instance.GetCurrentProgress(q);
                    int target = q.targetAmount;
                    float ratio = Mathf.Clamp01((float)cur / target);

                    if (isDone)
                    {
                        // ĐÃ HOÀN THÀNH: Hiện rõ Tên mốc + Tiến độ (target/target) và Mô tả + Phần thưởng
                        string titleText = $"[ĐẠT MỐC] {q.title} ({target}/{target})";
                        string rewardText = $"Đã đạt mốc! Bấm [NHẬN]: {q.GetRewardSummary()}";
                        UpdateQuestUI(titleText, rewardText, 1f, isReadyToClaim: true);

                        if (questBarOutline != null)
                        {
                            float t = Mathf.PingPong(Time.unscaledTime * 3f, 1f);
                            questBarOutline.effectColor = Color.Lerp(new Color(1f, 0.85f, 0.2f), Color.green, t);
                        }
                    }
                    else
                    {
                        // ĐANG LÀM: Hiện rõ Tên mốc + Tiến độ (cur/target) và Mô tả chi tiết ở Scroll View dưới
                        string titleText = $"[Mốc] {q.title} ({Mathf.Min(cur, target)}/{target})";
                        string rewardText = string.IsNullOrEmpty(q.description)
                            ? $"Thưởng: {q.GetRewardSummary()}"
                            : $"{q.description}  |  Thưởng: {q.GetRewardSummary()}";
                        UpdateQuestUI(titleText, rewardText, ratio, isReadyToClaim: false);

                        if (questBarOutline != null)
                        {
                            questBarOutline.effectColor = q.isHard ? new Color(1f, 0.85f, 0.2f, 0.85f) : new Color(0.35f, 0.65f, 1f, 0.85f);
                        }
                    }
                }
                else
                {
                    UpdateQuestUI("Hoàn Thành Toàn Bộ Mốc Tu Đạo!", "Công Đức Viên Mãn", 1f, isReadyToClaim: false);
                    if (questBarOutline != null) questBarOutline.effectColor = Color.green;
                }
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private static bool HasDaoAnUpgrade()
    {
        var progression = DaoAnProgressionService.Instance;
        var nodes = progression?.Catalog?.nodes;
        return nodes != null && System.Array.Exists(nodes, node => node != null && progression.GetLevel(node.id) > 0);
    }

    private void UpdateQuestUI(string title, string reward, float progressRatio, bool isReadyToClaim = false)
    {
        if (questBarObj == null) EnsureUI();
        if (questBarObj != null && !questBarObj.activeSelf) questBarObj.SetActive(true);

        if (txtQuestTitle != null) txtQuestTitle.text = title;
        SetQuestReward(reward);

        if (questProgressFill != null)
        {
            RectTransform pFillRt = questProgressFill.GetComponent<RectTransform>();
            pFillRt.anchorMax = new Vector2(Mathf.Clamp01(progressRatio), 1f);
        }

        // Cập nhật trạng thái hiển thị của nút Nhận Nhanh / Xem Mốc
        if (btnClaimQuick != null && txtClaimQuick != null)
        {
            Image btnImg = btnClaimQuick.GetComponent<Image>();
            if (isReadyToClaim)
            {
                txtClaimQuick.text = "NHẬN";
                txtClaimQuick.color = new Color(0.12f, 0.08f, 0f);
                if (btnImg != null) btnImg.color = new Color(1f, 0.82f, 0.15f, 1f);
            }
            else
            {
                txtClaimQuick.text = "Mốc";
                txtClaimQuick.color = Color.white;
                if (btnImg != null) btnImg.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
            }
        }
    }

    private void SetQuestReward(string reward)
    {
        if (txtQuestReward == null || txtQuestReward.text == reward) return;
        txtQuestReward.text = reward;
        if (descScrollRect != null && descScrollRect.content != null)
        {
            descScrollRect.StopMovement();
            descScrollRect.content.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Xử lý click trực tiếp vào thanh Quest Bar hoặc nút Nhận Nhanh:
    /// Nếu có mốc hoàn thành -> Nhận ngay lập tức!
    /// Nếu chưa có mốc hoàn thành -> Mở Bảng ScrollView Nhiệm Vụ Mốc (MilestoneModalUI).
    /// </summary>
    public void OnQuestBarClicked()
    {
        if (isCollapsed)
        {
            Transform bt = questBarObj != null ? questBarObj.transform.Find("BtnToggle") : null;
            Button b = bt != null ? bt.GetComponent<Button>() : null;
            if (b != null) { b.onClick.Invoke(); return; }
        }

        if (MilestoneManager.Instance != null)
        {
            MilestoneQuest pending = MilestoneManager.Instance.GetNextPendingMilestone();
            if (pending != null && MilestoneManager.Instance.IsCompleted(pending))
            {
                MilestoneManager.Instance.ClaimReward(pending.id);
                return;
            }
        }

        OpenMilestoneModal();
    }

    public void OpenMilestoneModal()
    {
        if (MilestoneModalUI.Instance != null)
        {
            MilestoneModalUI.Instance.Open();
            return;
        }

        MilestoneModalUI found = FindObjectOfType<MilestoneModalUI>(true);
        if (found != null)
        {
            found.Open();
            return;
        }

        Debug.LogWarning("[TutorialManager] Không tìm thấy MilestoneModalUI trên Scene! Hãy dùng menu TuTien/UI/Tạo Sẵn Bảng Nhiệm Vụ Mốc.");
    }

    public void OnAchievementButtonClicked()
    {
        if (AchievementModalUI.Instance != null)
        {
            AchievementModalUI.Instance.OpenModal();
            return;
        }

        AchievementModalUI found = FindObjectOfType<AchievementModalUI>(true);
        if (found != null)
        {
            found.OpenModal();
            return;
        }

        Debug.LogWarning("[TutorialManager] Không tìm thấy AchievementModalUI trên Scene!");
    }

    private IEnumerator CompleteStepFeedback(string message, int rewardLinhThach)
    {
        HidePointer();

        // Thưởng nóng Linh Thạch cho người chơi
        if (EconomyManager.Instance != null && rewardLinhThach > 0)
        {
            EconomyManager.Instance.AddLinhThach(rewardLinhThach);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }

        // Hiệu ứng chúc mừng đổi màu xanh lá
        if (txtQuestTitle != null) txtQuestTitle.text = $"<color=#55FF55>{message}</color>";
        SetQuestReward($"+{rewardLinhThach} LT");
        if (questProgressFill != null) questProgressFill.color = Color.green;

        yield return new WaitForSecondsRealtime(1.0f);

        if (questProgressFill != null) questProgressFill.color = new Color(0.2f, 0.85f, 0.35f, 1f);
        SaveProgress();
    }

    private void CheckTribulationVisibility()
    {
        // Khi đang đánh Boss Độ Kiếp (Thiên Kiếp): Tạm ẩn toàn bộ Quest UI để người chơi tập trung
        bool isTribulation = (CombatManager.Instance != null && CombatManager.Instance.isTribulationBossActive);
        if (questBarObj != null && questBarObj.activeSelf == isTribulation)
        {
            questBarObj.SetActive(!isTribulation);
        }
        if (isTribulation && pointerObj != null && pointerObj.activeSelf)
        {
            pointerObj.SetActive(false);
        }
    }

    private void EnsureSwordsForMerge()
    {
        if (MergeManager.Instance == null) return;
        if (HasSwordLevel(2)) return;

        // Nếu chưa có kiếm cấp 2 và chưa đủ phôi để ghép, tặng 1 phôi cấp 1 vào kho
        MergeManager.Instance.TryAddSword(1);
    }

    private bool HasSwordLevel(int minLevel)
    {
        if (MergeManager.Instance == null) return false;

        // Quét ô trang bị
        if (MergeManager.Instance.equipPanel != null)
        {
            foreach (Transform slot in MergeManager.Instance.equipPanel)
            {
                if (slot == null) continue;
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null && item.level >= minLevel) return true;
            }
        }

        // Quét kho đồ
        if (MergeManager.Instance.inventoryPages != null)
        {
            foreach (Transform page in MergeManager.Instance.inventoryPages)
            {
                if (page == null) continue;
                foreach (Transform slot in page)
                {
                    if (slot == null) continue;
                    MergeItem item = slot.GetComponentInChildren<MergeItem>();
                    if (item != null && item.level >= minLevel) return true;
                }
            }
        }

        return false;
    }

    public RectTransform FindButtonRect(string buttonName)
    {
        // 1. Tìm trực tiếp nếu đang active
        GameObject go = GameObject.Find(buttonName);
        if (go != null)
        {
            return go.GetComponent<RectTransform>();
        }

        // 2. Quét sâu toàn bộ Canvas (kể cả các panel/nút đang Inactive)
        Canvas canvas = FindMainCanvas();
        if (canvas != null)
        {
            Transform[] all = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t != null && t.name == buttonName)
                {
                    return t.GetComponent<RectTransform>();
                }
            }
        }
        return null;
    }

    public RectTransform FindKiemYButtonRect()
    {
        // 1. Thử tìm qua UpgradeUIPlaceholder
        UpgradeUIPlaceholder uip = FindObjectOfType<UpgradeUIPlaceholder>();
        if (uip != null && uip.btnKiemY != null)
        {
            return uip.btnKiemY.GetComponent<RectTransform>();
        }

        // 2. Quét sâu trên Canvas tìm Card_KiemY hoặc btnKiemY (kể cả inactive)
        Canvas canvas = FindMainCanvas();
        if (canvas != null)
        {
            Transform[] all = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t != null && (t.name == "Card_KiemY" || t.name == "btnKiemY"))
                {
                    Button b = t.GetComponentInChildren<Button>(true);
                    if (b != null) return b.GetComponent<RectTransform>();
                    return t.GetComponent<RectTransform>();
                }
            }
        }

        return null;
    }

    private void SaveProgress()
    {
        if (DataManager.Instance != null && DataManager.Instance.isSessionActive)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }
    }

    // ── SAVE / LOAD THEO CHARACTER SAVE DATA (TƯƠNG THÍCH 100%) ──────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.tutorialSwordDone = swordDone;
        slot.tutorialCoDuyenDone = coDuyenDone;
        slot.tutorialUpgradeDone = upgradeDone;
        slot.tutorialShopDone = shopDone;
        slot.tutorialCompleted = (swordDone && coDuyenDone && upgradeDone && shopDone && breakthroughDone);
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        swordDone = slot.tutorialSwordDone;
        coDuyenDone = slot.tutorialCoDuyenDone;
        upgradeDone = slot.tutorialUpgradeDone;
        shopDone = slot.tutorialShopDone;
        breakthroughDone = slot.tutorialCompleted;

        if (slot.tutorialCompleted)
        {
            swordDone = true;
            coDuyenDone = true;
            upgradeDone = true;
            shopDone = true;
            breakthroughDone = true;

            HidePointer();

            if (questRoutine != null)
            {
                StopCoroutine(questRoutine);
                questRoutine = null;
            }

            // Tiếp tục khởi chạy luồng Nhiệm Vụ Mốc khi người chơi đã hoàn tất hướng dẫn
            questRoutine = StartCoroutine(MilestoneLoopRoutine());
            return;
        }

        // Nhân vật mới hoặc chưa xong hướng dẫn -> Khởi chạy luồng mục tiêu
        RestartTutorial();
    }

    // ── HÀM TƯƠNG THÍCH NGƯỢC (DỰ PHÒNG CHO EDITOR / UI BUTTONS CŨ) ──────────
    public void DismissTutorial()
    {
        HidePointer();
    }

    public void OnOverlayClicked()
    {
        DismissTutorial();
    }
}

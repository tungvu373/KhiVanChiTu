using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Modal Bảng Nhiệm Vụ Mốc (Milestone Quests Modal).
/// Cho phép người chơi theo dõi tiến độ và bấm nhận thưởng Đan Dược & Cơ Duyên.
/// </summary>
public class MilestoneModalUI : MonoBehaviour
{
    public static MilestoneModalUI Instance { get; private set; }

    [Header("Cấu trúc UI Modal")]
    public GameObject modalPanel;
    public RectTransform contentContainer; // Container chứa các hàng nhiệm vụ trong ScrollView
    public Text txtTotalProgress;
    public Button btnClose;
    public Button btnClaimAll;

    [Header("Prefab / Mẫu Hàng Nhiệm Vụ")]
    public GameObject rowTemplate;

    private List<GameObject> spawnedRows = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tự động tìm và liên kết components nếu chưa được gán trên Inspector
        if (contentContainer == null)
        {
            Transform foundContent = transform.Find("ModalBox/ScrollView/Viewport/Content");
            if (foundContent != null) contentContainer = foundContent.GetComponent<RectTransform>();
        }
        if (btnClose == null)
        {
            Transform foundClose = transform.Find("ModalBox/BtnClose");
            if (foundClose != null) btnClose = foundClose.GetComponent<Button>();
        }
        if (btnClaimAll == null)
        {
            Transform foundClaimAll = transform.Find("ModalBox/BtnClaimAll");
            if (foundClaimAll != null) btnClaimAll = foundClaimAll.GetComponent<Button>();
        }
        if (txtTotalProgress == null)
        {
            Transform foundProg = transform.Find("ModalBox/TxtTotalProgress");
            if (foundProg != null) txtTotalProgress = foundProg.GetComponent<Text>();
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Close);
        }

        if (btnClaimAll != null)
        {
            btnClaimAll.onClick.RemoveAllListeners();
            btnClaimAll.onClick.AddListener(OnClaimAllClicked);
        }

        if (modalPanel == null)
        {
            modalPanel = gameObject;
        }

        modalPanel.SetActive(false);
    }

    private void Start()
    {
        if (MilestoneManager.Instance != null)
        {
            MilestoneManager.Instance.OnMilestonesUpdated += RefreshUI;
        }
    }

    private void OnDestroy()
    {
        if (MilestoneManager.Instance != null)
        {
            MilestoneManager.Instance.OnMilestonesUpdated -= RefreshUI;
        }
    }

    public void Open()
    {
        if (modalPanel == null)
        {
            modalPanel = gameObject;
        }

        modalPanel.SetActive(true);
        transform.SetAsLastSibling(); // Luôn nổi lên trên cùng
        RefreshUI();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    public void Close()
    {
        if (modalPanel != null) modalPanel.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    public void RefreshUI()
    {
        if (MilestoneManager.Instance == null || contentContainer == null) return;

        EnsureContainerLayout();

        // 1. Cập nhật tiến độ tổng
        int claimedCount = 0;
        int totalCount = MilestoneManager.Instance.allQuests.Count;
        int readyToClaimCount = 0;

        foreach (var q in MilestoneManager.Instance.allQuests)
        {
            if (q.isClaimed) claimedCount++;
            else if (MilestoneManager.Instance.IsCompleted(q)) readyToClaimCount++;
        }

        if (txtTotalProgress != null)
        {
            txtTotalProgress.text = $"Tiến Độ Tu Đạo: {claimedCount}/{totalCount} Đạt Mốc | ({readyToClaimCount} Chờ Nhận)";
        }

        if (btnClaimAll != null)
        {
            btnClaimAll.interactable = (readyToClaimCount > 0);
            Text btnTxt = btnClaimAll.GetComponentInChildren<Text>();
            if (btnTxt != null)
            {
                btnTxt.text = readyToClaimCount > 0 ? $"✨ NHẬN TOÀN BỘ ({readyToClaimCount})" : "Không Có Thưởng Chờ";
            }
        }

        // 2. Dọn dẹp hàng cũ
        foreach (var r in spawnedRows)
        {
            if (r != null) Destroy(r);
        }
        spawnedRows.Clear();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 3. Sinh các hàng nhiệm vụ
        foreach (var quest in MilestoneManager.Instance.allQuests)
        {
            GameObject rowObj = CreateQuestRow(quest, defaultFont);
            if (rowObj != null)
            {
                spawnedRows.Add(rowObj);
            }
        }

        // Ép Layout tính toán ngay lập tức
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
    }

    private void EnsureContainerLayout()
    {
        if (contentContainer == null) return;

        // Đảm bảo Viewport dùng RectMask2D để clip mượt mà, không bị lỗi stencil Mask
        if (contentContainer.parent != null)
        {
            Mask oldMask = contentContainer.parent.GetComponent<Mask>();
            if (oldMask != null) DestroyImmediate(oldMask);

            RectMask2D rMask = contentContainer.parent.GetComponent<RectMask2D>();
            if (rMask == null) rMask = contentContainer.parent.gameObject.AddComponent<RectMask2D>();
        }

        VerticalLayoutGroup vlg = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        ContentSizeFitter csf = contentContainer.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private GameObject CreateQuestRow(MilestoneQuest quest, Font font)
    {
        GameObject row = new GameObject($"QuestRow_{quest.id}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Outline));
        row.transform.SetParent(contentContainer, false);

        LayoutElement le = row.GetComponent<LayoutElement>();
        le.minHeight = 74f;
        le.preferredHeight = 74f;
        le.flexibleHeight = 0f;
        le.minWidth = 200f;
        le.flexibleWidth = 1f;

        RectTransform rRt = row.GetComponent<RectTransform>();
        rRt.sizeDelta = new Vector2(0f, 74f);

        Image rowBg = row.GetComponent<Image>();
        rowBg.color = quest.isHard ? new Color(0.2f, 0.16f, 0.06f, 0.95f) : new Color(0.09f, 0.11f, 0.18f, 0.95f);

        Outline outline = row.GetComponent<Outline>();
        outline.effectColor = quest.isHard ? new Color(1f, 0.85f, 0.2f, 0.9f) : new Color(0.25f, 0.35f, 0.55f, 0.6f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        // ── 1. KHU VỰC THÔNG TIN (TIÊU ĐỀ & MÔ TẢ) BÊN TRÁI ──────────────────
        // Title
        GameObject titleObj = new GameObject("TxtTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(row.transform, false);
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = font;
        titleTxt.fontSize = 13;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = quest.isHard ? new Color(1f, 0.9f, 0.3f) : Color.white;
        titleTxt.text = quest.isHard ? $"{quest.title} <color=#FFD700>[✨ Cơ Duyên]</color>" : quest.title;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleObj.GetComponent<Outline>().effectColor = Color.black;
        titleObj.GetComponent<Outline>().effectDistance = new Vector2(1f, -1f);

        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.5f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0f, 0.5f);
        tRt.offsetMin = new Vector2(14f, 0f);
        tRt.offsetMax = new Vector2(-225f, -6f);

        // Desc & Reward
        GameObject descObj = new GameObject("TxtDesc", typeof(RectTransform), typeof(Text));
        descObj.transform.SetParent(row.transform, false);
        Text descTxt = descObj.GetComponent<Text>();
        descTxt.font = font;
        descTxt.fontSize = 11;
        descTxt.alignment = TextAnchor.MiddleLeft;
        descTxt.color = new Color(0.78f, 0.88f, 1f);
        descTxt.text = $"{quest.description}\n<color=#FFD700>🎁 Thưởng: {quest.GetRewardSummary()}</color>";

        RectTransform dRt = descObj.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 0f);
        dRt.anchorMax = new Vector2(1f, 0.5f);
        dRt.pivot = new Vector2(0f, 0.5f);
        dRt.offsetMin = new Vector2(14f, 6f);
        dRt.offsetMax = new Vector2(-225f, 0f);

        // ── 2. KHU VỰC TIẾN ĐỘ (GIỮA - BÊN TRÁI NÚT NHẬN) ─────────────────────
        int curProgress = MilestoneManager.Instance.GetCurrentProgress(quest);
        int target = quest.targetAmount;
        float ratio = Mathf.Clamp01((float)curProgress / target);
        bool isDone = MilestoneManager.Instance.IsCompleted(quest);

        GameObject pBgObj = new GameObject("ProgBG", typeof(RectTransform), typeof(Image), typeof(Outline));
        pBgObj.transform.SetParent(row.transform, false);
        RectTransform pBgRt = pBgObj.GetComponent<RectTransform>();
        pBgRt.anchorMin = new Vector2(1f, 0.5f);
        pBgRt.anchorMax = new Vector2(1f, 0.5f);
        pBgRt.pivot = new Vector2(1f, 0.5f);
        pBgRt.anchoredPosition = new Vector2(-124f, 0f);
        pBgRt.sizeDelta = new Vector2(92f, 22f);
        pBgObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        pBgObj.GetComponent<Outline>().effectColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);

        GameObject pFillObj = new GameObject("ProgFill", typeof(RectTransform), typeof(Image));
        pFillObj.transform.SetParent(pBgObj.transform, false);
        Image pFill = pFillObj.GetComponent<Image>();
        pFill.color = isDone ? new Color(0.2f, 0.9f, 0.35f) : new Color(0.25f, 0.65f, 1f);
        RectTransform pfRt = pFillObj.GetComponent<RectTransform>();
        pfRt.anchorMin = Vector2.zero;
        pfRt.anchorMax = new Vector2(ratio, 1f);
        pfRt.offsetMin = Vector2.zero;
        pfRt.offsetMax = Vector2.zero;

        GameObject pTxtObj = new GameObject("ProgTxt", typeof(RectTransform), typeof(Text), typeof(Outline));
        pTxtObj.transform.SetParent(pBgObj.transform, false);
        Text pTxt = pTxtObj.GetComponent<Text>();
        pTxt.font = font;
        pTxt.fontSize = 11;
        pTxt.fontStyle = FontStyle.Bold;
        pTxt.alignment = TextAnchor.MiddleCenter;
        pTxt.color = Color.white;
        pTxt.text = $"{Mathf.Min(curProgress, target)}/{target}";
        pTxtObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform ptRt = pTxtObj.GetComponent<RectTransform>();
        ptRt.anchorMin = Vector2.zero;
        ptRt.anchorMax = Vector2.one;
        ptRt.offsetMin = Vector2.zero;
        ptRt.offsetMax = Vector2.zero;

        // ── 3. KHU VỰC NÚT HÀNH ĐỘNG (BÊN PHẢI CÙNG) ────────────────────────
        GameObject btnObj = new GameObject("BtnAction", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
        btnObj.transform.SetParent(row.transform, false);
        RectTransform bRt = btnObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(1f, 0.5f);
        bRt.anchorMax = new Vector2(1f, 0.5f);
        bRt.pivot = new Vector2(1f, 0.5f);
        bRt.anchoredPosition = new Vector2(-12f, 0f);
        bRt.sizeDelta = new Vector2(104f, 38f);

        Image btnImg = btnObj.GetComponent<Image>();
        Button btn = btnObj.GetComponent<Button>();
        Outline btnOutline = btnObj.GetComponent<Outline>();

        GameObject btnTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(Text));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        Text bTxt = btnTxtObj.GetComponent<Text>();
        bTxt.font = font;
        bTxt.fontSize = 12;
        bTxt.fontStyle = FontStyle.Bold;
        bTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform btRt = btnTxtObj.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.offsetMin = Vector2.zero;
        btRt.offsetMax = Vector2.zero;

        if (quest.isClaimed)
        {
            btnImg.color = new Color(0.18f, 0.28f, 0.2f, 0.7f);
            btnOutline.effectColor = Color.black;
            bTxt.text = "✓ Đã Nhận";
            bTxt.color = new Color(0.7f, 0.95f, 0.7f);
            btn.interactable = false;
        }
        else if (isDone)
        {
            btnImg.color = new Color(1f, 0.82f, 0.15f, 1f);
            btnOutline.effectColor = new Color(1f, 1f, 0.6f, 1f);
            bTxt.text = "🎁 NHẬN";
            bTxt.color = new Color(0.15f, 0.1f, 0.02f);
            btn.interactable = true;

            string qId = quest.id;
            btn.onClick.AddListener(() =>
            {
                MilestoneManager.Instance.ClaimReward(qId);
            });
        }
        else
        {
            btnImg.color = new Color(0.2f, 0.22f, 0.28f, 0.8f);
            btnOutline.effectColor = Color.black;
            bTxt.text = "Chưa Đạt";
            bTxt.color = new Color(0.6f, 0.6f, 0.6f);
            btn.interactable = false;
        }

        return row;
    }

    private void OnClaimAllClicked()
    {
        if (MilestoneManager.Instance == null) return;

        int claimed = 0;
        foreach (var q in MilestoneManager.Instance.allQuests)
        {
            if (!q.isClaimed && MilestoneManager.Instance.IsCompleted(q))
            {
                MilestoneManager.Instance.ClaimReward(q.id);
                claimed++;
            }
        }

        if (claimed > 0)
        {
            RefreshUI();
        }
    }
}

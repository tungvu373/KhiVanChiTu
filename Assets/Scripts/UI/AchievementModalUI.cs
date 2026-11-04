using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Modal Bảng Thành Tựu Tiên Lộ (Achievement Modal UI).
/// Thiết kế chuẩn Mobile Portrait 9:16, hỗ trợ 6 bộ lọc danh mục, tiến độ trực quan và nhận thưởng nhanh.
/// </summary>
public class AchievementModalUI : MonoBehaviour
{
    private static AchievementModalUI _instance;
    public static AchievementModalUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AchievementModalUI>(true);
            }
            return _instance;
        }
    }

    public bool IsModalOpen => isOpen;

    [Header("Cấu trúc Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform modalBox;
    public Button btnClose;
    public Button btnBackdrop;

    [Header("Tiến độ & Nút Nhận")]
    public Text txtTotalProgress;
    public Image fillTotalProgress;
    public Button btnClaimAll;
    public Text txtClaimAll;

    [Header("Bộ Lọc Danh Mục (Tabs)")]
    public Button[] tabButtons = new Button[6]; // 0: Tất Cả, 1: Tu Luyện, 2: Chiến Đấu, 3: Kiếm Đạo, 4: Bách Nghệ, 5: Kỳ Duyên
    public AchievementCategory selectedCategory = AchievementCategory.All;

    [Header("Nội dung Danh Sách")]
    public RectTransform contentContainer;
    public ScrollRect scrollRect;

    private List<GameObject> spawnedRows = new List<GameObject>();
    private Coroutine animRoutine;
    private bool isOpen = false;
    private Font cachedFont;

    private Font GetFont()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return cachedFont;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        cachedFont = GetFont();

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        WireEvents();
    }

    private void Start()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementsUpdated += OnAchievementsUpdatedHandler;
        }

        if (!isOpen)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementsUpdated -= OnAchievementsUpdatedHandler;
        }
    }

    private void OnAchievementsUpdatedHandler()
    {
        // Tối ưu hiệu năng: Chỉ tái tạo UI cards khi modal ĐANG MỞ!
        if (isOpen && gameObject.activeInHierarchy)
        {
            RefreshUI();
        }
    }

    private void WireEvents()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(CloseModal);
        }

        if (btnBackdrop != null)
        {
            btnBackdrop.onClick.RemoveAllListeners();
            btnBackdrop.onClick.AddListener(CloseModal);
        }

        if (btnClaimAll != null)
        {
            btnClaimAll.onClick.RemoveAllListeners();
            btnClaimAll.onClick.AddListener(OnClaimAllClicked);
        }

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int catIndex = i;
                if (tabButtons[i] != null)
                {
                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => SelectCategory((AchievementCategory)catIndex));
                }
            }
        }
    }

    // ── MỞ & ĐÓNG MODAL ──────────────────────────────────────────────────────
    public static void Open()
    {
        if (Instance != null) Instance.OpenModal();
    }

    public static void Close()
    {
        if (Instance != null) Instance.CloseModal();
    }

    public void OpenModal()
    {
        isOpen = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        UpdateTabVisuals();
        RefreshUI();

        Time.timeScale = 0f; // Đóng băng thế giới để người chơi duyệt thành tựu

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen());
    }

    public void CloseModal()
    {
        isOpen = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateClose());
    }

    public void SwitchToMilestones()
    {
        CloseModal();
        if (MilestoneModalUI.Instance != null) MilestoneModalUI.Instance.Open();
        else
        {
            var found = FindObjectOfType<MilestoneModalUI>(true);
            if (found != null) found.Open();
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (modalBox != null)
        {
            float duration = 0.16f;
            float elapsed = 0f;
            Vector3 startScale = new Vector3(0.88f, 0.88f, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                modalBox.localScale = Vector3.Lerp(startScale, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
                if (canvasGroup != null) canvasGroup.alpha = t;
                yield return null;
            }
            modalBox.localScale = Vector3.one;
        }
        animRoutine = null;
    }

    private IEnumerator AnimateClose()
    {
        if (modalBox != null)
        {
            float duration = 0.12f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                modalBox.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.9f, 0.9f, 1f), t);
                if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                yield return null;
            }
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        gameObject.SetActive(false);
        animRoutine = null;
    }

    // ── TABS & BỘ LỌC ────────────────────────────────────────────────────────
    public void SelectCategory(AchievementCategory category)
    {
        selectedCategory = category;
        UpdateTabVisuals();
        RefreshUI();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f; // Cuộn lên đầu
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private void UpdateTabVisuals()
    {
        if (tabButtons == null) return;
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] == null) continue;
            bool isSelected = (i == (int)selectedCategory);
            Image btnImg = tabButtons[i].GetComponent<Image>();
            Text btnTxt = tabButtons[i].GetComponentInChildren<Text>();

            if (btnImg != null)
            {
                btnImg.color = isSelected 
                    ? new Color(0.96f, 0.77f, 0.26f, 0.95f) // Gold nổi bật
                    : new Color(0.12f, 0.16f, 0.24f, 0.90f); // Dark Slate
            }
            if (btnTxt != null)
            {
                btnTxt.color = isSelected ? new Color(0.08f, 0.08f, 0.12f, 1f) : new Color(0.75f, 0.82f, 0.92f, 1f);
                btnTxt.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }

    // ── CẬP NHẬT UI TOÀN BỘ ──────────────────────────────────────────────────
    public void RefreshUI()
    {
        if (!isOpen && !gameObject.activeInHierarchy) return;
        var mgr = AchievementManager.Instance;
        if (mgr == null || contentContainer == null) return;

        // 1. Tiến độ tổng
        int claimedCount = mgr.GetTotalClaimedCount();
        int totalCount = mgr.allAchievements.Count;
        int readyToClaimCount = mgr.GetReadyToClaimCount(AchievementCategory.All);

        if (txtTotalProgress != null)
        {
            float pct = totalCount > 0 ? ((float)claimedCount / totalCount) * 100f : 0f;
            txtTotalProgress.text = $"Tiến Độ Tiên Lộ: {claimedCount}/{totalCount} ({pct:F0}%) | <color=#FFD700>{readyToClaimCount} Chờ Nhận</color>";
        }

        if (fillTotalProgress != null)
        {
            fillTotalProgress.fillAmount = totalCount > 0 ? (float)claimedCount / totalCount : 0f;
        }

        // 2. Nút Nhận Toàn Bộ
        if (btnClaimAll != null)
        {
            int readyInCat = mgr.GetReadyToClaimCount(selectedCategory);
            btnClaimAll.interactable = (readyInCat > 0);
            if (txtClaimAll != null)
            {
                txtClaimAll.text = readyInCat > 0 ? $"✨ NHẬN TOÀN BỘ ({readyInCat})" : "Không Có Thưởng Chờ";
            }
        }

        // 3. Xóa các card cũ
        foreach (var row in spawnedRows)
        {
            if (row != null)
            {
                if (Application.isPlaying) Destroy(row);
                else DestroyImmediate(row);
            }
        }
        spawnedRows.Clear();

        // 4. Lấy danh sách thành tựu theo tab
        List<AchievementItem> list = mgr.GetAchievementsByCategory(selectedCategory);

        // Sắp xếp: Chờ nhận lên đầu -> Đang làm dở -> Đã nhận xuống cuối
        list.Sort((a, b) =>
        {
            bool aReady = !a.isClaimed && mgr.IsCompleted(a);
            bool bReady = !b.isClaimed && mgr.IsCompleted(b);
            if (aReady != bReady) return aReady ? -1 : 1;
            if (a.isClaimed != b.isClaimed) return a.isClaimed ? 1 : -1;
            return 0;
        });

        foreach (var item in list)
        {
            GameObject rowObj = CreateAchievementCard(item, mgr);
            if (rowObj != null)
            {
                spawnedRows.Add(rowObj);
            }
        }
    }

    private GameObject CreateAchievementCard(AchievementItem item, AchievementManager mgr)
    {
        bool isDone = mgr.IsCompleted(item);
        bool isClaimed = item.isClaimed;
        bool isReady = isDone && !isClaimed;
        int curProgress = mgr.GetCurrentProgress(item);
        int target = item.targetAmount;
        float progressPct = Mathf.Clamp01((float)curProgress / target);

        // ── Card Root ──
        GameObject card = new GameObject($"Card_{item.id}", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(contentContainer, false);

        RectTransform cRt = card.GetComponent<RectTransform>();
        cRt.sizeDelta = new Vector2(0f, 92f);

        Image cImg = card.GetComponent<Image>();
        // Nền: Vàng kim nhạt nếu sẵn sàng nhận, Đen xám nếu chưa/đã nhận
        cImg.color = isReady
            ? new Color(0.18f, 0.22f, 0.16f, 0.98f) // Khẽ ánh xanh lục ngọc
            : (isClaimed ? new Color(0.08f, 0.10f, 0.14f, 0.85f) : new Color(0.11f, 0.14f, 0.20f, 0.95f));

        Outline cOl = card.GetComponent<Outline>();
        cOl.effectColor = isReady
            ? new Color(0.3f, 0.9f, 0.4f, 0.8f) // Xanh lục phát sáng
            : (isClaimed ? new Color(0.2f, 0.25f, 0.35f, 0.4f) : new Color(0.25f, 0.35f, 0.5f, 0.6f));
        cOl.effectDistance = new Vector2(1f, -1f);

        // ── Left Badge (Icon Phân Loại) ──
        GameObject iconObj = new GameObject("IconBadge", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(card.transform, false);
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0f, 0.5f);
        iconRt.anchoredPosition = new Vector2(12f, 0f);
        iconRt.sizeDelta = new Vector2(56f, 56f);

        Image iconBg = iconObj.GetComponent<Image>();
        iconBg.color = isReady ? new Color(0.96f, 0.77f, 0.26f, 0.25f) : new Color(0.15f, 0.20f, 0.28f, 0.7f);

        GameObject iconTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        iconTxtObj.transform.SetParent(iconObj.transform, false);
        RectTransform itRt = iconTxtObj.GetComponent<RectTransform>();
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.sizeDelta = Vector2.zero;

        Text iconTxt = iconTxtObj.GetComponent<Text>();
        iconTxt.font = GetFont();
        iconTxt.fontSize = 28;
        iconTxt.alignment = TextAnchor.MiddleCenter;
        iconTxt.text = GetCategoryIcon(item.category);

        // ── Title & Category (Dòng 1) ──
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(card.transform, false);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0f, 1f);
        tRt.anchoredPosition = new Vector2(78f, -8f);
        tRt.sizeDelta = new Vector2(-195f, 22f);

        Text tTxt = titleObj.GetComponent<Text>();
        tTxt.font = GetFont();
        tTxt.fontSize = 14;
        tTxt.fontStyle = FontStyle.Bold;
        tTxt.color = isReady ? new Color(1f, 0.9f, 0.4f, 1f) : (isClaimed ? Color.gray : Color.white);
        tTxt.text = item.title;

        // ── Description (Dòng 2) ──
        GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(Text));
        descObj.transform.SetParent(card.transform, false);
        RectTransform dRt = descObj.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 1f);
        dRt.anchorMax = new Vector2(1f, 1f);
        dRt.pivot = new Vector2(0f, 1f);
        dRt.anchoredPosition = new Vector2(78f, -30f);
        dRt.sizeDelta = new Vector2(-195f, 18f);

        Text dTxt = descObj.GetComponent<Text>();
        dTxt.font = GetFont();
        dTxt.fontSize = 11;
        dTxt.color = isClaimed ? new Color(0.5f, 0.5f, 0.6f, 1f) : new Color(0.72f, 0.78f, 0.88f, 1f);
        dTxt.text = item.description;

        // ── Mini Progress Bar (Dòng 3) ──
        GameObject barBg = new GameObject("ProgBG", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(card.transform, false);
        RectTransform barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0f, 0f);
        barBgRt.anchorMax = new Vector2(1f, 0f);
        barBgRt.pivot = new Vector2(0f, 0f);
        barBgRt.anchoredPosition = new Vector2(78f, 26f);
        barBgRt.sizeDelta = new Vector2(-195f, 12f);

        Image barBgImg = barBg.GetComponent<Image>();
        barBgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        GameObject barFill = new GameObject("ProgFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform fillRt = barFill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(progressPct, 1f);
        fillRt.sizeDelta = Vector2.zero;

        Image fillImg = barFill.GetComponent<Image>();
        fillImg.color = isDone ? new Color(0.2f, 0.85f, 0.35f, 1f) : new Color(0.96f, 0.77f, 0.26f, 0.85f);

        // Progress Text
        GameObject progTxtObj = new GameObject("ProgText", typeof(RectTransform), typeof(Text));
        progTxtObj.transform.SetParent(barBg.transform, false);
        RectTransform ptRt = progTxtObj.GetComponent<RectTransform>();
        ptRt.anchorMin = Vector2.zero;
        ptRt.anchorMax = Vector2.one;
        ptRt.sizeDelta = Vector2.zero;

        Text ptTxt = progTxtObj.GetComponent<Text>();
        ptTxt.font = GetFont();
        ptTxt.fontSize = 9;
        ptTxt.alignment = TextAnchor.MiddleCenter;
        ptTxt.color = Color.white;
        ptTxt.text = isDone ? $"{target} / {target} (Hoàn Thành)" : $"{curProgress} / {target} ({progressPct * 100f:F0}%)";

        // ── Reward Text (Dòng 4 mép dưới) ──
        GameObject rewObj = new GameObject("Reward", typeof(RectTransform), typeof(Text));
        rewObj.transform.SetParent(card.transform, false);
        RectTransform rRt = rewObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0f);
        rRt.pivot = new Vector2(0f, 0f);
        rRt.anchoredPosition = new Vector2(78f, 8f);
        rRt.sizeDelta = new Vector2(-195f, 16f);

        Text rTxt = rewObj.GetComponent<Text>();
        rTxt.font = GetFont();
        rTxt.fontSize = 11;
        rTxt.color = new Color(0.96f, 0.77f, 0.26f, 1f);
        rTxt.text = item.GetRewardSummary();

        // ── Right Action Button ──
        GameObject btnObj = new GameObject("BtnAction", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnObj.transform.SetParent(card.transform, false);
        RectTransform bRt = btnObj.GetComponent<RectTransform>();
        bRt.anchorMin = bRt.anchorMax = new Vector2(1f, 0.5f);
        bRt.pivot = new Vector2(1f, 0.5f);
        bRt.anchoredPosition = new Vector2(-12f, 0f);
        bRt.sizeDelta = new Vector2(100f, 44f);

        Image bImg = btnObj.GetComponent<Image>();
        Button btn = btnObj.GetComponent<Button>();
        Outline bOl = btnObj.GetComponent<Outline>();

        GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        RectTransform btRt = btnTxtObj.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.sizeDelta = Vector2.zero;

        Text bTxt = btnTxtObj.GetComponent<Text>();
        bTxt.font = GetFont();
        bTxt.fontSize = 13;
        bTxt.fontStyle = FontStyle.Bold;
        bTxt.alignment = TextAnchor.MiddleCenter;

        if (isClaimed)
        {
            bImg.color = new Color(0.12f, 0.15f, 0.20f, 0.8f);
            bOl.effectColor = Color.clear;
            bTxt.text = "ĐÃ NHẬN";
            bTxt.color = new Color(0.45f, 0.52f, 0.62f, 1f);
            btn.interactable = false;
        }
        else if (isReady)
        {
            bImg.color = new Color(0.18f, 0.80f, 0.35f, 1f); // Xanh lục ngọc
            bOl.effectColor = new Color(0.96f, 0.77f, 0.26f, 1f);
            bOl.effectDistance = new Vector2(1.2f, -1.2f);
            bTxt.text = "✨ NHẬN";
            bTxt.color = Color.white;
            btn.interactable = true;

            string idToClaim = item.id;
            btn.onClick.AddListener(() =>
            {
                mgr.ClaimReward(idToClaim);
            });
        }
        else
        {
            bImg.color = new Color(0.14f, 0.18f, 0.26f, 0.8f);
            bOl.effectColor = new Color(0.2f, 0.25f, 0.35f, 0.5f);
            bOl.effectDistance = new Vector2(1f, -1f);
            bTxt.text = $"{curProgress}/{target}";
            bTxt.color = new Color(0.6f, 0.68f, 0.78f, 1f);
            btn.interactable = false;
        }

        return card;
    }

    private string GetCategoryIcon(AchievementCategory cat)
    {
        switch (cat)
        {
            case AchievementCategory.Cultivation: return "⚡";
            case AchievementCategory.Combat: return "⚔️";
            case AchievementCategory.Sword: return "🗡️";
            case AchievementCategory.Arts: return "⚗️";
            case AchievementCategory.Destiny: return "💎";
            default: return "🏆";
        }
    }

    private void OnClaimAllClicked()
    {
        if (AchievementManager.Instance != null)
        {
            int count = AchievementManager.Instance.ClaimAllCompleted(selectedCategory);
            if (count > 0 && GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"🎉 Nhận thành công trọn bộ {count} Thành Tựu!", Color.yellow);
            }
        }
    }
}

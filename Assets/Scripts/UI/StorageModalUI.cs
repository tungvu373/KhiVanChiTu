using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StorageCategory
{
    All = 0,
    Swords = 1,
    Pills = 2,
    Materials = 3,
    Pending = 4
}

public enum StorageViewMode
{
    GridSlots = 0,
    CompactList = 1
}

/// <summary>
/// GIAO DIỆN TÚI TRỮ VẬT, BỘ LỌC ƯU TIÊN & LÒ PHÂN GIẢI (STORAGE MODAL UI).
/// Thiết kế chuẩn Mobile Portrait 9:16:
/// - 5 Tab phân loại linh hoạt.
/// - Chuyển đổi linh hoạt giữa Dạng Ô (Grid Slots - 6 cột túi đồ RPG) và Dạng Danh Sách (Compact List).
/// - Khung cuộn mượt mà có RectMask2D chống tràn viền.
/// - Cài đặt bộ lọc thu thập ưu tiên (Loot Priority Filter).
/// - Xem chi tiết tài nguyên, tách vật phẩm đơn lẻ hoặc phân giải phế phẩm hàng loạt.
/// - Bảo vệ vật phẩm quý qua DismantleConfirmModalUI.
/// </summary>
public class StorageModalUI : MonoBehaviour
{
    private static StorageModalUI _instance;
    public static StorageModalUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StorageModalUI>(true);
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

    [Header("Tài Nguyên Ví")]
    public Text txtLinhThach;
    public Text txtKiemY;
    public Text txtOre;
    public Text txtDust;

    [Header("Bộ Lọc Danh Mục (Tabs)")]
    public Button[] tabButtons = new Button[5]; // 0: Tất Cả, 1: Kiếm Phôi, 2: Đan Dược, 3: Nguyên Liệu, 4: Hòm Tạm
    public StorageCategory selectedCategory = StorageCategory.All;

    [Header("Cài Đặt Bộ Lọc Thu Thập Ưu Tiên (Priority Filter Toggles)")]
    public Toggle toggleSwordHighLevel;
    public Toggle togglePillBreakthrough;
    public Toggle toggleAutoDismantleLv1;

    [Header("Chế Độ Hiển Thị (Dạng Ô / Danh Sách)")]
    public StorageViewMode currentViewMode = StorageViewMode.GridSlots;
    public Button btnToggleView;
    public Text txtToggleView;
    public RectTransform gridContent;
    public RectTransform listContent;

    [Header("Nội dung Danh Sách")]
    public RectTransform contentContainer;
    public ScrollRect scrollRect;
    public Text txtEmptyNotice;

    [Header("Nút Hành Động Chân Trang (Footer)")]
    public Button btnClaimPriority;
    public Text txtClaimPriority;
    public Button btnBatchDismantle;
    public Text txtBatchDismantle;

    private List<GameObject> spawnedRows = new List<GameObject>();
    private Coroutine animRoutine;
    private bool isOpen = false;
    private Font cachedFont;
    private const int ItemsPerPage = 48;
    private int currentPage;
    private int pageCount = 1;
    private bool listRefreshPending;

    private Font GetFont()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = UiTextStyle.BodyFont;
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
        if (StorageManager.Instance != null)
            StorageManager.Instance.OnStorageChanged += OnDataChanged;
        if (DismantleService.Instance != null)
            DismantleService.Instance.OnDismantleExecuted += OnDataChanged;
    }

    private void OnDestroy()
    {
        if (StorageManager.Instance != null)
            StorageManager.Instance.OnStorageChanged -= OnDataChanged;
        if (DismantleService.Instance != null)
            DismantleService.Instance.OnDismantleExecuted -= OnDataChanged;
    }

    private void WireEvents()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Close);
        }
        if (btnBackdrop != null)
        {
            btnBackdrop.onClick.RemoveAllListeners();
            btnBackdrop.onClick.AddListener(Close);
        }

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int catIndex = i;
                if (tabButtons[i] != null)
                {
                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => SelectCategory((StorageCategory)catIndex));
                }
            }
        }

        // Toggles
        if (toggleSwordHighLevel != null)
        {
            toggleSwordHighLevel.onValueChanged.RemoveAllListeners();
            toggleSwordHighLevel.onValueChanged.AddListener((val) =>
            {
                if (StorageManager.Instance != null) StorageManager.Instance.FilterSwordsHighLevel = val;
            });
        }
        if (togglePillBreakthrough != null)
        {
            togglePillBreakthrough.onValueChanged.RemoveAllListeners();
            togglePillBreakthrough.onValueChanged.AddListener((val) =>
            {
                if (StorageManager.Instance != null) StorageManager.Instance.FilterPillsBreakthrough = val;
            });
        }
        if (toggleAutoDismantleLv1 != null)
        {
            toggleAutoDismantleLv1.onValueChanged.RemoveAllListeners();
            toggleAutoDismantleLv1.onValueChanged.AddListener((val) =>
            {
                if (StorageManager.Instance != null) StorageManager.Instance.AutoDismantleLv1SwordsWhenFull = val;
            });
        }

        // Footer Buttons
        if (btnClaimPriority != null)
        {
            btnClaimPriority.onClick.RemoveAllListeners();
            btnClaimPriority.onClick.AddListener(OnClaimPriorityClicked);
        }
        if (btnBatchDismantle != null)
        {
            btnBatchDismantle.onClick.RemoveAllListeners();
            btnBatchDismantle.onClick.AddListener(OnBatchDismantleClicked);
        }

        // Toggle View Mode Button
        if (btnToggleView != null)
        {
            btnToggleView.onClick.RemoveAllListeners();
            btnToggleView.onClick.AddListener(ToggleViewMode);
        }

        // Tự động kết nối nút TÚI trên HUD chính nếu chưa gắn
        GameObject hudStorageBtn = GameObject.Find("Canvas/MobileUIRoot/BtnHUDStorage");
        if (hudStorageBtn != null)
        {
            var btn = hudStorageBtn.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Open());
            }
        }
    }

    public static void Open(StorageCategory initialCategory = StorageCategory.All)
    {
        if (Instance == null)
        {
            Debug.LogError("[StorageModalUI] Không tìm thấy Instance trong Scene!");
            return;
        }
        Instance.Show(initialCategory);
    }

    public void Show(StorageCategory category = StorageCategory.All)
    {
        selectedCategory = category;
        currentPage = 0;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        isOpen = true;

        SyncToggles();
        UpdateTabVisuals();
        UpdateViewModeVisuals();
        RefreshWalletStats();
        RebuildItemList();

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen(true));

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    public void ToggleViewMode()
    {
        currentViewMode = (currentViewMode == StorageViewMode.GridSlots) ? StorageViewMode.CompactList : StorageViewMode.GridSlots;
        UpdateViewModeVisuals();
        RefreshItemList();
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private void UpdateViewModeVisuals()
    {
        bool isGrid = (currentViewMode == StorageViewMode.GridSlots);
        if (gridContent != null) gridContent.gameObject.SetActive(isGrid);
        if (listContent != null) listContent.gameObject.SetActive(!isGrid);

        contentContainer = isGrid ? (gridContent != null ? gridContent : contentContainer) : (listContent != null ? listContent : contentContainer);
        if (scrollRect != null && contentContainer != null)
        {
            scrollRect.content = contentContainer;
        }

        if (txtToggleView != null)
        {
            txtToggleView.text = isGrid ? "Xem DS" : "Xem Ô";
        }
    }

    public void Close()
    {
        if (!isOpen && !gameObject.activeInHierarchy) return;
        isOpen = false;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen(false));

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private void OnDataChanged()
    {
        if (isOpen && gameObject.activeInHierarchy)
        {
            RefreshWalletStats();
            RefreshItemList();
        }
    }

    public void SelectCategory(StorageCategory cat)
    {
        selectedCategory = cat;
        currentPage = 0;
        UpdateTabVisuals();
        RefreshItemList();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private void SyncToggles()
    {
        if (StorageManager.Instance == null) return;
        if (toggleSwordHighLevel != null) toggleSwordHighLevel.SetIsOnWithoutNotify(StorageManager.Instance.FilterSwordsHighLevel);
        if (togglePillBreakthrough != null) togglePillBreakthrough.SetIsOnWithoutNotify(StorageManager.Instance.FilterPillsBreakthrough);
        if (toggleAutoDismantleLv1 != null) toggleAutoDismantleLv1.SetIsOnWithoutNotify(StorageManager.Instance.AutoDismantleLv1SwordsWhenFull);
    }

    private void UpdateTabVisuals()
    {
        if (tabButtons == null) return;
        Color activeCol = new Color(0.96f, 0.78f, 0.26f, 1f); // Vàng kim
        Color normalCol = new Color(0.12f, 0.16f, 0.22f, 0.95f); // Đêm tối

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] == null) continue;
            bool isActive = (i == (int)selectedCategory);

            Image img = tabButtons[i].GetComponent<Image>();
            if (img != null) img.color = isActive ? activeCol : normalCol;

            Text txt = tabButtons[i].GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.color = isActive ? (MobileUIAnimationDirector.For(this)!=null?JadeUITheme.Ivory:new Color(0.08f, 0.05f, 0.02f, 1f)) : new Color(0.85f, 0.88f, 0.92f, 1f);
                txt.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;

                // Cập nhật số lượng hòm tạm trên Tab 4
                if (i == 4 && StorageManager.Instance != null)
                {
                    int pCount = StorageManager.Instance.PendingCount;
                    txt.text = pCount > 0 ? $"Hòm Tạm ({pCount})" : "Hòm Tạm";
                }
            }
        }
    }

    private void RefreshWalletStats()
    {
        if (txtLinhThach != null && EconomyManager.Instance != null)
            txtLinhThach.text = $"LT {EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach)}";
        if (txtKiemY != null && EconomyManager.Instance != null)
            txtKiemY.text = $"K.Ý {EconomyManager.Instance.currentKiemY:N0}";
        if (txtOre != null && ArtifactService.Instance != null)
            txtOre.text = $"Quặng {ArtifactService.Instance.Ore:N0}";
        if (txtDust != null && AlchemyService.Instance != null)
            txtDust.text = $"Bụi {AlchemyService.Instance.State.medicineDust:N0}";
    }

    private void OnClaimPriorityClicked()
    {
        if (StorageManager.Instance != null)
        {
            int count = StorageManager.Instance.ClaimPriorityLoot();
            if (count > 0 && AudioManager.Instance != null)
                AudioManager.Instance.PlayClickSFX();
            RefreshItemList();
            RefreshWalletStats();
        }
    }

    private void OnBatchDismantleClicked()
    {
        if (DismantleService.Instance != null)
        {
            var reward = DismantleService.Instance.BatchDismantleLowTier();
            if (!reward.IsEmpty && AudioManager.Instance != null)
                AudioManager.Instance.PlayMergeSwordSFX();
            RefreshItemList();
            RefreshWalletStats();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TẠO DANH SÁCH VẬT PHẨM (DYNAMIC ITEM CARDS)
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshItemList()
    {
        // Storage and dismantle can both signal during the same transaction.
        // Rebuilding here used to allocate the whole queue after EACH removed item.
        listRefreshPending = true;
    }

    private void LateUpdate()
    {
        if (!listRefreshPending || !isOpen || !gameObject.activeInHierarchy) return;
        RebuildItemList();
        RefreshWalletStats();
        UpdateTabVisuals();
    }

    private bool IsOnCurrentPage(int itemIndex)
    {
        return itemIndex >= currentPage * ItemsPerPage && itemIndex < (currentPage + 1) * ItemsPerPage;
    }

    private void RebuildItemList()
    {
        listRefreshPending = false;
        foreach (var go in spawnedRows)
        {
            if (go == null) continue;
            // Destroy is deferred until the end of the frame. Keep old rows out
            // of layout/raycast rebuilds while their replacements are created.
            go.SetActive(false);
            Destroy(go);
        }
        spawnedRows.Clear();

        if (contentContainer == null) return;

        int totalItemsShown = 0;

        // 1. KIẾM PHÔI
        if (selectedCategory == StorageCategory.All || selectedCategory == StorageCategory.Swords)
        {
            if (MergeManager.Instance != null)
            {
                var swords = MergeManager.Instance.GetAllSwords(includeEquipped: true);
                foreach (var s in swords)
                {
                    if (s == null) continue;
                    bool isEquipped = MergeManager.Instance.IsSwordEquipped(s);
                    if (IsOnCurrentPage(totalItemsShown++)) CreateSwordCard(s, isEquipped);
                }
            }
        }

        // 2. ĐAN DƯỢC
        if (selectedCategory == StorageCategory.All || selectedCategory == StorageCategory.Pills)
        {
            if (AlchemyService.Instance != null)
            {
                for (int r = 0; r < 9; r++)
                {
                    for (int rec = 0; rec < 2; rec++)
                    {
                        for (int q = 0; q < 4; q++)
                        {
                            int count = AlchemyService.Instance.GetPillCount(r, (PillRecipe)rec, (PillQuality)q);
                            // Nếu là phế đan thì lấy trực tiếp từ State.pills
                            if (q == (int)PillQuality.Waste)
                            {
                                int wIdx = r * 8 + rec * 4 + (int)PillQuality.Waste;
                                count = (wIdx >= 0 && wIdx < AlchemyService.Instance.State.pills.Length) ? AlchemyService.Instance.State.pills[wIdx] : 0;
                            }

                            if (count > 0)
                            {
                                if (IsOnCurrentPage(totalItemsShown++)) CreatePillCard(r, (PillRecipe)rec, (PillQuality)q, count);
                            }
                        }
                    }
                }
            }
        }

        // 3. NGUYÊN LIỆU (Thảo Dược & Quặng Phôi)
        if (selectedCategory == StorageCategory.All || selectedCategory == StorageCategory.Materials)
        {
            if (AlchemyService.Instance != null)
            {
                string[] herbNames = { "Linh Chi Vạn Niên", "Cỏ Ngưng Khí", "Tuyết Liên Ngàn Năm" };
                for (int h = 0; h < 3; h++)
                {
                    int hCount = AlchemyService.Instance.GetHerbCount(h);
                    if (hCount > 0)
                    {
                        if (IsOnCurrentPage(totalItemsShown++)) CreateHerbCard(h, herbNames[h], hCount);
                    }
                }
                if (AlchemyService.Instance.State.medicineDust > 0)
                {
                    if (IsOnCurrentPage(totalItemsShown++)) CreateDustCard(AlchemyService.Instance.State.medicineDust);
                }
            }

            if (ArtifactService.Instance != null && ArtifactService.Instance.Ore > 0)
            {
                if (IsOnCurrentPage(totalItemsShown++)) CreateOreCard(ArtifactService.Instance.Ore);
            }
        }

        // 4. HÒM TẠM CHỜ LỌC
        if (selectedCategory == StorageCategory.All || selectedCategory == StorageCategory.Pending)
        {
            if (StorageManager.Instance != null)
            {
                var pendingList = StorageManager.Instance.GetPendingItems();
                foreach (var p in pendingList)
                {
                    if (p != null)
                    {
                        if (IsOnCurrentPage(totalItemsShown++)) CreatePendingCard(p);
                    }
                }
            }
        }

        pageCount = Mathf.Max(1, Mathf.CeilToInt(totalItemsShown / (float)ItemsPerPage));
        if (currentPage >= pageCount)
        {
            currentPage = pageCount - 1;
            RebuildItemList();
            return;
        }
        if (pageCount > 1)
        {
            Transform navigation = null;
            if (currentViewMode == StorageViewMode.CompactList)
            {
                var row = new GameObject("StoragePageNavigation", typeof(RectTransform), typeof(UnityEngine.UI.LayoutElement), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                row.transform.SetParent(contentContainer, false);
                spawnedRows.Add(row);
                row.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 132f;
                var layout = row.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.spacing = 8f;
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                navigation = row.transform;
            }
            CreatePageControl("StoragePreviousPage", "‹ Trước", currentPage > 0 ? (Action)(() => SelectPage(currentPage - 1)) : null, navigation);
            CreatePageControl("StoragePageNumber", $"Trang {currentPage + 1}/{pageCount}", null, navigation);
            CreatePageControl("StorageNextPage", "Tiếp ›", currentPage < pageCount - 1 ? (Action)(() => SelectPage(currentPage + 1)) : null, navigation);
        }

        if (txtEmptyNotice != null)
        {
            txtEmptyNotice.gameObject.SetActive(totalItemsShown == 0);
            if (totalItemsShown == 0)
            {
                txtEmptyNotice.text = selectedCategory == StorageCategory.Pending 
                    ? "Hòm tạm hiện đang trống!\nCác vật phẩm khi kho đầy sẽ được lưu giữ tại đây."
                    : "Không có vật phẩm nào trong danh mục này.";
            }
        }
    }

    public void SelectPage(int page)
    {
        currentPage = Mathf.Clamp(page, 0, pageCount - 1);
        RefreshItemList();
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void CreatePageControl(string objectName, string label, Action onClick, Transform navigation = null)
    {
        var control = new GameObject(objectName, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.LayoutElement));
        control.transform.SetParent(navigation != null ? navigation : contentContainer, false);
        if (navigation == null) spawnedRows.Add(control);
        var layout = control.GetComponent<UnityEngine.UI.LayoutElement>();
        layout.minHeight = layout.preferredHeight = 132f;
        layout.minWidth = layout.preferredWidth = 132f;
        var background = control.GetComponent<UnityEngine.UI.Image>();
        background.color = new Color(0.12f, 0.20f, 0.23f, 1f);
        background.raycastTarget = onClick != null;
        if (onClick != null)
        {
            var button = control.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => onClick());
        }
        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(UnityEngine.UI.Text));
        labelObject.transform.SetParent(control.transform, false);
        var rect = (RectTransform)labelObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6, 6);
        rect.offsetMax = new Vector2(-6, -6);
        var text = labelObject.GetComponent<UnityEngine.UI.Text>();
        text.font = GetFont();
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = onClick != null ? JadeUITheme.Gold : JadeUITheme.Ivory;
        text.raycastTarget = false;
        text.text = label;
    }

    private void CreateSwordCard(MergeItem sword, bool isEquipped)
    {
        int level = sword.level;
        int pieces = sword.currentPieces;
        string swordName = DismantleService.GetSwordDisplayName(level);
        var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateSwordReward(level, pieces) : null;
        bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValueSword(level, pieces, isEquipped);

        string badge = isEquipped ? "<color=#FFD700>[ĐANG TRANG BỊ]</color> " : "";
        string title = $"{badge}<color=#FFEC8B>{swordName}</color>";
        string detail = $"Cấp {level} | Tích luỹ: {pieces} phôi";
        string rewardStr = reward != null ? $"Tách: {reward.GetFormattedSummary()}" : "";

        Action doDismantle = () =>
        {
            if (isHighValue || currentViewMode == StorageViewMode.GridSlots)
            {
                DismantleConfirmModalUI.OpenConfirm(
                    $"{swordName} (Lv.{level})",
                    $"{detail} {(isEquipped ? "\nKiếm này đang xuất trận trên chiến trường!" : "")}",
                    reward,
                    () =>
                    {
                        DismantleService.Instance?.DismantleSword(sword);
                        RefreshItemList();
                        RefreshWalletStats();
                    });
            }
            else
            {
                DismantleService.Instance?.DismantleSword(sword);
                RefreshItemList();
                RefreshWalletStats();
            }
        };

        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color rarityColor = GetRarityColorForLevel(level, isEquipped);
            string countBadge = pieces > 1 ? $"+{pieces}" : $"Lv.{level}";
            var presentation = GetComponentInParent<MobileUIReconstruction>();
            CreateGridSlot("Kiếm", $"Lv.{level}", isEquipped ? "TRANG BỊ" : "", countBadge, rarityColor, isEquipped, isHighValue, doDismantle,
                presentation != null && presentation.assets != null ? presentation.assets.SwordRarity(level) : null);
        }
        else
        {
            CreateGenericCard("Kiếm", title, detail, rewardStr, isHighValue, doDismantle);
        }
    }

    private void CreatePillCard(int realm, PillRecipe recipe, PillQuality quality, int count)
    {
        string realmStr = GetRealmName(realm);
        string recipeStr = recipe == PillRecipe.Breakthrough ? "Đột Phá" : "Tu Vi";
        string qualityColor = GetQualityColor(quality);
        string pillTitle = $"<color={qualityColor}>[{quality}] {realmStr} {recipeStr} Đan</color>";
        string detail = $"Số lượng: x{count} viên";

        var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculatePillReward(realm, recipe, quality, 1) : null;
        string rewardStr = reward != null ? $"Tách 1: {reward.GetFormattedSummary()}" : "";
        bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValuePill(realm, recipe, quality);

        Action doDismantle = () =>
        {
            if (isHighValue || currentViewMode == StorageViewMode.GridSlots)
            {
                DismantleConfirmModalUI.OpenConfirm(
                    $"[{quality}] {realmStr} {recipeStr} Đan",
                    $"Phân giải 1 viên từ x{count} viên đang có trong kho.",
                    reward,
                    () =>
                    {
                        DismantleService.Instance?.DismantlePills(realm, recipe, quality, 1);
                        RefreshItemList();
                        RefreshWalletStats();
                    });
            }
            else
            {
                DismantleService.Instance?.DismantlePills(realm, recipe, quality, 1);
                RefreshItemList();
                RefreshWalletStats();
            }
        };

        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color rarityCol = GetQualityColorValue(quality);
            var presentation = GetComponentInParent<MobileUIReconstruction>();
            CreateGridSlot("Đan", $"{recipeStr}", "", $"x{count}", rarityCol, false, isHighValue, doDismantle,
                presentation != null && presentation.assets != null ? presentation.assets.PillRarity(quality) : null);
        }
        else
        {
            CreateGenericCard("Đan", pillTitle, detail, rewardStr, isHighValue, doDismantle);
        }
    }

    private void CreateHerbCard(int herbIndex, string name, int count)
    {
        string title = $"<color=#7CFC00>{name}</color>";
        string detail = $"Số lượng: x{count} cây";
        var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateHerbReward(herbIndex, 1) : null;
        string rewardStr = reward != null ? $"Tách 1: {reward.GetFormattedSummary()}" : "";

        Action doDismantle = () =>
        {
            if (currentViewMode == StorageViewMode.GridSlots)
            {
                DismantleConfirmModalUI.OpenConfirm(
                    $"{name}",
                    $"Phân giải 1 cây từ x{count} cây trong kho.",
                    reward,
                    () =>
                    {
                        DismantleService.Instance?.DismantleHerbs(herbIndex, 1);
                        RefreshItemList();
                        RefreshWalletStats();
                    });
            }
            else
            {
                DismantleService.Instance?.DismantleHerbs(herbIndex, 1);
                RefreshItemList();
                RefreshWalletStats();
            }
        };

        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color herbCol = new Color(0.48f, 0.98f, 0.48f, 0.85f);
            CreateGridSlot("Dược", name.Length > 7 ? name.Substring(0, 6) + ".." : name, "", $"x{count}", herbCol, false, false, doDismantle);
        }
        else
        {
            CreateGenericCard("Dược", title, detail, rewardStr, false, doDismantle);
        }
    }

    private void CreateDustCard(int dust)
    {
        string title = "<color=#98FB98>Dược Trần (Bột Thuốc)</color>";
        string detail = $"Số lượng: {dust} hạt (Tăng tốc hoàn tất lò luyện đan)";
        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color dustCol = new Color(0.60f, 0.98f, 0.60f, 0.85f);
            CreateGridSlot("Bụi", "Dược Trần", "", $"x{dust}", dustCol, false, false, null);
        }
        else
        {
            CreateGenericCard("Bụi", title, detail, "Dùng trong Lò Luyện Đan", false, null);
        }
    }

    private void CreateOreCard(int ore)
    {
        string title = "<color=#FFB90F>Quặng Phôi Pháp Khí</color>";
        string detail = $"Số lượng: {ore} khối";
        var reward = DismantleService.Instance != null ? DismantleService.Instance.CalculateOreReward(1) : null;
        string rewardStr = reward != null ? $"Tách 1: {reward.GetFormattedSummary()}" : "";

        Action doDismantle = () =>
        {
            if (currentViewMode == StorageViewMode.GridSlots)
            {
                DismantleConfirmModalUI.OpenConfirm(
                    "Quặng Phôi Pháp Khí",
                    $"Phân giải 1 khối từ x{ore} khối trong kho.",
                    reward,
                    () =>
                    {
                        DismantleService.Instance?.DismantleOre(1);
                        RefreshItemList();
                        RefreshWalletStats();
                    });
            }
            else
            {
                DismantleService.Instance?.DismantleOre(1);
                RefreshItemList();
                RefreshWalletStats();
            }
        };

        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color oreCol = new Color(1f, 0.73f, 0.15f, 0.85f);
            CreateGridSlot("Quặng", "Quặng", "", $"x{ore}", oreCol, false, false, doDismantle);
        }
        else
        {
            CreateGenericCard("Quặng", title, detail, rewardStr, false, doDismantle);
        }
    }

    private void CreatePendingCard(PendingLootItem item)
    {
        string title = $"<color=#00FFFF>[Hòm Tạm] {item.displayName}</color>";
        string detail = item.itemType == PendingLootType.Sword 
            ? $"Kiếm Cấp {item.level} ({item.pieces} phôi)"
            : $"Số lượng: x{item.amount}";

        DismantleReward reward = null;
        if (DismantleService.Instance != null)
        {
            switch (item.itemType)
            {
                case PendingLootType.Sword:
                    reward = DismantleService.Instance.CalculateSwordReward(item.level, item.pieces);
                    break;
                case PendingLootType.Pill:
                    reward = DismantleService.Instance.CalculatePillReward(item.realm, (PillRecipe)item.recipe, (PillQuality)item.quality, item.amount);
                    break;
                case PendingLootType.Herb:
                    reward = DismantleService.Instance.CalculateHerbReward(item.level, item.amount);
                    break;
                case PendingLootType.Ore:
                    reward = DismantleService.Instance.CalculateOreReward(item.amount);
                    break;
            }
        }

        string rewardStr = reward != null ? $"Tách: {reward.GetFormattedSummary()}" : "";
        bool isHighValue = DismantleService.Instance != null && DismantleService.Instance.IsHighValuePendingItem(item);

        Action doDismantle = () =>
        {
            if (isHighValue || currentViewMode == StorageViewMode.GridSlots)
            {
                DismantleConfirmModalUI.OpenConfirm(
                    item.displayName,
                    detail,
                    reward,
                    () =>
                    {
                        DismantleService.Instance?.DismantlePendingItem(item);
                        RefreshItemList();
                        RefreshWalletStats();
                    });
            }
            else
            {
                DismantleService.Instance?.DismantlePendingItem(item);
                RefreshItemList();
                RefreshWalletStats();
            }
        };

        if (currentViewMode == StorageViewMode.GridSlots)
        {
            Color pendingCol = isHighValue ? new Color(1f, 0.8f, 0.2f, 0.9f) : new Color(0f, 0.85f, 0.95f, 0.85f);
            string countStr = item.itemType == PendingLootType.Sword ? $"Lv.{item.level}" : $"x{item.amount}";
            CreateGridSlot("Hòm", item.displayName.Length > 7 ? item.displayName.Substring(0, 6) + ".." : item.displayName, "", countStr, pendingCol, false, isHighValue, doDismantle);
        }
        else
        {
            CreateGenericCard("Hòm", title, detail, rewardStr, isHighValue, doDismantle);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TẠO THẺ VẬT PHẨM TRỰC QUAN (CARD & SLOT BUILDER)
    // ─────────────────────────────────────────────────────────────────────────

    private void CreateGridSlot(string icon, string label, string ribbonTag, string countBadge, Color rarityColor, bool isEquipped, bool isHighValue, Action onClick, Sprite rarityFrame = null)
    {
        Font font = GetFont();

        GameObject slotObj = new GameObject("GridSlot", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        slotObj.transform.SetParent(contentContainer, false);
        spawnedRows.Add(slotObj);

        RectTransform rt = slotObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(92f, 92f);

        LayoutElement le = slotObj.GetComponent<LayoutElement>();
        le.preferredWidth = 92f;
        le.preferredHeight = 92f;
        le.minWidth = 92f;
        le.minHeight = 92f;

        Image img = slotObj.GetComponent<Image>();
        img.color = new Color(0.08f, 0.11f, 0.16f, 0.96f);

        if (rarityFrame != null)
        {
            var frame = new GameObject("P6Rarity", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(slotObj.transform, false);
            var frameRect = (RectTransform)frame.transform;
            frameRect.anchorMin = Vector2.zero; frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero; frameRect.offsetMax = Vector2.zero;
            var frameImage = frame.GetComponent<Image>();
            frameImage.sprite = rarityFrame; frameImage.type = Image.Type.Sliced; frameImage.raycastTarget = false;
            MobileUISlicedSurface.Configure(frameImage);
            if(isHighValue)MobileUIAnimationDirector.For(this)?.Accent(frameImage);
        }

        Outline ol = slotObj.GetComponent<Outline>();
        ol.effectColor = isHighValue ? new Color(1f, 0.82f, 0.25f, 0.95f) : rarityColor;
        ol.effectDistance = new Vector2(1.5f, -1.5f);

        if (onClick != null)
        {
            Button btn = slotObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsDismantleUnlocked())
                {
                    if (GameLogger.Instance != null)
                    {
                        GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow);
                    }
                    return;
                }
                onClick.Invoke();
            });
        }

        // 1. Top Ribbon if equipped
        if (isEquipped)
        {
            GameObject ribObj = new GameObject("Ribbon", typeof(RectTransform), typeof(Image));
            ribObj.transform.SetParent(slotObj.transform, false);
            RectTransform ribRt = ribObj.GetComponent<RectTransform>();
            ribRt.anchorMin = new Vector2(0f, 1f);
            ribRt.anchorMax = new Vector2(1f, 1f);
            ribRt.pivot = new Vector2(0.5f, 1f);
            ribRt.anchoredPosition = Vector2.zero;
            ribRt.sizeDelta = new Vector2(0f, 16f);

            Image ribImg = ribObj.GetComponent<Image>();
            ribImg.color = new Color(0.85f, 0.35f, 0.10f, 0.95f);
            ribImg.raycastTarget = false;

            GameObject rTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            rTextObj.transform.SetParent(ribObj.transform, false);
            RectTransform rtRt = rTextObj.GetComponent<RectTransform>();
            rtRt.anchorMin = Vector2.zero;
            rtRt.anchorMax = Vector2.one;
            rtRt.sizeDelta = Vector2.zero;

            Text rtTxt = rTextObj.GetComponent<Text>();
            if (font != null) rtTxt.font = font;
            rtTxt.fontSize = 8;
            rtTxt.fontStyle = FontStyle.Bold;
            rtTxt.alignment = TextAnchor.MiddleCenter;
            rtTxt.color = Color.white;
            rtTxt.text = "XUẤT TRẬN";
        }

        // 2. High value badge
        if (isHighValue)
        {
            GameObject starObj = new GameObject("Star", typeof(RectTransform), typeof(Text));
            starObj.transform.SetParent(slotObj.transform, false);
            RectTransform stRt = starObj.GetComponent<RectTransform>();
            stRt.anchorMin = stRt.anchorMax = new Vector2(0f, 1f);
            stRt.pivot = new Vector2(0f, 1f);
            stRt.anchoredPosition = new Vector2(4f, -4f);
            stRt.sizeDelta = new Vector2(25f, 16f);

            Text stTxt = starObj.GetComponent<Text>();
            if (font != null) stTxt.font = font;
            stTxt.fontSize = 11;
            stTxt.alignment = TextAnchor.MiddleCenter;
            stTxt.color = new Color(1f, 0.84f, 0f, 1f);
            stTxt.text = "Quý";
        }

        // 3. Center Icon & Label
        GameObject centerObj = new GameObject("Center", typeof(RectTransform), typeof(Text));
        centerObj.transform.SetParent(slotObj.transform, false);
        RectTransform cRt = centerObj.GetComponent<RectTransform>();
        cRt.anchorMin = Vector2.zero;
        cRt.anchorMax = Vector2.one;
        cRt.offsetMin = new Vector2(4f, isEquipped ? 4f : 8f);
        cRt.offsetMax = new Vector2(-4f, isEquipped ? -16f : -8f);

        Text cTxt = centerObj.GetComponent<Text>();
        if (font != null) cTxt.font = font;
        cTxt.fontSize = 11;
        cTxt.alignment = TextAnchor.MiddleCenter;
        cTxt.lineSpacing = 1.05f;
        cTxt.supportRichText = true;
        cTxt.color = Color.white;
        cTxt.text = $"<size=12>{icon}</size>\n{label}";

        // 4. Bottom Right Badge
        if (!string.IsNullOrEmpty(countBadge))
        {
            GameObject cntObj = new GameObject("CountBadge", typeof(RectTransform), typeof(Image));
            cntObj.transform.SetParent(slotObj.transform, false);
            RectTransform cntRt = cntObj.GetComponent<RectTransform>();
            cntRt.anchorMin = cntRt.anchorMax = new Vector2(1f, 0f);
            cntRt.pivot = new Vector2(1f, 0f);
            cntRt.anchoredPosition = new Vector2(-3f, 3f);
            cntRt.sizeDelta = new Vector2(44f, 16f);

            Image cntImg = cntObj.GetComponent<Image>();
            cntImg.color = new Color(0.04f, 0.06f, 0.08f, 0.88f);
            cntImg.raycastTarget = false;

            GameObject cntTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            cntTxtObj.transform.SetParent(cntObj.transform, false);
            RectTransform ctRt = cntTxtObj.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.sizeDelta = Vector2.zero;

            Text ctTxt = cntTxtObj.GetComponent<Text>();
            if (font != null) ctTxt.font = font;
            ctTxt.fontSize = 9;
            ctTxt.fontStyle = FontStyle.Bold;
            ctTxt.alignment = TextAnchor.MiddleCenter;
            ctTxt.color = new Color(0.96f, 0.85f, 0.40f, 1f);
            ctTxt.text = countBadge;
        }
    }

    private void CreateGenericCard(string icon, string title, string detail, string rewardSummary, bool isHighValue, Action onDismantle)
    {
        Font font = GetFont();

        GameObject cardObj = new GameObject("StorageCard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        cardObj.transform.SetParent(contentContainer, false);
        spawnedRows.Add(cardObj);

        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(610f, 48f);

        LayoutElement le = cardObj.GetComponent<LayoutElement>();
        le.preferredHeight = 48f;
        le.minHeight = 48f;

        Image img = cardObj.GetComponent<Image>();
        img.color = new Color(0.08f, 0.11f, 0.16f, 0.96f);

        Outline ol = cardObj.GetComponent<Outline>();
        ol.effectColor = isHighValue ? new Color(0.96f, 0.65f, 0.20f, 0.85f) : new Color(0.20f, 0.28f, 0.38f, 0.65f);
        ol.effectDistance = new Vector2(1f, -1f);

        // 1. Left Icon badge (34x34)
        GameObject iconBadge = new GameObject("IconBadge", typeof(RectTransform), typeof(Image));
        iconBadge.transform.SetParent(cardObj.transform, false);
        RectTransform ibRt = iconBadge.GetComponent<RectTransform>();
        ibRt.anchorMin = ibRt.anchorMax = new Vector2(0f, 0.5f);
        ibRt.pivot = new Vector2(0f, 0.5f);
        ibRt.anchoredPosition = new Vector2(8f, 0f);
        ibRt.sizeDelta = new Vector2(34f, 34f);

        Image ibImg = iconBadge.GetComponent<Image>();
        ibImg.color = new Color(0.04f, 0.06f, 0.09f, 0.90f);
        ibImg.raycastTarget = false;

        GameObject ibTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        ibTxtObj.transform.SetParent(iconBadge.transform, false);
        RectTransform ibtRt = ibTxtObj.GetComponent<RectTransform>();
        ibtRt.anchorMin = Vector2.zero;
        ibtRt.anchorMax = Vector2.one;
        ibtRt.sizeDelta = Vector2.zero;

        Text ibTxt = ibTxtObj.GetComponent<Text>();
        if (font != null) ibTxt.font = font;
        ibTxt.fontSize = 12;
        ibTxt.alignment = TextAnchor.MiddleCenter;
        ibTxt.text = icon;

        // 2. Middle Text Info (2 lines)
        GameObject textObj = new GameObject("Info", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(cardObj.transform, false);
        RectTransform tRt = textObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.offsetMin = new Vector2(50f, 2f);
        tRt.offsetMax = new Vector2(-88f, -2f);

        Text txt = textObj.GetComponent<Text>();
        if (font != null) txt.font = font;
        txt.fontSize = 11;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.lineSpacing = 1.05f;
        txt.supportRichText = true;
        txt.text = $"{title}\n<color=#A0AEC0><size=10>{detail}  |  {rewardSummary}</size></color>";

        // 3. Right Dismantle Button (74x30)
        if (onDismantle != null)
        {
            bool isDismantleUnlocked = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsDismantleUnlocked();

            GameObject btnObj = new GameObject("BtnDismantle", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
            btnObj.transform.SetParent(cardObj.transform, false);

            RectTransform bRt = btnObj.GetComponent<RectTransform>();
            bRt.anchorMin = bRt.anchorMax = new Vector2(1f, 0.5f);
            bRt.pivot = new Vector2(1f, 0.5f);
            bRt.anchoredPosition = new Vector2(-8f, 0f);
            bRt.sizeDelta = new Vector2(74f, 30f);

            Image bImg = btnObj.GetComponent<Image>();
            if (!isDismantleUnlocked)
            {
                bImg.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
            }
            else
            {
                bImg.color = isHighValue ? new Color(0.75f, 0.20f, 0.18f, 0.95f) : new Color(0.18f, 0.32f, 0.48f, 0.95f);
            }

            Outline bOl = btnObj.GetComponent<Outline>();
            bOl.effectColor = !isDismantleUnlocked ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : (isHighValue ? new Color(1f, 0.8f, 0.3f, 0.8f) : new Color(0.4f, 0.6f, 0.8f, 0.6f));
            bOl.effectDistance = new Vector2(1f, -1f);

            Button btn = btnObj.GetComponent<Button>();
            if (!isDismantleUnlocked)
            {
                btn.onClick.AddListener(() =>
                {
                    if (GameLogger.Instance != null && FeatureUnlockService.Instance != null)
                    {
                        GameLogger.Instance.Log(FeatureUnlockService.Instance.GetFeatureUnlockRequirementText(FeatureId.Dismantle), Color.yellow);
                    }
                });
            }
            else
            {
                btn.onClick.AddListener(() => onDismantle.Invoke());
            }

            GameObject bTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            bTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRt = bTextObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;

            Text bTxt = bTextObj.GetComponent<Text>();
            if (font != null) bTxt.font = font;
            bTxt.fontSize = 11;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = !isDismantleUnlocked ? new Color(0.6f, 0.6f, 0.65f) : Color.white;
            bTxt.text = isDismantleUnlocked ? "Tách" : "Khóa";
        }
    }

    private Color GetRarityColorForLevel(int level, bool isEquipped)
    {
        if (isEquipped) return new Color(1f, 0.82f, 0.25f, 0.95f);
        if (level >= 8) return new Color(1f, 0.45f, 0.10f, 0.95f); // Cam
        if (level >= 5) return new Color(0.85f, 0.35f, 1f, 0.95f); // Tím
        if (level >= 3) return new Color(0.20f, 0.65f, 1f, 0.95f); // Lam
        if (level >= 2) return new Color(0.35f, 0.90f, 0.45f, 0.95f); // Lục
        return new Color(0.45f, 0.52f, 0.62f, 0.75f); // Trắng
    }

    private Color GetQualityColorValue(PillQuality quality)
    {
        switch (quality)
        {
            case PillQuality.Supreme: return new Color(1f, 0.84f, 0f, 0.95f);
            case PillQuality.Refined: return new Color(0f, 0.75f, 1f, 0.95f);
            case PillQuality.Basic: return new Color(0.48f, 0.98f, 0f, 0.95f);
            default: return new Color(0.62f, 0.68f, 0.75f, 0.75f);
        }
    }

    private string GetRealmName(int realm)
    {
        string[] names = { "Luyện Khí", "Trúc Cơ", "Kim Đan", "Nguyên Anh", "Hóa Thần", "Luyện Hư", "Hợp Thể", "Đại Thừa", "Độ Kiếp" };
        return (realm >= 0 && realm < names.Length) ? names[realm] : $"Cảnh Giới {realm}";
    }

    private string GetQualityColor(PillQuality quality)
    {
        switch (quality)
        {
            case PillQuality.Waste: return "#A0AEC0";
            case PillQuality.Basic: return "#7CFC00";
            case PillQuality.Refined: return "#00BFFF";
            case PillQuality.Supreme: return "#FFD700";
            default: return "#FFFFFF";
        }
    }

    private IEnumerator AnimateOpen(bool open)
    {
        if(MobileUIAnimationDirector.For(this)!=null)
        {
            yield return MobileUIModalMotion.Animate(this,canvasGroup,modalBox,open);
            animRoutine=null;yield break;
        }
        float elapsed = 0f;
        float duration = 0.22f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : (open ? 0f : 1f);
        float endAlpha = open ? 1f : 0f;

        Vector3 startScale = open ? new Vector3(0.90f, 0.90f, 1f) : Vector3.one;
        Vector3 endScale = open ? Vector3.one : new Vector3(0.90f, 0.90f, 1f);

        if (modalBox != null) modalBox.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smooth);
            if (modalBox != null) modalBox.localScale = Vector3.Lerp(startScale, endScale, smooth);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = endAlpha;
        if (modalBox != null) modalBox.localScale = endScale;

        if (!open)
        {
            gameObject.SetActive(false);
        }
    }
}


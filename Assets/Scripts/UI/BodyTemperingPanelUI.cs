// File: Assets/Scripts/UI/BodyTemperingPanelUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Cây Thiên Phú / Cây Kỹ Năng Luyện Thể (Body Tempering Skill Tree)
/// Thiết kế chuẩn Mobile RPG (Task Bar Hero style):
/// 1. Cây kỹ năng cuộn dọc mượt mà (ScrollView) trong Drawer, không bị nhồi nhét.
/// 2. Bấm vào bất kỳ node nào sẽ mở Bảng Thấu Thị Chi Tiết ở GIỮA MÀN HÌNH (Center Modal Popup).
/// 3. Thao tác cường hóa, nâng x5, nâng tối đa tập trung trong Center Modal với đầy đủ Lore & Cơ chế chiến đấu.
/// </summary>
public sealed class BodyTemperingPanelUI : MonoBehaviour
{
    public static BodyTemperingPanelUI Instance { get; private set; }

    [Header("Header & Tài Nguyên")]
    public Text txtLinhThachBalance;
    public Text txtTotalBodyLevel;

    [Header("Scrollable Tree View Trong Drawer")]
    public ScrollRect treeScrollRect;
    public RectTransform treeContent;

    [Header("Danh Sách 6 Thẻ Kỹ Năng Trong Tree")]
    public RectTransform[] nodeCardTransforms = new RectTransform[6];
    public Image[] nodeBackgrounds = new Image[6];
    public Image[] nodeOutlines = new Image[6];
    public Text[] nodeNameTexts = new Text[6];
    public Text[] nodeLevelTexts = new Text[6];
    public Text[] nodeEffectTexts = new Text[6];
    public Text[] nodeCostTexts = new Text[6];
    public Button[] nodeSelectButtons = new Button[6];
    public Button[] nodeQuickUpgradeButtons = new Button[6];
    public GameObject[] nodeLockOverlays = new GameObject[6];
    public Text[] nodeLockRequirementTexts = new Text[6];

    [Header("Đường Liên Kết Cây (Connector Lines)")]
    // Line 0: 0->2 (Khí Huyết -> Dũ Hợp)
    // Line 1: 2->1 (Dũ Hợp -> Cương Khí)
    // Line 2: 3->4 (Kình Lực -> Phệ Huyết)
    // Line 3: 4->5 (Phệ Huyết -> Định Tâm)
    public Image[] connectorLines = new Image[4];

    [Header("Bảng Thấu Thị Chi Tiết Ở Giữa Màn Hình (Center Screen Modal)")]
    public GameObject centerModalRoot;
    public RectTransform centerModalBox;
    public Button centerModalCloseBtn;
    public Button centerModalBackdropBtn;
    public Text txtModalTitle;
    public Text txtModalTier;
    public Text txtModalLore;
    public Text txtModalMechanics;
    public Text txtModalCurrent;
    public Text txtModalNext;
    public Text txtModalRequirement;
    public GameObject goModalLockBanner;
    public Button btnModalUpgrade;
    public Button btnModalUpgrade5;
    public Button btnModalUpgradeMax;
    public Text txtModalCost;

    [Header("Tương thích ngược (Legacy References)")]
    public Text[] statLabels = new Text[6];
    public Button[] upgradeButtons = new Button[6];

    private int selectedIndex = 0; // Node đang được chọn trong Center Modal

    // Bảng màu phong cách Tu Tiên huyền huyễn
    private static readonly Color ColorSelectedOutline = new Color(0.98f, 0.82f, 0.28f, 1f); // Vàng kim rực rỡ
    private static readonly Color ColorUnlockedOutline = new Color(0.18f, 0.65f, 0.85f, 0.65f); // Lam thanh khiết
    private static readonly Color ColorLockedOutline   = new Color(0.25f, 0.30f, 0.38f, 0.40f); // Xám đen ẩn

    private static readonly Color ColorNodeCardActive  = new Color(0.09f, 0.14f, 0.22f, 0.95f);
    private static readonly Color ColorNodeCardLocked  = new Color(0.06f, 0.08f, 0.12f, 0.90f);

    private static readonly Color ColorLineActive      = new Color(0.20f, 0.85f, 0.82f, 0.90f); // Ngọc bích sáng
    private static readonly Color ColorLineLocked      = new Color(0.22f, 0.27f, 0.35f, 0.35f); // Tối màu

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < 6; i++)
        {
            int index = i;
            if (nodeSelectButtons != null && i < nodeSelectButtons.Length && nodeSelectButtons[i] != null)
            {
                nodeSelectButtons[i].onClick.RemoveAllListeners();
                nodeSelectButtons[i].onClick.AddListener(() => OpenCenterModal(index));
            }

            if (nodeQuickUpgradeButtons != null && i < nodeQuickUpgradeButtons.Length && nodeQuickUpgradeButtons[i] != null)
            {
                nodeQuickUpgradeButtons[i].onClick.RemoveAllListeners();
                nodeQuickUpgradeButtons[i].onClick.AddListener(() => OpenCenterModal(index));
            }
        }

        // Cài đặt Center Modal
        if (centerModalCloseBtn != null)
        {
            centerModalCloseBtn.onClick.RemoveAllListeners();
            centerModalCloseBtn.onClick.AddListener(CloseCenterModal);
        }

        if (centerModalBackdropBtn != null)
        {
            centerModalBackdropBtn.onClick.RemoveAllListeners();
            centerModalBackdropBtn.onClick.AddListener(CloseCenterModal);
        }

        if (btnModalUpgrade != null)
        {
            btnModalUpgrade.onClick.RemoveAllListeners();
            btnModalUpgrade.onClick.AddListener(() => UpgradeNode(selectedIndex, 1));
        }

        if (btnModalUpgrade5 != null)
        {
            btnModalUpgrade5.onClick.RemoveAllListeners();
            btnModalUpgrade5.onClick.AddListener(() => UpgradeNode(selectedIndex, 5));
        }

        if (btnModalUpgradeMax != null)
        {
            btnModalUpgradeMax.onClick.RemoveAllListeners();
            btnModalUpgradeMax.onClick.AddListener(() => UpgradeMax(selectedIndex));
        }

        if (centerModalRoot != null)
        {
            centerModalRoot.SetActive(false);
        }

        // Đồng bộ legacy arrays
        if (upgradeButtons == null || upgradeButtons.Length < 6) upgradeButtons = nodeQuickUpgradeButtons;
        if (statLabels == null || statLabels.Length < 6) statLabels = nodeEffectTexts;
    }

    private void OnEnable()
    {
        if (PlayerStatsService.Instance != null)
            PlayerStatsService.Instance.OnStatsChanged += Refresh;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged += HandleCurrencyChanged;
        
        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerStatsService.Instance != null)
            PlayerStatsService.Instance.OnStatsChanged -= Refresh;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged -= HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged(float amount) => Refresh();

    public void OpenCenterModal(int index)
    {
        if (index < 0 || index >= 6) return;
        selectedIndex = index;

        if (centerModalRoot != null)
        {
            centerModalRoot.SetActive(true);
            centerModalRoot.transform.SetAsLastSibling();
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        Refresh();
    }

    public void CloseCenterModal()
    {
        if (centerModalRoot != null)
        {
            centerModalRoot.SetActive(false);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    public void UpgradeNode(int index, int count)
    {
        var stats = PlayerStatsService.Instance;
        if (stats == null) return;

        if (count == 1)
        {
            stats.TryUpgrade(index);
        }
        else
        {
            stats.TryUpgradeMultiple(index, count);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        Refresh();
    }

    public void UpgradeMax(int index)
    {
        var stats = PlayerStatsService.Instance;
        if (stats == null) return;

        stats.TryUpgradeMultiple(index, 50); // Nâng tối đa trong 50 cấp
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        Refresh();
    }

    public void Refresh()
    {
        var stats = PlayerStatsService.Instance;
        var economy = EconomyManager.Instance;
        if (stats == null) return;

        // 1. Header Resource
        if (txtLinhThachBalance != null && economy != null)
        {
            txtLinhThachBalance.text = $"Linh Thạch: {EconomyManager.FormatNumber(economy.currentLinhThach)} 💎";
        }
        if (txtTotalBodyLevel != null)
        {
            txtTotalBodyLevel.text = $"Tổng Cấp Thể Phách: {stats.GetTotalBodyLevel()}";
        }

        // 2. Refresh 6 Node Cards trong ScrollView
        for (int i = 0; i < 6; i++)
        {
            if (i >= PlayerStatsService.TalentNodes.Length) break;
            var nodeDef = PlayerStatsService.TalentNodes[i];
            bool isUnlocked = stats.IsNodeUnlocked(i);
            int level = stats.GetLevel(i);
            float cost = stats.GetUpgradeCost(i);
            bool canAfford = isUnlocked && economy != null && economy.currentLinhThach >= cost;

            // Tên và Biểu tượng
            if (i < nodeNameTexts.Length && nodeNameTexts[i] != null)
            {
                nodeNameTexts[i].text = $"{nodeDef.iconEmoji} {nodeDef.name}";
            }

            // Cấp độ
            if (i < nodeLevelTexts.Length && nodeLevelTexts[i] != null)
            {
                nodeLevelTexts[i].text = isUnlocked ? $"Cấp {level}" : "Chưa Mở";
                nodeLevelTexts[i].color = isUnlocked ? Color.white : new Color(0.7f, 0.4f, 0.4f, 1f);
            }

            // Hiệu quả ngắn gọn
            if (i < nodeEffectTexts.Length && nodeEffectTexts[i] != null)
            {
                if (isUnlocked)
                {
                    nodeEffectTexts[i].text = level > 0 
                        ? $"{nodeDef.statPerLevelText}\n<color=#00e5ff>Hiện tại: {stats.GetCurrentEffectString(i)}</color>"
                        : $"{nodeDef.statPerLevelText}\n<color=#aaaaaa>Chưa kích hoạt</color>";
                }
                else
                {
                    nodeEffectTexts[i].text = $"<color=#ff7675>🔒 {stats.GetUnlockRequirementText(i)}</color>";
                }
            }

            // Chi phí nút nâng nhanh / mở modal
            if (i < nodeCostTexts.Length && nodeCostTexts[i] != null)
            {
                nodeCostTexts[i].text = isUnlocked ? $"{EconomyManager.FormatNumber(cost)} 💎" : "Khóa";
                nodeCostTexts[i].color = canAfford ? new Color(1f, 0.9f, 0.3f, 1f) : new Color(0.6f, 0.65f, 0.7f, 1f);
            }

            // Outline & Background
            if (i < nodeOutlines.Length && nodeOutlines[i] != null)
            {
                nodeOutlines[i].color = isUnlocked ? ColorUnlockedOutline : ColorLockedOutline;
            }
            if (i < nodeBackgrounds.Length && nodeBackgrounds[i] != null)
            {
                nodeBackgrounds[i].color = isUnlocked ? ColorNodeCardActive : ColorNodeCardLocked;
            }

            // Lock Overlay
            if (i < nodeLockOverlays.Length && nodeLockOverlays[i] != null)
            {
                nodeLockOverlays[i].SetActive(!isUnlocked);
            }
            if (i < nodeLockRequirementTexts.Length && nodeLockRequirementTexts[i] != null)
            {
                nodeLockRequirementTexts[i].text = stats.GetUnlockRequirementText(i);
            }
        }

        // 3. Refresh Connector Lines
        if (connectorLines != null && connectorLines.Length > 0 && connectorLines[0] != null)
            connectorLines[0].color = stats.IsNodeUnlocked(2) ? ColorLineActive : ColorLineLocked;
        if (connectorLines != null && connectorLines.Length > 1 && connectorLines[1] != null)
            connectorLines[1].color = stats.IsNodeUnlocked(1) ? ColorLineActive : ColorLineLocked;
        if (connectorLines != null && connectorLines.Length > 2 && connectorLines[2] != null)
            connectorLines[2].color = stats.IsNodeUnlocked(4) ? ColorLineActive : ColorLineLocked;
        if (connectorLines != null && connectorLines.Length > 3 && connectorLines[3] != null)
            connectorLines[3].color = stats.IsNodeUnlocked(5) ? ColorLineActive : ColorLineLocked;

        // 4. Refresh Center Modal nếu đang mở
        if (centerModalRoot != null && centerModalRoot.activeSelf)
        {
            RefreshCenterModal(stats, economy);
        }
    }

    private void RefreshCenterModal(PlayerStatsService stats, EconomyManager economy)
    {
        if (selectedIndex < 0 || selectedIndex >= PlayerStatsService.TalentNodes.Length) return;
        var nodeDef = PlayerStatsService.TalentNodes[selectedIndex];
        bool isUnlocked = stats.IsNodeUnlocked(selectedIndex);
        int level = stats.GetLevel(selectedIndex);
        float cost = stats.GetUpgradeCost(selectedIndex);
        bool canAfford = isUnlocked && economy != null && economy.currentLinhThach >= cost;

        // Tiêu đề
        if (txtModalTitle != null)
        {
            txtModalTitle.text = $"{nodeDef.iconEmoji} {nodeDef.name.ToUpper()}\n<size=20><color=#ffd32a>{nodeDef.title}</color></size>";
        }

        // Nhánh & Tier
        if (txtModalTier != null)
        {
            string branchColor = nodeDef.branchName == "Cương Thân" ? "#00cec9" : "#ff7675";
            txtModalTier.text = $"<color={branchColor}>Nhánh {nodeDef.branchName}</color>  •  Tier {nodeDef.tier}  •  Cấp {level}";
        }

        // Lore tu tiên
        if (txtModalLore != null)
        {
            txtModalLore.text = $"<i>\"{nodeDef.lore}\"</i>";
        }

        // Cơ chế chiến đấu chi tiết
        if (txtModalMechanics != null)
        {
            txtModalMechanics.text = $"<b>Công năng thực chiến:</b>\n{nodeDef.combatMechanics}\n\n" +
                                     $"<color=#ffeaa7>• Tăng tiến mỗi cấp:</color> <b>{nodeDef.statPerLevelText}</b>";
        }

        // So sánh chỉ số Hiện tại vs Cấp kế
        if (txtModalCurrent != null)
        {
            txtModalCurrent.text = level > 0 
                ? $"Hiện tại (Cấp {level}):  <b><color=#55efc4>{stats.GetCurrentEffectString(selectedIndex)}</color></b>"
                : $"Hiện tại:  <color=#b2bec3>Chưa kích hoạt (0%)</color>";
        }

        if (txtModalNext != null)
        {
            txtModalNext.text = isUnlocked 
                ? $"Cấp kế (Cấp {level + 1}):  <b><color=#74b9ff>{stats.GetNextEffectString(selectedIndex)}</color></b>"
                : $"Cấp kế:  <color=#ff7675>Cần mở khóa nhánh trước</color>";
        }

        // Điều kiện mở khóa
        if (txtModalRequirement != null)
        {
            txtModalRequirement.gameObject.SetActive(!isUnlocked);
            if (!isUnlocked)
            {
                txtModalRequirement.text = $"⚠️ {stats.GetUnlockRequirementText(selectedIndex)}";
            }
        }

        if (goModalLockBanner != null)
        {
            goModalLockBanner.SetActive(!isUnlocked);
        }

        // Nút Cường Hóa x1
        if (btnModalUpgrade != null)
        {
            btnModalUpgrade.interactable = canAfford;
        }

        // Chi phí hiển thị
        if (txtModalCost != null)
        {
            if (!isUnlocked)
            {
                txtModalCost.text = "CHƯA MỞ KHÓA";
                txtModalCost.color = new Color(0.7f, 0.4f, 0.4f, 1f);
            }
            else
            {
                txtModalCost.text = $"CƯỜNG HÓA ({EconomyManager.FormatNumber(cost)} 💎)";
                txtModalCost.color = canAfford ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }

        // Nút Cường Hóa x5
        if (btnModalUpgrade5 != null)
        {
            btnModalUpgrade5.gameObject.SetActive(isUnlocked);
            if (isUnlocked)
            {
                float cost5 = 0f;
                int tempLv = level;
                for (int c = 0; c < 5; c++)
                {
                    cost5 += Mathf.Ceil(100f * (selectedIndex + 1) * Mathf.Pow(1.2f, tempLv++));
                }
                btnModalUpgrade5.interactable = economy != null && economy.currentLinhThach >= cost;
                var textComp = btnModalUpgrade5.GetComponentInChildren<Text>();
                if (textComp != null)
                {
                    textComp.text = $"NÂNG x5\n({EconomyManager.FormatNumber(cost5)} 💎)";
                }
            }
        }

        // Nút Nâng Tối Đa
        if (btnModalUpgradeMax != null)
        {
            btnModalUpgradeMax.gameObject.SetActive(isUnlocked);
            btnModalUpgradeMax.interactable = canAfford;
        }
    }
}

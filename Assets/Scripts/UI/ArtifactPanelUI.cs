using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Luyện Khí Điện / Hộ Thân Pháp Khí (Drawer_3).
/// Chuẩn Mobile Portrait:
/// 1. Thanh tài nguyên Quặng Phôi & Hào Quang Hộ Thân ở trên.
/// 2. Danh sách 4 Thẻ Bài Pháp Khí cuộn mượt mà (Áo, Mũ, Giày, Ngọc Bội).
/// 3. Hiển thị chi phí động, chỉ số chiến đấu và nút Cường Hóa to, dễ bấm 1 tay.
/// </summary>
public class ArtifactPanelUI : MonoBehaviour
{
    public static ArtifactPanelUI Instance { get; private set; }

    [Header("Thanh Tài Nguyên")]
    public Text oreLabel;           // "Quặng Phôi: X"
    public Text auraLabel;          // "Hào Quang: Bậc X"

    [Header("4 Thẻ Bài Pháp Khí (Robe, Crown, Boots, Amulet)")]
    public Text[]   artifactLabels;         // Tên + Cấp
    public Text[]   artifactStatsTexts;     // Chỉ số chiến đấu chi tiết
    public Text[]   artifactCostTexts;      // Chi phí nâng cấp động
    public Button[] upgradeButtons;         // Nút Cường Hóa

    private static readonly string[] SlotNames =
    {
        "Phi Thiên Áo",
        "Cửu Thiên Tiên Miện",
        "Vân Phong Hài",
        "Linh Ngọc Bội"
    };

    private void Awake()
    {
        Instance = this;
        WireButtons();
    }

    private void WireButtons()
    {
        if (upgradeButtons == null) return;
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int slotIndex = i;
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(slotIndex));
            }
        }
    }

    private void OnEnable()
    {
        if (ArtifactService.Instance != null)
            ArtifactService.Instance.OnChanged += RefreshUI;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged += HandleLinhThachChanged;
        if (FeatureUnlockService.Instance != null)
            FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI;
        if (CultivationManager.Instance != null)
            CultivationManager.Instance.OnStageChanged += HandleStageChanged;
        RefreshUI();
    }

    private void HandleLinhThachChanged(float _) => RefreshUI();
    private void HandleStageChanged(CultivationStageData _) => RefreshUI();

    private void OnDisable()
    {
        if (ArtifactService.Instance != null)
            ArtifactService.Instance.OnChanged -= RefreshUI;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged;
        if (FeatureUnlockService.Instance != null)
            FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI;
        if (CultivationManager.Instance != null)
            CultivationManager.Instance.OnStageChanged -= HandleStageChanged;
    }

    public void RefreshUI()
    {
        if (ArtifactService.Instance == null) return;
        var svc = ArtifactService.Instance;
        float stone = EconomyManager.Instance?.currentLinhThach ?? 0f;
        int ore = svc.Ore;

        if (oreLabel != null) oreLabel.text = $"Quặng Phôi: <b><color=#f39c12>{ore}</color></b>";
        if (auraLabel != null) auraLabel.text = $"Hào Quang: <b><color=#ffd32a>Bậc {svc.AuraTier}</color></b>";

        for (int i = 0; i < 4; i++)
        {
            var slot = (ArtifactSlot)i;
            int level = svc.GetLevel(slot);
            bool maxed = level >= ArtifactService.MaxLevel;
            int oreCost = svc.GetOreCost(slot);
            float stoneCost = svc.GetStoneCost(slot);
            bool unlocked = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsArtifactSlotUnlocked(slot);
            bool canAfford = unlocked && !maxed && ore >= oreCost && stone >= stoneCost;

            // 1. Tên & Cấp độ
            if (artifactLabels != null && i < artifactLabels.Length && artifactLabels[i] != null)
            {
                string levelStr = maxed ? "<color=#ffd32a>MAX</color>" : $"<color=#55efc4>Lv.{level}</color>/30";
                artifactLabels[i].text = $"<b>{SlotNames[i]}</b>  {levelStr}";
            }

            // 2. Chỉ số chiến đấu chi tiết
            if (artifactStatsTexts != null && i < artifactStatsTexts.Length && artifactStatsTexts[i] != null)
            {
                artifactStatsTexts[i].text = GetSlotStatsFormatted(slot, level);
            }

            // 3. Chi phí động
            if (artifactCostTexts != null && i < artifactCostTexts.Length && artifactCostTexts[i] != null)
            {
                if (!unlocked)
                {
                    artifactCostTexts[i].text = FeatureUnlockService.Instance.GetArtifactSlotUnlockRequirementText(slot);
                }
                else if (maxed)
                {
                    artifactCostTexts[i].text = "<color=#ffd32a>Đạt Cực Hạn Cường Hóa</color>";
                }
                else
                {
                    string oreColor = ore >= oreCost ? "#55efc4" : "#ff7675";
                    string stoneColor = stone >= stoneCost ? "#ffeaa7" : "#ff7675";
                    artifactCostTexts[i].text = $"Cần: <color={oreColor}>Quặng {oreCost}</color>  |  <color={stoneColor}>LT {EconomyManager.FormatNumber(stoneCost)}</color>";
                }
            }

            // 4. Nút bấm
            if (upgradeButtons != null && i < upgradeButtons.Length && upgradeButtons[i] != null)
            {
                upgradeButtons[i].interactable = canAfford;
                var btnText = upgradeButtons[i].GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = !unlocked ? "CHƯA MỞ KHÓA" : maxed ? "ĐẠI THÀNH" : (level == 0 ? "CHẾ TẠO" : "CƯỜNG HÓA");
                }
            }
        }
    }

    private string GetSlotStatsFormatted(ArtifactSlot slot, int level)
    {
        switch (slot)
        {
            case ArtifactSlot.Robe:
                float hp = level * 100f;
                float bossRed = Mathf.Min(30f, level * 1.0f);
                return $"• Khí Huyết Tối Đa: <b><color=#55efc4>+{hp:N0}</color></b>\n• Giảm Sát Thương Boss: <b><color=#74b9ff>-{bossRed:F1}%</color></b>";

            case ArtifactSlot.Crown:
                float regen = level * 2f;
                float ten = Mathf.Min(50f, level * 1.5f);
                return $"• Hồi Huyết Mỗi Giây: <b><color=#55efc4>+{regen:F1}/s</color></b>\n• Kháng Choáng & Khống Chế: <b><color=#ffeaa7>+{ten:F1}%</color></b>";

            case ArtifactSlot.Boots:
                float dodge = Mathf.Min(30f, level * 1.0f);
                float spd = level * 1.2f;
                return $"• Thân Pháp Né Tránh: <b><color=#55efc4>+{dodge:F1}%</color></b>\n• Tốc Độ Phi Kiếm: <b><color=#74b9ff>+{spd:F1}%</color></b>";

            case ArtifactSlot.Amulet:
                float crit = Mathf.Min(25f, level * 0.6f);
                float stoneDrop = level * 2.0f;
                return $"• Tỉ Lệ Bạo Kích: <b><color=#ff7675>+{crit:F1}%</color></b>\n• Tỉ Lệ Rơi Linh Thạch: <b><color=#ffd32a>+{stoneDrop:F1}%</color></b>";

            default:
                return "";
        }
    }

    public void OnUpgradeClicked(int slotIndex)
    {
        if (ArtifactService.Instance == null) return;
        bool ok = ArtifactService.Instance.TryUpgrade((ArtifactSlot)slotIndex);
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log($"Cường hóa thành công {SlotNames[slotIndex]}!", Color.cyan);
        }
        else
        {
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log("Không đủ Quặng Phôi hoặc Linh Thạch!", Color.yellow);
        }
        RefreshUI();
    }

    public void OnUpgradeRobe()   => OnUpgradeClicked(0);
    public void OnUpgradeCrown()  => OnUpgradeClicked(1);
    public void OnUpgradeBoots()  => OnUpgradeClicked(2);
    public void OnUpgradeAmulet() => OnUpgradeClicked(3);
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Trận Pháp Các / Hộ Thân Đại Trận (Drawer_4).
/// Chuẩn Mobile Portrait:
/// 1. Thanh tiêu đề & trạng thái Đại Trận đang sử dụng.
/// 2. Khung trạng thái Tụ Linh Trận (Đồng hồ đếm ngược & Nút kích hoạt 30s).
/// 3. ScrollView cuộn mượt mà 6 Đại Trận Pháp (Lôi, Tụ Linh, Băng, Vạn Kiếm, Huyết Ma, Kim Cang).
/// 4. Mỗi thẻ có nút [KÍCH HOẠT] đổi trận và [TU LUYỆN] hiển thị chi phí Quặng + Linh Thạch.
/// </summary>
public class FormationPanelUI : MonoBehaviour
{
    public static FormationPanelUI Instance { get; private set; }

    [Header("Trạng thái Tụ Linh (Chủ động x2 Tu Vi)")]
    public Text spiritCooldownLabel;
    public Button activateSpiritBtn;

    [Header("Tài Nguyên & Trận Hiện Dùng")]
    public Text txtOreLabel;
    public Text txtCurrentActiveLabel;

    [Header("6 Đại Trận Pháp")]
    public Text[]   formationLabels;        // [6] — Tên + Cấp độ
    public Text[]   formationDescTexts;     // [6] — Mô tả chiến đấu chi tiết
    public Text[]   formationCostTexts;     // [6] — Chi phí Quặng + Linh Thạch
    public Button[] selectButtons;          // [6] — Nút [KÍCH HOẠT]
    public Button[] upgradeButtons;         // [6] — Nút [TU LUYỆN]
    public Text[]   statusBadges;           // [6] — Tag "[ĐANG DÙNG]", "[SẴN SÀNG]", "[CHƯA HỌC]"

    public static readonly string[] FormationNames =
    {
        "Cửu Thiên Lôi Trận",
        "Bát Quái Tụ Linh Trận",
        "Cực Hàn Băng Trận",
        "Vạn Kiếm Quy Tông Trận",
        "Huyết Ma Hóa Sinh Trận",
        "Huyền Vũ Kim Cang Trận"
    };

    public static readonly string[] FormationDescs =
    {
        "Định kỳ giáng sấm sét AoE quét sạch quái vật xung quanh người chơi.\n  +0.25m phạm vi & +40x sát thương mỗi cấp.",
        "Kích hoạt chủ động: x2 tốc độ tích lũy Tu Vi trong 30 giây (Cooldown 120s).\n  Cấp càng cao, thời gian kéo dài càng tăng (+2s/Lv).",
        "Hàn khí áp chế cực độ, làm chậm tốc độ di chuyển và tốc độ đánh của Boss.\n  Tốc độ Boss giảm từ 50% xuống tối đa 25%.",
        "Tăng tốc độ bay của toàn bộ phi kiếm +20% (+1.5%/Lv).\n  Định kỳ mỗi 8s triệu hồi cơn mưa kiếm bão tố quét toàn màn hình.",
        "Mỗi đòn chém của phi kiếm chuyển hóa 5% - 20% sát thương thành Khí Huyết hồi phục.\n  Duy trì sự sống bền bỉ khi vượt ải yêu quái.",
        "Huyền Vũ hộ thể: Giảm trực tiếp 15% - 40% toàn bộ sát thương nhận vào.\n  Bảo vệ an toàn tính mạng trước đòn sát thủ của Boss."
    };

    private void Awake()
    {
        Instance = this;
        WireButtons();
    }

    private void WireButtons()
    {
        if (selectButtons != null)
        {
            for (int i = 0; i < selectButtons.Length; i++)
            {
                int index = i;
                if (selectButtons[i] != null)
                {
                    selectButtons[i].onClick.RemoveAllListeners();
                    selectButtons[i].onClick.AddListener(() => OnSelectFormation(index));
                }
            }
        }

        if (upgradeButtons != null)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int index = i;
                if (upgradeButtons[i] != null)
                {
                    upgradeButtons[i].onClick.RemoveAllListeners();
                    upgradeButtons[i].onClick.AddListener(() => OnUpgradeFormation(index));
                }
            }
        }

        if (activateSpiritBtn != null)
        {
            activateSpiritBtn.onClick.RemoveAllListeners();
            activateSpiritBtn.onClick.AddListener(OnActivateSpirit);
        }
    }

    private void OnEnable()
    {
        if (FormationService.Instance != null)
            FormationService.Instance.OnChanged += RefreshUI;
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
        if (FormationService.Instance != null)
            FormationService.Instance.OnChanged -= RefreshUI;
        if (ArtifactService.Instance != null)
            ArtifactService.Instance.OnChanged -= RefreshUI;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged -= HandleLinhThachChanged;
        if (FeatureUnlockService.Instance != null)
            FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI;
        if (CultivationManager.Instance != null)
            CultivationManager.Instance.OnStageChanged -= HandleStageChanged;
    }

    private void Update()
    {
        UpdateSpiritCooldown();
    }

    public void RefreshUI()
    {
        if (FormationService.Instance == null) return;
        var svc = FormationService.Instance;
        int ore = ArtifactService.Instance?.Ore ?? 0;
        float stone = EconomyManager.Instance?.currentLinhThach ?? 0f;
        FormationId selected = svc.Selected;

        if (txtOreLabel != null) txtOreLabel.text = $"Quặng Phôi: <b><color=#f39c12>{ore}</color></b>";
        if (txtCurrentActiveLabel != null)
        {
            int sIdx = (int)selected;
            string sName = sIdx >= 0 && sIdx < FormationNames.Length ? FormationNames[sIdx] : "Chưa chọn";
            txtCurrentActiveLabel.text = $"Đang Dùng: <b><color=#ffd32a>{sName}</color></b>";
        }

        int count = Mathf.Min(6, FormationNames.Length);
        for (int i = 0; i < count; i++)
        {
            var id = (FormationId)i;
            int level = svc.GetLevel(id);
            int oreCost = svc.GetOreCost(id);
            float stoneCost = svc.GetStoneCost(id);
            bool isSelected = selected == id && level > 0;
            bool maxed = level >= FormationService.MaxLevel;
            bool unlocked = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFormationUnlocked(id);
            bool canAfford = unlocked && !maxed && ore >= oreCost && stone >= stoneCost;

            // 1. Tên & Cấp
            if (formationLabels != null && i < formationLabels.Length && formationLabels[i] != null)
            {
                string lvlStr = level == 0 ? "<color=#888888>(Chưa Học)</color>"
                    : maxed ? "<color=#ffd32a>MAX</color>"
                    : $"<color=#55efc4>Lv.{level}</color>/30";
                formationLabels[i].text = $"<b>{FormationNames[i]}</b>  {lvlStr}";
            }

            // 2. Mô tả chiến đấu
            if (formationDescTexts != null && i < formationDescTexts.Length && formationDescTexts[i] != null)
            {
                formationDescTexts[i].text = FormationDescs[i];
            }

            // 3. Chi phí
            if (formationCostTexts != null && i < formationCostTexts.Length && formationCostTexts[i] != null)
            {
                if (!unlocked)
                {
                    formationCostTexts[i].text = FeatureUnlockService.Instance.GetFormationUnlockRequirementText(id);
                }
                else if (maxed)
                {
                    formationCostTexts[i].text = "<color=#ffd32a>Đại Trận Đã Đại Thành</color>";
                }
                else
                {
                    string oreColor = ore >= oreCost ? "#55efc4" : "#ff7675";
                    string stoneColor = stone >= stoneCost ? "#ffeaa7" : "#ff7675";
                    formationCostTexts[i].text = $"Cần: <color={oreColor}>Quặng {oreCost}</color> | <color={stoneColor}>LT {EconomyManager.FormatNumber(stoneCost)}</color>";
                }
            }

            // 4. Tag trạng thái
            if (statusBadges != null && i < statusBadges.Length && statusBadges[i] != null)
            {
                if (!unlocked)
                {
                    statusBadges[i].text = "CHƯA MỞ KHÓA";
                }
                else if (isSelected)
                {
                    statusBadges[i].text = "<color=#ffd32a>ĐANG KÍCH HOẠT</color>";
                }
                else if (level == 0)
                {
                    statusBadges[i].text = "<color=#7f8c8d>Chưa Khai Ngộ</color>";
                }
                else
                {
                    statusBadges[i].text = "<color=#74b9ff>Sẵn Sàng Dùng</color>";
                }
            }

            // 5. Nút Kích Hoạt
            if (selectButtons != null && i < selectButtons.Length && selectButtons[i] != null)
            {
                selectButtons[i].interactable = unlocked && level > 0 && !isSelected;
                var bText = selectButtons[i].GetComponentInChildren<Text>();
                if (bText != null)
                {
                    bText.text = isSelected ? "ĐANG DÙNG" : (level == 0 ? "CHƯA HỌC" : "KÍCH HOẠT");
                }
                var img = selectButtons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = isSelected ? new Color(0.85f, 0.65f, 0.15f, 1f)
                        : (level > 0 ? new Color(0.15f, 0.45f, 0.35f, 1f) : new Color(0.2f, 0.25f, 0.3f, 1f));
                }
            }

            // 6. Nút Tu Luyện
            if (upgradeButtons != null && i < upgradeButtons.Length && upgradeButtons[i] != null)
            {
                upgradeButtons[i].interactable = canAfford;
                var upgText = upgradeButtons[i].GetComponentInChildren<Text>();
                if (upgText != null)
                {
                    upgText.text = maxed ? "ĐẠI THÀNH" : (level == 0 ? "HỌC TRẬN" : "TU LUYỆN");
                }
            }
        }

        UpdateSpiritCooldown();
    }

    private void UpdateSpiritCooldown()
    {
        if (FormationService.Instance == null) return;
        var svc = FormationService.Instance;
        bool spiritSelected = svc.Selected == FormationId.Spirit && svc.GetLevel(FormationId.Spirit) > 0;

        if (activateSpiritBtn != null)
        {
            bool unlocked = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFormationUnlocked(FormationId.Spirit);
            activateSpiritBtn.interactable = unlocked && spiritSelected && svc.SpiritCooldown == System.TimeSpan.Zero && !svc.SpiritActive;
            activateSpiritBtn.gameObject.SetActive(spiritSelected);
        }

        if (spiritCooldownLabel != null)
        {
            if (!spiritSelected)
            {
                spiritCooldownLabel.text = "";
                return;
            }
            if (svc.SpiritActive)
            {
                spiritCooldownLabel.text = "<b><color=#55efc4>Tụ Linh Trận Đang Phát Huy Uy Lực (x2 Tu Vi)!</color></b>";
            }
            else if (svc.SpiritCooldown > System.TimeSpan.Zero)
            {
                var cd = svc.SpiritCooldown;
                spiritCooldownLabel.text = $"⏳ Hồi chiêu: <b>{cd.Minutes:D2}:{cd.Seconds:D2}</b>";
            }
            else
            {
                spiritCooldownLabel.text = "<b><color=#ffd32a>Tụ Linh Trận Sẵn Sàng Kích Hoạt!</color></b>";
            }
        }
    }

    public void OnSelectFormation(int index)
    {
        if (FormationService.Instance == null) return;
        bool ok = FormationService.Instance.TrySelect((FormationId)index);
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log($"Đã chuyển sang kích hoạt {FormationNames[index]}!", Color.cyan);
        }
        RefreshUI();
    }

    public void OnUpgradeFormation(int index)
    {
        if (FormationService.Instance == null) return;
        bool ok = FormationService.Instance.TryUpgrade((FormationId)index);
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log($"Tu luyện thành công {FormationNames[index]}!", Color.yellow);
        }
        else
        {
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log("Không đủ Quặng Phôi hoặc Linh Thạch!", Color.yellow);
        }
        RefreshUI();
    }

    public void OnActivateSpirit()
    {
        bool ok = FormationService.Instance?.TryActivateSpirit() ?? false;
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log("Tụ Linh Trận đại phát uy lực, nhân đôi tốc độ tu vi!", Color.green);
        }
        else
        {
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log("Tụ Linh Trận đang hồi chiêu hoặc chưa chọn!", Color.yellow);
        }
        RefreshUI();
    }

    public void OnSelectThunder() => OnSelectFormation(0);
    public void OnSelectSpirit()  => OnSelectFormation(1);
    public void OnSelectFrost()   => OnSelectFormation(2);
}

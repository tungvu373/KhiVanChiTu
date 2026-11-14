// File: Assets/Scripts/UI/AlchemyPanelUI.cs
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Luyện Đan Các (Alchemy Chamber - Drawer_2).
/// Thiết kế chuẩn Mobile Portrait:
/// 1. Thanh kho Thảo Dược & Cấp Đan Sư ở trên cùng.
/// 2. Khung Lò Luyện Trung Tâm hiển thị ngọn lửa, đồng hồ đếm ngược và nút hoàn tất tức thì.
/// 3. Hai Thẻ Bài Thuốc (Tu Vi Đan & Đột Phá Đan) hiển thị chi phí thảo dược động và nút Bắt Đầu Luyện.
/// 4. Kho Đan Dược sẵn có kèm nút [UỐNG ĐAN] nhận Tu Vi tức thì.
/// </summary>
public class AlchemyPanelUI : MonoBehaviour
{
    public static AlchemyPanelUI Instance { get; private set; }

    [Header("Kho Thảo Dược & Cấp Đan Sư")]
    public Text txtHerbLinhChi;
    public Text txtHerbNgungKhi;
    public Text txtHerbTuyetLien;
    public Text txtAlchemistRank;
    public Text txtMedicineDust;

    [Header("Lò Luyện Trung Tâm")]
    public Text txtFurnaceStatus;
    public Text txtFurnaceTimer;
    public Image imgFurnaceProgress;
    public Button btnFinishWithDust;
    public Text txtDustCost;

    [Header("Thẻ Bài Thuốc 1: Tu Vi Đan")]
    public Text txtCostTuVi;
    public Button btnBrewTuVi;

    [Header("Thẻ Bài Thuốc 2: Đột Phá Đan")]
    public Text txtCostBreakthrough;
    public Button btnBrewBreakthrough;

    [Header("Kho Đan Dược")]
    public Text txtTuViPillsSummary;
    public Text txtBreakthroughPillsSummary;
    public Button btnUseTuViPill;

    // Tương thích ngược với các trường cũ
    [Header("Legacy References")]
    public Text linhChiLabel;
    public Text caoNgungKhiLabel;
    public Text tuyetLienLabel;
    public Text brewTimerLabel;
    public Button brewCultivationBtn;
    public Button brewBreakthroughBtn;
    public Button useDustBtn;
    public Text dustLabel;
    public Text pillInventoryLabel;

    private void Awake()
    {
        Instance = this;

        if (btnBrewTuVi != null)
        {
            btnBrewTuVi.onClick.RemoveAllListeners();
            btnBrewTuVi.onClick.AddListener(OnBrewCultivationClicked);
        }

        if (btnBrewBreakthrough != null)
        {
            btnBrewBreakthrough.onClick.RemoveAllListeners();
            btnBrewBreakthrough.onClick.AddListener(OnBrewBreakthroughClicked);
        }

        if (btnFinishWithDust != null)
        {
            if (txtDustCost == null) txtDustCost = btnFinishWithDust.GetComponentInChildren<Text>(true);
            btnFinishWithDust.onClick.RemoveAllListeners();
            btnFinishWithDust.onClick.AddListener(OnUseDustClicked);
        }

        if (btnUseTuViPill != null)
        {
            btnUseTuViPill.onClick.RemoveAllListeners();
            btnUseTuViPill.onClick.AddListener(OnUseTuViPillClicked);
        }

        // Đồng bộ legacy buttons
        if (brewCultivationBtn != null) brewCultivationBtn.onClick.AddListener(OnBrewCultivationClicked);
        if (brewBreakthroughBtn != null) brewBreakthroughBtn.onClick.AddListener(OnBrewBreakthroughClicked);
        if (useDustBtn != null) useDustBtn.onClick.AddListener(OnUseDustClicked);
    }

    private void OnEnable()
    {
        if (AlchemyService.Instance != null)
            AlchemyService.Instance.OnChanged += RefreshUI;
        if (FeatureUnlockService.Instance != null)
            FeatureUnlockService.Instance.OnUnlockStateChanged += RefreshUI;
        if (CultivationManager.Instance != null)
            CultivationManager.Instance.OnStageChanged += HandleStageChanged;
        RefreshUI();
    }

    private void HandleStageChanged(CultivationStageData _) => RefreshUI();

    private void OnDisable()
    {
        if (AlchemyService.Instance != null)
            AlchemyService.Instance.OnChanged -= RefreshUI;
        if (FeatureUnlockService.Instance != null)
            FeatureUnlockService.Instance.OnUnlockStateChanged -= RefreshUI;
        if (CultivationManager.Instance != null)
            CultivationManager.Instance.OnStageChanged -= HandleStageChanged;
    }

    private void Update()
    {
        if (AlchemyService.Instance != null && AlchemyService.Instance.IsBrewing)
        {
            UpdateBrewTimer();
        }
    }

    public void RefreshUI()
    {
        if (AlchemyService.Instance == null) return;
        var svc = AlchemyService.Instance;
        int curRealm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0;

        // 1. Kho Thảo Dược
        int cLinhChi = svc.GetHerbCount(0);
        int cNgungKhi = svc.GetHerbCount(1);
        int cTuyetLien = svc.GetHerbCount(2);

        if (txtHerbLinhChi != null)   txtHerbLinhChi.text   = $"Linh Chi: <b><color=#55efc4>{cLinhChi}</color></b>";
        if (txtHerbNgungKhi != null)  txtHerbNgungKhi.text  = $"Ngưng Khí: <b><color=#ffeaa7>{cNgungKhi}</color></b>";
        if (txtHerbTuyetLien != null) txtHerbTuyetLien.text = $"Tuyết Liên: <b><color=#74b9ff>{cTuyetLien}</color></b>";

        // Cấp Đan Sư & Bụi Dược
        if (txtAlchemistRank != null) txtAlchemistRank.text = $"Đan Sư Cấp {svc.AlchemistRank}";
        if (txtMedicineDust != null)  txtMedicineDust.text  = $"Bụi Dược: {svc.State.medicineDust}";

        // Legacy labels
        if (linhChiLabel != null)     linhChiLabel.text     = $"Linh Chi: {cLinhChi}";
        if (caoNgungKhiLabel != null) caoNgungKhiLabel.text = $"Ngưng Khí Thảo: {cNgungKhi}";
        if (tuyetLienLabel != null)   tuyetLienLabel.text   = $"Tuyết Liên: {cTuyetLien}";
        if (dustLabel != null)        dustLabel.text        = $"Bụi Dược: {svc.State.medicineDust}";

        // 2. Chi phí công thức
        int[] costTuVi = svc.GetRecipeCosts(PillRecipe.Cultivation, curRealm);
        bool unlocked = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy);
        bool canBrewTuVi = unlocked && !svc.IsBrewing && cLinhChi >= costTuVi[0] && cNgungKhi >= costTuVi[1];
        if (txtCostTuVi != null)
        {
            string col0 = cLinhChi >= costTuVi[0] ? "#55efc4" : "#ff7675";
            string col1 = cNgungKhi >= costTuVi[1] ? "#ffeaa7" : "#ff7675";
            txtCostTuVi.text = $"Cần: <color={col0}>Chi {costTuVi[0]}</color> · <color={col1}>Ngưng {costTuVi[1]}</color>";
        }
        if (btnBrewTuVi != null) btnBrewTuVi.interactable = canBrewTuVi;
        if (brewCultivationBtn != null) brewCultivationBtn.interactable = canBrewTuVi;

        int[] costBT = svc.GetRecipeCosts(PillRecipe.Breakthrough, curRealm);
        bool canBrewBT = unlocked && !svc.IsBrewing && cLinhChi >= costBT[0] && cNgungKhi >= costBT[1] && cTuyetLien >= costBT[2];
        if (txtCostBreakthrough != null)
        {
            string col0 = cLinhChi >= costBT[0] ? "#55efc4" : "#ff7675";
            string col1 = cNgungKhi >= costBT[1] ? "#ffeaa7" : "#ff7675";
            string col2 = cTuyetLien >= costBT[2] ? "#74b9ff" : "#ff7675";
            txtCostBreakthrough.text = $"Cần: <color={col0}>Chi {costBT[0]}</color> · <color={col1}>Ngưng {costBT[1]}</color> · <color={col2}>Liên {costBT[2]}</color>";
        }
        if (btnBrewBreakthrough != null) btnBrewBreakthrough.interactable = canBrewBT;
        if (brewBreakthroughBtn != null) brewBreakthroughBtn.interactable = canBrewBT;

        // Nút hoàn tất bằng Bụi Dược
        int dustCost = svc.DustFinishCost;
        bool canDust = svc.IsBrewing && svc.State.medicineDust >= dustCost;
        if (txtDustCost != null) txtDustCost.text = $"Tốc Hành ({dustCost} Bụi Dược)";
        if (btnFinishWithDust != null) btnFinishWithDust.interactable = canDust;
        if (useDustBtn != null)        useDustBtn.interactable        = canDust;

        UpdateBrewTimer();

        // 3. Tồn kho đan dược & Uống đan
        RefreshPillVault(svc, curRealm);
    }

    private void UpdateBrewTimer()
    {
        if (AlchemyService.Instance == null) return;
        var svc = AlchemyService.Instance;

        if (svc.IsBrewing)
        {
            var remaining = svc.BrewRemaining;
            string pillName = svc.State.brewingRecipe == 0 ? "Tu Vi Đan" : "Đột Phá Đan";
            if (remaining.TotalSeconds > 0)
            {
                if (txtFurnaceStatus != null) txtFurnaceStatus.text = $"Đang Luyện: <b><color=#ffd32a>{pillName}</color></b>";
                if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = $"⏳ Thời gian còn: <b>{remaining.Minutes:D2}:{remaining.Seconds:D2}</b>";
                if (brewTimerLabel != null)   brewTimerLabel.text   = $"⏳ Đang luyện {pillName}: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
            else
            {
                if (txtFurnaceStatus != null) txtFurnaceStatus.text = "Đan thành xuất lò!";
                if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = "Chờ thu nạp...";
                if (brewTimerLabel != null)   brewTimerLabel.text   = "Đan thành xuất lò!";
            }
        }
        else
        {
            if (txtFurnaceStatus != null) txtFurnaceStatus.text = "Lò Luyện Trống";
            if (txtFurnaceTimer != null)  txtFurnaceTimer.text  = "Chọn bài thuốc bên dưới để bắt đầu luyện đan";
            if (brewTimerLabel != null)   brewTimerLabel.text   = "Lò luyện sẵn sàng";
        }
    }

    private void RefreshPillVault(AlchemyService svc, int realm)
    {
        int tuViBasic = svc.GetPillCount(realm, PillRecipe.Cultivation, PillQuality.Basic);
        int tuViRefined = svc.GetPillCount(realm, PillRecipe.Cultivation, PillQuality.Refined);
        int tuViSupreme = svc.GetPillCount(realm, PillRecipe.Cultivation, PillQuality.Supreme);
        int totalTuVi = tuViBasic + tuViRefined + tuViSupreme;

        if (txtTuViPillsSummary != null)
        {
            txtTuViPillsSummary.text = totalTuVi > 0
                ? $"<b>Tu Vi Đan:</b> <color=#55efc4>Sơ:{tuViBasic}</color> | <color=#74b9ff>Trung:{tuViRefined}</color> | <color=#ffd32a>Cực:{tuViSupreme}</color>"
                : "<b>Tu Vi Đan:</b> <i>(Chưa có)</i>";
        }

        int btBasic = svc.GetPillCount(realm, PillRecipe.Breakthrough, PillQuality.Basic);
        int btRefined = svc.GetPillCount(realm, PillRecipe.Breakthrough, PillQuality.Refined);
        int btSupreme = svc.GetPillCount(realm, PillRecipe.Breakthrough, PillQuality.Supreme);
        int totalBT = btBasic + btRefined + btSupreme;

        if (txtBreakthroughPillsSummary != null)
        {
            txtBreakthroughPillsSummary.text = totalBT > 0
                ? $"<b>Đột Phá Đan:</b> <color=#55efc4>Sơ:{btBasic}</color> | <color=#74b9ff>Trung:{btRefined}</color> | <color=#ffd32a>Cực:{btSupreme}</color>"
                : "<b>Đột Phá Đan:</b> <i>(Chưa có)</i>";
        }

        // Nút uống đan Tu Vi
        bool canDrink = totalTuVi > 0 && CultivationManager.Instance != null && !CultivationManager.Instance.IsTuViMaxed();
        if (btnUseTuViPill != null)
        {
            btnUseTuViPill.interactable = canDrink;
            var label = btnUseTuViPill.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = totalTuVi > 0 ? $"UỐNG ĐAN ({totalTuVi})" : "HẾT ĐAN";
            }
        }

        // Legacy pill inventory label
        if (pillInventoryLabel != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>Tồn kho đan cảnh giới {realm}</b>");
            if (totalTuVi > 0) sb.AppendLine($"  Tu Vi Đan: {totalTuVi} viên (Sơ:{tuViBasic}, Trung:{tuViRefined}, Cực:{tuViSupreme})");
            if (totalBT > 0)   sb.AppendLine($"  Đột Phá Đan: {totalBT} viên (Sơ:{btBasic}, Trung:{btRefined}, Cực:{btSupreme})");
            if (totalTuVi == 0 && totalBT == 0) sb.AppendLine("  <i>(Chưa có đan dược)</i>");
            pillInventoryLabel.text = sb.ToString();
        }
    }

    // ── Button Callbacks ──────────────────────────────────────────────────────

    public void OnBrewCultivationClicked()
    {
        if (AlchemyService.Instance == null) return;
        int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0;
        bool ok = AlchemyService.Instance.TryStartBrew(PillRecipe.Cultivation, realm);
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Đã bắt đầu luyện một lò Tu Vi Đan!", Color.cyan);
        }
        else
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Không đủ thảo dược hoặc lò đang bận!", Color.yellow);
        }
        RefreshUI();
    }

    public void OnBrewBreakthroughClicked()
    {
        if (AlchemyService.Instance == null) return;
        int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0;
        bool ok = AlchemyService.Instance.TryStartBrew(PillRecipe.Breakthrough, realm);
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Đã bắt đầu luyện một lò Đột Phá Đan!", new Color(1f, 0.6f, 0.2f));
        }
        else
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Không đủ thảo dược hoặc lò đang bận!", Color.yellow);
        }
        RefreshUI();
    }

    public void OnUseDustClicked()
    {
        var alchemy = AlchemyService.Instance;
        if (alchemy == null) return;
        int cost = alchemy.DustFinishCost;
        bool ok = alchemy.TryUseDustToFinish();
        if (ok)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"Tiêu hao {cost} Bụi Dược, lò luyện tức thì đại thành!", Color.green);
        }
        RefreshUI();
    }

    public void OnUseTuViPillClicked()
    {
        if (AlchemyService.Instance == null) return;
        int realm = CultivationManager.Instance != null ? (int)CultivationManager.Instance.CurrentRealm : 0;

        // Ưu tiên uống đan từ thấp lên cao (Sơ -> Trung -> Cực)
        bool used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Basic);
        if (!used) used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Refined);
        if (!used) used = AlchemyService.Instance.TryUseCultivationPill(realm, PillQuality.Supreme);

        if (used)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Uống 1 viên Tu Vi Đan, tu vi tăng mạnh!", Color.cyan);
        }
        RefreshUI();
    }
}

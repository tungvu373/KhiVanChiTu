using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIPlaceholder : MonoBehaviour
{
    [Header("UI Tiền tệ")]
    public Text txtLinhThach;

    [Header("UI Linh Lực")]
    public Text txtLinhLuc;
    public Button btnLinhLuc;

    [Header("UI Kiếm Ý")]
    public Text txtKiemY;
    public Button btnKiemY;

    [Header("UI Thần Thức")]
    public Text txtThanThuc;
    public Button btnThanThuc;

    [Header("UI Tụ Linh")]
    public Text txtTuLinh;
    public Button btnTuLinh;

    private void Start()
    {
        // Đăng ký lắng nghe sự kiện thay đổi tiền tệ và nâng cấp
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnLinhThachChanged += UpdateEconomyUI;
            UpdateEconomyUI(EconomyManager.Instance.currentLinhThach);
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradesChanged += UpdateUpgradesUI;
            UpdateUpgradesUI();
        }

        // Gắn sự kiện click cho các nút
        if(btnLinhLuc != null) btnLinhLuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("LinhLuc"));
        if(btnKiemY != null) btnKiemY.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("KiemY"));
        if(btnThanThuc != null) btnThanThuc.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("ThanThuc"));
        if(btnTuLinh != null) btnTuLinh.onClick.AddListener(() => UpgradeManager.Instance.BuyUpgrade("TuLinh"));
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnLinhThachChanged -= UpdateEconomyUI;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradesChanged -= UpdateUpgradesUI;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // NÚT CHEAT ĐỂ TEST: Bấm phím Space để nhận 100 Linh Thạch
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (EconomyManager.Instance != null) EconomyManager.Instance.AddLinhThach(100);
            Debug.Log("[CHEAT] +100 Linh Thạch (Editor Only)");
        }
#endif
    }

    private void UpdateEconomyUI(float linhThach)
    {
        if (txtLinhThach != null)
            txtLinhThach.text = EconomyManager.FormatNumber(linhThach);
        
        // Cập nhật lại trạng thái nút (bật/tắt) khi tiền thay đổi
        UpdateUpgradesUI();
    }

    private void UpdateUpgradesUI()
    {
        if (UpgradeManager.Instance == null) return;

        UpdateSingleUpgradeUI(txtLinhLuc, btnLinhLuc, UpgradeManager.Instance.linhLucData, UpgradeManager.Instance.linhLucLevel);
        UpdateSingleUpgradeUI(txtKiemY, btnKiemY, UpgradeManager.Instance.kiemYData, UpgradeManager.Instance.kiemYLevel);
        UpdateSingleUpgradeUI(txtThanThuc, btnThanThuc, UpgradeManager.Instance.thanThucData, UpgradeManager.Instance.thanThucLevel);
        UpdateSingleUpgradeUI(txtTuLinh, btnTuLinh, UpgradeManager.Instance.tuLinhData, UpgradeManager.Instance.tuLinhLevel);
    }

    private void UpdateSingleUpgradeUI(Text txt, Button btn, UpgradeData data, int level)
    {
        if (data == null || txt == null || btn == null) return;

        float cost = data.GetCost(level);
        float value = data.GetValue(level);
        float nextValue = data.GetValue(level + 1);

        txt.text = $"{data.upgradeName} (Lv.{level})\nHiệu quả: {value:F1} -> {nextValue:F1}";
        
        Text btnText = btn.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.text = $"Nâng cấp\n{Mathf.FloorToInt(cost)} LT";
        }

        // Vô hiệu hóa nút nếu không đủ tiền
        btn.interactable = (EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= cost);
    }
}

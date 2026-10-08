using UnityEngine;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Dữ liệu các thẻ nâng cấp")]
    public UpgradeData linhLucData;
    public UpgradeData kiemYData;
    public UpgradeData thanThucData;
    public UpgradeData tuLinhData;

    [Header("Cấp độ hiện tại")]
    public int linhLucLevel = 1;
    public int kiemYLevel = 1;
    public int thanThucLevel = 1;
    public int tuLinhLevel = 1;

    // Sự kiện được gọi mỗi khi có một nâng cấp thành công
    public event Action OnUpgradesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ApplyUpgradeEffects();
    }

    public void BuyUpgrade(string upgradeId)
    {
        UpgradeData data = null;
        int currentLevel = 0;

        switch (upgradeId)
        {
            case "LinhLuc":
                data = linhLucData;
                currentLevel = linhLucLevel;
                break;
            case "KiemY":
                data = kiemYData;
                currentLevel = kiemYLevel;
                break;
            case "ThanThuc":
                data = thanThucData;
                currentLevel = thanThucLevel;
                break;
            case "TuLinh":
                data = tuLinhData;
                currentLevel = tuLinhLevel;
                break;
        }

        if (data != null)
        {
            float cost = data.GetCost(currentLevel);
            if (EconomyManager.Instance.SpendLinhThach(cost))
            {
                switch (upgradeId)
                {
                    case "LinhLuc": linhLucLevel++; break;
                    case "KiemY": kiemYLevel++; break;
                    case "ThanThuc": thanThucLevel++; break;
                    case "TuLinh": tuLinhLevel++; break;
                }
                
                ApplyUpgradeEffects();
                OnUpgradesChanged?.Invoke();
                Debug.Log($"Đã mua {data.upgradeName} lên cấp {currentLevel + 1} với giá {cost} Linh Thạch");
            }
            else
            {
                Debug.LogWarning("Không đủ Linh Thạch để nâng cấp!");
            }
        }
    }

    private void ApplyUpgradeEffects()
    {
        // 1. Linh Lực: Cập nhật dung lượng và tốc độ hồi mana cho SkillManager
        if (SkillManager.Instance != null && linhLucData != null)
        {
            float manaValue = linhLucData.GetValue(linhLucLevel);
            SkillManager.Instance.maxLinhLuc = manaValue;
            SkillManager.Instance.linhLucRegenRate = manaValue * 0.1f; // Tốc độ hồi bằng 10% mana tối đa
        }

        // Push cập nhật kiếm đang trang bị ngay khi upgrade Kiếm Ý / Thần Thức
        if (MergeManager.Instance != null)
            MergeManager.Instance.OnEquipChanged();
    }

    // Các hàm cung cấp chỉ số cho các hệ thống khác (Combat, Phi Kiếm, Drop rates)
    public float GetKiemYValue() => kiemYData != null ? kiemYData.GetValue(kiemYLevel) : 0;
    public float GetThanThucValue() => thanThucData != null ? thanThucData.GetValue(thanThucLevel) : 0;
    public float GetTuLinhValue() => tuLinhData != null ? tuLinhData.GetValue(tuLinhLevel) : 0;
}

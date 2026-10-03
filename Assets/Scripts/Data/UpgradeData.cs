using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "TuTien/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeId; // VD: LinhLuc, KiemY, ThanThuc, TuLinh
    public string upgradeName; // Tên hiển thị
    public float baseCost; // Giá trị gốc ban đầu
    public float costMultiplier; // Hệ số nhân giá trị
    public float baseValue; // Giá trị buff cơ bản
    public float valueIncrementPerLevel; // Giá trị tăng mỗi cấp

    // Tính tiền yêu cầu để nâng cấp
    public float GetCost(int currentLevel) 
    {
        return baseCost * Mathf.Pow(costMultiplier, currentLevel);
    }

    // Tính giá trị sức mạnh đạt được ở cấp hiện tại
    public float GetValue(int currentLevel)
    {
        return baseValue + (valueIncrementPerLevel * currentLevel);
    }
}

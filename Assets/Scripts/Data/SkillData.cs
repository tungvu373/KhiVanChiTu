using UnityEngine;

public enum SkillType
{
    VanKiemQuyTong, // Trạng thái buff xả 20 kiếm
    AnChuong,       // Sát thương diện rộng đẩy lùi
    AmDuongTran,    // Tăng tốc đánh / Gọi bóng
    LoiPhat         // Sát thương đơn mục tiêu cực mạnh (Đánh Boss)
}

[CreateAssetMenu(fileName = "New Skill", menuName = "TuTien/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public SkillType skillType;
    public float manaCost; // Lượng linh lực yêu cầu
    public float baseCooldown; // Thời gian hồi kỹ năng
    public float baseDamage; // Sát thương cơ bản của kỹ năng
    
    public bool isUnlocked = false; // Đã học hay chưa
    public int level = 1; // Cấp độ kỹ năng (Lv.1 khi mở khóa, tăng dần khi nâng cấp)
    public int requiredStageIndex; // Yêu cầu đạt Cảnh Giới thứ mấy (0,1,2,3...) mới được học
    public int unlockCost; // Tiêu hao điểm Cơ Duyên để mở khóa

    // Chi phí Cơ Duyên nâng cấp tiếp theo:
    // Cấp 1 -> 2: unlockCost
    // Cấp L -> L+1: unlockCost + (level - 1) * 2
    public int GetUpgradeCost()
    {
        int curLvl = Mathf.Max(1, level);
        return unlockCost + (curLvl - 1) * 2;
    }

    // Hệ số sát thương: +30% sức mạnh mỗi cấp độ
    public float GetDamageMultiplier()
    {
        int curLvl = Mathf.Max(1, level);
        return 1f + (curLvl - 1) * 0.30f;
    }

    // Sát thương thực tế sau khi tính Level
    public float GetCurrentDamage()
    {
        return baseDamage * GetDamageMultiplier();
    }
}

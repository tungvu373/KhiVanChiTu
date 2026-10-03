using UnityEngine;

public enum SkillType
{
    VanKiemQuyTong, // Trạng thái buff xả 20 kiếm
    AnChuong,       // Sát thương diện rộng đẩy lùi
    PhanThan,       // Tăng tốc đánh / Gọi bóng
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
    public int requiredStageIndex; // Yêu cầu đạt Cảnh Giới thứ mấy (0,1,2,3...) mới được học
    public int unlockCost; // Tiêu hao điểm Cơ Duyên để mở khóa
}

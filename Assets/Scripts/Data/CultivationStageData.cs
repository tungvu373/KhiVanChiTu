using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "TuTien/Cultivation Stage")]
public class CultivationStageData : ScriptableObject
{
    public string stageName;
    public float requiredTuVi; // Lượng Tu Vi cần để đột phá
    public bool hasTribulationBoss; // Cần đánh Lôi kiếp?
    public float statMultiplierBonus; // Thưởng chỉ số vĩnh viễn
}

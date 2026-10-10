using UnityEngine;

public enum SkillType
{
    VanKiemQuyTong, // Tráº¡ng thÃ¡i buff xáº£ 20 kiáº¿m
    AnChuong,       // SÃ¡t thÆ°Æ¡ng diá»‡n rá»™ng Ä‘áº©y lÃ¹i
    AmDuongTran,       // TÄƒng tá»‘c Ä‘Ã¡nh / Gá»i bÃ³ng
    LoiPhat         // SÃ¡t thÆ°Æ¡ng Ä‘Æ¡n má»¥c tiÃªu cá»±c máº¡nh (ÄÃ¡nh Boss)
}

[CreateAssetMenu(fileName = "New Skill", menuName = "TuTien/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public SkillType skillType;
    public float manaCost; // LÆ°á»£ng linh lá»±c yÃªu cáº§u
    public float baseCooldown; // Thá»i gian há»“i ká»¹ nÄƒng
    public float baseDamage; // SÃ¡t thÆ°Æ¡ng cÆ¡ báº£n cá»§a ká»¹ nÄƒng
    
    public bool isUnlocked = false; // ÄÃ£ há»c hay chÆ°a
    public int requiredStageIndex; // YÃªu cáº§u Ä‘áº¡t Cáº£nh Giá»›i thá»© máº¥y (0,1,2,3...) má»›i Ä‘Æ°á»£c há»c
    public int unlockCost; // TiÃªu hao Ä‘iá»ƒm CÆ¡ DuyÃªn Ä‘á»ƒ má»Ÿ khÃ³a
}


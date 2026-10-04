using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    public float currentLinhLuc;
    public float maxLinhLuc = 100f; 
    public float linhLucRegenRate = 5f; 
    
    public SkillData activeSkill;
    private float currentCooldown;
    public bool isSkillActive = false;

    [Header("Các Kỹ Năng Thường")]
    public SkillData[] normalSkills = new SkillData[3];
    private float[] normalCooldowns = new float[3];

    // UI Elements
    public UnityEngine.UI.Image manaFill;
    public UnityEngine.UI.Text skillText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    private void Start()
    {
        if (activeSkill == null)
        {
            activeSkill = ScriptableObject.CreateInstance<SkillData>();
            activeSkill.skillName = "Vạn Kiếm Quy Tông";
            activeSkill.skillType = SkillType.VanKiemQuyTong;
            activeSkill.manaCost = 100f;
            activeSkill.baseCooldown = 3f;
            activeSkill.baseDamage = 500f;
            activeSkill.isUnlocked = false;
            activeSkill.requiredStageIndex = 15; // Trúc Cơ Sơ Kỳ (Index 15)
            activeSkill.unlockCost = 2; // 2 Cơ Duyên
        }

        // Tự tạo 3 kỹ năng thường nếu chưa có
        if (normalSkills[0] == null)
        {
            normalSkills[0] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[0].skillName = "Ấn Chưởng";
            normalSkills[0].skillType = SkillType.AnChuong;
            normalSkills[0].baseCooldown = 8f;
            normalSkills[0].baseDamage = 150f;
            normalSkills[0].isUnlocked = false;
            normalSkills[0].requiredStageIndex = 18; // Kết Đan Sơ Kỳ
            normalSkills[0].unlockCost = 4;

            normalSkills[1] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[1].skillName = "Lôi Phạt";
            normalSkills[1].skillType = SkillType.LoiPhat;
            normalSkills[1].baseCooldown = 12f;
            normalSkills[1].baseDamage = 800f;
            normalSkills[1].isUnlocked = false;
            normalSkills[1].requiredStageIndex = 21; // Nguyên Anh Sơ Kỳ
            normalSkills[1].unlockCost = 8;

            normalSkills[2] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[2].skillName = "Phân Thân";
            normalSkills[2].skillType = SkillType.PhanThan;
            normalSkills[2].baseCooldown = 20f;
            normalSkills[2].baseDamage = 0f;
            normalSkills[2].isUnlocked = false;
            normalSkills[2].requiredStageIndex = 24; // Hóa Thần Sơ Kỳ
            normalSkills[2].unlockCost = 16;
        }
    }

    private void Update()
    {
        CheckUnlocks();

        if (isSkillActive)
        {
            // THUẬT TOÁN CÂN BẰNG GAME (SOFT-CAP)
            // Nếu chỉ dùng 20f cố định, Ulti sẽ tồn tại vô tận về Late game.
            // Công thức: BaseDrain (20) + Penalty (5% Max Mana).
            // Kết quả: Thời gian Ulti = MaxMana / (20 + MaxMana * 0.05). 
            // Giới hạn tiệm cận (Limit): Dù có Max Mana là 1 Tỷ, Ulti cũng KHÔNG BAO GIỜ vượt quá 20 giây! 
            float drainRate = 20f + (maxLinhLuc * 0.05f); 
            currentLinhLuc -= drainRate * Time.deltaTime;
            
            if (currentLinhLuc <= 0)
            {
                currentLinhLuc = 0;
                isSkillActive = false;
                currentCooldown = activeSkill.baseCooldown; // Bắt đầu tính hồi chiêu
                if (CombatManager.Instance != null) CombatManager.Instance.DeactivateVanKiemQuyTong();
            }
        }
        else
        {
            // 1. Tự động hồi Linh Lực
            if (currentLinhLuc < maxLinhLuc)
            {
                float regenBuff = ShopManager.Instance != null ? ShopManager.Instance.GetManaRegenMultiplier() : 1f;
                currentLinhLuc += linhLucRegenRate * regenBuff * Time.deltaTime;
                currentLinhLuc = Mathf.Min(currentLinhLuc, maxLinhLuc);
            }

            // 2. Đếm ngược Cooldown
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
            }

            // 3. Tự động kích hoạt khi Full Mana và hết CD (nếu đã học)
            if (activeSkill != null && activeSkill.isUnlocked && currentCooldown <= 0 && currentLinhLuc >= maxLinhLuc)
            {
                ActivateSkill();
            }
        }
        
        UpdateNormalSkills();
        UpdateUI();
    }

    private void CheckUnlocks()
    {
        // Tính năng tự động unlock đã bỏ, giờ phải mua bằng Cơ Duyên.
        // NHƯNG thêm CHEAT phím 'U' để mở khóa toàn bộ Skill cho việc test:
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
        {
            if (activeSkill != null) activeSkill.isUnlocked = true;
            for (int i = 0; i < 3; i++)
            {
                if (normalSkills[i] != null) normalSkills[i].isUnlocked = true;
            }
            Debug.Log("[CHEAT] Đã mở khóa toàn bộ Kỹ Năng!");
        }
    }

    private void UpdateNormalSkills()
    {
        for (int i = 0; i < 3; i++)
        {
            if (normalSkills[i] == null || !normalSkills[i].isUnlocked) continue;

            if (normalCooldowns[i] > 0)
            {
                normalCooldowns[i] -= Time.deltaTime;
            }
            else
            {
                // Thêm cơ chế tiêu hao Linh Lực (mỗi kỹ năng tiêu hao một lượng cố định)
                float manaRequired = 10f * (i + 1); // Ấn chưởng: 10, Lôi Phạt: 20, Phân Thân: 30
                if (currentLinhLuc >= manaRequired)
                {
                    currentLinhLuc -= manaRequired;
                    ExecuteNormalSkill(normalSkills[i]);
                    normalCooldowns[i] = normalSkills[i].baseCooldown;
                }
            }
        }
    }

    public float GetActiveSkillCooldown() => currentCooldown;
    public float GetNormalSkillCooldown(int index) => normalCooldowns[index];

    private void ExecuteNormalSkill(SkillData skill)
    {
        float damage = skill.baseDamage * (CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f);
        
        switch (skill.skillType)
        {
            case SkillType.AnChuong:
                if (CombatManager.Instance != null) CombatManager.Instance.CastAnChuong(damage);
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: Ấn Chưởng", new Color(1f, 0.5f, 0f));
                break;
            case SkillType.LoiPhat:
                if (CombatManager.Instance != null) CombatManager.Instance.CastLoiPhat(damage);
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: Lôi Phạt", new Color(0.8f, 0f, 1f));
                break;
            case SkillType.PhanThan:
                if (CombatManager.Instance != null) CombatManager.Instance.CastPhanThan();
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: Phân Thân", Color.cyan);
                break;
        }
    }

    private void ActivateSkill()
    {
        isSkillActive = true;
        
        float finalDamage = activeSkill.baseDamage;
        if (CultivationManager.Instance != null)
        {
            finalDamage *= CultivationManager.Instance.permanentStatMultiplier;
        }
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ActivateVanKiemQuyTong(finalDamage);
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: VẠN KIẾM QUY TÔNG!", Color.yellow);
        }
    }
    
    private void UpdateUI()
    {
        if (manaFill != null)
        {
            manaFill.fillAmount = currentLinhLuc / maxLinhLuc;
        }
        if (skillText != null && activeSkill != null)
        {
            if (isSkillActive)
                skillText.text = "VẠN KIẾM QUY TÔNG!";
            else if (currentCooldown > 0)
                skillText.text = $"Hồi chiêu: {currentCooldown:F1}s";
            else
                skillText.text = $"Mana: {Mathf.FloorToInt(currentLinhLuc)}/{maxLinhLuc}";
        }
    }
}

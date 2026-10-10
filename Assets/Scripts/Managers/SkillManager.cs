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

    [Header("CÃ¡c Ká»¹ NÄƒng ThÆ°á»ng")]
    public SkillData[] normalSkills = new SkillData[3];
    private float[] normalCooldowns = new float[3];

    // UI Elements
    public UnityEngine.UI.Image manaFill;
    public UnityEngine.UI.Text skillText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }
    
    private void Start()
    {
        if (activeSkill == null)
        {
            activeSkill = ScriptableObject.CreateInstance<SkillData>();
            activeSkill.skillName = "Váº¡n Kiáº¿m Quy TÃ´ng";
            activeSkill.skillType = SkillType.VanKiemQuyTong;
            activeSkill.manaCost = 100f;
            activeSkill.baseCooldown = 3f;
            activeSkill.baseDamage = 500f;
            activeSkill.isUnlocked = false;
            activeSkill.requiredStageIndex = 15; // TrÃºc CÆ¡ SÆ¡ Ká»³ (Index 15)
            activeSkill.unlockCost = 2; // 2 CÆ¡ DuyÃªn
        }

        // Tá»± táº¡o 3 ká»¹ nÄƒng thÆ°á»ng náº¿u chÆ°a cÃ³
        if (normalSkills[0] == null)
        {
            normalSkills[0] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[0].skillName = "áº¤n ChÆ°á»Ÿng";
            normalSkills[0].skillType = SkillType.AnChuong;
            normalSkills[0].baseCooldown = 8f;
            normalSkills[0].baseDamage = 150f;
            normalSkills[0].isUnlocked = false;
            normalSkills[0].requiredStageIndex = 18; // Káº¿t Äan SÆ¡ Ká»³
            normalSkills[0].unlockCost = 4;

            normalSkills[1] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[1].skillName = "Lôi Phạt";
            normalSkills[1].skillType = SkillType.LoiPhat;
            normalSkills[1].baseCooldown = 12f;
            normalSkills[1].baseDamage = 800f;
            normalSkills[1].isUnlocked = false;
            normalSkills[1].requiredStageIndex = 21; // NguyÃªn Anh SÆ¡ Ká»³
            normalSkills[1].unlockCost = 8;

            normalSkills[2] = ScriptableObject.CreateInstance<SkillData>();
            normalSkills[2].skillName = "Âm Dương Trận";
            normalSkills[2].skillType = SkillType.AmDuongTran;
            normalSkills[2].baseCooldown = 20f;
            normalSkills[2].baseDamage = 0f;
            normalSkills[2].isUnlocked = false;
            normalSkills[2].isUnlocked = false;
            normalSkills[2].requiredStageIndex = 24; // HÃ³a Tháº§n SÆ¡ Ká»³
            normalSkills[2].unlockCost = 16;
        }
    }

    private void OnDestroy()
    {
    }

    private void Update()
    {
        CheckUnlocks();

        if (isSkillActive)
        {
            // THUáº¬T TOÃN CÃ‚N Báº°NG GAME (SOFT-CAP)
            // Náº¿u chá»‰ dÃ¹ng 20f cá»‘ Ä‘á»‹nh, Ulti sáº½ tá»“n táº¡i vÃ´ táº­n vá» Late game.
            // CÃ´ng thá»©c: BaseDrain (20) + Penalty (5% Max Mana).
            // Káº¿t quáº£: Thá»i gian Ulti = MaxMana / (20 + MaxMana * 0.05). 
            // Giá»›i háº¡n tiá»‡m cáº­n (Limit): DÃ¹ cÃ³ Max Mana lÃ  1 Tá»·, Ulti cÅ©ng KHÃ”NG BAO GIá»œ vÆ°á»£t quÃ¡ 20 giÃ¢y! 
            float drainRate = 20f + (maxLinhLuc * 0.05f); 
            currentLinhLuc -= drainRate * Time.deltaTime;
            
            if (currentLinhLuc <= 0)
            {
                currentLinhLuc = 0;
                isSkillActive = false;
                currentCooldown = activeSkill.baseCooldown; // Báº¯t Ä‘áº§u tÃ­nh há»“i chiÃªu
                if (CombatManager.Instance != null) CombatManager.Instance.DeactivateVanKiemQuyTong();
            }
        }
        else
        {
            // 1. Tá»± Ä‘á»™ng há»“i Linh Lá»±c
            if (currentLinhLuc < maxLinhLuc)
            {
                float regenBuff = ShopManager.Instance != null ? ShopManager.Instance.GetManaRegenMultiplier() : 1f;
                currentLinhLuc += linhLucRegenRate * regenBuff * Time.deltaTime;
                currentLinhLuc = Mathf.Min(currentLinhLuc, maxLinhLuc);
            }

            // 2. Äáº¿m ngÆ°á»£c Cooldown
            if (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
            }

            // 3. Tá»± Ä‘á»™ng kÃ­ch hoáº¡t khi Full Mana vÃ  háº¿t CD (náº¿u Ä‘Ã£ há»c)
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
#if UNITY_EDITOR
        // TÃ­nh nÄƒng tá»± Ä‘á»™ng unlock Ä‘Ã£ bá», giá» pháº£i mua báº±ng CÆ¡ DuyÃªn.
        // NHÆ¯NG thÃªm CHEAT phÃ­m 'U' Ä‘á»ƒ má»Ÿ khÃ³a toÃ n bá»™ Skill cho viá»‡c test:
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
        {
            if (activeSkill != null) activeSkill.isUnlocked = true;
            for (int i = 0; i < 3; i++)
            {
                if (normalSkills[i] != null) normalSkills[i].isUnlocked = true;
            }
            Debug.Log("[CHEAT] ÄÃ£ má»Ÿ khÃ³a toÃ n bá»™ Ká»¹ NÄƒng!");
        }
#endif
    }

    private float normalSkillGlobalCooldown = 0f;

    private void UpdateNormalSkills()
    {
        // Không cho Normal Skills tốn Mana khi Ulti đang active

        if (isSkillActive) return;

        // Giảm hồi chiêu cho tất cả các kỹ năng trước
        for (int i = 0; i < 3; i++)
        {
            if (normalCooldowns[i] > 0)
            {
                normalCooldowns[i] -= Time.deltaTime;
            }
        }

        // Nếu đang trong thời gian chờ giữa 2 chiêu (Global Cooldown) thì không ra chiêu mới
        if (normalSkillGlobalCooldown > 0)
        {
            normalSkillGlobalCooldown -= Time.deltaTime;
            return;
        }

        // Tìm kỹ năng có thể thi triển
        for (int i = 0; i < 3; i++)
        {
            if (normalSkills[i] == null || !normalSkills[i].isUnlocked) continue;

            if (normalCooldowns[i] <= 0)
            {
                float manaRequired = 10f * (i + 1); // Ấn chưởng: 10, Lôi Phạt: 20, Âm Dương Trận: 30
                if (currentLinhLuc >= manaRequired)
                {
                    currentLinhLuc -= manaRequired;
                    ExecuteNormalSkill(normalSkills[i]);
                    normalCooldowns[i] = normalSkills[i].baseCooldown;
                    normalSkillGlobalCooldown = 1.5f; // Chờ 1.5s rồi mới ra chiêu tiếp theo để tránh spam
                    break; // Chỉ dùng 1 chiêu tại 1 thời điểm
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
            case SkillType.AmDuongTran:
                if (CombatManager.Instance != null) CombatManager.Instance.CastAmDuongTran();
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Thi triển: Âm Dương Trận", Color.cyan);
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
            float fillRatio = Mathf.Clamp01(currentLinhLuc / maxLinhLuc);
            RectTransform fillRt = manaFill.GetComponent<RectTransform>();
            fillRt.anchorMax = new Vector2(fillRatio, 1f);
        }
        if (skillText != null && activeSkill != null)
        {
            if (isSkillActive)
                skillText.text = "VẠN KIẾM QUY TÔNG!";
            else if (currentCooldown > 0)
                skillText.text = $"Hồi chiêu: {currentCooldown:F1}s";
            else
                skillText.text = $"MP: {Mathf.FloorToInt(currentLinhLuc)}/{Mathf.FloorToInt(maxLinhLuc)}";
        }
    }

    // â”€â”€ SAVE / LOAD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.skillActiveUnlocked = activeSkill != null && activeSkill.isUnlocked;
        slot.skillNormal0Unlocked = normalSkills[0] != null && normalSkills[0].isUnlocked;
        slot.skillNormal1Unlocked = normalSkills[1] != null && normalSkills[1].isUnlocked;
        slot.skillNormal2Unlocked = normalSkills[2] != null && normalSkills[2].isUnlocked;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (activeSkill != null) activeSkill.isUnlocked = slot.skillActiveUnlocked;
        if (normalSkills[0] != null) normalSkills[0].isUnlocked = slot.skillNormal0Unlocked;
        if (normalSkills[1] != null) normalSkills[1].isUnlocked = slot.skillNormal1Unlocked;
        if (normalSkills[2] != null) normalSkills[2].isUnlocked = slot.skillNormal2Unlocked;
    }
}



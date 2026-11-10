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
            createdActiveInstance = true;
            activeSkill.skillName = "Vạn Kiếm Quy Tông";
            activeSkill.skillType = SkillType.VanKiemQuyTong;
            activeSkill.manaCost = 100f;
            activeSkill.baseCooldown = 30f; // Cooldown 30s cân bằng
            activeSkill.baseDamage = 200f;
            activeSkill.isUnlocked = false;
            activeSkill.requiredStageIndex = 7;
            activeSkill.unlockCost = 2;
        }
        else
        {
            activeSkill.baseDamage = 200f;
            activeSkill.baseCooldown = 30f;
            activeSkill.manaCost = 100f;
            activeSkill.skillType = SkillType.VanKiemQuyTong;
        }

        // Tự tạo 3 kỹ năng thường nếu chưa có
        if (normalSkills[0] == null)
        {
            normalSkills[0] = ScriptableObject.CreateInstance<SkillData>();
            createdNormalInstances[0] = true;
            normalSkills[0].skillName = "Ấn Chưởng";
            normalSkills[0].skillType = SkillType.AnChuong;
            normalSkills[0].baseCooldown = 22f; // Tăng CD lên 22s
            normalSkills[0].baseDamage = 100000f; // Thần thông Đại Thừa: 100,000 sát thương cơ bản
            normalSkills[0].manaCost = 40f;
            normalSkills[0].isUnlocked = false;
            normalSkills[0].unlockCost = 10;

            normalSkills[1] = ScriptableObject.CreateInstance<SkillData>();
            createdNormalInstances[1] = true;
            normalSkills[1].skillName = "Lôi Phạt";
            normalSkills[1].skillType = SkillType.LoiPhat;
            normalSkills[1].baseCooldown = 25f; // Tăng CD lên 25s
            normalSkills[1].baseDamage = 5000f; // Kỹ năng Nguyên Anh
            normalSkills[1].manaCost = 25f;
            normalSkills[1].isUnlocked = false;
            normalSkills[1].unlockCost = 6;

            normalSkills[2] = ScriptableObject.CreateInstance<SkillData>();
            createdNormalInstances[2] = true;
            normalSkills[2].skillName = "Âm Dương Trận";
            normalSkills[2].skillType = SkillType.AmDuongTran;
            normalSkills[2].baseCooldown = 40f; // Tăng CD lên 40s
            normalSkills[2].baseDamage = 0f;
            normalSkills[2].manaCost = 30f;
            normalSkills[2].isUnlocked = false;
            normalSkills[2].unlockCost = 8;
        }
        else
        {
            if (normalSkills[0] != null)
            {
                normalSkills[0].baseDamage = 100000f;
                normalSkills[0].baseCooldown = 22f;
                normalSkills[0].manaCost = 40f;
            }
            if (normalSkills[1] != null)
            {
                normalSkills[1].baseDamage = 5000f;
                normalSkills[1].baseCooldown = 25f;
                normalSkills[1].manaCost = 25f;
            }
            if (normalSkills[2] != null)
            {
                normalSkills[2].skillName = "Âm Dương Trận";
                normalSkills[2].skillType = SkillType.AmDuongTran;
                normalSkills[2].baseCooldown = 40f;
                normalSkills[2].manaCost = 30f;
                normalSkills[2].baseDamage = 0f;
            }
        }
    }

    private bool createdActiveInstance = false;
    private bool[] createdNormalInstances = new bool[3];

    private void OnDestroy()
    {
        if (createdActiveInstance && activeSkill != null)
        {
            Destroy(activeSkill);
        }
        for (int i = 0; i < normalSkills.Length; i++)
        {
            if (createdNormalInstances != null && i < createdNormalInstances.Length && createdNormalInstances[i] && normalSkills[i] != null)
            {
                Destroy(normalSkills[i]);
            }
        }
    }

    private void DummyUnusedMethod()
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

    public float GetSkillManaCost(int index)
    {
        switch (index)
        {
            case 0: return 40f; // Ấn Chưởng (Đại Thừa)
            case 1: return 25f; // Lôi Phạt (Nguyên Anh)
            case 2: return 30f; // Âm Dương Trận (4 Trận Pháp)
            default: return 20f;
        }
    }

    public int GetHighestSwordLevel()
    {
        int maxLvl = 1;
        if (AchievementManager.Instance != null && AchievementManager.Instance.State != null)
        {
            maxLvl = Mathf.Max(maxLvl, AchievementManager.Instance.State.maxSwordLevelEver);
        }
        if (MergeManager.Instance != null)
        {
            if (MergeManager.Instance.equipPanel != null)
            {
                foreach (Transform slot in MergeManager.Instance.equipPanel)
                {
                    if (slot == null) continue;
                    var item = slot.GetComponentInChildren<MergeItem>();
                    if (item != null) maxLvl = Mathf.Max(maxLvl, item.level);
                }
            }
            if (MergeManager.Instance.inventoryPages != null)
            {
                foreach (Transform page in MergeManager.Instance.inventoryPages)
                {
                    if (page == null) continue;
                    foreach (Transform slot in page)
                    {
                        if (slot == null) continue;
                        var item = slot.GetComponentInChildren<MergeItem>();
                        if (item != null) maxLvl = Mathf.Max(maxLvl, item.level);
                    }
                }
            }
        }
        return maxLvl;
    }

    public void UnlockAllSkillsForTesting()
    {
        if (activeSkill != null) activeSkill.isUnlocked = true;
        if (normalSkills != null)
        {
            foreach (var s in normalSkills)
            {
                if (s != null) s.isUnlocked = true;
            }
        }
    }

    public int GetLearnedFormationCount()
    {
        if (FormationService.Instance == null || FormationService.Instance.State == null || FormationService.Instance.State.levels == null) return 0;
        int count = 0;
        for (int i = 0; i < FormationService.Instance.State.levels.Length; i++)
        {
            if (FormationService.Instance.State.levels[i] > 0) count++;
        }
        return count;
    }

    public bool CastNormalSkillManual(int index)
    {
        if (index < 0 || index >= normalSkills.Length) return false;
        var skill = normalSkills[index];
        if (skill == null || !skill.isUnlocked)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Kỹ năng chưa mở khóa!", Color.gray);
            return false;
        }
        if (isSkillActive)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log("Đang trong trạng thái Vạn Kiếm Quy Tông!", Color.gray);
            return false;
        }
        if (normalCooldowns[index] > 0)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"{skill.skillName} đang hồi chiêu ({normalCooldowns[index]:F1}s)!", Color.gray);
            return false;
        }
        float manaRequired = GetSkillManaCost(index);
        if (currentLinhLuc < manaRequired)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Linh Lực! Cần {manaRequired:F0} MP.", Color.cyan);
            return false;
        }

        currentLinhLuc -= manaRequired;
        ExecuteNormalSkill(skill);
        normalCooldowns[index] = skill.baseCooldown;
        normalSkillGlobalCooldown = 1.0f;
        return true;
    }

    private void CheckUnlocks()
    {
        var realm = CultivationManager.Instance != null ? CultivationManager.Instance.CurrentRealm : RealmId.LuyenKhi;

        // 1. Vạn Kiếm Quy Tông: Mở khóa khi sở hữu kiếm level >= 7
        if (activeSkill != null && !activeSkill.isUnlocked)
        {
            int maxSword = GetHighestSwordLevel();
            if (maxSword >= 7)
            {
                activeSkill.isUnlocked = true;
                if (GameLogger.Instance != null) GameLogger.Instance.Log("⚔️ Sở hữu Kiếm Cấp 7: Lĩnh ngộ Tuyệt Kỹ VẠN KIẾM QUY TÔNG!", Color.yellow);
            }
        }

        // 2. Lôi Phạt: Mở khóa khi độ kiếp thành công lên Nguyên Anh (Realm >= NguyenAnh)
        if (normalSkills != null && normalSkills.Length > 1 && normalSkills[1] != null && !normalSkills[1].isUnlocked)
        {
            if (realm >= RealmId.NguyenAnh)
            {
                normalSkills[1].isUnlocked = true;
                if (GameLogger.Instance != null) GameLogger.Instance.Log("⚡ Độ kiếp Nguyên Anh thành công: Lĩnh ngộ tuyệt kỹ LÔI PHẠT!", new Color(0.8f, 0.4f, 1f));
            }
        }

        // 3. Âm Dương Trận: Mở khóa khi học được 4 loại trận khác nhau
        if (normalSkills != null && normalSkills.Length > 2 && normalSkills[2] != null && !normalSkills[2].isUnlocked)
        {
            int learnedFormations = GetLearnedFormationCount();
            if (learnedFormations >= 4)
            {
                normalSkills[2].isUnlocked = true;
                if (GameLogger.Instance != null) GameLogger.Instance.Log("☯️ Đã lĩnh hội 4 loại Trận Pháp: Lĩnh ngộ tuyệt kỹ ÂM DƯƠNG TRẬN!", Color.cyan);
            }
        }

        // 4. Ấn Chưởng: Mở khóa khi đạt cảnh giới Đại Thừa (Realm >= DaiThua)
        if (normalSkills != null && normalSkills.Length > 0 && normalSkills[0] != null && !normalSkills[0].isUnlocked)
        {
            if (realm >= RealmId.DaiThua)
            {
                normalSkills[0].isUnlocked = true;
                if (GameLogger.Instance != null) GameLogger.Instance.Log("🖐️ Đạt cảnh giới Đại Thừa Chí Tôn: Khai thông đại thần thông ẤN CHƯỞNG!", new Color(1f, 0.6f, 0.1f));
            }
        }

#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
        {
            UnlockAllSkillsForTesting();
            Debug.Log("[CHEAT] Đã mở khóa toàn bộ Kỹ Năng!");
        }
#endif
    }

    private float normalSkillGlobalCooldown = 0f;

    private void UpdateNormalSkills()
    {
        if (isSkillActive) return;

        for (int i = 0; i < 3; i++)
        {
            if (normalCooldowns[i] > 0)
            {
                normalCooldowns[i] -= Time.deltaTime;
            }
        }

        if (normalSkillGlobalCooldown > 0)
        {
            normalSkillGlobalCooldown -= Time.deltaTime;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            if (normalSkills[i] == null || !normalSkills[i].isUnlocked) continue;

            if (normalCooldowns[i] <= 0)
            {
                float manaRequired = GetSkillManaCost(i);
                if (currentLinhLuc >= manaRequired)
                {
                    currentLinhLuc -= manaRequired;
                    ExecuteNormalSkill(normalSkills[i]);
                    normalCooldowns[i] = normalSkills[i].baseCooldown;
                    normalSkillGlobalCooldown = 1.5f;
                    break;
                }
            }
        }
    }

    public float GetActiveSkillCooldown() => currentCooldown;
    public float GetNormalSkillCooldown(int index) => normalCooldowns[index];

    public float CalculateTotalSkillDamage(SkillData skill)
    {
        if (skill == null) return 0f;
        float baseDmg = skill.GetCurrentDamage();
        if (baseDmg <= 0) return 0f;

        float kiemY = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetKiemYValue() : 0f;
        
        // Vạn Kiếm Quy Tông: Mỗi thanh kiếm nhận +100% Kiếm Ý
        // Ấn Chưởng (Đại Thừa): Nhận +1500% Kiếm Ý (x15 uy lực Kiếm Ý)
        // Lôi Phạt (Nguyên Anh): Nhận +300% Kiếm Ý
        float totalBase;
        if (skill.skillType == SkillType.VanKiemQuyTong)
            totalBase = baseDmg + kiemY;
        else if (skill.skillType == SkillType.AnChuong)
            totalBase = baseDmg + (kiemY * 15f);
        else
            totalBase = baseDmg + (kiemY * 3f);

        float multiplier = CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f;
        if (ShopManager.Instance != null) multiplier *= ShopManager.Instance.GetDamageMultiplier();
        if (EconomyManager.Instance != null) multiplier *= (1f + EconomyManager.Instance.currentKiemY * 0.1f);
        if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive)
            multiplier *= CombatManager.Instance.GetAmDuongTranDamageMultiplier();

        return totalBase * multiplier;
    }

    private void ExecuteNormalSkill(SkillData skill)
    {
        float damage = CalculateTotalSkillDamage(skill);
        
        switch (skill.skillType)
        {
            case SkillType.AnChuong:
                if (CombatManager.Instance != null) CombatManager.Instance.CastAnChuong(damage);
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Ấn Chưởng (Lv.{skill.level})", new Color(1f, 0.5f, 0f));
                break;
            case SkillType.LoiPhat:
                if (CombatManager.Instance != null) CombatManager.Instance.CastLoiPhat(damage);
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Lôi Phạt (Lv.{skill.level})", new Color(0.8f, 0f, 1f));
                break;
            case SkillType.AmDuongTran:
                if (CombatManager.Instance != null) CombatManager.Instance.CastAmDuongTran(skill.level);
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Thi triển: Âm Dương Trận (Lv.{skill.level})", Color.cyan);
                break;
        }
    }

    public void CastActiveSkill()
    {
        if (activeSkill == null || !activeSkill.isUnlocked) return;
        if (isSkillActive) return;
        if (currentCooldown > 0)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"Vạn Kiếm Quy Tông đang hồi chiêu ({currentCooldown:F1}s)!", Color.gray);
            return;
        }
        if (currentLinhLuc < maxLinhLuc)
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"Chưa đủ Linh Lực để kích hoạt Vạn Kiếm Quy Tông! ({Mathf.FloorToInt(currentLinhLuc)}/{Mathf.FloorToInt(maxLinhLuc)})", Color.cyan);
            return;
        }

        ActivateSkill();
    }

    public void ActivateSkill()
    {
        isSkillActive = true;
        
        float finalDamage = CalculateTotalSkillDamage(activeSkill);
        
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
        slot.skillActiveLevel = activeSkill != null ? activeSkill.level : 1;
        slot.skillNormal0Unlocked = normalSkills[0] != null && normalSkills[0].isUnlocked;
        slot.skillNormal0Level = normalSkills[0] != null ? normalSkills[0].level : 1;
        slot.skillNormal1Unlocked = normalSkills[1] != null && normalSkills[1].isUnlocked;
        slot.skillNormal1Level = normalSkills[1] != null ? normalSkills[1].level : 1;
        slot.skillNormal2Unlocked = normalSkills[2] != null && normalSkills[2].isUnlocked;
        slot.skillNormal2Level = normalSkills[2] != null ? normalSkills[2].level : 1;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (activeSkill != null) 
        {
            activeSkill.isUnlocked = slot.skillActiveUnlocked;
            activeSkill.level = Mathf.Max(1, slot.skillActiveLevel);
        }
        if (normalSkills[0] != null) 
        {
            normalSkills[0].isUnlocked = slot.skillNormal0Unlocked;
            normalSkills[0].level = Mathf.Max(1, slot.skillNormal0Level);
        }
        if (normalSkills[1] != null) 
        {
            normalSkills[1].isUnlocked = slot.skillNormal1Unlocked;
            normalSkills[1].level = Mathf.Max(1, slot.skillNormal1Level);
        }
        if (normalSkills[2] != null) 
        {
            normalSkills[2].isUnlocked = slot.skillNormal2Unlocked;
            normalSkills[2].level = Mathf.Max(1, slot.skillNormal2Level);
        }
    }
}



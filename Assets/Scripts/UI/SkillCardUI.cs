using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillIndex; // 0: Vạn Kiếm, 1: Ấn Chưởng, 2: Lôi Phạt, 3: Phân Thân
    public Text titleText;
    public Text condText;
    public Button unlockButton;
    public Text unlockBtnText;

    private void Start()
    {
        // Không ghi đè RectTransform, Font, Căn lề trong code để bạn tự do kéo thả, chỉnh font, vị trí trong Inspector!
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }
    }

    private string GetShortStageName(int stageIndex)
    {
        if (CultivationManager.Instance == null || CultivationManager.Instance.allStages == null || stageIndex >= CultivationManager.Instance.allStages.Length)
        {
            if (stageIndex >= 24) return "Hóa Thần";
            if (stageIndex >= 21) return "Nguyên Anh";
            if (stageIndex >= 18) return "Kết Đan";
            if (stageIndex >= 15) return "Trúc Cơ";
            return "Luyện Khí";
        }

        string fullName = CultivationManager.Instance.allStages[stageIndex].stageName;
        if (fullName.Contains("Trúc Cơ")) return "Trúc Cơ";
        if (fullName.Contains("Kết Đan") || fullName.Contains("Kim Đan")) return "Kết Đan";
        if (fullName.Contains("Nguyên Anh")) return "Nguyên Anh";
        if (fullName.Contains("Hóa Thần")) return "Hóa Thần";
        if (fullName.Contains("Luyện Khí")) return "Luyện Khí";
        
        return fullName;
    }

    private void Update()
    {
        if (SkillManager.Instance == null || EconomyManager.Instance == null || CultivationManager.Instance == null) return;
        
        SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
        if (skill == null) return;

        string shortStage = GetShortStageName(skill.requiredStageIndex);
        int currentStage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        bool reachStage = currentStage >= skill.requiredStageIndex;
        string stageColor = reachStage ? "#88FF88" : "#FF6666";
        int currentCoDuyen = EconomyManager.Instance.currentCoDuyen;

        string reqText = skill.isUnlocked 
            ? $"Yêu cầu: <color=#88FF88>{shortStage}</color> | <color=#FFD700>Lv.{skill.level}</color>" 
            : $"Yêu cầu: <color={stageColor}>{shortStage}</color>";

        // 1. Cập nhật condText nếu có
        if (condText != null)
        {
            condText.text = reqText;
        }

        // 2. Cập nhật titleText: Tên kỹ năng (kèm Level nếu đã học), nếu condText bị ẩn thì gộp dòng yêu cầu
        if (titleText != null)
        {
            string displayName = skill.isUnlocked ? $"{skill.skillName} (Lv.{skill.level})" : skill.skillName;
            if (condText == null || !condText.gameObject.activeInHierarchy)
            {
                titleText.text = $"{displayName}\n{reqText}";
            }
            else
            {
                titleText.text = displayName;
            }
        }

        // 3. Cập nhật nút bấm: Chưa học = Mở khóa (CD), Đã học = Nâng cấp (CD)
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(true);

            if (!skill.isUnlocked)
            {
                int cost = skill.unlockCost;
                bool canUnlock = reachStage && (currentCoDuyen >= cost);
                if (unlockBtnText != null) unlockBtnText.text = $"Mở khóa ({cost} CD)";
                unlockButton.interactable = canUnlock;
            }
            else
            {
                int upCost = skill.GetUpgradeCost();
                bool canUpgrade = currentCoDuyen >= upCost;
                if (unlockBtnText != null) unlockBtnText.text = $"Nâng cấp ({upCost} CD)";
                unlockButton.interactable = canUpgrade;
            }
        }
    }

    public void OnUnlockClicked()
    {
        if (SkillManager.Instance == null || EconomyManager.Instance == null) return;
        
        SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
        if (skill == null) return;

        int currentCoDuyen = EconomyManager.Instance.currentCoDuyen;

        // TRƯỜNG HỢP 1: CHƯA HỌC -> MỞ KHÓA
        if (!skill.isUnlocked)
        {
            // Kiểm tra Cảnh Giới
            if (CultivationManager.Instance != null && CultivationManager.Instance.currentStageIndex < skill.requiredStageIndex)
            {
                string shortStage = GetShortStageName(skill.requiredStageIndex);
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Chưa đạt cảnh giới yêu cầu: {shortStage}!", Color.red);
                return;
            }

            // Tiêu hao Cơ Duyên mở khóa
            if (EconomyManager.Instance.SpendCoDuyen(skill.unlockCost))
            {
                skill.isUnlocked = true;
                skill.level = 1;
                Debug.Log($"[Skill] Đã dùng {skill.unlockCost} Cơ Duyên để mở khóa {skill.skillName}!");
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Đã lĩnh ngộ thành công: {skill.skillName} (Lv.1)!", Color.green);
            }
            else
            {
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Cơ Duyên để mở khóa ({currentCoDuyen}/{skill.unlockCost} CD)!", Color.yellow);
            }
            return;
        }

        // TRƯỜNG HỢP 2: ĐÃ HỌC -> NÂNG CẤP TĂNG SỨC MẠNH
        int upgradeCost = skill.GetUpgradeCost();
        if (EconomyManager.Instance.SpendCoDuyen(upgradeCost))
        {
            skill.level++;
            float currentDmg = skill.GetCurrentDamage();
            Debug.Log($"[Skill] Đã nâng cấp {skill.skillName} lên Lv.{skill.level} (Sát thương: {currentDmg:F0})");
            if (GameLogger.Instance != null) 
            {
                GameLogger.Instance.Log($"Nâng cấp thành công: {skill.skillName} lên Cấp {skill.level}! (+30% Sát thương)", Color.green);
            }
        }
        else
        {
            if (GameLogger.Instance != null) GameLogger.Instance.Log($"Không đủ Cơ Duyên để nâng cấp ({currentCoDuyen}/{upgradeCost} CD)!", Color.yellow);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && SkillManager.Instance != null)
        {
            SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
            if (skill != null)
            {
                string shortStage = GetShortStageName(skill.requiredStageIndex);
                int currentStage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
                bool reachStage = currentStage >= skill.requiredStageIndex;
                int currentCoDuyen = EconomyManager.Instance != null ? EconomyManager.Instance.currentCoDuyen : 0;

                string stageColor = reachStage ? "#88FF88" : "#FF6666";

                float currentDmg = skill.GetCurrentDamage();
                string titleStr = skill.isUnlocked 
                    ? $"{skill.skillName} (Cấp {skill.level})" 
                    : skill.skillName;

                string info = $"<size=15><b><color=#00FFFF>{titleStr}</color></b></size>\n\n";
                if (skill.baseDamage > 0)
                {
                    info += $"<color=#FFD700>Sát thương:</color> {currentDmg:F0}";
                    if (skill.level > 1) info += $" <color=#88FF88>(+{Mathf.RoundToInt((skill.level - 1) * 30)}%)</color>";
                    info += "\n";
                }
                else
                {
                    info += $"<color=#FFD700>Hiệu ứng:</color> Tốc độ Phi Kiếm x2 (Lv.{skill.level})\n";
                }

                info += $"<color=#FFD700>Hồi chiêu:</color> {skill.baseCooldown:F1}s\n";
                info += $"<color=#FFD700>Tiêu hao:</color> {skill.manaCost:F0} Linh Lực\n";
                info += $"<color=#FFD700>Yêu cầu:</color> <color={stageColor}>{shortStage}</color>\n";

                if (!skill.isUnlocked)
                {
                    string cdColor = (currentCoDuyen >= skill.unlockCost) ? "#88FF88" : "#FFB84D";
                    info += $"<color=#FFD700>Cơ duyên mở khóa:</color> <color={cdColor}>{currentCoDuyen}/{skill.unlockCost} CD</color>\n\n";
                    if (reachStage && currentCoDuyen >= skill.unlockCost) 
                        info += "<color=#88FF88><i>[Bấm nút để Mở Khóa!]</i></color>";
                    else 
                        info += "<color=#FF8888><i>[Chưa đủ điều kiện mở khóa]</i></color>";
                }
                else
                {
                    int upCost = skill.GetUpgradeCost();
                    string cdColor = (currentCoDuyen >= upCost) ? "#88FF88" : "#FFB84D";
                    info += $"<color=#FFD700>Cơ duyên nâng cấp (Lv.{skill.level + 1}):</color> <color={cdColor}>{currentCoDuyen}/{upCost} CD</color>\n\n";
                    if (currentCoDuyen >= upCost)
                        info += "<color=#88FF88><i>[Bấm nút để Nâng Cấp (+30% Sức mạnh)!]</i></color>";
                    else
                        info += "<color=#FFB84D><i>[Cần thêm Cơ Duyên để nâng cấp]</i></color>";
                }

                TooltipManager.Instance.ShowTooltip(info);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}

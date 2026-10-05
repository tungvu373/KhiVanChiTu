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
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }
    }

    private void Update()
    {
        if (SkillManager.Instance == null || EconomyManager.Instance == null || CultivationManager.Instance == null) return;
        
        SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
        if (skill == null) return;

        if (skill.isUnlocked)
        {
            float cd = (skillIndex == 0) ? SkillManager.Instance.GetActiveSkillCooldown() : SkillManager.Instance.GetNormalSkillCooldown(skillIndex - 1);
            if (cd > 0)
            {
                condText.text = $"Hồi chiêu: {cd:F1}s";
                condText.color = Color.yellow;
            }
            else
            {
                condText.text = "Sẵn sàng (Auto)";
                condText.color = Color.green;
            }
            unlockButton.gameObject.SetActive(false);
        }
        else
        {
            int currentCoDuyen = EconomyManager.Instance.currentCoDuyen;
            int cost = skill.unlockCost;
            int currentStage = CultivationManager.Instance.currentStageIndex;
            string stageName = "Không xác định";
            if (skill.requiredStageIndex < CultivationManager.Instance.allStages.Length)
            {
                stageName = CultivationManager.Instance.allStages[skill.requiredStageIndex].stageName;
            }

            condText.text = $"Yêu cầu:\n{stageName}\nCơ duyên: {currentCoDuyen}/{cost}";
            condText.color = Color.red;

            unlockButton.gameObject.SetActive(true);
            unlockBtnText.text = $"Học ({cost} CD)";

            if (currentStage >= skill.requiredStageIndex && currentCoDuyen >= cost)
            {
                unlockButton.interactable = true;
            }
            else
            {
                unlockButton.interactable = false;
            }
        }
    }

    public void OnUnlockClicked()
    {
        if (SkillManager.Instance == null || EconomyManager.Instance == null) return;
        
        SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
        if (skill == null || skill.isUnlocked) return;

        if (EconomyManager.Instance.SpendCoDuyen(skill.unlockCost))
        {
            skill.isUnlocked = true;
            Debug.Log($"[Skill] Đã dùng Cơ Duyên để lĩnh ngộ {skill.skillName}!");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null && SkillManager.Instance != null)
        {
            SkillData skill = (skillIndex == 0) ? SkillManager.Instance.activeSkill : SkillManager.Instance.normalSkills[skillIndex - 1];
            if (skill != null)
            {
                string info = $"<color=#00FFFF>{skill.skillName}</color>\n";
                info += $"<color=#FFD700>Sát thương:</color> {skill.baseDamage:F0}\n";
                info += $"<color=#FFD700>Hồi chiêu:</color> {skill.baseCooldown}s\n";
                info += $"<color=#FFD700>Tiêu hao:</color> {skill.manaCost} Linh Lực";
                
                if (skillIndex == 0) info += "\n\n<i>Chiêu cuối chủ động (Bấm Space)</i>";
                else info += "\n\n<i>Kỹ năng tự động xuất chiêu</i>";

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

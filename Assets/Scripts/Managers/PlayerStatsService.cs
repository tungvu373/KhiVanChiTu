// File: Assets/Scripts/Managers/PlayerStatsService.cs
using UnityEngine;
using System;

[System.Serializable]
public struct BodyTalentNodeInfo
{
    public int index;
    public string name;
    public string title;
    public string branchName;
    public int tier;
    public int parentIndex; // -1 if root
    public int requiredParentLevel;
    public string iconEmoji;
    public string statPerLevelText;
    public string combatMechanics;
    public string lore;
}

/// <summary>
/// Service trung tâm tổng hợp 6 chỉ số Luyện Thể (Thể Tu) theo dạng Cây Kỹ Năng / Thiên Phú (Skill Tree).
/// Phân thành 2 nhánh:
/// - Nhánh Cương Thân (Phòng Ngự & Sinh Tồn): Khí Huyết (T1) -> Dũ Hợp (T2) -> Cương Khí (T3)
/// - Nhánh Thần Lực (Tấn Công & Chiến Đấu): Kình Lực (T1) -> Phệ Huyết (T2) -> Định Tâm (T3)
/// Index: 0=KhíHuyết, 1=CươngKhí, 2=DũHợp, 3=KìnhLực, 4=PhệHuyết, 5=ĐịnhTâm.
/// Các Manager subscribe OnStatsChanged, không đọc public field trực tiếp.
/// </summary>
public class PlayerStatsService : MonoBehaviour
{
    private static PlayerStatsService _instance;
    public static PlayerStatsService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<PlayerStatsService>();
            }
            return _instance;
        }
        private set => _instance = value;
    }
    private readonly int[] bodyStatLevels = new int[6];

    public static readonly BodyTalentNodeInfo[] TalentNodes = new BodyTalentNodeInfo[]
    {
        new BodyTalentNodeInfo
        {
            index = 0,
            name = "Khí Huyết",
            title = "Luyện Bì Cương Thể",
            branchName = "Cương Thân",
            tier = 1,
            parentIndex = -1,
            requiredParentLevel = 0,
            iconEmoji = "🛡️",
            statPerLevelText = "+150 Máu Tối Đa / Cấp",
            combatMechanics = "Tăng trực tiếp máu tối đa cơ bản của nhân vật. Giúp đạo hữu chống chịu bền bỉ trước đòn đánh quái đông và sấm sét lôi kiếp mà không bị sốc sát thương.",
            lore = "Khí huyết như sóng cuộn, thân thể như bàn thạch, ngàn đao vạn kiếm chém vào không sứt mẻ."
        },
        new BodyTalentNodeInfo
        {
            index = 1,
            name = "Cương Khí",
            title = "Kim Cương Hộ Thể",
            branchName = "Cương Thân",
            tier = 3,
            parentIndex = 2,
            requiredParentLevel = 5,
            iconEmoji = "💎",
            statPerLevelText = "+0.5% Giảm Sát Thương / Cấp (Tối đa 60%)",
            combatMechanics = "Ngưng tụ một lớp kình khí bảo hộ quanh kinh mạch, trực tiếp triệt tiêu % sát thương gánh chịu từ mọi nguồn công kích từ quái và đại Boss bí cảnh.",
            lore = "Hộ thể cương khí ngưng kết thành giáp, vạn pháp bất xâm, đao thương bất nhập."
        },
        new BodyTalentNodeInfo
        {
            index = 2,
            name = "Dũ Hợp",
            title = "Huyết Mạch Tuần Hoàn",
            branchName = "Cương Thân",
            tier = 2,
            parentIndex = 0,
            requiredParentLevel = 5,
            iconEmoji = "🌿",
            statPerLevelText = "+5 HP Hồi Phục/Giây / Cấp",
            combatMechanics = "Kích hoạt khả năng tự chữa lành vết thương liên tục mỗi giây trong giao tranh. Cực kỳ quan trọng để cắm máy treo đồ (AFK) an toàn không bao giờ hết máu.",
            lore = "Sinh cơ dồi dào như cỏ xuân gặp mưa rào, huyết nhục tái sinh trong chớp mắt."
        },
        new BodyTalentNodeInfo
        {
            index = 3,
            name = "Kình Lực",
            title = "Luyện Cốt Thần Kình",
            branchName = "Thần Lực",
            tier = 1,
            parentIndex = -1,
            requiredParentLevel = 0,
            iconEmoji = "⚔️",
            statPerLevelText = "+10 Sát Thương Gốc Kiếm / Cấp",
            combatMechanics = "Truyền kình đạo cương mãnh vào tâm kiếm, cộng thẳng vào sát thương cơ bản của TẤT CẢ phi kiếm trên trận, khuếch đại cực lớn khi mở thêm ô ngự kiếm.",
            lore = "Kình lực quán thông bách mạch, ngón tay búng ra kình khí chém đứt ngọn núi xa vạn dặm."
        },
        new BodyTalentNodeInfo
        {
            index = 4,
            name = "Phệ Huyết",
            title = "Ma Linh Hấp Huyết",
            branchName = "Thần Lực",
            tier = 2,
            parentIndex = 3,
            requiredParentLevel = 5,
            iconEmoji = "🩸",
            statPerLevelText = "+0.2% Hút Máu Đòn Đánh / Cấp",
            combatMechanics = "Mỗi khi phi kiếm trảm trúng đối thủ, chuyển hóa một phần sát thương gây ra thành sinh lực hồi phục cho bản thân. Đánh càng đau, hồi phục càng mãnh liệt.",
            lore = "Dĩ chiến dưỡng chiến, lấy máu kẻ địch bồi bổ huyết khí của ta, sinh sinh bất tức."
        },
        new BodyTalentNodeInfo
        {
            index = 5,
            name = "Định Tâm",
            title = "Bất Động Thần Niệm",
            branchName = "Thần Lực",
            tier = 3,
            parentIndex = 4,
            requiredParentLevel = 5,
            iconEmoji = "🧘",
            statPerLevelText = "+1.0% Kháng Choáng & Khống Chế / Cấp (Tối đa 80%)",
            combatMechanics = "Rèn luyện thần thức và định lực thép, giảm thời gian bị choáng (stun), làm chậm và các hiệu ứng khống chế từ các đại Boss bí cảnh.",
            lore = "Tâm tĩnh như mặt hồ mùa đông, sấm sét nổ bên tai thần sắc không hề đổi."
        }
    };

    // ── 6 chỉ số Thể Tu (read-only từ ngoài) ─────────────────────────────────
    /// <summary>Khí Huyết: +150 HP/cấp. Base = 1000.</summary>
    public float BaseMaxHp        { get; private set; } = 1000f;
    /// <summary>Cương Khí: +0.5% giảm sát thương/cấp, tối đa 60%.</summary>
    public float DamageReduction  { get; private set; } = 0f;
    /// <summary>Dũ Hợp: +5 HP hồi/giây/cấp.</summary>
    public float HpRegenPerSec    { get; private set; } = 0f;
    /// <summary>Kình Lực: +10 base damage phi kiếm/cấp.</summary>
    public float BonusSwordDamage { get; private set; } = 0f;
    /// <summary>Phệ Huyết: +0.2% hút máu/cấp (0..1).</summary>
    public float LifestealPercent { get; private set; } = 0f;
    /// <summary>Định Tâm: +1% kháng choáng/cấp (0..1).</summary>
    public float TenacityPercent  { get; private set; } = 0f;

    /// <summary>Phát sau mỗi lần RecalculateStats. Subscriber cập nhật UI/combat.</summary>
    public event Action OnStatsChanged;

    public int TotalBodyLevel
    {
        get
        {
            int total = 0;
            for (int i = 0; i < bodyStatLevels.Length; i++) total += bodyStatLevels[i];
            return total;
        }
    }

    public int GetLevel(int index)
    {
        return index >= 0 && index < bodyStatLevels.Length ? bodyStatLevels[index] : 0;
    }

    public float GetUpgradeCost(int index) => GetUpgradeCostAtLevel(index, GetLevel(index));

    public float GetUpgradeCostAtLevel(int index, int level)
    {
        if (index < 0 || index >= bodyStatLevels.Length) return float.PositiveInfinity;
        var branchDiscount = index < 3 ? DaoAnEffect.BodyGuardCostReduction : DaoAnEffect.BodyForceCostReduction;
        float multiplier = DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.BodyCostReduction, branchDiscount);
        return Mathf.Ceil(100f * (index + 1) * Mathf.Pow(1.2f, level) * multiplier);
    }

    public bool IsNodeUnlocked(int index)
    {
        if (index < 0 || index >= TalentNodes.Length) return false;
        // Bảo lưu cấp độ nếu save cũ đã nâng
        if (GetLevel(index) > 0) return true;

        // Cổng tu vi Hóa Thần theo KE_HOACH_TAI_CAU_TRUC_MO_KHOA_THEO_TU_VI.md
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsBodyNodeUnlocked(index)) return false;

        int pIndex = TalentNodes[index].parentIndex;
        if (pIndex < 0) return true; // Căn bản (Tier 1) mở sẵn khi đạt cảnh giới

        return GetLevel(pIndex) >= TalentNodes[index].requiredParentLevel;
    }

    public string GetUnlockRequirementText(int index)
    {
        if (index < 0 || index >= TalentNodes.Length) return "";
        if (IsNodeUnlocked(index)) return "Đã mở khóa";
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsBodyNodeUnlocked(index))
        {
            return FeatureUnlockService.Instance.GetBodyNodeUnlockRequirementText(index);
        }
        int pIndex = TalentNodes[index].parentIndex;
        int reqLv = TalentNodes[index].requiredParentLevel;
        return $"Yêu cầu: {TalentNodes[pIndex].name} Cấp {reqLv} (Hiện có: Cấp {GetLevel(pIndex)})";
    }

    public string GetCurrentEffectString(int index)
    {
        int lv = GetLevel(index);
        switch (index)
        {
            case 0: return $"+{lv * 150f:N0} HP";
            case 1: return $"{Mathf.Min(lv * 0.5f, 60f):F1}% Miễn thương";
            case 2: return $"+{lv * 5f:N0} HP/giây";
            case 3: return $"+{lv * 10f:N0} Sát thương kiếm";
            case 4: return $"{lv * 0.2f:F1}% Hút máu";
            case 5: return $"{Mathf.Min(lv * 1f, 80f):F0}% Kháng choáng";
            default: return "";
        }
    }

    public string GetNextEffectString(int index)
    {
        int nextLv = GetLevel(index) + 1;
        switch (index)
        {
            case 0: return $"+{nextLv * 150f:N0} HP (+150)";
            case 1: return $"{Mathf.Min(nextLv * 0.5f, 60f):F1}% Miễn thương (+0.5%)";
            case 2: return $"+{nextLv * 5f:N0} HP/giây (+5)";
            case 3: return $"+{nextLv * 10f:N0} Sát thương kiếm (+10)";
            case 4: return $"{nextLv * 0.2f:F1}% Hút máu (+0.2%)";
            case 5: return $"{Mathf.Min(nextLv * 1f, 80f):F0}% Kháng choáng (+1%)";
            default: return "";
        }
    }

    public int GetTotalBodyLevel()
    {
        int total = 0;
        for (int i = 0; i < bodyStatLevels.Length; i++) total += bodyStatLevels[i];
        return total;
    }

    public bool CanUpgrade(int index)
    {
        if (!IsNodeUnlocked(index)) return false;
        float cost = GetUpgradeCost(index);
        return EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= cost;
    }

    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= bodyStatLevels.Length || EconomyManager.Instance == null) return false;
        if (!IsNodeUnlocked(index))
        {
            Debug.LogWarning($"[PlayerStatsService] Chưa mở khóa node {TalentNodes[index].name}!");
            return false;
        }

        float cost = GetUpgradeCost(index);
        if (!EconomyManager.Instance.SpendLinhThach(cost)) return false;

        bodyStatLevels[index]++;
        RecalculateStats(bodyStatLevels, null);
        return true;
    }

    public int TryUpgradeMultiple(int index, int maxCount)
    {
        if (index < 0 || index >= bodyStatLevels.Length || EconomyManager.Instance == null || maxCount <= 0) return 0;
        if (!IsNodeUnlocked(index)) return 0;

        int upgraded = 0;
        for (int i = 0; i < maxCount; i++)
        {
            float cost = GetUpgradeCost(index);
            if (!EconomyManager.Instance.SpendLinhThach(cost)) break;
            bodyStatLevels[index]++;
            upgraded++;
        }

        if (upgraded > 0)
        {
            RecalculateStats(bodyStatLevels, null);
        }
        return upgraded;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    /// <summary>
    /// Tính lại toàn bộ 6 chỉ số từ array level Luyện Thể.
    /// Gọi khi: load slot, upgrade stat, respec.
    /// </summary>
    public void RecalculateStats(int[] bodyStatLevels, bool[] milestonePassives)
    {
        if (bodyStatLevels == null || bodyStatLevels.Length < 6)
        {
            Debug.LogWarning("[PlayerStatsService] bodyStatLevels null hoặc thiếu phần tử.");
            return;
        }

        for (int i = 0; i < this.bodyStatLevels.Length; i++)
            this.bodyStatLevels[i] = Mathf.Max(0, bodyStatLevels[i]);

        BaseMaxHp        = 1000f + (this.bodyStatLevels[0] * 150f);
        DamageReduction  = Mathf.Min(this.bodyStatLevels[1] * 0.005f, 0.60f);
        HpRegenPerSec    = this.bodyStatLevels[2] * 5f;
        BonusSwordDamage = this.bodyStatLevels[3] * 10f;
        LifestealPercent = Mathf.Min(this.bodyStatLevels[4] * 0.002f, 0.20f); // TBH.md: trần 20%
        TenacityPercent  = Mathf.Min(this.bodyStatLevels[5] * 0.01f, 0.80f);  // TBH.md mục 4: trần 80% kháng choáng

        Debug.Log($"[PlayerStatsService] Recalc: MaxHP={BaseMaxHp}, " +
                  $"DmgReduce={DamageReduction * 100f:F1}%, BonusDmg={BonusSwordDamage}");
        OnStatsChanged?.Invoke();
    }

    /// <summary>Load stats từ save slot đã qua migration.</summary>
    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (slot == null || slot.isSlotEmpty) return;
        for (int i = 0; i < bodyStatLevels.Length; i++)
            bodyStatLevels[i] = slot.bodyStatLevels != null && i < slot.bodyStatLevels.Length
                ? Mathf.Max(0, slot.bodyStatLevels[i]) : 0;
        RecalculateStats(bodyStatLevels, slot.bodyMilestonePassives);
    }

    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot == null) return;
        slot.bodyStatLevels = (int[])bodyStatLevels.Clone();
    }
}

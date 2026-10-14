using System;
using System.Collections.Generic;
using UnityEngine;

public enum MilestoneType
{
    KillMonster,   // Diệt quái thường
    DefeatBoss,    // Vượt ải / Boss Thiên Kiếp (Đạt cảnh giới)
    ForgeSword,    // Rèn / Ghép được kiếm cấp cao
    UpgradeKiemY   // Nâng cấp Kiếm Ý
}

[Serializable]
public class MilestoneQuest
{
    public string id;
    public string title;
    public string description;
    public MilestoneType type;
    public int targetAmount;

    [Header("Phần Thưởng")]
    public float rewardLinhThach;
    public int rewardCoDuyen;       // Điểm Cơ Duyên cho nhiệm vụ khó
    public int rewardDanDuocIndex = -1; // 0: Hồi Linh, 1: Tật Phong, 2: Hồi Huyết, 3: Cuồng Nộ, 4: Tĩnh Tâm
    public int rewardDanDuocAmount = 0; // Số lượng đan dược
    public int rewardSwordLevel = 0;    // Kiếm cấp cao nếu có

    public bool isHard = false;     // Đánh dấu nhiệm vụ khó (viền vàng kim, có Cơ Duyên)
    public bool isClaimed = false;

    public string GetRewardSummary()
    {
        List<string> parts = new List<string>();
        if (rewardLinhThach > 0) parts.Add($"+{rewardLinhThach:F0} LT");
        if (rewardCoDuyen > 0) parts.Add($"✨ Cơ Duyên +{rewardCoDuyen}");

        if (rewardDanDuocIndex >= 0 && rewardDanDuocAmount > 0)
        {
            string[] danDuocNames = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Nộ Đan", "Tĩnh Tâm Đan" };
            string dName = (rewardDanDuocIndex < danDuocNames.Length) ? danDuocNames[rewardDanDuocIndex] : "Linh Đan";
            parts.Add($"💊 {dName} x{rewardDanDuocAmount}");
        }

        if (rewardSwordLevel > 0)
        {
            parts.Add($"⚔️ Kiếm Cấp {rewardSwordLevel}");
        }

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Quản lý Hệ Thống Nhiệm Vụ Mốc (Milestone Quest System):
/// - Tiêu diệt quái, Vượt ải/Độ kiếp Boss, Rèn kiếm cấp cao, Nâng Kiếm Ý.
/// - Thưởng Đan dược trợ chiến (kích hoạt tức thì) và Cơ duyên (cho nhiệm vụ khó).
/// </summary>
public class MilestoneManager : MonoBehaviour
{
    public static MilestoneManager Instance { get; private set; }

    public List<MilestoneQuest> allQuests = new List<MilestoneQuest>();

    public event Action<MilestoneQuest> OnMilestoneClaimed;
    public event Action OnMilestonesUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        InitializeQuests();
    }

    private void InitializeQuests()
    {
        allQuests.Clear();

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 1: TIÊU DIỆT YÊU THÚ (MONSTER HUNT)
        // ═══════════════════════════════════════════════════════════════════
        allQuests.Add(new MilestoneQuest
        {
            id = "kill_10",
            title = "⚔️ Trảm Thảo Trừ Căn I",
            description = "Tiêu diệt 10 Yêu Thú tại bí cảnh",
            type = MilestoneType.KillMonster,
            targetAmount = 10,
            rewardLinhThach = 100,
            rewardDanDuocIndex = 2, // Hồi Huyết Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kill_30",
            title = "⚔️ Trảm Thảo Trừ Căn II",
            description = "Tiêu diệt 30 Yêu Thú tại bí cảnh",
            type = MilestoneType.KillMonster,
            targetAmount = 30,
            rewardLinhThach = 250,
            rewardDanDuocIndex = 1, // Tật Phong Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kill_80",
            title = "⚔️ Trảm Thảo Trừ Căn III",
            description = "Tiêu diệt 80 Yêu Thú tại bí cảnh",
            type = MilestoneType.KillMonster,
            targetAmount = 80,
            rewardLinhThach = 600,
            rewardDanDuocIndex = 3, // Cuồng Nộ Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kill_200",
            title = "🔥 Huyết Chiến Yêu Tộc (Khó)",
            description = "Tiêu diệt 200 Yêu Thú lập uy danh chấn động",
            type = MilestoneType.KillMonster,
            targetAmount = 200,
            rewardLinhThach = 1500,
            rewardCoDuyen = 10, // Cơ Duyên
            rewardDanDuocIndex = 3, // Cuồng Nộ Đan
            rewardDanDuocAmount = 2,
            isHard = true
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kill_500",
            title = "👑 Yêu Giới Đồ Tể (Cực Khó)",
            description = "Tiêu diệt 500 Yêu Thú, quét sạch sơn hà",
            type = MilestoneType.KillMonster,
            targetAmount = 500,
            rewardLinhThach = 3500,
            rewardCoDuyen = 25, // Cơ Duyên lớn
            rewardDanDuocIndex = 4, // Tĩnh Tâm Đan
            rewardDanDuocAmount = 2,
            rewardSwordLevel = 3,
            isHard = true
        });

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 2: VƯỢT THIÊN KIẾP & ĐÁNH BẠI BOSS (TRIBULATION & BOSS)
        // ═══════════════════════════════════════════════════════════════════
        allQuests.Add(new MilestoneQuest
        {
            id = "boss_stage_1",
            title = "⚡ Lôi Kiếp Sơ Khai",
            description = "Độ kiếp đánh bại Boss, thăng tiến Luyện Khí Tầng 2",
            type = MilestoneType.DefeatBoss,
            targetAmount = 1,
            rewardLinhThach = 300,
            rewardDanDuocIndex = 4, // Tĩnh Tâm Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "boss_stage_2",
            title = "⚡ Củng Cố Căn Đạo",
            description = "Độ kiếp đánh bại Boss, thăng tiến Luyện Khí Tầng 3",
            type = MilestoneType.DefeatBoss,
            targetAmount = 2,
            rewardLinhThach = 700,
            rewardDanDuocIndex = 0, // Hồi Linh Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "boss_stage_3",
            title = "⚡ Nghịch Thiên Phá Cảnh (Khó)",
            description = "Độ kiếp đánh bại Boss, thăng tiến Luyện Khí Tầng 4",
            type = MilestoneType.DefeatBoss,
            targetAmount = 3,
            rewardLinhThach = 1200,
            rewardCoDuyen = 15,
            rewardDanDuocIndex = 4, // Tĩnh Tâm Đan
            rewardDanDuocAmount = 2,
            isHard = true
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "boss_stage_9",
            title = "🌟 Đại Viên Mãn Nhập Đạo (Cực Khó)",
            description = "Vượt qua toàn bộ kiếp nạn Luyện Khí, thăng hoa cảnh giới lớn",
            type = MilestoneType.DefeatBoss,
            targetAmount = 9,
            rewardLinhThach = 4000,
            rewardCoDuyen = 40,
            rewardSwordLevel = 4,
            isHard = true
        });

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 3: LUYỆN KIẾM HỢP NHẤT (SWORD FORGING)
        // ═══════════════════════════════════════════════════════════════════
        allQuests.Add(new MilestoneQuest
        {
            id = "sword_lvl_3",
            title = "🗡️ Đúc Kiếm Hóa Hình",
            description = "Hợp nhất tạo ra Phi Kiếm Cấp 3",
            type = MilestoneType.ForgeSword,
            targetAmount = 3,
            rewardLinhThach = 150,
            rewardDanDuocIndex = 0, // Hồi Linh Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "sword_lvl_5",
            title = "🗡️ Bảo Kiếm Xuất Hạp",
            description = "Hợp nhất tạo ra Phi Kiếm Cấp 5",
            type = MilestoneType.ForgeSword,
            targetAmount = 5,
            rewardLinhThach = 500,
            rewardDanDuocIndex = 1, // Tật Phong Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "sword_lvl_7",
            title = "🗡️ Kiếm Khí Trùng Tiêu (Khó)",
            description = "Hợp nhất tạo ra Phi Kiếm Cấp 7 uy lực phi phàm",
            type = MilestoneType.ForgeSword,
            targetAmount = 7,
            rewardLinhThach = 1200,
            rewardCoDuyen = 15,
            rewardDanDuocIndex = 3, // Cuồng Nộ Đan
            rewardDanDuocAmount = 2,
            isHard = true
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "sword_lvl_10",
            title = "✨ Thần Binh Tuyệt Thế (Cực Khó)",
            description = "Hợp nhất tôi luyện thành công Phi Kiếm Cấp 10",
            type = MilestoneType.ForgeSword,
            targetAmount = 10,
            rewardLinhThach = 3000,
            rewardCoDuyen = 50,
            rewardSwordLevel = 5,
            isHard = true
        });

        // ═══════════════════════════════════════════════════════════════════
        // NHÓM 4: TU LUYỆN KIẾM Ý (CULTIVATION MASTERY)
        // ═══════════════════════════════════════════════════════════════════
        allQuests.Add(new MilestoneQuest
        {
            id = "kiemy_lvl_3",
            title = "🧘 Khí Quán Kiếm Phong",
            description = "Nâng cấp Kiếm Ý lên Cấp 3 tăng sát thương",
            type = MilestoneType.UpgradeKiemY,
            targetAmount = 3,
            rewardLinhThach = 200,
            rewardDanDuocIndex = 1, // Tật Phong Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kiemy_lvl_5",
            title = "🧘 Nhân Kiếm Hợp Nhất",
            description = "Nâng cấp Kiếm Ý lên Cấp 5",
            type = MilestoneType.UpgradeKiemY,
            targetAmount = 5,
            rewardLinhThach = 600,
            rewardDanDuocIndex = 3, // Cuồng Nộ Đan
            rewardDanDuocAmount = 1
        });

        allQuests.Add(new MilestoneQuest
        {
            id = "kiemy_lvl_10",
            title = "🌌 Vạn Kiếm Quy Tâm (Khó)",
            description = "Nâng cấp Kiếm Ý lên Cấp 10 lĩnh ngộ cảnh giới tối cao",
            type = MilestoneType.UpgradeKiemY,
            targetAmount = 10,
            rewardLinhThach = 2000,
            rewardCoDuyen = 20,
            rewardDanDuocIndex = 4, // Tĩnh Tâm Đan
            rewardDanDuocAmount = 2,
            isHard = true
        });
    }

    public int GetCurrentProgress(MilestoneQuest quest)
    {
        if (quest == null) return 0;
        switch (quest.type)
        {
            case MilestoneType.KillMonster:
                return StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0;

            case MilestoneType.DefeatBoss:
                return CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;

            case MilestoneType.ForgeSword:
                return GetHighestSwordLevel();

            case MilestoneType.UpgradeKiemY:
                return UpgradeManager.Instance != null ? UpgradeManager.Instance.kiemYLevel : 1;
        }
        return 0;
    }

    public bool IsCompleted(MilestoneQuest quest)
    {
        if (quest == null) return false;
        return GetCurrentProgress(quest) >= quest.targetAmount;
    }

    public int GetHighestSwordLevel()
    {
        int maxLvl = 1;
        if (MergeManager.Instance == null) return maxLvl;

        // Quét ô trang bị
        if (MergeManager.Instance.equipPanel != null)
        {
            foreach (Transform slot in MergeManager.Instance.equipPanel)
            {
                if (slot == null) continue;
                MergeItem item = slot.GetComponentInChildren<MergeItem>();
                if (item != null && item.level > maxLvl) maxLvl = item.level;
            }
        }

        // Quét kho đồ
        if (MergeManager.Instance.inventoryPages != null)
        {
            foreach (Transform page in MergeManager.Instance.inventoryPages)
            {
                if (page == null) continue;
                foreach (Transform slot in page)
                {
                    if (slot == null) continue;
                    MergeItem item = slot.GetComponentInChildren<MergeItem>();
                    if (item != null && item.level > maxLvl) maxLvl = item.level;
                }
            }
        }
        return maxLvl;
    }

    public bool ClaimReward(string questId)
    {
        MilestoneQuest quest = allQuests.Find(q => q.id == questId);
        if (quest == null) return false;

        if (quest.isClaimed)
        {
            Debug.Log($"[Milestone] Nhiệm vụ '{quest.title}' đã nhận rồi!");
            return false;
        }

        if (!IsCompleted(quest))
        {
            Debug.LogWarning($"[Milestone] Nhiệm vụ '{quest.title}' chưa đủ điều kiện!");
            return false;
        }

        quest.isClaimed = true;

        // 1. Thưởng Linh Thạch
        if (quest.rewardLinhThach > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddLinhThach(quest.rewardLinhThach);
        }

        // 2. Thưởng Cơ Duyên
        if (quest.rewardCoDuyen > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddCoDuyen(quest.rewardCoDuyen);
        }

        // 3. Thưởng Đan Dược
        if (quest.rewardDanDuocIndex >= 0 && quest.rewardDanDuocAmount > 0 && ShopManager.Instance != null)
        {
            for (int i = 0; i < quest.rewardDanDuocAmount; i++)
            {
                ShopManager.Instance.ApplyBuff(quest.rewardDanDuocIndex);
            }
        }

        // 4. Thưởng Kiếm Cấp Cao
        if (quest.rewardSwordLevel > 0 && MergeManager.Instance != null)
        {
            MergeManager.Instance.TryAddSword(quest.rewardSwordLevel);
        }

        // SFX & Feedback
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"🎉 Nhận Thưởng Mốc [{quest.title}]: {quest.GetRewardSummary()}!", Color.green);
        }

        // Lưu tiến trình
        if (DataManager.Instance != null && DataManager.Instance.isSessionActive)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }

        OnMilestoneClaimed?.Invoke(quest);
        OnMilestonesUpdated?.Invoke();

        return true;
    }

    /// <summary>
    /// Đếm số nhiệm vụ đã hoàn thành nhưng CHƯA NHẬN THƯỞNG (để hiện badge chấm đỏ/số đếm).
    /// </summary>
    public int GetReadyToClaimCount()
    {
        int count = 0;
        foreach (var q in allQuests)
        {
            if (!q.isClaimed && IsCompleted(q)) count++;
        }
        return count;
    }

    /// <summary>
    /// Lấy nhiệm vụ mốc tiếp theo gần nhất (ưu tiên nhiệm vụ đã xong nhưng chưa nhận, sau đó đến nhiệm vụ đang làm dở).
    /// </summary>
    public MilestoneQuest GetNextPendingMilestone()
    {
        // 1. Ưu tiên nhiệm vụ đã xong chờ nhận thưởng
        foreach (var q in allQuests)
        {
            if (!q.isClaimed && IsCompleted(q)) return q;
        }

        // 2. Nhiệm vụ chưa xong gần nhất
        foreach (var q in allQuests)
        {
            if (!q.isClaimed) return q;
        }

        return null;
    }

    // ── SAVE / LOAD THEO CHARACTER SAVE DATA ──────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        List<string> claimedList = new List<string>();
        foreach (var q in allQuests)
        {
            if (q.isClaimed) claimedList.Add(q.id);
        }
        slot.claimedMilestoneIds = claimedList.ToArray();
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        HashSet<string> claimedSet = new HashSet<string>(slot.claimedMilestoneIds ?? new string[0]);
        foreach (var q in allQuests)
        {
            q.isClaimed = claimedSet.Contains(q.id);
        }
        OnMilestonesUpdated?.Invoke();
    }
}

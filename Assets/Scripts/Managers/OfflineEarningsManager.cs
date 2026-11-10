using System;
using UnityEngine;

/// <summary>
/// Quản lý phần thưởng Bế Quan Tu Luyện (Treo máy Offline).
/// Tính toán tài nguyên thu được khi người chơi offline dựa theo thời gian UTC và tiến trình game.
/// Giới hạn tối đa 8 giờ để khuyến khích retention.
/// Tích hợp sẵn hook x2 thưởng qua Rewarded Ads.
/// </summary>
public class OfflineEarningsManager : MonoBehaviour
{
    public static OfflineEarningsManager Instance { get; private set; }

    [Serializable]
    public struct OfflineRewardData
    {
        public float offlineSeconds;
        public float linhThach;
        public float tuVi;
        public int linhChi;
        public int ngungKhi;
        public int tuyetLien;
        public int ore;
        public bool hasReward;
    }

    public const float MAX_OFFLINE_SECONDS = 8f * 3600f; // Tối đa 8 giờ
    public const float MIN_OFFLINE_SECONDS = 60f;        // Tối thiểu 1 phút vắng mặt mới tính

    public OfflineRewardData PendingReward { get; private set; }
    public bool HasPendingReward => PendingReward.hasReward;

    public event Action<OfflineRewardData> OnOfflineRewardReady;
    public event Action OnOfflineRewardClaimed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Xử lý tính toán phần thưởng offline khi nạp dữ liệu nhân vật.
    /// </summary>
    public void ProcessOfflineEarnings(CharacterSaveData slot)
    {
        if (slot == null || slot.isSlotEmpty) return;

        PendingReward = slot.pendingOfflineReward;

        long currentUtcTicks = DateTime.UtcNow.Ticks;

        // Nếu là save mới hoặc chưa có mốc thời gian, khởi tạo mốc hiện tại
        if (slot.lastActiveUtcTicks <= 0)
        {
            slot.lastActiveUtcTicks = currentUtcTicks;
            ShowPendingReward();
            return;
        }

        long elapsedTicks = currentUtcTicks - slot.lastActiveUtcTicks;
        if (elapsedTicks <= 0)
        {
            // Trường hợp người chơi đổi giờ lùi về quá khứ
            slot.lastActiveUtcTicks = currentUtcTicks;
            ShowPendingReward();
            return;
        }

        float elapsedSeconds = (float)(elapsedTicks / (double)TimeSpan.TicksPerSecond);

        // Cập nhật lại mốc hoạt động mới nhất
        slot.lastActiveUtcTicks = currentUtcTicks;

        // Nếu vắng mặt chưa đủ 1 phút, không kích hoạt popup
        if (elapsedSeconds < MIN_OFFLINE_SECONDS)
        {
            ShowPendingReward();
            return;
        }

        // Giới hạn tối đa 8 giờ
        float alreadyAccrued = Mathf.Min(PendingReward.offlineSeconds, MAX_OFFLINE_SECONDS);
        float effectiveSeconds = Mathf.Min(elapsedSeconds, MAX_OFFLINE_SECONDS - alreadyAccrued);

        // ── 1. Tính toán Linh Thạch offline ──
        int stage = slot.combatStage > 0 ? slot.combatStage : 1;
        int realm = slot.realmId;
        float stoneDropMult = 1f;
        if (ArtifactService.Instance != null)
        {
            stoneDropMult = ArtifactService.Instance.StoneDropMultiplier;
        }
        else if (slot.artifacts != null && slot.artifacts.levels != null && slot.artifacts.levels.Length > 3)
        {
            stoneDropMult = 1f + slot.artifacts.levels[3] * 0.02f;
        }

        // Tốc độ cơ bản: ~20 + stage*5 Linh Thạch / giây
        float stonePerSec = (20f + stage * 5f) * (1f + realm * 0.25f) * stoneDropMult;
        float daoAnOffline = DaoAnProgressionService.Instance != null
            ? DaoAnProgressionService.Instance.GetMultiplier(DaoAnEffect.OfflineLinhThach) : 1f;
        float totalStone = stonePerSec * effectiveSeconds * daoAnOffline;

        // ── 2. Tính toán Tu Vi offline ──
        float tuViPerSec = (1.5f + stage * 0.4f) * (1f + realm * 0.2f);
        float totalTuVi = tuViPerSec * effectiveSeconds * (1f + DaoAnProgressionService.Bonus(DaoAnEffect.OfflineTuVi));

        // ── 3. Thảo dược & Quặng phôi thu gom khi bế quan (Chặn theo cổng Tu Vi) ──
        bool canGetHerbs = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropHerbs();
        bool canGetOre = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropOre();

        // Cứ mỗi 10 phút (600s) được 1 mẻ Linh Chi (chỉ từ LK7+)
        int linhChi = canGetHerbs ? Mathf.FloorToInt(effectiveSeconds / 600f) * (1 + stage / 30) : 0;
        // Cứ mỗi 15 phút (900s) được 1 mẻ Cỏ Ngưng Khí (chỉ từ LK7+)
        int ngungKhi = canGetHerbs ? Mathf.FloorToInt(effectiveSeconds / 900f) * (1 + stage / 40) : 0;
        // Cứ mỗi 30 phút (1800s) được 1 đóa Tuyết Liên (chỉ từ LK7+)
        int tuyetLien = canGetHerbs ? Mathf.FloorToInt(effectiveSeconds / 1800f) : 0;
        // Cứ mỗi 20 phút (1200s) nhặt được Quặng Phôi (chỉ từ Trúc Cơ 1+)
        int ore = canGetOre ? Mathf.FloorToInt(effectiveSeconds / 1200f) * (1 + stage / 25) : 0;

        PendingReward = new OfflineRewardData
        {
            offlineSeconds = PendingReward.offlineSeconds + elapsedSeconds,
            linhThach = PendingReward.linhThach + totalStone,
            tuVi = PendingReward.tuVi + totalTuVi,
            linhChi = PendingReward.linhChi + linhChi,
            ngungKhi = PendingReward.ngungKhi + ngungKhi,
            tuyetLien = PendingReward.tuyetLien + tuyetLien,
            ore = PendingReward.ore + ore,
            hasReward = true
        };
        slot.pendingOfflineReward = PendingReward;

        Debug.Log($"[OfflineEarnings] Vắng mặt {elapsedSeconds:F0}s (tính {effectiveSeconds:F0}s). " +
                  $"Thu được: {totalStone:F0} Linh Thạch, {totalTuVi:F0} Tu Vi, " +
                  $"{linhChi} Linh Chi, {ngungKhi} Cỏ Ngưng Khí, {tuyetLien} Tuyết Liên, {ore} Quặng.");

        ShowPendingReward();
    }

    private void ShowPendingReward()
    {
        if (!PendingReward.hasReward) return;
        OnOfflineRewardReady?.Invoke(PendingReward);
        OfflineRewardPopupUI.Show(PendingReward);
    }

    /// <summary>
    /// Nhận phần thưởng tích lũy (hỗ trợ hệ số nhân khi xem quảng cáo x2).
    /// </summary>
    public void ClaimReward(float multiplier = 1f)
    {
        if (!PendingReward.hasReward) return;

        multiplier = Mathf.Max(1f, multiplier);

        float finalStone = PendingReward.linhThach * multiplier;
        float finalTuVi = PendingReward.tuVi * multiplier;
        int finalLinhChi = Mathf.RoundToInt(PendingReward.linhChi * multiplier);
        int finalNgungKhi = Mathf.RoundToInt(PendingReward.ngungKhi * multiplier);
        int finalTuyetLien = Mathf.RoundToInt(PendingReward.tuyetLien * multiplier);
        int finalOre = Mathf.RoundToInt(PendingReward.ore * multiplier);

        // 1. Linh Thạch
        if (finalStone > 0f)
        {
            EconomyManager.Instance?.AddLinhThach(finalStone);
        }

        // 2. Tu Vi
        if (finalTuVi > 0f && CultivationManager.Instance != null)
        {
            CultivationManager.Instance.AddTuVi(finalTuVi);
        }

        // 3. Thảo dược
        if (AlchemyService.Instance != null)
        {
            if (finalLinhChi > 0) AlchemyService.Instance.AddHerb(0, finalLinhChi);
            if (finalNgungKhi > 0) AlchemyService.Instance.AddHerb(1, finalNgungKhi);
            if (finalTuyetLien > 0) AlchemyService.Instance.AddHerb(2, finalTuyetLien);
        }

        // 4. Quặng phôi
        if (finalOre > 0 && ArtifactService.Instance != null)
        {
            ArtifactService.Instance.AddOre(finalOre);
        }

        string bonusNotice = multiplier > 1f ? $" <color=#FFD700>(x{multiplier:F0} Quảng Cáo)</color>" : "";
        GameLogger.Instance?.Log($"Nhận thưởng Bế Quan{bonusNotice}: +{EconomyManager.FormatNumber(finalStone)} Linh Thạch, +{finalTuVi:F0} Tu Vi!", Color.green);
        AchievementManager.Instance?.OnOfflineClaimed();

        // Reset phần thưởng pending
        PendingReward = default;
        int slotIndex = DataManager.Instance != null ? DataManager.Instance.gameSaveData.lastPlayedSlotIndex : -1;
        if (slotIndex >= 0 && slotIndex < DataManager.Instance.gameSaveData.slots.Length)
            DataManager.Instance.gameSaveData.slots[slotIndex].pendingOfflineReward = default;
        OnOfflineRewardClaimed?.Invoke();

        // Lưu dữ liệu ngay để bảo toàn tài nguyên vừa nhận
        DataManager.Instance?.SaveCurrentGame(silent: true);
    }

    /// <summary>
    /// Cập nhật thời điểm active UTC ticks khi lưu game.
    /// </summary>
    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot != null)
        {
            slot.lastActiveUtcTicks = DateTime.UtcNow.Ticks;
            slot.pendingOfflineReward = PendingReward;
        }
    }
}


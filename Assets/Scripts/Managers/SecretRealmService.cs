using System;
using UnityEngine;

/// <summary>
/// Quản lý toàn bộ logic Kỳ Ngộ Bí Cảnh Thượng Cổ (Secret Realm).
/// Tích hợp điểm Cơ Duyên (CD Sink), lượt miễn phí hàng ngày, và phân phát phần thưởng vào hệ sinh thái game.
/// </summary>
public class SecretRealmService : MonoBehaviour
{
    public static SecretRealmService Instance { get; private set; }

    public const int CO_DUYEN_ENTRY_COST = 5;

    public SecretRealmSaveState State { get; private set; } = new SecretRealmSaveState();

    public event Action OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CheckDailyReset();
    }

    public void CheckDailyReset()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (State.lastResetDate != today)
        {
            State.lastResetDate = today;
            State.freeAttemptsToday = 1;
            OnStateChanged?.Invoke();
        }
    }

    public bool HasFreeAttempt()
    {
        CheckDailyReset();
        return State.freeAttemptsToday > 0;
    }

    public bool CanEnter()
    {
        if (HasFreeAttempt()) return true;
        return EconomyManager.Instance != null && EconomyManager.Instance.currentCoDuyen >= CO_DUYEN_ENTRY_COST;
    }

    public bool TryConsumeEntry(out bool usedFree)
    {
        CheckDailyReset();
        if (State.freeAttemptsToday > 0)
        {
            State.freeAttemptsToday--;
            usedFree = true;
            OnStateChanged?.Invoke();
            return true;
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoDuyen(CO_DUYEN_ENTRY_COST))
        {
            usedFree = false;
            OnStateChanged?.Invoke();
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log($"✨ Đã tiêu hao {CO_DUYEN_ENTRY_COST} Cơ Duyên để mở Cổng Bí Cảnh!", new Color(1f, 0.85f, 0.3f));
            return true;
        }

        usedFree = false;
        return false;
    }

    public struct RewardResult
    {
        public int herbs0;
        public int herbs1;
        public int herbs2;
        public int ore;
        public float linhThach;
        public float tuVi;
        public int bonusCoDuyen;
        public int swordLevel;
        public string specialNote;
    }

    /// <summary>
    /// Tính toán và phân phát phần thưởng sau khi hoàn thành mini-game Bí Cảnh
    /// </summary>
    public RewardResult ProcessAndGrantRewards(SecretRealmType type, int score, bool doubleReward)
    {
        RewardResult res = new RewardResult();
        float multiplier = doubleReward ? 2f : 1f;

        switch (type)
        {
            case SecretRealmType.HordeSurvival:
                // score = số quái tiêu diệt trong 30s (0 - 50)
                State.hordeRecord = Mathf.Max(State.hordeRecord, score);
                res.herbs0 = Mathf.RoundToInt((score * 1) * multiplier);
                res.herbs1 = Mathf.RoundToInt((score / 2) * multiplier);
                res.herbs2 = Mathf.RoundToInt((score / 5) * multiplier);
                res.linhThach = (score * 400f) * multiplier;
                if (score >= 40)
                {
                    res.bonusCoDuyen = doubleReward ? 2 : 1;
                    res.specialNote = "Đại Thắng Vạn Ma: Thưởng thêm Cơ Duyên!";
                }
                break;

            case SecretRealmType.BarrierBreak:
                // score = % cấm chế đã phá vỡ (0 - 100)
                State.barrierRecord = Mathf.Max(State.barrierRecord, score);
                res.ore = Mathf.RoundToInt((2 + (score / 10)) * multiplier);
                res.linhThach = (score * 300f) * multiplier;
                if (score >= 100)
                {
                    res.ore += Mathf.RoundToInt(5 * multiplier);
                    res.specialNote = "Cấm Chế Vỡ Vụn: Rớt Phôi Pháp Khí Thượng Cổ!";
                }
                break;

            case SecretRealmType.DanCauldron:
                // score = 1: Cực phẩm (trúng), 0: Thường
                if (score > 0)
                {
                    State.cauldronPillsFound++;
                    res.bonusCoDuyen = Mathf.RoundToInt(3 * multiplier);
                    res.herbs0 = Mathf.RoundToInt(15 * multiplier);
                    res.herbs1 = Mathf.RoundToInt(10 * multiplier);
                    res.herbs2 = Mathf.RoundToInt(8 * multiplier);
                    res.linhThach = (10000f) * multiplier;
                    res.specialNote = "Nổ Hũ Tuyệt Phẩm: Nhận Dược Liệu Quý & Cơ Duyên!";
                }
                else
                {
                    res.herbs0 = Mathf.RoundToInt(6 * multiplier);
                    res.herbs1 = Mathf.RoundToInt(3 * multiplier);
                    res.linhThach = (3000f) * multiplier;
                    res.specialNote = "Mở trúng Đan Hương Dưỡng Thể.";
                }
                break;

            case SecretRealmType.LightningDodge:
                // score = số Lôi Tinh Cầu nhặt được (0 - 20)
                State.lightningOrbsRecord = Mathf.Max(State.lightningOrbsRecord, score);
                res.tuVi = (score * 2500f) * multiplier;
                res.linhThach = (score * 250f) * multiplier;
                if (score >= 12)
                {
                    res.bonusCoDuyen = doubleReward ? 2 : 1;
                    res.specialNote = "Lôi Thần Ban Phúc: Thưởng thêm Cơ Duyên!";
                }
                break;

            case SecretRealmType.SwordTomb:
                // score = Cấp kiếm rút được (4 -> 8)
                res.swordLevel = Mathf.Clamp(score, 4, 8);
                State.swordTombMaxLevel = Mathf.Max(State.swordTombMaxLevel, res.swordLevel);
                res.linhThach = (res.swordLevel * 2000f) * multiplier;
                res.specialNote = $"Rút được Phi Kiếm Thượng Cổ Lv.{res.swordLevel}!";
                break;
        }

        // Thực hiện cấp phát vào game
        if (res.herbs0 > 0 && AlchemyService.Instance != null) AlchemyService.Instance.AddHerb(0, res.herbs0);
        if (res.herbs1 > 0 && AlchemyService.Instance != null) AlchemyService.Instance.AddHerb(1, res.herbs1);
        if (res.herbs2 > 0 && AlchemyService.Instance != null) AlchemyService.Instance.AddHerb(2, res.herbs2);
        if (res.ore > 0 && ArtifactService.Instance != null) ArtifactService.Instance.AddOre(res.ore);
        if (res.linhThach > 0 && EconomyManager.Instance != null) EconomyManager.Instance.AddLinhThach(res.linhThach);
        if (res.tuVi > 0 && CultivationManager.Instance != null) CultivationManager.Instance.AddTuVi(res.tuVi);
        if (res.bonusCoDuyen > 0 && EconomyManager.Instance != null) EconomyManager.Instance.AddCoDuyen(res.bonusCoDuyen);

        if (res.swordLevel > 0)
        {
            if (StorageManager.Instance != null)
                StorageManager.Instance.HandleSwordDrop(res.swordLevel, false);
            else if (MergeManager.Instance != null)
                MergeManager.Instance.TryAddSword(res.swordLevel);
        }

        State.totalCleared++;
        OnStateChanged?.Invoke();

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"🎉 Hoàn tất Bí Cảnh! Nhận: +{res.linhThach:N0} LT, {res.herbs0 + res.herbs1 + res.herbs2} Thảo Dược, {res.ore} Quặng.", new Color(0.3f, 1f, 0.4f));
        }

        return res;
    }

    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot == null) return;
        slot.secretRealm = State.Clone();
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        if (slot == null || slot.secretRealm == null)
        {
            State = new SecretRealmSaveState();
        }
        else
        {
            State = slot.secretRealm.Clone();
        }
        CheckDailyReset();
        OnStateChanged?.Invoke();
    }
}


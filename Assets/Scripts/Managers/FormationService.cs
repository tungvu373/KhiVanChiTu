using System;
using UnityEngine;

/// <summary>
/// Sáu Đại Trận Pháp Hộ Thân:
/// 0. Lôi Trận (Thunder - Sét đánh AoE tự động)
/// 1. Tụ Linh Trận (Spirit - Chủ động x2 Tu Vi)
/// 2. Băng Trận (Frost - Làm chậm tốc độ Boss)
/// 3. Vạn Kiếm Trận (SwordRain - Tăng tốc kiếm & Mưa kiếm bão tố)
/// 4. Huyết Ma Trận (BloodDrain - Hút máu hồi phục & Thiêu đốt)
/// 5. Kim Cang Trận (GoldenBell - Giảm sát thương & Hộ thể thuẫn)
/// </summary>
public class FormationService : MonoBehaviour
{
    public static FormationService Instance { get; private set; }
    public event Action OnChanged;
    public FormationSaveState State { get; private set; } = new FormationSaveState();
    public const int MaxLevel = 30;

    private float nextThunderTime;
    private float nextSwordRainTime;
    private float nextShieldCooldownTime;

    public FormationId Selected => (FormationId)State.selectedFormation;
    public bool SpiritActive => State.spiritEndUtcTicks > DateTime.UtcNow.Ticks;
    public TimeSpan SpiritCooldown => TimeSpan.FromTicks(Math.Max(0L, State.spiritReadyUtcTicks - DateTime.UtcNow.Ticks));

    // 1. Tụ Linh Trận: x2 Tu Vi
    public float CultivationMultiplier => SpiritActive ? 2f + (GetLevel(FormationId.Spirit) - 1) * 0.05f : 1f;

    // 2. Băng Trận: Giảm tốc Boss
    public float BossSpeedMultiplier => Selected == FormationId.Frost && GetLevel(FormationId.Frost) > 0
        ? Mathf.Max(0.25f, 0.5f - (GetLevel(FormationId.Frost) - 1) * 0.01f) : 1f;

    // 3. Vạn Kiếm Trận: Tăng tốc độ bay của kiếm
    public float SwordSpeedMultiplier => Selected == FormationId.SwordRain && GetLevel(FormationId.SwordRain) > 0
        ? 1.20f + (GetLevel(FormationId.SwordRain) - 1) * 0.015f : 1f;

    // 4. Huyết Ma Trận: Hút máu chuyển thành HP
    public float LifestealBonus => Selected == FormationId.BloodDrain && GetLevel(FormationId.BloodDrain) > 0
        ? Mathf.Min(0.20f, 0.05f + (GetLevel(FormationId.BloodDrain) - 1) * 0.005f) : 0f;

    // 5. Kim Cang Trận: Giảm sát thương nhận vào
    public float DamageReduction => Selected == FormationId.GoldenBell && GetLevel(FormationId.GoldenBell) > 0
        ? Mathf.Min(0.40f, 0.15f + (GetLevel(FormationId.GoldenBell) - 1) * 0.01f) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        // Xử lý Lôi Trận
        if (Selected == FormationId.Thunder && GetLevel(FormationId.Thunder) > 0)
        {
            if (Time.time >= nextThunderTime)
            {
                nextThunderTime = Time.time + Mathf.Max(3.5f, 8f - (GetLevel(FormationId.Thunder) - 1) * 0.15f);
                PulseThunder();
            }
        }

        // Xử lý Vạn Kiếm Trận (Mưa kiếm quét quái định kỳ)
        if (Selected == FormationId.SwordRain && GetLevel(FormationId.SwordRain) > 0)
        {
            if (Time.time >= nextSwordRainTime)
            {
                nextSwordRainTime = Time.time + Mathf.Max(4f, 9f - (GetLevel(FormationId.SwordRain) - 1) * 0.18f);
                PulseSwordRain();
            }
        }
    }

    public int GetLevel(FormationId id) => Valid(id) ? State.levels[(int)id] : 0;

    public int GetOreCost(FormationId id)
    {
        if (!Valid(id) || GetLevel(id) >= MaxLevel) return int.MaxValue;
        int nextLv = GetLevel(id) + 1;
        return 3 * nextLv * nextLv;
    }

    public float GetStoneCost(FormationId id)
    {
        if (!Valid(id) || GetLevel(id) >= MaxLevel) return float.PositiveInfinity;
        return Mathf.Ceil(700f * Mathf.Pow(1.45f, GetLevel(id)));
    }

    public bool TryUpgrade(FormationId id)
    {
        if (!Valid(id) || GetLevel(id) >= MaxLevel || ArtifactService.Instance == null || ArtifactService.Instance.Ore < GetOreCost(id)) return false;
        var economy = EconomyManager.Instance;
        if (economy == null || !economy.SpendLinhThach(GetStoneCost(id))) return false;
        ArtifactService.Instance.SpendOre(GetOreCost(id));
        State.levels[(int)id]++;
        OnChanged?.Invoke();
        return true;
    }

    public bool TrySelect(FormationId id)
    {
        if (!Valid(id) || GetLevel(id) == 0) return false;
        State.selectedFormation = (int)id;
        nextThunderTime = Time.time + 5f;
        nextSwordRainTime = Time.time + 4f;
        OnChanged?.Invoke();
        return true;
    }

    public bool TryActivateSpirit()
    {
        if (Selected != FormationId.Spirit || GetLevel(FormationId.Spirit) == 0 || SpiritCooldown > TimeSpan.Zero) return false;
        DateTime now = DateTime.UtcNow;
        State.spiritEndUtcTicks = now.AddSeconds(30 + (GetLevel(FormationId.Spirit) - 1) * 2).Ticks;
        State.spiritReadyUtcTicks = now.AddSeconds(120).Ticks;
        OnChanged?.Invoke();
        return true;
    }

    private void PulseThunder()
    {
        var combat = CombatManager.Instance;
        if (combat == null || combat.playerTransform == null || combat.isPlayerDead) return;
        float radius = 10f + GetLevel(FormationId.Thunder) * 0.25f;
        float damage = 40f * GetLevel(FormationId.Thunder) * (1f + (CultivationManager.Instance?.currentStageIndex ?? 0) * 0.15f);
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.isDead || enemy.isBoss) continue;
            if ((enemy.transform.position - combat.playerTransform.position).sqrMagnitude <= radius * radius)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void PulseSwordRain()
    {
        var combat = CombatManager.Instance;
        if (combat == null || combat.playerTransform == null || combat.isPlayerDead) return;
        float damage = 30f * GetLevel(FormationId.SwordRain) * (1f + (CultivationManager.Instance?.currentStageIndex ?? 0) * 0.18f);
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.isDead) continue;
            enemy.TakeDamage(damage);
        }
        if (GameLogger.Instance != null)
            GameLogger.Instance.Log("🗡️ Vạn Kiếm Trận phát động bão kiếm diệt địch!", new Color(0.4f, 0.8f, 1f));
    }

    public void OnSwordHitEnemy(float finalDamage)
    {
        // Kích hoạt hút máu từ Huyết Ma Trận
        if (LifestealBonus > 0f && finalDamage > 0f)
        {
            float heal = finalDamage * LifestealBonus;
            CombatManager.Instance?.HealPlayer(heal);
        }
    }

    private static bool Valid(FormationId id) => id >= FormationId.Thunder && id <= FormationId.GoldenBell;

    public void SaveToSlot(CharacterSaveData slot) { if (slot != null) slot.formations = State; }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        State = slot?.formations ?? new FormationSaveState();
        if (State.levels == null || State.levels.Length != 6)
        {
            int[] newLvls = new int[6];
            newLvls[0] = 1; // Lôi Trận mặc định mở khóa
            if (State.levels != null)
            {
                for (int i = 0; i < State.levels.Length && i < 6; i++)
                    newLvls[i] = State.levels[i];
            }
            State.levels = newLvls;
        }

        for (int i = 0; i < State.levels.Length; i++)
            State.levels[i] = Mathf.Clamp(State.levels[i], 0, MaxLevel);

        State.selectedFormation = Mathf.Clamp(State.selectedFormation, 0, 5);
        if (GetLevel(Selected) == 0) State.selectedFormation = 0;

        nextThunderTime = Time.time + 6f;
        nextSwordRainTime = Time.time + 5f;
        OnChanged?.Invoke();
    }
}

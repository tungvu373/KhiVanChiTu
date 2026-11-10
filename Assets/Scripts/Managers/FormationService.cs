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

    public bool IsFormationActive(FormationId id)
    {
        if (Selected != id || GetLevel(id) == 0) return false;
        return FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFormationUnlocked(id);
    }

    // 1. Tụ Linh Trận: x2 Tu Vi
    public float CultivationMultiplier => SpiritActive && (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFormationUnlocked(FormationId.Spirit))
        ? 2f + (GetLevel(FormationId.Spirit) - 1) * 0.05f : 1f;

    // 2. Băng Trận: Giảm tốc Boss
    public float BossSpeedMultiplier => IsFormationActive(FormationId.Frost)
        ? Mathf.Max(0.25f, 0.5f - (GetLevel(FormationId.Frost) - 1) * 0.01f) : 1f;

    // 3. Vạn Kiếm Trận: Tăng tốc độ bay của kiếm
    public float SwordSpeedMultiplier => IsFormationActive(FormationId.SwordRain)
        ? 1.20f + (GetLevel(FormationId.SwordRain) - 1) * 0.015f : 1f;

    // 4. Huyết Ma Trận: Hút máu chuyển thành HP
    public float LifestealBonus => IsFormationActive(FormationId.BloodDrain)
        ? Mathf.Min(0.20f, 0.05f + (GetLevel(FormationId.BloodDrain) - 1) * 0.005f) : 0f;

    // 5. Kim Cang Trận: Giảm sát thương nhận vào
    public float DamageReduction => IsFormationActive(FormationId.GoldenBell)
        ? Mathf.Min(0.40f, 0.15f + (GetLevel(FormationId.GoldenBell) - 1) * 0.01f) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDisable()
    {
        if (HitVFXManager.HasInstance)
        {
            HitVFXManager.Instance.DisableFormationVisuals();
        }
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        var combat = CombatManager.Instance;

        // Xử lý Lôi Trận (chỉ kích hoạt khi đã mở khóa mốc Trúc Cơ 1)
        if (IsFormationActive(FormationId.Thunder))
        {
            if (Time.time >= nextThunderTime)
            {
                float interval = Mathf.Max(3.5f, 8f - (GetLevel(FormationId.Thunder) - 1) * 0.15f);
                nextThunderTime = Time.time + interval * DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.FormationCooldown);
                PulseThunder();
            }
        }

        // Xử lý Vạn Kiếm Trận (Mưa kiếm quét quái định kỳ, chỉ khi đạt Kim Đan 1)
        if (IsFormationActive(FormationId.SwordRain))
        {
            if (Time.time >= nextSwordRainTime)
            {
                float interval = 9f - (GetLevel(FormationId.SwordRain) - 1) * 0.18f;
                float multiplier = DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.SwordRainCooldownReduction);
                nextSwordRainTime = Time.time + Mathf.Max(4f, interval * multiplier);
                PulseSwordRain();
            }
        }

        // Duy trì hiệu ứng Trận Pháp Tu Tiên Toàn Diện (Prompt 04, 05)
        bool playerPresent = combat != null && combat.playerTransform != null && !combat.isPlayerDead;
        bool canShowAura = FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.IsFormationUnlocked(Selected);
        var vfx = HitVFXManager.Instance;
        if (playerPresent && canShowAura && GetLevel(Selected) > 0)
        {
            vfx?.UpdateFormationVisuals(Selected, SpiritActive, combat.playerTransform.position);
            vfx?.SetBaguaSustain(SpiritActive, combat.playerTransform.position);
        }
        else
        {
            vfx?.DisableFormationVisuals();
            return;
        }

        // Duy trì hiệu ứng Cực Hàn Băng Trận sát chân Boss (Prompt 05)
        bool frostActive = IsFormationActive(FormationId.Frost);
        var boss = combat != null ? combat.CurrentBoss : null;
        if (frostActive && boss != null && !boss.isDead && boss.isActiveAndEnabled)
        {
            vfx?.SetFrostMist(true, boss.transform.position);
        }
        else
        {
            vfx?.SetFrostMist(false, Vector3.zero);
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
        return Mathf.Ceil(700f * Mathf.Pow(1.45f, GetLevel(id)) * DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.FormationCostReduction));
    }

    public bool TryUpgrade(FormationId id)
    {
        if (!Valid(id) || GetLevel(id) >= MaxLevel || ArtifactService.Instance == null || ArtifactService.Instance.Ore < GetOreCost(id)) return false;
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsFormationUnlocked(id)) return false;
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
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsFormationUnlocked(id)) return false;
        State.selectedFormation = (int)id;
        nextThunderTime = Time.time + 5f;
        nextSwordRainTime = Time.time + 4f;
        OnChanged?.Invoke();
        return true;
    }

    public bool TryActivateSpirit()
    {
        if (Selected != FormationId.Spirit || GetLevel(FormationId.Spirit) == 0 || SpiritCooldown > TimeSpan.Zero) return false;
        if (FeatureUnlockService.Instance != null && !FeatureUnlockService.Instance.IsFormationUnlocked(FormationId.Spirit)) return false;
        DateTime now = DateTime.UtcNow;
        float duration = 30 + (GetLevel(FormationId.Spirit) - 1) * 2 + DaoAnProgressionService.Bonus(DaoAnEffect.FormationDuration);
        State.spiritEndUtcTicks = now.AddSeconds(duration).Ticks;
        State.spiritReadyUtcTicks = now.AddSeconds(120f * DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.SpiritCooldownReduction)).Ticks;

        if (CombatManager.Instance != null && CombatManager.Instance.playerTransform != null)
        {
            HitVFXManager.Instance?.SpawnFormationRuneRing(CombatManager.Instance.playerTransform.position, FormationGlyphVFX.Palette(FormationId.Spirit), 3.2f);
        }

        OnChanged?.Invoke();
        return true;
    }

    private void PulseThunder()
    {
        var combat = CombatManager.Instance;
        if (combat == null || combat.playerTransform == null || combat.isPlayerDead) return;
        float radius = 10f + GetLevel(FormationId.Thunder) * 0.25f;
        float damage = 40f * GetLevel(FormationId.Thunder) * (1f + (CultivationManager.Instance?.currentStageIndex ?? 0) * 0.15f);

        HitVFXManager.Instance?.SpawnFormationRuneRing(combat.playerTransform.position, FormationGlyphVFX.Palette(FormationId.Thunder), 2.8f);

        int strikesSpawned = 0;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.isDead || enemy.isBoss) continue;
            if ((enemy.transform.position - combat.playerTransform.position).sqrMagnitude <= radius * radius)
            {
                enemy.TakeDamage(damage);
                if (strikesSpawned < 6)
                {
                    HitVFXManager.Instance?.SpawnFormationThunder(enemy.transform.position);
                    strikesSpawned++;
                }
            }
        }
    }

    private void PulseSwordRain()
    {
        var combat = CombatManager.Instance;
        if (combat == null || combat.playerTransform == null || combat.isPlayerDead) return;
        float damage = 30f * GetLevel(FormationId.SwordRain) * (1f + (CultivationManager.Instance?.currentStageIndex ?? 0) * 0.18f);

        HitVFXManager.Instance?.SpawnFormationRuneRing(combat.playerTransform.position, FormationGlyphVFX.Palette(FormationId.SwordRain), 3f);

        int swordsSpawned = 0;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.isDead) continue;
            enemy.TakeDamage(damage);
            if (swordsSpawned < 8)
            {
                HitVFXManager.Instance?.SpawnFormationSwordRain(enemy.transform.position);
                swordsSpawned++;
            }
        }
        if (GameLogger.Instance != null)
            GameLogger.Instance.Log("🗡️ Vạn Kiếm Trận phát động bão kiếm diệt địch!", new Color(0.4f, 0.8f, 1f));
    }

    public void OnSwordHitEnemy(float finalDamage, Vector3 hitPosition = default)
    {
        // Kích hoạt hút máu từ Huyết Ma Trận
        if (LifestealBonus > 0f && finalDamage > 0f)
        {
            float heal = finalDamage * LifestealBonus;
            CombatManager.Instance?.HealPlayer(heal);
            if (CombatManager.Instance != null && CombatManager.Instance.playerTransform != null && hitPosition != default)
            {
                HitVFXManager.Instance?.SpawnLifestealStreak(hitPosition, CombatManager.Instance.playerTransform.position);
            }
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

using System;
using UnityEngine;

/// <summary>Bốn pháp khí tăng tiến bằng quặng boss và Linh Thạch.</summary>
public class ArtifactService : MonoBehaviour
{
    public static ArtifactService Instance { get; private set; }
    public event Action OnChanged;
    public ArtifactSaveState State { get; private set; } = new ArtifactSaveState();
    public const int MaxLevel = 30;
    public int Ore => State.ore;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void AddOre(int amount)
    {
        if (amount <= 0) return;
        State.ore = Mathf.Min(1000000, State.ore + amount);
        OnChanged?.Invoke();
    }

    public bool SpendOre(int amount)
    {
        if (amount < 0 || State.ore < amount) return false;
        State.ore -= amount;
        OnChanged?.Invoke();
        return true;
    }

    public void OnEnemyDefeated(bool boss, int combatStage)
    {
        if (boss) AddOre(2 + combatStage / 10);
        else if (UnityEngine.Random.value < 0.015f) AddOre(1);
    }

    public int GetLevel(ArtifactSlot slot) => Valid(slot) ? State.levels[(int)slot] : 0;
    public int GetOreCost(ArtifactSlot slot)
    {
        int next = GetLevel(slot) + 1;
        return Valid(slot) && next <= MaxLevel ? next * next * (1 + (int)slot) : int.MaxValue;
    }
    public float GetStoneCost(ArtifactSlot slot)
    {
        int next = GetLevel(slot) + 1;
        return Valid(slot) && next <= MaxLevel ? Mathf.Ceil(400f * Mathf.Pow(1.48f, next - 1)) : float.PositiveInfinity;
    }

    public bool TryUpgrade(ArtifactSlot slot)
    {
        if (!Valid(slot) || GetLevel(slot) >= MaxLevel || State.ore < GetOreCost(slot)) return false;
        var economy = EconomyManager.Instance;
        if (economy == null || !economy.SpendLinhThach(GetStoneCost(slot))) return false;
        SpendOre(GetOreCost(slot));
        State.levels[(int)slot]++;
        CombatManager.Instance?.RecalculateMaxHp(false);
        OnChanged?.Invoke();
        return true;
    }

    public float MaxHpBonus => GetLevel(ArtifactSlot.Robe) * 100f;
    public float BossDamageReduction => Mathf.Min(0.30f, GetLevel(ArtifactSlot.Robe) * 0.01f);
    public float HpRegenPerSec => GetLevel(ArtifactSlot.Crown) * 2f;
    public float Tenacity => Mathf.Min(0.5f, GetLevel(ArtifactSlot.Crown) * 0.015f);
    public float DodgeChance => Mathf.Min(0.3f, GetLevel(ArtifactSlot.Boots) * 0.01f);
    public float SwordSpeedMultiplier => 1f + GetLevel(ArtifactSlot.Boots) * 0.012f;
    public float CritChanceBonus => Mathf.Min(0.25f, GetLevel(ArtifactSlot.Amulet) * 0.006f);
    public float StoneDropMultiplier => 1f + GetLevel(ArtifactSlot.Amulet) * 0.02f;
    public int AuraTier
    {
        get
        {
            int total = 0;
            foreach (int level in State.levels) total += level / 3;
            return Mathf.Min(9, total);
        }
    }

    private static bool Valid(ArtifactSlot slot) => slot >= ArtifactSlot.Robe && slot <= ArtifactSlot.Amulet;
    public void SaveToSlot(CharacterSaveData slot) { if (slot != null) slot.artifacts = State; }
    public void LoadFromSlot(CharacterSaveData slot)
    {
        State = slot?.artifacts ?? new ArtifactSaveState();
        if (State.levels == null || State.levels.Length != 4) State.levels = new int[4];
        State.ore = Mathf.Max(0, State.ore);
        for (int i = 0; i < State.levels.Length; i++) State.levels[i] = Mathf.Clamp(State.levels[i], 0, MaxLevel);
        OnChanged?.Invoke();
    }
}

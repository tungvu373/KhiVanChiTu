using System;
using UnityEngine;

/// <summary>Kho dược liệu, lò luyện theo thời gian thực và đan dược riêng cho từng cảnh giới.</summary>
public class AlchemyService : MonoBehaviour
{
    public static AlchemyService Instance { get; private set; }
    public event Action OnChanged;
    public AlchemySaveState State { get; private set; } = new AlchemySaveState();
    public PillQuality ActiveTribulationQuality { get; private set; } = PillQuality.Waste;
    public int AlchemistRank => Mathf.Min(10, State.masteryXp / 20);
    public bool IsBrewing => State.brewingRecipe >= 0 && State.brewingRecipe <= 1;
    public TimeSpan BrewRemaining => IsBrewing
        ? TimeSpan.FromTicks(Math.Max(0L, State.brewEndUtcTicks - DateTime.UtcNow.Ticks))
        : TimeSpan.Zero;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void Update() { if (IsBrewing && BrewRemaining == TimeSpan.Zero) CompleteBrew(); }

    public void AddHerb(int herb, int amount)
    {
        if (herb < 0 || herb >= 3 || amount <= 0) return;
        State.herbs[herb] = Mathf.Min(1000000, State.herbs[herb] + amount);
        OnChanged?.Invoke();
    }

    public int GetHerbCount(int herb) => herb >= 0 && herb < 3 ? State.herbs[herb] : 0;

    public int[] GetRecipeCosts(PillRecipe recipe, int realm)
    {
        if (realm < 0 || realm >= 9 || (int)recipe < 0 || (int)recipe > 1) return new int[3];
        return new[]
        {
            2 + realm + (recipe == PillRecipe.Breakthrough ? 2 : 0),
            1 + realm / 2 + (recipe == PillRecipe.Breakthrough ? 1 : 0),
            recipe == PillRecipe.Breakthrough ? 1 + realm / 3 : 0
        };
    }

    public void OnEnemyDefeated(bool boss, int stage)
    {
        if (boss)
        {
            AddHerb(0, 2 + stage / 20);
            AddHerb(1, 1 + stage / 30);
            AddHerb(2, 1 + stage / 40);
        }
        else if (UnityEngine.Random.value < 0.18f)
            AddHerb(UnityEngine.Random.value < 0.8f ? 0 : 1, 1);
    }

    public bool TryStartBrew(PillRecipe recipe, int realm)
    {
        if (IsBrewing || realm < 0 || realm >= 9 || (int)recipe < 0 || (int)recipe > 1) return false;
        if (CultivationManager.Instance != null && realm > (int)CultivationManager.Instance.CurrentRealm) return false;
        int[] costs = GetRecipeCosts(recipe, realm);
        for (int i = 0; i < costs.Length; i++)
            if (State.herbs[i] < costs[i]) return false;
        for (int i = 0; i < costs.Length; i++) State.herbs[i] -= costs[i];
        State.brewingRecipe = (int)recipe;
        State.brewingRealm = realm;
        State.brewEndUtcTicks = DateTime.UtcNow.AddSeconds(Math.Max(10, 30 + realm * 15 - AlchemistRank * 2)).Ticks;
        OnChanged?.Invoke();
        return true;
    }

    public bool TryUseDustToFinish()
    {
        if (!IsBrewing || State.medicineDust < 10) return false;
        State.medicineDust -= 10;
        State.brewEndUtcTicks = DateTime.UtcNow.Ticks;
        CompleteBrew();
        return true;
    }

    public PillQuality CompleteBrew()
    {
        if (!IsBrewing || BrewRemaining > TimeSpan.Zero) return PillQuality.Waste;
        float roll = UnityEngine.Random.value;
        float supremeChance = Mathf.Min(0.10f, 0.04f + AlchemistRank * 0.004f);
        float wasteChance = Mathf.Max(0.08f, 0.25f - AlchemistRank * 0.015f);
        PillQuality quality = State.craftsWithoutSupreme >= 29 || roll < supremeChance
            ? PillQuality.Supreme : roll < supremeChance + 0.25f
                ? PillQuality.Refined : roll < 1f - wasteChance
                    ? PillQuality.Basic : PillQuality.Waste;
        if (quality == PillQuality.Waste) State.medicineDust += 2;
        else State.pills[PillIndex(State.brewingRealm, (PillRecipe)State.brewingRecipe, quality)]++;
        State.craftsWithoutSupreme = quality == PillQuality.Supreme ? 0 : State.craftsWithoutSupreme + 1;
        State.masteryXp += 1 + (int)quality;
        State.brewingRecipe = -1;
        State.brewEndUtcTicks = 0;
        OnChanged?.Invoke();
        if (quality > PillQuality.Waste)
            AchievementManager.Instance?.OnPillBrewed(quality);
        return quality;
    }

    public int GetPillCount(int realm, PillRecipe recipe, PillQuality quality)
    {
        return realm >= 0 && realm < 9 && (int)recipe >= 0 && (int)recipe <= 1 && quality >= PillQuality.Basic && quality <= PillQuality.Supreme
            ? State.pills[PillIndex(realm, recipe, quality)] : 0;
    }

    public bool TryUseCultivationPill(int realm, PillQuality quality)
    {
        var cultivation = CultivationManager.Instance;
        if (cultivation == null || realm != (int)cultivation.CurrentRealm || cultivation.IsTuViMaxed()) return false;
        int index = PillIndex(realm, PillRecipe.Cultivation, quality);
        if (quality < PillQuality.Basic || quality > PillQuality.Supreme || realm < 0 || realm >= 9 || State.pills[index] <= 0) return false;
        var stage = cultivation.GetCurrentStageData();
        if (stage == null) return false;
        State.pills[index]--;
        cultivation.AddTuVi(stage.requiredTuVi * 0.08f * QualityMultiplier(quality));
        OnChanged?.Invoke();
        return true;
    }

    public bool HasBreakthroughPill(int realm)
    {
        if (realm < 0 || realm >= 9) return false;
        for (int q = (int)PillQuality.Basic; q <= (int)PillQuality.Supreme; q++)
        {
            if (State.pills[PillIndex(realm, PillRecipe.Breakthrough, (PillQuality)q)] > 0)
                return true;
        }
        return false;
    }

    public PillQuality GetHighestBreakthroughPillQuality(int realm)
    {
        if (realm < 0 || realm >= 9) return PillQuality.Waste;
        for (int q = (int)PillQuality.Supreme; q >= (int)PillQuality.Basic; q--)
        {
            if (State.pills[PillIndex(realm, PillRecipe.Breakthrough, (PillQuality)q)] > 0)
                return (PillQuality)q;
        }
        return PillQuality.Waste;
    }

    public bool TryConsumeBreakthroughPill(int realm, PillQuality preferredQuality = PillQuality.Supreme)
    {
        if (realm < 0 || realm >= 9) return false;
        if (preferredQuality >= PillQuality.Basic && preferredQuality <= PillQuality.Supreme)
        {
            int prefIndex = PillIndex(realm, PillRecipe.Breakthrough, preferredQuality);
            if (State.pills[prefIndex] > 0)
            {
                State.pills[prefIndex]--;
                ActiveTribulationQuality = preferredQuality;
                OnChanged?.Invoke();
                return true;
            }
        }
        for (int q = (int)PillQuality.Supreme; q >= (int)PillQuality.Basic; q--)
        {
            int index = PillIndex(realm, PillRecipe.Breakthrough, (PillQuality)q);
            if (State.pills[index] <= 0) continue;
            State.pills[index]--;
            ActiveTribulationQuality = (PillQuality)q;
            OnChanged?.Invoke();
            return true;
        }
        return false;
    }

    public float TribulationDamageMultiplier => ActiveTribulationQuality switch
    {
        PillQuality.Supreme => 1.50f,
        PillQuality.Refined => 1.30f,
        PillQuality.Basic   => 1.15f,
        _                   => 1.00f
    };
    public void EndTribulation() { ActiveTribulationQuality = PillQuality.Waste; }
    public static float QualityMultiplier(PillQuality quality) => quality == PillQuality.Supreme ? 1.5f : quality == PillQuality.Refined ? 1.25f : 1f;
    private static int PillIndex(int realm, PillRecipe recipe, PillQuality quality) => (realm * 2 + (int)recipe) * 4 + (int)quality;

    public void SaveToSlot(CharacterSaveData slot) { if (slot != null) slot.alchemy = State; }
    public void LoadFromSlot(CharacterSaveData slot)
    {
        State = slot?.alchemy ?? new AlchemySaveState();
        if (State.herbs == null || State.herbs.Length != 3) State.herbs = new int[3];
        if (State.pills == null || State.pills.Length != 72) State.pills = new int[72];
        if (State.brewingRecipe < -1 || State.brewingRecipe > 1 || State.brewingRealm < 0 || State.brewingRealm >= 9)
            State.brewingRecipe = -1;
        ActiveTribulationQuality = PillQuality.Waste;
        if (IsBrewing && BrewRemaining == TimeSpan.Zero) CompleteBrew();
        OnChanged?.Invoke();
    }
}

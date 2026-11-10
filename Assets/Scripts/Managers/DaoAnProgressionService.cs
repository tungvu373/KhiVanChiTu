using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Per-character permanent upgrades. Catalog assets contain rules; saves contain only purchased levels.</summary>
public class DaoAnProgressionService : MonoBehaviour
{
    public static DaoAnProgressionService Instance { get; private set; }
    public DaoAnCatalog Catalog { get; private set; }
    public event Action OnChanged;

    private readonly Dictionary<string, int> levels = new Dictionary<string, int>(StringComparer.Ordinal);
    private readonly Dictionary<DaoAnEffect, float> bonuses = new Dictionary<DaoAnEffect, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        Catalog = Resources.Load<DaoAnCatalog>("Progression/DaoAnCatalog");
        if (Catalog == null) Debug.LogWarning("[DaoAn] Missing Resources/Progression/DaoAnCatalog asset.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public int GetLevel(string id) => !string.IsNullOrEmpty(id) && levels.TryGetValue(id, out int level) ? level : 0;

    public bool CanPurchase(string id)
    {
        if (Catalog == null || EconomyManager.Instance == null) return false;
        DaoAnDefinition node = Catalog.Find(id);
        if (node == null || node.MaxLevel == 0) return false;
        int current = GetLevel(id);
        if (current >= node.MaxLevel) return false;
        if (!string.IsNullOrEmpty(node.prerequisiteId) &&
            (node.prerequisiteId == id || GetLevel(node.prerequisiteId) < Math.Max(1, node.prerequisiteLevel))) return false;

        // Cổng tu vi cho từng cấp nâng cấp
        if (node.minRealms != null && current < node.minRealms.Length)
        {
            RealmId reqRealm = node.minRealms[current];
            int reqSub = (node.minSubLevels != null && current < node.minSubLevels.Length) ? node.minSubLevels[current] : 1;
            if (!FeatureUnlockService.IsCultivationReached(reqRealm, reqSub)) return false;
        }

        if (node.costs == null || current >= node.costs.Length) return false;
        float cost = node.costs[current];
        return !float.IsNaN(cost) && !float.IsInfinity(cost) && cost >= 0f &&
            EconomyManager.Instance.currentLinhThach >= cost;
    }

    public bool TryPurchase(string id)
    {
        if (!CanPurchase(id))
        {
            Debug.LogWarning($"[DaoAn] Không thể nâng cấp node '{id}' (CanPurchase=false, LinhThach={EconomyManager.Instance?.currentLinhThach})");
            return false;
        }
        DaoAnDefinition node = Catalog.Find(id);
        int current = GetLevel(id);
        if (!EconomyManager.Instance.SpendLinhThach(node.costs[current])) return false;
        levels[id] = current + 1;
        RefreshBonuses();
        OnChanged?.Invoke();
        if (DataManager.Instance != null && DataManager.Instance.isSessionActive)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }
        return true;
    }

    public static float Bonus(DaoAnEffect effect) => Instance != null ? Instance.GetBonus(effect) : 0f;

    public static float ReductionMultiplier(DaoAnEffect effect, DaoAnEffect? extraEffect = null)
    {
        float reduction = Bonus(effect);
        if (extraEffect.HasValue) reduction += Bonus(extraEffect.Value);
        return 1f - Mathf.Clamp(reduction, 0f, 0.9f);
    }

    public float GetMultiplier(DaoAnEffect effect) => 1f + GetBonus(effect);

    private float GetBonus(DaoAnEffect effect) => bonuses.TryGetValue(effect, out var bonus) ? bonus : 0f;

    private void RefreshBonuses()
    {
        bonuses.Clear();
        if (Catalog == null || Catalog.nodes == null) return;
        foreach (var node in Catalog.nodes)
        {
            if (node == null) continue;
            int level = GetLevel(node.id);
            if (level > 0) bonuses[node.effect] = GetBonus(node.effect) + node.GetTotalBonus(level);
        }
    }

    public void SaveToSlot(CharacterSaveData slot)
    {
        if (slot == null) return;
        var entries = new List<DaoAnLevelState>(levels.Count);
        foreach (var pair in levels)
            if (pair.Value > 0) entries.Add(new DaoAnLevelState { id = pair.Key, level = pair.Value });
        entries.Sort((a, b) => StringComparer.Ordinal.Compare(a.id, b.id));
        slot.daoAn = new DaoAnSaveState { levels = entries.ToArray() };
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        levels.Clear();
        if (slot?.daoAn?.levels != null)
        {
            foreach (var entry in slot.daoAn.levels)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id) || entry.level <= 0) continue;
                // Keep progress for catalog entries absent from this build.
                var node = Catalog?.Find(entry.id);
                int level = node == null ? entry.level : Mathf.Min(entry.level, node.MaxLevel);
                levels[entry.id] = Math.Max(level, GetLevel(entry.id));
            }
        }
        RefreshBonuses();
        OnChanged?.Invoke();
    }
}

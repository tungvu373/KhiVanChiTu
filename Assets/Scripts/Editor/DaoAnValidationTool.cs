#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DaoAnValidationTool
{
    [MenuItem("TuTien/Progression/Run Isolated Play QA")]
    public static void RunIsolatedPlayQa()
    {
        if (!Application.isPlaying || DataManager.Instance == null ||
            DataManager.Instance.SaveProfileId != "codex-daoan-qa")
            throw new InvalidOperationException("Run only in Play Mode on the isolated codex-daoan-qa scene.");

        var data = DataManager.Instance;
        var economy = EconomyManager.Instance;
        var service = DaoAnProgressionService.Instance;
        Debug.Log("[DaoAnQA] managersRoot=" + (GameObject.Find("Managers") != null) +
            ", economyObjects=" + Resources.FindObjectsOfTypeAll<EconomyManager>().Length +
            ", gameManager=" + (GameManager.Instance != null) +
            ", achievementManager=" + (AchievementManager.Instance != null));
        if (economy == null || service == null || service.Catalog == null || DaoAnModalUI.Instance == null)
            throw new InvalidOperationException("Dao An runtime services are missing: economy=" + (economy != null) +
                ", service=" + (service != null) + ", catalog=" + (service != null && service.Catalog != null) +
                ", modal=" + (DaoAnModalUI.Instance != null));
        var cultivation = CultivationManager.Instance ?? throw new InvalidOperationException("Cultivation service is missing.");
        data.gameSaveData.slots[0] = new CharacterSaveData { isSlotEmpty = false, characterName = "DaoAnQA" };
        data.gameSaveData.lastPlayedSlotIndex = 0;
        data.ActivateSession();
        service.LoadFromSlot(data.gameSaveData.slots[0]);
        economy.currentLinhThach = 10000f;
        cultivation.SetStageAndTuViForEditor(0, 0f);
        if (service.CanPurchase("sword") || !service.TryPurchase("wealth") || service.CanPurchase("repose"))
            throw new InvalidOperationException("Dao An realm gate failed.");
        cultivation.SetStageAndTuViForEditor(8, 0f); // Luyện Khí 7
        if (!service.TryPurchase("repose") || service.CanPurchase("sword") ||
            !service.TryPurchase("wealth") || !service.TryPurchase("sword"))
            throw new InvalidOperationException("Dao An purchase prerequisite failed.");
        if (Mathf.Abs(economy.currentLinhThach - 5000f) > 0.01f ||
            Mathf.Abs(service.GetMultiplier(DaoAnEffect.MonsterLinhThach) - 1.10f) > 0.001f ||
            Mathf.Abs(service.GetMultiplier(DaoAnEffect.OfflineLinhThach) - 1.05f) > 0.001f ||
            Mathf.Abs(service.GetMultiplier(DaoAnEffect.SwordDamage) - 1.03f) > 0.001f)
            throw new InvalidOperationException("Dao An wallet or modifier result failed.");
        var slot = data.gameSaveData.slots[0];
        if (slot.daoAn?.levels == null || slot.daoAn.levels.Length != 3)
            throw new InvalidOperationException("Dao An state was not saved to the active slot.");
        service.LoadFromSlot(slot);
        if (service.GetLevel("wealth") != 2 || service.GetLevel("repose") != 1 || service.GetLevel("sword") != 1)
            throw new InvalidOperationException("Dao An state did not round-trip.");
        DaoAnModalUI.Instance.Open();
        if (!DaoAnModalUI.Instance.IsModalOpen)
            throw new InvalidOperationException("Dao An modal did not open.");
        Debug.Log("[DaoAn] Isolated play QA passed: locked purchase, spend, effects, save/load and modal.");
    }

    [MenuItem("TuTien/Progression/Validate Vận Khí")]
    public static void Validate()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<DaoAnCatalog>(
            "Assets/Resources/Progression/DaoAnCatalog.asset");
        if (catalog == null || catalog.nodes == null || catalog.nodes.Length == 0)
            throw new InvalidOperationException("Dao An catalog is missing or empty.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in catalog.nodes)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.id) || !ids.Add(node.id) || node.MaxLevel == 0)
                throw new InvalidOperationException("Invalid or duplicate Dao An node.");
            if (node.costs.Length != node.bonuses.Length || node.minRealms?.Length != node.MaxLevel || node.minSubLevels?.Length != node.MaxLevel)
                throw new InvalidOperationException("Incomplete Dao An levels: " + node.id);
            for (int i = 0; i < node.MaxLevel; i++)
                if (node.costs[i] <= 0 || float.IsNaN(node.costs[i]) || float.IsInfinity(node.costs[i]) ||
                    node.bonuses[i] < 0 || float.IsNaN(node.bonuses[i]) || float.IsInfinity(node.bonuses[i]) ||
                    !Enum.IsDefined(typeof(RealmId), node.minRealms[i]) || node.minSubLevels[i] < 1 || node.minSubLevels[i] > 10 ||
                    (i > 0 && !FeatureUnlockService.CompareCultivation(node.minRealms[i], node.minSubLevels[i], node.minRealms[i - 1], node.minSubLevels[i - 1])))
                    throw new InvalidOperationException("Invalid Dao An cost/bonus: " + node.id);
        }
        foreach (var node in catalog.nodes)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal) { node.id };
            var current = node;
            while (!string.IsNullOrEmpty(current.prerequisiteId))
            {
                var prerequisite = catalog.Find(current.prerequisiteId);
                if (prerequisite == null || !seen.Add(prerequisite.id) || current.prerequisiteLevel < 1 || current.prerequisiteLevel > prerequisite.MaxLevel)
                    throw new InvalidOperationException("Broken/cyclic Dao An prerequisite: " + node.id);
                current = prerequisite;
            }
        }

        // A v3 save must gain only an empty Dao An state: no legacy stage remap or pill grant.
        var old = new CharacterSaveData
        {
            isSlotEmpty = false, saveVersion = 3, realmId = 0, subLevel = 5,
            daoAn = null, alchemy = new AlchemySaveState()
        };
        if (!SaveMigration.Run(old) || old.saveVersion != SaveMigration.CURRENT_SAVE_VERSION || old.realmId != 0 || old.subLevel != 5 ||
            old.alchemy.pills[5] != 0 || old.daoAn?.levels == null || SaveMigration.Run(old))
            throw new InvalidOperationException("v3 -> v4 migration changed gameplay state or was not idempotent.");
        Debug.Log("[DaoAn] Validation passed: catalog, prerequisites, and non-destructive v3 migration.");
    }
}
#endif

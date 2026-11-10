using System;
using UnityEngine;

/// <summary>
/// Dịch vụ quản lý bảng rơi đồ (Loot Table & Reward Gates) tuân thủ chặt chẽ
/// KE_HOACH_TAI_CAU_TRUC_MO_KHOA_THEO_TU_VI.md mục 3.
/// Tách biệt nguồn thưởng quái/Boss, kiểm tra cổng tính năng TRƯỚC roll và trước khi cộng.
/// </summary>
public class LootRewardService : MonoBehaviour
{
    private static LootRewardService _instance;
    public static LootRewardService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<LootRewardService>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    /// <summary>
    /// Xử lý rơi đồ khi quái thường hoặc Boss bị tiêu diệt
    /// </summary>
    public void HandleEnemyDefeated(bool isBoss, int combatStage)
    {
        RealmId currentRealm = RealmId.LuyenKhi;
        int subLevel = 1;

        if (CultivationManager.Instance != null)
        {
            currentRealm = CultivationManager.Instance.CurrentRealm;
            subLevel = CultivationManager.Instance.CurrentSubLevel;
        }

        if (isBoss)
        {
            HandleBossLoot(currentRealm, subLevel, combatStage);
        }
        else
        {
            HandleRegularMonsterLoot(currentRealm, subLevel, combatStage);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. RƠI ĐỒ QUÁI THƯỜNG (REGULAR MONSTER LOOT)
    // ─────────────────────────────────────────────────────────────────────────

    private void HandleRegularMonsterLoot(RealmId realm, int subLevel, int combatStage)
    {
        // A. ROLL RƠI PHI KIẾM (Tối đa 1 kiếm trên mỗi quái thường)
        RollSwordDrop(realm, subLevel);

        // B. ROLL RƠI THẢO DƯỢC (Chỉ rơi từ Luyện Khí 7 trở đi)
        if (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropHerbs())
        {
            RollHerbDrop(realm, subLevel);
        }

        // C. ROLL RƠI QUẶNG TINH THIẾT (Chỉ rơi từ Trúc Cơ 1 trở đi)
        if (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropOre())
        {
            RollOreDrop(realm, subLevel);
        }
    }

    private void RollSwordDrop(RealmId realm, int subLevel)
    {
        float p1 = 0f, p2 = 0f, p3 = 0f, p4 = 0f;

        if (realm == RealmId.LuyenKhi)
        {
            if (subLevel <= 3)
            {
                p1 = 0.24f; // 24% Lv1
            }
            else if (subLevel <= 6)
            {
                p1 = 0.22f; // 22% Lv1
                p2 = 0.01f; // 1% Lv2
            }
            else
            {
                p1 = 0.19f; // 19% Lv1
                p2 = 0.02f; // 2% Lv2
            }
        }
        else if (realm == RealmId.TrucCo)
        {
            p1 = 0.13f;
            p2 = 0.04f;
            p3 = 0.005f;
        }
        else if (realm == RealmId.KimDan)
        {
            p1 = 0.10f;
            p2 = 0.05f;
            p3 = 0.01f;
        }
        else if (realm == RealmId.NguyenAnh)
        {
            p1 = 0.08f;
            p2 = 0.06f;
            p3 = 0.02f;
            p4 = 0.002f;
        }
        else // Hóa Thần trở lên
        {
            p1 = 0.06f;
            p2 = 0.06f;
            p3 = 0.03f;
            p4 = 0.005f;
        }

        float dropChance = Mathf.Clamp01((p1 + p2 + p3 + p4) * (1f + DaoAnProgressionService.Bonus(DaoAnEffect.SwordDropRate)));
        float roll = UnityEngine.Random.value;
        if (roll >= dropChance) return;

        // Redistribute only the tiers already present in this realm's drop table.
        float tierMultiplier = 1f + DaoAnProgressionService.Bonus(DaoAnEffect.SwordTierWeight);
        p2 *= tierMultiplier;
        p3 *= tierMultiplier;
        p4 *= tierMultiplier;
        float tierRoll = roll / dropChance * (p1 + p2 + p3 + p4);
        int droppedLevel = 1;
        if (tierRoll < p4) droppedLevel = 4;
        else if (tierRoll < p4 + p3) droppedLevel = 3;
        else if (tierRoll < p4 + p3 + p2) droppedLevel = 2;
        DeliverSword(droppedLevel, isMonsterDrop: true);
    }

    private void RollHerbDrop(RealmId realm, int subLevel)
    {
        if (AlchemyService.Instance == null) return;

        float pLinhChi = 0f;
        float pNgungKhi = 0f;
        float pTuyetLien = 0f;

        if (realm == RealmId.LuyenKhi)
        {
            if (subLevel >= 7)
            {
                pLinhChi = 0.08f;
                pNgungKhi = 0.04f;
                pTuyetLien = 0.004f;
            }
        }
        else if (realm == RealmId.TrucCo)
        {
            pLinhChi = 0.08f;
            pNgungKhi = 0.05f;
            pTuyetLien = 0.008f;
        }
        else if (realm == RealmId.KimDan)
        {
            pLinhChi = 0.07f;
            pNgungKhi = 0.05f;
            pTuyetLien = 0.01f;
        }
        else if (realm == RealmId.NguyenAnh)
        {
            pLinhChi = 0.07f;
            pNgungKhi = 0.06f;
            pTuyetLien = 0.015f;
        }
        else // Hóa Thần trở lên
        {
            pLinhChi = 0.06f;
            pNgungKhi = 0.06f;
            pTuyetLien = 0.02f;
        }

        float multiplier = 1f + DaoAnProgressionService.Bonus(DaoAnEffect.HerbDropRate);
        if (pLinhChi > 0 && UnityEngine.Random.value < Mathf.Clamp01(pLinhChi * multiplier))
        {
            AlchemyService.Instance.AddHerb(0, 1);
            GameLogger.Instance?.Log("Thu thập: 1 Linh Chi", new Color(0.5f, 1f, 0.5f));
        }

        if (pNgungKhi > 0 && UnityEngine.Random.value < Mathf.Clamp01(pNgungKhi * multiplier))
        {
            AlchemyService.Instance.AddHerb(1, 1);
            GameLogger.Instance?.Log("Thu thập: 1 Cỏ Ngưng Khí", new Color(0.2f, 0.9f, 0.6f));
        }

        if (pTuyetLien > 0 && UnityEngine.Random.value < Mathf.Clamp01(pTuyetLien * multiplier))
        {
            AlchemyService.Instance.AddHerb(2, 1);
            GameLogger.Instance?.Log("Thu thập quý: 1 Tuyết Liên Tinh Khiết!", Color.cyan);
        }
    }

    private void RollOreDrop(RealmId realm, int subLevel)
    {
        if (ArtifactService.Instance == null) return;

        float pOre = 0f;

        if (realm == RealmId.TrucCo) pOre = 0.015f;
        else if (realm == RealmId.KimDan) pOre = 0.02f;
        else if (realm == RealmId.NguyenAnh) pOre = 0.025f;
        else if (realm >= RealmId.HoaThan) pOre = 0.03f;

        float multiplier = 1f + DaoAnProgressionService.Bonus(DaoAnEffect.OreDropRate);
        if (pOre > 0 && UnityEngine.Random.value < Mathf.Clamp01(pOre * multiplier))
        {
            ArtifactService.Instance.AddOre(1);
            GameLogger.Instance?.Log("Khai khoáng: 1 Quặng Tinh Thiết", new Color(1f, 0.75f, 0.2f));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. RƠI ĐỒ ĐẠI BOSS (BOSS LOOT TABLE)
    // ─────────────────────────────────────────────────────────────────────────

    private void HandleBossLoot(RealmId realm, int subLevel, int combatStage)
    {
        // 1. Boss luôn rớt 1 Kiếm Phôi chất lượng cao (Cấp 2 hoặc Cấp 3)
        int bossSwordLevel = 1;
        if (realm == RealmId.LuyenKhi)
        {
            bossSwordLevel = (subLevel >= 7) ? 2 : 1;
        }
        else if (realm == RealmId.TrucCo)
        {
            bossSwordLevel = 2;
        }
        else
        {
            bossSwordLevel = (UnityEngine.Random.value < 0.4f) ? 3 : 2;
        }
        DeliverSword(bossSwordLevel, isMonsterDrop: true);

        // 2. Dược liệu Boss: Chỉ rơi khi Luyện Đan đã mở (LK7+)
        if (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropHerbs())
        {
            if (AlchemyService.Instance != null)
            {
                int lc = 2 + combatStage / 20 + Mathf.FloorToInt(DaoAnProgressionService.Bonus(DaoAnEffect.BossHerbBonus));
                int nk = 1 + combatStage / 30;
                int tl = 1 + combatStage / 40;
                AlchemyService.Instance.AddHerb(0, lc);
                AlchemyService.Instance.AddHerb(1, nk);
                AlchemyService.Instance.AddHerb(2, tl);
                GameLogger.Instance?.Log($"Yêu Vương rơi bảo vật: +{lc} Linh Chi, +{nk} Cỏ NK, +{tl} Tuyết Liên!", Color.green);
            }
        }

        // 3. Quặng Boss: Chỉ rơi khi Pháp Trận đã mở (Trúc Cơ 1+)
        if (FeatureUnlockService.Instance == null || FeatureUnlockService.Instance.CanDropOre())
        {
            if (ArtifactService.Instance != null)
            {
                int oreAmount = 2 + combatStage / 10 + Mathf.FloorToInt(DaoAnProgressionService.Bonus(DaoAnEffect.BossOreDrop));
                ArtifactService.Instance.AddOre(oreAmount);
                GameLogger.Instance?.Log($"Yêu Vương rơi quặng quý: +{oreAmount} Quặng Tinh Thiết!", new Color(1f, 0.8f, 0.2f));
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. VẬN CHUYỂN KIẾM & GHI NHẬN THÀNH TỰU MONSTER DROP
    // ─────────────────────────────────────────────────────────────────────────

    private void DeliverSword(int level, bool isMonsterDrop)
    {
        string swordName = DismantleService.GetSwordDisplayName(level);

        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.HandleSwordDrop(level, isMonsterDrop);
        }
        else if (MergeManager.Instance != null)
        {
            bool added = MergeManager.Instance.TryAddSword(level);
            if (added && isMonsterDrop)
            {
                FeatureUnlockService.Instance?.OnMonsterSwordAcquired(level);
            }
        }

        GameLogger.Instance?.Log($"Trảm quái nhặt được: {swordName}!", Color.yellow);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public GameObject[] normalEnemyPrefabs;
    public GameObject[] bossEnemyPrefabs;
    public GameObject damagePopupPrefab; // Hiá»ƒn thá»‹ sá»‘ sÃ¡t thÆ°Æ¡ng
    public Transform playerTransform;

    private float spawnTimer;
    public float spawnInterval = 0.2f; // Spawn nhanh hÆ¡n (0.2s)
    
    private List<Enemy> activeEnemies = new List<Enemy>();
    private const int MAX_ENEMIES = 50;

    public float playerMaxHp = 1000f;
    public float playerCurrentHp = 1000f;
    private float playerVisualHp = 1000f; // DÃ¹ng Ä‘á»ƒ trÆ°á»£t mÆ°á»£t mÃ 
    
    [Header("UI References")]
    public UnityEngine.UI.Image playerHpFill;
    public UnityEngine.UI.Text playerHpText;

    [Header("Skill VFX Materials")]
    public Material magicArrayMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        playerMaxHp = 1000f;
        playerCurrentHp = 1000f;
        playerVisualHp = 1000f;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.OnStageChanged += HandleStageChanged;
            
            // TÃ­nh toÃ¡n láº§n Ä‘áº§u
            HandleStageChanged(null);
        }

        if (SimplePool.Instance != null)
        {
            // Pool toÃ n bá»™ quÃ¡i thÆ°á»ng
            if (normalEnemyPrefabs != null && normalEnemyPrefabs.Length > 0)
            {
                foreach (var prefab in normalEnemyPrefabs)
                {
                    if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, 20);
                }
            }
            
            // Pool toÃ n bá»™ boss
            if (bossEnemyPrefabs != null && bossEnemyPrefabs.Length > 0)
            {
                foreach (var prefab in bossEnemyPrefabs)
                {
                    if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, 5);
                }
            }

            if (damagePopupPrefab != null) SimplePool.Instance.AddPool("DamagePopup", damagePopupPrefab, 50);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.OnStageChanged -= HandleStageChanged;
        }
    }

    private void HandleStageChanged(CultivationStageData stageData)
    {
        if (CultivationManager.Instance != null)
        {
            float oldMaxHp = playerMaxHp;
            playerMaxHp = 1000f; // MÃ¡u máº·c Ä‘á»‹nh
            
            // Há»“i phá»¥c lÆ°á»£ng mÃ¡u chÃªnh lá»‡ch khi lÃªn cáº¥p
            if (playerCurrentHp > 0)
            {
                playerCurrentHp += (playerMaxHp - oldMaxHp);
            }
            
            UpdatePlayerHPUI();
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.BossTribulation)
        {
            ClearAllEnemies();
            SpawnBoss();
        }
    }

    private void ClearAllEnemies()
    {
        foreach (var e in activeEnemies)
        {
            if (e != null) e.gameObject.SetActive(false);
        }
        activeEnemies.Clear();
    }

    private Enemy currentBoss;

    private void SpawnBoss()
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0 || playerTransform == null) return;
        
        GameObject bossPrefab = bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Length)];
        
        // Sinh Boss trÃªn trá»¥c Z (phÃ­a trÆ°á»›c) vÃ  Ã©p cá»©ng tá»a Ä‘á»™ Y báº±ng vá»›i NhÃ¢n váº­t
        Vector3 spawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z + 8f);
        GameObject go = null;
        if (SimplePool.Instance != null) 
        {
            go = SimplePool.Instance.SpawnFromPool(bossPrefab.name, spawnPos, Quaternion.identity);
        }
        
        // Fallback nếu pool không có sẵn tag của Boss
        if (go == null) 
        {
            go = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
        
        // Bá» scale 3x vÃ¬ Boss Ä‘Ã£ cÃ³ kÃ­ch thÆ°á»›c chuáº©n 1.5 tá»« file Prefab
        // go.transform.localScale = Vector3.one * 3f; 
        
        BossEnemy boss = go.GetComponent<BossEnemy>();
        if (boss == null)
        {
            Debug.LogWarning($"[CombatManager] Prefab Boss '{bossPrefab.name}' chưa được gắn script BossEnemy! Tự động thay thế Enemy bằng BossEnemy.");
            Enemy oldEnemy = go.GetComponent<Enemy>();
            boss = go.AddComponent<BossEnemy>();
            
            if (oldEnemy != null)
            {
                // Copy các thông số Inspector để không bị mất
                boss.hpBarHeight = oldEnemy.hpBarHeight;
                boss.attackRange = oldEnemy.attackRange;
                boss.animator = oldEnemy.animator;
                Destroy(oldEnemy);
            }
        }
        
        currentBoss = boss;
        
        int stage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
        bool isTribulation = GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation;
        
        boss.InitBoss(stage, playerTransform, isTribulation);
        activeEnemies.Add(currentBoss);
    }

    private void Update()
    {
        // TrÆ°á»£t thanh mÃ¡u mÆ°á»£t mÃ  (Game Feel) - Xá»­ lÃ½ ngay cáº£ khi Ä‘Ã¡nh Boss
        if (playerHpFill != null)
        {
            playerVisualHp = Mathf.Lerp(playerVisualHp, playerCurrentHp, Time.deltaTime * 10f);
            float fillRatio = Mathf.Clamp01(playerVisualHp / playerMaxHp);
            RectTransform fillRt = playerHpFill.GetComponent<RectTransform>();
            fillRt.anchorMax = new Vector2(fillRatio, 1f);
        }

        // Táº¡m ngÆ°ng sinh quÃ¡i vÃ  há»“i mÃ¡u thá»¥ Ä‘á»™ng náº¿u Ä‘ang á»Ÿ tráº¡ng thÃ¡i khÃ¡c (ÄÃ¡nh Boss/Äá»™t phÃ¡)
        // (LÆ°u Ã½: Game Feel HP trÆ°á»£t váº«n cháº¡y á»Ÿ trÃªn)
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm)
            return;

        // Há»“i mÃ¡u thá»¥ Ä‘á»™ng máº·c Ä‘á»‹nh cá»§a game (1% má»—i giÃ¢y)
        if (playerCurrentHp > 0 && playerCurrentHp < playerMaxHp)
        {
            HealPlayer(playerMaxHp * 0.01f * Time.deltaTime);
        }

        // Há»“i mÃ¡u tá»« Shop (Há»“i Huyáº¿t Äan: 5% má»—i giÃ¢y)
        if (ShopManager.Instance != null && ShopManager.Instance.hpRegenVisualTimer > 0)
        {
            HealPlayer(playerMaxHp * 0.05f * Time.deltaTime);
        }

        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("âš ï¸ CombatManager: ChÆ°a cÃ³ Normal Enemy Prefabs! HÃ£y kÃ©o Asset 3D QuÃ¡i vÃ o Inspector.");
            return;
        }

        spawnTimer -= Time.deltaTime;
        activeEnemies.RemoveAll(e => e == null);
        
        // Logic mới: Sau 10 tầng số lượng quái +10 dần dần
        int currentStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
        int maxEnemies = 10 + (currentStage / 10) * 10;
        
        if (spawnTimer <= 0 && playerTransform != null && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyPrefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
        
        // Sinh quÃ¡i dÃ n hÃ ng ngang (rá»™ng hÆ¡n)
        float randomX = Random.Range(-8f, 8f);
        Vector3 spawnPos = new Vector3(playerTransform.position.x + randomX, playerTransform.position.y, playerTransform.position.z + 18f); 
        
        GameObject go = null;
        if (SimplePool.Instance != null) 
        {
            go = SimplePool.Instance.SpawnFromPool(enemyPrefab.name, spawnPos, Quaternion.identity);
        }
        
        // Fallback nếu pool không có sẵn tag
        if (go == null) 
        {
            go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
        
        go.transform.localScale = Vector3.one; // Reset scale (vÃ¬ cÃ³ thá»ƒ tÃ¡i sá»­ dá»¥ng tá»« Boss)
        
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"[Lá»–I] Prefab '{enemyPrefab.name}' cá»§a báº¡n CHÆ¯A ÄÆ¯á»¢C Gáº®N script 'Enemy.cs'! HÃ£y má»Ÿ Prefab Ä‘Ã³ lÃªn vÃ  Add Component -> Enemy.");
            return;
        }
        
        int stageIndex = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
        
        // Logic mới: Quái thường trâu hơn rõ rệt qua mỗi tầng (hệ số tăng nhanh hơn)
        float health = 50f * Mathf.Pow(1.8f, stageIndex - 1); 
        
        enemy.Init(health, playerTransform, false);
        
        activeEnemies.Add(enemy);
    }

    public void OnEnemyDied(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        
        if (enemy == currentBoss)
        {
            currentBoss = null;
            if (CultivationManager.Instance != null && GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.BossTribulation)
            {
                CultivationManager.Instance.Breakthrough();
            }
        }
        else
        {
            // Tỉ lệ 20% rớt Kiếm Phôi Lv1
            if (MergeManager.Instance != null && Random.value <= 0.2f)
            {
                MergeManager.Instance.TryAddSword(1);
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Nhặt được: Kiếm Phôi Lv1", Color.yellow);
            }
        }
    }

    public void DamagePlayer(float amount)
    {
        playerCurrentHp -= amount;
        if (playerCurrentHp <= 0)
        {
            playerCurrentHp = 0;
            
            if (CultivationManager.Instance != null)
            {
                CultivationManager.Instance.HandlePlayerDeath();
            }
            
            // XÃ³a boss Ä‘ang hiá»‡n diá»‡n khi player thua
            ClearAllEnemies();
            currentBoss = null;

            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);

            playerCurrentHp = playerMaxHp;
        }
        UpdatePlayerHPUI();
    }

    public void HealPlayer(float amount)
    {
        playerCurrentHp = Mathf.Min(playerCurrentHp + amount, playerMaxHp);
        UpdatePlayerHPUI();
    }

    private void UpdatePlayerHPUI()
    {
        if (playerHpText != null) playerHpText.text = $"HP: {Mathf.Ceil(playerCurrentHp)} / {playerMaxHp}";
    }

    public void ChangeToBossState()
    {
        ClearAllEnemies();
        SpawnBoss();
        if (MapScroller.Instance != null) MapScroller.Instance.StopMoving();
    }

    public void ChangeToIdleState()
    {
        ClearAllEnemies();
        currentBoss = null;
        if (MapScroller.Instance != null) MapScroller.Instance.ResumeMoving();
    }

    public GameObject swordPrefab;
    private FlyingSword[] activeSwords = new FlyingSword[4];

    public void UpdateEquippedSwords(int[] levels)
    {
        if (playerTransform == null)
        {
            Debug.LogError("[Combat] playerTransform bá»‹ NULL! KhÃ´ng thá»ƒ hiá»ƒn thá»‹ kiáº¿m.");
            return;
        }
        
        if (swordPrefab == null)
        {
            Debug.LogError("[Combat] swordPrefab bá»‹ NULL! CombatManager bá»‹ máº¥t Prefab Phi Kiáº¿m.");
            return;
        }
        
        for (int i = 0; i < 4; i++)
        {
            if (levels[i] > 0) // Ã” cÃ³ kiáº¿m
            {
                if (activeSwords[i] == null)
                {
                    GameObject go = Instantiate(swordPrefab, playerTransform.position, Quaternion.identity);
                    activeSwords[i] = go.GetComponent<FlyingSword>();
                    Debug.Log($"[Combat] ÄÃ£ Spawn thÃ nh cÃ´ng kiáº¿m váº­t lÃ½ 3D cho slot {i}");
                }
                
                if (activeSwords[i] != null)
                {
                    activeSwords[i].gameObject.SetActive(true);
                    activeSwords[i].swordIndex = i; // GÃ¡n vá»‹ trÃ­ cho kiáº¿m Ä‘á»ƒ khÃ´ng bá»‹ trÃ¹ng láº¥p
                    
                    // Láº¥y data cÆ¡ báº£n (Damage, Speed) chuáº©n tá»« thiáº¿t káº¿ cá»§a Kiáº¿m
                    activeSwords[i].baseDamage = FlyingSword.CalculateBaseDamage(levels[i]); 
                    activeSwords[i].baseSpeed = FlyingSword.CalculateBaseSpeed(levels[i]); 
                    
                    // Äá»•i hÃ¬nh áº£nh kiáº¿m 2D khá»›p vá»›i Level
                    activeSwords[i].SetLevelVisual(levels[i]);
                    
                    // Äá»•i mÃ u Ä‘uÃ´i sÃ¡ng bay theo level cho Ä‘áº¹p
                    float hue = (levels[i] * 0.15f) % 1f;
                    Color swordColor = Color.HSVToRGB(hue, 0.7f, 0.9f);
                    TrailRenderer trail = activeSwords[i].GetComponent<TrailRenderer>();
                    if (trail != null) trail.startColor = swordColor;
                }
            }
            else // Ã” trá»‘ng
            {
                if (activeSwords[i] != null)
                {
                    activeSwords[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private List<FlyingSword> ultimateSwords = new List<FlyingSword>();

    public void ActivateVanKiemQuyTong(float damage)
    {
        if (swordPrefab == null || playerTransform == null) return;
        
        // Triá»‡u há»“i 20 thanh phi kiáº¿m mÃ u VÃ ng Kim
        for (int i = 0; i < 20; i++)
        {
            GameObject go = Instantiate(swordPrefab, playerTransform.position, Quaternion.identity);
            FlyingSword sword = go.GetComponent<FlyingSword>();
            
            sword.swordIndex = i;
            sword.baseDamage = damage;
            sword.baseSpeed = 15f; // Tá»‘c Ä‘á»™ xÃ© giÃ³
            sword.isUltimateClone = true; // Cá» phÃ¢n biá»‡t quá»¹ Ä‘áº¡o
            
            // Nhuá»™m vÃ ng
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.yellow);
            block.SetColor("_Color", Color.yellow);
            sword.GetComponent<Renderer>().SetPropertyBlock(block);
            
            TrailRenderer trail = sword.GetComponent<TrailRenderer>();
            if (trail != null) trail.startColor = Color.yellow;
            
            ultimateSwords.Add(sword);
        }
        
        Debug.Log("[Skill] âš”ï¸ ÄÃ£ kÃ­ch hoáº¡t Váº¡n Kiáº¿m Quy TÃ´ng! Triá»‡u há»“i 20 Phi Kiáº¿m!");
    }

    public void DeactivateVanKiemQuyTong()
    {
        foreach (var sword in ultimateSwords)
        {
            if (sword != null) Destroy(sword.gameObject);
        }
        ultimateSwords.Clear();
        Debug.Log("[Skill] Háº¿t Mana. Thu há»“i Váº¡n Kiáº¿m.");
    }

    // HÃ m cung cáº¥p má»¥c tiÃªu cho Phi Kiáº¿m
    public Enemy GetNearestEnemy(Vector3 position)
    {
        Enemy nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (Enemy e in activeEnemies)
        {
            if (e == null) continue;
            float dist = Vector3.Distance(position, e.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    public Enemy GetOptimalTarget(FlyingSword seekingSword)
    {
        Enemy bestEnemy = null;
        float minScore = float.MaxValue;
        
        foreach (Enemy e in activeEnemies)
        {
            if (e == null || e.hp <= 0) continue;
            
            // TÃ­nh toÃ¡n lÆ°á»£ng sÃ¡t thÆ°Æ¡ng dá»± kiáº¿n quÃ¡i sáº½ pháº£i nháº­n tá»« cÃ¡c kiáº¿m khÃ¡c
            float incomingDmg = 0f;
            foreach (var sword in activeSwords)
            {
                if (sword != null && sword != seekingSword && sword.gameObject.activeInHierarchy && sword.TargetEnemy == e)
                {
                    incomingDmg += sword.GetExpectedDamage();
                }
            }
            foreach (var sword in ultimateSwords)
            {
                if (sword != null && sword != seekingSword && sword.gameObject.activeInHierarchy && sword.TargetEnemy == e)
                {
                    incomingDmg += sword.GetExpectedDamage();
                }
            }

            if (e.hp - incomingDmg <= 0)
                continue;

            float dist = Vector3.Distance(seekingSword.transform.position, e.transform.position);
            if (dist < minScore)
            {
                minScore = dist;
                bestEnemy = e;
            }
        }
        
        return bestEnemy;
    }

    // --- 3 KỸ NĂNG THƯỜNG ---
    public void CastAnChuong(float damage)
    {
        Enemy nearest = GetNearestEnemy(playerTransform.position);
        if (nearest != null)
        {
            // Đánh lan (AoE) bán kính 6m
            Collider[] hitColliders = Physics.OverlapSphere(nearest.transform.position, 6f);
            foreach (var hit in hitColliders)
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null && !e.isDead)
                {
                    e.TakeDamage(damage);
                }
            }
            
            // Hiệu ứng Bàn tay đè xuống (Ấn Chưởng to hơn)
            GameObject handVFX = new GameObject("GiantHand");
            handVFX.transform.position = nearest.transform.position + Vector3.up * 15f;
            handVFX.transform.localScale = new Vector3(3f, 3f, 3f);
            GiantHandVFX gh = handVFX.AddComponent<GiantHandVFX>();
            gh.Setup(new Color(1f, 0.4f, 0f), magicArrayMaterial); 
            
            Debug.Log("[Skill] 🖐️ ẤN CHƯỞNG!");
        }
    }

    public void CastLoiPhat(float damage)
    {
        Enemy target = currentBoss != null ? currentBoss : GetNearestEnemy(playerTransform.position);
        if (target != null)
        {
            target.TakeDamage(damage);
            Vector3 targetPos = target.transform.position;
            
            // Sóng xung kích
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.transform.position = targetPos + Vector3.up * 0.5f; 
            shockwave.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            if (magicArrayMaterial != null) shockwave.GetComponent<Renderer>().material = magicArrayMaterial;
            else { Material swMat = new Material(Shader.Find("Sprites/Default")); swMat.color = new Color(0.8f, 0.2f, 1f, 0.6f); shockwave.GetComponent<Renderer>().material = swMat; }
            Destroy(shockwave.GetComponent<Collider>());
            shockwave.AddComponent<ShockwaveVFX>();

            // Hiệu ứng Sét đánh từ trời xuống (5 tia)
            for (int i = 0; i < 5; i++)
            {
                GameObject lightning = new GameObject("LightningStrike");
                Vector3 endPos = targetPos;
                if (i > 0) endPos += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                Vector3 startPos = endPos + Vector3.up * 20f;
                lightning.AddComponent<LightningVFX>().Setup(startPos, endPos, new Color(0.8f, 0.2f, 1f), magicArrayMaterial);
            }

            Debug.Log("[Skill] ⚡ LÔI PHẠT!");
        }
    }

    private bool isAmDuongTranActive = false;

    public void CastAmDuongTran()
    {
        if (isAmDuongTranActive) return;
        isAmDuongTranActive = true;

        // Dùng GameObject rỗng thay vì Cylinder để xóa vòng tròn ở giữa
        GameObject aura = new GameObject("AmDuongTranPivot");
        aura.transform.position = playerTransform.position + Vector3.up * 0.5f;
        
        AuraVFX auraVfx = aura.AddComponent<AuraVFX>();
        auraVfx.SetupYinYang();
        Destroy(aura, 5f);

        foreach (var sword in activeSwords)
        {
            if (sword != null && sword.gameObject.activeSelf)
            {
                sword.baseSpeed *= 2f;
                TrailRenderer tr = sword.GetComponent<TrailRenderer>();
                if (tr != null) tr.startColor = Color.green;
            }
        }
        Invoke(nameof(EndAmDuongTran), 5f);
        Debug.Log("[Skill] 👥 ÂM DƯƠNG TRẬN! (Tốc độ Phi Kiếm x2)");
    }

    private void EndAmDuongTran()
    {
        isAmDuongTranActive = false;
        foreach (var sword in activeSwords)
        {
            if (sword != null && sword.gameObject.activeSelf)
            {
                sword.baseSpeed /= 2f; // Phục hồi tốc độ
                // Phục hồi màu mặc định (Cyan)
                TrailRenderer tr = sword.GetComponent<TrailRenderer>();
                if (tr != null) tr.startColor = Color.cyan; 
            }
        }
    }

    // â”€â”€ SAVE / LOAD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.playerMaxHp = playerMaxHp;
        slot.playerCurrentHp = playerCurrentHp;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        playerMaxHp = Mathf.Max(1000f, slot.playerMaxHp);
        playerCurrentHp = Mathf.Clamp(slot.playerCurrentHp, 1f, playerMaxHp);
        playerVisualHp = playerCurrentHp;
        UpdatePlayerHPUI();
    }
}



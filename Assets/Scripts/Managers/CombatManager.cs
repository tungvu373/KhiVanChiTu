using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public GameObject[] normalEnemyPrefabs;
    public GameObject[] bossEnemyPrefabs;
    public GameObject damagePopupPrefab; // Hiển thị số sát thương
    public Transform playerTransform;
    [Header("Player Visual & Animation")]
    public Animator playerAnimator;
    public bool isPlayerDead = false;

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

    private Coroutine deathAndReviveCoroutine;
    private float reviveIdleLockTimer = 0f;

    private void OnDisable()
    {
        // Đảm bảo khôi phục lại thời gian trò chơi bình thường nếu thoát/tắt Scene
        Time.timeScale = 1f;
    }

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
        FindPlayerAnimator();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
        if (CultivationManager.Instance != null)
        {
            CultivationManager.Instance.OnStageChanged += HandleStageChanged;
            CultivationManager.Instance.OnTuViChanged += HandleTuViChanged;
            
            // Tính toán lượng máu ban đầu theo Cảnh Giới và Tu Vi
            RecalculateMaxHp(false);
        }

        // Tự động kích hoạt Hệ thống Mây Tiên (Particle System đi theo Player) nếu chưa có
        if (FindObjectOfType<TuTienCloudSystem>() == null)
        {
            GameObject cloudObj = new GameObject("TuTienCloudSystem");
            cloudObj.AddComponent<TuTienCloudSystem>();
        }

        // Nếu MainMenu không mở (chơi trực tiếp Scene), đảm bảo nhạc nền Ingame tự động phát
        if (MainMenuManager.Instance == null || !MainMenuManager.Instance.gameObject.activeInHierarchy)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInGameBGM();
            }
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
            CultivationManager.Instance.OnTuViChanged -= HandleTuViChanged;
        }
    }

    /// <summary>
    /// Tính toán lượng Máu Tối Đa (Max HP) theo Cảnh Giới và Điểm Tu Vi tích lũy
    /// </summary>
    public void RecalculateMaxHp(bool fullHeal = false)
    {
        if (CultivationManager.Instance == null) return;

        float oldMax = playerMaxHp;

        // Công thức HP chuẩn Tu Tiên:
        // 1. Máu khởi điểm căn bản: 1000 HP
        // 2. Thưởng Cảnh Giới: mỗi cấp cảnh giới (Luyện Khí 1..15, Trúc Cơ, Kết Đan...) +200 HP
        // 3. Thưởng Tu Vi hiện tại: mỗi điểm Tu Vi +0.5 HP
        // 4. Nhân hệ số đột phá vĩnh viễn (permanentStatMultiplier)
        int stageIdx = CultivationManager.Instance.currentStageIndex;
        float tuVi = CultivationManager.Instance.currentTuVi;
        float statMult = CultivationManager.Instance.permanentStatMultiplier;

        float baseHp = 1000f + (stageIdx * 200f) + (tuVi * 0.5f);
        playerMaxHp = Mathf.Round(baseHp * (statMult > 0 ? statMult : 1f));

        if (fullHeal)
        {
            playerCurrentHp = playerMaxHp;
        }
        else
        {
            float diff = playerMaxHp - oldMax;
            if (diff > 0)
            {
                playerCurrentHp += diff; // Tăng máu theo lượng max HP mới mở rộng
            }
            playerCurrentHp = Mathf.Clamp(playerCurrentHp, 1f, playerMaxHp);
        }

        UpdatePlayerHPUI();
    }

    private void HandleStageChanged(CultivationStageData stageData)
    {
        // Khi đột phá cảnh giới: Tính lại Max HP và hồi phục đầy máu
        RecalculateMaxHp(fullHeal: true);
        if (GameLogger.Instance != null && stageData != null)
        {
            GameLogger.Instance.Log($"Đột phá {stageData.stageName}! HP tối đa tăng lên {playerMaxHp:F0}!", Color.green);
        }
    }

    private void HandleTuViChanged(float currentTuVi, float maxTuVi)
    {
        // Khi tăng Tu Vi (farm quái, cắn đan...): HP lập tức tăng theo
        RecalculateMaxHp(fullHeal: false);
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
            if (e != null)
            {
                e.CancelInvoke();
                e.isDead = false;
                e.gameObject.SetActive(false);
            }
        }
        activeEnemies.Clear();

        if (currentBoss != null)
        {
            currentBoss.CancelInvoke();
            currentBoss.isDead = false;
            currentBoss.gameObject.SetActive(false);
            currentBoss = null;
        }

        // Quét sạch tất cả Enemy đang active trong Scene để đảm bảo không sót
        Enemy[] lingering = FindObjectsOfType<Enemy>();
        for (int i = 0; i < lingering.Length; i++)
        {
            if (lingering[i] != null && lingering[i].gameObject.activeSelf)
            {
                lingering[i].CancelInvoke();
                lingering[i].isDead = false;
                lingering[i].gameObject.SetActive(false);
            }
        }
    }

    private Enemy currentBoss;

    private void SpawnBoss()
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0 || playerTransform == null) return;
        
        GameObject bossPrefab = bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Length)];
        
        // 1. Sinh Boss từ xa (cách 26m phía trước) uy nghi tiến dần đến Player
        Vector3 spawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z + 26f);
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
        boss.moveSpeed = 1.8f; // Boss bước đi trầm ổn, oai vệ từ xa
        activeEnemies.Add(currentBoss);

        // 2. Sinh toán quái con (tiền trạm) chạy trước hộ giá Boss!
        SpawnMinionsAheadOfBoss(stage);
    }

    private void SpawnMinionsAheadOfBoss(int stage)
    {
        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0 || playerTransform == null) return;

        // Sinh 4-6 quái con ở cự ly gần hơn (12m - 16m), chạy trước xung phong
        int minionCount = Mathf.Clamp(4 + (stage / 10), 4, 8);
        for (int i = 0; i < minionCount; i++)
        {
            GameObject prefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
            float xOffset = Random.Range(-5f, 5f);
            float zOffset = Random.Range(12f, 16f) + (i * 0.7f);
            Vector3 spawnPos = new Vector3(playerTransform.position.x + xOffset, playerTransform.position.y, playerTransform.position.z + zOffset);

            GameObject minionObj = null;
            if (SimplePool.Instance != null)
            {
                minionObj = SimplePool.Instance.SpawnFromPool(prefab.name, spawnPos, Quaternion.identity);
            }
            if (minionObj == null)
            {
                minionObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            }

            minionObj.transform.localScale = Vector3.one;

            Enemy enemy = minionObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                float health = 50f * Mathf.Pow(1.8f, Mathf.Max(0, stage - 1));
                enemy.Init(health, playerTransform, false);
                enemy.moveSpeed = 3.6f; // Quái con chạy nhanh hơn Boss xông lên trước
                activeEnemies.Add(enemy);
            }
        }
    }

    private void Update()
    {
        // 1. Đồng bộ Trạng thái Di chuyển Map & Animation Run / Idle của Player
        UpdateMapAndPlayerState();

        // 2. Trượt thanh máu mượt mà (Game Feel) - Xử lý ngay cả khi đánh Boss
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

        // Nếu Player đang tử trận: Ngưng hoàn toàn sinh quái và hồi máu
        if (isPlayerDead)
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
        activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy || e.isDead);
        
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

    public void DamagePlayer(float amount, bool isBoss = false)
    {
        if (isPlayerDead) return;

        // Nếu bị Boss tấn công: Kích hoạt hiệu ứng chớp đỏ viền màn hình combat
        if (isBoss)
        {
            BossDamageVignette.TriggerFlash();
        }

        playerCurrentHp -= amount;
        if (playerCurrentHp <= 0)
        {
            playerCurrentHp = 0;
            isPlayerDead = true;

            // 1. Đóng băng toàn bộ thế giới game bằng Time.timeScale = 0 theo yêu cầu
            Time.timeScale = 0f;

            // 2. Dừng cuộn map ngay khi tử trận
            if (MapScroller.Instance != null) MapScroller.Instance.StopMoving();

            // 3. Hoạt ảnh tử trận của Player (dùng UnscaledTime để hoạt ảnh diễn ra mượt mà khi timeScale = 0)
            if (playerAnimator == null) FindPlayerAnimator();
            if (playerAnimator != null)
            {
                playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                playerAnimator.SetBool("IsMoving", false);
                playerAnimator.ResetTrigger("Revive");
                playerAnimator.SetTrigger("Die");
                Debug.Log("💀 [CombatManager] Nhân vật tử trận! Đã gọi trigger 'Die' (TimeScale = 0).");
            }
            else
            {
                Debug.LogError("❌ [CombatManager] playerAnimator bị NULL khi tử trận!");
            }

            // 4. Hiển thị thông báo Tử trận uy nghiêm ở giữa màn Combat 3D
            CombatNotificationUI.ShowDeath();

            // 5. Bảo toàn tu vi và làm mới tiến trình ải
            if (CultivationManager.Instance != null)
            {
                CultivationManager.Instance.HandlePlayerDeath();
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.ResetCurrentStageOnDeath();
            }

            if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);

            // 6. Chờ thời gian thực (Realtime) khi Time.timeScale = 0 để hồi sinh và reset màn hình combat
            if (deathAndReviveCoroutine != null) StopCoroutine(deathAndReviveCoroutine);
            deathAndReviveCoroutine = StartCoroutine(PlayerDeathAndReviveRoutine(2.2f));
        }
        UpdatePlayerHPUI();
    }

    private System.Collections.IEnumerator PlayerDeathAndReviveRoutine(float delayRealtime)
    {
        yield return new WaitForSecondsRealtime(delayRealtime);
        RevivePlayer();
        deathAndReviveCoroutine = null;
    }

    /// <summary>
    /// Hồi sinh người chơi và Reset toàn bộ màn hình Combat về trạng thái ban đầu sạch sẽ.
    /// </summary>
    public void RevivePlayer()
    {
        // 1. Khôi phục lại thời gian trò chơi bình thường
        Time.timeScale = 1f;

        // 2. Khôi phục toàn bộ máu người chơi và cập nhật thanh máu ngay lập tức
        playerCurrentHp = playerMaxHp;
        playerVisualHp = playerMaxHp;
        isPlayerDead = false;
        UpdatePlayerHPUI();

        // 3. Reset toàn bộ màn hình Combat:
        // a. Quét sạch tất cả quái vật và Boss hiện diện
        ClearAllEnemies();

        // b. Xóa sạch mọi chữ số sát thương (Damage Popup) đang bay lơ lửng trên màn hình
        if (SimplePool.Instance != null)
        {
            SimplePool.Instance.DeactivateAllInPool("DamagePopup");
        }

        // c. Đặt lại mục tiêu của toàn bộ phi kiếm để bay quay về quanh người chơi
        FlyingSword[] swords = FindObjectsOfType<FlyingSword>();
        foreach (var sword in swords)
        {
            if (sword != null) sword.ResetTarget();
        }

        // d. Tắt thông báo tử trận, viền đỏ chớp và rung camera
        if (CombatNotificationUI.Instance != null)
        {
            CombatNotificationUI.Instance.HideImmediate();
        }
        if (BossDamageVignette.Instance != null)
        {
            BossDamageVignette.Instance.ResetVignette();
        }
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ResetShake();
        }

        // e. Khôi phục Animator người chơi và trở về hoạt ảnh IDLE chuẩn mực
        if (playerAnimator == null) FindPlayerAnimator();
        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            playerAnimator.ResetTrigger("Die");
            playerAnimator.ResetTrigger("Revive");
            playerAnimator.SetBool("IsMoving", false);
            // Ép Animator chuyển ngay về State "idle" (Layer 0, thời gian 0f) để không bị kẹt ở Dead hoặc ép Run
            playerAnimator.Play("idle", 0, 0f);
            Debug.Log("✨ [CombatManager] Nhân vật hồi sinh! Đã trở về hoạt ảnh 'idle'.");
        }

        // f. Dừng cuộn map trong thời gian nghỉ hồi sinh (nhân vật đứng yên thế thủ)
        if (MapScroller.Instance != null)
        {
            MapScroller.Instance.StopMoving();
        }

        // g. Khóa trạng thái Idle trong 1.0s (bằng đúng spawnTimer) để nhân vật đứng yên thế thủ trước khi quái xuất hiện
        reviveIdleLockTimer = 1.0f;
        spawnTimer = 1.0f;
    }

    // ── KIỂM TRA QUÁI TRONG TẦM ĐÁNH & ĐIỀU KHIỂN MAP / PLAYER ───────────
    public bool HasEnemyInAttackRange()
    {
        // 1. Đang có Boss -> Coi như đang trong tầm đánh, dừng map
        if (currentBoss != null && !currentBoss.isDead && currentBoss.gameObject.activeInHierarchy)
            return true;

        if (playerTransform == null) return false;

        // 2. Quét danh sách quái thường đang hoạt động
        Vector2 playerPos2D = new Vector2(playerTransform.position.x, playerTransform.position.z);
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            Enemy e = activeEnemies[i];
            if (e != null && !e.isDead && e.gameObject.activeInHierarchy)
            {
                Vector2 enemyPos2D = new Vector2(e.transform.position.x, e.transform.position.z);
                float dist = Vector2.Distance(enemyPos2D, playerPos2D);
                // Quái đã tiến vào tầm đánh của nó
                if (dist <= e.attackRange + 0.3f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateMapAndPlayerState()
    {
        // 1. Nếu Player đang tử trận: Map dừng, Player không Run
        if (isPlayerDead)
        {
            if (MapScroller.Instance != null && MapScroller.Instance.isMoving)
            {
                MapScroller.Instance.StopMoving();
            }
            if (playerAnimator != null && playerAnimator.GetBool("IsMoving"))
            {
                playerAnimator.SetBool("IsMoving", false);
            }
            return;
        }

        // 2. Nếu đang trong thời gian nghỉ sau hồi sinh: Giữ nguyên thế IDLE và dừng map
        if (reviveIdleLockTimer > 0f)
        {
            reviveIdleLockTimer -= Time.deltaTime;
            if (MapScroller.Instance != null && MapScroller.Instance.isMoving)
            {
                MapScroller.Instance.StopMoving();
            }
            if (playerAnimator != null && playerAnimator.GetBool("IsMoving"))
            {
                playerAnimator.SetBool("IsMoving", false);
            }
            return;
        }

        bool enemyInRange = HasEnemyInAttackRange();

        if (enemyInRange)
        {
            // Có quái/Boss vào tầm đánh: DỪNG MAP, NHÂN VẬT ĐỔI SANG IDLE
            if (MapScroller.Instance != null && MapScroller.Instance.isMoving)
            {
                MapScroller.Instance.StopMoving();
            }
            if (playerAnimator != null && playerAnimator.GetBool("IsMoving"))
            {
                playerAnimator.SetBool("IsMoving", false);
            }
        }
        else
        {
            // Không có quái nào trong tầm đánh: CUỘN MAP, NHÂN VẬT ĐỔI SANG RUN
            if (GameManager.Instance == null || GameManager.Instance.currentState == GameManager.GameState.IdleFarm)
            {
                if (MapScroller.Instance != null && !MapScroller.Instance.isMoving)
                {
                    MapScroller.Instance.ResumeMoving();
                }
                if (playerAnimator != null && !playerAnimator.GetBool("IsMoving"))
                {
                    playerAnimator.SetBool("IsMoving", true);
                }
            }
        }
    }

    private void FindPlayerAnimator()
    {
        if (playerAnimator != null) return;

        if (playerTransform != null)
        {
            playerAnimator = playerTransform.GetComponentInChildren<Animator>();
            if (playerAnimator == null) playerAnimator = playerTransform.GetComponentInParent<Animator>();
            if (playerAnimator == null) playerAnimator = playerTransform.GetComponent<Animator>();
        }

        if (playerAnimator == null)
        {
            Animator[] allAnims = FindObjectsOfType<Animator>();
            foreach (var a in allAnims)
            {
                if (a.name.ToLower().Contains("player") || 
                    (a.runtimeAnimatorController != null && a.runtimeAnimatorController.name.ToLower().Contains("player")))
                {
                    playerAnimator = a;
                    break;
                }
            }
        }

        if (playerAnimator != null)
        {
            Debug.Log($"✅ [CombatManager] Đã kết nối Player Animator: '{playerAnimator.gameObject.name}', Controller: '{playerAnimator.runtimeAnimatorController?.name}'");
        }
        else
        {
            Debug.LogWarning("⚠️ [CombatManager] Chưa tìm thấy Player Animator! Hãy kiểm tra lại Transform nhân vật trên Inspector.");
        }
    }

    public void HealPlayer(float amount)
    {
        playerCurrentHp = Mathf.Min(playerCurrentHp + amount, playerMaxHp);
        UpdatePlayerHPUI();
    }

    private void UpdatePlayerHPUI()
    {
        if (playerHpText != null) playerHpText.text = $"HP: {Mathf.Ceil(playerCurrentHp)} / {playerMaxHp:F0}";
    }

    public void ChangeToBossState()
    {
        ClearAllEnemies();
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.BossTribulation)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.BossTribulation);
        }
        else
        {
            SpawnBoss();
        }
        if (MapScroller.Instance != null) MapScroller.Instance.StopMoving();
    }

    public void ChangeToIdleState()
    {
        ClearAllEnemies();
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }
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
        if (seekingSword == null) return null;

        Enemy bestEnemy = null;
        float minScore = float.MaxValue;

        Enemy fallbackEnemy = null;
        float minFallbackDist = float.MaxValue;

        Vector3 swordPos = seekingSword.transform.position;

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            Enemy e = activeEnemies[i];
            if (e == null || !e.gameObject.activeInHierarchy || e.isDead || e.hp <= 0) continue;

            float dist = Vector3.Distance(swordPos, e.transform.position);

            // Luôn lưu quái/Boss gần nhất còn sống làm Fallback (để kiếm không bao giờ bị mất mục tiêu)
            if (dist < minFallbackDist)
            {
                minFallbackDist = dist;
                fallbackEnemy = e;
            }

            // Tính toán lượng sát thương dự kiến quái sẽ phải nhận từ các kiếm khác đang bay tới
            float incomingDmg = 0f;
            if (activeSwords != null)
            {
                for (int s = 0; s < activeSwords.Length; s++)
                {
                    var sword = activeSwords[s];
                    if (sword != null && sword != seekingSword && sword.gameObject.activeInHierarchy && sword.TargetEnemy == e)
                    {
                        incomingDmg += sword.GetExpectedDamage();
                    }
                }
            }

            if (ultimateSwords != null)
            {
                for (int s = 0; s < ultimateSwords.Count; s++)
                {
                    var sword = ultimateSwords[s];
                    if (sword != null && sword != seekingSword && sword.gameObject.activeInHierarchy && sword.TargetEnemy == e)
                    {
                        incomingDmg += sword.GetExpectedDamage();
                    }
                }
            }

            // Nếu quái chưa nhận đủ sát thương kết liễu từ các kiếm khác -> Ưu tiên nhắm vào
            if (e.hp - incomingDmg > 0)
            {
                if (dist < minScore)
                {
                    minScore = dist;
                    bestEnemy = e;
                }
            }
        }

        // Nếu tất cả quái đều đã có kiếm nhắm tới (hoặc chỉ có 1 quái/Boss) -> BẮT BUỘC trả về fallbackEnemy để kiếm tiếp tục chém bồi!
        return bestEnemy != null ? bestEnemy : fallbackEnemy;
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
                lightning.AddComponent<LightningVFX>().Setup(startPos, endPos, new Color(1f, 0.95f, 0.2f), magicArrayMaterial);
            }

            // Phát âm thanh kỹ năng Lôi Phạt
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLoiPhatSFX();
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
        RecalculateMaxHp(false);
        if (slot.playerMaxHp > playerMaxHp) playerMaxHp = slot.playerMaxHp;
        playerCurrentHp = Mathf.Clamp(slot.playerCurrentHp > 0 ? slot.playerCurrentHp : playerMaxHp, 1f, playerMaxHp);
        playerVisualHp = playerCurrentHp;
        UpdatePlayerHPUI();
    }
}



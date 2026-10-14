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
    [Header("Spawning & Density Control")]
    public float spawnInterval = 0.2f; // Tốc độ spawn quái (0.2s)
    public int maxOnScreenEnemies = 15; // Giới hạn số lượng quái xuất hiện đồng thời trên sân (tránh lag, đè chồng)
    
    private List<Enemy> activeEnemies = new List<Enemy>();

    public float playerMaxHp = 1000f;
    public float playerCurrentHp = 1000f;
    private float playerVisualHp = 1000f; // DÃ¹ng Ä‘á»ƒ trÆ°á»£t mÆ°á»£t mÃ 
    
    [Header("UI References")]
    public UnityEngine.UI.Image playerHpFill;
    public UnityEngine.UI.Text playerHpText;

    [Header("Skill VFX Materials")]
    public Material magicArrayMaterial;

    [Header("Tribulation Boss (Độ Kiếp Thăng Cấp)")]
    public bool isTribulationBossActive = false;
    private Coroutine tribulationStormCoroutine;

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

        if (magicArrayMaterial == null)
        {
#if UNITY_EDITOR
            magicArrayMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_MagicArray.mat");
#endif
            if (magicArrayMaterial == null)
            {
                Shader s = Shader.Find("Sprites/Default") ?? Shader.Find("Mobile/Particles/Additive");
                magicArrayMaterial = new Material(s);
                magicArrayMaterial.color = new Color(0.9f, 0.85f, 1f, 0.9f);
            }
        }

        if (SimplePool.Instance != null)
        {
            // Pool toÃ n bá»™ quÃ¡i thÆ°á»ng
            if (normalEnemyPrefabs != null && normalEnemyPrefabs.Length > 0)
            {
                int perPrefabSize = Mathf.Max(10, 30 / normalEnemyPrefabs.Length);
                foreach (var prefab in normalEnemyPrefabs)
                {
                    if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, perPrefabSize);
                }
            }
            
            // Pool toàn bộ boss (chỉ duy nhất 1 con mỗi 10 vòng, pool size = 1)
            if (bossEnemyPrefabs != null && bossEnemyPrefabs.Length > 0)
            {
                foreach (var prefab in bossEnemyPrefabs)
                {
                    if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, 1);
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
        // 1. Kiểm tra an toàn: Nếu đã có Boss đang tồn tại và hoạt động thì TUYỆT ĐỐI KHÔNG spawn thêm tránh dư thừa
        if (currentBoss != null && currentBoss.gameObject.activeInHierarchy && !currentBoss.isDead)
        {
            Debug.LogWarning("⚠️ [CombatManager] Đang có Boss tồn tại trên sân! Bỏ qua lệnh SpawnBoss để tránh dư thừa Boss.");
            return;
        }

        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0 || playerTransform == null) return;
        
        // 2. Dọn sạch quái thường còn sót lại trên sân trước khi sinh Boss
        ClearAllEnemies();

        GameObject bossPrefab = bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Length)];
        
        // Sinh Boss từ xa (cách 26m phía trước) uy nghi tiến dần đến Player
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

        // Sinh đội quân quái hộ tống Boss xung phong tiền trạm (lấy từ pool quái thường 30 con)
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
                // Máu quái con hộ giá Boss: 75% máu quái thường của tầng đó (tầng 1-4 rất thấp giúp dễ thở)
                float health = CalculateEnemyHealth(stage) * 0.75f;
                enemy.Init(health, playerTransform, false);
                enemy.moveSpeed = 3.6f; // Quái con chạy nhanh hơn Boss xông lên trước
                activeEnemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Bắt đầu trận chiến Boss Độ Kiếp (Thiên Kiếp Hàng Lâm).
    /// Reset toàn bộ quái thường, tạo sấm sét liên tục trên map bằng Material Lôi Phạt có sẵn,
    /// và spawn Boss Độ Kiếp có sức mạnh tương ứng theo bậc Cảnh Giới.
    /// </summary>
    public void StartTribulationBossBattle()
    {
        if (isTribulationBossActive) return;

        // 1. Tạm thời ẩn UI Stage tầng (tiến độ quái/tầng) trong suốt trận chiến Thiên Kiếp
        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetStageUIVisible(false);
        }

        // 2. Dọn dẹp sạch sẽ toàn bộ quái thường và đạn/popup cũ
        ClearAllEnemies();
        if (SimplePool.Instance != null)
        {
            SimplePool.Instance.DeactivateAllInPool("DamagePopup");
        }

        isTribulationBossActive = true;

        // 3. Dừng map di chuyển để tập trung chiến đấu sinh tử
        if (MapScroller.Instance != null)
        {
            MapScroller.Instance.StopMoving();
        }

        // 4. Spawn Boss Thiên Kiếp (Độ Kiếp) - CHỈ DUY NHẤT 1 MÌNH BOSS, ĐẤU 1-1 VỚI PLAYER!
        SpawnTribulationBoss();

        // 4. Bật hiệu ứng sấm sét Lôi Phạt liên tục trên map bằng magicArrayMaterial
        StopTribulationStorm();
        tribulationStormCoroutine = StartCoroutine(TribulationLightningStormRoutine());

        // 5. Hiệu ứng khởi đầu trận chiến uy nghiêm
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.8f, 0.5f);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoiPhatSFX();
        }
    }

    private void SpawnTribulationBoss()
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0 || playerTransform == null)
        {
            Debug.LogError("❌ [CombatManager] Không có Boss Prefabs để spawn Boss Độ Kiếp!");
            return;
        }

        GameObject bossPrefab = bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Length)];
        // Rút ngắn cự ly spawn xuống 13m (thay vì 24m) để Boss áp sát chỉ sau ~2.5 - 3s, ra đòn ngay lập tức
        Vector3 spawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z + 13f);

        GameObject go = null;
        if (SimplePool.Instance != null)
        {
            go = SimplePool.Instance.SpawnFromPool(bossPrefab.name, spawnPos, Quaternion.identity);
        }
        if (go == null)
        {
            go = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }

        BossEnemy boss = go.GetComponent<BossEnemy>();
        if (boss == null)
        {
            Enemy oldEnemy = go.GetComponent<Enemy>();
            boss = go.AddComponent<BossEnemy>();
            if (oldEnemy != null)
            {
                boss.hpBarHeight = oldEnemy.hpBarHeight;
                boss.attackRange = oldEnemy.attackRange;
                boss.animator = oldEnemy.animator;
                Destroy(oldEnemy);
            }
        }

        currentBoss = boss;

        int realmTier = CultivationManager.Instance != null ? CultivationManager.Instance.GetCurrentTribulationTier() : 1;
        int stageIdx = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        string targetRealm = CultivationManager.Instance != null ? CultivationManager.Instance.GetNextRealmName() : "Đại Cảnh Giới Mới";

        boss.InitTribulationBoss(realmTier, stageIdx, targetRealm, playerTransform);
        activeEnemies.Add(boss);

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"⚡ THIÊN KIẾP HÀNG LÂM! Boss Độ Kiếp xuất hiện! Quyết chiến để tấn thăng {targetRealm}!", Color.red);
        }
    }

    /// <summary>
    /// Hiệu ứng sấm sét Lôi Phạt giáng xuống liên tục khắp map trong suốt trận chiến Boss Độ Kiếp.
    /// Sử dụng magicArrayMaterial có sẵn để tạo tia sét từ trời đánh xuống đất.
    /// </summary>
    private System.Collections.IEnumerator TribulationLightningStormRoutine()
    {
        while (isTribulationBossActive && !isPlayerDead)
        {
            float waitTime = Random.Range(0.6f, 1.2f);
            yield return new WaitForSeconds(waitTime);

            if (!isTribulationBossActive || isPlayerDead || playerTransform == null) break;

            // Đánh 2 - 3 tia sét ngẫu nhiên tại các vị trí xung quanh đấu trường
            int strikeCount = Random.Range(2, 4);
            for (int i = 0; i < strikeCount; i++)
            {
                float xOff = Random.Range(-10f, 10f);
                float zOff = Random.Range(-2f, 22f);
                Vector3 strikePos = playerTransform.position + new Vector3(xOff, 0f, zOff);
                Vector3 skyPos = strikePos + Vector3.up * 22f;

                GameObject lightning = new GameObject("TribulationStormLightning");
                LightningVFX vfx = lightning.AddComponent<LightningVFX>();
                Color lightningColor = new Color(0.95f, 0.85f, 1f); // Tím lam lôi điện thiên kiếp
                vfx.Setup(skyPos, strikePos, lightningColor, magicArrayMaterial);
            }

            // Thỉnh thoảng rung nhẹ mặt đất và phát tiếng sét nổ từ xa
            if (Random.value <= 0.4f)
            {
                if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.2f, 0.15f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayLoiPhatSFX();
            }
        }
    }

    public void StopTribulationStorm()
    {
        if (tribulationStormCoroutine != null)
        {
            StopCoroutine(tribulationStormCoroutine);
            tribulationStormCoroutine = null;
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
        // Tuyệt đối không sinh quái nếu đang đối đầu Boss Thiên Kiếp (Boss chỉ xuất hiện 1 mình đấu 1-1 với Player)
        if (isTribulationBossActive)
            return;

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
            float potionHpRate = ShopManager.Instance.GetHpRegenRate();
            if (potionHpRate > 0)
            {
                HealPlayer(playerMaxHp * potionHpRate * Time.deltaTime);
            }
        }

        // Hiệu ứng hồi máu & MP liên tục khi Âm Dương Trận đang kích hoạt (6 giây)
        if (isAmDuongTranActive)
        {
            amDuongTranTimer -= Time.deltaTime;
            if (amDuongTranTimer <= 0)
            {
                EndAmDuongTran();
            }
            else
            {
                // Hồi máu: 3% + 0.5%/cấp mỗi giây
                if (playerCurrentHp > 0 && playerCurrentHp < playerMaxHp)
                {
                    HealPlayer(playerMaxHp * GetAmDuongTranHpRegenPercent() * Time.deltaTime);
                }
                // Hồi Linh Lực (MP): 15 + 3/cấp mỗi giây
                if (SkillManager.Instance != null && !SkillManager.Instance.isSkillActive)
                {
                    SkillManager.Instance.currentLinhLuc = Mathf.Min(
                        SkillManager.Instance.maxLinhLuc, 
                        SkillManager.Instance.currentLinhLuc + GetAmDuongTranMpRegenFlat() * Time.deltaTime
                    );
                }
            }
        }

        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("âš ï¸ CombatManager: ChÆ°a cÃ³ Normal Enemy Prefabs! HÃ£y kÃ©o Asset 3D QuÃ¡i vÃ o Inspector.");
            return;
        }

        spawnTimer -= Time.deltaTime;
        activeEnemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy || e.isDead);
        
        // Cân bằng số quái yêu cầu theo tầng (1-10: 10, 11-20: 20...) và tính số quái còn thiếu để active đủ
        int targetKills = StageManager.Instance != null ? StageManager.Instance.enemiesToBoss : 10;
        int alreadyKilled = StageManager.Instance != null ? StageManager.Instance.enemiesKilled : 0;
        int killsNeededRemaining = Mathf.Max(0, targetKills - alreadyKilled);
        
        // Số quái còn lại cần spawn cho tầng này (tổng số quái spawn ra không vượt quá targetKills)
        int remainingToSpawn = killsNeededRemaining - activeEnemies.Count;

        // Giới hạn số lượng quái cùng xuất hiện trên sân (tối đa maxOnScreenEnemies = 15 con, hoặc ít hơn nếu tầng yêu cầu ít hơn)
        int currentMaxOnScreen = Mathf.Min(maxOnScreenEnemies, targetKills);
        
        // Điều kiện spawn: Chưa vượt trần hiển thị trên sân (tránh lag/đè chồng) VÀ vẫn còn quái cần spawn cho tầng này
        if (spawnTimer <= 0 && playerTransform != null && activeEnemies.Count < currentMaxOnScreen && remainingToSpawn > 0)
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
        
        // Máu quái thường: Cân bằng tầng 1, 2, 3, 4 máu thấp xuống để người chơi dễ dàng farm đầu game
        float health = CalculateEnemyHealth(stageIndex); 
        
        enemy.Init(health, playerTransform, false);
        
        activeEnemies.Add(enemy);
    }

    /// <summary>
    /// Tính toán máu của Enemy thường theo tầng:
    /// Tầng 1: 35 HP (kiếm Lv1 10 dame chém 4 nhát là diệt)
    /// Tầng 2: 65 HP
    /// Tầng 3: 110 HP
    /// Tầng 4: 180 HP
    /// Tầng 5 trở đi: Tăng trưởng lũy tiến nhịp nhàng (x1.22 mỗi tầng)
    /// </summary>
    public static float CalculateEnemyHealth(int stage)
    {
        if (stage <= 1) return 35f;
        if (stage == 2) return 65f;
        if (stage == 3) return 110f;
        if (stage == 4) return 180f;

        return 180f * Mathf.Pow(1.22f, stage - 4);
    }

    public void OnEnemyDied(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        
        bool isBossEnemy = (enemy == currentBoss) || (enemy != null && enemy.isBoss) || (enemy is BossEnemy);
        if (isBossEnemy)
        {
            currentBoss = null;

            // TRƯỜNG HỢP 1: TIÊU DIỆT BOSS ĐỘ KIẾP (THIÊN KIẾP) THÀNH CÔNG!
            if (isTribulationBossActive)
            {
                StopTribulationStorm();
                isTribulationBossActive = false;

                // Hiển thị lại UI Stage tầng
                if (StageManager.Instance != null)
                {
                    StageManager.Instance.SetStageUIVisible(true);
                }

                string targetRealm = CultivationManager.Instance != null ? CultivationManager.Instance.GetNextRealmName() : "Cảnh Giới Mới";

                // Tấn thăng cảnh giới
                if (CultivationManager.Instance != null)
                {
                    CultivationManager.Instance.Breakthrough();
                }

                // Hiển thị thông báo chiến thắng rực rỡ
                CombatNotificationUI.ShowBreakthrough(targetRealm);

                if (GameLogger.Instance != null)
                {
                    GameLogger.Instance.Log($"⚡ ĐỘ KIẾP THÀNH CÔNG! Đã vượt qua Thiên Kiếp, tấn thăng {targetRealm}!", Color.green);
                }

                // Chuyển về trạng thái farm tiếp tục leo tầng bình thường
                ChangeToIdleState();
                return;
            }

            // TRƯỜNG HỢP 2: TIÊU DIỆT BOSS TẦNG THƯỜNG (Stage Boss mỗi 10 tầng: 10, 20, 30...)
            Debug.Log("⚔️ [CombatManager] Tiêu diệt Boss tầng thành công! Đạt mốc qua tầng mới.");
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("Tiêu diệt Boss tầng thành công! Chuẩn bị chuyển tầng tiếp theo!", Color.yellow);
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

            // 4. Xử lý THẤT BẠI ĐỘ KIẾP hoặc TỬ TRẬN THƯỜNG
            if (isTribulationBossActive)
            {
                StopTribulationStorm();
                isTribulationBossActive = false;

                // Hiển thị lại UI Stage tầng
                if (StageManager.Instance != null)
                {
                    StageManager.Instance.SetStageUIVisible(true);
                }

                // Độ kiếp thất bại: Bị thiên kiếp phản phệ, tổn thất 25% điểm Tu Vi
                float lostTuVi = 0f;
                if (CultivationManager.Instance != null)
                {
                    lostTuVi = CultivationManager.Instance.ApplyTribulationFailurePenalty();
                }

                // Hiển thị Banner Độ Kiếp Thất Bại sấm sét
                CombatNotificationUI.ShowTribulationFailed(lostTuVi);

                if (GameLogger.Instance != null)
                {
                    GameLogger.Instance.Log($"⚡ ĐỘ KIẾP THẤT BẠI! Bị Thiên Kiếp phản phệ, tổn thất {lostTuVi:F0} Tu Vi (-25%)!", Color.red);
                }

                if (StageManager.Instance != null)
                {
                    StageManager.Instance.ResetCurrentStageOnDeath();
                }

                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
            }
            else
            {
                // Hiển thị thông báo Tử trận uy nghiêm ở giữa màn Combat 3D
                CombatNotificationUI.ShowDeath();

                // Bảo toàn tu vi và làm mới tiến trình ải
                if (CultivationManager.Instance != null)
                {
                    CultivationManager.Instance.HandlePlayerDeath();
                }

                if (StageManager.Instance != null)
                {
                    StageManager.Instance.ResetCurrentStageOnDeath();
                }

                if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
            }

            // 5. Chờ thời gian thực (Realtime) khi Time.timeScale = 0 để hồi sinh và reset màn hình combat
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
        StopTribulationStorm();
        isTribulationBossActive = false;
        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetStageUIVisible(true);
        }

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
        spawnTimer = 0f; // Bắt đầu sinh quái cho tầng mới ngay lập tức
        // Nếu đang trong hoạt cảnh Đột Phá, giữ nguyên để CultivationManager chạy trọn vẹn hiệu ứng 2 giây
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm && GameManager.Instance.currentState != GameManager.GameState.Breakthrough)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }
        if (MapScroller.Instance != null && (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Breakthrough))
        {
            MapScroller.Instance.ResumeMoving();
        }
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
            // Đánh lan (AoE) bán kính 6m + GÂY CHOÁNG 2 GIÂY ĐỨNG IM
            Collider[] hitColliders = Physics.OverlapSphere(nearest.transform.position, 6f);
            foreach (var hit in hitColliders)
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null && !e.isDead)
                {
                    e.TakeDamage(damage);
                    if (!e.isBoss)
                    {
                        e.Stun(2.0f); // Quái thường đứng im hoàn toàn 2 giây (Boss miễn nhiễm)
                    }
                }
            }
            
            // Hiệu ứng Bàn tay đè xuống (Ấn Chưởng to hơn)
            GameObject handVFX = new GameObject("GiantHand");
            handVFX.transform.position = nearest.transform.position + Vector3.up * 15f;
            handVFX.transform.localScale = new Vector3(3f, 3f, 3f);
            GiantHandVFX gh = handVFX.AddComponent<GiantHandVFX>();
            gh.Setup(new Color(1f, 0.4f, 0f), magicArrayMaterial); 
            
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("🖐️ ẤN CHƯỞNG: Chấn áp kinh thiên, toàn bộ quái vật bị CHOÁNG 2s!", new Color(1f, 0.5f, 0f));
            }
            Debug.Log("[Skill] 🖐️ ẤN CHƯỞNG: Đã gây choáng 2s!");
        }
    }

    public void CastLoiPhat(float damage)
    {
        Enemy target = currentBoss != null ? currentBoss : GetNearestEnemy(playerTransform.position);
        if (target != null)
        {
            Vector3 targetPos = target.transform.position;
            
            // 1. MỤC TIÊU CHÍNH:
            if (target.isBoss)
            {
                // Boss KHÔNG chịu ảnh hưởng đạo tiêu tức khắc, nhận sát thương sấm sét chí mạng cực lớn
                target.TakeDamage(damage, true);
                if (GameLogger.Instance != null)
                {
                    GameLogger.Instance.Log($"⚡ LÔI PHẠT: Giáng thẳng vào BOSS, gây {damage:N0} sát thương chí mạng!", new Color(0.8f, 0f, 1f));
                }
            }
            else
            {
                // Quái thường: BỊ ĐẠO TIÊU NGAY TỨC KHẮC!
                float executeDamage = target.hp + 999999f;
                target.TakeDamage(executeDamage, true);
                if (GameLogger.Instance != null)
                {
                    GameLogger.Instance.Log("⚡ LÔI KIẾP: Thiên kiếp giáng lâm, yêu quái đã bị ĐẠO TIÊU ngay tức khắc!", Color.red);
                }
            }

            // 2. PHẦN ĐÁNH LAN (AoE Sóng xung kích bán kính 6m):
            Collider[] hitColliders = Physics.OverlapSphere(targetPos, 6f);
            foreach (var hit in hitColliders)
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null && !e.isDead && e != target)
                {
                    // Quái xung quanh chịu sát thương lan (60% sát thương sấm sét)
                    e.TakeDamage(damage * 0.6f, false);
                }
            }

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

            Debug.Log("[Skill] ⚡ LÔI PHẠT / LÔI KIẾP!");
        }
    }

    // --- ÂM DƯƠNG TRẬN (Buff toàn diện: Tốc độ kiếm, Sát thương, Hồi HP, Hồi MP) ---
    public bool isAmDuongTranActive = false;
    private float amDuongTranTimer = 0f;
    private int amDuongTranLevel = 1;

    public bool IsAmDuongTranActive => isAmDuongTranActive;
    public float GetAmDuongTranDamageMultiplier() => isAmDuongTranActive ? (1.30f + (amDuongTranLevel - 1) * 0.05f) : 1f; // +30% (+5%/lv)
    public float GetAmDuongTranSpeedMultiplier() => isAmDuongTranActive ? (1.50f + (amDuongTranLevel - 1) * 0.05f) : 1f; // +50% (+5%/lv)
    public float GetAmDuongTranHpRegenPercent() => 0.03f + (amDuongTranLevel - 1) * 0.005f; // 3% HP/s (+0.5%/lv)
    public float GetAmDuongTranMpRegenFlat() => 15f + (amDuongTranLevel - 1) * 3f; // 15 MP/s (+3/lv)

    public void CastAmDuongTran(int level = 1)
    {
        amDuongTranLevel = Mathf.Max(1, level);
        amDuongTranTimer = 6f; // Duy trì 6 giây
        isAmDuongTranActive = true;

        // Vòng trận pháp Thái Cực xoay dưới chân người chơi
        GameObject aura = new GameObject("AmDuongTranPivot");
        aura.transform.position = playerTransform.position + Vector3.up * 0.5f;
        AuraVFX auraVfx = aura.AddComponent<AuraVFX>();
        auraVfx.SetupYinYang();
        Destroy(aura, 6f);

        // Đổi màu vệt kiếm sang xanh ngọc lục bảo (Emerald) phát sáng rực rỡ
        if (activeSwords != null)
        {
            foreach (var sword in activeSwords)
            {
                if (sword != null && sword.gameObject.activeSelf)
                {
                    TrailRenderer tr = sword.GetComponent<TrailRenderer>();
                    if (tr != null) tr.startColor = new Color(0.2f, 1f, 0.6f);
                }
            }
        }

        Debug.Log($"[Skill] 👥 ÂM DƯƠNG TRẬN (Lv.{level}) kích hoạt! Tốc độ +{Mathf.RoundToInt((GetAmDuongTranSpeedMultiplier() - 1f) * 100)}%, Sát thương +{Mathf.RoundToInt((GetAmDuongTranDamageMultiplier() - 1f) * 100)}%, Hồi {GetAmDuongTranHpRegenPercent() * 100f:F1}% HP/s, Hồi {GetAmDuongTranMpRegenFlat():F0} MP/s.");
    }

    private void EndAmDuongTran()
    {
        isAmDuongTranActive = false;
        amDuongTranTimer = 0f;
        if (activeSwords != null)
        {
            foreach (var sword in activeSwords)
            {
                if (sword != null && sword.gameObject.activeSelf)
                {
                    TrailRenderer tr = sword.GetComponent<TrailRenderer>();
                    if (tr != null) tr.startColor = Color.cyan;
                }
            }
        }
        Debug.Log("[Skill] Âm Dương Trận kết thúc.");
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



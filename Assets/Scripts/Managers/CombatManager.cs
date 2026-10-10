using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public GameObject[] normalEnemyPrefabs;
    public GameObject[] bossEnemyPrefabs;
    public GameObject damagePopupPrefab; // Hiển thị số sát thương
    public Transform playerTransform;

    private float spawnTimer;
    public float spawnInterval = 0.2f; // Spawn nhanh hơn (0.2s)
    
    private List<Enemy> activeEnemies = new List<Enemy>();
    private const int MAX_ENEMIES = 50;

    public float playerMaxHp = 1000f;
    public float playerCurrentHp = 1000f;
    private float playerVisualHp = 1000f; // Dùng để trượt mượt mà
    private UnityEngine.UI.Image playerHpFill;
    private UnityEngine.UI.Text playerHpText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerMaxHp = 1000f;
        playerCurrentHp = 1000f;
        playerVisualHp = 1000f;
        CreatePlayerHPUI();
    }

    private void CreatePlayerHPUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        
        GameObject hpObj = new GameObject("PlayerHPBar", typeof(RectTransform));
        hpObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = hpObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(-150, 150); // Ở giữa dưới cùng, phía trên skill bar một chút
        rt.sizeDelta = new Vector2(300, 30);

        GameObject bg = new GameObject("BG", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        bg.transform.SetParent(hpObj.transform, false);
        bg.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0, 0.7f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        fill.transform.SetParent(bg.transform, false);
        playerHpFill = fill.GetComponent<UnityEngine.UI.Image>();
        playerHpFill.color = Color.green;
        // KHÔNG dùng Filled Type vì không có Sprite sẽ không hoạt động, dùng Anchor.
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; 
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; 
        fillRt.offsetMax = Vector2.zero;

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.Outline));
        txt.transform.SetParent(hpObj.transform, false);
        playerHpText = txt.GetComponent<UnityEngine.UI.Text>();
        playerHpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playerHpText.alignment = TextAnchor.MiddleCenter;
        playerHpText.color = Color.white;
        playerHpText.fontSize = 16;
        playerHpText.fontStyle = FontStyle.Bold;
        txt.GetComponent<UnityEngine.UI.Outline>().effectColor = Color.black;
        RectTransform txtRt = txt.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
        
        UpdatePlayerHPUI();
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
            
            // Tính toán lần đầu
            HandleStageChanged(null);
        }

        if (SimplePool.Instance != null)
        {
            // Pool toàn bộ quái thường
            if (normalEnemyPrefabs != null && normalEnemyPrefabs.Length > 0)
            {
                foreach (var prefab in normalEnemyPrefabs)
                {
                    if (prefab != null) SimplePool.Instance.AddPool(prefab.name, prefab, 20);
                }
            }
            
            // Pool toàn bộ boss
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
            int stage = CultivationManager.Instance.currentStageIndex;
            float oldMaxHp = playerMaxHp;
            playerMaxHp = 1000f * Mathf.Pow(1.2f, stage); // Máu tăng 20% mỗi cảnh giới
            
            // Hồi phục lượng máu chênh lệch khi lên cấp
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
        
        // Sinh Boss trên trục Z (phía trước) và ép cứng tọa độ Y bằng với Nhân vật
        Vector3 spawnPos = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z + 8f);
        GameObject go = null;
        if (SimplePool.Instance != null) go = SimplePool.Instance.SpawnFromPool(bossPrefab.name, spawnPos, Quaternion.identity);
        else go = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        
        go.transform.localScale = Vector3.one * 3f; // Boss to gấp 3
        
        currentBoss = go.GetComponent<Enemy>();
        
        int stageIndex = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 1;
        float bossHealth = 500f * Mathf.Pow(1.5f, stageIndex); // Máu boss tăng theo hàm mũ
        
        currentBoss.Init(bossHealth, playerTransform, true);
        activeEnemies.Add(currentBoss);
        
        // Bỏ logic đổi màu đỏ thủ công vì giờ dùng Asset 3D thật (Boss có màu riêng)
        
        Debug.Log($"[Combat] 👹 BOSS LÔI KIẾP XUẤT HIỆN! HP: {bossHealth:F0}");
    }

    private void Update()
    {
        // Trượt thanh máu mượt mà (Game Feel) - Xử lý ngay cả khi đánh Boss
        if (playerHpFill != null)
        {
            playerVisualHp = Mathf.Lerp(playerVisualHp, playerCurrentHp, Time.deltaTime * 10f);
            float fillRatio = Mathf.Clamp01(playerVisualHp / playerMaxHp);
            RectTransform fillRt = playerHpFill.GetComponent<RectTransform>();
            fillRt.anchorMax = new Vector2(fillRatio, 1f);
        }

        // Tạm ngưng sinh quái và hồi máu thụ động nếu đang ở trạng thái khác (Đánh Boss/Đột phá)
        // (Lưu ý: Game Feel HP trượt vẫn chạy ở trên)
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm)
            return;

        // Hồi máu thụ động mặc định của game (1% mỗi giây)
        if (playerCurrentHp > 0 && playerCurrentHp < playerMaxHp)
        {
            HealPlayer(playerMaxHp * 0.01f * Time.deltaTime);
        }

        // Hồi máu từ Shop (Hồi Huyết Đan: 5% mỗi giây)
        if (ShopManager.Instance != null && ShopManager.Instance.hpRegenVisualTimer > 0)
        {
            HealPlayer(playerMaxHp * 0.05f * Time.deltaTime);
        }

        if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ CombatManager: Chưa có Normal Enemy Prefabs! Hãy kéo Asset 3D Quái vào Inspector.");
            return;
        }

        spawnTimer -= Time.deltaTime;
        activeEnemies.RemoveAll(e => e == null);
        if (spawnTimer <= 0 && playerTransform != null && activeEnemies.Count < MAX_ENEMIES)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyPrefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
        
        // Sinh quái xuất hiện dọc theo trục Z của thế giới
        float randomX = Random.Range(-2f, 2f);
        Vector3 spawnPos = new Vector3(playerTransform.position.x + randomX, playerTransform.position.y, playerTransform.position.z + 15f); 
        
        GameObject go = null;
        if (SimplePool.Instance != null) go = SimplePool.Instance.SpawnFromPool(enemyPrefab.name, spawnPos, Quaternion.identity);
        else go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        go.transform.localScale = Vector3.one; // Reset scale (vì có thể tái sử dụng từ Boss)
        
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError($"[LỖI] Prefab '{enemyPrefab.name}' của bạn CHƯA ĐƯỢC GẮN script 'Enemy.cs'! Hãy mở Prefab đó lên và Add Component -> Enemy.");
            return;
        }
        
        int stageIndex = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
        float health = 50f * Mathf.Pow(1.5f, stageIndex); // Tăng theo Stage
        enemy.Init(health, playerTransform, false);
        
        activeEnemies.Add(enemy);
    }

    public void OnEnemyDied(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        
        if (enemy == currentBoss)
        {
            currentBoss = null;
            if (CultivationManager.Instance != null)
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
            
            // Xóa boss đang hiện diện khi player thua
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
            Debug.LogError("[Combat] playerTransform bị NULL! Không thể hiển thị kiếm.");
            return;
        }
        
        if (swordPrefab == null)
        {
            Debug.LogError("[Combat] swordPrefab bị NULL! CombatManager bị mất Prefab Phi Kiếm.");
            return;
        }
        
        for (int i = 0; i < 4; i++)
        {
            if (levels[i] > 0) // Ô có kiếm
            {
                if (activeSwords[i] == null)
                {
                    GameObject go = Instantiate(swordPrefab, playerTransform.position, Quaternion.identity);
                    activeSwords[i] = go.GetComponent<FlyingSword>();
                    Debug.Log($"[Combat] Đã Spawn thành công kiếm vật lý 3D cho slot {i}");
                }
                
                if (activeSwords[i] != null)
                {
                    activeSwords[i].gameObject.SetActive(true);
                    activeSwords[i].swordIndex = i; // Gán vị trí cho kiếm để không bị trùng lấp
                    
                    // Lấy data cơ bản (Damage, Speed) chuẩn từ thiết kế của Kiếm
                    activeSwords[i].baseDamage = FlyingSword.CalculateBaseDamage(levels[i]); 
                    activeSwords[i].baseSpeed = FlyingSword.CalculateBaseSpeed(levels[i]); 
                    
                    // Đổi hình ảnh kiếm 2D khớp với Level
                    activeSwords[i].SetLevelVisual(levels[i]);
                    
                    // Đổi màu đuôi sáng bay theo level cho đẹp
                    float hue = (levels[i] * 0.15f) % 1f;
                    Color swordColor = Color.HSVToRGB(hue, 0.7f, 0.9f);
                    TrailRenderer trail = activeSwords[i].GetComponent<TrailRenderer>();
                    if (trail != null) trail.startColor = swordColor;
                }
            }
            else // Ô trống
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
        
        // Triệu hồi 20 thanh phi kiếm màu Vàng Kim
        for (int i = 0; i < 20; i++)
        {
            GameObject go = Instantiate(swordPrefab, playerTransform.position, Quaternion.identity);
            FlyingSword sword = go.GetComponent<FlyingSword>();
            
            sword.swordIndex = i;
            sword.baseDamage = damage;
            sword.baseSpeed = 15f; // Tốc độ xé gió
            sword.isUltimateClone = true; // Cờ phân biệt quỹ đạo
            
            // Nhuộm vàng
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.yellow);
            block.SetColor("_Color", Color.yellow);
            sword.GetComponent<Renderer>().SetPropertyBlock(block);
            
            TrailRenderer trail = sword.GetComponent<TrailRenderer>();
            if (trail != null) trail.startColor = Color.yellow;
            
            ultimateSwords.Add(sword);
        }
        
        Debug.Log("[Skill] ⚔️ Đã kích hoạt Vạn Kiếm Quy Tông! Triệu hồi 20 Phi Kiếm!");
    }

    public void DeactivateVanKiemQuyTong()
    {
        foreach (var sword in ultimateSwords)
        {
            if (sword != null) Destroy(sword.gameObject);
        }
        ultimateSwords.Clear();
        Debug.Log("[Skill] Hết Mana. Thu hồi Vạn Kiếm.");
    }

    // Hàm cung cấp mục tiêu cho Phi Kiếm
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
            
            // Tính toán lượng sát thương dự kiến quái sẽ phải nhận từ các kiếm khác
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

            // Nếu quái đã nhận đủ sát thương chí tử từ các kiếm khác, thì kiếm này bỏ qua tìm con khác.
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
            nearest.TakeDamage(damage);
            
            // Hiệu ứng Bàn tay đè xuống (Ấn Chưởng)
            GameObject handVFX = new GameObject("GiantHand");
            handVFX.transform.position = nearest.transform.position + Vector3.up * 15f;
            GiantHandVFX gh = handVFX.AddComponent<GiantHandVFX>();
            gh.Setup(new Color(1f, 0.4f, 0f)); // Cam rực
            
            Debug.Log("[Skill] 🖐️ ẤN CHƯỞNG!");
        }
    }

    public void CastLoiPhat(float damage)
    {
        Enemy target = currentBoss != null ? currentBoss : GetNearestEnemy(playerTransform.position);
        if (target != null)
        {
            target.TakeDamage(damage);
            
            // Hiệu ứng Sét đánh từ trời xuống
            GameObject lightning = new GameObject("LightningStrike");
            Vector3 startPos = target.transform.position + Vector3.up * 20f;
            Vector3 endPos = target.transform.position;
            lightning.AddComponent<LightningVFX>().Setup(startPos, endPos, new Color(0.8f, 0.2f, 1f)); // Tím nhạt
            
            // Sóng xung kích nhỏ
            GameObject shockwave = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shockwave.transform.position = endPos + Vector3.up * 0.1f;
            shockwave.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            Material swMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            swMat.SetFloat("_Mode", 3);
            swMat.SetColor("_Color", new Color(0.8f, 0.2f, 1f, 0.6f));
            shockwave.GetComponent<Renderer>().material = swMat;
            Destroy(shockwave.GetComponent<Collider>());
            shockwave.AddComponent<ShockwaveVFX>();

            Debug.Log("[Skill] ⚡ LÔI PHẠT!");
        }
    }

    private bool isPhanThanActive = false;

    public void CastPhanThan()
    {
        if (isPhanThanActive) return;
        isPhanThanActive = true;

        // Hiệu ứng Phân Thân: Trận pháp xoay dưới chân
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        aura.transform.position = playerTransform.position + Vector3.up * 0.05f;
        aura.transform.localScale = new Vector3(6, 0.01f, 6);
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Mode", 3);
        mat.SetColor("_Color", new Color(0, 1f, 0, 0.5f)); // Xanh lá
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0, 0.5f, 0));
        aura.GetComponent<Renderer>().material = mat;
        
        Destroy(aura.GetComponent<Collider>());
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
        Invoke(nameof(EndPhanThan), 5f);
        Debug.Log("[Skill] 👥 PHÂN THÂN! (Tốc độ Phi Kiếm x2)");
    }

    private void EndPhanThan()
    {
        isPhanThanActive = false;
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
}

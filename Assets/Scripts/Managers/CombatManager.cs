using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public GameObject enemyPrefab;
    public GameObject damagePopupPrefab; // Hiển thị số sát thương
    public Transform playerTransform;

    private float spawnTimer;
    public float spawnInterval = 2f; // Cứ 2s đẻ 1 con
    
    private List<Enemy> activeEnemies = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
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
            if (e != null) Destroy(e.gameObject);
        }
        activeEnemies.Clear();
    }

    private Enemy currentBoss;

    private void SpawnBoss()
    {
        if (enemyPrefab == null || playerTransform == null) return;
        
        Vector3 spawnPos = playerTransform.position + new Vector3(8f, 0, 0); // Đứng đối diện
        GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * 3f; // Boss to gấp 3
        
        currentBoss = go.GetComponent<Enemy>();
        
        int stageIndex = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 1;
        float bossHealth = 500f * Mathf.Pow(1.5f, stageIndex); // Máu boss tăng theo hàm mũ
        
        currentBoss.Init(bossHealth, playerTransform);
        activeEnemies.Add(currentBoss);
        
        // Đổi màu Boss thành Đỏ nguy hiểm
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", Color.red);
        go.GetComponent<Renderer>().SetPropertyBlock(block);
        
        Debug.Log($"[Combat] 👹 BOSS LÔI KIẾP XUẤT HIỆN! HP: {bossHealth:F0}");
    }

    private void Update()
    {
        // Tạm ngưng sinh quái nếu đang ở trạng thái khác (Đánh Boss/Đột phá)
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.IdleFarm)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("⚠️ CombatManager: Chưa có Enemy Prefab! Bạn cần chạy menu TuTien -> Tạo hệ thống Combat 3D.");
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && playerTransform != null)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        // Sinh quái ngẫu nhiên trên mặt phẳng 3D XZ (cách player 5 unit để dễ nhìn thấy ngay)
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerTransform.position + new Vector3(randomDir.x, 0, randomDir.y) * 5f; 
        
        GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        
        int stageIndex = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 1;
        float health = 50f * Mathf.Pow(1.5f, stageIndex); // Máu quái thường tăng theo hàm mũ
        enemy.Init(health, playerTransform);
        
        activeEnemies.Add(enemy);
        Debug.Log($"[Combat] 😈 Sinh một Yêu Thú 3D tại tọa độ {spawnPos}");
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
            }
        }
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
                    
                    // Đổi màu kiếm 3D khớp với màu UI
                    float hue = (levels[i] * 0.15f) % 1f;
                    Color swordColor = Color.HSVToRGB(hue, 0.7f, 0.9f);
                    
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    block.SetColor("_BaseColor", swordColor);
                    block.SetColor("_Color", swordColor);
                    activeSwords[i].GetComponent<Renderer>().SetPropertyBlock(block);
                    
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
            Material swMat = new Material(Shader.Find("Standard"));
            swMat.SetFloat("_Mode", 3);
            swMat.SetColor("_Color", new Color(0.8f, 0.2f, 1f, 0.6f));
            shockwave.GetComponent<Renderer>().material = swMat;
            Destroy(shockwave.GetComponent<Collider>());
            shockwave.AddComponent<ShockwaveVFX>();

            Debug.Log("[Skill] ⚡ LÔI PHẠT!");
        }
    }

    public void CastPhanThan()
    {
        // Hiệu ứng Phân Thân: Trận pháp xoay dưới chân
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        aura.transform.position = playerTransform.position + Vector3.up * 0.05f;
        aura.transform.localScale = new Vector3(6, 0.01f, 6);
        
        Material mat = new Material(Shader.Find("Standard"));
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

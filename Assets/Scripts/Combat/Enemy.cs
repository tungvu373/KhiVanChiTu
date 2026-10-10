using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxHp = 50f;
    public float hp = 50f;
    public float moveSpeed = 2f;
    
    public bool isBoss = false;
    public float damage = 10f;
    public float attackSpeed = 1f; // Tấn công mỗi giây
    private float attackTimer = 0f;

    private Transform playerTarget;
    
    // UI & Game Feel
    private Image hpFill;
    private float visualHp;
    private Renderer[] renderers; // Hỗ trợ Model 3D có nhiều bộ phận
    private Color[] originalColors;
    private float flashTimer;
    private Camera mainCamera;
    
    // Animation
    public Animator animator;
    public bool isDead = false;
    
    [Header("Settings")]
    public float hpBarHeight = 2.5f; // Chiều cao thanh máu (tùy chỉnh trên Inspector)

    public float attackRange = 3.5f; // Khoảng cách đứng đánh (Viền pháp trận)

    public void Init(float health, Transform target, bool boss = false)
    {
        maxHp = health;
        hp = health;
        visualHp = health;
        playerTarget = target;
        isBoss = boss;
        damage = boss ? health * 0.2f : health * 0.05f; // Sát thương tỉ lệ theo máu
        isDead = false;

        CreateHPBar();

        // Lấy tất cả SkinnedMeshRenderer/MeshRenderer trên Model 3D
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
            else if (renderers[i].material.HasProperty("_BaseColor"))
                originalColors[i] = renderers[i].material.GetColor("_BaseColor");
            else
                originalColors[i] = Color.white;
        }
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        mainCamera = Camera.main;
    }

    private void CreateHPBar()
    {
        // Tạo Canvas World Space trên đầu quái
        GameObject canvasObj = new GameObject("HPCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, hpBarHeight, 0);
        
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        RectTransform cRt = canvasObj.GetComponent<RectTransform>();
        cRt.sizeDelta = new Vector2(1.5f, 0.2f); // Kích thước thanh máu

        // Background
        GameObject bgObj = new GameObject("HP_BG", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = Color.black;
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        // Fill
        GameObject fillObj = new GameObject("HP_Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(bgObj.transform, false);
        hpFill = fillObj.GetComponent<Image>();
        hpFill.color = Color.red; // Đổi lại thành đỏ theo yêu cầu
        // Không dùng Filled vì thiếu Sprite gốc sẽ lỗi, dùng Anchor thay thế
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; 
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; 
        fillRt.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        // Smooth HP Bar (Game Feel)
        if (hpFill != null)
        {
            visualHp = Mathf.Lerp(visualHp, hp, Time.deltaTime * 10f);
            float fillRatio = Mathf.Clamp01(visualHp / maxHp);
            RectTransform fillRt = hpFill.GetComponent<RectTransform>();
            fillRt.anchorMax = new Vector2(fillRatio, 1f);
            
            // Xoay thanh máu luôn nhìn về Camera chính
            if (mainCamera != null)
            {
                hpFill.transform.parent.parent.LookAt(hpFill.transform.parent.parent.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }

        // Đổi màu giật cục (Flash)
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                // Trả lại màu gốc
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        if (renderers[i].material.HasProperty("_Color"))
                            renderers[i].material.color = originalColors[i];
                        else if (renderers[i].material.HasProperty("_BaseColor"))
                            renderers[i].material.SetColor("_BaseColor", originalColors[i]);
                    }
                }
            }
        }

        if (playerTarget != null && !isDead)
        {
            if (isBoss)
            {
                // Boss KHÔNG di chuyển, chỉ xoay mặt về phía người chơi và tấn công
                Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
                transform.LookAt(targetPos);
                
                if (animator != null) animator.SetBool("IsMoving", false);
                
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    attackTimer = 1.5f; // Thời gian delay giữa 2 đòn đánh là 1.5s
                    
                    if (animator != null) animator.SetTrigger("Attack"); // Kích hoạt Anim Đánh
                    
                    if (CombatManager.Instance != null)
                    {
                        CombatManager.Instance.DamagePlayer(damage);
                    }
                }
            }
            else
            {
                // Quái thường: Di chuyển lại gần rồi mới đánh
                // BỎ QUA TRỤC Y khi tính khoảng cách (để tránh lỗi quái không bao giờ tới gần vì độ cao 3D)
                float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(playerTarget.position.x, playerTarget.position.z));
                if (dist > attackRange)
                {
                    // Di chuyển
                    Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
                    transform.LookAt(targetPos);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    
                    if (animator != null) animator.SetBool("IsMoving", true);
                }
                else
                {
                    // Tấn công Player
                    if (animator != null) animator.SetBool("IsMoving", false);
                    
                    if (attackTimer > 0)
                    {
                        attackTimer -= Time.deltaTime;
                    }
                    else
                    {
                        attackTimer = 1f / attackSpeed;
                        
                        if (animator != null) animator.SetTrigger("Attack"); // Kích hoạt Anim Đánh
                        
                        if (CombatManager.Instance != null)
                        {
                            CombatManager.Instance.DamagePlayer(damage);
                        }
                    }
                }
            }
        }
    }

    public void TakeDamage(float amount, bool isCrit = false)
    {
        hp -= amount;
        
        // Game Feel: Flash trắng cho toàn bộ Model 3D
        flashTimer = 0.1f;
        foreach (var r in renderers)
        {
            if (r != null)
            {
                if (r.material.HasProperty("_Color"))
                    r.material.color = Color.white;
                else if (r.material.HasProperty("_BaseColor"))
                    r.material.SetColor("_BaseColor", Color.white);
            }
        }
        
        if (CombatManager.Instance != null && CombatManager.Instance.damagePopupPrefab != null)
        {
            if (SimplePool.Instance != null)
            {
                GameObject popup = SimplePool.Instance.SpawnFromPool("DamagePopup", transform.position + Vector3.up * (hpBarHeight + 0.3f), Quaternion.identity);
                if (popup != null) popup.GetComponent<DamagePopup>().Setup(amount, isCrit || amount >= 400);
            }
            else
            {
                GameObject popup = Instantiate(CombatManager.Instance.damagePopupPrefab, transform.position + Vector3.up * (hpBarHeight + 0.3f), Quaternion.identity);
                popup.GetComponent<DamagePopup>().Setup(amount, isCrit || amount >= 400); 
            }
        }

        if (hp <= 0 && !isDead)
        {
            isDead = true;
            if (animator != null) animator.SetTrigger("Die");
            
            // Xóa Canvas máu ngay khi chết
            if (hpFill != null) hpFill.transform.parent.parent.gameObject.SetActive(false);
            
            // Đợi Animation chết chạy xong mới gọi Die()
            Invoke(nameof(Die), 1.5f); // Chờ 1.5s
        }
    }

    private void Die()
    {
        int stageIndex = 1;
        if (CultivationManager.Instance != null) stageIndex = CultivationManager.Instance.currentStageIndex;

        float baseDrop = 100f * Mathf.Pow(2.0f, stageIndex); // Rớt rất nhiều Linh Thạch
        if (isBoss) baseDrop *= 10f; // Boss rớt nhiều gấp 10 lần quái thường

        float tuLinhBonus = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetTuLinhValue() : 0;
        
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddLinhThach(baseDrop + tuLinhBonus);
        }
        
        if (CultivationManager.Instance != null)
        {
            float tuViBonus = 5f * Mathf.Pow(1.2f, stageIndex);
            if (isBoss) tuViBonus *= 5f;
            CultivationManager.Instance.AddTuVi(tuViBonus);
        }
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnEnemyDied(this);
        }
        
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyKilled(isBoss);
        }
        
        // Spawn Flying VFX (Game Feel)
        if (VFXHelper.Instance != null)
        {
            VFXHelper.Instance.SpawnFlyingLoot(transform.position, isBoss ? 5 : 1);
        }

        gameObject.SetActive(false); // Pooling thay vì Destroy
    }

    private void OnDestroy()
    {
        // Cleanup vật liệu để tránh rò rỉ bộ nhớ
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r != null && r.material != null) Destroy(r.material);
            }
        }
        
        if (hpFill != null)
        {
            Transform hpCanvas = hpFill.transform.parent?.parent;
            if (hpCanvas != null) Destroy(hpCanvas.gameObject);
        }
    }
}

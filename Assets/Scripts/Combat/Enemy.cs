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
    private Material mat;
    private Color originalColor;
    private float flashTimer;
    private Camera mainCamera;

    public void Init(float health, Transform target, bool boss = false)
    {
        maxHp = health;
        hp = health;
        visualHp = health;
        playerTarget = target;
        isBoss = boss;
        damage = boss ? health * 0.2f : health * 0.05f; // Sát thương tỉ lệ theo máu

        CreateHPBar();

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            mat = r.material;
            originalColor = mat.color;
        }
        mainCamera = Camera.main;
    }

    private void CreateHPBar()
    {
        // Tạo Canvas World Space trên đầu quái
        GameObject canvasObj = new GameObject("HPCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        
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
        hpFill.color = isBoss ? Color.magenta : Color.red;
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
        if (flashTimer > 0 && mat != null)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0) mat.color = originalColor;
        }

        if (playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist > 1.5f)
            {
                // Di chuyển
                Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
                transform.LookAt(targetPos);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                // Tấn công Player
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    attackTimer = 1f / attackSpeed;
                    if (CombatManager.Instance != null)
                    {
                        CombatManager.Instance.DamagePlayer(damage);
                    }
                }
            }
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        
        // Game Feel: Flash trắng
        if (mat != null)
        {
            mat.color = Color.white;
            flashTimer = 0.1f;
        }
        
        if (CombatManager.Instance != null && CombatManager.Instance.damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(CombatManager.Instance.damagePopupPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(amount, amount >= 400); 
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        int stageIndex = 1;
        if (CultivationManager.Instance != null) stageIndex = CultivationManager.Instance.currentStageIndex;

        float baseDrop = 10f * Mathf.Pow(1.3f, stageIndex);
        if (isBoss) baseDrop *= 5f; // Boss rớt nhiều gấp 5

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
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
        if (hpFill != null)
        {
            Transform hpCanvas = hpFill.transform.parent?.parent;
            if (hpCanvas != null) Destroy(hpCanvas.gameObject);
        }
    }
}

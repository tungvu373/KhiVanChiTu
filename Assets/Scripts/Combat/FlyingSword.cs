using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    public float baseDamage = 10f;
    public float baseSpeed = 5f;
    public int swordIndex = 0; // Biến phân bổ vị trí bay
    public bool isUltimateClone = false; // Đánh dấu đây là kiếm của Ultimate
    
    [Header("Sprites cho từng Level (1-8)")]
    public Sprite[] levelSprites;
    
    // Tách biệt rạch ròi Data của Kiếm và Data Nâng cấp (Upgrade)
    public static float CalculateBaseDamage(int level) { return level * level * 10f; }
    public static float CalculateBaseSpeed(int level) { return 5f + (level * 2f); }

    public Enemy TargetEnemy { get; private set; }
    private float searchTimer = 0f;

    // === CHỐNG DÍNH KIẾM: Cooldown + Retreat ===
    private float attackCooldown = 0f;
    private const float ATTACK_COOLDOWN_TIME = 0.45f; // Thời gian hồi chiêu giữa 2 nhát chém (giây)
    private const float RETREAT_DURATION = 0.25f; // Thời gian bay lùi ra xa sau khi chém
    private float retreatTimer = 0f;

    public float GetExpectedDamage()
    {
        float kiemY = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetKiemYValue() : 0;
        float bonusMultiplier = CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f;
        if (ShopManager.Instance != null) bonusMultiplier *= ShopManager.Instance.GetDamageMultiplier();
        if (EconomyManager.Instance != null) bonusMultiplier *= (1f + EconomyManager.Instance.currentKiemY * 0.5f);
        if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive) bonusMultiplier *= CombatManager.Instance.GetAmDuongTranDamageMultiplier();
        return (baseDamage + kiemY) * bonusMultiplier;
    }

    private void Start()
    {
        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null)
        {
            if (tr.sharedMaterial == null || tr.sharedMaterial.name == "Default-Material")
            {
                tr.material = new Material(Shader.Find("Sprites/Default"));
            }
            tr.startWidth = 0.4f;
            tr.endWidth = 0f;
            tr.time = 0.25f;
        }
    }

    private void Update()
    {
        // Đếm ngược cooldown tấn công và tìm mục tiêu
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        if (searchTimer > 0f) searchTimer -= Time.deltaTime;

        // === PHASE 1: RETREAT (Bay lùi về quỹ đạo quanh Player sau khi chém) ===
        if (retreatTimer > 0f)
        {
            retreatTimer -= Time.deltaTime;
            Vector3 center = CombatManager.Instance != null ? CombatManager.Instance.playerTransform.position + Vector3.up * 1.5f : Vector3.up * 1.5f;
            float currentRadius = isUltimateClone ? 4f : 2f;
            float angleOffset = isUltimateClone ? (360f / 20f) : 90f;
            float angle = Time.time * (isUltimateClone ? 200f : 150f) + (swordIndex * angleOffset);
            Vector3 retreatPos = center + Quaternion.Euler(0, angle, 0) * new Vector3(currentRadius, 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, retreatPos, 18f * Time.deltaTime);

            Vector3 radius = transform.position - center;
            Vector3 tangent = Vector3.Cross(Vector3.up, radius);
            if (tangent != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(tangent), 10f * Time.deltaTime);
            return;
        }

        // === PHASE 2: TÌM MỤC TIÊU (Orbiting quanh Player khi không có target) ===
        if (TargetEnemy == null || !TargetEnemy.gameObject.activeInHierarchy || TargetEnemy.isDead || TargetEnemy.hp <= 0)
        {
            TargetEnemy = null;
            if (searchTimer <= 0f)
            {
                if (CombatManager.Instance != null)
                {
                    TargetEnemy = CombatManager.Instance.GetOptimalTarget(this);
                }
                searchTimer = 0.2f; 
            }
            
            Vector3 center = CombatManager.Instance != null ? CombatManager.Instance.playerTransform.position + Vector3.up * 1.5f : Vector3.up * 1.5f;
            
            // Xếp đội hình quỹ đạo: Kiếm thường bay vòng trong (2m, 4 góc vuông). Kiếm Ultimate bay vòng ngoài (4m, 20 góc).
            float currentRadius = isUltimateClone ? 4f : 2f;
            float angleOffset = isUltimateClone ? (360f / 20f) : 90f;
            float angle = Time.time * (isUltimateClone ? 200f : 150f) + (swordIndex * angleOffset);
            
            Vector3 targetOrbitPos = center + Quaternion.Euler(0, angle, 0) * new Vector3(currentRadius, 0, 0);
            
            // Di chuyển mượt về điểm quỹ đạo (rất đẹp khi kiếm bay từ xác quái về Player)
            transform.position = Vector3.MoveTowards(transform.position, targetOrbitPos, 10f * Time.deltaTime);
            
            // Xoay mũi kiếm hướng về phía trước theo quỹ đạo tròn (tiếp tuyến)
            Vector3 radius = transform.position - center;
            Vector3 tangent = Vector3.Cross(Vector3.up, radius);
            if (tangent != Vector3.zero) 
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(tangent), 10f * Time.deltaTime);
                
            return;
        }

        // === PHASE 3: LAO TỚI MỤC TIÊU VÀ TẤN CÔNG ===
        // Sát thương và Tốc độ = Base (Từ Phi Kiếm) + Buff (Từ Upgrade) + Buff (Từ Shop)
        float thanThuc = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetThanThucValue() : 0;
        if (ShopManager.Instance != null) thanThuc *= ShopManager.Instance.GetSpiritMultiplier();
        
        float finalSpeed = baseSpeed + thanThuc;
        if (ShopManager.Instance != null) finalSpeed *= ShopManager.Instance.GetSpeedMultiplier();
        if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive) finalSpeed *= CombatManager.Instance.GetAmDuongTranSpeedMultiplier();
        
        if (TargetEnemy == null) return;
        Vector3 targetPos = TargetEnemy.transform.position + Vector3.up * 1.5f;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, finalSpeed * Time.deltaTime);
        
        // Chĩa mũi kiếm (trục Z) về phía quái vật
        Vector3 dir = targetPos - transform.position;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Kiểm tra va chạm (khoảng cách < 0.5) VÀ cooldown đã hết
        if (attackCooldown <= 0f && Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            AttackTarget();
        }
    }

    private void AttackTarget()
    {
        if (TargetEnemy == null) return;

        float expectedDamage = GetExpectedDamage();
        
        // Random sát thương dao động +- 20%
        float minDamage = expectedDamage * 0.8f;
        float maxDamage = expectedDamage * 1.2f;
        float finalDamage = Random.Range(minDamage, maxDamage);
        
        // Tỉ lệ Crit 20%, Crit x2 sát thương
        bool isCrit = Random.value <= 0.2f;
        if (isCrit) 
        {
            finalDamage *= 2f;
        }
        
        Enemy hitEnemy = TargetEnemy;
        hitEnemy.TakeDamage(finalDamage, isCrit);

        // VẠN KIẾM QUY TÔNG: Đẩy lùi mục tiêu về phía sau (ra xa khỏi Player) - Trừ Boss không bị đẩy lùi
        if (isUltimateClone && !hitEnemy.isBoss)
        {
            Vector3 pushDir = hitEnemy.transform.position - (CombatManager.Instance != null ? CombatManager.Instance.playerTransform.position : transform.position);
            pushDir.y = 0f;
            hitEnemy.Knockback(pushDir.normalized, 1.8f);
        }
        
        // Phát âm thanh va chạm kiếm chém trúng quái / boss
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwordHitSFX();
        }
        
        // CHỐNG DÍNH: Hủy mục tiêu → kích hoạt cooldown + retreat bay lùi ra quỹ đạo quanh Player
        TargetEnemy = null; 
        attackCooldown = ATTACK_COOLDOWN_TIME;
        retreatTimer = RETREAT_DURATION;
        searchTimer = ATTACK_COOLDOWN_TIME; // Không tìm mục tiêu mới cho tới khi cooldown hết
    }

    public void SetLevelVisual(int level)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && levelSprites != null && levelSprites.Length > 0)
        {
            // Level 1 -> index 0. Dùng Clamp để không bị lỗi quá mảng
            int index = Mathf.Clamp(level - 1, 0, levelSprites.Length - 1);
            sr.sprite = levelSprites[index];
        }
    }

    /// <summary>
    /// Đặt lại mục tiêu của kiếm khi hồi sinh hoặc đổi ải (để kiếm bay về quanh Player)
    /// </summary>
    public void ResetTarget()
    {
        TargetEnemy = null;
        searchTimer = 0.4f;
        attackCooldown = 0f;
        retreatTimer = 0f;
    }
}


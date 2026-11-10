using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    public float baseDamage = 10f;
    public float baseSpeed = 5f;
    public int swordIndex = 0;
    public int currentLevel { get; private set; } = 1; // Biến phân bổ vị trí bay
    public int formationCount = 4; // tổng số kiếm đang active; dùng để tính góc orbit đều nhau
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
    private float bladePunchTimer = 0f;
    private Enemy lastStyleTarget;
    private int consecutiveStyleHits;
    private int lastStyleTargetVersion;
    private TrailRenderer trail;
    private Color swordBaseColor = Color.white;
    public enum SwordMotionState { Orbiting, Dashing, Retreating }
    private SwordMotionState currentMotionState = SwordMotionState.Orbiting;

    private void OnEnable()
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();
        UpdateTrailState(SwordMotionState.Orbiting);
    }

    private void OnDisable()
    {
        lastStyleTarget = null;
        consecutiveStyleHits = 0;
        if (trail != null) trail.Clear();
    }

    public void UpdateTrailState(SwordMotionState newState)
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (trail == null) return;
        currentMotionState = newState;
        switch (newState)
        {
            case SwordMotionState.Orbiting:
                trail.time = 0.15f;
                trail.startWidth = 0.22f;
                trail.endWidth = 0f;
                SetTrailGradient(0.28f);
                break;
            case SwordMotionState.Dashing:
                trail.time = 0.28f;
                trail.startWidth = 0.40f;
                trail.endWidth = 0.04f;
                SetTrailGradient(0.95f);
                break;
            case SwordMotionState.Retreating:
                trail.time = 0.10f;
                trail.startWidth = 0.18f;
                trail.endWidth = 0f;
                SetTrailGradient(0.18f);
                break;
        }
    }

    private void SetTrailGradient(float peakAlpha)
    {
        if (trail == null) return;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(swordBaseColor, 0f), new GradientColorKey(swordBaseColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(peakAlpha, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trail.colorGradient = g;
    }

    private float cachedExpectedDamage = -1f;
    private float damageCacheTimer = 0f;

    public float GetExpectedDamage()
    {
        if (cachedExpectedDamage > 0f && damageCacheTimer > 0f) return cachedExpectedDamage;
        float kiemY = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetKiemYValue() : 0;
        float bonusMultiplier = CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f;
        if (ShopManager.Instance != null) bonusMultiplier *= ShopManager.Instance.GetDamageMultiplier();
        if (DaoAnProgressionService.Instance != null)
            bonusMultiplier *= DaoAnProgressionService.Instance.GetMultiplier(DaoAnEffect.SwordDamage);
        if (EconomyManager.Instance != null) bonusMultiplier *= (1f + EconomyManager.Instance.currentKiemY * 0.5f);
        if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive) bonusMultiplier *= CombatManager.Instance.GetAmDuongTranDamageMultiplier();
        if (CombatManager.Instance != null && CombatManager.Instance.isTribulationBossActive && AlchemyService.Instance != null)
            bonusMultiplier *= AlchemyService.Instance.TribulationDamageMultiplier;
        float bodyDamage = PlayerStatsService.Instance != null ? PlayerStatsService.Instance.BonusSwordDamage : 0f;
        cachedExpectedDamage = (baseDamage + kiemY + bodyDamage) * bonusMultiplier;
        damageCacheTimer = 0.25f;
        return cachedExpectedDamage;
    }

    private void Start()
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            if (trail.sharedMaterial == null || trail.sharedMaterial.name == "Default-Material")
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                trail.material = new Material(shader);
            }
            UpdateTrailState(currentMotionState);
        }
    }

    private void Update()
    {
        // Đếm ngược cooldown tấn công và tìm mục tiêu
        if (attackCooldown > 0f) attackCooldown -= Time.deltaTime;
        if (damageCacheTimer > 0f) damageCacheTimer -= Time.deltaTime;
        if (searchTimer > 0f) searchTimer -= Time.deltaTime;

        // Game Feel: Lực chém đanh thép của kiếm khi chạm địch (Blade Punch Scale)
        if (bladePunchTimer > 0f)
        {
            bladePunchTimer -= Time.deltaTime;
            float p = Mathf.Clamp01(bladePunchTimer / 0.08f);
            transform.localScale = Vector3.one * (1f + 0.35f * p);
        }
        else if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
        }

        // === PHASE 1: RETREAT (Bay lùi về quỹ đạo quanh Player sau khi chém) ===
        if (retreatTimer > 0f)
        {
            if (currentMotionState != SwordMotionState.Retreating) UpdateTrailState(SwordMotionState.Retreating);
            retreatTimer -= Time.deltaTime;
            Vector3 center = CombatManager.Instance != null ? CombatManager.Instance.playerTransform.position + Vector3.up * 1.5f : Vector3.up * 1.5f;
            float currentRadius = isUltimateClone ? 4f : 2f;
            float angleOffset = isUltimateClone ? (360f / 20f) : (360f / Mathf.Max(1, formationCount));
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
            if (currentMotionState != SwordMotionState.Orbiting) UpdateTrailState(SwordMotionState.Orbiting);
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
            float angleOffset = isUltimateClone ? (360f / 20f) : (360f / Mathf.Max(1, formationCount));
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
        if (currentMotionState != SwordMotionState.Dashing) UpdateTrailState(SwordMotionState.Dashing);
        // Sát thương và Tốc độ = Base (Từ Phi Kiếm) + Buff (Từ Upgrade) + Buff (Từ Shop)
        float thanThuc = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetThanThucValue() : 0;
        if (ShopManager.Instance != null) thanThuc *= ShopManager.Instance.GetSpiritMultiplier();
        
        float finalSpeed = baseSpeed + thanThuc;
        if (ShopManager.Instance != null) finalSpeed *= ShopManager.Instance.GetSpeedMultiplier();
        if (CombatManager.Instance != null && CombatManager.Instance.IsAmDuongTranActive) finalSpeed *= CombatManager.Instance.GetAmDuongTranSpeedMultiplier();
        if (ArtifactService.Instance != null) finalSpeed *= ArtifactService.Instance.SwordSpeedMultiplier;
        if (FormationService.Instance != null) finalSpeed *= FormationService.Instance.SwordSpeedMultiplier;
        finalSpeed *= 1f + DaoAnProgressionService.Bonus(DaoAnEffect.SwordFlightSpeed);
        
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
        float artifactCrit = ArtifactService.Instance != null ? ArtifactService.Instance.CritChanceBonus : 0f;
        float daoAnCrit = DaoAnProgressionService.Bonus(DaoAnEffect.SwordCritChance);
        bool isCrit = Random.value <= Mathf.Clamp01(0.2f + artifactCrit + daoAnCrit);
        if (isCrit) 
        {
            finalDamage *= 2f;
        }
        
        Enemy hitEnemy = TargetEnemy;

        // BOSS ĐỘ KIẾP KHẮC CHẾ VẠN KIẾM QUY TÔNG:
        // Boss Độ Kiếp chỉ nhận giới hạn 20% sát thương từ kỹ năng Vạn Kiếm Quy Tông (giảm 80% sát thương của skill)
        if (isUltimateClone)
        {
            BossEnemy boss = hitEnemy as BossEnemy;
            if (boss != null && boss.isTribulationBoss)
            {
                finalDamage *= 0.20f; // Chỉ nhận tối đa 20% sát thương
            }
        }

        Vector3 slashDir = transform.forward;
        if (slashDir.sqrMagnitude < 0.001f)
        {
            slashDir = (hitEnemy.transform.position - transform.position).normalized;
        }
        slashDir.y = 0f;
        if (slashDir.sqrMagnitude > 0.001f) slashDir.Normalize();

        bladePunchTimer = 0.08f;
        if (!isUltimateClone && GameplayChoicesService.Instance != null)
        {
            int style = GameplayChoicesService.Instance.Style;
            if (style == 1)
            {
                consecutiveStyleHits = lastStyleTarget == hitEnemy && lastStyleTargetVersion == hitEnemy.SpawnVersion
                    ? Mathf.Min(4, consecutiveStyleHits + 1) : 0;
                lastStyleTarget = hitEnemy;
                lastStyleTargetVersion = hitEnemy.SpawnVersion;
                finalDamage *= 1f + consecutiveStyleHits * 0.2f;

                if (consecutiveStyleHits > 0 && HitVFXManager.Instance != null)
                {
                    HitVFXManager.Instance.SpawnArmorBreakStack(hitEnemy.transform.position + Vector3.up * 1f, consecutiveStyleHits);
                }

                var guard = hitEnemy.GetComponent<OpeningBossGuard>();
                if (guard != null) guard.Absorb(finalDamage);
            }
            else
            {
                lastStyleTarget = null; consecutiveStyleHits = 0;
                if (style == 0 && CombatManager.Instance != null)
                {
                    int hitCount = 0;
                    Vector3 direction = (CombatManager.Instance.playerTransform != null)
                        ? hitEnemy.transform.position - CombatManager.Instance.playerTransform.position
                        : slashDir;
                    if (direction.sqrMagnitude < 0.001f) direction = slashDir.sqrMagnitude > 0.001f ? slashDir : Vector3.forward;
                    direction.y = 0; direction.Normalize();
                    var candidates = CombatManager.Instance.ActiveEnemies;
                    for (int index = candidates.Count - 1; index >= 0; index--)
                    {
                        if (index >= candidates.Count) continue;
                        var other = candidates[index];
                        if (other == null) continue;
                        if (other == hitEnemy || other.isDead || other.hp <= 0) continue;
                        Vector3 offset = other.transform.position - hitEnemy.transform.position; offset.y = 0;
                        float along = Vector3.Dot(offset, direction);
                        if (along < 0 || along > 6 || (offset - direction * along).sqrMagnitude > 4) continue;

                        if (HitVFXManager.Instance != null)
                        {
                            HitVFXManager.Instance.SpawnPierceTracer(hitEnemy.transform.position + Vector3.up * 1f, other.transform.position + Vector3.up * 1f);
                        }

                        other.TakeDamage(ApplyBossBonus(other, finalDamage * 0.5f), isCrit, direction);
                        if (++hitCount == 2) break;
                    }
                }
            }
        }
        finalDamage = ApplyBossBonus(hitEnemy, finalDamage);
        hitEnemy.TakeDamage(finalDamage, isCrit, slashDir);
        if (!isUltimateClone && CombatManager.Instance != null && PlayerStatsService.Instance != null)
            CombatManager.Instance.HealPlayer(finalDamage * PlayerStatsService.Instance.LifestealPercent);
        if (!isUltimateClone && FormationService.Instance != null)
            FormationService.Instance.OnSwordHitEnemy(finalDamage, hitEnemy.transform.position);

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

    private static float ApplyBossBonus(Enemy target, float damage) => target.isBoss
        ? damage * (1f + DaoAnProgressionService.Bonus(DaoAnEffect.SwordBossDamage))
        : damage;

    public void SetLevelVisual(int level)
    {
        currentLevel = level;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && levelSprites != null && levelSprites.Length > 0)
        {
            // Level 1 -> index 0. Dùng Clamp để không bị lỗi quá mảng
            int index = Mathf.Clamp(level - 1, 0, levelSprites.Length - 1);
            sr.sprite = levelSprites[index];
        }
        float hue = (level * 0.15f) % 1f;
        swordBaseColor = Color.HSVToRGB(hue, 0.7f, 0.95f);
        UpdateTrailState(currentMotionState);
    }

    /// <summary>
    /// Đặt lại mục tiêu của kiếm khi hồi sinh hoặc đổi ải (để kiếm bay về quanh Player)
    /// </summary>
    public void RestoreBaseTrailColor()
    {
        UpdateTrailState(currentMotionState);
    }

    public void ResetTarget()
    {
        lastStyleTarget = null;
        consecutiveStyleHits = 0;
        TargetEnemy = null;
        searchTimer = 0.4f;
        attackCooldown = 0f;
        retreatTimer = 0f;
        if (trail != null) trail.Clear();
        UpdateTrailState(SwordMotionState.Orbiting);
    }
}

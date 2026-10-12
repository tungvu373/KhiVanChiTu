using UnityEngine;
using System.Collections;

/// <summary>
/// Quản lý hành vi và hoạt ảnh của Boss (Tấn công uy lực, bước đi oai vệ, chống giật animation).
/// Tích hợp đầy đủ các tham số Animator: IsMoving (bool), Attack (trigger), Die (trigger).
/// </summary>
public class BossEnemy : Enemy
{
    [Header("--- Boss Animation & Motion Settings ---")]
    [Tooltip("Tốc độ xoay hướng nhìn mượt mà về phía người chơi (độ/giây)")]
    public float turnSpeed = 360f;

    [Tooltip("Độ trễ chống rung giật animation Walk/Idle khi đứng sát mép tầm đánh (Hysteresis)")]
    public float stoppingBuffer = 0.35f;

    [Tooltip("Khoảng thời gian hồi giữa các đòn đánh của Boss")]
    public float attackInterval = 1.8f;

    [Tooltip("Thời gian chờ từ lúc vung tay đến lúc đòn đánh chạm đích gây sát thương")]
    public float attackImpactDelay = 0.35f;

    [Tooltip("Thời gian hồi phục sau đòn đánh trước khi Boss có thể bước đi lại")]
    public float attackRecovery = 0.8f;

    // Animator Parameter Hashes (Tối ưu hiệu năng, tránh GC Alloc chuỗi string)
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private bool hasHashIsMoving = false;
    private bool hasHashAttack = false;
    private bool hasHashDie = false;

    // Quản lý trạng thái nội bộ
    private bool isAttacking = false;
    private bool isMovingState = false;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        CacheAnimatorParameters();
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        isAttacking = false;
        isMovingState = false;
        isDead = false;
    }

    /// <summary>
    /// Tính toán máu của Boss theo tầng: Cân bằng tầng 1-4 thấp xuống để người chơi dễ dàng farm đầu game.
    /// </summary>
    public static float CalculateBossHealth(int stage, bool isTribulation)
    {
        float baseHp;
        if (stage <= 1) baseHp = 350f;
        else if (stage == 2) baseHp = 750f;
        else if (stage == 3) baseHp = 1400f;
        else if (stage == 4) baseHp = 2500f;
        else
        {
            // Tầng 5 trở đi: Khởi điểm từ 4000 HP, tăng trưởng đều theo tầng
            float stageScale = (stage - 5) / 5f;
            baseHp = 4000f * Mathf.Pow(2.2f, stageScale);
        }

        if (isTribulation) baseHp *= 1.35f;
        return baseHp;
    }

    /// <summary>
    /// Tính toán sát thương của Boss theo tầng: Cân bằng tầng 1-4 không bị one-shot người chơi.
    /// </summary>
    public static float CalculateBossDamage(int stage, bool isTribulation)
    {
        float finalDamage;
        if (stage <= 1) finalDamage = 8f;
        else if (stage == 2) finalDamage = 14f;
        else if (stage == 3) finalDamage = 22f;
        else if (stage == 4) finalDamage = 32f;
        else finalDamage = 45f + ((stage - 5) / 5f) * 25f;

        if (isTribulation) finalDamage *= 1.25f;
        return finalDamage;
    }

    public void InitBoss(int stage, Transform target, bool isTribulation = false)
    {
        // 1. Máu (HP) của Boss: Cân bằng thấp ở tầng 1-4
        float finalHealth = CalculateBossHealth(stage, isTribulation);

        // Chiều cao thanh máu cao hơn đầu Boss (Model Boss to 1.5x)
        if (this.hpBarHeight < 3.2f) this.hpBarHeight = 3.5f;

        // Tầm đánh cận chiến của Boss (Boss to hơn nên áp sát cự ly 2.4m)
        this.attackRange = 2.4f;
        if (this.moveSpeed < 1f) this.moveSpeed = 1.8f;

        // Gọi hàm Init của lớp cha (Enemy)
        base.Init(finalHealth, target, true);

        // Đảm bảo vị trí Canvas thanh máu nằm cao hơn đầu Boss
        Transform hpCanvas = transform.Find("HPCanvas");
        if (hpCanvas != null)
        {
            hpCanvas.localPosition = new Vector3(0, hpBarHeight, 0);
        }

        // 2. Tính toán Sát thương (Damage)
        float finalDamage = CalculateBossDamage(stage, isTribulation);
        this.damage = finalDamage;

        // Kích thước to hơn để thể hiện sự đáng sợ, uy nghiêm
        transform.localScale = Vector3.one * 1.5f;

        // Reset trạng thái chiến đấu & hoạt ảnh
        isAttacking = false;
        isMovingState = false;
        attackTimer = 0.5f; // Thời gian khởi động ngắn trước đòn đánh đầu tiên

        CacheAnimatorParameters();
        SetMoving(false);

        Debug.Log($"[BossEnemy] Sinh ra Boss! Tầng {stage}, Máu: {finalHealth:F0}, Sát thương: {finalDamage:F0}, Lôi Kiếp: {isTribulation}");
    }

    /// <summary>
    /// Kiểm tra và lưu cache các tham số thực tế có trong Animator của Boss.
    /// </summary>
    private void CacheAnimatorParameters()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = GetComponentInParent<Animator>();
        }

        hasHashIsMoving = false;
        hasHashAttack = false;
        hasHashDie = false;

        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.name == "IsMoving" && p.type == AnimatorControllerParameterType.Bool) hasHashIsMoving = true;
                if (p.name == "Attack" && p.type == AnimatorControllerParameterType.Trigger) hasHashAttack = true;
                if (p.name == "Die" && p.type == AnimatorControllerParameterType.Trigger) hasHashDie = true;
            }
        }
    }

    /// <summary>
    /// Ghi đè logic di chuyển và chiến đấu của Boss để mượt mà, không bị giật lùi hay ngắt quãng hoạt ảnh.
    /// </summary>
    protected override void UpdateMovementAndCombat()
    {
        if (playerTarget == null || isDead) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(playerTarget.position.x, currentPos.y, playerTarget.position.z);

        // 1. Nếu Boss đang trong hoạt ảnh vung đòn (Attack):
        if (isAttacking)
        {
            // Đảm bảo không kích hoạt walk khi đang đánh
            SetMoving(false);

            // Vẫn xoay nhẹ nhàng bám theo Player để cú đánh chuẩn hướng
            Vector3 attackLookDir = (targetPos - currentPos).normalized;
            if (attackLookDir.sqrMagnitude > 0.001f)
            {
                Quaternion attackRot = Quaternion.LookRotation(attackLookDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, attackRot, (turnSpeed * 0.5f) * Time.deltaTime);
            }
            return; // Đứng yên tại chỗ, không trượt chân khi đang vung vuốt/đao
        }

        // 2. Tính toán cự ly trên mặt phẳng 2D X-Z (Bỏ qua trục Y độ cao)
        float dist = Vector2.Distance(new Vector2(currentPos.x, currentPos.z), new Vector2(targetPos.x, targetPos.z));

        // 3. Xoay người mượt mà theo hướng Player
        Vector3 lookDir = (targetPos - currentPos).normalized;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // 4. Thuật toán Hysteresis chống rung giật hoạt ảnh Đi/Đứng:
        // - Khi đang đi: Chỉ dừng khi đã vào hẳn cự ly attackRange.
        // - Khi đang đứng: Chỉ bước tiếp khi cự ly vượt xa tầm đánh + stoppingBuffer.
        float resumeDistance = attackRange + stoppingBuffer;
        bool shouldMove = isMovingState ? (dist > attackRange) : (dist > resumeDistance);

        if (shouldMove)
        {
            // Boss bước đi uy nghi tiến dần tới sát người chơi
            SetMoving(true);
            transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Đã áp sát tầm đánh -> Dừng bước chân và chuyển về thế Idle/Attack
            SetMoving(false);

            // Đếm ngược thời gian hồi đòn đánh
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                // Kích hoạt chuỗi đòn đánh của Boss
                attackTimer = attackInterval;
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(BossAttackRoutine());
            }
        }
    }

    public override void Stun(float duration)
    {
        // Boss miễn nhiễm hoàn toàn hiệu ứng Choáng!
    }

    public override void Knockback(Vector3 direction, float distance)
    {
        // Boss miễn nhiễm hoàn toàn hiệu ứng Đẩy lùi!
    }

    /// <summary>
    /// Coroutine thực hiện đòn đánh: Đồng bộ giữa hoạt ảnh, thời điểm vung trúng đích và hồi phục.
    /// </summary>
    private IEnumerator BossAttackRoutine()
    {
        isAttacking = true;
        SetMoving(false);

        // Kích hoạt trigger Attack trên Animator
        if (animator != null && hasHashAttack)
        {
            animator.ResetTrigger(HashAttack);
            animator.SetTrigger(HashAttack);
        }

        // 1. Chờ đến đúng thời điểm vung tay/vuốt chạm đích
        yield return new WaitForSeconds(attackImpactDelay);

        if (!isDead && playerTarget != null)
        {
            // Gây sát thương lên Player (CombatManager tự động kích hoạt viền chớp đỏ BossDamageVignette)
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.DamagePlayer(damage, true);
            }

            // Game Feel: Rung màn hình khi Boss giáng đòn nặng nề
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.25f, 0.2f);
            }
        }

        // 2. Chờ kết thúc nốt phần phục hồi của hoạt ảnh đánh
        float remainingRecovery = Mathf.Max(0.1f, attackRecovery - attackImpactDelay);
        yield return new WaitForSeconds(remainingRecovery);

        isAttacking = false;
        attackCoroutine = null;
    }

    /// <summary>
    /// Cập nhật trạng thái IsMoving vào Animator một cách an toàn và chỉ cập nhật khi thay đổi.
    /// </summary>
    private void SetMoving(bool moving)
    {
        if (isMovingState == moving) return;
        isMovingState = moving;

        if (animator != null && hasHashIsMoving)
        {
            animator.SetBool(HashIsMoving, moving);
        }
    }

    /// <summary>
    /// Xử lý khi nhận sát thương hoặc tử trận.
    /// </summary>
    public override void TakeDamage(float amount, bool isCrit = false)
    {
        if (hp <= amount && !isDead)
        {
            // Hủy coroutine đánh nếu Boss tử trận giữa chừng
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            isAttacking = false;
            SetMoving(false);
        }

        base.TakeDamage(amount, isCrit);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý hành vi, hoạt ảnh và cơ chế chiến đấu của Boss.
/// Tích hợp cơ chế chuẩn cho BOSS THIÊN KIẾP (ĐỘ KIẾP):
/// 1. Sát thương gây ra tính chuẩn xác theo % Máu Tối Đa của Player: 1 đòn = 15% Max HP (7 đòn chắc chắn tử trận).
/// 2. Đợt tấn công hoạt động chuẩn theo Animation và sau mỗi 1.5s:
///    - Vung đòn theo hoạt ảnh -> Chạm đích ở giây 0.48 -> Trừ 15% HP Player kèm rung màn hình -> Thu đòn hoàn tất chu kỳ 1.5s.
///    - Tuyệt đối không trừ máu khi Boss đứng yên.
/// 3. Cơ chế 3 Phase độc lập (KHÔNG phóng to kích thước):
///    - Phase 1 (100% -> 66.6%): Kim Cương Thể (Cam Vàng), áp sát 3.2 m/s.
///    - Phase 2 (66.6% -> 33.3%): Lôi Giáp Bạo Phát (Tím Lôi Điện), áp sát 3.8 m/s, tăng kháng ST.
///    - Phase 3 (33.3% -> 0%): Cuồng Nộ Diệt Thế (Đỏ Ma Lôi), áp sát 4.3 m/s, tăng kháng ST.
/// 4. Chia chỉ số Boss chuẩn xác theo từng Đại Cảnh Giới và Tiểu Tầng tu vi thăng tiến.
/// 5. Damage Cap (Chống One-Shot): Mỗi đòn của Player tối đa 4% Max HP của Boss (đảm bảo ít nhất 25-30 hit).
/// </summary>
public class BossEnemy : Enemy
{
    [Header("--- Boss Animation & Motion Settings ---")]
    [Tooltip("Tốc độ xoay hướng nhìn mượt mà về phía người chơi (độ/giây)")]
    public float turnSpeed = 400f;

    [Tooltip("Khoảng thời gian chu kỳ giữa các đòn đánh của Boss (chuẩn 1.5s theo animation)")]
    public float attackInterval = 1.5f;

    [Header("--- Tribulation Boss (Độ Kiếp) 3-Phase Settings ---")]
    public bool isTribulationBoss = false;
    public int currentPhase = 1; // 1, 2, 3
    [Tooltip("Tỉ lệ giảm sát thương nhận vào (0.15 = giảm 15% sát thương)")]
    public float damageReductionPercent = 0.15f;

    private Text phaseTextUI;
    private Vector3 baseScale = Vector3.one * 1.5f;

    // Animator Parameter Hashes (Tối ưu hiệu năng, tránh GC Alloc chuỗi string)
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private bool hasHashIsMoving = false;
    private bool hasHashAttack = false;
    private bool hasHashDie = false;

    // Quản lý trạng thái nội bộ
    private bool isMovingState = false;
    private bool isAttackingAnimation = false;
    private Coroutine attackRoutine = null;

    private void Awake()
    {
        CacheAnimatorParameters();
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        isAttackingAnimation = false;
        isMovingState = false;
        isDead = false;
    }

    /// <summary>
    /// Tính toán máu của Boss tầng thường theo stage (cứ 10 tầng gặp 1 lần).
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
            float stageScale = (stage - 5) / 5f;
            baseHp = 4000f * Mathf.Pow(2.2f, stageScale);
        }

        if (isTribulation) baseHp *= 1.35f;
        return baseHp;
    }

    /// <summary>
    /// Tính toán sát thương của Boss tầng thường.
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

    /// <summary>
    /// Khởi tạo Boss Tầng thường (mỗi 10 tầng 1 lần).
    /// </summary>
    public void InitBoss(int stage, Transform target, bool isTribulation = false)
    {
        float finalHealth = CalculateBossHealth(stage, isTribulation);

        if (this.hpBarHeight < 3.2f) this.hpBarHeight = 3.5f;
        this.attackRange = 2.4f;
        this.moveSpeed = 1.8f;
        this.isTribulationBoss = false;
        this.currentPhase = 1;
        this.damageReductionPercent = 0f;
        this.attackInterval = 1.5f;

        base.Init(finalHealth, target, true);

        Transform hpCanvas = transform.Find("HPCanvas");
        if (hpCanvas != null)
        {
            hpCanvas.localPosition = new Vector3(0, hpBarHeight, 0);
            Transform pTxt = hpCanvas.Find("PhaseText");
            if (pTxt != null) pTxt.gameObject.SetActive(false);
        }

        float finalDamage = CalculateBossDamage(stage, isTribulation);
        this.damage = finalDamage;
        this.baseScale = Vector3.one * 1.5f;
        transform.localScale = this.baseScale;

        isMovingState = false;
        isAttackingAnimation = false;
        attackTimer = 0.5f;

        CacheAnimatorParameters();
        SetMoving(false);
    }

    /// <summary>
    /// Khởi tạo Boss Thiên Kiếp (Độ Kiếp Thăng Cấp Đại Cảnh Giới):
    /// Chia chỉ số chuẩn xác theo từng cảnh giới tu vi thăng tiến, 3 Phase độc lập, KHÔNG phóng to kích thước.
    /// </summary>
    public void InitTribulationBoss(int realmTier, string targetRealmName, Transform target)
    {
        int stageIdx = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        InitTribulationBoss(realmTier, stageIdx, targetRealmName, target);
    }

    public void InitTribulationBoss(int realmTier, int stageIndex, string targetRealmName, Transform target)
    {
        this.isBoss = true;
        this.isTribulationBoss = true;
        this.playerTarget = target;
        this.attackRange = 2.4f;
        this.hpBarHeight = 3.6f;

        this.currentPhase = 1;

        // Tốc độ di chuyển nhanh mạnh mẽ (3.2m/s) để áp sát player chỉ trong ~2.5 - 3 giây
        this.moveSpeed = 3.2f;
        // Đợt tấn công hoạt động chuẩn theo Animation và lặp lại sau mỗi 1.5s
        this.attackInterval = 1.5f;

        // --- PHÂN CHIA CHỈ SỐ MÁU VÀ KHÁNG SÁT THƯƠNG THEO TỪNG CẢNH GIỚI TU VI ---
        float hp;
        float dmgReduction;

        switch (realmTier)
        {
            case 1: // Luyện Khí Tầng 15 -> Trúc Cơ Sơ Kỳ
                // Người chơi ở giai đoạn đầu (kiếm lv 2-3, dame ~80-150): Máu vừa tầm 1,800 HP để đánh 15-20 hit
                hp = 1800f;
                dmgReduction = 0.15f; // Kháng 15%
                break;

            case 2: // Trúc Cơ Hậu Kỳ -> Kết Đan Sơ Kỳ
                // Người chơi đã mở Vạn Kiếm Quy Tông (20 kiếm chém dồn), kiếm lv 4-6
                hp = 25000f;
                dmgReduction = 0.25f; // Kháng 25%
                break;

            case 3: // Kết Đan Hậu Kỳ -> Nguyên Anh Sơ Kỳ
                // Kiếm lv 7-10, buff Tu Lực nhân nhiều lần
                hp = 350000f;
                dmgReduction = 0.35f;
                break;

            case 4: // Nguyên Anh Hậu Kỳ -> Hóa Thần Sơ Kỳ
                hp = 4500000f;
                dmgReduction = 0.45f;
                break;

            case 5: // Hóa Thần Hậu Kỳ -> Đỉnh Phong
                hp = 50000000f;
                dmgReduction = 0.50f;
                break;

            default:
                // Tự động nội suy theo tầng tu vi cụ thể nếu dùng cheat test
                hp = 1800f * Mathf.Pow(2.2f, Mathf.Max(0, (stageIndex - 14) / 3f));
                dmgReduction = Mathf.Clamp(0.15f + (realmTier * 0.08f), 0.15f, 0.55f);
                break;
        }

        this.damageReductionPercent = dmgReduction;

        // Giữ nguyên kích thước cơ bản hài hòa, KHÔNG phóng to quá đà
        this.baseScale = Vector3.one * 1.5f;
        transform.localScale = this.baseScale;

        // Gọi Init lớp cha
        base.Init(hp, target, true);

        // Khởi tạo UI hiển thị Phase trên thanh máu
        SetupTribulationHPBar();

        isMovingState = false;
        isAttackingAnimation = false;
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        // Khi vừa tiếp cận, chờ 0.2s để chuyển từ thế chạy sang thế đánh
        attackTimer = 0.2f;

        CacheAnimatorParameters();
        SetMoving(false);

        Debug.Log($"⚡ [BossEnemy] Sinh Boss Thiên Kiếp (3 Phase)! Đột phá: {targetRealmName} (Tier {realmTier}), Tổng Máu: {hp:N0}, Kháng ST Phase 1: {dmgReduction * 100:F0}%");
    }

    private void SetupTribulationHPBar()
    {
        Transform hpCanvas = transform.Find("HPCanvas");
        if (hpCanvas != null)
        {
            hpCanvas.localPosition = new Vector3(0, hpBarHeight, 0);

            Transform phaseTr = hpCanvas.Find("PhaseText");
            if (phaseTr == null)
            {
                GameObject phaseObj = new GameObject("PhaseText", typeof(RectTransform), typeof(Text), typeof(Outline));
                phaseObj.transform.SetParent(hpCanvas, false);
                phaseTextUI = phaseObj.GetComponent<Text>();
                phaseTextUI.alignment = TextAnchor.MiddleCenter;
                phaseTextUI.fontSize = 14;
                phaseTextUI.fontStyle = FontStyle.Bold;
                phaseObj.GetComponent<Outline>().effectColor = Color.black;
                RectTransform pRt = phaseObj.GetComponent<RectTransform>();
                pRt.anchoredPosition = new Vector2(0, 0.28f);
                pRt.sizeDelta = new Vector2(3.5f, 0.4f);
            }
            else
            {
                phaseTextUI = phaseTr.GetComponent<Text>();
                phaseTextUI.gameObject.SetActive(true);
            }

            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (phaseTextUI != null && f != null) phaseTextUI.font = f;
        }

        UpdatePhaseVisuals(1);
    }

    private void UpdatePhaseVisuals(int phase)
    {
        if (phaseTextUI != null)
        {
            switch (phase)
            {
                case 1:
                    phaseTextUI.text = "⚡ [PHASE 1/3] Kim Cương Thể";
                    phaseTextUI.color = new Color(1f, 0.85f, 0.2f); // Cam vàng
                    break;
                case 2:
                    phaseTextUI.text = "⚡ [PHASE 2/3] Lôi Giáp Bạo Phát";
                    phaseTextUI.color = new Color(0.9f, 0.45f, 1f); // Tím lôi điện
                    break;
                case 3:
                    phaseTextUI.text = "🔥 [PHASE 3/3] Cuồng Nộ Diệt Thế";
                    phaseTextUI.color = new Color(1f, 0.25f, 0.25f); // Đỏ rực ma lôi
                    break;
            }
        }

        if (hpFill != null)
        {
            switch (phase)
            {
                case 1:
                    hpFill.color = new Color(1f, 0.75f, 0.1f); // Cam vàng
                    break;
                case 2:
                    hpFill.color = new Color(0.85f, 0.3f, 1f); // Tím lôi điện
                    break;
                case 3:
                    hpFill.color = new Color(1f, 0.15f, 0.15f); // Đỏ rực
                    break;
            }
        }
    }

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
    /// Vòng lặp di chuyển và chiến đấu:
    /// - Khi dist > attackRange: Lao tới sát Player.
    /// - Khi dist <= attackRange: Dừng chân và kích hoạt chu kỳ tấn công theo Animation sau mỗi 1.5s.
    /// - Chỉ gây sát thương khi hoạt ảnh chém THỰC SỰ vung chạm đích (giây 0.48).
    /// </summary>
    protected override void UpdateMovementAndCombat()
    {
        if (playerTarget == null || isDead) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(playerTarget.position.x, currentPos.y, playerTarget.position.z);

        // 1. Luôn xoay mượt mặt nhìn thẳng về phía Player
        Vector3 lookDir = (targetPos - currentPos).normalized;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            float currentTurnSpeed = isAttackingAnimation ? (turnSpeed * 0.4f) : turnSpeed;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, currentTurnSpeed * Time.deltaTime);
        }

        // 2. Nếu đang trong tiến trình hoạt ảnh tấn công: Đứng yên hoàn tất động tác chém
        if (isAttackingAnimation)
        {
            SetMoving(false);
            return;
        }

        // 3. Tính khoảng cách 2D mặt phẳng X-Z (bỏ qua trục Y độ cao)
        float dist = Vector2.Distance(new Vector2(currentPos.x, currentPos.z), new Vector2(targetPos.x, targetPos.z));

        // 4. Phân nhánh hành vi:
        if (dist > attackRange)
        {
            // Ở ngoài tầm đánh: Di chuyển tiến sát Player
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            SetMoving(true);
        }
        else
        {
            // Đã vào tầm đánh cận chiến: Dừng chân
            SetMoving(false);

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                // BẮT ĐẦU ĐỢT TẤN CÔNG THEO ANIMATION VÀ SAU MỖI 1.5S
                attackTimer = attackInterval;
                if (attackRoutine != null) StopCoroutine(attackRoutine);
                attackRoutine = StartCoroutine(PerformAttackCycleRoutine());
            }
        }
    }

    /// <summary>
    /// Chu kỳ tấn công của Boss chuẩn xác theo Animation và kéo dài đúng 1.5s:
    /// - Giây 0.00: Kích hoạt Trigger "Attack" trong Animator, Boss vung tay lên cao lấy đà.
    /// - Giây 0.48: Đòn đánh chém bổ xuống chạm vào Player -> Trừ 15% Max HP kèm rung màn hình + chớp đỏ!
    /// - Giây 0.48 -> 1.50: Thu đòn về thế thủ, hoàn tất trọn vẹn chu kỳ 1.5s trước khi ra đòn tiếp theo.
    /// </summary>
    private IEnumerator PerformAttackCycleRoutine()
    {
        isAttackingAnimation = true;
        SetMoving(false);

        // 1. Bắt đầu hoạt cảnh tấn công
        if (animator != null && hasHashAttack)
        {
            animator.ResetTrigger(HashAttack);
            animator.SetTrigger(HashAttack);
        }

        // 2. Chờ đúng thời điểm tay Boss chém chạm vào mục tiêu theo Animation (Wind-up Delay)
        yield return new WaitForSeconds(0.48f);

        // 3. GÂY SÁT THƯƠNG ĐÚNG THỜI ĐIỂM CHẠM ĐÍCH
        if (!isDead && playerTarget != null)
        {
            float finalDmg = damage;

            // QUY TẮC CỐT LÕI: Boss Thiên Kiếp gây đúng 15% Max HP của Player mỗi đòn đánh!
            // 7 nhát trúng đích = 105% HP -> Player CHẮC CHẮN TỬ TRẬN!
            if (isTribulationBoss && CombatManager.Instance != null)
            {
                finalDmg = CombatManager.Instance.playerMaxHp * 0.15f;
            }

            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.DamagePlayer(finalDmg, true);
            }

            // Rung chấn màn hình tạo lực chém chân thực
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.35f, 0.25f);
            }
        }

        // 4. Chờ hoàn tất phần còn lại của Animation và chu kỳ 1.5s (Recovery Cooldown)
        float remainingDuration = Mathf.Max(0.1f, 1.5f - 0.48f);
        yield return new WaitForSeconds(remainingDuration);

        isAttackingAnimation = false;
        attackRoutine = null;
    }

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
    /// Xử lý khi nhận sát thương:
    /// - Áp dụng Damage Cap chống bị bốc hơi one-shot giữa đường (tối đa 4% Max HP/hit).
    /// - Kiểm tra và kích hoạt chuyển 3 Phase mượt mà.
    /// </summary>
    public override void TakeDamage(float amount, bool isCrit = false)
    {
        if (isTribulationBoss)
        {
            // 1. Damage Cap: Mỗi đòn đánh của phi kiếm/kỹ năng chỉ được gây tối đa 4% Max HP của Boss
            // (Đảm bảo người chơi phải tấn công liên tục ít nhất 25 - 30 đòn mới hạ được Boss, không thể bị nuke chết giữa đường)
            float maxDamagePerHit = maxHp * 0.04f;
            amount = Mathf.Min(amount, maxDamagePerHit);

            // 2. Áp dụng Giảm Sát Thương theo Phase
            if (damageReductionPercent > 0f)
            {
                amount *= (1f - damageReductionPercent);
            }

            // 3. Kiểm tra ngưỡng Chuyển Phase
            float hpAfter = hp - amount;

            // Ngưỡng chuyển sang Phase 2 (66.6% Máu)
            if (currentPhase == 1 && hpAfter <= maxHp * 0.666f)
            {
                amount = Mathf.Max(0f, hp - (maxHp * 0.666f));
                TransitionToPhase(2);
            }
            // Ngưỡng chuyển sang Phase 3 (33.3% Máu)
            else if (currentPhase == 2 && hpAfter <= maxHp * 0.333f)
            {
                amount = Mathf.Max(0f, hp - (maxHp * 0.333f));
                TransitionToPhase(3);
            }
        }
        else
        {
            // Boss tầng thường
            if (damageReductionPercent > 0f)
            {
                amount *= (1f - damageReductionPercent);
            }
        }

        if (hp <= amount && !isDead)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
            isAttackingAnimation = false;
            SetMoving(false);
        }

        base.TakeDamage(amount, isCrit);
    }

    /// <summary>
    /// Chuyển đổi Phase mượt mà ngay trong trận chiến:
    /// Đổi màu thanh máu, tăng tốc chạy, tăng kháng sát thương, KHÔNG phóng to kích thước và KHÔNG đứng đơ.
    /// </summary>
    private void TransitionToPhase(int nextPhase)
    {
        currentPhase = nextPhase;

        // Âm thanh sét đánh bùng nổ chuyển phase
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoiPhatSFX();
        }

        if (nextPhase == 2)
        {
            // PHASE 2: Lôi Giáp Bạo Phát
            this.moveSpeed = 3.8f; // Tăng tốc độ chạy áp sát
            this.damageReductionPercent += 0.10f; // Tăng thêm 10% kháng sát thương

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.5f, 0.3f);
            }

            UpdatePhaseVisuals(2);

            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("⚡ BOSS THIÊN KIẾP BẠO PHÁT: Bước vào PHASE 2 (Lôi Giáp Bất Hoại, Tăng Tốc Độ)!", Color.yellow);
            }
        }
        else if (nextPhase == 3)
        {
            // PHASE 3: Cuồng Nộ Diệt Thế (KHÔNG phóng to kích thước)
            this.moveSpeed = 4.3f; // Lao cực nhanh vào áp sát
            this.damageReductionPercent += 0.10f; // Tăng thêm 10% kháng sát thương

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.7f, 0.4f);
            }

            UpdatePhaseVisuals(3);

            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("🔥 BOSS THIÊN KIẾP CUỒNG NỘ: Bước vào PHASE 3 (Cuồng Nộ Diệt Thế)!", Color.red);
            }
        }
    }
}

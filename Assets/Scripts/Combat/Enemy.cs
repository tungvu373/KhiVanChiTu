using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float maxHp = 35f;
    public float hp = 35f;
    public float moveSpeed = 2f;
    
    public bool isBoss = false;
    public float damage = 10f;
    public float attackSpeed = 1f; // Tấn công mỗi giây
    protected float attackTimer = 0f;

    protected Transform playerTarget;
    
    // UI & Game Feel
    protected Image hpFill;
    protected float visualHp;
    protected Renderer[] renderers; // Hỗ trợ Model 3D có nhiều bộ phận
    protected Color[] originalColors;
    protected float flashTimer;
    protected Camera mainCamera;
    
    // Animation
    public Animator animator;
    public bool isDead = false;
    public int SpawnVersion { get; private set; }
    protected bool hasIsMovingParam = false;
    
    // UI Damage Popup (Cộng dồn sát thương tránh đè chữ)
    protected DamagePopup activePopup;

    // Hit Recoil & Game Feel Spring
    protected Vector3 currentRecoilOffset = Vector3.zero;
    protected float scalePunchTimer = 0f;
    protected Vector3 originalLocalScale = Vector3.one;
    protected bool hasCapturedScale = false;

    [Header("Crowd Control (Khống Chế)")]
    public float stunTimer = 0f;
    public bool IsStunned => stunTimer > 0f;

    [Header("Settings")]
    public float hpBarHeight = 2.5f; // Chiều cao thanh máu (tùy chỉnh trên Inspector)

    public float attackRange = 1.8f; // Khoảng cách đứng đánh cận chiến sát người chơi

    private void OnDisable()
    {
        CancelInvoke();
        isDead = false;
        activePopup = null;
        stunTimer = 0f;
    }

    public virtual void Stun(float duration)
    {
        if (isDead || isBoss) return; // Boss miễn nhiễm hoàn toàn hiệu ứng Choáng!
        stunTimer = Mathf.Max(stunTimer, duration);
        if (animator != null && hasIsMovingParam)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    public virtual void Knockback(Vector3 direction, float distance)
    {
        if (isDead || isBoss) return; // Boss miễn nhiễm hoàn toàn hiệu ứng Đẩy lùi!
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            direction.Normalize();
        else
            direction = Vector3.forward;

        transform.position += direction * distance;
    }

    public void Init(float health, Transform target, bool boss = false)
    {
        SpawnVersion++;
        CancelInvoke();
        maxHp = health;
        hp = health;
        visualHp = health;
        playerTarget = target;
        isBoss = boss;
        damage = boss ? health * 0.2f : Mathf.Max(2f, health * 0.05f); // Sát thương tỉ lệ theo máu (tối thiểu 2)
        isDead = false;
        attackTimer = 0.5f;
        activePopup = null;
        stunTimer = 0f;
        currentRecoilOffset = Vector3.zero;
        scalePunchTimer = 0f;

        if (!hasCapturedScale)
        {
            originalLocalScale = transform.localScale;
            hasCapturedScale = true;
        }
        else
        {
            transform.localScale = originalLocalScale;
        }

        SetupHPBar();

        // Lấy tất cả SkinnedMeshRenderer/MeshRenderer trên Model 3D
        if (renderers == null || renderers.Length == 0)
        {
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
        }
        else
        {
            // Phục hồi lại màu ban đầu nếu từng bị Flash trắng
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && originalColors != null && i < originalColors.Length)
                {
                    if (renderers[i].material.HasProperty("_Color"))
                        renderers[i].material.color = originalColors[i];
                    else if (renderers[i].material.HasProperty("_BaseColor"))
                        renderers[i].material.SetColor("_BaseColor", originalColors[i]);
                }
            }
        }
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        hasIsMovingParam = false;
        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                if (p.name == "IsMoving") { hasIsMovingParam = true; break; }
            }

            animator.ResetTrigger("Die");
            animator.ResetTrigger("Attack");
            if (hasIsMovingParam) animator.SetBool("IsMoving", false);

            // Đưa Animator về trạng thái ban đầu để tránh bị đông cứng ở thế nằm chết
            animator.Rebind();
            animator.Update(0f);
        }
        
        mainCamera = Camera.main;
    }

    private void SetupHPBar()
    {
        Transform canvasTr = transform.Find("HPCanvas");
        if (canvasTr != null)
        {
            canvasTr.gameObject.SetActive(true);
            canvasTr.localPosition = new Vector3(0, hpBarHeight, 0);
            if (hpFill == null)
            {
                Transform fillTr = canvasTr.Find("HP_BG/HP_Fill");
                if (fillTr != null) hpFill = fillTr.GetComponent<Image>();
            }
            if (hpFill != null)
            {
                hpFill.color = new Color(1f, 0.05f, 0.05f, 1f); // Đỏ tươi rực rỡ, không bị hồng
                RectTransform fillRt = hpFill.GetComponent<RectTransform>();
                fillRt.anchorMax = Vector2.one;
            }
            return;
        }

        CreateHPBar();
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
        hpFill.color = new Color(1f, 0.05f, 0.05f, 1f); // Đỏ tươi rực rỡ, không bị hồng
        // Không dùng Filled vì thiếu Sprite gốc sẽ lỗi, dùng Anchor thay thế
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; 
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; 
        fillRt.offsetMax = Vector2.zero;
    }

    protected virtual void Update()
    {
        UpdateHpBar();
        UpdateFlashEffect();
        UpdateGameFeelRecoil();

        // Xử lý Choáng (Stun): Quái vật đứng im hoàn toàn, không di chuyển và không đánh
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (animator != null && hasIsMovingParam) animator.SetBool("IsMoving", false);
            return;
        }

        UpdateMovementAndCombat();
    }

    protected virtual void UpdateHpBar()
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
    }

    protected virtual void UpdateFlashEffect()
    {
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
    }

    protected virtual void UpdateGameFeelRecoil()
    {
        // 1. Phục hồi lò xo đẩy lùi đàn hồi (Elastic Spring-Back)
        if (currentRecoilOffset.sqrMagnitude > 0.0001f)
        {
            transform.position -= currentRecoilOffset;
            currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, Vector3.zero, Time.deltaTime * 16f);
            transform.position += currentRecoilOffset;
        }
        else if (currentRecoilOffset != Vector3.zero)
        {
            transform.position -= currentRecoilOffset;
            currentRecoilOffset = Vector3.zero;
        }

        // 2. Phục hồi Squash & Stretch (Co giãn mô hình khi trúng đòn)
        if (scalePunchTimer > 0f)
        {
            scalePunchTimer -= Time.deltaTime;
            float progress = Mathf.Clamp01(scalePunchTimer / 0.12f);
            Vector3 targetPunchScale = new Vector3(
                originalLocalScale.x * (1f + 0.15f * progress),
                originalLocalScale.y * (1f - 0.20f * progress),
                originalLocalScale.z * (1f + 0.15f * progress)
            );
            transform.localScale = targetPunchScale;
        }
        else if (transform.localScale != originalLocalScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalLocalScale, Time.deltaTime * 20f);
            if ((transform.localScale - originalLocalScale).sqrMagnitude < 0.0001f)
            {
                transform.localScale = originalLocalScale;
            }
        }
    }

    protected virtual void UpdateMovementAndCombat()
    {
        if (playerTarget == null || isDead) return;

        // BỎ QUA TRỤC Y khi tính khoảng cách (để tránh lỗi quái không bao giờ tới gần vì độ cao 3D)
        float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(playerTarget.position.x, playerTarget.position.z));
        Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
        
        // Xoay mượt về phía Player
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 500f * Time.deltaTime);
        }

        if (dist > attackRange)
        {
            // Cả quái thường lẫn Boss đều di chuyển tiến lại gần sát người chơi
            float formationSlow = isBoss && FormationService.Instance != null ? FormationService.Instance.BossSpeedMultiplier : 1f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * formationSlow * Time.deltaTime);
            
            if (animator != null && hasIsMovingParam) animator.SetBool("IsMoving", true);
        }
        else
        {
            // Đã vào tầm đánh cận chiến sát Player -> Dừng lại và tấn công
            if (animator != null && hasIsMovingParam) animator.SetBool("IsMoving", false);
            
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                attackTimer = isBoss ? 1.5f : (1f / attackSpeed);
                
                if (animator != null) animator.SetTrigger("Attack"); // Kích hoạt Anim Đánh
                
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.DamagePlayer(damage, isBoss);
                }
            }
        }
    }

    public virtual void TakeDamage(float amount, bool isCrit = false, Vector3 hitDirection = default)
    {
        hp -= amount;
        
        // Game Feel 1: Flash trắng cho toàn bộ Model 3D
        flashTimer = 0.1f;
        if (renderers != null)
        {
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
        }

        // Game Feel 2: Hiệu ứng va chạm (Sparks / Crit Burst)
        if (HitVFXManager.Instance != null)
        {
            Vector3 hitPoint = transform.position + Vector3.up * 1.0f;
            HitVFXManager.Instance.SpawnHitImpact(hitPoint, hitDirection, isCrit);
        }

        // Game Feel 3: Giật lùi đàn hồi & Squash-and-Stretch
        if (hitDirection == default || hitDirection.sqrMagnitude < 0.001f)
        {
            Vector3 pPos = CombatManager.Instance != null && CombatManager.Instance.playerTransform != null
                ? CombatManager.Instance.playerTransform.position
                : transform.position - Vector3.forward;
            hitDirection = (transform.position - pPos).normalized;
        }
        hitDirection.y = 0f;
        if (hitDirection.sqrMagnitude > 0.001f) hitDirection.Normalize();

        float recoilDist = isBoss ? 0.16f : (isCrit ? 0.65f : 0.38f);
        Vector3 impulse = hitDirection * recoilDist;
        transform.position += impulse;
        currentRecoilOffset += impulse;
        float maxRecoil = isBoss ? 0.30f : 0.85f;
        if (currentRecoilOffset.magnitude > maxRecoil)
        {
            currentRecoilOffset = currentRecoilOffset.normalized * maxRecoil;
        }

        scalePunchTimer = 0.12f;
        
        // Hiển thị sát thương (Gộp tổng dame nhận từ nhiều kiếm chém vào cùng mục tiêu)
        ShowDamagePopup(amount, isCrit);

        if (hp <= 0 && !isDead)
        {
            isDead = true;
            activePopup = null;
            if (animator != null) animator.SetTrigger("Die");
            
            // Ẩn Canvas máu ngay khi chết
            Transform canvasTr = transform.Find("HPCanvas");
            if (canvasTr != null) canvasTr.gameObject.SetActive(false);
            else if (hpFill != null && hpFill.transform.parent != null && hpFill.transform.parent.parent != null)
                hpFill.transform.parent.parent.gameObject.SetActive(false);
            
            // Đợi Animation chết chạy xong mới gọi Die()
            Invoke(nameof(Die), 1.5f); // Chờ 1.5s
        }
    }

    /// <summary>
    /// Hiển thị chữ số sát thương bay lên: Tự động gom/cộng dồn các đòn đánh liên tiếp vào 1 số duy nhất.
    /// </summary>
    protected virtual void ShowDamagePopup(float amount, bool isCrit)
    {
        if (CombatManager.Instance == null || CombatManager.Instance.damagePopupPrefab == null) return;

        // Nếu popup hiện tại vẫn đang tồn tại và còn trong cửa sổ nhận sát thương -> Cộng dồn vào popup đó!
        if (activePopup != null && activePopup.gameObject.activeInHierarchy && activePopup.CanAccumulate)
        {
            activePopup.AddDamage(amount, isCrit);
            return;
        }

        // Tạo popup mới khi chưa có hoặc popup trước đã bay đi
        Vector3 spawnPos = transform.position + Vector3.up * (hpBarHeight + 0.3f);
        // Thêm độ lệch nhẹ ngẫu nhiên để các đợt số kế tiếp không trùng khít tọa độ
        spawnPos.x += Random.Range(-0.25f, 0.25f);
        spawnPos.z += Random.Range(-0.15f, 0.15f);

        GameObject popup = null;
        if (SimplePool.Instance != null)
        {
            popup = SimplePool.Instance.SpawnFromPool("DamagePopup", spawnPos, Quaternion.identity);
        }
        else
        {
            popup = Instantiate(CombatManager.Instance.damagePopupPrefab, spawnPos, Quaternion.identity);
        }

        if (popup != null)
        {
            activePopup = popup.GetComponent<DamagePopup>();
            if (activePopup != null)
            {
                activePopup.Setup(amount, isCrit || amount >= 400f);
            }
        }
    }

    protected virtual void Die()
    {
        int stageIndex = 1;
        if (CultivationManager.Instance != null) stageIndex = CultivationManager.Instance.currentStageIndex;

        float baseDrop = 100f * Mathf.Pow(2.0f, stageIndex); // Rớt rất nhiều Linh Thạch
        if (isBoss) baseDrop *= 10f; // Boss rớt nhiều gấp 10 lần quái thường

        float tuLinhBonus = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetTuLinhValue() : 0;
        
        if (EconomyManager.Instance != null)
        {
            float artifactDrop = ArtifactService.Instance != null ? ArtifactService.Instance.StoneDropMultiplier : 1f;
            EconomyManager.Instance.AddLinhThach((baseDrop + tuLinhBonus) * artifactDrop);
        }
        
        // Cộng Tu Vi nếu người chơi chưa đạt 100% tu vi (Boss thưởng tu vi gấp 5 lần quái thường)
        if (CultivationManager.Instance != null && !CultivationManager.Instance.IsTuViMaxed())
        {
            float tuViBonus = 5f * Mathf.Pow(1.2f, stageIndex);
            if (isBoss) tuViBonus *= 5f;
            float formationGain = FormationService.Instance != null ? FormationService.Instance.CultivationMultiplier : 1f;
            CultivationManager.Instance.AddTuVi(tuViBonus * formationGain);
        }

        int combatStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
        AlchemyService.Instance?.OnEnemyDefeated(isBoss, combatStage);
        ArtifactService.Instance?.OnEnemyDefeated(isBoss, combatStage);
        AchievementManager.Instance?.OnEnemyKilled(isBoss);
        GameplayChoicesService.Instance?.OnEnemyDefeated(isBoss);
        
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

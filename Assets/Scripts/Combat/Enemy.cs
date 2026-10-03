using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp = 50f;
    public float moveSpeed = 2f;
    private Transform playerTarget;

    public void Init(float health, Transform target)
    {
        hp = health;
        playerTarget = target;
    }

    private void Update()
    {
        if (playerTarget != null)
        {
            // Tiến về phía người chơi (bỏ qua khác biệt độ cao y)
            Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            
            // Luôn quay mặt về phía người chơi (cần thiết cho Model 3D sau này)
            transform.LookAt(targetPos);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        
        // Hiện số máu bay lên (Cam = sát thương nhỏ, Đỏ = Sát thương Ulti)
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

        // Tính Linh Thạch rớt ra = Base (10) * 1.3^Stage + Bonus từ Tụ Linh
        float baseDrop = 10f * Mathf.Pow(1.3f, stageIndex);
        float tuLinhBonus = 0f;
        
        if (UpgradeManager.Instance != null)
        {
            tuLinhBonus = UpgradeManager.Instance.GetTuLinhValue();
        }
        
        float dropAmount = baseDrop + tuLinhBonus;
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddLinhThach(dropAmount);
        }
        
        // Thưởng Tu Vi khi giết quái (Giúp đột phá nhanh hơn)
        if (CultivationManager.Instance != null)
        {
            float tuViBonus = 5f * Mathf.Pow(1.2f, stageIndex); // Tăng dần theo cấp độ
            CultivationManager.Instance.AddTuVi(tuViBonus);
        }
        
        // Gửi sự kiện cho CombatManager và hủy quái
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnEnemyDied(this);
        }
        Destroy(gameObject);
    }
}

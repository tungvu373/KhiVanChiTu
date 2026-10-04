using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    public float baseDamage = 10f;
    public float baseSpeed = 5f;
    public int swordIndex = 0; // Biến phân bổ vị trí bay
    public bool isUltimateClone = false; // Đánh dấu đây là kiếm của Ultimate
    
    // Tách biệt rạch ròi Data của Kiếm và Data Nâng cấp (Upgrade)
    public static float CalculateBaseDamage(int level) { return level * level * 10f; }
    public static float CalculateBaseSpeed(int level) { return 5f + (level * 2f); }

    private Enemy targetEnemy;
    private float searchTimer = 0f;

    private void Update()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = null;
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                if (CombatManager.Instance != null)
                {
                    targetEnemy = CombatManager.Instance.GetNearestEnemy(transform.position);
                }
                searchTimer = 0.2f; 
            }
            
            Vector3 center = CombatManager.Instance != null ? CombatManager.Instance.playerTransform.position : Vector3.zero;
            
            // Xếp đội hình quỹ đạo: Kiếm thường bay vòng trong (2m, 4 góc vuông). Kiếm Ultimate bay vòng ngoài (4m, 20 góc).
            float currentRadius = isUltimateClone ? 4f : 2f;
            float angleOffset = isUltimateClone ? (360f / 20f) : 90f; // 20 kiếm cách nhau 18 độ
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

        // Sát thương và Tốc độ = Base (Từ Phi Kiếm) + Buff (Từ Upgrade) + Buff (Từ Shop)
        float thanThuc = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetThanThucValue() : 0;
        if (ShopManager.Instance != null) thanThuc *= ShopManager.Instance.GetSpiritMultiplier();
        
        float finalSpeed = baseSpeed + thanThuc;
        if (ShopManager.Instance != null) finalSpeed *= ShopManager.Instance.GetSpeedMultiplier();
        
        if (targetEnemy == null) return;
        transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, finalSpeed * Time.deltaTime);
        
        // Chĩa mũi kiếm (trục Z) về phía quái vật
        Vector3 dir = targetEnemy.transform.position - transform.position;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Kiểm tra va chạm (khoảng cách < 0.5)
        if (Vector3.Distance(transform.position, targetEnemy.transform.position) < 0.5f)
        {
            AttackTarget();
        }
    }

    private void AttackTarget()
    {
        float kiemY = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetKiemYValue() : 0;
        float bonusMultiplier = CultivationManager.Instance != null ? CultivationManager.Instance.permanentStatMultiplier : 1f;
        if (ShopManager.Instance != null) bonusMultiplier *= ShopManager.Instance.GetDamageMultiplier();
        
        // baseDamage đã được set trực tiếp bởi CombatManager (dựa vào Level kiếm)
        float finalDamage = (baseDamage + kiemY) * bonusMultiplier;
        
        targetEnemy.TakeDamage(finalDamage);
        
        // Hủy mục tiêu để frame tiếp theo tìm con mới
        targetEnemy = null; 
    }
}

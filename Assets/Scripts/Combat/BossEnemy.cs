using UnityEngine;

public class BossEnemy : Enemy
{
    // Viết riêng script Boss để dễ dàng mở rộng kỹ năng đặc biệt sau này
    public void InitBoss(int stage, Transform target, bool isTribulation = false)
    {
        // 1. Tính toán Máu (HP): Player DPS ở tầng 5 ~ 500. Để Boss chịu được ~25s -> HP cần ~ 12,500.
        // Tầng 10 Player DPS ~ 1500 -> HP cần ~ 37,500.
        float healthMultiplier = isTribulation ? 3f : 2.5f;
        float finalHealth = 5000f * Mathf.Pow(healthMultiplier, stage / 5f);
        
        // Chiều cao thanh máu cao hơn đầu Boss (Model Boss to 1.5x, nếu chưa chỉnh hoặc <= 2.5f thì tự nâng lên 3.5f)
        if (this.hpBarHeight < 3.2f) this.hpBarHeight = 3.5f;
        
        // Tầm đánh cận chiến của Boss (Boss to hơn nên áp sát cự ly 2.2m để vung vuốt)
        this.attackRange = 2.2f;
        if (this.moveSpeed < 1f) this.moveSpeed = 2.0f;
        
        // Gọi hàm Init của lớp cha (Enemy)
        base.Init(finalHealth, target, true);

        // Đảm bảo vị trí Canvas thanh máu nằm cao hơn đầu Boss
        Transform hpCanvas = transform.Find("HPCanvas");
        if (hpCanvas != null)
        {
            hpCanvas.localPosition = new Vector3(0, hpBarHeight, 0);
        }

        // 2. Tính toán Sát thương (Damage): Không tính theo % máu nữa vì máu Boss quá cao sẽ gây One-shot Player.
        // Tầng 5: Sát thương ~ 50. Tầng 10: ~ 100. (Vừa phải để Player có cơ hội hồi máu/sống sót)
        float finalDamage = 50f + ((stage - 5) / 5f) * 50f;
        if (finalDamage < 20f) finalDamage = 20f; // Sát thương tối thiểu
        if (isTribulation) finalDamage *= 1.5f;
        
        this.damage = finalDamage; // Ghi đè lại thuộc tính damage của Enemy cơ bản

        // Kích thước to hơn để thể hiện sự đáng sợ
        transform.localScale = Vector3.one * 1.5f;

        Debug.Log($"[BossEnemy] Sinh ra Boss! Máu: {finalHealth}, Lôi Kiếp: {isTribulation}");
    }
}

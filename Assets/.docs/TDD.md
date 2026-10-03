# TECHNICAL DESIGN DOCUMENT (TDD) - KHÍ VẬN CHI TỬ

## 1. Architecture Overview (Unity/C#)
Dự án sử dụng kiến trúc **Manager Pattern** kết hợp rộng rãi với **ScriptableObjects** để dễ dàng tinh chỉnh (balance) chỉ số kinh tế và sức mạnh mà không cần can thiệp vào code.
- `GameManager`: Quản lý State chung của game (IdleFarm, BossTribulation, Breakthrough, Defeat).
- `EconomyManager`: Quản lý Linh Thạch và Điểm Cơ Duyên, cung cấp hàm tính toán mức giá nâng cấp theo công thức lũy tiến.
- `CultivationManager`: Quản lý thanh Tu Vi (EXP) và 27 cấp bậc Cảnh Giới. Xử lý Logic cộng điểm Kỹ năng khi đột phá thành công.
- `MergeManager`: Quản lý Grid Lò Luyện, sinh Kiếm Phôi (dựa trên RNG rate) và check logic ghép kiếm phôi cùng cấp.
- `SkillManager`: Xử lý hồi chiêu (Cooldown Timer) và check ngưỡng Linh Lực (Mana) để tự động kích hoạt kỹ năng (Vạn Kiếm Quy Tông, Xoay Kiếm).
- `CombatManager`: Spawn yêu thú, Lôi Kiếp Boss và tính toán sát thương đầu ra của Phi Kiếm/Nhân vật.

## 2. Core Data Structures (ScriptableObjects)

### 2.1 CultivationStageData (Dữ liệu Cảnh Giới)
Dùng để config 27 mốc từ Luyện Khí Tầng 1 đến Hóa Thần Hậu Kỳ.
```csharp
[CreateAssetMenu(fileName = "New Stage", menuName = "TuTien/Cultivation Stage")]
public class CultivationStageData : ScriptableObject
{
    public string stageName; // Luyện Khí Tầng 1, Trúc Cơ Sơ Kỳ...
    public float requiredTuVi; // Lượng Tu Vi cần để đột phá cảnh giới này
    public bool hasTribulationBoss; // True nếu phải đánh Lôi Kiếp Boss để qua mốc lớn
    public float statMultiplierBonus; // Bonus chỉ số vĩnh viễn (Dame/Linh Lực) sau khi đột phá
}
```

### 2.2 UpgradeData (Dữ liệu 4 Nút Nâng cấp)
```csharp
[CreateAssetMenu(fileName = "New Upgrade", menuName = "TuTien/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeId; // LinhLuc, KiemY, ThanThuc, TuLinh
    public float baseCost; // Giá Linh Thạch gốc
    public float costMultiplier; // Hệ số nhân giá
    public float baseValue; // Giá trị gốc
    public float valueIncrementPerLevel; // Giá trị cộng thêm mỗi cấp

    // Hàm tính giá tiền yêu cầu cho cấp hiện tại
    public float GetCost(int currentLevel) 
    {
        return baseCost * Mathf.Pow(costMultiplier, currentLevel);
    }

    // Hàm lấy giá trị sức mạnh ở cấp hiện tại
    public float GetValue(int currentLevel)
    {
        return baseValue + (valueIncrementPerLevel * currentLevel);
    }
}
```

## 3. Pseudo-code & Core Logic

### 3.1 Skill Execution Logic (Mana + Cooldown)
Đoạn code chạy ngầm kiểm tra điều kiện xuất chiêu (Vạn Kiếm Quy Tông). Phụ thuộc vào tốc độ hồi Linh Lực (Upgrade 1) và base cooldown của Skill.
```csharp
public class SkillManager : MonoBehaviour
{
    public float currentLinhLuc;
    public float maxLinhLuc; // Phụ thuộc Upgrade Linh Lực
    public float linhLucRegenRate; // Phụ thuộc Upgrade Linh Lực

    public SkillData activeSkill;
    private float currentCooldown;

    void Update()
    {
        // 1. Tự động hồi Linh Lực
        if (currentLinhLuc < maxLinhLuc) {
            currentLinhLuc += linhLucRegenRate * Time.deltaTime;
        }

        // 2. Đếm ngược Cooldown
        if (currentCooldown > 0) {
            currentCooldown -= Time.deltaTime;
        }

        // 3. Tự động xả Skill nếu đủ Linh Lực và hết Cooldown
        if (currentCooldown <= 0 && currentLinhLuc >= activeSkill.manaCost) {
            ExecuteSkill();
        }
    }

    void ExecuteSkill()
    {
        // Trừ Linh Lực và reset Cooldown
        currentLinhLuc -= activeSkill.manaCost;
        currentCooldown = activeSkill.baseCooldown;
        
        // Logic kích hoạt: Spawn VFX Vạn Kiếm Quy Tông
        // Gọi CombatManager.DealAOEDamage() trừ máu toàn bộ quái trên sân
        Debug.Log("VẠN KIẾM QUY TÔNG!!");
    }
}
```

### 3.2 Flying Sword Auto-Attack (Từ Trúc Cơ Kỳ)
Script gắn trên mỗi Prefab Phi Kiếm được trang bị từ Lò Luyện.
```csharp
public class FlyingSword : MonoBehaviour
{
    public float baseDamage;
    public float currentSpeed; // Lấy từ Upgrade Thần Thức
    private Transform targetEnemy;

    void Update()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy) {
            targetEnemy = FindNearestEnemy();
            return;
        }

        // Bay lượn về phía mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, currentSpeed * Time.deltaTime);
        transform.up = targetEnemy.position - transform.position; // Hướng mũi kiếm về địch

        // Logic va chạm / chém quái
        if (Vector3.Distance(transform.position, targetEnemy.position) < 0.5f) {
            // Gây sát thương tổng = Dame Kiếm + Bonus từ Cảnh Giới + Bonus Upgrade Kiếm Ý
            float finalDamage = CalculateFinalDamage(baseDamage);
            targetEnemy.GetComponent<Enemy>().TakeDamage(finalDamage);
            
            // Tìm mục tiêu mới để tiếp tục bay
            targetEnemy = null; 
        }
    }

    private Transform FindNearestEnemy()
    {
        // Sử dụng logic quét quái gần nhất hoặc overlap sphere
        return null;
    }
}
```

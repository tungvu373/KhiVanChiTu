# TECHNICAL DESIGN DOCUMENT (TDD) - KHÍ VẬN CHI TỬ (COMMERCIAL EDITION)
## ĐẶC TẢ KIẾN TRÚC KỸ THUẬT UNITY / C#

---

## 1. Kiến Trúc Tổng Thể (Modular Architecture)
Dự án áp dụng mô hình **Event-Driven Architecture** kết hợp sâu rộng với **ScriptableObjects** để phân tách hoàn toàn giữa Dữ Liệu (Data), Xử Lý Logic (Managers) và Hiển Thị (UI View), đảm bảo không gây thắt nút cổ chai (Bottleneck) khi mở rộng quy mô thương mại.

```
                  ┌─────────────────────────────────────────┐
                  │          GameManager (Core State)       │
                  └────────────────────┬────────────────────┘
                                       │
         ┌───────────────┬─────────────┼───────────────┬────────────────┐
         ▼               ▼             ▼               ▼                ▼
┌────────────────┐┌──────────────┐┌──────────────┐┌──────────────┐┌────────────────┐
│CultivationMgr  ││BodyCultivMgr ││  MergeMgr    ││  AlchemyMgr  ││  EquipmentMgr  │
│(Khí Tu 10 Lv)  ││(Luyện Thể)   ││(Gated Slots) ││(May Rủi Đan) ││(4 Món Hộ Thân) │
└────────┬───────┘└──────┬───────┘└──────┬───────┘└──────┬───────┘└────────┬───────┘
         │               │               │               │                 │
         └───────────────┴───────────────┼───────────────┴─────────────────┘
                                         ▼
                 ┌────────────────────────────────────────────────┐
                 │          Dynamic Drawer UI Controller          │
                 │   (Collapsible Bottom Sheet 1-Chạm Mở/Đóng)    │
                 └────────────────────────────────────────────────┘
```

---

## 2. Danh Sách Các Manager Cốt Lõi

### 2.1 `CultivationManager.cs` (Quản Lý Khí Tu & Đột Phá Lôi Kiếp)
* **Trách nhiệm:** Quản lý tiến trình tích lũy Tu vi, 10 sub-level nội bộ, cơ chế khóa bình cảnh tại Level 10 và kiểm tra điều kiện Đan Dược Đột Phá.
* **Biến số cốt lõi:**
  * `int currentMajorRealmIndex`: Chỉ số đại cảnh giới (0 = Luyện Khí, 1 = Trúc Cơ...).
  * `int currentSubLevel`: Từ 1 đến 10.
  * `float currentTuVi`, `float maxTuViForLevel`.
* **Logic then chốt:**
  * `AddTuVi(float amount)`: Tích lũy tu vi. Nếu ở Level 1-9 và đạt 100%, tự động tăng `currentSubLevel++`. Nếu ở Level 10, kẹt tại 100% (bình cảnh).
  * `AttemptBreakthrough(string pillId)`: Kiểm tra đan dược. Tính toán tỷ lệ thành công.
    * *Thành công:* `currentMajorRealmIndex++`, `currentSubLevel = 1`, gọi sự kiện `OnMajorRealmUpgraded` (để `MergeManager` mở thêm ô kiếm).
    * *Thất bại:* Tiêu hao đan, `currentTuVi = maxTuViForLevel * 0.7f` (rớt 30% Tu vi Level 10), kích hoạt hiệu ứng sấm sét thất bại.
  * `WatchAdToPreventTuViLoss()`: Hook xem Ad để khôi phục 100% tu vi và buff +10% rate lần sau.

### 2.2 `BodyCultivationManager.cs` (Thay Thế SkillManager Cũ - Hệ Thống Luyện Thể)
* **Trách nhiệm:** Quản lý Cây Chỉ Số Thể Phách (Thể Tu), tiêu hao Linh Thạch để củng cố sức mạnh sinh tồn và nội lực cho nhân vật.
* **Cấu trúc Dữ liệu Chỉ số:**
  ```csharp
  [System.Serializable]
  public class BodyStatNode
  {
      public string statId;       // MaxHP, DamageReduction, HPRegen, PhysicalDamage, Lifesteal, Tenacity
      public int currentLevel;
      public float baseCost;
      public float costMultiplier;
      public float valuePerLevel;
      
      public float GetCost() => baseCost * Mathf.Pow(costMultiplier, currentLevel);
      public float GetValue() => valuePerLevel * currentLevel;
  }
  ```
* **Logic then chốt:**
  * `UpgradeStat(string statId)`: Kiểm tra đủ Linh Thạch từ `EconomyManager` $\rightarrow$ Trừ tiền $\rightarrow$ Tăng cấp chỉ số $\rightarrow$ Cập nhật thông số nhân vật ngay lập tức.
  * `CheckTierMilestone()`: Khi tất cả các node trong 1 tầng đạt cấp quy định, kích hoạt **Nội Tại Bá Thể** (Thiềm Thừ Bì, Huyết Nộ, Phản Chấn, Tẩy Tủy...).

### 2.3 `MergeManager.cs` (Cải Tiến Gated Slots & Khóa Quick Merge)
* **Trách nhiệm:** Quản lý lưới phôi kiếm trong Drawer Kiếm Lô và số lượng ô xuất chiến của Phi Kiếm.
* **Nâng cấp thương mại:**
  * `UpdateUnlockedSlots(int majorRealmIndex)`:
    * Luyện Khí (Index 0) = 1 Ô.
    * Trúc Cơ (Index 1) = 2 Ô...
    * Độ Kiếp (Index 8) = 10 Ô.
    * Các ô vượt quá giới hạn cảnh giới sẽ hiển thị ổ khóa và yêu cầu thăng cấp cảnh giới để mở.
  * `QuickMerge()`:
    * Gắn cờ kiểm tra: `if (!HasQuickMergePrivilege()) ShowMonetizationPopup();`
    * Quyền lợi mở khóa khi: Mua thẻ tháng / Gói Thần Kiếm Lệnh HOẶC xem 1 Rewarded Ad (nhận 15 phút đếm ngược).

### 2.4 `AlchemyManager.cs` (Hệ Thống Luyện Đan May Rủi)
* **Trách nhiệm:** Quản lý kho dược liệu, công thức chế tạo đan dược đột phá và tính toán tỷ lệ ra đan Sơ/Trung/Cực phẩm hoặc Phế đan.
* **Logic tính xác suất:**
  * Tỷ lệ cơ bản: Phế đan (25% - cấp đan sư), Sơ phẩm (50%), Trung phẩm (20%), Cực phẩm (5% luôn mở ngay từ cấp 1).
  * Sản phẩm trả về đối tượng `PillItem` mang hệ số nhân buff tỷ lệ đột phá cho `CultivationManager`.

### 2.5 `EquipmentManager.cs` & `ArrayManager.cs`
* **EquipmentManager:** Quản lý 4 slot Mũ, Áo, Giày, Ngọc Bội. Nâng cấp bằng quặng rơi từ Boss, cập nhật Aura hào quang ở mốc +3, +6, +9.
* **ArrayManager:** Quản lý vòng lặp Cooldown của Pháp Trận AoE (Cửu Thiên Lôi Trận giáng sét toàn màn hình mỗi 10 giây).

---

## 3. Cấu Trúc Dữ Liệu ScriptableObjects

### 3.1 `CultivationStageData.cs`
```csharp
[CreateAssetMenu(fileName = "New Realm", menuName = "TuTien/Major Realm Data")]
public class CultivationStageData : ScriptableObject
{
    public string majorRealmName;       // "Trúc Cơ", "Kim Đan"...
    public int majorRealmIndex;         // 0 -> 8
    public float baseTuViRequired;      // Tu vi gốc của Level 1
    public float tuViMultiplierPerSub;  // Hệ số tăng qua từng sub-level (1 -> 10)
    public float baseBreakthroughRate;  // Tỷ lệ cơ bản ở Level 10 (0.0 -> 1.0)
    public string requiredPillId;       // ID đan dược cần để cắn đột phá
    public int unlockedSwordSlots;      // Số ô kiếm mở khóa tại cảnh giới này
}
```

### 3.2 `BodyTalentTreeData.cs`
```csharp
[CreateAssetMenu(fileName = "New Body Tree", menuName = "TuTien/Body Talent Tree")]
public class BodyTalentTreeData : ScriptableObject
{
    public int tierIndex;               // Tầng 1 -> 6 (Luyện Bì, Rèn Nhục...)
    public string tierName;
    public BodyStatNode[] statNodes;    // Danh sách 6 chỉ số
    public string milestonePassiveDesc; // Mô tả nội tại bá thể khi thông quan tầng
}
```

---

## 4. Đặc Tả Giao Diện Mobile Drawer (`MobileDrawerUIController.cs`)
* Quản lý trạng thái đóng/mở của 5 Panel Drawer:
  * Trạng thái mặc định: Cả 5 Panel ẩn dưới đáy màn hình (`anchoredPosition.y = -PanelHeight`), Đấu trường giãn nở full 85%.
  * Khi người chơi chạm tab `[i]`:
    * Nếu Panel `[i]` đang mở $\rightarrow$ Trượt đóng lại (Toggle close).
    * Nếu đang đóng $\rightarrow$ Trượt lên vị trí `y = 0` bằng Tween mượt mà (0.25s EaseOutQuad).
  * Lắng nghe sự kiện chạm ra ngoài (Backdrop raycast) và cử chỉ vuốt kéo xuống (Drag Event) để tự động đóng Drawer.

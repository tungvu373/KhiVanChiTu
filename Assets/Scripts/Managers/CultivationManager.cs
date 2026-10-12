using UnityEngine;
using System;

public class CultivationManager : MonoBehaviour
{
    public static CultivationManager Instance { get; private set; }

    public float currentTuVi;
    public int currentStageIndex;
    
    [Header("Dữ liệu Cảnh Giới")]
    public CultivationStageData[] allStages;

    [Header("Hiệu ứng (Game Feel)")]
    public GameObject lightningVFXPrefab;

    [Header("Giao diện (UI) mới")]
    public UnityEngine.UI.Text stageText;
    public UnityEngine.UI.Text characterInfoText;

    [Header("Chỉ số cộng thêm (Buff vĩnh viễn)")]
    public float permanentStatMultiplier = 1f;

    // Tên nhân vật (lấy từ DataManager khi vào game)
    private string characterName = "Đạo Hữu";
    public string CharacterName => characterName;

    public event Action<float, float> OnTuViChanged; // current, max
    public event Action<CultivationStageData> OnStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Cập nhật UI ngay lúc bắt đầu
        if (allStages != null && currentStageIndex < allStages.Length)
        {
            OnStageChanged?.Invoke(allStages[currentStageIndex]);
            OnTuViChanged?.Invoke(currentTuVi, allStages[currentStageIndex].requiredTuVi);
        }
    }

    public void AddTuVi(float amount)
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return;

        CultivationStageData currentStageData = allStages[currentStageIndex];
        
        // Kẹt ở Lôi Kiếp Boss thì không tăng tu vi nữa
        if (currentTuVi >= currentStageData.requiredTuVi) return;

        currentTuVi += amount;
        if (currentTuVi >= currentStageData.requiredTuVi)
        {
            currentTuVi = currentStageData.requiredTuVi;
        }

        UpdateDashboardUI(currentStageData);
        OnTuViChanged?.Invoke(currentTuVi, currentStageData.requiredTuVi);
        CheckBreakthrough();
    }

    /// <summary>
    /// Lấy dữ liệu cảnh giới hiện tại an toàn.
    /// </summary>
    public CultivationStageData GetCurrentStageData()
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return null;
        return allStages[currentStageIndex];
    }

    /// <summary>
    /// Kiểm tra xem Cảnh Giới hiện tại có phải là mốc Đột Phá (cần diệt Boss) hay không.
    /// QUY TẮC CHUẨN:
    /// - Riêng 15 tầng Luyện Khí Kỳ: CHỈ CÓ DUY NHẤT TẦNG 15 mới là Đột Phá (cần diệt Boss lên Trúc Cơ).
    ///   Các tầng 1 -> 14 chỉ là tích lũy tu vi tiểu cảnh giới, đủ 100% Tu Vi là TỰ ĐỘNG THĂNG TẦNG!
    /// - Các đại cảnh giới khác (Trúc Cơ, Kết Đan...): Căn cứ theo hasTribulationBoss của Cảnh Giới đó.
    /// </summary>
    public bool RequiresBossToBreakthrough(CultivationStageData stageData)
    {
        if (stageData == null) return false;

        // Riêng 15 tầng Luyện Khí Kỳ: CHỈ CÓ DUY NHẤT TẦNG 15 MỚI LÀ ĐỘT PHÁ!
        if (stageData.stageName.Contains("Luyện Khí"))
        {
            return stageData.stageName.Contains("Tầng 15");
        }

        // Các cảnh giới khác: Cần đánh boss nếu có hasTribulationBoss
        return stageData.hasTribulationBoss;
    }

    /// <summary>
    /// Kiểm tra người chơi đã đạt cực hạn 100% tu vi (đạt bình cảnh) để sẵn sàng đột phá hay chưa.
    /// </summary>
    public bool IsTuViMaxed()
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return false;
        return currentTuVi >= allStages[currentStageIndex].requiredTuVi;
    }

    private void UpdateDashboardUI(CultivationStageData currentStageData)
    {
        if (stageText != null)
        {
            bool needBoss = RequiresBossToBreakthrough(currentStageData);
            if (currentTuVi >= currentStageData.requiredTuVi && needBoss)
            {
                stageText.text = $"Cảnh Giới: {currentStageData.stageName} - 100% (Cần diệt Boss để Đột phá)";
            }
            else
            {
                float progress = (currentTuVi / currentStageData.requiredTuVi) * 100f;
                stageText.text = $"Cảnh Giới: {currentStageData.stageName} - Tiến độ: {progress:F1}%";
            }
        }

        if (characterInfoText != null)
        {
            characterInfoText.text = $"Tên: {characterName}\nCảnh giới: {currentStageData.stageName}\nTu Vi: {Mathf.FloorToInt(currentTuVi)}/{currentStageData.requiredTuVi}";
        }
    }

    /// <summary>Thiết lập tên nhân vật và cập nhật UI ngay lập tức.</summary>
    public void SetCharacterName(string name)
    {
        characterName = string.IsNullOrEmpty(name) ? "Đạo Hữu" : name;
        // Refresh UI ngay
        if (allStages != null && currentStageIndex < allStages.Length)
            UpdateDashboardUI(allStages[currentStageIndex]);
    }

    private void CheckBreakthrough()
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return;
        CultivationStageData currentStageData = allStages[currentStageIndex];

        if (currentTuVi >= currentStageData.requiredTuVi)
        {
            bool needBoss = RequiresBossToBreakthrough(currentStageData);

            if (needBoss)
            {
                // MỐC ĐỘT PHÁ (DUY NHẤT Luyện Khí Tầng 15 hoặc Đại Cảnh Giới): Cần Boss để Đột Phá!
                if (currentStageData.hasTribulationBoss)
                {
                    Debug.Log("⚠️ LÔI KIẾP HÀNG LÂM! Chuẩn bị đánh Boss Lôi Kiếp để đột phá!");
                    if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(1.0f, 0.6f);
                    if (lightningVFXPrefab != null && CombatManager.Instance != null && CombatManager.Instance.playerTransform != null)
                    {
                        Instantiate(lightningVFXPrefab, CombatManager.Instance.playerTransform.position + Vector3.up * 8f, Quaternion.Euler(90, 0, 0));
                    }
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayLoiPhatSFX();
                    }
                    GameManager.Instance.ChangeState(GameManager.GameState.BossTribulation);
                }
            }
            else
            {
                // TIỂU CẢNH GIỚI (Luyện Khí Tầng 1 -> 14, Trúc Cơ Sơ -> Trung...):
                // Đủ 100% Tu Vi là TỰ ĐỘNG THĂNG TẦNG / LÊN CẤP NGAY!
                Breakthrough();
            }
        }
    }

    private bool isBreakthroughInProgress = false;

    public void Breakthrough()
    {
        if (currentStageIndex >= allStages.Length) return;
        if (isBreakthroughInProgress) return; // Chống double-call
        isBreakthroughInProgress = true;

        CultivationStageData currentStageData = allStages[currentStageIndex];
        
        // Rung màn hình và Sấm sét chúc mừng (Chỉ khi vượt Đại Cảnh Giới)
        bool isMajorBreakthrough = currentStageData.stageName.Contains("Tầng 15") || currentStageData.stageName.Contains("Hậu Kỳ");
        if (isMajorBreakthrough)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.8f, 0.5f);
            if (lightningVFXPrefab != null && CombatManager.Instance != null && CombatManager.Instance.playerTransform != null)
            {
                Instantiate(lightningVFXPrefab, CombatManager.Instance.playerTransform.position + Vector3.up * 8f, Quaternion.Euler(90, 0, 0));
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLoiPhatSFX();
            }
        }

        // Reset tu vi về 0 để lên cấp mới
        currentTuVi = 0; 

        // Phát âm thanh nổ vang chúc mừng đột phá tu vi (bang.mp3)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBreakthroughSFX();
        }
        
        // Thưởng điểm Cơ Duyên (Quy tắc chống lạm phát)
        int coDuyenBonus = 0;
        if (currentStageData.stageName.Contains("Luyện Khí"))
        {
            // Chỉ tầng 5, 10, 15 mới được 1 điểm
            if (currentStageData.stageName.Contains("Tầng 5") || 
                currentStageData.stageName.Contains("Tầng 10") || 
                currentStageData.stageName.Contains("Tầng 15"))
            {
                coDuyenBonus = 1;
            }
        }
        else
        {
            // Các cảnh giới khác: Hậu Kỳ được 2, Sơ/Trung được 1
            if (currentStageData.stageName.Contains("Hậu Kỳ")) coDuyenBonus = 2;
            else coDuyenBonus = 1;
        }

        if (coDuyenBonus > 0 && EconomyManager.Instance != null) 
        {
            EconomyManager.Instance.AddCoDuyen(coDuyenBonus);
        }

        // Tăng hệ số sức mạnh vĩnh viễn
        permanentStatMultiplier *= currentStageData.statMultiplierBonus;

        currentStageIndex++;
        Debug.Log($"✨ ĐỘT PHÁ THÀNH CÔNG! Thưởng: {coDuyenBonus} Cơ Duyên. Hệ số SM: {permanentStatMultiplier:F2}");

        if (GameLogger.Instance != null && currentStageIndex < allStages.Length)
        {
            GameLogger.Instance.Log($"Đột phá: {allStages[currentStageIndex].stageName}!", Color.cyan);
        }

        if (currentStageIndex < allStages.Length)
        {
            CombatNotificationUI.ShowBreakthrough(allStages[currentStageIndex].stageName);
            OnStageChanged?.Invoke(allStages[currentStageIndex]);
            OnTuViChanged?.Invoke(currentTuVi, allStages[currentStageIndex].requiredTuVi);
            UpdateDashboardUI(allStages[currentStageIndex]);
        }
        else
        {
            Debug.Log("🏆 BẠN ĐÃ ĐẠT ĐỈNH PHONG CỦA THẾ GIỚI TU TIÊN (Hóa Thần Hậu Kỳ)!");
        }

        GameManager.Instance.ChangeState(GameManager.GameState.Breakthrough);
        Invoke(nameof(ReturnToIdle), 2f);
    }

    private void ReturnToIdle()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Breakthrough)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }
        isBreakthroughInProgress = false; // Mở lại sau 2s
        if (MapScroller.Instance != null) MapScroller.Instance.ResumeMoving();
    }

    public void HandlePlayerDeath()
    {
        // Khi tử trận: Giữ nguyên Cảnh giới và BẢO LƯU toàn bộ Tu vi hiện tại (không bị trừ tu vi nữa)
        // currentTuVi được giữ nguyên vẹn

        if (allStages != null && currentStageIndex < allStages.Length)
        {
            CultivationStageData currentStageData = allStages[currentStageIndex];
            UpdateDashboardUI(currentStageData);
            OnTuViChanged?.Invoke(currentTuVi, currentStageData.requiredTuVi);
            OnStageChanged?.Invoke(currentStageData);
        }

        Debug.Log($"💀 [CultivationManager] TỬ TRẬN! Bảo toàn toàn bộ Tu Vi ({currentTuVi:F0}) và Cảnh Giới.");
        if (GameLogger.Instance != null) GameLogger.Instance.Log("TỬ TRẬN! Bảo toàn Tu Vi và Cảnh Giới", Color.yellow);
    }

    // ── SAVE / LOAD ─────────────────────────────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.stageIndex = currentStageIndex;
        slot.stageName = (allStages != null && currentStageIndex < allStages.Length)
            ? allStages[currentStageIndex].stageName : "Luyện Khí Kỳ Tầng 1";
        slot.currentTuVi = currentTuVi;
        slot.permanentStatMultiplier = permanentStatMultiplier;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        currentStageIndex = Mathf.Clamp(slot.stageIndex, 0, allStages != null ? allStages.Length - 1 : 0);
        currentTuVi = slot.currentTuVi;
        permanentStatMultiplier = Mathf.Max(1f, slot.permanentStatMultiplier);

        if (allStages != null && currentStageIndex < allStages.Length)
        {
            OnStageChanged?.Invoke(allStages[currentStageIndex]);
            OnTuViChanged?.Invoke(currentTuVi, allStages[currentStageIndex].requiredTuVi);
            UpdateDashboardUI(allStages[currentStageIndex]);
        }
    }
}


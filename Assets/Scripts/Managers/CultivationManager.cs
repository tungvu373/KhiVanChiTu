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

    public event Action<float, float> OnTuViChanged; // current, max
    public event Action<CultivationStageData> OnStageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
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

    private void UpdateDashboardUI(CultivationStageData currentStageData)
    {
        if (stageText != null)
        {
            float progress = (currentTuVi / currentStageData.requiredTuVi) * 100f;
            stageText.text = $"Cảnh Giới: {currentStageData.stageName} - Tiến độ: {progress:F1}%";
        }

        if (characterInfoText != null)
        {
            characterInfoText.text = $"Tên: Đạo Hữu\nCảnh giới: {currentStageData.stageName}\nTu Vi: {Mathf.FloorToInt(currentTuVi)}/{currentStageData.requiredTuVi}\nLoại: Nhân Tộc";
        }
    }

    private void CheckBreakthrough()
    {
        CultivationStageData currentStageData = allStages[currentStageIndex];
        if (currentTuVi >= currentStageData.requiredTuVi)
        {
            if (currentStageData.hasTribulationBoss)
            {
                Debug.Log("⚠️ LÔI KIẾP HÀNG LÂM! Chuẩn bị đánh Boss Lôi Kiếp!");
                GameManager.Instance.ChangeState(GameManager.GameState.BossTribulation);
            }
            else
            {
                Breakthrough();
            }
        }
    }

    public void Breakthrough()
    {
        if (currentStageIndex >= allStages.Length) return;

        CultivationStageData currentStageData = allStages[currentStageIndex];
        
        // Rung màn hình và Sấm sét chúc mừng (Chỉ khi vượt Đại Cảnh Giới)
        bool isMajorBreakthrough = currentStageData.stageName.Contains("Tầng 15") || currentStageData.stageName.Contains("Hậu Kỳ");
        if (isMajorBreakthrough)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.8f, 0.5f);
            if (lightningVFXPrefab != null && CombatManager.Instance != null)
            {
                Instantiate(lightningVFXPrefab, CombatManager.Instance.playerTransform.position + Vector3.up * 8f, Quaternion.Euler(90, 0, 0));
            }
        }

        // Reset tu vi về 0 để lên cấp mới
        currentTuVi = 0; 
        
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

        if (currentStageIndex < allStages.Length)
        {
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
        if (GameManager.Instance.currentState == GameManager.GameState.Breakthrough)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }
    }
}

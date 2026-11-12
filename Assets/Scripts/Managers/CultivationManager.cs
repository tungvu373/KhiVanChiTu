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
    private CultivationPhaseBarUI phaseBar;
    private bool tribulationPromptPending;

    // ── V3: Realm/SubLevel (song song stageIndex trong giai đoạn chuyển tiếp) ──
    public RealmId CurrentRealm    { get; private set; } = RealmId.LuyenKhi;
    public int     CurrentSubLevel { get; private set; } = 1; // 1 → 10

    private bool HasNextStage => allStages != null && currentStageIndex >= 0 && currentStageIndex < allStages.Length - 1;
    public bool IsAtBottleneck => HasNextStage && RequiresBossToBreakthrough(GetCurrentStageData());

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
            UpdateDashboardUI(allStages[currentStageIndex]);
            OnStageChanged?.Invoke(allStages[currentStageIndex]);
            OnTuViChanged?.Invoke(currentTuVi, allStages[currentStageIndex].requiredTuVi);
        }
    }

    public void AddTuVi(float amount)
    {
        var currentStageData = GetCurrentStageData();
        if (currentStageData == null || !(amount > 0f) || currentTuVi >= currentStageData.requiredTuVi) return;

        currentTuVi = Mathf.Min(currentTuVi + amount, currentStageData.requiredTuVi);
        UpdateDashboardUI(currentStageData);
        OnTuViChanged?.Invoke(currentTuVi, currentStageData.requiredTuVi);
        CheckBreakthrough();
    }

    internal float GetBaseTuViPerKill()
    {
        if (currentStageIndex < 15) return 5f * Mathf.Pow(1.2f, currentStageIndex);

        // Trúc Cơ Sơ Kỳ: 120 quái thường, tăng 20 quái mỗi mốc. Boss, Đan và Đạo Vận tính riêng.
        var stage = GetCurrentStageData();
        return stage != null ? stage.requiredTuVi / (120f + (currentStageIndex - 15) * 20f) : 0f;
    }

    /// <summary>
    /// Lấy dữ liệu cảnh giới hiện tại an toàn.
    /// </summary>
    public CultivationStageData GetCurrentStageData()
    {
        if (allStages == null || currentStageIndex < 0 || currentStageIndex >= allStages.Length) return null;
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
        var stage = GetCurrentStageData();
        return stage != null && currentTuVi >= stage.requiredTuVi;
    }

    public bool IsTribulationReady => IsAtBottleneck && IsTuViMaxed();

    public void ShowPendingTribulationPrompt()
    {
        if (tribulationPromptPending) TribulationModalUI.Open();
    }

    public void MarkTribulationPromptShown() => tribulationPromptPending = false;

    private void UpdateDashboardUI(CultivationStageData currentStageData)
    {
        if (currentStageData == null) return;
        float stageProgress = currentStageData.requiredTuVi > 0f
            ? Mathf.Clamp01(currentTuVi / currentStageData.requiredTuVi) : 0f;
        float phaseProgress = CultivationDisplay.PhaseProgress(currentStageIndex, stageProgress);
        string displayName = GetDisplayRealmName();
        if (stageText != null)
        {
            bool tribulationReady = IsTribulationReady;
            stageText.text = $"{displayName} · {phaseProgress * 100f:F0}%";
            if (tribulationReady)
                stageText.text = $"{displayName} · Độ kiếp";
            else if (!HasNextStage && stageProgress >= 1f)
                stageText.text = $"{displayName} · Viên Mãn";
            if (phaseBar == null) phaseBar = CultivationPhaseBarUI.Ensure(stageText);
            phaseBar?.Refresh(currentStageIndex, stageProgress, tribulationReady);
        }

        if (characterInfoText != null)
        {
            string nameDisplay = (string.IsNullOrEmpty(characterName) || characterName == "1") ? "Đạo Hữu" : characterName;
            string tuViCur = EconomyManager.FormatNumber(currentTuVi);
            string tuViReq = (currentStageData != null && currentStageData.requiredTuVi > 0) ? EconomyManager.FormatNumber(currentStageData.requiredTuVi) : "Max";
            characterInfoText.supportRichText = true;
            characterInfoText.text = $"<b><color=#FFD700>{nameDisplay}</color></b> · <color=#66D9EF>{displayName}</color>\n<color=#A0B0C5>Tu Vi:</color> <color=#FFFFFF>{tuViCur} / {tuViReq}</color>";
            if (characterInfoText.GetComponentInParent<JadeUITheme>() != null)
                characterInfoText.text = $"<size=28><b><color=#F1E8CF>{nameDisplay}</color></b></size>\n<size=26><color=#6EDDB6>{displayName}</color></size>\n<size=24><color=#CFBE9C>Tu vi {tuViCur} / {tuViReq}</color></size>";
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

    /// <summary>
    /// Lấy thứ hạng Đại Cảnh Giới đang cần Đột Phá:
    /// 1 = Lên Trúc Cơ (Luyện Khí 15)
    /// 2 = Lên Kết Đan (Trúc Cơ Hậu Kỳ)
    /// 3 = Lên Nguyên Anh (Kết Đan Hậu Kỳ)
    /// 4 = Lên Hóa Thần (Nguyên Anh Hậu Kỳ)
    /// </summary>
    public int GetCurrentTribulationTier()
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return 1;
        string name = allStages[currentStageIndex].stageName;
        if (name.Contains("Luyện Khí")) return 1;
        if (name.Contains("Trúc Cơ")) return 2;
        if (name.Contains("Kết Đan")) return 3;
        if (name.Contains("Nguyên Anh")) return 4;
        if (name.Contains("Hóa Thần")) return 5;
        return Mathf.Clamp((currentStageIndex / 6) + 1, 1, 6);
    }

    /// <summary>
    /// Lấy tên Đại Cảnh Giới kế tiếp
    /// </summary>
    public string GetNextRealmName()
    {
        int nextIdx = currentStageIndex + 1;
        if (allStages != null && nextIdx < allStages.Length)
        {
            return CultivationDisplay.NameForStage(nextIdx);
        }
        return "Cảnh Giới Mới";
    }

    public static float TribulationFailureLossRate => Mathf.Max(0f, 0.30f - DaoAnProgressionService.Bonus(DaoAnEffect.TribulationProtection));

    /// <summary>Áp dụng hình phạt Tu Vi sau khi độ kiếp thất bại.</summary>
    public float ApplyTribulationFailurePenalty()
    {
        if (allStages == null || currentStageIndex >= allStages.Length) return 0f;

        tribulationPromptPending = false;
        CultivationStageData currentStageData = allStages[currentStageIndex];
        float lossRate = TribulationFailureLossRate;
        float penalty = currentTuVi * lossRate;
        currentTuVi -= penalty;

        UpdateDashboardUI(currentStageData);
        OnTuViChanged?.Invoke(currentTuVi, currentStageData.requiredTuVi);

        Debug.Log($"⚡ [CultivationManager] Độ kiếp thất bại! Bị thiên kiếp phản phệ, tổn thất {lossRate:0%} Tu Vi (-{penalty:F0}). Tu vi hiện tại: {currentTuVi:F0}/{currentStageData.requiredTuVi}");
        return penalty;
    }

    private void CheckBreakthrough()
    {
        if (!HasNextStage || isBreakthroughInProgress || !IsTuViMaxed()) return;
        if (IsAtBottleneck)
        {
            tribulationPromptPending = true;
            ShowPendingTribulationPrompt();
        }
        else
        {
            Breakthrough();
        }
    }

    private bool isBreakthroughInProgress = false;

    public void Breakthrough()
    {
        if (!HasNextStage || isBreakthroughInProgress) return;
        isBreakthroughInProgress = true;

        CultivationStageData currentStageData = allStages[currentStageIndex];
        string previousDisplayName = GetDisplayRealmName();
        
        // Rung màn hình và Sấm sét chúc mừng (Chỉ khi vượt Đại Cảnh Giới)
        bool isMajorBreakthrough = RequiresBossToBreakthrough(currentStageData);
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
        tribulationPromptPending = false;
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
            coDuyenBonus = isMajorBreakthrough ? 2 : 1;
        }

        if (coDuyenBonus > 0 && EconomyManager.Instance != null) 
        {
            EconomyManager.Instance.AddCoDuyen(coDuyenBonus);
        }

        // Tăng hệ số sức mạnh vĩnh viễn
        permanentStatMultiplier *= currentStageData.statMultiplierBonus;

        currentStageIndex++;
        UpdateStageIdentity();
        AchievementManager.Instance?.OnTribulationSuccess();
        Debug.Log($"✨ ĐỘT PHÁ THÀNH CÔNG! Thưởng: {coDuyenBonus} Cơ Duyên. Hệ số SM: {permanentStatMultiplier:F2}");

        bool phaseChanged = previousDisplayName != GetDisplayRealmName();
        if (phaseChanged && GameLogger.Instance != null)
        {
            GameLogger.Instance.Log($"Đột phá: {GetDisplayRealmName()}!", Color.cyan);
        }

        var nextStage = allStages[currentStageIndex];
        if (phaseChanged) CombatNotificationUI.ShowBreakthrough(GetDisplayRealmName());
        OnStageChanged?.Invoke(nextStage);
        OnTuViChanged?.Invoke(currentTuVi, nextStage.requiredTuVi);
        UpdateDashboardUI(nextStage);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Breakthrough);
        }
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
        CheckBreakthrough();
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
        slot.realmId = (int)CurrentRealm;
        slot.subLevel = CurrentSubLevel;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        CancelInvoke(nameof(ReturnToIdle));
        CancelInvoke(nameof(CheckBreakthrough));
        isBreakthroughInProgress = false;
        tribulationPromptPending = false;
        int lastStage = allStages != null ? Mathf.Max(0, allStages.Length - 1) : 0;
        currentStageIndex = Mathf.Clamp(slot.stageIndex, 0, lastStage);
        var stage = GetCurrentStageData();
        currentTuVi = stage != null ? Mathf.Clamp(slot.currentTuVi, 0f, stage.requiredTuVi) : slot.currentTuVi;
        // stageIndex vẫn là mốc gốc, kể cả save cũ từng vượt qua Hóa Thần Hậu Kỳ.
        UpdateStageIdentity();
        permanentStatMultiplier = Mathf.Max(1f, slot.permanentStatMultiplier);

        if (stage != null)
        {
            OnStageChanged?.Invoke(stage);
            OnTuViChanged?.Invoke(currentTuVi, stage.requiredTuVi);
            UpdateDashboardUI(stage);

            // Đợi các manager còn lại nạp xong trước khi khôi phục một lần thăng cấp đang chờ.
            if (HasNextStage && !IsAtBottleneck && IsTuViMaxed())
                Invoke(nameof(CheckBreakthrough), 0f);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// [Cheat Tool Editor] Thiết lập cảnh giới và tu vi trực tiếp từ Editor Tool.
    /// Hoàn toàn bị strip khi Build game.
    /// </summary>
    public void SetStageAndTuViForEditor(int stageIndex, float tuVi)
    {
        if (allStages == null || allStages.Length == 0) return;
        currentStageIndex = Mathf.Clamp(stageIndex, 0, allStages.Length - 1);
        var stageData = allStages[currentStageIndex];
        currentTuVi = tuVi;
        UpdateStageIdentity();

        UpdateDashboardUI(stageData);
        OnStageChanged?.Invoke(stageData);
        OnTuViChanged?.Invoke(currentTuVi, stageData.requiredTuVi);
    }
#endif

    private void UpdateStageIdentity()
    {
        SaveMigration.MapLegacyStage(currentStageIndex, out RealmId realm, out int subLevel);
        CurrentRealm = realm;
        CurrentSubLevel = subLevel;
        SwordLoadoutService.Instance?.UpdateCapacity(realm, subLevel);
    }

    /// <summary>
    /// Tên cảnh giới hiển thị theo stage đã lưu, không dùng stageName legacy.
    /// </summary>
    public string GetDisplayRealmName()
    {
        return CultivationDisplay.NameForStage(currentStageIndex);
    }
}


using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public int currentStage = 1;
    public int enemiesKilled = 0;
    public int enemiesToBoss = 10; // Cập nhật động theo tầng (1-10: 10, 11-20: 20...)

    public bool isBossStage = false;
    public float bossTimer = 30f;
    
    [Header("UI References (Kéo thả từ Hierarchy vào đây)")]
    public Text stageText;
    public Image progressBar;
    public GameObject bossTimerUI;
    public Text bossTimerText;

    /// <summary>
    /// Số lượng quái thường cần tiêu diệt để vượt tầng (hoặc diện kiến Boss mỗi 10 tầng):
    /// - Tầng 1 - 10: 10 quái (10/10)
    /// - Tầng 11 - 20: 20 quái (20/20)
    /// - Tầng 21 - 30: 30 quái (30/30)
    /// Tăng dần thêm 10 quái mỗi mốc 10 tầng.
    /// </summary>
    public static int GetRequiredEnemiesForStage(int stage)
    {
        if (stage < 1) stage = 1;
        return ((stage - 1) / 10 + 1) * 10;
    }

    public void UpdateEnemiesRequirement()
    {
        enemiesToBoss = GetRequiredEnemiesForStage(currentStage);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        
        UpdateEnemiesRequirement();
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        if (isBossStage && bossTimerText != null)
        {
            bossTimer -= Time.deltaTime;
            bossTimerText.text = $"Thời gian: {Mathf.Ceil(bossTimer)}s";

            if (bossTimer <= 0)
            {
                // Thua Boss do hết giờ
                FailBoss();
            }
        }
    }

    public void OnEnemyKilled(bool isBoss)
    {
        if (isBoss)
        {
            if (isBossStage)
            {
                // Thắng Boss Tầng (Mỗi 10 tầng: 10, 20, 30...) -> Qua tầng tiếp theo!
                PassStage();
            }
            // Boss Độ Kiếp (Tribulation Boss) được CombatManager quản lý độc lập, không can thiệp vào tầng
            return;
        }

        if (!isBossStage)
        {
            enemiesKilled++;
            if (enemiesKilled >= enemiesToBoss)
            {
                if (currentStage % 10 == 0) // Cứ 10 tầng mới gặp Boss 1 lần
                {
                    TriggerBoss();
                }
                else
                {
                    // Tầng thường thì qua màn luôn
                    PassStage();
                }
            }
            UpdateUI();
        }
    }

    private void TriggerBoss()
    {
        isBossStage = true;
        bossTimer = 30f;
        if (bossTimerUI != null) bossTimerUI.SetActive(true);
        UpdateUI();
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ChangeToBossState();
        }
    }

    private void PassStage()
    {
        isBossStage = false;
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        currentStage++;
        enemiesKilled = 0;
        UpdateEnemiesRequirement();
        
        UpdateUI();
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ChangeToIdleState();
        }

        // Đổi màu nền nhẹ để tạo cảm giác chuyển map
        if (MapScroller.Instance != null)
        {
            MapScroller.Instance.RandomizeBiomeColor();
        }

        // Tự động lưu tiến trình qua màn ngay lập tức
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }

        // Nếu người chơi đã đạt Tu Vi Đại Viên Mãn (cần độ kiếp) nhưng trước đó đã bấm Hủy (để củng cố sức mạnh),
        // thì sau mỗi khi vượt tầng, hệ thống sẽ mở lại Panel hỏi người chơi có muốn độ kiếp thăng cấp hay không.
        if (CultivationManager.Instance != null && CultivationManager.Instance.IsTuViMaxed())
        {
            var curData = CultivationManager.Instance.GetCurrentStageData();
            if (CultivationManager.Instance.RequiresBossToBreakthrough(curData))
            {
                TribulationModalUI.Open();
            }
        }
    }

    public void FailBoss()
    {
        ResetCurrentStageOnDeath();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ChangeToIdleState();
        }
    }

    // Làm mới lại tiến độ của tầng hiện tại khi người chơi tử trận
    public void ResetCurrentStageOnDeath()
    {
        isBossStage = false;
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        enemiesKilled = 0; // Làm mới lại từ đầu tiến trình quái của tầng này
        UpdateEnemiesRequirement();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.IdleFarm);
        }

        UpdateUI();
        
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }

        Debug.Log($"🔄 [StageManager] Nhân vật tử trận! Làm mới lại Tầng {currentStage} [0/{enemiesToBoss}]");
    }

    public void UpdateUI()
    {
        UpdateEnemiesRequirement();
        if (stageText == null) return;

        if (isBossStage)
        {
            stageText.text = $"Tầng {currentStage} - KHIÊU CHIẾN LÔI KIẾP!";
            stageText.color = Color.red;
            if (progressBar != null) progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f); // Đầy thanh
        }
        else
        {
            stageText.text = $"Tầng {currentStage} [ {enemiesKilled} / {enemiesToBoss} ]";
            stageText.color = Color.white;
            float ratio = enemiesToBoss > 0 ? (float)enemiesKilled / enemiesToBoss : 0f;
            if (progressBar != null) progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(ratio, 1f);
        }
    }

    /// <summary>
    /// Tạm thời ẩn các thành phần hiển thị Stage tầng khi đang đối đầu Boss Thiên Kiếp,
    /// và hiển thị lại khi trận chiến Thiên Kiếp kết thúc.
    /// Ẩn sạch sẽ toàn bộ cụm StageUI (StageText, ProgressBG, ProgressFill, BossTimer),
    /// tuyệt đối kiểm tra không tắt Canvas cha.
    /// </summary>
    public void SetStageUIVisible(bool visible)
    {
        // 1. Ẩn/hiện StageText
        if (stageText != null)
        {
            stageText.gameObject.SetActive(visible);
        }

        // 2. Ẩn/hiện ProgressBar Fill & ProgressBG
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(visible);

            // Ẩn/hiện luôn ProgressBG (GameObject cha trực tiếp của ProgressBar Fill)
            if (progressBar.transform.parent != null && progressBar.transform.parent.GetComponent<Canvas>() == null)
            {
                progressBar.transform.parent.gameObject.SetActive(visible);
            }
        }

        // 3. Ẩn/hiện BossTimer
        if (bossTimerUI != null)
        {
            if (!visible) bossTimerUI.SetActive(false);
            else if (isBossStage) bossTimerUI.SetActive(true);
        }

        // 4. Ẩn/hiện cả Container tổng StageUI nếu có
        Transform container = null;
        if (stageText != null && stageText.transform.parent != null)
        {
            container = stageText.transform.parent;
        }
        else if (progressBar != null && progressBar.transform.parent != null && progressBar.transform.parent.parent != null)
        {
            container = progressBar.transform.parent.parent;
        }

        if (container != null && container.GetComponent<Canvas>() == null)
        {
            container.gameObject.SetActive(visible);
        }
    }

    // ── SAVE / LOAD ─────────────────────────────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.combatStage = currentStage;
        slot.enemiesKilled = enemiesKilled;
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        currentStage = Mathf.Max(1, slot.combatStage);
        UpdateEnemiesRequirement();
        enemiesKilled = Mathf.Clamp(slot.enemiesKilled, 0, enemiesToBoss);
        isBossStage = false; // Khi load lại luôn ở trạng thái farm thường
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        UpdateUI();
        Debug.Log($"[StageManager] Đã nạp dữ liệu: Tầng {currentStage} ({enemiesKilled}/{enemiesToBoss})");
    }
}

using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public int currentStage = 1;
    public int enemiesKilled = 0;
    public int enemiesToBoss = 20; // 20 quái nhỏ mỗi stage (để tạo cảm giác đông đúc)

    public bool isBossStage = false;
    public float bossTimer = 30f;
    
    [Header("UI References (Kéo thả từ Hierarchy vào đây)")]
    public Text stageText;
    public Image progressBar;
    public GameObject bossTimerUI;
    public Text bossTimerText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        
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
        if (isBoss && isBossStage)
        {
            // Thắng Boss
            PassStage();
        }
        else if (!isBossStage)
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
        enemiesKilled = 0; // Làm mới lại từ đầu tiến trình quái của tầng này (0 / 20)
        
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
            float ratio = (float)enemiesKilled / enemiesToBoss;
            if (progressBar != null) progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(ratio, 1f);
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
        enemiesKilled = Mathf.Clamp(slot.enemiesKilled, 0, enemiesToBoss);
        isBossStage = false; // Khi load lại luôn ở trạng thái farm thường
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        UpdateUI();
        Debug.Log($"[StageManager] Đã nạp dữ liệu: Tầng {currentStage} ({enemiesKilled}/{enemiesToBoss})");
    }
}

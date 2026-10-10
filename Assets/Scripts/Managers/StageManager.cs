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
            Destroy(gameObject);
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
                TriggerBoss();
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
    }

    public void FailBoss()
    {
        isBossStage = false;
        if (bossTimerUI != null) bossTimerUI.SetActive(false);
        enemiesKilled = 0; // Đánh lại từ đầu stage hiện tại
        
        UpdateUI();
        
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ChangeToIdleState();
        }
    }

    private void UpdateUI()
    {
        if (stageText == null) return;

        if (isBossStage)
        {
            stageText.text = $"Tầng {currentStage} - KHIÊU CHIẾN LÔI KIẾP!";
            stageText.color = Color.red;
            progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f); // Đầy thanh
        }
        else
        {
            stageText.text = $"Tầng {currentStage} [ {enemiesKilled} / {enemiesToBoss} ]";
            stageText.color = Color.white;
            float ratio = (float)enemiesKilled / enemiesToBoss;
            if (progressBar != null) progressBar.GetComponent<RectTransform>().anchorMax = new Vector2(ratio, 1f);
        }
    }
}

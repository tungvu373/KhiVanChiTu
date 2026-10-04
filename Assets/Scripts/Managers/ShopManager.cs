using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Buffs (Giới hạn tối đa 150s - 10 viên)")]
    public float manaRegenBuffTimer = 0f;
    public float speedBuffTimer = 0f;
    public float damageBuffTimer = 0f;
    public float spiritBuffTimer = 0f; 
    public float hpRegenVisualTimer = 0f; 

    private Text[] buffTexts = new Text[5];
    private Image[] buffFills = new Image[5];
    private GameObject[] buffRows = new GameObject[5];

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
        FindBuffUITexts();
    }

    private void FindBuffUITexts()
    {
        // Luôn tìm kiếm từ gốc Canvas để đảm bảo không bị trượt nếu hierarchy phức tạp
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        Transform abPanelT = canvas.transform.Find("ActiveBuffPanel");
        if (abPanelT == null) return;

        for (int i = 0; i < 5; i++)
        {
            Transform rowT = abPanelT.Find($"BuffRow_{i}");
            if (rowT != null)
            {
                buffRows[i] = rowT.gameObject;
                
                Transform txtT = rowT.Find("Text");
                if (txtT != null) buffTexts[i] = txtT.GetComponent<Text>();
                
                Transform fillT = rowT.Find("BgBar/FillBar");
                if (fillT != null) buffFills[i] = fillT.GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        if (manaRegenBuffTimer > 0) manaRegenBuffTimer -= Time.deltaTime;
        if (speedBuffTimer > 0) speedBuffTimer -= Time.deltaTime;
        if (hpRegenVisualTimer > 0) hpRegenVisualTimer -= Time.deltaTime;
        if (damageBuffTimer > 0) damageBuffTimer -= Time.deltaTime;
        if (spiritBuffTimer > 0) spiritBuffTimer -= Time.deltaTime;

        UpdateBuffUI();
    }

    private void UpdateBuffUI()
    {
        if (buffRows[0] == null)
        {
            FindBuffUITexts();
        }
        
        UpdateRow(0, "Hồi Linh: +5%", manaRegenBuffTimer);
        UpdateRow(1, "Tốc Độ: +5%", speedBuffTimer);
        UpdateRow(2, "Hồi Máu: +5%/s", hpRegenVisualTimer);
        UpdateRow(3, "Sát Thương: +5%", damageBuffTimer);
        UpdateRow(4, "Thần Thức: +5%", spiritBuffTimer);
    }

    private void UpdateRow(int index, string label, float timer)
    {
        if (buffRows[index] != null)
        {
            if (timer > 0)
            {
                buffRows[index].SetActive(true);
                if (buffTexts[index] != null)
                {
                    buffTexts[index].text = $"{label} ({Mathf.Ceil(timer)}s)";
                }
                if (buffFills[index] != null)
                {
                    // Scale chiều ngang của thanh (thời gian tối đa 150s)
                    float fillRatio = Mathf.Clamp01(timer / 150f);
                    RectTransform fillRt = buffFills[index].GetComponent<RectTransform>();
                    fillRt.anchorMax = new Vector2(fillRatio, 1);
                }
            }
            else
            {
                buffRows[index].SetActive(false);
            }
        }
    }

    public void BuyItem(int itemIndex)
    {
        int stage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        float baseCost = (itemIndex == 4) ? 1000f : 500f; // Tĩnh Tâm Đan
        float cost = baseCost * Mathf.Pow(1.2f, stage);

        // Check giới hạn 150s (10 viên * 15s)
        float currentTimer = GetTimerByIndex(itemIndex);
        if (currentTimer + 15f > 150f)
        {
            Debug.Log("[Shop] Giới hạn tối đa 10 viên mỗi loại! Đã đạt max buff.");
            return;
        }

        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendLinhThach(cost))
        {
            ApplyBuff(itemIndex);
            
            // Tìm lại UI trong trường hợp Editor vừa sinh UI
            FindBuffUITexts(); 
            
            Debug.Log($"[Shop] Mua thành công vật phẩm số {itemIndex}");
            
            if (GameLogger.Instance != null)
            {
                string[] names = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Bạo Đan", "Ngưng Thần Đan" };
                GameLogger.Instance.Log($"Mua: {names[itemIndex]}", Color.green);
            }
        }
        else
        {
            Debug.LogWarning($"[Shop] LỖI: Không đủ Linh Thạch! Cần {cost} LT nhưng bạn chỉ có {(EconomyManager.Instance != null ? EconomyManager.Instance.currentLinhThach : 0)} LT.");
        }
    }

    private float GetTimerByIndex(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return manaRegenBuffTimer;
            case 1: return speedBuffTimer;
            case 2: return hpRegenVisualTimer;
            case 3: return damageBuffTimer;
            case 4: return spiritBuffTimer;
        }
        return 0;
    }

    private void ApplyBuff(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: manaRegenBuffTimer += 15f; break;
            case 1: speedBuffTimer += 15f; break;
            case 2: hpRegenVisualTimer += 15f; break;
            case 3: damageBuffTimer += 15f; break;
            case 4: spiritBuffTimer += 15f; break;
        }
    }

    // Helper functions for other managers to read buff states
    private float GetScaledBuff()
    {
        int stage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        return 1f + 0.05f + (stage * 0.005f);
    }

    public float GetManaRegenMultiplier() => manaRegenBuffTimer > 0 ? GetScaledBuff() : 1f;
    public float GetSpeedMultiplier() => speedBuffTimer > 0 ? GetScaledBuff() : 1f;
    public float GetDamageMultiplier() => damageBuffTimer > 0 ? GetScaledBuff() : 1f;
    public float GetSpiritMultiplier() => spiritBuffTimer > 0 ? GetScaledBuff() : 1f;
}

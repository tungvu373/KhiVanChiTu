using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Buffs (Cộng dồn tối đa 5 lọ = 75s, +25%)")]
    public float manaRegenBuffTimer = 0f;
    public float speedBuffTimer = 0f;
    public float damageBuffTimer = 0f;
    public float spiritBuffTimer = 0f; 
    public float hpRegenVisualTimer = 0f; 

    [Header("Buff Stacks (1 lọ 5% -> 5 lọ 25%)")]
    public int manaRegenStacks = 0;
    public int speedStacks = 0;
    public int hpRegenStacks = 0;
    public int damageStacks = 0;
    public int spiritStacks = 0;

    private Text[] buffTexts = new Text[5];
    private Image[] buffFills = new Image[5];
    private GameObject[] buffRows = new GameObject[5];

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
        if (manaRegenBuffTimer > 0)
        {
            manaRegenBuffTimer -= Time.deltaTime;
            if (manaRegenBuffTimer <= 0) { manaRegenBuffTimer = 0; manaRegenStacks = 0; }
        }
        if (speedBuffTimer > 0)
        {
            speedBuffTimer -= Time.deltaTime;
            if (speedBuffTimer <= 0) { speedBuffTimer = 0; speedStacks = 0; }
        }
        if (hpRegenVisualTimer > 0)
        {
            hpRegenVisualTimer -= Time.deltaTime;
            if (hpRegenVisualTimer <= 0) { hpRegenVisualTimer = 0; hpRegenStacks = 0; }
        }
        if (damageBuffTimer > 0)
        {
            damageBuffTimer -= Time.deltaTime;
            if (damageBuffTimer <= 0) { damageBuffTimer = 0; damageStacks = 0; }
        }
        if (spiritBuffTimer > 0)
        {
            spiritBuffTimer -= Time.deltaTime;
            if (spiritBuffTimer <= 0) { spiritBuffTimer = 0; spiritStacks = 0; }
        }

        UpdateBuffUI();
    }

    private void UpdateBuffUI()
    {
        if (buffRows[0] == null)
        {
            FindBuffUITexts();
        }
        
        UpdateRow(0, "Hồi Linh", manaRegenStacks, manaRegenBuffTimer);
        UpdateRow(1, "Tốc Độ", speedStacks, speedBuffTimer);
        UpdateRow(2, "Hồi Máu", hpRegenStacks, hpRegenVisualTimer);
        UpdateRow(3, "Sát Thương", damageStacks, damageBuffTimer);
        UpdateRow(4, "Thần Thức", spiritStacks, spiritBuffTimer);
    }

    private void UpdateRow(int index, string label, int stacks, float timer)
    {
        if (buffRows[index] != null)
        {
            if (timer > 0 && stacks > 0)
            {
                buffRows[index].SetActive(true);
                if (buffTexts[index] != null)
                {
                    int percent = stacks * 5;
                    string unit = (index == 2) ? "%/s" : "%";
                    buffTexts[index].text = $"{label}: +{percent}{unit} (x{stacks}) [{Mathf.Ceil(timer)}s]";
                }
                if (buffFills[index] != null)
                {
                    // Scale chiều ngang của thanh (thời gian tối đa 75s = 5 lọ)
                    float fillRatio = Mathf.Clamp01(timer / 75f);
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

    public int GetStacksByIndex(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0: return manaRegenStacks;
            case 1: return speedStacks;
            case 2: return hpRegenStacks;
            case 3: return damageStacks;
            case 4: return spiritStacks;
        }
        return 0;
    }

    public float GetTimerByIndex(int itemIndex)
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

    public void BuyItem(int itemIndex)
    {
        int currentStacks = GetStacksByIndex(itemIndex);
        if (currentStacks >= 5)
        {
            Debug.Log("[Shop] Đã đạt giới hạn tối đa 5 lọ cộng dồn (+25%)! Hãy đợi hết hiệu lực.");
            if (GameLogger.Instance != null)
            {
                string[] names = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Nộ Đan", "Tĩnh Tâm Đan" };
                GameLogger.Instance.Log($"{names[itemIndex]} đã đạt tối đa 5 lọ (+25%)!", Color.yellow);
            }
            return;
        }

        int stage = CultivationManager.Instance != null ? CultivationManager.Instance.currentStageIndex : 0;
        float baseCost = (itemIndex == 4) ? 1000f : 500f; // Tĩnh Tâm Đan
        float cost = baseCost * Mathf.Pow(1.2f, stage);

        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendLinhThach(cost))
        {
            ApplyBuff(itemIndex);
            
            // Tìm lại UI trong trường hợp Editor vừa sinh UI
            FindBuffUITexts(); 
            
            string[] names = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Nộ Đan", "Tĩnh Tâm Đan" };
            int newStacks = GetStacksByIndex(itemIndex);
            Debug.Log($"[Shop] Mua thành công {names[itemIndex]} (Tầng {newStacks}/5 - +{newStacks * 5}%)");
            
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log($"Dùng {names[itemIndex]}: +{newStacks * 5}% (Tầng {newStacks}/5, {Mathf.Ceil(GetTimerByIndex(itemIndex))}s)", Color.green);
            }
        }
        else
        {
            Debug.LogWarning($"[Shop] LỖI: Không đủ Linh Thạch! Cần {cost} LT nhưng bạn chỉ có {(EconomyManager.Instance != null ? EconomyManager.Instance.currentLinhThach : 0)} LT.");
        }
    }

    public void ApplyBuff(int itemIndex)
    {
        switch (itemIndex)
        {
            case 0:
                manaRegenStacks = Mathf.Min(5, manaRegenStacks + 1);
                manaRegenBuffTimer = Mathf.Min(75f, manaRegenBuffTimer + 15f);
                break;
            case 1:
                speedStacks = Mathf.Min(5, speedStacks + 1);
                speedBuffTimer = Mathf.Min(75f, speedBuffTimer + 15f);
                break;
            case 2:
                hpRegenStacks = Mathf.Min(5, hpRegenStacks + 1);
                hpRegenVisualTimer = Mathf.Min(75f, hpRegenVisualTimer + 15f);
                break;
            case 3:
                damageStacks = Mathf.Min(5, damageStacks + 1);
                damageBuffTimer = Mathf.Min(75f, damageBuffTimer + 15f);
                break;
            case 4:
                spiritStacks = Mathf.Min(5, spiritStacks + 1);
                spiritBuffTimer = Mathf.Min(75f, spiritBuffTimer + 15f);
                break;
        }
    }

    // Helper functions for other managers to read buff states
    public float GetManaRegenMultiplier() => (manaRegenBuffTimer > 0 && manaRegenStacks > 0) ? (1f + manaRegenStacks * 0.05f) : 1f;
    public float GetSpeedMultiplier() => (speedBuffTimer > 0 && speedStacks > 0) ? (1f + speedStacks * 0.05f) : 1f;
    public float GetDamageMultiplier() => (damageBuffTimer > 0 && damageStacks > 0) ? (1f + damageStacks * 0.05f) : 1f;
    public float GetSpiritMultiplier() => (spiritBuffTimer > 0 && spiritStacks > 0) ? (1f + spiritStacks * 0.05f) : 1f;
    public float GetHpRegenRate() => (hpRegenVisualTimer > 0 && hpRegenStacks > 0) ? (hpRegenStacks * 0.05f) : 0f;
}

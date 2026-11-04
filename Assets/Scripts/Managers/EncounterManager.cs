using UnityEngine;
using UnityEngine.EventSystems;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    public float spawnIntervalMin = 15f;
    public float spawnIntervalMax = 60f; // Tần suất xuất hiện: 15-60s
    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        timer = 15f; // Lần đầu tiên luôn là 15s để người chơi dễ tiếp cận
    }

    private void Update()
    {
        if (Time.timeScale <= 0f ||
            (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) ||
            (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay)) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (!CanSpawnEncounter()) return;
            SpawnEncounter();
            timer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    public static bool CanSpawnEncounter()
    {
        if (GameplayChoicesService.Instance != null && (GameplayChoicesService.Instance.IsChoosing || GameplayChoicesService.Instance.ChallengeRemaining > 0)) return false;
        if (Time.timeScale <= 0f) return false;
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) return false;
        if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay) return false;
        return GameObject.Find("PauseMenuModal") == null && GameObject.Find("TribulationModalUI") == null &&
               GameObject.Find("OfflineRewardPopupUI") == null && GameObject.Find("CharacterDetailModal") == null &&
               GameObject.Find("MilestoneModalUI") == null;
    }

    private void SpawnEncounter()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        RectTransform playArea = canvas.transform.Find("MobileUIRoot") as RectTransform;
        if (playArea == null) playArea = canvas.transform as RectTransform;
        if (playArea == null || playArea.rect.width <= 0f || playArea.rect.height <= 0f) return;

        // Tạo prefab túi quà ngẫu nhiên
        GameObject encounterObj = new GameObject("RandomEncounter", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        encounterObj.transform.SetParent(playArea, false);
        
        RectTransform rt = encounterObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(110f, 110f);
        
        // Spawn ngẫu nhiên ở ngoài viền lề trái hoặc phải
        bool spawnLeft = Random.value > 0.5f;
        float edgeX = Mathf.Max(0f, playArea.rect.width * 0.5f - rt.sizeDelta.x * 0.5f);
        float startX = spawnLeft ? -edgeX : edgeX;
        float startY = Random.Range(-playArea.rect.height * 0.20f, playArea.rect.height * 0.30f);
        rt.anchoredPosition = new Vector2(startX, startY);

        UnityEngine.UI.Image img = encounterObj.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(1f, 0.8f, 0f); // Màu Vàng rực rỡ
        
        // Thêm outline cho nổi bật
        UnityEngine.UI.Outline outline = encounterObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.red;
        outline.effectDistance = new Vector2(2, -2);
        
        // Chữ "?" ở giữa
        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(UnityEngine.UI.Text));
        txtObj.transform.SetParent(encounterObj.transform, false);
        UnityEngine.UI.Text txt = txtObj.GetComponent<UnityEngine.UI.Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "?";
        txt.color = Color.red;
        txt.fontSize = 40;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        txt.GetComponent<RectTransform>().anchorMax = Vector2.one;
        txt.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Di chuyển qua màn hình (đối diện)
        float targetX = -startX;
        
        EncounterItem item = encounterObj.AddComponent<EncounterItem>();
        item.targetPos = new Vector2(targetX, startY);
        item.speed = 120f; // Tốc độ bay
        
        Debug.Log("[Encounter] Một Kỳ Ngộ ngẫu nhiên vừa xuất hiện!");
    }
}

public class EncounterItem : MonoBehaviour, IPointerClickHandler
{
    public Vector2 targetPos;
    public float speed = 100f;
    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rt.anchoredPosition = Vector2.MoveTowards(rt.anchoredPosition, targetPos, speed * Time.deltaTime);
        if (Vector2.Distance(rt.anchoredPosition, targetPos) < 1f)
        {
            Destroy(gameObject); // Bay hết màn hình thì biến mất
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!EncounterManager.CanSpawnEncounter()) return;
        if (GameplayChoicesService.Instance != null)
        {
            if (GameplayChoicesService.Instance.OfferEncounter()) Destroy(gameObject);
            return;
        }
        AchievementManager.Instance?.OnEncounterCollected();
        // Phân chia tỷ lệ cân bằng: 50% Kiếm, 40% Đan Dược (Thuốc), 10% Cơ Duyên
        float rng = Random.value;
        if (rng < 0.5f) // 50% rớt kiếm
        {
            if (MergeManager.Instance != null)
            {
                int lvl = Random.Range(2, 5); // Lv2 - Lv4
                bool added = MergeManager.Instance.TryAddSword(lvl);
                if (!added)
                {
                    float compensation = 500f * lvl;
                    EconomyManager.Instance?.AddLinhThach(compensation);
                    if (GameLogger.Instance != null) GameLogger.Instance.Log($"Túi kiếm đầy! Quy đổi Kỳ Ngộ thành {compensation:F0} Linh Thạch.", Color.yellow);
                    Destroy(gameObject);
                    return;
                }
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Kỳ Ngộ: Đã nhặt được 1 Phôi Kiếm Lv{lvl}!", Color.yellow);
            }
        }
        else if (rng < 0.9f) // 40% rớt Đan Dược ngẫu nhiên trong Shop
        {
            if (ShopManager.Instance != null)
            {
                int randomBuff = Random.Range(0, 5); // 0 đến 4
                ShopManager.Instance.ApplyBuff(randomBuff);
                
                string[] names = { "Hồi Linh Đan", "Tật Phong Đan", "Hồi Huyết Đan", "Cuồng Bạo Đan", "Ngưng Thần Đan" };
                if (GameLogger.Instance != null) GameLogger.Instance.Log($"Kỳ Ngộ: Nhặt được 1 {names[randomBuff]}!", Color.green);
            }
        }
        else // 10% rớt Cơ Duyên
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddCoDuyen(1);
                if (GameLogger.Instance != null) GameLogger.Instance.Log("Kỳ Ngộ: Nhận được 1 Cơ Duyên!", Color.cyan);
            }
        }
        
        Destroy(gameObject);
    }
}

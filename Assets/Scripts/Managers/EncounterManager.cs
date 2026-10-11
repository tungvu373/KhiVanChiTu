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
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnEncounter();
            timer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    private void SpawnEncounter()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // Tạo prefab túi quà ngẫu nhiên
        GameObject encounterObj = new GameObject("RandomEncounter", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        encounterObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = encounterObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        
        // Spawn ngẫu nhiên ở ngoài viền lề trái hoặc phải
        bool spawnLeft = Random.value > 0.5f;
        float startX = spawnLeft ? -450f : 450f;
        float startY = Random.Range(-200f, 200f);
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
        float targetX = spawnLeft ? 450f : -450f;
        
        EncounterItem item = encounterObj.AddComponent<EncounterItem>();
        item.targetPos = new Vector2(targetX, startY + Random.Range(-100f, 100f));
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
        // Phân chia tỷ lệ cân bằng: 50% Kiếm, 40% Đan Dược (Thuốc), 10% Cơ Duyên
        float rng = Random.value;
        if (rng < 0.5f) // 50% rớt kiếm
        {
            if (MergeManager.Instance != null)
            {
                int lvl = Random.Range(2, 5); // Lv2 - Lv4
                MergeManager.Instance.TryAddSword(lvl);
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

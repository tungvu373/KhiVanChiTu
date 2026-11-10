using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    public float spawnIntervalMin = 15f;
    public float spawnIntervalMax = 60f; // Tần suất xuất hiện: 15-60s
    private float timer;
    private EncounterItem activeEncounter;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        timer = 15f; // Lần đầu tiên luôn là 15s để người chơi dễ tiếp cận
    }

    private void Update()
    {
        if (activeEncounter != null || !CanSpawnEncounter()) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnEncounter();
            timer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    public static bool CanSpawnEncounter()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.IdleFarm) return false;
        return CanInteractWithEncounter();
    }

    internal static bool CanInteractWithEncounter()
    {
        var state = GameManager.Instance != null ? GameManager.Instance.currentState : GameManager.GameState.IdleFarm;
        if (Time.timeScale <= 0f || state == GameManager.GameState.BossTribulation || state == GameManager.GameState.Defeat) return false;
        var menu = MainMenuManager.Instance;
        if (menu != null && menu.isActiveAndEnabled) return false;
        var choices = GameplayChoicesService.Instance;
        if (choices != null)
            return !choices.IsChoosing && choices.ChallengeRemaining == 0 && !choices.IsAnyModalBlocking();

        if (MobileDrawerUIController.Instance != null && MobileDrawerUIController.Instance.IsBlockingGameplay) return false;
        return GameObject.Find("PauseMenuModal") == null && GameObject.Find("TribulationModalUI") == null &&
               GameObject.Find("OfflineRewardPopupUI") == null && GameObject.Find("CharacterDetailModal") == null &&
               GameObject.Find("MilestoneModalUI") == null;
    }

    private void SpawnEncounter()
    {
        if (activeEncounter != null) return;
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        RectTransform playArea = canvas.transform.Find("MobileUIRoot") as RectTransform;
        if (playArea == null) playArea = canvas.transform as RectTransform;
        if (playArea == null || !playArea.gameObject.activeInHierarchy || playArea.rect.width <= 0f || playArea.rect.height <= 0f) return;

        // Tạo prefab túi quà ngẫu nhiên
        GameObject encounterObj = new GameObject("RandomEncounter", typeof(RectTransform), typeof(Image),
            typeof(Button), typeof(CanvasGroup));
        encounterObj.transform.SetParent(playArea, false);
        
        RectTransform rt = encounterObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        float touchSize = playArea.rect.width * (48f / 360f);
        rt.sizeDelta = new Vector2(touchSize, touchSize * 0.92f);
        
        // Spawn ngẫu nhiên ở ngoài viền lề trái hoặc phải
        bool spawnLeft = Random.value > 0.5f;
        float edgeX = Mathf.Max(0f, playArea.rect.width * 0.5f - rt.sizeDelta.x * 0.5f);
        float startX = spawnLeft ? -edgeX : edgeX;
        float startY = Random.Range(playArea.rect.height * 0.10f, playArea.rect.height * 0.23f);
        rt.anchoredPosition = new Vector2(startX, startY);

        var img = encounterObj.GetComponent<Image>();
        img.color = new Color(0.16f, 0.10f, 0.08f, 0.96f);
        encounterObj.GetComponent<Button>().targetGraphic = img;
        
        // Thêm outline cho nổi bật
        var outline = encounterObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.74f, 0.25f, 1f);
        outline.effectDistance = new Vector2(3, -3);
        
        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(encounterObj.transform, false);
        var txt = txtObj.GetComponent<Text>();
        txt.font = UiTextStyle.BodyFont;
        txt.text = "KỲ NGỘ\nChạm mở";
        txt.color = new Color(1f, 0.85f, 0.44f);
        txt.fontSize = 24;
        txt.fontStyle = FontStyle.Bold;
        txt.raycastTarget = false;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        txt.GetComponent<RectTransform>().anchorMax = Vector2.one;
        txt.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Di chuyển qua màn hình (đối diện)
        float targetX = -startX;
        
        activeEncounter = encounterObj.AddComponent<EncounterItem>();
        activeEncounter.targetPos = new Vector2(targetX, startY);
        activeEncounter.speed = 105f;
        
        Debug.Log("[Encounter] Một Kỳ Ngộ ngẫu nhiên vừa xuất hiện!");
    }
}

[RequireComponent(typeof(RectTransform), typeof(Button), typeof(CanvasGroup))]
public class EncounterItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 targetPos;
    public float speed = 100f;
    private RectTransform rt;
    private CanvasGroup canvasGroup;
    private Button button;
    private int? heldPointer;
    private float edgeTimeRemaining = 1.5f;
    private bool collected;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Collect);
    }

    private void Update()
    {
        bool available = !collected && EncounterManager.CanInteractWithEncounter();
        canvasGroup.alpha = available ? 1f : 0f;
        canvasGroup.blocksRaycasts = canvasGroup.interactable = available;
        if (!available)
        {
            heldPointer = null;
            return;
        }
        if (heldPointer.HasValue) return;

        rt.anchoredPosition = Vector2.MoveTowards(rt.anchoredPosition, targetPos, speed * Time.deltaTime);
        if (Vector2.Distance(rt.anchoredPosition, targetPos) < 1f)
        {
            edgeTimeRemaining -= Time.deltaTime;
            if (edgeTimeRemaining <= 0f) Destroy(gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !heldPointer.HasValue)
            heldPointer = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (heldPointer == eventData.pointerId) heldPointer = null;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) heldPointer = null;
    }

    private void Collect()
    {
        if (collected || !EncounterManager.CanInteractWithEncounter()) return;
        if (GameplayChoicesService.Instance != null)
        {
            if (!GameplayChoicesService.Instance.OfferEncounter()) return;
            collected = true;
            Destroy(gameObject);
            return;
        }
        collected = true;
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

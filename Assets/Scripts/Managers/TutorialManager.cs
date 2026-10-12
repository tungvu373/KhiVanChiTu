using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TutorialManager : MonoBehaviour, IPointerClickHandler
{
    public static TutorialManager Instance { get; private set; }

    [Header("Trạng thái các bước Hướng dẫn")]
    public bool swordDone = false;
    public bool upgradeDone = false;
    public bool coDuyenDone = false;
    public bool shopDone = false;

    [Header("UI References")]
    public GameObject tutorialOverlay; 
    public Text tutorialText;

    private Coroutine tutorialCoroutine;
    private float overlayShowTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Xóa sạch key cũ trong PlayerPrefs để không xung đột với hệ thống Save Slot theo nhân vật
        if (PlayerPrefs.HasKey("TutorialCompleted"))
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
        }

        EnsureUI();
    }

    private void Start()
    {
        EnsureUI();
        RestartTutorial();
    }

    private void Update()
    {
        // Nếu Tutorial đang hiển thị trên màn hình: Bắt trực tiếp mọi thao tác click/chạm
        if (tutorialOverlay == null || !tutorialOverlay.activeSelf) return;

        // Chống click đúp vô tình làm tắt ngay lập tức trong frame vừa xuất hiện (trễ 0.1s unscaled)
        if (Time.unscaledTime - overlayShowTime < 0.1f) return;

        bool userClicked = false;

        // 1. Kiểm tra New Input System (Mouse / Touchscreen)
        #if ENABLE_INPUT_SYSTEM || true
        try
        {
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                userClicked = true;
            }
            else if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                userClicked = true;
            }
            else if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                userClicked = true;
            }
        }
        catch { }
        #endif

        // 2. Fallback sang Legacy Input System (Dự phòng cho mọi môi trường Unity)
        try
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                userClicked = true;
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                userClicked = true;
            }
        }
        catch { }

        if (userClicked)
        {
            DismissTutorial();
        }
    }

    /// <summary>
    /// Bắt sự kiện Click từ uGUI EventSystem nếu có EventSystem raycast trúng
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        DismissTutorial();
    }

    /// <summary>
    /// Đảm bảo UI Tutorial luôn tồn tại và sẵn sàng hoạt động (tự tạo nếu bị thiếu trong Scene).
    /// </summary>
    public void EnsureUI()
    {
        if (tutorialOverlay == null)
        {
            GameObject existingOverlay = GameObject.Find("TutorialOverlay");
            if (existingOverlay != null)
            {
                tutorialOverlay = existingOverlay;
            }
        }

        if (tutorialOverlay == null)
        {
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogWarning("[TutorialManager] Không tìm thấy Canvas chính để tạo TutorialOverlay!");
                return;
            }

            // 1. Tạo Overlay toàn màn hình chặn click
            GameObject overlayObj = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
            overlayObj.transform.SetParent(canvas.transform, false);
            overlayObj.transform.SetAsLastSibling();

            RectTransform rt = overlayObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = overlayObj.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.82f);
            img.raycastTarget = true;

            Button overlayBtn = overlayObj.GetComponent<Button>();
            overlayBtn.targetGraphic = img;
            overlayBtn.onClick.AddListener(DismissTutorial);

            // Gắn helper click listener
            overlayObj.AddComponent<TutorialClickBridge>();

            // 2. Tạo Khung thông báo chính giữa
            GameObject boxObj = new GameObject("TutorialBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            boxObj.transform.SetParent(overlayObj.transform, false);
            RectTransform boxRt = boxObj.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.sizeDelta = new Vector2(620f, 230f);

            Image boxImg = boxObj.GetComponent<Image>();
            boxImg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);
            boxImg.raycastTarget = true;

            Button boxBtn = boxObj.GetComponent<Button>();
            boxBtn.targetGraphic = boxImg;
            boxBtn.onClick.AddListener(DismissTutorial);
            boxObj.AddComponent<TutorialClickBridge>();

            Outline boxOutline = boxObj.GetComponent<Outline>();
            boxOutline.effectColor = new Color(1f, 0.85f, 0.25f, 0.85f);
            boxOutline.effectDistance = new Vector2(2, -2);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 3. Tiêu đề Hướng Dẫn
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(boxObj.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.82f);
            titleRt.anchorMax = new Vector2(0.5f, 0.82f);
            titleRt.sizeDelta = new Vector2(580f, 40f);

            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.85f, 0.2f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.text = "📜 HƯỚNG DẪN TU TIÊN 📜";
            titleTxt.raycastTarget = false; // Không cản trở click

            // 4. Nội dung Hướng Dẫn
            GameObject textObj = new GameObject("TutorialText", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(boxObj.transform, false);
            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.5f, 0.48f);
            txtRt.anchorMax = new Vector2(0.5f, 0.48f);
            txtRt.sizeDelta = new Vector2(580f, 90f);

            Text txt = textObj.GetComponent<Text>();
            txt.font = font;
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = "";
            txt.raycastTarget = false; // Không cản trở click

            // 5. Nút bấm "Đã Hiểu"
            GameObject btnObj = new GameObject("DismissButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(boxObj.transform, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.16f);
            btnRt.anchorMax = new Vector2(0.5f, 0.16f);
            btnRt.sizeDelta = new Vector2(180f, 40f);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.65f, 0.35f, 1f);
            btnImg.raycastTarget = true;

            Button btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(DismissTutorial);
            btnObj.AddComponent<TutorialClickBridge>();

            GameObject btnTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(Text));
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
            btnTxtRt.anchorMin = Vector2.zero;
            btnTxtRt.anchorMax = Vector2.one;
            btnTxtRt.offsetMin = Vector2.zero;
            btnTxtRt.offsetMax = Vector2.zero;

            Text btnTxt = btnTxtObj.GetComponent<Text>();
            btnTxt.font = font;
            btnTxt.fontSize = 18;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = Color.white;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.text = "Đã Hiểu (Click)";
            btnTxt.raycastTarget = false; // Không cản trở click vào nút

            tutorialOverlay = overlayObj;
            tutorialText = txt;
        }

        // Đảm bảo Text được link
        if (tutorialText == null && tutorialOverlay != null)
        {
            tutorialText = tutorialOverlay.GetComponentInChildren<Text>();
        }

        // Trang bị Button & Bridge cho overlay nếu đã tồn tại sẵn từ Scene
        if (tutorialOverlay != null)
        {
            Image overlayImg = tutorialOverlay.GetComponent<Image>();
            if (overlayImg != null) overlayImg.raycastTarget = true;

            Button overlayBtn = tutorialOverlay.GetComponent<Button>();
            if (overlayBtn == null) overlayBtn = tutorialOverlay.AddComponent<Button>();
            overlayBtn.targetGraphic = overlayImg;
            overlayBtn.onClick.RemoveListener(DismissTutorial);
            overlayBtn.onClick.AddListener(DismissTutorial);

            if (tutorialOverlay.GetComponent<TutorialClickBridge>() == null)
            {
                tutorialOverlay.AddComponent<TutorialClickBridge>();
            }

            // Gắn vào cả nút con nếu có
            Button[] childButtons = tutorialOverlay.GetComponentsInChildren<Button>(true);
            foreach (var b in childButtons)
            {
                b.onClick.RemoveListener(DismissTutorial);
                b.onClick.AddListener(DismissTutorial);
                if (b.GetComponent<TutorialClickBridge>() == null)
                {
                    b.gameObject.AddComponent<TutorialClickBridge>();
                }
            }

            tutorialOverlay.SetActive(false);
        }
    }

    private Canvas FindMainCanvas()
    {
        // 1. Thử tìm GameObject "Canvas" gốc trước
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo != null)
        {
            Canvas c = canvasGo.GetComponent<Canvas>();
            if (c != null) return c;
        }

        // 2. Tìm Canvas ScreenSpace (không phải WorldSpace trên đầu quái vật)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c != null && c.renderMode != RenderMode.WorldSpace && !c.gameObject.name.Contains("HPCanvas"))
            {
                return c;
            }
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    /// <summary>
    /// Bắt đầu hoặc làm mới lại luồng hướng dẫn tân thủ.
    /// </summary>
    public void RestartTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }

        // Nếu tất cả các bước đã hoàn tất thì không cần chạy
        if (swordDone && upgradeDone && coDuyenDone && shopDone)
        {
            if (tutorialOverlay != null) tutorialOverlay.SetActive(false);
            return;
        }

        tutorialCoroutine = StartCoroutine(TutorialRoutine());
    }

    private IEnumerator TutorialRoutine()
    {
        EnsureUI();

        while (!swordDone || !upgradeDone || !coDuyenDone || !shopDone)
        {
            // 1. Chờ cho tới khi MainMenuManager ẩn đi (người chơi đã vào game thực sự)
            if (MainMenuManager.Instance != null && MainMenuManager.Instance.gameObject.activeInHierarchy)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                continue;
            }

            // 2. Chờ session active trong DataManager
            if (DataManager.Instance != null && !DataManager.Instance.isSessionActive)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                continue;
            }

            // 3. Nếu đang hiển thị 1 bảng Hướng dẫn nào đó thì chờ người chơi tắt đi
            if (tutorialOverlay != null && tutorialOverlay.activeSelf) 
            {
                yield return null;
                continue;
            }

            // 4. Bước 1: Check Kiếm (Lần đầu tiên rớt ra túi đồ)
            if (!swordDone && GetInventorySwordCount() >= 1)
            {
                swordDone = true;
                ShowTutorial("Chúc mừng nhận được Phi Kiếm mới!\nKéo thả 2 thanh kiếm trong túi đồ vào nhau\nđể Đột Phá cấp bậc! -->");
                SaveProgress();
            }
            // 5. Bước 2: Check Cơ Duyên (Kỳ ngộ bay qua)
            else if (!coDuyenDone && Object.FindObjectOfType<EncounterItem>() != null)
            {
                coDuyenDone = true;
                ShowTutorial("Kỳ ngộ (Cơ Duyên) vừa bay ngang qua màn hình!\nClick nhanh vào nó trước khi bay mất để nhận bảo vật!");
                SaveProgress();
            }
            // 6. Bước 3: Check Linh Thạch 500
            else if (!upgradeDone && EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= 500)
            {
                upgradeDone = true;
                ShowTutorial("Linh thạch đã đủ!\nHãy Nâng Cấp chỉ số để gia tăng sát thương! -->");
                SaveProgress();
            }
            // 7. Bước 4: Check Linh Thạch 2000
            else if (!shopDone && EconomyManager.Instance != null && EconomyManager.Instance.currentLinhThach >= 2000)
            {
                shopDone = true;
                ShowTutorial("Đã tích lũy nhiều Linh Thạch!\nHãy mở Tiệm Đan Dược mua đồ sinh tồn chuẩn bị cho Lôi Kiếp!");
                SaveProgress();
            }

            yield return new WaitForSecondsRealtime(0.2f);
        }

        SaveProgress();
        Debug.Log("✅ [TutorialManager] Đã hoàn thành toàn bộ Hướng Dẫn Tân Thủ cho nhân vật hiện tại!");
    }

    private int GetInventorySwordCount()
    {
        if (MergeManager.Instance == null || MergeManager.Instance.inventoryPages == null) return 0;
        int count = 0;
        foreach (Transform page in MergeManager.Instance.inventoryPages)
        {
            if (page == null) continue;
            foreach (Transform slot in page)
            {
                if (slot != null && slot.GetComponentInChildren<MergeItem>() != null) count++;
            }
        }
        return count;
    }

    private void ShowTutorial(string text)
    {
        EnsureUI();
        if (tutorialOverlay != null)
        {
            tutorialOverlay.transform.SetAsLastSibling(); // Luôn nổi lên trên cùng màn hình
            tutorialOverlay.SetActive(true);
        }
        if (tutorialText != null) tutorialText.text = text;
        
        overlayShowTime = Time.unscaledTime;
        Time.timeScale = 0f; 

        // Đảm bảo chặn click xuyên qua lớp mờ
        Image overlayImg = tutorialOverlay != null ? tutorialOverlay.GetComponent<Image>() : null;
        if (overlayImg != null) overlayImg.raycastTarget = true;
    }

    /// <summary>
    /// Đóng bảng hướng dẫn, tiếp tục thời gian game để người chơi thao tác
    /// </summary>
    public void DismissTutorial()
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
        }
        Time.timeScale = 1f; 
        Debug.Log("👉 [TutorialManager] Đã đóng Panel Hướng dẫn, người chơi bắt đầu thao tác.");
    }

    // Tương thích ngược với tên hàm cũ
    public void OnOverlayClicked()
    {
        DismissTutorial();
    }

    private void SaveProgress()
    {
        if (DataManager.Instance != null && DataManager.Instance.isSessionActive)
        {
            DataManager.Instance.SaveCurrentGame(silent: true);
        }
    }

    // ── SAVE / LOAD THEO CHARACTER SAVE DATA ────────────────────────────────
    public void SaveToSlot(CharacterSaveData slot)
    {
        slot.tutorialSwordDone = swordDone;
        slot.tutorialUpgradeDone = upgradeDone;
        slot.tutorialCoDuyenDone = coDuyenDone;
        slot.tutorialShopDone = shopDone;
        slot.tutorialCompleted = (swordDone && upgradeDone && coDuyenDone && shopDone);
    }

    public void LoadFromSlot(CharacterSaveData slot)
    {
        swordDone = slot.tutorialSwordDone;
        upgradeDone = slot.tutorialUpgradeDone;
        coDuyenDone = slot.tutorialCoDuyenDone;
        shopDone = slot.tutorialShopDone;

        if (slot.tutorialCompleted)
        {
            swordDone = true;
            upgradeDone = true;
            coDuyenDone = true;
            shopDone = true;
            if (tutorialOverlay != null) tutorialOverlay.SetActive(false);
            if (tutorialCoroutine != null)
            {
                StopCoroutine(tutorialCoroutine);
                tutorialCoroutine = null;
            }
            return;
        }

        // Nhân vật mới hoặc chưa xong hướng dẫn -> Khởi chạy Tutorial
        RestartTutorial();
    }
}

/// <summary>
/// Component cầu nối bắt sự kiện click từ EventSystem (UI Raycaster) và chuyển tiếp tới TutorialManager
/// </summary>
public class TutorialClickBridge : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.DismissTutorial();
        }
    }
}



using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel thông báo khi Tu Vi đạt Đại Viên Mãn (100% Tu Vi cần Đột Phá Đại Cảnh Giới).
/// Cho phép người chơi lựa chọn:
/// 1. XÁC NHẬN: Vào trận chiến Boss Độ Kiếp (Lôi Kiếp Hàng Lâm).
/// 2. TỪ CHỐI: Tiếp tục farm củng cố sức mạnh. Sẽ tự động hỏi lại sau mỗi khi vượt tầng!
/// </summary>
public class TribulationModalUI : MonoBehaviour
{
    private static TribulationModalUI _instance;
    public static TribulationModalUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TribulationModalUI>(true);
            }
            return _instance;
        }
    }

    [Header("Cấu trúc Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform modalBox;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Texts hiển thị")]
    public Text titleText;
    public Text subtitleText;
    public Text currentRealmText;
    public Text targetRealmText;
    public Text warningText;

    private Coroutine animRoutine;
    private bool isOpen = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }
        }
        _instance = this;

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Chỉ ẩn đi khi bắt đầu Scene nếu modal CHƯA ĐƯỢC YÊU CẦU MỞ
        // (Tránh trường hợp vừa EnsureInstanceInCanvas và gọi OpenModal thì Start() vô tình ẩn đi)
        if (Application.isPlaying && !isOpen)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Mở Panel Thông Báo Độ Kiếp
    /// </summary>
    public static void Open()
    {
        if (Instance == null)
        {
            EnsureInstanceInCanvas();
        }

        if (Instance != null)
        {
            Instance.OpenModal();
        }
    }

    /// <summary>
    /// Đóng Panel Thông Báo
    /// </summary>
    public static void Close()
    {
        if (Instance != null)
        {
            Instance.CloseModal();
        }
    }

    public void OpenModal()
    {
        // Không mở modal nếu đang trong trận chiến Boss Độ Kiếp
        if (CombatManager.Instance != null && CombatManager.Instance.isTribulationBossActive) return;

        isOpen = true;

        // Đảm bảo Canvas cha luôn active, không bị tắt nhầm
        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        if (parentCanvas == null) parentCanvas = FindObjectOfType<Canvas>(true);
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
        {
            parentCanvas.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        // Đảm bảo CanvasGroup hiển thị ngay lập tức 100%, không bị tàng hình
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;
        if (modalBox != null)
        {
            modalBox.anchoredPosition = Vector2.zero;
            modalBox.localScale = Vector3.one;
        }

        RefreshData();

        // Đóng băng toàn bộ thời gian của game theo yêu cầu để người chơi quyết định
        Time.timeScale = 0f;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen());
    }

    public void CloseModal()
    {
        isOpen = false;

        // Khôi phục lại tốc độ thời gian trò chơi bình thường
        Time.timeScale = 1f;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateClose());
    }

    private void RefreshData()
    {
        string curRealm = "Luyện Khí Tầng 15";
        string nxtRealm = "Trúc Cơ Sơ Kỳ";

        if (CultivationManager.Instance != null)
        {
            var curData = CultivationManager.Instance.GetCurrentStageData();
            if (curData != null) curRealm = curData.stageName;
            nxtRealm = CultivationManager.Instance.GetNextRealmName();
        }

        if (titleText != null) titleText.text = "⚡ THIÊN KIẾP HÀNG LÂM ⚡";
        if (subtitleText != null) subtitleText.text = "Tu vi đã đạt Đại Viên Mãn — Bạn có muốn Độ Kiếp Thăng Cấp?";
        if (currentRealmText != null) currentRealmText.text = $"• Cảnh giới hiện tại: <color=#FFE666>{curRealm}</color> (Đại Viên Mãn)";
        if (targetRealmText != null) targetRealmText.text = $"• Đột phá tấn thăng: <color=#66FFB2>{nxtRealm}</color>";
        if (warningText != null)
        {
            warningText.text = "⚔️ <color=#FFD700>Thành công:</color> Tấn thăng đại cảnh giới, mở khóa sức mạnh mới!\n" +
                               "⚠️ <color=#FF5555>Thất bại:</color> Bị Thiên Kiếp phản phệ, <color=#FF3333><b>tổn thất 25% Tu Vi</b></color>!\n" +
                               "💡 <i>Nếu chưa tự tin, hãy chọn 'Củng Cố' để nâng cấp phi kiếm. Hệ thống sẽ hỏi lại sau mỗi khi vượt tầng.</i>";
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (modalBox != null)
        {
            modalBox.anchoredPosition = Vector2.zero;
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 initialScale = Vector3.one * 0.9f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                modalBox.localScale = Vector3.Lerp(initialScale, Vector3.one, t);
                yield return null;
            }
            modalBox.localScale = Vector3.one;
        }

        animRoutine = null;
    }

    private IEnumerator AnimateClose()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            if (modalBox != null)
            {
                modalBox.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, t);
            }
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        animRoutine = null;
    }

    public void OnConfirmClicked()
    {
        CloseModal();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }

        // Bắt đầu trận chiến Boss Độ Kiếp
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartTribulationBossBattle();
        }
    }

    public void OnCancelClicked()
    {
        CloseModal();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }

        if (GameLogger.Instance != null)
        {
            GameLogger.Instance.Log("🛡️ Đạo hữu quyết định tạm hoãn độ kiếp để củng cố sức mạnh! Sẽ nhắc lại sau khi vượt tầng.", Color.cyan);
        }
    }

    /// <summary>
    /// Đảm bảo Panel tồn tại trong Canvas nếu chưa được tạo trên Scene View.
    /// Tự động thiết lập đầy đủ Button Listener, Fallback Font, và Layout chuẩn mực.
    /// </summary>
    private static void EnsureInstanceInCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>(true);
        if (canvas == null)
        {
            GameObject cObj = GameObject.Find("Canvas");
            if (cObj != null) canvas = cObj.GetComponent<Canvas>();
        }
        if (canvas == null)
        {
            Debug.LogError("❌ [TribulationModalUI] Không tìm thấy Canvas nào trong Scene!");
            return;
        }

        if (!canvas.gameObject.activeSelf) canvas.gameObject.SetActive(true);

        // Fallback font đa tầng
        Font fontToUse = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fontToUse == null) fontToUse = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (fontToUse == null) fontToUse = Font.CreateDynamicFontFromOSFont("Arial", 16);

        GameObject modalObj = new GameObject("TribulationModalUI", typeof(RectTransform), typeof(CanvasGroup), typeof(TribulationModalUI));
        modalObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = modalObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Background overlay
        Image overlay = modalObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.75f);

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Modal Box
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(modalObj.transform, false);
        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(560, 420);
        boxObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 0.98f);
        Outline boxOutline = boxObj.GetComponent<Outline>();
        boxOutline.effectColor = new Color(1f, 0.8f, 0.2f, 0.9f);
        boxOutline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(boxObj.transform, false);
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = fontToUse;
        titleTxt.text = "⚡ THIÊN KIẾP HÀNG LÂM ⚡";
        titleTxt.fontSize = 26;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(1f, 0.85f, 0.2f);
        titleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 150);
        titleRt.sizeDelta = new Vector2(520, 50);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
        subObj.transform.SetParent(boxObj.transform, false);
        Text subTxt = subObj.GetComponent<Text>();
        subTxt.font = fontToUse;
        subTxt.text = "Tu vi đã đạt Đại Viên Mãn — Bạn có muốn Độ Kiếp Thăng Cấp?";
        subTxt.fontSize = 17;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.6f, 1f, 0.8f);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0, 110);
        subRt.sizeDelta = new Vector2(520, 35);

        // Content / Warning Box
        GameObject contentObj = new GameObject("WarningText", typeof(RectTransform), typeof(Text));
        contentObj.transform.SetParent(boxObj.transform, false);
        Text warnTxt = contentObj.GetComponent<Text>();
        warnTxt.font = fontToUse;
        warnTxt.fontSize = 16;
        warnTxt.alignment = TextAnchor.UpperLeft;
        warnTxt.color = Color.white;
        warnTxt.lineSpacing = 1.3f;
        RectTransform warnRt = contentObj.GetComponent<RectTransform>();
        warnRt.anchoredPosition = new Vector2(0, 5);
        warnRt.sizeDelta = new Vector2(490, 150);

        // Button Confirm (Độ Kiếp Ngay)
        GameObject btnConfObj = new GameObject("BtnConfirm", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnConfObj.transform.SetParent(boxObj.transform, false);
        btnConfObj.GetComponent<Image>().color = new Color(0.9f, 0.65f, 0.1f); // Vàng kim
        btnConfObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnConfRt = btnConfObj.GetComponent<RectTransform>();
        btnConfRt.anchoredPosition = new Vector2(130, -145);
        btnConfRt.sizeDelta = new Vector2(220, 55);

        GameObject btnConfTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnConfTxtObj.transform.SetParent(btnConfObj.transform, false);
        Text btnConfTxt = btnConfTxtObj.GetComponent<Text>();
        btnConfTxt.font = fontToUse;
        btnConfTxt.text = "⚔️ ĐỘ KIẾP NGAY";
        btnConfTxt.fontSize = 18;
        btnConfTxt.fontStyle = FontStyle.Bold;
        btnConfTxt.alignment = TextAnchor.MiddleCenter;
        btnConfTxt.color = Color.black;
        RectTransform bctRt = btnConfTxtObj.GetComponent<RectTransform>();
        bctRt.anchorMin = Vector2.zero; bctRt.anchorMax = Vector2.one;
        bctRt.offsetMin = Vector2.zero; bctRt.offsetMax = Vector2.zero;

        // Button Cancel (Củng Cố)
        GameObject btnCancObj = new GameObject("BtnCancel", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnCancObj.transform.SetParent(boxObj.transform, false);
        btnCancObj.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f); // Xám xanh
        btnCancObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnCancRt = btnCancObj.GetComponent<RectTransform>();
        btnCancRt.anchoredPosition = new Vector2(-130, -145);
        btnCancRt.sizeDelta = new Vector2(220, 55);

        GameObject btnCancTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnCancTxtObj.transform.SetParent(btnCancObj.transform, false);
        Text btnCancTxt = btnCancTxtObj.GetComponent<Text>();
        btnCancTxt.font = fontToUse;
        btnCancTxt.text = "🛡️ CỦNG CỐ SỨC MẠNH";
        btnCancTxt.fontSize = 17;
        btnCancTxt.fontStyle = FontStyle.Bold;
        btnCancTxt.alignment = TextAnchor.MiddleCenter;
        btnCancTxt.color = new Color(0.85f, 0.9f, 0.95f);
        RectTransform bcancRt = btnCancTxtObj.GetComponent<RectTransform>();
        bcancRt.anchorMin = Vector2.zero; bcancRt.anchorMax = Vector2.one;
        bcancRt.offsetMin = Vector2.zero; bcancRt.offsetMax = Vector2.zero;

        // Wire references and listeners
        TribulationModalUI modal = modalObj.GetComponent<TribulationModalUI>();
        modal.canvasGroup = cg;
        modal.modalBox = boxRt;
        modal.titleText = titleTxt;
        modal.subtitleText = subTxt;
        modal.warningText = warnTxt;

        Button btnConf = btnConfObj.GetComponent<Button>();
        Button btnCanc = btnCancObj.GetComponent<Button>();
        modal.confirmButton = btnConf;
        modal.cancelButton = btnCanc;

        btnConf.onClick.RemoveAllListeners();
        btnConf.onClick.AddListener(modal.OnConfirmClicked);
        btnCanc.onClick.RemoveAllListeners();
        btnCanc.onClick.AddListener(modal.OnCancelClicked);

        modal.isOpen = false;
        _instance = modal;
    }
}

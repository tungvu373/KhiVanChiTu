using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>SDK quảng cáo đăng ký provider khi khởi tạo; callback chỉ thành công sau khi nhận reward.</summary>
public interface IRewardedAdProvider
{
    bool IsReady { get; }
    void ShowOfflineReward(Action<bool> onRewarded);
}

public static class RewardedAdBridge
{
    public static IRewardedAdProvider Provider { get; set; }
    public static bool IsReady => Provider != null && Provider.IsReady;
}

/// <summary>
/// Giao diện thông báo nhận thưởng Treo Máy / Bế Quan Tu Luyện (Offline Earnings).
/// Tự động hiển thị khi người chơi đăng nhập trở lại sau một khoảng thời gian vắng mặt.
/// Hỗ trợ nút Nhận Thường (x1) và Xem Quảng Cáo (x2).
/// </summary>
public class OfflineRewardPopupUI : MonoBehaviour
{
    private static OfflineRewardPopupUI _instance;
    public static OfflineRewardPopupUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OfflineRewardPopupUI>(true);
            }
            return _instance;
        }
    }

    [Header("Cấu trúc Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform modalBox;
    public Button claimNormalButton;
    public Button claimDoubleButton;
    public Button closeButton;

    [Header("Texts hiển thị")]
    public Text titleText;
    public Text timeAwayText;
    public Text rewardDetailsText;

    private Coroutine animRoutine;
    private bool isOpen = false;
    private bool adInProgress;
    internal bool IsModalOpen => isOpen && gameObject.activeInHierarchy;

    internal void DismissFromBack()
    {
        if (!adInProgress) CloseModal();
    }

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

        if (claimNormalButton != null)
        {
            claimNormalButton.onClick.RemoveAllListeners();
            claimNormalButton.onClick.AddListener(OnClaimNormalClicked);
        }
        if (claimDoubleButton != null)
        {
            claimDoubleButton.onClick.RemoveAllListeners();
            claimDoubleButton.onClick.AddListener(OnClaimDoubleClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
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
    /// Hiển thị Popup thưởng Bế quan. Tự động khởi tạo giao diện nếu chưa có trên Canvas.
    /// </summary>
    public static void Show(OfflineEarningsManager.OfflineRewardData reward)
    {
        if (Instance == null)
        {
            EnsureInstanceInCanvas();
        }

        if (Instance != null)
        {
            Instance.OpenModal(reward);
        }
    }

    public void OpenModal(OfflineEarningsManager.OfflineRewardData reward)
    {
        Canvas parentCanvas = FindGameplayCanvas();
        if (parentCanvas == null) return;
        if (transform != parentCanvas.transform &&
            (transform.parent == null || GetComponentInParent<Canvas>(true)?.rootCanvas != parentCanvas || !transform.parent.gameObject.activeInHierarchy))
            transform.SetParent(parentCanvas.transform, false);
        if (!parentCanvas.gameObject.activeSelf) parentCanvas.gameObject.SetActive(true);

        isOpen = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

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

        PopulateData(reward);

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen());
    }

    public void CloseModal()
    {
        isOpen = false;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateClose());
    }

    private void PopulateData(OfflineEarningsManager.OfflineRewardData reward)
    {
        if (titleText != null)
        {
            titleText.text = "BẾ QUAN TU LUYỆN";
        }

        TimeSpan span = TimeSpan.FromSeconds(reward.offlineSeconds);
        string timeStr;
        if (span.TotalHours >= 1)
            timeStr = $"{(int)span.TotalHours} giờ {span.Minutes} phút";
        else
            timeStr = $"{span.Minutes} phút {span.Seconds} giây";

        if (reward.offlineSeconds > OfflineEarningsManager.MAX_OFFLINE_SECONDS)
        {
            timeStr += " <color=#FF8844>(Tối đa 8 giờ)</color>";
        }

        if (timeAwayText != null)
        {
            timeAwayText.text = $"Đạo hữu đã bế quan tĩnh tọa: <color=#66FFB2>{timeStr}</color>";
        }

        if (rewardDetailsText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b><color=#FFE666>Thu Hoạch Khi Bế Quan:</color></b>");
            sb.AppendLine($"  Linh Thạch: <color=#FFE666>+{EconomyManager.FormatNumber(reward.linhThach)}</color>");
            sb.AppendLine($"  Tu Vi: <color=#66FF99>+{reward.tuVi:F0}</color>");

            if (reward.linhChi > 0)
                sb.AppendLine($"  Linh Chi: <color=#66FFB2>+{reward.linhChi}</color>");
            if (reward.ngungKhi > 0)
                sb.AppendLine($"  Cỏ Ngưng Khí: <color=#66FFB2>+{reward.ngungKhi}</color>");
            if (reward.tuyetLien > 0)
                sb.AppendLine($"  Tuyết Liên: <color=#99E6FF>+{reward.tuyetLien}</color>");
            if (reward.ore > 0)
                sb.AppendLine($"  Quặng Phôi: <color=#E0A868>+{reward.ore}</color>");

            rewardDetailsText.text = sb.ToString().TrimEnd();
        }
        if (claimDoubleButton != null)
        {
            claimDoubleButton.interactable = RewardedAdBridge.IsReady;
            Text label = claimDoubleButton.GetComponentInChildren<Text>();
            if (label != null) label.text = RewardedAdBridge.IsReady ? "XEM QC (x2)" : "QC chưa sẵn sàng";
        }
    }

    public void OnClaimNormalClicked()
    {
        if (adInProgress) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        OfflineEarningsManager.Instance?.ClaimReward(1.0f);
        CloseModal();
    }

    public void OnClaimDoubleClicked()
    {
        if (adInProgress || !RewardedAdBridge.IsReady) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
        adInProgress = true;
        if (claimNormalButton != null) claimNormalButton.interactable = false;
        if (claimDoubleButton != null) claimDoubleButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;
        try
        {
            RewardedAdBridge.Provider.ShowOfflineReward(rewarded =>
            {
                if (!adInProgress) return; // SDK có thể gọi callback lặp.
                adInProgress = false;
                if (rewarded && OfflineEarningsManager.Instance != null && OfflineEarningsManager.Instance.HasPendingReward)
                {
                    OfflineEarningsManager.Instance.ClaimReward(2.0f);
                    CloseModal();
                }
                else
                {
                    if (claimNormalButton != null) claimNormalButton.interactable = true;
                    if (claimDoubleButton != null) claimDoubleButton.interactable = RewardedAdBridge.IsReady;
                }
                if (closeButton != null) closeButton.interactable = true;
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[OfflineRewardPopupUI] Không mở được quảng cáo: {ex.Message}");
            adInProgress = false;
            if (claimNormalButton != null) claimNormalButton.interactable = true;
            if (claimDoubleButton != null) claimDoubleButton.interactable = RewardedAdBridge.IsReady;
            if (closeButton != null) closeButton.interactable = true;
        }
    }

    public void OnCloseClicked()
    {
        // Khi bấm nút đóng, mặc định nhận phần thưởng x1 để không làm mất tài nguyên của người chơi
        OnClaimNormalClicked();
    }

    private IEnumerator AnimateOpen()
    {
        if(MobileUIAnimationDirector.For(this)!=null)
        {
            if(canvasGroup==null)canvasGroup=GetComponent<CanvasGroup>();
            if(modalBox==null)modalBox=transform.Find("ModalBox") as RectTransform;
            yield return MobileUIModalMotion.Animate(this,canvasGroup,modalBox,true,reward:true);
            animRoutine=null;yield break;
        }
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one * 0.9f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (modalBox != null) modalBox.localScale = Vector3.Lerp(initialScale, Vector3.one, t);
            yield return null;
        }
        if (modalBox != null) modalBox.localScale = Vector3.one;
        animRoutine = null;
    }

    private IEnumerator AnimateClose()
    {
        if(MobileUIAnimationDirector.For(this)!=null)
        {
            yield return MobileUIModalMotion.Animate(this,canvasGroup,modalBox,false);
            animRoutine=null;yield break;
        }
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
            if (modalBox != null) modalBox.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, t);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        animRoutine = null;
    }

    private static Canvas FindGameplayCanvas()
    {
        Canvas fallback = null;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (!root.TryGetComponent<Canvas>(out var canvas) || canvas.renderMode == RenderMode.WorldSpace) continue;
            if (root.name == "Canvas") return canvas;
            if (fallback == null) fallback = canvas;
        }
        return fallback;
    }

    /// <summary>
    /// Đảm bảo Panel tồn tại trong Canvas nếu chưa được tạo thủ công.
    /// Tự động thiết lập đầy đủ Button, Layout, Font cho Mobile Portrait.
    /// </summary>
    public static void EnsureInstanceInCanvas()
    {
        Canvas canvas = FindGameplayCanvas();
        if (canvas == null)
        {
            Debug.LogError("❌ [OfflineRewardPopupUI] Không tìm thấy Canvas trong Scene!");
            return;
        }

        if (!canvas.gameObject.activeSelf) canvas.gameObject.SetActive(true);

        Font fontToUse = UiTextStyle.BodyFont;
        if (fontToUse == null) fontToUse = Font.CreateDynamicFontFromOSFont("Arial", 16);

        GameObject modalObj = new GameObject("OfflineRewardPopupUI", typeof(RectTransform), typeof(CanvasGroup), typeof(OfflineRewardPopupUI));
        modalObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = modalObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Background dark overlay
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
        boxRt.sizeDelta = new Vector2(620, 520);
        boxObj.GetComponent<Image>().color = new Color(0.07f, 0.10f, 0.16f, 0.98f);
        Outline boxOutline = boxObj.GetComponent<Outline>();
        boxOutline.effectColor = new Color(0.2f, 0.8f, 0.6f, 0.9f);
        boxOutline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(boxObj.transform, false);
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = fontToUse;
        titleTxt.text = "BẾ QUAN TU LUYỆN";
        titleTxt.fontSize = 26;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(1f, 0.85f, 0.2f);
        titleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 210);
        titleRt.sizeDelta = new Vector2(580, 50);

        // Time Away Subtitle
        GameObject timeObj = new GameObject("TimeAway", typeof(RectTransform), typeof(Text));
        timeObj.transform.SetParent(boxObj.transform, false);
        Text timeTxt = timeObj.GetComponent<Text>();
        timeTxt.font = fontToUse;
        timeTxt.text = "Đạo hữu đã bế quan tĩnh tọa: 0 phút";
        timeTxt.fontSize = 17;
        timeTxt.alignment = TextAnchor.MiddleCenter;
        timeTxt.color = new Color(0.7f, 0.9f, 1f);
        RectTransform timeRt = timeObj.GetComponent<RectTransform>();
        timeRt.anchoredPosition = new Vector2(0, 165);
        timeRt.sizeDelta = new Vector2(580, 35);

        // Reward Details Box
        GameObject detailsObj = new GameObject("RewardDetails", typeof(RectTransform), typeof(Image));
        detailsObj.transform.SetParent(boxObj.transform, false);
        detailsObj.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.10f, 0.95f);
        RectTransform detailsRt = detailsObj.GetComponent<RectTransform>();
        detailsRt.anchoredPosition = new Vector2(0, 30);
        detailsRt.sizeDelta = new Vector2(560, 210);

        GameObject detailsTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        detailsTxtObj.transform.SetParent(detailsObj.transform, false);
        Text detailsTxt = detailsTxtObj.GetComponent<Text>();
        detailsTxt.font = fontToUse;
        detailsTxt.fontSize = 16;
        detailsTxt.alignment = TextAnchor.UpperLeft;
        detailsTxt.color = Color.white;
        detailsTxt.lineSpacing = 1.25f;
        RectTransform dtRt = detailsTxtObj.GetComponent<RectTransform>();
        dtRt.anchorMin = Vector2.zero; dtRt.anchorMax = Vector2.one;
        dtRt.offsetMin = new Vector2(16, 12); dtRt.offsetMax = new Vector2(-16, -12);

        // Button Claim Normal (Nhận Thường)
        GameObject btnNormalObj = new GameObject("BtnClaimNormal", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnNormalObj.transform.SetParent(boxObj.transform, false);
        btnNormalObj.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.45f);
        btnNormalObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnNormRt = btnNormalObj.GetComponent<RectTransform>();
        btnNormRt.anchoredPosition = new Vector2(-140, -185);
        btnNormRt.sizeDelta = new Vector2(240, 60);

        GameObject btnNormTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnNormTxtObj.transform.SetParent(btnNormalObj.transform, false);
        Text btnNormTxt = btnNormTxtObj.GetComponent<Text>();
        btnNormTxt.font = fontToUse;
        btnNormTxt.text = "THU NHẬN (x1)";
        btnNormTxt.fontSize = 17;
        btnNormTxt.fontStyle = FontStyle.Bold;
        btnNormTxt.alignment = TextAnchor.MiddleCenter;
        btnNormTxt.color = Color.white;
        RectTransform bntRt = btnNormTxtObj.GetComponent<RectTransform>();
        bntRt.anchorMin = Vector2.zero; bntRt.anchorMax = Vector2.one;
        bntRt.offsetMin = Vector2.zero; bntRt.offsetMax = Vector2.zero;

        // Button Claim Double (Xem Quảng Cáo x2)
        GameObject btnDoubleObj = new GameObject("BtnClaimDouble", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnDoubleObj.transform.SetParent(boxObj.transform, false);
        btnDoubleObj.GetComponent<Image>().color = new Color(0.9f, 0.65f, 0.1f);
        btnDoubleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnDblRt = btnDoubleObj.GetComponent<RectTransform>();
        btnDblRt.anchoredPosition = new Vector2(140, -185);
        btnDblRt.sizeDelta = new Vector2(240, 60);

        GameObject btnDblTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnDblTxtObj.transform.SetParent(btnDoubleObj.transform, false);
        Text btnDblTxt = btnDblTxtObj.GetComponent<Text>();
        btnDblTxt.font = fontToUse;
        btnDblTxt.text = "XEM QC (x2)";
        btnDblTxt.fontSize = 18;
        btnDblTxt.fontStyle = FontStyle.Bold;
        btnDblTxt.alignment = TextAnchor.MiddleCenter;
        btnDblTxt.color = Color.black;
        RectTransform bdtRt = btnDblTxtObj.GetComponent<RectTransform>();
        bdtRt.anchorMin = Vector2.zero; bdtRt.anchorMax = Vector2.one;
        bdtRt.offsetMin = Vector2.zero; bdtRt.offsetMax = Vector2.zero;

        // Button Close (×)
        GameObject btnCloseObj = new GameObject("BtnClose", typeof(RectTransform), typeof(Image), typeof(Button));
        btnCloseObj.transform.SetParent(boxObj.transform, false);
        btnCloseObj.GetComponent<Image>().color = new Color(0.35f, 0.15f, 0.15f, 0.9f);
        RectTransform btnCloseRt = btnCloseObj.GetComponent<RectTransform>();
        btnCloseRt.anchorMin = new Vector2(1f, 1f);
        btnCloseRt.anchorMax = new Vector2(1f, 1f);
        btnCloseRt.pivot = new Vector2(1f, 1f);
        btnCloseRt.anchoredPosition = new Vector2(-12, -12);
        btnCloseRt.sizeDelta = new Vector2(46, 46);

        GameObject btnCloseTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnCloseTxtObj.transform.SetParent(btnCloseObj.transform, false);
        Text btnCloseTxt = btnCloseTxtObj.GetComponent<Text>();
        btnCloseTxt.font = fontToUse;
        btnCloseTxt.text = "×";
        btnCloseTxt.fontSize = 28;
        btnCloseTxt.fontStyle = FontStyle.Bold;
        btnCloseTxt.alignment = TextAnchor.MiddleCenter;
        btnCloseTxt.color = Color.white;
        RectTransform bctRt = btnCloseTxtObj.GetComponent<RectTransform>();
        bctRt.anchorMin = Vector2.zero; bctRt.anchorMax = Vector2.one;
        bctRt.offsetMin = Vector2.zero; bctRt.offsetMax = Vector2.zero;

        // Setup component references
        OfflineRewardPopupUI popup = modalObj.GetComponent<OfflineRewardPopupUI>();
        popup.canvasGroup = cg;
        popup.modalBox = boxRt;
        popup.titleText = titleTxt;
        popup.timeAwayText = timeTxt;
        popup.rewardDetailsText = detailsTxt;

        Button bNorm = btnNormalObj.GetComponent<Button>();
        Button bDbl = btnDoubleObj.GetComponent<Button>();
        Button bClose = btnCloseObj.GetComponent<Button>();

        popup.claimNormalButton = bNorm;
        popup.claimDoubleButton = bDbl;
        popup.closeButton = bClose;

        bNorm.onClick.RemoveAllListeners();
        bNorm.onClick.AddListener(popup.OnClaimNormalClicked);
        bDbl.onClick.RemoveAllListeners();
        bDbl.onClick.AddListener(popup.OnClaimDoubleClicked);
        bClose.onClick.RemoveAllListeners();
        bClose.onClick.AddListener(popup.OnCloseClicked);

        popup.isOpen = false;
        _instance = popup;
    }
}


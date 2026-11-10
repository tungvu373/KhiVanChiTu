using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PANEL CẢNH BÁO XÁC NHẬN PHÂN GIẢI VẬT PHẨM GIÁ TRỊ CAO (DISMANTLE CONFIRMATION MODAL).
/// Bảo vệ tuyệt đối công sức cày cuốc của người chơi:
/// Chỉ hiển thị khi phân giải Kiếm cấp >= 3, Kiếm nhiều phôi, hoặc Đan Dược Đột Phá / Cực Phẩm.
/// </summary>
public class DismantleConfirmModalUI : MonoBehaviour
{
    private static DismantleConfirmModalUI _instance;
    public static DismantleConfirmModalUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DismantleConfirmModalUI>(true);
            }
            return _instance;
        }
    }

    public bool IsModalOpen => isOpen;

    [Header("Cấu trúc Panel")]
    public CanvasGroup canvasGroup;
    public RectTransform modalBox;
    public Button btnCancel;
    public Button btnConfirm;
    public Button btnBackdrop;

    [Header("Nội Dung Hiển Thị")]
    public Text txtTitle;
    public Text txtItemName;
    public Text txtItemDetail;
    public Text txtRewardSummary;
    public Text txtWarning;

    private Action onConfirmedAction;
    private Coroutine animRoutine;
    private bool isOpen = false;
    private Font cachedFont;

    private Font GetFont()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = UiTextStyle.BodyFont;
        return cachedFont;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        cachedFont = GetFont();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (modalBox == null) modalBox = transform.Find("ModalBox") as RectTransform;

        EnsureCanvasSorting();
        WireEvents();
    }

    private void EnsureCanvasSorting()
    {
        transform.SetAsLastSibling();
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 900;
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void WireEvents()
    {
        if (btnCancel != null)
        {
            btnCancel.onClick.RemoveAllListeners();
            btnCancel.onClick.AddListener(Close);
        }
        if (btnBackdrop != null)
        {
            btnBackdrop.onClick.RemoveAllListeners();
            btnBackdrop.onClick.AddListener(Close);
        }
        if (btnConfirm != null)
        {
            btnConfirm.onClick.RemoveAllListeners();
            btnConfirm.onClick.AddListener(OnConfirmClicked);
        }
    }

    public static void OpenConfirm(string itemName, string itemDetail, DismantleReward reward, Action onConfirm)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[DismantleConfirmModalUI] Không tìm thấy Instance trong Scene!");
            onConfirm?.Invoke(); // Failsafe
            return;
        }
        Instance.Show(itemName, itemDetail, reward, onConfirm);
    }

    public void Show(string itemName, string itemDetail, DismantleReward reward, Action onConfirm)
    {
        EnsureCanvasSorting();
        onConfirmedAction = onConfirm;

        if (txtTitle != null) txtTitle.text = "XÁC NHẬN PHÂN GIẢI BẢO VẬT";
        if (txtItemName != null) txtItemName.text = itemName;
        if (txtItemDetail != null) txtItemDetail.text = itemDetail;
        if (txtRewardSummary != null)
        {
            txtRewardSummary.text = "Tài nguyên sẽ thu hồi:\n" + (reward != null ? reward.GetFormattedSummary() : "Không có");
        }
        if (txtWarning != null)
        {
            txtWarning.text = "Đây là vật phẩm quý giá được rèn luyện kỳ công!\nSau khi tách sẽ bị tiêu biến vĩnh viễn và không thể hoàn tác!";
        }

        gameObject.SetActive(true);
        isOpen = true;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen(true));

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private void OnConfirmClicked()
    {
        var action = onConfirmedAction;
        Close();
        action?.Invoke();
    }

    public void Close()
    {
        if (!isOpen && !gameObject.activeInHierarchy) return;
        isOpen = false;
        onConfirmedAction = null;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateOpen(false));

        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
    }

    private IEnumerator AnimateOpen(bool open)
    {
        float elapsed = 0f;
        float duration = 0.20f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : (open ? 0f : 1f);
        float endAlpha = open ? 1f : 0f;

        Vector3 startScale = open ? new Vector3(0.85f, 0.85f, 1f) : Vector3.one;
        Vector3 endScale = open ? Vector3.one : new Vector3(0.85f, 0.85f, 1f);

        if (modalBox != null) modalBox.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smooth);
            if (modalBox != null) modalBox.localScale = Vector3.Lerp(startScale, endScale, smooth);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = endAlpha;
        if (modalBox != null) modalBox.localScale = endScale;

        if (!open)
        {
            gameObject.SetActive(false);
        }
    }
}


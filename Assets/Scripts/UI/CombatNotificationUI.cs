using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị thông báo uy nghiêm ở chính giữa màn hình Combat 3D (nửa trái màn hình)
/// khi Đột phá Cảnh Giới thành công hoặc Nhân vật Tử Trận.
/// Được tạo sẵn trên Canvas để Designer/User tự do kéo thả, căn chỉnh tọa độ và kích thước trong Scene View.
/// </summary>
[ExecuteAlways]
public class CombatNotificationUI : MonoBehaviour
{
    private static CombatNotificationUI _instance;
    public static CombatNotificationUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CombatNotificationUI>();
            }
            return _instance;
        }
    }

    [Header("UI References (Được gắn sẵn)")]
    public CanvasGroup canvasGroup;
    public RectTransform containerRect;
    public Image backgroundImage;
    public Image borderTop;
    public Image borderBottom;
    public Text titleText;
    public Text subtitleText;
    public Text extraDetailText;

    private Coroutine activeAnimRoutine;
    private Vector2 userConfiguredPosition;
    private Vector3 userConfiguredScale = Vector3.one;

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
    }

    private void Start()
    {
        if (containerRect == null) containerRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (containerRect != null)
        {
            userConfiguredPosition = containerRect.anchoredPosition;
            userConfiguredScale = containerRect.localScale;
        }

        if (Application.isPlaying)
        {
            // Khi nhấn Play: Ẩn banner đi, chờ đến khi có sự kiện Đột phá hoặc Tử trận mới hiện lên
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
        else
        {
            // Trong chế độ Edit (Scene View): Luôn hiện rõ ràng với Alpha = 1 để User dễ quan sát & căn chỉnh
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// Hiển thị thông báo Đột Phá Cảnh Giới thành công
    /// </summary>
    public static void ShowBreakthrough(string realmName)
    {
        if (string.IsNullOrEmpty(realmName)) return;

        // Tầng 1 lúc mới bắt đầu chơi không hiển thị thông báo đột phá
        if (realmName.Contains("Tầng 1") && realmName.Contains("Luyện Khí")) return;

        if (Instance == null)
        {
            Debug.LogWarning("[CombatNotificationUI] Không tìm thấy Banner trong Scene!");
            return;
        }
        Instance.DisplayBreakthrough(realmName);
    }

    /// <summary>
    /// Hiển thị thông báo Nhân vật Tử Trận
    /// </summary>
    public static void ShowDeath()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[CombatNotificationUI] Không tìm thấy Banner trong Scene!");
            return;
        }
        Instance.DisplayDeath();
    }

    public void DisplayBreakthrough(string realmName)
    {
        if (titleText != null)
        {
            titleText.text = "✨ ĐỘT PHÁ THÀNH CÔNG ✨";
            titleText.color = new Color(1f, 0.92f, 0.25f); // Vàng kim hào quang
        }

        if (subtitleText != null)
        {
            subtitleText.text = $"[ {realmName} ]";
            subtitleText.color = new Color(0.4f, 1f, 0.7f); // Ngọc bích tinh thuần
        }

        if (extraDetailText != null)
        {
            extraDetailText.text = "Khí tức ngập tràn • Thọ nguyên gia tăng • Sinh lực hồi phục";
            extraDetailText.color = new Color(0.9f, 0.95f, 0.8f, 0.95f);
        }

        // Đổi màu viền và nền sang tông hoàng kim
        Color goldBorder = new Color(1f, 0.84f, 0f, 0.95f);
        if (borderTop != null) borderTop.color = goldBorder;
        if (borderBottom != null) borderBottom.color = goldBorder;
        if (backgroundImage != null) backgroundImage.color = new Color(0.05f, 0.1f, 0.15f, 0.92f);

        // Hiệu ứng Rung màn hình nhẹ chúc mừng
        if (CameraShake.Instance != null && Application.isPlaying)
        {
            CameraShake.Instance.TriggerShake(0.5f, 0.35f);
        }

        PlayBannerAnimation(duration: 2.8f);
    }

    public void DisplayDeath()
    {
        if (titleText != null)
        {
            titleText.text = "💀 ĐẠO THÂN TỬ TRẬN 💀";
            titleText.color = new Color(1f, 0.25f, 0.25f); // Đỏ thẫm máu lửa
        }

        if (subtitleText != null)
        {
            subtitleText.text = "Bất Hạnh Vẫn Lạc — Đạo Tâm Bất Diệt";
            subtitleText.color = new Color(1f, 0.75f, 0.75f);
        }

        if (extraDetailText != null)
        {
            extraDetailText.text = "Làm mới tiến trình ải • Đang trùng tu hồi sinh...";
            extraDetailText.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        }

        // Đổi màu viền và nền sang tông đỏ đậm máu
        Color bloodBorder = new Color(0.9f, 0.15f, 0.15f, 0.95f);
        if (borderTop != null) borderTop.color = bloodBorder;
        if (borderBottom != null) borderBottom.color = bloodBorder;
        if (backgroundImage != null) backgroundImage.color = new Color(0.15f, 0.03f, 0.03f, 0.94f);

        // Rung nhẹ khi ngã xuống
        if (CameraShake.Instance != null && Application.isPlaying)
        {
            CameraShake.Instance.TriggerShake(0.4f, 0.3f);
        }

        PlayBannerAnimation(duration: 2.2f);
    }

    private void PlayBannerAnimation(float duration)
    {
        if (!Application.isPlaying)
        {
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            return;
        }

        if (activeAnimRoutine != null)
        {
            StopCoroutine(activeAnimRoutine);
        }
        activeAnimRoutine = StartCoroutine(AnimateBannerCoroutine(duration));
    }

    private IEnumerator AnimateBannerCoroutine(float duration)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (containerRect == null) containerRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // Giữ vị trí mà người dùng đã căn chỉnh trên Editor
        Vector2 basePosition = userConfiguredPosition;
        if (basePosition == Vector2.zero && containerRect != null)
        {
            basePosition = containerRect.anchoredPosition;
        }
        Vector3 baseScale = userConfiguredScale;

        // Giai đoạn 1: Pop In (Scale từ 0.6 nảy lên 1.12 rồi về 1.0, Alpha từ 0 lên 1) trong 0.25 giây
        float popTime = 0.25f;
        float elapsed = 0f;
        while (elapsed < popTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popTime);

            float scaleProgress;
            if (t < 0.7f)
            {
                scaleProgress = Mathf.Lerp(0.6f, 1.12f, t / 0.7f);
            }
            else
            {
                scaleProgress = Mathf.Lerp(1.12f, 1.0f, (t - 0.7f) / 0.3f);
            }

            if (containerRect != null)
            {
                containerRect.localScale = baseScale * scaleProgress;
                containerRect.anchoredPosition = basePosition;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t * 1.5f);
            }
            yield return null;
        }

        if (containerRect != null) containerRect.localScale = baseScale;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Giai đoạn 2: Giữ cố định và trôi nhẹ lên trên 15px
        float holdTime = Mathf.Max(0.5f, duration - 0.7f);
        elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float floatT = elapsed / holdTime;
            if (containerRect != null)
            {
                containerRect.anchoredPosition = basePosition + new Vector2(0, floatT * 15f);
            }
            yield return null;
        }

        // Giai đoạn 3: Fade Out mượt mà trong 0.45 giây
        float fadeTime = 0.45f;
        elapsed = 0f;
        Vector2 startFadePos = containerRect != null ? containerRect.anchoredPosition : basePosition;
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeTime);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }
            if (containerRect != null)
            {
                containerRect.anchoredPosition = startFadePos + new Vector2(0, t * 10f);
            }
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (containerRect != null)
        {
            containerRect.anchoredPosition = basePosition;
            containerRect.localScale = baseScale;
        }
        activeAnimRoutine = null;
    }

    /// <summary>
    /// Ẩn ngay lập tức thông báo khi hồi sinh hoặc đổi màn hình
    /// </summary>
    public void HideImmediate()
    {
        if (activeAnimRoutine != null)
        {
            StopCoroutine(activeAnimRoutine);
            activeAnimRoutine = null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (containerRect != null && userConfiguredPosition != Vector2.zero)
        {
            containerRect.anchoredPosition = userConfiguredPosition;
            containerRect.localScale = userConfiguredScale;
        }
    }

    [ContextMenu("Test Breakthrough Banner")]
    public void TestBreakthrough()
    {
        DisplayBreakthrough("Luyện Khí Tầng 6");
    }

    [ContextMenu("Test Death Banner")]
    public void TestDeath()
    {
        DisplayDeath();
    }
}

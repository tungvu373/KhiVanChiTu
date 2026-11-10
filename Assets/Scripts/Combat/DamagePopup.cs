using UnityEngine;

/// <summary>
/// Hiển thị chữ số sát thương bay lên (Damage Popup).
/// Hỗ trợ gộp tổng sát thương từ nhiều kiếm chém dồn dập (Accumulated Damage)
/// để tránh tình trạng chữ số bị đè lên nhau, kèm dấu '-' đằng trước.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private float moveYSpeed = 2.2f;
    private float disappearTimer;
    private const float DISAPPEAR_TIMER_MAX = 0.95f;
    private Color textColor;
    private Camera mainCamera;

    private float totalDamage = 0f;
    private bool isCritical = false;
    private float accumulateTimer = 0f;
    private const float ACCUMULATE_WINDOW = 0.42f; // Gom một nhịp chém, không sinh chữ theo từng frame.
    private float currentScale = 1f;

    /// <summary>
    /// Kiểm tra xem popup này còn trong thời gian nhận cộng dồn sát thương hay không.
    /// </summary>
    public bool CanAccumulate => gameObject.activeInHierarchy && accumulateTimer > 0f && disappearTimer > 0.25f;

    public void Setup(float damageAmount, bool isCrit)
    {
        if (textMesh == null) textMesh = GetComponent<TextMesh>();
        if (textMesh == null) return;

        totalDamage = damageAmount;
        isCritical = isCrit;
        accumulateTimer = ACCUMULATE_WINDOW;
        disappearTimer = DISAPPEAR_TIMER_MAX;
        currentScale = 1.3f;
        transform.localScale = Vector3.one * currentScale;

        UpdateVisuals();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Hiển thị chữ trạng thái chiến đấu tùy chỉnh (NÉ!, TRÙNG SINH!, v.v.).
    /// </summary>
    public void SetupCustom(string text, Color color, float size = 0.15f)
    {
        if (textMesh == null) textMesh = GetComponent<TextMesh>();
        if (textMesh == null) return;

        totalDamage = 0f;
        isCritical = false;
        accumulateTimer = 0f;
        disappearTimer = DISAPPEAR_TIMER_MAX;
        currentScale = 1.4f;
        transform.localScale = Vector3.one * currentScale;

        textMesh.text = text;
        textColor = color;
        textMesh.color = color;
        textMesh.characterSize = size;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Cộng dồn sát thương từ các kiếm tiếp theo chém trúng cùng một đợt.
    /// </summary>
    public void AddDamage(float additionalAmount, bool isCrit)
    {
        totalDamage += additionalAmount;
        if (isCrit) isCritical = true;

        // Giữ cửa sổ combo đếm ngược tự nhiên (không reset về ban đầu) để các đợt đâm sau sinh ra popup mới nảy lên
        disappearTimer = Mathf.Max(disappearTimer, 0.6f);

        // Hiệu ứng nảy chữ (Punch scale) mỗi khi nhận thêm sát thương
        currentScale = 1.35f;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (textMesh == null) return;

        // Thêm dấu '-' đằng trước lượng sát thương theo yêu cầu
        textMesh.text = "-" + Mathf.RoundToInt(totalDamage).ToString();

        // Đòn thường trắng vàng nhạt; crit đỏ cam bạo kích sắc nét, kích cỡ chuẩn Mobile F2P
        if (isCritical)
        {
            textColor = new Color(1f, 0.25f, 0.15f, 1f); // Đỏ cam bạo kích rực rỡ
            textMesh.characterSize = 0.17f;
        }
        else
        {
            textColor = new Color(1f, 0.95f, 0.85f, 0.95f); // Trắng ngà thanh lịch
            textMesh.characterSize = 0.13f;
        }

        textMesh.color = textColor;
    }

    private void Update()
    {
        // Bay lên mượt mà theo phương Y (dùng unscaledDeltaTime để hoạt động cả khi Time.timeScale = 0)
        transform.position += Vector3.up * (moveYSpeed * Time.unscaledDeltaTime);

        // Hiệu ứng nảy chữ đàn hồi (Scale bounce)
        if (currentScale > 1f)
        {
            currentScale = Mathf.MoveTowards(currentScale, 1f, Time.unscaledDeltaTime * 3.5f);
            transform.localScale = Vector3.one * currentScale;
        }

        // Luôn xoay mặt về Camera (Billboarding)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
        }

        if (accumulateTimer > 0f)
        {
            accumulateTimer -= Time.unscaledDeltaTime;
        }

        disappearTimer -= Time.unscaledDeltaTime;
        if (disappearTimer < 0f)
        {
            // Mờ dần rồi ẩn đi về Object Pool
            float disappearSpeed = 3.5f;
            textColor.a -= disappearSpeed * Time.unscaledDeltaTime;
            if (textMesh != null) textMesh.color = textColor;
            if (textColor.a <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

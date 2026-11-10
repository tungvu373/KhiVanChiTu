using UnityEngine;

/// <summary>
/// Quản lý lớp Hộ thể (Shield) của Boss khai màn và Boss độ kiếp.
/// Tích hợp hiệu ứng thị giác rào chắn năng lượng 3D, đổi màu rạn nứt theo lượng khiên còn lại
/// và nổ vỡ vụn crystalline qua HitVFXManager khi giá trị giảm về 0.
/// Không hiển thị text "Hộ thể" thô trên đầu Boss theo yêu cầu Game Feel.
/// </summary>
public class OpeningBossGuard : MonoBehaviour
{
    public float Shield { get; private set; }
    public float MaximumShield { get; private set; }
    private Camera cachedCam;

    [Header("Visual Shield Barrier")]
    private GameObject shieldVisualObj;
    private MeshRenderer shieldRenderer;
    private MaterialPropertyBlock shieldBlock;
    private float pulseTimer = 0f;
    private Vector3 baseScale = new Vector3(3.4f, 3.4f, 3.4f);

    private void Awake()
    {
        // Xóa triệt để text Hộ Thể nếu còn sót lại từ prefab cũ
        Transform oldLabel = transform.Find("OpeningBossShieldLabel");
        if (oldLabel != null) Destroy(oldLabel.gameObject);
    }

    private void EnsureShieldVisual()
    {
        if (shieldVisualObj == null)
        {
            shieldVisualObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldVisualObj.name = "BossShieldBarrier";
            shieldVisualObj.transform.SetParent(transform, false);
            shieldVisualObj.transform.localPosition = Vector3.up * 1.5f;
            shieldVisualObj.transform.localScale = baseScale;

            var col = shieldVisualObj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            shieldRenderer = shieldVisualObj.GetComponent<MeshRenderer>();
            Material shieldMat = null;
#if UNITY_EDITOR
            shieldMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningGlow_URP.mat");
#endif
            if (shieldMat == null)
            {
                var s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (s == null) s = Shader.Find("Sprites/Default");
                shieldMat = new Material(s);
            }
            shieldRenderer.sharedMaterial = shieldMat;
            shieldBlock = new MaterialPropertyBlock();
        }
    }

    private void UpdateShieldVisualColor(float ratio)
    {
        if (shieldRenderer == null) return;
        if (shieldBlock == null) shieldBlock = new MaterialPropertyBlock();

        // Tỷ lệ còn lại: từ xanh ngọc thanh tú (1.0) -> chuyển sang cam rạn nứt (0.0)
        Color shieldCol;
        if (ratio > 0.5f)
        {
            shieldCol = Color.Lerp(new Color(0.1f, 0.9f, 0.95f, 0.45f), new Color(0.2f, 0.6f, 1f, 0.55f), (ratio - 0.5f) * 2f);
        }
        else
        {
            shieldCol = Color.Lerp(new Color(1f, 0.35f, 0.1f, 0.65f), new Color(0.1f, 0.9f, 0.95f, 0.45f), ratio * 2f);
        }

        shieldRenderer.GetPropertyBlock(shieldBlock);
        if (shieldRenderer.sharedMaterial != null && shieldRenderer.sharedMaterial.HasProperty("_BaseColor"))
            shieldBlock.SetColor("_BaseColor", shieldCol);
        else
            shieldBlock.SetColor("_Color", shieldCol);
        shieldRenderer.SetPropertyBlock(shieldBlock);
    }

    public void ResetGuard(float amount)
    {
        Shield = MaximumShield = Mathf.Max(0, amount);
        EnsureShieldVisual();
        if (shieldVisualObj != null)
        {
            shieldVisualObj.SetActive(MaximumShield > 0);
            UpdateShieldVisualColor(1f);
        }

        // Xóa text Hộ Thể nếu có
        Transform oldLabel = transform.Find("OpeningBossShieldLabel");
        if (oldLabel != null) Destroy(oldLabel.gameObject);
    }

    public float Absorb(float amount)
    {
        if (Shield <= 0f) return amount;
        float prevShield = Shield;
        float absorbed = Mathf.Min(Shield, Mathf.Max(0, amount));
        Shield -= absorbed;

        // Hiệu ứng rạn nứt / chớp khi hấp thụ sát thương
        pulseTimer = 0.10f;
        float ratio = MaximumShield > 0f ? (Shield / MaximumShield) : 0f;
        UpdateShieldVisualColor(ratio);

        if (prevShield > 0f && Shield <= 0f)
        {
            if (shieldVisualObj != null) shieldVisualObj.SetActive(false);
            if (HitVFXManager.Instance != null)
            {
                HitVFXManager.Instance.SpawnShieldBreakBurst(transform.position + Vector3.up * 1.5f);
            }
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("🛡️ HỘ THỂ BOSS ĐÃ BỊ PHÁ VỠ! Boss rơi vào trạng thái suy yếu!", Color.cyan);
            }
        }
        return Mathf.Max(0, amount - absorbed);
    }

    private void LateUpdate()
    {
        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime;
            float punch = Mathf.Clamp01(pulseTimer / 0.10f) * 0.15f;
            if (shieldVisualObj != null) shieldVisualObj.transform.localScale = baseScale * (1f + punch);
        }
        else if (shieldVisualObj != null && shieldVisualObj.transform.localScale != baseScale)
        {
            shieldVisualObj.transform.localScale = baseScale;
        }

        if (shieldVisualObj != null && shieldVisualObj.activeSelf)
        {
            shieldVisualObj.transform.Rotate(Vector3.up * 30f * Time.deltaTime, Space.Self);
        }
    }
}

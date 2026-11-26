using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DynamicCameraFrustum : MonoBehaviour
{
    [SerializeField] private float targetAspect = 9f / 16f;

    [Header("Camera Overview Placement")]
    [Tooltip("Tự động nâng cao độ cao và góc nghiêng camera để có tầm nhìn bao quát toàn bộ chiến trường")]
    public bool autoPositionCamera = true;
    [Tooltip("Vị trí Camera bao quát (độ cao Y cao hơn, lùi xa hơn để thấy quái xung quanh)")]
    public Vector3 overviewPosition = new Vector3(0.36f, 13.5f, -14f);
    [Tooltip("Góc nghiêng nhìn xuống từ trên cao (Isometric 45 độ)")]
    public Vector3 overviewRotation = new Vector3(45f, 0f, 0f);

    [Header("Camera FOV Settings")]
    [Tooltip("Góc nhìn của Camera (càng lớn góc nhìn càng rộng, bao quát nhiều không gian hơn)")]
    [Range(45f, 100f)]
    public float defaultFov = 80f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyOverviewTransform();
        AdjustCamera();
    }

    private void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (autoPositionCamera && !Application.isPlaying)
        {
            ApplyOverviewTransform();
        }
        AdjustCamera();
    }

    public void ApplyOverviewTransform()
    {
        if (autoPositionCamera)
        {
            transform.position = overviewPosition;
            transform.rotation = Quaternion.Euler(overviewRotation);
        }
    }

    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
            AdjustCamera();
    }

    public void AdjustCamera()
    {
        if (cam == null) return;
        if (Screen.height <= 0 || Screen.width <= 0) return;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        float currentAspect = (float)lastWidth / lastHeight;
        if (currentAspect < targetAspect)
        {
            float halfFovRadians = defaultFov * Mathf.Deg2Rad * 0.5f;
            cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(halfFovRadians) * targetAspect / currentAspect) * Mathf.Rad2Deg;
        }
        else
            cam.fieldOfView = defaultFov;
    }
}

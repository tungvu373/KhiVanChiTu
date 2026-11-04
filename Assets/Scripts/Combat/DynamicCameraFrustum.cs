using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DynamicCameraFrustum : MonoBehaviour
{
    [SerializeField] private float targetAspect = 9f / 16f;
    [SerializeField] private float defaultFov = 60f;
    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
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

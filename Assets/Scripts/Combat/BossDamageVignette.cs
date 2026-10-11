using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Hiệu ứng viền chớp đỏ quanh nửa màn hình Combat 3D (bên trái) khi bị Boss tấn công.
/// Hoạt động hoàn toàn tự động, độc lập (Plug & Play), không cần cấu hình thủ công trong Editor.
/// </summary>
public class BossDamageVignette : MonoBehaviour
{
    private static BossDamageVignette _instance;
    public static BossDamageVignette Instance
    {
        get
        {
            if (_instance == null)
            {
                CreateAutoInstance();
            }
            return _instance;
        }
    }

    private CanvasGroup canvasGroup;
    private Coroutine flashCoroutine;
    private static Texture2D vignetteTexture;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetupUI();
    }

    private static void CreateAutoInstance()
    {
        // Tự động tìm Canvas trong Scene
        Canvas targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            Debug.LogWarning("[BossDamageVignette] Không tìm thấy Canvas trong Scene!");
            return;
        }

        // Tạo GameObject độc lập con của Canvas
        GameObject vignetteObj = new GameObject("BossDamageVignetteOverlay", typeof(RectTransform));
        vignetteObj.transform.SetParent(targetCanvas.transform, false);
        _instance = vignetteObj.AddComponent<BossDamageVignette>();
    }

    private void SetupUI()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        // Căn chỉnh bao trọn đúng 50% màn hình bên Trái (Khu vực Combat 3D)
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Cho overlay xuống dưới các popup text nhưng trên nền combat
        transform.SetAsLastSibling();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // 1. Tạo hình nền Vignette viền đỏ mềm (Gradient viền tỏa vào tâm)
        GameObject vignetteImgObj = new GameObject("VignetteGlow", typeof(RectTransform), typeof(Image));
        vignetteImgObj.transform.SetParent(transform, false);
        RectTransform vRt = vignetteImgObj.GetComponent<RectTransform>();
        vRt.anchorMin = Vector2.zero;
        vRt.anchorMax = Vector2.one;
        vRt.sizeDelta = Vector2.zero;

        Image vignetteImg = vignetteImgObj.GetComponent<Image>();
        vignetteImg.raycastTarget = false;
        vignetteImg.color = new Color(1f, 0.05f, 0.05f, 0.75f); // Đỏ rực
        vignetteImg.sprite = GetOrCreateVignetteSprite();

        // 2. Tạo thêm 4 dải viền ngoài cùng đậm nét (Edge borders)
        CreateEdgeBorder("TopBorder", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 20), new Color(1f, 0f, 0f, 0.9f));
        CreateEdgeBorder("BottomBorder", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Color(1f, 0f, 0f, 0.9f));
        CreateEdgeBorder("LeftBorder", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(20, 0), new Color(1f, 0f, 0f, 0.9f));
        CreateEdgeBorder("RightBorder", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(20, 0), new Color(1f, 0f, 0f, 0.9f));
    }

    private void CreateEdgeBorder(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Color color)
    {
        GameObject edgeObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        edgeObj.transform.SetParent(transform, false);
        RectTransform edgeRt = edgeObj.GetComponent<RectTransform>();
        edgeRt.anchorMin = anchorMin;
        edgeRt.anchorMax = anchorMax;
        edgeRt.pivot = pivot;
        edgeRt.sizeDelta = sizeDelta;
        
        Image img = edgeObj.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private Sprite GetOrCreateVignetteSprite()
    {
        if (vignetteTexture == null)
        {
            int size = 64;
            vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            vignetteTexture.wrapMode = TextureWrapMode.Clamp;
            
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / (size - 1);
                    float v = (float)y / (size - 1);
                    
                    // Khoảng cách tới mép gần nhất (0 ở viền, 0.5 ở tâm)
                    float edgeDist = Mathf.Min(u, 1f - u, v, 1f - v);
                    // Biên đỏ tỏa sâu vào trong 40%
                    float alpha = Mathf.Clamp01(1f - (edgeDist / 0.35f));
                    alpha = Mathf.Pow(alpha, 1.8f); // Cong mượt
                    
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            vignetteTexture.SetPixels(pixels);
            vignetteTexture.Apply();
        }

        return Sprite.Create(vignetteTexture, new Rect(0, 0, vignetteTexture.width, vignetteTexture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Kích hoạt hiệu ứng chớp đỏ viền khi Boss tung đòn
    /// </summary>
    public static void TriggerFlash()
    {
        if (Instance != null)
        {
            Instance.DoFlash();
        }
    }

    public void DoFlash()
    {
        if (canvasGroup == null) return;
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // 1. Chớp đỏ mạnh tức thì
        canvasGroup.alpha = 0.95f;
        yield return new WaitForSeconds(0.04f);

        // 2. Mờ dần mượt mà trong 0.4 giây
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(0.95f, 0f, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        flashCoroutine = null;
    }
}

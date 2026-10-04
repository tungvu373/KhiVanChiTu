using UnityEngine;

// --- Hiệu ứng Vòng sóng xung kích (Shockwave) ---
public class ShockwaveVFX : MonoBehaviour
{
    public float expandSpeed = 20f;
    public float fadeSpeed = 2f;
    private Material mat;
    private Color color;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            mat = r.material;
            color = mat.color;
        }
    }

    void Update()
    {
        transform.localScale += new Vector3(expandSpeed, 0, expandSpeed) * Time.deltaTime;
        
        if (mat != null)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            mat.color = color;
            if (color.a <= 0) Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}

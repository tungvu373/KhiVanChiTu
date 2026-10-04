using UnityEngine;

// --- Hiệu ứng Vòng hào quang xoay (Aura) & Vòng tròn Âm Dương ---
public class AuraVFX : MonoBehaviour
{
    public float rotateSpeed = 360f; // Xoay rất nhanh
    
    public void SetupYinYang()
    {
        // Orb Trắng (Dương)
        GameObject yang = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yang.transform.SetParent(transform);
        yang.transform.localPosition = new Vector3(3f, 1f, 0f);
        yang.transform.localScale = Vector3.one * 1.5f;
        Destroy(yang.GetComponent<Collider>());
        
        Material matYang = new Material(Shader.Find("Standard"));
        matYang.color = Color.white;
        matYang.EnableKeyword("_EMISSION");
        matYang.SetColor("_EmissionColor", Color.white);
        yang.GetComponent<Renderer>().material = matYang;

        TrailRenderer trYang = yang.AddComponent<TrailRenderer>();
        trYang.time = 0.5f;
        trYang.startWidth = 1.5f;
        trYang.endWidth = 0f;
        trYang.material = new Material(Shader.Find("Sprites/Default"));
        trYang.startColor = Color.white;
        trYang.endColor = new Color(1, 1, 1, 0);

        // Orb Đen (Âm)
        GameObject yin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yin.transform.SetParent(transform);
        yin.transform.localPosition = new Vector3(-3f, 1f, 0f);
        yin.transform.localScale = Vector3.one * 1.5f;
        Destroy(yin.GetComponent<Collider>());

        Material matYin = new Material(Shader.Find("Standard"));
        matYin.color = Color.black;
        yin.GetComponent<Renderer>().material = matYin;

        TrailRenderer trYin = yin.AddComponent<TrailRenderer>();
        trYin.time = 0.5f;
        trYin.startWidth = 1.5f;
        trYin.endWidth = 0f;
        trYin.material = new Material(Shader.Find("Sprites/Default"));
        trYin.startColor = Color.black;
        trYin.endColor = new Color(0, 0, 0, 0);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}

using UnityEngine;

// --- Hiệu ứng Bàn Tay Khổng Lồ (Ấn Chưởng) ---
public class GiantHandVFX : MonoBehaviour
{
    private float fallSpeed = 30f;
    private bool hasHit = false;

    public void Setup(Color color)
    {
        // Tạo Lòng bàn tay (Palm)
        GameObject palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palm.transform.SetParent(transform);
        palm.transform.localPosition = Vector3.zero;
        palm.transform.localScale = new Vector3(2f, 0.5f, 2f);
        ApplyMaterial(palm, color);

        // Tạo 4 Ngón tay (Fingers)
        for (int i = 0; i < 4; i++)
        {
            GameObject finger = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            finger.transform.SetParent(transform);
            float xOffset = -0.75f + (i * 0.5f);
            finger.transform.localPosition = new Vector3(xOffset, 0, 1.25f);
            finger.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
            finger.transform.localRotation = Quaternion.Euler(90, 0, 0); // Chỉ thẳng tới trước
            ApplyMaterial(finger, color);
        }

        // Tạo Ngón cái (Thumb)
        GameObject thumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        thumb.transform.SetParent(transform);
        thumb.transform.localPosition = new Vector3(1.25f, 0, -0.2f);
        thumb.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        thumb.transform.localRotation = Quaternion.Euler(90, 45, 0);
        ApplyMaterial(thumb, color);

        // Hơi nghiêng bàn tay chúi xuống đất
        transform.rotation = Quaternion.Euler(30, 0, 0);
    }

    private void ApplyMaterial(GameObject obj, Color color)
    {
        Destroy(obj.GetComponent<Collider>()); // Bỏ va chạm
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetColor("_Color", new Color(color.r, color.g, color.b, 0.8f));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 1.5f);
        obj.GetComponent<Renderer>().material = mat;
    }

    void Update()
    {
        if (hasHit) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        
        // Khi chạm đất (y <= 0)
        if (transform.position.y <= 0)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            hasHit = true;
            CreateImpactParticles();
            
            // Xóa bàn tay sau 0.5s
            Destroy(gameObject, 0.5f);
        }
    }

    private void CreateImpactParticles()
    {
        // 1. Bụi bốc lên (Dust Burst)
        GameObject dustObj = new GameObject("DustParticles");
        dustObj.transform.position = transform.position;
        ParticleSystem dustPs = dustObj.AddComponent<ParticleSystem>();
        dustPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var dustMain = dustPs.main;
        dustMain.duration = 1f;
        dustMain.startLifetime = 1f;
        dustMain.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        dustMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        dustMain.startColor = new Color(0.6f, 0.4f, 0.2f, 0.8f); // Màu đất
        dustMain.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var dustEmission = dustPs.emission;
        dustEmission.rateOverTime = 0;
        dustEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 40) });
        
        var dustShape = dustPs.shape;
        dustShape.shapeType = ParticleSystemShapeType.Cone;
        dustShape.angle = 45f;
        
        var dustRenderer = dustPs.GetComponent<ParticleSystemRenderer>();
        dustRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        dustPs.Play();
        Destroy(dustObj, 2f);

        // 2. Hiệu ứng Nứt Đất (Cracks - dùng Stretched Billboard màu đen chĩa ra nhiều hướng)
        GameObject crackObj = new GameObject("CrackParticles");
        crackObj.transform.position = transform.position + Vector3.up * 0.05f;
        ParticleSystem crackPs = crackObj.AddComponent<ParticleSystem>();
        crackPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var crackMain = crackPs.main;
        crackMain.duration = 1f;
        crackMain.startLifetime = 2f;
        crackMain.startSpeed = 10f; // Văng ra
        crackMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        crackMain.startColor = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Màu đen xám của vết nứt
        
        var crackEmission = crackPs.emission;
        crackEmission.rateOverTime = 0;
        crackEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 15) });
        
        var crackShape = crackPs.shape;
        crackShape.shapeType = ParticleSystemShapeType.Circle;
        crackShape.radius = 0.5f;
        
        var crackVel = crackPs.velocityOverLifetime;
        crackVel.enabled = true;
        // Bắt hạt dừng lại rất nhanh để tạo hình vết nứt nằm trên mặt đất
        var crackLimit = crackPs.limitVelocityOverLifetime;
        crackLimit.enabled = true;
        crackLimit.limit = 0;
        crackLimit.dampen = 0.3f; // Kháng lực mạnh để văng ra rồi khựng lại
        
        var crackRenderer = crackPs.GetComponent<ParticleSystemRenderer>();
        crackRenderer.material = new Material(Shader.Find("Sprites/Default"));
        crackRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        crackRenderer.cameraVelocityScale = 0;
        crackRenderer.velocityScale = 0.5f; // Kéo dài ra
        crackRenderer.lengthScale = 3f;
        // Đặt hạt nằm ngang trên mặt đất
        crackRenderer.alignment = ParticleSystemRenderSpace.Local;
        crackObj.transform.rotation = Quaternion.Euler(90, 0, 0); 
        
        crackPs.Play();
        Destroy(crackObj, 3f);
    }
    
    private void OnDestroy()
    {
        // Dọn dẹp tất cả Material con
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.material != null) Destroy(r.material);
        }
    }
}

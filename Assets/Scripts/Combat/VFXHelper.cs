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

// --- Hiệu ứng Tia sét (Lightning Line) ---
public class LightningVFX : MonoBehaviour
{
    private LineRenderer lr;
    private float lifetime = 0.3f;

    public void Setup(Vector3 start, Vector3 end, Color color)
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 5; // Tạm 5 khúc gấp khúc
        lr.startWidth = 0.5f;
        lr.endWidth = 0.1f;
        
        // Dùng shader Unlit/Color để phát sáng
        lr.material = new Material(Shader.Find("Sprites/Default")); 
        lr.startColor = color;
        lr.endColor = Color.white;

        // Tạo đường gấp khúc ngẫu nhiên
        lr.SetPosition(0, start);
        for (int i = 1; i < 4; i++)
        {
            Vector3 pos = Vector3.Lerp(start, end, i / 4f);
            pos += Random.insideUnitSphere * 1.5f; // Độ giật của sét
            lr.SetPosition(i, pos);
        }
        lr.SetPosition(4, end);

        // Ánh sáng loé lên
        Light light = gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 5f;
        light.range = 15f;

        // Tạo tia lửa điện dưới đất (Sparks Particle System)
        GameObject sparkObj = new GameObject("Sparks");
        sparkObj.transform.position = end;
        ParticleSystem sparkPs = sparkObj.AddComponent<ParticleSystem>();
        var main = sparkPs.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = color; // Màu sét
        
        var emission = sparkPs.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 30) });
        
        var shape = sparkPs.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        
        var renderer = sparkPs.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f; // Tia lửa dãn dài ra
        
        Destroy(sparkObj, 1f);
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
        else if (lr != null)
        {
            // Nhấp nháy alpha
            Color c = lr.startColor;
            c.a = Random.Range(0.2f, 1f);
            lr.startColor = c;
        }
    }
}

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
        
        Destroy(dustObj, 2f);

        // 2. Hiệu ứng Nứt Đất (Cracks - dùng Stretched Billboard màu đen chĩa ra nhiều hướng)
        GameObject crackObj = new GameObject("CrackParticles");
        crackObj.transform.position = transform.position + Vector3.up * 0.05f;
        ParticleSystem crackPs = crackObj.AddComponent<ParticleSystem>();
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
        
        Destroy(crackObj, 3f);
    }
}

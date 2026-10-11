using UnityEngine;

/// <summary>
/// Hệ thống Mây Tiên (Particle System) lơ lửng bồng bềnh đi theo Player.
/// Tạo không gian Tiên Cảnh tu tiên bổ trợ cho Sương mù ở xa (Fog).
/// </summary>
public class TuTienCloudSystem : MonoBehaviour
{
    public static TuTienCloudSystem Instance { get; private set; }

    [Header("Cài đặt Theo Dấu Nhân Vật")]
    public Transform playerTarget;
    public float heightOffset = -0.3f; // Nằm sát mặt đất / chân nhân vật
    public float forwardOffset = 5.0f;  // Dồn mây nhẹ về phía trước tầm nhìn

    [Header("Hiệu ứng Hạt (Particle System)")]
    public ParticleSystem cloudParticleSystem;

    private static Texture2D softCloudTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        FindPlayerTarget();
        SetupCloudParticleSystem();
    }

    private void Start()
    {
        FindPlayerTarget();
    }

    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null) return;
        }

        // Luôn đi theo vị trí của Player trên trục X và Z, giữ Y ổn định quanh mặt đài
        Vector3 targetPos = new Vector3(
            playerTarget.position.x, 
            playerTarget.position.y + heightOffset, 
            playerTarget.position.z + forwardOffset
        );

        transform.position = targetPos;
    }

    private void FindPlayerTarget()
    {
        if (playerTarget != null) return;

        if (CombatManager.Instance != null && CombatManager.Instance.playerTransform != null)
        {
            playerTarget = CombatManager.Instance.playerTransform;
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            return;
        }

        // Fallback tìm đối tượng tên Player
        GameObject p = GameObject.Find("Player") ?? GameObject.Find("PlayerModel");
        if (p != null) playerTarget = p.transform;
    }

    public void SetupCloudParticleSystem()
    {
        if (cloudParticleSystem == null)
        {
            cloudParticleSystem = GetComponent<ParticleSystem>();
            if (cloudParticleSystem == null)
            {
                cloudParticleSystem = gameObject.AddComponent<ParticleSystem>();
            }
        }

        // Dừng và xóa hạt nếu đang phát trước khi thiết lập duration để tránh lỗi Unity:
        if (cloudParticleSystem.isPlaying)
        {
            cloudParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Cấu hình Main
        var main = cloudParticleSystem.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 13f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(6f, 12f); // Đám mây to bồng bềnh
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor = new Color(0.92f, 0.96f, 1f, 0.16f); // Trắng ngọc tiên khí trong suốt
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Mây trôi mượt trong không gian thế giới khi Player di chuyển
        main.maxParticles = 80; // Số lượng vừa vặn, không che khuất thanh máu và chiêu thức

        // Cấu hình Emission (Sinh mây đều đặn)
        var emission = cloudParticleSystem.emission;
        emission.rateOverTime = 7f;

        // Cấu hình Vùng bao quanh Player (Shape: Box phủ rộng quanh người chơi)
        var shape = cloudParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(26f, 1.2f, 32f); // Rộng 26m, dày 1.2m, dài 32m

        // Vận tốc trôi nhẹ (Velocity over Lifetime)
        var vel = cloudParticleSystem.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        vel.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.8f, -0.3f); // Trôi nhẹ theo chiều cuộn map

        // Hiệu ứng Fade In và Fade Out mượt mà (Color over Lifetime)
        var col = cloudParticleSystem.colorOverLifetime;
        col.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(new Color(0.85f, 0.95f, 1f), 0.5f), 
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),       // Mới sinh: Trong suốt hoàn toàn
                new GradientAlphaKey(1f, 0.25f),    // 25% dòng đời: Hiện rõ mềm mại
                new GradientAlphaKey(1f, 0.75f),    // 75% dòng đời: Duy trì
                new GradientAlphaKey(0f, 1f)        // 100% dòng đời: Tan biến dần
            }
        );
        col.color = gradient;

        // Kích thước phồng to nhẹ theo thời gian (Size over Lifetime)
        var sol = cloudParticleSystem.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.75f);
        sizeCurve.AddKey(0.5f, 1.0f);
        sizeCurve.AddKey(1f, 1.3f);
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Xoay nhẹ nhàng (Rotation over Lifetime)
        var rol = cloudParticleSystem.rotationOverLifetime;
        rol.enabled = true;
        rol.z = new ParticleSystem.MinMaxCurve(-8f * Mathf.Deg2Rad, 8f * Mathf.Deg2Rad);

        // Vật liệu (Renderer)
        var renderer = cloudParticleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -1; // Vẽ dưới thanh máu và text

            Material cloudMat = CreateSoftCloudMaterial();
            if (cloudMat != null)
            {
                renderer.material = cloudMat;
            }
        }

        if (!cloudParticleSystem.isPlaying)
        {
            cloudParticleSystem.Play();
        }
    }

    private static Material CreateSoftCloudMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default") 
                     ?? Shader.Find("Universal Render Pipeline/Particles/Unlit") 
                     ?? Shader.Find("Particles/Standard Unlit");

        Material mat = new Material(shader);
        mat.name = "M_TuTienCloudSoft";

        if (softCloudTexture == null)
        {
            softCloudTexture = GenerateSoftRadialCloudTexture();
        }

        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", softCloudTexture);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", softCloudTexture);

        return mat;
    }

    private static Texture2D GenerateSoftRadialCloudTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "T_SoftCloudProcedural";
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / radius);
                // Làm mềm cạnh viền hạt (Gaussian Smoothstep)
                float alpha = Mathf.SmoothStep(1f, 0f, t);
                alpha = Mathf.Pow(alpha, 1.8f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return tex;
    }
}

using UnityEngine;

/// <summary>
/// Quản lý hiệu ứng va chạm kiếm chém và bạo kích (Hit Sparks, Flash, Game Feel).
/// Tối ưu hiệu năng cao: Dùng ParticleSystem Emit trực tiếp, không sinh rác bộ nhớ (Zero GC).
/// </summary>
public class HitVFXManager : MonoBehaviour
{
    public static HitVFXManager Instance { get; private set; }

    private ParticleSystem sparkParticles;
    private ParticleSystem critParticles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        // 1. Hệ thống tia lửa kiếm chém thường (Luminous Sparks)
        GameObject sparkObj = new GameObject("VFX_HitSparks");
        sparkObj.transform.SetParent(transform);
        sparkParticles = sparkObj.AddComponent<ParticleSystem>();
        
        var main = sparkParticles.main;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = 0.22f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 16f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new Color(1f, 0.92f, 0.45f, 1f); // Ánh vàng lôi kiếm rực rỡ
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = sparkParticles.emission;
        emission.enabled = false;

        var shape = sparkParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var colorOverLifetime = sparkParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.45f, 0.1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var psr = sparkParticles.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));

        // 2. Hệ thống bạo kích (Crit Explosion Sparks)
        GameObject critObj = new GameObject("VFX_CritSparks");
        critObj.transform.SetParent(transform);
        critParticles = critObj.AddComponent<ParticleSystem>();
        
        var cMain = critParticles.main;
        cMain.duration = 0.35f;
        cMain.loop = false;
        cMain.startLifetime = 0.32f;
        cMain.startSpeed = new ParticleSystem.MinMaxCurve(12f, 24f);
        cMain.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        cMain.startColor = new Color(1f, 0.25f, 0.1f, 1f); // Huyết quang bạo phát
        cMain.simulationSpace = ParticleSystemSimulationSpace.World;
        cMain.playOnAwake = false;

        var cEmission = critParticles.emission;
        cEmission.enabled = false;

        var cShape = critParticles.shape;
        cShape.shapeType = ParticleSystemShapeType.Sphere;
        cShape.radius = 0.3f;

        var cColor = critParticles.colorOverLifetime;
        cColor.enabled = true;
        Gradient cGrad = new Gradient();
        cGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.15f, 0.1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        cColor.color = cGrad;

        var cPsr = critParticles.GetComponent<ParticleSystemRenderer>();
        cPsr.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SpawnHitImpact(Vector3 hitPoint, Vector3 swordDirection, bool isCrit)
    {
        if (isCrit && critParticles != null)
        {
            critParticles.transform.position = hitPoint;
            critParticles.Emit(16);
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.12f, 0.08f);
            }
        }
        else if (sparkParticles != null)
        {
            sparkParticles.transform.position = hitPoint;
            sparkParticles.Emit(8);
        }
    }
}


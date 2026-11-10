using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý hiệu ứng va chạm kiếm chém, bạo kích, Kiếm Xuyên, Phá Giáp, đòn đánh quái/Boss,
/// Sáu Đại Pháp Trận (Lôi Trận, Mưa Kiếm, Tụ Linh, Băng Trận, Huyết Ma, Kim Cang),
/// Bốn Kỹ Năng (Ấn Chưởng, Lôi Phạt, Âm Dương Trận, Vạn Kiếm Quy Tông) và Hào Quang Pháp Khí.
/// Tối ưu hiệu năng cao: Dùng ParticleSystem Emit trực tiếp, LineRenderer pool, Zero GC runtime.
/// </summary>
public class HitVFXManager : MonoBehaviour
{
    private static HitVFXManager _instance;
    private static bool isQuitting = false;
    public static bool HasInstance => _instance != null && !isQuitting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _instance = null;
        isQuitting = false;
    }

    public static HitVFXManager Instance
    {
        get
        {
            if (isQuitting) return null;
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<HitVFXManager>();
                if (_instance == null && !isQuitting && Application.isPlaying)
                {
                    GameObject go = new GameObject("HitVFXManager");
                    _instance = go.AddComponent<HitVFXManager>();
                }
            }
            if (_instance != null)
            {
                _instance.EnsureInitialized();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("URP Materials")]
    public Material sparkMaterial;
    public Material critMaterial;
    public Material ringMaterial;
    public Material tracerMaterial;
    public Material glowMaterial;
    public Material runeMaterial;
    public Material goldenPalmMaterial;
    public Material bloodMaterial;
    public Material bloodSplatMaterial;

    // Prompt 02: Particle systems
    private ParticleSystem sparkParticles;
    private ParticleSystem critSparksParticles;
    private ParticleSystem critCoreParticles;
    private ParticleSystem armorBreakParticles;
    private ParticleSystem shieldBreakParticles;
    private ParticleSystem shieldRingParticles;

    // Prompt 03: Hệ thống hạt quái thường, boss và người chơi
    private ParticleSystem enemySwipeParticles;
    private ParticleSystem deathDissolveParticles;
    private ParticleSystem bossWindUpParticles;
    private ParticleSystem bossImpactParticles;
    private ParticleSystem bossShockwaveParticles;
    private ParticleSystem reviveParticles;

    // Prompt 04 & 05: Trận pháp
    private ParticleSystem formationRuneParticles;
    private ParticleSystem goldenBellParticles;
    private ParticleSystem baguaWispsParticles;
    private ParticleSystem frostMistParticles;
    private ParticleSystem formationImpactParticles;
    private Material formationSparkMaterial;
    private Material formationRingMaterial;
    private GameObject formationAuraObj;
    private FormationGlyphVFX formationGlyph;
    private ParticleSystem formationAuraParticles;
    private LineRenderer goldenBellWard;
    private float goldenBellWardTime;
    private const float GoldenBellWardDuration = .38f;
    private const float ThunderWidth = .44f;
    private const float RainWidth = .30f;
    private const float SiphonWidth = .24f;

    // Prompt 06 & 07: Kỹ năng chiến đấu
    private ParticleSystem anChuongDustParticles;
    private ParticleSystem loiPhatShockwaveParticles;
    private ParticleSystem loiPhatExecuteParticles;
    private ParticleSystem stunParticles;

    // Prompt 09 & 10: Vạn Kiếm Quy Tông & Pháp Khí
    private ParticleSystem ultimateSummonParticles;
    private ParticleSystem artifactAuraParticles;

    // LineRenderer Pool cho Kiếm Xuyên (Pierce Tracers - Prompt 02)
    private const int PIERCE_LINE_POOL_SIZE = 6;
    private LineRenderer[] pierceLines;
    private float[] pierceLineTimers;
    private int currentPierceIndex = 0;

    // LineRenderer Pool cho Cửu Thiên Lôi Trận (PulseThunder - Prompt 04)
    private const int THUNDER_LINE_POOL_SIZE = 6;
    private LineRenderer[] formationThunderLines;
    private float[] formationThunderTimers;
    private int currentThunderIndex = 0;

    // LineRenderer Pool cho Vạn Kiếm Trận (PulseSwordRain - Prompt 04)
    private const int SWORD_RAIN_POOL_SIZE = 8;
    private LineRenderer[] formationSwordRainLines;
    private float[] formationSwordRainTimers;
    private int currentSwordRainIndex = 0;

    // LineRenderer Pool cho Huyết Ma Hóa Sinh Trận (Lifesteal Siphon - Prompt 05)
    private const int LIFESTEAL_POOL_SIZE = 4;
    private LineRenderer[] lifestealLines;
    private float[] lifestealTimers;
    private int currentLifestealIndex = 0;
    private float lastLifestealTime = 0f;

    // Lôi Phạt (Prompt 07)
    private LineRenderer loiPhatMainLine;
    private LineRenderer[] loiPhatBranchLines;
    private float loiPhatTimer = 0f;

    // Dynamic Objects (Sustain)
    private GameObject baguaSustainObj;
    private GameObject frostMistObj;
    private GameObject amDuongTranObj;
    private Transform amDuongTranTarget;
    private float amDuongTranTimer = 0f;
    private GameObject giantHandObj;

    private void Awake()
    {
        isQuitting = false;
        if (_instance != null && _instance != this)
        {
            // This scene may contain another HitVFXManager on the shared Managers root.
            // Removing the root would also remove Economy, Combat, and every other manager.
            Destroy(this);
            return;
        }
        _instance = this;
        EnsureInitialized();
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (formationSparkMaterial != null) Destroy(formationSparkMaterial);
        if (formationRingMaterial != null) Destroy(formationRingMaterial);
        if (_instance == this)
        {
            _instance = null;
            isQuitting = true;
        }
    }

    private void Start()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (sparkParticles == null)
        {
            LoadDefaultMaterials();
            formationSparkMaterial = CreateFormationSpriteMaterial(sparkMaterial);
            formationRingMaterial = CreateFormationSpriteMaterial(ringMaterial);
            SetupParticleSystems();
            SetupLinePools();
            SetupPersistentVFXObjects();
        }
    }

    private static Material CreateFormationSpriteMaterial(Material template)
    {
        // The source textures have straight alpha. Keep their transparent margins out of the glow.
        var material = new Material(template) { hideFlags = HideFlags.DontSave };
        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        material.color = Color.white;
        return material;
    }

    private void LoadDefaultMaterials()
    {
#if UNITY_EDITOR
        if (sparkMaterial == null)
            sparkMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningSparks_URP.mat");
        if (critMaterial == null)
            critMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningCore_URP.mat");
        if (ringMaterial == null)
            ringMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_ShockwaveRing_URP.mat");
        if (tracerMaterial == null)
            tracerMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningCore_URP.mat");
        if (glowMaterial == null)
            glowMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningGlow_URP.mat");
        if (runeMaterial == null)
            runeMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_GroundTelegraph_URP.mat");
        if (goldenPalmMaterial == null)
            goldenPalmMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_GoldenPalm_URP.mat");
        if (bloodMaterial == null)
            bloodMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_BloodSplatter_URP.mat");
        if (bloodSplatMaterial == null)
            bloodSplatMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_BloodSplat_URP.mat");
#endif
        Shader urpParticlesShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (urpParticlesShader == null) urpParticlesShader = Shader.Find("Sprites/Default");

        if (sparkMaterial == null)
        {
            sparkMaterial = new Material(urpParticlesShader);
            sparkMaterial.color = new Color(1f, 0.9f, 0.4f, 1f);
        }
        if (critMaterial == null)
        {
            critMaterial = new Material(urpParticlesShader);
            critMaterial.color = new Color(1f, 0.3f, 0.1f, 1f);
        }
        if (ringMaterial == null)
        {
            ringMaterial = new Material(urpParticlesShader);
            ringMaterial.color = new Color(0.2f, 0.8f, 1f, 1f);
        }
        if (tracerMaterial == null)
        {
            tracerMaterial = new Material(urpParticlesShader);
            tracerMaterial.color = new Color(0.7f, 0.9f, 1f, 1f);
        }
        if (glowMaterial == null)
        {
            glowMaterial = new Material(urpParticlesShader);
            glowMaterial.color = new Color(0.8f, 0.9f, 1f, 0.8f);
        }
        if (runeMaterial == null)
        {
            runeMaterial = ringMaterial;
        }
        if (goldenPalmMaterial == null)
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? urpParticlesShader;
            goldenPalmMaterial = new Material(unlit);
            goldenPalmMaterial.color = new Color(1f, 0.84f, 0.18f, 0.95f);
        }
        if (bloodMaterial == null)
        {
            bloodMaterial = new Material(urpParticlesShader);
            bloodMaterial.color = new Color(0.88f, 0.08f, 0.12f, 0.95f);
        }
    }

    private void SetupParticleSystems()
    {
        // 1. Hiệu ứng chém thường - Giọt máu tóe tươi sáng rõ nét (Blood Splatter)
        GameObject sparkObj = new GameObject("VFX_HitSparks");
        sparkObj.transform.SetParent(transform);
        sparkParticles = sparkObj.AddComponent<ParticleSystem>();
        ConfigureBloodSplatterSystem(sparkParticles, bloodMaterial != null ? bloodMaterial : sparkMaterial, 
            new Color(1f, 0.14f, 0.18f, 1f), new Color(0.60f, 0.03f, 0.08f, 0f), 0.22f, 2.8f, 5.0f, 0.14f, 0.22f, 1.2f);

        // 2. Bạo kích - Máu tóe bạo liệt đỏ rực rỡ (Crit Blood Burst)
        GameObject critSparksObj = new GameObject("VFX_CritSparks");
        critSparksObj.transform.SetParent(transform);
        critSparksParticles = critSparksObj.AddComponent<ParticleSystem>();
        ConfigureBloodSplatterSystem(critSparksParticles, bloodMaterial != null ? bloodMaterial : sparkMaterial, 
            new Color(1f, 0.22f, 0.12f, 1f), new Color(0.70f, 0.04f, 0.08f, 0f), 0.28f, 3.8f, 6.5f, 0.18f, 0.30f, 1.3f);

        // 3. Vết chém lóe sáng huyết quang (Blood Slash Flash)
        GameObject critCoreObj = new GameObject("VFX_CritCore");
        critCoreObj.transform.SetParent(transform);
        critCoreParticles = critCoreObj.AddComponent<ParticleSystem>();
        ConfigureBillboardFlashSystem(critCoreParticles, bloodSplatMaterial != null ? bloodSplatMaterial : glowMaterial, new Color(1f, 0.25f, 0.20f, 0.95f), 0.10f, 0.38f);

        // 4. Phá Giáp - Tia lửa vỡ giáp tích lũy (Prompt 02: Tinh thể kim sắc nhỏ gọn)
        GameObject armorBreakObj = new GameObject("VFX_ArmorBreakSparks");
        armorBreakObj.transform.SetParent(transform);
        armorBreakParticles = armorBreakObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(armorBreakParticles, sparkMaterial, new Color(1f, 0.75f, 0.25f, 1f), new Color(1f, 0.3f, 0.05f, 0f), 0.20f, 4f, 8f, 0.15f, 0.25f);

        // 5. Phá Giáp - Hộ thể Boss vỡ vụn (Prompt 02: Mảnh vỡ tinh thể kim cương xanh ngọc, không bị khối vuông)
        GameObject shieldBreakObj = new GameObject("VFX_ShieldBreak");
        shieldBreakObj.transform.SetParent(transform);
        shieldBreakParticles = shieldBreakObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(shieldBreakParticles, sparkMaterial, new Color(0.4f, 0.9f, 1f, 1f), new Color(0.1f, 0.4f, 1f, 0f), 0.25f, 5f, 10f, 0.18f, 0.30f);

        // 6. Phá Giáp - Vòng sóng chấn khi khiên vỡ (Prompt 02)
        GameObject shieldRingObj = new GameObject("VFX_ShieldRing");
        shieldRingObj.transform.SetParent(transform);
        shieldRingParticles = shieldRingObj.AddComponent<ParticleSystem>();
        ConfigureRingSystem(shieldRingParticles, ringMaterial, new Color(0.3f, 0.85f, 1f, 0.9f), 0.30f, 2.8f);

        // 7. Vệt cào/chém cận chiến của quái thường (Prompt 03)
        GameObject enemySwipeObj = new GameObject("VFX_EnemySwipe");
        enemySwipeObj.transform.SetParent(transform);
        enemySwipeParticles = enemySwipeObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(enemySwipeParticles, sparkMaterial, new Color(1f, 0.35f, 0.2f, 1f), new Color(1f, 0.1f, 0.05f, 0f), 0.14f, 5f, 9f, 0.12f, 0.24f);

        // 8. Hồn phách tan nhẹ sau khi quái chết (Prompt 03)
        GameObject dissolveObj = new GameObject("VFX_DeathDissolve");
        dissolveObj.transform.SetParent(transform);
        deathDissolveParticles = dissolveObj.AddComponent<ParticleSystem>();
        ConfigureUpwardSmokeSystem(deathDissolveParticles, glowMaterial, new Color(0.85f, 0.9f, 1f, 0.6f), new Color(0.4f, 0.5f, 0.8f, 0f), 0.45f, 0.35f, 0.65f);

        // 9. Boss tụ năng lượng lấy đà (Prompt 03)
        GameObject bossWindUpObj = new GameObject("VFX_BossWindUp");
        bossWindUpObj.transform.SetParent(transform);
        bossWindUpParticles = bossWindUpObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(bossWindUpParticles, sparkMaterial, new Color(1f, 0.4f, 0.1f, 1f), new Color(1f, 0.8f, 0.2f, 0f), 0.35f, 3f, 7f, 0.2f, 0.4f);

        // 10. Boss dộng đất uy lực (Prompt 03)
        GameObject bossImpactObj = new GameObject("VFX_BossGroundImpact");
        bossImpactObj.transform.SetParent(transform);
        bossImpactParticles = bossImpactObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(bossImpactParticles, sparkMaterial, new Color(1f, 0.3f, 0.1f, 1f), new Color(1f, 0.1f, 0.02f, 0f), 0.30f, 10f, 18f, 0.25f, 0.50f);

        GameObject bossWaveObj = new GameObject("VFX_BossShockwave");
        bossWaveObj.transform.SetParent(transform);
        bossShockwaveParticles = bossWaveObj.AddComponent<ParticleSystem>();
        ConfigureRingSystem(bossShockwaveParticles, ringMaterial, new Color(1f, 0.35f, 0.15f, 0.85f), 0.35f, 3.6f);

        // 11. Cột sáng hồi sinh / trùng sinh (Prompt 03)
        GameObject reviveObj = new GameObject("VFX_PlayerRevive");
        reviveObj.transform.SetParent(transform);
        reviveParticles = reviveObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(reviveParticles, critMaterial, new Color(1f, 0.95f, 0.4f, 1f), new Color(1f, 0.7f, 0.1f, 0f), 0.60f, 8f, 16f, 0.3f, 0.6f);

        // 12. Vòng ấn trận pháp phát động (Prompt 04)
        GameObject runeObj = new GameObject("VFX_FormationRune");
        runeObj.transform.SetParent(transform);
        formationRuneParticles = runeObj.AddComponent<ParticleSystem>();
        ConfigureRingSystem(formationRuneParticles, formationRingMaterial, Color.white, .55f, 1f);
        var impactObj = new GameObject("VFX_FormationImpact");
        impactObj.transform.SetParent(transform, false);
        formationImpactParticles = impactObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(formationImpactParticles, formationSparkMaterial, Color.white, Color.white, .36f, 3f, 8f, .15f, .28f);
        var impactMain = formationImpactParticles.main;
        impactMain.maxParticles = 96;

        // 13. Thuẫn lục giác Kim Cang Trận (Prompt 05)
        GameObject bellObj = new GameObject("VFX_GoldenBellShield");
        bellObj.transform.SetParent(transform);
        goldenBellParticles = bellObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(goldenBellParticles, formationSparkMaterial, new Color(1f, 0.88f, 0.35f, 1f), new Color(1f, 0.6f, 0.1f, 0f), 0.22f, 4f, 8f, 0.25f, 0.45f);

        // 14. Bụi nổ Ấn Chưởng (Prompt 06: Mote tròn kim quang mềm mại)
        GameObject dustObj = new GameObject("VFX_AnChuongDust");
        dustObj.transform.SetParent(transform);
        anChuongDustParticles = dustObj.AddComponent<ParticleSystem>();
        ConfigureUpwardSmokeSystem(anChuongDustParticles, glowMaterial, new Color(1f, 0.85f, 0.35f, 0.9f), new Color(0.8f, 0.5f, 0.15f, 0f), 0.45f, 0.35f, 0.65f);

        // 15. Sóng chấn 6m Lôi Phạt (Prompt 07)
        GameObject lpWaveObj = new GameObject("VFX_LoiPhatShockwave");
        lpWaveObj.transform.SetParent(transform);
        loiPhatShockwaveParticles = lpWaveObj.AddComponent<ParticleSystem>();
        ConfigureRingSystem(loiPhatShockwaveParticles, ringMaterial, new Color(1f, 0.9f, 0.35f, 0.95f), 0.35f, 6.0f);

        // 16. Tiêu biến quái thường khi bị Lôi Phạt đạo tiêu (Prompt 07)
        GameObject lpExecObj = new GameObject("VFX_LoiPhatExecute");
        lpExecObj.transform.SetParent(transform);
        loiPhatExecuteParticles = lpExecObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(loiPhatExecuteParticles, critMaterial, new Color(1f, 0.95f, 0.5f, 1f), new Color(1f, 0.4f, 0.1f, 0f), 0.35f, 10f, 18f, 0.25f, 0.50f);

        // 17. Biểu tượng ngôi sao / ấn choáng trên đầu quái (Prompt 06)
        GameObject stunObj = new GameObject("VFX_StunStars");
        stunObj.transform.SetParent(transform);
        stunParticles = stunObj.AddComponent<ParticleSystem>();
        ConfigureSparksSystem(stunParticles, sparkMaterial, new Color(1f, 0.9f, 0.3f, 1f), new Color(1f, 0.6f, 0.1f, 0f), 0.4f, 1f, 3f, 0.15f, 0.25f);

        // 18. Vòng ấn triệu hồi Vạn Kiếm Quy Tông (Prompt 09)
        GameObject vkObj = new GameObject("VFX_UltimateSummonRing");
        vkObj.transform.SetParent(transform);
        ultimateSummonParticles = vkObj.AddComponent<ParticleSystem>();
        ConfigureRingSystem(ultimateSummonParticles, ringMaterial, new Color(1f, 0.85f, 0.25f, 0.95f), 0.45f, 5.0f);

        // 19. Hào quang Pháp Khí trên người chơi (Prompt 10)
        GameObject auraObj = new GameObject("VFX_ArtifactAura");
        auraObj.transform.SetParent(transform);
        artifactAuraParticles = auraObj.AddComponent<ParticleSystem>();
        ConfigureUpwardSmokeSystem(artifactAuraParticles, glowMaterial, new Color(0.85f, 0.75f, 0.4f, 0.4f), new Color(0.9f, 0.6f, 0.2f, 0f), 0.6f, 0.2f, 0.45f);
    }

    private void SetupLinePools()
    {
        // 1. Pierce Lines
        pierceLines = new LineRenderer[PIERCE_LINE_POOL_SIZE];
        pierceLineTimers = new float[PIERCE_LINE_POOL_SIZE];
        GameObject pRoot = new GameObject("VFX_PierceTracers");
        pRoot.transform.SetParent(transform);
        for (int i = 0; i < PIERCE_LINE_POOL_SIZE; i++)
        {
            GameObject lineObj = new GameObject($"Tracer_{i}");
            lineObj.transform.SetParent(pRoot.transform);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = tracerMaterial;
            lr.startWidth = 0.16f;
            lr.endWidth = 0.04f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startColor = new Color(0.7f, 0.95f, 1f, 0.95f);
            lr.endColor = new Color(0.3f, 0.6f, 1f, 0f);
            lr.enabled = false;
            pierceLines[i] = lr;
            pierceLineTimers[i] = 0f;
        }

        // 2. Formation Thunder Lines (Prompt 04)
        formationThunderLines = new LineRenderer[THUNDER_LINE_POOL_SIZE];
        formationThunderTimers = new float[THUNDER_LINE_POOL_SIZE];
        GameObject tRoot = new GameObject("VFX_FormationThunderLines");
        tRoot.transform.SetParent(transform);
        for (int i = 0; i < THUNDER_LINE_POOL_SIZE; i++)
        {
            GameObject lineObj = new GameObject($"ThunderLine_{i}");
            lineObj.transform.SetParent(tRoot.transform);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = tracerMaterial;
            lr.startWidth = 0.14f;
            lr.endWidth = 0.05f;
            lr.positionCount = 4;
            lr.useWorldSpace = true;
            lr.startColor = new Color(0.85f, 0.70f, 1f, 0.95f);
            lr.endColor = new Color(0.55f, 0.25f, 1f, 0.1f);
            lr.enabled = false;
            formationThunderLines[i] = lr;
            formationThunderTimers[i] = 0f;
        }

        // 3. Formation Sword Rain Lines (Prompt 04)
        formationSwordRainLines = new LineRenderer[SWORD_RAIN_POOL_SIZE];
        formationSwordRainTimers = new float[SWORD_RAIN_POOL_SIZE];
        GameObject srRoot = new GameObject("VFX_FormationSwordRainLines");
        srRoot.transform.SetParent(transform);
        for (int i = 0; i < SWORD_RAIN_POOL_SIZE; i++)
        {
            GameObject lineObj = new GameObject($"SwordRainLine_{i}");
            lineObj.transform.SetParent(srRoot.transform);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = tracerMaterial;
            lr.startWidth = 0.18f;
            lr.endWidth = 0.04f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startColor = new Color(0.60f, 0.95f, 1f, 0.95f);
            lr.endColor = new Color(0.20f, 0.60f, 1f, 0f);
            lr.enabled = false;
            formationSwordRainLines[i] = lr;
            formationSwordRainTimers[i] = 0f;
        }

        // 4. Lifesteal Lines (Prompt 05)
        lifestealLines = new LineRenderer[LIFESTEAL_POOL_SIZE];
        lifestealTimers = new float[LIFESTEAL_POOL_SIZE];
        GameObject lsRoot = new GameObject("VFX_LifestealLines");
        lsRoot.transform.SetParent(transform);
        for (int i = 0; i < LIFESTEAL_POOL_SIZE; i++)
        {
            GameObject lineObj = new GameObject($"LifestealLine_{i}");
            lineObj.transform.SetParent(lsRoot.transform);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = glowMaterial;
            lr.startWidth = 0.18f;
            lr.endWidth = 0.05f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startColor = new Color(0.95f, 0.15f, 0.25f, 0.9f);
            lr.endColor = new Color(0.40f, 0.05f, 0.10f, 0f);
            lr.enabled = false;
            lifestealLines[i] = lr;
            lifestealTimers[i] = 0f;
        }

        // 5. Loi Phat Main & Branch Lines (Prompt 07)
        GameObject lpRoot = new GameObject("VFX_LoiPhatBolt");
        lpRoot.transform.SetParent(transform);
        GameObject mainBoltObj = new GameObject("LoiPhatMain");
        mainBoltObj.transform.SetParent(lpRoot.transform);
        loiPhatMainLine = mainBoltObj.AddComponent<LineRenderer>();
        loiPhatMainLine.material = tracerMaterial;
        loiPhatMainLine.startWidth = 0.32f;
        loiPhatMainLine.endWidth = 0.12f;
        loiPhatMainLine.positionCount = 5;
        loiPhatMainLine.useWorldSpace = true;
        loiPhatMainLine.startColor = new Color(1f, 0.98f, 0.75f, 1f);
        loiPhatMainLine.endColor = new Color(1f, 0.70f, 0.20f, 0.8f);
        loiPhatMainLine.enabled = false;

        loiPhatBranchLines = new LineRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject branchObj = new GameObject($"LoiPhatBranch_{i}");
            branchObj.transform.SetParent(lpRoot.transform);
            var lr = branchObj.AddComponent<LineRenderer>();
            lr.material = tracerMaterial;
            lr.startWidth = 0.12f;
            lr.endWidth = 0.04f;
            lr.positionCount = 3;
            lr.useWorldSpace = true;
            lr.startColor = new Color(1f, 0.85f, 0.4f, 0.8f);
            lr.endColor = new Color(1f, 0.5f, 0.1f, 0f);
            lr.enabled = false;
            loiPhatBranchLines[i] = lr;
        }
    }

    private void SetupPersistentVFXObjects()
    {
        // Bagua Sustain Object (Prompt 05)
        baguaSustainObj = new GameObject("VFX_BaguaSustain");
        baguaSustainObj.transform.SetParent(transform);
        baguaWispsParticles = baguaSustainObj.AddComponent<ParticleSystem>();
        ConfigureFormationSustain(baguaWispsParticles, FormationGlyphVFX.Palette(FormationId.Spirit), 1.45f, 18f, 1.3f, 2.8f);
        baguaSustainObj.SetActive(false);

        // Frost Mist Object (Prompt 05)
        frostMistObj = new GameObject("VFX_FrostMist");
        frostMistObj.transform.SetParent(transform);
        frostMistParticles = frostMistObj.AddComponent<ParticleSystem>();
        ConfigureFormationSustain(frostMistParticles, FormationGlyphVFX.Palette(FormationId.Frost), 2.4f, 14f, .45f, 1.25f);
        var frostCrown = FormationGlyphVFX.CreateLine(frostMistObj.transform, "Frost crown", tracerMaterial);
        FormationGlyphVFX.SetRing(frostCrown, 12, 2.2f, .58f, FormationGlyphVFX.Palette(FormationId.Frost), .09f);
        frostMistObj.SetActive(false);

        // Formation Array Aura (Vòng Trận Đồ Tu Tiên xoay dưới chân Player cho cả 6 trận)
        formationAuraObj = new GameObject("VFX_FormationArrayAura");
        formationAuraObj.transform.SetParent(transform);
        formationGlyph = new FormationGlyphVFX(formationAuraObj.transform, tracerMaterial);

        formationAuraParticles = formationAuraObj.AddComponent<ParticleSystem>();
        ConfigureFormationSustain(formationAuraParticles, Color.white, 1.9f, 8f, .35f, 1.2f);
        formationAuraObj.SetActive(false);

        goldenBellWard = FormationGlyphVFX.CreateLine(transform, "VFX_GoldenBellWard", tracerMaterial);
        goldenBellWard.positionCount = 6;
        goldenBellWard.loop = true;
        for (int i = 0; i < goldenBellWard.positionCount; i++)
        {
            float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
            goldenBellWard.SetPosition(i, new Vector3(Mathf.Cos(angle) * 1.38f, Mathf.Sin(angle) * 1.38f, 0f));
        }
    }

    private void ConfigureFormationSustain(ParticleSystem ps, Color color, float radius, float rate, float minSpeed, float maxSpeed)
    {
        ConfigureSparksSystem(ps, formationSparkMaterial, Color.white, Color.white, .95f, minSpeed, maxSpeed, .12f, .24f);
        var main = ps.main;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startColor = color;
        main.maxParticles = 24;
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.rotation = new Vector3(-90f, 0f, 0f);
        shape.radius = radius;
        shape.randomDirectionAmount = .22f;
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = .8f;
    }

    private void ConfigureBloodSplatterSystem(ParticleSystem ps, Material mat, Color startCol, Color endCol, float lifetime, float minSpeed, float maxSpeed, float minSize, float maxSize, float gravity = 1.0f)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = startCol;
        main.gravityModifier = gravity; // Trọng lực giúp giọt máu rơi cong tự nhiên
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 50f; // Góc quạt rộng 50 độ giúp giọt máu tỏa đều trên màn hình 9:16
        shape.radius = 0.15f;

        // Giọt máu thu nhỏ dần khi tan biến
        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1.0f);
        sizeCurve.AddKey(0.5f, 0.9f);
        sizeCurve.AddKey(1f, 0.35f);
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(startCol, 0f), 
                new GradientColorKey(Color.Lerp(startCol, endCol, 0.4f), 0.5f),
                new GradientColorKey(endCol, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0f), 
                new GradientAlphaKey(0.95f, 0.6f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        col.color = g;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material = mat;
        // Chế độ Billboard: Giữ trọn hình dáng giọt máu tròn sắc bén, nhỏ gọn, không bị kéo dài thô dạng chữ nhật
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void ConfigureSparksSystem(ParticleSystem ps, Material mat, Color startCol, Color endCol, float lifetime, float minSpeed, float maxSpeed, float minSize, float maxSize)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = startCol;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.15f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startCol, 0f), new GradientColorKey(endCol, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material = mat;
        // Chế độ Billboard: Giữ hình dáng ngôi sao 4 cánh / mảnh tinh thể sắc nhọn, không bị kéo giãn thành hộp chữ nhật
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void ConfigureUpwardSmokeSystem(ParticleSystem ps, Material mat, Color startCol, Color endCol, float lifetime, float minSize, float maxSize)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = startCol;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.35f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startCol, 0f), new GradientColorKey(endCol, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material = mat;
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void ConfigureBillboardFlashSystem(ParticleSystem ps, Material mat, Color col, float lifetime, float size)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = size;
        main.startColor = col;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var colOver = ps.colorOverLifetime;
        colOver.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new GradientColorKey[] { new GradientColorKey(col, 0f), new GradientColorKey(col, 1f) },
                   new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colOver.color = g;

        var psrFlash = ps.GetComponent<ParticleSystemRenderer>();
        psrFlash.material = mat;
        psrFlash.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void ConfigureRingSystem(ParticleSystem ps, Material mat, Color col, float lifetime, float targetSize)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = 0.2f;
        main.startColor = col;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = false;

        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.2f);
        curve.AddKey(1f, targetSize);
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var colOver = ps.colorOverLifetime;
        colOver.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new GradientColorKey[] { new GradientColorKey(col, 0f), new GradientColorKey(col, 1f) },
                   new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colOver.color = g;

        var psrRing = ps.GetComponent<ParticleSystemRenderer>();
        psrRing.material = mat;
        psrRing.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
    }

    private void Update()
    {
        // 1. Quản lý tuổi thọ của Pierce Tracers
        if (pierceLines != null)
        {
            for (int i = 0; i < PIERCE_LINE_POOL_SIZE; i++)
            {
                if (pierceLineTimers[i] > 0f)
                {
                    pierceLineTimers[i] -= Time.deltaTime;
                    if (pierceLineTimers[i] <= 0f && pierceLines[i] != null)
                    {
                        pierceLines[i].enabled = false;
                    }
                }
            }
        }

        // 2. Quản lý tuổi thọ của Formation Thunder Lines (0.55s với crackle và fade out rõ nét)
        if (formationThunderLines != null)
        {
            for (int i = 0; i < THUNDER_LINE_POOL_SIZE; i++)
            {
                if (formationThunderTimers[i] > 0f)
                {
                    formationThunderTimers[i] -= Time.deltaTime;
                    var lr = formationThunderLines[i];
                    if (lr != null)
                    {
                        if (formationThunderTimers[i] > 0.25f)
                        {
                            // Điện giật rung giật crackle
                            if (lr.positionCount >= 6)
                            {
                                Vector3 p2 = lr.GetPosition(2) + new Vector3(Random.Range(-0.06f, 0.06f), 0, Random.Range(-0.06f, 0.06f));
                                Vector3 p3 = lr.GetPosition(3) + new Vector3(Random.Range(-0.06f, 0.06f), 0, Random.Range(-0.06f, 0.06f));
                                lr.SetPosition(2, p2);
                                lr.SetPosition(3, p3);
                            }
                        }
                        else
                        {
                            // Fade out mượt mà trong 0.25s cuối
                            float alpha = Mathf.Clamp01(formationThunderTimers[i] / 0.25f);
                            lr.startWidth = ThunderWidth * alpha;
                            lr.endWidth = 0.13f * alpha;
                            lr.startColor = new Color(0.95f, 0.90f, 1f, alpha);
                            lr.endColor = new Color(0.65f, 0.35f, 1f, alpha * 0.85f);
                        }

                        if (formationThunderTimers[i] <= 0f)
                        {
                            lr.enabled = false;
                        }
                    }
                }
            }
        }

        // 3. Quản lý tuổi thọ của Formation Sword Rain Lines
        if (formationSwordRainLines != null)
        {
            for (int i = 0; i < SWORD_RAIN_POOL_SIZE; i++)
            {
                if (formationSwordRainTimers[i] > 0f)
                {
                    formationSwordRainTimers[i] -= Time.deltaTime;
                    var lr = formationSwordRainLines[i];
                    if (lr != null)
                    {
                        if (formationSwordRainTimers[i] <= 0.20f)
                        {
                            float alpha = Mathf.Clamp01(formationSwordRainTimers[i] / 0.20f);
                            lr.startWidth = RainWidth * alpha;
                            lr.endWidth = 0.07f * alpha;
                            lr.startColor = new Color(.55f, 1f, .94f, alpha);
                            lr.endColor = new Color(.23f, .86f, .75f, alpha);
                        }
                        if (formationSwordRainTimers[i] <= 0f)
                        {
                            lr.enabled = false;
                        }
                    }
                }
            }
        }

        // 4. Quản lý tuổi thọ của Lifesteal Lines
        if (lifestealLines != null)
        {
            for (int i = 0; i < LIFESTEAL_POOL_SIZE; i++)
            {
                if (lifestealTimers[i] > 0f)
                {
                    lifestealTimers[i] -= Time.deltaTime;
                    var lr = lifestealLines[i];
                    if (lr != null)
                    {
                        if (lifestealTimers[i] <= 0.15f)
                        {
                            float alpha = Mathf.Clamp01(lifestealTimers[i] / 0.15f);
                            lr.startWidth = SiphonWidth * alpha;
                            lr.endWidth = 0.07f * alpha;
                        }
                        if (lifestealTimers[i] <= 0f)
                        {
                            lr.enabled = false;
                        }
                    }
                }
            }
        }

        if (goldenBellWard != null && goldenBellWardTime > 0f)
        {
            goldenBellWardTime = Mathf.Max(0f, goldenBellWardTime - Time.deltaTime);
            float fade = goldenBellWardTime / GoldenBellWardDuration;
            var color = FormationGlyphVFX.Palette(FormationId.GoldenBell);
            color.a = fade;
            FormationGlyphVFX.SetStyle(goldenBellWard, color, .15f * fade);
            goldenBellWard.transform.localScale = Vector3.one * Mathf.Lerp(1.22f, .86f, fade * fade);
            goldenBellWard.enabled = goldenBellWardTime > 0f;
        }

        // 5. Quản lý tuổi thọ của Lôi Phạt
        if (loiPhatTimer > 0f)
        {
            loiPhatTimer -= Time.deltaTime;
            if (loiPhatTimer <= 0f)
            {
                if (loiPhatMainLine != null) loiPhatMainLine.enabled = false;
                if (loiPhatBranchLines != null)
                {
                    for (int i = 0; i < loiPhatBranchLines.Length; i++)
                        if (loiPhatBranchLines[i] != null) loiPhatBranchLines[i].enabled = false;
                }
            }
        }

        // 6. Theo dõi người chơi cho Âm Dương Trận
        if (amDuongTranTimer > 0f && amDuongTranObj != null)
        {
            amDuongTranTimer -= Time.deltaTime;
            if (amDuongTranTarget != null)
            {
                amDuongTranObj.transform.position = amDuongTranTarget.position + Vector3.up * 0.05f;
            }
            if (amDuongTranTimer <= 0f)
            {
                StopAmDuongTran();
            }
        }
    }

    /// <summary>
    /// Phát hiệu ứng chém trúng quái (Hit thường hoặc Chí mạng):
    /// Hiệu ứng tóe máu (Blood Splatter) sắc bén, giọt máu bắn theo hướng chém và rơi nhẹ tự nhiên theo trọng lực,
    /// kèm lóe sáng vết chém tại bề mặt tiếp xúc (không bị thân quái che khuất).
    /// </summary>
    public void SpawnHitImpact(Vector3 hitPoint, Vector3 swordDirection, bool isCrit)
    {
        EnsureInitialized();

        if (swordDirection.sqrMagnitude < 0.001f) swordDirection = Vector3.forward;
        swordDirection.y = 0f;
        swordDirection.Normalize();

        // Vector hướng về Camera (cho góc nhìn 9:16 nhìn từ trên chếch xuống)
        Vector3 camPos = Camera.main != null ? Camera.main.transform.position : new Vector3(0.36f, 6.99f, -12f);
        Vector3 dirToCam = (camPos - hitPoint).normalized;

        // Điểm sinh VFX nằm ở mặt trước quái hướng về phía Camera để hiển thị rõ nhất
        Vector3 spawnPos = hitPoint + dirToCam * 0.45f;

        // Vết chém quét ngang: vector vuông góc với hướng kiếm và hướng camera
        Vector3 lateral = Vector3.Cross(swordDirection, Vector3.up).normalized;
        if (lateral.sqrMagnitude < 0.001f) lateral = Vector3.right;

        // Hướng bắn tóe máu: Phun tỏa ra phía trước (về hướng camera), chếch lên trời và tỏa sang 2 bên chém
        // Tạo thành hình quạt máu (blood fan spray) cực kỳ rõ nét trên màn hình điện thoại
        Vector3 sprayDir = (dirToCam * 0.60f + Vector3.up * 0.50f + lateral * 0.35f).normalized;
        Quaternion rot = Quaternion.LookRotation(sprayDir);

        if (isCrit)
        {
            if (critSparksParticles != null)
            {
                critSparksParticles.transform.position = spawnPos;
                critSparksParticles.transform.rotation = rot;
                critSparksParticles.Emit(16);
            }
            if (critCoreParticles != null)
            {
                // Lóe sáng huyết quang bạo kích rực rỡ
                critCoreParticles.transform.position = spawnPos;
                var main = critCoreParticles.main;
                main.startColor = new Color(1f, 0.28f, 0.15f, 1f);
                main.startSize = 0.55f;
                critCoreParticles.Emit(1);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.08f, 0.05f);
            }
        }
        else
        {
            if (sparkParticles != null)
            {
                sparkParticles.transform.position = spawnPos;
                sparkParticles.transform.rotation = rot;
                sparkParticles.Emit(10);
            }
            if (critCoreParticles != null)
            {
                // Lóe nhẹ huyết sắc tại điểm chém
                critCoreParticles.transform.position = spawnPos;
                var main = critCoreParticles.main;
                main.startColor = new Color(1f, 0.15f, 0.20f, 0.95f);
                main.startSize = 0.38f;
                critCoreParticles.Emit(1);
            }
        }
    }

    /// <summary>
    /// KIẾM XUYÊN: Vẽ vệt kiếm khí tiếp nối từ mục tiêu chính sang mục tiêu phụ bị xuyên thủng.
    /// </summary>
    public void SpawnPierceTracer(Vector3 fromPos, Vector3 toPos)
    {
        if (pierceLines == null || pierceLines.Length == 0) return;
        int idx = currentPierceIndex;
        currentPierceIndex = (currentPierceIndex + 1) % PIERCE_LINE_POOL_SIZE;

        var lr = pierceLines[idx];
        if (lr != null)
        {
            lr.SetPosition(0, fromPos);
            lr.SetPosition(1, toPos);
            lr.enabled = true;
            pierceLineTimers[idx] = 0.12f;
        }
    }

    /// <summary>
    /// PHÁ GIÁP: Phát tín hiệu tích lũy rạn nứt giáp trên cùng một mục tiêu.
    /// </summary>
    public void SpawnArmorBreakStack(Vector3 targetPos, int stack)
    {
        if (armorBreakParticles != null)
        {
            armorBreakParticles.transform.position = targetPos;
            int burstCount = Mathf.Clamp(stack * 4, 4, 16);
            armorBreakParticles.Emit(burstCount);
        }
    }

    /// <summary>
    /// PHÁ GIÁP: Phát nổ vỡ vụn khi hộ thể Boss (OpeningBossGuard) chính thức giảm về 0!
    /// </summary>
    public void SpawnShieldBreakBurst(Vector3 targetPos)
    {
        if (shieldBreakParticles != null)
        {
            shieldBreakParticles.transform.position = targetPos;
            shieldBreakParticles.Emit(32);
        }
        if (shieldRingParticles != null)
        {
            shieldRingParticles.transform.position = targetPos;
            shieldRingParticles.Emit(1);
        }
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.35f, 0.15f);
        }
    }

    /// <summary>
    /// QUÁI THƯỜNG: Vệt vuốt chém ngắn khi quái ra đòn.
    /// </summary>
    public void SpawnEnemyAttackSwipe(Vector3 pos, Vector3 forwardDir)
    {
        if (enemySwipeParticles != null)
        {
            if (forwardDir.sqrMagnitude < 0.001f) forwardDir = Vector3.forward;
            enemySwipeParticles.transform.position = pos;
            enemySwipeParticles.transform.rotation = Quaternion.LookRotation(forwardDir);
            enemySwipeParticles.Emit(5);
        }
    }

    /// <summary>
    /// QUÁI THƯỜNG: Hồn phách tan nhẹ sau khi quái hoàn tất hoạt ảnh chết.
    /// </summary>
    public void SpawnEnemyDeathDissolve(Vector3 pos)
    {
        if (deathDissolveParticles != null)
        {
            deathDissolveParticles.transform.position = pos;
            deathDissolveParticles.Emit(8);
        }
    }

    /// <summary>
    /// BOSS: Vệt lấy đà trên tay/vũ khí tại giây 0.00 của chu kỳ tấn công.
    /// </summary>
    public void SpawnBossWindUp(Vector3 handPos)
    {
        if (bossWindUpParticles != null)
        {
            bossWindUpParticles.transform.position = handPos;
            bossWindUpParticles.Emit(12);
        }
    }

    /// <summary>
    /// BOSS: Va chạm chấn địa và sóng chấn tại giây 0.48 khi đòn đánh trúng đích.
    /// </summary>
    public void SpawnBossGroundImpact(Vector3 impactPos)
    {
        if (bossImpactParticles != null)
        {
            bossImpactParticles.transform.position = impactPos;
            bossImpactParticles.Emit(24);
        }
        if (bossShockwaveParticles != null)
        {
            bossShockwaveParticles.transform.position = impactPos;
            bossShockwaveParticles.Emit(1);
        }
    }

    /// <summary>
    /// NGƯỜI CHƠI: Cột sáng vàng kim lóe sáng khi kích hoạt trùng sinh / hồi sinh.
    /// </summary>
    public void SpawnReviveBurst(Vector3 playerPos)
    {
        if (reviveParticles != null)
        {
            reviveParticles.transform.position = playerPos + Vector3.up * 0.2f;
            reviveParticles.Emit(30);
        }
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.40f, 0.20f);
        }
    }

    // =========================================================================
    // PROMPT 04: CỬU THIÊN LÔI TRẬN & VẠN KIẾM QUY TÔNG TRẬN
    // =========================================================================

    /// <summary>
    /// Vòng ấn trận pháp mỏng lóe quanh người chơi khi phát động chu kỳ trận pháp.
    /// </summary>
    public void SpawnFormationRuneRing(Vector3 centerPos, Color color, float radius)
    {
        if (formationRuneParticles != null)
        {
            var ring = new ParticleSystem.EmitParams
            {
                position = centerPos + Vector3.up * .14f,
                startColor = color,
                startSize = radius * 2f
            };
            formationRuneParticles.Emit(ring, 1);
        }
    }

    private void EmitFormationImpact(Vector3 position, Color color, int count)
    {
        if (formationImpactParticles == null) return;
        formationImpactParticles.transform.position = position;
        formationImpactParticles.Emit(new ParticleSystem.EmitParams { startColor = color }, count);
    }

    /// <summary>
    /// CỬU THIÊN LÔI TRẬN: Sét tím trắng giáng mạnh mẽ từ trời xuống quái, crackle giật điện và tồn tại rõ ràng (0.55s).
    /// </summary>
    public void SpawnFormationThunder(Vector3 targetPos)
    {
        if (formationThunderLines == null || formationThunderLines.Length == 0) return;
        int idx = currentThunderIndex;
        currentThunderIndex = (currentThunderIndex + 1) % THUNDER_LINE_POOL_SIZE;

        var lr = formationThunderLines[idx];
        if (lr != null)
        {
            Vector3 top = targetPos + Vector3.up * 10f;
            Vector3 p1 = Vector3.Lerp(top, targetPos, 0.20f) + new Vector3(Random.Range(-0.35f, 0.35f), 0, Random.Range(-0.35f, 0.35f));
            Vector3 p2 = Vector3.Lerp(top, targetPos, 0.40f) + new Vector3(Random.Range(-0.45f, 0.45f), 0, Random.Range(-0.45f, 0.45f));
            Vector3 p3 = Vector3.Lerp(top, targetPos, 0.65f) + new Vector3(Random.Range(-0.40f, 0.40f), 0, Random.Range(-0.40f, 0.40f));
            Vector3 p4 = Vector3.Lerp(top, targetPos, 0.85f) + new Vector3(Random.Range(-0.25f, 0.25f), 0, Random.Range(-0.25f, 0.25f));

            lr.positionCount = 6;
            lr.SetPosition(0, top);
            lr.SetPosition(1, p1);
            lr.SetPosition(2, p2);
            lr.SetPosition(3, p3);
            lr.SetPosition(4, p4);
            lr.SetPosition(5, targetPos);

            lr.startWidth = ThunderWidth;
            lr.endWidth = 0.13f;
            lr.startColor = new Color(0.95f, 0.90f, 1f, 1f);
            lr.endColor = new Color(0.65f, 0.35f, 1f, 0.85f);
            lr.enabled = true;
            formationThunderTimers[idx] = 0.55f; // Tăng từ 0.14s lên 0.55s hiển thị rõ ràng, hoành tráng
        }

        // Bùng nổ lôi phù tím và tia sét tỏa đất
        Color lightning = FormationGlyphVFX.Palette(FormationId.Thunder);
        SpawnFormationRuneRing(targetPos, lightning, 1.45f);
        EmitFormationImpact(targetPos + Vector3.up * .25f, lightning, 12);
    }

    /// <summary>
    /// VẠN KIẾM QUY TÔNG TRẬN: Dải bóng kiếm xanh bạc chéo góc từ trời giáng xuống quái (0.42s).
    /// </summary>
    public void SpawnFormationSwordRain(Vector3 targetPos)
    {
        if (formationSwordRainLines == null || formationSwordRainLines.Length == 0) return;
        int idx = currentSwordRainIndex;
        currentSwordRainIndex = (currentSwordRainIndex + 1) % SWORD_RAIN_POOL_SIZE;

        var lr = formationSwordRainLines[idx];
        if (lr != null)
        {
            Vector3 startPos = targetPos + new Vector3(-4.2f, 8.4f, 1.4f);
            Vector3 bladeBase = Vector3.Lerp(startPos, targetPos, .78f);
            Vector3 side = Vector3.Cross(Vector3.up, targetPos - startPos).normalized * .42f;
            lr.positionCount = 7;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, bladeBase);
            lr.SetPosition(2, bladeBase + side);
            lr.SetPosition(3, targetPos);
            lr.SetPosition(4, bladeBase - side);
            lr.SetPosition(5, bladeBase);
            lr.SetPosition(6, targetPos);
            lr.startWidth = RainWidth;
            lr.endWidth = .07f;
            lr.startColor = new Color(.64f, 1f, .94f);
            lr.endColor = new Color(.28f, .91f, .78f);
            lr.enabled = true;
            formationSwordRainTimers[idx] = 0.42f; // Tăng từ 0.12s lên 0.42s
        }

        EmitFormationImpact(targetPos, FormationGlyphVFX.Palette(FormationId.SwordRain), 8);
    }

    // =========================================================================
    // PROMPT 05: BỐN TRẬN HỖ TRỢ (TỤ LINH, CỰC HÀN, HUYẾT MA, KIM CANG)
    // =========================================================================

    /// <summary>
    /// VÒNG TRẬN ĐỒ TU TIÊN TOÀN DIỆN CHO CẢ 6 ĐẠI TRẬN PHÁP (DƯỚI CHÂN PLAYER)
    /// </summary>
    public void UpdateFormationVisuals(FormationId id, bool spiritActive, Vector3 playerPos)
    {
        if (formationAuraObj == null) return;
        if (!formationAuraObj.activeSelf) formationAuraObj.SetActive(true);
        formationGlyph.Show(id, spiritActive, playerPos);
        var main = formationAuraParticles.main;
        main.startColor = FormationGlyphVFX.Palette(id);
        if (!formationAuraParticles.isPlaying) formationAuraParticles.Play();
    }

    public void DisableFormationVisuals()
    {
        if (formationAuraObj != null && formationAuraObj.activeSelf)
        {
            formationAuraParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            formationAuraObj.SetActive(false);
        }
        SetBaguaSustain(false, Vector3.zero);
        SetFrostMist(false, Vector3.zero);
        goldenBellWardTime = 0f;
        if (goldenBellWard != null) goldenBellWard.enabled = false;
    }

    /// <summary>
    /// BÁT QUÁI TỤ LINH TRẬN: Vòng bát quái xanh lục xoay nhẹ dưới chân và linh khí hội tụ.
    /// </summary>
    public void SetBaguaSustain(bool active, Vector3 playerPos)
    {
        if (baguaSustainObj != null)
        {
            if (active)
            {
                baguaSustainObj.SetActive(true);
                baguaSustainObj.transform.position = playerPos + Vector3.up * 0.12f;
                baguaSustainObj.transform.Rotate(Vector3.up * 40f * Time.deltaTime);
                if (baguaWispsParticles != null && !baguaWispsParticles.isPlaying)
                {
                    baguaWispsParticles.Play();
                }
            }
            else
            {
                if (baguaWispsParticles != null && baguaWispsParticles.isPlaying)
                {
                    baguaWispsParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                baguaSustainObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// CỰC HÀN BĂNG TRẬN: Quầng sương băng giá và tinh thể tuyết sát chân Boss bị làm chậm.
    /// </summary>
    public void SetFrostMist(bool active, Vector3 bossPos)
    {
        if (frostMistObj != null)
        {
            if (active)
            {
                frostMistObj.SetActive(true);
                frostMistObj.transform.position = bossPos + Vector3.up * .14f;
                frostMistObj.transform.rotation = Quaternion.Euler(0f, Time.time * -32f, 0f);
                if (frostMistParticles != null && !frostMistParticles.isPlaying)
                {
                    frostMistParticles.Play();
                }
            }
            else
            {
                if (frostMistParticles != null && frostMistParticles.isPlaying)
                {
                    frostMistParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                frostMistObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// HUYẾT MA HÓA SINH TRẬN: Vệt đỏ rượu kéo từ vị trí chém về người chơi khi hút máu thành công.
    /// </summary>
    public void SpawnLifestealStreak(Vector3 fromPos, Vector3 toPos)
    {
        if (Time.time - lastLifestealTime < 0.16f) return; // Gom nhiều đòn kiếm gần nhau.
        lastLifestealTime = Time.time;

        if (lifestealLines == null || lifestealLines.Length == 0) return;
        int idx = currentLifestealIndex;
        currentLifestealIndex = (currentLifestealIndex + 1) % LIFESTEAL_POOL_SIZE;

        var lr = lifestealLines[idx];
        if (lr != null)
        {
            Vector3 start = fromPos + Vector3.up * .36f;
            Vector3 end = toPos + Vector3.up * .8f;
            lr.positionCount = 4;
            lr.SetPosition(0, start);
            lr.SetPosition(1, Vector3.Lerp(start, end, .30f) + Vector3.up * .72f);
            lr.SetPosition(2, Vector3.Lerp(start, end, .72f) + Vector3.up * 1.25f);
            lr.SetPosition(3, end);
            lr.startColor = new Color(.88f, .10f, .26f, .84f);
            lr.endColor = new Color(1f, .46f, .54f);
            lr.startWidth = SiphonWidth;
            lr.endWidth = .07f;
            lr.enabled = true;
            lifestealTimers[idx] = 0.38f; // Tăng từ 0.16s lên 0.38s hiển thị dòng huyết khí rõ nét
        }

        EmitFormationImpact(toPos + Vector3.up * .8f, FormationGlyphVFX.Palette(FormationId.BloodDrain), 5);
    }

    /// <summary>
    /// HUYỀN VŨ KIM CANG TRẬN: Thuẫn lục giác vàng nhạt lóe tại phía nhận đòn giảm sát thương.
    /// </summary>
    public void SpawnGoldenBellShield(Vector3 playerPos, Vector3 hitDir)
    {
        if (goldenBellWardTime > .22f) return;
        hitDir.y = 0f;
        hitDir = hitDir.sqrMagnitude >= .001f ? hitDir.normalized : Vector3.forward;
        Vector3 wardCenter = playerPos + hitDir * .95f + Vector3.up * 1.3f;
        Quaternion facing = Quaternion.LookRotation(hitDir);
        if (goldenBellWard != null)
        {
            goldenBellWard.transform.SetPositionAndRotation(wardCenter, facing);
            goldenBellWard.transform.localScale = Vector3.one * .86f;
            FormationGlyphVFX.SetStyle(goldenBellWard, FormationGlyphVFX.Palette(FormationId.GoldenBell), .15f);
            goldenBellWard.enabled = true;
            goldenBellWardTime = GoldenBellWardDuration;
        }
        if (goldenBellParticles != null)
        {
            goldenBellParticles.transform.SetPositionAndRotation(wardCenter, facing);
            goldenBellParticles.Emit(14);
        }
    }

    // =========================================================================
    // PROMPT 06: ẤN CHƯỞNG
    // =========================================================================

    /// <summary>
    /// ẤN CHƯỞNG: Bàn tay hổ phách khổng lồ giáng xuống đất đúng tick sát thương,
    /// kèm vòng bụi đất và sóng chấn nứt nẻ.
    /// </summary>
    public void SpawnAnChuong(Vector3 targetPos, float radius)
    {
        // 1. Vòng bụi đất bốc lên
        if (anChuongDustParticles != null)
        {
            anChuongDustParticles.transform.position = targetPos + Vector3.up * 0.05f;
            anChuongDustParticles.Emit(28);
        }

        // 2. Sóng chấn lan rộng theo bán kính chiêu
        if (shieldRingParticles != null)
        {
            shieldRingParticles.transform.position = targetPos + Vector3.up * 0.05f;
            shieldRingParticles.Emit(1);
        }

        // 3. Rung chấn camera
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.30f, 0.18f);
        }

        // 4. Bàn tay hổ phách đập xuống
        StartCoroutine(AnChuongPalmRoutine(targetPos));
    }

    private IEnumerator AnChuongPalmRoutine(Vector3 targetPos)
    {
        if (giantHandObj == null)
        {
            giantHandObj = new GameObject("VFX_GiantHandPalm");
            giantHandObj.transform.SetParent(transform);

            Material palmMat = goldenPalmMaterial != null ? goldenPalmMaterial : sparkMaterial;

            // 1. Khối lòng bàn tay bầu dục mềm mại (Smooth Ellipsoid - Thay thế hoàn toàn khối vuông cũ)
            GameObject palm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            palm.name = "GoldenPalm_Center";
            palm.transform.SetParent(giantHandObj.transform, false);
            palm.transform.localPosition = Vector3.zero;
            palm.transform.localScale = new Vector3(2.8f, 0.35f, 2.4f);
            Destroy(palm.GetComponent<Collider>());
            var rend = palm.GetComponent<MeshRenderer>();
            if (rend != null) rend.sharedMaterial = palmMat;

            // 2. Ấn ký hoàng kim trung tâm bàn tay (Golden Palm Seal Disc)
            GameObject sealDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sealDisc.name = "GoldenPalm_Seal";
            sealDisc.transform.SetParent(giantHandObj.transform, false);
            sealDisc.transform.localPosition = new Vector3(0f, 0.19f, 0f);
            sealDisc.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);
            Destroy(sealDisc.GetComponent<Collider>());
            var sealRend = sealDisc.GetComponent<MeshRenderer>();
            if (sealRend != null) sealRend.sharedMaterial = palmMat;

            // 3. 4 ngón tay thon dài xòe hình quạt uy lực (Golden Fingers)
            float[] fingerX = new float[] { -1.05f, -0.35f, 0.35f, 1.05f };
            float[] fingerLengths = new float[] { 1.35f, 1.65f, 1.50f, 1.20f };
            float[] fingerAngles = new float[] { -6f, -1f, 3f, 8f };

            for (int i = 0; i < 4; i++)
            {
                GameObject f = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                f.name = $"GoldenPalm_Finger_{i}";
                f.transform.SetParent(giantHandObj.transform, false);
                f.transform.localPosition = new Vector3(fingerX[i], 0f, 1.2f + fingerLengths[i] * 0.45f);
                f.transform.localScale = new Vector3(0.48f, fingerLengths[i] * 0.5f, 0.48f);
                f.transform.localRotation = Quaternion.Euler(90f, fingerAngles[i], 0f);
                Destroy(f.GetComponent<Collider>());
                var r = f.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = palmMat;
            }

            // 4. Ngón cái xòe góc uy nghi (Golden Thumb)
            GameObject thumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thumb.name = "GoldenPalm_Thumb";
            thumb.transform.SetParent(giantHandObj.transform, false);
            thumb.transform.localPosition = new Vector3(1.75f, 0f, -0.15f);
            thumb.transform.localScale = new Vector3(0.52f, 0.70f, 0.52f);
            thumb.transform.localRotation = Quaternion.Euler(90f, 42f, 0f);
            Destroy(thumb.GetComponent<Collider>());
            var tr = thumb.GetComponent<MeshRenderer>();
            if (tr != null) tr.sharedMaterial = palmMat;
        }

        giantHandObj.SetActive(true);
        giantHandObj.transform.position = targetPos + Vector3.up * 4.8f;
        giantHandObj.transform.rotation = Quaternion.Euler(20f, 0, 0);

        float elapsed = 0f;
        float duration = 0.22f;
        Vector3 start = targetPos + Vector3.up * 4.8f;
        Vector3 end = targetPos + Vector3.up * 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            giantHandObj.transform.position = Vector3.Lerp(start, end, t * t);
            yield return null;
        }

        giantHandObj.transform.position = end;

        // Bùng nổ sóng chấn hoàng kim khi kim chưởng tiếp đất
        if (shieldRingParticles != null)
        {
            shieldRingParticles.transform.position = end + Vector3.up * 0.05f;
            var main = shieldRingParticles.main;
            main.startColor = new Color(1f, 0.84f, 0.20f, 0.95f);
            shieldRingParticles.Emit(1);
        }

        yield return new WaitForSeconds(0.20f);
        giantHandObj.SetActive(false);
    }

    /// <summary>
    /// Gắn ấn choáng nhỏ lơ lửng trên đầu quái thường trong thời gian choáng.
    /// </summary>
    public void SpawnStunIndicator(GameObject targetEnemy, float duration)
    {
        if (targetEnemy != null && stunParticles != null)
        {
            stunParticles.transform.position = targetEnemy.transform.position + Vector3.up * 1.6f;
            stunParticles.Emit(8);
        }
    }

    // =========================================================================
    // PROMPT 07: LÔI PHẠT
    // =========================================================================

    /// <summary>
    /// LÔI PHẠT: Một tia sấm sét vàng trắng cực dày giáng xuống mục tiêu chính,
    /// 3 nhánh phụ, sóng xung kích 6m lan sát mặt đất.
    /// </summary>
    public void SpawnLoiPhat(Vector3 targetPos, bool isBoss, bool executeRegular)
    {
        // 1. Thân sét chính
        if (loiPhatMainLine != null)
        {
            Vector3 top = targetPos + Vector3.up * 12f;
            loiPhatMainLine.SetPosition(0, top);
            loiPhatMainLine.SetPosition(1, Vector3.Lerp(top, targetPos, 0.25f) + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)));
            loiPhatMainLine.SetPosition(2, Vector3.Lerp(top, targetPos, 0.50f) + new Vector3(Random.Range(-0.4f, 0.4f), 0, Random.Range(-0.4f, 0.4f)));
            loiPhatMainLine.SetPosition(3, Vector3.Lerp(top, targetPos, 0.75f) + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f)));
            loiPhatMainLine.SetPosition(4, targetPos);
            loiPhatMainLine.enabled = true;
        }

        // 2. Nhánh phụ
        if (loiPhatBranchLines != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var br = loiPhatBranchLines[i];
                if (br != null)
                {
                    float heightT = 0.3f + i * 0.2f;
                    Vector3 brStart = Vector3.Lerp(targetPos + Vector3.up * 12f, targetPos, heightT);
                    Vector3 brMid = brStart + new Vector3(Random.Range(-1.5f, 1.5f), -1.5f, Random.Range(-1.5f, 1.5f));
                    Vector3 brEnd = brMid + new Vector3(Random.Range(-0.8f, 0.8f), -1.2f, Random.Range(-0.8f, 0.8f));
                    br.SetPosition(0, brStart);
                    br.SetPosition(1, brMid);
                    br.SetPosition(2, brEnd);
                    br.enabled = true;
                }
            }
        }
        loiPhatTimer = 0.20f;

        // 3. Sóng chấn 6m
        if (loiPhatShockwaveParticles != null)
        {
            loiPhatShockwaveParticles.transform.position = targetPos + Vector3.up * 0.05f;
            loiPhatShockwaveParticles.Emit(1);
        }

        // 4. Phản hồi mục tiêu
        if (executeRegular && loiPhatExecuteParticles != null)
        {
            loiPhatExecuteParticles.transform.position = targetPos;
            loiPhatExecuteParticles.Emit(24);
        }
        else if (isBoss && critCoreParticles != null)
        {
            critCoreParticles.transform.position = targetPos + Vector3.up * 0.8f;
            critCoreParticles.Emit(2);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.35f, 0.22f);
        }
    }

    // =========================================================================
    // PROMPT 08: ÂM DƯƠNG TRẬN
    // =========================================================================

    /// <summary>
    /// Khởi động vòng trận Thái Cực Đen - Trắng bám theo người chơi, gồm vòng đồ Thái Cực mặt đất
    /// cùng 2 luồng Bạch Quang (Dương) và Hắc Quang (Âm) xoay chuyển hài hòa.
    /// </summary>
    public void StartAmDuongTran(Transform player, float duration)
    {
        amDuongTranTarget = player;
        amDuongTranTimer = duration;

        if (amDuongTranObj == null)
        {
            amDuongTranObj = new GameObject("VFX_AmDuongTranSustain");
            amDuongTranObj.transform.SetParent(transform);
            var auraVfx = amDuongTranObj.AddComponent<AuraVFX>();
            auraVfx.SetupYinYang();
        }
        else
        {
            var auraVfx = amDuongTranObj.GetComponent<AuraVFX>();
            if (auraVfx != null && amDuongTranObj.transform.childCount == 0)
            {
                auraVfx.SetupYinYang();
            }
        }
        amDuongTranObj.SetActive(true);
        if (player != null) amDuongTranObj.transform.position = player.position + Vector3.up * 0.05f;
    }

    public void StopAmDuongTran()
    {
        amDuongTranTimer = 0f;
        if (amDuongTranObj != null)
        {
            amDuongTranObj.SetActive(false);
        }
    }

    // =========================================================================
    // PROMPT 09: VẠN KIẾM QUY TÔNG
    // =========================================================================

    /// <summary>
    /// Vòng ấn triệu hồi vàng trắng bung ra khi kích hoạt Vạn Kiếm Quy Tông.
    /// </summary>
    public void SpawnUltimateSummonRing(Vector3 playerPos)
    {
        if (ultimateSummonParticles != null)
        {
            ultimateSummonParticles.transform.position = playerPos + Vector3.up * 0.05f;
            ultimateSummonParticles.Emit(1);
        }
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.25f, 0.12f);
        }
    }

    /// <summary>
    /// Dấu hiệu thu hồi ngắn tại người chơi khi hết mana / kết thúc Vạn Kiếm.
    /// </summary>
    public void SpawnUltimateRecallPulse(Vector3 playerPos)
    {
        if (formationRuneParticles != null)
        {
            formationRuneParticles.transform.position = playerPos + Vector3.up * 0.05f;
            var main = formationRuneParticles.main;
            main.startColor = new Color(1f, 0.85f, 0.3f, 0.9f);
            formationRuneParticles.Emit(1);
        }
    }

    // =========================================================================
    // PROMPT 10: TIẾN TRÌNH & HÀO QUANG PHÁP KHÍ
    // =========================================================================

    /// <summary>
    /// Điều chỉnh hào quang Pháp Khí trên nhân vật theo cấp độ AuraTier (0..4).
    /// </summary>
    public void SetPlayerArtifactAura(Transform player, int auraTier)
    {
        if (artifactAuraParticles == null || player == null) return;
        if (auraTier <= 0)
        {
            if (artifactAuraParticles.isPlaying) artifactAuraParticles.Stop();
            return;
        }

        artifactAuraParticles.transform.position = player.position + Vector3.up * 0.1f;
        var main = artifactAuraParticles.main;
        if (auraTier == 1)
        {
            main.startColor = new Color(0.7f, 0.85f, 1f, 0.35f);
        }
        else if (auraTier == 2)
        {
            main.startColor = new Color(0.4f, 1f, 0.7f, 0.45f);
        }
        else if (auraTier == 3)
        {
            main.startColor = new Color(0.9f, 0.6f, 1f, 0.55f);
        }
        else
        {
            main.startColor = new Color(1f, 0.85f, 0.3f, 0.65f);
        }

        if (!artifactAuraParticles.isPlaying)
        {
            artifactAuraParticles.Play();
        }
    }
}

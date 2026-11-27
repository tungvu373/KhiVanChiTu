using UnityEngine;
using UnityEngine.Rendering;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Stylized Xianxia Immortal Cloud Cushion ("Đám Mây Tiên") beneath the meditating cultivator.
    /// Features:
    /// - Sculpted low-poly 3D swirling cloud pedestal (~1.6m diameter, nests seamlessly inside the ~3.2m Formation Ring)
    /// - 2 ultra-light Shuriken particle layers (soft wispy cloud billows + ascending spirit motes) with zero hard squares/circles
    /// - Full Realm-based color evolution (Luyện Khí white -> Kim Đan gold -> Nguyên Anh azure -> Hóa Thần violet-gold -> Ngũ Sắc)
    /// - Synchronized gentle bobbing (+0.28m ride height), Tribulation golden-violet vortex lift, and Death dispersion/reform
    /// - Automatic CodexDrop/VFX texture override support for Terra 6.1 assets & WebGL/iPhone Safari density scaling
    /// </summary>
    public sealed class RemakeCloudCushionVfx : MonoBehaviour
    {
        // Cloud pedestal top center sits at local y ~ 0.45m; user-tuned cloud local offset (0.012, 0.224, 0.105) nests the plush cloud right against the seated cultivator's hips!
        public const float BaseSeatLocalY = 0.34f;
        public static readonly Vector3 BaseCloudLocalOffset = new Vector3(0.012f, 0.224f, 0.105f);

        [SerializeField] private Transform cloudMeshTransform;
        [SerializeField] private MeshRenderer cloudMeshRenderer;
        [SerializeField] private ParticleSystem cloudWispsPs;
        [SerializeField] private ParticleSystem spiritMotesPs;

        private Material _cloudMat;
        private Material _wispMat;
        private Material _moteMat;
        private int _realmIndex;
        private bool _inTribulation;
        private float _tribBlend;
        private float _dissolveAmount; // 0 = full solid cloud, 1 = dispersed on death
        private float _bobPhase;
        private Vector3 _baseMeshScale = Vector3.one;
        private float _densityFactor = 1f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public float CurrentSeatLocalY { get; private set; } = BaseSeatLocalY;

        public void InitializeRuntime(Mesh cloudMesh)
        {
            if (cloudMeshTransform == null)
            {
                var meshGo = new GameObject("CloudPedestalMesh");
                meshGo.transform.SetParent(transform, false);
                meshGo.transform.localPosition = Vector3.zero;
                meshGo.transform.localRotation = Quaternion.identity;
                meshGo.transform.localScale = Vector3.one;
                cloudMeshTransform = meshGo.transform;

                var mf = meshGo.AddComponent<MeshFilter>();
                mf.sharedMesh = cloudMesh != null ? cloudMesh : BuildFallbackCloudMesh();
                cloudMeshRenderer = meshGo.AddComponent<MeshRenderer>();
                cloudMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                cloudMeshRenderer.receiveShadows = false;
            }
            _baseMeshScale = cloudMeshTransform.localScale;
            EnsureMaterialsAndParticles();
            ApplyGraphicsDensity(SaveService.Current != null ? SaveService.Current.graphics.vfxDensity : RemakeVfxDensity.Full);
            SyncRealm(SaveService.Current != null ? SaveService.Current.realmIndex : 0);
        }

        private void Awake()
        {
            if (cloudMeshTransform != null)
                _baseMeshScale = cloudMeshTransform.localScale;
            EnsureMaterialsAndParticles();
            ApplyGraphicsDensity(SaveService.Current != null ? SaveService.Current.graphics.vfxDensity : RemakeVfxDensity.Full);
            SyncRealm(SaveService.Current != null ? SaveService.Current.realmIndex : 0);
        }

        private void OnDestroy()
        {
            if (_cloudMat != null) Destroy(_cloudMat);
            if (_wispMat != null) Destroy(_wispMat);
            if (_moteMat != null) Destroy(_moteMat);
        }

        private void EnsureMaterialsAndParticles()
        {
            Texture2D albedoTex = CodexArtLoader.LoadCodexTexture("VFX/tex_cloud_cushion_albedo")
                ?? Resources.Load<Texture2D>("VFX/Tex_CloudPedestal_Base");
            Texture2D puffTex = CodexArtLoader.LoadCodexTexture("VFX/tex_cloud_puff_soft")
                ?? Resources.Load<Texture2D>("VFX/Tex_CloudPuff_Soft");

            if (cloudMeshRenderer != null && _cloudMat == null)
            {
                if (cloudMeshRenderer.sharedMaterial != null)
                {
                    _cloudMat = new Material(cloudMeshRenderer.sharedMaterial);
                    _cloudMat.name = "Mat_CloudCushion_Runtime";
                }
                else
                {
                    Shader lit = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    _cloudMat = new Material(lit);
                    _cloudMat.name = "Mat_CloudCushion_Runtime";
                }
                if (albedoTex != null)
                {
                    _cloudMat.mainTexture = albedoTex;
                    if (_cloudMat.HasProperty("_BaseMap")) _cloudMat.SetTexture("_BaseMap", albedoTex);
                }
                if (_cloudMat.HasProperty("_Smoothness")) _cloudMat.SetFloat("_Smoothness", 0.05f);
                if (_cloudMat.HasProperty("_EmissionColor"))
                    _cloudMat.EnableKeyword("_EMISSION");

                cloudMeshRenderer.sharedMaterial = _cloudMat;
                cloudMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                cloudMeshRenderer.receiveShadows = false;
            }

            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");

            if (cloudWispsPs == null)
            {
                var wGo = new GameObject("CloudSwirlWisps");
                wGo.transform.SetParent(transform, false);
                wGo.transform.localPosition = new Vector3(0f, 0.25f, 0f);
                cloudWispsPs = wGo.AddComponent<ParticleSystem>();
                ConfigureWispParticles(cloudWispsPs, pShader, puffTex);
            }
            else if (_wispMat == null)
            {
                ConfigureWispMaterial(cloudWispsPs.GetComponent<ParticleSystemRenderer>(), pShader, puffTex);
            }

            if (spiritMotesPs == null)
            {
                var mGo = new GameObject("CloudSpiritMotes");
                mGo.transform.SetParent(transform, false);
                mGo.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                spiritMotesPs = mGo.AddComponent<ParticleSystem>();
                ConfigureMoteParticles(spiritMotesPs, pShader, puffTex);
            }
            else if (_moteMat == null)
            {
                ConfigureMoteMaterial(spiritMotesPs.GetComponent<ParticleSystemRenderer>(), pShader, puffTex);
            }
        }

        private void ConfigureWispMaterial(ParticleSystemRenderer pr, Shader pShader, Texture2D puffTex)
        {
            if (pr == null) return;
            _wispMat = new Material(pShader);
            _wispMat.name = "Mat_CloudWisp_Runtime";
            if (puffTex != null)
            {
                _wispMat.mainTexture = puffTex;
                if (_wispMat.HasProperty("_BaseMap")) _wispMat.SetTexture("_BaseMap", puffTex);
            }
            if (_wispMat.HasProperty("_Surface"))
            {
                _wispMat.SetFloat("_Surface", 1f);
                _wispMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                _wispMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                _wispMat.SetInt("_ZWrite", 0);
                _wispMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            pr.sharedMaterial = _wispMat;
            pr.shadowCastingMode = ShadowCastingMode.Off;
            pr.receiveShadows = false;
            pr.sortingFudge = 2f;
        }

        private void ConfigureMoteMaterial(ParticleSystemRenderer pr, Shader pShader, Texture2D puffTex)
        {
            if (pr == null) return;
            _moteMat = new Material(pShader);
            _moteMat.name = "Mat_CloudMote_Runtime";
            if (puffTex != null)
            {
                _moteMat.mainTexture = puffTex;
                if (_moteMat.HasProperty("_BaseMap")) _moteMat.SetTexture("_BaseMap", puffTex);
            }
            if (_moteMat.HasProperty("_Surface"))
            {
                _moteMat.SetFloat("_Surface", 1f);
                _moteMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                _moteMat.SetInt("_DstBlend", (int)BlendMode.One); // Soft additive shimmer
                _moteMat.SetInt("_ZWrite", 0);
                _moteMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            pr.sharedMaterial = _moteMat;
            pr.shadowCastingMode = ShadowCastingMode.Off;
            pr.receiveShadows = false;
        }

        private void ConfigureWispParticles(ParticleSystem ps, Shader pShader, Texture2D puffTex)
        {
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.20f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.72f, 1.18f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 48;

            var em = ps.emission;
            em.rateOverTime = 16.0f;

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Donut;
            sh.radius = 0.64f;
            sh.donutRadius = 0.26f;
            sh.rotation = new Vector3(90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(0f);
            vel.y = new ParticleSystem.MinMaxCurve(0.08f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            vel.orbitalX = new ParticleSystem.MinMaxCurve(0f);
            vel.orbitalY = new ParticleSystem.MinMaxCurve(0.62f);
            vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
            vel.radial = new ParticleSystem.MinMaxCurve(0.10f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.80f, 0.26f), new GradientAlphaKey(0.60f, 0.68f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.65f),
                new Keyframe(0.45f, 1.08f),
                new Keyframe(1f, 1.32f)));

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);

            ConfigureWispMaterial(ps.GetComponent<ParticleSystemRenderer>(), pShader, puffTex);
            ps.Play();
        }

        private void ConfigureMoteParticles(ParticleSystem ps, Shader pShader, Texture2D puffTex)
        {
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.14f);
            main.maxParticles = 24;

            var em = ps.emission;
            em.rateOverTime = 6.0f;

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Circle;
            sh.radius = 0.72f;
            sh.rotation = new Vector3(-90f, 0f, 0f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.30f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            ConfigureMoteMaterial(ps.GetComponent<ParticleSystemRenderer>(), pShader, puffTex);
            ps.Play();
        }

        public static void GetRealmCloudColors(int realmIndex, float timeSec, out Color bodyColor, out Color rimGlowColor)
        {
            int r = Mathf.Clamp(realmIndex, 0, 8);
            switch (r)
            {
                case 0: // Luyện Khí — Sunlit Ivory-White
                    bodyColor = new Color(1.00f, 0.99f, 0.97f, 1f);
                    rimGlowColor = new Color(0.86f, 0.96f, 0.92f, 1f);
                    break;
                case 1: // Trúc Cơ — Pearl Jade-Mint
                    bodyColor = new Color(0.92f, 0.99f, 0.96f, 1f);
                    rimGlowColor = new Color(0.46f, 0.88f, 0.74f, 1f);
                    break;
                case 2: // Kim Đan — Radiant Sunlit Gold
                    bodyColor = new Color(1.00f, 0.91f, 0.64f, 1f);
                    rimGlowColor = new Color(1.00f, 0.76f, 0.20f, 1f);
                    break;
                case 3: // Nguyên Anh — Celestial Azure-Jade
                    bodyColor = new Color(0.78f, 0.94f, 1.00f, 1f);
                    rimGlowColor = new Color(0.28f, 0.80f, 1.00f, 1f);
                    break;
                case 4: // Hóa Thần — Imperial Amethyst-Gold
                    bodyColor = new Color(0.92f, 0.83f, 1.00f, 1f);
                    rimGlowColor = new Color(0.78f, 0.44f, 1.00f, 1f);
                    break;
                case 5: // Luyện Hư — Astral Indigo-Silver
                    bodyColor = new Color(0.89f, 0.91f, 1.00f, 1f);
                    rimGlowColor = new Color(0.48f, 0.56f, 0.98f, 1f);
                    break;
                case 6: // Hợp Thể — Radiant Turquoise-Gold
                    bodyColor = new Color(0.85f, 1.00f, 0.95f, 1f);
                    rimGlowColor = new Color(0.24f, 0.88f, 0.68f, 1f);
                    break;
                case 7: // Đại Thừa — Sovereign Golden-Crimson Cloud
                    bodyColor = new Color(1.00f, 0.93f, 0.80f, 1f);
                    rimGlowColor = new Color(1.00f, 0.64f, 0.22f, 1f);
                    break;
                default: // Độ Kiếp / Phi Thăng — Ngũ Sắc Tường Vân (Five-Color Celestial Cloud)
                    float hue = Mathf.Repeat(timeSec * 0.14f, 1f);
                    Color prism = Color.HSVToRGB(hue, 0.45f, 1.0f);
                    bodyColor = Color.Lerp(new Color(1.0f, 0.97f, 0.88f, 1f), prism, 0.34f);
                    rimGlowColor = Color.HSVToRGB(Mathf.Repeat(hue + 0.15f, 1f), 0.72f, 1.0f);
                    break;
            }
        }

        public void SyncRealm(int realmIndex)
        {
            _realmIndex = Mathf.Clamp(realmIndex, 0, 8);
            ApplyColors(Time.time);
        }

        public void SetTribulationActive(bool active)
        {
            _inTribulation = active;
        }

        public void SetDissolveProgress(float dissolve01)
        {
            float clamped = Mathf.Clamp01(dissolve01);
            bool justDied = _dissolveAmount < 0.05f && clamped >= 0.05f;
            _dissolveAmount = clamped;
            if (justDied && cloudWispsPs != null)
            {
                // Soft billowing puff burst as the cloud disperses on player death
                cloudWispsPs.Emit(Mathf.RoundToInt(10 * _densityFactor));
            }
        }

        public void ApplyGraphicsDensity(RemakeVfxDensity density)
        {
            _densityFactor = density == RemakeVfxDensity.Minimal ? 0.40f : 1.0f;
            UpdateEmissionRates();
        }

        public float TickCloud(float dt)
        {
            _bobPhase += dt * (_inTribulation ? 2.65f : 1.85f);
            _tribBlend = Mathf.MoveTowards(_tribBlend, _inTribulation ? 1f : 0f, dt * 1.8f);

            float bobAmp = Mathf.Lerp(0.044f, 0.068f, _tribBlend);
            float bobOffset = Mathf.Sin(_bobPhase) * bobAmp;
            // During Tribulation, character clip lifts hips +0.45m; lift cloud +0.40m so the thick cloud stays snugly beneath the cultivator's hips!
            float tribCloudLift = _tribBlend * 0.40f;
            float seatExtraLift = _tribBlend * 0.14f;

            // Gentle squash & stretch breathing on cloud puffs so it feels soft & buoyant!
            float puffBreathe = 1f + Mathf.Sin(_bobPhase * 1.35f) * 0.045f;
            float puffY = 1f + Mathf.Cos(_bobPhase * 1.35f) * 0.065f;

            float vis = Mathf.Clamp01(1f - _dissolveAmount);
            float scaleVis = Mathf.Lerp(0.45f, 1.0f, vis);
            float tribScale = Mathf.Lerp(1.0f, 1.15f, _tribBlend);

            float cloudLocalY = bobOffset + tribCloudLift - _dissolveAmount * 0.12f;
            transform.localPosition = new Vector3(
                BaseCloudLocalOffset.x,
                BaseCloudLocalOffset.y + cloudLocalY,
                BaseCloudLocalOffset.z);
            CurrentSeatLocalY = BaseSeatLocalY + bobOffset + seatExtraLift - _dissolveAmount * 0.12f;

            if (cloudMeshTransform != null)
            {
                float spinDeg = Mathf.Lerp(7f, 40f, _tribBlend);
                cloudMeshTransform.Rotate(0f, spinDeg * dt, 0f, Space.Self);
                float xz = scaleVis * tribScale * puffBreathe;
                float yy = scaleVis * tribScale * puffY;
                cloudMeshTransform.localScale = new Vector3(_baseMeshScale.x * xz, _baseMeshScale.y * yy, _baseMeshScale.z * xz);
                if (cloudMeshRenderer != null)
                    cloudMeshRenderer.enabled = vis > 0.03f;
            }

            UpdateEmissionRates();
            ApplyColors(Time.time);
            return CurrentSeatLocalY;
        }

        private void UpdateEmissionRates()
        {
            float aliveFactor = Mathf.Clamp01(1f - _dissolveAmount * 1.3f);
            if (cloudWispsPs != null)
            {
                var em = cloudWispsPs.emission;
                em.rateOverTime = Mathf.Lerp(16.0f, 28f, _tribBlend) * _densityFactor * aliveFactor;
                var vel = cloudWispsPs.velocityOverLifetime;
                vel.orbitalY = new ParticleSystem.MinMaxCurve(Mathf.Lerp(0.62f, 2.5f, _tribBlend));
            }
            if (spiritMotesPs != null)
            {
                var em = spiritMotesPs.emission;
                em.rateOverTime = Mathf.Lerp(6.0f, 18f, _tribBlend) * _densityFactor * aliveFactor;
            }
        }

        private void ApplyColors(float timeSec)
        {
            GetRealmCloudColors(_realmIndex, timeSec, out Color body, out Color rim);

            // Blend into dramatic golden-violet vortex during Heavenly Tribulation!
            if (_tribBlend > 0.001f)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(timeSec * 5.2f);
                Color tribBody = Color.Lerp(new Color(0.98f, 0.91f, 0.72f, 1f), new Color(0.84f, 0.72f, 1.0f, 1f), pulse);
                Color tribRim = Color.Lerp(new Color(1.00f, 0.80f, 0.24f, 1f), new Color(0.76f, 0.42f, 1.0f, 1f), pulse);
                body = Color.Lerp(body, tribBody, _tribBlend);
                rim = Color.Lerp(rim, tribRim, _tribBlend);
            }

            float alpha = Mathf.Clamp01(1f - _dissolveAmount);
            if (_cloudMat != null)
            {
                Color c = new Color(body.r, body.g, body.b, alpha * 0.96f);
                if (_cloudMat.HasProperty(BaseColorId)) _cloudMat.SetColor(BaseColorId, c);
                if (_cloudMat.HasProperty(ColorId)) _cloudMat.SetColor(ColorId, c);
                if (_cloudMat.HasProperty(EmissionColorId))
                {
                    float emStrength = Mathf.Lerp(0.18f, 0.52f, _tribBlend) * alpha;
                    _cloudMat.SetColor(EmissionColorId, rim * emStrength);
                }
            }

            if (cloudWispsPs != null)
            {
                var main = cloudWispsPs.main;
                main.startColor = Color.Lerp(body, rim, 0.42f);
            }
            if (spiritMotesPs != null)
            {
                var main = spiritMotesPs.main;
                main.startColor = rim;
            }
        }

        private static Mesh BuildFallbackCloudMesh()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh src = go.GetComponent<MeshFilter>().sharedMesh;
            Mesh copy = Instantiate(src);
            var verts = copy.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].x *= 1.55f;
                verts[i].z *= 1.55f;
                verts[i].y = verts[i].y * 0.22f + 0.16f;
            }
            copy.vertices = verts;
            copy.RecalculateNormals();
            copy.RecalculateBounds();
            Destroy(go);
            return copy;
        }
    }
}

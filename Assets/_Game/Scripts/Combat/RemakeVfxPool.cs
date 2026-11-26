using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeVfxPool : MonoBehaviour
    {
        public static RemakeVfxPool Instance { get; private set; }

        public event Action<Vector3, double, bool> OnDamagePopupRequested;

        private ParticleSystem _hitSparks;
        private ParticleSystem _critBurst;
        private ParticleSystem _deathDissolve;
        private ParticleSystem _shockwaveRing;
        private LineRenderer[] _boltPool;
        private float[] _boltTimers;
        private int _boltCursor;

        // V2 Distinct Skill VFX pools
        private LineRenderer[] _swordRainPool;
        private float[] _swordRainTimers;
        private GameObject _yinYangDisc;
        private float _yinYangTimer;
        private GameObject _palmStrikeDisc;
        private float _palmTimer;
        private Vector3 _palmImpactPos;
        private int _hitStopToken;
        private RemakeTribulationVfx _tribulationVfx;

        private Material _sparkMat;
        private Material _ringMat;
        private Material _boltMat;
        private Material _swordRayMat;
        private Material _yinYangMat;
        private Material _palmMat;
        private Texture2D _glowTex;
        private Texture2D _ringTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializePools();
        }

        private void OnDestroy()
        {
            if (_hitStopToken > 0 && Time.timeScale != 1f) Time.timeScale = 1f;
            if (_sparkMat != null) Destroy(_sparkMat);
            if (_ringMat != null) Destroy(_ringMat);
            if (_boltMat != null) Destroy(_boltMat);
            if (_swordRayMat != null) Destroy(_swordRayMat);
            if (_yinYangMat != null) Destroy(_yinYangMat);
            if (_palmMat != null) Destroy(_palmMat);
            if (_glowTex != null) Destroy(_glowTex);
            if (_ringTex != null) Destroy(_ringTex);
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_boltPool != null)
            {
                for (int i = 0; i < _boltPool.Length; i++)
                {
                    if (_boltTimers[i] > 0f)
                    {
                        _boltTimers[i] -= dt;
                        if (_boltTimers[i] <= 0f && _boltPool[i] != null)
                            _boltPool[i].enabled = false;
                    }
                }
            }

            if (_swordRainPool != null)
            {
                for (int i = 0; i < _swordRainPool.Length; i++)
                {
                    if (_swordRainTimers[i] > 0f)
                    {
                        _swordRainTimers[i] -= dt;
                        float u = 1f - Mathf.Clamp01(_swordRainTimers[i] / 0.36f);
                        var lr = _swordRainPool[i];
                        if (lr != null && lr.enabled)
                        {
                            Vector3 start = lr.GetPosition(0);
                            Vector3 end = lr.GetPosition(1);
                            lr.SetPosition(0, Vector3.Lerp(start, end, dt * 7f));
                            if (_swordRainTimers[i] <= 0f) lr.enabled = false;
                        }
                    }
                }
            }

            if (_yinYangTimer > 0f && _yinYangDisc != null)
            {
                _yinYangTimer -= dt;
                float u = 1f - Mathf.Clamp01(_yinYangTimer / 0.85f);
                float scale = Mathf.Lerp(1.8f, 7.6f, 1f - (1f - u) * (1f - u));
                _yinYangDisc.transform.localScale = new Vector3(scale, scale, 1f);
                _yinYangDisc.transform.Rotate(0f, 0f, 165f * dt, Space.Self);
                if (_yinYangTimer <= 0f) _yinYangDisc.SetActive(false);
            }

            if (_palmTimer > 0f && _palmStrikeDisc != null)
            {
                _palmTimer -= dt;
                float u = 1f - Mathf.Clamp01(_palmTimer / 0.38f);
                float fallT = Mathf.Clamp01(u / 0.45f);
                float y = Mathf.Lerp(6.2f, 0.28f, fallT * fallT);
                float sc = Mathf.Lerp(2.2f, 4.4f, Mathf.Clamp01((u - 0.35f) / 0.65f));
                _palmStrikeDisc.transform.position = new Vector3(_palmImpactPos.x, y, _palmImpactPos.z);
                _palmStrikeDisc.transform.localScale = new Vector3(sc, sc, 1f);
                if (_palmTimer <= 0f) _palmStrikeDisc.SetActive(false);
            }
        }

        public void TriggerBossHitStop(float duration = 0.045f)
        {
            if (GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal)
                return;
            StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            _hitStopToken++;
            int myToken = _hitStopToken;
            Time.timeScale = 0.22f;
            yield return new WaitForSecondsRealtime(Mathf.Clamp(duration, 0.02f, 0.08f));
            if (myToken == _hitStopToken)
                Time.timeScale = 1f;
        }

        public void RequestDamagePopup(Vector3 worldPos, double amount, bool isCrit)
        {
            OnDamagePopupRequested?.Invoke(worldPos, amount, isCrit);
        }

        public void SpawnSwordHit(Vector3 pos, bool isCrit)
        {
            float scale = GraphicsSettingsService.Instance != null ? GraphicsSettingsService.Instance.VfxEmitScale : 1f;
            if (isCrit && _critBurst != null)
            {
                _critBurst.transform.position = pos;
                _critBurst.Emit(Mathf.Max(3, Mathf.RoundToInt(10 * scale)));
                if (_shockwaveRing != null)
                {
                    _shockwaveRing.transform.position = pos;
                    _shockwaveRing.Emit(1);
                }
            }
            else if (_hitSparks != null)
            {
                _hitSparks.transform.position = pos;
                _hitSparks.Emit(Mathf.Max(2, Mathf.RoundToInt(6 * scale)));
            }
        }

        public void SpawnDeathDissolve(Vector3 pos)
        {
            float scale = GraphicsSettingsService.Instance != null ? GraphicsSettingsService.Instance.VfxEmitScale : 1f;
            if (_deathDissolve != null)
            {
                _deathDissolve.transform.position = pos;
                _deathDissolve.Emit(Mathf.Max(3, Mathf.RoundToInt(8 * scale)));
            }
        }

        public void SpawnLoiPhatLightning(Vector3 targetPos)
        {
            if (_boltPool != null && _boltPool.Length > 0)
            {
                LineRenderer lr = _boltPool[_boltCursor];
                _boltTimers[_boltCursor] = 0.28f;
                _boltCursor = (_boltCursor + 1) % _boltPool.Length;
                if (lr != null)
                {
                    Vector3 top = targetPos + new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), 11f, UnityEngine.Random.Range(-0.4f, 0.4f));
                    lr.SetPosition(0, top);
                    lr.SetPosition(1, Vector3.Lerp(top, targetPos, 0.35f) + new Vector3(0.45f, 0f, -0.35f));
                    lr.SetPosition(2, Vector3.Lerp(top, targetPos, 0.70f) + new Vector3(-0.35f, 0f, 0.30f));
                    lr.SetPosition(3, targetPos);
                    lr.enabled = true;
                }
            }

            if (_shockwaveRing != null)
            {
                _shockwaveRing.transform.position = new Vector3(targetPos.x, 0.15f, targetPos.z);
                _shockwaveRing.Emit(1);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 0.9f;
                _critBurst.Emit(12);
            }
        }

        public void SpawnYinYangDomain(Vector3 center)
        {
            if (_yinYangDisc != null)
            {
                _yinYangDisc.transform.position = new Vector3(center.x, 0.12f, center.z);
                _yinYangDisc.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
                _yinYangDisc.SetActive(true);
                _yinYangTimer = 0.85f;
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(0.24f, 0.88f, 0.72f, 0.92f);
                _shockwaveRing.transform.position = center + Vector3.up * 0.16f;
                _shockwaveRing.Emit(2);
            }
            if (_hitSparks != null)
            {
                _hitSparks.transform.position = center + Vector3.up * 0.55f;
                _hitSparks.Emit(12);
            }
        }

        public void SpawnWanJianVolley(Vector3 targetPos)
        {
            if (_swordRainPool != null)
            {
                bool minimal = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal;
                int count = minimal ? 6 : _swordRainPool.Length;
                for (int i = 0; i < count; i++)
                {
                    var lr = _swordRainPool[i];
                    if (lr == null) continue;
                    Vector2 off = UnityEngine.Random.insideUnitCircle * 3.8f;
                    Vector3 dst = new Vector3(targetPos.x + off.x, 0.08f, targetPos.z + off.y);
                    Vector3 src = dst + new Vector3(UnityEngine.Random.Range(-1.8f, -0.6f), 9.5f + UnityEngine.Random.Range(0f, 3f), UnityEngine.Random.Range(-1.2f, 1.2f));
                    lr.SetPosition(0, src);
                    lr.SetPosition(1, dst);
                    lr.enabled = true;
                    _swordRainTimers[i] = 0.36f;
                }
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 0.75f;
                _critBurst.Emit(12);
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(0.98f, 0.82f, 0.25f, 0.92f);
                _shockwaveRing.transform.position = targetPos + Vector3.up * 0.15f;
                _shockwaveRing.Emit(2);
            }
        }

        public void SpawnAnChuongStrike(Vector3 targetPos)
        {
            if (_palmStrikeDisc != null)
            {
                _palmImpactPos = targetPos;
                _palmStrikeDisc.transform.position = new Vector3(targetPos.x, 6.2f, targetPos.z);
                _palmStrikeDisc.SetActive(true);
                _palmTimer = 0.38f;
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(1.0f, 0.78f, 0.20f, 0.98f);
                _shockwaveRing.transform.position = targetPos + Vector3.up * 0.22f;
                _shockwaveRing.Emit(3);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 1.1f;
                _critBurst.Emit(20);
            }
        }

        public void SetTribulationSeclusionVisual(bool active, Vector3 playerPos)
        {
            EnsureTribulationVfxLoaded();
            if (_tribulationVfx != null)
                _tribulationVfx.SetSeclusionActive(active, playerPos);
        }

        public void SpawnBreakthroughFlash(Vector3 pos, bool success)
        {
            EnsureTribulationVfxLoaded();
            if (_tribulationVfx != null)
            {
                _tribulationVfx.PlayOutcomeBurst(pos, success);
                return;
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = success ? new Color(1f, 0.85f, 0.28f, 0.95f) : new Color(0.90f, 0.25f, 0.20f, 0.9f);
                _shockwaveRing.transform.position = pos + Vector3.up * 0.2f;
                _shockwaveRing.Emit(3);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = pos + Vector3.up * 1.0f;
                _critBurst.Emit(success ? 24 : 16);
            }
            SpawnLoiPhatLightning(pos);
        }

        public void SpawnTribulationStrike(Vector3 playerPos, int strikeIndex, int totalStrikes)
        {
            EnsureTribulationVfxLoaded();
            if (_tribulationVfx != null)
            {
                _tribulationVfx.PlayStrike(playerPos, strikeIndex, totalStrikes);
                return;
            }

            // Fallback if prefab not yet baked
            SpawnLoiPhatLightning(playerPos);
        }

        private void EnsureTribulationVfxLoaded()
        {
            if (_tribulationVfx != null) return;
            var prefab = Resources.Load<RemakeTribulationVfx>("VFX/VFX_HeavenlyTribulation");
            if (prefab != null)
            {
                _tribulationVfx = Instantiate(prefab, transform);
                _tribulationVfx.name = "Pooled_HeavenlyTribulationVFX";
            }
        }

        private void InitializePools()
        {
            _glowTex = CreateRadialGlowTexture(32);
            _ringTex = CreateShockwaveRingTexture(64);

            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            _sparkMat = CreateTransparentParticleMat(particleShader, _glowTex, new Color(1f, 0.92f, 0.55f, 1f), true);
            _ringMat = CreateTransparentParticleMat(particleShader, _ringTex, new Color(0.95f, 0.85f, 0.35f, 0.95f), true);
            _boltMat = CreateTransparentParticleMat(particleShader, _glowTex, new Color(1f, 0.94f, 0.45f, 1f), true);
            _swordRayMat = CreateTransparentParticleMat(particleShader, _glowTex, new Color(1f, 0.86f, 0.30f, 1f), true);

            _hitSparks = CreateBurstSystem("VFX_HitSparks", _sparkMat, new Color(1f, 0.88f, 0.35f, 1f), 0.24f, 3.2f, 6.2f, 0.14f, 0.24f);
            _critBurst = CreateBurstSystem("VFX_CritBurst", _sparkMat, new Color(1f, 0.55f, 0.16f, 1f), 0.32f, 4.2f, 8.2f, 0.20f, 0.34f);
            _deathDissolve = CreateBurstSystem("VFX_DeathDissolve", _ringMat, new Color(0.45f, 0.95f, 0.78f, 0.85f), 0.42f, 1.8f, 3.6f, 0.22f, 0.38f);
            _shockwaveRing = CreateRingSystem("VFX_Shockwave", _ringMat, new Color(0.95f, 0.85f, 0.32f, 0.9f), 0.35f, 3.6f);

            _boltPool = new LineRenderer[3];
            _boltTimers = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject($"VFX_Bolt_{i}");
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = _boltMat;
                lr.positionCount = 4;
                lr.useWorldSpace = true;
                lr.startWidth = 0.30f;
                lr.endWidth = 0.10f;
                lr.startColor = new Color(1f, 0.98f, 0.65f, 1f);
                lr.endColor = new Color(0.45f, 0.88f, 1f, 0.8f);
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                _boltPool[i] = lr;
            }

            _swordRainPool = new LineRenderer[14];
            _swordRainTimers = new float[14];
            for (int i = 0; i < 14; i++)
            {
                var go = new GameObject($"VFX_SwordRay_{i}");
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = _swordRayMat;
                lr.positionCount = 2;
                lr.useWorldSpace = true;
                lr.startWidth = 0.08f;
                lr.endWidth = 0.26f;
                lr.startColor = new Color(1f, 0.96f, 0.75f, 0.95f);
                lr.endColor = new Color(0.98f, 0.74f, 0.18f, 1f);
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                _swordRainPool[i] = lr;
            }

            Texture2D yyTex = CodexArtLoader.LoadReusedSprite("SkillYinYang")?.texture ?? _ringTex;
            _yinYangMat = CreateTransparentParticleMat(particleShader, yyTex, new Color(0.85f, 1f, 0.92f, 0.88f), false);
            _yinYangDisc = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _yinYangDisc.name = "VFX_YinYangDisc";
            Destroy(_yinYangDisc.GetComponent<Collider>());
            _yinYangDisc.transform.SetParent(transform, false);
            _yinYangDisc.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var yyMr = _yinYangDisc.GetComponent<MeshRenderer>();
            yyMr.sharedMaterial = _yinYangMat;
            yyMr.shadowCastingMode = ShadowCastingMode.Off;
            yyMr.receiveShadows = false;
            _yinYangDisc.SetActive(false);

            Texture2D palmTex = CodexArtLoader.LoadReusedSprite("SkillPalm")?.texture ?? _glowTex;
            _palmMat = CreateTransparentParticleMat(particleShader, palmTex, new Color(1f, 0.88f, 0.35f, 0.94f), false);
            _palmStrikeDisc = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _palmStrikeDisc.name = "VFX_PalmSeal";
            Destroy(_palmStrikeDisc.GetComponent<Collider>());
            _palmStrikeDisc.transform.SetParent(transform, false);
            _palmStrikeDisc.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            var pmMr = _palmStrikeDisc.GetComponent<MeshRenderer>();
            pmMr.sharedMaterial = _palmMat;
            pmMr.shadowCastingMode = ShadowCastingMode.Off;
            pmMr.receiveShadows = false;
            _palmStrikeDisc.SetActive(false);

            // Prewarm Heavenly Tribulation VFX prefab once at boot (zero runtime Instantiate!)
            EnsureTribulationVfxLoaded();
        }

        private ParticleSystem CreateBurstSystem(string goName, Material mat, Color startColor, float life, float minSpd, float maxSpd, float minSz, float maxSz)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = life;
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpd, maxSpd);
            main.startSize = new ParticleSystem.MinMaxCurve(minSz, maxSz);
            main.startColor = startColor;
            main.gravityModifier = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        private ParticleSystem CreateRingSystem(string goName, Material mat, Color startColor, float life, float endSize)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = life;
            main.startSpeed = 0f;
            main.startSize = 0.35f;
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            var curve = new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, endSize));
            sz.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        private static Material CreateTransparentParticleMat(Shader shader, Texture2D tex, Color color, bool additive)
        {
            var mat = new Material(shader) { color = color, mainTexture = tex };
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", additive ? 2f : 0f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_SrcBlendAlpha")) mat.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            if (mat.HasProperty("_DstBlendAlpha")) mat.SetFloat("_DstBlendAlpha", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return mat;
        }

        private static Texture2D CreateRadialGlowTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - dist);
                    a *= a;
                    tex.SetPixel(x, y, new Color(a, a, a, a));
                }
            }
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D CreateShockwaveRingTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float dRing = (dist - 0.72f) / 0.14f;
                    float a = dist <= 0.98f ? Mathf.Exp(-dRing * dRing) : 0f;
                    tex.SetPixel(x, y, new Color(a, a, a, a));
                }
            }
            tex.Apply(false, true);
            return tex;
        }
    }
}

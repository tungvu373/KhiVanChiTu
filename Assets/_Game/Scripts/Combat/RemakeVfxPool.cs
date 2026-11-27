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

        private ParticleSystem _wanJianRainPs;
        private GameObject _yinYangDisc;
        private float _yinYangTimer;
        private GameObject _palmStrikeDisc;
        private float _palmTimer;
        private Vector3 _palmImpactPos;
        private int _hitStopToken;
        private RemakeTribulationVfx _tribulationVfx;

        private Material _sparkMat;
        private Material _wispMat;
        private Material _ringMat;
        private Material _boltMat;
        private Material _yinYangMat;
        private Material _palmMat;
        private Material _wanJianMat;
        private Texture2D _glowTex;
        private Texture2D _streakTex;
        private Texture2D _bladeRainTex;
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
            if (_wispMat != null) Destroy(_wispMat);
            if (_ringMat != null) Destroy(_ringMat);
            if (_boltMat != null) Destroy(_boltMat);
            if (_yinYangMat != null) Destroy(_yinYangMat);
            if (_palmMat != null) Destroy(_palmMat);
            if (_wanJianMat != null) Destroy(_wanJianMat);
            if (_glowTex != null) Destroy(_glowTex);
            if (_streakTex != null) Destroy(_streakTex);
            if (_bladeRainTex != null) Destroy(_bladeRainTex);
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

            if (_yinYangTimer > 0f && _yinYangDisc != null)
            {
                const float yinYangDur = 4.5f;
                _yinYangTimer -= dt;
                float elapsed = yinYangDur - Mathf.Max(0f, _yinYangTimer);
                float scale;
                if (elapsed < 0.65f)
                {
                    float e = elapsed / 0.65f;
                    scale = Mathf.Lerp(1.8f, 7.6f, 1f - (1f - e) * (1f - e));
                }
                else if (_yinYangTimer < 0.65f)
                {
                    float f = Mathf.Clamp01(_yinYangTimer / 0.65f);
                    scale = Mathf.Lerp(2.2f, 7.6f, f);
                }
                else
                {
                    scale = 7.6f + Mathf.Sin(elapsed * 3.4f) * 0.28f;
                }
                _yinYangDisc.transform.localScale = new Vector3(scale, scale, 1f);
                _yinYangDisc.transform.Rotate(0f, 0f, 64f * dt, Space.Self);
                if (_yinYangTimer <= 0f) _yinYangDisc.SetActive(false);
            }

            if (_palmTimer > 0f && _palmStrikeDisc != null)
            {
                const float palmDur = 1.25f;
                _palmTimer -= dt;
                float elapsed = palmDur - Mathf.Max(0f, _palmTimer);
                float fallT = Mathf.Clamp01(elapsed / 0.28f);
                float y = Mathf.Lerp(6.2f, 0.24f, fallT * fallT);
                float sc = elapsed < 0.45f
                    ? Mathf.Lerp(2.2f, 4.6f, Mathf.Clamp01((elapsed - 0.18f) / 0.27f))
                    : 4.6f + Mathf.Sin(elapsed * 5f) * 0.14f;
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
                _critBurst.Emit(Mathf.Max(3, Mathf.RoundToInt(9 * scale)));
                if (_shockwaveRing != null)
                {
                    var main = _shockwaveRing.main;
                    main.startColor = new Color(1f, 0.88f, 0.42f, 0.55f);
                    _shockwaveRing.transform.position = pos + Vector3.up * 0.05f;
                    _shockwaveRing.Emit(1);
                }
            }
            else if (_hitSparks != null)
            {
                _hitSparks.transform.position = pos;
                _hitSparks.Emit(Mathf.Max(2, Mathf.RoundToInt(5 * scale)));
            }
        }

        public void SpawnDeathDissolve(Vector3 pos)
        {
            float scale = GraphicsSettingsService.Instance != null ? GraphicsSettingsService.Instance.VfxEmitScale : 1f;
            if (_deathDissolve != null)
            {
                _deathDissolve.transform.position = pos;
                _deathDissolve.Emit(Mathf.Max(4, Mathf.RoundToInt(12 * scale)));
            }
        }

        public void SpawnLoiPhatLightning(Vector3 targetPos)
        {
            if (_boltPool != null && _boltPool.Length > 0)
            {
                LineRenderer lr = _boltPool[_boltCursor];
                _boltTimers[_boltCursor] = 0.44f;
                _boltCursor = (_boltCursor + 1) % _boltPool.Length;
                if (lr != null)
                {
                    Vector3 top = targetPos + new Vector3(UnityEngine.Random.Range(-0.55f, 0.55f), 11.2f, UnityEngine.Random.Range(-0.40f, 0.40f));
                    int count = lr.positionCount;
                    lr.SetPosition(0, top);
                    for (int s = 1; s < count - 1; s++)
                    {
                        float u = s / (float)(count - 1);
                        float amp = Mathf.Sin(u * Mathf.PI) * 0.45f;
                        Vector3 jitter = new Vector3(UnityEngine.Random.Range(-amp, amp), 0f, UnityEngine.Random.Range(-amp, amp));
                        lr.SetPosition(s, Vector3.Lerp(top, targetPos, u) + jitter);
                    }
                    lr.SetPosition(count - 1, targetPos);
                    lr.enabled = true;
                }
            }

            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(0.62f, 0.94f, 1.0f, 0.58f);
                _shockwaveRing.transform.position = new Vector3(targetPos.x, 0.14f, targetPos.z);
                _shockwaveRing.Emit(1);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 0.85f;
                _critBurst.Emit(10);
            }
        }

        public void SpawnYinYangDomain(Vector3 center)
        {
            if (_yinYangDisc != null)
            {
                _yinYangDisc.transform.position = new Vector3(center.x, 0.12f, center.z);
                _yinYangDisc.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
                _yinYangDisc.SetActive(true);
                _yinYangTimer = 4.5f;
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(0.28f, 0.92f, 0.76f, 0.52f);
                _shockwaveRing.transform.position = center + Vector3.up * 0.16f;
                _shockwaveRing.Emit(1);
            }
            if (_deathDissolve != null)
            {
                _deathDissolve.transform.position = center + Vector3.up * 0.45f;
                _deathDissolve.Emit(10);
            }
        }

        private static readonly ParticleSystem.Burst[] WanJianBurstsMinimal =
        {
            new ParticleSystem.Burst(0.00f, 6),
            new ParticleSystem.Burst(0.28f, 6)
        };

        private static readonly ParticleSystem.Burst[] WanJianBurstsFull =
        {
            new ParticleSystem.Burst(0.00f, 10),
            new ParticleSystem.Burst(0.25f, 10),
            new ParticleSystem.Burst(0.50f, 8)
        };

        public void SpawnWanJianVolley(Vector3 targetPos)
        {
            bool minimal = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal;
            if (_wanJianRainPs != null)
            {
                _wanJianRainPs.transform.position = targetPos + new Vector3(-1.4f, 5.6f, 0.8f);
                var em = _wanJianRainPs.emission;
                em.SetBursts(minimal ? WanJianBurstsMinimal : WanJianBurstsFull);
                _wanJianRainPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _wanJianRainPs.Play(true);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 0.75f;
                _critBurst.Emit(minimal ? 6 : 10);
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(0.98f, 0.84f, 0.30f, 0.54f);
                _shockwaveRing.transform.position = targetPos + Vector3.up * 0.15f;
                _shockwaveRing.Emit(1);
            }
        }

        public void SpawnAnChuongStrike(Vector3 targetPos)
        {
            if (_palmStrikeDisc != null)
            {
                _palmImpactPos = targetPos;
                _palmStrikeDisc.transform.position = new Vector3(targetPos.x, 6.2f, targetPos.z);
                _palmStrikeDisc.SetActive(true);
                _palmTimer = 1.25f;
            }
            if (_shockwaveRing != null)
            {
                var main = _shockwaveRing.main;
                main.startColor = new Color(1.0f, 0.80f, 0.24f, 0.62f);
                _shockwaveRing.transform.position = targetPos + Vector3.up * 0.22f;
                _shockwaveRing.Emit(2);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = targetPos + Vector3.up * 1.1f;
                _critBurst.Emit(16);
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
                main.startColor = success ? new Color(1f, 0.85f, 0.28f, 0.65f) : new Color(0.90f, 0.25f, 0.20f, 0.60f);
                _shockwaveRing.transform.position = pos + Vector3.up * 0.2f;
                _shockwaveRing.Emit(2);
            }
            if (_critBurst != null)
            {
                _critBurst.transform.position = pos + Vector3.up * 1.0f;
                _critBurst.Emit(success ? 18 : 12);
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
            _glowTex = CreateRadialGlowTexture(64);
            _streakTex = CreateSpindleStreakTexture(128, 32);
            _bladeRainTex = CreatePhantomBladeTexture(128, 32);
            _ringTex = CreateShockwaveRingTexture(96);

            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            _sparkMat = CreateTransparentParticleMat(particleShader, _streakTex, new Color(1f, 0.94f, 0.65f, 1f), true);
            _wispMat = CreateTransparentParticleMat(particleShader, _glowTex, new Color(0.75f, 1.0f, 0.90f, 0.92f), true);
            _ringMat = CreateTransparentParticleMat(particleShader, _ringTex, new Color(0.98f, 0.88f, 0.42f, 0.65f), true);
            _boltMat = CreateTransparentParticleMat(particleShader, _streakTex, new Color(0.88f, 0.97f, 1.0f, 1f), true);

            // Stretched Sword-Qi needle slashes instead of round/square blobs!
            _hitSparks = CreateStreakBurstSystem("VFX_HitSparks", _sparkMat, new Color(0.78f, 0.98f, 0.90f, 1f), 0.22f, 4.2f, 7.8f, 0.08f, 0.14f, 2.6f);
            _critBurst = CreateStreakBurstSystem("VFX_CritBurst", _sparkMat, new Color(1f, 0.78f, 0.26f, 1f), 0.28f, 5.2f, 9.5f, 0.11f, 0.18f, 3.4f);

            // Ascending ethereal Spirit Wisps on death/dissolve (zero flat rings or squares!)
            _deathDissolve = CreateSpiritWispSystem("VFX_DeathDissolve", _wispMat, new Color(0.58f, 0.98f, 0.84f, 0.90f));

            // Delicate feathered Qi ripple
            _shockwaveRing = CreateRingSystem("VFX_Shockwave", _ringMat, new Color(0.95f, 0.85f, 0.36f, 0.55f), 0.34f, 3.2f);

            _wanJianMat = CreateTransparentParticleMat(particleShader, _bladeRainTex, Color.white, true);
            _wanJianRainPs = CreateWanJianRainSystem("VFX_WanJianRain", _wanJianMat);

            _boltPool = new LineRenderer[3];
            _boltTimers = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject($"VFX_Bolt_{i}");
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = _boltMat;
                lr.positionCount = 8;
                lr.useWorldSpace = true;
                lr.startWidth = 0.18f;
                lr.endWidth = 0.05f;
                lr.numCornerVertices = 4;
                lr.numCapVertices = 4;
                lr.startColor = new Color(1f, 0.99f, 0.78f, 0.95f);
                lr.endColor = new Color(0.45f, 0.90f, 1f, 0.75f);
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                _boltPool[i] = lr;
            }

            Texture2D yyTex = CodexArtLoader.LoadReusedSprite("SkillYinYang")?.texture ?? _ringTex;
            _yinYangMat = CreateTransparentParticleMat(particleShader, yyTex, new Color(0.85f, 1f, 0.92f, 0.82f), false);
            _yinYangDisc = RemakeRenderPrimitive.Create(PrimitiveType.Quad);
            _yinYangDisc.name = "VFX_YinYangDisc";
            _yinYangDisc.transform.SetParent(transform, false);
            _yinYangDisc.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var yyMr = _yinYangDisc.GetComponent<MeshRenderer>();
            yyMr.sharedMaterial = _yinYangMat;
            yyMr.shadowCastingMode = ShadowCastingMode.Off;
            yyMr.receiveShadows = false;
            _yinYangDisc.SetActive(false);

            Texture2D palmTex = CodexArtLoader.LoadReusedSprite("SkillPalm")?.texture ?? _glowTex;
            _palmMat = CreateTransparentParticleMat(particleShader, palmTex, new Color(1f, 0.88f, 0.35f, 0.90f), false);
            _palmStrikeDisc = RemakeRenderPrimitive.Create(PrimitiveType.Quad);
            _palmStrikeDisc.name = "VFX_PalmSeal";
            _palmStrikeDisc.transform.SetParent(transform, false);
            _palmStrikeDisc.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            var pmMr = _palmStrikeDisc.GetComponent<MeshRenderer>();
            pmMr.sharedMaterial = _palmMat;
            pmMr.shadowCastingMode = ShadowCastingMode.Off;
            pmMr.receiveShadows = false;
            _palmStrikeDisc.SetActive(false);

            EnsureTribulationVfxLoaded();
        }

        private ParticleSystem CreateStreakBurstSystem(string goName, Material mat, Color startColor, float life, float minSpd, float maxSpd, float minSz, float maxSz, float stretchLen)
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
            main.gravityModifier = 0.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.12f;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.15f)));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey( Color.Lerp(startColor, Color.white, 0.45f), 0f), new GradientColorKey(startColor, 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.045f;
            r.lengthScale = stretchLen;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        private ParticleSystem CreateSpiritWispSystem(string goName, Material mat, Color tint)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.65f, 1.85f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(tint, new Color(1.0f, 0.92f, 0.56f, 0.90f));
            main.gravityModifier = -0.42f; // Ascend gently into heaven as spirit motes!
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.35f;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.45f),
                new Keyframe(0.25f, 1.0f),
                new Keyframe(1f, 0.0f)));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(tint, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.90f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.06f;
            r.lengthScale = 1.6f;
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
            var curve = new AnimationCurve(new Keyframe(0f, 0.30f), new Keyframe(1f, endSize));
            sz.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
                new[] { new GradientAlphaKey(0.60f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        private ParticleSystem CreateWanJianRainSystem(string goName, Material mat)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.90f;
            main.maxParticles = 40;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.78f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.34f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.94f, 0.46f, 1f),
                new Color(0.45f, 0.94f, 1.0f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 0f;
            em.SetBursts(WanJianBurstsFull);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 3.8f;
            shape.rotation = new Vector3(72f, 28f, 0f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.98f, 0.80f), 0f),
                    new GradientColorKey(new Color(0.98f, 0.78f, 0.24f), 0.6f),
                    new GradientColorKey(new Color(0.88f, 0.42f, 0.12f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.95f, 0.12f),
                    new GradientAlphaKey(0.88f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.lengthScale = 6.2f;
            r.velocityScale = 0.06f;
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
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_SrcBlendAlpha")) mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            if (mat.HasProperty("_DstBlendAlpha")) mat.SetFloat("_DstBlendAlpha", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;
            return mat;
        }

        private static Texture2D CreateRadialGlowTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r2 = dx * dx + dy * dy;
                    float a = r2 < 0.94f ? Mathf.Exp(-r2 * 5.2f) * Mathf.Clamp01((0.94f - r2) / 0.22f) : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D CreateSpindleStreakTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < h; y++)
            {
                float ny = Mathf.Abs((y / (float)(h - 1)) * 2f - 1f); // 0 at center -> 1 at top/bottom edge
                for (int x = 0; x < w; x++)
                {
                    float nx = Mathf.Abs((x / (float)(w - 1)) * 2f - 1f); // 0 at middle -> 1 at tips
                    float taperWidth = Mathf.Max(0.05f, 1f - nx * nx);
                    float d = ny / taperWidth;
                    float a = (d < 1f && nx < 0.98f)
                        ? Mathf.Pow(1f - d, 2.4f) * Mathf.Pow(1f - nx, 1.35f)
                        : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D CreatePhantomBladeTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < h; y++)
            {
                float ny = Mathf.Abs((y / (float)(h - 1)) * 2f - 1f);
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)(w - 1); // 0 = pommel/hilt, 0.26 = crossguard, 1.0 = sharp sword tip
                    float bladeProfile = u < 0.20f ? 0.16f : Mathf.Lerp(0.32f, 0.02f, (u - 0.20f) / 0.80f);
                    float guardCross = Mathf.Exp(-Mathf.Pow((u - 0.24f) / 0.035f, 2f)) * 0.82f;
                    float maxHalfW = Mathf.Max(bladeProfile, guardCross);
                    float a = (ny < maxHalfW && u > 0.03f && u < 0.98f)
                        ? Mathf.Pow(1f - ny / Mathf.Max(0.02f, maxHalfW), 1.6f) * Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI * 0.95f + 0.05f)
                        : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(a)));
                }
            }
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D CreateShockwaveRingTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float dRing = (dist - 0.76f) / 0.085f;
                    float edgeFade = Mathf.Clamp01((0.96f - dist) / 0.12f);
                    float a = dist <= 0.96f ? Mathf.Exp(-dRing * dRing) * edgeFade * 0.72f : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply(false, true);
            return tex;
        }
    }
}

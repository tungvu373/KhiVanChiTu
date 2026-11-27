using UnityEngine;
using UnityEngine.Rendering;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeFlyingSwordActor : MonoBehaviour
    {
        public enum MotionState { Orbiting, Dashing, Retreating }

        [Header("3D Sword Tier Assets (Lv.1 .. Lv.10)")]
        [SerializeField] private MeshFilter swordMeshFilter;
        [SerializeField] private MeshRenderer swordMeshRenderer;
        [SerializeField] private Mesh[] tierMeshes;
        [SerializeField] private Material[] tierMaterials;
        [Tooltip("Enable when the blade tip points along the mesh's local -Z axis.")]
        [SerializeField] private bool bladeTipPointsBackward = true;

        private const float ImpactProgress = 0.66f;
        private static readonly Quaternion BackwardBladeCorrection = Quaternion.Euler(0f, 180f, 0f);
        private float _bladeTipZ;

        private int _swordIndex;
        private int _formationTotal = 1;
        private int _swordLevel = 1;
        private Transform _orbitCenter;
        private MotionState _state = MotionState.Orbiting;

        private RemakeMonsterActor _target;
        private Vector3 _dashStartPos;
        private Vector3 _dashControlPos;
        private Vector3 _dashHitPos;
        private Vector3 _dashContactPos;
        private Vector3 _dashImpactDirection;
        private Vector3 _dashExitPos;
        private float _dashProgress;
        private bool _dashDidHit;
        private float _attackCooldown;
        private float _retreatTimer;
        private float _searchTimer;
        private float _punchTimer;

        private Vector3 _smoothVel;
        private float _axialSpinDeg;
        private float _flourishSpinVel;
        private float _flourishCooldown = 2.5f;

        private TrailRenderer _trail;
        private Material _runtimeTrailMat;
        private static Texture2D _sharedSilkRibbonTex;
        private Color _swordColor = new Color(0.28f, 0.85f, 0.72f, 1f);
        private float _tierBaseScale = 0.70f;
        private Gradient _gradOrbit;
        private Gradient _gradDash;
        private Gradient _gradRetreat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticTex()
        {
            _sharedSilkRibbonTex = null;
        }

        private void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
            if (swordMeshFilter == null) swordMeshFilter = GetComponentInChildren<MeshFilter>();
            if (swordMeshRenderer == null) swordMeshRenderer = GetComponentInChildren<MeshRenderer>();

            // Hide any legacy 2D SpriteRenderer if present
            var legacySr = GetComponentInChildren<SpriteRenderer>();
            if (legacySr != null) legacySr.enabled = false;

            UpdateBladeTip();
            EnsureTrailMaterial();
        }

        private void OnDestroy()
        {
            if (_runtimeTrailMat != null) Destroy(_runtimeTrailMat);
        }

        public void Configure(int index, int totalCount, int level, Transform orbitCenter)
        {
            _swordIndex = index;
            _formationTotal = Mathf.Max(1, totalCount);
            _swordLevel = Mathf.Clamp(level, 1, 10);
            _orbitCenter = orbitCenter;
            _tierBaseScale = Mathf.Lerp(0.82f, 1.02f, (_swordLevel - 1) / 9f);

            int tierIdx = _swordLevel - 1;
            if (swordMeshFilter != null && tierMeshes != null && tierIdx < tierMeshes.Length && tierMeshes[tierIdx] != null)
            {
                swordMeshFilter.sharedMesh = tierMeshes[tierIdx];
                swordMeshFilter.transform.localScale = Vector3.one * _tierBaseScale;
            }
            if (swordMeshRenderer != null)
            {
                swordMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                swordMeshRenderer.receiveShadows = false;
                if (tierMaterials != null && tierIdx < tierMaterials.Length && tierMaterials[tierIdx] != null)
                    swordMeshRenderer.sharedMaterial = tierMaterials[tierIdx];
            }

            UpdateBladeTip();
            _swordColor = _swordLevel switch
            {
                <= 2 => new Color(0.26f, 0.90f, 0.76f, 1f), // Thanh Ngoc
                <= 4 => new Color(0.32f, 0.78f, 1.00f, 1f), // Thien Lam
                <= 6 => new Color(0.76f, 0.50f, 0.98f, 1f), // Tu Loi
                <= 8 => new Color(1.00f, 0.78f, 0.26f, 1f), // Huyen Kim
                _ => new Color(1.00f, 0.92f, 0.58f, 1f)      // Thai Hu Tien Kim
            };
            RebuildCachedTrailGradients();
            SetTrailState(_state);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (_attackCooldown > 0f) _attackCooldown -= dt;
            if (_searchTimer > 0f) _searchTimer -= dt;

            if (_punchTimer > 0f)
            {
                _punchTimer -= dt;
                float k = Mathf.Clamp01(_punchTimer / 0.09f);
                transform.localScale = Vector3.one * (1f + 0.24f * k);
            }
            else if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
            }

            Vector3 center = _orbitCenter != null ? _orbitCenter.position + Vector3.up * 0.96f : Vector3.up * 0.96f;
            var engine = GameplayEngine.Instance;
            bool inTrib = engine != null && engine.IsInTribulation;
            bool respawning = engine != null && engine.IsRespawning;

            if (respawning || inTrib)
            {
                _target = null;
                _dashProgress = 0f;
                _retreatTimer = 0f;
                if (_state != MotionState.Orbiting) SetTrailState(MotionState.Orbiting);
                Vector3 guardCenter = inTrib ? center + Vector3.up * 0.45f : center;
                Vector3 idleOrbit = ComputeOrbitPosition(guardCenter, inTrib ? 1.55f : 1.0f);
                SteerSmoothlyToOrbit(idleOrbit, guardCenter, dt, inTrib ? 0.14f : 0.22f, inTrib ? 320f : 110f);
                return;
            }

            if (_retreatTimer > 0f)
            {
                if (_state != MotionState.Retreating) SetTrailState(MotionState.Retreating);
                _retreatTimer -= dt;
                Vector3 orbitPt = ComputeOrbitPosition(center, 1.0f);
                SteerSmoothlyToOrbit(orbitPt, center, dt, 0.18f, 420f);
                return;
            }

            if (!(_state == MotionState.Dashing && _dashDidHit) && (_target == null || !_target.IsAlive))
            {
                if (_state != MotionState.Orbiting) SetTrailState(MotionState.Orbiting);
                _target = null;
                _dashProgress = 0f;

                if (_searchTimer <= 0f && _attackCooldown <= 0f && RemakeArenaController.Instance != null)
                {
                    _target = RemakeArenaController.Instance.AcquireTarget(_swordIndex);
                    _searchTimer = 0.22f;
                    if (_target != null && _target.IsAlive)
                    {
                        BeginPiercingSwoop(_target.HitTargetPoint);
                        return;
                    }
                }

                // Periodic sword-spirit flourish spin while orbiting!
                _flourishCooldown -= dt;
                if (_flourishCooldown <= 0f)
                {
                    _flourishCooldown = Random.Range(3.2f, 6.0f);
                    _flourishSpinVel = 540f;
                }
                _flourishSpinVel = Mathf.MoveTowards(_flourishSpinVel, 95f, 380f * dt);

                Vector3 orbitPt = ComputeOrbitPosition(center, 1.0f);
                SteerSmoothlyToOrbit(orbitPt, center, dt, 0.20f, _flourishSpinVel);
                return;
            }

            if (_state != MotionState.Dashing)
                BeginPiercingSwoop(_target.HitTargetPoint);

            if (!_dashDidHit)
                _dashHitPos = _target.HitTargetPoint;

            float speed = CultivationTable.CalculateSwordSpeed(_swordLevel);
            if (engine != null)
                speed *= engine.GetSwordSpeedMultiplier();

            float totalArcLen = Mathf.Max(2.2f, Vector3.Distance(_dashStartPos, _dashHitPos) + 1.15f);
            _dashProgress += (speed / totalArcLen) * dt;
            float t = Mathf.Clamp01(_dashProgress);
            float spinSpeed = 920f + _swordLevel * 28f;

            if (!_dashDidHit)
            {
                Vector3 toTarget = _dashHitPos - _dashStartPos;
                Vector3 direction = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;

                if (t >= ImpactProgress)
                {
                    _dashImpactDirection = direction;
                    FaceFlightDirection(direction);
                    ApplyVisualSpinRoll(dt, spinSpeed, 0f);
                    PositionBladeTipAt(_dashHitPos);
                    _dashContactPos = transform.position;
                    _dashExitPos = _dashContactPos + direction * 1.3f + Vector3.up * 0.38f;
                    _dashDidHit = true;
                    PerformAttack(_dashHitPos);
                    return;
                }

                float tipReach = swordMeshFilter == null ? 0f :
                    Mathf.Abs(_bladeTipZ * swordMeshFilter.transform.lossyScale.z);
                Vector3 contact = _dashHitPos - direction * tipReach;
                transform.position = EvaluateQuadraticBezier(_dashStartPos, _dashControlPos, contact, t / ImpactProgress);
                FaceFlightDirection(_dashHitPos - transform.position);
            }
            else
            {
                Vector3 exitControl = _dashContactPos + _dashImpactDirection * 0.65f;
                float exitProgress = (t - ImpactProgress) / (1f - ImpactProgress);
                transform.position = EvaluateQuadraticBezier(_dashContactPos, exitControl, _dashExitPos, exitProgress);
                FaceFlightDirection(Vector3.Lerp(exitControl - _dashContactPos, _dashExitPos - exitControl, exitProgress));
            }

            ApplyVisualSpinRoll(dt, spinSpeed, 0f);

            if (t >= 0.995f)
            {
                _target = null;
                _retreatTimer = 0.34f;
                SetTrailState(MotionState.Retreating);
            }
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        private void FaceFlightDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            Vector3 forward = direction.normalized;
            Vector3 up = Mathf.Abs(forward.y) > 0.98f ? Vector3.forward : Vector3.up;
            transform.rotation = Quaternion.LookRotation(forward, up);
        }

        private void UpdateBladeTip()
        {
            Mesh mesh = swordMeshFilter != null ? swordMeshFilter.sharedMesh : null;
            _bladeTipZ = mesh == null ? 0f : (bladeTipPointsBackward ? mesh.bounds.min.z : mesh.bounds.max.z);
        }

        private void PositionBladeTipAt(Vector3 target)
        {
            // These sword meshes are centered on the local Z axis.
            Vector3 tip = swordMeshFilter != null ?
                swordMeshFilter.transform.TransformPoint(new Vector3(0f, 0f, _bladeTipZ)) :
                transform.position;
            transform.position += target - tip;
        }

        private void SteerSmoothlyToOrbit(Vector3 targetOrbitPos, Vector3 center, float dt, float smoothTime, float axialSpinSpeed)
        {
            Vector3 prev = transform.position;
            transform.position = Vector3.SmoothDamp(prev, targetOrbitPos, ref _smoothVel, smoothTime, 14f, dt);

            Vector3 moveVec = transform.position - prev;
            Vector3 radiusVec = transform.position - center;
            Vector3 fallbackTangent = Vector3.Cross(Vector3.up, radiusVec);
            Vector3 forwardDir = moveVec.sqrMagnitude > 0.00005f ? moveVec.normalized
                : (fallbackTangent.sqrMagnitude > 0.001f ? fallbackTangent.normalized : Vector3.forward);

            // Graceful inward banking toward cultivator center
            Vector3 inward = (-radiusVec).sqrMagnitude > 0.01f ? (-radiusVec).normalized : Vector3.up;
            Vector3 bankedUp = (Vector3.up + inward * 0.42f).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forwardDir, bankedUp), 11f * dt);

            ApplyVisualSpinRoll(dt, axialSpinSpeed, 18f);
        }

        private void ApplyVisualSpinRoll(float dt, float spinDegPerSec, float gentlePitchWave)
        {
            if (swordMeshFilter == null || swordMeshFilter.transform == transform) return;
            _axialSpinDeg = Mathf.Repeat(_axialSpinDeg + spinDegPerSec * dt, 360f);
            float wobble = gentlePitchWave > 0f ? Mathf.Sin(Time.time * 3.1f + _swordIndex * 1.3f) * 4.5f : 0f;
            Quaternion roll = Quaternion.Euler(wobble, 0f, _axialSpinDeg);
            swordMeshFilter.transform.localRotation = bladeTipPointsBackward ? roll * BackwardBladeCorrection : roll;
        }

        private void BeginPiercingSwoop(Vector3 targetHitPt)
        {
            SetTrailState(MotionState.Dashing);
            _dashStartPos = transform.position;
            _dashHitPos = targetHitPt;

            Vector3 toTarget = _dashHitPos - _dashStartPos;
            Vector3 dir = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector3.forward;
            Vector3 flatRight = Vector3.Cross(Vector3.up, dir);
            float sideSign = (_swordIndex % 2 == 0) ? 1f : -1f;
            float arcSide = Mathf.Clamp(toTarget.magnitude * 0.38f, 0.75f, 1.95f);

            _dashControlPos = Vector3.Lerp(_dashStartPos, _dashHitPos, 0.42f) + flatRight * (sideSign * arcSide) + Vector3.up * 0.55f;
            _dashProgress = 0f;
            _dashDidHit = false;
        }

        private void PerformAttack(Vector3 hitPoint)
        {
            if (_target == null || !_target.IsAlive) return;
            var engine = GameplayEngine.Instance;
            double swordBase = CultivationTable.CalculateSwordDamage(_swordLevel);
            double dmg = swordBase * (engine != null ? engine.GetSwordDamageMultiplier() : 1f);
            dmg *= Random.Range(0.88f, 1.12f);

            bool isCrit = engine != null && Random.value <= engine.GetCritChance();
            if (isCrit) dmg *= engine != null ? engine.GetCritDamageMultiplier() : 2.0;
            if (_target.Definition != null && _target.Definition.isBoss && engine != null)
                dmg *= engine.GetBossBonusMultiplier();

            Vector3 knockDir = hitPoint - (_orbitCenter != null ? _orbitCenter.position : transform.position);
            knockDir.y = 0f;
            if (knockDir.sqrMagnitude > 0.001f) knockDir.Normalize();

            _target.ApplyDamage(dmg, isCrit, knockDir);
            RemakeVfxPool.Instance?.SpawnSwordHit(hitPoint, isCrit);
            RemakeVfxPool.Instance?.RequestDamagePopup(hitPoint + Vector3.up * 0.5f, dmg, isCrit);
            RemakeAudioService.Instance?.PlaySwordHit();

            _punchTimer = 0.09f;
            float cd = CultivationTable.CalculateSwordAttackCooldown(_swordLevel);
            _attackCooldown = cd;
            _searchTimer = cd;
        }

        private Vector3 ComputeOrbitPosition(Vector3 center, float speedMult)
        {
            float angleStep = 360f / Mathf.Max(1, _formationTotal);
            float orbitSpd = CultivationTable.CalculateSwordOrbitSpeed(_swordLevel) * speedMult;
            float angle = Time.time * orbitSpd + _swordIndex * angleStep;

            // Dual-plane inclined 3D Lotus Sword Array around seated cultivator
            float planeTiltX = Mathf.Sin(_swordIndex * 2.09f) * 13f;
            float planeTiltZ = Mathf.Cos(_swordIndex * 2.09f) * 13f;
            float radius = 1.72f + 0.14f * Mathf.Sin(Time.time * 1.6f + _swordIndex * 0.9f);
            float bobY = Mathf.Sin(Time.time * 2.3f + _swordIndex * 1.15f) * 0.16f;

            Vector3 localOrbit = Quaternion.Euler(0f, angle, 0f) * new Vector3(radius, bobY, 0f);
            return center + Quaternion.Euler(planeTiltX, 0f, planeTiltZ) * localOrbit;
        }

        private static Texture2D GetOrCreateSilkRibbonTexture()
        {
            if (_sharedSilkRibbonTex != null) return _sharedSilkRibbonTex;
            const int w = 64;
            const int h = 32;
            _sharedSilkRibbonTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                name = "Proc_SwordSilkRibbon",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var cols = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = y / (float)(h - 1);
                // Smooth feathered cross-section: 0 alpha at top & bottom edges, bright core at v=0.5!
                float edge = Mathf.Sin(v * Mathf.PI);
                float feather = Mathf.Pow(Mathf.Clamp01(edge), 2.2f);
                float core = Mathf.Pow(Mathf.Clamp01(edge), 6.0f);
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)(w - 1); // 0 = head near sword, 1 = tail fade
                    float tailFade = Mathf.Pow(1f - u, 1.35f);
                    float a = feather * tailFade;
                    float lum = Mathf.Lerp(0.82f, 1.0f, core);
                    cols[y * w + x] = new Color(lum, lum, lum, a);
                }
            }
            _sharedSilkRibbonTex.SetPixels(cols);
            _sharedSilkRibbonTex.Apply(false, true);
            return _sharedSilkRibbonTex;
        }

        private void EnsureTrailMaterial()
        {
            if (_trail == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null)
            {
                Texture2D silk = GetOrCreateSilkRibbonTexture();
                _runtimeTrailMat = new Material(s) { mainTexture = silk, color = Color.white };
                if (_runtimeTrailMat.HasProperty("_BaseMap")) _runtimeTrailMat.SetTexture("_BaseMap", silk);
                if (_runtimeTrailMat.HasProperty("_BaseColor")) _runtimeTrailMat.SetColor("_BaseColor", Color.white);
                if (_runtimeTrailMat.HasProperty("_Surface"))
                {
                    _runtimeTrailMat.SetFloat("_Surface", 1f);
                    _runtimeTrailMat.SetFloat("_Blend", 2f); // Additive silk Sword-Qi glow!
                    _runtimeTrailMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    _runtimeTrailMat.SetInt("_DstBlend", (int)BlendMode.One);
                    _runtimeTrailMat.SetInt("_ZWrite", 0);
                    _runtimeTrailMat.renderQueue = 3000;
                }
                _trail.sharedMaterial = _runtimeTrailMat;
            }
            _trail.minVertexDistance = 0.045f;
            _trail.numCornerVertices = 5;
            _trail.numCapVertices = 4;
            _trail.shadowCastingMode = ShadowCastingMode.Off;
            _trail.receiveShadows = false;
            _trail.widthCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.55f),
                new Keyframe(0.18f, 1.0f),
                new Keyframe(0.65f, 0.42f),
                new Keyframe(1.0f, 0.0f));
        }

        private void RebuildCachedTrailGradients()
        {
            _gradOrbit = BuildTrailGradient(0.28f);
            _gradDash = BuildTrailGradient(0.76f);
            _gradRetreat = BuildTrailGradient(0.32f);
        }

        private Gradient BuildTrailGradient(float peakAlpha)
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.Lerp(_swordColor, Color.white, 0.55f), 0f),
                    new GradientColorKey(_swordColor, 0.45f),
                    new GradientColorKey(_swordColor * 0.85f, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(peakAlpha * 0.85f, 0f),
                    new GradientAlphaKey(peakAlpha, 0.15f),
                    new GradientAlphaKey(0f, 1f)
                });
            return g;
        }

        private void SetTrailState(MotionState newState)
        {
            _state = newState;
            if (_trail == null) return;
            if (_gradOrbit == null) RebuildCachedTrailGradients();

            switch (newState)
            {
                case MotionState.Dashing:
                    _trail.time = 0.22f;
                    _trail.widthMultiplier = 0.13f;
                    _trail.colorGradient = _gradDash;
                    break;
                case MotionState.Retreating:
                    _trail.time = 0.15f;
                    _trail.widthMultiplier = 0.08f;
                    _trail.colorGradient = _gradRetreat;
                    break;
                default:
                    _trail.time = 0.14f;
                    _trail.widthMultiplier = 0.06f;
                    _trail.colorGradient = _gradOrbit;
                    break;
            }
        }
    }
}

using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeFlyingSwordActor : MonoBehaviour
    {
        public enum MotionState { Orbiting, Dashing, Retreating }

        private int _swordIndex;
        private int _formationTotal = 1;
        private int _swordLevel = 1;
        private Transform _orbitCenter;
        private MotionState _state = MotionState.Orbiting;

        private RemakeMonsterActor _target;
        private Vector3 _dashStartPos;
        private Vector3 _dashControlPos;
        private Vector3 _dashEndPos;
        private float _dashProgress;
        private bool _dashDidHit;
        private float _attackCooldown;
        private float _retreatTimer;
        private float _searchTimer;
        private float _punchTimer;

        private TrailRenderer _trail;
        private SpriteRenderer _spriteRenderer;
        private Material _runtimeTrailMat;
        private Color _swordColor = new Color(0.28f, 0.85f, 0.72f, 1f);
        private Gradient _gradOrbit;
        private Gradient _gradDash;
        private Gradient _gradRetreat;

        private void Awake()
        {
            _trail = GetComponent<TrailRenderer>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

            if (_spriteRenderer != null)
            {
                Sprite s = CodexArtLoader.LoadReusedSprite($"Sword{_swordLevel:00}");
                if (s != null)
                {
                    _spriteRenderer.sprite = s;
                    // Adjust normalized scale for tall sword illustration
                    _spriteRenderer.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
                    _spriteRenderer.transform.localRotation = Quaternion.Euler(90f, 0f, -45f);
                }
            }

            _swordColor = _swordLevel switch
            {
                <= 2 => new Color(0.24f, 0.86f, 0.68f, 1f), // Thanh Ngoc
                <= 4 => new Color(0.28f, 0.72f, 0.98f, 1f), // Thien Lam
                <= 6 => new Color(0.68f, 0.48f, 0.95f, 1f), // Tu Ha diu
                <= 8 => new Color(0.98f, 0.76f, 0.22f, 1f), // Kim Quang
                _ => new Color(1.0f, 0.92f, 0.58f, 1f)      // Tien Bach Kim
            };
            RebuildCachedTrailGradients();
            SetTrailState(_state);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_attackCooldown > 0f) _attackCooldown -= dt;
            if (_searchTimer > 0f) _searchTimer -= dt;

            if (_punchTimer > 0f)
            {
                _punchTimer -= dt;
                float k = Mathf.Clamp01(_punchTimer / 0.08f);
                transform.localScale = Vector3.one * (1f + 0.35f * k);
            }
            else if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
            }

            Vector3 center = _orbitCenter != null ? _orbitCenter.position + Vector3.up * 1.45f : Vector3.up * 1.45f;

            if (_retreatTimer > 0f)
            {
                if (_state != MotionState.Retreating) SetTrailState(MotionState.Retreating);
                _retreatTimer -= dt;
                Vector3 orbitPt = ComputeOrbitPosition(center);
                transform.position = Vector3.MoveTowards(transform.position, orbitPt, 18f * dt);
                OrientTowardTangent(transform.position - center, dt);
                return;
            }

            if (_target == null || !_target.IsAlive)
            {
                if (_state != MotionState.Orbiting) SetTrailState(MotionState.Orbiting);
                _target = null;
                _dashProgress = 0f;

                if (_searchTimer <= 0f && _attackCooldown <= 0f && RemakeArenaController.Instance != null)
                {
                    _target = RemakeArenaController.Instance.AcquireTarget(_swordIndex);
                    _searchTimer = 0.14f;
                    if (_target != null && _target.IsAlive)
                        BeginBezierSwoop(_target.HitTargetPoint);
                }

                Vector3 orbitPt = ComputeOrbitPosition(center);
                transform.position = Vector3.MoveTowards(transform.position, orbitPt, 12f * dt);
                OrientTowardTangent(transform.position - center, dt);
                return;
            }

            // Smooth Quadratic Bezier swoop past enemy for graceful flying-sword feel
            if (_state != MotionState.Dashing)
            {
                BeginBezierSwoop(_target.HitTargetPoint);
            }

            if (_target != null && _target.IsAlive)
                _dashEndPos = _target.HitTargetPoint;

            float speed = CultivationTable.CalculateSwordSpeed(_swordLevel);
            if (GameplayEngine.Instance != null)
                speed *= GameplayEngine.Instance.GetSwordSpeedMultiplier();

            float pathLen = Mathf.Max(1.5f, Vector3.Distance(_dashStartPos, _dashEndPos));
            _dashProgress += (speed / pathLen) * dt;
            float t = Mathf.Clamp01(_dashProgress);

            Vector3 prevPos = transform.position;
            Vector3 p01 = Vector3.Lerp(_dashStartPos, _dashControlPos, t);
            Vector3 p12 = Vector3.Lerp(_dashControlPos, _dashEndPos, t);
            Vector3 nextPos = Vector3.Lerp(p01, p12, t);
            transform.position = nextPos;

            Vector3 moveDir = nextPos - prevPos;
            if (moveDir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(moveDir.normalized);

            if (!_dashDidHit && t >= 0.90f)
            {
                _dashDidHit = true;
                PerformAttack(_dashEndPos);
            }
        }

        private void BeginBezierSwoop(Vector3 targetHitPt)
        {
            SetTrailState(MotionState.Dashing);
            _dashStartPos = transform.position;
            _dashEndPos = targetHitPt;
            Vector3 toTarget = _dashEndPos - _dashStartPos;
            Vector3 flatRight = Vector3.Cross(Vector3.up, toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector3.forward);
            float sideSign = (_swordIndex % 2 == 0) ? 1f : -1f;
            float arcSide = Mathf.Clamp(toTarget.magnitude * 0.42f, 0.8f, 2.2f);
            _dashControlPos = (_dashStartPos + _dashEndPos) * 0.5f + flatRight * (sideSign * arcSide) + Vector3.up * 0.45f;
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

            Vector3 knockDir = (hitPoint - (_orbitCenter != null ? _orbitCenter.position : transform.position));
            knockDir.y = 0f;
            if (knockDir.sqrMagnitude > 0.001f) knockDir.Normalize();

            _target.ApplyDamage(dmg, isCrit, knockDir);
            RemakeVfxPool.Instance?.SpawnSwordHit(hitPoint, isCrit);
            RemakeVfxPool.Instance?.RequestDamagePopup(hitPoint + Vector3.up * 0.5f, dmg, isCrit);
            RemakeAudioService.Instance?.PlaySwordHit();

            _punchTimer = 0.08f;
            _target = null;
            _attackCooldown = 0.42f;
            _retreatTimer = 0.22f;
            _searchTimer = 0.42f;
        }

        private Vector3 ComputeOrbitPosition(Vector3 center)
        {
            float angleStep = 360f / Mathf.Max(1, _formationTotal);
            float angle = Time.time * 135f + _swordIndex * angleStep;
            return center + Quaternion.Euler(0f, angle, 0f) * new Vector3(1.85f, Mathf.Sin(Time.time * 3f + _swordIndex) * 0.16f, 0f);
        }

        private void OrientTowardTangent(Vector3 radiusVec, float dt)
        {
            Vector3 tangent = Vector3.Cross(Vector3.up, radiusVec);
            if (tangent.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(tangent), 12f * dt);
            }
        }

        private void EnsureTrailMaterial()
        {
            if (_trail == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null)
            {
                _runtimeTrailMat = new Material(s) { color = Color.white };
                _trail.sharedMaterial = _runtimeTrailMat;
            }
        }

        private void RebuildCachedTrailGradients()
        {
            _gradOrbit = BuildTrailGradient(0.32f);
            _gradDash = BuildTrailGradient(0.90f);
            _gradRetreat = BuildTrailGradient(0.22f);
        }

        private Gradient BuildTrailGradient(float peakAlpha)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.Lerp(_swordColor, Color.white, 0.45f), 0f), new GradientColorKey(_swordColor, 1f) },
                new[] { new GradientAlphaKey(peakAlpha, 0f), new GradientAlphaKey(0f, 1f) });
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
                    _trail.time = 0.24f;
                    _trail.startWidth = 0.30f;
                    _trail.endWidth = 0.03f;
                    _trail.colorGradient = _gradDash;
                    break;
                case MotionState.Retreating:
                    _trail.time = 0.10f;
                    _trail.startWidth = 0.14f;
                    _trail.endWidth = 0f;
                    _trail.colorGradient = _gradRetreat;
                    break;
                default:
                    _trail.time = 0.14f;
                    _trail.startWidth = 0.18f;
                    _trail.endWidth = 0f;
                    _trail.colorGradient = _gradOrbit;
                    break;
            }
        }
    }
}

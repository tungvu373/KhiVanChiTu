using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeMonsterActor : MonoBehaviour
    {
        public MonsterDefinitionSO Definition { get; private set; }
        public double MaxHp { get; private set; }
        public double CurrentHp { get; private set; }
        public bool IsAlive { get; private set; }

        public Vector3 HitTargetPoint => transform.position + Vector3.up * (_definitionHover + 0.85f);

        private Animator _animator;
        private int _hashIsMoving;
        private int _hashAttack;
        private int _hashDie;
        private bool _hasMovingParam;
        private bool _hasAttackParam;
        private bool _hasDieParam;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;
        private GameObject _shadowDisc;
        private GameObject _bossAuraRing;
        private Transform _hpBarRoot;
        private Transform _hpBarFill;
        private Transform _hpBarTrail;
        private float _displayedHpRatio = 1f;
        private float _trailHpRatio = 1f;

        private Transform _playerTarget;
        private double _attackDamage;
        private float _attackTimer;
        private float _flashTimer;
        private float _scalePunchTimer;
        private float _lungeTimer;
        private bool _isDying;
        private float _deathTimer;
        private Vector3 _deathStartPos;
        private Vector3 _baseScale = Vector3.one;
        private float _definitionHover;
        private Action<RemakeMonsterActor> _onDeathCallback;

        private static readonly int ColorPropId = Shader.PropertyToID("_BaseColor");
        private static Camera _cachedMainCamera;

        private static Camera GetMainCamera()
        {
            if (_cachedMainCamera == null || !_cachedMainCamera.isActiveAndEnabled)
                _cachedMainCamera = Camera.main;
            return _cachedMainCamera;
        }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
            CacheAnimatorParams();
            CreateHelperVisuals();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (_isDying)
            {
                _deathTimer -= dt;
                float progress = 1f - Mathf.Clamp01(_deathTimer / 0.58f);
                float fadeU = Mathf.Clamp01((progress - 0.60f) / 0.40f);
                transform.localScale = Vector3.Lerp(_baseScale, _baseScale * 0.24f, fadeU * fadeU);
                transform.position = _deathStartPos - Vector3.up * (0.35f * fadeU);
                if (_deathTimer <= 0f)
                {
                    _isDying = false;
                    gameObject.SetActive(false);
                    _onDeathCallback?.Invoke(this);
                }
                return;
            }

            if (!IsAlive) return;

            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                ApplyHitFlashTint(_flashTimer > 0f);
            }

            if (_scalePunchTimer > 0f)
            {
                _scalePunchTimer -= dt;
                float k = Mathf.Clamp01(_scalePunchTimer / 0.10f);
                transform.localScale = _baseScale * (1f + 0.14f * k);
            }
            else if (transform.localScale != _baseScale)
            {
                transform.localScale = _baseScale;
            }

            if (_hpBarRoot != null && _hpBarRoot.gameObject.activeSelf)
            {
                Camera cam = GetMainCamera();
                if (cam != null)
                    _hpBarRoot.rotation = cam.transform.rotation;
                if (_hpBarTrail != null && _trailHpRatio > _displayedHpRatio)
                {
                    _trailHpRatio = Mathf.MoveTowards(_trailHpRatio, _displayedHpRatio, dt * 1.6f);
                    _hpBarTrail.localScale = new Vector3(_trailHpRatio, 1f, 1f);
                    _hpBarTrail.localPosition = new Vector3(-(1f - _trailHpRatio) * 0.5f, 0f, -0.005f);
                }
            }

            if (_bossAuraRing != null && _bossAuraRing.activeSelf)
            {
                _bossAuraRing.transform.Rotate(0f, 55f * dt, 0f, Space.Self);
            }

            if (_playerTarget == null) return;
            var engine = GameplayEngine.Instance;
            if (engine != null && (engine.IsRespawning || engine.IsInTribulation))
            {
                SetMovingParam(false);
                return;
            }
            if (Definition != null && Definition.isBoss && engine != null && engine.IsBossStunned)
            {
                SetMovingParam(false);
                return;
            }

            Vector3 targetGround = new Vector3(_playerTarget.position.x, _definitionHover, _playerTarget.position.z);
            Vector3 currentPos = transform.position;
            Vector3 flatOffset = new Vector3(targetGround.x - currentPos.x, 0f, targetGround.z - currentPos.z);
            float dist = flatOffset.magnitude;
            float range = Definition != null ? Definition.attackRange : 1.9f;

            if (flatOffset.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(flatOffset.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 6.5f * dt);
            }

            if (_lungeTimer > 0f)
            {
                _lungeTimer -= dt;
                float lu = Mathf.Sin((_lungeTimer / 0.22f) * Mathf.PI) * 0.22f;
                Vector3 fwd = flatOffset.sqrMagnitude > 0.001f ? flatOffset.normalized : transform.forward;
                transform.position = new Vector3(currentPos.x, _definitionHover, currentPos.z) + fwd * (lu * dt * 6.5f);
            }

            if (dist > range)
            {
                float speed = (Definition != null ? Definition.moveSpeed : 2.2f) * 0.62f;
                Vector3 next = Vector3.MoveTowards(currentPos, targetGround, speed * dt);
                next.y = _definitionHover + (Definition != null && Definition.hoverHeight > 0.05f ? Mathf.Sin(Time.time * 2.4f) * 0.16f : 0f);
                transform.position = next;
                SetMovingParam(true);
            }
            else
            {
                SetMovingParam(false);
                _attackTimer -= dt;
                if (_attackTimer <= 0f)
                {
                    float baseInterval = Definition != null ? Definition.attackInterval : 1.35f;
                    _attackTimer = baseInterval * 1.60f;
                    _lungeTimer = 0.22f;
                    if (_hasAttackParam && _animator != null) _animator.SetTrigger(_hashAttack);
                    if (Definition != null)
                        RemakeAudioService.Instance?.PlayMonsterAttack(Definition.ResolveAudioFamily());
                    engine?.OnPlayerDamaged(_attackDamage);
                }
            }
        }

        public void Spawn(
            MonsterDefinitionSO def,
            double maxHp,
            double attackDamage,
            Vector3 spawnPos,
            Transform playerTarget,
            Action<RemakeMonsterActor> onDeath)
        {
            Definition = def;
            MaxHp = Math.Max(1, maxHp);
            CurrentHp = MaxHp;
            _attackDamage = Math.Max(1, attackDamage);
            _playerTarget = playerTarget;
            _onDeathCallback = onDeath;

            _definitionHover = def != null ? def.hoverHeight : 0f;
            float sc = def != null ? def.visualScale : 1f;
            _baseScale = Vector3.one * sc;
            transform.localScale = _baseScale;
            transform.position = new Vector3(spawnPos.x, _definitionHover, spawnPos.z);

            Vector3 faceDir = (playerTarget != null ? playerTarget.position - transform.position : Vector3.back);
            faceDir.y = 0f;
            if (faceDir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(faceDir);

            IsAlive = true;
            _isDying = false;
            _flashTimer = 0f;
            _scalePunchTimer = 0f;
            _lungeTimer = 0f;
            _attackTimer = 1.25f;
            _displayedHpRatio = 1f;
            _trailHpRatio = 1f;
            ApplyHitFlashTint(false);

            bool useFakeShadow = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.UseFakeBlobShadows;
            if (_shadowDisc != null) _shadowDisc.SetActive(useFakeShadow);
            if (_bossAuraRing != null) _bossAuraRing.SetActive(def != null && def.isBoss);

            if (_hpBarRoot != null)
            {
                _hpBarRoot.localPosition = new Vector3(0f, def != null ? def.hpBarHeight : 2.1f, 0f);
                _hpBarRoot.gameObject.SetActive(false); // Shown on first hit or never for Boss (Boss uses HUD bar)
            }
            UpdateWorldHpBar();

            gameObject.SetActive(true);
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                if (_hasAttackParam) _animator.ResetTrigger(_hashAttack);
                if (_hasDieParam) _animator.ResetTrigger(_hashDie);
                _animator.Rebind();
                _animator.Update(0f);
            }
            SetMovingParam(true);
            if (def != null)
                RemakeAudioService.Instance?.PlayMonsterSpawn(def.ResolveAudioFamily(), def.monsterId);
        }

        public void DespawnImmediate()
        {
            IsAlive = false;
            _isDying = false;
            gameObject.SetActive(false);
        }

        public void SyncShadowSetting(bool useFakeBlobShadow)
        {
            if (_shadowDisc != null) _shadowDisc.SetActive(useFakeBlobShadow);
        }

        public void ApplyDamage(double amount, bool isCrit, Vector3 knockDir)
        {
            if (!IsAlive) return;
            var engine = GameplayEngine.Instance;
            float armor = Definition != null ? Definition.armorMitigation : 0f;
            float pen = engine != null ? engine.GetArmorPenetration() : 0f;
            double mitigated = amount * (1.0 - armor * (1f - pen));
            bool isBoss = Definition != null && Definition.isBoss;
            if (isBoss && engine != null)
            {
                if (engine.BossBrokenRemaining > 0f)
                    mitigated *= 1.35;
                else if (engine.BossShieldActive)
                    engine.ApplyBossShieldDamage(mitigated, false);
            }
            CurrentHp = Math.Max(0, CurrentHp - Math.Max(1, mitigated));
            _flashTimer = 0.09f;
            _scalePunchTimer = 0.10f;
            ApplyHitFlashTint(true);

            if (!isBoss && knockDir.sqrMagnitude > 0.001f)
            {
                Vector3 pushed = transform.position + knockDir * (isCrit ? 0.25f : 0.12f);
                transform.position = new Vector3(pushed.x, transform.position.y, pushed.z);
            }

            if (isBoss)
            {
                engine?.SetActiveBoss(true, Definition.displayName, CurrentHp, MaxHp);
            }
            else if (_hpBarRoot != null && !_hpBarRoot.gameObject.activeSelf && CurrentHp < MaxHp)
            {
                _hpBarRoot.gameObject.SetActive(true);
            }
            UpdateWorldHpBar();

            if (CurrentHp <= 0)
            {
                Die();
            }
            else if (Definition != null)
            {
                RemakeAudioService.Instance?.PlayMonsterHurt(Definition.ResolveAudioFamily());
            }
        }

        private void Die()
        {
            IsAlive = false;
            _isDying = true;
            _deathTimer = 0.58f;
            _deathStartPos = transform.position;
            if (_hpBarRoot != null) _hpBarRoot.gameObject.SetActive(false);
            if (_bossAuraRing != null) _bossAuraRing.SetActive(false);
            SetMovingParam(false);
            if (_hasDieParam && _animator != null) _animator.SetTrigger(_hashDie);
            RemakeVfxPool.Instance?.SpawnDeathDissolve(transform.position + Vector3.up * 0.8f);
            bool wasBoss = Definition != null && Definition.isBoss;
            if (Definition != null)
                RemakeAudioService.Instance?.PlayMonsterDeath(Definition.ResolveAudioFamily());
            if (wasBoss) RemakeVfxPool.Instance?.TriggerBossHitStop(0.045f);
            GameplayEngine.Instance?.OnMonsterKilled(Definition, wasBoss, transform.position);
        }

        private void UpdateWorldHpBar()
        {
            if (_hpBarFill == null) return;
            _displayedHpRatio = Mathf.Clamp01((float)(CurrentHp / MaxHp));
            _hpBarFill.localScale = new Vector3(_displayedHpRatio, 1f, 1f);
            _hpBarFill.localPosition = new Vector3(-(1f - _displayedHpRatio) * 0.5f, 0f, -0.01f);
            if (_hpBarTrail != null && _displayedHpRatio >= _trailHpRatio)
            {
                _trailHpRatio = _displayedHpRatio;
                _hpBarTrail.localScale = new Vector3(_trailHpRatio, 1f, 1f);
                _hpBarTrail.localPosition = new Vector3(-(1f - _trailHpRatio) * 0.5f, 0f, -0.005f);
            }
        }

        private void ApplyHitFlashTint(bool flashing)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null || r.transform == _hpBarFill || (r.transform.parent != null && r.transform.parent == _hpBarRoot))
                    continue;
                r.GetPropertyBlock(_mpb);
                if (flashing) _mpb.SetColor(ColorPropId, new Color(1.65f, 0.85f, 0.85f, 1f));
                else _mpb.Clear();
                r.SetPropertyBlock(_mpb);
            }
        }

        private void CacheAnimatorParams()
        {
            _hashIsMoving = Animator.StringToHash("IsMoving");
            _hashAttack = Animator.StringToHash("Attack");
            _hashDie = Animator.StringToHash("Die");
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            foreach (AnimatorControllerParameter p in _animator.parameters)
            {
                if (p.nameHash == _hashIsMoving) _hasMovingParam = true;
                else if (p.nameHash == _hashAttack) _hasAttackParam = true;
                else if (p.nameHash == _hashDie) _hasDieParam = true;
            }
        }

        private void SetMovingParam(bool moving)
        {
            if (_hasMovingParam && _animator != null)
                _animator.SetBool(_hashIsMoving, moving);
        }

        private void CreateHelperVisuals()
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");

            // Lightweight fake shadow disc (visible when 3D shadows = Off for iPhone Safari performance)
            _shadowDisc = RemakeRenderPrimitive.Create(PrimitiveType.Cylinder);
            _shadowDisc.name = "BlobShadow";
            _shadowDisc.transform.SetParent(transform, false);
            _shadowDisc.transform.localPosition = new Vector3(0f, -_definitionHover + 0.02f, 0f);
            _shadowDisc.transform.localScale = new Vector3(1.1f, 0.01f, 1.1f);
            var shadowMat = new Material(unlit) { color = new Color(0.06f, 0.14f, 0.08f, 0.28f) };
            if (shadowMat.HasProperty("_BaseColor")) shadowMat.SetColor("_BaseColor", new Color(0.06f, 0.14f, 0.08f, 0.28f));
            if (shadowMat.HasProperty("_Surface"))
            {
                shadowMat.SetFloat("_Surface", 1f);
                shadowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                shadowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                shadowMat.SetInt("_ZWrite", 0);
                shadowMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                shadowMat.renderQueue = 2550;
            }
            var shadowMr = _shadowDisc.GetComponent<Renderer>();
            shadowMr.sharedMaterial = shadowMat;
            shadowMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowMr.receiveShadows = false;

            // Crimson-sunlit Boss aura ring (matches user reference visual focal point)
            _bossAuraRing = new GameObject("BossAuraRing");
            _bossAuraRing.transform.SetParent(transform, false);
            _bossAuraRing.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            var lr = _bossAuraRing.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 24;
            lr.startWidth = 0.14f;
            lr.endWidth = 0.14f;
            lr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? unlit)
            {
                color = new Color(0.95f, 0.22f, 0.16f, 0.9f)
            };
            for (int i = 0; i < 24; i++)
            {
                float a = i * (Mathf.PI * 2f / 24f);
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * 1.4f, 0f, Mathf.Sin(a) * 1.4f));
            }
            _bossAuraRing.SetActive(false);

            // Tiny slim dual-layer billboard HP bar with delayed Damage Trail
            var barGo = new GameObject("WorldHpBar");
            barGo.transform.SetParent(transform, false);
            _hpBarRoot = barGo.transform;

            var bgQuad = RemakeRenderPrimitive.Create(PrimitiveType.Quad);
            bgQuad.name = "HpBg";
            bgQuad.transform.SetParent(_hpBarRoot, false);
            bgQuad.transform.localScale = new Vector3(1.24f, 0.12f, 1f);
            var bgMr = bgQuad.GetComponent<MeshRenderer>();
            bgMr.sharedMaterial = new Material(unlit) { color = new Color(0.12f, 0.18f, 0.16f, 0.82f) };
            bgMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bgMr.receiveShadows = false;

            var trailHolder = new GameObject("HpTrailHolder");
            trailHolder.transform.SetParent(_hpBarRoot, false);
            trailHolder.transform.localScale = new Vector3(1.18f, 0.075f, 1f);
            trailHolder.transform.localPosition = new Vector3(0f, 0f, -0.005f);
            var trailQuad = RemakeRenderPrimitive.Create(PrimitiveType.Quad);
            trailQuad.name = "HpTrail";
            trailQuad.transform.SetParent(trailHolder.transform, false);
            var trMr = trailQuad.GetComponent<MeshRenderer>();
            trMr.sharedMaterial = new Material(unlit) { color = new Color(1f, 0.94f, 0.80f, 0.95f) };
            trMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trMr.receiveShadows = false;
            _hpBarTrail = trailQuad.transform;

            var fillHolder = new GameObject("HpFillHolder");
            fillHolder.transform.SetParent(_hpBarRoot, false);
            fillHolder.transform.localScale = new Vector3(1.18f, 0.075f, 1f);
            fillHolder.transform.localPosition = new Vector3(0f, 0f, -0.01f);

            var fillQuad = RemakeRenderPrimitive.Create(PrimitiveType.Quad);
            fillQuad.name = "HpFill";
            fillQuad.transform.SetParent(fillHolder.transform, false);
            var flMr = fillQuad.GetComponent<MeshRenderer>();
            flMr.sharedMaterial = new Material(unlit) { color = new Color(0.90f, 0.25f, 0.20f, 1f) };
            flMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            flMr.receiveShadows = false;
            _hpBarFill = fillQuad.transform;

            barGo.SetActive(false);
        }
    }
}

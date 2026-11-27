using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeArenaController : MonoBehaviour
    {
        public static RemakeArenaController Instance { get; private set; }

        [Header("Arena References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform swordsRoot;
        [SerializeField] private Transform groundFormationRing;
        [SerializeField] private RemakeFlyingSwordActor flyingSwordPrefab;
        [SerializeField] private MonsterDefinitionSO[] act1NormalMonsters;
        [SerializeField] private MonsterDefinitionSO act1BossMonster;

        public Vector3 PlayerWorldPosition => playerTransform != null ? playerTransform.position : Vector3.zero;

        private readonly Dictionary<string, Queue<RemakeMonsterActor>> _monsterPools = new Dictionary<string, Queue<RemakeMonsterActor>>();
        private readonly List<RemakeMonsterActor> _activeMonsters = new List<RemakeMonsterActor>();
        private readonly List<RemakeFlyingSwordActor> _activeSwords = new List<RemakeFlyingSwordActor>();

        private Animator _playerAnimator;
        private GameObject _playerShadowDisc;
        private LineRenderer _playerRealmRing;
        private Material _playerRealmRingMat;
        private float _spawnCooldown;
        private float _waveCheckTimer = 0.25f;
        private int _currentWaveIndex;
        private int _totalWaves = 3;
        private int[] _waveCounts = { 3, 3, 4 };
        private bool _finalWaveIsBoss;

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

            if (playerTransform != null)
            {
                playerTransform.localScale = Vector3.one * 1.25f;
                _playerAnimator = playerTransform.GetComponentInChildren<Animator>();
                CreatePlayerShadowDisc();
                CreatePlayerRealmAura();
                SyncPlayerRealmAura();
            }

            PrewarmMonsterPools();
        }

        private void OnEnable()
        {
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnLoiPhatCast += HandleLoiPhatCast;
                GameplayEngine.Instance.OnSkillCast += HandleSkillCast;
                GameplayEngine.Instance.OnTribulationStateChanged += HandleTribulationStateChanged;
                GameplayEngine.Instance.OnTribulationStrike += HandleTribulationStrike;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged += HandleGraphicsChanged;
        }

        private void OnDisable()
        {
            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.OnLoiPhatCast -= HandleLoiPhatCast;
                GameplayEngine.Instance.OnSkillCast -= HandleSkillCast;
                GameplayEngine.Instance.OnTribulationStateChanged -= HandleTribulationStateChanged;
                GameplayEngine.Instance.OnTribulationStrike -= HandleTribulationStrike;
            }
            if (GraphicsSettingsService.Instance != null)
                GraphicsSettingsService.Instance.OnGraphicsChanged -= HandleGraphicsChanged;
        }

        private void OnDestroy()
        {
            if (_playerRealmRingMat != null) Destroy(_playerRealmRingMat);
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            SyncPlayerSwordsVisual();
            HandleGraphicsChanged();
            SpawnNextWaveIfNeeded();
        }

        private void Update()
        {
            if (SceneFlowController.Instance != null && !SceneFlowController.Instance.IsInGame) return;

            _spawnCooldown -= Time.deltaTime;
            _waveCheckTimer += Time.deltaTime;
            if (_waveCheckTimer >= 0.25f)
            {
                _waveCheckTimer = 0f;
                SpawnNextWaveIfNeeded();
            }

            // Keep ground formation ring & realm aura rotating (faster during 15s Tribulation Seclusion!)
            bool inTrib = GameplayEngine.Instance != null && GameplayEngine.Instance.IsInTribulation;
            if (groundFormationRing != null)
                groundFormationRing.Rotate(0f, 0f, (inTrib ? 92f : 18f) * Time.deltaTime, Space.Self);
            if (_playerRealmRing != null)
                _playerRealmRing.transform.Rotate(0f, (inTrib ? -135f : -24f) * Time.deltaTime, 0f, Space.Self);

            // Face closest active enemy cleanly
            if (playerTransform != null && _activeMonsters.Count > 0)
            {
                RemakeMonsterActor nearest = FindNearestActive();
                if (nearest != null)
                {
                    Vector3 look = nearest.transform.position - playerTransform.position;
                    look.y = 0f;
                    if (look.sqrMagnitude > 0.01f)
                        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(look), 7f * Time.deltaTime);
                }
            }
        }

        public bool HasAliveTarget()
        {
            for (int i = 0; i < _activeMonsters.Count; i++)
                if (_activeMonsters[i] != null && _activeMonsters[i].IsAlive) return true;
            return false;
        }

        public RemakeMonsterActor AcquireTarget(int swordIndex)
        {
            if (_activeMonsters.Count == 0) return null;
            int aliveCount = 0;
            for (int i = 0; i < _activeMonsters.Count; i++)
                if (_activeMonsters[i] != null && _activeMonsters[i].IsAlive) aliveCount++;
            if (aliveCount == 0) return null;

            int pick = swordIndex % aliveCount;
            int seen = 0;
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                var m = _activeMonsters[i];
                if (m == null || !m.IsAlive) continue;
                if (seen == pick) return m;
                seen++;
            }
            return FindNearestActive();
        }

        public bool TryFindPriorityTargetPos(out Vector3 pos)
        {
            RemakeMonsterActor best = null;
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                var m = _activeMonsters[i];
                if (m == null || !m.IsAlive) continue;
                if (m.Definition != null && m.Definition.isBoss) { best = m; break; }
                if (best == null || m.CurrentHp > best.CurrentHp) best = m;
            }
            if (best != null)
            {
                pos = best.transform.position;
                return true;
            }
            pos = Vector3.zero;
            return false;
        }

        public void SyncPlayerSwordsVisual()
        {
            if (flyingSwordPrefab == null || swordsRoot == null) return;
            var save = SaveService.Current;
            int maxSlots = CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex);
            int targetCount = Mathf.Clamp(save.equippedSwordLevels.Count, 1, maxSlots);

            while (_activeSwords.Count < targetCount)
            {
                var sword = Instantiate(flyingSwordPrefab, swordsRoot);
                sword.gameObject.name = $"RemakeSword_{_activeSwords.Count + 1}";
                _activeSwords.Add(sword);
            }

            for (int i = 0; i < _activeSwords.Count; i++)
            {
                bool active = i < targetCount;
                _activeSwords[i].gameObject.SetActive(active);
                if (active)
                {
                    int lv = i < save.equippedSwordLevels.Count ? save.equippedSwordLevels[i] : 1;
                    _activeSwords[i].Configure(i, targetCount, lv, playerTransform);
                }
            }
        }

        public void ResetForStageChange()
        {
            for (int i = _activeMonsters.Count - 1; i >= 0; i--)
            {
                ReturnMonsterToPool(_activeMonsters[i]);
            }
            _activeMonsters.Clear();
            _currentWaveIndex = 0;
            _spawnCooldown = 0f;
            GameplayEngine.Instance?.SetActiveBoss(false, "", 0, 0);
            SpawnNextWaveIfNeeded();
        }

        private MonsterDefinitionSO[] ResolveNormalPool(GameplayEngine engine)
        {
            if (act1NormalMonsters != null && act1NormalMonsters.Length > 0 && act1NormalMonsters[0] != null)
                return act1NormalMonsters;
            var stage = engine != null ? engine.GetCurrentStageSpec() : null;
            return stage != null ? stage.enemyPool : null;
        }

        private MonsterDefinitionSO ResolveBossDefinition(GameplayEngine engine)
        {
            if (act1BossMonster != null) return act1BossMonster;
            var stage = engine != null ? engine.GetCurrentStageSpec() : null;
            return stage != null ? stage.bossMonster : null;
        }

        private void SpawnNextWaveIfNeeded()
        {
            // Strict Wave-Based Spawning: NEVER trickle-spawn replacements while current wave still has alive monsters!
            if (_activeMonsters.Count > 0 || _spawnCooldown > 0f) return;
            var engine = GameplayEngine.Instance;
            if (engine == null || engine.IsInTribulation) return;

            var stage = engine.GetCurrentStageSpec();
            GameplayEngine.GetWavePlan(engine.ActiveArenaMode, stage.isBossStage, engine.ActiveChallengeFloor,
                out _totalWaves, out _waveCounts, out _finalWaveIsBoss);

            _currentWaveIndex++;
            if (_currentWaveIndex > _totalWaves)
            {
                if (engine.ActiveArenaMode == RemakeArenaMode.Campaign)
                    _currentWaveIndex = 1; // Continuous idle wave loop in Campaign
                else
                    return; // Challenge mode ends on last kill
            }

            int count = _waveCounts[Mathf.Clamp(_currentWaveIndex - 1, 0, _waveCounts.Length - 1)];
            bool isFinalWave = _currentWaveIndex >= _totalWaves;
            Vector3 center = PlayerWorldPosition;
            MonsterDefinitionSO[] pool = ResolveNormalPool(engine);
            MonsterDefinitionSO bossDef = ResolveBossDefinition(engine);

            string banner = null;

            if (engine.ActiveArenaMode == RemakeArenaMode.DemonTower)
            {
                var towerSpec = CultivationTable.GetDemonTowerFloorSpec(engine.ActiveChallengeFloor);
                if (_finalWaveIsBoss && isFinalWave && bossDef != null)
                {
                    SpawnOneCustom(bossDef, towerSpec.hp, towerSpec.dmg, center + new Vector3(1.0f, 0f, 2.6f));
                    engine.SetActiveBoss(true, $"Hộ Tháp Tầng {engine.ActiveChallengeFloor}", towerSpec.hp, towerSpec.hp);
                    MeadowEnvironmentController.Instance?.AddCameraTrauma(0.24f);
                    banner = $"Hộ Tháp Thủ Lĩnh Tầng {engine.ActiveChallengeFloor} xuất hiện!";
                }
                else if (pool != null && pool.Length > 0)
                {
                    double waveHp = _finalWaveIsBoss ? Math.Round(towerSpec.hp * 0.22) : towerSpec.hp;
                    SpawnWaveRingCustom(pool, count, waveHp, towerSpec.dmg, center, isFinalWave && !_finalWaveIsBoss);
                    banner = isFinalWave ? $"Tầng {engine.ActiveChallengeFloor} — Đợt Cuối (Tinh Anh)!" : $"Vạn Yêu Tháp T{engine.ActiveChallengeFloor} — Đợt {_currentWaveIndex}/{_totalWaves}";
                }
            }
            else if (engine.ActiveArenaMode == RemakeArenaMode.SecretRealm)
            {
                var srSpec = CultivationTable.GetSecretRealmTierSpec(engine.ActiveSecretRealmKind, engine.ActiveSecretRealmTier);
                if (pool != null && pool.Length > 0)
                {
                    SpawnWaveRingCustom(pool, count, srSpec.hp, srSpec.dmg, center, isFinalWave);
                    banner = isFinalWave ? $"{srSpec.title} — Đợt Cuối (Thủ Vệ Tinh Anh)!" : $"{srSpec.title} — Đợt {_currentWaveIndex}/{_totalWaves}";
                }
            }
            else if (_finalWaveIsBoss && isFinalWave)
            {
                if (bossDef != null)
                {
                    SpawnOne(bossDef, stage, center + new Vector3(1.1f, 0f, 2.4f));
                    engine.SetActiveBoss(true, bossDef.displayName, stage.enemyHp * bossDef.hpMultiplier, stage.enemyHp * bossDef.hpMultiplier);
                    MeadowEnvironmentController.Instance?.AddCameraTrauma(0.28f);
                    banner = $"Thủ Lĩnh {bossDef.displayName} giáng lâm!";
                }
            }
            else if (pool != null && pool.Length > 0)
            {
                double hpMult = stage.isBossStage ? 0.20 : 1.0; // Vanguard wave before Boss X-10
                SpawnWaveRingCampaign(pool, count, stage, center, hpMult, isFinalWave && !stage.isBossStage);
                if (_currentWaveIndex >= 2)
                    banner = isFinalWave ? $"Ải {stage.stageCode} — Đợt Cuối (Yêu Thú Tinh Anh)!" : $"Ải {stage.stageCode} — Đợt {_currentWaveIndex}/{_totalWaves}";
                else if (stage.isBossStage)
                    banner = $"Ải Boss {stage.stageCode} — Dọn Hộ Vệ Đợt 1/{_totalWaves}!";
            }

            RemakeVfxPool.Instance?.SpawnDeathDissolve(center + Vector3.up * 0.3f);
            engine.ReportWaveState(_currentWaveIndex, _totalWaves, _activeMonsters.Count, banner);
        }

        private void SpawnWaveRingCampaign(MonsterDefinitionSO[] pool, int count, ActStageDatabaseSO.StageSpec stage, Vector3 center, double hpScale, bool includeElite)
        {
            float baseAngle = UnityEngine.Random.Range(0f, 360f);
            float sector = 360f / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                MonsterDefinitionSO def = pool[UnityEngine.Random.Range(0, pool.Length)];
                if (def == null) continue;
                bool isElite = includeElite && i == 0;
                float deg = baseAngle + i * sector + UnityEngine.Random.Range(-18f, 18f);
                float rad = deg * Mathf.Deg2Rad;
                Vector3 pos = center + new Vector3(Mathf.Cos(rad) * UnityEngine.Random.Range(3.4f, 4.3f), 0f, Mathf.Sin(rad) * UnityEngine.Random.Range(4.0f, 5.8f));
                double hp = Math.Round(stage.enemyHp * def.hpMultiplier * hpScale * (isElite ? 1.35 : 1.0));
                double dmg = Math.Round(stage.enemyDamage * def.damageMultiplier * (isElite ? 1.15 : 1.0));
                SpawnOneCustom(def, hp, dmg, pos);
                if (isElite && _activeMonsters.Count > 0)
                    _activeMonsters[_activeMonsters.Count - 1].transform.localScale *= 1.18f;
            }
        }

        private void SpawnWaveRingCustom(MonsterDefinitionSO[] pool, int count, double baseHp, double baseDmg, Vector3 center, bool includeElite)
        {
            float baseAngle = UnityEngine.Random.Range(0f, 360f);
            float sector = 360f / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                MonsterDefinitionSO def = pool[i % pool.Length];
                if (def == null) continue;
                bool isElite = includeElite && i == 0;
                float deg = baseAngle + i * sector + UnityEngine.Random.Range(-16f, 16f);
                float rad = deg * Mathf.Deg2Rad;
                Vector3 pos = center + new Vector3(Mathf.Cos(rad) * 3.9f, 0f, Mathf.Sin(rad) * 4.8f);
                SpawnOneCustom(def, baseHp * (isElite ? 1.35 : 1.0), baseDmg * (isElite ? 1.15 : 1.0), pos);
                if (isElite && _activeMonsters.Count > 0)
                    _activeMonsters[_activeMonsters.Count - 1].transform.localScale *= 1.18f;
            }
        }

        private void SpawnOneCustom(MonsterDefinitionSO def, double hp, double dmg, Vector3 pos)
        {
            if (def == null || def.prefab == null) return;
            RemakeMonsterActor actor = RentMonsterFromPool(def);
            if (actor == null) return;
            _activeMonsters.Add(actor);
            actor.Spawn(def, Math.Round(hp), Math.Round(dmg), pos, playerTransform, HandleMonsterDied);
        }

        private void SpawnOne(MonsterDefinitionSO def, ActStageDatabaseSO.StageSpec stage, Vector3 pos)
        {
            if (def == null || def.prefab == null) return;
            RemakeMonsterActor actor = RentMonsterFromPool(def);
            if (actor == null) return;

            double hp = Math.Round(stage.enemyHp * def.hpMultiplier);
            double dmg = Math.Round(stage.enemyDamage * def.damageMultiplier);
            _activeMonsters.Add(actor);
            actor.Spawn(def, hp, dmg, pos, playerTransform, HandleMonsterDied);
        }

        private void HandleMonsterDied(RemakeMonsterActor deadActor)
        {
            _activeMonsters.Remove(deadActor);
            ReturnMonsterToPool(deadActor);
            GameplayEngine.Instance?.ReportWaveState(_currentWaveIndex, _totalWaves, _activeMonsters.Count, null);
            if (_activeMonsters.Count == 0)
                _spawnCooldown = 0.75f; // Clean breather between waves before next wave spawns
        }

        private void HandleLoiPhatCast(Vector3 targetPos, double damage, bool isCrit)
        {
            RemakeVfxPool.Instance?.SpawnLoiPhatLightning(targetPos);
            MeadowEnvironmentController.Instance?.AddCameraTrauma(0.35f);
            DamageEnemiesInRadius(targetPos, 4.0f, damage, 1.2f, 0.65, Vector3.zero, false);
        }

        private void HandleTribulationStateChanged(bool entering)
        {
            MeadowEnvironmentController.Instance?.SetTribulationStormMode(entering);
            RemakeVfxPool.Instance?.SetTribulationSeclusionVisual(entering, PlayerWorldPosition);

            if (entering)
            {
                for (int i = _activeMonsters.Count - 1; i >= 0; i--)
                    ReturnMonsterToPool(_activeMonsters[i]);
                _activeMonsters.Clear();
                if (_playerRealmRingMat != null)
                {
                    Color gold = new Color(1.0f, 0.85f, 0.22f, 0.98f);
                    _playerRealmRingMat.color = gold;
                    if (_playerRealmRingMat.HasProperty("_BaseColor"))
                        _playerRealmRingMat.SetColor("_BaseColor", gold);
                }
            }
            else
            {
                SyncPlayerRealmAura();
                _spawnCooldown = 0.75f;
                SpawnNextWaveIfNeeded();
            }
        }

        private void HandleTribulationStrike(int strikeIndex, int totalStrikes)
        {
            float intensity = Mathf.Clamp01((float)strikeIndex / Mathf.Max(1, totalStrikes));
            Color flashTint = strikeIndex <= 3
                ? new Color(0.55f, 0.92f, 1.0f, 1f)
                : strikeIndex <= 6
                    ? new Color(0.84f, 0.56f, 1.0f, 1f)
                    : new Color(1.0f, 0.86f, 0.36f, 1f);

            RemakeVfxPool.Instance?.SpawnTribulationStrike(PlayerWorldPosition, strikeIndex, totalStrikes);
            MeadowEnvironmentController.Instance?.TriggerTribulationSkyFlash(intensity, flashTint);
            MeadowEnvironmentController.Instance?.AddCameraTrauma(Mathf.Lerp(0.42f, 0.88f, intensity));
            RemakeAudioService.Instance?.PlayLoiPhat();
        }

        private void HandleSkillCast(int slot, Vector3 targetPos, double damage, bool isCrit)
        {
            if (slot == 0) return; // Already handled by OnLoiPhatCast
            MeadowEnvironmentController.Instance?.AddCameraTrauma(slot == 3 ? 0.50f : 0.30f);

            switch (slot)
            {
                case 1: // Am Duong Tran: Centered domain wave around player hitting all nearby monsters
                    RemakeVfxPool.Instance?.SpawnYinYangDomain(targetPos);
                    DamageEnemiesInRadius(targetPos, 6.2f, damage, 6.2f, 1.0, Vector3.zero, false);
                    break;
                case 2: // Van Kiem Quy Tong: Multi-hit volley on up to 6 monsters across arena
                    RemakeVfxPool.Instance?.SpawnWanJianVolley(targetPos);
                    DamageEnemiesInRadius(targetPos, 7.5f, damage * 0.72, 2.5f, 0.75, Vector3.zero, true);
                    break;
                default: // An Chuong: Heavy palm impact with knockback
                    RemakeVfxPool.Instance?.SpawnAnChuongStrike(targetPos);
                    DamageEnemiesInRadius(targetPos, 5.0f, damage, 1.8f, 0.75, (targetPos - PlayerWorldPosition).normalized, false);
                    break;
            }
        }

        private void DamageEnemiesInRadius(Vector3 center, float radius, double fullDmg, float innerRadius, double outerRatio, Vector3 knockBias, bool randomCrit)
        {
            float radSq = radius * radius;
            float inSq = innerRadius * innerRadius;
            var engine = GameplayEngine.Instance;
            engine?.BeginAoEDamageBatch();
            try
            {
                for (int i = _activeMonsters.Count - 1; i >= 0; i--)
                {
                    if (i >= _activeMonsters.Count) continue;
                    var m = _activeMonsters[i];
                    if (m == null || !m.IsAlive) continue;
                    float distSq = (m.transform.position - center).sqrMagnitude;
                    if (distSq <= radSq)
                    {
                        double dmg = distSq < inSq ? fullDmg : fullDmg * outerRatio;
                        Vector3 push = knockBias.sqrMagnitude > 0.01f ? knockBias : (m.transform.position - PlayerWorldPosition).normalized;
                        m.ApplyDamage(dmg, !randomCrit || UnityEngine.Random.value < 0.5f, push);
                        RemakeVfxPool.Instance?.RequestDamagePopup(m.HitTargetPoint, dmg, true);
                    }
                }
            }
            finally
            {
                engine?.EndAoEDamageBatch();
            }
        }

        private void HandleGraphicsChanged()
        {
            bool useFakeShadow = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.UseFakeBlobShadows;
            if (_playerShadowDisc != null) _playerShadowDisc.SetActive(useFakeShadow);
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                if (_activeMonsters[i] != null) _activeMonsters[i].SyncShadowSetting(useFakeShadow);
            }
        }

        private void PrewarmMonsterPools()
        {
            if (act1NormalMonsters != null)
            {
                for (int i = 0; i < act1NormalMonsters.Length; i++)
                {
                    var def = act1NormalMonsters[i];
                    if (def == null || def.prefab == null) continue;
                    for (int k = 0; k < 3; k++)
                        CreatePooledInstance(def);
                }
            }
            if (act1BossMonster != null && act1BossMonster.prefab != null)
                CreatePooledInstance(act1BossMonster);
        }

        private RemakeMonsterActor CreatePooledInstance(MonsterDefinitionSO def)
        {
            if (!_monsterPools.TryGetValue(def.monsterId, out Queue<RemakeMonsterActor> q))
            {
                q = new Queue<RemakeMonsterActor>();
                _monsterPools[def.monsterId] = q;
            }
            GameObject go = Instantiate(def.prefab, transform);
            go.name = $"Pool_{def.monsterId}";
            go.SetActive(false);
            var actor = go.GetComponent<RemakeMonsterActor>() ?? go.AddComponent<RemakeMonsterActor>();
            q.Enqueue(actor);
            return actor;
        }

        private RemakeMonsterActor RentMonsterFromPool(MonsterDefinitionSO def)
        {
            if (!_monsterPools.TryGetValue(def.monsterId, out Queue<RemakeMonsterActor> q) || q.Count == 0)
                CreatePooledInstance(def);

            q = _monsterPools[def.monsterId];
            return q.Count > 0 ? q.Dequeue() : null;
        }

        private void ReturnMonsterToPool(RemakeMonsterActor actor)
        {
            if (actor == null) return;
            actor.DespawnImmediate();
            string id = actor.Definition != null ? actor.Definition.monsterId : string.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                if (!_monsterPools.TryGetValue(id, out Queue<RemakeMonsterActor> q))
                {
                    q = new Queue<RemakeMonsterActor>();
                    _monsterPools[id] = q;
                }
                q.Enqueue(actor);
            }
        }

        private RemakeMonsterActor FindNearestActive()
        {
            RemakeMonsterActor best = null;
            float bestDist = float.MaxValue;
            Vector3 pPos = PlayerWorldPosition;
            for (int i = 0; i < _activeMonsters.Count; i++)
            {
                var m = _activeMonsters[i];
                if (m == null || !m.IsAlive) continue;
                float d = (m.transform.position - pPos).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = m; }
            }
            return best;
        }

        private void CreatePlayerShadowDisc()
        {
            if (playerTransform == null) return;
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            _playerShadowDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _playerShadowDisc.name = "PlayerBlobShadow";
            Destroy(_playerShadowDisc.GetComponent<Collider>());
            _playerShadowDisc.transform.SetParent(playerTransform, false);
            _playerShadowDisc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            _playerShadowDisc.transform.localScale = new Vector3(1.1f, 0.01f, 1.1f);
            var mr = _playerShadowDisc.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(unlit) { color = new Color(0.08f, 0.16f, 0.08f, 0.48f) };
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        private void CreatePlayerRealmAura()
        {
            if (playerTransform == null) return;
            var go = new GameObject("PlayerRealmAura");
            go.transform.SetParent(playerTransform, false);
            go.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 28;
            lr.startWidth = 0.09f;
            lr.endWidth = 0.09f;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            _playerRealmRingMat = new Material(pShader) { color = new Color(0.35f, 0.90f, 0.72f, 0.85f) };
            lr.sharedMaterial = _playerRealmRingMat;
            for (int i = 0; i < 28; i++)
            {
                float a = i * (Mathf.PI * 2f / 28f);
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * 1.18f, 0f, Mathf.Sin(a) * 1.18f));
            }
            _playerRealmRing = lr;
        }

        public void SyncPlayerRealmAura()
        {
            if (_playerRealmRingMat == null) return;
            int realm = SaveService.Current != null ? SaveService.Current.realmIndex : 0;
            Color c = realm switch
            {
                0 => new Color(0.45f, 0.92f, 0.74f, 0.80f), // Luyen Khi - Thanh Ngoc
                1 => new Color(0.28f, 0.78f, 0.98f, 0.86f), // Truc Co - Thien Lam
                2 => new Color(0.98f, 0.82f, 0.28f, 0.90f), // Kim Dan - Kim Quang
                3 => new Color(0.70f, 0.48f, 0.98f, 0.92f), // Nguyen Anh - Tu Ha
                4 => new Color(0.98f, 0.56f, 0.24f, 0.94f), // Hoa Than - Xich Diem
                _ => new Color(1.00f, 0.92f, 0.65f, 0.96f)  // Luyen Hu..Do Kiep - Tien Kim
            };
            _playerRealmRingMat.color = c;
            if (_playerRealmRingMat.HasProperty("_BaseColor"))
                _playerRealmRingMat.SetColor("_BaseColor", c);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class GameplayEngine : MonoBehaviour
    {
        public static GameplayEngine Instance { get; private set; }

        [SerializeField] private ActStageDatabaseSO stageDatabase;

        public event Action OnStatsChanged;
        public event Action OnStageChanged;
        public event Action OnQuestOrTutorialChanged;
        public event Action<string> OnToastMessage;
        public event Action<bool, string> OnBreakthroughResolved;
        public event Action<bool, int, float> OnSwordMergeResolved;
        public event Action<Vector3, double, bool> OnLoiPhatCast;
        public event Action<int, Vector3, double, bool> OnSkillCast;
        public event Action<string, double, double> OnBossStatusUpdated; // name, hp, maxHp (null when none)
        public event Action<bool> OnTribulationStateChanged;
        public event Action<int, int> OnTribulationStrike; // (strikeIndex 1..9, total)
        public event Action<bool> OnRespawnStateChanged;
        public sealed class OfflineCatchupSummary
        {
            public double elapsedSeconds;
            public double efficiency;
            public int killsSimulated;
            public double tuViGained;
            public double linhThachGained;
            public int herbsGained;
            public int herbTier;
            public int swordsGained;
            public bool bonusClaimed;
        }

        public event Action<OfflineCatchupSummary> OnOfflineCatchupSummary;
        public OfflineCatchupSummary LastOfflineSummary { get; private set; }

        public event Action OnAlchemyBrewResolved;
        public event Action<int, int, string> OnWaveStarted;
        public event Action OnBossShieldStateChanged;

        // AoE Batching & O(1) Cached Combat/Drop Multipliers (zero event storms & zero per-hit list scans!)
        private int _aoeBatchDepth;
        private bool _batchStatsDirty;
        private bool _batchStageDirty;
        private bool _batchQuestDirty;
        private bool _batchPendingArenaReset;
        private bool _batchPendingCheckpoint;

        private bool _statCacheDirty = true;
        private float _cachedSwordDmgMult = 1f;
        private float _cachedSwordSpeedMult = 1f;
        private float _cachedArmorPen;
        private float _cachedBossMult = 1f;
        private float _cachedDamageReduction;
        private float _cachedTuViMultNoKyNgo = 1f;
        private float _cachedLinhThachMult = 1f;
        private float _cachedCritChance = 0.08f;
        private double _cachedCritDmgMult = 2.0;
        private double _cachedTotalSwordAtk = 10.0;
        private float _cachedBodyLeechPct;
        private float _cachedHerbMult = 1f;
        private float _cachedOreMult = 1f;
        private float _cachedGearMult = 1f;
        private float _cachedSwordDropMult = 1f;
        private float _cachedCdRate = 1f;
        private float _cachedMpRegen = 7.5f;

        public void InvalidateStatCache() => _statCacheDirty = true;

        public void BeginAoEDamageBatch()
        {
            _aoeBatchDepth++;
        }

        public void EndAoEDamageBatch()
        {
            if (_aoeBatchDepth <= 0) return;
            _aoeBatchDepth--;
            if (_aoeBatchDepth > 0) return;

            bool resetArena = _batchPendingArenaReset;
            bool stats = _batchStatsDirty;
            bool stage = _batchStageDirty;
            bool quest = _batchQuestDirty;
            _batchPendingArenaReset = false;
            _batchStatsDirty = false;
            _batchStageDirty = false;
            _batchQuestDirty = false;

            bool checkpoint = _batchPendingCheckpoint;
            _batchPendingCheckpoint = false;
            if (checkpoint) SaveCombatCheckpoint();
            if (resetArena) SyncArenaOrReturnToCampaignScene();
            if (stats) OnStatsChanged?.Invoke();
            if (stage) OnStageChanged?.Invoke();
            if (quest) OnQuestOrTutorialChanged?.Invoke();
        }

        private void EmitKillEvents(bool stats, bool stage, bool quest)
        {
            if (_aoeBatchDepth > 0)
            {
                if (stats) _batchStatsDirty = true;
                if (stage) _batchStageDirty = true;
                if (quest) _batchQuestDirty = true;
                return;
            }
            if (stats) OnStatsChanged?.Invoke();
            if (stage) OnStageChanged?.Invoke();
            if (quest) OnQuestOrTutorialChanged?.Invoke();
        }

        public double PlayerCurrentHp { get; private set; }
        public double PlayerMaxHp { get; private set; }
        public float PlayerCurrentMp { get; private set; }
        public float PlayerMaxMp { get; private set; } = 120f;
        public float LoiPhatCooldownRemaining => _skillCooldowns[0];
        public float LoiPhatCooldownTotal => SkillBaseCooldowns[0];
        public bool IsBreakthroughReady =>
            CultivationTable.RequiresBreakthroughAt(SaveService.Current.subStage) &&
            SaveService.Current.tuVi >= GetRequiredTuViForCurrent() &&
            !IsInTribulation && !IsRespawning;

        // Player Death Respawn Countdown (5.0s: zero monster spawns, zero attacks, stays on same stage/Boss)
        public bool IsRespawning { get; private set; }
        public float RespawnTimeRemaining { get; private set; }
        public const float RespawnDuration = 5.0f;
        public string RespawnReasonText { get; private set; }
        public float RespawnProgress01 => IsRespawning ? Mathf.Clamp01(1f - RespawnTimeRemaining / RespawnDuration) : 0f;

        // Alchemy Brew Countdown (3.5s)
        public bool IsBrewingPill { get; private set; }
        public int BrewingTargetRealm { get; private set; }
        public float BrewTimeRemaining { get; private set; }
        public const float BrewDuration = 3.5f;
        public float BrewProgress01 => IsBrewingPill ? Mathf.Clamp01(1f - BrewTimeRemaining / BrewDuration) : 0f;

        // 15-Second Breakthrough Seclusion & Heavenly Tribulation (9 Lightning Strikes)
        public bool IsInTribulation { get; private set; }
        public float TribulationTimeRemaining { get; private set; }
        public const float TribulationDuration = 15.0f;
        public int TribulationStrikesDone { get; private set; }
        public const int TribulationStrikesTotal = 9;
        private const float TribulationStrikeInterval = (TribulationDuration - 2f) / TribulationStrikesTotal;
        private static readonly float[] TribulationHpRatios = { 0.88f, 0.76f, 0.63f, 0.50f, 0.37f, 0.25f, 0.15f, 0.085f, 0.04f };
        public float TribulationTargetChance { get; private set; }
        public string TribulationTargetLabel { get; private set; }
        private RemakePillQuality? _tribulationSelectedPill;
        private bool _tribulationPillConsumed;
        private bool? _tribulationPreRolledOutcome;
        private float _tribulationNextStrikeTime;

        public bool HasActiveBoss { get; private set; }
        public string ActiveBossName { get; private set; }
        public double ActiveBossHp { get; private set; }
        public double ActiveBossMaxHp { get; private set; }
        public float ActiveBossTimeRemaining { get; private set; }
        public const float BossTimeLimit = 60.0f;
        public const float DemonTowerTimeLimit = 55f;

        // V3 Tactical Boss X-10 Shield-Break State (Pha Ho The)
        public bool BossShieldActive { get; private set; }
        public double BossShieldHp { get; private set; }
        public double BossShieldMaxHp { get; private set; }
        public float BossShieldTimeRemaining { get; private set; }
        public float BossBrokenRemaining { get; private set; }
        public bool IsBossStunned => BossBrokenRemaining > 1.5f;
        private float _bossShieldCooldown = 11f;

        // V2 Arena Modes (Campaign / Demon Tower 100F / Secret Realm)
        public RemakeArenaMode ActiveArenaMode { get; private set; } = RemakeArenaMode.Campaign;
        public int ActiveChallengeFloor { get; private set; } = 1;
        public RemakeSecretRealmKind ActiveSecretRealmKind { get; private set; }
        public int ActiveSecretRealmTier { get; private set; } = 1;
        public float ChallengeModeTimeRemaining { get; private set; }
        public int ChallengeKillsRemaining { get; private set; }
        public int CurrentWaveIndex { get; private set; } = 1;
        public int TotalWavesInStage { get; private set; } = 3;
        public int WaveEnemiesRemaining { get; private set; }

        public void ReportWaveState(int waveIndex, int totalWaves, int enemiesRemaining, string bannerText = null)
        {
            CurrentWaveIndex = Mathf.Max(1, waveIndex);
            TotalWavesInStage = Mathf.Max(1, totalWaves);
            WaveEnemiesRemaining = Mathf.Max(0, enemiesRemaining);
            EmitKillEvents(false, true, false);
            if (!string.IsNullOrEmpty(bannerText))
                OnWaveStarted?.Invoke(CurrentWaveIndex, TotalWavesInStage, bannerText);
        }

        public static void GetWavePlan(RemakeArenaMode mode, bool isBossStage, int towerFloor, out int totalWaves, out int[] waveCounts, out bool finalWaveIsBoss)
        {
            if (mode == RemakeArenaMode.DemonTower)
            {
                bool towerBoss = (towerFloor % 5 == 0);
                totalWaves = 2;
                waveCounts = towerBoss ? new[] { 3, 1 } : new[] { 4, 5 };
                finalWaveIsBoss = towerBoss;
                return;
            }
            if (mode == RemakeArenaMode.SecretRealm)
            {
                totalWaves = 3;
                waveCounts = new[] { 4, 5, 4 };
                finalWaveIsBoss = false;
                return;
            }
            if (isBossStage)
            {
                totalWaves = 2;
                waveCounts = new[] { 3, 1 };
                finalWaveIsBoss = true;
                return;
            }
            totalWaves = 3;
            waveCounts = new[] { 3, 3, 4 }; // 3 + 3 + 4 = 10 kills/stage (matches 30-day economy!)
            finalWaveIsBoss = false;
        }

        private void SyncArenaOrReturnToCampaignScene()
        {
            if (ActiveArenaMode == RemakeArenaMode.Campaign &&
                SceneFlowController.Instance != null &&
                SceneFlowController.Instance.IsInGame)
            {
                string expected = SceneFlowController.GetWorldSceneForAct(SaveService.Current.activeAct);
                if (SceneFlowController.Instance.ActiveWorldSceneName != expected)
                {
                    SceneFlowController.Instance.ReturnToCampaignWorld();
                    return;
                }
            }
            RemakeArenaController.Instance?.ResetForStageChange();
        }

        // V2 Active Engagement: Ky Ngo Linh Co (25s floating orb every ~160s)
        public bool HasKyNgoAvailable { get; private set; }
        public float KyNgoTimeRemaining { get; private set; }
        public bool IsTuViDoubleBuffActive => SaveService.Current.tuViDoubleBuffEndUtcTicks > DateTime.UtcNow.Ticks;
        public float TuViDoubleBuffRemainingSec => (float)Math.Max(0, (SaveService.Current.tuViDoubleBuffEndUtcTicks - DateTime.UtcNow.Ticks) / 10_000_000.0);

        private static readonly float[] SkillBaseCooldowns = { 15.0f, 24.0f, 34.0f, 45.0f };
        private static readonly float[] SkillMpCosts = { 30f, 40f, 55f, 65f };
        private readonly float[] _skillCooldowns = new float[4];

        private double _killsWindowCount;
        private float _killsWindowTimer;
        private float _autosaveTimer;
        private bool _combatProgressDirty;
        private float _regenTimer;
        private float _kyNgoSpawnTimer = 95f;
        private float _realmLockToastCooldown;
        private bool _gateCapToastSentForStage;

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
            if (stageDatabase == null)
                stageDatabase = Resources.Load<ActStageDatabaseSO>("ActStageDatabase");
            _kyNgoSpawnTimer = SaveService.IsTestAllProfileActive ? 6f : 95f;
            RecomputePlayerVitals(true);
            RestorePendingActions();
        }

        public void ReloadFromActiveProfile()
        {
            ActiveArenaMode = RemakeArenaMode.Campaign;
            HasKyNgoAvailable = false;
            KyNgoTimeRemaining = 0f;
            _autosaveTimer = 0f;
            _combatProgressDirty = false;
            IsRespawning = false;
            RespawnTimeRemaining = 0f;
            RespawnReasonText = null;
            _realmLockToastCooldown = 0f;
            _gateCapToastSentForStage = false;
            _kyNgoSpawnTimer = SaveService.IsTestAllProfileActive ? 6f : 95f;
            Array.Clear(_skillCooldowns, 0, _skillCooldowns.Length);
            SetActiveBoss(false, "", 0, 0);
            RecomputePlayerVitals(true);
            RestorePendingActions();
            OnStatsChanged?.Invoke();
            OnStageChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
        }

        private void RestorePendingActions()
        {
            var save = SaveService.Current;
            BrewTimeRemaining = Mathf.Clamp(save.pendingBrewSeconds, 0f, BrewDuration);
            IsBrewingPill = BrewTimeRemaining > 0f;
            BrewingTargetRealm = Mathf.Clamp(save.pendingBrewTargetRealm, 0, 7);

            TribulationTimeRemaining = Mathf.Clamp(save.pendingTribulationSeconds, 0f, TribulationDuration);
            IsInTribulation = TribulationTimeRemaining > 0f;
            TribulationStrikesDone = 0;
            _tribulationSelectedPill = null;
            _tribulationPillConsumed = false;
            _tribulationPreRolledOutcome = null;
            _tribulationNextStrikeTime = TribulationDuration - 0.5f;
            if (!IsInTribulation) return;

            int pill = save.pendingTribulationPillQuality;
            if (pill >= (int)RemakePillQuality.HaPham && pill <= (int)RemakePillQuality.CucPham)
            {
                _tribulationSelectedPill = (RemakePillQuality)pill;
                _tribulationPillConsumed = true;
            }

            TribulationTargetChance = Mathf.Clamp01(save.pendingTribulationChance);
            TribulationTargetLabel = GetTribulationTargetLabel(save);
            _tribulationPreRolledOutcome = save.pendingTribulationSuccess;
            while (TribulationStrikesDone < TribulationStrikesTotal && TribulationTimeRemaining <= _tribulationNextStrikeTime)
            {
                TribulationStrikesDone++;
                _tribulationNextStrikeTime -= TribulationStrikeInterval;
            }
            if (TribulationStrikesDone > 0)
                ApplyTribulationStrikeHpDrain(TribulationStrikesDone, TribulationStrikesTotal);
        }

        public void NotifyArenaReady()
        {
            if (IsInTribulation) OnTribulationStateChanged?.Invoke(true);
        }

        private void SaveCombatCheckpoint()
        {
            if (_aoeBatchDepth > 0)
            {
                _batchPendingCheckpoint = true;
                return;
            }
            _autosaveTimer = 0f;
            _combatProgressDirty = false;
            BackgroundCatchupService.MarkActiveTimestamp();
            SaveService.Save();
        }

        private void Update()
        {
            var save = SaveService.Current;
            if (!save.hasCreatedCharacter) return;
            if (SceneFlowController.Instance == null || !SceneFlowController.Instance.IsInGame) return;

            float dt = Time.deltaTime;
            if (_realmLockToastCooldown > 0f) _realmLockToastCooldown = Mathf.Max(0f, _realmLockToastCooldown - dt);
            EnsureStatCache();
            for (int i = 0; i < 4; i++)
            {
                if (_skillCooldowns[i] > 0f)
                    _skillCooldowns[i] = Mathf.Max(0f, _skillCooldowns[i] - dt * _cachedCdRate);
            }

            // Alchemy Brew 3.5s Countdown (runs even while respawning / seclusion / modal closed)
            if (IsBrewingPill)
                AdvanceBrewTimer(dt);

            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= 15f || (_combatProgressDirty && _autosaveTimer >= 3f))
                SaveCombatCheckpoint();

            // Player Respawn Countdown: zero monster spawns, zero attacks, zero skills until revived!
            if (IsRespawning)
            {
                AdvanceRespawnTimer(dt);
                return;
            }

            // 15-Second Breakthrough Seclusion & Tribulation Lightning Strikes
            if (IsInTribulation)
            {
                AdvanceTribulation(dt);
                return;
            }

            PlayerCurrentMp = Mathf.Min(PlayerMaxMp, PlayerCurrentMp + _cachedMpRegen * dt);

            _regenTimer += dt;
            if (_regenTimer >= 1.0f)
            {
                _regenTimer = 0f;
                double regen = PlayerMaxHp * (0.015 + GetDaoVanLevel("branch_body") * 0.015 + GetDaoVanLevel("body_leech") * 0.008);
                PlayerCurrentHp = Math.Min(PlayerMaxHp, PlayerCurrentHp + regen);
            }

            _killsWindowTimer += dt;
            if (_killsWindowTimer >= 10f)
            {
                if (_killsWindowCount > 0)
                {
                    double rate = _killsWindowCount / _killsWindowTimer;
                    save.measuredKillsPerSec = save.measuredKillsPerSec * 0.5 + rate * 0.5;
                }
                _killsWindowTimer = 0f;
                _killsWindowCount = 0;
            }

            if (RemakeArenaController.Instance != null && RemakeArenaController.Instance.HasAliveTarget())
            {
                for (int slot = 0; slot < 4; slot++)
                {
                    if (GetSkillAuto(slot) && IsSkillUnlocked(slot) &&
                        _skillCooldowns[slot] <= 0f && PlayerCurrentMp >= SkillMpCosts[slot])
                    {
                        TryCastSkill(slot);
                        break; // Space out automatic casts cleanly 1 frame apart
                    }
                }
            }

            // Boss 60s timeout & V3 Tactical Boss Shield-Break cycle
            if (ActiveArenaMode == RemakeArenaMode.Campaign && HasActiveBoss)
            {
                ActiveBossTimeRemaining -= dt;
                if (ActiveBossTimeRemaining <= 0f)
                {
                    ResetBossShieldState();
                    BeginPlayerRespawn("Hết 60 giây khiêu chiến Thủ Lĩnh! Hồi sinh để tái chiến Thủ Lĩnh.");
                }
                else
                {
                    AdvanceBossShieldCycle(dt);
                }
            }
            else if (ActiveArenaMode != RemakeArenaMode.Campaign)
            {
                ChallengeModeTimeRemaining -= dt;
                if (ChallengeModeTimeRemaining <= 0f)
                {
                    ActiveArenaMode = RemakeArenaMode.Campaign;
                    SetActiveBoss(false, "", 0, 0);
                    PlayerCurrentHp = PlayerMaxHp;
                    SyncArenaOrReturnToCampaignScene();
                    OnStageChanged?.Invoke();
                    OnToastMessage?.Invoke("Hết giờ thử thách! Quay về Đấu Trường chính.");
                }
            }

            // V2 Ky Ngo Linh Co (25s floating event; WhiteFox companion auto-collects!)
            if (ActiveArenaMode == RemakeArenaMode.Campaign && save.totalKills >= 5)
            {
                if (HasKyNgoAvailable)
                {
                    KyNgoTimeRemaining -= dt;
                    if ((RemakeCompanionId)save.activeCompanionId == RemakeCompanionId.WhiteFox && KyNgoTimeRemaining <= 18f)
                    {
                        TryClaimKyNgoChoice(0);
                    }
                    else if (KyNgoTimeRemaining <= 0f)
                    {
                        HasKyNgoAvailable = false;
                        OnQuestOrTutorialChanged?.Invoke();
                    }
                }
                else
                {
                    _kyNgoSpawnTimer -= dt;
                    if (_kyNgoSpawnTimer <= 0f)
                    {
                        _kyNgoSpawnTimer = UnityEngine.Random.Range(145f, 210f);
                        HasKyNgoAvailable = true;
                        KyNgoTimeRemaining = 25f;
                        OnQuestOrTutorialChanged?.Invoke();
                    }
                }
            }
        }

        public ActStageDatabaseSO.StageSpec GetCurrentStageSpec()
        {
            var save = SaveService.Current;
            if (stageDatabase != null)
                return stageDatabase.GetStage((RemakeDifficulty)save.activeDifficulty, save.activeAct, save.activeStage);
            return ActStageDatabaseSO.BuildFallbackSpec((RemakeDifficulty)save.activeDifficulty, save.activeAct, save.activeStage);
        }

        public ActStageDatabaseSO.StageSpec GetStageSpec(RemakeDifficulty diff, int act, int stage)
        {
            if (stageDatabase != null)
                return stageDatabase.GetStage(diff, act, stage);
            return ActStageDatabaseSO.BuildFallbackSpec(diff, act, stage);
        }

        public double GetRequiredTuViForCurrent()
        {
            var save = SaveService.Current;
            return CultivationTable.GetRequiredTuVi(save.realmIndex, save.subStage);
        }

        public void CreateOrSelectCharacter(string rawName) =>
            CreateOrSelectCharacter(rawName, SaveService.Current != null ? SaveService.Current.gender : RemakeCharacterGender.Female);

        public void CreateOrSelectCharacter(string rawName, RemakeCharacterGender gender)
        {
            var save = SaveService.Current;
            string clean = string.IsNullOrWhiteSpace(rawName) ? "Thanh Vân Tử" : rawName.Trim();
            if (clean.Length > 20) clean = clean.Substring(0, 20);
            save.daoName = clean;
            save.gender = gender;
            save.hasCreatedCharacter = true;
            save.lastTickUtcTicks = DateTime.UtcNow.Ticks;
            RecomputePlayerVitals(true);
            SaveService.Save();
            RemakeOnlineService.Instance?.ConnectIfNeeded();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
        }

        // =========================================================
        // MULTIPLIERS: DAO VAN + GEAR + SETS + FORMATIONS + SWORD STYLE
        // =========================================================
        public int GetDaoVanLevel(string nodeId)
        {
            var nodes = SaveService.Current.daoVanNodes;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].nodeId == nodeId) return nodes[i].level;
            return 0;
        }

        public double GetDaoVanUpgradeCost(DaoVanNodeSpec node, int currentLevel)
        {
            if (node == null) return 999999;
            if (node.costsDaoPoints)
                return Math.Ceiling(node.baseCost * Math.Pow(node.costGrowth, currentLevel));
            return Math.Round(node.baseCost * Math.Pow(node.costGrowth, currentLevel));
        }

        public bool CanUnlockOrUpgradeNode(DaoVanNodeSpec node)
        {
            if (node == null) return false;
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage))
                return false;
            if (!string.IsNullOrEmpty(node.parentId) && GetDaoVanLevel(node.parentId) <= 0)
                return false;
            int lv = GetDaoVanLevel(node.id);
            if (lv >= node.maxLevel) return false;

            double cost = GetDaoVanUpgradeCost(node, lv);
            return node.costsDaoPoints ? save.daoPoints >= cost : save.linhThach >= cost;
        }

        public bool TryUpgradeDaoVanNode(string nodeId)
        {
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.DaoVanTree, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Cây Đạo Vận mở khóa tại Luyện Khí Tầng 4!");
                return false;
            }
            var spec = FindNodeSpec(nodeId);
            if (spec == null) return false;
            if (!string.IsNullOrEmpty(spec.parentId) && GetDaoVanLevel(spec.parentId) <= 0)
            {
                OnToastMessage?.Invoke("Cần mở node gốc trước!");
                return false;
            }
            int lv = GetDaoVanLevel(nodeId);
            if (lv >= spec.maxLevel)
            {
                OnToastMessage?.Invoke($"{spec.title} đã đạt cấp tối đa!");
                return false;
            }
            double cost = GetDaoVanUpgradeCost(spec, lv);
            if (spec.costsDaoPoints)
            {
                if (save.daoPoints < cost) { OnToastMessage?.Invoke("Không đủ Điểm Đạo Vận!"); return false; }
                save.daoPoints -= (int)cost;
            }
            else
            {
                if (save.linhThach < cost) { OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(cost)} Linh Thạch!"); return false; }
                save.linhThach -= cost;
            }

            SetDaoVanLevel(nodeId, lv + 1);
            RecomputePlayerVitals(false);
            CheckDaoVanAndSkillQuest();
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            OnToastMessage?.Invoke($"Nâng {spec.title} lên Cấp {lv + 1}/{spec.maxLevel}!");
            return true;
        }

        public float GetEquipmentStatSum(RemakeEquipStatType stat)
        {
            float sum = 0f;
            var save = SaveService.Current;
            for (int i = 0; i < save.equippedGear.Count; i++)
            {
                var item = save.equippedGear[i];
                if (item == null || item.substats == null) continue;
                int slotEnhance = save.equipSlotEnhanceLevels != null && (int)item.slot < save.equipSlotEnhanceLevels.Length
                    ? save.equipSlotEnhanceLevels[(int)item.slot] : 0;
                float enhMult = 1f + slotEnhance * 0.08f;
                for (int j = 0; j < item.substats.Count; j++)
                    if (item.substats[j].statType == stat) sum += item.substats[j].value * enhMult;
            }
            return sum;
        }

        public int GetEquipSetCount(RemakeEquipSet setId)
        {
            if (setId == RemakeEquipSet.None) return 0;
            int count = 0;
            var gear = SaveService.Current.equippedGear;
            for (int i = 0; i < gear.Count; i++)
                if (gear[i] != null && gear[i].setId == setId) count++;
            return count;
        }

        private float GetFormationBonus(Func<FormationSpec, float> selector)
        {
            float total = 0f;
            var save = SaveService.Current;
            if (save.activeMainFormation >= 0 && save.activeMainFormation < RemakeCatalogs.Formations.Count &&
                CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Formation1, save.realmIndex, save.subStage))
                total += selector(RemakeCatalogs.Formations[save.activeMainFormation]);

            if (save.activeSubFormation >= 0 && save.activeSubFormation < RemakeCatalogs.Formations.Count &&
                save.activeSubFormation != save.activeMainFormation &&
                CultivationTable.IsFeatureUnlocked(RemakeFeatureId.FormationSubSlot, save.realmIndex, save.subStage))
                total += selector(RemakeCatalogs.Formations[save.activeSubFormation]) * 0.50f;

            float formAmp = 1f + GetDaoVanLevel("arts_formation") * 0.12f;
            return total * formAmp;
        }

        private float GetFormationTuViPct() => GetFormationBonus(f => f.tuViPct);

        private void EnsureStatCache()
        {
            if (!_statCacheDirty) return;
            _statCacheDirty = false;
            var save = SaveService.Current;

            int coreLv = GetDaoVanLevel("core_daotam");
            int branchSwordLv = GetDaoVanLevel("branch_sword");
            int swordCritDmgLv = GetDaoVanLevel("sword_crit_dmg");
            int swordArmorPenLv = GetDaoVanLevel("sword_armor_pierce");
            int swordMasteryLv = GetDaoVanLevel("sword_mastery");
            int bodyArmorLv = GetDaoVanLevel("body_armor");
            int bodyLeechLv = GetDaoVanLevel("body_leech");
            int branchCultLv = GetDaoVanLevel("branch_cultivation");
            int cultEnlightLv = GetDaoVanLevel("cult_enlightenment");
            int branchWealthLv = GetDaoVanLevel("branch_wealth");
            int wealthOreLv = GetDaoVanLevel("wealth_ore");
            int wealthFortuneLv = GetDaoVanLevel("wealth_fortune");
            int branchArtsLv = GetDaoVanLevel("branch_arts");

            int thanhVan = GetEquipSetCount(RemakeEquipSet.ThanhVan);
            int huyenVu = GetEquipSetCount(RemakeEquipSet.HuyenVu);
            int danTong = GetEquipSetCount(RemakeEquipSet.DanTong);
            var companion = (RemakeCompanionId)save.activeCompanionId;
            int codexEntries = GetCodexUnlockedCount(out _, out _, out _);
            float codexBonus = codexEntries * 0.008f; // +0.8% Sword Dmg & Tu Vi per unlocked Codex entry

            // Sword Damage Mult
            float daoSwordMult = 1f + coreLv * 0.10f + branchSwordLv * 0.15f + bodyLeechLv * 0.10f + swordMasteryLv * 0.28f;
            float eqSwordPct = GetEquipmentStatSum(RemakeEquipStatType.SwordDamagePct) / 100f;
            if (thanhVan >= 2) eqSwordPct += 0.15f;
            if (thanhVan >= 4) eqSwordPct += 0.30f;
            if (thanhVan >= 6) eqSwordPct += 0.60f;
            float formSwordPct = GetFormationBonus(f => f.swordDamagePct) / 100f;
            float compSwordMult = companion == RemakeCompanionId.ThunderSpirit ? 1.18f : 1.0f;
            _cachedSwordDmgMult = daoSwordMult * (1f + eqSwordPct + formSwordPct + codexBonus) * compSwordMult;

            // Sword Speed Mult
            float spd = 1f + coreLv * 0.05f;
            if ((RemakeSwordStyle)save.swordStyle == RemakeSwordStyle.KiemXuyen) spd += 0.28f;
            if (companion == RemakeCompanionId.AzureCrane) spd += 0.12f;
            spd += GetFormationBonus(f => f.swordSpeedPct) / 100f;
            _cachedSwordSpeedMult = spd;

            // Armor Pen & Boss Mult
            float pen = swordArmorPenLv * 0.06f;
            if ((RemakeSwordStyle)save.swordStyle == RemakeSwordStyle.PhaGiap) pen += 0.60f;
            _cachedArmorPen = Mathf.Clamp01(pen);

            float bossB = 1f + swordArmorPenLv * 0.08f;
            if ((RemakeSwordStyle)save.swordStyle == RemakeSwordStyle.PhaGiap) bossB += 0.35f;
            _cachedBossMult = bossB;

            // Damage Reduction
            float dr = bodyArmorLv * 0.03f;
            if (huyenVu >= 2) dr += 0.06f;
            if (huyenVu >= 4) dr += 0.12f;
            if (huyenVu >= 6) dr += 0.20f;
            if (companion == RemakeCompanionId.JadeTurtle) dr += 0.08f;
            dr += GetFormationBonus(f => f.damageReductionPct) / 100f;
            _cachedDamageReduction = Mathf.Clamp(dr, 0f, 0.72f);

            // Tu Vi & Linh Thach Mults
            float daoTuVi = 1f + coreLv * 0.12f + branchCultLv * 0.15f + cultEnlightLv * 0.25f;
            if (companion == RemakeCompanionId.AzureCrane) daoTuVi += 0.10f;
            float eqTuVi = GetEquipmentStatSum(RemakeEquipStatType.TuViGainPct) / 100f;
            if (danTong >= 2) eqTuVi += 0.15f;
            if (danTong >= 4) eqTuVi += 0.30f;
            if (danTong >= 6) eqTuVi += 0.55f;
            float formTuVi = GetFormationBonus(f => f.tuViPct) / 100f;
            _cachedTuViMultNoKyNgo = daoTuVi * (1f + eqTuVi + formTuVi + codexBonus);

            float daoLt = 1f + branchWealthLv * 0.15f + wealthFortuneLv * 0.30f;
            if (companion == RemakeCompanionId.WhiteFox) daoLt += 0.15f;
            float eqLt = GetEquipmentStatSum(RemakeEquipStatType.LinhThachPct) / 100f;
            _cachedLinhThachMult = daoLt * (1f + eqLt);

            // Crit
            float crit = 0.08f + branchSwordLv * 0.02f;
            if ((RemakeSwordStyle)save.swordStyle == RemakeSwordStyle.KiemXuyen) crit += 0.08f;
            crit += GetEquipmentStatSum(RemakeEquipStatType.CritChancePct) / 100f;
            crit += GetFormationBonus(f => f.critChancePct) / 100f;
            _cachedCritChance = Mathf.Clamp(crit, 0.05f, 0.85f);
            _cachedCritDmgMult = 2.0 + swordCritDmgLv * 0.18;

            // Total Sword Attack Power
            int maxSlots = CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex);
            double total = 0;
            for (int i = 0; i < save.equippedSwordLevels.Count && i < maxSlots; i++)
                total += CultivationTable.CalculateSwordDamage(save.equippedSwordLevels[i]);
            total += GetEquipmentStatSum(RemakeEquipStatType.FlatSwordAtk);
            _cachedTotalSwordAtk = total <= 0 ? 0.0 : Math.Max(1.0, Math.Round(total * _cachedSwordDmgMult));

            // Drop, Leech, CD & MP Regen multipliers
            _cachedBodyLeechPct = bodyLeechLv * 0.008f;
            _cachedHerbMult = 1f + branchArtsLv * 0.15f;
            _cachedOreMult = 1f + wealthOreLv * 0.20f;
            _cachedGearMult = 1f + wealthOreLv * 0.12f;
            _cachedSwordDropMult = 1f + branchWealthLv * 0.08f;
            float cdRed = Mathf.Clamp(cultEnlightLv * 0.04f, 0f, 0.35f);
            _cachedCdRate = 1f / (1f - cdRed);
            _cachedMpRegen = 7.5f + formTuVi * 10f;
        }

        public float GetSwordDamageMultiplier() { EnsureStatCache(); return _cachedSwordDmgMult; }
        public float GetSwordSpeedMultiplier() { EnsureStatCache(); return _cachedSwordSpeedMult; }
        public float GetArmorPenetration() { EnsureStatCache(); return _cachedArmorPen; }
        public float GetBossBonusMultiplier() { EnsureStatCache(); return _cachedBossMult; }
        public float GetDamageReduction() { EnsureStatCache(); return _cachedDamageReduction; }
        public float GetTuViMultiplier() { EnsureStatCache(); return _cachedTuViMultNoKyNgo * (IsTuViDoubleBuffActive ? 2.0f : 1.0f); }
        public float GetLinhThachMultiplier() { EnsureStatCache(); return _cachedLinhThachMult; }
        public float GetCritChance() { EnsureStatCache(); return _cachedCritChance; }
        public double GetCritDamageMultiplier() { EnsureStatCache(); return _cachedCritDmgMult; }
        public double GetTotalSwordAttackPower() { EnsureStatCache(); return _cachedTotalSwordAtk; }

        // =========================================================
        // COMBAT KILLS, LOOT DROPS & 120-STAGE PROGRESSION
        // =========================================================
        public bool IsStageRealmUnlocked(ActStageDatabaseSO.StageSpec spec)
        {
            if (spec == null) return false;
            var save = SaveService.Current;
            return CultivationTable.IsStageRealmMet(spec.requiredRealmIndex, spec.requiredSubStage, save.realmIndex, save.subStage);
        }

        public bool IsStageFullyUnlocked(ActStageDatabaseSO.StageSpec spec)
        {
            if (spec == null) return false;
            return spec.LinearIndex <= SaveService.Current.highestUnlockedLinearStage && IsStageRealmUnlocked(spec);
        }

        private bool AddTuViWithAutoLevel(double gain)
        {
            if (gain <= 0) return false;
            var save = SaveService.Current;
            save.tuVi += gain;
            bool leveledUp = false;

            while (save.subStage < 10 && !CultivationTable.RequiresBreakthroughAt(save.subStage))
            {
                double req = GetRequiredTuViForCurrent();
                if (save.tuVi < req) break;
                save.tuVi -= req;
                save.subStage++;
                _gateCapToastSentForStage = false;
                leveledUp = true;

                if (save.mainQuestIndex == 2) save.mainQuestProgress = save.subStage;
                RecomputePlayerVitals(true);
                RemakeAudioService.Instance?.PlayMerge();
                RemakeVfxPool.Instance?.SpawnBreakthroughFlash(
                    RemakeArenaController.Instance != null ? RemakeArenaController.Instance.PlayerWorldPosition : Vector3.zero, true);
                OnToastMessage?.Invoke($"Tu vi tinh tiến! Tự động lên {CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)}!");
            }

            double cap = GetRequiredTuViForCurrent();
            if (save.tuVi >= cap)
            {
                save.tuVi = cap;
                if (CultivationTable.RequiresBreakthroughAt(save.subStage) && !_gateCapToastSentForStage)
                {
                    _gateCapToastSentForStage = true;
                    OnToastMessage?.Invoke($"Tu Vi viên mãn! Đã chạm bình cảnh {CultivationTable.GetBreakthroughGateTitle(save.realmIndex, save.subStage)} — bấm Đột Phá!");
                }
            }
            return leveledUp;
        }

        public void OnMonsterKilled(MonsterDefinitionSO monster, bool wasBoss) => OnMonsterKilled(monster, wasBoss, null);

        public void OnMonsterKilled(MonsterDefinitionSO monster, bool wasBoss, Vector3? deathPos)
        {
            var save = SaveService.Current;
            _killsWindowCount++;
            save.totalKills++;
            _combatProgressDirty = true;

            if (ActiveArenaMode != RemakeArenaMode.Campaign)
            {
                HandleChallengeModeKill(deathPos);
                return;
            }

            EnsureStatCache();
            var stage = GetCurrentStageSpec();
            double tuViGain = Math.Round(stage.tuViPerKill * (monster != null ? monster.tuViMultiplier : 1f) * GetTuViMultiplier());
            double linhThachGain = Math.Round(stage.linhThachPerKill * (monster != null ? monster.linhThachMultiplier : 1f) * GetLinhThachMultiplier());

            AddTuViWithAutoLevel(tuViGain);
            save.linhThach += linhThachGain;

            // Lifesteal sustain on hit/kill
            double heal = PlayerMaxHp * _cachedBodyLeechPct;
            if (heal > 0) PlayerCurrentHp = Math.Min(PlayerMaxHp, PlayerCurrentHp + heal);

            // Herb drops
            bool stageCleared = false;
            bool droppedHerb = false;
            float herbChance = stage.herbDropRate * _cachedHerbMult;
            if (UnityEngine.Random.value <= herbChance)
            {
                int tierIdx = Mathf.Clamp(stage.herbTier - 1, 0, save.herbsByTier.Length - 1);
                save.herbsByTier[tierIdx]++;
                droppedHerb = true;
            }

            // Ore drops (when Equipment unlocked at Truc Co+)
            bool equipUnlocked = CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, save.realmIndex, save.subStage);
            if (equipUnlocked && (wasBoss || UnityEngine.Random.value <= 0.25f))
            {
                int oreQty = Mathf.RoundToInt((wasBoss ? 4 : 1) * _cachedOreMult);
                save.ore += Math.Max(1, oreQty);
            }

            // Equipment Loot drop (6 slots, auto-dismantle filter)
            RemakeRarity? droppedEquipRarity = null;
            if (equipUnlocked)
            {
                float gearChance = (wasBoss ? 1.0f : 0.04f) * _cachedGearMult;
                if (UnityEngine.Random.value <= gearChance)
                    droppedEquipRarity = RollAndGrantEquipment(stage, wasBoss);
            }

            // Sword embryo drops (1.5%-3% normal, 100% Boss, tutorial #5 guarantee — single clean drop!)
            bool dropSword = false;
            int dropLevel = 1;
            if (!save.tutorialGuaranteedSwordDropped && save.totalKills >= 5)
            {
                save.tutorialGuaranteedSwordDropped = true;
                dropSword = true;
                dropLevel = 1;
            }
            else if (wasBoss)
            {
                dropSword = true;
                dropLevel = UnityEngine.Random.Range(stage.swordDropMinLevel, stage.swordDropMaxLevel + 1);
                if (UnityEngine.Random.value <= 0.45f) save.skillScrollCount++;
                else save.reforgeStoneCount++;
            }
            else
            {
                float chance = stage.swordDropRate * _cachedSwordDropMult;
                if (UnityEngine.Random.value <= chance)
                {
                    dropSword = true;
                    dropLevel = stage.swordDropMinLevel;
                }
            }

            if (dropSword)
            {
                save.swordInventoryLevels.Add(dropLevel);
                save.codexSwordsMask |= (1 << Mathf.Clamp(dropLevel - 1, 0, 9));
                OnToastMessage?.Invoke($"Nhặt Phôi Kiếm Cấp {dropLevel}!");
            }

            if (deathPos.HasValue)
                RemakeLootOrbPool.Instance?.SpawnMonsterDrops(deathPos.Value, wasBoss, droppedHerb, dropSword, droppedEquipRarity);

            // Stage clear & 120-stage progression:
            // - X-1..X-9 clear without dying -> auto-advance to X+1 if Realm gate met!
            // - Boss X-10 win -> unlock next linear stage, stay on Boss X-10 & repeat Boss continuously!
            if (wasBoss)
            {
                if (stage.actIndex == 0 && stage.stageNumber == 10) save.bossAct1Cleared = true;
                save.codexBossesMask |= (1 << Mathf.Clamp(stage.actIndex, 0, 2));

                int nextLinear = Mathf.Min(119, stage.LinearIndex + 1);
                if (nextLinear > save.highestUnlockedLinearStage)
                {
                    save.highestUnlockedLinearStage = nextLinear;
                    var nextSpec = GetStageSpec((RemakeDifficulty)(nextLinear / 30), (nextLinear % 30) / 10, nextLinear % 10 + 1);
                    string reqTxt = IsStageRealmUnlocked(nextSpec)
                        ? "Có thể chuyển Vực mới trong Bản Đồ!"
                        : $"Cần {CultivationTable.GetRealmDisplayName(nextSpec.requiredRealmIndex, nextSpec.requiredSubStage)} để tiến vào.";
                    OnToastMessage?.Invoke($"Hạ Thủ Lĩnh {stage.stageCode}! Mở {nextSpec.stageCode} ({reqTxt}) — Tiếp tục cày Thủ Lĩnh!");
                }
                else
                {
                    OnToastMessage?.Invoke($"Hạ gục Thủ Lĩnh {stage.stageCode}! Chuẩn bị tái chiến Thủ Lĩnh...");
                }

                if (save.mainQuestIndex == 5 && stage.actIndex == 0 && stage.stageNumber == 10) save.mainQuestProgress = 1;
                if (save.mainQuestIndex == 9 && stage.actIndex == 1 && stage.stageNumber == 10) save.mainQuestProgress = 1;
                if (save.mainQuestIndex == 11 && stage.actIndex == 2 && stage.stageNumber == 10) save.mainQuestProgress = 1;
            }
            else if (!stage.isBossStage)
            {
                save.stageWaveKills++;
                if (save.stageWaveKills >= stage.killsToClearWave)
                {
                    save.stageWaveKills = 0;
                    stageCleared = true;
                    int nextLinear = Mathf.Min(119, stage.LinearIndex + 1);
                    if (nextLinear > save.highestUnlockedLinearStage)
                        save.highestUnlockedLinearStage = nextLinear;

                    if (stage.stageNumber < 10)
                    {
                        var nextSpec = GetStageSpec(stage.difficulty, stage.actIndex, stage.stageNumber + 1);
                        bool realmOk = IsStageRealmUnlocked(nextSpec);
                        bool bossAllowed = (stage.stageNumber < 9) || save.autoChallengeBoss;

                        if (realmOk && bossAllowed)
                        {
                            save.activeStage = stage.stageNumber + 1;
                            OnToastMessage?.Invoke(stage.stageNumber == 9
                                ? $"Vượt Ải {stage.stageCode}! Tự động tiến vào Thủ Lĩnh {nextSpec.stageCode}!"
                                : $"Vượt Ải {stage.stageCode}! Tự động tiến sang Ải {nextSpec.stageCode}!");
                            if (_aoeBatchDepth > 0) _batchPendingArenaReset = true;
                            else RemakeArenaController.Instance?.ResetForStageChange();
                        }
                        else if (!realmOk && _realmLockToastCooldown <= 0f)
                        {
                            _realmLockToastCooldown = 40f;
                            OnToastMessage?.Invoke($"Vượt Ải {stage.stageCode}! Cần đạt {CultivationTable.GetRealmDisplayName(nextSpec.requiredRealmIndex, nextSpec.requiredSubStage)} để mở Ải {nextSpec.stageCode}.");
                        }
                    }
                }
            }

            // Quests & Tutorial Beat 1
            if (save.mainQuestIndex == 0)
            {
                save.mainQuestProgress = Mathf.Min(5, save.totalKills);
                if (save.tutorialStep == 0 && save.totalKills >= 5) save.tutorialStep = 1;
            }
            else if (save.mainQuestIndex == 2)
            {
                save.mainQuestProgress = save.subStage;
            }
            else if (save.mainQuestIndex == 7 && save.activeAct >= 1)
            {
                save.mainQuestProgress = 1;
            }

            if (wasBoss) SetActiveBoss(false, "", 0, 0);

            if (wasBoss || stageCleared || dropSword || droppedEquipRarity.HasValue)
                SaveCombatCheckpoint();
            EmitKillEvents(true, true, true);
        }

        public void SetActiveBoss(bool active, string bossName, double hp, double maxHp)
        {
            if (active && !HasActiveBoss)
            {
                ActiveBossTimeRemaining = BossTimeLimit;
                ResetBossShieldState();
                _bossShieldCooldown = 10f;
            }
            else if (!active && HasActiveBoss)
            {
                ResetBossShieldState();
            }
            HasActiveBoss = active;
            ActiveBossName = bossName;
            ActiveBossHp = Math.Max(0, hp);
            ActiveBossMaxHp = Math.Max(1, maxHp);
            OnBossStatusUpdated?.Invoke(active ? bossName : null, ActiveBossHp, ActiveBossMaxHp);
        }

        private void ResetBossShieldState()
        {
            bool changed = BossShieldActive || BossBrokenRemaining > 0f;
            BossShieldActive = false;
            BossShieldHp = 0;
            BossShieldMaxHp = 0;
            BossShieldTimeRemaining = 0f;
            BossBrokenRemaining = 0f;
            _bossShieldCooldown = 12f;
            if (changed) OnBossShieldStateChanged?.Invoke();
        }

        public void AdvanceBossShieldCycle(float dt)
        {
            if (BossBrokenRemaining > 0f)
            {
                BossBrokenRemaining = Mathf.Max(0f, BossBrokenRemaining - dt);
                if (BossBrokenRemaining <= 0f) OnBossShieldStateChanged?.Invoke();
                return;
            }
            if (BossShieldActive)
            {
                BossShieldTimeRemaining -= dt;
                if (BossShieldTimeRemaining <= 0f)
                {
                    BossShieldActive = false;
                    BossShieldHp = 0;
                    _bossShieldCooldown = 13f;
                    // Boss completes charge: heavy slam!
                    var stage = GetCurrentStageSpec();
                    OnPlayerDamaged(stage.enemyDamage * 1.45);
                    OnToastMessage?.Invoke("Thủ Lĩnh bùng phát Yêu Lực! Hãy dùng Kỹ Năng hoặc Phá Giáp khi Khiên xuất hiện!");
                    OnBossShieldStateChanged?.Invoke();
                }
                return;
            }
            _bossShieldCooldown -= dt;
            if (_bossShieldCooldown <= 0f && ActiveBossMaxHp > 0 && ActiveBossHp > ActiveBossMaxHp * 0.12)
            {
                BossShieldActive = true;
                BossShieldMaxHp = Math.Max(20, Math.Round(ActiveBossMaxHp * 0.11));
                BossShieldHp = BossShieldMaxHp;
                BossShieldTimeRemaining = 4.0f;
                OnToastMessage?.Invoke("Thủ Lĩnh mở Khiên Yêu Lực (4s)! Tung Kỹ Năng để Phá Phòng!");
                OnBossShieldStateChanged?.Invoke();
            }
        }

        public void ApplyBossShieldDamage(double rawDamage, bool fromActiveSkill)
        {
            if (!BossShieldActive || BossShieldHp <= 0) return;
            var save = SaveService.Current;
            double mult = fromActiveSkill ? 2.6 : ((RemakeSwordStyle)save.swordStyle == RemakeSwordStyle.PhaGiap ? 1.8 : 1.0);
            if ((RemakeCompanionId)save.activeCompanionId == RemakeCompanionId.ThunderSpirit) mult *= 1.20;
            BossShieldHp = Math.Max(0, BossShieldHp - rawDamage * mult);
            if (BossShieldHp <= 0)
            {
                BossShieldActive = false;
                BossShieldTimeRemaining = 0f;
                BossBrokenRemaining = 5.0f; // 3.5s stun + 5s +35% damage taken!
                _bossShieldCooldown = 15f;
                RemakeAudioService.Instance?.PlayBreakthrough();
                if (RemakeArenaController.Instance != null && RemakeArenaController.Instance.TryFindPriorityTargetPos(out var bPos))
                    RemakeVfxPool.Instance?.SpawnBreakthroughFlash(bPos, true);
                OnToastMessage?.Invoke("PHÁ HỘ THỂ! Thủ Lĩnh choáng 3.5s & nhận +35% sát thương!");
            }
            OnBossShieldStateChanged?.Invoke();
        }

        public void BeginPlayerRespawn(string reasonToast)
        {
            if (IsRespawning) return;
            var save = SaveService.Current;
            save.stageWaveKills = 0; // Restart current stage from Wave 1 (never drop stage!)
            PlayerCurrentHp = 0;
            SetActiveBoss(false, "", 0, 0);
            IsRespawning = true;
            RespawnTimeRemaining = RespawnDuration;
            RespawnReasonText = string.IsNullOrEmpty(reasonToast)
                ? $"Trọng thương tại Ải {GetCurrentStageSpec().stageCode}! Đang hồi sinh..."
                : reasonToast;

            SaveService.Save();
            OnRespawnStateChanged?.Invoke(true);
            OnStageChanged?.Invoke();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke(RespawnReasonText);
        }

        public void AdvanceRespawnTimer(float dt)
        {
            if (!IsRespawning) return;
            RespawnTimeRemaining = Mathf.Max(0f, RespawnTimeRemaining - Mathf.Max(0f, dt));
            if (RespawnTimeRemaining <= 0f)
            {
                IsRespawning = false;
                RespawnTimeRemaining = 0f;
                RespawnReasonText = null;
                PlayerCurrentHp = PlayerMaxHp;
                PlayerCurrentMp = PlayerMaxMp;
                OnRespawnStateChanged?.Invoke(false);
                OnStageChanged?.Invoke();
                OnStatsChanged?.Invoke();
                OnToastMessage?.Invoke($"Đã hồi sinh! Tiếp tục chiến đấu tại Ải {GetCurrentStageSpec().stageCode}.");
            }
        }

        public void OnPlayerDamaged(double dmg)
        {
            if (IsInTribulation || IsRespawning) return;
            double mitigated = Math.Max(1, dmg * (1.0 - GetDamageReduction()));
            PlayerCurrentHp = Math.Max(0, PlayerCurrentHp - mitigated);
            if (PlayerCurrentHp <= 0)
            {
                if (ActiveArenaMode != RemakeArenaMode.Campaign)
                {
                    PlayerCurrentHp = PlayerMaxHp;
                    ActiveArenaMode = RemakeArenaMode.Campaign;
                    SetActiveBoss(false, "", 0, 0);
                    SyncArenaOrReturnToCampaignScene();
                    OnStageChanged?.Invoke();
                    OnToastMessage?.Invoke("Thất bại trong thử thách! Quay về Đấu Trường chính.");
                }
                else if (HasActiveBoss || GetCurrentStageSpec().isBossStage)
                {
                    BeginPlayerRespawn($"Thất thủ trước Thủ Lĩnh {GetCurrentStageSpec().stageCode}! Hồi sinh sau {RespawnDuration:0}s để tái chiến.");
                }
                else
                {
                    BeginPlayerRespawn($"Trọng thương tại Ải {GetCurrentStageSpec().stageCode}! Hồi sinh sau {RespawnDuration:0}s và chạy lại ải này.");
                }
            }
            OnStatsChanged?.Invoke();
        }

        // =========================================================
        // STAGE SELECTION (ENFORCES BOTH STAGE PROGRESSION & REALM)
        // =========================================================
        public bool TrySelectStage(RemakeDifficulty diff, int act, int stageNumber)
        {
            if (IsInTribulation || SceneFlowController.Instance?.IsTransitioning == true) return false;
            var spec = GetStageSpec(diff, act, stageNumber);
            if (spec.LinearIndex > SaveService.Current.highestUnlockedLinearStage)
            {
                OnToastMessage?.Invoke($"Cần vượt qua ải trước để mở {ActStageDatabaseSO.GetDifficultyLabel(diff)} {spec.stageCode}!");
                return false;
            }
            if (!IsStageRealmUnlocked(spec))
            {
                OnToastMessage?.Invoke($"Cần đạt {CultivationTable.GetRealmDisplayName(spec.requiredRealmIndex, spec.requiredSubStage)} để tiến vào {ActStageDatabaseSO.GetDifficultyLabel(diff)} {spec.stageCode}!");
                return false;
            }

            if (IsRespawning)
            {
                IsRespawning = false;
                RespawnTimeRemaining = 0f;
                RespawnReasonText = null;
                PlayerCurrentHp = PlayerMaxHp;
                PlayerCurrentMp = PlayerMaxMp;
                OnRespawnStateChanged?.Invoke(false);
            }

            var save = SaveService.Current;
            ActiveArenaMode = RemakeArenaMode.Campaign;
            save.activeDifficulty = (int)diff;
            save.activeAct = act;
            save.activeStage = stageNumber;
            save.stageWaveKills = 0;
            if (save.mainQuestIndex == 7 && act >= 1) save.mainQuestProgress = 1;

            SetActiveBoss(false, "", 0, 0);
            SaveService.Save();

            SyncArenaOrReturnToCampaignScene();

            OnStageChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            return true;
        }

        // =========================================================
        // KIEM LO (SWORD PATH + TALISMAN + STYLE)
        // =========================================================
        public void SetSwordStyle(RemakeSwordStyle style)
        {
            SaveService.Current.swordStyle = (int)style;
            InvalidateStatCache();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke(style == RemakeSwordStyle.KiemXuyen
                ? "Chuyển Kiếm Pháp: Kiếm Xuyên (+Tốc Độ & Bạo Kích)"
                : "Chuyển Kiếm Pháp: Phá Giáp (+Xuyên Giáp & Công Boss)");
        }

        public float GetSingleFodderMergeContribution(int mainLevel, int fodderLevel)
        {
            mainLevel = Mathf.Clamp(mainLevel, 1, 9);
            fodderLevel = Mathf.Clamp(fodderLevel, 1, 10);
            float basePair = CultivationTable.GetSwordMergeEffectiveChance(mainLevel, 0f);
            float sameLevelContrib = Mathf.Clamp(basePair * 0.56f, 0.14f, 0.55f);
            int delta = fodderLevel - mainLevel;
            if (delta == 0) return sameLevelContrib;
            if (delta == -1) return Mathf.Max(0.05f, sameLevelContrib * 0.45f);
            if (delta == -2) return Mathf.Max(0.03f, sameLevelContrib * 0.20f);
            if (delta <= -3) return Mathf.Max(0.02f, sameLevelContrib * 0.08f);
            return Mathf.Min(0.95f, sameLevelContrib * (1.55f + (delta - 1) * 0.35f));
        }

        public float ComputeFifaSwordMergeChance(int mainLevel, IReadOnlyList<int> fodderLevels, bool useForgeTalisman, out float fodderSum, out float bonusSum)
        {
            fodderSum = 0f;
            bonusSum = 0f;
            if (mainLevel < 1 || mainLevel >= 10 || fodderLevels == null || fodderLevels.Count == 0)
                return 0f;

            var save = SaveService.Current;
            bool isTutorialMerge = !save.tutorialCompleted && save.totalMerges == 0 && mainLevel == 1;
            int count = Mathf.Min(5, fodderLevels.Count);
            for (int i = 0; i < count; i++)
                fodderSum += GetSingleFodderMergeContribution(mainLevel, fodderLevels[i]);

            float pity = save.swordPityByLevel[mainLevel - 1];
            float daoFortune = GetDaoVanLevel("wealth_fortune") * 0.02f;
            float taliBonus = (useForgeTalisman && save.forgeTalismanCount > 0) ? 0.10f : 0f;
            bonusSum = pity + daoFortune + taliBonus;
            if (isTutorialMerge) return 1.0f;
            return Mathf.Clamp(fodderSum + bonusSum, 0.03f, 1.0f);
        }

        public bool TryMergeSwordFifa(bool mainFromEquipped, int mainIndex, IReadOnlyList<int> fodderBagIndices, bool useForgeTalisman)
        {
            var save = SaveService.Current;
            int mainLevel;
            if (mainFromEquipped)
            {
                if (mainIndex < 0 || mainIndex >= save.equippedSwordLevels.Count)
                {
                    OnToastMessage?.Invoke("Vui lòng chọn 1 Kiếm Chính hợp lệ!");
                    return false;
                }
                mainLevel = save.equippedSwordLevels[mainIndex];
            }
            else
            {
                if (mainIndex < 0 || mainIndex >= save.swordInventoryLevels.Count)
                {
                    OnToastMessage?.Invoke("Vui lòng chọn 1 Kiếm Chính hợp lệ!");
                    return false;
                }
                mainLevel = save.swordInventoryLevels[mainIndex];
            }

            if (mainLevel < 1 || mainLevel >= 10)
            {
                OnToastMessage?.Invoke("Kiếm đã đạt cấp tối đa (Cấp 10)!");
                return false;
            }

            var validFodders = new List<int>();
            if (fodderBagIndices != null)
            {
                for (int i = 0; i < fodderBagIndices.Count && validFodders.Count < 5; i++)
                {
                    int bIdx = fodderBagIndices[i];
                    if (bIdx < 0 || bIdx >= save.swordInventoryLevels.Count) continue;
                    if (!mainFromEquipped && bIdx == mainIndex) continue;
                    if (!validFodders.Contains(bIdx)) validFodders.Add(bIdx);
                }
            }

            if (validFodders.Count == 0)
            {
                OnToastMessage?.Invoke("Hãy thêm ít nhất 1 phôi kiếm (tối đa 5 phôi) vào ô ghép!");
                return false;
            }

            double cost = CultivationTable.GetSwordMergeCost(mainLevel);
            bool isTutorialMerge = !save.tutorialCompleted && save.totalMerges == 0 && mainLevel == 1;
            if (!isTutorialMerge && save.linhThach < cost)
            {
                OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(cost)} Linh Thạch để rèn hợp nhất!");
                return false;
            }
            if (useForgeTalisman && save.forgeTalismanCount <= 0)
                useForgeTalisman = false;

            var fodderLevels = new List<int>(validFodders.Count);
            for (int i = 0; i < validFodders.Count; i++)
                fodderLevels.Add(save.swordInventoryLevels[validFodders[i]]);

            float chance = ComputeFifaSwordMergeChance(mainLevel, fodderLevels, useForgeTalisman, out _, out _);
            if (!isTutorialMerge) save.linhThach -= cost;
            if (useForgeTalisman) save.forgeTalismanCount--;

            bool success = UnityEngine.Random.value <= chance;

            // Consume fodders descending while adjusting bag mainIndex if needed
            validFodders.Sort();
            int adjustedMainIdx = mainIndex;
            for (int i = validFodders.Count - 1; i >= 0; i--)
            {
                int rm = validFodders[i];
                save.swordInventoryLevels.RemoveAt(rm);
                if (!mainFromEquipped && rm < adjustedMainIdx) adjustedMainIdx--;
            }

            save.totalMerges++;
            if (success)
            {
                save.swordPityByLevel[mainLevel - 1] = 0f;
                int newLevel = mainLevel + 1;
                if (mainFromEquipped) save.equippedSwordLevels[mainIndex] = newLevel;
                else save.swordInventoryLevels[adjustedMainIdx] = newLevel;
                save.codexSwordsMask |= (1 << Mathf.Clamp(newLevel - 1, 0, 9));
                InvalidateStatCache();

                if (save.tutorialStep == 1 || save.tutorialStep == 2)
                {
                    save.tutorialStep = 3;
                    save.tutorialCompleted = true;
                }
                if (save.mainQuestIndex == 1) save.mainQuestProgress = 1;

                RemakeAudioService.Instance?.PlayMerge();
                RemakeArenaController.Instance?.SyncPlayerSwordsVisual();
                OnSwordMergeResolved?.Invoke(true, newLevel, 0f);
                OnToastMessage?.Invoke($"Rèn thành công! Phi Kiếm thăng cấp Lv.{newLevel} ({Mathf.RoundToInt(chance * 100)}%)!");
                if (newLevel >= 9)
                    RemakeOnlineService.Instance?.BroadcastThienDaoAnnouncement($"{save.daoName} vừa rèn thành công Phi Kiếm Cấp {newLevel}!");
            }
            else
            {
                float basePity = CultivationTable.SwordMergePityStep[mainLevel - 1];
                float pityGain = basePity * (0.55f + 0.15f * validFodders.Count);
                save.swordPityByLevel[mainLevel - 1] = Mathf.Clamp01(save.swordPityByLevel[mainLevel - 1] + pityGain);
                OnSwordMergeResolved?.Invoke(false, mainLevel, save.swordPityByLevel[mainLevel - 1]);
                OnToastMessage?.Invoke($"Rèn thất bại ({Mathf.RoundToInt(chance * 100)}%)! Giữ nguyên Kiếm Lv.{mainLevel} (+{Mathf.RoundToInt(pityGain * 100)}% Chúc Phúc)");
            }

            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            return success;
        }

        public bool TryMergeSwords(int fromLevel) => TryMergeSwords(fromLevel, false);

        public bool TryMergeSwords(int fromLevel, bool useForgeTalisman)
        {
            var save = SaveService.Current;
            if (fromLevel < 1 || fromLevel >= 10)
            {
                OnToastMessage?.Invoke("Kiếm đã đạt cấp tối đa (Cấp 10)!");
                return false;
            }

            int bagMatches = 0;
            for (int i = 0; i < save.swordInventoryLevels.Count; i++)
                if (save.swordInventoryLevels[i] == fromLevel) bagMatches++;

            int equippedMatches = 0;
            int equippedIdx = -1;
            for (int i = 0; i < save.equippedSwordLevels.Count; i++)
            {
                if (save.equippedSwordLevels[i] == fromLevel)
                {
                    equippedMatches++;
                    if (equippedIdx < 0) equippedIdx = i;
                }
            }

            if (bagMatches + equippedMatches < 2)
            {
                OnToastMessage?.Invoke($"Cần đủ 2 thanh Kiếm Cấp {fromLevel} để ghép!");
                return false;
            }

            double cost = CultivationTable.GetSwordMergeCost(fromLevel);
            bool isTutorialMerge = !save.tutorialCompleted && save.totalMerges == 0 && fromLevel == 1;
            if (!isTutorialMerge && save.linhThach < cost)
            {
                OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(cost)} Linh Thạch!");
                return false;
            }
            if (useForgeTalisman && save.forgeTalismanCount <= 0)
                useForgeTalisman = false;

            if (!isTutorialMerge) save.linhThach -= cost;
            if (useForgeTalisman) save.forgeTalismanCount--;

            float pity = save.swordPityByLevel[fromLevel - 1];
            float daoFortune = GetDaoVanLevel("wealth_fortune") * 0.02f;
            float taliBonus = useForgeTalisman ? 0.10f : 0f;
            float chance = isTutorialMerge ? 1.0f : Mathf.Clamp01(CultivationTable.GetSwordMergeEffectiveChance(fromLevel, pity + daoFortune + taliBonus));
            bool success = UnityEngine.Random.value <= chance;

            bool removedSub = RemoveOneSwordFromBag(fromLevel);
            if (!removedSub && equippedMatches >= 2)
            {
                for (int i = save.equippedSwordLevels.Count - 1; i >= 0; i--)
                {
                    if (save.equippedSwordLevels[i] == fromLevel && i != equippedIdx)
                    {
                        save.equippedSwordLevels.RemoveAt(i);
                        break;
                    }
                }
            }

            save.totalMerges++;
            if (success)
            {
                save.swordPityByLevel[fromLevel - 1] = 0f;
                int newLevel = fromLevel + 1;
                if (equippedIdx >= 0)
                    save.equippedSwordLevels[equippedIdx] = newLevel;
                else
                {
                    RemoveOneSwordFromBag(fromLevel);
                    save.swordInventoryLevels.Add(newLevel);
                }
                save.codexSwordsMask |= (1 << Mathf.Clamp(newLevel - 1, 0, 9));
                InvalidateStatCache();

                if (save.tutorialStep == 1 || save.tutorialStep == 2)
                {
                    save.tutorialStep = 3;
                    save.tutorialCompleted = true;
                }
                if (save.mainQuestIndex == 1) save.mainQuestProgress = 1;

                RemakeAudioService.Instance?.PlayMerge();
                RemakeArenaController.Instance?.SyncPlayerSwordsVisual();
                OnSwordMergeResolved?.Invoke(true, newLevel, 0f);
                OnToastMessage?.Invoke($"Ghép thành công Kiếm Cấp {newLevel}!");
                if (newLevel >= 9)
                    RemakeOnlineService.Instance?.BroadcastThienDaoAnnouncement($"{save.daoName} vừa rèn thành công Phi Kiếm Cấp {newLevel}!");
            }
            else
            {
                float pityGain = CultivationTable.SwordMergePityStep[fromLevel - 1];
                save.swordPityByLevel[fromLevel - 1] = Mathf.Clamp01(pity + pityGain);
                OnSwordMergeResolved?.Invoke(false, fromLevel, save.swordPityByLevel[fromLevel - 1]);
                OnToastMessage?.Invoke($"Ghép thất bại! Giữ nguyên Kiếm gốc (+{Mathf.RoundToInt(pityGain * 100)}% Chúc Phúc)");
            }

            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            return success;
        }

        public bool TryEquipFromBag(int bagIndex)
        {
            var save = SaveService.Current;
            if (bagIndex < 0 || bagIndex >= save.swordInventoryLevels.Count) return false;
            int lv = save.swordInventoryLevels[bagIndex];
            int maxSlots = CultivationTable.GetMaxSwordSlotsForRealm(save.realmIndex);

            if (save.equippedSwordLevels.Count < maxSlots)
            {
                save.swordInventoryLevels.RemoveAt(bagIndex);
                save.equippedSwordLevels.Add(lv);
            }
            else
            {
                int weakestIdx = 0;
                for (int i = 1; i < save.equippedSwordLevels.Count; i++)
                    if (save.equippedSwordLevels[i] < save.equippedSwordLevels[weakestIdx]) weakestIdx = i;
                int oldLv = save.equippedSwordLevels[weakestIdx];
                save.equippedSwordLevels[weakestIdx] = lv;
                save.swordInventoryLevels[bagIndex] = oldLv;
            }

            InvalidateStatCache();
            RemakeArenaController.Instance?.SyncPlayerSwordsVisual();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Đã xuất trận Phi Kiếm Cấp {lv}!");
            return true;
        }

        public bool TryUnequipSword(int equippedIndex)
        {
            var save = SaveService.Current;
            if (equippedIndex < 0 || equippedIndex >= save.equippedSwordLevels.Count) return false;
            int lv = save.equippedSwordLevels[equippedIndex];
            save.equippedSwordLevels.RemoveAt(equippedIndex);
            save.swordInventoryLevels.Add(lv);

            InvalidateStatCache();
            RemakeArenaController.Instance?.SyncPlayerSwordsVisual();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Đã tháo Phi Kiếm Cấp {lv} về Túi Phôi Kiếm!");
            return true;
        }

        // =========================================================
        // LUYEN DAN M2 (8 PILLS + 8 RANKS + BREW SLIDER + EXCESS SINKS)
        // =========================================================
        public bool TryBrewTrucCoPill() => TryBrewPill(0);

        public float GetAlchemyBrewSuccessChance(int targetRealm)
        {
            var save = SaveService.Current;
            targetRealm = Mathf.Clamp(targetRealm, 0, 7);
            return Mathf.Clamp(0.75f + (save.alchemistRank - targetRealm - 1) * 0.06f
                + (save.spiritFlameTier - 1) * 0.04f
                + GetDaoVanLevel("arts_alchemy") * 0.03f, 0.35f, 0.96f);
        }

        public void GetAlchemyQualityOdds(out int haPct, out int trungPct, out int thuongPct, out int cucPct)
        {
            var save = SaveService.Current;
            float danTongBonus = GetEquipSetCount(RemakeEquipSet.DanTong) >= 4 ? 0.05f : 0f;
            float shift = CultivationTable.GetAlchemistQualityBonus(save.alchemistRank) * 0.4f + (save.spiritFlameTier - 1) * 0.03f + danTongBonus;
            float pCuc = Mathf.Clamp01(0.04f + shift);
            float pThuong = Mathf.Clamp01(0.16f + shift) - pCuc;
            float pTrung = Mathf.Clamp01(0.44f + shift) - (pCuc + pThuong);
            float pHa = Mathf.Max(0f, 1f - pCuc - pThuong - pTrung);
            cucPct = Mathf.RoundToInt(pCuc * 100f);
            thuongPct = Mathf.RoundToInt(pThuong * 100f);
            trungPct = Mathf.RoundToInt(pTrung * 100f);
            haPct = Mathf.Max(0, 100 - cucPct - thuongPct - trungPct);
        }

        public bool TryStartBrewPill(int targetRealm)
        {
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Luyện Đan mở ở Luyện Khí Tầng 7!");
                return false;
            }
            if (IsBrewingPill)
            {
                OnToastMessage?.Invoke($"Đan Lô đang luyện đan ({BrewTimeRemaining:0.0}s), vui lòng chờ xuất lò!");
                return false;
            }

            targetRealm = Mathf.Clamp(targetRealm, 0, 7);
            int herbTierIdx = CultivationTable.GetPillRequiredHerbTier(targetRealm) - 1;
            int herbNeed = CultivationTable.GetPillRequiredHerbCount(targetRealm);
            double ltCost = CultivationTable.GetPillBrewLinhThachCost(targetRealm);

            if (save.herbsByTier[herbTierIdx] < herbNeed)
            {
                OnToastMessage?.Invoke($"Cần {herbNeed} {ActStageDatabaseSO.GetHerbName(herbTierIdx + 1)}!");
                return false;
            }
            if (save.linhThach < ltCost)
            {
                OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(ltCost)} Linh Thạch!");
                return false;
            }

            save.herbsByTier[herbTierIdx] -= herbNeed;
            save.linhThach -= ltCost;
            IsBrewingPill = true;
            BrewingTargetRealm = targetRealm;
            BrewTimeRemaining = BrewDuration;
            save.pendingBrewSeconds = BrewTimeRemaining;
            save.pendingBrewTargetRealm = BrewingTargetRealm;

            string pillName = CultivationTable.GetPillNameForRealmBreakthrough(targetRealm);
            int pct = Mathf.RoundToInt(GetAlchemyBrewSuccessChance(targetRealm) * 100f);
            RemakeAudioService.Instance?.PlayClick();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Khai lò luyện {pillName} ({pct}%) — Đang ngưng đan {BrewDuration:0.0}s...");
            return true;
        }

        public void AdvanceBrewTimer(float dt)
        {
            if (!IsBrewingPill) return;
            BrewTimeRemaining = Mathf.Max(0f, BrewTimeRemaining - Mathf.Max(0f, dt));
            SaveService.Current.pendingBrewSeconds = BrewTimeRemaining;
            if (BrewTimeRemaining <= 0f)
            {
                IsBrewingPill = false;
                ResolveBrewPillOutcome(BrewingTargetRealm);
            }
        }

        public bool TryBrewPill(int targetRealm)
        {
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Alchemy, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Luyện Đan mở ở Luyện Khí Tầng 7!");
                return false;
            }

            targetRealm = Mathf.Clamp(targetRealm, 0, 7);
            int herbTierIdx = CultivationTable.GetPillRequiredHerbTier(targetRealm) - 1;
            int herbNeed = CultivationTable.GetPillRequiredHerbCount(targetRealm);
            double ltCost = CultivationTable.GetPillBrewLinhThachCost(targetRealm);

            if (save.herbsByTier[herbTierIdx] < herbNeed)
            {
                OnToastMessage?.Invoke($"Cần {herbNeed} {ActStageDatabaseSO.GetHerbName(herbTierIdx + 1)}!");
                return false;
            }
            if (save.linhThach < ltCost)
            {
                OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(ltCost)} Linh Thạch!");
                return false;
            }

            save.herbsByTier[herbTierIdx] -= herbNeed;
            save.linhThach -= ltCost;
            return ResolveBrewPillOutcome(targetRealm);
        }

        private bool ResolveBrewPillOutcome(int targetRealm)
        {
            var save = SaveService.Current;
            float baseSuccess = GetAlchemyBrewSuccessChance(targetRealm);
            bool success = UnityEngine.Random.value <= baseSuccess;
            string pillName = CultivationTable.GetPillNameForRealmBreakthrough(targetRealm);
            if (success)
            {
                float danTongBonus = GetEquipSetCount(RemakeEquipSet.DanTong) >= 4 ? 0.05f : 0f;
                float qRoll = UnityEngine.Random.value - CultivationTable.GetAlchemistQualityBonus(save.alchemistRank) * 0.4f - (save.spiritFlameTier - 1) * 0.03f - danTongBonus;
                RemakePillQuality quality = qRoll < 0.04f ? RemakePillQuality.CucPham
                    : qRoll < 0.16f ? RemakePillQuality.ThuongPham
                    : qRoll < 0.44f ? RemakePillQuality.TrungPham
                    : RemakePillQuality.HaPham;
                AddPill(targetRealm, quality, 1);
                save.codexPillsMask |= (1 << Mathf.Clamp(targetRealm, 0, 7));
                GrantAlchemistXp((targetRealm + 1) * (quality >= RemakePillQuality.ThuongPham ? 25 : 15));
                if (save.mainQuestIndex == 4) save.mainQuestProgress = 1;
                if (quality == RemakePillQuality.CucPham)
                    RemakeOnlineService.Instance?.BroadcastThienDaoAnnouncement($"{save.daoName} vừa xuất lò {pillName} CỰC PHẨM (Đan Văn)!");
                OnToastMessage?.Invoke($"Xuất lò thành công: +1 {pillName} ({CultivationTable.GetPillQualityName(quality)})!");
            }
            else
            {
                save.herbDust++;
                AddPill(targetRealm, RemakePillQuality.PheDan, 1);
                GrantAlchemistXp((targetRealm + 1) * 6);
                OnToastMessage?.Invoke($"Nổ lò thất bại! Thu được 1 {pillName} (Phế Đan — Tế Hỏa/Luyện Hóa) & +1 Dược Trần");
            }

            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            OnAlchemyBrewResolved?.Invoke();
            return success;
        }

        public int TryMergeAllPills(int realmTarget)
        {
            int totalMerged = 0;
            RemakePillQuality[] tiers = { RemakePillQuality.HaPham, RemakePillQuality.TrungPham };
            for (int t = 0; t < tiers.Length; t++)
            {
                var q = tiers[t];
                while (GetPillCount(realmTarget, q) >= 3)
                {
                    for (int i = 0; i < 3; i++) ConsumePill(realmTarget, q);
                    RemakePillQuality next = q == RemakePillQuality.HaPham ? RemakePillQuality.TrungPham : RemakePillQuality.ThuongPham;
                    AddPill(realmTarget, next, 1);
                    GrantAlchemistXp(18);
                    totalMerged++;
                }
            }
            if (totalMerged > 0)
            {
                RemakeAudioService.Instance?.PlayMerge();
                SaveService.Save();
                OnStatsChanged?.Invoke();
                OnToastMessage?.Invoke($"Hợp Nhanh thành công {totalMerged} lượt {CultivationTable.GetPillNameForRealmBreakthrough(realmTarget)}!");
            }
            else
            {
                OnToastMessage?.Invoke("Cần ít nhất 3 viên Hạ Phẩm hoặc Trung Phẩm để Hợp Nhanh!");
            }
            return totalMerged;
        }

        public bool TryConsumePillForTuVi(int realmTarget, RemakePillQuality quality, int count = 1)
        {
            int owned = GetPillCount(realmTarget, quality);
            int actual = Mathf.Clamp(count, 1, owned);
            if (owned <= 0)
            {
                OnToastMessage?.Invoke("Không có đan dược để hấp thu!");
                return false;
            }
            for (int i = 0; i < actual; i++) ConsumePill(realmTarget, quality);
            var save = SaveService.Current;
            double perPillTuVi = Math.Round(CultivationTable.GetRequiredTuVi(Mathf.Clamp(realmTarget, 0, 7), 4) * (0.06 + (int)quality * 0.06));
            double gainTuVi = perPillTuVi * actual;
            AddTuViWithAutoLevel(gainTuVi);
            int dustGain = (1 + (int)quality) * actual;
            save.herbDust += dustGain;
            GrantAlchemistXp(6 * actual);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Hấp thu {actual} {CultivationTable.GetPillNameForRealmBreakthrough(realmTarget)}: +{CultivationTable.FormatNumber(gainTuVi)} Tu Vi & +{dustGain} Dược Trần!");
            return true;
        }

        public bool TryRefinePillToLinhThach(int realmTarget, RemakePillQuality quality, int count = 1)
        {
            int owned = GetPillCount(realmTarget, quality);
            int actual = Mathf.Clamp(count, 1, owned);
            if (owned <= 0)
            {
                OnToastMessage?.Invoke("Không có đan dược để luyện hóa!");
                return false;
            }
            for (int i = 0; i < actual; i++) ConsumePill(realmTarget, quality);
            var save = SaveService.Current;
            double perPillLt = Math.Round(CultivationTable.GetPillBrewLinhThachCost(Mathf.Clamp(realmTarget, 0, 7)) * (0.55 + (int)quality * 0.40));
            double gainLt = Math.Max(40, perPillLt) * actual;
            save.linhThach += gainLt;
            GrantAlchemistXp(5 * actual);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Luyện hóa {actual} {CultivationTable.GetPillNameForRealmBreakthrough(realmTarget)}: +{CultivationTable.FormatNumber(gainLt)} Linh Thạch!");
            return true;
        }

        public bool TryMergePills(int realmTarget, RemakePillQuality fromQuality)
        {
            if (fromQuality != RemakePillQuality.HaPham && fromQuality != RemakePillQuality.TrungPham)
                return false;
            if (GetPillCount(realmTarget, fromQuality) < 3)
            {
                OnToastMessage?.Invoke("Cần 3 viên cùng phẩm chất để Hợp Đan!");
                return false;
            }
            for (int i = 0; i < 3; i++) ConsumePill(realmTarget, fromQuality);
            RemakePillQuality next = fromQuality == RemakePillQuality.HaPham ? RemakePillQuality.TrungPham : RemakePillQuality.ThuongPham;
            AddPill(realmTarget, next, 1);
            GrantAlchemistXp(18);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Hợp Đan thành công: +1 {CultivationTable.GetPillNameForRealmBreakthrough(realmTarget)} ({CultivationTable.GetPillQualityName(next)})!");
            return true;
        }

        public bool TrySacrificePillToFlame(int realmTarget, RemakePillQuality quality)
        {
            var save = SaveService.Current;
            if (save.spiritFlameTier >= 4)
            {
                OnToastMessage?.Invoke("Linh Hỏa đã đạt đỉnh Tiên Thiên Hỏa!");
                return false;
            }

            int xpGain = 0;
            if (quality == RemakePillQuality.PheDan && GetPillCount(realmTarget, RemakePillQuality.PheDan) > 0)
            {
                ConsumePill(realmTarget, RemakePillQuality.PheDan);
                xpGain = 15 + realmTarget * 8;
            }
            else if (quality == RemakePillQuality.PheDan && save.herbDust > 0)
            {
                save.herbDust--;
                xpGain = 12;
            }
            else if (GetPillCount(realmTarget, quality) > 0)
            {
                ConsumePill(realmTarget, quality);
                xpGain = (int)quality * 22 + realmTarget * 12;
            }
            else
            {
                OnToastMessage?.Invoke("Không có Phế Đan / Dược Trần / Đan phẩm phù hợp để Tế Hỏa!");
                return false;
            }

            save.spiritFlameXp += xpGain;
            bool leveledUp = false;
            while (save.spiritFlameTier < 4 && save.spiritFlameXp >= CultivationTable.GetSpiritFlameNextTierXp(save.spiritFlameTier))
            {
                save.spiritFlameXp -= CultivationTable.GetSpiritFlameNextTierXp(save.spiritFlameTier);
                save.spiritFlameTier++;
                leveledUp = true;
                OnToastMessage?.Invoke($"Linh Hỏa thăng cấp: {CultivationTable.GetSpiritFlameName(save.spiritFlameTier)}!");
            }
            if (!leveledUp)
                OnToastMessage?.Invoke($"Tế Chân Hỏa thành công: +{xpGain} XP Hỏa Linh!");

            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            return true;
        }

        private void GrantAlchemistXp(int baseXp)
        {
            var save = SaveService.Current;
            float mult = 1f + GetDaoVanLevel("arts_alchemy") * 0.2f;
            save.alchemistXp += Mathf.Max(1, Mathf.RoundToInt(baseXp * mult));
            while (save.alchemistRank < 8 && save.alchemistXp >= CultivationTable.AlchemistRankXpReq[save.alchemistRank])
            {
                save.alchemistRank++;
                OnToastMessage?.Invoke($"Thăng cấp Đan Sư {CultivationTable.GetAlchemistRankName(save.alchemistRank)}!");
            }
        }

        public int GetPillCount(int realmTarget, RemakePillQuality quality)
        {
            var list = SaveService.Current.pills;
            for (int i = 0; i < list.Count; i++)
                if (list[i].realmTarget == realmTarget && list[i].quality == quality) return list[i].count;
            return 0;
        }

        // =========================================================
        // TRANG BI LOOT RPG (6 SLOTS + SETS + ENHANCE + REFORGE + FILTER)
        // =========================================================
        private RemakeRarity? RollAndGrantEquipment(ActStageDatabaseSO.StageSpec stage, bool wasBoss)
        {
            var save = SaveService.Current;
            int maxRar = Mathf.Clamp((int)stage.maxEquipmentRarity, 0, 4);
            float rRoll = UnityEngine.Random.value;
            int rarityIdx = 0;
            if (wasBoss && maxRar >= 1) rarityIdx = Mathf.Min(maxRar, rRoll < 0.2f ? maxRar : maxRar - 1);
            else if (rRoll < 0.08f && maxRar >= 2) rarityIdx = Math.Min(maxRar, 2);
            else if (rRoll < 0.30f && maxRar >= 1) rarityIdx = 1;

            RemakeRarity rarity = (RemakeRarity)rarityIdx;
            if ((int)rarity <= save.autoDismantleMaxRarity)
            {
                save.ore += 1 + rarityIdx * 2;
                save.linhThach += 35 * (1 + rarityIdx);
                return null;
            }

            var item = new RemakeEquipmentItem
            {
                uid = Guid.NewGuid().ToString("N").Substring(0, 10),
                slot = (RemakeEquipSlot)UnityEngine.Random.Range(0, 6),
                rarity = rarity,
                setId = UnityEngine.Random.value < 0.65f ? (RemakeEquipSet)UnityEngine.Random.Range(1, 4) : RemakeEquipSet.None,
                tier = Mathf.Clamp(1 + (stage.LinearIndex / 10), 1, 12)
            };
            RollSubstats(item);

            if (save.equipmentBag.Count >= 40)
            {
                save.ore += 2;
                return null;
            }
            save.equipmentBag.Add(item);
            if (wasBoss || rarity >= RemakeRarity.Linh)
                OnToastMessage?.Invoke($"Nhặt {CultivationTable.GetEquipSlotName(item.slot)} [{CultivationTable.GetRarityName(rarity)}]!");
            return rarity;
        }

        private static void RollSubstats(RemakeEquipmentItem item)
        {
            item.substats.Clear();
            int count = Mathf.Clamp(1 + (int)item.rarity, 1, 4);
            float scale = (1f + (int)item.rarity * 0.45f) * (1f + (item.tier - 1) * 0.35f);

            for (int i = 0; i < count; i++)
            {
                var st = (RemakeEquipStatType)((i + UnityEngine.Random.Range(0, 6)) % 6);
                float val = st switch
                {
                    RemakeEquipStatType.FlatSwordAtk => Mathf.Round(10f * scale),
                    RemakeEquipStatType.SwordDamagePct => Mathf.Round(6f * scale * 10f) / 10f,
                    RemakeEquipStatType.CritChancePct => Mathf.Clamp(Mathf.Round((1.5f + (int)item.rarity * 1.2f) * 10f) / 10f, 1f, 15f),
                    RemakeEquipStatType.MaxHpPct => Mathf.Round(8f * scale * 10f) / 10f,
                    RemakeEquipStatType.TuViGainPct => Mathf.Round(5f * scale * 10f) / 10f,
                    _ => Mathf.Round(7f * scale * 10f) / 10f
                };
                item.substats.Add(new RemakeEquipSubstatEntry { statType = st, value = val });
            }
        }

        public RemakeEquipmentItem GetEquippedItem(RemakeEquipSlot slot)
        {
            var list = SaveService.Current.equippedGear;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].slot == slot) return list[i];
            return null;
        }

        public bool TryEquipGear(string uid)
        {
            var save = SaveService.Current;
            int bagIdx = -1;
            for (int i = 0; i < save.equipmentBag.Count; i++)
                if (save.equipmentBag[i].uid == uid) { bagIdx = i; break; }
            if (bagIdx < 0) return false;

            var nextItem = save.equipmentBag[bagIdx];
            save.equipmentBag.RemoveAt(bagIdx);

            for (int i = 0; i < save.equippedGear.Count; i++)
            {
                if (save.equippedGear[i].slot == nextItem.slot)
                {
                    save.equipmentBag.Add(save.equippedGear[i]);
                    save.equippedGear.RemoveAt(i);
                    break;
                }
            }
            save.equippedGear.Add(nextItem);
            if (save.mainQuestIndex == 8 && save.activeMainFormation >= 0) save.mainQuestProgress = 1;
            RecomputePlayerVitals(false);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            OnToastMessage?.Invoke($"Đã trang bị {CultivationTable.GetRarityName(nextItem.rarity)} {CultivationTable.GetEquipSlotName(nextItem.slot)} Bậc {nextItem.tier}!");
            return true;
        }

        public bool TryUnequipGear(RemakeEquipSlot slot)
        {
            var save = SaveService.Current;
            for (int i = 0; i < save.equippedGear.Count; i++)
            {
                if (save.equippedGear[i] != null && save.equippedGear[i].slot == slot)
                {
                    if (save.equipmentBag.Count >= 40)
                    {
                        OnToastMessage?.Invoke("Kho Trang Bị đã đầy (40/40)! Hãy phân rã bớt trước khi tháo.");
                        return false;
                    }
                    var item = save.equippedGear[i];
                    save.equippedGear.RemoveAt(i);
                    save.equipmentBag.Add(item);
                    RecomputePlayerVitals(false);
                    RemakeAudioService.Instance?.PlayClick();
                    SaveService.Save();
                    OnStatsChanged?.Invoke();
                    OnToastMessage?.Invoke($"Đã tháo {CultivationTable.GetRarityName(item.rarity)} {CultivationTable.GetEquipSlotName(slot)} về Kho!");
                    return true;
                }
            }
            return false;
        }

        public bool TryEnhanceEquipSlot(int slotIdx)
        {
            var save = SaveService.Current;
            if (slotIdx < 0 || slotIdx >= 6) return false;
            int lv = save.equipSlotEnhanceLevels[slotIdx];
            double ltCost = CultivationTable.GetEquipSlotEnhanceCostLt(lv);
            int oreCost = CultivationTable.GetEquipSlotEnhanceCostOre(lv);
            if (save.linhThach < ltCost || save.ore < oreCost)
            {
                OnToastMessage?.Invoke($"Cần {CultivationTable.FormatNumber(ltCost)} LT & {oreCost} Quặng!");
                return false;
            }
            save.linhThach -= ltCost;
            save.ore -= oreCost;
            save.equipSlotEnhanceLevels[slotIdx]++;
            RecomputePlayerVitals(false);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Cường hóa ô {CultivationTable.GetEquipSlotName((RemakeEquipSlot)slotIdx)} lên +{lv + 1}!");
            return true;
        }

        public bool TryReforgeGear(string uid)
        {
            var save = SaveService.Current;
            var item = FindGearByUid(uid);
            if (item == null) return false;
            if (save.reforgeStoneCount <= 0 || save.ore < 3)
            {
                OnToastMessage?.Invoke("Cần 1 Đá Tẩy Luyện & 3 Quặng!");
                return false;
            }
            save.reforgeStoneCount--;
            save.ore -= 3;
            RollSubstats(item);
            RecomputePlayerVitals(false);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Tẩy luyện thành công dòng phụ {CultivationTable.GetEquipSlotName(item.slot)}!");
            return true;
        }

        public bool TryDismantleGear(string uid)
        {
            var save = SaveService.Current;
            for (int i = 0; i < save.equipmentBag.Count; i++)
            {
                if (save.equipmentBag[i].uid == uid)
                {
                    if (save.equipmentBag[i].isLocked)
                    {
                        OnToastMessage?.Invoke("Trang bị đang khóa! Bỏ khóa trước khi phân rã.");
                        return false;
                    }
                    int rar = (int)save.equipmentBag[i].rarity;
                    int oreGain = 2 + rar * 3;
                    double ltGain = 60 * (1 + rar);
                    save.equipmentBag.RemoveAt(i);
                    save.ore += oreGain;
                    save.linhThach += ltGain;
                    SaveService.Save();
                    OnStatsChanged?.Invoke();
                    OnToastMessage?.Invoke($"Phân rã thành công: +{oreGain} Quặng & +{CultivationTable.FormatNumber(ltGain)} LT!");
                    return true;
                }
            }
            return false;
        }

        public bool TryToggleGearLock(string uid)
        {
            var item = FindGearByUid(uid);
            if (item == null) return false;
            item.isLocked = !item.isLocked;
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke(item.isLocked
                ? $"Đã khóa bảo vệ {CultivationTable.GetEquipSlotName(item.slot)}!"
                : $"Đã mở khóa {CultivationTable.GetEquipSlotName(item.slot)}!");
            return true;
        }

        public int TryAutoEquipBestGear()
        {
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.EquipmentLoot, save.realmIndex, save.subStage))
                return 0;
            int changed = 0;
            for (int s = 0; s < 6; s++)
            {
                var slotEnum = (RemakeEquipSlot)s;
                var cur = GetEquippedItem(slotEnum);
                float curScore = CultivationTable.CalculateEquipItemScore(cur, save.realmIndex);
                RemakeEquipmentItem best = null;
                float bestScore = curScore;
                for (int i = 0; i < save.equipmentBag.Count; i++)
                {
                    var cand = save.equipmentBag[i];
                    if (cand == null || cand.slot != slotEnum) continue;
                    float sc = CultivationTable.CalculateEquipItemScore(cand, save.realmIndex);
                    if (sc > bestScore + 0.1f) { bestScore = sc; best = cand; }
                }
                if (best != null && TryEquipGear(best.uid))
                    changed++;
            }
            OnToastMessage?.Invoke(changed > 0
                ? $"Đã mặc nhanh {changed} Pháp Khí mạnh hơn!"
                : "Toàn bộ trang bị đang mặc đều đang tối ưu!");
            return changed;
        }

        public int TryQuickMergeSwords(int maxFromLevel = 3)
        {
            int merged = 0;
            for (int lv = 1; lv <= Mathf.Clamp(maxFromLevel, 1, 9); lv++)
            {
                int guard = 0;
                while (guard++ < 15 && TryMergeSwords(lv, false))
                    merged++;
            }
            OnToastMessage?.Invoke(merged > 0
                ? $"Ghép nhanh thành công {merged} lượt Kiếm!"
                : "Chưa đủ cặp Phôi Kiếm Cấp 1–3 hoặc thiếu Linh Thạch!");
            return merged;
        }

        public void SetAutoDismantleMaxRarity(int maxRarity)
        {
            SaveService.Current.autoDismantleMaxRarity = Mathf.Clamp(maxRarity, -1, 2);
            SaveService.Save();
            OnStatsChanged?.Invoke();

        }



        private RemakeEquipmentItem FindGearByUid(string uid)
        {
            var save = SaveService.Current;
            for (int i = 0; i < save.equippedGear.Count; i++)
                if (save.equippedGear[i].uid == uid) return save.equippedGear[i];
            for (int i = 0; i < save.equipmentBag.Count; i++)
                if (save.equipmentBag[i].uid == uid) return save.equipmentBag[i];
            return null;
        }

        // =========================================================
        // 6 DAI TRAN PHAP + O TRAN PHU HOA THAN
        // =========================================================
        public bool TryActivateFormation(int formIdx, bool asSubSlot)
        {
            var save = SaveService.Current;
            if (formIdx < 0 || formIdx >= RemakeCatalogs.Formations.Count) return false;
            var spec = RemakeCatalogs.Formations[formIdx];
            if (save.realmIndex < spec.minRealmIndex)
            {
                OnToastMessage?.Invoke($"Cần đạt {spec.realmReqText} để mở {spec.name}!");
                return false;
            }
            if (asSubSlot)
            {
                if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.FormationSubSlot, save.realmIndex, save.subStage))
                {
                    OnToastMessage?.Invoke("Ô Trận Phụ (50%) mở ở Hóa Thần Sơ Kỳ!");
                    return false;
                }
                if (save.activeMainFormation == formIdx)
                {
                    OnToastMessage?.Invoke("Không thể đặt trùng Trận Chính và Trận Phụ!");
                    return false;
                }
                save.activeSubFormation = save.activeSubFormation == formIdx ? -1 : formIdx;
            }
            else
            {
                if (save.activeSubFormation == formIdx) save.activeSubFormation = -1;
                save.activeMainFormation = formIdx;
            }

            if (save.mainQuestIndex == 8 && save.activeMainFormation >= 0 && save.equippedGear.Count > 0)
                save.mainQuestProgress = 1;

            RecomputePlayerVitals(false);
            RemakeArenaController.Instance?.TriggerFormationResonance(5.5f);
            RemakeAudioService.Instance?.PlayMerge();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            OnToastMessage?.Invoke(asSubSlot ? $"Đã đặt Trận Phụ: {spec.name}" : $"Đã khai trận: {spec.name}");
            return true;
        }

        // =========================================================
        // BREAKTHROUGH (+ HUONG + HO PHAP M3)
        // =========================================================
        public float ComputeBreakthroughChance(RemakePillQuality? selectedQuality)
        {
            var save = SaveService.Current;
            float baseChance = CultivationTable.GetBaseBreakthroughChance(save.realmIndex);
            float pillBonus = selectedQuality.HasValue ? CultivationTable.GetPillBreakthroughBonus(selectedQuality.Value) : 0f;
            float daoVanBonus = GetDaoVanLevel("branch_arts") * 0.015f + GetDaoVanLevel("arts_formation") * 0.02f;
            float hoPhapBonus = Mathf.Clamp(save.hoPhapBonusSteps, 0, 3) * 0.01f;
            return Mathf.Clamp(baseChance + pillBonus + daoVanBonus + hoPhapBonus, 0.03f, 0.98f);
        }

        public bool TryUseCalmIncense()
        {
            var save = SaveService.Current;
            if (save.consecutiveBreakthroughFails <= 0)
            {
                OnToastMessage?.Invoke("Đạo tâm đang bình ổn, chưa cần dùng Tĩnh Tâm Hương!");
                return false;
            }
            if (save.calmIncenseCount <= 0)
            {
                OnToastMessage?.Invoke("Không đủ Tĩnh Tâm Hương!");
                return false;
            }
            save.calmIncenseCount--;
            save.consecutiveBreakthroughFails = 0;
            save.mindCalmResetUtcTicks = 0;
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke("Đã thắp Tĩnh Tâm Hương: Xóa bỏ chuỗi phạt Tâm Ma!");
            return true;
        }

        public bool TryStartTribulation(RemakePillQuality? selectedPillQuality)
        {
            var save = SaveService.Current;
            if (IsInTribulation)
            {
                OnToastMessage?.Invoke($"Đang bế quan đón Thiên Kiếp ({TribulationTimeRemaining:0.0}s)!");
                return false;
            }
            if (IsRespawning)
            {
                OnToastMessage?.Invoke("Đang hồi sinh, vui lòng chờ hoàn tất trước khi Đột Phá!");
                return false;
            }
            if (!CultivationTable.RequiresBreakthroughAt(save.subStage))
            {
                OnToastMessage?.Invoke("Tầng thường tự động thăng cấp khi đủ 100% Tu Vi! Chỉ cần Đột Phá ở mốc Sơ Kỳ (T3) → Trung Kỳ (T6) → Đỉnh Phong (T10).");
                return false;
            }
            double req = GetRequiredTuViForCurrent();
            if (save.tuVi < req)
            {
                OnToastMessage?.Invoke("Tu Vi chưa đạt 100%, hãy tiếp tục tu luyện!");
                return false;
            }

            CheckMindCalmReset();
            bool isMajor = save.subStage >= 10;
            bool pillConsumed = false;
            RemakePillQuality? effectivePill = null;

            if (isMajor && selectedPillQuality.HasValue && selectedPillQuality.Value != RemakePillQuality.PheDan)
            {
                if (!ConsumePill(save.realmIndex, selectedPillQuality.Value))
                {
                    OnToastMessage?.Invoke("Không đủ Đan Đột Phá đã chọn!");
                    return false;
                }
                effectivePill = selectedPillQuality.Value;
                pillConsumed = true;
            }

            if (ActiveArenaMode != RemakeArenaMode.Campaign)
                ExitChallengeToCampaign();

            IsInTribulation = true;
            TribulationTimeRemaining = TribulationDuration;
            TribulationStrikesDone = 0;
            TribulationTargetChance = isMajor ? ComputeBreakthroughChance(effectivePill) : 1.0f;
            TribulationTargetLabel = GetTribulationTargetLabel(save);
            _tribulationSelectedPill = effectivePill;
            _tribulationPillConsumed = pillConsumed;
            _tribulationPreRolledOutcome = !isMajor || (UnityEngine.Random.value <= TribulationTargetChance);
            _tribulationNextStrikeTime = TribulationDuration - 0.5f;
            save.pendingTribulationSeconds = TribulationTimeRemaining;
            save.pendingTribulationChance = TribulationTargetChance;
            save.pendingTribulationSuccess = _tribulationPreRolledOutcome.Value;
            save.pendingTribulationPillQuality = effectivePill.HasValue ? (int)effectivePill.Value : -1;

            RecomputePlayerVitals(true);
            SetActiveBoss(false, "", 0, 0);
            SaveService.Save();

            OnTribulationStateChanged?.Invoke(true);
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Tiến vào Bế Quan Đón Thiên Kiếp (15s) hướng tới {TribulationTargetLabel}!");
            return true;
        }

        private static string GetTribulationTargetLabel(RemakeSaveData save)
        {
            int nextSub = save.subStage < 10 ? save.subStage + 1 : 1;
            int nextRealm = save.subStage < 10 ? save.realmIndex : Mathf.Min(8, save.realmIndex + 1);
            return CultivationTable.GetRealmDisplayName(nextRealm, nextSub);
        }

        private void ApplyTribulationStrikeHpDrain(int strikeIndex, int totalStrikes)
        {
            if (PlayerMaxHp <= 0) return;
            // Progressive 9-turn Heavenly Lightning HP drain: drops step-by-step down to ~4% HP on Strike 9 before final verdict!
            int idx = Mathf.Clamp(strikeIndex - 1, 0, TribulationHpRatios.Length - 1);
            float targetRatio = strikeIndex >= totalStrikes
                ? (_tribulationPreRolledOutcome == false ? 0.025f : 0.045f)
                : TribulationHpRatios[idx];

            double targetHp = Math.Max(1.0, Math.Round(PlayerMaxHp * targetRatio));
            double dmgTaken = Math.Max(0.0, PlayerCurrentHp - targetHp);
            PlayerCurrentHp = Math.Min(PlayerCurrentHp, targetHp);

            if (dmgTaken >= 1.0 && RemakeArenaController.Instance != null)
            {
                Vector3 hitPos = RemakeArenaController.Instance.PlayerWorldPosition + Vector3.up * 1.45f;
                RemakeVfxPool.Instance?.RequestDamagePopup(hitPos, dmgTaken, strikeIndex >= 7);
            }
            OnStatsChanged?.Invoke();
        }

        public void AdvanceTribulation(float dt)
        {
            if (!IsInTribulation) return;
            TribulationTimeRemaining = Mathf.Max(0f, TribulationTimeRemaining - Mathf.Max(0f, dt));

            if (TribulationTimeRemaining > 0f)
                SaveService.Current.pendingTribulationSeconds = TribulationTimeRemaining;

            while (TribulationStrikesDone < TribulationStrikesTotal && TribulationTimeRemaining <= _tribulationNextStrikeTime)
            {
                TribulationStrikesDone++;
                _tribulationNextStrikeTime -= TribulationStrikeInterval;
                ApplyTribulationStrikeHpDrain(TribulationStrikesDone, TribulationStrikesTotal);
                OnTribulationStrike?.Invoke(TribulationStrikesDone, TribulationStrikesTotal);
            }

            if (TribulationTimeRemaining <= 0f)
            {
                while (TribulationStrikesDone < TribulationStrikesTotal)
                {
                    TribulationStrikesDone++;
                    ApplyTribulationStrikeHpDrain(TribulationStrikesDone, TribulationStrikesTotal);
                    OnTribulationStrike?.Invoke(TribulationStrikesDone, TribulationStrikesTotal);
                }
                var pill = _tribulationSelectedPill;
                bool consumed = _tribulationPillConsumed;
                bool? preRolled = _tribulationPreRolledOutcome;
                IsInTribulation = false;
                TribulationTimeRemaining = 0f;
                _tribulationSelectedPill = null;
                _tribulationPillConsumed = false;
                _tribulationPreRolledOutcome = null;

                var save = SaveService.Current;
                save.pendingTribulationSeconds = 0f;
                save.pendingTribulationPillQuality = -1;
                ResolveBreakthroughInternal(pill, consumed, preRolled);
                OnTribulationStateChanged?.Invoke(false);
            }
        }

        public bool TryAttemptBreakthrough(RemakePillQuality? selectedPillQuality)
        {
            if (IsInTribulation) return false;
            return ResolveBreakthroughInternal(selectedPillQuality, false, null);
        }

        private bool ResolveBreakthroughInternal(RemakePillQuality? selectedPillQuality, bool pillAlreadyConsumed, bool? preRolledSuccess = null)
        {
            var save = SaveService.Current;
            if (!CultivationTable.RequiresBreakthroughAt(save.subStage))
            {
                OnToastMessage?.Invoke("Tầng thường sẽ tự động tăng cấp khi đủ 100% Tu Vi!");
                return false;
            }
            double req = GetRequiredTuViForCurrent();
            if (save.tuVi < req)
            {
                OnToastMessage?.Invoke("Tu Vi chưa đạt 100%, hãy tiếp tục tu luyện!");
                return false;
            }

            CheckMindCalmReset();

            if (save.subStage < 10)
            {
                save.tuVi = 0;
                save.subStage++;
                _gateCapToastSentForStage = false;
                if (save.subStage == 4 || save.subStage == 7)
                    save.daoPoints++;
                if (save.mainQuestIndex == 2) save.mainQuestProgress = save.subStage;

                RecomputePlayerVitals(true);
                RemakeAudioService.Instance?.PlayBreakthrough();
                RemakeVfxPool.Instance?.SpawnBreakthroughFlash(RemakeArenaController.Instance != null ? RemakeArenaController.Instance.PlayerWorldPosition : Vector3.zero, true);
                SaveService.Save();
                OnStatsChanged?.Invoke();
                OnQuestOrTutorialChanged?.Invoke();
                string msg = $"Đột phá thành công lên {CultivationTable.GetRealmDisplayName(save.realmIndex, save.subStage)}! (+1 Điểm Đạo Vận)";
                OnBreakthroughResolved?.Invoke(true, msg);
                OnToastMessage?.Invoke(msg);
                return true;
            }

            RemakePillQuality pillUsed = RemakePillQuality.PheDan;
            if (selectedPillQuality.HasValue && selectedPillQuality.Value != RemakePillQuality.PheDan)
            {
                if (!pillAlreadyConsumed && !ConsumePill(save.realmIndex, selectedPillQuality.Value))
                {
                    OnToastMessage?.Invoke("Không đủ Đan Đột Phá đã chọn!");
                    return false;
                }
                pillUsed = selectedPillQuality.Value;
            }

            float finalChance = ComputeBreakthroughChance(pillUsed == RemakePillQuality.PheDan ? null : pillUsed);
            bool success = preRolledSuccess ?? (UnityEngine.Random.value <= finalChance);

            if (success)
            {
                save.realmIndex++;
                save.subStage = 1;
                save.tuVi = 0;
                _gateCapToastSentForStage = false;
                save.consecutiveBreakthroughFails = 0;
                save.mindCalmResetUtcTicks = 0;
                save.hoPhapBonusSteps = 0;
                save.daoPoints += 2;
                RemakeOnlineService.Instance?.ConsumeHoPhapOnServer();
                if (save.realmIndex >= 7)
                    RemakeOnlineService.Instance?.BroadcastThienDaoAnnouncement($"{save.daoName} đã Độ Kiếp thành công, chứng đạo {CultivationTable.GetRealmDisplayName(save.realmIndex, 1)}!");
                if (save.mainQuestIndex == 6 && save.realmIndex >= 1) save.mainQuestProgress = 1;
                if (save.mainQuestIndex == 10 && save.realmIndex >= 2) save.mainQuestProgress = 1;

                RecomputePlayerVitals(true);
                RemakeArenaController.Instance?.SyncPlayerSwordsVisual();
                RemakeArenaController.Instance?.SyncPlayerRealmAura();
                RemakeAudioService.Instance?.PlayBreakthrough();
                RemakeVfxPool.Instance?.SpawnBreakthroughFlash(RemakeArenaController.Instance != null ? RemakeArenaController.Instance.PlayerWorldPosition : Vector3.zero, true);
                string msg = $"ĐỘ KIẾP THÀNH CÔNG! Bước vào {CultivationTable.GetRealmDisplayName(save.realmIndex, 1)}!";
                OnBreakthroughResolved?.Invoke(true, msg);
                OnToastMessage?.Invoke(msg);
            }
            else
            {
                float penaltyPct = CultivationTable.GetFailurePenaltyPercent(save.consecutiveBreakthroughFails, pillUsed);
                if ((RemakeCompanionId)save.activeCompanionId == RemakeCompanionId.JadeTurtle)
                    penaltyPct = Mathf.Max(0.30f, penaltyPct - 0.10f);
                save.tuVi = Math.Max(0, save.tuVi - req * penaltyPct);
                _gateCapToastSentForStage = false;
                save.consecutiveBreakthroughFails++;
                save.mindCalmResetUtcTicks = DateTime.UtcNow.AddHours(24).Ticks;
                PlayerCurrentHp = Math.Max(1.0, Math.Round(PlayerMaxHp * 0.06));

                RemakeVfxPool.Instance?.SpawnBreakthroughFlash(RemakeArenaController.Instance != null ? RemakeArenaController.Instance.PlayerWorldPosition : Vector3.zero, false);
                string msg = $"Độ kiếp thất bại (-{Mathf.RoundToInt(penaltyPct * 100)}% Tu Vi Tầng 10)! Cần Đan phẩm cao hơn hoặc Tĩnh Tâm.";
                OnBreakthroughResolved?.Invoke(false, msg);
                OnToastMessage?.Invoke(msg);
            }

            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            return success;
        }

        // =========================================================
        // 4 COMBAT SKILLS (LOI PHAT + AM DUONG + VAN KIEM + AN CHUONG)
        // =========================================================
        public bool IsSkillUnlocked(int slot)
        {
            var save = SaveService.Current;
            return slot switch
            {
                0 => CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillLoiPhat, save.realmIndex, save.subStage),
                1 => CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillYinYang, save.realmIndex, save.subStage),
                2 => CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillSwordRain, save.realmIndex, save.subStage),
                _ => CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SkillPalm, save.realmIndex, save.subStage)
            };
        }

        public float GetSkillCooldownRemaining(int slot) => _skillCooldowns[Mathf.Clamp(slot, 0, 3)];
        public float GetSkillCooldownTotal(int slot) => SkillBaseCooldowns[Mathf.Clamp(slot, 0, 3)];

        public int GetSkillLevel(int slot)
        {
            var s = SaveService.Current;
            return slot switch { 0 => s.loiPhatLevel, 1 => s.yinYangLevel, 2 => s.swordRainLevel, _ => s.palmLevel };
        }

        public bool GetSkillAuto(int slot)
        {
            var s = SaveService.Current;
            return slot switch { 0 => s.loiPhatAuto, 1 => s.yinYangAuto, 2 => s.swordRainAuto, _ => s.palmAuto };
        }

        public void SetSkillAuto(int slot, bool auto)
        {
            var s = SaveService.Current;
            switch (slot)
            {
                case 0: s.loiPhatAuto = auto; break;
                case 1: s.yinYangAuto = auto; break;
                case 2: s.swordRainAuto = auto; break;
                default: s.palmAuto = auto; break;
            }
            SaveService.Save();
            OnStatsChanged?.Invoke();

        }



        // =========================================================
        // V2: BOSS CHALLENGE, KY NGO, DEMON TOWER & SECRET REALM
        // =========================================================
        public bool CanChallengeActBossNow()
        {
            var save = SaveService.Current;
            if (IsInTribulation || SceneFlowController.Instance?.IsTransitioning == true ||
                ActiveArenaMode != RemakeArenaMode.Campaign || save.activeStage != 9) return false;
            var bossSpec = GetStageSpec((RemakeDifficulty)save.activeDifficulty, save.activeAct, 10);
            return bossSpec.LinearIndex <= save.highestUnlockedLinearStage && IsStageRealmUnlocked(bossSpec);
        }

        public bool TryChallengeCurrentActBoss()
        {
            var save = SaveService.Current;
            if (!CanChallengeActBossNow()) return false;
            ActiveArenaMode = RemakeArenaMode.Campaign;
            save.activeStage = 10;
            save.stageWaveKills = 0;
            PlayerCurrentHp = PlayerMaxHp;
            SetActiveBoss(false, "", 0, 0);
            SaveService.Save();
            RemakeArenaController.Instance?.ResetForStageChange();
            OnStageChanged?.Invoke();
            OnStatsChanged?.Invoke();
            return true;
        }

        public void SetAutoChallengeBoss(bool enabled)
        {
            SaveService.Current.autoChallengeBoss = enabled;
            SaveService.Save();
            OnStageChanged?.Invoke();
            OnToastMessage?.Invoke(enabled ? "Đã bật Tự Tiến Đánh Thủ Lĩnh X-10!" : "Đã tắt Tự Tiến Thủ Lĩnh (Giữ treo tại X-9).");
        }

        public bool TryClaimKyNgoChoice(int choiceIndex)
        {
            if (!HasKyNgoAvailable) return false;
            var save = SaveService.Current;
            HasKyNgoAvailable = false;
            save.kyNgoClaimedCount++;
            double ltReward = Math.Round(GetCurrentStageSpec().linhThachPerKill * 18);

            if (choiceIndex == 0)
            {
                save.tuViDoubleBuffEndUtcTicks = DateTime.UtcNow.AddSeconds(90).Ticks;
                save.linhThach += ltReward;
                OnToastMessage?.Invoke($"Kỳ Ngộ Linh Tuyền: x2 Tu Vi trong 90s & +{CultivationTable.FormatNumber(ltReward)} LT!");
            }
            else
            {
                int tierIdx = Mathf.Clamp(GetCurrentStageSpec().herbTier - 1, 0, 5);
                save.herbsByTier[tierIdx] += 4;
                save.forgeTalismanCount += 1;
                save.linhThach += ltReward;
                OnToastMessage?.Invoke($"Kỳ Ngộ Tàng Bảo: +4 Thảo Dược, +1 Phù Rèn & +{CultivationTable.FormatNumber(ltReward)} LT!");
            }

            RemakeAudioService.Instance?.PlayBreakthrough();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            return true;
        }

        public void EnsureDailyChallengeReset()
        {
            var save = SaveService.Current;
            long todayKey = DateTime.UtcNow.Year * 1000L + DateTime.UtcNow.DayOfYear;
            if (save.lastSecretRealmResetDayUtc != todayKey)
            {
                save.lastSecretRealmResetDayUtc = todayKey;
                save.secretRealmDailyRemaining = 3;
            }
        }

        public bool TryStartDemonTowerFloor(int floor)
        {
            if (IsInTribulation || SceneFlowController.Instance?.IsTransitioning == true) return false;
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.ImmortalTower, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Mở khóa Vạn Yêu Tháp tại Trúc Cơ Tầng 5!");
                return false;
            }
            int nextFloor = Mathf.Clamp(floor, 1, save.demonTowerHighestFloor + 1);
            if (nextFloor > 100) nextFloor = 100;

            GetWavePlan(RemakeArenaMode.DemonTower, false, nextFloor, out int totalWaves, out int[] counts, out _);
            int totalEnemies = 0;
            for (int i = 0; i < counts.Length; i++) totalEnemies += counts[i];
            ActiveArenaMode = RemakeArenaMode.DemonTower;
            ActiveChallengeFloor = nextFloor;
            ChallengeModeTimeRemaining = DemonTowerTimeLimit;
            ChallengeKillsRemaining = totalEnemies;
            CurrentWaveIndex = 1;
            TotalWavesInStage = totalWaves;
            WaveEnemiesRemaining = counts[0];
            PlayerCurrentHp = PlayerMaxHp;
            SetActiveBoss(false, "", 0, 0);
            if (SceneFlowController.Instance != null && SceneFlowController.Instance.IsInGame)
                SceneFlowController.Instance.SwitchToArenaScene(SceneFlowController.WorldDemonTowerScene, $"Vạn Yêu Tháp · Tầng {nextFloor}");
            else
                RemakeArenaController.Instance?.ResetForStageChange();
            OnStageChanged?.Invoke();
            OnToastMessage?.Invoke($"Vào Vạn Yêu Tháp Tầng {nextFloor} ({totalWaves} Đợt · 55s)!");
            return true;
        }

        public bool TryClaimTowerDailySalary()
        {
            var save = SaveService.Current;
            if (save.demonTowerHighestFloor <= 0)
            {
                OnToastMessage?.Invoke("Hãy vượt ít nhất Tầng 1 Vạn Yêu Tháp để nhận Bổng Lộc!");
                return false;
            }
            long todayKey = DateTime.UtcNow.Year * 1000L + DateTime.UtcNow.DayOfYear;
            if (save.lastTowerSalaryDayUtc == todayKey)
            {
                OnToastMessage?.Invoke("Hôm nay đã nhận Bổng Lộc Tháp rồi!");
                return false;
            }
            save.lastTowerSalaryDayUtc = todayKey;
            double lt = save.demonTowerHighestFloor * 260;
            int dust = save.demonTowerHighestFloor * 3;
            save.linhThach += lt;
            save.herbDust += dust;
            SaveService.Save();
            RemakeAudioService.Instance?.PlayBreakthrough();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Bổng Lộc Tháp Tầng {save.demonTowerHighestFloor}: +{CultivationTable.FormatNumber(lt)} LT & +{dust} Dược Trần!");
            return true;
        }

        public bool TryEnterSecretRealm(RemakeSecretRealmKind kind, int tier)
        {
            if (IsInTribulation || SceneFlowController.Instance?.IsTransitioning == true) return false;
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.SecretRealm, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Mở khóa Thái Hư Bí Cảnh tại Kim Đan Sơ Kỳ!");
                return false;
            }
            EnsureDailyChallengeReset();
            if (save.secretRealmDailyRemaining <= 0)
            {
                OnToastMessage?.Invoke("Hết 3 lượt Bí Cảnh hôm nay! Quay lại ngày mai.");
                return false;
            }
            tier = Mathf.Clamp(tier, 1, 5);
            save.secretRealmDailyRemaining--;
            GetWavePlan(RemakeArenaMode.SecretRealm, false, 1, out int totalWaves, out int[] counts, out _);
            int totalEnemies = 0;
            for (int i = 0; i < counts.Length; i++) totalEnemies += counts[i];
            string title = CultivationTable.GetSecretRealmTierSpec(kind, tier).title;
            ActiveArenaMode = RemakeArenaMode.SecretRealm;
            ActiveSecretRealmKind = kind;
            ActiveSecretRealmTier = tier;
            ChallengeModeTimeRemaining = 60f;
            ChallengeKillsRemaining = totalEnemies;
            CurrentWaveIndex = 1;
            TotalWavesInStage = totalWaves;
            WaveEnemiesRemaining = counts[0];
            PlayerCurrentHp = PlayerMaxHp;
            SetActiveBoss(false, "", 0, 0);
            SaveService.Save();
            if (SceneFlowController.Instance != null && SceneFlowController.Instance.IsInGame)
                SceneFlowController.Instance.SwitchToArenaScene(SceneFlowController.GetWorldSceneForSecretRealm(kind), title);
            else
                RemakeArenaController.Instance?.ResetForStageChange();
            OnStageChanged?.Invoke();
            OnToastMessage?.Invoke($"Tiến vào {title} ({totalWaves} Đợt · 60s)!");
            return true;
        }

        public bool TrySweepSecretRealm(RemakeSecretRealmKind kind, int tier)
        {
            var save = SaveService.Current;
            EnsureDailyChallengeReset();
            if (save.secretRealmDailyRemaining <= 0)
            {
                OnToastMessage?.Invoke("Hết lượt Bí Cảnh hôm nay!");
                return false;
            }
            int kIdx = (int)kind;
            if (save.secretRealmClearedTier[kIdx] < tier)
            {
                OnToastMessage?.Invoke($"Cần vượt qua Tầng {tier} ít nhất 1 lần trước khi Càn Quét!");
                return false;
            }
            save.secretRealmDailyRemaining--;
            GrantSecretRealmRewards(kind, tier);
            SaveService.Save();
            OnStatsChanged?.Invoke();
            return true;
        }

        public void ExitChallengeToCampaign()
        {
            if (ActiveArenaMode == RemakeArenaMode.Campaign) return;
            ActiveArenaMode = RemakeArenaMode.Campaign;
            SetActiveBoss(false, "", 0, 0);
            PlayerCurrentHp = PlayerMaxHp;
            SyncArenaOrReturnToCampaignScene();
            OnStageChanged?.Invoke();
        }

        private void HandleChallengeModeKill(Vector3? deathPos)
        {
            var save = SaveService.Current;
            ChallengeKillsRemaining = Math.Max(0, ChallengeKillsRemaining - 1);
            if (deathPos.HasValue)
                RemakeLootOrbPool.Instance?.SpawnMonsterDrops(deathPos.Value, ChallengeKillsRemaining == 0, ActiveSecretRealmKind == RemakeSecretRealmKind.HerbGarden, false, null);

            if (ChallengeKillsRemaining > 0)
            {
                EmitKillEvents(false, true, false);
                return;
            }

            if (ActiveArenaMode == RemakeArenaMode.DemonTower)
            {
                var spec = CultivationTable.GetDemonTowerFloorSpec(ActiveChallengeFloor);
                save.linhThach += spec.rewardLt;
                save.reforgeStoneCount += (ActiveChallengeFloor % 5 == 0) ? 2 : 1;
                if (ActiveChallengeFloor % 10 == 0)
                {
                    save.daoPoints++;
                    save.skillScrollCount++;
                }
                if (ActiveChallengeFloor > save.demonTowerHighestFloor)
                    save.demonTowerHighestFloor = ActiveChallengeFloor;

                RemakeAudioService.Instance?.PlayBreakthrough();

                if (save.autoClimbDemonTower && ActiveChallengeFloor < 100)
                {
                    int nextF = ActiveChallengeFloor + 1;
                    GetWavePlan(RemakeArenaMode.DemonTower, false, nextF, out int totalWaves, out int[] counts, out _);
                    int totalEnemies = 0;
                    for (int i = 0; i < counts.Length; i++) totalEnemies += counts[i];
                    ActiveChallengeFloor = nextF;
                    ChallengeModeTimeRemaining = DemonTowerTimeLimit;
                    ChallengeKillsRemaining = totalEnemies;
                    CurrentWaveIndex = 1;
                    TotalWavesInStage = totalWaves;
                    WaveEnemiesRemaining = counts[0];
                    PlayerCurrentHp = PlayerMaxHp;
                    SetActiveBoss(false, "", 0, 0);
                    SaveService.Save();
                    OnToastMessage?.Invoke($"Vượt Tháp T{nextF - 1}! Tự động tiến lên Tầng {nextF} (+{CultivationTable.FormatNumber(spec.rewardLt)} LT)!");
                    if (_aoeBatchDepth > 0) _batchPendingArenaReset = true;
                    else RemakeArenaController.Instance?.ResetForStageChange();
                }
                else
                {
                    OnToastMessage?.Invoke($"Vượt Tháp Tầng {ActiveChallengeFloor}: +{CultivationTable.FormatNumber(spec.rewardLt)} LT & Đá Tẩy Luyện!");
                    ActiveArenaMode = RemakeArenaMode.Campaign;
                    SetActiveBoss(false, "", 0, 0);
                    PlayerCurrentHp = PlayerMaxHp;
                    SaveService.Save();
                    if (_aoeBatchDepth > 0) _batchPendingArenaReset = true;
                    else SyncArenaOrReturnToCampaignScene();
                }
            }
            else if (ActiveArenaMode == RemakeArenaMode.SecretRealm)
            {
                int kIdx = (int)ActiveSecretRealmKind;
                if (ActiveSecretRealmTier > save.secretRealmClearedTier[kIdx])
                    save.secretRealmClearedTier[kIdx] = ActiveSecretRealmTier;
                GrantSecretRealmRewards(ActiveSecretRealmKind, ActiveSecretRealmTier);
                ActiveArenaMode = RemakeArenaMode.Campaign;
                SetActiveBoss(false, "", 0, 0);
                PlayerCurrentHp = PlayerMaxHp;
                SaveService.Save();
                if (_aoeBatchDepth > 0) _batchPendingArenaReset = true;
                else SyncArenaOrReturnToCampaignScene();
            }

            EmitKillEvents(true, true, false);
        }

        private void GrantSecretRealmRewards(RemakeSecretRealmKind kind, int tier)
        {
            var save = SaveService.Current;
            switch (kind)
            {
                case RemakeSecretRealmKind.SwordTomb:
                    int swordLv = Mathf.Clamp(tier, 1, 5);
                    save.swordInventoryLevels.Add(swordLv);
                    save.swordInventoryLevels.Add(Mathf.Max(1, swordLv - 1));
                    save.forgeTalismanCount += 1;
                    OnToastMessage?.Invoke($"Càn Kiếm Mộ T{tier}: +2 Phôi Kiếm & +1 Phù Rèn Kiếm!");
                    break;
                case RemakeSecretRealmKind.HerbGarden:
                    int hTier = Mathf.Clamp(tier - 1, 0, 5);
                    save.herbsByTier[hTier] += 5 + tier;
                    save.calmIncenseCount += 1;
                    OnToastMessage?.Invoke($"Thu Bách Thảo T{tier}: +{5 + tier} {ActStageDatabaseSO.GetHerbName(hTier + 1)} & +1 Tĩnh Tâm Hương!");
                    break;
                default:
                    int ore = 12 * tier;
                    save.ore += ore;
                    save.reforgeStoneCount += 1 + tier / 2;
                    OnToastMessage?.Invoke($"Khai Tinh Thiết T{tier}: +{ore} Quặng & +{1 + tier / 2} Đá Tẩy Luyện!");
                    break;
            }
            RemakeAudioService.Instance?.PlayBreakthrough();
        }



        public bool TryCastLoiPhat() => TryCastSkill(0);

        public bool TryCastSkill(int slot)
        {
            slot = Mathf.Clamp(slot, 0, 3);
            if (!IsSkillUnlocked(slot)) return false;
            if (_skillCooldowns[slot] > 0f || PlayerCurrentMp < SkillMpCosts[slot]) return false;

            Vector3 targetPos = Vector3.zero;
            if (RemakeArenaController.Instance != null)
            {
                if (slot == 1) targetPos = RemakeArenaController.Instance.PlayerWorldPosition;
                else if (!RemakeArenaController.Instance.TryFindPriorityTargetPos(out targetPos)) return false;
            }

            PlayerCurrentMp -= SkillMpCosts[slot];
            _skillCooldowns[slot] = SkillBaseCooldowns[slot];

            int lv = GetSkillLevel(slot);
            double awakenMult = lv >= 30 ? 2.2 : lv >= 20 ? 1.65 : lv >= 10 ? 1.3 : 1.0;
            double coef = slot switch
            {
                0 => (3.2 + lv * 0.25),
                1 => (2.4 + lv * 0.22),
                2 => (4.2 + lv * 0.32),
                _ => (5.6 + lv * 0.42)
            };
            double dmg = Math.Round(GetTotalSwordAttackPower() * coef * awakenMult);

            RemakeAudioService.Instance?.PlayLoiPhat();
            BeginAoEDamageBatch();
            try
            {
                if (BossShieldActive)
                    ApplyBossShieldDamage(dmg, true);
                if (slot == 0) OnLoiPhatCast?.Invoke(targetPos, dmg, true);
                OnSkillCast?.Invoke(slot, targetPos, dmg, true);
                CheckDaoVanAndSkillQuest();
                EmitKillEvents(true, false, false);
            }
            finally
            {
                EndAoEDamageBatch();
            }
            return true;
        }

        public bool TryUpgradeLoiPhat() => TryUpgradeSkill(0, false);

        public bool TryUpgradeSkill(int slot, bool useScrollIfAvailable = true)
        {
            slot = Mathf.Clamp(slot, 0, 3);
            if (!IsSkillUnlocked(slot))
            {
                OnToastMessage?.Invoke("Chưa đủ cảnh giới mở khóa kỹ năng này!");
                return false;
            }
            var save = SaveService.Current;
            int lv = GetSkillLevel(slot);
            if (useScrollIfAvailable && save.skillScrollCount > 0)
            {
                save.skillScrollCount--;
            }
            else
            {
                double cost = Math.Round((180 + slot * 320) * Math.Pow(1.22, lv - 1));
                if (save.linhThach < cost)
                {
                    OnToastMessage?.Invoke($"Thiếu {CultivationTable.FormatNumber(cost)} Linh Thạch (hoặc Tàn Quyển Công Pháp)!");
                    return false;
                }
                save.linhThach -= cost;
            }

            int nextLv = lv + 1;
            switch (slot)
            {
                case 0: save.loiPhatLevel = nextLv; break;
                case 1: save.yinYangLevel = nextLv; break;
                case 2: save.swordRainLevel = nextLv; break;
                default: save.palmLevel = nextLv; break;
            }
            SaveService.Save();
            RemakeAudioService.Instance?.PlayMerge();
            OnStatsChanged?.Invoke();
            string wakeMsg = (nextLv == 10 || nextLv == 20 || nextLv == 30) ? " (THỨC TỈNH ĐỘT BIẾN!)" : "";
            OnToastMessage?.Invoke($"Nâng Kỹ Năng #{slot + 1} lên Cấp {nextLv}{wakeMsg}!");
            return true;
        }

        // =========================================================
        // QUESTS, 30-DAY PIONEER DIARY & CATCHUP
        // =========================================================
        public bool TryClaimMainQuest()
        {
            var save = SaveService.Current;
            if (save.mainQuestIndex >= RemakeCatalogs.MainQuests.Count) return false;
            var q = RemakeCatalogs.MainQuests[save.mainQuestIndex];
            if (save.mainQuestProgress < q.targetCount) return false;

            save.linhThach += q.rewardLinhThach;
            save.daoPoints += q.rewardDaoPoints;
            if (q.rewardSwordLevel > 0) save.swordInventoryLevels.Add(q.rewardSwordLevel);

            save.mainQuestIndex++;
            save.mainQuestProgress = SyncQuestInitialProgress(save.mainQuestIndex);
            RemakeAudioService.Instance?.PlayBreakthrough();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnQuestOrTutorialChanged?.Invoke();
            OnToastMessage?.Invoke($"Hoàn thành nhiệm vụ! +{CultivationTable.FormatNumber(q.rewardLinhThach)} Linh Thạch");
            return true;
        }

        public bool IsPioneerMilestoneReady(PioneerMilestoneSpec m)
        {
            var save = SaveService.Current;
            bool already = (save.claimedPioneerMilestonesMask & (1 << m.index)) != 0;
            return !already && (save.realmIndex >= m.reqRealmIndex || save.highestUnlockedLinearStage >= m.reqLinearStage);
        }

        public bool TryClaimPioneerMilestone(int index)
        {
            if (index < 0 || index >= RemakeCatalogs.PioneerMilestones.Count) return false;
            var m = RemakeCatalogs.PioneerMilestones[index];
            if (!IsPioneerMilestoneReady(m)) return false;
            var save = SaveService.Current;
            save.claimedPioneerMilestonesMask |= (1 << m.index);
            save.linhThach += m.rewardLinhThach;
            save.daoPoints += m.rewardDaoPoints;
            save.forgeTalismanCount += m.rewardForgeTalismans;
            save.skillScrollCount += m.rewardSkillScrolls;
            save.calmIncenseCount += m.rewardCalmIncense;
            RemakeAudioService.Instance?.PlayBreakthrough();
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Nhận {m.title}: +{CultivationTable.FormatNumber(m.rewardLinhThach)} LT & +{m.rewardDaoPoints} Đạo Vận!");
            return true;
        }

        public void ApplyCatchupSimulation(double seconds, double efficiency, bool forceSummaryPopup = false)
        {
            var save = SaveService.Current;
            var stage = GetCurrentStageSpec();
            double offMult = 1.0 + GetDaoVanLevel("branch_cultivation") * 0.05 + GetDaoVanLevel("cult_offline") * 0.14;
            double kills = Math.Max(0, seconds * Math.Max(0.25, save.measuredKillsPerSec) * efficiency * offMult);
            if (kills < 1.0) return;

            double totalTuVi = Math.Round(kills * stage.tuViPerKill * GetTuViMultiplier());
            double totalLt = Math.Round(kills * stage.linhThachPerKill * GetLinhThachMultiplier());
            AddTuViWithAutoLevel(totalTuVi);
            save.linhThach += totalLt;

            int herbs = Mathf.FloorToInt((float)(kills * stage.herbDropRate));
            if (herbs > 0) save.herbsByTier[Mathf.Clamp(stage.herbTier - 1, 0, 5)] += herbs;

            int embryos = Mathf.Min(20, Mathf.FloorToInt((float)(kills * stage.swordDropRate)));
            for (int i = 0; i < embryos; i++)
                save.swordInventoryLevels.Add(stage.swordDropMinLevel);

            CheckMindCalmReset();
            SaveService.Save();
            OnStatsChanged?.Invoke();

            LastOfflineSummary = new OfflineCatchupSummary
            {
                elapsedSeconds = seconds,
                efficiency = efficiency,
                killsSimulated = Mathf.FloorToInt((float)kills),
                tuViGained = totalTuVi,
                linhThachGained = totalLt,
                herbsGained = herbs,
                herbTier = stage.herbTier,
                swordsGained = embryos,
                bonusClaimed = false
            };

            // Avoid toast spam on tiny scene loads (<60s): silent catchup; 1-15m -> short toast; >=15m -> full Welcome-Back Popup!
            if (forceSummaryPopup || seconds >= 900.0)
            {
                OnOfflineCatchupSummary?.Invoke(LastOfflineSummary);
            }
            else if (seconds >= 60.0)
            {
                string durationLabel = $"{Mathf.FloorToInt((float)(seconds / 60))}p";
                OnToastMessage?.Invoke($"Bế Quan ({durationLabel}): +{CultivationTable.FormatNumber(totalTuVi)} Tu Vi · +{CultivationTable.FormatNumber(totalLt)} LT");
            }
        }

        public bool TryClaimOfflineBonusWithCoDuyen()
        {
            if (LastOfflineSummary == null || LastOfflineSummary.bonusClaimed) return false;
            var save = SaveService.Current;
            const int cost = 10;
            if (save.coDuyen < cost)
            {
                OnToastMessage?.Invoke($"Cần {cost} Cơ Duyên để nhận thêm +50% thưởng Bế Quan!");
                return false;
            }
            save.coDuyen -= cost;
            double extraTv = Math.Round(LastOfflineSummary.tuViGained * 0.5);
            double extraLt = Math.Round(LastOfflineSummary.linhThachGained * 0.5);
            AddTuViWithAutoLevel(extraTv);
            save.linhThach += extraLt;
            LastOfflineSummary.bonusClaimed = true;
            SaveService.Save();
            RemakeAudioService.Instance?.PlayBreakthrough();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Nhận thêm +50% Bế Quan: +{CultivationTable.FormatNumber(extraTv)} Tu Vi & +{CultivationTable.FormatNumber(extraLt)} LT!");
            return true;
        }

        public void SetAutoClimbDemonTower(bool enabled)
        {
            SaveService.Current.autoClimbDemonTower = enabled;
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke(enabled ? "Vạn Yêu Tháp: Đã bật Tự Leo Tầng Kế!" : "Vạn Yêu Tháp: Đã tắt Tự Leo Tầng.");
        }

        // =========================================================
        // V3: CODEX COLLECTION, COMPANIONS & ASYNC PVP (LUAN KIEM DAI)
        // =========================================================
        public int GetCodexUnlockedCount(out int swords, out int bosses, out int pills)
        {
            var save = SaveService.Current;
            swords = CountBits(save.codexSwordsMask, 10);
            bosses = CountBits(save.codexBossesMask, 3);
            pills = CountBits(save.codexPillsMask, 8);
            return swords + bosses + pills;
        }

        private static int CountBits(int mask, int maxBits)
        {
            int c = 0;
            for (int i = 0; i < maxBits; i++)
                if ((mask & (1 << i)) != 0) c++;
            return c;
        }

        public static readonly int[] CodexMilestoneThresholds = { 3, 6, 9, 12, 15, 18, 21 };

        public bool IsCodexMilestoneReady(int tierIdx)
        {
            if (tierIdx < 0 || tierIdx >= CodexMilestoneThresholds.Length) return false;
            var save = SaveService.Current;
            if ((save.codexClaimedMask & (1 << tierIdx)) != 0) return false;
            return GetCodexUnlockedCount(out _, out _, out _) >= CodexMilestoneThresholds[tierIdx];
        }

        public bool TryClaimCodexMilestone(int tierIdx)
        {
            if (!IsCodexMilestoneReady(tierIdx)) return false;
            var save = SaveService.Current;
            save.codexClaimedMask |= (1 << tierIdx);
            int coDuyenGain = 15 + tierIdx * 10;
            double ltGain = Math.Round(500 * Math.Pow(2.4, tierIdx));
            save.coDuyen += coDuyenGain;
            save.linhThach += ltGain;
            save.forgeTalismanCount += 1;
            InvalidateStatCache();
            SaveService.Save();
            RemakeAudioService.Instance?.PlayBreakthrough();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke($"Đồ Giám Mốc {tierIdx + 1}: +{coDuyenGain} Cơ Duyên, +{CultivationTable.FormatNumber(ltGain)} LT & +1 Phù Rèn!");
            return true;
        }

        public bool TryUnlockOrSelectCompanion(RemakeCompanionId id)
        {
            var save = SaveService.Current;
            if (id == RemakeCompanionId.None)
            {
                save.activeCompanionId = 0;
                RecomputePlayerVitals(false);
                SaveService.Save();
                OnStatsChanged?.Invoke();
                return true;
            }
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.Companions, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Linh Thú Hộ Đạo mở ở Trúc Cơ Tầng 7!");
                return false;
            }
            var spec = CultivationTable.GetCompanionSpec(id);
            int bit = 1 << (int)id;
            if ((save.unlockedCompanionsMask & bit) == 0)
            {
                if (save.realmIndex < spec.reqRealmIndex)
                {
                    OnToastMessage?.Invoke($"Cần đạt {CultivationTable.RealmNames[spec.reqRealmIndex]} để thu phục {spec.name}!");
                    return false;
                }
                double cost = Math.Round(1200 * Math.Pow(3.2, (int)id - 1));
                if (save.linhThach < cost)
                {
                    OnToastMessage?.Invoke($"Cần {CultivationTable.FormatNumber(cost)} Linh Thạch để kết khế ước {spec.name}!");
                    return false;
                }
                save.linhThach -= cost;
                save.unlockedCompanionsMask |= bit;
                OnToastMessage?.Invoke($"Thu phục thành công Linh Thú: {spec.name}!");
            }
            else
            {
                OnToastMessage?.Invoke($"Đã xuất chiến Linh Thú: {spec.name}!");
            }
            save.activeCompanionId = (int)id;
            RecomputePlayerVitals(false);
            SaveService.Save();
            RemakeAudioService.Instance?.PlayMerge();
            OnStatsChanged?.Invoke();
            return true;
        }

        public double GetPlayerCombatPowerEstimate()
        {
            EnsureStatCache();
            double dps = GetTotalSwordAttackPower() * _cachedSwordSpeedMult * (1.0 + _cachedCritChance * (_cachedCritDmgMult - 1.0));
            return Math.Max(10.0, Math.Round(dps * 4.5 + PlayerMaxHp * 0.45));
        }

        public bool IsFeatureTutorialCompleted(RemakeFeatureId fid)
        {
            int bit = 1 << (int)fid;
            return (SaveService.Current.completedFeatureTutorialsMask & bit) != 0;
        }

        public void MarkFeatureTutorialCompleted(RemakeFeatureId fid)
        {
            int bit = 1 << (int)fid;
            var save = SaveService.Current;
            if ((save.completedFeatureTutorialsMask & bit) != 0) return;
            save.completedFeatureTutorialsMask |= bit;
            SaveService.Save();
            OnQuestOrTutorialChanged?.Invoke();
        }

        public void EnsureDailyPvpReset()
        {
            var save = SaveService.Current;
            long todayKey = DateTime.UtcNow.Year * 1000L + DateTime.UtcNow.DayOfYear;
            if (save.lastPvpResetDayUtc != todayKey)
            {
                save.lastPvpResetDayUtc = todayKey;
                save.pvpDailyRemaining = 5;
            }
        }

        public bool TryChallengePvpOpponent(string opponentName, double opponentPower)
        {
            var save = SaveService.Current;
            if (!CultivationTable.IsFeatureUnlocked(RemakeFeatureId.PvpArena, save.realmIndex, save.subStage))
            {
                OnToastMessage?.Invoke("Luận Kiếm Đài mở ở Nguyên Anh Tầng 5!");
                return false;
            }
            EnsureDailyPvpReset();
            if (save.pvpDailyRemaining <= 0)
            {
                OnToastMessage?.Invoke("Đã hết 5 lượt Luận Kiếm hôm nay! Quay lại ngày mai.");
                return false;
            }
            save.pvpDailyRemaining--;
            double myPower = GetPlayerCombatPowerEstimate();
            double ratio = myPower / Math.Max(1.0, opponentPower);
            float winChance = Mathf.Clamp((float)(0.50 + (ratio - 1.0) * 0.65), 0.12f, 0.96f);
            bool won = UnityEngine.Random.value <= winChance;
            if (won)
            {
                save.pvpWins++;
                int prestige = 85;
                double lt = Math.Round(GetCurrentStageSpec().linhThachPerKill * 24);
                save.swordPrestigePoints += prestige;
                save.linhThach += lt;
                RemakeAudioService.Instance?.PlayBreakthrough();
                OnToastMessage?.Invoke($"Chiến thắng {opponentName}! +{prestige} Kiếm Vọng & +{CultivationTable.FormatNumber(lt)} LT!");
                RemakeOnlineService.Instance?.BroadcastThienDaoAnnouncement($"{save.daoName} vừa đánh bại {opponentName} tại Luận Kiếm Đài!");
            }
            else
            {
                int prestige = 30;
                save.swordPrestigePoints += prestige;
                RemakeAudioService.Instance?.PlayClick();
                OnToastMessage?.Invoke($"Thất bại trước {opponentName}! Nhận +{prestige} Kiếm Vọng an ủi.");
            }
            SaveService.Save();
            OnStatsChanged?.Invoke();
            return won;
        }

        public bool TryBuyPvpReward(int rewardIdx)
        {
            var save = SaveService.Current;
            int cost = rewardIdx switch { 0 => 120, 1 => 140, 2 => 180, 3 => 350, _ => 600 };
            if (save.swordPrestigePoints < cost)
            {
                OnToastMessage?.Invoke($"Thiếu {cost} Điểm Kiếm Vọng!");
                return false;
            }
            if (rewardIdx == 3 && (save.unlockedFramesMask & (1 << 1)) != 0)
            {
                return TryEquipAvatarFrame(1);
            }
            if (rewardIdx == 4 && (save.unlockedFramesMask & (1 << 2)) != 0)
            {
                return TryEquipAvatarFrame(2);
            }
            save.swordPrestigePoints -= cost;
            switch (rewardIdx)
            {
                case 0: save.forgeTalismanCount += 2; OnToastMessage?.Invoke("Đổi thành công +2 Phù Rèn Kiếm!"); break;
                case 1: save.reforgeStoneCount += 3; OnToastMessage?.Invoke("Đổi thành công +3 Đá Tẩy Luyện!"); break;
                case 2: save.calmIncenseCount += 1; OnToastMessage?.Invoke("Đổi thành công +1 Tĩnh Tâm Hương!"); break;
                case 3:
                    save.unlockedFramesMask |= (1 << 1);
                    save.equippedAvatarFrame = 1;
                    OnToastMessage?.Invoke("Mở khóa & đeo Khung Avatar Kiếm Thánh!");
                    break;
                default:
                    save.unlockedFramesMask |= (1 << 2);
                    save.equippedAvatarFrame = 2;
                    OnToastMessage?.Invoke("Mở khóa & đeo Khung Avatar Thiên Đạo Chí Tôn!");
                    break;
            }
            SaveService.Save();
            RemakeAudioService.Instance?.PlayMerge();
            OnStatsChanged?.Invoke();
            return true;
        }

        public bool TryEquipAvatarFrame(int frameId)
        {
            var save = SaveService.Current;
            int bit = 1 << Mathf.Clamp(frameId, 0, 2);
            if ((save.unlockedFramesMask & bit) == 0) return false;
            save.equippedAvatarFrame = Mathf.Clamp(frameId, 0, 2);
            SaveService.Save();
            OnStatsChanged?.Invoke();
            OnToastMessage?.Invoke("Đã đổi Khung Avatar!");
            return true;
        }

        private void CheckMindCalmReset()
        {
            var save = SaveService.Current;
            if (save.consecutiveBreakthroughFails > 0 && save.mindCalmResetUtcTicks > 0 && DateTime.UtcNow.Ticks >= save.mindCalmResetUtcTicks)
            {
                save.consecutiveBreakthroughFails = 0;
                save.mindCalmResetUtcTicks = 0;
                OnToastMessage?.Invoke("Trấn Tâm 24h hoàn tất: Đạo tâm đã khôi phục bình ổn!");
            }
        }

        private void RecomputePlayerVitals(bool fillToMax)
        {
            InvalidateStatCache();
            var save = SaveService.Current;
            int realm = save.realmIndex;
            double baseHp = 220.0 * Math.Pow(2.3, realm) + save.subStage * 28.0 * Math.Pow(1.8, realm);
            double flatBody = GetDaoVanLevel("branch_body") * 140.0 * Math.Pow(1.6, realm);
            float pctMult = 1f + GetDaoVanLevel("branch_body") * 0.06f
                + ((RemakeCompanionId)save.activeCompanionId == RemakeCompanionId.JadeTurtle ? 0.15f : 0f)
                + GetEquipmentStatSum(RemakeEquipStatType.MaxHpPct) / 100f
                + GetFormationBonus(f => f.maxHpPct) / 100f;
            int huyenVu = GetEquipSetCount(RemakeEquipSet.HuyenVu);
            if (huyenVu >= 2) pctMult += 0.12f;
            if (huyenVu >= 4) pctMult += 0.25f;

            PlayerMaxHp = Math.Round((baseHp + flatBody) * pctMult);
            PlayerCurrentHp = fillToMax ? PlayerMaxHp : Math.Min(PlayerMaxHp, Math.Max(1, PlayerCurrentHp));
            PlayerMaxMp = 120f + realm * 25f;
            if (fillToMax) PlayerCurrentMp = PlayerMaxMp;
        }

        private void CheckDaoVanAndSkillQuest()
        {
            var save = SaveService.Current;
            if (save.mainQuestIndex == 3 && GetDaoVanLevel("core_daotam") >= 1) save.mainQuestProgress = 1;
        }

        private int SyncQuestInitialProgress(int questIdx)
        {
            var save = SaveService.Current;
            bool hasAnyTrucCoPill = GetPillCount(0, RemakePillQuality.HaPham) + GetPillCount(0, RemakePillQuality.TrungPham)
                + GetPillCount(0, RemakePillQuality.ThuongPham) + GetPillCount(0, RemakePillQuality.CucPham) > 0;
            return questIdx switch
            {
                1 => save.totalMerges > 0 ? 1 : 0,
                2 => save.subStage,
                3 => GetDaoVanLevel("core_daotam") >= 1 ? 1 : 0,
                4 => hasAnyTrucCoPill ? 1 : 0,
                5 => save.bossAct1Cleared ? 1 : 0,
                6 => save.realmIndex >= 1 ? 1 : 0,
                7 => save.activeAct >= 1 ? 1 : 0,
                8 => (save.activeMainFormation >= 0 && save.equippedGear.Count > 0) ? 1 : 0,
                9 => save.highestUnlockedLinearStage >= 20 ? 1 : 0,
                10 => save.realmIndex >= 2 ? 1 : 0,
                11 => save.highestUnlockedLinearStage >= 30 ? 1 : 0,
                _ => 0
            };
        }

        private void SetDaoVanLevel(string nodeId, int newLevel)
        {
            var list = SaveService.Current.daoVanNodes;
            for (int i = 0; i < list.Count; i++)
                if (list[i].nodeId == nodeId) { list[i].level = newLevel; return; }
            list.Add(new RemakeDaoVanSaveEntry { nodeId = nodeId, level = newLevel });
        }

        private static DaoVanNodeSpec FindNodeSpec(string id)
        {
            var all = RemakeCatalogs.StarterDaoVanNodes;
            for (int i = 0; i < all.Count; i++)
                if (all[i].id == id) return all[i];
            return null;
        }

        private bool RemoveOneSwordFromBag(int level)
        {
            var bag = SaveService.Current.swordInventoryLevels;
            for (int i = bag.Count - 1; i >= 0; i--)
                if (bag[i] == level) { bag.RemoveAt(i); return true; }
            return false;
        }

        private void AddPill(int realmTarget, RemakePillQuality q, int qty)
        {
            var list = SaveService.Current.pills;
            for (int i = 0; i < list.Count; i++)
                if (list[i].realmTarget == realmTarget && list[i].quality == q) { list[i].count += qty; return; }
            list.Add(new RemakePillStackEntry { realmTarget = realmTarget, quality = q, count = qty });
        }

        private bool ConsumePill(int realmTarget, RemakePillQuality q)
        {
            var list = SaveService.Current.pills;
            for (int i = 0; i < list.Count; i++)
                if (list[i].realmTarget == realmTarget && list[i].quality == q && list[i].count > 0) { list[i].count--; return true; }
            return false;
        }
    }
}

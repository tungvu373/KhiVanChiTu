using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// 5-Bus Software Audio Mixing Engine (100% WebGL & iOS Safari safe):
    /// Bus 1: BGM A/B Crossfade (preserves existing game BGM clips)
    /// Bus 2: Ambience A/B Crossfade (8 Scenes + Tribulation Storm)
    /// Bus 3: Gendered Voice Nam/Nu with Priority Queue + Auto-Ducking (Standard 40% / FullCinematic 8%)
    /// Bus 4: Combat & UI SFX Pool (6 sources, throttled anti-spam)
    /// Bus 5: Monster & Boss SFX Pool (4 sources, max 2 concurrent normal monsters)
    /// </summary>
    public sealed class RemakeAudioService : MonoBehaviour
    {
        public static RemakeAudioService Instance { get; private set; }

        [SerializeField] private RemakeAudioBankSO audioBank;

        private const float MaxBgmGain = 0.38f;
        private const float MaxAmbienceGain = 0.32f;
        private const float MaxVoiceGain = 0.95f;
        private const float MaxSfxGain = 0.78f;
        private const float MaxMonsterGain = 0.68f;
        private const float MonsterSfxRatio = 0.85f;

        private const float BgmCrossfadeDuration = 1.5f;
        private const float AmbCrossfadeDuration = 2.0f;

        // Bus 1: BGM A/B
        private AudioSource _bgmA;
        private AudioSource _bgmB;
        private bool _bgmUsingA = true;
        private float _bgmBlend = 1f;

        // Bus 2: Ambience A/B
        private AudioSource _ambA;
        private AudioSource _ambB;
        private bool _ambUsingA = true;
        private float _ambBlend = 1f;
        private RemakeAmbienceZone _baseAmbienceZone = RemakeAmbienceZone.None;
        private RemakeAmbienceZone _activeAmbienceZone = RemakeAmbienceZone.None;
        private bool _hasTempAmbience;

        // Bus 3: Gendered Voice + Auto-Ducking + ~1-Minute Idle Cultivation Voice Scheduler
        public const float IdleVoiceIntervalMinSec = 55f;
        public const float IdleVoiceIntervalMaxSec = 68f;
        private AudioSource _voiceSource;
        private int _activeVoicePriority = 99;
        private RemakeVoiceDuckMode _activeDuckMode = RemakeVoiceDuckMode.Standard;
        private float _bgmDuckFactor = 1f;
        private float _ambDuckFactor = 1f;
        private float _lastSkillVoiceTime = -100f;
        private float _lastSelectVoiceTime = -100f;
        private float _lastBossVoiceTime = -100f;
        private float _lastRespawnVoiceTime = -100f;
        private float _idleVoiceCountdown = 28f;
        private int _lastIdleVoiceIndex = -1;
        private bool _wasInGameForIdleVoice;

        // Bus 4: SFX Pool (6 sources)
        private readonly AudioSource[] _sfxPool = new AudioSource[6];
        private int _sfxCursor;
        private float _lastSwordSfxTime = -10f;

        // Bus 5: Monster Pool (4 sources)
        private readonly AudioSource[] _monsterPool = new AudioSource[4];
        private int _monsterCursor;
        private float _lastMobHurtTime = -10f;
        private float _lastMobDeathTime = -10f;
        private float _lastMobAttackTime = -10f;
        private float _lastMobSpawnTime = -10f;

        // Volume cache & WebGL/iOS Safari autoplay unlock
        private float _targetBgmVol;
        private float _targetAmbVol;
        private float _targetVoiceVol;
        private float _targetSfxVol;
        private float _targetMonsterVol;
        private bool _bgmMuted;
        private bool _ambMuted;
        private bool _voiceMuted;
        private bool _sfxMuted;
        private bool _audioGestureUnlocked;

        public RemakeAmbienceZone ActiveAmbienceZone => _activeAmbienceZone;
        public float CurrentBgmDuckFactor => _bgmDuckFactor;
        public float CurrentAmbienceDuckFactor => _ambDuckFactor;
        public int ActiveVoicePriority => _voiceSource != null && _voiceSource.isPlaying ? _activeVoicePriority : 99;

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
            if (audioBank == null)
                audioBank = Resources.Load<RemakeAudioBankSO>("RemakeAudioBank");

            _bgmA = CreateLoopSource();
            _bgmB = CreateLoopSource();
            _ambA = CreateLoopSource();
            _ambB = CreateLoopSource();

            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.loop = false;
            _voiceSource.playOnAwake = false;
            _voiceSource.spatialBlend = 0f;

            for (int i = 0; i < _sfxPool.Length; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.loop = false;
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                _sfxPool[i] = s;
            }

            for (int i = 0; i < _monsterPool.Length; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.loop = false;
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                _monsterPool[i] = s;
            }

            RefreshVolumes();
        }

        private AudioSource CreateLoopSource()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0f;
            return src;
        }

        private void Update()
        {
            float dt = Mathf.Min(0.1f, Time.unscaledDeltaTime);

            // WebGL / iOS Safari gesture unlock: resume loop buses cleanly on first user input
            if (!_audioGestureUnlocked && HasUserGestureThisFrame())
            {
                _audioGestureUnlocked = true;
                ResumePendingLoop(_bgmUsingA ? _bgmA : _bgmB, _bgmMuted);
                ResumePendingLoop(_ambUsingA ? _ambA : _ambB, _ambMuted);
            }

            // Voice status & Auto-Ducking envelope
            bool voicePlaying = _voiceSource != null && _voiceSource.isPlaying;
            if (!voicePlaying)
                _activeVoicePriority = 99;

            ComputeVoiceDuckTargets(voicePlaying, _activeDuckMode, out float desiredBgmDuck, out float desiredAmbDuck);
            float attackSpeed = _activeDuckMode == RemakeVoiceDuckMode.FullCinematic ? (1f / 0.25f) : (1f / 0.18f);
            float releaseSpeed = _activeDuckMode == RemakeVoiceDuckMode.FullCinematic ? (1f / 1.20f) : (1f / 0.90f);

            float bgmStep = (desiredBgmDuck < _bgmDuckFactor ? attackSpeed : releaseSpeed) * dt;
            float ambStep = (desiredAmbDuck < _ambDuckFactor ? attackSpeed : releaseSpeed) * dt;
            _bgmDuckFactor = Mathf.MoveTowards(_bgmDuckFactor, desiredBgmDuck, bgmStep);
            _ambDuckFactor = Mathf.MoveTowards(_ambDuckFactor, desiredAmbDuck, ambStep);

            // Bus 1 BGM A/B Crossfade
            if (_bgmBlend < 1f)
            {
                _bgmBlend = Mathf.MoveTowards(_bgmBlend, 1f, dt / BgmCrossfadeDuration);
                if (_bgmBlend >= 1f)
                {
                    var oldBgm = _bgmUsingA ? _bgmB : _bgmA;
                    if (oldBgm != null && oldBgm.isPlaying) oldBgm.Stop();
                }
            }
            ApplyCrossfadeVolumes(_bgmA, _bgmB, _bgmUsingA, _bgmBlend, _targetBgmVol * _bgmDuckFactor, _bgmMuted);

            // Bus 2 Ambience A/B Crossfade
            if (_ambBlend < 1f)
            {
                _ambBlend = Mathf.MoveTowards(_ambBlend, 1f, dt / AmbCrossfadeDuration);
                if (_ambBlend >= 1f)
                {
                    var oldAmb = _ambUsingA ? _ambB : _ambA;
                    if (oldAmb != null && oldAmb.isPlaying) oldAmb.Stop();
                }
            }
            ApplyCrossfadeVolumes(_ambA, _ambB, _ambUsingA, _ambBlend, _targetAmbVol * _ambDuckFactor, _ambMuted);

            // ~1-Minute Ambient Cultivation Voice Scheduler (P4 lowest priority, non-repeating shuffle)
            UpdateIdleVoiceScheduler(dt, voicePlaying);
        }

        private void UpdateIdleVoiceScheduler(float dt, bool voicePlaying)
        {
            bool inGame = SceneFlowController.Instance != null && SceneFlowController.Instance.IsInGame;
            if (!inGame)
            {
                _wasInGameForIdleVoice = false;
                return;
            }
            if (!_wasInGameForIdleVoice)
            {
                _wasInGameForIdleVoice = true;
                _idleVoiceCountdown = Random.Range(24f, 34f); // First ambient line ~30s after entering arena
                return;
            }

            if (_voiceMuted || voicePlaying) return;
            var save = SaveService.Current;
            if (save == null || !save.hasCreatedCharacter || save.graphics.afkDimMode) return;
            var eng = GameplayEngine.Instance;
            if (eng != null && (eng.IsInTribulation || eng.IsRespawning)) return;

            _idleVoiceCountdown -= dt;
            if (_idleVoiceCountdown <= 0f)
            {
                _idleVoiceCountdown = Random.Range(IdleVoiceIntervalMinSec, IdleVoiceIntervalMaxSec);
                RemakeVoiceLineId nextLine = PickNextIdleVoiceLine(_lastIdleVoiceIndex, Random.Range(0, 4), out int nextIdx);
                _lastIdleVoiceIndex = nextIdx;
                TryPlayVoice(nextLine, save.gender, true);
            }
        }

        public static RemakeVoiceLineId PickNextIdleVoiceLine(int lastIndex, int randomRoll, out int chosenIndex)
        {
            int idx = Mathf.Abs(randomRoll) % 4;
            if (idx == lastIndex)
                idx = (idx + 1) % 4;
            chosenIndex = idx;
            return idx switch
            {
                0 => RemakeVoiceLineId.IdleLine1,
                1 => RemakeVoiceLineId.IdleLine2,
                2 => RemakeVoiceLineId.IdleLine3,
                _ => RemakeVoiceLineId.IdleLine4
            };
        }

        private static bool HasUserGestureThisFrame()
        {
            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;
            return false;
        }

        private static void ResumePendingLoop(AudioSource active, bool muted)
        {
            if (active != null && !muted && active.clip != null && !active.isPlaying)
                active.Play();
        }

        private static void ApplyCrossfadeVolumes(AudioSource a, AudioSource b, bool usingA, float blend, float busVol, bool muted)
        {
            AudioSource active = usingA ? a : b;
            AudioSource fading = usingA ? b : a;
            if (active != null)
            {
                active.mute = muted;
                active.volume = muted ? 0f : busVol * blend;
            }
            if (fading != null)
            {
                fading.mute = muted;
                fading.volume = muted ? 0f : busVol * (1f - blend);
            }
        }

        // =========================================================
        // PURE STATIC HELPERS (UNIT-TESTABLE MIXING RULES)
        // =========================================================
        public static RemakeVoiceDuckMode ResolveDuckMode(RemakeVoiceLineId line)
        {
            return line switch
            {
                RemakeVoiceLineId.RealmTrucCo => RemakeVoiceDuckMode.FullCinematic,
                RemakeVoiceLineId.RealmKimDan => RemakeVoiceDuckMode.FullCinematic,
                RemakeVoiceLineId.RealmNguyenAnh => RemakeVoiceDuckMode.FullCinematic,
                RemakeVoiceLineId.RealmHoaThan => RemakeVoiceDuckMode.FullCinematic,
                RemakeVoiceLineId.RealmDaiThua => RemakeVoiceDuckMode.FullCinematic,
                _ => RemakeVoiceDuckMode.Standard
            };
        }

        public static int ResolveVoicePriority(RemakeVoiceLineId line)
        {
            return line switch
            {
                RemakeVoiceLineId.SelectPreview => 1,
                RemakeVoiceLineId.RealmTrucCo => 1,
                RemakeVoiceLineId.RealmKimDan => 1,
                RemakeVoiceLineId.RealmNguyenAnh => 1,
                RemakeVoiceLineId.RealmHoaThan => 1,
                RemakeVoiceLineId.RealmDaiThua => 1,
                RemakeVoiceLineId.BreakthroughFail => 1,
                RemakeVoiceLineId.TribulationStart => 2,
                RemakeVoiceLineId.BossEncounter => 2,
                RemakeVoiceLineId.PlayerRespawn => 2,
                RemakeVoiceLineId.IdleLine1 => 4,
                RemakeVoiceLineId.IdleLine2 => 4,
                RemakeVoiceLineId.IdleLine3 => 4,
                RemakeVoiceLineId.IdleLine4 => 4,
                _ => 3
            };
        }

        public static void ComputeVoiceDuckTargets(bool voicePlaying, RemakeVoiceDuckMode mode, out float bgmDuck, out float ambDuck)
        {
            if (!voicePlaying)
            {
                bgmDuck = 1f;
                ambDuck = 1f;
                return;
            }
            if (mode == RemakeVoiceDuckMode.FullCinematic)
            {
                // Google Flow voice clip has baked Xianxia music bed -> drop in-game BGM to 8% to avoid double-music clash
                bgmDuck = 0.08f;
                ambDuck = 0.25f;
            }
            else
            {
                bgmDuck = 0.40f;
                ambDuck = 0.55f;
            }
        }

        public static RemakeVoiceLineId? ResolveMajorRealmVoiceLine(int newRealmIndex)
        {
            return newRealmIndex switch
            {
                1 => RemakeVoiceLineId.RealmTrucCo,
                2 => RemakeVoiceLineId.RealmKimDan,
                3 => RemakeVoiceLineId.RealmNguyenAnh,
                4 => RemakeVoiceLineId.RealmHoaThan,
                7 => RemakeVoiceLineId.RealmDaiThua,
                8 => RemakeVoiceLineId.RealmDaiThua,
                _ => null
            };
        }

        // =========================================================
        // VOLUME & MUTE MANAGEMENT (5 SLIDERS)
        // =========================================================
        public void RefreshVolumes()
        {
            var gfx = SaveService.Current != null ? SaveService.Current.graphics : null;
            if (gfx == null) return;

            float master = Mathf.Clamp01(gfx.masterVolume);
            float bgm = Mathf.Clamp01(gfx.musicVolume) * master;
            float amb = Mathf.Clamp01(gfx.ambienceVolume) * master;
            float voice = Mathf.Clamp01(gfx.voiceVolume) * master;
            float sfx = Mathf.Clamp01(gfx.sfxVolume) * master;

            _targetBgmVol = bgm * MaxBgmGain;
            _targetAmbVol = amb * MaxAmbienceGain;
            _targetVoiceVol = voice * MaxVoiceGain;
            _targetSfxVol = sfx * MaxSfxGain;
            _targetMonsterVol = sfx * MonsterSfxRatio * MaxMonsterGain;

            _bgmMuted = !gfx.musicEnabled || bgm <= 0.001f;
            _ambMuted = !gfx.soundEnabled || amb <= 0.001f;
            _voiceMuted = !gfx.soundEnabled || voice <= 0.001f;
            _sfxMuted = !gfx.soundEnabled || sfx <= 0.001f;

            ApplyCrossfadeVolumes(_bgmA, _bgmB, _bgmUsingA, _bgmBlend, _targetBgmVol * _bgmDuckFactor, _bgmMuted);
            ApplyCrossfadeVolumes(_ambA, _ambB, _ambUsingA, _ambBlend, _targetAmbVol * _ambDuckFactor, _ambMuted);

            if (_voiceSource != null)
            {
                _voiceSource.mute = _voiceMuted;
                _voiceSource.volume = _targetVoiceVol;
            }
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                if (_sfxPool[i] != null)
                {
                    _sfxPool[i].mute = _sfxMuted;
                    _sfxPool[i].volume = _targetSfxVol;
                }
            }
            for (int i = 0; i < _monsterPool.Length; i++)
            {
                if (_monsterPool[i] != null)
                {
                    _monsterPool[i].mute = _sfxMuted;
                    _monsterPool[i].volume = _targetMonsterVol;
                }
            }
        }

        // =========================================================
        // BUS 1: BGM (PRESERVES EXISTING GAME BGM)
        // =========================================================
        public void PlayMenuMusic()
        {
            AudioClip clip = audioBank != null ? audioBank.menuBgm : null;
            if (clip == null) clip = AudioDropLoader.LoadBgmOverride("bgm_menu_daylight");
            CrossfadeBgm(clip);
        }

        public void PlayGameMusic()
        {
            AudioClip clip = audioBank != null ? audioBank.gameBgm : null;
            if (clip == null) clip = AudioDropLoader.LoadBgmOverride("bgm_act1_meadow");
            CrossfadeBgm(clip);
        }

        public void PlayBossMusic()
        {
            AudioClip clip = audioBank != null ? audioBank.bossBgm : null;
            if (clip == null) clip = AudioDropLoader.LoadBgmOverride("bgm_boss_challenge");
            if (clip == null && audioBank != null) clip = audioBank.gameBgm;
            CrossfadeBgm(clip);
        }

        private void CrossfadeBgm(AudioClip nextClip)
        {
            if (nextClip == null || _bgmA == null || _bgmB == null) return;
            AudioSource current = _bgmUsingA ? _bgmA : _bgmB;
            if (current.clip == nextClip && current.isPlaying) return;

            _bgmUsingA = !_bgmUsingA;
            AudioSource target = _bgmUsingA ? _bgmA : _bgmB;
            target.clip = nextClip;
            target.volume = 0f;
            target.mute = _bgmMuted;
            target.Play();
            _bgmBlend = current.isPlaying ? 0f : 1f;
        }

        // =========================================================
        // BUS 2: AMBIENCE (8 SCENES + 15s TRIBULATION STORM)
        // =========================================================
        public void SetAmbienceZone(RemakeAmbienceZone zone)
        {
            _baseAmbienceZone = zone;
            if (!_hasTempAmbience)
                ApplyAmbienceZoneInternal(zone);
        }

        public void PushTemporaryAmbience(RemakeAmbienceZone tempZone)
        {
            _hasTempAmbience = true;
            ApplyAmbienceZoneInternal(tempZone);
        }

        public void RestoreBaseAmbience()
        {
            if (!_hasTempAmbience) return;
            _hasTempAmbience = false;
            ApplyAmbienceZoneInternal(_baseAmbienceZone);
        }

        private void ApplyAmbienceZoneInternal(RemakeAmbienceZone zone)
        {
            if (_activeAmbienceZone == zone) return;
            _activeAmbienceZone = zone;
            if (_ambA == null || _ambB == null) return;

            AudioClip nextClip = AudioDropLoader.LoadAmbience(zone);
            if (nextClip == null)
            {
                _ambUsingA = !_ambUsingA;
                AudioSource emptyTarget = _ambUsingA ? _ambA : _ambB;
                emptyTarget.Stop();
                emptyTarget.clip = null;
                _ambBlend = 0f;
                return;
            }

            AudioSource current = _ambUsingA ? _ambA : _ambB;
            if (current.clip == nextClip && current.isPlaying) return;

            _ambUsingA = !_ambUsingA;
            AudioSource target = _ambUsingA ? _ambA : _ambB;
            target.clip = nextClip;
            target.volume = 0f;
            target.mute = _ambMuted;
            target.Play();
            _ambBlend = current.isPlaying ? 0f : 1f;
        }

        // =========================================================
        // BUS 3: GENDERED VOICE NAM / NU + PRIORITY + AUTO-DUCKING
        // =========================================================
        public bool TryPlayVoice(RemakeVoiceLineId line, RemakeCharacterGender gender, bool bypassCooldown = false)
        {
            if (_voiceSource == null || _voiceMuted) return false;

            float now = Time.unscaledTime;
            int priority = ResolveVoicePriority(line);

            if (!bypassCooldown)
            {
                if (line == RemakeVoiceLineId.SelectPreview && now - _lastSelectVoiceTime < 1.2f) return false;
                if ((line == RemakeVoiceLineId.SkillLoiPhat || line == RemakeVoiceLineId.SkillAmDuong ||
                     line == RemakeVoiceLineId.SkillVanKiem || line == RemakeVoiceLineId.SkillAnChuong) &&
                    now - _lastSkillVoiceTime < 16.0f) return false;
                if (line == RemakeVoiceLineId.BossEncounter && now - _lastBossVoiceTime < 45.0f) return false;
                if (line == RemakeVoiceLineId.PlayerRespawn && now - _lastRespawnVoiceTime < 20.0f) return false;
            }

            if (_voiceSource.isPlaying && priority > _activeVoicePriority)
                return false;

            AudioClip clip = AudioDropLoader.LoadVoice(line, gender);
            if (clip == null) return false;

            _activeVoicePriority = priority;
            _activeDuckMode = ResolveDuckMode(line);
            _voiceSource.Stop();
            _voiceSource.clip = clip;
            _voiceSource.pitch = 1f;
            _voiceSource.volume = _targetVoiceVol;
            _voiceSource.Play();

            if (line == RemakeVoiceLineId.SelectPreview) _lastSelectVoiceTime = now;
            else if (line == RemakeVoiceLineId.BossEncounter) _lastBossVoiceTime = now;
            else if (line == RemakeVoiceLineId.PlayerRespawn) _lastRespawnVoiceTime = now;
            else if (priority == 3) _lastSkillVoiceTime = now;

            // Reset ~1-minute idle voice timer whenever any voice speaks so lines stay comfortably spaced
            if (priority <= 3)
                _idleVoiceCountdown = Random.Range(IdleVoiceIntervalMinSec, IdleVoiceIntervalMaxSec);

            return true;
        }

        public bool TryPlaySkillVoice(int slot, RemakeCharacterGender gender)
        {
            RemakeVoiceLineId line = slot switch
            {
                0 => RemakeVoiceLineId.SkillLoiPhat,
                1 => RemakeVoiceLineId.SkillAmDuong,
                2 => RemakeVoiceLineId.SkillVanKiem,
                _ => RemakeVoiceLineId.SkillAnChuong
            };
            return TryPlayVoice(line, gender, false);
        }

        // =========================================================
        // BUS 4: COMBAT & UI SFX POOL (6 SOURCES)
        // =========================================================
        public void PlayClick() => PlaySfxOneShot(AudioDropLoader.LoadSfx("sfx_ui_click") ?? (audioBank != null ? audioBank.clickSfx : null), 0.50f, 1f);

        public void PlayMerge() => PlayMergeResult(true);

        public void PlayMergeResult(bool success)
        {
            if (success)
                PlaySfxOneShot(AudioDropLoader.LoadSfx("sfx_sword_merge_ok") ?? (audioBank != null ? audioBank.swordMergeSfx : null), 0.72f, 1.0f);
            else
                PlaySfxOneShot(AudioDropLoader.LoadSfx("sfx_sword_merge_fail") ?? (audioBank != null ? audioBank.clickSfx : null), 0.55f, 0.74f);
        }

        public void PlayLoiPhat() => PlaySkill(0);

        public void PlaySkill(int slot)
        {
            string key = slot switch
            {
                0 => "sfx_skill_loiphat",
                1 => "sfx_skill_amduong",
                2 => "sfx_skill_vankiem",
                _ => "sfx_skill_anchuong"
            };
            AudioClip custom = AudioDropLoader.LoadSfx(key);
            if (custom != null)
            {
                PlaySfxOneShot(custom, 0.78f, 1.0f);
                return;
            }
            float fallbackPitch = slot switch { 0 => 1.0f, 1 => 0.86f, 2 => 1.18f, _ => 0.78f };
            AudioClip fb = slot == 2
                ? (audioBank != null ? audioBank.swordMergeSfx : null)
                : (audioBank != null ? audioBank.loiPhatSfx : null);
            PlaySfxOneShot(fb, 0.72f, fallbackPitch);
        }

        public void PlayAlchemyStart()
        {
            AudioClip custom = AudioDropLoader.LoadSfx("sfx_alchemy_start");
            PlaySfxOneShot(custom ?? (audioBank != null ? audioBank.clickSfx : null), custom != null ? 0.65f : 0.45f, custom != null ? 1.0f : 0.88f);
        }

        public void PlayAlchemyOutcome(bool success)
        {
            if (success)
                PlaySfxOneShot(AudioDropLoader.LoadSfx("sfx_alchemy_ok") ?? (audioBank != null ? audioBank.swordMergeSfx : null), 0.72f, 1.06f);
            else
                PlaySfxOneShot(AudioDropLoader.LoadSfx("sfx_alchemy_fail") ?? (audioBank != null ? audioBank.loiPhatSfx : null), 0.58f, 0.68f);
        }

        public void PlayTribulationStrike(int strikeIndex, int totalStrikes)
        {
            float intensity = Mathf.Lerp(0.88f, 1.12f, Mathf.Clamp01((float)strikeIndex / Mathf.Max(1, totalStrikes)));
            AudioClip clip = AudioDropLoader.LoadSfx("sfx_tribulation_strike") ?? (audioBank != null ? audioBank.loiPhatSfx : null);
            PlaySfxOneShot(clip, 0.72f, intensity);
        }

        public void PlayLootRare()
        {
            AudioClip custom = AudioDropLoader.LoadSfx("sfx_loot_rare");
            if (custom != null)
                PlaySfxOneShot(custom, 0.68f, 1.0f);
            else
                PlaySfxOneShot(audioBank != null ? audioBank.swordMergeSfx : null, 0.36f, 1.24f);
        }

        public void PlayBreakthrough(bool isMajor = false)
        {
            AudioClip clip = AudioDropLoader.LoadSfx(isMajor ? "sfx_breakthrough_major" : "sfx_breakthrough")
                ?? (audioBank != null ? audioBank.breakthroughSfx : null);
            PlaySfxOneShot(clip, isMajor ? 0.88f : 0.78f, isMajor ? 1.0f : 1.06f);
        }

        public void PlaySwordHit()
        {
            if (_sfxMuted || Time.unscaledTime - _lastSwordSfxTime < 0.11f) return;
            _lastSwordSfxTime = Time.unscaledTime;
            AudioClip clip = AudioDropLoader.LoadSfx("sfx_sword_hit") ?? (audioBank != null ? audioBank.swordHitSfx : null);
            if (clip == null) return;
            PlaySfxOneShot(clip, 0.28f, Random.Range(0.92f, 1.08f));
        }

        private void PlaySfxOneShot(AudioClip clip, float relativeVolume, float pitch)
        {
            if (_sfxMuted || clip == null) return;
            var src = _sfxPool[_sfxCursor];
            _sfxCursor = (_sfxCursor + 1) % _sfxPool.Length;
            if (src == null) return;
            src.pitch = Mathf.Clamp(pitch, 0.5f, 1.8f);
            src.volume = _targetSfxVol;
            src.PlayOneShot(clip, Mathf.Clamp01(relativeVolume));
        }

        // =========================================================
        // BUS 5: MONSTER & BOSS SFX POOL (4 SOURCES, MAX 2 NORMAL)
        // =========================================================
        public void PlayMonsterSpawn(RemakeMonsterAudioFamily family, string monsterId = null)
        {
            if (_sfxMuted) return;
            bool isBoss = family == RemakeMonsterAudioFamily.Boss;
            if (!isBoss && Time.unscaledTime - _lastMobSpawnTime < 0.85f) return;
            if (!isBoss && CountActiveMonsterSources() >= 2) return;

            AudioClip clip = AudioDropLoader.LoadMonsterCue(family, isBoss ? "roar" : "spawn");
            if (clip == null) return;
            if (!isBoss) _lastMobSpawnTime = Time.unscaledTime;
            PlayMonsterClip(clip, isBoss ? 0.95f : 0.45f, isBoss ? 1.0f : Random.Range(0.94f, 1.06f));
        }

        public void PlayMonsterAttack(RemakeMonsterAudioFamily family)
        {
            if (_sfxMuted) return;
            bool isBoss = family == RemakeMonsterAudioFamily.Boss;
            if (!isBoss && Time.unscaledTime - _lastMobAttackTime < 0.45f) return;
            if (!isBoss && CountActiveMonsterSources() >= 2) return;

            AudioClip clip = AudioDropLoader.LoadMonsterCue(family, "attack");
            if (clip == null) return;
            if (!isBoss) _lastMobAttackTime = Time.unscaledTime;
            PlayMonsterClip(clip, isBoss ? 0.82f : 0.50f, Random.Range(0.93f, 1.07f));
        }

        public void PlayMonsterHurt(RemakeMonsterAudioFamily family)
        {
            if (_sfxMuted) return;
            bool isBoss = family == RemakeMonsterAudioFamily.Boss;
            if (Time.unscaledTime - _lastMobHurtTime < (isBoss ? 0.18f : 0.22f)) return;
            if (!isBoss && CountActiveMonsterSources() >= 2) return;
            _lastMobHurtTime = Time.unscaledTime;

            AudioClip clip = AudioDropLoader.LoadMonsterCue(family, "hurt");
            if (clip != null)
            {
                PlayMonsterClip(clip, isBoss ? 0.68f : 0.46f, Random.Range(0.92f, 1.09f));
                return;
            }
            if (family == RemakeMonsterAudioFamily.Armored && audioBank != null && audioBank.swordHitSfx != null)
                PlayMonsterClip(audioBank.swordHitSfx, 0.22f, Random.Range(1.22f, 1.36f));
        }

        public void PlayMonsterDeath(RemakeMonsterAudioFamily family)
        {
            if (_sfxMuted) return;
            bool isBoss = family == RemakeMonsterAudioFamily.Boss;
            if (!isBoss && Time.unscaledTime - _lastMobDeathTime < 0.35f) return;
            if (!isBoss && CountActiveMonsterSources() >= 2) return;
            _lastMobDeathTime = Time.unscaledTime;

            AudioClip clip = AudioDropLoader.LoadMonsterCue(family, "death");
            if (clip != null)
                PlayMonsterClip(clip, isBoss ? 0.95f : 0.56f, Random.Range(0.93f, 1.05f));
        }

        public void PlayBossShieldActivate()
        {
            if (_sfxMuted) return;
            AudioClip clip = AudioDropLoader.LoadSfx("sfx_boss_shield_on")
                ?? AudioDropLoader.LoadMonsterCue(RemakeMonsterAudioFamily.Boss, "shield")
                ?? (audioBank != null ? audioBank.loiPhatSfx : null);
            if (clip != null) PlayMonsterClip(clip, 0.78f, 0.78f);
        }

        public void PlayBossShieldBreak()
        {
            if (_sfxMuted) return;
            AudioClip clip = AudioDropLoader.LoadSfx("sfx_boss_shield_break")
                ?? (audioBank != null ? audioBank.breakthroughSfx : null);
            if (clip != null) PlayMonsterClip(clip, 0.90f, 1.08f);
        }

        private int CountActiveMonsterSources()
        {
            int active = 0;
            for (int i = 0; i < _monsterPool.Length; i++)
                if (_monsterPool[i] != null && _monsterPool[i].isPlaying) active++;
            return active;
        }

        private void PlayMonsterClip(AudioClip clip, float relativeVolume, float pitch)
        {
            var src = _monsterPool[_monsterCursor];
            _monsterCursor = (_monsterCursor + 1) % _monsterPool.Length;
            if (src == null) return;
            src.pitch = Mathf.Clamp(pitch, 0.6f, 1.6f);
            src.volume = _targetMonsterVol;
            src.PlayOneShot(clip, Mathf.Clamp01(relativeVolume));
        }
    }
}

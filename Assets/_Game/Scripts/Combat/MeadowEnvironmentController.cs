using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class MeadowEnvironmentController : MonoBehaviour
    {
        public static MeadowEnvironmentController Instance { get; private set; }

        public enum WorldThemePreset
        {
            Act1Meadow = 0,
            Act2Wasteland = 1,
            Act3JadePeaks = 2,
            DemonTower = 3,
            SecretSwordTomb = 4,
            SecretHerbGarden = 5,
            SecretOreCavern = 6
        }

        [Header("Scene References")]
        [SerializeField] private WorldThemePreset worldTheme = WorldThemePreset.Act1Meadow;
        [SerializeField] private Renderer[] grassChunkRenderers;
        [SerializeField] private Renderer[] dirtPatchRenderers;
        [SerializeField] private Renderer[] codexPropRenderers;
        [SerializeField] private Renderer skyDomeRenderer;
        [SerializeField] private Camera arenaCamera;

        public static bool IsRearCombatView { get; private set; }

        public const float DefaultRearYawOffsetDeg = 0f;
        public const float MinRearYawOffsetDeg = -58f;
        public const float MaxRearYawOffsetDeg = 58f;
        public const float DefaultRearPitchDeg = 22f;
        public const float MinRearPitchDeg = 16f;
        public const float MaxRearPitchDeg = 54f;
        public const float DefaultRearDistance = 6.2f;
        public const float MinRearDistance = 4.6f;
        public const float MaxRearDistance = 8.8f;
        public const float MinRearCameraHeightAbovePlayer = 1.85f;

        public float RearYawOffsetDeg => _targetRearYawDeg;
        public float RearPitchDeg => _targetRearPitchDeg;
        public float RearDistance => _targetRearDistance;

        private Vector3 _overviewPosition;
        private Quaternion _overviewRotation;
        private float _overviewFov;
        private float _rearFov;
        private float _cameraViewBlend;
        private float _targetRearYawDeg = DefaultRearYawOffsetDeg;
        private float _rearYawDeg = DefaultRearYawOffsetDeg;
        private float _targetRearPitchDeg = DefaultRearPitchDeg;
        private float _rearPitchDeg = DefaultRearPitchDeg;
        private float _targetRearDistance = DefaultRearDistance;
        private float _rearDistance = DefaultRearDistance;
        private int _lastScreenW;
        private int _lastScreenH;
        private float _trauma;
        private bool _stormActive;
        private float _stormBlend;
        private float _skyFlashTimer;
        private Color _skyFlashTint = Color.white;
        private Color _baseSkyColor;
        private Color _baseEqColor;
        private Color _baseGroundColor;
        private Color _baseFogColor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
            IsRearCombatView = false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (arenaCamera == null) arenaCamera = Camera.main;
            if (arenaCamera != null)
            {
                _overviewPosition = arenaCamera.transform.position;
                _overviewRotation = arenaCamera.transform.rotation;
            }
            _cameraViewBlend = IsRearCombatView ? 1f : 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            ApplyDaylightLighting(worldTheme);
            _baseSkyColor = RenderSettings.ambientSkyColor;
            _baseEqColor = RenderSettings.ambientEquatorColor;
            _baseGroundColor = RenderSettings.ambientGroundColor;
            _baseFogColor = RenderSettings.fogColor;
            TryBindCodexMapTextures();
            UpdateResponsiveCameraFov();
        }

        private void LateUpdate()
        {
            if (Screen.width != _lastScreenW || Screen.height != _lastScreenH)
                UpdateResponsiveCameraFov();
            float dt = Time.deltaTime;
            UpdateCameraPose(dt);

            // Zero-cost RenderSettings storm darkening & sharp lightning sky flash during 15s Tribulation
            float targetStorm = _stormActive ? 1f : 0f;
            if (!Mathf.Approximately(_stormBlend, targetStorm) || _skyFlashTimer > 0f)
            {
                _stormBlend = Mathf.MoveTowards(_stormBlend, targetStorm, dt * 1.8f);
                float flash = 0f;
                if (_skyFlashTimer > 0f)
                {
                    _skyFlashTimer = Mathf.Max(0f, _skyFlashTimer - dt);
                    flash = Mathf.Clamp01(_skyFlashTimer / 0.22f);
                    flash *= flash;
                }

                Color stormSky = new Color(0.22f, 0.24f, 0.36f, 1f);
                Color stormEq = new Color(0.28f, 0.32f, 0.42f, 1f);
                Color stormGnd = new Color(0.32f, 0.34f, 0.38f, 1f);
                Color stormFog = new Color(0.26f, 0.28f, 0.40f, 1f);

                Color sky = Color.Lerp(_baseSkyColor, stormSky, _stormBlend);
                Color eq = Color.Lerp(_baseEqColor, stormEq, _stormBlend);
                Color gnd = Color.Lerp(_baseGroundColor, stormGnd, _stormBlend);
                Color fog = Color.Lerp(_baseFogColor, stormFog, _stormBlend);

                if (flash > 0.001f)
                {
                    sky = Color.Lerp(sky, _skyFlashTint, flash * 0.85f);
                    eq = Color.Lerp(eq, _skyFlashTint, flash * 0.65f);
                    fog = Color.Lerp(fog, _skyFlashTint, flash * 0.70f);
                }

                RenderSettings.ambientSkyColor = sky;
                RenderSettings.ambientEquatorColor = eq;
                RenderSettings.ambientGroundColor = gnd;
                RenderSettings.fogColor = fog;
            }
        }

        public bool TryToggleCombatCamera()
        {
            if (arenaCamera == null || RemakeArenaController.Instance == null ||
                SceneFlowController.Instance?.IsTransitioning == true)
                return false;

            IsRearCombatView = !IsRearCombatView;
            ResetRearCameraOrbit();
            return true;
        }

        public void ResetRearCameraOrbit()
        {
            _targetRearYawDeg = DefaultRearYawOffsetDeg;
            _targetRearPitchDeg = DefaultRearPitchDeg;
            _targetRearDistance = DefaultRearDistance;
            if (!IsRearCombatView)
            {
                _rearYawDeg = DefaultRearYawOffsetDeg;
                _rearPitchDeg = DefaultRearPitchDeg;
                _rearDistance = DefaultRearDistance;
            }
        }

        /// <summary>
        /// Orbits the rear combat camera smoothly behind the character's back.
        /// Clamps yaw strictly within [-58, +58] deg (never wraps around to front face)
        /// and clamps pitch within [16, 54] deg (never looks upward from underneath/below ground).
        /// </summary>
        public void ApplyRearCameraDrag(float deltaScreenX, float deltaScreenY)
        {
            if (!IsRearCombatView) return;
            _targetRearYawDeg = Mathf.Clamp(_targetRearYawDeg + deltaScreenX * 0.24f, MinRearYawOffsetDeg, MaxRearYawOffsetDeg);
            _targetRearPitchDeg = Mathf.Clamp(_targetRearPitchDeg + deltaScreenY * 0.18f, MinRearPitchDeg, MaxRearPitchDeg);
        }

        public void ApplyRearCameraZoom(float wheelDeltaY)
        {
            if (!IsRearCombatView) return;
            _targetRearDistance = Mathf.Clamp(_targetRearDistance + wheelDeltaY * 0.35f, MinRearDistance, MaxRearDistance);
        }

        public static void ComputeRearCameraPose(
            Vector3 playerPos,
            Vector3 playerFacingDir,
            float yawOffsetDeg,
            float pitchDeg,
            float distance,
            out Vector3 cameraPos,
            out Quaternion cameraRot)
        {
            Vector3 forward = Vector3.ProjectOnPlane(playerFacingDir, Vector3.up);
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

            float clampedYaw = Mathf.Clamp(yawOffsetDeg, MinRearYawOffsetDeg, MaxRearYawOffsetDeg);
            float clampedPitch = Mathf.Clamp(pitchDeg, MinRearPitchDeg, MaxRearPitchDeg);
            float clampedDist = Mathf.Clamp(distance, MinRearDistance, MaxRearDistance);

            Vector3 rearHorizDir = Quaternion.AngleAxis(clampedYaw, Vector3.up) * (-forward);
            float pitchRad = clampedPitch * Mathf.Deg2Rad;
            Vector3 pivot = playerPos + Vector3.up * 1.15f;
            cameraPos = pivot
                + rearHorizDir * (Mathf.Cos(pitchRad) * clampedDist)
                + Vector3.up * (Mathf.Sin(pitchRad) * clampedDist);
            cameraPos.y = Mathf.Max(cameraPos.y, playerPos.y + MinRearCameraHeightAbovePlayer);

            Vector3 orbitForward = -rearHorizDir;
            Vector3 focus = playerPos + Vector3.up * 1.05f + orbitForward * 2.15f;
            cameraRot = Quaternion.LookRotation(focus - cameraPos, Vector3.up);
        }

        private void UpdateCameraPose(float dt)
        {
            if (arenaCamera == null) return;

            float unscaledDt = Time.unscaledDeltaTime;
            _cameraViewBlend = Mathf.MoveTowards(_cameraViewBlend, IsRearCombatView ? 1f : 0f, unscaledDt * 3.5f);
            float blend = Mathf.SmoothStep(0f, 1f, _cameraViewBlend);

            float orbitLerp = 1f - Mathf.Exp(-14f * unscaledDt);
            _rearYawDeg = Mathf.Lerp(_rearYawDeg, _targetRearYawDeg, orbitLerp);
            _rearPitchDeg = Mathf.Lerp(_rearPitchDeg, _targetRearPitchDeg, orbitLerp);
            _rearDistance = Mathf.Lerp(_rearDistance, _targetRearDistance, orbitLerp);

            Vector3 position = _overviewPosition;
            Quaternion rotation = _overviewRotation;
            var arena = RemakeArenaController.Instance;

            if (blend > 0f && arena != null)
            {
                Vector3 player = arena.PlayerWorldPosition;
                ComputeRearCameraPose(player, arena.PlayerFacingDirection, _rearYawDeg, _rearPitchDeg, _rearDistance, out Vector3 rearPosition, out Quaternion rearRotation);

                if (blend < 1f)
                {
                    position = Vector3.Lerp(_overviewPosition, rearPosition, blend);
                    rotation = Quaternion.Slerp(_overviewRotation, rearRotation, blend);
                }
                else
                {
                    position = rearPosition;
                    rotation = rearRotation;
                }
            }

            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - dt * 1.55f);
                float shake = _trauma * _trauma;
                float t = Time.unscaledTime * 34f;
                position += new Vector3(Mathf.Sin(t * 1.7f) * 0.24f * shake,
                    Mathf.Cos(t * 2.3f) * 0.18f * shake, 0f);
            }

            if (arenaCamera.transform.position != position || arenaCamera.transform.rotation != rotation)
                arenaCamera.transform.SetPositionAndRotation(position, rotation);

            float fov = Mathf.Lerp(_overviewFov, _rearFov, blend);
            if (!Mathf.Approximately(arenaCamera.fieldOfView, fov))
                arenaCamera.fieldOfView = fov;
        }

        public void AddCameraTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        public void SetTribulationStormMode(bool active)
        {
            _stormActive = active;
        }

        public void TriggerTribulationSkyFlash(float intensity, Color flashTint)
        {
            _skyFlashTimer = Mathf.Lerp(0.16f, 0.28f, Mathf.Clamp01(intensity));
            _skyFlashTint = Color.Lerp(Color.white, flashTint, 0.38f);
        }

        private void UpdateResponsiveCameraFov()
        {
            _lastScreenW = Screen.width;
            _lastScreenH = Screen.height;
            if (arenaCamera == null || _lastScreenH <= 0) return;
            float aspect = _lastScreenW / (float)_lastScreenH;
            // Landscape PC/Laptop gets slightly tighter FOV so cultivator & 8 swords fill frame cleanly; Portrait gets wider FOV for side clearance
            _overviewFov = aspect > 1.25f ? 44f : aspect < 0.75f ? 52f : 48f;
            _rearFov = aspect > 1.25f ? 51f : aspect < 0.75f ? 63f : 57f;
        }

        public static void ApplyDaylightLighting(WorldThemePreset theme = WorldThemePreset.Act1Meadow)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            switch (theme)
            {
                case WorldThemePreset.Act2Wasteland:
                    RenderSettings.ambientSkyColor = new Color(0.95f, 0.90f, 0.72f, 1f);
                    RenderSettings.ambientEquatorColor = new Color(0.96f, 0.84f, 0.56f, 1f);
                    RenderSettings.ambientGroundColor = new Color(0.86f, 0.68f, 0.42f, 1f);
                    RenderSettings.fogColor = new Color(0.98f, 0.91f, 0.76f, 1f);
                    break;
                case WorldThemePreset.Act3JadePeaks:
                    RenderSettings.ambientSkyColor = new Color(0.62f, 0.88f, 1.00f, 1f);
                    RenderSettings.ambientEquatorColor = new Color(0.75f, 0.94f, 0.90f, 1f);
                    RenderSettings.ambientGroundColor = new Color(0.76f, 0.86f, 0.84f, 1f);
                    RenderSettings.fogColor = new Color(0.82f, 0.95f, 1.00f, 1f);
                    break;
                case WorldThemePreset.DemonTower:
                    // Floor-tier sunlit celestial pagoda mood (shifts gently every 20 floors)
                    int floor = GameplayEngine.Instance != null ? GameplayEngine.Instance.ActiveChallengeFloor : 1;
                    int tier = Mathf.Clamp((floor - 1) / 20, 0, 3);
                    if (tier == 0)
                    {
                        RenderSettings.ambientSkyColor = new Color(0.74f, 0.88f, 0.96f, 1f);
                        RenderSettings.ambientEquatorColor = new Color(0.90f, 0.86f, 0.74f, 1f);
                        RenderSettings.ambientGroundColor = new Color(0.78f, 0.76f, 0.70f, 1f);
                        RenderSettings.fogColor = new Color(0.88f, 0.92f, 0.95f, 1f);
                    }
                    else if (tier == 1)
                    {
                        RenderSettings.ambientSkyColor = new Color(0.96f, 0.86f, 0.66f, 1f);
                        RenderSettings.ambientEquatorColor = new Color(0.96f, 0.84f, 0.60f, 1f);
                        RenderSettings.ambientGroundColor = new Color(0.84f, 0.76f, 0.62f, 1f);
                        RenderSettings.fogColor = new Color(0.96f, 0.90f, 0.78f, 1f);
                    }
                    else
                    {
                        RenderSettings.ambientSkyColor = new Color(0.82f, 0.78f, 0.98f, 1f);
                        RenderSettings.ambientEquatorColor = new Color(0.92f, 0.84f, 0.96f, 1f);
                        RenderSettings.ambientGroundColor = new Color(0.80f, 0.78f, 0.86f, 1f);
                        RenderSettings.fogColor = new Color(0.90f, 0.88f, 0.98f, 1f);
                    }
                    break;
                case WorldThemePreset.SecretSwordTomb:
                    RenderSettings.ambientSkyColor = new Color(0.76f, 0.90f, 0.96f, 1f);
                    RenderSettings.ambientEquatorColor = new Color(0.92f, 0.89f, 0.76f, 1f);
                    RenderSettings.ambientGroundColor = new Color(0.74f, 0.80f, 0.78f, 1f);
                    RenderSettings.fogColor = new Color(0.88f, 0.94f, 0.94f, 1f);
                    break;
                case WorldThemePreset.SecretHerbGarden:
                    RenderSettings.ambientSkyColor = new Color(0.60f, 0.90f, 0.98f, 1f);
                    RenderSettings.ambientEquatorColor = new Color(0.68f, 0.95f, 0.76f, 1f);
                    RenderSettings.ambientGroundColor = new Color(0.76f, 0.88f, 0.72f, 1f);
                    RenderSettings.fogColor = new Color(0.80f, 0.96f, 0.90f, 1f);
                    break;
                case WorldThemePreset.SecretOreCavern:
                    RenderSettings.ambientSkyColor = new Color(0.72f, 0.92f, 1.00f, 1f);
                    RenderSettings.ambientEquatorColor = new Color(0.92f, 0.86f, 0.72f, 1f);
                    RenderSettings.ambientGroundColor = new Color(0.78f, 0.80f, 0.82f, 1f);
                    RenderSettings.fogColor = new Color(0.84f, 0.93f, 0.98f, 1f);
                    break;
                default:
                    RenderSettings.ambientSkyColor = new Color(0.53f, 0.81f, 0.98f, 1f);     // #87CEFA
                    RenderSettings.ambientEquatorColor = new Color(0.66f, 0.90f, 0.64f, 1f); // #A8E6A3
                    RenderSettings.ambientGroundColor = new Color(0.82f, 0.76f, 0.60f, 1f);  // #D2C19A
                    RenderSettings.fogColor = new Color(0.75f, 0.90f, 1.00f, 1f);            // #BFE6FF
                    break;
            }
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 26f;
            RenderSettings.fogEndDistance = 76f;
        }

        private void TryBindCodexMapTextures()
        {
            Texture2D groundTex = worldTheme switch
            {
                WorldThemePreset.Act2Wasteland => Resources.Load<Texture2D>("BakedMaps/tex_act2_sand_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_act2_sand_bright"),
                WorldThemePreset.Act3JadePeaks => Resources.Load<Texture2D>("BakedMaps/tex_act3_jade_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_act3_jade_cliff"),
                WorldThemePreset.DemonTower => Resources.Load<Texture2D>("BakedMaps/tex_tower_floor_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_tower_floor_albedo"),
                WorldThemePreset.SecretSwordTomb => Resources.Load<Texture2D>("BakedMaps/tex_secret_sword_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_sword_floor"),
                WorldThemePreset.SecretHerbGarden => Resources.Load<Texture2D>("BakedMaps/tex_secret_herb_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_herb_floor"),
                WorldThemePreset.SecretOreCavern => Resources.Load<Texture2D>("BakedMaps/tex_secret_ore_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_ore_floor"),
                _ => Resources.Load<Texture2D>("BakedMaps/tex_grass_meadow_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_grass_meadow_albedo")
            };
            if (groundTex != null && grassChunkRenderers != null)
            {
                for (int i = 0; i < grassChunkRenderers.Length; i++)
                {
                    if (grassChunkRenderers[i] != null && grassChunkRenderers[i].sharedMaterial != null)
                        grassChunkRenderers[i].sharedMaterial.mainTexture = groundTex;
                }
            }

            Texture2D dirtTex = CodexArtLoader.LoadCodexTexture("Map/tex_dirt_patch_albedo");
            if (dirtTex != null && dirtPatchRenderers != null)
            {
                for (int i = 0; i < dirtPatchRenderers.Length; i++)
                {
                    if (dirtPatchRenderers[i] != null && dirtPatchRenderers[i].sharedMaterial != null)
                        dirtPatchRenderers[i].sharedMaterial.mainTexture = dirtTex;
                }
            }

            Texture2D skyTex = worldTheme switch
            {
                WorldThemePreset.DemonTower => Resources.Load<Texture2D>("BakedMaps/tex_tower_sky_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_tower_sky_panorama"),
                WorldThemePreset.SecretSwordTomb => Resources.Load<Texture2D>("BakedMaps/tex_secret_sword_sky_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_sword_sky"),
                WorldThemePreset.SecretHerbGarden => Resources.Load<Texture2D>("BakedMaps/tex_secret_herb_sky_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_herb_sky"),
                WorldThemePreset.SecretOreCavern => Resources.Load<Texture2D>("BakedMaps/tex_secret_ore_sky_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_secret_ore_sky"),
                _ => null
            };
            if (skyTex == null)
                skyTex = Resources.Load<Texture2D>("BakedMaps/tex_sky_daylight_seamless")
                    ?? CodexArtLoader.LoadCodexTexture("Map/tex_sky_daylight_panorama");
            if (skyTex != null && skyDomeRenderer != null && skyDomeRenderer.sharedMaterial != null)
            {
                skyDomeRenderer.sharedMaterial.mainTexture = skyTex;
            }

            // Auto-upgrade perimeter billboard props as soon as Codex drops Group I prop PNGs!
            string propKey = worldTheme switch
            {
                WorldThemePreset.DemonTower => "Map/prop_tower_pillar",
                WorldThemePreset.SecretSwordTomb => "Map/prop_giant_ancient_sword",
                WorldThemePreset.SecretHerbGarden => "Map/prop_spirit_herb_cluster",
                WorldThemePreset.SecretOreCavern => "Map/prop_crystal_vein",
                _ => null
            };
            if (!string.IsNullOrEmpty(propKey) && codexPropRenderers != null && codexPropRenderers.Length > 0)
            {
                Texture2D propTex = CodexArtLoader.LoadCodexTexture(propKey);
                if (propTex != null)
                {
                    for (int i = 0; i < codexPropRenderers.Length; i++)
                    {
                        if (codexPropRenderers[i] != null && codexPropRenderers[i].sharedMaterial != null)
                        {
                            codexPropRenderers[i].sharedMaterial.mainTexture = propTex;
                            codexPropRenderers[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}

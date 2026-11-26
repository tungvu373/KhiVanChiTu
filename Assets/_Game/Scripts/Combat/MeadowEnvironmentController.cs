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
            Act3JadePeaks = 2
        }

        [Header("Scene References")]
        [SerializeField] private WorldThemePreset worldTheme = WorldThemePreset.Act1Meadow;
        [SerializeField] private Renderer[] grassChunkRenderers;
        [SerializeField] private Renderer[] dirtPatchRenderers;
        [SerializeField] private Renderer skyDomeRenderer;
        [SerializeField] private Camera arenaCamera;

        private Vector3 _baseCamLocalPos;
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
            if (arenaCamera != null) _baseCamLocalPos = arenaCamera.transform.localPosition;
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
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (arenaCamera != null)
            {
                if (_trauma > 0f)
                {
                    _trauma = Mathf.Max(0f, _trauma - dt * 1.55f);
                    float shake = _trauma * _trauma;
                    float t = Time.unscaledTime * 34f;
                    arenaCamera.transform.localPosition = _baseCamLocalPos + new Vector3(
                        Mathf.Sin(t * 1.7f) * 0.24f * shake,
                        Mathf.Cos(t * 2.3f) * 0.18f * shake,
                        0f);
                }
                else if (arenaCamera.transform.localPosition != _baseCamLocalPos)
                {
                    arenaCamera.transform.localPosition = _baseCamLocalPos;
                }
            }

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

        public static void ApplyDaylightLighting(WorldThemePreset theme = WorldThemePreset.Act1Meadow)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            if (theme == WorldThemePreset.Act2Wasteland)
            {
                RenderSettings.ambientSkyColor = new Color(0.95f, 0.90f, 0.72f, 1f);
                RenderSettings.ambientEquatorColor = new Color(0.96f, 0.84f, 0.56f, 1f);
                RenderSettings.ambientGroundColor = new Color(0.86f, 0.68f, 0.42f, 1f);
                RenderSettings.fogColor = new Color(0.98f, 0.91f, 0.76f, 1f);
            }
            else if (theme == WorldThemePreset.Act3JadePeaks)
            {
                RenderSettings.ambientSkyColor = new Color(0.62f, 0.88f, 1.00f, 1f);
                RenderSettings.ambientEquatorColor = new Color(0.75f, 0.94f, 0.90f, 1f);
                RenderSettings.ambientGroundColor = new Color(0.76f, 0.86f, 0.84f, 1f);
                RenderSettings.fogColor = new Color(0.82f, 0.95f, 1.00f, 1f);
            }
            else
            {
                RenderSettings.ambientSkyColor = new Color(0.53f, 0.81f, 0.98f, 1f);     // #87CEFA
                RenderSettings.ambientEquatorColor = new Color(0.66f, 0.90f, 0.64f, 1f); // #A8E6A3
                RenderSettings.ambientGroundColor = new Color(0.82f, 0.76f, 0.60f, 1f);  // #D2C19A
                RenderSettings.fogColor = new Color(0.75f, 0.90f, 1.00f, 1f);            // #BFE6FF
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

            Texture2D skyTex = Resources.Load<Texture2D>("BakedMaps/tex_sky_daylight_seamless")
                ?? CodexArtLoader.LoadCodexTexture("Map/tex_sky_daylight_panorama");
            if (skyTex != null && skyDomeRenderer != null && skyDomeRenderer.sharedMaterial != null)
            {
                skyDomeRenderer.sharedMaterial.mainTexture = skyTex;
            }
        }
    }
}

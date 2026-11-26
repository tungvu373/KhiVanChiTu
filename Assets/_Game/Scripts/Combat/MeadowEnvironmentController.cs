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
            TryBindCodexMapTextures();
        }

        private void LateUpdate()
        {
            if (arenaCamera == null) return;
            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - Time.deltaTime * 1.6f);
                float shake = _trauma * _trauma;
                float t = Time.unscaledTime * 28f;
                arenaCamera.transform.localPosition = _baseCamLocalPos + new Vector3(
                    Mathf.Sin(t * 1.7f) * 0.14f * shake,
                    Mathf.Cos(t * 2.3f) * 0.10f * shake,
                    0f);
            }
            else if (arenaCamera.transform.localPosition != _baseCamLocalPos)
            {
                arenaCamera.transform.localPosition = _baseCamLocalPos;
            }
        }

        public void AddCameraTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
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

using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public enum RemakeLootOrbKind
    {
        TuViGold = 0,
        LinhThachCyan = 1,
        HerbGreen = 2,
        SwordEmbryoBeam = 3,
        EquipmentRarityBeam = 4
    }

    /// <summary>
    /// Object-pooled drop orbs + rarity light beams (zero physics, zero shadow casting, safe for WebGL 60+ FPS).
    /// </summary>
    public sealed class RemakeLootOrbPool : MonoBehaviour
    {
        public static RemakeLootOrbPool Instance { get; private set; }

        private struct OrbSlot
        {
            public GameObject root;
            public MeshRenderer orbRenderer;
            public MeshRenderer beamRenderer;
            public MaterialPropertyBlock orbMpb;
            public MaterialPropertyBlock beamMpb;
            public Vector3 startPos;
            public Vector3 scatterPos;
            public float elapsed;
            public float bounceDuration;
            public float totalDuration;
            public float baseScale;
            public Color color;
            public bool hasBeam;
            public bool active;
        }

        private const int PoolSize = 28;
        private readonly OrbSlot[] _pool = new OrbSlot[PoolSize];
        private int _cursor;
        private Material _unlitOrbMat;
        private Material _beamMat;
        private Texture2D _softGlowTex;
        private Texture2D _beamGradTex;
        private static readonly int ColorPropId = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() => Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializePool();
        }

        private void OnDestroy()
        {
            if (_unlitOrbMat != null) Destroy(_unlitOrbMat);
            if (_beamMat != null) Destroy(_beamMat);
            if (_softGlowTex != null) Destroy(_softGlowTex);
            if (_beamGradTex != null) Destroy(_beamGradTex);
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            Vector3 playerPos = RemakeArenaController.Instance != null
                ? RemakeArenaController.Instance.PlayerWorldPosition + Vector3.up * 1.15f
                : Vector3.up * 1.15f;
            float dt = Time.deltaTime;
            Camera cam = Camera.main;

            for (int i = 0; i < PoolSize; i++)
            {
                if (!_pool[i].active) continue;
                ref OrbSlot slot = ref _pool[i];
                slot.elapsed += dt;

                if (slot.elapsed >= slot.totalDuration)
                {
                    slot.active = false;
                    slot.root.SetActive(false);
                    continue;
                }

                if (slot.elapsed < slot.bounceDuration)
                {
                    float u = slot.elapsed / slot.bounceDuration;
                    float arc = Mathf.Sin(u * Mathf.PI) * (slot.hasBeam ? 1.15f : 0.72f);
                    Vector3 pos = Vector3.Lerp(slot.startPos, slot.scatterPos, u);
                    pos.y = Mathf.Max(0.18f, arc + 0.18f);
                    slot.root.transform.position = pos;
                    slot.root.transform.localScale = Vector3.one * (slot.baseScale * (0.85f + 0.25f * Mathf.Sin(u * Mathf.PI)));
                }
                else
                {
                    float u = (slot.elapsed - slot.bounceDuration) / Mathf.Max(0.1f, slot.totalDuration - slot.bounceDuration);
                    float ease = u * u * u;
                    Vector3 arcMid = (slot.scatterPos + playerPos) * 0.5f + Vector3.up * 1.45f;
                    Vector3 p1 = Vector3.Lerp(slot.scatterPos + Vector3.up * 0.22f, arcMid, ease);
                    Vector3 p2 = Vector3.Lerp(arcMid, playerPos, ease);
                    slot.root.transform.position = Vector3.Lerp(p1, p2, ease);
                    float sc = slot.baseScale * Mathf.Lerp(1.1f, 0.25f, ease);
                    slot.root.transform.localScale = Vector3.one * sc;

                    if (slot.hasBeam && slot.beamRenderer != null)
                        slot.beamRenderer.gameObject.SetActive(u < 0.55f);
                }

                if (cam != null && slot.orbRenderer != null)
                    slot.orbRenderer.transform.rotation = cam.transform.rotation;
            }
        }

        public void SpawnMonsterDrops(Vector3 worldPos, bool isBoss, bool droppedHerb, bool droppedSword, RemakeRarity? equipRarity)
        {
            bool minimal = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal;
            int tuViOrbs = minimal ? 1 : (isBoss ? 4 : 2);
            int ltOrbs = minimal ? 1 : (isBoss ? 4 : 1);

            for (int i = 0; i < tuViOrbs; i++)
                SpawnOrb(worldPos, RemakeLootOrbKind.TuViGold, Color.white);
            for (int i = 0; i < ltOrbs; i++)
                SpawnOrb(worldPos, RemakeLootOrbKind.LinhThachCyan, Color.white);
            if (droppedHerb)
                SpawnOrb(worldPos, RemakeLootOrbKind.HerbGreen, Color.white);
            if (droppedSword)
                SpawnOrb(worldPos, RemakeLootOrbKind.SwordEmbryoBeam, new Color(1.0f, 0.72f, 0.18f, 1f));
            if (equipRarity.HasValue && (!minimal || equipRarity.Value >= RemakeRarity.Linh))
                SpawnOrb(worldPos, RemakeLootOrbKind.EquipmentRarityBeam, GetRarityColor(equipRarity.Value));
        }

        public void SpawnOrb(Vector3 origin, RemakeLootOrbKind kind, Color customTint)
        {
            ref OrbSlot slot = ref _pool[_cursor];
            _cursor = (_cursor + 1) % PoolSize;

            Color c = kind switch
            {
                RemakeLootOrbKind.TuViGold => new Color(1.0f, 0.84f, 0.25f, 1f),
                RemakeLootOrbKind.LinhThachCyan => new Color(0.26f, 0.90f, 0.74f, 1f),
                RemakeLootOrbKind.HerbGreen => new Color(0.40f, 0.92f, 0.36f, 1f),
                RemakeLootOrbKind.SwordEmbryoBeam => new Color(1.0f, 0.65f, 0.16f, 1f),
                _ => customTint
            };

            bool beam = kind == RemakeLootOrbKind.SwordEmbryoBeam || kind == RemakeLootOrbKind.EquipmentRarityBeam;
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = beam ? UnityEngine.Random.Range(0.55f, 1.05f) : UnityEngine.Random.Range(0.75f, 1.55f);

            slot.startPos = new Vector3(origin.x, 0.35f, origin.z);
            slot.scatterPos = new Vector3(origin.x + Mathf.Cos(angle) * radius, 0f, origin.z + Mathf.Sin(angle) * radius);
            slot.elapsed = 0f;
            slot.bounceDuration = beam ? 0.58f : 0.34f;
            slot.totalDuration = beam ? 1.15f : 0.78f;
            slot.baseScale = beam ? 0.42f : 0.28f;
            slot.color = c;
            slot.hasBeam = beam;
            slot.active = true;

            slot.orbMpb.SetColor(ColorPropId, c);
            slot.orbRenderer.SetPropertyBlock(slot.orbMpb);
            if (slot.beamRenderer != null)
            {
                slot.beamRenderer.gameObject.SetActive(beam);
                if (beam)
                {
                    slot.beamMpb.SetColor(ColorPropId, new Color(c.r, c.g, c.b, 0.72f));
                    slot.beamRenderer.SetPropertyBlock(slot.beamMpb);
                }
            }

            slot.root.transform.position = slot.startPos;
            slot.root.transform.localScale = Vector3.one * slot.baseScale;
            slot.root.SetActive(true);
        }

        private static Color GetRarityColor(RemakeRarity r) => r switch
        {
            RemakeRarity.Linh => new Color(0.28f, 0.88f, 0.48f, 1f),
            RemakeRarity.Huyen => new Color(0.28f, 0.68f, 1.0f, 1f),
            RemakeRarity.Dia => new Color(0.76f, 0.42f, 0.98f, 1f),
            RemakeRarity.Thien => new Color(1.0f, 0.74f, 0.16f, 1f),
            _ => new Color(0.92f, 0.95f, 0.92f, 1f)
        };

        private void InitializePool()
        {
            _softGlowTex = CreateGlowTex();
            _beamGradTex = CreateBeamTex();
            Shader unlit = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            _unlitOrbMat = new Material(unlit) { mainTexture = _softGlowTex, color = Color.white };
            _beamMat = new Material(unlit) { mainTexture = _beamGradTex, color = Color.white };
            ConfigureTransparentBlend(_unlitOrbMat);
            ConfigureTransparentBlend(_beamMat);

            for (int i = 0; i < PoolSize; i++)
            {
                var root = new GameObject($"LootOrb_{i}");
                root.transform.SetParent(transform, false);

                var orbGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                orbGo.name = "OrbGlow";
                Destroy(orbGo.GetComponent<Collider>());
                orbGo.transform.SetParent(root.transform, false);
                var orbMr = orbGo.GetComponent<MeshRenderer>();
                orbMr.sharedMaterial = _unlitOrbMat;
                orbMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                orbMr.receiveShadows = false;

                var beamGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                beamGo.name = "LootBeam";
                Destroy(beamGo.GetComponent<Collider>());
                beamGo.transform.SetParent(root.transform, false);
                beamGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                beamGo.transform.localScale = new Vector3(0.55f, 4.6f, 1f);
                var beamMr = beamGo.GetComponent<MeshRenderer>();
                beamMr.sharedMaterial = _beamMat;
                beamMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                beamMr.receiveShadows = false;
                beamGo.SetActive(false);

                root.SetActive(false);
                _pool[i] = new OrbSlot
                {
                    root = root,
                    orbRenderer = orbMr,
                    beamRenderer = beamMr,
                    orbMpb = new MaterialPropertyBlock(),
                    beamMpb = new MaterialPropertyBlock()
                };
            }
        }

        private static void ConfigureTransparentBlend(Material mat)
        {
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3020;
        }

        private static Texture2D CreateGlowTex()
        {
            const int sz = 32;
            var t = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    float dx = (x + 0.5f) / sz * 2f - 1f;
                    float dy = (y + 0.5f) / sz * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    t.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
                }
            t.Apply();
            return t;
        }

        private static Texture2D CreateBeamTex()
        {
            const int w = 16, h = 64;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                float vy = y / (float)(h - 1);
                float verticalFade = Mathf.Sin(Mathf.Clamp01(vy) * Mathf.PI * 0.85f) * (1f - vy * 0.85f);
                for (int x = 0; x < w; x++)
                {
                    float hx = Mathf.Abs((x + 0.5f) / w * 2f - 1f);
                    float a = Mathf.Clamp01((1f - hx) * (1f - hx)) * Mathf.Clamp01(verticalFade);
                    t.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            t.Apply();
            return t;
        }
    }
}

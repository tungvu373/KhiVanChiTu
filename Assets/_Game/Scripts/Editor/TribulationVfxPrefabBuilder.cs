#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KhiVanChiTu.Remake.Editor
{
    public static class TribulationVfxPrefabBuilder
    {
        public const string PrefabResourcesPath = "Assets/_Game/Resources/VFX/VFX_HeavenlyTribulation.prefab";
        private const string ArtDir = "Assets/_Game/Prefabs/VFX";
        private const string TexDir = ArtDir + "/Textures";
        private const string MatDir = ArtDir + "/Materials";

        [MenuItem("KhiVanChiTu/Remake/Build Heavenly Tribulation VFX Prefab")]
        public static string BuildPrefab()
        {
            EnsureDir("Assets/_Game/Resources/VFX");
            EnsureDir(ArtDir);
            EnsureDir(TexDir);
            EnsureDir(MatDir);

            Texture2D beamTex = BakeAndImportTexture(TexDir + "/tex_vfx_lightning_beam.png", GenerateBeamTexture(64, 64));
            Texture2D sparkTex = BakeAndImportTexture(TexDir + "/tex_vfx_electric_spark.png", GenerateSparkTexture(64));
            Texture2D ringTex = BakeAndImportTexture(TexDir + "/tex_vfx_tribulation_ring.png", GenerateDoubleRingTexture(128));
            Texture2D cloudTex = BakeAndImportTexture(TexDir + "/tex_vfx_storm_vortex.png", GenerateStormCloudTexture(128));

            Shader urpParticle = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");

            Material matBolt = CreateOrUpdateMaterial(MatDir + "/Mat_Tribulation_Bolt.mat", urpParticle, beamTex, Color.white, true);
            Material matSpark = CreateOrUpdateMaterial(MatDir + "/Mat_Tribulation_Spark.mat", urpParticle, sparkTex, Color.white, true);
            Material matRing = CreateOrUpdateMaterial(MatDir + "/Mat_Tribulation_Ring.mat", urpParticle, ringTex, Color.white, true);
            Material matStorm = CreateOrUpdateMaterial(MatDir + "/Mat_Tribulation_StormCloud.mat", urpParticle, cloudTex, new Color(0.24f, 0.22f, 0.38f, 0.56f), false);

            var root = new GameObject("VFX_HeavenlyTribulation");
            try
            {
                var ctrl = root.AddComponent<RemakeTribulationVfx>();

                var outers = CreateBoltPool(root.transform, "Trib_OuterPillar", 3, RemakeTribulationVfx.PrimarySegments, matBolt, 1.0f);
                var cores = CreateBoltPool(root.transform, "Trib_CorePillar", 3, RemakeTribulationVfx.PrimarySegments, matBolt, 0.35f);
                var branches = CreateBoltPool(root.transform, "Trib_ForkBranch", 5, RemakeTribulationVfx.BranchSegments, matBolt, 0.28f);
                var arcs = CreateBoltPool(root.transform, "Trib_GroundArc", 5, RemakeTribulationVfx.GroundArcSegments, matBolt, 0.26f);

                ParticleSystem stormPs = CreateStormVortexSystem(root.transform, matStorm);
                ParticleSystem qiPs = CreateGatheringQiSystem(root.transform, matSpark);
                ParticleSystem sparksPs = CreateImpactSparksSystem(root.transform, matSpark);
                ParticleSystem ringPs = CreateShockwaveRingSystem(root.transform, matRing);
                ParticleSystem flashPs = CreateImpactFlashSystem(root.transform, matSpark);
                ParticleSystem climaxPs = CreateClimaxBurstSystem(root.transform, matSpark);

                ctrl.ConfigureReferences(outers, cores, branches, arcs, stormPs, qiPs, sparksPs, ringPs, flashPs, climaxPs);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabResourcesPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[TribulationVfxPrefabBuilder] Saved prefab -> {PrefabResourcesPath}");
                return PrefabResourcesPath;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static LineRenderer[] CreateBoltPool(Transform parent, string prefix, int count, int segments, Material mat, float width)
        {
            var arr = new LineRenderer[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"{prefix}_{i}");
                go.transform.SetParent(parent, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = mat;
                lr.useWorldSpace = true;
                lr.positionCount = segments;
                lr.startWidth = width;
                lr.endWidth = width * 0.5f;
                lr.numCapVertices = 2;
                lr.numCornerVertices = 2;
                lr.alignment = LineAlignment.View;
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.enabled = false;
                arr[i] = lr;
            }
            return arr;
        }

        private static ParticleSystem CreateStormVortexSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_StormVortex");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.up * 8.4f;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 4f;
            main.maxParticles = 12;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 3.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(5.5f, 8.8f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.20f, 0.22f, 0.36f, 0.45f),
                new Color(0.32f, 0.22f, 0.44f, 0.52f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 3.2f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 4.2f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-0.85f, -0.35f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.75f, 0.25f), new GradientAlphaKey(0.75f, 0.72f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            ConfigureRenderer(ps, mat, ParticleSystemRenderMode.HorizontalBillboard);
            return ps;
        }

        private static ParticleSystem CreateGatheringQiSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_GatheringQi");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 2f;
            main.maxParticles = 28;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.34f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.45f, 0.92f, 1.0f, 0.95f),
                new Color(1.0f, 0.84f, 0.30f, 0.95f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = true;
            em.rateOverTime = 18f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2.0f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.orbitalY = 2.6f;
            vel.radial = -0.85f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ConfigureRenderer(ps, mat, ParticleSystemRenderMode.Stretch);
            r.lengthScale = 2.1f;
            return ps;
        }

        private static ParticleSystem CreateImpactSparksSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_ImpactSparks");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 48;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.24f, 0.54f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6.5f, 15.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.34f);
            main.gravityModifier = 1.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.45f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.45f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ConfigureRenderer(ps, mat, ParticleSystemRenderMode.Stretch);
            r.lengthScale = 3.2f;
            r.velocityScale = 0.08f;
            return ps;
        }

        private static ParticleSystem CreateShockwaveRingSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_ShockwaveRings");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 8;
            main.startLifetime = 0.44f;
            main.startSpeed = 0f;
            main.startSize = 0.85f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            var curve = new AnimationCurve(
                new Keyframe(0f, 0.25f, 0f, 18f),
                new Keyframe(0.45f, 5.4f, 6f, 4f),
                new Keyframe(1f, 8.2f, 1.5f, 0f));
            sz.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.35f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            ConfigureRenderer(ps, mat, ParticleSystemRenderMode.HorizontalBillboard);
            return ps;
        }

        private static ParticleSystem CreateImpactFlashSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_ImpactFlash");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 6;
            main.startLifetime = 0.22f;
            main.startSpeed = 0f;
            main.startSize = 4.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.55f), new Keyframe(0.25f, 1.25f), new Keyframe(1f, 0.2f)));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            ConfigureRenderer(ps, mat, ParticleSystemRenderMode.Billboard);
            return ps;
        }

        private static ParticleSystem CreateClimaxBurstSystem(Transform parent, Material mat)
        {
            var go = new GameObject("PS_ClimaxBurst");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 40;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.95f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 12.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.52f);
            main.gravityModifier = 0.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ConfigureRenderer(ps, mat, ParticleSystemRenderMode.Stretch);
            r.lengthScale = 2.6f;
            return ps;
        }

        private static ParticleSystemRenderer ConfigureRenderer(ParticleSystem ps, Material mat, ParticleSystemRenderMode mode)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = mode;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return r;
        }

        private static Material CreateOrUpdateMaterial(string assetPath, Shader shader, Texture2D tex, Color color, bool additive)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            mat.shader = shader;
            mat.color = color;
            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            // URP 17 Particles/Unlit: 0=Alpha, 1=Premultiply, 2=Additive, 3=Multiply
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", additive ? 2f : 0f);
            if (mat.HasProperty("_ColorMode")) mat.SetFloat("_ColorMode", 0f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_SrcBlendAlpha")) mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            if (mat.HasProperty("_DstBlendAlpha")) mat.SetFloat("_DstBlendAlpha", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f); // Double-sided
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_ALPHAMODULATE_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D BakeAndImportTexture(string assetPath, Texture2D tex)
        {
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 128;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        // Sharp blazing lightning beam gradient across V with premultiplied black edges (zero square bleed!)
        private static Texture2D GenerateBeamTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                float v = Mathf.Abs((y / (float)(h - 1)) * 2f - 1f);
                float core = Mathf.Exp(-(v * v) / 0.042f);
                float halo = Mathf.Exp(-(v * v) / 0.24f) * 0.72f;
                float radialMask = Mathf.Clamp01(1f - v * v);
                float a = Mathf.Clamp01((core + halo) * radialMask);
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)(w - 1);
                    float edgeFade = Mathf.Clamp01(Mathf.Min(u * 12f, (1f - u) * 12f));
                    float fa = a * edgeFade;
                    tex.SetPixel(x, y, new Color(fa, fa, fa, fa));
                }
            }
            tex.Apply();
            return tex;
        }

        // Crisp radial plasma glow with pure-black zero-alpha corners (zero square edges!)
        private static Texture2D GenerateSparkTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d2 = dx * dx + dy * dy;
                    if (d2 >= 1f) { tex.SetPixel(x, y, Color.clear); continue; }
                    float core = Mathf.Exp(-d2 * 11f);
                    float halo = Mathf.Exp(-d2 * 3.2f) * 0.62f;
                    float edge = (1f - d2) * (1f - d2);
                    float a = Mathf.Clamp01((core + halo) * edge);
                    tex.SetPixel(x, y, new Color(a, a, a, a));
                }
            }
            tex.Apply();
            return tex;
        }

        // Double electric shockwave ring with pure-black zero-alpha outer boundary
        private static Texture2D GenerateDoubleRingTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= 0.98f) { tex.SetPixel(x, y, Color.clear); continue; }
                    float ang = Mathf.Atan2(dy, dx);
                    float ripple = 0.03f * Mathf.Sin(ang * 12f) + 0.015f * Mathf.Cos(ang * 20f);
                    float r1 = (dist - (0.76f + ripple)) / 0.072f;
                    float r2 = (dist - (0.52f - ripple)) / 0.052f;
                    float a = Mathf.Clamp01(Mathf.Exp(-r1 * r1) + Mathf.Exp(-r2 * r2) * 0.72f);
                    tex.SetPixel(x, y, new Color(a, a, a, a));
                }
            }
            tex.Apply();
            return tex;
        }

        // Soft swirling dark storm cloud puff with smooth zero-alpha outer edge
        private static Texture2D GenerateStormCloudTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist >= 0.98f) { tex.SetPixel(x, y, Color.clear); continue; }
                    float ang = Mathf.Atan2(dy, dx);
                    float swirl = 0.5f + 0.5f * Mathf.Sin(ang * 3f + dist * 6.5f);
                    float n = Mathf.PerlinNoise(x * 0.06f + 4.1f, y * 0.06f + 2.7f);
                    float falloff = Mathf.Clamp01(1f - dist);
                    falloff = falloff * falloff;
                    float a = Mathf.Clamp01(falloff * (0.45f + 0.55f * (swirl * 0.5f + n * 0.5f)));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return tex;
        }

        private static void EnsureDir(string assetPath)
        {
            if (!Directory.Exists(assetPath))
                Directory.CreateDirectory(assetPath);
        }
    }
}
#endif

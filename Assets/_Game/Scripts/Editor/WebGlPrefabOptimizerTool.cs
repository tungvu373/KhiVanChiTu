using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace KhiVanChiTu.Remake.Editor
{
    /// <summary>
    /// Non-destructively decimates ultra-high-poly Meshy meshes, downscales 2K/4K unit textures to 512/256px,
    /// and isolates Avatar/Clips into Assets/_Game/Prefabs/Optimized so WebGL data.gz drops ~80%
    /// while leaving Assets/Import and Assets/Prefabs completely untouched for PCContent.
    /// </summary>
    public static class WebGlPrefabOptimizerTool
    {
        private const string OptRoot = "Assets/_Game/Prefabs/Optimized";
        private const string MeshDir = OptRoot + "/Meshes";
        private const string TexDir = OptRoot + "/Textures";
        private const string MatDir = OptRoot + "/Materials";
        private const string AvatarDir = OptRoot + "/Avatars";
        private const string AnimDir = OptRoot + "/Anims";
        private const string EnvDir = OptRoot + "/Environment";

        public static void OptimizeAllGamePrefabs()
        {
            EnsureFolders();

            string[] targets =
            {
                "Assets/_Game/Prefabs/Player/RemakePlayer.prefab",
                "Assets/_Game/Prefabs/Monsters/Monster_Act1_Wolf.prefab",
                "Assets/_Game/Prefabs/Monsters/Monster_Act1_Bird.prefab",
                "Assets/_Game/Prefabs/Monsters/Monster_Act1_Turtle.prefab",
                "Assets/_Game/Prefabs/Bosses/Boss_Act1_Guardian.prefab",
                "Assets/_Game/Prefabs/Monsters/Monster_Act2_Bear.prefab",
                "Assets/_Game/Prefabs/Monsters/Monster_Act3_ThunderSpirit.prefab"
            };

            foreach (string prefabPath in targets)
            {
                if (!File.Exists(Path.GetFullPath(prefabPath))) continue;
                bool isPlayer = prefabPath.Contains("RemakePlayer");
                OptimizeSinglePrefab(prefabPath, isPlayer ? 6500 : 4200, isPlayer ? 512 : 256);
            }
            OptimizeHolotnaEnvironmentPrefabs();
            AssetDatabase.SaveAssets();
        }

        public static void OptimizeHolotnaEnvironmentPrefabs()
        {
            EnsureFolders();
            // Build strictly shadow-free, collider-free, GPU-instanced 256px-diffuse prefabs from Holotna/Mountain
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Tree01A.prefab", "Env_Act1_Tree", new Color(0.95f, 1.04f, 0.92f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Bush01.prefab", "Env_Act1_Bush", new Color(0.96f, 1.08f, 0.92f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Rock01.prefab", "Env_Act1_Rock", new Color(0.90f, 0.96f, 0.88f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Flowers01A.prefab", "Env_Act1_Flower", new Color(1.06f, 1.06f, 1.02f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Mountain01.prefab", "Env_Act1_Mountain", new Color(0.86f, 0.96f, 0.92f));

            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Rock01.prefab", "Env_Act2_Rock", new Color(1.02f, 0.82f, 0.56f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Pebbles01.prefab", "Env_Act2_Pebble", new Color(1.04f, 0.86f, 0.60f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Bush01.prefab", "Env_Act2_DryBush", new Color(0.96f, 0.82f, 0.50f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Mountain01.prefab", "Env_Act2_Mountain", new Color(1.02f, 0.84f, 0.60f));

            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Rock02.prefab", "Env_Act3_JadeCrystal", new Color(0.68f, 1.04f, 0.96f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Tree01B.prefab", "Env_Act3_SnowPine", new Color(0.82f, 1.02f, 0.98f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Flowers01B.prefab", "Env_Act3_Flower", new Color(0.86f, 1.02f, 1.04f));
            BuildEnvVariant("Assets/Holotna/Mountain/Prefabs/Mountain01.prefab", "Env_Act3_Mountain", new Color(0.76f, 0.96f, 0.98f));
        }

        private static GameObject BuildEnvVariant(string srcPrefabPath, string key, Color tint)
        {
            string destPrefabPath = $"{EnvDir}/{key}.prefab";
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(srcPrefabPath);
            if (src == null) return null;

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var col in inst.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(col);
                foreach (var lod in inst.GetComponentsInChildren<LODGroup>(true))
                {
                    var lods = lod.GetLODs();
                    int keepLod = lods.Length > 1 ? 1 : 0;
                    var keepRenderers = new HashSet<Renderer>();
                    if (keepLod < lods.Length && lods[keepLod].renderers != null)
                        foreach (var kr in lods[keepLod].renderers) if (kr != null) keepRenderers.Add(kr);
                    for (int li = 0; li < lods.Length; li++)
                    {
                        if (li == keepLod || lods[li].renderers == null) continue;
                        foreach (var rr in lods[li].renderers)
                        {
                            if (rr != null && !keepRenderers.Contains(rr))
                                UnityEngine.Object.DestroyImmediate(rr.gameObject);
                        }
                    }
                    UnityEngine.Object.DestroyImmediate(lod);
                }

                int mIdx = 0;
                foreach (var mf in inst.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null) continue;
                    string baseMeshKey = SanitizeFileName(mf.sharedMesh.name);
                    Mesh optMesh = GetOrCreateOptimizedMesh(mf.sharedMesh, $"EnvMesh_{baseMeshKey}", 2400);
                    if (optMesh != null) mf.sharedMesh = optMesh;

                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.shadowCastingMode = ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                        var mats = mr.sharedMaterials;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            Material srcMat = mats[m];
                            string matPath = $"{MatDir}/{key}_{mIdx}_{m}.mat";
                            Material envMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                            if (envMat == null)
                            {
                                Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? srcMat?.shader;
                                envMat = new Material(lit);
                                AssetDatabase.CreateAsset(envMat, matPath);
                            }

                            Texture srcTex = null;
                            if (srcMat != null)
                            {
                                if (srcMat.HasProperty("_Albedo")) srcTex = srcMat.GetTexture("_Albedo");
                                if (srcTex == null && srcMat.HasProperty("_BaseMap")) srcTex = srcMat.GetTexture("_BaseMap");
                                if (srcTex == null) srcTex = srcMat.mainTexture;
                            }
                            if (srcTex is Texture2D tex2d)
                            {
                                Texture2D smallTex = GetOrCreateDownscaledTexture(tex2d, $"EnvTex_{SanitizeFileName(tex2d.name)}", 256);
                                if (smallTex != null)
                                {
                                    envMat.mainTexture = smallTex;
                                    if (envMat.HasProperty("_BaseMap")) envMat.SetTexture("_BaseMap", smallTex);
                                }
                            }

                            bool isAlphaCut = srcMat != null && srcMat.HasProperty("_AlphaClip") && srcMat.GetFloat("_AlphaClip") > 0.5f;
                            if (envMat.HasProperty("_AlphaClip")) envMat.SetFloat("_AlphaClip", isAlphaCut ? 1f : 0f);
                            if (envMat.HasProperty("_Cutoff")) envMat.SetFloat("_Cutoff", 0.42f);
                            if (isAlphaCut)
                            {
                                envMat.EnableKeyword("_ALPHATEST_ON");
                                envMat.SetOverrideTag("RenderType", "TransparentCutout");
                                envMat.renderQueue = (int)RenderQueue.AlphaTest;
                            }
                            else
                            {
                                envMat.DisableKeyword("_ALPHATEST_ON");
                                envMat.SetOverrideTag("RenderType", "Opaque");
                                envMat.renderQueue = -1;
                            }

                            if (envMat.HasProperty("_BaseColor")) envMat.SetColor("_BaseColor", tint);
                            envMat.color = tint;
                            if (envMat.HasProperty("_Smoothness")) envMat.SetFloat("_Smoothness", 0.08f);
                            envMat.enableInstancing = true;
                            EditorUtility.SetDirty(envMat);
                            mats[m] = envMat;
                        }
                        mr.sharedMaterials = mats;
                    }
                    mIdx++;
                }

                inst.isStatic = true;
                return PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        public static GameObject CreateTintedPrefabVariant(string srcOptPrefabPath, string destPrefabPath, string matVariantPrefix, Color tint)
        {
            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(srcOptPrefabPath);
            if (src == null) return null;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                int idx = 0;
                foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var mats = smr.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                        mats[m] = CreateTintedVariantMaterial(mats[m], $"{MatDir}/{matVariantPrefix}_s{idx}_{m}.mat", tint) ?? mats[m];
                    smr.sharedMaterials = mats;
                    idx++;
                }
                foreach (var mr in inst.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mats = mr.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                        mats[m] = CreateTintedVariantMaterial(mats[m], $"{MatDir}/{matVariantPrefix}_m{idx}_{m}.mat", tint) ?? mats[m];
                    mr.sharedMaterials = mats;
                    idx++;
                }
                return PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        public static Material CreateTintedVariantMaterial(Material srcMat, string destAssetPath, Color tint)
        {
            if (srcMat == null) return null;
            var existing = AssetDatabase.LoadAssetAtPath<Material>(destAssetPath);
            if (existing == null)
            {
                existing = new Material(srcMat);
                AssetDatabase.CreateAsset(existing, destAssetPath);
            }
            else
            {
                existing.shader = srcMat.shader;
                existing.mainTexture = srcMat.mainTexture;
            }
            if (existing.HasProperty("_BaseMap") && srcMat.HasProperty("_BaseMap"))
                existing.SetTexture("_BaseMap", srcMat.GetTexture("_BaseMap"));
            if (existing.HasProperty("_BaseColor"))
                existing.SetColor("_BaseColor", tint);
            existing.color = tint;
            existing.enableInstancing = true;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static void OptimizeSinglePrefab(string prefabPath, int targetTris, int maxTexSize)
        {
            string unitKey = Path.GetFileNameWithoutExtension(prefabPath);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            bool modified = false;

            try
            {
                int meshIndex = 0;
                foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (smr.sharedMesh != null)
                    {
                        Mesh optMesh = GetOrCreateOptimizedMesh(smr.sharedMesh, $"{unitKey}_smr{meshIndex}", targetTris);
                        if (optMesh != null && optMesh != smr.sharedMesh)
                        {
                            smr.sharedMesh = optMesh;
                            modified = true;
                        }
                    }
                    Material[] mats = smr.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        Material optMat = GetOrCreateLightweightMaterial(mats[m], $"{unitKey}_mat{meshIndex}_{m}", maxTexSize);
                        if (optMat != null && optMat != mats[m])
                        {
                            mats[m] = optMat;
                            modified = true;
                        }
                    }
                    smr.sharedMaterials = mats;
                    meshIndex++;
                }

                foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null) continue;
                    Mesh optMesh = GetOrCreateOptimizedMesh(mf.sharedMesh, $"{unitKey}_mf{meshIndex}", targetTris);
                    if (optMesh != null && optMesh != mf.sharedMesh)
                    {
                        mf.sharedMesh = optMesh;
                        modified = true;
                    }
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        Material[] mats = mr.sharedMaterials;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            Material optMat = GetOrCreateLightweightMaterial(mats[m], $"{unitKey}_mfmat{meshIndex}_{m}", maxTexSize);
                            if (optMat != null && optMat != mats[m])
                            {
                                mats[m] = optMat;
                                modified = true;
                            }
                        }
                        mr.sharedMaterials = mats;
                    }
                    meshIndex++;
                }

                // Clone Avatar & AnimatorController into _Game so Build scenes do not drag giant source FBXs
                var anim = root.GetComponentInChildren<Animator>(true);
                if (anim != null)
                {
                    if (anim.avatar != null && !AssetDatabase.GetAssetPath(anim.avatar).StartsWith(OptRoot))
                    {
                        string avPath = $"{AvatarDir}/{unitKey}_Avatar.asset";
                        Avatar clonedAv = AssetDatabase.LoadAssetAtPath<Avatar>(avPath);
                        if (clonedAv == null)
                        {
                            clonedAv = UnityEngine.Object.Instantiate(anim.avatar);
                            clonedAv.name = $"{unitKey}_Avatar";
                            AssetDatabase.CreateAsset(clonedAv, avPath);
                        }
                        anim.avatar = clonedAv;
                        modified = true;
                    }

                    if (anim.runtimeAnimatorController is AnimatorController srcCtrl &&
                        !AssetDatabase.GetAssetPath(srcCtrl).StartsWith(OptRoot))
                    {
                        var clonedCtrl = CloneAnimatorControllerLight(srcCtrl, unitKey);
                        if (clonedCtrl != null)
                        {
                            anim.runtimeAnimatorController = clonedCtrl;
                            modified = true;
                        }
                    }
                }

                if (modified)
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static AnimatorController CloneAnimatorControllerLight(AnimatorController srcCtrl, string unitKey)
        {
            string ctrlPath = $"{AnimDir}/{unitKey}_Ctrl.controller";
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (existing != null) return existing;

            string srcCtrlPath = AssetDatabase.GetAssetPath(srcCtrl);
            if (!AssetDatabase.CopyAsset(srcCtrlPath, ctrlPath)) return srcCtrl;

            var cloned = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (cloned == null) return srcCtrl;

            // Duplicate referenced AnimationClips from Import/*.fbx cleanly into AnimDir so FBX isn't packed
            var clipCache = new Dictionary<AnimationClip, AnimationClip>();
            foreach (var layer in cloned.layers)
            {
                RemapStateMachineClips(layer.stateMachine, unitKey, clipCache);
            }
            EditorUtility.SetDirty(cloned);
            return cloned;
        }

        private static void RemapStateMachineClips(AnimatorStateMachine sm, string unitKey, Dictionary<AnimationClip, AnimationClip> cache)
        {
            if (sm == null) return;
            foreach (var child in sm.states)
            {
                if (child.state?.motion is AnimationClip clip)
                {
                    child.state.motion = CloneClipAsset(clip, unitKey, cache);
                }
            }
            foreach (var sub in sm.stateMachines)
                RemapStateMachineClips(sub.stateMachine, unitKey, cache);
        }

        private static AnimationClip CloneClipAsset(AnimationClip srcClip, string unitKey, Dictionary<AnimationClip, AnimationClip> cache)
        {
            if (srcClip == null) return null;
            if (cache.TryGetValue(srcClip, out var hit)) return hit;

            string srcPath = AssetDatabase.GetAssetPath(srcClip);
            if (srcPath.StartsWith(OptRoot))
            {
                cache[srcClip] = srcClip;
                return srcClip;
            }

            string cleanName = SanitizeFileName(srcClip.name);
            string destPath = $"{AnimDir}/{unitKey}_{cleanName}_{cache.Count}.anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath);
            if (existing == null)
            {
                existing = UnityEngine.Object.Instantiate(srcClip);
                existing.name = $"{unitKey}_{cleanName}";
                AssetDatabase.CreateAsset(existing, destPath);
            }
            cache[srcClip] = existing;
            return existing;
        }

        private static Material GetOrCreateLightweightMaterial(Material srcMat, string key, int maxTexSize)
        {
            if (srcMat == null) return null;
            string srcPath = AssetDatabase.GetAssetPath(srcMat);
            if (srcPath.StartsWith(OptRoot)) return srcMat;

            string matPath = $"{MatDir}/{key}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null) return existing;

            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? srcMat.shader;
            var mat = new Material(lit);
            Color baseCol = srcMat.HasProperty("_BaseColor") ? srcMat.GetColor("_BaseColor")
                : srcMat.HasProperty("_Color") ? srcMat.GetColor("_Color") : Color.white;
            if (key.StartsWith("RemakePlayer", StringComparison.OrdinalIgnoreCase))
            {
                baseCol = new Color(Mathf.Min(1f, baseCol.r * 1.25f + 0.14f), Mathf.Min(1f, baseCol.g * 1.25f + 0.14f), Mathf.Min(1f, baseCol.b * 1.25f + 0.14f), 1f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.11f, 0.14f, 0.15f));
            }
            mat.SetColor("_BaseColor", baseCol);
            mat.SetFloat("_Smoothness", 0.14f);
            mat.SetFloat("_Metallic", 0.0f);
            mat.enableInstancing = true;

            Texture srcTex = srcMat.HasProperty("_BaseMap") ? srcMat.GetTexture("_BaseMap") : srcMat.mainTexture;
            if (srcTex is Texture2D tex2d)
            {
                Texture2D lightTex = GetOrCreateDownscaledTexture(tex2d, $"{key}_diff", maxTexSize);
                if (lightTex != null)
                {
                    mat.mainTexture = lightTex;
                    mat.SetTexture("_BaseMap", lightTex);
                }
            }

            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
        }

        private static Texture2D GetOrCreateDownscaledTexture(Texture2D srcTex, string key, int maxSize)
        {
            string srcPath = AssetDatabase.GetAssetPath(srcTex);
            if (string.IsNullOrEmpty(srcPath) || srcPath.StartsWith(OptRoot)) return srcTex;

            string destPath = $"{TexDir}/{key}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
            if (existing != null) return existing;

            string fullSrc = Path.GetFullPath(srcPath);
            if (!File.Exists(fullSrc)) return srcTex;

            byte[] bytes = File.ReadAllBytes(fullSrc);
            var tmp = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(tmp, bytes, false))
                    return srcTex;

                int tw = Mathf.Min(maxSize, tmp.width);
                int th = Mathf.Min(maxSize, tmp.height);
                // Maintain aspect ratio
                float scale = Mathf.Min(maxSize / (float)Mathf.Max(1, tmp.width), maxSize / (float)Mathf.Max(1, tmp.height));
                if (scale < 1f)
                {
                    tw = Mathf.Max(16, Mathf.RoundToInt(tmp.width * scale));
                    th = Mathf.Max(16, Mathf.RoundToInt(tmp.height * scale));
                }

                Texture2D resized = ResizeBilinear(tmp, tw, th);
                try
                {
                    File.WriteAllBytes(Path.GetFullPath(destPath), resized.EncodeToPNG());
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(resized);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tmp);
            }

            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(destPath);
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Default;
                imp.maxTextureSize = maxSize;
                imp.mipmapEnabled = true;
                imp.textureCompression = TextureImporterCompression.Compressed;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
        }

        private static Texture2D ResizeBilinear(Texture2D src, int newW, int newH)
        {
            RenderTexture rt = RenderTexture.GetTemporary(newW, newH, 0, RenderTextureFormat.ARGB32);
            RenderTexture prev = RenderTexture.active;
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;
            var dst = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
            dst.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
            dst.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return dst;
        }

        private static Mesh GetOrCreateOptimizedMesh(Mesh srcMesh, string key, int targetMaxTris)
        {
            string srcPath = AssetDatabase.GetAssetPath(srcMesh);
            if (srcPath.StartsWith(OptRoot)) return srcMesh;

            string destPath = $"{MeshDir}/{key}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(destPath);
            if (existing != null) return existing;

            int srcTris = srcMesh.triangles.Length / 3;
            ModelImporter modImp = !string.IsNullOrEmpty(srcPath) ? AssetImporter.GetAtPath(srcPath) as ModelImporter : null;
            bool toggledReadable = false;

            try
            {
                if (!srcMesh.isReadable && modImp != null)
                {
                    modImp.isReadable = true;
                    modImp.SaveAndReimport();
                    toggledReadable = true;
                }

                Mesh result = srcTris <= targetMaxTris + 2500
                    ? CloneMeshDirect(srcMesh, key)
                    : DecimateMeshVoxelCluster(srcMesh, key, targetMaxTris);

                AssetDatabase.CreateAsset(result, destPath);
                Debug.Log($"[WebGlOptimize] {key}: {srcTris} tris -> {result.triangles.Length / 3} tris");
                return result;
            }
            finally
            {
                if (toggledReadable && modImp != null)
                {
                    modImp.isReadable = false;
                    modImp.SaveAndReimport();
                }
            }
        }

        private static Mesh CloneMeshDirect(Mesh src, string key)
        {
            var m = UnityEngine.Object.Instantiate(src);
            m.name = key;
            return m;
        }

        private static Mesh DecimateMeshVoxelCluster(Mesh src, string key, int targetMaxTris)
        {
            Vector3[] verts = src.vertices;
            Vector3[] norms = src.normals;
            Vector2[] uvs = src.uv;
            BoneWeight[] weights = src.boneWeights;
            Bounds b = src.bounds;
            Vector3 size = b.size;
            float maxDim = Mathf.Max(0.001f, Mathf.Max(size.x, Mathf.Max(size.y, size.z)));

            int gridRes = 42;
            Mesh bestMesh = null;

            for (int iter = 0; iter < 5; iter++)
            {
                float cellSize = maxDim / gridRes;
                var cellToNewIdx = new Dictionary<long, int>(verts.Length / 4);
                var oldToNew = new int[verts.Length];
                var sumPos = new List<Vector3>(verts.Length / 4);
                var sumNorm = new List<Vector3>(verts.Length / 4);
                var sumUv = new List<Vector2>(verts.Length / 4);
                var counts = new List<int>(verts.Length / 4);
                var outWeights = new List<BoneWeight>(verts.Length / 4);

                bool hasNorm = norms != null && norms.Length == verts.Length;
                bool hasUv = uvs != null && uvs.Length == verts.Length;
                bool hasBw = weights != null && weights.Length == verts.Length;

                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 rel = (verts[i] - b.min) / cellSize;
                    int gx = Mathf.Clamp(Mathf.FloorToInt(rel.x), 0, 2000);
                    int gy = Mathf.Clamp(Mathf.FloorToInt(rel.y), 0, 2000);
                    int gz = Mathf.Clamp(Mathf.FloorToInt(rel.z), 0, 2000);
                    long cellKey = ((long)gx << 40) | ((long)gy << 20) | (long)gz;

                    if (!cellToNewIdx.TryGetValue(cellKey, out int newIdx))
                    {
                        newIdx = sumPos.Count;
                        cellToNewIdx[cellKey] = newIdx;
                        sumPos.Add(verts[i]);
                        sumNorm.Add(hasNorm ? norms[i] : Vector3.up);
                        sumUv.Add(hasUv ? uvs[i] : Vector2.zero);
                        counts.Add(1);
                        if (hasBw) outWeights.Add(weights[i]);
                    }
                    else
                    {
                        sumPos[newIdx] += verts[i];
                        if (hasNorm) sumNorm[newIdx] += norms[i];
                        if (hasUv) sumUv[newIdx] += uvs[i];
                        counts[newIdx]++;
                    }
                    oldToNew[i] = newIdx;
                }

                int vCount = sumPos.Count;
                for (int i = 0; i < vCount; i++)
                {
                    float inv = 1f / counts[i];
                    sumPos[i] *= inv;
                    sumNorm[i] = sumNorm[i].sqrMagnitude > 0.0001f ? sumNorm[i].normalized : Vector3.up;
                    sumUv[i] *= inv;
                }

                var candidate = new Mesh { name = key };
                if (vCount > 65000) candidate.indexFormat = IndexFormat.UInt32;
                candidate.SetVertices(sumPos);
                if (hasNorm) candidate.SetNormals(sumNorm);
                if (hasUv) candidate.SetUVs(0, sumUv);
                if (hasBw && outWeights.Count == vCount)
                {
                    candidate.boneWeights = outWeights.ToArray();
                    candidate.bindposes = src.bindposes;
                }

                candidate.subMeshCount = src.subMeshCount;
                int totalTris = 0;
                for (int sm = 0; sm < src.subMeshCount; sm++)
                {
                    int[] srcTris = src.GetTriangles(sm);
                    var dstTris = new List<int>(srcTris.Length / 2);
                    for (int t = 0; t + 2 < srcTris.Length; t += 3)
                    {
                        int a = oldToNew[srcTris[t]];
                        int c = oldToNew[srcTris[t + 1]];
                        int d = oldToNew[srcTris[t + 2]];
                        if (a != c && c != d && a != d)
                        {
                            dstTris.Add(a);
                            dstTris.Add(c);
                            dstTris.Add(d);
                        }
                    }
                    totalTris += dstTris.Count / 3;
                    candidate.SetTriangles(dstTris, sm);
                }

                candidate.RecalculateBounds();
                if (bestMesh != null) UnityEngine.Object.DestroyImmediate(bestMesh);
                bestMesh = candidate;

                if (totalTris <= targetMaxTris * 1.2f && totalTris >= 1800) break;
                if (totalTris > targetMaxTris) gridRes = Mathf.Max(14, Mathf.RoundToInt(gridRes * 0.72f));
                else gridRes = Mathf.RoundToInt(gridRes * 1.35f);
            }

            return bestMesh;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_').Replace('|', '_');
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { OptRoot, MeshDir, TexDir, MatDir, AvatarDir, AnimDir, EnvDir })
            {
                string[] parts = folder.Split('/');
                string cur = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = cur + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(cur, parts[i]);
                    cur = next;
                }
            }
        }
    }
}

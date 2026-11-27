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
    /// Builds Sunlit Xianxia (Tiên Hiệp Thanh Vân Sáng) 3D character & monster prefabs,
    /// binds baked UV-clean diffuse textures + subtle sunlit emission, builds clean 4-state
    /// AnimatorControllers, and isolates Meshes/Avatars/Clips into Assets/_Game/Prefabs/Optimized
    /// while leaving Assets/Import and Assets/Prefabs completely untouched for PCContent.
    /// </summary>
    public static class WebGlPrefabOptimizerTool
    {
        private const string OptRoot = "Assets/_Game/Prefabs/Optimized";
        private const string ModelDir = OptRoot + "/Models";
        private const string MeshDir = OptRoot + "/Meshes";
        private const string TexDir = OptRoot + "/Textures";
        private const string MatDir = OptRoot + "/Materials";
        private const string AvatarDir = OptRoot + "/Avatars";
        private const string AnimDir = OptRoot + "/Anims";
        private const string EnvDir = OptRoot + "/Environment";

        public static void OptimizeAllGamePrefabs()
        {
            EnsureFolders();

            // Optimize Mixamo rigged units WITHOUT voxel-decimating their UVs/weights, and bind Sunlit Xianxia textures!
            OptimizeMixamoBossPrefab(
                "Assets/_Game/Prefabs/Bosses/Boss_Act1_Guardian.prefab",
                "Boss_Act1_Guardian",
                "Tex_Boss1_Body.png",
                "Tex_Boss1_Armor.png",
                new Color(0.09f, 0.15f, 0.10f));

            OptimizeMixamoSingleTexPrefab(
                "Assets/_Game/Prefabs/Monsters/Monster_Act2_Bear.prefab",
                "Monster_Act2_Bear",
                "Tex_Act2_Bear.png",
                new Color(0.13f, 0.10f, 0.06f));

            OptimizeMixamoSingleTexPrefab(
                "Assets/_Game/Prefabs/Monsters/Monster_Act3_ThunderSpirit.prefab",
                "Monster_Act3_ThunderSpirit",
                "Tex_Act3_ThunderSpirit.png",
                new Color(0.11f, 0.15f, 0.18f));

            OptimizeHolotnaEnvironmentPrefabs();
            AssetDatabase.SaveAssets();
        }

        public static GameObject BuildXianxiaPlayerPrefab(string destPrefabPath)
        {
            EnsureFolders();
            string fbxPath = $"{ModelDir}/RemakePlayer_Xianxia.fbx";
            ConfigureXianxiaModelImporter(fbxPath);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (modelAsset == null)
                throw new InvalidOperationException($"Missing animated player FBX: {fbxPath}");

            Texture2D tex = ConfigureAndLoadXianxiaTexture("Tex_Player_ThanhVan.png", 1024);
            Material mat = CreateOrUpdateXianxiaMaterial(
                $"{MatDir}/Mat_Player_ThanhVan.mat",
                tex,
                Color.white,
                new Color(0.16f, 0.19f, 0.21f),
                0.18f);

            AnimationClip idleClip = ExtractAndConfigureClip(fbxPath, "Idle", "Player_Idle", true);
            AnimationClip castClip = ExtractAndConfigureClip(fbxPath, "CastSkill", "Player_CastSkill", false);
            AnimationClip tribClip = ExtractAndConfigureClip(fbxPath, "Tribulation", "Player_Tribulation", true);
            AnimationClip respawnClip = ExtractAndConfigureClip(fbxPath, "Respawn", "Player_Respawn", false);
            AnimatorController ctrl = BuildPlayerAnimatorController($"{AnimDir}/RemakePlayer_Xianxia_Ctrl.controller", idleClip, castClip, tribClip, respawnClip);

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                inst.name = "RemakePlayer";
                inst.transform.position = Vector3.zero;
                inst.transform.rotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;

                CloneAndBindSkinnedMeshesAndMaterials(inst, "RemakePlayer_Xianxia", new[] { mat });

                var anim = inst.GetComponentInChildren<Animator>(true) ?? inst.AddComponent<Animator>();
                anim.avatar = CloneAvatarFromFbx(fbxPath, "RemakePlayer_Xianxia");
                anim.runtimeAnimatorController = ctrl;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                anim.applyRootMotion = false;

                var saved = PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
                AssetDatabase.SaveAssets();
                return saved;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        public static void BuildXianxia3DSwordAssets(out Mesh[] meshes, out Material[] mats)
        {
            EnsureFolders();
            string swordModelFolder = $"{ModelDir}/Swords";
            if (!AssetDatabase.IsValidFolder(swordModelFolder))
                AssetDatabase.CreateFolder(ModelDir, "Swords");

            meshes = new Mesh[10];
            mats = new Material[10];
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Standard");

            for (int i = 1; i <= 10; i++)
            {
                string fbxPath = $"{swordModelFolder}/Sword_{i:D2}_Xianxia.fbx";
                AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceSynchronousImport);
                if (AssetImporter.GetAtPath(fbxPath) is ModelImporter mi)
                {
                    bool dirty = false;
                    if (mi.importAnimation) { mi.importAnimation = false; dirty = true; }
                    if (mi.animationType != ModelImporterAnimationType.None) { mi.animationType = ModelImporterAnimationType.None; dirty = true; }
                    if (mi.materialImportMode != ModelImporterMaterialImportMode.None) { mi.materialImportMode = ModelImporterMaterialImportMode.None; dirty = true; }
                    if (mi.isReadable) { mi.isReadable = false; dirty = true; }
                    if (dirty) mi.SaveAndReimport();
                }

                Mesh srcMesh = null;
                foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
                {
                    if (sub is Mesh m) { srcMesh = m; break; }
                }
                if (srcMesh != null)
                {
                    Mesh cleanSwordMesh = ForceCreateCleanMeshAsset(srcMesh, $"Sword_{i:D2}_Xianxia_Mesh");
                    NormalizeSwordMeshToTargetLength(cleanSwordMesh, 1.36f);
                    meshes[i - 1] = cleanSwordMesh;
                }

                Texture2D diffTex = ConfigureAndLoadXianxiaTexture($"Tex_Sword_{i:D2}.png", 512);
                Texture2D emitTex = ConfigureAndLoadXianxiaTexture($"Tex_Sword_{i:D2}_Emit.png", 512);

                string matPath = $"{MatDir}/Mat_Sword_{i:D2}_Xianxia.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(litShader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                else if (mat.shader != litShader && litShader != null)
                {
                    mat.shader = litShader;
                }

                mat.mainTexture = diffTex;
                mat.color = Color.white;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", diffTex);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.36f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.64f);
                if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
                if (emitTex != null && mat.HasProperty("_EmissionMap"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetTexture("_EmissionMap", emitTex);
                    float glowBoost = Mathf.Lerp(0.95f, 1.45f, (i - 1) / 9f);
                    mat.SetColor("_EmissionColor", Color.white * glowBoost);
                }
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
                mats[i - 1] = mat;
            }
            AssetDatabase.SaveAssets();
        }

        private static void NormalizeSwordMeshToTargetLength(Mesh mesh, float targetLengthMeters)
        {
            if (mesh == null) return;
            mesh.RecalculateBounds();
            Vector3 sz = mesh.bounds.size;
            float maxDim = Mathf.Max(sz.x, Mathf.Max(sz.y, sz.z));
            if (maxDim < 0.00001f) return;

            // Blender FBX raw Mesh sub-assets import at 0.01x (centimeters) while root Transform holds 100x.
            // Scale vertices directly so the standalone Mesh asset is 1.36m tip-to-pommel along +Z!
            float scale = targetLengthMeters / maxDim;
            bool needAxisSwap = sz.y > sz.z * 1.5f; // Ensure blade length aligns along Unity +Z (Vector3.forward)!
            if (Mathf.Abs(scale - 1f) < 0.03f && !needAxisSwap) return;

            Vector3[] verts = mesh.vertices;
            for (int v = 0; v < verts.Length; v++)
            {
                Vector3 p = verts[v] * scale;
                if (needAxisSwap)
                    p = new Vector3(p.x, p.z, p.y);
                verts[v] = p;
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
        }

        public static GameObject BuildXianxiaMonsterPrefab(
            string fbxPath,
            string destPrefabPath,
            string unitKey,
            string texFileName,
            Color emissionLift)
        {
            EnsureFolders();
            ConfigureXianxiaModelImporter(fbxPath);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (modelAsset == null)
                throw new InvalidOperationException($"Missing animated monster FBX: {fbxPath}");

            Texture2D tex = ConfigureAndLoadXianxiaTexture(texFileName, 512);
            Material mat = CreateOrUpdateXianxiaMaterial(
                $"{MatDir}/Mat_{unitKey}_Xianxia.mat",
                tex,
                Color.white,
                emissionLift,
                0.15f);

            AnimationClip idleClip = ExtractAndConfigureClip(fbxPath, "Idle", $"{unitKey}_Idle", true);
            AnimationClip moveClip = ExtractAndConfigureClip(fbxPath, "Move", $"{unitKey}_Move", true);
            AnimationClip atkClip = ExtractAndConfigureClip(fbxPath, "Attack", $"{unitKey}_Attack", false);
            AnimationClip dieClip = ExtractAndConfigureClip(fbxPath, "Die", $"{unitKey}_Die", false);
            AnimatorController ctrl = BuildMonsterAnimatorController($"{AnimDir}/{unitKey}_Xianxia_Ctrl.controller", idleClip, moveClip, atkClip, dieClip);

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                inst.name = Path.GetFileNameWithoutExtension(destPrefabPath);
                inst.transform.position = Vector3.zero;
                inst.transform.rotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;

                CloneAndBindSkinnedMeshesAndMaterials(inst, unitKey, new[] { mat });

                var anim = inst.GetComponentInChildren<Animator>(true) ?? inst.AddComponent<Animator>();
                anim.avatar = CloneAvatarFromFbx(fbxPath, unitKey);
                anim.runtimeAnimatorController = ctrl;
                anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                anim.applyRootMotion = false;

                if (inst.GetComponent<RemakeMonsterActor>() == null)
                    inst.AddComponent<RemakeMonsterActor>();

                var saved = PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
                AssetDatabase.SaveAssets();
                return saved;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        public static GameObject CreateXianxiaTexturedVariant(
            string srcOptPrefabPath,
            string destPrefabPath,
            string variantKey,
            string texFileName,
            Color emissionLift)
        {
            EnsureFolders();
            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(srcOptPrefabPath);
            if (src == null) return null;

            Texture2D tex = ConfigureAndLoadXianxiaTexture(texFileName, 512);
            Material mat = CreateOrUpdateXianxiaMaterial(
                $"{MatDir}/Mat_{variantKey}_Xianxia.mat",
                tex,
                Color.white,
                emissionLift,
                0.15f);

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var mats = smr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    smr.sharedMaterials = mats;
                }
                foreach (var mr in inst.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mats = mr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    mr.sharedMaterials = mats;
                }
                var srcAnim = src.GetComponentInChildren<Animator>(true);
                var dstAnim = inst.GetComponentInChildren<Animator>(true);
                if (srcAnim != null && dstAnim != null)
                {
                    dstAnim.runtimeAnimatorController = srcAnim.runtimeAnimatorController;
                    dstAnim.avatar = srcAnim.avatar;
                }
                return PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        public static GameObject CreateXianxiaBossVariant(
            string srcBossPrefabPath,
            string destPrefabPath,
            string variantKey,
            string bodyTexFileName,
            string armorTexFileName,
            Color emissionLift)
        {
            EnsureFolders();
            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(srcBossPrefabPath);
            if (src == null) return null;

            Texture2D bodyTex = ConfigureAndLoadXianxiaTexture(bodyTexFileName, 512);
            Texture2D armorTex = ConfigureAndLoadXianxiaTexture(armorTexFileName, 512);
            Material bodyMat = CreateOrUpdateXianxiaMaterial($"{MatDir}/Mat_{variantKey}_Body.mat", bodyTex, Color.white, emissionLift, 0.16f);
            Material armorMat = CreateOrUpdateXianxiaMaterial($"{MatDir}/Mat_{variantKey}_Armor.mat", armorTex, Color.white, emissionLift * 1.15f, 0.28f);

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var mats = smr.sharedMaterials;
                    if (mats.Length >= 2)
                    {
                        mats[0] = bodyMat;
                        mats[1] = armorMat;
                    }
                    else if (mats.Length == 1)
                    {
                        mats[0] = armorMat;
                    }
                    smr.sharedMaterials = mats;
                }
                var srcAnim = src.GetComponentInChildren<Animator>(true);
                var dstAnim = inst.GetComponentInChildren<Animator>(true);
                if (srcAnim != null && dstAnim != null)
                {
                    dstAnim.runtimeAnimatorController = srcAnim.runtimeAnimatorController;
                    dstAnim.avatar = srcAnim.avatar;
                }
                return PrefabUtility.SaveAsPrefabAsset(inst, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(inst);
            }
        }

        private static void OptimizeMixamoSingleTexPrefab(string prefabPath, string unitKey, string texFileName, Color emissionLift)
        {
            if (!File.Exists(Path.GetFullPath(prefabPath))) return;
            Texture2D tex = ConfigureAndLoadXianxiaTexture(texFileName, 512);
            Material mat = CreateOrUpdateXianxiaMaterial($"{MatDir}/Mat_{unitKey}_Xianxia.mat", tex, Color.white, emissionLift, 0.16f);
            OptimizeMixamoPrefabInternal(prefabPath, unitKey, new[] { mat });
        }

        private static void OptimizeMixamoBossPrefab(string prefabPath, string unitKey, string bodyTexFile, string armorTexFile, Color emissionLift)
        {
            if (!File.Exists(Path.GetFullPath(prefabPath))) return;
            Texture2D bodyTex = ConfigureAndLoadXianxiaTexture(bodyTexFile, 512);
            Texture2D armorTex = ConfigureAndLoadXianxiaTexture(armorTexFile, 512);
            Material bodyMat = CreateOrUpdateXianxiaMaterial($"{MatDir}/Mat_{unitKey}_Body.mat", bodyTex, Color.white, emissionLift, 0.16f);
            Material armorMat = CreateOrUpdateXianxiaMaterial($"{MatDir}/Mat_{unitKey}_Armor.mat", armorTex, Color.white, emissionLift * 1.15f, 0.28f);
            OptimizeMixamoPrefabInternal(prefabPath, unitKey, new[] { bodyMat, armorMat });
        }

        private static void OptimizeMixamoPrefabInternal(string prefabPath, string unitKey, Material[] targetMats)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int meshIndex = 0;
                foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (smr.sharedMesh != null)
                    {
                        smr.sharedMesh = ForceCreateCleanMeshAsset(smr.sharedMesh, $"{unitKey}_clean_smr{meshIndex}");
                    }
                    var mats = smr.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                        mats[m] = targetMats[Mathf.Min(m, targetMats.Length - 1)];
                    smr.sharedMaterials = mats;
                    meshIndex++;
                }

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
                    }

                    if (anim.runtimeAnimatorController is AnimatorController srcCtrl &&
                        !AssetDatabase.GetAssetPath(srcCtrl).StartsWith(OptRoot))
                    {
                        var clonedCtrl = CloneAnimatorControllerLight(srcCtrl, unitKey);
                        if (clonedCtrl != null)
                            anim.runtimeAnimatorController = clonedCtrl;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureXianxiaModelImporter(string fbxPath)
        {
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.Generic)
            {
                imp.animationType = ModelImporterAnimationType.Generic;
                dirty = true;
            }
            if (!imp.importAnimation)
            {
                imp.importAnimation = true;
                dirty = true;
            }
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
                dirty = true;
            }
            if (dirty)
                imp.SaveAndReimport();
        }

        private static Texture2D ConfigureAndLoadXianxiaTexture(string texFileName, int maxSize)
        {
            string path = $"{TexDir}/{texFileName}";
            if (!File.Exists(Path.GetFullPath(path)))
                throw new InvalidOperationException($"Missing baked Xianxia texture: {path}");

            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                bool dirty = false;
                if (imp.textureType != TextureImporterType.Default) { imp.textureType = TextureImporterType.Default; dirty = true; }
                if (!imp.sRGBTexture) { imp.sRGBTexture = true; dirty = true; }
                if (imp.maxTextureSize != maxSize) { imp.maxTextureSize = maxSize; dirty = true; }
                if (!imp.mipmapEnabled) { imp.mipmapEnabled = true; dirty = true; }
                if (imp.textureCompression != TextureImporterCompression.Compressed) { imp.textureCompression = TextureImporterCompression.Compressed; dirty = true; }
                if (dirty) imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material CreateOrUpdateXianxiaMaterial(string matPath, Texture2D tex, Color baseTint, Color emissionLift, float smoothness)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(lit);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != lit)
            {
                mat.shader = lit;
            }

            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseTint);
            mat.color = baseTint;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.0f);

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", tex);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionLift);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.enableInstancing = true;

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CloneAndBindSkinnedMeshesAndMaterials(GameObject inst, string unitKey, Material[] mats)
        {
            int idx = 0;
            foreach (var smr in inst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh != null)
                {
                    smr.sharedMesh = ForceCreateCleanMeshAsset(smr.sharedMesh, $"{unitKey}_mesh{idx}");
                }
                var arr = new Material[Mathf.Max(1, smr.sharedMaterials.Length)];
                for (int i = 0; i < arr.Length; i++)
                    arr[i] = mats[Mathf.Min(i, mats.Length - 1)];
                smr.sharedMaterials = arr;
                smr.updateWhenOffscreen = true;
                idx++;
            }
        }

        private static Mesh ForceCreateCleanMeshAsset(Mesh srcMesh, string assetName)
        {
            string destPath = $"{MeshDir}/{assetName}.asset";
            var cloned = UnityEngine.Object.Instantiate(srcMesh);
            cloned.name = assetName;

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(destPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(cloned, existing);
                UnityEngine.Object.DestroyImmediate(cloned);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(cloned, destPath);
            return cloned;
        }

        private static Avatar CloneAvatarFromFbx(string fbxPath, string unitKey)
        {
            Avatar srcAv = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (o is Avatar av) { srcAv = av; break; }
            }
            if (srcAv == null) return null;

            string destPath = $"{AvatarDir}/{unitKey}_Avatar.asset";
            var cloned = UnityEngine.Object.Instantiate(srcAv);
            cloned.name = $"{unitKey}_Avatar";

            var existing = AssetDatabase.LoadAssetAtPath<Avatar>(destPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(cloned, existing);
                UnityEngine.Object.DestroyImmediate(cloned);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(cloned, destPath);
            return cloned;
        }

        private static AnimationClip ExtractAndConfigureClip(string fbxPath, string clipSuffix, string outClipName, bool loopTime)
        {
            AnimationClip found = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (o is AnimationClip c && !c.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    if (string.Equals(c.name, clipSuffix, StringComparison.OrdinalIgnoreCase) ||
                        c.name.EndsWith("|" + clipSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        found = c;
                        break;
                    }
                }
            }
            if (found == null)
                throw new InvalidOperationException($"AnimationClip '{clipSuffix}' not found in {fbxPath}");

            var cloned = UnityEngine.Object.Instantiate(found);
            cloned.name = outClipName;
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(cloned);
            settings.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(cloned, settings);

            string destPath = $"{AnimDir}/{outClipName}.anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(cloned, existing);
                UnityEngine.Object.DestroyImmediate(cloned);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(cloned, destPath);
            return cloned;
        }

        private static AnimatorController BuildPlayerAnimatorController(
            string ctrlPath,
            AnimationClip idleClip,
            AnimationClip castClip,
            AnimationClip tribClip,
            AnimationClip respawnClip)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (existing != null && existing.layers.Length > 0 && existing.layers[0].stateMachine.states.Length == 4)
            {
                foreach (var st in existing.layers[0].stateMachine.states)
                {
                    if (st.state.name == "Idle") st.state.motion = idleClip;
                    else if (st.state.name == "CastSkill") st.state.motion = castClip;
                    else if (st.state.name == "Tribulation") st.state.motion = tribClip;
                    else if (st.state.name == "Respawn") st.state.motion = respawnClip;
                }
                EditorUtility.SetDirty(existing);
                return existing;
            }
            if (File.Exists(Path.GetFullPath(ctrlPath)))
                AssetDatabase.DeleteAsset(ctrlPath);

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("CastSkill", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("InTribulation", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("IsRespawning", AnimatorControllerParameterType.Bool);

            var sm = ctrl.layers[0].stateMachine;
            var stIdle = sm.AddState("Idle", new Vector3(260, 120, 0));
            stIdle.motion = idleClip;
            sm.defaultState = stIdle;

            var stCast = sm.AddState("CastSkill", new Vector3(520, 20, 0));
            stCast.motion = castClip;

            var stTrib = sm.AddState("Tribulation", new Vector3(520, 140, 0));
            stTrib.motion = tribClip;

            var stRespawn = sm.AddState("Respawn", new Vector3(520, 250, 0));
            stRespawn.motion = respawnClip;

            var tAnyCast = sm.AddAnyStateTransition(stCast);
            tAnyCast.hasExitTime = false;
            tAnyCast.duration = 0.12f;
            tAnyCast.canTransitionToSelf = false;
            tAnyCast.AddCondition(AnimatorConditionMode.If, 0, "CastSkill");
            tAnyCast.AddCondition(AnimatorConditionMode.IfNot, 0, "InTribulation");
            tAnyCast.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRespawning");

            var tCastBack = stCast.AddTransition(stIdle);
            tCastBack.hasExitTime = true;
            tCastBack.exitTime = 0.90f;
            tCastBack.duration = 0.16f;

            var tToTrib = stIdle.AddTransition(stTrib);
            tToTrib.hasExitTime = false;
            tToTrib.duration = 0.22f;
            tToTrib.AddCondition(AnimatorConditionMode.If, 0, "InTribulation");

            var tCastToTrib = stCast.AddTransition(stTrib);
            tCastToTrib.hasExitTime = false;
            tCastToTrib.duration = 0.18f;
            tCastToTrib.AddCondition(AnimatorConditionMode.If, 0, "InTribulation");

            var tFromTrib = stTrib.AddTransition(stIdle);
            tFromTrib.hasExitTime = false;
            tFromTrib.duration = 0.24f;
            tFromTrib.AddCondition(AnimatorConditionMode.IfNot, 0, "InTribulation");

            var tToResp = stIdle.AddTransition(stRespawn);
            tToResp.hasExitTime = false;
            tToResp.duration = 0.18f;
            tToResp.AddCondition(AnimatorConditionMode.If, 0, "IsRespawning");

            var tCastToResp = stCast.AddTransition(stRespawn);
            tCastToResp.hasExitTime = false;
            tCastToResp.duration = 0.15f;
            tCastToResp.AddCondition(AnimatorConditionMode.If, 0, "IsRespawning");

            var tFromResp = stRespawn.AddTransition(stIdle);
            tFromResp.hasExitTime = false;
            tFromResp.duration = 0.24f;
            tFromResp.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRespawning");

            EditorUtility.SetDirty(ctrl);
            return ctrl;
        }

        private static AnimatorController BuildMonsterAnimatorController(
            string ctrlPath,
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip attackClip,
            AnimationClip dieClip)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (existing != null && existing.layers.Length > 0 && existing.layers[0].stateMachine.states.Length == 4)
            {
                foreach (var st in existing.layers[0].stateMachine.states)
                {
                    if (st.state.name == "Idle") st.state.motion = idleClip;
                    else if (st.state.name == "Move") st.state.motion = moveClip;
                    else if (st.state.name == "Attack") st.state.motion = attackClip;
                    else if (st.state.name == "Die") st.state.motion = dieClip;
                }
                EditorUtility.SetDirty(existing);
                return existing;
            }
            if (File.Exists(Path.GetFullPath(ctrlPath)))
                AssetDatabase.DeleteAsset(ctrlPath);

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var sm = ctrl.layers[0].stateMachine;
            var stIdle = sm.AddState("Idle", new Vector3(260, 120, 0));
            stIdle.motion = idleClip;
            sm.defaultState = stIdle;

            var stMove = sm.AddState("Move", new Vector3(520, 120, 0));
            stMove.motion = moveClip;

            var stAtk = sm.AddState("Attack", new Vector3(390, 10, 0));
            stAtk.motion = attackClip;

            var stDie = sm.AddState("Die", new Vector3(390, 240, 0));
            stDie.motion = dieClip;

            var tIdleToMove = stIdle.AddTransition(stMove);
            tIdleToMove.hasExitTime = false;
            tIdleToMove.duration = 0.12f;
            tIdleToMove.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");

            var tMoveToIdle = stMove.AddTransition(stIdle);
            tMoveToIdle.hasExitTime = false;
            tMoveToIdle.duration = 0.14f;
            tMoveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

            var tAnyAtk = sm.AddAnyStateTransition(stAtk);
            tAnyAtk.hasExitTime = false;
            tAnyAtk.duration = 0.08f;
            tAnyAtk.canTransitionToSelf = false;
            tAnyAtk.AddCondition(AnimatorConditionMode.If, 0, "Attack");

            var tAtkToMove = stAtk.AddTransition(stMove);
            tAtkToMove.hasExitTime = true;
            tAtkToMove.exitTime = 0.88f;
            tAtkToMove.duration = 0.12f;
            tAtkToMove.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");

            var tAtkToIdle = stAtk.AddTransition(stIdle);
            tAtkToIdle.hasExitTime = true;
            tAtkToIdle.exitTime = 0.88f;
            tAtkToIdle.duration = 0.12f;
            tAtkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");

            var tAnyDie = sm.AddAnyStateTransition(stDie);
            tAnyDie.hasExitTime = false;
            tAnyDie.duration = 0.08f;
            tAnyDie.canTransitionToSelf = false;
            tAnyDie.AddCondition(AnimatorConditionMode.If, 0, "Die");

            EditorUtility.SetDirty(ctrl);
            return ctrl;
        }

        public static void OptimizeHolotnaEnvironmentPrefabs()
        {
            EnsureFolders();
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
                    Mesh optMesh = GetOrCreateOptimizedEnvMesh(mf.sharedMesh, $"EnvMesh_{baseMeshKey}");
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

        private static Mesh GetOrCreateOptimizedEnvMesh(Mesh srcMesh, string key)
        {
            string srcPath = AssetDatabase.GetAssetPath(srcMesh);
            if (srcPath.StartsWith(OptRoot)) return srcMesh;

            string destPath = $"{MeshDir}/{key}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(destPath);
            if (existing != null) return existing;

            var cloned = UnityEngine.Object.Instantiate(srcMesh);
            cloned.name = key;
            AssetDatabase.CreateAsset(cloned, destPath);
            return cloned;
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

            var clipCache = new Dictionary<AnimationClip, AnimationClip>();
            foreach (var layer in cloned.layers)
                RemapStateMachineClips(layer.stateMachine, unitKey, clipCache);
            EditorUtility.SetDirty(cloned);
            return cloned;
        }

        private static void RemapStateMachineClips(AnimatorStateMachine sm, string unitKey, Dictionary<AnimationClip, AnimationClip> cache)
        {
            if (sm == null) return;
            foreach (var child in sm.states)
            {
                if (child.state?.motion is AnimationClip clip)
                    child.state.motion = CloneClipAsset(clip, unitKey, cache);
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

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_').Replace('|', '_');
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { OptRoot, ModelDir, MeshDir, TexDir, MatDir, AvatarDir, AnimDir, EnvDir })
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

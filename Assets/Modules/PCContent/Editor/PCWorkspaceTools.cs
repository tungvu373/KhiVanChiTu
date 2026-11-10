using System;
using System.IO;
using System.Linq;
using KhiVanChiTu.Collaboration;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace KhiVanChiTu.PCContent.Editor
{
    public static class PCWorkspaceTools
    {
        public const string Root = "Assets/Modules/PCContent";
        public const string AndroidScene = "Assets/Scenes/SampleScene_MobileWork.unity";
        public const string PCScene = Root + "/Scenes/PCGameplayWork.unity";
        public const string SandboxScene = Root + "/Scenes/ContentSandbox.unity";
        public const string ChunkPrefab = Root + "/Content/Opening/Prefabs/OpeningChunkTemplate.prefab";
        public const string ProfileId = "pc-workspace";

        [MenuItem("TuTien/Collaboration/1. Create PC Workspace (non-destructive)")]
        public static void CreateWorkspace()
        {
            RequireEditMode();
            foreach (Scene scene in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt))
                if (scene.isDirty) throw new InvalidOperationException("Save open scenes before creating the PC workspace.");
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EnsureFolder(Root + "/Scenes");
                EnsureFolder(Root + "/Content/Opening/Prefabs");
                EnsureFolder(Root + "/Content/Opening/Materials");
                if (AssetDatabase.LoadAssetAtPath<GameObject>(ChunkPrefab) == null) CreateChunkPrefab();
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PCScene) == null)
                {
                    if (!AssetDatabase.CopyAsset(AndroidScene, PCScene))
                        throw new InvalidOperationException("Could not copy the saved Android source scene.");
                    Scene pc = EditorSceneManager.OpenScene(PCScene, OpenSceneMode.Additive);
                    var marker = new GameObject("__PCWorkspace_SaveIsolation");
                    SceneManager.MoveGameObjectToScene(marker, pc);
                    marker.AddComponent<WorkspaceSaveProfile>().profileId = ProfileId;
                    var contentRoot = new GameObject("PCContent_IntegrationRoot");
                    SceneManager.MoveGameObjectToScene(contentRoot, pc);
                    // The source can be saved in direct-gameplay debug mode. Restore its
                    // existing menu only in the new PC copy so sessions load/save normally.
                    foreach (GameObject root in pc.GetRootGameObjects())
                        foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                            if (component != null && component.GetType().Name == "MainMenuManager")
                                for (Transform item = component.transform; item != null; item = item.parent)
                                    item.gameObject.SetActive(true);
                    EditorSceneManager.SaveScene(pc);
                    EditorSceneManager.CloseScene(pc, true);
                }
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SandboxScene) == null) CreateSandbox();
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
            ValidateWorkspace();
        }

        [MenuItem("TuTien/Collaboration/2. Open PC Gameplay")]
        public static void OpenPC()
        {
            RequireEditMode();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(PCScene, OpenSceneMode.Single);
        }

        [MenuItem("TuTien/Collaboration/3. Open Content Sandbox")]
        public static void OpenSandbox()
        {
            RequireEditMode();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(SandboxScene, OpenSceneMode.Single);
        }

        [MenuItem("TuTien/Collaboration/4. Validate PC Workspace")]
        public static void ValidateWorkspace()
        {
            RequireEditMode();
            foreach (Scene scene in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt))
                if (scene.isDirty) throw new InvalidOperationException("Save open scenes before validating the PC workspace.");
            if (AssetDatabase.AssetPathToGUID(AndroidScene) == AssetDatabase.AssetPathToGUID(PCScene))
                throw new InvalidOperationException("PC and Android scene GUIDs must differ.");
            if (EditorBuildSettings.scenes.Any(s => s.path == PCScene || s.path == SandboxScene))
                throw new InvalidOperationException("Workspace scenes must not be in the Android global build list.");
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene pc = OpenForValidation(PCScene);
                CheckMissingScripts(pc);
                if (WorkspaceSaveProfile.Resolve(pc) != ProfileId)
                    throw new InvalidOperationException("PC scene save isolation is missing or has been changed.");
                Scene sandbox = OpenForValidation(SandboxScene);
                CheckMissingScripts(sandbox);
                foreach (GameObject root in sandbox.GetRootGameObjects())
                    foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                        if (component != null && component.GetType().Assembly.GetName().Name == "Assembly-CSharp")
                            throw new InvalidOperationException("Sandbox contains a shared gameplay component: " + component.GetType().Name);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChunkPrefab);
                if (prefab == null || prefab.GetComponent<ContentChunk>() == null)
                    throw new InvalidOperationException("Content chunk template is missing.");
                foreach (MonoBehaviour component in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                    if (component == null || !(component is ContentChunk))
                        throw new InvalidOperationException("Content prefab may only contain the ContentChunk authoring behaviour.");
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
            Debug.Log("[PCWorkspace] PASS: independent scene GUIDs, isolated save, no missing scripts, clean sandbox, Android build list preserved.");
        }

        [MenuItem("TuTien/Collaboration/5. Build Windows Development")]
        public static void BuildWindows()
        {
            BuildWindowsAtPath(Path.GetFullPath("Builds/PCContent/KhiVanChiTu_PC.exe"));
        }

        public static void BuildWindowsAtPath(string outputPath)
        {
            RequireEditMode();
            ValidateWorkspace();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                throw new InvalidOperationException("Use the collaborator's checkout with Windows active. This tool does not switch the Android checkout's build target.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Install Windows build support for Unity 6000.6.4f1.");
            if (File.Exists(outputPath)) throw new IOException("Output already exists. Choose a new build directory: " + outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { PCScene },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result);
            Debug.Log("[PCWorkspace] Windows development build: " + outputPath);
        }

        private static void CreateChunkPrefab()
        {
            string materialPath = Root + "/Content/Opening/Materials/OpeningGround.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
                material = new Material(shader) { color = new Color(0.18f, 0.28f, 0.23f) };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var root = new GameObject("OpeningChunkTemplate");
            try
            {
                root.AddComponent<ContentChunk>();
                CreateBlock(root.transform, "Ground_24x80", new Vector3(0, -0.25f, 40), new Vector3(24, 0.5f, 80), material);
                for (int i = 0; i < 4; i++)
                {
                    CreateBlock(root.transform, "LeftMarker_" + i, new Vector3(-10, 1, 10 + i * 20), new Vector3(1, 2, 1), material);
                    CreateBlock(root.transform, "RightMarker_" + i, new Vector3(10, 1, 10 + i * 20), new Vector3(1, 2, 1), material);
                }
                PrefabUtility.SaveAsPrefabAsset(root, ChunkPrefab);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateSandbox()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var environment = new GameObject("EnvironmentPreview");
            var loop = environment.AddComponent<ContentPreviewLoop>();
            loop.chunks = new ContentChunk[2];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChunkPrefab);
            for (int i = 0; i < 2; i++)
            {
                var chunk = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                chunk.name = "OpeningChunk_" + (i + 1);
                chunk.transform.SetParent(environment.transform, false);
                chunk.transform.localPosition = new Vector3(0, 0, i * 80f);
                loop.chunks[i] = chunk.GetComponent<ContentChunk>();
            }
            var cameraObject = new GameObject("Sandbox Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(24, 32, -18);
            cameraObject.transform.LookAt(new Vector3(0, 0, 45));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.09f, 0.14f);
            camera.farClipPlane = 400;
            var light = new GameObject("Sandbox Key Light", typeof(Light));
            light.GetComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
            EditorSceneManager.SaveScene(scene, SandboxScene);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static Scene OpenForValidation(string path)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            if (scene.IsValid() && scene.isLoaded) return scene;
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        private static void CheckMissingScripts(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) != 0)
                        throw new InvalidOperationException("Missing script in " + scene.path + ": " + item.name);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before workspace authoring or building.");
        }
    }
}

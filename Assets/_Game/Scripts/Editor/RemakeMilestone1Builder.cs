using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake.Editor
{
    public static class RemakeMilestone1Builder
    {
        private const string ScenesRoot = "Assets/_Game/Scenes";
        private const string PrefabsRoot = "Assets/_Game/Prefabs";
        private const string DataRoot = "Assets/_Game/Data";
        private const string ResourcesRoot = "Assets/_Game/Resources";
        private const string DefaultArtRoot = DataRoot + "/MeadowDefaultArt";

        [MenuItem("KhiVan/Remake/Build All Milestone 1 Scenes & Assets")]
        public static void BuildAllMilestone1()
        {
            EnsureFolders();
            CodexSeamlessAndSliceTool.BakeAll(false);
            CodexArtImportPostprocessor.RebuildArtRegistry();

            var audioBank = CreateOrUpdateAudioBank();
            var fontAsset = CreateOrUpdateVietnameseFontAsset();
            var theme = CreateOrUpdateThemeStyleSheet();
            var panelSettings = CreateOrUpdatePanelSettings(fontAsset, theme);

            var defaultArt = CreateOrUpdateDefaultMeadowArt();
            var volumeProfile = CreateOrUpdateDaylightVolumeProfile();

            var flyingSwordPrefab = CreateFlyingSwordPrefab();
            CloneCleanPlayerPrefab(PrefabsRoot + "/Player/RemakePlayer.prefab");
            var act1Monsters = CreateAct1Monsters();
            CloneCleanModelPrefab("Assets/Prefabs/Enemy1.prefab", PrefabsRoot + "/Monsters/Monster_Act2_Bear.prefab");
            CloneCleanModelPrefab("Assets/Prefabs/Enemy2.prefab", PrefabsRoot + "/Monsters/Monster_Act3_ThunderSpirit.prefab");

            // Optimize meshes (589K->6K tris), downscale textures (2K->256/512px) and detach FBX refs for WebGL
            WebGlPrefabOptimizerTool.OptimizeAllGamePrefabs();

            var act2Monsters = CreateAct2Monsters(act1Monsters);
            var act3Monsters = CreateAct3Monsters(act1Monsters);
            var stageDb = CreateActStageDatabase(act1Monsters, act2Monsters, act3Monsters);

            // Flush all newly created ScriptableObjects/Materials/Font/PanelSettings to disk BEFORE NewScene serializes scene references!
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BuildBootScene(panelSettings, audioBank);
            BuildMainMenuScene(panelSettings);
            BuildUiOverlayScene(panelSettings);
            string[] act1Paths =
            {
                DataRoot + "/Monsters/Act1_Wolf.asset",
                DataRoot + "/Monsters/Act1_Bird.asset",
                DataRoot + "/Monsters/Act1_Turtle.asset",
                DataRoot + "/Monsters/Act1_Boss_Guardian.asset"
            };
            string[] act2Paths =
            {
                DataRoot + "/Monsters/Act2_Scorpion.asset",
                DataRoot + "/Monsters/Act2_Bear.asset",
                DataRoot + "/Monsters/Act2_SandTurtle.asset",
                DataRoot + "/Monsters/Act2_Boss_Overlord.asset"
            };
            string[] act3Paths =
            {
                DataRoot + "/Monsters/Act3_ThunderSpirit.asset",
                DataRoot + "/Monsters/Act3_JadeCrane.asset",
                DataRoot + "/Monsters/Act3_JadeTurtle.asset",
                DataRoot + "/Monsters/Act3_Boss_Archon.asset"
            };
            BuildWorldActScene(0, defaultArt, volumeProfile, flyingSwordPrefab, act1Paths);
            BuildWorldActScene(1, defaultArt, volumeProfile, flyingSwordPrefab, act2Paths);
            BuildWorldActScene(2, defaultArt, volumeProfile, flyingSwordPrefab, act3Paths);
            BuildChallengeArenaScene(MeadowEnvironmentController.WorldThemePreset.DemonTower, ScenesRoot + "/05_World_DemonTower.unity", defaultArt, volumeProfile, flyingSwordPrefab, act3Paths);
            BuildChallengeArenaScene(MeadowEnvironmentController.WorldThemePreset.SecretSwordTomb, ScenesRoot + "/06_World_Secret_SwordTomb.unity", defaultArt, volumeProfile, flyingSwordPrefab, act3Paths);
            BuildChallengeArenaScene(MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden, ScenesRoot + "/07_World_Secret_HerbGarden.unity", defaultArt, volumeProfile, flyingSwordPrefab, act1Paths);
            BuildChallengeArenaScene(MeadowEnvironmentController.WorldThemePreset.SecretOreCavern, ScenesRoot + "/08_World_Secret_OreCavern.unity", defaultArt, volumeProfile, flyingSwordPrefab, act2Paths);

            CreateOrUpdateBuildProfile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // Always return active scene to 00_Boot so pressing Play in Unity Editor boots full HUD+Systems cleanly
            EditorSceneManager.OpenScene(ScenesRoot + "/00_Boot.unity", OpenSceneMode.Single);
            Debug.Log("[RemakeM1Builder] Built all scenes (Boot, Menu, UI, Act 1-3, DemonTower, 3 Secret Realms), optimized WebGL prefabs, and 120 stages cleanly.");
        }

        [MenuItem("KhiVan/Remake/Build Local WebGL (Builds/LocalWeb)")]
        public static void BuildLocalWebGl()
        {
            BuildAllMilestone1();
            string outDir = Path.GetFullPath("Builds/LocalWeb");
            Directory.CreateDirectory(outDir);

            bool prevRunInBackground = PlayerSettings.runInBackground;
            try
            {
                // Configure WebGL settings without leaving global runInBackground altered
                PlayerSettings.WebGL.template = "PROJECT:KhiVanDaylight";
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.initialMemorySize = 64;
                PlayerSettings.WebGL.maximumMemorySize = 256;
                PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
                PlayerSettings.runInBackground = true;

                var scenes = new[]
                {
                    ScenesRoot + "/00_Boot.unity",
                    ScenesRoot + "/01_MainMenu.unity",
                    ScenesRoot + "/02_World_Act1_Meadow.unity",
                    ScenesRoot + "/03_World_Act2_Wasteland.unity",
                    ScenesRoot + "/04_World_Act3_JadePeaks.unity",
                    ScenesRoot + "/05_World_DemonTower.unity",
                    ScenesRoot + "/06_World_Secret_SwordTomb.unity",
                    ScenesRoot + "/07_World_Secret_HerbGarden.unity",
                    ScenesRoot + "/08_World_Secret_OreCavern.unity",
                    ScenesRoot + "/99_UI_Overlay.unity"
                };

                var opts = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outDir,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(opts);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new InvalidOperationException($"WebGL build failed: {report.summary.result} (errors={report.summary.totalErrors})");
                }
                Debug.Log($"[RemakeM1Builder] WebGL LAN build succeeded -> {outDir} ({report.summary.totalSize / (1024f * 1024f):0.2} MB)");
            }
            finally
            {
                PlayerSettings.runInBackground = prevRunInBackground;
            }
        }

        private static void EnsureFolders()
        {
            foreach (string f in new[]
            {
                ScenesRoot,
                PrefabsRoot + "/Player",
                PrefabsRoot + "/Monsters",
                PrefabsRoot + "/Bosses",
                DataRoot + "/Monsters",
                DataRoot + "/BuildProfiles",
                DefaultArtRoot,
                "Assets/_Game/UI/Themes",
                ResourcesRoot + "/CodexDrop"
            })
            {
                EnsureFolder(f);
            }
        }

        private static RemakeAudioBankSO CreateOrUpdateAudioBank()
        {
            string path = ResourcesRoot + "/RemakeAudioBank.asset";
            var bank = AssetDatabase.LoadAssetAtPath<RemakeAudioBankSO>(path);
            if (bank == null)
            {
                if (File.Exists(path))
                    AssetDatabase.DeleteAsset(path);
                bank = ScriptableObject.CreateInstance<RemakeAudioBankSO>();
                AssetDatabase.CreateAsset(bank, path);
            }
            bank.menuBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/nhạcmenu.mp3");
            bank.gameBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/NhạcNềnInGame.mp3");
            bank.clickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/click.mp3");
            bank.swordHitSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/sword.mp3");
            bank.swordMergeSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/mergesword.mp3");
            bank.loiPhatSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/LoiPhat.mp3");
            bank.breakthroughSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/bang.mp3");
            EditorUtility.SetDirty(bank);
            return bank;
        }

        private static FontAsset CreateOrUpdateVietnameseFontAsset()
        {
            string assetPath = ResourcesRoot + "/RemakeFontSDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<FontAsset>(assetPath);
            if (existing != null) return existing;

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/SourceSansPro-Regular.ttf");
            if (sourceFont == null)
                sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf");

            if (sourceFont == null)
                throw new InvalidOperationException("SourceSansPro Vietnamese TTF font not found.");

            FontAsset fontAsset = FontAsset.CreateFontAsset(sourceFont);
            fontAsset.name = "RemakeFontSDF";
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            // Save atlas texture & material as subassets so Dynamic font atlas works in builds
            if (fontAsset.material != null)
            {
                fontAsset.material.name = "RemakeFontSDF_Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = "RemakeFontSDF_Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }
            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }

        private static ThemeStyleSheet CreateOrUpdateThemeStyleSheet()
        {
            string path = "Assets/_Game/UI/Themes/RemakeDefaultTheme.tss";
            var existing = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(path);
            if (existing != null) return existing;

            File.WriteAllText(path, "@import url(\"unity-theme://default\");\n");
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(path);
        }

        private static PanelSettings CreateOrUpdatePanelSettings(FontAsset fontAsset, ThemeStyleSheet theme)
        {
            string path = ResourcesRoot + "/RemakePanelSettings.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(ps, path);
            }

            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(540, 960);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.referenceDpi = 96f;
            ps.fallbackDpi = 96f;
            if (theme != null) ps.themeStyleSheet = theme;

            string textSettingsPath = "Assets/_Game/UI/Themes/RemakePanelTextSettings.asset";
            var ts = AssetDatabase.LoadAssetAtPath<PanelTextSettings>(textSettingsPath);
            if (ts == null)
            {
                ts = ScriptableObject.CreateInstance<PanelTextSettings>();
                AssetDatabase.CreateAsset(ts, textSettingsPath);
            }

            // Bind SourceSansPro Vietnamese FontAsset cleanly as default font
            var defaultFontProp = typeof(TextSettings).GetProperty("defaultFontAsset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            defaultFontProp?.SetValue(ts, fontAsset);
            EditorUtility.SetDirty(ts);

            ps.textSettings = ts;
            EditorUtility.SetDirty(ps);
            return ps;
        }

        private sealed class MeadowDefaultAssets
        {
            public Texture2D grassTex;
            public Texture2D dirtTex;
            public Texture2D skyTex;
            public Material grassMat;
            public Material dirtMat;
            public Material skyMat;
            public Material rockMat;
            public Material leafMat;
            public Material trunkMat;
            public Material propBushTreeMat;
            public Material propGrassTuftMat;
            public Material groundRingMat;
            public Material desertSandMat;
            public Material jadeCliffMat;
            public Material desertRockMat;
            public Material jadeSpireMat;
            public Material towerFloorMat;
            public Material swordTombFloorMat;
            public Material herbGardenFloorMat;
            public Material oreCavernFloorMat;
            public Material towerPropMat;
            public Material swordTombPropMat;
            public Material herbGardenPropMat;
            public Material oreCavernPropMat;
            public Material towerAccentMat;
            public Material swordAccentMat;
            public Material oreAccentMat;
            public Mesh groundChunkMesh;
        }

        private static MeadowDefaultAssets CreateOrUpdateDefaultMeadowArt()
        {
            var res = new MeadowDefaultAssets();
            res.grassTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_grass_meadow.png", 256, 256, false, (x, y, w, h) =>
            {
                float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.65f + Mathf.PerlinNoise(x * 0.25f + 13f, y * 0.25f + 7f) * 0.35f;
                Color c1 = new Color(0.31f, 0.72f, 0.24f, 1f);
                Color c2 = new Color(0.46f, 0.84f, 0.30f, 1f);
                return Color.Lerp(c1, c2, n);
            });

            res.dirtTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_dirt_patch.png", 128, 128, true, (x, y, w, h) =>
            {
                float nx = (x / (float)(w - 1)) * 2f - 1f;
                float ny = (y / (float)(h - 1)) * 2f - 1f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float noise = Mathf.PerlinNoise(x * 0.1f + 3f, y * 0.1f + 9f) * 0.22f;
                float alpha = Mathf.Clamp01((0.88f - (dist - noise)) * 3.2f);
                return new Color(0.79f, 0.69f, 0.48f, alpha);
            });

            res.skyTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_sky_daylight.png", 512, 256, false, (x, y, w, h) =>
            {
                float v = y / (float)(h - 1); // 0 bottom -> 1 top
                Color horizon = new Color(0.76f, 0.92f, 1.00f, 1f);
                Color zenith = new Color(0.34f, 0.72f, 0.98f, 1f);
                Color c = Color.Lerp(horizon, zenith, Mathf.SmoothStep(0.38f, 0.95f, v));

                // Soft mountain silhouette along horizon
                float mH = 0.42f + Mathf.Sin(x * 0.025f) * 0.045f + Mathf.PerlinNoise(x * 0.03f, 2f) * 0.055f;
                if (v < mH)
                {
                    c = Color.Lerp(new Color(0.74f, 0.89f, 0.92f), new Color(0.54f, 0.76f, 0.88f), Mathf.Clamp01((mH - v) * 8f));
                }

                // Soft sunlit clouds
                float cloud = Mathf.PerlinNoise(x * 0.025f + 5f, y * 0.06f + 1f);
                if (v > 0.50f && v < 0.86f && cloud > 0.62f)
                {
                    float blend = Mathf.Clamp01((cloud - 0.62f) * 3.2f);
                    c = Color.Lerp(c, Color.white, blend * 0.85f);
                }
                return c;
            });

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

            res.grassMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_MeadowGrass.mat", urpLit, m =>
            {
                m.mainTexture = res.grassTex;
                m.mainTextureScale = new Vector2(6f, 6f);
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_Smoothness", 0.08f);
                m.enableInstancing = true;
            });

            res.dirtMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_DirtPatch.mat", urpUnlit, m =>
            {
                m.mainTexture = res.dirtTex;
                m.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.72f));
                m.SetFloat("_Surface", 1f); // Transparent
                m.renderQueue = 2500;
            });

            res.skyMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_DaylightSky.mat", urpUnlit, m =>
            {
                m.mainTexture = res.skyTex;
                m.SetColor("_BaseColor", Color.white);
                m.SetInt("_Cull", (int)CullMode.Off);
            });

            res.rockMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_MeadowRock.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.62f, 0.61f, 0.54f));
                m.SetFloat("_Smoothness", 0.15f);
                m.enableInstancing = true;
            });

            res.leafMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_MeadowFoliage.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.28f, 0.68f, 0.25f));
                m.SetFloat("_Smoothness", 0.12f);
                m.enableInstancing = true;
            });

            res.trunkMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_TreeTrunk.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.45f, 0.34f, 0.23f));
                m.SetFloat("_Smoothness", 0.1f);
                m.enableInstancing = true;
            });

            var bushTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/CodexDrop/Map/prop_bush_tree_stylized.png");
            res.propBushTreeMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_PropBushTree.mat", urpLit, m =>
            {
                m.mainTexture = bushTex;
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_AlphaClip", 1f);
                m.SetFloat("_Cutoff", 0.45f);
                m.SetFloat("_Smoothness", 0.05f);
                m.SetInt("_Cull", (int)CullMode.Off);
                m.EnableKeyword("_ALPHATEST_ON");
                m.renderQueue = 2450;
                m.enableInstancing = true;
            });

            var tuftTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/CodexDrop/Map/prop_grass_tuft.png");
            res.propGrassTuftMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_PropGrassTuft.mat", urpLit, m =>
            {
                m.mainTexture = tuftTex;
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_AlphaClip", 1f);
                m.SetFloat("_Cutoff", 0.45f);
                m.SetFloat("_Smoothness", 0.05f);
                m.SetInt("_Cull", (int)CullMode.Off);
                m.EnableKeyword("_ALPHATEST_ON");
                m.renderQueue = 2450;
                m.enableInstancing = true;
            });

            var ringTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/CodexDrop/Formations/formation_ground_ring_light.png");
            res.groundRingMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_GroundFormationRing.mat", urpUnlit, m =>
            {
                m.mainTexture = ringTex;
                m.SetColor("_BaseColor", new Color(1f, 0.96f, 0.80f, 0.72f));
                m.SetFloat("_Surface", 1f);
                m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.SetInt("_Cull", (int)CullMode.Off);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 2600;
            });

            var sandTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_act2_sand_seamless.png") ?? res.grassTex;
            res.desertSandMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_Act2Sand.mat", urpLit, m =>
            {
                m.mainTexture = sandTex;
                m.mainTextureScale = new Vector2(6f, 6f);
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_Smoothness", 0.08f);
                m.enableInstancing = true;
            });

            var jadeTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_act3_jade_seamless.png") ?? res.grassTex;
            res.jadeCliffMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_Act3Jade.mat", urpLit, m =>
            {
                m.mainTexture = jadeTex;
                m.mainTextureScale = new Vector2(6f, 6f);
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_Smoothness", 0.14f);
                m.enableInstancing = true;
            });

            res.desertRockMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_DesertSandstone.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.84f, 0.65f, 0.40f));
                m.SetFloat("_Smoothness", 0.12f);
                m.enableInstancing = true;
            });

            res.jadeSpireMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_JadeSpire.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.76f, 0.94f, 0.89f));
                m.SetFloat("_Smoothness", 0.28f);
                m.enableInstancing = true;
            });

            var towerFloorTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_tower_floor.png", 256, 256, false, (x, y, w, h) =>
            {
                bool seam = (x % 64 < 3) || (y % 64 < 3);
                float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                Color stone = Color.Lerp(new Color(0.84f, 0.86f, 0.82f), new Color(0.94f, 0.92f, 0.86f), n);
                return seam ? new Color(0.76f, 0.64f, 0.34f, 1f) : stone;
            });
            var swordFloorTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_sword_tomb_floor.png", 256, 256, false, (x, y, w, h) =>
            {
                bool crack = ((x + y) % 96 < 2);
                float n = Mathf.PerlinNoise(x * 0.07f + 4f, y * 0.07f + 2f);
                Color slate = Color.Lerp(new Color(0.76f, 0.84f, 0.82f), new Color(0.88f, 0.92f, 0.89f), n);
                return crack ? new Color(0.95f, 0.86f, 0.45f, 1f) : slate;
            });
            var herbFloorTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_herb_garden_floor.png", 256, 256, false, (x, y, w, h) =>
            {
                float n = Mathf.PerlinNoise(x * 0.09f + 7f, y * 0.09f + 3f);
                float pebble = Mathf.PerlinNoise(x * 0.22f, y * 0.22f);
                Color lush = Color.Lerp(new Color(0.28f, 0.76f, 0.46f), new Color(0.50f, 0.88f, 0.56f), n);
                return pebble > 0.78f ? new Color(0.90f, 0.94f, 0.88f, 1f) : lush;
            });
            var oreFloorTex = GenerateOrLoadTexture(DefaultArtRoot + "/default_ore_cavern_floor.png", 256, 256, false, (x, y, w, h) =>
            {
                float n = Mathf.PerlinNoise(x * 0.08f + 11f, y * 0.08f + 5f);
                float vein = Mathf.PerlinNoise(x * 0.16f + 2f, y * 0.16f + 9f);
                Color rock = Color.Lerp(new Color(0.78f, 0.79f, 0.76f), new Color(0.89f, 0.86f, 0.80f), n);
                return vein > 0.76f ? new Color(0.32f, 0.76f, 0.92f, 1f) : rock;
            });

            res.towerFloorMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_DemonTowerFloor.mat", urpLit, m =>
            {
                m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_tower_floor_seamless.png") ?? towerFloorTex;
                m.mainTextureScale = new Vector2(7f, 7f);
                m.SetColor("_BaseColor", Color.white); m.SetFloat("_Smoothness", 0.22f); m.enableInstancing = true;
            });
            res.swordTombFloorMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_SecretSwordFloor.mat", urpLit, m =>
            {
                m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_secret_sword_seamless.png") ?? swordFloorTex;
                m.mainTextureScale = new Vector2(7f, 7f);
                m.SetColor("_BaseColor", Color.white); m.SetFloat("_Smoothness", 0.14f); m.enableInstancing = true;
            });
            res.herbGardenFloorMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_SecretHerbFloor.mat", urpLit, m =>
            {
                m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_secret_herb_seamless.png") ?? herbFloorTex;
                m.mainTextureScale = new Vector2(6f, 6f);
                m.SetColor("_BaseColor", Color.white); m.SetFloat("_Smoothness", 0.10f); m.enableInstancing = true;
            });
            res.oreCavernFloorMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_SecretOreFloor.mat", urpLit, m =>
            {
                m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/BakedMaps/tex_secret_ore_seamless.png") ?? oreFloorTex;
                m.mainTextureScale = new Vector2(6f, 6f);
                m.SetColor("_BaseColor", Color.white); m.SetFloat("_Smoothness", 0.24f); m.enableInstancing = true;
            });

            Material MakeCodexPropMat(string matName, string codexRelPath)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/CodexDrop/" + codexRelPath);
                return CreateOrUpdateMaterial(DefaultArtRoot + "/" + matName, urpLit, m =>
                {
                    m.mainTexture = tex;
                    m.SetColor("_BaseColor", Color.white);
                    m.SetFloat("_AlphaClip", 1f);
                    m.SetFloat("_Cutoff", 0.42f);
                    m.SetFloat("_Smoothness", 0.08f);
                    m.SetInt("_Cull", (int)CullMode.Off);
                    m.EnableKeyword("_ALPHATEST_ON");
                    m.renderQueue = 2450;
                    m.enableInstancing = true;
                });
            }

            res.towerPropMat = MakeCodexPropMat("Mat_PropTowerPillar.mat", "Map/prop_tower_pillar.png");
            res.swordTombPropMat = MakeCodexPropMat("Mat_PropAncientSword.mat", "Map/prop_giant_ancient_sword.png");
            res.herbGardenPropMat = MakeCodexPropMat("Mat_PropSpiritHerb.mat", "Map/prop_spirit_herb_cluster.png");
            res.oreCavernPropMat = MakeCodexPropMat("Mat_PropCrystalVein.mat", "Map/prop_crystal_vein.png");

            res.towerAccentMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_TowerJadePillar.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.92f, 0.87f, 0.72f)); m.SetFloat("_Smoothness", 0.30f); m.enableInstancing = true;
            });
            res.swordAccentMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_AncientSwordBlade.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.76f, 0.88f, 0.90f)); m.SetFloat("_Smoothness", 0.45f); m.enableInstancing = true;
            });
            res.oreAccentMat = CreateOrUpdateMaterial(DefaultArtRoot + "/Mat_CrystalOreCluster.mat", urpLit, m =>
            {
                m.SetColor("_BaseColor", new Color(0.32f, 0.82f, 0.96f)); m.SetFloat("_Smoothness", 0.55f); m.enableInstancing = true;
            });

            res.groundChunkMesh = CreateOrUpdateGroundMesh(DefaultArtRoot + "/MeadowGroundChunkMesh.asset");
            return res;
        }

        private static Mesh CreateOrUpdateGroundMesh(string path)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool isNew = mesh == null;
            if (isNew) mesh = new Mesh { name = "MeadowGroundChunkMesh" };
            else mesh.Clear();

            const int gx = 8;
            const int gz = 8;
            const float width = 48f;
            const float length = 30f;

            var verts = new Vector3[(gx + 1) * (gz + 1)];
            var uvs = new Vector2[verts.Length];
            var tris = new int[gx * gz * 6];

            for (int z = 0; z <= gz; z++)
            {
                for (int x = 0; x <= gx; x++)
                {
                    int idx = z * (gx + 1) + x;
                    float px = (x / (float)gx - 0.5f) * width;
                    float pz = (z / (float)gz - 0.5f) * length;
                    // Keep central battleground [-11m..+11m] completely flat at Y=0, lift far flanks softly
                    float tEdge = Mathf.InverseLerp(11f, 24f, Mathf.Abs(px));
                    float edgeFactor = Mathf.SmoothStep(0f, 1f, tEdge);
                    float py = edgeFactor * (0.95f + Mathf.Sin(pz * 0.28f) * 0.22f);
                    verts[idx] = new Vector3(px, py, pz);
                    uvs[idx] = new Vector2(x / (float)gx, z / (float)gz);
                }
            }

            int ti = 0;
            for (int z = 0; z < gz; z++)
            {
                for (int x = 0; x < gx; x++)
                {
                    int a = z * (gx + 1) + x;
                    int b = a + 1;
                    int c = a + gx + 1;
                    int d = c + 1;
                    tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                    tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
                }
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (isNew) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Texture2D GenerateOrLoadTexture(string path, int w, int h, bool repeatAlpha, Func<int, int, int, int, Color> pixelFn)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = pixelFn(x, y, w, h);
            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp != null)
            {
                imp.alphaIsTransparency = repeatAlpha;
                imp.wrapMode = repeatAlpha ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                imp.mipmapEnabled = !repeatAlpha;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material CreateOrUpdateMaterial(string path, Shader shader, Action<Material> config)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }
            config?.Invoke(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static VolumeProfile CreateOrUpdateDaylightVolumeProfile()
        {
            string path = DataRoot + "/DaylightVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile != null && profile.components != null && profile.components.Exists(c => c == null))
            {
                AssetDatabase.DeleteAsset(path);
                profile = null;
            }
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            if (!profile.TryGet(out Bloom bloom))
                bloom = profile.Add<Bloom>(true);
            bloom.hideFlags = HideFlags.HideInHierarchy;
            if (!AssetDatabase.Contains(bloom))
                AssetDatabase.AddObjectToAsset(bloom, profile);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.05f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.20f;
            EditorUtility.SetDirty(bloom);

            if (!profile.TryGet(out Tonemapping tone))
                tone = profile.Add<Tonemapping>(true);
            tone.hideFlags = HideFlags.HideInHierarchy;
            if (!AssetDatabase.Contains(tone))
                AssetDatabase.AddObjectToAsset(tone, profile);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.Neutral;
            EditorUtility.SetDirty(tone);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static RemakeFlyingSwordActor CreateFlyingSwordPrefab()
        {
            string path = PrefabsRoot + "/Player/RemakeFlyingSword.prefab";
            var temp = new GameObject("RemakeFlyingSword");
            try
            {
                var trail = temp.AddComponent<TrailRenderer>();
                trail.time = 0.16f;
                trail.minVertexDistance = 0.08f;

                var visual = new GameObject("Visual");
                visual.transform.SetParent(temp.transform, false);
                visual.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
                visual.transform.localRotation = Quaternion.Euler(90f, 0f, -45f);
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/GameIcons/Art/Sword01.png");

                var actor = temp.AddComponent<RemakeFlyingSwordActor>();
                var prefabGo = PrefabUtility.SaveAsPrefabAsset(temp, path);
                return prefabGo.GetComponent<RemakeFlyingSwordActor>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temp);
            }
        }

        private static MonsterDefinitionSO[] CreateAct1Monsters()
        {
            var wolfPrefab = CloneCleanModelPrefab("Assets/Prefabs/sói.prefab", PrefabsRoot + "/Monsters/Monster_Act1_Wolf.prefab");
            var birdPrefab = CloneCleanModelPrefab("Assets/Prefabs/chim.prefab", PrefabsRoot + "/Monsters/Monster_Act1_Bird.prefab");
            var turtlePrefab = CloneCleanModelPrefab("Assets/Prefabs/rùa.prefab", PrefabsRoot + "/Monsters/Monster_Act1_Turtle.prefab");
            var bossPrefab = CloneCleanModelPrefab("Assets/Prefabs/Boss.prefab", PrefabsRoot + "/Bosses/Boss_Act1_Guardian.prefab");

            var wolfDef = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act1_Wolf.asset", d =>
            {
                d.monsterId = "act1_wolf";
                d.displayName = "Thanh Phong Lang";
                d.description = "Yêu thú đồng cỏ tốc độ cao, hay áp sát thành bầy.";
                d.archetype = RemakeMonsterArchetype.FastMelee;
                d.codexPortraitKey = "Monsters/act1_wolf";
                d.prefab = wolfPrefab;
                d.hpMultiplier = 0.90f;
                d.damageMultiplier = 1.0f;
                d.moveSpeed = 2.9f;
                d.hoverHeight = 0f;
                d.visualScale = 1.0f;
                d.hpBarHeight = 2.0f;
                d.isBoss = false;
            });

            var birdDef = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act1_Bird.asset", d =>
            {
                d.monsterId = "act1_bird";
                d.displayName = "Linh Vũ Điểu";
                d.description = "Phi cầm linh khí nhẹ, bay lơ lửng tầm trung.";
                d.archetype = RemakeMonsterArchetype.FlyingAgile;
                d.codexPortraitKey = "Monsters/act1_bird";
                d.prefab = birdPrefab;
                d.hpMultiplier = 0.85f;
                d.damageMultiplier = 0.95f;
                d.moveSpeed = 3.2f;
                d.hoverHeight = 1.15f;
                d.visualScale = 1.0f;
                d.hpBarHeight = 2.2f;
                d.isBoss = false;
            });

            var turtleDef = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act1_Turtle.asset", d =>
            {
                d.monsterId = "act1_turtle";
                d.displayName = "Huyền Giáp Quy";
                d.description = "Giáp dày chống chịu cao, thưởng Tu Vi khá.";
                d.archetype = RemakeMonsterArchetype.ArmoredTank;
                d.codexPortraitKey = "Monsters/act1_turtle";
                d.prefab = turtlePrefab;
                d.hpMultiplier = 1.45f;
                d.damageMultiplier = 0.85f;
                d.moveSpeed = 1.85f;
                d.armorMitigation = 0.22f;
                d.tuViMultiplier = 1.25f;
                d.hoverHeight = 0f;
                d.visualScale = 1.05f;
                d.hpBarHeight = 1.9f;
                d.isBoss = false;
            });

            var bossDef = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act1_Boss_Guardian.asset", d =>
            {
                d.monsterId = "act1_boss_guardian";
                d.displayName = "Cổ Mộc Yêu Vương";
                d.description = "Thủ lĩnh ải 1-10 Đồng Cỏ Thanh Vân — rơi chắc chắn Phôi Kiếm & Thảo Dược.";
                d.archetype = RemakeMonsterArchetype.BossGuardian;
                d.codexPortraitKey = "Monsters/act1_boss_guardian";
                d.prefab = bossPrefab;
                d.hpMultiplier = 1.0f;
                d.damageMultiplier = 1.0f;
                d.moveSpeed = 1.9f;
                d.attackRange = 2.4f;
                d.attackInterval = 1.5f;
                d.visualScale = 1.35f;
                d.hpBarHeight = 3.4f;
                d.isBoss = true;
            });

            return new[] { wolfDef, birdDef, turtleDef, bossDef };
        }

        private static MonsterDefinitionSO[] CreateAct2Monsters(MonsterDefinitionSO[] act1Monsters)
        {
            var scorpionPrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Monsters/Monster_Act1_Wolf.prefab",
                PrefabsRoot + "/Monsters/Monster_Act2_Scorpion.prefab",
                "Act2_Scorpion", new Color(0.95f, 0.58f, 0.34f, 1f)) ?? act1Monsters[0].prefab;
            var bearPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Monsters/Monster_Act2_Bear.prefab") ?? act1Monsters[1].prefab;
            var sandTurtlePrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Monsters/Monster_Act1_Turtle.prefab",
                PrefabsRoot + "/Monsters/Monster_Act2_SandTurtle.prefab",
                "Act2_SandTurtle", new Color(1.0f, 0.86f, 0.48f, 1f)) ?? act1Monsters[2].prefab;
            var bossPrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Bosses/Boss_Act1_Guardian.prefab",
                PrefabsRoot + "/Bosses/Boss_Act2_Overlord.prefab",
                "Act2_BossOverlord", new Color(1.0f, 0.45f, 0.25f, 1f)) ?? act1Monsters[3].prefab;

            var m1 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act2_Scorpion.asset", d =>
            {
                d.monsterId = "act2_scorpion"; d.displayName = "Huyết Độc Sa Thú";
                d.description = "Yêu thú sa mạc nhanh nhẹn, tấn công dồn dập.";
                d.archetype = RemakeMonsterArchetype.FastMelee; d.codexPortraitKey = "Monsters/act2_scorpion";
                d.prefab = scorpionPrefab; d.hpMultiplier = 1.0f; d.damageMultiplier = 1.05f;
                d.moveSpeed = 3.1f; d.visualScale = 1.05f; d.isBoss = false;
            });
            var m2 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act2_Bear.asset", d =>
            {
                d.monsterId = "act2_bear"; d.displayName = "Cuồng Sa Hùng Yêu";
                d.description = "Hùng yêu lực lượng hùng mạnh giữa bão cát.";
                d.archetype = RemakeMonsterArchetype.FastMelee; d.codexPortraitKey = "Monsters/act2_scorpion";
                d.prefab = bearPrefab; d.hpMultiplier = 1.35f; d.damageMultiplier = 1.1f;
                d.moveSpeed = 2.4f; d.armorMitigation = 0.18f; d.visualScale = 0.95f; d.isBoss = false;
            });
            var m3 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act2_SandTurtle.asset", d =>
            {
                d.monsterId = "act2_sand_turtle"; d.displayName = "Hoàng Sa Cổ Quy";
                d.description = "Giáp đá sa mạc cứng cáp, rơi nhiều Linh Thạch.";
                d.archetype = RemakeMonsterArchetype.ArmoredTank; d.codexPortraitKey = "Monsters/act2_scorpion";
                d.prefab = sandTurtlePrefab; d.hpMultiplier = 1.55f; d.damageMultiplier = 0.9f;
                d.moveSpeed = 1.95f; d.armorMitigation = 0.28f; d.linhThachMultiplier = 1.3f; d.isBoss = false;
            });
            var boss = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act2_Boss_Overlord.asset", d =>
            {
                d.monsterId = "act2_boss_overlord"; d.displayName = "Sa Hoàng Yêu Đế";
                d.description = "Thủ lĩnh ải X-10 Hoang Mạc Nắng Vàng.";
                d.archetype = RemakeMonsterArchetype.BossGuardian; d.codexPortraitKey = "Monsters/act2_boss_overlord";
                d.prefab = bossPrefab; d.hpMultiplier = 1.15f; d.damageMultiplier = 1.05f;
                d.moveSpeed = 2.0f; d.attackRange = 2.5f; d.visualScale = 1.55f; d.hpBarHeight = 3.5f; d.isBoss = true;
            });
            return new[] { m1, m2, m3, boss };
        }

        private static MonsterDefinitionSO[] CreateAct3Monsters(MonsterDefinitionSO[] act1Monsters)
        {
            var spiritPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Monsters/Monster_Act3_ThunderSpirit.prefab") ?? act1Monsters[0].prefab;
            var cranePrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Monsters/Monster_Act1_Bird.prefab",
                PrefabsRoot + "/Monsters/Monster_Act3_JadeCrane.prefab",
                "Act3_JadeCrane", new Color(0.75f, 0.98f, 0.95f, 1f)) ?? act1Monsters[1].prefab;
            var jadeTurtlePrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Monsters/Monster_Act1_Turtle.prefab",
                PrefabsRoot + "/Monsters/Monster_Act3_JadeTurtle.prefab",
                "Act3_JadeTurtle", new Color(0.55f, 0.96f, 0.80f, 1f)) ?? act1Monsters[2].prefab;
            var bossPrefab = WebGlPrefabOptimizerTool.CreateTintedPrefabVariant(
                PrefabsRoot + "/Bosses/Boss_Act1_Guardian.prefab",
                PrefabsRoot + "/Bosses/Boss_Act3_Archon.prefab",
                "Act3_BossArchon", new Color(0.62f, 0.92f, 1.0f, 1f)) ?? act1Monsters[3].prefab;

            var m1 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act3_ThunderSpirit.asset", d =>
            {
                d.monsterId = "act3_thunder_spirit"; d.displayName = "Cửu Tiêu Lôi Linh";
                d.description = "Tinh linh ngọc đỉnh mang sấm sét thanh khiết.";
                d.archetype = RemakeMonsterArchetype.FastMelee; d.codexPortraitKey = "Monsters/act3_thunder_spirit";
                d.prefab = spiritPrefab; d.hpMultiplier = 1.15f; d.damageMultiplier = 1.15f;
                d.moveSpeed = 3.25f; d.visualScale = 0.95f; d.isBoss = false;
            });
            var m2 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act3_JadeCrane.asset", d =>
            {
                d.monsterId = "act3_jade_crane"; d.displayName = "Bạch Ngọc Linh Cầm";
                d.description = "Phi cầm tiên sơn bay lượn với tốc độ cực cao.";
                d.archetype = RemakeMonsterArchetype.FlyingAgile; d.codexPortraitKey = "Monsters/act3_thunder_spirit";
                d.prefab = cranePrefab; d.hpMultiplier = 1.0f; d.damageMultiplier = 1.1f;
                d.moveSpeed = 3.5f; d.hoverHeight = 1.25f; d.isBoss = false;
            });
            var m3 = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act3_JadeTurtle.asset", d =>
            {
                d.monsterId = "act3_jade_turtle"; d.displayName = "Thái Cổ Ngọc Quy";
                d.description = "Hộ sơn thần quy phòng ngự dày đặc, nhiều Tu Vi.";
                d.archetype = RemakeMonsterArchetype.ArmoredTank; d.codexPortraitKey = "Monsters/act3_thunder_spirit";
                d.prefab = jadeTurtlePrefab; d.hpMultiplier = 1.65f; d.damageMultiplier = 1.0f;
                d.moveSpeed = 2.0f; d.armorMitigation = 0.34f; d.tuViMultiplier = 1.35f; d.isBoss = false;
            });
            var boss = CreateOrUpdateMonsterSO(DataRoot + "/Monsters/Act3_Boss_Archon.asset", d =>
            {
                d.monsterId = "act3_boss_archon"; d.displayName = "Thái Hư Kiếm Tôn";
                d.description = "Thủ lĩnh ải X-10 Tiên Sơn Bạch Ngọc.";
                d.archetype = RemakeMonsterArchetype.BossGuardian; d.codexPortraitKey = "Monsters/act3_boss_archon";
                d.prefab = bossPrefab; d.hpMultiplier = 1.35f; d.damageMultiplier = 1.15f;
                d.moveSpeed = 2.1f; d.attackRange = 2.6f; d.visualScale = 1.65f; d.hpBarHeight = 3.6f; d.isBoss = true;
            });
            return new[] { m1, m2, m3, boss };
        }

        private static GameObject CloneCleanModelPrefab(string sourcePrefabPath, string destPrefabPath)
        {
            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (src == null) throw new InvalidOperationException($"Source prefab missing: {sourcePrefabPath}");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                // Strip any legacy Assembly-CSharp MonoBehaviours & Colliders cleanly
                foreach (var mb in instance.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null && mb.GetType().Assembly.GetName().Name == "Assembly-CSharp")
                        UnityEngine.Object.DestroyImmediate(mb);
                }
                foreach (var col in instance.GetComponentsInChildren<Collider>(true))
                {
                    UnityEngine.Object.DestroyImmediate(col);
                }
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                if (instance.GetComponent<RemakeMonsterActor>() == null)
                    instance.AddComponent<RemakeMonsterActor>();

                return PrefabUtility.SaveAsPrefabAsset(instance, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static GameObject CloneCleanPlayerPrefab(string destPrefabPath)
        {
            GameObject src = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/player nu.prefab");
            if (src == null) throw new InvalidOperationException("Assets/Prefabs/player nu.prefab missing");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var mb in instance.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null && mb.GetType().Assembly.GetName().Name == "Assembly-CSharp")
                        UnityEngine.Object.DestroyImmediate(mb);
                }
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                return PrefabUtility.SaveAsPrefabAsset(instance, destPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static MonsterDefinitionSO CreateOrUpdateMonsterSO(string path, Action<MonsterDefinitionSO> configure)
        {
            var so = AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<MonsterDefinitionSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            configure?.Invoke(so);
            EditorUtility.SetDirty(so);
            return so;
        }

        private static ActStageDatabaseSO CreateActStageDatabase(
            MonsterDefinitionSO[] act1Monsters,
            MonsterDefinitionSO[] act2Monsters,
            MonsterDefinitionSO[] act3Monsters)
        {
            string path = ResourcesRoot + "/ActStageDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<ActStageDatabaseSO>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ActStageDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }

            var list = new List<ActStageDatabaseSO.StageSpec>(120);
            MonsterDefinitionSO[][] pools = { act1Monsters, act2Monsters, act3Monsters };

            for (int d = 0; d < 4; d++)
            {
                for (int a = 0; a < 3; a++)
                {
                    var mSet = pools[a];
                    var normalPool = new[] { mSet[0], mSet[1], mSet[2] };
                    var boss = mSet[3];
                    for (int s = 1; s <= 10; s++)
                    {
                        var spec = ActStageDatabaseSO.BuildFallbackSpec((RemakeDifficulty)d, a, s);
                        spec.enemyPool = normalPool;
                        spec.bossMonster = boss;
                        list.Add(spec);
                    }
                }
            }
            db.SetStages(list);
            EditorUtility.SetDirty(db);
            return db;
        }

        private static void BuildBootScene(PanelSettings panelSettings, RemakeAudioBankSO audioBank)
        {
            Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null;

            var root = new GameObject("RemakeBootCore");
            root.AddComponent<AudioListener>(); // Single global AudioListener in Boot
            root.AddComponent<GameBootstrap>();
            root.AddComponent<PlatformWebBridge>();
            root.AddComponent<GraphicsSettingsService>();
            root.AddComponent<FpsMonitorService>();
            root.AddComponent<BackgroundCatchupService>();
            var audioSvc = root.AddComponent<RemakeAudioService>();
            var resolvedAudioBank = AssetDatabase.LoadAssetAtPath<RemakeAudioBankSO>(ResourcesRoot + "/RemakeAudioBank.asset") ?? audioBank;
            SetSerializedObjectField(audioSvc, "audioBank", resolvedAudioBank);
            EditorUtility.SetDirty(audioSvc);

            root.AddComponent<SceneFlowController>();
            root.AddComponent<RemakeOnlineService>();
            var engine = root.AddComponent<GameplayEngine>();
            SetSerializedObjectField(engine, "stageDatabase", AssetDatabase.LoadAssetAtPath<ActStageDatabaseSO>(ResourcesRoot + "/ActStageDatabase.asset"));
            EditorUtility.SetDirty(engine);

            var uiGo = new GameObject("BootOverlayUI");
            var uiDoc = uiGo.AddComponent<UIDocument>();
            var resolvedPs = AssetDatabase.LoadAssetAtPath<PanelSettings>(ResourcesRoot + "/RemakePanelSettings.asset")
                ?? Resources.Load<PanelSettings>("RemakePanelSettings")
                ?? panelSettings;
            if (resolvedPs == null) throw new InvalidOperationException("RemakePanelSettings.asset missing when building 00_Boot");
            uiDoc.panelSettings = resolvedPs;
            uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Game/UI/UXML/BootOverlay.uxml");
            uiDoc.sortingOrder = 100;
            SetSerializedObjectField(uiDoc, "m_PanelSettings", resolvedPs);
            EditorUtility.SetDirty(uiDoc);
            var ctrl = uiGo.AddComponent<BootOverlayController>();
            SetSerializedObjectField(ctrl, "uiDocument", uiDoc);

            EditorSceneManager.SaveScene(sc, ScenesRoot + "/00_Boot.unity");
        }

        private static void BuildMainMenuScene(PanelSettings panelSettings)
        {
            Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("MainCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.84f, 0.94f, 0.99f, 1f);
            camGo.tag = "MainCamera";

            var uiGo = new GameObject("MainMenuUI");
            var uiDoc = uiGo.AddComponent<UIDocument>();
            var resolvedPs = AssetDatabase.LoadAssetAtPath<PanelSettings>(ResourcesRoot + "/RemakePanelSettings.asset")
                ?? Resources.Load<PanelSettings>("RemakePanelSettings")
                ?? panelSettings;
            if (resolvedPs == null) throw new InvalidOperationException("RemakePanelSettings.asset missing when building 01_MainMenu");
            uiDoc.panelSettings = resolvedPs;
            uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Game/UI/UXML/MainMenu.uxml");
            uiDoc.sortingOrder = 10;
            SetSerializedObjectField(uiDoc, "m_PanelSettings", resolvedPs);
            EditorUtility.SetDirty(uiDoc);
            var ctrl = uiGo.AddComponent<MainMenuController>();
            SetSerializedObjectField(ctrl, "uiDocument", uiDoc);

            EditorSceneManager.SaveScene(sc, ScenesRoot + "/01_MainMenu.unity");
        }

        private static void BuildUiOverlayScene(PanelSettings panelSettings)
        {
            Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var uiGo = new GameObject("InGameHudUI");
            var uiDoc = uiGo.AddComponent<UIDocument>();
            var resolvedPs = AssetDatabase.LoadAssetAtPath<PanelSettings>(ResourcesRoot + "/RemakePanelSettings.asset")
                ?? Resources.Load<PanelSettings>("RemakePanelSettings")
                ?? panelSettings;
            if (resolvedPs == null) throw new InvalidOperationException("RemakePanelSettings.asset missing when building 99_UI_Overlay");
            uiDoc.panelSettings = resolvedPs;
            uiDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Game/UI/UXML/InGameHud.uxml");
            uiDoc.sortingOrder = 20;
            SetSerializedObjectField(uiDoc, "m_PanelSettings", resolvedPs);
            EditorUtility.SetDirty(uiDoc);
            var ctrl = uiGo.AddComponent<InGameHudController>();
            SetSerializedObjectField(ctrl, "uiDocument", uiDoc);

            EditorSceneManager.SaveScene(sc, ScenesRoot + "/99_UI_Overlay.unity");
        }

        private static void BuildWorldActScene(
            int actIndex,
            MeadowDefaultAssets art,
            VolumeProfile volumeProfile,
            RemakeFlyingSwordActor flyingSwordPrefab,
            string[] monsterAssetPaths)
        {
            var theme = (MeadowEnvironmentController.WorldThemePreset)Mathf.Clamp(actIndex, 0, 2);
            string scenePath = actIndex == 1 ? ScenesRoot + "/03_World_Act2_Wasteland.unity"
                : actIndex == 2 ? ScenesRoot + "/04_World_Act3_JadePeaks.unity"
                : ScenesRoot + "/02_World_Act1_Meadow.unity";
            Material groundMat = actIndex == 1 ? art.desertSandMat : actIndex == 2 ? art.jadeCliffMat : art.grassMat;
            Color skyBg = actIndex == 1 ? new Color(0.98f, 0.92f, 0.78f, 1f)
                : actIndex == 2 ? new Color(0.82f, 0.95f, 1.00f, 1f)
                : new Color(0.75f, 0.90f, 1.00f, 1f);

            Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MeadowEnvironmentController.ApplyDaylightLighting(theme);

            // Top-down arena camera centered on Cultivator at (0,0,0) with 360-degree visibility
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 15.6f, -6.2f);
            camGo.transform.rotation = Quaternion.Euler(66f, 0f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = skyBg;
            var urpCam = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            // Sunlit Directional Light
            var lightGo = new GameObject("SunLight");
            var dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = actIndex == 1 ? new Color(1f, 0.94f, 0.82f, 1f) : new Color(1f, 0.97f, 0.90f, 1f);
            dirLight.intensity = actIndex == 1 ? 1.35f : 1.28f;
            dirLight.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            // Global Post Volume (Bright crisp look, no dark vignette)
            var volGo = new GameObject("DaylightVolume");
            var vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = volumeProfile;

            // Environment Root: 3 Lightweight Ground Chunks + Sky Dome + Perimeter Props
            var envGo = new GameObject("WorldEnvironment");
            var envCtrl = envGo.AddComponent<MeadowEnvironmentController>();
            SetSerializedObjectEnum(envCtrl, "worldTheme", (int)theme);
            var grassRenderers = new List<Renderer>();
            var dirtRenderers = new List<Renderer>();

            for (int c = 0; c < 3; c++)
            {
                var chunk = new GameObject($"GroundChunk_{c}");
                chunk.transform.SetParent(envGo.transform, false);
                chunk.transform.position = new Vector3(0f, 0f, (c - 1) * 28f);
                chunk.isStatic = true;
                chunk.AddComponent<MeshFilter>().sharedMesh = art.groundChunkMesh;
                var mr = chunk.AddComponent<MeshRenderer>();
                mr.sharedMaterial = groundMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                grassRenderers.Add(mr);
            }

            if (actIndex == 0)
            {
                Vector3[] dirtSpots =
                {
                    new Vector3(-4.2f, 0.03f, 4.0f),
                    new Vector3(4.4f, 0.03f, -3.6f),
                    new Vector3(-3.8f, 0.03f, -5.2f),
                    new Vector3(3.6f, 0.03f, 5.4f)
                };
                for (int i = 0; i < dirtSpots.Length; i++)
                {
                    var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    p.name = $"DirtPatch_{i}";
                    UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());
                    p.transform.SetParent(envGo.transform, false);
                    p.transform.position = dirtSpots[i];
                    p.transform.rotation = Quaternion.Euler(90f, i * 37f, 0f);
                    p.transform.localScale = new Vector3(4.4f + (i % 2) * 0.8f, 3.6f, 1f);
                    var mr = p.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = art.dirtMat;
                    mr.shadowCastingMode = ShadowCastingMode.Off;
                    dirtRenderers.Add(mr);
                }
            }

            var propsRoot = new GameObject("PerimeterProps");
            propsRoot.transform.SetParent(envGo.transform, false);
            BuildHolotnaPerimeterEnvironment(propsRoot.transform, actIndex, art);
            CreateAmbientMotes(envGo.transform, actIndex);

            // Sky Dome
            var sky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sky.name = "DaylightSkyDome";
            UnityEngine.Object.DestroyImmediate(sky.GetComponent<Collider>());
            sky.transform.SetParent(envGo.transform, false);
            sky.transform.position = new Vector3(0f, -6f, 12f);
            sky.transform.localScale = new Vector3(110f, 75f, 110f);
            var skyMr = sky.GetComponent<MeshRenderer>();
            skyMr.sharedMaterial = art.skyMat;
            skyMr.shadowCastingMode = ShadowCastingMode.Off;

            SetSerializedObjectArray(envCtrl, "grassChunkRenderers", grassRenderers.ToArray());
            SetSerializedObjectArray(envCtrl, "dirtPatchRenderers", dirtRenderers.ToArray());
            SetSerializedObjectField(envCtrl, "skyDomeRenderer", skyMr);
            SetSerializedObjectField(envCtrl, "arenaCamera", cam);

            // IMPORTANT: Load ALREADY-OPTIMIZED Player prefab (never re-clone raw FBX!)
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Player/RemakePlayer.prefab");
            GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerGo.name = "CultivatorPlayer";
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity;
            playerGo.transform.localScale = Vector3.one * 1.25f;

            var ringGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ringGo.name = "GroundFormationRing";
            UnityEngine.Object.DestroyImmediate(ringGo.GetComponent<Collider>());
            ringGo.transform.SetParent(playerGo.transform, false);
            ringGo.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            ringGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ringGo.transform.localScale = new Vector3(3.4f, 3.4f, 1f);
            var ringMr = ringGo.GetComponent<MeshRenderer>();
            ringMr.sharedMaterial = art.groundRingMat;
            ringMr.shadowCastingMode = ShadowCastingMode.Off;
            ringMr.receiveShadows = false;

            var vfxGo = new GameObject("RemakeVfxPool");
            vfxGo.AddComponent<RemakeVfxPool>();
            var lootPoolGo = new GameObject("RemakeLootOrbPool");
            lootPoolGo.AddComponent<RemakeLootOrbPool>();
            var swordsGo = new GameObject("SwordsRoot");

            var resolvedSwordPrefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Player/RemakeFlyingSword.prefab");
            var resolvedSwordPrefab = resolvedSwordPrefabGo != null ? resolvedSwordPrefabGo.GetComponent<RemakeFlyingSwordActor>() : flyingSwordPrefab;
            var resolvedNormalMonsters = new[]
            {
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[0]),
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[1]),
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[2])
            };
            var resolvedBoss = AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[3]);
            if (resolvedBoss == null || resolvedNormalMonsters[0] == null || resolvedNormalMonsters[1] == null || resolvedNormalMonsters[2] == null)
                throw new InvalidOperationException($"Missing MonsterDefinitionSO while building {scenePath}");

            var arenaGo = new GameObject("RemakeArenaController");
            var arenaCtrl = arenaGo.AddComponent<RemakeArenaController>();
            SetSerializedObjectField(arenaCtrl, "playerTransform", playerGo.transform);
            SetSerializedObjectField(arenaCtrl, "swordsRoot", swordsGo.transform);
            SetSerializedObjectField(arenaCtrl, "groundFormationRing", ringGo.transform);
            SetSerializedObjectField(arenaCtrl, "flyingSwordPrefab", resolvedSwordPrefab);
            SetSerializedObjectArray(arenaCtrl, "act1NormalMonsters", resolvedNormalMonsters);
            SetSerializedObjectField(arenaCtrl, "act1BossMonster", resolvedBoss);
            EditorUtility.SetDirty(arenaCtrl);
            EditorSceneManager.MarkSceneDirty(sc);

            EditorSceneManager.SaveScene(sc, scenePath);
        }

        private static void BuildChallengeArenaScene(
            MeadowEnvironmentController.WorldThemePreset theme,
            string scenePath,
            MeadowDefaultAssets art,
            VolumeProfile volumeProfile,
            RemakeFlyingSwordActor flyingSwordPrefab,
            string[] monsterAssetPaths)
        {
            Material groundMat = theme switch
            {
                MeadowEnvironmentController.WorldThemePreset.DemonTower => art.towerFloorMat,
                MeadowEnvironmentController.WorldThemePreset.SecretSwordTomb => art.swordTombFloorMat,
                MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden => art.herbGardenFloorMat,
                _ => art.oreCavernFloorMat
            };
            Color skyBg = theme switch
            {
                MeadowEnvironmentController.WorldThemePreset.DemonTower => new Color(0.88f, 0.92f, 0.96f, 1f),
                MeadowEnvironmentController.WorldThemePreset.SecretSwordTomb => new Color(0.86f, 0.93f, 0.94f, 1f),
                MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden => new Color(0.78f, 0.95f, 0.90f, 1f),
                _ => new Color(0.82f, 0.92f, 0.98f, 1f)
            };

            Scene sc = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MeadowEnvironmentController.ApplyDaylightLighting(theme);

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 15.6f, -6.2f);
            camGo.transform.rotation = Quaternion.Euler(66f, 0f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = skyBg;
            var urpCam = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            var lightGo = new GameObject("SunLight");
            var dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1f, 0.97f, 0.90f, 1f);
            dirLight.intensity = 1.32f;
            dirLight.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var volGo = new GameObject("DaylightVolume");
            var vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = volumeProfile;

            var envGo = new GameObject("WorldEnvironment");
            var envCtrl = envGo.AddComponent<MeadowEnvironmentController>();
            SetSerializedObjectEnum(envCtrl, "worldTheme", (int)theme);

            var groundRenderers = new List<Renderer>();
            for (int c = 0; c < 3; c++)
            {
                var chunk = new GameObject($"GroundChunk_{c}");
                chunk.transform.SetParent(envGo.transform, false);
                chunk.transform.position = new Vector3(0f, 0f, (c - 1) * 28f);
                chunk.isStatic = true;
                chunk.AddComponent<MeshFilter>().sharedMesh = art.groundChunkMesh;
                var mr = chunk.AddComponent<MeshRenderer>();
                mr.sharedMaterial = groundMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                groundRenderers.Add(mr);
            }

            var propsRoot = new GameObject("ChallengeArenaProps");
            propsRoot.transform.SetParent(envGo.transform, false);
            var codexPropRenderers = new List<Renderer>();
            BuildChallengePerimeterProps(propsRoot.transform, theme, art, codexPropRenderers);

            int moteThemeIdx = theme == MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden ? 0
                : theme == MeadowEnvironmentController.WorldThemePreset.DemonTower ? 1 : 2;
            CreateAmbientMotes(envGo.transform, moteThemeIdx);

            var sky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sky.name = "ChallengeSkyDome";
            UnityEngine.Object.DestroyImmediate(sky.GetComponent<Collider>());
            sky.transform.SetParent(envGo.transform, false);
            sky.transform.position = new Vector3(0f, -6f, 12f);
            sky.transform.localScale = new Vector3(110f, 75f, 110f);
            var skyMr = sky.GetComponent<MeshRenderer>();
            skyMr.sharedMaterial = art.skyMat;
            skyMr.shadowCastingMode = ShadowCastingMode.Off;

            SetSerializedObjectArray(envCtrl, "grassChunkRenderers", groundRenderers.ToArray());
            SetSerializedObjectArray(envCtrl, "codexPropRenderers", codexPropRenderers.ToArray());
            SetSerializedObjectField(envCtrl, "skyDomeRenderer", skyMr);
            SetSerializedObjectField(envCtrl, "arenaCamera", cam);

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Player/RemakePlayer.prefab");
            GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerGo.name = "CultivatorPlayer";
            playerGo.transform.position = Vector3.zero;
            playerGo.transform.rotation = Quaternion.identity;
            playerGo.transform.localScale = Vector3.one * 1.25f;

            var ringGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ringGo.name = "GroundFormationRing";
            UnityEngine.Object.DestroyImmediate(ringGo.GetComponent<Collider>());
            ringGo.transform.SetParent(playerGo.transform, false);
            ringGo.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            ringGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ringGo.transform.localScale = new Vector3(3.6f, 3.6f, 1f);
            var ringMr = ringGo.GetComponent<MeshRenderer>();
            ringMr.sharedMaterial = art.groundRingMat;
            ringMr.shadowCastingMode = ShadowCastingMode.Off;
            ringMr.receiveShadows = false;

            new GameObject("RemakeVfxPool").AddComponent<RemakeVfxPool>();
            new GameObject("RemakeLootOrbPool").AddComponent<RemakeLootOrbPool>();
            var swordsGo = new GameObject("SwordsRoot");

            var resolvedSwordPrefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Player/RemakeFlyingSword.prefab");
            var resolvedSwordPrefab = resolvedSwordPrefabGo != null ? resolvedSwordPrefabGo.GetComponent<RemakeFlyingSwordActor>() : flyingSwordPrefab;
            var resolvedNormalMonsters = new[]
            {
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[0]),
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[1]),
                AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[2])
            };
            var resolvedBoss = AssetDatabase.LoadAssetAtPath<MonsterDefinitionSO>(monsterAssetPaths[3]);

            var arenaGo = new GameObject("RemakeArenaController");
            var arenaCtrl = arenaGo.AddComponent<RemakeArenaController>();
            SetSerializedObjectField(arenaCtrl, "playerTransform", playerGo.transform);
            SetSerializedObjectField(arenaCtrl, "swordsRoot", swordsGo.transform);
            SetSerializedObjectField(arenaCtrl, "groundFormationRing", ringGo.transform);
            SetSerializedObjectField(arenaCtrl, "flyingSwordPrefab", resolvedSwordPrefab);
            SetSerializedObjectArray(arenaCtrl, "act1NormalMonsters", resolvedNormalMonsters);
            SetSerializedObjectField(arenaCtrl, "act1BossMonster", resolvedBoss);
            EditorUtility.SetDirty(arenaCtrl);
            EditorSceneManager.MarkSceneDirty(sc);

            EditorSceneManager.SaveScene(sc, scenePath);
        }

        private static void BuildChallengePerimeterProps(
            Transform parent,
            MeadowEnvironmentController.WorldThemePreset theme,
            MeadowDefaultAssets art,
            List<Renderer> codexPropRenderers)
        {
            Material codexPropMat = theme switch
            {
                MeadowEnvironmentController.WorldThemePreset.DemonTower => art.towerPropMat,
                MeadowEnvironmentController.WorldThemePreset.SecretSwordTomb => art.swordTombPropMat,
                MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden => art.herbGardenPropMat,
                _ => art.oreCavernPropMat
            };
            bool hasCodexPropTex = codexPropMat != null && codexPropMat.mainTexture != null;

            for (int i = 0; i < 8; i++)
            {
                float deg = i * 45f + 22.5f;
                if (deg > 235f && deg < 305f) continue;
                float rad = deg * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(rad) * 6.4f, 0f, Mathf.Sin(rad) * 7.0f + 0.5f);

                if (!hasCodexPropTex && theme == MeadowEnvironmentController.WorldThemePreset.DemonTower)
                {
                    var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pillar.name = $"TowerPillar_{i}";
                    UnityEngine.Object.DestroyImmediate(pillar.GetComponent<Collider>());
                    pillar.transform.SetParent(parent, false);
                    pillar.transform.position = pos + Vector3.up * 1.6f;
                    pillar.transform.localScale = new Vector3(0.75f, 1.6f, 0.75f);
                    pillar.GetComponent<MeshRenderer>().sharedMaterial = art.towerAccentMat;
                    StripShadowsRecursive(pillar);
                }
                else if (!hasCodexPropTex && theme == MeadowEnvironmentController.WorldThemePreset.SecretSwordTomb)
                {
                    var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    blade.name = $"AncientSword_{i}";
                    UnityEngine.Object.DestroyImmediate(blade.GetComponent<Collider>());
                    blade.transform.SetParent(parent, false);
                    blade.transform.position = pos + Vector3.up * 1.45f;
                    blade.transform.rotation = Quaternion.Euler(12f - (i % 3) * 8f, i * 45f, -14f + (i % 2) * 28f);
                    blade.transform.localScale = new Vector3(0.28f, 3.1f, 0.08f);
                    blade.GetComponent<MeshRenderer>().sharedMaterial = art.swordAccentMat;
                    StripShadowsRecursive(blade);
                }
                else if (!hasCodexPropTex && theme == MeadowEnvironmentController.WorldThemePreset.SecretOreCavern)
                {
                    var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crystal.name = $"CrystalVein_{i}";
                    UnityEngine.Object.DestroyImmediate(crystal.GetComponent<Collider>());
                    crystal.transform.SetParent(parent, false);
                    crystal.transform.position = pos + Vector3.up * 0.95f;
                    crystal.transform.rotation = Quaternion.Euler(18f, i * 50f, 15f);
                    crystal.transform.localScale = new Vector3(0.72f, 2.0f, 0.72f);
                    crystal.GetComponent<MeshRenderer>().sharedMaterial = art.oreAccentMat;
                    StripShadowsRecursive(crystal);
                }
                else if (!hasCodexPropTex)
                {
                    string key = (i % 2 == 0) ? "Env_Act1_Flower" : "Env_Act1_Bush";
                    var pf = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Optimized/Environment/" + key + ".prefab");
                    if (pf != null)
                    {
                        var inst = (GameObject)PrefabUtility.InstantiatePrefab(pf);
                        inst.name = $"HerbProp_{i}";
                        inst.transform.SetParent(parent, false);
                        inst.transform.position = pos;
                        inst.transform.localScale = Vector3.one * (i % 2 == 0 ? 1.25f : 0.62f);
                        StripShadowsRecursive(inst);
                    }
                }

                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                q.name = $"CodexChallengeProp_{i}";
                UnityEngine.Object.DestroyImmediate(q.GetComponent<Collider>());
                q.transform.SetParent(parent, false);
                q.transform.position = pos + new Vector3(0f, 1.35f, 0f);
                q.transform.rotation = Quaternion.Euler(46f, 0f, 0f);
                q.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
                var qmr = q.GetComponent<MeshRenderer>();
                qmr.sharedMaterial = codexPropMat;
                qmr.shadowCastingMode = ShadowCastingMode.Off;
                qmr.receiveShadows = false;
                q.SetActive(hasCodexPropTex);
                codexPropRenderers.Add(qmr);
            }

            string mtnKey = theme == MeadowEnvironmentController.WorldThemePreset.SecretHerbGarden ? "Env_Act1_Mountain"
                : theme == MeadowEnvironmentController.WorldThemePreset.SecretOreCavern ? "Env_Act2_Mountain" : "Env_Act3_Mountain";
            var mtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsRoot + "/Optimized/Environment/" + mtnKey + ".prefab");
            if (mtnPrefab != null)
            {
                Vector3[] spots = { new Vector3(-9.5f, -0.2f, 14.0f), new Vector3(0f, -0.2f, 15.6f), new Vector3(9.5f, -0.2f, 13.5f) };
                for (int m = 0; m < spots.Length; m++)
                {
                    var mtn = (GameObject)PrefabUtility.InstantiatePrefab(mtnPrefab);
                    mtn.name = $"{mtnKey}_Ch_{m}";
                    mtn.transform.SetParent(parent, false);
                    mtn.transform.position = spots[m];
                    mtn.transform.rotation = Quaternion.Euler(0f, m * 50f - 20f, 0f);
                    mtn.transform.localScale = new Vector3(0.048f, 0.042f, 0.048f);
                    StripShadowsRecursive(mtn);
                }
            }
        }

        private static void BuildHolotnaPerimeterEnvironment(Transform parent, int actIndex, MeadowDefaultAssets art)
        {
            string envRoot = PrefabsRoot + "/Optimized/Environment/";
            string[] ringKeys = actIndex == 1
                ? new[] { "Env_Act2_Rock", "Env_Act2_Pebble", "Env_Act2_DryBush" }
                : actIndex == 2
                    ? new[] { "Env_Act3_JadeCrystal", "Env_Act3_SnowPine", "Env_Act3_Flower" }
                    : new[] { "Env_Act1_Tree", "Env_Act1_Bush", "Env_Act1_Rock", "Env_Act1_Flower" };

            float[] ringAngles = { 8f, 34f, 62f, 92f, 120f, 148f, 174f, 198f, 340f };
            for (int i = 0; i < ringAngles.Length; i++)
            {
                float rad = ringAngles[i] * Mathf.Deg2Rad;
                float rx = 5.6f + (i % 3) * 0.95f;
                float rz = 6.4f + (i % 2) * 1.15f;
                Vector3 pos = new Vector3(Mathf.Cos(rad) * rx, 0f, Mathf.Sin(rad) * rz + 0.6f);

                string key = ringKeys[i % ringKeys.Length];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(envRoot + key + ".prefab");
                if (prefab != null)
                {
                    var prop = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    prop.name = $"{key}_{i}";
                    prop.transform.SetParent(parent, false);
                    prop.transform.position = pos;
                    prop.transform.rotation = Quaternion.Euler(0f, i * 43f, 0f);
                    float s = key.Contains("Tree") || key.Contains("Pine") ? 0.56f + (i % 2) * 0.10f
                        : key.Contains("Crystal") ? 0.28f + (i % 3) * 0.05f
                        : key.Contains("Rock") ? 0.34f + (i % 3) * 0.07f
                        : key.Contains("Bush") ? 0.58f + (i % 2) * 0.14f
                        : key.Contains("Pebble") ? 0.65f + (i % 2) * 0.15f
                        : 1.05f + (i % 2) * 0.25f;
                    prop.transform.localScale = Vector3.one * s;
                    StripShadowsRecursive(prop);
                }
            }

            // Inner low grass / pebbles / flowers accent ring (strictly shadow-free)
            string detailKey = actIndex == 1 ? "Env_Act2_Pebble" : actIndex == 2 ? "Env_Act3_Flower" : "Env_Act1_Flower";
            var detailPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(envRoot + detailKey + ".prefab");
            for (int t = 0; t < 10; t++)
            {
                float deg = t * 36f + 15f;
                if (deg > 220f && deg < 320f) continue;
                float rad = deg * Mathf.Deg2Rad;
                Vector3 dp = new Vector3(Mathf.Cos(rad) * (4.4f + (t % 2) * 0.8f), 0f, Mathf.Sin(rad) * (5.2f + (t % 3) * 0.9f));
                if (detailPrefab != null)
                {
                    var d = (GameObject)PrefabUtility.InstantiatePrefab(detailPrefab);
                    d.name = $"{detailKey}_Sub_{t}";
                    d.transform.SetParent(parent, false);
                    d.transform.position = dp;
                    d.transform.rotation = Quaternion.Euler(0f, t * 57f, 0f);
                    float ds = detailKey.Contains("Pebble") ? (0.45f + (t % 3) * 0.10f) : (0.95f + (t % 3) * 0.20f);
                    d.transform.localScale = Vector3.one * ds;
                    StripShadowsRecursive(d);
                }
            }

            // Keep 4 stylized grass tufts in Act 1 for foreground lushness
            if (actIndex == 0)
            {
                for (int t = 0; t < 8; t++)
                {
                    float rad = (t * 44f + 8f) * Mathf.Deg2Rad;
                    var tuft = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tuft.name = $"MeadowGrassTuft_{t}";
                    UnityEngine.Object.DestroyImmediate(tuft.GetComponent<Collider>());
                    tuft.transform.SetParent(parent, false);
                    tuft.transform.position = new Vector3(Mathf.Cos(rad) * 5.0f, 0.42f, Mathf.Sin(rad) * 5.8f);
                    tuft.transform.rotation = Quaternion.Euler(44f, (t % 2 == 0 ? 12f : -12f), 0f);
                    tuft.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                    tuft.isStatic = true;
                    var mr = tuft.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = art.propGrassTuftMat;
                    mr.shadowCastingMode = ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                }
            }

            // Distant Mountain backdrop silhouettes along northern horizon (276m base mesh -> ~13m width at 0.048)
            string mtnKey = actIndex == 1 ? "Env_Act2_Mountain" : actIndex == 2 ? "Env_Act3_Mountain" : "Env_Act1_Mountain";
            var mtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(envRoot + mtnKey + ".prefab");
            if (mtnPrefab != null)
            {
                Vector3[] mtnSpots = { new Vector3(-9.5f, -0.2f, 13.5f), new Vector3(0f, -0.2f, 15.2f), new Vector3(9.5f, -0.2f, 13.0f) };
                for (int m = 0; m < mtnSpots.Length; m++)
                {
                    var mtn = (GameObject)PrefabUtility.InstantiatePrefab(mtnPrefab);
                    mtn.name = $"{mtnKey}_{m}";
                    mtn.transform.SetParent(parent, false);
                    mtn.transform.position = mtnSpots[m];
                    mtn.transform.rotation = Quaternion.Euler(0f, m * 55f - 25f, 0f);
                    mtn.transform.localScale = new Vector3(0.048f, 0.042f, 0.048f);
                    StripShadowsRecursive(mtn);
                }
            }
        }

        private static void StripShadowsRecursive(GameObject root)
        {
            root.isStatic = true;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        private static void CreateAmbientMotes(Transform parent, int actIndex)
        {
            var go = new GameObject("AmbientMotes");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 1.6f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = 18;
            main.startLifetime = 4.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startColor = actIndex == 1 ? new Color(1f, 0.88f, 0.50f, 0.55f)
                : actIndex == 2 ? new Color(0.68f, 0.98f, 0.92f, 0.60f)
                : new Color(0.98f, 0.96f, 0.62f, 0.58f);
            var em = ps.emission;
            em.rateOverTime = 4f;
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(10f, 1.5f, 14f);
            var pr = ps.GetComponent<ParticleSystemRenderer>();
            Shader pShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            pr.sharedMaterial = new Material(pShader) { color = main.startColor.color };
            pr.shadowCastingMode = ShadowCastingMode.Off;
            pr.receiveShadows = false;
        }

        private static void CreateActRockOrJadeSpire(Transform parent, Vector3 pos, Material mat, bool isJadeSpire, int seed) => BuildHolotnaPerimeterEnvironment(parent, isJadeSpire ? 2 : 1, new MeadowDefaultAssets());
        private static void CreateStylizedTreeAndRock(Transform parent, Vector3 pos, MeadowDefaultAssets art, int seed) { }

        private static void CreateOrUpdateBuildProfile()
        {
            string bpPath = DataRoot + "/BuildProfiles/Remake_WebGL_LocalLAN.asset";
            BuildProfile bp = BuildProfile.GetBuildProfileAtPath(bpPath);
            if (bp == null)
            {
                bp = ScriptableObject.CreateInstance<BuildProfile>();
                AssetDatabase.CreateAsset(bp, bpPath);
            }

            bp.overrideGlobalScenes = true;
            bp.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenesRoot + "/00_Boot.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/01_MainMenu.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/02_World_Act1_Meadow.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/03_World_Act2_Wasteland.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/04_World_Act3_JadePeaks.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/05_World_DemonTower.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/06_World_Secret_SwordTomb.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/07_World_Secret_HerbGarden.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/08_World_Secret_OreCavern.unity", true),
                new EditorBuildSettingsScene(ScenesRoot + "/99_UI_Overlay.unity", true)
            };
            EditorUtility.SetDirty(bp);
        }

        private static void SetSerializedObjectEnum(UnityEngine.Object target, string fieldName, int enumVal)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.enumValueIndex = enumVal;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetSerializedObjectField(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetSerializedObjectArray(UnityEngine.Object target, string fieldName, UnityEngine.Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null && prop.isArray)
            {
                prop.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
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

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KhiVanChiTu.Remake.Editor
{
    public static class CodexSeamlessAndSliceTool
    {
        private const string CodexRoot = "Assets/_Game/Resources/CodexDrop";
        private const string BakedRoot = "Assets/_Game/Resources/BakedMaps";

        [MenuItem("KhiVan/Remake/Bake Seamless Maps & 9-Slice UI")]
        public static void BakeAllMenu()
        {
            BakeAll(true);
        }

        private static bool _forceRebake;

        public static void BakeAll(bool refreshAndLog)
        {
            _forceRebake = refreshAndLog;
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Resources"))
                AssetDatabase.CreateFolder("Assets/_Game", "Resources");
            if (!AssetDatabase.IsValidFolder(BakedRoot))
                AssetDatabase.CreateFolder("Assets/_Game/Resources", "BakedMaps");

            BakeSeamlessTile("Map/tex_grass_meadow_albedo.png", "tex_grass_meadow_seamless.png", 0.16f, 512);
            BakeSeamlessTile("Map/tex_act2_sand_bright.png", "tex_act2_sand_seamless.png", 0.16f, 512);
            BakeSeamlessTile("Map/tex_act3_jade_cliff.png", "tex_act3_jade_seamless.png", 0.16f, 512);
            BakeHorizontalPanorama("Map/tex_sky_daylight_panorama.jpg", "tex_sky_daylight_seamless.jpg", 0.14f, 1024);

            // Group I: Dedicated Demon Tower & 3 Secret Realm Maps (auto-baked as soon as Codex drops them!)
            BakeSeamlessTile("Map/tex_tower_floor_albedo.png", "tex_tower_floor_seamless.png", 0.16f, 512);
            BakeHorizontalPanorama("Map/tex_tower_sky_panorama.jpg", "tex_tower_sky_seamless.jpg", 0.14f, 1024);
            BakeSeamlessTile("Map/tex_secret_sword_floor.png", "tex_secret_sword_seamless.png", 0.16f, 512);
            BakeHorizontalPanorama("Map/tex_secret_sword_sky.jpg", "tex_secret_sword_sky_seamless.jpg", 0.14f, 1024);
            BakeSeamlessTile("Map/tex_secret_herb_floor.png", "tex_secret_herb_seamless.png", 0.16f, 512);
            BakeHorizontalPanorama("Map/tex_secret_herb_sky.jpg", "tex_secret_herb_sky_seamless.jpg", 0.14f, 1024);
            BakeSeamlessTile("Map/tex_secret_ore_floor.png", "tex_secret_ore_seamless.png", 0.16f, 512);
            BakeHorizontalPanorama("Map/tex_secret_ore_sky.jpg", "tex_secret_ore_sky_seamless.jpg", 0.14f, 1024);

            BakeNineSlicePanel("HUD/hud_panel_light_9slice.png", "hud_panel_light_9slice.png", true, true, new Vector4(146, 118, 146, 118));
            BakeNineSlicePanel("HUD/hud_card_light_9slice.png", "hud_card_light_9slice.png", false, false, new Vector4(110, 84, 110, 142));
            BakeTrimmedButton9Slice("HUD/hud_btn_primary_9slice.png", "hud_btn_primary_9slice.png", 34, 150, false, new Vector4(128, 20, 128, 20));
            BakeTrimmedButton9Slice("HUD/hud_btn_secondary_9slice.png", "hud_btn_secondary_9slice.png", 20, 165, true, new Vector4(116, 22, 116, 22));

            if (refreshAndLog)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[CodexSeamlessAndSliceTool] Baked all seamless maps and clean 9-slice UI sprites into Assets/_Game/Resources/BakedMaps.");
            }
        }

        private static Texture2D LoadReadableCopyFromDisk(string assetPath)
        {
            string full = Path.GetFullPath(assetPath);
            if (!File.Exists(full)) return null;
            byte[] bytes = File.ReadAllBytes(full);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, bytes, false))
            {
                UnityEngine.Object.DestroyImmediate(tex);
                return null;
            }
            return tex;
        }

        private static bool IsBakedFileUpToDate(string srcAssetPath, string destAssetPath)
        {
            if (_forceRebake) return false;
            string fullSrc = Path.GetFullPath(srcAssetPath);
            string fullDst = Path.GetFullPath(destAssetPath);
            if (!File.Exists(fullSrc) || !File.Exists(fullDst)) return false;
            return File.GetLastWriteTimeUtc(fullDst) >= File.GetLastWriteTimeUtc(fullSrc);
        }

        private static void BakeSeamlessTile(string relSrc, string destFile, float borderFrac, int maxSize)
        {
            string srcPath = $"{CodexRoot}/{relSrc}";
            string destPath = $"{BakedRoot}/{destFile}";
            if (IsBakedFileUpToDate(srcPath, destPath)) return;
            var src = LoadReadableCopyFromDisk(srcPath);
            if (src == null) return;

            try
            {
                int w = src.width;
                int h = src.height;
                Color[] pix = src.GetPixels();
                Color[] outPix = new Color[w * h];

                int bw = Mathf.Max(8, Mathf.RoundToInt(w * borderFrac));
                int bh = Mathf.Max(8, Mathf.RoundToInt(h * borderFrac));

                for (int y = 0; y < h; y++)
                {
                    float wy = EdgeWeight(y, h, bh);
                    int sy = (y + h / 2) % h;
                    for (int x = 0; x < w; x++)
                    {
                        float wx = EdgeWeight(x, w, bw);
                        float weightOrig = wx * wy;
                        int sx = (x + w / 2) % w;

                        Color cOrig = pix[y * w + x];
                        Color cShift = pix[sy * w + sx];
                        outPix[y * w + x] = Color.Lerp(cShift, cOrig, weightOrig);
                    }
                }

                // Shift result by (-w/4, -h/4)? Not needed: edges (x=0 & x=w-1) already sample continuous interior cShift(w/2)
                var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                outTex.SetPixels(outPix);
                outTex.Apply();
                File.WriteAllBytes(Path.GetFullPath(destPath), outTex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(outTex);

                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var imp = (TextureImporter)AssetImporter.GetAtPath(destPath);
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.mipmapEnabled = true;
                    imp.wrapMode = TextureWrapMode.Repeat;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.npotScale = TextureImporterNPOTScale.None;
                    imp.alphaIsTransparency = false;
                    imp.maxTextureSize = maxSize;
                    imp.textureCompression = TextureImporterCompression.Compressed;
                    CodexArtImportPostprocessor.ApplyWebGlMobileCompression(imp, maxSize);
                    imp.SaveAndReimport();
                }

                // Compute final L-R & T-B seam MAE for logging
                float lrErr = 0f;
                for (int y = 0; y < h; y++)
                    lrErr += ColorDiff(outPix[y * w], outPix[y * w + (w - 1)]);
                float tbErr = 0f;
                for (int x = 0; x < w; x++)
                    tbErr += ColorDiff(outPix[x], outPix[(h - 1) * w + x]);
                Debug.Log($"[SeamlessBake] {destFile} ({w}x{h}) L-R MAE={lrErr / h * 255f:0.00}, T-B MAE={tbErr / w * 255f:0.00}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(src);
            }
        }

        private static void BakeHorizontalPanorama(string relSrc, string destFile, float borderFrac, int maxSize)
        {
            string srcPath = $"{CodexRoot}/{relSrc}";
            string destPath = $"{BakedRoot}/{destFile}";
            if (IsBakedFileUpToDate(srcPath, destPath)) return;
            var src = LoadReadableCopyFromDisk(srcPath);
            if (src == null) return;

            try
            {
                int w = src.width;
                int h = src.height;
                Color[] pix = src.GetPixels();
                Color[] outPix = new Color[w * h];
                int bw = Mathf.Max(8, Mathf.RoundToInt(w * borderFrac));

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float wx = EdgeWeight(x, w, bw);
                        int sx = (x + w / 2) % w;
                        outPix[y * w + x] = Color.Lerp(pix[y * w + sx], pix[y * w + x], wx);
                    }
                }

                var outTex = new Texture2D(w, h, TextureFormat.RGB24, false);
                outTex.SetPixels(outPix);
                outTex.Apply();
                File.WriteAllBytes(Path.GetFullPath(destPath), outTex.EncodeToJPG(90));
                UnityEngine.Object.DestroyImmediate(outTex);

                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var imp = (TextureImporter)AssetImporter.GetAtPath(destPath);
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.mipmapEnabled = false;
                    imp.wrapModeU = TextureWrapMode.Repeat;
                    imp.wrapModeV = TextureWrapMode.Clamp;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.npotScale = TextureImporterNPOTScale.None;
                    imp.maxTextureSize = maxSize;
                    imp.textureCompression = TextureImporterCompression.Compressed;
                    CodexArtImportPostprocessor.ApplyWebGlMobileCompression(imp, maxSize);
                    imp.SaveAndReimport();
                }

                float lrErr = 0f;
                for (int y = 0; y < h; y++)
                    lrErr += ColorDiff(outPix[y * w], outPix[y * w + (w - 1)]);
                Debug.Log($"[SeamlessBake] {destFile} ({w}x{h}) Horizon Wrap MAE={lrErr / h * 255f:0.00}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(src);
            }
        }

        private static void BakeNineSlicePanel(string relSrc, string destFile, bool cleanHorizMidClasp, bool cleanVertMidClasp, Vector4 spriteBorder)
        {
            string srcPath = $"{CodexRoot}/{relSrc}";
            string destPath = $"{BakedRoot}/{destFile}";
            if (IsBakedFileUpToDate(srcPath, destPath)) return;
            var src = LoadReadableCopyFromDisk(srcPath);
            if (src == null) return;

            try
            {
                int w = src.width;
                int h = src.height;
                Color[] pix = src.GetPixels();

                if (cleanHorizMidClasp)
                {
                    // Mirror adjacent clean straight border pixels [184..228] & [284..328] across center [232..280]
                    for (int y = 0; y < h; y++)
                    {
                        if (y > 46 && y < h - 46) continue;
                        for (int x = 230; x <= 282; x++)
                        {
                            float t = (x - 230) / 52f;
                            Color cL = pix[y * w + (182 + (x - 230))];
                            Color cR = pix[y * w + (284 + (x - 230))];
                            pix[y * w + x] = Color.Lerp(cL, cR, t);
                        }
                    }
                }

                if (cleanVertMidClasp)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (x > 46 && x < w - 46) continue;
                        for (int y = 230; y <= 282; y++)
                        {
                            float t = (y - 230) / 52f;
                            Color cB = pix[(182 + (y - 230)) * w + x];
                            Color cT = pix[(284 + (y - 230)) * w + x];
                            pix[y * w + x] = Color.Lerp(cB, cT, t);
                        }
                    }
                }

                WriteNineSliceSpritePng(destPath, w, h, pix, spriteBorder);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(src);
            }
        }

        private static void BakeTrimmedButton9Slice(string relSrc, string destFile, int topTrimTopDown, int bottomTrimTopDown, bool cleanCenterClasps, Vector4 spriteBorder)
        {
            string srcPath = $"{CodexRoot}/{relSrc}";
            string destPath = $"{BakedRoot}/{destFile}";
            if (IsBakedFileUpToDate(srcPath, destPath)) return;
            var src = LoadReadableCopyFromDisk(srcPath);
            if (src == null) return;

            try
            {
                int w = src.width;
                int h = src.height;
                Color[] pix = src.GetPixels();

                // Clean ONLY top/bottom jade rim clasps [y < 42 or y > h-42] so middle background mountains stay seamless
                if (cleanCenterClasps)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (y > 42 && y < h - 42) continue;
                        for (int x = 208; x <= 304; x++)
                        {
                            float t = (x - 208) / 96f;
                            int lx = 160 + (x - 208) / 2;
                            int rx = 304 + (x - 208) / 2;
                            Color patched = Color.Lerp(pix[y * w + lx], pix[y * w + rx], t);
                            float edgeFeather = Mathf.Clamp01(Mathf.Min(x - 208, 304 - x) / 10f);
                            pix[y * w + x] = Color.Lerp(pix[y * w + x], patched, edgeFeather);
                        }
                    }
                }

                // Unity Texture2D coordinates start at bottom (y=0), while PIL/top-down coordinates start at top
                int yMaxUnity = Mathf.Clamp(h - topTrimTopDown, 1, h);
                int yMinUnity = Mathf.Clamp(h - bottomTrimTopDown, 0, yMaxUnity - 1);
                int outH = yMaxUnity - yMinUnity;
                Color[] outPix = new Color[w * outH];
                for (int y = 0; y < outH; y++)
                {
                    Array.Copy(pix, (yMinUnity + y) * w, outPix, y * w, w);
                }

                WriteNineSliceSpritePng(destPath, w, outH, outPix, spriteBorder);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(src);
            }
        }

        private static void WriteNineSliceSpritePng(string destPath, int w, int h, Color[] pixels, Vector4 spriteBorder)
        {
            var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels(pixels);
            outTex.Apply();
            File.WriteAllBytes(Path.GetFullPath(destPath), outTex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(outTex);

            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(destPath);
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                var settings = new TextureImporterSettings();
                imp.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                imp.SetTextureSettings(settings);
                imp.spriteBorder = spriteBorder;
                imp.mipmapEnabled = false;
                imp.alphaIsTransparency = true;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.filterMode = FilterMode.Bilinear;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.maxTextureSize = 512;
                imp.textureCompression = TextureImporterCompression.Compressed;
                CodexArtImportPostprocessor.ApplyWebGlMobileCompression(imp, 256);
                imp.SaveAndReimport();
            }
        }

        private static float EdgeWeight(int coord, int size, int border)
        {
            int dist = Mathf.Min(coord, size - 1 - coord);
            if (dist >= border) return 1f;
            float t = Mathf.Clamp01(dist / (float)border);
            // Smooth cosine fade: 0 at edge, 1 inside
            return 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
        }

        private static float ColorDiff(Color a, Color b)
        {
            return (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)) / 3f;
        }
    }
}

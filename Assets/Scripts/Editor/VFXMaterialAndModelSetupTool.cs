#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class VFXMaterialAndModelSetupTool
{
    private const string VFX_DIR = "Assets/Materials/VFX/";

    [MenuItem("Khí Vận Chi Tử/Setup All VFX Materials and Textures")]
    public static void SetupMaterials()
    {
        Debug.Log("🎨 [VFX Setup] Bắt đầu thiết lập Materials, Shaders và Textures...");

        // 0. Tạo mới các Textures tóe máu với viền trong suốt chuẩn và anti-aliasing
        GenerateBloodTextures();

        // 1. Load Textures
        Texture2D texGlow = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_SoftGlow.png");
        Texture2D texSpark = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_HitSpark.png");
        Texture2D texTaiji = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_Taiji.png");
        Texture2D texRing = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_ShockwaveRing.png");
        Texture2D texRune = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_RuneTelegraph.png");
        Texture2D texBloodDrop = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_BloodDrop.png");
        Texture2D texBloodSplat = AssetDatabase.LoadAssetAtPath<Texture2D>(VFX_DIR + "Tex_VFX_BloodSplat.png");

        Shader urpParticlesUnlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpParticlesUnlit == null) urpParticlesUnlit = Shader.Find("Sprites/Default");
        if (urpUnlit == null) urpUnlit = urpParticlesUnlit;

        // 2. Setup Mat_LightningCore_URP (Additive Soft Glow - LOẠI BỎ HÌNH VUÔNG 0.75M)
        Material matCore = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_LightningCore_URP.mat");
        if (matCore == null)
        {
            matCore = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matCore, VFX_DIR + "Mat_LightningCore_URP.mat");
        }
        matCore.shader = urpParticlesUnlit;
        matCore.SetFloat("_Surface", 1); // Transparent
        matCore.SetFloat("_Blend", 1);   // Additive
        matCore.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matCore.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        matCore.SetFloat("_ZWrite", 0);
        matCore.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));
        if (texGlow != null) matCore.SetTexture("_BaseMap", texGlow);
        EditorUtility.SetDirty(matCore);

        // 3. Setup Mat_LightningSparks_URP (Additive Diamond Hit Sparks - TIA LỬA SẮC NÉT)
        Material matSparks = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_LightningSparks_URP.mat");
        if (matSparks == null)
        {
            matSparks = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matSparks, VFX_DIR + "Mat_LightningSparks_URP.mat");
        }
        matSparks.shader = urpParticlesUnlit;
        matSparks.SetFloat("_Surface", 1); // Transparent
        matSparks.SetFloat("_Blend", 1);   // Additive
        matSparks.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matSparks.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        matSparks.SetFloat("_ZWrite", 0);
        matSparks.SetColor("_BaseColor", new Color(1f, 0.95f, 0.7f, 1f));
        if (texSpark != null) matSparks.SetTexture("_BaseMap", texSpark);
        EditorUtility.SetDirty(matSparks);

        // 4. Setup Mat_LightningGlow_URP (Additive Smooth Radial Glow)
        Material matGlow = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_LightningGlow_URP.mat");
        if (matGlow == null)
        {
            matGlow = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matGlow, VFX_DIR + "Mat_LightningGlow_URP.mat");
        }
        matGlow.shader = urpParticlesUnlit;
        matGlow.SetFloat("_Surface", 1);
        matGlow.SetFloat("_Blend", 1);
        matGlow.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matGlow.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        matGlow.SetFloat("_ZWrite", 0);
        matGlow.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));
        if (texGlow != null) matGlow.SetTexture("_BaseMap", texGlow);
        EditorUtility.SetDirty(matGlow);

        // 5. Setup Mat_YinYangTaiji_URP (Alpha Blended B&W Taiji Disc - ÂM DƯƠNG THÁI CỰC ĐEN TRẮNG)
        Material matTaiji = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_YinYangTaiji_URP.mat");
        if (matTaiji == null)
        {
            matTaiji = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matTaiji, VFX_DIR + "Mat_YinYangTaiji_URP.mat");
        }
        matTaiji.shader = urpParticlesUnlit;
        matTaiji.SetFloat("_Surface", 1); // Transparent
        matTaiji.SetFloat("_Blend", 0);   // Alpha Blended (cho phép hiển thị màu đen obsidian)
        matTaiji.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matTaiji.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        matTaiji.SetFloat("_ZWrite", 0);
        matTaiji.SetFloat("_Cull", 0);    // Two-sided
        matTaiji.SetColor("_BaseColor", Color.white);
        if (texTaiji != null) matTaiji.SetTexture("_BaseMap", texTaiji);
        EditorUtility.SetDirty(matTaiji);

        // 6. Setup Mat_GoldenPalm_URP (Radiant Golden Palm - ẤN CHƯỞNG VÀNG KIM BỪNG SÁNG)
        Material matPalm = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_GoldenPalm_URP.mat");
        if (matPalm == null)
        {
            matPalm = new Material(urpUnlit);
            AssetDatabase.CreateAsset(matPalm, VFX_DIR + "Mat_GoldenPalm_URP.mat");
        }
        matPalm.shader = urpUnlit;
        matPalm.SetFloat("_Surface", 1); // Transparent
        matPalm.SetFloat("_Blend", 0);   // Alpha
        matPalm.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matPalm.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        matPalm.SetFloat("_ZWrite", 0);
        matPalm.SetColor("_BaseColor", new Color(1f, 0.84f, 0.18f, 0.95f)); // Vàng kim hổ phách rực rỡ
        EditorUtility.SetDirty(matPalm);

        // 7. Setup Mat_YinBlack_URP (Obsidian Black Material cho luồng Âm)
        Material matBlack = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_YinBlack_URP.mat");
        if (matBlack == null)
        {
            matBlack = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matBlack, VFX_DIR + "Mat_YinBlack_URP.mat");
        }
        matBlack.shader = urpParticlesUnlit;
        matBlack.SetFloat("_Surface", 1);
        matBlack.SetFloat("_Blend", 0); // Alpha Blended
        matBlack.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matBlack.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        matBlack.SetFloat("_ZWrite", 0);
        matBlack.SetColor("_BaseColor", new Color(0.04f, 0.04f, 0.06f, 0.95f)); // Đen tuyền huyền ảo
        if (texGlow != null) matBlack.SetTexture("_BaseMap", texGlow);
        EditorUtility.SetDirty(matBlack);

        // 8. Setup Mat_BloodSplatter_URP (Vật liệu Tóe Máu Tươi - Huyết Sắc Tinh Tế)
        Material matBlood = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_BloodSplatter_URP.mat");
        if (matBlood == null)
        {
            matBlood = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matBlood, VFX_DIR + "Mat_BloodSplatter_URP.mat");
        }
        matBlood.shader = urpParticlesUnlit;
        matBlood.SetFloat("_Surface", 1); // Transparent
        matBlood.SetFloat("_Blend", 0);   // Alpha Blended cho độ đặc sệt của giọt máu
        matBlood.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matBlood.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        matBlood.SetFloat("_ZWrite", 0);
        matBlood.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        matBlood.SetFloat("_Cull", 0);
        matBlood.renderQueue = 3100;
        matBlood.SetOverrideTag("RenderType", "Transparent");
        matBlood.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));
        if (texBloodDrop != null) matBlood.SetTexture("_BaseMap", texBloodDrop);
        EditorUtility.SetDirty(matBlood);

        // 9. Setup Mat_BloodSplat_URP (Vệt Máu Bung Nở - Splatter Core)
        Material matSplat = AssetDatabase.LoadAssetAtPath<Material>(VFX_DIR + "Mat_BloodSplat_URP.mat");
        if (matSplat == null)
        {
            matSplat = new Material(urpParticlesUnlit);
            AssetDatabase.CreateAsset(matSplat, VFX_DIR + "Mat_BloodSplat_URP.mat");
        }
        matSplat.shader = urpParticlesUnlit;
        matSplat.SetFloat("_Surface", 1);
        matSplat.SetFloat("_Blend", 0);
        matSplat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        matSplat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        matSplat.SetFloat("_ZWrite", 0);
        matSplat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        matSplat.SetFloat("_Cull", 0);
        matSplat.renderQueue = 3100;
        matSplat.SetOverrideTag("RenderType", "Transparent");
        matSplat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));
        if (texBloodSplat != null) matSplat.SetTexture("_BaseMap", texBloodSplat);
        EditorUtility.SetDirty(matSplat);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ [VFX Setup] Đã hoàn tất cấu hình tất cả VFX Materials chuẩn URP!");
    }

    public static void GenerateBloodTextures()
    {
        int size = 256;

        // 1. Tex_VFX_BloodDrop.png (Giọt máu thon nhọn khí động học, viền trong suốt rộng để không bị vệt vuông)
        Texture2D texDrop = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] dropPixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - 128f);
                float alpha = 0f;

                // Phần đầu giọt máu: hình tròn bán kính 22 tại (128, 150)
                if (y >= 150)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(128f, 150f));
                    if (dist < 22f)
                    {
                        alpha = Mathf.SmoothStep(1f, 0f, (dist - 14f) / 8f);
                    }
                }
                // Phần thân và đuôi giọt máu: vuốt thon đều từ y=150 xuống y=80
                else if (y >= 80)
                {
                    float t = (y - 80f) / 70f; // 0 ở chóp đuôi, 1 ở đầu giọt
                    float curRadius = 22f * Mathf.Pow(t, 0.70f);
                    if (dx < curRadius)
                    {
                        alpha = Mathf.SmoothStep(1f, 0f, (dx - (curRadius * 0.65f)) / Mathf.Max(1f, curRadius * 0.35f));
                    }
                }

                dropPixels[y * size + x] = alpha > 0.001f ? new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)) : Color.clear;
            }
        }
        texDrop.SetPixels(dropPixels);
        texDrop.Apply();
        string dropPath = VFX_DIR + "Tex_VFX_BloodDrop.png";
        System.IO.File.WriteAllBytes(dropPath, texDrop.EncodeToPNG());
        AssetDatabase.ImportAsset(dropPath, ImportAssetOptions.ForceUpdate);

        var dropImporter = AssetImporter.GetAtPath(dropPath) as TextureImporter;
        if (dropImporter != null)
        {
            dropImporter.textureType = TextureImporterType.Default;
            dropImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            dropImporter.alphaIsTransparency = true;
            dropImporter.mipmapEnabled = false;
            dropImporter.wrapMode = TextureWrapMode.Clamp;
            dropImporter.filterMode = FilterMode.Bilinear;
            dropImporter.SaveAndReimport();
        }

        // 2. Tex_VFX_BloodSplat.png (Vệt máu bung nở trung tâm + 5 giọt vệ tinh)
        Texture2D texSplat = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] splatPixels = new Color[size * size];
        Vector2[] satellites = new Vector2[]
        {
            new Vector2(128f + 32f, 128f + 25f),
            new Vector2(128f - 30f, 128f + 28f),
            new Vector2(128f + 25f, 128f - 32f),
            new Vector2(128f - 28f, 128f - 24f),
            new Vector2(128f + 42f, 128f - 8f)
        };
        float[] satRadii = new float[] { 7f, 6.5f, 8f, 6f, 5.5f };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y);
                // Vết nổ chính ở giữa
                float dCenter = Vector2.Distance(p, new Vector2(128f, 128f));
                float a = 0f;
                if (dCenter < 26f)
                {
                    a = Mathf.SmoothStep(1f, 0f, (dCenter - 16f) / 10f);
                }

                // Các giọt vệ tinh
                for (int i = 0; i < satellites.Length; i++)
                {
                    float dSat = Vector2.Distance(p, satellites[i]);
                    float r = satRadii[i];
                    if (dSat < r)
                    {
                        float satA = Mathf.SmoothStep(1f, 0f, (dSat - (r * 0.5f)) / (r * 0.5f));
                        a = Mathf.Max(a, satA);
                    }
                }

                splatPixels[y * size + x] = a > 0.001f ? new Color(1f, 1f, 1f, Mathf.Clamp01(a)) : Color.clear;
            }
        }
        texSplat.SetPixels(splatPixels);
        texSplat.Apply();
        string splatPath = VFX_DIR + "Tex_VFX_BloodSplat.png";
        System.IO.File.WriteAllBytes(splatPath, texSplat.EncodeToPNG());
        AssetDatabase.ImportAsset(splatPath, ImportAssetOptions.ForceUpdate);

        var splatImporter = AssetImporter.GetAtPath(splatPath) as TextureImporter;
        if (splatImporter != null)
        {
            splatImporter.textureType = TextureImporterType.Default;
            splatImporter.alphaSource = TextureImporterAlphaSource.FromInput;
            splatImporter.alphaIsTransparency = true;
            splatImporter.mipmapEnabled = false;
            splatImporter.wrapMode = TextureWrapMode.Clamp;
            splatImporter.filterMode = FilterMode.Bilinear;
            splatImporter.SaveAndReimport();
        }
    }
}
#endif


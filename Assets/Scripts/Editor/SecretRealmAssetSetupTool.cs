#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SecretRealmAssetSetupTool
{
    [MenuItem("TuTien/SecretRealm/Setup Sprites Import Settings")]
    public static void SetupSprites()
    {
        string dir = "Assets/Resources/SecretRealm";
        if (!Directory.Exists(dir)) return;

        string[] files = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;
            string assetPath = file.Replace("\\", "/");
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }
                if (changed)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    Debug.Log($"[SecretRealmAsset] Configured Sprite for: {assetPath}");
                }
            }
        }
    }
}
#endif


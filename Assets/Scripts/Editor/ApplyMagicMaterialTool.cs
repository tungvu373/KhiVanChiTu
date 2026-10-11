using UnityEngine;
using UnityEditor;

public class ApplyMagicMaterialTool : EditorWindow
{
    [MenuItem("Tools/Apply Magic Array Material to Skills")]
    public static void ApplyMaterial()
    {
        // Đường dẫn chính xác tới Material mà bạn vừa tạo
        string path = "Assets/Materials/Mat_MagicArray.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        if (mat == null)
        {
            Debug.LogError("❌ Không tìm thấy Material tại: " + path + ". Hãy chắc chắn bạn đã tạo và đặt đúng tên thư mục và file.");
            return;
        }

        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            // Gán Material vào biến mới được thêm trong CombatManager
            combatManager.magicArrayMaterial = mat;
            EditorUtility.SetDirty(combatManager);
        }

        // Gán Material vào Prefab LightningVFX (Lôi Kiếp)
        string vfxPath = "Assets/Prefabs/LightningVFX.prefab";
        GameObject lightningPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath);
        if (lightningPrefab != null)
        {
            ParticleSystemRenderer psr = lightningPrefab.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.material = mat;
                EditorUtility.SetDirty(lightningPrefab);
                AssetDatabase.SaveAssets();
            }
        }

        CultivationManager cult = FindObjectOfType<CultivationManager>();
        if (cult != null && lightningPrefab != null)
        {
            cult.lightningVFXPrefab = lightningPrefab;
            EditorUtility.SetDirty(cult);
        }

        Debug.Log("✅ Đã gán Mat_MagicArray thành công cho Lôi Kiếp, Lôi Phạt và Ấn Chưởng!");
    }
}

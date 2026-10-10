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
            
            Debug.Log("✅ Đã gán Material thành công! Bây giờ Lôi Phạt và Ấn Chưởng sẽ sử dụng chung Material này.");
        }
        else
        {
            Debug.LogError("❌ Không tìm thấy CombatManager trong Scene hiện tại! Bạn đã mở đúng Scene chứa game chưa?");
        }
    }
}

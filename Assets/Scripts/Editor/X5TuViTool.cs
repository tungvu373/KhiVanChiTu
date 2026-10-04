using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class X5TuViTool
{
    [MenuItem("TuTien/Fix/X5 Tu Vi All Stages")]
    public static void MultiplyTuVi()
    {
        string[] guids = AssetDatabase.FindAssets("t:CultivationStageData", new[] { "Assets/ScriptableObjects/Stages" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy CultivationStageData nào trong Assets/ScriptableObjects/Stages");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CultivationStageData data = AssetDatabase.LoadAssetAtPath<CultivationStageData>(path);
            if (data != null)
            {
                data.requiredTuVi *= 5f;
                EditorUtility.SetDirty(data);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Tool] Đã nhân 5 yêu cầu Tu Vi cho {guids.Length} cảnh giới thành công!");
    }
}

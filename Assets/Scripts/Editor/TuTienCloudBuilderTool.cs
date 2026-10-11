#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class TuTienCloudBuilderTool
{
    [MenuItem("TuTien/Môi trường/Tạo Mây Tiên (Particle System đi theo Player)")]
    public static void CreateCloudSystemInScene()
    {
        TuTienCloudSystem existing = Object.FindObjectOfType<TuTienCloudSystem>();
        if (existing != null)
        {
            existing.SetupCloudParticleSystem();
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("<color=cyan>[TuTien] Đã làm mới hệ thống Mây Tiên (TuTienCloudSystem) có sẵn trong Scene!</color>");
            return;
        }

        GameObject cloudObj = new GameObject("TuTienCloudSystem");
        TuTienCloudSystem system = cloudObj.AddComponent<TuTienCloudSystem>();
        system.SetupCloudParticleSystem();

        Selection.activeGameObject = cloudObj;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>[TuTien] ĐÃ TẠO THÀNH CÔNG Hệ Thống Mây Tiên (TuTienCloudSystem) lơ lửng bồng bềnh đi theo Player!</color>");
    }
}
#endif

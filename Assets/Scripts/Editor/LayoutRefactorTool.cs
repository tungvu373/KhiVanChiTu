#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LayoutRefactorTool
{
    [MenuItem("TuTien/Giai đoạn 7/1. Tối ưu Bố cục (Move Player)")]
    public static void RefactorLayout()
    {
        // 1. Di chuyển Player sang trái theo tọa độ yêu cầu (-1.5, 0, -5)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(-1.5f, 0, -5f);
        }

        // 2. Chỉnh CombatManager để sinh quái nhanh hơn (nhịp độ cuồng nhiệt)
        CombatManager cm = Object.FindObjectOfType<CombatManager>();
        if (cm != null)
        {
            cm.spawnInterval = 0.5f; // Quái ra liên tục mỗi 0.5s
            EditorUtility.SetDirty(cm);
        }
        
        // 3. Chỉnh lại góc Camera để bao quát khu vực bên trái tốt hơn
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(-1.5f, 10f, -12f);
            Camera.main.transform.rotation = Quaternion.Euler(45f, 0, 0);
        }
        
        Debug.Log("✅ Đã dời Player sang tọa độ (-1.5, 0, -5) và tăng tốc độ sinh quái!");
    }
}
#endif

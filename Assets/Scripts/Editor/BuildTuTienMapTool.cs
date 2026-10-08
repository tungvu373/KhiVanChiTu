using UnityEngine;
using UnityEditor;

public class BuildTuTienMapTool
{
    [MenuItem("Tools/Xây Map Tu Tiên (Cuộn Vô Tận)")]
    public static void BuildMap()
    {
        // Xóa map cũ
        GameObject oldMap = GameObject.Find("TuTien_PathChunk");
        if (oldMap != null) Undo.DestroyObjectImmediate(oldMap);
        oldMap = GameObject.Find("TuTien_CombatMap");
        if (oldMap != null) Undo.DestroyObjectImmediate(oldMap);

        // Tạo Root Chunk dài 20 đơn vị
        GameObject root = new GameObject("TuTien_PathChunk");
        Undo.RegisterCreatedObjectUndo(root, "Create Path Chunk");
        root.transform.position = Vector3.zero;

        string basePath = "Assets/Holotna/Mountain/Prefabs/";
        GameObject mountainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "Mountain01.prefab");
        GameObject rock1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "Rock01.prefab");
        GameObject tree1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "Tree01A.prefab");
        GameObject grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "Grass01A.prefab");
        GameObject bridgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "Bridge01.prefab");

        // --- 1. LÀM ĐƯỜNG ĐI (THÔNG THOÁNG Ở GIỮA X = 0) ---
        // Sử dụng Rock1 làm phẳng để tạo con đường đá lởm chởm dài 20 đơn vị
        for (int z = -10; z <= 10; z += 5)
        {
            GameObject ground = PrefabUtility.InstantiatePrefab(rock1Prefab) as GameObject;
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(0, -0.5f, z);
            ground.transform.localScale = new Vector3(3f, 0.1f, 3f); // Dẹt làm đường
            ground.name = "Đường Đi";
        }

        // --- 2. VÁCH NÚI VÀ CÂY BÊN TRÁI (X = -4 đến -8) ---
        GameObject leftMountain = PrefabUtility.InstantiatePrefab(mountainPrefab) as GameObject;
        leftMountain.transform.SetParent(root.transform);
        leftMountain.transform.position = new Vector3(-8, -2f, 0);
        leftMountain.transform.localScale = new Vector3(1.5f, 1.5f, 2.5f);
        leftMountain.name = "Núi Trái";

        for (int z = -8; z <= 8; z += 6)
        {
            GameObject tree = PrefabUtility.InstantiatePrefab(tree1Prefab) as GameObject;
            tree.transform.SetParent(root.transform);
            tree.transform.position = new Vector3(-3.5f, 0, z); // Nằm sát lề trái
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            GameObject grass = PrefabUtility.InstantiatePrefab(grassPrefab) as GameObject;
            grass.transform.SetParent(root.transform);
            grass.transform.position = new Vector3(-2.5f, 0, z + 1);
        }

        // --- 3. VÁCH NÚI VÀ CÂY BÊN PHẢI (X = 4 đến 8) ---
        GameObject rightMountain = PrefabUtility.InstantiatePrefab(mountainPrefab) as GameObject;
        rightMountain.transform.SetParent(root.transform);
        rightMountain.transform.position = new Vector3(8, -2f, 5);
        rightMountain.transform.localScale = new Vector3(1.5f, 1.2f, 2f);
        rightMountain.transform.rotation = Quaternion.Euler(0, 180, 0);
        rightMountain.name = "Núi Phải";

        for (int z = -6; z <= 8; z += 7)
        {
            GameObject tree = PrefabUtility.InstantiatePrefab(tree1Prefab) as GameObject;
            tree.transform.SetParent(root.transform);
            tree.transform.position = new Vector3(3.5f, 0, z); // Nằm sát lề phải
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            GameObject grass = PrefabUtility.InstantiatePrefab(grassPrefab) as GameObject;
            grass.transform.SetParent(root.transform);
            grass.transform.position = new Vector3(2.5f, 0, z - 1);
        }
        
        // --- 4. CỔNG CHÀO TẠO CHIỀU SÂU MỖI CHUNK (Tùy chọn) ---
        // Đặt một cây cầu trên cao vắt ngang
        if (bridgePrefab != null)
        {
            GameObject bridge = PrefabUtility.InstantiatePrefab(bridgePrefab) as GameObject;
            bridge.transform.SetParent(root.transform);
            bridge.transform.position = new Vector3(0, 4f, 8f); // Đặt trên cao 4m
            bridge.name = "Cầu Vắt Ngang";
        }

        Debug.Log("[Thành công] Đã xây xong Chunk Map đi bộ (Trục giữa đã được dọn sạch). Có thể Duplicate thành 3 Chunk để chạy cuộn!");
    }
}

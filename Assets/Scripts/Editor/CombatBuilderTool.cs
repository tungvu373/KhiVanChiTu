#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CombatBuilderTool
{
    [MenuItem("TuTien/Tạo hệ thống Combat 3D (Giai đoạn 4)")]
    public static void GenerateCombatSystem()
    {
        // 1. Tạo Player Center (3D Capsule)
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.blue); // URP
            block.SetColor("_Color", Color.blue); // Built-in
            player.GetComponent<Renderer>().SetPropertyBlock(block);
            
            player.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
        }

        // 2. Tạo Phi Kiếm 3D (Flying Sword)
        GameObject sword = GameObject.Find("FlyingSword");
        if (sword == null)
        {
            sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "FlyingSword";
            
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.cyan); 
            block.SetColor("_Color", Color.cyan); 
            sword.GetComponent<Renderer>().SetPropertyBlock(block);
            
            // Ép dẹt Khối lập phương để ra hình thanh kiếm
            sword.transform.localScale = new Vector3(0.2f, 0.2f, 1f); 
            sword.transform.position = new Vector3(2, 0, 0);
            
            sword.AddComponent<FlyingSword>();

            TrailRenderer trail = sword.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.cyan;
            trail.endColor = new Color(0, 1, 1, 0);
            
            Undo.RegisterCreatedObjectUndo(sword, "Create Sword");
        }

        // 3. Tạo Prefab Yêu Thú (Enemy 3D)
        GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemyObj.name = "EnemyPrefab";
        
        MaterialPropertyBlock enemyBlock = new MaterialPropertyBlock();
        enemyBlock.SetColor("_BaseColor", Color.red); 
        enemyBlock.SetColor("_Color", Color.red); 
        enemyObj.GetComponent<Renderer>().SetPropertyBlock(enemyBlock);
        
        enemyObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        enemyObj.AddComponent<Enemy>();
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
            
        string prefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
        Object.DestroyImmediate(enemyObj);

        // 4. Liên kết vào CombatManager (Nếu chưa có thì tự động gắn vào Managers)
        CombatManager combatManager = Object.FindObjectOfType<CombatManager>();
        if (combatManager == null)
        {
            GameObject managers = GameObject.Find("Managers");
            if (managers == null) managers = new GameObject("Managers");
            combatManager = managers.AddComponent<CombatManager>();
        }

        if (combatManager != null)
        {
            combatManager.playerTransform = player.transform;
            combatManager.enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorUtility.SetDirty(combatManager);
        }
        
        // Cấu hình Camera 3D (Góc nhìn từ trên chéo xuống)
        if (Camera.main != null) 
        {
            // Đẩy Camera sang PHẢI (X = 4.5) để gốc tọa độ (Player X = 0) lọt vào TRÁI màn hình
            Camera.main.transform.position = new Vector3(4.5f, 12f, -10f); 
            Camera.main.transform.rotation = Quaternion.Euler(50f, 0, 0); 
            Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.2f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        
        Debug.Log("✅ Đã tạo xong Hệ thống Combat 3D: Player, Phi Kiếm, Yêu Thú và CombatManager!");
    }
}
#endif

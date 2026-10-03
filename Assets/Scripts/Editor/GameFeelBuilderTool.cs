#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GameFeelBuilderTool
{
    [MenuItem("TuTien/Tạo hệ thống Game Feel & Đóng gói (Giai đoạn 7)")]
    public static void GenerateGameFeel()
    {
        // 1. Setup Camera Shake
        if (Camera.main != null)
        {
            if (Camera.main.gameObject.GetComponent<CameraShake>() == null)
            {
                Camera.main.gameObject.AddComponent<CameraShake>();
            }
        }
        
        // 2. Tạo Prefab Damage Popup
        GameObject popupObj = new GameObject("DamagePopupPrefab");
        TextMesh tm = popupObj.AddComponent<TextMesh>();
        tm.text = "100";
        tm.fontSize = 40;
        tm.characterSize = 0.2f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        
        // Sử dụng Font LegacyRuntime mặc định cho nổi bật
        Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (runtimeFont != null)
        {
            tm.font = runtimeFont;
            MeshRenderer mr = popupObj.GetComponent<MeshRenderer>();
            mr.material = runtimeFont.material;
        }
        
        popupObj.AddComponent<DamagePopup>();
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        string prefabPath = "Assets/Prefabs/DamagePopup.prefab";
        GameObject savedPopup = PrefabUtility.SaveAsPrefabAsset(popupObj, prefabPath);
        Object.DestroyImmediate(popupObj);
        
        // 3. Gán Prefab vào CombatManager
        CombatManager cm = Object.FindObjectOfType<CombatManager>();
        if (cm != null)
        {
            cm.damagePopupPrefab = savedPopup;
            EditorUtility.SetDirty(cm);
        }
        
        // 4. Tạo VFX Sấm sét (Breakthrough / Boss)
        GameObject lightning = new GameObject("LightningVFX_Prefab");
        ParticleSystem ps = lightning.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 30f; 
        main.startSize = 1.5f;
        main.startColor = new Color(0.5f, 0.8f, 1f, 1f); // Màu xanh sét
        main.playOnAwake = true; // Cho tự nổ khi Instantiate
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) }); // Bùng nổ 20 tia
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 10f;
        shape.radius = 0.5f;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.2f;

        lightning.transform.rotation = Quaternion.Euler(90, 0, 0); // Bắn từ trên xuống (Trời đánh)
        
        // Script tự hủy sau khi nổ xong (1 giây)
        lightning.AddComponent<DestroyAfterTime>().lifeTime = 1.5f;
        
        string vfxPath = "Assets/Prefabs/LightningVFX.prefab";
        GameObject savedVFX = PrefabUtility.SaveAsPrefabAsset(lightning, vfxPath);
        Object.DestroyImmediate(lightning);
        
        CultivationManager cult = Object.FindObjectOfType<CultivationManager>();
        if (cult != null)
        {
            cult.lightningVFXPrefab = savedVFX;
            EditorUtility.SetDirty(cult);
        }
        
        Debug.Log("✅ Đã hoàn thiện Game Feel! (Camera Shake, Damage Popup, VFX Sấm Sét)");
        Debug.Log("🚀 Giao diện, Hiệu ứng và Tính năng đã hội tụ đủ. Trò chơi đã SẴN SÀNG để Build (Ctrl+B)!");
    }
}
#endif

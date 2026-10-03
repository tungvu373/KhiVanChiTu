#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EnvironmentBuilderTool
{
    [MenuItem("TuTien/Giai đoạn 8/1. Tạo bối cảnh Tu Tiên (Môi trường)")]
    public static void GenerateEnvironment()
    {
        // 1. Chỉnh màu Camera thành bầu trời đêm lung linh
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.15f); // Đêm tối tu tiên
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            
            // Thêm nhạc nền
            AudioSource bgm = Camera.main.gameObject.GetComponent<AudioSource>();
            if (bgm == null) bgm = Camera.main.gameObject.AddComponent<AudioSource>();
            bgm.loop = true;
            bgm.volume = 0.5f;
            // Ở đây nếu có file mp3 thì gán vào, tạm thời để trống Audio Clip
        }
        
        // 2. Tạo mặt đất (Đài tu luyện / Trận pháp Ngọc Thạch)
        GameObject ground = GameObject.Find("TuTienGround");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "TuTienGround";
            // Đặt hơi xê dịch sang phải một chút để khớp với vị trí phi kiếm và quái
            ground.transform.position = new Vector3(2f, -1.5f, 5f);
            ground.transform.localScale = new Vector3(30f, 0.5f, 30f);
            
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            // Màu xanh ngọc thạch bí ẩn
            block.SetColor("_BaseColor", new Color(0.05f, 0.2f, 0.15f)); 
            block.SetColor("_Color", new Color(0.05f, 0.2f, 0.15f));
            ground.GetComponent<Renderer>().SetPropertyBlock(block);
        }
        
        // 3. Đèn đóm (Tạo không khí)
        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight == null)
        {
            dirLight = new GameObject("Directional Light");
            Light light = dirLight.AddComponent<Light>();
            light.type = LightType.Directional;
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        Light l = dirLight.GetComponent<Light>();
        l.color = new Color(0.7f, 0.8f, 1f); // Ánh trăng xanh
        l.intensity = 1.2f;

        // 4. Tạo hiệu ứng sương mù (Linh Khí lượn lờ)
        GameObject mist = GameObject.Find("LingQiVFX");
        if (mist == null)
        {
            mist = new GameObject("LingQiVFX");
            mist.transform.position = new Vector3(2f, -0.5f, 5f);
            
            ParticleSystem ps = mist.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 5f;
            main.startLifetime = 8f;
            main.startSpeed = 0.5f; // Bay lờ đờ
            main.startSize = 3f;
            main.startColor = new Color(0.6f, 0.9f, 1f, 0.15f); // Trắng xanh nhạt trong suốt
            main.maxParticles = 200;
            
            var emission = ps.emission;
            emission.rateOverTime = 30f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 1f, 30f); // Phủ kín cái đài tu luyện
            
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        Debug.Log("✅ Đã tạo xong Bối cảnh Tu Tiên: Trận pháp Ngọc Thạch dưới Ánh trăng ngập tràn Linh Khí sương mù!");
    }
}
#endif

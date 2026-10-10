using UnityEngine;

// --- Hiệu ứng Tia sét (Lightning Line) ---
public class LightningVFX : MonoBehaviour
{
    private LineRenderer lr;
    private float lifetime = 0.3f;

    public void Setup(Vector3 start, Vector3 end, Color color, Material customMat = null)
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 5; // Tạm 5 khúc gấp khúc
        lr.startWidth = 0.5f;
        lr.endWidth = 0.1f;
        
        // Dùng shader Unlit/Color để phát sáng hoặc customMat
        if (customMat != null)
        {
            lr.material = customMat;
        }
        else
        {
            lr.material = new Material(Shader.Find("Sprites/Default")); 
            lr.startColor = color;
            lr.endColor = Color.white;
        }

        // Tạo đường gấp khúc ngẫu nhiên
        lr.SetPosition(0, start);
        for (int i = 1; i < 4; i++)
        {
            Vector3 pos = Vector3.Lerp(start, end, i / 4f);
            pos += Random.insideUnitSphere * 1.5f; // Độ giật của sét
            lr.SetPosition(i, pos);
        }
        lr.SetPosition(4, end);

        // Ánh sáng loé lên
        Light light = gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = 5f;
        light.range = 15f;

        // Tạo tia lửa điện dưới đất (Sparks Particle System)
        GameObject sparkObj = new GameObject("Sparks");
        sparkObj.transform.position = end;
        ParticleSystem sparkPs = sparkObj.AddComponent<ParticleSystem>();
        sparkPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = sparkPs.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = color; // Màu sét
        
        var emission = sparkPs.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 30) });
        
        var shape = sparkPs.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        
        var renderer = sparkPs.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f; // Tia lửa dãn dài ra
        
        sparkPs.Play();
        Destroy(sparkObj, 1f);
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0) Destroy(gameObject);
        else if (lr != null)
        {
            // Nhấp nháy alpha
            Color c = lr.startColor;
            c.a = Random.Range(0.2f, 1f);
            lr.startColor = c;
        }
    }
}

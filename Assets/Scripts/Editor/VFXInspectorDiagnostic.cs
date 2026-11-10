#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class VFXInspectorDiagnostic
{
    [MenuItem("Khí Vận Chi Tử/Diagnose Hit VFX")]
    public static void Diagnose()
    {
        string report = "";
        report += "=== [VFX DIAGNOSTIC] ===\n";
        
        var v = HitVFXManager.Instance;
        if (v == null)
        {
            File.WriteAllText("Temp/diagnostic.txt", "HitVFXManager.Instance is NULL!");
            return;
        }

        report += $"GameObject: {v.gameObject.name}\n";
        report += $"sparkMaterial: {(v.sparkMaterial != null ? v.sparkMaterial.name : "NULL")}\n";
        report += $"critMaterial: {(v.critMaterial != null ? v.critMaterial.name : "NULL")}\n";
        report += $"glowMaterial: {(v.glowMaterial != null ? v.glowMaterial.name : "NULL")}\n";
        report += $"bloodMaterial: {(v.bloodMaterial != null ? v.bloodMaterial.name : "NULL")}\n";
        if (v.bloodMaterial != null)
        {
            report += $"bloodMaterial.shader: {v.bloodMaterial.shader.name}\n";
            Texture bTex = v.bloodMaterial.mainTexture;
            report += $"bloodMaterial.mainTexture: {(bTex != null ? bTex.name : "NULL")}\n";
            if (bTex is Texture2D t2d) report += $"tex format: {t2d.format}, width: {t2d.width}, height: {t2d.height}\n";
            report += $"bloodMaterial.renderQueue: {v.bloodMaterial.renderQueue}\n";
            string[] kw = v.bloodMaterial.shaderKeywords;
            report += $"bloodMaterial.keywords: {string.Join(", ", kw)}\n";
            var s = v.bloodMaterial.shader;
            report += $"Shader property count: {s.GetPropertyCount()}\n";
            for (int i = 0; i < s.GetPropertyCount(); i++)
            {
                if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    string propName = s.GetPropertyName(i);
                    Texture tex = v.bloodMaterial.GetTexture(propName);
                    report += $"TexProp: {propName} = {(tex != null ? tex.name : "NULL")}\n";
                }
                else if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Float || s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Range)
                {
                    string propName = s.GetPropertyName(i);
                    report += $"FloatProp: {propName} = {v.bloodMaterial.GetFloat(propName)}\n";
                }
            }
        }

        Vector3 testPos = new Vector3(0, 1.5f, 0);
        v.SpawnHitImpact(testPos, Vector3.forward, false);

        var fShield = typeof(HitVFXManager).GetField("shieldBreakParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var psShield = fShield != null ? fShield.GetValue(v) as ParticleSystem : null;
        if (psShield != null)
        {
            var psrS = psShield.GetComponent<ParticleSystemRenderer>();
            report += $"shieldBreak renderMode: {psrS.renderMode}, material: {(psrS.sharedMaterial != null ? psrS.sharedMaterial.name : "NULL")}\n";
        }
        var fSparks = typeof(HitVFXManager).GetField("sparkParticles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ps = fSparks != null ? fSparks.GetValue(v) as ParticleSystem : null;
        if (ps != null)
        {
            report += $"sparkParticles GameObject: {ps.gameObject.name}, activeInHierarchy={ps.gameObject.activeInHierarchy}\n";
            report += $"particleCount={ps.particleCount}\n";
            report += $"main.startLifetime={ps.main.startLifetime.constant}\n";
            report += $"main.startSize={ps.main.startSize.constantMin} to {ps.main.startSize.constantMax}\n";
            var psr = ps.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                report += $"psr.renderMode={psr.renderMode}\n";
                report += $"psr.sharedMaterial={(psr.sharedMaterial != null ? psr.sharedMaterial.name : "NULL")}\n";
                report += $"psr.velocityScale={psr.velocityScale}, lenScale={psr.lengthScale}\n";
            }
        }
        else
        {
            report += "sparkParticles is NULL!\n";
        }

        File.WriteAllText("Temp/diagnostic.txt", report);
        Debug.Log("Diagnostic written to Temp/diagnostic.txt:\n" + report);
    }
}
#endif

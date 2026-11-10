using UnityEngine;

/// <summary>
/// Hiệu ứng Vòng Trận Pháp Âm Dương (Thái Cực Trận Đồ):
/// 1. Vòng trận Thái Cực Đen - Trắng (Taiji Disc) nằm sát mặt đất xoay tròn uyển chuyển.
/// 2. Luồng Dương (Bạch Quang): Khối cầu sáng trắng thuần khiết vạch nên vệt sáng hào quang.
/// 3. Luồng Âm (Hắc Quang): Khối cầu hắc diệu thạch obsidian vạch nên vệt khói đen kỳ bí.
/// Hai luồng âm dương đối xứng xoay quanh chu vi trận đồ, tỏa khí tức cân bằng thiên địa.
/// </summary>
public class AuraVFX : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float discRotateSpeed = 35f;
    public float orbRotateSpeed = 160f;

    private GameObject groundDiscObj;
    private GameObject yangOrb;
    private GameObject yinOrb;
    private float currentAngle = 0f;
    private float currentDiscAngle = 0f;
    private const float RADIUS = 1.35f;
    private const float DISC_SIZE = 3.0f;

    public void SetupYinYang()
    {
        // Xóa child cũ nếu có
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // 1. Load Materials chuẩn URP
        Material taijiMat = null;
        Material whiteGlowMat = null;
        Material blackYinMat = null;
#if UNITY_EDITOR
        taijiMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_YinYangTaiji_URP.mat");
        whiteGlowMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_LightningGlow_URP.mat");
        blackYinMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VFX/Mat_YinBlack_URP.mat");
#endif
        Shader urpParticles = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
        if (taijiMat == null) taijiMat = new Material(urpParticles);
        if (whiteGlowMat == null) whiteGlowMat = new Material(urpParticles);
        if (blackYinMat == null)
        {
            blackYinMat = new Material(urpParticles);
            blackYinMat.color = new Color(0.04f, 0.04f, 0.06f, 0.95f);
        }

        // 2. Ground Taiji Disc (Vòng Thái Cực Đen - Trắng chuẩn Đạo Gia)
        groundDiscObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        groundDiscObj.name = "TaijiGroundDisc";
        groundDiscObj.transform.SetParent(transform, false);
        groundDiscObj.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        groundDiscObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        groundDiscObj.transform.localScale = new Vector3(DISC_SIZE, DISC_SIZE, 1f);
        Destroy(groundDiscObj.GetComponent<Collider>());
        var rDisc = groundDiscObj.GetComponent<MeshRenderer>();
        if (rDisc != null) rDisc.sharedMaterial = taijiMat;

        // 3. Luồng Dương (Bạch Quang - Pure Radiant White)
        yangOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yangOrb.name = "Yang_WhiteRadiance";
        yangOrb.transform.SetParent(transform, false);
        yangOrb.transform.localScale = Vector3.one * 0.24f;
        Destroy(yangOrb.GetComponent<Collider>());
        var rYang = yangOrb.GetComponent<MeshRenderer>();
        if (rYang != null) rYang.sharedMaterial = whiteGlowMat;

        TrailRenderer trYang = yangOrb.AddComponent<TrailRenderer>();
        trYang.time = 0.42f;
        trYang.startWidth = 0.28f;
        trYang.endWidth = 0.02f;
        trYang.material = whiteGlowMat;
        trYang.startColor = new Color(1f, 1f, 1f, 0.95f);
        trYang.endColor = new Color(1f, 1f, 1f, 0f);

        // 4. Luồng Âm (Hắc Diệu Thạch - Deep Obsidian Black)
        yinOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yinOrb.name = "Yin_BlackObsidian";
        yinOrb.transform.SetParent(transform, false);
        yinOrb.transform.localScale = Vector3.one * 0.24f;
        Destroy(yinOrb.GetComponent<Collider>());
        var rYin = yinOrb.GetComponent<MeshRenderer>();
        if (rYin != null) rYin.sharedMaterial = blackYinMat;

        TrailRenderer trYin = yinOrb.AddComponent<TrailRenderer>();
        trYin.time = 0.42f;
        trYin.startWidth = 0.28f;
        trYin.endWidth = 0.02f;
        trYin.material = blackYinMat;
        trYin.startColor = new Color(0.04f, 0.04f, 0.06f, 0.92f);
        trYin.endColor = new Color(0.04f, 0.04f, 0.06f, 0f);

        UpdatePositions();
    }

    private void Update()
    {
        currentAngle += orbRotateSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        currentDiscAngle += discRotateSpeed * Time.deltaTime;
        if (currentDiscAngle >= 360f) currentDiscAngle -= 360f;

        if (groundDiscObj != null)
        {
            groundDiscObj.transform.localRotation = Quaternion.Euler(90f, currentDiscAngle, 0f);
        }

        UpdatePositions();
    }

    private void UpdatePositions()
    {
        float radYang = currentAngle * Mathf.Deg2Rad;
        float radYin = currentAngle * Mathf.Deg2Rad + Mathf.PI; // Đối xứng 180 độ cùng chiều xoay

        if (yangOrb != null)
        {
            yangOrb.transform.localPosition = new Vector3(Mathf.Cos(radYang) * RADIUS, 0.16f, Mathf.Sin(radYang) * RADIUS);
        }
        if (yinOrb != null)
        {
            yinOrb.transform.localPosition = new Vector3(Mathf.Cos(radYin) * RADIUS, 0.16f, Mathf.Sin(radYin) * RADIUS);
        }
    }
}

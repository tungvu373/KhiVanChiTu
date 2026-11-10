using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VFX Cửu Trọng Thiên Lôi Kiếp V2 (Khí Vận Chi Tử).
/// Hiệu ứng thiên phạt tu tiên:
/// - Lấy lúc sét chạm làm mốc 0.
/// - t in [-0.12s, 0s]: Điểm chạm lóe nhẹ báo trước (Telegraph).
/// - t in [0s, 0.08s]: Tia chính lóe mạnh (Core trắng sắc, viền tím lạnh lv1-5 / đỏ cam lv6-9, đạo thứ 9 dày và bùng nổ nhất).
/// - t in [0.08s, 0.50s]: Tia lửa bắn thấp quanh chân (Shuriken sparks), vòng xung kích mỏng lan sát đất, dư âm tan trong 0.5s.
/// - Sét nền (isAmbient): Mảnh và tối hơn rõ rệt để người chơi quan sát nhân vật và thanh máu.
/// </summary>
[ExecuteAlways]
public class TribulationLightningEffect : MonoBehaviour
{
    [Header("Line Renderers (Thân Sét)")]
    public LineRenderer coreLine;
    public LineRenderer glowLine;
    public LineRenderer[] branchLines;

    [Header("Impact & Ground Effects")]
    public Transform groundTelegraph;
    public Renderer telegraphRenderer;
    public ParticleSystem sparksParticles;
    public Transform shockwaveRing;
    public Renderer shockwaveRenderer;
    public Light impactLight;

    [Header("Configuration")]
    public int strikeIndex = 1;
    public bool isAmbient = false;
    public Vector3 startPos;
    public Vector3 endPos;

    [Header("Playback / Preview Timing")]
    [Range(-0.12f, 0.60f)]
    public float currentTime = -0.12f;
    public bool isPreviewMode = false;
    public float autoDestroyDelay = 0.65f;

    private MaterialPropertyBlock coreBlock;
    private MaterialPropertyBlock glowBlock;
    private MaterialPropertyBlock telegraphBlock;
    private MaterialPropertyBlock shockwaveBlock;

    private bool hasSparksFired = false;
    private Vector3[] boltPoints;
    private List<Vector3[]> branchPointsList = new List<Vector3[]>();

    private void Awake()
    {
        coreBlock = new MaterialPropertyBlock();
        glowBlock = new MaterialPropertyBlock();
        telegraphBlock = new MaterialPropertyBlock();
        shockwaveBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Khởi tạo và thiết lập thông số cho tia lôi kiếp
    /// </summary>
    public void Setup(Vector3 start, Vector3 end, int strike, bool ambient = false)
    {
        startPos = start;
        endPos = end;
        strikeIndex = strike;
        isAmbient = ambient;
        currentTime = -0.12f;
        hasSparksFired = false;

        GenerateLightningGeometry();
        ApplyStyling();
        UpdateStateAtTime(-0.12f);
    }

    /// <summary>
    /// Tạo tọa độ ngẫu nhiên gãy khúc cho thân sét và các nhánh phụ
    /// </summary>
    public void GenerateLightningGeometry()
    {
        // 1. Thân sét chính: 8 đoạn gãy khúc
        int segments = 8;
        boltPoints = new Vector3[segments + 1];
        boltPoints[0] = startPos;
        boltPoints[segments] = endPos;

        Vector3 dir = (endPos - startPos).normalized;
        Vector3 right = Vector3.Cross(dir, Vector3.forward);
        if (right.sqrMagnitude < 0.001f) right = Vector3.Cross(dir, Vector3.right);
        right.Normalize();
        Vector3 up = Vector3.Cross(dir, right).normalized;

        float jitterScale = isAmbient ? 0.8f : (strikeIndex == 9 ? 1.6f : 1.2f);
        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 basePt = Vector3.Lerp(startPos, endPos, t);
            float envelope = Mathf.Sin(t * Mathf.PI); // Giật mạnh nhất ở giữa thân sét
            float rOff = Random.Range(-1.2f, 1.2f) * jitterScale * envelope;
            float uOff = Random.Range(-1.0f, 1.0f) * jitterScale * envelope;
            boltPoints[i] = basePt + right * rOff + up * uOff;
        }

        if (coreLine != null)
        {
            coreLine.positionCount = boltPoints.Length;
            coreLine.SetPositions(boltPoints);
        }
        if (glowLine != null)
        {
            glowLine.positionCount = boltPoints.Length;
            glowLine.SetPositions(boltPoints);
        }

        // 2. Nhánh phụ gãy ngắn (3 nhánh rẽ nhánh hướng xuống)
        branchPointsList.Clear();
        int[] branchOrigins = new int[] { 2, 4, 6 };
        if (branchLines != null)
        {
            for (int b = 0; b < branchLines.Length; b++)
            {
                if (branchLines[b] == null) continue;
                if (b < branchOrigins.Length && branchOrigins[b] < boltPoints.Length)
                {
                    Vector3 origin = boltPoints[branchOrigins[b]];
                    Vector3 bDir = (Vector3.down * 1.5f + right * (b % 2 == 0 ? 1.2f : -1.2f)).normalized;
                    int bSegs = 4;
                    Vector3[] bPts = new Vector3[bSegs];
                    bPts[0] = origin;
                    Vector3 currentPt = origin;
                    for (int s = 1; s < bSegs; s++)
                    {
                        currentPt += bDir * (strikeIndex == 9 ? 1.8f : 1.3f) + Random.insideUnitSphere * 0.4f;
                        bPts[s] = currentPt;
                    }
                    branchPointsList.Add(bPts);
                    branchLines[b].positionCount = bSegs;
                    branchLines[b].SetPositions(bPts);
                    branchLines[b].gameObject.SetActive(false); // Sẽ bật khi sét chạm
                }
                else
                {
                    branchLines[b].gameObject.SetActive(false);
                }
            }
        }

        // 3. Đặt vị trí Telegraph & Shockwave ngay tại điểm chạm đất
        if (groundTelegraph != null)
        {
            groundTelegraph.position = endPos + Vector3.up * 0.02f;
            groundTelegraph.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        if (shockwaveRing != null)
        {
            shockwaveRing.position = endPos + Vector3.up * 0.03f;
            shockwaveRing.rotation = Quaternion.Euler(90f, 0f, 0f);
            shockwaveRing.localScale = Vector3.zero;
        }
        if (sparksParticles != null)
        {
            sparksParticles.transform.position = endPos + Vector3.up * 0.05f;
        }
        if (impactLight != null)
        {
            impactLight.transform.position = endPos + Vector3.up * 1.2f;
        }
    }

    /// <summary>
    /// Áp dụng màu sắc và độ dày theo từng giai đoạn kiếp số
    /// </summary>
    private void ApplyStyling()
    {
        // Màu sắc: Đạo 1-5 viền tím lạnh; Đạo 6-9 viền đỏ cam
        Color glowColor;
        Color sparkColor;
        Color telegraphColor;

        if (strikeIndex <= 5)
        {
            // Viền tím lạnh
            glowColor = new Color(0.68f, 0.35f, 1.0f, 0.95f);
            sparkColor = new Color(0.78f, 0.55f, 1.0f);
            telegraphColor = new Color(0.72f, 0.40f, 1.0f, 0.85f);
        }
        else
        {
            // Viền đỏ cam bùng cháy
            glowColor = new Color(1.0f, 0.25f, 0.08f, 0.98f);
            sparkColor = new Color(1.0f, 0.50f, 0.15f);
            telegraphColor = new Color(1.0f, 0.30f, 0.10f, 0.9f);
        }

        if (isAmbient)
        {
            glowColor.a *= 0.35f;
            sparkColor *= 0.6f;
            telegraphColor.a *= 0.3f;
        }

        // Độ dày tia sét:
        float coreWidth = isAmbient ? 0.15f : (strikeIndex == 9 ? 0.75f : 0.42f);
        float glowWidth = isAmbient ? 0.45f : (strikeIndex == 9 ? 1.85f : 1.15f);

        if (coreLine != null)
        {
            coreLine.startWidth = coreWidth;
            coreLine.endWidth = coreWidth * 0.4f;
            coreLine.startColor = Color.white;
            coreLine.endColor = new Color(1f, 1f, 1f, 0.8f);
        }

        if (glowLine != null)
        {
            glowLine.startWidth = glowWidth;
            glowLine.endWidth = glowWidth * 0.5f;
            glowLine.startColor = glowColor;
            glowLine.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
        }

        if (branchLines != null)
        {
            for (int b = 0; b < branchLines.Length; b++)
            {
                if (branchLines[b] != null)
                {
                    branchLines[b].startWidth = coreWidth * 0.5f;
                    branchLines[b].endWidth = 0.02f;
                    branchLines[b].startColor = glowColor;
                    branchLines[b].endColor = Color.white;
                }
            }
        }

        if (impactLight != null)
        {
            impactLight.color = glowColor;
            impactLight.range = strikeIndex == 9 ? 22f : 15f;
        }

        if (sparksParticles != null)
        {
            var main = sparksParticles.main;
            main.startColor = sparkColor;
            var emission = sparksParticles.emission;
            int burstCount = isAmbient ? 8 : (strikeIndex == 9 ? 55 : 30);
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });
        }
    }

    private void Update()
    {
        if (isPreviewMode) return;

        currentTime += Time.deltaTime;
        UpdateStateAtTime(currentTime);

        if (currentTime >= autoDestroyDelay)
        {
            if (Application.isPlaying) Destroy(gameObject);
        }
    }

    /// <summary>
    /// Điều khiển toàn bộ hoạt ảnh của VFX dựa trên mốc thời gian (sét chạm = 0s)
    /// Hỗ trợ cả playback thời gian thực lẫn preview thanh trượt trong Editor.
    /// </summary>
    public void UpdateStateAtTime(float t)
    {
        currentTime = t;

        // =========================================================================
        // 1. GIAI ĐOẠN 1: BÁO TRƯỚC (Telegraph: t in [-0.12s, 0.00s))
        // =========================================================================
        if (t < 0f)
        {
            // Thân sét và xung kích CHƯA xuất hiện
            SetLinesActive(false);
            if (shockwaveRing != null) shockwaveRing.gameObject.SetActive(false);

            // Điểm chạm lóe nhẹ khoảng 0.12s trước đó
            if (groundTelegraph != null)
            {
                groundTelegraph.gameObject.SetActive(!isAmbient);
                float progress = Mathf.Clamp01((t + 0.12f) / 0.12f); // 0 -> 1
                float scale = Mathf.Lerp(0.8f, strikeIndex == 9 ? 2.6f : 1.8f, progress);
                groundTelegraph.localScale = new Vector3(scale, scale, scale);

                if (telegraphRenderer != null)
                {
                    Color baseCol = (strikeIndex <= 5) ? new Color(0.72f, 0.40f, 1.0f) : new Color(1.0f, 0.35f, 0.10f);
                    float alpha = Mathf.Sin(progress * Mathf.PI * 0.5f) * 0.85f;
                    SetRendererColor(telegraphRenderer, telegraphBlock, new Color(baseCol.r, baseCol.g, baseCol.b, alpha));
                }
            }

            if (impactLight != null)
            {
                impactLight.enabled = true;
                float progress = Mathf.Clamp01((t + 0.12f) / 0.12f);
                impactLight.intensity = Mathf.Lerp(0.2f, 1.5f, progress);
            }
            return;
        }

        // =========================================================================
        // 2. GIAI ĐOẠN 2: SÉT GIÁNG & LÓE MẠNH (Impact: t in [0.00s, 0.08s])
        // =========================================================================
        if (t <= 0.08f)
        {
            // Ẩn vòng báo trước
            if (groundTelegraph != null) groundTelegraph.gameObject.SetActive(false);

            // Thân sét chính và nhánh lóe cực mạnh
            SetLinesActive(true);
            float impactRatio = t / 0.08f;
            float flashAlpha = 1.0f; // Duy trì độ sáng cực đại 0.08s

            Color glowCol = (strikeIndex <= 5) ? new Color(0.68f, 0.35f, 1.0f) : new Color(1.0f, 0.28f, 0.08f);
            if (glowLine != null)
            {
                Color c = glowCol;
                c.a = flashAlpha * (isAmbient ? 0.4f : 0.98f);
                glowLine.startColor = c;
                glowLine.endColor = c * 0.5f;
            }
            if (coreLine != null)
            {
                coreLine.startColor = Color.white;
                coreLine.endColor = new Color(1f, 1f, 1f, flashAlpha);
            }

            // Kích hoạt Shuriken tia lửa bắn ra quanh chân đúng mốc t = 0
            if (!hasSparksFired && sparksParticles != null)
            {
                hasSparksFired = true;
                if (!isPreviewMode && Application.isPlaying)
                {
                    sparksParticles.Play();
                }
            }

            if (impactLight != null)
            {
                impactLight.enabled = true;
                impactLight.intensity = (strikeIndex == 9 ? 12f : 8f) * (isAmbient ? 0.3f : 1f);
            }

            // Vòng xung kích bắt đầu xuất hiện và mở rộng nhẹ
            if (shockwaveRing != null)
            {
                shockwaveRing.gameObject.SetActive(!isAmbient);
                float maxR = strikeIndex == 9 ? 6.2f : 4.0f;
                float currentScale = Mathf.Lerp(0.2f, maxR * 0.25f, impactRatio);
                shockwaveRing.localScale = new Vector3(currentScale, currentScale, currentScale);

                if (shockwaveRenderer != null)
                {
                    Color swCol = (strikeIndex <= 5) ? new Color(0.70f, 0.40f, 1.0f, 0.9f) : new Color(1.0f, 0.30f, 0.10f, 0.9f);
                    SetRendererColor(shockwaveRenderer, shockwaveBlock, swCol);
                }
            }
            return;
        }

        // =========================================================================
        // 3. GIAI ĐOẠN 3: DƯ ÂM TAN DẦN (Aftermath: t in (0.08s, 0.50s])
        // =========================================================================
        float aftermathElapsed = t - 0.08f;
        float aftermathDuration = 0.42f; // 0.08s -> 0.50s (tổng 0.5s từ mốc 0)
        float normalizedAftermath = Mathf.Clamp01(aftermathElapsed / aftermathDuration);

        // a. Thân sét tan rất nhanh (từ 0.08s đến 0.18s là tắt hẳn)
        if (t <= 0.18f)
        {
            float boltFade = 1f - Mathf.Clamp01((t - 0.08f) / 0.10f);
            Color glowCol = (strikeIndex <= 5) ? new Color(0.68f, 0.35f, 1.0f) : new Color(1.0f, 0.28f, 0.08f);
            if (glowLine != null)
            {
                glowCol.a = boltFade * (isAmbient ? 0.4f : 0.98f);
                glowLine.startColor = glowCol;
            }
            if (coreLine != null)
            {
                coreLine.startColor = new Color(1f, 1f, 1f, boltFade);
            }
        }
        else
        {
            SetLinesActive(false);
        }

        // b. Vòng xung kích mỏng lan rộng sát đất và tan biến
        if (shockwaveRing != null)
        {
            shockwaveRing.gameObject.SetActive(!isAmbient);
            float maxR = strikeIndex == 9 ? 6.2f : 4.0f;
            float currentScale = Mathf.Lerp(maxR * 0.25f, maxR, Mathf.Sqrt(normalizedAftermath));
            shockwaveRing.localScale = new Vector3(currentScale, currentScale, currentScale);

            if (shockwaveRenderer != null)
            {
                Color swCol = (strikeIndex <= 5) ? new Color(0.70f, 0.40f, 1.0f) : new Color(1.0f, 0.30f, 0.10f);
                float alpha = Mathf.Pow(1f - normalizedAftermath, 1.6f) * 0.9f;
                SetRendererColor(shockwaveRenderer, shockwaveBlock, new Color(swCol.r, swCol.g, swCol.b, alpha));
            }
        }

        // c. Tia lửa (Sparks) trong chế độ preview được giả lập bằng Simulate
        if (isPreviewMode && sparksParticles != null)
        {
            sparksParticles.Simulate(t, true, true);
        }

        // d. Ánh sáng mờ dần
        if (impactLight != null)
        {
            impactLight.intensity = Mathf.Lerp(strikeIndex == 9 ? 12f : 8f, 0f, normalizedAftermath);
            if (normalizedAftermath >= 1f) impactLight.enabled = false;
        }

        // e. Hoàn toàn tan biến ở mốc 0.5s
        if (t >= 0.50f)
        {
            if (shockwaveRing != null) shockwaveRing.gameObject.SetActive(false);
            if (impactLight != null) impactLight.enabled = false;
        }
    }

    private void SetLinesActive(bool active)
    {
        if (coreLine != null) coreLine.gameObject.SetActive(active);
        if (glowLine != null) glowLine.gameObject.SetActive(active);
        if (branchLines != null)
        {
            for (int b = 0; b < branchLines.Length; b++)
            {
                if (branchLines[b] != null) branchLines[b].gameObject.SetActive(active && !isAmbient);
            }
        }
    }

    private void SetRendererColor(Renderer r, MaterialPropertyBlock block, Color color)
    {
        if (r == null) return;
        if (block == null) block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        r.SetPropertyBlock(block);
    }
}


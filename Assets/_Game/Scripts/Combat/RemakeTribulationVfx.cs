using UnityEngine;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Zero-GC, WebGL/iPhone-friendly Heavenly Tribulation VFX controller attached to VFX_HeavenlyTribulation.prefab.
    /// Unleashes ferocious 3-wave cascading jagged lightning pillars, forked branches, spidering ground arcs,
    /// ambient inter-strike static arcs, Shuriken storm vortex, double shockwaves, and ascension climax bursts.
    /// Zero runtime Instantiate/Destroy and zero dynamic PointLights.
    /// </summary>
    public sealed class RemakeTribulationVfx : MonoBehaviour
    {
        public const int PrimarySegments = 10;
        public const int BranchSegments = 6;
        public const int GroundArcSegments = 6;

        [Header("Primary Jagged Lightning Pillars (Outer Glow + Hot White Core)")]
        [SerializeField] private LineRenderer[] outerPillars;
        [SerializeField] private LineRenderer[] corePillars;

        [Header("Forked Air Branches & Ground Crackle Arcs")]
        [SerializeField] private LineRenderer[] forkBranches;
        [SerializeField] private LineRenderer[] groundArcs;

        [Header("Shuriken Particle Layers (Zero-Shadow URP Unlit)")]
        [SerializeField] private ParticleSystem stormVortexPs;
        [SerializeField] private ParticleSystem gatheringQiPs;
        [SerializeField] private ParticleSystem impactSparksPs;
        [SerializeField] private ParticleSystem shockwaveRingPs;
        [SerializeField] private ParticleSystem impactFlashPs;
        [SerializeField] private ParticleSystem climaxBurstPs;

        // Pre-allocated point scratch arrays -> ZERO GC allocations per strike/frame!
        private readonly Vector3[] _pillarPts = new Vector3[PrimarySegments];
        private readonly Vector3[] _branchPts = new Vector3[BranchSegments];
        private readonly Vector3[] _arcPts = new Vector3[GroundArcSegments];

        private float _strikeTimer;
        private float _strikeDuration = 1.05f;
        private float _rejitterTimer;
        private int _cascadesRemaining;
        private float _nextCascadeTimer;
        private float _ambientArcTimer;
        private float _ambientArcFade;

        private int _activePillars;
        private int _activeBranches;
        private int _activeArcs;
        private Vector3 _strikeTargetPos;
        private float _strikeIntensity;
        private Color _outerColor = new Color(0.22f, 0.86f, 1f, 1f);
        private Color _coreColor = Color.white;
        private bool _seclusionActive;

        public bool IsSeclusionActive => _seclusionActive;
        public bool IsStrikeVisualActive => _strikeTimer > 0f;

        public void ConfigureReferences(
            LineRenderer[] outers,
            LineRenderer[] cores,
            LineRenderer[] branches,
            LineRenderer[] arcs,
            ParticleSystem stormPs,
            ParticleSystem qiPs,
            ParticleSystem sparksPs,
            ParticleSystem ringPs,
            ParticleSystem flashPs,
            ParticleSystem climaxPs)
        {
            outerPillars = outers;
            corePillars = cores;
            forkBranches = branches;
            groundArcs = arcs;
            stormVortexPs = stormPs;
            gatheringQiPs = qiPs;
            impactSparksPs = sparksPs;
            shockwaveRingPs = ringPs;
            impactFlashPs = flashPs;
            climaxBurstPs = climaxPs;
        }

        private void Awake()
        {
            DisableAllBolts();
            StopAllLoopSystems(true);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_strikeTimer > 0f)
            {
                _strikeTimer -= dt;
                if (_strikeTimer <= 0f)
                {
                    _strikeTimer = 0f;
                    _cascadesRemaining = 0;
                    DisableAllBolts();
                }
                else
                {
                    float norm = Mathf.Clamp01(_strikeTimer / Mathf.Max(0.05f, _strikeDuration));

                    // Secondary & tertiary thunderclap cascades inside the same strike!
                    if (_cascadesRemaining > 0)
                    {
                        _nextCascadeTimer -= dt;
                        if (_nextCascadeTimer <= 0f)
                        {
                            _cascadesRemaining--;
                            _nextCascadeTimer = 0.28f;
                            FireCascadeAftershock();
                        }
                    }

                    // Rapid crackling snap re-jitter every 55ms -> authentic violent lightning flicker!
                    _rejitterTimer -= dt;
                    if (_rejitterTimer <= 0f && norm > 0.14f)
                    {
                        _rejitterTimer = 0.055f;
                        RebuildActiveBoltGeometry(norm);
                    }

                    float flicker = norm > 0.55f ? 1f : (0.58f + 0.42f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 88f)));
                    float widthScale = Mathf.Lerp(0.28f, 1.0f, norm) * (0.86f + 0.14f * flicker);
                    float alpha = Mathf.Clamp01(norm * 1.55f) * flicker;
                    ApplyBoltVisualStyle(widthScale, alpha);
                }
                return;
            }

            // Continuous ambient storm crackle arcs between major strikes so the 15s Tribulation NEVER feels empty!
            if (_seclusionActive)
            {
                if (_ambientArcFade > 0f)
                {
                    _ambientArcFade -= dt;
                    if (_ambientArcFade <= 0f)
                        DisableArray(groundArcs);
                }

                _ambientArcTimer -= dt;
                if (_ambientArcTimer <= 0f)
                {
                    _ambientArcTimer = Random.Range(0.18f, 0.30f);
                    FireAmbientStaticCrackles();
                }
            }
        }

        public void SetSeclusionActive(bool active, Vector3 centerPos)
        {
            _seclusionActive = active;
            _strikeTargetPos = centerPos;
            transform.position = centerPos;

            if (active)
            {
                _ambientArcTimer = 0.12f;
                if (stormVortexPs != null)
                {
                    // Framed cleanly in upper portrait gameplay camera
                    stormVortexPs.transform.position = centerPos + new Vector3(0f, 3.8f, 2.5f);
                    stormVortexPs.Play(true);
                }
                if (gatheringQiPs != null)
                {
                    gatheringQiPs.transform.position = centerPos + Vector3.up * 0.15f;
                    gatheringQiPs.Play(true);
                }
            }
            else
            {
                _ambientArcFade = 0f;
                DisableArray(groundArcs);
                StopAllLoopSystems(false);
            }
        }

        public void PlayStrike(Vector3 playerPos, int strikeIndex, int totalStrikes)
        {
            transform.position = playerPos;
            _strikeTargetPos = playerPos;
            _strikeIntensity = Mathf.Clamp01((float)strikeIndex / Mathf.Max(1, totalStrikes));

            bool minimal = GraphicsSettingsService.Instance != null &&
                           GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal;

            // Color escalation: Strikes 1-3 Thanh Loi (Cyan) -> 4-6 Tu Tieu (Violet) -> 7-9 Cuu Tieu Kim Loi (Gold-Crimson)
            if (strikeIndex <= 3)
            {
                _outerColor = new Color(0.18f, 0.86f, 1.00f, 1f);
                _coreColor = new Color(0.95f, 1.00f, 1.00f, 1f);
            }
            else if (strikeIndex <= 6)
            {
                _outerColor = new Color(0.74f, 0.24f, 1.00f, 1f);
                _coreColor = new Color(1.00f, 0.94f, 1.00f, 1f);
            }
            else
            {
                _outerColor = strikeIndex >= totalStrikes
                    ? new Color(1.00f, 0.42f, 0.10f, 1f)
                    : new Color(1.00f, 0.80f, 0.16f, 1f);
                _coreColor = new Color(1.00f, 1.00f, 0.90f, 1f);
            }

            int maxPillars = outerPillars != null ? outerPillars.Length : 0;
            int maxBranches = forkBranches != null ? forkBranches.Length : 0;
            int maxArcs = groundArcs != null ? groundArcs.Length : 0;

            _activePillars = Mathf.Min(maxPillars, strikeIndex >= 7 ? 3 : strikeIndex >= 4 ? 2 : 2);
            if (minimal) _activePillars = Mathf.Min(_activePillars, 2);

            _activeBranches = Mathf.Min(maxBranches, minimal ? 3 : Mathf.RoundToInt(Mathf.Lerp(3f, maxBranches, _strikeIntensity)));
            _activeArcs = Mathf.Min(maxArcs, minimal ? 3 : Mathf.RoundToInt(Mathf.Lerp(3f, maxArcs, _strikeIntensity)));

            _strikeDuration = Mathf.Lerp(0.92f, 1.15f, _strikeIntensity);
            _strikeTimer = _strikeDuration;
            _rejitterTimer = 0.055f;
            _cascadesRemaining = minimal ? 1 : 2;
            _nextCascadeTimer = 0.26f;

            DisableAllBolts();
            RebuildActiveBoltGeometry(1f);
            ApplyBoltVisualStyle(1f, 1f);
            EmitImpactParticles(1.0f);
        }

        public void SimulateImpactForPreview(float stepSeconds)
        {
            float t = Mathf.Clamp(stepSeconds, 0.02f, 0.35f);
            if (stormVortexPs != null) stormVortexPs.Simulate(1.5f, true, false);
            if (gatheringQiPs != null) gatheringQiPs.Simulate(0.6f, true, false);
            if (impactFlashPs != null) impactFlashPs.Simulate(t * 0.5f, true, false);
            if (shockwaveRingPs != null) shockwaveRingPs.Simulate(t, true, false);
            if (impactSparksPs != null) impactSparksPs.Simulate(t, true, false);
        }

        public void PlayOutcomeBurst(Vector3 playerPos, bool success)
        {
            transform.position = playerPos;
            Color cA = success ? new Color(1.00f, 0.88f, 0.28f, 1f) : new Color(0.95f, 0.22f, 0.16f, 1f);
            Color cB = success ? new Color(0.42f, 0.98f, 0.82f, 1f) : new Color(0.55f, 0.10f, 0.14f, 1f);

            if (climaxBurstPs != null)
            {
                var main = climaxBurstPs.main;
                main.startColor = new ParticleSystem.MinMaxGradient(cA, cB);
                climaxBurstPs.transform.position = playerPos + Vector3.up * 1.0f;
                climaxBurstPs.Emit(success ? 38 : 28);
            }
            if (shockwaveRingPs != null)
            {
                var main = shockwaveRingPs.main;
                main.startColor = cA;
                main.startSize = 1.25f;
                shockwaveRingPs.transform.position = playerPos + Vector3.up * 0.18f;
                shockwaveRingPs.Emit(3);
            }
            if (impactFlashPs != null)
            {
                var main = impactFlashPs.main;
                main.startColor = cA;
                main.startSize = 6.0f;
                impactFlashPs.transform.position = playerPos + Vector3.up * 1.1f;
                impactFlashPs.Emit(2);
            }
        }

        public void ClearImmediate()
        {
            _strikeTimer = 0f;
            _cascadesRemaining = 0;
            _ambientArcFade = 0f;
            _seclusionActive = false;
            DisableAllBolts();
            StopAllLoopSystems(true);
            if (impactSparksPs != null) impactSparksPs.Clear(true);
            if (shockwaveRingPs != null) shockwaveRingPs.Clear(true);
            if (impactFlashPs != null) impactFlashPs.Clear(true);
            if (climaxBurstPs != null) climaxBurstPs.Clear(true);
        }

        private void FireCascadeAftershock()
        {
            RebuildActiveBoltGeometry(1.0f);
            ApplyBoltVisualStyle(0.95f, 1.0f);
            EmitImpactParticles(0.65f);
            MeadowEnvironmentController.Instance?.TriggerTribulationSkyFlash(_strikeIntensity * 0.72f, _outerColor);
            MeadowEnvironmentController.Instance?.AddCameraTrauma(0.28f + _strikeIntensity * 0.25f);
        }

        private void FireAmbientStaticCrackles()
        {
            if (groundArcs == null || groundArcs.Length == 0) return;
            _ambientArcFade = 0.14f;
            int cnt = Mathf.Min(2, groundArcs.Length);
            for (int a = 0; a < cnt; a++)
            {
                var lr = groundArcs[a];
                if (lr == null) continue;
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float r0 = Random.Range(0.4f, 1.3f);
                float r1 = Random.Range(2.2f, 3.6f);
                Vector3 p0 = _strikeTargetPos + new Vector3(Mathf.Cos(ang) * r0, 0.22f, Mathf.Sin(ang) * r0);
                float ang2 = ang + Random.Range(-0.9f, 0.9f);
                Vector3 p1 = _strikeTargetPos + new Vector3(Mathf.Cos(ang2) * r1, 0.18f, Mathf.Sin(ang2) * r1);
                _arcPts[0] = p0;
                for (int i = 1; i < GroundArcSegments - 1; i++)
                {
                    float t = (float)i / (GroundArcSegments - 1);
                    Vector3 pt = Vector3.Lerp(p0, p1, t);
                    pt.x += Random.Range(-0.35f, 0.35f);
                    pt.z += Random.Range(-0.35f, 0.35f);
                    pt.y = 0.18f + Random.Range(0f, 0.4f) * Mathf.Sin(t * Mathf.PI);
                    _arcPts[i] = pt;
                }
                _arcPts[GroundArcSegments - 1] = p1;
                lr.positionCount = GroundArcSegments;
                lr.SetPositions(_arcPts);
                lr.startWidth = 0.24f;
                lr.endWidth = 0.06f;
                lr.startColor = _coreColor;
                lr.endColor = _outerColor;
                lr.enabled = true;
            }
        }

        private void EmitImpactParticles(float powerMult)
        {
            bool minimal = GraphicsSettingsService.Instance != null &&
                           GraphicsSettingsService.Instance.Settings.vfxDensity == RemakeVfxDensity.Minimal;
            float emitScale = (GraphicsSettingsService.Instance != null ? GraphicsSettingsService.Instance.VfxEmitScale : 1f) * powerMult;

            if (impactFlashPs != null)
            {
                var main = impactFlashPs.main;
                main.startColor = _outerColor;
                main.startSize = Mathf.Lerp(3.8f, 6.2f, _strikeIntensity) * Mathf.Lerp(0.75f, 1f, powerMult);
                impactFlashPs.transform.position = _strikeTargetPos + Vector3.up * 0.9f;
                impactFlashPs.Emit(minimal ? 1 : 2);
            }

            if (shockwaveRingPs != null)
            {
                var main = shockwaveRingPs.main;
                main.startColor = _outerColor;
                main.startSize = Mathf.Lerp(0.85f, 1.25f, _strikeIntensity);
                shockwaveRingPs.transform.position = _strikeTargetPos + Vector3.up * 0.16f;
                shockwaveRingPs.Emit(powerMult >= 0.9f ? 2 : 1);
            }

            if (impactSparksPs != null)
            {
                var main = impactSparksPs.main;
                main.startColor = new ParticleSystem.MinMaxGradient(_coreColor, _outerColor);
                impactSparksPs.transform.position = _strikeTargetPos + Vector3.up * 0.65f;
                int sparkCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(22f, 40f, _strikeIntensity) * emitScale), 8, 42);
                impactSparksPs.Emit(sparkCount);
            }
        }

        private void RebuildActiveBoltGeometry(float energy)
        {
            float jaggedAmp = Mathf.Lerp(0.85f, 1.55f, _strikeIntensity) * (0.78f + 0.22f * energy);
            Vector3 mainMidAnchor = _strikeTargetPos + Vector3.up * 5.2f;

            for (int p = 0; p < _activePillars; p++)
            {
                Vector3 hitOffset = p == 0
                    ? Vector3.zero
                    : new Vector3(Random.Range(-1.45f, 1.45f), 0f, Random.Range(-1.35f, 1.35f));
                Vector3 dst = _strikeTargetPos + hitOffset + Vector3.up * 0.12f;
                // Angled sharply down from upper storm cloud so full zig-zag bolt fills portrait camera!
                Vector3 src = dst + new Vector3(Random.Range(-2.4f, 2.4f), 13.5f, Random.Range(0.4f, 2.8f));

                _pillarPts[0] = src;
                for (int i = 1; i < PrimarySegments - 1; i++)
                {
                    float t = (float)i / (PrimarySegments - 1);
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    Vector3 basePt = Vector3.Lerp(src, dst, t);
                    basePt.x += Random.Range(-jaggedAmp, jaggedAmp) * envelope;
                    basePt.z += Random.Range(-jaggedAmp * 0.85f, jaggedAmp * 0.85f) * envelope;
                    _pillarPts[i] = basePt;
                }
                _pillarPts[PrimarySegments - 1] = dst;
                if (p == 0) mainMidAnchor = _pillarPts[4];

                if (outerPillars != null && p < outerPillars.Length && outerPillars[p] != null)
                {
                    outerPillars[p].positionCount = PrimarySegments;
                    outerPillars[p].SetPositions(_pillarPts);
                    outerPillars[p].enabled = true;
                }
                if (corePillars != null && p < corePillars.Length && corePillars[p] != null)
                {
                    corePillars[p].positionCount = PrimarySegments;
                    corePillars[p].SetPositions(_pillarPts);
                    corePillars[p].enabled = true;
                }
            }

            // Violent forked sky branches splitting outward
            for (int b = 0; b < _activeBranches; b++)
            {
                if (forkBranches == null || b >= forkBranches.Length || forkBranches[b] == null) break;
                Vector3 bStart = Vector3.Lerp(_strikeTargetPos + Vector3.up * 9.5f, mainMidAnchor, Random.Range(0.25f, 0.85f));
                bStart.x = _strikeTargetPos.x + Random.Range(-0.85f, 0.85f);
                bStart.z = _strikeTargetPos.z + Random.Range(-0.5f, 1.2f);

                float angle = (b * (360f / Mathf.Max(1, _activeBranches)) + Random.Range(-30f, 30f)) * Mathf.Deg2Rad;
                float reach = Random.Range(2.8f, 4.9f);
                Vector3 bEnd = bStart + new Vector3(Mathf.Cos(angle) * reach, -Random.Range(2.4f, 4.6f), Mathf.Sin(angle) * reach);
                bEnd.y = Mathf.Max(0.18f, bEnd.y);

                _branchPts[0] = bStart;
                for (int i = 1; i < BranchSegments - 1; i++)
                {
                    float t = (float)i / (BranchSegments - 1);
                    Vector3 pt = Vector3.Lerp(bStart, bEnd, t);
                    pt.x += Random.Range(-0.62f, 0.62f);
                    pt.z += Random.Range(-0.62f, 0.62f);
                    _branchPts[i] = pt;
                }
                _branchPts[BranchSegments - 1] = bEnd;

                forkBranches[b].positionCount = BranchSegments;
                forkBranches[b].SetPositions(_branchPts);
                forkBranches[b].enabled = true;
            }

            // Radial ground spider arcs whipping across the meadow around the Cultivator
            for (int a = 0; a < _activeArcs; a++)
            {
                if (groundArcs == null || a >= groundArcs.Length || groundArcs[a] == null) break;
                float ang = (a * (360f / Mathf.Max(1, _activeArcs)) + Random.Range(-22f, 22f)) * Mathf.Deg2Rad;
                float dist = Mathf.Lerp(2.8f, 4.8f, _strikeIntensity) * Random.Range(0.85f, 1.15f);
                Vector3 aStart = _strikeTargetPos + Vector3.up * 0.25f;
                Vector3 aEnd = _strikeTargetPos + new Vector3(Mathf.Cos(ang) * dist, 0.14f, Mathf.Sin(ang) * dist);

                _arcPts[0] = aStart;
                for (int i = 1; i < GroundArcSegments - 1; i++)
                {
                    float t = (float)i / (GroundArcSegments - 1);
                    Vector3 pt = Vector3.Lerp(aStart, aEnd, t);
                    pt.x += Random.Range(-0.48f, 0.48f);
                    pt.z += Random.Range(-0.48f, 0.48f);
                    pt.y = 0.18f + Random.Range(0f, 0.45f) * Mathf.Sin(t * Mathf.PI);
                    _arcPts[i] = pt;
                }
                _arcPts[GroundArcSegments - 1] = aEnd;

                groundArcs[a].positionCount = GroundArcSegments;
                groundArcs[a].SetPositions(_arcPts);
                groundArcs[a].enabled = true;
            }
        }

        private void ApplyBoltVisualStyle(float widthScale, float alpha)
        {
            float baseOuterW = Mathf.Lerp(1.10f, 1.95f, _strikeIntensity) * widthScale;
            float baseCoreW = baseOuterW * 0.36f;

            Color outA = new Color(_outerColor.r, _outerColor.g, _outerColor.b, alpha);
            Color outB = new Color(_outerColor.r * 0.85f, _outerColor.g * 0.85f, _outerColor.b, alpha * 0.94f);
            Color coreA = new Color(_coreColor.r, _coreColor.g, _coreColor.b, alpha);

            if (outerPillars != null)
            {
                for (int i = 0; i < _activePillars && i < outerPillars.Length; i++)
                {
                    var lr = outerPillars[i];
                    if (lr == null) continue;
                    float scale = i == 0 ? 1.0f : 0.72f;
                    lr.startWidth = baseOuterW * scale;
                    lr.endWidth = baseOuterW * 0.62f * scale;
                    lr.startColor = outA;
                    lr.endColor = outB;
                }
            }
            if (corePillars != null)
            {
                for (int i = 0; i < _activePillars && i < corePillars.Length; i++)
                {
                    var lr = corePillars[i];
                    if (lr == null) continue;
                    float scale = i == 0 ? 1.0f : 0.72f;
                    lr.startWidth = baseCoreW * scale;
                    lr.endWidth = baseCoreW * 0.56f * scale;
                    lr.startColor = coreA;
                    lr.endColor = coreA;
                }
            }
            if (forkBranches != null)
            {
                for (int i = 0; i < _activeBranches && i < forkBranches.Length; i++)
                {
                    var lr = forkBranches[i];
                    if (lr == null) continue;
                    lr.startWidth = baseOuterW * 0.34f;
                    lr.endWidth = 0.05f;
                    lr.startColor = coreA;
                    lr.endColor = new Color(_outerColor.r, _outerColor.g, _outerColor.b, alpha * 0.65f);
                }
            }
            if (groundArcs != null)
            {
                for (int i = 0; i < _activeArcs && i < groundArcs.Length; i++)
                {
                    var lr = groundArcs[i];
                    if (lr == null) continue;
                    lr.startWidth = baseOuterW * 0.32f;
                    lr.endWidth = 0.06f;
                    lr.startColor = coreA;
                    lr.endColor = new Color(_outerColor.r, _outerColor.g, _outerColor.b, alpha * 0.75f);
                }
            }
        }

        private void DisableAllBolts()
        {
            DisableArray(outerPillars);
            DisableArray(corePillars);
            DisableArray(forkBranches);
            DisableArray(groundArcs);
        }

        private static void DisableArray(LineRenderer[] arr)
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != null) arr[i].enabled = false;
        }

        private void StopAllLoopSystems(bool clear)
        {
            var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting;
            if (stormVortexPs != null) stormVortexPs.Stop(true, mode);
            if (gatheringQiPs != null) gatheringQiPs.Stop(true, mode);
        }
    }
}

using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Bo dem FPS khong cap phat rac bo nho theo tung frame (cap nhat 4 lan/giay).
    /// </summary>
    public sealed class FpsMonitorService : MonoBehaviour
    {
        public static FpsMonitorService Instance { get; private set; }

        public float CurrentFps { get; private set; } = 60f;
        public float FrameTimeMs { get; private set; } = 16.6f;
        public int StatusTier { get; private set; } // 0 = Tot, 1 = Trung binh, 2 = Thap
        public string DisplayText { get; private set; } = "60 FPS · 16.6ms";

        public event Action OnFpsUpdated;

        private float _accumTime;
        private int _accumFrames;
        private const float SampleWindowSec = 0.25f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            _accumTime += Time.unscaledDeltaTime;
            _accumFrames++;

            if (_accumTime < SampleWindowSec) return;

            float avgDt = _accumTime / Mathf.Max(1, _accumFrames);
            CurrentFps = avgDt > 0.0001f ? 1f / avgDt : 60f;
            FrameTimeMs = avgDt * 1000f;
            _accumTime = 0f;
            _accumFrames = 0;

            bool afkMode = GraphicsSettingsService.Instance != null && GraphicsSettingsService.Instance.Settings.afkDimMode;
            int roundedFps = Mathf.RoundToInt(CurrentFps);
            if (afkMode)
            {
                StatusTier = 0;
                DisplayText = $"{roundedFps} FPS · Tĩnh Tọa";
            }
            else
            {
                StatusTier = roundedFps >= 48 ? 0 : roundedFps >= 26 ? 1 : 2;
                DisplayText = $"{roundedFps} FPS · {FrameTimeMs:0.0}ms";
            }

            OnFpsUpdated?.Invoke();
        }
    }
}

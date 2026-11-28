using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KhiVanChiTu.Remake
{
    public sealed class GraphicsSettingsService : MonoBehaviour
    {
        public static GraphicsSettingsService Instance { get; private set; }

        public event Action OnGraphicsChanged;

        private static readonly float[] RenderScaleValues = { 0.70f, 0.85f, 1.00f };

        // Snapshot URP defaults so Editor assets are restored cleanly on Play Mode stop
        private UniversalRenderPipelineAsset _capturedUrp;
        private float _origRenderScale = 1f;
        private int _origShadowRes = 1024;
        private int _origCascadeCount = 2;
        private float _origShadowDist = 40f;
        private int _origMsaa = 1;
        private bool _hasCapturedUrp;

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
            CaptureUrpSnapshot();
            ApplyMobileWebSmartDefaultIfNeeded();
            ApplyFromSave();
        }

        private void ApplyMobileWebSmartDefaultIfNeeded()
        {
            var save = SaveService.Current;
            if (save == null || save.hasCreatedCharacter || !PlatformWebBridge.IsTouchPlatform()) return;
            // First-time iPhone Safari / Android visitor: shadow-free mobile balanced preset (zero shadow pass & bloom RT)
            var g = save.graphics;
            if (g.preset == RemakeGraphicsPreset.Medium && g.shadowMode == RemakeShadowMode.Low)
            {
                g.preset = RemakeGraphicsPreset.Low;
                g.renderScaleIndex = 1; // 85% * 1.5 DPR = crisp & cool battery
                g.shadowMode = RemakeShadowMode.Off;
                g.vfxDensity = RemakeVfxDensity.Full;
                g.targetFps = 60;
            }
        }

        private void OnDestroy()
        {
            RestoreUrpSnapshot();
            if (Instance == this) Instance = null;
        }

        public RemakeGraphicsSettingsData Settings => SaveService.Current.graphics;
        public bool UseFakeBlobShadows => Settings.shadowMode == RemakeShadowMode.Off;
        public float VfxEmitScale => Settings.vfxDensity == RemakeVfxDensity.Minimal ? 0.40f : 1.00f;
        public float CurrentRenderScale => RenderScaleValues[Mathf.Clamp(Settings.renderScaleIndex, 0, 2)];

        public void ApplyPreset(RemakeGraphicsPreset preset)
        {
            var g = Settings;
            g.preset = preset;
            switch (preset)
            {
                case RemakeGraphicsPreset.Low:
                    g.renderScaleIndex = 0;
                    g.shadowMode = RemakeShadowMode.Off;
                    g.vfxDensity = RemakeVfxDensity.Minimal;
                    g.targetFps = 30;
                    break;
                case RemakeGraphicsPreset.Medium:
                    g.renderScaleIndex = 1;
                    g.shadowMode = RemakeShadowMode.Low;
                    g.vfxDensity = RemakeVfxDensity.Full;
                    g.targetFps = 60;
                    break;
                case RemakeGraphicsPreset.High:
                    g.renderScaleIndex = 2;
                    g.shadowMode = RemakeShadowMode.High;
                    g.vfxDensity = RemakeVfxDensity.Full;
                    g.targetFps = 60;
                    break;
            }
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetRenderScaleIndex(int idx)
        {
            Settings.renderScaleIndex = Mathf.Clamp(idx, 0, 2);
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetShadowMode(RemakeShadowMode mode)
        {
            Settings.shadowMode = mode;
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetVfxDensity(RemakeVfxDensity density)
        {
            Settings.vfxDensity = density;
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetTargetFps(int fps)
        {
            Settings.targetFps = fps <= 35 ? 30 : 60;
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetShowFpsCounter(bool visible)
        {
            Settings.showFpsCounter = visible;
            SaveService.Save();
            OnGraphicsChanged?.Invoke();
        }

        public void ToggleAfkDimMode()
        {
            Settings.afkDimMode = !Settings.afkDimMode;
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetAfkDimMode(bool enabled)
        {
            if (Settings.afkDimMode == enabled) return;
            Settings.afkDimMode = enabled;
            SaveService.Save();
            ApplyFromSave();
        }

        public void SetAudioEnabled(bool sound, bool music)
        {
            Settings.soundEnabled = sound;
            Settings.musicEnabled = music;
            SaveService.Save();
            RemakeAudioService.Instance?.RefreshVolumes();
            OnGraphicsChanged?.Invoke();
        }

        public void SetMasterVolume(float value, bool saveAndNotify = true)
        {
            Settings.masterVolume = Mathf.Clamp01(value);
            Settings.audioVolumeInitialized = true;
            RemakeAudioService.Instance?.RefreshVolumes();
            if (saveAndNotify)
            {
                SaveService.Save();
                OnGraphicsChanged?.Invoke();
            }
        }

        public void SetMusicVolume(float value, bool saveAndNotify = true)
        {
            Settings.musicVolume = Mathf.Clamp01(value);
            Settings.audioVolumeInitialized = true;
            RemakeAudioService.Instance?.RefreshVolumes();
            if (saveAndNotify)
            {
                SaveService.Save();
                OnGraphicsChanged?.Invoke();
            }
        }

        public void SetSfxVolume(float value, bool saveAndNotify = true)
        {
            Settings.sfxVolume = Mathf.Clamp01(value);
            Settings.audioVolumeInitialized = true;
            RemakeAudioService.Instance?.RefreshVolumes();
            if (saveAndNotify)
            {
                SaveService.Save();
                OnGraphicsChanged?.Invoke();
            }
        }

        public void SetAmbienceVolume(float value, bool saveAndNotify = true)
        {
            Settings.ambienceVolume = Mathf.Clamp01(value);
            Settings.extendedAudioInitialized = true;
            RemakeAudioService.Instance?.RefreshVolumes();
            if (saveAndNotify)
            {
                SaveService.Save();
                OnGraphicsChanged?.Invoke();
            }
        }

        public void SetVoiceVolume(float value, bool saveAndNotify = true)
        {
            Settings.voiceVolume = Mathf.Clamp01(value);
            Settings.extendedAudioInitialized = true;
            RemakeAudioService.Instance?.RefreshVolumes();
            if (saveAndNotify)
            {
                SaveService.Save();
                OnGraphicsChanged?.Invoke();
            }
        }

        public static bool IsSoftwareWebGlRenderer()
        {
            string dev = SystemInfo.graphicsDeviceName ?? "";
            return dev.IndexOf("SwiftShader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   dev.IndexOf("Basic Render", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   dev.IndexOf("llvmpipe", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   dev.IndexOf("Software", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void ApplyFromSave()
        {
            var g = Settings;
            QualitySettings.vSyncCount = 0;

#if UNITY_WEBGL && !UNITY_EDITOR
            // CRITICAL PC WebGL FPS FIX:
            // Setting Application.targetFrameRate = 60 on WebGL disables requestAnimationFrame() and falls back
            // to JS setTimeout(16ms), which Windows Chrome/Edge timer quantization throttles down to 25-35 FPS
            // across ALL graphics presets! Using -1 restores native requestAnimationFrame() synced smoothly to the display.
            if (g.afkDimMode)
                Application.targetFrameRate = 15;
            else if (g.targetFps <= 35)
                Application.targetFrameRate = 30;
            else
                Application.targetFrameRate = -1;
#else
            Application.targetFrameRate = g.afkDimMode ? 15 : (g.targetFps <= 35 ? 30 : 60);
#endif

            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.renderScale = CurrentRenderScale;
                switch (g.shadowMode)
                {
                    case RemakeShadowMode.Off:
                        urp.shadowDistance = 0f;
                        urp.shadowCascadeCount = 1;
                        break;
                    case RemakeShadowMode.Low:
                        urp.mainLightShadowmapResolution = 512;
                        urp.shadowCascadeCount = 1;
                        urp.shadowDistance = 24f;
                        break;
                    case RemakeShadowMode.High:
                        urp.mainLightShadowmapResolution = 1024;
                        urp.shadowCascadeCount = 2;
                        urp.shadowDistance = 40f;
                        break;
                }
#if UNITY_WEBGL && !UNITY_EDITOR
                // Avoid expensive WebGL 2 multisample renderbuffer resolve blits on ANGLE D3D11 desktop browsers
                urp.msaaSampleCount = 1;
#else
                urp.msaaSampleCount = (g.preset == RemakeGraphicsPreset.High && !PlatformWebBridge.IsTouchPlatform()) ? 2 : 1;
#endif
            }

            // Enable Post-Processing (Bloom) only on High preset for WebGL/Mobile so Low & Medium stay zero-blit fast
#if UNITY_WEBGL && !UNITY_EDITOR
            bool enablePostFx = g.preset == RemakeGraphicsPreset.High;
#else
            bool enablePostFx = g.preset == RemakeGraphicsPreset.High ||
                                (g.preset == RemakeGraphicsPreset.Medium && !PlatformWebBridge.IsTouchPlatform());
#endif
            var cams = FindObjectsByType<UniversalAdditionalCameraData>(FindObjectsInactive.Exclude);
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i] != null) cams[i].renderPostProcessing = enablePostFx;
            }

            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            for (int i = 0; i < lights.Length; i++)
            {
                Light l = lights[i];
                if (l != null && l.type == LightType.Directional)
                {
                    l.shadows = g.shadowMode == RemakeShadowMode.Off ? LightShadows.None : LightShadows.Soft;
                }
            }

            OnGraphicsChanged?.Invoke();
        }

        private void CaptureUrpSnapshot()
        {
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) return;
            _capturedUrp = urp;
            _origRenderScale = urp.renderScale;
            _origShadowRes = urp.mainLightShadowmapResolution;
            _origCascadeCount = urp.shadowCascadeCount;
            _origShadowDist = urp.shadowDistance;
            _origMsaa = urp.msaaSampleCount;
            _hasCapturedUrp = true;
        }

        private void RestoreUrpSnapshot()
        {
#if UNITY_EDITOR
            if (!_hasCapturedUrp || _capturedUrp == null) return;
            _capturedUrp.renderScale = _origRenderScale;
            _capturedUrp.mainLightShadowmapResolution = _origShadowRes;
            _capturedUrp.shadowCascadeCount = _origCascadeCount;
            _capturedUrp.shadowDistance = _origShadowDist;
            _capturedUrp.msaaSampleCount = _origMsaa;
#endif
        }
    }
}

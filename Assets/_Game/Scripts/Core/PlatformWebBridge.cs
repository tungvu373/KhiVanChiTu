using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class PlatformWebBridge : MonoBehaviour
    {
        public static PlatformWebBridge Instance { get; private set; }

        public event Action<string> OnLifecycleSignal;

        private Action<string> _pendingTextCallback;
        private bool _webSuspended;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLBridge_InstallLifecycleHooks(string gameObjectName);

        [DllImport("__Internal")]
        private static extern int WebGLBridge_IsTouchBrowser();

        [DllImport("__Internal")]
        private static extern int WebGLBridge_OpenNativeTextInput(
            string gameObjectName,
            string callbackMethod,
            string title,
            string initialValue,
            int maxLength);
#endif

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

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLBridge_InstallLifecycleHooks(gameObject.name);
#endif
        }

        public static bool IsTouchPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { return WebGLBridge_IsTouchBrowser() == 1; } catch { return Application.isMobilePlatform; }
#else
            return Application.isMobilePlatform;
#endif
        }

        public bool TryOpenTouchNativeInput(string title, string initialValue, int maxLength, Action<string> onConfirmed)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (IsTouchPlatform())
            {
                _pendingTextCallback = onConfirmed;
                return WebGLBridge_OpenNativeTextInput(gameObject.name, nameof(OnNativeInputCompleted), title, initialValue ?? "", maxLength) == 1;
            }
#endif
            return false;
        }

        // Called from JS (.jslib) on WebGL builds
        public void OnNativeInputCompleted(string submittedValue)
        {
            Action<string> cb = _pendingTextCallback;
            _pendingTextCallback = null;
            cb?.Invoke(submittedValue ?? "");
        }

        // Called from WebGL visibility and graphics lifecycle hooks.
        public void OnWebLifecycleSignal(string reason)
        {
            if (reason == "hidden" || reason == "pagehide")
            {
                if (_webSuspended) return;
                _webSuspended = true;
                SaveActiveCheckpoint();
            }
            else if (reason == "visible")
            {
                _webSuspended = false;
                BackgroundCatchupService.TryCatchupFromElapsed();
            }
            else if (reason == "graphics-lost")
            {
                SaveActiveCheckpoint();
            }
            OnLifecycleSignal?.Invoke(reason);
        }

        private static void SaveActiveCheckpoint()
        {
            BackgroundCatchupService.MarkActiveTimestamp();
            SaveService.Save();
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnApplicationQuit() => SaveActiveCheckpoint();

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveActiveCheckpoint();
            }
            else
            {
                BackgroundCatchupService.TryCatchupFromElapsed();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveActiveCheckpoint();
            }
            else
            {
                BackgroundCatchupService.TryCatchupFromElapsed();
            }
        }
#endif
    }
}

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

        // Called from JS visibilitychange / pagehide / beforeunload
        public void OnWebLifecycleSignal(string reason)
        {
            if (reason == "hidden" || reason == "pagehide" || reason == "beforeunload")
            {
                BackgroundCatchupService.MarkActiveTimestamp();
                SaveService.Save();
            }
            else if (reason == "visible")
            {
                BackgroundCatchupService.TryCatchupFromElapsed();
            }
            OnLifecycleSignal?.Invoke(reason);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                BackgroundCatchupService.MarkActiveTimestamp();
                SaveService.Save();
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
                BackgroundCatchupService.MarkActiveTimestamp();
                SaveService.Save();
            }
            else
            {
                BackgroundCatchupService.TryCatchupFromElapsed();
            }
        }
    }
}

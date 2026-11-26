mergeInto(LibraryManager.library, {
  WebGLBridge_SyncIndexedDb: function () {
    try {
      if (typeof FS !== "undefined" && typeof FS.syncfs === "function") {
        FS.syncfs(false, function (err) {
          if (err && typeof console !== "undefined") {
            console.warn("[KhiVan] IndexedDB syncfs warning:", err);
          }
        });
      }
    } catch (e) {
      if (typeof console !== "undefined") console.warn("[KhiVan] SyncIndexedDb error:", e);
    }
  },

  WebGLBridge_InstallLifecycleHooks: function (gameObjectNamePtr) {
    try {
      var targetName = UTF8ToString(gameObjectNamePtr);
      window.__khivanLifecycleTarget = targetName;
      if (!window.__khivanLifecycleInstalled) {
        window.__khivanLifecycleInstalled = true;
        var flushAndNotify = function (reason) {
          try {
            if (typeof FS !== "undefined" && typeof FS.syncfs === "function") {
              FS.syncfs(false, function () {});
            }
            if (window.unityInstance && window.__khivanLifecycleTarget) {
              window.unityInstance.SendMessage(window.__khivanLifecycleTarget, "OnWebLifecycleSignal", reason);
            }
          } catch (e) {}
        };
        document.addEventListener("visibilitychange", function () {
          flushAndNotify(document.visibilityState === "hidden" ? "hidden" : "visible");
        });
        window.addEventListener("pagehide", function () {
          flushAndNotify("pagehide");
        });
        window.addEventListener("beforeunload", function () {
          flushAndNotify("beforeunload");
        });
      }
    } catch (e) {}
  },

  WebGLBridge_IsTouchBrowser: function () {
    try {
      var touch = ("ontouchstart" in window) || (navigator.maxTouchPoints > 0);
      var mobileUa = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent || "");
      return (touch || mobileUa) ? 1 : 0;
    } catch (e) {
      return 0;
    }
  },

  WebGLBridge_OpenNativeTextInput: function (gameObjectNamePtr, callbackMethodPtr, titlePtr, initialValuePtr, maxLength) {
    try {
      var goName = UTF8ToString(gameObjectNamePtr);
      var method = UTF8ToString(callbackMethodPtr);
      var title = UTF8ToString(titlePtr);
      var initialValue = UTF8ToString(initialValuePtr);

      if (window.KhiVanWebBridge && typeof window.KhiVanWebBridge.openInput === "function") {
        window.KhiVanWebBridge.openInput(goName, method, title, initialValue, maxLength);
        return 1;
      }

      var result = window.prompt(title, initialValue);
      if (result !== null && window.unityInstance) {
        if (maxLength > 0 && result.length > maxLength) result = result.substring(0, maxLength);
        window.unityInstance.SendMessage(goName, method, result);
      }
      return 1;
    } catch (e) {
      return 0;
    }
  }
});

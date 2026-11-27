mergeInto(LibraryManager.library, {
  $KhiVanSaveSync__deps: ["$FS"],
  $KhiVanSaveSync: {
    running: false,
    pending: false,
    requests: 0,
    waiters: [],
    flush: function () {
      KhiVanSaveSync.requests++;
      KhiVanSaveSync.pending = true;
      if (!KhiVanSaveSync.running) KhiVanSaveSync.write();
    },
    write: function () {
      KhiVanSaveSync.running = true;
      KhiVanSaveSync.pending = false;
      var complete = function (error) {
        KhiVanSaveSync.running = false;
        if (error) {
          console.warn("[KhiVan] IndexedDB save failed:", error);
        } else if (KhiVanSaveSync.pending) {
          KhiVanSaveSync.write();
          return;
        }
        KhiVanSaveSync.pending = false;
        var waiters = KhiVanSaveSync.waiters;
        KhiVanSaveSync.waiters = [];
        for (var i = 0; i < waiters.length; i++) waiters[i](!error);
      };
      try {
        FS.syncfs(false, complete);
      } catch (error) {
        complete(error);
      }
    },
  },

  WebGLBridge_SyncIndexedDb__deps: ["$KhiVanSaveSync"],
  WebGLBridge_SyncIndexedDb: function () {
    KhiVanSaveSync.flush();
  },

  WebGLBridge_InstallLifecycleHooks__deps: ["$KhiVanSaveSync"],
  WebGLBridge_InstallLifecycleHooks: function (gameObjectNamePtr) {
    window.__khivanLifecycleTarget = UTF8ToString(gameObjectNamePtr);
    if (window.__khivanLifecycleInstalled) return;
    window.__khivanLifecycleInstalled = true;

    var notify = function (reason) {
      if (window.unityInstance)
        window.unityInstance.SendMessage(window.__khivanLifecycleTarget, "OnWebLifecycleSignal", reason);
    };
    window.KhiVanSaveBeforeGraphicsReload = function () {
      return new Promise(function (resolve) {
        var previousRequests = KhiVanSaveSync.requests;
        try {
          notify("graphics-lost");
        } catch (error) {
          console.warn("[KhiVan] Could not request a graphics recovery checkpoint:", error);
        }
        if (KhiVanSaveSync.requests === previousRequests) {
          resolve(false);
        } else if (KhiVanSaveSync.running) {
          KhiVanSaveSync.waiters.push(resolve);
        } else {
          resolve(false);
        }
      });
    };
    document.addEventListener("visibilitychange", function () {
      notify(document.visibilityState === "hidden" ? "hidden" : "visible");
    });
    window.addEventListener("pagehide", function () {
      notify("pagehide");
    });
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

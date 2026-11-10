#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MobileDeviceViewTool
{
    private static Type gvsType => typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
    private static Type gvSizeType => typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
    private static Type gvSizeGroupType => typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeGroup");
    private static Type gameViewType => typeof(Editor).Assembly.GetType("UnityEditor.GameView");

    private static object GetCurrentGroup()
    {
        var instance = gvsType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
        return gvsType.GetProperty("currentGroup", BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance);
    }

    public static int FindOrAddSize(string name, int width, int height, bool isFixed)
    {
        var currentGroup = GetCurrentGroup();
        if (currentGroup == null) return -1;

        var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");
        var getTotalCount = currentGroup.GetType().GetMethod("GetTotalCount");
        int count = (int)getTotalCount.Invoke(currentGroup, null);

        for (int i = 0; i < count; i++)
        {
            var item = getGameViewSize.Invoke(currentGroup, new object[] { i });
            string text = (string)item.GetType().GetProperty("baseText")?.GetValue(item);
            int w = (int)item.GetType().GetProperty("width")?.GetValue(item);
            int h = (int)item.GetType().GetProperty("height")?.GetValue(item);
            if (w == width && h == height && text == name)
            {
                return i;
            }
        }

        // Add custom size
        // Constructor: GameViewSize(GameViewSizeType type, int width, int height, string baseText)
        var sizeTypeEnum = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        object sizeTypeVal = Enum.ToObject(sizeTypeEnum, isFixed ? 1 : 0);
        var ctor = gvSizeType.GetConstructor(new Type[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
        var newSize = ctor.Invoke(new object[] { sizeTypeVal, width, height, name });

        var addCustomSize = currentGroup.GetType().GetMethod("AddCustomSize");
        addCustomSize.Invoke(currentGroup, new object[] { newSize });

        return (int)getTotalCount.Invoke(currentGroup, null) - 1;
    }

    public static void SetGameViewSize(int index)
    {
        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        if (gameView == null || index < 0) return;

        var sizeSelectMethod = gameView.GetType().GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sizeSelectMethod?.Invoke(gameView, new object[] { index, null });
        gameView.Repaint();
    }

    [MenuItem("TuTien/UI/Set GameView 9:16 Standard")]
    public static void Set9x16()
    {
        int idx = FindOrAddSize("Mobile 9:16 (1080x1920)", 1080, 1920, true);
        SetGameViewSize(idx);
        Debug.Log($"✅ GameView set to Mobile 9:16 (1080x1920) at index {idx}");
    }

    [MenuItem("TuTien/UI/Set GameView Notch 9:19.5")]
    public static void SetNotch()
    {
        int idx = FindOrAddSize("Mobile Notch (1080x2340)", 1080, 2340, true);
        SetGameViewSize(idx);
        Debug.Log($"✅ GameView set to Mobile Notch (1080x2340) at index {idx}");
    }

    [MenuItem("TuTien/UI/Setup Gameplay Arena State")]
    public static void SetupGameplayArenaState()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var mainMenu = GameObject.Find("MainMenuManager") ?? GameObject.Find("Canvas/MainMenuManager");
        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
            Debug.Log("✅ MainMenuManager deactivated.");
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot != null)
        {
            mobileRoot.gameObject.SetActive(true);
            Debug.Log("✅ MobileUIRoot activated.");
        }

        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        gameView?.Repaint();
    }

    [MenuItem("TuTien/UI/Test Filter Wealth In DaoAn")]
    public static void TestFilterWealthInDaoAn()
    {
        var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (daoAnUI == null) { Debug.LogError("DaoAnModalUI not found!"); return; }
        daoAnUI.SetFilter("wealth");
        Debug.Log("Switched filter to 'wealth'. Refreshing view...");
    }

    [MenuItem("TuTien/UI/Test Filter Sword In DaoAn")]
    public static void TestFilterSwordInDaoAn()
    {
        var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (daoAnUI == null) { Debug.LogError("DaoAnModalUI not found!"); return; }
        daoAnUI.SetFilter("sword");
        Debug.Log("Switched filter to 'sword'. Refreshing view...");
    }

    [MenuItem("TuTien/UI/Test Filter All In DaoAn")]
    public static void TestFilterAllInDaoAn()
    {
        var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (daoAnUI == null) { Debug.LogError("DaoAnModalUI not found!"); return; }
        daoAnUI.SetFilter("all");
        Debug.Log("Switched filter to 'all'. Refreshing view...");
    }

    [MenuItem("TuTien/UI/Rebuild And Test DaoAn Modal")]
    public static void RebuildAndTestDaoAnModal()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var old = canvas.transform.Find("DaoAnModal");
        if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        if (daoAnUI == null)
        {
            var uiMgr = GameObject.Find("Canvas/UIManager") ?? canvas;
            daoAnUI = uiMgr.AddComponent<DaoAnModalUI>();
        }

        // Clean any old Canvas component from DaoAnModal if it exists
        daoAnUI.BuildFreshModal(canvas.transform);
        var newModal = canvas.transform.Find("DaoAnModal");
        if (newModal != null)
        {
            var c = newModal.GetComponent<Canvas>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
            var ray = newModal.GetComponent<GraphicRaycaster>();
            if (ray != null) UnityEngine.Object.DestroyImmediate(ray);

            var box = newModal.Find("ModalBox");
            if (box != null && box.GetComponent<SafeAreaFitter>() == null)
            {
                var sf = box.gameObject.AddComponent<SafeAreaFitter>();
                sf.Refresh();
            }

            newModal.gameObject.SetActive(true);
            newModal.SetAsLastSibling();
            daoAnUI.Refresh();
            Debug.Log("✅ Rebuilt DaoAnModal as pure ScreenSpaceOverlay child with SafeAreaFitter!");
        }

        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        gameView?.Repaint();
    }
    public static void TestOpenDaoAnModalInEditor()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            daoAn.gameObject.SetActive(true);
            daoAn.SetAsLastSibling();
            var ui = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
            if (ui != null)
            {
                ui.BuildIfNeeded();
                ui.Refresh();
            }
            Debug.Log("✅ DaoAnModal activated and refreshed in Editor.");
        }
    }
    public static void ToggleDaoAnModalInView()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var modalTr = canvas.transform.Find("DaoAnModal");
        if (modalTr != null)
        {
            bool newState = !modalTr.gameObject.activeSelf;
            modalTr.gameObject.SetActive(newState);
            if (newState)
            {
                var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
                daoAnUI?.Refresh();
            }
            Debug.Log($"✅ DaoAnModal active set to: {newState}");
        }

        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        gameView?.Repaint();
    }
    [MenuItem("TuTien/UI/Dump ModalBox Direct Children")]
    public static void DumpModalBoxDirectChildren()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var box = canvas.transform.Find("DaoAnModal/ModalBox") as RectTransform;
        if (box == null) { Debug.LogError("ModalBox not found"); return; }
        
        Vector3[] bWc = new Vector3[4];
        box.GetWorldCorners(bWc);
        Debug.Log($"=== MODALBOX: aMin={box.anchorMin}, aMax={box.anchorMax}, pos={box.anchoredPosition}, sizeDelta={box.sizeDelta}, rect={box.rect}, bl={bWc[0]}, tr={bWc[2]} ===");

        for (int i = 0; i < box.childCount; i++)
        {
            var ch = box.GetChild(i) as RectTransform;
            if (ch == null) continue;
            Vector3[] wc = new Vector3[4];
            ch.GetWorldCorners(wc);
            var txt = ch.GetComponent<Text>();
            string tInfo = txt != null ? $" txt='{txt.text}' (active={ch.gameObject.activeSelf})" : "";
            Debug.Log($"   [{i}] '{ch.name}': aMin={ch.anchorMin}, aMax={ch.anchorMax}, pos={ch.anchoredPosition}, size={ch.rect.size}, bl={wc[0]}, tr={wc[2]}{tInfo}");
        }
    }

    [MenuItem("TuTien/UI/Open DaoAn Modal For Capture")]
    public static void OpenDaoAnModalForCapture()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            daoAn.gameObject.SetActive(true);
            daoAn.SetAsLastSibling();
            var ui = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
            if (ui != null)
            {
                ui.Refresh();
            }
            Debug.Log("✅ DaoAnModal opened for capture!");
        }
        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        gameView?.Repaint();
    }

    [MenuItem("TuTien/UI/Close DaoAn Modal For Capture")]
    public static void CloseDaoAnModalForCapture()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            daoAn.gameObject.SetActive(false);
            Debug.Log("✅ DaoAnModal closed for capture!");
        }
        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        gameView?.Repaint();
    }
    public static void DumpCanvasChildrenOrder()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        Debug.Log($"=== CANVAS DIRECT CHILDREN ({canvas.transform.childCount}) ===");
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var ch = canvas.transform.GetChild(i);
            var cComp = ch.GetComponent<Canvas>();
            string cInfo = cComp != null ? $" [CANVAS overrideSorting={cComp.overrideSorting} sortingOrder={cComp.sortingOrder} renderMode={cComp.renderMode}]" : "";
            Debug.Log($"[{i}] '{ch.name}' active={ch.gameObject.activeSelf}{cInfo}");
        }
    }

    [MenuItem("TuTien/UI/Dump DaoAn Modal Hierarchy")]
    public static void DumpDaoAnModalHierarchy()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            Debug.Log($"=== COMPONENTS ON DaoAnModal ===");
            foreach (var c in daoAn.GetComponents<Component>())
            {
                Debug.Log($"   Comp: {c.GetType().FullName}");
            }
        }
        var daoAnUI = UnityEngine.Object.FindAnyObjectByType<DaoAnModalUI>(FindObjectsInactive.Include);
        Debug.Log($"DaoAnModalUI instance in scene: {(daoAnUI != null ? daoAnUI.gameObject.name : "NONE")}");
    }
    public static void InspectMobileDockDetailed()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var dock = canvas.transform.Find("MobileUIRoot/MobileDock");
        if (dock == null) return;

        Debug.Log($"=== DOCK COMPONENTS ===");
        foreach (var c in dock.GetComponents<Component>())
        {
            Debug.Log($"   Dock comp: {c.GetType().Name}");
            if (c is HorizontalLayoutGroup hlg)
            {
                Debug.Log($"      HLG: childControlWidth={hlg.childControlWidth}, childControlHeight={hlg.childControlHeight}, childForceExpandWidth={hlg.childForceExpandWidth}, spacing={hlg.spacing}");
            }
        }

        for (int i = 0; i < dock.childCount; i++)
        {
            var ch = dock.GetChild(i) as RectTransform;
            var txt = ch.GetComponentInChildren<Text>();
            Debug.Log($"   Tab [{i}] '{ch.name}': anchorMin={ch.anchorMin}, anchorMax={ch.anchorMax}, offsetMin={ch.offsetMin}, offsetMax={ch.offsetMax}, pos={ch.anchoredPosition}, size={ch.rect.size}, text='{txt?.text}'");
        }
    }
    public static void PrintScreenCoordinates()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var root = canvas.transform.Find("MobileUIRoot") as RectTransform;
        if (root == null) return;

        Debug.Log($"=== SCREEN PIXEL RESOLUTION: {Screen.width} x {Screen.height}, safeArea={Screen.safeArea} ===");
        var canvasComp = canvas.GetComponent<Canvas>();
        Debug.Log($"Canvas pixelRect: {canvasComp.pixelRect}, scaleFactor: {canvasComp.scaleFactor}");

        string[] list = { "TopBar", "CharacterFrame", "BtnHUDDaoAn", "PauseButton", "TutorialTargetQuestBar", "MobileDock", "PlayerHPBar" };
        foreach (var item in list)
        {
            var tr = root.Find(item) as RectTransform;
            if (tr == null) continue;
            Vector3[] wc = new Vector3[4];
            tr.GetWorldCorners(wc);
            Debug.Log($"[{item}]: World Corners: [0] (bl)={wc[0]}, [2] (tr)={wc[2]}");
        }
    }
    public static void InspectGameViewMembers()
    {
        var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
        if (gameView == null) return;

        Debug.Log($"=== GAMEVIEW TYPE: {gameView.GetType().FullName}, base={gameView.GetType().BaseType?.FullName} ===");
        var type = gameView.GetType();
        while (type != null && type != typeof(EditorWindow))
        {
            Debug.Log($"--- Type {type.Name} properties: ---");
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                try
                {
                    object val = prop.GetValue(gameView);
                    Debug.Log($"   Prop: {prop.Name} ({prop.PropertyType.Name}) = {val}");
                }
                catch { }
            }
            type = type.BaseType;
        }
    }
    public static void DeepInspectTopElementsVisuals()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        var root = canvas.transform.Find("MobileUIRoot");
        if (root == null) return;

        string[] list = { "TopBar", "CharacterFrame", "BtnHUDDaoAn", "PauseButton", "PlayerHPBar", "MobileDock" };
        foreach (var item in list)
        {
            var tr = root.Find(item);
            if (tr == null) { Debug.LogWarning($"Visual check: {item} NOT FOUND!"); continue; }
            Debug.Log($"=== VISUAL CHECK: {item} (activeInHierarchy={tr.gameObject.activeInHierarchy}) ===");
            foreach (var comp in tr.GetComponents<Component>())
            {
                if (comp is Graphic g) Debug.Log($"   Graphic ({comp.GetType().Name}): color={g.color}, raycastTarget={g.raycastTarget}, canvasRenderer.cull={tr.GetComponent<CanvasRenderer>()?.cull}");
                if (comp is CanvasGroup cg) Debug.Log($"   CanvasGroup: alpha={cg.alpha}, interactable={cg.interactable}");
            }
            // Children
            for (int i = 0; i < tr.childCount; i++)
            {
                var ch = tr.GetChild(i);
                var g = ch.GetComponent<Graphic>();
                var txt = ch.GetComponent<Text>();
                string tStr = txt != null ? $" text='{txt.text}', font={(txt.font != null ? txt.font.name : "null")}, fontSize={txt.fontSize}" : "";
                string gStr = g != null ? $" color={g.color}, active={ch.gameObject.activeSelf}" : $" active={ch.gameObject.activeSelf}";
                Debug.Log($"     child [{i}] {ch.name}:{gStr}{tStr}");
            }
        }
    }
    public static void DeepInspectHUDRectTransforms()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var root = canvas.transform.Find("MobileUIRoot") as RectTransform;
        if (root != null)
        {
            Debug.Log($"[MobileUIRoot] anchorMin={root.anchorMin}, anchorMax={root.anchorMax}, offsetMin={root.offsetMin}, offsetMax={root.offsetMax}, rect={root.rect}, sizeDelta={root.sizeDelta}, pivot={root.pivot}");
            var safe = root.GetComponent<SafeAreaFitter>();
            Debug.Log($"   SafeAreaFitter: present={(safe != null)}");
        }

        string[] targets = { "TopBar", "CharacterFrame", "BtnHUDDaoAn", "StageUI", "StagePanel", "TutorialTargetQuestBar", "MobileDock", "BottomDockToggle", "PlayerHPBar" };
        foreach (var t in targets)
        {
            var tr = root != null ? root.Find(t) as RectTransform : null;
            if (tr == null) tr = canvas.transform.Find(t) as RectTransform;
            if (tr != null)
            {
                Debug.Log($"[{t}] active={tr.gameObject.activeSelf}, anchorMin={tr.anchorMin}, anchorMax={tr.anchorMax}, offsetMin={tr.offsetMin}, offsetMax={tr.offsetMax}, pos={tr.anchoredPosition}, sizeDelta={tr.sizeDelta}, rect={tr.rect}");
            }
            else
            {
                Debug.LogWarning($"[{t}] NOT FOUND!");
            }
        }

        // Also inspect DaoAnModal
        var daoAn = canvas.transform.Find("DaoAnModal");
        if (daoAn != null)
        {
            Debug.Log($"=== DETAILED DAO AN MODAL CHILDREN DUMP ===");
            PrintChildren(daoAn, 0);
        }
    }
    public static void InspectCanvasAndCamera()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var c = canvas.GetComponent<Canvas>();
            var cs = canvas.GetComponent<CanvasScaler>();
            Debug.Log($"=== CANVAS: renderMode={c.renderMode}, worldCamera={(c.worldCamera != null ? c.worldCamera.name : "null")}, sortingOrder={c.sortingOrder}, scaleFactor={c.scaleFactor}, pixelRect={c.pixelRect} ===");
            if (cs != null)
            {
                Debug.Log($"CanvasScaler: mode={cs.uiScaleMode}, refRes={cs.referenceResolution}, matchWidthOrHeight={cs.matchWidthOrHeight}");
            }
        }

        var cam = Camera.main;
        if (cam != null)
        {
            Debug.Log($"=== CAMERA: name={cam.name}, cullingMask={cam.cullingMask}, clearFlags={cam.clearFlags}, orthographic={cam.orthographic}, rect={cam.rect} ===");
        }
    }
    public static void ListSceneRoots()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        Debug.Log($"=== SCENE ROOTS ({roots.Length}) ===");
        foreach (var r in roots)
        {
            Debug.Log($"Root: {r.name} (activeSelf={r.activeSelf})");
            for (int i = 0; i < r.transform.childCount; i++)
            {
                var ch = r.transform.GetChild(i);
                Debug.Log($"   -> {ch.name} (activeSelf={ch.gameObject.activeSelf})");
            }
        }
    }
    [MenuItem("TuTien/UI/Audit Mobile HUD Hierarchy")]
    public static void AuditHUDHierarchy()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found!"); return; }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot == null) { Debug.LogError("MobileUIRoot not found!"); return; }

        Debug.Log("=== AUDIT MOBILE HUD HIERARCHY ===");
        var safeFitter = mobileRoot.GetComponent<SafeAreaFitter>();
        Debug.Log($"MobileUIRoot has SafeAreaFitter: {safeFitter != null}");

        for (int i = 0; i < mobileRoot.childCount; i++)
        {
            var child = mobileRoot.GetChild(i) as RectTransform;
            if (child == null) continue;
            var btn = child.GetComponent<UnityEngine.UI.Button>();
            string btnInfo = btn != null ? $" [BUTTON targetGraphic={btn.targetGraphic?.name}]" : "";
            Debug.Log($"Child [{i}] {child.name}: active={child.gameObject.activeSelf}, anchorMin={child.anchorMin}, anchorMax={child.anchorMax}, pivot={child.pivot}, pos={child.anchoredPosition}, size={child.rect.size}{btnInfo}");

            // If it's a container like TopBar, MobileDock, inspect its children
            if (child.name == "TopBar" || child.name == "MobileDock" || child.name == "PlayerHPBar")
            {
                for (int j = 0; j < child.childCount; j++)
                {
                    var sub = child.GetChild(j) as RectTransform;
                    if (sub == null) continue;
                    var subBtn = sub.GetComponent<UnityEngine.UI.Button>();
                    string sBtn = subBtn != null ? " [BUTTON]" : "";
                    Debug.Log($"   -> [{j}] {sub.name}: active={sub.gameObject.activeSelf}, pos={sub.anchoredPosition}, size={sub.rect.size}{sBtn}");
                }
            }
        }
    }

    private static void PrintChildren(Transform parent, int depth)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            string indent = new string(' ', depth * 2);
            var rt = child as RectTransform;
            var img = child.GetComponent<UnityEngine.UI.Image>();
            var txt = child.GetComponent<UnityEngine.UI.Text>();
            string extra = "";
            if (rt != null) extra += $" [Rect aMin={rt.anchorMin} aMax={rt.anchorMax} pos={rt.anchoredPosition} size={rt.rect.size}]";
            if (img != null) extra += $" [Image color={img.color}]";
            if (txt != null) extra += $" [Text='{txt.text}' font={(txt.font != null ? txt.font.name : "null")} color={txt.color}]";
            Debug.Log($"{indent}- {child.name} (active={child.gameObject.activeSelf}){extra}");
            PrintChildren(child, depth + 1);
        }
    }
}
#endif

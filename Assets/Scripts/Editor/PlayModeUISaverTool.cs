#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Công cụ cho phép chỉnh sửa vị trí, kích thước, anchor của UI (RectTransform)
/// ngay trong lúc Play Mode và TỰ ĐỘNG GIỮ LẠI (LƯU) khi ấn Stop!
/// </summary>
[InitializeOnLoad]
public class PlayModeUISaverTool : EditorWindow
{
    [Serializable]
    public class RectTransformSnapshot
    {
        public string hierarchyPath;
        public string objectName;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localEulerAngles;
        public Vector3 localScale;
    }

    private static List<RectTransformSnapshot> savedSnapshots = new List<RectTransformSnapshot>();
    private static bool autoApplyOnStop = true;

    static PlayModeUISaverTool()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (autoApplyOnStop && savedSnapshots.Count > 0)
            {
                // Delay 1 frame để scene Edit Mode ổn định hoàn toàn
                EditorApplication.delayCall += () =>
                {
                    ApplySavedSnapshots();
                };
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CÁC MENU ITEM TIỆN ÍCH (SHORTCUT, CONTEXT MENU)
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("TuTien/UI/💾 Lưu Vị Trí UI Đang Chọn (Giữ khi Stop) %&s", false, 10)]
    [MenuItem("GameObject/TuTien/💾 Lưu Vị Trí UI (Giữ khi Stop)", false, 0)]
    public static void SaveSelectedUI()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Thông báo", "Vui lòng chọn ít nhất một GameObject UI (có RectTransform) trong Hierarchy!", "Đồng ý");
            return;
        }

        int count = 0;
        foreach (var go in Selection.gameObjects)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                SaveRectTransform(rt);
                count++;
            }
        }

        if (count > 0)
        {
            string msg = $"✅ [PlayMode UI Saver] Đã lưu thông số cho {count} đối tượng UI!\n" +
                         "Khi bạn bấm STOP thoát Play Mode, vị trí và kích thước mới sẽ được TỰ ĐỘNG ÁP DỤNG lại vào Scene.";
            Debug.Log(msg);
            if (Application.isPlaying)
            {
                // Hiện toast nhẹ thông báo trên màn hình Editor
                ShowNotificationMessage($"Đã lưu {count} UI! Vị trí sẽ giữ khi Stop.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayMode UI Saver] Không có đối tượng nào có RectTransform được chọn!");
        }
    }

    [MenuItem("CONTEXT/RectTransform/💾 Lưu Vị Trí Này (Giữ khi Stop)")]
    public static void SaveContextRectTransform(MenuCommand command)
    {
        var rt = command.context as RectTransform;
        if (rt != null)
        {
            SaveRectTransform(rt);
            string msg = $"✅ [PlayMode UI Saver] Đã lưu vị trí '{rt.name}'! Sẽ tự động áp dụng khi Stop.";
            Debug.Log(msg);
            ShowNotificationMessage($"Đã lưu '{rt.name}'!");
        }
    }

    [MenuItem("TuTien/UI/⚡ Áp Dụng Lại Vị Trí Đã Lưu (Restore Manually)", false, 11)]
    public static void ApplyManually()
    {
        ApplySavedSnapshots();
    }

    [MenuItem("TuTien/UI/🪟 Cửa Sổ PlayMode UI Saver", false, 12)]
    public static void OpenWindow()
    {
        var win = GetWindow<PlayModeUISaverTool>("UI Saver (PlayMode)");
        win.minSize = new Vector2(360, 420);
        win.Show();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // XỬ LÝ LƯU & ÁP DỤNG DỮ LIỆU
    // ─────────────────────────────────────────────────────────────────────────

    public static void SaveRectTransform(RectTransform rt)
    {
        string path = GetHierarchyPath(rt.transform);

        // Xóa snapshot cũ nếu đã lưu trước đó
        savedSnapshots.RemoveAll(s => s.hierarchyPath == path);

        var snap = new RectTransformSnapshot
        {
            hierarchyPath = path,
            objectName = rt.name,
            anchorMin = rt.anchorMin,
            anchorMax = rt.anchorMax,
            pivot = rt.pivot,
            anchoredPosition = rt.anchoredPosition,
            sizeDelta = rt.sizeDelta,
            localEulerAngles = rt.localEulerAngles,
            localScale = rt.localScale
        };

        savedSnapshots.Add(snap);
    }

    public static void ApplySavedSnapshots()
    {
        if (savedSnapshots == null || savedSnapshots.Count == 0)
        {
            Debug.Log("[PlayMode UI Saver] Không có dữ liệu UI nào đã lưu cần áp dụng.");
            return;
        }

        int restored = 0;
        foreach (var snap in savedSnapshots)
        {
            var go = GameObject.Find(snap.hierarchyPath);
            if (go == null)
            {
                // Thử tìm không kể gạch chéo đầu
                string trimPath = snap.hierarchyPath.TrimStart('/');
                go = GameObject.Find(trimPath);
            }

            if (go != null)
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Undo.RecordObject(rt, "Restore PlayMode UI Transform");
                    rt.anchorMin = snap.anchorMin;
                    rt.anchorMax = snap.anchorMax;
                    rt.pivot = snap.pivot;
                    rt.anchoredPosition = snap.anchoredPosition;
                    rt.sizeDelta = snap.sizeDelta;
                    rt.localEulerAngles = snap.localEulerAngles;
                    rt.localScale = snap.localScale;

                    EditorUtility.SetDirty(rt);
                    restored++;
                }
            }
            else
            {
                Debug.LogWarning($"[PlayMode UI Saver] Không tìm thấy đối tượng trong Scene: {snap.hierarchyPath}");
            }
        }

        if (restored > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"🎉 [PlayMode UI Saver] ĐÃ ÁP DỤNG THÀNH CÔNG VỊ TRÍ MỚI CHO {restored} ĐỐI TƯỢNG UI VÀ ĐÃ LƯU DIRTY SCENE!");
            ShowNotificationMessage($"Đã giữ thành công {restored} UI vào Scene!");
        }
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return "/" + path;
    }

    private static void ShowNotificationMessage(string message)
    {
        var windows = Resources.FindObjectsOfTypeAll<SceneView>();
        if (windows != null && windows.Length > 0)
        {
            windows[0].ShowNotification(new GUIContent(message));
        }
        else
        {
            var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            if (gvType != null)
            {
                var gv = EditorWindow.GetWindow(gvType, false, null, false);
                if (gv != null) gv.ShowNotification(new GUIContent(message));
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GIAO DIỆN CỬA SỔ TOOL
    // ─────────────────────────────────────────────────────────────────────────

    private Vector2 scrollPos;

    private void OnGUI()
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("🎮 PlayMode UI Saver (Khí Vận Chi Tử)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Cho phép căn chỉnh UI (vị trí, kích thước, anchors) khi đang chạy game (Play Mode) và TỰ ĐỘNG LƯU lại khi bạn ấn Stop!", MessageType.Info);

        GUILayout.Space(8);
        autoApplyOnStop = EditorGUILayout.ToggleLeft("Tự động áp dụng khi Stop Play Mode", autoApplyOnStop, EditorStyles.boldLabel);

        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("📸 Lưu Vị Trí UI Đang Chọn\n(Ctrl + Alt + S)", GUILayout.Height(40)))
        {
            SaveSelectedUI();
        }

        GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
        if (GUILayout.Button("⚡ Áp Dụng Lại Bây Giờ\n(Apply Snapshots)", GUILayout.Height(40)))
        {
            ApplySavedSnapshots();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Danh sách UI đã ghi nhớ ({savedSnapshots.Count}):", EditorStyles.boldLabel);
        if (GUILayout.Button("Xóa bộ nhớ", GUILayout.Width(90)))
        {
            savedSnapshots.Clear();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box);
        if (savedSnapshots.Count == 0)
        {
            EditorGUILayout.LabelField("Chưa có UI nào được lưu. Hãy chọn UI và bấm 'Lưu Vị Trí'!", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < savedSnapshots.Count; i++)
            {
                var snap = savedSnapshots[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"<b>{i + 1}. {snap.objectName}</b>", new GUIStyle(EditorStyles.label) { richText = true });
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    savedSnapshots.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Path: {snap.hierarchyPath}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Pos: ({snap.anchoredPosition.x:F1}, {snap.anchoredPosition.y:F1}) | Size: ({snap.sizeDelta.x:F1}, {snap.sizeDelta.y:F1})", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(8);
        EditorGUILayout.HelpBox("💡 Mẹo: Bạn cũng có thể nhấp chuột phải vào component RectTransform bất kỳ trong Inspector -> chọn '💾 Lưu Vị Trí Này (Giữ khi Stop)'!", MessageType.None);
    }
}
#endif


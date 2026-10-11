#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBuilderTool
{
    [MenuItem("TuTien/Tạo Hệ Thống Hướng Dẫn Tân Thủ (Tutorial)")]
    public static void GenerateTutorialSystem()
    {
        // 1. Tìm hoặc tạo Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas! Hãy đảm bảo bạn đã tạo giao diện UI cơ bản trước.");
            return;
        }

        // 2. Tạo Tutorial Overlay
        GameObject overlayObj = GameObject.Find("TutorialOverlay");
        if (overlayObj == null)
        {
            overlayObj = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(Image));
            overlayObj.transform.SetParent(canvas.transform, false);
            overlayObj.transform.SetAsLastSibling(); // Lên trên cùng
            
            RectTransform overlayRt = overlayObj.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            // Nền đen mờ 80% (có Block Raycast vì Image tự bật Raycast Target)
            Image img = overlayObj.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
            
            // Text hướng dẫn
            GameObject textObj = new GameObject("TutorialText", typeof(RectTransform), typeof(Text), typeof(Outline));
            textObj.transform.SetParent(overlayObj.transform, false);
            
            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.5f, 0.2f); // Nằm ở 20% chiều cao phía dưới
            txtRt.anchorMax = new Vector2(0.5f, 0.2f);
            txtRt.sizeDelta = new Vector2(600, 150);
            
            Text txt = textObj.GetComponent<Text>();
            txt.text = "Hướng dẫn Tân Thủ sẽ hiện ở đây!";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 28;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            
            Outline outline = textObj.GetComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
            
            Undo.RegisterCreatedObjectUndo(overlayObj, "Create Tutorial Overlay");
        }

        // 3. Gắn TutorialManager vào GameObject Managers
        GameObject managers = GameObject.Find("Managers");
        if (managers == null) managers = new GameObject("Managers");
        
        TutorialManager tm = managers.GetComponent<TutorialManager>();
        if (tm == null) tm = managers.AddComponent<TutorialManager>();
        
        // Link reference
        tm.tutorialOverlay = overlayObj;
        tm.tutorialText = overlayObj.transform.Find("TutorialText").GetComponent<Text>();
        
        // Ẩn Overlay đi mặc định
        overlayObj.SetActive(false);
        EditorUtility.SetDirty(tm);

        Debug.Log("✅ Đã khởi tạo Hệ Thống Hướng Dẫn Tân Thủ (Tutorial) thành công!");
    }
}
#endif

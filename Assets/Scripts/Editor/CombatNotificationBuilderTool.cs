#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tạo sẵn Banner Thông Báo (CombatNotificationBanner) trực tiếp vào Canvas trong Scene.
/// Giúp Game Designer / Sinh viên có thể tự do căn chỉnh vị trí, kích thước, màu sắc trong Scene View.
/// </summary>
[InitializeOnLoad]
public class CombatNotificationBuilderTool
{
    static CombatNotificationBuilderTool()
    {
        EditorApplication.delayCall += () =>
        {
            EnsureBannerInScene(forceSelect: false);
        };
    }

    [MenuItem("TuTien/UI/Tạo Sẵn Banner Thông Báo (Đột Phá & Tử Trận)")]
    public static void CreateOrSelectBannerMenu()
    {
        EnsureBannerInScene(forceSelect: true);
    }

    public static void EnsureBannerInScene(bool forceSelect)
    {
        if (Application.isPlaying) return;

        // 1. Tìm Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Canvas c = Object.FindObjectOfType<Canvas>();
            if (c != null) canvasObj = c.gameObject;
        }

        if (canvasObj == null)
        {
            Debug.LogWarning("[CombatNotificationBuilder] Không tìm thấy Canvas trong Scene để tạo Banner!");
            return;
        }

        // 2. Kiểm tra nếu Banner đã tồn tại
        Transform existing = canvasObj.transform.Find("CombatNotificationBanner");
        if (existing != null)
        {
            if (forceSelect)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                Debug.Log("<color=cyan>🎯 [CombatNotification] Banner 'CombatNotificationBanner' ĐÃ CÓ SẴN trong Canvas. Đã chọn GameObject để bạn tự do căn chỉnh trên Scene View!</color>");
            }
            return;
        }

        // 3. Tạo GameObject Banner chính
        GameObject bannerObj = new GameObject("CombatNotificationBanner", typeof(RectTransform), typeof(CanvasGroup), typeof(CombatNotificationUI));
        Undo.RegisterCreatedObjectUndo(bannerObj, "Tạo CombatNotificationBanner");
        bannerObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = bannerObj.GetComponent<RectTransform>();
        // Căn đúng tâm của nửa màn hình Combat 3D bên trái (X từ 0 -> 0.5; tâm tại X = 0.25)
        rt.anchorMin = new Vector2(0.25f, 0.58f);
        rt.anchorMax = new Vector2(0.25f, 0.58f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(460f, 125f);

        CanvasGroup cg = bannerObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f; // Luôn hiển thị trong Editor để User quan sát & kéo chỉnh
        cg.blocksRaycasts = false;
        cg.interactable = false;

        Font fontToUse = FindBestFont(canvasObj);

        // 4. Tạo Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(bannerObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.9f);
        bgImg.raycastTarget = false;

        // 5. Tạo Viền Trên & Dưới
        Image borderTop = CreateBorder(bannerObj.transform, "BorderTop", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 3f), new Color(1f, 0.84f, 0f, 0.95f));
        Image borderBottom = CreateBorder(bannerObj.transform, "BorderBottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 3f), new Color(1f, 0.84f, 0f, 0.95f));

        // 6. Tạo Title Text (Tiêu đề lớn)
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
        titleObj.transform.SetParent(bannerObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.52f);
        titleRt.anchorMax = new Vector2(1f, 0.95f);
        titleRt.offsetMin = new Vector2(10f, 0f);
        titleRt.offsetMax = new Vector2(-10f, 0f);

        Text titleText = titleObj.GetComponent<Text>();
        titleText.font = fontToUse;
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "✨ ĐỘT PHÁ THÀNH CÔNG ✨";
        titleText.color = new Color(1f, 0.92f, 0.25f);
        titleText.raycastTarget = false;

        Outline titleOutline = titleObj.GetComponent<Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        titleOutline.effectDistance = new Vector2(2f, -2f);

        Shadow titleShadow = titleObj.GetComponent<Shadow>();
        titleShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        titleShadow.effectDistance = new Vector2(3f, -3f);

        // 7. Tạo Subtitle Text (Tên Cảnh giới)
        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(Text), typeof(Outline));
        subObj.transform.SetParent(bannerObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0f, 0.22f);
        subRt.anchorMax = new Vector2(1f, 0.55f);
        subRt.offsetMin = new Vector2(10f, 0f);
        subRt.offsetMax = new Vector2(-10f, 0f);

        Text subText = subObj.GetComponent<Text>();
        subText.font = fontToUse;
        subText.fontSize = 20;
        subText.fontStyle = FontStyle.Bold;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.text = "[ Luyện Khí Tầng 6 ]";
        subText.color = new Color(0.4f, 1f, 0.7f);
        subText.raycastTarget = false;

        Outline subOutline = subObj.GetComponent<Outline>();
        subOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        subOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // 8. Tạo Extra Detail Text
        GameObject extraObj = new GameObject("ExtraDetailText", typeof(RectTransform), typeof(Text), typeof(Outline));
        extraObj.transform.SetParent(bannerObj.transform, false);
        RectTransform extraRt = extraObj.GetComponent<RectTransform>();
        extraRt.anchorMin = new Vector2(0f, 0.04f);
        extraRt.anchorMax = new Vector2(1f, 0.24f);
        extraRt.offsetMin = new Vector2(10f, 0f);
        extraRt.offsetMax = new Vector2(-10f, 0f);

        Text extraText = extraObj.GetComponent<Text>();
        extraText.font = fontToUse;
        extraText.fontSize = 13;
        extraText.fontStyle = FontStyle.Italic;
        extraText.alignment = TextAnchor.MiddleCenter;
        extraText.text = "Khí tức ngập tràn • Thọ nguyên gia tăng • Sinh lực hồi phục";
        extraText.color = new Color(0.9f, 0.95f, 0.8f, 0.95f);
        extraText.raycastTarget = false;

        Outline extraOutline = extraObj.GetComponent<Outline>();
        extraOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        extraOutline.effectDistance = new Vector2(1f, -1f);

        // 9. Liên kết các thuộc tính vào script CombatNotificationUI
        CombatNotificationUI ui = bannerObj.GetComponent<CombatNotificationUI>();
        ui.containerRect = rt;
        ui.canvasGroup = cg;
        ui.backgroundImage = bgImg;
        ui.borderTop = borderTop;
        ui.borderBottom = borderBottom;
        ui.titleText = titleText;
        ui.subtitleText = subText;
        ui.extraDetailText = extraText;

        // Đảm bảo banner nằm trên các UI khác
        bannerObj.transform.SetAsLastSibling();

        EditorUtility.SetDirty(bannerObj);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = bannerObj;
        EditorGUIUtility.PingObject(bannerObj);
        Debug.Log("<color=green>✅ [CombatNotification] ĐÃ TẠO SẴN 'CombatNotificationBanner' trong Canvas! Bạn có thể chọn và tự do di chuyển/căn chỉnh trong Scene View ngay lúc này.</color>");
    }

    private static Image CreateBorder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Color color)
    {
        GameObject line = new GameObject(name, typeof(RectTransform), typeof(Image));
        line.transform.SetParent(parent, false);
        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;

        Image img = line.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static Font FindBestFont(GameObject canvasObj)
    {
        Text existingText = canvasObj.GetComponentInChildren<Text>(true);
        if (existingText != null && existingText.font != null)
        {
            return existingText.font;
        }

        try
        {
            Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (runtimeFont != null) return runtimeFont;
        }
        catch { }

        return Font.CreateDynamicFontFromOSFont("Arial", 16);
    }
}
#endif

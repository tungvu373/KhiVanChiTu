#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tạo sẵn TribulationModalUI trực tiếp vào Canvas trong Scene.
/// Tự động chạy khi khởi động Editor hoặc có thể chọn qua Menu TuTien/UI.
/// </summary>
public class TribulationUIBuilderTool
{
    [MenuItem("TuTien/UI/Tạo Sẵn Panel Độ Kiếp (TribulationModalUI)")]
    public static void CreateOrSelectMenu()
    {
        EnsurePanelInScene(forceSelect: true);
    }

    public static void EnsurePanelInScene(bool forceSelect)
    {
        if (Application.isPlaying) return;

        // 1. Tìm Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Canvas c = Object.FindObjectOfType<Canvas>();
            if (c != null) canvasObj = c.gameObject;
        }

        if (canvasObj == null) return;

        // 2. Kiểm tra nếu đã tồn tại
        Transform existing = canvasObj.transform.Find("TribulationModalUI");
        if (existing != null)
        {
            if (forceSelect)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                Debug.Log("<color=cyan>🎯 [TribulationUIBuilder] Panel 'TribulationModalUI' ĐÃ CÓ SẴN trong Canvas.</color>");
            }
            return;
        }

        // Fallback font đa tầng
        Font fontToUse = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fontToUse == null) fontToUse = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (fontToUse == null) fontToUse = Font.CreateDynamicFontFromOSFont("Arial", 16);

        // 3. Tạo GameObject Modal
        GameObject modalObj = new GameObject("TribulationModalUI", typeof(RectTransform), typeof(CanvasGroup), typeof(TribulationModalUI));
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = modalObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image overlay = modalObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.75f);

        CanvasGroup cg = modalObj.GetComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Modal Box
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(modalObj.transform, false);
        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(560, 420);
        boxObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 0.98f);
        Outline boxOutline = boxObj.GetComponent<Outline>();
        boxOutline.effectColor = new Color(1f, 0.8f, 0.2f, 0.9f);
        boxOutline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(boxObj.transform, false);
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = fontToUse;
        titleTxt.text = "⚡ THIÊN KIẾP HÀNG LÂM ⚡";
        titleTxt.fontSize = 26;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(1f, 0.85f, 0.2f);
        titleObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 150);
        titleRt.sizeDelta = new Vector2(520, 50);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
        subObj.transform.SetParent(boxObj.transform, false);
        Text subTxt = subObj.GetComponent<Text>();
        subTxt.font = fontToUse;
        subTxt.text = "Tu vi đã đạt Đại Viên Mãn — Bạn có muốn Độ Kiếp Thăng Cấp?";
        subTxt.fontSize = 17;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = new Color(0.6f, 1f, 0.8f);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchoredPosition = new Vector2(0, 110);
        subRt.sizeDelta = new Vector2(520, 35);

        // Content / Warning Box
        GameObject contentObj = new GameObject("WarningText", typeof(RectTransform), typeof(Text));
        contentObj.transform.SetParent(boxObj.transform, false);
        Text warnTxt = contentObj.GetComponent<Text>();
        warnTxt.font = fontToUse;
        warnTxt.fontSize = 16;
        warnTxt.alignment = TextAnchor.UpperLeft;
        warnTxt.color = Color.white;
        warnTxt.lineSpacing = 1.3f;
        RectTransform warnRt = contentObj.GetComponent<RectTransform>();
        warnRt.anchoredPosition = new Vector2(0, 5);
        warnRt.sizeDelta = new Vector2(490, 150);
        warnTxt.text = "⚔️ <color=#FFD700>Thành công:</color> Tấn thăng đại cảnh giới, mở khóa sức mạnh mới!\n" +
                       "⚠️ <color=#FF5555>Thất bại:</color> Bị Thiên Kiếp phản phệ, <color=#FF3333><b>tổn thất 25% Tu Vi</b></color>!\n" +
                       "💡 <i>Nếu chưa tự tin, hãy chọn 'Củng Cố' để nâng cấp phi kiếm. Hệ thống sẽ hỏi lại sau mỗi khi vượt tầng.</i>";

        // Button Confirm (Độ Kiếp Ngay)
        GameObject btnConfObj = new GameObject("BtnConfirm", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnConfObj.transform.SetParent(boxObj.transform, false);
        btnConfObj.GetComponent<Image>().color = new Color(0.9f, 0.65f, 0.1f);
        btnConfObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnConfRt = btnConfObj.GetComponent<RectTransform>();
        btnConfRt.anchoredPosition = new Vector2(130, -145);
        btnConfRt.sizeDelta = new Vector2(220, 55);

        GameObject btnConfTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnConfTxtObj.transform.SetParent(btnConfObj.transform, false);
        Text btnConfTxt = btnConfTxtObj.GetComponent<Text>();
        btnConfTxt.font = fontToUse;
        btnConfTxt.text = "⚔️ ĐỘ KIẾP NGAY";
        btnConfTxt.fontSize = 18;
        btnConfTxt.fontStyle = FontStyle.Bold;
        btnConfTxt.alignment = TextAnchor.MiddleCenter;
        btnConfTxt.color = Color.black;
        RectTransform bctRt = btnConfTxtObj.GetComponent<RectTransform>();
        bctRt.anchorMin = Vector2.zero; bctRt.anchorMax = Vector2.one;
        bctRt.offsetMin = Vector2.zero; bctRt.offsetMax = Vector2.zero;

        // Button Cancel (Củng Cố)
        GameObject btnCancObj = new GameObject("BtnCancel", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnCancObj.transform.SetParent(boxObj.transform, false);
        btnCancObj.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f);
        btnCancObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform btnCancRt = btnCancObj.GetComponent<RectTransform>();
        btnCancRt.anchoredPosition = new Vector2(-130, -145);
        btnCancRt.sizeDelta = new Vector2(220, 55);

        GameObject btnCancTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        btnCancTxtObj.transform.SetParent(btnCancObj.transform, false);
        Text btnCancTxt = btnCancTxtObj.GetComponent<Text>();
        btnCancTxt.font = fontToUse;
        btnCancTxt.text = "🛡️ CỦNG CỐ SỨC MẠNH";
        btnCancTxt.fontSize = 17;
        btnCancTxt.fontStyle = FontStyle.Bold;
        btnCancTxt.alignment = TextAnchor.MiddleCenter;
        btnCancTxt.color = new Color(0.85f, 0.9f, 0.95f);
        RectTransform bcancRt = btnCancTxtObj.GetComponent<RectTransform>();
        bcancRt.anchorMin = Vector2.zero; bcancRt.anchorMax = Vector2.one;
        bcancRt.offsetMin = Vector2.zero; bcancRt.offsetMax = Vector2.zero;

        // Wire references
        TribulationModalUI modal = modalObj.GetComponent<TribulationModalUI>();
        modal.canvasGroup = cg;
        modal.modalBox = boxRt;
        modal.titleText = titleTxt;
        modal.subtitleText = subTxt;
        modal.warningText = warnTxt;
        modal.confirmButton = btnConfObj.GetComponent<Button>();
        modal.cancelButton = btnCancObj.GetComponent<Button>();

        modal.confirmButton.onClick.RemoveAllListeners();
        modal.confirmButton.onClick.AddListener(modal.OnConfirmClicked);
        modal.cancelButton.onClick.RemoveAllListeners();
        modal.cancelButton.onClick.AddListener(modal.OnCancelClicked);

        modalObj.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>✅ [TribulationUIBuilder] Đã tạo thành công TribulationModalUI trong Canvas!</color>");
    }
}
#endif

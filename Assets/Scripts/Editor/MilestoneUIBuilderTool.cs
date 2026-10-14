#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tạo sẵn Giao diện Bảng Nhiệm Vụ Mốc (MilestoneModalUI) trực tiếp ra Canvas trong Scene.
/// Tự động kết nối MilestoneManager và MilestoneModalUI.
/// </summary>
public class MilestoneUIBuilderTool
{
    [MenuItem("TuTien/UI/Tạo Sẵn Bảng Nhiệm Vụ Mốc (MilestoneModalUI)")]
    public static void CreateOrSelectMenu()
    {
        EnsureMilestoneUIInScene(forceSelect: true);
    }

    public static void EnsureMilestoneUIInScene(bool forceSelect)
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
            if (forceSelect) Debug.LogError("❌ Không tìm thấy Canvas trong Scene!");
            return;
        }

        // 2. Đảm bảo Managers có MilestoneManager
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
        {
            managers = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers Object");
        }
        MilestoneManager mm = managers.GetComponent<MilestoneManager>();
        if (mm == null) mm = managers.AddComponent<MilestoneManager>();
        EditorUtility.SetDirty(mm);

        // 3. Kiểm tra nếu MilestoneModalUI đã có sẵn (Tự động nâng cấp / rebuild nếu phiên bản cũ bị lỗi)
        Transform existingModal = canvasObj.transform.Find("MilestoneModalUI");
        if (existingModal != null)
        {
            // Kiểm tra xem modal cũ có bị lỗi không (dùng Mask thay vì RectMask2D)
            Transform oldVp = existingModal.Find("ModalBox/ScrollView/Viewport");
            bool needsRebuild = (oldVp != null && oldVp.GetComponent<Mask>() != null) || forceSelect;

            if (needsRebuild)
            {
                Undo.DestroyObjectImmediate(existingModal.gameObject);
                existingModal = null;
            }
            else
            {
                if (forceSelect)
                {
                    existingModal.gameObject.SetActive(true);
                    Selection.activeGameObject = existingModal.gameObject;
                    EditorGUIUtility.PingObject(existingModal.gameObject);
                }
                return;
            }
        }

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (defaultFont == null) defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

        // 4. Tạo Overlay nền mờ (Toàn màn hình)
        GameObject modalObj = new GameObject("MilestoneModalUI", typeof(RectTransform), typeof(Image), typeof(MilestoneModalUI));
        modalObj.transform.SetParent(canvasObj.transform, false);

        RectTransform mRt = modalObj.GetComponent<RectTransform>();
        mRt.anchorMin = Vector2.zero;
        mRt.anchorMax = Vector2.one;
        mRt.offsetMin = Vector2.zero;
        mRt.offsetMax = Vector2.zero;

        Image mBg = modalObj.GetComponent<Image>();
        mBg.color = new Color(0f, 0f, 0f, 0.78f);

        MilestoneModalUI ui = modalObj.GetComponent<MilestoneModalUI>();
        ui.modalPanel = modalObj;

        // 5. Tạo Modal Box (640 x 520)
        GameObject boxObj = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        boxObj.transform.SetParent(modalObj.transform, false);

        RectTransform boxRt = boxObj.GetComponent<RectTransform>();
        boxRt.sizeDelta = new Vector2(640f, 520f);
        boxRt.anchoredPosition = Vector2.zero;

        Image boxBg = boxObj.GetComponent<Image>();
        boxBg.color = new Color(0.08f, 0.09f, 0.15f, 0.98f);

        Outline boxOutline = boxObj.GetComponent<Outline>();
        boxOutline.effectColor = new Color(1f, 0.85f, 0.25f, 0.95f);
        boxOutline.effectDistance = new Vector2(2f, -2f);

        // ── HEADER ─────────────────────────────────────────────────────────
        GameObject titleObj = new GameObject("TxtHeaderTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
        titleObj.transform.SetParent(boxObj.transform, false);
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = defaultFont;
        titleTxt.fontSize = 20;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.text = "📜 MỐC TU TIÊN KHỞI ĐẠO";
        titleTxt.color = new Color(1f, 0.9f, 0.35f);
        titleObj.GetComponent<Outline>().effectColor = Color.black;
        titleObj.GetComponent<Outline>().effectDistance = new Vector2(1.5f, -1.5f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.5f, 1f);
        tRt.anchorMax = new Vector2(0.5f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.anchoredPosition = new Vector2(0, -12f);
        tRt.sizeDelta = new Vector2(400f, 30f);

        GameObject progObj = new GameObject("TxtTotalProgress", typeof(RectTransform), typeof(Text));
        progObj.transform.SetParent(boxObj.transform, false);
        Text progTxt = progObj.GetComponent<Text>();
        progTxt.font = defaultFont;
        progTxt.fontSize = 12;
        progTxt.alignment = TextAnchor.MiddleCenter;
        progTxt.text = "Tiến Độ: 0/16 Đạt Mốc | (0 Chờ Nhận)";
        progTxt.color = new Color(0.7f, 0.85f, 1f);
        RectTransform pRt = progObj.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 1f);
        pRt.anchorMax = new Vector2(0.5f, 1f);
        pRt.pivot = new Vector2(0.5f, 1f);
        pRt.anchoredPosition = new Vector2(0, -42f);
        pRt.sizeDelta = new Vector2(400f, 20f);
        ui.txtTotalProgress = progTxt;

        // Nút Đóng [X]
        GameObject btnCloseObj = new GameObject("BtnClose", typeof(RectTransform), typeof(Button), typeof(Text));
        btnCloseObj.transform.SetParent(boxObj.transform, false);
        RectTransform cRt = btnCloseObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(1, 1);
        cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(1, 1);
        cRt.anchoredPosition = new Vector2(-10f, -10f);
        cRt.sizeDelta = new Vector2(30f, 30f);
        Text cTxt = btnCloseObj.GetComponent<Text>();
        cTxt.font = defaultFont;
        cTxt.fontSize = 20;
        cTxt.fontStyle = FontStyle.Bold;
        cTxt.text = "✕";
        cTxt.color = new Color(1f, 0.4f, 0.4f);
        cTxt.alignment = TextAnchor.MiddleCenter;
        ui.btnClose = btnCloseObj.GetComponent<Button>();

        // ── SCROLL VIEW (Danh sách nhiệm vụ) ────────────────────────────────
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(boxObj.transform, false);
        RectTransform sRt = scrollObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0);
        sRt.anchorMax = new Vector2(1, 1);
        sRt.offsetMin = new Vector2(15f, 60f); // Chừa chỗ cho nút Nhận Toàn Bộ ở dưới
        sRt.offsetMax = new Vector2(-15f, -70f);

        Image scrollImg = scrollObj.GetComponent<Image>();
        scrollImg.color = new Color(0.04f, 0.05f, 0.08f, 0.6f);

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 25f;

        // Viewport: Dùng RectMask2D cắt gọn 100%, không bị lỗi stencil
        GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        vpObj.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = vpObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        sr.viewport = vpRt;

        // Content
        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(vpObj.transform, false);
        RectTransform cntRt = contentObj.GetComponent<RectTransform>();
        cntRt.anchorMin = new Vector2(0, 1);
        cntRt.anchorMax = new Vector2(1, 1);
        cntRt.pivot = new Vector2(0.5f, 1);
        cntRt.offsetMin = Vector2.zero;
        cntRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        sr.content = cntRt;
        ui.contentContainer = cntRt;

        // ── FOOTER: Nút Nhận Toàn Bộ ───────────────────────────────────────
        GameObject btnClaimObj = new GameObject("BtnClaimAll", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
        btnClaimObj.transform.SetParent(boxObj.transform, false);
        RectTransform clRt = btnClaimObj.GetComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0.5f, 0);
        clRt.anchorMax = new Vector2(0.5f, 0);
        clRt.pivot = new Vector2(0.5f, 0);
        clRt.anchoredPosition = new Vector2(0, 12f);
        clRt.sizeDelta = new Vector2(240f, 38f);

        Image clImg = btnClaimObj.GetComponent<Image>();
        clImg.color = new Color(0.95f, 0.75f, 0.15f, 1f);

        Outline clOutline = btnClaimObj.GetComponent<Outline>();
        clOutline.effectColor = Color.black;
        clOutline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject clTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        clTxtObj.transform.SetParent(btnClaimObj.transform, false);
        Text clTxt = clTxtObj.GetComponent<Text>();
        clTxt.font = defaultFont;
        clTxt.fontSize = 14;
        clTxt.fontStyle = FontStyle.Bold;
        clTxt.text = "✨ NHẬN TOÀN BỘ";
        clTxt.color = new Color(0.1f, 0.1f, 0.1f);
        clTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform cltRt = clTxtObj.GetComponent<RectTransform>();
        cltRt.anchorMin = Vector2.zero;
        cltRt.anchorMax = Vector2.one;
        cltRt.offsetMin = Vector2.zero;
        cltRt.offsetMax = Vector2.zero;

        ui.btnClaimAll = btnClaimObj.GetComponent<Button>();

        // Mặc định ẩn trong Editor khi khởi động
        modalObj.SetActive(false);

        Undo.RegisterCreatedObjectUndo(modalObj, "Create MilestoneModalUI");
        EditorUtility.SetDirty(ui);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        if (forceSelect)
        {
            modalObj.SetActive(true);
            Selection.activeGameObject = modalObj;
            EditorGUIUtility.PingObject(modalObj);
            Debug.Log("<color=green>✅ [MilestoneUIBuilder] Đã tạo sẵn 'MilestoneModalUI' trên Canvas thành công!</color>");
        }
    }
}
#endif

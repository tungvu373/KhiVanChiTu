#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool tạo sẵn UI Hướng Dẫn Tân Thủ (Target Quest Bar + Pointer Arrow) trực tiếp ra Canvas trong Scene.
/// Giúp người phát triển dễ dàng quan sát, kéo thả, căn chỉnh RectTransform theo ý muốn.
/// </summary>
public class TutorialBuilderTool
{
    [MenuItem("TuTien/UI/Tạo Sẵn UI Hướng Dẫn (Target Quest Bar)")]
    public static void CreateOrSelectMenu()
    {
        EnsureTutorialUIInScene(forceSelect: true);
    }

    [MenuItem("TuTien/Tạo Hệ Thống Hướng Dẫn Tân Thủ (Tutorial)")]
    public static void GenerateTutorialSystem()
    {
        EnsureTutorialUIInScene(forceSelect: true);
    }

    public static void EnsureTutorialUIInScene(bool forceSelect)
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

        // 2. Dọn dẹp TutorialOverlay màn hình đen cũ nếu còn sót lại
        GameObject legacyOverlay = GameObject.Find("TutorialOverlay");
        if (legacyOverlay != null)
        {
            legacyOverlay.SetActive(false);
            Undo.DestroyObjectImmediate(legacyOverlay);
        }

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (defaultFont == null) defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

        // 3. TẠO HOẶC TÌM THANH NHIỆM VỤ MỤC TIÊU (TutorialTargetQuestBar)
        Transform existingQuestBar = canvasObj.transform.Find("TutorialTargetQuestBar");
        GameObject questBarObj = existingQuestBar != null ? existingQuestBar.gameObject : null;

        Text txtQuestTitle = null;
        Text txtQuestReward = null;
        Image questBarBg = null;
        Image questProgressFill = null;
        Outline questBarOutline = null;
        Button btnClaimQuick = null;
        Text txtClaimQuick = null;
        ScrollRect descScrollRect = null;

        bool isNewQuestBar = (questBarObj == null);
        if (isNewQuestBar)
        {
            questBarObj = new GameObject("TutorialTargetQuestBar", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
            questBarObj.transform.SetParent(canvasObj.transform, false);
            Undo.RegisterCreatedObjectUndo(questBarObj, "Create Tutorial Target Quest Bar");
        }

        // Kích thước & Vị trí:
        // TUYỆT ĐỐI GIỮ NGUYÊN 100% ANCHOR, PIVOT, VỊ TRÍ NẾU OBJECT ĐÃ ĐƯỢC USER CĂN CHỈNH!
        // CHỈ GÁN MẶC ĐỊNH NẾU ĐÂY LÀ OBJECT MỚI TINH ĐƯỢC TẠO
        RectTransform qRt = questBarObj.GetComponent<RectTransform>();
        if (isNewQuestBar)
        {
            qRt.anchorMin = new Vector2(0f, 1f);
            qRt.anchorMax = new Vector2(0f, 1f);
            qRt.pivot = new Vector2(0f, 1f);
            qRt.anchoredPosition = new Vector2(15f, -44f);
            qRt.sizeDelta = new Vector2(430f, 56f);
        }

        questBarBg = questBarObj.GetComponent<Image>();
        if (questBarBg == null) questBarBg = questBarObj.AddComponent<Image>();
        questBarBg.color = new Color(0.06f, 0.06f, 0.12f, 0.94f);
        questBarBg.raycastTarget = true;

        Button qBtn = questBarObj.GetComponent<Button>();
        if (qBtn == null) qBtn = questBarObj.AddComponent<Button>();
        qBtn.transition = Selectable.Transition.None;

        questBarOutline = questBarObj.GetComponent<Outline>();
        if (questBarOutline == null) questBarOutline = questBarObj.AddComponent<Outline>();
        questBarOutline.effectColor = new Color(1f, 0.82f, 0.25f, 0.85f);
        questBarOutline.effectDistance = new Vector2(1.2f, -1.2f);

        // MiniProgressBG
        Transform pBgTr = questBarObj.transform.Find("MiniProgressBG");
        GameObject pBgObj = pBgTr != null ? pBgTr.gameObject : null;
        if (pBgObj == null)
        {
            pBgObj = new GameObject("MiniProgressBG", typeof(RectTransform), typeof(Image));
            pBgObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform pBgRt = pBgObj.GetComponent<RectTransform>();
        pBgRt.anchorMin = new Vector2(0, 0);
        pBgRt.anchorMax = new Vector2(1, 0);
        pBgRt.pivot = new Vector2(0, 0);
        pBgRt.anchoredPosition = Vector2.zero;
        pBgRt.sizeDelta = new Vector2(0, 4f);
        Image pBgImg = pBgObj.GetComponent<Image>();
        pBgImg.color = new Color(0, 0, 0, 0.6f);
        pBgImg.raycastTarget = false;

        // MiniProgressFill
        Transform pFillTr = pBgObj.transform.Find("MiniProgressFill");
        GameObject pFillObj = pFillTr != null ? pFillTr.gameObject : null;
        if (pFillObj == null)
        {
            pFillObj = new GameObject("MiniProgressFill", typeof(RectTransform), typeof(Image));
            pFillObj.transform.SetParent(pBgObj.transform, false);
        }
        questProgressFill = pFillObj.GetComponent<Image>();
        questProgressFill.color = new Color(0.2f, 0.85f, 0.35f, 1f);
        questProgressFill.raycastTarget = false;
        RectTransform pFillRt = pFillObj.GetComponent<RectTransform>();
        pFillRt.anchorMin = Vector2.zero;
        pFillRt.anchorMax = new Vector2(0.2f, 1f);
        pFillRt.offsetMin = Vector2.zero;
        pFillRt.offsetMax = Vector2.zero;

        // DÒNG 1: Tên Nhiệm Vụ (TxtQuestTitle)
        Transform txtTr = questBarObj.transform.Find("TxtQuestTitle");
        GameObject txtObj = txtTr != null ? txtTr.gameObject : null;
        if (txtObj == null)
        {
            txtObj = new GameObject("TxtQuestTitle", typeof(RectTransform), typeof(Text), typeof(Outline));
            txtObj.transform.SetParent(questBarObj.transform, false);
        }
        txtQuestTitle = txtObj.GetComponent<Text>();
        txtQuestTitle.font = defaultFont;
        txtQuestTitle.fontSize = 13;
        txtQuestTitle.fontStyle = FontStyle.Bold;
        txtQuestTitle.alignment = TextAnchor.MiddleLeft;
        txtQuestTitle.color = Color.white;
        txtQuestTitle.text = "🎯 [1/5] Để Phi Kiếm diệt 3 Yêu Thú (0/3)";
        txtQuestTitle.raycastTarget = false;
        Outline tOl = txtObj.GetComponent<Outline>();
        if (tOl == null) tOl = txtObj.AddComponent<Outline>();
        tOl.effectColor = Color.black;
        tOl.effectDistance = new Vector2(1f, -1f);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0f, 0.48f);
        txtRt.anchorMax = new Vector2(1f, 1f);
        txtRt.pivot = new Vector2(0f, 0.5f);
        txtRt.offsetMin = new Vector2(12f, 0f);
        txtRt.offsetMax = new Vector2(-105f, -2f);

        // DÒNG 2: Scroll View Mô Tả & Phần Thưởng (QuestDescScrollView)
        Transform sTr = questBarObj.transform.Find("QuestDescScrollView");
        GameObject scrollObj = sTr != null ? sTr.gameObject : null;
        if (scrollObj == null)
        {
            scrollObj = new GameObject("QuestDescScrollView", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
            scrollObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform sRt = scrollObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f);
        sRt.anchorMax = new Vector2(1f, 0.48f);
        sRt.pivot = new Vector2(0f, 0.5f);
        sRt.offsetMin = new Vector2(12f, 4f);
        sRt.offsetMax = new Vector2(-105f, 0f);

        descScrollRect = scrollObj.GetComponent<ScrollRect>();
        descScrollRect.horizontal = true;
        descScrollRect.vertical = false;
        descScrollRect.movementType = ScrollRect.MovementType.Clamped;
        descScrollRect.inertia = true;
        descScrollRect.scrollSensitivity = 15f;

        // TxtQuestReward nằm trong Scroll View
        Transform rTr = scrollObj.transform.Find("TxtQuestReward");
        if (rTr == null) rTr = questBarObj.transform.Find("TxtQuestReward");
        GameObject rObj = rTr != null ? rTr.gameObject : null;
        if (rObj == null)
        {
            rObj = new GameObject("TxtQuestReward", typeof(RectTransform), typeof(Text), typeof(Outline));
        }
        rObj.transform.SetParent(scrollObj.transform, false);

        txtQuestReward = rObj.GetComponent<Text>();
        txtQuestReward.font = defaultFont;
        txtQuestReward.fontSize = 11;
        txtQuestReward.alignment = TextAnchor.MiddleLeft;
        txtQuestReward.color = new Color(1f, 0.88f, 0.35f, 1f);
        txtQuestReward.text = "🎁 Thưởng: +100 Linh Thạch";
        txtQuestReward.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtQuestReward.verticalOverflow = VerticalWrapMode.Truncate;
        txtQuestReward.raycastTarget = false;

        Outline rOl = rObj.GetComponent<Outline>();
        if (rOl == null) rOl = rObj.AddComponent<Outline>();
        rOl.effectColor = Color.black;
        rOl.effectDistance = new Vector2(1f, -1f);

        RectTransform rRt = rObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(0f, 1f);
        rRt.pivot = new Vector2(0f, 0.5f);
        rRt.anchoredPosition = Vector2.zero;
        rRt.sizeDelta = new Vector2(700f, 0f);
        descScrollRect.content = rRt;

        // NÚT HÀNH ĐỘNG NHANH [NHẬN] / [📜 MỐC] (BtnClaimQuick)
        Transform baTr = questBarObj.transform.Find("BtnClaimQuick");
        GameObject btnActionObj = baTr != null ? baTr.gameObject : null;
        if (btnActionObj == null)
        {
            btnActionObj = new GameObject("BtnClaimQuick", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
            btnActionObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform baRt = btnActionObj.GetComponent<RectTransform>();
        baRt.anchorMin = new Vector2(1f, 0.5f);
        baRt.anchorMax = new Vector2(1f, 0.5f);
        baRt.pivot = new Vector2(1f, 0.5f);
        baRt.anchoredPosition = new Vector2(-28f, 0f);
        baRt.sizeDelta = new Vector2(74f, 34f);

        Image baImg = btnActionObj.GetComponent<Image>();
        baImg.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
        Outline baOutline = btnActionObj.GetComponent<Outline>();
        baOutline.effectColor = Color.black;
        baOutline.effectDistance = new Vector2(1f, -1f);

        Transform batTr = btnActionObj.transform.Find("Text");
        GameObject baTxtObj = batTr != null ? batTr.gameObject : null;
        if (baTxtObj == null)
        {
            baTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            baTxtObj.transform.SetParent(btnActionObj.transform, false);
        }
        txtClaimQuick = baTxtObj.GetComponent<Text>();
        txtClaimQuick.font = defaultFont;
        txtClaimQuick.fontSize = 12;
        txtClaimQuick.fontStyle = FontStyle.Bold;
        txtClaimQuick.alignment = TextAnchor.MiddleCenter;
        txtClaimQuick.text = "📜 Mốc";
        txtClaimQuick.color = Color.white;
        RectTransform batRt = baTxtObj.GetComponent<RectTransform>();
        batRt.anchorMin = Vector2.zero;
        batRt.anchorMax = Vector2.one;
        batRt.offsetMin = Vector2.zero;
        batRt.offsetMax = Vector2.zero;

        btnClaimQuick = btnActionObj.GetComponent<Button>();

        // Ẩn nút BtnMilestone cũ nếu có
        Transform oldBtnM = questBarObj.transform.Find("BtnMilestone");
        if (oldBtnM != null) oldBtnM.gameObject.SetActive(false);

        // Nút Thu Nhỏ/Mở Rộng [-] (BtnToggle)
        Transform btTr = questBarObj.transform.Find("BtnToggle");
        GameObject btnToggleObj = btTr != null ? btTr.gameObject : null;
        if (btnToggleObj == null)
        {
            btnToggleObj = new GameObject("BtnToggle", typeof(RectTransform), typeof(Button), typeof(Text));
            btnToggleObj.transform.SetParent(questBarObj.transform, false);
        }
        RectTransform btRt = btnToggleObj.GetComponent<RectTransform>();
        btRt.anchorMin = new Vector2(1, 0.5f);
        btRt.anchorMax = new Vector2(1, 0.5f);
        btRt.pivot = new Vector2(1, 0.5f);
        btRt.anchoredPosition = new Vector2(-4f, 0f);
        btRt.sizeDelta = new Vector2(22f, 24f);

        Text tTxt = btnToggleObj.GetComponent<Text>();
        tTxt.font = defaultFont;
        tTxt.fontSize = 15;
        tTxt.text = "[-]";
        tTxt.color = Color.gray;
        tTxt.alignment = TextAnchor.MiddleCenter;

        // 4. TẠO HOẶC TÌM CON TRỎ MŨI TÊN (TutorialPointerArrow)
        Transform existingPointer = canvasObj.transform.Find("TutorialPointerArrow");
        GameObject pointerObj = existingPointer != null ? existingPointer.gameObject : null;
        RectTransform pointerRt = null;
        Text txtPointerHint = null;

        if (pointerObj == null)
        {
            pointerObj = new GameObject("TutorialPointerArrow", typeof(RectTransform), typeof(CanvasGroup));
            pointerObj.transform.SetParent(canvasObj.transform, false);

            CanvasGroup cg = pointerObj.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            pointerRt = pointerObj.GetComponent<RectTransform>();
            pointerRt.sizeDelta = new Vector2(80f, 80f);
            pointerRt.anchorMin = new Vector2(0f, 1f);
            pointerRt.anchorMax = new Vector2(0f, 1f);
            pointerRt.pivot = new Vector2(0.5f, 0.5f);
            pointerRt.anchoredPosition = new Vector2(200f, -110f); // Đặt ở vị trí nổi bật ngay dưới Quest Bar để dev căn chỉnh

            // Icon Mũi tên chỉ xuống ▼ (Unicode chuẩn 100% không lo lỗi font)
            GameObject arrowIconObj = new GameObject("ArrowIcon", typeof(RectTransform), typeof(Text), typeof(Outline));
            arrowIconObj.transform.SetParent(pointerObj.transform, false);
            Text arrowText = arrowIconObj.GetComponent<Text>();
            arrowText.font = defaultFont;
            arrowText.fontSize = 38;
            arrowText.fontStyle = FontStyle.Bold;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.text = "▼";
            arrowText.color = new Color(1f, 0.85f, 0.2f);
            arrowText.raycastTarget = false;
            Outline arrowOutline = arrowIconObj.GetComponent<Outline>();
            arrowOutline.effectColor = Color.black;
            arrowOutline.effectDistance = new Vector2(2f, -2f);
            RectTransform aRt = arrowIconObj.GetComponent<RectTransform>();
            aRt.anchoredPosition = new Vector2(0, -10f);
            aRt.sizeDelta = new Vector2(50f, 50f);

            GameObject hintObj = new GameObject("TxtHint", typeof(RectTransform), typeof(Text), typeof(Outline));
            hintObj.transform.SetParent(pointerObj.transform, false);
            txtPointerHint = hintObj.GetComponent<Text>();
            txtPointerHint.font = defaultFont;
            txtPointerHint.fontSize = 14;
            txtPointerHint.fontStyle = FontStyle.Bold;
            txtPointerHint.alignment = TextAnchor.MiddleCenter;
            txtPointerHint.text = "Bấm vào đây!";
            txtPointerHint.color = new Color(1f, 0.95f, 0.4f);
            txtPointerHint.raycastTarget = false;
            Outline hintOutline = hintObj.GetComponent<Outline>();
            hintOutline.effectColor = Color.black;
            hintOutline.effectDistance = new Vector2(1.5f, -1.5f);
            RectTransform hRt = hintObj.GetComponent<RectTransform>();
            hRt.anchoredPosition = new Vector2(0, 26f);
            hRt.sizeDelta = new Vector2(160f, 26f);

            // BẬT SẴN để người phát triển nhìn thấy và căn chỉnh trực quan trên Scene
            pointerObj.SetActive(true);

            Undo.RegisterCreatedObjectUndo(pointerObj, "Create Tutorial Pointer Arrow");
        }
        else
        {
            pointerObj.SetActive(true);
            pointerRt = pointerObj.GetComponent<RectTransform>();
            txtPointerHint = pointerObj.transform.Find("TxtHint")?.GetComponent<Text>();
            
            Text aTxt = pointerObj.transform.Find("ArrowIcon")?.GetComponent<Text>();
            if (aTxt != null && (string.IsNullOrEmpty(aTxt.text) || aTxt.text == "👇"))
            {
                aTxt.text = "▼";
                aTxt.fontStyle = FontStyle.Bold;
                aTxt.fontSize = 38;
            }
        }

        // 5. GẮN VÀ LIÊN KẾT REFERENCES VÀO TutorialManager
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
        {
            managers = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers Object");
        }

        TutorialManager tm = managers.GetComponent<TutorialManager>();
        if (tm == null) tm = managers.AddComponent<TutorialManager>();

        MilestoneManager mm = managers.GetComponent<MilestoneManager>();
        if (mm == null) mm = managers.AddComponent<MilestoneManager>();
        EditorUtility.SetDirty(mm);

        tm.questBarObj = questBarObj;
        tm.txtQuestTitle = txtQuestTitle;
        tm.txtQuestReward = txtQuestReward;
        tm.questBarBg = questBarBg;
        tm.questProgressFill = questProgressFill;
        tm.questBarOutline = questBarOutline;

        tm.btnClaimQuick = btnClaimQuick;
        tm.txtClaimQuick = txtClaimQuick;
        tm.descScrollRect = descScrollRect;

        tm.pointerObj = pointerObj;
        tm.pointerRt = pointerRt;
        tm.txtPointerHint = txtPointerHint;

        tm.tutorialOverlay = null;
        tm.tutorialText = null;

        EditorUtility.SetDirty(tm);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // 6. Highlight thanh Quest Bar để dev căn chỉnh ngay lập tức
        if (questBarObj != null)
        {
            questBarObj.SetActive(true);
            if (forceSelect)
            {
                Selection.activeGameObject = questBarObj;
                EditorGUIUtility.PingObject(questBarObj);
                Debug.Log("<color=green>🎯 [TutorialBuilderTool] Đã tạo sẵn 'TutorialTargetQuestBar' trên Canvas và liên kết vào TutorialManager thành công! Bạn có thể căn chỉnh trực tiếp trên Hierarchy/Scene View.</color>");
            }
        }
    }
}
#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HUDLayoutRefinementTool
{
    private static readonly Color ColBgDark = new Color(0.05f, 0.07f, 0.11f, 0.94f);
    private static readonly Color ColGoldBorder = new Color(0.96f, 0.78f, 0.26f, 0.75f);
    private static readonly Color ColGoldText = new Color(0.96f, 0.82f, 0.35f, 1f);
    private static readonly Color ColHpFill = new Color(0.85f, 0.20f, 0.20f, 1f);
    private static readonly Color ColHpBg = new Color(0.08f, 0.03f, 0.04f, 0.92f);
    private static readonly Color ColRewardText = new Color(0.68f, 0.85f, 0.95f, 0.90f);

    [MenuItem("TuTien/UI/Apply Compact HUD Layout")]
    public static void ApplyCompactLayout()
    {
        var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("[HUDRefine] Không tìm thấy Canvas/MobileUIRoot!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(mobileRoot, "Apply Compact HUD Layout");

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 1. THANH TRẠNG THÁI (PlayerHPBar) - Thu gọn về góc trái dưới Avatar nhân vật
        Transform hpBarTr = mobileRoot.transform.Find("PlayerHPBar");
        if (hpBarTr != null)
        {
            hpBarTr.gameObject.SetActive(true);
            RectTransform rt = hpBarTr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(14f, -88f);
            rt.sizeDelta = new Vector2(250f, 20f);

            var bgImg = GetOrAdd<Image>(hpBarTr.gameObject);
            bgImg.color = ColHpBg;

            var outline = GetOrAdd<Outline>(hpBarTr.gameObject);
            outline.effectColor = new Color(0.35f, 0.40f, 0.50f, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);

            Transform bgChild = hpBarTr.Find("BG");
            if (bgChild != null)
            {
                var bgChildImg = bgChild.GetComponent<Image>();
                if (bgChildImg != null) bgChildImg.color = ColHpBg;

                Transform fillTr = bgChild.Find("Fill");
                if (fillTr != null)
                {
                    var fillImg = fillTr.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.color = ColHpFill;
                        fillImg.type = Image.Type.Filled;
                        fillImg.fillMethod = Image.FillMethod.Horizontal;
                    }
                }
            }

            Transform hpTxt = hpBarTr.Find("HPText");
            if (hpTxt != null)
            {
                var txt = hpTxt.GetComponent<Text>();
                if (txt != null)
                {
                    if (font != null) txt.font = font;
                    txt.fontSize = 12;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = Color.white;
                }
                RectTransform txtRt = hpTxt.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.pivot = new Vector2(0.5f, 0.5f);
                txtRt.anchoredPosition = Vector2.zero;
                txtRt.sizeDelta = Vector2.zero;

                var txtOutline = GetOrAdd<Outline>(hpTxt.gameObject);
                txtOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
                txtOutline.effectDistance = new Vector2(1f, -1f);
            }

            Debug.Log("✅ [HUDRefine] Đã thu gọn PlayerHPBar (250x20px) đặt trực tiếp dưới khung Avatar góc trái.");
        }

        // 2. THANH NHIỆM VỤ (TutorialTargetQuestBar) - Thu gọn thành Capsule Card nổi bên trái
        Transform questTr = mobileRoot.transform.Find("TutorialTargetQuestBar");
        if (questTr != null)
        {
            questTr.gameObject.SetActive(true);
            RectTransform rt = questTr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(14f, -114f);
            rt.sizeDelta = new Vector2(460f, 48f);

            var qImg = GetOrAdd<Image>(questTr.gameObject);
            qImg.color = ColBgDark;

            var qOutline = GetOrAdd<Outline>(questTr.gameObject);
            qOutline.effectColor = ColGoldBorder;
            qOutline.effectDistance = new Vector2(1f, -1f);

            // Mini Progress Underline
            Transform pBgTr = questTr.Find("MiniProgressBG");
            if (pBgTr != null)
            {
                RectTransform pBgRt = pBgTr.GetComponent<RectTransform>();
                pBgRt.anchorMin = new Vector2(0f, 0f);
                pBgRt.anchorMax = new Vector2(1f, 0f);
                pBgRt.pivot = new Vector2(0f, 0f);
                pBgRt.anchoredPosition = Vector2.zero;
                pBgRt.sizeDelta = new Vector2(0f, 3f);
            }

            // Tiêu đề nhiệm vụ
            Transform qTitle = questTr.Find("TxtQuestTitle");
            if (qTitle != null)
            {
                var txt = qTitle.GetComponent<Text>();
                if (txt != null)
                {
                    if (font != null) txt.font = font;
                    txt.fontSize = 13;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleLeft;
                    txt.color = ColGoldText;
                }
                RectTransform titleRt = qTitle.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0f, 0.45f);
                titleRt.anchorMax = new Vector2(1f, 1f);
                titleRt.pivot = new Vector2(0f, 0.5f);
                titleRt.anchoredPosition = new Vector2(12f, -2f);
                titleRt.sizeDelta = new Vector2(-135f, 0f);
            }

            // Phần thưởng nhiệm vụ
            Transform scrollTr = questTr.Find("QuestDescScrollView");
            if (scrollTr != null)
            {
                RectTransform sRt = scrollTr.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0f, 0f);
                sRt.anchorMax = new Vector2(1f, 0.45f);
                sRt.pivot = new Vector2(0f, 0.5f);
                sRt.anchoredPosition = new Vector2(12f, 2f);
                sRt.sizeDelta = new Vector2(-135f, 0f);

                Transform rTxtTr = scrollTr.Find("TxtQuestReward");
                if (rTxtTr != null)
                {
                    var rTxt = rTxtTr.GetComponent<Text>();
                    if (rTxt != null)
                    {
                        if (font != null) rTxt.font = font;
                        rTxt.fontSize = 11;
                        rTxt.fontStyle = FontStyle.Normal;
                        rTxt.alignment = TextAnchor.MiddleLeft;
                        rTxt.color = ColRewardText;
                    }
                }
            }

            // Nút nhận thưởng nhanh (BtnClaimQuick)
            Transform claimBtnTr = questTr.Find("BtnClaimQuick");
            if (claimBtnTr != null)
            {
                RectTransform btnRt = claimBtnTr.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(1f, 0.5f);
                btnRt.anchorMax = new Vector2(1f, 0.5f);
                btnRt.pivot = new Vector2(1f, 0.5f);
                btnRt.anchoredPosition = new Vector2(-8f, 0f);
                btnRt.sizeDelta = new Vector2(76f, 32f);

                var btnImg = claimBtnTr.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(0.95f, 0.65f, 0.15f, 1f);

                Transform btnTxt = claimBtnTr.Find("Text");
                if (btnTxt != null)
                {
                    var t = btnTxt.GetComponent<Text>();
                    if (t != null)
                    {
                        if (font != null) t.font = font;
                        t.fontSize = 12;
                        t.fontStyle = FontStyle.Bold;
                        t.color = new Color(0.12f, 0.08f, 0.02f, 1f);
                    }
                }
            }

            // Nút Thành Tựu [🏆] (BtnAchievement)
            Transform achBtnTr = questTr.Find("BtnAchievement");
            if (achBtnTr == null)
            {
                var achObj = new GameObject("BtnAchievement", typeof(RectTransform), typeof(Button), typeof(Image), typeof(Outline));
                achObj.transform.SetParent(questTr, false);
                achBtnTr = achObj.transform;

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                txtObj.transform.SetParent(achBtnTr, false);
                var tt = txtObj.GetComponent<RectTransform>();
                tt.anchorMin = Vector2.zero;
                tt.anchorMax = Vector2.one;
                tt.sizeDelta = Vector2.zero;

                var t = txtObj.GetComponent<Text>();
                if (font != null) t.font = font;
                t.fontSize = 16;
                t.alignment = TextAnchor.MiddleCenter;
                t.text = "🏆";
            }
            if (achBtnTr != null)
            {
                RectTransform aRt = achBtnTr.GetComponent<RectTransform>();
                aRt.anchorMin = new Vector2(1f, 0.5f);
                aRt.anchorMax = new Vector2(1f, 0.5f);
                aRt.pivot = new Vector2(1f, 0.5f);
                aRt.anchoredPosition = new Vector2(-88f, 0f);
                aRt.sizeDelta = new Vector2(34f, 32f);

                var aImg = achBtnTr.GetComponent<Image>();
                if (aImg != null) aImg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

                var aOl = achBtnTr.GetComponent<Outline>();
                if (aOl != null)
                {
                    aOl.effectColor = ColGoldBorder;
                    aOl.effectDistance = new Vector2(1f, -1f);
                }

                var btn = achBtnTr.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => AchievementModalUI.Open());
                }
            }

            // Nút thu nhỏ / phóng to (BtnToggle)
            Transform toggleBtnTr = questTr.Find("BtnToggle");
            if (toggleBtnTr != null)
            {
                RectTransform tRt = toggleBtnTr.GetComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0f, 0.5f);
                tRt.anchorMax = new Vector2(0f, 0.5f);
                tRt.pivot = new Vector2(0f, 0.5f);
                tRt.anchoredPosition = new Vector2(-28f, 0f); // Nằm sát mép ngoài nếu cần
                tRt.sizeDelta = new Vector2(24f, 24f);
            }

            Debug.Log("✅ [HUDRefine] Đã tái cấu trúc TutorialTargetQuestBar thành Capsule Card (460x48px) nổi gọn gàng bên trái.");
        }

        // 3. THANH ẢI (StageUI) - Căn giữa thon gọn, không che chắn
        Transform stageTr = mobileRoot.transform.Find("StageUI");
        if (stageTr != null)
        {
            stageTr.gameObject.SetActive(true);
            RectTransform rt = stageTr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -94f);
            rt.sizeDelta = new Vector2(400f, 32f);

            Transform sTxt = stageTr.Find("StageText");
            if (sTxt != null)
            {
                var txt = sTxt.GetComponent<Text>();
                if (txt != null)
                {
                    if (font != null) txt.font = font;
                    txt.fontSize = 13;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = ColGoldText;
                }
            }

            Transform pBg = stageTr.Find("ProgressBG");
            if (pBg != null)
            {
                RectTransform pRt = pBg.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.10f, 0.08f);
                pRt.anchorMax = new Vector2(0.90f, 0.28f);
                pRt.anchoredPosition = Vector2.zero;
                pRt.sizeDelta = Vector2.zero;
            }

            Debug.Log("✅ [HUDRefine] Đã tinh chỉnh StageUI thành Badge trung tâm (400x32px).");
        }

        // 4. ACTIVE BUFF PANEL - Đặt gọn dưới thanh nhiệm vụ
        Transform buffTr = mobileRoot.transform.Find("ActiveBuffPanel");
        if (buffTr != null)
        {
            RectTransform rt = buffTr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(14f, -168f);
            rt.sizeDelta = new Vector2(360f, 24f);
            Debug.Log("✅ [HUDRefine] Đã dời ActiveBuffPanel (360x24px) gọn gàng bên trái.");
        }

        Canvas.ForceUpdateCanvases();
        EditorUtility.SetDirty(mobileRoot);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mobileRoot.scene);
        Debug.Log("🎉 [HUDRefine] HOÀN TẤT: Thanh nhiệm vụ & thanh trạng thái đã được sắp xếp gọn gàng, mở rộng tối đa không gian chiến trường!");
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}
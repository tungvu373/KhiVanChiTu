#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tool tinh chỉnh và nâng cấp toàn diện In-Game UI theo thiết kế Xianxia Mobile UI chuẩn Figma.
/// - Đồng bộ màu sắc Dark Xianxia: Midnight (#0B0F18), Gold (#F5C842, #8C7335), Jade (#2ECC71), Spirit Cyan (#00D2D3).
/// - Top HUD tối ưu 1 tay: Character Avatar với Level Badge, Realm Title, Wallet Pills, Stage Pill, Health Bar.
/// - 5 Function Drawers: Kiếm Lô, Luyện Thể, Luyện Đan (Đại tu giao diện Bát Quái Lò & Rate bar), Pháp Khí, Trận Pháp.
/// - Mobile Dock: 5 Tabs chuẩn responsive với active gold highlight.
/// - Combat Arena: 85% chiều cao mở rộng khi đóng Drawer, CombatLogPanel bán trong suốt.
/// </summary>
public static class InGameUIRefinementTool
{
    // BẢNG MÀU CHUẨN XIANXIA MOBILE TỪ FIGMA
    public static readonly Color ColMidnight = new Color(0.043f, 0.059f, 0.094f, 0.98f);     // #0B0F18
    public static readonly Color ColPanelBg = new Color(0.055f, 0.078f, 0.130f, 0.98f);      // #0E1421
    public static readonly Color ColCardBg = new Color(0.071f, 0.102f, 0.165f, 0.96f);       // #121A2A
    public static readonly Color ColPillBg = new Color(0.055f, 0.082f, 0.137f, 0.92f);
    public static readonly Color ColGoldPrimary = new Color(0.961f, 0.784f, 0.259f, 1f);     // #F5C842
    public static readonly Color ColGoldBorder = new Color(0.549f, 0.451f, 0.208f, 0.85f);    // #8C7335
    public static readonly Color ColGoldMuted = new Color(0.72f, 0.58f, 0.28f, 1f);
    public static readonly Color ColJadeGreen = new Color(0.180f, 0.800f, 0.443f, 1f);      // #2ECC71
    public static readonly Color ColSpiritCyan = new Color(0.000f, 0.824f, 0.827f, 1f);     // #00D2D3
    public static readonly Color ColCrimsonRed = new Color(0.906f, 0.298f, 0.235f, 1f);     // #E74C3C
    public static readonly Color ColMagicPurple = new Color(0.608f, 0.349f, 0.714f, 1f);    // #9B59B6
    public static readonly Color ColTextLight = new Color(0.94f, 0.95f, 0.97f, 1f);
    public static readonly Color ColTextMuted = new Color(0.54f, 0.60f, 0.68f, 1f);

    [MenuItem("TuTien/UI/Apply In-Game UI Refinement")]
    public static void ApplyFullRefinement()
    {
        Debug.Log("=================================================================");
        Debug.Log(">>> BẮT ĐẦU TINH CHỈNH GIAO DIỆN IN-GAME THEO CHUẨN FIGMA MOBILE <<<");
        Debug.Log("=================================================================");

        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[UIRefine] Không tìm thấy Canvas trong Scene hiện tại!");
            return;
        }

        Transform mobileRootTr = canvas.transform.Find("MobileUIRoot");
        if (mobileRootTr == null)
        {
            Debug.LogError("[UIRefine] Không tìm thấy Canvas/MobileUIRoot!");
            return;
        }
        RectTransform mobileRoot = mobileRootTr as RectTransform;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Tắt tất cả modal/overlay để màn hình gameplay thông thoáng 85%
        DismissAllModals(canvas, mobileRoot);

        // 1. Tinh chỉnh Top HUD
        RefineTopHUD(mobileRoot, font);

        // 2. Tinh chỉnh Combat Log & Arena
        RefineCombatFeedback(mobileRoot, font);

        // 3. Tinh chỉnh Bottom MobileDock
        RefineMobileDock(mobileRoot, font);

        // 4. Tinh chỉnh 5 Drawer theo thiết kế Figma
        RefineDrawer0_KiemLo(mobileRoot, font);
        RefineDrawer1_LuyenThe(mobileRoot, font);
        RefineDrawer2_LuyenDan(mobileRoot, font);
        RefineDrawer3_PhapKhi(mobileRoot, font);
        RefineDrawer4_TranPhap(mobileRoot, font);

        // 5. Cập nhật backdrop overlay
        RefineBackdrop(mobileRoot);

        // 6. Đồng bộ Drawer Controller
        var controller = mobileRoot.GetComponent<MobileDrawerUIController>();
        if (controller != null)
        {
            controller.UpdateDockVisuals();
            EditorUtility.SetDirty(controller);
        }

        // Lưu scene
        EditorSceneManager.MarkSceneDirty(canvas.scene);
        EditorSceneManager.SaveScene(canvas.scene);
        Debug.Log("🎉 [UIRefine] HOÀN TẤT TINH CHỈNH GIAO DIỆN IN-GAME MOBILE & ĐÃ LƯU SCENE!");
    }

    private static void DismissAllModals(GameObject canvas, RectTransform mobileRoot)
    {
        string[] modalNames = {
            "MilestoneModalUI", "CombatNotificationBanner", "TribulationModalUI",
            "PauseMenuModal", "CharacterDetailModal", "OfflineRewardPopupUI", "ShopPanel", "TutorialOverlay", "RightPanel"
        };
        foreach (var mName in modalNames)
        {
            Transform t = canvas.transform.Find(mName);
            if (t == null) t = mobileRoot.Find(mName);
            if (t != null)
            {
                t.gameObject.SetActive(false);
            }
        }

        Transform bodyModal = mobileRoot.Find("BodyTalentDetailModal");
        if (bodyModal != null) bodyModal.gameObject.SetActive(false);
    }

    private static void RefineTopHUD(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 1. Tinh chỉnh Top HUD theo Figma chuẩn...");

        // 1. TopBar (Thanh đỉnh bao gồm Linh Thạch, Tiên Ngọc)
        Transform topBarTr = mobileRoot.Find("TopBar");
        if (topBarTr != null)
        {
            RectTransform rt = topBarTr as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -6f);
            rt.sizeDelta = new Vector2(0f, 84f);

            var bg = GetOrAdd<Image>(topBarTr.gameObject);
            bg.color = ColMidnight;

            var outline = GetOrAdd<Outline>(topBarTr.gameObject);
            outline.effectColor = ColGoldBorder;
            outline.effectDistance = new Vector2(0f, -1.5f);

            // Linh Thach Pill (Đá Linh Thạch)
            Transform ltTr = topBarTr.Find("txt_LinhThach");
            if (ltTr != null)
            {
                Transform oldImg = ltTr.Find("IMG");
                if (oldImg != null) oldImg.gameObject.SetActive(false);

                // Tạo/Cập nhật BG_LinhThach nằm trước txt_LinhThach để làm khung bo tròn
                Transform bgLt = topBarTr.Find("BG_LinhThach");
                if (bgLt == null)
                {
                    GameObject bgGO = new GameObject("BG_LinhThach", typeof(RectTransform));
                    bgGO.transform.SetParent(topBarTr, false);
                    bgLt = bgGO.transform;
                }
                bgLt.SetSiblingIndex(ltTr.GetSiblingIndex());

                RectTransform bgRt = bgLt as RectTransform;
                bgRt.anchorMin = new Vector2(0.24f, 0.16f);
                bgRt.anchorMax = new Vector2(0.53f, 0.84f);
                bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

                var bgImg = GetOrAdd<Image>(bgLt.gameObject);
                if (bgImg != null)
                {
                    bgImg.sprite = null;
                    bgImg.color = ColPillBg;
                }
                var pillOutline = GetOrAdd<Outline>(bgLt.gameObject);
                if (pillOutline != null)
                {
                    pillOutline.effectColor = new Color(0f, 0.82f, 0.83f, 0.45f);
                    pillOutline.effectDistance = new Vector2(1f, -1f);
                }

                RectTransform ltRt = ltTr as RectTransform;
                ltRt.anchorMin = new Vector2(0.24f, 0.16f);
                ltRt.anchorMax = new Vector2(0.53f, 0.84f);
                ltRt.offsetMin = Vector2.zero; ltRt.offsetMax = Vector2.zero;

                var txt = ltTr.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 20;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = ColSpiritCyan;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.text = "💎 5.00K";
                    EditorUtility.SetDirty(txt);
                }
            }

            // Co Duyen / Tien Ngoc Pill (Ngọc Duyên)
            Transform cdTr = topBarTr.Find("txt_CoDuyen");
            if (cdTr != null)
            {
                Transform oldImg = cdTr.Find("IMG");
                if (oldImg != null) oldImg.gameObject.SetActive(false);

                // Tạo/Cập nhật BG_CoDuyen nằm trước txt_CoDuyen
                Transform bgCd = topBarTr.Find("BG_CoDuyen");
                if (bgCd == null)
                {
                    GameObject bgGO = new GameObject("BG_CoDuyen", typeof(RectTransform));
                    bgGO.transform.SetParent(topBarTr, false);
                    bgCd = bgGO.transform;
                }
                bgCd.SetSiblingIndex(cdTr.GetSiblingIndex());

                RectTransform bgRt = bgCd as RectTransform;
                bgRt.anchorMin = new Vector2(0.55f, 0.16f);
                bgRt.anchorMax = new Vector2(0.84f, 0.84f);
                bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

                var bgImg = GetOrAdd<Image>(bgCd.gameObject);
                if (bgImg != null)
                {
                    bgImg.sprite = null;
                    bgImg.color = ColPillBg;
                }
                var pillOutline = GetOrAdd<Outline>(bgCd.gameObject);
                if (pillOutline != null)
                {
                    pillOutline.effectColor = new Color(0.61f, 0.35f, 0.71f, 0.45f);
                    pillOutline.effectDistance = new Vector2(1f, -1f);
                }

                RectTransform cdRt = cdTr as RectTransform;
                cdRt.anchorMin = new Vector2(0.55f, 0.16f);
                cdRt.anchorMax = new Vector2(0.84f, 0.84f);
                cdRt.offsetMin = Vector2.zero; cdRt.offsetMax = Vector2.zero;

                var txt = cdTr.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 20;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = ColGoldPrimary;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.text = "🔮 100";
                    EditorUtility.SetDirty(txt);
                }
            }
        }

        // 2. CharacterFrame (Avatar & Cảnh Giới bên trái)
        Transform charFrameTr = mobileRoot.Find("CharacterFrame");
        if (charFrameTr != null)
        {
            RectTransform rt = charFrameTr as RectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -10f);
            rt.sizeDelta = new Vector2(210f, 74f);

            var img = GetOrAdd<Image>(charFrameTr.gameObject);
            img.color = new Color(0.08f, 0.11f, 0.17f, 0.90f);

            var outline = GetOrAdd<Outline>(charFrameTr.gameObject);
            outline.effectColor = ColGoldPrimary;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Transform avatarTr = charFrameTr.Find("Avatar");
            if (avatarTr != null)
            {
                RectTransform aRt = avatarTr as RectTransform;
                aRt.anchorMin = new Vector2(0f, 0.5f);
                aRt.anchorMax = new Vector2(0f, 0.5f);
                aRt.pivot = new Vector2(0f, 0.5f);
                aRt.anchoredPosition = new Vector2(6f, 0f);
                aRt.sizeDelta = new Vector2(60f, 60f);

                var aOutline = GetOrAdd<Outline>(avatarTr.gameObject);
                aOutline.effectColor = ColGoldPrimary;
                aOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            Transform infoTr = charFrameTr.Find("InfoText");
            if (infoTr != null)
            {
                RectTransform iRt = infoTr as RectTransform;
                iRt.anchorMin = new Vector2(0f, 0f);
                iRt.anchorMax = new Vector2(1f, 1f);
                iRt.pivot = new Vector2(0f, 0.5f);
                iRt.anchoredPosition = new Vector2(72f, 0f);
                iRt.sizeDelta = new Vector2(-76f, 0f);

                var txt = infoTr.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 14;
                    txt.lineSpacing = 1.1f;
                    txt.color = ColTextLight;
                    txt.alignment = TextAnchor.MiddleLeft;
                    txt.text = "<b><color=#F5C842>Luyện Khí · Sơ Kỳ</color></b>\n<color=#94A3B8>Lực Chiến: 1.25K</color>";
                }
            }

            Transform hintTr = charFrameTr.Find("DetailHintText");
            if (hintTr != null) hintTr.gameObject.SetActive(false);
        }

        // 3. PauseButton (Góc phải)
        Transform pauseBtnTr = mobileRoot.Find("PauseButton");
        if (pauseBtnTr != null)
        {
            RectTransform rt = pauseBtnTr as RectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-12f, -14f);
            rt.sizeDelta = new Vector2(66f, 66f);

            var img = GetOrAdd<Image>(pauseBtnTr.gameObject);
            img.color = ColCardBg;

            var outline = GetOrAdd<Outline>(pauseBtnTr.gameObject);
            outline.effectColor = ColGoldBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var txt = pauseBtnTr.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.font = font;
                txt.fontSize = 24;
                txt.color = ColGoldPrimary;
                txt.text = "⚙️";
                txt.alignment = TextAnchor.MiddleCenter;
            }
        }

        // 4. StageUI (Căn giữa - Tầng Đánh Quái)
        Transform stageUITr = mobileRoot.Find("StageUI");
        if (stageUITr != null)
        {
            stageUITr.gameObject.SetActive(true);
            RectTransform rt = stageUITr as RectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -96f);
            rt.sizeDelta = new Vector2(560f, 36f);

            var img = GetOrAdd<Image>(stageUITr.gameObject);
            img.color = new Color(0.04f, 0.06f, 0.10f, 0.90f);

            var outline = GetOrAdd<Outline>(stageUITr.gameObject);
            outline.effectColor = ColGoldBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Transform txtTr = stageUITr.Find("StageText");
            if (txtTr != null)
            {
                RectTransform tRt = txtTr as RectTransform;
                tRt.anchorMin = new Vector2(0f, 0.35f);
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

                var txt = txtTr.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 17;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = ColGoldPrimary;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.text = "⚔️ TẦNG 1: SƠ NHẬP TIÊN LỘ [ 0 / 10 ]";
                }
            }

            Transform pBg = stageUITr.Find("ProgressBG");
            if (pBg != null)
            {
                RectTransform pRt = pBg as RectTransform;
                pRt.anchorMin = new Vector2(0.12f, 0.08f);
                pRt.anchorMax = new Vector2(0.88f, 0.32f);
                pRt.offsetMin = Vector2.zero; pRt.offsetMax = Vector2.zero;

                var bgImg = GetOrAdd<Image>(pBg.gameObject);
                bgImg.color = new Color(0.02f, 0.03f, 0.06f, 0.95f);

                Transform fillTr = pBg.Find("ProgressFill");
                if (fillTr != null)
                {
                    var fillImg = fillTr.GetComponent<Image>();
                    if (fillImg != null) fillImg.color = ColGoldPrimary;
                }
            }

            Transform timerTr = stageUITr.Find("BossTimer");
            if (timerTr != null)
            {
                timerTr.gameObject.SetActive(false); // Chỉ bật khi vào đấu Boss
            }
        }

        // 5. PlayerHPBar (Thanh máu thanh mảnh đỏ thẫm)
        Transform hpBarTr = mobileRoot.Find("PlayerHPBar");
        if (hpBarTr != null)
        {
            hpBarTr.gameObject.SetActive(true);
            RectTransform rt = hpBarTr as RectTransform;
            rt.anchorMin = new Vector2(0.08f, 1f);
            rt.anchorMax = new Vector2(0.92f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -140f);
            rt.sizeDelta = new Vector2(0f, 14f);

            var bg = GetOrAdd<Image>(hpBarTr.gameObject);
            bg.color = new Color(0.03f, 0.04f, 0.07f, 0.90f);

            var outline = GetOrAdd<Outline>(hpBarTr.gameObject);
            outline.effectColor = new Color(0.4f, 0.45f, 0.55f, 0.5f);
            outline.effectDistance = new Vector2(1f, -1f);

            // Tìm Fill bên trong BG/Fill
            Transform bgTr = hpBarTr.Find("BG");
            Transform fillTr = bgTr != null ? bgTr.Find("Fill") : hpBarTr.Find("Fill");
            if (fillTr != null)
            {
                var fillImg = fillTr.GetComponent<Image>();
                if (fillImg != null)
                {
                    fillImg.color = ColCrimsonRed;
                    EditorUtility.SetDirty(fillImg);
                }
            }

            Transform hpTxt = hpBarTr.Find("HPText");
            if (hpTxt != null)
            {
                var txt = hpTxt.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 11;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = Color.white;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.text = "HP 100 / 100";
                    EditorUtility.SetDirty(txt);
                }
            }
        }

        // Tắt PlayerManaBar không dùng
        Transform manaBarTr = mobileRoot.Find("PlayerManaBar");
        if (manaBarTr != null)
        {
            manaBarTr.gameObject.SetActive(false);
            EditorUtility.SetDirty(manaBarTr.gameObject);
        }

        // Thanh tu vi theo kỳ có nội dung riêng, không ẩn cùng StagePanel legacy.
        Transform stagePanelTr = mobileRoot.Find("StagePanel");
        if (stagePanelTr != null)
        {
            bool hasPhaseBar = stagePanelTr.GetComponent<CultivationPhaseBarUI>() != null;
            stagePanelTr.gameObject.SetActive(hasPhaseBar);
            if (hasPhaseBar && stagePanelTr is RectTransform phaseRect)
                phaseRect.anchoredPosition = new Vector2(0f, -138f);
            EditorUtility.SetDirty(stagePanelTr.gameObject);
        }

        // Tắt BossTimer mặc định (chỉ mở khi đấu Boss)
        Transform stageUITrCheck = mobileRoot.Find("StageUI");
        if (stageUITrCheck != null)
        {
            Transform bTimer = stageUITrCheck.Find("BossTimer");
            if (bTimer != null)
            {
                bTimer.gameObject.SetActive(false);
                EditorUtility.SetDirty(bTimer.gameObject);
            }
        }

        // 6. TutorialTargetQuestBar (Nhiệm vụ tân thủ)
        Transform questTr = mobileRoot.Find("TutorialTargetQuestBar");
        if (questTr != null)
        {
            questTr.gameObject.SetActive(true);
            RectTransform rt = questTr as RectTransform;
            rt.anchorMin = new Vector2(0.06f, 1f);
            rt.anchorMax = new Vector2(0.94f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -162f);
            rt.sizeDelta = new Vector2(0f, 40f);

            var img = GetOrAdd<Image>(questTr.gameObject);
            img.color = new Color(0.07f, 0.10f, 0.16f, 0.90f);

            var outline = GetOrAdd<Outline>(questTr.gameObject);
            outline.effectColor = ColGoldBorder;
            outline.effectDistance = new Vector2(1f, -1f);

            Transform qTitle = questTr.Find("TxtQuestTitle");
            if (qTitle != null)
            {
                var txt = qTitle.GetComponent<Text>();
                if (txt != null)
                {
                    txt.font = font;
                    txt.fontSize = 15;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = ColGoldPrimary;
                }
            }
        }

        // 7. ActiveBuffPanel (Hạ thấp)
        Transform buffPanel = mobileRoot.Find("ActiveBuffPanel");
        if (buffPanel != null)
        {
            RectTransform rt = buffPanel as RectTransform;
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -210f);
            rt.sizeDelta = new Vector2(0f, 26f);
        }
    }

    private static void RefineCombatFeedback(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 2. Tinh chỉnh Combat Log & Arena Feedback...");
        Transform combatLogTr = mobileRoot.Find("CombatLogPanel");
        if (combatLogTr != null)
        {
            combatLogTr.gameObject.SetActive(true);
            RectTransform rt = combatLogTr as RectTransform;
            rt.anchorMin = new Vector2(0.05f, 0f);
            rt.anchorMax = new Vector2(0.95f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 195f);
            rt.sizeDelta = new Vector2(0f, 44f);

            var img = GetOrAdd<Image>(combatLogTr.gameObject);
            img.color = new Color(0.04f, 0.05f, 0.08f, 0.35f);
            img.raycastTarget = false;

            var outline = GetOrAdd<Outline>(combatLogTr.gameObject);
            outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
            outline.effectDistance = new Vector2(1f, -1f);

            foreach (var txt in combatLogTr.GetComponentsInChildren<Text>(true))
            {
                txt.font = font;
                txt.fontSize = 18;
                txt.color = ColTextLight;
                txt.raycastTarget = false;
            }
        }
    }

    private static void RefineMobileDock(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 3. Tinh chỉnh Mobile Dock & 5 Tabs...");
        Transform dockTr = mobileRoot.Find("MobileDock");
        if (dockTr == null) return;

        RectTransform dockRt = dockTr as RectTransform;
        dockRt.anchorMin = Vector2.zero;
        dockRt.anchorMax = new Vector2(1f, 0.095f);
        dockRt.offsetMin = Vector2.zero;
        dockRt.offsetMax = Vector2.zero;

        var dockImg = GetOrAdd<Image>(dockTr.gameObject);
        dockImg.color = ColMidnight;

        var topBorder = GetOrAdd<Outline>(dockTr.gameObject);
        topBorder.effectColor = ColGoldBorder;
        topBorder.effectDistance = new Vector2(0f, 1.5f);

        string[] tabLabels = {
            "⚔️\nKiếm Lô",
            "🛡️\nLuyện Thể",
            "⚗️\nLuyện Đan",
            "✨\nPháp Khí",
            "🌀\nTrận Pháp"
        };

        for (int i = 0; i < tabLabels.Length; i++)
        {
            Transform cellTr = dockTr.Find("Dock_" + i);
            if (cellTr == null) continue;

            RectTransform cellRt = cellTr as RectTransform;
            cellRt.anchorMin = new Vector2(i / 5f, 0f);
            cellRt.anchorMax = new Vector2((i + 1) / 5f, 1f);
            cellRt.offsetMin = Vector2.zero;
            cellRt.offsetMax = Vector2.zero;

            var cellImg = GetOrAdd<Image>(cellTr.gameObject);
            cellImg.color = new Color(0.07f, 0.10f, 0.16f, 0.95f);

            var txt = cellTr.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.font = font;
                txt.text = tabLabels[i];
                txt.fontSize = 20;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = ColTextMuted;
                txt.lineSpacing = 1.05f;
            }
        }
    }

    private static void StyleDrawerBase(Transform drawer, string titleText, Font font)
    {
        var img = GetOrAdd<Image>(drawer.gameObject);
        img.color = ColPanelBg;

        var outline = GetOrAdd<Outline>(drawer.gameObject);
        outline.effectColor = ColGoldBorder;
        outline.effectDistance = new Vector2(0f, 2f);

        Transform titleTr = drawer.Find("Title");
        if (titleTr != null)
        {
            var txt = titleTr.GetComponent<Text>();
            if (txt != null)
            {
                txt.font = font;
                txt.text = titleText;
                txt.fontSize = 24;
                txt.fontStyle = FontStyle.Bold;
                txt.color = ColGoldPrimary;
                txt.alignment = TextAnchor.MiddleLeft;
            }
        }

        Transform closeTr = drawer.Find("CloseButton");
        if (closeTr != null)
        {
            var cImg = GetOrAdd<Image>(closeTr.gameObject);
            cImg.color = new Color(0.20f, 0.25f, 0.33f, 0.95f);

            var cTxt = closeTr.GetComponentInChildren<Text>();
            if (cTxt != null)
            {
                cTxt.font = font;
                cTxt.text = "✕";
                cTxt.fontSize = 22;
                cTxt.fontStyle = FontStyle.Bold;
                cTxt.color = ColTextLight;
                cTxt.alignment = TextAnchor.MiddleCenter;
            }
        }

        Transform handleTr = drawer.Find("SwipeHandle");
        if (handleTr != null)
        {
            var hImg = GetOrAdd<Image>(handleTr.gameObject);
            hImg.color = new Color(0.55f, 0.62f, 0.72f, 0.50f);
        }
    }

    private static void RefineDrawer0_KiemLo(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 4a. Tinh chỉnh Drawer 0: Kiếm Lô...");
        Transform d0 = mobileRoot.Find("Drawer_0");
        if (d0 == null) return;
        StyleDrawerBase(d0, "⚔️ KIẾM LÔ (HỢP KIẾM)", font);

        Transform mergeTr = d0.Find("MergePanel");
        if (mergeTr == null) return;

        // EquipPanel (Ô trang bị kiếm trên đầu)
        Transform equipTr = mergeTr.Find("EquipPanel");
        if (equipTr != null)
        {
            var grid = equipTr.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = new Vector2(130f, 130f);
                grid.spacing = new Vector2(12f, 12f);
                grid.padding = new RectOffset(10, 10, 10, 10);
            }
            foreach (Transform slot in equipTr)
            {
                var sImg = slot.GetComponent<Image>();
                if (sImg != null) sImg.color = new Color(0.09f, 0.15f, 0.22f, 0.95f);
                var sOutline = GetOrAdd<Outline>(slot.gameObject);
                sOutline.effectColor = ColGoldBorder;
                sOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        // Thêm / Căn chỉnh Action & Pagination Controls ở dưới đáy Kiếm Lô
        Transform invContainer = mergeTr.Find("InventoryContainer");
        if (invContainer != null)
        {
            // 1. XÓA BỎ ActionButtonsBar nếu tồn tại vì nó đè lên PaginationPanel
            Transform oldActionBar = invContainer.Find("ActionButtonsBar");
            if (oldActionBar != null)
            {
                Object.DestroyImmediate(oldActionBar.gameObject);
                Debug.Log("[UIRefine] Đã xóa ActionButtonsBar cũ bị trùng lặp vị trí.");
            }

            // 2. Tinh chỉnh ScrollView để dành 15% đáy cho thanh điều khiển PaginationPanel
            Transform scrollTr = invContainer.Find("ScrollView");
            if (scrollTr != null)
            {
                RectTransform sRt = scrollTr as RectTransform;
                sRt.anchorMin = new Vector2(0f, 0.15f);
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = Vector2.zero;
                sRt.offsetMax = Vector2.zero;

                // Tinh chỉnh phong cách cho tất cả các ô trong 5 trang kho đồ
                Transform pagesContainer = scrollTr.Find("Viewport/Content/PagesContainer");
                if (pagesContainer != null)
                {
                    for (int p = 0; p < pagesContainer.childCount; p++)
                    {
                        Transform pageTr = pagesContainer.GetChild(p);
                        // Chỉ hiển thị trang đầu tiên mặc định
                        pageTr.gameObject.SetActive(p == 0);

                        foreach (Transform slot in pageTr)
                        {
                            var slotImg = slot.GetComponent<Image>();
                            if (slotImg != null) slotImg.color = new Color(0.09f, 0.13f, 0.20f, 0.90f);

                            var sOutline = GetOrAdd<Outline>(slot.gameObject);
                            sOutline.effectColor = new Color(0.85f, 0.65f, 0.13f, 0.35f);
                            sOutline.effectDistance = new Vector2(1f, -1f);
                        }
                    }
                }
            }

            // 3. Tinh chỉnh PaginationPanel tích hợp đầy đủ: Đúc Kiếm + Hợp Nhanh + Sắp Xếp + Lật Trang (◀ 1/5 ▶)
            Transform pagePanelTr = invContainer.Find("PaginationPanel");
            if (pagePanelTr == null)
            {
                GameObject ppGO = new GameObject("PaginationPanel", typeof(RectTransform));
                ppGO.transform.SetParent(invContainer, false);
                pagePanelTr = ppGO.transform;
            }

            RectTransform pRt = pagePanelTr as RectTransform;
            pRt.anchorMin = new Vector2(0f, 0f);
            pRt.anchorMax = new Vector2(1f, 0.14f);
            pRt.offsetMin = Vector2.zero;
            pRt.offsetMax = Vector2.zero;

            var ppImg = GetOrAdd<Image>(pagePanelTr.gameObject);
            ppImg.color = ColMidnight;

            var ppOutline = GetOrAdd<Outline>(pagePanelTr.gameObject);
            ppOutline.effectColor = ColGoldBorder;
            ppOutline.effectDistance = new Vector2(0f, 1.5f);

            var hlg = GetOrAdd<HorizontalLayoutGroup>(pagePanelTr.gameObject);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(12, 12, 6, 6);
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 3.1. Nút Đúc Kiếm (BtnSpawnSword)
            Transform btnSpawn = pagePanelTr.Find("BtnSpawnSword");
            if (btnSpawn == null)
            {
                GameObject bGO = new GameObject("BtnSpawnSword", typeof(RectTransform));
                bGO.transform.SetParent(pagePanelTr, false);
                btnSpawn = bGO.transform;
            }
            RectTransform bsRt = btnSpawn as RectTransform;
            bsRt.sizeDelta = new Vector2(185f, 52f);

            var bsImg = GetOrAdd<Image>(btnSpawn.gameObject);
            bsImg.color = ColGoldPrimary;
            var bsOutline = GetOrAdd<Outline>(btnSpawn.gameObject);
            bsOutline.effectColor = new Color(0.4f, 0.25f, 0.05f, 0.5f);
            bsOutline.effectDistance = new Vector2(1f, -1f);

            var bsBtn = GetOrAdd<Button>(btnSpawn.gameObject);
            bsBtn.onClick.RemoveAllListeners();
            bsBtn.onClick.AddListener(() =>
            {
                if (MergeManager.Instance != null) MergeManager.Instance.TryAddSword(1);
            });

            Transform bsTxtTr = btnSpawn.Find("Label");
            if (bsTxtTr == null) bsTxtTr = btnSpawn.Find("Text");
            if (bsTxtTr == null)
            {
                GameObject tGO = new GameObject("Label", typeof(RectTransform));
                tGO.transform.SetParent(btnSpawn, false);
                bsTxtTr = tGO.transform;
            }
            RectTransform bstRt = bsTxtTr as RectTransform;
            bstRt.anchorMin = Vector2.zero; bstRt.anchorMax = Vector2.one;
            bstRt.offsetMin = Vector2.zero; bstRt.offsetMax = Vector2.zero;
            var bsTxt = GetOrAdd<Text>(bsTxtTr.gameObject);
            bsTxt.font = font;
            bsTxt.fontSize = 18;
            bsTxt.fontStyle = FontStyle.Bold;
            bsTxt.color = new Color(0.12f, 0.08f, 0.01f, 1f);
            bsTxt.alignment = TextAnchor.MiddleCenter;
            bsTxt.text = "⚔️ ĐÚC KIẾM";

            // 3.2. Nút Hợp Nhanh (btn_QuickMerge) - Tên cố định cho TutorialManager & TooltipManager
            Transform btnQuick = pagePanelTr.Find("btn_QuickMerge");
            if (btnQuick == null)
            {
                GameObject bGO = new GameObject("btn_QuickMerge", typeof(RectTransform));
                bGO.transform.SetParent(pagePanelTr, false);
                btnQuick = bGO.transform;
            }
            RectTransform bqRt = btnQuick as RectTransform;
            bqRt.sizeDelta = new Vector2(185f, 52f);

            var bqImg = GetOrAdd<Image>(btnQuick.gameObject);
            bqImg.color = ColJadeGreen;
            var bqOutline = GetOrAdd<Outline>(btnQuick.gameObject);
            bqOutline.effectColor = new Color(0.08f, 0.35f, 0.15f, 0.5f);
            bqOutline.effectDistance = new Vector2(1f, -1f);

            var bqBtn = GetOrAdd<Button>(btnQuick.gameObject);
            bqBtn.onClick.RemoveAllListeners();
            bqBtn.onClick.AddListener(() =>
            {
                if (MergeManager.Instance != null) MergeManager.Instance.QuickMerge();
            });

            Transform bqTxtTr = btnQuick.Find("Text");
            if (bqTxtTr == null) bqTxtTr = btnQuick.Find("Label");
            if (bqTxtTr == null)
            {
                GameObject tGO = new GameObject("Text", typeof(RectTransform));
                tGO.transform.SetParent(btnQuick, false);
                bqTxtTr = tGO.transform;
            }
            RectTransform bqtRt = bqTxtTr as RectTransform;
            bqtRt.anchorMin = Vector2.zero; bqtRt.anchorMax = Vector2.one;
            bqtRt.offsetMin = Vector2.zero; bqtRt.offsetMax = Vector2.zero;
            var bqTxt = GetOrAdd<Text>(bqTxtTr.gameObject);
            bqTxt.font = font;
            bqTxt.fontSize = 18;
            bqTxt.fontStyle = FontStyle.Bold;
            bqTxt.color = Color.white;
            bqTxt.alignment = TextAnchor.MiddleCenter;
            bqTxt.text = "⚡ HỢP NHANH";

            // 3.3. Nút Sắp Xếp (SortBtn)
            Transform btnSort = pagePanelTr.Find("SortBtn");
            if (btnSort == null)
            {
                GameObject bGO = new GameObject("SortBtn", typeof(RectTransform));
                bGO.transform.SetParent(pagePanelTr, false);
                btnSort = bGO.transform;
            }
            RectTransform bstRt2 = btnSort as RectTransform;
            bstRt2.sizeDelta = new Vector2(140f, 52f);

            var bsoImg = GetOrAdd<Image>(btnSort.gameObject);
            bsoImg.color = new Color(0.78f, 0.42f, 0.15f, 1f); // Warm amber
            var bsoOutline = GetOrAdd<Outline>(btnSort.gameObject);
            bsoOutline.effectColor = ColGoldBorder;
            bsoOutline.effectDistance = new Vector2(1f, -1f);

            var bsoBtn = GetOrAdd<Button>(btnSort.gameObject);
            bsoBtn.onClick.RemoveAllListeners();
            bsoBtn.onClick.AddListener(() =>
            {
                if (MergeManager.Instance != null) MergeManager.Instance.SortInventory();
            });

            Transform bsoTxtTr = btnSort.Find("Txt");
            if (bsoTxtTr == null) bsoTxtTr = btnSort.Find("Text");
            if (bsoTxtTr == null) bsoTxtTr = btnSort.Find("Label");
            if (bsoTxtTr == null)
            {
                GameObject tGO = new GameObject("Txt", typeof(RectTransform));
                tGO.transform.SetParent(btnSort, false);
                bsoTxtTr = tGO.transform;
            }
            RectTransform bsotRt = bsoTxtTr as RectTransform;
            bsotRt.anchorMin = Vector2.zero; bsotRt.anchorMax = Vector2.one;
            bsotRt.offsetMin = Vector2.zero; bsotRt.offsetMax = Vector2.zero;
            var bsoTxt = GetOrAdd<Text>(bsoTxtTr.gameObject);
            bsoTxt.font = font;
            bsoTxt.fontSize = 17;
            bsoTxt.fontStyle = FontStyle.Bold;
            bsoTxt.color = Color.white;
            bsoTxt.alignment = TextAnchor.MiddleCenter;
            bsoTxt.text = "SẮP XẾP";

            // 3.4. Nút Lùi Trang (PrevBtn)
            Transform btnPrev = pagePanelTr.Find("PrevBtn");
            if (btnPrev == null)
            {
                GameObject bGO = new GameObject("PrevBtn", typeof(RectTransform));
                bGO.transform.SetParent(pagePanelTr, false);
                btnPrev = bGO.transform;
            }
            RectTransform bpRt = btnPrev as RectTransform;
            bpRt.sizeDelta = new Vector2(58f, 52f);

            var bpImg = GetOrAdd<Image>(btnPrev.gameObject);
            bpImg.color = ColCardBg;
            var bpOutline = GetOrAdd<Outline>(btnPrev.gameObject);
            bpOutline.effectColor = ColGoldBorder;
            bpOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var bpBtn = GetOrAdd<Button>(btnPrev.gameObject);
            bpBtn.onClick.RemoveAllListeners();
            bpBtn.onClick.AddListener(() =>
            {
                if (MergeManager.Instance != null) MergeManager.Instance.PrevPage();
            });

            Transform bpTxtTr = btnPrev.Find("Txt");
            if (bpTxtTr == null) bpTxtTr = btnPrev.Find("Text");
            if (bpTxtTr == null)
            {
                GameObject tGO = new GameObject("Txt", typeof(RectTransform));
                tGO.transform.SetParent(btnPrev, false);
                bpTxtTr = tGO.transform;
            }
            RectTransform bptRt = bpTxtTr as RectTransform;
            bptRt.anchorMin = Vector2.zero; bptRt.anchorMax = Vector2.one;
            bptRt.offsetMin = Vector2.zero; bptRt.offsetMax = Vector2.zero;
            var bpTxt = GetOrAdd<Text>(bpTxtTr.gameObject);
            bpTxt.font = font;
            bpTxt.fontSize = 22;
            bpTxt.fontStyle = FontStyle.Bold;
            bpTxt.color = ColGoldPrimary;
            bpTxt.alignment = TextAnchor.MiddleCenter;
            bpTxt.text = "◀";

            // 3.5. Nhãn Hiển Thị Số Trang (PageText)
            Transform txtPageTr = pagePanelTr.Find("PageText");
            if (txtPageTr == null)
            {
                GameObject tGO = new GameObject("PageText", typeof(RectTransform));
                tGO.transform.SetParent(pagePanelTr, false);
                txtPageTr = tGO.transform;
            }
            RectTransform ptRt = txtPageTr as RectTransform;
            ptRt.sizeDelta = new Vector2(80f, 52f);

            var pTxt = GetOrAdd<Text>(txtPageTr.gameObject);
            pTxt.font = font;
            pTxt.fontSize = 20;
            pTxt.fontStyle = FontStyle.Bold;
            pTxt.color = ColGoldPrimary;
            pTxt.alignment = TextAnchor.MiddleCenter;
            pTxt.text = "1/5";

            // 3.6. Nút Tiến Trang (NextBtn)
            Transform btnNext = pagePanelTr.Find("NextBtn");
            if (btnNext == null)
            {
                GameObject bGO = new GameObject("NextBtn", typeof(RectTransform));
                bGO.transform.SetParent(pagePanelTr, false);
                btnNext = bGO.transform;
            }
            RectTransform bnRt = btnNext as RectTransform;
            bnRt.sizeDelta = new Vector2(58f, 52f);

            var bnImg = GetOrAdd<Image>(btnNext.gameObject);
            bnImg.color = ColCardBg;
            var bnOutline = GetOrAdd<Outline>(btnNext.gameObject);
            bnOutline.effectColor = ColGoldBorder;
            bnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var bnBtn = GetOrAdd<Button>(btnNext.gameObject);
            bnBtn.onClick.RemoveAllListeners();
            bnBtn.onClick.AddListener(() =>
            {
                if (MergeManager.Instance != null) MergeManager.Instance.NextPage();
            });

            Transform bnTxtTr = btnNext.Find("Txt");
            if (bnTxtTr == null) bnTxtTr = btnNext.Find("Text");
            if (bnTxtTr == null)
            {
                GameObject tGO = new GameObject("Txt", typeof(RectTransform));
                tGO.transform.SetParent(btnNext, false);
                bnTxtTr = tGO.transform;
            }
            RectTransform bntRt = bnTxtTr as RectTransform;
            bntRt.anchorMin = Vector2.zero; bntRt.anchorMax = Vector2.one;
            bntRt.offsetMin = Vector2.zero; bntRt.offsetMax = Vector2.zero;
            var bnTxt = GetOrAdd<Text>(bnTxtTr.gameObject);
            bnTxt.font = font;
            bnTxt.fontSize = 22;
            bnTxt.fontStyle = FontStyle.Bold;
            bnTxt.color = ColGoldPrimary;
            bnTxt.alignment = TextAnchor.MiddleCenter;
            bnTxt.text = "▶";

            // Cố định thứ tự hiển thị từ trái sang phải
            btnSpawn.SetSiblingIndex(0);
            btnQuick.SetSiblingIndex(1);
            btnSort.SetSiblingIndex(2);
            btnPrev.SetSiblingIndex(3);
            txtPageTr.SetSiblingIndex(4);
            btnNext.SetSiblingIndex(5);

            // 4. Đồng bộ tham chiếu và Persistent Event cho MergeManager
            var mergeMgr = Object.FindAnyObjectByType<MergeManager>(FindObjectsInactive.Include);
            if (mergeMgr != null)
            {
                mergeMgr.pageText = pTxt;

                Transform pagesContainer = invContainer.Find("ScrollView/Viewport/Content/PagesContainer");
                if (pagesContainer != null)
                {
                    Transform[] pArray = new Transform[pagesContainer.childCount];
                    for (int i = 0; i < pagesContainer.childCount; i++)
                    {
                        pArray[i] = pagesContainer.GetChild(i);
                        pArray[i].gameObject.SetActive(i == 0);
                    }
                    mergeMgr.inventoryPages = pArray;
                    pTxt.text = $"1/{pArray.Length}";
                    EditorUtility.SetDirty(pTxt);
                    EditorUtility.SetDirty(txtPageTr.gameObject);
                }

                UnityEditor.Events.UnityEventTools.RemovePersistentListener(bpBtn.onClick, mergeMgr.PrevPage);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(bpBtn.onClick, mergeMgr.PrevPage);

                UnityEditor.Events.UnityEventTools.RemovePersistentListener(bnBtn.onClick, mergeMgr.NextPage);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(bnBtn.onClick, mergeMgr.NextPage);

                UnityEditor.Events.UnityEventTools.RemovePersistentListener(bsoBtn.onClick, mergeMgr.SortInventory);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(bsoBtn.onClick, mergeMgr.SortInventory);

                UnityEditor.Events.UnityEventTools.RemovePersistentListener(bqBtn.onClick, mergeMgr.QuickMerge);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(bqBtn.onClick, mergeMgr.QuickMerge);

                EditorUtility.SetDirty(mergeMgr);
                Debug.Log("[UIRefine] Đã đồng bộ thành công PageText & 5 Pages & Event Listeners cho MergeManager.");
            }
        }
    }

    private static void RefineDrawer1_LuyenThe(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 4b. Tinh chỉnh Drawer 1: Luyện Thể...");
        Transform d1 = mobileRoot.Find("Drawer_1");
        if (d1 == null) return;
        StyleDrawerBase(d1, "🛡️ THIÊN PHÚ THỂ TU", font);

        var ui = d1.GetComponent<BodyTemperingPanelUI>();
        if (ui == null) return;

        // Resource Bar
        Transform resBar = d1.Find("ResourceBar");
        if (resBar != null)
        {
            var rImg = GetOrAdd<Image>(resBar.gameObject);
            rImg.color = ColMidnight;
            var rOutline = GetOrAdd<Outline>(resBar.gameObject);
            rOutline.effectColor = ColGoldBorder;
            rOutline.effectDistance = new Vector2(1f, -1f);
        }

        // Tinh chỉnh 6 Node Cards
        Transform treeContent = d1.Find("TreeScrollView/Viewport/Content");
        if (treeContent != null)
        {
            for (int i = 0; i < 6; i++)
            {
                Transform card = treeContent.Find("NodeCard_" + i);
                if (card == null) continue;

                var cImg = GetOrAdd<Image>(card.gameObject);
                cImg.color = ColCardBg;

                var cOutline = GetOrAdd<Outline>(card.gameObject);
                cOutline.effectColor = ColGoldBorder;
                cOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Transform btnAct = card.Find("BtnAction");
                if (btnAct != null)
                {
                    var bImg = GetOrAdd<Image>(btnAct.gameObject);
                    bImg.color = ColJadeGreen;
                }
            }
        }
    }

    private static void RefineDrawer2_LuyenDan(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 4c. Đại tu & Tinh chỉnh Drawer 2: Luyện Đan (Bát Quái Lò & Tỷ Lệ)...");
        Transform d2 = mobileRoot.Find("Drawer_2");
        if (d2 == null) return;
        StyleDrawerBase(d2, "⚗️ LUYỆN ĐAN CÁC", font);

        var ui = d2.GetComponent<AlchemyPanelUI>();
        if (ui == null) return;

        // 1. Kho Thảo Dược & Cấp Đan Sư (HerbVault)
        Transform vaultTr = d2.Find("HerbVault");
        if (vaultTr != null)
        {
            var vImg = GetOrAdd<Image>(vaultTr.gameObject);
            vImg.color = ColMidnight;
            var vOutline = GetOrAdd<Outline>(vaultTr.gameObject);
            vOutline.effectColor = ColGoldBorder;
            vOutline.effectDistance = new Vector2(1f, -1f);
        }

        // 2. Lò Luyện Trung Tâm (FurnaceCard)
        Transform furnaceTr = d2.Find("FurnaceCard");
        if (furnaceTr != null)
        {
            var fImg = GetOrAdd<Image>(furnaceTr.gameObject);
            fImg.color = ColCardBg;

            var fOutline = GetOrAdd<Outline>(furnaceTr.gameObject);
            fOutline.effectColor = ColGoldPrimary;
            fOutline.effectDistance = new Vector2(2f, -2f);

            // Thêm Thanh Hiển Thị Tỷ Lệ Đan Dược (Quality Rates Bar)
            Transform ratesBarTr = furnaceTr.Find("RatesBar");
            if (ratesBarTr == null)
            {
                GameObject rbGO = new GameObject("RatesBar", typeof(RectTransform));
                rbGO.transform.SetParent(furnaceTr, false);
                ratesBarTr = rbGO.transform;
            }
            RectTransform rbRt = ratesBarTr as RectTransform;
            rbRt.anchorMin = new Vector2(0.08f, 0.32f);
            rbRt.anchorMax = new Vector2(0.92f, 0.42f);
            rbRt.offsetMin = Vector2.zero; rbRt.offsetMax = Vector2.zero;

            var rbImg = GetOrAdd<Image>(ratesBarTr.gameObject);
            rbImg.color = new Color(0.02f, 0.03f, 0.05f, 0.95f);

            // 4 phân đoạn tỷ lệ: Phế 10%, Hạ 50%, Trung 30%, Cực Phẩm 10%
            string[] segNames = { "Seg_Phe", "Seg_Ha", "Seg_Trung", "Seg_CucPham" };
            Color[] segColors = {
                new Color(0.45f, 0.45f, 0.50f, 1f), // Xám (Phế Đan)
                ColJadeGreen,                       // Xanh lục (Hạ phẩm)
                ColSpiritCyan,                      // Xanh lam (Trung phẩm)
                ColGoldPrimary                      // Vàng kim (Cực phẩm)
            };
            float[] minX = { 0f, 0.10f, 0.60f, 0.90f };
            float[] maxX = { 0.10f, 0.60f, 0.90f, 1f };

            for (int s = 0; s < 4; s++)
            {
                Transform seg = ratesBarTr.Find(segNames[s]);
                if (seg == null)
                {
                    GameObject sGO = new GameObject(segNames[s], typeof(RectTransform));
                    sGO.transform.SetParent(ratesBarTr, false);
                    seg = sGO.transform;
                }
                RectTransform sRt = seg as RectTransform;
                sRt.anchorMin = new Vector2(minX[s], 0f);
                sRt.anchorMax = new Vector2(maxX[s], 1f);
                sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;
                GetOrAdd<Image>(seg.gameObject).color = segColors[s];
            }
        }

        // 3. Hai Thẻ Bài Thuốc (RecipeContainer)
        Transform recipesTr = d2.Find("RecipeContainer");
        if (recipesTr != null)
        {
            Transform cardTuVi = recipesTr.Find("Card_TuVi");
            if (cardTuVi != null)
            {
                var cImg = GetOrAdd<Image>(cardTuVi.gameObject);
                cImg.color = ColCardBg;
                var cOutline = GetOrAdd<Outline>(cardTuVi.gameObject);
                cOutline.effectColor = ColJadeGreen;
                cOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            Transform cardBT = recipesTr.Find("Card_Breakthrough");
            if (cardBT != null)
            {
                var cImg = GetOrAdd<Image>(cardBT.gameObject);
                cImg.color = ColCardBg;
                var cOutline = GetOrAdd<Outline>(cardBT.gameObject);
                cOutline.effectColor = ColGoldPrimary;
                cOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        // 4. Đan Khố & Uống Đan (PillVault)
        Transform pillVaultTr = d2.Find("PillVault");
        if (pillVaultTr != null)
        {
            var vImg = GetOrAdd<Image>(pillVaultTr.gameObject);
            vImg.color = ColMidnight;

            var vOutline = GetOrAdd<Outline>(pillVaultTr.gameObject);
            vOutline.effectColor = ColGoldBorder;
            vOutline.effectDistance = new Vector2(1f, -1f);

            Transform btnDrink = pillVaultTr.Find("BtnDrinkTuVi");
            if (btnDrink != null)
            {
                var bImg = GetOrAdd<Image>(btnDrink.gameObject);
                bImg.color = new Color(0.18f, 0.50f, 0.75f, 1f);
            }
        }
    }

    private static void RefineDrawer3_PhapKhi(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 4d. Tinh chỉnh Drawer 3: Pháp Khí...");
        Transform d3 = mobileRoot.Find("Drawer_3");
        if (d3 == null) return;
        StyleDrawerBase(d3, "✨ HỘ THÂN PHÁP KHÍ", font);

        var ui = d3.GetComponent<ArtifactPanelUI>();
        if (ui == null) return;

        Transform resBar = d3.Find("ResourceBar");
        if (resBar != null)
        {
            var rImg = GetOrAdd<Image>(resBar.gameObject);
            rImg.color = ColMidnight;
            var rOutline = GetOrAdd<Outline>(resBar.gameObject);
            rOutline.effectColor = ColGoldBorder;
            rOutline.effectDistance = new Vector2(1f, -1f);
        }

        Transform content = d3.Find("ArtifactScrollView/Viewport/Content");
        if (content != null)
        {
            for (int i = 0; i < 4; i++)
            {
                Transform card = content.Find("ArtCard_" + i);
                if (card == null) continue;

                var cImg = GetOrAdd<Image>(card.gameObject);
                cImg.color = ColCardBg;

                var cOutline = GetOrAdd<Outline>(card.gameObject);
                cOutline.effectColor = ColMagicPurple;
                cOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Transform btnUpg = card.Find("BtnUpgrade");
                if (btnUpg != null)
                {
                    var bImg = GetOrAdd<Image>(btnUpg.gameObject);
                    bImg.color = ColJadeGreen;
                }
            }
        }
    }

    private static void RefineDrawer4_TranPhap(RectTransform mobileRoot, Font font)
    {
        Debug.Log("[UIRefine] 4e. Tinh chỉnh Drawer 4: Trận Pháp...");
        Transform d4 = mobileRoot.Find("Drawer_4");
        if (d4 == null) return;
        StyleDrawerBase(d4, "🌀 TRẬN PHÁP CÁC", font);

        var ui = d4.GetComponent<FormationPanelUI>();
        if (ui == null) return;

        Transform resBar = d4.Find("ResourceBar");
        if (resBar != null)
        {
            var rImg = GetOrAdd<Image>(resBar.gameObject);
            rImg.color = ColMidnight;
            var rOutline = GetOrAdd<Outline>(resBar.gameObject);
            rOutline.effectColor = ColGoldBorder;
            rOutline.effectDistance = new Vector2(1f, -1f);
        }

        Transform content = d4.Find("FormationScrollView/Viewport/Content");
        if (content != null)
        {
            for (int i = 0; i < 6; i++)
            {
                Transform card = content.Find("FormCard_" + i);
                if (card == null) continue;

                var cImg = GetOrAdd<Image>(card.gameObject);
                cImg.color = ColCardBg;

                var cOutline = GetOrAdd<Outline>(card.gameObject);
                cOutline.effectColor = ColSpiritCyan;
                cOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Transform btnSel = card.Find("BtnSelect");
                if (btnSel != null)
                {
                    var bImg = GetOrAdd<Image>(btnSel.gameObject);
                    bImg.color = ColJadeGreen;
                }

                Transform btnUpg = card.Find("BtnUpgrade");
                if (btnUpg != null)
                {
                    var bImg = GetOrAdd<Image>(btnUpg.gameObject);
                    bImg.color = new Color(0.65f, 0.40f, 0.12f, 1f);
                }
            }
        }
    }

    private static void RefineBackdrop(RectTransform mobileRoot)
    {
        Transform backdropTr = mobileRoot.Find("MobileBackdrop");
        if (backdropTr != null)
        {
            var img = GetOrAdd<Image>(backdropTr.gameObject);
            img.color = new Color(0f, 0f, 0f, 0.58f);
            img.raycastTarget = true;
        }
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        T comp = go.GetComponent<T>();
        if (comp != null) return comp;
        try
        {
            return go.AddComponent<T>();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UIRefine] Không thể AddComponent<{typeof(T).Name}> vào {go.name}: {ex.Message}");
            return null;
        }
    }
}
#endif

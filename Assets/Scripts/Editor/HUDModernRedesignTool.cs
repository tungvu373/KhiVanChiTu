#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// MASTER TOOL TÁI THIẾT KẾ TOÀN DIỆN UX/UI MOBILE PORTRAIT 9:16 (HUD MODERN REDESIGN V2).
/// Giải quyết triệt để 100% tình trạng thiếu rõ ràng, lỗi font emoji, che khuất text và đè chồng:
/// 1. TopBar: Sắp xếp thứ tự Sibling chuẩn xác (BG luôn nằm sau Text), giải phóng text Linh Thạch & Cơ Duyên.
/// 2. Thay thế toàn bộ Emoji hỏng (🎒 -> "TÚI", 🏆 -> "T.TỰU", ⚔️ -> chuẩn text) bằng nhãn mạ vàng kim sắc nét.
/// 3. CharacterFrame: Hiển thị Đạo Hữu, Cảnh Giới (Cyan), Tu Vi rút gọn (FormatNumber) cực kỳ sang trọng.
/// 4. PlayerHPBar: Thanh máu ngọc huyết thanh mảnh (170x15px) bo góc ngay ngắn dưới Avatar.
/// 5. StageUI: Capsule Tiên Giới (240x24px) căn chính giữa, thanh tiến độ mảnh vàng kim.
/// 6. TutorialTargetQuestBar: Thu gọn chiều cao xuống 34px (từ 42px), nền kính Obsidian mờ với viền vàng kim tinh tế.
/// </summary>
public static class HUDModernRedesignTool
{
    private static readonly Color ColBgHeader = new Color(0.05f, 0.07f, 0.11f, 0.92f); // Deep Obsidian Glass
    private static readonly Color ColBorderGold = new Color(0.92f, 0.75f, 0.28f, 0.85f);
    private static readonly Color ColTextGold = new Color(0.98f, 0.86f, 0.42f, 1f);
    private static readonly Color ColTextCyan = new Color(0.40f, 0.90f, 1f, 1f);
    private static readonly Color ColPillBg = new Color(0.08f, 0.11f, 0.17f, 0.95f);
    private static readonly Color ColHpFill = new Color(0.88f, 0.22f, 0.20f, 1f);
    private static readonly Color ColHpBg = new Color(0.12f, 0.05f, 0.06f, 0.92f);

    [MenuItem("TuTien/UI/✨ Áp Dụng Thiết Kế HUD Hiện Đại (Master Redesign)")]
    public static void ApplyModernHUDLayout()
    {
        var mobileRoot = GameObject.Find("Canvas/MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("❌ [HUDModernRedesign] Không tìm thấy Canvas/MobileUIRoot!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(mobileRoot, "Apply Modern HUD Layout V2");

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // ─────────────────────────────────────────────────────────────────────
        // 1. TOP BAR CONTAINER (Thanh Header Trên Cùng)
        // ─────────────────────────────────────────────────────────────────────
        Transform topBarTr = mobileRoot.transform.Find("TopBar");
        if (topBarTr != null)
        {
            RectTransform tbRt = topBarTr.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 1f);
            tbRt.anchorMax = new Vector2(1f, 1f);
            tbRt.pivot = new Vector2(0.5f, 1f);
            tbRt.anchoredPosition = new Vector2(0f, 0f);
            tbRt.sizeDelta = new Vector2(0f, 62f);

            var tbImg = GetOrAdd<Image>(topBarTr.gameObject);
            tbImg.color = ColBgHeader;

            // 1A. Ví Linh Thạch (Cyan Pill)
            Transform bgLt = topBarTr.Find("BG_LinhThach");
            if (bgLt != null)
            {
                RectTransform rt = bgLt.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-206f, 0f);
                rt.sizeDelta = new Vector2(110f, 32f);

                var img = GetOrAdd<Image>(bgLt.gameObject);
                img.color = ColPillBg;
                img.raycastTarget = false;

                var ol = GetOrAdd<Outline>(bgLt.gameObject);
                ol.effectColor = new Color(0.30f, 0.75f, 0.85f, 0.70f);
                ol.effectDistance = new Vector2(1f, -1f);
            }

            Transform txtLt = topBarTr.Find("txt_LinhThach");
            if (txtLt != null)
            {
                RectTransform rt = txtLt.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-206f, 0f);
                rt.sizeDelta = new Vector2(110f, 32f);

                Text t = txtLt.GetComponent<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 12;
                    t.fontStyle = FontStyle.Bold;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.color = ColTextCyan;
                    t.raycastTarget = false;
                }
            }

            // 1B. Ví Cơ Duyên (Gold Pill)
            Transform bgCd = topBarTr.Find("BG_CoDuyen");
            if (bgCd != null)
            {
                RectTransform rt = bgCd.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-104f, 0f);
                rt.sizeDelta = new Vector2(96f, 32f);

                var img = GetOrAdd<Image>(bgCd.gameObject);
                img.color = ColPillBg;
                img.raycastTarget = false;

                var ol = GetOrAdd<Outline>(bgCd.gameObject);
                ol.effectColor = ColBorderGold;
                ol.effectDistance = new Vector2(1f, -1f);
            }

            Transform txtCd = topBarTr.Find("txt_CoDuyen");
            if (txtCd != null)
            {
                RectTransform rt = txtCd.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.anchoredPosition = new Vector2(-104f, 0f);
                rt.sizeDelta = new Vector2(96f, 32f);

                Text t = txtCd.GetComponent<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 12;
                    t.fontStyle = FontStyle.Bold;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.color = ColTextGold;
                    t.raycastTarget = false;
                }
            }

            // ĐẢM BẢO THỨ TỰ SIBLING: BG PHẢI NẰM DƯỚI TEXT!
            if (bgLt != null) bgLt.SetSiblingIndex(0);
            if (txtLt != null) txtLt.SetSiblingIndex(1);
            if (bgCd != null) bgCd.SetSiblingIndex(2);
            if (txtCd != null) txtCd.SetSiblingIndex(3);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. CHARACTER FRAME (Avatar & Thông Tin Nhân Vật Bên Trái)
        // ─────────────────────────────────────────────────────────────────────
        Transform charFrameTr = mobileRoot.transform.Find("CharacterFrame");
        if (charFrameTr != null)
        {
            RectTransform rt = charFrameTr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -5f);
            rt.sizeDelta = new Vector2(210f, 52f);

            var img = GetOrAdd<Image>(charFrameTr.gameObject);
            img.color = new Color(0f, 0f, 0f, 0f); // Trong suốt

            Transform avatar = charFrameTr.Find("Avatar");
            if (avatar != null)
            {
                RectTransform aRt = avatar.GetComponent<RectTransform>();
                aRt.anchorMin = aRt.anchorMax = new Vector2(0f, 0.5f);
                aRt.pivot = new Vector2(0f, 0.5f);
                aRt.anchoredPosition = new Vector2(0f, 0f);
                aRt.sizeDelta = new Vector2(46f, 46f);

                var aOl = GetOrAdd<Outline>(avatar.gameObject);
                aOl.effectColor = ColBorderGold;
                aOl.effectDistance = new Vector2(1.5f, -1.5f);
            }

            Transform info = charFrameTr.Find("InfoText");
            if (info != null)
            {
                RectTransform iRt = info.GetComponent<RectTransform>();
                iRt.anchorMin = new Vector2(0f, 0f);
                iRt.anchorMax = new Vector2(1f, 1f);
                iRt.pivot = new Vector2(0f, 0.5f);
                iRt.anchoredPosition = new Vector2(52f, 0f);
                iRt.sizeDelta = new Vector2(-52f, 0f);

                Text t = info.GetComponent<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 11;
                    t.lineSpacing = 1.15f;
                    t.alignment = TextAnchor.MiddleLeft;
                    t.supportRichText = true;
                    // Format mẫu sắc sảo trước khi vào Play Mode
                    t.text = "<b><color=#FFD700>Đạo Hữu</color></b> · <color=#66D9EF>Luyện Khí · Sơ Kỳ</color>\n<color=#A0B0C5>Tu Vi:</color> <color=#FFFFFF>0 / 200</color>";
                }
            }

            Transform hint = charFrameTr.Find("DetailHintText");
            if (hint != null) hint.gameObject.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. ACTION BUTTONS: KHO ĐỒ [TÚI] & CÀI ĐẶT [⚙] (Góc Phải Trên Cùng)
        // ─────────────────────────────────────────────────────────────────────
        Transform pauseTr = mobileRoot.transform.Find("PauseButton");
        if (pauseTr != null)
        {
            RectTransform rt = pauseTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-8f, -10f);
            rt.sizeDelta = new Vector2(48f, 48f);

            var pImg = GetOrAdd<Image>(pauseTr.gameObject);
            pImg.color = ColPillBg;

            var pOl = GetOrAdd<Outline>(pauseTr.gameObject);
            pOl.effectColor = new Color(0.60f, 0.65f, 0.75f, 0.80f);
            pOl.effectDistance = new Vector2(1f, -1f);

            Text t = pauseTr.GetComponentInChildren<Text>();
            if (t != null)
            {
                if (font != null) t.font = font;
                t.fontSize = 16;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.white;
                t.text = "II";
            }
        }

        Transform storageTr = mobileRoot.transform.Find("BtnHUDStorage");
        if (storageTr != null)
        {
            RectTransform rt = storageTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-62f, -10f);
            rt.sizeDelta = new Vector2(56f, 48f);

            var sImg = GetOrAdd<Image>(storageTr.gameObject);
            sImg.color = ColPillBg;

            var sOl = GetOrAdd<Outline>(storageTr.gameObject);
            sOl.effectColor = ColBorderGold;
            sOl.effectDistance = new Vector2(1.5f, -1.5f);

            GetOrAdd<Button>(storageTr.gameObject);
            GetOrAdd<StorageButtonHandler>(storageTr.gameObject);

            Text t = storageTr.GetComponentInChildren<Text>();
            if (t != null)
            {
                if (font != null) t.font = font;
                t.fontSize = 12;
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = ColTextGold;
                t.text = "TÚI"; // Chữ TÚI màu vàng kim sắc nét, không bị lỗi glyph emoji
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. PLAYER HP BAR (Nằm ở Góc Dưới Bên Phải - Bottom-Right, tối ưu 1 ngón tay khi nằm nghiêng)
        // ─────────────────────────────────────────────────────────────────────
        Transform hpBarTr = mobileRoot.transform.Find("PlayerHPBar");
        if (hpBarTr != null)
        {
            hpBarTr.gameObject.SetActive(true);
            RectTransform rt = hpBarTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-16f, 202f);
            rt.sizeDelta = new Vector2(280f, 32f);

            var bgImg = GetOrAdd<Image>(hpBarTr.gameObject);
            bgImg.color = new Color(0.04f, 0.07f, 0.12f, 0.90f);

            var outline = GetOrAdd<Outline>(hpBarTr.gameObject);
            outline.effectColor = new Color(0.25f, 0.70f, 0.90f, 0.75f);
            outline.effectDistance = new Vector2(1.2f, -1.2f);

            Transform bgChild = hpBarTr.Find("BG");
            if (bgChild != null)
            {
                var bgChildImg = bgChild.GetComponent<Image>();
                if (bgChildImg != null) bgChildImg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);

                Transform fillTr = bgChild.Find("Fill");
                if (fillTr != null)
                {
                    var fillImg = fillTr.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.color = new Color(0.20f, 0.85f, 0.40f, 1f); // Xanh Ngọc Bích tràn đầy sinh mệnh
                    }
                }
            }

            Transform hpTxt = hpBarTr.Find("HPText");
            if (hpTxt != null)
            {
                Text t = hpTxt.GetComponent<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 13;
                    t.fontStyle = FontStyle.Bold;
                    t.alignment = TextAnchor.MiddleCenter;
                    t.color = Color.white;
                    t.text = "HP: 1,000 / 1,000 (100%)";
                }
                var txtOutline = GetOrAdd<Outline>(hpTxt.gameObject);
                txtOutline.effectColor = Color.black;
                txtOutline.effectDistance = new Vector2(1f, -1f);
            }
        }

        Transform mpBarTr = mobileRoot.transform.Find("PlayerManaBar");
        if (mpBarTr != null)
        {
            RectTransform rt = mpBarTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-16f, 238f);
            rt.sizeDelta = new Vector2(280f, 16f);
        }

        // CombatLogPanel bên trái dưới (đối xứng HP Bar bên phải)
        Transform logPanelTr = mobileRoot.transform.Find("CombatLogPanel");
        if (logPanelTr != null)
        {
            RectTransform rt = logPanelTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(16f, 202f);
            rt.sizeDelta = new Vector2(420f, 44f);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. STAGE UI (Badge Tầng Căn Chính Giữa: 240x24px)
        // ─────────────────────────────────────────────────────────────────────
        Transform stageTr = mobileRoot.transform.Find("StageUI");
        if (stageTr != null)
        {
            stageTr.gameObject.SetActive(true);
            RectTransform rt = stageTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -66f);
            rt.sizeDelta = new Vector2(240f, 24f);

            var img = GetOrAdd<Image>(stageTr.gameObject);
            img.color = ColPillBg;

            var ol = GetOrAdd<Outline>(stageTr.gameObject);
            ol.effectColor = ColBorderGold;
            ol.effectDistance = new Vector2(1f, -1f);

            Transform sTxt = stageTr.Find("StageText");
            if (sTxt != null)
            {
                Text txt = sTxt.GetComponent<Text>();
                if (txt != null)
                {
                    if (font != null) txt.font = font;
                    txt.fontSize = 11;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = ColTextGold;
                    txt.text = "TẦNG 1 · SƠ NHẬP TIÊN LỘ";
                }
            }

            Transform pBg = stageTr.Find("ProgressBG");
            if (pBg != null)
            {
                RectTransform pRt = pBg.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.06f, 0f);
                pRt.anchorMax = new Vector2(0.94f, 0f);
                pRt.pivot = new Vector2(0.5f, 0f);
                pRt.anchoredPosition = Vector2.zero;
                pRt.sizeDelta = new Vector2(0f, 2.5f);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. TUTORIAL TARGET QUEST BAR (THANH MỤC TIÊU GIỮA TRÁI: Thu Gọn/Mở Rộng Bằng Mũi Tên)
        // ─────────────────────────────────────────────────────────────────────
        Transform questTr = mobileRoot.transform.Find("TutorialTargetQuestBar");
        if (questTr != null)
        {
            questTr.gameObject.SetActive(true);
            RectTransform rt = questTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.52f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(10f, 0f);
            rt.sizeDelta = new Vector2(260f, 44f); // Nhỏ gọn thanh thoát bên giữa trái màn hình

            var qImg = GetOrAdd<Image>(questTr.gameObject);
            qImg.color = ColBgHeader;

            var qOutline = GetOrAdd<Outline>(questTr.gameObject);
            qOutline.effectColor = new Color(0.85f, 0.70f, 0.25f, 0.75f);
            qOutline.effectDistance = new Vector2(1f, -1f);

            Transform qStorage = questTr.Find("BtnStorage");
            if (qStorage != null) qStorage.gameObject.SetActive(false);

            // Tiêu đề nhiệm vụ
            Transform qTitle = questTr.Find("TxtQuestTitle");
            if (qTitle != null)
            {
                Text txt = qTitle.GetComponent<Text>();
                if (txt != null)
                {
                    if (font != null) txt.font = font;
                    txt.fontSize = 11;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleLeft;
                    txt.color = ColTextGold;
                    txt.text = "[Mục Tiêu] Trảm Thảo Trừ Căn (0/10)";
                }
                RectTransform titleRt = qTitle.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0f, 0f);
                titleRt.anchorMax = new Vector2(1f, 1f);
                titleRt.pivot = new Vector2(0f, 0.5f);
                titleRt.anchoredPosition = new Vector2(10f, 0f);
                titleRt.sizeDelta = new Vector2(-140f, 0f);
            }

            // Nút Thu Nhỏ/Mở Rộng Bằng Mũi Tên (BtnToggle)
            Transform togBtnTr = questTr.Find("BtnToggle");
            if (togBtnTr != null)
            {
                RectTransform tRt = togBtnTr.GetComponent<RectTransform>();
                tRt.anchorMin = tRt.anchorMax = new Vector2(1f, 0.5f);
                tRt.pivot = new Vector2(1f, 0.5f);
                tRt.anchoredPosition = new Vector2(-4f, 0f);
                tRt.sizeDelta = new Vector2(28f, 28f);

                Text t = togBtnTr.GetComponent<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 16;
                    t.fontStyle = FontStyle.Bold;
                    t.text = "◀";
                    t.color = ColTextGold;
                    t.alignment = TextAnchor.MiddleCenter;
                }
            }

            // Nút Nhận Mốc [MỐC] / [NHẬN]
            Transform claimBtnTr = questTr.Find("BtnClaimQuick");
            if (claimBtnTr != null)
            {
                RectTransform btnRt = claimBtnTr.GetComponent<RectTransform>();
                btnRt.anchorMin = btnRt.anchorMax = new Vector2(1f, 0.5f);
                btnRt.pivot = new Vector2(1f, 0.5f);
                btnRt.anchoredPosition = new Vector2(-36f, 0f);
                btnRt.sizeDelta = new Vector2(46f, 24f);

                var btnImg = claimBtnTr.GetComponent<Image>();
                if (btnImg != null) btnImg.color = new Color(0.92f, 0.55f, 0.12f, 1f);

                Text t = claimBtnTr.GetComponentInChildren<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 10;
                    t.fontStyle = FontStyle.Bold;
                    t.color = new Color(0.10f, 0.05f, 0.01f, 1f);
                    t.text = "MỐC";
                }
            }

            // Nút Thành Tựu [T.TỰU]
            Transform achBtnTr = questTr.Find("BtnAchievement");
            if (achBtnTr != null)
            {
                RectTransform aRt = achBtnTr.GetComponent<RectTransform>();
                aRt.anchorMin = aRt.anchorMax = new Vector2(1f, 0.5f);
                aRt.pivot = new Vector2(1f, 0.5f);
                aRt.anchoredPosition = new Vector2(-86f, 0f);
                aRt.sizeDelta = new Vector2(46f, 24f);

                var aImg = achBtnTr.GetComponent<Image>();
                if (aImg != null) aImg.color = ColPillBg;

                var aOl = GetOrAdd<Outline>(achBtnTr.gameObject);
                aOl.effectColor = ColBorderGold;
                aOl.effectDistance = new Vector2(1f, -1f);

                Text t = achBtnTr.GetComponentInChildren<Text>();
                if (t != null)
                {
                    if (font != null) t.font = font;
                    t.fontSize = 10;
                    t.fontStyle = FontStyle.Bold;
                    t.color = ColTextGold;
                    t.text = "T.TỰU";
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. ACTIVE BUFF PANEL (NẰM DƯỚI STAGE UI: y = -98)
        // ─────────────────────────────────────────────────────────────────────
        Transform buffTr = mobileRoot.transform.Find("ActiveBuffPanel");
        if (buffTr != null)
        {
            RectTransform rt = buffTr.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -98f);
            rt.sizeDelta = new Vector2(340f, 20f);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 8. COMBAT LOG PANEL (ẨN MẶC ĐỊNH ĐỂ GIẢI PHÓNG ĐẤU TRƯỜNG)
        // ─────────────────────────────────────────────────────────────────────
        Transform logTr = mobileRoot.transform.Find("CombatLogPanel");
        if (logTr != null)
        {
            logTr.gameObject.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 9. DISMANTLE CONFIRM MODAL (ĐẢM BẢO XẾP TRÊN STORAGE PANEL)
        // ─────────────────────────────────────────────────────────────────────
        var dismantleModal = GameObject.Find("Canvas/DismantleConfirmModalUI");
        if (dismantleModal != null)
        {
            dismantleModal.transform.SetAsLastSibling();
            var c = GetOrAdd<Canvas>(dismantleModal);
            c.overrideSorting = true;
            c.sortingOrder = 900;
            GetOrAdd<GraphicRaycaster>(dismantleModal);
            EditorUtility.SetDirty(dismantleModal);
        }

        Canvas.ForceUpdateCanvases();
        EditorUtility.SetDirty(mobileRoot);
        EditorSceneManager.MarkSceneDirty(mobileRoot.scene);

        Debug.Log("🎉 [HUDModernRedesign V2] HOÀN TẤT: Đã tái thiết kế toàn bộ HUD, giải quyết triệt để lỗi font emoji và che khuất!");
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}
#endif

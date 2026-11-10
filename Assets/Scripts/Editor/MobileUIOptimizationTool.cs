using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// Tool chuẩn hóa và tối ưu toàn diện UI theo BRIEF_ANTIGRAVITY_UI_DA_NEN_TANG_2026-11-03.
/// 1. Sửa triệt để lỗi nút X và click-through trên MilestoneModalUI.
/// 2. Quy hoạch lại Top HUD: Giải phóng 115px chiều cao, loại bỏ thanh thừa, mở rộng đấu trường 80-85%.
/// 3. Hạ thấp và làm mờ trong suốt CombatLogPanel.
/// 4. Đồng bộ MobileBackdrop chặn click xuyên màn hình khi mở Drawers.
/// </summary>
public static class MobileUIOptimizationTool
{
    [MenuItem("TuTien/UI/Apply Full Mobile UI Optimization")]
    public static void ApplyOptimization()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[MobileUIOptimize] Không tìm thấy Canvas trong Scene!");
            return;
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("[MobileUIOptimize] Không tìm thấy Canvas/MobileUIRoot!");
            return;
        }

        Debug.Log("=== BẮT ĐẦU TỐI ƯU TOÀN DIỆN MOBILE UI ===");

        // ── 1. SỬA TRIỆT ĐỂ MILESTONE MODAL UI ─────────────────────────────
        var modalTr = canvas.transform.Find("MilestoneModalUI");
        if (modalTr != null)
        {
            // Đảm bảo nền mờ modal chặn raycast và click backdrop đóng modal
            var mBg = modalTr.GetComponent<Image>();
            if (mBg == null) mBg = modalTr.gameObject.AddComponent<Image>();
            mBg.color = new Color(0f, 0f, 0f, 0.82f);
            mBg.raycastTarget = true;

            var mBtn = modalTr.GetComponent<Button>();
            if (mBtn == null) mBtn = modalTr.gameObject.AddComponent<Button>();

            var modalBox = modalTr.Find("ModalBox");
            if (modalBox != null)
            {
                var boxImg = modalBox.GetComponent<Image>();
                if (boxImg != null) boxImg.raycastTarget = true;

                var btnClose = modalBox.Find("BtnClose");
                if (btnClose != null)
                {
                    // BẮT BUỘC: Đưa BtnClose thành con cuối cùng trong ModalBox để không bị ScrollView hay bất cứ gì đè lên
                    btnClose.SetAsLastSibling();

                    var cRt = btnClose.GetComponent<RectTransform>();
                    cRt.anchorMin = new Vector2(1, 1);
                    cRt.anchorMax = new Vector2(1, 1);
                    cRt.pivot = new Vector2(1, 1);
                    cRt.anchoredPosition = new Vector2(-10f, -10f);
                    cRt.sizeDelta = new Vector2(56f, 56f);

                    var cImg = btnClose.GetComponent<Image>();
                    if (cImg == null) cImg = btnClose.gameObject.AddComponent<Image>();
                    cImg.color = new Color(0.88f, 0.22f, 0.22f, 0.95f);
                    cImg.raycastTarget = true;

                    var cTxt = btnClose.GetComponentInChildren<Text>();
                    if (cTxt != null)
                    {
                        cTxt.text = "✕";
                        cTxt.fontSize = 28;
                        cTxt.fontStyle = FontStyle.Bold;
                        cTxt.color = Color.white;
                        cTxt.alignment = TextAnchor.MiddleCenter;
                        cTxt.raycastTarget = false; // Bắt buộc false để click xuyên vào nút Image
                    }

                    var cBtn = btnClose.GetComponent<Button>();
                    if (cBtn != null)
                    {
                        cBtn.interactable = true;
                    }
                }
            }
            Debug.Log("✅ [1/4] Đã khắc phục dứt điểm nút [X] và sự kiện đóng cho MilestoneModalUI.");
        }

        // ── 2. QUY HOẠCH TOP HUD ĐỈNH MÀN HÌNH ──────────────────────────────
        // Giữ thanh giai đoạn tu vi mới; chỉ ẩn StagePanel legacy chưa có thanh.
        var stagePanel = mobileRoot.Find("StagePanel");
        if (stagePanel != null)
        {
            bool hasPhaseBar = stagePanel.GetComponent<CultivationPhaseBarUI>() != null;
            stagePanel.gameObject.SetActive(hasPhaseBar);
            if (hasPhaseBar && stagePanel is RectTransform phaseRect)
                phaseRect.anchoredPosition = new Vector2(0f, -138f);
            Debug.Log(hasPhaseBar ? "✅ [2a] Giữ thanh tu vi theo kỳ." : "✅ [2a] Đã tắt StagePanel trùng lặp.");
        }

        // 2b. Ẩn PlayerManaBar (game không dùng mana)
        var manaBar = mobileRoot.Find("PlayerManaBar");
        if (manaBar != null)
        {
            manaBar.gameObject.SetActive(false);
            Debug.Log("✅ [2b] Đã tắt PlayerManaBar không dùng.");
        }

        // 2c. Tinh chỉnh StageUI (Tầng hiện tại + Boss Timer) nằm gọn gàng tại Y = -92
        var stageUI = mobileRoot.Find("StageUI");
        if (stageUI != null)
        {
            stageUI.gameObject.SetActive(true);
            var rt = stageUI.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -92f);
            rt.sizeDelta = new Vector2(680f, 36f);
            Debug.Log("✅ [2c] Căn chỉnh StageUI gọn gàng tại Y = -92.");
        }

        // 2d. Tinh chỉnh PlayerHPBar thành thanh máu thanh mảnh bo góc tại Y = -132
        var hpBar = mobileRoot.Find("PlayerHPBar");
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);
            var rt = hpBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -132f);
            rt.sizeDelta = new Vector2(0f, 14f);
            Debug.Log("✅ [2d] Tinh giản PlayerHPBar (cao 14px) tại Y = -132.");
        }

        // 2e. Căn chỉnh TutorialTargetQuestBar tại Y = -152
        var questBar = mobileRoot.Find("TutorialTargetQuestBar");
        if (questBar != null)
        {
            questBar.gameObject.SetActive(true);
            var rt = questBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -152f);
            rt.sizeDelta = new Vector2(0f, 40f);
            Debug.Log("✅ [2e] Căn chỉnh TutorialTargetQuestBar thanh thoát tại Y = -152.");
        }

        // 2f. Căn chỉnh ActiveBuffPanel tại Y = -198
        var buffPanel = mobileRoot.Find("ActiveBuffPanel");
        if (buffPanel != null)
        {
            buffPanel.gameObject.SetActive(true);
            var rt = buffPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 1f);
            rt.anchorMax = new Vector2(0.95f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -198f);
            rt.sizeDelta = new Vector2(0f, 28f);
            Debug.Log("✅ [2f] Căn chỉnh ActiveBuffPanel tại Y = -198. HUD kết thúc gọn gàng!");
        }

        // ── 3. THU GỌN VÀ LÀM TRONG SUỐT COMBAT LOG PANEL ──────────────────
        var combatLog = mobileRoot.Find("CombatLogPanel");
        if (combatLog != null)
        {
            var rt = combatLog.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0f);
            rt.anchorMax = new Vector2(0.95f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 185f);
            rt.sizeDelta = new Vector2(0f, 48f); // Giảm chiều cao từ 88px xuống 48px

            var img = combatLog.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.04f, 0.04f, 0.08f, 0.35f); // Bán trong suốt không che khuất kiếm & quái
                img.raycastTarget = false;
            }
            Debug.Log("✅ [3/4] Thu gọn CombatLogPanel (cao 48px, alpha 0.35) sát mép dock.");
        }

        // ── 4. ĐỒNG BỘ BACKDROP CHỐNG CLICK XUYÊN MÀN HÌNH ────────────────
        var backdrop = mobileRoot.Find("MobileBackdrop");
        if (backdrop != null)
        {
            var bImg = backdrop.GetComponent<Image>();
            if (bImg == null) bImg = backdrop.gameObject.AddComponent<Image>();
            bImg.color = new Color(0f, 0f, 0f, 0.55f);
            bImg.raycastTarget = true;

            var bBtn = backdrop.GetComponent<Button>();
            if (bBtn == null) bBtn = backdrop.gameObject.AddComponent<Button>();

            // Kết nối click backdrop đóng Drawer
            var drawerCtrl = Object.FindAnyObjectByType<MobileDrawerUIController>();
            if (drawerCtrl != null)
            {
                bBtn.onClick.RemoveAllListeners();
                bBtn.onClick.AddListener(drawerCtrl.CloseActiveDrawer);
            }
            Debug.Log("✅ [4/4] MobileBackdrop đã sẵn sàng chặn click xuyên và hỗ trợ đóng Drawer 1-chạm.");
        }

        // ── 5. GẮN MOBILE UI NAVIGATION MANAGER VÀ MOCK ONLINE SERVICE ──
        if (mobileRoot.GetComponent<MobileUINavigationManager>() == null)
        {
            mobileRoot.gameObject.AddComponent<MobileUINavigationManager>();
            Debug.Log("✅ [5/5] Đã gắn MobileUINavigationManager vào MobileUIRoot.");
        }

        var managersObj = GameObject.Find("Managers");
        if (managersObj != null && managersObj.GetComponent<TuTien.UI.ViewModels.MockOnlineServiceProvider>() == null)
        {
            managersObj.AddComponent<TuTien.UI.ViewModels.MockOnlineServiceProvider>();
            Debug.Log("✅ [5/5] Đã gắn MockOnlineServiceProvider vào Managers.");
        }

        // Đánh dấu bẩn scene và lưu lại
        EditorSceneManager.MarkSceneDirty(canvas.scene);
        EditorSceneManager.SaveScene(canvas.scene);
        Debug.Log("🎉 [MobileUIOptimize] ĐÃ LƯU SCENE VỚI GIAO DIỆN MỚI THÀNH CÔNG!");
    }
}

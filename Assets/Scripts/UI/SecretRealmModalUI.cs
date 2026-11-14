using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện Hub Trung Tâm Kỳ Ngộ Bí Cảnh Thượng Cổ (Secret Realm Hub).
/// Thiết kế chuẩn Mobile Portrait 9:16: Header tinh gọn, hiển thị số dư Cơ Duyên & Lượt miễn phí,
/// Danh sách 5 thẻ Bí Cảnh sắc nét, chống đè lớp và tích hợp SafeAreaFitter.
/// </summary>
public class SecretRealmModalUI : MonoBehaviour
{
    public static SecretRealmModalUI Instance { get; private set; }

    public bool IsModalOpen => modalOverlay != null && modalOverlay.activeSelf;

    private GameObject modalOverlay;
    private Text walletText;
    private Text freeAttemptsText;
    private readonly List<Button> enterButtons = new List<Button>();

    public static Font DefaultFont
    {
        get
        {
            var f = UiTextStyle.BodyFont;
            if (f != null) return f;
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) return f;
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildIfNeeded();
    }

    public void Open()
    {
        BuildIfNeeded();
        if (modalOverlay != null)
        {
            modalOverlay.SetActive(true);
            modalOverlay.transform.SetAsLastSibling();
            RefreshUI();
        }
    }

    public void Close()
    {
        if (modalOverlay != null)
        {
            modalOverlay.SetActive(false);
        }
    }

    public void BuildIfNeeded()
    {
        var canvas = GameObject.Find("Canvas")?.transform;
        if (canvas == null) return;

        if (modalOverlay != null) return;

        var existing = canvas.Find("SecretRealmModal");
        if (existing != null)
        {
            modalOverlay = existing.gameObject;
            WireExistingModal();
            return;
        }

        BuildFreshModal(canvas);
    }

    private void BuildFreshModal(Transform canvas)
    {
        // 1. Overlay cản click
        modalOverlay = new GameObject("SecretRealmModal", typeof(RectTransform), typeof(Image), typeof(Button));
        modalOverlay.transform.SetParent(canvas, false);
        var rt = modalOverlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        modalOverlay.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.92f);
        modalOverlay.GetComponent<Button>().onClick.AddListener(Close);
        modalOverlay.transform.SetAsLastSibling();

        // 2. Modal Box
        var box = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(SafeAreaFitter));
        box.transform.SetParent(modalOverlay.transform, false);
        var bRt = box.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.offsetMin = bRt.offsetMax = Vector2.zero;
        box.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.15f, 0.99f);
        var bOl = box.GetComponent<Outline>();
        bOl.effectColor = new Color(0.85f, 0.45f, 1f, 0.5f);
        bOl.effectDistance = new Vector2(1f, -1f);
        box.GetComponent<SafeAreaFitter>().Refresh();

        // 3. Top Border
        var border = new GameObject("BorderTop", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(box.transform, false);
        var brt = border.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(1f, 1f);
        brt.offsetMin = new Vector2(0f, -4f);
        brt.offsetMax = Vector2.zero;
        border.GetComponent<Image>().color = new Color(0.85f, 0.45f, 1f, 1f);

        // 4. Close X
        var btnX = new GameObject("BtnCloseX", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnX.transform.SetParent(box.transform, false);
        var xrt = btnX.GetComponent<RectTransform>();
        xrt.anchorMin = xrt.anchorMax = Vector2.one;
        xrt.offsetMin = new Vector2(-52f, -52f);
        xrt.offsetMax = new Vector2(-12f, -12f);
        btnX.GetComponent<Image>().color = new Color(0.2f, 0.15f, 0.25f, 0.95f);
        var xtxt = CreateText(btnX.transform, "X", "X", 20, FontStyle.Bold, Vector2.zero, Color.white);
        xtxt.alignment = TextAnchor.MiddleCenter;
        btnX.GetComponent<Button>().onClick.AddListener(Close);

        // 5. Title & Subtitle
        var tGo = CreateText(box.transform, "Title", "🌌 KỲ NGỘ BÍ CẢNH THƯỢNG CỔ", 22, FontStyle.Bold, new Vector2(-20f, -28f), new Color(0.95f, 0.8f, 1f));
        tGo.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        tGo.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        tGo.GetComponent<RectTransform>().offsetMin = new Vector2(20f, -44f);
        tGo.GetComponent<RectTransform>().offsetMax = new Vector2(-60f, -8f);
        tGo.alignment = TextAnchor.MiddleLeft;

        var stGo = CreateText(box.transform, "SubTitle", "Thí Luyện Dị Giới - Thu Thập Trân Bảo & Đốt Cháy Tiến Trình", 13, FontStyle.Normal, new Vector2(0f, -56f), new Color(0.7f, 0.75f, 0.85f));
        stGo.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        stGo.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        stGo.GetComponent<RectTransform>().offsetMin = new Vector2(20f, -68f);
        stGo.GetComponent<RectTransform>().offsetMax = new Vector2(-20f, -44f);
        stGo.alignment = TextAnchor.MiddleLeft;

        // 6. StatsBar (Ví Cơ Duyên & Lượt Miễn Phí)
        var statsBar = new GameObject("StatsBar", typeof(RectTransform), typeof(Image));
        statsBar.transform.SetParent(box.transform, false);
        var srt = statsBar.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.offsetMin = new Vector2(16f, -132f);
        srt.offsetMax = new Vector2(-16f, -74f);
        statsBar.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.96f);

        walletText = CreateText(statsBar.transform, "Wallet", "✨ Cơ Duyên: 0 CD", 15, FontStyle.Bold, Vector2.zero, new Color(1f, 0.85f, 0.3f));
        var wrt = walletText.GetComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0f, 0f);
        wrt.anchorMax = new Vector2(0.5f, 1f);
        wrt.offsetMin = new Vector2(14f, 0f);
        wrt.offsetMax = new Vector2(-10f, 0f);
        walletText.alignment = TextAnchor.MiddleLeft;

        freeAttemptsText = CreateText(statsBar.transform, "FreeAttempts", "Lượt Miễn Phí: 1/1", 15, FontStyle.Bold, Vector2.zero, new Color(0.3f, 1f, 0.5f));
        var frt = freeAttemptsText.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0.5f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = new Vector2(10f, 0f);
        frt.offsetMax = new Vector2(-14f, 0f);
        freeAttemptsText.alignment = TextAnchor.MiddleRight;

        // 7. ScrollView danh sách 5 Bí Cảnh
        var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(box.transform, false);
        var scrt = scrollGo.GetComponent<RectTransform>();
        scrt.anchorMin = Vector2.zero;
        scrt.anchorMax = Vector2.one;
        scrt.offsetMin = new Vector2(16f, 76f);
        scrt.offsetMax = new Vector2(-16f, -140f);
        scrollGo.GetComponent<Image>().color = Color.clear;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollGo.transform, false);
        var vprt = viewport.GetComponent<RectTransform>();
        vprt.anchorMin = Vector2.zero;
        vprt.anchorMax = Vector2.one;
        vprt.offsetMin = vprt.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(4, 4, 6, 6);

        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.content = crt;
        scrollRect.viewport = vprt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Sinh 5 thẻ Bí Cảnh
        CreateRealmCards(content.transform);

        // 8. Nút Đóng Dưới Cùng
        var closeBottom = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        closeBottom.transform.SetParent(box.transform, false);
        var cbrt = closeBottom.GetComponent<RectTransform>();
        cbrt.anchorMin = new Vector2(0.2f, 0f);
        cbrt.anchorMax = new Vector2(0.8f, 0f);
        cbrt.offsetMin = new Vector2(0f, 14f);
        cbrt.offsetMax = new Vector2(0f, 66f);
        closeBottom.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.32f, 0.95f);
        var cbol = closeBottom.GetComponent<Outline>();
        cbol.effectColor = new Color(0.85f, 0.45f, 1f, 0.7f);
        cbol.effectDistance = new Vector2(1f, -1f);

        var cbtxt = CreateText(closeBottom.transform, "Text", "ĐÓNG GIAO DIỆN", 15, FontStyle.Bold, Vector2.zero, Color.white);
        cbtxt.alignment = TextAnchor.MiddleCenter;
        closeBottom.GetComponent<Button>().onClick.AddListener(Close);

        modalOverlay.SetActive(false);
    }

    private void CreateRealmCards(Transform content)
    {
        enterButtons.Clear();

        CreateOneRealmCard(content, SecretRealmType.HordeSurvival,
            "⚔️ VẠN MA TRIỀU DÂNG (Sinh Tồn 30s)",
            "Yêu ma tràn ngập 4 phương! Dồn toàn lực tuyệt kỹ quét sạch bầy quái trước khi hết giờ.",
            "🎁 Thưởng: Rương Thảo Dược Bội Thu (Cỏ Ngưng Khí, Tuyết Liên, Linh Chi)",
            new Color(0.12f, 0.16f, 0.24f, 0.96f),
            new Color(0.9f, 0.3f, 0.3f, 1f));

        CreateOneRealmCard(content, SecretRealmType.BarrierBreak,
            "🗿 TRẢM PHÁ CẤM CHẾ (Dồn Sát Thương 20s)",
            "Thạch bi phù văn viễn cổ chứa bảo vật. Đập vỡ cấm chế nhận quặng quý và phôi pháp khí.",
            "🎁 Thưởng: Quặng Tinh Thiết & Phôi Pháp Khí Thượng Cổ",
            new Color(0.12f, 0.16f, 0.24f, 0.96f),
            new Color(0.2f, 0.65f, 1f, 1f));

        CreateOneRealmCard(content, SecretRealmType.DanCauldron,
            "🔮 ĐAN THẦN KỲ DUYÊN (Tam Đỉnh Tầm Bảo)",
            "Lạc vào Đan Các cổ tích! Giải mã sấm ngôn và chọn 1 trong 3 đỉnh đan để nổ hũ thần đan.",
            "🎁 Thưởng: Thượng Cổ Tuyệt Phẩm Đan, Thảo Dược & Cơ Duyên",
            new Color(0.12f, 0.16f, 0.24f, 0.96f),
            new Color(0.85f, 0.5f, 1f, 1f));

        CreateOneRealmCard(content, SecretRealmType.LightningDodge,
            "⚡ VƯỢT LÔI TRÌ (Né Sét Nhặt Tinh Cầu 25s)",
            "Sấm sét thiên kiếp giáng liên tục! Nhanh tay nhặt các hạt Lôi Tinh Cầu phát sáng trên sàn.",
            "🎁 Thưởng: Tu Vi Thuần Khiết & Tàn Quyển Trận Pháp",
            new Color(0.12f, 0.16f, 0.24f, 0.96f),
            new Color(0.3f, 0.9f, 0.9f, 1f));

        CreateOneRealmCard(content, SecretRealmType.SwordTomb,
            "🗡️ KIẾM CHỦNG CỔ THẦN (Rút Kiếm Cực Phẩm)",
            "3 lượt rút kiếm cổ! Canh đúng vạch Hoàng Kim ở giữa để rút ngay Phi Kiếm Bậc Cao.",
            "🎁 Thưởng: Phi Kiếm Cấp 4, 6 hoặc 8 thẳng vào kho đồ",
            new Color(0.12f, 0.16f, 0.24f, 0.96f),
            new Color(1f, 0.8f, 0.2f, 1f));
    }

    private void CreateOneRealmCard(Transform parent, SecretRealmType type, string title, string desc, string reward, Color bgColor, Color accentColor)
    {
        var card = new GameObject("Card_" + type, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        var crt = card.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(0f, 130f);
        var le = card.GetComponent<LayoutElement>();
        le.minHeight = 130f;
        le.preferredHeight = 130f;
        card.GetComponent<Image>().color = bgColor;
        var ol = card.GetComponent<Outline>();
        ol.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
        ol.effectDistance = new Vector2(1f, -1f);

        // Title
        var tTxt = CreateText(card.transform, "Title", title, 15, FontStyle.Bold, Vector2.zero, accentColor);
        var trt = tTxt.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = new Vector2(14f, -32f);
        trt.offsetMax = new Vector2(-130f, -4f);
        tTxt.alignment = TextAnchor.MiddleLeft;

        // Desc
        var dTxt = CreateText(card.transform, "Desc", desc, 12, FontStyle.Normal, Vector2.zero, new Color(0.85f, 0.85f, 0.9f));
        var drt = dTxt.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0f, 1f);
        drt.anchorMax = new Vector2(1f, 1f);
        drt.offsetMin = new Vector2(14f, -76f);
        drt.offsetMax = new Vector2(-130f, -34f);
        dTxt.alignment = TextAnchor.UpperLeft;

        // Reward Tag
        var rTxt = CreateText(card.transform, "Reward", reward, 11, FontStyle.Italic, Vector2.zero, new Color(1f, 0.85f, 0.35f));
        var rrt = rTxt.GetComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0f, 0f);
        rrt.anchorMax = new Vector2(1f, 0f);
        rrt.offsetMin = new Vector2(14f, 6f);
        rrt.offsetMax = new Vector2(-130f, 44f);
        rTxt.alignment = TextAnchor.MiddleLeft;

        // Button Enter
        var btnGo = new GameObject("BtnEnter", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(card.transform, false);
        var brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1f, 0.5f);
        brt.anchorMax = new Vector2(1f, 0.5f);
        brt.anchoredPosition = new Vector2(-68f, 0f);
        brt.sizeDelta = new Vector2(116f, 50f);
        btnGo.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.25f, 0.95f);
        var bol = btnGo.GetComponent<Outline>();
        bol.effectColor = new Color(0.4f, 1f, 0.6f, 0.8f);
        bol.effectDistance = new Vector2(1f, -1f);

        var btxt = CreateText(btnGo.transform, "Text", "VÀO ẢI", 13, FontStyle.Bold, Vector2.zero, Color.white);
        btxt.alignment = TextAnchor.MiddleCenter;

        var btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            OnEnterRealmClicked(type);
        });
        enterButtons.Add(btn);
    }

    private void OnEnterRealmClicked(SecretRealmType type)
    {
        if (SecretRealmService.Instance == null) return;

        if (!SecretRealmService.Instance.CanEnter())
        {
            if (GameLogger.Instance != null)
                GameLogger.Instance.Log("❌ Đã hết lượt miễn phí! Cần 5 Cơ Duyên (CD) để mở cổng.", Color.yellow);
            return;
        }

        if (SecretRealmService.Instance.TryConsumeEntry(out bool usedFree))
        {
            Close();
            if (SecretRealmPlayController.Instance == null)
            {
                var pcGo = GameObject.Find("Canvas");
                if (pcGo != null && pcGo.GetComponent<SecretRealmPlayController>() == null)
                    pcGo.AddComponent<SecretRealmPlayController>();
            }
            SecretRealmPlayController.Instance?.LaunchMiniGame(type);
        }
    }

    public void RefreshUI()
    {
        if (SecretRealmService.Instance == null) return;

        bool hasFree = SecretRealmService.Instance.HasFreeAttempt();
        int cd = EconomyManager.Instance != null ? EconomyManager.Instance.currentCoDuyen : 0;

        if (walletText != null)
        {
            walletText.text = $"✨ Ví Cơ Duyên: <color=#FFE666>{cd} CD</color>";
        }

        if (freeAttemptsText != null)
        {
            if (hasFree)
            {
                freeAttemptsText.text = "Lượt Miễn Phí: <color=#55FF88>1 / 1</color>";
            }
            else
            {
                freeAttemptsText.text = "Phí Vào Cổng: <color=#FFB84D>5 CD / Lần</color>";
            }
        }

        // Cập nhật text trên các nút vào ải
        foreach (var b in enterButtons)
        {
            if (b == null) continue;
            var t = b.GetComponentInChildren<Text>();
            if (t == null) continue;

            if (hasFree)
            {
                t.text = "<b>MIỄN PHÍ\n(1/1)</b>";
                b.GetComponent<Image>().color = new Color(0.15f, 0.45f, 0.25f, 0.95f);
                b.interactable = true;
            }
            else if (cd >= SecretRealmService.CO_DUYEN_ENTRY_COST)
            {
                t.text = "<b>VÀO ẢI\n(5 CD)</b>";
                b.GetComponent<Image>().color = new Color(0.45f, 0.3f, 0.12f, 0.95f);
                b.interactable = true;
            }
            else
            {
                t.text = "<b>CẦN 5 CD</b>";
                b.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.7f);
                b.interactable = false;
            }
        }
    }

    private void WireExistingModal()
    {
        if (modalOverlay == null) return;
        walletText = modalOverlay.transform.Find("ModalBox/StatsBar/Wallet")?.GetComponent<Text>();
        freeAttemptsText = modalOverlay.transform.Find("ModalBox/StatsBar/FreeAttempts")?.GetComponent<Text>();

        var btnX = modalOverlay.transform.Find("ModalBox/BtnCloseX")?.GetComponent<Button>();
        if (btnX != null)
        {
            btnX.onClick.RemoveAllListeners();
            btnX.onClick.AddListener(Close);
        }

        var btnBackdrop = modalOverlay.GetComponent<Button>();
        if (btnBackdrop != null)
        {
            btnBackdrop.onClick.RemoveAllListeners();
            btnBackdrop.onClick.AddListener(Close);
        }

        var btnCloseBottom = modalOverlay.transform.Find("ModalBox/CloseButton")?.GetComponent<Button>();
        if (btnCloseBottom != null)
        {
            btnCloseBottom.onClick.RemoveAllListeners();
            btnCloseBottom.onClick.AddListener(Close);
        }

        enterButtons.Clear();
        var content = modalOverlay.transform.Find("ModalBox/ScrollView/Viewport/Content");
        if (content != null)
        {
            SecretRealmType[] types = {
                SecretRealmType.HordeSurvival,
                SecretRealmType.BarrierBreak,
                SecretRealmType.DanCauldron,
                SecretRealmType.LightningDodge,
                SecretRealmType.SwordTomb
            };

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var card = content.Find("Card_" + type);
                if (card != null)
                {
                    if (type == SecretRealmType.SwordTomb)
                    {
                        var rewardLabel = card.Find("Reward")?.GetComponent<Text>();
                        if (rewardLabel != null) rewardLabel.text = "🎁 Thưởng: Phi Kiếm Cấp 4, 6 hoặc 8 thẳng vào kho đồ";
                    }
                    var btnEnter = card.Find("BtnEnter")?.GetComponent<Button>();
                    if (btnEnter != null)
                    {
                        btnEnter.onClick.RemoveAllListeners();
                        btnEnter.onClick.AddListener(() => OnEnterRealmClicked(type));
                        enterButtons.Add(btnEnter);
                    }
                }
            }
        }
    }

    private Text CreateText(Transform parent, string name, string val, int size, FontStyle style, Vector2 pos, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 30f);
        var txt = go.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.color = col;
        txt.text = val;
        return txt;
    }
}

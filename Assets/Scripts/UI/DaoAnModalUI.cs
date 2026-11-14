using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Giao diện Cây Kỹ Năng Đạo Ấn (Vận Khí) toàn diện chuẩn Commercial Mobile.
/// Thiết kế chuẩn UX Mobile Portrait 9:16: Header tinh gọn, Nút Đóng [X] góc trên,
/// Thanh trạng thái hợp nhất, Tab lọc ngang mượt mà, Thẻ Đạo Ấn sắc nét và SortingOrder = 90 chống đè.
/// </summary>
public class DaoAnModalUI : MonoBehaviour
{
    public static DaoAnModalUI Instance { get; private set; }
    public bool IsModalOpen
    {
        get
        {
            if (modalOverlay != null) return modalOverlay.activeSelf;
            var tr = GameObject.Find("Canvas/DaoAnModal");
            return tr != null && tr.activeSelf;
        }
    }

    private GameObject modalOverlay;
    internal Transform ModalTransform => modalOverlay != null ? modalOverlay.transform : null;
    private RectTransform contentContainer;
    private ScrollRect scrollRectComp;
    private Text walletText;
    private Text rankText;
    private Text activeBuffsText;
    private float nextRefresh;
    private string currentFilter = "all";
    private RealmId displayedRealm;
    private int displayedSubLevel;

    private readonly List<GameObject> spawnedNodeCards = new List<GameObject>();
    private readonly List<Button> filterButtons = new List<Button>();
    private readonly List<(string id, Button button)> buyButtons = new List<(string, Button)>();
    private readonly string[] filterKeys = { "all", "wealth", "sword", "cultivation", "alchemy", "formation", "body_artifact", "body" };
    private readonly string[] filterLabels = { "Tất Cả", "Tài Lộc", "Kiếm Đạo", "Tu Luyện", "Dược Linh", "Pháp Trận", "Pháp Khí", "Luyện Thể" };

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start() => BuildIfNeeded();
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BuildIfNeeded();

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private static RectTransform SetupRect(GameObject go, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    private Text CreateLabel(string name, Transform parent, string value, int size, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color, bool bold = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        SetupRect(go, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        var text = go.GetComponent<Text>();
        text.font = DefaultFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string title, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color bgColor, Color textColor, int fontSize = 20)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        SetupRect(go, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        go.GetComponent<Image>().color = bgColor;
        
        var ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(textColor.r, textColor.g, textColor.b, 0.5f);
        ol.effectDistance = new Vector2(1f, -1f);

        var btn = go.GetComponent<Button>();
        CreateLabel("Text", go.transform, title, fontSize, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, textColor, true);
        return btn;
    }

    public void BuildIfNeeded()
    {
        var canvas = GameObject.Find("Canvas")?.transform;
        if (canvas == null) return;

        // The avatar popup contains character details; Dao Van keeps its HUD entry.

        // 2. Phím tắt Đạo Vận trực tiếp trên Top HUD (MobileUIRoot) cạnh CharacterFrame (Top-Left)
        var mobileRoot = canvas.Find("MobileUIRoot");
        // The portrait reconstruction retains the original bound shortcut in its quick-action row.
        // Recreating the former direct-child badge produces a second, undersized hit target.
        if (mobileRoot != null && canvas.GetComponent<MobileUIReconstruction>() == null)
        {
            var existingBtn = mobileRoot.Find("BtnHUDDaoAn");
            GameObject btnGo = existingBtn != null ? existingBtn.gameObject : null;
            if (btnGo == null)
            {
                btnGo = new GameObject("BtnHUDDaoAn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(DaoAnButtonHandler));
                btnGo.transform.SetParent(mobileRoot, false);
                var rt = btnGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(226f, -10f);
                rt.sizeDelta = new Vector2(108f, 48f);
            }

            var img = btnGo.GetComponent<Image>();
            if (img == null) img = btnGo.AddComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

            var ol = btnGo.GetComponent<Outline>();
            if (ol == null) ol = btnGo.AddComponent<Outline>();
            ol.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.85f);
            ol.effectDistance = new Vector2(1f, -1f);

            var txt = btnGo.GetComponentInChildren<Text>();
            if (txt == null)
            {
                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                txtObj.transform.SetParent(btnGo.transform, false);
                var trt = txtObj.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;
                txt = txtObj.GetComponent<Text>();
            }
            txt.font = DefaultFont;
            txt.text = "<b><color=#FFE666>ĐẠO VẬN</color></b>";
            txt.fontSize = 15;
            txt.alignment = TextAnchor.MiddleCenter;

            if (btnGo.GetComponent<DaoAnButtonHandler>() == null)
                btnGo.AddComponent<DaoAnButtonHandler>();
        }

        // Kiểm tra xem đã có Modal trong Hierarchy chưa
        var existingModal = canvas.Find("DaoAnModal");
        if (existingModal != null)
        {
            modalOverlay = existingModal.gameObject;
            EnsureModalCanvasSorting(modalOverlay);
            WireModalComponents(modalOverlay);
            return;
        }

        // Tạo Modal mới nếu chưa có
        BuildFreshModal(canvas);
    }

    private void EnsureModalCanvasSorting(GameObject modal)
    {
        var canvasComp = modal.GetComponent<Canvas>();
        if (canvasComp != null)
        {
            var raycaster = modal.GetComponent<GraphicRaycaster>();
            if (raycaster != null) DestroyImmediate(raycaster);
            DestroyImmediate(canvasComp);
        }
        modal.transform.SetAsLastSibling();
    }

    private void WireModalComponents(GameObject modal)
    {
        if (modal == null) return;
        modalOverlay = modal;

        var boxTr = modal.transform.Find("ModalBox");
        if (boxTr == null) return;

        var boxRt = boxTr.GetComponent<RectTransform>();
        if (boxRt != null)
        {
            boxRt.anchorMin = Vector2.zero;
            boxRt.anchorMax = Vector2.one;
            boxRt.offsetMin = Vector2.zero;
            boxRt.offsetMax = Vector2.zero;
        }

        var sf = boxTr.GetComponent<SafeAreaFitter>();
        if (sf == null) sf = boxTr.gameObject.AddComponent<SafeAreaFitter>();
        sf.Refresh();

        // Chuẩn hóa layout các phần tử cố định
        var borderTop = boxTr.Find("BorderTop")?.GetComponent<RectTransform>();
        if (borderTop != null)
        {
            borderTop.anchorMin = new Vector2(0f, 1f);
            borderTop.anchorMax = new Vector2(1f, 1f);
            borderTop.offsetMin = new Vector2(0f, -4f);
            borderTop.offsetMax = Vector2.zero;
        }

        var closeXRt = boxTr.Find("BtnCloseX")?.GetComponent<RectTransform>();
        if (closeXRt != null)
        {
            closeXRt.anchorMin = closeXRt.anchorMax = Vector2.one;
            closeXRt.offsetMin = new Vector2(-52f, -52f);
            closeXRt.offsetMax = new Vector2(-12f, -12f);
            var xTxt = closeXRt.GetComponentInChildren<Text>();
            if (xTxt != null) xTxt.text = "X";
        }

        var titleRt = boxTr.Find("Title")?.GetComponent<RectTransform>();
        if (titleRt != null)
        {
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(20f, -44f);
            titleRt.offsetMax = new Vector2(-60f, -8f);
            var tTxt = titleRt.GetComponent<Text>();
            if (tTxt != null) tTxt.alignment = TextAnchor.MiddleLeft;
        }

        var subTitleRt = boxTr.Find("SubTitle")?.GetComponent<RectTransform>();
        if (subTitleRt != null)
        {
            subTitleRt.anchorMin = new Vector2(0f, 1f);
            subTitleRt.anchorMax = new Vector2(1f, 1f);
            subTitleRt.offsetMin = new Vector2(20f, -68f);
            subTitleRt.offsetMax = new Vector2(-20f, -44f);
            var stTxt = subTitleRt.GetComponent<Text>();
            if (stTxt != null)
            {
                stTxt.text = "Cây Thiên Phú Bị Động - Vĩnh Viễn Không Mất Khi Độ Kiếp";
                stTxt.alignment = TextAnchor.MiddleLeft;
            }
        }

        var statsBarRt = boxTr.Find("StatsBar")?.GetComponent<RectTransform>();
        if (statsBarRt != null)
        {
            statsBarRt.anchorMin = new Vector2(0f, 1f);
            statsBarRt.anchorMax = new Vector2(1f, 1f);
            statsBarRt.offsetMin = new Vector2(16f, -132f);
            statsBarRt.offsetMax = new Vector2(-16f, -74f);
        }

        var filterScrollRt = boxTr.Find("FilterScrollView")?.GetComponent<RectTransform>();
        if (filterScrollRt != null)
        {
            filterScrollRt.anchorMin = new Vector2(0f, 1f);
            filterScrollRt.anchorMax = new Vector2(1f, 1f);
            filterScrollRt.offsetMin = new Vector2(16f, -188f);
            filterScrollRt.offsetMax = new Vector2(-16f, -138f);
        }

        walletText = boxTr.Find("StatsBar/Wallet")?.GetComponent<Text>();
        rankText = boxTr.Find("StatsBar/Rank")?.GetComponent<Text>();
        activeBuffsText = boxTr.Find("StatsBar/ActiveBuffs")?.GetComponent<Text>();
        
        var scrollTr = boxTr.Find("ScrollView");
        if (scrollTr != null)
        {
            scrollRectComp = scrollTr.GetComponent<ScrollRect>();
            EnsureScrollSurface(scrollRectComp);
            var sRt = scrollTr.GetComponent<RectTransform>();
            if (sRt != null)
            {
                sRt.anchorMin = Vector2.zero;
                sRt.anchorMax = Vector2.one;
                sRt.offsetMin = new Vector2(16f, 76f);
                sRt.offsetMax = new Vector2(-16f, -194f);
            }
        }

        var contentTr = boxTr.Find("ScrollView/Viewport/Content");
        if (contentTr != null) contentContainer = contentTr.GetComponent<RectTransform>();

        var filterContentTr = boxTr.Find("FilterScrollView/Viewport/Content") ?? boxTr.Find("FilterBar");
        if (filterContentTr != null)
        {
            var hlg = filterContentTr.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.padding = new RectOffset(4, 4, 3, 3);
            }
            WireFilterButtons(filterContentTr);
        }

        // Nút Close góc trên
        var closeX = boxTr.Find("BtnCloseX")?.GetComponent<Button>();
        if (closeX != null)
        {
            closeX.onClick.RemoveAllListeners();
            closeX.onClick.AddListener(Close);
        }

        // Nút Close dưới đáy
        var closeBottom = boxTr.Find("CloseButton")?.GetComponent<Button>();
        if (closeBottom != null)
        {
            var cbRt = closeBottom.GetComponent<RectTransform>();
            if (cbRt != null)
            {
                cbRt.anchorMin = new Vector2(0.2f, 0f);
                cbRt.anchorMax = new Vector2(0.8f, 0f);
                cbRt.offsetMin = new Vector2(0f, 14f);
                cbRt.offsetMax = new Vector2(0f, 66f);
            }
            closeBottom.onClick.RemoveAllListeners();
            closeBottom.onClick.AddListener(Close);
        }
    }

    private static void EnsureScrollSurface(ScrollRect scroll)
    {
        if (scroll == null || scroll.viewport == null) return;
        var viewport = scroll.viewport;
        var surface = viewport.GetComponent<Graphic>();
        if (surface == null)
        {
            surface = viewport.gameObject.AddComponent<Image>();
            surface.color = Color.clear;
        }
        surface.raycastTarget = true;
    }

    public void BuildFreshModal(Transform canvas)
    {
        // 3. Modal Overlay toàn màn hình
        modalOverlay = new GameObject("DaoAnModal", typeof(RectTransform), typeof(Image), typeof(Button));
        SetupRect(modalOverlay, canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        modalOverlay.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.07f, 0.90f);
        modalOverlay.GetComponent<Button>().onClick.AddListener(Close);
        modalOverlay.transform.SetAsLastSibling();

        // 4. Modal Box trung tâm với SafeAreaFitter (bảo đảm an toàn tuyệt đối trên màn hình có notch / tai thỏ)
        var box = new GameObject("ModalBox", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(SafeAreaFitter));
        var boxRect = SetupRect(box, modalOverlay.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        box.GetComponent<SafeAreaFitter>().Refresh();
        box.GetComponent<Image>().color = new Color(0.07f, 0.10f, 0.16f, 0.99f);

        var boxOl = box.GetComponent<Outline>();
        boxOl.effectColor = new Color(0.96f, 0.78f, 0.26f, 0.50f);
        boxOl.effectDistance = new Vector2(1f, -1f);

        // Viền trên hoàng kim
        var borderTop = new GameObject("BorderTop", typeof(RectTransform), typeof(Image));
        SetupRect(borderTop, box.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -4f), Vector2.zero);
        borderTop.GetComponent<Image>().color = new Color(0.96f, 0.78f, 0.26f, 1f);

        // 5. Nút Đóng [X] ở góc trên bên phải
        var closeX = CreateButton("BtnCloseX", box.transform, "X",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -52f), new Vector2(-12f, -12f),
            new Color(0.18f, 0.22f, 0.30f, 0.95f), new Color(1f, 0.45f, 0.45f, 1f), 20);
        closeX.onClick.AddListener(Close);

        // 6. Tiêu đề & Phụ đề
        CreateLabel("Title", box.transform, "THIÊN PHÚ: ĐẠO VẬN", 22, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -44f), new Vector2(-60f, -8f), new Color(0.96f, 0.78f, 0.26f), true);

        CreateLabel("SubTitle", box.transform, "Cây Thiên Phú Bị Động - Vĩnh Viễn Không Mất Khi Độ Kiếp", 13, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -68f), new Vector2(-20f, -44f), new Color(0.65f, 0.72f, 0.82f));

        // 7. Thanh Trạng Thái Hợp Nhất (StatsBar: Linh Thạch + Đạo Vị + Tóm tắt Buff)
        var statsBar = new GameObject("StatsBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        SetupRect(statsBar, box.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -132f), new Vector2(-16f, -74f));
        statsBar.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.10f, 0.96f);
        
        var sbOl = statsBar.GetComponent<Outline>();
        sbOl.effectColor = new Color(0.20f, 0.85f, 0.45f, 0.35f);
        sbOl.effectDistance = new Vector2(1f, -1f);

        walletText = CreateLabel("Wallet", statsBar.transform, "Linh Thạch: 0", 15, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0.5f, 1f), new Vector2(14f, 0f), new Vector2(-10f, 0f), new Color(0.96f, 0.78f, 0.26f), true);

        rankText = CreateLabel("Rank", statsBar.transform, "Đạo Vị: Sơ Sinh", 15, TextAnchor.MiddleRight,
            new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(-14f, 0f), new Color(0.20f, 0.85f, 0.45f), true);

        activeBuffsText = CreateLabel("ActiveBuffs", statsBar.transform, "0 Đạo Ấn Hoạt Hóa - Không Mất Khi Độ Kiếp", 12, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(10f, 0f), new Vector2(-10f, 0f), new Color(0.40f, 0.75f, 0.95f));

        // 8. Thanh Lọc Danh Mục Dạng Cuộn Ngang (Horizontal Filter ScrollView)
        var filterScroll = new GameObject("FilterScrollView", typeof(RectTransform), typeof(ScrollRect));
        SetupRect(filterScroll, box.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -188f), new Vector2(-16f, -138f));
        var fScroll = filterScroll.GetComponent<ScrollRect>();
        fScroll.horizontal = true;
        fScroll.vertical = false;
        fScroll.movementType = ScrollRect.MovementType.Clamped;

        var fViewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        SetupRect(fViewport, filterScroll.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fScroll.viewport = fViewport.GetComponent<RectTransform>();

        var fContent = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        var fcRt = SetupRect(fContent, fViewport.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        fcRt.pivot = new Vector2(0f, 0.5f);

        var hlg = fContent.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(4, 4, 3, 3);

        var fcsf = fContent.GetComponent<ContentSizeFitter>();
        fcsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fcsf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        fScroll.content = fcRt;

        WireFilterButtons(fContent.transform);

        // 9. Vùng cuộn ScrollView chứa các Node Đạo Ấn
        var scrollView = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        SetupRect(scrollView, box.transform, Vector2.zero, Vector2.one, new Vector2(16f, 76f), new Vector2(-16f, -194f));
        scrollRectComp = scrollView.GetComponent<ScrollRect>();
        scrollRectComp.horizontal = false;
        scrollRectComp.vertical = true;
        scrollRectComp.movementType = ScrollRect.MovementType.Clamped;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        SetupRect(viewport, scrollView.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        scrollRectComp.viewport = viewport.GetComponent<RectTransform>();

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentContainer = SetupRect(content, viewport.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        contentContainer.pivot = new Vector2(0.5f, 1f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.padding = new RectOffset(4, 4, 6, 6);

        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRectComp.content = contentContainer;
        EnsureScrollSurface(scrollRectComp);

        // 10. Nút đóng dưới đáy
        var closeBtn = CreateButton("CloseButton", box.transform, "ĐÓNG GIAO DIỆN",
            new Vector2(0.2f, 0f), new Vector2(0.8f, 0f), new Vector2(0f, 14f), new Vector2(0f, 66f),
            new Color(0.16f, 0.20f, 0.28f, 0.95f), new Color(0.90f, 0.92f, 0.95f, 1f), 15);
        closeBtn.onClick.AddListener(Close);

        modalOverlay.SetActive(false);
    }

    public void Open()
    {
        if (modalOverlay == null)
        {
            var tr = GameObject.Find("Canvas/DaoAnModal");
            if (tr != null) modalOverlay = tr.gameObject;
        }
        if (modalOverlay == null)
        {
            BuildIfNeeded();
        }
        if (modalOverlay == null) return;

        // Luôn luôn kết nối lại các component và button listeners để đảm bảo không bị mất
        WireModalComponents(modalOverlay);

        modalOverlay.SetActive(true);
        modalOverlay.transform.SetAsLastSibling();
        Refresh();
    }

    public void Close()
    {
        if (modalOverlay != null) modalOverlay.SetActive(false);
    }

    private void WireFilterButtons(Transform content)
    {
        var catalog = DaoAnProgressionService.Instance?.Catalog ?? Resources.Load<DaoAnCatalog>("Progression/DaoAnCatalog");
        filterButtons.Clear();
        for (int i = 0; i < filterKeys.Length; i++)
        {
            string key = filterKeys[i];
            int count = 0;
            if (catalog?.nodes != null)
                foreach (var node in catalog.nodes)
                    if (node != null && MatchesFilter(node, key)) count++;

            string label = $"{filterLabels[i]} ({count})";
            var button = content.Find("Tab_" + key)?.GetComponent<Button>();
            if (button == null)
                button = CreateButton("Tab_" + key, content, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white, Color.white, 13);
            else
            {
                var text = button.GetComponentInChildren<Text>();
                if (text != null) text.text = label;
            }

            var layout = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = key == "all" ? 135f : 114f;
            layout.preferredHeight = layout.minHeight = 44f;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SetFilter(key));
            filterButtons.Add(button);
        }
        UpdateFilterColors();
    }

    private static bool MatchesFilter(DaoAnDefinition node, string key) =>
        key == "all" || string.Equals(node.branch, key, StringComparison.OrdinalIgnoreCase);

    public void SetFilter(string filter)
    {
        currentFilter = filter;
        if (filterButtons.Count == 0)
        {
            if (modalOverlay == null)
            {
                var tr = GameObject.Find("Canvas/DaoAnModal");
                if (tr != null) modalOverlay = tr.gameObject;
            }
            if (modalOverlay != null) WireModalComponents(modalOverlay);
        }
        UpdateFilterColors();
        Refresh();
        if (scrollRectComp != null) scrollRectComp.verticalNormalizedPosition = 1f;
    }

    private void UpdateFilterColors()
    {
        for (int i = 0; i < filterButtons.Count && i < filterKeys.Length; i++)
        {
            if (filterButtons[i] == null) continue;
            bool isActive = (filterKeys[i] == currentFilter);
            filterButtons[i].GetComponent<Image>().color = isActive
                ? new Color(0.18f, 0.32f, 0.52f, 0.98f)
                : new Color(0.09f, 0.13f, 0.20f, 0.90f);
            var txt = filterButtons[i].GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.color = isActive
                    ? new Color(0.96f, 0.78f, 0.26f, 1f)
                    : new Color(0.65f, 0.72f, 0.80f, 1f);
            }
        }

    }

    private void Update()
    {
        if (!IsModalOpen || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.5f;
        var cultivation = CultivationManager.Instance;
        if (cultivation != null && (cultivation.CurrentRealm != displayedRealm || cultivation.CurrentSubLevel != displayedSubLevel))
        {
            Refresh();
            return;
        }
        UpdateWalletAndRankOnly();
        UpdateBuyButtons();
    }

    private void UpdateBuyButtons()
    {
        var service = DaoAnProgressionService.Instance;
        foreach (var (id, button) in buyButtons)
        {
            bool canBuy = service != null && service.CanPurchase(id);
            button.interactable = canBuy;
            button.image.color = canBuy ? new Color(0.12f, 0.48f, 0.25f, 0.98f) : new Color(0.22f, 0.24f, 0.28f, 0.8f);
        }
    }

    private void UpdateWalletAndRankOnly()
    {
        var service = DaoAnProgressionService.Instance;
        if (service == null) return;

        if (walletText != null && EconomyManager.Instance != null)
        {
            walletText.text = "Linh Thạch: " + EconomyManager.FormatNumber(EconomyManager.Instance.currentLinhThach);
        }

        var nodes = service != null ? service.Catalog?.nodes : Resources.Load<DaoAnCatalog>("Progression/DaoAnCatalog")?.nodes;
        if (nodes == null) return;

        int totalLv = 0;
        int maxTotal = 0;
        int activeCount = 0;
        foreach (var node in nodes)
        {
            if (node == null) continue;
            int lv = service != null ? service.GetLevel(node.id) : 0;
            totalLv += lv;
            maxTotal += node.MaxLevel;
            if (lv > 0) activeCount++;
        }

        string rankTitle = "Sơ Sinh";
        if (totalLv >= 40) rankTitle = "Đạo Tổ";
        else if (totalLv >= 25) rankTitle = "Thông Huyền";
        else if (totalLv >= 15) rankTitle = "Tề Thiên";
        else if (totalLv >= 8) rankTitle = "Đại Thịnh";
        else if (totalLv >= 3) rankTitle = "Tiểu Thịnh";

        if (rankText != null) rankText.text = $"Đạo Vị: {rankTitle} ({totalLv}/{maxTotal})";
        if (activeBuffsText != null) activeBuffsText.text = $"{activeCount} Đạo Ấn Hoạt Hóa ({totalLv} Cấp) • Không Mất Khi Độ Kiếp";
    }

    public void Refresh()
    {
        if (modalOverlay == null)
        {
            var tr = GameObject.Find("Canvas/DaoAnModal");
            if (tr != null) modalOverlay = tr.gameObject;
        }
        if (contentContainer == null && modalOverlay != null)
        {
            WireModalComponents(modalOverlay);
        }

        var service = DaoAnProgressionService.Instance;
        var catalog = service != null ? service.Catalog : Resources.Load<DaoAnCatalog>("Progression/DaoAnCatalog");
#if UNITY_EDITOR
        if (catalog == null)
            catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<DaoAnCatalog>("Assets/Resources/Progression/DaoAnCatalog.asset");
#endif
        if (catalog == null || contentContainer == null)
        {
            Debug.LogWarning($"[DaoAn] Refresh aborted: catalog={(catalog != null)}, contentContainer={(contentContainer != null)}");
            return;
        }

        UpdateWalletAndRankOnly();

        var cultivation = CultivationManager.Instance;
        displayedRealm = cultivation != null ? cultivation.CurrentRealm : RealmId.LuyenKhi;
        displayedSubLevel = cultivation != null ? cultivation.CurrentSubLevel : 1;
        float scrollPosition = scrollRectComp != null ? scrollRectComp.verticalNormalizedPosition : 1f;

        // 1. Dọn sạch triệt để toàn bộ children cũ trong contentContainer
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = contentContainer.GetChild(i).gameObject;
            child.SetActive(false);
            child.transform.SetParent(null);
            if (!Application.isPlaying)
                DestroyImmediate(child);
            else
                Destroy(child);
        }
        spawnedNodeCards.Clear();
        buyButtons.Clear();

        var nodes = catalog.nodes;
        if (nodes == null) return;

        // 2. Nhóm theo 6 Tầng và lọc chính xác theo branch
        int renderedCount = 0;
        for (int tier = 1; tier <= 6; tier++)
        {
            int currentTier = tier;
            var tierNodes = new List<DaoAnDefinition>();
            foreach (var node in nodes)
            {
                if (node == null || node.tier != currentTier) continue;
                if (MatchesFilter(node, currentFilter))
                {
                    tierNodes.Add(node);
                }
            }

            if (tierNodes.Count == 0) continue;

            // Header cho từng Tầng
            string tierTitle = tierNodes[0].tierName;
            var tierHeader = new GameObject("TierHeader_" + tier, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            tierHeader.transform.SetParent(contentContainer, false);
            tierHeader.GetComponent<Image>().color = new Color(0.12f, 0.17f, 0.25f, 0.90f);
            var thLe = tierHeader.GetComponent<LayoutElement>();
            thLe.minHeight = 32f;
            thLe.preferredHeight = 32f;

            CreateLabel("TierText", tierHeader.transform, tierTitle.ToUpper(), 14, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.64f, 0.61f, 0.99f), true);
            spawnedNodeCards.Add(tierHeader);

            // Sinh card cho từng node trong Tầng
            foreach (var node in tierNodes)
            {
                var card = CreateNodeCard(node, service);
                if (card != null)
                {
                    card.transform.SetParent(contentContainer, false);
                    spawnedNodeCards.Add(card);
                    renderedCount++;
                }
            }
        }

        // 3. Thông báo nếu bộ lọc không có node nào
        if (renderedCount == 0)
        {
            var emptyNotice = new GameObject("EmptyNotice", typeof(RectTransform), typeof(LayoutElement));
            emptyNotice.transform.SetParent(contentContainer, false);
            var le = emptyNotice.GetComponent<LayoutElement>();
            le.minHeight = 100f;
            le.preferredHeight = 100f;
            CreateLabel("Text", emptyNotice.transform, "Chưa có Đạo Ấn nào thuộc nhánh này.", 15, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.65f, 0.72f, 0.80f));
            spawnedNodeCards.Add(emptyNotice);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        Canvas.ForceUpdateCanvases();
        if (scrollRectComp != null)
        {
            scrollRectComp.StopMovement();
            scrollRectComp.verticalNormalizedPosition = Mathf.Clamp01(scrollPosition);
        }
        UpdateBuyButtons();

        Debug.Log($"[DaoAn] SetFilter('{currentFilter}'): rendered {renderedCount} nodes ({spawnedNodeCards.Count} elements total).");
    }

    private GameObject CreateNodeCard(DaoAnDefinition node, DaoAnProgressionService service)
    {
        int level = service != null ? service.GetLevel(node.id) : 0;
        bool isMax = (level >= node.MaxLevel);

        // Kiểm tra prerequisite
        bool prereqMet = true;
        string prereqReason = "";
        if (!string.IsNullOrEmpty(node.prerequisiteId) && service != null)
        {
            int prereqLv = service.GetLevel(node.prerequisiteId);
            if (prereqLv < Mathf.Max(1, node.prerequisiteLevel))
            {
                prereqMet = false;
                var parentNode = service.Catalog != null ? service.Catalog.Find(node.prerequisiteId) : null;
                string pName = parentNode != null ? parentNode.displayName : node.prerequisiteId;
                prereqReason = $"Cần {pName} Lv.{node.prerequisiteLevel}";
            }
        }

        // Kiểm tra Cảnh Giới
        bool realmMet = true;
        string realmReason = "";
        if (node.minRealms != null && level < node.minRealms.Length)
        {
            RealmId reqRealm = node.minRealms[level];
            int reqSub = (node.minSubLevels != null && level < node.minSubLevels.Length) ? node.minSubLevels[level] : 1;
            if (!FeatureUnlockService.IsCultivationReached(reqRealm, reqSub))
            {
                realmMet = false;
                realmReason = $"Cần {CultivationDisplay.RealmName(reqRealm)} tầng {reqSub}";
            }
        }

        bool isLocked = (!prereqMet || !realmMet) && !isMax;
        string lockText = !realmMet ? realmReason : prereqReason;

        float cost = !isMax && level < node.costs.Length ? node.costs[level] : 0f;

        // Tạo Card GameObject
        var card = new GameObject("Card_" + node.id, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        var cLe = card.GetComponent<LayoutElement>();
        cLe.minHeight = 78f;
        cLe.preferredHeight = 78f;
        card.GetComponent<Image>().color = isMax
            ? new Color(0.10f, 0.16f, 0.14f, 0.95f)
            : (isLocked ? new Color(0.06f, 0.08f, 0.12f, 0.90f) : new Color(0.09f, 0.13f, 0.19f, 0.95f));

        var cOl = card.GetComponent<Outline>();
        cOl.effectColor = isMax ? new Color(0.96f, 0.78f, 0.26f, 0.35f) : new Color(0.20f, 0.30f, 0.42f, 0.35f);
        cOl.effectDistance = new Vector2(1f, -1f);

        // Icon Box (Trái)
        var iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image), typeof(Outline));
        SetupRect(iconBox, card.transform, new Vector2(0.02f, 0.15f), new Vector2(0.13f, 0.85f), Vector2.zero, Vector2.zero);
        iconBox.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
        var ibOl = iconBox.GetComponent<Outline>();
        ibOl.effectColor = new Color(0.30f, 0.45f, 0.65f, 0.5f);
        ibOl.effectDistance = new Vector2(1f, -1f);

        string displayIcon = !string.IsNullOrEmpty(node.displayName) ? node.displayName.Substring(0, 1) : "A";
        CreateLabel("Icon", iconBox.transform, displayIcon, 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.96f, 0.78f, 0.26f), true);

        // Thông tin text (Giữa)
        var infoBox = new GameObject("InfoBox", typeof(RectTransform));
        SetupRect(infoBox, card.transform, new Vector2(0.15f, 0.05f), new Vector2(0.73f, 0.95f), Vector2.zero, Vector2.zero);

        // Tên node + Level
        CreateLabel("Name", infoBox.transform, $"{node.displayName}  [Lv.{level}/{node.MaxLevel}]", 16, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.66f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, isMax ? new Color(0.96f, 0.78f, 0.26f) : Color.white, true);

        // Mô tả
        CreateLabel("Desc", infoBox.transform, node.description, 13, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.36f), new Vector2(1f, 0.66f), Vector2.zero, Vector2.zero, new Color(0.70f, 0.78f, 0.86f));

        // Hiệu ứng hiện tại hoặc lý do khóa
        if (isLocked)
        {
            CreateLabel("LockReason", infoBox.transform, lockText, 13, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0.36f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.42f, 0.42f));
        }
        else
        {
            string unit = (node.unit ?? "").Trim();
            bool isPercent = unit.StartsWith("%", StringComparison.Ordinal);
            string current = FormatBonus(node.GetTotalBonus(level), isPercent);
            string next = isMax ? "" : $" → {FormatBonus(node.GetTotalBonus(level + 1), isPercent)}";
            string effectText = $"{current}{next} {unit.TrimStart('%').TrimStart()}";
            CreateLabel("Effect", infoBox.transform, effectText, 13, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0.36f), Vector2.zero, Vector2.zero, level > 0 ? new Color(0.20f, 0.85f, 0.45f) : new Color(0.55f, 0.62f, 0.70f));
        }

        // Nút nâng cấp (Phải)
        string btnLabel = isMax ? "Tối đa" : (isLocked ? "Chưa mở" : $"{EconomyManager.FormatNumber(cost)} LT");
        Color btnBg = isMax ? new Color(0.18f, 0.24f, 0.22f, 0.8f) : new Color(0.14f, 0.17f, 0.22f, 0.8f);

        var buyBtn = CreateButton("BtnBuy", card.transform, btnLabel,
            new Vector2(0.75f, 0.20f), new Vector2(0.98f, 0.80f), Vector2.zero, Vector2.zero,
            btnBg, isMax ? new Color(0.96f, 0.78f, 0.26f) : (isLocked ? new Color(0.6f, 0.65f, 0.7f) : Color.white), 15);

        buyBtn.interactable = false;
        if (!isMax && !isLocked) buyButtons.Add((node.id, buyBtn));
        buyBtn.onClick.AddListener(() =>
        {
            var activeService = DaoAnProgressionService.Instance ?? service;
            if (activeService == null)
            {
                Debug.LogWarning("[DaoAn] Service is null!");
                return;
            }
            if (activeService.TryPurchase(node.id))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSFX();
                GameLogger.Instance?.Log($"Khai mở Đạo Ấn: {node.displayName} Cấp {activeService.GetLevel(node.id)}!", Color.green);
                Refresh();
            }
        });

        return card;
    }

    private static string FormatBonus(float value, bool isPercent) => isPercent ? $"{value:0.##%}" : $"{value:0.##}";
}

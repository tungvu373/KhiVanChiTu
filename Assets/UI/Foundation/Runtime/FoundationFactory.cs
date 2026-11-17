using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    public enum FoundationKind { PrimaryButton, SecondaryButton, ScreenHeader, TabBar, ResourceDisplay, ItemSlot, ItemCard, ProgressBar, StatRow, FullScreenPanel, BottomSheet, RewardModal, ConfirmDialog, Tooltip, Toast }

    /// <summary>Only builds under an explicit parent. Never discovers or edits a gameplay Canvas.</summary>
    public sealed class FoundationFactory
    {
        public readonly FoundationTheme theme;
        public FoundationFactory(FoundationTheme value) { theme = value; }
        public static RectTransform Rect(string name, Transform parent)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            if (parent != null) r.SetParent(parent, false);
            r.sizeDelta = new Vector2(900, 132); return r;
        }
        public static void Fill(RectTransform r, float left = 0, float bottom = 0, float right = 0, float top = 0)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, -top); }
        public UnityEngine.UI.Image Surface(RectTransform r, Color color, bool hit = false)
        {
            var image = r.gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = theme.surfaceSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced; image.color = color; image.raycastTarget = hit;
            return image;
        }
        public TMP_Text Text(string name, Transform parent, string value, float size, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
        {
            var r = Rect(name, parent); Fill(r, theme.padding, 12, theme.padding, 12);
            var text = r.gameObject.AddComponent<TextMeshProUGUI>(); text.font = theme.bodyFont;
            text.fontSize = size; text.text = value; text.color = theme.ivory; text.alignment = alignment;
            text.raycastTarget = false; text.richText = false; text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Truncate;
            return text;
        }
        public FoundationAction Action(string name, Transform parent, string caption, bool primary = false)
        {
            var r = Rect(name, parent); var image = Surface(r, primary ? theme.gold : theme.raised, true);
            var button = r.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.9f, 1f, .95f); colors.pressedColor = new Color(.68f, .78f, .73f); colors.disabledColor = new Color(.55f,.55f,.55f,1); colors.fadeDuration = .12f; button.colors = colors;
            var action = r.gameObject.AddComponent<FoundationAction>(); action.button = button;
            action.label = Text("Label", r, caption, theme.bodySize, TextAlignmentOptions.Center);
            action.label.color = primary ? theme.ink : theme.ivory;
            return action;
        }
        public FoundationValue Value(string name, Transform parent)
        {
            var r = Rect(name, parent); Surface(r, theme.surface);
            var v = r.gameObject.AddComponent<FoundationValue>(); v.label = Text("Label", r, "Linh th\u1ea1ch", theme.bodySize);
            v.label.rectTransform.anchorMax = new Vector2(.52f, 1);
            v.value = Text("Value", r, "1.234.567", theme.bodySize, TextAlignmentOptions.MidlineRight);
            v.value.color = theme.gold; v.value.rectTransform.anchorMin = new Vector2(.52f, 0);
            return v;
        }
        public FoundationProgress Progress(Transform parent)
        {
            var r = Rect("ProgressBar", parent); Surface(r, theme.ink);
            var track = Rect("Track", r); Fill(track, 8, 8, 8, 8);
            var p = r.gameObject.AddComponent<FoundationProgress>(); p.fill = Rect("Fill", track);
            Fill(p.fill); Surface(p.fill, new Color32(40,99,79,255)); p.label = Text("Value", r, "72%", theme.bodySize, TextAlignmentOptions.Center); p.label.color = theme.ivory;
            p.Set(72, 100); return p;
        }
        public FoundationTabs Tabs(Transform parent)
        {
            var r = Rect("TabBar", parent); var row = r.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            row.spacing = 12; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = true;
            var t = r.gameObject.AddComponent<FoundationTabs>(); t.tabs = new FoundationAction[3];
            string[] names = { "T\u1ea5t c\u1ea3", "Trang b\u1ecb", "Nguy\u00ean li\u1ec7u" };
            for (int i = 0; i < 3; i++) t.tabs[i] = Action("Tab" + i, r, names[i]);
            return t;
        }
        public FoundationItem Item(Transform parent, bool card)
        {
            var a = Action(card ? "ItemCard" : "ItemSlot", parent, ""); var r = (RectTransform)a.transform;
            var item = r.gameObject.AddComponent<FoundationItem>(); item.action = a; a.label.gameObject.SetActive(false);
            item.title = Text("Title", r, "Thanh V\u00e2n Ki\u1ebfm", theme.headingSize);
            item.title.rectTransform.anchorMin = new Vector2(.18f,.48f); item.title.rectTransform.anchorMax = new Vector2(.86f,1);
            item.detail = Text("Detail", r, "B\u1eadc hi\u1ebfm \u00b7 C\u1ea5p 12", theme.captionSize);
            item.detail.color = theme.muted; item.detail.rectTransform.anchorMin = new Vector2(.18f,0); item.detail.rectTransform.anchorMax = new Vector2(.86f,.5f);
            item.badge = Text("Count", r, "12", theme.captionSize, TextAlignmentOptions.Center);
            item.badge.rectTransform.anchorMin = new Vector2(.82f,0); item.badge.rectTransform.anchorMax = Vector2.one;
            var icon = Rect("Icon", r); Fill(icon,16,16,16,16); icon.anchorMax = new Vector2(.2f,1); item.icon = Surface(icon,theme.gold);item.icon.type=UnityEngine.UI.Image.Type.Simple;item.icon.preserveAspect=true;
            var selected = Rect("Selected", r); Fill(selected,0,0,0,0); selected.anchorMax = new Vector2(.008f,1); item.selection = Surface(selected,theme.gold); item.SetSelected(false);
            if (!card)
            {
                r.sizeDelta = new Vector2(260,280);
                item.title.fontSize = theme.captionSize; Fill(item.title.rectTransform,16,12,16,0);
                item.title.rectTransform.anchorMax = new Vector2(1,.28f); item.title.alignment = TextAlignmentOptions.Center;
                item.detail.gameObject.SetActive(false);
                Fill(icon,56,88,56,40); icon.anchorMax = Vector2.one;
                Fill(item.badge.rectTransform,8,0,8,8); item.badge.rectTransform.anchorMin = new Vector2(.65f,.72f);
            }
            return item;
        }
        public FoundationPanel Panel(Transform parent, FoundationKind kind)
        {
            var r = Rect(kind.ToString(), parent); Fill(r); r.gameObject.SetActive(false);
            var p = r.gameObject.AddComponent<FoundationPanel>();
            var bg = Rect("Backdrop", r); Fill(bg); var bgImage = Surface(bg, new Color(0, .025f, .03f, .88f), true);
            p.backdrop = bg.gameObject.AddComponent<UnityEngine.UI.Button>(); p.backdrop.targetGraphic = bgImage;
            var box = Rect("Surface", r); Fill(box,36,36,36,36);
            if (kind == FoundationKind.BottomSheet) box.anchorMax = new Vector2(1,.72f);
            else if (kind != FoundationKind.FullScreenPanel) { box.anchorMin = new Vector2(0,.24f); box.anchorMax = new Vector2(1,.76f); }
            Surface(box,theme.surface,true);
            var title = Rect("Header",box); Fill(title,0,0,0,0); title.anchorMin = new Vector2(0,1); title.offsetMin = new Vector2(0,-144);
            p.title = Text("Title",title,kind.ToString(),theme.titleSize); p.title.font = theme.headingFont;
            var body = Rect("Scroll",box); Fill(body,32,196,32,160);
            var scroll = body.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport",body); Fill(viewport); Surface(viewport,Color.clear,true); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            p.content = Rect("Content",viewport); p.content.anchorMin = new Vector2(0,1); p.content.anchorMax = Vector2.one; p.content.pivot = new Vector2(.5f,1); p.content.sizeDelta = new Vector2(0,160); p.content.anchoredPosition = Vector2.zero;
            p.message = Text("Message",p.content,"N\u1ed9i dung h\u1ed9p tho\u1ea1i",theme.bodySize); Fill(p.message.rectTransform,0,0,0,0);
            // Content height is driven from the text's preferred height by a layout group.
            var layout = p.content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
            var contentFit = p.content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); contentFit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = p.content;
            var footer = Rect("Footer",box); Fill(footer,32,32,32,0); footer.anchorMax = new Vector2(1,0); footer.offsetMax = new Vector2(-32,164);
            var row = footer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); row.spacing = 24; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = true;
            p.cancel = Action("Close",footer,"\u0110\u00f3ng");
            p.confirm = Action("Confirm",footer,"X\u00e1c nh\u1eadn",true);
            p.dismissOnBackdrop = kind != FoundationKind.ConfirmDialog;
            return p;
        }
        public FoundationNotice Notice(Transform parent, bool tooltip)
        {
            var r = Rect(tooltip ? "Tooltip" : "Toast",parent); Surface(r,theme.raised);
            var group = r.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            var n = r.gameObject.AddComponent<FoundationNotice>(); n.text = Text("Text",r,"Th\u00f4ng b\u00e1o",theme.bodySize,TextAlignmentOptions.Center);
            r.gameObject.SetActive(false); return n;
        }
        public GameObject Create(FoundationKind kind, Transform parent)
        {
            switch (kind)
            {
                case FoundationKind.PrimaryButton: return Action("PrimaryButton",parent,"Ti\u1ebfp t\u1ee5c",true).gameObject;
                case FoundationKind.SecondaryButton: return Action("SecondaryButton",parent,"Quay l\u1ea1i").gameObject;
                case FoundationKind.ScreenHeader:
                    var h = Action("ScreenHeader",parent,"\u2039   T\u00e0ng Kinh C\u00e1c"); h.label.fontSize = theme.titleSize; h.label.font = theme.headingFont; return h.gameObject;
                case FoundationKind.TabBar: return Tabs(parent).gameObject;
                case FoundationKind.ResourceDisplay: return Value("ResourceDisplay",parent).gameObject;
                case FoundationKind.StatRow: return Value("StatRow",parent).gameObject;
                case FoundationKind.ProgressBar: return Progress(parent).gameObject;
                case FoundationKind.ItemSlot: return Item(parent,false).gameObject;
                case FoundationKind.ItemCard: return Item(parent,true).gameObject;
                case FoundationKind.Tooltip: return Notice(parent,true).gameObject;
                case FoundationKind.Toast: return Notice(parent,false).gameObject;
                default: return Panel(parent,kind).gameObject;
            }
        }
    }
}

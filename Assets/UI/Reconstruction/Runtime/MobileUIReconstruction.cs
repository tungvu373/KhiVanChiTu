using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Mobile-only presentation registry. Navigation and transactions remain with the existing owners.</summary>
[DefaultExecutionOrder(1800), DisallowMultipleComponent]
public sealed class MobileUIReconstruction : MonoBehaviour
{
    [Serializable] public sealed class Screen
    {
        public string key, layer;
        public GameObject target;
    }
    public MobileUIAssetLibrary assets;
    public JadeUITheme theme;
    public MobileDrawerUIController drawers;
    public MobileUINavigationManager navigation;
    public Screen[] screens;
    public RectTransform titleHeader, titleActions, titleFooter;
    public RectTransform swordContent, dismantleShortcut;
    readonly Dictionary<string, GameObject> registry = new Dictionary<string, GameObject>();
    Vector2Int lastViewport;
    Rect lastSafeArea;

    void Awake()
    {
        if (theme != null) theme.reconstruction = this;
        if (screens != null) foreach (var screen in screens)
            if (screen != null && screen.target != null && !string.IsNullOrEmpty(screen.key)) registry[screen.key] = screen.target;
    }
    void Start() { RefreshPresentation(); ApplyHudPresentation(); }
    void LateUpdate()
    {
        if(lastViewport.x==UnityEngine.Screen.width&&lastViewport.y==UnityEngine.Screen.height&&lastSafeArea==UnityEngine.Screen.safeArea)return;
        lastViewport=new Vector2Int(UnityEngine.Screen.width,UnityEngine.Screen.height);lastSafeArea=UnityEngine.Screen.safeArea;
        // Legacy Text caches its generated mesh at the previous Canvas scale. Refresh only on viewport/inset changes.
        foreach(var text in GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.cachedTextGenerator.Invalidate();text.SetAllDirty();}
        ApplyHudPresentation();
    }
    public GameObject Resolve(string key)
    {
        if (registry.TryGetValue(key, out var value) && value != null) return value;
        if (screens != null) foreach (var screen in screens) if (screen.key == key) return screen.target;
        return null;
    }
    public void RegisterRuntime(string key, GameObject target)
    { if (!string.IsNullOrEmpty(key) && target != null) registry[key] = target; }
    public void OpenWorkbench(int index) { if (drawers != null) drawers.OpenDrawer(index); }
    public bool Back() { return navigation != null && navigation.HandleBackNavigation(); }
    public void RefreshPresentation()
    {
        if (assets == null) return;
        foreach (var rt in GetComponentsInChildren<RectTransform>(true)) StyleElement(rt);
        MobileUIResponsiveRepairs.ApplyTree(this,transform);
    }

    public void ApplyHudPresentation()
    {
        var hud=GetComponentInChildren<PortraitHudController>(true);
        if(hud==null||assets==null)return;
        JadeUITheme.SetSurface(hud.profile.GetComponent<UnityEngine.UI.Image>(),assets.hud);
        foreach(var bar in new[]{hud.hp,hud.mana})
        {
            if(bar==null)continue;
            var image=bar.GetComponent<UnityEngine.UI.Image>();if(image!=null)JadeUITheme.SetSurface(image,assets.track);
        }
        // Keep the original synchronized Text objects/references, while presenting one equivalent mana value.
        if(hud.mana!=null&&hud.manaValue!=null)
            foreach(var text in hud.mana.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                if(text!=hud.manaValue)text.enabled=false;
    }

    public void StyleElement(RectTransform rt)
    {
        if (assets == null || rt == null || rt.name.StartsWith("P6")) return;
        var image = rt.GetComponent<UnityEngine.UI.Image>();
        if (image == null) return;
        string n = rt.name;
        if(n=="GridSlot")
        {
            foreach(var text in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                text.fontSize=text.transform.parent!=null&&(text.transform.parent.name=="CountBadge"||text.transform.parent.name=="Ribbon")?24:28;
                text.resizeTextForBestFit=false;text.cachedTextGenerator.Invalidate();text.SetAllDirty();
                if(text.transform.parent!=null&&(text.transform.parent.name=="CountBadge"||text.transform.parent.name=="Ribbon"))
                {
                    var badge=(RectTransform)text.transform.parent;badge.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,40);
                    if(badge.name=="CountBadge")
                    {badge.anchorMin=Vector2.zero;badge.anchorMax=new Vector2(1,0);badge.pivot=new Vector2(.5f,0);badge.anchoredPosition=new Vector2(0,6);badge.sizeDelta=new Vector2(-12,40);}
                    text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
                }
            }
        }
        var old = image.sprite;
        if (old != null)
        {
            Sprite next = null;
            switch (old.name)
            {
                case "Panel": case "Window": next = assets.panel; break;
                case "Slot": next = assets.slot; break;
                case "SelectedSlot": next = assets.selection; break;
                case "PrimaryButton": next = assets.upgrade; break;
                case "SecondaryButton": next = assets.navigation; break;
                case "ResourceBar": next = assets.track; break;
                case "Divider": next = assets.divider; break;
                case "SpiritStone": next = assets.stone; break;
                case "SpiritFlame": next = assets.fate; break;
                case "CultivationHalo": next = assets.cultivation; break;
            }
            if (next != null)
            {
                image.sprite = next;
                image.type = next.border.sqrMagnitude > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
                if (next.border.sqrMagnitude == 0) image.preserveAspect = true;
            }
        }
        if (n == "JadeOpaqueSurface") { image.enabled = false; image.raycastTarget = false; }
        if (n == "ModalBox" || n == "PauseBox" || n == "Box" || n == "DialogBox" || n == "GameplayChoicePanel" ||
            (n == "Card" && rt.parent != null && rt.parent.name == "SecretRealmResultModal"))
        {
            image.sprite=null;image.color=new Color32(10,31,34,255);
            MountFrame(rt, assets.modalPrefab);
            LayoutDecision(rt);
        }
        if (n.StartsWith("Drawer_") && rt.parent == drawers?.transform)
        {
            image.sprite=null;image.color=Color.clear;
            var view=MountFrame(rt, assets.workbenchPrefab);
            if(view!=null)
            {
                var frame=(RectTransform)view.transform;frame.offsetMin=new Vector2(0,192);frame.offsetMax=Vector2.zero;
                view.surface.enabled=true;view.surface.sprite=null;view.surface.color=new Color32(10,31,34,255);
            }
        }
        if(n=="ResourceBar")JadeUITheme.SetSurface(image,assets.hud);
        MobileUISlicedSurface.Configure(image);
    }

    static void LayoutDecision(RectTransform box)
    {
        var message=box.Find("Message") as RectTransform;
        var confirm=box.Find("ConfirmBtn") as RectTransform;var cancel=box.Find("CancelBtn") as RectTransform;
        if(message==null||confirm==null||cancel==null)return;
        box.anchorMin=box.anchorMax=box.pivot=new Vector2(.5f,.5f);box.sizeDelta=new Vector2(936,600);box.anchoredPosition=Vector2.zero;
        var fit=box.GetComponent<JadeModalFit>();if(fit!=null){fit.preferredScale=1;fit.Fit();}
        var title=box.Find("Title") as RectTransform;
        if(title!=null){StretchRow(title,1,-36,84,72);StyleLabel(title,54);}
        message.anchorMin=Vector2.zero;message.anchorMax=Vector2.one;message.offsetMin=new Vector2(72,216);message.offsetMax=new Vector2(-72,-156);StyleLabel(message,42);
        float width=(936-144)/2;
        foreach(var button in new[]{cancel,confirm})
        {
            bool right=button==confirm;button.anchorMin=button.anchorMax=button.pivot=new Vector2(right?1:0,0);
            button.anchoredPosition=new Vector2(right?-48:48,48);button.sizeDelta=new Vector2(width,144);
            foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.fontSize=42;text.resizeTextForBestFit=false;}
        }
    }
    static void StretchRow(RectTransform rt,float y,float position,float height,float inset)
    {rt.anchorMin=new Vector2(0,y);rt.anchorMax=new Vector2(1,y);rt.pivot=new Vector2(.5f,y);rt.anchoredPosition=new Vector2(0,position);rt.sizeDelta=new Vector2(-inset*2,height);}
    static void StyleLabel(RectTransform rt,int size)
    {var text=rt.GetComponent<UnityEngine.UI.Text>();if(text!=null){text.fontSize=size;text.resizeTextForBestFit=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;}}

    public MobileUIPanelView MountFrame(RectTransform target, MobileUIPanelView prefab)
    {
        if (prefab == null) return null;
        var existing = target.Find("P6Frame");
        if (existing != null) return existing.GetComponent<MobileUIPanelView>();
        MobileUIPanelView view;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            view = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab.gameObject, target)).GetComponent<MobileUIPanelView>();
        else
#endif
            view = Instantiate(prefab, target, false);
        view.name = "P6Frame";
        var rect = (RectTransform)view.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        // The retained root Image is the filled surface and event target; this instance supplies only ornament.
        if (view.surface != null) view.surface.enabled = false;
        if (view.title != null) view.title.gameObject.SetActive(false);
        view.SetEmblem(null);
        view.transform.SetAsLastSibling();
        return view;
    }
}

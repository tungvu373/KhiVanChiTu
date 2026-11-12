using System.Collections.Generic;
using UnityEngine;

/// <summary>Scene-owned presentation assets. Existing controllers retain data, events and state colours.</summary>
[DefaultExecutionOrder(900)]
[DisallowMultipleComponent]
public sealed class JadeUITheme : MonoBehaviour
{
    public Sprite panel, window, slot, selectedSlot, primaryButton, secondaryButton, resourceBar, divider;
    public Sprite[] navigationIcons;
    public Sprite spiritStone, spiritFlame, cultivationHalo;
    public static readonly Color Ivory = new Color32(241, 232, 207, 255);
    public static readonly Color Gold = new Color32(213, 181, 116, 255);
    public static readonly Color Jade = new Color32(110, 221, 182, 255);
    public static readonly Color Surface = new Color32(10, 31, 34, 250);
    private readonly HashSet<RectTransform> styled = new HashSet<RectTransform>();
    private readonly List<Transform> pending = new List<Transform>();

    private void Start() { StyleTree(transform); }
    public void RefreshPresentation() { styled.Clear(); StyleTree(transform); }
    public void StyleSkills()
    {
        var hud=GetComponentInChildren<MobileSkillHUD>(true);
        if(hud==null||navigationIcons==null||navigationIcons.Length<5)return;
        var buttons=new[]{hud.btnUltimate,hud.btnNormalSkills[0],hud.btnNormalSkills[1],hud.btnNormalSkills[2]};
        var icons=new[]{navigationIcons[0],navigationIcons[1],spiritFlame,navigationIcons[4]};
        for(int i=0;i<buttons.Length;i++)
        {
            var button=buttons[i];if(button==null)continue;
            SetSurface(button.GetComponent<UnityEngine.UI.Image>(),slot);
            var emblem=button.transform.Find("JadeSkillEmblem") as RectTransform;
            if(emblem==null)
            {var go=new GameObject("JadeSkillEmblem",typeof(RectTransform),typeof(UnityEngine.UI.Image));emblem=(RectTransform)go.transform;emblem.SetParent(button.transform,false);}
            emblem.anchorMin=emblem.anchorMax=emblem.pivot=new Vector2(.5f,.5f);emblem.anchoredPosition=Vector2.zero;emblem.sizeDelta=new Vector2(66,66);emblem.SetAsFirstSibling();
            var image=emblem.GetComponent<UnityEngine.UI.Image>();image.sprite=icons[i];image.preserveAspect=true;image.raycastTarget=false;
            var title=i==0?hud.txtUltiTitle:hud.txtNormalTitles[i-1];
            var cost=i==0?hud.txtUltiMana:hud.txtNormalCosts[i-1];
            SkillLabel(title,true);SkillLabel(cost,false);
        }
        if(hud.imgUltiFill!=null)
        {var fill=hud.imgUltiFill.rectTransform;fill.anchorMin=Vector2.zero;fill.anchorMax=new Vector2(1,0);fill.pivot=new Vector2(.5f,0);fill.sizeDelta=new Vector2(-12,8);fill.anchoredPosition=new Vector2(0,3);}
        if(hud.imgUltiGlow!=null){SetSurface(hud.imgUltiGlow,selectedSlot);hud.imgUltiGlow.raycastTarget=false;}
    }
    static void SkillLabel(UnityEngine.UI.Text text,bool top)
    {
        if(text==null)return;var rt=text.rectTransform;rt.anchorMin=new Vector2(0,top?1:0);rt.anchorMax=new Vector2(1,top?1:0);rt.pivot=new Vector2(.5f,top?1:0);rt.sizeDelta=new Vector2(-6,top?34:28);rt.anchoredPosition=new Vector2(0,top?-6:8);
        text.fontSize=top?26:22;text.resizeTextForBestFit=true;text.resizeTextMinSize=top?24:20;text.resizeTextMaxSize=text.fontSize;text.alignment=TextAnchor.MiddleCenter;
    }
    public void Queue(Transform scope) { if (scope != null && !pending.Contains(scope)) pending.Add(scope); }
    private void LateUpdate()
    {
        // Only hierarchy changes schedule work. No per-frame list scans or rebinding of gameplay.
        for (int i = 0; i < pending.Count; i++) if (pending[i] != null) StyleTree(pending[i]);
        pending.Clear();
    }
    public void StyleTree(Transform root)
    {
        styled.RemoveWhere(item => item == null);
        var all = root.GetComponentsInChildren<RectTransform>(true);
        foreach (var rt in all)
        {
            string n = rt.name;
            if (rt == transform || n == "Content" || n == "GameplayChoicePanel" || n == "PagesContainer" || n == "SlotContainer" || n.StartsWith("Page_") || n.Contains("Grid") || n.Contains("Filter"))
            {
                var observer = rt.GetComponent<JadeUIStyleScope>();
                if (observer == null) observer = rt.gameObject.AddComponent<JadeUIStyleScope>();
                observer.theme = this;
            }
            if (!styled.Add(rt)) continue;
            StyleElement(rt);
            JadeUIAdaptiveSkin.Apply(rt,this);
        }
    }
    public void StyleElement(RectTransform rt)
    {
        string n = rt.name;
        var text = rt.GetComponent<UnityEngine.UI.Text>();
        if (text != null)
        {
            text.font = UiTextStyle.BodyFont;
            var selectable = rt.GetComponent<UnityEngine.UI.Selectable>();
            text.raycastTarget = selectable != null && selectable.targetGraphic == text;
            if (rt.parent != null && rt.parent.GetComponent<UnityEngine.UI.Button>() != null &&
                !rt.parent.name.StartsWith("Dock_") && rt.GetComponentInParent<MobileSkillHUD>() == null &&
                (n=="Text"||n=="Label"||n=="Txt"||n=="X"||n=="TxtCost"))
            {
                rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(8,4);rt.offsetMax=new Vector2(-8,-4);
            }
            // InputField values keep their own text/caret semantics.
            if (text.GetComponentInParent<UnityEngine.UI.InputField>() == null)
                text.text = UiTextStyle.RemoveIcons(text.text);
            if (text.color.r > .7f && text.color.g > .7f && text.color.b > .7f) text.color = Ivory;
            if(rt.GetComponentInParent<GameplayChoicesService>()!=null)
            {text.fontSize=n=="Title"?36:30;text.resizeTextForBestFit=true;text.resizeTextMinSize=28;text.resizeTextMaxSize=text.fontSize;}
        }
        var image = rt.GetComponent<UnityEngine.UI.Image>();
        if (image == null) return;
        if(n=="BorderTop"||n=="BorderBottom"||n=="BorderLeft"||n=="BorderRight"||n=="BtnBorder"||n=="CardBorder"||n=="AudioBorder"||n=="ConfBorder")
        {image.enabled=false;image.raycastTarget=false;return;}
        var button = rt.GetComponent<UnityEngine.UI.Button>();
        bool isSlot = rt.GetComponent<MergeSlot>() != null || n == "GridSlot";
        bool card = n.StartsWith("Card_") || n == "StorageCard" || n.EndsWith("Vault") || n == "FurnaceCard" || n == "ResourceBar" || n=="HeaderBar" || n=="SaveInfoCard" || n=="AudioSection" || n=="WalletBar" || n=="PriorityFilterBar";
        bool box = n == "ModalBox" || n == "PauseBox" || n == "Box" || n == "DialogBox";
        if (isSlot) SetSurface(image, slot);
        else if (box) SetSurface(image, window);
        else if (card) SetSurface(image, panel);
        else if (button != null && !n.StartsWith("Dock_") && !n.Contains("Overlay") && !n.Contains("Backdrop") && rt.rect.width < 1050 && rt.rect.height < 350)
        {
            bool primary = n.Contains("Upgrade") || n.Contains("Brew") || n.Contains("Confirm") || n.Contains("Play") || n.Contains("Claim") || n.Contains("Resume") || n.Contains("Spawn") || n.Contains("QuickMerge");
            SetSurface(image, primary ? primaryButton : secondaryButton);
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1.08f, 1.08f, 1.02f);
            colours.pressedColor = new Color(.68f, .86f, .78f);
            colours.disabledColor = new Color(.43f, .48f, .47f, .8f);
            colours.fadeDuration = .08f;
            button.colors = colours;
            if (rt.GetComponent<JadeButtonFeedback>() == null) rt.gameObject.AddComponent<JadeButtonFeedback>();
        }
        else return;
        var outline = rt.GetComponent<UnityEngine.UI.Outline>();
        if (outline != null && !isSlot) outline.enabled = false;
        if (button == null && !isSlot) image.raycastTarget = false;
    }
    public static void SetSurface(UnityEngine.UI.Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.type = sprite.border.sqrMagnitude > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        image.pixelsPerUnitMultiplier = 1f;
        image.color = Color.white;
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using KhiVanChiTu.UI.CombatHUD;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Navigation.Editor
{
    public static class GlobalNavigationBuilder
    {
        public const string ScenePath="Assets/Scenes/SampleScene_MobileWork.unity";
        public const string Evidence="Assets/.docs/UIUX_Phase3_20261117/Evidence";
        static string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
        [MenuItem("TuTien/UI Global Navigation/0. Inspect Current Scene")]
        public static void Inspect()
        {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=ScenePath||EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Open mobile scene in Edit Mode.");
            Directory.CreateDirectory(Evidence);
            var roots=scene.GetRootGameObjects();
            var all=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var data=new{scene=scene.path,dirty=scene.isDirty,compiling=EditorApplication.isCompiling,
                canvases=all.Select(t=>t.GetComponent<Canvas>()).Where(c=>c!=null).Select(c=>new{path=PathOf(c.transform),c.sortingOrder,c.overrideSorting,mode=c.renderMode.ToString()}),
                hud=all.Where(t=>t.GetComponent<PortraitHudController>()!=null||t.name.Contains("Drawer_")||t.name=="MobileUIRoot"||t.name.Contains("Modal")||t.name=="ShopPanel").Select(t=>new{path=PathOf(t),active=t.gameObject.activeSelf,components=t.GetComponents<Component>().Where(c=>c!=null).Select(c=>c.GetType().Name)}),
                buttons=all.Select(t=>t.GetComponent<UnityEngine.UI.Button>()).Where(b=>b!=null).Select(b=>new{path=PathOf(b.transform),active=b.gameObject.activeInHierarchy,id=GlobalObjectId.GetGlobalObjectIdSlow(b).ToString(),events=Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Select(i=>new{method=b.onClick.GetPersistentMethodName(i),target=b.onClick.GetPersistentTarget(i)?.name})})};
            File.WriteAllText(Evidence+"/editor-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented));
            Debug.Log("[Global Navigation] Read-only scene inventory exported; no changes applied.");
        }
        [MenuItem("TuTien/UI Global Navigation/1. Build Mobile Navigation")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=ScenePath||scene.isDirty||EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Use saved mobile scene in Edit Mode.");
            if(UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include)!=null)throw new InvalidOperationException("Navigation already exists; preserve current work.");
            var hud=UnityEngine.Object.FindAnyObjectByType<CombatHudPresentation>(FindObjectsInactive.Include);
            if(hud==null)throw new InvalidOperationException("Phase 2 HUD is required.");
            var original=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<UnityEngine.UI.Button>(true)).Select(b=>GlobalObjectId.GetGlobalObjectIdSlow(b).ToString()).ToArray();
            Undo.IncrementCurrentGroup();Undo.SetCurrentGroupName("Phase 3 global navigation");
            var f=new FoundationFactory(hud.theme);var root=FoundationFactory.Rect("GlobalNavigationShell",hud.transform.parent);FoundationFactory.Fill(root);
            var nav=Undo.AddComponent<GlobalNavigationController>(root.gameObject);nav.hud=hud;nav.theme=hud.theme;nav.shell=root;
            Undo.RecordObject(hud,"Connect global navigation");hud.externalNavigation=true;hud.ApplyLayout();
            nav.hub=FoundationFactory.Rect("FullScreenPanel",root);FoundationFactory.Fill(nav.hub);f.Surface(nav.hub,hud.theme.ink,true);Layer(nav.hub,100);
            var header=FoundationFactory.Rect("ScreenHeader",nav.hub);CombatHudPresentation.Top(header,36,32,1008,152);
            nav.backButton=f.Action("Back",header,"\u2039");CombatHudPresentation.Top((RectTransform)nav.backButton.transform,0,0,132,132);nav.backButton.label.fontSize=56;
            nav.heading=f.Text("GroupTitle",header,"Trang b\u1ecb",52);nav.heading.font=hud.theme.headingFont;CombatHudPresentation.Top(nav.heading.rectTransform,168,8,804,72);
            nav.subtitle=f.Text("GroupDescription",header,"Phi ki\u1ebfm v\u00e0 h\u00e0nh trang",28);nav.subtitle.color=hud.theme.muted;CombatHudPresentation.Top(nav.subtitle.rectTransform,168,88,804,52);
            nav.breadcrumb=f.Text("Breadcrumb",nav.hub,"KH\u00cd V\u1eacN CHI T\u1eec",24);nav.breadcrumb.color=hud.theme.gold;CombatHudPresentation.Top(nav.breadcrumb.rectTransform,36,200,1008,44);
            var divider=FoundationFactory.Rect("HeaderDivider",nav.hub);CombatHudPresentation.Top(divider,36,252,1008,2);f.Surface(divider,hud.theme.gold);
            var body=Scroll(f,nav.hub,"SecondaryNavigation",32,192,32,280,out var content);nav.hubScroll=body;
            nav.cards=new NavigationCard[GlobalNavigationController.Destinations.Length];
            for(int i=0;i<nav.cards.Length;i++)
            {
                var d=GlobalNavigationController.Destinations[i];var action=f.Action("Route_"+d.route,content,d.title);var r=(RectTransform)action.transform;
                var e=r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();e.minHeight=e.preferredHeight=164;
                action.label.font=hud.theme.headingFont;action.label.fontSize=36;action.label.alignment=TextAlignmentOptions.MidlineLeft;FoundationFactory.Fill(action.label.rectTransform,104,96,64,12);action.label.textWrappingMode=TextWrappingModes.NoWrap;
                var desc=f.Text("Description",r,d.description,28);desc.color=hud.theme.muted;FoundationFactory.Fill(desc.rectTransform,104,12,48,72);
                var icon=FoundationFactory.Rect("Icon",r);CombatHudPresentation.Top(icon,28,48,48,48);var art=icon.gameObject.AddComponent<NavigationIcon>();art.symbol=(int)d.group;art.color=hud.theme.gold;art.raycastTarget=false;
                var arrow=f.Text("Arrow",r,"\u203a",40,TextAlignmentOptions.Center);PortraitHudController.Place(arrow.rectTransform,1,.5f,1,.5f,-16,0,40,64);
                nav.cards[i]=new NavigationCard{action=action,description=desc,route=d.route};
            }
            nav.footer=FoundationFactory.Rect("BottomNavigation",root);CombatHudPresentation.Bottom(nav.footer,24,16,1032,144);f.Surface(nav.footer,hud.theme.ink);Layer(nav.footer,120);
            var row=nav.footer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();row.spacing=6;row.padding=new RectOffset(4,4,4,4);row.childControlWidth=row.childControlHeight=true;row.childForceExpandWidth=true;
            nav.groupButtons=new FoundationAction[5];nav.groupIcons=new NavigationIcon[5];
            for(int i=0;i<5;i++)
            {
                var action=f.Action("Group_"+(NavigationGroup)i,nav.footer,GlobalNavigationController.GroupTitles[i]);nav.groupButtons[i]=action;
                action.label.fontSize=26;action.label.textWrappingMode=TextWrappingModes.NoWrap;FoundationFactory.Fill(action.label.rectTransform,8,8,8,0);action.label.rectTransform.anchorMax=new Vector2(1,.4f);
                var icon=FoundationFactory.Rect("Icon",action.transform);PortraitHudController.Place(icon,.5f,1,.5f,1,0,-16,48,48);nav.groupIcons[i]=icon.gameObject.AddComponent<NavigationIcon>();nav.groupIcons[i].symbol=i;nav.groupIcons[i].color=hud.theme.muted;nav.groupIcons[i].raycastTarget=false;
            }
            nav.blocker=FoundationFactory.Rect("GlobalNavigationBlockerModal",hud.GetComponentInParent<Canvas>().rootCanvas.transform);FoundationFactory.Fill(nav.blocker);f.Surface(nav.blocker,new Color(0,.015f,.02f,.72f),true);Layer(nav.blocker,299);nav.blocker.gameObject.SetActive(false);
            nav.progressionSheet=UtilitySheet(f,root,nav,"ProgressionBottomSheet","C\u00f4ng Ph\u00e1p & K\u1ef9 N\u0103ng",out var progression);
            nav.suppliesSheet=UtilitySheet(f,root,nav,"SuppliesBottomSheet","H\u1ed7 Tr\u1ee3 Tu Luy\u1ec7n",out var supplies);
            var canvas=hud.GetComponentInParent<Canvas>().rootCanvas.transform;
            var oldUpgrades=canvas.Find("RightPanel/UpgradeScrollView/UpgradePanel");
            if(oldUpgrades==null)throw new InvalidOperationException("Original progression cards are missing.");
            var oldCards=oldUpgrades.Cast<Transform>().Where(t=>t.name.StartsWith("Card_")||t.name.StartsWith("SkillCard_")).ToArray();
            if(oldCards.Length!=8)throw new InvalidOperationException("Expected 4 original upgrade + 4 skill cards; inspect before migration.");
            foreach(var card in oldCards)MoveCard(card,progression,f);
            var shop=canvas.Find("ShopPanel/ScrollView/Content");
            if(shop==null)throw new InvalidOperationException("Original shop cards are missing.");
            var shopCards=shop.Cast<Transform>().Where(t=>t.GetComponent<ShopItemUI>()!=null).ToArray();
            if(shopCards.Length!=5)throw new InvalidOperationException("Expected 5 original support items.");
            foreach(var card in shopCards)MoveCard(card,supplies,f);
            foreach(var toggle in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<ShopToggleUI>(true)))
            {Undo.RecordObject(toggle,"Preserve original shop routes");toggle.shopPanel=nav.suppliesSheet.gameObject;EditorUtility.SetDirty(toggle);}
            nav.hub.gameObject.SetActive(false);nav.progressionSheet.gameObject.SetActive(false);nav.suppliesSheet.gameObject.SetActive(false);
            var dock=hud.legacy.dock.GetComponent<CanvasGroup>();if(dock==null)dock=hud.legacy.dock.gameObject.AddComponent<CanvasGroup>();dock.alpha=0;dock.interactable=dock.blocksRaycasts=false;
            EditorUtility.SetDirty(hud);EditorUtility.SetDirty(nav);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            var after=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<UnityEngine.UI.Button>(true)).Select(b=>GlobalObjectId.GetGlobalObjectIdSlow(b).ToString()).ToHashSet();
            if(original.Any(id=>!after.Contains(id)))throw new InvalidOperationException("An original button identity was lost.");
            Debug.Log("[Global Navigation] Built five-group navigation; all original buttons retained. No gameplay or save data changed.");
        }
        static void Layer(RectTransform r,int order)
        {var c=r.gameObject.AddComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=order;r.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();r.gameObject.AddComponent<CanvasGroup>();}
        static UnityEngine.UI.ScrollRect Scroll(FoundationFactory f,Transform parent,string name,float left,float bottom,float right,float top,out RectTransform content)
        {
            var root=FoundationFactory.Rect(name,parent);FoundationFactory.Fill(root,left,bottom,right,top);var scroll=root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport=FoundationFactory.Rect("Viewport",root);FoundationFactory.Fill(viewport);f.Surface(viewport,Color.clear,true);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            content=FoundationFactory.Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=Vector2.zero;
            var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.spacing=20;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=viewport;scroll.content=content;return scroll;
        }
        static RectTransform UtilitySheet(FoundationFactory f,Transform parent,GlobalNavigationController nav,string name,string caption,out RectTransform content)
        {
            var root=FoundationFactory.Rect(name,parent);FoundationFactory.Fill(root);Layer(root,300);
            var surface=FoundationFactory.Rect("BottomSheet",root);FoundationFactory.Fill(surface,24,16,24,0);surface.anchorMax=new Vector2(1,.88f);f.Surface(surface,f.theme.ink,true);
            var title=f.Text("SheetTitle",surface,caption,40);title.font=f.theme.headingFont;CombatHudPresentation.Top(title.rectTransform,32,28,816,80);
            var close=f.Action("Close",surface,"\u00d7");PortraitHudController.Place((RectTransform)close.transform,1,1,1,1,-24,-24,132,112);close.label.fontSize=48;UnityEditor.Events.UnityEventTools.AddPersistentListener(close.clicked,nav.CloseCurrentLayer);
            Scroll(f,surface,"Scroll",24,24,24,144,out content);root.gameObject.SetActive(false);return root;
        }
        static void MoveCard(Transform card,RectTransform content,FoundationFactory f)
        {
            Undo.SetTransformParent(card,content,"Migrate existing function card");card.localScale=Vector3.one;card.gameObject.SetActive(true);
            var layout=card.GetComponent<UnityEngine.UI.LayoutElement>();if(layout==null)layout=card.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();layout.minHeight=layout.preferredHeight=248;
            var image=card.GetComponent<UnityEngine.UI.Image>();if(image!=null){image.sprite=f.theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=f.theme.surface;}
            foreach(var group in card.GetComponents<UnityEngine.UI.LayoutGroup>())group.enabled=false;
            var button=card.GetComponentInChildren<UnityEngine.UI.Button>(true);
            if(button!=null){PortraitHudController.Place((RectTransform)button.transform,1,.5f,1,.5f,-20,0,300,132);var bg=button.GetComponent<UnityEngine.UI.Image>();if(bg!=null){bg.sprite=f.theme.surfaceSprite;bg.type=UnityEngine.UI.Image.Type.Sliced;bg.color=f.theme.raised;}}
            int line=0;int count=card.GetComponentsInChildren<UnityEngine.UI.Text>(true).Count(t=>button==null||!t.transform.IsChildOf(button.transform));
            foreach(var text in card.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                text.resizeTextForBestFit=false;text.raycastTarget=false;text.font=UiTextStyle.BodyFont;text.color=JadeUITheme.Ivory;
                if(button!=null&&text.transform.IsChildOf(button.transform)){PortraitHudController.Fill(text.rectTransform,12,8,12,8);text.fontSize=28;text.alignment=TextAnchor.MiddleCenter;}
                else {PortraitHudController.Place(text.rectTransform,0,1,1,1,0,-20-line*76,0,count==1?208:line==0?68:124);text.rectTransform.offsetMin=new Vector2(28,text.rectTransform.offsetMin.y);text.rectTransform.offsetMax=new Vector2(-352,text.rectTransform.offsetMax.y);text.fontSize=line==0?32:26;text.alignment=TextAnchor.MiddleLeft;line++;}
            }
        }
    }
}

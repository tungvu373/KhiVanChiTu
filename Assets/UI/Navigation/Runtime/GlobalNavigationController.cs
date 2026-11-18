using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.CombatHUD;

namespace KhiVanChiTu.UI.Navigation
{
    public enum NavigationGroup { Battle, Equipment, Cultivation, SecretRealm, Other }
    public enum NavigationRoute { Merge, Storage, Body, Alchemy, Artifact, Formation, Progression, Tribulation, RealmList, Horde, Barrier, Cauldron, Lightning, SwordTomb, Talents, Milestones, Achievements, Character, Resources, Buffs, Log, Settings, Supplies, SwordStyle }
    public enum NavigationLayerKind { Screen, BottomSheet, Modal }
    [Serializable] public sealed class NavigationDestination
    {
        public NavigationGroup group;public NavigationRoute route;public string title,description;
        public NavigationDestination(NavigationGroup g,NavigationRoute r,string t,string d){group=g;route=r;title=t;description=d;}
    }
    [Serializable] public sealed class NavigationCard
    {public FoundationAction action;public TMP_Text description;public NavigationRoute route;}

    /// <summary>Scene-local navigation and input ownership. Never changes gameplay time or player data.</summary>
    [DefaultExecutionOrder(10000),DisallowMultipleComponent]
    public sealed class GlobalNavigationController : MonoBehaviour
    {
        public CombatHudPresentation hud;
        public FoundationTheme theme;
        public RectTransform shell,hub,footer,blocker,progressionSheet,suppliesSheet;
        public TMP_Text heading,subtitle,breadcrumb;
        public UnityEngine.UI.ScrollRect hubScroll;
        public FoundationAction backButton;
        public FoundationAction[] groupButtons;
        public NavigationIcon[] groupIcons;
        public NavigationCard[] cards;
        public static GlobalNavigationController Instance {get;private set;}
        public NavigationGroup SelectedGroup {get;private set;}
        public int StackDepth=>active.Count;
        public string TopLayer=>active.Count>0?active[active.Count-1].key:"";
        public bool IsInSession=>isActiveAndEnabled&&(MainMenuManager.Instance==null||!MainMenuManager.Instance.isActiveAndEnabled);
        public bool IsHubVisible=>hub!=null&&hub.gameObject.activeInHierarchy;
        public bool IsBusy=>pendingGroup.HasValue;
        public int VisibleFullScreenCount {get;private set;}
        Canvas rootCanvas;
        CanvasGroup topHud,bottomHud,oldDock,hubGroup,footerGroup;
        MobileDrawerUIController drawer;
        readonly List<Layer> layers=new List<Layer>(),active=new List<Layer>();
        int stamp;
        NavigationGroup? pendingGroup;
        float nextClose,nextBack;
        bool wired;
        sealed class Layer
        {
            public string key;public Transform root;public Action close;public NavigationLayerKind kind;
            public Canvas canvas;public CanvasGroup group;public bool wasActive,suppressed;public float previousAlpha=1;public int stamp;
        }
        public static readonly string[] GroupTitles={"Chi\u1ebfn \u0111\u1ea5u","Trang b\u1ecb","Tu luy\u1ec7n","B\u00ed c\u1ea3nh","Kh\u00e1c"};
        public static readonly NavigationDestination[] Destinations={
            new NavigationDestination(NavigationGroup.Equipment,NavigationRoute.Merge,"Ki\u1ebfm L\u1ed9","Dung h\u1ee3p phi ki\u1ebfm, s\u1eafp tr\u1eadn v\u00e0 m\u1edf \u00f4 xu\u1ea5t chi\u1ebfn"),
            new NavigationDestination(NavigationGroup.Equipment,NavigationRoute.Storage,"Kho \u0110\u1ed3","Ki\u1ebfm, \u0111an d\u01b0\u1ee3c, nguy\u00ean li\u1ec7u, l\u1ecdc v\u00e0 ph\u00e2n gi\u1ea3i"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Body,"Luy\u1ec7n Th\u1ec3","Th\u1ec3 ph\u00e1ch v\u00e0 c\u00e1c nh\u00e1nh thi\u00ean ph\u00fa"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Alchemy,"Luy\u1ec7n \u0110an","Luy\u1ec7n d\u01b0\u1ee3c, ti\u1ebfn \u0111\u1ed9 n\u1ea5u v\u00e0 d\u00f9ng \u0111an"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Artifact,"Ph\u00e1p Kh\u00ed","N\u00e2ng ph\u00e1p kh\u00ed v\u00e0 theo d\u00f5i hi\u1ec7u \u1ee9ng"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Formation,"Tr\u1eadn Ph\u00e1p","H\u1ecdc, ch\u1ecdn tr\u1eadn v\u00e0 k\u00edch ho\u1ea1t tr\u1eadn linh"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Progression,"C\u00f4ng Ph\u00e1p & K\u1ef9 N\u0103ng","N\u00e2ng ch\u1ec9 s\u1ed1, m\u1edf kh\u00f3a v\u00e0 n\u00e2ng k\u1ef9 n\u0103ng c\u0169"),
            new NavigationDestination(NavigationGroup.Cultivation,NavigationRoute.Tribulation,"\u0110\u1ed9 Ki\u1ebfp","Xem x\u00e1c nh\u1eadn khi \u0111\u1ee7 \u0111i\u1ec1u ki\u1ec7n c\u1ea3nh gi\u1edbi"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.RealmList,"T\u1ea5t c\u1ea3 B\u00ed C\u1ea3nh","L\u01b0\u1ee3t v\u00e0o, chi ph\u00ed v\u00e0 ph\u1ea7n th\u01b0\u1edfng hi\u1ec7n c\u00f3"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.Horde,"V\u1ea1n Ma Tri\u1ec1u D\u00e2ng","Xem c\u1eeda v\u00e0o th\u1eed th\u00e1ch sinh t\u1ed3n"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.Barrier,"Tr\u1ea3m Ph\u00e1 C\u1ea5m Ch\u1ebf","Xem c\u1eeda v\u00e0o th\u1eed th\u00e1ch s\u00e1t th\u01b0\u01a1ng"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.Cauldron,"\u0110an Th\u1ea7n K\u1ef3 Duy\u00ean","Xem c\u1eeda v\u00e0o k\u1ef3 duy\u00ean l\u00f2 \u0111an"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.Lightning,"V\u01b0\u1ee3t L\u00f4i Tr\u00ec","Xem c\u1eeda v\u00e0o thu th\u1eadp v\u00e0 n\u00e9 tr\u00e1nh"),
            new NavigationDestination(NavigationGroup.SecretRealm,NavigationRoute.SwordTomb,"Ki\u1ebfm Ch\u1ee7ng C\u1ed5 Th\u1ea7n","Xem c\u1eeda v\u00e0o th\u1eed th\u00e1ch r\u00fat ki\u1ebfm"),
            new NavigationDestination(NavigationGroup.Equipment,NavigationRoute.Talents,"Đạo Vận","\u0110\u1ea1o \u1ea4n, b\u1ed9 l\u1ecdc v\u00e0 n\u00e2ng c\u1ea5p"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Milestones,"M\u1ed1c Tu Ti\u00ean","Ti\u1ebfn \u0111\u1ed9 v\u00e0 c\u00e1c ph\u1ea7n th\u01b0\u1edfng \u0111ang ch\u1edd"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Achievements,"Th\u00e0nh T\u1ef1u","Danh s\u00e1ch th\u00e0nh t\u1ef1u v\u00e0 nh\u1eadn th\u01b0\u1edfng"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Character,"Nh\u00e2n V\u1eadt","C\u1ea3nh gi\u1edbi, trang b\u1ecb v\u00e0 ch\u1ec9 s\u1ed1 chi ti\u1ebft"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Resources,"T\u00e0i Nguy\u00ean","Xem s\u1ed1 d\u01b0 \u0111\u1ea7y \u0111\u1ee7"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Buffs,"Hi\u1ec7u \u1ee8ng","C\u00e1c hi\u1ec7u \u1ee9ng h\u1ed7 tr\u1ee3 \u0111ang ho\u1ea1t \u0111\u1ed9ng"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Log,"Nh\u1eadt K\u00fd","Nh\u1eadt k\u00fd chi\u1ebfn \u0111\u1ea5u v\u00e0 s\u1ef1 ki\u1ec7n"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Settings,"C\u00e0i \u0110\u1eb7t & T\u1ea1m D\u1eebng","\u00c2m thanh, l\u01b0u ti\u1ebfn tr\u00ecnh, menu v\u00e0 tho\u00e1t"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.Supplies,"H\u1ed7 Tr\u1ee3 Tu Luy\u1ec7n","N\u0103m v\u1eadt ph\u1ea9m c\u1eeda h\u00e0ng v\u00e0 gi\u1edbi h\u1ea1n c\u1ed9ng d\u1ed3n c\u0169"),
            new NavigationDestination(NavigationGroup.Other,NavigationRoute.SwordStyle,"Ki\u1ebfm Ph\u00e1p","Xem v\u00e0 \u0111\u1ed5i ki\u1ebfm ph\u00e1p hi\u1ec7n c\u00f3")};

        public static GlobalNavigationController For(Component context)
        {return Instance!=null&&Instance.gameObject.scene==context.gameObject.scene?Instance:null;}
        void Awake()
        {
            Instance=this;rootCanvas=GetComponentInParent<Canvas>().rootCanvas;drawer=hud.drawer;
            topHud=Group(hud.legacy.upper);bottomHud=Group(hud.legacy.lower);oldDock=Group(hud.legacy.dock);
            hubGroup=Group(hub);footerGroup=Group(footer);
        }
        void Start()
        {
            if(hud!=null&&hud.legacy!=null&&hud.legacy.secondaryOverlay!=null)
            {
                foreach(var b in hud.legacy.secondaryOverlay.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                {
                    foreach(var mb in b.GetComponents<MonoBehaviour>())
                    {
                        if(mb is StorageButtonHandler||mb is BiCanhButtonHandler||mb is DaoAnButtonHandler||mb is AchievementButtonHandler)
                        {
                            mb.SendMessage("Awake",SendMessageOptions.DontRequireReceiver);
                            mb.SendMessage("Start",SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
            }
            for(int i=0;i<groupButtons.Length;i++){var group=(NavigationGroup)i;groupButtons[i].Bind(GroupTitles[i],()=>SelectGroup(group));}
            foreach(var card in cards){var route=card.route;card.action.Bind(card.action.label.text,()=>OpenRoute(route));}
            backButton.Bind("\u2039",()=>Back());
            hud.legacy.menuButton.onClick.AddListener(OpenMore);wired=true;SelectGroup(NavigationGroup.Battle);
        }
        void OnDestroy(){if(wired&&hud!=null&&hud.legacy!=null)hud.legacy.menuButton.onClick.RemoveListener(OpenMore);if(Instance==this)Instance=null;}
        void OnDisable(){if(topHud!=null){SetGate(topHud,true);SetGate(bottomHud,true);}if(blocker!=null)blocker.gameObject.SetActive(false);}
        void OpenMore(){hud.legacy.CloseSecondary();SelectGroup(NavigationGroup.Other);}
        public void SelectGroup(NavigationGroup group)
        {
            if(!IsInSession)return;
            pendingGroup=group;nextClose=0;Discover();Collect();ProcessRequest();
        }
        void ProcessRequest()
        {
            if(!pendingGroup.HasValue)return;
            if(active.Count>0||drawer.IsBlockingGameplay)
            {
                if(Time.unscaledTime<nextClose)return;
                nextClose=Time.unscaledTime+.4f;
                if(active.Count>0)active[active.Count-1].close();else drawer.CloseActiveDrawer();return;
            }
            SelectedGroup=pendingGroup.Value;pendingGroup=null;heading.text=GroupTitles[(int)SelectedGroup];
            subtitle.text=SelectedGroup==NavigationGroup.Equipment?"Phi ki\u1ebfm v\u00e0 h\u00e0nh trang":SelectedGroup==NavigationGroup.Cultivation?"T\u1eebng b\u01b0\u1edbc v\u1eefng \u0111\u1ea1o t\u00e2m":SelectedGroup==NavigationGroup.SecretRealm?"Ch\u1ecdn th\u1eed th\u00e1ch, xem l\u01b0\u1ee3t v\u00e0o tr\u01b0\u1edbc khi xu\u1ea5t ph\u00e1t":"\u0110\u1ea1o l\u1ed9 v\u00e0 ti\u1ec7n \u00edch";
            breadcrumb.text="KH\u00cd V\u1eacN CHI T\u1eec  /  "+GroupTitles[(int)SelectedGroup];
            // Bind by route identity; serialized cards retain their existing order/references.
            foreach(var card in cards)card.action.gameObject.SetActive(FindDestination(card.route).group==SelectedGroup);
            if(SelectedGroup==NavigationGroup.Equipment)
                foreach(var card in cards)if(card.route==NavigationRoute.Talents)card.action.transform.SetAsFirstSibling();
            Canvas.ForceUpdateCanvases();hubScroll.verticalNormalizedPosition=1;
            if(UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
        public void OpenRoute(NavigationRoute route)
        {
            if(!IsInSession||active.Count>0||IsBusy)return;
            switch(route)
            {
                case NavigationRoute.Merge:drawer.OpenDrawer(0);break;
                case NavigationRoute.Storage:
                    hud.legacy.quickButtons[0].onClick.Invoke();
                    if(StorageModalUI.Instance!=null&&!StorageModalUI.Instance.IsModalOpen)StorageModalUI.Open();
                    break;
                case NavigationRoute.Body:drawer.OpenDrawer(1);break;
                case NavigationRoute.Alchemy:drawer.OpenDrawer(2);break;
                case NavigationRoute.Artifact:drawer.OpenDrawer(3);break;
                case NavigationRoute.Formation:drawer.OpenDrawer(4);break;
                case NavigationRoute.Progression:progressionSheet.gameObject.SetActive(true);break;
                case NavigationRoute.Supplies:suppliesSheet.gameObject.SetActive(true);break;
                case NavigationRoute.Tribulation:TribulationModalUI.Open();break;
                case NavigationRoute.RealmList:case NavigationRoute.Horde:case NavigationRoute.Barrier:case NavigationRoute.Cauldron:case NavigationRoute.Lightning:case NavigationRoute.SwordTomb:
                    hud.legacy.quickButtons[1].onClick.Invoke();
                    if(SecretRealmModalUI.Instance!=null&&!SecretRealmModalUI.Instance.IsModalOpen)SecretRealmModalUI.Instance.Open();
                    var realmScroll=rootCanvas.transform.Find("SecretRealmModal/ModalBox/ScrollView")?.GetComponent<UnityEngine.UI.ScrollRect>();
                    if(realmScroll!=null){Canvas.ForceUpdateCanvases();realmScroll.verticalNormalizedPosition=route==NavigationRoute.RealmList?1:1-((int)route-(int)NavigationRoute.Horde)/4f;}
                    var realmView=rootCanvas.transform.Find("SecretRealmModal")?.GetComponent<KhiVanChiTu.UI.SecretRealm.RealmHubView>();
                    if(realmView!=null)realmView.Focus(route==NavigationRoute.RealmList?SecretRealmType.HordeSurvival:(SecretRealmType)((int)route-(int)NavigationRoute.Horde));break;
                case NavigationRoute.Talents:
                    hud.legacy.quickButtons[2].onClick.Invoke();
                    if(DaoAnModalUI.Instance!=null&&!DaoAnModalUI.Instance.IsModalOpen)DaoAnModalUI.Instance.Open();
                    break;
                case NavigationRoute.Milestones:hud.legacy.milestoneButton.onClick.Invoke();break;
                case NavigationRoute.Achievements:
                    hud.legacy.achievementButton.onClick.Invoke();
                    if(AchievementModalUI.Instance!=null&&!AchievementModalUI.Instance.IsModalOpen)AchievementModalUI.Open();
                    break;
                case NavigationRoute.Character:hud.legacy.profileButton.onClick.Invoke();break;
                case NavigationRoute.Resources:hud.legacy.walletButton.onClick.Invoke();break;
                case NavigationRoute.Buffs:hud.legacy.buffsButton.onClick.Invoke();break;
                case NavigationRoute.Log:hud.legacy.logButton.onClick.Invoke();break;
                case NavigationRoute.Settings:
                    hud.pause.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    PauseMenuManager.Instance?.GetComponent<KhiVanChiTu.UI.Menu.MenuPauseView>()?.ShowSettings();break;
                case NavigationRoute.SwordStyle:
                    var style=hud.legacy.styleHost.GetComponentInChildren<UnityEngine.UI.Button>(true);
                    if(style!=null&&style.interactable)style.onClick.Invoke();break;
            }
            Discover();Collect();Present();
        }
        public bool Back()
        {
            if(!IsInSession)return false;
            if(Time.unscaledTime<nextBack)return true;nextBack=Time.unscaledTime+.16f;
            pendingGroup=null;Discover();Collect();
            if(active.Count>0){active[active.Count-1].close();return true;}
            if(drawer.IsBlockingGameplay){drawer.CloseActiveDrawer();return true;}
            if(SelectedGroup!=NavigationGroup.Battle){SelectGroup(NavigationGroup.Battle);return true;}
            PauseMenuManager.Instance?.OpenPauseMenu();return true;
        }
        public void CloseCurrentLayer(){Back();}
        void LateUpdate()
        {
            if(!IsInSession)return;
            Discover();Collect();ProcessRequest();RefreshAvailability();Present();
        }
        void RefreshAvailability()
        {
            foreach(var card in cards)if(card.action.gameObject.activeSelf)
            {
                bool available=true;int index=card.route==NavigationRoute.Merge?0:card.route==NavigationRoute.Body?1:card.route==NavigationRoute.Alchemy?2:card.route==NavigationRoute.Artifact?3:card.route==NavigationRoute.Formation?4:-1;
                if(index>=0){available=drawer.IsDrawerUnlocked(index,out string reason);if(!available)card.description.text=reason;else card.description.text=FindDestination(card.route).description;}
                if(card.route==NavigationRoute.Tribulation)
                {var source=hud.legacy.realm.GetComponent<UnityEngine.UI.Button>();available=source!=null&&source.interactable;}
                if(card.route==NavigationRoute.SwordStyle){var source=hud.legacy.styleHost.GetComponentInChildren<UnityEngine.UI.Button>(true);available=source!=null&&source.interactable;}
                card.action.SetAvailable(available);
            }
        }
        static NavigationDestination FindDestination(NavigationRoute route){foreach(var d in Destinations)if(d.route==route)return d;return null;}
        public void Synchronize(){Discover();Collect();Present();}
        void Present()
        {
            bool any=active.Count>0,screen=false;for(int i=0;i<active.Count;i++)if(active[i].kind==NavigationLayerKind.Screen)screen=true;
            bool showHub=SelectedGroup!=NavigationGroup.Battle&&!screen;
            hub.gameObject.SetActive(showHub);
            bool showFooter=!any&&(SelectedGroup!=NavigationGroup.Battle||!hud.IsCompact);
            footer.gameObject.SetActive(showFooter);
            SetGate(topHud,!any&&!showHub);SetGate(bottomHud,!any&&!showHub);
            oldDock.alpha=0;oldDock.interactable=oldDock.blocksRaycasts=false;
            hubGroup.interactable=hubGroup.blocksRaycasts=!any&&!IsBusy;
            footerGroup.interactable=footerGroup.blocksRaycasts=!any&&!IsBusy;
            VisibleFullScreenCount=showHub?1:0;
            for(int i=0;i<groupButtons.Length;i++)
            {
                bool selected=(int)SelectedGroup==i;groupButtons[i].button.targetGraphic.color=hud.hudSurface!=null?(selected?Color.white:new Color(.72f,.78f,.76f,1)):selected?theme.raised:theme.ink;
                groupButtons[i].label.color=selected?theme.gold:theme.muted;groupIcons[i].color=selected?theme.gold:theme.muted;
            }
            blocker.gameObject.SetActive(any);
            if(any){var c=blocker.GetComponent<Canvas>();c.sortingOrder=300+(active.Count-1)*4-1;}
            for(int i=0;i<active.Count;i++)
            {
                var layer=active[i];bool top=i==active.Count-1;
                layer.canvas.overrideSorting=true;layer.canvas.sortingOrder=300+i*4;layer.group.ignoreParentGroups=top;layer.group.interactable=layer.group.blocksRaycasts=top;
                bool hide=false;
                if(layer.kind==NavigationLayerKind.Screen)
                {
                    for(int j=i+1;j<active.Count;j++)if(active[j].kind==NavigationLayerKind.Screen)hide=true;
                    if(!hide)VisibleFullScreenCount++;
                }
                if(hide){if(!layer.suppressed)layer.previousAlpha=layer.group.alpha;layer.group.alpha=0;layer.suppressed=true;}
                else if(layer.suppressed){layer.group.alpha=layer.previousAlpha;layer.suppressed=false;}
            }
        }
        void Collect()
        {
            active.Clear();
            foreach(var layer in layers)
            {
                bool visible=layer.root!=null&&layer.root.gameObject.activeInHierarchy;
                if(visible&&!layer.wasActive)layer.stamp=++stamp;
                if(!visible&&layer.suppressed){layer.group.alpha=layer.previousAlpha;layer.suppressed=false;}
                layer.wasActive=visible;if(visible)active.Add(layer);
            }
            active.Sort((a,b)=>a.stamp.CompareTo(b.stamp));
        }
        void Discover()
        {
            var canvas=rootCanvas.transform;
            Add("item-detail",canvas.Find("ItemDetailBottomSheet"),NavigationLayerKind.BottomSheet,()=>KhiVanChiTu.UI.Equipment.ItemDetailSheet.Find(this)?.Close());
            Add("storage",canvas.Find("StorageModalUI"),NavigationLayerKind.Screen,()=>StorageModalUI.Instance.Close());
            Add("talents",canvas.Find("DaoAnModal"),NavigationLayerKind.Screen,()=>DaoAnModalUI.Instance.Close());
            Add("milestones",canvas.Find("MilestoneModalUI"),NavigationLayerKind.Screen,()=>MilestoneModalUI.Instance.Close());
            Add("achievements",canvas.Find("AchievementModalUI"),NavigationLayerKind.Screen,AchievementModalUI.Close);
            Add("character",canvas.Find("CharacterDetailModal"),NavigationLayerKind.Screen,CharacterDetailModalUI.Close);
            Add("realm-list",canvas.Find("SecretRealmModal"),NavigationLayerKind.Screen,()=>SecretRealmModalUI.Instance.Close());
            Add("realm-play",canvas.Find("SecretRealmPlayRoot"),NavigationLayerKind.Screen,()=>SecretRealmPlayController.Instance.CloseAll());
            Add("realm-result",canvas.Find("SecretRealmResultModal"),NavigationLayerKind.Modal,()=>SecretRealmPlayController.Instance.CloseAll());
            Add("dismantle",canvas.Find("DismantleConfirmModalUI"),NavigationLayerKind.Modal,()=>DismantleConfirmModalUI.Instance.Close());
            Add("tribulation",canvas.Find("TribulationModalUI"),NavigationLayerKind.Modal,()=>TribulationModalUI.Instance.OnCancelClicked());
            Add("offline",canvas.Find("OfflineRewardPopupUI"),NavigationLayerKind.Modal,()=>OfflineRewardPopupUI.Instance.DismissFromBack());
            Add("pause",canvas.Find("PauseMenuModal"),NavigationLayerKind.Modal,()=>PauseMenuManager.Instance.CloseTopLayer());
            if(PauseMenuManager.Instance!=null)Add("pause-confirm",PauseMenuManager.Instance.confirmationPopup?.transform,NavigationLayerKind.Modal,()=>PauseMenuManager.Instance.CloseConfirmation());
            if(GameplayChoicesService.Instance!=null)Add("choices",GameplayChoicesService.Instance.ChoiceTransform,NavigationLayerKind.Modal,()=>GameplayChoicesService.Instance.ClosePanel());
            Add("body-detail",drawer.transform.Find("BodyTalentDetailModal"),NavigationLayerKind.Modal,()=>drawer.drawerPanels[1].GetComponent<BodyTemperingPanelUI>().CloseCenterModal());
            Add("hud-detail",hud.legacy.secondaryOverlay.transform,NavigationLayerKind.Screen,hud.legacy.CloseSecondary);
            for(int i=0;i<drawer.drawerPanels.Length;i++)Add("drawer-"+i,drawer.drawerPanels[i],NavigationLayerKind.BottomSheet,drawer.CloseActiveDrawer);
            Add("progression",progressionSheet,NavigationLayerKind.BottomSheet,()=>progressionSheet.gameObject.SetActive(false));
            Add("supplies",suppliesSheet,NavigationLayerKind.BottomSheet,()=>suppliesSheet.gameObject.SetActive(false));
        }
        void Add(string key,Transform root,NavigationLayerKind kind,Action close)
        {
            if(root==null)return;foreach(var old in layers)if(old.root==root)return;
            var canvas=root.GetComponent<Canvas>();if(canvas==null)canvas=root.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;
            if(root.GetComponent<UnityEngine.UI.GraphicRaycaster>()==null)root.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            layers.Add(new Layer{key=key,root=root,close=close,kind=kind,canvas=canvas,group=Group(root)});
        }
        static CanvasGroup Group(Transform t){var g=t.GetComponent<CanvasGroup>();return g!=null?g:t.gameObject.AddComponent<CanvasGroup>();}
        static void SetGate(CanvasGroup group,bool show){group.alpha=show?1:0;group.interactable=group.blocksRaycasts=show;}
    }
}

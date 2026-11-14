#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Creates a mobile-only HUD shell and grafts live controls without replacing their IDs or events.</summary>
public static class PortraitHudPrototypeBuilder
{
    public const string ScenePath = "Assets/Scenes/SampleScene_MobileWork.unity";
    public const string PrefabFolder = "Assets/UI/MobileHUD/hud_prefabs";
    const string Evidence = "Docs/UI_Redesign/Backups/Phase04-20261112-173028";
    static PortraitHudController hud;
    static Transform mobile;
    static JadeUITheme theme;
    static readonly Color Surface = new Color32(10,31,34,255);
    static readonly Color Ivory = new Color32(241,232,207,255);

    [MenuItem("TuTien/UI/Portrait HUD/01 Build functional prototype")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != ScenePath || scene.isDirty)
            throw new InvalidOperationException("Build requires the saved mobile work scene in Edit Mode.");
        if (!File.Exists(Evidence + "/editor-baseline.json")) throw new InvalidOperationException("Record the Phase04 checkpoint first.");
        mobile = GameObject.Find("Canvas/MobileUIRoot")?.transform;
        if (mobile == null) throw new InvalidOperationException("Mobile root is missing.");
        if (mobile.GetComponentInChildren<PortraitHudController>(true) != null)
            throw new InvalidOperationException("HUD already exists. Use Reflow instead of rebuilding controls.");
        theme = mobile.GetComponentInParent<JadeUITheme>();
        if (theme == null) throw new InvalidOperationException("The existing Jade atlas theme is required.");
        var drawer = mobile.GetComponent<MobileDrawerUIController>();
        Undo.IncrementCurrentGroup(); Undo.SetCurrentGroupName("Build portrait HUD prototype");
        var shell = Node(mobile,"PortraitHudShell"); Fill(shell);
        hud = Undo.AddComponent<PortraitHudController>(shell.gameObject);
        hud.panelSprite=theme.panel;hud.windowSprite=theme.window;hud.slotSprite=theme.slot;hud.selectedSprite=theme.selectedSlot;
        hud.regularFont=UiTextStyle.BodyFont;
        hud.semiboldFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf");
        hud.upper=Node(shell,"UpperInformation");Fill(hud.upper);
        hud.lower=Node(shell,"PrimaryActions");Fill(hud.lower);
        hud.profile=Move("CharacterFrame",hud.upper);
        SurfaceImage(hud.profile,theme.panel,true);
        var avatar=hud.profile.Find("Avatar") as RectTransform;
        Rect(avatar,0,.5f,0,.5f,12,0,120,120);
        var halo=hud.profile.Find("JadeHalo") as RectTransform;if(halo!=null)Rect(halo,0,.5f,0,.5f,12,0,120,120);
        hud.sourceProfile=hud.profile.Find("InfoText").GetComponent<UnityEngine.UI.Text>();
        hud.sourceProfile.enabled=false;
        hud.playerName=Label(hud.profile,"PlayerName","Đạo Hữu",42);
        Rect(hud.playerName.rectTransform,0,1,1,1,66,-8,-156,84);
        hud.playerRealm=Label(hud.profile,"RealmName","Luyện Khí · Sơ Kỳ",36);
        Rect(hud.playerRealm.rectTransform,0,0,1,0,66,8,-156,44);
        hud.profile.Find("DetailHintText")?.gameObject.SetActive(false);
        hud.wallet=Move("TopBar",hud.upper);
        var walletBg=hud.wallet.GetComponent<UnityEngine.UI.Image>();walletBg.sprite=null;walletBg.color=Color.clear;walletBg.raycastTarget=false;
        string[] bgNames={"BG_LinhThach","BG_CoDuyen"};string[] valueNames={"txt_LinhThach","txt_CoDuyen"};
        for(int i=0;i<2;i++)
        {
            var bg=hud.wallet.Find(bgNames[i]) as RectTransform;
            Rect(bg,0,i==0?1:0,1,i==0?1:0,0,i==0?0:0,0,68);SurfaceImage(bg,theme.resourceBar,false);
            var value=hud.wallet.Find(valueNames[i]) as RectTransform;
            Rect(value,0,i==0?1:0,1,i==0?1:0,18,i==0?0:0,-52,68);
            var icon=hud.wallet.Find("JadeCurrency_"+i) as RectTransform;
            Rect(icon,0,i==0?1:0,0,i==0?1:0,10,i==0?-10:10,44,48);
        }
        var pulse=Node(hud.wallet,"FeedbackLine");Rect(pulse,0,0,1,0,0,0,0,4);Image(pulse,null,new Color(0,0,0,0));
        hud.walletButton=hud.wallet.GetComponent<UnityEngine.UI.Button>()??Undo.AddComponent<UnityEngine.UI.Button>(hud.wallet.gameObject);
        hud.walletButton.targetGraphic=walletBg;walletBg.raycastTarget=true;hud.walletButton.transition=UnityEngine.UI.Selectable.Transition.None;
        var pause=Move("PauseButton",hud.upper);Rect(pause,1,1,1,1,-36,-24,132,144);
        var pauseLabel=pause.GetComponentInChildren<UnityEngine.UI.Text>(true);pauseLabel.text="Tạm\ndừng";pauseLabel.fontSize=36;
        hud.realm=Move("StagePanel",hud.upper);
        hud.cultivationValue=Label(hud.realm,"CultivationValue","Tu vi 0 / 100",36);
        var realmPulse=Node(hud.realm,"FeedbackLine");Rect(realmPulse,0,0,1,0,0,0,0,4);Image(realmPulse,null,Color.clear);
        hud.stage=Move("StageUI",hud.upper);
        hud.quest=Move("TutorialTargetQuestBar",hud.upper);
        hud.notification=Move("CombatNotificationBanner",shell);
        hud.vitals=Node(hud.lower,"VitalBars");
        hud.hp=Move("PlayerHPBar",hud.vitals);hud.mana=Move("PlayerManaBar",hud.vitals);
        hud.manaValue=Label(hud.mana,"ManaValue","MP 0 / 100",36);
        hud.mana.gameObject.SetActive(true);
        hud.skills=Move("MobileSkillHUD",hud.lower);
        hud.quick=Node(hud.lower,"QuickAccessBar");
        var storage=Move("BtnHUDStorage",hud.quick).GetComponent<UnityEngine.UI.Button>();
        var trial=Move("BtnHUDBiCanh",hud.quick).GetComponent<UnityEngine.UI.Button>();
        var dao=Move("BtnHUDDaoAn",hud.quick).GetComponent<UnityEngine.UI.Button>();
        hud.milestoneButton=Button(hud.quick,"MilestoneButton","Mốc");
        hud.quickButtons=new[]{storage,trial,dao,hud.milestoneButton};
        string[] labels={"Kho","Bí cảnh","Vận khí","Mốc"};
        for(int i=0;i<4;i++){hud.quickButtons[i].gameObject.SetActive(true);hud.quickButtons[i].GetComponentInChildren<UnityEngine.UI.Text>(true).text=labels[i];}
        hud.dock=Move("MobileDock",mobile);
        var dockImage=hud.dock.GetComponent<UnityEngine.UI.Image>();dockImage.sprite=null;dockImage.color=Color.clear;dockImage.raycastTarget=false;
        hud.focusButton=Move("BottomDockToggle",shell);
        hud.menuButton=Move("JadeMenuButton",hud.upper).GetComponent<UnityEngine.UI.Button>();
        hud.menuButton.GetComponentInChildren<UnityEngine.UI.Text>(true).text="Đạo\nlộ";
        hud.styleHost=Node(shell,"SwordStyleActionHost");
        BuildSecondary(shell);
        // Preserve the two factory-owned alias paths and their bound rows/content.
        hud.logPanel=mobile.Find("CombatLogPanel") as RectTransform;
        hud.buffPanel=mobile.Find("ActiveBuffPanel") as RectTransform;
        hud.logPanel.gameObject.SetActive(false);hud.buffPanel.gameObject.SetActive(false);
        var content=hud.logPanel.Find("Viewport/Content");
        if(content.GetComponent<PortraitHudLogStyleScope>()==null)Undo.AddComponent<PortraitHudLogStyleScope>(content.gameObject);
        var buffLayout=hud.buffPanel.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if(buffLayout!=null){buffLayout.spacing=18;buffLayout.childControlHeight=true;buffLayout.childControlWidth=true;buffLayout.childForceExpandHeight=false;}
        foreach(Transform row in hud.buffPanel)
        {
            if(!row.name.StartsWith("BuffRow_"))continue;
            var le=row.GetComponent<UnityEngine.UI.LayoutElement>()??Undo.AddComponent<UnityEngine.UI.LayoutElement>(row.gameObject);le.minHeight=132;le.preferredHeight=132;
            foreach(var text in row.GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.fontSize=36;text.resizeTextForBestFit=false;text.raycastTarget=false;}
        }
        var oldLayout=mobile.GetComponent<JadeUIRuntimeLayout>();Undo.RecordObject(oldLayout,"Disable conflicting legacy layout");oldLayout.enabled=false;
        var oldMenu=mobile.GetComponent<JadeSecondaryMenu>();Undo.RecordObject(oldMenu,"Replace legacy secondary presentation");oldMenu.enabled=false;
        mobile.Find("JadeMenuDismiss")?.gameObject.SetActive(false);
        Undo.RecordObject(drawer,"Keep drawer commands and new HUD view references");
        drawer.mobileDock=hud.dock;drawer.dockToggleButton=hud.focusButton;drawer.playerHpBar=null;
        foreach(var panel in drawer.drawerPanels)
        {
            // Leave workbench content and slot topology intact. Align the sheet with the new bottom dock.
            panel.anchorMin=new Vector2(0,0);panel.anchorMax=new Vector2(1,.55f);panel.pivot=new Vector2(.5f,0);
            panel.offsetMin=new Vector2(16,188);panel.offsetMax=new Vector2(-16,188);
        }
        hud.ConfigureViews();hud.ApplyLayout();
        foreach(var graphic in hud.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            if(graphic.GetComponent<UnityEngine.UI.Button>()==null && graphic.transform!=hud.profile)graphic.raycastTarget=false;
        foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(button.targetGraphic!=null)button.targetGraphic.raycastTarget=true;
            button.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
        }
        hud.secondaryOverlay.SetActive(false);
        OrderLayers(hud,drawer);
        Directory.CreateDirectory(PrefabFolder);
        PrefabUtility.SaveAsPrefabAsset(hud.gameObject,PrefabFolder+"/PortraitHudShell.prefab");
        PrefabUtility.SaveAsPrefabAsset(hud.dock.gameObject,PrefabFolder+"/HudCoreDock.prefab");
        SaveExamplePrefabs();
        EditorUtility.SetDirty(hud);EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        Debug.Log("[Portrait HUD] New functional mobile shell saved. Original control IDs, services and slot topology retained.");
    }

    static void BuildSecondary(RectTransform shell)
    {
        var overlay=Node(shell,"SecondaryOverlay");Fill(overlay);hud.secondaryOverlay=overlay.gameObject;
        hud.menuBackdrop=Button(overlay,"MenuDimmer","");Fill((RectTransform)hud.menuBackdrop.transform);
        var dimmer=hud.menuBackdrop.GetComponent<UnityEngine.UI.Image>();dimmer.sprite=null;dimmer.color=new Color(.008f,.04f,.06f,.72f);
        var card=Node(overlay,"MenuCard");Image(card,null,Surface).raycastTarget=true;
        var frame=Node(card,"OrnamentFrame");Fill(frame);Image(frame,theme.window,Color.white);
        hud.menuTitle=Label(card,"Title","Đạo lộ",60);Rect(hud.menuTitle.rectTransform,0,1,1,1,-72,-32,-144,84);
        hud.closeMenuButton=Button(card,"CloseMenu","Đóng");Rect((RectTransform)hud.closeMenuButton.transform,1,1,1,1,-24,-24,144,144);
        var grid=Node(card,"MenuGrid");PortraitHudController.Fill(grid,36,36,36,192);hud.menuGrid=grid.gameObject;
        hud.profileButton=Button(grid,"ProfileButton","Nhân vật");
        hud.achievementButton=Move("BtnHUDAchievement",grid).GetComponent<UnityEngine.UI.Button>();hud.achievementButton.gameObject.SetActive(true);
        hud.achievementButton.GetComponentInChildren<UnityEngine.UI.Text>(true).text="Thành tựu";
        hud.buffsButton=Button(grid,"BuffsButton","Hiệu ứng · 0");hud.buffSummary=hud.buffsButton.GetComponentInChildren<UnityEngine.UI.Text>();
        hud.logButton=Button(grid,"LogButton","Nhật ký");
        var buttons=new[]{hud.profileButton,hud.achievementButton,hud.buffsButton,hud.logButton};
        for(int i=0;i<buttons.Length;i++)Rect((RectTransform)buttons[i].transform,0,1,1,1,0,-i*168,0,144);
        var detail=Node(card,"DetailsPage");PortraitHudController.Fill(detail,48,48,48,192);hud.detailsPage=detail.gameObject;
        hud.detailsText=Label(detail,"DetailsText","",42);Fill(hud.detailsText.rectTransform);hud.detailsText.alignment=TextAnchor.UpperLeft;
        hud.detailsPage.SetActive(false);
    }
    static void SaveExamplePrefabs()
    {
        var example=Button(null,"HudActionButton","Hành động");Rect((RectTransform)example.transform,.5f,.5f,.5f,.5f,0,0,300,144);
        PrefabUtility.SaveAsPrefabAsset(example.gameObject,PrefabFolder+"/HudActionButton.prefab");UnityEngine.Object.DestroyImmediate(example.gameObject);
        var chip=Node(null,"HudResourceChip");Rect(chip,.5f,.5f,.5f,.5f,0,0,318,144);Image(chip,theme.resourceBar,Color.white);
        var label=Label(chip,"Value","Linh Thạch 0",42);PortraitHudController.Fill(label.rectTransform,24,12,24,12);
        PrefabUtility.SaveAsPrefabAsset(chip.gameObject,PrefabFolder+"/HudResourceChip.prefab");UnityEngine.Object.DestroyImmediate(chip.gameObject);
    }
    [MenuItem("TuTien/UI/Portrait HUD/02 Reflow existing prototype")]
    public static void Reflow()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Reflow persistent layout in Edit Mode.");
        var h=UnityEngine.Object.FindAnyObjectByType<PortraitHudController>(FindObjectsInactive.Include);if(h==null)throw new InvalidOperationException("No prototype.");
        var box=h.secondaryOverlay.transform.Find("MenuCard") as RectTransform;
        var background=box.GetComponent<UnityEngine.UI.Image>();background.sprite=null;background.color=Surface;background.raycastTarget=true;
        if(box.Find("OrnamentFrame")==null){var frame=Node(box,"OrnamentFrame");Fill(frame);Image(frame,h.windowSprite,Color.white);frame.SetAsFirstSibling();}
        foreach(var bar in new[]{h.hp,h.mana})if(bar.GetComponent<UnityEngine.UI.Image>()==null)Undo.AddComponent<UnityEngine.UI.Image>(bar.gameObject);
        if(h.manaValue==null)h.manaValue=Label(h.mana,"ManaValue","MP 0 / 100",36);
        if(h.dock.GetComponent<CanvasGroup>()==null)Undo.AddComponent<CanvasGroup>(h.dock.gameObject);
        var controller=h.GetComponentInParent<MobileDrawerUIController>();
        OrderLayers(h,controller);
        h.ConfigureViews();h.ApplyLayout();
        PrefabUtility.SaveAsPrefabAsset(h.gameObject,PrefabFolder+"/PortraitHudShell.prefab");
        PrefabUtility.SaveAsPrefabAsset(h.dock.gameObject,PrefabFolder+"/HudCoreDock.prefab");
        EditorUtility.SetDirty(h);EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveScene(h.gameObject.scene);AssetDatabase.SaveAssets();
    }
    static void OrderLayers(PortraitHudController h,MobileDrawerUIController controller)
    {
        h.transform.SetAsFirstSibling();
        if(h.dock.parent!=controller.transform)Undo.SetTransformParent(h.dock,controller.transform,"Keep navigation above drawers");
        int last=controller.drawerPanels.Max(p=>p.GetSiblingIndex());h.dock.SetSiblingIndex(last+1);
        // The original modal roots remain after this subtree, so all HUD navigation stays below modals.
    }
    static RectTransform Move(string name,Transform parent)
    {
        var item=mobile.Find(name) as RectTransform;if(item==null)throw new InvalidOperationException("Required original control missing: "+name);
        Undo.SetTransformParent(item,parent,"Move original HUD control");return item;
    }
    static RectTransform Node(Transform parent,string name)
    {var go=new GameObject(name,typeof(RectTransform));Undo.RegisterCreatedObjectUndo(go,"Create HUD view");go.transform.SetParent(parent,false);return(RectTransform)go.transform;}
    static UnityEngine.UI.Image Image(RectTransform r,Sprite sprite,Color color)
    {var image=r.GetComponent<UnityEngine.UI.Image>()??Undo.AddComponent<UnityEngine.UI.Image>(r.gameObject);JadeUITheme.SetSurface(image,sprite);image.color=color;image.raycastTarget=false;return image;}
    static void SurfaceImage(RectTransform r,Sprite sprite,bool hit)
    {var image=Image(r,sprite,Color.white);image.raycastTarget=hit;}
    static UnityEngine.UI.Text Label(Transform parent,string name,string value,int size)
    {var r=Node(parent,name);var text=Undo.AddComponent<UnityEngine.UI.Text>(r.gameObject);text.font=UiTextStyle.BodyFont;text.text=value;text.fontSize=size;text.color=Ivory;text.alignment=TextAnchor.MiddleLeft;text.raycastTarget=false;text.resizeTextForBestFit=false;text.supportRichText=true;return text;}
    static UnityEngine.UI.Button Button(Transform parent,string name,string value)
    {var r=Node(parent,name);var image=Image(r,theme.secondaryButton,Color.white);image.raycastTarget=true;var button=Undo.AddComponent<UnityEngine.UI.Button>(r.gameObject);button.targetGraphic=image;var colors=button.colors;colors.pressedColor=new Color(.68f,.86f,.78f);colors.disabledColor=new Color(.55f,.62f,.59f);colors.fadeDuration=.08f;button.colors=colors;var label=Label(r,"Label",value,42);Fill(label.rectTransform);label.alignment=TextAnchor.MiddleCenter;Undo.AddComponent<JadeButtonFeedback>(r.gameObject);return button;}
    static void Fill(RectTransform r){PortraitHudController.Fill(r,0,0,0,0);}
    static void Rect(RectTransform r,float ax,float ay,float bx,float by,float x,float y,float w,float h){PortraitHudController.Place(r,ax,ay,bx,by,x,y,w,h);}
}
#endif

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MobileUIReconstructionBuilder
{
    public const string ScenePath = "Assets/Scenes/SampleScene_MobileWork.unity";
    public const string Folder = "Assets/UI/Reconstruction";
    public const string Evidence = "Docs/UI_Redesign/Backups/Phase06-20261112-212200";
    static MobileUIAssetLibrary art;
    static readonly Color Ivory = new Color32(241,232,207,255);
    static readonly Color Gold = new Color32(213,181,116,255);

    [MenuItem("TuTien/UI/Reconstruction/01 Build Phase 06")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != ScenePath || scene.isDirty)
            throw new InvalidOperationException("Build only the saved mobile work scene in Edit Mode.");
        if (!File.Exists(Evidence + "/editor-baseline.json")) throw new InvalidOperationException("Capture Phase06 baseline first.");
        var canvas = GameObject.Find("Canvas");
        if (canvas == null || canvas.GetComponent<MobileUIReconstruction>() != null)
            throw new InvalidOperationException("Missing Canvas or migration already exists; do not rebuild user work.");
        const string backup = "Assets/Scenes/Backups/SampleScene_MobileWork_PrePhase06.unity";
        if (!File.Exists(backup))
        {
            if (!AssetDatabase.CopyAsset(ScenePath, backup)) throw new IOException("Cannot retain original scene.");
        }
        else
        {
            var baseline=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Evidence+"/baseline.json"));
            using(var hash=System.Security.Cryptography.SHA256.Create())
            {
                string actual=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(backup))).Replace("-","").ToLowerInvariant();
                if(actual!=(string)baseline["files"][ScenePath])throw new InvalidOperationException("Retained scene hash mismatch.");
            }
        }
        CreateLibrary();
        CreatePrefabs();
        Undo.IncrementCurrentGroup(); Undo.SetCurrentGroupName("Mobile UI Phase06 reconstruction");
        Undo.RegisterFullObjectHierarchyUndo(canvas, "Reconstruct mobile UI presentation");
        var root = Undo.AddComponent<MobileUIReconstruction>(canvas);
        root.assets = art;
        root.theme = canvas.GetComponent<JadeUITheme>();
        root.drawers = canvas.GetComponentInChildren<MobileDrawerUIController>(true);
        root.navigation = root.drawers.GetComponent<MobileUINavigationManager>();
        root.theme.reconstruction = root;
        var theme = root.theme;
        theme.panel = art.panel; theme.window = art.panel; theme.slot = art.slot;
        theme.selectedSlot = art.selection; theme.primaryButton = art.upgrade;
        theme.secondaryButton = art.navigation; theme.resourceBar = art.track; theme.divider = art.divider;
        theme.spiritStone = art.stone; theme.spiritFlame = art.fate; theme.cultivationHalo = art.cultivation;
        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = 0;
        scaler.referencePixelsPerUnit = 100;
        if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null) Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(canvas);
        BuildMenu(root);
        BuildWorkbenches(root);
        var hud = canvas.GetComponentInChildren<PortraitHudController>(true);
        hud.panelSprite = art.panel; hud.windowSprite = art.panel; hud.slotSprite = art.slot; hud.selectedSprite = art.selection;
        foreach (var image in hud.wallet.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if (image.name == "JadeCurrency_0") image.sprite = art.stone;
            if (image.name == "JadeCurrency_1") image.sprite = art.fate;
        }
        var halo = hud.profile.Find("JadeHalo")?.GetComponent<UnityEngine.UI.Image>();
        if (halo != null) halo.sprite = art.cultivation;
        root.RefreshPresentation();
        ReflowContent(root);
        var links = new List<MobileUIReconstruction.Screen>();
        var mobile = root.drawers.transform;
        links.Add(Link("game.hud", "HUD", hud.gameObject));
        string[] tabIds = {"sword","body","alchemy","artifact","formation"};
        for(int i=0;i<5;i++) links.Add(Link("workbench."+tabIds[i],"Workbench",root.drawers.drawerPanels[i].gameObject));
        var menu = canvas.GetComponentInChildren<MainMenuManager>(true);
        links.Add(Link("title.home","Page",menu.mainPanel));
        links.Add(Link("title.roster","Page",menu.characterSelectPanel));
        links.Add(Link("title.create","Page",menu.characterCreatePanel));
        links.Add(Link("settings.audio","Page",menu.settingsPanel));
        links.Add(Link("title.confirm","Overlay",menu.confirmationPopup));
        foreach(Transform child in canvas.transform)
        {
            if(child.name.EndsWith("Modal") || child.name.EndsWith("ModalUI") || child.name == "SecretRealmPlayRoot")
                links.Add(Link(child.name, child.name == "SecretRealmPlayRoot"?"Page":"Overlay",child.gameObject));
        }
        foreach(string name in new[]{"ActiveBuffPanel","CombatLogPanel","BodyTalentDetailModal"})
        {
            var child=mobile.Find(name); if(child!=null) links.Add(Link(name,"Overlay",child.gameObject));
        }
        root.screens = links.ToArray();
        foreach(var scroll in canvas.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
        {
            // Preserve dynamic/custom grid sizing. Only repair missing viewport masks, never invent a fitter for it.
            if(scroll.viewport!=null && scroll.viewport.GetComponent<UnityEngine.UI.Mask>()==null && scroll.viewport.GetComponent<UnityEngine.UI.RectMask2D>()==null)
                Undo.AddComponent<UnityEngine.UI.RectMask2D>(scroll.viewport.gameObject);
        }
        foreach(var image in canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true)) MobileUISlicedSurface.Configure(image);
        foreach(var component in canvas.GetComponentsInChildren<Component>(true))
            if(component!=null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        EditorUtility.SetDirty(root); EditorUtility.SetDirty(theme);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[UI Phase06] Reconstructed mobile views with generated art; retained original IDs, controls and callback owners.");
    }
    static MobileUIReconstruction.Screen Link(string key,string layer,GameObject target)
        => new MobileUIReconstruction.Screen{key=key,layer=layer,target=target};

    [MenuItem("TuTien/UI/Reconstruction/02 Refresh presentation")]
    public static void Refresh()
    {
        if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Mobile Edit mode only");
        var root=GameObject.Find("Canvas").GetComponent<MobileUIReconstruction>();
        if(root==null)throw new InvalidOperationException("Build first");art=root.assets;
        var menu=root.GetComponentInChildren<MainMenuManager>(true);
        foreach(var slider in new[]{menu.bgmSlider,menu.sfxSlider})RepairSlider(slider);
        ReflowContent(root);root.RefreshPresentation();root.ApplyHudPresentation();Canvas.ForceUpdateCanvases();
        foreach(var image in root.GetComponentsInChildren<UnityEngine.UI.Image>(true))MobileUISlicedSurface.Configure(image);
        foreach(var component in root.GetComponentsInChildren<Component>(true))
            if(component!=null&&PrefabUtility.IsPartOfPrefabInstance(component))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{Folder+"/Prefabs"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var prefab=PrefabUtility.LoadPrefabContents(path);
            try{foreach(var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true))MobileUISlicedSurface.Configure(image);PrefabUtility.SaveAsPrefabAsset(prefab,path);}
            finally{PrefabUtility.UnloadPrefabContents(prefab);}
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
    }
    static void RepairSlider(UnityEngine.UI.Slider slider)
    {
        var handleArea=slider.transform.Find("Handle Slide Area") as RectTransform;
        if(handleArea!=null)Bottom(handleArea,-8,132,24);
        if(slider.handleRect!=null)
        {
            At(slider.handleRect,Vector2.zero,new Vector2(0,1),new Vector2(.5f,.5f),Vector2.zero,new Vector2(132,0));
            var image=slider.handleRect.GetComponent<UnityEngine.UI.Image>();if(image!=null)JadeUITheme.SetSurface(image,art.navigation);
        }
        if(slider.fillRect!=null)
        {
            var image=slider.fillRect.GetComponent<UnityEngine.UI.Image>();if(image!=null){image.sprite=null;image.color=new Color32(110,221,182,255);image.raycastTarget=false;}
        }
    }
    static Sprite Sprite(string name)
    {
        string path="Assets/Art/UI/Generated/JadeV5_"+name+".png";
        var result=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().SingleOrDefault();
        if(result==null)throw new InvalidOperationException("Missing generated sprite: "+path);
        return result;
    }
    static void CreateLibrary()
    {
        Directory.CreateDirectory(Folder+"/Prefabs");
        string path=Folder+"/MobileUIAssets.asset";
        if(File.Exists(path))throw new InvalidOperationException("New library path already exists.");
        art=ScriptableObject.CreateInstance<MobileUIAssetLibrary>();
        art.panel=Sprite("PanelBackground");art.border=Sprite("GoldJadeBorder");art.popup=Sprite("PopupFrame");art.hud=Sprite("HudContainer");
        art.navigation=Sprite("NavigationButton");art.upgrade=Sprite("UpgradeButton");art.slot=Sprite("EquipmentSlot");art.selection=Sprite("SelectionFrame");
        art.reward=Sprite("RewardFrame");art.tab=Sprite("TabBackground");art.track=Sprite("ResourceTrack");art.divider=Sprite("SectionDivider");
        art.stone=Sprite("ResourceSpiritStone");art.fate=Sprite("ResourceCoDuyen");art.ore=Sprite("ResourceOre");art.dust=Sprite("ResourceMedicineDust");
        art.cultivation=Sprite("CultivationHalo");art.alchemy=Sprite("AlchemyDecoration");art.formation=Sprite("FormationDecoration");
        art.rarity=new[]{"Neutral","Green","Blue","Violet","Orange","Gold"}.Select(x=>Sprite("Rarity"+x)).ToArray();
        art.regular=UiTextStyle.BodyFont;
        art.semibold=AssetDatabase.LoadAssetAtPath<Font>("Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf");
        AssetDatabase.CreateAsset(art,path);
    }
    static RectTransform Node(Transform parent,string name)
    {
        var go=new GameObject(name,typeof(RectTransform));var rt=(RectTransform)go.transform;
        if(parent!=null)rt.SetParent(parent,false);
        return rt;
    }
    static void Fill(RectTransform rt,float left=0,float bottom=0,float right=0,float top=0)
    {rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.pivot=new Vector2(.5f,.5f);rt.offsetMin=new Vector2(left,bottom);rt.offsetMax=new Vector2(-right,-top);}
    static void At(RectTransform rt,Vector2 min,Vector2 max,Vector2 pivot,Vector2 position,Vector2 size)
    {rt.anchorMin=min;rt.anchorMax=max;rt.pivot=pivot;rt.anchoredPosition=position;rt.sizeDelta=size;}
    static void Top(RectTransform rt,float y,float height,float inset=24)
        =>At(rt,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-y),new Vector2(-inset*2,height));
    static void Bottom(RectTransform rt,float y,float height,float inset=24)
        =>At(rt,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,y),new Vector2(-inset*2,height));
    static UnityEngine.UI.Image Image(RectTransform rt,Sprite sprite,bool raycast=false)
    {
        var image=rt.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.color=Color.white;
        image.type=sprite!=null&&sprite.border.sqrMagnitude>0?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;
        image.raycastTarget=raycast;image.preserveAspect=sprite!=null&&sprite.border.sqrMagnitude==0;
        MobileUISlicedSurface.Configure(image);return image;
    }
    static UnityEngine.UI.Text Text(RectTransform rt,string value,int size=42)
    {
        var text=rt.gameObject.AddComponent<UnityEngine.UI.Text>();text.text=value;text.font=art.regular;text.fontSize=size;
        text.color=Ivory;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
        text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
    }
    static void Label(UnityEngine.UI.Text text,int size,TextAnchor alignment=TextAnchor.MiddleCenter)
    {if(text==null)return;text.font=art.regular;text.fontSize=size;text.resizeTextForBestFit=false;text.color=Ivory;text.alignment=alignment;}
    static GameObject Save(RectTransform root,string name)
    {
        var path=Folder+"/Prefabs/"+name+".prefab";
        if(File.Exists(path))throw new InvalidOperationException("Refusing to overwrite "+path);
        var saved=PrefabUtility.SaveAsPrefabAsset(root.gameObject,path);UnityEngine.Object.DestroyImmediate(root.gameObject);return saved;
    }
    static MobileUIPanelView Panel(string name,Sprite rim)
    {
        var rt=Node(null,name);rt.sizeDelta=new Vector2(984,1000);
        var view=rt.gameObject.AddComponent<MobileUIPanelView>();
        var surface=Node(rt,"P6Surface");Fill(surface);view.surface=Image(surface,art.panel);
        var ornament=Node(rt,"P6Ornament");Fill(ornament,8,8,8,8);view.ornament=Image(ornament,rim);
        view.header=Node(rt,"P6Header");Top(view.header,16,176,16);
        view.body=Node(rt,"P6Body");Fill(view.body,24,168,24,216);
        view.footer=Node(rt,"P6Footer");Bottom(view.footer,24,144,24);
        var title=Node(view.header,"P6Title");Fill(title,144,24,168,24);view.title=Text(title,"Tiêu đề",48);
        var emblem=Node(view.header,"P6Emblem");At(emblem,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(24,0),new Vector2(108,108));view.emblem=Image(emblem,null);
        return Save(rt,name).GetComponent<MobileUIPanelView>();
    }
    static void CreatePrefabs()
    {
        art.workbenchPrefab=Panel("WorkbenchFrame",art.border);
        art.modalPrefab=Panel("ModalFrame",art.popup);
        art.pagePrefab=Panel("PageFrame",art.border);
        foreach(var pair in new[]{("CostActionButton",art.upgrade),("NavigationButton",art.navigation),("TabButton",art.tab)})
        {
            var rt=Node(null,pair.Item1);rt.sizeDelta=new Vector2(480,144);
            var image=Image(rt,pair.Item2,true);var button=rt.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,1,.88f);colors.pressedColor=new Color(.72f,.85f,.78f);colors.disabledColor=new Color(.45f,.5f,.48f);button.colors=colors;
            var caption=Node(rt,"P6Caption");Top(caption,14,62,72);var label=Text(caption,"Hành động",42);
            var val=Node(rt,"P6Value");Bottom(val,12,48,72);var value=Text(val,"",36);
            var state=rt.gameObject.AddComponent<MobileUIActionView>();state.button=button;state.caption=label;state.value=value;
            Save(rt,pair.Item1);
        }
        {
            var rt=Node(null,"ResourceCounter");rt.sizeDelta=new Vector2(360,120);Image(rt,art.hud);
            var icon=Node(rt,"P6Icon");At(icon,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(18,0),new Vector2(84,84));Image(icon,art.stone);
            var value=Node(rt,"P6Value");Fill(value,114,12,24,12);Text(value,"",42);Save(rt,"ResourceCounter");
        }
        {
            var rt=Node(null,"EquipmentItem");rt.sizeDelta=new Vector2(192,240);var view=rt.gameObject.AddComponent<MobileUIItemView>();view.assets=art;
            var slot=Node(rt,"P6Slot");Top(slot,0,192,0);Image(slot,art.slot);
            var icon=Node(slot,"P6ItemIcon");Fill(icon,36,36,36,36);view.icon=Image(icon,null);
            var frame=Node(slot,"P6Rarity");Fill(frame);view.rarity=Image(frame,art.rarity[0]);
            var selected=Node(slot,"P6Selection");Fill(selected);view.selection=Image(selected,art.selection);view.selection.enabled=false;
            var label=Node(rt,"P6Level");Bottom(label,0,48,0);view.caption=Text(label,"",36);
            var count=Node(slot,"P6Count");Bottom(count,8,48,12);view.count=Text(count,"",36);Save(rt,"EquipmentItem");
        }
        {
            var rt=Node(null,"RewardFrame");rt.sizeDelta=new Vector2(720,960);Image(rt,art.panel);
            var frame=Node(rt,"P6RewardRim");Fill(frame);Image(frame,art.reward);Save(rt,"RewardFrame");
        }
        {
            var rt=Node(null,"ScrollBody");rt.sizeDelta=new Vector2(936,720);
            var scroll=rt.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;
            var viewport=Node(rt,"P6Viewport");Fill(viewport);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var content=Node(viewport,"P6Content");At(content,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),Vector2.zero,new Vector2(0,0));
            var group=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();group.spacing=24;group.padding=new RectOffset(12,12,12,12);group.childControlWidth=true;group.childControlHeight=true;group.childForceExpandHeight=false;
            var fitter=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fitter.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport;scroll.content=content;art.scrollPrefab=Save(rt,"ScrollBody").GetComponent<UnityEngine.UI.ScrollRect>();
        }
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
    }
    static void Move(Transform child,Transform parent)
    {if(child!=null)Undo.SetTransformParent(child,parent,"Retain UI object identity");}
    static void Action(UnityEngine.UI.Button button,Sprite sprite,int font=48)
    {
        if(button==null)return;
        var image=button.GetComponent<UnityEngine.UI.Image>();if(image!=null)JadeUITheme.SetSurface(image,sprite);
        foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))Label(text,font);
        var element=button.GetComponent<UnityEngine.UI.LayoutElement>()??button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.minHeight=144;element.preferredHeight=144;
    }
    static void BuildMenu(MobileUIReconstruction root)
    {
        var menu=root.GetComponentInChildren<MainMenuManager>(true);
        var home=menu.mainPanel.transform;
        root.titleHeader=Node(home,"P6TitleHeader");Top(root.titleHeader,72,520,36);
        root.titleActions=Node(home,"P6PrimaryActions");At(root.titleActions,new Vector2(0,.16f),new Vector2(1,.16f),new Vector2(.5f,0),Vector2.zero,new Vector2(-144,504));
        var group=root.titleActions.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();group.spacing=24;group.childControlWidth=true;group.childControlHeight=true;group.childForceExpandHeight=false;
        root.titleFooter=Node(home,"P6TitleFooter");Bottom(root.titleFooter,36,120,48);
        var title=home.Find("TitleText");Move(title,root.titleHeader);Top((RectTransform)title,8,280,0);Label(title.GetComponent<UnityEngine.UI.Text>(),96);
        var subtitle=home.Find("JadeSubtitle");Move(subtitle,root.titleHeader);Bottom((RectTransform)subtitle,56,96,0);Label(subtitle.GetComponent<UnityEngine.UI.Text>(),36);subtitle.GetComponent<UnityEngine.UI.Text>().color=Gold;
        var divider=home.Find("JadeTitleDivider");Move(divider,root.titleFooter);Fill((RectTransform)divider,60,0,60,0);divider.GetComponent<UnityEngine.UI.Image>().sprite=art.divider;
        foreach(var button in new[]{menu.playButton,menu.settingsButton,menu.quitButton})Move(button.transform,root.titleActions);
        Action(menu.playButton,art.upgrade);Action(menu.settingsButton,art.navigation);Action(menu.quitButton,art.navigation);
        var roster=menu.characterSelectPanel.transform;
        var head=Node(roster,"P6Header");Top(head,36,168,36);var selectTitle=roster.Find("SelectTitle");Move(selectTitle,head);Fill((RectTransform)selectTitle);Label(selectTitle.GetComponent<UnityEngine.UI.Text>(),60);
        var slots=(RectTransform)roster.Find("SlotContainer");Fill(slots,48,240,48,240);
        var slotGroup=slots.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();if(slotGroup!=null){slotGroup.spacing=24;slotGroup.childControlHeight=true;slotGroup.childForceExpandHeight=false;}
        foreach(var slot in menu.saveSlots)
        {
            var element=slot.GetComponent<UnityEngine.UI.LayoutElement>()??slot.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.minHeight=288;element.preferredHeight=288;
            JadeUITheme.SetSurface(slot.GetComponent<UnityEngine.UI.Image>(),art.panel);
            Label(slot.nameText,42,TextAnchor.MiddleLeft);Label(slot.cultivationText,36,TextAnchor.MiddleLeft);Label(slot.playTimeText,36,TextAnchor.MiddleLeft);
            if(slot.deleteButton!=null){Action(slot.deleteButton,art.navigation,36);var del=(RectTransform)slot.deleteButton.transform;At(del,Vector2.one,Vector2.one,Vector2.one,new Vector2(-16,-16),new Vector2(144,144));}
        }
        var rosterFooter=Node(roster,"P6Footer");Bottom(rosterFooter,36,144,96);Move(menu.backFromSelectButton.transform,rosterFooter);Fill((RectTransform)menu.backFromSelectButton.transform);Action(menu.backFromSelectButton,art.navigation);
        var create=menu.characterCreatePanel.transform;var createHeader=Node(create,"P6Header");Top(createHeader,96,180,36);var createTitle=create.Find("CreateTitle");Move(createTitle,createHeader);Fill((RectTransform)createTitle);Label(createTitle.GetComponent<UnityEngine.UI.Text>(),60);
        var createBody=Node(create,"P6Body");Fill(createBody,72,480,72,360);Move(menu.nameInput.transform,createBody);Top((RectTransform)menu.nameInput.transform,144,168,0);JadeUITheme.SetSurface(menu.nameInput.GetComponent<UnityEngine.UI.Image>(),art.panel);
        var prompt=menu.nameInput.transform.Find("Label");At((RectTransform)prompt,new Vector2(0,1),Vector2.one,new Vector2(.5f,0),new Vector2(0,24),new Vector2(0,96));Label(prompt.GetComponent<UnityEngine.UI.Text>(),42,TextAnchor.MiddleLeft);
        Fill(menu.nameInput.textComponent.rectTransform,36,12,36,12);Label(menu.nameInput.textComponent,48,TextAnchor.MiddleLeft);
        var createFooter=Node(create,"P6Footer");Bottom(createFooter,72,312,96);var createActions=createFooter.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();createActions.spacing=24;createActions.childControlHeight=true;createActions.childControlWidth=true;createActions.childForceExpandHeight=false;
        Move(menu.confirmCreateButton.transform,createFooter);Move(menu.cancelCreateButton.transform,createFooter);Action(menu.confirmCreateButton,art.upgrade);Action(menu.cancelCreateButton,art.navigation);
        var settings=menu.settingsPanel.transform;var settingsHeader=Node(settings,"P6Header");Top(settingsHeader,72,180,36);var heading=settings.Find("JadeHeading");Move(heading,settingsHeader);Fill((RectTransform)heading);Label(heading.GetComponent<UnityEngine.UI.Text>(),60);
        var settingsBody=Node(settings,"P6Body");Fill(settingsBody,96,360,96,360);var settingLayout=settingsBody.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();settingLayout.spacing=72;settingLayout.childControlWidth=true;settingLayout.childControlHeight=true;settingLayout.childForceExpandHeight=false;
        foreach(var slider in new[]{menu.bgmSlider,menu.sfxSlider})
        {
            Move(slider.transform,settingsBody);var element=slider.GetComponent<UnityEngine.UI.LayoutElement>()??slider.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.minHeight=216;element.preferredHeight=216;
            var sliderLabel=slider.transform.Find("Label") as RectTransform;Top(sliderLabel,0,84,0);Label(sliderLabel.GetComponent<UnityEngine.UI.Text>(),42,TextAnchor.MiddleLeft);
            var bg=slider.transform.Find("Background") as RectTransform;Bottom(bg,16,84,0);JadeUITheme.SetSurface(bg.GetComponent<UnityEngine.UI.Image>(),art.track);
            var fill=slider.transform.Find("Fill Area") as RectTransform;Bottom(fill,40,36,24);if(slider.handleRect!=null)slider.handleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,132);
            RepairSlider(slider);
        }
        var settingsFooter=Node(settings,"P6Footer");Bottom(settingsFooter,48,144,96);Move(menu.closeSettingsButton.transform,settingsFooter);Fill((RectTransform)menu.closeSettingsButton.transform);Action(menu.closeSettingsButton,art.upgrade);
    }
    static void BuildWorkbenches(MobileUIReconstruction root)
    {
        for(int i=0;i<root.drawers.drawerPanels.Length;i++)
        {
            var panel=root.drawers.drawerPanels[i];
            JadeUITheme.SetSurface(panel.GetComponent<UnityEngine.UI.Image>(),art.panel);
            var view=root.MountFrame(panel,art.workbenchPrefab);
            var title=panel.Find("Title");var close=panel.Find("CloseButton");var handle=panel.Find("SwipeHandle");
            if(title!=null){Move(title,view.header);view.title=title.GetComponent<UnityEngine.UI.Text>();view.title.gameObject.SetActive(true);Fill((RectTransform)title,144,32,168,32);Label(view.title,48,TextAnchor.MiddleLeft);}
            if(close!=null){Move(close,view.header);At((RectTransform)close,Vector2.one,Vector2.one,Vector2.one,new Vector2(-8,-16),new Vector2(132,132));Action(close.GetComponent<UnityEngine.UI.Button>(),art.navigation,42);}
            if(handle!=null){Move(handle,view.header);At((RectTransform)handle,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(.5f,1),Vector2.zero,new Vector2(300,42));}
            view.SetEmblem(i==2?art.alchemy:i==4?art.formation:i==1?art.cultivation:null);
            var content=i==0?panel.Find("MergePanel") as RectTransform:panel.Find(i==2?"JadeAlchemyScroll":i==3?"ArtifactScrollView":i==4?"FormationScrollView":"TreeScrollView") as RectTransform;
            if(content!=null)view.body=content;
            view.footer.gameObject.SetActive(false);
        }
    }
    static void ReflowContent(MobileUIReconstruction root)
    {
        var menu=root.GetComponentInChildren<MainMenuManager>(true);
        JadeUITheme.SetSurface(menu.nameInput.GetComponent<UnityEngine.UI.Image>(),art.navigation);
        Fill(menu.nameInput.textComponent.rectTransform,84,12,84,12);
        var legacyRoster=menu.characterSelectPanel.transform.Find("SlotContainer");
        var oldRowLayout=legacyRoster.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();if(oldRowLayout!=null)oldRowLayout.enabled=false;
        var roster=menu.characterSelectPanel.transform.Find("P6RosterBody") as RectTransform;
        if(roster==null)roster=Node(menu.characterSelectPanel.transform,"P6RosterBody");
        Fill(roster,48,240,48,240);
        var rosterLayout=roster.GetComponent<UnityEngine.UI.VerticalLayoutGroup>()??roster.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        rosterLayout.spacing=24;rosterLayout.childAlignment=TextAnchor.UpperCenter;rosterLayout.childControlWidth=true;rosterLayout.childControlHeight=true;rosterLayout.childForceExpandHeight=false;rosterLayout.childForceExpandWidth=true;
        foreach(var slot in menu.saveSlots)
        {
            Move(slot.transform,roster);
            var row=slot.GetComponent<UnityEngine.UI.LayoutElement>();row.minHeight=324;row.preferredHeight=324;
            Top(slot.nameText.rectTransform,16,108,180);Label(slot.nameText,42,TextAnchor.MiddleLeft);
            Top(slot.cultivationText.rectTransform,124,60,180);Label(slot.cultivationText,36,TextAnchor.MiddleLeft);
            Top(slot.playTimeText.rectTransform,184,96,180);Label(slot.playTimeText,36,TextAnchor.MiddleLeft);
            foreach(var text in new[]{slot.nameText,slot.cultivationText,slot.playTimeText})text.horizontalOverflow=HorizontalWrapMode.Wrap;
        }
        legacyRoster.gameObject.SetActive(false);
        if(art.scrollPrefab==null){art.scrollPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Prefabs/ScrollBody.prefab").GetComponent<UnityEngine.UI.ScrollRect>();EditorUtility.SetDirty(art);}
        for(int i=0;i<5;i++)
        {
            var panel=root.drawers.drawerPanels[i];var frame=panel.Find("P6Frame").GetComponent<MobileUIPanelView>();
            var handle=frame.header.Find("SwipeHandle");
            if(handle!=null){Move(handle,frame.transform);At((RectTransform)handle,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,0),new Vector2(300,42));}
            var subtitle=panel.Find("SubTitle") as RectTransform;
            if(subtitle!=null){Move(subtitle,frame.header);Top(subtitle,112,48,144);Label(subtitle.GetComponent<UnityEngine.UI.Text>(),36,TextAnchor.MiddleLeft);}
            if(frame.title!=null){Top(frame.title.rectTransform,36,72,144);frame.title.alignment=TextAnchor.MiddleLeft;}
            if(i==0)
            {
                var outer=panel.Find("P6SwordScroll")?.GetComponent<UnityEngine.UI.ScrollRect>();
                if(outer==null)
                {
                    outer=((GameObject)PrefabUtility.InstantiatePrefab(art.scrollPrefab.gameObject,panel)).GetComponent<UnityEngine.UI.ScrollRect>();outer.name="P6SwordScroll";
                    root.swordContent=panel.Find("MergePanel") as RectTransform;
                    Move(root.swordContent,outer.content);
                }
                if(root.swordContent==null)root.swordContent=outer.content.Find("MergePanel") as RectTransform;
                Fill((RectTransform)outer.transform,24,216,24,208);frame.body=(RectTransform)outer.transform;
                var content=root.swordContent;
                var element=content.GetComponent<UnityEngine.UI.LayoutElement>()??content.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();element.minHeight=1016;element.preferredHeight=1016;
                var equip=content.Find("EquipPanel") as RectTransform;Top(equip,144,282,0);
                var inventory=content.Find("InventoryContainer") as RectTransform;Fill(inventory,0,0,0,450);
                var bag=inventory.Find("ScrollView") as RectTransform;Fill(bag,0,144,0,0);
                root.dismantleShortcut=content.Find("BtnDrawerDismantle") as RectTransform;
                At(root.dismantleShortcut,Vector2.one,Vector2.one,Vector2.one,new Vector2(-8,-8),new Vector2(430,132));
                var equipLabel=content.Find("JadeEquipLabel") as RectTransform;if(equipLabel!=null){Top(equipLabel,24,72,0);equipLabel.offsetMax=new Vector2(-454,equipLabel.offsetMax.y);}
            }
            else
            {
                string name=i==1?"TreeScrollView":i==2?"JadeAlchemyScroll":i==3?"ArtifactScrollView":"FormationScrollView";
                var scroll=panel.Find(name) as RectTransform;
                if(scroll!=null){Fill(scroll,24,216,24,i==1?304:216);frame.body=scroll;}
                if(i==1){var resource=panel.Find("ResourceBar") as RectTransform;if(resource!=null)Top(resource,208,72,24);}
            }
            frame.transform.SetAsFirstSibling();
        }
        EditorUtility.SetDirty(root);
    }
}
#endif

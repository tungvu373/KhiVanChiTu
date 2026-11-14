#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>In-place, scene-scoped redesign. Never recreates controls or clears UnityEvents.</summary>
public static class JadeUIRedesignTool
{
    const string AtlasPath = "Assets/Art/UI/Generated/JadeGoldAtlas.png";
    static JadeUITheme theme;
    static Transform canvas, mobile;
    static readonly string[] Names = { "Panel", "Window", "Slot", "SelectedSlot", "PrimaryButton", "SecondaryButton", "ResourceBar", "Divider", "Sword", "Body", "Alchemy", "Artifact", "Formation", "SpiritStone", "SpiritFlame", "CultivationHalo" };

    [MenuItem("TuTien/UI/Jade V2/01 Import original art")]
    public static void ImportArt()
    {
        AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
        var rects = new List<SpriteRect>();
        for (int i = 0; i < Names.Length; i++)
        {
            int row = i / 4, col = i % 4;
            // Trim only atlas padding; source pixels and transparency remain untouched.
            float left = .035f, right = .965f, top = .035f, bottom = .965f;
            if (row == 0) { left = .065f; right = .955f; top = .065f; bottom = .93f; }
            if (row == 1) { left = .025f; right = .98f; top = .30f; bottom = .68f; }
            if (i == 6) { top = .38f; bottom = .62f; }
            float cellW = texture.width / 4f, cellH = texture.height / 4f;
            var rect = new Rect((col + left) * cellW, texture.height - (row + bottom) * cellH, (right - left) * cellW, (bottom - top) * cellH);
            Vector4 border = Vector4.zero;
            if (i < 4) border = new Vector4(30, 30, 30, 30) * (texture.width / 1254f);
            else if (i < 7) border = new Vector4(52, 16, 52, 16) * (texture.width / 1254f);
            rects.Add(new SpriteRect { name = Names[i], rect = rect, alignment = SpriteAlignment.Center, pivot = new Vector2(.5f, .5f), border = border, spriteID = old.TryGetValue(Names[i], out var id) ? id : GUID.Generate() });
        }
        provider.SetSpriteRects(rects.ToArray());
        var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (ids != null) ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply(); importer.SaveAndReimport();
        Debug.Log("[Jade V2] Imported 16 reusable original sprites with alpha and 9-slice borders.");
    }

    [MenuItem("TuTien/UI/Jade V2/02 Apply complete presentation")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/SampleScene_MobileWork.unity")
            throw new InvalidOperationException("Apply only to the mobile work scene in Edit Mode.");
        if (!File.Exists(JadeUIRedesignAudit.Folder + "/baseline.json")) throw new InvalidOperationException("Record baseline first.");
        canvas = GameObject.Find("Canvas").transform;
        mobile = canvas.Find("MobileUIRoot");
        if (mobile == null) throw new InvalidOperationException("MobileUIRoot is missing.");
        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Jade V2 mobile redesign");
        theme = Get<JadeUITheme>(canvas.gameObject);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>().ToDictionary(s => s.name);
        theme.panel = sprites["Panel"]; theme.window = sprites["Window"]; theme.slot = sprites["Slot"]; theme.selectedSlot = sprites["SelectedSlot"];
        theme.primaryButton = sprites["PrimaryButton"]; theme.secondaryButton = sprites["SecondaryButton"]; theme.resourceBar = sprites["ResourceBar"]; theme.divider = sprites["Divider"];
        theme.navigationIcons = new[] { sprites["Sword"], sprites["Body"], sprites["Alchemy"], sprites["Artifact"], sprites["Formation"] };
        theme.spiritStone = sprites["SpiritStone"]; theme.spiritFlame = sprites["SpiritFlame"]; theme.cultivationHalo = sprites["CultivationHalo"];
        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0f;
        // Keep the original font-backed Text components: serialized bindings must not change type.
        foreach (var t in canvas.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            t.font = UiTextStyle.BodyFont;
            var selectable=t.GetComponent<UnityEngine.UI.Selectable>();
            t.raycastTarget=selectable!=null&&selectable.targetGraphic==t;
            t.text = UiTextStyle.RemoveIcons(t.text);
        }
        Header(); DockAndDrawers(); SwordFurnace(); Alchemy(); Lists(); Menus(); Dialogs(); ImproveCards();
        Get<JadeUIRuntimeLayout>(mobile.gameObject);
        // HUD is below the drawer backdrop; skill buttons can never cover drawer controls.
        mobile.Find("MobileBackdrop").SetAsLastSibling();
        for(int i=0;i<5;i++)mobile.Find("Drawer_"+i).SetAsLastSibling();
        mobile.Find("BottomDockToggle").SetAsLastSibling();mobile.Find("MobileDock").SetAsLastSibling();
        mobile.Find("JadeMenuDismiss").SetAsLastSibling();
        foreach(var action in mobile.GetComponent<JadeSecondaryMenu>().actions)if(action!=null)action.transform.SetAsLastSibling();
        mobile.Find("JadeMenuButton").SetAsLastSibling();mobile.Find("BodyTalentDetailModal").SetAsLastSibling();
        theme.RefreshPresentation();
        theme.StyleSkills();
        // Decorative borders supplied by the atlas replace the old doubled outlines.
        foreach (var o in mobile.GetComponentsInChildren<UnityEngine.UI.Outline>(true)) if(o.GetComponent<MergeSlot>()==null)o.enabled = false;
        CreateReusablePrefabs();
        EditorUtility.SetDirty(theme);
        EditorSceneManager.MarkSceneDirty(scene);
        Canvas.ForceUpdateCanvases();
        Debug.Log("[Jade V2] Presentation applied in place. Original controls, serialized references and events retained.");
    }
    static T Get<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    static RectTransform RT(Transform tr) => tr as RectTransform;
    static void Place(Transform tr, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        if (tr == null) return;
        var r = RT(tr); r.anchorMin = min; r.anchorMax = max; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos; r.localScale = Vector3.one;
    }
    static void Box(Transform tr, float left, float bottom, float right, float top)
    {
        if (tr == null) return;
        var r = RT(tr); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f,.5f); r.offsetMin = new Vector2(left,bottom); r.offsetMax = new Vector2(-right,-top); r.localScale = Vector3.one;
    }
    static void Top(Transform tr, float x, float y, float w, float h, float anchor = 0)
        => Place(tr, new Vector2(anchor,1), new Vector2(anchor,1), new Vector2(anchor,1), new Vector2(x,-y), new Vector2(w,h));
    static void StyleText(Transform tr, int size, TextAnchor align = TextAnchor.MiddleLeft, bool bold = false)
    {
        if (tr == null) return;
        var text = tr.GetComponent<UnityEngine.UI.Text>(); if (text == null) return;
        text.font = UiTextStyle.BodyFont; text.fontSize = size; text.color = JadeUITheme.Ivory;
        text.alignment = align; text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.resizeTextForBestFit = true; text.resizeTextMinSize = Mathf.Min(size, 28); text.resizeTextMaxSize = size;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
    }
    static Transform New(Transform parent, string name, params Type[] types)
    {
        var existing = parent.Find(name); if (existing != null) return existing;
        var go = new GameObject(name, new[] { typeof(RectTransform) }.Concat(types).ToArray());
        Undo.RegisterCreatedObjectUndo(go, "Jade V2 " + name); go.transform.SetParent(parent,false); return go.transform;
    }
    static Transform Label(Transform parent, string name, string value, int size)
    {
        var t = New(parent, name, typeof(UnityEngine.UI.Text)); t.GetComponent<UnityEngine.UI.Text>().text = value;
        StyleText(t,size); return t;
    }
    static Transform Art(Transform parent, string name, Sprite sprite)
    {
        var t = New(parent,name,typeof(UnityEngine.UI.Image)); var image = t.GetComponent<UnityEngine.UI.Image>();
        image.sprite = sprite; image.color = Color.white; image.preserveAspect = true; image.raycastTarget = false; return t;
    }
    static void Surface(Transform tr, Sprite sprite)
    {
        if (tr == null) return;
        JadeUITheme.SetSurface(Get<UnityEngine.UI.Image>(tr.gameObject),sprite);
    }
    static void Header()
    {
        var bar = mobile.Find("TopBar");
        Place(bar,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),Vector2.zero,new Vector2(0,182));
        Surface(bar,theme.panel); bar.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        var frame=mobile.Find("CharacterFrame"); Top(frame,28,26,550,140);
        frame.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
        Top(frame.Find("Avatar"),8,8,104,104); Surface(frame.Find("Avatar"),theme.slot);
        var emblem=Art(frame.Find("Avatar"),"JadeSwordEmblem",theme.navigationIcons[0]);Box(emblem,12,12,12,12);
        var halo=Art(frame,"JadeHalo",theme.cultivationHalo); Top(halo,0,0,120,120); halo.SetAsFirstSibling();
        var info=frame.Find("InfoText"); Box(info,136,10,4,8); StyleText(info,32,TextAnchor.MiddleLeft);
        frame.Find("DetailHintText")?.gameObject.SetActive(false);
        string[] bgs={"BG_LinhThach","BG_CoDuyen"}; string[] values={"txt_LinhThach","txt_CoDuyen"};
        for(int i=0;i<2;i++)
        {
            Top(bar.Find(bgs[i]),-180,22+i*74,294,64,1); Surface(bar.Find(bgs[i]),theme.resourceBar);
            Top(bar.Find(values[i]),-194,22+i*74,238,64,1); StyleText(bar.Find(values[i]),32,TextAnchor.MiddleRight,true);
            var icon=Art(bar,"JadeCurrency_"+i,i==0?theme.spiritStone:theme.spiritFlame); Top(icon,-414,32+i*74,40,44,1);
        }
        Top(mobile.Find("PauseButton"),-24,24,132,132,1);
        var pauseText=mobile.Find("PauseButton").GetComponentInChildren<UnityEngine.UI.Text>(true); StyleText(pauseText.transform,32,TextAnchor.MiddleCenter,true); pauseText.text="Dừng";
        var quest=mobile.Find("TutorialTargetQuestBar"); Top(quest,24,206,852,96); Surface(quest,theme.panel);
        foreach(var t in quest.GetComponentsInChildren<UnityEngine.UI.Text>(true)) StyleText(t.transform,30,TextAnchor.MiddleLeft);
        Top(mobile.Find("StageUI"),0,356,460,68,.5f); Surface(mobile.Find("StageUI"),theme.resourceBar);
        foreach(var t in mobile.Find("StageUI").GetComponentsInChildren<UnityEngine.UI.Text>(true)) StyleText(t.transform,30,TextAnchor.MiddleCenter,true);
        Top(mobile.Find("ActiveBuffPanel"),28,414,500,84);
        Place(mobile.Find("PlayerHPBar"),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,382),new Vector2(560,44));
        Place(mobile.Find("PlayerManaBar"),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,342),new Vector2(560,24));
        foreach(var t in mobile.Find("PlayerHPBar").GetComponentsInChildren<UnityEngine.UI.Text>(true))StyleText(t.transform,26,TextAnchor.MiddleCenter);
        Place(mobile.Find("StagePanel"),new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,264),new Vector2(-64,64));
        Place(mobile.Find("CombatLogPanel"),new Vector2(0,0),new Vector2(0,0),Vector2.zero,new Vector2(24,450),new Vector2(580,92));
        Place(mobile.Find("MobileSkillHUD"),new Vector2(1,.4f),new Vector2(1,.4f),new Vector2(1,0),new Vector2(-24,0),new Vector2(132,460));
        var skills=mobile.Find("MobileSkillHUD").GetComponent<UnityEngine.UI.VerticalLayoutGroup>(); if(skills!=null) skills.spacing=16;
        SecondaryMenu();
    }
    static void SecondaryMenu()
    {
        var dismiss=New(mobile,"JadeMenuDismiss",typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
        Box(dismiss,0,0,0,182); dismiss.GetComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,.32f); dismiss.SetSiblingIndex(8);
        var menu=Get<JadeSecondaryMenu>(mobile.gameObject);
        var toggle=New(mobile,"JadeMenuButton",typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
        Top(toggle,-24,202,156,132,1); Surface(toggle,theme.secondaryButton);
        var label=Label(toggle,"Label","Tiện ích",30); Box(label,8,4,8,4); StyleText(label,30,TextAnchor.MiddleCenter,true);
        string[] names={"BtnHUDStorage","BtnHUDDaoAn","BtnHUDBiCanh","BtnHUDAchievement"};
        string[] labels={"Kho đồ","Vận khí","Bí cảnh","Thành tựu"};
        menu.actions=new UnityEngine.UI.Button[names.Length];
        for(int i=0;i<names.Length;i++)
        {
            var action=mobile.Find(names[i]); if(action==null) continue;
            Top(action,-24,350+i*140,244,132,1); Surface(action,theme.secondaryButton);
            var txt=action.GetComponentInChildren<UnityEngine.UI.Text>(true); if(txt!=null){txt.text=labels[i];StyleText(txt.transform,32,TextAnchor.MiddleCenter,true);}
            menu.actions[i]=action.GetComponent<UnityEngine.UI.Button>();
            action.SetAsLastSibling(); action.gameObject.SetActive(false);
        }
        toggle.SetAsLastSibling(); menu.toggle=toggle.GetComponent<UnityEngine.UI.Button>(); menu.dismissArea=dismiss.gameObject; dismiss.gameObject.SetActive(false);
    }
    static void DockAndDrawers()
    {
        var dock=mobile.Find("MobileDock"); Place(dock,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),Vector2.zero,new Vector2(-32,176)); Surface(dock,theme.panel);
        string[] titles={"Kiếm Lô","Luyện Thể","Luyện Đan","Pháp Khí","Trận Pháp"};
        for(int i=0;i<5;i++)
        {
            var tab=dock.Find("Dock_"+i); Place(tab,new Vector2(i/5f,0),new Vector2((i+1)/5f,1),new Vector2(.5f,.5f),Vector2.zero,new Vector2(-8,-16));
            Surface(tab,theme.slot);
            var label=tab.Find("Label"); Place(label,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,14),new Vector2(-6,40));
            label.GetComponent<UnityEngine.UI.Text>().text=titles[i]; StyleText(label,30,TextAnchor.MiddleCenter,true);
            var icon=Art(tab,"JadeNavIcon",theme.navigationIcons[i]); Place(icon,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-10),new Vector2(102,102));
            var selected=Art(tab,"JadeSelected",theme.selectedSlot); Box(selected,0,0,0,0); selected.GetComponent<UnityEngine.UI.Image>().preserveAspect=false; selected.gameObject.SetActive(false);
            var drawer=mobile.Find("Drawer_"+i); Place(drawer,Vector2.zero,new Vector2(1,.55f),new Vector2(.5f,0),Vector2.zero,new Vector2(-32,0)); Surface(drawer,theme.window);
            var opaque=New(drawer,"JadeOpaqueSurface",typeof(UnityEngine.UI.Image));Box(opaque,20,20,20,20);opaque.SetAsFirstSibling();opaque.GetComponent<UnityEngine.UI.Image>().color=new Color(.028f,.085f,.09f,.94f);opaque.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            Top(drawer.Find("Title"),36,34,780,68); StyleText(drawer.Find("Title"),42,TextAnchor.MiddleLeft,true);
            drawer.Find("Title").GetComponent<UnityEngine.UI.Text>().text=titles[i].ToUpperInvariant();
            Top(drawer.Find("CloseButton"),-16,16,132,132,1);
            foreach(var t in drawer.Find("CloseButton").GetComponentsInChildren<UnityEngine.UI.Text>(true)) { t.text="Đóng"; StyleText(t.transform,30,TextAnchor.MiddleCenter,true); }
            // Large invisible swipe hit region; a thin jade grip remains the visual affordance.
            Top(drawer.Find("SwipeHandle"),0,0,300,42,.5f); drawer.Find("SwipeHandle").GetComponent<UnityEngine.UI.Image>().color=Color.clear;
            var grip=New(drawer.Find("SwipeHandle"),"JadeGrip",typeof(UnityEngine.UI.Image)); Place(grip,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(116,7)); grip.GetComponent<UnityEngine.UI.Image>().color=JadeUITheme.Gold; grip.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var divider=Art(drawer,"JadeHeaderDivider",theme.divider); Top(divider,0,104,840,28,.5f);
        }
        var ctrl=mobile.GetComponent<MobileDrawerUIController>();
        Place(ctrl.dockToggleButton,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,178),new Vector2(320,68));
        if(ctrl.dockToggleLabelText!=null) StyleText(ctrl.dockToggleLabelText.transform,28,TextAnchor.MiddleCenter);
    }
    static void SwordFurnace()
    {
        var merge=mobile.Find("Drawer_0/MergePanel"); Box(merge,24,184,24,132);
        merge.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
        var equip=merge.Find("EquipPanel"); Place(equip,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-92),new Vector2(0,282));
        var grid=equip.GetComponent<UnityEngine.UI.GridLayoutGroup>(); grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount=5; grid.cellSize=new Vector2(180,132); grid.spacing=new Vector2(14,14); grid.childAlignment=TextAnchor.UpperCenter; grid.padding=new RectOffset(8,8,0,0);
        var equipLabel=Label(merge,"JadeEquipLabel","PHI KIẾM XUẤT CHIẾN",28); Top(equipLabel,12,0,700,32); equipLabel.GetComponent<UnityEngine.UI.Text>().color=JadeUITheme.Gold;
        var inventory=merge.Find("InventoryContainer"); Box(inventory,0,0,0,394);
        var pagination=inventory.Find("PaginationPanel"); Place(pagination,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),Vector2.zero,new Vector2(0,132));
        var layout=pagination.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>(); layout.spacing=8; layout.padding=new RectOffset(0,0,0,0); layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;
        foreach(Transform child in pagination)
        {
            var le=Get<UnityEngine.UI.LayoutElement>(child.gameObject); le.minWidth=child.GetComponent<UnityEngine.UI.Button>()!=null?132:72; le.preferredWidth=child.name=="BtnSpawnSword"||child.name=="btn_QuickMerge"?210:le.minWidth; le.flexibleWidth=0;
            foreach(var text in child.GetComponentsInChildren<UnityEngine.UI.Text>(true)) StyleText(text.transform,28,TextAnchor.MiddleCenter,true);
        }
        var scroll=inventory.Find("ScrollView"); Box(scroll,0,148,0,0);
        foreach(var page in inventory.GetComponentsInChildren<UnityEngine.UI.GridLayoutGroup>(true))
        {
            page.cellSize=new Vector2(174,160); page.spacing=new Vector2(18,18); page.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;page.constraintCount=5;page.childAlignment=TextAnchor.UpperCenter;
        }
    }
    static void Alchemy()
    {
        var drawer=mobile.Find("Drawer_2");
        var scroll=New(drawer,"JadeAlchemyScroll",typeof(UnityEngine.UI.ScrollRect)); Box(scroll,24,192,24,136);
        var viewport=New(scroll,"Viewport",typeof(UnityEngine.UI.RectMask2D)); Box(viewport,0,0,0,0);
        var content=New(viewport,"Content",typeof(UnityEngine.UI.VerticalLayoutGroup),typeof(UnityEngine.UI.ContentSizeFitter));
        Place(content,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),Vector2.zero,new Vector2(0,0));
        var group=content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>(); group.padding=new RectOffset(8,8,8,24);group.spacing=20;group.childControlWidth=true;group.childControlHeight=true;group.childForceExpandHeight=false;group.childForceExpandWidth=true;
        content.GetComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        var sr=scroll.GetComponent<UnityEngine.UI.ScrollRect>();sr.viewport=RT(viewport);sr.content=RT(content);sr.horizontal=false;sr.vertical=true;sr.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;sr.scrollSensitivity=36;
        string[] names={"HerbVault","FurnaceCard","RecipeContainer","PillVault","FooterGuide"};float[] heights={184,320,420,300,96};
        for(int i=0;i<names.Length;i++)
        {
            var child=drawer.Find(names[i])??content.Find(names[i]);if(child==null)continue;
            Undo.SetTransformParent(child,content,"Scrollable alchemy cards");child.localScale=Vector3.one;var le=Get<UnityEngine.UI.LayoutElement>(child.gameObject);le.preferredHeight=heights[i];le.minHeight=heights[i];
            foreach(var t in child.GetComponentsInChildren<UnityEngine.UI.Text>(true))StyleText(t.transform,t.name.Contains("Title")?34:30, t.alignment,t.fontStyle==FontStyle.Bold);
        }
        var furnace=content.Find("FurnaceCard");
        Top(furnace.Find("TxtFurnaceStatus"),28,20,870,56);Top(furnace.Find("TxtFurnaceTimer"),28,78,870,48);
        Place(furnace.Find("BtnFinishWithDust"),new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-24,28),new Vector2(470,132));
        Place(furnace.Find("RatesBar"),new Vector2(0,0),new Vector2(.42f,0),Vector2.zero,new Vector2(28,64),new Vector2(-28,24));
        foreach(Transform card in content.Find("RecipeContainer"))
        {
            Top(card.Find("Title"),20,16,410,56);Top(card.Find("Desc"),20,78,410,72);Top(card.Find("Cost"),20,160,410,84);
            foreach(var b in card.GetComponentsInChildren<UnityEngine.UI.Button>(true))Place(b.transform,new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,20),new Vector2(-32,132));
        }
        var pill=content.Find("PillVault");Top(pill.Find("VaultTitle"),24,12,850,50);
        Top(pill.Find("TxtTuViSummary"),24,72,580,88);Top(pill.Find("TxtBTSummary"),24,170,580,100);
        Place(pill.Find("BtnDrinkTuVi"),new Vector2(1,.5f),new Vector2(1,.5f),new Vector2(1,.5f),new Vector2(-24,-12),new Vector2(300,156));
    }
    static void Lists()
    {
        for(int i=1;i<5;i++)
        {
            if(i==2)continue; var drawer=mobile.Find("Drawer_"+i);
            var resources=drawer.Find("ResourceBar"); if(resources!=null)Place(resources,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-134),new Vector2(-56,72));
            foreach(var sr in drawer.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
            {
                if(sr.name=="TreeScrollView"||sr.name=="ArtifactScrollView"||sr.name=="FormationScrollView")
                {Box(sr.transform,28,250,28,i==4?278:222);sr.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;sr.scrollSensitivity=40;}
            }
            var guide=drawer.Find(i==3?"ArtifactGuide":"FormationGuide");if(guide!=null)Place(guide,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,186),new Vector2(-56,64));
            if(i==4){var active=drawer.Find("ActiveSpiritBanner");if(active!=null)Place(active,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-212),new Vector2(-56,60));}
            foreach(var t in drawer.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                if(t.transform.parent==drawer && (t.name=="Title"||t.name=="SubTitle"))continue;
                if(t.fontSize<34)StyleText(t.transform,34,t.alignment,t.fontStyle==FontStyle.Bold);
            }
        }
    }
    static void Menus()
    {
        var menu=canvas.Find("MainMenuManager");
        const string backdropPath="Assets/Art/UI/Generated/ImmortalRealmPortrait.png";
        AssetDatabase.ImportAsset(backdropPath,ImportAssetOptions.ForceSynchronousImport);
        var backdropImporter=(TextureImporter)AssetImporter.GetAtPath(backdropPath);
        backdropImporter.textureType=TextureImporterType.Sprite;backdropImporter.spriteImportMode=SpriteImportMode.Single;backdropImporter.mipmapEnabled=false;backdropImporter.maxTextureSize=2048;backdropImporter.SaveAndReimport();
        var backdrop=Art(menu,"JadeBackground",AssetDatabase.LoadAssetAtPath<Sprite>(backdropPath));
        Box(backdrop,0,0,0,0);backdrop.SetAsFirstSibling();
        var aspect=Get<UnityEngine.UI.AspectRatioFitter>(backdrop.gameObject);aspect.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
        aspect.aspectRatio=(float)backdrop.GetComponent<UnityEngine.UI.Image>().sprite.texture.width/backdrop.GetComponent<UnityEngine.UI.Image>().sprite.texture.height;
        foreach(string name in new[]{"MainPanel","CharacterSelectPanel","CharacterCreatePanel","SettingsPanel"})
        {
            var panel=menu.Find(name);var img=panel.GetComponent<UnityEngine.UI.Image>();if(name!="MainPanel"){img.sprite=null;img.color=JadeUITheme.Surface;}
            Get<SafeAreaFitter>(panel.gameObject);
        }
        var main=menu.Find("MainPanel");
        main.GetComponent<UnityEngine.UI.Image>().sprite=null;main.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
        var shade=New(main,"JadeAtmosphere",typeof(UnityEngine.UI.Image));Box(shade,0,0,0,0);shade.GetComponent<UnityEngine.UI.Image>().color=new Color(.015f,.075f,.075f,.48f);shade.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;shade.SetAsFirstSibling();
        var title=main.Find("TitleText");Place(title,new Vector2(.5f,.73f),new Vector2(.5f,.73f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(960,220));
        title.GetComponent<UnityEngine.UI.Text>().text="KHÍ VẬN\nCHI TỬ";StyleText(title,92,TextAnchor.MiddleCenter,true);title.GetComponent<UnityEngine.UI.Text>().color=JadeUITheme.Ivory;
        var subtitle=Label(main,"JadeSubtitle","MỘT NIỆM TU TIÊN · VẠN KIẾM QUY TÔNG",30);Place(subtitle,new Vector2(.5f,.63f),new Vector2(.5f,.63f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(940,64));StyleText(subtitle,30,TextAnchor.MiddleCenter);subtitle.GetComponent<UnityEngine.UI.Text>().color=JadeUITheme.Gold;
        var divider=Art(main,"JadeTitleDivider",theme.divider);Place(divider,new Vector2(.5f,.59f),new Vector2(.5f,.59f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(680,60));
        string[] buttons={"PlayButton","SettingsButton","QuitButton"};
        for(int i=0;i<buttons.Length;i++)
        {
            var b=main.Find(buttons[i]);Place(b,new Vector2(.5f,.12f),new Vector2(.5f,.12f),new Vector2(.5f,0),new Vector2(0,(2-i)*164),new Vector2(700,140));Surface(b,i==0?theme.primaryButton:theme.secondaryButton);
            foreach(var t in b.GetComponentsInChildren<UnityEngine.UI.Text>(true))StyleText(t.transform,40,TextAnchor.MiddleCenter,true);
        }
        var select=menu.Find("CharacterSelectPanel");Top(select.Find("SelectTitle"),0,90,940,100,.5f);StyleText(select.Find("SelectTitle"),52,TextAnchor.MiddleCenter,true);
        var slots=select.Find("SlotContainer");var oldGroup=slots.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();if(oldGroup!=null)oldGroup.enabled=false;
        Place(slots,new Vector2(0,.27f),new Vector2(1,.80f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(-96,0));
        int slotIndex=0;
        foreach(Transform slot in slots)
        {
            Place(slot,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-slotIndex++*296),new Vector2(0,272));
            var le=Get<UnityEngine.UI.LayoutElement>(slot.gameObject);le.preferredHeight=272;le.flexibleHeight=0;Surface(slot,theme.panel);
            var data=slot.Find("DataPanel");Top(data.Find("ModelImage"),24,44,160,160);Top(data.Find("NameText"),210,28,530,64);Top(data.Find("CultivationText"),210,96,530,60);Top(data.Find("TimeText"),210,162,530,60);Top(data.Find("DeleteBtn"),-18,70,132,132,1);
            foreach(var t in slot.GetComponentsInChildren<UnityEngine.UI.Text>(true))StyleText(t.transform,34,t.alignment,t.fontStyle==FontStyle.Bold);
        }
        Place(select.Find("BackButton"),new Vector2(.5f,.08f),new Vector2(.5f,.08f),new Vector2(.5f,0),Vector2.zero,new Vector2(620,132));
        var create=menu.Find("CharacterCreatePanel");Top(create.Find("CreateTitle"),0,240,940,100,.5f);StyleText(create.Find("CreateTitle"),52,TextAnchor.MiddleCenter,true);
        Place(create.Find("NameInput"),new Vector2(.5f,.55f),new Vector2(.5f,.55f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(860,150));Surface(create.Find("NameInput"),theme.panel);
        foreach(var t in create.Find("NameInput").GetComponentsInChildren<UnityEngine.UI.Text>(true))StyleText(t.transform,40,t.alignment);
        Place(create.Find("ConfirmButton"),new Vector2(.5f,.22f),new Vector2(.5f,.22f),new Vector2(.5f,0),new Vector2(0,168),new Vector2(800,140));Place(create.Find("CancelButton"),new Vector2(.5f,.22f),new Vector2(.5f,.22f),new Vector2(.5f,0),Vector2.zero,new Vector2(800,140));
        var settings=menu.Find("SettingsPanel");var heading=Label(settings,"JadeHeading","CÀI ĐẶT ÂM THANH",48);Top(heading,0,200,920,100,.5f);StyleText(heading,48,TextAnchor.MiddleCenter,true);
        string[] sliderNames={"BGMSlider","SFXSlider"};
        for(int i=0;i<2;i++)
        {
            var slider=settings.Find(sliderNames[i]);Place(slider,new Vector2(.5f,.57f),new Vector2(.5f,.57f),new Vector2(.5f,.5f),new Vector2(0,-i*240),new Vector2(800,132));
            var label=slider.Find("Label");Top(label,0,-90,800,72);StyleText(label,38);
            var handle=slider.Find("Handle Slide Area/Handle");if(handle!=null)handle.GetComponent<RectTransform>().sizeDelta=new Vector2(72,132);
        }
        Place(settings.Find("CloseButton"),new Vector2(.5f,.1f),new Vector2(.5f,.1f),new Vector2(.5f,0),Vector2.zero,new Vector2(700,140));
        foreach(var t in menu.GetComponentsInChildren<UnityEngine.UI.Text>(true))if(t.fontSize<28)StyleText(t.transform,32,t.alignment,t.fontStyle==FontStyle.Bold);
    }
    static void Dialogs()
    {
        foreach(var r in canvas.GetComponentsInChildren<RectTransform>(true))
        {
            if(r.name=="ModalBox"||r.name=="PauseBox"||r.name=="Box")
            {
                var fit=Get<JadeModalFit>(r.gameObject);
                fit.preferredScale=r.rect.width<=700?Mathf.Min(1.8f,984/r.rect.width):1f;
                Surface(r,theme.window);
                fit.Fit();
            }
        }
    }
    static void ImproveCards()
    {
        var body=mobile.Find("Drawer_1").GetComponent<BodyTemperingPanelUI>();
        if(body!=null&&body.treeContent!=null)
        {
            body.treeContent.sizeDelta=new Vector2(body.treeContent.sizeDelta.x,1280);
            for(int i=0;i<6;i++)
            {
                var card=body.nodeCardTransforms[i];if(card==null)continue;
                int col=i<3?0:1;int row=i<3?(i==1?2:i==2?1:0):i-3;
                Place(card,new Vector2(col*.51f,1),new Vector2(col*.51f+.49f,1),new Vector2(.5f,1),new Vector2(0,-66-row*404),new Vector2(-8,360));
                Surface(card,theme.panel);
                if(body.nodeOutlines[i]!=null){JadeUITheme.SetSurface(body.nodeOutlines[i],theme.selectedSlot);body.nodeOutlines[i].raycastTarget=false;}
                Top(body.nodeNameTexts[i].transform,22,14,290,54);Top(body.nodeLevelTexts[i].transform,-22,14,120,54,1);
                Place(body.nodeEffectTexts[i].transform,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-78),new Vector2(-44,108));
                var button=body.nodeQuickUpgradeButtons[i];if(button!=null)Place(button.transform,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,18),new Vector2(-32,132));
                if(body.nodeLockRequirementTexts[i]!=null)StyleText(body.nodeLockRequirementTexts[i].transform,30,TextAnchor.MiddleCenter);
            }
        }
        var artifacts=mobile.Find("Drawer_3").GetComponent<ArtifactPanelUI>();
        if(artifacts!=null)
        {
            for(int i=0;i<artifacts.upgradeButtons.Length;i++)
            {
                var button=artifacts.upgradeButtons[i];if(button==null)continue;var card=button.transform.parent;
                Place(card,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-i*344),new Vector2(-8,320));Surface(card,theme.panel);
                RT(card.parent).sizeDelta=new Vector2(RT(card.parent).sizeDelta.x,artifacts.upgradeButtons.Length*344);
                Top(artifacts.artifactLabels[i].transform,26,16,860,56);Top(artifacts.artifactStatsTexts[i].transform,26,78,860,80);
                Place(artifacts.artifactCostTexts[i].transform,Vector2.zero,new Vector2(.6f,0),Vector2.zero,new Vector2(26,18),new Vector2(-26,132));
                Place(button.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-22,18),new Vector2(330,132));
            }
        }
        var formations=mobile.Find("Drawer_4").GetComponent<FormationPanelUI>();
        if(formations!=null)
        {
            for(int i=0;i<formations.upgradeButtons.Length;i++)
            {
                var button=formations.upgradeButtons[i];if(button==null)continue;var card=button.transform.parent;
                Place(card,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-i*464),new Vector2(-8,440));Surface(card,theme.panel);
                RT(card.parent).sizeDelta=new Vector2(RT(card.parent).sizeDelta.x,formations.upgradeButtons.Length*464);
                Top(formations.formationLabels[i].transform,26,14,870,56);Top(formations.statusBadges[i].transform,26,72,870,38);
                Top(formations.formationDescTexts[i].transform,26,114,870,86);Top(formations.formationCostTexts[i].transform,26,206,870,48);
                Place(button.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-22,20),new Vector2(430,132));
                Place(formations.selectButtons[i].transform,Vector2.zero,Vector2.zero,Vector2.zero,new Vector2(22,20),new Vector2(430,132));
            }
        }
        // The existing storage shortcut is retained; give it a proper, readable header lane.
        var merge=mobile.Find("Drawer_0/MergePanel");
        foreach(var t in merge.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            if(t.text.Contains("LÒ PHÂN GIẢI"))
            {
                var shortcut=t.GetComponentInParent<UnityEngine.UI.Button>();
                if(shortcut!=null&&shortcut.transform.IsChildOf(merge)){Top(shortcut.transform,-8,0,430,88,1);Box(t.transform,12,4,12,4);}
                StyleText(t.transform,28,TextAnchor.MiddleCenter,true);
            }
        var equipLabel=merge.Find("JadeEquipLabel");Top(equipLabel,12,14,490,64);StyleText(equipLabel,28,TextAnchor.MiddleLeft,true);
    }
    static void CreateReusablePrefabs()
    {
        const string folder="Assets/Art/UI/Generated/Prefabs"; Directory.CreateDirectory(folder);
        var sprites=new[]{theme.panel,theme.window,theme.slot,theme.primaryButton,theme.secondaryButton,theme.resourceBar};
        foreach(var sprite in sprites)
        {
            var go=new GameObject("Jade"+sprite.name,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            go.GetComponent<RectTransform>().sizeDelta=new Vector2(600,sprite.name.Contains("Button")?132:400);
            JadeUITheme.SetSurface(go.GetComponent<UnityEngine.UI.Image>(),sprite);go.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            PrefabUtility.SaveAsPrefabAsset(go,folder+"/Jade"+sprite.name+".prefab");UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
#endif

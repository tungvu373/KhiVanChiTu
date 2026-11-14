#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static partial class MobileUIPhase09Checks
{
    static Rect PixelRect(RectTransform rt)
    {
        var corners=new Vector3[4];rt.GetWorldCorners(corners);
        var points=corners.Select(c=>RectTransformUtility.WorldToScreenPoint(null,c)).ToArray();
        return Rect.MinMaxRect(points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y));
    }
    static bool Contains(Rect outer,Rect inner,float tolerance=1.5f)=>inner.xMin>=outer.xMin-tolerance&&inner.yMin>=outer.yMin-tolerance&&inner.xMax<=outer.xMax+tolerance&&inner.yMax<=outer.yMax+tolerance;
    static Rect ClipRect(Component c)
    {
        var rect=new Rect(0,0,Screen.width,Screen.height);
        foreach(var mask in c.GetComponentsInParent<RectMask2D>())rect=Intersection(rect,PixelRect((RectTransform)mask.transform));
        foreach(var mask in c.GetComponentsInParent<Mask>())rect=Intersection(rect,PixelRect((RectTransform)mask.transform));
        return rect;
    }
    static Rect Intersection(Rect a,Rect b)=>Rect.MinMaxRect(Mathf.Max(a.xMin,b.xMin),Mathf.Max(a.yMin,b.yMin),Mathf.Min(a.xMax,b.xMax),Mathf.Min(a.yMax,b.yMax));
    static string PathOf(Component c)=>AnimationUtility.CalculateTransformPath(c.transform,null);
    static object RectData(Rect r)=>new{x=r.x,y=r.y,width=r.width,height=r.height};
    static bool Visible(Component c)=>c.gameObject.activeInHierarchy&&c.GetComponentsInParent<CanvasGroup>().All(g=>g.alpha>.01f);
    static JObject Geometry(string state,Transform[] roots)
    {
        var buttons=new JArray();var texts=new JArray();var slices=new JArray();
        foreach(var b in roots.SelectMany(t=>t.GetComponentsInChildren<Button>()).Distinct().Where(Visible))
        {
            var rect=PixelRect((RectTransform)b.transform);var clip=ClipRect(b);bool centerVisible=clip.Contains(rect.center);
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=rect.center},hits);
            var top=hits.Count==0?null:hits[0].gameObject.GetComponentInParent<Button>();
            buttons.Add(JObject.FromObject(new{path=PathOf(b),rect=RectData(rect),centerVisible,fullyInClip=Contains(clip,rect),safe=Contains(Screen.safeArea,rect),logicalWidth=rect.width*360/Screen.width,logicalHeight=rect.height*360/Screen.width,interactable=b.IsInteractable(),hitMatches=top==b,topHit=hits.Count>0?PathOf(hits[0].gameObject.transform):null}));
        }
        foreach(var t in roots.SelectMany(r=>r.GetComponentsInChildren<Text>()).Distinct().Where(t=>t.enabled&&Visible(t)&&!string.IsNullOrWhiteSpace(t.text)))
        {
            var rect=PixelRect(t.rectTransform);var clip=ClipRect(t);if(!clip.Overlaps(rect))continue;
            texts.Add(JObject.FromObject(new{path=PathOf(t),text=t.text,rect=RectData(rect),fontSize=t.fontSize,bestFit=t.resizeTextForBestFit,effectiveFont=t.resizeTextForBestFit?t.cachedTextGenerator.fontSizeUsedForBestFit:t.fontSize,logicalFont=t.fontSize*t.transform.lossyScale.y*360/Screen.width,preferredWidth=t.preferredWidth,preferredHeight=t.preferredHeight,localWidth=t.rectTransform.rect.width,localHeight=t.rectTransform.rect.height,wrap=t.horizontalOverflow.ToString(),overflow=t.verticalOverflow.ToString()}));
        }
        foreach(var image in roots.SelectMany(r=>r.GetComponentsInChildren<Image>()).Distinct().Where(i=>i.enabled&&Visible(i)&&i.type==Image.Type.Sliced&&i.sprite!=null))
        {
            slices.Add(JObject.FromObject(new{path=PathOf(image),sprite=image.sprite.name,border=image.sprite.border.ToString(),ppu=image.sprite.pixelsPerUnit,multiplier=image.pixelsPerUnitMultiplier,rect=RectData(PixelRect(image.rectTransform))}));
        }
        return JObject.FromObject(new{state,frame=Time.frameCount,width=Screen.width,height=Screen.height,safeArea=RectData(Screen.safeArea),buttons,texts,slices,source="Actual Play Mode geometry and EventSystem.RaycastAll; center hit does not prove OS touch delivery"});
    }
    static IEnumerable CaptureState(string profile,string state,params Transform[] roots)
    {
        yield return .38;Canvas.ForceUpdateCanvases();RequireIsolation();
        roots=roots.Where(r=>r!=null).ToArray();
        var geometry=Geometry(state,roots);File.WriteAllText(Evidence+"/"+profile+"-"+state+".json",geometry.ToString());
        Case(profile+"/"+state,"VISUAL","UI/PortraitHudController.cs","ApplyLayout",()=>{
            Expect(roots.Length>0&&roots.All(r=>r.gameObject.activeInHierarchy),"Requested screen root unavailable");
            Expect(EventSystem.current!=null,"Missing EventSystem");
            Expect(Screen.width>0&&Screen.height>0,"Invalid viewport");
            Note("geometry",profile+"-"+state+".json");Note("screenshot",profile+"-"+state+".png");
        });
        Case(profile+"/"+state+"-input-geometry","VISUAL","Assets/UI/Reconstruction/Runtime/MobileUIResponsiveRepairs.cs","ApplyTree",()=>{
            foreach(JObject button in (JArray)geometry["buttons"])
            {
                string path=(string)button["path"],name=path.Split('/').Last();
                if(!(bool)button["centerVisible"]||!(bool)button["fullyInClip"]||!(bool)button["interactable"]||name.Contains("Overlay")||name.Contains("Backdrop")||name=="MenuDimmer")continue;
                if(state=="pause-confirm"&&!path.Contains("ConfirmationPopup"))continue;
                // Moving arena actors may overlap; verify the topmost actor in the functional suite.
                if(name.StartsWith("DemonBadge_")||name.StartsWith("ThunderOrb_"))continue;
                Expect((bool)button["hitMatches"],"Button center intercepted: "+path+" by "+button["topHit"]);
                Expect((float)button["logicalWidth"]>=43.5f&&(float)button["logicalHeight"]>=43.5f,"Touch target below 44 logical pixels: "+path);
            }
            if(state=="character")
            {
                var ui=Find<CharacterDetailModalUI>();
                Expect(ui.txtName.cachedTextGenerator.vertexCount>4,"Character name has no rendered glyphs");
                Expect(ui.txtEquipSwords.rectTransform.rect.height>=ui.txtEquipSwords.preferredHeight,"Equipment value text clips in its layout row");
            }
            if(state=="body-detail")
            {var ui=GameObject.Find("Canvas").transform.Find("MobileUIRoot/Drawer_1").GetComponent<BodyTemperingPanelUI>();Expect(!PixelRect(ui.txtModalCurrent.rectTransform).Overlaps(PixelRect(ui.txtModalNext.rectTransform)),"Current and next values overlap");}
            if(state.StartsWith("storage"))foreach(JObject text in (JArray)geometry["texts"])
                if(((string)text["path"]).Contains("/Ribbon/")||((string)text["path"]).Contains("/CountBadge/"))Expect((float)text["localHeight"]>=(float)text["preferredHeight"],"Inventory badge glyph clips at this Canvas scale");
        });
        ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath(Evidence+"/"+profile+"-"+state+".png"));yield return .12;
    }
    static IEnumerable Bootstrap()
    {
        yield return 1.0;RequireIsolation();int start=Time.frameCount;double deadline=EditorApplication.timeSinceStartup+8;
        while(Time.frameCount<start+5&&EditorApplication.timeSinceStartup<deadline)yield return .02;
        Expect(Time.frameCount>=start+5,"Player loop frozen");
        DataManager.Instance.gameSaveData=new GameSaveData();DataManager.Instance.SaveData();
    }
    static IEnumerable EnterGameplay()
    {
        var menu=MainMenuManager.Instance;menu.OpenCharacterCreation(0);menu.nameInput.text="Nguyễn Tiêu Vân Thiên Mệnh";Click(menu.confirmCreateButton);yield return .7;
        RequireIsolation();Time.timeScale=0;Highest();Rich();EconomyManager.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,linhThach=1000000000,coDuyen=10000});
        var save=DataManager.Instance.gameSaveData.slots[0];save.swordStyleUnlocked=true;
        PlayerStatsService.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,bodyStatLevels=Enumerable.Repeat(20,6).ToArray()});
        MergeManager.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,swords=Enumerable.Range(0,32).Select(i=>new SwordSaveEntry{level=1+i%8,isEquipped=i<10,equipSlotIndex=i<10?i:-1}).ToArray()});yield return .15;
    }
    static IEnumerator ResponsiveSuite()
    {
        foreach(var step in Bootstrap())yield return step;
        var menu=MainMenuManager.Instance;
        var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(1080,2400)};
        foreach(var size in sizes)
        {
            string p=size.x+"x"+size.y;SetResolution(size.x,size.y);yield return .35;
            foreach(var step in CaptureState(p,"menu",menu.mainPanel.transform))yield return step;
            Click(menu.settingsButton);foreach(var step in CaptureState(p,"main-settings",menu.settingsPanel.transform))yield return step;Click(menu.closeSettingsButton);
            menu.OpenCharacterSelection();foreach(var step in CaptureState(p,"roster",menu.characterSelectPanel.transform))yield return step;
            menu.OpenCharacterCreation(0);menu.nameInput.text="Nguyễn Tiêu Vân Thiên Mệnh";foreach(var step in CaptureState(p,"create",menu.characterCreatePanel.transform))yield return step;Click(menu.cancelCreateButton);Click(menu.backFromSelectButton);
        }
        foreach(var step in EnterGameplay())yield return step;
        foreach(var size in sizes)
        {
            string p=size.x+"x"+size.y;SetResolution(size.x,size.y);yield return .4;
            Case(p+"/exact-viewport","HUD","UI/SafeAreaFitter.cs","Refresh",()=>{Expect(Screen.width==size.x&&Screen.height==size.y,"Game View did not apply requested resolution");Expect(Contains(new Rect(0,0,size.x,size.y),Screen.safeArea),"Safe area invalid");});
            foreach(var step in GameplayScreens(p))yield return step;
        }
        RuntimeBindingsSnapshot();
    }
    static IEnumerable GameplayScreens(string p)
    {
        var root=GameObject.Find("Canvas").transform.Find("MobileUIRoot");var hud=Find<PortraitHudController>();var drawer=MobileDrawerUIController.Instance;
        foreach(var step in CaptureState(p,"hud",root))yield return step;
        Case(p+"/arena-unblocked","HUD","UI/PortraitHudController.cs","ApplyLayout",()=>{
            Expect(hud.CentralClearArea.height>0,"No clear gameplay area");
            var area=PixelRect((RectTransform)hud.transform);var point=new Vector2(area.center.x,area.yMin+hud.CentralClearArea.center.y*area.width/1080);
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);Expect(hits.Count==0,"Clear arena intercepts gameplay touch");Note("arenaHeightReference",hud.CentralClearArea.height);
        });
        for(int i=0;i<5;i++)
        {
            Click(drawer.dockButtons[i]);yield return .28;
            foreach(var step in CaptureState(p,"drawer-"+i,drawer.drawerPanels[i]))yield return step;
            foreach(var sr in drawer.drawerPanels[i].GetComponentsInChildren<ScrollRect>().Where(s=>s.isActiveAndEnabled&&s.vertical&&s.content!=null&&s.viewport!=null&&s.content.rect.height>s.viewport.rect.height+1))
            {
                sr.verticalNormalizedPosition=0;yield return .1;
                Case(p+"/scroll-"+i+"-"+sr.name,"SCROLL","UI/MobileDrawerUIController.cs","OpenDrawer",()=>{Expect(sr.viewport.GetComponent<Mask>()!=null||sr.viewport.GetComponent<RectMask2D>()!=null,"Scroll viewport missing clipping");Expect(sr.verticalNormalizedPosition<.01,"Scroll bottom unreachable");});
                foreach(var step in CaptureState(p,"drawer-"+i+"-bottom-"+sr.name,drawer.drawerPanels[i]))yield return step;sr.verticalNormalizedPosition=1;
            }
            if(i==1){var body=root.Find("Drawer_1").GetComponent<BodyTemperingPanelUI>();body.OpenCenterModal(5);foreach(var step in CaptureState(p,"body-detail",body.centerModalBox))yield return step;body.CloseCenterModal();}
            Expect(MobileUINavigationManager.Instance.HandleBackNavigation(),"Drawer Back failed");yield return .28;
        }
        hud.OpenSecondary();foreach(var step in CaptureState(p,"utility",hud.secondaryOverlay.transform))yield return step;hud.CloseSecondary();
        Click(hud.walletButton);foreach(var step in CaptureState(p,"wallet",hud.secondaryOverlay.transform))yield return step;hud.CloseSecondary();
        Find<CharacterDetailModalUI>().OpenPanel();var character=Find<CharacterDetailModalUI>();foreach(var step in CaptureState(p,"character",character.modalBox))yield return step;character.ClosePanel();yield return .2;
        StorageModalUI.Open();var storage=StorageModalUI.Instance;foreach(var step in CaptureState(p,"storage-summary",storage.modalBox))yield return step;
        for(int i=1;i<5;i++){Click(storage.tabButtons[i]);foreach(var step in CaptureState(p,"storage-"+i,storage.modalBox))yield return step;}storage.Close();yield return .2;
        DismantleConfirmModalUI.OpenConfirm("Huyền Thiên Kiếm · Bậc 8","Phân giải vật phẩm QA",new DismantleReward(),()=>{});var dismantle=Find<DismantleConfirmModalUI>();foreach(var step in CaptureState(p,"dismantle",dismantle.transform))yield return step;dismantle.Close();yield return .2;
        DaoAnModalUI.Instance.Open();foreach(var step in CaptureState(p,"daoan",((GameObject)Field(DaoAnModalUI.Instance,"modalOverlay")).transform))yield return step;DaoAnModalUI.Instance.Close();
        AchievementModalUI.Open();foreach(var step in CaptureState(p,"achievements",AchievementModalUI.Instance.modalBox))yield return step;AchievementModalUI.Close();yield return .2;
        Find<MilestoneModalUI>().Open();foreach(var step in CaptureState(p,"milestones",MilestoneModalUI.Instance.transform))yield return step;MilestoneModalUI.Instance.Close();
        GameplayChoicesService.Instance.OpenStyles();yield return .1;foreach(var step in CaptureState(p,"style",((GameObject)Field(GameplayChoicesService.Instance,"panel")).transform))yield return step;GameplayChoicesService.Instance.SelectStyle(0);
        GameplayChoicesService.Instance.OfferEncounter();yield return .1;foreach(var step in CaptureState(p,"encounter",((GameObject)Field(GameplayChoicesService.Instance,"panel")).transform))yield return step;GameplayChoicesService.Instance.ResolveEncounter(false);
        SecretRealmModalUI.Instance.Open();foreach(var step in CaptureState(p,"secret-hub",GameObject.Find("Canvas").transform.Find("SecretRealmModal")))yield return step;SecretRealmModalUI.Instance.Close();
        foreach(SecretRealmType type in Enum.GetValues(typeof(SecretRealmType)))
        {
            Call(SecretRealmPlayController.Instance,"LaunchMiniGame",type);foreach(var step in CaptureState(p,"trial-"+type,SecretRealmPlayController.Instance.ActiveNavigationLayer))yield return step;SecretRealmPlayController.Instance.CloseAll();SecretRealmModalUI.Instance.Close();yield return .08;
        }
        SecretRealmPlayController.Instance.ShowResultModal(new SecretRealmService.RewardResult{linhThach=123456,ore=99});foreach(var step in CaptureState(p,"secret-result",SecretRealmPlayController.Instance.ActiveNavigationLayer))yield return step;SecretRealmPlayController.Instance.CloseAll();SecretRealmModalUI.Instance.Close();
        var reward=new OfflineEarningsManager.OfflineRewardData{hasReward=true,linhThach=999999,tuVi=123456,offlineSeconds=86400,ore=100,linhChi=100};typeof(OfflineEarningsManager).GetProperty("PendingReward",All).SetValue(OfflineEarningsManager.Instance,reward);OfflineRewardPopupUI.Show(reward);foreach(var step in CaptureState(p,"offline",OfflineRewardPopupUI.Instance.transform))yield return step;Click(OfflineRewardPopupUI.Instance.claimNormalButton);yield return .4;
        var cm=CultivationManager.Instance;cm.SetStageAndTuViForEditor(14,cm.allStages[14].requiredTuVi);TribulationModalUI.Open();foreach(var step in CaptureState(p,"tribulation",TribulationModalUI.Instance.transform))yield return step;TribulationModalUI.Close();yield return .2;Highest();
        PauseMenuManager.Instance.OpenPauseMenu();foreach(var step in CaptureState(p,"pause",PauseMenuManager.Instance.transform))yield return step;Click(PauseMenuManager.Instance.returnMainMenuButton);foreach(var step in CaptureState(p,"pause-confirm",PauseMenuManager.Instance.transform))yield return step;Click(PauseMenuManager.Instance.btnPopupCancel);PauseMenuManager.Instance.ResumeGame();
        drawer.ToggleDockCollapse();yield return .35;foreach(var step in CaptureState(p,"hud-focus",root))yield return step;drawer.ToggleDockCollapse();yield return .35;
    }
    static IEnumerator DevicesSuite()
    {
        foreach(var step in Bootstrap())yield return step;foreach(var step in EnterGameplay())yield return step;
        var type=Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor.DeviceSimulatorModule",true);
        var existing=Resources.FindObjectsOfTypeAll(type).Cast<EditorWindow>().FirstOrDefault();bool created=existing==null;var window=existing!=null?existing:EditorWindow.GetWindow(type);yield return .3;
        var main=type.GetField("m_Main",All).GetValue(window);var index=main.GetType().GetProperty("deviceIndex",All);int previous=(int)index.GetValue(main);var stateField=type.GetField("m_State",All);object previousState=stateField.GetValue(window);
        var devices=((IEnumerable)main.GetType().GetProperty("devices",All).GetValue(main)).Cast<object>().ToArray();bool retry=SessionState.GetString(Key+"Mode","")=="devices-retry";var retryIndices=rows.Where(r=>!(bool)r["passed"]&&((string)r["name"]).StartsWith("device-")).Select(r=>int.Parse(((string)r["name"]).Split('/')[0].Substring(7))).ToArray();var inventory=retry?JArray.Parse(File.ReadAllText(Evidence+"/device-inventory-runtime.json")):new JArray();var oldOrientation=Screen.orientation;var previousRepaintView=gameView;gameView=window;
        try
        {
            stateField.SetValue(window,Enum.Parse(stateField.FieldType,"Enabled"));main.GetType().GetMethod("Enable",All).Invoke(main,null);type.GetMethod("OnFocus",All).Invoke(window,null);window.Focus();Screen.orientation=ScreenOrientation.Portrait;
            for(int i=0;i<devices.Length;i++)
            {
                if(retry&&!retryIndices.Contains(i))continue;
                var info=devices[i].GetType().GetField("deviceInfo",All).GetValue(devices[i]);string name=(string)info.GetType().GetField("friendlyName",All).GetValue(info);
                index.SetValue(main,i);Screen.orientation=ScreenOrientation.Portrait;
                stateField.SetValue(window,Enum.Parse(stateField.FieldType,"Enabled"));main.GetType().GetMethod("Enable",All).Invoke(main,null);type.GetMethod("OnFocus",All).Invoke(window,null);window.Focus();window.Repaint();
                yield return .25;
                Vector2 wanted=(Vector2)main.GetType().GetProperty("targetSize",All).GetValue(main);double deadline=EditorApplication.timeSinceStartup+3;
                while((Screen.width!=Mathf.RoundToInt(wanted.x)||Screen.height!=Mathf.RoundToInt(wanted.y)||!Contains(new Rect(0,0,Screen.width,Screen.height),Screen.safeArea))&&EditorApplication.timeSinceStartup<deadline)
                {type.GetMethod("OnFocus",All).Invoke(window,null);window.Focus();window.Repaint();yield return .1;}
                Canvas.ForceUpdateCanvases();
                string label="device-"+i;var hud=Geometry("hud",new[]{GameObject.Find("Canvas").transform.Find("MobileUIRoot")});
                StorageModalUI.Open();yield return .2;Canvas.ForceUpdateCanvases();var modal=Geometry("storage",new Transform[]{StorageModalUI.Instance.modalBox});
                Click(StorageModalUI.Instance.tabButtons[3]);yield return .1;Canvas.ForceUpdateCanvases();var materials=Geometry("storage-materials",new Transform[]{StorageModalUI.Instance.modalBox});
                var safe=Screen.safeArea;var entry=JObject.FromObject(new{index=i,name,width=Screen.width,height=Screen.height,expectedWidth=wanted.x,expectedHeight=wanted.y,safeArea=RectData(safe),hud,modal,materials});if(retry)inventory[i]=entry;else inventory.Add(entry);
                Case(label+"/safe-area","HUD","UI/SafeAreaFitter.cs","Refresh",()=>{
                    Expect(Screen.width==Mathf.RoundToInt(wanted.x)&&Screen.height==Mathf.RoundToInt(wanted.y),"NOT TESTED: Simulator resolution did not settle");
                    Expect(safe.width>0&&safe.height>0&&Contains(new Rect(0,0,Screen.width,Screen.height),safe),"Invalid Simulator safe area");
                    foreach(var b in new[]{StorageModalUI.Instance.btnClose,StorageModalUI.Instance.btnToggleView})Expect(Contains(safe,PixelRect((RectTransform)b.transform)),"Critical popup control outside device safe area");
                    foreach(JObject text in ((JArray)modal["texts"]).Concat((JArray)materials["texts"]))
                        if(((string)text["path"]).Contains("/Ribbon/")||((string)text["path"]).Contains("/CountBadge/"))Expect((float)text["localHeight"]>=(float)text["preferredHeight"],"Inventory badge glyph clips at device Canvas scale");
                    Note("device",name);Note("safeArea",RectData(safe));
                });
                if(name.Contains("iPhone 12")||name.Contains("Pixel 5")||name.Contains("Redmi Note 7")||name.Contains("Galaxy S10")||name.Contains("iPad"))
                {ScreenCapture.CaptureScreenshot(System.IO.Path.GetFullPath(Evidence+"/"+label+"-storage.png"));yield return .1;}
                StorageModalUI.Instance.Close();yield return .16;
                File.WriteAllText(Evidence+"/device-inventory-runtime.json",inventory.ToString());
            }
        }
        finally
        {
            Screen.orientation=oldOrientation;
            gameView=previousRepaintView;
            if(created)window.Close();else{index.SetValue(main,previous);stateField.SetValue(window,previousState);main.GetType().GetMethod(previousState.ToString()=="Enabled"?"Enable":"Disable",All).Invoke(main,null);}
        }
        yield return .2;
    }
    static IEnumerator BadgeSuite()
    {
        foreach(var step in Bootstrap())yield return step;foreach(var step in EnterGameplay())yield return step;
        foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(1080,2400)})
        {
            SetResolution(size.x,size.y);yield return .3;StorageModalUI.Open((StorageCategory)3);
            foreach(var step in CaptureState(size.x+"x"+size.y,"storage-3",StorageModalUI.Instance.modalBox))yield return step;
            StorageModalUI.Instance.Close();yield return .2;
        }
    }
}
#endif

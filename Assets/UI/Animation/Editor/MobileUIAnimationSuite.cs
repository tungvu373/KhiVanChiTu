#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using Unity.Profiling;
using Newtonsoft.Json.Linq;

public static partial class MobileUIAnimationChecks
{
    static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).First();
    static IEnumerator FullSuite()
    {
        yield return 1.0;RequireIsolation();int frame=Time.frameCount;double timeout=EditorApplication.timeSinceStartup+8;
        while(Time.frameCount<frame+5&&EditorApplication.timeSinceStartup<timeout)yield return .02;
        Expect(Time.frameCount>=frame+5,"Frozen player loop");
        var menu=MainMenuManager.Instance;DataManager.Instance.gameSaveData=new GameSaveData();DataManager.Instance.SaveData();menu.OpenCharacterCreation(0);menu.nameInput.text="P08 Motion Fixture";Click(menu.confirmCreateButton);yield return .5;Time.timeScale=0;
        var director=Find<MobileUIAnimationDirector>();
        Check("scene-assets-and-unscaled-clock",()=>{
            Expect(DataManager.Instance.isSessionActive&&DataManager.Instance.SaveProfileId==Profile,"Wrong save owner");
            foreach(var p in new[]{director.modalOpen,director.modalClose,director.sheetOpen,director.sheetClose,director.buttonDown,director.buttonUp,director.rewardReveal,director.accent,director.progress})
            {Expect(p!=null&&p.previewClip!=null,"Missing animation asset");Expect(Mathf.Abs(p.previewClip.length-p.duration)<.002f,"Clip duration mismatch");Expect(p.Evaluate(0)==0&&p.Evaluate(1)==1,"Ease endpoints");float previous=-1;for(int i=0;i<=20;i++){float v=p.Evaluate(i/20f);Expect(v>=previous,"Ease overshoot/reversal");previous=v;}}
            Expect(Time.timeScale==0,"QA must verify paused UI");
        });
        foreach(var step in StressProfile(director))yield return step;
        SetResolution(390,844);yield return .4;
        var hud=Find<PortraitHudController>();var button=hud.menuButton;var label=button.GetComponentInChildren<Text>(true).rectTransform;Vector3 rest=label.localScale;Vector2 hitSize=((RectTransform)button.transform).rect.size;Vector3 hitPosition=button.transform.position;
        ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerDownHandler);yield return .09;
        Check("press-visual-only",()=>{Expect(label.localScale.x<rest.x&&label.localScale.x>=rest.x*.96f,"Press pose missing");Expect(((RectTransform)button.transform).rect.size==hitSize&&button.transform.position==hitPosition,"Touch target moved");});
        for(int i=0;i<20;i++){ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerUpHandler);}
        yield return .16;Check("press-rapid-release",()=>Expect(Vector3.Distance(label.localScale,rest)<.0001f,"Press remained squashed"));
        button.interactable=false;ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerDownHandler);yield return .1;
        Check("press-disabled",()=>Expect(label.localScale==rest,"Disabled button animated"));button.interactable=true;
        ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerDownHandler);yield return .02;button.gameObject.SetActive(false);button.gameObject.SetActive(true);
        Check("press-disable-restores",()=>Expect(label.localScale==rest,"Disable left stale visual scale"));
        var storage=StorageModalUI.Instance;StorageModalUI.Open();yield return .04;
        Diagnostics("popup");
        Check("popup-intermediate-and-input",()=>{Expect(storage.canvasGroup.alpha>0&&storage.canvasGroup.alpha<1,"No intermediate fade");Expect(storage.canvasGroup.blocksRaycasts&&storage.canvasGroup.interactable,"Opening lost input/blocker ownership");var state=storage.modalBox.GetComponent<MobileUIModalMotion>();Expect(state.HasScaleChannel&&state.VisualScale.x/state.RestScale.x>.97f&&state.VisualScale.x/state.RestScale.x<=1,"Ornamental frame scale outside subtle range");});
        storage.Close();yield return .03;StorageModalUI.Open();yield return .22;
        Check("popup-close-reopen-latest-wins",()=>{Expect(storage.IsModalOpen&&storage.gameObject.activeInHierarchy,"Old close hid reopened popup");Expect(storage.canvasGroup.alpha==1&&storage.canvasGroup.interactable&&storage.canvasGroup.blocksRaycasts,"Reopened popup invisible or unclickable");Expect(!storage.modalBox.GetComponent<MobileUIModalMotion>().IsAnimating,"Popup did not settle");});
        for(int i=0;i<8;i++){storage.Close();yield return .015;StorageModalUI.Open();yield return .015;}
        yield return .22;Check("popup-eight-interruptions",()=>Expect(storage.IsModalOpen&&storage.canvasGroup.alpha==1,"Rapid popup state corrupt"));
        storage.gameObject.SetActive(false);StorageModalUI.Open();yield return .22;Check("popup-external-disable-recovery",()=>Expect(storage.IsModalOpen&&storage.canvasGroup.alpha==1,"Disable/re-enable retained hidden pose"));
        storage.Close();Check("closing-releases-input-immediately",()=>Expect(!storage.canvasGroup.blocksRaycasts&&!storage.canvasGroup.interactable,"Closing intercepts next gameplay tap"));yield return .18;
        foreach(var step in NativePopupCases())yield return step;
        var banner=CombatNotificationUI.Instance;CombatNotificationUI.ShowBreakthrough("P08 Realm A");yield return .04;
        Check("breakthrough-banner-passive",()=>{Expect(banner.canvasGroup.alpha>0&&banner.canvasGroup.alpha<=1,"Banner reveal absent");Expect(!banner.canvasGroup.blocksRaycasts&&!banner.canvasGroup.interactable,"Decorative banner intercepts gameplay");});
        CombatNotificationUI.ShowDeath();yield return .25;
        Check("notification-interruption",()=>Expect(banner.gameObject.activeInHierarchy&&banner.canvasGroup.alpha==1,"Latest banner did not settle"));yield return 2.2;
        Check("notification-unscaled-exit",()=>Expect(!banner.gameObject.activeSelf,"Paused notification did not close"));
        CultivationManager.Instance.SetStageAndTuViForEditor(CultivationManager.Instance.allStages.Length-1,0);
        var drawer=MobileDrawerUIController.Instance;
        drawer.OpenDrawer(0);drawer.OpenDrawer(4);drawer.OpenDrawer(1);drawer.OpenDrawer(2);yield return .65;
        Check("tabs-latest-request",()=>{Expect(drawer.ActiveDrawerIndex==2,"Rapid tab intent ignored");Expect(drawer.drawerPanels.Count(p=>p.gameObject.activeSelf)==1,"Multiple drawers visible");});
        drawer.CloseActiveDrawer();yield return .24;drawer.OpenDrawer(0);drawer.CloseActiveDrawer();yield return .48;
        Check("tabs-close-during-open",()=>Expect(!drawer.IsBlockingGameplay&&drawer.ActiveDrawerIndex==-1&&!drawer.backdropOverlay.activeSelf,"Backdrop stuck after rapid close"));
        drawer.OpenDrawer(0);drawer.ToggleDrawer(0);drawer.ToggleDrawer(0);yield return .5;
        Check("tabs-toggle-parity",()=>Expect(drawer.ActiveDrawerIndex==0,"Two queued toggles lost parity"));drawer.CloseActiveDrawer();yield return .24;
        drawer.OpenDrawer(4);yield return .03;drawer.gameObject.SetActive(false);drawer.gameObject.SetActive(true);yield return .3;
        Check("tabs-disable-mid-transition",()=>Expect(!drawer.IsBlockingGameplay&&drawer.drawerPanels.All(p=>!p.gameObject.activeSelf),"Disabled drawer reopened from stale coroutine"));
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)},hits);
        Check("arena-raycast-after-transitions",()=>Expect(!hits.Any(h=>h.gameObject==drawer.backdropOverlay||h.gameObject.GetComponentInParent<MobileUIModalMotion>()!=null),"Animation blocker covers arena"));
        director.reducedMotion=true;StorageModalUI.Open();yield return .04;
        Check("low-motion-no-scale",()=>{var state=storage.modalBox.GetComponent<MobileUIModalMotion>();Expect(state.VisualScale==state.RestScale,"Reduced motion scaled ornamental frame");});yield return .10;
        Check("low-motion-fade-settles",()=>Expect(storage.canvasGroup.alpha==1,"Reduced-motion fade incomplete"));storage.Close();yield return .12;
        drawer.OpenDrawer(2);yield return .12;
        Check("low-motion-tabs-snap",()=>Expect(drawer.ActiveDrawerIndex==2&&drawer.drawerPanels[2].anchoredPosition.y==0,"Reduced-motion drawer still moving"));drawer.CloseActiveDrawer();yield return .12;director.reducedMotion=false;
        EconomyManager.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,linhThach=1000});EconomyManager.Instance.AddLinhThach(321);yield return .03;
        Diagnostics("gain");
        Check("currency-immediate-data-and-gain-only-accent",()=>{Expect(hud.wallet.Find("txt_LinhThach").GetComponent<Text>().text=="LT "+PortraitHudController.Compact(1321),"Animation delayed currency data");Expect(hud.wallet.Find("FeedbackLine").GetComponent<Image>().color.a>0,"Gain accent missing");Expect(hud.realm.Find("FeedbackLine").GetComponent<Image>().color.a==0,"Currency gain flashed progression");});yield return .4;
        Check("currency-accent-rests",()=>Expect(hud.wallet.Find("FeedbackLine").GetComponent<Image>().color.a==0,"Gain glow loops at idle"));
        EconomyManager.Instance.SpendLinhThach(100);yield return .03;Check("spend-is-not-gain",()=>Expect(hud.wallet.Find("FeedbackLine").GetComponent<Image>().color.a==0,"Spend replayed gain effect"));
        CultivationManager.Instance.SetStageAndTuViForEditor(CultivationManager.Instance.currentStageIndex-1,0);yield return .03;
        Check("progression-accent-independent",()=>{Expect(hud.realm.Find("FeedbackLine").GetComponent<Image>().color.a>0,"Progression feedback absent");Expect(hud.wallet.Find("FeedbackLine").GetComponent<Image>().color.a==0,"Progression flashed currency");});yield return .25;
        Check("progression-accent-finite",()=>Expect(hud.realm.Find("FeedbackLine").GetComponent<Image>().color.a==0,"Progression glow loops"));
        MergeManager.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,swords=Enumerable.Range(0,20).Select(i=>new SwordSaveEntry{level=8}).ToArray()});yield return .05;StorageModalUI.Open((StorageCategory)1);yield return .04;
        Check("rarity-real-items-and-six-channel-cap",()=>{
            var frames=storage.GetComponentsInChildren<Image>(true).Where(i=>i.name=="P6Rarity").ToArray();var expected=Find<MobileUIReconstruction>().assets.SwordRarity(8);
            Expect(frames.Length>=12&&frames.All(i=>i.sprite==expected&&!i.raycastTarget),"Rarity reveal is not on real data/nonblocking frames");
            var active=storage.GetComponentsInChildren<MobileUIAccentPulse>(true).Where(p=>p.enabled).ToArray();Expect(active.Length>0&&active.Length<=6,"Rarity cap missing");Expect(active.All(p=>p.GetComponent<Image>().canvasRenderer.GetAlpha()>.55f&&p.GetComponent<Image>().canvasRenderer.GetAlpha()<1),"No intermediate rarity reveal");
        });yield return .4;
        Check("rarity-sleeps-after-reveal",()=>Expect(storage.GetComponentsInChildren<MobileUIAccentPulse>(true).All(p=>!p.enabled&&p.GetComponent<Image>().canvasRenderer.GetAlpha()==1),"Rarity updates at rest"));storage.Close();yield return .18;
        director.reducedMotion=true;StorageModalUI.Open((StorageCategory)1);yield return .12;
        Check("low-motion-rarity-static",()=>Expect(storage.GetComponentsInChildren<MobileUIAccentPulse>(true).All(p=>!p.enabled),"Reduced motion spawned rarity pulses"));storage.Close();yield return .12;director.reducedMotion=false;
        foreach(int width in new[]{360,390})
        {
            SetResolution(width,width==360?640:844);yield return .3;StorageModalUI.Open();yield return .22;
            Check("portrait-popup-"+width,()=>{Expect(Screen.width==width,"Wrong Game View profile");Expect(storage.canvasGroup.alpha==1&&storage.IsModalOpen,"Portrait popup not ready");});
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Evidence+"/storage-"+width+".png"));yield return .1;storage.Close();yield return .18;
        }
        MobileUIAnimationBuilder.Capture("runtime-scene");
        Check("motion-never-owns-time-scale",()=>Expect(Time.timeScale==0,"UI changed gameplay clock"));
        yield return .2;
    }
    static IEnumerable NativePopupCases()
    {
        var character=Find<CharacterDetailModalUI>();character.OpenPanel();yield return .03;character.ClosePanel();yield return .03;character.OpenPanel();yield return .23;
        Check("character-rapid-open-close",()=>Expect(character.canvasGroup.alpha==1&&character.gameObject.activeInHierarchy,"Character popup stale close"));character.ClosePanel();yield return .18;
        AchievementModalUI.Open();yield return .03;AchievementModalUI.Close();yield return .03;AchievementModalUI.Open();yield return .22;
        var achievement=AchievementModalUI.Instance;Check("achievement-rapid-open-close",()=>Expect(achievement.IsModalOpen&&achievement.canvasGroup.alpha==1,"Achievement popup stale close"));AchievementModalUI.Close();yield return .18;
        int confirmed=0;DismantleConfirmModalUI.OpenConfirm("P08 fixture","visual QA",new DismantleReward(),()=>confirmed++);yield return .03;DismantleConfirmModalUI.Instance.Close();yield return .03;
        DismantleConfirmModalUI.OpenConfirm("P08 fixture","visual QA",new DismantleReward(),()=>confirmed++);yield return .04;
        var dismantle=DismantleConfirmModalUI.Instance;var confirm=(Button)typeof(DismantleConfirmModalUI).GetField("btnConfirm",All).GetValue(dismantle);Click(confirm);yield return .18;
        Check("confirm-during-animation-exactly-once",()=>Expect(confirmed==1&&!dismantle.gameObject.activeSelf,"Animation blocked or duplicated confirm"));
        var offline=OfflineEarningsManager.Instance;var reward=new OfflineEarningsManager.OfflineRewardData{hasReward=true,linhThach=500,offlineSeconds=600};
        typeof(OfflineEarningsManager).GetProperty("PendingReward",All).SetValue(offline,reward);EconomyManager.Instance.currentLinhThach=1000;OfflineRewardPopupUI.Show(reward);yield return .04;
        var popup=OfflineRewardPopupUI.Instance;Click(popup.claimNormalButton);yield return .4;
        Check("reward-reveal-never-grants",()=>{Expect(EconomyManager.Instance.currentLinhThach==1500,"Reveal changed reward amount/count");Expect(!offline.HasPendingReward,"Reward owner did not consume pending");Expect(!popup.gameObject.activeSelf,"Reward close did not finish");});
        SecretRealmPlayController.Instance.ShowResultModal(new SecretRealmService.RewardResult{linhThach=123,ore=2});yield return .08;
        var result=SecretRealmPlayController.Instance.ActiveNavigationLayer;var card=result.Find("Card");
        Check("result-reveal-intermediate",()=>{Expect(card.GetComponent<CanvasGroup>().alpha>0&&card.GetComponent<CanvasGroup>().alpha<1,"Reward result has no reveal");Expect(result.Find("Card/RewardBox/RewardDetailsText").GetComponent<Text>().text.Contains("123"),"Reveal delayed result data");Expect(EconomyManager.Instance.currentLinhThach==1500,"Showing result granted money");});
        Click(result.Find("Card/BtnConfirm").GetComponent<Button>());yield return .1;SecretRealmModalUI.Instance.Close();yield return .1;
        Check("result-close-cancels-reveal",()=>Expect(SecretRealmPlayController.Instance.ActiveNavigationLayer==null,"Result reappeared after interrupted reveal"));
        var cultivation=CultivationManager.Instance;cultivation.SetStageAndTuViForEditor(14,cultivation.allStages[14].requiredTuVi);AlchemyService.Instance.State.pills[5]=1;
        TribulationModalUI.Open();yield return .03;TribulationModalUI.Close();yield return .03;TribulationModalUI.Open();yield return .22;
        var tribulation=TribulationModalUI.Instance;Check("tribulation-rapid-open-close",()=>Expect(tribulation.IsOpen&&tribulation.canvasGroup.alpha==1,"Tribulation motion state corrupt"));Click(tribulation.cancelButton);yield return .18;
        Check("tribulation-animation-does-not-consume-pill",()=>Expect(AlchemyService.Instance.State.pills[5]==1&&!CombatManager.Instance.isTribulationBossActive,"Presentation started/consumed gameplay"));
    }
    static IEnumerable StressProfile(MobileUIAnimationDirector director)
    {
        var canvas=GameObject.Find("Canvas");var graphics=canvas.GetComponentsInChildren<Image>().Where(i=>i.isActiveAndEnabled&&!i.raycastTarget).Take(12).Cast<Graphic>().ToArray();
        var pulses=graphics.Select(g=>g.gameObject.GetComponent<MobileUIAccentPulse>()??g.gameObject.AddComponent<MobileUIAccentPulse>()).ToArray();
        for(int i=0;i<pulses.Length;i++)pulses[i].Play(graphics[i],director.accent,false);yield return .4;
        var label=Find<PortraitHudController>().menuButton.GetComponentInChildren<Text>().rectTransform;Vector3 rest=label.localScale;director.Press(label,rest,true);yield return .1;director.Press(label,rest,false);yield return .15;
        var scale=label.GetComponent<MobileUIVisualScaleMotion>();
        using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",256))
        using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",256))
        using(var ui=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"KV.UI.Motion",256))
        {
            int start=Time.frameCount,last=-1;while(Time.frameCount<start+120)
            {
                if(Time.frameCount!=last)
                {
                    last=Time.frameCount;int n=last-start;
                    if(n%24==0)for(int i=0;i<pulses.Length;i++)pulses[i].Play(graphics[i],director.accent,false);
                    if(n%20==0)scale.Play(rest,n%40==0?director.buttonDown:director.buttonUp,false);
                }
                yield return null;
            }
            cpu.Stop();gc.Stop();ui.Stop();var c=new List<ProfilerRecorderSample>();var a=new List<ProfilerRecorderSample>();var m=new List<ProfilerRecorderSample>();cpu.CopyTo(c);gc.CopyTo(a);ui.CopyTo(m);
            File.WriteAllText(Evidence+"/motion-profiler.json",JObject.FromObject(new{frameStart=start,frameEnd=Time.frameCount,width=Screen.width,height=Screen.height,timeScale=Time.timeScale,decorativeChannels=pulses.Length,mainThreadNs=c.Select(s=>s.Value).ToArray(),gcAllocatedBytes=a.Select(s=>s.Value).ToArray(),motionNs=m.Select(s=>s.Value).ToArray(),motionRecorderValid=ui.Valid,scope="Editor stress above production rarity cap; no mobile FPS claim"}).ToString());
        }
        scale.Cancel();yield return .45;
        Check("finite-channels-sleep-at-rest",()=>Expect(pulses.All(p=>!p.enabled)&&!scale.enabled,"Animation updates continue at rest"));
    }
    static void SetResolution(int width,int height)
    {
        var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var kind=asm.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(sizeType,All,null,new object[]{Enum.ToObject(kind,1),width,height,"Phase08 portrait QA"},null);group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));var prop=view.GetType().GetProperty("selectedSizeIndex",All);
        if(!SessionState.GetBool(Key+"ViewSaved",false)){SessionState.SetInt(Key+"View",(int)prop.GetValue(view));SessionState.SetBool(Key+"ViewSaved",true);}
        prop.SetValue(view,(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1);view.Focus();
    }
    static void Diagnostics(string label)
    {
        var d=Find<MobileUIAnimationDirector>();var hud=Find<PortraitHudController>();var s=StorageModalUI.Instance;var state=s.modalBox.GetComponent<MobileUIModalMotion>();
        var cached=typeof(PortraitHudController).GetField("uiMotion",All).GetValue(hud) as MobileUIAnimationDirector;
        File.WriteAllText(Evidence+"/diagnostic-"+label+".json",JObject.FromObject(new{reduced=d.reducedMotion,cachedDirectorPresent=cached!=null,cachedMatchesDirector=ReferenceEquals(cached,d),forStorage=MobileUIAnimationDirector.For(s)!=null,forHud=MobileUIAnimationDirector.For(hud)!=null,forSecret=MobileUIAnimationDirector.For(SecretRealmPlayController.Instance)!=null,storageScale=s.modalBox.localScale.ToString(),storageRest=state!=null?state.RestScale.ToString():"no state",storageAlpha=s.canvasGroup.alpha,walletTimer=typeof(PortraitHudController).GetField("walletPulseTime",All).GetValue(hud),walletColor=hud.wallet.Find("FeedbackLine").GetComponent<Image>().color.ToString(),realmColor=hud.realm.Find("FeedbackLine").GetComponent<Image>().color.ToString(),frame=Time.frameCount,dt=Time.unscaledDeltaTime}).ToString());
    }
}
#endif

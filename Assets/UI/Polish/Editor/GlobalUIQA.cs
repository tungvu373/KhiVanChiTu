#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using TMPro;
using KhiVanChiTu.Collaboration;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Polish.Editor
{
    [InitializeOnLoad]
    public static class GlobalUIQA
    {
        const string Key="GlobalUIQA";
        static string Profile=>SessionState.GetString(Key+"Profile","");
        static string Mode=>SessionState.GetString(Key+"Mode","");
        static string Evidence=>Mode=="refinement"?"UIUX_Polish/Evidence/refinement":"Docs/UI_Redesign/"+((Mode=="polish"||Mode=="baseline")?"Phase9":"Phase10")+"/Evidence/"+Mode;
        static IEnumerator routine;static double due;
        static readonly List<object> rows=new List<object>();
        static readonly List<string> errors=new List<string>();
        static EditorWindow gameView;static PropertyInfo selectedSize;static object sizeGroup;static int originalSize;static readonly List<int> addedSizes=new List<int>();
        static Vector2Int wantedSize;static int wantedIndex;static double rasterDeadline;
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        static GlobalUIQA()
        {
            EditorApplication.playModeStateChanged+=s=>{if(!SessionState.GetBool(Key,false))return;if(s==PlayModeStateChange.EnteredPlayMode){routine=Run();due=EditorApplication.timeSinceStartup+2;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Cleanup;};
            EditorApplication.update+=()=>{if(SessionState.GetBool(Key,false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling)Cleanup();};
        }
        public static void Begin(string mode="baseline")
        {
            KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Require();
            if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include).Length>0)throw new Exception("Save profile already owned");
            SessionState.SetString(Key+"Profile","global-ui-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));SessionState.SetString(Key+"Mode",mode);
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));SessionState.SetFloat(Key+"Scale",Time.timeScale);
            rows.Clear();errors.Clear();wantedSize=Vector2Int.zero;Directory.CreateDirectory(Evidence);SessionState.SetBool(Key,true);
            new GameObject("__GlobalUIQA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;EditorApplication.isPlaying=true;
        }
        static void Log(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(m+"\n"+stack);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<due)return;EditorApplication.QueuePlayerLoopUpdate();
            try{
                if(wantedSize.x>0&&(Screen.width!=wantedSize.x||Screen.height!=wantedSize.y))
                {if(EditorApplication.timeSinceStartup>rasterDeadline)throw new Exception("Raster did not settle to "+wantedSize+"; actual "+Screen.width+"x"+Screen.height);selectedSize.SetValue(gameView,wantedIndex);gameView.Focus();return;}
                rasterDeadline=EditorApplication.timeSinceStartup+15;
                if(!routine.MoveNext()){Stop();return;}due=EditorApplication.timeSinceStartup+.01;}
            catch(Exception e){Case("HARNESS","Suite completes without unexpected exception",()=>throw e);Stop();}
        }
        static void Expect(bool b,string m){if(!b)throw new Exception(m);}
        static void Case(string id,string expected,Action action,string evidence=null)
        {
            string error=null;try{action();}catch(Exception e){error=e.Message;}
            rows.Add(new{id,description=id,expected,actual=error??expected,status=error==null?"PASS":"FAIL",evidence,frame=Time.frameCount,width=Screen.width,height=Screen.height,profile=Profile});
            File.WriteAllText(Evidence+"/cases.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
        }
        static void Capture(string name){ScreenCapture.CaptureScreenshot(Evidence+"/"+name+"-"+Screen.width+"x"+Screen.height+".png");}
        static void FitText(Transform root)
        {
            Canvas.ForceUpdateCanvases();var bad=new List<string>();foreach(var t in root.GetComponentsInChildren<TMP_Text>())
            {if(!t.isActiveAndEnabled||t.overflowMode==TextOverflowModes.Ellipsis)continue;t.ForceMeshUpdate();if(t.isTextOverflowing)bad.Add(t.name+": "+t.text);}
            Expect(bad.Count==0,string.Join(" | ",bad));
        }
        static IEnumerator Run()
        {
            yield return null;
            Expect(DataManager.Instance.SaveProfileId==Profile,"Isolation required before fixture writes");
            var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Huyền Thiên Đạo Hữu Trường Sinh Vô Cực",stageIndex=26,realmId=5,subLevel=1,combatStage=5,linhThach=1e30f,playerMaxHp=50000,playerCurrentHp=50000,monsterSwordsAcquired=600};
            slot.swords=Enumerable.Range(0,64).Select(i=>new SwordSaveEntry{level=1+i%10,pieces=1,isEquipped=i<3,equipSlotIndex=i<3?i:-1}).ToArray();slot.artifacts.ore=99999;slot.alchemy.herbs=new[]{999999,888888,777777};slot.formations.levels=new[]{1,1,1,1,1,1};
            DataManager.Instance.gameSaveData.slots[0]=slot;MainMenuManager.Instance.AskConfirmEnterGame(0,slot);yield return null;MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();
            // Native LoadCurrentGame is delayed by scaled Invoke(0.05). Never pause before it runs.
            double loadDeadline=EditorApplication.timeSinceStartup+12;
            while(!DataManager.Instance.isSessionActive&&EditorApplication.timeSinceStartup<loadDeadline)yield return null;
            Expect(DataManager.Instance.isSessionActive,"Native delayed LoadCurrentGame did not run");
            foreach(var x in Delay(.4f))yield return x;Time.timeScale=0;
            SetupSizes();SelectSize(new Vector2Int(1080,1920));yield return null;
            if(Mode=="combat"){foreach(var x in CombatWindow())yield return x;yield break;}
            var nav=GlobalNavigationController.Instance;nav.SelectGroup(NavigationGroup.Equipment);yield return null;nav.OpenRoute(NavigationRoute.Storage);yield return null;yield return null;yield return null;
            foreach(var x in MeasureInventory())yield return x;
            StorageModalUI.Instance.Close();foreach(var x in Delay(.45f))yield return x;
            if(Mode=="baseline")yield break;
            if(Mode=="refinement")foreach(var x in KhiVanChiTu.UI.Refinement.Editor.HudPolishChecks.Run(nav,SelectSize,Case,Capture))yield return x;
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280),new Vector2Int(1440,3200)})
            {
                SelectSize(size);yield return null;yield return null;nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Delay(.45f))yield return x;
                Case("HUD-full-"+size,"Full HUD visible with bounded HP/MP",()=>{Expect(nav.hud.gameObject.activeInHierarchy,"HUD hidden");Expect(nav.hud.character.canvasRenderer.GetInheritedAlpha()>.9f,"HUD still gated");foreach(var p in new[]{nav.hud.hp,nav.hud.mp}){var r=(RectTransform)p.transform;Expect(r.rect.width>100&&r.rect.height>20,"Vital bar dimension");Expect(p.Normalized>=0&&p.Normalized<=1,"Vital range");}});Capture("hud-full");yield return null;
                nav.hud.drawer.ToggleDockCollapse();foreach(var x in Delay(.4f))yield return x;
                Case("HUD-compact-"+size,"Compact HUD keeps vitals and gameplay clear",()=>{Expect(nav.hud.IsCompact,"Compact state missing");Expect(nav.hud.hp.gameObject.activeInHierarchy&&nav.hud.mp.gameObject.activeInHierarchy,"Compact vitals hidden");Expect(nav.hud.ClearHeightFraction>.45f,"Insufficient clear gameplay area");});Capture("hud-compact");yield return null;
                nav.hud.drawer.ToggleDockCollapse();foreach(var x in Delay(.4f))yield return x;
                foreach(var route in new[]{NavigationRoute.Merge,NavigationRoute.Storage,NavigationRoute.Body,NavigationRoute.Alchemy,NavigationRoute.Artifact,NavigationRoute.Formation,NavigationRoute.Talents,NavigationRoute.Milestones,NavigationRoute.RealmList,NavigationRoute.Character,NavigationRoute.Achievements,NavigationRoute.Resources,NavigationRoute.Buffs,NavigationRoute.Log,NavigationRoute.Supplies,NavigationRoute.SwordStyle,NavigationRoute.Settings})
                {
                    nav.OpenRoute(route);foreach(var x in Delay(.45f))yield return x;
                    Case("layout-"+route+"-"+size,"Active redesigned text fits without clipping",()=>{foreach(var scope in UnityEngine.Object.FindObjectsByType<RpgViewScope>())if(scope.isActiveAndEnabled)FitText(scope.transform);},route+"-"+Screen.width+"x"+Screen.height+".png");Capture(route.ToString());yield return null;
                    if(route==NavigationRoute.Achievements||route==NavigationRoute.Resources||route==NavigationRoute.Buffs||route==NavigationRoute.Log||route==NavigationRoute.SwordStyle)
                        Case("legacy-surfaces-"+route+"-"+size,"Remaining legacy window frames use the Foundation surface",()=>{var windows=UnityEngine.Object.FindObjectsByType<LegacySurfacePolish>();Expect(windows.Length>0,"No legacy window active");foreach(var window in windows)foreach(var image in window.target.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i=>i.isActiveAndEnabled))if(image.sprite!=null&&image.rectTransform.rect.width>150)Expect(!image.sprite.name.StartsWith("JadeV"),"Ornate legacy frame remains: "+image.name);});
                    if(route==NavigationRoute.Settings)PauseMenuManager.Instance.ResumeGame();else nav.Back();foreach(var x in Delay(.4f))yield return x;
                    // Close secondary overlays before testing the next independent route.
                    for(int n=0;n<4&&nav.TopLayer!="";n++){nav.Back();yield return null;}
                }
            }
            foreach(var step in MotionChecks(nav))yield return step;
            Case("skill-input-mana-cooldown","Native normal skill consumes correct mana, enters cooldown and shows lock",Skills);
            Case("progress-invalid-large-values","Progress clamps invalid and oversized values",()=>{var p=UnityEngine.Object.FindAnyObjectByType<FoundationProgress>(FindObjectsInactive.Include);p.Set(float.NaN,0);Expect(p.Normalized==0,"NaN");p.Set(1e30f,1);Expect(p.Normalized==1,"clamp");p.Set(-1,100);Expect(p.Normalized==0,"negative");});
            Case("missing-scripts","Scene has no missing scripts",()=>{foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Expect(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,t.name);});
            Case("runtime-errors","No runtime errors during this run",()=>Expect(errors.Count==0,string.Join("\n",errors)));
        }
        static IEnumerable<object> Delay(float seconds){float until=Time.unscaledTime+seconds;while(Time.unscaledTime<until)yield return null;}
        static IEnumerable<object> CombatWindow()
        {
            var nav=GlobalNavigationController.Instance;nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Delay(.45f))yield return x;
            var names=new[]{"Main Thread","Canvas.BuildBatch","GC Allocated In Frame","GPU Frame Time","Texture Memory","SetPass Calls Count"};
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);var recorders=handles.Select(ProfilerRecorderHandle.GetDescription).Where(d=>names.Contains(d.Name)).Select(d=>(name:d.Name,unit:d.UnitType.ToString(),recorder:ProfilerRecorder.StartNew(d.Category,d.Name,512))).ToArray();
            Time.timeScale=1;int begin=Time.frameCount,last=begin,count=0;float start=Time.realtimeSinceStartup;
            while(count<240&&Time.realtimeSinceStartup-start<30){if(Time.frameCount!=last){last=Time.frameCount;count++;}yield return null;}
            float seconds=Time.realtimeSinceStartup-start;var counters=new List<object>();foreach(var entry in recorders){var r=entry.recorder;r.Stop();var samples=new List<ProfilerRecorderSample>();r.CopyTo(samples);counters.Add(new{entry.name,entry.unit,valid=r.Valid,values=samples.Select(v=>v.Value).ToArray()});r.Dispose();}
            File.WriteAllText(Evidence+"/combat-profile.json",JsonConvert.SerializeObject(new{editorOnly=true,scriptDriven=true,frames=count,beginFrame=begin,endFrame=Time.frameCount,seconds,observedLoopFramesPerSecond=count/seconds,counters},Formatting.Indented));
            Case("combat-advances","240 actual combat frames observed with timeScale 1",()=>Expect(count==240,"Only "+count));Capture("combat-hud");yield return null;Time.timeScale=0;
            bool old=StageManager.Instance.isBossStage;float timer=StageManager.Instance.bossTimer;StageManager.Instance.isBossStage=true;StageManager.Instance.bossTimer=17;foreach(var x in Delay(.25f))yield return x;
            Case("boss-bound-fixture","Boss banner, timer and progress respond to existing StageManager state",()=>{Expect(nav.hud.boss.gameObject.activeInHierarchy,"Boss banner hidden");Expect(nav.hud.timer.text.Contains("17"),"Timer stale");Expect(nav.hud.bossHealth.Normalized>=0&&nav.hud.bossHealth.Normalized<=1,"Boss progress invalid");FitText(nav.hud.boss);});Capture("boss-fixture");yield return null;StageManager.Instance.isBossStage=old;StageManager.Instance.bossTimer=timer;
            CultivationManager.Instance.SetStageAndTuViForEditor(0,0);yield return null;CultivationManager.Instance.SetStageAndTuViForEditor(26,0);foreach(var x in Delay(.2f))yield return x;
            Case("unlock-feedback","Real unlock event produces presentation feedback",()=>Expect(GlobalFeedbackHost.LastMessage.Contains("Đã mở khóa"),"Unlock toast absent"));
            nav.OpenRoute(NavigationRoute.Body);foreach(var x in Delay(.45f))yield return x;
            var body=UnityEngine.Object.FindObjectsByType<KhiVanChiTu.UI.Cultivation.CultivationScreen>(FindObjectsInactive.Include).First(v=>v.kind==KhiVanChiTu.UI.Cultivation.CultivationScreenKind.Body);body.Select(0);body.primary.button.onClick.Invoke();foreach(var x in Delay(.2f))yield return x;
            Case("upgrade-toast","Successful native upgrade feedback reaches global toast",()=>Expect(GlobalFeedbackHost.LastMessage.Contains("Thành công"),"Upgrade toast absent"));Capture("upgrade-feedback");yield return null;nav.Back();foreach(var x in Delay(.45f))yield return x;
            Case("combat-runtime-errors","No runtime errors during combat measurement",()=>Expect(errors.Count==0,string.Join("\n",errors)));
        }
        static IEnumerable<object> MotionChecks(GlobalNavigationController nav)
        {
            nav.OpenRoute(NavigationRoute.Body);foreach(var x in Delay(.45f))yield return x;
            var body=UnityEngine.Object.FindObjectsByType<KhiVanChiTu.UI.Cultivation.CultivationScreen>(FindObjectsInactive.Include).First(v=>v.kind==KhiVanChiTu.UI.Cultivation.CultivationScreenKind.Body);body.Select(0);
            var a=body.primary;var rect=(RectTransform)a.transform;var rest=rect.localScale;var labelRest=a.label.rectTransform.localScale;
            var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
            UnityEngine.EventSystems.ExecuteEvents.Execute(a.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);foreach(var x in Delay(.11f))yield return x;
            Case("motion-press","TMP caption presses without moving touch target",()=>{Expect(rect.localScale==rest,"Touch target scaled");Expect(a.label.rectTransform.localScale.x<labelRest.x,"Caption did not press");});Capture("button-pressed");yield return null;
            UnityEngine.EventSystems.ExecuteEvents.Execute(a.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);foreach(var x in Delay(.25f))yield return x;
            Case("motion-release","Press returns to rest with timeScale zero",()=>Expect((a.label.rectTransform.localScale-labelRest).sqrMagnitude<.0001f,"Did not settle"));
            EconomyManager.Instance.currentLinhThach=0;body.Refresh();UnityEngine.EventSystems.ExecuteEvents.Execute(a.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);foreach(var x in Delay(.2f))yield return x;
            Case("insufficient-feedback","Disabled action shows its actual condition without running command",()=>Expect(GlobalFeedbackHost.LastMessage==body.condition.text,"Missing actual unavailable reason"));Capture("insufficient-toast");yield return null;
            GlobalFeedbackHost.Notify("Đã nhận thưởng · Huyền Thiên Cửu Chuyển Vạn Niên Linh Kiếm · 1.23E+30 Linh Thạch");foreach(var x in Delay(.2f))yield return x;
            var host=UnityEngine.Object.FindAnyObjectByType<GlobalFeedbackHost>();
            Case("toast-layout-input","Long toast wraps and never blocks input",()=>{FitText(host.notice.transform);Expect(!host.notice.GetComponent<CanvasGroup>().blocksRaycasts,"Toast blocks");});Capture("long-toast");yield return null;
            foreach(var x in Delay(2.8f))yield return x;Case("toast-expiry","Toast expires while gameplay is paused",()=>Expect(!host.notice.gameObject.activeSelf,"Toast stuck"));
            EconomyManager.Instance.currentLinhThach=1e30f;body.Refresh();nav.Back();foreach(var x in Delay(.45f))yield return x;
            Case("motion-sleeps","All transient motion channels sleep at rest",()=>{Expect(UnityEngine.Object.FindObjectsByType<MobileUIAccentPulse>().All(p=>!p.enabled),"Accent running");Expect(UnityEngine.Object.FindObjectsByType<MobileUIVisualScaleMotion>().All(p=>!p.enabled),"Press running");});
        }
        static void Skills()
        {
            var sm=SkillManager.Instance;var ui=UnityEngine.Object.FindAnyObjectByType<MobileSkillHUD>(FindObjectsInactive.Include);var original=sm.normalSkills;var copies=original.Select(UnityEngine.Object.Instantiate).ToArray();
            try{sm.normalSkills=copies;sm.maxLinhLuc=1000;sm.currentLinhLuc=1000;var cooldown=(float[])typeof(SkillManager).GetField("normalCooldowns",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sm);copies[0].isUnlocked=true;cooldown[0]=0;float before=sm.currentLinhLuc,cost=sm.GetSkillManaCost(0);ui.SendMessage("UpdateNormalSkillsUI",sm);ui.btnNormalSkills[0].onClick.Invoke();Expect(Mathf.Approximately(sm.currentLinhLuc,before-cost),"mana cost");Expect(sm.GetNormalSkillCooldown(0)>0,"cooldown absent");ui.SendMessage("UpdateNormalSkillsUI",sm);Expect(!ui.btnNormalSkills[0].interactable,"cooldown interactable");copies[0].isUnlocked=false;ui.SendMessage("UpdateNormalSkillsUI",sm);Expect(ui.lockOverlays[0].activeSelf,"lock missing");}
            finally{sm.normalSkills=original;foreach(var c in copies)UnityEngine.Object.Destroy(c);}
        }
        static IEnumerable<object> MeasureInventory()
        {
            var view=StorageModalUI.Instance.presentation;
            var method=typeof(InventoryPresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            method.Invoke(view,null);long before=GC.GetAllocatedBytesForCurrentThread();var clock=System.Diagnostics.Stopwatch.StartNew();for(int n=0;n<100;n++)method.Invoke(view,null);clock.Stop();long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
            var names=new[]{"Main Thread","GC Allocated In Frame","Texture Memory","Canvas.BuildBatch","Canvas.SendWillRenderCanvases","LayoutRebuilder.Rebuild","Draw Calls Count","SetPass Calls Count","GPU Frame Time"};
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);var descriptions=handles.Select(ProfilerRecorderHandle.GetDescription).Where(d=>names.Contains(d.Name)).ToArray();
            var recorders=descriptions.Select(d=>(name:d.Name,category:d.Category.Name,unit:d.UnitType.ToString(),recorder:ProfilerRecorder.StartNew(d.Category,d.Name,256))).ToList();
            var graphics=view.GetComponentsInChildren<UnityEngine.UI.Graphic>();int layouts=0,vertices=0;UnityEngine.Events.UnityAction layout=()=>layouts++,vertex=()=>vertices++;
            foreach(var g in graphics){g.RegisterDirtyLayoutCallback(layout);g.RegisterDirtyVerticesCallback(vertex);}
            int start=Time.frameCount,last=start,count=0;double deadline=EditorApplication.timeSinceStartup+35;
            while(count<120){if(EditorApplication.timeSinceStartup>deadline)break;if(Time.frameCount!=last){last=Time.frameCount;count++;}yield return null;}
            foreach(var g in graphics){g.UnregisterDirtyLayoutCallback(layout);g.UnregisterDirtyVerticesCallback(vertex);}
            var counters=new List<object>();foreach(var entry in recorders){var r=entry.recorder;r.Stop();var samples=new List<ProfilerRecorderSample>();r.CopyTo(samples);counters.Add(new{entry.name,entry.category,entry.unit,valid=r.Valid,values=samples.Select(x=>x.Value).ToArray()});r.Dispose();}
            var report=new{mode=Mode,profile=Profile,width=Screen.width,height=Screen.height,startFrame=start,endFrame=Time.frameCount,observedFrames=count,editorOnly=true,reflectionCallbackIterations=100,reflectionCallbackBytes=bytes,reflectionCallbackMs=clock.Elapsed.TotalMilliseconds,dirtyLayout=layouts,dirtyVertices=vertices,activeGraphics=graphics.Length,raycastTargets=graphics.Count(g=>g.raycastTarget),counters,unavailable=names.Where(n=>!descriptions.Any(d=>d.Name==n)).ToArray(),textures=UnityEngine.Resources.FindObjectsOfTypeAll<Texture2D>().Where(t=>t.width>1).Select(t=>new{t.name,t.width,t.height,bytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)}).OrderByDescending(t=>t.bytes).Take(25).ToArray()};
            File.WriteAllText(Evidence+"/inventory-profile.json",JsonConvert.SerializeObject(report,Formatting.Indented));Case("profile-120-frames","Measured 120 advancing Editor frames",()=>Expect(count==120,"Only "+count+" frames"));
        }
        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});gameView=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));selectedSize=gameView.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);originalSize=(int)selectedSize.GetValue(gameView);
        }
        static void SelectSize(Vector2Int size){var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Global QA "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);addedSizes.Add(count-1);wantedSize=size;wantedIndex=count-1;rasterDeadline=EditorApplication.timeSinceStartup+15;selectedSize.SetValue(gameView,count-1);gameView.Focus();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/console-errors.json",JsonConvert.SerializeObject(errors,Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;
            SessionState.SetBool(Key,false);
            foreach(var m in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include))if(m.profileId==Profile)UnityEngine.Object.DestroyImmediate(m.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(gameView!=null&&selectedSize!=null){selectedSize.SetValue(gameView,originalSize);for(int i=addedSizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{addedSizes[i]});}addedSizes.Clear();
            File.WriteAllText(Evidence+"/preservation.json",JsonConvert.SerializeObject(new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")},Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif

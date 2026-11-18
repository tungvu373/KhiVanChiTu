#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.Collaboration;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment.Editor;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Progression.Editor
{
    [InitializeOnLoad]
    public static class ProgressionQA
    {
        static string Profile=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Profile("progression-phase6-qa");
        const string Key="ProgressionQA";
        public static string Evidence=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Evidence("Docs/UI_Redesign/Phase6/Evidence");
        static IEnumerator routine;
        static double due;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static EditorWindow view;
        static PropertyInfo sizeProperty;
        static object sizeGroup;
        static int oldSize;
        static readonly List<int> sizes=new List<int>();
        static ProgressionQA(){EditorApplication.update+=()=>{if(SessionState.GetBool(Key,false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling)Cleanup();};EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))Attach();if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Cleanup;};}
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        public static void ActionsOnly(){Begin();SessionState.SetBool(Key+"ActionsOnly",true);}
        public static void Preview(){Begin();SessionState.SetBool(Key+"Preview",true);}
        public static void Begin(int phase=6)
        {
            EquipmentRedesignBuilder.Require();if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0)throw new Exception("A save profile owner already exists.");
            SessionState.SetBool(Key+"ActionsOnly",false);SessionState.SetBool(Key+"Preview",false);SessionState.SetInt(Key+"Phase",phase);Directory.CreateDirectory(Evidence);if(File.Exists(Evidence+"/failure.txt"))File.Move(Evidence+"/failure.txt",Evidence+"/previous-failure-"+DateTime.UtcNow.Ticks+".txt");checks.Clear();errors.Clear();
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));SessionState.SetFloat(Key+"Scale",Time.timeScale);SessionState.SetBool(Key,true);
            new GameObject("__Phase6_DisposableQA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;
            EditorApplication.isPlaying=true;
        }
        static void Attach(){routine=Run();due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<due)return;
            try{if(!routine.MoveNext()){File.WriteAllText(Evidence+"/checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));Stop();}else due=EditorApplication.timeSinceStartup+.45;}
            catch(Exception e){File.WriteAllText(Evidence+"/failure.txt",e.ToString());Debug.LogException(e);Stop();}
        }
        static void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m+"\n"+stack);}
        static void Check(bool b,string m){if(!b)throw new Exception(m);checks.Add(m);}
        static void Capture(string n){ScreenCapture.CaptureScreenshot(Evidence+"/"+n+"-"+Screen.width+"x"+Screen.height+".png");}

        static IEnumerator Run()
        {
            while(DataManager.Instance==null||MainMenuManager.Instance==null)yield return null;
            Check(DataManager.Instance.SaveProfileId==Profile,"Disposable progression profile active");
            var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Đạo Hữu Phase 6",linhThach=100000,coDuyen=100,stageIndex=15,combatStage=5,playerMaxHp=50000,playerCurrentHp=50000,monsterSwordsAcquired=600};
            slot.swords=new[]{new SwordSaveEntry{level=5,pieces=1,isEquipped=true,equipSlotIndex=0}};slot.claimedMilestoneIds=new[]{"kill_10"};
            DataManager.Instance.gameSaveData.slots[0]=slot;MainMenuManager.Instance.AskConfirmEnterGame(0,slot);yield return null;MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();yield return null;yield return null;yield return null;
            int frame=Time.frameCount;yield return null;Check(Time.frameCount>frame,"Game loop advances");Time.timeScale=0;StageManager.Instance.enemiesKilled=30;
            var nav=GlobalNavigationController.Instance;var milestone=UnityEngine.Object.FindObjectsByType<ProgressionScreen>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>!v.talents);var talents=UnityEngine.Object.FindObjectsByType<ProgressionScreen>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(v=>v.talents);
            SetupSizes();
            if(SessionState.GetBool(Key+"Preview",false)){SelectSize(new Vector2Int(1080,1920));nav.SelectGroup(NavigationGroup.Other);yield return null;nav.OpenRoute(NavigationRoute.Milestones);while(true)yield return null;}
            foreach(var size in SessionState.GetBool(Key+"ActionsOnly",false)?Array.Empty<Vector2Int>():new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;nav.SelectGroup(NavigationGroup.Other);yield return null;nav.OpenRoute(NavigationRoute.Milestones);yield return null;yield return null;
                Check(milestone.gameObject.activeInHierarchy&&nav.TopLayer=="milestones","Milestone opens above navigation "+size);
                Check(milestone.VisibleCount==MilestoneManager.Instance.allQuests.Count,"All milestone rows present "+size);
                Check(milestone.FindCard("kill_10").State==ProgressionState.Completed,"Claimed milestone is Completed "+size);Check(milestone.FindCard("kill_30").State==ProgressionState.Claimable,"Eligible milestone is Claimable "+size);Check(milestone.FindCard("kill_80").State==ProgressionState.InProgress,"Unmet milestone is In Progress "+size);
                Check(milestone.content.GetChild(0).GetComponent<ProgressionCard>().State==ProgressionState.Claimable,"Claimable rows lead the list "+size);
                Verify(milestone);SurfaceTap(milestone);Hit(milestone.close.button);Drag(milestone.branchScroll,true);Drag(milestone.stateScroll,true);Drag(milestone.scroll,false);milestone.scroll.verticalNormalizedPosition=1;Capture("milestones-top");yield return null;
                milestone.SetStateFilter(1);yield return null;Check(milestone.Cards.Where(r=>r.gameObject.activeSelf).All(r=>r.State==ProgressionState.Claimable),"Actionable milestone filter "+size);
                milestone.SetStateFilter(3);yield return null;Check(milestone.VisibleCount==0&&milestone.empty.gameObject.activeInHierarchy,"No fabricated milestone locks and correct empty state "+size);milestone.SetStateFilter(0);
                foreach(string branch in milestone.BranchKeys){milestone.SetBranch(branch);yield return null;Check(milestone.VisibleCount==(branch=="all"?16:MilestoneManager.Instance.allQuests.Count(q=>((int)q.type).ToString()==branch)),"Milestone branch "+branch+" "+size);}
                milestone.SetBranch("all");Canvas.ForceUpdateCanvases();milestone.scroll.StopMovement();milestone.scroll.verticalNormalizedPosition=0;yield return null;Check(milestone.scroll.verticalNormalizedPosition<.01f&&milestone.content.rect.height>milestone.scroll.viewport.rect.height,"Long milestone list actually scrolls "+size);Verify(milestone);Capture("milestones-bottom");yield return null;
                milestone.talentTab.button.onClick.Invoke();yield return null;yield return null;Check(talents.gameObject.activeInHierarchy&&!milestone.gameObject.activeSelf&&nav.TopLayer=="talents","Cross-navigation opens only talents "+size);
                Check(talents.VisibleCount==DaoAnProgressionService.Instance.Catalog.nodes.Length,"All talents bound "+size);Check(talents.FindCard("wealth").State==ProgressionState.Available,"Affordable root talent available "+size);Check(talents.FindCard("sword_luck").State==ProgressionState.Locked,"Prerequisite talent locked "+size);
                Verify(talents);SurfaceTap(talents);Hit(talents.close.button);Drag(talents.branchScroll,true);Drag(talents.stateScroll,true);Drag(talents.scroll,false);talents.scroll.verticalNormalizedPosition=1;Capture("talents-top");yield return null;
                foreach(string branch in talents.BranchKeys){talents.SetBranch(branch);yield return null;Check(talents.VisibleCount==(branch=="all"?38:DaoAnProgressionService.Instance.Catalog.nodes.Count(n=>n.branch==branch)),"Talent branch "+branch+" "+size);}
                talents.SetBranch("all");talents.SetStateFilter(3);yield return null;Check(talents.Cards.Where(r=>r.gameObject.activeSelf).All(r=>r.State==ProgressionState.Locked),"Locked talent filter "+size);Capture("talents-locked");yield return null;talents.SetStateFilter(0);Canvas.ForceUpdateCanvases();talents.scroll.StopMovement();talents.scroll.verticalNormalizedPosition=0;yield return null;
                Check(talents.content.rect.height>talents.scroll.viewport.rect.height&&talents.scroll.verticalNormalizedPosition<.01f,"Long talent list actually scrolls "+size);Verify(talents);Capture("talents-bottom");yield return null;
                int count=talents.GetComponentsInChildren<ProgressionCard>(true).Length;nav.Back();yield return null;yield return null;Check(!talents.gameObject.activeInHierarchy,"Back closes talents "+size);nav.OpenRoute(NavigationRoute.Talents);yield return null;yield return null;Check(talents.GetComponentsInChildren<ProgressionCard>(true).Length==count,"Reopen reuses cards without duplicates "+size);talents.close.button.onClick.Invoke();yield return null;yield return null;
                Check(nav.StackDepth==0,"Close button clears progression layer "+size);
            }
            nav.OpenRoute(NavigationRoute.Milestones);yield return null;yield return null;
            var manager=MilestoneManager.Instance;var q=manager.allQuests.Single(x=>x.id=="kill_30");float before=EconomyManager.Instance.currentLinhThach;int fate=EconomyManager.Instance.currentCoDuyen;milestone.FindCard(q.id).action.button.onClick.Invoke();yield return null;
            Check(q.isClaimed&&milestone.FindCard(q.id).State==ProgressionState.Completed,"Claim button completes the real milestone");Check(Mathf.Approximately(EconomyManager.Instance.currentLinhThach,before+q.rewardLinhThach)&&EconomyManager.Instance.currentCoDuyen==fate+q.rewardCoDuyen,"Claim grants exact existing stone and fate rewards");Check(milestone.feedback.text.Contains("Đã nhận"),"Claim feedback visible");
            before=EconomyManager.Instance.currentLinhThach;milestone.FindCard(q.id).action.button.onClick.Invoke();Check(EconomyManager.Instance.currentLinhThach==before,"Completed milestone cannot be claimed twice");before=EconomyManager.Instance.currentLinhThach;milestone.FindCard("kill_80").action.button.onClick.Invoke();Check(EconomyManager.Instance.currentLinhThach==before&&!manager.allQuests.Single(x=>x.id=="kill_80").isClaimed,"Incomplete milestone action cannot grant rewards");
            int claimed=manager.allQuests.Count(x=>x.isClaimed);milestone.primary.button.onClick.Invoke();yield return null;Check(manager.allQuests.Count(x=>x.isClaimed)>claimed&&manager.GetReadyToClaimCount()==0,"Claim all delegates to existing sequential claim command");Check(!milestone.primary.button.interactable,"No rewards left disables Claim All");Capture("milestones-completed");yield return null;
            milestone.talentTab.button.onClick.Invoke();yield return null;yield return null;
            var svc=DaoAnProgressionService.Instance;var node=svc.Catalog.Find("wealth");before=EconomyManager.Instance.currentLinhThach;float price=node.costs[0];talents.FindCard(node.id).action.button.onClick.Invoke();
            Check(svc.GetLevel(node.id)==1&&Mathf.Approximately(EconomyManager.Instance.currentLinhThach,before-price),$"Talent purchase keeps exact service cost and level (level={svc.GetLevel(node.id)}, before={before}, after={EconomyManager.Instance.currentLinhThach}, price={price}, state={talents.FindCard(node.id).State}, enabled={talents.FindCard(node.id).action.button.IsInteractable()}, root={talents.gameObject.activeInHierarchy}, layer={nav.TopLayer})");yield return null;Check(Mathf.Approximately(DaoAnProgressionService.Bonus(node.effect),node.GetTotalBonus(1)),"Talent effects remain catalog effects");Check(talents.FindCard("sword_luck").State==ProgressionState.Available,"Purchase unlocks dependent talent according to core");Check(talents.feedback.text.Contains("Đã nâng"),"Talent upgrade feedback visible");Capture("talent-upgraded");yield return null;
            EconomyManager.Instance.currentLinhThach=0;talents.Refresh();before=svc.GetLevel(node.id);talents.FindCard(node.id).action.button.onClick.Invoke();Check(svc.GetLevel(node.id)==before&&!talents.FindCard(node.id).action.button.interactable,"Insufficient funds cannot purchase talent");Check(talents.FindCard(node.id).State==ProgressionState.InProgress,"Insufficient talent remains in progress");Capture("talent-insufficient");yield return null;
            svc.LoadFromSlot(new CharacterSaveData{daoAn=new DaoAnSaveState{levels=new[]{new DaoAnLevelState{id="wealth",level=2}}}});CultivationManager.Instance.SetStageAndTuViForEditor(0,0);talents.Refresh();Check(talents.FindCard("wealth").State==ProgressionState.Locked&&talents.FindCard("wealth").requirements.text.Contains("Trúc Cơ"),"Next-level realm gate locks an already owned talent correctly");
            svc.LoadFromSlot(new CharacterSaveData{daoAn=new DaoAnSaveState{levels=new[]{new DaoAnLevelState{id="wealth",level=node.MaxLevel}}}});talents.Refresh();Check(talents.FindCard("wealth").State==ProgressionState.Completed&&!talents.FindCard("wealth").action.button.interactable,"Maxed talent is Completed and read-only");talents.SetStateFilter(4);yield return null;Check(talents.VisibleCount==1,"Completed talent filter");Capture("talent-completed");yield return null;talents.SetStateFilter(0);
            // Isolated in-memory catalog clone: stress text and large values without writing the database.
            var original=svc.Catalog;var clone=UnityEngine.Object.Instantiate(original);clone.name="Phase6QA_Catalog_Transient";typeof(DaoAnProgressionService).GetProperty("Catalog").GetSetMethod(true).Invoke(svc,new object[]{clone});var test=clone.Find("wealth");test.displayName="Thiên Phú Huyền Thiên Cửu Chuyển Đạo Vận Vạn Cổ Trường Sinh Bất Diệt Có Tên Rất Dài";test.description=string.Concat(Enumerable.Repeat("Thiên phú nâng đỡ đạo tâm, ghi nhận tiến trình tu luyện và các chỉ số động. ",5));test.costs[0]=1e20f;svc.LoadFromSlot(new CharacterSaveData());EconomyManager.Instance.currentLinhThach=1e25f;talents.Refresh();yield return null;yield return null;Verify(talents);Capture("talent-long-text");yield return null;typeof(DaoAnProgressionService).GetProperty("Catalog").GetSetMethod(true).Invoke(svc,new object[]{original});UnityEngine.Object.Destroy(clone);svc.LoadFromSlot(new CharacterSaveData{daoAn=new DaoAnSaveState{levels=new[]{new DaoAnLevelState{id="wealth",level=1}}}});talents.Refresh();
            talents.milestoneTab.button.onClick.Invoke();yield return null;yield return null;q=manager.allQuests.Single(x=>x.id=="kill_80");string title=q.title,description=q.description;float reward=q.rewardLinhThach;q.title="Mốc Tu Tiên Huyền Thiên Đại Đạo Trường Sinh Vạn Cổ Có Tên Rất Dài Để Kiểm Tra Xuống Dòng";q.description=string.Concat(Enumerable.Repeat("Điều kiện tiến trình hiện có cần được đọc rõ ràng trên màn hình dọc. ",4));q.rewardLinhThach=1e25f;milestone.SetBranch("0");milestone.SetStateFilter(2);milestone.Refresh();yield return null;yield return null;Verify(milestone);Capture("milestone-long-text");yield return null;q.title=title;q.description=description;q.rewardLinhThach=reward;milestone.SetBranch("all");milestone.SetStateFilter(0);
            var state=new CharacterSaveData();svc.SaveToSlot(state);manager.SaveToSlot(state);string levels=JsonUtility.ToJson(state.daoAn);string ids=string.Join(",",state.claimedMilestoneIds.OrderBy(x=>x));DataManager.Instance.SaveCurrentGame(true);DataManager.Instance.LoadData();var disk=DataManager.Instance.gameSaveData.slots[0];svc.LoadFromSlot(disk);manager.LoadFromSlot(disk);var restored=new CharacterSaveData();svc.SaveToSlot(restored);manager.SaveToSlot(restored);Check(JsonUtility.ToJson(restored.daoAn)==levels&&string.Join(",",restored.claimedMilestoneIds.OrderBy(x=>x))==ids,"Milestone and talent persistence round trip without schema changes");Check(Time.timeScale==0,"Progression actions preserve pause ownership");nav.Back();yield return null;yield return null;
        }
        static void Verify(ProgressionScreen v)
        {
            Canvas.ForceUpdateCanvases();foreach(var text in v.GetComponentsInChildren<TMP_Text>(false)){text.ForceMeshUpdate();if(text.overflowMode!=TextOverflowModes.Ellipsis)Check(!text.isTextOverflowing,"No text overflow: "+(v.talents?"talent/":"milestone/")+text.name+" "+Screen.width+"x"+Screen.height);}
            foreach(var row in v.Cards.Where(x=>x.gameObject.activeSelf)){Check(row.progress.Normalized>=0&&row.progress.Normalized<=1,"Progress bounded: "+row.BoundId);Check(row.action.GetComponent<RectTransform>().rect.height>=128,"Primary touch height: "+row.BoundId);}
        }
        static void SurfaceTap(ProgressionScreen view)
        {
            var row=view.Cards.First(r=>r.gameObject.activeSelf);var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            var handler=UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(row.gameObject);
            Check(handler==view.surface.gameObject,"Card surface owns taps instead of legacy backdrop "+Screen.width+"x"+Screen.height);
            UnityEngine.EventSystems.ExecuteEvents.Execute(handler,data,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            Check(view.gameObject.activeInHierarchy&&data.used,"Reading a card does not close progression "+Screen.width+"x"+Screen.height);
        }
        static void Drag(UnityEngine.UI.ScrollRect scroll,bool horizontal)
        {
            Canvas.ForceUpdateCanvases();scroll.StopMovement();var initial=scroll.content.anchoredPosition;
            var point=RectTransformUtility.WorldToScreenPoint(null,scroll.viewport.TransformPoint(scroll.viewport.rect.center));var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point,pressPosition=point,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
            scroll.OnInitializePotentialDrag(data);scroll.OnBeginDrag(data);data.position=point+(horizontal?new Vector2(-180,0):new Vector2(0,180));scroll.OnDrag(data);scroll.OnEndDrag(data);scroll.StopMovement();
            Check((scroll.content.anchoredPosition-initial).sqrMagnitude>1,"Native touch drag moves "+scroll.name+" "+Screen.width+"x"+Screen.height);
            if(horizontal)scroll.horizontalNormalizedPosition=0;else scroll.verticalNormalizedPosition=1;
        }
        static void Hit(UnityEngine.UI.Button b)
        {
            var r=(RectTransform)b.transform;var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);Check(hits.Count>0&&(hits[0].gameObject.transform==r||hits[0].gameObject.transform.IsChildOf(r)),"Close button receives touch "+Screen.width+"x"+Screen.height);
        }
        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));sizeProperty=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);oldSize=(int)sizeProperty.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Phase6 "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);sizes.Add(count-1);sizeProperty.SetValue(view,count-1);view.Focus();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/console-errors.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;
            SessionState.SetBool(Key,false);
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(view!=null&&sizeProperty!=null){sizeProperty.SetValue(view,oldSize);for(int i=sizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{sizes[i]});}sizes.Clear();
            var preservation=new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")};File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(preservation,Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());Debug.Log("[Phase6 QA] Finished. Evidence: "+Evidence);
        }
    }
}
#endif

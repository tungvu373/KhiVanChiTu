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

namespace KhiVanChiTu.UI.SecretRealm.Editor
{
    [InitializeOnLoad]
    public static class RealmQA
    {
        static string Profile=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Profile("realm-phase7-qa");
        const string Key="RealmQA";
        public static string Evidence=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Evidence("Docs/UI_Redesign/Phase7/Evidence");
        static IEnumerator routine;
        static double due;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static EditorWindow view;
        static PropertyInfo sizeProperty;
        static object sizeGroup;
        static int oldSize;
        static readonly List<int> sizes=new List<int>();
        static RealmQA(){EditorApplication.update+=()=>{if(SessionState.GetBool(Key,false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling)Cleanup();};EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))Attach();if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Cleanup;};}
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        public static void ActionsOnly(){Begin();SessionState.SetBool(Key+"ActionsOnly",true);}
        public static void Preview(){Begin();SessionState.SetBool(Key+"Preview",true);}
        public static void Begin(int phase=7)
        {
            EquipmentRedesignBuilder.Require();if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0)throw new Exception("A save profile owner already exists.");
            SessionState.SetBool(Key+"ActionsOnly",false);SessionState.SetBool(Key+"Preview",false);SessionState.SetInt(Key+"Phase",phase);Directory.CreateDirectory(Evidence);if(File.Exists(Evidence+"/failure.txt"))File.Move(Evidence+"/failure.txt",Evidence+"/previous-failure-"+DateTime.UtcNow.Ticks+".txt");checks.Clear();errors.Clear();
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));SessionState.SetFloat(Key+"Scale",Time.timeScale);SessionState.SetBool(Key,true);
            new GameObject("__Phase7_DisposableQA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;
            EditorApplication.isPlaying=true;
        }
        static void Attach(){routine=Run();due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<due)return;
            try{if(!routine.MoveNext()){File.WriteAllText(Evidence+"/checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));Stop();}else due=EditorApplication.timeSinceStartup+.09;}
            catch(Exception e){File.WriteAllText(Evidence+"/failure.txt",e.ToString());Debug.LogException(e);Stop();}
        }
        static void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m+"\n"+stack);}
        static void Check(bool b,string m){if(!b)throw new Exception(m);checks.Add(m);}
        static void Capture(string n){ScreenCapture.CaptureScreenshot(Evidence+"/"+n+"-"+Screen.width+"x"+Screen.height+".png");}

        static T Get<T>(string n) where T:UnityEngine.Object=>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include).First() ;
        static object Field(string name)=>typeof(SecretRealmPlayController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(SecretRealmPlayController.Instance);
        static RectTransform Arena=>(RectTransform)Field("arenaContainer");
        static bool Running=>(bool)Field("isGameRunning");
        static void Hit(UnityEngine.UI.Button button)
        {
            Canvas.ForceUpdateCanvases();var r=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
            var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
            Check(button.IsInteractable(),"Action interactable: "+button.name);Check(hits.Count>0&&(hits[0].gameObject.transform==r||hits[0].gameObject.transform.IsChildOf(r)),"Native touch reaches "+button.name);
            UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        }
        static bool TouchTarget(UnityEngine.UI.Button button)
        {
            Canvas.ForceUpdateCanvases();var r=(RectTransform)button.transform;var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
            if(hits.Count==0||!(hits[0].gameObject.transform==r||hits[0].gameObject.transform.IsChildOf(r)))return false;
            UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);return true;
        }
        static IEnumerable<object> Ready(UnityEngine.UI.Button button)
        {
            for(int attempt=0;attempt<24;attempt++)
            {
                Canvas.ForceUpdateCanvases();var r=(RectTransform)button.transform;
                var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};
                var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
                if(button.IsInteractable()&&hits.Count>0&&(hits[0].gameObject.transform==r||hits[0].gameObject.transform.IsChildOf(r)))yield break;
                yield return null;
            }
            throw new Exception("Touch target never settled: "+button.name+" at "+Screen.width+"x"+Screen.height);
        }
        static void VerifyText(Transform root)
        {
            Canvas.ForceUpdateCanvases();foreach(var t in root.GetComponentsInChildren<TMP_Text>())
            {if(!t.isActiveAndEnabled)continue;t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text fits "+t.name+" "+Screen.width+"x"+Screen.height);}
        }
        static void VerifyPlay(RealmPlayView view)
        {
            VerifyText(view.surface);Check(view.arena.rect.size==new Vector2(384,440),"Logical viewport includes existing spawn coordinates with side gutters");
            var a=view.playArea;var corners=new Vector3[4];view.arena.GetWorldCorners(corners);Check(corners.Select(a.InverseTransformPoint).All(p=>p.x>=a.rect.xMin-2&&p.x<=a.rect.xMax+2&&p.y>=a.rect.yMin-2&&p.y<=a.rect.yMax+2),"Arena fits without covering controls");
            Check(view.playArea.rect.height>view.surface.rect.height*.5f,"Gameplay has majority of screen height");
            Check(((RectTransform)view.exit.transform).rect.height>=132,"Exit touch height");Check(((RectTransform)view.progress.transform).rect.height>=64,"Readable progress height");
            foreach(var btn in view.arena.GetComponentsInChildren<UnityEngine.UI.Button>()){var r=(RectTransform)btn.transform;Check(Mathf.Abs(r.anchoredPosition.x)+r.rect.width/2<=192.1f&&Mathf.Abs(r.anchoredPosition.y)+r.rect.height/2<=220.1f,"Target inside logical arena "+btn.name);}
        }
        static void Drag(UnityEngine.UI.ScrollRect scroll)
        {
            Canvas.ForceUpdateCanvases();scroll.StopMovement();scroll.verticalNormalizedPosition=1;var initial=scroll.content.anchoredPosition;var p=RectTransformUtility.WorldToScreenPoint(null,scroll.viewport.TransformPoint(scroll.viewport.rect.center));var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=p,pressPosition=p,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};scroll.OnInitializePotentialDrag(e);scroll.OnBeginDrag(e);e.position=p+new Vector2(0,200);scroll.OnDrag(e);scroll.OnEndDrag(e);scroll.StopMovement();Check((scroll.content.anchoredPosition-initial).sqrMagnitude>10,"Native drag scrolls realm list");scroll.verticalNormalizedPosition=1;
        }
        static IEnumerable Launch(RealmHubView hub,SecretRealmType type)
        {
            Time.timeScale=0;SecretRealmService.Instance.State.freeAttemptsToday=1;SecretRealmService.Instance.State.lastResetDate=DateTime.UtcNow.ToString("yyyy-MM-dd");SecretRealmModalUI.Instance.Open();hub.Refresh();hub.Focus(type);yield return null;Hit(hub.cards[(int)type].enter.button);yield return null;yield return null;
            Check(Running&&SecretRealmPlayController.Instance.ActiveNavigationLayer!=null,"Launch "+type);Check(SecretRealmService.Instance.State.freeAttemptsToday==0,"Entry charged once "+type);
        }
        static IEnumerator Run()
        {
            while(DataManager.Instance==null||MainMenuManager.Instance==null)yield return null;
            Check(DataManager.Instance.SaveProfileId==Profile,"Disposable secret realm profile active");
            var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Đạo Hữu Bí Cảnh",linhThach=100000,coDuyen=100,stageIndex=15,combatStage=5,playerMaxHp=50000,playerCurrentHp=50000,monsterSwordsAcquired=600,swords=new[]{new SwordSaveEntry{level=5,pieces=1,isEquipped=true,equipSlotIndex=0}}};
            DataManager.Instance.gameSaveData.slots[0]=slot;MainMenuManager.Instance.AskConfirmEnterGame(0,slot);yield return null;MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();yield return null;yield return null;yield return null;
            int frame=Time.frameCount;yield return null;Check(Time.frameCount>frame,"Game loop advances");Time.timeScale=0;
            var nav=GlobalNavigationController.Instance;var hub=Get<RealmHubView>("");var play=Get<RealmPlayView>("");var result=Get<RealmResultView>("");var service=SecretRealmService.Instance;
            SetupSizes();SelectSize(new Vector2Int(1080,1920));nav.SelectGroup(NavigationGroup.SecretRealm);yield return null;nav.OpenRoute(NavigationRoute.RealmList);yield return null;yield return null;
            if(SessionState.GetBool(Key+"Preview",false)){while(true)yield return null;}
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;SecretRealmModalUI.Instance.Open();hub.Refresh();yield return null;Canvas.ForceUpdateCanvases();
                Check(hub.cards.Length==5&&hub.cards.All(c=>c.artwork.sprite!=null),"Five illustrated realm cards "+size);VerifyText(hub.surface);Drag(hub.scroll);Capture("realm-list-top");yield return null;
                hub.Focus(SecretRealmType.SwordTomb);yield return null;Capture("realm-list-bottom");yield return null;
                foreach(SecretRealmType type in Enum.GetValues(typeof(SecretRealmType)))
                {
                    foreach(var x in Launch(hub,type))yield return x;VerifyPlay(play);Check(errors.Count==0,"No runtime errors after launch "+type);
                    if(type==SecretRealmType.HordeSurvival)
                    {
                        // Auto damage can remove every initial target even while timeScale is zero.
                        // Spawn one through the unchanged core function to verify a live mask and native tap.
                        SecretRealmPlayController.Instance.enabled=false;
                        typeof(SecretRealmPlayController).GetMethod("SpawnHordeDemon",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(SecretRealmPlayController.Instance,null);
                        typeof(RealmPlayView).GetMethod("StyleArena",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(play,null);
                        var target=Arena.GetChild(Arena.childCount-1).GetComponent<UnityEngine.UI.Button>();yield return null;Check(target.GetComponentsInChildren<RealmDemonIcon>().Length==1&&target.GetComponentInChildren<RealmDemonIcon>().GetComponent<CanvasRenderer>()!=null,"Live demon mask has renderer");int kills=(int)Field("hordeKilled");Hit(target);Check((int)Field("hordeKilled")==kills+1,"Native demon tap preserves original kill callback");SecretRealmPlayController.Instance.enabled=true;
                    }
                    if(type==SecretRealmType.SwordTomb){var image=Arena.Find("FloatingSword").GetComponent<UnityEngine.UI.Image>();Check(image.sprite.name=="kiem_7"&&image.overrideSprite==play.swordArt[2],"Transparent sword retains original sprite selection");}
                    Capture("play-"+type);yield return null;
                    int cleared=service.State.totalCleared;foreach(var ready in Ready(play.exit.button))yield return ready;Hit(play.exit.button);yield return null;Check(!Running&&SecretRealmModalUI.Instance.IsModalOpen&&service.State.totalCleared==cleared,"Exit without reward "+type+" "+size);
                }
            }
            SelectSize(new Vector2Int(1080,1920));yield return null;
            // Native hub admission: no free entry, insufficient funds, paid entry, daily reset.
            service.State.lastResetDate=DateTime.UtcNow.ToString("yyyy-MM-dd");service.State.freeAttemptsToday=0;EconomyManager.Instance.currentCoDuyen=4;hub.Refresh();Check(hub.cards.All(c=>!c.enter.button.interactable),"Insufficient resource buttons disabled");
            SecretRealmModalUI.Instance.EnterRealm(SecretRealmType.HordeSurvival);Check(!Running&&EconomyManager.Instance.currentCoDuyen==4,"Insufficient entry blocked by original command");
            EconomyManager.Instance.currentCoDuyen=10;hub.Refresh();hub.Focus(SecretRealmType.HordeSurvival);yield return null;Hit(hub.cards[0].enter.button);yield return null;Check(EconomyManager.Instance.currentCoDuyen==5,"Paid entry deducts exactly 5 CD");foreach(var ready in Ready(play.exit.button))yield return ready;Hit(play.exit.button);yield return null;
            service.State.lastResetDate=DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");hub.Refresh();Check(service.State.freeAttemptsToday==1,"Original daily free reset retained");
            // Success via existing gameplay input. Horde uses normal spawning and clock, no score injection.
            foreach(var x in Launch(hub,SecretRealmType.HordeSurvival))yield return x;Time.timeScale=1;float start=Time.realtimeSinceStartup;
            while(Running&&Time.realtimeSinceStartup-start<35){var button=play.actionHost.GetComponentInChildren<UnityEngine.UI.Button>();if(button!=null&&button.IsInteractable())Hit(button);yield return null;}
            Check(!Running&&(int)Field("hordeKilled")==50,"Horde real spawning and action reach 50 before timeout");Time.timeScale=0;yield return null;Check(result.status.text.Contains("HOÀN THÀNH"),"Horde victory presentation");VerifyText(result.transform);Capture("victory-horde");yield return null;int clearedOnce=service.State.totalCleared;Hit(result.confirm.button);yield return null;Check(service.State.totalCleared==clearedOnce,"Complete result does not grant rewards twice");
            foreach(var x in Launch(hub,SecretRealmType.BarrierBreak))yield return x;for(int i=0;i<20&&Running;i++)Hit(play.actionHost.GetComponentInChildren<UnityEngine.UI.Button>());yield return null;Check(!Running&&(int)Field("currentScore")==100,"Barrier win from original 20 attack inputs");Check(result.status.text.Contains("HOÀN THÀNH"),"Barrier victory");Hit(result.confirm.button);yield return null;
            // Wrong and correct cauldron choices derived from actual hint, preserving RNG.
            foreach(bool win in new[]{false,true})
            {
                foreach(var x in Launch(hub,SecretRealmType.DanCauldron))yield return x;string riddle=play.objective.text;int winner=riddle.Contains("TẢ")?0:riddle.Contains("TRUNG TÂM")?1:2;int select=win?winner:(winner+1)%3;
                Hit(Arena.Find("Cauldron_"+select).GetComponent<UnityEngine.UI.Button>());play.Refresh();Check(play.score.text.Contains(win?"CỰC PHẨM":"THƯỢNG PHẨM")&&play.directAction.text.Contains("Đã khai đỉnh"),"Immediate choice feedback "+win);float deadline=Time.realtimeSinceStartup+4;while(Running&&Time.realtimeSinceStartup<deadline)yield return null;
                Check(!Running&&(int)Field("currentScore")== (win?1:0),"Cauldron "+(win?"correct":"wrong")+" native input");Check(result.status.text.Contains(win?"CỰC PHẨM":"DUYÊN THƯỜNG"),"Accurate cauldron outcome");Capture(win?"cauldron-win":"cauldron-common");yield return null;Hit(result.confirm.button);yield return null;
            }
            foreach(var x in Launch(hub,SecretRealmType.LightningDodge))yield return x;Time.timeScale=1;int spins=0;
            while(Running&&spins++<100){foreach(var b in Arena.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.name.StartsWith("ThunderOrb")).ToArray())if(b!=null)TouchTarget(b);yield return null;}
            Check(!Running&&(int)Field("orbsCollected")==20,"Lightning native orb input reaches 20");Check(result.status.text.Contains("HOÀN THÀNH"),"Lightning victory");Hit(result.confirm.button);yield return null;
            // Sword lower outcome: three native taps outside the central bands, no needle position injection.
            foreach(bool gold in new[]{false,true})
            {
                foreach(var x in Launch(hub,SecretRealmType.SwordTomb))yield return x;Time.timeScale=1;float limit=Time.realtimeSinceStartup+16;
                while(Running&&Time.realtimeSinceStartup<limit)
                {var needle=(RectTransform)Field("needleTransform");var button=play.actionHost.GetComponentInChildren<UnityEngine.UI.Button>();float x=Mathf.Abs(needle.anchoredPosition.x);if(button!=null&&button.IsInteractable()&&(gold?x<=20:x>=110))Hit(button);yield return null;}
                Check(!Running&&(int)Field("currentScore")== (gold?8:4),"Sword "+(gold?"gold window":"three outer taps")+" uses original moving needle");Time.timeScale=0;yield return null;VerifyText(result.transform);Capture(gold?"sword-perfect":"sword-common");yield return null;Hit(result.confirm.button);yield return null;
            }
            // Real unmodified timers: passive barrier (20s) and no-input lightning (25s).
            foreach(var type in new[]{SecretRealmType.BarrierBreak,SecretRealmType.LightningDodge})
            {
                foreach(var x in Launch(hub,type))yield return x;Time.timeScale=1;float initial=(float)Field("timeRemaining"),clock=Time.realtimeSinceStartup;
                bool warning=false;while(Running&&Time.realtimeSinceStartup-clock<initial+4){if(!warning&&(float)Field("timeRemaining")<4.5f){play.Refresh();Check(play.timer.color==play.theme.danger,"Last-five-seconds warning parses current decimal culture "+type);warning=true;}yield return null;}Check(warning,"Timer warning reached "+type);
                float elapsed=Time.realtimeSinceStartup-clock;Check(!Running&&Mathf.Abs(elapsed-initial)<2,"Unmodified real timer expires "+type);Time.timeScale=0;yield return null;
                Check(result.status.text.Contains("HẾT THỜI GIAN"),"Timeout presentation "+type);Check(type==SecretRealmType.BarrierBreak?result.VisibleRewardCount>0:result.VisibleRewardCount==0,"Original timeout reward outcome retained "+type);VerifyText(result.transform);Capture("timeout-"+type);yield return null;Hit(result.confirm.button);yield return null;
            }
            // Presentation refresh must not consume RNG.
            // Horde's timeout branch from a near-expiry QA checkpoint, without changing timer code.
            foreach(var x in Launch(hub,SecretRealmType.HordeSurvival))yield return x;typeof(SecretRealmPlayController).GetField("timeRemaining",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SecretRealmPlayController.Instance,.01f);Time.timeScale=1;yield return null;yield return null;Check(!Running&&result.status.text.Contains("HẾT THỜI GIAN"),"Horde timeout branch from isolated near-expiry checkpoint");Capture("timeout-HordeSurvival-checkpoint");yield return null;Time.timeScale=0;Hit(result.confirm.button);yield return null;
            var random=JsonUtility.ToJson(UnityEngine.Random.state);hub.Refresh();Check(random==JsonUtility.ToJson(UnityEngine.Random.state),"Hub presentation consumes no RNG");
            // Display-only stress data, never written to the service/catalog.
            SecretRealmModalUI.Instance.Open();hub.Focus(SecretRealmType.HordeSurvival);string oldTitle=hub.cards[0].title.text,oldDesc=hub.cards[0].description.text;
            hub.cards[0].title.text="Vạn Ma Triều Dâng · Thử Luyện Thượng Cổ Giữa Động Thiên Huyền Bí";hub.cards[0].description.text=string.Join(" ",Enumerable.Repeat("Yêu ma vây kín động thiên. Triệu vạn kiếm, quét sạch ma triều.",5));yield return null;yield return null;VerifyText(hub.surface);Check(((RectTransform)hub.cards[0].transform).rect.height>660,"Realm card grows for long text");Capture("realm-long-text");yield return null;hub.cards[0].title.text=oldTitle;hub.cards[0].description.text=oldDesc;
            result.Show(SecretRealmType.SwordTomb,8,new SecretRealmService.RewardResult{linhThach=1e25f,tuVi=1e20f,herbs0=int.MaxValue,herbs1=9999999,herbs2=99999,ore=9999999,bonusCoDuyen=int.MaxValue,swordLevel=8,specialNote=string.Join(" ",Enumerable.Repeat("Bảo vật đã được ghi nhận trong thử nghiệm hiển thị.",5))},()=>SecretRealmPlayController.Instance.CloseAll());yield return null;yield return null;VerifyText(result.transform);Check(result.VisibleRewardCount==8,"All eight reward categories displayed");Drag(result.scroll);Capture("reward-large-values");yield return null;Hit(result.confirm.button);yield return null;
            nav.Back();yield return null;yield return null;Check(!SecretRealmModalUI.Instance.IsModalOpen,"Navigation Back closes realm hub");nav.OpenRoute(NavigationRoute.SwordTomb);yield return null;yield return null;Check(SecretRealmModalUI.Instance.IsModalOpen,"Direct navigation returns to realm list");
            DataManager.Instance.SaveCurrentGame(true);var saved=service.State.Clone();service.State.totalCleared=0;DataManager.Instance.LoadData();DataManager.Instance.LoadCurrentGame();yield return null;Check(service.State.totalCleared==saved.totalCleared&&service.State.swordTombMaxLevel==saved.swordTombMaxLevel&&service.State.cauldronPillsFound==saved.cauldronPillsFound,"Secret realm save load round trip");
            Check(Time.timeScale==0,"UI retains pause ownership");int cards=hub.cards.Length;SecretRealmModalUI.Instance.Close();SecretRealmModalUI.Instance.Open();Check(hub.cards.Length==cards,"Reopen does not duplicate realm cards");Check(errors.Count==0,"No runtime console errors in full QA");
        }
        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));sizeProperty=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);oldSize=(int)sizeProperty.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Phase7 "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);sizes.Add(count-1);sizeProperty.SetValue(view,count-1);view.Focus();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/console-errors.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;
            SessionState.SetBool(Key,false);
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(view!=null&&sizeProperty!=null){sizeProperty.SetValue(view,oldSize);for(int i=sizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{sizes[i]});}sizes.Clear();
            var preservation=new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")};File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(preservation,Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());Debug.Log("[Phase7 QA] Finished. Evidence: "+Evidence);
        }
    }
}
#endif

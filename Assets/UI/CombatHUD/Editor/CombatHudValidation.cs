using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KhiVanChiTu.Collaboration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.UI.CombatHUD.Editor
{
    [InitializeOnLoad]
    public static class CombatHudValidation
    {
        const string Profile="hud-phase2-qa";
        const string Key="CombatHudP2QA.";
        static readonly Vector2Int[] Sizes={new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)};
        static readonly List<object> checks=new List<object>();
        static readonly List<object> errors=new List<object>();
        static readonly List<DamagePopup> stressPopups=new List<DamagePopup>();
        static readonly List<int> added=new List<int>();
        static EditorWindow view;
        static PropertyInfo selected;
        static object group;
        static CombatHudPresentation hud;
        static int phase,index,frame,previousSize,navigationIndex;
        static double readyAt,timeout;
        static float previousScale;
        static bool running;
        static CombatHudValidation(){EditorApplication.playModeStateChanged+=OnState;}
        static void OnState(PlayModeStateChange state)
        {if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"Active",false))EditorApplication.delayCall+=Cleanup;}
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string FileHash(string path)=>File.Exists(path)?Hash(Convert.ToBase64String(File.ReadAllBytes(path))):"absent";
        static string ProductionFile=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        [MenuItem("TuTien/UI Combat HUD/2. Validate with Isolated Profile")]
        public static void Start()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!=CombatHudBuilder.ScenePath||scene.isDirty)throw new InvalidOperationException("Open saved mobile HUD scene in Edit Mode first.");
            if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include).Length!=0)throw new InvalidOperationException("Existing save-profile owner found. Keep it intact.");
            Directory.CreateDirectory(CombatHudBuilder.Evidence);checks.Clear();added.Clear();errors.Clear();Application.logMessageReceived+=CollectLog;
            SessionState.SetString(Key+"Production",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));
            SessionState.SetString(Key+"ProductionFile",FileHash(ProductionFile));SessionState.SetString(Key+"ProductionBak",FileHash(ProductionFile+".bak"));
            SessionState.SetFloat(Key+"Scale",Time.timeScale);previousScale=Time.timeScale;
            SessionState.SetBool(Key+"Active",true);
            var marker=new GameObject("__CombatHUD_Phase2_QA_Temporary");marker.AddComponent<WorkspaceSaveProfile>().profileId=Profile;
            phase=0;index=0;running=true;readyAt=EditorApplication.timeSinceStartup+1;timeout=readyAt+45;
            EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
        }
        static void RequireIsolation()
        {Expect(DataManager.Instance!=null&&DataManager.Instance.SaveProfileId==Profile,"QA must use the disposable HUD profile.");}
        static void Expect(bool condition,string reason){if(!condition)throw new InvalidOperationException(reason);}
        static void Wait(int next,double seconds=.25){phase=next;frame=Time.frameCount;readyAt=EditorApplication.timeSinceStartup+seconds;timeout=readyAt+25;}
        static void Tick()
        {
            if(!running)return;
            try
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if(EditorApplication.timeSinceStartup>timeout)throw new TimeoutException("HUD validation timed out at phase "+phase);
                if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<readyAt)return;
                if(phase==0)
                {
                    if(DataManager.Instance==null||MainMenuManager.Instance==null)return;RequireIsolation();
                    hud=UnityEngine.Object.FindAnyObjectByType<CombatHudPresentation>(FindObjectsInactive.Include);Expect(hud!=null,"Presentation missing");
                    var slot=new CharacterSaveData{isSlotEmpty=false,characterName="V\u0169 Kh\u00e1nh T\u00f9ng \u2014 Thi\u00ean V\u00e2n \u0110\u1ea1o H\u1eefu Tr\u01b0\u1eddng Sinh",linhThach=1234567890,coDuyen=123456,stageIndex=30,realmId=5,subLevel=1,combatStage=9,playerMaxHp=10000,playerCurrentHp=10000,skillActiveUnlocked=true,skillNormal0Unlocked=true,skillNormal1Unlocked=true,skillNormal2Unlocked=true,swordStyleUnlocked=true,monsterSwordsAcquired=600};
                    slot.formations.levels=new[]{1,1,1,1,1,1};slot.swords=new[]{new SwordSaveEntry{level=7,pieces=1,isEquipped=true,equipSlotIndex=0}};
                    DataManager.Instance.gameSaveData.slots[0]=slot;MainMenuManager.Instance.AskConfirmEnterGame(0,slot);Wait(1);return;
                }
                RequireIsolation();
                if(phase==1){MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();Wait(2,2);return;}
                if(phase==2)
                {
                    Expect(Time.frameCount>frame,"Game loop did not advance");Time.timeScale=0;
                    var combat=CombatManager.Instance;
                    // Additional real enemy instances are a QA fixture only; production spawn settings are not edited.
                    for(int i=0;i<8;i++)
                    {
                        var go=UnityEngine.Object.Instantiate(combat.normalEnemyPrefabs[0],combat.playerTransform.position+new Vector3((i%4-1.5f)*1.5f,0,3+(i/4)*1.6f),Quaternion.identity);
                        go.GetComponent<Enemy>().Init(100000,combat.playerTransform,false);
                    }
                    SetupViewSizes();SelectSize(Sizes[0]);Wait(3);return;
                }
                if(Screen.width!=Sizes[index].x||Screen.height!=Sizes[index].y||Time.frameCount<frame+5)return;
                if(phase==3)
                {
                    if(hud.IsCompact)hud.drawer.ToggleDockCollapse();
                    var c=CombatManager.Instance;var sm=SkillManager.Instance;c.playerCurrentHp=c.playerMaxHp;sm.currentLinhLuc=sm.maxLinhLuc*.9f;sm.isSkillActive=false;c.DeactivateVanKiemQuyTong();
                    typeof(SkillManager).GetField("normalSkillGlobalCooldown",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(sm,100f);
                    StageManager.Instance.currentStage=9;StageManager.Instance.isBossStage=false;
                    if(UnityEngine.Object.FindObjectsByType<Enemy>().Count(e=>!e.isDead)<8)
                        for(int i=0;i<8;i++){var enemy=UnityEngine.Object.Instantiate(c.normalEnemyPrefabs[0],c.playerTransform.position+new Vector3((i%4-1.5f)*1.5f,0,3+(i/4)*1.6f),Quaternion.identity);enemy.GetComponent<Enemy>().Init(100000,c.playerTransform,false);}
                    SetCooldowns(0);hud.Refresh();hud.ApplyLayout();Wait(4);return;
                }
                if(phase==4)
                {
                    Geometry();CheckCenter();Expect(hud.ClearHeightFraction>.58f,"Full HUD leaves too little gameplay height");
                    Expect(UnityEngine.Object.FindObjectsByType<Enemy>().Count(e=>!e.isDead)>=8,"Crowd fixture did not contain multiple enemies");
                    Expect(Mathf.Abs(hud.waveProgress.Normalized-Mathf.Clamp01((float)StageManager.Instance.enemiesKilled/StageManager.Instance.enemiesToBoss))<.001f,"Wave progress binding mismatch");
                    Record("full",new{clearFraction=hud.ClearHeightFraction,enemyCount=UnityEngine.Object.FindObjectsByType<Enemy>().Count(e=>!e.isDead)});Capture("full");Wait(5);return;
                }
                if(phase==5)
                {
                    CombatManager.Instance.playerCurrentHp=CombatManager.Instance.playerMaxHp*.09f;SkillManager.Instance.currentLinhLuc=0;hud.Refresh();Wait(6);return;
                }
                if(phase==6)
                {
                    Expect(Mathf.Abs(hud.hp.Normalized-.09f)<.001f&&hud.mp.Normalized==0,"HP/MP binding mismatch");
                    Expect(hud.skillViews.Skip(1).All(v=>!v.button.interactable),"0 MP did not disable skills");
                    Record("low-hp-zero-mp",new{hp=hud.hp.Normalized,mp=hud.mp.Normalized});Capture("low-hp-zero-mp");Wait(7);return;
                }
                if(phase==7){SkillManager.Instance.currentLinhLuc=SkillManager.Instance.maxLinhLuc*.9f;SetCooldowns(12);hud.Refresh();Wait(8);return;}
                if(phase==8)
                {
                    Expect(hud.skillViews.Skip(1).All(v=>!v.button.interactable&&v.cooldown.fillAmount>0),"Cooldown state mismatch");
                    Record("cooldown",new{cooldown=SkillManager.Instance.GetNormalSkillCooldown(0)});Capture("cooldown");hud.drawer.ToggleDockCollapse();Wait(9,.4);return;
                }
                if(phase==9)
                {
                    Expect(hud.IsCompact&&hud.ClearHeightFraction>.7f,"Compact state or clear area");Expect(hud.hp.gameObject.activeInHierarchy&&hud.mp.gameObject.activeInHierarchy&&hud.legacy.skills.gameObject.activeInHierarchy,"Compact hid vital combat controls");
                    Geometry();Record("compact",new{clearFraction=hud.ClearHeightFraction});Capture("compact");Wait(10);return;
                }
                if(phase==10)
                {
                    hud.drawer.ToggleDockCollapse();var s=StageManager.Instance;s.currentStage=10;s.isBossStage=true;s.bossTimer=17.2f;s.UpdateUI();CombatManager.Instance.ChangeToBossState();hud.ApplyLayout();hud.Refresh();Wait(11,.4);return;
                }
                if(phase==11)
                {
                    Expect(hud.boss.gameObject.activeInHierarchy&&hud.timer.text=="18s","Boss timer presentation mismatch");
                    Expect(Mathf.Abs(StageManager.Instance.bossTimer-17.2f)<.001f,"HUD mutated paused battle timer");
                    Expect(hud.bossHealth.Normalized>0,"Boss HP binding empty");
                    Record("boss",new{timer=StageManager.Instance.bossTimer,boss=CombatManager.Instance.CurrentBoss!=null});Capture("boss");Wait(111);return;
                }
                if(phase==111){CombatManager.Instance.CurrentBoss.hp=CombatManager.Instance.CurrentBoss.maxHp=1e20f;Time.timeScale=1;Wait(112,.25);return;}
                if(phase==112){Expect(StageManager.Instance.bossTimer<17.2f&&StageManager.Instance.bossTimer>0,"Live battle timer did not advance");Time.timeScale=0;Record("live-timer",new{remaining=StageManager.Instance.bossTimer});Wait(12);return;}
                if(phase==12)
                {
                    StageManager.Instance.isBossStage=false;CombatManager.Instance.ChangeToIdleState();SetCooldowns(0);
                    // Display stress uses the real damage popup prefab/pool contract; no damage is applied to a target.
                    stressPopups.Clear();var c=CombatManager.Instance;for(int i=0;i<12;i++){var go=UnityEngine.Object.Instantiate(c.damagePopupPrefab,c.playerTransform.position+new Vector3(0,2,2),Quaternion.identity);var popup=go.GetComponent<DamagePopup>();popup.Setup(12345+i*100,i%3==0);stressPopups.Add(popup);}
                    hud.ApplyLayout();Wait(13,.12);return;
                }
                if(phase==13)
                {
                    PopupGeometry();Record("popup-stress",new{popups=stressPopups.Count,overlappingPairs=0});Capture("popup-stress");Wait(14,.1);return;
                }
                if(phase==14)
                {
                    if(index+1<Sizes.Length){index++;SelectSize(Sizes[index]);Wait(3);return;}
                    SkillCallbacks();hud.legacy.menuButton.onClick.Invoke();Wait(20);return;
                }
                if(phase==20)
                {
                    Expect(hud.legacy.IsSecondaryOpen&&hud.legacy.quick.gameObject.activeInHierarchy,"Navigation menu did not expose original shortcuts");
                    Record("navigation",new{shortcuts=hud.legacy.quickButtons.Length});Capture("navigation");Wait(201);return;
                }
                if(phase==201){hud.legacy.closeMenuButton.onClick.Invoke();navigationIndex=0;Wait(21);return;}
                if(phase==21){hud.legacy.quickButtons[navigationIndex].onClick.Invoke();Wait(22,.4);return;}
                if(phase==22)
                {
                    switch(navigationIndex)
                    {
                        case 0:Expect(StorageModalUI.Instance.IsModalOpen,"Storage shortcut failed");StorageModalUI.Instance.Close();break;
                        case 1:Expect(SecretRealmModalUI.Instance.IsModalOpen,"Secret realm shortcut failed");SecretRealmModalUI.Instance.Close();break;
                        case 2:Expect(DaoAnModalUI.Instance.IsModalOpen,"Talent shortcut failed");DaoAnModalUI.Instance.Close();break;
                        case 3:Expect(MilestoneModalUI.Instance.IsModalOpen,"Milestone shortcut failed");MilestoneModalUI.Instance.Close();break;
                    }
                    Record("shortcut",new{button=hud.legacy.quickButtons[navigationIndex].name});navigationIndex++;
                    if(navigationIndex<hud.legacy.quickButtons.Length)Wait(21,.4);else{navigationIndex=0;Wait(23,.4);}return;
                }
                if(phase==23){hud.drawer.dockButtons[navigationIndex].onClick.Invoke();Wait(24,.4);return;}
                if(phase==24)
                {
                    Expect(hud.drawer.ActiveDrawerIndex==navigationIndex,"Original dock route failed: "+navigationIndex);Record("dock",new{index=navigationIndex});
                    hud.drawer.CloseActiveDrawer();Wait(25,.4);return;
                }
                if(phase==25)
                {
                    navigationIndex++;if(navigationIndex<hud.drawer.dockButtons.Length)Wait(23);else{hud.pause.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Wait(26,.4);}return;
                }
                if(phase==26)
                {
                    Expect(PauseMenuManager.Instance.IsModalOpen,"Pause route failed");PauseMenuManager.Instance.resumeButton.onClick.Invoke();Wait(27,.4);return;
                }
                if(phase==27)
                {
                    Expect(!PauseMenuManager.Instance.IsModalOpen,"Resume route failed");Record("pause-resume",new{closed=true});
                    Expect(Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Production",""),"Production prefs changed");
                    File.WriteAllText(CombatHudBuilder.Evidence+"/runtime-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
                    Debug.Log("[Combat HUD] Three-resolution isolated fixtures passed. Stopping QA and checking production save hashes.");Stop();
                }
            }
            catch(Exception e){File.WriteAllText(CombatHudBuilder.Evidence+"/failure.txt",e.ToString());Debug.LogException(e);Stop();}
        }
        static void SetCooldowns(float seconds)
        {
            var sm=SkillManager.Instance;typeof(SkillManager).GetField("normalCooldowns",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(sm,new[]{seconds,seconds,seconds});
            typeof(SkillManager).GetField("currentCooldown",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(sm,seconds);
        }
        static void CollectLog(string message,string stack,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(new{type=type.ToString(),message,stack});}
        static void SkillCallbacks()
        {
            var sm=SkillManager.Instance;sm.isSkillActive=false;CombatManager.Instance.DeactivateVanKiemQuyTong();
            for(int i=0;i<3;i++)
            {
                sm.currentLinhLuc=sm.maxLinhLuc*.9f;SetCooldowns(0);float before=sm.currentLinhLuc,cost=sm.GetSkillManaCost(i);
                hud.skillViews[i+1].button.onClick.Invoke();
                Expect(Mathf.Abs(sm.currentLinhLuc-(before-cost))<.01f&&sm.GetNormalSkillCooldown(i)>0,"Original skill callback/cost failed: "+i);
            }
            SetCooldowns(0);sm.currentLinhLuc=sm.maxLinhLuc;hud.skillViews[0].button.onClick.Invoke();Expect(sm.isSkillActive,"Original ultimate callback failed");
            Record("skill-callbacks",new{normal=3,ultimate=true});
        }
        static void Capture(string name){ScreenCapture.CaptureScreenshot(CombatHudBuilder.Evidence+"/"+name+"-"+Screen.width+"x"+Screen.height+".png");}
        static void Record(string name,object details){checks.Add(new{name,details,width=Screen.width,height=Screen.height,frame=Time.frameCount,profile=DataManager.Instance.SaveProfileId});}
        static void CheckCenter()
        {
            var pointer=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Expect(!hits.Any(h=>h.gameObject.GetComponentInParent<CombatHudPresentation>()!=null),"HUD intercepts gameplay center");
        }
        static void Geometry()
        {
            Canvas.ForceUpdateCanvases();
            foreach(var v in hud.skillViews){var r=(RectTransform)v.button.transform;Expect(r.rect.width>=120&&r.rect.height>=120,"Skill target too small in reference space");}
            foreach(var t in hud.GetComponentsInChildren<TMPro.TMP_Text>(false))if(t.overflowMode!=TMPro.TextOverflowModes.Ellipsis){t.ForceMeshUpdate();Expect(!t.isTextOverflowing,"Text overflow: "+t.name+" / "+t.text);}
            Expect(hud.hp.fill.rect.width>0&&hud.mp.fill.rect.width>0,"Vital fill has zero size");
            foreach(var bar in new[]{hud.hp,hud.mp,hud.waveProgress,hud.bossHealth,hud.cultivation})Expect(bar.fill.parent.GetComponent<RectTransform>().rect.height>0,"Progress track has zero height: "+bar.name);
        }
        static void PopupGeometry()
        {
            var camera=Camera.main;var rects=new List<Rect>();
            foreach(var popup in stressPopups)
            {
                var label=popup.GetComponentInChildren<TMPro.TextMeshPro>();Expect(label!=null,"Readable damage label missing");
                var bounds=label.renderer.bounds;Vector2 min=new Vector2(float.PositiveInfinity,float.PositiveInfinity),max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                for(int i=0;i<8;i++){var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));Vector2 point=camera.WorldToScreenPoint(corner);min=Vector2.Min(min,point);max=Vector2.Max(max,point);}
                rects.Add(Rect.MinMaxRect(min.x,min.y,max.x,max.y));
            }
            for(int i=0;i<rects.Count;i++)for(int j=i+1;j<rects.Count;j++)Expect(!rects[i].Overlaps(rects[j]),"Damage popup labels overlap: "+i+" / "+j);
        }
        static void SetupViewSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
            var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(sizes);group=type.GetMethod("GetGroup").Invoke(sizes,new[]{gt});
            view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));selected=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);previousSize=(int)selected.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Combat HUD QA "+size.x+"x"+size.y},null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{entry});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);added.Add(count-1);selected.SetValue(view,count-1);view.Focus();
        }
        static void Stop(){running=false;EditorApplication.update-=Tick;Application.logMessageReceived-=CollectLog;File.WriteAllText(CombatHudBuilder.Evidence+"/qa-console.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));Time.timeScale=previousScale;EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            if(EditorApplication.isPlaying)return;
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);
            if(view!=null&&selected!=null){selected.SetValue(view,previousSize);for(int i=added.Count-1;i>=0;i--)group.GetType().GetMethod("RemoveCustomSize").Invoke(group,new object[]{added[i]});}added.Clear();
            bool prefs=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Production","");
            bool file=FileHash(ProductionFile)==SessionState.GetString(Key+"ProductionFile","");bool bak=FileHash(ProductionFile+".bak")==SessionState.GetString(Key+"ProductionBak","");
            File.WriteAllText(CombatHudBuilder.Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{productionPrefsUnchanged=prefs,productionJsonUnchanged=file,productionBackupUnchanged=bak,profileMarkerRemoved=true},Newtonsoft.Json.Formatting.Indented));
            SessionState.SetBool(Key+"Active",false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            if(!prefs||!file||!bak)Debug.LogError("[Combat HUD] Production save preservation check failed.");else Debug.Log("[Combat HUD] QA marker removed; production save hashes unchanged.");
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using KhiVanChiTu.Collaboration;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Progression;

namespace KhiVanChiTu.UI.Equipment.Editor
{
    [InitializeOnLoad]
    public static class SwordCapQA
    {
        const string Key="SwordCapQA",Profile="sword-cap10-qa",Evidence="Docs/UI_Redesign/SwordCap10";
        static IEnumerator routine;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static double due,deadline;
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        static SwordCapQA()
        {
            EditorApplication.playModeStateChanged+=s=>{
                if(!SessionState.GetBool(Key,false))return;
                if(s==PlayModeStateChange.EnteredPlayMode){routine=Run();due=EditorApplication.timeSinceStartup+2;deadline=due+100;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
                if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Cleanup;
            };
        }
        public static void Begin()
        {
            EquipmentRedesignBuilder.Require();
            if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include).Length>0)throw new Exception("Save profile already owned.");
            Directory.CreateDirectory(Evidence);checks.Clear();errors.Clear();SessionState.SetBool(Key,true);
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));
            SessionState.SetBool(Key+"Dirty",UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);SessionState.SetFloat(Key+"Scale",Time.timeScale);
            new GameObject("__SwordCapQA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;
            EditorApplication.isPlaying=true;
        }
        static void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m+"\n"+stack);}
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<due)return;
            try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("QA deadline");if(!routine.MoveNext()){Stop();return;}due=EditorApplication.timeSinceStartup+.25;}
            catch(Exception e){errors.Add(e.ToString());Stop();}
        }
        static void Check(bool value,string message){if(!value)throw new Exception(message);checks.Add(message);}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=errors.Count==0,checks,errors},Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Clear()
        {
            foreach(var item in MergeManager.Instance.GetAllSwords())UnityEngine.Object.DestroyImmediate(item.gameObject);
        }
        static MergeItem Add(int level,int pieces=1,bool equip=false)
        {
            var m=MergeManager.Instance;var before=m.GetAllSwords();Check(m.TryAddSword(level),"Add sword level "+level);
            var item=m.GetAllSwords().Single(s=>!before.Contains(s));item.SetLevel(level,pieces);
            if(equip)item.transform.SetParent(m.equipPanel.GetChild(0),false);return item;
        }
        static void Drop(MergeItem target,MergeItem source)
        {source.originalParent=source.transform.parent;target.GetComponentInParent<MergeSlot>(true).OnDrop(new PointerEventData(EventSystem.current){pointerDrag=source.gameObject});}
        static IEnumerator Run()
        {
            while(DataManager.Instance==null||MainMenuManager.Instance==null)yield return null;
            Check(DataManager.Instance.SaveProfileId==Profile,"Isolated QA profile");
            var state=new CharacterSaveData{isSlotEmpty=false,characterName="Kiểm kiếm cấp 10",linhThach=100000,stageIndex=15,combatStage=5,playerMaxHp=50000,playerCurrentHp=50000,monsterSwordsAcquired=600,swords=new[]{new SwordSaveEntry{level=1,pieces=1,isEquipped=true,equipSlotIndex=0}}};
            DataManager.Instance.gameSaveData.slots[0]=state;MainMenuManager.Instance.AskConfirmEnterGame(0,state);yield return null;MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();yield return null;yield return null;yield return null;
            int frame=Time.frameCount;yield return null;Check(Time.frameCount>frame,"Player loop advancing");Time.timeScale=0;
            var nav=GlobalNavigationController.Instance;nav.SelectGroup(NavigationGroup.Equipment);yield return null;nav.OpenRoute(NavigationRoute.Merge);yield return null;
            for(int l=1;l<=10;l++)Check(SwordRules.RequiredPieces(l)==(l<=2?2:l==10?9999:l),"Piece rule level "+l);
            Clear();var a=Add(8,7);var b=Add(8);Drop(a,b);yield return null;Check(a.level==9&&a.currentPieces==1,"Manual merge 8 to 9");
            Clear();a=Add(9,8);b=Add(9);Drop(a,b);yield return null;Check(a.level==10&&a.currentPieces==1,"Manual merge 9 to 10");
            b=Add(10,9999);Drop(a,b);yield return null;Check(a.level==10&&b!=null&&b.level==10,"Manual merge stops at 10");
            var m=MergeManager.Instance;Clear();Add(8,72);Add(10,3);m.QuickMerge();yield return null;Check(m.GetAllSwords().Count==1&&m.GetAllSwords()[0].level==10&&m.GetAllSwords()[0].currentPieces==4,"Quick merge chain and existing level 10 preserved");
            Clear();a=Add(8,1,true);Add(8,71);m.QuickMerge();yield return null;Check(a.level==10&&m.IsSwordEquipped(a),"Quick merge upgrades equipped 8 to 10");
            Clear();a=Add(9,8);b=Add(9);
            var bench=MergeWorkbench.Find(m);bench.Select(a);bench.Select(b);Check(bench.merge.button.interactable&&bench.preview.text.Contains("cấp 10"),"Workbench enables level 9 and previews 10");bench.merge.button.onClick.Invoke();yield return null;yield return null;Check(a.level==10,"Workbench actual merge reaches 10");
            bench.clear.button.onClick.Invoke();bench.Select(a);b=Add(10);bench.Select(b);Check(!bench.merge.button.interactable&&bench.preview.text.Contains("tối đa 10"),"Workbench blocks level 10 with cap text");
            var sheet=ItemDetailSheet.Find(m);sheet.ShowSword(a);Check(sheet.description.text.Contains("tối đa 10")&&!sheet.selectMerge.button.interactable,"Detail displays max 10 and disables selection");sheet.Close();
            a.SetLevel(8);sheet.ShowSword(a);Check(!sheet.description.text.Contains("tối đa")&&sheet.selectMerge.button.interactable,"Level 8 is upgradeable in detail");sheet.Close();a.SetLevel(10);
            var quest=MilestoneManager.Instance.allQuests.Single(q=>q.id=="sword_lvl_10");Check(MilestoneManager.Instance.IsCompleted(quest),"Level 10 milestone condition reachable");
            nav.SelectGroup(NavigationGroup.Other);yield return null;nav.OpenRoute(NavigationRoute.Milestones);yield return null;
            var view=UnityEngine.Object.FindObjectsByType<ProgressionScreen>(FindObjectsInactive.Include).Single(v=>!v.talents);view.Refresh();Check(view.FindCard("sword_lvl_10").State==ProgressionState.Claimable,"Level 10 milestone UI Claimable");
            Clear();Check(StorageManager.Instance.HandleSwordDrop(9)&&m.GetAllSwords().Single().level==9,"Storage intake preserves 9");Check(StorageManager.Instance.HandleSwordDrop(10)&&m.GetAllSwords().Any(s=>s.level==10),"Storage intake preserves 10");
            Clear();a=Add(10,2,true);Add(9,4);Add(10,3);m.OnEquipChanged();DataManager.Instance.SaveCurrentGame(true);
            var serialized=JsonUtility.ToJson(DataManager.Instance.gameSaveData.slots[0]);var expected=JsonUtility.FromJson<CharacterSaveData>(serialized);
            Check(expected.swords.Any(s=>s.level==10&&s.isEquipped&&s.pieces==2)&&expected.swords.Any(s=>s.level==9&&s.pieces==4),"Serialized save retains 9, 10 and stacks");
            Clear();DataManager.Instance.LoadData();DataManager.Instance.LoadCurrentGame();yield return null;yield return null;
            Check(m.GetAllSwords().Any(s=>s.level==10&&s.currentPieces==2&&m.IsSwordEquipped(s))&&m.GetAllSwords().Any(s=>s.level==9&&s.currentPieces==4),"Disk Save Load preserves equipment 10 and inventory 9");
            Clear();foreach(var page in m.inventoryPages)foreach(Transform slot in page)if(slot.childCount==0)Add(1);
            Check(StorageManager.Instance.HandleSwordDrop(10)&&StorageManager.Instance.State.pendingLoot.Last().level==10,"Full inventory pending loot preserves 10");
            Check(Time.timeScale==0,"Pause ownership preserved");
        }
        static void Cleanup()
        {
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);
            if(!SessionState.GetBool(Key+"Dirty",false))UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")},Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);
        }
    }
}
#endif

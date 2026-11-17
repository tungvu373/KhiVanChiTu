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
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Equipment.Editor
{
    [InitializeOnLoad]
    public static class Phase45QA
    {
        const string Profile="equipment-cultivation-redesign-qa",Key="Phase45QA";
        public static string Evidence=>"Docs/UI_Redesign/Phase"+SessionState.GetInt(Key+"Phase",4)+"/Evidence";
        static IEnumerator routine;
        static double due;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static EditorWindow view;
        static PropertyInfo sizeProperty;
        static object sizeGroup;
        static int oldSize;
        static readonly List<int> sizes=new List<int>();
        static Phase45QA(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))Attach();if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Cleanup;};}
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        public static void Preview(){Begin(4);SessionState.SetBool(Key+"Preview",true);}
        public static void Begin(int phase=4)
        {
            EquipmentRedesignBuilder.Require();if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length>0)throw new Exception("A save profile owner already exists.");
            SessionState.SetBool(Key+"Preview",false);SessionState.SetInt(Key+"Phase",phase);Directory.CreateDirectory(Evidence);if(File.Exists(Evidence+"/failure.txt"))File.Move(Evidence+"/failure.txt",Evidence+"/previous-failure-"+DateTime.UtcNow.Ticks+".txt");checks.Clear();errors.Clear();
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));SessionState.SetFloat(Key+"Scale",Time.timeScale);SessionState.SetBool(Key,true);
            new GameObject("__Phase45_DisposableQA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;
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
            Check(DataManager.Instance.SaveProfileId==Profile,"Disposable save profile active");
            var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Đạo Hữu Kiểm Thử - Tên Dài Có Dấu",linhThach=999999999,stageIndex=15,realmId=4,subLevel=2,combatStage=5,playerMaxHp=50000,playerCurrentHp=50000,monsterSwordsAcquired=600};
            slot.swords=new[]{new SwordSaveEntry{level=5,pieces=1,isEquipped=true,equipSlotIndex=0},new SwordSaveEntry{level=1,pieces=1,isEquipped=false},new SwordSaveEntry{level=1,pieces=1,isEquipped=false},new SwordSaveEntry{level=3,pieces=2,isEquipped=false}};
            slot.formations.levels=new[]{1,1,1,1,1,1};slot.artifacts.ore=99999;slot.alchemy.herbs=new[]{999999,888888,777777};slot.alchemy.medicineDust=999;slot.bodyStatLevels=new[]{5,5,5,5,5,5};
            DataManager.Instance.gameSaveData.slots[0]=slot;MainMenuManager.Instance.AskConfirmEnterGame(0,slot);yield return null;MainMenuManager.Instance.popupConfirmBtn.onClick.Invoke();yield return null;yield return null;yield return null;
            var frame=Time.frameCount;yield return null;Check(Time.frameCount>frame,"Player loop advances");Time.timeScale=0;
            SetupSizes();
            if(SessionState.GetBool(Key+"Preview",false)){SelectSize(new Vector2Int(1080,1920));GlobalNavigationController.Instance.SelectGroup(NavigationGroup.Equipment);yield return null;GlobalNavigationController.Instance.OpenRoute(NavigationRoute.Merge);while(true)yield return null;}

            var nav=GlobalNavigationController.Instance;var owner=StorageModalUI.Instance;var inv=owner.presentation;var mgr=MergeManager.Instance;
            AlchemyService.Instance.State.pills[1]=999999;AlchemyService.Instance.State.pills[4*8+1]=23456;StorageManager.Instance.EnqueuePendingOre(999999);
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;nav.SelectGroup(NavigationGroup.Equipment);yield return null;nav.OpenRoute(NavigationRoute.Storage);yield return null;yield return null;
                Check(owner.IsModalOpen,"Inventory opens "+size);Check(owner.gridContent.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount==4,"Four-column inventory "+size);Capture("inventory");yield return null;
                var item=owner.gridContent.GetComponentsInChildren<FoundationItem>().First();item.action.button.onClick.Invoke();yield return null;
                Check(inv.sheet.gameObject.activeInHierarchy&&nav.TopLayer=="item-detail","Item selection opens detail on top "+size);Capture("detail");yield return null;nav.Back();yield return null;Check(owner.IsModalOpen&&!inv.sheet.gameObject.activeSelf,"Back closes detail before inventory "+size);
                owner.SelectCategory(StorageCategory.Materials);yield return null;Check(owner.gridContent.GetComponentsInChildren<FoundationItem>().Length>=3,"Material filter "+size);inv.view.button.onClick.Invoke();yield return null;Check(owner.currentViewMode==StorageViewMode.CompactList,"List switch "+size);inv.view.button.onClick.Invoke();yield return null;
                owner.scrollRect.verticalNormalizedPosition=0;yield return null;Check(owner.scrollRect.verticalNormalizedPosition<=.01f,"Inventory scroll "+size);owner.Close();yield return null;yield return null;
                nav.OpenRoute(NavigationRoute.Merge);yield return null;yield return null;Check(MobileDrawerUIController.Instance.ActiveDrawerIndex==0,"Merge opens "+size);Check(MobileDrawerUIController.Instance.drawerPanels[0].GetComponent<Canvas>().overrideSorting,"Drawer renders above navigation "+size);Check(MergeWorkbench.Find(nav).merge.button.GetComponent<RectTransform>().rect.height>=120,"Primary merge touch area "+size);Capture("merge");yield return null;MobileDrawerUIController.Instance.CloseActiveDrawer();yield return null;yield return null;
            }
            var sword=mgr.GetAllSwords(false).First(s=>s.level==3);inv.sheet.ShowSword(sword);yield return null;var before=mgr.IsSwordEquipped(sword);inv.sheet.equip.button.onClick.Invoke();Check(mgr.IsSwordEquipped(sword)!=before,"Touch equip delegates to existing command");inv.sheet.equip.button.onClick.Invoke();Check(!mgr.IsSwordEquipped(sword),"Touch unequip returns sword to inventory");inv.sheet.Close();yield return null;
            nav.OpenRoute(NavigationRoute.Merge);yield return null;yield return null;var work=MergeWorkbench.Find(nav);var ingredients=mgr.GetAllSwords(false).Where(s=>s.level==1).Take(2).ToArray();work.Select(ingredients[0]);work.Select(ingredients[1]);yield return null;Check(work.merge.button.interactable,"Merge available for matching materials");Capture("merge-preview");yield return null;work.merge.button.onClick.Invoke();yield return null;Check(mgr.GetAllSwords(false).Any(s=>s.level==2),"Merge produced expected level through existing MergeSlot");Check(!work.merge.button.interactable,"Insufficient materials disables Merge");
            var saved=new CharacterSaveData();mgr.SaveToSlot(saved);string original=JsonUtility.ToJson(saved.swords);mgr.LoadFromSlot(saved);yield return null;var reloaded=new CharacterSaveData();mgr.SaveToSlot(reloaded);Check(JsonUtility.ToJson(new SwordSet{items=saved.swords})==JsonUtility.ToJson(new SwordSet{items=reloaded.swords}),"Inventory SaveToSlot/LoadFromSlot round trip");
            MobileDrawerUIController.Instance.CloseActiveDrawer();yield return null;yield return null;StorageManager.Instance.ClearPendingLoot();owner.Show(StorageCategory.Pending);yield return null;Check(owner.txtEmptyNotice.gameObject.activeSelf,"Empty pending inventory message");Capture("empty");yield return null;owner.Close();yield return null;yield return null;
            ArtifactService.Instance.State.ore=0;EconomyManager.Instance.currentLinhThach=0;var art=UnityEngine.Object.FindObjectsByType<ArtifactPanelUI>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(a=>a.upgradeButtons!=null&&a.upgradeButtons.Length==4&&a.upgradeButtons[0]!=null);art.RefreshUI();Check(!art.upgradeButtons[0].interactable,"Existing upgrade disables with missing resources");
            DataManager.Instance.SaveCurrentGame(true);var diskExpected=new SwordSet{items=DataManager.Instance.gameSaveData.slots[0].swords};string diskJson=JsonUtility.ToJson(diskExpected);DataManager.Instance.LoadData();Check(JsonUtility.ToJson(new SwordSet{items=DataManager.Instance.gameSaveData.slots[0].swords})==diskJson,"Disposable inventory persisted and reloaded by DataManager");
            Check(Time.timeScale==0,"UI interactions preserve existing pause ownership");
        }
        [Serializable]class SwordSet{public SwordSaveEntry[] items;}

        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));sizeProperty=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);oldSize=(int)sizeProperty.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Phase45 "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);sizes.Add(count-1);sizeProperty.SetValue(view,count-1);view.Focus();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/console-errors.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(view!=null&&sizeProperty!=null){sizeProperty.SetValue(view,oldSize);for(int i=sizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{sizes[i]});}sizes.Clear();
            var preservation=new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")};File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(preservation,Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());Debug.Log("[Phase45 QA] Finished. Evidence: "+Evidence);
        }
    }
}
#endif

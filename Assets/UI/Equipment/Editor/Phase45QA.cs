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
        static string Profile=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Profile("equipment-cultivation-redesign-qa");
        const string Key="Phase45QA";
        public static string Evidence=>KhiVanChiTu.UI.Polish.Editor.FinalUIValidation.Evidence("Docs/UI_Redesign/Phase"+SessionState.GetInt(Key+"Phase",4)+"/Evidence");
        static IEnumerator routine;
        static double due;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static EditorWindow view;
        static PropertyInfo sizeProperty;
        static object sizeGroup;
        static int oldSize;
        static readonly List<int> sizes=new List<int>();
        static Phase45QA(){EditorApplication.update+=()=>{if(SessionState.GetBool(Key,false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling)Cleanup();};EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))Attach();if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Cleanup;};}
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

            if(SessionState.GetInt(Key+"Phase",4)==5){foreach(var x in RunCultivation())yield return x;yield break;}
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
            StorageManager.Instance.ClearPendingLoot();
            string longName="Quặng Phôi Huyền Thiên Cửu Chuyển Vạn Niên Dùng Cho Pháp Khí Và Trận Pháp Tối Thượng";
            for(int n=0;n<60;n++)StorageManager.Instance.State.pendingLoot.Add(new PendingLootItem(PendingLootType.Ore,longName+" "+n,1,1,999999));
            owner.Show(StorageCategory.Pending);yield return null;yield return null;Canvas.ForceUpdateCanvases();Check(owner.gridContent.rect.height>owner.scrollRect.viewport.rect.height,"Large inventory produces actual scroll overflow");owner.scrollRect.StopMovement();owner.scrollRect.verticalNormalizedPosition=0;yield return null;Check(owner.scrollRect.verticalNormalizedPosition<.01f,"Large inventory scroll reaches bottom");Capture("large-inventory-scroll");yield return null;
            owner.gridContent.Find("StorageNextPage").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;Check(owner.gridContent.GetComponentsInChildren<FoundationItem>().Length==12,"Pagination shows remaining items beyond the 48-item page");
            owner.gridContent.GetComponentsInChildren<FoundationItem>().First().action.button.onClick.Invoke();yield return null;Check(inv.sheet.description.text.Contains(longName),"Long item name is fully available in the scrollable detail body");Capture("long-name-detail");yield return null;inv.sheet.Close();owner.Close();yield return null;yield return null;StorageManager.Instance.ClearPendingLoot();
            var sword=mgr.GetAllSwords(false).First(s=>s.level==3);inv.sheet.ShowSword(sword);yield return null;var before=mgr.IsSwordEquipped(sword);inv.sheet.equip.button.onClick.Invoke();Check(mgr.IsSwordEquipped(sword)!=before,"Touch equip delegates to existing command");inv.sheet.equip.button.onClick.Invoke();Check(!mgr.IsSwordEquipped(sword),"Touch unequip returns sword to inventory");inv.sheet.Close();yield return null;
            nav.OpenRoute(NavigationRoute.Merge);yield return null;yield return null;var work=MergeWorkbench.Find(nav);var ingredients=mgr.GetAllSwords(false).Where(s=>s.level==1).Take(2).ToArray();work.Select(ingredients[0]);work.Select(ingredients[1]);yield return null;Check(work.merge.button.interactable,"Merge available for matching materials");Check(!work.result.action.button.interactable,"Merge result preview stays read-only");work.first.action.button.onClick.Invoke();Check(!work.first.IsSelected,"Tap selected material clears the ingredient");work.Select(ingredients[0]);Capture("merge-preview");yield return null;work.merge.button.onClick.Invoke();yield return null;Check(mgr.GetAllSwords(false).Any(s=>s.level==2),"Merge produced expected level through existing MergeSlot");Check(!work.merge.button.interactable,"Insufficient materials disables Merge");
            var saved=new CharacterSaveData();mgr.SaveToSlot(saved);string original=JsonUtility.ToJson(saved.swords);mgr.LoadFromSlot(saved);yield return null;var reloaded=new CharacterSaveData();mgr.SaveToSlot(reloaded);Check(JsonUtility.ToJson(new SwordSet{items=saved.swords})==JsonUtility.ToJson(new SwordSet{items=reloaded.swords}),"Inventory SaveToSlot/LoadFromSlot round trip");
            MobileDrawerUIController.Instance.CloseActiveDrawer();yield return null;yield return null;StorageManager.Instance.ClearPendingLoot();owner.Show(StorageCategory.Pending);yield return null;Check(owner.txtEmptyNotice.gameObject.activeSelf,"Empty pending inventory message");Capture("empty");yield return null;owner.Close();yield return null;yield return null;
            ArtifactService.Instance.State.ore=0;EconomyManager.Instance.currentLinhThach=0;var art=UnityEngine.Object.FindObjectsByType<ArtifactPanelUI>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(a=>a.upgradeButtons!=null&&a.upgradeButtons.Length==4&&a.upgradeButtons[0]!=null);art.RefreshUI();Check(!art.upgradeButtons[0].interactable,"Existing upgrade disables with missing resources");
            DataManager.Instance.SaveCurrentGame(true);var diskExpected=new SwordSet{items=DataManager.Instance.gameSaveData.slots[0].swords};string diskJson=JsonUtility.ToJson(diskExpected);DataManager.Instance.LoadData();Check(JsonUtility.ToJson(new SwordSet{items=DataManager.Instance.gameSaveData.slots[0].swords})==diskJson,"Disposable inventory persisted and reloaded by DataManager");
            Check(Time.timeScale==0,"UI interactions preserve existing pause ownership");
        }
        [Serializable]class SwordSet{public SwordSaveEntry[] items;}


        static IEnumerable<object> RunCultivation()
        {
            var nav=GlobalNavigationController.Instance;
            var views=UnityEngine.Object.FindObjectsByType<KhiVanChiTu.UI.Cultivation.CultivationScreen>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(v=>(int)v.kind).ToArray();
            Check(views.Length==4,"Four bound cultivation screens exist");
            var manager=CultivationManager.Instance;manager.SetStageAndTuViForEditor(26,0);
            Check(manager.CurrentRealm>=RealmId.HoaThan,"Fixture unlocks all cultivation systems using real stage mapping");
            var routes=new[]{NavigationRoute.Body,NavigationRoute.Alchemy,NavigationRoute.Artifact,NavigationRoute.Formation};
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;nav.SelectGroup(NavigationGroup.Cultivation);yield return null;
                for(int i=0;i<4;i++)
                {
                    nav.OpenRoute(routes[i]);yield return null;yield return null;var v=views[i];v.Refresh();yield return null;
                    Check(v.gameObject.activeInHierarchy,"Cultivation view opens "+v.kind+" "+size);
                    Check(v.primary.button.GetComponent<RectTransform>().rect.height>=120,"Large primary action "+v.kind+" "+size);
                    Check(MobileDrawerUIController.Instance.drawerPanels[i+1].GetComponent<Canvas>().overrideSorting,"Cultivation view renders above hub "+v.kind+" "+size);
                    VerifyCultivationText(v);VerifyCloseHit(v);Capture(v.kind+"-top");yield return null;
                    for(int n=0;n<v.selectors.Length;n++){v.selectors[n].button.onClick.Invoke();Check(v.Selected==n,"Selector "+v.kind+" "+n+" "+size);}
                    Canvas.ForceUpdateCanvases();v.scroll.StopMovement();v.scroll.verticalNormalizedPosition=0;yield return null;yield return null;VerifyCultivationText(v);Capture(v.kind+"-details");yield return null;
                    Check(v.scroll.verticalNormalizedPosition<=.01f,"Scroll reaches dynamic details "+v.kind+" "+size);nav.Back();yield return null;yield return null;
                    Check(!v.gameObject.activeInHierarchy,"Back closes "+v.kind+" "+size);
                    nav.OpenRoute(routes[i]);yield return null;yield return null;
                    v.transform.Find("Header/Close").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;yield return null;
                    Check(!v.gameObject.activeInHierarchy,"Close button closes "+v.kind+" "+size);
                }
            }
            var body=views[0];nav.OpenRoute(NavigationRoute.Body);yield return null;yield return null;body.Select(0);body.Refresh();
            int old=PlayerStatsService.Instance.GetLevel(0);float stone=EconomyManager.Instance.currentLinhThach;float price=PlayerStatsService.Instance.GetUpgradeCost(0);body.primary.button.onClick.Invoke();Check(PlayerStatsService.Instance.GetLevel(0)==old+1,"Body primary upgrades one level");Check(Mathf.Approximately(EconomyManager.Instance.currentLinhThach,stone-price),"Body cost remains service cost");old=PlayerStatsService.Instance.GetLevel(0);body.secondary.button.onClick.Invoke();Check(PlayerStatsService.Instance.GetLevel(0)==old+5,"Body five-level command preserved");
            EconomyManager.Instance.currentLinhThach=0;body.Refresh();Check(!body.primary.button.interactable&&body.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.InsufficientResources,"Body insufficient state");Capture("body-insufficient");yield return null;
            PlayerStatsService.Instance.RecalculateStats(new int[6],null);manager.SetStageAndTuViForEditor(0,0);body.Select(5);Check(body.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.Locked,"Body locked node shows real requirement");Capture("body-locked");yield return null;
            PlayerStatsService.Instance.RecalculateStats(new[]{120,120,120,120,120,120},null);manager.SetStageAndTuViForEditor(26,0);EconomyManager.Instance.currentLinhThach=1e30f;body.Select(1);Check(body.State!=KhiVanChiTu.UI.Cultivation.CultivationActionState.MaxLevel,"Body does not invent a maximum level absent in core");VerifyCultivationText(body);Capture("body-large-values");yield return null;nav.Back();yield return null;yield return null;
            EconomyManager.Instance.currentLinhThach=999999999;ArtifactService.Instance.State.ore=99999;
            var artifact=views[2];nav.OpenRoute(NavigationRoute.Artifact);yield return null;yield return null;artifact.Select(0);
            for(int n=0;n<4;n++){artifact.Select(n);old=ArtifactService.Instance.GetLevel((ArtifactSlot)n);int oreCost=ArtifactService.Instance.GetOreCost((ArtifactSlot)n);price=ArtifactService.Instance.GetStoneCost((ArtifactSlot)n);stone=EconomyManager.Instance.currentLinhThach;int ore=ArtifactService.Instance.Ore;artifact.primary.button.onClick.Invoke();Check(ArtifactService.Instance.GetLevel((ArtifactSlot)n)==old+1,"Artifact actual upgrade "+n);Check(ArtifactService.Instance.Ore==ore-oreCost&&Mathf.Approximately(EconomyManager.Instance.currentLinhThach,stone-price),"Artifact exact resource cost "+n);}
            ArtifactService.Instance.State.ore=0;artifact.Refresh();Check(artifact.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.InsufficientResources,"Artifact insufficient state");
            ArtifactService.Instance.State.levels[artifact.Selected]=30;artifact.Refresh();Check(artifact.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.MaxLevel&&!artifact.primary.button.interactable,"Artifact max level disables primary");Capture("artifact-max");yield return null;manager.SetStageAndTuViForEditor(0,0);artifact.Refresh();Check(artifact.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.Locked,"Artifact locked state");manager.SetStageAndTuViForEditor(26,0);nav.Back();yield return null;yield return null;
            var formation=views[3];ArtifactService.Instance.State.ore=99999;nav.OpenRoute(NavigationRoute.Formation);yield return null;yield return null;
            for(int n=0;n<6;n++){formation.Select(n);old=FormationService.Instance.GetLevel((FormationId)n);formation.primary.button.onClick.Invoke();Check(FormationService.Instance.GetLevel((FormationId)n)==old+1,"Formation actual upgrade "+n);formation.secondary.button.onClick.Invoke();Check(FormationService.Instance.Selected==(FormationId)n,"Formation selection "+n);}
            formation.Select(1);formation.secondary.button.onClick.Invoke();formation.tertiary.button.onClick.Invoke();Check(FormationService.Instance.SpiritActive,"Spirit activation command");formation.Refresh();Check(!formation.tertiary.button.interactable,"Spirit active/cooldown disables reactivation");Capture("formation-active");yield return null;
            FormationService.Instance.State.spiritEndUtcTicks=0;formation.Refresh();Check(!formation.tertiary.button.interactable&&FormationService.Instance.SpiritCooldown>TimeSpan.Zero,"Spirit cooldown state");FormationService.Instance.State.levels[1]=30;formation.Refresh();Check(formation.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.MaxLevel,"Formation max state");
            formation.Select(0);ArtifactService.Instance.State.ore=0;formation.Refresh();Check(formation.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.InsufficientResources,"Formation insufficient state");manager.SetStageAndTuViForEditor(0,0);formation.Refresh();Check(formation.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.Locked,"Formation locked state");nav.Back();yield return null;yield return null;
            var alchemy=views[1];manager.SetStageAndTuViForEditor(8,0);nav.OpenRoute(NavigationRoute.Alchemy);yield return null;yield return null;AlchemyService.Instance.State.herbs=new[]{999999,888888,777777};AlchemyService.Instance.State.medicineDust=999;AlchemyService.Instance.State.craftsWithoutSupreme=29;alchemy.Select(0);
            int realm=(int)manager.CurrentRealm;var costs=AlchemyService.Instance.GetRecipeCosts(PillRecipe.Cultivation,realm);int[] initial=(int[])AlchemyService.Instance.State.herbs.Clone();alchemy.primary.button.onClick.Invoke();Check(AlchemyService.Instance.IsBrewing&&alchemy.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.InProgress,"Brew starts and enters progress state");for(int n=0;n<3;n++)Check(AlchemyService.Instance.State.herbs[n]==initial[n]-costs[n],"Alchemy exact ingredient cost "+n);
            double expected=Math.Max(10,(30+realm*15-AlchemyService.Instance.AlchemistRank*2)*DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.AlchemyBrewSpeed));Check(Math.Abs(AlchemyService.Instance.BrewRemaining.TotalSeconds-expected)<2,"Brew duration matches existing service formula");var remaining=AlchemyService.Instance.BrewRemaining;yield return null;Check(AlchemyService.Instance.BrewRemaining<remaining,"Countdown advances while gameplay is paused");Capture("alchemy-brewing");yield return null;
            int count=AlchemyService.Instance.GetPillCount(realm,PillRecipe.Cultivation,PillQuality.Supreme);alchemy.secondary.button.onClick.Invoke();Check(!AlchemyService.Instance.IsBrewing&&AlchemyService.Instance.GetPillCount(realm,PillRecipe.Cultivation,PillQuality.Supreme)==count+1,"Dust finish retains real pity quality and inventory result");Check(alchemy.feedback.text.Contains("Cực phẩm"),"Brew result feedback reads produced quality");Capture("alchemy-result");yield return null;
            AlchemyService.Instance.State.pills[realm*8+1]=2;AlchemyService.Instance.State.pills[realm*8+3]=1;manager.currentTuVi=0;alchemy.Refresh();alchemy.tertiary.button.onClick.Invoke();Check(AlchemyService.Instance.State.pills[realm*8+1]==1&&AlchemyService.Instance.State.pills[realm*8+3]==1,"Use pill preserves Basic-before-Refined-before-Supreme priority");
            alchemy.Select(1);alchemy.primary.button.onClick.Invoke();Check(AlchemyService.Instance.State.brewingRecipe==1,"Breakthrough recipe uses real brew command");alchemy.secondary.button.onClick.Invoke();yield return null;
            AlchemyService.Instance.State.herbs=new int[3];alchemy.Refresh();Check(alchemy.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.InsufficientResources,"Alchemy insufficient state");manager.SetStageAndTuViForEditor(0,0);alchemy.Refresh();Check(alchemy.State==KhiVanChiTu.UI.Cultivation.CultivationActionState.Locked,"Alchemy locked state");
            manager.SetStageAndTuViForEditor(8,0);AlchemyService.Instance.State.herbs=new[]{50,50,50};alchemy.Select(0);alchemy.primary.button.onClick.Invoke();yield return null;while(AlchemyService.Instance.IsBrewing)yield return null;Check(alchemy.feedback.text.Contains("Xuất lò"),"Natural real-time brew completion updates result without dust");Capture("alchemy-natural-complete");yield return null;
            nav.Back();yield return null;yield return null;nav.SelectGroup(NavigationGroup.Equipment);yield return null;nav.OpenRoute(NavigationRoute.Merge);yield return null;yield return null;
            var work=MergeWorkbench.Find(nav);var ingredient=MergeManager.Instance.GetAllSwords(false).First();work.Select(ingredient);work.first.action.button.onClick.Invoke();Check(!work.first.IsSelected,"Tap material slot removes selected merge ingredient");
            MobileDrawerUIController.Instance.CloseActiveDrawer();yield return null;yield return null;
            Check(Time.timeScale==0,"Cultivation UI preserves pause ownership");
            DataManager.Instance.SaveCurrentGame(true);DataManager.Instance.LoadData();Check(DataManager.Instance.SaveProfileId==Profile,"Cultivation persistence stays isolated");
        }
        static void VerifyCloseHit(KhiVanChiTu.UI.Cultivation.CultivationScreen view)
        {
            var close=(RectTransform)view.transform.Find("Header/Close");
            var point=RectTransformUtility.WorldToScreenPoint(null,close.TransformPoint(close.rect.center));
            var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
            Check(hits.Count>0&&(hits[0].gameObject.transform==close||hits[0].gameObject.transform.IsChildOf(close)),"Close target receives touch: "+view.kind+" "+Screen.width+"x"+Screen.height);
        }
        static void VerifyCultivationText(KhiVanChiTu.UI.Cultivation.CultivationScreen view)
        {
            Canvas.ForceUpdateCanvases();foreach(var text in view.GetComponentsInChildren<TMP_Text>(false)){text.ForceMeshUpdate();if(text.overflowMode!=TextOverflowModes.Ellipsis)Check(!text.isTextOverflowing,"No text overflow: "+view.kind+"/"+text.name+" "+Screen.width+"x"+Screen.height);}
            Check(view.progress.Normalized>=0&&view.progress.Normalized<=1,"Progress stays in bounds: "+view.kind);
        }

        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));sizeProperty=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);oldSize=(int)sizeProperty.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Phase45 "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);sizes.Add(count-1);sizeProperty.SetValue(view,count-1);view.Focus();}
        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/console-errors.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            if(!SessionState.GetBool(Key,false)||EditorApplication.isPlayingOrWillChangePlaymode)return;
            SessionState.SetBool(Key,false);
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(view!=null&&sizeProperty!=null){sizeProperty.SetValue(view,oldSize);for(int i=sizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{sizes[i]});}sizes.Clear();
            var preservation=new{productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak","")};File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(preservation,Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());Debug.Log("[Phase45 QA] Finished. Evidence: "+Evidence);
        }
    }
}
#endif

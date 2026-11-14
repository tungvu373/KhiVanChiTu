#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Explicit Editor QA. All progression writes require the new, disposable Phase06 profile.</summary>
[InitializeOnLoad]
public static class MobileUIReconstructionChecks
{
    public const string Profile="ui-reconstruction-p06-qa";
    public const string Evidence=MobileUIReconstructionBuilder.Evidence;
    const string Active="MobileUIReconstructionQA.Active";
    static MobileUIReconstructionChecks()
    {
        EditorApplication.playModeStateChanged+=OnState;
        if(!EditorApplication.isPlayingOrWillChangePlaymode && SessionState.GetBool(Active,false))EditorApplication.delayCall+=Cleanup;
    }
    static void OnState(PlayModeStateChange state)
    {if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Active,false))EditorApplication.delayCall+=Cleanup;}
    public static void Start()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Already playing");
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!=MobileUIReconstructionBuilder.ScenePath)throw new InvalidOperationException("Wrong scene");
        if(UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)
            throw new InvalidOperationException("Keep the existing save-profile owner intact");
        Canvas.ForceUpdateCanvases();EditorSceneManager.SaveScene(scene);
        SessionState.SetString("MobileUIReconstructionQA.ProductionHash",Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString());
        SessionState.SetBool("MobileUIReconstructionQA.Background",PlayerSettings.runInBackground);
        SessionState.SetBool(Active,true);
        PlayerSettings.runInBackground=true;
        var marker=new GameObject("__UIReconstructionValidation_Temporary");marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        EditorApplication.isPlaying=true;
    }
    public static void RequireIsolation()
    {
        if(!EditorApplication.isPlaying||DataManager.Instance==null||DataManager.Instance.SaveProfileId!=Profile)
            throw new InvalidOperationException("QA may only modify its disposable profile");
    }
    public static void Record(string name,string detail)
    {
        RequireIsolation();string path=Evidence+"/runtime-checks.json";
        var rows=File.Exists(path)?Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(path)):new Newtonsoft.Json.Linq.JArray();
        rows.Add(Newtonsoft.Json.Linq.JObject.FromObject(new{name,passed=true,detail,frame=Time.frameCount,timeScale=Time.timeScale,width=Screen.width,height=Screen.height,profile=DataManager.Instance.SaveProfileId}));
        File.WriteAllText(path,rows.ToString(Newtonsoft.Json.Formatting.Indented));
    }
    public static void SetResolution(int width,int height)
    {
        var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(enumType,1),width,height,"Phase06 QA "+width+"x"+height},null);
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));var property=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
        if(!SessionState.GetBool("MobileUIReconstructionQA.ViewSaved",false))
        {SessionState.SetInt("MobileUIReconstructionQA.PreviousSize",(int)property.GetValue(view));SessionState.SetBool("MobileUIReconstructionQA.ViewSaved",true);}
        property.SetValue(view,count-1);view.Focus();
    }
    public static void Hit(UnityEngine.UI.Button button)
    {
        RequireIsolation();if(button==null||!button.gameObject.activeInHierarchy)throw new InvalidOperationException("Inactive button "+button?.name);
        Canvas.ForceUpdateCanvases();var rt=(RectTransform)button.transform;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center))};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        if(hits.Count==0||!(hits[0].gameObject.transform==button.transform||hits[0].gameObject.transform.IsChildOf(button.transform)))
            throw new InvalidOperationException(button.name+" blocked by "+(hits.Count>0?hits[0].gameObject.name:"nothing"));
    }
    public static void Tap(UnityEngine.UI.Button button)
    {
        Hit(button);if(!button.IsInteractable())throw new InvalidOperationException("Disabled "+button.name);
        var data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
        ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
    }
    public static void TitleHits()
    {
        RequireIsolation();var menu=MainMenuManager.Instance;
        Hit(menu.playButton);Hit(menu.settingsButton);Hit(menu.quitButton);
        if(Time.timeScale!=0)throw new InvalidOperationException("Title lost pause ownership");
        Record("title-primary-actions","Three primary actions are top raycast hits; original menu owns timeScale0.");
    }
    public static void EnterFixture(bool progressed)
    {
        RequireIsolation();
        var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Thiên Vân Đạo Hữu Trường Sinh",linhThach=progressed?1234567890:5000,coDuyen=progressed?123456:25,stageIndex=progressed?30:0,realmId=progressed?5:0,subLevel=1,currentTuVi=0,combatStage=progressed?25:1,playerMaxHp=10000,playerCurrentHp=10000,skillActiveUnlocked=progressed,skillNormal0Unlocked=progressed,skillNormal1Unlocked=progressed,skillNormal2Unlocked=progressed,swordStyleUnlocked=progressed,monsterSwordsAcquired=600};
        slot.artifacts.ore=100000;slot.formations.levels=new[]{1,1,1,1,1,1};
        slot.swords=new[]{new SwordSaveEntry{level=progressed?7:1,pieces=1,isEquipped=true,equipSlotIndex=0},new SwordSaveEntry{level=2,pieces=1},new SwordSaveEntry{level=2,pieces=1}};
        DataManager.Instance.gameSaveData.slots[0]=slot;
        MainMenuManager.Instance.AskConfirmEnterGame(0,slot);
    }
    public static void HudHits()
    {
        RequireIsolation();var h=PortraitHudController.Instance;
        foreach(var button in MobileDrawerUIController.Instance.dockButtons)Hit(button);
        foreach(var button in h.quickButtons)Hit(button);
        Hit(h.menuButton);Hit(h.walletButton);Hit(h.focusButton.GetComponent<UnityEngine.UI.Button>());
        var skills=h.skills.GetComponent<MobileSkillHUD>();Hit(skills.btnUltimate);foreach(var button in skills.btnNormalSkills)Hit(button);
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)},hits);
        if(hits.Any(x=>x.gameObject.GetComponentInParent<PortraitHudController>()!=null))throw new InvalidOperationException("HUD blocks central arena");
        Record("hud-hit-routing","5dock,4quick,menu,wallet,focus,4skills hit; central arena passes through.");
    }
    public static void DrawerReady(int index)
    {
        RequireIsolation();var drawer=MobileDrawerUIController.Instance;
        if(drawer.ActiveDrawerIndex!=index)throw new InvalidOperationException("Wrong drawer");
        Hit(drawer.closeButtons[index]);
        var panel=drawer.drawerPanels[index];var view=panel.Find("P6Frame").GetComponent<MobileUIPanelView>();
        if(view.title==null||!view.title.gameObject.activeInHierarchy)throw new InvalidOperationException("Missing retained title");
        foreach(var scroll in panel.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true))
            if(scroll.gameObject.activeInHierarchy&&scroll.viewport!=null&&scroll.viewport.GetComponent<UnityEngine.UI.Mask>()==null&&scroll.viewport.GetComponent<UnityEngine.UI.RectMask2D>()==null)throw new InvalidOperationException("Unmasked scroll viewport");
        Record("workbench-"+index,"Original dock route, close target, title and scroll masks remain available under new prefab chrome.");
    }
    public static object Geometry(string label)
    {
        RequireIsolation();Canvas.ForceUpdateCanvases();var canvas=UnityEngine.Object.FindFirstObjectByType<MobileUIReconstruction>();
        var buttons=new List<object>();foreach(var button in canvas.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if(!button.gameObject.activeInHierarchy)continue;var rt=(RectTransform)button.transform;var corners=new Vector3[4];rt.GetWorldCorners(corners);
            buttons.Add(new{path=AnimationUtility.CalculateTransformPath(button.transform,null),size=new[]{rt.rect.width,rt.rect.height},screen=new[]{corners[0].x,corners[0].y,corners[2].x,corners[2].y},interactable=button.IsInteractable()});
        }
        var result=new{label,width=Screen.width,height=Screen.height,safeArea=Screen.safeArea.ToString(),frame=Time.frameCount,timeScale=Time.timeScale,buttons};
        File.WriteAllText(Evidence+"/"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
    }
    public static void Cleanup()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play first");
        foreach(var marker in UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
        PlayerSettings.runInBackground=SessionState.GetBool("MobileUIReconstructionQA.Background",false);
        SessionState.SetBool(Active,false);
        string before=SessionState.GetString("MobileUIReconstructionQA.ProductionHash","");
        if(before!=Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString())throw new InvalidOperationException("Production save PlayerPrefs changed");
        if(SessionState.GetBool("MobileUIReconstructionQA.ViewSaved",false))
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,SessionState.GetInt("MobileUIReconstructionQA.PreviousSize",0));
            SessionState.SetBool("MobileUIReconstructionQA.ViewSaved",false);
        }
        Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
#endif

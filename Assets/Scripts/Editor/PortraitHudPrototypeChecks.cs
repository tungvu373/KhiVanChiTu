#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Editor-only, disposable-profile checks of real HUD bindings, geometry and pointer routing.</summary>
[InitializeOnLoad]
public static class PortraitHudPrototypeChecks
{
    public const string Profile="portrait-hud-p04-qa";
    public const string Evidence="Docs/UI_Redesign/Backups/Phase04-20261112-173028";
    const string ActiveKey="PortraitHudQA.Active";
    static readonly List<object> Results=new List<object>();
    static PortraitHudPrototypeChecks(){EditorApplication.playModeStateChanged+=OnState;}
    static void OnState(PlayModeStateChange state)
    {if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(ActiveKey,false))EditorApplication.delayCall+=Cleanup;}

    [MenuItem("TuTien/UI/Portrait HUD/03 Start isolated validation")]
    public static void Start()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!=PortraitHudPrototypeBuilder.ScenePath||scene.isDirty)throw new InvalidOperationException("Save the mobile prototype before validation.");
        if(UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include).Length!=0)throw new InvalidOperationException("Unexpected save isolation marker; keep the existing owner intact.");
        SessionState.SetString("PortraitHudQA.ProductionPrefsHash",Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString());
        SessionState.SetBool(ActiveKey,true);
        var marker=new GameObject("__PortraitHudValidation_Temporary");marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        EditorApplication.isPlaying=true;
    }
    public static void RequireIsolation()
    {if(!EditorApplication.isPlaying||DataManager.Instance==null||DataManager.Instance.SaveProfileId!=Profile)throw new InvalidOperationException("Validation may only write the disposable portrait HUD profile.");}

    public static void EnterFixture(bool progressed)
    {
        RequireIsolation();
        var slot=new CharacterSaveData{isSlotEmpty=false,characterName="Thiên Vân Đạo Hữu Trường Sinh",linhThach=progressed?1234567890:5000,coDuyen=progressed?123456:25,stageIndex=progressed?30:0,realmId=progressed?5:0,subLevel=1,currentTuVi=0,combatStage=progressed?25:1,playerMaxHp=10000,playerCurrentHp=10000,skillActiveUnlocked=progressed,skillNormal0Unlocked=progressed,skillNormal1Unlocked=progressed,skillNormal2Unlocked=progressed,swordStyleUnlocked=progressed,monsterSwordsAcquired=600};
        slot.artifacts.ore=100000;slot.formations.levels=new[]{1,1,1,1,1,1};
        slot.swords=new[]{new SwordSaveEntry{level=progressed?7:1,pieces=1,isEquipped=true,equipSlotIndex=0},new SwordSaveEntry{level=2,pieces=1},new SwordSaveEntry{level=2,pieces=1}};
        DataManager.Instance.gameSaveData.slots[0]=slot;
        MainMenuManager.Instance.AskConfirmEnterGame(0,slot);
        // Confirm in a subsequent editor tick so a newly enabled Graphic registers its raycast depth.
    }

    public static void SetResolution(int width,int height)
    {
        var asm=typeof(Editor).Assembly;
        var sizesType=asm.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(enumType,1),width,height,"Portrait HUD QA "+width+"x"+height},null);
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));
        if(!SessionState.GetBool("PortraitHudQA.ViewSaved",false))
        {SessionState.SetInt("PortraitHudQA.PreviousSize",(int)view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(view));SessionState.SetBool("PortraitHudQA.ViewSaved",true);}
        view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,count-1);view.Focus();
    }

    public static void Hit(UnityEngine.UI.Button button)
    {
        if(button==null||!button.gameObject.activeInHierarchy)throw new InvalidOperationException("Missing/inactive button: "+button?.name);
        Canvas.ForceUpdateCanvases();
        var rt=(RectTransform)button.transform;var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center))};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
        if(hits.Count==0||!(hits[0].gameObject.transform==button.transform||hits[0].gameObject.transform.IsChildOf(button.transform)))throw new InvalidOperationException(button.name+" blocked by "+(hits.Count>0?hits[0].gameObject.name:"no surface"));
    }
    public static void Tap(UnityEngine.UI.Button button){RequireIsolation();Hit(button);if(!button.IsInteractable())throw new InvalidOperationException(button.name+" disabled");button.onClick.Invoke();}
    public static string Snapshot(string label)
    {
        RequireIsolation();var hud=PortraitHudController.Instance;Canvas.ForceUpdateCanvases();
        var list=new List<object>();
        foreach(var b in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            var rt=(RectTransform)b.transform;var corners=new Vector3[4];rt.GetWorldCorners(corners);
            list.Add(new{name=b.name,active=b.gameObject.activeInHierarchy,interactable=b.IsInteractable(),referenceSize=new[]{rt.rect.width,rt.rect.height},screenRect=new[]{corners[0].x,corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y}});
        }
        var result=new{label,screen=new[]{Screen.width,Screen.height},safeArea=Screen.safeArea.ToString(),timeScale=Time.timeScale,frame=Time.frameCount,profile=DataManager.Instance.SaveProfileId,focus=hud.IsFocusMode,clearArea=hud.CentralClearArea.ToString(),buttons=list};
        string json=Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented);File.WriteAllText(Evidence+"/"+label+".json",json);return json;
    }

    public static void CheckDefaultHits()
    {
        RequireIsolation();var h=PortraitHudController.Instance;
        foreach(var b in MobileDrawerUIController.Instance.dockButtons)Hit(b);
        foreach(var b in h.quickButtons)Hit(b);
        Hit(h.menuButton);Hit(h.walletButton);Hit(h.focusButton.GetComponent<UnityEngine.UI.Button>());
        var skills=h.skills.GetComponent<MobileSkillHUD>();Hit(skills.btnUltimate);foreach(var b in skills.btnNormalSkills)Hit(b);
        var pointer=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        if(hits.Any(x=>x.gameObject.GetComponentInParent<PortraitHudController>()!=null))throw new InvalidOperationException("HUD intercepts the center of gameplay: "+hits[0].gameObject.name);
        Record("default-hit-routing",true,"Dock,quick actions,menu,wallet,focus,4skills are top hits;center passes through.");
    }
    public static void Record(string name,bool passed,string detail)
    {if(Results.Count==0&&File.Exists(Evidence+"/runtime-checks.json"))foreach(var old in Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(Evidence+"/runtime-checks.json")))Results.Add(old);Results.Add(new{name,passed,detail,frame=Time.frameCount,resolution=Screen.width+"x"+Screen.height});File.WriteAllText(Evidence+"/runtime-checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(Results,Newtonsoft.Json.Formatting.Indented));if(!passed)throw new InvalidOperationException(detail);}

    [MenuItem("TuTien/UI/Portrait HUD/04 Cleanup validation marker")]
    public static void Cleanup()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
        foreach(var m in UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include))if(m.profileId==Profile)UnityEngine.Object.DestroyImmediate(m.gameObject);
        SessionState.SetBool(ActiveKey,false);
        var before=SessionState.GetString("PortraitHudQA.ProductionPrefsHash","");var after=Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString();
        if(before!=after)throw new InvalidOperationException("Production PlayerPrefs changed during isolated validation.");
        if(SessionState.GetBool("PortraitHudQA.ViewSaved",false))
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,SessionState.GetInt("PortraitHudQA.PreviousSize",0));
            SessionState.SetBool("PortraitHudQA.ViewSaved",false);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Portrait HUD QA] Temporary marker removed; production PlayerPrefs unchanged.");
    }
}
#endif

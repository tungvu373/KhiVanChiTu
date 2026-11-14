#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using Unity.Profiling;
using Newtonsoft.Json.Linq;

[InitializeOnLoad]
public static partial class MobileUIAnimationChecks
{
    public const string Evidence="Docs/UI_Redesign/Backups/Phase08-20261113-045800Z";
    public const string Profile="ui-motion-p08-qa";
    const string Key="MobileUIAnimationQA.";
    static IEnumerator runner;static double next;static JArray results;static EditorWindow gameView;static int assertions;
    static readonly BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static MobileUIAnimationChecks()
    {
        EditorApplication.playModeStateChanged+=Mode;EditorApplication.update+=Tick;
        if(SessionState.GetBool(Key+"Active",false)&&!EditorApplication.isPlayingOrWillChangePlaymode)EditorApplication.delayCall+=Cleanup;
    }
    public static void StartBaseline()=>Start("baseline");
    public static void Start(string mode)
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!="Assets/Scenes/SampleScene_MobileWork.unity")throw new InvalidOperationException("Saved mobile scene in Edit mode required");
        if(UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)throw new InvalidOperationException("Keep existing save-profile owner intact");
        SessionState.SetString(Key+"Mode",mode);SessionState.SetBool(Key+"Background",PlayerSettings.runInBackground);
        SessionState.SetString(Key+"ProductionHash",Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString());SessionState.SetBool(Key+"Active",true);
        PlayerSettings.runInBackground=true;var marker=new GameObject("__UIMotionValidation_Temporary");marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        SessionState.SetBool(Key+"Entering",true);EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;
    }
    static void Mode(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Key+"Active",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            SessionState.SetBool(Key+"Entering",false);
            Application.runInBackground=true;EditorApplication.isPaused=false;gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));gameView.Focus();
            results=new JArray();string mode=SessionState.GetString(Key+"Mode","");runner=mode=="baseline"?BaselineSuite():mode=="simulator"?SimulatorSuite():FullSuite();next=0;
        }
        if(state==PlayModeStateChange.EnteredEditMode){runner=null;EditorApplication.delayCall+=Cleanup;}
    }
    static void Tick()
    {
        if(SessionState.GetBool(Key+"Active",false)&&EditorApplication.isPlaying){EditorApplication.QueuePlayerLoopUpdate();if(gameView!=null)gameView.Repaint();}
        if(SessionState.GetBool(Key+"Active",false)&&!SessionState.GetBool(Key+"Entering",false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&runner==null){Cleanup();return;}
        if(runner==null||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try{if(runner.MoveNext()){next=EditorApplication.timeSinceStartup+(runner.Current is double delay?delay:0);return;}}
        catch(Exception ex){Record("suite-error",false,ex.ToString());}
        runner=null;EditorApplication.isPlaying=false;
    }
    public static void RequireIsolation()
    {if(!EditorApplication.isPlaying||DataManager.Instance==null||DataManager.Instance.SaveProfileId!=Profile)throw new InvalidOperationException("Only the disposable animation profile may be changed");}
    static void Record(string name,bool passed,string detail)
    {RequireIsolation();results.Add(JObject.FromObject(new{name,passed,detail,assertions,frame=Time.frameCount,profile=Profile,width=Screen.width,height=Screen.height,utc=DateTime.UtcNow.ToString("o")}));File.WriteAllText(Evidence+"/"+SessionState.GetString(Key+"Mode","unknown")+"-checks.json",results.ToString());}
    static void Expect(bool condition,string reason){assertions++;if(!condition)throw new InvalidOperationException(reason);}
    static void Check(string name,Action test)
    {assertions=0;try{test();Record(name,true,"Intent, intermediate pose or settled state asserted.");}catch(Exception ex){Record(name,false,ex.GetBaseException().Message);}}
    static object Call(object target,string name,params object[] args)=>target.GetType().GetMethods(All).First(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(target,args);
    static void Click(UnityEngine.UI.Button b)
    {Expect(b!=null&&b.isActiveAndEnabled&&b.IsInteractable(),"Button unavailable");ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    static IEnumerator BaselineSuite()
    {
        yield return 1.0;RequireIsolation();int frame=Time.frameCount;double deadline=EditorApplication.timeSinceStartup+8;
        while(Time.frameCount<frame+5&&EditorApplication.timeSinceStartup<deadline)yield return 0.02;
        Expect(Time.frameCount>=frame+5,"Frozen player loop");
        var menu=MainMenuManager.Instance;DataManager.Instance.gameSaveData=new GameSaveData();DataManager.Instance.SaveData();menu.OpenCharacterCreation(0);menu.nameInput.text="P08 Motion Fixture";Click(menu.confirmCreateButton);yield return 0.5;RequireIsolation();Time.timeScale=0;
        Record("advancing-isolated-session",DataManager.Instance.isSessionActive,"Real Play frames advance; disposable slot loaded before profiling.");
        using(var main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",256))
        using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",256))
        {
            int start=Time.frameCount;while(Time.frameCount<start+120)yield return null;
            var cpu=new List<ProfilerRecorderSample>();main.CopyTo(cpu);var alloc=new List<ProfilerRecorderSample>();gc.CopyTo(alloc);
            File.WriteAllText(Evidence+"/baseline-profiler.json",JObject.FromObject(new{frameStart=start,frameEnd=Time.frameCount,timeScale=Time.timeScale,width=Screen.width,height=Screen.height,mainThreadNs=cpu.Select(s=>s.Value).ToArray(),gcAllocatedBytes=alloc.Select(s=>s.Value).ToArray(),scope="Unity Editor baseline; not physical-device performance"}).ToString());
        }
        Record("baseline-profiled",true,"120 real frames sampled with Main Thread and GC Allocated In Frame recorders.");
    }
    public static void Cleanup()
    {
        if(EditorApplication.isPlaying)return;
        foreach(var marker in UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
        PlayerSettings.runInBackground=SessionState.GetBool(Key+"Background",false);SessionState.SetBool(Key+"Active",false);SessionState.SetBool(Key+"Entering",false);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        if(SessionState.GetBool(Key+"ViewSaved",false))
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",All).SetValue(view,SessionState.GetInt(Key+"View",0));SessionState.SetBool(Key+"ViewSaved",false);
        }
        File.WriteAllText(Evidence+"/"+SessionState.GetString(Key+"Mode","unknown")+"-cleanup.json",JObject.FromObject(new{productionPrefsUnchanged=SessionState.GetString(Key+"ProductionHash","")==Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString(),background=PlayerSettings.runInBackground,playing=EditorApplication.isPlaying,sceneDirty=EditorSceneManager.GetActiveScene().isDirty}).ToString());
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Explicit integration QA against a disposable save profile. Never runs on a player profile.</summary>
[InitializeOnLoad]
public static partial class MobileUIPhase09Checks
{
    public static string Evidence=>SessionState.GetString("MobileUIPhase09QA.Evidence","Docs/UI_Redesign/Backups/Phase09-20261113-072438Z");
    public static string Profile=>SessionState.GetString("MobileUIPhase09QA.Profile","ui-responsive-p09-qa");
    const string Key="MobileUIPhase09QA.";
    static IEnumerator suite;
    static EditorWindow gameView;
    static double next;
    static JArray rows;
    static int assertions;
    static List<string> clicked;
    static JObject observations;
    static readonly BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static MobileUIPhase09Checks()
    {
        EditorApplication.playModeStateChanged+=OnMode;
        EditorApplication.update+=Tick;
        if(SessionState.GetBool(Key+"Active",false)&&!EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.delayCall+=Cleanup;
    }
    [MenuItem("TuTien/UI/QA/Run Phase09 functionality")]
    public static void RunFunctionality()=>Start("functional");
    [MenuItem("TuTien/UI/QA/Run Phase09 responsive screenshots")]
    public static void RunResponsive()=>Start("responsive");
    [MenuItem("TuTien/UI/QA/Run Phase09 device safe areas")]
    public static void RunDevices()=>Start("devices");
    // Later phases reuse the verified integration suite without touching older evidence or save profiles.
    public static void StartExternal(string evidence,string profile,string mode)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool(Key+"Active",false))
            throw new InvalidOperationException("Another validation is active");
        if(!evidence.StartsWith("Docs/UI_Redesign/Backups/Phase10-")||profile!="ui-optimization-p10-qa")
            throw new InvalidOperationException("Phase10 requires its own checkpoint and disposable profile");
        SessionState.SetString(Key+"Evidence",evidence);SessionState.SetString(Key+"Profile",profile);
        try{Start(mode);}catch{SessionState.EraseString(Key+"Evidence");SessionState.EraseString(Key+"Profile");throw;}
    }
    public static void Start(string mode="functional")
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!=MobileUIReconstructionBuilder.ScenePath)
            throw new InvalidOperationException("Requires the saved mobile scene in Edit mode");
        if(!File.Exists(Evidence+"/checkpoint.json"))throw new InvalidOperationException("Checkpoint required");
        if(UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)
            throw new InvalidOperationException("Existing save profile must not be replaced");
        SessionState.SetString(Key+"Mode",mode);Snapshot(mode+"-edit-before");
        var prefs=new JArray();
        foreach(var field in typeof(AudioManager).GetFields(All).Where(f=>f.IsLiteral&&f.FieldType==typeof(string)&&f.Name.StartsWith("PREF_")))
        {
            string key=(string)field.GetRawConstantValue();bool integer=field.Name.Contains("MUTED");
            prefs.Add(new JObject{{"key",key},{"exists",PlayerPrefs.HasKey(key)},{"integer",integer},{"value",integer?PlayerPrefs.GetInt(key):PlayerPrefs.GetFloat(key)}});
        }
        SessionState.SetString(Key+"Audio",prefs.ToString());
        SessionState.SetString(Key+"ProductionHash",Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString());
        SessionState.SetBool(Key+"Background",PlayerSettings.runInBackground);
        SessionState.SetBool(Key+"Active",true);
        if(!SessionState.GetBool(Key+"Resume",false)&&mode!="responsive-badge"&&mode!="devices-retry")File.WriteAllText(ResultPath,"[]");
        SetResolution(1080,1920);
        PlayerSettings.runInBackground=true;
        var marker=new GameObject("__UIPhase09Validation_Temporary");
        marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        SessionState.SetBool(Key+"Entering",true);EditorApplication.delayCall+=()=>EditorApplication.isPlaying=true;
    }
    public static void StartResume(){SessionState.SetBool(Key+"Resume",true);Start("resume");}
    static void OnMode(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Key+"Active",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            SessionState.SetBool(Key+"Entering",false);Application.runInBackground=true;EditorApplication.isPaused=false;
            gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));gameView.Focus();
            bool resume=SessionState.GetBool(Key+"Resume",false);SessionState.SetBool(Key+"Resume",false);
            string mode=SessionState.GetString(Key+"Mode","");rows=resume?JArray.Parse(File.ReadAllText(Evidence+"/runtime-checks.json")):(mode=="responsive-badge"||mode=="devices-retry")?JArray.Parse(File.ReadAllText(ResultPath)):new JArray();suite=mode=="content-repair"?ContentRepairSuite():mode=="p10-regressions"?OptimizationRegressions():mode.StartsWith("optimization-")?OptimizationSuite(mode.Substring("optimization-".Length)):mode=="responsive-badge"?BadgeSuite():mode=="responsive"?ResponsiveSuite():mode.StartsWith("devices")?DevicesSuite():resume?ResumeSuite():Suite();next=0;
        }
        if(state==PlayModeStateChange.EnteredEditMode){suite=null;EditorApplication.delayCall+=Cleanup;}
    }
    static void Tick()
    {
        if(SessionState.GetBool(Key+"Active",false)&&EditorApplication.isPlaying){EditorApplication.QueuePlayerLoopUpdate();if(gameView!=null)gameView.Repaint();}
        if(SessionState.GetBool(Key+"Active",false)&&!SessionState.GetBool(Key+"Entering",false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&suite==null){Cleanup();return;}
        if(suite==null||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try
        {
            if(suite.MoveNext()){next=EditorApplication.timeSinceStartup+(suite.Current is double delay?delay:0.05);return;}
        }
        catch(Exception ex){WriteFailure("suite-aborted",ex);}
        suite=null;EditorApplication.isPlaying=false;
    }
    public static void RequireIsolation()
    {
        if(!EditorApplication.isPlaying||DataManager.Instance==null||DataManager.Instance.SaveProfileId!=Profile)
            throw new InvalidOperationException("Blocked: QA may mutate only its own disposable profile");
    }
    static void Case(string name,string screen,string script,string methods,Action test)
    {
        RequireIsolation();assertions=0;clicked=new List<string>();observations=new JObject();string error=null;
        try{test();}catch(Exception ex){error=ex.GetBaseException().Message;}
        if(SessionState.GetString(Key+"Mode","").StartsWith("responsive"))foreach(var previous in rows.Where(r=>(string)r["name"]==name).ToArray())previous.Remove();
        var recorded=JObject.FromObject(new{name,screen,script=script.StartsWith("Assets/")?script:"Assets/Scripts/"+script,methods=methods.Split(','),passed=error==null,error,assertions,clicked,observations,frame=Time.frameCount,timeScale=Time.timeScale,width=Screen.width,height=Screen.height,profile=DataManager.Instance.SaveProfileId,utc=DateTime.UtcNow.ToString("o")});
        var prior=SessionState.GetString(Key+"Mode","")=="devices-retry"?rows.FirstOrDefault(r=>(string)r["name"]==name):null;
        if(prior!=null)prior.Replace(recorded);else rows.Add(recorded);
        File.WriteAllText(ResultPath,rows.ToString());
    }
    static void WriteFailure(string name,Exception ex)
    {
        if(rows==null)rows=new JArray();rows.Add(JObject.FromObject(new{name,passed=false,error=ex.GetBaseException().Message,stack=ex.ToString()}));
        File.WriteAllText(ResultPath,rows.ToString());
    }
    static void Expect(bool condition,string reason){assertions++;if(!condition)throw new InvalidOperationException(reason);}
    static void Near(float actual,float expected,string reason){Expect(Mathf.Abs(actual-expected)<=0.05f,reason+": "+actual+" != "+expected);}
    static void Note(string key,object value){observations[key]=value==null?JValue.CreateNull():JToken.FromObject(value);}
    static T Find<T>() where T:UnityEngine.Object
    {
        var found=UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        return found.FirstOrDefault(o=>o is Component c&&c.gameObject.activeInHierarchy)??found.FirstOrDefault();
    }
    static object Field(object owner,string name)=>owner.GetType().GetField(name,All).GetValue(owner);
    static object Call(object owner,string name,params object[] args)
    {var method=owner.GetType().GetMethods(All).First(m=>m.Name==name&&m.GetParameters().Length==args.Length);return method.Invoke(owner,args);}
    static void Click(Button button)
    {
        Expect(button!=null&&button.isActiveAndEnabled&&button.IsInteractable(),"Button unavailable: "+(button==null?"null":button.name));
        clicked?.Add(AnimationUtility.CalculateTransformPath(button.transform,null));
        ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
    }
    static Button Named(Transform root,string name)=>root.GetComponentsInChildren<Button>(true).First(b=>b.name==name);
    static void Rich(){RequireIsolation();EconomyManager.Instance.currentLinhThach=1e9f;EconomyManager.Instance.currentCoDuyen=10000;ArtifactService.Instance.AddOre(100000);}
    static void Stage(int index){RequireIsolation();CultivationManager.Instance.SetStageAndTuViForEditor(index,0);}
    static void Highest(){Stage(CultivationManager.Instance.allStages.Length-1);}
    static void CloseDrawer()=>Call(MobileDrawerUIController.Instance,"CloseAllInstant");
    static void SetResolution(int width,int height)
    {
        var asm=typeof(Editor).Assembly;var sizesType=asm.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
        var getGroup=sizesType.GetMethod("GetGroup");
        var groupKind=Enum.Parse(getGroup.GetParameters()[0].ParameterType,EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android?"Android":"Standalone");
        var group=getGroup.Invoke(sizes,new[]{groupKind});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,All,null,new object[]{Enum.ToObject(enumType,1),width,height,"Phase09 portrait QA"},null);
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));var prop=view.GetType().GetProperty("selectedSizeIndex",All);
        if(!SessionState.GetBool(Key+"ViewSaved",false)){SessionState.SetInt(Key+"View",(int)prop.GetValue(view));SessionState.SetBool(Key+"ViewSaved",true);}prop.SetValue(view,count-1);view.Focus();
    }
    public static object Snapshot(string label)
    {
        var scene=EditorSceneManager.GetActiveScene();var nodes=new JArray();var references=new JArray();var callbacks=new JArray();
        foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            var go=t.gameObject;string id=GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
            nodes.Add(JObject.FromObject(new{id,path=AnimationUtility.CalculateTransformPath(t,null),active=go.activeSelf,components=go.GetComponents<Component>().Select(c=>c==null?"MISSING":c.GetType().FullName).ToArray()}));
            foreach(var c in go.GetComponents<MonoBehaviour>())
            {
                if(c==null)continue;var so=new SerializedObject(c);var it=so.GetIterator();
                while(it.Next(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.objectReferenceValue!=null)
                    references.Add(JObject.FromObject(new{owner=id,component=c.GetType().FullName,property=it.propertyPath,target=GlobalObjectId.GetGlobalObjectIdSlow(it.objectReferenceValue).ToString()}));
            }
            var b=go.GetComponent<Button>();if(b!=null)for(int i=0;i<b.onClick.GetPersistentEventCount();i++)
                callbacks.Add(JObject.FromObject(new{owner=id,index=i,target=GlobalObjectId.GetGlobalObjectIdSlow(b.onClick.GetPersistentTarget(i)).ToString(),method=b.onClick.GetPersistentMethodName(i)}));
        }
        var result=new{scene=scene.path,nodes,references,callbacks,playing=EditorApplication.isPlaying,sceneDirty=scene.isDirty,productionPrefsHash=Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString()};
        File.WriteAllText(Evidence+"/"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{nodes=nodes.Count,references=references.Count,callbacks=callbacks.Count};
    }
    static string ResultPath=>Evidence+"/"+(SessionState.GetString(Key+"Mode","")=="p10-regressions"?"p10-regression-checks":SessionState.GetString(Key+"Mode","").StartsWith("optimization-")?SessionState.GetString(Key+"Mode","")+"-checks":SessionState.GetString(Key+"Mode","").StartsWith("responsive")?"responsive-checks":SessionState.GetString(Key+"Mode","").StartsWith("devices")?"device-checks":"runtime-checks")+".json";
    public static void Cleanup()
    {
        if(EditorApplication.isPlaying)return;
        foreach(var m in UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(m.profileId==Profile)UnityEngine.Object.DestroyImmediate(m.gameObject);
        PlayerSettings.runInBackground=SessionState.GetBool(Key+"Background",false);
        foreach(JObject p in JArray.Parse(SessionState.GetString(Key+"Audio","[]")))
        {
            string key=(string)p["key"];
            if(!(bool)p["exists"])PlayerPrefs.DeleteKey(key);else if((bool)p["integer"])PlayerPrefs.SetInt(key,(int)p["value"]);else PlayerPrefs.SetFloat(key,(float)p["value"]);
        }
        PlayerPrefs.Save();
        var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",All).SetValue(view,SessionState.GetInt(Key+"View",0));
        SessionState.SetBool(Key+"Active",false);SessionState.SetBool(Key+"Entering",false);SessionState.SetBool(Key+"ViewSaved",false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());Snapshot(SessionState.GetString(Key+"Mode","")+"-edit-after");
        bool unchanged=SessionState.GetString(Key+"ProductionHash","")==Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString();
        File.WriteAllText(Evidence+"/"+SessionState.GetString(Key+"Mode","")+"-cleanup.json",JObject.FromObject(new{productionPrefsUnchanged=unchanged,playing=EditorApplication.isPlaying,qaMarkers=UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Count(m=>m.profileId==Profile),background=PlayerSettings.runInBackground}).ToString());
        SessionState.EraseString(Key+"Evidence");SessionState.EraseString(Key+"Profile");
    }
}
#endif

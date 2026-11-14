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
public static partial class MobileUIFeatureReconnectionChecks
{
    public const string Evidence="Docs/UI_Redesign/Backups/Phase07-20261112-162600Z";
    public const string Profile="ui-reconnection-p07-qa";
    const string Key="MobileUIReconnectionQA.";
    static IEnumerator suite;
    static double next;
    static JArray rows;
    static int assertions;
    static List<string> clicked;
    static JObject observations;
    static readonly BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static MobileUIFeatureReconnectionChecks()
    {
        EditorApplication.playModeStateChanged+=OnMode;
        EditorApplication.update+=Tick;
        if(SessionState.GetBool(Key+"Active",false)&&!EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.delayCall+=Cleanup;
    }
    [MenuItem("TuTien/UI/Reconnection/Run isolated integration checks")]
    public static void Start()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.isDirty||scene.path!=MobileUIReconstructionBuilder.ScenePath)
            throw new InvalidOperationException("Requires the saved mobile scene in Edit mode");
        if(!File.Exists(Evidence+"/baseline.json"))throw new InvalidOperationException("Checkpoint required");
        if(UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)
            throw new InvalidOperationException("Existing save profile must not be replaced");
        Snapshot("edit-before");
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
        if(!SessionState.GetBool(Key+"Resume",false))File.WriteAllText(Evidence+"/runtime-checks.json","[]");
        SetResolution(390,844);
        PlayerSettings.runInBackground=true;
        var marker=new GameObject("__UIReconnectionValidation_Temporary");
        marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        EditorApplication.isPlaying=true;
    }
    public static void StartResume(){SessionState.SetBool(Key+"Resume",true);Start();}
    static void OnMode(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Key+"Active",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode)
        {
            Application.runInBackground=true;EditorApplication.isPaused=false;
            EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            bool resume=SessionState.GetBool(Key+"Resume",false);SessionState.SetBool(Key+"Resume",false);
            rows=resume?JArray.Parse(File.ReadAllText(Evidence+"/runtime-checks.json")):new JArray();suite=resume?ResumeSuite():Suite();next=0;
        }
        if(state==PlayModeStateChange.EnteredEditMode){suite=null;EditorApplication.delayCall+=Cleanup;}
    }
    static void Tick()
    {
        if(SessionState.GetBool(Key+"Active",false)&&EditorApplication.isPlaying)EditorApplication.QueuePlayerLoopUpdate();
        if(SessionState.GetBool(Key+"Active",false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&suite==null){Cleanup();return;}
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
        rows.Add(JObject.FromObject(new{name,screen,script="Assets/Scripts/"+script,methods=methods.Split(','),passed=error==null,error,assertions,clicked,observations,frame=Time.frameCount,timeScale=Time.timeScale,width=Screen.width,height=Screen.height,profile=DataManager.Instance.SaveProfileId,utc=DateTime.UtcNow.ToString("o")}));
        File.WriteAllText(Evidence+"/runtime-checks.json",rows.ToString());
    }
    static void WriteFailure(string name,Exception ex)
    {
        if(rows==null)rows=new JArray();rows.Add(JObject.FromObject(new{name,passed=false,error=ex.GetBaseException().Message,stack=ex.ToString()}));
        File.WriteAllText(Evidence+"/runtime-checks.json",rows.ToString());
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
        clicked.Add(AnimationUtility.CalculateTransformPath(button.transform,null));
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
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{0});
        var sizeType=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,All,null,new object[]{Enum.ToObject(enumType,1),width,height,"Phase07 portrait QA"},null);
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
        var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));var prop=view.GetType().GetProperty("selectedSizeIndex",All);
        SessionState.SetInt(Key+"View",(int)prop.GetValue(view));prop.SetValue(view,count-1);view.Focus();
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
        SessionState.SetBool(Key+"Active",false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());Snapshot("edit-after");
        bool unchanged=SessionState.GetString(Key+"ProductionHash","")==Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString();
        File.WriteAllText(Evidence+"/cleanup.json",JObject.FromObject(new{productionPrefsUnchanged=unchanged,playing=EditorApplication.isPlaying,qaMarkers=UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None).Count(m=>m.profileId==Profile),background=PlayerSettings.runInBackground}).ToString());
    }
}
#endif

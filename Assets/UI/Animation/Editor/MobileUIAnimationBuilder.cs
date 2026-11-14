#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MobileUIAnimationBuilder
{
    const string Root="Assets/UI/Animation";
    [MenuItem("TuTien/UI/Animation/Install mobile motion")]
    public static void Install()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.isDirty||scene.path!="Assets/Scenes/SampleScene_MobileWork.unity")throw new InvalidOperationException("Saved mobile scene in Edit mode required");
        if(!File.Exists(MobileUIAnimationChecks.Evidence+"/baseline-profiler.json"))throw new InvalidOperationException("Profile the original UI before installing motion");
        Capture("scene-before");
        EnsureFolder(Root,"Profiles");EnsureFolder(Root,"Clips");
        var canvas=GameObject.Find("Canvas");var director=canvas.GetComponent<MobileUIAnimationDirector>();
        if(director!=null)throw new InvalidOperationException("Motion already installed; preserve existing tuning");
        director=Undo.AddComponent<MobileUIAnimationDirector>(canvas);
        director.modalOpen=Create("ModalOpen",.14f,.98f,1,0,1);
        director.modalClose=Create("ModalClose",.12f,1,.98f,1,0);
        director.sheetOpen=Create("SheetOpen",.20f,1,1,1,1);
        director.sheetClose=Create("SheetClose",.18f,1,1,1,1);
        director.buttonDown=Create("ButtonDown",.07f,1,.97f,1,1);
        director.buttonUp=Create("ButtonUp",.10f,.97f,1,1,1);
        director.rewardReveal=Create("RewardReveal",.32f,.98f,1,0,1);
        director.accent=Create("RarityAccent",.32f,1,1,.55f,1);
        director.progress=Create("ProgressAccent",.18f,1,1,.65f,1);
        EditorUtility.SetDirty(director);AssetDatabase.SaveAssets();Canvas.ForceUpdateCanvases();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Capture("scene-after-install");
    }
    static void EnsureFolder(string parent,string child){if(!AssetDatabase.IsValidFolder(parent+"/"+child))AssetDatabase.CreateFolder(parent,child);}
    static MobileUIMotionPreset Create(string name,float seconds,float fromScale,float toScale,float fromAlpha,float toAlpha)
    {
        string path=Root+"/Profiles/"+name+".asset";
        if(AssetDatabase.LoadMainAssetAtPath(path)!=null)throw new InvalidOperationException("Preserve existing preset: "+path);
        var preset=ScriptableObject.CreateInstance<MobileUIMotionPreset>();preset.name=name;preset.duration=seconds;preset.scaleFrom=fromScale;preset.scaleTo=toScale;preset.alphaFrom=fromAlpha;preset.alphaTo=toAlpha;
        preset.easing=new AnimationCurve(new Keyframe(0,0,3,3),new Keyframe(1,1,0,0));
        var clip=new AnimationClip{name="Preview_"+name,frameRate=60};
        foreach(string axis in new[]{"x","y"})AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(RectTransform),"m_LocalScale."+axis),Curve(seconds,fromScale,toScale));
        AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(CanvasGroup),"m_Alpha"),Curve(seconds,fromAlpha,toAlpha));
        if(name.StartsWith("Sheet"))AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(RectTransform),"m_AnchoredPosition.y"),Curve(seconds,name=="SheetOpen"?-24:0,name=="SheetOpen"?0:-24));
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,Root+"/Clips/Preview_"+name+".anim");preset.previewClip=clip;AssetDatabase.CreateAsset(preset,path);return preset;
    }
    static AnimationCurve Curve(float seconds,float from,float to)=>new AnimationCurve(new Keyframe(0,from,(to-from)*3/seconds,(to-from)*3/seconds),new Keyframe(seconds,to,0,0));
    public static object Capture(string label)
    {
        var scene=EditorSceneManager.GetActiveScene();var nodes=new List<object>();var references=new List<object>();var events=new List<object>();
        foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            var go=t.gameObject;string id=GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
            nodes.Add(new{id,path=AnimationUtility.CalculateTransformPath(t,null),active=go.activeSelf,components=go.GetComponents<Component>().Select(c=>c==null?"MISSING":c.GetType().FullName).ToArray()});
            foreach(var c in go.GetComponents<MonoBehaviour>())
            {
                if(c==null)continue;var it=new SerializedObject(c).GetIterator();
                while(it.Next(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.objectReferenceValue!=null)references.Add(new{owner=id,component=c.GetType().FullName,property=it.propertyPath,target=GlobalObjectId.GetGlobalObjectIdSlow(it.objectReferenceValue).ToString()});
            }
            var b=go.GetComponent<Button>();if(b!=null)for(int i=0;i<b.onClick.GetPersistentEventCount();i++)events.Add(new{owner=id,index=i,target=GlobalObjectId.GetGlobalObjectIdSlow(b.onClick.GetPersistentTarget(i)).ToString(),method=b.onClick.GetPersistentMethodName(i)});
        }
        var result=new{scene=scene.path,nodes,references,events};File.WriteAllText(MobileUIAnimationChecks.Evidence+"/"+label+".json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{nodes=nodes.Count,references=references.Count,events=events.Count};
    }
}
#endif

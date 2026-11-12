#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Repeatable checks against the running Editor, guarded by disposable save isolation.</summary>
[InitializeOnLoad]
public static class JadeUIRuntimeChecks
{
    const string Profile="jade-ui-qa";
    const string Key="JadeUIQA.Active";
    static JadeUIRuntimeChecks(){EditorApplication.playModeStateChanged+=OnState;}
    static void OnState(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredEditMode||!SessionState.GetBool(Key,false))return;
        EditorApplication.delayCall+=Cleanup;
    }
    [MenuItem("TuTien/UI/Jade V2/07 Start isolated QA")]
    public static void StartIsolated()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Already playing.");
        CleanupMarkers();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        var marker=new GameObject("__JadeUI_QA_Temporary");marker.AddComponent<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>().profileId=Profile;
        SessionState.SetBool(Key,true);
        SessionState.SetString("JadeUIQA.SaveHash",Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString());
        EditorApplication.isPlaying=true;
    }
    static void CleanupMarkers()
    {
        foreach(var marker in UnityEngine.Object.FindObjectsByType<KhiVanChiTu.Collaboration.WorkspaceSaveProfile>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
    }
    [MenuItem("TuTien/UI/Jade V2/08 Cleanup QA profile marker")]
    public static void Cleanup()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before cleanup.");
        CleanupMarkers();SessionState.SetBool(Key,false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        string before=SessionState.GetString("JadeUIQA.SaveHash","");
        string after=Hash128.Compute(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")).ToString();
        if(before!=""&&before!=after)throw new InvalidOperationException("Production save hash changed.");
        Debug.Log("[Jade V2 QA] Temporary scene marker removed. Production save key unchanged.");
    }
    public static void RequireIsolation()
    {
        if(!EditorApplication.isPlaying||DataManager.Instance==null||DataManager.Instance.SaveProfileId!=Profile)throw new InvalidOperationException("Checks require the isolated QA profile in Play Mode.");
    }
    public static void CheckRaycast(UnityEngine.UI.Button button)
    {
        if(button==null||!button.gameObject.activeInHierarchy)throw new InvalidOperationException("Button is inactive or missing.");
        var rect=(RectTransform)button.transform;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        if(hits.Count==0||!(hits[0].gameObject.transform==button.transform||hits[0].gameObject.transform.IsChildOf(button.transform)))
            throw new InvalidOperationException(button.name+" is blocked by "+(hits.Count>0?hits[0].gameObject.name:"no raycast surface"));
    }
    [MenuItem("TuTien/UI/Jade V2/QA Check dock and drawer hits")]
    public static void DockHits()
    {
        RequireIsolation();Canvas.ForceUpdateCanvases();
        var drawer=MobileDrawerUIController.Instance;
        foreach(var button in drawer.dockButtons)CheckRaycast(button);
        if(drawer.ActiveDrawerIndex>=0)CheckRaycast(drawer.closeButtons[drawer.ActiveDrawerIndex]);
        File.AppendAllText(JadeUIRedesignAudit.Folder+"/runtime-checks.tsv","PASS\tdock and drawer close GraphicRaycaster hits at "+Screen.width+"x"+Screen.height+"\n");
        Debug.Log("[Jade V2 QA] Dock and active drawer close targets receive raycasts.");
    }
}
#endif

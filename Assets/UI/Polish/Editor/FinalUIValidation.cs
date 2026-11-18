#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KhiVanChiTu.UI.Polish.Editor
{
    public static class FinalUIValidation
    {
        public static string Evidence(string fallback)=>SessionState.GetString("FinalUIQA.EvidenceOverride",fallback);
        public static string Profile(string fallback)=>SessionState.GetString("FinalUIQA.Profile",fallback);
        public static void BeginSuite(string suite)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Wait for previous suite cleanup");
            SessionState.SetString("FinalUIQA.EvidenceOverride","Docs/UI_Redesign/Phase10/Evidence/"+suite);
            SessionState.SetString("FinalUIQA.Profile","final-"+suite+"-"+DateTime.UtcNow.ToString("MMdd-HHmmss"));
            switch(suite)
            {
                case "inventory":KhiVanChiTu.UI.Equipment.Editor.Phase45QA.Begin(4);break;
                case "cultivation":KhiVanChiTu.UI.Equipment.Editor.Phase45QA.Begin(5);break;
                case "progression":KhiVanChiTu.UI.Progression.Editor.ProgressionQA.Begin();break;
                case "realms":KhiVanChiTu.UI.SecretRealm.Editor.RealmQA.Begin();break;
                case "menu-create":KhiVanChiTu.UI.Menu.Editor.MenuQA.Begin();break;
                case "menu-load":KhiVanChiTu.UI.Menu.Editor.MenuQA.Begin(true);break;
                default:throw new ArgumentException(suite);
            }
        }
        public static void ClearOverrides(){SessionState.EraseString("FinalUIQA.EvidenceOverride");SessionState.EraseString("FinalUIQA.Profile");}
        public static void QueueAndroidBuild()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new InvalidOperationException("Saved Android scene in Edit Mode required");
            EditorApplication.delayCall+=()=>
            {
                string path="Docs/UI_Redesign/Phase10/Evidence/android-build.json";Directory.CreateDirectory(Path.GetDirectoryName(path));
                try
                {
                    Directory.CreateDirectory("Temp/Phase10Build");var options=new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.Android,locationPathName="Temp/Phase10Build/UI_Phase10_Validation.apk",options=BuildOptions.None};
                    var report=BuildPipeline.BuildPlayer(options);var summary=report.summary;
                    File.WriteAllText(path,JsonConvert.SerializeObject(new{status=summary.result==BuildResult.Succeeded?"PASS":"FAIL",result=summary.result.ToString(),summary.totalErrors,summary.totalWarnings,summary.totalSize,totalSeconds=summary.totalTime.TotalSeconds,summary.outputPath,steps=report.steps.Select(s=>new{name=s.name,messages=s.messages.Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content).ToArray()}).Where(s=>s.messages.Length>0).ToArray()},Formatting.Indented));
                }
                catch(Exception e){File.WriteAllText(path,JsonConvert.SerializeObject(new{status="FAIL",exception=e.ToString()},Formatting.Indented));}
            };
        }
    }
}
#endif

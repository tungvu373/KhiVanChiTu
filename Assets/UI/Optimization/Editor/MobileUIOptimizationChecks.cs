#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

/// <summary>Matched, advancing-frame Editor measurements. Results are not device FPS or player memory.</summary>
public static partial class MobileUIPhase09Checks
{
    static readonly ProfilerMarker CurrencyMarker=new ProfilerMarker("KV.P10.CurrencyBinding");
    static IEnumerator OptimizationSuite(string label)
    {
        yield return 1.0;
        RequireIsolation();int initialFrame=Time.frameCount;yield return .3;
        Case("advancing-profile","P10","Data/DataManager.cs","Awake",()=>{
            Expect(Time.frameCount>initialFrame,"Frozen game loop");Expect(Screen.width==1080&&Screen.height==1920,"Unexpected raster size");
        });
        if(!(bool)rows.Last["passed"])yield break;
        DataManager.Instance.gameSaveData=new GameSaveData();DataManager.Instance.SaveData();
        foreach(var step in EnterGameplay())yield return step;
        MergeManager.Instance.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false,swords=Enumerable.Range(0,64).Select(i=>new SwordSaveEntry{level=1+i%8,isEquipped=i<10,equipSlotIndex=i<10?i:-1}).ToArray()});
        yield return .5;
        var samples=new JArray();
        foreach(var step in MeasureWindow(label,"hud-idle",0,samples))yield return step;
        MobileDrawerUIController.Instance.OpenDrawer(1);yield return .5;
        var body=BodyTemperingPanelUI.Instance;body.OpenCenterModal(0);yield return .6;
        foreach(var step in MeasureWindow(label,"body-idle",0,samples))yield return step;
        foreach(var step in MeasureWindow(label,"body-currency-10frames",10,samples))yield return step;
        foreach(var step in MeasureWindow(label,"body-currency-every-frame",1,samples))yield return step;
        foreach(var step in MeasureWindow(label,"body-refresh-direct",-1,samples))yield return step;
        body.CloseCenterModal();CloseDrawer();yield return .5;
        StorageModalUI.Open();yield return .8;
        foreach(var step in MeasureWindow(label,"storage-64-idle",0,samples))yield return step;
        StorageModalUI.Instance.Close();yield return .5;
        foreach(var step in MeasureWindow(label,"hud-motion-stress",-2,samples))yield return step;
        yield return .8;
        Case("motion-settles","P10","Assets/UI/Animation/Runtime/MobileUIAnimationDirector.cs","Press,Accent",()=>{
            Expect(UnityEngine.Object.FindObjectsByType<MobileUIVisualScaleMotion>(FindObjectsSortMode.None).All(m=>!m.enabled),"Press motion remains enabled at rest");
            Expect(UnityEngine.Object.FindObjectsByType<MobileUIAccentPulse>(FindObjectsSortMode.None).All(m=>!m.enabled),"Accent remains enabled at rest");
        });
        File.WriteAllText(Evidence+"/"+label+"-profile.json",JObject.FromObject(new{label,utc=DateTime.UtcNow.ToString("o"),profile=Profile,width=Screen.width,height=Screen.height,timeScale=Time.timeScale,editorOnly=true,samples}).ToString());
    }
    static IEnumerable MeasureWindow(string label,string name,int stimulus,JArray results)
    {
        const int count=180;
        var names=new[]{"Main Thread","CPU Main Thread Frame Time","GPU Frame Time","GC Allocated In Frame","Texture Memory","Total Used Memory","GC Used Memory","Canvas.BuildBatch","UGUI.Rendering.UpdateBatches","Canvas.RenderOverlays","SetPass Calls Count","KV.UI.Motion"};
        var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);
        var descriptions=handles.Select(ProfilerRecorderHandle.GetDescription).Where(d=>names.Contains(d.Name)).ToArray();
        var recorders=new List<(ProfilerRecorder recorder,string name,string category,string unit)>();
        var unavailable=new JArray(names.Where(n=>!descriptions.Any(d=>d.Name==n)));
        foreach(var d in descriptions)
        {
            var recorder=ProfilerRecorder.StartNew(d.Category,d.Name,1024);
            recorders.Add((recorder,d.Name,d.Category.Name,d.UnitType.ToString()));
        }
        var cpu=new List<double>(count);var allocated=new List<long>(count);var frameGaps=new List<int>(count);
        var graphics=UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None).Where(g=>g.isActiveAndEnabled).ToArray();
        int dirtyLayout=0,dirtyVertices=0;
        UnityEngine.Events.UnityAction onLayout=()=>dirtyLayout++,onVertices=()=>dirtyVertices++;
        foreach(var g in graphics){g.RegisterDirtyLayoutCallback(onLayout);g.RegisterDirtyVerticesCallback(onVertices);}
        double coverage=0;var corners=new Vector3[4];
        foreach(var g in graphics)
        {
            if(g.canvasRenderer.cull||g.canvasRenderer.GetInheritedAlpha()<.001f)continue;
            g.rectTransform.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            coverage+=Math.Max(0,Math.Min(Screen.width,Math.Max(a.x,b.x))-Math.Max(0,Math.Min(a.x,b.x)))*Math.Max(0,Math.Min(Screen.height,Math.Max(a.y,b.y))-Math.Max(0,Math.Min(a.y,b.y)))/(Screen.width*(double)Screen.height);
        }
        int first=Time.frameCount,last=first;double deadline=UnityEditor.EditorApplication.timeSinceStartup+45;
        var body=BodyTemperingPanelUI.Instance;var director=UnityEngine.Object.FindFirstObjectByType<MobileUIAnimationDirector>();
        var dock=MobileDrawerUIController.Instance.dockButtons[0].transform as RectTransform;var rest=dock.localScale;
        try
        {
            int collected=0;
            while(collected<count)
            {
                if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new InvalidOperationException("Profiling window did not advance: "+name);
                if(Time.frameCount==last){yield return 0.0;continue;}
                frameGaps.Add(Time.frameCount-last);last=Time.frameCount;
                bool eventDue=stimulus>0&&collected%stimulus==0;
                if(eventDue||stimulus==-1)
                {
                    long bytes=GC.GetAllocatedBytesForCurrentThread(),ticks=Stopwatch.GetTimestamp();
                    using(CurrencyMarker.Auto()){if(eventDue)EconomyManager.Instance.AddLinhThach(128);else body.Refresh();}
                    cpu.Add((Stopwatch.GetTimestamp()-ticks)*1000.0/Stopwatch.Frequency);allocated.Add(GC.GetAllocatedBytesForCurrentThread()-bytes);
                }
                if(stimulus==-2&&collected%12==0){director.Press(dock,rest,collected%24==0);director.Accent(dock.GetComponent<Graphic>());}
                collected++;yield return 0.0;
            }
        }
        finally
        {
            foreach(var g in graphics)if(g!=null){g.UnregisterDirtyLayoutCallback(onLayout);g.UnregisterDirtyVerticesCallback(onVertices);}
            foreach(var r in recorders)r.recorder.Stop();
        }
        var counters=new JArray();
        foreach(var r in recorders)
        {
            var buffer=new List<ProfilerRecorderSample>();r.recorder.CopyTo(buffer);
            counters.Add(JObject.FromObject(new{r.name,r.category,r.unit,valid=r.recorder.Valid,stats=Stats(buffer.Select(s=>(double)s.Value)),values=buffer.Select(s=>s.Value).ToArray()}));r.recorder.Dispose();
        }
        var row=JObject.FromObject(new{name,firstFrame=first,lastFrame=Time.frameCount,observedFrames=frameGaps.Count,frameGaps,stimulus,callbackMs=Stats(cpu),callbackAllocatedBytes=Stats(allocated.Select(v=>(double)v)),callbackValuesMs=cpu,callbackAllocatedValues=allocated,dirtyLayout,dirtyVertices,activeGraphics=graphics.Length,activeRaycastTargets=graphics.Count(g=>g!=null&&g.raycastTarget),rectangleCoverageUpperBound=coverage,memory=new{allocated=Profiler.GetTotalAllocatedMemoryLong(),reserved=Profiler.GetTotalReservedMemoryLong(),monoUsed=Profiler.GetMonoUsedSizeLong()},counters,unavailable});
        results.Add(row);File.WriteAllText(Evidence+"/"+label+"-profile-progress.json",results.ToString());
    }
    static object Stats(IEnumerable<double> source)
    {
        var v=source.OrderBy(x=>x).ToArray();return new{count=v.Length,nonzero=v.Count(x=>x!=0),min=v.Length==0?0:v[0],median=v.Length==0?0:v[v.Length/2],p95=v.Length==0?0:v[Math.Min(v.Length-1,(int)Math.Ceiling(v.Length*.95)-1)],max=v.Length==0?0:v[v.Length-1],mean=v.Length==0?0:v.Average()};
    }
    static IEnumerator OptimizationRegressions()
    {
        foreach(var step in Bootstrap())yield return step;foreach(var step in EnterGameplay())yield return step;
        MobileDrawerUIController.Instance.OpenDrawer(1);yield return .4;
        var body=BodyTemperingPanelUI.Instance;
        foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(1080,2400)})
        {
            SetResolution(size.x,size.y);yield return .3;
            for(int node=0;node<6;node++)
            {
                int index=node;body.OpenCenterModal(node);yield return .25;
                Case($"p10-{size.x}x{size.y}-node-{node}","BODY_DETAIL","UI/BodyTemperingPanelUI.cs","Refresh,OpenCenterModal",()=>{
                    Expect(!body.txtModalTitle.text.Contains("<size"),"Cached title restored tiny rich-text sizing");
                    Expect(body.txtModalTitle.text.Contains(PlayerStatsService.TalentNodes[index].title),"Title stale after node switch");
                    Expect(body.txtModalMechanics.text.Contains(PlayerStatsService.TalentNodes[index].combatMechanics),"Mechanics stale after node switch");
                    Expect(body.txtModalMechanics.rectTransform.rect.height>=body.txtModalMechanics.preferredHeight,"Changed node mechanics clips");
                    Expect(!PixelRect(body.txtModalCurrent.rectTransform).Overlaps(PixelRect(body.txtModalNext.rectTransform)),"Comparison rows overlap");
                    Expect(Contains(Screen.safeArea,PixelRect((RectTransform)body.btnModalUpgrade.transform)),"Primary action outside safe area");
                    for(int j=0;j<30;j++)EconomyManager.Instance.AddLinhThach(128);
                    Expect(!body.txtModalTitle.text.Contains("<size"),"Currency event regressed title");
                    HitClick(body.btnModalUpgrade);Expect(body.txtModalCurrent.text.Contains("Cấp "+PlayerStatsService.Instance.GetLevel(index)),"Upgrade left current value stale");
                    Note("height",body.txtModalMechanics.rectTransform.rect.height);Note("node",index);
                });
                if(node==5){ScreenCapture.CaptureScreenshot(Path.GetFullPath(Evidence+$"/p10-{size.x}x{size.y}-body-node5.png"));yield return .15;}
                body.CloseCenterModal();
            }
        }
        CloseDrawer();StorageModalUI.Open();yield return .5;
        Case("p10-storage-decorative-raycasts","STORAGE","UI/StorageModalUI.cs","CreateGridSlot,CreateGenericCard",()=>{
            var decoration=StorageModalUI.Instance.GetComponentsInChildren<Image>(true).Where(i=>i.name=="CountBadge"||i.name=="Ribbon"||i.name=="IconBadge").ToArray();
            Expect(decoration.Length>0,"Fixture did not produce inventory badges");Expect(decoration.All(i=>!i.raycastTarget),"Decorative badge remains a raycast target");
            var item=StorageModalUI.Instance.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="GridSlot"&&b.IsInteractable());
            Expect(item!=null,"No inventory item action");scrollIntoView(item);HitClick(item);
            Expect(!DismantleConfirmModalUI.Instance.IsModalOpen,"Locked dismantle unexpectedly opened confirmation");
            DataManager.Instance.gameSaveData.slots[0].monsterSwordsAcquired=500;
            HitClick(item);Expect(DismantleConfirmModalUI.Instance.IsModalOpen,"Unlocked inventory click did not route to confirmation");
            Click(DismantleConfirmModalUI.Instance.btnCancel);Expect(!DismantleConfirmModalUI.Instance.IsModalOpen,"Cancel did not close confirmation");
            Note("decorativeTargetsDisabled",decoration.Length);
        });
        yield return .2;
    }
    static void scrollIntoView(Button button)
    {
        // Target the first visible clickable slot without changing inventory data.
        var scroll=StorageModalUI.Instance.scrollRect;scroll.verticalNormalizedPosition=1;Canvas.ForceUpdateCanvases();
    }
}
#endif

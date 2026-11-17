using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.UI.Foundation.Editor
{
    /// <summary>Runs only in the isolated showcase, never in an existing gameplay scene.</summary>
    public static class FoundationValidation
    {
        const string Output = "Assets/.docs/UIUX_Phase1_20261117/Evidence";
        static readonly Vector2Int[] Sizes = {new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)};
        static readonly List<string> results = new List<string>();
        static FoundationShowcase demo;
        static EditorWindow gameView;
        static object group;
        static PropertyInfo selected;
        static int previousIndex, index, phase, firstFrame;
        static double deadline;
        static bool running;
        static readonly List<int> addedSizes = new List<int>();
        [MenuItem("TuTien/UI Foundation/3. Validate and Capture Three Sizes")]
        public static void Run()
        {
            if(running)throw new InvalidOperationException("Foundation validation already running.");
            if(!EditorApplication.isPlaying || SceneManager.sceneCount!=1 || SceneManager.GetActiveScene().path!=FoundationBuilder.ScenePath)
                throw new InvalidOperationException("Open only UIFoundationShowcase, enter Play Mode, then run validation.");
            demo=UnityEngine.Object.FindAnyObjectByType<FoundationShowcase>();
            if(demo==null)throw new InvalidOperationException("Showcase component not found.");
            // A dependency accidentally added to the demo must never start a production transaction.
            foreach(var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if(behaviour==null)continue;
                var ns=behaviour.GetType().Namespace??"";
                if(!ns.StartsWith("KhiVanChiTu.UI.Foundation")&&!ns.StartsWith("UnityEngine")&&!ns.StartsWith("TMPro")&&!ns.StartsWith("UnityEditor"))
                    throw new InvalidOperationException("Unexpected behaviour in isolated showcase: "+behaviour.GetType().FullName);
            }
            var asm=typeof(UnityEditor.Editor).Assembly;var sizeType=asm.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizeType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
            var groupType=sizeType.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(sizes);
            group=sizeType.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});
            gameView=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));
            selected=gameView.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            previousIndex=(int)selected.GetValue(gameView);
            Directory.CreateDirectory(Output);results.Clear();index=0;phase=0;running=true;
            try { SelectSize(Sizes[0]); EditorApplication.update+=Tick; }
            catch { Cleanup();throw; }
        }
        static void SelectSize(Vector2Int size)
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var st=asm.GetType("UnityEditor.GameViewSize");var et=asm.GetType("UnityEditor.GameViewSizeType");
            var entry=Activator.CreateInstance(st,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(et,1),size.x,size.y,"Foundation QA "+size.x+"x"+size.y},null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{entry});
            int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);addedSizes.Add(count-1);
            selected.SetValue(gameView,count-1);gameView.Focus();firstFrame=Time.frameCount;deadline=EditorApplication.timeSinceStartup+15;
        }
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying||demo==null)throw new InvalidOperationException("Showcase Play Mode ended during validation.");
                EditorApplication.QueuePlayerLoopUpdate();gameView.Repaint();
                var size=Sizes[index];
                if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Game View did not advance at "+size);
                if(Screen.width!=size.x||Screen.height!=size.y||Time.frameCount<firstFrame+8)return;
                if(phase==0)
                {
                    CheckInteractions();CheckGeometry();
                    demo.root.ApplySafeArea(new Rect(0,size.y*.03f,size.x,size.y*.92f),size);Canvas.ForceUpdateCanvases();CheckSafeRect();
                    demo.root.ApplySafeArea(Screen.safeArea,size);Canvas.ForceUpdateCanvases();
                    phase=10;firstFrame=Time.frameCount;return;
                }
                if(phase==10)
                {
                    CheckGeometry();
                    ScreenCapture.CaptureScreenshot(Output+"/gallery-"+size.x+"x"+size.y+".png");
                    phase=1;firstFrame=Time.frameCount;return;
                }
                if(phase==1)
                {
                    demo.GetComponentInChildren<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition=0;
                    phase=11;firstFrame=Time.frameCount;return;
                }
                if(phase==11)
                {
                    ScreenCapture.CaptureScreenshot(Output+"/controls-"+size.x+"x"+size.y+".png");
                    phase=12;firstFrame=Time.frameCount;return;
                }
                if(phase==12)
                {
                    demo.confirm.Show("X\u00e1c nh\u1eadn",FoundationShowcase.LongCopy);Canvas.ForceUpdateCanvases();
                    CheckGeometry();phase=2;firstFrame=Time.frameCount;return;
                }
                if(phase==2)
                {
                    CheckModalRaycast();
                    ScreenCapture.CaptureScreenshot(Output+"/dialog-"+size.x+"x"+size.y+".png");phase=3;firstFrame=Time.frameCount;return;
                }
                if(phase==3)
                {
                    demo.root.Back();
                    results.Add("{\"width\":"+size.x+",\"height\":"+size.y+",\"framesAdvanced\":"+(Time.frameCount-firstFrame)+",\"checks\":\"passed\"}");
                    index++;if(index<Sizes.Length){phase=0;SelectSize(Sizes[index]);}else Finish();
                }
            }
            catch(Exception e)
            {
                File.WriteAllText(Output+"/validation-failure.txt",e.ToString());Debug.LogException(e);Cleanup();
            }
        }
        static void Expect(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
        static void CheckInteractions()
        {
            demo.Wire();demo.secondary.Execute();int start=demo.DemoValue;
            demo.primary.Execute();Expect(demo.DemoValue==(start+14)%101,"Primary command did not execute exactly once.");
            demo.primary.SetAvailable(false);start=demo.DemoValue;demo.primary.Execute();Expect(start==demo.DemoValue,"Disabled button executed.");demo.primary.SetAvailable(true);
            demo.progress.Set(0,100);Expect(demo.progress.Normalized==0,"0% progress");demo.progress.Set(100,100);Expect(demo.progress.Normalized==1,"100% progress");demo.progress.Set(1,0);Expect(demo.progress.Normalized==0,"Zero denominator");
            demo.tabs.Select(2);Expect(demo.tabs.SelectedIndex==2,"Tab selection");demo.tabs.Select(0);
            bool selectedBefore=demo.card.IsSelected;demo.card.action.Execute();Expect(demo.card.IsSelected!=selectedBefore,"Item selection");
            int accepted=0;demo.fullScreen.Show("Screen",FoundationShowcase.LongCopy);demo.confirm.Show("Confirm",FoundationShowcase.LongCopy,()=>accepted++);
            Expect(demo.root.OpenPanelCount==2,"Modal stack depth");demo.root.Back();Expect(accepted==0&&!demo.confirm.IsOpen&&demo.fullScreen.IsOpen,"Back invoked confirm or closed wrong layer");
            demo.root.Back();demo.confirm.Show("Confirm",FoundationShowcase.LongCopy,()=>accepted++);demo.confirm.confirm.Execute();Expect(accepted==1,"Confirmation not exactly once");
            demo.sheet.Show("Sheet",FoundationShowcase.LongCopy);demo.root.Back();Expect(!demo.sheet.IsOpen,"Bottom sheet Back");
            demo.reward.Show("Reward","Sample",()=>accepted++);demo.reward.confirm.Execute();Expect(accepted==2,"Reward callback");
            demo.toast.Show("Toast",.1f);Expect(demo.toast.gameObject.activeSelf,"Toast Show");demo.toast.Hide();Expect(!demo.toast.gameObject.activeSelf,"Toast Hide");
            demo.tooltip.Show("Tooltip",0);demo.tooltip.Hide();demo.secondary.Execute();demo.tabs.Select(0);
            var scroll=demo.GetComponentInChildren<UnityEngine.UI.ScrollRect>();if(scroll!=null)scroll.verticalNormalizedPosition=1;
        }
        static void CheckSafeRect()
        {
            var safe=demo.root.safeArea;Expect(safe.anchorMin.y>.02f&&safe.anchorMax.y<.96f,"Simulated notch/gesture insets not applied");
        }
        static void CheckGeometry()
        {
            Canvas.ForceUpdateCanvases();
            foreach(var text in demo.GetComponentsInChildren<TMP_Text>(false))
            {
                text.ForceMeshUpdate();Expect(text.font!=null,"Unassigned font: "+text.name);
                Expect(!text.isTextOverflowing,"Overflow: "+text.name+" / "+text.text);
            }
            foreach(var scroll in demo.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(false))
                Expect(scroll.content!=null&&scroll.viewport!=null&&scroll.viewport.rect.width>0&&scroll.viewport.rect.height>0,"Invalid ScrollRect geometry: "+scroll.name);
            foreach(var button in demo.GetComponentsInChildren<UnityEngine.UI.Button>(false))
                Expect(((RectTransform)button.transform).rect.width>0&&((RectTransform)button.transform).rect.height>0,"Zero-size button "+button.name);
        }
        static void CheckModalRaycast()
        {
            var pointer=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Expect(hits.Count>0&&hits[0].gameObject.transform.IsChildOf(demo.confirm.transform),"Pointer raycast passed through modal. Hits: "+string.Join(",",hits.Select(h=>h.gameObject.name))+"; modal active="+demo.confirm.gameObject.activeInHierarchy+" blocks="+demo.confirm.GetComponent<CanvasGroup>().blocksRaycasts);
            Expect(!demo.root.gameplayHUD.GetComponent<CanvasGroup>().blocksRaycasts,"HUD not blocked by modal");
        }
        static void Finish()
        {
            File.WriteAllText(Output+"/validation-results.json","{\"environment\":\"Unity Editor Game View\",\"unity\":\""+Application.unityVersion+"\",\"results\":["+string.Join(",",results)+"]}");
            Debug.Log("[Foundation] Runtime interactions, geometry, modal raycast and three Game View sizes passed. Screenshots requested; verify saved image dimensions.");Cleanup();
        }
        static void Cleanup()
        {
            EditorApplication.update-=Tick;running=false;
            if(demo!=null){while(demo.root.Back()){}demo.root.ApplySafeArea(Screen.safeArea,new Vector2(Screen.width,Screen.height));}
            if(gameView!=null&&selected!=null)selected.SetValue(gameView,previousIndex);
            // Remove only entries this run added, in reverse order. Preserve user Game View presets.
            if(group!=null)
            {
                for(int i=addedSizes.Count-1;i>=0;i--)group.GetType().GetMethod("RemoveCustomSize").Invoke(group,new object[]{addedSizes[i]});
            }
            addedSizes.Clear();
        }
    }
}

using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Polish
{
    /// <summary>One non-blocking, unscaled notification. Commands and state remain with existing services.</summary>
    public sealed class GlobalFeedbackHost : MonoBehaviour
    {
        public FoundationNotice notice;
        public FoundationTheme theme;
        static GlobalFeedbackHost instance;
        FeatureUnlockService unlocks;
        CultivationManager cultivation;
        bool sessionReady;
        readonly bool[] known=new bool[7];
        static readonly string[] Names={"Kiếm Lô","Thiên Phú Đạo Vận","Luyện Đan","Trận Pháp","Pháp Khí","Luyện Thể","Phân Giải"};
        public static string LastMessage=>instance!=null?instance.notice.text.text:"";
        void Awake(){instance=this;}
        void Start()
        {
            unlocks=FeatureUnlockService.Instance;cultivation=CultivationManager.Instance;
            sessionReady=DataManager.Instance!=null&&DataManager.Instance.isSessionActive;
            if(unlocks!=null){Snapshot(false);unlocks.OnUnlockStateChanged+=Unlocked;}
            if(cultivation!=null)cultivation.OnStageChanged+=StageChanged;
            if(!sessionReady)StartCoroutine(WaitForSession());
        }
        System.Collections.IEnumerator WaitForSession()
        {
            while(DataManager.Instance==null||!DataManager.Instance.isSessionActive)yield return null;
            if(unlocks!=null)Snapshot(false);sessionReady=true;
        }
        void OnDestroy(){if(unlocks!=null)unlocks.OnUnlockStateChanged-=Unlocked;if(cultivation!=null)cultivation.OnStageChanged-=StageChanged;if(instance==this)instance=null;}
        void Unlocked()=>Snapshot(true);
        void StageChanged(CultivationStageData stage)
        {if(unlocks!=null)Snapshot(sessionReady);sessionReady=DataManager.Instance!=null&&DataManager.Instance.isSessionActive;}
        void Snapshot(bool announce)
        {for(int i=0;i<known.Length;i++){bool now=unlocks.IsFeatureUnlocked((FeatureId)i);if(announce&&now&&!known[i])Notify("Đã mở khóa · "+Names[i]);known[i]=now;}}
        public static void Notify(string message,bool failure=false)
        {
            if(instance==null||string.IsNullOrWhiteSpace(message))return;
            var n=instance.notice;n.text.color=failure?instance.theme.danger:instance.theme.ivory;
            n.Show(message,2.6f);Canvas.ForceUpdateCanvases();
            float height=n.text.GetPreferredValues(message,Mathf.Max(160,n.text.rectTransform.rect.width),10000).y+48;
            var r=(RectTransform)n.transform;r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Max(112,height));
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.Progression
{
    public enum ProgressionState { Locked, InProgress, Claimable, Available, Completed }
    /// <summary>A live presentation row, with no owned currency, rewards, or progression state.</summary>
    public sealed class ProgressionCard : MonoBehaviour
    {
        public RectTransform header;
        public TMP_Text title,caption,description,status,effect,requirements,reward;
        public GameObject rewardSurface;
        public UnityEngine.UI.Graphic icon;
        public UnityEngine.UI.Image accent;
        public FoundationProgress progress;
        public FoundationAction action;
        public string BoundId {get;private set;}
        public ProgressionState State {get;private set;}
        public void Bind(string id,string name,string detail,string body,string stateText,string effects,string conditions,string rewards,
            ProgressionState state,float current,float maximum,string progressText,string button,bool actionable,Action execute,FoundationTheme theme)
        {
            BoundId=id;State=state;title.text=name;caption.text=detail;description.text=body;status.text=stateText;effect.text=effects;requirements.text=conditions;reward.text=rewards;
            description.gameObject.SetActive(!string.IsNullOrEmpty(body));effect.gameObject.SetActive(!string.IsNullOrEmpty(effects));requirements.gameObject.SetActive(!string.IsNullOrEmpty(conditions));rewardSurface.SetActive(!string.IsNullOrEmpty(rewards));
            Color tone=actionable?theme.gold:state==ProgressionState.Completed?theme.jade:state==ProgressionState.Locked?theme.muted:theme.ivory;
            accent.color=tone;status.color=tone;icon.color=tone;
            action.Bind(button,execute,actionable);action.button.targetGraphic.color=actionable?theme.gold:theme.raised;action.label.color=actionable?theme.ink:theme.muted;
            progress.Set(current,maximum,progressText);
            float width=Mathf.Max(200,header.rect.width-160);
            float h=Mathf.Max(64,title.GetPreferredValues(name,width,10000).y+8);
            RpgUI.Top(title.rectTransform,0,h,144,0);RpgUI.Top(caption.rectTransform,h+4,48,144,0);RpgUI.Height(header,Mathf.Max(128,h+56));
        }
    }
}

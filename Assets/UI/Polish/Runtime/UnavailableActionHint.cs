using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Polish
{
    [DisallowMultipleComponent]
    public sealed class UnavailableActionHint : MonoBehaviour,IPointerDownHandler
    {
        UnityEngine.UI.Button button;
        void Awake(){button=GetComponent<UnityEngine.UI.Button>();}
        public void OnPointerDown(PointerEventData data)
        {
            if(button==null||button.IsInteractable()||data.button!=PointerEventData.InputButton.Left)return;
            var cultivation=GetComponentInParent<KhiVanChiTu.UI.Cultivation.CultivationScreen>();
            var card=GetComponentInParent<KhiVanChiTu.UI.Progression.ProgressionCard>();
            var work=GetComponentInParent<KhiVanChiTu.UI.Equipment.MergeWorkbench>();
            string reason=cultivation!=null?cultivation.condition.text:card!=null?card.status.text+"\n"+card.requirements.text:work!=null?work.preview.text:null;
            if(!string.IsNullOrWhiteSpace(reason))GlobalFeedbackHost.Notify(reason,true);
        }
    }
}

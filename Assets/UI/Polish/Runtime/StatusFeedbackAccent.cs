using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Polish
{
    [DisallowMultipleComponent]
    public sealed class StatusFeedbackAccent : MonoBehaviour
    {
        TMP_Text label;string previous;
        void Awake(){label=GetComponent<TMP_Text>();}
        void OnEnable(){if(label!=null)previous=label.text;}
        void LateUpdate()
        {
            if(label==null||previous==label.text)return;previous=label.text;if(string.IsNullOrWhiteSpace(previous))return;
            MobileUIAnimationDirector.For(this)?.Accent(label);
            bool failure=previous.StartsWith("Không")||previous.StartsWith("Chưa")||previous.StartsWith("Cần")||previous.StartsWith("Điều kiện");
            GlobalFeedbackHost.Notify(previous,failure);
        }
    }
}

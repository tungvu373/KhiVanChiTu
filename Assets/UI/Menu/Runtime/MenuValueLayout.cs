using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuValueLayout : MonoBehaviour
    {
        public FoundationValue row;
        public bool stacked;
        void LateUpdate()
        {
            float w=Mathf.Max(160,row.value.rectTransform.rect.width);float h=row.value.GetPreferredValues(row.value.text,w,10000).y;
            float height=Mathf.Max(stacked?160:132,h+(stacked?72:40));var le=GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null&&Mathf.Abs(le.preferredHeight-height)>1)le.minHeight=le.preferredHeight=height;
            if(stacked)RpgUI.Top(row.value.rectTransform,56,Mathf.Max(64,h),24,24);
        }
    }
}

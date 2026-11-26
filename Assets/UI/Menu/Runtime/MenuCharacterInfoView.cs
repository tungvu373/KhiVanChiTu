using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuCharacterInfoView : MonoBehaviour
    {
        public CharacterDetailModalUI owner;
        public TMP_Text title,realm;
        public FoundationValue[] rows;
        public UnityEngine.UI.Text[] sources;
        public string[] captions;
        public FoundationAction close;
        public UnityEngine.UI.ScrollRect scroll;
        public RectTransform identity;
        float next;
        void Awake(){close.Bind("Đóng",()=>owner.ClosePanel());}
        void LateUpdate(){if(identity!=null)KhiVanChiTu.UI.Equipment.RpgUI.Height(identity,Mathf.Max(320,UnityEngine.UI.LayoutUtility.GetPreferredHeight(title.transform.parent as RectTransform)+16));if(Time.unscaledTime<next)return;next=Time.unscaledTime+.4f;owner.RefreshData();}
        public void Sync()
        {title.text=MenuCopy.Value(owner.txtName?.text);realm.text=MenuCopy.Value(owner.txtRealm?.text);for(int i=0;i<rows.Length;i++)rows[i].Set(captions[i],MenuCopy.Value(sources[i]?.text));}
    }
}

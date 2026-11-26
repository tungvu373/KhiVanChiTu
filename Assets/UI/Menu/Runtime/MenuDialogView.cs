using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuDialogView : MonoBehaviour
    {
        public FoundationTheme theme;
        public UnityEngine.UI.Text sourceTitle,sourceMessage;
        public TMP_Text title,message,severity;
        public UnityEngine.UI.Button confirm;
        public MenuButtonLabel confirmCaption;
        public MenuPresentation menu;
        void OnEnable(){Refresh();}
        void LateUpdate(){Refresh();}
        void Refresh()
        {
            title.text=MenuCopy.Plain(sourceTitle?.text);bool destructive=title.text.ToLowerInvariant().Contains("xóa");
            message.text=(destructive&&menu!=null&&!string.IsNullOrEmpty(menu.SelectedName)?"Nhân vật: "+menu.SelectedName+"\n\n":"")+MenuCopy.Plain(sourceMessage?.text);
            severity.text=destructive?"HÀNH ĐỘNG KHÔNG THỂ HOÀN TÁC":"XÁC NHẬN HÀNH ĐỘNG";severity.color=destructive?theme.danger:theme.gold;
            confirm.targetGraphic.color=destructive?theme.danger:theme.gold;confirmCaption.caption=destructive?"Xóa nhân vật":"Xác nhận";confirmCaption.Refresh();
        }
    }
}

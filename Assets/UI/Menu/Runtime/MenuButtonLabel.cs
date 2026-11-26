using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuButtonLabel : MonoBehaviour
    {
        public UnityEngine.UI.Text source;
        public TMP_Text label;
        public string caption;
        void LateUpdate(){Refresh();}
        public void Refresh(){if(label!=null)label.text=string.IsNullOrEmpty(caption)&&source!=null?MenuCopy.Plain(source.text):caption;}
    }
}

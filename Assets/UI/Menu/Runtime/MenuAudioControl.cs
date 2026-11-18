using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuAudioControl : MonoBehaviour
    {
        public UnityEngine.UI.Slider slider;
        public TMP_Text value;
        void OnEnable(){Refresh();}
        void LateUpdate(){Refresh();}
        void Refresh(){value.text=Mathf.RoundToInt(slider.value*100)+"%";}
    }
}

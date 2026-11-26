using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuPauseView : MonoBehaviour
    {
        public PauseMenuManager owner;
        public RectTransform mainPage,settingsPage;
        public TMP_Text title,character,status,muteState;
        public FoundationAction audio,back;
        public UnityEngine.UI.ScrollRect mainScroll,settingsScroll;
        void Awake(){audio.Bind("Âm thanh & cài đặt",ShowSettings);back.Bind("Trở lại menu",ShowMain);}
        void OnEnable(){ShowMain();}
        public void ShowSettings(){mainPage.gameObject.SetActive(false);settingsPage.gameObject.SetActive(true);title.text="Cài Đặt Âm Thanh";Canvas.ForceUpdateCanvases();settingsScroll.verticalNormalizedPosition=1;}
        public void ShowMain(){mainPage.gameObject.SetActive(true);settingsPage.gameObject.SetActive(false);title.text="Menu Trong Trận";Canvas.ForceUpdateCanvases();mainScroll.verticalNormalizedPosition=1;}
        void LateUpdate()
        {character.text=MenuCopy.Plain(owner.txtCharacterInfo?.text);status.text=MenuCopy.Plain(owner.txtSaveStatus?.text);muteState.text=owner.muteToggle!=null&&owner.muteToggle.isOn?"ĐÃ TẮT TIẾNG":"ĐANG BẬT ÂM THANH";}
    }
}

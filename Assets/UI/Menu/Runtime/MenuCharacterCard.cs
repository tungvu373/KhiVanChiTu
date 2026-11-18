using System;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuCharacterCard : MonoBehaviour
    {
        public FoundationTheme theme;
        public MenuPresentation menu;
        public SaveSlotUI native;
        public TMP_Text nameLabel,realm,history,slot,state;
        public RectTransform details;
        public UnityEngine.UI.Image portrait,accent;
        public FoundationAction enter;
        public int index;
        public string CharacterName {get;private set;}
        CharacterSaveData data;
        void Awake(){enter.Bind("",()=>native.slotButton.onClick.Invoke());}
        public void Bind(CharacterSaveData value,int slotIndex)
        {
            data=value;index=slotIndex;CharacterName=data.isSlotEmpty?"Chưa có nhân vật":data.characterName;
            native.slotButton.onClick.AddListener(()=>menu.Select(index));
            if(!data.isSlotEmpty)native.deleteButton.onClick.AddListener(()=>menu.Select(index));
            Refresh();Mark(menu.Selected==index);
        }
        void Refresh()
        {
            slot.text="ĐẠO LỘ 0"+(index+1);nameLabel.text=CharacterName;
            realm.text=data.isSlotEmpty?"Bắt đầu từ Luyện Khí":CultivationDisplay.NameForStage(data.stageIndex)+$"\nTầng chiến đấu {Mathf.Max(1,data.combatStage):N0}";
            double hours=Math.Max(0,data.playTime)/3600d;
            history.text=data.isSlotEmpty?"Tạo một đạo hiệu và bước vào tiên giới.":$"Đã tu luyện {Math.Floor(hours):N0} giờ {Math.Floor((hours%1)*60):00} phút\nLần chơi: {data.lastPlayedDate}";
            enter.label.text=data.isSlotEmpty?"Tạo nhân vật":"Tiếp tục đạo lộ";portrait.color=data.isSlotEmpty?new Color(1,1,1,.3f):Color.white;
        }
        public void Mark(bool selected)
        {if(accent==null)return;accent.color=selected?theme.gold:theme.raised;state.text=selected?"ĐANG CHỌN":"";}
        void LateUpdate()
        {
            if(data==null)return;float h=Mathf.Max(480,UnityEngine.UI.LayoutUtility.GetPreferredHeight(details)+224);var le=GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null&&Mathf.Abs(le.preferredHeight-h)>1)le.minHeight=le.preferredHeight=h;
        }
    }
}

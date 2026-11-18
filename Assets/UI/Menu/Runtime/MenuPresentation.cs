using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Menu
{
    public sealed class MenuPresentation : MonoBehaviour
    {
        public MainMenuManager owner;
        public TMP_Text continueHint,creationHint;
        public UnityEngine.UI.Text legacyCreationHint;
        public MenuCharacterCard[] cards;
        public UnityEngine.UI.ScrollRect selectionScroll;
        public int Selected {get;private set;}=-1;
        public string SelectedName=>Selected>=0&&Selected<cards.Length?cards[Selected].CharacterName:"";
        void OnEnable()
        {if(DataManager.Instance!=null)Select(DataManager.Instance.gameSaveData.lastPlayedSlotIndex);}
        public void Select(int slot){Selected=slot;foreach(var c in cards)if(c!=null)c.Mark(c.index==slot);}
        void LateUpdate()
        {
            var data=DataManager.Instance?.gameSaveData;
            if(data!=null)
            {
                if(Selected<0)Select(data.lastPlayedSlotIndex);
                int index=data.lastPlayedSlotIndex;
                continueHint.text=index>=0&&index<data.slots.Length&&!data.slots[index].isSlotEmpty?"Lần trước: "+data.slots[index].characterName+"\nChọn nhân vật để tiếp tục đạo lộ":"Một đạo hiệu, một hành trình tu tiên.";
            }
            if(legacyCreationHint!=null)creationHint.text=MenuCopy.Plain(legacyCreationHint.text);
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Equipment
{
    public sealed class ItemDetailSheet : MonoBehaviour
    {
        public FoundationTheme theme;
        public TMP_Text title,rarity,description,feedback;
        public UnityEngine.UI.Image art;
        public FoundationAction close,equip,dismantle,selectMerge;
        public UnityEngine.UI.ScrollRect scroll;
        MergeItem sword;
        Action remove;
        public static ItemDetailSheet Find(Component source)
        {foreach(var s in UnityEngine.Object.FindObjectsByType<ItemDetailSheet>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(s.gameObject.scene==source.gameObject.scene)return s;return null;}
        void Awake(){close.Bind("Đóng",Close);}
        public void Close(){gameObject.SetActive(false);sword=null;remove=null;}
        public void Show(string name,string quality,string body,Sprite image,Color color,Action onRemove=null,MergeItem item=null)
        {
            var generated=item==null?KhiVanChiTu.UI.GameIcons.GameIconCatalogue.Current?.Item(name,quality):null;
            if(generated!=null)image=generated;
            sword=item;remove=onRemove;title.text=name;rarity.text=quality;rarity.color=color;description.text=name+"\n\n"+body;feedback.text="";art.sprite=image;art.enabled=image!=null;art.color=Color.white;art.preserveAspect=true;RpgEmblem.Hide(art);if(generated==null){if(quality=="Dược liệu")RpgEmblem.Show(art,RpgEmblemKind.Herb,theme.jade);else if(quality.Contains("phẩm")||quality=="Phế đan")RpgEmblem.Show(art,RpgEmblemKind.Pill,color);}
            gameObject.SetActive(true);transform.SetAsLastSibling();
            RpgUI.Button(dismantle,"Phân giải",remove!=null&& (FeatureUnlockService.Instance==null||FeatureUnlockService.Instance.IsDismantleUnlocked()),()=>{var action=remove;Close();action?.Invoke();});
            selectMerge.gameObject.SetActive(item!=null&&MergeManager.Instance!=null&&!MergeManager.Instance.IsSwordEquipped(item));
            selectMerge.Bind(item!=null&&item.level>=SwordRules.MaxLevel?"Đã đạt cấp tối đa":"Chọn ghép",()=>{if(sword!=null)MergeWorkbench.Find(this)?.Select(sword);Close();},item!=null&&item.level<SwordRules.MaxLevel);
            RefreshEquip();Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;
        }
        public void ShowSword(MergeItem s,Action onRemove=null)
        {
            if(s==null)return;
            bool used=MergeManager.Instance!=null&&MergeManager.Instance.IsSwordEquipped(s);
            string progress=s.level>=SwordRules.MaxLevel?$"Đã đạt cấp tối đa {SwordRules.MaxLevel}":$"Phôi tích lũy {s.currentPieces:N0}/{s.GetRequiredPieces():N0}\nGhép các kiếm cùng cấp trong kho; kéo vào ô xuất chiến để đổi trang bị.";
            Show(DismantleService.GetSwordDisplayName(s.level),$"Bậc {RpgUI.SwordRarity(s.level)} · Cấp {s.level} · "+(used?"Đang trang bị":"Trong kho"),$"SÁT THƯƠNG  {FlyingSword.CalculateBaseDamage(s.level):N0}\nTỐC ĐỘ  {FlyingSword.CalculateBaseSpeed(s.level):N0}\n\n{progress}\n\nGhép không tốn tiền; số phôi cần theo cấp kiếm hiện tại.",RpgUI.SwordArt(s),RpgUI.SwordColor(s.level),onRemove,s);
        }
        void RefreshEquip()
        {
            equip.gameObject.SetActive(sword!=null);
            if(sword==null)return;
            bool used=MergeManager.Instance.IsSwordEquipped(sword);
            RpgUI.Button(equip,used?"Tháo trang bị":"Trang bị",true,()=>
            {
                if(sword==null){Close();return;}
                var parent=sword.transform.parent;
                sword.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Right});
                if(sword.transform.parent==parent)feedback.text="Không có ô phù hợp. Kiểm tra chỗ trống và cấp kiếm.";
                else {feedback.text="Đã cập nhật trang bị.";RefreshEquip();}
            });
        }
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Equipment
{
    [DefaultExecutionOrder(11000)]
    public sealed class InventoryPresentation : MonoBehaviour
    {
        public FoundationTheme theme;
        public StorageModalUI owner;
        public FoundationItem slotPrefab,cardPrefab;
        public ItemDetailSheet sheet;
        public TMP_Text wallet,summary;
        public FoundationAction sort,view;
        public FoundationAction[] tabs;
        public Sprite pill,herb,ore,dust,pending;
        public bool descending=true;
        Vector2 lastSize;
        void Awake()
        {
            sort.Bind("Kiếm: cấp giảm ↓",()=>{descending=!descending;sort.label.text=descending?"Kiếm: cấp giảm ↓":"Kiếm: cấp tăng ↑";owner.SelectCategory(owner.selectedCategory);});
            view.Bind("Dạng danh sách",()=>{owner.ToggleViewMode();view.label.text=owner.currentViewMode==StorageViewMode.GridSlots?"Dạng danh sách":"Dạng ô";});
            for(int i=0;i<tabs.Length;i++){int n=i;tabs[i].Bind(new[]{"Tất cả","Kiếm","Đan","Vật liệu","Hòm tạm"}[i],()=>owner.SelectCategory((StorageCategory)n));}
        }
        void LateUpdate()
        {
            if(owner==null)return;
            var e=EconomyManager.Instance;
            wallet.text=$"LINH THẠCH  {RpgUI.Number(e!=null?e.currentLinhThach:0)}    ·    QUẶNG  {ArtifactService.Instance?.Ore??0:N0}";
            summary.text="Chạm để xem chi tiết · Giữ vật phẩm quý trong xác nhận phân giải";
            for(int i=0;i<tabs.Length;i++){tabs[i].button.targetGraphic.color=(int)owner.selectedCategory==i?theme.raised:theme.surface;tabs[i].label.color=(int)owner.selectedCategory==i?theme.gold:theme.ivory;}
            var grid=owner.gridContent!=null?owner.gridContent.GetComponent<UnityEngine.UI.GridLayoutGroup>():null;
            if(grid!=null&&lastSize!=owner.gridContent.rect.size){lastSize=owner.gridContent.rect.size;float w=(lastSize.x-36)/4;grid.cellSize=new Vector2(w,w+64);}
        }
        public GameObject Add(string name,string rarity,string count,string body,Sprite icon,Color color,bool equipped,Action remove,MergeItem sword=null)
        {
            var item=Instantiate(owner.currentViewMode==StorageViewMode.GridSlots?slotPrefab:cardPrefab,owner.contentContainer);
            item.name="InventoryItem";RpgUI.Top(item.badge.rectTransform,0,52,8,8);item.Bind(name,rarity,icon,count,()=>
            {
                foreach(var other in owner.contentContainer.GetComponentsInChildren<FoundationItem>())other.SetSelected(other==item);
                if(sword!=null)sheet.ShowSword(sword,remove);else sheet.Show(name,rarity,body,icon,color,remove);
            });
            if(rarity=="Dược liệu")RpgEmblem.Show(item.icon,RpgEmblemKind.Herb,theme.jade);
            else if(rarity.Contains("phẩm")||rarity=="Phế đan")RpgEmblem.Show(item.icon,RpgEmblemKind.Pill,color);
            item.detail.gameObject.SetActive(true);item.detail.text=equipped?"ĐANG TRANG BỊ":rarity;item.detail.color=equipped?theme.gold:color;
            var edge=item.transform.Find("RarityEdge").GetComponent<UnityEngine.UI.Image>();edge.color=color;
            return item.gameObject;
        }
    }
}

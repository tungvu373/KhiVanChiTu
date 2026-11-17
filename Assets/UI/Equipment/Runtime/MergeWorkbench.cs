using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Equipment
{
    [DefaultExecutionOrder(12000)]
    public sealed class MergeWorkbench : MonoBehaviour
    {
        public FoundationTheme theme;
        public MergeManager manager;
        public TMP_Text capacity,preview,feedback;
        public FoundationItem first,second,result;
        public FoundationAction merge,clear;
        public UnityEngine.UI.GridLayoutGroup[] grids;
        MergeItem a,b;
        bool busy;
        float next;
        public static MergeWorkbench Find(Component c)
        {foreach(var m in Object.FindObjectsByType<MergeWorkbench>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(m.gameObject.scene==c.gameObject.scene)return m;return null;}
        void Awake(){merge.Bind("Dung hợp",()=>StartCoroutine(Merge()));clear.Bind("Bỏ chọn",()=>{a=b=null;Refresh();});}
        public void Select(MergeItem item)
        {if(item==null||manager.IsSwordEquipped(item))return;if(a==item)a=null;else if(b==item)b=null;else if(a==null)a=item;else b=item;Refresh();}
        public void Refresh()
        {
            bool valid=a!=null&&b!=null&&a!=b&&!manager.IsSwordEquipped(a)&&!manager.IsSwordEquipped(b)&&a.level==b.level&&a.level<8;
            Bind(first,a,"Nguyên liệu 1");Bind(second,b,"Nguyên liệu 2");
            merge.SetAvailable(valid&&!busy);
            if(valid)
            {
                int l=a.level,p=a.currentPieces+b.currentPieces;
                while(l<8&&p>=Required(l)){p-=Required(l);l++;if(p<=0)p=1;}
                var art=a.levelSprites!=null&&a.levelSprites.Length>=l?a.levelSprites[l-1]:RpgUI.SwordArt(a);
                result.Bind(DismantleService.GetSwordDisplayName(l),$"Cấp {l}",art,$"{p:N0} phôi");result.detail.gameObject.SetActive(true);
                preview.text=$"Kết quả: cấp {l} · {p:N0} phôi\nChi phí: {a.currentPieces:N0} + {b.currentPieces:N0} phôi cùng cấp · Không tốn tiền";
            }
            else {result.Bind("Kết quả","",null,"");preview.text=a==null?"Chạm kiếm → Chọn ghép để đặt nguyên liệu.\nDùng hai kiếm cùng cấp trong kho, dưới cấp 8.":"Chọn thêm kiếm cùng cấp trong kho.\nKiếm đang xuất chiến không dùng làm nguyên liệu.";}
        }
        static int Required(int l)=>l<=2?2:l>=8?9999:l;
        static void Bind(FoundationItem target,MergeItem s,string empty)
        {target.Bind(s!=null?DismantleService.GetSwordDisplayName(s.level):empty,s!=null?$"Cấp {s.level}":"",RpgUI.SwordArt(s),s!=null?$"{s.currentPieces:N0} phôi":"");target.detail.gameObject.SetActive(true);target.SetSelected(s!=null);}
        IEnumerator Merge()
        {
            if(a==null||b==null||a==b||a.level!=b.level||a.level>=8||manager.IsSwordEquipped(a)||manager.IsSwordEquipped(b)){feedback.text="Cần hai kiếm cùng cấp trong kho.";Refresh();yield break;}
            busy=true;var target=a;var source=b;source.originalParent=source.transform.parent;
            target.GetComponentInParent<MergeSlot>().OnDrop(new PointerEventData(EventSystem.current){pointerDrag=source.gameObject});
            yield return null;a=b=null;busy=false;feedback.text=target!=null?$"Dung hợp thành công · Cấp {target.level}, {target.currentPieces:N0} phôi":"Đã cập nhật kho kiếm.";Refresh();
        }
        void OnEnable(){a=b=null;if(first!=null)Refresh();}
        void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;
            capacity.text=$"XUẤT CHIẾN  {manager.EquippedCount}/{SwordLoadoutService.Instance?.GetCurrentCapacity()??0}    ·    Chạm kiếm để xem chỉ số và trang bị";
            int cap=SwordLoadoutService.Instance?.GetCurrentCapacity()??manager.equipPanel.childCount;
            int visible=Mathf.Min(manager.equipPanel.childCount,Mathf.Max(4,Mathf.CeilToInt(cap/4f)*4));
            for(int i=0;i<manager.equipPanel.childCount;i++)manager.equipPanel.GetChild(i).gameObject.SetActive(i<visible);
            RpgUI.Height(manager.equipPanel,Mathf.CeilToInt(visible/4f)*278);
            foreach(var g in grids)if(g!=null){g.enabled=true;float w=(((RectTransform)g.transform).rect.width-36)/4;g.cellSize=new Vector2(w,w+36);}
            foreach(var sword in manager.GetAllSwords())
            {
                var r=(RectTransform)sword.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(30,52);r.offsetMax=new Vector2(-30,-18);r.localRotation=Quaternion.identity;
                var caption=sword.transform.Find("EquipmentCaption")?.GetComponent<TMP_Text>();
                if(caption==null)
                {caption=new FoundationFactory(theme).Text("EquipmentCaption",sword.transform,"",28,TextAlignmentOptions.Center);RpgUI.Bottom(caption.rectTransform,-48,60,-16,-16);}
                caption.text=$"C{sword.level} · {sword.currentPieces:N0} phôi";caption.color=theme.ivory;
                var surface=sword.transform.parent.GetComponent<UnityEngine.UI.Image>();if(surface!=null){surface.color=theme.surface;surface.sprite=theme.surfaceSprite;}
            }
            if((a==null&&first.IsSelected)||(b==null&&second.IsSelected))Refresh();
        }
    }
}

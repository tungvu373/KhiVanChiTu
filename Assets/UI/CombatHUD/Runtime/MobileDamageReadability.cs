using UnityEngine;

namespace KhiVanChiTu.UI.CombatHUD
{
    /// <summary>Scene-scoped visual lanes; never changes damage totals, hit timing, RNG or pool lifetime.</summary>
    public sealed class MobileDamageReadability : MonoBehaviour
    {
        static int sequence;
        TextMesh text;
        Renderer textRenderer;
        Camera view;
        TMPro.TextMeshPro readable;
        public static void Begin(DamagePopup popup,TextMesh label)
        {
            if(!CombatHudPresentation.Owns(popup.gameObject.scene))return;
            var component=popup.GetComponent<MobileDamageReadability>();if(component==null)component=popup.gameObject.AddComponent<MobileDamageReadability>();
            component.text=label;component.textRenderer=label.GetComponent<Renderer>();component.view=Camera.main;
            if(component.view!=null)
            {
                int lane=sequence++%12;
                var screen=component.view.WorldToScreenPoint(popup.transform.position);
                screen.x=Mathf.Clamp(screen.x+(lane%4-1.5f)*Screen.width*.13f,Screen.width*.12f,Screen.width*.88f);
                screen.y+=Screen.height*(.035f+(lane/4)*.02f);
                if(screen.z>0)popup.transform.position=component.view.ScreenToWorldPoint(screen);
            }
        }
        public static void Style(DamagePopup popup,TextMesh label,float damage,bool critical)
        {
            if(!CombatHudPresentation.Owns(popup.gameObject.scene))return;
            label.text=(critical?"! ":"-")+(Mathf.Abs(damage)>=1000?PortraitHudController.Compact(damage):Mathf.RoundToInt(damage).ToString());
        }
        void LateUpdate()
        {
            if(view==null||textRenderer==null||text==null)return;
            if(!CombatHudPresentation.Owns(gameObject.scene)||(!text.text.StartsWith("-")&&!text.text.StartsWith("! ")))
            {if(readable!=null)readable.gameObject.SetActive(false);textRenderer.enabled=true;return;}
            if(readable==null)
            {
                var child=new GameObject("P2ReadableDamage",typeof(RectTransform));child.transform.SetParent(transform,false);
                readable=child.AddComponent<TMPro.TextMeshPro>();readable.font=PortraitHudController.Instance.GetComponent<CombatHudPresentation>().theme.headingFont;
                var material=PortraitHudController.Instance.GetComponent<CombatHudPresentation>().damageMaterial;if(material!=null)readable.fontSharedMaterial=material;
                readable.fontSize=32;readable.alignment=TMPro.TextAlignmentOptions.Center;readable.textWrappingMode=TMPro.TextWrappingModes.NoWrap;readable.overflowMode=TMPro.TextOverflowModes.Overflow;readable.richText=false;
                readable.rectTransform.sizeDelta=new Vector2(24,4);readable.renderer.sortingOrder=textRenderer.sortingOrder+1;
            }
            readable.gameObject.SetActive(true);textRenderer.enabled=false;
            if(readable.text!=text.text){readable.text=text.text;readable.ForceMeshUpdate();}readable.color=text.color;
            var bounds=readable.renderer.bounds;
            float depth=view.WorldToScreenPoint(bounds.center).z;if(depth<=0)return;
            float min=float.PositiveInfinity,max=float.NegativeInfinity;
            for(int i=0;i<8;i++)
            {
                var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                float y=view.WorldToScreenPoint(corner).y;min=Mathf.Min(min,y);max=Mathf.Max(max,y);
            }
            float pixels=max-min;
            if(pixels>1)transform.localScale*=Mathf.Clamp(pixels,Screen.height*.009f,Screen.height*.0125f)/pixels;
        }
    }
}

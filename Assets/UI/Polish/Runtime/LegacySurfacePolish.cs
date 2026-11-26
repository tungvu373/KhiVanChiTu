using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Polish
{
    /// <summary>Finish the remaining legacy windows after their native layout/style pass. No transactions or layout ownership.</summary>
    [DefaultExecutionOrder(18000)]
    public sealed class LegacySurfacePolish : MonoBehaviour
    {
        public FoundationTheme theme;
        public Transform target;
        public UnityEngine.UI.Image backdrop;
        bool pending;
        void OnEnable(){pending=true;}
        public void Invalidate(){pending=true;}
        void LateUpdate(){if(!pending)return;pending=false;Apply();}
        void Apply()
        {
            var root=target!=null?target:transform;
            foreach(var image in root.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                // Old ornament prefabs sit above the content with transparent centers.
                // Replacing those textures with an opaque surface would cover every label.
                var decoration=image.GetComponentInParent<MobileUIPanelView>();
                if(decoration!=null&&decoration.name=="P6Frame"){image.enabled=false;continue;}
                if(image==backdrop){image.sprite=null;image.color=new Color(0,.025f,.03f,.72f);continue;}
                string n=image.name;var button=image.GetComponent<UnityEngine.UI.Button>();
                bool fill=n.Contains("ProgFill");bool track=n=="ProgBG"||n=="TotalProgBG";
                bool frame=image.sprite!=null&&(image.sprite.name.StartsWith("JadeV")||image.sprite.name.Contains("Window"));
                if(!fill&&!track&&button==null&&!n.StartsWith("Card_")&&!frame)continue;
                if(frame&&button==null&&image.rectTransform.rect.width<150&&!fill&&!track)continue;
                var caption=button!=null?button.GetComponentInChildren<UnityEngine.UI.Text>(true):null;
                bool closing=button!=null&&(button.name.ToLowerInvariant().Contains("close")||(caption!=null&&(caption.text.Trim()=="Đóng"||caption.text.Trim()=="Hủy"||caption.text.Trim()=="X")));
                bool gold=button!=null&&!closing&&((image.color.r>.65f&&image.color.g>.5f&&image.color.b<.65f)||((n=="BtnAction"||n=="BtnClaimAll")&&button.interactable));
                image.sprite=theme.surfaceSprite;
                if(!fill)image.type=UnityEngine.UI.Image.Type.Sliced;
                image.color=fill?theme.jade:track?theme.ink:button!=null?(gold?theme.gold:theme.raised):n.StartsWith("Card_")?theme.surface:theme.ink;
                if(button!=null)
                {
                    foreach(var label in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))label.color=!button.interactable?theme.muted:gold?theme.ink:theme.ivory;
                    if(button.GetComponent<JadeButtonFeedback>()==null)button.gameObject.AddComponent<JadeButtonFeedback>();
                }
            }
            foreach(var outline in root.GetComponentsInChildren<UnityEngine.UI.Outline>(true))outline.enabled=false;
        }
    }
}

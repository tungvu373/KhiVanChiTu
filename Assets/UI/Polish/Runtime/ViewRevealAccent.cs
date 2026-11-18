using UnityEngine;

namespace KhiVanChiTu.UI.Polish
{
    /// <summary>A short reveal on a decorative header/art only; never gates input or owns panel closing.</summary>
    public sealed class ViewRevealAccent : MonoBehaviour
    {
        public UnityEngine.UI.Graphic[] visuals;
        void OnEnable(){var director=MobileUIAnimationDirector.For(this);if(director==null)return;foreach(var visual in visuals)if(visual!=null)director.Accent(visual,director.progress);}
    }
}

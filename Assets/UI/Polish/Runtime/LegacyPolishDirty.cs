using UnityEngine;

namespace KhiVanChiTu.UI.Polish
{
    public sealed class LegacyPolishDirty : MonoBehaviour
    {
        public LegacySurfacePolish owner;
        void OnTransformChildrenChanged(){if(owner!=null)owner.Invalidate();}
    }
}

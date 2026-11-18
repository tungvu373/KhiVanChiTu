using UnityEngine;

namespace KhiVanChiTu.UI.Menu
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuVeil : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper m)
        {m.Clear();var r=GetPixelAdjustedRect();var bottom=color;bottom.a=.95f;var top=color;top.a=0;m.AddVert(new Vector3(r.xMin,r.yMin),bottom,Vector2.zero);m.AddVert(new Vector3(r.xMin,r.yMax),top,Vector2.zero);m.AddVert(new Vector3(r.xMax,r.yMax),top,Vector2.zero);m.AddVert(new Vector3(r.xMax,r.yMin),bottom,Vector2.zero);m.AddTriangle(0,1,2);m.AddTriangle(0,2,3);}
    }
}

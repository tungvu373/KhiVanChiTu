using UnityEngine;

namespace KhiVanChiTu.UI.Navigation
{
    /// <summary>Five matching vector emblems; no texture stretching or font-symbol dependency.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NavigationIcon : UnityEngine.UI.MaskableGraphic
    {
        [Range(0,4)] public int symbol;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            switch(symbol)
            {
                case 0:Line(mesh,9,7,38,40);Line(mesh,33,7,10,36);Line(mesh,4,16,17,4);Line(mesh,27,4,40,16);break;
                case 1:Line(mesh,8,35,24,44);Line(mesh,24,44,40,35);Line(mesh,40,35,36,15);Line(mesh,36,15,24,4);Line(mesh,24,4,12,15);Line(mesh,12,15,8,35);Line(mesh,24,12,24,34);break;
                case 2:Line(mesh,24,6,24,42);Line(mesh,24,42,13,26);Line(mesh,13,26,24,11);Line(mesh,24,11,35,26);Line(mesh,35,26,24,42);Line(mesh,6,18,24,6);Line(mesh,24,6,42,18);break;
                case 3:Line(mesh,8,7,8,32);Line(mesh,8,32,15,41);Line(mesh,15,41,33,41);Line(mesh,33,41,40,32);Line(mesh,40,32,40,7);Line(mesh,15,8,24,22);Line(mesh,24,22,33,8);break;
                default:Box(mesh,7,7);Box(mesh,28,7);Box(mesh,7,28);Box(mesh,28,28);break;
            }
        }
        void Box(UnityEngine.UI.VertexHelper m,float x,float y){Line(m,x,y,x+12,y);Line(m,x+12,y,x+12,y+12);Line(m,x+12,y+12,x,y+12);Line(m,x,y+12,x,y);}
        void Line(UnityEngine.UI.VertexHelper mesh,float ax,float ay,float bx,float by)
        {
            var r=GetPixelAdjustedRect();Vector2 a=new Vector2(r.xMin+ax*r.width/48,r.yMin+ay*r.height/48),b=new Vector2(r.xMin+bx*r.width/48,r.yMin+by*r.height/48);
            var normal=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(r.width,r.height)*.028f;int start=mesh.currentVertCount;
            mesh.AddVert(a-normal,color,Vector2.zero);mesh.AddVert(a+normal,color,Vector2.zero);mesh.AddVert(b+normal,color,Vector2.zero);mesh.AddVert(b-normal,color,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}

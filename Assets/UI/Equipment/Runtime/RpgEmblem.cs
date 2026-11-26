using UnityEngine;

namespace KhiVanChiTu.UI.Equipment
{
    public enum RpgEmblemKind { Pill, Herb, Robe, Crown, Boots, Amulet, Body, Formation }
    /// <summary>Resolution-independent item silhouettes, using the same line language as NavigationIcon.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RpgEmblem : UnityEngine.UI.MaskableGraphic
    {
        public RpgEmblemKind symbol;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper m)
        {
            m.Clear();
            switch(symbol)
            {
                case RpgEmblemKind.Pill:
                    Ring(m,24,24,16);Ring(m,24,24,12);Line(m,15,18,31,32);Line(m,18,34,22,36);break;
                case RpgEmblemKind.Herb:
                    Line(m,12,5,34,40);Leaf(m,19,16,-1);Leaf(m,25,25,1);Leaf(m,31,34,-1);Line(m,10,6,20,6);break;
                case RpgEmblemKind.Robe:
                    Poly(m,new[]{new Vector2(16,41),new Vector2(8,35),new Vector2(3,24),new Vector2(12,19),new Vector2(15,28),new Vector2(12,5),new Vector2(36,5),new Vector2(33,28),new Vector2(36,19),new Vector2(45,24),new Vector2(40,35),new Vector2(32,41)});Line(m,16,41,24,30);Line(m,24,30,32,41);Line(m,16,18,32,18);break;
                case RpgEmblemKind.Crown:
                    Poly(m,new[]{new Vector2(7,34),new Vector2(16,27),new Vector2(24,41),new Vector2(32,27),new Vector2(41,34),new Vector2(36,12),new Vector2(12,12)});Line(m,12,7,36,7);Ring(m,24,20,3);break;
                case RpgEmblemKind.Boots:
                    Poly(m,new[]{new Vector2(12,40),new Vector2(28,40),new Vector2(26,20),new Vector2(38,13),new Vector2(38,7),new Vector2(10,7),new Vector2(10,18)});Line(m,14,33,26,33);Line(m,10,12,37,12);break;
                case RpgEmblemKind.Amulet:
                    Ring(m,24,33,8);Poly(m,new[]{new Vector2(24,28),new Vector2(38,18),new Vector2(24,5),new Vector2(10,18)});Ring(m,24,17,5);Line(m,20,4,18,0);Line(m,28,4,30,0);break;
                case RpgEmblemKind.Body:
                    Ring(m,24,36,5);Line(m,24,31,24,13);Line(m,10,22,24,28);Line(m,24,28,38,22);Line(m,24,13,12,7);Line(m,24,13,36,7);Ring(m,24,22,16);break;
                default:
                    Ring(m,24,24,19);Ring(m,24,24,13);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(m,24+16*Mathf.Cos(a),24+16*Mathf.Sin(a),24+21*Mathf.Cos(a),24+21*Mathf.Sin(a));}Poly(m,new[]{new Vector2(24,36),new Vector2(35,18),new Vector2(13,18)});Ring(m,24,24,4);break;
            }
        }
        void Leaf(UnityEngine.UI.VertexHelper m,float x,float y,float side){Poly(m,new[]{new Vector2(x,y),new Vector2(x+side*11,y+1),new Vector2(x+side*10,y+9),new Vector2(x,y+6)});}
        void Poly(UnityEngine.UI.VertexHelper m,Vector2[] p){for(int i=0;i<p.Length;i++)Line(m,p[i].x,p[i].y,p[(i+1)%p.Length].x,p[(i+1)%p.Length].y);}
        void Ring(UnityEngine.UI.VertexHelper m,float x,float y,float radius){for(int i=0;i<48;i++){float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;Line(m,x+radius*Mathf.Cos(a),y+radius*Mathf.Sin(a),x+radius*Mathf.Cos(b),y+radius*Mathf.Sin(b));}}
        void Line(UnityEngine.UI.VertexHelper m,float x,float y,float xx,float yy)
        {
            var r=GetPixelAdjustedRect();Vector2 a=new Vector2(r.xMin+x*r.width/48,r.yMin+y*r.height/48),b=new Vector2(r.xMin+xx*r.width/48,r.yMin+yy*r.height/48);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(r.width,r.height)*.018f;int k=m.currentVertCount;m.AddVert(a-n,color,Vector2.zero);m.AddVert(a+n,color,Vector2.zero);m.AddVert(b+n,color,Vector2.zero);m.AddVert(b-n,color,Vector2.zero);m.AddTriangle(k,k+1,k+2);m.AddTriangle(k,k+2,k+3);
        }
        public static void Show(UnityEngine.UI.Image host,RpgEmblemKind kind,Color tint,int variant=-1)
        {
            var sprite=KhiVanChiTu.UI.GameIcons.GameIconCatalogue.Current?.Emblem(kind,variant);
            if(sprite!=null){Hide(host);host.sprite=sprite;host.type=UnityEngine.UI.Image.Type.Simple;host.color=Color.white;host.preserveAspect=true;host.enabled=true;return;}
            host.enabled=false;var child=host.transform.Find("VectorEmblem");var r=child!=null?(RectTransform)child:RpgUI.Rect("VectorEmblem",host.transform);RpgUI.Fill(r);var e=r.GetComponent<RpgEmblem>()??r.gameObject.AddComponent<RpgEmblem>();e.symbol=kind;e.color=tint;e.raycastTarget=false;e.SetAllDirty();r.gameObject.SetActive(true);
        }
        public static void Hide(UnityEngine.UI.Image host){var child=host.transform.Find("VectorEmblem");if(child!=null)child.gameObject.SetActive(false);}
    }
}

using UnityEngine;

namespace KhiVanChiTu.UI.Navigation
{
    /// <summary>Five matching vector emblems; no texture stretching or font-symbol dependency.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NavigationIcon : UnityEngine.UI.MaskableGraphic
    {
        [Range(0,4)] public int symbol;
        public Sprite artwork;
        Sprite Artwork=>artwork!=null?artwork:KhiVanChiTu.UI.GameIcons.GameIconCatalogue.At(KhiVanChiTu.UI.GameIcons.GameIconCatalogue.Current?.navigation,symbol);
        public override Texture mainTexture=>Artwork!=null?Artwork.texture:base.mainTexture;
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            var sprite=Artwork;
            if(sprite!=null)
            {
                var rect=GetPixelAdjustedRect();var center=rect.center;
                float aspect=sprite.rect.width/sprite.rect.height;
                if(rect.width>rect.height*aspect)rect.width=rect.height*aspect;else rect.height=rect.width/aspect;
                rect.center=center;var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
                float light=Mathf.Clamp(Mathf.Max(color.r,Mathf.Max(color.g,color.b)),.55f,1);
                var tone=new Color(light,light,light,color.a);
                mesh.AddVert(new Vector3(rect.xMin,rect.yMin),tone,new Vector2(uv.x,uv.y));
                mesh.AddVert(new Vector3(rect.xMin,rect.yMax),tone,new Vector2(uv.x,uv.w));
                mesh.AddVert(new Vector3(rect.xMax,rect.yMax),tone,new Vector2(uv.z,uv.w));
                mesh.AddVert(new Vector3(rect.xMax,rect.yMin),tone,new Vector2(uv.z,uv.y));
                mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,3);return;
            }
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

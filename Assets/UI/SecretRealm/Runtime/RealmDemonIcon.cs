using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.SecretRealm
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RealmDemonIcon : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();Line(mesh,.23f,.75f,.18f,.98f);Line(mesh,.18f,.98f,.4f,.78f);Line(mesh,.77f,.75f,.82f,.98f);Line(mesh,.82f,.98f,.6f,.78f);
            Line(mesh,.22f,.73f,.22f,.35f);Line(mesh,.22f,.35f,.5f,.12f);Line(mesh,.5f,.12f,.78f,.35f);Line(mesh,.78f,.35f,.78f,.73f);Line(mesh,.22f,.73f,.78f,.73f);
            Line(mesh,.29f,.6f,.44f,.53f);Line(mesh,.56f,.53f,.71f,.6f);Line(mesh,.38f,.33f,.62f,.33f);Line(mesh,.5f,.5f,.46f,.4f);Line(mesh,.46f,.4f,.54f,.4f);
        }
        void Line(UnityEngine.UI.VertexHelper m,float x,float y,float xx,float yy)
        {var r=GetPixelAdjustedRect();Vector2 a=new Vector2(r.x+x*r.width,r.y+y*r.height),b=new Vector2(r.x+xx*r.width,r.y+yy*r.height);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(r.width,r.height)*.028f;int k=m.currentVertCount;m.AddVert(a-n,color,Vector2.zero);m.AddVert(a+n,color,Vector2.zero);m.AddVert(b+n,color,Vector2.zero);m.AddVert(b-n,color,Vector2.zero);m.AddTriangle(k,k+1,k+2);m.AddTriangle(k,k+2,k+3);}
    }
}

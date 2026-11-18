using System;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Equipment
{
    // Marks a subtree whose layout belongs to the Phase 1 design system.
    public sealed class RpgViewScope : MonoBehaviour { }
    public static class RpgUI
    {
        // Presentation only: keep late-game values readable without changing balances or precision in services.
        public static string Number(float value)=>Mathf.Abs(value)>=1e15f?value.ToString("0.##E+0"):EconomyManager.FormatNumber(value);
        public static RectTransform Rect(string n,Transform p)=>FoundationFactory.Rect(n,p);
        public static void Fill(RectTransform r,float l=0,float b=0,float rr=0,float t=0)=>FoundationFactory.Fill(r,l,b,rr,t);
        public static void Top(RectTransform r,float y,float h,float l=0,float right=0)
        {r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);r.offsetMin=new Vector2(l,-y-h);r.offsetMax=new Vector2(-right,-y);}
        public static void Bottom(RectTransform r,float y,float h,float l=0,float right=0)
        {r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,0);r.pivot=new Vector2(.5f,0);r.offsetMin=new Vector2(l,y);r.offsetMax=new Vector2(-right,y+h);}
        public static void Height(Transform r,float h)
        {var le=r.GetComponent<UnityEngine.UI.LayoutElement>()??r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minHeight=le.preferredHeight=h;}
        public static UnityEngine.UI.ScrollRect Scroll(Transform parent,out RectTransform content)
        {
            var r=Rect("ReadingScroll",parent);Fill(r);var s=r.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();s.horizontal=false;s.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var v=Rect("Viewport",r);Fill(v);v.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();var hit=v.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=Color.clear;
            content=Rect("Items",v);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;content.anchoredPosition=Vector2.zero;
            var g=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();g.spacing=20;g.childControlHeight=g.childControlWidth=true;g.childForceExpandHeight=false;
            content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;s.viewport=v;s.content=content;return s;
        }
        public static Color SwordColor(int l)=>l>=8?new Color(1,.45f,.1f,.95f):l>=5?new Color(.85f,.35f,1,.95f):l>=3?new Color(.2f,.65f,1,.95f):l>=2?new Color(.35f,.9f,.45f,.95f):new Color(.45f,.52f,.62f,.75f);
        public static string SwordRarity(int l)=>l>=8?"Cam":l>=5?"Tím":l>=3?"Lam":l>=2?"Lục":"Trắng";
        public static string Quality(PillQuality q)=>q==PillQuality.Supreme?"Cực phẩm":q==PillQuality.Refined?"Trung phẩm":q==PillQuality.Basic?"Sơ phẩm":"Phế đan";
        public static Color PillColor(PillQuality q)=>q==PillQuality.Supreme?new Color(1,.84f,0,.95f):q==PillQuality.Refined?new Color(0,.75f,1,.95f):q==PillQuality.Basic?new Color(.48f,.98f,0,.95f):new Color(.62f,.68f,.75f,.75f);
        public static Sprite SwordArt(MergeItem s)=>s!=null&&s.levelSprites!=null&&s.levelSprites.Length>0?s.levelSprites[Mathf.Clamp(s.level-1,0,s.levelSprites.Length-1)]:null;
        public static void Button(FoundationAction a,string caption,bool ready,Action click=null)
        {if(click!=null)a.Bind(caption,click);else a.label.text=caption;a.SetAvailable(ready);}
    }
}

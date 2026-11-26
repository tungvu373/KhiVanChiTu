using System;
using UnityEngine;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.GameIcons
{
    /// <summary>Presentation artwork only. Contains no item IDs, rarity rules, costs or player state.</summary>
    [CreateAssetMenu(menuName="TuTien/UI/Game Icon Catalogue")]
    public sealed class GameIconCatalogue : ScriptableObject
    {
        public Sprite[] swords,skills,herbs,artifacts,formations,navigation,progression;
        public Sprite cultivationPill,breakthroughPill,ore,dust,chest,cauldron,body,stone,fortune;
        static GameIconCatalogue cached;
        static bool lookedUp;
        public static GameIconCatalogue Current
        {
            get{if(!lookedUp){cached=Resources.Load<GameIconCatalogue>("UI/GameIconCatalogue");lookedUp=true;}return cached;}
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InvalidateCache(){cached=null;lookedUp=false;}
        public static Sprite At(Sprite[] values,int index)=>values!=null&&index>=0&&index<values.Length?values[index]:null;
        static bool Has(string value,string part)=>value!=null&&value.IndexOf(part,StringComparison.OrdinalIgnoreCase)>=0;
        public Sprite Item(string name,string quality)
        {
            if(Has(name,"Dược Trần"))return dust;
            if(Has(name,"Đan"))return Has(name,"Đột Phá")?breakthroughPill:cultivationPill;
            if(Has(name,"Linh Chi"))return At(herbs,0);
            if(Has(name,"Ngưng Khí"))return At(herbs,1);
            if(Has(name,"Tuyết Liên"))return At(herbs,2);
            if(quality=="Dược liệu")return At(herbs,0);
            if(Has(name,"Quặng"))return ore;
            if(Has(name,"Bụi"))return dust;
            if(Has(name,"Hòm"))return chest;
            if(Has(name,"Linh Thạch"))return stone;
            if(Has(name,"Cơ Duyên"))return fortune;
            return null;
        }
        public Sprite Emblem(RpgEmblemKind kind,int variant=-1)
        {
            switch(kind)
            {
                case RpgEmblemKind.Pill:return cultivationPill;
                case RpgEmblemKind.Herb:return At(herbs,Mathf.Clamp(variant,0,2));
                case RpgEmblemKind.Robe:case RpgEmblemKind.Crown:case RpgEmblemKind.Boots:case RpgEmblemKind.Amulet:return At(artifacts,(int)kind-(int)RpgEmblemKind.Robe);
                case RpgEmblemKind.Body:return NavigationArtCatalogue.Current?.BodyNode(variant)??body;
                case RpgEmblemKind.Formation:return At(formations,Mathf.Clamp(variant,0,5));
                default:return null;
            }
        }
        public Sprite Reward(string caption,int swordLevel=1)
        {
            if(Has(caption,"Linh thạch"))return stone;
            if(Has(caption,"Tu vi"))return At(formations,1);
            if(Has(caption,"Thảo dược"))return At(herbs,Has(caption,"3")?2:Has(caption,"2")?1:0);
            if(Has(caption,"Quặng"))return ore;
            if(Has(caption,"Cơ Duyên"))return fortune;
            if(Has(caption,"Phi kiếm"))return At(swords,Mathf.Clamp(swordLevel-1,0,swords.Length-1));
            return chest;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.GameIcons
{
    /// <summary>Artwork keyed by the existing route identity. No progression or save data.</summary>
    public sealed class NavigationArtCatalogue : ScriptableObject
    {
        [Serializable] public sealed class RouteArt
        {
            public NavigationRoute route;
            public Sprite icon;
        }
        public RouteArt[] routes;
        public Sprite[] bodyNodes;
        static NavigationArtCatalogue cached;
        static bool lookedUp;
        Dictionary<NavigationRoute,Sprite> lookup;
        public static NavigationArtCatalogue Current
        {
            get
            {
                if(!lookedUp){cached=Resources.Load<NavigationArtCatalogue>("UI/NavigationArtCatalogue");lookedUp=true;}
                return cached;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InvalidateCache(){cached=null;lookedUp=false;}
        void OnEnable(){lookup=null;}
        public Sprite Route(NavigationRoute route)
        {
            if(lookup==null)
            {
                lookup=new Dictionary<NavigationRoute,Sprite>();
                if(routes!=null)foreach(var entry in routes)if(entry!=null)lookup[entry.route]=entry.icon;
            }
            return lookup.TryGetValue(route,out var icon)?icon:null;
        }
        public Sprite BodyNode(int index)=>GameIconCatalogue.At(bodyNodes,index);
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Cultivation;

namespace KhiVanChiTu.UI.GameIcons.Editor
{
    public static class NavigationArtChecks
    {
        static void Expect(bool value,string reason){if(!value)throw new Exception(reason);}
        static IEnumerable<object> Wait(float seconds=.4f){float end=Time.unscaledTime+seconds;while(Time.unscaledTime<end)yield return null;}
        static void TextFits(Transform root)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var text in root.GetComponentsInChildren<TMPro.TMP_Text>())
            {text.ForceMeshUpdate();Expect(!text.isTextOverflowing,"Text overflow: "+text.name+" / "+text.text);}
        }
        static void Reveal(UnityEngine.UI.ScrollRect scroll,RectTransform item)
        {
            scroll.StopMovement();Canvas.ForceUpdateCanvases();
            var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport,item);
            float delta=bounds.min.y<scroll.viewport.rect.yMin?scroll.viewport.rect.yMin-bounds.min.y:bounds.max.y>scroll.viewport.rect.yMax?scroll.viewport.rect.yMax-bounds.max.y:0;
            scroll.content.anchoredPosition+=Vector2.up*delta;Canvas.ForceUpdateCanvases();
        }
        static void Target(UnityEngine.UI.Button button,RectTransform point,bool click=false)
        {
            Canvas.ForceUpdateCanvases();
            var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,point.TransformPoint(point.rect.center)),button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
            var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);
            Expect(hits.Count>0&&(hits[0].gameObject.transform==button.transform||hits[0].gameObject.transform.IsChildOf(button.transform)),"Artwork blocks or misses native button: "+button.name);
            if(click){Expect(button.IsInteractable(),"Action is disabled: "+button.name);UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,input,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);}
        }
        public static IEnumerable<object> Run(GlobalNavigationController nav,Action<Vector2Int> resize,Action<string,string,Action,string> report,Action<string> capture)
        {
            void Test(string id,string expected,Action action)=>report(id,expected,action,null);
            var art=NavigationArtCatalogue.Current;
            Test("distinct-route-catalogue","All 24 native routes and six body nodes have distinct generated images",()=>
            {
                Expect(art!=null&&art.routes.Length==GlobalNavigationController.Destinations.Length&&art.routes.Length==24,"Incomplete route map");
                var images=art.routes.Select(r=>r.icon).Concat(art.bodyNodes).ToArray();
                Expect(images.Length==30&&images.All(s=>s!=null)&&images.Distinct().Count()==30,"Repeated/missing image reference");
                foreach(var destination in GlobalNavigationController.Destinations)Expect(art.Route(destination.route)!=null,"Unmapped route "+destination.route);
                foreach(var image in images)
                {
                    var path=AssetDatabase.GetAssetPath(image);Expect(path.StartsWith(NavigationArtBuilder.Folder,StringComparison.Ordinal),"Old/group icon used "+path);
                    var importer=(TextureImporter)AssetImporter.GetAtPath(path);Expect(importer.alphaIsTransparency&&!importer.mipmapEnabled&&!importer.isReadable,"Incorrect alpha/memory import "+path);
                }
            });
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                resize(size);yield return null;
                foreach(var group in new[]{NavigationGroup.Equipment,NavigationGroup.Cultivation,NavigationGroup.SecretRealm,NavigationGroup.Other})
                {
                    nav.groupButtons[(int)group].button.onClick.Invoke();foreach(var x in Wait())yield return x;
                    Test("group-"+group+"-"+size,"Native group displays its existing route cards with readable text",()=>
                    {
                        Expect(nav.SelectedGroup==group&&nav.IsHubVisible,"Group not visible");
                        Expect(nav.cards.Count(c=>c.action.gameObject.activeInHierarchy)==GlobalNavigationController.Destinations.Count(d=>d.group==group),"Route card lost");TextFits(nav.hub);
                    });
                    capture("hub-"+group);yield return null;
                    foreach(var card in nav.cards.Where(c=>c.action.gameObject.activeInHierarchy))
                    {
                        Reveal(nav.hubScroll,(RectTransform)card.action.transform);yield return null;
                        Test("route-"+card.route+"-"+size,"Route has its unique large icon; icon tap area reaches original button",()=>
                        {
                            var icon=card.action.GetComponentInChildren<NavigationIcon>(true);
                            Expect(icon!=null&&icon.artwork==art.Route(card.route),"Wrong route image "+card.route);
                            Expect(icon.mainTexture==icon.artwork.texture&&!icon.raycastTarget,"Image/input mismatch");
                            Expect(icon.rectTransform.rect.size==new Vector2(104,104),"Route icon too small");
                            var corners=new Vector3[4];icon.rectTransform.GetWorldCorners(corners);
                            float right=corners.Max(c=>card.action.transform.InverseTransformPoint(c).x);
                            var label=(RectTransform)card.action.label.transform;label.GetWorldCorners(corners);
                            Expect(corners.Min(c=>card.action.transform.InverseTransformPoint(c).x)>right,"Icon overlaps title");
                            Target(card.action.button,icon.rectTransform);
                        });
                    }
                    if(group==NavigationGroup.Other){capture("hub-Other-bottom");yield return null;}
                }
                nav.SelectGroup(NavigationGroup.Cultivation);foreach(var x in Wait())yield return x;
                var bodyCard=nav.cards.First(c=>c.route==NavigationRoute.Body);Reveal(nav.hubScroll,(RectTransform)bodyCard.action.transform);
                Target(bodyCard.action.button,bodyCard.action.GetComponentInChildren<NavigationIcon>().rectTransform,true);foreach(var x in Wait(.5f))yield return x;
                var body=UnityEngine.Object.FindObjectsByType<CultivationScreen>().First(s=>s.kind==CultivationScreenKind.Body);
                var levels=Enumerable.Range(0,6).Select(i=>PlayerStatsService.Instance.GetLevel(i)).ToArray();float money=EconomyManager.Instance.currentLinhThach;
                for(int i=0;i<6;i++)
                {
                    body.selectors[i].button.onClick.Invoke();yield return null;
                    int node=i;Test("body-node-"+i+"-"+size,"Native body selector changes subject artwork to its matching unique node",()=>
                    {
                        Expect(body.Selected==node&&body.subject.text==body.Names[node],"Selected node mismatch");
                        Expect(body.subjectArt.enabled&&body.subjectArt.sprite==art.BodyNode(node)&&body.subjectArt.preserveAspect,"Wrong body-node illustration");
                        Expect(Enumerable.Range(0,6).All(n=>PlayerStatsService.Instance.GetLevel(n)==levels[n])&&EconomyManager.Instance.currentLinhThach==money,"Art selection altered progression/currency");
                        TextFits(body.transform);
                    });capture("body-"+i);yield return null;
                }
                nav.Back();foreach(var x in Wait())yield return x;
                Test("back-"+size,"Back closes body screen and leaves navigation available",()=>Expect(!body.gameObject.activeInHierarchy&&nav.IsHubVisible,"Back failed"));
                nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait())yield return x;
                Test("dock-preserved-"+size,"Five dock icons and gameplay return remain intact",()=>Expect(!nav.IsHubVisible&&nav.groupIcons.Length==5&&nav.groupIcons.All(i=>i.mainTexture!=null),"Dock/gameplay lost"));
            }
        }
    }
}
#endif

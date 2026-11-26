#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Cultivation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.GameIcons.Editor
{
    public static class GameIconChecks
    {
        static void Expect(bool value,string message){if(!value)throw new Exception(message);}
        static IEnumerable<object> Wait(float seconds=.35f){float until=Time.unscaledTime+seconds;while(Time.unscaledTime<until)yield return null;}
        static bool Generated(Sprite art)=>art!=null&&AssetDatabase.GetAssetPath(art).StartsWith(GameIconBuilder.Folder,StringComparison.Ordinal);
        public static IEnumerable<object> Run(GlobalNavigationController nav,Action<Vector2Int> resize,Action<string,string,Action,string> report,Action<string> capture)
        {
            void Test(string id,string expected,Action action)=>report(id,expected,action,null);
            var catalogue=GameIconCatalogue.Current;
            Test("icon-catalogue","All 35 generated PNGs imported and referenced by presentation catalogue",()=>
            {
                Expect(catalogue!=null,"Missing catalogue");
                var sprites=catalogue.swords.Concat(catalogue.skills).Concat(catalogue.herbs).Concat(catalogue.artifacts).Concat(catalogue.formations).Concat(catalogue.navigation).Concat(catalogue.progression).Concat(new[]{catalogue.cultivationPill,catalogue.breakthroughPill,catalogue.ore,catalogue.dust,catalogue.chest,catalogue.cauldron,catalogue.body,catalogue.stone,catalogue.fortune}).Distinct().ToArray();
                Expect(sprites.Length==35&&sprites.All(Generated),"Incomplete or non-generated icon reference");
                foreach(var s in sprites){var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s));Expect(importer.alphaIsTransparency&&!importer.mipmapEnabled&&!importer.isReadable&&s.texture.width<=512&&s.texture.height<=512,"Import contract: "+s.name);}
            });
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                resize(size);yield return null;nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait())yield return x;
                Test("skill-art-"+size,"Four native skill buttons retain callbacks/cooldowns and use generated artwork",()=>{
                    for(int i=0;i<4;i++){var v=nav.hud.skillViews[i];Expect(v.icon.sprite==catalogue.skills[i]&&v.icon.preserveAspect,"Wrong skill sprite "+i);Expect(v.button!=null&&v.cooldown!=null,"Lost skill binding");}
                    foreach(var icon in nav.groupIcons)Expect(icon.mainTexture!=null&&!icon.raycastTarget,"Navigation icon/input binding");});
                capture("icons-hud");yield return null;
                nav.OpenRoute(NavigationRoute.Merge);foreach(var x in Wait(.5f))yield return x;
                Test("sword-art-"+size,"Levels 1–10 use ten distinct UI images while cap/rarity remain native",()=>{
                    var manager=MergeManager.Instance;Expect(SwordRules.MaxLevel==10,"Changed cap");
                    Expect(AssetDatabase.GetAssetPath(manager.mergeItemPrefab)==GameIconBuilder.SwordVariant,"Native mobile variant not bound");
                    Expect(manager.mergeItemPrefab.GetComponent<MergeItem>().levelSprites.SequenceEqual(catalogue.swords),"Variant sprite array");
                    foreach(var sword in manager.GetAllSwords()){Expect(sword.levelSprites.SequenceEqual(catalogue.swords),"Live sword array");Expect(sword.GetComponent<UnityEngine.UI.Image>().sprite==RpgUI.SwordArt(sword),"Live sword sprite");}
                    Expect(catalogue.swords.Distinct().Count()==10,"Repeated sword art");Expect(RpgUI.SwordRarity(9)=="Cam"&&RpgUI.SwordRarity(10)=="Cam","Changed rarity mapping");});
                capture("icons-swords");yield return null;nav.Back();foreach(var x in Wait(.4f))yield return x;
                nav.OpenRoute(NavigationRoute.Storage);foreach(var x in Wait(.5f))yield return x;
                var inventory=UnityEngine.Object.FindAnyObjectByType<InventoryPresentation>();
                inventory.tabs[0].button.onClick.Invoke();foreach(var x in Wait(.3f))yield return x;
                Test("inventory-art-"+size,"Visible native inventory page uses generated images and keeps quantities/status labels",()=>{
                    var items=inventory.owner.contentContainer.GetComponentsInChildren<FoundationItem>();
                    int matched=0;
                    foreach(var item in items){var expected=catalogue.Item(item.title.text,item.detail.text);if(expected==null)continue;matched++;Expect(item.icon.enabled&&item.icon.sprite==expected&&item.icon.preserveAspect,"Wrong item art: "+item.title.text);Expect(!string.IsNullOrEmpty(item.badge.text),"Quantity badge lost");}
                    // Native inventory paginates All. A sword-filled first page
                    // legitimately contains no materials; inspect their own tabs below.
                    Expect(items.Length>0&&items.All(i=>Generated(i.icon.sprite)),"Visible page has missing/old artwork");
                    Expect(catalogue.Item("Tu Vi Đan","Sơ phẩm")!=catalogue.Item("Đột Phá Đan","Sơ phẩm"),"Recipes indistinguishable");
                    Expect(new[]{catalogue.Item("Linh Chi Vạn Niên","Dược liệu"),catalogue.Item("Cỏ Ngưng Khí","Dược liệu"),catalogue.Item("Tuyết Liên Ngàn Năm","Dược liệu")}.Distinct().Count()==3,"Herbs indistinguishable");});
                capture("icons-inventory");yield return null;
                inventory.tabs[3].button.onClick.Invoke();foreach(var x in Wait(.3f))yield return x;
                Test("material-icons-"+size,"Material tab shows five distinct matching herb, ore and dust images",()=>{
                    var rows=inventory.owner.contentContainer.GetComponentsInChildren<FoundationItem>();
                    Expect(rows.Length==5,"Native material fixture count");
                    foreach(var row in rows)Expect(row.icon.enabled&&row.icon.sprite==catalogue.Item(row.title.text,row.detail.text),"Material mapping: "+row.title.text);
                    Expect(rows.Select(r=>r.icon.sprite).Distinct().Count()==5,"Materials share wrong image");});
                capture("icons-materials");yield return null;
                inventory.tabs[2].button.onClick.Invoke();foreach(var x in Wait(.3f))yield return x;
                Test("pill-icons-"+size,"Native pill page distinguishes both recipes and keeps four quality states",()=>{
                    var rows=inventory.owner.contentContainer.GetComponentsInChildren<FoundationItem>();
                    Expect(rows.Length>=8,"Native pill fixture count");
                    foreach(var row in rows)Expect(row.icon.enabled&&row.icon.sprite==catalogue.Item(row.title.text,row.detail.text),"Pill mapping: "+row.title.text);
                    Expect(rows.Select(r=>r.icon.sprite).Distinct().Count()==2,"Recipes share wrong image");
                    Expect(rows.Select(r=>r.detail.text).Distinct().Count()==4,"Quality states lost");});
                capture("icons-pills");yield return null;
                var first=inventory.owner.contentContainer.GetComponentsInChildren<FoundationItem>().First(i=>catalogue.Item(i.title.text,i.detail.text)!=null);
                first.action.button.onClick.Invoke();foreach(var x in Wait(.3f))yield return x;
                Test("detail-art-"+size,"Item detail reuses the exact selected inventory illustration",()=>Expect(inventory.sheet.art.sprite==first.icon.sprite&&inventory.sheet.art.enabled,"Detail art mismatch"));capture("icons-item-detail");yield return null;
                inventory.sheet.Close();StorageModalUI.Instance.Close();foreach(var x in Wait(.4f))yield return x;
                foreach(var route in new[]{NavigationRoute.Body,NavigationRoute.Alchemy,NavigationRoute.Artifact,NavigationRoute.Formation})
                {
                    nav.OpenRoute(route);foreach(var x in Wait(.4f))yield return x;
                    var screen=UnityEngine.Object.FindObjectsByType<CultivationScreen>().First(s=>s.isActiveAndEnabled);
                    int selections=route==NavigationRoute.Artifact?4:route==NavigationRoute.Formation?6:1;
                    for(int i=0;i<selections;i++)
                    {
                        screen.Select(i);yield return null;
                        Test("subject-art-"+route+"-"+i+"-"+size,"Selected native cultivation subject uses its matching generated art",()=>{
                            var expected=route==NavigationRoute.Alchemy?catalogue.cauldron:route==NavigationRoute.Body?(NavigationArtCatalogue.Current?.BodyNode(screen.Selected)??catalogue.body):route==NavigationRoute.Artifact?catalogue.artifacts[screen.Selected]:catalogue.formations[screen.Selected];
                            Expect(screen.subjectArt.enabled&&screen.subjectArt.sprite==expected&&screen.subjectArt.preserveAspect,"Wrong subject image");});
                        capture("icons-"+route+"-"+i);yield return null;
                    }
                    nav.Back();foreach(var x in Wait(.4f))yield return x;
                }
                nav.SelectGroup(NavigationGroup.Equipment);foreach(var x in Wait(.3f))yield return x;capture("icons-navigation");yield return null;
                nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait(.4f))yield return x;
            }
        }
    }
}
#endif

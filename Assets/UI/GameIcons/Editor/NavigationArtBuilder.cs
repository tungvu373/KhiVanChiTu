#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Cultivation;

namespace KhiVanChiTu.UI.GameIcons.Editor
{
    public static class NavigationArtBuilder
    {
        public const string Folder="Assets/UI/GameIcons/RouteArt/";
        public const string CataloguePath="Assets/Resources/UI/NavigationArtCatalogue.asset";
        public static readonly string[] BodyNames={"BodyVitality","BodyGuard","BodyRecovery","BodyForce","BodyDrain","BodyFocus"};
        public static string[] Names=>GlobalNavigationController.Destinations.Select(d=>"Route"+d.route).Concat(BodyNames).ToArray();
        static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Folder+name+".png");
        [MenuItem("TuTien/AI Icons/Apply unique route and body icons")]
        public static void Apply()
        {
            KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Require();
            var missing=Names.Where(n=>!File.Exists(Folder+n+".png")).ToArray();
            if(missing.Length>0)throw new InvalidOperationException("Generated PNGs required: "+string.Join(", ",missing));
            AssetDatabase.Refresh();
            foreach(var name in Names)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+name+".png");
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;importer.isReadable=false;importer.spriteBorder=Vector4.zero;
                importer.maxTextureSize=name.StartsWith("Route",StringComparison.Ordinal)?256:512;
                importer.spritePixelsPerUnit=100;importer.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                if(Art(name)==null)throw new InvalidOperationException("Sprite import failed: "+name);
            }
            var catalogue=AssetDatabase.LoadAssetAtPath<NavigationArtCatalogue>(CataloguePath);
            if(catalogue==null){catalogue=ScriptableObject.CreateInstance<NavigationArtCatalogue>();AssetDatabase.CreateAsset(catalogue,CataloguePath);}
            catalogue.routes=GlobalNavigationController.Destinations.Select(d=>new NavigationArtCatalogue.RouteArt{route=d.route,icon=Art("Route"+d.route)}).ToArray();
            catalogue.bodyNodes=BodyNames.Select(Art).ToArray();EditorUtility.SetDirty(catalogue);NavigationArtCatalogue.InvalidateCache();
            var nav=UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include);
            if(nav==null)throw new InvalidOperationException("Existing mobile navigation required");
            nav.ApplyRouteArtwork();
            foreach(var card in nav.cards)
            {
                var icon=card.action.GetComponentInChildren<NavigationIcon>(true);
                if(icon==null)throw new InvalidOperationException("Missing existing route graphic: "+card.route);
                var rect=icon.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(80,0);rect.sizeDelta=new Vector2(104,104);
                var label=card.action.label.rectTransform;var min=label.offsetMin;min.x=152;label.offsetMin=min;
                var desc=card.description.rectTransform;min=desc.offsetMin;min.x=152;desc.offsetMin=min;
                EditorUtility.SetDirty(icon);EditorUtility.SetDirty(rect);EditorUtility.SetDirty(label);EditorUtility.SetDirty(desc);
            }
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach(var body in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CultivationScreen>(true)).Where(s=>s.kind==CultivationScreenKind.Body))
            {body.subjectArt.sprite=catalogue.bodyNodes[0];body.subjectArt.preserveAspect=true;body.subjectArt.color=Color.white;EditorUtility.SetDirty(body.subjectArt);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/Unique_Route_Icons/Evidence");
            File.WriteAllText("Docs/Unique_Route_Icons/Evidence/import.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,routeCount=catalogue.routes.Length,bodyCount=catalogue.bodyNodes.Length,assets=Names.Select(n=>new{name=n,path=Folder+n+".png",width=Art(n).texture.width,height=Art(n).texture.height})},Newtonsoft.Json.Formatting.Indented));
            Debug.Log("[Route Icons] Applied 24 distinct route icons and six body-node illustrations through Editor API.");
        }
    }
}
#endif

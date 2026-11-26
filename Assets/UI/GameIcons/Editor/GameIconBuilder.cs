#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using KhiVanChiTu.UI.CombatHUD;
using KhiVanChiTu.UI.Cultivation;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.GameIcons.Editor
{
    public static class GameIconBuilder
    {
        public const string Folder="Assets/UI/GameIcons/Art/";
        public const string CataloguePath="Assets/Resources/UI/GameIconCatalogue.asset";
        public const string SwordVariant="Assets/UI/GameIcons/Prefabs/MergeItemIllustrated.prefab";
        public static readonly string[] Names=Enumerable.Range(1,10).Select(i=>"Sword"+i.ToString("00")).Concat(new[]{"SkillSword","SkillPalm","SkillThunder","SkillYinYang","PillCultivation","PillBreakthrough","HerbLingzhi","HerbCondensing","HerbSnowLotus","Ore","Dust","Chest","Robe","Crown","Boots","Amulet","Body","Cauldron","SpiritLotus","FrostFormation","BloodFormation","GoldenBell","SpiritStone","FortuneCoin","RealmPortal"}).ToArray();
        static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Folder+name+".png");
        [MenuItem("TuTien/AI Icons/Import and apply illustrated icon set")]
        public static void Apply()
        {
            KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Require();
            var missing=Names.Where(n=>!File.Exists(Folder+n+".png")).ToArray();
            if(missing.Length>0)throw new InvalidOperationException("Real generated PNGs required: "+string.Join(", ",missing));
            AssetDatabase.Refresh();
            foreach(var name in Names)
            {
                var path=Folder+name+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;importer.isReadable=false;importer.spriteBorder=Vector4.zero;
                importer.maxTextureSize=512;importer.spritePixelsPerUnit=100;importer.textureCompression=TextureImporterCompression.Uncompressed;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                if(Sprite(name)==null)throw new InvalidOperationException("Sprite import failed: "+path);
            }
            Directory.CreateDirectory("Assets/Resources/UI");Directory.CreateDirectory("Assets/UI/GameIcons/Prefabs");AssetDatabase.Refresh();
            var catalogue=AssetDatabase.LoadAssetAtPath<GameIconCatalogue>(CataloguePath);
            if(catalogue==null){catalogue=ScriptableObject.CreateInstance<GameIconCatalogue>();AssetDatabase.CreateAsset(catalogue,CataloguePath);}
            catalogue.swords=Enumerable.Range(1,10).Select(i=>Sprite("Sword"+i.ToString("00"))).ToArray();
            catalogue.skills=new[]{"SkillSword","SkillPalm","SkillThunder","SkillYinYang"}.Select(Sprite).ToArray();
            catalogue.herbs=new[]{"HerbLingzhi","HerbCondensing","HerbSnowLotus"}.Select(Sprite).ToArray();
            catalogue.artifacts=new[]{"Robe","Crown","Boots","Amulet"}.Select(Sprite).ToArray();
            catalogue.formations=new[]{"SkillThunder","SpiritLotus","FrostFormation","SkillSword","BloodFormation","GoldenBell"}.Select(Sprite).ToArray();
            catalogue.navigation=new[]{"SkillSword","Robe","SpiritLotus","RealmPortal","Chest"}.Select(Sprite).ToArray();
            catalogue.progression=new[]{"SkillSword","SpiritStone","SpiritLotus","Cauldron","Body"}.Select(Sprite).ToArray();
            catalogue.cultivationPill=Sprite("PillCultivation");catalogue.breakthroughPill=Sprite("PillBreakthrough");
            catalogue.ore=Sprite("Ore");catalogue.dust=Sprite("Dust");catalogue.chest=Sprite("Chest");catalogue.cauldron=Sprite("Cauldron");catalogue.body=Sprite("Body");catalogue.stone=Sprite("SpiritStone");catalogue.fortune=Sprite("FortuneCoin");
            EditorUtility.SetDirty(catalogue);GameIconCatalogue.InvalidateCache();
            var manager=UnityEngine.Object.FindAnyObjectByType<MergeManager>(FindObjectsInactive.Include);
            if(manager==null||manager.mergeItemPrefab==null)throw new InvalidOperationException("Existing mobile MergeManager prefab required");
            UpdateSwordVariant(manager.mergeItemPrefab,catalogue.swords);
            manager.mergeItemPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(SwordVariant);EditorUtility.SetDirty(manager);
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach(var item in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MergeItem>(true)))
            {item.levelSprites=catalogue.swords;EditorUtility.SetDirty(item);var image=item.GetComponent<UnityEngine.UI.Image>();if(image!=null){image.sprite=catalogue.swords[Mathf.Clamp(item.level-1,0,9)];image.preserveAspect=true;EditorUtility.SetDirty(image);}}
            var hud=UnityEngine.Object.FindAnyObjectByType<CombatHudPresentation>(FindObjectsInactive.Include);
            for(int i=0;i<hud.skillViews.Length;i++){var image=hud.skillViews[i].icon;image.sprite=catalogue.skills[i];image.preserveAspect=true;image.color=Color.white;EditorUtility.SetDirty(image);}
            var navigation=UnityEngine.Object.FindAnyObjectByType<KhiVanChiTu.UI.Navigation.GlobalNavigationController>(FindObjectsInactive.Include);
            foreach(var icon in navigation.groupIcons){icon.rectTransform.sizeDelta=new Vector2(80,80);EditorUtility.SetDirty(icon.rectTransform);}
            hud.ApplyLayout();
            foreach(var inventory in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<InventoryPresentation>(true)))
            {inventory.pill=catalogue.cultivationPill;inventory.herb=catalogue.herbs[0];inventory.ore=catalogue.ore;inventory.dust=catalogue.dust;inventory.pending=catalogue.chest;EditorUtility.SetDirty(inventory);}
            foreach(var screen in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CultivationScreen>(true)))
            {
                screen.subjectArt.sprite=screen.kind==CultivationScreenKind.Alchemy?catalogue.cauldron:screen.kind==CultivationScreenKind.Body?catalogue.body:screen.kind==CultivationScreenKind.Artifact?catalogue.artifacts[0]:catalogue.formations[0];
                screen.subjectArt.preserveAspect=true;screen.subjectArt.type=UnityEngine.UI.Image.Type.Simple;screen.subjectArt.color=Color.white;EditorUtility.SetDirty(screen.subjectArt);
            }
            foreach(var icon in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<KhiVanChiTu.UI.Navigation.NavigationIcon>(true)))icon.SetAllDirty();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/AI_Icon_Refresh/Evidence");
            File.WriteAllText("Docs/AI_Icon_Refresh/Evidence/import.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scene=scene.path,assetCount=Names.Length,catalogue=CataloguePath,swordVariant=SwordVariant,swordLevelCap=SwordRules.MaxLevel,assets=Names.Select(n=>new{name=n,path=Folder+n+".png",textureWidth=Sprite(n).texture.width,textureHeight=Sprite(n).texture.height})},Newtonsoft.Json.Formatting.Indented));
            Debug.Log("[AI Icons] Applied "+Names.Length+" actual generated icons. Native data and callbacks preserved.");
        }
        static void UpdateSwordVariant(GameObject original,Sprite[] swords)
        {
            bool exists=AssetDatabase.LoadAssetAtPath<GameObject>(SwordVariant)!=null;
            var root=exists?PrefabUtility.LoadPrefabContents(SwordVariant):(GameObject)PrefabUtility.InstantiatePrefab(original);
            try
            {
                var item=root.GetComponent<MergeItem>();if(item==null)throw new InvalidOperationException("MergeItem component must be retained");
                item.levelSprites=swords;var image=root.GetComponent<UnityEngine.UI.Image>();image.sprite=swords[0];image.preserveAspect=true;image.color=Color.white;
                PrefabUtility.SaveAsPrefabAsset(root,SwordVariant);
            }
            finally{if(exists)PrefabUtility.UnloadPrefabContents(root);else UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}
#endif

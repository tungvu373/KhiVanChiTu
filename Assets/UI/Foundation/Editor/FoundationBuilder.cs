using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace KhiVanChiTu.UI.Foundation.Editor
{
    public static class FoundationBuilder
    {
        public const string Base = "Assets/UI/Foundation";
        public const string ScenePath = Base + "/Showcase/UIFoundationShowcase.unity";
        public static string Glyphs
        {
            get
            {
                string s = "\u0103\u0102\u00e2\u00c2\u0111\u0110\u00ea\u00ca\u00f4\u00d4\u01a1\u01a0\u01b0\u01af\u00e0\u00e1\u00e3\u00c0\u00c1\u00c3\u00e8\u00e9\u00c8\u00c9\u00ec\u00ed\u00cc\u00cd\u00f2\u00f3\u00f5\u00d2\u00d3\u00d5\u00f9\u00fa\u00d9\u00da\u00fd\u00dd\u0129\u0128\u0169\u0168\u00b7\u2039\u2192";
                for (int i = 32; i <= 126; i++) s += (char)i;
                for (int i = 0x1EA0; i <= 0x1EF9; i++) s += (char)i;
                return s;
            }
        }
        static TMP_FontAsset Font(string source, string name)
        {
            string path = Base + "/Theme/" + name + ".asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null) return font;
            var original = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(source);
            if (original == null) throw new InvalidOperationException("Font not found: " + source);
            if(Resources.Load<TMP_Settings>("TMP Settings")==null)throw new InvalidOperationException("TMP Essentials must be imported before generating the SDF fonts.");
            font = TMP_FontAsset.CreateFontAsset(original, 72, 8, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            if(font==null)throw new InvalidOperationException("TMP could not create the SDF font from "+source);
            font.name = name;
            if (!font.TryAddCharacters(Glyphs, out string missing)) throw new InvalidOperationException("Missing Vietnamese glyphs: " + missing);
            AssetDatabase.CreateAsset(font, path);
            if (font.material != null) AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var texture in font.atlasTextures) if (texture != null && !AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            EditorUtility.SetDirty(font); return font;
        }
        static Sprite Surface()
        {
            string path = Base + "/Theme/Surface9Slice.png";
            if (File.Exists(path)) return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var texture = new Texture2D(64,64,TextureFormat.RGBA32,false);
            for (int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float dx=Mathf.Max(12-x,0,x-51),dy=Mathf.Max(12-y,0,y-51);
                float coverage=Mathf.Clamp01(12.5f-Mathf.Sqrt(dx*dx+dy*dy));
                float light = y > 59 ? 1.08f : y < 4 ? .88f : 1;
                texture.SetPixel(x,y,new Color(light,light,light,coverage));
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spriteBorder=new Vector4(14,14,14,14); importer.spritePixelsPerUnit=100; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.wrapMode=TextureWrapMode.Clamp; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        [MenuItem("TuTien/UI Foundation/1. Create Library and Showcase")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before creating the isolated showcase.");
            if (File.Exists(ScenePath)) { Debug.Log("[Foundation] Showcase already exists; existing assets preserved."); return; }
            Directory.CreateDirectory(Base+"/Theme"); Directory.CreateDirectory(Base+"/Prefabs"); Directory.CreateDirectory(Base+"/Showcase"); AssetDatabase.Refresh();
            var theme = AssetDatabase.LoadAssetAtPath<FoundationTheme>(Base+"/Theme/JadeFoundation.asset");
            if(theme==null) { theme=ScriptableObject.CreateInstance<FoundationTheme>(); AssetDatabase.CreateAsset(theme,Base+"/Theme/JadeFoundation.asset"); }
            theme.bodyFont=Font("Assets/Resources/SourceSansPro-Regular.ttf","VietnameseBodySDF");
            theme.headingFont=Font("Assets/Import/Asset/Font/Source Sans Pro/SourceSansPro-Semibold.ttf","VietnameseHeadingSDF");
            theme.surfaceSprite=Surface(); EditorUtility.SetDirty(theme); AssetDatabase.SaveAssets();
            var original=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene); var f=new FoundationFactory(theme);
                foreach(FoundationKind kind in Enum.GetValues(typeof(FoundationKind)))
                {
                    string path=Base+"/Prefabs/"+kind+".prefab";
                    if(File.Exists(path))continue;
                    var go=f.Create(kind,null); PrefabUtility.SaveAsPrefabAsset(go,path); UnityEngine.Object.DestroyImmediate(go);
                }
                BuildShowcase(f);
                if (!EditorSceneManager.SaveScene(scene,ScenePath)) throw new IOException("Showcase scene save failed.");
            }
            finally
            {
                if(original.IsValid()&&original.isLoaded)SceneManager.SetActiveScene(original);
                EditorSceneManager.CloseScene(scene,true);
            }
            AssetDatabase.SaveAssets(); Debug.Log("[Foundation] Created 15 prefabs, theme, Vietnamese SDF fonts and isolated showcase. No gameplay scene modified.");
        }
        static RectTransform Layer(string name,RectTransform safe,int order)
        {
            var r=FoundationFactory.Rect(name,safe); FoundationFactory.Fill(r);
            var canvas=r.gameObject.AddComponent<Canvas>(); canvas.overrideSorting=true;canvas.sortingOrder=order;
            r.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();r.gameObject.AddComponent<CanvasGroup>();return r;
        }
        static T Prefab<T>(FoundationKind kind,Transform parent) where T:Component
        {var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Base+"/Prefabs/"+kind+".prefab");return ((GameObject)PrefabUtility.InstantiatePrefab(asset,parent)).GetComponent<T>();}
        static void Height(Component component,float height)
        {var le=component.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minHeight=le.preferredHeight=height;le.flexibleWidth=1;}
        static void BuildShowcase(FoundationFactory f)
        {
            var camera=new GameObject("ShowcaseCamera",typeof(Camera)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=f.theme.ink;camera.cullingMask=0;
            var canvas=FoundationFactory.Rect("FoundationCanvas",null);canvas.gameObject.AddComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=f.theme.referenceResolution;scaler.matchWidthOrHeight=0;
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();f.Surface(canvas,f.theme.ink);
            var root=canvas.gameObject.AddComponent<FoundationRoot>();root.theme=f.theme;root.safeArea=FoundationFactory.Rect("SafeArea",canvas);FoundationFactory.Fill(root.safeArea);
            root.gameplayHUD=Layer("GameplayHUD",root.safeArea,100);root.navigation=Layer("Navigation",root.safeArea,110);root.screenOverlay=Layer("ScreenOverlay",root.safeArea,200);root.modal=Layer("Modal",root.safeArea,300);root.notification=Layer("Notification",root.safeArea,400);
            var d=canvas.gameObject.AddComponent<FoundationShowcase>();d.root=root;
            d.header=Prefab<FoundationAction>(FoundationKind.ScreenHeader,root.gameplayHUD);var hr=(RectTransform)d.header.transform;FoundationFactory.Fill(hr,36,0,36,32);hr.anchorMin=new Vector2(0,1);hr.offsetMin=new Vector2(36,-180);
            var scroll=FoundationFactory.Rect("ComponentGallery",root.gameplayHUD);FoundationFactory.Fill(scroll,36,180,36,208);f.Surface(scroll,Color.clear,true);
            var sr=scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();sr.horizontal=false;sr.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport=FoundationFactory.Rect("Viewport",scroll);FoundationFactory.Fill(viewport);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var content=FoundationFactory.Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;content.anchoredPosition=Vector2.zero;
            var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.spacing=24;layout.padding=new RectOffset(0,0,8,32);layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            var fit=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;sr.viewport=viewport;sr.content=content;
            var intro=f.Text("Introduction",content,"NG\u1eccC TR\u1ea6M \u00b7 V\u00c0NG C\u1ed4\nUI Design System  /  Component Gallery",32);intro.color=f.theme.muted;Height(intro,136);
            d.tabs=Prefab<FoundationTabs>(FoundationKind.TabBar,content);Height(d.tabs,132);
            d.resource=Prefab<FoundationValue>(FoundationKind.ResourceDisplay,content);Height(d.resource,116);
            d.stat=Prefab<FoundationValue>(FoundationKind.StatRow,content);d.stat.Set("C\u01b0\u1eddng \u0111\u1ed9","72 \u2192 86");Height(d.stat,116);
            d.progress=Prefab<FoundationProgress>(FoundationKind.ProgressBar,content);Height(d.progress,80);
            d.primary=Prefab<FoundationAction>(FoundationKind.PrimaryButton,content);Height(d.primary,132);d.primary.label.text="T\u0103ng ti\u1ebfn \u0111\u1ed9";
            d.secondary=Prefab<FoundationAction>(FoundationKind.SecondaryButton,content);Height(d.secondary,132);
            d.card=Prefab<FoundationItem>(FoundationKind.ItemCard,content);Height(d.card,184);
            var row=FoundationFactory.Rect("ItemAndHelp",content);Height(row,280);var horizontal=row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();horizontal.spacing=24;horizontal.childControlWidth=horizontal.childControlHeight=true;horizontal.childForceExpandWidth=false;
            d.slot=Prefab<FoundationItem>(FoundationKind.ItemSlot,row);var slotSize=d.slot.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();slotSize.minWidth=slotSize.preferredWidth=280;
            d.help=Prefab<FoundationAction>(FoundationKind.SecondaryButton,row);d.help.label.text="Gi\u1eef \u0111\u1ec3 xem\nTooltip";var helpSize=d.help.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();helpSize.flexibleWidth=1;
            d.openScreen=Prefab<FoundationAction>(FoundationKind.SecondaryButton,content);Height(d.openScreen,132);d.openScreen.label.text="FullScreenPanel";
            d.openSheet=Prefab<FoundationAction>(FoundationKind.SecondaryButton,content);Height(d.openSheet,132);d.openSheet.label.text="BottomSheet";
            d.openReward=Prefab<FoundationAction>(FoundationKind.PrimaryButton,content);Height(d.openReward,132);d.openReward.label.text="RewardModal";
            d.openConfirm=Prefab<FoundationAction>(FoundationKind.SecondaryButton,content);Height(d.openConfirm,132);d.openConfirm.label.text="ConfirmDialog";
            d.showToast=Prefab<FoundationAction>(FoundationKind.SecondaryButton,content);Height(d.showToast,132);d.showToast.label.text="Toast";
            d.fullScreen=Prefab<FoundationPanel>(FoundationKind.FullScreenPanel,root.screenOverlay);d.sheet=Prefab<FoundationPanel>(FoundationKind.BottomSheet,root.modal);d.reward=Prefab<FoundationPanel>(FoundationKind.RewardModal,root.modal);d.confirm=Prefab<FoundationPanel>(FoundationKind.ConfirmDialog,root.modal);
            d.toast=Prefab<FoundationNotice>(FoundationKind.Toast,root.notification);var tr=(RectTransform)d.toast.transform;FoundationFactory.Fill(tr,60,32,60,0);tr.anchorMax=new Vector2(1,0);tr.offsetMax=new Vector2(-60,164);
            d.tooltip=Prefab<FoundationNotice>(FoundationKind.Tooltip,root.notification);var tt=(RectTransform)d.tooltip.transform;FoundationFactory.Fill(tt,60,0,60,32);tt.anchorMin=new Vector2(0,1);tt.offsetMin=new Vector2(60,-260);
            d.status=f.Text("Status",root.navigation,"UI FOUNDATION  \u00b7  SAFE AREA",28,TextAlignmentOptions.Center);var st=d.status.rectTransform;FoundationFactory.Fill(st,36,32,36,0);st.anchorMax=new Vector2(1,0);st.offsetMax=new Vector2(-36,144);d.status.color=f.theme.muted;
            d.sampleSword=AssetDatabase.LoadAllAssetsAtPath("Assets/Import/Asset/kiem.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="kiem_2");
            d.card.icon.sprite=d.slot.icon.sprite=d.sampleSword;d.card.icon.color=d.slot.icon.color=Color.white;
            var es=new GameObject("FoundationEventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
        [MenuItem("TuTien/UI Foundation/2. Open Isolated Showcase")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Current scene has unsaved edits. Preserve them before opening the showcase.");
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }
        [MenuItem("TuTien/UI Foundation/4. Ensure Showcase Camera")]
        public static void EnsureCamera()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open the isolated showcase in Edit Mode first.");
            foreach(var kind in new[]{FoundationKind.ItemSlot,FoundationKind.ItemCard,FoundationKind.ProgressBar})
            {
                string path=Base+"/Prefabs/"+kind+".prefab";var contents=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var item=contents.GetComponent<FoundationItem>();if(item!=null){item.icon.type=UnityEngine.UI.Image.Type.Simple;item.icon.preserveAspect=true;}
                    var progress=contents.GetComponent<FoundationProgress>();if(progress!=null)progress.fill.GetComponent<UnityEngine.UI.Image>().color=new Color32(40,99,79,255);
                    PrefabUtility.SaveAsPrefabAsset(contents,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(contents);}
            }
            if(Camera.main==null&&UnityEngine.Object.FindAnyObjectByType<Camera>()==null)
            {var c=new GameObject("ShowcaseCamera",typeof(Camera)).GetComponent<Camera>();c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color32(16,40,42,255);c.cullingMask=0;}
            var demo=UnityEngine.Object.FindAnyObjectByType<FoundationShowcase>();
            if(demo!=null)
            {
                demo.sampleSword=AssetDatabase.LoadAllAssetsAtPath("Assets/Import/Asset/kiem.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="kiem_2");
                demo.slot.icon.sprite=demo.card.icon.sprite=demo.sampleSword;
                demo.slot.icon.preserveAspect=demo.card.icon.preserveAspect=true;
                EditorUtility.SetDirty(demo);
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
    }
}

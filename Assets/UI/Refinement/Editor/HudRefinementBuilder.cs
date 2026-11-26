#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KhiVanChiTu.UI.CombatHUD;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.SecretRealm;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.Refinement.Editor
{
    public static class HudRefinementBuilder
    {
        const string Art="Assets/UI/Refinement/Art/JadeSurface.png";
        [MenuItem("TuTien/UI Refinement/Apply audited HUD polish")]
        public static void Apply()
        {
            KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Require();
            var hud=UnityEngine.Object.FindAnyObjectByType<CombatHudPresentation>(FindObjectsInactive.Include);
            var nav=UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include);
            if(hud==null||nav==null||!File.Exists(Art))throw new InvalidOperationException("Audited mobile scene and generated artwork required");
            var imp=(TextureImporter)AssetImporter.GetAtPath(Art);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
            imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.isReadable=false;
            imp.spriteBorder=new Vector4(180,180,180,180);imp.spritePixelsPerUnit=100;
            imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();
            var art=AssetDatabase.LoadAssetAtPath<Sprite>(Art);hud.hudSurface=art;
            // Static artwork only. Fills/cooldowns/icons keep their existing sprite, type,
            // mask, anchors and bindings. No controls or persistent callbacks are rebuilt.
            foreach(var root in new[]{hud.legacy.profile,hud.legacy.wallet,hud.pause,hud.legacy.focusButton,(RectTransform)hud.legacy.menuButton.transform,hud.task,hud.wave,hud.boss})Skin(root,art);
            foreach(var skill in hud.skillViews)Skin(skill.button.transform,art);
            foreach(var action in hud.task.GetComponentsInChildren<FoundationAction>(true))Skin(action.transform,art);
            foreach(var action in nav.groupButtons)Skin(action.transform,art);
            foreach(var bar in new[]{hud.hp,hud.mp,hud.bossHealth,hud.waveProgress,hud.cultivation})
            {
                Skin(bar.transform,art);
                // Track geometry stays intact; do not touch its dynamic fill.
                var track=bar.fill.parent.GetComponent<UnityEngine.UI.Image>();
                if(track!=null){track.sprite=art;track.type=UnityEngine.UI.Image.Type.Sliced;track.pixelsPerUnitMultiplier=10;track.color=new Color(.6f,.68f,.65f,1);EditorUtility.SetDirty(track);}
            }
            hud.drawer.externalDockLayout=true;
            // Restore control is already a sibling of Upper/LowerHUD, not a child
            // of the hidden dock. Explicitly exclude it from future parent layouts.
            var le=hud.legacy.focusButton.GetComponent<UnityEngine.UI.LayoutElement>()??hud.legacy.focusButton.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.ignoreLayout=true;
            hud.ApplyLayout();
            var notifications=nav.GetComponent<NavigationNotifications>()??nav.gameObject.AddComponent<NavigationNotifications>();notifications.navigation=nav;
            notifications.groups=nav.groupButtons.Select(a=>Badge(a.transform)).ToArray();
            notifications.routes=nav.cards.Select(c=>Badge(c.action.transform)).ToArray();
            foreach(var card in nav.cards)if(card.route==NavigationRoute.Talents){card.action.label.text="Đạo Vận";card.action.transform.SetAsFirstSibling();EditorUtility.SetDirty(card.action.label);}
            AlignRealms();
            AlignRealmCardPrefab();
            EditorUtility.SetDirty(hud);EditorUtility.SetDirty(hud.drawer);EditorUtility.SetDirty(notifications);
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[UI Refinement] Generated HUD artwork, stable toggle, route relocation, badges and realm text saved.");
        }
        static void Skin(Transform root,Sprite art)
        {
            if(root==null)return;var image=root.GetComponent<UnityEngine.UI.Image>();
            if(image!=null){image.sprite=art;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=10;image.color=Color.white;EditorUtility.SetDirty(image);}
            // Accent bars duplicated the generated sprite's thin perimeter.
            var accent=root.Find("P2Accent");if(accent!=null)accent.gameObject.SetActive(false);
        }
        static NotificationBadge Badge(Transform parent)
        {
            var r=parent.Find("AvailableBadge") as RectTransform;
            if(r==null)r=FoundationFactory.Rect("AvailableBadge",parent);
            r.anchorMin=r.anchorMax=r.pivot=Vector2.one;r.anchoredPosition=new Vector2(-12,-12);r.sizeDelta=new Vector2(22,22);r.SetAsLastSibling();
            if(r.GetComponent<CanvasRenderer>()==null)r.gameObject.AddComponent<CanvasRenderer>();
            var badge=r.GetComponent<NotificationBadge>()??r.gameObject.AddComponent<NotificationBadge>();badge.color=new Color32(216,79,88,255);badge.raycastTarget=false;
            var le=r.GetComponent<UnityEngine.UI.LayoutElement>()??r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.ignoreLayout=true;badge.SetVisible(false);return badge;
        }
        static void Center(TMP_Text text)
        {if(text==null)return;text.alignment=TextAlignmentOptions.Center;text.margin=Vector4.zero;EditorUtility.SetDirty(text);}
        public static void AlignRealms()
        {
            var play=UnityEngine.Object.FindAnyObjectByType<RealmPlayView>(FindObjectsInactive.Include);
            // Title is centered in the content region left of Exit, with symmetric
            // internal padding. The exit target and arena retain all geometry.
            Center(play.title);Center(play.mode);Center(play.objective);Center(play.hint);Center(play.timer);Center(play.score);Center(play.progress.label);
            FoundationFactory.Fill(play.timer.rectTransform,20,0,20,0);play.timer.rectTransform.anchorMax=new Vector2(.5f,1);
            FoundationFactory.Fill(play.score.rectTransform,20,0,20,0);play.score.rectTransform.anchorMin=new Vector2(.5f,0);
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach(var hub in root.GetComponentsInChildren<RealmHubView>(true))
                    foreach(var card in hub.cards){Center(card.title);Center(card.mode);Center(card.enter.label);card.description.alignment=TextAlignmentOptions.TopLeft;card.description.textWrappingMode=TextWrappingModes.Normal;}
            var result=UnityEngine.Object.FindAnyObjectByType<RealmResultView>(FindObjectsInactive.Include);
            foreach(var t in new[]{result.title,result.status,result.score,result.receipt,result.confirm.label})Center(t);
        }
        static void AlignRealmCardPrefab()
        {
            const string path="Assets/UI/SecretRealm/Prefabs/RealmCard.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var card=root.GetComponent<RealmCard>();
                Center(card.title);Center(card.mode);Center(card.enter.label);
                card.description.alignment=TextAlignmentOptions.TopLeft;
                card.description.textWrappingMode=TextWrappingModes.Normal;
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Equipment.Editor;
using KhiVanChiTu.UI.Progression;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.SecretRealm.Editor
{
    public static class RealmRedesignBuilder
    {
        const string Folder="Assets/UI/SecretRealm/Prefabs";
        static FoundationFactory F=>EquipmentRedesignBuilder.Factory;
        static FoundationTheme Theme=>EquipmentRedesignBuilder.Theme;
        static Sprite[] Art()=>new[]{"Assets/Resources/SecretRealm/bicanh_horde_survival.jpg","Assets/Resources/SecretRealm/bicanh_barrier_break.jpg","Assets/Resources/SecretRealm/bicanh_dan_cauldron.jpg","Assets/UI/SecretRealm/Art/LightningPool.png","Assets/UI/SecretRealm/Art/AncientSwordTomb.png"}.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
        static void Stack(RectTransform root,int pad=0,int gap=12)
        {var g=root.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();g.padding=new RectOffset(pad,pad,pad,pad);g.spacing=gap;g.childControlHeight=g.childControlWidth=true;g.childForceExpandHeight=false;}
        static TMP_Text Flow(Transform parent,string name,string text,int size,Color tone)
        {var t=F.Text(name,parent,text,size);t.color=tone;var le=t.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minHeight=size+14;return t;}
        static TMP_Text Text(Transform parent,string name,string text,float top,float height,int size=32,Color? tone=null,float left=28,float right=28)
        {var t=F.Text(name,parent,text,size);RpgUI.Top(t.rectTransform,top,height,left,right);t.color=tone??Theme.ivory;return t;}
        static UnityEngine.UI.Image ArtBox(Transform parent,string name,Sprite art)
        {
            var clip=RpgUI.Rect(name,parent);clip.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var image=RpgUI.Rect("Illustration",clip);RpgUI.Fill(image);var img=image.gameObject.AddComponent<UnityEngine.UI.Image>();img.sprite=art;img.color=Color.white;img.raycastTarget=false;
            var fit=image.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=art!=null?art.rect.width/art.rect.height:1;return img;
        }
        static RectTransform Surface(Transform parent,string name)
        {var safe=EquipmentRedesignBuilder.Scope(name,parent);safe.gameObject.AddComponent<SafeAreaFitter>();safe.gameObject.AddComponent<ProgressionInputShield>();return safe;}
        static FoundationAction Close(Transform header,string caption)
        {var b=F.Action("Exit",header,caption);var r=(RectTransform)b.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,1);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(168,132);return b;}
        static RealmCard Card()
        {
            var r=RpgUI.Rect("RealmCard",null);r.sizeDelta=new Vector2(1024,660);r.gameObject.AddComponent<RpgViewScope>();F.Surface(r,Theme.surface,true);RpgUI.Height(r,660);
            var card=r.gameObject.AddComponent<RealmCard>();card.artwork=ArtBox(r,"Artwork",null);var clip=(RectTransform)card.artwork.transform.parent;RpgUI.Fill(clip,0,164,0,0);clip.anchorMax=new Vector2(.34f,1);
            var body=RpgUI.Rect("Details",r);RpgUI.Fill(body,24,180,24,20);body.anchorMin=new Vector2(.34f,0);Stack(body,0,10);
            card.mode=Flow(body,"Mode","",26,Theme.gold);card.title=Flow(body,"Name","",40,Theme.ivory);card.title.font=Theme.headingFont;card.description=Flow(body,"Description","",28,Theme.muted);card.reward=Flow(body,"Rewards","",28,Theme.jade);card.record=Flow(body,"Record","",26,Theme.muted);
            card.condition=F.Text("Condition",r,"",26);RpgUI.Bottom(card.condition.rectTransform,132,42,24,24);
            card.enter=F.Action("Enter",r,"Vào bí cảnh",true);RpgUI.Bottom((RectTransform)card.enter.transform,16,112,16,16);
            return card;
        }
        static RealmRewardRow Reward()
        {
            var r=RpgUI.Rect("RealmRewardRow",null);r.sizeDelta=new Vector2(960,100);F.Surface(r,Theme.surface);RpgUI.Height(r,100);var row=r.gameObject.AddComponent<RealmRewardRow>();
            var symbol=RpgUI.Rect("Symbol",r);symbol.anchorMin=symbol.anchorMax=new Vector2(0,.5f);symbol.anchoredPosition=new Vector2(48,0);symbol.sizeDelta=new Vector2(48,48);row.icon=F.Surface(symbol,Theme.gold);row.icon.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/SecretRealm/Tex_VFX_RuneTelegraph.png");row.icon.type=UnityEngine.UI.Image.Type.Simple;row.icon.preserveAspect=true;
            row.label=F.Text("Label",r,"",30);RpgUI.Fill(row.label.rectTransform,96,12,280,12);row.value=F.Text("Value",r,"",34,TextAlignmentOptions.MidlineRight);RpgUI.Fill(row.value.rectTransform,0,12,24,12);row.value.rectTransform.anchorMin=new Vector2(.66f,0);row.value.color=Theme.gold;return row;
        }
        [MenuItem("TuTien/UI Phase 7/Apply Complete Secret Realm Redesign")]
        public static void Apply()
        {
            EquipmentRedesignBuilder.Require();var canvas=GameObject.Find("Canvas").transform;
            foreach(string n in new[]{"SecretRealmModal","SecretRealmPlayRoot","SecretRealmResultModal"})
            {var root=canvas.Find(n);if(root==null||root.GetComponent<RpgViewScope>()!=null)throw new Exception("Initial Phase 7 requires the existing three realm roots. Update already-built views in place.");}
            foreach(string file in System.IO.Directory.GetFiles("Assets/UI/SecretRealm/Art","*.png"))
            {var imp=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.Compressed;imp.SaveAndReimport();}
            var art=Art();if(art.Any(a=>a==null))throw new Exception("All five real illustrations are required.");
            System.IO.Directory.CreateDirectory(Folder);var c=Card();PrefabUtility.SaveAsPrefabAsset(c.gameObject,Folder+"/RealmCard.prefab");UnityEngine.Object.DestroyImmediate(c.gameObject);var reward=Reward();PrefabUtility.SaveAsPrefabAsset(reward.gameObject,Folder+"/RealmRewardRow.prefab");UnityEngine.Object.DestroyImmediate(reward.gameObject);
            BuildHub(canvas.Find("SecretRealmModal"),art);BuildPlay(canvas.Find("SecretRealmPlayRoot"),art);BuildResult(canvas.Find("SecretRealmResultModal"),art);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void BuildHub(Transform root,Sprite[] art)
        {
            root.gameObject.AddComponent<RpgViewScope>();foreach(Transform old in root)old.gameObject.SetActive(false);
            var v=root.gameObject.AddComponent<RealmHubView>();v.owner=UnityEngine.Object.FindAnyObjectByType<SecretRealmModalUI>(FindObjectsInactive.Include);v.theme=Theme;
            v.surface=Surface(root,"Phase7Hub");var safe=v.surface;var header=RpgUI.Rect("Header",safe);RpgUI.Top(header,20,132,28,28);var title=Text(header,"Title","Bí Cảnh Thượng Cổ",0,132,48,Theme.ivory,0,190);title.font=Theme.headingFont;v.close=Close(header,"Đóng");
            v.summary=Text(safe,"Summary","",164,48,28,Theme.muted);v.wallet=Text(safe,"Wallet","",224,52,34,Theme.gold);v.access=Text(safe,"Access","",282,52,28,Theme.jade);
            var list=RpgUI.Rect("RealmList",safe);RpgUI.Fill(list,28,28,28,358);v.scroll=RpgUI.Scroll(list,out var content);content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=24;
            v.cards=new RealmCard[5];var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/RealmCard.prefab");
            for(int i=0;i<5;i++)
            {var card=((GameObject)PrefabUtility.InstantiatePrefab(prefab,content)).GetComponent<RealmCard>();card.name="Realm_"+(SecretRealmType)i;card.type=(SecretRealmType)i;card.artwork.sprite=art[i];RealmPlayView.FitArt(card.artwork);card.mode.text=RealmCopy.Modes[i];card.title.text=RealmCopy.Names[i];card.description.text=RealmCopy.Descriptions[i];card.reward.text=RealmCopy.Rewards[i];v.cards[i]=card;}
            root.gameObject.SetActive(false);
        }
        static void BuildPlay(Transform root,Sprite[] art)
        {
            root.gameObject.AddComponent<RpgViewScope>();root.Find("HeaderPanel").gameObject.SetActive(false);root.Find("FooterPanel").gameObject.SetActive(false);
            var v=root.gameObject.AddComponent<RealmPlayView>();v.theme=Theme;v.owner=UnityEngine.Object.FindAnyObjectByType<SecretRealmPlayController>(FindObjectsInactive.Include);v.realmArt=art;
            v.swordArt=new[]{"SwordGreenCutout","SwordPurpleCutout","SwordGoldCutout"}.Select(n=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SecretRealm/Art/"+n+".png")).ToArray();
            v.arena=(RectTransform)root.Find("ArenaContainer");
            v.legacyTimer=root.Find("HeaderPanel/TimerText").GetComponent<UnityEngine.UI.Text>();v.legacyScore=root.Find("HeaderPanel/ScoreValue").GetComponent<UnityEngine.UI.Text>();v.legacyInstructions=root.Find("HeaderPanel/RuleCard/TxtRule").GetComponent<UnityEngine.UI.Text>();v.legacyProgress=root.Find("FooterPanel/MainProgressBarBg/TxtProgress").GetComponent<UnityEngine.UI.Text>();v.legacyFill=root.Find("FooterPanel/MainProgressBarBg/Fill").GetComponent<UnityEngine.UI.Image>();
            var safe=Surface(root,"Phase7Play");v.surface=safe;var header=RpgUI.Rect("Header",safe);RpgUI.Top(header,20,132,28,28);v.title=Text(header,"Title","Bí Cảnh",0,92,42,Theme.ivory,0,190);v.title.font=Theme.headingFont;v.exit=Close(header,"Rời ải");v.mode=Text(header,"Mode","",90,40,26,Theme.gold,0,190);
            v.objective=Text(safe,"Objective","",168,112,30);v.hint=Text(safe,"Instructions","",284,56,27,Theme.muted);
            var metrics=RpgUI.Rect("Metrics",safe);RpgUI.Top(metrics,350,64,28,28);F.Surface(metrics,Theme.surface);v.timer=F.Text("Timer",metrics,"",36);RpgUI.Fill(v.timer.rectTransform,20,0,0,0);v.timer.rectTransform.anchorMax=new Vector2(.5f,1);v.score=F.Text("Score",metrics,"",32,TextAlignmentOptions.MidlineRight);RpgUI.Fill(v.score.rectTransform,0,0,20,0);v.score.rectTransform.anchorMin=new Vector2(.5f,0);
            v.progress=F.Progress(safe);RpgUI.Top((RectTransform)v.progress.transform,426,64,28,28);v.progress.label.fontSize=26;
            v.playArea=RpgUI.Rect("GameplayArea",safe);RpgUI.Fill(v.playArea,28,208,28,514);v.artwork=ArtBox(v.playArea,"RealmBackground",art[0]);RpgUI.Fill((RectTransform)v.artwork.transform.parent);var shade=RpgUI.Rect("ReadabilityVeil",v.playArea);RpgUI.Fill(shade);F.Surface(shade,new Color(.015f,.055f,.05f,.28f));
            v.notice=F.Text("StartFeedback",v.playArea,"",28,TextAlignmentOptions.Center);RpgUI.Top(v.notice.rectTransform,8,48,12,12);v.notice.color=Theme.gold;var group=v.notice.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            v.arenaViewport=RpgUI.Rect("LogicalArenaViewport",v.playArea);v.arena.SetParent(v.arenaViewport,false);v.arena.anchoredPosition=Vector2.zero;
            v.actionHost=RpgUI.Rect("PrimaryAction",safe);RpgUI.Bottom(v.actionHost,36,132,28,28);v.directAction=F.Text("DirectInteractionHint",safe,"",30,TextAlignmentOptions.Center);RpgUI.Bottom(v.directAction.rectTransform,40,124,28,28);v.directAction.color=Theme.jade;
            root.gameObject.SetActive(false);
        }
        static void BuildResult(Transform root,Sprite[] art)
        {
            root.gameObject.AddComponent<RpgViewScope>();foreach(Transform old in root)old.gameObject.SetActive(false);var v=root.gameObject.AddComponent<RealmResultView>();v.theme=Theme;v.realmArt=art;v.rowPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/RealmRewardRow.prefab").GetComponent<RealmRewardRow>();
            var safe=Surface(root,"Phase7Result");v.reveal=safe.gameObject.AddComponent<CanvasGroup>();v.artwork=ArtBox(safe,"ResultIllustration",art[0]);v.artwork.rectTransform.pivot=new Vector2(.5f,.67f);RpgUI.Top((RectTransform)v.artwork.transform.parent,0,300);
            v.status=Text(safe,"Outcome","",324,80,30,Theme.jade);v.title=Text(safe,"Title","",414,104,48);v.title.font=Theme.headingFont;v.score=Text(safe,"Score","",536,64,34,Theme.gold);v.receipt=Text(safe,"Receipt","",612,72,28,Theme.muted);
            var list=RpgUI.Rect("RewardScroll",safe);RpgUI.Fill(list,28,216,28,712);v.scroll=RpgUI.Scroll(list,out var content);v.rows=content;content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().spacing=12;
            var noteRow=RpgUI.Rect("SpecialNote",content);Stack(noteRow,20,0);v.note=Flow(noteRow,"Note","",30,Theme.jade);var order=noteRow.GetComponent<UnityEngine.UI.LayoutElement>()??noteRow.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();order.ignoreLayout=false;
            // Notes follow reward rows, maintained by Show; no fixed footer can cover the scrolling rewards.
            v.confirm=F.Action("Complete",safe,"Hoàn tất · Trở về bí cảnh",true);RpgUI.Bottom((RectTransform)v.confirm.transform,36,132,28,28);root.gameObject.SetActive(false);
        }
    }
}
#endif

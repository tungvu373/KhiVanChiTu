#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Equipment.Editor;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Progression.Editor
{
    public static class ProgressionRedesignBuilder
    {
        static FoundationFactory F=>EquipmentRedesignBuilder.Factory;
        static FoundationTheme Theme=>EquipmentRedesignBuilder.Theme;
        static TMP_Text FlowText(Transform parent,string name,string value,float size,Color color)
        {
            var t=F.Text(name,parent,value,size);RpgUI.Fill(t.rectTransform);t.color=color;
            var le=t.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minHeight=size+20;
            return t;
        }
        static void Stack(RectTransform r,int padding=0,float gap=12)
        {
            var g=r.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();g.padding=new RectOffset(padding,padding,padding,padding);g.spacing=gap;g.childControlHeight=g.childControlWidth=true;g.childForceExpandWidth=true;g.childForceExpandHeight=false;
        }
        static ProgressionCard BuildCard()
        {
            var root=RpgUI.Rect("ProgressionCard",null);root.sizeDelta=new Vector2(976,720);root.gameObject.AddComponent<RpgViewScope>();F.Surface(root,Theme.surface,true);Stack(root,24);
            var card=root.gameObject.AddComponent<ProgressionCard>();
            card.header=RpgUI.Rect("Identity",root);RpgUI.Height(card.header,128);
            var icon=RpgUI.Rect("Emblem",card.header);RpgUI.Top(icon,8,104,0,0);icon.anchorMax=new Vector2(0,1);icon.pivot=new Vector2(0,1);icon.anchoredPosition=new Vector2(0,-8);icon.sizeDelta=new Vector2(104,104);card.icon=icon.gameObject.AddComponent<NavigationIcon>();card.icon.color=Theme.gold;card.icon.raycastTarget=false;
            card.title=F.Text("Name",card.header,"",38);card.title.font=Theme.headingFont;RpgUI.Top(card.title.rectTransform,0,72,144,0);
            card.caption=F.Text("TierAndLevel",card.header,"",28);RpgUI.Top(card.caption.rectTransform,76,48,144,0);card.caption.color=Theme.muted;
            card.status=FlowText(root,"State","",28,Theme.gold);
            card.description=FlowText(root,"Description","",30,Theme.muted);
            card.effect=FlowText(root,"EffectComparison","",32,Theme.jade);
            card.progress=F.Progress(root);RpgUI.Height(card.progress.transform,76);card.progress.label.fontSize=28;
            card.requirements=FlowText(root,"RequirementsAndCost","",30,Theme.ivory);
            var rewards=RpgUI.Rect("RewardCard",root);F.Surface(rewards,Theme.raised);Stack(rewards,16,8);card.rewardSurface=rewards.gameObject;
            FlowText(rewards,"RewardCaption","PHẦN THƯỞNG",26,Theme.muted);card.reward=FlowText(rewards,"RewardValue","",30,Theme.ivory);
            card.action=F.Action("CardAction",root,"Tiếp tục",true);RpgUI.Height(card.action.transform,132);
            var edge=RpgUI.Rect("StateAccent",root);RpgUI.Fill(edge);edge.anchorMax=new Vector2(0,1);edge.sizeDelta=new Vector2(4,0);var ignore=edge.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();ignore.ignoreLayout=true;card.accent=F.Surface(edge,Theme.gold);
            return card;
        }
        static UnityEngine.UI.ScrollRect Chips(Transform parent,string name,string[] captions,out FoundationAction[] actions)
        {
            var r=RpgUI.Rect(name,parent);var s=r.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();s.horizontal=true;s.vertical=false;s.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var viewport=RpgUI.Rect("Viewport",r);RpgUI.Fill(viewport);F.Surface(viewport,Color.clear,true);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var content=RpgUI.Rect("Chips",viewport);content.anchorMin=Vector2.zero;content.anchorMax=new Vector2(0,1);content.pivot=new Vector2(0,.5f);content.sizeDelta=Vector2.zero;content.anchoredPosition=Vector2.zero;
            var g=content.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();g.spacing=12;g.childControlWidth=g.childControlHeight=true;g.childForceExpandWidth=false;g.childForceExpandHeight=true;
            content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().horizontalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            actions=new FoundationAction[captions.Length];for(int i=0;i<captions.Length;i++){actions[i]=F.Action("Filter"+i,content,captions[i]);var le=actions[i].gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minWidth=le.preferredWidth=i==0?248:232;}
            s.viewport=viewport;s.content=content;return s;
        }
        [MenuItem("TuTien/UI Phase 6/Apply Progression and Talent Redesign")]
        public static void Apply()
        {
            EquipmentRedesignBuilder.Require();var canvas=UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include).GetComponentInParent<Canvas>().rootCanvas.transform;
            foreach(string name in new[]{"DaoAnModal","MilestoneModalUI"})
            {var existing=canvas.Find(name);if(existing==null||existing.GetComponent<ProgressionScreen>()!=null)throw new InvalidOperationException("Open the mobile scene before initial Phase 6 application; an existing Phase 6 view must be updated in place.");}
            System.IO.Directory.CreateDirectory("Assets/UI/Progression/Prefabs");var sample=BuildCard();PrefabUtility.SaveAsPrefabAsset(sample.gameObject,"Assets/UI/Progression/Prefabs/ProgressionCard.prefab");UnityEngine.Object.DestroyImmediate(sample.gameObject);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Progression/Prefabs/ProgressionCard.prefab").GetComponent<ProgressionCard>();
            foreach(bool talents in new[]{false,true})
            {
                var overlay=canvas.Find(talents?"DaoAnModal":"MilestoneModalUI");if(overlay==null)throw new InvalidOperationException("Existing progression overlay not found.");
                if(overlay.GetComponent<ProgressionScreen>()!=null)throw new InvalidOperationException("Phase 6 already exists; update the existing view.");
                foreach(Transform old in overlay)old.gameObject.SetActive(false);
                var v=overlay.gameObject.AddComponent<ProgressionScreen>();v.talents=talents;v.theme=Theme;v.cardPrefab=prefab;v.milestoneOwner=UnityEngine.Object.FindAnyObjectByType<MilestoneModalUI>(FindObjectsInactive.Include);
                var safe=EquipmentRedesignBuilder.Scope("Phase6Presentation",overlay);RpgUI.Fill(safe);safe.gameObject.AddComponent<SafeAreaFitter>();safe.gameObject.AddComponent<ProgressionInputShield>();v.surface=safe;
                var header=RpgUI.Rect("Header",safe);RpgUI.Top(header,20,132,28,28);
                v.heading=F.Text("Title",header,talents?"Thiên Phú Đạo Vận":"Mốc Tu Tiên",48);RpgUI.Fill(v.heading.rectTransform,0,0,164,0);v.heading.font=Theme.headingFont;
                v.close=F.Action("Close",header,"Đóng");RpgUI.Top((RectTransform)v.close.transform,0,132);((RectTransform)v.close.transform).anchorMin=new Vector2(1,1);((RectTransform)v.close.transform).offsetMin=new Vector2(-144,-132);((RectTransform)v.close.transform).offsetMax=Vector2.zero;
                v.summary=F.Text("Summary",safe,"",30);RpgUI.Top(v.summary.rectTransform,164,76,32,32);v.summary.color=Theme.gold;
                v.totalProgress=F.Progress(safe);RpgUI.Top((RectTransform)v.totalProgress.transform,248,64,28,28);v.totalProgress.label.fontSize=28;
                var nav=EquipmentRedesignBuilder.Row(safe,"ProgressionNavigation",132);RpgUI.Top(nav,328,132,28,28);v.milestoneTab=F.Action("Milestones",nav,"Mốc Tu Tiên");v.talentTab=F.Action("Talents",nav,"Thiên Phú Đạo Vận");
                v.wallet=F.Text("Context",safe,"",28);RpgUI.Top(v.wallet.rectTransform,476,64,32,32);v.wallet.color=Theme.muted;
                v.stateScroll=Chips(safe,"StateFilters",new[]{"Tất cả","Sẵn sàng","Đang tiến","Chưa mở","Đã xong"},out var state);v.stateButtons=state;RpgUI.Top((RectTransform)v.stateScroll.transform,556,132,28,28);
                v.branchScroll=Chips(safe,"BranchFilters",v.BranchLabels,out var branches);v.branchButtons=branches;RpgUI.Top((RectTransform)v.branchScroll.transform,704,132,28,28);
                v.scroll=RpgUI.Scroll(safe,out var items);v.content=items;RpgUI.Fill((RectTransform)v.scroll.transform,28,talents?112:244,28,860);
                var empty=RpgUI.Rect("EmptyNotice",v.scroll.viewport);RpgUI.Fill(empty,32,40,32,40);v.empty=F.Text("Message",empty,"",32,TextAlignmentOptions.Center);RpgUI.Fill(v.empty.rectTransform);empty.gameObject.SetActive(true);
                v.feedback=F.Text("Feedback",safe,"",30);RpgUI.Bottom(v.feedback.rectTransform,talents?16:164,80,28,28);v.feedback.color=Theme.jade;v.feedback.overflowMode=TextOverflowModes.Ellipsis;
                var footer=EquipmentRedesignBuilder.Row(safe,"ScreenActions",132);RpgUI.Bottom(footer,20,132,28,28);v.primary=F.Action("ClaimAll",footer,"Nhận tất cả",true);v.secondary=F.Action("Achievements",footer,"Thành tựu");if(talents)footer.gameObject.SetActive(false);
                EditorUtility.SetDirty(v);
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("[Phase 6] Live milestone and talent screens and reusable card prefab saved.");
        }
    }
}
#endif

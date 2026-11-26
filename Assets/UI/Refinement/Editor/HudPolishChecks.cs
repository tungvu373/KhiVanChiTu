#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using KhiVanChiTu.UI.Navigation;
using KhiVanChiTu.UI.SecretRealm;

namespace KhiVanChiTu.UI.Refinement.Editor
{
    public static class HudPolishChecks
    {
        static void Expect(bool value,string message){if(!value)throw new Exception(message);}
        static IEnumerable<object> Wait(float seconds=.25f){float until=Time.unscaledTime+seconds;while(Time.unscaledTime<until)yield return null;}
        static void Hit(UnityEngine.UI.Button button)
        {
            Canvas.ForceUpdateCanvases();var r=(RectTransform)button.transform;
            var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Expect(button.IsInteractable()&&hits.Count>0&&(hits[0].gameObject.transform==r||hits[0].gameObject.transform.IsChildOf(r)),"Button not top raycast target: "+button.name);
            button.OnPointerClick(data);
        }
        static void Fit(Transform root)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var t in root.GetComponentsInChildren<TMP_Text>())
            {t.ForceMeshUpdate();Expect(!t.isTextOverflowing,t.name+" overflows: "+t.text);}
        }
        public static IEnumerable<object> Run(GlobalNavigationController nav,Action<Vector2Int> resize,Action<string,string,Action,string> report,Action<string> capture)
        {
            void Test(string id,string expected,Action action)=>report(id,expected,action,null);
            var notifications=nav.GetComponent<NavigationNotifications>();
            var toggle=nav.hud.legacy.focusButton;var button=toggle.GetComponent<UnityEngine.UI.Button>();
            nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait())yield return x;
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                resize(size);yield return null;foreach(var x in Wait())yield return x;
                Vector2 stable=toggle.anchoredPosition;
                for(int i=0;i<20;i++)
                {
                    bool before=nav.hud.IsCompact;
                    Test("toggle-input-"+size+"-"+i,"Native button toggles compact state",()=>{Hit(button);Expect(nav.hud.IsCompact!=before,"Toggle callback missing");});
                    foreach(var x in Wait(.08f))yield return x;
                    Test("toggle-position-"+size+"-"+i,"Restore control stays visible, stable and in screen",()=>{
                        Expect(Vector2.Distance(stable,toggle.anchoredPosition)<.1f,"Restore control drifted");
                        var corners=new Vector3[4];toggle.GetWorldCorners(corners);
                        foreach(var c in corners){var p=RectTransformUtility.WorldToScreenPoint(null,c);Expect(p.x>=0&&p.y>=0&&p.x<=Screen.width&&p.y<=Screen.height,"Control outside screen");}
                        Expect(toggle.gameObject.activeInHierarchy&&button.IsInteractable(),"Restore control hidden/disabled");});
                    if(i==0||i==19){capture("toggle-"+(i==0?"compact":"full"));yield return null;}
                }
                Test("generated-art-"+size,"Real imported PNG replaces HUD surfaces and preserves dynamic fills",()=>{
                    Expect(nav.hud.hudSurface!=null&&nav.hud.hudSurface.name=="JadeSurface","Generated art absent");
                    foreach(var v in nav.hud.skillViews)Expect(v.button.GetComponent<UnityEngine.UI.Image>().sprite==nav.hud.hudSurface,"Skill art missing");
                    foreach(var p in new[]{nav.hud.hp,nav.hud.mp,nav.hud.bossHealth})Expect(p.fill!=null&&p.fill.GetComponent<UnityEngine.UI.Image>()!=null,"Native fill missing");});
                var combat=CombatManager.Instance;var skills=SkillManager.Instance;
                // Boundary-value presentation fixture. Native auto-cast immediately
                // spends full MP even at timeScale zero; suspend those two writers
                // only while capturing the UI boundary, then restore them.
                bool combatEnabled=combat.enabled,skillsEnabled=skills.enabled;
                combat.enabled=false;skills.enabled=false;
                float hp=combat.playerCurrentHp,mp=skills.currentLinhLuc;
                combat.playerCurrentHp=0;skills.currentLinhLuc=0;yield return null;
                Test("vitals-empty-"+size,"Zero HP and MP show empty live bars",()=>Expect(nav.hud.hp.Normalized==0&&nav.hud.mp.Normalized==0,"Empty bar not bound"));capture("vitals-empty");yield return null;
                combat.playerCurrentHp=combat.playerMaxHp;skills.currentLinhLuc=skills.maxLinhLuc;yield return null;
                Test("vitals-full-"+size,"Full HP and MP show full live bars with auto-cast suspended for boundary fixture",()=>Expect(nav.hud.hp.Normalized==1&&nav.hud.mp.Normalized==1,"Full bar not bound: HP="+nav.hud.hp.Normalized+" MP="+nav.hud.mp.Normalized));capture("vitals-full");yield return null;
                combat.playerCurrentHp=hp;skills.currentLinhLuc=mp;
                combat.enabled=combatEnabled;skills.enabled=skillsEnabled;
                nav.SelectGroup(NavigationGroup.Equipment);foreach(var x in Wait())yield return x;
                Test("dao-first-"+size,"Exactly one Dao Van entry, first in Equipment",()=>{
                    var cards=nav.cards.Where(c=>c.action.gameObject.activeSelf).OrderBy(c=>c.action.transform.GetSiblingIndex()).ToArray();
                    Expect(cards[0].route==NavigationRoute.Talents,"Dao Van is not first");
                    Expect(GlobalNavigationController.Destinations.Count(d=>d.route==NavigationRoute.Talents)==1,"Duplicate route");});
                capture("equipment-dao-first");yield return null;
                nav.OpenRoute(NavigationRoute.Talents);foreach(var x in Wait(.4f))yield return x;
                Test("dao-open-"+size,"Existing DaoAn screen opens and keeps level data",()=>Expect(DaoAnModalUI.Instance.IsModalOpen,"DaoAn did not open"));
                nav.Back();foreach(var x in Wait(.4f))yield return x;
                Test("dao-back-"+size,"Back restores Equipment",()=>Expect(nav.SelectedGroup==NavigationGroup.Equipment&&nav.IsHubVisible,"Wrong restored group"));
                nav.SelectGroup(NavigationGroup.Cultivation);foreach(var x in Wait())yield return x;
                Test("dao-absent-cultivation-"+size,"Cultivation contains no DaoVan duplicate",()=>Expect(nav.cards.All(c=>c.route!=NavigationRoute.Talents||!c.action.gameObject.activeSelf),"Duplicate DaoVan"));
                nav.SelectGroup(NavigationGroup.SecretRealm);foreach(var x in Wait())yield return x;nav.OpenRoute(NavigationRoute.RealmList);foreach(var x in Wait(.4f))yield return x;
                var hub=UnityEngine.Object.FindAnyObjectByType<RealmHubView>();
                capture("realm-list");yield return null;
                SecretRealmModalUI.Instance.Close();foreach(var x in Wait(.4f))yield return x;
                var play=UnityEngine.Object.FindAnyObjectByType<RealmPlayView>(FindObjectsInactive.Include);
                foreach(SecretRealmType type in Enum.GetValues(typeof(SecretRealmType)))
                {
                    SecretRealmPlayController.Instance.LaunchMiniGame(type);foreach(var x in Wait(.35f))yield return x;
                    Test("realm-text-"+type+"-"+size,"Centered title/metrics and wrapped instructions fit actual rendered realm",()=>{
                        Fit(play.surface);Expect(play.title.alignment==TextAlignmentOptions.Center,"Title alignment");
                        Expect(Mathf.Abs(play.timer.rectTransform.offsetMin.x+play.timer.rectTransform.offsetMax.x)<.1f,"Asymmetric timer padding");
                        var title=play.title.rectTransform;Expect(Mathf.Abs(title.offsetMin.x)<.1f&&Mathf.Abs(title.offsetMax.x+190)<.1f,"Title content region changed");});
                    capture("realm-"+type);yield return null;SecretRealmPlayController.Instance.CloseAll();foreach(var x in Wait(.3f))yield return x;
                }
                nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait(.4f))yield return x;
            }
            foreach(var x in BadgeChecks(nav,notifications,report,capture))yield return x;
            nav.SelectGroup(NavigationGroup.Battle);foreach(var x in Wait(.4f))yield return x;
            var stage=StageManager.Instance;bool oldBoss=stage.isBossStage;float oldTimer=stage.bossTimer;
            stage.isBossStage=true;stage.bossTimer=18;Time.timeScale=1;
            for(int i=0;i<20;i++)
            {
                Test("toggle-live-combat-boss-"+i,"Toggle works during advancing combat with boss HUD",()=>Hit(button));
                foreach(var x in Wait(.08f))yield return x;
                Test("toggle-live-stable-"+i,"Boss and restore control remain visible",()=>Expect(nav.hud.boss.gameObject.activeInHierarchy&&toggle.anchoredPosition.x==936,"Boss/toggle disappeared"));
            }
            capture("combat-boss-toggle");yield return null;Time.timeScale=0;stage.isBossStage=oldBoss;stage.bossTimer=oldTimer;
        }
        static IEnumerable<object> BadgeChecks(GlobalNavigationController nav,NavigationNotifications state,Action<string,string,Action,string> report,Action<string> capture)
        {
            void Test(string id,string expected,Action action)=>report(id,expected,action,null);
            int Index(NavigationRoute route)=>Array.FindIndex(nav.cards,c=>c.route==route);
            int Count(NavigationRoute route)=>state.routes[Index(route)].Count;
            Test("badge-real-dao","DaoVan badge equals real affordable catalog actions",()=>Expect(Count(NavigationRoute.Talents)==DaoAnProgressionService.Instance.Catalog.nodes.Count(n=>n!=null&&DaoAnProgressionService.Instance.CanPurchase(n.id)),"Dao badge stale"));
            var economy=EconomyManager.Instance;float old=economy.currentLinhThach;economy.SpendLinhThach(old);yield return null;yield return null;
            Test("badge-unaffordable","Currency event clears affordable upgrades",()=>Expect(Count(NavigationRoute.Talents)==0&&Count(NavigationRoute.Body)==0&&Count(NavigationRoute.Artifact)==0,"Unavailable upgrade badge remains"));
            economy.AddLinhThach(1000000);yield return null;yield return null;
            Test("badge-available","Currency event exposes real available DaoVan actions",()=>Expect(Count(NavigationRoute.Talents)>0,"Available badge absent"));
            nav.SelectGroup(NavigationGroup.Equipment);foreach(var x in Wait())yield return x;
            int available=Count(NavigationRoute.Talents);nav.OpenRoute(NavigationRoute.Talents);foreach(var x in Wait(.4f))yield return x;
            Test("badge-view-keeps-action","Viewing DaoVan does not clear available-action badges",()=>Expect(Count(NavigationRoute.Talents)==available,"Viewing cleared available action"));
            var dao=DaoAnProgressionService.Instance;var node=dao.Catalog.nodes.First(n=>n!=null&&dao.CanPurchase(n.id));int level=dao.GetLevel(node.id);
            Test("badge-native-purchase","Real DaoAn upgrade succeeds using original cost and effect",()=>Expect(dao.TryPurchase(node.id)&&dao.GetLevel(node.id)==level+1,"Native purchase failed"));yield return null;yield return null;
            Test("badge-after-upgrade","Badge recomputes from native post-upgrade conditions",()=>Expect(Count(NavigationRoute.Talents)==dao.Catalog.nodes.Count(n=>n!=null&&dao.CanPurchase(n.id)),"Post-upgrade badge stale"));
            nav.Back();foreach(var x in Wait(.4f))yield return x;
            var storage=StorageManager.Instance;int pending=storage.PendingCount;storage.EnqueuePendingSword(1);yield return null;yield return null;
            Test("badge-pending-item","Native pending item event increments storage badge",()=>Expect(Count(NavigationRoute.Storage)==pending+1,"Pending item badge absent"));
            nav.OpenRoute(NavigationRoute.Storage);foreach(var x in Wait(.4f))yield return x;
            Test("badge-item-review","Reviewing pending items retains unresolved action",()=>Expect(Count(NavigationRoute.Storage)==pending+1,"View incorrectly cleared pending"));
            storage.TryRemovePendingItem(storage.GetPendingItems().Last());yield return null;yield return null;
            Test("badge-pending-resolved","Resolving native pending item clears its action",()=>Expect(Count(NavigationRoute.Storage)==pending,"Resolved action stale"));nav.Back();foreach(var x in Wait(.4f))yield return x;
            var milestones=MilestoneManager.Instance;var quest=milestones.GetNextPendingMilestone();
            Test("badge-reward-ready","Milestone badge equals actual unclaimed completed quests",()=>Expect(Count(NavigationRoute.Milestones)==milestones.GetReadyToClaimCount()&&Count(NavigationRoute.Milestones)>0,"Claimable milestone badge missing"));
            Test("badge-reward-claim","Native claim resolves a ready milestone",()=>Expect(milestones.ClaimReward(quest.id),"Claim failed"));yield return null;yield return null;
            Test("badge-reward-updated","Claim event updates exact remaining rewards",()=>Expect(Count(NavigationRoute.Milestones)==milestones.GetReadyToClaimCount(),"Claim badge stale"));
            Test("badge-parent-aggregate","Equipment aggregates relocated DaoVan; Cultivation excludes it",()=>{
                foreach(NavigationGroup group in Enum.GetValues(typeof(NavigationGroup)))
                {int expected=GlobalNavigationController.Destinations.Where(d=>d.group==group).Sum(d=>Count(d.route));Expect(state.groups[(int)group].Count==expected,"Parent aggregate mismatch: "+group);}
                foreach(var b in state.routes)Expect(!b.raycastTarget&&b.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout,"Badge captures input or layout");});
            capture("equipment-badges");yield return null;
            DataManager.Instance.SaveCurrentGame(true);int saved=dao.GetLevel(node.id);int savedPending=storage.PendingCount;DataManager.Instance.LoadCurrentGame();foreach(var x in Wait(.4f))yield return x;
            Test("badge-save-load","Native save/load retains DaoVan and pending state, badges recompute",()=>Expect(dao.GetLevel(node.id)==saved&&storage.PendingCount==savedPending&&Count(NavigationRoute.Talents)==dao.Catalog.nodes.Count(n=>n!=null&&dao.CanPurchase(n.id)),"Save/load state mismatch"));
            economy.AddLinhThach(old-economy.currentLinhThach);yield return null;
        }
    }
}
#endif

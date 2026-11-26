using System;
using UnityEngine;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Refinement
{
    [DefaultExecutionOrder(15000)]
    public sealed class NavigationNotifications : MonoBehaviour
    {
        public GlobalNavigationController navigation;
        public NotificationBadge[] groups, routes;
        bool dirty = true, subscribed, sessionActive;
        EconomyManager economy;
        StorageManager storage;
        DaoAnProgressionService dao;
        MilestoneManager milestones;
        PlayerStatsService body;
        ArtifactService artifacts;
        FeatureUnlockService unlocks;
        CultivationManager cultivation;
        void Start() { Subscribe(); RefreshBadgeState(); }
        void Subscribe()
        {
            if(subscribed) return;
            economy=EconomyManager.Instance;storage=StorageManager.Instance;
            dao=DaoAnProgressionService.Instance;milestones=MilestoneManager.Instance;
            body=PlayerStatsService.Instance;artifacts=ArtifactService.Instance;
            unlocks=FeatureUnlockService.Instance;cultivation=CultivationManager.Instance;
            if(economy!=null)economy.OnLinhThachChanged+=Currency;
            if(storage!=null)storage.OnStorageChanged+=Invalidate;
            if(dao!=null)dao.OnChanged+=Invalidate;
            if(milestones!=null)milestones.OnMilestonesUpdated+=Invalidate;
            if(body!=null)body.OnStatsChanged+=Invalidate;
            if(artifacts!=null)artifacts.OnChanged+=Invalidate;
            if(unlocks!=null)unlocks.OnUnlockStateChanged+=Invalidate;
            if(cultivation!=null)cultivation.OnStageChanged+=Stage;
            subscribed=true;
        }
        void OnEnable(){dirty=true;if(subscribed)Subscribe();}
        void OnDisable()
        {
            if(economy!=null)economy.OnLinhThachChanged-=Currency;
            if(storage!=null)storage.OnStorageChanged-=Invalidate;
            if(dao!=null)dao.OnChanged-=Invalidate;
            if(milestones!=null)milestones.OnMilestonesUpdated-=Invalidate;
            if(body!=null)body.OnStatsChanged-=Invalidate;
            if(artifacts!=null)artifacts.OnChanged-=Invalidate;
            if(unlocks!=null)unlocks.OnUnlockStateChanged-=Invalidate;
            if(cultivation!=null)cultivation.OnStageChanged-=Stage;
            subscribed=false;
        }
        void Currency(float _) => Invalidate();
        void Stage(CultivationStageData _) => Invalidate();
        void Invalidate() => dirty=true;
        // A dirty flag coalesces a batch of economy/service events into one read.
        // No catalog or inventory polling occurs on frames without an event.
        void LateUpdate()
        {
            bool active=DataManager.Instance!=null&&DataManager.Instance.isSessionActive;
            if(active!=sessionActive){sessionActive=active;dirty=true;}
            if(dirty){if(!subscribed)Subscribe();RefreshBadgeState();}
        }
        public int CountFor(NavigationRoute route)
        {
            if(DataManager.Instance==null||!DataManager.Instance.isSessionActive)return 0;
            switch(route)
            {
                case NavigationRoute.Storage:return storage!=null?storage.PendingCount:0;
                case NavigationRoute.Milestones:return milestones!=null?milestones.GetReadyToClaimCount():0;
                case NavigationRoute.Talents:
                    int count=0;
                    if(dao!=null&&dao.Catalog!=null&&dao.Catalog.nodes!=null)
                        foreach(var node in dao.Catalog.nodes)if(node!=null&&dao.CanPurchase(node.id))count++;
                    return count;
                case NavigationRoute.Body:
                    if(unlocks!=null&&!unlocks.IsFeatureUnlocked(FeatureId.BodyCultivation))return 0;
                    int available=0;
                    if(body!=null)for(int i=0;i<PlayerStatsService.TalentNodes.Length;i++)if(body.CanUpgrade(i))available++;
                    return available;
                case NavigationRoute.Artifact:
                    int upgrades=0;
                    if(artifacts!=null&&economy!=null)for(int i=0;i<4;i++)
                    {
                        var slot=(ArtifactSlot)i;
                        if((unlocks==null||unlocks.IsArtifactSlotUnlocked(slot))&&artifacts.GetLevel(slot)<ArtifactService.MaxLevel&&artifacts.Ore>=artifacts.GetOreCost(slot)&&economy.currentLinhThach>=artifacts.GetStoneCost(slot))upgrades++;
                    }
                    return upgrades;
                default:return 0;
            }
        }
        public void RefreshBadgeState()
        {
            if(navigation==null)return;
            var counts=new int[groups.Length];
            for(int i=0;i<navigation.cards.Length;i++)
            {
                var route=navigation.cards[i].route;int count=CountFor(route);
                if(i<routes.Length&&routes[i]!=null)routes[i].SetCount(count);
                foreach(var destination in GlobalNavigationController.Destinations)
                    if(destination.route==route){counts[(int)destination.group]+=count;break;}
            }
            for(int i=0;i<groups.Length;i++)if(groups[i]!=null)groups[i].SetCount(counts[i]);
            dirty=false;
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static partial class MobileUIPhase09Checks
{
    sealed class AdFixture:IRewardedAdProvider
    {
        public bool IsReady=>true;
        public Action<bool> callback;
        public void ShowOfflineReward(Action<bool> onRewarded){callback=onRewarded;}
    }
    static IEnumerable AdditionalSuite()
    {
        Highest();CloseDrawer();MobileDrawerUIController.Instance.OpenDrawer(0);yield return 0.4;
        var merge=MergeManager.Instance;
        Case("sword-pagination-sort","SWORD","Managers/MergeManager.cs","NextPage,PrevPage,SortInventory",()=>{
            var root=GameObject.Find("Canvas").transform;int p=(int)Field(merge,"currentPage");Click(Named(root,"NextBtn"));Expect((int)Field(merge,"currentPage")==Mathf.Min(p+1,merge.inventoryPages.Length-1),"Next route wrong");Click(Named(root,"PrevBtn"));Expect((int)Field(merge,"currentPage")==p,"Prev route wrong");Click(Named(root,"SortBtn"));Expect(merge.pageText.text.Length>0,"Page label missing");
        });
        Case("sword-drop-merge-and-invalid","SWORD","Combat/MergeSlot.cs","OnDrop",()=>{
            var slot=new CharacterSaveData{swords=new[]{new SwordSaveEntry{level=1},new SwordSaveEntry{level=1},new SwordSaveEntry{level=3}}};merge.LoadFromSlot(slot);
            var items=merge.inventoryPages.SelectMany(p=>p.GetComponentsInChildren<MergeItem>(true)).ToArray();var a=items.First(i=>i.level==1);var b=items.Last(i=>i.level==1);var target=b.GetComponentInParent<MergeSlot>();a.originalParent=a.transform.parent;
            int before=b.currentPieces;ExecuteEvents.Execute(target.gameObject,new PointerEventData(EventSystem.current){pointerDrag=a.gameObject},ExecuteEvents.dropHandler);Expect(b.currentPieces>before||b.level>1,"Drop did not merge");
            var c=items.First(i=>i.level==3);c.originalParent=c.transform.parent;var origin=c.originalParent;ExecuteEvents.Execute(target.gameObject,new PointerEventData(EventSystem.current){pointerDrag=c.gameObject},ExecuteEvents.dropHandler);Expect(c.transform.parent==origin&&c.level==3,"Invalid merge changed item");
        });
        yield return 0.2;
        Case("sword-quickmerge","SWORD","Managers/MergeManager.cs","QuickMerge",()=>{
            var saved=new CharacterSaveData{swords=Enumerable.Range(0,8).Select(i=>new SwordSaveEntry{level=1}).ToArray()};merge.LoadFromSlot(saved);
            var root=GameObject.Find("Canvas").transform;Click(Named(root,"btn_QuickMerge"));var items=merge.inventoryPages.SelectMany(p=>p.GetComponentsInChildren<MergeItem>(true)).ToArray();Expect(items.Any(i=>i.level>1),"Quick merge no progression");
        });
        yield return 0.2;CloseDrawer();
        StorageModalUI.Open();yield return 0.3;
        var storage=StorageModalUI.Instance;
        Case("storage-categories-views-filters","STORAGE","UI/StorageModalUI.cs","SelectCategory,ToggleViewMode,WireEvents",()=>{
            for(int i=0;i<5;i++){Click(storage.tabButtons[i]);Expect((int)storage.selectedCategory==i,"Wrong category input");}
            storage.toggleSwordHighLevel.isOn=!storage.toggleSwordHighLevel.isOn;Expect(StorageManager.Instance.FilterSwordsHighLevel==storage.toggleSwordHighLevel.isOn,"Sword filter disconnected");
            storage.togglePillBreakthrough.isOn=!storage.togglePillBreakthrough.isOn;Expect(StorageManager.Instance.FilterPillsBreakthrough==storage.togglePillBreakthrough.isOn,"Pill filter disconnected");
            storage.toggleAutoDismantleLv1.isOn=!storage.toggleAutoDismantleLv1.isOn;Expect(StorageManager.Instance.AutoDismantleLv1SwordsWhenFull==storage.toggleAutoDismantleLv1.isOn,"Auto filter disconnected");
            var mode=storage.currentViewMode;Click(storage.btnToggleView);Expect(mode!=storage.currentViewMode,"View not toggled");Click(storage.btnToggleView);Expect(mode==storage.currentViewMode,"View toggle not reversible");
            Click(storage.tabButtons[0]);Expect(storage.txtLinhThach.text.Length>0&&storage.txtOre.text.Length>0,"Wallet fields missing");Click(storage.btnClose);
        });
        yield return 0.3;
        DaoAnModalUI.Instance.Open();yield return 0.3;
        Case("daoan-purchase-cost-disabled","DAOAN","UI/DaoAnModalUI.cs","Refresh,UpdateBuyButtons",()=>{
            Rich();Highest();var svc=DaoAnProgressionService.Instance;var def=svc.Catalog.nodes.First(n=>svc.CanPurchase(n.id));DaoAnModalUI.Instance.Refresh();
            var root=((GameObject)Field(DaoAnModalUI.Instance,"modalOverlay")).transform;var card=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Card_"+def.id);var b=Named(card,"BtnBuy");int lv=svc.GetLevel(def.id);float money=EconomyManager.Instance.currentLinhThach,cost=def.costs[lv];Click(b);Expect(svc.GetLevel(def.id)==lv+1,"Wrong DaoAn node/duplicate callback");Near(EconomyManager.Instance.currentLinhThach,money-cost,"DaoAn cost");
            EconomyManager.Instance.currentLinhThach=0;DaoAnModalUI.Instance.Refresh();Expect(!svc.CanPurchase(def.id),"No-funds accepted");Expect(root.GetComponentsInChildren<Button>(true).Where(x=>x.name=="BtnBuy").All(x=>!x.interactable),"No-funds buy buttons enabled");DaoAnModalUI.Instance.Close();
        });
        yield return 0.3;
        AchievementModalUI.Open();yield return 0.3;
        Case("achievement-tabs-claim-once","ACHIEVEMENTS","UI/AchievementModalUI.cs","SelectCategory,OnClaimAllClicked",()=>{
            var ui=AchievementModalUI.Instance;var svc=AchievementManager.Instance;for(int i=0;i<ui.tabButtons.Length;i++){Click(ui.tabButtons[i]);Expect((int)ui.selectedCategory==i,"Achievement category input wrong");}
            Click(ui.tabButtons[0]);int ready=svc.GetReadyToClaimCount();Expect(ready>0,"Fixture has no completed achievements");int claimed=svc.GetTotalClaimedCount();Click(ui.btnClaimAll);Expect(svc.GetTotalClaimedCount()==claimed+ready,"Claim-all not delivered");float stone=EconomyManager.Instance.currentLinhThach;ui.btnClaimAll.onClick.Invoke();Near(EconomyManager.Instance.currentLinhThach,stone,"Duplicate achievement reward");Click(ui.btnClose);
        });
        yield return 0.3;
        Find<MilestoneModalUI>().Open();yield return 0.3;
        Case("milestone-claim-once","MILESTONES","UI/MilestoneModalUI.cs","OnClaimAllClicked",()=>{
            var ui=MilestoneModalUI.Instance;var svc=MilestoneManager.Instance;Expect(svc.GetReadyToClaimCount()>0,"No completed milestone fixture");Click(ui.btnClaimAll);Expect(svc.GetReadyToClaimCount()==0,"Milestones not claimed");float money=EconomyManager.Instance.currentLinhThach;ui.btnClaimAll.onClick.Invoke();Near(EconomyManager.Instance.currentLinhThach,money,"Duplicate milestone reward");Click(ui.btnClose);
        });
        yield return 0.3;
        var offline=OfflineEarningsManager.Instance;OfflineRewardPopupUI popup=null;
        var reward=new OfflineEarningsManager.OfflineRewardData{offlineSeconds=600,linhThach=1200,tuVi=0,linhChi=2,ore=3,hasReward=true};
        Action prepare=()=>{RequireIsolation();typeof(OfflineEarningsManager).GetProperty("PendingReward",All).SetValue(offline,reward);EconomyManager.Instance.currentLinhThach=1000;ArtifactService.Instance.State.ore=100;OfflineRewardPopupUI.Show(reward);popup=OfflineRewardPopupUI.Instance;};
        Case("offline-normal-no-duplicate","OFFLINE","UI/OfflineRewardPopupUI.cs","OnClaimNormalClicked",()=>{
            prepare();int ore=ArtifactService.Instance.Ore;Click(popup.claimNormalButton);Near(EconomyManager.Instance.currentLinhThach,2200,"Normal reward");Expect(ArtifactService.Instance.Ore==ore+3,"Offline ore missing");Expect(!offline.HasPendingReward,"Pending not consumed");popup.OnClaimNormalClicked();Near(EconomyManager.Instance.currentLinhThach,2200,"Repeated claim granted twice");
        });
        yield return 0.3;
        Case("offline-ad-pending-failure-success-duplicate","OFFLINE","UI/OfflineRewardPopupUI.cs","OnClaimDoubleClicked,OnClaimNormalClicked",()=>{
            var previous=RewardedAdBridge.Provider;try{
                var ad=new AdFixture();RewardedAdBridge.Provider=ad;prepare();Click(popup.claimDoubleButton);Expect(!popup.claimNormalButton.interactable&&!popup.closeButton.interactable,"Pending inputs enabled");Near(EconomyManager.Instance.currentLinhThach,1000,"Ad request prematurely rewarded");ad.callback(false);Expect(offline.HasPendingReward&&popup.claimNormalButton.interactable,"Ad failure lost reward");
                Click(popup.claimDoubleButton);ad.callback(true);Near(EconomyManager.Instance.currentLinhThach,3400,"Double reward");ad.callback(true);Near(EconomyManager.Instance.currentLinhThach,3400,"Duplicate SDK callback awarded twice");Expect(!offline.HasPendingReward,"Double pending not consumed");
            }finally{RewardedAdBridge.Provider=previous;}
        });
        yield return 0.3;
        Case("style-open","STYLE","Managers/GameplayChoicesService.cs","OpenStyles",()=>{
            var svc=GameplayChoicesService.Instance;var slot=DataManager.Instance.gameSaveData.slots[0];slot.swordStyleUnlocked=true;svc.OpenStyles();Expect(svc.IsChoosing,"Style menu absent");
        });
        yield return 0.3;
        Case("style-select","STYLE","Managers/GameplayChoicesService.cs","SelectStyle",()=>{var svc=GameplayChoicesService.Instance;var buttons=((GameObject)Field(svc,"panel")).GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Choice")).ToArray();Expect(buttons.Length>=2,"Style choices absent");Click(buttons[1]);Expect(svc.Style==1&&!svc.IsChoosing,"Style choice input wrong");});
        yield return 0.3;
        Case("encounter-offer","ENCOUNTER","Managers/GameplayChoicesService.cs","OfferEncounter",()=>Expect(GameplayChoicesService.Instance.OfferEncounter(),"Encounter route unavailable"));
        yield return 0.3;
        Case("encounter-challenge-input-progress","ENCOUNTER","Managers/GameplayChoicesService.cs","ResolveEncounter,OnEnemyDefeated",()=>{var svc=GameplayChoicesService.Instance;var buttons=((GameObject)Field(svc,"panel")).GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Choice")).ToArray();Click(buttons[1]);Expect(svc.ChallengeRemaining==8,"Challenge input wrong");for(int k=0;k<8;k++)svc.OnEnemyDefeated(false);Expect(svc.ChallengeRemaining==0,"Challenge progress did not resolve");});
        yield return 0.3;
        Case("pause-save-confirm-cancel-back","PAUSE","UI/PauseMenuManager.cs","OnManualSaveClicked,OnReturnMainMenuClicked,OnQuitGameClicked,CloseTopLayer",()=>{
            var ui=PauseMenuManager.Instance;ui.OpenPauseMenu();Click(ui.manualSaveButton);Expect(ui.txtSaveStatus.text.Contains("thành công"),"Save feedback missing");Click(ui.returnMainMenuButton);Expect(ui.btnPopupCancel.gameObject.activeInHierarchy,"Return confirmation missing");Click(ui.btnPopupCancel);Expect(ui.IsModalOpen,"Cancel closed parent settings");Click(ui.quitButton);Click(ui.btnPopupCancel);Expect(MobileUINavigationManager.Instance.HandleBackNavigation(),"Settings Back unhandled");
        });
        yield return 0.3;
    }
}
#endif

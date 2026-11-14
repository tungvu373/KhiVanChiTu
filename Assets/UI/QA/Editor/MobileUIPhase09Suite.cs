#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static partial class MobileUIPhase09Checks
{
    static IEnumerator Suite()
    {
        yield return 1.0;
        RequireIsolation();int firstFrame=Time.frameCount;
        yield return 0.3;
        Case("frames-and-profile","SAVE_LIFECYCLE","Data/DataManager.cs","Awake",()=>{Expect(Time.frameCount>firstFrame,"Game loop frozen");Expect(DataManager.Instance.SaveProfileId==Profile,"Wrong profile");Note("framesAdvanced",Time.frameCount-firstFrame);});
        if(!(bool)rows[0]["passed"])yield break;
        var menu=MainMenuManager.Instance;var data=DataManager.Instance;
        data.gameSaveData=new GameSaveData();data.SaveData();
        Case("main-settings-audio","MAIN_SETTINGS","UI/MainMenuManager.cs","InitializeMenu",()=>{
            Click(menu.settingsButton);Expect(menu.settingsPanel.activeInHierarchy,"Settings did not open");
            menu.bgmSlider.value=0.31f;menu.sfxSlider.value=0.47f;
            Near(AudioManager.Instance.bgmVolume,0.31f,"BGM input");Near(AudioManager.Instance.sfxVolume,0.47f,"SFX input");
            Near(PlayerPrefs.GetFloat(AudioManager.PREF_BGM_VOLUME),0.31f,"BGM preference");Click(menu.closeSettingsButton);Expect(menu.mainPanel.activeInHierarchy,"Settings Back");
        });
        Case("roster-slot-delete-confirm-cancel","SELECT","UI/MainMenuManager.cs","OpenCharacterSelection,AskConfirmDeleteSlot",()=>{
            Click(menu.playButton);Expect(menu.characterSelectPanel.activeInHierarchy,"Roster route");
            data.gameSaveData.slots[1]=new CharacterSaveData{isSlotEmpty=false,characterName="P09 disposable delete"};menu.OpenCharacterSelection();
            Click(menu.saveSlots[1].deleteButton);Expect(menu.confirmationPopup.activeInHierarchy,"Delete needs confirmation");Click(menu.popupCancelBtn);Expect(!data.gameSaveData.slots[1].isSlotEmpty,"Cancel deleted fixture");
            Click(menu.saveSlots[1].deleteButton);Click(menu.popupConfirmBtn);Expect(data.gameSaveData.slots[1].isSlotEmpty,"Confirm did not delete selected fixture");
        });
        Case("create-name-validation-and-save","CREATE","UI/MainMenuManager.cs","OpenCharacterCreation,OnConfirmCreateCharacter,ConfirmEnterGame",()=>{
            Click(menu.saveSlots[0].slotButton);Expect(menu.characterCreatePanel.activeInHierarchy,"Empty-slot route");menu.nameInput.text="   ";Click(menu.confirmCreateButton);Expect(menu.characterCreatePanel.activeInHierarchy,"Whitespace accepted");
            menu.nameInput.text="P09 Kiểm Chứng Đạo Hữu";Click(menu.confirmCreateButton);Expect(data.gameSaveData.slots[0].characterName=="P09 Kiểm Chứng Đạo Hữu","Wrong name input");Expect(!menu.gameObject.activeSelf,"Menu remained open");
        });
        yield return 0.8;
        RequireIsolation();Time.timeScale=0;Rich();Highest();
        Case("session-loaded","SAVE_LIFECYCLE","Data/DataManager.cs","LoadCurrentGame",()=>{Expect(data.isSessionActive,"Session not activated");Expect(data.gameSaveData.lastPlayedSlotIndex==0,"Wrong save slot");});
        for(int i=0;i<5;i++)
        {
            int index=i;Case("dock-open-"+i,"DOCK","UI/MobileDrawerUIController.cs","OpenDrawer,ToggleDrawer",()=>Click(MobileDrawerUIController.Instance.dockButtons[index]));
            yield return 0.35;
            Case("dock-back-"+i,"DOCK","UI/MobileUINavigationManager.cs","HandleBackNavigation",()=>{Expect(MobileDrawerUIController.Instance.ActiveDrawerIndex==index,"Wrong drawer input");Expect(MobileUINavigationManager.Instance.HandleBackNavigation(),"Back unhandled");});
            yield return 0.35;
            Case("dock-closed-"+i,"DOCK","UI/MobileDrawerUIController.cs","CloseActiveDrawer",()=>Expect(MobileDrawerUIController.Instance.ActiveDrawerIndex==-1,"Drawer stayed open"));
        }
        // Validate UI commands against real services. Fixture writes never leave the isolated profile.
        MobileDrawerUIController.Instance.OpenDrawer(3);yield return 0.4;
        var artifact=Find<ArtifactPanelUI>();var art=ArtifactService.Instance;
        Case("artifact-gates-and-live-stage","ARTIFACT","UI/ArtifactPanelUI.cs","RefreshUI,HandleStageChanged",()=>{
            Rich();Stage(0);artifact.RefreshUI();Expect(artifact.upgradeButtons.All(b=>!b.interactable),"Realm-locked artifact enabled");
            Highest();Expect(artifact.upgradeButtons.All(b=>b.interactable),"Stage event failed to refresh gates");
            Note("requirements",artifact.artifactCostTexts.Select(t=>t.text).ToArray());
        });
        for(int i=0;i<4;i++)
        {
            int index=i;Case("artifact-upgrade-slot-"+i,"ARTIFACT","UI/ArtifactPanelUI.cs","OnUpgradeClicked",()=>{
                Rich();int level=art.GetLevel((ArtifactSlot)index),ore=art.Ore,oreCost=art.GetOreCost((ArtifactSlot)index);float stone=EconomyManager.Instance.currentLinhThach,cost=art.GetStoneCost((ArtifactSlot)index);
                artifact.RefreshUI();Click(artifact.upgradeButtons[index]);Expect(art.GetLevel((ArtifactSlot)index)==level+1,"Wrong slot or duplicate callback");Expect(art.Ore==ore-oreCost,"Wrong ore cost");Near(EconomyManager.Instance.currentLinhThach,stone-cost,"Stone cost");Expect(artifact.artifactLabels[index].text.Contains("Lv."+(level+1)),"Level text stale");Note("slot",index);
            });
        }
        Case("artifact-no-funds-max-and-unsubscribe","ARTIFACT","UI/ArtifactPanelUI.cs","OnEnable,OnDisable,RefreshUI",()=>{
            EconomyManager.Instance.currentLinhThach=0;artifact.RefreshUI();Expect(artifact.upgradeButtons.All(b=>!b.interactable),"No-funds enabled");Rich();art.State.levels[0]=ArtifactService.MaxLevel;artifact.RefreshUI();Expect(!artifact.upgradeButtons[0].interactable,"Max enabled");
            var field=typeof(EconomyManager).GetField("OnLinhThachChanged",All);int count=((Delegate)field.GetValue(EconomyManager.Instance))?.GetInvocationList().Count(d=>ReferenceEquals(d.Target,artifact))??0;
            artifact.enabled=false;int off=((Delegate)field.GetValue(EconomyManager.Instance))?.GetInvocationList().Count(d=>ReferenceEquals(d.Target,artifact))??0;artifact.enabled=true;
            int restored=((Delegate)field.GetValue(EconomyManager.Instance))?.GetInvocationList().Count(d=>ReferenceEquals(d.Target,artifact))??0;
            Expect(count==1&&off==0&&restored==1,"Leaked economy subscription");Note("subscriberCounts",new[]{count,off,restored});
        });
        CloseDrawer();MobileDrawerUIController.Instance.OpenDrawer(4);yield return 0.4;
        var formation=Find<FormationPanelUI>();var form=FormationService.Instance;
        Case("formation-gates-live-stage","FORMATION","UI/FormationPanelUI.cs","RefreshUI,HandleStageChanged",()=>{Rich();Stage(0);formation.RefreshUI();Expect(formation.upgradeButtons.All(b=>!b.interactable),"Locked formation upgrade enabled");Expect(formation.selectButtons.All(b=>!b.interactable),"Locked select enabled");Highest();Expect(formation.upgradeButtons.All(b=>b.interactable),"Stage event stale");});
        for(int i=0;i<6;i++)
        {
            int index=i;Case("formation-upgrade-select-"+i,"FORMATION","UI/FormationPanelUI.cs","OnUpgradeFormation,OnSelectFormation",()=>{
                Rich();int lv=form.GetLevel((FormationId)index),ore=art.Ore,cost=form.GetOreCost((FormationId)index);formation.RefreshUI();Click(formation.upgradeButtons[index]);Expect(form.GetLevel((FormationId)index)==lv+1,"Wrong formation index/duplicate callback");Expect(art.Ore==ore-cost,"Wrong formation cost");
                if(form.Selected!=(FormationId)index)Click(formation.selectButtons[index]);Expect(form.Selected==(FormationId)index,"Select input wrong");Expect(!formation.selectButtons[index].interactable,"Selected item not disabled");
            });
        }
        Case("formation-spirit-cooldown","FORMATION","UI/FormationPanelUI.cs","OnActivateSpirit,UpdateSpiritCooldown",()=>{
            form.TrySelect(FormationId.Spirit);formation.RefreshUI();Click(formation.activateSpiritBtn);Expect(form.SpiritActive,"Spirit did not activate");Expect(form.SpiritCooldown>TimeSpan.Zero,"Cooldown absent");Expect(!formation.activateSpiritBtn.interactable,"Cooldown enabled");
        });
        CloseDrawer();MobileDrawerUIController.Instance.OpenDrawer(2);yield return 0.4;
        var alchemy=Find<AlchemyPanelUI>();var alc=AlchemyService.Instance;
        Case("alchemy-gate-and-resources","ALCHEMY","UI/AlchemyPanelUI.cs","RefreshUI,HandleStageChanged",()=>{
            for(int h=0;h<3;h++)alc.AddHerb(h,1000);Stage(0);alchemy.RefreshUI();Expect(!alchemy.btnBrewTuVi.interactable&&!alchemy.btnBrewBreakthrough.interactable,"Locked recipes enabled");
            Stage(8);Expect(alchemy.btnBrewTuVi.interactable&&alchemy.btnBrewBreakthrough.interactable,"Alchemy unlock event stale");Expect(alchemy.txtHerbLinhChi.text.Contains(alc.GetHerbCount(0).ToString()),"Herb value stale");
        });
        for(int i=0;i<2;i++)
        {
            int recipe=i;Case("alchemy-brew-dust-"+i,"ALCHEMY","UI/AlchemyPanelUI.cs","OnBrewCultivationClicked,OnBrewBreakthroughClicked,OnUseDustClicked",()=>{
                int[] cost=alc.GetRecipeCosts((PillRecipe)recipe,0),before=Enumerable.Range(0,3).Select(alc.GetHerbCount).ToArray();alc.State.craftsWithoutSupreme=29;alc.State.medicineDust=100;
                alchemy.RefreshUI();Click(recipe==0?alchemy.btnBrewTuVi:alchemy.btnBrewBreakthrough);Expect(alc.IsBrewing&&alc.State.brewingRecipe==recipe,"Wrong recipe input");for(int h=0;h<3;h++)Expect(alc.GetHerbCount(h)==before[h]-cost[h],"Wrong herb debit");
                Expect(!alchemy.btnBrewTuVi.interactable&&!alchemy.btnBrewBreakthrough.interactable,"Busy controls enabled");int pills=alc.GetPillCount(0,(PillRecipe)recipe,PillQuality.Supreme),dust=alc.State.medicineDust;Click(alchemy.btnFinishWithDust);Expect(!alc.IsBrewing,"Dust did not finish");Expect(alc.State.medicineDust==dust-alc.DustFinishCost,"Dust cost wrong");Expect(alc.GetPillCount(0,(PillRecipe)recipe,PillQuality.Supreme)==pills+1,"Craft output not bound");
            });
        }
        Case("alchemy-drink-and-empty","ALCHEMY","UI/AlchemyPanelUI.cs","OnUseTuViPillClicked",()=>{float before=CultivationManager.Instance.currentTuVi;int count=alc.GetPillCount(0,PillRecipe.Cultivation,PillQuality.Supreme);Click(alchemy.btnUseTuViPill);Expect(alc.GetPillCount(0,PillRecipe.Cultivation,PillQuality.Supreme)==count-1,"Pill not consumed exactly once");Expect(CultivationManager.Instance.currentTuVi>before,"TuVi unchanged");alchemy.RefreshUI();Expect(!alchemy.btnUseTuViPill.interactable,"Empty pill enabled");});
        CloseDrawer();Highest();MobileDrawerUIController.Instance.OpenDrawer(1);yield return 0.4;
        var body=Find<BodyTemperingPanelUI>();var stats=PlayerStatsService.Instance;
        Case("body-parent-and-grandfather-gates","BODY","UI/BodyTemperingPanelUI.cs","Refresh,OpenCenterModal",()=>{
            stats.LoadFromSlot(new CharacterSaveData{isSlotEmpty=false});int child=Array.FindIndex(PlayerStatsService.TalentNodes,n=>n.parentIndex>=0);Expect(child>=0,"No child node");Expect(!stats.IsNodeUnlocked(child),"Parent gate absent");body.OpenCenterModal(child);Expect(!body.btnModalUpgrade.interactable,"Locked modal upgrade enabled");
            var saved=new CharacterSaveData{isSlotEmpty=false};saved.bodyStatLevels[child]=1;stats.LoadFromSlot(saved);Stage(0);Expect(stats.IsNodeUnlocked(child),"Old-save ownership lost");body.CloseCenterModal();Highest();
        });
        for(int i=0;i<6;i++)
        {
            int index=i;Case("body-upgrade-node-"+i,"BODY_DETAIL","UI/BodyTemperingPanelUI.cs","OpenCenterModal,UpgradeNode,UpgradeMax",()=>{
                Rich();var saved=new CharacterSaveData{isSlotEmpty=false,bodyStatLevels=Enumerable.Repeat(20,6).ToArray()};stats.LoadFromSlot(saved);body.Refresh();Click(body.nodeSelectButtons[index]);int lv=stats.GetLevel(index);float money=EconomyManager.Instance.currentLinhThach,cost=stats.GetUpgradeCost(index);
                Click(body.btnModalUpgrade);Expect(stats.GetLevel(index)==lv+1,"Wrong body node/duplicate callback");Near(EconomyManager.Instance.currentLinhThach,money-cost,"Body cost");
                Click(body.btnModalUpgrade5);Expect(stats.GetLevel(index)==lv+6,"x5 input wrong");Click(body.btnModalUpgradeMax);Expect(stats.GetLevel(index)>lv+6,"Max command absent");Click(body.centerModalCloseBtn);
            });
        }
        CloseDrawer();Highest();
        Case("character-ten-slot-data","CHARACTER","UI/CharacterDetailModalUI.cs","OpenPanel,RefreshData,ClosePanel",()=>{
            var slot=new CharacterSaveData{swords=Enumerable.Range(0,10).Select(i=>new SwordSaveEntry{level=2,isEquipped=true,equipSlotIndex=i}).ToArray()};MergeManager.Instance.LoadFromSlot(slot);Find<CharacterDetailModalUI>().OpenPanel();var ui=Find<CharacterDetailModalUI>();Expect(ui.txtEquipSwords.text.Contains("10/10"),"Not all equipped slots counted: "+ui.txtEquipSwords.text);Expect(ui.txtName.text.Contains(data.gameSaveData.slots[0].characterName),"Name data stale");Click(ui.closeButton);
        });
        Case("dismantle-cancel-confirm-once","DISMANTLE_CONFIRM","UI/DismantleConfirmModalUI.cs","OpenConfirm,OnConfirmClicked,Close",()=>{
            int fired=0;DismantleConfirmModalUI.OpenConfirm("QA sword","fixture",new DismantleReward(),()=>fired++);var ui=Find<DismantleConfirmModalUI>();Click((Button)Field(ui,"btnCancel"));Expect(fired==0,"Cancel invoked transaction");
            DismantleConfirmModalUI.OpenConfirm("QA sword","fixture",new DismantleReward(),()=>fired++);Click((Button)Field(ui,"btnConfirm"));Expect(fired==1,"Confirm count incorrect");
        });
        foreach(SecretRealmType type in Enum.GetValues(typeof(SecretRealmType)))
        {
            var trial=type;
            Case("trial-entry-"+type,"SECRET_HUB","UI/SecretRealmModalUI.cs","OnEnterRealmClicked",()=>{
                var svc=SecretRealmService.Instance;svc.State.lastResetDate=DateTime.UtcNow.ToString("yyyy-MM-dd");svc.State.freeAttemptsToday=0;EconomyManager.Instance.currentCoDuyen=100;
                SecretRealmModalUI.Instance.Open();Click(Named(SecretRealmModalUI.Instance.IsModalOpen?GameObject.Find("Canvas").transform.Find("SecretRealmModal/ModalBox/ScrollView/Viewport/Content/Card_"+trial):null,"BtnEnter"));
                Expect(EconomyManager.Instance.currentCoDuyen==95,"Entry not charged exactly once");Expect((SecretRealmType)Field(SecretRealmPlayController.Instance,"activeType")==trial,"Wrong trial input");Expect(SecretRealmPlayController.Instance.ActiveNavigationLayer!=null,"Play layer missing");
            });
            yield return 0.2;
            Case("trial-input-"+type,"TRIAL_"+type.ToString().ToUpperInvariant(),"UI/SecretRealmPlayController.cs","LaunchMiniGame",()=>{
                var play=SecretRealmPlayController.Instance;int score=(int)Field(play,"currentScore");
                if(trial==SecretRealmType.DanCauldron){var root=play.ActiveNavigationLayer;Click(Named(root,"Cauldron_0"));Expect((bool)Field(play,"cauldronChosen"),"Cauldron callback absent");}
                else if(trial==SecretRealmType.LightningDodge){Call(play,"SpawnThunderOrb");var root=play.ActiveNavigationLayer;var b=root.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("ThunderOrb_"));int n=(int)Field(play,"orbsCollected");Click(b);Expect((int)Field(play,"orbsCollected")==n+1,"Orb callback absent");}
                else{float hp=trial==SecretRealmType.BarrierBreak?(float)Field(play,"barrierCurrentHp"):0;if(trial==SecretRealmType.HordeSurvival)Call(play,"SpawnHordeDemon");var action=(GameObject)Field(play,"actionButtonGo");Click(action.GetComponent<Button>());if(trial==SecretRealmType.SwordTomb)Expect((int)Field(play,"swordTombAttemptsLeft")==2,"Sword attempt input wrong");else if(trial==SecretRealmType.BarrierBreak)Near((float)Field(play,"barrierCurrentHp"),hp-50000,"Barrier input absent");else Expect((int)Field(play,"currentScore")>score,"Horde input absent");}
            });
            Case("trial-back-"+type,"TRIAL_ROOT","UI/MobileUINavigationManager.cs","HandleBackNavigation",()=>{Expect(MobileUINavigationManager.Instance.HandleBackNavigation(),"Trial Back unhandled");Expect(SecretRealmPlayController.Instance.ActiveNavigationLayer==null,"Trial not closed");Expect(SecretRealmModalUI.Instance.IsModalOpen,"Hub not restored");SecretRealmModalUI.Instance.Close();});
            int clearedAtExit=SecretRealmService.Instance.State.totalCleared;
            yield return 1.4;
            Case("trial-no-late-result-"+type,"TRIAL_ROOT","UI/SecretRealmPlayController.cs","CloseAll,EndMiniGame",()=>{Expect(SecretRealmPlayController.Instance.ActiveNavigationLayer==null,"Closed trial reopened delayed result");Expect(SecretRealmService.Instance.State.totalCleared==clearedAtExit,"Exited trial granted delayed rewards");});
        }
        Case("trial-result-actual-zero-and-back","SECRET_RESULT","UI/SecretRealmPlayController.cs","ShowResultModal,CloseAll",()=>{
            var play=SecretRealmPlayController.Instance;float money=EconomyManager.Instance.currentLinhThach;play.ShowResultModal(default);var root=play.ActiveNavigationLayer;var text=root.Find("Card/RewardBox/RewardDetailsText").GetComponent<Text>().text;Expect(!text.Contains("20,000")&&!text.Contains("Bậc 8"),"Fabricated jackpot shown");Near(EconomyManager.Instance.currentLinhThach,money,"Result display granted money");Expect(MobileUINavigationManager.Instance.HandleBackNavigation(),"Result Back unhandled");Expect(play.ActiveNavigationLayer==null,"Result stayed open");SecretRealmModalUI.Instance.Close();
        });
        foreach(var step in AdditionalSuite())yield return step;
        foreach(var step in HudAndTribulationSuite())yield return step;
        foreach(var step in ResponsiveRegressionSuite())yield return step;
        Case("save-write-load-roundtrip","SAVE_LIFECYCLE","Data/DataManager.cs","SaveCurrentGame,SaveData,LoadData,LoadCurrentGame",()=>{
            CloseDrawer();data.SaveCurrentGame(true);float money=EconomyManager.Instance.currentLinhThach;int level=art.GetLevel(ArtifactSlot.Crown),bodyLevel=stats.GetLevel(0);string name=data.gameSaveData.slots[0].characterName;
            EconomyManager.Instance.currentLinhThach=0;data.gameSaveData=new GameSaveData();data.LoadData();Expect(data.gameSaveData.slots[0].characterName==name,"Disk load name lost");data.LoadCurrentGame();Near(EconomyManager.Instance.currentLinhThach,money,"Wallet roundtrip");Expect(art.GetLevel(ArtifactSlot.Crown)==level&&stats.GetLevel(0)==bodyLevel,"Progression roundtrip lost");Note("diskRoundTrip",true);
            System.IO.File.WriteAllText(Evidence+"/resume-expected.json",Newtonsoft.Json.Linq.JObject.FromObject(new{money,name,ore=art.Ore,bodyLevel,artifactCrown=level,formation=form.State.levels,filters=StorageManager.Instance.State}).ToString());
        });
        RuntimeBindingsSnapshot();
        yield return 0.3;
    }
}
#endif

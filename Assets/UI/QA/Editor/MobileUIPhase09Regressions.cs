#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static partial class MobileUIPhase09Checks
{
    static void HitClick(Button button)
    {
        var point=PixelRect((RectTransform)button.transform).center;var hits=new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
        Expect(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"Actual EventSystem hit intercepted for "+button.name);Click(button);
    }
    static void DragScroll(ScrollRect scroll,float pixels)
    {
        var start=PixelRect(scroll.viewport).center;var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=start,pressPosition=start,pointerDrag=scroll.gameObject};
        ExecuteEvents.Execute(scroll.gameObject,pointer,ExecuteEvents.beginDragHandler);pointer.position=start+new Vector2(0,pixels);pointer.delta=new Vector2(0,pixels);
        ExecuteEvents.Execute(scroll.gameObject,pointer,ExecuteEvents.dragHandler);ExecuteEvents.Execute(scroll.gameObject,pointer,ExecuteEvents.endDragHandler);
    }
    static IEnumerable ResponsiveRegressionSuite()
    {
        Highest();Rich();CloseDrawer();var root=GameObject.Find("Canvas").transform.Find("MobileUIRoot");var drawer=MobileDrawerUIController.Instance;
        drawer.OpenDrawer(1);yield return .35;var body=root.Find("Drawer_1").GetComponent<BodyTemperingPanelUI>();
        Case("raycast-body-opens-selected-node","BODY","UI/BodyTemperingPanelUI.cs","OpenCenterModal",()=>{
            body.treeScrollRect.verticalNormalizedPosition=1;body.Refresh();int level=PlayerStatsService.Instance.GetLevel(0);HitClick(body.nodeQuickUpgradeButtons[0]);Expect(body.centerModalRoot.activeInHierarchy,"Card action did not open its original detail route");Expect(PlayerStatsService.Instance.GetLevel(0)==level,"Opening detail unexpectedly upgraded");Expect(body.connectorLines.All(i=>i==null||!i.raycastTarget),"Connector still blocks touches");
        });yield return .25;
        Case("raycast-body-upgrade-through-connectors","BODY_DETAIL","UI/BodyTemperingPanelUI.cs","UpgradeNode",()=>{int level=PlayerStatsService.Instance.GetLevel(0);HitClick(body.btnModalUpgrade);Expect(PlayerStatsService.Instance.GetLevel(0)==level+1,"Detail upgrade not delivered exactly once");});body.CloseCenterModal();CloseDrawer();
        Case("daoan-no-duplicate-shortcut","HUD","UI/DaoAnModalUI.cs","BuildIfNeeded,Open",()=>{
            var buttons=root.GetComponentsInChildren<Button>(true).Where(b=>b.name=="BtnHUDDaoAn").ToArray();Expect(buttons.Length==1,"Factory recreated obsolete shortcut");Expect(PixelRect((RectTransform)buttons[0].transform).height*360/Screen.width>=43.5f,"Replacement shortcut too short");HitClick(buttons[0]);Expect(DaoAnModalUI.Instance.IsModalOpen,"Retained full-size shortcut not connected");
        });yield return .25;DaoAnModalUI.Instance.Close();
        var hud=Find<PortraitHudController>();TutorialManager.Instance.PointToTarget(hud.stage,"P09 visible target",1);yield return .1;
        Find<CharacterDetailModalUI>().OpenPanel();yield return .3;var character=Find<CharacterDetailModalUI>();
        Case("character-rendered-data-and-tutorial-occlusion","CHARACTER","UI/CharacterDetailModalUI.cs","OpenPanel,RefreshData",()=>{
            Expect(character.txtName.text.Contains(DataManager.Instance.gameSaveData.slots[0].characterName),"Name not synchronized");Expect(character.txtName.cachedTextGenerator.vertexCount>4,"Name was data-only with no rendered glyphs");
            var pointer=GameObject.Find("Canvas").transform.Find("TutorialPointerArrow");Expect(pointer!=null&&pointer.GetComponent<CanvasGroup>().alpha==0,"Gameplay hint bleeds over modal");Expect(ReferenceEquals(Field(TutorialManager.Instance,"currentTargetRect"),hud.stage),"Occlusion lost tutorial target");
        });
        character.scrollRect.verticalNormalizedPosition=1;yield return .1;
        Case("character-real-scroll-drag","CHARACTER","UI/CharacterDetailModalUI.cs","OpenPanel",()=>{DragScroll(character.scrollRect,350);Expect(character.scrollRect.verticalNormalizedPosition<.99f,"uGUI drag did not scroll retained rows");});
        HitClick(character.closeButton);yield return .25;
        Case("tutorial-resumes-after-modal","QUEST","Managers/TutorialManager.cs","PointToTarget",()=>{var pointer=GameObject.Find("Canvas").transform.Find("TutorialPointerArrow");Expect(pointer.gameObject.activeInHierarchy&&pointer.GetComponent<CanvasGroup>().alpha==1,"Tutorial hint not restored");});
        SecretRealmPlayController.Instance.LaunchMiniGame(SecretRealmType.BarrierBreak);yield return .3;var play=SecretRealmPlayController.Instance;var trial=play.ActiveNavigationLayer;
        Case("trial-rule-scroll-and-density","TRIAL_BARRIER","UI/SecretRealmPlayController.cs","LaunchMiniGame",()=>{
            var scroll=trial.Find("HeaderPanel/RuleCard").GetComponent<ScrollRect>();Expect(scroll!=null&&scroll.content!=null,"Long rule not scrollable");Expect(scroll.content.GetComponent<MobileUITextDensity>()!=null,"Scaled text density not corrected");
            scroll.verticalNormalizedPosition=1;DragScroll(scroll,80);Expect(scroll.verticalNormalizedPosition<.99f,"Rule content cannot be read by drag");
        });
        Case("trial-cosmetics-never-capture-input","TRIAL_BARRIER","UI/SecretRealmPlayController.cs","SpawnFloatingText,SpawnHitSpark",()=>{
            Call(play,"SpawnFloatingText","QA FX",Vector2.zero,Color.white,1f);Call(play,"SpawnHitSpark",Vector2.zero,Color.white,32f);
            Expect(trial.GetComponentsInChildren<Graphic>().Where(g=>g.name=="FText"||g.name=="HitSpark").All(g=>!g.raycastTarget),"Decorative feedback intercepts input");
            float hp=(float)Field(play,"barrierCurrentHp");HitClick(Named(trial,"MainActionBtn"));Near((float)Field(play,"barrierCurrentHp"),hp-50000,"Real raycast action sent wrong damage");
        });play.CloseAll();SecretRealmModalUI.Instance.Close();
        play.LaunchMiniGame(SecretRealmType.LightningDodge);yield return .3;trial=play.ActiveNavigationLayer;
        Case("trial-topmost-orb-real-hit","TRIAL_LIGHTNING","UI/SecretRealmPlayController.cs","SpawnThunderOrb",()=>{
            Call(play,"SpawnThunderOrb");var candidates=trial.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("ThunderOrb_")).ToArray();
            var target=candidates.LastOrDefault(b=>{var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=PixelRect((RectTransform)b.transform).center},hits);return hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b;});Expect(target!=null,"No orb accessible through raycast");int before=(int)Field(play,"orbsCollected");HitClick(target);Expect((int)Field(play,"orbsCollected")==before+1,"Topmost orb callback not exactly once");
        });play.CloseAll();SecretRealmModalUI.Instance.Close();
        var combat=CombatManager.Instance;float previousHp=combat.playerCurrentHp;var visualField=combat.GetType().GetField("playerVisualHp",All);float previousVisual=(float)visualField.GetValue(combat);
        foreach(float fraction in new[]{0f,.5f,1f})
        {
            combat.playerCurrentHp=combat.playerMaxHp*fraction;visualField.SetValue(combat,combat.playerCurrentHp);Call(combat,"UpdatePlayerHPUI");yield return .06;
            Case("combat-hp-progress-"+fraction,"WORLD_COMBAT_UI","Managers/CombatManager.cs","Update,UpdatePlayerHPUI",()=>{Expect(Mathf.Abs(combat.playerHpFill.rectTransform.anchorMax.x-fraction)<.001f,"HP anchor fill mismatch");Expect(combat.playerHpText.text.Length>0,"HP value absent");});
        }
        combat.playerCurrentHp=previousHp;visualField.SetValue(combat,previousVisual);Call(combat,"UpdatePlayerHPUI");
        Case("skill-mana-progress-endpoints","SKILLHUD","UI/MobileSkillHUD.cs","UpdateUltimateUI",()=>{
            var skill=SkillManager.Instance;var ui=Find<MobileSkillHUD>();float mana=skill.currentLinhLuc;var previousSkill=skill.activeSkill;var fixture=UnityEngine.Object.Instantiate(previousSkill);fixture.isUnlocked=true;skill.activeSkill=fixture;
            try{foreach(float fraction in new[]{0f,.5f,1f}){skill.currentLinhLuc=skill.maxLinhLuc*fraction;Call(ui,"UpdateUltimateUI",skill);Near(ui.imgUltiFill.fillAmount,fraction,"Mana fill mismatch");}}
            finally{skill.currentLinhLuc=mana;skill.activeSkill=previousSkill;UnityEngine.Object.Destroy(fixture);Call(ui,"UpdateUltimateUI",skill);}
        });
        drawer.OpenDrawer(2);yield return .3;
        Case("alchemy-real-countdown-status","ALCHEMY","UI/AlchemyPanelUI.cs","RefreshUI,UpdateBrewTimer",()=>{
            var service=AlchemyService.Instance;var ui=Find<AlchemyPanelUI>();var state=service.State;int recipe=state.brewingRecipe,realm=state.brewingRealm;long deadline=state.brewEndUtcTicks;
            try{state.brewingRecipe=0;state.brewingRealm=0;state.brewEndUtcTicks=DateTime.UtcNow.AddSeconds(65).Ticks;ui.RefreshUI();Expect(ui.txtFurnaceTimer.text.Contains("01:04")||ui.txtFurnaceTimer.text.Contains("01:05"),"Countdown not bound to saved deadline");Expect(ui.txtFurnaceStatus.text.Contains("Tu Vi"),"Recipe status disconnected");state.brewingRecipe=-1;ui.RefreshUI();Expect(ui.txtFurnaceStatus.text.Contains("Trống"),"Idle status stale");}
            finally{state.brewingRecipe=recipe;state.brewingRealm=realm;state.brewEndUtcTicks=deadline;ui.RefreshUI();}
        });CloseDrawer();
        Case("cultivation-phase-progress","HUD","UI/CultivationPhaseBarUI.cs","Refresh",()=>{
            var cultivation=CultivationManager.Instance;int stage=cultivation.currentStageIndex;float tuvi=cultivation.currentTuVi;
            try{foreach(float fraction in new[]{0f,.5f,1f}){cultivation.SetStageAndTuViForEditor(0,cultivation.allStages[0].requiredTuVi*fraction);var bar=Find<CultivationPhaseBarUI>();var fills=(Image[])Field(bar,"fills");Expect(Mathf.Abs(fills[0].rectTransform.anchorMax.x-fraction/3)<.001f,"First cultivation phase not synchronized");}}
            finally{cultivation.SetStageAndTuViForEditor(stage,tuvi);}
        });
        yield return .15;
    }
}
#endif

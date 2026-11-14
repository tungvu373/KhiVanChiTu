#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static partial class MobileUIPhase09Checks
{
    static IEnumerable HudAndTribulationSuite()
    {
        CloseDrawer();
        Case("hud-wallet-live-data","HUD","UI/PortraitHudController.cs","RefreshResources,ShowWallet",()=>{
            var hud=Find<PortraitHudController>();var label=hud.wallet.Find("txt_LinhThach").GetComponent<Text>();string before=label.text;EconomyManager.Instance.currentLinhThach=1000;EconomyManager.Instance.AddLinhThach(321);Expect(label.text!=before,"Wallet event did not synchronize");Click(hud.walletButton);Expect(hud.detailsText.text.Contains(1321f.ToString("N0")),"Wallet detail value stale");Expect(hud.CloseTopLayer(),"Wallet overlay not opened");Note("compactValue",label.text);Note("actualWallet",EconomyManager.Instance.currentLinhThach);
        });
        Case("quest-toggle-state","QUEST","UI/CollapsibleQuestBar.cs","ToggleQuestBar",()=>{var ui=Find<CollapsibleQuestBar>();var b=ui.GetComponentsInChildren<Button>(true).First(x=>x.name=="BtnToggle");var before=Newtonsoft.Json.JsonConvert.SerializeObject(new{ui.enabled});Click(b);Click(b);Expect(ui.gameObject.activeInHierarchy,"Quest bar inaccessible after toggle");});
        Case("skills-index-mana-cooldown-locks","SKILLHUD","UI/MobileSkillHUD.cs","WireButtons,UpdateNormalSkillsUI,UpdateUltimateUI",()=>{
            var sm=SkillManager.Instance;var ui=Find<MobileSkillHUD>();var old=sm.normalSkills;var oldActive=sm.activeSkill;
            var copies=old.Select(s=>UnityEngine.Object.Instantiate(s)).ToArray();var ultimate=UnityEngine.Object.Instantiate(oldActive);
            try{
                sm.normalSkills=copies;sm.activeSkill=ultimate;sm.isSkillActive=false;sm.maxLinhLuc=1000;sm.currentLinhLuc=1000;
                var cooldowns=(float[])Field(sm,"normalCooldowns");for(int i=0;i<3;i++){cooldowns[i]=0;copies[i].isUnlocked=true;}
                for(int i=0;i<3;i++){
                    Call(ui,"UpdateNormalSkillsUI",sm);float mana=sm.currentLinhLuc,cost=sm.GetSkillManaCost(i);Click(ui.btnNormalSkills[i]);Near(sm.currentLinhLuc,mana-cost,"Wrong skill mana/index");Expect(sm.GetNormalSkillCooldown(i)>0,"Skill cooldown absent");Call(ui,"UpdateNormalSkillsUI",sm);Expect(!ui.btnNormalSkills[i].interactable,"Cooldown control enabled");
                }
                copies[0].isUnlocked=false;Call(ui,"UpdateNormalSkillsUI",sm);Expect(!ui.btnNormalSkills[0].interactable&&ui.lockOverlays[0].activeSelf,"Skill lock state absent");
                ultimate.isUnlocked=true;sm.currentLinhLuc=0;Call(ui,"UpdateUltimateUI",sm);Expect(!ui.btnUltimate.interactable,"Empty mana ultimate enabled");sm.currentLinhLuc=sm.maxLinhLuc;Call(ui,"UpdateUltimateUI",sm);Click(ui.btnUltimate);Expect(sm.isSkillActive,"Ultimate callback absent");
            }finally{sm.normalSkills=old;sm.activeSkill=oldActive;sm.isSkillActive=false;CombatManager.Instance.DeactivateVanKiemQuyTong();foreach(var c in copies)UnityEngine.Object.Destroy(c);UnityEngine.Object.Destroy(ultimate);}
        });
        yield return 0.3;
        // Verify cancel/missing-pill state without starting a long combat simulation.
        Case("tribulation-missing-pill-routes-alchemy","TRIBULATION","UI/TribulationModalUI.cs","OpenModal,OnConfirmClicked,RefreshData",()=>{
            CloseDrawer();var cm=CultivationManager.Instance;cm.SetStageAndTuViForEditor(14,cm.allStages[14].requiredTuVi);Expect(cm.IsTribulationReady,"Fixture not at realm boundary");
            for(int q=1;q<=3;q++)AlchemyService.Instance.State.pills[4+q]=0;
            TribulationModalUI.Open();var ui=TribulationModalUI.Instance;Expect(ui!=null&&ui.IsOpen,"Tribulation modal absent");Expect(ui.confirmButton.GetComponentInChildren<Text>().text.Contains("LÒ"),"Missing-pill action not explained");Click(ui.confirmButton);Expect(!CombatManager.Instance.isTribulationBossActive,"Missing pill started boss");
        });
        yield return 0.4;
        Case("tribulation-alchemy-target-and-cancel","TRIBULATION","UI/TribulationModalUI.cs","OnCancelClicked,OnConfirmClicked",()=>{
            Expect(MobileDrawerUIController.Instance.ActiveDrawerIndex==2,"Missing-pill shortcut did not open alchemy");CloseDrawer();AlchemyService.Instance.State.pills[5]=1;TribulationModalUI.Open();var ui=TribulationModalUI.Instance;Click(ui.cancelButton);Expect(AlchemyService.Instance.State.pills[5]==1,"Cancel consumed pill");
        });
        yield return 0.3;
        // Capture real Play-mode rendering of the reconnected footer.
        Highest();Rich();MobileDrawerUIController.Instance.OpenDrawer(1);yield return 0.35;
        var body=GameObject.Find("Canvas").transform.Find("MobileUIRoot/Drawer_1").GetComponent<BodyTemperingPanelUI>();body.OpenCenterModal(0);yield return 0.3;
        Case("body-max-touch-target","BODY_DETAIL","UI/BodyTemperingPanelUI.cs","UpgradeMax",()=>{
            var rect=(RectTransform)body.btnModalUpgradeMax.transform;Expect(rect.rect.height>=132,"Maximum button too short");Expect(rect.rect.width>=132,"Maximum button too narrow");Note("referenceSize",rect.rect.size.ToString());
            var p=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);Expect(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==body.btnModalUpgradeMax,"Maximum button touch intercepted");
        });
        ScreenCapture.CaptureScreenshot(Path.GetFullPath(Evidence+"/body-footer.png"));yield return 0.3;body.CloseCenterModal();CloseDrawer();
    }
    static IEnumerator ResumeSuite()
    {
        yield return 1.0;RequireIsolation();int frame=Time.frameCount;yield return 0.3;
        var expected=JObject.Parse(File.ReadAllText(Evidence+"/resume-expected.json"));
        Case("restart-roster-enter-cancel-confirm","SELECT","UI/MainMenuManager.cs","OpenCharacterSelection,AskConfirmEnterGame,ConfirmEnterGame",()=>{
            Expect(Time.frameCount>frame,"Restart loop frozen");var menu=MainMenuManager.Instance;Click(menu.playButton);Expect(menu.saveSlots[0].nameText.text.Contains((string)expected["name"]),"Saved character not displayed after restart");Click(menu.saveSlots[0].slotButton);Click(menu.popupCancelBtn);Expect(menu.characterSelectPanel.activeInHierarchy,"Enter cancel lost roster");Click(menu.saveSlots[0].slotButton);Click(menu.popupConfirmBtn);
        });
        yield return 0.7;Time.timeScale=0;
        Case("restart-saved-state-and-bindings","SAVE_LIFECYCLE","Data/DataManager.cs","LoadData,LoadCurrentGame",()=>{
            Expect(DataManager.Instance.isSessionActive,"Restart session absent");Near(EconomyManager.Instance.currentLinhThach,(float)expected["money"],"Wallet across Play restart");Expect(ArtifactService.Instance.Ore==(int)expected["ore"],"Ore across restart");Expect(ArtifactService.Instance.GetLevel(ArtifactSlot.Crown)==(int)expected["artifactCrown"],"Artifact across restart");Expect(PlayerStatsService.Instance.GetLevel(0)==(int)expected["bodyLevel"],"Body across restart");Expect(JToken.DeepEquals(JArray.FromObject(FormationService.Instance.State.levels),expected["formation"]),"Formation across restart");
            StorageModalUI.Open();Expect(StorageModalUI.Instance.toggleSwordHighLevel.isOn==StorageManager.Instance.FilterSwordsHighLevel,"Filter UI not rebound after restart");StorageModalUI.Instance.Close();
        });
        yield return 0.4;
        Case("return-menu-save-confirm","CONFIRM_PAUSE","UI/PauseMenuManager.cs","OnReturnMainMenuClicked,SyncAndSaveGame",()=>{var ui=PauseMenuManager.Instance;ui.OpenPauseMenu();Click(ui.returnMainMenuButton);Click(ui.btnPopupConfirm);});
        yield return 0.8;
        Case("returned-menu-profile-retained","MENU","UI/MainMenuManager.cs","InitializeMenu",()=>{Expect(MainMenuManager.Instance!=null&&MainMenuManager.Instance.mainPanel.activeInHierarchy,"Menu reload failed");Expect(DataManager.Instance.SaveProfileId==Profile,"Scene reload changed save owner");});
        yield return 0.3;
    }
    public static void RuntimeBindingsSnapshot()
    {
        RequireIsolation();var buttons=new JArray();var texts=new JArray();
        foreach(var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var delegates=new List<string>();var calls=(IEnumerable)typeof(UnityEngine.Events.UnityEventBase).GetMethod("PrepareInvoke",All).Invoke(b.onClick,null);
            foreach(var call in calls)for(var type=call.GetType();type!=null;type=type.BaseType)
                foreach(var field in type.GetFields(All|System.Reflection.BindingFlags.DeclaredOnly))if(typeof(Delegate).IsAssignableFrom(field.FieldType)&&field.GetValue(call) is Delegate handler)
                    delegates.AddRange(handler.GetInvocationList().Select(h=>h.Method.DeclaringType.FullName+"."+h.Method.Name));
            buttons.Add(JObject.FromObject(new{path=UnityEditor.AnimationUtility.CalculateTransformPath(b.transform,null),active=b.gameObject.activeInHierarchy,interactable=b.IsInteractable(),listeners=delegates.Distinct().ToArray(),persistent=b.onClick.GetPersistentEventCount()}));
        }
        foreach(var t in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))texts.Add(JObject.FromObject(new{path=UnityEditor.AnimationUtility.CalculateTransformPath(t.transform,null),active=t.gameObject.activeInHierarchy,value=t.text}));
        File.WriteAllText(Evidence+"/runtime-bindings.json",new JObject{{"buttons",buttons},{"texts",texts},{"frame",Time.frameCount},{"profile",DataManager.Instance.SaveProfileId}}.ToString());
    }
}
#endif

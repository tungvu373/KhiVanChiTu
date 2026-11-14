#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Newtonsoft.Json.Linq;

public static partial class MobileUIAnimationChecks
{
    static IEnumerator SimulatorSuite()
    {
        yield return 1.0;RequireIsolation();var menu=MainMenuManager.Instance;DataManager.Instance.gameSaveData=new GameSaveData();DataManager.Instance.SaveData();menu.OpenCharacterCreation(0);menu.nameInput.text="P08 Safe Area Fixture";Click(menu.confirmCreateButton);yield return .5;Time.timeScale=0;
        var type=Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow, UnityEditor.DeviceSimulatorModule",true);
        var existing=Resources.FindObjectsOfTypeAll(type).Cast<EditorWindow>().FirstOrDefault();bool created=existing==null;var window=existing!=null?existing:EditorWindow.GetWindow(type);
        yield return .3;
        var main=type.GetField("m_Main",All).GetValue(window);var index=main.GetType().GetProperty("deviceIndex",All);int previous=(int)index.GetValue(main);var stateField=type.GetField("m_State",All);object previousState=stateField.GetValue(window);
        try
        {
            index.SetValue(main,12);stateField.SetValue(window,Enum.Parse(stateField.FieldType,"Enabled"));main.GetType().GetMethod("Enable",All).Invoke(main,null);type.GetMethod("OnFocus",All).Invoke(window,null);window.Focus();
            yield return .4;Canvas.ForceUpdateCanvases();
            Check("simulator-real-safe-area",()=>{Expect(Screen.width==1170&&Screen.height==2532,"Unexpected device profile");Expect(Screen.safeArea.yMin>0&&Screen.safeArea.yMax<Screen.height,"No real notch/gesture inset");});
            File.WriteAllText(Evidence+"/simulator-screen.json",JObject.FromObject(new{width=Screen.width,height=Screen.height,safeArea=new{Screen.safeArea.x,Screen.safeArea.y,Screen.safeArea.width,Screen.safeArea.height},profileIndex=12,frame=Time.frameCount,source="Unity Device Simulator module, not synthetic Screen.safeArea data"}).ToString());
            StorageModalUI.Open();yield return .06;
            Check("simulator-popup-close-safe-and-live",()=>{
                var ui=StorageModalUI.Instance;Expect(ui.canvasGroup.alpha>0&&ui.canvasGroup.interactable,"Popup lost input during animation");
                var corners=new Vector3[4];((RectTransform)ui.btnClose.transform).GetWorldCorners(corners);
                foreach(var corner in corners){var point=RectTransformUtility.WorldToScreenPoint(null,corner);Expect(Screen.safeArea.Contains(point),"Close control outside safe area");}
                Click(ui.btnClose);
            });yield return .18;
            Check("simulator-close-settled",()=>Expect(!StorageModalUI.Instance.IsModalOpen&&!StorageModalUI.Instance.gameObject.activeSelf,"Close did not finish on notched profile"));
            CultivationManager.Instance.SetStageAndTuViForEditor(CultivationManager.Instance.allStages.Length-1,0);var drawer=MobileDrawerUIController.Instance;drawer.OpenDrawer(2);drawer.OpenDrawer(4);drawer.OpenDrawer(1);yield return .65;
            Check("simulator-rapid-tabs",()=>Expect(drawer.ActiveDrawerIndex==1&&drawer.drawerPanels.Count(p=>p.gameObject.activeSelf)==1,"Notched profile lost latest tab intent"));
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Evidence+"/simulator-hud.png"));yield return .2;drawer.CloseActiveDrawer();yield return .24;
            Check("simulator-no-gameplay-clock-change",()=>Expect(Time.timeScale==0&&!drawer.IsBlockingGameplay,"Simulator animation changed gameplay/input state"));
        }
        finally
        {
            if(created)window.Close();
            else
            {
                index.SetValue(main,previous);stateField.SetValue(window,previousState);
                main.GetType().GetMethod(previousState.ToString()=="Enabled"?"Enable":"Disable",All).Invoke(main,null);
            }
        }
        yield return .2;
    }
}
#endif

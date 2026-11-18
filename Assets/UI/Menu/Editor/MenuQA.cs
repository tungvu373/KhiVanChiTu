#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.Collaboration;
using KhiVanChiTu.UI.Equipment.Editor;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Menu.Editor
{
    [InitializeOnLoad]
    public static class MenuQA
    {
        const string Key="MenuPhase8QA";
        public const string Evidence="Docs/UI_Redesign/Phase8/Evidence";
        static string Profile=>SessionState.GetString(Key+"Profile","");
        static IEnumerator routine;
        static double due,deadline;
        static readonly List<string> checks=new List<string>(),errors=new List<string>();
        static EditorWindow view;static PropertyInfo sizeProperty;static object sizeGroup;static int oldSize;static readonly List<int> sizes=new List<int>();
        static readonly string[] audioKeys={AudioManager.PREF_BGM_VOLUME,AudioManager.PREF_SFX_VOLUME,AudioManager.PREF_IS_MUTED};
        static string Prod=>Path.Combine(Application.persistentDataPath,"KhiVanChiTu_SaveData.json");
        static string Hash(string s)=>Hash128.Compute(s).ToString();
        static string FileHash(string p)=>File.Exists(p)?Hash(Convert.ToBase64String(File.ReadAllBytes(p))):"absent";
        static MenuQA(){EditorApplication.update+=CleanupWhenIdle;EditorApplication.playModeStateChanged+=s=>{if(!SessionState.GetBool(Key,false))return;if(s==PlayModeStateChange.EnteredPlayMode){routine=Run();due=EditorApplication.timeSinceStartup+2;deadline=due+300;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Cleanup;};}
        static void CleanupWhenIdle(){if(SessionState.GetBool(Key,false)&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling)Cleanup();}
        public static void Begin(bool load=false,bool preview=false)
        {
            EquipmentRedesignBuilder.Require();if(SessionState.GetBool(Key,false))Cleanup();if(UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include).Length>0)throw new Exception("Save profile already owned");
            if(!load)SessionState.SetString(Key+"Profile","menu8-qa-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));if(string.IsNullOrEmpty(Profile))throw new Exception("No QA profile for load test");
            SessionState.SetBool(Key+"Load",load);SessionState.SetBool(Key+"Preview",preview);SessionState.SetBool(Key,true);Directory.CreateDirectory(Evidence);if(File.Exists(Evidence+"/failure.txt"))File.Move(Evidence+"/failure.txt",Evidence+"/previous-failure-"+DateTime.UtcNow.Ticks+".txt");
            SessionState.SetString(Key+"Prefs",Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData","")));SessionState.SetString(Key+"File",FileHash(Prod));SessionState.SetString(Key+"Bak",FileHash(Prod+".bak"));SessionState.SetFloat(Key+"Scale",Time.timeScale);
            for(int i=0;i<3;i++){SessionState.SetBool(Key+"Had"+i,PlayerPrefs.HasKey(audioKeys[i]));SessionState.SetFloat(Key+"Audio"+i,i<2?PlayerPrefs.GetFloat(audioKeys[i],.8f):PlayerPrefs.GetInt(audioKeys[i],0));}
            new GameObject("__MenuPhase8QA").AddComponent<WorkspaceSaveProfile>().profileId=Profile;EditorApplication.isPlaying=true;
        }
        static void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m+"\n"+stack);}
        static void Tick()
        {
            EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<due)return;
            try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("QA deadline");if(!routine.MoveNext()){Stop();return;}due=EditorApplication.timeSinceStartup+.14;}
            catch(Exception e){File.WriteAllText(Evidence+"/failure.txt",e.ToString());errors.Add(e.ToString());Stop();}
        }
        static void Check(bool b,string text){if(!b)throw new Exception(text);checks.Add(text);}
        static void Capture(string n){ScreenCapture.CaptureScreenshot(Evidence+"/"+n+"-"+Screen.width+"x"+Screen.height+".png");}
        static UnityEngine.EventSystems.PointerEventData Point(RectTransform rect,float x=.5f)
        {var p=rect.TransformPoint(new Vector2(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,x),rect.rect.center.y));return new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,p),button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};}
        static void Hit(UnityEngine.UI.Button button)
        {
            Canvas.ForceUpdateCanvases();var data=Point((RectTransform)button.transform);var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
            Check(button.IsInteractable(),"Button available "+button.name);Check(hits.Count>0&&(hits[0].gameObject.transform==button.transform||hits[0].gameObject.transform.IsChildOf(button.transform)),"Touch reaches "+button.name+" over "+(hits.Count>0?hits[0].gameObject.name:"nothing"));UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,data,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
        }
        static void Texts(Transform root)
        {Canvas.ForceUpdateCanvases();foreach(var t in root.GetComponentsInChildren<TMP_Text>()){if(!t.isActiveAndEnabled)continue;t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Text fits "+t.name+" "+Screen.width+"x"+Screen.height);}}
        static void Drag(UnityEngine.UI.ScrollRect s)
        {Canvas.ForceUpdateCanvases();s.verticalNormalizedPosition=1;s.StopMovement();if(s.content.rect.height<=s.viewport.rect.height+1){Check(s.movementType==UnityEngine.UI.ScrollRect.MovementType.Clamped,"Fitting list remains stable "+s.name);return;}var before=s.content.anchoredPosition;var p=Point(s.viewport);p.pressPosition=p.position;s.OnInitializePotentialDrag(p);s.OnBeginDrag(p);p.position+=new Vector2(0,180);s.OnDrag(p);s.OnEndDrag(p);s.StopMovement();Check((s.content.anchoredPosition-before).sqrMagnitude>4,"Native list scroll "+s.name);s.verticalNormalizedPosition=1;}
        static void Slider(UnityEngine.UI.Slider s,float ratio)
        {
            var area=s.handleRect.parent as RectTransform;var e=Point(area,ratio);s.OnPointerDown(e);s.OnDrag(e);s.OnPointerUp(e);Check(Mathf.Abs(s.value-ratio)<.025f,"Native audio slider drag "+ratio);
            Check(s.handleRect.rect.width>=72&&s.handleRect.rect.height>=72,"Slider handle touch area");
        }
        static IEnumerable Ready(UnityEngine.UI.Button b)
        {
            for(int i=0;i<24;i++){Canvas.ForceUpdateCanvases();var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(Point((RectTransform)b.transform),hits);if(hits.Count>0&&(hits[0].gameObject.transform==b.transform||hits[0].gameObject.transform.IsChildOf(b.transform)))yield break;yield return null;}
        }
        static IEnumerator Run()
        {
            checks.Clear();errors.Clear();if(SessionState.GetBool(Key+"Load",false)&&File.Exists(Evidence+"/create-checks.json"))checks.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Evidence+"/create-checks.json")));
            while(DataManager.Instance==null||MainMenuManager.Instance==null)yield return null;
            Check(DataManager.Instance.SaveProfileId==Profile,"Unique QA save profile "+Profile);var m=MainMenuManager.Instance;var ui=m.GetComponent<MenuPresentation>();SetupSizes();SelectSize(new Vector2Int(1080,1920));yield return null;yield return null;
            int frame=Time.frameCount;yield return null;Check(Time.frameCount>frame,"Player loop advances in menu");Check(Time.timeScale==0,"Original menu time freeze retained");
            if(SessionState.GetBool(Key+"Preview",false)){while(true)yield return null;}
            bool load=SessionState.GetBool(Key+"Load",false);
            if(load)
            {
                Check(m.playButtonText.text.Contains("Tiếp"),"Continue detected saved character after Play restart");
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
                {
                    SelectSize(size);yield return null;Hit(m.playButton);yield return null;yield return null;Check(!DataManager.Instance.gameSaveData.slots[0].isSlotEmpty,"Saved character remains present");Texts(m.characterSelectPanel.transform);Capture("character-selection-saved");yield return null;
                    string before=JsonUtility.ToJson(DataManager.Instance.gameSaveData);Hit(m.saveSlots[0].deleteButton);yield return null;Texts(m.confirmationPopup.transform);Capture("confirm-delete-cancel-only");yield return null;Check(m.confirmationPopup.GetComponent<MenuDialogView>().message.text.Contains(SessionState.GetString(Key+"Name","")),"Delete warning names selected character");Hit(m.popupCancelBtn);yield return null;Check(JsonUtility.ToJson(DataManager.Instance.gameSaveData)==before,"Cancel delete leaves all characters untouched");foreach(var step in Ready(m.backFromSelectButton))yield return step;Hit(m.backFromSelectButton);yield return null;
                }
                Hit(m.quitButton);yield return null;Hit(m.popupCancelBtn);yield return null;Check(m.gameObject.activeInHierarchy,"Cancel quit stays in main menu");Hit(m.playButton);yield return null;
                Hit(ui.cards[0].enter.button);yield return null;Check(m.confirmationPopup.activeInHierarchy,"Load confirmation opens");Hit(m.popupConfirmBtn);yield return null;yield return null;yield return null;Check(!m.gameObject.activeInHierarchy,"Load enters gameplay");Check(CultivationManager.Instance.CharacterName==SessionState.GetString(Key+"Name",""),"Character name restored from actual saved profile");Check(Time.timeScale==1,"Load restores original running state");Check(errors.Count==0,"No runtime errors in restart Load");yield break;
            }
            Check(DataManager.Instance.gameSaveData.slots.All(s=>s.isSlotEmpty),"Fresh unique profile without deleting any character");
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;Texts(m.mainPanel.transform);Capture("main-menu");yield return null;Hit(m.settingsButton);yield return null;Texts(m.settingsPanel.transform);Slider(m.bgmSlider,.2f);Slider(m.sfxSlider,.8f);yield return null;Check(ui.gameObject.activeInHierarchy,"Settings remains outside gameplay");Capture("main-settings");yield return null;Hit(m.closeSettingsButton);yield return null;
                Hit(m.playButton);yield return null;yield return null;Texts(m.characterSelectPanel.transform);Capture("character-selection-empty");yield return null;Check(m.saveSlots.All(s=>s.emptyPanel.activeSelf),"All new-profile slots empty");Drag(ui.selectionScroll);Hit(ui.cards[0].enter.button);yield return null;Texts(m.characterCreatePanel.transform);Capture("character-creation");yield return null;Hit(m.confirmCreateButton);yield return null;Check(DataManager.Instance.gameSaveData.slots.All(s=>s.isSlotEmpty),"Empty name never creates or changes a character");Check(ui.creationHint.text.Contains("nhập"),"Empty name validation visible");Hit(m.cancelCreateButton);yield return null;Hit(m.backFromSelectButton);yield return null;
            }
            SelectSize(new Vector2Int(1080,1920));yield return null;Hit(m.playButton);yield return null;Hit(ui.cards[0].enter.button);yield return null;
            string name="Huyền Thiên Đạo Hữu Trường Sinh Vô Cực";SessionState.SetString(Key+"Name",name);m.nameInput.text=name;Hit(m.confirmCreateButton);yield return null;yield return null;yield return null;
            Check(!m.gameObject.activeInHierarchy&&Time.timeScale==1,"Native New Game starts session");Check(!DataManager.Instance.gameSaveData.slots[0].isSlotEmpty&&DataManager.Instance.gameSaveData.slots[0].characterName==name,"Native New Game saves chosen name");
            var p=PauseMenuManager.Instance;var pv=p.GetComponent<MenuPauseView>();var info=CharacterDetailModalUI.Instance;var iv=info.GetComponent<MenuCharacterInfoView>();
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(720,1280)})
            {
                SelectSize(size);yield return null;Time.timeScale=.35f;p.OpenPauseMenu();yield return null;yield return null;Check(Time.timeScale==.35f&&p.IsModalOpen,"Pause menu keeps existing realtime policy "+size);Texts(pv.mainPage);Capture("pause-menu");yield return null;
                var hud=GlobalNavigationController.Instance.hud.legacy.profileButton;var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(Point((RectTransform)hud.transform),hits);Check(hits.Count>0&&!hits[0].gameObject.transform.IsChildOf(hud.transform)&&hits[0].gameObject!=hud.gameObject,"Pause blocks touch to HUD");
                Hit(pv.audio.button);yield return null;Texts(pv.settingsPage);Slider(p.bgmSlider,.15f);Slider(p.sfxSlider,.85f);yield return null;Check(Mathf.Abs(AudioManager.Instance.bgmVolume-.15f)<.025f&&Mathf.Abs(AudioManager.Instance.sfxVolume-.85f)<.025f,"Native callbacks update audio service");
                var toggle=p.muteToggle;var e=Point((RectTransform)toggle.transform);UnityEngine.EventSystems.ExecuteEvents.Execute(toggle.gameObject,e,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);yield return null;Check(AudioManager.Instance.isMuted==toggle.isOn,"Native mute callback retained");AudioManager.Instance.LoadSettings();Check(Mathf.Abs(AudioManager.Instance.bgmVolume-.15f)<.025f&&AudioManager.Instance.isMuted==toggle.isOn,"Settings persistence reload matches actual controls");Capture("pause-settings");yield return null;
                Hit(pv.back.button);yield return null;Hit(p.manualSaveButton);yield return null;Check(MenuCopy.Plain(p.txtSaveStatus.text).Contains("Đã lưu"),"Manual save status feedback");Hit(p.returnMainMenuButton);yield return null;Texts(p.confirmationPopup.transform);Capture("confirm-return-main");yield return null;Hit(p.btnPopupCancel);yield return null;Check(p.IsModalOpen,"Cancel return keeps current session");Hit(p.quitButton);yield return null;Hit(p.btnPopupCancel);yield return null;Hit(p.resumeButton);yield return null;Check(!p.IsModalOpen&&Time.timeScale==.35f,"Resume preserves prior time scale "+size);Time.timeScale=1;
                CharacterDetailModalUI.Open();yield return null;yield return null;yield return null;Texts(iv.transform);Capture("character-info-top");yield return null;Drag(iv.scroll);iv.scroll.verticalNormalizedPosition=0;yield return null;Capture("character-info-bottom");yield return null;Hit(iv.close.button);yield return null;yield return null;yield return null;Check(!info.gameObject.activeInHierarchy,"Character information closes "+size);
            }
            DataManager.Instance.SaveCurrentGame(true);Check(DataManager.Instance.gameSaveData.slots.Skip(1).All(s=>s.isSlotEmpty),"Unused character slots remain unchanged");Check(errors.Count==0,"No runtime console errors in Phase8 create and gameplay QA");
        }
        static void SetupSizes()
        {
            var asm=typeof(UnityEditor.Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var instance=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);var gt=type.GetProperty("currentGroupType",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instance);sizeGroup=type.GetMethod("GetGroup").Invoke(instance,new[]{gt});view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));sizeProperty=view.GetType().GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);oldSize=(int)sizeProperty.GetValue(view);
        }
        static void SelectSize(Vector2Int size)
        {var asm=typeof(UnityEditor.Editor).Assembly;var entry=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.ToObject(asm.GetType("UnityEditor.GameViewSizeType"),1),size.x,size.y,"Phase8 "+size},null);sizeGroup.GetType().GetMethod("AddCustomSize").Invoke(sizeGroup,new[]{entry});int count=(int)sizeGroup.GetType().GetMethod("GetTotalCount").Invoke(sizeGroup,null);sizes.Add(count-1);sizeProperty.SetValue(view,count-1);view.Focus();}

        static void Stop(){EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllText(Evidence+"/checks.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));File.WriteAllText(Evidence+"/console-errors.json",Newtonsoft.Json.JsonConvert.SerializeObject(errors,Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        static void Cleanup()
        {
            foreach(var marker in UnityEngine.Object.FindObjectsByType<WorkspaceSaveProfile>(FindObjectsInactive.Include))if(marker.profileId==Profile)UnityEngine.Object.DestroyImmediate(marker.gameObject);
            for(int i=0;i<3;i++){if(!SessionState.GetBool(Key+"Had"+i,false))PlayerPrefs.DeleteKey(audioKeys[i]);else if(i<2)PlayerPrefs.SetFloat(audioKeys[i],SessionState.GetFloat(Key+"Audio"+i,0));else PlayerPrefs.SetInt(audioKeys[i],(int)SessionState.GetFloat(Key+"Audio"+i,0));}PlayerPrefs.Save();
            Time.timeScale=SessionState.GetFloat(Key+"Scale",1);if(view!=null&&sizeProperty!=null){sizeProperty.SetValue(view,oldSize);for(int i=sizes.Count-1;i>=0;i--)sizeGroup.GetType().GetMethod("RemoveCustomSize").Invoke(sizeGroup,new object[]{sizes[i]});}sizes.Clear();
            bool audio=true;for(int i=0;i<3;i++)audio&=PlayerPrefs.HasKey(audioKeys[i])==SessionState.GetBool(Key+"Had"+i,false)&&(!PlayerPrefs.HasKey(audioKeys[i])||Mathf.Approximately(i<2?PlayerPrefs.GetFloat(audioKeys[i]):PlayerPrefs.GetInt(audioKeys[i]),SessionState.GetFloat(Key+"Audio"+i,0)));
            File.WriteAllText(Evidence+"/save-preservation.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{profile=Profile,productionPrefsUnchanged=Hash(PlayerPrefs.GetString("KhiVanChiTu_SaveData",""))==SessionState.GetString(Key+"Prefs",""),productionFileUnchanged=FileHash(Prod)==SessionState.GetString(Key+"File",""),productionBackupUnchanged=FileHash(Prod+".bak")==SessionState.GetString(Key+"Bak",""),audioPreferencesRestored=audio},Newtonsoft.Json.Formatting.Indented));SessionState.SetBool(Key,false);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
#endif

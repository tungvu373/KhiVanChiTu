#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Equipment.Editor;
using KhiVanChiTu.UI.Progression;

namespace KhiVanChiTu.UI.Menu.Editor
{
    public static class MenuRedesignBuilder
    {
        static FoundationFactory F=>EquipmentRedesignBuilder.Factory;
        static FoundationTheme T=>EquipmentRedesignBuilder.Theme;
        static Sprite Portrait=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Menu/Art/CharacterPortrait.png");
        static void Stack(RectTransform r,int pad=0,float gap=20)
        {var g=r.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();g.padding=new RectOffset(pad,pad,pad,pad);g.spacing=gap;g.childControlWidth=g.childControlHeight=true;g.childForceExpandHeight=false;}
        static TMP_Text Flow(Transform p,string n,string text,int size,Color? tone=null)
        {var v=F.Text(n,p,text,size);v.color=tone??T.ivory;v.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight=size+14;return v;}
        static TMP_Text Top(Transform p,string n,string text,float y,float h,int size=32,Color? tone=null,float left=32,float right=32)
        {var v=F.Text(n,p,text,size);v.color=tone??T.ivory;RpgUI.Top(v.rectTransform,y,h,left,right);return v;}
        static void Hide(Transform root)
        {foreach(Transform c in root)c.gameObject.SetActive(false);}
        static void Root(Transform root)
        {
            if(root.GetComponent<RpgViewScope>()==null)root.gameObject.AddComponent<RpgViewScope>();
            var r=(RectTransform)root;RpgUI.Fill(r);r.localScale=Vector3.one;
            foreach(var fit in root.GetComponents<JadeModalFit>())fit.enabled=false;
            foreach(var fit in root.GetComponents<MobileUILogicalViewport>())fit.enabled=false;
            foreach(var old in root.GetComponents<JadeModalPresentation>())old.enabled=false;
        }
        static RectTransform Safe(Transform p,string n,bool opaque=true)
        {var r=RpgUI.Rect(n,p);RpgUI.Fill(r);r.gameObject.AddComponent<SafeAreaFitter>();r.gameObject.AddComponent<ProgressionInputShield>();F.Surface(r,opaque?T.ink:Color.clear,true);return r;}
        static MenuButtonLabel Button(UnityEngine.UI.Button b,Transform p,string caption,bool primary=false)
        {
            b.transform.SetParent(p,false);b.gameObject.SetActive(true);RpgUI.Height(b.transform,132);
            var image=b.GetComponent<UnityEngine.UI.Image>();if(image==null)image=b.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=T.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=primary?T.gold:T.raised;b.targetGraphic=image;
            foreach(var o in b.GetComponents<UnityEngine.UI.Outline>())o.enabled=false;
            var legacy=b.GetComponentsInChildren<UnityEngine.UI.Text>(true).FirstOrDefault();foreach(var text in b.GetComponentsInChildren<UnityEngine.UI.Text>(true))text.enabled=false;
            var mirror=b.gameObject.AddComponent<MenuButtonLabel>();mirror.source=legacy;mirror.caption=caption;mirror.label=F.Text("Phase8Label",b.transform,caption??"",32,TextAlignmentOptions.Center);RpgUI.Fill(mirror.label.rectTransform,24,12,24,12);mirror.label.color=primary?T.ink:T.ivory;
            return mirror;
        }
        static UnityEngine.UI.Image PortraitImage(Transform p,string name="Portrait")
        {var r=RpgUI.Rect(name,p);var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.sprite=Portrait;img.preserveAspect=true;img.raycastTarget=false;return img;}
        static RectTransform Header(Transform p,string title,UnityEngine.UI.Button back=null)
        {var r=RpgUI.Rect("Header",p);RpgUI.Top(r,20,132,32,32);var t=Top(r,"Title",title,0,132,48,null,0,190);t.font=T.headingFont;if(back!=null){Button(back,r,"Trở lại");var b=(RectTransform)back.transform;b.anchorMin=b.anchorMax=b.pivot=new Vector2(1,1);b.anchoredPosition=Vector2.zero;b.sizeDelta=new Vector2(168,132);}return r;}
        static void Audio(Transform p,UnityEngine.UI.Slider slider,string caption)
        {
            var r=RpgUI.Rect("Audio_"+caption,p);F.Surface(r,T.surface);RpgUI.Height(r,220);var view=r.gameObject.AddComponent<MenuAudioControl>();view.slider=slider;
            Top(r,"Caption",caption,12,52,34,null,24,180);view.value=Top(r,"Percent","",12,52,34,T.gold,0,24);view.value.rectTransform.anchorMin=new Vector2(.72f,1);view.value.alignment=TextAlignmentOptions.MidlineRight;
            slider.transform.SetParent(r,false);slider.gameObject.SetActive(true);var sr=(RectTransform)slider.transform;RpgUI.Bottom(sr,12,132,16,16);
            var hit=slider.GetComponent<UnityEngine.UI.Image>();if(hit==null)hit=slider.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=Color.clear;hit.raycastTarget=true;
            var background=slider.transform.Find("Background") as RectTransform;if(background!=null){RpgUI.Fill(background,24,58,24,58);var img=background.GetComponent<UnityEngine.UI.Image>();img.sprite=T.surfaceSprite;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=T.ink;}
            if(slider.fillRect!=null)
            {var area=slider.fillRect.parent as RectTransform;RpgUI.Fill(area,24,58,24,58);RpgUI.Fill(slider.fillRect);var img=slider.fillRect.GetComponent<UnityEngine.UI.Image>();img.sprite=T.surfaceSprite;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=T.jade;}
            if(slider.handleRect!=null)
            {var area=slider.handleRect.parent as RectTransform;RpgUI.Fill(area,24,30,24,30);var h=slider.handleRect;h.pivot=new Vector2(.5f,.5f);h.sizeDelta=new Vector2(72,0);var img=h.GetComponent<UnityEngine.UI.Image>();img.sprite=T.surfaceSprite;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=T.gold;slider.targetGraphic=img;}
            foreach(var o in slider.GetComponentsInChildren<UnityEngine.UI.Outline>(true))o.enabled=false;
            foreach(var text in slider.GetComponentsInChildren<UnityEngine.UI.Text>(true))text.enabled=false;
            typeof(UnityEngine.UI.Slider).GetMethod("UpdateVisuals",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(slider,null);
        }
        [MenuItem("TuTien/UI Phase 8/Apply Menu Character Pause and Settings Redesign")]
        public static void Apply()
        {
            EquipmentRedesignBuilder.Require();var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuManager>(FindObjectsInactive.Include);var info=UnityEngine.Object.FindAnyObjectByType<CharacterDetailModalUI>(FindObjectsInactive.Include);
            if(menu==null||pause==null||info==null||menu.GetComponent<MenuPresentation>()!=null)throw new Exception("Use the mobile scene for the initial Phase 8 apply. Existing views must be updated in place.");
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/UI/Menu/Art/CharacterPortrait.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.SaveAndReimport();
            BuildMenu(menu);BuildPause(pause);BuildInfo(info);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
        static void BuildMenu(MainMenuManager m)
        {
            Root(m.transform);var v=m.gameObject.AddComponent<MenuPresentation>();v.owner=m;
            var bg=m.transform.Find("JadeBackground");if(bg!=null){RpgUI.Fill((RectTransform)bg);var image=bg.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=false;var fit=bg.GetComponent<UnityEngine.UI.AspectRatioFitter>()??bg.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=image.sprite.rect.width/image.sprite.rect.height;}
            foreach(var panel in new[]{m.mainPanel,m.characterSelectPanel,m.characterCreatePanel,m.settingsPanel}){Root(panel.transform);Hide(panel.transform);}
            var main=Safe(m.mainPanel.transform,"Phase8Main",false);var veil=RpgUI.Rect("LowerVeil",main);RpgUI.Fill(veil);veil.anchorMax=new Vector2(1,.5f);var gradient=veil.gameObject.AddComponent<MenuVeil>();gradient.color=T.ink;gradient.raycastTarget=false;
            var name=Top(main,"GameTitle","KHÍ VẬN\nCHI TỬ",96,200,72,T.ivory,72,72);name.font=T.headingFont;Top(main,"Tagline","Khí vận tề thiên · Vạn kiếm quy nhất",314,76,32,T.gold,72,72);
            v.continueHint=F.Text("ContinueContext",main,"",30);v.continueHint.color=T.ivory;RpgUI.Bottom(v.continueHint.rectTransform,534,112,72,72);
            Button(m.playButton,main,null,true).source=m.playButtonText;RpgUI.Bottom((RectTransform)m.playButton.transform,364,144,72,72);
            var row=EquipmentRedesignBuilder.Row(main,"SecondaryActions",132);RpgUI.Bottom(row,204,132,72,72);Button(m.settingsButton,row,"Cài đặt");Button(m.quitButton,row,"Thoát game");
            var footer=F.Text("Footer",main,"Một đạo lộ, vạn kỳ duyên",28,TextAlignmentOptions.Center);footer.color=T.muted;RpgUI.Bottom(footer.rectTransform,80,64,72,72);
            var select=Safe(m.characterSelectPanel.transform,"Phase8Selection");Header(select,"Chọn Nhân Vật",m.backFromSelectButton);Top(select,"Hint","Ba đạo lộ · Chọn nhân vật để tiếp tục hoặc tạo mới.",174,84,30,T.muted);
            var list=RpgUI.Rect("CharacterList",select);RpgUI.Fill(list,32,32,32,286);v.selectionScroll=RpgUI.Scroll(list,out var content);v.cards=new MenuCharacterCard[m.saveSlots.Length];
            for(int i=0;i<m.saveSlots.Length;i++)v.cards[i]=BuildCard(m.saveSlots[i],v,content,i);
            var create=Safe(m.characterCreatePanel.transform,"Phase8Creation");Header(create,"Khởi Đầu Đạo Lộ");var portrait=PortraitImage(create);RpgUI.Top(portrait.rectTransform,176,440,160,160);
            Top(create,"Instruction","Đặt đạo hiệu cho nhân vật của bạn",650,88,38,T.gold);
            m.nameInput.transform.SetParent(create,false);m.nameInput.gameObject.SetActive(true);RpgUI.Top((RectTransform)m.nameInput.transform,774,144,64,64);var inputImage=m.nameInput.GetComponent<UnityEngine.UI.Image>();inputImage.sprite=T.surfaceSprite;inputImage.type=UnityEngine.UI.Image.Type.Sliced;inputImage.color=T.surface;
            foreach(var o in m.nameInput.GetComponents<UnityEngine.UI.Outline>())o.enabled=false;m.nameInput.customCaretColor=true;m.nameInput.caretColor=T.gold;m.nameInput.selectionColor=new Color(T.jade.r,T.jade.g,T.jade.b,.3f);
            foreach(var text in m.nameInput.GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.font=UiTextStyle.BodyFont;text.fontSize=36;text.color=text==m.nameInput.textComponent?T.ivory:T.muted;RpgUI.Fill(text.rectTransform,28,20,28,20);}
            v.legacyCreationHint=(UnityEngine.UI.Text)new SerializedObject(m).FindProperty("characterNameLabel").objectReferenceValue;
            v.creationHint=Top(create,"Validation","",944,144,30,T.danger,64,64);if(v.legacyCreationHint!=null)v.legacyCreationHint.enabled=false;
            Button(m.confirmCreateButton,create,"Bắt đầu tu tiên",true);RpgUI.Bottom((RectTransform)m.confirmCreateButton.transform,208,132,64,64);Button(m.cancelCreateButton,create,"Trở lại chọn nhân vật");RpgUI.Bottom((RectTransform)m.cancelCreateButton.transform,52,132,64,64);
            var settings=Safe(m.settingsPanel.transform,"Phase8Settings");Header(settings,"Cài Đặt Âm Thanh",m.closeSettingsButton);Top(settings,"Hint","Điều chỉnh âm lượng cho hành trình tu tiên.",176,112,30,T.muted);
            var sound=RpgUI.Rect("AudioControls",settings);RpgUI.Top(sound,340,520,32,32);Stack(sound,0,24);Audio(sound,m.bgmSlider,"Nhạc nền");Audio(sound,m.sfxSlider,"Hiệu ứng");Top(settings,"Persistence","Thay đổi âm lượng được lưu tự động.",910,88,30,T.muted);
            BuildDialog(m.confirmationPopup,m.popupTitle,m.popupMessage,m.popupConfirmBtn,m.popupCancelBtn,v);
        }
        static MenuCharacterCard BuildCard(SaveSlotUI slot,MenuPresentation menu,Transform parent,int index)
        {
            Hide(slot.transform);slot.transform.SetParent(parent,false);slot.gameObject.SetActive(true);var image=slot.GetComponent<UnityEngine.UI.Image>();image.sprite=T.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=T.surface;foreach(var o in slot.GetComponents<UnityEngine.UI.Outline>())o.enabled=false;RpgUI.Height(slot.transform,480);
            var legacy=RpgUI.Rect("LegacyBindings",slot.transform);slot.dataPanel.transform.SetParent(legacy,false);slot.emptyPanel.transform.SetParent(legacy,false);legacy.gameObject.SetActive(false);
            var c=slot.gameObject.AddComponent<MenuCharacterCard>();c.theme=T;c.menu=menu;c.native=slot;c.index=index;
            c.portrait=PortraitImage(slot.transform);RpgUI.Fill(c.portrait.rectTransform,12,196,0,20);c.portrait.rectTransform.anchorMax=new Vector2(.29f,1);
            c.details=RpgUI.Rect("Identity",slot.transform);RpgUI.Fill(c.details,24,204,24,20);c.details.anchorMin=new Vector2(.29f,0);Stack(c.details,0,8);
            c.slot=Flow(c.details,"Slot","",26,T.gold);c.nameLabel=Flow(c.details,"CharacterName","",38);c.nameLabel.font=T.headingFont;c.realm=Flow(c.details,"Realm","",28,T.jade);c.history=Flow(c.details,"History","",27,T.muted);
            c.state=F.Text("SelectionState",slot.transform,"",26);c.state.color=T.gold;RpgUI.Bottom(c.state.rectTransform,164,40,24,24);
            var actions=EquipmentRedesignBuilder.Row(slot.transform,"Actions",132);RpgUI.Bottom(actions,16,132,16,16);c.enter=F.Action("Continue",actions,"Tiếp tục đạo lộ",true);Button(slot.deleteButton,actions,"Xóa");var le=slot.deleteButton.GetComponent<UnityEngine.UI.LayoutElement>()??slot.deleteButton.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minWidth=le.preferredWidth=180;le.flexibleWidth=0;
            var edge=RpgUI.Rect("SelectedAccent",slot.transform);edge.anchorMin=Vector2.zero;edge.anchorMax=new Vector2(0,1);edge.pivot=new Vector2(0,.5f);edge.sizeDelta=new Vector2(5,0);edge.anchoredPosition=Vector2.zero;c.accent=F.Surface(edge,T.raised);
            return c;
        }
        static void BuildPause(PauseMenuManager p)
        {
            Root(p.transform);Root(p.pauseModal.transform);Hide(p.pauseModal.transform);var oldImage=p.pauseModal.GetComponent<UnityEngine.UI.Image>();if(oldImage!=null)oldImage.color=Color.clear;foreach(var o in p.pauseModal.GetComponents<UnityEngine.UI.Outline>())o.enabled=false;
            var safe=Safe(p.pauseModal.transform,"Phase8Pause");var v=p.gameObject.AddComponent<MenuPauseView>();v.owner=p;var header=Header(safe,"Menu Trong Trận",p.closeHeaderButton);v.title=header.Find("Title").GetComponent<TMP_Text>();Top(safe,"RealtimeNotice","Trận đấu vẫn tiếp diễn khi mở menu.",176,80,30,T.gold);
            var pages=RpgUI.Rect("Pages",safe);RpgUI.Fill(pages,32,216,32,288);v.mainPage=RpgUI.Rect("Overview",pages);RpgUI.Fill(v.mainPage);v.mainScroll=RpgUI.Scroll(v.mainPage,out var content);v.character=Flow(content,"CharacterContext","",32);v.status=Flow(content,"SaveStatus","",28,T.jade);
            Button(p.manualSaveButton,content,"Lưu tiến trình");v.audio=F.Action("AudioSettings",content,"Âm thanh & cài đặt");RpgUI.Height(v.audio.transform,132);Button(p.returnMainMenuButton,content,"Lưu & về màn hình chính");Button(p.quitButton,content,"Lưu & thoát game");
            v.settingsPage=RpgUI.Rect("AudioSettingsPage",pages);RpgUI.Fill(v.settingsPage);v.settingsScroll=RpgUI.Scroll(v.settingsPage,out var audioContent);Audio(audioContent,p.bgmSlider,"Nhạc nền");Audio(audioContent,p.sfxSlider,"Hiệu ứng");
            var toggle=p.muteToggle;toggle.transform.SetParent(audioContent,false);toggle.gameObject.SetActive(true);RpgUI.Height(toggle.transform,132);var toggleImage=toggle.GetComponent<UnityEngine.UI.Image>();if(toggleImage==null)toggleImage=toggle.gameObject.AddComponent<UnityEngine.UI.Image>();toggleImage.sprite=T.surfaceSprite;toggleImage.type=UnityEngine.UI.Image.Type.Sliced;toggleImage.color=T.surface;
            foreach(var text in toggle.GetComponentsInChildren<UnityEngine.UI.Text>(true))text.enabled=false;
            var bg=toggle.transform.Find("Background") as RectTransform;if(bg!=null){bg.anchorMin=bg.anchorMax=new Vector2(0,.5f);bg.pivot=new Vector2(.5f,.5f);bg.anchoredPosition=new Vector2(56,0);bg.sizeDelta=new Vector2(64,64);var img=bg.GetComponent<UnityEngine.UI.Image>();img.sprite=T.surfaceSprite;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=T.raised;toggle.targetGraphic=img;}
            if(toggle.graphic!=null){RpgUI.Fill(toggle.graphic.rectTransform,12,12,12,12);toggle.graphic.color=T.gold;}
            var muteLabel=F.Text("MuteCaption",toggle.transform,"Tắt toàn bộ âm thanh",32);RpgUI.Fill(muteLabel.rectTransform,112,12,16,12);v.muteState=Flow(audioContent,"MuteState","",28,T.gold);Flow(audioContent,"AudioHint","Âm lượng vẫn được lưu khi tắt tiếng.",28,T.muted);v.back=F.Action("BackToPause",audioContent,"Trở lại menu");RpgUI.Height(v.back.transform,132);
            Button(p.resumeButton,safe,"Trở lại trận đấu",true);RpgUI.Bottom((RectTransform)p.resumeButton.transform,36,132,32,32);v.settingsPage.gameObject.SetActive(false);
            BuildDialog(p.confirmationPopup,p.txtPopupTitle,p.txtPopupMessage,p.btnPopupConfirm,p.btnPopupCancel,null);
            p.gameObject.SetActive(false);
        }
        static void BuildDialog(GameObject root,UnityEngine.UI.Text title,UnityEngine.UI.Text message,UnityEngine.UI.Button confirm,UnityEngine.UI.Button cancel,MenuPresentation menu)
        {
            Root(root.transform);Hide(root.transform);var safe=Safe(root.transform,"Phase8Dialog",false);var bg=safe.GetComponent<UnityEngine.UI.Image>();bg.color=new Color(.015f,.04f,.04f,.96f);
            var box=RpgUI.Rect("ConfirmDialog",safe);RpgUI.Fill(box,44,0,44,0);box.anchorMin=new Vector2(0,.16f);box.anchorMax=new Vector2(1,.84f);F.Surface(box,T.surface,true);
            var v=root.AddComponent<MenuDialogView>();v.theme=T;v.sourceTitle=title;v.sourceMessage=message;v.menu=menu;v.confirm=confirm;
            v.severity=Top(box,"Severity","",28,88,28,T.gold);var list=RpgUI.Rect("MessageScroll",box);RpgUI.Fill(list,32,204,32,132);var scroll=RpgUI.Scroll(list,out var content);v.title=Flow(content,"Title","",44);v.title.font=T.headingFont;v.message=Flow(content,"Message","",32);
            var buttons=EquipmentRedesignBuilder.Row(box,"Actions",132);RpgUI.Bottom(buttons,28,132,28,28);Button(cancel,buttons,"Hủy");v.confirmCaption=Button(confirm,buttons,"Xác nhận",true);root.SetActive(false);
        }
        static FoundationValue Value(Transform parent,string caption,bool stacked)
        {
            var v=F.Value("StatRow_"+caption,parent);RpgUI.Height(v.transform,stacked?160:132);v.label.text=caption;v.label.color=T.muted;v.label.fontSize=28;v.value.fontSize=32;v.value.color=T.ivory;
            if(stacked){RpgUI.Top(v.label.rectTransform,12,40,24,24);RpgUI.Top(v.value.rectTransform,56,80,24,24);v.value.alignment=TextAlignmentOptions.MidlineLeft;}
            else{RpgUI.Fill(v.label.rectTransform,24,16,12,16);v.label.rectTransform.anchorMax=new Vector2(.38f,1);RpgUI.Fill(v.value.rectTransform,12,16,24,16);v.value.rectTransform.anchorMin=new Vector2(.4f,0);v.value.alignment=TextAlignmentOptions.MidlineRight;}
            var fit=v.gameObject.AddComponent<MenuValueLayout>();fit.row=v;fit.stacked=stacked;return v;
        }
        static void BuildInfo(CharacterDetailModalUI c)
        {
            Root(c.transform);Hide(c.transform);var v=c.gameObject.AddComponent<MenuCharacterInfoView>();v.owner=c;var safe=Safe(c.transform,"Phase8CharacterInfo");c.modalBox=safe;
            Header(safe,"Thông Tin Nhân Vật");v.close=F.Action("Close",safe,"Đóng");var close=(RectTransform)v.close.transform;close.anchorMin=close.anchorMax=close.pivot=new Vector2(1,1);close.anchoredPosition=new Vector2(-32,-20);close.sizeDelta=new Vector2(168,132);
            var list=RpgUI.Rect("CharacterStats",safe);RpgUI.Fill(list,32,32,32,180);v.scroll=RpgUI.Scroll(list,out var content);c.scrollRect=v.scroll;
            var hero=RpgUI.Rect("Identity",content);v.identity=hero;RpgUI.Height(hero,320);var portrait=PortraitImage(hero);RpgUI.Fill(portrait.rectTransform,0,0,0,0);portrait.rectTransform.anchorMax=new Vector2(.28f,1);
            var names=RpgUI.Rect("Names",hero);RpgUI.Fill(names,24,0,0,0);names.anchorMin=new Vector2(.3f,0);Stack(names);v.title=Flow(names,"Name","",44);v.title.font=T.headingFont;v.realm=Flow(names,"Realm","",32,T.gold);
            v.sources=new[]{c.txtTuVi,c.txtLinhThach,c.txtCoDuyen,c.txtHp,c.txtMp,c.txtMultiplier,c.txtEquipSwords,c.txtUpgradeKiemY,c.txtUpgradeLinhLuc,c.txtUpgradeTuLinh,c.txtUpgradeThanThuc};v.captions=new[]{"Tu vi","Linh thạch","Cơ Duyên","Sinh lực","Linh lực","Khí tức tiên thiên","Phi kiếm xuất trận","Kiếm Ý","Linh Lực · Công pháp","Tụ Linh","Thần Thức"};v.rows=v.captions.Select((name,i)=>Value(content,name,i<3)).ToArray();
        }
    }
}
#endif

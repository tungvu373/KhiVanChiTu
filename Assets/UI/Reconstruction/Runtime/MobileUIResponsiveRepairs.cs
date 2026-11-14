using UnityEngine;

/// <summary>Scene-scoped corrections verified by Phase 09 runtime geometry and screenshots. No transactions or event rebinding.</summary>
public static class MobileUIResponsiveRepairs
{
    static void Top(Transform t,float x,float y,float w,float h,float anchor=0)
    {
        if(t==null)return;var r=(RectTransform)t;r.anchorMin=r.anchorMax=r.pivot=new Vector2(anchor,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
    }
    static void Bottom(Transform t,float x,float y,float w,float h,float anchor=.5f)
    {
        if(t==null)return;var r=(RectTransform)t;r.anchorMin=r.anchorMax=r.pivot=new Vector2(anchor,0);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);
    }
    static void Fill(Transform t,float left,float bottom,float right,float top)
    {if(t==null)return;var r=(RectTransform)t;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
    static void TopSpan(Transform t,float left,float top,float right,float height,float span=1)
    {if(t==null)return;var r=(RectTransform)t;r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(span,1);r.pivot=new Vector2(.5f,1);r.offsetMin=new Vector2(left,-top-height);r.offsetMax=new Vector2(-right,-top);}
    static void BottomSpan(Transform t,float left,float bottom,float right,float height,float span=1)
    {if(t==null)return;var r=(RectTransform)t;r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(span,0);r.pivot=new Vector2(.5f,0);r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,bottom+height);}
    static void Font(Transform t,int size)
    {if(t==null)return;var text=t.GetComponent<UnityEngine.UI.Text>();if(text==null)return;text.fontSize=size;text.resizeTextForBestFit=false;text.verticalOverflow=VerticalWrapMode.Truncate;text.raycastTarget=false;}
    static void Label(Transform button,int size)
    {if(button==null)return;foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true)){Font(text.transform,size);Fill(text.transform,6,6,6,6);text.alignment=TextAnchor.MiddleCenter;text.color=JadeUITheme.Ivory;}}
    static void Fit(RectTransform box,float width,float height,float scale)
    {
        box.anchorMin=box.anchorMax=box.pivot=new Vector2(.5f,.5f);box.anchoredPosition=Vector2.zero;box.sizeDelta=new Vector2(width,height);
        var fit=box.GetComponent<JadeModalFit>();if(fit==null)fit=box.gameObject.AddComponent<JadeModalFit>();fit.preferredScale=scale;fit.Fit();
    }
    public static void ApplyTree(MobileUIReconstruction owner,Transform scope)
    {
        if(!Application.isPlaying||owner==null||scope==null||scope.gameObject.scene!=owner.gameObject.scene)return;
        foreach(var density in scope.GetComponentsInChildren<MobileUITextDensity>(true))density.Restore();
        foreach(var rt in scope.GetComponentsInChildren<RectTransform>(true))
        {
            string n=rt.name, parent=rt.parent!=null?rt.parent.name:"";
            if(n=="TutorialPointerArrow")
            {var visibility=rt.GetComponent<MobileUITutorialOcclusion>();if(visibility==null)visibility=rt.gameObject.AddComponent<MobileUITutorialOcclusion>();visibility.owner=owner;}
            if(n.StartsWith("Line_")&&rt.GetComponentInParent<BodyTemperingPanelUI>()!=null)
            {var line=rt.GetComponent<UnityEngine.UI.Image>();if(line!=null)line.raycastTarget=false;}
            if(n=="SecretRealmPlayRoot")Trial(rt);
            if(n=="Card"&&parent=="SecretRealmResultModal")Result(rt);
            if(n=="Box"&&parent=="ConfirmationPopup")Decision(rt);
            if(n=="ModalBox"&&parent=="BodyTalentDetailModal")Body(rt);
            if(n=="ModalBox"&&parent=="CharacterDetailModal")Character(rt);
            if(n=="ModalBox"&&parent=="DismantleConfirmModalUI")Dismantle(rt);
            if(n=="ModalBox"&&parent=="TribulationModalUI")
            {
                var ui=rt.GetComponentInParent<TribulationModalUI>();if(ui!=null)
                {Bottom(ui.cancelButton.transform,24,24,264,100,0);Bottom(ui.confirmButton.transform,-24,24,264,100,1);Label(ui.cancelButton.transform,24);Label(ui.confirmButton.transform,24);foreach(var label in ui.confirmButton.GetComponentsInChildren<UnityEngine.UI.Text>(true))label.color=JadeUITheme.Ivory;}
            }
            if(n=="ModalBox"&&parent=="MilestoneModalUI")Milestones(rt);
            if(n=="ModalBox"&&parent=="OfflineRewardPopupUI")
                foreach(var button in rt.GetComponentsInChildren<UnityEngine.UI.Button>(true))foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))if(text.color.grayscale<.3f)text.color=JadeUITheme.Ivory;
            if(n=="PauseBox"&&parent=="PauseMenuModal")Pause(rt);
            if(n=="ModalBox"&&parent=="AchievementModalUI")
            {
                Top(rt.Find("BtnClose"),-12,12,100,100,1);Label(rt.Find("BtnClose"),28);
                Font(rt.Find("TxtTotalProgress"),26);Bottom(rt.Find("BtnClaimAll"),0,24,600,96);Label(rt.Find("BtnClaimAll"),26);
                foreach(var text in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))if(text.fontSize<26)Font(text.transform,26);
            }
            if(n.StartsWith("Card_")&&rt.Find("BtnAction")!=null)
            {
                var le=rt.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null){le.minHeight=352;le.preferredHeight=352;}
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,352);
                Top(rt.Find("IconBadge"),96,40,72,72);Label(rt.Find("IconBadge"),20);
                TopSpan(rt.Find("Title"),192,32,48,80);Font(rt.Find("Title"),28);
                TopSpan(rt.Find("Desc"),32,116,32,76);Font(rt.Find("Desc"),26);
                TopSpan(rt.Find("ProgBG"),32,208,20,36,.6f);
                BottomSpan(rt.Find("Reward"),96,24,20,76,.6f);Font(rt.Find("Reward"),26);
                var reward=rt.Find("Reward")?.GetComponent<UnityEngine.UI.Text>();if(reward!=null){reward.resizeTextForBestFit=true;reward.resizeTextMinSize=22;reward.resizeTextMaxSize=26;}
                Bottom(rt.Find("BtnAction"),-32,24,210,96,1);Label(rt.Find("BtnAction"),26);
            }
            if(n.StartsWith("QuestRow_")&&rt.Find("BtnAction")!=null)MilestoneRow(rt);
            if(n=="ModalBox"&&(parent=="DaoAnModal"||parent=="SecretRealmModal"))
            {
                foreach(var text in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))if(text.name!="SubTitle"&&text.fontSize<38)Font(text.transform,38);
                Font(rt.Find("SubTitle"),36);
                var close=rt.Find("BtnCloseX") as RectTransform;if(close!=null){close.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,144);close.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,144);}
                var footer=rt.Find("CloseButton") as RectTransform;if(footer!=null)footer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,144);
                foreach(var button in rt.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                    if(button.name.StartsWith("Tab_")||button.name=="BtnBuy")
                    {var r=(RectTransform)button.transform;r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,144);var le=r.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null){le.minHeight=144;le.preferredHeight=144;}}
            }
            if(n.StartsWith("Card_")&&rt.Find("InfoBox")!=null)
            {
                var le=rt.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null){le.minHeight=440;le.preferredHeight=440;}
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,440);var info=rt.Find("InfoBox");Fill(info,144,160,48,40);
                Top(rt.Find("IconBox"),40,40,96,96);
                TopSpan(info.Find("Name"),0,0,0,104);Font(info.Find("Name"),38);TopSpan(info.Find("Desc"),0,112,0,128);Font(info.Find("Desc"),36);
                foreach(string name in new[]{"Effect","LockReason"}){Bottom(info.Find(name),0,-142,500,136,0);Font(info.Find(name),36);}
            }
            if(n.StartsWith("Card_")&&rt.Find("BtnEnter")!=null)
            {
                var le=rt.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null){le.minHeight=472;le.preferredHeight=472;}
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,472);
                TopSpan(rt.Find("Title"),56,40,56,104);Font(rt.Find("Title"),38);
                TopSpan(rt.Find("Desc"),56,148,56,128);Font(rt.Find("Desc"),36);
                Bottom(rt.Find("Reward"),56,32,500,144,0);Font(rt.Find("Reward"),36);
                Bottom(rt.Find("BtnEnter"),-32,32,300,144,1);
            }
            if(n=="RuneSeal"&&parent=="AncientStele"&&owner.assets!=null&&owner.assets.formation!=null)
            {var rune=rt.GetComponent<UnityEngine.UI.Image>();if(rune!=null){rune.sprite=owner.assets.formation;rune.preserveAspect=true;rune.raycastTarget=false;}}
        }
        foreach(var body in scope.GetComponentsInChildren<BodyTemperingPanelUI>(true))
            if(body.connectorLines!=null)foreach(var line in body.connectorLines)if(line!=null)line.raycastTarget=false;
        foreach(var rt in scope.GetComponentsInChildren<RectTransform>(true))
            if(rt.name=="SecretRealmPlayRoot"||(rt.name=="Card"&&rt.parent!=null&&rt.parent.name=="SecretRealmResultModal"))
                foreach(var text in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if(text.name=="FText")continue;
                    var density=text.GetComponent<MobileUITextDensity>();if(density==null)density=text.gameObject.AddComponent<MobileUITextDensity>();density.Apply();
                }
        MobileUIContentSafety.Apply(scope);
    }
    static void Decision(RectTransform box)
    {
        var confirm=box.Find("BtnConfirm");var cancel=box.Find("BtnCancel");if(confirm==null||cancel==null)return;
        Fit(box,936,672,1);
        var title=box.Find("Title");Top(title,48,36,840,96);Font(title,48);
        if(title!=null){var text=title.GetComponent<UnityEngine.UI.Text>();if(text!=null)text.color=JadeUITheme.Gold;}
        var message=box.Find("Message");Fill(message,72,264,72,156);Font(message,42);
        if(message!=null){var text=message.GetComponent<UnityEngine.UI.Text>();if(text!=null)text.color=JadeUITheme.Ivory;}
        Bottom(cancel,60,72,378,144,0);Bottom(confirm,-60,72,378,144,1);Label(cancel,42);Label(confirm,42);
    }
    static void Result(RectTransform box)
    {
        var outline=box.GetComponent<UnityEngine.UI.Outline>();if(outline!=null)outline.enabled=false;
        Fit(box,340,440,984f/340);Top(box.Find("TitleText"),20,14,300,48);Font(box.Find("TitleText"),16);
        Top(box.Find("ScoreText"),20,78,300,48);Font(box.Find("ScoreText"),14);
        var reward=box.Find("RewardBox") as RectTransform;Fill(reward,20,108,20,146);Fill(reward!=null?reward.Find("RewardDetailsText"):null,12,12,12,12);Font(reward!=null?reward.Find("RewardDetailsText"):null,14);
        Bottom(box.Find("BtnConfirm"),0,26,292,52);Label(box.Find("BtnConfirm"),15);
    }
    static void Trial(RectTransform root)
    {
        var viewport=root.GetComponent<MobileUILogicalViewport>();if(viewport==null)viewport=root.gameObject.AddComponent<MobileUILogicalViewport>();viewport.Refresh();
        var header=root.Find("HeaderPanel") as RectTransform;if(header==null)return;
        header.anchorMin=new Vector2(0,1);header.anchorMax=Vector2.one;header.pivot=new Vector2(.5f,1);header.sizeDelta=new Vector2(0,168);header.anchoredPosition=Vector2.zero;
        Top(header.Find("BtnQuit"),8,8,76,48);Label(header.Find("BtnQuit"),14);
        Top(header.Find("TitleText"),92,8,166,48);Font(header.Find("TitleText"),14);
        Top(header.Find("TimerText"),-8,8,84,48,1);Font(header.Find("TimerText"),14);
        var rule=header.Find("RuleCard");Top(rule,8,64,344,64);Fill(rule!=null?rule.Find("TxtRule"):null,6,4,6,4);Font(rule!=null?rule.Find("TxtRule"):null,12);
        if(rule!=null)
        {
            var text=rule.Find("TxtRule").GetComponent<UnityEngine.UI.Text>();var content=text.rectTransform;
            Top(content,6,4,332,Mathf.Max(56,text.preferredHeight+8));
            var scroll=rule.GetComponent<UnityEngine.UI.ScrollRect>();if(scroll==null)scroll=rule.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            if(rule.GetComponent<UnityEngine.UI.RectMask2D>()==null)rule.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            scroll.content=content;scroll.viewport=(RectTransform)rule;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=20;
        }
        Top(header.Find("ScoreTitle"),8,136,190,24);Font(header.Find("ScoreTitle"),12);Top(header.Find("ScoreValue"),-8,136,150,24,1);Font(header.Find("ScoreValue"),14);
        Fill(root.Find("ArenaContainer"),0,140,0,168);
        var footer=root.Find("FooterPanel") as RectTransform;if(footer!=null){footer.anchorMin=Vector2.zero;footer.anchorMax=new Vector2(1,0);footer.pivot=new Vector2(.5f,0);footer.sizeDelta=new Vector2(0,140);footer.anchoredPosition=Vector2.zero;Bottom(footer.Find("MainProgressBarBg"),0,88,336,28);Font(footer.Find("MainProgressBarBg/TxtProgress"),12);Bottom(footer.Find("MainActionBtn"),0,20,330,48);Label(footer.Find("MainActionBtn"),14);}
    }
    static void Body(RectTransform box)
    {
        Fit(box,936,1464,1);Top(box.Find("BtnClose"),-12,12,132,132,1);Label(box.Find("BtnClose"),40);
        string[] names={"TxtModalTitle","TxtModalTier","TxtModalLore","TxtModalMechanics","TxtModalCurrent","TxtModalNext"};
        float[] y={36,164,248,420,780,936},h={112,72,156,336,132,132};
        for(int i=0;i<names.Length;i++){Top(box.Find(names[i]),60,y[i],i==0?672:816,h[i]);Font(box.Find(names[i]),42);}
        var body=box.GetComponentInParent<BodyTemperingPanelUI>();
        if(body==null)body=ownerBody(box);
        if(body!=null)
        {
            if(body.txtModalTitle!=null)body.txtModalTitle.text=System.Text.RegularExpressions.Regex.Replace(body.txtModalTitle.text,@"</?size(?:=\d+)?>","");
            if(body.txtModalMechanics!=null)
            {
                var text=body.txtModalMechanics;var panel=text.transform.parent as RectTransform;
                Top(panel,60,420,816,336);Font(text.transform,38);Top(text.transform,24,18,768,Mathf.Max(300,text.preferredHeight+12));
                var scroll=panel.GetComponent<UnityEngine.UI.ScrollRect>();if(scroll==null)scroll=panel.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();if(panel.GetComponent<UnityEngine.UI.RectMask2D>()==null)panel.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();scroll.content=text.rectTransform;scroll.viewport=panel;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            }
            var values=new[]{body.txtModalCurrent,body.txtModalNext};
            bool shared=values[0]!=null&&values[1]!=null&&values[0].transform.parent==values[1].transform.parent;
            if(shared)
            {Top(values[0].transform.parent,60,780,816,288);for(int i=0;i<values.Length;i++){Top(values[i].transform,24,18+i*156,768,108);Font(values[i].transform,38);}}
            else for(int i=0;i<values.Length;i++)if(values[i]!=null){Top(values[i].transform.parent,60,780+i*156,816,132);Fill(values[i].transform,24,18,24,18);Font(values[i].transform,38);}
            Bottom(body.btnModalUpgrade!=null?body.btnModalUpgrade.transform:null,0,236,816,144);Label(body.btnModalUpgrade!=null?body.btnModalUpgrade.transform:null,42);
            Bottom(body.btnModalUpgrade5!=null?body.btnModalUpgrade5.transform:null,60,72,390,144,0);Label(body.btnModalUpgrade5!=null?body.btnModalUpgrade5.transform:null,38);
            Bottom(body.btnModalUpgradeMax!=null?body.btnModalUpgradeMax.transform:null,-60,72,390,144,1);
            var action=body.btnModalUpgradeMax!=null?body.btnModalUpgradeMax.GetComponent<MobileUIActionView>():null;
            if(action!=null)
            {var caption=action.caption.rectTransform;caption.anchorMin=new Vector2(0,.42f);caption.anchorMax=new Vector2(1,.96f);caption.offsetMin=new Vector2(12,0);caption.offsetMax=new Vector2(-12,0);Font(caption,36);var value=action.value.rectTransform;value.anchorMin=new Vector2(0,.06f);value.anchorMax=new Vector2(1,.4f);value.offsetMin=new Vector2(12,0);value.offsetMax=new Vector2(-12,0);Font(value,30);}
        }
    }
    static BodyTemperingPanelUI ownerBody(RectTransform box)
    {var root=box.parent!=null?box.parent.parent:null;return root!=null&&root.Find("Drawer_1")!=null?root.Find("Drawer_1").GetComponent<BodyTemperingPanelUI>():null;}
    static void Character(RectTransform box)
    {
        Fit(box,540,860,1.8f);var header=box.Find("HeaderBar");Top(header,0,0,540,116);Top(header!=null?header.Find("TitleText"):null,32,26,400,56);Top(header!=null?header.Find("CloseButton"):null,-20,20,76,76,1);Label(header!=null?header.Find("CloseButton"):null,24);
        Fill(box.Find("DetailScrollView"),32,116,32,136);var content=box.Find("DetailScrollView/Viewport/Content") as RectTransform;
        if(content!=null)
        {
            var layout=content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if(layout!=null){layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;layout.spacing=8;}
            foreach(Transform child in content)
            {var t=child.GetComponent<UnityEngine.UI.Text>();if(t==null)continue;Font(t.transform,24);var le=t.GetComponent<UnityEngine.UI.LayoutElement>();if(le==null)le=t.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.minHeight=48;le.preferredHeight=Mathf.Max(48,t.preferredHeight+8);le.flexibleHeight=0;}
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(content);
        }
        Bottom(box.Find("BottomCloseButton"),0,24,300,76);Label(box.Find("BottomCloseButton"),24);
    }
    static void Dismantle(RectTransform box)
    {
        Fit(box,560,580,984f/560);Top(box.Find("TxtTitle"),24,20,512,56);Font(box.Find("TxtTitle"),28);Top(box.Find("TxtItemName"),24,84,512,48);Font(box.Find("TxtItemName"),24);Top(box.Find("TxtItemDetail"),24,140,512,64);Font(box.Find("TxtItemDetail"),24);
        Fill(box.Find("RewardBox"),24,184,24,216);Fill(box.Find("RewardBox/TxtReward"),12,12,12,12);Font(box.Find("RewardBox/TxtReward"),24);
        Fill(box.Find("RewardBox"),24,216,24,216);Bottom(box.Find("TxtWarning"),0,112,512,96);Font(box.Find("TxtWarning"),24);Bottom(box.Find("BtnCancel"),24,24,240,80,0);Bottom(box.Find("BtnConfirm"),-24,24,240,80,1);Label(box.Find("BtnCancel"),24);Label(box.Find("BtnConfirm"),24);
    }
    static void Milestones(RectTransform box)
    {
        Fit(box,640,1050,984f/640);Top(box.Find("TxtHeaderTitle"),24,20,500,64);Font(box.Find("TxtHeaderTitle"),30);Top(box.Find("TxtTotalProgress"),24,92,560,48);Font(box.Find("TxtTotalProgress"),26);Top(box.Find("BtnClose"),-12,12,100,100,1);Label(box.Find("BtnClose"),26);Fill(box.Find("ScrollView"),24,152,24,160);Bottom(box.Find("BtnClaimAll"),0,24,576,96);Label(box.Find("BtnClaimAll"),26);
    }
    static void MilestoneRow(RectTransform row)
    {
        var le=row.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null){le.minHeight=252;le.preferredHeight=252;}row.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,252);
        Top(row.Find("TxtTitle"),20,12,540,52);Font(row.Find("TxtTitle"),28);Top(row.Find("TxtDesc"),20,72,540,76);Font(row.Find("TxtDesc"),24);Bottom(row.Find("ProgBG"),20,28,260,48,0);Font(row.Find("ProgBG/ProgTxt"),24);Bottom(row.Find("BtnAction"),-16,16,220,96,1);Label(row.Find("BtnAction"),26);
    }
    static void Pause(RectTransform box)
    {
        Fit(box,540,1040,1.8f);var header=box.Find("HeaderBar");Top(header!=null?header.Find("CloseHeaderBtn"):null,-8,8,76,76,1);
        var save=box.Find("SaveInfoCard");Top(save,20,96,500,200);
        Top(save!=null?save.Find("TxtCharacterInfo"):null,96,24,308,112);Font(save!=null?save.Find("TxtCharacterInfo"):null,22);
        Top(save!=null?save.Find("TxtSaveStatus"):null,96,132,308,48);Font(save!=null?save.Find("TxtSaveStatus"):null,18);
        var audio=box.Find("AudioSection");Top(audio,20,312,500,316);
        if(audio!=null)
        {
            Top(audio.Find("TitleAudio"),96,24,308,40);Font(audio.Find("TitleAudio"),22);
            string[] names={"BgmRow","SfxRow","MuteRow"};for(int i=0;i<3;i++)Top(audio.Find(names[i]),i==2?96:36,76+i*82,i==2?368:428,i==2?64:76);
            foreach(var text in audio.GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(text.transform,22);
        }
        if(audio!=null)foreach(var slider in audio.GetComponentsInChildren<UnityEngine.UI.Slider>(true))((RectTransform)slider.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,76);
        foreach(var text in box.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            if(text.transform.IsChildOf(audio)||text.transform.IsChildOf(save))
            {if(text.fontSize>22)Font(text.transform,22);}
            else if(text.fontSize<24)Font(text.transform,24);
    }

    public static void ApplySavedContentLayout(Transform scope)
    {
        if(scope==null)return;
        foreach(var rect in scope.GetComponentsInChildren<RectTransform>(true))
            if(rect.name=="PauseBox"&&rect.parent!=null&&rect.parent.name=="PauseMenuModal")Pause(rect);
    }
}

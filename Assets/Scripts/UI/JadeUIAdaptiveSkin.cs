using UnityEngine;

/// <summary>Presentation adapters for the existing runtime-created modal and list hierarchies.</summary>
public static class JadeUIAdaptiveSkin
{
    static T Get<T>(GameObject go) where T:Component => go.GetComponent<T>()??go.AddComponent<T>();
    static void Rect(Transform t,Vector2 min,Vector2 max,Vector2 pivot,Vector2 pos,Vector2 size)
    {if(t==null)return;var r=(RectTransform)t;r.anchorMin=min;r.anchorMax=max;r.pivot=pivot;r.sizeDelta=size;r.anchoredPosition=pos;}
    static void Top(Transform t,float x,float y,float w,float h,float anchor=0)
        =>Rect(t,new Vector2(anchor,1),new Vector2(anchor,1),new Vector2(anchor,1),new Vector2(x,-y),new Vector2(w,h));
    static void Fill(Transform t,float left,float bottom,float right,float top)
    {if(t==null)return;var r=(RectTransform)t;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
    static void Font(Transform t,int size)
    {if(t==null)return;var text=t.GetComponent<UnityEngine.UI.Text>();if(text==null)return;text.fontSize=size;text.resizeTextForBestFit=true;text.resizeTextMinSize=size-2;text.resizeTextMaxSize=size;text.raycastTarget=t.GetComponent<UnityEngine.UI.Selectable>()!=null;text.verticalOverflow=VerticalWrapMode.Truncate;}
    static void Height(RectTransform rt,float h)
    {var le=Get<UnityEngine.UI.LayoutElement>(rt.gameObject);le.minHeight=h;le.preferredHeight=h;rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,h);}
    public static void Apply(RectTransform rt,JadeUITheme theme)
    {
        string n=rt.name;
        if(n=="ModalBox"||n=="PauseBox"||n=="Box"||n=="GameplayChoicePanel")Window(rt,theme);
        if(n.StartsWith("Choice")&&rt.parent!=null&&rt.parent.name=="GameplayChoicePanel"&&rt.GetComponent<UnityEngine.UI.Button>()!=null)rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,132);
        if(n.StartsWith("TierHeader_")){Height(rt,72);foreach(var t in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,30);}
        if(n.StartsWith("Card_")&&rt.Find("InfoBox")!=null)FateCard(rt);
        else if(n.StartsWith("Card_")&&rt.Find("BtnAction")!=null)AchievementCard(rt);
        else if(n.StartsWith("Card_")&&rt.Find("BtnEnter")!=null)RealmCard(rt);
        if(n=="GridSlot")
        {
            foreach(var t in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {t.text=System.Text.RegularExpressions.Regex.Replace(t.text,@"</?size(?:=\d+)?>","");Font(t.transform,t.name=="Text"?18:20);}
            var center=rt.Find("Center");if(center!=null)Fill(center,8,30,8,12);
            var badge=rt.Find("CountBadge");Rect(badge,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-6,6),new Vector2(74,30));
            if(badge!=null)Fill(badge.Find("Text"),0,0,0,0);
            var ribbon=rt.Find("Ribbon");if(ribbon!=null){var r=(RectTransform)ribbon;r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,30);Fill(ribbon.Find("Text"),0,0,0,0);}
            var star=rt.Find("Star") as RectTransform;if(star!=null)star.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,32);
        }
        if(n=="Content"&&rt.GetComponentInParent<StorageModalUI>()!=null)StorageGrid(rt);
    }
    static void Window(RectTransform box,JadeUITheme theme)
    {
        Get<JadeModalPresentation>(box.gameObject).theme=theme;
        var surface=box.GetComponent<UnityEngine.UI.Image>();if(surface!=null){JadeUITheme.SetSurface(surface,theme.window);surface.raycastTarget=true;}
        string owner=box.parent.name;
        var fit=Get<JadeModalFit>(box.gameObject);
        fit.preferredScale=box.rect.width<=700?Mathf.Min(1.8f,984/Mathf.Max(1,box.rect.width)):1f;
        fit.Fit();
        int minFont=Mathf.CeilToInt(28/fit.preferredScale);
        foreach(var t in box.GetComponentsInChildren<UnityEngine.UI.Text>(true))if(t.fontSize<minFont)Font(t.transform,minFont);
        if(owner=="DaoAnModal"||owner=="SecretRealmModal")
        {
            Top(box.Find("Title"),60,36,Mathf.Max(100,box.rect.width-252),64);Font(box.Find("Title"),40);
            Top(box.Find("SubTitle"),60,108,Mathf.Max(100,box.rect.width-252),96);Font(box.Find("SubTitle"),36);
            Top(box.Find("BtnCloseX"),-24,24,144,144,1);
            Rect(box.Find("StatsBar"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-220),new Vector2(-96,128));
            var stats=box.Find("StatsBar");
            if(stats!=null)
            {
                Top(stats.Find("Wallet"),20,8,450,52);Top(stats.Find("Rank"),-20,8,450,52,1);
                Top(stats.Find("FreeAttempts"),-20,8,450,52,1);
                Rect(stats.Find("ActiveBuffs"),Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,8),new Vector2(-32,48));
                foreach(var t in stats.GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,28);
            }
            var filters=box.Find("FilterScrollView");
            if(filters!=null)
            {
                Rect(filters,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-364),new Vector2(-96,144));
                foreach(var b in filters.GetComponentsInChildren<UnityEngine.UI.Button>(true)){var le=Get<UnityEngine.UI.LayoutElement>(b.gameObject);le.minWidth=200;le.preferredWidth=200;le.minHeight=124;le.preferredHeight=124;foreach(var t in b.GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,28);}
            }
            Fill(box.Find("ScrollView"),48,184,48,filters!=null?524:364);
            Rect(box.Find("CloseButton"),new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,24),new Vector2(-96,132));
        }
        if(owner=="StorageModalUI")StorageWindow(box);
        if(owner=="PauseMenuModal")PauseWindow(box);
        if(owner=="AchievementModalUI")
        {
            var achievements=box.GetComponentInParent<AchievementModalUI>();
            if(achievements!=null&&achievements.fillTotalProgress!=null)
            {achievements.fillTotalProgress.type=UnityEngine.UI.Image.Type.Filled;achievements.fillTotalProgress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;achievements.fillTotalProgress.fillOrigin=0;}
            box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1050);
            Top(box.Find("TxtHeaderTitle"),24,16,540,64);Font(box.Find("TxtHeaderTitle"),28);
            Top(box.Find("TxtTotalProgress"),24,82,580,48);Font(box.Find("TxtTotalProgress"),20);
            Top(box.Find("TotalProgBG"),24,138,600,12);
            Rect(box.Find("TabRow"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-158),new Vector2(-32,96));
            Fill(box.Find("ScrollView"),18,124,18,266);
            Rect(box.Find("BtnClaimAll"),new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,20),new Vector2(-64,90));
        }
        if(owner=="OfflineRewardPopupUI")
        {
            var popup=box.GetComponentInParent<OfflineRewardPopupUI>();
            if(popup!=null)
            {
                box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,620);
                Top(popup.titleText.transform,24,20,520,60);Font(popup.titleText.transform,28);
                Top(popup.timeAwayText.transform,24,86,520,56);Font(popup.timeAwayText.transform,20);
                var rewardParent=popup.rewardDetailsText.transform.parent;
                if(rewardParent!=box){Rect(rewardParent,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-160),new Vector2(-48,300));Fill(popup.rewardDetailsText.transform,18,18,18,18);}
                else Rect(popup.rewardDetailsText.transform,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-160),new Vector2(-64,300));
                Font(popup.rewardDetailsText.transform,22);popup.rewardDetailsText.alignment=TextAnchor.UpperLeft;
                Rect(popup.claimNormalButton.transform,Vector2.zero,Vector2.zero,Vector2.zero,new Vector2(24,24),new Vector2(264,100));
                Rect(popup.claimDoubleButton.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-24,24),new Vector2(264,100));
                foreach(var b in new[]{popup.claimNormalButton,popup.claimDoubleButton})foreach(var t in b.GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,20);
                if(popup.closeButton!=null)Top(popup.closeButton.transform,-8,8,88,88,1);
            }
        }
        fit.Fit();
    }
    static void FateCard(RectTransform rt)
    {
        Height(rt,328);
        Top(rt.Find("IconBox"),20,22,100,100);
        var info=rt.Find("InfoBox");Fill(info,144,140,24,20);
        Rect(info.Find("Name"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),Vector2.zero,new Vector2(0,52));Font(info.Find("Name"),34);
        Rect(info.Find("Desc"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-58),new Vector2(0,100));Font(info.Find("Desc"),28);
        foreach(string name in new[]{"Effect","LockReason"})
        {var t=info.Find(name);Rect(t,Vector2.zero,Vector2.zero,Vector2.zero,new Vector2(0,-118),new Vector2(500,100));Font(t,28);}
        Rect(rt.Find("BtnBuy"),new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-24,20),new Vector2(320,132));
        foreach(var t in rt.Find("BtnBuy").GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,30);
    }
    static void AchievementCard(RectTransform rt)
    {
        Height(rt,220);
        Top(rt.Find("IconBadge"),14,18,56,56);
        Top(rt.Find("Title"),82,14,490,42);Font(rt.Find("Title"),22);
        Top(rt.Find("Desc"),20,64,570,56);Font(rt.Find("Desc"),20);
        Rect(rt.Find("ProgBG"),Vector2.zero,new Vector2(.6f,0),Vector2.zero,new Vector2(20,80),new Vector2(-20,32));
        Fill(rt.Find("ProgBG/ProgText"),0,0,0,0);Font(rt.Find("ProgBG/ProgText"),18);
        Rect(rt.Find("Reward"),Vector2.zero,new Vector2(.6f,0),Vector2.zero,new Vector2(20,18),new Vector2(-20,56));Font(rt.Find("Reward"),20);
        Rect(rt.Find("BtnAction"),new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-18,18),new Vector2(210,88));
        foreach(var t in rt.Find("BtnAction").GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,20);
    }
    static void RealmCard(RectTransform rt)
    {
        Height(rt,360);Top(rt.Find("Title"),24,18,860,60);Font(rt.Find("Title"),34);
        Top(rt.Find("Desc"),24,84,860,88);Font(rt.Find("Desc"),28);
        Top(rt.Find("Reward"),24,180,540,130);Font(rt.Find("Reward"),28);
        Rect(rt.Find("BtnEnter"),new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(-24,22),new Vector2(300,132));
        foreach(var t in rt.Find("BtnEnter").GetComponentsInChildren<UnityEngine.UI.Text>(true))Font(t.transform,30);
    }
    static void StorageWindow(RectTransform box)
    {
        box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1100);
        Top(box.Find("TxtTitle"),22,16,500,50);Font(box.Find("TxtTitle"),28);
        Top(box.Find("TxtSubtitle"),22,72,600,48);Font(box.Find("TxtSubtitle"),18);
        Top(box.Find("BtnClose"),-8,8,92,92,1);
        Rect(box.Find("WalletBar"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-128),new Vector2(-32,68));
        var wallet=box.Find("WalletBar");
        string[] walletNames={"TxtLinhThach","TxtKiemY","TxtOre","TxtDust"};
        if(wallet!=null)for(int i=0;i<walletNames.Length;i++)
        {
            var value=wallet.Find(walletNames[i]) as RectTransform;if(value==null)continue;
            value.anchorMin=new Vector2(i/4f,0);value.anchorMax=new Vector2((i+1)/4f,1);
            value.offsetMin=new Vector2(56*(1-i/2f),6);value.offsetMax=new Vector2(56*(1-(i+1)/2f),-6);
        }
        for(int i=0;i<5;i++)Rect(box.Find("Tab_"+i),new Vector2(i/5f,1),new Vector2((i+1)/5f,1),new Vector2(.5f,1),new Vector2(0,-204),new Vector2(-6,90));
        Rect(box.Find("PriorityFilterBar"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-306),new Vector2(-32,110));
        var filters=box.Find("PriorityFilterBar");
        if(filters!=null)
        {
            int index=0;
            foreach(var toggle in filters.GetComponentsInChildren<UnityEngine.UI.Toggle>(true))
            {
                Rect(toggle.transform,new Vector2(index/4f,0),new Vector2((index+1)/4f,1),new Vector2(.5f,.5f),Vector2.zero,new Vector2(-8,0));index++;
                var toggleRect=(RectTransform)toggle.transform;
                toggleRect.offsetMin=new Vector2(64*(1-(index-1)/2f)+4,0);toggleRect.offsetMax=new Vector2(64*(1-index/2f)-4,0);
                var background=toggle.transform.Find("Background");if(background!=null){var image=background.GetComponent<UnityEngine.UI.Image>();if(image!=null)image.raycastTarget=true;}
                var hit=Get<UnityEngine.UI.Image>(toggle.gameObject);hit.color=Color.clear;hit.raycastTarget=true;
            }
            Rect(filters.Find("BtnToggleView"),new Vector2(.75f,0),Vector2.one,new Vector2(.5f,.5f),Vector2.zero,new Vector2(-8,0));
            var view=filters.Find("BtnToggleView") as RectTransform;if(view!=null){view.offsetMin=new Vector2(-28,0);view.offsetMax=new Vector2(-68,0);}
        }
        Fill(box.Find("ScrollView"),16,132,16,430);
        Rect(box.Find("FooterBar"),Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,16),new Vector2(-32,100));
        var footer=box.Find("FooterBar");if(footer!=null)foreach(var b in footer.GetComponentsInChildren<UnityEngine.UI.Button>(true)){var r=(RectTransform)b.transform;r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,92);}
        foreach(var grid in box.GetComponentsInChildren<UnityEngine.UI.GridLayoutGroup>(true))
        {grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=4;grid.cellSize=new Vector2(145,160);grid.spacing=new Vector2(10,10);grid.childAlignment=TextAnchor.UpperCenter;}
    }
    static void StorageGrid(RectTransform rt)
    {
        var grid=rt.GetComponent<UnityEngine.UI.GridLayoutGroup>();if(grid==null)return;
        grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=4;grid.cellSize=new Vector2(145,160);grid.spacing=new Vector2(10,10);
    }
    static void PauseWindow(RectTransform box)
    {
        box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,900);
        Rect(box.Find("HeaderBar"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),Vector2.zero,new Vector2(0,80));
        Top(box.Find("HeaderBar/CloseHeaderBtn"),-4,4,74,74,1);
        Rect(box.Find("SaveInfoCard"),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-96),new Vector2(-40,96));
        var audio=box.Find("AudioSection");Rect(audio,new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-210),new Vector2(-40,270));
        if(audio!=null)
        {
            Top(audio.Find("TitleAudio"),12,8,420,32);
            string[] rows={"BgmRow","SfxRow","MuteRow"};
            for(int i=0;i<3;i++)Rect(audio.Find(rows[i]),new Vector2(0,1),Vector2.one,new Vector2(.5f,1),new Vector2(0,-48-i*74),new Vector2(-24,70));
            foreach(var slider in audio.GetComponentsInChildren<UnityEngine.UI.Slider>(true))
            {
                var r=(RectTransform)slider.transform;
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,48);
                var hit=Get<UnityEngine.UI.Image>(slider.gameObject);hit.color=Color.clear;hit.raycastTarget=true;

                var bg=slider.transform.Find("Background") as RectTransform;
                if(bg!=null){bg.anchorMin=new Vector2(0,.5f);bg.anchorMax=new Vector2(1,.5f);bg.pivot=new Vector2(.5f,.5f);bg.anchoredPosition=Vector2.zero;bg.sizeDelta=new Vector2(0,14);}

                var fa=slider.transform.Find("Fill Area") as RectTransform;
                if(fa!=null){fa.anchorMin=new Vector2(0,.5f);fa.anchorMax=new Vector2(1,.5f);fa.pivot=new Vector2(.5f,.5f);fa.anchoredPosition=Vector2.zero;fa.sizeDelta=new Vector2(-28,14);}

                var ha=slider.transform.Find("Handle Slide Area") as RectTransform;
                if(ha!=null){ha.anchorMin=new Vector2(0,.5f);ha.anchorMax=new Vector2(1,.5f);ha.pivot=new Vector2(.5f,.5f);ha.anchoredPosition=Vector2.zero;ha.sizeDelta=new Vector2(-36,0);}

                if(slider.handleRect!=null)
                {
                    slider.handleRect.anchorMin=new Vector2(slider.normalizedValue,.5f);
                    slider.handleRect.anchorMax=new Vector2(slider.normalizedValue,.5f);
                    slider.handleRect.pivot=new Vector2(.5f,.5f);
                    slider.handleRect.anchoredPosition=Vector2.zero;
                    slider.handleRect.sizeDelta=new Vector2(36,36);
                    var hImg=slider.handleRect.GetComponent<UnityEngine.UI.Image>();
                    if(hImg!=null){hImg.preserveAspect=true;hImg.color=new Color(1f,.92f,.55f,1f);}
                }
            }
            var mute=audio.Find("MuteRow");if(mute!=null){var hit=Get<UnityEngine.UI.Image>(mute.gameObject);hit.color=Color.clear;hit.raycastTarget=true;}
        }
        var actions=box.Find("ActionsArea");Rect(actions,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,20),new Vector2(-40,384));
        string[] buttons={"BtnQuit","BtnMainMenu","BtnSave","BtnResume"};
        if(actions!=null)for(int i=0;i<buttons.Length;i++)
        {
            var btnTr=actions.Find(buttons[i]);
            Rect(btnTr,Vector2.zero,new Vector2(1,0),new Vector2(.5f,0),new Vector2(0,i*96),new Vector2(-16,84));
            if(btnTr!=null)
            {
                var txt=btnTr.GetComponentInChildren<UnityEngine.UI.Text>(true);
                if(txt!=null)
                {
                    txt.alignment=TextAnchor.MiddleCenter;
                    txt.rectTransform.anchorMin=Vector2.zero;
                    txt.rectTransform.anchorMax=Vector2.one;
                    txt.rectTransform.offsetMin=new Vector2(16,8);
                    txt.rectTransform.offsetMax=new Vector2(-16,-8);
                }
            }
        }
        var title=box.Find("HeaderBar/TitleText") as RectTransform;
        if(title!=null)
        {
            title.anchorMin=Vector2.zero;title.anchorMax=Vector2.one;
            title.offsetMin=new Vector2(60,8);title.offsetMax=new Vector2(-60,-8);
            var titleTxt=title.GetComponent<UnityEngine.UI.Text>();
            if(titleTxt!=null)titleTxt.alignment=TextAnchor.MiddleCenter;
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.CombatHUD
{
    [DefaultExecutionOrder(2500), DisallowMultipleComponent]
    public sealed class CombatHudPresentation : MonoBehaviour
    {
        [Serializable] public sealed class SkillView
        {
            public UnityEngine.UI.Button button;
            public TMP_Text title, state;
            public UnityEngine.UI.Image icon, cooldown;
        }
        public PortraitHudController legacy;
        public FoundationTheme theme;
        public Sprite hudSurface;
        public Material damageMaterial;
        public bool externalNavigation;
        public MobileDrawerUIController drawer;
        public RectTransform pause, task, wave, boss, vitalRoot;
        public FoundationProgress hp, mp, waveProgress, bossHealth;
        public TMP_Text character, realmCaption, wallet, objective, waveTitle, bossTitle, timer, focusLabel;
        public FoundationProgress cultivation;
        public UnityEngine.UI.Button questClaim;
        public SkillView[] skillViews;
        public bool IsCompact => drawer != null && drawer.IsDockCollapsed;
        public Rect ClearArea { get; private set; }
        public float ClearHeightFraction { get; private set; }
        Vector2 lastSize;
        bool lastCompact, lastBoss;
        RectTransform rect;
        bool configured;
        string lastQuest;
        UnityEngine.UI.Text sourceQuest, sourceClaim, sourceStyle;
        FoundationAction claimAction;
        UnityEngine.UI.Button styleButton;
        TMP_Text styleCaption;
        public static bool Owns(UnityEngine.SceneManagement.Scene scene)
        {
            var h=PortraitHudController.Instance;
            return h!=null&&h.externalLayout&&h.gameObject.scene==scene&&h.GetComponent<CombatHudPresentation>()!=null;
        }
        public void OpenObjectives() { if(legacy.milestoneButton!=null)legacy.milestoneButton.onClick.Invoke(); }
        public void ClaimObjective() { if(questClaim!=null&&questClaim.interactable)questClaim.onClick.Invoke(); }
        void Start() { configured=true; ConfigureAppearance(); ApplyLayout(); Refresh(); }
        void LateUpdate()
        {
            if(!configured)return;
            bool bossActive=HasBoss;
            if(rect==null)rect=(RectTransform)transform;
            if(rect.rect.size!=lastSize||IsCompact!=lastCompact||bossActive!=lastBoss)ApplyLayout();
            // Drawer owns state/commands; this layer owns its presentation, including during old slide coroutines.
            LayoutNavigation();Refresh();
        }
        bool HasBoss => (StageManager.Instance!=null&&StageManager.Instance.isBossStage)||(CombatManager.Instance!=null&&CombatManager.Instance.isTribulationBossActive);
        static void Set(TMP_Text text,string value) { if(text!=null&&text.text!=value)text.text=value; }
        public void Refresh()
        {
            var cm=CultivationManager.Instance;var e=EconomyManager.Instance;var combat=CombatManager.Instance;var sm=SkillManager.Instance;var stage=StageManager.Instance;
            if(cm!=null)
            {
                Set(character,cm.CharacterName);Set(realmCaption,cm.GetDisplayRealmName());
                var data=cm.GetCurrentStageData();cultivation.Set(cm.currentTuVi,data!=null?data.requiredTuVi:0,"");
            }
            if(e!=null)Set(wallet,"LT  "+PortraitHudController.Compact(e.currentLinhThach)+"\nCD  "+PortraitHudController.Compact(e.currentCoDuyen));
            if(sourceQuest==null&&legacy.quest!=null)sourceQuest=legacy.quest.Find("TxtQuestTitle")?.GetComponent<UnityEngine.UI.Text>();
            if(sourceQuest!=null&&lastQuest!=sourceQuest.text)
            {lastQuest=sourceQuest.text;Set(objective,System.Text.RegularExpressions.Regex.Replace(lastQuest,"<[^>]*>","").Replace("\n"," "));}
            if(claimAction==null)claimAction=task.GetComponentInChildren<FoundationAction>(true);
            if(sourceClaim==null&&questClaim!=null)sourceClaim=questClaim.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if(claimAction!=null&&questClaim!=null)
            {
                claimAction.SetAvailable(questClaim.interactable);
                bool ready=sourceClaim!=null&&sourceClaim.text.ToUpperInvariant().Contains("NH\u1eacN");
                Set(claimAction.label,ready?"Nh\u1eadn":"M\u1ed1c");
                claimAction.button.targetGraphic.color=hudSurface!=null?Color.white:ready?theme.gold:theme.raised;
                claimAction.label.color=hudSurface!=null?(ready?theme.gold:theme.ivory):ready?theme.ink:theme.ivory;
            }
            if(combat!=null)hp.Set(combat.playerCurrentHp,combat.playerMaxHp,"HP  "+PortraitHudController.Compact(combat.playerCurrentHp)+" / "+PortraitHudController.Compact(combat.playerMaxHp));
            if(sm!=null)mp.Set(sm.currentLinhLuc,sm.maxLinhLuc,"MP  "+Mathf.FloorToInt(sm.currentLinhLuc)+" / "+Mathf.FloorToInt(sm.maxLinhLuc));
            if(stage!=null)
            {
                Set(waveTitle,"T\u1ea7ng "+stage.currentStage+"  \u00b7  "+stage.enemiesKilled+" / "+stage.enemiesToBoss);
                waveProgress.Set(stage.enemiesKilled,stage.enemiesToBoss,"");
                Set(timer,stage.isBossStage?Mathf.Max(0,Mathf.CeilToInt(stage.bossTimer))+"s":"");
            }
            if(combat!=null&&HasBoss)
            {
                var target=combat.CurrentBoss;
                string bossName=combat.isTribulationBossActive?"THI\u00caN KI\u1ebeP":"BOSS \u00b7 T\u1ea6NG "+(stage!=null?stage.currentStage:0);
                Set(bossTitle,bossName+(target!=null?"  \u00b7  "+Mathf.CeilToInt(Mathf.Clamp01(target.hp/Mathf.Max(1,target.maxHp))*100)+"%":""));
                bossHealth.Set(target!=null?target.hp:0,target!=null?target.maxHp:0,target!=null?Mathf.CeilToInt(Mathf.Clamp01(target.hp/Mathf.Max(1,target.maxHp))*100)+"%":"\u0110ang xu\u1ea5t hi\u1ec7n");
            }
            Set(focusLabel,IsCompact?"\u0110\u1ee7":"G\u1ecdn");
            if(sm!=null)for(int i=0;i<skillViews.Length;i++)RefreshSkill(sm,i);
            hp.fill.GetComponent<UnityEngine.UI.Image>().color=combat!=null&&combat.playerCurrentHp<.25f*combat.playerMaxHp?new Color32(176,48,60,255):new Color32(133,57,64,255);
            RefreshNavigationStyle();RefreshSwordStyle();
        }
        void RefreshSkill(SkillManager sm,int index)
        {
            var view=skillViews[index];int normal=index-1;
            var data=index==0?sm.activeSkill:normal<sm.normalSkills.Length?sm.normalSkills[normal]:null;
            bool unlocked=data!=null&&data.isUnlocked;
            float cd=index==0?sm.GetActiveSkillCooldown():sm.GetNormalSkillCooldown(normal);
            float duration=data!=null?Mathf.Max(.01f,data.baseCooldown):1;
            float cost=index==0?sm.maxLinhLuc:sm.GetSkillManaCost(normal);
            string state=!unlocked?"Kh\u00f3a":cd>0?cd.ToString("0.0")+"s":sm.currentLinhLuc<cost?"H\u1ebft MP":index==0&&sm.isSkillActive?"Xu\u1ea5t tr\u1eadn":view.button.interactable?Mathf.CeilToInt(cost)+" MP":"Ch\u1edd";
            Set(view.state,state);
            view.cooldown.fillAmount=cd>0?Mathf.Clamp01(cd/duration):unlocked?0:1;
            view.icon.color=view.button.interactable?Color.white:new Color(.48f,.56f,.55f,1);
            view.state.color=sm.currentLinhLuc<cost&&unlocked&&cd<=0?theme.gold:theme.ivory;
            view.button.targetGraphic.color=hudSurface!=null?(view.button.interactable?Color.white:new Color(.55f,.62f,.60f,1)):view.button.interactable?theme.raised:theme.ink;
            view.title.color=view.button.interactable?theme.gold:theme.muted;
        }
        public void ConfigureAppearance()
        {
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
                if(text.name=="P2Label")
                {text.textWrappingMode=TextWrappingModes.NoWrap;FoundationFactory.Fill(text.rectTransform,12,4,12,4);}
            foreach(var root in new[]{pause,(RectTransform)legacy.menuButton.transform,legacy.focusButton})
            {
                var text=root.Find("P2Label")?.GetComponent<TMP_Text>();
                if(text!=null){text.fontSize=24;FoundationFactory.Fill(text.rectTransform,8,4,8,4);}
            }
            if(claimAction==null)claimAction=task.GetComponentInChildren<FoundationAction>(true);
            if(claimAction!=null)FoundationFactory.Fill(claimAction.label.rectTransform,8,4,8,4);
            // Thin bars need thin tracks: the foundation's default 8-unit inset consumes a 12-unit track.
            foreach(var bar in new[]{waveProgress,bossHealth,cultivation})
                if(bar!=null)FoundationFactory.Fill(bar.fill.parent as RectTransform,2,2,2,2);
            foreach(var v in skillViews)Accent((RectTransform)v.button.transform);
            foreach(var button in drawer.dockButtons)Accent((RectTransform)button.transform);
        }
        void Accent(RectTransform parent)
        {
            if(parent.Find("P2Accent")!=null)return;
            var line=FoundationFactory.Rect("P2Accent",parent);
            PortraitHudController.Place(line,0,1,1,1,0,-2,-24,2);
            new FoundationFactory(theme).Surface(line,theme.gold);
        }
        void RefreshNavigationStyle()
        {
            for(int i=0;i<drawer.dockButtons.Length;i++)
            {
                var button=drawer.dockButtons[i];bool unlocked=drawer.IsDrawerUnlocked(i,out _);
                bool selected=drawer.ActiveDrawerIndex==i;
                button.targetGraphic.color=hudSurface!=null?(selected?Color.white:new Color(.72f,.78f,.76f,1)):selected?theme.raised:theme.ink;
                var text=button.transform.Find("P2Label")?.GetComponent<TMP_Text>();
                if(text!=null)text.color=!unlocked?theme.muted:selected?theme.gold:theme.ivory;
            }
        }
        void RefreshSwordStyle()
        {
            if(styleButton==null)
            {
                styleButton=legacy.styleHost.GetComponentInChildren<UnityEngine.UI.Button>(true);
                if(styleButton==null)return;
                sourceStyle=styleButton.GetComponentInChildren<UnityEngine.UI.Text>(true);
                foreach(var graphic in styleButton.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))if(graphic.transform!=styleButton.transform)graphic.enabled=false;
                var image=styleButton.GetComponent<UnityEngine.UI.Image>();image.sprite=hudSurface!=null?hudSurface:theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;
                if(hudSurface!=null)image.pixelsPerUnitMultiplier=10;
                styleCaption=new FoundationFactory(theme).Text("P2SwordStyle",styleButton.transform,"",24,TextAlignmentOptions.Center);
                FoundationFactory.Fill(styleCaption.rectTransform,12,0,12,0);styleCaption.textWrappingMode=TextWrappingModes.NoWrap;styleCaption.color=theme.gold;
            }
            styleButton.targetGraphic.color=hudSurface!=null?Color.white:theme.surface;
            if(sourceStyle!=null)Set(styleCaption,sourceStyle.text);
        }
        public void ApplyLayout()
        {
            if(legacy==null)return;
            rect=(RectTransform)transform;lastSize=rect.rect.size;lastCompact=IsCompact;lastBoss=HasBoss;
            PortraitHudController.Fill(legacy.upper,0,0,0,0);PortraitHudController.Fill(legacy.lower,0,0,0,0);
            Top(legacy.profile,24,20,410,104);
            Top(legacy.wallet,450,20,246,104);
            Top(pause,720,20,96,104);Top((RectTransform)legacy.menuButton.transform,828,20,96,104);Top(legacy.focusButton,936,20,120,104);
            task.gameObject.SetActive(!IsCompact);Top(task,24,136,1032,72);
            Top(wave,24,IsCompact?136:220,HasBoss?660:684,56);
            Top(legacy.styleHost,720,IsCompact?136:220,336,56);
            boss.gameObject.SetActive(HasBoss);Top(boss,24,IsCompact?204:288,1032,88);
            Bottom(vitalRoot,24,IsCompact?164:externalNavigation?352:320,1032,88);
            HalfBar((RectTransform)hp.transform,true);HalfBar((RectTransform)mp.transform,false);
            hp.label.fontSize=26;mp.label.fontSize=24;
            FoundationFactory.Fill(hp.label.rectTransform,12,0,12,0);FoundationFactory.Fill(mp.label.rectTransform,12,0,12,0);
            bossHealth.label.gameObject.SetActive(false);
            Bottom(legacy.skills,24,IsCompact?20:externalNavigation?176:144,1032,IsCompact?132:160);legacy.skills.gameObject.SetActive(true);
            var layout=legacy.skills.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();if(layout!=null)layout.enabled=false;
            for(int i=0;i<skillViews.Length;i++)
            {
                var v=skillViews[i];PortraitHudController.Place((RectTransform)v.button.transform,i/4f,0,(i+1)/4f,1,0,0,-18,0);
                v.title.fontSize=26;v.state.fontSize=24;
                PortraitHudController.Place(v.title.rectTransform,0,1,1,1,0,-2,-12,40);
                PortraitHudController.Place(v.state.rectTransform,0,0,1,0,0,2,-8,40);
                var icon=v.icon.rectTransform;
                PortraitHudController.Place(icon,.5f,.5f,.5f,.5f,0,0,IsCompact?48:64,IsCompact?48:64);
                PortraitHudController.Place(v.cooldown.rectTransform,.5f,.5f,.5f,.5f,0,0,IsCompact?54:72,IsCompact?54:72);
            }
            Top(legacy.notification,156,HasBoss?392:292,768,116);
            var box=legacy.secondaryOverlay.transform.Find("MenuCard") as RectTransform;
            if(box!=null){PortraitHudController.Place(box,.5f,.5f,.5f,.5f,0,0,Mathf.Min(1008,lastSize.x-48),Mathf.Min(960,lastSize.y-96));}
            LayoutNavigation();
            float top=HasBoss?(IsCompact?308:392):(IsCompact?204:284);float bottom=IsCompact?264:externalNavigation?452:420;
            ClearArea=new Rect(24,bottom,Mathf.Max(0,lastSize.x-48),Mathf.Max(0,lastSize.y-top-bottom));
            ClearHeightFraction=lastSize.y>0?ClearArea.height/lastSize.y:0;
        }
        void LayoutNavigation()
        {
            if(externalNavigation)return;
            if(legacy.dock!=null)
            {
                Bottom(legacy.dock,24,16,1032,112);
                for(int i=0;i<drawer.dockButtons.Length;i++)
                    PortraitHudController.Place((RectTransform)drawer.dockButtons[i].transform,i/5f,0,(i+1)/5f,1,0,0,-12,0);
            }
            if(legacy.focusButton!=null)Top(legacy.focusButton,936,20,120,104);
            if(legacy.menuGrid!=null)
            {
                PortraitHudController.Fill((RectTransform)legacy.menuGrid.transform,32,32,32,180);
                var buttons=new[]{legacy.profileButton,legacy.achievementButton,legacy.buffsButton,legacy.logButton};
                for(int i=0;i<buttons.Length;i++)PortraitHudController.Place((RectTransform)buttons[i].transform,(i%2)*.5f,1,(i%2+1)*.5f,1,0,-(i/2)*160,-20,136);
                PortraitHudController.Place(legacy.quick,0,0,1,0,0,20,0,320);legacy.quick.gameObject.SetActive(true);
                for(int i=0;i<legacy.quickButtons.Length;i++)PortraitHudController.Place((RectTransform)legacy.quickButtons[i].transform,(i%2)*.5f,1,(i%2+1)*.5f,1,0,-(i/2)*160,-20,136);
            }
        }
        static void HalfBar(RectTransform r,bool top)
        {PortraitHudController.Place(r,0,top?1:0,1,top?1:0,0,0,0,top?48:36);}
        public static void Top(RectTransform r,float x,float y,float w,float h)
        {PortraitHudController.Place(r,0,1,0,1,x,-y,w,h);}
        public static void Bottom(RectTransform r,float x,float y,float w,float h)
        {PortraitHudController.Place(r,0,0,0,0,x,y,w,h);}
    }
}

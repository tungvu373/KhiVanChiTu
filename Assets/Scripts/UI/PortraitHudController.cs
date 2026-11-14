using System;
using System.Collections;
using UnityEngine;

/// <summary>Scene-scoped HUD presentation. Existing services and controls own every command/value.</summary>
[DefaultExecutionOrder(1600)]
[DisallowMultipleComponent]
public sealed class PortraitHudController : MonoBehaviour
{
    public static PortraitHudController Instance { get; private set; }
    public static bool IsOverlayBlocking => Instance != null && Instance.isActiveAndEnabled && Instance.IsSecondaryOpen;

    public RectTransform upper, lower, profile, wallet, realm, stage, quest, skills, quick, dock, vitals;
    public RectTransform hp, mana, styleHost, notification, focusButton, pauseButton;
    public UnityEngine.UI.Text sourceProfile, playerName, playerRealm, cultivationValue, menuTitle, walletDetails, buffSummary, manaValue;
    public UnityEngine.UI.Button menuButton, closeMenuButton, menuBackdrop, milestoneButton, profileButton, buffsButton, logButton, walletButton, achievementButton;
    public UnityEngine.UI.Button[] quickButtons;
    public GameObject secondaryOverlay, menuGrid, detailsPage;
    public UnityEngine.UI.Text detailsText;
    public Sprite panelSprite, windowSprite, slotSprite, selectedSprite;
    public Font regularFont, semiboldFont;
    public RectTransform logPanel, buffPanel;
    public bool IsSecondaryOpen => secondaryOverlay != null && secondaryOverlay.activeSelf;
    public bool IsFocusMode => drawer != null && drawer.IsDockCollapsed;
    public Rect CentralClearArea { get; private set; }

    MobileDrawerUIController drawer;
    EconomyManager economy;
    CultivationManager cultivation;
    RectTransform bounds;
    Vector2 lastSize;
    bool bound, questExpanded;
    string lastProfile;
    string lastCharacterName;
    float lastStones, pulseTime;
    float walletPulseTime,realmPulseTime;
    MobileUIAnimationDirector uiMotion;
    int lastFate;
    UnityEngine.UI.Image walletPulse, realmPulse;
    UnityEngine.UI.Text stoneValue, fateValue, questTitle, realmTitle;
    UnityEngine.UI.Text bossTimerLabel;
    bool lastBossTimerActive;
    CanvasGroup dockGroup;
    Vector2Int lastScreen;
    float lastMana=float.NaN,lastMaxMana=float.NaN;
    readonly Color ivory = new Color32(241,232,207,255);
    readonly Color jade = new Color32(110,221,182,255);
    readonly Color gold = new Color32(213,181,116,255);

    void Awake()
    {
        uiMotion=MobileUIAnimationDirector.For(this);
        Instance = this;
        bounds = (RectTransform)transform;
        drawer = GetComponentInParent<MobileDrawerUIController>();
        if(dock==null&&drawer!=null)dock=drawer.mobileDock;
        if(dock!=null)dockGroup=dock.GetComponent<CanvasGroup>()??dock.gameObject.AddComponent<CanvasGroup>();
        var parent = drawer != null ? drawer.transform : transform.parent;
        // These two aliases remain direct children for the existing factories' path lookups.
        if (parent != null)
        {
            if (logPanel == null) logPanel = parent.Find("CombatLogPanel") as RectTransform;
            if (buffPanel == null) buffPanel = parent.Find("ActiveBuffPanel") as RectTransform;
        }
        if (sourceProfile != null) sourceProfile.enabled = false;
        walletPulse = wallet != null ? wallet.Find("FeedbackLine")?.GetComponent<UnityEngine.UI.Image>() : null;
        realmPulse = realm != null ? realm.Find("FeedbackLine")?.GetComponent<UnityEngine.UI.Image>() : null;
        stoneValue=wallet.Find("txt_LinhThach").GetComponent<UnityEngine.UI.Text>();
        fateValue=wallet.Find("txt_CoDuyen").GetComponent<UnityEngine.UI.Text>();
        questTitle=quest.Find("TxtQuestTitle").GetComponent<UnityEngine.UI.Text>();
        realmTitle=realm.Find("StageText").GetComponent<UnityEngine.UI.Text>();
        bossTimerLabel=stage!=null?stage.Find("BossTimer")?.GetComponent<UnityEngine.UI.Text>():null;
        if (pauseButton == null && upper != null)
        {
            var pb = upper.Find("PauseButton");
            if (pb != null) pauseButton = pb as RectTransform;
        }
        CloseSecondary();
    }

    void OnEnable()
    {
        uiMotion=MobileUIAnimationDirector.For(this);
        if (!bound) return;
        RefreshRenderScale();
        Subscribe();
        RefreshProfile();
        ApplyLayout();
    }

    void RefreshRenderScale()
    {
        if(lastScreen.x!=Screen.width||lastScreen.y!=Screen.height)
        {
            lastScreen=new Vector2Int(Screen.width,Screen.height);
            foreach(var text in GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.cachedTextGenerator.Invalidate();text.SetAllDirty();}
            if(dock!=null)foreach(var text in dock.GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.cachedTextGenerator.Invalidate();text.SetAllDirty();}
        }
    }

    void Start()
    {
        Bind(menuButton, ToggleSecondary);
        Bind(closeMenuButton, CloseSecondary);
        Bind(menuBackdrop, CloseSecondary);
        Bind(milestoneButton, OpenMilestones);
        Bind(profileButton, OpenProfile);
        Bind(buffsButton, ShowBuffs);
        Bind(logButton, ShowLog);
        Bind(walletButton, ShowWallet);
        Bind(achievementButton, CloseSecondary);
        if (quickButtons != null) foreach (var button in quickButtons) Bind(button, CloseSecondary);
        bound = true;
        Subscribe();
        ConfigureViews();
        ApplyLayout();
        RefreshProfile();
        CloseSecondary();
    }

    void Subscribe()
    {
        Unsubscribe();
        economy = EconomyManager.Instance;
        cultivation = CultivationManager.Instance;
        if (economy != null)
        {
            lastStones = economy.currentLinhThach;
            lastFate = economy.currentCoDuyen;
            economy.OnLinhThachChanged += OnStones;
            economy.OnCoDuyenChanged += OnFate;
            RefreshWallet();
        }
        if (cultivation != null)
        {
            cultivation.OnTuViChanged += OnCultivation;
            cultivation.OnStageChanged += OnRealm;
        }
        if (drawer != null)
        {
            drawer.OnDockCollapseChanged += OnFocus;
            drawer.OnDrawerStateChanged += OnDrawer;
        }
    }

    void Unsubscribe()
    {
        if (economy != null) { economy.OnLinhThachChanged -= OnStones; economy.OnCoDuyenChanged -= OnFate; }
        if (cultivation != null) { cultivation.OnTuViChanged -= OnCultivation; cultivation.OnStageChanged -= OnRealm; }
        if (drawer != null) { drawer.OnDockCollapseChanged -= OnFocus; drawer.OnDrawerStateChanged -= OnDrawer; }
    }
    void OnDisable() { Unsubscribe(); CloseSecondary(); }
    void OnDestroy() { Unsubscribe(); if (Instance == this) Instance = null; }
    static void Bind(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action)
    { if (button == null) return; button.onClick.RemoveListener(action); button.onClick.AddListener(action); }
    void OnStones(float value)
    {
        if(uiMotion==null){if(value!=lastStones)pulseTime=.55f;}
        else if(value>lastStones&&!uiMotion.reducedMotion)walletPulseTime=uiMotion.accent.duration;
        lastStones=value;RefreshWallet();
    }
    void OnFate(int value)
    {
        if(uiMotion==null){if(value!=lastFate)pulseTime=.55f;}
        else if(value>lastFate&&!uiMotion.reducedMotion)walletPulseTime=uiMotion.accent.duration;
        lastFate=value;RefreshWallet();
    }
    void OnCultivation(float value, float required) { RefreshProfile(); }
    void OnRealm(CultivationStageData data)
    {
        if(uiMotion==null)pulseTime=.55f;else if(!uiMotion.reducedMotion)realmPulseTime=uiMotion.progress.duration;
        RefreshProfile();
    }
    void OnFocus(bool value) { CloseSecondary(); ApplyLayout(); }
    void OnDrawer(int index) { if (index >= 0) CloseSecondary(); ApplyLayout(); }

    void LateUpdate()
    {
        if (!bound) return;
        RefreshRenderScale();
        if (economy != EconomyManager.Instance || cultivation != CultivationManager.Instance) Subscribe();
        if (bounds.rect.size != lastSize) ApplyLayout();
        // Legacy tutorial collapse and character-name updates have no dedicated change event.
        bool expanded = quest != null && quest.Find("TxtQuestTitle")?.gameObject.activeSelf == true;
        if (expanded != questExpanded || (questTitle!=null&&questTitle.fontSize!=36)) LayoutQuest();
        if (realmTitle!=null&&(realmTitle.fontSize!=32||realmTitle.rectTransform.rect.height<44)) LayoutRealm();
        if (bossTimerLabel!=null&&bossTimerLabel.gameObject.activeSelf!=lastBossTimerActive) LayoutStage();
        if ((sourceProfile != null && sourceProfile.text != lastProfile) ||
            (cultivation != null && cultivation.CharacterName != lastCharacterName)) RefreshProfile();
        if (drawer != null && drawer.dockToggleLabelText != null)
        {
            string label = IsFocusMode ? "Hiện\nHUD" : "Thu\ngọn";
            if (drawer.dockToggleLabelText.text != label) drawer.dockToggleLabelText.text = label;
        }
        var choices = GameplayChoicesService.Instance;
        if (choices != null) choices.BindPortraitHudStyle(styleHost);
        var skillView=skills!=null?skills.GetComponent<MobileSkillHUD>():null;
        if(skillView!=null&&skillView.imgUltiGlow!=null&&skillView.imgUltiGlow.gameObject.activeSelf)
            skillView.imgUltiGlow.color=new Color(jade.r,jade.g,jade.b,.16f);
        var sm=SkillManager.Instance;
        if(manaValue!=null&&sm!=null&&(lastMana!=sm.currentLinhLuc||lastMaxMana!=sm.maxLinhLuc))
        {lastMana=sm.currentLinhLuc;lastMaxMana=sm.maxLinhLuc;manaValue.text="MP "+Mathf.FloorToInt(lastMana)+" / "+Mathf.FloorToInt(lastMaxMana);}
        pulseTime = Mathf.Max(0, pulseTime - Time.unscaledDeltaTime);
        var glow = new Color(jade.r, jade.g, jade.b, pulseTime / .55f * .8f);
        if(uiMotion!=null)
        {
            walletPulseTime=Mathf.Max(0,walletPulseTime-Time.unscaledDeltaTime);realmPulseTime=Mathf.Max(0,realmPulseTime-Time.unscaledDeltaTime);
            var walletGlow=new Color(jade.r,jade.g,jade.b,uiMotion.reducedMotion?0:walletPulseTime/Mathf.Max(.01f,uiMotion.accent.duration)*.6f);
            var realmGlow=new Color(gold.r,gold.g,gold.b,uiMotion.reducedMotion?0:realmPulseTime/Mathf.Max(.01f,uiMotion.progress.duration)*.6f);
            if(walletPulse!=null&&walletPulse.color!=walletGlow)walletPulse.color=walletGlow;
            if(realmPulse!=null&&realmPulse.color!=realmGlow)realmPulse.color=realmGlow;
        }
        else
        {
            if (walletPulse != null && walletPulse.color != glow) walletPulse.color = glow;
            if (realmPulse != null && realmPulse.color != glow) realmPulse.color = glow;
        }
        if (IsSecondaryOpen && detailsPage.activeSelf) RefreshDetails();
        if (buffSummary != null && ShopManager.Instance != null)
        {
            int count=0; for(int i=0;i<5;i++) if(ShopManager.Instance.GetTimerByIndex(i)>0) count++;
            string text = "Hiệu ứng · " + count;
            if (buffSummary.text != text) buffSummary.text = text;
        }
    }

    public void RefreshProfile()
    {
        lastProfile = sourceProfile != null ? sourceProfile.text : null;
        var cm = CultivationManager.Instance;
        if (cm == null) return;
        string name = cm.CharacterName;
        lastCharacterName = name;
        if (string.IsNullOrWhiteSpace(name)) name = "Đạo Hữu";
        playerName.text = name;
        playerRealm.text = cm.GetDisplayRealmName();
        playerRealm.enabled = false;
        var data = cm.GetCurrentStageData();
        // Hiển thị Lực Chiến / Tu Vi màu vàng gold chuẩn Ảnh 2
        cultivationValue.text = "⚔ Lực Chiến: " + Compact(cm.currentTuVi * 12.5f + 1000f);
        cultivationValue.color = gold;
    }

    void RefreshWallet()
    {
        if(economy==null||stoneValue==null||fateValue==null)return;
        stoneValue.text="LT "+Compact(economy.currentLinhThach);stoneValue.color=ivory;
        fateValue.text="CD "+Compact(economy.currentCoDuyen);fateValue.color=gold;
    }
    public static string Compact(double value)
    {
        double n=Math.Abs(value);string suffix="";
        if(n>=1e15)return value.ToString("0.##E+0");
        if(n>=1e12){value/=1e12;suffix="T";}
        else if(n>=1e9){value/=1e9;suffix="B";}
        else if(n>=1e6){value/=1e6;suffix="M";}
        else if(n>=1e3){value/=1e3;suffix="K";}
        return value.ToString(suffix==""?"N0":"0.##")+suffix;
    }

    public void ApplyLayout()
    {
        if (bounds == null) bounds = (RectTransform)transform;
        lastSize = bounds.rect.size;
        bool shortView = lastSize.y < 1500;
        bool focus = IsFocusMode;

        // ─── GÓC TRÊN-TRÁI: Profile + HP/MP ───────────────────────────────────
        // Profile block: avatar, tên, lực chiến vàng
        Place(profile, 0, 1, 0, 1, 20, -16, 450, 156);
        // Vitals (HP/MP) nằm gọn trong profile, bên phải avatar
        Place(vitals, 0, 1, 0, 1, 148, -82, 290, 62);
        LayoutVitals();

        // ─── CỘT NÚT TIỆN ÍCH GÓC PHẢI (Thẳng hàng dọc, cách nhau rõ ràng) ────
        // 1. Nút Tạm Dừng ở góc trên cùng bên phải
        if (pauseButton != null) Place(pauseButton, 1, 1, 1, 1, -16, -16, 72, 72);
        // 2. Nút Đạo Lộ (Menu) ngay bên dưới Tạm Dừng
        Place(menuButton != null ? menuButton.transform as RectTransform : null, 1, 1, 1, 1, -16, -96, 72, 72);
        // 3. Nút Thu Gọn / Hiện HUD ngay bên dưới Đạo Lộ
        if (focusButton != null) Place(focusButton, 1, 1, 1, 1, -16, -176, 72, 72);

        // ─── TÀI NGUYÊN (Linh Thạch & Cơ Duyên 2 tầng rõ ràng) ─────────────────
        // Đặt ở góc trên, bên trái cột nút tiện ích (X=-96), chiều cao 136px đủ cho cả 2 dòng
        Place(wallet, 1, 1, 1, 1, -96, -16, 360, 136);

        // ─── CẠNH PHẢI: Khung Tầng (Stage) & Đổi Kiếm (StyleHost) ────────────
        // Đặt thẳng dưới nút Thu Gọn / Hiện HUD, không bị đè hay trùng lặp
        Place(stage, 1, 1, 1, 1, -16, -264, 270, 84);
        Place(styleHost, 1, 1, 1, 1, -16, -356, 270, 72);

        // ─── CẠNH TRÁI: Bảng Nhiệm Vụ & Thanh Cảnh Giới ───────────────────────
        // Nhiệm vụ bắt đầu ngay dưới Profile
        LayoutQuest();
        // Cảnh giới đặt gọn gàng ngay dưới Nhiệm Vụ
        Place(realm, 0, 1, 0, 1, 20, -348, 420, 68);
        LayoutRealm();

        // ─── KỸ NĂNG: Vòng Cung bên phải màn hình ────────────────────────────
        if (skills != null)
        {
            skills.gameObject.SetActive(true);
            // Container phủ toàn canvas để định vị tuyệt đối từng nút
            skills.anchorMin = Vector2.zero;
            skills.anchorMax = Vector2.one;
            skills.offsetMin = Vector2.zero;
            skills.offsetMax = Vector2.zero;
            LayoutSkillsArc();
        }
        if (quick != null) quick.gameObject.SetActive(false);

        // ─── ĐÁY MÀN HÌNH: Thanh Dock ───────────────────────────────────────
        Place(dock, 0, 0, 1, 0, 0, 16, 0, 150);
        SetDockState();

        // ─── THÔNG BÁO + VÙNG 3D TRUNG TÂM ──────────────────────────────────
        Place(notification, 0.5f, 1, 0.5f, 1, 0, -560, 840, 120);
        CentralClearArea = new Rect(20, 172, Mathf.Max(0, lastSize.x - 40), Mathf.Max(0, lastSize.y - 480));

        var overlayBox = secondaryOverlay != null ? secondaryOverlay.transform.Find("MenuCard") as RectTransform : null;
        if (overlayBox != null)
        {
            overlayBox.anchorMin=overlayBox.anchorMax=overlayBox.pivot=new Vector2(.5f,.5f);
            overlayBox.sizeDelta=new Vector2(Mathf.Min(1008,lastSize.x-72),Mathf.Min(1100,lastSize.y-72));
            overlayBox.anchoredPosition=Vector2.zero;
        }
        if (logPanel != null) AuxRect(logPanel, overlayBox);
        if (buffPanel != null) AuxRect(buffPanel, overlayBox);
    }

    void AuxRect(RectTransform view, RectTransform box)
    {
        if (box == null) return;
        view.anchorMin=view.anchorMax=view.pivot=new Vector2(.5f,.5f);
        view.sizeDelta=new Vector2(box.rect.width-72,Mathf.Max(132,box.rect.height-240));
        view.anchoredPosition=new Vector2(0,-60);
    }

    void LayoutQuest()
    {
        if (quest == null) return;
        var title=quest.Find("TxtQuestTitle")?.GetComponent<UnityEngine.UI.Text>();
        questExpanded=title!=null&&title.gameObject.activeSelf;
        // Quest bắt đầu ngay dưới Profile (profile chiếm 16+156=172px → đặt tại -188 để có 16px gap)
        Place(quest, 0, 1, 0, 1, 20, -188, questExpanded ? 420 : 128, 148);
        if(title!=null)
        {
            Place(title.rectTransform, 0, 1, 1, 1, 16, -10, -120, 36);
            Text(title, 28, TextAnchor.MiddleLeft);
            title.color = gold;
        }
        var reward=quest.Find("QuestDescScrollView") as RectTransform;
        if(reward!=null) Place(reward, 0, 0, 1, 1, 16, 10, -120, -48);
        var rewardText=reward!=null?reward.GetComponentInChildren<UnityEngine.UI.Text>(true):null;
        if(rewardText!=null) { Text(rewardText, 24, TextAnchor.UpperLeft); rewardText.color = ivory; }
        var claim=quest.Find("BtnClaimQuick") as RectTransform;
        if(claim!=null) { Place(claim, 1, 0.5f, 1, 0.5f, -68, 0, 60, 60); if(questExpanded) claim.gameObject.SetActive(true); }
        var toggle=quest.Find("BtnToggle") as RectTransform;
        if(toggle!=null) { Place(toggle, 1, 0.5f, 1, 0.5f, -4, 0, 56, 56); var text=toggle.GetComponent<UnityEngine.UI.Text>(); if(text!=null) Text(text, 24, TextAnchor.MiddleCenter); }
        quest.Find("BtnAchievement")?.gameObject.SetActive(false);
        quest.Find("BtnStorage")?.gameObject.SetActive(false);
        quest.Find("BtnQuestToggle")?.gameObject.SetActive(false);
        foreach(var text in quest.GetComponentsInChildren<UnityEngine.UI.Text>(true)) text.raycastTarget=text.GetComponent<UnityEngine.UI.Button>()!=null;
    }

    public void ConfigureViews()
    {
        if(drawer==null)drawer=GetComponentInParent<MobileDrawerUIController>();
        foreach(var outline in GetComponentsInChildren<UnityEngine.UI.Outline>(true)) outline.enabled=false;
        foreach(var graphic in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            if(graphic.GetComponent<UnityEngine.UI.Button>()==null) graphic.raycastTarget=false;
        // Existing CharacterFrameButton handles pointer events without a Button component.
        var hit=profile.GetComponent<UnityEngine.UI.Image>(); if(hit!=null)hit.raycastTarget=true;
        foreach(var text in GetComponentsInChildren<UnityEngine.UI.Text>(true))
            if(text!=sourceProfile) { text.font=regularFont; text.resizeTextForBestFit=false; text.color=ivory; }
        Text(playerName,30,TextAnchor.MiddleLeft); playerName.font=semiboldFont;
        Place(playerName.rectTransform,0,1,0,1,145,-12,320,36);
        playerName.resizeTextForBestFit=true;playerName.resizeTextMinSize=22;playerName.resizeTextMaxSize=30;
        playerName.verticalOverflow=VerticalWrapMode.Truncate;
        Text(playerRealm,24,TextAnchor.MiddleLeft); playerRealm.color=jade;playerRealm.enabled=false;
        Text(cultivationValue,24,TextAnchor.MiddleLeft);
        cultivationValue.color=gold;
        Place(cultivationValue.rectTransform,0,1,0,1,145,-46,320,30);
        foreach(var value in wallet.GetComponentsInChildren<MobileCurrencyText>(true))
        {value.enabled=false;Text(value.GetComponent<UnityEngine.UI.Text>(),32,TextAnchor.MiddleRight);}
        foreach(var button in secondaryOverlay.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))Text(text,36,TextAnchor.MiddleCenter);
        menuTitle.fontSize=50;
        Place(menuTitle.rectTransform,0,1,1,1,0,-32,-144,84);
        LayoutRealm(); LayoutSkills(); LayoutVitals();
        if(drawer!=null)
        {
            for(int i=0;i<drawer.dockButtons.Length;i++)
            {
                var b=drawer.dockButtons[i];if(b==null)continue;
                Place((RectTransform)b.transform,i/5f,0,(i+1)/5f,1,0,0,-12,0);
                var icon=b.transform.Find("JadeNavIcon") as RectTransform;
                if(icon!=null)Place(icon,.5f,1,.5f,1,0,-14,78,78);
                var label=b.transform.Find("Label")?.GetComponent<UnityEngine.UI.Text>();
                if(label!=null){Place(label.rectTransform,0,0,1,0,0,12,-12,64);Text(label,30,TextAnchor.MiddleCenter);label.font=semiboldFont;}
            }
            if(drawer.dockToggleLabelText!=null)Text(drawer.dockToggleLabelText,30,TextAnchor.MiddleCenter);
        }
        if(quickButtons!=null)for(int i=0;i<quickButtons.Length;i++)
        {
            var b=quickButtons[i];if(b==null)continue;
            Place((RectTransform)b.transform,i/4f,0,(i+1)/4f,1,0,0,-12,0);
            foreach(var label in b.GetComponentsInChildren<UnityEngine.UI.Text>(true)){Fill(label.rectTransform,12,8,12,8);Text(label,36,TextAnchor.MiddleCenter);}
        }
        if(stage!=null)
        {
            LayoutStage();
        }
        if(hp!=null)hp.gameObject.SetActive(true);
        if(mana!=null)mana.gameObject.SetActive(true);
        var presentation=GetComponentInParent<MobileUIReconstruction>();
        var shortcut=presentation!=null&&presentation.dismantleShortcut!=null?presentation.dismantleShortcut:drawer!=null?drawer.transform.Find("Drawer_0/MergePanel/BtnDrawerDismantle") as RectTransform:null;
        if(shortcut!=null)
        {
            Place(shortcut,1,1,1,1,-16,0,430,132);
            foreach(var text in shortcut.GetComponentsInChildren<UnityEngine.UI.Text>(true)){Fill(text.rectTransform,20,12,20,12);Text(text,30,TextAnchor.MiddleCenter);}
        }
        LayoutQuest();
    }

    void LayoutRealm()
    {
        if(realm==null)return;
        var title=realm.Find("StageText")?.GetComponent<UnityEngine.UI.Text>();
        if(title!=null){Place(title.rectTransform,0,1,1,1,12,-6,-24,28);Text(title,24,TextAnchor.MiddleLeft);title.verticalOverflow=VerticalWrapMode.Truncate;}
        if(cultivationValue!=null&&cultivationValue.transform.parent==realm){Place(cultivationValue.rectTransform,0,1,1,1,12,-34,-24,24);Text(cultivationValue,22,TextAnchor.MiddleLeft);cultivationValue.verticalOverflow=VerticalWrapMode.Truncate;}
        var segments=realm.Find("PhaseSegments") as RectTransform;if(segments==null)return;
        segments.anchorMin=new Vector2(0,0);segments.anchorMax=new Vector2(1,0);segments.pivot=new Vector2(.5f,0);segments.sizeDelta=new Vector2(-24,24);segments.anchoredPosition=new Vector2(0,6);
        foreach(var text in segments.GetComponentsInChildren<UnityEngine.UI.Text>(true)){Text(text,20,TextAnchor.MiddleCenter);Fill(text.rectTransform,2,2,2,2);text.resizeTextForBestFit=true;text.resizeTextMinSize=16;text.resizeTextMaxSize=20;text.verticalOverflow=VerticalWrapMode.Truncate;}
    }

    public void LayoutStage()
    {
        if(stage==null)return;
        var title=stage.Find("StageText")?.GetComponent<UnityEngine.UI.Text>();
        var timer=stage.Find("BossTimer")?.GetComponent<UnityEngine.UI.Text>();
        bool hasTimer=timer!=null&&timer.gameObject.activeSelf;
        lastBossTimerActive=hasTimer;
        if(title!=null)
        {
            if(hasTimer)Place(title.rectTransform,0,1,1,1,0,-12,-20,36);
            else Fill(title.rectTransform,20,12,20,12);
            Text(title,28,TextAnchor.MiddleCenter);title.resizeTextForBestFit=true;title.resizeTextMinSize=22;title.resizeTextMaxSize=28;title.verticalOverflow=VerticalWrapMode.Truncate;
        }
        if(timer!=null){Place(timer.rectTransform,.5f,0,.5f,0,0,8,220,28);Text(timer,22,TextAnchor.MiddleCenter);timer.verticalOverflow=VerticalWrapMode.Truncate;}
    }

    void LayoutVitals()
    {
        for(int i=0;i<2;i++)
        {
            var view=i==0?hp:mana;if(view==null)continue;
            Place(view,0,i==0?1:0,1,i==0?1:0,0,i==0?0:-34,0,i==0?32:28);
            var rootImage=view.GetComponent<UnityEngine.UI.Image>();
            if(rootImage!=null)
            {
                if(rootImage.sprite==null&&panelSprite!=null)rootImage.sprite=panelSprite;
                rootImage.type=UnityEngine.UI.Image.Type.Sliced;
                rootImage.color=Color.white;
                rootImage.raycastTarget=false;
            }

            var bg=view.Find("BG") as RectTransform;
            if(bg!=null)
            {
                bg.anchorMin=Vector2.zero;
                bg.anchorMax=Vector2.one;
                bg.pivot=new Vector2(.5f,.5f);
                bg.offsetMin=new Vector2(6,2);
                bg.offsetMax=new Vector2(-6,-2);
                var bgImage=bg.GetComponent<UnityEngine.UI.Image>();
                if(bgImage!=null){bgImage.color=new Color(.02f,.05f,.07f,.95f);bgImage.raycastTarget=false;}

                var fill=bg.Find("Fill") as RectTransform;
                if(fill!=null)
                {
                    fill.anchorMin=Vector2.zero;
                    fill.anchorMax=new Vector2(fill.anchorMax.x,1f);
                    fill.offsetMin=Vector2.zero;
                    fill.offsetMax=Vector2.zero;
                    var fillImg=fill.GetComponent<UnityEngine.UI.Image>();
                    if(fillImg!=null)
                    {
                        fillImg.color=i==0?new Color(.92f,.25f,.25f,1f):new Color(.2f,.62f,.98f,1f);
                        fillImg.raycastTarget=false;
                    }
                }
            }

            if(i==0)
            {
                var hpLabel=view.Find("HPText")?.GetComponent<UnityEngine.UI.Text>();
                if(hpLabel!=null)
                {
                    Text(hpLabel,18,TextAnchor.MiddleCenter);
                    hpLabel.fontStyle=FontStyle.Bold;
                    hpLabel.color=ivory;
                    hpLabel.rectTransform.anchorMin=Vector2.zero;
                    hpLabel.rectTransform.anchorMax=Vector2.one;
                    hpLabel.rectTransform.offsetMin=new Vector2(10,0);
                    hpLabel.rectTransform.offsetMax=new Vector2(-10,0);
                    hpLabel.transform.SetAsLastSibling();
                }
            }
            else
            {
                if(manaValue!=null)
                {
                    Text(manaValue,18,TextAnchor.MiddleCenter);
                    manaValue.fontStyle=FontStyle.Bold;
                    manaValue.color=ivory;
                    manaValue.rectTransform.anchorMin=Vector2.zero;
                    manaValue.rectTransform.anchorMax=Vector2.one;
                    manaValue.rectTransform.offsetMin=new Vector2(10,0);
                    manaValue.rectTransform.offsetMax=new Vector2(-10,0);
                    manaValue.transform.SetAsLastSibling();
                }
                var legacy=view.Find("ManaText")?.GetComponent<UnityEngine.UI.Text>();
                if(legacy!=null) legacy.gameObject.SetActive(false);
            }

            var slider=view.GetComponent<UnityEngine.UI.Slider>();
            if(slider==null)slider=view.gameObject.AddComponent<UnityEngine.UI.Slider>();
            slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.interactable=false;
            slider.handleRect=null;
            if(bg!=null)
            {
                var fill=bg.Find("Fill") as RectTransform;
                if(fill!=null)slider.fillRect=fill;
            }
        }
    }

    // Wrapper để tương thích với lời gọi cũ trong ConfigureViews()
    void LayoutSkills() { LayoutSkillsArc(); }

    /// <summary>
    /// Đặt 4 nút kỹ năng theo vòng cung (quarter-circle arc) bên phải màn hình.
    /// Nút Ultimate to nhất ở vị trí trên-trong (11 giờ), 3 nút thường theo vòng
    /// cung xuống đến 4 giờ (gần mép phải màn hình), thuận ngón cái phải.
    /// Container skills được phủ toàn canvas để định vị tuyệt đối.
    /// </summary>
    void LayoutSkillsArc()
    {
        var hud = skills != null ? skills.GetComponent<MobileSkillHUD>() : null;
        if (hud == null) return;
        var vLayout = skills.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vLayout != null) vLayout.enabled = false;
        var hLayout = skills.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (hLayout != null) hLayout.enabled = false;

        float W = lastSize.x;
        float scale = Mathf.Clamp(W / 1080f, 0.75f, 1.35f);

        // 4 nút kỹ năng xếp theo vòng cung thoáng đãng bên phải màn hình:
        // i=3 (Âm Dương): thấp nhất, gần mép phải, trên thanh dock
        // i=2 (Lôi Phạt): chếch lên trên-trái
        // i=1 (Ấn Chưởng): chếch tiếp lên trên-trái
        // i=0 (Tuyệt kỹ - Vạn Kiếm): to nhất, vươn cao nhất về phía giữa
        // Khe hở giữa các nút > 40px, rộng rãi, thao tác ngón cái cực kỳ thoải mái
        float[] btnSizes = { 128f * scale, 106f * scale, 106f * scale, 106f * scale };
        float[] centerXs = {
            W - 480f * scale, // i=0 (Ultimate)
            W - 330f * scale, // i=1 (Ấn Chưởng)
            W - 195f * scale, // i=2 (Lôi Phạt)
            W - 78f * scale   // i=3 (Âm Dương)
        };
        float[] centerYs = {
            405f * scale,     // i=0 (Ultimate)
            380f * scale,     // i=1 (Ấn Chưởng)
            315f * scale,     // i=2 (Lôi Phạt)
            225f * scale      // i=3 (Âm Dương)
        };

        var buttons = new UnityEngine.UI.Button[]
        {
            hud.btnUltimate,
            hud.btnNormalSkills.Length > 0 ? hud.btnNormalSkills[0] : null,
            hud.btnNormalSkills.Length > 1 ? hud.btnNormalSkills[1] : null,
            hud.btnNormalSkills.Length > 2 ? hud.btnNormalSkills[2] : null,
        };

        for (int i = 0; i < 4; i++)
        {
            var b = buttons[i]; if (b == null) continue;
            float bx = centerXs[i];
            float by = centerYs[i];
            var rt = (RectTransform)b.transform;
            // Anchor = góc dưới-trái canvas, pivot = tâm nút
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(btnSizes[i], btnSizes[i]);
            rt.anchoredPosition = new Vector2(bx, by);

            // Style viền / sprite
            var image = b.GetComponent<UnityEngine.UI.Image>();
            if (image != null && slotSprite != null)
                JadeUITheme.SetSurface(image, i == 0 ? selectedSprite ?? slotSprite : slotSprite);
            foreach (var t in b.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                Text(t, i == 0 ? 26 : 22, TextAnchor.MiddleCenter);

            // Text tiêu đề kỹ năng (phía trên nút)
            var title = i == 0 ? hud.txtUltiTitle : (hud.txtNormalTitles.Length > i-1 ? hud.txtNormalTitles[i-1] : null);
            // Text chi phí MP (phía dưới nút)
            var cost = i == 0 ? hud.txtUltiMana : (hud.txtNormalCosts.Length > i-1 ? hud.txtNormalCosts[i-1] : null);
            if (title != null)
            {
                Fill(title.rectTransform, 4, btnSizes[i] * 0.44f, 4, 6);
                Text(title, i == 0 ? 24 : 20, TextAnchor.UpperCenter);
            }
            if (cost != null)
            {
                Fill(cost.rectTransform, 4, 6, 4, btnSizes[i] * 0.44f);
                Text(cost, i == 0 ? 20 : 17, TextAnchor.LowerCenter);
                cost.color = new Color(0.65f, 0.9f, 1f, 1f);
            }

            // Emblem icon ở giữa
            var emblem = b.transform.Find("JadeSkillEmblem") as RectTransform;
            float eSize = i == 0 ? 64f * scale : 52f * scale;
            if (emblem != null)
            {
                Place(emblem, 0.5f, 0.5f, 0.5f, 0.5f, 0, i == 0 ? 4f : 2f, eSize, eSize);
                emblem.SetAsFirstSibling();
            }

            // Cooldown timer + overlay + lock
            if (i > 0 && hud.txtNormalCooldownTimers.Length > i-1 && hud.txtNormalCooldownTimers[i-1] != null)
            {
                var tmr = hud.txtNormalCooldownTimers[i-1];
                Fill(tmr.rectTransform, 2, 2, 2, 2);
                Text(tmr, 20, TextAnchor.MiddleCenter);
                tmr.transform.SetAsLastSibling();
            }
            if (i > 0 && hud.imgNormalCooldownOverlays.Length > i-1 && hud.imgNormalCooldownOverlays[i-1] != null)
                Fill(hud.imgNormalCooldownOverlays[i-1].rectTransform, 0, 0, 0, 0);
            if (i > 0 && hud.lockOverlays.Length > i-1 && hud.lockOverlays[i-1] != null)
                Fill((RectTransform)hud.lockOverlays[i-1].transform, 0, 0, 0, 0);
        }
    }

    public void ToggleSecondary() { if(IsSecondaryOpen)CloseSecondary();else OpenSecondary(); }
    public void OpenSecondary()
    {
        if(drawer!=null&&drawer.IsBlockingGameplay)drawer.CloseActiveDrawer();
        secondaryOverlay.SetActive(true);menuGrid.SetActive(true);detailsPage.SetActive(false);menuTitle.text="Đạo lộ";
        SetDockState();
        if(logPanel!=null)logPanel.gameObject.SetActive(false);if(buffPanel!=null)buffPanel.gameObject.SetActive(false);
        secondaryOverlay.transform.SetAsLastSibling();
    }
    public bool CloseTopLayer() { if(!IsSecondaryOpen)return false;CloseSecondary();return true; }
    public void CloseSecondary()
    {
        if(secondaryOverlay!=null)secondaryOverlay.SetActive(false);
        if(logPanel!=null)logPanel.gameObject.SetActive(false);
        if(buffPanel!=null)buffPanel.gameObject.SetActive(false);
        SetDockState();
    }
    void SetDockState()
    {
        if(dockGroup==null&&dock!=null)dockGroup=dock.GetComponent<CanvasGroup>();
        if(dockGroup==null)return;
        bool enabled=!IsFocusMode&&!IsSecondaryOpen;
        dockGroup.interactable=enabled;dockGroup.blocksRaycasts=enabled;
        dockGroup.alpha=IsFocusMode?0:IsSecondaryOpen?.38f:1;
    }
    void OpenMilestones(){CloseSecondary();TutorialManager.Instance?.OpenMilestoneModal();}
    void OpenProfile(){CloseSecondary();CharacterDetailModalUI.Open();}
    string detailsKind;
    void Detail(string title,string kind)
    {OpenSecondary();menuGrid.SetActive(false);detailsPage.SetActive(true);menuTitle.text=title;detailsKind=kind;RefreshDetails();}
    void ShowWallet(){Detail("Tài nguyên","wallet");}
    void ShowBuffs(){Detail("Hiệu ứng đang có","buffs");}
    void ShowLog()
    {
        Detail("Nhật ký chiến đấu","log");detailsText.text="";
        if(logPanel!=null){logPanel.gameObject.SetActive(true);logPanel.SetAsLastSibling();}
    }
    void RefreshDetails()
    {
        if(detailsKind=="wallet")
        {
            var e=EconomyManager.Instance;var a=ArtifactService.Instance;
            detailsText.text=e==null?"Chưa có dữ liệu tài nguyên.":"Linh Thạch\n"+e.currentLinhThach.ToString("N0")+"\n\nCơ Duyên\n"+e.currentCoDuyen.ToString("N0")+"\n\nQuặng\n"+(a!=null?a.Ore.ToString("N0"):"0");
        }
        if(detailsKind=="buffs")
        {
            int count=0;var shop=ShopManager.Instance;if(shop!=null)for(int i=0;i<5;i++)if(shop.GetTimerByIndex(i)>0)count++;
            detailsText.text=count==0?"Chưa có hiệu ứng hỗ trợ đang hoạt động.":"";
            if(buffPanel!=null){buffPanel.gameObject.SetActive(true);buffPanel.SetAsLastSibling();}
        }
    }

    public static void Place(RectTransform r,float ax,float ay,float bx,float by,float x,float y,float w,float h)
    {if(r==null)return;var min=new Vector2(ax,ay);var max=new Vector2(bx,by);var pos=new Vector2(x,y);var size=new Vector2(w,h);if(r.anchorMin!=min)r.anchorMin=min;if(r.anchorMax!=max)r.anchorMax=max;var pivot=new Vector2(ax==bx?ax:.5f,ay==by?ay:.5f);if(r.pivot!=pivot)r.pivot=pivot;if(r.sizeDelta!=size)r.sizeDelta=size;if(r.anchoredPosition!=pos)r.anchoredPosition=pos;}
    public static void Fill(RectTransform r,float l,float b,float right,float top)
    {if(r==null)return;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(l,b);r.offsetMax=new Vector2(-right,-top);}
    void Text(UnityEngine.UI.Text t,int size,TextAnchor alignment)
    {if(t==null)return;t.font=regularFont;t.fontSize=size;t.resizeTextForBestFit=false;t.alignment=alignment;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=t.GetComponent<UnityEngine.UI.Button>()!=null;}
}

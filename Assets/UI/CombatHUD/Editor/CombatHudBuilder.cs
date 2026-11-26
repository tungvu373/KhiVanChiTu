using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.CombatHUD.Editor
{
    public static class CombatHudBuilder
    {
        public const string ScenePath="Assets/Scenes/SampleScene_MobileWork.unity";
        public const string Evidence="Assets/.docs/UIUX_Phase2_20261117/Evidence";
        [MenuItem("TuTien/UI Combat HUD/1. Apply to Mobile Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserve unsaved scene edits before applying HUD.");
            if(SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var h=UnityEngine.Object.FindAnyObjectByType<PortraitHudController>(FindObjectsInactive.Include);
            if(h==null||h.gameObject.scene.path!=ScenePath)throw new InvalidOperationException("Mobile HUD binding root missing.");
            if(h.GetComponent<CombatHudPresentation>()!=null){Debug.Log("[Combat HUD] Already applied. Existing control identity preserved.");return;}
            var theme=AssetDatabase.LoadAssetAtPath<FoundationTheme>("Assets/UI/Foundation/Theme/JadeFoundation.asset");
            if(theme==null||theme.bodyFont==null)throw new InvalidOperationException("Phase 1 foundation theme/fonts required.");
            Directory.CreateDirectory(Evidence);
            var before=h.GetComponentsInChildren<UnityEngine.UI.Button>(true).Select(b=>new{b.name,id=GlobalObjectId.GetGlobalObjectIdSlow(b).ToString()}).ToArray();
            File.WriteAllText(Evidence+"/buttons-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(before,Newtonsoft.Json.Formatting.Indented));
            Undo.IncrementCurrentGroup();Undo.SetCurrentGroupName("Phase 2 mobile combat HUD presentation");
            var p=Undo.AddComponent<CombatHudPresentation>(h.gameObject);p.legacy=h;p.theme=theme;p.damageMaterial=DamageMaterial(theme);p.drawer=h.GetComponentInParent<MobileDrawerUIController>();
            Undo.RecordObject(h,"Delegate mobile HUD layout");h.externalLayout=true;
            var f=new FoundationFactory(theme);
            // Every source object/reference remains live; suppress only its old graphics.
            Hidden(h.realm);Hidden(h.quest);Hidden(h.stage);Hidden(h.vitals);
            foreach(var old in new[]{h.profile,h.wallet})ClearGraphics(old,true);
            Skin(h.profile,f,theme.surface);Skin(h.wallet,f,theme.surface);
            p.character=Label(f,h.profile,"P2Character","\u0110\u1ea1o h\u1eefu",34);p.character.font=theme.headingFont;
            PortraitHudController.Place(p.character.rectTransform,0,1,1,1,0,-8,-48,44);p.character.textWrappingMode=TextWrappingModes.NoWrap;p.character.overflowMode=TextOverflowModes.Ellipsis;
            p.realmCaption=Label(f,h.profile,"P2Realm","Luy\u1ec7n Kh\u00ed",26);p.realmCaption.color=theme.muted;
            PortraitHudController.Place(p.realmCaption.rectTransform,0,0,1,0,0,12,-48,36);p.realmCaption.textWrappingMode=TextWrappingModes.NoWrap;p.realmCaption.overflowMode=TextOverflowModes.Ellipsis;
            p.cultivation=f.Progress(h.profile);FoundationFactory.Fill((RectTransform)p.cultivation.transform,24,2,24,96);p.cultivation.label.gameObject.SetActive(false);p.cultivation.fill.GetComponent<UnityEngine.UI.Image>().color=theme.gold;
            p.wallet=Label(f,h.wallet,"P2Wallet","LT  0\nCD  0",28);p.wallet.color=theme.gold;
            p.pause=h.upper.Find("PauseButton") as RectTransform;
            if(p.pause==null)p.pause=h.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name=="PauseButton").transform as RectTransform;
            SkinButton(p.pause,f,"D\u1eebng",28);SkinButton((RectTransform)h.menuButton.transform,f,"Menu",28);
            p.focusLabel=SkinButton(h.focusButton,f,"G\u1ecdn",28);
            p.task=FoundationFactory.Rect("P2MainObjective",h.upper);f.Surface(p.task,theme.surface,true);
            var taskButton=p.task.gameObject.AddComponent<UnityEngine.UI.Button>();taskButton.targetGraphic=p.task.GetComponent<UnityEngine.UI.Image>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(taskButton.onClick,p.OpenObjectives);
            p.objective=Label(f,p.task,"P2Objective","Nhi\u1ec7m v\u1ee5 ch\u00ednh",28);p.objective.overflowMode=TextOverflowModes.Ellipsis;
            var claimSource=h.quest.Find("BtnClaimQuick")?.GetComponent<UnityEngine.UI.Button>();
            if(claimSource!=null)
            {
                p.questClaim=claimSource;
                var claim=f.Action("P2ClaimObjective",p.task,"Nh\u1eadn");var rt=(RectTransform)claim.transform;
                PortraitHudController.Place(rt,1,0,1,1,0,0,120,0);claim.label.fontSize=24;
                UnityEditor.Events.UnityEventTools.AddPersistentListener(claim.clicked,p.ClaimObjective);
                FoundationFactory.Fill(p.objective.rectTransform,24,4,144,4);
            }
            p.wave=FoundationFactory.Rect("P2Wave",h.upper);p.waveTitle=Label(f,p.wave,"P2WaveTitle","T\u1ea7ng 1 \u00b7 0 / 10",28);
            p.waveProgress=f.Progress(p.wave);FoundationFactory.Fill((RectTransform)p.waveProgress.transform,0,0,0,44);p.waveProgress.label.gameObject.SetActive(false);
            FoundationFactory.Fill(p.waveTitle.rectTransform,12,16,12,0);
            p.boss=FoundationFactory.Rect("P2BossEvent",h.upper);f.Surface(p.boss,theme.surface);
            p.bossTitle=Label(f,p.boss,"P2BossTitle","BOSS",28);FoundationFactory.Fill(p.bossTitle.rectTransform,24,36,200,4);p.bossTitle.color=theme.gold;
            p.timer=Label(f,p.boss,"P2BattleTimer","30s",32);p.timer.alignment=TextAlignmentOptions.MidlineRight;FoundationFactory.Fill(p.timer.rectTransform,0,30,24,4);p.timer.rectTransform.anchorMin=new Vector2(.7f,0);
            p.bossHealth=f.Progress(p.boss);FoundationFactory.Fill((RectTransform)p.bossHealth.transform,24,12,24,60);p.bossHealth.label.fontSize=20;p.bossHealth.fill.GetComponent<UnityEngine.UI.Image>().color=new Color32(137,63,54,255);
            p.vitalRoot=FoundationFactory.Rect("P2VitalBars",h.lower);p.hp=f.Progress(p.vitalRoot);p.mp=f.Progress(p.vitalRoot);
            foreach(var bar in new[]{p.hp,p.mp}){bar.label.fontSize=26;bar.label.font=theme.headingFont;}
            p.mp.fill.GetComponent<UnityEngine.UI.Image>().color=new Color32(37,94,134,255);
            var sourceSkills=h.skills.GetComponent<MobileSkillHUD>();
            var buttons=new[]{sourceSkills.btnUltimate,sourceSkills.btnNormalSkills[0],sourceSkills.btnNormalSkills[1],sourceSkills.btnNormalSkills[2]};
            string[] names={"V\u1ea1n ki\u1ebfm","\u1ea4n ch\u01b0\u1edfng","L\u00f4i ph\u1ea1t","\u00c2m d\u01b0\u01a1ng"};
            string[] arts={"VanKiemQuyTong","ChuongAn","SamSet","PhapTran"};p.skillViews=new CombatHudPresentation.SkillView[4];
            for(int i=0;i<4;i++)
            {
                var b=buttons[i];ClearGraphics((RectTransform)b.transform,true);Skin((RectTransform)b.transform,f,theme.raised);
                var v=new CombatHudPresentation.SkillView{button=b};p.skillViews[i]=v;
                v.title=Label(f,b.transform,"P2SkillTitle",names[i],28);v.title.alignment=TextAlignmentOptions.Center;
                PortraitHudController.Place(v.title.rectTransform,0,1,1,1,0,-4,-12,36);
                var icon=FoundationFactory.Rect("P2SkillIcon",b.transform);v.icon=f.Surface(icon,Color.white);v.icon.type=UnityEngine.UI.Image.Type.Simple;v.icon.preserveAspect=true;
                v.icon.sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Import/Asset/Skills/"+arts[i]+".png").OfType<Sprite>().FirstOrDefault();
                var mask=FoundationFactory.Rect("P2CooldownRadial",b.transform);v.cooldown=f.Surface(mask,new Color(0,.02f,.03f,.78f));v.cooldown.type=UnityEngine.UI.Image.Type.Filled;v.cooldown.fillMethod=UnityEngine.UI.Image.FillMethod.Radial360;v.cooldown.fillOrigin=2;v.cooldown.fillClockwise=false;
                v.state=Label(f,b.transform,"P2SkillState","Kh\u00f3a",26);v.state.alignment=TextAlignmentOptions.Center;
                PortraitHudController.Place(v.state.rectTransform,0,0,1,0,0,4,-8,32);
            }
            h.quick.SetParent(h.menuGrid.transform,false);
            foreach(var b in h.quickButtons)SkinButton((RectTransform)b.transform,f,b.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text??b.name,36);
            foreach(var b in new[]{h.profileButton,h.achievementButton,h.buffsButton,h.logButton})SkinButton((RectTransform)b.transform,f,b.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text??b.name,36);
            foreach(var b in p.drawer.dockButtons)
            {
                SkinButton((RectTransform)b.transform,f,b.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text??b.name,25);
            }
            var legacyLayout=p.drawer.GetComponent<JadeUIRuntimeLayout>();if(legacyLayout!=null){Undo.RecordObject(legacyLayout,"Retire old HUD layout writer");legacyLayout.enabled=false;}
            p.ConfigureAppearance();p.ApplyLayout();p.boss.gameObject.SetActive(false);h.secondaryOverlay.SetActive(false);
            foreach(var graphic in h.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))if(graphic.GetComponent<UnityEngine.UI.Button>()==null&&graphic.transform!=h.profile)graphic.raycastTarget=false;
            foreach(var graphic in h.GetComponentsInChildren<TMP_Text>(true))graphic.raycastTarget=false;
            foreach(var b in h.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(b.targetGraphic!=null)b.targetGraphic.raycastTarget=true;
            EditorUtility.SetDirty(h);EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveScene(h.gameObject.scene);AssetDatabase.SaveAssets();Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            var ids=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include).ToDictionary(b=>GlobalObjectId.GetGlobalObjectIdSlow(b).ToString(),b=>b.name);
            if(before.Any(b=>!ids.ContainsKey(b.id)))throw new InvalidOperationException("An original button ID was lost.");
            Debug.Log("[Combat HUD] Applied mobile-only HUD. Original button IDs retained; no gameplay/service/save commands executed.");
        }
        [MenuItem("TuTien/UI Combat HUD/3. Refresh Presentation Styling")]
        public static void RefreshStyling()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open mobile scene in Edit Mode.");
            var p=UnityEngine.Object.FindAnyObjectByType<CombatHudPresentation>(FindObjectsInactive.Include);
            if(p==null)throw new InvalidOperationException("Apply the HUD first.");
            Undo.RecordObject(p,"Refresh Phase 2 HUD styles");p.damageMaterial=DamageMaterial(p.theme);p.ConfigureAppearance();p.ApplyLayout();EditorUtility.SetDirty(p);AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);
            Debug.Log("[Combat HUD] Presentation refreshed; original controllers and events retained.");
        }
        static Material DamageMaterial(FoundationTheme theme)
        {
            const string folder="Assets/UI/CombatHUD/Art",path=folder+"/JadeDamageOverlay.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material!=null)return material;
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/UI/CombatHUD","Art");
            var shader=Shader.Find("TextMeshPro/Mobile/Distance Field Overlay");if(shader==null)throw new InvalidOperationException("TMP mobile overlay shader missing.");
            material=new Material(theme.headingFont.material){name="JadeDamageOverlay",shader=shader};
            material.SetColor("_OutlineColor",new Color(.015f,.025f,.025f,.9f));material.SetFloat("_OutlineWidth",.12f);
            AssetDatabase.CreateAsset(material,path);return material;
        }
        static TMP_Text Label(FoundationFactory f,Transform parent,string name,string value,int size)
        {var t=f.Text(name,parent,value,size);FoundationFactory.Fill(t.rectTransform,24,8,24,8);return t;}
        static void Hidden(RectTransform r)
        {if(r==null)return;var group=r.GetComponent<CanvasGroup>();if(group==null)group=r.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;group.interactable=false;}
        static void ClearGraphics(RectTransform r,bool keepRoot)
        {
            foreach(var g in r.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))if(!keepRoot||g.transform!=r)g.enabled=false;
        }
        static void Skin(RectTransform r,FoundationFactory f,Color color)
        {
            var image=r.GetComponent<UnityEngine.UI.Image>();if(image==null)image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.enabled=true;image.sprite=f.theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=color;
        }
        static TMP_Text SkinButton(RectTransform r,FoundationFactory f,string label,int size)
        {
            ClearGraphics(r,true);Skin(r,f,f.theme.raised);var text=Label(f,r,"P2Label",label,size);text.alignment=TextAlignmentOptions.Center;
            var b=r.GetComponent<UnityEngine.UI.Button>();if(b!=null){b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};}
            return text;
        }
    }
}

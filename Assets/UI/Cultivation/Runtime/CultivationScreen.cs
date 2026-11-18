using System;
using System.Linq;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;

namespace KhiVanChiTu.UI.Cultivation
{
    public enum CultivationScreenKind { Body, Alchemy, Artifact, Formation }
    public enum CultivationActionState { Available, InsufficientResources, Locked, InProgress, MaxLevel }
    [DefaultExecutionOrder(12000)]
    public sealed class CultivationScreen : MonoBehaviour
    {
        public CultivationScreenKind kind;
        public FoundationTheme theme;
        public FoundationAction[] selectors;
        public FoundationAction primary,secondary,tertiary;
        public TMP_Text subject,level,current,next,materials,condition,explanation,feedback,wallet,result;
        public FoundationProgress progress;
        public UnityEngine.UI.Image subjectArt;
        public UnityEngine.UI.ScrollRect scroll;
        public int Selected {get;private set;}
        public CultivationActionState State {get;private set;}
        float nextTick;
        bool observedBrew;
        int brewingRealm,brewingRecipe,previousDust;
        int[] previousPills;
        double fullBrewSeconds;
        static readonly string[] ArtifactNames={"Phi Thiên Áo","Cửu Thiên Tiên Miện","Vân Phong Hài","Linh Ngọc Bội"};
        static readonly string[] BodyNames={"Khí Huyết","Cương Khí","Dũ Hợp","Kình Lực","Phệ Huyết","Định Tâm"};
        static readonly string[] RecipeNames={"Tu Vi Đan","Đột Phá Đan"};
        public string[] Names=>kind==CultivationScreenKind.Body?BodyNames:kind==CultivationScreenKind.Alchemy?RecipeNames:kind==CultivationScreenKind.Artifact?ArtifactNames:FormationPanelUI.FormationNames;
        void Awake()
        {
            for(int i=0;i<selectors.Length;i++){int n=i;selectors[i].Bind(Names[i],()=>Select(n));}
            primary.Bind("Nâng cấp",Primary);secondary.Bind("",Secondary);tertiary.Bind("",Tertiary);
        }
        void OnEnable()
        {
            if(PlayerStatsService.Instance!=null)PlayerStatsService.Instance.OnStatsChanged+=Refresh;
            if(ArtifactService.Instance!=null)ArtifactService.Instance.OnChanged+=Refresh;
            if(FormationService.Instance!=null)FormationService.Instance.OnChanged+=Refresh;
            if(AlchemyService.Instance!=null)AlchemyService.Instance.OnChanged+=Refresh;
            if(EconomyManager.Instance!=null)EconomyManager.Instance.OnLinhThachChanged+=Currency;
            if(FeatureUnlockService.Instance!=null)FeatureUnlockService.Instance.OnUnlockStateChanged+=Refresh;
            if(CultivationManager.Instance!=null)CultivationManager.Instance.OnStageChanged+=Stage;
            if(primary!=null)Refresh();
        }
        void OnDisable()
        {
            if(PlayerStatsService.Instance!=null)PlayerStatsService.Instance.OnStatsChanged-=Refresh;
            if(ArtifactService.Instance!=null)ArtifactService.Instance.OnChanged-=Refresh;
            if(FormationService.Instance!=null)FormationService.Instance.OnChanged-=Refresh;
            if(AlchemyService.Instance!=null)AlchemyService.Instance.OnChanged-=Refresh;
            if(EconomyManager.Instance!=null)EconomyManager.Instance.OnLinhThachChanged-=Currency;
            if(FeatureUnlockService.Instance!=null)FeatureUnlockService.Instance.OnUnlockStateChanged-=Refresh;
            if(CultivationManager.Instance!=null)CultivationManager.Instance.OnStageChanged-=Stage;
        }
        void Currency(float _)=>Refresh();void Stage(CultivationStageData _)=>Refresh();
        void Update(){if(Time.unscaledTime<nextTick)return;nextTick=Time.unscaledTime+.2f;if(kind==CultivationScreenKind.Alchemy||kind==CultivationScreenKind.Formation)Refresh();}
        public void Select(int index){Selected=Mathf.Clamp(index,0,Names.Length-1);feedback.text="";Refresh();}
        void SetState(bool unlocked,bool max,bool busy,bool enough,string available,string lockReason="")
        {
            State=!unlocked?CultivationActionState.Locked:max?CultivationActionState.MaxLevel:busy?CultivationActionState.InProgress:!enough?CultivationActionState.InsufficientResources:CultivationActionState.Available;
            primary.SetAvailable(State==CultivationActionState.Available);
            primary.label.text=State==CultivationActionState.Locked?"Chưa mở khóa":State==CultivationActionState.MaxLevel?"Đã đạt tối đa":State==CultivationActionState.InProgress?"Đang luyện…":State==CultivationActionState.InsufficientResources?"Thiếu nguyên liệu":available;
            primary.button.targetGraphic.color=State==CultivationActionState.Available?theme.gold:theme.raised;primary.label.color=State==CultivationActionState.Available?theme.ink:theme.muted;
            condition.text=State==CultivationActionState.Locked?lockReason:State==CultivationActionState.MaxLevel?"Đã đạt cấp tối đa của hệ thống.":State==CultivationActionState.InProgress?"Lò đang hoạt động. Có thể chờ hoặc dùng Bụi Dược.":State==CultivationActionState.InsufficientResources?"Chưa đủ tài nguyên · Xem số đang có và số cần bên dưới.":"Sẵn sàng · Chi phí được trừ khi thực hiện thành công.";
        }
        public void Refresh()
        {
            if(theme==null||primary==null)return;
            float stone=EconomyManager.Instance?.currentLinhThach??0;
            wallet.text=$"LINH THẠCH  {RpgUI.Number(stone)}     ·     QUẶNG  {ArtifactService.Instance?.Ore??0:N0}";
            for(int i=0;i<selectors.Length;i++){selectors[i].button.targetGraphic.color=i==Selected?theme.raised:theme.surface;selectors[i].label.color=i==Selected?theme.gold:theme.ivory;}
            subject.text=Names[Selected];
            secondary.gameObject.SetActive(true);tertiary.gameObject.SetActive(true);
            switch(kind)
            {
                case CultivationScreenKind.Body:RefreshBody(stone);break;
                case CultivationScreenKind.Alchemy:RefreshAlchemy();break;
                case CultivationScreenKind.Artifact:RefreshArtifact(stone);break;
                case CultivationScreenKind.Formation:RefreshFormation(stone);break;
            }
            Fit(current,124);Fit(next,124);Fit(materials,112);Fit(condition,90);Fit(explanation,128);Fit(result,72);
        }
        static void Fit(TMP_Text text,float min)
        {if(text==null)return;var le=text.GetComponent<UnityEngine.UI.LayoutElement>();if(le!=null)le.preferredHeight=Mathf.Max(min,text.GetPreferredValues(text.text,Mathf.Max(120,text.rectTransform.rect.width),10000).y+24);}
        void RefreshBody(float stone)
        {
            var s=PlayerStatsService.Instance;if(s==null)return;int l=s.GetLevel(Selected);bool unlocked=s.IsNodeUnlocked(Selected);var node=PlayerStatsService.TalentNodes[Selected];float cost=s.GetUpgradeCost(Selected);
            level.text=$"Cấp {l:N0} · {node.branchName} · Tầng {node.tier}";
            current.text="HIỆN TẠI\n"+s.GetCurrentEffectString(Selected);next.text="SAU MỘT CẤP\n"+(unlocked?s.GetNextEffectString(Selected):"Mở khóa để tu luyện nhánh này");
            materials.text=$"LINH THẠCH\nĐang có {RpgUI.Number(stone)}  /  Cần {RpgUI.Number(cost)}";
            explanation.text=node.statPerLevelText+"\n\n"+node.combatMechanics;
            SetState(unlocked,false,false,stone>=cost,"Nâng 1 cấp",s.GetUnlockRequirementText(Selected));
            secondary.label.text="Nâng đến 5 cấp";tertiary.label.text="Nâng đến 50 cấp";secondary.SetAvailable(unlocked&&stone>=cost);tertiary.SetAvailable(unlocked&&stone>=cost);
            if(State==CultivationActionState.Available)primary.label.text+=" · "+RpgUI.Number(cost)+" LT";
            result.text=$"Tổng thể phách: {s.GetTotalBodyLevel():N0} cấp · Nâng nhiều cấp mua lần lượt theo tài nguyên đang có.";
            if(node.parentIndex>=0){int p=s.GetLevel(node.parentIndex);progress.Set(p,node.requiredParentLevel,$"Điều kiện nhánh: {PlayerStatsService.TalentNodes[node.parentIndex].name} {p}/{node.requiredParentLevel}");}
            else progress.Set(Mathf.Min(l,5),5,$"Mốc mở nhánh tiếp: {Mathf.Min(l,5)}/5 cấp");
            RpgEmblem.Show(subjectArt,RpgEmblemKind.Body,Selected<3?theme.jade:theme.gold);
        }
        void RefreshArtifact(float stone)
        {
            var s=ArtifactService.Instance;if(s==null)return;var slot=(ArtifactSlot)Selected;int l=s.GetLevel(slot);bool max=l>=ArtifactService.MaxLevel;bool unlocked=FeatureUnlockService.Instance==null||FeatureUnlockService.Instance.IsArtifactSlotUnlocked(slot);int ore=s.GetOreCost(slot);float cost=s.GetStoneCost(slot);
            level.text=$"Cấp {l}/{ArtifactService.MaxLevel} · Hào quang bậc {s.AuraTier}";
            current.text="HIỆN TẠI\n"+ArtifactPanelUI.GetSlotStatsFormatted(slot,l);next.text=max?"ĐÃ ĐẠT CỰC HẠN":"SAU MỘT CẤP\n"+ArtifactPanelUI.GetSlotStatsFormatted(slot,l+1);
            materials.text=max?"Không cần thêm nguyên liệu.":$"QUẶNG PHÔI   {s.Ore:N0} / {ore:N0}\nLINH THẠCH   {RpgUI.Number(stone)} / {RpgUI.Number(cost)}";
            explanation.text="Pháp khí đã chế tạo tự động cộng chỉ số. Hào quang cộng từ các mốc 3 cấp của từng món, tối đa bậc 9.";
            SetState(unlocked,max,false,s.Ore>=ore&&stone>=cost,l==0?"Chế tạo pháp khí":"Cường hóa",FeatureUnlockService.Instance?.GetArtifactSlotUnlockRequirementText(slot));
            if(State==CultivationActionState.Available)primary.label.text+=$"\n{ore:N0} quặng · {RpgUI.Number(cost)} LT";
            secondary.gameObject.SetActive(false);tertiary.gameObject.SetActive(false);result.text="Các món đã chế tạo tự động phát huy chỉ số.";
            progress.Set(l,ArtifactService.MaxLevel,$"Tiến độ cường hóa · {l}/{ArtifactService.MaxLevel}");RpgEmblem.Show(subjectArt,(RpgEmblemKind)((int)RpgEmblemKind.Robe+Selected),theme.gold);
        }
        void RefreshAlchemy()
        {
            var s=AlchemyService.Instance;if(s==null)return;int realm=(int)(CultivationManager.Instance?.CurrentRealm??0);var recipe=(PillRecipe)Selected;bool unlocked=FeatureUnlockService.Instance==null||FeatureUnlockService.Instance.IsFeatureUnlocked(FeatureId.Alchemy);var costs=s.GetRecipeCosts(recipe,realm);bool enough=Enumerable.Range(0,3).All(i=>s.GetHerbCount(i)>=costs[i]);
            double duration=Math.Max(10,(30+realm*15-s.AlchemistRank*2)*DaoAnProgressionService.ReductionMultiplier(DaoAnEffect.AlchemyBrewSpeed));
            if(s.IsBrewing&&!observedBrew){observedBrew=true;brewingRealm=s.State.brewingRealm;brewingRecipe=s.State.brewingRecipe;fullBrewSeconds=duration;previousPills=(int[])s.State.pills.Clone();previousDust=s.State.medicineDust;}
            if(!s.IsBrewing&&observedBrew)
            {observedBrew=false;var quality=PillQuality.Waste;for(int q=1;q<4;q++)if(s.State.pills[brewingRealm*8+brewingRecipe*4+q]>previousPills[brewingRealm*8+brewingRecipe*4+q])quality=(PillQuality)q;feedback.text=quality==PillQuality.Waste?$"Xuất lò: Phế đan · +{s.State.medicineDust-previousDust} Bụi Dược":"Xuất lò: "+RpgUI.Quality(quality)+" "+RecipeNames[brewingRecipe];}
            level.text=$"Đan sư cấp {s.AlchemistRank}/10 · Kinh nghiệm {s.State.masteryXp:N0}";
            current.text=$"CÔNG THỨC ĐANG CHỌN\n{RecipeNames[Selected]} · Cảnh giới {CultivationManager.Instance?.GetDisplayRealmName()}\nThời gian dự kiến {TimeSpan.FromSeconds(duration):mm\\:ss}";
            next.text=recipe==PillRecipe.Cultivation?"KẾT QUẢ\nTu Vi Đan bổ sung tu vi khi sử dụng. Hiệu quả phụ thuộc phẩm chất và Đạo Ấn.":"KẾT QUẢ\nĐột Phá Đan dùng tại cổng độ kiếp của cảnh giới tương ứng.";
            string[] names={"LINH CHI","NGƯNG KHÍ","TUYẾT LIÊN"};materials.text=string.Join("\n",Enumerable.Range(0,3).Where(i=>costs[i]>0).Select(i=>$"{names[i]}   {s.GetHerbCount(i):N0} / {costs[i]:N0}"));
            SetState(unlocked,false,s.IsBrewing,enough,"Bắt đầu luyện",FeatureUnlockService.Instance?.GetFeatureUnlockRequirementText(FeatureId.Alchemy));
            double remain=s.BrewRemaining.TotalSeconds;progress.Set((float)(fullBrewSeconds-remain),(float)fullBrewSeconds,s.IsBrewing?$"{RecipeNames[s.State.brewingRecipe]} · Còn {TimeSpan.FromSeconds(remain):mm\\:ss}":"Lò luyện sẵn sàng");if(!s.IsBrewing)progress.Set(0,1,"Lò luyện sẵn sàng");
            secondary.label.text=$"Tốc hành · {s.DustFinishCost} bụi";secondary.SetAvailable(s.IsBrewing&&s.State.medicineDust>=s.DustFinishCost);tertiary.label.text="Uống Tu Vi Đan";tertiary.SetAvailable(CultivationManager.Instance!=null&&!CultivationManager.Instance.IsTuViMaxed()&&Enumerable.Range(1,3).Any(q=>s.GetPillCount(realm,PillRecipe.Cultivation,(PillQuality)q)>0));
            explanation.text=$"BỤI DƯỢC  {s.State.medicineDust:N0}\nPhẩm chất: Sơ phẩm · Trung phẩm · Cực phẩm. Luyện có thể tạo Phế đan; kết quả tự nhập kho.";
            result.text=string.Join("\n",Enumerable.Range(0,2).Select(r=>RecipeNames[r]+": "+string.Join(" · ",Enumerable.Range(1,3).Select(q=>$"{RpgUI.Quality((PillQuality)q)} {s.GetPillCount(realm,(PillRecipe)r,(PillQuality)q):N0}"))));
            subjectArt.enabled=true;RpgEmblem.Hide(subjectArt);
        }
        void RefreshFormation(float stone)
        {
            var s=FormationService.Instance;if(s==null)return;var id=(FormationId)Selected;int l=s.GetLevel(id);bool max=l>=FormationService.MaxLevel,unlocked=FeatureUnlockService.Instance==null||FeatureUnlockService.Instance.IsFormationUnlocked(id);int ore=s.GetOreCost(id);float cost=s.GetStoneCost(id);bool active=s.Selected==id&&l>0;
            level.text=$"Cấp {l}/{FormationService.MaxLevel} · "+(active?"Đang kích hoạt":l>0?"Đã học":"Chưa học");
            current.text="HIỆN TẠI\n"+FormationEffect(id,l);next.text=max?"ĐÃ ĐẠT CỰC HẠN":"SAU MỘT CẤP\n"+FormationEffect(id,l+1);
            materials.text=max?"Không cần thêm nguyên liệu.":$"QUẶNG PHÔI   {ArtifactService.Instance?.Ore??0:N0} / {ore:N0}\nLINH THẠCH   {RpgUI.Number(stone)} / {RpgUI.Number(cost)}";
            SetState(unlocked,max,false,(ArtifactService.Instance?.Ore??0)>=ore&&stone>=cost,l==0?"Học trận pháp":"Tu luyện trận pháp",FeatureUnlockService.Instance?.GetFormationUnlockRequirementText(id));
            if(State==CultivationActionState.Available)primary.label.text+=$"\n{ore:N0} quặng · {RpgUI.Number(cost)} LT";
            secondary.label.text=active?"Đang dùng":l>0?"Chọn trận":"Chưa học";secondary.SetAvailable(unlocked&&l>0&&!active);
            tertiary.gameObject.SetActive(id==FormationId.Spirit);bool can=unlocked&&active&&s.SpiritCooldown==TimeSpan.Zero&&!s.SpiritActive;tertiary.label.text=s.SpiritActive?"Tụ Linh đang tác dụng":s.SpiritCooldown>TimeSpan.Zero?$"Hồi {s.SpiritCooldown:mm\\:ss}":"Phát động Tụ Linh";tertiary.SetAvailable(can);
            progress.Set(l,FormationService.MaxLevel,$"Tiến độ tu luyện · {l}/{FormationService.MaxLevel}");explanation.text=id==FormationId.Spirit?"Chọn Tụ Linh rồi phát động để tăng tu vi. Thời gian tác dụng và hồi chiêu giữ theo hệ thống hiện tại.":"Chỉ trận đang chọn phát huy hiệu ứng. Sau khi học, dùng Chọn trận để kích hoạt.";
            result.text=s.SpiritActive?"Tụ Linh đang tác dụng · Tu vi ×"+s.CultivationMultiplier.ToString("0.##"):"Đang dùng: "+FormationPanelUI.FormationNames[(int)s.Selected];RpgEmblem.Show(subjectArt,RpgEmblemKind.Formation,Selected==1?theme.jade:theme.gold);
        }
        public static string FormationEffect(FormationId id,int l)
        {
            if(l<=0)return "Chưa học trận pháp";
            float stage=CultivationManager.Instance?.currentStageIndex??0;
            switch(id)
            {
                case FormationId.Thunder:return $"Sát thương sét {40*l*(1+stage*.15f):N0}\nPhạm vi {10+l*.25f:0.##}m";
                case FormationId.Spirit:return $"Tu vi khi phát động ×{2+(l-1)*.05f:0.##}\nTác dụng {30+(l-1)*2+DaoAnProgressionService.Bonus(DaoAnEffect.FormationDuration):0.##} giây";
                case FormationId.Frost:return $"Tốc độ Boss còn {Mathf.Max(.25f,.5f-(l-1)*.01f)*100:0.#}%";
                case FormationId.SwordRain:return $"Tốc độ kiếm ×{1.2f+(l-1)*.015f:0.###}\nSát thương mưa kiếm {30*l*(1+stage*.18f):N0}";
                case FormationId.BloodDrain:return $"Hút máu +{Mathf.Min(.2f,.05f+(l-1)*.005f)*100:0.#}%";
                default:return $"Giảm sát thương {Mathf.Min(.4f,.15f+(l-1)*.01f)*100:0.#}%";
            }
        }
        public void Primary()
        {
            bool ok=false;
            switch(kind){case CultivationScreenKind.Body:ok=PlayerStatsService.Instance.TryUpgrade(Selected);break;case CultivationScreenKind.Artifact:ok=ArtifactService.Instance.TryUpgrade((ArtifactSlot)Selected);break;case CultivationScreenKind.Formation:ok=FormationService.Instance.TryUpgrade((FormationId)Selected);break;case CultivationScreenKind.Alchemy:ok=AlchemyService.Instance.TryStartBrew((PillRecipe)Selected,(int)CultivationManager.Instance.CurrentRealm);break;}
            feedback.text=ok?(kind==CultivationScreenKind.Alchemy?"Đã nhóm lửa · Đang luyện "+Names[Selected]:"Thành công · "+Names[Selected]):"Chưa thể thực hiện. Kiểm tra điều kiện và tài nguyên.";Refresh();
        }
        public void Secondary()
        {if(kind==CultivationScreenKind.Body){int n=PlayerStatsService.Instance.TryUpgradeMultiple(Selected,5);feedback.text=$"Đã nâng {n} cấp.";}else if(kind==CultivationScreenKind.Formation){feedback.text=FormationService.Instance.TrySelect((FormationId)Selected)?"Đã chọn "+Names[Selected]:"Chưa thể chọn trận.";}else if(kind==CultivationScreenKind.Alchemy)AlchemyService.Instance.TryUseDustToFinish();Refresh();}
        public void Tertiary()
        {
            if(kind==CultivationScreenKind.Body){int n=PlayerStatsService.Instance.TryUpgradeMultiple(Selected,50);feedback.text=$"Đã nâng {n} cấp.";}
            else if(kind==CultivationScreenKind.Formation){feedback.text=FormationService.Instance.TryActivateSpirit()?"Tụ Linh đã phát động.":"Tụ Linh chưa sẵn sàng.";}
            else if(kind==CultivationScreenKind.Alchemy){int realm=(int)CultivationManager.Instance.CurrentRealm;bool used=false;for(int q=1;q<=3&&!used;q++)used=AlchemyService.Instance.TryUseCultivationPill(realm,(PillQuality)q);feedback.text=used?"Đã dùng Tu Vi Đan.":"Không có đan phù hợp hoặc tu vi đã đầy.";}Refresh();
        }
    }
}

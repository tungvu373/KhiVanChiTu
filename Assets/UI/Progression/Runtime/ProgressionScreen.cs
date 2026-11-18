using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Progression
{
    [DefaultExecutionOrder(12000)]
    public sealed class ProgressionScreen : MonoBehaviour
    {
        public bool talents;
        public FoundationTheme theme;
        public RectTransform surface,content;
        public ProgressionCard cardPrefab;
        public UnityEngine.UI.ScrollRect scroll,branchScroll,stateScroll;
        public TMP_Text heading,summary,wallet,feedback,empty;
        public FoundationProgress totalProgress;
        public FoundationAction close,milestoneTab,talentTab,primary,secondary;
        public FoundationAction[] branchButtons,stateButtons;
        public MilestoneModalUI milestoneOwner;
        readonly Dictionary<string,ProgressionCard> rows=new Dictionary<string,ProgressionCard>(StringComparer.Ordinal);
        public string Branch {get;private set;}="all";
        public int StateFilter {get;private set;}
        public int VisibleCount=>rows.Values.Count(r=>r.gameObject.activeSelf);
        public IEnumerable<ProgressionCard> Cards=>rows.Values;
        public int ActionableCount {get;private set;}
        string signature;
        float poll;
        float lastWidth;
        bool dirty=true;
        static readonly string[] TalentKeys={"all","wealth","sword","cultivation","alchemy","formation","body_artifact","body"};
        static readonly string[] TalentLabels={"Tất cả nhánh","Tài Lộc","Kiếm Đạo","Tu Luyện","Dược Linh","Pháp Trận","Pháp Khí","Luyện Thể"};
        static readonly string[] QuestKeys={"all","0","1","2","3"};
        static readonly string[] QuestLabels={"Tất cả mốc","Diệt quái","Độ kiếp","Rèn kiếm","Kiếm Ý"};
        public string[] BranchKeys=>talents?TalentKeys:QuestKeys;
        public string[] BranchLabels=>talents?TalentLabels:QuestLabels;
        void Awake()
        {
            close.Bind("Đóng",Close);
            milestoneTab.Bind("Mốc Tu Tiên",()=>Switch(false));talentTab.Bind("Thiên Phú Đạo Vận",()=>Switch(true));
            for(int i=0;i<branchButtons.Length;i++){int n=i;branchButtons[i].Bind(BranchLabels[i],()=>SetBranch(BranchKeys[n]));}
            string[] states={"Tất cả","Sẵn sàng","Đang tiến","Chưa mở","Đã xong"};
            for(int i=0;i<stateButtons.Length;i++){int n=i;stateButtons[i].Bind(states[i],()=>SetStateFilter(n));}
            primary.Bind("Nhận tất cả",ClaimAll);secondary.Bind("Thành tựu",()=>{Close();AchievementModalUI.Open();});
        }
        void OnEnable()
        {
            if(DaoAnProgressionService.Instance!=null)DaoAnProgressionService.Instance.OnChanged+=MarkDirty;
            if(MilestoneManager.Instance!=null)MilestoneManager.Instance.OnMilestonesUpdated+=MarkDirty;
            if(EconomyManager.Instance!=null)EconomyManager.Instance.OnLinhThachChanged+=Currency;
            if(CultivationManager.Instance!=null)CultivationManager.Instance.OnStageChanged+=Stage;
            dirty=true;
        }
        void OnDisable()
        {
            if(DaoAnProgressionService.Instance!=null)DaoAnProgressionService.Instance.OnChanged-=MarkDirty;
            if(MilestoneManager.Instance!=null)MilestoneManager.Instance.OnMilestonesUpdated-=MarkDirty;
            if(EconomyManager.Instance!=null)EconomyManager.Instance.OnLinhThachChanged-=Currency;
            if(CultivationManager.Instance!=null)CultivationManager.Instance.OnStageChanged-=Stage;
        }
        void Currency(float _)=>MarkDirty();void Stage(CultivationStageData _)=>MarkDirty();void MarkDirty()=>dirty=true;
        void LateUpdate()
        {
            if(!Application.isPlaying)return;
            if(surface.rect.width!=lastWidth){lastWidth=surface.rect.width;dirty=true;}
            if(!dirty&&Time.unscaledTime<poll)return;poll=Time.unscaledTime+.4f;
            string now=Fingerprint();if(!dirty&&signature==now)return;signature=now;dirty=false;Refresh();
        }
        string Fingerprint()
        {
            var economy=EconomyManager.Instance;string s=$"{economy?.currentLinhThach}|{CultivationManager.Instance?.currentStageIndex}";
            if(talents){var svc=DaoAnProgressionService.Instance;return s+"|"+(svc?.Catalog?.nodes==null?"":string.Join(",",svc.Catalog.nodes.Where(n=>n!=null).Select(n=>svc.GetLevel(n.id))));}
            var m=MilestoneManager.Instance;return s+"|"+(m==null?"":string.Join(",",m.allQuests.Select(q=>q.isClaimed+":"+m.GetCurrentProgress(q))));
        }
        public void Opened(){feedback.text="";Refresh();Canvas.ForceUpdateCanvases();scroll.StopMovement();scroll.verticalNormalizedPosition=1;}
        public void SetBranch(string value)
        {Branch=BranchKeys.Contains(value)?value:"all";Refresh();Canvas.ForceUpdateCanvases();scroll.StopMovement();scroll.verticalNormalizedPosition=1;}
        public void SetStateFilter(int value)
        {StateFilter=Mathf.Clamp(value,0,4);Refresh();Canvas.ForceUpdateCanvases();scroll.StopMovement();scroll.verticalNormalizedPosition=1;}
        public void Close()
        {if(talents)DaoAnModalUI.Instance?.Close();else milestoneOwner.Close();}
        void Switch(bool target)
        {if(target==talents)return;Close();if(target)DaoAnModalUI.Instance?.Open();else MilestoneModalUI.Instance?.Open();}
        public ProgressionCard FindCard(string id)=>rows.TryGetValue(id,out var row)?row:null;
        ProgressionCard Row(string id)
        {if(!rows.TryGetValue(id,out var row)){row=Instantiate(cardPrefab,content);row.name="ProgressionCard_"+id;rows.Add(id,row);}return row;}
        bool Visible(ProgressionState s,bool actionable)
        {return StateFilter==0||StateFilter==1&&actionable||StateFilter==2&&s==ProgressionState.InProgress||StateFilter==3&&s==ProgressionState.Locked||StateFilter==4&&s==ProgressionState.Completed;}
        static int Rank(ProgressionState s)=>s==ProgressionState.Claimable||s==ProgressionState.Available?0:s==ProgressionState.InProgress?1:s==ProgressionState.Locked?2:3;
        public void Refresh()
        {
            if(content==null||!Application.isPlaying)return;
            float position=scroll.verticalNormalizedPosition;
            var ordered=new List<(ProgressionCard row,int rank,int order)>();ActionableCount=0;int completed=0,total=0,levels=0,maxLevels=0;
            if(talents)
            {
                var service=DaoAnProgressionService.Instance;if(service?.Catalog?.nodes==null)return;
                foreach(var n in service.Catalog.nodes.Where(n=>n!=null))
                {
                    int level=service.GetLevel(n.id);bool max=level>=n.MaxLevel;bool can=service.CanPurchase(n.id);string reason=LockReason(n,service,level);bool locked=!max&&reason.Length>0;
                    var state=max?ProgressionState.Completed:locked?ProgressionState.Locked:can?ProgressionState.Available:ProgressionState.InProgress;
                    float cost=!max&&n.costs!=null&&level<n.costs.Length?n.costs[level]:0;
                    string status=max?"HOÀN THÀNH · Tối đa":locked?"CHƯA MỞ KHÓA":can?"CÓ THỂ NÂNG NGAY":"ĐANG PHÁT TRIỂN · Thiếu Linh Thạch";
                    string effects=$"HIỆN TẠI   {Bonus(n,level)}"+(max?"":$"\nCẤP KẾ   {Bonus(n,level+1)}");
                    string conditions=max?"Đã mua đủ các cấp trong dữ liệu hiện tại.":(reason.Length>0?reason+"\n":"")+$"CHI PHÍ   {RpgUI.Number(cost)} LT\nĐang có {RpgUI.Number(EconomyManager.Instance?.currentLinhThach??0)} LT";
                    var row=Row(n.id);string id=n.id;
                    row.Bind(id,n.displayName,$"Tầng {n.tier} · Cấp {level}/{n.MaxLevel}",n.description,status,effects,conditions,"",state,level,n.MaxLevel,$"Cấp thiên phú {level}/{n.MaxLevel}",max?"Đã tối đa":locked?"Chưa mở khóa":can?$"Nâng cấp · {RpgUI.Number(cost)} LT":"Chưa đủ Linh Thạch",can,()=>Purchase(id),theme);
                    Symbol(row,n.branch=="sword"?0:n.branch=="wealth"?1:n.branch=="cultivation"?2:n.branch=="alchemy"?3:4);
                    bool show=(Branch=="all"||string.Equals(n.branch,Branch,StringComparison.OrdinalIgnoreCase))&&Visible(state,can);row.gameObject.SetActive(show);ordered.Add((row,Rank(state),n.tier*1000+total));total++;levels+=level;maxLevels+=n.MaxLevel;if(max)completed++;if(can)ActionableCount++;
                }
                string rank=levels>=40?"Đạo Tổ":levels>=25?"Thông Huyền":levels>=15?"Tề Thiên":levels>=8?"Đại Thịnh":levels>=3?"Tiểu Thịnh":"Sơ Sinh";
                totalProgress.Set(levels,maxLevels,$"Đạo vị {rank} · {levels:N0}/{maxLevels:N0} cấp");
                summary.text=$"{ActionableCount} thiên phú có thể nâng ngay · {completed}/{total} đã tối đa";
                primary.gameObject.SetActive(false);secondary.gameObject.SetActive(false);
                wallet.text=$"LINH THẠCH  {RpgUI.Number(EconomyManager.Instance?.currentLinhThach??0)} · {CultivationManager.Instance?.GetDisplayRealmName()}";
            }
            else
            {
                var m=MilestoneManager.Instance;if(m==null)return;
                foreach(var q in m.allQuests)
                {
                    int p=m.GetCurrentProgress(q);bool can=!q.isClaimed&&m.IsCompleted(q);var state=q.isClaimed?ProgressionState.Completed:can?ProgressionState.Claimable:ProgressionState.InProgress;
                    var row=Row(q.id);string id=q.id;
                    row.Bind(id,q.title,(q.isHard?"Thử thách khó · ":"")+QuestLabels[(int)q.type+1],q.description,q.isClaimed?"HOÀN THÀNH · Đã nhận thưởng":can?"ĐỦ ĐIỀU KIỆN · Có thể nhận ngay":"ĐANG TIẾN HÀNH","","",Rewards(q),state,q.isClaimed?q.targetAmount:p,q.targetAmount,q.isClaimed?"Đã ghi nhận mốc":$"{p:N0} / {q.targetAmount:N0}",q.isClaimed?"Đã nhận":can?"Nhận thưởng":"Chưa đủ điều kiện",can,()=>Claim(id),theme);
                    Symbol(row,(int)q.type==0?0:(int)q.type==1?3:(int)q.type==2?0:2);
                    bool show=(Branch=="all"||Branch==((int)q.type).ToString())&&Visible(state,can);row.gameObject.SetActive(show);ordered.Add((row,Rank(state),total));total++;if(q.isClaimed)completed++;if(can)ActionableCount++;
                }
                totalProgress.Set(completed,total,$"Mốc đã nhận {completed}/{total}");summary.text=ActionableCount>0?$"{ActionableCount} phần thưởng đang chờ · Nhận để tiếp tục phát triển":"Tiếp tục diệt quái, độ kiếp và phát triển kiếm";
                primary.gameObject.SetActive(true);secondary.gameObject.SetActive(true);primary.label.text=ActionableCount>0?$"Nhận tất cả ({ActionableCount})":"Chưa có thưởng chờ";primary.SetAvailable(ActionableCount>0);
                wallet.text="Mốc được cập nhật theo tiến trình hiện tại · Thưởng đan áp dụng khi nhận";
            }
            foreach(var row in rows.Values)if(!ordered.Any(x=>x.row==row))row.gameObject.SetActive(false);
            foreach(var x in ordered.OrderBy(x=>x.rank).ThenBy(x=>x.order))x.row.transform.SetAsLastSibling();
            empty.gameObject.SetActive(VisibleCount==0);empty.text="Không có mục trong bộ lọc này.\nChọn Tất cả để xem các mục tiêu tiếp theo.";
            for(int i=0;i<branchButtons.Length;i++)Tint(branchButtons[i],Branch==BranchKeys[i]);for(int i=0;i<stateButtons.Length;i++)Tint(stateButtons[i],i==StateFilter);
            Tint(milestoneTab,!talents);Tint(talentTab,talents);dirty=false;signature=Fingerprint();Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=Mathf.Clamp01(position);
        }
        void Tint(FoundationAction a,bool selected){a.button.targetGraphic.color=selected?theme.raised:theme.surface;a.label.color=selected?theme.gold:theme.ivory;}
        static void Symbol(ProgressionCard row,int symbol){var icon=row.icon as NavigationIcon;if(icon!=null){icon.symbol=symbol;icon.SetAllDirty();}}
        public static string LockReason(DaoAnDefinition node,DaoAnProgressionService service,int level)
        {
            var reasons=new List<string>();if(!string.IsNullOrEmpty(node.prerequisiteId)&&(node.prerequisiteId==node.id||service.GetLevel(node.prerequisiteId)<Math.Max(1,node.prerequisiteLevel)))reasons.Add($"Cần {service.Catalog.Find(node.prerequisiteId)?.displayName??node.prerequisiteId} cấp {Math.Max(1,node.prerequisiteLevel)}");
            if(node.minRealms!=null&&level<node.minRealms.Length){var r=node.minRealms[level];int s=node.minSubLevels!=null&&level<node.minSubLevels.Length?node.minSubLevels[level]:1;if(!FeatureUnlockService.IsCultivationReached(r,s))reasons.Add($"Cần {CultivationDisplay.RealmName(r)} · Tầng {s}");}
            return string.Join("\n",reasons);
        }
        public static string Bonus(DaoAnDefinition n,int level)
        {string unit=(n.unit??"").Trim();float v=n.GetTotalBonus(level);return unit.StartsWith("%",StringComparison.Ordinal)?$"{v*100:0.##}% {unit.TrimStart('%').Trim()}":$"{v:0.##} {unit}";}
        static string Rewards(MilestoneQuest q)
        {
            var r=new List<string>();if(q.rewardLinhThach>0)r.Add($"+{RpgUI.Number(q.rewardLinhThach)} Linh Thạch");if(q.rewardCoDuyen>0)r.Add($"+{q.rewardCoDuyen:N0} Cơ Duyên");
            if(q.rewardDanDuocIndex>=0&&q.rewardDanDuocAmount>0){string[] n={"Hồi Linh Đan","Tật Phong Đan","Hồi Huyết Đan","Cuồng Nộ Đan","Tĩnh Tâm Đan"};r.Add((q.rewardDanDuocIndex<n.Length?n[q.rewardDanDuocIndex]:"Linh Đan")+$" ×{q.rewardDanDuocAmount:N0} · Hiệu lực áp dụng khi nhận");}if(q.rewardSwordLevel>0)r.Add($"Kiếm cấp {q.rewardSwordLevel}");return string.Join("\n",r);
        }
        void Purchase(string id)
        {bool ok=DaoAnProgressionService.Instance!=null&&DaoAnProgressionService.Instance.TryPurchase(id);feedback.text=ok?$"Đã nâng {DaoAnProgressionService.Instance.Catalog.Find(id).displayName} lên cấp {DaoAnProgressionService.Instance.GetLevel(id)}":"Điều kiện đã thay đổi. Kiểm tra tài nguyên và yêu cầu.";feedback.color=ok?theme.jade:theme.danger;Refresh();}
        void Claim(string id)
        {bool ok=MilestoneManager.Instance!=null&&MilestoneManager.Instance.ClaimReward(id);feedback.text=ok?"Đã nhận thưởng · "+MilestoneManager.Instance.allQuests.Find(q=>q.id==id).title:"Mốc chưa đủ điều kiện hoặc đã được nhận.";feedback.color=ok?theme.jade:theme.danger;Refresh();}
        void ClaimAll()
        {int before=MilestoneManager.Instance.allQuests.Count(q=>q.isClaimed);milestoneOwner.btnClaimAll.onClick.Invoke();int count=MilestoneManager.Instance.allQuests.Count(q=>q.isClaimed)-before;feedback.text=count>0?$"Đã nhận thưởng {count} mốc tu tiên.":"Không còn phần thưởng đủ điều kiện.";feedback.color=count>0?theme.jade:theme.muted;Refresh();}
    }
}

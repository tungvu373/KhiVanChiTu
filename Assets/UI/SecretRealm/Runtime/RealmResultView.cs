using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.SecretRealm
{
    public sealed class RealmResultView : MonoBehaviour
    {
        public FoundationTheme theme;
        public Sprite[] realmArt;
        public UnityEngine.UI.Image artwork;
        public TMP_Text title,status,score,note,receipt;
        public RealmRewardRow rowPrefab;
        public RectTransform rows;
        public UnityEngine.UI.ScrollRect scroll;
        public FoundationAction confirm;
        public CanvasGroup reveal;
        readonly List<RealmRewardRow> pool=new List<RealmRewardRow>();
        public int VisibleRewardCount {get;private set;}
        public void Show(SecretRealmType type,int points,SecretRealmService.RewardResult reward,Action close)
        {
            gameObject.SetActive(true);transform.SetAsLastSibling();artwork.sprite=realmArt[(int)type];RealmPlayView.FitArt(artwork);
            bool timed=type==SecretRealmType.HordeSurvival||type==SecretRealmType.BarrierBreak||type==SecretRealmType.LightningDodge;
            bool reached=type==SecretRealmType.HordeSurvival?points>=50:type==SecretRealmType.BarrierBreak?points>=100:type==SecretRealmType.LightningDodge?points>=20:points>0;
            status.text=timed?(reached?"MỤC TIÊU HOÀN THÀNH":"HẾT THỜI GIAN · CHƯA ĐẠT MỤC TIÊU"):type==SecretRealmType.DanCauldron?(points>0?"KHAI ĐỈNH · CỰC PHẨM":"KHAI ĐỈNH · DUYÊN THƯỜNG"):"CỔ KIẾM ĐÃ THỨC TỈNH";status.color=reached?theme.jade:theme.gold;
            title.text=RealmCopy.Names[(int)type];score.text=type==SecretRealmType.HordeSurvival?$"Đã trảm {points:N0}/50 yêu ma":type==SecretRealmType.BarrierBreak?$"Đã phá {points:N0}% cấm chế":type==SecretRealmType.LightningDodge?$"Đã thu {points:N0}/20 lôi tinh":type==SecretRealmType.SwordTomb?$"Phi kiếm cao nhất · Cấp {points:N0}":points>0?"Đã tìm đúng đỉnh theo sấm ngôn":"Đã khai mở đỉnh thảo dược";
            note.text=RealmCopy.Plain(reward.specialNote);note.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(note.text));VisibleRewardCount=0;
            Add("Linh thạch",reward.linhThach);Add("Tu vi",reward.tuVi);Add("Thảo dược · Bậc 1",reward.herbs0);Add("Thảo dược · Bậc 2",reward.herbs1);Add("Thảo dược · Bậc 3",reward.herbs2);Add("Quặng",reward.ore);Add("Cơ Duyên",reward.bonusCoDuyen);if(reward.swordLevel>0)Add("Phi kiếm",1,"Cấp "+reward.swordLevel);
            for(int i=VisibleRewardCount;i<pool.Count;i++)pool[i].gameObject.SetActive(false);
            note.transform.parent.SetAsLastSibling();
            receipt.text=VisibleRewardCount>0?"Phần thưởng đã được cộng vào hành trang.":"Lượt thử luyện đã kết thúc.";
            confirm.Bind("Hoàn tất · Trở về bí cảnh",close);Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;StopAllCoroutines();StartCoroutine(Reveal());
        }
        void Add(string caption,float amount,string value=null)
        {if(amount<=0)return;RealmRewardRow row;if(VisibleRewardCount<pool.Count)row=pool[VisibleRewardCount];else{row=Instantiate(rowPrefab,rows);pool.Add(row);}VisibleRewardCount++;row.gameObject.SetActive(true);row.label.text=caption;row.value.text=value??"+"+(amount>=1e12f?amount.ToString("0.##E+0"):EconomyManager.FormatNumber(amount));row.icon.color=theme.gold;}
        IEnumerator Reveal(){reveal.alpha=.35f;float elapsed=0;while(elapsed<.18f){elapsed+=Time.unscaledDeltaTime;reveal.alpha=Mathf.Lerp(.35f,1,Mathf.Clamp01(elapsed/.18f));yield return null;}reveal.alpha=1;}
    }

    // A readable Oni mask for tappable demons; it does not change the existing Button or its hit rectangle.
}

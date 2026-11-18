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
    public sealed class RealmHubView : MonoBehaviour
    {
        public FoundationTheme theme;
        public SecretRealmModalUI owner;
        public RealmCard[] cards;
        public TMP_Text wallet,access,summary;
        public FoundationAction close;
        public UnityEngine.UI.ScrollRect scroll;
        public RectTransform surface;
        void Awake(){close.Bind("Đóng",()=>owner.Close());foreach(var card in cards){var c=card;c.enter.Bind("Vào bí cảnh",()=>owner.EnterRealm(c.type));}}
        void OnEnable(){Refresh();}
        float next;
        void Update(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.3f;Refresh();}
        public void Refresh()
        {
            var s=SecretRealmService.Instance;if(s==null)return;bool free=s.HasFreeAttempt();bool ready=s.CanEnter();int cd=EconomyManager.Instance?.currentCoDuyen??0;
            wallet.text=$"CƠ DUYÊN  {cd:N0}";access.text=free?"Còn 1 lượt miễn phí hôm nay":$"Mỗi lượt: {SecretRealmService.CO_DUYEN_ENTRY_COST} Cơ Duyên";summary.text=$"5 kỳ ngộ thượng cổ  ·  Đã hoàn tất {s.State.totalCleared:N0} lượt";
            foreach(var c in cards)
            {
                c.condition.text=free?"ĐÃ MỞ · Lượt miễn phí sẵn sàng":ready?"ĐÃ MỞ · Đủ Cơ Duyên tham gia":"CHƯA ĐỦ CƠ DUYÊN · Cần 5 để tham gia";c.condition.color=ready?theme.jade:theme.muted;
                c.enter.label.text=free?"Vào bí cảnh · Miễn phí":ready?"Vào bí cảnh · 5 Cơ Duyên":"Cần 5 Cơ Duyên";c.enter.SetAvailable(ready);c.enter.button.targetGraphic.color=ready?theme.gold:theme.raised;c.enter.label.color=ready?theme.ink:theme.muted;
                switch(c.type){case SecretRealmType.HordeSurvival:c.record.text=$"Kỷ lục {s.State.hordeRecord:N0}/50 yêu ma";break;case SecretRealmType.BarrierBreak:c.record.text=$"Kỷ lục phá {s.State.barrierRecord:N0}%";break;case SecretRealmType.DanCauldron:c.record.text=$"Tìm đúng đỉnh {s.State.cauldronPillsFound:N0} lần";break;case SecretRealmType.LightningDodge:c.record.text=$"Kỷ lục {s.State.lightningOrbsRecord:N0}/20 lôi tinh";break;default:c.record.text=$"Kiếm cao nhất: cấp {s.State.swordTombMaxLevel:N0}";break;}
            }
        }
        public void Focus(SecretRealmType type)
        {
            Canvas.ForceUpdateCanvases();scroll.StopMovement();float travel=Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height);
            var card=(RectTransform)cards[(int)type].transform;float top=-card.anchoredPosition.y-card.rect.yMax;scroll.verticalNormalizedPosition=travel>0?1-Mathf.Clamp01(top/travel):1;
        }
    }
}

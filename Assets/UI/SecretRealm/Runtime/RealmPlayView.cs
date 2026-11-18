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
    [DefaultExecutionOrder(12000)]
    public sealed class RealmPlayView : MonoBehaviour
    {
        public FoundationTheme theme;
        public RectTransform surface,playArea,arena,arenaViewport,actionHost;
        public UnityEngine.UI.Image artwork,legacyFill;
        public Sprite[] realmArt;
        public Sprite[] swordArt;
        public TMP_Text title,mode,objective,hint,timer,score,notice,directAction;
        public UnityEngine.UI.Text legacyTimer,legacyScore,legacyInstructions,legacyProgress;
        public FoundationProgress progress;
        public FoundationAction exit;
        public SecretRealmPlayController owner;
        SecretRealmType type;
        UnityEngine.UI.Button action;
        TMP_Text actionLabel;
        CanvasGroup intro;
        float began;
        Vector2 previous;
        readonly HashSet<Transform> styled=new HashSet<Transform>();
        void Awake(){exit.Bind("Rời ải",()=>owner.CloseAll());intro=notice.GetComponent<CanvasGroup>();}
        public void Begin(SecretRealmType value)
        {
            type=value;int i=(int)value;title.text=RealmCopy.Names[i];mode.text=RealmCopy.Modes[i];artwork.sprite=realmArt[i];artwork.color=new Color(.48f,.62f,.56f,1);FitArt(artwork);
            string riddle=RealmCopy.Plain(legacyInstructions.text).Split('\n')[0];objective.text=value==SecretRealmType.DanCauldron?riddle:RealmCopy.Objectives[i];hint.text=RealmCopy.Hints[i];
            directAction.text=value==SecretRealmType.DanCauldron?"Chạm một đỉnh để khai mở":value==SecretRealmType.LightningDodge?"Chạm trực tiếp các lôi tinh trong trận":"";
            directAction.gameObject.SetActive(value==SecretRealmType.DanCauldron||value==SecretRealmType.LightningDodge);
            // LaunchMiniGame has stopped the previous round's coroutines. Discard an interrupted
            // visual shake offset here; during a round only the original shake coroutine owns it.
            arena.anchoredPosition=Vector2.zero;
            notice.text="BÍ CẢNH ĐÃ MỞ";began=Time.unscaledTime;styled.Clear();previous=Vector2.zero;Layout();Refresh();
        }
        public static void FitArt(UnityEngine.UI.Image image)
        {if(image.sprite==null)return;var fitter=image.GetComponent<UnityEngine.UI.AspectRatioFitter>();if(fitter!=null)fitter.aspectRatio=image.sprite.rect.width/image.sprite.rect.height;}
        public void StyleAction(GameObject button)
        {
            button.transform.SetParent(actionHost,false);RpgUI.Fill((RectTransform)button.transform);action=button.GetComponent<UnityEngine.UI.Button>();var image=button.GetComponent<UnityEngine.UI.Image>();image.sprite=theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=theme.gold;action.targetGraphic=image;
            foreach(var line in button.GetComponents<UnityEngine.UI.Outline>())line.enabled=false;
            foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))text.enabled=false;
            var icon=button.transform.Find("Icon") as RectTransform;if(icon!=null){icon.anchorMin=icon.anchorMax=new Vector2(0,.5f);icon.pivot=new Vector2(.5f,.5f);icon.anchoredPosition=new Vector2(60,0);icon.sizeDelta=new Vector2(64,64);var img=icon.GetComponent<UnityEngine.UI.Image>();img.raycastTarget=false;var cutout=icon.gameObject.AddComponent<RealmSwordImage>();cutout.source=img;cutout.silhouettes=swordArt;}
            actionLabel=new FoundationFactory(theme).Text("Phase7ActionLabel",button.transform,"",34,TextAlignmentOptions.Center);RpgUI.Fill(actionLabel.rectTransform,112,10,24,10);actionLabel.color=theme.ink;
        }
        public void Layout()
        {
            if(arena==null||playArea==null)return;Canvas.ForceUpdateCanvases();
            // Existing spawn coordinates and target sizes are unchanged. A 12-unit gutter on each side
            // contains the original ±155 spawn range plus each 32-unit target half-width.
            float scale=Mathf.Min(playArea.rect.width/384f,playArea.rect.height/440f);
            arenaViewport.anchorMin=arenaViewport.anchorMax=arenaViewport.pivot=new Vector2(.5f,.5f);arenaViewport.sizeDelta=new Vector2(384,440);arenaViewport.localScale=Vector3.one*Mathf.Max(.01f,scale);arenaViewport.anchoredPosition=Vector2.zero;
            arena.anchorMin=arena.anchorMax=arena.pivot=new Vector2(.5f,.5f);arena.sizeDelta=new Vector2(384,440);arena.localScale=Vector3.one;
            var background=arena.GetComponent<UnityEngine.UI.Image>();background.color=Color.clear;previous=surface.rect.size;
        }
        void LateUpdate()
        {if(previous!=surface.rect.size)Layout();Refresh();StyleArena();if(intro!=null)intro.alpha=Mathf.Clamp01(1-(Time.unscaledTime-began-.9f)/.3f);}
        public void Refresh()
        {
            bool timed=type!=SecretRealmType.DanCauldron&&type!=SecretRealmType.SwordTomb;
            timer.text=timed?RealmCopy.Plain(legacyTimer.text):type==SecretRealmType.DanCauldron?"Không giới hạn":"3 lượt rút";timer.color=theme.gold;
            if(timed){var text=timer.text.TrimEnd('s');if((float.TryParse(text,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.CurrentCulture,out float seconds)||float.TryParse(text,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out seconds))&&seconds<=5)timer.color=theme.danger;}
            score.text=type==SecretRealmType.DanCauldron&&owner.PresentationActionAvailable?"Chờ khai đỉnh":RealmCopy.Plain(legacyScore.text);
            if(type==SecretRealmType.DanCauldron)directAction.text=owner.PresentationActionAvailable?"Chạm một đỉnh để khai mở":"Đã khai đỉnh · Đang mở chiến báo…";
            progress.Set(legacyFill.fillAmount,1,RealmCopy.Plain(legacyProgress.text));
            if(action!=null){bool ready=owner.PresentationActionAvailable;action.interactable=ready;actionLabel.text=ready?type==SecretRealmType.HordeSurvival?"Vạn Kiếm Quy Tông":type==SecretRealmType.BarrierBreak?"Khai Thiên Trảm":"Rút kiếm ngay":"Đang hiển thị kết quả…";}
        }
        void StyleArena()
        {
            for(int i=0;i<arena.childCount;i++)
            {
                var child=arena.GetChild(i);if(!styled.Add(child))continue;
                foreach(var text in child.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if(text.name=="Icon"&&child.name.StartsWith("DemonBadge"))
                    {text.enabled=false;var mask=RpgUI.Rect("DemonMask",text.transform);RpgUI.Fill(mask);var emblem=mask.gameObject.AddComponent<RealmDemonIcon>();emblem.color=theme.ivory;emblem.raycastTarget=false;continue;}
                    if(text.name=="SwordFeedback")text.rectTransform.sizeDelta=new Vector2(340,58);
                    var bridge=text.gameObject.AddComponent<RealmLegacyLabel>();bridge.theme=theme;bridge.source=text;
                }
                if(child.name.StartsWith("DemonBadge")){child.GetComponent<UnityEngine.UI.Image>().color=theme.danger;var o=child.GetComponent<UnityEngine.UI.Outline>();if(o!=null)o.effectColor=theme.gold;}
                if(child.name.StartsWith("ThunderOrb")){child.GetComponent<UnityEngine.UI.Image>().color=theme.jade;}
                if(child.name=="FloatingSword"){var cutout=child.gameObject.AddComponent<RealmSwordImage>();cutout.source=child.GetComponent<UnityEngine.UI.Image>();cutout.silhouettes=swordArt;}
                if(child.name=="GaugeBg")
                {
                    child.GetComponent<UnityEngine.UI.Image>().color=theme.ink;
                    foreach(var outline in child.GetComponentsInChildren<UnityEngine.UI.Outline>()){outline.effectColor=theme.gold;outline.effectDistance=new Vector2(.5f,-.5f);}
                    child.Find("GoldSweetSpot").GetComponent<UnityEngine.UI.Image>().color=theme.gold;
                    foreach(string zone in new[]{"PurpleLeft","PurpleRight"})child.Find(zone).GetComponent<UnityEngine.UI.Image>().color=new Color32(99,78,118,255);
                    foreach(string zone in new[]{"BlueLeft","BlueRight"})child.Find(zone).GetComponent<UnityEngine.UI.Image>().color=new Color32(49,77,88,255);
                    child.Find("Needle").GetComponent<UnityEngine.UI.Image>().color=theme.ivory;
                }
                if(child.name.StartsWith("Cauldron_"))
                {
                    var img=child.GetComponent<UnityEngine.UI.Image>();img.sprite=theme.surfaceSprite;img.type=UnityEngine.UI.Image.Type.Sliced;img.color=theme.surface;
                    foreach(var outline in child.GetComponentsInChildren<UnityEngine.UI.Outline>())outline.enabled=false;
                    var open=child.Find("BtnOpen")?.GetComponent<UnityEngine.UI.Image>();if(open!=null){open.sprite=theme.surfaceSprite;open.type=UnityEngine.UI.Image.Type.Sliced;open.color=theme.gold;}
                    var oldIcon=child.Find("CauldronIcon")?.GetComponent<UnityEngine.UI.Image>();if(oldIcon!=null)oldIcon.enabled=false;
                    var glow=child.Find("GlowAura")?.GetComponent<UnityEngine.UI.Image>();if(glow!=null)glow.color=new Color(theme.jade.r,theme.jade.g,theme.jade.b,.14f);
                    // Reuse the actual three cauldrons in the existing illustration, without creating new item data.
                    int index=child.name.EndsWith("0")?0:child.name.EndsWith("1")?1:2;var crop=RpgUI.Rect("CauldronIllustration",child);crop.anchoredPosition=new Vector2(0,38);crop.sizeDelta=new Vector2(82,86);
                    var art=crop.gameObject.AddComponent<UnityEngine.UI.RawImage>();art.texture=realmArt[2].texture;art.raycastTarget=false;
                    art.uvRect=index==0?new Rect(.12f,.315f,.275f,.22f):index==1?new Rect(.35f,.245f,.3f,.25f):new Rect(.69f,.32f,.26f,.22f);
                }
            }
        }
    }

    // Replaces unsupported emoji/legacy font rendering while the original controller keeps updating its Text references.
}

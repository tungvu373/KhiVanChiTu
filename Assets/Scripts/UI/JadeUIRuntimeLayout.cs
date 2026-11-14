using UnityEngine;

/// <summary>Adapts legacy views that create or reset their presentation after gameplay entry.</summary>
[DefaultExecutionOrder(1200)]
public sealed class JadeUIRuntimeLayout : MonoBehaviour
{
    private Transform quest;
    private UnityEngine.UI.Text title;
    private Vector2 lastQuestSize;
    private void Start()
    {
        var presentation=GetComponentInParent<MobileUIReconstruction>();
        var shortcut=presentation!=null&&presentation.dismantleShortcut!=null?presentation.dismantleShortcut:transform.Find("Drawer_0/MergePanel/BtnDrawerDismantle") as RectTransform;
        if(shortcut!=null)
        {
            shortcut.anchorMin=shortcut.anchorMax=Vector2.one;shortcut.pivot=Vector2.one;shortcut.sizeDelta=new Vector2(430,88);shortcut.anchoredPosition=new Vector2(-8,0);
            foreach(var text in shortcut.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;text.rectTransform.offsetMin=new Vector2(12,4);text.rectTransform.offsetMax=new Vector2(-12,-4);text.fontSize=28;text.resizeTextForBestFit=true;text.resizeTextMinSize=26;text.resizeTextMaxSize=28;}
            var skin=GetComponentInParent<JadeUITheme>();if(skin!=null)JadeUITheme.SetSurface(shortcut.GetComponent<UnityEngine.UI.Image>(),skin.secondaryButton);
        }
        quest=transform.Find("TutorialTargetQuestBar");
        if(quest!=null)title=quest.Find("TxtQuestTitle").GetComponent<UnityEngine.UI.Text>();
        var hud=transform.Find("MobileSkillHUD");
        if(hud!=null)
        {
            var group=hud.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if(group!=null){group.childControlHeight=true;group.childControlWidth=true;group.childForceExpandHeight=false;group.childForceExpandWidth=true;}
            foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                var le=button.GetComponent<UnityEngine.UI.LayoutElement>()??button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                le.minHeight=132;le.preferredHeight=132;le.minWidth=132;le.preferredWidth=132;
                foreach(var text in button.GetComponentsInChildren<UnityEngine.UI.Text>(true)) {text.fontSize=26;text.resizeTextForBestFit=true;text.resizeTextMinSize=22;text.resizeTextMaxSize=26;}
            }
        }
        GetComponentInParent<JadeUITheme>()?.StyleSkills();
    }
    private void LateUpdate()
    {
        if(title==null)return;
        // Two cheap property comparisons detect the legacy tutorial's one-off rebuild/collapse.
        var rt=(RectTransform)quest;
        if(title.fontSize==30&&rt.sizeDelta==lastQuestSize)return;
        bool expanded=title.gameObject.activeSelf;
        rt.sizeDelta=new Vector2(expanded?852:132,132);lastQuestSize=rt.sizeDelta;
        title.fontSize=30;title.resizeTextForBestFit=true;title.resizeTextMinSize=28;title.resizeTextMaxSize=30;
        SetRect(title.rectTransform,new Vector2(0,.4f),Vector2.one,new Vector2(18,0),new Vector2(-286,-8));
        var reward=quest.Find("QuestDescScrollView") as RectTransform;
        if(reward!=null)SetRect(reward,Vector2.zero,new Vector2(1,.4f),new Vector2(18,8),new Vector2(-286,0));
        var rewardText=quest.Find("QuestDescScrollView/TxtQuestReward")?.GetComponent<UnityEngine.UI.Text>();
        if(rewardText!=null)rewardText.fontSize=28;
        var claim=quest.Find("BtnClaimQuick") as RectTransform;
        if(claim!=null)ButtonRect(claim,-142,132);
        var toggle=quest.Find("BtnToggle") as RectTransform;
        if(toggle!=null)
        {
            ButtonRect(toggle,-4,132);
            var text=toggle.GetComponent<UnityEngine.UI.Text>();if(text!=null){text.fontSize=28;text.raycastTarget=true;}
        }
        // Equivalent full-size routes live in the secondary menu.
        quest.Find("BtnAchievement")?.gameObject.SetActive(false);
        quest.Find("BtnStorage")?.gameObject.SetActive(false);
        quest.Find("BtnQuestToggle")?.gameObject.SetActive(false);
    }
    static void SetRect(RectTransform rt,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
    {rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=offsetMin;rt.offsetMax=offsetMax;}
    static void ButtonRect(RectTransform rt,float x,float width)
    {rt.anchorMin=rt.anchorMax=new Vector2(1,.5f);rt.pivot=new Vector2(1,.5f);rt.sizeDelta=new Vector2(width,124);rt.anchoredPosition=new Vector2(x,0);foreach(var t in rt.GetComponentsInChildren<UnityEngine.UI.Text>(true)){t.fontSize=28;t.resizeTextForBestFit=true;t.resizeTextMinSize=28;t.resizeTextMaxSize=28;}}
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>Occludes the gameplay hint while a modal owns attention, retaining tutorial progress and target.</summary>
[DefaultExecutionOrder(1900), DisallowMultipleComponent]
public sealed class MobileUITutorialOcclusion : MonoBehaviour
{
    public MobileUIReconstruction owner;
    CanvasGroup group;
    readonly List<GameObject> modals=new List<GameObject>();
    int previousChildren=-1;
    static readonly HashSet<string> Names=new HashSet<string>{"SecretRealmModal","SecretRealmPlayRoot","SecretRealmResultModal","StorageModalUI","CharacterDetailModal","DismantleConfirmModalUI","AchievementModalUI","MilestoneModalUI","OfflineRewardPopupUI","TribulationModalUI","PauseMenuModal"};
    void LateUpdate()
    {
        if(owner==null)return;if(group==null)group=GetComponent<CanvasGroup>();if(group==null)return;
        if(previousChildren!=owner.transform.childCount)
        {
            previousChildren=owner.transform.childCount;modals.Clear();
            foreach(Transform child in owner.transform)if(Names.Contains(child.name))modals.Add(child.gameObject);
            var body=owner.transform.Find("MobileUIRoot/BodyTalentDetailModal");if(body!=null)modals.Add(body.gameObject);
        }
        bool blocked=PortraitHudController.IsOverlayBlocking||(GameplayChoicesService.Instance!=null&&GameplayChoicesService.Instance.IsChoosing);
        foreach(var modal in modals)if(modal!=null&&modal.activeInHierarchy){blocked=true;break;}
        group.alpha=blocked?0:1;
    }
    void OnDisable(){if(group!=null)group.alpha=1;}
}

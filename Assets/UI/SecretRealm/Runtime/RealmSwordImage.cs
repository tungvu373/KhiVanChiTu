using UnityEngine;

namespace KhiVanChiTu.UI.SecretRealm
{
    // UI-only transparent variants of the exact original sprite selections. The controller still
    // owns source.sprite and uses its original 4/6/8 result values and timing windows.
    [DefaultExecutionOrder(12500)]
    public sealed class RealmSwordImage : MonoBehaviour
    {
        public UnityEngine.UI.Image source;
        public Sprite[] silhouettes;
        Sprite previous;
        void LateUpdate()
        {
            if(source==null||source.sprite==previous||silhouettes==null||silhouettes.Length!=3)return;
            previous=source.sprite;int index=previous!=null&&previous.name=="kiem_3"?0:previous!=null&&previous.name=="kiem_5"?1:previous!=null&&previous.name=="kiem_7"?2:-1;
            source.overrideSprite=index>=0?silhouettes[index]:null;
        }
    }
}

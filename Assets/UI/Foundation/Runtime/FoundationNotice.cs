using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationNotice : MonoBehaviour
    {
        public TMP_Text text;
        Coroutine expiry;
        CanvasGroup group;
        public void Show(string message, float duration = 2.5f)
        {
            gameObject.SetActive(true); text.text = message ?? "";
            group=GetComponent<CanvasGroup>();
            if(group!=null){group.blocksRaycasts=false;group.interactable=false;group.alpha=1;}
            if (expiry != null) StopCoroutine(expiry);
            expiry = duration > 0 && Application.isPlaying ? StartCoroutine(HideAfter(duration)) : null;
        }
        IEnumerator HideAfter(float seconds)
        {
            float began=Time.unscaledTime;
            while(Time.unscaledTime-began<seconds)
            {
                float elapsed=Time.unscaledTime-began;
                if(group!=null)group.alpha=Mathf.Clamp01(Mathf.Min(elapsed/.12f,(seconds-elapsed)/.12f));
                yield return null;
            }
            expiry=null;Hide();
        }
        public void Hide() { gameObject.SetActive(false); }
        void OnDisable() { if (expiry != null) StopCoroutine(expiry); expiry = null; }
    }
}

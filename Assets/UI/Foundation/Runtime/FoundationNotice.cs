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
        public void Show(string message, float duration = 2.5f)
        {
            gameObject.SetActive(true); text.text = message ?? "";
            if (expiry != null) StopCoroutine(expiry);
            expiry = duration > 0 && Application.isPlaying ? StartCoroutine(HideAfter(duration)) : null;
        }
        IEnumerator HideAfter(float seconds) { yield return new WaitForSecondsRealtime(seconds); Hide(); }
        public void Hide() { gameObject.SetActive(false); }
        void OnDisable() { if (expiry != null) StopCoroutine(expiry); expiry = null; }
    }
}

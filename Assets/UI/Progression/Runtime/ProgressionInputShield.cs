using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Progression
{
    /// <summary>Consumes taps on the screen surface so the legacy ancestor backdrop Button cannot close a reading card. Child buttons and ScrollRect drag handlers keep their input ownership.</summary>
    public sealed class ProgressionInputShield : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public void OnPointerDown(PointerEventData data)=>data.Use();
        public void OnPointerUp(PointerEventData data)=>data.Use();
        public void OnPointerClick(PointerEventData data)=>data.Use();
    }
}

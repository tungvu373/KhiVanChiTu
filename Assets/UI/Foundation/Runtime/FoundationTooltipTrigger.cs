using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public FoundationNotice tooltip;
        [TextArea] public string message;
        public void OnPointerEnter(PointerEventData e) { tooltip?.Show(message, 0); }
        public void OnPointerExit(PointerEventData e) { tooltip?.Hide(); }
        public void OnPointerDown(PointerEventData e) { tooltip?.Show(message, 0); }
        public void OnPointerUp(PointerEventData e) { tooltip?.Hide(); }
        void OnDisable() { tooltip?.Hide(); }
    }
}

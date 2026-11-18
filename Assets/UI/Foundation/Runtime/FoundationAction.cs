using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace KhiVanChiTu.UI.Foundation
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class FoundationAction : MonoBehaviour
    {
        public TMP_Text label;
        public UnityEngine.UI.Button button;
        public UnityEvent clicked = new UnityEvent();
        Action command;
        public void Bind(string caption, Action action, bool enabled = true)
        { if (label != null) label.text = caption ?? ""; command = action; button.interactable = enabled; }
        void OnEnable() { if (button == null) button = GetComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(Execute); }
        void Start()
        {
            if(GetComponent<JadeButtonFeedback>()==null)gameObject.AddComponent<JadeButtonFeedback>();
            if(GetComponent<KhiVanChiTu.UI.Polish.UnavailableActionHint>()==null)gameObject.AddComponent<KhiVanChiTu.UI.Polish.UnavailableActionHint>();
        }
        void OnDisable() { if (button != null) button.onClick.RemoveListener(Execute); }
        public void Execute()
        { if (button != null && button.IsInteractable() && isActiveAndEnabled) { command?.Invoke(); clicked.Invoke(); } }
        public void SetAvailable(bool value) { button.interactable = value; }
    }
}

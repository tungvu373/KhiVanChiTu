using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Foundation
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FoundationPanel : MonoBehaviour
    {
        public TMP_Text title, message;
        public RectTransform content;
        public FoundationAction confirm, cancel;
        public UnityEngine.UI.Button backdrop;
        public bool dismissOnBackdrop = true;
        FoundationRoot owner;
        Action accepted;
        GameObject previousSelection;
        public bool IsOpen => gameObject.activeSelf;
        void OnEnable()
        {
            owner = GetComponentInParent<FoundationRoot>();
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            owner?.Push(this);
            if (backdrop != null) backdrop.onClick.AddListener(OnBackdrop);
            if (cancel != null) cancel.Bind("\u0110\u00f3ng", Close);
            if (confirm != null) confirm.Bind("X\u00e1c nh\u1eadn", Accept);
        }
        void OnDisable()
        {
            owner?.Remove(this); accepted = null;
            if (backdrop != null) backdrop.onClick.RemoveListener(OnBackdrop);
        }
        public void Show(string caption, string body, Action onAccept = null)
        {
            if (title != null) title.text = caption ?? "";
            if (message != null) message.text = body ?? "";
            gameObject.SetActive(true); accepted = onAccept; owner?.Push(this);
            if (EventSystem.current != null && confirm != null) EventSystem.current.SetSelectedGameObject(confirm.gameObject);
        }
        public void Accept() { var action = accepted; Close(); action?.Invoke(); }
        public void Close()
        {
            gameObject.SetActive(false);
            if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        void OnBackdrop() { if (dismissOnBackdrop) Close(); }
        public void SetInputEnabled(bool value)
        { var group = GetComponent<CanvasGroup>(); group.interactable = group.blocksRaycasts = value; }
    }
}

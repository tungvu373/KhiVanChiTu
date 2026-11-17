using System;
using TMPro;
using UnityEngine;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationItem : MonoBehaviour
    {
        public FoundationAction action;
        public TMP_Text title, detail, badge;
        public UnityEngine.UI.Image icon, selection;
        public bool IsSelected { get; private set; }
        public void Bind(string caption, string description, Sprite art, string count, Action onSelect = null)
        {
            title.text = caption ?? ""; detail.text = description ?? ""; badge.text = count ?? "";
            icon.sprite = art; icon.enabled = art != null;
            action.Bind("", () => { SetSelected(!IsSelected); onSelect?.Invoke(); });
        }
        public void SetSelected(bool value) { IsSelected = value; if (selection != null) selection.enabled = value; }
    }
}

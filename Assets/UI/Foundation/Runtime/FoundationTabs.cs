using System;
using UnityEngine;
using UnityEngine.Events;

namespace KhiVanChiTu.UI.Foundation
{
    public sealed class FoundationTabs : MonoBehaviour
    {
        public FoundationAction[] tabs;
        public GameObject[] pages;
        public UnityEvent<int> selected = new UnityEvent<int>();
        public int SelectedIndex { get; private set; } = -1;
        public void Initialize()
        {
            for (int i = 0; i < tabs.Length; i++)
            { int index = i; tabs[i].Bind(tabs[i].label.text, () => Select(index)); }
            Select(0);
        }
        void Start() { Initialize(); }
        public void Select(int index)
        {
            if (index < 0 || tabs == null || index >= tabs.Length) return;
            SelectedIndex = index;
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].button.interactable = true;
                tabs[i].button.targetGraphic.color = i == index ? new Color32(201,167,106,255) : new Color32(30,76,72,255);
                tabs[i].label.color = i == index ? new Color32(16,40,42,255) : new Color32(243,235,216,255);
            }
            if (pages != null) for (int i = 0; i < pages.Length; i++) if (pages[i] != null) pages[i].SetActive(i == index);
            selected.Invoke(index);
        }
    }
}

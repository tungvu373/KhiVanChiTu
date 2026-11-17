using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KhiVanChiTu.UI.Foundation
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class FoundationRoot : MonoBehaviour
    {
        public FoundationTheme theme;
        public RectTransform safeArea, gameplayHUD, navigation, screenOverlay, modal, notification;
        readonly List<FoundationPanel> stack = new List<FoundationPanel>();
        Rect lastSafe;
        Vector2Int lastScreen;
        public int OpenPanelCount => stack.Count;
        public void ApplySafeArea(Rect area, Vector2 size)
        {
            if (safeArea == null || size.x <= 0 || size.y <= 0) return;
            if (area.width <= 0 || area.height <= 0) area = new Rect(Vector2.zero, size);
            safeArea.anchorMin = new Vector2(Mathf.Clamp01(area.xMin / size.x), Mathf.Clamp01(area.yMin / size.y));
            safeArea.anchorMax = new Vector2(Mathf.Clamp01(area.xMax / size.x), Mathf.Clamp01(area.yMax / size.y));
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }
        void Update()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            if (size != lastScreen || lastSafe != Screen.safeArea)
            {
                lastScreen = size; lastSafe = Screen.safeArea;
                ApplySafeArea(lastSafe, size);
            }
            if (!Application.isPlaying) return;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true) Back();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) Back();
#endif
        }
        public void Push(FoundationPanel panel)
        {
            stack.RemoveAll(p => p == null || p == panel);
            stack.Add(panel); panel.transform.SetAsLastSibling(); RefreshBlocking();
        }
        public void Remove(FoundationPanel panel) { stack.Remove(panel); RefreshBlocking(); }
        void RefreshBlocking()
        {
            stack.RemoveAll(p => p == null);
            // The input stack must follow rendered Canvas order, including screens below modals.
            stack.Sort((a,b) =>
            {
                var ac = a.GetComponentInParent<Canvas>(); var bc = b.GetComponentInParent<Canvas>();
                int order = (ac != null ? ac.sortingOrder : 0).CompareTo(bc != null ? bc.sortingOrder : 0);
                return order != 0 ? order : a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
            });
            for (int i = 0; i < stack.Count; i++) stack[i].SetInputEnabled(i == stack.Count - 1);
            foreach (var layer in new[] { gameplayHUD, navigation })
            {
                if (layer == null) continue;
                var group = layer.GetComponent<CanvasGroup>();
                if (group != null) group.interactable = group.blocksRaycasts = stack.Count == 0;
            }
        }
        public bool Back()
        {
            stack.RemoveAll(p => p == null || !p.gameObject.activeSelf);
            if (stack.Count == 0) return false;
            stack[stack.Count - 1].Close(); return true;
        }
    }
}

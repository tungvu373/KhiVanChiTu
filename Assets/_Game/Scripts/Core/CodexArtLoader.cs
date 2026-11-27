using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Quan ly hinh anh CodexDrop (Resources/CodexDrop) va 47 anh trong suot tai su dung.
    /// Tuyet doi khong thay the vi tri hinh anh bang emoji hoac icon chu.
    /// </summary>
    public static class CodexArtLoader
    {
        private static RemakeArtRegistrySO _registry;
        private static readonly Dictionary<string, Texture2D> _texCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> _reusedLookup = new Dictionary<string, Sprite>();
        private static bool _reusedIndexed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCaches()
        {
            _registry = null;
            _texCache.Clear();
            _reusedLookup.Clear();
            _reusedIndexed = false;
        }

        public static RemakeArtRegistrySO Registry
        {
            get
            {
                if (_registry == null)
                    _registry = Resources.Load<RemakeArtRegistrySO>("RemakeArtRegistry");
                return _registry;
            }
        }

        public static void InvalidateCaches()
        {
            _registry = null;
            _texCache.Clear();
            _reusedLookup.Clear();
            _reusedIndexed = false;
        }

        public static Texture2D LoadCodexTexture(string relativeKeyWithoutExt)
        {
            if (string.IsNullOrEmpty(relativeKeyWithoutExt)) return null;
            string cleanKey = NormalizeKey(relativeKeyWithoutExt);
            if (_texCache.TryGetValue(cleanKey, out Texture2D cached))
                return cached;

            Texture2D tex = Resources.Load<Texture2D>("CodexDrop/" + cleanKey);
            _texCache[cleanKey] = tex;
            return tex;
        }

        public static Sprite LoadReusedSprite(string key)
        {
            EnsureReusedIndex();
            return !string.IsNullOrEmpty(key) && _reusedLookup.TryGetValue(key, out Sprite s) ? s : null;
        }

        public static Texture2D LoadReusedSwordTexture(int swordLevel)
        {
            int lv = Mathf.Clamp(swordLevel, 1, 10);
            Texture2D codex3d = LoadCodexTexture($"Swords/sword_3d_icon_{lv:00}");
            if (codex3d != null) return codex3d;
            Sprite s = LoadReusedSprite($"Sword{lv:00}");
            return s != null ? s.texture : null;
        }

        public static void ApplyIconOrPlaceholder(
            VisualElement iconHost,
            string codexKey,
            string placeholderLabel,
            string reusedKey = null)
        {
            if (iconHost == null) return;

            Label label = iconHost.Q<Label>("art-placeholder-label");
            Texture2D codexTex = LoadCodexTexture(codexKey);
            if (codexTex != null)
            {
                iconHost.style.backgroundImage = new StyleBackground(codexTex);
                iconHost.RemoveFromClassList("art-slot-placeholder");
                if (label != null) label.style.display = DisplayStyle.None;
                return;
            }

            Sprite reused = LoadReusedSprite(reusedKey);
            if (reused != null)
            {
                iconHost.style.backgroundImage = new StyleBackground(reused);
                iconHost.RemoveFromClassList("art-slot-placeholder");
                if (label != null) label.style.display = DisplayStyle.None;
                return;
            }

            iconHost.style.backgroundImage = StyleKeyword.None;
            iconHost.AddToClassList("art-slot-placeholder");
            if (label == null)
            {
                label = new Label { name = "art-placeholder-label" };
                label.AddToClassList("art-placeholder-text");
                iconHost.Add(label);
            }
            label.style.display = DisplayStyle.Flex;
            label.text = placeholderLabel;
        }

        public static void GetCodexProgress(out int loadedCount, out int totalCount, List<string> missingItems = null)
        {
            RemakeArtRegistrySO reg = Registry;
            loadedCount = 0;
            if (reg == null || reg.CodexCatalog == null || reg.CodexCatalog.Count == 0)
            {
                totalCount = 79;
                return;
            }

            totalCount = reg.CodexCatalog.Count;
            for (int i = 0; i < totalCount; i++)
            {
                RemakeArtRegistrySO.CodexSlotMeta meta = reg.CodexCatalog[i];
                if (LoadCodexTexture(meta.key) != null)
                {
                    loadedCount++;
                }
                else
                {
                    missingItems?.Add($"[{meta.code}] {meta.displayName} ({meta.key})");
                }
            }
        }

        private static void EnsureReusedIndex()
        {
            if (_reusedIndexed) return;
            _reusedIndexed = true;
            _reusedLookup.Clear();
            RemakeArtRegistrySO reg = Registry;
            if (reg == null || reg.ReusedSprites == null) return;
            for (int i = 0; i < reg.ReusedSprites.Count; i++)
            {
                var entry = reg.ReusedSprites[i];
                if (entry != null && !string.IsNullOrEmpty(entry.key) && entry.sprite != null)
                    _reusedLookup[entry.key] = entry.sprite;
            }
        }

        private static string NormalizeKey(string path)
        {
            string s = path.Replace("\\", "/");
            if (s.EndsWith(".png")) s = s.Substring(0, s.Length - 4);
            else if (s.EndsWith(".jpg")) s = s.Substring(0, s.Length - 4);
            else if (s.EndsWith(".jpeg")) s = s.Substring(0, s.Length - 5);
            return s;
        }
    }
}

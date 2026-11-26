using System;
using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    [CreateAssetMenu(fileName = "RemakeArtRegistry", menuName = "KhiVan/Remake/Art Registry")]
    public sealed class RemakeArtRegistrySO : ScriptableObject
    {
        [Serializable]
        public sealed class NamedSpriteEntry
        {
            public string key;
            public Sprite sprite;
        }

        [Serializable]
        public sealed class CodexSlotMeta
        {
            public string code;
            public string key;
            public string displayName;
            public bool isMilestone1Priority;
        }

        [SerializeField] private List<NamedSpriteEntry> reusedSprites = new List<NamedSpriteEntry>();
        [SerializeField] private List<CodexSlotMeta> codexCatalog = new List<CodexSlotMeta>();

        public IReadOnlyList<NamedSpriteEntry> ReusedSprites => reusedSprites;
        public IReadOnlyList<CodexSlotMeta> CodexCatalog => codexCatalog;

        public void SetReusedSprites(List<NamedSpriteEntry> entries)
        {
            reusedSprites = entries ?? new List<NamedSpriteEntry>();
        }

        public void SetCodexCatalog(List<CodexSlotMeta> items)
        {
            codexCatalog = items ?? new List<CodexSlotMeta>();
        }
    }
}

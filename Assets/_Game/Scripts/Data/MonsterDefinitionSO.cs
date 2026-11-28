using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    [CreateAssetMenu(fileName = "MonsterDefinition", menuName = "KhiVan/Remake/Monster Definition")]
    public sealed class MonsterDefinitionSO : ScriptableObject
    {
        public string monsterId = "act1_wolf";
        public string displayName = "Thanh Phong Lang";
        [TextArea(2, 3)] public string description = "Yêu thú đồng cỏ tốc độ cao.";
        public RemakeMonsterArchetype archetype = RemakeMonsterArchetype.FastMelee;
        public string codexPortraitKey = "Monsters/act1_wolf";
        public GameObject prefab;

        [Header("Base Balance Multipliers")]
        [Min(0.2f)] public float hpMultiplier = 1f;
        [Min(0.2f)] public float damageMultiplier = 1f;
        [Min(0.5f)] public float moveSpeed = 2.6f;
        [Min(0.8f)] public float attackRange = 1.9f;
        [Min(0f)] public float hoverHeight = 0f;
        [Min(0.5f)] public float attackInterval = 1.35f;
        [Range(0f, 0.7f)] public float armorMitigation = 0f;
        [Min(0.2f)] public float tuViMultiplier = 1f;
        [Min(0.2f)] public float linhThachMultiplier = 1f;
        public float hpBarHeight = 2.1f;
        public float visualScale = 1f;
        public bool isBoss = false;
        public RemakeMonsterAudioFamily audioFamily = RemakeMonsterAudioFamily.Auto;

        public RemakeMonsterAudioFamily ResolveAudioFamily()
        {
            if (audioFamily != RemakeMonsterAudioFamily.Auto)
                return audioFamily;
            return ResolveAudioFamilyFromMetadata(monsterId, isBoss, archetype);
        }

        public static RemakeMonsterAudioFamily ResolveAudioFamilyFromMetadata(
            string id,
            bool boss,
            RemakeMonsterArchetype arch)
        {
            if (boss || arch == RemakeMonsterArchetype.BossGuardian)
                return RemakeMonsterAudioFamily.Boss;

            string lo = (id ?? "").ToLowerInvariant();
            if (lo.Contains("boss")) return RemakeMonsterAudioFamily.Boss;
            if (lo.Contains("scorpion") || lo.Contains("thunder_spirit") || lo.Contains("insect"))
                return RemakeMonsterAudioFamily.SpiritInsect;
            if (lo.Contains("turtle") || arch == RemakeMonsterArchetype.ArmoredTank)
                return RemakeMonsterAudioFamily.Armored;
            if (lo.Contains("bird") || lo.Contains("crane") || arch == RemakeMonsterArchetype.FlyingAgile)
                return RemakeMonsterAudioFamily.Avian;
            return RemakeMonsterAudioFamily.Beast;
        }
    }
}

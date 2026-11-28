using System.Collections.Generic;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Zero-manual-drag runtime loader for Google Flow processed audio clips dropped into
    /// Assets/_Game/Resources/AudioDrop/{Ambience,Voice/Male,Voice/Female,Monsters,SFX}/.
    /// Caches negative hits so missing slots never re-scan Resources and always fall back cleanly.
    /// </summary>
    public static class AudioDropLoader
    {
        private const string RootPrefix = "AudioDrop/";
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCacheOnReload()
        {
            Cache.Clear();
        }

        public static void InvalidateCache() => Cache.Clear();

        public static AudioClip LoadByKey(string relativeKey)
        {
            if (string.IsNullOrEmpty(relativeKey)) return null;
            if (Cache.TryGetValue(relativeKey, out AudioClip cached))
                return cached;

            AudioClip loaded = Resources.Load<AudioClip>(RootPrefix + relativeKey);
            Cache[relativeKey] = loaded;
            return loaded;
        }

        public static string GetAmbienceKey(RemakeAmbienceZone zone)
        {
            return zone switch
            {
                RemakeAmbienceZone.MenuMountain => "Ambience/amb_menu_mountain",
                RemakeAmbienceZone.Act1Meadow => "Ambience/amb_act1_meadow",
                RemakeAmbienceZone.Act2Wasteland => "Ambience/amb_act2_wasteland",
                RemakeAmbienceZone.Act3JadePeaks => "Ambience/amb_act3_jadepeaks",
                RemakeAmbienceZone.DemonTower => "Ambience/amb_demon_tower",
                RemakeAmbienceZone.SecretSword => "Ambience/amb_secret_sword",
                RemakeAmbienceZone.SecretHerb => "Ambience/amb_secret_herb",
                RemakeAmbienceZone.SecretOre => "Ambience/amb_secret_ore",
                RemakeAmbienceZone.TribulationStorm => "Ambience/amb_tribulation_storm",
                _ => null
            };
        }

        public static string GetVoiceKey(RemakeVoiceLineId line, RemakeCharacterGender gender)
        {
            string folder = gender == RemakeCharacterGender.Male ? "Voice/Male" : "Voice/Female";
            string prefix = gender == RemakeCharacterGender.Male ? "vo_male_" : "vo_female_";
            string suffix = GetVoiceSuffix(line);
            return $"{folder}/{prefix}{suffix}";
        }

        public static string GetVoiceLegacyKey(RemakeVoiceLineId line, RemakeCharacterGender gender)
        {
            string folder = gender == RemakeCharacterGender.Male ? "Voice/Male" : "Voice/Female";
            return $"{folder}/vo_{GetVoiceSuffix(line)}";
        }

        private static string GetVoiceSuffix(RemakeVoiceLineId line)
        {
            return line switch
            {
                RemakeVoiceLineId.SelectPreview => "select_preview",
                RemakeVoiceLineId.RealmTrucCo => "realm_trucco",
                RemakeVoiceLineId.RealmKimDan => "realm_kimdan",
                RemakeVoiceLineId.RealmNguyenAnh => "realm_nguyenanh",
                RemakeVoiceLineId.RealmHoaThan => "realm_hoathan",
                RemakeVoiceLineId.RealmDaiThua => "realm_daithua",
                RemakeVoiceLineId.SkillLoiPhat => "skill_loiphat",
                RemakeVoiceLineId.SkillAmDuong => "skill_amduong",
                RemakeVoiceLineId.SkillVanKiem => "skill_vankiem",
                RemakeVoiceLineId.SkillAnChuong => "skill_anchuong",
                RemakeVoiceLineId.TribulationStart => "tribulation_start",
                RemakeVoiceLineId.BossEncounter => "boss_encounter",
                RemakeVoiceLineId.BreakthroughFail => "breakthrough_fail",
                RemakeVoiceLineId.PlayerRespawn => "player_hurt_respawn",
                RemakeVoiceLineId.IdleLine1 => "idle_01",
                RemakeVoiceLineId.IdleLine2 => "idle_02",
                RemakeVoiceLineId.IdleLine3 => "idle_03",
                RemakeVoiceLineId.IdleLine4 => "idle_04",
                _ => "select_preview"
            };
        }

        public static AudioClip LoadAmbience(RemakeAmbienceZone zone)
        {
            string key = GetAmbienceKey(zone);
            return LoadByKey(key);
        }

        public static AudioClip LoadVoice(RemakeVoiceLineId line, RemakeCharacterGender gender)
        {
            return LoadByKey(GetVoiceKey(line, gender))
                ?? LoadByKey(GetVoiceLegacyKey(line, gender));
        }

        public static AudioClip LoadMonsterCue(RemakeMonsterAudioFamily family, string eventSuffix)
        {
            string fam = family switch
            {
                RemakeMonsterAudioFamily.Beast => "beast",
                RemakeMonsterAudioFamily.Avian => "avian",
                RemakeMonsterAudioFamily.Armored => "armored",
                RemakeMonsterAudioFamily.SpiritInsect => "insect",
                RemakeMonsterAudioFamily.Boss => "boss",
                _ => "beast"
            };

            if (!string.IsNullOrEmpty(eventSuffix))
            {
                AudioClip exact = LoadByKey($"Monsters/mob_{fam}_{eventSuffix}");
                if (exact != null) return exact;
            }
            if (family == RemakeMonsterAudioFamily.Boss)
            {
                AudioClip bossRoar = LoadByKey("Monsters/mob_boss_roar");
                if (bossRoar != null) return bossRoar;
            }
            return LoadByKey($"Monsters/mob_{fam}_pack");
        }

        public static AudioClip LoadSfx(string sfxKeyWithoutPrefix)
        {
            if (string.IsNullOrEmpty(sfxKeyWithoutPrefix)) return null;
            return LoadByKey("SFX/" + sfxKeyWithoutPrefix);
        }

        public static AudioClip LoadBgmOverride(string bgmKeyWithoutPrefix)
        {
            if (string.IsNullOrEmpty(bgmKeyWithoutPrefix)) return null;
            return LoadByKey("BGM/" + bgmKeyWithoutPrefix);
        }
    }
}

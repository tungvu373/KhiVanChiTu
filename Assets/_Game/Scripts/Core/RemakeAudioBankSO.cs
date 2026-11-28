using UnityEngine;

namespace KhiVanChiTu.Remake
{
    /// <summary>
    /// Audio clip container asset for the Light Sunlit Xianxia remake.
    /// Kept in its own file matching its class name so Unity MonoScript serialization stays valid.
    /// </summary>
    [CreateAssetMenu(fileName = "RemakeAudioBank", menuName = "KhiVan/Remake/Audio Bank")]
    public sealed class RemakeAudioBankSO : ScriptableObject
    {
        public AudioClip menuBgm;
        public AudioClip gameBgm;
        public AudioClip bossBgm;
        public AudioClip clickSfx;
        public AudioClip swordHitSfx;
        public AudioClip swordMergeSfx;
        public AudioClip loiPhatSfx;
        public AudioClip breakthroughSfx;
    }
}

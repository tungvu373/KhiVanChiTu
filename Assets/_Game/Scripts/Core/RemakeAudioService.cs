using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class RemakeAudioService : MonoBehaviour
    {
        public static RemakeAudioService Instance { get; private set; }

        [SerializeField] private RemakeAudioBankSO audioBank;
        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private float _lastSwordSfxTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (audioBank == null)
                audioBank = Resources.Load<RemakeAudioBankSO>("RemakeAudioBank");

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = 0.28f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = 0.55f;

            RefreshVolumes();
        }

        public void RefreshVolumes()
        {
            var gfx = SaveService.Current.graphics;
            if (_bgmSource != null) _bgmSource.mute = !gfx.musicEnabled;
            if (_sfxSource != null) _sfxSource.mute = !gfx.soundEnabled;
        }

        public void PlayMenuMusic()
        {
            if (audioBank != null && audioBank.menuBgm != null)
                PlayBgmClip(audioBank.menuBgm);
        }

        public void PlayGameMusic()
        {
            if (audioBank != null && audioBank.gameBgm != null)
                PlayBgmClip(audioBank.gameBgm);
        }

        public void PlayClick() => PlayOneShot(audioBank != null ? audioBank.clickSfx : null, 0.5f);
        public void PlayMerge() => PlayOneShot(audioBank != null ? audioBank.swordMergeSfx : null, 0.7f);
        public void PlayLoiPhat() => PlayOneShot(audioBank != null ? audioBank.loiPhatSfx : null, 0.75f);
        public void PlayBreakthrough() => PlayOneShot(audioBank != null ? audioBank.breakthroughSfx : null, 0.8f);

        public void PlaySwordHit()
        {
            if (Time.unscaledTime - _lastSwordSfxTime < 0.08f) return;
            _lastSwordSfxTime = Time.unscaledTime;
            if (_sfxSource == null || _sfxSource.mute || audioBank == null || audioBank.swordHitSfx == null) return;
            _sfxSource.pitch = Random.Range(0.92f, 1.08f);
            _sfxSource.PlayOneShot(audioBank.swordHitSfx, 0.35f);
        }

        private void PlayBgmClip(AudioClip clip)
        {
            if (_bgmSource == null || _bgmSource.clip == clip && _bgmSource.isPlaying) return;
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (_sfxSource == null || _sfxSource.mute || clip == null) return;
            _sfxSource.pitch = 1f;
            _sfxSource.PlayOneShot(clip, volume);
        }
    }
}

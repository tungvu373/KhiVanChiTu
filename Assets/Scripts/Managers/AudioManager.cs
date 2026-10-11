using UnityEngine;

/// <summary>
/// Quản lý âm thanh toàn cục (BGM, SFX, Mute)
/// Tự động đồng bộ và lưu trữ thiết lập âm lượng vào PlayerPrefs.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    public const string PREF_BGM_VOLUME = "KhiVan_BGM_Volume";
    public const string PREF_SFX_VOLUME = "KhiVan_SFX_Volume";
    public const string PREF_IS_MUTED = "KhiVan_IsMuted";

    public float bgmVolume { get; private set; } = 0.8f;
    public float sfxVolume { get; private set; } = 0.8f;
    public bool isMuted { get; private set; } = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    public void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.8f);
        sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.8f);
        isMuted = PlayerPrefs.GetInt(PREF_IS_MUTED, 0) == 1;

        ApplySettings();
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PREF_BGM_VOLUME, bgmVolume);
        PlayerPrefs.Save();
        ApplySettings();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, sfxVolume);
        PlayerPrefs.Save();
        ApplySettings();
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        PlayerPrefs.SetInt(PREF_IS_MUTED, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplySettings();
    }

    public void ApplySettings()
    {
        // 1. Áp dụng trạng thái câm tiếng toàn cục
        AudioListener.pause = isMuted;

        // 2. Tìm BGM AudioSource trên Camera nếu có
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            AudioSource bgmSource = mainCam.GetComponent<AudioSource>();
            if (bgmSource != null)
            {
                bgmSource.volume = isMuted ? 0f : bgmVolume;
            }
        }
    }
}

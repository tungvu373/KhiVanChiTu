using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dieu khien Volume qua UI Slider.
/// Gan vao bat ky Canvas / Panel cai dat am thanh.
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    [Header("Slider Nhac Nen (Music)")]
    [Tooltip("Slider chinh am luong nhac nen")]
    public Slider musicVolumeSlider;

    [Header("Slider Hieu Ung Am Thanh (SFX)")]
    [Tooltip("Slider chinh am luong SFX")]
    public Slider sfxVolumeSlider;

    // Keys luu vao PlayerPrefs
    private const string PREF_MUSIC = "AudioPref_MusicVolume";
    private const string PREF_SFX   = "AudioPref_SFXVolume";

    private void Start()
    {
        // Khoi tao gia tri tu PlayerPrefs (lan dau = default)
        float savedMusic = PlayerPrefs.GetFloat(PREF_MUSIC, 0.6f);
        float savedSFX   = PlayerPrefs.GetFloat(PREF_SFX,   1.0f);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedMusic;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = savedSFX;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Ap dung gia tri da luu
        ApplyMusicVolume(savedMusic);
        ApplySFXVolume(savedSFX);
    }

    private void OnDestroy()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    // === Callbacks ===
    private void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);
        PlayerPrefs.SetFloat(PREF_MUSIC, value);
        PlayerPrefs.Save();
    }

    private void OnSFXVolumeChanged(float value)
    {
        ApplySFXVolume(value);
        PlayerPrefs.SetFloat(PREF_SFX, value);
        PlayerPrefs.Save();
    }

    // === Apply ===
    private void ApplyMusicVolume(float vol)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(vol);
    }

    private void ApplySFXVolume(float vol)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.SetMasterVolume(vol);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Quản lý âm thanh toàn cục (BGM, SFX, Mute, Click tự động, Va chạm kiếm, Ghép kiếm).
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

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource swordHitSource;

    [Header("BGM Clips")]
    public AudioClip menuBgm;
    public AudioClip ingameBgm;

    [Header("SFX Clips")]
    public AudioClip clickSfx;
    public AudioClip swordHitSfx;
    public AudioClip mergeSwordSfx;
    public AudioClip loiPhatSfx;
    public AudioClip breakthroughSfx;

    public float bgmVolume { get; private set; } = 0.8f;
    public float sfxVolume { get; private set; } = 0.8f;
    public bool isMuted { get; private set; } = false;

    private Coroutine bgmFadeCoroutine;
    private float lastSwordHitTime = -1f;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        AutoLoadClips();
        LoadSettings();
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        if (swordHitSource == null)
        {
            swordHitSource = gameObject.AddComponent<AudioSource>();
            swordHitSource.loop = false;
            swordHitSource.playOnAwake = false;
        }
    }

    public void AutoLoadClips()
    {
#if UNITY_EDITOR
        if (menuBgm == null)
            menuBgm = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/nhạcmenu.mp3");
        if (ingameBgm == null)
            ingameBgm = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/NhạcNềnInGame.mp3");
        if (clickSfx == null)
            clickSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/click.mp3");
        if (swordHitSfx == null)
            swordHitSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/sword.mp3");
        if (mergeSwordSfx == null)
            mergeSwordSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/mergesword.mp3");
        if (loiPhatSfx == null)
            loiPhatSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/LoiPhat.mp3");
        if (breakthroughSfx == null)
            breakthroughSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/bang.mp3");
#endif
    }

    private void OnValidate()
    {
        AutoLoadClips();
    }

    private void Update()
    {
        CheckGlobalButtonClick();
    }

    /// <summary>
    /// Tự động phát hiện khi người dùng click vào bất kì Button UI nào có thể tương tác trên màn hình
    /// </summary>
    private void CheckGlobalButtonClick()
    {
        bool pointerDown = false;
        Vector2 pointerPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointerDown = true;
            pointerPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            pointerDown = true;
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (!pointerDown || EventSystem.current == null) return;

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = pointerPos };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int i = 0; i < raycastResults.Count; i++)
        {
            var hitGo = raycastResults[i].gameObject;
            if (hitGo == null) continue;

            Button btn = hitGo.GetComponentInParent<Button>();
            if (btn != null && btn.interactable && btn.isActiveAndEnabled)
            {
                PlayClickSFX();
                break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SETTINGS & PERSISTENCE
    // ─────────────────────────────────────────────────────────────────────────
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
        if (bgmSource != null)
        {
            bgmSource.mute = isMuted;
            bgmSource.volume = isMuted ? 0f : bgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.mute = isMuted;
            sfxSource.volume = isMuted ? 0f : sfxVolume;
        }

        if (swordHitSource != null)
        {
            swordHitSource.mute = isMuted;
            swordHitSource.volume = isMuted ? 0f : sfxVolume;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BGM METHODS
    // ─────────────────────────────────────────────────────────────────────────
    public void PlayMenuBGM()
    {
        if (menuBgm == null) AutoLoadClips();
        PlayBGM(menuBgm, loop: true);
    }

    public void PlayInGameBGM()
    {
        if (ingameBgm == null) AutoLoadClips();
        PlayBGM(ingameBgm, loop: true);
    }

    public void PlayBGM(AudioClip clip, bool loop = true, float fadeDuration = 0.25f)
    {
        if (clip == null) return;
        EnsureAudioSources();

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeToBgmRoutine(clip, loop, fadeDuration));
    }

    private IEnumerator FadeToBgmRoutine(AudioClip newClip, bool loop, float duration)
    {
        float targetVol = isMuted ? 0f : bgmVolume;

        // Nếu đang phát nhạc thì fade out nhanh
        if (bgmSource.isPlaying && bgmSource.volume > 0.05f)
        {
            float startVol = bgmSource.volume;
            float elapsed = 0f;
            float halfDuration = Mathf.Max(0.05f, duration * 0.5f);

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
                yield return null;
            }
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = loop;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // Fade in
        {
            float elapsed = 0f;
            float halfDuration = Mathf.Max(0.05f, duration * 0.5f);
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                targetVol = isMuted ? 0f : bgmVolume;
                bgmSource.volume = Mathf.Lerp(0f, targetVol, elapsed / halfDuration);
                yield return null;
            }
            bgmSource.volume = targetVol;
        }
        bgmFadeCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SFX METHODS
    // ─────────────────────────────────────────────────────────────────────────
    public void PlayClickSFX()
    {
        if (isMuted || sfxVolume <= 0f) return;
        if (clickSfx == null) AutoLoadClips();
        if (clickSfx == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clickSfx, sfxVolume);
    }

    public void PlayBtnClick()
    {
        PlayClickSFX();
    }

    public void PlaySwordHitSFX()
    {
        if (isMuted || sfxVolume <= 0f) return;

        // Giới hạn tần suất phát (tối đa ~25 lần/s) để tránh xé tiếng khi hàng chục kiếm chém cùng lúc
        if (Time.unscaledTime - lastSwordHitTime < 0.04f) return;
        lastSwordHitTime = Time.unscaledTime;

        if (swordHitSfx == null) AutoLoadClips();
        if (swordHitSfx == null || swordHitSource == null) return;

        // Biến thiên cao độ (pitch) nhẹ (0.93 - 1.07) giúp âm thanh va chạm sinh động, uy lực hơn
        swordHitSource.pitch = Random.Range(0.93f, 1.07f);
        swordHitSource.PlayOneShot(swordHitSfx, sfxVolume);
    }

    public void PlayMergeSwordSFX()
    {
        if (isMuted || sfxVolume <= 0f) return;
        if (mergeSwordSfx == null) AutoLoadClips();
        if (mergeSwordSfx == null || sfxSource == null) return;

        sfxSource.PlayOneShot(mergeSwordSfx, sfxVolume);
    }

    public void PlayLoiPhatSFX()
    {
        if (isMuted || sfxVolume <= 0f) return;
        if (loiPhatSfx == null) AutoLoadClips();
        if (loiPhatSfx == null || sfxSource == null) return;

        sfxSource.PlayOneShot(loiPhatSfx, sfxVolume);
    }

    public void PlayBreakthroughSFX()
    {
        if (isMuted || sfxVolume <= 0f) return;
        if (breakthroughSfx == null) AutoLoadClips();
        if (breakthroughSfx == null || sfxSource == null) return;

        sfxSource.PlayOneShot(breakthroughSfx, sfxVolume);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (isMuted || sfxVolume <= 0f || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }
}

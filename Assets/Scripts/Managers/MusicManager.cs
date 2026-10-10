using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    // =============================================
    //  SINGLETON
    // =============================================
    public static MusicManager Instance { get; private set; }

    // =============================================
    //  INSPECTOR
    // =============================================
    [Header("--- NHAC NEN ---")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("--- AM LUONG ---")]
    [Range(0f, 1f)] public float mainMenuVolume  = 0.7f;
    [Range(0f, 1f)] public float gameplayVolume   = 0.6f;

    [Header("--- FADE (giay) ---")]
    public float fadeTime = 1.0f;

    [Header("--- TEN SCENE ---")]
    public string[] mainMenuScenes  = { "MainMenu", "Main Menu", "Menu" };
    public string[] gameplayScenes  = { "SampleScene", "Gameplay", "Game", "Level" };

    // =============================================
    //  PRIVATE
    // =============================================
    private AudioSource _src;
    private Coroutine   _fade;

    // =============================================
    //  UNITY
    // =============================================
    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup AudioSource
        _src              = GetComponent<AudioSource>();
        _src.loop         = true;
        _src.playOnAwake  = false;
        _src.spatialBlend = 0f;     // 2D

        // Lang nghe chuyen scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Phat nhac cho scene HIEN TAI ngay lap tuc
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =============================================
    //  SCENE LOAD
    // =============================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        if (MatchScene(mainMenuScenes, sceneName))
        {
            PlayClip(mainMenuMusic, mainMenuVolume);
        }
        else if (MatchScene(gameplayScenes, sceneName))
        {
            PlayClip(gameplayMusic, gameplayVolume);
        }
        else
        {
            // Scene khong khop mapping nao -> phat clip nao co san
            if (gameplayMusic  != null) PlayClip(gameplayMusic,  gameplayVolume);
            else if (mainMenuMusic != null) PlayClip(mainMenuMusic, mainMenuVolume);
            else Debug.LogWarning($"[MusicManager] Scene '{sceneName}' khong khop mapping nao va khong co AudioClip nao duoc gan!");
        }
    }

    // =============================================
    //  PLAY LOGIC
    // =============================================
    private void PlayClip(AudioClip clip, float vol)
    {
        if (clip == null)
        {
            Debug.LogWarning("[MusicManager] AudioClip chua duoc gan vao Inspector!", this);
            return;
        }

        // Neu dang phat dung clip nay roi thi thoi
        if (_src.clip == clip && _src.isPlaying)
        {
            _src.volume = vol;
            return;
        }

        // Stop fade cu neu dang chay
        if (_fade != null) StopCoroutine(_fade);

        _fade = StartCoroutine(CrossFade(clip, vol));
    }

    private IEnumerator CrossFade(AudioClip newClip, float targetVol)
    {
        // --- Fade OUT (neu dang phat) ---
        if (_src.isPlaying && fadeTime > 0f)
        {
            float start = _src.volume;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                _src.volume = Mathf.Lerp(start, 0f, t / fadeTime);
                yield return null;
            }
        }
        _src.Stop();

        // --- Phat clip moi ---
        _src.clip   = newClip;
        _src.volume = 0f;
        _src.Play();

        Debug.Log($"[MusicManager] Dang phat: {newClip.name}");

        // --- Fade IN ---
        if (fadeTime > 0f)
        {
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                _src.volume = Mathf.Lerp(0f, targetVol, t / fadeTime);
                yield return null;
            }
        }

        _src.volume = targetVol;
        _fade = null;
    }

    // =============================================
    //  PUBLIC API
    // =============================================
    public void PlayMainMenuMusic() => PlayClip(mainMenuMusic,  mainMenuVolume);
    public void PlayGameplayMusic() => PlayClip(gameplayMusic,  gameplayVolume);
    public void StopMusic()
    {
        if (_fade != null) StopCoroutine(_fade);
        _src.Stop();
    }
    public void SetVolume(float v) => _src.volume = Mathf.Clamp01(v);

    // =============================================
    //  HELPER
    // =============================================
    private static bool MatchScene(string[] list, string sceneName)
    {
        foreach (var s in list)
            if (string.Equals(s, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}

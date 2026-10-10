using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quan ly am thanh hieu ung (SFX) cho game Khi Van Chi Tu.
/// Singleton - DontDestroyOnLoad.
/// Dung Pool de tranh tao nhieu AudioSource.
/// </summary>
public class SFXManager : MonoBehaviour
{
    // === Singleton ===
    public static SFXManager Instance { get; private set; }

    [Header("Cai Dat Pool")]
    [Tooltip("So AudioSource tao san trong pool")]
    [Range(4, 32)]
    public int poolSize = 12;

    [Header("Am Luong SFX Tong The")]
    [Range(0f, 1f)]
    public float masterSFXVolume = 1.0f;

    // === Private State ===
    private readonly List<AudioSource> _pool = new List<AudioSource>();

    // === Unity Lifecycle ===
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPool();
    }

    // === Pool ===
    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go  = new GameObject($"SFX_Source_{i}");
            go.transform.SetParent(transform);
            var src             = go.AddComponent<AudioSource>();
            src.playOnAwake     = false;
            src.spatialBlend    = 0f;
            _pool.Add(src);
        }
    }

    private AudioSource GetFreeSource()
    {
        foreach (var src in _pool)
            if (!src.isPlaying)
                return src;

        // Pool het - lay source dang phat lau nhat (volume nho nhat)
        AudioSource oldest = _pool[0];
        foreach (var src in _pool)
            if (src.volume < oldest.volume)
                oldest = src;

        oldest.Stop();
        return oldest;
    }

    // === Public API ===

    /// <summary>Phat SFX tai vi tri World (3D).</summary>
    public void Play(AudioClip clip, Vector3 worldPos, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var src = GetFreeSource();
        src.spatialBlend = 1f;
        src.transform.position = worldPos;
        src.clip   = clip;
        src.volume = Mathf.Clamp01(volume * masterSFXVolume);
        src.pitch  = pitch;
        src.Play();
    }

    /// <summary>Phat SFX 2D (UI, am thanh toan cuc).</summary>
    public void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var src = GetFreeSource();
        src.spatialBlend = 0f;
        src.clip   = clip;
        src.volume = Mathf.Clamp01(volume * masterSFXVolume);
        src.pitch  = pitch;
        src.Play();
    }

    /// <summary>Chinh am luong tong SFX (0-1).</summary>
    public void SetMasterVolume(float vol)
    {
        masterSFXVolume = Mathf.Clamp01(vol);
    }

    /// <summary>Dung tat ca SFX dang phat.</summary>
    public void StopAll()
    {
        foreach (var src in _pool)
            src.Stop();
    }
}

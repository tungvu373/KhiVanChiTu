using UnityEngine;

/// <summary>
/// Tu dong khoi tao MusicManager va SFXManager neu chua ton tai.
/// Gan script nay vao bat ky Scene nao - ke ca Scene dau tien load.
/// KHONG can tay tao GameObject hay gán AudioSource thu cong.
/// </summary>
public class AudioBootstrapper : MonoBehaviour
{
    [Header("Cau Hinh Khoi Tao")]
    [Tooltip("Gan vao day neu ban da co san GameObject MusicManager trong Scene.\n" +
             "De trong de tu dong tao moi.")]
    public MusicManager existingMusicManager;

    [Tooltip("Gan vao day neu ban da co san GameObject SFXManager trong Scene.\n" +
             "De trong de tu dong tao moi.")]
    public SFXManager existingSFXManager;

    private void Awake()
    {
        EnsureMusicManager();
        EnsureSFXManager();
    }

    private void EnsureMusicManager()
    {
        // Da co singleton - khong lam gi them
        if (MusicManager.Instance != null) return;

        MusicManager mgr = existingMusicManager;

        if (mgr == null)
        {
            // Tu dong tao GameObject moi
            var go = new GameObject("[MusicManager]");
            mgr = go.AddComponent<MusicManager>();
            Debug.Log("[AudioBootstrapper] Da tu dong tao MusicManager moi.");
        }

        // Ghi nhan (Awake cua MusicManager tu xu ly DontDestroyOnLoad)
    }

    private void EnsureSFXManager()
    {
        if (SFXManager.Instance != null) return;

        SFXManager mgr = existingSFXManager;

        if (mgr == null)
        {
            var go = new GameObject("[SFXManager]");
            mgr = go.AddComponent<SFXManager>();
            Debug.Log("[AudioBootstrapper] Da tu dong tao SFXManager moi.");
        }
    }
}

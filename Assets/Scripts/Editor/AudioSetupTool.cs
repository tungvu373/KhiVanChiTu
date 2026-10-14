#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AudioSetupTool
{
    [MenuItem("TuTien/Âm thanh/Thiết lập Hệ thống Âm thanh Toàn Cục (AudioManager)")]
    public static void ManualSetup()
    {
        SetupAudioManagerInScene(silent: false);
    }

    public static void SetupAudioManagerInScene(bool silent)
    {
        if (Application.isPlaying) return;

        AudioManager audioMgr = Object.FindObjectOfType<AudioManager>();
        GameObject audioObj;

        if (audioMgr == null)
        {
            audioObj = new GameObject("AudioManager");
            audioMgr = audioObj.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(audioObj, "Create AudioManager");
        }
        else
        {
            audioObj = audioMgr.gameObject;
        }

        // Tạo / Lấy các AudioSource
        AudioSource[] sources = audioObj.GetComponents<AudioSource>();
        AudioSource bgmSource = null;
        AudioSource sfxSource = null;
        AudioSource swordHitSource = null;

        if (sources.Length >= 3)
        {
            bgmSource = sources[0];
            sfxSource = sources[1];
            swordHitSource = sources[2];
        }
        else
        {
            if (sources.Length > 0) bgmSource = sources[0];
            else bgmSource = audioObj.AddComponent<AudioSource>();

            if (sources.Length > 1) sfxSource = sources[1];
            else sfxSource = audioObj.AddComponent<AudioSource>();

            swordHitSource = audioObj.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // 2D Sound

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        swordHitSource.loop = false;
        swordHitSource.playOnAwake = false;
        swordHitSource.spatialBlend = 0f;

        // Load các file âm thanh từ Assets/Sounds/
        AudioClip menuBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/nhạcmenu.mp3");
        AudioClip ingameBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/NhạcNềnInGame.mp3");
        AudioClip clickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/click.mp3");
        AudioClip swordHitSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/sword.mp3");
        AudioClip mergeSwordSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/mergesword.mp3");
        AudioClip loiPhatSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/LoiPhat.mp3");
        AudioClip breakthroughSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/bang.mp3");

        // Gán vào AudioManager
        audioMgr.bgmSource = bgmSource;
        audioMgr.sfxSource = sfxSource;
        audioMgr.swordHitSource = swordHitSource;

        if (menuBgm != null) audioMgr.menuBgm = menuBgm;
        if (ingameBgm != null) audioMgr.ingameBgm = ingameBgm;
        if (clickSfx != null) audioMgr.clickSfx = clickSfx;
        if (swordHitSfx != null) audioMgr.swordHitSfx = swordHitSfx;
        if (mergeSwordSfx != null) audioMgr.mergeSwordSfx = mergeSwordSfx;
        if (loiPhatSfx != null) audioMgr.loiPhatSfx = loiPhatSfx;
        if (breakthroughSfx != null) audioMgr.breakthroughSfx = breakthroughSfx;

        EditorUtility.SetDirty(audioMgr);
        EditorUtility.SetDirty(audioObj);

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.IsValid() && activeScene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        if (!silent)
        {
            Selection.activeGameObject = audioObj;
            Debug.Log("<color=green>[AudioManager] ✅ ĐÃ CẤU HÌNH THÀNH CÔNG HỆ THỐNG ÂM THANH TOÀN CỤC!</color>\n" +
                      $"- Nhạc Menu: {(menuBgm != null ? menuBgm.name : "Chưa tìm thấy")}\n" +
                      $"- Nhạc Ingame: {(ingameBgm != null ? ingameBgm.name : "Chưa tìm thấy")}\n" +
                      $"- Tiếng Click Button: {(clickSfx != null ? clickSfx.name : "Chưa tìm thấy")}\n" +
                      $"- Tiếng Kiếm chém: {(swordHitSfx != null ? swordHitSfx.name : "Chưa tìm thấy")}\n" +
                      $"- Tiếng Ghép kiếm: {(mergeSwordSfx != null ? mergeSwordSfx.name : "Chưa tìm thấy")}\n" +
                      $"- Tiếng Lôi Phạt: {(loiPhatSfx != null ? loiPhatSfx.name : "Chưa tìm thấy")}\n" +
                      $"- Tiếng Đột Phá Tu Vi: {(breakthroughSfx != null ? breakthroughSfx.name : "Chưa tìm thấy")}");
        }
    }
}
#endif

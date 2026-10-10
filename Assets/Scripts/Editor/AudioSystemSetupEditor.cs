#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tool: Tu dong tao va cau hinh Audio System trong Scene.
/// Su dung qua Menu: Tools > Khi Van Chi Tu > Setup Audio System
/// HOAC: Tu dong chay khi Unity mo du an (InitializeOnLoad).
/// </summary>
[InitializeOnLoad]
public static class AudioSystemSetupEditor
{
    // Da xoa AutoCheckAudioSystem de tranh loi interrupt scene loading tren Unity 6

    [MenuItem("Tools/Khi Van Chi Tu/Setup Audio System %#a")]
    public static void SetupAudioSystem()
    {
        bool created = false;

        // --- MusicManager ---
        MusicManager musicMgr = Object.FindFirstObjectByType<MusicManager>();
        if (musicMgr == null)
        {
            var go  = new GameObject("[MusicManager]");
            musicMgr = go.AddComponent<MusicManager>();

            var src          = go.GetComponent<AudioSource>();
            src.loop         = true;
            src.playOnAwake  = false;
            src.spatialBlend = 0f;
            src.volume       = 0.6f;

            Undo.RegisterCreatedObjectUndo(go, "Create MusicManager");
            created = true;
            Debug.Log("[AudioSetup] Da tao MusicManager.");
        }
        else
        {
            // Dam bao AudioSource dung
            var src = musicMgr.GetComponent<AudioSource>();
            if (src != null)
            {
                src.loop         = true;
                src.playOnAwake  = false;
                src.spatialBlend = 0f;
            }
            Debug.Log("[AudioSetup] MusicManager da ton tai, da cap nhat AudioSource settings.");
        }

        // --- SFXManager ---
        SFXManager sfxMgr = Object.FindFirstObjectByType<SFXManager>();
        if (sfxMgr == null)
        {
            var go  = new GameObject("[SFXManager]");
            sfxMgr  = go.AddComponent<SFXManager>();

            Undo.RegisterCreatedObjectUndo(go, "Create SFXManager");
            created = true;
            Debug.Log("[AudioSetup] Da tao SFXManager.");
        }

        // --- Danh dau Scene la dirty de save ---
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        // --- Focus vao MusicManager ---
        Selection.activeGameObject = musicMgr.gameObject;
        EditorGUIUtility.PingObject(musicMgr.gameObject);

        if (created)
        {
            EditorUtility.DisplayDialog(
                "Audio System - Hoan Tat!",
                "Da tao thanh cong:\n" +
                "  [MusicManager] - Quan ly nhac nen\n" +
                "  [SFXManager]   - Quan ly SFX\n\n" +
                "Buoc tiep theo:\n" +
                "1. GameObject [MusicManager] da duoc chon trong Hierarchy\n" +
                "2. Keo file nhac vao 'Main Menu Music' va 'Gameplay Music'\n" +
                "3. Nhan Ctrl+S de luu Scene\n" +
                "4. Nhan Play - nhac se tu dong phat!",
                "OK!"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Audio System",
                "Tat ca component da ton tai.\n\n" +
                "Hay kiem tra [MusicManager] trong Inspector:\n" +
                "- 'Gameplay Music' da gan chua?\n" +
                "- 'Gameplay Scene Names' co chua ten Scene hien tai?",
                "OK"
            );
        }
    }

    [MenuItem("Tools/Khi Van Chi Tu/Validate Audio System")]
    public static void ValidateAudioSystem()
    {
        MusicManager musicMgr = Object.FindFirstObjectByType<MusicManager>();
        SFXManager   sfxMgr   = Object.FindFirstObjectByType<SFXManager>();
        string activeScene    = EditorSceneManager.GetActiveScene().name;

        string report = "=== BAO CAO AUDIO SYSTEM ===\n\n";
        report += $"Scene hien tai: {activeScene}\n\n";

        if (musicMgr != null)
        {
            report += "[MusicManager]: TIM THAY\n";
            report += musicMgr.mainMenuMusic  != null
                    ? $"  Main Menu Music : OK ({musicMgr.mainMenuMusic.name})\n"
                    : "  Main Menu Music : CHUA GAN!\n";
            report += musicMgr.gameplayMusic  != null
                    ? $"  Gameplay Music  : OK ({musicMgr.gameplayMusic.name})\n"
                    : "  Gameplay Music  : CHUA GAN!\n";

            var src = musicMgr.GetComponent<AudioSource>();
            report += src != null
                    ? $"  AudioSource     : OK (Loop={src.loop}, 3D={src.spatialBlend})\n"
                    : "  AudioSource     : KHONG TIM THAY!\n";

            report += $"\n  Main Menu Scenes : {string.Join(", ", musicMgr.mainMenuScenes)}\n";
            report += $"  Gameplay Scenes  : {string.Join(", ", musicMgr.gameplayScenes)}\n";

            bool sceneIsGameplay = System.Array.Exists(
                musicMgr.gameplayScenes,
                s => s.Equals(activeScene, System.StringComparison.OrdinalIgnoreCase));
            bool sceneIsMenu = System.Array.Exists(
                musicMgr.mainMenuScenes,
                s => s.Equals(activeScene, System.StringComparison.OrdinalIgnoreCase));

            report += sceneIsGameplay ? $"\n  Scene '{activeScene}' -> GAMEPLAY (nhac gameplay se phat)\n"
                    : sceneIsMenu     ? $"\n  Scene '{activeScene}' -> MAIN MENU (nhac menu se phat)\n"
                                      : $"\n  CANH BAO: Scene '{activeScene}' KHONG khop voi bat ky mapping nao!\n  Them ten nay vao Gameplay Scene Names hoac Main Menu Scene Names.\n";
        }
        else
        {
            report += "[MusicManager]: KHONG TIM THAY!\n";
            report += "  -> Chay 'Tools > Khi Van Chi Tu > Setup Audio System'\n";
        }

        report += sfxMgr != null ? "\n[SFXManager]: TIM THAY\n" : "\n[SFXManager]: KHONG TIM THAY\n";

        Debug.Log(report);
        EditorUtility.DisplayDialog("Ket Qua Kiem Tra Audio", report, "Dong");
    }
}
#endif

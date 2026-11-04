using UnityEditor;
using UnityEngine;

public static class BuildSettingsChecker
{
    [MenuItem("TuTien/Debug/Check and Set Build Scenes")]
    public static void CheckBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        Debug.Log($"EditorBuildSettings has {scenes.Length} scenes:");
        for (int i = 0; i < scenes.Length; i++)
        {
            Debug.Log($"  [{i}] {scenes[i].path} (enabled={scenes[i].enabled})");
        }

        string targetScene = "Assets/Scenes/SampleScene_MobileWork.unity";
        bool hasTarget = false;
        foreach (var s in scenes)
        {
            if (s.path == targetScene && s.enabled) hasTarget = true;
        }

        if (!hasTarget)
        {
            var newScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(targetScene, true)
            };
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"[BuildSettings] Set {targetScene} as default build scene 0!");
        }
    }
}


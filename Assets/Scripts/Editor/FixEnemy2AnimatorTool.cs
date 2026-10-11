#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixEnemy2AnimatorTool
{
    [MenuItem("TuTien/Fixes/Reimport & Refresh Enemy2")]
    public static void FixEnemy2()
    {
        AssetDatabase.ImportAsset("Assets/Import/Enemy/Zombie Run.anim", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Import/Enemy/Idle.anim", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Import/Enemy/Enemy2.controller", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Prefabs/Enemy1.prefab", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Prefabs/Enemy2.prefab", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Prefabs/Boss.prefab", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[TuTien] Đã ép Unity Reimport và nạp mới hoàn toàn Animations (Loop Time), Enemy2 và Boss!</color>");
    }
}
#endif

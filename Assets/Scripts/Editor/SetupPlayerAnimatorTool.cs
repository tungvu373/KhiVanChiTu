#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupPlayerAnimatorTool
{
    [MenuItem("TuTien/Player/Tự động kết nối Animator cho Player")]
    public static void AutoSetupPlayerAnimator()
    {
        CombatManager combatMgr = Object.FindObjectOfType<CombatManager>();
        if (combatMgr == null)
        {
            Debug.LogError("❌ [TuTien] Không tìm thấy CombatManager trong Scene!");
            return;
        }

        Transform playerTransform = combatMgr.playerTransform;
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj == null)
            {
                foreach (var go in Object.FindObjectsOfType<GameObject>())
                {
                    if (go.name.ToLower().Contains("player") || go.name.ToLower().Contains("chibi"))
                    {
                        playerObj = go;
                        break;
                    }
                }
            }

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                combatMgr.playerTransform = playerTransform;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("❌ [TuTien] Không tìm thấy Player trong Scene để gán Animator!");
            return;
        }

        // Tìm hoặc thêm Animator trên Player hoặc con của Player
        Animator animator = playerTransform.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = playerTransform.GetComponent<Animator>();
        }
        if (animator == null)
        {
            animator = playerTransform.gameObject.AddComponent<Animator>();
            Debug.Log("➕ Đã thêm component Animator vào Player.");
        }

        // Nạp Animator Controller chuẩn của Player
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Import/player.controller");
        if (controller == null)
        {
            controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Import/Meshy_AI_Chibi_Crane_Cultivato_biped/Player.controller");
        }

        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log($"✅ Đã gán Controller: '{controller.name}' vào Animator của Player!");
        }
        else
        {
            Debug.LogWarning("⚠️ Không tìm thấy player.controller tại Assets/Import/player.controller!");
        }

        // Kết nối vào CombatManager
        combatMgr.playerAnimator = animator;
        combatMgr.playerTransform = playerTransform;

        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(combatMgr);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = animator.gameObject;
        Debug.Log("<color=green>🎉 [TuTien] HOÀN TẤT KẾT NỐI ANIMATOR CHO PLAYER!</color>\n" +
                  $"- Player Object: {playerTransform.name}\n" +
                  $"- Animator: {animator.name}\n" +
                  $"- Controller: {(controller != null ? controller.name : "None")}\n" +
                  $"- Đã kết nối vào CombatManager.playerAnimator!");
    }
}
#endif

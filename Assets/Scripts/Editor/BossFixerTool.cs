#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tool chuẩn hóa Boss.prefab và khắc phục triệt để lỗi Boss nằm ngang dưới đất:
/// 1. Xóa bỏ child 'Enemy' rác (-12m offset, 50 HP) từng làm Boss bị kích hoạt 'Die' ngay khi vừa xuất hiện.
/// 2. Gắn BossEnemy và CapsuleCollider trực tiếp lên ROOT của Boss.prefab.
/// 3. Tắt applyRootMotion để Animator không can thiệp làm nghiêng/lật ngửa model.
/// 4. Đảm bảo nút TÚI (BtnHUDStorage) luôn có StorageButtonHandler để bấm mở ngay.
/// </summary>
public static class BossFixerTool
{
    [MenuItem("TuTien/Fix/Sửa Triệt Để Lỗi Boss Nằm Ngang & Prefab")]
    public static void FixBossPrefabAndStorageButton()
    {
        // 1. Sửa Boss.prefab
        string prefabPath = "Assets/Prefabs/Boss.prefab";
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (bossPrefab != null)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editingScope.prefabContentsRoot;

                // Xóa child "Enemy" rác gây chết ảo và trigger Die sớm
                Transform childEnemy = root.transform.Find("Enemy");
                if (childEnemy != null)
                {
                    Debug.Log("🗑️ [BossFixer] Đã xóa child 'Enemy' rác khỏi Boss.prefab!");
                    Object.DestroyImmediate(childEnemy.gameObject);
                }

                // Gắn BossEnemy lên chính ROOT
                BossEnemy bossComp = root.GetComponent<BossEnemy>();
                if (bossComp == null)
                {
                    bossComp = root.AddComponent<BossEnemy>();
                }

                // Cài đặt thông số chuẩn trên BossEnemy
                bossComp.animator = root.GetComponent<Animator>();
                bossComp.isBoss = true;
                bossComp.hpBarHeight = 3.5f;
                bossComp.attackRange = 2.4f;
                bossComp.moveSpeed = 1.8f;

                // Đảm bảo có CapsuleCollider chuẩn
                CapsuleCollider col = root.GetComponent<CapsuleCollider>();
                if (col == null) col = root.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0f, 1.5f, 0f);
                col.radius = 0.9f;
                col.height = 3.0f;
                col.direction = 1; // Y-Axis

                // Tắt Root Motion để Animator không làm nghiêng/lật ngửa model
                Animator anim = root.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.applyRootMotion = false;
                }

                Debug.Log("✅ [BossFixer] Đã chuẩn hóa Boss.prefab: BossEnemy trên Root, CapsuleCollider chuẩn, xóa child rác!");
            }
        }
        else
        {
            Debug.LogError("❌ [BossFixer] Không tìm thấy " + prefabPath);
        }

        // 2. Gắn StorageButtonHandler lên BtnHUDStorage trong Scene
        GameObject btnStorageObj = GameObject.Find("Canvas/MobileUIRoot/BtnHUDStorage");
        if (btnStorageObj != null)
        {
            var handler = btnStorageObj.GetComponent<StorageButtonHandler>();
            if (handler == null) handler = btnStorageObj.AddComponent<StorageButtonHandler>();
            var btn = btnStorageObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(handler.OnStorageButtonClicked);
            }
            EditorUtility.SetDirty(btnStorageObj);
            Debug.Log("✅ [BossFixer] Đã gắn StorageButtonHandler lên Canvas/MobileUIRoot/BtnHUDStorage!");
        }

        // 3. Đảm bảo StorageModalUI tự động hook nút nếu bị mất
        var storageUI = Object.FindAnyObjectByType<StorageModalUI>(FindObjectsInactive.Include);
        if (storageUI != null)
        {
            EditorUtility.SetDirty(storageUI.gameObject);
        }

        AssetDatabase.SaveAssets();
    }
}
#endif


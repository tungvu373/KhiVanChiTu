#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class NormalEnemyPrefabSetupTool
{
    private static readonly string[] PrefabPaths =
    {
        "Assets/Prefabs/sói.prefab",
        "Assets/Prefabs/chim.prefab",
        "Assets/Prefabs/rùa.prefab"
    };

    [MenuItem("TuTien/Enemies/Prepare Wolf Bird Turtle Prefabs")]
    public static void PreparePrefabs()
    {
        try
        {
            foreach (string path in PrefabPaths)
                PreparePrefab(path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NormalEnemyPrefabSetup] PASS: Sói, Chim, Rùa have Enemy AI, Animator references and solid colliders; no Rigidbody/NavMeshAgent added because current Enemy movement is transform-driven.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void PreparePrefab(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            throw new InvalidOperationException("Missing normal enemy prefab: " + path);

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Enemy enemy = root.GetComponent<Enemy>();
            if (enemy == null) enemy = root.AddComponent<Enemy>();
            enemy.isBoss = false;

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null || animator.runtimeAnimatorController == null)
                throw new InvalidOperationException(path + " requires an Animator with a controller.");
            enemy.animator = animator;

            bool hasSolidCollider = false;
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider != null && collider.enabled && !collider.isTrigger)
                {
                    hasSolidCollider = true;
                    break;
                }
            }

            if (!hasSolidCollider)
                AddFittedCapsule(root);

            int moving = 0, attack = 0, death = 0;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == "IsMoving" && parameter.type == AnimatorControllerParameterType.Bool) moving++;
                if (parameter.name == "Attack" && parameter.type == AnimatorControllerParameterType.Trigger) attack++;
                if (parameter.name == "Die" && parameter.type == AnimatorControllerParameterType.Trigger) death++;
            }

            if (moving == 0 || attack == 0 || death == 0)
                Debug.LogWarning($"[NormalEnemyPrefabSetup] {path} controller '{animator.runtimeAnimatorController.name}' has {animator.parameters.Length} parameters (IsMoving={moving}, Attack={attack}, Die={death}). The supplied default animation is retained; Enemy.cs safely skips unavailable parameters.");

            bool hasBody = root.GetComponentInChildren<Rigidbody>(true) != null;
            bool hasAgent = root.GetComponentInChildren<NavMeshAgent>(true) != null;
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (saved == null)
                throw new InvalidOperationException("Failed to save prepared prefab: " + path);

            Debug.Log($"[NormalEnemyPrefabSetup] PASS {path}: root Enemy={saved.GetComponent<Enemy>() != null}, solidCollider={HasSolidCollider(saved)}, Animator={animator.runtimeAnimatorController.name}, Rigidbody={hasBody}, NavMeshAgent={hasAgent}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void AddFittedCapsule(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            throw new InvalidOperationException(root.name + " has no Renderer to fit a collider.");

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.bounds;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 world = new Vector3(
                    (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                Vector3 local = root.transform.InverseTransformPoint(world);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }
        }

        Vector3 size = max - min;
        float radius = Mathf.Clamp(Mathf.Max(size.x, size.z) * 0.38f, 0.18f, 0.9f);
        CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
        capsule.direction = 1;
        capsule.center = (min + max) * 0.5f;
        capsule.radius = radius;
        capsule.height = Mathf.Max(size.y * 0.9f, radius * 2f);
        capsule.isTrigger = false;
        Enemy enemy = root.GetComponent<Enemy>();
        if (enemy != null) enemy.hpBarHeight = Mathf.Max(1.2f, size.y + 0.35f);
    }

    private static bool HasSolidCollider(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            if (collider != null && collider.enabled && !collider.isTrigger) return true;
        return false;
    }
}
#endif

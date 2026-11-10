using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public static SimplePool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    
    // Stack chứa các đối tượng ĐANG RẢNH (Inactive) - truy xuất O(1)
    private Dictionary<string, Stack<GameObject>> availablePools;
    // Danh sách toàn bộ đối tượng đã tạo cho từng tag (để phục vụ DeactivateAllInPool)
    private Dictionary<string, List<GameObject>> allSpawnedObjects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        availablePools = new Dictionary<string, Stack<GameObject>>();
        allSpawnedObjects = new Dictionary<string, List<GameObject>>();
        if (pools == null) pools = new List<Pool>();

        foreach (Pool pool in pools)
        {
            CreatePoolInternal(pool.tag, pool.prefab, pool.size);
        }
    }

    public void AddPool(string tag, GameObject prefab, int size)
    {
        if (availablePools == null) availablePools = new Dictionary<string, Stack<GameObject>>();
        if (allSpawnedObjects == null) allSpawnedObjects = new Dictionary<string, List<GameObject>>();
        if (availablePools.ContainsKey(tag)) return; // Already exists

        CreatePoolInternal(tag, prefab, size);

        if (pools == null) pools = new List<Pool>();
        pools.Add(new Pool { tag = tag, prefab = prefab, size = size });
    }

    private void CreatePoolInternal(string tag, GameObject prefab, int size)
    {
        Stack<GameObject> stack = new Stack<GameObject>(size);
        List<GameObject> allList = new List<GameObject>(size);

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            var tracker = obj.AddComponent<PooledObjectTracker>();
            tracker.Init(tag, this);
            obj.SetActive(false);
            stack.Push(obj);
            allList.Add(obj);
        }

        availablePools[tag] = stack;
        allSpawnedObjects[tag] = allList;
    }

    /// <summary>
    /// Lấy đối tượng từ pool với độ phức tạp O(1), không xoay vòng Queue khi có nhiều object đang active.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (availablePools == null || !availablePools.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Stack<GameObject> stack = availablePools[tag];
        GameObject objToSpawn = null;

        while (stack.Count > 0)
        {
            GameObject candidate = stack.Pop();
            if (candidate != null)
            {
                objToSpawn = candidate;
                break;
            }
        }

        // Nếu pool hết đối tượng rảnh, tạo thêm một đối tượng mới
        if (objToSpawn == null)
        {
            Pool p = pools.Find(x => x.tag == tag);
            if (p != null && p.prefab != null)
            {
                objToSpawn = Instantiate(p.prefab, transform);
                var tracker = objToSpawn.AddComponent<PooledObjectTracker>();
                tracker.Init(tag, this);
                if (allSpawnedObjects.TryGetValue(tag, out var allList))
                {
                    allList.Add(objToSpawn);
                }
            }
            else
            {
                return null;
            }
        }

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);

        return objToSpawn;
    }

    public void ReturnObject(string tag, GameObject obj)
    {
        if (obj == null) return;
        if (availablePools != null && availablePools.TryGetValue(tag, out var stack))
        {
            if (!stack.Contains(obj))
            {
                stack.Push(obj);
            }
        }
    }

    public void DeactivateAllInPool(string tag)
    {
        if (allSpawnedObjects != null && allSpawnedObjects.TryGetValue(tag, out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].activeSelf)
                {
                    list[i].SetActive(false);
                }
            }
        }
    }
}

/// <summary>
/// Hook tự động thông báo khi đối tượng bị SetActive(false) để thu hồi về Stack O(1).
/// </summary>
public class PooledObjectTracker : MonoBehaviour
{
    private string poolTag;
    private SimplePool pool;

    public void Init(string tag, SimplePool poolInstance)
    {
        poolTag = tag;
        pool = poolInstance;
    }

    private void OnDisable()
    {
        if (pool != null)
        {
            pool.ReturnObject(poolTag, gameObject);
        }
    }
}

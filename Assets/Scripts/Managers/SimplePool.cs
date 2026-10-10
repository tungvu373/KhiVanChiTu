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
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        if (pools == null) pools = new List<Pool>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public void AddPool(string tag, GameObject prefab, int size)
    {
        if (poolDictionary == null) poolDictionary = new Dictionary<string, Queue<GameObject>>();
        if (poolDictionary.ContainsKey(tag)) return; // Already exists

        Queue<GameObject> objectPool = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
        poolDictionary.Add(tag, objectPool);

        if (pools == null) pools = new List<Pool>();
        pools.Add(new Pool { tag = tag, prefab = prefab, size = size });
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[tag];
        GameObject objToSpawn = null;
        
        // Cố gắng tìm một object không active trong queue
        int initialCount = objectPool.Count;
        for (int i = 0; i < initialCount; i++)
        {
            GameObject go = objectPool.Dequeue();
            if (go == null) continue; // Bỏ qua nếu đã bị Destroy ngoài ý muốn

            if (!go.activeInHierarchy)
            {
                objToSpawn = go;
                break;
            }
            else
            {
                // Nếu đang active thì nhét lại vào cuối hàng
                objectPool.Enqueue(go);
            }
        }

        // Nếu không tìm thấy (tất cả đều đang active), ta phải tạo mới
        if (objToSpawn == null)
        {
            Pool p = pools.Find(x => x.tag == tag);
            if (p != null)
            {
                objToSpawn = Instantiate(p.prefab, transform);
            }
            else
            {
                return null;
            }
        }

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;

        // Bỏ lại vào queue để tái sử dụng sau này
        objectPool.Enqueue(objToSpawn);

        return objToSpawn;
    }
}

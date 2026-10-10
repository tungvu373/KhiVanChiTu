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
            Destroy(gameObject);
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

        if (objectPool.Count == 0 || objectPool.Peek().activeInHierarchy)
        {
            Pool p = pools.Find(x => x.tag == tag);
            if (p != null)
            {
                GameObject newObj = Instantiate(p.prefab, transform);
                newObj.SetActive(false);
                objectPool.Enqueue(newObj);
            }
            else
            {
                return null;
            }
        }

        GameObject objToSpawn = objectPool.Dequeue();

        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;

        objectPool.Enqueue(objToSpawn);

        return objToSpawn;
    }
}

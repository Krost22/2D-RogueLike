using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // Map prefab -> Pool
    private Dictionary<GameObject, ObjectPool<GameObject>> poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();
    
    // Map instance -> Pool (so we know where to return it)
    private Dictionary<GameObject, ObjectPool<GameObject>> instanceToPoolMap = new Dictionary<GameObject, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawns an object from the pool corresponding to the given prefab.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolManager: Cannot spawn null prefab.");
            return null;
        }

        if (!poolDictionary.ContainsKey(prefab))
        {
            InitializePool(prefab);
        }

        GameObject instance = poolDictionary[prefab].Get();
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        return instance;
    }

    /// <summary>
    /// Returns an object to its pool.
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (instanceToPoolMap.TryGetValue(instance, out ObjectPool<GameObject> pool))
        {
            // Only release if active to avoid "already released" errors
            if (instance.activeSelf)
            {
                pool.Release(instance);
            }
        }
        else
        {
            Debug.LogWarning($"PoolManager: Trying to despawn {instance.name} but it was not spawned via PoolManager. Destroying instead.");
            Destroy(instance);
        }
    }

    /// <summary>
    /// Returns an object to its pool after a delay.
    /// </summary>
    public void Despawn(GameObject instance, float delay)
    {
        StartCoroutine(DespawnRoutine(instance, delay));
    }

    private IEnumerator DespawnRoutine(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(instance);
    }

    private void InitializePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = null;
        
        // Initialize the pool
        pool = new ObjectPool<GameObject>(
            createFunc: () => 
            {
                GameObject obj = Instantiate(prefab);
                // Register the instance to this pool
                instanceToPoolMap[obj] = pool;
                // Ensure it doesn't get destroyed on scene load if we want persistent pools, 
                // but for now let's keep it simple.
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => 
            {
                instanceToPoolMap.Remove(obj);
                Destroy(obj);
            },
            defaultCapacity: 10,
            maxSize: 200 // Cap to prevent infinite memory growth
        );

        poolDictionary[prefab] = pool;
    }
}

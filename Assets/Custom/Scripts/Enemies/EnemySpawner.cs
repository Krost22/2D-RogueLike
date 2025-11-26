using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Prefab to instantiate as a warning before the enemy appears.")]
    public GameObject spawnWarningPrefab;
    [Tooltip("Time in seconds to show the warning before spawning.")]
    public float warningDuration = 1.5f;

    [Header("References")]
    [Tooltip("List of areas where enemies can spawn.")]
    public SpawnArea[] spawnAreas;

    // Event triggered when an enemy is spawned
    public static event Action<GameObject> OnEnemySpawned;

    /// <summary>
    /// Spawns a specific enemy prefab after a warning delay.
    /// </summary>
    /// <param name="enemyPrefab">The enemy prefab to spawn.</param>
    /// <param name="count">How many of this enemy to spawn in this batch.</param>
    public void SpawnBatch(GameObject enemyPrefab, int count)
    {
        if (spawnAreas.Length == 0)
        {
            Debug.LogWarning("No SpawnAreas assigned to EnemySpawner!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Pick a random area
            SpawnArea area = spawnAreas[UnityEngine.Random.Range(0, spawnAreas.Length)];
            Vector2 position = area.GetRandomPosition();

            StartCoroutine(SpawnRoutine(enemyPrefab, position));
        }
    }

    private IEnumerator SpawnRoutine(GameObject enemyPrefab, Vector2 position)
    {
        // 1. Show Warning
        GameObject warningInstance = null;
        if (spawnWarningPrefab != null)
        {
            warningInstance = PoolManager.Instance.Spawn(spawnWarningPrefab, position, Quaternion.identity);
        }

        // 2. Wait for grace period
        yield return new WaitForSeconds(warningDuration);

        // 3. Remove Warning
        if (warningInstance != null)
        {
            PoolManager.Instance.Despawn(warningInstance);
        }

        // 4. Spawn Enemy
        GameObject enemyInstance = PoolManager.Instance.Spawn(enemyPrefab, position, Quaternion.identity);
        
        // 5. Notify system
        OnEnemySpawned?.Invoke(enemyInstance);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 1f; // Simple weight for now
    public int minDifficultyToSpawn = 0; // Only spawn after this difficulty level
}

public class HordeManager : MonoBehaviour
{
    [Header("Configuration")]
    public EnemySpawner spawner;
    public List<EnemyType> enemyTypes;

    [Header("Difficulty Settings")]
    [Tooltip("How often (in seconds) a spawn wave occurs initially.")]
    public float baseSpawnInterval = 5f;
    [Tooltip("How much the difficulty increases per second.")]
    public float difficultyIncreaseRate = 0.1f;
    [Tooltip("Maximum number of enemies per wave cap.")]
    public int maxEnemiesPerWave = 10;

    [Header("State (Read Only)")]
    public float currentDifficulty = 1f;
    public float timeSinceStart = 0f;

    private float _nextSpawnTime = 0f;

    // Events
    public static event Action<float> OnDifficultyChanged;

    private void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
        _nextSpawnTime = Time.time + baseSpawnInterval;
    }

    private void Update()
    {
        // 1. Increase Difficulty over time
        timeSinceStart += Time.deltaTime;
        currentDifficulty = 1f + (timeSinceStart * difficultyIncreaseRate);
        
        // Notify difficulty change (optional: could throttle this if too frequent)
        OnDifficultyChanged?.Invoke(currentDifficulty);

        // 2. Check Spawn Timer
        if (Time.time >= _nextSpawnTime)
        {
            SpawnWave();
            CalculateNextSpawnTime();
        }
    }

    private void SpawnWave()
    {
        // Calculate how many enemies to spawn based on difficulty
        // Example formula: 1 enemy base + 1 for every 5 difficulty levels
        int count = Mathf.FloorToInt(currentDifficulty / 2f) + 1;
        count = Mathf.Clamp(count, 1, maxEnemiesPerWave);

        // Pick an enemy type suitable for current difficulty
        GameObject enemyToSpawn = GetRandomEnemyPrefab();

        if (enemyToSpawn != null)
        {
            spawner.SpawnBatch(enemyToSpawn, count);
        }
    }

    private void CalculateNextSpawnTime()
    {
        // As difficulty rises, spawn interval decreases
        // Formula: Base / (1 + (Difficulty * 0.1))
        // Example: Diff 1 -> 5s / 1.1 = 4.5s
        // Example: Diff 10 -> 5s / 2.0 = 2.5s
        float interval = baseSpawnInterval / (1f + (currentDifficulty * 0.1f));
        interval = Mathf.Max(0.5f, interval); // Hard cap at 0.5s minimum
        _nextSpawnTime = Time.time + interval;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        // Filter valid enemies
        List<EnemyType> validEnemies = new List<EnemyType>();
        foreach (var e in enemyTypes)
        {
            if (currentDifficulty >= e.minDifficultyToSpawn)
            {
                validEnemies.Add(e);
            }
        }

        if (validEnemies.Count == 0) return null;

        // Simple random pick (could be weighted)
        return validEnemies[UnityEngine.Random.Range(0, validEnemies.Count)].prefab;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 1f;
    public int minDifficultyToSpawn = 0;
}

public class HordeManager : MonoBehaviour
{
    public enum HordeState
    {
        WaitingToStart,
        Spawning,
        Fighting,
        CalmPhase // PowerUp selection
    }

    [Header("Configuration")]
    public EnemySpawner spawner;
    public List<EnemyType> enemyTypes;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int baseEnemiesPerWave = 5;
    public float difficultyMultiplier = 1.2f; // Increase difficulty by 20% each wave

    [Header("State (Read Only)")]
    public HordeState currentState = HordeState.WaitingToStart;
    public int enemiesAlive = 0;
    public int enemiesToSpawnTotal = 0;
    public int enemiesSpawnedSoFar = 0;

    // Events
    public static event Action<int> OnWaveStarted;
    public static event Action OnWaveEnded;
    public static event Action OnCalmPhaseStarted;

    public static HordeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
        
        // Subscribe to enemy death
        EnemyController.OnEnemyDeath += HandleEnemyDeath;

        // Start first wave automatically for now, or wait for player?
        // Let's start after a short delay
        StartCoroutine(StartFirstWaveRoutine());
    }

    private void OnDestroy()
    {
        EnemyController.OnEnemyDeath -= HandleEnemyDeath;
    }

    private IEnumerator StartFirstWaveRoutine()
    {
        yield return new WaitForSeconds(2f);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentState == HordeState.Spawning || currentState == HordeState.Fighting) return;

        currentWave++;
        currentState = HordeState.Spawning;
        
        // Calculate enemies for this wave
        enemiesToSpawnTotal = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(difficultyMultiplier, currentWave - 1));
        enemiesSpawnedSoFar = 0;
        enemiesAlive = enemiesToSpawnTotal;

        Debug.Log($"Starting Wave {currentWave}. Enemies to spawn: {enemiesToSpawnTotal}");
        OnWaveStarted?.Invoke(currentWave);

        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        // Spawn enemies over time, not all at once
        while (enemiesSpawnedSoFar < enemiesToSpawnTotal)
        {
            // If we somehow entered calm phase (e.g. forced end), stop spawning
            if (currentState == HordeState.CalmPhase) yield break;

            SpawnOneEnemy();
            enemiesSpawnedSoFar++;
            // Wait a bit between spawns so they don't stack perfectly
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        }

        currentState = HordeState.Fighting;
        Debug.Log("All enemies spawned. Fighting phase.");
    }

    private void SpawnOneEnemy()
    {
        GameObject enemyPrefab = GetRandomEnemyPrefab();
        if (enemyPrefab != null)
        {
            // Use the spawner to spawn 1 enemy
            spawner.SpawnBatch(enemyPrefab, 1);
        }
    }

    private void HandleEnemyDeath(EnemyController enemy)
    {
        // If we are already in CalmPhase, ignore deaths (shouldn't happen if cleanup works, but for safety)
        if (currentState == HordeState.CalmPhase) return;

        enemiesAlive--;
        Debug.Log($"Enemy died. Enemies remaining: {enemiesAlive}");

        if (enemiesAlive <= 0 && enemiesSpawnedSoFar >= enemiesToSpawnTotal)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        currentState = HordeState.CalmPhase;
        Debug.Log("Wave Ended! Entering Calm Phase.");
        
        CleanupProjectiles(); // Clean up remaining projectiles
        
        OnWaveEnded?.Invoke();
        
        // Trigger Calm Phase Logic (Power Up Selection)
        StartCalmPhase();
    }

    private void CleanupProjectiles()
    {
        // Find all active enemy projectiles and despawn them
        // Since we are using pooling, we can just find them by type and despawn
        EnemyProjectile[] projectiles = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);
        foreach (var p in projectiles)
        {
            PoolManager.Instance.Despawn(p.gameObject);
        }
        Debug.Log($"Cleaned up {projectiles.Length} enemy projectiles.");
    }

    private List<GameObject> activePickups = new List<GameObject>();
    public List<WeaponData> availableUpgrades; // Pool of weapon upgrades
    public List<StatUpgradeData> availableStatUpgrades; // Pool of stat upgrades
    public GameObject upgradePickupPrefab; // Prefab with UpgradePickup component
    public Transform[] upgradeSpawnPoints; // Where to spawn them

    private void StartCalmPhase()
    {
        OnCalmPhaseStarted?.Invoke();
        Debug.Log(">> CHOOSE YOUR POWER UP! <<");
        
        SpawnUpgrades();
    }
    private void SpawnUpgrades()
    {
        // Clear old pickups if any
        ClearPickups();

        if (upgradePickupPrefab == null)
        {
            Debug.LogWarning("Cannot spawn upgrades: Missing prefab.");
            StartNextWave(); // Fallback
            return;
        }

        // Combine lists for selection
        int totalOptions = availableUpgrades.Count + availableStatUpgrades.Count;
        if (totalOptions == 0)
        {
             Debug.LogWarning("No upgrades available.");
             StartNextWave();
             return;
        }

        // Pick 2 or 3 random upgrades
        int count = Mathf.Min(3, totalOptions);
        
        // Create a list of indices to pick from [0...totalOptions-1]
        List<int> indices = new List<int>();
        for(int k=0; k<totalOptions; k++) indices.Add(k);

        for (int i = 0; i < count; i++)
        {
            if (i >= upgradeSpawnPoints.Length) break;
            if (indices.Count == 0) break;

            int randIndex = UnityEngine.Random.Range(0, indices.Count);
            int selection = indices[randIndex];
            indices.RemoveAt(randIndex);

            Vector3 pos = upgradeSpawnPoints[i].position;
            GameObject pickupObj = Instantiate(upgradePickupPrefab, pos, Quaternion.identity);
            UpgradePickup pickup = pickupObj.GetComponent<UpgradePickup>();
            
            if (selection < availableUpgrades.Count)
            {
                pickup.weaponUpgrade = availableUpgrades[selection];
                pickup.statUpgrade = null;
            }
            else
            {
                pickup.statUpgrade = availableStatUpgrades[selection - availableUpgrades.Count];
                pickup.weaponUpgrade = null;
            }
            
            activePickups.Add(pickupObj);
        }
    }

    public void OnUpgradePicked(UpgradePickup picked)
    {
        if (currentState != HordeState.CalmPhase) return;

        string name = "Unknown";
        if (picked.weaponUpgrade != null) name = picked.weaponUpgrade.weaponName;
        else if (picked.statUpgrade != null) name = picked.statUpgrade.upgradeName;

        Debug.Log($"Upgrade picked: {name}");
        
        ClearPickups();
        StartNextWave();
    }

    private void ClearPickups()
    {
        foreach (var p in activePickups)
        {
            if (p != null) Destroy(p);
        }
        activePickups.Clear();
    }

    private GameObject GetRandomEnemyPrefab()
    {
        // Simple random logic, can be improved with weights later
        if (enemyTypes.Count == 0) return null;
        
        // Filter by difficulty if needed, or just random for now
        List<EnemyType> valid = new List<EnemyType>();
        foreach(var e in enemyTypes)
        {
            if (currentWave >= e.minDifficultyToSpawn) valid.Add(e);
        }

        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)].prefab;
    }
}

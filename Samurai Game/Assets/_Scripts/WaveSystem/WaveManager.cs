using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Debugging Settings")]

    [Tooltip("If true, the game will not save the highest wave reached")]
    [SerializeField] private bool isDebugging = false;


    [Header("Wave Settings")]

    [SerializeField] private string levelName = "";

    [ReadOnlyInspector]
    [SerializeField] private int currentWave = 1;

    [ReadOnlyInspector]
    [SerializeField] private int currentEnemies = 0;

    [ReadOnlyInspector]
    [SerializeField] private int enemiesLeftToSpawn = 0;

    [Min(0)]
    [SerializeField] private int maxEnemiesPerWave = 10;

    [Min(0)]
    [Tooltip("The delay between which enemies are spawned")]
    [SerializeField] private float spawnDelay = 0.25f;

    [Min(0)]
    [Tooltip("The time between when a enemy spawn is chosen and when it is deployed")]
    [SerializeField] private float deployDelay = 1.0f;

    [SerializeField] private LayerMask groundMask;

    [Tooltip("The VFX that appears when a location for the enemy to spawn is chosen")]
    [SerializeField] private GameObject vfxSpawnPrefab;

    [Tooltip("The VFX that appears when the enemy is deployed, after the spawn delay")]
    [SerializeField] private GameObject vfxDeployPrefab;

    [SerializeField] private int vfxPoolSize = 5;

    [SerializeField] private List<EnemyData> enemiesToSpawnList = new();


    [Header("References")]

    [SerializeField] private TMP_Text waveCounter;


    private GridGraph gridGraph;
    private readonly List<GraphNode> spawnableNodes = new();

    private List<GameObject> spawnedEnemies = new();
    private List<ParticleSystem> spawnParticles = new();
    private List<ParticleSystem> deployParticles = new();


    private void Awake()
    {
        enemiesLeftToSpawn = currentWave;
    }

    private void Start()
    {
        gridGraph = AstarPath.active.data.gridGraph;
        CacheSpawnableNodes();

        // INFO: Pool VFX to match max enemies per wave
        if (vfxSpawnPrefab != null)
        {
            GameObject spawnParticlePool = new("SpawnParticlePool");
            spawnParticlePool.transform.SetParent(transform);

            for (int i = 0; i < vfxPoolSize; i++)
            {
                GameObject spawnVFX = Instantiate(vfxSpawnPrefab);
                spawnVFX.transform.SetParent(spawnParticlePool.transform);

                // INFO: Set the spawn VFX to play for the deploy delay duration
                ParticleSystem[] particles = spawnVFX.GetComponents<ParticleSystem>();

                if (particles.Length > 0)
                {
                    foreach (ParticleSystem particle in particles)
                    {
                        ParticleSystem.MainModule main = particle.main;
                        main.duration = deployDelay;
                    }
                }

                spawnParticles.Add(spawnVFX.GetComponent<ParticleSystem>());
            }
        }

        if (vfxDeployPrefab != null)
        {
            GameObject deployParticlePool = new("DeployParticlePool");
            deployParticlePool.transform.SetParent(transform);

            for (int i = 0; i < vfxPoolSize; i++)
            {
                GameObject deployVFX = Instantiate(vfxDeployPrefab);
                deployVFX.transform.SetParent(deployParticlePool.transform);

                deployParticles.Add(deployVFX.GetComponent<ParticleSystem>());
            }
        }

        // INFO: Subscribe to player death to reset game
        PlayerHealthController player = FindFirstObjectByType<PlayerHealthController>();
        if (player) { player.OnPlayerRespawnEvent += ResetGame; }

        SetWaveCounterText();
        StartCoroutine(SpawnEnemyCoroutine());
    }

    private void CacheSpawnableNodes()
    {
        gridGraph.GetNodes(node =>
        {
            if (node.Walkable)
            {
                spawnableNodes.Add(node);
            }
        });
    }

    private ParticleSystem GetAvailableSpawnVFX()
    {
        foreach (ParticleSystem spawnVFX in spawnParticles)
        {
            if (spawnVFX.isPlaying) { continue; }

            return spawnVFX;
        }

        // INFO: If all Spawn VFX are busy, we add a new one
        if (vfxSpawnPrefab != null)
        {
            GameObject spawnParticlePool = transform.Find("SpawnParticlePool").gameObject;

            GameObject spawnVFX = Instantiate(vfxSpawnPrefab);
            spawnVFX.transform.SetParent(spawnParticlePool.transform);

            // INFO: Set the spawn VFX to play for the deploy delay duration
            ParticleSystem[] particles = spawnVFX.GetComponents<ParticleSystem>();

            if (particles.Length > 0)
            {
                foreach (ParticleSystem particle in particles)
                {
                    ParticleSystem.MainModule main = particle.main;
                    main.duration = deployDelay;
                }
            }

            ParticleSystem spawnParticle = spawnVFX.GetComponent<ParticleSystem>();
            spawnParticles.Add(spawnParticle);

            return spawnParticle;
        }

        return null;
    }

    private ParticleSystem GetAvailableDeployVFX()
    {
        foreach (ParticleSystem deployVFX in deployParticles)
        {
            if (deployVFX.isPlaying) { continue; }

            return deployVFX;
        }

        // INFO: If all Deploy VFX are busy, we add a new one
        if (vfxDeployPrefab != null)
        {
            GameObject deployParticlePool = transform.Find("DeployParticlePool").gameObject;

            GameObject deployVFX = Instantiate(vfxDeployPrefab);
            deployVFX.transform.SetParent(deployParticlePool.transform);

            ParticleSystem deployParticle = deployVFX.GetComponent<ParticleSystem>();
            deployParticles.Add(deployParticle);

            return deployParticle;
        }

        return null;
    }

    private Vector2 GetRandomSpawnablePosition()
    {
        if (spawnableNodes.Count == 0)
        {
            Debug.LogWarning("No spawnable nodes found.");
            return Vector2.zero;
        }

        int randomIndex = Random.Range(0, spawnableNodes.Count);
        GraphNode randomNode = spawnableNodes[randomIndex];
        return (Vector3)randomNode.position;
    }

    private void SpawnEnemy()
    {
        // INFO: Keep supplying enemies as long as there is space in the wave and enemies left to spawn 
        if (currentEnemies < maxEnemiesPerWave && enemiesLeftToSpawn > 0)
        {
            InstantiateEnemy();
        }
        // INFO: Spawn the next wave
        else if (currentEnemies <= 0 && enemiesLeftToSpawn <= 0)
        {
            currentWave++;
            currentEnemies = 0;
            enemiesLeftToSpawn = currentWave;

            AudioManager.Instance.PlaySFX("WaveComplete", 0.5f, true, waveCounter.transform.position, 5.0f, 25.0f);
            SetWaveCounterText();
            StartCoroutine(SpawnEnemyCoroutine());
        }
    }

    private IEnumerator SpawnEnemyCoroutine()
    {
        // INFO: Supplies max enemies at start of wave, then enemy spawning is handled by each enemies death event
        while (currentEnemies < maxEnemiesPerWave)
        {
            if (enemiesLeftToSpawn <= 0) { yield break; }

            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void InstantiateEnemy()
    {
        if (enemiesToSpawnList.Count == 0)
        {
            Debug.LogWarning("No enemies to spawn.");
            return;
        }

        int randomIndex = Random.Range(0, enemiesToSpawnList.Count);
        EnemyData enemyData = enemiesToSpawnList[randomIndex];

        Vector2 spawnPosition = GetRandomSpawnablePosition();

        // INFO: Find ground if the enemy is a ground type
        if (enemyData.isGrounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(spawnPosition, Vector2.down, Mathf.Infinity, groundMask);

            if (hit.collider != null)
            {
                spawnPosition = hit.point + enemyData.groundOffset;
            }
        }

        // INFO: Play Spawn VFX
        ParticleSystem spawnVFX = GetAvailableSpawnVFX();

        if (spawnVFX != null)
        {
            spawnVFX.transform.position = spawnPosition;
            spawnVFX.Play(true);
        }

        // INFO: Update the current enemies and enemies left to spawn
        currentEnemies++;
        enemiesLeftToSpawn--;

        // INFO: Spawn the enemy
        StartCoroutine(InstantiateEnemyCoroutine(spawnPosition, enemyData));
    }

    private IEnumerator InstantiateEnemyCoroutine(Vector2 spawnPosition, EnemyData enemyData)
    {
        yield return new WaitForSeconds(deployDelay);

        AudioManager.Instance.PlaySFX("Spawn", 0.25f, true, spawnPosition);

        // INFO: Play Deploy VFX
        ParticleSystem deployVFX = GetAvailableDeployVFX();

        if (deployVFX != null)
        {
            deployVFX.transform.position = spawnPosition;
            deployVFX.Play(true);
        }

        // INFO: Keep track of the spawned enemies
        GameObject gameObject = Instantiate(enemyData.enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(gameObject);

        if (gameObject.TryGetComponent(out AICharacter enemy))
        {
            // INFO: Subscribe to enemy death event
            enemy.OnEnemyDeathEvent += () => currentEnemies--;
            enemy.OnEnemyDeathEvent += SpawnEnemy;
        }
    }

    private void SetWaveCounterText()
    {
        if (waveCounter != null)
        {
            if (currentWave / 10.0f < 1.0f)
            {
                waveCounter.text = "0" + currentWave.ToString();
            }
            else
            {
                waveCounter.text = currentWave.ToString();
            }
        }
    }

    private void ResetGame()
    {
        SaveHighestWave();

        currentWave = 1;
        enemiesLeftToSpawn = 1;
        currentEnemies = 0;
        
        foreach (GameObject enemy in spawnedEnemies)
        {
            Destroy(enemy);
        }

        spawnedEnemies.Clear();

        SetWaveCounterText();
        StartCoroutine(SpawnEnemyCoroutine());
    }

    public void SaveHighestWave()
    {
        if (isDebugging) { return; }

        // INFO: Compare GameData Wave to current wave and save if the current wave is higher
        GameData gameData = SavingSystem.LoadGameData();

        if (gameData == null)
        {
            Debug.LogWarning("GameData is null. Cannot save highest wave.");
            return;
        }

        // INFO: Get the highest wave for the current level from GameData
        if (gameData.levelNames.Contains(levelName))
        {
            int index = gameData.levelNames.IndexOf(levelName);

            // INFO: Check if the current wave is higher than the saved highest wave for this level
            if (gameData.highestWavePerLevel[index] < currentWave - 1)
            {
                gameData.highestWavePerLevel[index] = currentWave - 1;
            }
        }

        // INFO: Check if the current wave is higher than the global highest wave
        if (gameData.highestWave < currentWave - 1)
        {
            gameData.highestWave = currentWave - 1;
        }

        SavingSystem.SaveGameData(gameData);
    }
}

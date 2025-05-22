using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]

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

    [SerializeField] private List<EnemyData> enemiesToSpawnList = new();


    [Header("References")]

    [SerializeField] private TMP_Text waveCounter;


    private GridGraph gridGraph;
    private readonly List<GraphNode> spawnableNodes = new();
    private List<GameObject> spawnedEnemies = new();


    private void Awake()
    {
        enemiesLeftToSpawn = currentWave;
    }

    private void Start()
    {
        gridGraph = AstarPath.active.data.gridGraph;
        CacheSpawnableNodes();

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

        // INFO: Spawn the spawn VFX
        if (vfxSpawnPrefab != null)
        {
            GameObject particleObject = Instantiate(vfxSpawnPrefab, spawnPosition, Quaternion.identity);

            // INFO: Make the particle object destroy itself after the spawn delay
            Destroy(particleObject, deployDelay);

            ParticleSystem[] particles = particleObject.GetComponents<ParticleSystem>();

            if (particles.Length > 0)
            {
                foreach (ParticleSystem particle in particles)
                {
                    ParticleSystem.MainModule main = particle.main;
                    main.duration = deployDelay;
                    particle.Play();
                }
            }
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

        // INFO: Spawn the deploy VFX
        if (vfxDeployPrefab != null)
        {
            GameObject particleObject = Instantiate(vfxDeployPrefab, spawnPosition, Quaternion.identity);

            // INFO: Make the particle object destroy itself after the main particle system is done
            if (particleObject.TryGetComponent(out ParticleSystem particleSystem))
            {
                Destroy(particleObject, particleSystem.main.duration);
            }
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
}

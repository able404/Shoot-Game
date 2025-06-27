using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    LivingEntity playerEntity;
    Transform playerTransform;
    MapGenerator map;

    Wave currentWave;
    int currentWaveNum;

    float timeBetweenCampingChecks = 2f;
    float campThreshholdDistance = 1f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    public Wave[] waves;
    public Enemy enemy;

    void Start()
    {
        playerEntity = FindFirstObjectByType<PlayerController>();
        playerEntity.OnDeath += OnPlayerDeath;
        playerTransform = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerTransform.position;

        map = FindFirstObjectByType<MapGenerator>(); 
        NextWave();
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerTransform.position, campPositionOld) < campThreshholdDistance);
                campPositionOld = playerTransform.position;
            }

            if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1f;
        float tileFlashSpeed = 4f;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerTransform.position);
        }

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0f;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        if (enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void ResetPlayerPosition()
    {
        playerTransform.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 5f;
        Debug.Log("reset");
    }

    void NextWave()
    {
        currentWaveNum++;

        if (currentWaveNum - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNum - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNum);
            }
            Debug.Log("reset ready");
            ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawns;
    }
}

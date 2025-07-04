using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Spawner : MonoBehaviour
{
    InputSystem_Actions inputActions;
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

    public bool devMode;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.NextWave.Enable();

        inputActions.UI.NextWave.performed += SkipToNextWave;
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.NextWave.performed -= SkipToNextWave;
            inputActions.UI.NextWave.Disable();
        }
    }

    void SkipToNextWave(InputAction.CallbackContext context)
    {
        if (!devMode) return;

        StopCoroutine("SpawnEnemy");
        foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                Destroy(enemyObj);
            }
        }
        NextWave();
    }

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

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
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
        Color initialColor = new Color(0.7f, 0.7f, 0.7f, 1f);
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
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth);
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
        var characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerTransform.position = (Vector3.up * 5f) + (map.GetTileFromPosition(Vector3.zero).position);

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Reset player state
        var playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetMovementState();
        }
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

            if (playerEntity != null)
            {
                playerEntity.RestoreFullHealth();
            }

            ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
    }
}

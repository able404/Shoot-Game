using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    static int killCount = 0;

    public static int score { get; private set; }

    public static event System.Action OnTenKills;

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
    }

    void OnDestroy()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }

    void OnEnemyKilled(float enemyHealth)
    {
        score += Mathf.RoundToInt(enemyHealth * 10);

        killCount++;
        if (killCount > 0 && killCount % 10 == 0)
        {
            OnTenKills?.Invoke();
        }
    }

    public static void Reset()
    {
        score = 0;
        killCount = 0;
    }
}

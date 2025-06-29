using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    void Start()
    {
        FindFirstObjectByType<PlayerController>().OnDeath += OnGameOver;
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0f, 0f, 0f, .8f), 1f));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1f / time;
        float percent = 0f;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    // UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}

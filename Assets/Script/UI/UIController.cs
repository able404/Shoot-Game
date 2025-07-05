using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public Slider healthSlider;
    public TextMeshProUGUI fireModeText;
    public TextMeshProUGUI ammoText;

    void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            player.OnDeath += OnGameOver;

            if (healthSlider != null)
            {
                healthSlider.maxValue = player.startingHealth;
                healthSlider.value = player.startingHealth;
            }
        }
    }

    public void UpdateAmmoUI(int currentAmmo, int magSize)
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + magSize;
        }
    }

    public void UpdateFireModeUI(string modeName)
    {
        if (fireModeText != null)
        {
            fireModeText.text = "Current Fire Mode: " + modeName;
        }
    }

    public void UpdateHealthUI(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0f, 0f, 0f, .8f), 1f));
        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (gameOverUI == null || !gameOverUI.activeSelf)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    public void StartNewGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene("GameScene");
    }
}

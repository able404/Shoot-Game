using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UIController : MonoBehaviour
{
    InputSystem_Actions inputActions;

    [Header("Fade")]
    public Image fadePlane;

    [Header("Game Over UI")]
    public GameObject gameOverUI;

    [Header("Volume Setting UI")]
    public GameObject volumeSettingUI;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Player UI")]
    public Slider healthSlider;
    public TextMeshProUGUI fireModeText;
    public TextMeshProUGUI ammoText;

    bool isPaused = false;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Pause.Enable();
        inputActions.UI.Pause.performed += OnPause;
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed -= OnPause;
            inputActions.UI.Pause.Disable();
        }
    }

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

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.instance.GetVolume("Master");
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.instance.GetVolume("Music");
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.instance.GetVolume("SFX");
        }
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (!isPaused)
        {
            StartCoroutine(Fade(Color.clear, new Color(0f, 0f, 0f, .8f), .5f));
            volumeSettingUI.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            isPaused = true;
        }
        else
        {
            StartCoroutine(Fade(new Color(0f, 0f, 0f, .8f), Color.clear, .5f));
            volumeSettingUI.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;

            isPaused = false;
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

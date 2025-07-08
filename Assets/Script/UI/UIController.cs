using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public enum GameState { PreGame, Running, Paused, GameOver }

public class UIController : MonoBehaviour
{
    InputSystem_Actions inputActions;

    [Header("Fade")]
    public Image fadePlane;

    [Header("Start UI")]
    public GameObject startUI;
    public Image startFadeImage;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;

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

    public static GameState CurrentState { get; private set; }

    void Awake()
    {
        CurrentState = GameState.PreGame;

        inputActions = new InputSystem_Actions();
        inputActions.UI.Pause.Enable();
        inputActions.UI.Pause.performed += OnPause;

        inputActions.UI.StartGame.Enable();
        inputActions.UI.StartGame.performed += OnStartGame;
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.Pause.performed -= OnPause;
            inputActions.UI.Pause.Disable();

            inputActions.UI.StartGame.performed -= OnStartGame;
            inputActions.UI.StartGame.Disable();
        }
    }

    void Start()
    {
        if (AudioManager.IsRestarting)
        {
            CurrentState = GameState.Running; 
            startUI.SetActive(false);        
            gameOverUI.SetActive(false);  
            volumeSettingUI.SetActive(false); 

            FindFirstObjectByType<Spawner>()?.NextWave();

            AudioManager.IsRestarting = false;
            ScoreKeeper.Reset();
        }
        else
        {
            startUI.SetActive(true);
            gameOverUI.SetActive(false);
            volumeSettingUI.SetActive(false);

            StartCoroutine(PulsateFade());
        }

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

        if (AudioManager.instance != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.instance.GetVolume("Master");
                masterVolumeSlider.onValueChanged.AddListener(AudioManager.instance.SetMasterVolume);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.instance.GetVolume("Music");
                musicVolumeSlider.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.instance.GetVolume("SFX");
                sfxVolumeSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
            }
        }
    }

    void Update()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + ScoreKeeper.score.ToString("D5");
        }
    }

    void OnStartGame(InputAction.CallbackContext context)
    {
        if (CurrentState == GameState.PreGame)
        {
            CurrentState = GameState.Running;
            startUI.SetActive(false);
            StopCoroutine(PulsateFade()); 

            FindFirstObjectByType<Spawner>()?.NextWave();
            ScoreKeeper.Reset();
        }
    }

    IEnumerator PulsateFade()
    {
        float fadeDuration = 1.5f;
        Color originalColor = startFadeImage.color;

        while (CurrentState == GameState.PreGame)
        {
            // Use Mathf.PingPong to create a smooth back-and-forth loop from 0.5 to 1
            float alpha = Mathf.PingPong(Time.time / fadeDuration, 0.5f) + 0.5f;
            startFadeImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }

    void OnPause(InputAction.CallbackContext context)
    {
        if (CurrentState == GameState.Running)
        {
            CurrentState = GameState.Paused;

            StartCoroutine(Fade(Color.clear, new Color(0f, 0f, 0f, .9f), .5f));
            volumeSettingUI.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Running;

            StartCoroutine(Fade(new Color(0f, 0f, 0f, .9f), Color.clear, .5f));
            volumeSettingUI.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
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
        CurrentState = GameState.GameOver;
        StartCoroutine(Fade(Color.clear, new Color(0f, 0f, 0f, .9f), 1f));

        volumeSettingUI.SetActive(false);
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
            percent += Time.unscaledDeltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame()
    {
        AudioManager.IsRestarting = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene("GameScene");
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
}

using UnityEngine;
using UnityEngine.InputSystem;

public class MusicManager : MonoBehaviour
{
    InputSystem_Actions inputActions;

    bool isMenuMusic = true;

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.StartGame.Enable();
        inputActions.UI.StartGame.performed += SwitchMusic;
    }

    void OnDisable()
    {
        if (inputActions!= null)
        {
            inputActions.UI.StartGame.performed -= SwitchMusic;
            inputActions.UI.StartGame.Disable();
        }
    }

    void Start()
    {
        if (AudioManager.instance != null && AudioManager.IsRestarting)
        {
            isMenuMusic = false;
            AudioManager.instance.PlayMusic(mainTheme, 0f);
        }
        else
        {
            isMenuMusic = true;
            AudioManager.instance.PlayMusic(menuTheme, 2f);
        }
    }

    void SwitchMusic(InputAction.CallbackContext context)
    {
        if (!isMenuMusic)  return;

        AudioManager.instance.PlayMusic(mainTheme, 3f);
        isMenuMusic = false;
    }
}

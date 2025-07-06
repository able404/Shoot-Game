using UnityEngine;
using UnityEngine.InputSystem;

public class MusicManager : MonoBehaviour
{
    InputSystem_Actions inputActions;

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.SwitchToMainMusic.Enable();
        inputActions.UI.SwitchToMainMusic.performed += SwitchMusic;
    }

    void OnDisable()
    {
        if (inputActions!= null)
        {
            inputActions.UI.SwitchToMainMusic.performed -= SwitchMusic;
            inputActions.UI.SwitchToMainMusic.Disable();
        }
    }

    void Start()
    {
        AudioManager.instance.PlayMusic(menuTheme, 2f);
    }

    void Update()
    {
        
    }

    void SwitchMusic(InputAction.CallbackContext context)
    {
        AudioManager.instance.PlayMusic(mainTheme, 3f);
    }
}

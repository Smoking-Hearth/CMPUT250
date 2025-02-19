using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    enum State { Showing, Hiding }

    [SerializeField] CanvasGroup pauseScreen;
    [SerializeField] Button resume, menu;

    private State state;
    private MotionHandle anim = MotionHandle.None;
    private LevelState prevState = LevelState.None;

    void Awake()
    {
        pauseScreen.alpha = 0f;
        pauseScreen.interactable = false;
        state = State.Hiding;
    }

    void OnEnable()
    {
        PlayerController.Controls.Menu.Pause.performed += TogglePauseKeybind;
        resume.onClick.AddListener(OnResumeClick);
        menu.onClick.AddListener(OnMenuClick);
    }

    void OnDisable()
    {
        PlayerController.Controls.Menu.Pause.performed -= TogglePauseKeybind;
        resume.onClick.RemoveListener(OnResumeClick);
        menu.onClick.RemoveListener(OnMenuClick);
    }

    public void TogglePauseKeybind(InputAction.CallbackContext callbackContext)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (prevState == LevelState.None)
        {
            prevState = gameObject.MyLevelManager().levelState;
            gameObject.MyLevelManager().levelState = LevelState.Paused;
        }
        else
        {
            gameObject.MyLevelManager().levelState = prevState;
            prevState = LevelState.None;
        }
    }

    void Update()
    {
        LevelState levelState = gameObject.MyLevelManager().levelState;
        if (levelState == LevelState.Paused && state == State.Hiding)
        {
            ShowScreen();
        }
        else if (levelState != LevelState.Paused && state == State.Showing)
        {
            HideScreen();
        }
        if (pauseScreen.alpha >= 0.999f)
        {
            pauseScreen.interactable = true;
        }
        else
        {
            pauseScreen.interactable = false;
        }
    }

    void ShowScreen()
    {
        state = State.Showing;
        anim.TryCancel();
        anim = LMotion.Create(pauseScreen.alpha, 1f, 0.3f)
            .BindToAlpha(pauseScreen);
    }

    void HideScreen()
    {
        state = State.Hiding;
        anim.TryCancel();
        anim = LMotion.Create(pauseScreen.alpha, 0f, 0.3f)
            .BindToAlpha(pauseScreen);
    }

    void OnResumeClick()
    {
        TogglePause();
    }

    void OnMenuClick()
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.MainMenu));
    }
}
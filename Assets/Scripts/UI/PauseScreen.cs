using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    enum State { Showing, Hiding }

    [SerializeField] CanvasGroup pauseScreen;
    [SerializeField] Button resume, settings, menu;

    private State state;
    private MotionHandle anim = MotionHandle.None;

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
        settings.onClick.AddListener(OnSettingsClick);
        menu.onClick.AddListener(OnMenuClick);
    }

    void OnDisable()
    {
        PlayerController.Controls.Menu.Pause.performed -= TogglePauseKeybind;
        resume.onClick.RemoveListener(OnResumeClick);
        settings.onClick.AddListener(OnSettingsClick);
        menu.onClick.RemoveListener(OnMenuClick);
    }

    public void TogglePauseKeybind(InputAction.CallbackContext callbackContext)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        LevelManager levelManager = gameObject.MyLevelManager();
        levelManager.SetPause(levelManager.levelState != LevelState.Paused);    //If the level is not paused, pause
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
            pauseScreen.blocksRaycasts = true;
        }
        else
        {
            pauseScreen.interactable = false;
            pauseScreen.blocksRaycasts = false;
        }
    }

    void ShowScreen()
    {
        state = State.Showing;
        anim.TryCancel();
        anim = LMotion.Create(pauseScreen.alpha, 1f, 0.3f)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
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

    void OnSettingsClick()
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Settings, keepInHistory: true));
    }

    void OnMenuClick()
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.MainMenu));
    }
}
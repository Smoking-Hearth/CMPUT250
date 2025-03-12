using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class LevelManagerExtension
{
    // This will get the `LevelManager` for the scene the gameObject 
    // belongs to. This is basically an alias and the most robust way
    public static LevelManager MyLevelManager(this GameObject gameObject)
    {
        int buildIndex = gameObject.scene.buildIndex;
        if (!GameManager.SceneSystem.IsLoaded(buildIndex))
        {
            // The Coroutine running the load wan't polled. But this object is calling
            // it's start/awake so the rest of the Scene must be loaded.
            GameManager.SceneSystem.RegisterScene(gameObject.scene);
        }
        return GameManager.SceneSystem.LevelManagers[gameObject.scene.buildIndex];
    }

    public static bool MyLevelLoaded(this GameObject gameObject)
    {
        return GameManager.SceneSystem.IsLoaded(gameObject.scene.buildIndex);
    }

    /// <summary>
    ///  Use this to guard Update and FixedUpdate
    /// </summary>
    public static bool ShouldUpdate(this GameObject gameObject)
    {
        if (!GameManager.IsInit) return false;
        LevelManager levelManager = gameObject.MyLevelManager();
        if (levelManager == null) return false;
        return levelManager.IsLevelRunning;
    }
}

public enum LevelCommand
{
    Load,
    Activate,
    Deactivate,
    Unload,
}

// TODO: Give this info about how it got loaded.
public class LevelManager : MonoBehaviour 
{
    [Header("Systems")]
    [field: SerializeField] public DialogSystem DialogSystem;
    [field: SerializeField] public EventSystem EventSystem;
    [field: SerializeField] public CheckpointSystem CheckpointSystem;
    [field: SerializeField] public InteractableSystem InteractableSystem;
    [field: SerializeField] public MusicSystem MusicSystem;
    [field: SerializeField] public TimeSystem TimeSystem;
    [field: SerializeField] public AudioListener AudioListener;

    [Header("State")]
    [SerializeField] public LevelState levelState;
    private LevelState prevState;
    [SerializeField] public Animator cameraAnimator;

    [Header("Key Objects")]
    [SerializeField] public FireSettings fireSettings;
    [SerializeField] public List<GameObject> UI;
    [field: SerializeField] public Camera LevelCamera; 

    [SerializeField] private GameObject gameOverScreen;

    [SerializeField] private Rigidbody2D setPlayer;
    private Player player;
    public Player Player
    {
        get
        {
            if (player == null)
            {
                player = new Player(setPlayer);
                SavedVariableAccess playerPosAccess = new();
                
                playerPosAccess.Get += () => {
                    return setPlayer.position;
                };

                playerPosAccess.Set += (pos) => { 
                    setPlayer.position = (Vector2)pos;
                };

                CheckpointSystem.RegisterState(playerPosAccess);
            }
            return player;
        }
    }
    public GameObject[] defaultEnabledRootObjects;
    
    [SerializeField] private bool isLevelRunning = false;
    public bool IsLevelRunning
    {
        get { return isLevelRunning; }
    }

    public delegate void OnFireTick();
    public event OnFireTick onFireTick;
    private float fireTickTimer;
    
    public delegate void LoadCallback();
    public event LoadCallback onLoad;

    public delegate void UnloadCallback();
    public event UnloadCallback onUnload;

    public delegate void ActivateCallback();
    public event ActivateCallback onActivate;

    public delegate void DeactivateCallback();
    public event DeactivateCallback onDeactivate;

    private readonly Queue<LevelCommand> callbackCommands = new();

    // Why a Queue? There is a race condition here hiding with a gun.
    // - GameManager.Init() runs in Awake
    // - Objects subscribe to the lifecycle stuff in Start (hopefully)
    // - GameManager initializes the SceneSystem which triggers the active levels
    // hooks for onLoad and onActivate.
    // If we aren't careful the setup and activation code won't be run.
    public void NotifyLevel(LevelCommand cmd)
    {
        callbackCommands.Enqueue(cmd);
    }

    void Awake()
    {
        GameManager.Init(fireSettings);
    }

    public void Activate()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(true);
        LevelCamera.gameObject.SetActive(true);
        LevelCamera.enabled = true;

        if (EventSystem != null)
            EventSystem.enabled = true;
        if (AudioListener != null)
            AudioListener.enabled = true;

        if (cameraAnimator != null)
            cameraAnimator.Play("Game");

        foreach (var uiObject in UI)
        {
            uiObject.SetActive(true);
        }
    }

    public void Deactivate()
    {
        foreach (var uiObject in UI)
        {
            uiObject.SetActive(false);
        }
        LevelCamera?.gameObject.SetActive(false);

        if (EventSystem != null)
            EventSystem.enabled =  false;
        if (AudioListener != null)
            AudioListener.enabled = false;
        
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (callbackCommands.TryDequeue(out LevelCommand cmd))
        {
            switch (cmd)
            {
                case LevelCommand.Load:
                    onLoad?.Invoke();
                    break;
                case LevelCommand.Activate:
                    try 
                    {
                        onActivate?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    isLevelRunning = true;
                    break;
                case LevelCommand.Deactivate:
                    isLevelRunning = false;
                    onDeactivate?.Invoke();
                    break;
                case LevelCommand.Unload:
                    onUnload?.Invoke();
                    break;
            }
        }
    }
    public void GameOver()
    {
        if (!gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
            PlayerController.Controls.Disable();
        }
    }

    public void SetPause(bool paused)
    {
        if (paused)
        {
            prevState = levelState;
            levelState = LevelState.Paused;
            Time.timeScale = 0;
        }
        else
        {
            levelState = prevState;
            prevState = LevelState.None;
            Time.timeScale = 1;
        }
    }

    public void SetCutsceneState(bool cutsceneOn)
    {
        if (cutsceneOn)
        {
            levelState = LevelState.Cutscene;
        }
        else
        {
            levelState = LevelState.Playing;
        }
    }
    private void FixedUpdate()
    {
        switch(levelState)
        {
            case LevelState.Paused:
                break;
            case LevelState.Playing:
                if (fireTickTimer > 0)
                {
                    fireTickTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    fireTickTimer = GameManager.FireSettings.FireDelay;
                    if (onFireTick != null)
                    {
                        onFireTick();
                    }
                }
                break;
            case LevelState.Defeat:
                GameOver();
                break;
            case LevelState.Win:
                break;
            case LevelState.Cutscene:
                break;
        }
    }

    public void Restartlevel()
    {
        gameOverScreen.SetActive(false);
    }

    public void RespawnCheckpoint()
    {
        // Teleports player to most recent checkpoint from checkPointManager, also resets health/healthbar
        PlayerController.Controls.Enable();
        gameOverScreen.SetActive(false);
        levelState = LevelState.Playing;

        CheckpointSystem.LoadState();
        Player.Health.ResetHealth();
    }
}

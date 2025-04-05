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
    [field: SerializeField] public TrackerSystem TrackerSystem;

    [Header("State")]
    [SerializeField] public LevelState levelState;
    private LevelState prevState;
    [SerializeField] public Animator cameraAnimator;

    [Header("Key Objects")]
    [SerializeField] public FireSettings fireSettings;
    [SerializeField] public List<GameObject> UI;
    [SerializeField] private float setCameraStateDelay; 
    [field: SerializeField] public Camera LevelCamera; 

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject deathOverlay;
    [field: SerializeField] public SettingsScreen settingsScreen { get; private set; }


    [SerializeField] private Rigidbody2D setPlayer;
    [SerializeField] private Rigidbody2D setSoul;
    private Player player;
    private Player soul;
    public Player Player
    {
        get
        {
            if (levelState != LevelState.Defeat)
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

            if (soul == null)
            {
                soul = new Player(setSoul);
            }
            return soul;
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
    private bool swapped = false;

    public delegate void OnPlayerRespawn();
    public static event OnPlayerRespawn onPlayerRespawn;    //event called when the player has finished their respawn animation

    public void DispatchCommand(LevelCommand cmd)
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

        foreach (var uiObject in UI)
        {
            uiObject.SetActive(true);
        }

        PlayerHealth.onDeath += GameOver;

        if (setPlayer != null)
        {
            setPlayer.gameObject.SetActive(true);
        }
        else
        {
            return;
        }

        if (soul == null)
        {
            soul = new Player(setSoul);
        }
    }

    public void Deactivate()
    {
        foreach (var uiObject in UI)
        {
            uiObject.SetActive(false);
        }
        LevelCamera?.gameObject.SetActive(false);

        if (setPlayer != null)
            setPlayer.gameObject.SetActive(false);

        if (EventSystem != null)
            EventSystem.enabled =  false;
        if (AudioListener != null)
            AudioListener.enabled = false;

        PlayerHealth.onDeath -= GameOver;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (callbackCommands.TryDequeue(out LevelCommand cmd))
        {
            DispatchCommand(cmd);
        }

        if (Time.time > setCameraStateDelay && !swapped)
        {
            if (cameraAnimator != null)
                cameraAnimator.Play("Game");

            swapped = true;
        }

        if (levelState == LevelState.Defeat && Vector2.Distance(setPlayer.transform.position, setSoul.transform.position) < 0.8f)
        {
            levelState = LevelState.Respawning;
            cameraAnimator.SetBool("IsDead", false);
            TrackerSystem.RemoveTracker(player.Movement.transform);
            soul.Movement.freeze = true;
            soul.Movement.ResetMovement();
            if (onPlayerRespawn != null)
            {
                onPlayerRespawn();
            }
        }
    }

    public void GameOver()
    {
        if (!gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
        }
        levelState = LevelState.Defeat;
        if (player != null)
        {
            player.Movement.freeze = true;
            player.Movement.ResetMovement();
        }
        if (soul != null)
        {
            soul.Movement.freeze = true;
            CheckpointSystem.ReturnToCheckpoint(soul.Movement.transform);
        }
        deathOverlay.SetActive(true);
        if (soul != null)
        {
            soul.Movement.ResetMovement();
            setSoul.gameObject.SetActive(true);
        }
        if (cameraAnimator != null)
            cameraAnimator.SetBool("IsDead", true);
    }
    public void GiveSoulControl()
    {
        gameOverScreen.SetActive(false);
        TrackerSystem.AddTracker(player.Movement.transform);
        if (soul != null)
        {
            soul.Movement.freeze = false;
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

    public void TransitionTo(int sceneIndex)
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.NextLevel(
                (SceneIndex)gameObject.scene.buildIndex, 
                (SceneIndex)sceneIndex
            ));
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
                FireSounds.UpdateGlobalHitTimer();
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
    }

    public void RespawnControl()
    {
        if (player != null)
        {
            player.Movement.freeze = false;
            player.Movement.ResetMovement();
        }
        if (soul != null)
        {
            setSoul.gameObject.SetActive(false);
        }
        levelState = LevelState.Playing;
        Player.Health.Current = 40;
        Player.Movement.ResetMovement();
        deathOverlay.SetActive(false);
    }
}

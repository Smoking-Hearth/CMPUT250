using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class LevelManagerExtension
{
    // This will get the `LevelManager` for the scene the gameObject 
    // belongs to. This is basically an alias and the most robust way
    public static LevelManager MyLevelManager(this GameObject gameObject)
    {
        return GameManager.SceneSystem.LevelManagers[gameObject.scene.buildIndex];
    }

    public static bool ShouldUpdate(this GameObject gameObject)
    {
        return GameManager.IsInit && gameObject.MyLevelManager().IsLevelActive;
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

    // To make this object available for editor callbacks
    [HideInInspector] public static GameManager GameManager
    {
        get { return GameManager.Instance; }
    }

    // For convenience
    [HideInInspector] public static LevelManager Active
    {
        get { return GameManager.SceneSystem.ActiveLevel; }
    }

    [Header("Systems")]
    [field: SerializeField] public DialogSystem DialogSystem;
    [field: SerializeField] public EventSystem EventSystem;
    [field: SerializeField] public CheckpointSystem CheckpointSystem;
    [field: SerializeField] public InteractableSystem InteractableSystem;
    [field: SerializeField] public MusicSystem MusicSystem;
    [field: SerializeField] public TimeSystem TimeSystem;

    [Header("State")]
    [SerializeField] public LevelState levelState;
    
    [SerializeField] public Animator cameraAnimator;

    [Header("Key Objects")]
    [SerializeField] public List<GameObject> UI;

    [SerializeField] private GameObject gameOverScreen;

    [SerializeField] private PlayerController player;
    private Health playerHealth;
    public GameObject[] defaultEnabledRootObjects;
    public bool IsLevelActive = false;

    public Vector2 PlayerPosition
    {
        get
        {
            return player.transform.position;
        }
    }
    public Health PlayerHealth
    {
        get
        {
            return playerHealth;
        }
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

    public void NotifyLevel(LevelCommand cmd)
    {
        switch (cmd)
        {
            case LevelCommand.Load:
                onLoad?.Invoke();
                break;
            case LevelCommand.Activate:
                onActivate?.Invoke();
                break;
            case LevelCommand.Deactivate:
                onDeactivate?.Invoke();
                break;
            case LevelCommand.Unload:
                onUnload?.Invoke();
                break;
        }
    }

    void Awake()
    {
        gameObject.MyLevelManager().onLoad += Load;
        gameObject.MyLevelManager().onActivate += Activate;
        gameObject.MyLevelManager().onDeactivate += Deactivate;
    }

    void Start()
    {
        GameManager.Init();
    }

    void Activate()
    {
        TimeSystem.onTimeout += GameOver;
        levelState = LevelState.Playing;
        DevLog.Info("Activate called");
    }

    void Deactivate()
    {
        TimeSystem.onTimeout -= GameOver;
    }

    void Load()
    {
        DevLog.Info("Load called");
        cameraAnimator.Play("Game");
        playerHealth = player.GetComponent<Health>();
    }

    public void GameOver()
    {
        if (!gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
            PlayerController.Controls.Disable();
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
        TimeSystem.Reset();
    }

    public void RespawnCheckpoint()
    {
        // Teleports player to most recent checkpoint from checkPointManager, also resets health/healthbar
        PlayerController.Controls.Enable();
        gameOverScreen.SetActive(false);
        levelState = LevelState.Playing;

        CheckpointSystem.ReturnToCurrent(player);
        playerHealth.ResetHealth();
        TimeSystem.SetTimer(TimeSystem.levelTimeLimitSeconds);
    }
}

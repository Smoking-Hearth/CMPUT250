using UnityEngine;
using UnityEngine.UI;

public enum LevelState
{
    Paused, Playing, Defeat, Win, Cutscene
}
public class GameManager : MonoBehaviour
{
    public static LevelState levelState;
    [SerializeField] private FireSettings setFireSettings;
    private static FireSettings fireSettings;
    public static FireSettings FireSettings
    {
        get
        {
            if (fireSettings == null)
            {
                fireSettings = FireSettings.GetOrCreate();
            }
            return fireSettings;
        }
    }
    private static LevelTimeManager levelTimeManager;
    public static LevelTimeManager LevelTimeManager
    {
        get
        {
            return levelTimeManager;
        }
    }

    private static InteractableManager interactableManager;
    public static InteractableManager InteractableManager
    {
        get
        {
            return interactableManager;
        }
    }

    [SerializeField] private float levelTimeLimitSeconds;
    [SerializeField] private Slider timeLimitBar;
    [SerializeField] private PlayerController setPlayer;
    [SerializeField] private GameObject gameOverScreen;
    private static PlayerController player;
    private static Health playerHealth;

    public delegate void OnFireTick();
    public static event OnFireTick onFireTick;
    private float fireTickTimer;

    public delegate void OnEnemyAttack(Vector2 attackCenter, Vector2 sourcePosition, EnemyAttackInfo attackInfo);
    public static OnEnemyAttack onEnemyAttack;

    public static Vector2 PlayerPosition
    {
        get
        {
            return player.transform.position;
        }
    }
    public static Health PlayerHealth
    {
        get
        {
            return playerHealth;
        }
    }

    [SerializeField] private Animator setCameraAnimator;
    public static Animator cameraAnimator;

    [SerializeField] private DialogSystem setDialogSystem;
    public static DialogSystem dialogSystem;

    [SerializeField] private CheckpointManager setCheckpointManager;
    public static CheckpointManager checkpointManager;

    void Awake()
    {
        checkpointManager = setCheckpointManager;
        fireSettings = setFireSettings;
        player = setPlayer;
        cameraAnimator = setCameraAnimator;
        playerHealth = player.GetComponent<Health>();
        dialogSystem = setDialogSystem;
        interactableManager = new InteractableManager();
        levelTimeManager = new LevelTimeManager(levelTimeLimitSeconds, timeLimitBar);
        SetTimer(levelTimeLimitSeconds);
    }

    private void OnEnable()
    {
        levelTimeManager.onTimeout += GameOver;
        levelState = LevelState.Playing;
    }

    private void OnDisable()
    {
        levelTimeManager.onTimeout -= GameOver;
    }

    void Update()
    {
        interactableManager.CheckNearestTarget(player.transform.position);

        if (checkpointManager != null)
        {
            checkpointManager.UpdateCheckpoint(player.transform.position);
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
                    fireTickTimer = FireSettings.FireDelay;
                    if (onFireTick != null)
                    {
                        onFireTick();
                    }
                }

                if (LevelTimeManager.activated)
                {
                    LevelTimeManager.DepleteTime(Time.fixedDeltaTime);
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
        LevelTimeManager.Reset();
    }

    public void RespawnCheckpoint()
    {
        // Teleports player to most recent checkpoint from checkPointManager, also resets health/healthbar
        PlayerController.Controls.Enable();
        gameOverScreen.SetActive(false);
        levelState = LevelState.Playing;
        checkpointManager.ReturnToCurrent(player);
        playerHealth.ResetHealth();
        SetTimer(levelTimeLimitSeconds);
    }

    public void GameOver()
    {
        if (!gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
            PlayerController.Controls.Disable();
        }
    }

    public void SetTimer(float limit)
    {
        LevelTimeManager.Reset(limit);
    }

    public void StartTimer()
    {
        levelTimeManager.SetVisibility(true);
        levelTimeManager.activated = true;
    }

    public void StopTimer()
    {
        levelTimeManager.SetVisibility(false);
        LevelTimeManager.activated = false;
    }
}

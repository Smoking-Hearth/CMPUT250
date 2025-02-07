using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
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
            if (levelTimeManager == null)
            {

            }

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
    }

    private void OnEnable()
    {
        levelTimeManager.onTimeout += GameOver;
    }

    private void OnDisable()
    {
        levelTimeManager.onTimeout -= GameOver;
    }

    void Update()
    {
        interactableManager.CheckNearestTarget(player.transform.position);
    }

    private void FixedUpdate()
    {
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

        LevelTimeManager.DepleteTime(Time.fixedDeltaTime);
    }

    public void Restartlevel()
    {
        gameOverScreen.SetActive(false);
        levelTimeManager.Reset();
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
    }
}

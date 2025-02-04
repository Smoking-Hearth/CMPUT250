using UnityEngine;
public class GameManager : MonoBehaviour
{
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
    private static InteractableManager interactableManager;
    public static InteractableManager InteractableManager
    {
        get
        {
            return interactableManager;
        }
    }

    [SerializeField] private GameObject[] sceneInteractables;
    [SerializeField] private PlayerController setPlayer;
    private static PlayerController player;
    private static Health playerHealth;

    public delegate void OnFireTick();
    public static event OnFireTick onFireTick;
    private float fireTickTimer;

    public delegate void OnEnemyAttack(Vector2 position, EnemyAttackInfo attackInfo);
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

    void Awake()
    {
        player = setPlayer;
        cameraAnimator = setCameraAnimator;
        playerHealth = player.GetComponent<Health>();
        dialogSystem = setDialogSystem;

        IInteractable[] interactables = new IInteractable[sceneInteractables.Length];

        for (int i = 0; i < sceneInteractables.Length; i++)
        {
            interactables[i] = sceneInteractables[i].GetComponent<IInteractable>();
        }
        interactableManager = new InteractableManager(interactables);
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
    }
}

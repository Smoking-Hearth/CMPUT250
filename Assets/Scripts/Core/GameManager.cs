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
    public static Vector2 PlayerPosition
    {
        get
        {
            return player.transform.position;
        }
    }

    public delegate void OnFireTick();
    public static event OnFireTick onFireTick;
    private float fireTickTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = setPlayer;
        IInteractable[] interactables = new IInteractable[sceneInteractables.Length];

        for (int i = 0; i < sceneInteractables.Length; i++)
        {
            interactables[i] = sceneInteractables[i].GetComponent<IInteractable>();
        }
        interactableManager = new InteractableManager(interactables);
    }

    // Update is called once per frame
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

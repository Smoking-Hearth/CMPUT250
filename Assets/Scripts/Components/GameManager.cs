using UnityEngine;

public class GameManager : MonoBehaviour
{
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
}

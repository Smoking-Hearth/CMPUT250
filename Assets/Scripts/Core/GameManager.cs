using UnityEngine;

public enum LevelState
{
    Paused, Playing, Defeat, Win, Cutscene
}

public class GameManager : MonoBehaviour
{
    private static bool isInit = false;
    public static bool IsInit 
    {
        get { return isInit; }
    }

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get 
        { 
            return instance; 
        }
    }

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

    private SceneSystem sceneSystem;
    public static SceneSystem SceneSystem
    {
        get { return Instance?.sceneSystem; }
    }

    public delegate void OnEnemyAttack(Vector2 attackCenter, Vector2 sourcePosition, EnemyAttackInfo attackInfo);
    public static OnEnemyAttack onEnemyAttack;

    public static void Init()
    {
        if (isInit) return;
        isInit = true;

        if (instance == null)
        {
            instance = new GameObject("GameManager").AddComponent<GameManager>();
        }
        if (instance.sceneSystem == null)
        {
            instance.sceneSystem = new SceneSystem();
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            // GameManager was manually added to the scene.
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != instance)
        {
            // Somehow we ended up with a duplicate.
            Destroy(this.gameObject);
            return;
        }
        else
        {
            // Instance was created upon request in the getter.
            DontDestroyOnLoad(gameObject);
        } 
        Init();
    }
}

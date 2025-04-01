using UnityEngine;

public enum LevelState
{
    None, Paused, Playing, Respawning, Defeat, Win, Cutscene, Dialogue
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
            instance ??= new GameObject("GameManager").AddComponent<GameManager>();
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
                fireSettings = SceneSystem.LevelManagers[SceneSystem.Active].fireSettings;
            }
            return fireSettings;
        }
    }

    private SceneSystem sceneSystem;
    public static SceneSystem SceneSystem
    {
        get 
        {
            Instance.sceneSystem ??= new SceneSystem();
            return Instance.sceneSystem; 
        }
    }

    public delegate void OnEnemyAttack(Vector2 attackCenter, Vector2 sourcePosition, EnemyAttackInfo attackInfo);
    public static OnEnemyAttack onEnemyAttack;
    
    public static void Init(FireSettings fs = null)
    {
        if (isInit) return;
        isInit = true;

        instance ??= new GameObject("GameManager").AddComponent<GameManager>();
        instance.sceneSystem ??= new SceneSystem();
        fireSettings ??= fs;
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

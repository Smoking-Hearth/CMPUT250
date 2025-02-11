using UnityEngine;

public enum LevelState
{
    Paused, Playing, Defeat, Win, Cutscene
}
public class GameManager : MonoBehaviour
{
    private bool isInit = false;
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        { 
            if (instance != null) return instance; 
            instance = new GameObject("SceneLoader").AddComponent<GameManager>();
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
        get { return Instance.sceneSystem; }
    }

    public delegate void OnEnemyAttack(Vector2 attackCenter, Vector2 sourcePosition, EnemyAttackInfo attackInfo);
    public static OnEnemyAttack onEnemyAttack;

    void Init()
    {
        sceneSystem = new SceneSystem();
        fireSettings = setFireSettings;
    }

    void Awake()
    {
        if (instance == null)
        {
            // Scene Loader was manually added to the scene.
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

        if (!isInit)
        {
            Init();
            isInit = true;
        }
    }
}

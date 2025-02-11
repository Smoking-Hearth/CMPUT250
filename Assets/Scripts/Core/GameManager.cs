using UnityEngine;

public enum LevelState
{
    Paused, Playing, Defeat, Win, Cutscene
}
public class GameManager : MonoBehaviour
{
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

    private static Unquenchable.SceneManager sceneManager;
    public static Unquenchable.SceneManager SceneSystem
    {
        get { return sceneManager; }
    }

    LevelManager currentLevel;
    public static LevelManager CurrentLevel 
    {
        get { return Instance.currentLevel; }
    }

    public delegate void OnEnemyAttack(Vector2 attackCenter, Vector2 sourcePosition, EnemyAttackInfo attackInfo);
    public static OnEnemyAttack onEnemyAttack;


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
        }
        else
        {
            // Instance was created upon request in the getter.
            DontDestroyOnLoad(gameObject);
        } 

        fireSettings = setFireSettings;
    }
}

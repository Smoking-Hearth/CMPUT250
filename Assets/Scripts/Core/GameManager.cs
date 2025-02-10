using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public static LevelManager CurrentLevel;

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

    void Awake()
    {
        fireSettings = setFireSettings;
        playerHealth = player.GetComponent<Health>();
    }
}

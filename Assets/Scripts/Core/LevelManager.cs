using UnityEngine;
using Unquenchable;

// TODO: Give this info about how it got loaded.
public class LevelManager : MonoBehaviour 
{
    public static GameManager gameManager;
    public DialogSystem dialogSystem;
    [SerializeField] private GameObject gameOverScreen;
    public LevelState levelState;

    [SerializeField] public Animator cameraAnimator;
    [SerializeField] private PlayerController player;
    private Health playerHealth;

    void Awake()
    {
        cameraAnimator.Play("Game");
        playerHealth = player.GetComponent<Health>();
    }

    void OnEnable()
    {
        GameManager.SceneSystem.Data.timeSystem.onTimeout += GameOver;
        levelState = LevelState.Playing;
    }

    void OnDisable()
    {
        GameManager.SceneSystem.Data.timeSystem.onTimeout -= GameOver;
    }

    public void GameOver()
    {
        if (!gameOverScreen.activeSelf)
        {
            gameOverScreen.SetActive(true);
            PlayerController.Controls.Disable();
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
        GameManager.SceneSystem.Data.timeSystem.Reset();
    }

    public void RespawnCheckpoint()
    {
        // Teleports player to most recent checkpoint from checkPointManager, also resets health/healthbar
        PlayerController.Controls.Enable();
        gameOverScreen.SetActive(false);
        levelState = LevelState.Playing;
        SceneInfo data = GameManager.SceneSystem.Data;

        data.checkpointSystem.ReturnToCurrent(player);
        playerHealth.ResetHealth();
        data.timeSystem.SetTimer(data.timeSystem.levelTimeLimitSeconds);
    }
}

using UnityEngine;

public class ForestSystem: MonoBehaviour
{
    LevelManager levelManager;
    void Start()
    {
        levelManager = gameObject.MyLevelManager();
        levelManager.onActivate += Activate;
    }

    void Activate()
    {
        levelManager.cameraAnimator.Play("Game");
        levelManager.levelState = LevelState.Playing;
        levelManager.TimeSystem.onTimeout += levelManager.GameOver;
    }

    void Deactivate()
    {
        levelManager.TimeSystem.onTimeout -= levelManager.GameOver;
    }
}
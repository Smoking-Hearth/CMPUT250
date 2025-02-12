using UnityEngine;
using UnityEngine.SceneManagement;

public class TechDemo : MonoBehaviour 
{
    public void BackToMenu()
    {
        // I've really gotta fix this mess.
        LevelManager.Active.cameraAnimator.Play("MainMenu");
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.MainMenu));
    }
}
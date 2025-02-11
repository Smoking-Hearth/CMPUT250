using UnityEngine;
using UnityEngine.SceneManagement;

public class TechDemo : MonoBehaviour 
{
    public void BackToMenu()
    {
        // I've really gotta fix this mess.
        GameManager.CurrentLevel.cameraAnimator.Play("MainMenu");
        Unquenchable.SceneManager.UnloadHook(SceneManager.GetActiveScene());
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(SceneIndex.MainMenu));
    }
}
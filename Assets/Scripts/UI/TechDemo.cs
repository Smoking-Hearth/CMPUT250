using UnityEngine;
using UnityEngine.SceneManagement;

public class TechDemo : MonoBehaviour 
{
    public void BackToMenu()
    {
        // I've really gotta fix this mess.
        GameManager.CurrentLevel.cameraAnimator.Play("MainMenu");
        Unquenchable.SceneManagerWrapper.UnloadHook(SceneManager.GetActiveScene());
        StartCoroutine(Unquenchable.SceneManagerWrapper.SetSceneActive(SceneIndex.MainMenu));
    }
}
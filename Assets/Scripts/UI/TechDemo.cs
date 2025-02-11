using UnityEngine;
using UnityEngine.SceneManagement;

public class TechDemo : MonoBehaviour 
{
    public void BackToMenu()
    {
        // I've really gotta fix this mess.
        GameManager.cameraAnimator.Play("MainMenu");
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(SceneIndex.MainMenu, false));
    }
}
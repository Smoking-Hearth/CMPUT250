using UnityEngine;

public class TechDemo : MonoBehaviour 
{
    public void BackToMenu()
    {
        // I've really gotta fix this mess.
        gameObject.MyLevelManager().cameraAnimator.Play("MainMenu");
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.MainMenu));
    }
}
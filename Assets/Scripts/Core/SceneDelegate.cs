using UnityEngine;

public class SceneDelegate : MonoBehaviour
{
    public void Start()
    {
        // If we are here then this is the first scene loaded.
        SceneIndex sceneIdx = (SceneIndex)gameObject.scene.buildIndex;

        // switch (sceneIdx)
        // {
            // case SceneIndex.Forest:
                // GameManager.cameraAnimator.Play("Game");
                // break;
            // case SceneIndex.TechDemo:
                // GameManager.cameraAnimator.Play("Game");
                // break;
            // default:
                // break;
        // }
    }
}
using UnityEngine;

public class SceneManagerDelegate : MonoBehaviour
{
    bool isStartScene = false;

    public void Awake() 
    {
        isStartScene = Unquenchable.SceneManager.Init();
    }

    public void Start()
    {
        if (!isStartScene) return;

        // If we are here then this is the first scene loaded.
        SceneIndex sceneIdx = (SceneIndex)gameObject.scene.buildIndex;

        switch (sceneIdx)
        {
            case SceneIndex.Forest:
                GameManager.cameraAnimator.Play("Game");
                break;
            case SceneIndex.TechDemo:
                GameManager.cameraAnimator.Play("Game");
                break;
            default:
                break;
        }
    }
}
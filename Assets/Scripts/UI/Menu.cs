using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene management functionality.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] Button play, settings, credits;

    SceneIndex selectedLevel = SceneIndex.Forest;

    void Awake()
    {
        // NOTE: This doesn't load in the sense you may think. This will start a background
        // task to load the given scene, and it will be immediatly visible in the game world.
        StartCoroutine(Unquenchable.SceneManager.Load(selectedLevel, false));
    }

    void OnEnable()
    {
        play.onClick.AddListener(OnPlayClick);
        settings.onClick.AddListener(OnSettingsClick);
        credits.onClick.AddListener(OnCreditsClick);
    }

    void OnDisable()
    {
        play.onClick.RemoveListener(OnPlayClick);
        settings.onClick.RemoveListener(OnSettingsClick);
        credits.onClick.RemoveListener(OnCreditsClick);
    }

    void OnPlayClick()
    {
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(selectedLevel));
        GameManager.cameraAnimator.Play("Start");
    }

    void OnSettingsClick()
    {
        Debug.Log("TODO :)");
    }

    void OnCreditsClick()
    {
        Debug.Log("TODO :)");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scene management functionality.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] Button play, settings, credits;

    SceneIndex selectedLevel = SceneIndex.TechDemo;
    bool firstLoad = true;

    void Awake()
    {
        // NOTE: This doesn't load in the sense you may think. This will start a background
        // task to load the given scene, and it will be immediatly visible in the game world.
        StartCoroutine(GameManager.SceneSystem.Load(selectedLevel, false));
    }

    void OnEnable()
    {
        // FIXME: impl proper transition functions to get rid of this trash.
        if (GameManager.SceneSystem.IsLoaded(selectedLevel))
        {
            firstLoad = false;
        }
        else 
        {
            SceneManager.UnloadSceneAsync((int)selectedLevel).completed += (_) =>
            {
                Scene scene = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
                GameManager.SceneSystem.SetSceneVisible(scene);
            };
        }

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
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(selectedLevel));
    }

    void OnSettingsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        GameManager.SceneSystem.SetSceneVisible(selected, false);
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Settings));
    }

    void OnCreditsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        GameManager.SceneSystem.SetSceneVisible(selected, false);
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Credits));
    }
}

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
        StartCoroutine(Unquenchable.SceneManager.Load(selectedLevel, false));
        SceneManager.sceneUnloaded += Unquenchable.SceneManager.UnloadHook;
    }

    void OnEnable()
    {
        // FIXME: impl proper transition functions to get rid of this trash.
        if (firstLoad)
        {
            firstLoad = false;
        }
        else 
        {
            SceneManager.UnloadSceneAsync((int)selectedLevel).completed += (_) =>
            {
                StartCoroutine(Unquenchable.SceneManager.Load(selectedLevel, false));
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
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(selectedLevel));
    }

    void OnSettingsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        Unquenchable.SceneManager.SetSceneVisible(selected, false);
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(SceneIndex.Settings));
    }

    void OnCreditsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        Unquenchable.SceneManager.SetSceneVisible(selected, false);
        StartCoroutine(Unquenchable.SceneManager.SetSceneActive(SceneIndex.Credits));
    }
}

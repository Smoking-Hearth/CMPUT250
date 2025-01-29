using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scene management functionality.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] Button play, settings, credits;

    AsyncOperation levelLoad, settingsLoad, creditsLoad; 

    SceneIndex selectedLevel = SceneIndex.Forest;

    static void HideObjectsOnLoad(Scene loaded)
    {
        foreach (var go in loaded.GetRootGameObjects())
        {
            go.SetActive(false);
        }
    }

    void Awake()
    {
        // NOTE: This doesn't load in the sense you may think. This will start a background
        // task to load the given scene, and it will be immediatly visible in the game world.
        levelLoad = SceneManager.LoadSceneAsync((int)selectedLevel, LoadSceneMode.Additive);
        levelLoad.completed += (_) =>
        {
            
        };
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
        // This is ******* weird. But I do it in Rust also.
        static void Callback(SceneIndex selectedLevel)
        {
            Scene loaded = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
            SceneManager.MergeScenes(loaded, SceneManager.GetActiveScene());
        }

        if (levelLoad == null)
        {
            levelLoad = SceneManager.LoadSceneAsync((int)selectedLevel, LoadSceneMode.Additive);
            levelLoad.allowSceneActivation = false;
        }

        if (levelLoad.isDone)
        {
            Callback(selectedLevel);
        }
        else 
        {
            levelLoad.completed += (_) => { Callback(selectedLevel); };
        }
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

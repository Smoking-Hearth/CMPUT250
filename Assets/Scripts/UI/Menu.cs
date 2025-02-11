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

    void Awake()
    {
        // NOTE: This doesn't load in the sense you may think. This will start a background
        // task to load the given scene, and it will be immediatly visible in the game world.
        StartCoroutine(Unquenchable.SceneManagerWrapper.Load(selectedLevel, false));
    }

    void OnEnable()
    {
        // FIXME: impl proper transition functions to get rid of this trash.
        if (Unquenchable.SceneManagerWrapper.IsLoaded(selectedLevel))
        {
            Scene scene = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
            if (scene != null && scene.IsValid())
            {
                Unquenchable.SceneManagerWrapper.SetSceneVisible(scene);
            }
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
        StartCoroutine(Unquenchable.SceneManagerWrapper.SetSceneActive(selectedLevel));
    }

    void OnSettingsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        Unquenchable.SceneManagerWrapper.SetSceneVisible(selected, false);
        StartCoroutine(Unquenchable.SceneManagerWrapper.SetSceneActive(SceneIndex.Settings));
    }

    void OnCreditsClick()
    {
        Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        Unquenchable.SceneManagerWrapper.SetSceneVisible(selected, false);
        StartCoroutine(Unquenchable.SceneManagerWrapper.SetSceneActive(SceneIndex.Credits));
    }
}

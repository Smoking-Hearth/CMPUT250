using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene management functionality.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] Button play, back, settings, credits;
    [SerializeField] CanvasGroup buttonOverlay;

    [SerializeField] private LevelContainer[] containers;
    private int expandedLevel = -1;
    private bool HasSelected
    {
        get { return 0 <= expandedLevel && expandedLevel < containers.Length; }
    }

    void Awake()
    {
        // NOTE: This doesn't load in the sense you may think. This will start a background
        // task to load the given scene, and it will be immediatly visible in the game world.
        buttonOverlay.alpha = 0f;
        buttonOverlay.interactable = false;
        
        for (int i = 0; i < containers.Length; ++i) 
        {
            // WARN: This is needed to capture by value for the lambda.
            int idx = i;
            containers[i].button.onClick.AddListener(() => ClickedContainer(idx));
        }
    }

    void ClickedContainer(int idx)
    {
        if (HasSelected)
        {
            expandedLevel = -1;
        }
        else
        {
            expandedLevel = idx;
        }
    }

    void Update()
    {
        if (buttonOverlay.alpha >= 0.999f)
        {
            buttonOverlay.interactable = true;
        }
        else
        {
            buttonOverlay.interactable = false;
        }
    }

    void OnEnable()
    {
        // FIXME: impl proper transition functions to get rid of this trash.
        // if (GameManager.SceneSystem.IsLoaded(selectedLevel))
        // {
            // firstLoad = false;
        // }
        // else 
        // {
            // SceneManager.UnloadSceneAsync((int)selectedLevel).completed += (_) =>
            // {
                // Scene scene = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
                // GameManager.SceneSystem.SetSceneVisible(scene);
            // };
        // }

        play.onClick.AddListener(OnPlayClick);
        back.onClick.AddListener(OnBackClick);
        settings.onClick.AddListener(OnSettingsClick);
        credits.onClick.AddListener(OnCreditsClick);
    }

    void OnDisable()
    {
        play.onClick.RemoveListener(OnPlayClick);
        back.onClick.RemoveListener(OnBackClick);
        settings.onClick.RemoveListener(OnSettingsClick);
        credits.onClick.RemoveListener(OnCreditsClick);
    }

    void OnPlayClick()
    {
        if (!HasSelected) return;
        SceneIndex selectedLevel = containers[expandedLevel].levelIndex;
        GameManager.SceneSystem.LevelManagers[(int)selectedLevel].LevelCamera.targetTexture = null;
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(selectedLevel));
    }

    void OnBackClick()
    {
        if (!HasSelected) return;
        containers[expandedLevel].BackToMenu();
        expandedLevel = -1;
    }

    void OnSettingsClick()
    {
        // Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        // GameManager.SceneSystem.SetSceneVisible(selected, false);
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Settings, keepInHistory: true));
    }

    void OnCreditsClick()
    {
        // Scene selected = SceneManager.GetSceneByBuildIndex((int)selectedLevel);
        // GameManager.SceneSystem.SetSceneVisible(selected, false);
        StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Credits, keepInHistory: true));
    }
}

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
        buttonOverlay.blocksRaycasts = false;
        
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
            buttonOverlay.blocksRaycasts = true;
        }
        else
        {
            buttonOverlay.interactable = false;
            buttonOverlay.blocksRaycasts = false;
        }
    }

    void OnEnable()
    {
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

    public void ResetLevel()
    {
        SceneIndex sceneIdx = containers[expandedLevel].levelIndex;
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.Unload((int)sceneIdx));
    }

    void OnPlayClick()
    {
        if (!HasSelected) return;

        SceneIndex selectedLevel = containers[expandedLevel].levelIndex;
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(selectedLevel));
    }

    void OnBackClick()
    {
        if (!HasSelected) return;
        containers[expandedLevel].BackToMenu();
        expandedLevel = -1;
    }

    void OnSettingsClick()
    {
        SettingsScreen settingsScreen = gameObject.MyLevelManager().settingsScreen;
        if (settingsScreen != null)
        {
            settingsScreen.gameObject.SetActive(true);
        }
    }

    void OnCreditsClick()
    {
        GameManager.Instance.StartCoroutine(GameManager.SceneSystem.SetSceneActive(SceneIndex.Credits, keepInHistory: true));
    }
}

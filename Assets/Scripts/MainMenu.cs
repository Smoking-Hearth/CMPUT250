using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    enum SceneName 
    {
        None = 0,
        Settings = 1 << 0,
        Forest = 1 << 1,
    }

    [SerializeField] Button play, settings;

    SceneName transitionTo = SceneName.None;
    AsyncOperation loadStatus = null;

    void Start()
    {
        play.onClick.AddListener(OnPlayClick);
        settings.onClick.AddListener(OnSettingsClick);
        loadStatus = SceneManager.LoadSceneAsync("Forest"); 
    }

    // Update is called once per frame
    void OnPlayClick()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Forest"));
    }

    void OnSettingsClick()
    {
    }

    void SceneLoaded()
    {

    }
}

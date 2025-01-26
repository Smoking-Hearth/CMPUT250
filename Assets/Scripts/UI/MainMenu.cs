using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Flags]
    public enum SceneMask
    {
        None = 0,
        Settings = 1 << 0,
        Forest = 1 << 1,
        Suburbs = 1 << 2,
        City = 1 << 3,
    }

    SceneMask loaded = SceneMask.None;
    public SceneMask Loaded 
    {
        get { return loaded; }
    }

    public enum TransitionType
    {
        // It seems like our concept in the opening requires this.
        // Since the camera is in the sky with just the top of the 
        // watchtower in view, and then it tracks down to show the 
        // player standing at the bottom after play is hit.
        Continuous,
        Fade,
        Jump
    }
    
    static SceneLoader instance;
    public static SceneLoader Instance
    {
        get 
        { 
            if (instance != null) return instance; 
            instance = new GameObject("SceneLoader").AddComponent<SceneLoader>();
            return instance;
        }
    }

    public void Transition(SceneMask scenes, TransitionType type)
    {
        if (scenes == SceneMask.None) return;
        if (scenes == SceneMask.Forest) 
        {
            SceneManager.LoadSceneAsync("Forest");
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            // Scene Loader was manually added to the scene.
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != Instance)
        {
            // Somehow we ended up with a duplicate.
            Destroy(this.gameObject);
        }
        else
        {
            // Instance was created upon request in the getter.
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    void SceneLoaded(Scene loaded, LoadSceneMode mode)
    {
        SceneManager.MergeScenes(loaded, SceneManager.GetActiveScene());
    }
}

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button play, settings;

    void OnEnable()
    {
        play.onClick.AddListener(OnPlayClick);
        settings.onClick.AddListener(OnSettingsClick);
    }

    void OnDisable()
    {
        play.onClick.RemoveListener(OnPlayClick);
        settings.onClick.RemoveListener(OnSettingsClick);
    }

    void OnPlayClick()
    {
        SceneLoader.Instance.Transition(SceneLoader.SceneMask.Forest, SceneLoader.TransitionType.Jump);
    }

    void OnSettingsClick()
    {
        SceneLoader.Instance.Transition(SceneLoader.SceneMask.Settings, SceneLoader.TransitionType.Jump);
    }
}

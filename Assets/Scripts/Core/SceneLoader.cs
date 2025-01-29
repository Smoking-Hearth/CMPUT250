using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum SceneIndex
{
    MainMenu = 0,
    Forest = 1,
    City = 2,
    Skyscraper = 3,
    Settings = 4,
    Credits = 5,
}
/// <summary>
/// A convenience wrapper for SceneManager to avoid needing to refer to scenes 
/// with strings. It also has some functionality for dealing with more complex
/// scene management: load a scene from multiple scene files.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    // NOTE: This must be kept in sync with the SceneList
    [Flags]
    public enum SceneMask
    {
        None = 0,
        MainMenu = 1 << 0,
        Forest = 1 << 1,
        City = 1 << 2,
        Skyscraper = 1 << 3,
        Settings = 1 << 4,
        Credits = 1 << 5,
    }


    public static int BuildIndex(SceneMask scene)
    {
        switch (scene)
        {
            case SceneMask.MainMenu: return 0;
            case SceneMask.Forest: return 1;
            case SceneMask.City: return 2;
            case SceneMask.Skyscraper: return 3;
            case SceneMask.Settings: return 4;
            case SceneMask.Credits: return 5;
            default: return -1;
        }
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

    // PERF: This is fast as long as all the scenes are loaded.
    /// <summary>
    /// Will asyncronously load a collection of scenes into one scene which can
    /// be specified by its buildIndex. If no targetBuildIndex is given this will
    /// load into the requested scene with the lowest build index.
    /// </summary>
    /// <returns>Number of scenes queued for loading</returns>
    public int Load(SceneMask scenes, int targetBuildIndex = -1)
    {
        if (scenes == SceneMask.None) return 0;

        if (targetBuildIndex >= 0)
        {
            // There is a scene we are supposed to be loading the others into.
            if (!SceneManager.GetSceneByBuildIndex(targetBuildIndex).IsValid())
            {
                // Which is not loaded yet. We should start loading queue it for loading.
                AsyncOperation load = SceneManager.LoadSceneAsync(targetBuildIndex, LoadSceneMode.Additive);
                load.allowSceneActivation = false;
                load.priority += 1;
                load.completed += (_) =>
                {
                    SceneLoaded(targetBuildIndex);
                };
            }
        }

        // We want to only retain the scenes that have not been loaded
        int notLoaded = ~(int)this.loaded;
        int toLoad = (int)scenes & notLoaded;
        int idx = 0;

        // While there is still something left to load.
        while (toLoad > 0)
        {
            if ((toLoad & 1) != 0)
            {
                if (targetBuildIndex < 0)
                {
                    targetBuildIndex = idx;
                }

                AsyncOperation load = SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);
                load.allowSceneActivation = false;
                if (idx == targetBuildIndex)
                {
                    load.priority += 1;
                    load.completed += (_) =>
                    {
                        SceneLoaded(idx);
                    };
                }
                else
                {
                    load.completed += (_) => 
                    {
                        SceneLoaded(idx, targetBuildIndex);
                    };
                }
            }
            idx += 1;
            toLoad >>= 1;
        }
        return idx;
    }
    
    void Awake()
    {
        if (instance == null)
        {
            // Scene Loader was manually added to the scene.
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != instance)
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

    void SceneLoaded(int loadedBuildIndex, int mergeIntoBuildIndex = -1)
    {
        if (mergeIntoBuildIndex == loadedBuildIndex)
        {
            // Even though we bumped the priority on this task we may have loaded after 
            // another scene. See if anyone loaded before us and merge them in.
        }
        else if (mergeIntoBuildIndex >= 0)
        {
            Scene loadedScene = SceneManager.GetSceneByBuildIndex(loadedBuildIndex);
            SceneManager.MergeScenes(loadedScene, SceneManager.GetActiveScene());
        }
    }
}
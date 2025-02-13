using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public enum SceneIndex
{
    MainMenu = 0,
    Forest = 1,
    City = 2,
    Skyscraper = 3,
    Settings = 4,
    TechDemo = 5,
    Credits = 6,
}

/// <summary>
/// This is a wrapper for `SceneManager` to add some structure to the idea of a `Scene`
/// and make it easier to do things like have two scenes loaded at once but only one "Active"
/// </summary>
public class SceneSystem
{
    // These are bitflags. The nth bit belongs to the nth scene.
    // e.g. First bit is for MainMenu since (1 << 0).
    private int loading = 0;
    private int loaded = 0;
    private int visible = 0;
    private int active = 0;
    
    private LevelManager[] levelManagers = new LevelManager[7];

    public LevelManager[] LevelManagers
    {
        get { return levelManagers; }
    }

    /// <summary>
    /// This gives you access to data for the currently active scene.
    /// </summary>
    public LevelManager ActiveLevel
    {
        get { return levelManagers[active]; }
    }

    public void RegisterLoad(Scene scene)
    {
        RegisterLoad(scene.buildIndex);
    }

    public void RegisterLoad(SceneIndex sceneIndex)
    {
        RegisterLoad((int)sceneIndex);
    }

    public void RegisterLoad(int buildIndex)
    {
        loaded |= 1 << buildIndex;
    }

    public void RegisterLevelManager(SceneIndex sceneIndex)
    {
        RegisterLevelManager((int)sceneIndex);
    }

    public void RegisterLevelManager(int buildIndex)
    {
        RegisterLevelManager(SceneManager.GetSceneByBuildIndex(buildIndex));
    }

    public void RegisterLevelManager(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            LevelManager lm = go.GetComponent<LevelManager>();
            if (lm != null)
            {
                levelManagers[scene.buildIndex] = lm;
                break;
            }
        }
    }

    /// <returns>If we did the initialization</returns>
    public SceneSystem()
    {
        Scene first = SceneManager.GetActiveScene();
        active = first.buildIndex;
        RegisterLoad(first);
        RegisterLevelManager(first);
        levelManagers[active].NotifyLevel(LevelCommand.Load);
        levelManagers[active].NotifyLevel(LevelCommand.Activate);
    }

    // This is absolutely terrible. Becuase it overwrites active on objects
    // that we may want to be inactive when the scene is loaded.
    public void SetSceneVisible(Scene scene, bool visible = true)
    {
        int idx = scene.buildIndex;

        if (!IsLoaded(idx))
        {
            DevLog.Error($"Tried to set (unloaded) scene {(SceneIndex)idx} visible");
            return;
        }

        foreach (var go in levelManagers[scene.buildIndex].UI)
        {
            go.SetActive(visible);
        }
    }

    public void LoadHook(Scene scene, LoadSceneMode _)
    {
        int idx = 1 << (int)scene.buildIndex;
        loaded |= idx;
    }

    public void UnloadHook(Scene scene)
    {
        loaded &= ~(1 << (int)scene.buildIndex);
    }
    
    public bool IsLoaded(SceneIndex sceneIdx)
    {
        return IsLoaded((int)sceneIdx);
    }

    public bool IsLoaded(int buildIndex)
    {
        return (loaded & (1 << buildIndex)) != 0;
    }

    public IEnumerator Load(SceneIndex sceneIdx, bool hideLoaded = true)
    {
        // Don't reload loaded scenes
        if (IsLoaded(sceneIdx)) yield break;
        int idx = (int)sceneIdx;
        AsyncOperation op = SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

        while (!op.isDone) yield return null;

        int mask = 1 << idx;
        loaded |= mask;

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(idx);
        RegisterLevelManager(loadedScene);

        // An active level manager means time is passing. We don't want that.
        LevelManager loadedManager = levelManagers[idx];
        loadedManager.NotifyLevel(LevelCommand.Load);
        loadedManager.gameObject.SetActive(false);

        if (hideLoaded)
        {
            SetSceneVisible(loadedScene, false);
        }
    }

    public IEnumerator SetSceneActive(SceneIndex sceneIdx, bool hideCurrent = true)
    {
        Scene prev = SceneManager.GetActiveScene();
        Scene next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);

        if (IsLoaded(sceneIdx))
        {
            SetSceneVisible(next, true);

        }
        else 
        {
            // We don't hide the objects on this load
            yield return Load(sceneIdx, false);

            // FIXME: This may not be necessary, SceneManager could be updating
            // the object previously returned.
            next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);
        }

        LevelManager prevManager = levelManagers[prev.buildIndex];
        LevelManager nextManager = levelManagers[next.buildIndex];

        prevManager.NotifyLevel(LevelCommand.Deactivate);
        prevManager.gameObject.SetActive(false);

        nextManager.gameObject.SetActive(true);
        nextManager.NotifyLevel(LevelCommand.Activate);

        // Hide the current scene
        if (hideCurrent)
        {
            SetSceneVisible(SceneManager.GetActiveScene(), false);
        }

        SceneManager.SetActiveScene(next);
    }
}
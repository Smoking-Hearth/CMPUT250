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

    public SceneSystem()
    {
        Scene first = SceneManager.GetActiveScene();
        active = first.buildIndex;
        RegisterScene(first, true);
        levelManagers[active].NotifyLevel(LevelCommand.Load);
        levelManagers[active].NotifyLevel(LevelCommand.Activate);
    }

    public void SceneObjectsToContainer(Scene scene, int container)
    {
        if (container == 0) return;
        float offset = (float)((container - 1) * 1000);
        foreach (var go in scene.GetRootGameObjects())
        {
            Vector3 position = go.transform.position;
            position.x += offset;
            go.transform.position = position;
        }
        Camera camera = levelManagers[scene.buildIndex].LevelCamera;
        Vector3 cameraPos = camera.transform.position;
        cameraPos.x = offset;
        camera.transform.position = cameraPos;
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

    public void RegisterScene(Scene scene, bool isFirst = false)
    {
        if (IsLoaded(scene.buildIndex)) return;
        int idx = scene.buildIndex;

        RegisterLoad(scene);
        RegisterLevelManager(scene);
        SceneObjectsToContainer(scene, idx);

        // An active level manager means time is passing. We don't want that.
        LevelManager loadedManager = levelManagers[idx];
        loadedManager.NotifyLevel(LevelCommand.Load);

        if (isFirst)
        {
            loadedManager.gameObject.SetActive(true);
            loadedManager.Activate();
        }
        else
        {
            loadedManager.Deactivate();
            loadedManager.gameObject.SetActive(false);
        }
    }

    public IEnumerator Load(SceneIndex sceneIdx)
    {
        // Don't reload loaded scenes
        if (IsLoaded(sceneIdx)) yield break;
        int idx = (int)sceneIdx;
        AsyncOperation op = SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

        while (!op.isDone) yield return null;
        DevLog.Info($"Load for {sceneIdx} finished");
        RegisterScene(SceneManager.GetSceneByBuildIndex(idx));
    }

    public IEnumerator SetSceneActive(SceneIndex sceneIdx)
    {
        Scene prev = SceneManager.GetActiveScene();
        Scene next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);

        if (!IsLoaded(sceneIdx))
        {
            // We don't hide the objects on this load
            yield return Load(sceneIdx);

            // FIXME: This may not be necessary, SceneManager could be updating
            // the object previously returned.
            next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);
        }

        LevelManager prevManager = levelManagers[prev.buildIndex];
        LevelManager nextManager = levelManagers[next.buildIndex];

        prevManager.NotifyLevel(LevelCommand.Deactivate);
        prevManager.Deactivate();

        nextManager.Activate();
        nextManager.NotifyLevel(LevelCommand.Activate);

        SceneManager.SetActiveScene(next);
    }
}
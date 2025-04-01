using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public enum SceneIndex
{
    MainMenu = 0,
    Forest = 1,
    City = 2,
    Skyscraper = 3,
    Credits = 4,
    LDProto = 5,
}

/// <summary>
/// This is a wrapper for `SceneManager` to add some structure to the idea of a `Scene`
/// and make it easier to do things like have two scenes loaded at once but only one "Active"
/// </summary>
public class SceneSystem
{
    // These are bitflags. The nth bit belongs to the nth scene.
    // e.g. First bit is for MainMenu since (1 << 0).
    private int queued = 0;
    private int loaded = 0;



    public int Active
    {
        get => SceneManager.GetActiveScene().buildIndex;
    }
    
    private LevelManager[] levelManagers = new LevelManager[6];

    public LevelManager[] LevelManagers
    {
        get { return levelManagers; }
    }

    public Stack<int> history;

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

    public void RegisterUnload(int buildIndex)
    {
        DevLog.Info($"Before Unload: {loaded}");
        loaded &= ~(1 << buildIndex);
        DevLog.Info($"Tried unload for {(SceneIndex)buildIndex}, After: {loaded}");
    }

    public void RegisterQueue(int buildIndex)
    {
        queued |= 1 << buildIndex;
    }

    public void RegisterDequeue(int buildIndex)
    {
        queued &= ~(1 << buildIndex);
    }

    public bool IsQueued(int buildIndex)
    {
        return (queued & (1 << buildIndex)) != 0;
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
            if (go.TryGetComponent<LevelManager>(out var lm))
            {
                levelManagers[scene.buildIndex] = lm;
                DevLog.Info($"Set LevelManager for {scene.name}");
                break;
            }
        }
    }

    public SceneSystem()
    {
        history = new();
        Scene first = SceneManager.GetActiveScene();
        RegisterScene(first, true);
        levelManagers[first.buildIndex].NotifyLevel(LevelCommand.Load);
        levelManagers[first.buildIndex].NotifyLevel(LevelCommand.Activate);
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

    public bool IsLoaded(SceneIndex sceneIdx)
    {
        return IsLoaded((int)sceneIdx);
    }

    public bool IsLoaded(int buildIndex)
    {
        return (loaded & (1 << buildIndex)) != 0;
    }

    public async Awaitable Unload(int buildIdx)
    {
        // WARN: We don't want to unload active scenes
        if (!IsLoaded(buildIdx) || buildIdx == Active) return;

        RegisterUnload(buildIdx);

        await SceneManager.UnloadSceneAsync(buildIdx);

        // Note that by this point we don't actually know if there is other code
        // running that has reloaded level. Don't overwrite LevelManager.
        if (!(IsLoaded(buildIdx) || IsQueued(buildIdx)))
        {
            levelManagers[buildIdx] = null;
        }

    }

    public void RegisterScene(Scene scene, bool isFirst = false)
    {
        if (IsLoaded(scene.buildIndex)) return;

        int idx = scene.buildIndex;

        RegisterDequeue(idx);
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

    // If the load has not yet been started this will start it. If we are loading the Awaitable
    // will resolve at latest by the frame after registration. If the scene is loaded this is a 
    // no-op.
    public async Awaitable Preload(SceneIndex sceneIdx)
    {
        int buildIdx = (int)sceneIdx;
        
        // Don't reload loaded scenes
        if (IsLoaded(buildIdx)) return;

        if (IsQueued(buildIdx))
        {
            while (IsQueued(buildIdx))
            {
                await Awaitable.NextFrameAsync();
            }
            return;
        }

        RegisterQueue(buildIdx);
        await SceneManager.LoadSceneAsync(buildIdx, LoadSceneMode.Additive);

        DevLog.Info($"Load for {sceneIdx} finished");
        RegisterScene(SceneManager.GetSceneByBuildIndex(buildIdx));
    }

    public async Awaitable GoBack()
    {
        if (!history.TryPop(out int buildIndex)) return;
        await SetSceneActive((SceneIndex)buildIndex);
    }

    public async Awaitable SetSceneActive(SceneIndex sceneIdx, bool keepInHistory = false)
    {
        Scene prev = SceneManager.GetActiveScene();
        Scene next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);

        // SOME BRAINDAMAGE IS HAPPENING HERE. 
        if (!(next.IsValid() || next.isLoaded))
        {
            RegisterUnload((int)sceneIdx);
            DevLog.Info("Scene not loaded.");
            // We don't hide the objects on this load
            await Preload(sceneIdx);

            // FIXME: This may not be necessary, SceneManager could be updating
            // the object previously returned.
            next = SceneManager.GetSceneByBuildIndex((int)sceneIdx);
        }
        else 
        {
            DevLog.Info("Scene is loaded, not bothering with load");
        }

        LevelManager prevManager = levelManagers[prev.buildIndex];
        DevLog.Info($"Index for next ({next.name}) is {next.buildIndex}");
        DevLog.Info($"BITS: {queued}, {loaded}");
        LevelManager nextManager = levelManagers[next.buildIndex];

        prevManager.NotifyLevel(LevelCommand.Deactivate);
        prevManager.Deactivate();

        if (keepInHistory)
        {
            history.Push(prev.buildIndex);
        }

        nextManager.Activate();
        nextManager.NotifyLevel(LevelCommand.Activate);
        DevLog.Info($"Called Activate for {next.name}");

        SceneManager.SetActiveScene(next);
    }

    // Will transition to a scene and unload all other levels
    public async Awaitable NextLevel(SceneIndex current, SceneIndex next)
    {
        await SetSceneActive(next);
        await Unload((int)current);
    }
}
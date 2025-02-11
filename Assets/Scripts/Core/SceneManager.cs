using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

public struct SceneData
{
    const int k_EventSystemIdx = 0;
    const int k_UIIdx = 1;

    public Canvas UI;
    public EventSystem eventSystem;
    public DialogSystem dialogSystem;
    public CheckpointManager checkpointSystem;
    public InteractableManager interactableSystem;
    public LevelTimeManager timeSystem;
    // FIXME: This may be global state
    public MusicManager musicSystem;
    public Scene scene;

    public List<GameObject> defaultEnabledRootObject;

    // PERF: If the scene doesn't have the things in expected places this is...
    // expensive.
    public SceneData(Scene scene)
    {
        UI = null;
        eventSystem = null;
        dialogSystem = null;
        checkpointSystem = null;
        interactableSystem = null;
        timeSystem = null;
        musicSystem = null;
        this.scene = scene;

        defaultEnabledRootObject = new List<GameObject>();

        GameObject[] rootObjects = scene.GetRootGameObjects();
        if (rootObjects.Length > k_EventSystemIdx)
        {
            eventSystem = rootObjects[k_EventSystemIdx].GetComponent<EventSystem>();
        }
        if (rootObjects.Length > k_UIIdx)
        {
            UI = rootObjects[k_UIIdx].GetComponent<Canvas>();
        }

        foreach (var go in rootObjects)
        {
            if (go.activeSelf)
            {
                defaultEnabledRootObject.Add(go);
            }
            if (eventSystem == null)
            {
                eventSystem = go.GetComponent<EventSystem>();
            }
            if (UI == null)
            {
                UI = go.GetComponent<Canvas>();
            }
        }
    }
}

/// <summary>
/// This is a wrapper for `SceneManager` to add some structure to the idea of a `Scene`
/// and make it easier to do things like have two scenes loaded at once but only one "Active"
/// </summary>
public class SceneSystem
{
    // These are bitflags. The nth bit belongs to the nth scene.
    // e.g. First bit is for MainMenu since (1 << 0).
    private int loaded = 0;
    private SceneData[] data = new SceneData[7];

    public SceneData[] Data
    {
        get { return data; }
    }

    /// <summary>
    /// This gives you access to data for the currently active scene.
    /// </summary>
    public SceneData CurrentData
    {
        get 
        { 
            int current = SceneManager.GetActiveScene().buildIndex;
            return data[current];
        }
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

    public void RegisterData(SceneIndex sceneIndex)
    {
        RegisterData((int)sceneIndex);
    }

    public void RegisterData(int buildIndex)
    {
        RegisterData(SceneManager.GetSceneByBuildIndex(buildIndex));
    }

    public void RegisterData(Scene scene)
    {
        data[scene.buildIndex] = new SceneData(scene);
    }

    /// <returns>If we did the initialization</returns>
    public SceneSystem()
    {
        Scene first = SceneManager.GetActiveScene();
        RegisterLoad(first);
        RegisterData(first);
    }

    // This is absolutely terrible. Becuase it overwrites active on objects
    // that we may want to be inactive when the scene is loaded.
    public void SetSceneVisible(Scene scene, bool visible = true)
    {
        int idx = scene.buildIndex;
        foreach (var ob in data[idx].defaultEnabledRootObject) 
        {
            ob.SetActive(visible);
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
        return (loaded & (1 << (int)sceneIdx)) != 0;
    }

    public IEnumerator Load(SceneIndex sceneIdx, bool hideLoaded = true)
    {
        // Don't reload loaded scenes
        if (IsLoaded(sceneIdx)) yield break;
        int idx = (int)sceneIdx;
        AsyncOperation op = SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

        while (!op.isDone) yield return null;

        Scene loadedScene = SceneManager.GetSceneByBuildIndex(idx);
        data[idx] = new SceneData(loadedScene);

        // Even if we aren't hiding the scene we probably want these inactive
        data[idx].eventSystem.gameObject.SetActive(false);
        data[idx].UI.gameObject.SetActive(false);

        if (hideLoaded)
        {
            SetSceneVisible(loadedScene, false);
        }

        int mask = 1 << idx;
        loaded |= mask;
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

        data[prev.buildIndex].eventSystem.gameObject.SetActive(false);
        data[prev.buildIndex].UI.gameObject.SetActive(false);

        data[next.buildIndex].eventSystem.gameObject.SetActive(true);
        data[next.buildIndex].UI.gameObject.SetActive(true);

        // Hide the current scene
        if (hideCurrent)
        {
            SetSceneVisible(SceneManager.GetActiveScene(), false);
        }

        SceneManager.SetActiveScene(next);
    }
}
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

namespace Unquenchable {
    public struct SceneInfo
    {
        const int k_EventSystemIdx = 0;
        const int k_UIIdx = 1;

        public Canvas UI;
        public EventSystem eventSystem;
        public List<GameObject> defaultEnabledRootObject;

        // PERF: If the scene doesn't have the things in expected places this is...
        // expensive.
        public SceneInfo(Scene scene)
        {
            UI = null;
            eventSystem = null;
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
    /// Querying for load is pretty cheap.
    /// Wraps LoadSceneAsync so that it can be run as a coroutine.
    /// </summary>
    public static class SceneManager
    {
        static bool hasBeenInit = false;

        // These are bitflags. The nth bit belongs to the nth scene.
        // e.g. First bit is for MainMenu since (1 << 0).
        static int loaded = 0;
        static SceneInfo[] sceneInfos = new SceneInfo[7];

        public static SceneInfo[] SceneInfos
        {
            get { return sceneInfos; }
        }

        /// <returns>If we did the initialization</returns>
        public static bool Init()
        {
            if (hasBeenInit) return false;
            hasBeenInit = true;

            Scene first = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            int idx = first.buildIndex;
            sceneInfos[idx] = new SceneInfo(first);
            loaded |= 1 << idx;

            return true;
        }

        // This is absolutely terrible. Becuase it overwrites active on objects
        // that we may want to be inactive when the scene is loaded.
        public static void SetSceneVisible(Scene scene, bool visible = true)
        {
            int idx = scene.buildIndex;
            foreach (var ob in sceneInfos[idx].defaultEnabledRootObject) 
            {
                ob.SetActive(visible);
            }
        }

        public static void LoadHook(Scene scene, LoadSceneMode _)
        {
            int idx = 1 << (int)scene.buildIndex;
            loaded |= idx;
        }

        public static void UnloadHook(Scene scene)
        {
            loaded &= ~(1 << (int)scene.buildIndex);
        }
        
        public static void SceneChangeHook(Scene from, Scene into)
        {
            // TODO? 
        }

        public static bool IsLoaded(SceneIndex sceneIdx)
        {
            return (loaded & (1 << (int)sceneIdx)) != 0;
        }

        public static IEnumerator Load(SceneIndex sceneIdx, bool hideLoaded = true)
        {
            // Don't reload loaded scenes
            if (IsLoaded(sceneIdx)) yield break;
            int idx = (int)sceneIdx;
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

            while (!op.isDone) yield return null;

            Scene loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(idx);
            sceneInfos[idx] = new SceneInfo(loadedScene);

            // Even if we aren't hiding the scene we probably want these inactive
            sceneInfos[idx].eventSystem.gameObject.SetActive(false);
            sceneInfos[idx].UI.gameObject.SetActive(false);

            if (hideLoaded)
            {
                SetSceneVisible(loadedScene, false);
            }

            int mask = 1 << idx;
            loaded |= mask;
        }

        public static IEnumerator SetSceneActive(SceneIndex sceneIdx, bool hideCurrent = true)
        {
            Scene prev = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Scene next = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)sceneIdx);

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
                next = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)sceneIdx);
            }

            sceneInfos[prev.buildIndex].eventSystem.gameObject.SetActive(false);
            sceneInfos[prev.buildIndex].UI.gameObject.SetActive(false);

            sceneInfos[next.buildIndex].eventSystem.gameObject.SetActive(true);
            sceneInfos[next.buildIndex].UI.gameObject.SetActive(true);

            // Hide the current scene
            if (hideCurrent)
            {
                SetSceneVisible(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), false);
            }

            UnityEngine.SceneManagement.SceneManager.SetActiveScene(next);
        }
    }
}
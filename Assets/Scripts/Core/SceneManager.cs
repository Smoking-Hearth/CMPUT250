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
    Credits = 5,
}

namespace Unquenchable {
    /// <summary>
    /// Querying for load is pretty cheap.
    /// Wraps LoadSceneAsync so that it can be run as a coroutine.
    /// </summary>
    public class SceneManager
    {
        // These are bitflags. The nth bit belongs to the nth scene.
        // e.g. First bit is for MainMenu since (1 << 0).
        static int loaded = 0;

        // This is absolutely terrible. Becuase it overwrites active on objects
        // that we may want to be inactive when the scene is loaded.
        public static void SetSceneObjectsActive(Scene scene, bool active)
        {
            foreach (var ob in scene.GetRootGameObjects()) ob.SetActive(active);
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
            return (loaded & (int)sceneIdx) != 0;
        }

        public static IEnumerator Load(SceneIndex sceneIdx, bool hideLoaded = true)
        {
            // Don't reload loaded scenes
            if (IsLoaded(sceneIdx)) yield break;
            int idx = (int)sceneIdx;
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(idx, LoadSceneMode.Additive);

            // NOTE: This may work right after the while loop, but that requires
            // Unity to be polling the coroutine pretty consistently otherwise we'll
            // see the object flash on-screen.
            if (hideLoaded)
            {
                op.completed += (_) => 
                {
                    Scene loaded = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(idx);
                    SetSceneObjectsActive(loaded, false);
                };
            }

            while (!op.isDone) yield return null;
        }

        public static IEnumerator SetSceneActive(SceneIndex sceneIdx, bool hideCurrent = true)
        {
            // Make sure the scene is loaded
            int mask = 1 << (int)sceneIdx;

            Scene next = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)sceneIdx);

            if (IsLoaded(sceneIdx))
            {
                SetSceneObjectsActive(next, true);
            }
            else 
            {
                // We don't hide the objects on this load
                yield return Load(sceneIdx, false);

                // FIXME: This may not be necessary, SceneManager could be updating
                // the object previously returned.
                next = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex((int)sceneIdx);
            }

            // Hide the current scene
            if (hideCurrent)
            {
                SetSceneObjectsActive(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), false);
            }
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(next);
        }
    }

}
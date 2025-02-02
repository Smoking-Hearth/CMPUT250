using UnityEngine;

public class SceneManagerDelegate : MonoBehaviour
{
    public void Awake() 
    {
        // This is basically useless. May be better to just initialize the first
        // time one of the methods is called.
        Unquenchable.SceneManager.Init();
    }
}
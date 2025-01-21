using UnityEngine;

/// <summary>
/// This will destroy whatever it's attached to after a specified amount of time. 
/// A callback can be added to listen for this destruction.
/// </summary>
public class Timer : MonoBehaviour
{
    public float timeLeft_s = float.NegativeInfinity;

    public delegate void OnFinish(GameObject gameObject);
    public event OnFinish OnFinishCallback = null;

    void Update()
    {
        if (OnFinishCallback == null) return;
        timeLeft_s -= Time.deltaTime;
        if (timeLeft_s <= 0.0f)
        {
            OnFinishCallback(this.gameObject);
            // WARN: Destroy may not happen immediately. Prevent the callback from
            // happening again.
            OnFinishCallback = null;
        }
    }
}
using UnityEngine;

public enum WalkableType
{
    Flat, Platform, Slope, SlopePlatform
}

public class Ground : MonoBehaviour
{
    [SerializeField] private WalkableType walkableType;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip landClip;
    public WalkableType WalkableType { get { return walkableType; } }
    public AudioClip FootstepClip { get { return footstepClip; } }
    public AudioClip LandClip { get { return landClip != null ? landClip : footstepClip; } }

    [SerializeField] private Effector2D effector;

    private void Start()
    {
        if (footstepClip == null)
        {
            Debug.LogError(gameObject.name + " is missing footstep sounds");
        }
    }

    public void IgnoreLayer(int layer)
    {
        if (effector != null)
        {
            effector.colliderMask &= ~(1 << layer);
        }
    }
    public void IgnoreLayer(LayerMask layers)
    {
        if (effector != null)
        {
            effector.colliderMask &= ~(layers);
        }
    }
    public void AddLayer(int layer)
    {
        if (effector != null)
        {
            effector.colliderMask |= (1 << layer);
        }
    }
    public void AddLayer(LayerMask layers)
    {
        if (effector != null)
        {
            effector.colliderMask |= (layers);
        }
    }
}

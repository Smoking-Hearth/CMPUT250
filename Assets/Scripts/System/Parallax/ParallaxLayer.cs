using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [HideInInspector] public MeshRenderer meshRenderer = null;
    [field: SerializeField] public Texture Texture { get; private set;}
    [field: SerializeField] public bool IsTiled {get; private set; }

    private Vector2 initPosition;
    public Vector2 InitPosition 
    { 
        get { return initPosition; }
    }

    // This can be thought of as the same thing as the speed
    private float depth = 1f;

    void Awake()
    {
        initPosition = transform.localPosition;
    }
}
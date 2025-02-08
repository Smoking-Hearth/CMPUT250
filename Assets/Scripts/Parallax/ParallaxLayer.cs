using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [field: SerializeField] public MeshRenderer MeshRenderer { get; private set;}
    [field: SerializeField] public Texture Texture { get; private set;}
}

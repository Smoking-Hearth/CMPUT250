using System.Collections.Generic;
using UnityEngine;

// PERF: Faster to have ParallaxBackground be the quad and write a small shader to
// do the offset since we can get camera info easily.
// That approach would also allow snapping the verts of the quad to screen corners
// by setting the appropriate vertices to (0, 0), (1, 0), etc in teh vertex shader.
// But it annoyingly need to be done in ShaderLab since shadergraph wants object space
// ouputs and I want to set NDC coords (since working backwards expensive and unnecessary)
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private List<ParallaxLayer> layers;

    void Awake()
    {
        foreach (var layer in layers)
        {
            layer.MeshRenderer.material.SetTexture("_MainTex", layer.Texture);
            Debug.Log(layer.Texture);
        }
    }

    void Update()
    {
        Vector2 cameraTransform = Camera.main.transform.transform.position;
        // FIXME
        cameraTransform.y = 0;
        foreach (var layer in layers)
        {
            // This is just a pseudo perspective divide but weird.
            layer.MeshRenderer.material.SetTextureOffset("_MainTex", cameraTransform / layer.transform.position.z);
        }
    }
}
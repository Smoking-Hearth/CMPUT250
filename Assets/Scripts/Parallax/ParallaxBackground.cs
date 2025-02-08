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

    [Tooltip("This is the origin as far as parallax is concerned")]
    [SerializeField] private GameObject relativeTo;

    // These are attached to the active camera
    [SerializeField] private MeshRenderer tiledLayerPrefab;
    private GameObject tiledLayerAnchor;

    void Awake()
    {
        Vector2 minTiledSize = Vector2.positiveInfinity;

        foreach (var layer in layers)
        {
            if (layer.IsTiled)
            {
                if (tiledLayerAnchor == null)                
                {
                    Vector3 pos = Vector3.zero;
                    pos.z = transform.position.z;
                    tiledLayerAnchor = new GameObject("ParallaxTiledLayerAnchor");
                    tiledLayerAnchor.transform.SetParent(Camera.main.transform);
                    tiledLayerAnchor.transform.localPosition = pos;
                }

                layer.meshRenderer = Instantiate(tiledLayerPrefab, tiledLayerAnchor.transform);
                Vector2 size = layer.meshRenderer.bounds.extents;
                minTiledSize = Vector2.Min(minTiledSize, layer.meshRenderer.bounds.extents);
                layer.meshRenderer.material.SetTexture("_MainTex", layer.Texture);
            }
        }

        if (tiledLayerAnchor != null) 
        {
            float height = Camera.main.orthographicSize;
            float width = Camera.main.aspect * height;

            Vector3 scale = tiledLayerAnchor.transform.localScale;
            scale.x = width / minTiledSize.x;
            scale.y = height / minTiledSize.y;
            tiledLayerAnchor.transform.localScale = scale;
        }
    }

    void Update()
    {
        Vector3 cameraTransform = Camera.main.transform.transform.position;
        if (relativeTo != null)
        {
            cameraTransform -= relativeTo.transform.position;
        }

        foreach (var layer in layers)
        {
            Vector3 offset = cameraTransform / layer.transform.position.z;
            if (layer.IsTiled)
            {
                // This is just a pseudo perspective divide but weird.
                layer.meshRenderer.material.SetTextureOffset("_MainTex", offset);
            }
            else
            {
                layer.transform.position = layer.InitPosition + offset;
            }
        }
    }
}
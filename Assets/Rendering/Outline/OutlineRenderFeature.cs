using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

class PassData 
{
    internal Material material;
    internal int width;
    internal int height; 
    internal Color color;
    internal int thickness;
    internal List<SpriteRenderer> renderers;
}

[Serializable]
public class OutlineSettings
{
    [Range(0,8)] public int thickness;
    public Color color = Color.white;
}


public class OutlineRenderFeature : ScriptableRendererFeature
{
    [SerializeField] public OutlineSettings outlineSettings;
    [SerializeField] public Shader shader;

    private Material material;

    public static readonly List<OutlineComponent> outlineComponents = new();

    public static void RegisterOutlineComponent(OutlineComponent outlineComponent)
    {
        outlineComponents.Add(outlineComponent);
    }

    public static void UnregisterOutlineComponent(OutlineComponent outlineComponent)
    {
        outlineComponents.Remove(outlineComponent);
    }

    public override void Create()
    {
        if (shader == null)
        {
            DevLog.Error("OutlineRenderFeature missing shader");
            return;
        }


        material = new Material(shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) return;
        
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(renderingData.cameraData.camera);

        // Ah yes. A render pass for each outlined object. That sounds like a good idea :(
        foreach (var outlineComponent in outlineComponents)
        {
            // Is this something we want outlined?
            if (outlineComponent.gameObject.IsDestroyed() 
                || !outlineComponent.gameObject.activeInHierarchy 
                || !outlineComponent.enabled)
            {
                continue;
            }

            // Is this object actually showing?
            bool visible = false;
            foreach (var spriteRenderer in outlineComponent.renderers)
            {
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, spriteRenderer.bounds))
                {
                    visible = true;
                    break;
                }
            }

            if (!visible) continue;

            // WELP too bad, we have to render this.
            OutlineRenderPass outlinePass = new(material, outlineComponent.outlineSettings, outlineComponent.renderers);
            renderer.EnqueuePass(outlinePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else 
        {
            DestroyImmediate(material);
        }
    }

}

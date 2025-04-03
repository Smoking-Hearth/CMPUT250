// Most credit and blame to: https://bgolus.medium.com/the-quest-for-very-wide-outlines-ba82ed442cd9

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

class OutlineRenderPass : ScriptableRenderPass 
{
    // shader properties
    private static readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int outlineWidthID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int stepWidthID = Shader.PropertyToID("_StepWidth");

    // pass names
    private const int SHADER_PASS_INTERIOR_STENCIL = 0;
    private const int SHADER_PASS_SILHOUETTE_BUFFER_FILL = 1;
    private const int SHADER_PASS_JFA_INIT = 2;
    private const int SHADER_PASS_JFA_FLOOD = 3;
    private const int SHADER_PASS_JFA_OUTLINE = 4;

    private readonly OutlineSettings defaultSettings;

    private readonly Material material;
    private readonly List<SpriteRenderer> spriteRenderers;

    public OutlineRenderPass(Material material, OutlineSettings outlineSettings, List<SpriteRenderer> spriteRenderers) 
    {
        this.material = material;
        defaultSettings = outlineSettings;
        this.spriteRenderers = spriteRenderers;
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameCtx)
    {
        if (defaultSettings.thickness <= 0 || spriteRenderers == null || spriteRenderers.Count == 0)
            return;

        // Get resources from context
        var cameraData = frameCtx.Get<UniversalCameraData>();
        var resourceData = frameCtx.Get<UniversalResourceData>();
        
        var width = cameraData.scaledWidth;
        var height = cameraData.scaledHeight;

        TextureHandle cameraColorTarget = resourceData.activeColorTexture;

        // Create required textures
        var silhouetteBuffer = renderGraph.CreateTexture(new TextureDesc(width, height)
        {
            colorFormat = GraphicsFormat.R8_UNorm,
            clearBuffer = true,
            clearColor = Color.clear,
            useMipMap = false,
            autoGenerateMips = false,
            depthBufferBits = DepthBits.None,
            msaaSamples = MSAASamples.None,
            name = "SilhouetteBuffer"
        });

        var nearestPointBuffer = renderGraph.CreateTexture(new TextureDesc(width, height)
        {
            colorFormat = GraphicsFormat.R16G16_SNorm,
            useMipMap = false,
            autoGenerateMips = false,
            depthBufferBits = DepthBits.None,
            msaaSamples = MSAASamples.None,
            name = "NearestPointBuffer"
        });

        var pingPongBuffer = renderGraph.CreateTexture(new TextureDesc(width, height)
        {
            colorFormat = GraphicsFormat.R16G16_SNorm,
            useMipMap = false,
            autoGenerateMips = false,
            depthBufferBits = DepthBits.None,
            msaaSamples = MSAASamples.None,
            name = "PingPongBuffer"
        });

        // Step 1: Stencil Pass
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Stencil", out var passData))
        {
            builder.SetRenderAttachment(cameraColorTarget, 0);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                foreach (var renderer in spriteRenderers)
                {
                    if (renderer != null)
                        ctx.cmd.DrawRenderer(renderer, material, 0, SHADER_PASS_INTERIOR_STENCIL);
                }
            });
        }

        // Step 2: Silhouette Fill Pass
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Silhouette Fill", out var passData))
        {
            builder.SetRenderAttachment(silhouetteBuffer, 0);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                foreach (var renderer in spriteRenderers)
                {
                    // WARN: This expects Sprites to have Textures so we can't outline sprites generated from Materials
                    if (renderer != null)
                        ctx.cmd.DrawRenderer(renderer, material, 0, SHADER_PASS_SILHOUETTE_BUFFER_FILL);
                }
            });
        }

        // Step 3: JFA Init Pass
        // Ohh this is getting culled cause the other passes are failing to be added.
        renderGraph.AddBlitPass(
            new RenderGraphUtils.BlitMaterialParameters(
                silhouetteBuffer,
                nearestPointBuffer, 
                material,
                SHADER_PASS_JFA_INIT
            ),
            passName: "Outline JFA Init"
        );

        int numMips = Mathf.CeilToInt(Mathf.Log(defaultSettings.thickness + 1, 2f));
        int jfaIterations = numMips - 1;

        TextureHandle currentSrc = nearestPointBuffer;
        TextureHandle currentDst = pingPongBuffer;

        // Step 4: JFA Flood Passes
        // K for some reason we can't run passes 4 and 5, I suspect either that there is an error in the
        // shader that I have somehow missed and so it fails to compile, or there is a problem binding 
        // the created textures to _BlitTexture cause _BlitTexture is Texture2D<float4> and the textures should
        // be Texture2D<float2>. Aside from that the fragment shader also c
        for (int i = jfaIterations; i >= 0; --i)
        {
            int stepWidth = 1 << i;
            
            material.SetFloat(stepWidthID, stepWidth + 0.5f);
            renderGraph.AddBlitPass(
                new RenderGraphUtils.BlitMaterialParameters(
                    currentSrc,
                    currentDst, 
                    material,
                    SHADER_PASS_JFA_FLOOD
                ),
                passName: $"Outline JFA Flood {i}"
            );

            // Swap textures for next iteration
            (currentDst, currentSrc) = (currentSrc, currentDst);
        }

        // Step 5: Final Outline Pass
        material.SetColor(outlineColorID, defaultSettings.color);
        material.SetFloat(outlineWidthID, defaultSettings.thickness);
        renderGraph.AddBlitPass(
            new RenderGraphUtils.BlitMaterialParameters(
                currentSrc,
                cameraColorTarget, 
                material,
                SHADER_PASS_JFA_OUTLINE
            ),
            passName: "Color Outline"
        );
    }
}
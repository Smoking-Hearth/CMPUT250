using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Add this to things you want to outline. In order to generate outlines for collections
/// of sprites make sure to add their renderers. MAKE SURE TO ADD THE RENDERER FOR THE SPRITE
/// THIS IS ON ASWELL.
/// </summary>
public class OutlineComponent : MonoBehaviour
{
    [field: SerializeField] public OutlineSettings outlineSettings { get; set; }
    [field: SerializeField] public List<SpriteRenderer> renderers {get; private set;}

    void Awake()
    {
        OutlineRenderFeature.RegisterOutlineComponent(this);
    }

    void OnDestroy()
    {
        OutlineRenderFeature.UnregisterOutlineComponent(this);
    }
}
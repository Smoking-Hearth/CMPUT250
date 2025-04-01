#ifndef UVRect_HLSL
#define UVRect_HLSL

// UVRect(left, width, top, height)
void UVToRect_float(float2 UV, float4 UVRect, out float2 Out) {
    // We first rescale the provided coordinates then shift them
    // WARN: This may be upside-down
    // This is the U axis (horizontal)
    UV.r *= UVRect[1];
    UV.r += UVRect[0];
    // This is the V axis (vertical)
    UV.g *= UVRect[3];
    UV.g += UVRect[2];

    Out = UV;
}

#endif 
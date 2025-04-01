#ifndef ANIMATEDBAR_HLSL
#define ANIMATEDBAR_HLSL

// level: This is a single UV channel across the Canvas item
// barPosition: Where within the UV the edge of the bar should be
// offset: 

float UpperGradient(float level, float barPosition) {
    return (level - barPosition) / (1.0 - barPosition);
}

void UpperGradient_float(float level, float barPosition, out float Out) {
    Out = UpperGradient(level, barPosition);
}

void AnimatedBar_float(float level, float barPosition, float intensity, float noise, out float Out) {
    const float threshold = 0.2;
    float upperGradient = UpperGradient(level, barPosition);
    float backgroundSmoke = upperGradient * (noise * intensity + (1.0 - intensity));
    // This is the step from filled to unfilled, we offset the edge by noise because...
    float unstableStep = 1.0 - smoothstep(noise * threshold + barPosition, barPosition, level);
    Out = step(min(barPosition, threshold), min(backgroundSmoke, unstableStep));
}

void FillColor_float(float value, float4 black, float4 white, out float4 Out) {
    Out = (1.0 - value) * black + white;
}

#endif
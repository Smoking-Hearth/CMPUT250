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

void AnimatedBar_float(float x, float lo, float hi, float intensity, float gradient, out float Out) {
    float edgeOffset = clamp(hi - lo, intensity * smoothstep(0.05, 0.20, x), 1.0) * gradient;
    hi += edgeOffset;
    lo += edgeOffset;
    float mask = step(lo, x) * (1.0 - step(hi, x));
    float bar = smoothstep(lo, hi, x) + (gradient + intensity) * mask;
    Out = step(0.02, bar);
}

void FillColor_float(float value, float4 black, float4 white, out float4 Out) {
    Out = (1.0 - value) * black + white;
}

#endif
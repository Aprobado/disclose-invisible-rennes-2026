
#ifndef APROBADO_ENVIRONMENT_OCCLUSION_INCLUDED
#define APROBADO_ENVIRONMENT_OCCLUSION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.meta.xr.sdk.core/Shaders/EnvironmentDepth/URP/EnvironmentOcclusionURP.hlsl"
float2 CalculateUVCoords(float3 worldCoords)
{
    const float4 depthSpace =
    mul(_EnvironmentDepthReprojectionMatrices[unity_StereoEyeIndex], float4(worldCoords, 1.0));

    const float2 uvCoords = (depthSpace.xy / depthSpace.w + 1.0f) * 0.5f;
    return uvCoords;
}
float EyeToLinearDepth(float eyeDepth)
{
    const float inputDepthNdc = eyeDepth * 2.0 - 1.0;
    if (inputDepthNdc == 1.0f) {
        // When 'inputDepthNdc == 1.0f', this will produce division by zero because '_EnvironmentDepthZBufferParams.y == -1.0'
        // So we return a big linear value when this happens. 10km should be enough for applications that rely on Depth API.
        return 10000;
    }
    const float linearDepth = (1.0f / (inputDepthNdc + _EnvironmentDepthZBufferParams.y)) * _EnvironmentDepthZBufferParams.x;

    return linearDepth;
}
// given by ChatGPT
// float SamplePreprocessedEnvironmentDepthLinear(float2 uv)
// {
//     float4 texSample = SamplePreprocessedDepth(uv, unity_StereoEyeIndex);
//
//     // Reconstruct the average encoded depth.
//     float encodedDepth = 1.0f - texSample.x + texSample.z;
//
//     // Convert to the same NDC convention as EnvironmentDepth.
//     float depthNdc = encodedDepth * 2.0f - 1.0f;
//
//     if (depthNdc == 1.0f)
//         return 10000.0f;
//
//     return _EnvironmentDepthZBufferParams.x /
//            (depthNdc + _EnvironmentDepthZBufferParams.y);
// }
#endif

#include "ShaderApiReflectionSupport.hlsl"

///<funchints>
///     <sg:ProviderKey>EnvironmentDepth</sg:ProviderKey>
///     <sg:DisplayName>Environment Depth</sg:DisplayName>
///     <sg:SearchCategory>Custom</sg:SearchCategory>
///</funchints>
///<paramhints name = "worldCoords">
///    <sg:DisplayName>World Coords</sg:DisplayName>
///    <sg:Position/>
///    <sg:Default>0,0,0</sg:Default>
///</paramhints>
UNITY_EXPORT_REFLECTION
void EnvironmentDepth_float(float3 worldCoords, out float depth)
{
    #ifndef SHADERGRAPH_PREVIEW
    float2 uvCoords = CalculateUVCoords(worldCoords);
    depth = SampleEnvironmentDepth(uvCoords);
    #else
    depth = 0;
    #endif
}

///<funchints>
///     <sg:ProviderKey>PreprocessedAveragedDepth</sg:ProviderKey>
///     <sg:DisplayName>Preprocessed Averaged Depth</sg:DisplayName>
///     <sg:SearchCategory>Custom</sg:SearchCategory>
///</funchints>
///<paramhints name = "preprocessedDepth">
///    <sg:DisplayName>Preprocessed Depth</sg:DisplayName>
///    <sg:Color/>
///    <sg:Default>0,0,0,0</sg:Default>
///</paramhints>
UNITY_EXPORT_REFLECTION
void PreprocessedAveragedDepth_float(float4 preprocessedDepth, out float depth)
{
    #ifndef SHADERGRAPH_PREVIEW
    depth = 1.0f - preprocessedDepth.x + preprocessedDepth.z;
    #else
    depth = 0;
    #endif
}

///<funchints>
///     <sg:ProviderKey>EnvironmentPreprocessedDepth</sg:ProviderKey>
///     <sg:DisplayName>Environment Preprocessed Depth</sg:DisplayName>
///     <sg:SearchCategory>Custom</sg:SearchCategory>
///</funchints>
///<paramhints name = "worldCoords">
///    <sg:DisplayName>World Coords</sg:DisplayName>
///    <sg:Position/>
///    <sg:Default>0,0,0</sg:Default>
///</paramhints>
UNITY_EXPORT_REFLECTION
void EnvironmentPreprocessedDepth_float(float3 worldCoords, out float4 result)
{
    #ifndef SHADERGRAPH_PREVIEW
    float2 uvCoords = CalculateUVCoords(worldCoords);
    const float2 halfPixelOffset = 0.5f * float2(_PreprocessedEnvironmentDepthTexture_TexelSize.xy);
    uvCoords -= halfPixelOffset;
    result = SamplePreprocessedDepth(uvCoords, unity_StereoEyeIndex);
    #else
    result = float4(1,1,1,1);
    #endif
}

///<funchints>
///     <sg:ProviderKey>EnvironmentEyeToLinearDepth</sg:ProviderKey>
///     <sg:DisplayName>Environment Eye To Linear Depth</sg:DisplayName>
///     <sg:SearchCategory>Custom</sg:SearchCategory>
///</funchints>
///<paramhints name = "eyeDepth">
///    <sg:DisplayName>Eye Depth</sg:DisplayName>
///    <sg:Float/>
///    <sg:Default>0</sg:Default>
///</paramhints>
UNITY_EXPORT_REFLECTION
void EnvironmentEyeToLinearDepth_float(float eyeDepth, out float linearDepth)
{
    #ifndef SHADERGRAPH_PREVIEW
    linearDepth = EyeToLinearDepth(eyeDepth);
    #else
    linearDepth = 0;
    #endif
}

///<funchints>
///     <sg:ProviderKey>EnvironmentDepthLinear</sg:ProviderKey>
///     <sg:DisplayName>Environment Depth Linear</sg:DisplayName>
///     <sg:SearchCategory>Custom</sg:SearchCategory>
///</funchints>
///<paramhints name = "worldCoords">
///    <sg:DisplayName>World Coords</sg:DisplayName>
///    <sg:Position/>
///    <sg:Default>0,0,0</sg:Default>
///</paramhints>
UNITY_EXPORT_REFLECTION
void EnvironmentDepthLinear_float(float3 worldCoords, out float depth)
{
    #ifdef SHADERGRAPH_PREVIEW
    depth = 1.0;
    #else
    float2 uvCoords = CalculateUVCoords(worldCoords);
    depth = 0.5;
    #if defined(HARD_OCCLUSION)
    depth = SampleEnvironmentDepthLinear(uvCoords);
    // return CalculateEnvironmentDepthHardOcclusion(uvCoords, linearSceneDepth);
    #elif defined(SOFT_OCCLUSION)
    float4 texSample = SamplePreprocessedDepth(uvCoords, unity_StereoEyeIndex);
    float interp = texSample.z / texSample.w;
    float eyeDepth = lerp(texSample.x, texSample.y, interp);
    depth = eyeDepth;
    /*
    float min = EyeToLinearDepth(1 - texSample.x);
    float max = EyeToLinearDepth(1 - texSample.y);
    // float depth = SampleEnvironmentDepthLinear(uvCoords);
    // if (alphas.y - alphas.x > 0.03f) {
        // Mix the minima and maxima according to the given interpolation factor.
        // Adjust the smoothstep coefficients to shrink or enlarge the transition radius.
        // Between 0.0f (transition deepest in the foreground) and 0.5f (don't transition in the foreground)
        const float kForegroundLevel = 0.2f;
        // Between 0.5f (don't transition in the background) and 1.0f (transition deepest in the background)
        const float kBackgroundLevel = 0.8f;
        float interp = texSample.z / texSample.w;
        depth = texSample.x;// lerp(min, max, smoothstep(kForegroundLevel, kBackgroundLevel, interp));
        // alpha = lerp(min, max, smoothstep(kForegroundLevel, kBackgroundLevel, interp));
    // }
    // return alpha;
    // return CalculateEnvironmentDepthSoftOcclusion(uvCoords, linearSceneDepth);
    */
    #endif
    #endif
}

#endif

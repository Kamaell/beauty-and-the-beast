//-------------------------------------------------------------------------------------
// Fill SurfaceData/Builtin data function
//-------------------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/TerrainLit/TerrainLitSplatCommon.hlsl"

TEXTURE2D(_MainTex);
TEXTURE2D(_MetallicTex);
SAMPLER(sampler_MainTex);

#ifdef DEBUG_DISPLAY
float4 _MainTex_TexelSize;
float4 _MainTex_MipInfo;
#endif

// We don't use emission for terrain
#define _EmissiveColor float3(0,0,0)
#define _AlbedoAffectEmissive 0
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitBuiltinData.hlsl"
#undef _EmissiveColor
#undef _AlbedoAffectEmissive

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

void GetSurfaceAndBuiltinData(FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
#ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
    {
        float3 normalOS = SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_MainTex, (input.texCoord0.xy + 0.5f) * _TerrainHeightmapRecipSize.xy).rgb * 2 - 1;
        float3 normalWS = mul((float3x3)GetObjectToWorldMatrix(), normalOS);
        float3 tangentWS = cross(GetObjectToWorldMatrix()._13_23_33, normalWS);

        input.worldToTangent = BuildWorldToTangent(float4(tangentWS, 1.0), normalWS);

        input.texCoord0.xy *= _TerrainHeightmapRecipSize.zw;
    }
#endif

    // terrain lightmap uvs are always taken from uv0
    input.texCoord1 = input.texCoord2 = input.texCoord0;

    surfaceData.baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texCoord0.xy).rgb;

    surfaceData.perceptualSmoothness = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texCoord0.xy).a;
    surfaceData.ambientOcclusion = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MainTex, input.texCoord0.xy).g;
    surfaceData.metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MainTex, input.texCoord0.xy).r;
    surfaceData.tangentWS = normalize(input.worldToTangent[0].xyz); // The tangent is not normalize in worldToTangent for mikkt. Tag: SURFACE_GRADIENT
    surfaceData.subsurfaceMask = 0;
    surfaceData.thickness = 1;
    surfaceData.diffusionProfile = 0;

    surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

    // Init other parameters
    surfaceData.anisotropy = 0.0;
    surfaceData.specularColor = float3(0.0, 0.0, 0.0);
    surfaceData.coatMask = 0.0;
    surfaceData.iridescenceThickness = 0.0;
    surfaceData.iridescenceMask = 0.0;

    // Transparency parameters
    // Use thickness from SSS
    surfaceData.ior = 1.0;
    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
    surfaceData.atDistance = 1000000.0;
    surfaceData.transmittanceMask = 0.0;

#ifdef SURFACE_GRADIENT
    float3 normalTS = float3(0.0, 0.0, 0.0); // No gradient
#else
    float3 normalTS = float3(0.0, 0.0, 1.0);
#endif

    GetNormalWS(input, normalTS, surfaceData.normalWS, float3(1.0, 1.0, 1.0));

    surfaceData.geomNormalWS = input.worldToTangent[2];

    float3 bentNormalWS = surfaceData.normalWS;

    if (surfaceData.ambientOcclusion != 1.0f)
        surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
    else
        surfaceData.specularOcclusion = 1.0f;

#if HAVE_DECALS
    if (_EnableDecals)
    {
        float alpha = 1.0; // unused
        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, alpha);
        ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
    }
#endif

#ifdef DEBUG_DISPLAY
    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
    {
        surfaceData.baseColor = GetTextureDataDebug(_DebugMipMapMode, input.texCoord0.xy, _MainTex, _MainTex_TexelSize, _MainTex_MipInfo, surfaceData.baseColor);
        surfaceData.metallic = 0;
    }

    // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
    // as it can modify attribute use for static lighting
    ApplyDebugToSurfaceData(input.worldToTangent, surfaceData);
#endif

    GetBuiltinData(input, V, posInput, surfaceData, 1, bentNormalWS, 0, builtinData);
}

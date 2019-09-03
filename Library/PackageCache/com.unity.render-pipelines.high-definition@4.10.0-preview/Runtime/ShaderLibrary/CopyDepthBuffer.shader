Shader "Hidden/HDRP/CopyDepthBuffer"
{

    Properties{
        _FlipY("FlipY", Int) = 0
    }
    HLSLINCLUDE



    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Name "Copy Depth"

            Cull   Off
            ZTest  Always
            ZWrite On
            Blend  Off
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
            #pragma fragment Frag
            #pragma vertex Vert
            // #pragma enable_d3d11_debug_symbols

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            TEXTURE2D_FLOAT(_InputDepthTexture);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_Position;
            };

            int _FlipY;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                return output;
            }

            float Frag(Varyings input) : SV_Depth
            {
                PositionInputs posInputs = GetPositionInput(input.positionCS.xy, _ScreenSize.zw);
                return LOAD_TEXTURE2D(_InputDepthTexture, (_FlipY == 0) ? posInputs.positionSS : float2(posInputs.positionNDC.x, 1.0 - posInputs.positionNDC.y) * _ScreenSize.xy).x;
            }

            ENDHLSL
        }
    }
    Fallback Off
}

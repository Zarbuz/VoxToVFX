// This is based on the work of Steven Sell
// Check out their excellent article at https://www.vertexfragment.com/ramblings/unity-postprocessing-sobel-outline/
Shader "Hidden/Shader/Sobel"
{
    Properties
    {
        // This property is necessary to make the CommandBuffer.Blit bind the source texture to _MainTex
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    float _Thickness;
    float4 _Colour;
    float _DepthMultiplier;
    float _DepthBias;
    float _NormalMultiplier;
    float _NormalBias;
    TEXTURE2D_X(_MainTex);

    float Sobel_Basic(float topLeft, float top, float topRight,
                      float left, float right,
                      float bottomLeft, float bottom, float bottomRight)
    {
        float x =  topLeft + 2 * left + bottomLeft - topRight   - 2 * right  - bottomRight;
        float y = -topLeft - 2 * top  - topRight   + bottomLeft + 2 * bottom + bottomRight;

        return sqrt(x * x + y * y);
    }

    float Sobel_Scharr(float topLeft, float top, float topRight,
                       float left, float right,
                       float bottomLeft, float bottom, float bottomRight)
    {
        float x = -3 * topLeft - 10 * left - 3 * bottomLeft + 3 * topRight   + 10 * right  + 3 * bottomRight;
        float y =  3 * topLeft + 10 * top  + 3 * topRight   - 3 * bottomLeft - 10 * bottom - 3 * bottomRight;

        return sqrt(x * x + y * y);
    }

    float SobelSampleDepth(float2 uv, float offsetU, float offsetV)
    {
        float topLeft       = SampleCameraDepth(uv + float2(-offsetU,  offsetV));
        float top           = SampleCameraDepth(uv + float2(       0,  offsetV));
        float topRight      = SampleCameraDepth(uv + float2( offsetU,  offsetV));

        float left          = SampleCameraDepth(uv + float2(-offsetU,        0));
        float centre        = SampleCameraDepth(uv + float2(       0,        0));
        float right         = SampleCameraDepth(uv + float2( offsetU,        0));

        float bottomLeft    = SampleCameraDepth(uv + float2(-offsetU, -offsetV));
        float bottom        = SampleCameraDepth(uv + float2(       0, -offsetV));
        float bottomRight   = SampleCameraDepth(uv + float2( offsetU, -offsetV));

        return Sobel_Scharr(abs(topLeft - centre),      abs(top - centre),       abs(topRight - centre),
                             abs(left - centre),                                  abs(right - centre),
                             abs(bottomLeft - centre),   abs(bottom - centre),    abs(bottomRight - centre));
    }

    float3 SampleWorldNormal(float2 uv)
    {
        // if the camera depth is invalid - early out
        if (SampleCameraDepth(uv) <= 0)
            return float3(0, 0, 0);

        NormalData normalData;
        DecodeFromNormalBuffer(uv * _ScreenSize.xy, normalData);

        return normalData.normalWS;
    }


    float Sobel_Basic(float3 topLeft, float3 top, float3 topRight,
                      float3 left, float3 right,
                      float3 bottomLeft, float3 bottom, float3 bottomRight)
    {
        float3 x =  topLeft + 2 * left + bottomLeft - topRight   - 2 * right  - bottomRight;
        float3 y = -topLeft - 2 * top  - topRight   + bottomLeft + 2 * bottom + bottomRight;

        return sqrt(dot(x, x) + dot(y, y));
    }

    float Sobel_Scharr(float3 topLeft, float3 top, float3 topRight,
                       float3 left, float3 right,
                       float3 bottomLeft, float3 bottom, float3 bottomRight)
    {
        float3 x = -3 * topLeft - 10 * left - 3 * bottomLeft + 3 * topRight   + 10 * right  + 3 * bottomRight;
        float3 y =  3 * topLeft + 10 * top  + 3 * topRight   - 3 * bottomLeft - 10 * bottom - 3 * bottomRight;

        return sqrt(dot(x, x) + dot(y, y));
    }

    float SobelSampleNormal(float2 uv, float offsetU, float offsetV)
    {
        float3 topLeft       = SampleWorldNormal(uv + float2(-offsetU,  offsetV));
        float3 top           = SampleWorldNormal(uv + float2(       0,  offsetV));
        float3 topRight      = SampleWorldNormal(uv + float2( offsetU,  offsetV));

        float3 left          = SampleWorldNormal(uv + float2(-offsetU,        0));
        float3 centre        = SampleWorldNormal(uv + float2(       0,        0));
        float3 right         = SampleWorldNormal(uv + float2( offsetU,        0));

        float3 bottomLeft    = SampleWorldNormal(uv + float2(-offsetU, -offsetV));
        float3 bottom        = SampleWorldNormal(uv + float2(       0, -offsetV));
        float3 bottomRight   = SampleWorldNormal(uv + float2( offsetU, -offsetV));

        return Sobel_Scharr(abs(topLeft - centre),      abs(top - centre),       abs(topRight - centre),
                            abs(left - centre),                                  abs(right - centre),
                            abs(bottomLeft - centre),   abs(bottom - centre),    abs(bottomRight - centre));
    }

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Note that if HDUtils.DrawFullScreen is used to render the post process, use ClampAndScaleUVForBilinearPostProcessTexture(input.texcoord.xy) to get the correct UVs

        float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord).xyz;

        // determine our offsets
        float offsetU = _Thickness / _ScreenSize.x;
        float offsetV = _Thickness / _ScreenSize.y;

        // run the sobel sampling of the depth buffer
        float sobelDepth = SobelSampleDepth(input.texcoord.xy, offsetU, offsetV);
        sobelDepth = pow(abs(saturate(sobelDepth)) * _DepthMultiplier, _DepthBias);

        // run the sobel sampling of the normals
        float sobelNormal = SobelSampleNormal(input.texcoord.xy, offsetU, offsetV);
        sobelNormal = pow(abs(saturate(sobelNormal)) * _NormalMultiplier, _NormalBias);

        float outlineIntensity = saturate(max(sobelDepth, sobelNormal));

        // apply the sobel effect
        float3 finalColor = lerp(sourceColor, _Colour, outlineIntensity * _Intensity);

        //return float4(sobelNormal, sobelNormal, sobelNormal, 1);
        //return float4(sobelDepth, sobelDepth, sobelDepth, 1);
        return float4(finalColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Sobel"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
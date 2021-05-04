#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float3 Color;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Color : TEXCOORD0;
};

VertexShaderOutput MainVertexShader(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    // Project position
    output.Position = mul(input.Position, WorldViewProjection);

    // Propagate color
    output.Color = input.Color;

    return output;
}

float4 MainPixelShader(VertexShaderOutput input) : COLOR
{
    return float4(Color, 1.0);
}

float4 BackgroundPixelShader(VertexShaderOutput input) : COLOR
{
    return float4(Color * 0.5, 1.0);
}


technique Gizmos
{
    pass Background
    {
        VertexShader = compile VS_SHADERMODEL MainVertexShader();
        PixelShader = compile PS_SHADERMODEL BackgroundPixelShader();
    }
    pass Foreground
    {
        VertexShader = compile VS_SHADERMODEL MainVertexShader();
        PixelShader = compile PS_SHADERMODEL MainPixelShader();
    }
};

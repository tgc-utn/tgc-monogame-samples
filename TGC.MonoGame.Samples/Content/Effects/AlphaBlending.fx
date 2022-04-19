#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float AlphaFactor;
float3 Tint;

texture Texture;
sampler2D textureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};



struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = mul(input.Position, WorldViewProjection);

    // Propagate Texture Coordinates
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture using our Texture Coordinates
    // Alpha blending in this case is the texture alpha channel with a factor
    // Color is the texture color with a tint
    float4 color = tex2D(textureSampler, input.TextureCoordinate);
    
    return float4(color.rgb * Tint, color.a * AlphaFactor);
}


technique AlphaBlending
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

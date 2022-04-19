#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 WorldViewProjection;
float2 Tiling;

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

struct WorldVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct WorldVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};


VertexShaderOutput BaseTilingVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

    // Propagate scaled Texture Coordinates
    output.TextureCoordinate = input.TextureCoordinate * Tiling;

    return output;
}

float4 BaseTilingPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture using our scaled Texture Coordinates
    return tex2D(textureSampler, input.TextureCoordinate);
}



WorldVertexShaderOutput WorldTilingVS(in WorldVertexShaderInput input)
{
    WorldVertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

    // Propagate the World Position with the Tiling applied (scaling them)
    output.WorldPosition = mul(input.Position, World);

    // Propagate the Normal to choose from which side the coordinates will be used
    output.Normal = input.Normal;

    return output;
}

float4 WorldTilingPS(WorldVertexShaderOutput input) : COLOR
{
    // Get how parallel the normal of this point is to the X plane
    float xAlignment = abs(dot(input.Normal, float3(1, 0, 0)));
    // Same for the Y plane
    float yAlignment = abs(dot(input.Normal, float3(0, 1, 0)));

    // Use the world position as texture coordinates 
    // Choose which coordinates we will use based on our normal
    float2 yPlane = lerp(input.WorldPosition.xy, input.WorldPosition.xz, yAlignment);
    float2 resultPlane = lerp(yPlane, input.WorldPosition.yz, xAlignment);

    float2 textureCoordinates = resultPlane * Tiling;


    // Sample the texture using our scaled World Texture Coordinates
    return tex2D(textureSampler, textureCoordinates);
}

technique BaseTiling
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL BaseTilingVS();
        PixelShader = compile PS_SHADERMODEL BaseTilingPS();
    }
};

technique WorldTiling
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL WorldTilingVS();
        PixelShader = compile PS_SHADERMODEL WorldTilingPS();
    }
};


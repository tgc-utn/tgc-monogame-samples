#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float2 Tiling;

float2 TextureSize;
float MipLevelCount;

texture Texture;
sampler2D textureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D textureSamplerBilinear = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = Point;
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

    // Propagate scaled Texture Coordinates
    output.TextureCoordinate = input.TextureCoordinate * Tiling;

    return output;
}

float4 TrilinearPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture using our scaled Texture Coordinates
    return tex2D(textureSampler, input.TextureCoordinate);
}

float4 LinearPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture using our scaled Texture Coordinates    
    return tex2D(textureSamplerBilinear, input.TextureCoordinate);
}

float MipMapLevel(in float2 textureCoordinates, float2 textureResolution)
{
    textureCoordinates *= textureResolution;
    float2 dx_vtc = ddx(textureCoordinates);
    float2 dy_vtc = ddy(textureCoordinates);
    float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
    float mml = 0.5 * log2(delta_max_sqr);
    return max(0, mml);
}


float4 DebugPS(VertexShaderOutput input) : COLOR
{
    float mip = MipMapLevel(input.TextureCoordinate, TextureSize.xy);
    mip = round(mip);
    mip /= MipLevelCount;
    
    return mip;
}



technique Trilinear
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL TrilinearPS();
    }
};


technique Bilinear
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL LinearPS();
    }
};


technique Debug
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DebugPS();
    }
};


#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
    
static const int kernel_r = 6;
static const int kernel_size = 13;
static const float Kernel[kernel_size] =
{
    0.002216, 0.008764, 0.026995, 0.064759, 0.120985, 0.176033, 0.199471, 0.176033, 0.120985, 0.064759, 0.026995, 0.008764, 0.002216,
};

float2 screenSize;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 BlurPS(in VertexShaderOutput input) : COLOR
{
    float4 finalColor = float4(0, 0, 0, 1);
    for (int x = 0; x < kernel_size; x++)
        for (int y = 0; y < kernel_size; y++)
        {
            float2 scaledTextureCoordinates = input.TextureCoordinates + float2((float) (x - kernel_r) / screenSize.x, (float) (y - kernel_r) / screenSize.y);
            finalColor += tex2D(textureSampler, scaledTextureCoordinates) * Kernel[x] * Kernel[y];
        }
    return finalColor;
}


float4 BlurHorizontal(in VertexShaderOutput input) : COLOR
{
    float4 finalColor = float4(0, 0, 0, 1);
    for (int i = 0; i < kernel_size; i++)
    {
        float2 scaledTextureCoordinates = input.TextureCoordinates + float2((float) (i - kernel_r) / screenSize.x, 0);
        finalColor += tex2D(textureSampler, scaledTextureCoordinates) * Kernel[i];
    }
    return finalColor;    
}

float4 BlurVertical(in VertexShaderOutput input) : COLOR
{
    float4 finalColor = float4(0, 0, 0, 1);
    for (int i = 0; i < kernel_size; i++)
    {
        float2 scaledTextureCoordinates = input.TextureCoordinates + float2(0, (float) (i - kernel_r) / screenSize.y);
        finalColor += tex2D(textureSampler, scaledTextureCoordinates) * Kernel[i];
    }
    return finalColor;
}


technique Blur
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BlurPS();
    }
};

technique BlurHorizontalTechnique
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BlurHorizontal();
    }
};

technique BlurVerticalTechnique
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BlurVertical();
    }
};
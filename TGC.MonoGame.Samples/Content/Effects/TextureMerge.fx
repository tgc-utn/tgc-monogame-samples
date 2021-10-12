#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float time;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture overlayTexture;
sampler2D overlayTextureSampler = sampler_state
{
	Texture = (overlayTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

    
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
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

uniform float2 InverseScreenSize;

uniform float Displacement;

float4 MergePS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
    
    
    // 0 - 1
    // 0 - 1
    
    float factor = distance(input.TextureCoordinates, float2(0.5, 0.5));
    
    float displacement = Displacement * factor * 4.0;
    
    float4 rightFragmentColor = tex2D(textureSampler,
        input.TextureCoordinates + InverseScreenSize.x * displacement);
    
    float4 leftFragmentColor = tex2D(textureSampler,
        input.TextureCoordinates - InverseScreenSize.x * displacement);
    
    baseColor.b = rightFragmentColor.b;
    baseColor.r = leftFragmentColor.r;
    
    return baseColor;
}

technique Merge
{
    pass Pass0
    {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MergePS();
	}
};
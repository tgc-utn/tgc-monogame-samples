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

float4 MergePS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
	float4 overlayColor = tex2D(overlayTextureSampler, input.TextureCoordinates);
    
	float timeFactor = sin(time * 2.0) * 0.5 + 0.5;
	float4 finalColor = float4(lerp(baseColor.rgb, overlayColor.rgb, overlayColor.a * timeFactor), 1.0);
    
    return finalColor;
}

technique Merge
{
    pass Pass0
    {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MergePS();
	}
};
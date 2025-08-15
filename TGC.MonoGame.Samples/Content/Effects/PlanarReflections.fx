#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 ReflectionView;
float4x4 Projection;
float4x4 WorldViewProjection;
float4x4 World;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float4 Normal : TEXCOORD2;
	float4 ReflectionPosition : TEXCOORD3;
};

texture ReflectionTexture;
sampler2D reflectionSampler = sampler_state
{
	Texture = (ReflectionTexture);
	ADDRESSU = Clamp;
	ADDRESSV = Clamp;
	MINFILTER = Linear;
	MAGFILTER = Linear;
	MIPFILTER = Linear;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput) 0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.WorldPosition = mul(input.Position, World);
	output.Normal = input.Normal;
	output.TextureCoordinates = input.TextureCoordinates;

	float4x4 reflectProjectWorld = mul(ReflectionView, Projection);
	reflectProjectWorld = mul(World, reflectProjectWorld);
    
	output.ReflectionPosition = mul(input.Position, reflectProjectWorld);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 reflectionTexCoord = input.ReflectionPosition;

	// Normalized device coordinates
	reflectionTexCoord.xy /= reflectionTexCoord.w;

	// Adjust offset
	reflectionTexCoord.x = 0.5f * reflectionTexCoord.x + 0.5f;
	reflectionTexCoord.y = -0.5f * reflectionTexCoord.y + 0.5f;

	float4 reflectionColor = tex2D(reflectionSampler, reflectionTexCoord.xy);

	return reflectionColor;
	
	//return float4(input.Normal.x, input.Normal.y, input.Normal.z, 1.0f);
}

technique PlanarReflections
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
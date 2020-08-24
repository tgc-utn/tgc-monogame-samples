#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;

float nearPlaneDistance, farPlaneDistance;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (baseTexture);
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

	output.Position = mul(input.Position, World);
	output.TextureCoordinates = input.TextureCoordinates;
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tex2D(textureSampler, input.TextureCoordinates);
}

// Required when using a perspective projection matrix
float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return ((2.0 * nearPlaneDistance * farPlaneDistance) / (farPlaneDistance + nearPlaneDistance - z * (farPlaneDistance - nearPlaneDistance))) / farPlaneDistance;
}

float4 DepthPS(VertexShaderOutput input) : COLOR
{
    float depth = tex2D(textureSampler, input.TextureCoordinates).r;
    float linearDepth = LinearizeDepth(depth);
	// Perspective
	return float4(linearDepth, linearDepth, linearDepth, 1.0); 
}

technique Default
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};


technique DebugDepth
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL DepthPS();
    }
};
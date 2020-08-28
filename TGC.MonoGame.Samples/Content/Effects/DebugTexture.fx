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

texture cubeMapTexture;
samplerCUBE cubeMapTextureSampler = sampler_state
{
    Texture = (cubeMapTexture);
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



float4 CubeMapPS(VertexShaderOutput input) : COLOR
{
    float2 remappedTextureCoordinates = input.TextureCoordinates * 2.0 - 1.0;
    
    float division = input.TextureCoordinates.x * 6.0;
    
    float2 fractional = float2(frac(input.TextureCoordinates.x * 6.0), remappedTextureCoordinates.y);
    float integer = floor(division);
    float3 lookUpVector = float3(0, 0, 0);
    if (integer == 0)
        lookUpVector = float3(fractional, 1);
    else if (integer == 1)
        lookUpVector = float3(1, fractional);
    else if (integer == 2)
        lookUpVector = float3(fractional, -1);
    else if (integer == 3)
        lookUpVector = float3(-1, fractional);
    else if (integer == 4)
        lookUpVector = float3(fractional.x, 1, fractional.y);
    else if (integer == 5)
        lookUpVector = float3(fractional.x, -1, fractional.y);
    
    return texCUBE(cubeMapTextureSampler, lookUpVector);
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

technique DebugCubeMap
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL CubeMapPS();
    }
};
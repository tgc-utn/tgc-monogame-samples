#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float tiling;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD1;
};

texture ModelTexture;

sampler2D textureClamp = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

sampler2D textureMirror = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = MIRROR;
    AddressV = MIRROR;
};

sampler2D textureWrap = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = WRAP;
    AddressV = WRAP;
};

sampler2D textureBorder = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = BORDER;
    AddressV = BORDER;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	
	// Project position
    output.Position = mul(viewPosition, Projection);

	// Propagate texture coordinates
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 ClampPS(VertexShaderOutput input) : COLOR
{
    // Tiling
    input.TextureCoordinate = input.TextureCoordinate * tiling;
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2D(textureClamp, input.TextureCoordinate);
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
    return textureColor;
}

float4 MirrorPS(VertexShaderOutput input) : COLOR
{
    // Tiling
    input.TextureCoordinate = input.TextureCoordinate * tiling;
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2D(textureMirror, input.TextureCoordinate);
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
    return textureColor;
}


float4 WrapPS(VertexShaderOutput input) : COLOR
{
    // Tiling
    input.TextureCoordinate = input.TextureCoordinate * tiling;
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2D(textureWrap, input.TextureCoordinate);
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
    return textureColor;
}

float4 BorderPS(VertexShaderOutput input) : COLOR
{
    // Tiling
    input.TextureCoordinate = input.TextureCoordinate * tiling;
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2D(textureBorder, input.TextureCoordinate);
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
    return textureColor;
}

float4 LodPS(VertexShaderOutput input) : COLOR
{
    // Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2Dlod(textureBorder, float4(input.TextureCoordinate,tiling,tiling));
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
    return textureColor;
}

technique TextureClamp
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ClampPS();
	}
};

technique TextureMirror
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MirrorPS();
	}
};

technique TextureWrap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL WrapPS();
	}
};

technique TextureBorder
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL BorderPS();
	}
};

technique TextureLod
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL LodPS();
	}
};
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
	float3 WorldPos: TEXCOORD2;
};

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float Time = 0;

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Animate position
	//input.Position.y = F(input.Position.x , input.Position.z);

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
	
	// Project position
    output.Position = mul(viewPosition, Projection);

	// Propagate texture coordinates
    output.TextureCoordinate = input.TextureCoordinate;

	output.WorldPos = worldPosition.xyz;

	// Propagate color by vertex
    output.Color = input.Color;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 clr;
	if (input.TextureCoordinate.x > 5)
		clr = float4(input.Color.rgb, 1);
	else
		clr = float4(tex2D(textureSampler, input.TextureCoordinate).rgb , 1);

	return clr;

	//return float4(input.TextureCoordinate, 0, 1);
	//return float4(input.Color.rgb*input.TextureCoordinate.y  ,1);

}

float4 ColorPS(VertexShaderOutput input) : COLOR
{
	float4 clr;
	if (input.TextureCoordinate.x > 5)
		clr = float4(input.Color.rgb, 1);
	else
		clr = float4(input.Color.rgb*input.TextureCoordinate.y  ,1);
	return clr;
}

float4 TexCoordsPS(VertexShaderOutput input) : COLOR
{
	float4 clr;
	if (input.TextureCoordinate.x > 5)
		clr = float4(input.Color.rgb, 1);
	else
		clr = float4(input.TextureCoordinate.xy, 0, 1);
	return clr;
}

float4 EdgeDetectPS(VertexShaderOutput input) : COLOR
{
	float4 clr;
	if (input.TextureCoordinate.x > 5)
		clr = float4(input.Color.rgb, 1);
	else
	{
		float2 d = fwidth(input.TextureCoordinate);
		float2 a2 = smoothstep(float2(0,0), d * 8.0, input.TextureCoordinate);
		float k = min(a2.x, a2.y);
		clr = float4(k, k, k, 1);
	}
	return clr;
}

float4 DummyPS(VertexShaderOutput input) : COLOR
{
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	textureColor.a = 1 + Time;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
	return 0.8 * textureColor + 0.2 * input.Color;
}

technique ColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ColorPS();
	}
};

technique EdgeDectect
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL EdgeDetectPS();
	}
};

technique TexCoordsDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL TexCoordsPS();
	}
};

technique TextureDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique Dummy
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL DummyPS();
	}
};

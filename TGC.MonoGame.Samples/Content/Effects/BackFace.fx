#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD3;
    float InsideCylinder : TEXCOORD4;
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

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, WorldViewProjection);
    output.InsideCylinder = step(length(input.Position.xy), 0.2);
    output.Normal = input.Normal;
	
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float facesForward = saturate(sign(dot(normal, float3(0, 0, 1))));
    float inside = 1.0 - step(input.InsideCylinder, 0.0);
    if (inside * facesForward)
        discard;
    
    return float4(input.Normal + 0.5, 1.0);
}

technique DefaultTechnique
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

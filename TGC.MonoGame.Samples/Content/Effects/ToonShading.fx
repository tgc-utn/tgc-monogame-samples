#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float4x4 World;
float4x4 InverseTransposeWorld;

float3 colorA;
float3 colorB;
float3 colorC;
float3 colorD;

float3 colorRange;

float KAmbient; 

float3 lightPosition;


texture LookUpTableTexture;
sampler1D lookUpTableSampler = sampler_state
{
    Texture = (LookUpTableTexture);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Normal : TEXCOORD1;        
    float4 WorldPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPosition = mul(input.Position, World);
    output.Normal = mul(float4(normalize(input.Normal.xyz), 1.0), InverseTransposeWorld);
	
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 normal = normalize(input.Normal.xyz);
    
	// Calculate the diffuse light
    float NdotL = dot(normal, lightDirection);
    float diffuseLight = NdotL * 0.5 + 0.5;
    
    // Add an ambient factor
    float light = KAmbient + diffuseLight;
    
    // Interpolate between colors depending on the amount of light
    float3 color = lerp(colorA, colorB, step(colorRange.x, light));
    color = lerp(color, colorC, step(colorRange.y, light));
    color = lerp(color, colorD, step(colorRange.z, light));
     
    return float4(color, 1.0);

}

float4 LookUpTablePS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 normal = normalize(input.Normal.xyz);
    
	// Calculate the diffuse light
    float NdotL = dot(normal, lightDirection);
    float diffuseLight = NdotL * 0.5 + 0.5;
    
    // Add an ambient factor
    float light = KAmbient + diffuseLight;
    
    // Get the final color using a look up table, using our light as 1D coordinate from 0 to 1
    float3 color = tex1D(lookUpTableSampler, light).rgb;
     
    return float4(color, 1.0);
}

technique Default
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};



technique LookUpTable
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL LookUpTablePS();
    }
};

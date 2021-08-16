#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 WorldViewProjection;

float Time;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL;
    float2 TextureCoordinates : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Normal : TEXCOORD0;
    float2 TextureCoordinates : TEXCOORD1;
};
// One color for each render target
struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Inverse : COLOR1;
    float4 Normal : COLOR2;
    float4 Animation : COLOR3;
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
    VertexShaderOutput output = (VertexShaderOutput) 0;
    //Animate position

    float y = input.Position.y;
    float z = input.Position.z;
    input.Position.y = y * cos(Time) - z * sin(Time);
    input.Position.z = z * cos(Time) + y * sin(Time);
    
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    output.Normal = mul(input.Normal, World);
    output.TextureCoordinates = input.TextureCoordinates;
	
    return output;
}

// Instead of just a float4, we output a struct with up to 4 float4 in our PS
// if transparency is not required, its possible to use the alpha value to 
// output more data

PixelShaderOutput MainPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput) 0;
    
    float3 color = tex2D(textureSampler, input.TextureCoordinates).rgb;
    
    output.Color = float4(color, 1);
    
    output.Inverse = float4(1 - color, 1);
    
    output.Normal = input.Normal;

    float r = color.r * abs(sin(Time));
    float g = color.g * abs(cos(Time));
    
    output.Animation = float4(r, g, color.b, 1);
    
    return output;
}

technique MRT
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
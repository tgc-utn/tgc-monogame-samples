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

float alphaValue = 1;
float3 lightPosition = float3(1000, 1000, 1000);
float time = 0;

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texDiffuseMap2;
sampler2D diffuseMap2 = sampler_state
{
    Texture = (texDiffuseMap2);
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texColorMap;
sampler2D colorMap = sampler_state
{
    Texture = (texColorMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

VS_OUTPUT vs_RenderTerrain(VS_INPUT input)
{
    VS_OUTPUT output;

    //Proyectar posicion
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    //Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

    //todo: que le pase el inv trasp. word
    float4x4 matInverseTransposeWorld = World;
    output.WorldPos = worldPosition.xyz;
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

    return output;
}

struct PS_INPUT
{
    float2 Texcoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

//Pixel Shader
float4 ps_RenderTerrain(PS_INPUT input) : COLOR0
{
    float3 N = normalize(input.WorldNormal);
    float3 L = normalize(lightPosition - input.WorldPos);
    float kd = saturate(0.4 + 0.7 * saturate(dot(N, L)));

    float3 c = tex2D(colorMap, input.Texcoord).rgb;
    float3 tex1 = tex2D(diffuseMap, input.Texcoord * 31).rgb;
    float3 tex2 = tex2D(diffuseMap2, input.Texcoord * 27).rgb;
    float3 clr = lerp(lerp(tex1, tex2, c.r), c, 0.3);
    return float4(clr * kd, 1);
}

technique RenderTerrain
{
    pass Pass_0
    {
        VertexShader = compile VS_SHADERMODEL vs_RenderTerrain();
        PixelShader = compile PS_SHADERMODEL ps_RenderTerrain();
    }
}

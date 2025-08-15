#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 world;
float4x4 view;
float4x4 projection;
float4x4 inverseTransposeWorld;
float KD, KS, shininess;
float3 color;

struct VSI
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0; 
    float2 TexCoord: TEXCOORD;
};

struct VSO
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Normal : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
};
struct PSO
{
    float4 color : COLOR0;
    float4 normal : COLOR1;
    float4 position : COLOR2;
    float4 material : COLOR3;
};

texture colorTexture;
sampler2D colorSampler = sampler_state
{
    Texture = (colorTexture);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

VSO VS(in VSI input)
{
    VSO output = (VSO) 0;
    float4 worldPosition = mul(input.Position, world);
    float4 viewPosition = mul(worldPosition, view);
    float4 screenPos = mul(viewPosition, projection);
    output.WorldPos = worldPosition;
    output.Position = screenPos;
    output.Normal = mul(float4(input.Normal, 1), inverseTransposeWorld);
    output.TexCoord = input.TexCoord;
    return output;
}

PSO PS(VSO input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;

    float3 texColor = tex2D(colorSampler, input.TexCoord).rgb;
    
    output.color = float4(texColor, 1);
    output.normal = float4(normal, 1);
    output.position = float4(input.WorldPos.xyz, 1);
    output.material = float4(KD,KS,shininess, 1);
    
    return output;
}

PSO ColorPS(VSO input)
{
    PSO output;
    float3 n = normalize(input.Normal.xyz);
  
    float3 normal = (n + 1.0) * 0.5;
    
    output.color = float4(color, 1);
    output.normal = float4(normal, 1);
    output.position = float4(input.WorldPos.xyz, 1);
    output.material = float4(0, KS, shininess, 1);
    
    return output;
}

technique plain_color
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL ColorPS();
    }
}

technique textured
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS();
        PixelShader = compile PS_SHADERMODEL PS();
    }
}
